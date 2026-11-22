using System.Text.Json;
using TheSingularityWorkshop.Physics.Geology;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: VolcanicTerrain2DExport <output.json>");
    return 2;
}

var revision = RequiredEnvironment("PHYSICS_SOURCE_REVISION");
var runId = RequiredEnvironment("PHYSICS_RUN_ID");
const int width = 24;
const int height = 18;
const double cellWidth = 10d;
const double cellHeight = 10d;

var initial = new double[width * height];
for (var y = 0; y < height; y++)
for (var x = 0; x < width; x++)
{
    var nx = (x - (width - 1) / 2d) / width;
    var ny = (y - (height - 1) / 2d) / height;
    initial[y * width + x] = 18d * Math.Sin(nx * Math.PI * 2d)
        + 12d * Math.Cos(ny * Math.PI * 2d)
        - 8d * (nx * nx + ny * ny);
}

var terrain = new VolcanicTerrain2D(width, height, cellWidth, cellHeight, initial);
var depositedVolume = 250_000d;
var maximumRise = terrain.DepositEruption(width / 2, height / 2, 60d, depositedVolume);
var metadata = new VolcanicTerrain2DExportMetadata
{
    DatasetId = $"physics-volcanic-terrain-2d-{revision[..12]}",
    Repository = "https://github.com/TrentBest/TheSingularityWorkshop.Physics",
    Revision = revision,
    ModelVersion = "1",
    RunId = runId,
    CoordinateFrameId = "physics-local-east-north-v1",
    OriginX = 0d,
    OriginY = 0d,
    PositiveXAxis = "east",
    PositiveYAxis = "north",
    VerticalDatum = "model-base-elevation",
    PositiveVerticalDirection = "up"
};

var envelope = VolcanicTerrain2DExport.Create(terrain, metadata);
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, VolcanicTerrain2DExport.Serialize(envelope));

using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
var root = document.RootElement;
var samples = root.GetProperty("elevations").EnumerateArray().Select(value => value.GetDouble()).ToArray();
if (root.GetProperty("schema").GetString() != VolcanicTerrain2DExport.Schema ||
    root.GetProperty("source").GetProperty("revision").GetString() != revision ||
    root.GetProperty("source").GetProperty("runId").GetString() != runId ||
    root.GetProperty("grid").GetProperty("width").GetInt32() != width ||
    root.GetProperty("grid").GetProperty("height").GetInt32() != height ||
    samples.Length != width * height ||
    samples.Any(value => !double.IsFinite(value)) ||
    !samples.SequenceEqual(terrain.SnapshotElevation()))
{
    throw new InvalidOperationException("Serialized terrain export failed provenance, shape, finiteness, or exact-sample validation.");
}

var finalVolume = samples.Zip(initial, (final, start) => (final - start) * cellWidth * cellHeight).Sum();
if (Math.Abs(finalVolume - depositedVolume) > 1e-7)
    throw new InvalidOperationException($"Deposited volume mismatch: expected {depositedVolume}, observed {finalVolume}.");

Console.WriteLine($"PASS: exported {width}x{height} actual VolcanicTerrain2D samples.");
Console.WriteLine($"Revision: {revision}; run: {runId}; deposited volume: {finalVolume:R} m^3; maximum rise: {maximumRise:R} m.");
Console.WriteLine($"Frame: local east/north metres; vertical datum: model-base-elevation (not georeferenced).");
Console.WriteLine($"Generated: {outputPath}");
return 0;

static string RequiredEnvironment(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    return !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new InvalidOperationException($"Required provenance environment variable {name} is missing.");
}
