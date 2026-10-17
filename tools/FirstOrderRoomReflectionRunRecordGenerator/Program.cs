using System.Text.Json;
using TheSingularityWorkshop.Physics.Acoustics;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: FirstOrderRoomReflectionRunRecordGenerator <output.json> [--check]");
    return 2;
}

var room = new AcousticRoomBox(
    "studio-room",
    new AcousticPosition3(0d, 0d, 0d),
    new AcousticPosition3(10d, 10d, 10d),
    xMinReflectionCoefficient: 0d,
    xMaxReflectionCoefficient: 0.5d,
    yMinReflectionCoefficient: 0d,
    yMaxReflectionCoefficient: 0d,
    zMinReflectionCoefficient: 0d,
    zMaxReflectionCoefficient: 0d);
var source = new AcousticSource(
    "studio-source",
    new AcousticPosition3(2d, 5d, 5d),
    referenceLevelDbSpl: 80d,
    referenceDistanceMeters: 1d);
var listener = new AcousticListener("studio-listener", new AcousticPosition3(8d, 7d, 5d));
const double speedOfSoundMetersPerSecond = FreeFieldAcoustics.DefaultSpeedOfSoundMetersPerSecond;
var direct = FreeFieldAcoustics.Observe(source, listener, speedOfSoundMetersPerSecond);
var reflections = FirstOrderRoomReflections.Observe(source, listener, room, speedOfSoundMetersPerSecond)
    .Select(path => new
    {
        surfaceId = path.SurfaceId,
        reflectionPointMeters = new
        {
            x = path.ReflectionPoint.X,
            y = path.ReflectionPoint.Y,
            z = path.ReflectionPoint.Z
        },
        pathLengthMeters = path.PathLengthMeters,
        propagationDelaySeconds = path.PropagationDelaySeconds,
        reflectionCoefficient = path.ReflectionCoefficient,
        receivedLevelDbSpl = path.ReceivedLevelDbSpl
    })
    .ToArray();

var record = new
{
    schemaVersion = 1,
    modelId = "acoustics.first-order-room-reflection",
    modelVersion = 1,
    status = "computed-from-physics-model",
    room = new
    {
        id = room.Id,
        minimumMeters = new { x = room.Minimum.X, y = room.Minimum.Y, z = room.Minimum.Z },
        maximumMeters = new { x = room.Maximum.X, y = room.Maximum.Y, z = room.Maximum.Z },
        reflectionCoefficientsPressureAmplitude = new
        {
            xMin = room.XMinReflectionCoefficient,
            xMax = room.XMaxReflectionCoefficient,
            yMin = room.YMinReflectionCoefficient,
            yMax = room.YMaxReflectionCoefficient,
            zMin = room.ZMinReflectionCoefficient,
            zMax = room.ZMaxReflectionCoefficient
        }
    },
    source = new
    {
        id = source.Id,
        positionMeters = new { x = source.Position.X, y = source.Position.Y, z = source.Position.Z },
        referenceLevelDbSpl = source.ReferenceLevelDbSpl,
        referenceDistanceMeters = source.ReferenceDistanceMeters
    },
    listener = new
    {
        id = listener.Id,
        positionMeters = new { x = listener.Position.X, y = listener.Position.Y, z = listener.Position.Z }
    },
    speedOfSoundMetersPerSecond,
    directObservation = new
    {
        distanceMeters = direct.DistanceMeters,
        propagationDelaySeconds = direct.PropagationDelaySeconds,
        receivedLevelDbSpl = direct.ReceivedLevelDbSpl
    },
    reflections,
    limitations = new[]
    {
        "First-order specular path metadata only; not a pressure waveform or audible room response.",
        "Axis-aligned rectangular room; only the maximum-X wall reflects in this controlled scenario.",
        "Frequency-independent pressure-amplitude coefficient; no phase, interference, scattering, diffraction, transmission, occlusion, or multiple reflections.",
        "Direct and reflected levels are separate per-path estimates and are not combined into a total listener level."
    }
};
var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
var path = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
if (File.Exists(path) && args.Length == 2 && args[1] == "--check")
{
    if (File.ReadAllText(path) != json)
    {
        Console.Error.WriteLine("First-order room-reflection record differs from current Physics model output.");
        Console.Error.WriteLine(json);
        return 1;
    }
    Console.WriteLine("First-order room-reflection record matches current Physics model output.");
    return 0;
}
File.WriteAllText(path, json);
Console.WriteLine($"Wrote first-order room-reflection run record: {path}");
return 0;
