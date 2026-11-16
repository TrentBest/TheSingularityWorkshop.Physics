using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>Supported prescribed-trajectory time profiles.</summary>
public enum PrescribedKinematicMotionProfile
{
    /// <summary>Constant velocity over the open motion interval; endpoint velocity is discontinuous.</summary>
    ConstantVelocity = 0,

    /// <summary>Quintic smooth-step with zero velocity and acceleration at both endpoints.</summary>
    QuinticSmoothStep = 1
}

/// <summary>
/// Immutable definition of one bounded prescribed kinematic trajectory.
/// </summary>
/// <remarks>
/// Position is in metres in a right-handed world frame. Orientation is a normalized
/// active body-local-to-world rotation stored as scalar-first (W, X, Y, Z).
/// Time is seconds and angular quantities are radians. The selected profile shapes both
/// translation and shortest-arc orientation progress. The model is prescribed
/// kinematics, not force-driven dynamics, and does not provide
/// collision, contact, balance, gait, or obstacle response.
/// </remarks>
public sealed record PrescribedKinematicMotionDefinition
{
    /// <summary>Gets the stable identity of this motion definition.</summary>
    public string DefinitionId { get; }

    /// <summary>Gets the stable actor/body identity.</summary>
    public string ActorId { get; }

    /// <summary>Gets the positive immutable definition version.</summary>
    public int Version { get; }

    /// <summary>Gets the initial achieved pose used as the trajectory origin.</summary>
    public Pose3d StartPose { get; }

    /// <summary>Gets the requested destination pose.</summary>
    public Pose3d DestinationPose { get; }

    /// <summary>Gets the absolute logical start time in seconds.</summary>
    public double StartTimeSeconds { get; }

    /// <summary>Gets the positive trajectory duration in seconds.</summary>
    public double DurationSeconds { get; }

    /// <summary>Gets the declared maximum linear speed in metres per second.</summary>
    public double MaximumLinearSpeed { get; }

    /// <summary>Gets the declared maximum angular speed in radians per second.</summary>
    public double MaximumAngularSpeed { get; }

    /// <summary>Gets the selected time profile.</summary>
    public PrescribedKinematicMotionProfile Profile { get; }

    /// <summary>Gets the declared maximum linear acceleration in metres per second squared.</summary>
    public double MaximumLinearAcceleration { get; }

    /// <summary>Gets the declared maximum angular acceleration in radians per second squared.</summary>
    public double MaximumAngularAcceleration { get; }

    internal Vector3d Translation => DestinationPose.Position - StartPose.Position;
    internal double RotationAngle { get; }
    internal Vector3d RotationAxis { get; }

    /// <summary>Creates a validated, immutable motion definition.</summary>
    public PrescribedKinematicMotionDefinition(
        string definitionId,
        string actorId,
        Pose3d startPose,
        Pose3d destinationPose,
        double startTimeSeconds,
        double durationSeconds,
        double maximumLinearSpeed,
        double maximumAngularSpeed,
        int version = 1,
        PrescribedKinematicMotionProfile profile = PrescribedKinematicMotionProfile.ConstantVelocity,
        double maximumLinearAcceleration = double.MaxValue,
        double maximumAngularAcceleration = double.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("A stable motion definition identity is required.", nameof(definitionId));
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("A stable actor identity is required.", nameof(actorId));
        if (version <= 0)
            throw new ArgumentOutOfRangeException(nameof(version), "The definition version must be positive.");
        if (!startPose.IsValid || !destinationPose.IsValid)
            throw new ArgumentException("Start and destination poses must be valid.");
        if (!double.IsFinite(startTimeSeconds) || startTimeSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(startTimeSeconds));
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d ||
            !double.IsFinite(startTimeSeconds + durationSeconds))
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        ValidateLimit(maximumLinearSpeed, nameof(maximumLinearSpeed));
        ValidateLimit(maximumAngularSpeed, nameof(maximumAngularSpeed));
        ValidateLimit(maximumLinearAcceleration, nameof(maximumLinearAcceleration));
        ValidateLimit(maximumAngularAcceleration, nameof(maximumAngularAcceleration));
        if (!Enum.IsDefined(profile))
            throw new ArgumentOutOfRangeException(nameof(profile));

        DefinitionId = definitionId;
        ActorId = actorId;
        Version = version;
        StartPose = startPose;
        DestinationPose = destinationPose;
        StartTimeSeconds = startTimeSeconds;
        DurationSeconds = durationSeconds;
        MaximumLinearSpeed = maximumLinearSpeed;
        MaximumAngularSpeed = maximumAngularSpeed;
        Profile = profile;
        MaximumLinearAcceleration = maximumLinearAcceleration;
        MaximumAngularAcceleration = maximumAngularAcceleration;

        var start = startPose.Orientation.Normalized();
        var end = destinationPose.Orientation.Normalized();
        var dot = Dot(start, end);
        if (dot < 0d)
        {
            end = Negate(end);
            dot = -dot;
        }

        dot = Math.Clamp(dot, -1d, 1d);
        RotationAngle = 2d * Math.Acos(dot);
        var relative = end * start.Conjugate();
        var sineHalfAngle = Math.Sqrt(Math.Max(0d, 1d - (dot * dot)));
        RotationAxis = sineHalfAngle <= 1e-15
            ? Vector3d.Zero
            : new Vector3d(relative.X, relative.Y, relative.Z).Normalized();

        var peakSpeedFactor = profile == PrescribedKinematicMotionProfile.QuinticSmoothStep ? 1.875d : 1d;
        var requiredLinearSpeed = (Translation.Magnitude / durationSeconds) * peakSpeedFactor;
        var requiredAngularSpeed = (RotationAngle / durationSeconds) * peakSpeedFactor;
        if (!double.IsFinite(requiredLinearSpeed) || requiredLinearSpeed > maximumLinearSpeed)
            throw new ArgumentException("The trajectory exceeds the declared linear-speed limit.", nameof(maximumLinearSpeed));
        if (!double.IsFinite(requiredAngularSpeed) || requiredAngularSpeed > maximumAngularSpeed)
            throw new ArgumentException("The trajectory exceeds the declared angular-speed limit.", nameof(maximumAngularSpeed));

        if (profile == PrescribedKinematicMotionProfile.QuinticSmoothStep)
        {
            var peakAccelerationFactor = 10d / Math.Sqrt(3d);
            var requiredLinearAcceleration = (Translation.Magnitude / (durationSeconds * durationSeconds)) * peakAccelerationFactor;
            var requiredAngularAcceleration = (RotationAngle / (durationSeconds * durationSeconds)) * peakAccelerationFactor;
            if (!double.IsFinite(requiredLinearAcceleration) || requiredLinearAcceleration > maximumLinearAcceleration)
                throw new ArgumentException("The trajectory exceeds the declared linear-acceleration limit.", nameof(maximumLinearAcceleration));
            if (!double.IsFinite(requiredAngularAcceleration) || requiredAngularAcceleration > maximumAngularAcceleration)
                throw new ArgumentException("The trajectory exceeds the declared angular-acceleration limit.", nameof(maximumAngularAcceleration));
        }
    }

    private static double Dot(Quaterniond a, Quaterniond b) =>
        (a.W * b.W) + (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

    private static Quaterniond Negate(Quaterniond value) =>
        new(-value.W, -value.X, -value.Y, -value.Z);

    private static void ValidateLimit(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d)
            throw new ArgumentOutOfRangeException(parameterName, "Motion limits must be finite and non-negative.");
    }
}

/// <summary>Evaluates prescribed kinematic motion and validates separately supplied committed observations.</summary>
public static class PrescribedKinematicMotion
{
    /// <summary>
    /// Evaluates a definition at one absolute logical time and step.
    /// </summary>
    /// <remarks>
    /// The returned rigid-body snapshot is the prescribed kinematic sample. It is
    /// not automatically committed to a world or substituted for a dynamically
    /// achieved state. The caller owns the commit decision and must retain the
    /// previous committed snapshot if a motion is interrupted.
    /// </remarks>
    public static PrescribedKinematicMotionSample Evaluate(
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
            throw new ArgumentOutOfRangeException(nameof(logicalTimeSeconds),
                "Evaluation time must lie within the declared trajectory interval.");

        var elapsed = logicalTimeSeconds - definition.StartTimeSeconds;
        var fraction = elapsed / definition.DurationSeconds;
        var progress = definition.Profile == PrescribedKinematicMotionProfile.QuinticSmoothStep
            ? (10d * Math.Pow(fraction, 3d)) - (15d * Math.Pow(fraction, 4d)) + (6d * Math.Pow(fraction, 5d))
            : fraction;
        var progressRate = definition.Profile == PrescribedKinematicMotionProfile.QuinticSmoothStep
            ? 30d * fraction * fraction * (1d - fraction) * (1d - fraction)
            : 1d;
        var position = definition.StartPose.Position + (definition.Translation * progress);
        var linearVelocity = definition.Translation * (progressRate / definition.DurationSeconds);
        var orientation = InterpolateOrientation(
            definition.StartPose.Orientation,
            definition.DestinationPose.Orientation,
            progress);
        var angularVelocity = definition.RotationAxis * (definition.RotationAngle * progressRate / definition.DurationSeconds);

        var state = new RigidBodyState(
            definition.ActorId,
            new Pose3d(position, orientation),
            linearVelocity,
            angularVelocity,
            stepIndex,
            logicalTimeSeconds);

        return new PrescribedKinematicMotionSample(
            definition.DefinitionId, definition.Version, state, fraction == 1d);
    }

    /// <summary>
    /// Evaluates one step in a declared fixed-step schedule.
    /// </summary>
    /// <param name="definition">The immutable prescribed trajectory.</param>
    /// <param name="stepIndex">Zero-based logical step, including the destination step.</param>
    /// <param name="fixedTimeStepSeconds">Positive fixed logical timestep in seconds.</param>
    /// <param name="totalStepCount">Number of steps spanning the trajectory duration.</param>
    /// <remarks>
    /// The schedule must span the definition duration. The evaluator derives
    /// logical time from the step identity; callers cannot supply render-frame time.
    /// </remarks>
    public static PrescribedKinematicMotionSample EvaluateFixedStep(
        PrescribedKinematicMotionDefinition definition,
        long stepIndex,
        double fixedTimeStepSeconds,
        long totalStepCount)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!double.IsFinite(fixedTimeStepSeconds) || fixedTimeStepSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(fixedTimeStepSeconds));
        if (totalStepCount <= 0 || stepIndex < 0 || stepIndex > totalStepCount)
            throw new ArgumentOutOfRangeException(nameof(stepIndex));

        var scheduledDuration = fixedTimeStepSeconds * totalStepCount;
        var tolerance = 1e-12d * Math.Max(1d, definition.DurationSeconds);
        if (!double.IsFinite(scheduledDuration) ||
            Math.Abs(scheduledDuration - definition.DurationSeconds) > tolerance)
            throw new ArgumentException("The fixed-step schedule must span the motion definition duration.", nameof(totalStepCount));

        var time = stepIndex == totalStepCount
            ? definition.StartTimeSeconds + definition.DurationSeconds
            : definition.StartTimeSeconds + (stepIndex * fixedTimeStepSeconds);
        return Evaluate(definition, time, stepIndex);
    }

    /// <summary>
    /// Validates a host/world-committed state against the prescribed sample.
    /// </summary>
    /// <remarks>
    /// The achieved snapshot is supplied by the caller; this method never writes
    /// or fabricates achieved state. Completion is reported only when the
    /// committed snapshot is at the destination sample and lies within both
    /// declared tolerances. To cancel between steps, do not commit another
    /// sample: retain the last immutable committed RigidBodyState.
    /// </remarks>
    public static PrescribedKinematicMotionCommit Commit(
        PrescribedKinematicMotionDefinition definition,
        PrescribedKinematicMotionSample requested,
        RigidBodyState achieved,
        double positionToleranceMeters,
        double angularToleranceRadians)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!achieved.IsValid)
            throw new ArgumentException("A valid achieved state is required.", nameof(achieved));
        if (!double.IsFinite(positionToleranceMeters) || positionToleranceMeters < 0d)
            throw new ArgumentOutOfRangeException(nameof(positionToleranceMeters));
        if (!double.IsFinite(angularToleranceRadians) || angularToleranceRadians < 0d)
            throw new ArgumentOutOfRangeException(nameof(angularToleranceRadians));
        if (requested.DefinitionId != definition.DefinitionId ||
            requested.DefinitionVersion != definition.Version ||
            requested.State.BodyId != definition.ActorId ||
            requested.State.StepIndex != achieved.StepIndex ||
            requested.State.ElapsedTime != achieved.ElapsedTime ||
            achieved.BodyId != definition.ActorId)
            throw new ArgumentException("Requested and achieved observations must identify the same actor, step, and logical time.", nameof(achieved));

        var expected = Evaluate(definition, achieved.ElapsedTime, achieved.StepIndex);
        if (requested != expected)
            throw new ArgumentException("The requested sample does not match the supplied definition, step, and time.", nameof(requested));

        var positionError = Vector3d.Distance(requested.State.Pose.Position, achieved.Pose.Position);
        var a = requested.State.Pose.Orientation.Normalized();
        var b = achieved.Pose.Orientation.Normalized();
        var dot = Math.Abs((a.W * b.W) + (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z));
        var angularError = 2d * Math.Acos(Math.Clamp(dot, -1d, 1d));
        var arrived = requested.IsComplete &&
            positionError <= positionToleranceMeters &&
            angularError <= angularToleranceRadians;

        return new PrescribedKinematicMotionCommit(
            requested,
            achieved,
            positionError,
            angularError,
            arrived);
    }

    private static Quaterniond InterpolateOrientation(Quaterniond start, Quaterniond end, double amount)
    {
        start = start.Normalized();
        end = end.Normalized();
        var dot = (start.W * end.W) + (start.X * end.X) + (start.Y * end.Y) + (start.Z * end.Z);
        if (dot < 0d)
        {
            end = new Quaterniond(-end.W, -end.X, -end.Y, -end.Z);
            dot = -dot;
        }

        dot = Math.Clamp(dot, -1d, 1d);
        if (dot > 0.9999995d)
        {
            return new Quaterniond(
                start.W + ((end.W - start.W) * amount),
                start.X + ((end.X - start.X) * amount),
                start.Y + ((end.Y - start.Y) * amount),
                start.Z + ((end.Z - start.Z) * amount)).Normalized();
        }

        var angle = Math.Acos(dot);
        var denominator = Math.Sin(angle);
        var a = Math.Sin((1d - amount) * angle) / denominator;
        var b = Math.Sin(amount * angle) / denominator;
        return new Quaterniond(
            (a * start.W) + (b * end.W),
            (a * start.X) + (b * end.X),
            (a * start.Y) + (b * end.Y),
            (a * start.Z) + (b * end.Z)).Normalized();
    }
}

/// <summary>One immutable prescribed state sample and its destination-boundary flag.</summary>
/// <param name="DefinitionId">Stable identity of the motion definition.</param>
/// <param name="DefinitionVersion">Version of the motion definition.</param>
/// <param name="State">The state calculated by the prescribed trajectory.</param>
/// <param name="IsComplete">True only when the sample is exactly at the destination time.</param>
public readonly record struct PrescribedKinematicMotionSample(
    string DefinitionId,
    int DefinitionVersion,
    RigidBodyState State,
    bool IsComplete);


/// <summary>Pairs a requested trajectory sample with the separately supplied achieved state.</summary>
/// <param name="Requested">The immutable requested trajectory sample.</param>
/// <param name="Achieved">The independently supplied committed state.</param>
/// <param name="PositionErrorMeters">Position error between requested and achieved states.</param>
/// <param name="AngularErrorRadians">Shortest orientation error in radians.</param>
/// <param name="IsArrived">Whether the committed state is within destination tolerances.</param>
public readonly record struct PrescribedKinematicMotionCommit(
    PrescribedKinematicMotionSample Requested,
    RigidBodyState Achieved,
    double PositionErrorMeters,
    double AngularErrorRadians,
    bool IsArrived);
