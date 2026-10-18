using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>A finite spherical body state for Newtonian N-body integration.</summary>
/// <param name="BodyId">Stable, unique body identity.</param>
/// <param name="MassKilograms">Positive mass in kilograms.</param>
/// <param name="RadiusMeters">Positive physical radius in metres; used for identity and later encounter policy, not yet collision resolution.</param>
/// <param name="PositionMeters">Position in metres in the shared inertial Cartesian frame.</param>
/// <param name="VelocityMetersPerSecond">Velocity in metres per second in the same frame.</param>
public sealed record OrbitalBodyState(
    string BodyId,
    double MassKilograms,
    double RadiusMeters,
    Vector3d PositionMeters,
    Vector3d VelocityMetersPerSecond)
{
    /// <summary>Validates the body's identity and physical magnitudes.</summary>
    public OrbitalBodyState EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(BodyId))
            throw new ArgumentException("A stable body ID is required.", nameof(BodyId));
        if (!double.IsFinite(MassKilograms) || MassKilograms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(MassKilograms), "Mass must be finite and positive.");
        if (!double.IsFinite(RadiusMeters) || RadiusMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(RadiusMeters), "Radius must be finite and positive.");

        return this;
    }
}
