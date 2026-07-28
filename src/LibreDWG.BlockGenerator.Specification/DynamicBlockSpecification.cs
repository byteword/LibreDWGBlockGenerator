using System.Text.Json.Serialization;

namespace LibreDWG.BlockGenerator.Specification;

public sealed class DynamicBlockDocument
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("document")]
    public DocumentSettings Document { get; set; } = new();

    [JsonPropertyName("block")]
    public DynamicBlockDefinition Block { get; set; } = new();
}

public sealed class DocumentSettings
{
    [JsonPropertyName("dwgVersion")]
    public string DwgVersion { get; set; } = "r2004";

    [JsonPropertyName("units")]
    public string Units { get; set; } = "millimeters";
}

public sealed class DynamicBlockDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public decimal[] Origin { get; set; } = [0, 0, 0];

    [JsonPropertyName("geometry")]
    public List<BlockGeometry> Geometry { get; set; } = [];

    [JsonPropertyName("parameters")]
    public List<BlockParameter> Parameters { get; set; } = [];

    [JsonPropertyName("actions")]
    public List<BlockAction> Actions { get; set; } = [];
}

public sealed class BlockGeometry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("closed")]
    public bool Closed { get; set; }

    [JsonPropertyName("vertices")]
    public List<decimal[]> Vertices { get; set; } = [];
}

public sealed class BlockParameter
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("start")]
    public decimal[] Start { get; set; } = [];

    [JsonPropertyName("end")]
    public decimal[] End { get; set; } = [];

    [JsonPropertyName("minimum")]
    public decimal? Minimum { get; set; }

    [JsonPropertyName("maximum")]
    public decimal? Maximum { get; set; }
}

public sealed class BlockAction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("parameterId")]
    public string ParameterId { get; set; } = string.Empty;

    [JsonPropertyName("selection")]
    public List<string> Selection { get; set; } = [];

    [JsonPropertyName("stretchFrame")]
    public List<decimal[]> StretchFrame { get; set; } = [];
}

