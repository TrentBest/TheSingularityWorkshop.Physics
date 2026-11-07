namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>Physical parameters for an ideal planar point-mass pendulum.</summary>
/// <remarks>
/// The pivot is fixed, the gravitational field is uniform, and the rod/string is
/// massless and inextensible. The model has no damping, drive, or pivot motion.
/// </remarks>
public sealed record PlanarPendulumDefinition(
    double MassKilograms,
    double LengthMetres,
    double GravitationalAccelerationMetresPerSecondSquared)
{
    /// <summary>Validates that mass, length, and gravitational acceleration are finite and positive.</summary>
    public void Validate()
    {
        if (!double.IsFinite(MassKilograms) || MassKilograms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(MassKilograms), "Mass must be finite and positive.");
        if (!double.IsFinite(LengthMetres) || LengthMetres <= 0d)
            throw new ArgumentOutOfRangeException(nameof(LengthMetres), "Length must be finite and positive.");
        if (!double.IsFinite(GravitationalAccelerationMetresPerSecondSquared) ||
            GravitationalAccelerationMetresPerSecondSquared <= 0d)
            throw new ArgumentOutOfRangeException(
                nameof(GravitationalAccelerationMetresPerSecondSquared),
                "Gravitational acceleration must be finite and positive.");
    }
}
