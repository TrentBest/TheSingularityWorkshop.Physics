using System.Text.Json;
using TheSingularityWorkshop.Physics.Experiments;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: FallingBodyDragRunGenerator <output.json>");
    return 2;
}

var definition = new FallingBodyDragDefinition(
    InitialHeight: 100d,
    InitialVelocity: 0d,
    Mass: 1d,
    Gravity: 9.81d,
    AirDensity: 1.225d,
    DragCoefficient: 0.47d,
    ReferenceArea: 0.05d,
    DeltaTime: 0.01d,
    StepCount: 100);

var vacuum = FallingBodyDragExperiment.Run(definition, includeDrag: false);
var air = FallingBodyDragExperiment.Run(definition, includeDrag: true);
if (vacuum.Samples.Count != 101 || air.Samples.Count != 101 ||
    air.FinalState.Height <= vacuum.FinalState.Height ||
    air.FinalState.Velocity <= vacuum.FinalState.Velocity)
{
    Console.Error.WriteLine("Falling-body drag acceptance checks failed.");
    return 1;
}

var record = new
{
    schemaVersion = 1,
    modelId = "aerodynamics.falling-body-quadratic-drag",
    modelVersion = "1",
    status = "computed",
    units = new { height = "m", time = "s", velocity = "m/s", acceleration = "m/s^2", mass = "kg", airDensity = "kg/m^3", referenceArea = "m^2" },
    definition,
    integration = new { method = "semi-implicit-euler", groundContact = "not-resolved", airVelocity = "zero" },
    validation = new
    {
        sampleCount = air.Samples.Count,
        elapsedTimeSeconds = air.FinalState.ElapsedTime,
        vacuumFinalHeightMetres = vacuum.FinalState.Height,
        dragFinalHeightMetres = air.FinalState.Height,
        vacuumFinalVelocityMetresPerSecond = vacuum.FinalState.Velocity,
        dragFinalVelocityMetresPerSecond = air.FinalState.Velocity,
        dragReducesDownwardSpeed = air.FinalState.Velocity > vacuum.FinalState.Velocity,
        dragReducesDistanceFallen = air.FinalState.Height > vacuum.FinalState.Height
    },
    vacuumSamples = vacuum.Samples,
    dragSamples = air.Samples,
    limitations = new[]
    {
        "Still air; no wind field.",
        "Constant drag coefficient and reference area.",
        "One-dimensional vertical motion.",
        "Semi-implicit Euler integration; outcomes depend on declared timestep.",
        "Ground contact, lift, body rotation, and changing atmospheric density are not modeled.",
        "Computed numerical experiment, not empirical validation."
    }
};

var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, json + Environment.NewLine);
Console.WriteLine($"Wrote {air.Samples.Count} actual states per case to {outputPath}");
Console.WriteLine($"At {air.FinalState.ElapsedTime:F2} s: vacuum height {vacuum.FinalState.Height:F3} m, drag height {air.FinalState.Height:F3} m.");
return 0;
