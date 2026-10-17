namespace TheSingularityWorkshop.Physics.Acoustics;

/// <summary>One geometrically valid, first-order specular reflection path through a room boundary.</summary>
/// <remarks>
/// The level is a per-path estimate with one frequency-independent pressure reflection coefficient.
/// It is not combined with the direct path or other reflections; phase, interference, spectrum, and
/// waveform synthesis are outside this model.
/// </remarks>
public readonly record struct AcousticReflectionObservation(
    string SourceId,
    string ListenerId,
    string RoomId,
    string SurfaceId,
    AcousticPosition3 ReflectionPoint,
    double PathLengthMeters,
    double PropagationDelaySeconds,
    double ReflectionCoefficient,
    double ReceivedLevelDbSpl);

/// <summary>
/// Computes direct geometric path candidates for the six finite walls of an axis-aligned room.
/// </summary>
/// <remarks>
/// The image-source construction returns only first-order specular paths whose wall intersection lies
/// strictly inside the rectangular face. Source and listener must be strictly inside the room. This
/// model does not include diffraction, transmission, frequency-dependent absorption, scattering,
/// multiple reflections, phase interference, or audible waveform generation.
/// </remarks>
public static class FirstOrderRoomReflections
{
    /// <summary>Computes valid first-order reflected-path metadata in a stable wall order.</summary>
    public static IReadOnlyList<AcousticReflectionObservation> Observe(
        AcousticSource source,
        AcousticListener listener,
        AcousticRoomBox room,
        double speedOfSoundMetersPerSecond = FreeFieldAcoustics.DefaultSpeedOfSoundMetersPerSecond)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(listener);
        ArgumentNullException.ThrowIfNull(room);
        if (!double.IsFinite(speedOfSoundMetersPerSecond) || speedOfSoundMetersPerSecond <= 0d)
            throw new ArgumentOutOfRangeException(nameof(speedOfSoundMetersPerSecond), "Sound speed must be finite and greater than zero.");
        if (!room.ContainsInterior(source.Position))
            throw new ArgumentOutOfRangeException(nameof(source), "The source must be strictly inside the room bounds.");
        if (!room.ContainsInterior(listener.Position))
            throw new ArgumentOutOfRangeException(nameof(listener), "The listener must be strictly inside the room bounds.");

        var results = new List<AcousticReflectionObservation>(6);
        AddReflection("x-min", 0, room.Minimum.X, room.XMinReflectionCoefficient);
        AddReflection("x-max", 0, room.Maximum.X, room.XMaxReflectionCoefficient);
        AddReflection("y-min", 1, room.Minimum.Y, room.YMinReflectionCoefficient);
        AddReflection("y-max", 1, room.Maximum.Y, room.YMaxReflectionCoefficient);
        AddReflection("z-min", 2, room.Minimum.Z, room.ZMinReflectionCoefficient);
        AddReflection("z-max", 2, room.Maximum.Z, room.ZMaxReflectionCoefficient);
        return results.AsReadOnly();

        void AddReflection(string surfaceId, int axis, double wallCoordinate, double coefficient)
        {
            if (coefficient <= 0d)
                return;

            var sourcePoint = source.Position;
            var listenerPoint = listener.Position;
            var sourceAxis = Coordinate(sourcePoint, axis);
            var listenerAxis = Coordinate(listenerPoint, axis);
            var imageAxis = 2d * wallCoordinate - sourceAxis;
            var denominator = imageAxis - listenerAxis;
            if (denominator == 0d)
                return;

            var t = (wallCoordinate - listenerAxis) / denominator;
            if (!(t > 0d && t < 1d))
                return;

            var hit = new AcousticPosition3(
                listenerPoint.X + t * (ImageCoordinate(sourcePoint, axis, imageAxis, 0) - listenerPoint.X),
                listenerPoint.Y + t * (ImageCoordinate(sourcePoint, axis, imageAxis, 1) - listenerPoint.Y),
                listenerPoint.Z + t * (ImageCoordinate(sourcePoint, axis, imageAxis, 2) - listenerPoint.Z));

            if (!InsideFaceInterior(hit, room, axis))
                return;

            var firstLeg = Distance(sourcePoint, hit);
            var secondLeg = Distance(hit, listenerPoint);
            var pathLength = firstLeg + secondLeg;
            if (!double.IsFinite(pathLength) || pathLength <= 0d)
                throw new ArithmeticException("Reflected path length is outside the representable numeric range.");

            var levelDistance = Math.Max(pathLength, source.ReferenceDistanceMeters);
            var receivedLevel = source.ReferenceLevelDbSpl -
                20d * Math.Log10(levelDistance / source.ReferenceDistanceMeters) +
                20d * Math.Log10(coefficient);
            if (!double.IsFinite(receivedLevel))
                throw new ArithmeticException("Reflected path level is outside the representable numeric range.");

            results.Add(new AcousticReflectionObservation(
                source.Id,
                listener.Id,
                room.Id,
                surfaceId,
                hit,
                pathLength,
                pathLength / speedOfSoundMetersPerSecond,
                coefficient,
                receivedLevel));
        }
    }

    private static double Coordinate(AcousticPosition3 point, int axis) => axis switch
    {
        0 => point.X,
        1 => point.Y,
        _ => point.Z
    };

    private static double ImageCoordinate(AcousticPosition3 source, int reflectedAxis, double imageAxis, int axis) =>
        axis == reflectedAxis ? imageAxis : Coordinate(source, axis);

    private static bool InsideFaceInterior(AcousticPosition3 point, AcousticRoomBox room, int wallAxis) =>
        wallAxis switch
        {
            0 => point.Y > room.Minimum.Y && point.Y < room.Maximum.Y &&
                 point.Z > room.Minimum.Z && point.Z < room.Maximum.Z,
            1 => point.X > room.Minimum.X && point.X < room.Maximum.X &&
                 point.Z > room.Minimum.Z && point.Z < room.Maximum.Z,
            _ => point.X > room.Minimum.X && point.X < room.Maximum.X &&
                 point.Y > room.Minimum.Y && point.Y < room.Maximum.Y
        };

    private static double Distance(AcousticPosition3 a, AcousticPosition3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
