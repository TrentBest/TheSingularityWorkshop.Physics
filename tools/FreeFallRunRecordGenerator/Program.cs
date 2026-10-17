using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: FreeFallRunRecordGenerator <output.json>");
    return 2;
}

// Teaching scenario: an apple starts 4.905 m above the ground at rest.
// Positive position is upward; gravity is constant at -9.81 m/s^2.
// In this ideal model, the apple reaches ground level at exactly t = 1 second.
// Air resistance, apple size, branches, and ground contact response are omitted.
var definition = new ConstantAccelerationExperimentDefinition(
    InitialState: new KinematicState(Position: 4.905d, Velocity: 0d, ElapsedTime: 0d),
    Acceleration: -9.81d,
    DeltaTime: 0.1d,
    StepCount: 10);

var result = ConstantAccelerationExperiment.Run(definition);
var record = ConstantAccelerationRunRecord.FromResult(result);
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, record.ToJson() + Environment.NewLine);

if (result.Samples.Count != 11 ||
    Math.Abs(result.FinalState.ElapsedTime - 1d) > 1e-12 ||
    Math.Abs(result.FinalState.Position) > 1e-12 ||
    Math.Abs(result.FinalState.Velocity + 9.81d) > 1e-12)
{
    Console.Error.WriteLine("Free-fall teaching scenario diverged from its analytical acceptance values.");
    return 1;
}

Console.WriteLine($"Wrote {result.Samples.Count} actual C# states to {outputPath}");
Console.WriteLine("Acceptance: 1.000 s, ground-level position 0 m, impact velocity -9.81 m/s (ideal model).");
return 0;
