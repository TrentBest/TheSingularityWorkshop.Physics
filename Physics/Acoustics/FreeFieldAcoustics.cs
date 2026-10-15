namespace TheSingularityWorkshop.Physics.Acoustics;

/// <summary>A position in three-dimensional world space, measured in metres.</summary>
public readonly record struct AcousticPosition3
{
    /// <summary>Creates a finite position in metres.</summary>
    public AcousticPosition3(double x, double y, double z)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x), "Position coordinates must be finite.");
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y), "Position coordinates must be finite.");
        if (!double.IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z), "Position coordinates must be finite.");
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>X coordinate in metres.</summary>
    public double X { get; }
    /// <summary>Y coordinate in metres.</summary>
    public double Y { get; }
    /// <summary>Z coordinate in metres.</summary>
    public double Z { get; }
}

/// <summary>A point sound source described by its reference sound-pressure level.</summary>
/// <remarks>
/// The reference level is dB SPL at <see cref="ReferenceDistanceMeters"/> in a free field.
/// This first model does not carry a waveform, spectrum, directivity, or source motion.
/// </remarks>
public sealed class AcousticSource
{
    /// <summary>Creates a point source with a reference sound-pressure level.</summary>
    public AcousticSource(
        string id,
        AcousticPosition3 position,
        double referenceLevelDbSpl,
        double referenceDistanceMeters = 1d)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A source identifier is required.", nameof(id));
        if (!double.IsFinite(referenceLevelDbSpl)) throw new ArgumentOutOfRangeException(nameof(referenceLevelDbSpl));
        if (!double.IsFinite(referenceDistanceMeters) || referenceDistanceMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(referenceDistanceMeters), "Reference distance must be finite and greater than zero.");

        Id = id;
        Position = position;
        ReferenceLevelDbSpl = referenceLevelDbSpl;
        ReferenceDistanceMeters = referenceDistanceMeters;
    }

    /// <summary>Stable identifier for this source.</summary>
    public string Id { get; }
    /// <summary>Source position in metres.</summary>
    public AcousticPosition3 Position { get; }
    /// <summary>Sound-pressure level in dB SPL at the reference distance.</summary>
    public double ReferenceLevelDbSpl { get; }
    /// <summary>Reference distance in metres; default is one metre.</summary>
    public double ReferenceDistanceMeters { get; }
}

/// <summary>A listener position used to evaluate sound arriving through a free field.</summary>
public sealed class AcousticListener
{
    /// <summary>Creates a listener with a stable identifier and position.</summary>
    public AcousticListener(string id, AcousticPosition3 position)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A listener identifier is required.", nameof(id));
        Id = id;
        Position = position;
    }

    /// <summary>Stable identifier for this listener.</summary>
    public string Id { get; }
    /// <summary>Listener position in metres.</summary>
    public AcousticPosition3 Position { get; }
}

/// <summary>A free-field estimate of when and at what level a source reaches a listener.</summary>
/// <remarks>
/// This is an observation of propagation metadata, not a delayed audio signal or a complete hearing simulation.
/// Direction is the normalized vector from the listener toward the source; it is zero when positions coincide.
/// </remarks>
public readonly record struct AcousticObservation(
    string SourceId,
    string ListenerId,
    double DistanceMeters,
    double PropagationDelaySeconds,
    double ReceivedLevelDbSpl,
    double DirectionX,
    double DirectionY,
    double DirectionZ)
{
    /// <summary>Whether the source direction is defined (the source and listener are not coincident).</summary>
    public bool HasDirection => DistanceMeters > 0d;
}

/// <summary>Deterministic point-source propagation estimates for an unobstructed free field.</summary>
/// <remarks>
/// Uses constant sound speed and spherical spreading. Pressure amplitude falls in proportion to 1/r,
/// so sound-pressure level falls by 20 log10(r/reference distance). This approximation is clamped at
/// the reference distance for closer points to avoid extrapolating a far-field point-source model into
/// its undefined near field. Atmospheric absorption, reflections, barriers, frequency, and Doppler
/// are not represented in this baseline.
/// </remarks>
public static class FreeFieldAcoustics
{
    /// <summary>Approximate speed of sound in air at 20 °C, in metres per second.</summary>
    public const double DefaultSpeedOfSoundMetersPerSecond = 343d;

    /// <summary>Estimates distance, arrival delay, direction, and free-field received level.</summary>
    /// <param name="source">The emitting point source.</param>
    /// <param name="listener">The receiving listener.</param>
    /// <param name="speedOfSoundMetersPerSecond">Configured sound speed in metres per second.</param>
    public static AcousticObservation Observe(
        AcousticSource source,
        AcousticListener listener,
        double speedOfSoundMetersPerSecond = DefaultSpeedOfSoundMetersPerSecond)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(listener);
        if (!double.IsFinite(speedOfSoundMetersPerSecond) || speedOfSoundMetersPerSecond <= 0d)
            throw new ArgumentOutOfRangeException(nameof(speedOfSoundMetersPerSecond), "Sound speed must be finite and greater than zero.");

        var dx = source.Position.X - listener.Position.X;
        var dy = source.Position.Y - listener.Position.Y;
        var dz = source.Position.Z - listener.Position.Z;
        var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (!double.IsFinite(distance))
            throw new ArithmeticException("Source-listener distance is outside the representable numeric range.");

        var levelDistance = Math.Max(distance, source.ReferenceDistanceMeters);
        var receivedLevel = source.ReferenceLevelDbSpl -
            20d * Math.Log10(levelDistance / source.ReferenceDistanceMeters);
        var directionX = distance > 0d ? dx / distance : 0d;
        var directionY = distance > 0d ? dy / distance : 0d;
        var directionZ = distance > 0d ? dz / distance : 0d;

        return new AcousticObservation(
            source.Id,
            listener.Id,
            distance,
            distance / speedOfSoundMetersPerSecond,
            receivedLevel,
            directionX,
            directionY,
            directionZ);
    }
}
