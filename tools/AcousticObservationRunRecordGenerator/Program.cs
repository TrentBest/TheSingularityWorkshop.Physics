using System.Text.Json;
using TheSingularityWorkshop.Physics.Acoustics;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: AcousticObservationRunRecordGenerator <output.json> [--check]");
    return 2;
}

var source = new AcousticSource(
    "studio-source",
    new AcousticPosition3(0d, 0d, 0d),
    referenceLevelDbSpl: 80d,
    referenceDistanceMeters: 1d);
var listener = new AcousticListener("studio-listener", new AcousticPosition3(3d, 4d, 0d));
const double speedOfSoundMetersPerSecond = FreeFieldAcoustics.DefaultSpeedOfSoundMetersPerSecond;
var observation = FreeFieldAcoustics.Observe(source, listener, speedOfSoundMetersPerSecond);

var record = new
{
    schemaVersion = 1,
    modelId = "acoustics.free-field-point-source-observation",
    modelVersion = 1,
    status = "computed-from-physics-model",
    units = new
    {
        position = "m",
        propagationDelay = "s",
        receivedLevel = "dB SPL",
        direction = "unit vector"
    },
    definition = new
    {
        source = new
        {
            id = source.Id,
            positionMeters = Position(source.Position),
            referenceLevelDbSpl = source.ReferenceLevelDbSpl,
            referenceDistanceMeters = source.ReferenceDistanceMeters
        },
        listener = new
        {
            id = listener.Id,
            positionMeters = Position(listener.Position)
        },
        speedOfSoundMetersPerSecond
    },
    observation = new
    {
        sourceId = observation.SourceId,
        listenerId = observation.ListenerId,
        distanceMeters = observation.DistanceMeters,
        propagationDelaySeconds = observation.PropagationDelaySeconds,
        receivedLevelDbSpl = observation.ReceivedLevelDbSpl,
        directionFromListenerToSource = new
        {
            x = observation.DirectionX,
            y = observation.DirectionY,
            z = observation.DirectionZ
        },
        hasDirection = observation.HasDirection
    },
    limitations = new[]
    {
        "Analytical free-field estimate; not an audio waveform.",
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
        Console.Error.WriteLine("Acoustic run record differs from the current FreeFieldAcoustics output.");
        Console.Error.WriteLine("Fresh model record:");
        Console.Error.WriteLine(json);
        return 1;
    }
    Console.WriteLine("Acoustic run record matches current FreeFieldAcoustics output.");
    return 0;
}
File.WriteAllText(path, json);
Console.WriteLine($"Wrote acoustic observation run record: {path}");
return 0;

static object Position(AcousticPosition3 value) => new { x = value.X, y = value.Y, z = value.Z };
