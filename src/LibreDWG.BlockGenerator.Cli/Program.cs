using LibreDWG.BlockGenerator.Specification;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
    {
        PrintHelp();
        return 0;
    }

    if (args[0] is "--version" or "version")
    {
        Console.WriteLine("libredwg-block-generator 0.1.0");
        return 0;
    }

    var input = ReadOption(args, "--input");
    if (args[0] == "validate")
        return Validate(input);
    if (args[0] == "generate")
        return Generate(input, ReadOption(args, "--output"));

    Console.Error.WriteLine($"Unknown command: {args[0]}");
    PrintHelp();
    return 2;
}

static int Validate(string? input)
{
    if (input is null)
    {
        Console.Error.WriteLine("Missing required option: --input");
        return 2;
    }

    var result = SpecificationValidator.ReadAndValidate(input);
    if (result.IsValid)
    {
        Console.WriteLine("Specification is valid.");
        return 0;
    }

    foreach (var error in result.Errors)
        Console.Error.WriteLine($"{error.Path}: {error.Message}");
    return 2;
}

static int Generate(string? input, string? output)
{
    if (input is null || output is null)
    {
        Console.Error.WriteLine("generate requires --input and --output.");
        return 2;
    }

    var validation = SpecificationValidator.ReadAndValidate(input);
    if (!validation.IsValid)
    {
        foreach (var error in validation.Errors)
            Console.Error.WriteLine($"{error.Path}: {error.Message}");
        return 2;
    }

    var document = validation.Document!;
    if (document.Document.DwgVersion != "r2000"
        || document.Block.Parameters.Count != 0
        || document.Block.Actions.Count != 0
        || document.Block.Geometry.Count != 1)
    {
        Console.Error.WriteLine(
            "This backend milestone supports one static polyline2d block in r2000 only.");
        return 3;
    }

    var fullOutput = Path.GetFullPath(output);
    if (File.Exists(fullOutput) || !Directory.Exists(Path.GetDirectoryName(fullOutput)))
    {
        Console.Error.WriteLine("Output must be a new file in an existing directory.");
        return 7;
    }

    try
    {
        var geometry = document.Block.Geometry[0];
        var coordinates = geometry.Vertices.SelectMany(point => point).Select(decimal.ToDouble).ToArray();
        var exitCode = NativeBackend.GenerateR2000PolylineBlock(
            fullOutput,
            document.Block.Name,
            document.Block.Origin.Select(decimal.ToDouble).ToArray(),
            coordinates,
            geometry.Vertices.Count,
            geometry.Closed);
        if (exitCode == 0)
            Console.WriteLine(fullOutput);
        return exitCode;
    }
    catch (DllNotFoundException exception)
    {
        Console.Error.WriteLine(exception.Message);
        return 3;
    }
}

static string? ReadOption(string[] args, string name)
{
    for (var index = 1; index < args.Length - 1; index++)
        if (string.Equals(args[index], name, StringComparison.Ordinal))
            return args[index + 1];
    return null;
}

static void PrintHelp()
{
    Console.WriteLine("LibreDWG Block Generator");
    Console.WriteLine("Usage:");
    Console.WriteLine("  libredwg-block-generator validate --input <spec.json>");
    Console.WriteLine("  libredwg-block-generator generate --input <spec.json> --output <file.dwg>");
    Console.WriteLine("  libredwg-block-generator --version");
}
