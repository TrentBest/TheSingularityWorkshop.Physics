namespace TheSingularityWorkshop.Physics.Acoustics;

/// <summary>
/// An axis-aligned rectangular acoustic room with per-wall pressure-amplitude reflection coefficients.
/// </summary>
/// <remarks>
/// Bounds are expressed in metres in the same world frame as the source and listener. Each coefficient
/// is a frequency-independent pressure-amplitude ratio from zero (no reflected pressure) to one
/// (ideal pressure reflection). This is a deliberately bounded first-order geometry model, not a
/// frequency-dependent material model or a complete room-acoustics solver.
/// </remarks>
public sealed class AcousticRoomBox
{
    /// <summary>Creates a room with finite positive dimensions and per-wall reflection coefficients.</summary>
    public AcousticRoomBox(
        string id,
        AcousticPosition3 minimum,
        AcousticPosition3 maximum,
        double xMinReflectionCoefficient = 0.8d,
        double xMaxReflectionCoefficient = 0.8d,
        double yMinReflectionCoefficient = 0.8d,
        double yMaxReflectionCoefficient = 0.8d,
        double zMinReflectionCoefficient = 0.8d,
        double zMaxReflectionCoefficient = 0.8d)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A room identifier is required.", nameof(id));
        if (minimum.X >= maximum.X || minimum.Y >= maximum.Y || minimum.Z >= maximum.Z)
            throw new ArgumentException("Room maximum bounds must be greater than minimum bounds on every axis.", nameof(maximum));

        ValidateCoefficient(xMinReflectionCoefficient, nameof(xMinReflectionCoefficient));
        ValidateCoefficient(xMaxReflectionCoefficient, nameof(xMaxReflectionCoefficient));
        ValidateCoefficient(yMinReflectionCoefficient, nameof(yMinReflectionCoefficient));
        ValidateCoefficient(yMaxReflectionCoefficient, nameof(yMaxReflectionCoefficient));
        ValidateCoefficient(zMinReflectionCoefficient, nameof(zMinReflectionCoefficient));
        ValidateCoefficient(zMaxReflectionCoefficient, nameof(zMaxReflectionCoefficient));

        Id = id;
        Minimum = minimum;
        Maximum = maximum;
        XMinReflectionCoefficient = xMinReflectionCoefficient;
        XMaxReflectionCoefficient = xMaxReflectionCoefficient;
        YMinReflectionCoefficient = yMinReflectionCoefficient;
        YMaxReflectionCoefficient = yMaxReflectionCoefficient;
        ZMinReflectionCoefficient = zMinReflectionCoefficient;
        ZMaxReflectionCoefficient = zMaxReflectionCoefficient;
    }

    /// <summary>Stable room identifier.</summary>
    public string Id { get; }

    /// <summary>Minimum room corner in metres.</summary>
    public AcousticPosition3 Minimum { get; }

    /// <summary>Maximum room corner in metres.</summary>
    public AcousticPosition3 Maximum { get; }

    /// <summary>Pressure-amplitude reflection coefficient for the minimum-X wall.</summary>
    public double XMinReflectionCoefficient { get; }

    /// <summary>Pressure-amplitude reflection coefficient for the maximum-X wall.</summary>
    public double XMaxReflectionCoefficient { get; }

    /// <summary>Pressure-amplitude reflection coefficient for the minimum-Y wall.</summary>
    public double YMinReflectionCoefficient { get; }

    /// <summary>Pressure-amplitude reflection coefficient for the maximum-Y wall.</summary>
    public double YMaxReflectionCoefficient { get; }

    /// <summary>Pressure-amplitude reflection coefficient for the minimum-Z wall.</summary>
    public double ZMinReflectionCoefficient { get; }

    /// <summary>Pressure-amplitude reflection coefficient for the maximum-Z wall.</summary>
    public double ZMaxReflectionCoefficient { get; }

    /// <summary>Whether a position lies strictly inside the room, not on a boundary.</summary>
    public bool ContainsInterior(AcousticPosition3 position) =>
        position.X > Minimum.X && position.X < Maximum.X &&
        position.Y > Minimum.Y && position.Y < Maximum.Y &&
        position.Z > Minimum.Z && position.Z < Maximum.Z;

    private static void ValidateCoefficient(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d || value > 1d)
            throw new ArgumentOutOfRangeException(parameterName, "Reflection coefficients must be finite pressure-amplitude ratios between zero and one.");
    }
}
