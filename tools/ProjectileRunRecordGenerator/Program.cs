using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("Usage: ProjectileRunRecordGenerator <output.json> [--check]");
    return 2;
}

var outputPath = Path.GetFullPath(args[0]);
var check = args.Length == 2 && args[1] == "--check";
if (args.Length == 2 && !check)
{
    Console.Error.WriteLine($"Unknown option: {args[1]}");
    return 2;
}

var definition = new ProjectileTrajectoryExperimentDefinition(
    InitialState: new ProjectileState(
        Position: new Vector3d(0d, 0d, 0d),
        Velocity: new Vector3d(10d, 0d, 12d),
        ElapsedTime: 0d),
    Acceleration: new Vector3d(0d, 0d, -9.81d),
    DeltaTime: 0.25d,
    StepCount: 8,
    CoordinateFrame: "world, Z-up");

var result = ProjectileTrajectoryExperiment.Run(definition);
var generated = ProjectileTrajectoryRunRecord.FromResult(result).ToJson() + Environment.NewLine;

if (check)
{
    if (!File.Exists(outputPath))
    {
        Console.Error.WriteLine($"Run record is missing: {outputPath}");
        return 1;
    }

    var existing = File.ReadAllText(outputPath);
    if (!string.Equals(existing, generated, StringComparison.Ordinal))
    {
        Console.Error.WriteLine("Run record is stale: regenerate it from the C# experiment.");
        return 1;
    }

    Console.WriteLine("Run record matches the C# experiment output.");
    return 0;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, generated);
Console.WriteLine($"Wrote {result.Samples.Count} recorded states to {outputPath}");
return 0;
