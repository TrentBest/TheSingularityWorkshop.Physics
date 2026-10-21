using System.Text.Json;
using TheSingularityWorkshop.Physics.Spatial;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: SeededPointSet2dExport <output.json>");
    return 2;
}

const int count = 128;
const uint seed = 20261021;
const double minX = -10d;
const double minY = -10d;
const double maxX = 10d;
const double maxY = 10d;

var points = SeededPointSet2d.Generate(count, minX, minY, maxX, maxY, seed);
var runRecord = new
{
    modelId = "procedural.seeded-point-set-2d",
    modelVersion = "1",
    schemaVersion = 1,
    seed,
    bounds = new { minX, minY, maxX, maxY, convention = "minimum-inclusive, maximum-exclusive" },
    pointCount = points.Count,
    points = points.Select((point, index) => new { index, x = point.X, y = point.Y }).ToArray()
};

var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var json = JsonSerializer.Serialize(runRecord, new JsonSerializerOptions { WriteIndented = true });
await File.WriteAllTextAsync(outputPath, json);

using var parsed = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
var root = parsed.RootElement;
var writtenPoints = root.GetProperty("points").EnumerateArray().ToArray();
if (writtenPoints.Length != points.Count || root.GetProperty("pointCount").GetInt32() != points.Count)
    throw new InvalidOperationException("Serialized point count does not match the generated Physics result.");

for (var i = 0; i < points.Count; i++)
{
    var written = writtenPoints[i];
    if (written.GetProperty("index").GetInt32() != i ||
        written.GetProperty("x").GetDouble() != points[i].X ||
        written.GetProperty("y").GetDouble() != points[i].Y)
        throw new InvalidOperationException($"Serialized point {i} differs from the Physics result.");
}

Console.WriteLine($"PASS: serialized and re-read {points.Count} points from SeededPointSet2d.");
Console.WriteLine($"Model: procedural.seeded-point-set-2d v1; seed: {seed}; bounds: [{minX}, {maxX}) × [{minY}, {maxY}).");
Console.WriteLine($"Generated: {outputPath}");
return 0;
