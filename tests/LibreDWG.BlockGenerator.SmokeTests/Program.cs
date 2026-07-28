using LibreDWG.BlockGenerator.Specification;

var valid = new DynamicBlockDocument
{
    Block = new DynamicBlockDefinition
    {
        Name = "RECTANGLE",
        Geometry =
        [
            new BlockGeometry
            {
                Id = "outline",
                Type = "polyline2d",
                Closed = true,
                Vertices = [[0, 0], [1000, 0], [1000, 500], [0, 500]]
            }
        ],
        Parameters =
        [
            new BlockParameter
            {
                Id = "width",
                Type = "linear",
                Name = "Width",
                Start = [0, 0],
                End = [1000, 0],
                Minimum = 100,
                Maximum = 5000
            }
        ],
        Actions =
        [
            new BlockAction
            {
                Id = "stretch-width",
                Type = "stretch",
                ParameterId = "width",
                Selection = ["outline"],
                StretchFrame = [[500, -100], [1100, 600]]
            }
        ]
    }
};

Assert(SpecificationValidator.Validate(valid).IsValid, "Valid linear stretch specification was rejected.");

valid.Block.Actions[0].ParameterId = "missing";
Assert(!SpecificationValidator.Validate(valid).IsValid, "Unknown parameter reference was accepted.");

Console.WriteLine("Smoke tests passed.");
return;

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

