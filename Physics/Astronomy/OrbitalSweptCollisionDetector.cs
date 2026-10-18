namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>A predicted first contact between two orbital spheres during a linear sweep.</summary>
/// <param name="BodyAId">First body identity in ordinal order.</param>
/// <param name="BodyBId">Second body identity in ordinal order.</param>
/// <param name="TimeOfImpactSeconds">Seconds from the supplied states to first contact, inclusive of both interval endpoints.</param>
public sealed record OrbitalSweptCollision(
    string BodyAId,
    string BodyBId,
    double TimeOfImpactSeconds);

/// <summary>Predicts sphere contact along constant-velocity trajectories over a finite interval.</summary>
/// <remarks>
/// This is linear swept-sphere detection, not an acceleration-aware continuous collision solver.
/// It uses the supplied positions and velocities as straight-line trajectories for the requested interval;
/// callers integrating gravity should keep steps sufficiently short or use a trajectory-aware method.
/// Detection is observational: it does not mutate either body or choose a collision outcome.
/// </remarks>
public static class OrbitalSweptCollisionDetector
{
    /// <summary>
    /// Returns the earliest sphere contact within the interval, or null when the swept spheres do not meet.
    /// The result is independent of argument order.
    /// </summary>
    /// <exception cref="ArgumentNullException">A body is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The interval is not finite and positive.</exception>
    /// <exception cref="ArgumentException">The bodies have the same identity.</exception>
    /// <exception cref="ArithmeticException">The calculation exceeds the finite numeric range.</exception>
    public static OrbitalSweptCollision? TryDetect(
        OrbitalBodyState bodyA,
        OrbitalBodyState bodyB,
        double intervalSeconds)
    {
        ArgumentNullException.ThrowIfNull(bodyA);
        ArgumentNullException.ThrowIfNull(bodyB);
        bodyA.EnsureValid();
        bodyB.EnsureValid();

        if (!double.IsFinite(intervalSeconds) || intervalSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "The sweep interval must be finite and positive.");
        if (StringComparer.Ordinal.Equals(bodyA.BodyId, bodyB.BodyId))
            throw new ArgumentException("A swept collision requires two distinct body identities.", nameof(bodyB));

        var first = StringComparer.Ordinal.Compare(bodyA.BodyId, bodyB.BodyId) < 0 ? bodyA : bodyB;
        var second = ReferenceEquals(first, bodyA) ? bodyB : bodyA;

        var relativePosition = second.PositionMeters - first.PositionMeters;
        var relativeVelocity = second.VelocityMetersPerSecond - first.VelocityMetersPerSecond;
        var combinedRadius = first.RadiusMeters + second.RadiusMeters;
        if (!double.IsFinite(combinedRadius))
            throw new ArithmeticException("Combined body radius exceeded the finite numeric range.");

        var c = relativePosition.Dot(relativePosition) - (combinedRadius * combinedRadius);
        if (!double.IsFinite(c))
            throw new ArithmeticException("Initial separation exceeded the finite numeric range.");
        if (c <= 0d)
            return new OrbitalSweptCollision(first.BodyId, second.BodyId, 0d);

        var a = relativeVelocity.Dot(relativeVelocity);
        if (a == 0d)
            return null;

        var b = 2d * relativePosition.Dot(relativeVelocity);
        if (!double.IsFinite(b))
            throw new ArithmeticException("Relative trajectory exceeded the finite numeric range.");
        if (b >= 0d)
            return null;

        var discriminant = (b * b) - (4d * a * c);
        if (!double.IsFinite(discriminant))
            throw new ArithmeticException("Collision discriminant exceeded the finite numeric range.");
        if (discriminant < 0d)
            return null;

        var time = (-b - Math.Sqrt(discriminant)) / (2d * a);
        if (!double.IsFinite(time))
            throw new ArithmeticException("Time of impact exceeded the finite numeric range.");
        if (time < 0d || time > intervalSeconds)
            return null;

        return new OrbitalSweptCollision(first.BodyId, second.BodyId, time);
    }
}
