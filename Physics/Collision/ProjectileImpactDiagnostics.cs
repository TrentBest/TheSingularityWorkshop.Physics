using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>
/// Incoming impact measurements derived from a confirmed point-projectile/AABB intersection.
/// These measurements do not prescribe material damage or collision response.
/// </summary>
public sealed record ProjectileImpactDiagnosticsResult(
    string RunId,
    string TargetId,
    double TimeSeconds,
    Vector3d PositionMetres,
    Vector3d VelocityMetresPerSecond,
    double MassKilograms,
    Vector3d MomentumKilogramMetresPerSecond,
    double KineticEnergyJoules)
{
    /// <summary>Model identifier for the incoming-impact diagnostic contract.</summary>
    public string ModelId { get; init; } = "collision.projectile-impact-diagnostics";

    /// <summary>Version of this diagnostic contract.</summary>
    public string ModelVersion { get; init; } = "1";
}

/// <summary>
/// Calculates incoming projectile speed, momentum and kinetic energy from a computed impact.
/// No impulse, deformation, penetration, damage or energy transfer is inferred.
/// </summary>
public static class ProjectileImpactDiagnostics
{
    /// <summary>Computes diagnostics for a confirmed hit and a declared projectile mass in kilograms.</summary>
    public static ProjectileImpactDiagnosticsResult Compute(
        ProjectileExperimentResult experiment,
        ProjectileTargetIntersectionResult intersection,
        double massKilograms)
    {
        ArgumentNullException.ThrowIfNull(experiment);
        ArgumentNullException.ThrowIfNull(intersection);

        if (!intersection.Hit || intersection.IntersectionState is null)
            throw new ArgumentException("Impact diagnostics require a confirmed target intersection.", nameof(intersection));

        if (!string.Equals(experiment.Definition.RunId, intersection.RunId, StringComparison.Ordinal))
            throw new ArgumentException("The intersection must belong to the supplied experiment run.", nameof(intersection));

        if (!double.IsFinite(massKilograms) || massKilograms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(massKilograms), "Projectile mass must be positive and finite.");

        var state = intersection.IntersectionState.Value;
        Vector3d momentum;
        double kineticEnergy;
        try
        {
            momentum = state.Velocity * massKilograms;
            var speed = state.Velocity.Magnitude;
            kineticEnergy = 0.5d * massKilograms * speed * speed;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new OverflowException("Impact diagnostics exceed the finite numeric range.", exception);
        }
        catch (ArithmeticException exception)
        {
            throw new OverflowException("Impact diagnostics exceed the finite numeric range.", exception);
        }

        if (!double.IsFinite(kineticEnergy))
            throw new OverflowException("Impact kinetic energy exceeds the finite numeric range.");

        return new ProjectileImpactDiagnosticsResult(
            intersection.RunId,
            intersection.TargetId,
            state.ElapsedTime,
            state.Position,
            state.Velocity,
            massKilograms,
            momentum,
            kineticEnergy);
    }
}
