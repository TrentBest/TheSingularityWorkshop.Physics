using System.Globalization;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

if (args.Length < 1)
{
    PrintUsage();
    return 2;
}

var outputPath = Path.GetFullPath(args[0]);
var check = false;
var deltaTime = 0.25d;
var stepCount = 8;
var vx = 10d;
var vy = 0d;
var vz = 12d;
var ax = 0d;
var ay = 0d;
var az = -9.81d;

foreach (var argument in args.Skip(1))
{
    if (argument == "--check")
    {
        if (check)
        {
            Console.Error.WriteLine("Option --check may only be specified once.");
            return 2;
        }

        check = true;
        continue;
    }

    var separator = argument.IndexOf('=');
    if (separator < 3)
    {
        Console.Error.WriteLine($"Expected --name=value, got: {argument}");
        PrintUsage();
        return 2;
    }

    var name = argument[..separator];
    var value = argument[(separator + 1)..];

    if (name == "--steps")
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out stepCount)
            || stepCount is < 1 or > 400)
        {
            Console.Error.WriteLine("--steps must be an integer from 1 through 400.");
            return 2;
        }

        continue;
    }

    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
        || !double.IsFinite(number))
    {
        Console.Error.WriteLine($"{name} must be a finite number using invariant (dot-decimal) notation.");
        return 2;
    }

    switch (name)
    {
        case "--delta-time" when number is >= 0.001d and <= 1d:
            deltaTime = number;
            break;
        case "--vx" when number is >= -1000d and <= 1000d:
            vx = number;
            break;
        case "--vy" when number is >= -1000d and <= 1000d:
            vy = number;
            break;
        case "--vz" when number is >= -1000d and <= 1000d:
            vz = number;
            break;
        case "--ax" when number is >= -100d and <= 100d:
            ax = number;
            break;
        case "--ay" when number is >= -100d and <= 100d:
            ay = number;
            break;
        case "--az" when number is >= -100d and <= 100d:
            az = number;
            break;
        case "--delta-time":
            Console.Error.WriteLine("--delta-time must be between 0.001 and 1 second.");
            return 2;
        case "--vx" or "--vy" or "--vz":
            Console.Error.WriteLine("Velocity components must be between -1000 and 1000 m/s.");
            return 2;
        case "--ax" or "--ay" or "--az":
            Console.Error.WriteLine("Acceleration components must be between -100 and 100 m/s^2.");
            return 2;
        default:
            Console.Error.WriteLine($"Unknown or invalid option: {argument}");
            PrintUsage();
            return 2;
    }
}

var definition = new ProjectileTrajectoryExperimentDefinition(
    InitialState: new ProjectileState(
        Position: new Vector3d(0d, 0d, 0d),
        Velocity: new Vector3d(vx, vy, vz),
        ElapsedTime: 0d),
    Acceleration: new Vector3d(ax, ay, az),
    DeltaTime: deltaTime,
    StepCount: stepCount,
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
        Console.Error.WriteLine("Run record is stale: regenerate it from the C# experiment with the same inputs.");
        return 1;
    }

    Console.WriteLine("Run record matches the C# experiment output for these inputs.");
    return 0;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, generated);
Console.WriteLine($"Wrote {result.Samples.Count} recorded states to {outputPath}");
return 0;

static void PrintUsage()
{
    Console.Error.WriteLine(
        "Usage: ProjectileRunRecordGenerator <output.json> [--check] " +
        "[--delta-time=0.001..1] [--steps=1..400] " +
        "[--vx=-1000..1000] [--vy=-1000..1000] [--vz=-1000..1000] " +
        "[--ax=-100..100] [--ay=-100..100] [--az=-100..100]");
}
