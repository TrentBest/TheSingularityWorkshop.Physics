using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>
/// Immutable definition of a bounded, prescribed root-pose trajectory.
/// </summary>
/// <remarks>
/// Position is interpolated linearly in right-handed world-frame metres.
/// Orientation follows shortest-arc spherical interpolation between normalized
/// body-local-to-world quaternions. Time is absolute logical seconds. The
/// trajectory has constant translational and angular velocity in its open
/// interval; velocity changes discontinuously at its endpoints when the actor
/// is considered stationary outside the interval. No collision, contact,
/// force, balance, or obstacle response is modeled.
/// </remarks>
public sealed class PrescribedKinematicMotionDefinition
{
    /// <summary>Gets the stable motion-definition identity.</summary>
    public string MotionId { get; }
    /// <summary>Gets the stable actor identity.</summary>
    public string ActorId { get; }
    /// <summary>Gets the starting achieved pose.</summary>
    public Pose3d StartPose { get; }
    /// <summary>Gets the destination pose.</summary>
    public Pose3d EndPose { get; }
    /// <summary>Gets the absolute logical start time in seconds.</summary>
    public double StartTimeSeconds { get; }
    /// <summary>Gets the positive trajectory duration in seconds.</summary>
    public double DurationSeconds { get; }
    /// <summary>Gets the declared maximum linear speed in metres per second.</summary>
    public double MaximumLinearSpeed { get; }
    /// <summary>Gets the declared maximum angular speed in radians per second.</summary>
    public double MaximumAngularSpeed { get; }

    /// <summary>Creates a validated immutable prescribed-motion definition.</summary>
    public PrescribedKinematicMotionDefinition(
        string motionId,
        string actorId,
        Pose3d startPose,
        Pose3d endPose,
        double startTimeSeconds,
        double durationSeconds,
        double maximumLinearSpeed,
        double maximumAngularSpeed)
    {
        if (string.IsNullOrWhiteSpace(motionId))
            throw new ArgumentException("A stable motion identity is required.", nameof(motionId));
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("A stable actor identity is required.", nameof(actorId));
        if (!startPose.IsValid)
            throw new ArgumentException("A valid start pose is required.", nameof(startPose));
        if (!endPose.IsValid)
            throw new ArgumentException("A valid destination pose is required.", nameof(endPose));
        if (!double.IsFinite(startTimeSeconds))
            throw new ArgumentOutOfRangeException(nameof(startTimeSeconds));
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be finite and positive.");
        if (!double.IsFinite(maximumLinearSpeed) || maximumLinearSpeed < 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumLinearSpeed));
        if (!double.IsFinite(maximumAngularSpeed) || maximumAngularSpeed < 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumAngularSpeed));

        var endTime = startTimeSeconds + durationSeconds;
        if (!double.IsFinite(endTime))
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "The end time must remain finite.");

        var linearSpeed = Vector3d.Distance(startPose.Position, endPose.Position) / durationSeconds;
        var angularSpeed = AngularDistance(startPose.Orientation, endPose.Orientation) / durationSeconds;
        if (!double.IsFinite(linearSpeed) || linearSpeed > maximumLinearSpeed)
            throw new ArgumentException("The requested translation exceeds the declared maximum linear speed.", nameof(maximumLinearSpeed));
        if (!double.IsFinite(angularSpeed) || angularSpeed > maximumAngularSpeed)
            throw new ArgumentException("The requested rotation exceeds the declared maximum angular speed.", nameof(maximumAngularSpeed));

        MotionId = motionId;
        ActorId = actorId;
        StartPose = startPose;
        EndPose = endPose;
        StartTimeSeconds = startTimeSeconds;
        DurationSeconds = durationSeconds;
        MaximumLinearSpeed = maximumLinearSpeed;
        MaximumAngularSpeed = maximumAngularSpeed;
    }

    internal static double AngularDistance(Quaterniond start, Quaterniond end)
    {
        var a = start.Normalized();
        var b = end.Normalized();
        var dot = (a.W * b.W) + (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        dot = Math.Clamp(Math.Abs(dot), 0d, 1d);
        return 2d * Math.Acos(dot);
    }
}

/// <summary>An immutable observation evaluated from a prescribed motion definition.</summary>
/// <param name="MotionId">The source motion-definition identity.</param>
/// <param name="ActorId">The actor identity.</param>
/// <param name="State">The evaluated pose and prescribed velocity at this logical sample.</param>
/// <param name="IsComplete">Whether the sample is at the destination endpoint.</param>
public readonly record struct PrescribedKinematicMotionObservation(
    string MotionId,
    string ActorId,
    RigidBodyState State,
    bool IsComplete);

/// <summary>Pure absolute-logical-time evaluation of prescribed actor root motion.</summary>
public static class PrescribedKinematicMotion
{
    /// <summary>Evaluates a definition at an explicit logical time and committed sample index.</summary>
    /// <remarks>
    /// Evaluation is independent of render cadence and has no mutable internal state.
    /// Callers own sample ordering and commit policy. Cancellation therefore means
    /// retaining the last accepted observation; it does not mutate this evaluator.
    /// </remarks>
    public static PrescribedKinematicMotionObservation Evaluate(
        PrescribedKinematicMotionDefinition definition,
        double logicalTimeSeconds,
        long stepIndex)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!double.IsFinite(logicalTimeSeconds))
            throw new ArgumentOutOfRangeException(nameof(logicalTimeSeconds));
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex));

        var endTime = definition.StartTimeSeconds + definition.DurationSeconds;
        if (logicalTimeSeconds < definition.StartTimeSeconds || logicalTimeSeconds > endTime)
            throw new ArgumentOutOfRangeException(nameof(logicalTimeSeconds), "Evaluation time must lie within the declared motion interval.");

        var t = (logicalTimeSeconds - definition.StartTimeSeconds) / definition.DurationSeconds;
        var positionDelta = definition.EndPose.Position - definition.StartPose.Position;
        var linearVelocity = positionDelta / definition.DurationSeconds;
        var position = definition.StartPose.Position + (positionDelta * t);
        var orientation = SlerpShortest(definition.StartPose.Orientation, definition.EndPose.Orientation, t);
        var angularVelocity = AngularVelocityWorld(
            definition.StartPose.Orientation,
            definition.EndPose.Orientation,
            definition.DurationSeconds);

        var state = new RigidBodyState(
            definition.ActorId,
            new Pose3d(position, orientation),
            linearVelocity,
            angularVelocity,
            stepIndex,
            logicalTimeSeconds);

        return new PrescribedKinematicMotionObservation(
            definition.MotionId,
            definition.ActorId,
            state,
            logicalTimeSeconds == endTime);
    }

    private static Quaterniond SlerpShortest(Quaterniond start, Quaterniond end, double t)
    {
        var a = start.Normalized();
        var b = end.Normalized();
        var dot = (a.W * b.W) + (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        if (dot < 0d)
        {
            b = new Quaterniond(-b.W, -b.X, -b.Y, -b.Z);
            dot = -dot;
        }

        dot = Math.Clamp(dot, -1d, 1d);
        if (dot > 0.9995d)
        {
            return new Quaterniond(
                a.W + ((b.W - a.W) * t),
                a.X + ((b.X - a.X) * t),
                a.Y + ((b.Y - a.Y) * t),
                a.Z + ((b.Z - a.Z) * t)).Normalized();
        }

        var angle = Math.Acos(dot);
        var sinAngle = Math.Sin(angle);
        var startWeight = Math.Sin((1d - t) * angle) / sinAngle;
        var endWeight = Math.Sin(t * angle) / sinAngle;
        return new Quaterniond(
            (a.W * startWeight) + (b.W * endWeight),
            (a.X * startWeight) + (b.X * endWeight),
            (a.Y * startWeight) + (b.Y * endWeight),
            (a.Z * startWeight) + (b.Z * endWeight)).Normalized();
    }

    private static Vector3d AngularVelocityWorld(Quaterniond start, Quaterniond end, double duration)
    {
        var a = start.Normalized();
        var b = end.Normalized();
        // Active body-local-to-world rotations: the world-space delta composes on the left.
        var delta = b * a.Conjugate();
        if (delta.W < 0d)
            delta = new Quaterniond(-delta.W, -delta.X, -delta.Y, -delta.Z);

        var vector = new Vector3d(delta.X, delta.Y, delta.Z);
        var magnitude = vector.Magnitude;
        if (magnitude == 0d)
            return Vector3d.Zero;

        var angle = 2d * Math.Atan2(magnitude, Math.Clamp(delta.W, -1d, 1d));
        return vector.Normalized() * (angle / duration);
    }
}
