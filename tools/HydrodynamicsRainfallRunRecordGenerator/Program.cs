using System.Text.Json;
using TheSingularityWorkshop.Physics.Hydrodynamics;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: HydrodynamicsRainfallRunRecordGenerator <output.json> [--check]");
    return 2;
}

const int width = 12;
const int height = 8;
const double cellWidthMeters = 1d;
const double cellHeightMeters = 1d;
const double timeStepSeconds = 0.05d;
const int stepCount = 1200;
const double rainfallRateMetersPerSecond = 0.00002d;
const double gravityMetersPerSecondSquared = 9.80665d;

var bedElevationMeters = new double[width * height];
var initialDepthMeters = new double[width * height];
var rainfallRates = new double[width * height];
for (var y = 3; y <= 4; y++)
for (var x = 4; x <= 7; x++)
    rainfallRates[y * width + x] = rainfallRateMetersPerSecond;

var basin = new BathymetricShallowWaterBasin2D(
    width,
    height,
    cellWidthMeters,
    cellHeightMeters,
    timeStepSeconds,
    initialDepthMeters,
    bedElevationMeters,
    gravityMetersPerSecondSquared);
var rainfall = new RainfallRateField2D(width, height, rainfallRates);

var samples = new List<object>();
samples.Add(Capture(0, basin));
for (var step = 1; step <= stepCount; step++)
{
    basin.ApplyRainfall(rainfall, timeStepSeconds);
    basin.Advance();

    if (step % 300 == 0)
        samples.Add(Capture(step, basin));
}

var durationSeconds = stepCount * timeStepSeconds;
var expectedRainVolumeCubicMeters = rainfallRates.Sum() * durationSeconds * cellWidthMeters * cellHeightMeters;
var finalVolumeCubicMeters = basin.WaterVolume;
var volumeResidualCubicMeters = expectedRainVolumeCubicMeters - finalVolumeCubicMeters;
var finalDepthMeters = basin.SnapshotDepth();
var minimumDepthMeters = finalDepthMeters.Min();
var maximumDepthMeters = finalDepthMeters.Max();
var sourceRevision = Environment.GetEnvironmentVariable("PHYSICS_SOURCE_REVISION") ?? "working-tree";

var record = new
{
    schemaVersion = 1,
    modelId = "hydrodynamics.bathymetric-rainfall-accumulation-2d",
    modelVersion = 1,
    status = "computed-from-physics-model",
    units = new
    {
        length = "m",
        time = "s",
        rainfallRate = "m water equivalent / s",
        depth = "m",
        velocity = "m / s",
        volume = "m^3",
        gravity = "m / s^2"
    },
    definition = new
    {
        grid = new
        {
            width,
            height,
            cellWidthMeters,
            cellHeightMeters,
            layout = "row-major",
            cellAreaSquareMeters = cellWidthMeters * cellHeightMeters
        },
        initialDepthMeters,
        bedElevationMeters,
        rainfall = new
        {
            sourceId = "localized-rainfall-patch",
            ratesMetersPerSecond = rainfallRates,
            durationSeconds,
            applicationOrder = "ApplyRainfall(timeStepSeconds), then Advance(), repeated once per solver step"
        },
        timeStepSeconds,
        stepCount,
        gravityMetersPerSecondSquared,
        boundaryCondition = "impermeable closed walls"
    },
    result = new
    {
        simulationTimeSeconds = basin.SimulationTime,
        completedSteps = basin.StepCount,
        initialWaterVolumeCubicMeters = 0d,
        expectedRainVolumeCubicMeters,
        finalWaterVolumeCubicMeters = finalVolumeCubicMeters,
        volumeResidualCubicMeters,
        minimumDepthMeters,
        maximumDepthMeters,
        finalDepthMeters
    },
    samples,
    validation = new
    {
        allDepthsFiniteAndNonNegative = finalDepthMeters.All(value => double.IsFinite(value) && value >= 0d),
        completedExpectedStepCount = basin.StepCount == stepCount,
        elapsedTimeMatchesRequestedDuration = Math.Abs(basin.SimulationTime - durationSeconds) <= 1e-12,
        volumeBalanceToleranceCubicMeters = 1e-10,
        volumeBalanceWithinTolerance = Math.Abs(volumeResidualCubicMeters) <= 1e-10
    },
    provenance = new
    {
        repository = "TrentBest/TheSingularityWorkshop.Physics",
        sourceRevision,
        generator = "tools/HydrodynamicsRainfallRunRecordGenerator",
        deterministicScenario = true
    },
    limitations = new[]
    {
        "This is a depth-averaged 2D shallow-water model on a fixed, flat bed; it is not a 3D fluid simulation.",
        "Rainfall adds water-equivalent depth without injecting horizontal momentum.",
        "Infiltration, evaporation, drainage, runoff routing, and water loss are not modeled.",
        "The basin has impermeable closed walls; there is no external inflow or outflow.",
        "Individual raindrops, splashes, fountains, spray, and optical refraction are not modeled."
    }
};

var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
var outputPath = Path.GetFullPath(args[0]);
var check = args.Length == 2;
if (check)
{
    if (!File.Exists(outputPath))
    {
        Console.Error.WriteLine($"Run record is missing: {outputPath}");
        return 1;
    }

    if (!string.Equals(File.ReadAllText(outputPath), json, StringComparison.Ordinal))
    {
        Console.Error.WriteLine("Run record is stale: regenerate it from the current Physics model and source revision.");
        return 1;
    }

    Console.WriteLine("Hydrodynamics rainfall run record matches the current Physics model output.");
    return 0;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, json);
Console.WriteLine($"Wrote {samples.Count} recorded basin states to {outputPath}");
Console.WriteLine($"Water balance residual: {volumeResidualCubicMeters:R} m^3");
return 0;

static object Capture(int step, BathymetricShallowWaterBasin2D basin) => new
{
    step,
    simulationTimeSeconds = basin.SimulationTime,
    waterVolumeCubicMeters = basin.WaterVolume,
    depthMeters = basin.SnapshotDepth()
};
