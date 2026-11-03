using System.Text.Json;
using TheSingularityWorkshop.Physics.Spatial;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: ShapesAndSpaceRunRecordGenerator <output.json>");
    return 2;
}

// This is the lesson's declared construction. All geometric results below come
// from the Physics domain APIs; the generator contains no second geometry solver.
var vertices = new[]
{
    new Vector2d(0, 0),
    new Vector2d(4, 0),
    new Vector2d(4, 3),
    new Vector2d(2, 4),
    new Vector2d(0, 3)
};
var pointCases = new[]
{
    (Id: "inside", Point: new Vector2d(2, 2)),
    (Id: "boundary", Point: new Vector2d(2, 0)),
    (Id: "outside", Point: new Vector2d(5, 2))
};
string Relation(PointPolygonRelation2d relation) => relation switch
{
    PointPolygonRelation2d.Inside => "inside",
    PointPolygonRelation2d.Boundary => "boundary",
    PointPolygonRelation2d.Outside => "outside",
    _ => throw new ArgumentOutOfRangeException(nameof(relation))
};

double signedArea = Geometry2dPolygon.SignedArea(vertices);
var triangles = Geometry2dPolygon.Triangulate(vertices)
    .Select(triangle => new[] { triangle.AIndex, triangle.BIndex, triangle.CIndex })
    .ToArray();
var record = new
{
    modelId = "geometry.polygon-construction-2d",
    modelVersion = "1",
    schemaVersion = 1,
    status = "computed-from-geometry-primitives",
    units = new { coordinates = "coordinate-unit", edgeLength = "coordinate-unit", signedArea = "coordinate-unit^2" },
    definition = new
    {
        coordinateFrame = "lesson-plane",
        vertices = vertices.Select((point, index) => new
        {
            id = ((char)('A' + index)).ToString(),
            x = point.X,
            y = point.Y
        }).ToArray(),
        boundaryOrder = "A-B-C-D-E-A"
    },
    result = new
    {
        vertexCount = vertices.Length,
        edgeCount = vertices.Length,
        isSimple = Geometry2dPolygon.IsSimple(vertices),
        signedArea,
        winding = signedArea > 0 ? "counter-clockwise" : signedArea < 0 ? "clockwise" : "degenerate",
        pointClassifications = pointCases.Select(item => new
        {
            id = item.Id,
            x = item.Point.X,
            y = item.Point.Y,
            relation = Relation(Geometry2dPolygon.LocatePoint(vertices, item.Point))
        }).ToArray(),
        triangles
    },
    provenance = new
    {
        sourceTypes = new[] { "tested-geometry-primitives", "explicit-teaching-construction" },
        physicalObservation = false,
        simulation = false
    },
    limitations = new[]
    {
        "Coordinates are abstract and have no declared physical unit.",
        "This is a deterministic geometric construction, not a physical observation or simulation.",
        "The polygon is intentionally simple and convex; this lesson does not teach all polygon degeneracies.",
        "Lengths are Euclidean distances rounded for display; exact-predicate guarantees do not make measured coordinates exact."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, JsonSerializer.Serialize(record, options) + Environment.NewLine);
Console.WriteLine($"Wrote geometry primitive results to {outputPath}");
return 0;
