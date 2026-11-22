using System.Text.Json;
using TheSingularityWorkshop.Physics.Geology;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: TerrainHeightFieldRunRecordGenerator <output.json> [--check]");
    return 2;
}

var outputPath = Path.GetFullPath(args[0]);
var check = args.Length == 2;
var baseTerrain = TerrainHeightField2D.CreateBowl(
    width: 17,
    height: 17,
    cellWidth: 1d,
    cellHeight: 1d,
    originX: -8d,
    originY: -8d,
    coordinateFrameId: "local-east-north",
    verticalDatum: "local-zero",
    linearUnit: "m",
    positiveVerticalDirection: "up",
    rimElevation: 20d,
    depth: 8d,
    centerX: 0d,
    centerY: 0d,
    radius: 8d);
var editedTerrain = baseTerrain.ApplyRadialBrush(
    centerX: -2d,
    centerY: 2d,
    radius: 3d,
    delta: 2d);

var record = new
{
    modelId = "geology.terrain-height-field-2d",
    modelVersion = "1.0.0",
    schemaVersion = 1,
    units = new { horizontal = "m", elevation = "m" },
    definition = new
    {
        width = baseTerrain.Width,
        height = baseTerrain.Height,
        cellWidth = baseTerrain.CellWidth,
        cellHeight = baseTerrain.CellHeight,
        origin = new { x = baseTerrain.OriginX, y = baseTerrain.OriginY },
        coordinateFrameId = baseTerrain.CoordinateFrameId,
        verticalDatum = baseTerrain.VerticalDatum,
        positiveVerticalDirection = baseTerrain.PositiveVerticalDirection,
        baseOperation = "parabolic-bowl",
        bowl = new { rimElevation = 20d, depth = 8d, centerX = 0d, centerY = 0d, radius = 8d },
        editOperation = "smooth-radial-elevation-brush",
        brush = new { centerX = -2d, centerY = 2d, radius = 3d, delta = 2d }
    },
    samples = new
    {
        baseElevations = baseTerrain.SnapshotElevations(),
        editedElevations = editedTerrain.SnapshotElevations()
    },
    provenance = new
    {
        sourceFile = "Physics/Geology/TerrainHeightField2D.cs",
        testFile = "tests/TheSingularityWorkshop.Physics.Tests/TerrainHeightField2DTests.cs",
        generatorFile = "tools/TerrainHeightFieldRunRecordGenerator/Program.cs"
    },
    limitations = new[]
    {
        "A 2.5D height field cannot represent caves, overhangs, vertical faces, or multiple surface layers.",
        "The bowl and radial brush are geometric authoring operations, not a geological formation or erosion simulation."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
var generated = JsonSerializer.Serialize(record, options) + Environment.NewLine;

if (check)
{
    if (!File.Exists(outputPath))
    {
        Console.Error.WriteLine($"Terrain run record is missing: {outputPath}");
        return 1;
    }

    using var expected = JsonDocument.Parse(File.ReadAllText(outputPath));
    using var actual = JsonDocument.Parse(generated);
    if (!JsonElement.DeepEquals(expected.RootElement, actual.RootElement))
    {
        Console.Error.WriteLine("Terrain run record is stale: regenerate it from TerrainHeightField2D.");
        return 1;
    }

    Console.WriteLine("Terrain run record matches TerrainHeightField2D output.");
    return 0;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, generated);
Console.WriteLine($"Wrote {baseTerrain.Width * baseTerrain.Height} base and edited terrain samples to {outputPath}");
return 0;
