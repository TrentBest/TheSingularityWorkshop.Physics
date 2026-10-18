using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>Records the deterministic result of an explicitly selected perfectly inelastic merger.</summary>
/// <param name="EventId">Stable event identity derived from the sorted source body identities.</param>
/// <param name="BodyAId">First source identity in ordinal order.</param>
/// <param name="BodyBId">Second source identity in ordinal order.</param>
/// <param name="MergedBody">The resulting body, conserving total mass, centre of mass, and linear momentum.</param>
/// <param name="RelativeKineticEnergyJoules">Relative translational kinetic energy removed by the perfectly inelastic merger.</param>
public sealed record OrbitalMergeEvent(
    string EventId,
    string BodyAId,
    string BodyBId,
    OrbitalBodyState MergedBody,
    double RelativeKineticEnergyJoules);

/// <summary>Applies an explicit perfectly inelastic merge policy to one overlapping pair of spherical orbital bodies.</summary>
/// <remarks>
/// This is a pairwise encounter policy, not a collision detector for an entire N-body system.
/// It only merges when the supplied endpoint spheres overlap or touch; callers must choose a sufficiently
/// small integration step or add swept collision detection to avoid tunnelling. It conserves mass,
/// centre of mass, and linear momentum, and uses volume-additive sphere radius. It does not conserve
/// kinetic energy: the removed relative translational energy is reported. Spin, deformation, heat,
/// fragmentation, restitution, and angular-momentum storage are outside this model.
/// </remarks>
public static class OrbitalMergeResolver
{
    /// <summary>
    /// Merges two overlapping or touching spherical bodies, returning a stable event; returns null when separated.
    /// </summary>
    public static OrbitalMergeEvent? TryMerge(OrbitalBodyState bodyA, OrbitalBodyState bodyB)
    {
        ArgumentNullException.ThrowIfNull(bodyA);
        ArgumentNullException.ThrowIfNull(bodyB);
        bodyA.EnsureValid();
        bodyB.EnsureValid();

        if (StringComparer.Ordinal.Equals(bodyA.BodyId, bodyB.BodyId))
            throw new ArgumentException("A merge requires two distinct body identities.", nameof(bodyB));

        var first = StringComparer.Ordinal.Compare(bodyA.BodyId, bodyB.BodyId) < 0 ? bodyA : bodyB;
        var second = ReferenceEquals(first, bodyA) ? bodyB : bodyA;
        var separation = Vector3d.Distance(first.PositionMeters, second.PositionMeters);
        var combinedRadius = first.RadiusMeters + second.RadiusMeters;
        if (!double.IsFinite(combinedRadius))
            throw new ArithmeticException("Combined body radius exceeded the finite numeric range.");
        if (separation > combinedRadius)
            return null;

        var totalMass = first.MassKilograms + second.MassKilograms;
        if (!double.IsFinite(totalMass))
            throw new ArithmeticException("Combined body mass exceeded the finite numeric range.");

        var firstWeight = first.MassKilograms / totalMass;
        var secondWeight = second.MassKilograms / totalMass;
        var position = first.PositionMeters * firstWeight + second.PositionMeters * secondWeight;
        var velocity = first.VelocityMetersPerSecond * firstWeight +
                       second.VelocityMetersPerSecond * secondWeight;
        var radius = CombinedVolumeRadius(first.RadiusMeters, second.RadiusMeters);

        var relativeVelocity = first.VelocityMetersPerSecond - second.VelocityMetersPerSecond;
        var reducedMass = (first.MassKilograms / totalMass) * second.MassKilograms;
        var relativeEnergy = 0.5d * reducedMass * relativeVelocity.Magnitude * relativeVelocity.Magnitude;
        if (!double.IsFinite(relativeEnergy))
            throw new ArithmeticException("Merge energy accounting exceeded the finite numeric range.");

        var mergedId = $"merge[{first.BodyId.Length}:{first.BodyId}{second.BodyId.Length}:{second.BodyId}]";
        var merged = new OrbitalBodyState(mergedId, totalMass, radius, position, velocity).EnsureValid();
        var eventId = $"orbital-merge[{first.BodyId.Length}:{first.BodyId}{second.BodyId.Length}:{second.BodyId}]";

        return new OrbitalMergeEvent(eventId, first.BodyId, second.BodyId, merged, relativeEnergy);
    }

    private static double CombinedVolumeRadius(double radiusA, double radiusB)
    {
        var scale = Math.Max(radiusA, radiusB);
        var a = radiusA / scale;
        var b = radiusB / scale;
        var radius = scale * Math.Cbrt((a * a * a) + (b * b * b));
        if (!double.IsFinite(radius) || radius <= 0d)
            throw new ArithmeticException("Combined volume-equivalent radius is outside the finite numeric range.");
        return radius;
    }
}
