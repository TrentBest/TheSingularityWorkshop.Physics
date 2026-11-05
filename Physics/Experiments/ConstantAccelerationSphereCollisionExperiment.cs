using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>Classification produced by the bounded constant-acceleration sphere experiment.</summary>
public enum AcceleratedSpherePredictionStatus
{
    /// <summary>The interval was conservatively excluded.</summary>
    ProvenNoContact,
    /// <summary>A sampled overlap was found and the earliest possible contact is bracketed.</summary>
    SampledOverlap,
    /// <summary>Contact remains possible, but the configured bounds or budget did not resolve it.</summary>
    UnresolvedPossibleContact
}

/// <summary>Immutable result of an observational accelerated sphere prediction.</summary>
public sealed record AcceleratedSpherePrediction(
    AcceleratedSpherePredictionStatus Status,
    double EarliestPossibleSeconds,
    double LatestPossibleSeconds,
    int Evaluations,
    int Subdivisions);

/// <summary>
/// Experimental bounded predictor for two spheres following constant-acceleration trajectories.
/// It does not mutate bodies or resolve contacts. Unresolved results are never promoted to contact.
/// </summary>
public static class ConstantAccelerationSphereCollisionExperiment
{
    /// <summary>
    /// Searches the interval in deterministic earliest-first order. Acceleration vectors are expressed
    /// in metres per second squared in the same inertial frame as the body states.
    /// </summary>
    public static AcceleratedSpherePrediction Predict(
        OrbitalBodyState bodyA,
        Vector3d accelerationA,
        OrbitalBodyState bodyB,
        Vector3d accelerationB,
        double intervalSeconds,
        double timeToleranceSeconds,
        int maximumEvaluations = 4096,
        int maximumDepth = 64)
    {
        ArgumentNullException.ThrowIfNull(bodyA);
        ArgumentNullException.ThrowIfNull(bodyB);
        bodyA.EnsureValid();
        bodyB.EnsureValid();
        if (StringComparer.Ordinal.Equals(bodyA.BodyId, bodyB.BodyId))
            throw new ArgumentException("Prediction requires distinct body identities.", nameof(bodyB));
        if (!double.IsFinite(intervalSeconds) || intervalSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
        if (!double.IsFinite(timeToleranceSeconds) || timeToleranceSeconds <= 0d || timeToleranceSeconds > intervalSeconds)
            throw new ArgumentOutOfRangeException(nameof(timeToleranceSeconds));
        if (maximumEvaluations < 2)
            throw new ArgumentOutOfRangeException(nameof(maximumEvaluations), "At least two trajectory evaluations are required.");
        if (maximumDepth < 1)
            throw new ArgumentOutOfRangeException(nameof(maximumDepth));

        var first = StringComparer.Ordinal.Compare(bodyA.BodyId, bodyB.BodyId) < 0 ? bodyA : bodyB;
        var second = ReferenceEquals(first, bodyA) ? bodyB : bodyA;
        var firstAcceleration = ReferenceEquals(first, bodyA) ? accelerationA : accelerationB;
        var secondAcceleration = ReferenceEquals(first, bodyA) ? accelerationB : accelerationA;

        var r0 = second.PositionMeters - first.PositionMeters;
        var v0 = second.VelocityMetersPerSecond - first.VelocityMetersPerSecond;
        var acceleration = secondAcceleration - firstAcceleration;
        var radius = first.RadiusMeters + second.RadiusMeters;
        if (!double.IsFinite(radius))
            throw new ArithmeticException("Combined radius is outside the finite numeric range.");

        var evaluations = 0;
        var subdivisions = 0;
        if (!TryMagnitude(r0, out var initialDistance))
            return Unresolved(0d, intervalSeconds);
        if (initialDistance <= radius)
            return new(AcceleratedSpherePredictionStatus.SampledOverlap, 0d, 0d, evaluations, subdivisions);

        var initial = new Sample(0d, r0, v0, initialDistance);
        var result = Search(initial, intervalSeconds, 0);
        return result;

        AcceleratedSpherePrediction Search(Sample start, double endTime, int depth)
        {
            var width = endTime - start.Time;
            var midpointTime = start.Time + (width * 0.5d);
            if (evaluations + 2 > maximumEvaluations)
                return Unresolved(start.Time, endTime);

            if (!TryEvaluate(midpointTime, out var midpoint) ||
                !TryEvaluate(endTime, out var end))
                return Unresolved(start.Time, endTime);

            evaluations += 2;
            if (CanExclude(start, midpoint, end))
                return new(AcceleratedSpherePredictionStatus.ProvenNoContact,
                    start.Time, endTime, evaluations, subdivisions);

            if (width <= timeToleranceSeconds || depth >= maximumDepth || midpointTime == start.Time || midpointTime == endTime)
            {
                if (midpoint.Distance <= radius)
                    return new(AcceleratedSpherePredictionStatus.SampledOverlap,
                        start.Time, midpoint.Time, evaluations, subdivisions);
                if (end.Distance <= radius)
                    return new(AcceleratedSpherePredictionStatus.SampledOverlap,
                        start.Time, end.Time, evaluations, subdivisions);
                return Unresolved(start.Time, endTime);
            }

            subdivisions++;
            var left = Search(start, midpoint.Time, depth + 1);
            if (left.Status != AcceleratedSpherePredictionStatus.ProvenNoContact)
                return left;
            return Search(midpoint, end.Time, depth + 1);
        }

        bool TryEvaluate(double time, out Sample sample)
        {
            sample = default;
            var halfTimeSquared = 0.5d * time * time;
            if (!double.IsFinite(halfTimeSquared))
                return false;
            if (!TryVector(
                r0.X + (v0.X * time) + (acceleration.X * halfTimeSquared),
                r0.Y + (v0.Y * time) + (acceleration.Y * halfTimeSquared),
                r0.Z + (v0.Z * time) + (acceleration.Z * halfTimeSquared),
                out var position))
                return false;
            if (!TryVector(
                v0.X + (acceleration.X * time),
                v0.Y + (acceleration.Y * time),
                v0.Z + (acceleration.Z * time),
                out var velocity))
                return false;
            if (!TryMagnitude(position, out var distance))
                return false;
            sample = new Sample(time, position, velocity, distance);
            return true;
        }

        bool CanExclude(Sample start, Sample midpoint, Sample end)
        {
            // The norm of affine velocity is convex, so its maximum on this interval
            // is bounded by the larger endpoint speed. Every point is at most width/2
            // from the midpoint; this yields a conservative distance lower bound.
            if (!TryMagnitude(start.Velocity, out var startSpeed) ||
                !TryMagnitude(end.Velocity, out var endSpeed))
                return false;
            var maximumSpeed = Math.Max(startSpeed, endSpeed);
            var travelBound = maximumSpeed * (end.Time - start.Time) * 0.5d;
            var rawLowerBound = midpoint.Distance - travelBound;
            if (!double.IsFinite(travelBound) || !double.IsFinite(rawLowerBound))
                return false;

            // Round the lower bound down and the contact radius up before using a
            // strict separation test. Failure to establish separation means subdivide.
            var lowerBound = Math.BitDecrement(rawLowerBound);
            var upperRadius = Math.BitIncrement(radius);
            return lowerBound > upperRadius;
        }

        AcceleratedSpherePrediction Unresolved(double from, double to) =>
            new(AcceleratedSpherePredictionStatus.UnresolvedPossibleContact,
                from, to, evaluations, subdivisions);
    }

    private static bool TryVector(double x, double y, double z, out Vector3d value)
    {
        value = Vector3d.Zero;
        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(z))
            return false;
        value = new Vector3d(x, y, z);
        return true;
    }

    private static bool TryMagnitude(Vector3d value, out double magnitude)
    {
        try
        {
            magnitude = value.Magnitude;
            return double.IsFinite(magnitude);
        }
        catch (ArithmeticException)
        {
            magnitude = 0d;
            return false;
        }
    }

    private readonly record struct Sample(double Time, Vector3d Position, Vector3d Velocity, double Distance);
}
