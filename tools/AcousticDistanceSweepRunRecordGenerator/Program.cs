using System.Text.Json;
using TheSingularityWorkshop.Physics.Acoustics;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: AcousticDistanceSweepRunRecordGenerator <output.json> [--check]");
    return 2;
}

var source = new AcousticSource(
    "studio-source",
    new AcousticPosition3(0d, 0d, 0d),
    referenceLevelDbSpl: 80d,
    referenceDistanceMeters: 1d);
var distancesMeters = new[] { 1d, 2d, 5d, 10d };
const double speedOfSoundMetersPerSecond = FreeFieldAcoustics.DefaultSpeedOfSoundMetersPerSecond;
var observations = distancesMeters.Select(distance =>
{
    var listener = new AcousticListener(
        $"listener-{distance:0}m",
        new AcousticPosition3(distance, 0d, 0d));
    var result = FreeFieldAcoustics.Observe(source, listener, speedOfSoundMetersPerSecond);
    return new
    {
        listenerId = listener.Id,
        distanceMeters = result.DistanceMeters,
        propagationDelaySeconds = result.PropagationDelaySeconds,
        receivedLevelDbSpl = result.ReceivedLevelDbSpl
    };
}).ToArray();

var record = new
{
    schemaVersion = 1,
    modelId = "acoustics.free-field-distance-sweep",
    modelVersion = 1,
    status = "computed-from-physics-model",
    source = new
    {
        id = source.Id,
        positionMeters = new { x = source.Position.X, y = source.Position.Y, z = source.Position.Z },
        referenceLevelDbSpl = source.ReferenceLevelDbSpl,
        referenceDistanceMeters = source.ReferenceDistanceMeters
    },
    speedOfSoundMetersPerSecond,
    observations,
    limitations = new[]
    {
        "Analytical free-field estimates; not audio waveforms.",
        "No room reflections, absorption, occlusion, frequency response, source motion, or Doppler."
    }
};
var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
var path = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
if (File.Exists(path) && args.Length == 2 && args[1] == "--check")
{
    if (File.ReadAllText(path) != json)
    {
        Console.Error.WriteLine("Acoustic distance-sweep record differs from current FreeFieldAcoustics output.");
        Console.Error.WriteLine(json);
        return 1;
    }
    Console.WriteLine("Acoustic distance-sweep record matches current FreeFieldAcoustics output.");
    return 0;
}
File.WriteAllText(path, json);
Console.WriteLine($"Wrote acoustic distance-sweep run record: {path}");
return 0;
