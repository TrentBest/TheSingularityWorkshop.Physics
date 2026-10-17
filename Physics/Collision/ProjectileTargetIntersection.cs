using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>
/// Result of intersecting an ideal point-projectile trajectory with an axis-aligned target.
/// </summary>
/// <param name="RunId">Run whose declared trajectory was tested.</param>
/// <param name="TargetId">Stable identity supplied by the caller for the target.</param>
/// <param name="Hit">Whether the trajectory intersects the target during the recorded duration.</param>
/// <param name="IntersectionState">Earliest intersection state, or null when there is no intersection.</param>
public sealed record ProjectileTargetIntersectionResult(
    string RunId,
    string TargetId,
    bool Hit,
    ProjectileState? IntersectionState)
{
    /// <summary>Model identifier for this bounded intersection contract.</summary>
    public string ModelId { get; init; } = "collision.constant-acceleration-point-vs-aabb";

    /// <summary>Algorithm/schema version independent of package version.</summary>
    public string ModelVersion { get; init; } = "1";

    /// <summary>First intersection time in seconds, if the target is hit.</summary>
    public double? FirstIntersectionTime => IntersectionState?.ElapsedTime;
}

/// <summary>
/// Finds the earliest intersection between an ideal point projectile and a fixed axis-aligned
/// target over a recorded experiment's duration. The trajectory is evaluated analytically;
/// the result is not limited to the discrete sample positions.
/// </summary>
/// <remarks>
/// Target bounds and projectile position must use the same right-handed, z-up world frame
/// and metres. This is point-versus-AABB geometry: projectile radius, target rotation,
/// moving targets, drag, and collision response are outside this contract.
/// </remarks>
public static class ProjectileTargetIntersection
{
    /// <summary>
    /// Returns the first target intersection within the experiment duration, including a
    /// boundary touch. A run beginning inside the target is a hit at time zero.
    /// </summary>
    public static ProjectileTargetIntersectionResult FindFirst(
        ProjectileExperimentResult experiment,
        string targetId,
        in AxisAlignedBounds targetBounds)
    {
        ArgumentNullException.ThrowIfNull(experiment);
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("A non-empty target identifier is required.", nameof(targetId));

        var definition = experiment.Definition;
        var initial = new ProjectileState(
            definition.InitialPosition,
            definition.CreateInitialVelocity(),
            0d);
        var duration = experiment.Duration;

        if (Contains(targetBounds, initial.Position))
            return Hit(experiment, targetId, initial);

        var candidates = new List<double> { 0d, duration };
        AddBoundaryRoots(candidates, initial.Position.X, initial.Velocity.X, definition.Gravity.X,
            targetBounds.Min.X, targetBounds.Max.X, duration);
        AddBoundaryRoots(candidates, initial.Position.Y, initial.Velocity.Y, definition.Gravity.Y,
            targetBounds.Min.Y, targetBounds.Max.Y, duration);
        AddBoundaryRoots(candidates, initial.Position.Z, initial.Velocity.Z, definition.Gravity.Z,
            targetBounds.Min.Z, targetBounds.Max.Z, duration);

        candidates.Sort();
        var previous = double.NaN;
        foreach (var time in candidates)
        {
            if (time < 0d || time > duration || time.Equals(previous))
                continue;
            previous = time;

            var state = time == 0d
                ? initial
                : ConstantAccelerationProjectileSolver.Advance(initial, definition.Gravity, time);

            if (Contains(targetBounds, state.Position))
                return Hit(experiment, targetId, state);
        }

        return new ProjectileTargetIntersectionResult(
            definition.RunId,
            targetId,
            false,
            null);
    }

    private static ProjectileTargetIntersectionResult Hit(
        ProjectileExperimentResult experiment,
        string targetId,
        ProjectileState state) =>
        new(experiment.Definition.RunId, targetId, true, state);

    private static bool Contains(in AxisAlignedBounds bounds, Vector3d point) =>
        Within(point.X, bounds.Min.X, bounds.Max.X) &&
        Within(point.Y, bounds.Min.Y, bounds.Max.Y) &&
        Within(point.Z, bounds.Min.Z, bounds.Max.Z);

    private static bool Within(double value, double minimum, double maximum)
    {
        // Scale-aware tolerance absorbs floating-point root evaluation noise at closed faces.
        var scale = Math.Max(1d, Math.Max(Math.Abs(value), Math.Max(Math.Abs(minimum), Math.Abs(maximum))));
        var tolerance = 1e-10d * scale;
        return value >= minimum - tolerance && value <= maximum + tolerance;
    }

    private static void AddBoundaryRoots(
        ICollection<double> candidates,
        double position,
        double velocity,
        double acceleration,
        double minimum,
        double maximum,
        double duration)
    {
        AddRoots(candidates, position, velocity, acceleration, minimum, duration);
        AddRoots(candidates, position, velocity, acceleration, maximum, duration);
    }

    private static void AddRoots(
        ICollection<double> candidates,
        double position,
        double velocity,
        double acceleration,
        double boundary,
        double duration)
    {
        // Solve 0.5*a*t^2 + v*t + (p-boundary) = 0.
        var a = 0.5d * acceleration;
        var b = velocity;
        var c = position - boundary;

        if (a == 0d)
        {
            if (b != 0d)
                AddCandidate(candidates, -c / b, duration);
            return;
        }

        var discriminant = (b * b) - (4d * a * c);
        if (discriminant < 0d)
            return;

        var root = Math.Sqrt(discriminant);
        if (root == 0d)
        {
            AddCandidate(candidates, -b / (2d * a), duration);
            return;
        }

        // This form reduces cancellation when b and sqrt(discriminant) are similar.
        var q = -0.5d * (b + Math.CopySign(root, b));
        if (q == 0d)
        {
            AddCandidate(candidates, -b / (2d * a), duration);
            return;
        }

        AddCandidate(candidates, q / a, duration);
        AddCandidate(candidates, c / q, duration);
    }

    private static void AddCandidate(ICollection<double> candidates, double time, double duration)
    {
        if (double.IsFinite(time) && time >= 0d && time <= duration)
            candidates.Add(time);
    }
}
