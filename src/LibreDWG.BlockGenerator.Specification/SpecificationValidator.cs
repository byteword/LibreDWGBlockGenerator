using System.Text.Json;

namespace LibreDWG.BlockGenerator.Specification;

public static class SpecificationValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
    };

    public static ValidationResult ReadAndValidate(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return ValidationResult.Failure("input", "Input JSON file does not exist.");
        }

        try
        {
            var json = File.ReadAllText(path);
            var document = JsonSerializer.Deserialize<DynamicBlockDocument>(json, JsonOptions);
            return document is null
                ? ValidationResult.Failure("document", "The JSON document is empty.")
                : Validate(document);
        }
        catch (JsonException exception)
        {
            return ValidationResult.Failure("json", exception.Message);
        }
        catch (IOException exception)
        {
            return ValidationResult.Failure("input", exception.Message);
        }
    }

    public static ValidationResult Validate(DynamicBlockDocument document)
    {
        var errors = new List<ValidationError>();
        if (!string.Equals(document.SchemaVersion, "1.0", StringComparison.Ordinal))
            errors.Add(new("schemaVersion", "Only schema version 1.0 is supported."));
        if (document.Document.DwgVersion is not ("r2000" or "r2004"))
            errors.Add(new("document.dwgVersion", "Schema v1 supports r2000 and r2004 targets."));
        if (!string.Equals(document.Document.Units, "millimeters", StringComparison.Ordinal))
            errors.Add(new("document.units", "The initial implementation requires millimeters."));
        if (string.IsNullOrWhiteSpace(document.Block.Name))
            errors.Add(new("block.name", "Block name is required."));
        if (document.Block.Origin.Length != 3)
            errors.Add(new("block.origin", "Origin must contain exactly three coordinates."));
        if (document.Block.Geometry.Count == 0)
            errors.Add(new("block.geometry", "At least one geometry item is required."));

        ValidateUniqueIds(document, errors);
        ValidateGeometry(document, errors);
        ValidateParameters(document, errors);
        ValidateActions(document, errors);
        return new ValidationResult(document, errors);
    }

    private static void ValidateUniqueIds(DynamicBlockDocument document, List<ValidationError> errors)
    {
        var ids = document.Block.Geometry.Select(item => item.Id)
            .Concat(document.Block.Parameters.Select(item => item.Id))
            .Concat(document.Block.Actions.Select(item => item.Id))
            .ToList();
        if (ids.Any(string.IsNullOrWhiteSpace))
            errors.Add(new("block", "Every geometry, parameter, and action requires an id."));
        if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Count)
            errors.Add(new("block", "Geometry, parameter, and action ids must be unique."));
    }

    private static void ValidateGeometry(DynamicBlockDocument document, List<ValidationError> errors)
    {
        foreach (var geometry in document.Block.Geometry)
        {
            if (!string.Equals(geometry.Type, "polyline2d", StringComparison.Ordinal))
                errors.Add(new($"block.geometry[{geometry.Id}].type", "Only polyline2d is supported by schema v1."));
            if (geometry.Vertices.Count < 2 || geometry.Vertices.Any(point => point.Length != 2))
                errors.Add(new($"block.geometry[{geometry.Id}].vertices", "Polyline vertices must contain at least two XY points."));
        }
    }

    private static void ValidateParameters(DynamicBlockDocument document, List<ValidationError> errors)
    {
        foreach (var parameter in document.Block.Parameters)
        {
            if (!string.Equals(parameter.Type, "linear", StringComparison.Ordinal))
                errors.Add(new($"block.parameters[{parameter.Id}].type", "Only linear parameters are supported by schema v1."));
            if (string.IsNullOrWhiteSpace(parameter.Name))
                errors.Add(new($"block.parameters[{parameter.Id}].name", "Parameter name is required."));
            if (parameter.Start.Length != 2 || parameter.End.Length != 2)
                errors.Add(new($"block.parameters[{parameter.Id}]", "Linear parameter start and end must be XY points."));
            if (parameter.Minimum is not null && parameter.Maximum is not null
                && parameter.Minimum > parameter.Maximum)
                errors.Add(new($"block.parameters[{parameter.Id}]", "Minimum cannot exceed maximum."));
        }
    }

    private static void ValidateActions(DynamicBlockDocument document, List<ValidationError> errors)
    {
        var parameterIds = document.Block.Parameters.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var geometryIds = document.Block.Geometry.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var action in document.Block.Actions)
        {
            if (!string.Equals(action.Type, "stretch", StringComparison.Ordinal))
                errors.Add(new($"block.actions[{action.Id}].type", "Only stretch actions are supported by schema v1."));
            if (!parameterIds.Contains(action.ParameterId))
                errors.Add(new($"block.actions[{action.Id}].parameterId", "Action references an unknown parameter."));
            if (action.Selection.Count == 0 || action.Selection.Any(id => !geometryIds.Contains(id)))
                errors.Add(new($"block.actions[{action.Id}].selection", "Selection must reference existing geometry ids."));
            if (action.StretchFrame.Count != 2 || action.StretchFrame.Any(point => point.Length != 2))
                errors.Add(new($"block.actions[{action.Id}].stretchFrame", "Stretch frame must contain two XY corner points."));
        }
    }
}

public sealed record ValidationError(string Path, string Message);

public sealed record ValidationResult(DynamicBlockDocument? Document, IReadOnlyList<ValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Failure(string path, string message) =>
        new(null, [new ValidationError(path, message)]);
}
