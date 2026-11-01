using TheSingularityWorkshop.Physics.Experiments;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: InclinedPlaneRunRecordGenerator <output.json>");
    return 2;
}

// Reproducible baseline: 1 kg, 30-degree incline, 2 m vertical drop.
// The record is generated from the Physics model, not from a second implementation.
var definition = new InclinedPlaneExperimentDefinition(
    MassKilograms: 1d,
    AngleRadians: Math.PI / 6d,
    VerticalDropMeters: 2d,
    GravitationalAccelerationMetersPerSecondSquared: 9.80665d);

var result = InclinedPlaneExperiment.Run(definition);
var record = InclinedPlaneRunRecord.FromResult(result);
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, record.ToJson() + Environment.NewLine);

Console.WriteLine($"Wrote Physics model output to {outputPath}");
Console.WriteLine($"Path: {result.Outcome.PathLengthMeters:F12} m; " +
    $"acceleration: {result.Outcome.AccelerationMetersPerSecondSquared:F12} m/s^2; " +
    $"travel time: {result.Outcome.TravelTimeSeconds:F12} s; " +
    $"final speed: {result.Outcome.FinalSpeedMetersPerSecond:F12} m/s.");
return 0;
