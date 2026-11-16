using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>
/// Immutable definition of a bounded, prescribed root-pose trajectory.
/// </summary>
/// <remarks>
/// This is kinematic path evaluation, not force-driven dynamics. The path uses
/// cubic smoothstep interpolation in translation and shortest-arc quaternion
/// interpolation in orientation. It is evaluated at integer logical steps;
/// render cadence is not an input. Position is in right-handed world-frame
/// metres, orientation maps body-local directions into world directions, time
/// is in seconds, and angular quantities are in radians.
/// The trajectory has zero endpoint velocity and continuous velocity, but
/// acceleration may jump at its endpoints. No collision, contact, balance,
/// gait, or obstacle response is implied. A host must commit an evaluated
/// sample explicitly; evaluating a target does not mutate a RigidBodyState.
/// </remarks>
public sealed class PrescribedKinematicMotion
{
    private readonly Vector3d _translation;
    private readonly Quaterniond _endOrientation;
    private readonly Vector3d _rotationAxis;
    private readonly double _rotationAngle;
    private readonly double _duration;

    /// <summary>Gets the stable actor/body identity.</summary>
    public string ActorId { get; }

    /// <summary>Gets the stable identity of this motion definition.</summary>
    public string DefinitionId { get; }

    /// <summary>Gets the positive definition version.</summary>
    public int Version { get; }

    /// <summary>Gets the prescribed start pose.</summary>
    public Pose3d StartPose { get; }

    /// <summary>Gets the prescribed destination pose.</summary>
    public Pose3d DestinationPose { get; }

    /// <summary>Gets the first logical time in seconds.</summary>
    public double StartTimeSeconds { get; }

    /// <summary>Gets the fixed logical timestep in seconds.</summary>
    public double FixedTimeStepSeconds { get; }

    /// <summary>Gets the number of fixed steps in the trajectory.</summary>
    public long StepCount { get; }

    /// <summary>Gets the declared maximum permitted linear speed in metres per second.</summary>
    public double MaximumLinearSpeed { get; }

    /// <summary>Gets the declared maximum permitted angular speed in radians per second.</summary>
    public double MaximumAngularSpeed { get; }

    /// <summary>Gets the declared maximum permitted linear acceleration in metres per second squared.</summary>
    public double MaximumLinearAcceleration { get; }

    /// <summary>Gets the declared maximum permitted angular acceleration in radians per second squared.</summary>
    public double MaximumAngularAcceleration { get; }

    /// <summary>Gets the maximum speed required by this trajectory.</summary>
    public double RequiredMaximumLinearSpeed { get; }

    /// <summary>Gets the maximum angular speed required by this trajectory.</summary>
    public double RequiredMaximumAngularSpeed { get; }

    /// <summary>Gets the maximum acceleration magnitude required by this trajectory.</summary>
    public double RequiredMaximumLinearAcceleration { get; }

    /// <summary>Gets the maximum angular acceleration magnitude required by this trajectory.</summary>
    public double RequiredMaximumAngularAcceleration { get; }

    /// <summary>Creates and validates a fixed-step prescribed trajectory.</summary>
    public PrescribedKinematicMotion(
        string actorId,
        string definitionId,
        int version,
        Pose3d startPose,
        Pose3d destinationPose,
        double startTimeSeconds,
        double fixedTimeStepSeconds,
        long stepCount,
        double maximumLinearSpeed,
        double maximumAngularSpeed,
        double maximumLinearAcceleration,
        double maximumAngularAcceleration)
    {
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("A stable actor identity is required.", nameof(actorId));
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("A stable motion definition identity is required.", nameof(definitionId));
        if (version <= 0)
            throw new ArgumentOutOfRangeException(nameof(version), "The definition version must be positive.");
        if (!startPose.IsValid || !destinationPose.IsValid)
            throw new ArgumentException("Start and destination poses must be valid.");
        if (!double.IsFinite(startTimeSeconds) || startTimeSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(startTimeSeconds));
        if (!double.IsFinite(fixedTimeStepSeconds) || fixedTimeStepSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(fixedTimeStepSeconds));
        if (stepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(stepCount), "At least one logical step is required.");

        ValidateLimit(maximumLinearSpeed, nameof(maximumLinearSpeed));
        ValidateLimit(maximumAngularSpeed, nameof(maximumAngularSpeed));
        ValidateLimit(maximumLinearAcceleration, nameof(maximumLinearAcceleration));
        ValidateLimit(maximumAngularAcceleration, nameof(maximumAngularAcceleration));

        _duration = stepCount * fixedTimeStepSeconds;
        if (!double.IsFinite(_duration) || !double.IsFinite(startTimeSeconds + _duration))
            throw new ArgumentOutOfRangeException(nameof(stepCount), "The trajectory end time must be finite.");

        ActorId = actorId;
        DefinitionId = definitionId;
        Version = version;
        StartPose = startPose;
        DestinationPose = destinationPose;
        StartTimeSeconds = startTimeSeconds;
        FixedTimeStepSeconds = fixedTimeStepSeconds;
        StepCount = stepCount;
        MaximumLinearSpeed = maximumLinearSpeed;
        MaximumAngularSpeed = maximumAngularSpeed;
        MaximumLinearAcceleration = maximumLinearAcceleration;
        MaximumAngularAcceleration = maximumAngularAcceleration;

        _translation = destinationPose.Position - startPose.Position;

        var start = startPose.Orientation.Normalized();
        var end = destinationPose.Orientation.Normalized();
        var dot = Dot(start, end);
        if (dot < 0d)
        {
            end = Negate(end);
            dot = -dot;
        }

        dot = Math.Clamp(dot, -1d, 1d);
        _endOrientation = end;
        _rotationAngle = 2d * Math.Acos(dot);

        var relative = end * start.Conjugate();
        var sineHalfAngle = Math.Sqrt(Math.Max(0d, 1d - (dot * dot)));
        _rotationAxis = sineHalfAngle <= 1e-15
            ? Vector3d.Zero
            : new Vector3d(relative.X, relative.Y, relative.Z).Normalized();

        RequiredMaximumLinearSpeed = 1.5d * _translation.Magnitude / _duration;
        RequiredMaximumLinearAcceleration = 6d * _translation.Magnitude / (_duration * _duration);
        RequiredMaximumAngularSpeed = 1.5d * _rotationAngle / _duration;
        RequiredMaximumAngularAcceleration = 6d * _rotationAngle / (_duration * _duration);

        EnsureWithinLimit(RequiredMaximumLinearSpeed, maximumLinearSpeed, nameof(maximumLinearSpeed));
        EnsureWithinLimit(RequiredMaximumLinearAcceleration, maximumLinearAcceleration, nameof(maximumLinearAcceleration));
        EnsureWithinLimit(RequiredMaximumAngularSpeed, maximumAngularSpeed, nameof(maximumAngularSpeed));
        EnsureWithinLimit(RequiredMaximumAngularAcceleration, maximumAngularAcceleration, nameof(maximumAngularAcceleration));
    }

    /// <summary>Evaluates the prescribed pose and its analytic derivatives at one logical step.</summary>
    /// <param name="stepIndex">Zero-based step index in the inclusive range [0, StepCount].</param>
    public PrescribedKinematicMotionSample Evaluate(long stepIndex)
    {
        if (stepIndex < 0 || stepIndex > StepCount)
            throw new ArgumentOutOfRangeException(nameof(stepIndex));

        var elapsed = stepIndex * FixedTimeStepSeconds;
        var normalizedTime = elapsed / _duration;
        var blend = normalizedTime * normalizedTime * (3d - (2d * normalizedTime));
        var blendRate = 6d * normalizedTime * (1d - normalizedTime) / _duration;
        var blendAcceleration = 6d * (1d - (2d * normalizedTime)) / (_duration * _duration);

        var position = StartPose.Position + (_translation * blend);
        var linearVelocity = _translation * blendRate;
        var linearAcceleration = _translation * blendAcceleration;

        var orientation = InterpolateOrientation(StartPose.Orientation, _endOrientation, blend);
        var angularVelocity = _rotationAxis * (_rotationAngle * blendRate);
        var angularAcceleration = _rotationAxis * (_rotationAngle * blendAcceleration);

        return new PrescribedKinematicMotionSample(
            ActorId,
            DefinitionId,
            Version,
            stepIndex,
            StartTimeSeconds + elapsed,
            new Pose3d(position, orientation),
            linearVelocity,
            angularVelocity,
            linearAcceleration,
            angularAcceleration,
            stepIndex == StepCount);
    }

    private static Quaterniond InterpolateOrientation(Quaterniond start, Quaterniond end, double amount)
    {
        var dot = Dot(start, end);
        if (dot > 0.9999995d)
        {
            return new Quaterniond(
                start.W + ((end.W - start.W) * amount),
                start.X + ((end.X - start.X) * amount),
                start.Y + ((end.Y - start.Y) * amount),
                start.Z + ((end.Z - start.Z) * amount)).Normalized();
        }

        var angle = Math.Acos(Math.Clamp(dot, -1d, 1d));
        var denominator = Math.Sin(angle);
        var a = Math.Sin((1d - amount) * angle) / denominator;
        var b = Math.Sin(amount * angle) / denominator;
        return new Quaterniond(
            (a * start.W) + (b * end.W),
            (a * start.X) + (b * end.X),
            (a * start.Y) + (b * end.Y),
            (a * start.Z) + (b * end.Z)).Normalized();
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

    private static void EnsureWithinLimit(double required, double declared, string parameterName)
    {
        if (!double.IsFinite(required) || required > declared)
            throw new ArgumentException(
                $"The prescribed trajectory requires {required:R}, exceeding the declared limit {declared:R}.",
                parameterName);
    }
}

/// <summary>
/// Immutable analytic sample from a prescribed trajectory.
/// </summary>
/// <remarks>
/// This sample is a requested kinematic observation, not proof that a world
/// solver committed or achieved the pose. A host must keep this separate from
/// <see cref="Dynamics.RigidBodyState"/> until it explicitly commits the result.
/// Acceleration is analytically evaluated from the declared trajectory, never
/// inferred from render-frame samples.
/// </remarks>
public readonly record struct PrescribedKinematicMotionSample(
    string ActorId,
    string DefinitionId,
    int DefinitionVersion,
    long StepIndex,
    double LogicalTimeSeconds,
    Pose3d PrescribedPose,
    Vector3d PrescribedLinearVelocity,
    Vector3d PrescribedAngularVelocity,
    Vector3d PrescribedLinearAcceleration,
    Vector3d PrescribedAngularAcceleration,
    bool IsDestinationStep);
