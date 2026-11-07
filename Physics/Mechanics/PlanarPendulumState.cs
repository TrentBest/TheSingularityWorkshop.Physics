namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>An immutable planar pendulum state at one committed logical step.</summary>
/// <remarks>
/// Angle is measured in radians from the downward vertical, positive in the
/// chosen direction of rotation. Angular velocity is radians per second.
/// </remarks>
public readonly record struct PlanarPendulumState
{
    /// <summary>Gets the angular displacement from the downward vertical in radians.</summary>
    private readonly bool _isInitialized;

    public double AngleRadians { get; }

    /// <summary>Gets angular velocity in radians per second.</summary>
    public double AngularVelocityRadiansPerSecond { get; }

    /// <summary>Gets the zero-based committed logical step.</summary>
    public long StepIndex { get; }

    /// <summary>Gets elapsed logical simulation time in seconds.</summary>
    public double ElapsedTimeSeconds { get; }

    /// <summary>Gets whether all state fields are finite and the step/time are non-negative.</summary>
    public bool IsValid =>
        _isInitialized &&
        double.IsFinite(AngleRadians) &&
        double.IsFinite(AngularVelocityRadiansPerSecond) &&
        StepIndex >= 0 &&
        double.IsFinite(ElapsedTimeSeconds) &&
        ElapsedTimeSeconds >= 0d;

    /// <summary>Creates a validated pendulum state snapshot.</summary>
    public PlanarPendulumState(
        double angleRadians,
        double angularVelocityRadiansPerSecond,
        long stepIndex,
        double elapsedTimeSeconds)
    {
        if (!double.IsFinite(angleRadians))
            throw new ArgumentOutOfRangeException(nameof(angleRadians), "Angle must be finite.");
        if (!double.IsFinite(angularVelocityRadiansPerSecond))
            throw new ArgumentOutOfRangeException(nameof(angularVelocityRadiansPerSecond), "Angular velocity must be finite.");
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex), "Step index must be non-negative.");
        if (!double.IsFinite(elapsedTimeSeconds) || elapsedTimeSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedTimeSeconds), "Elapsed time must be finite and non-negative.");

        AngleRadians = angleRadians;
        AngularVelocityRadiansPerSecond = angularVelocityRadiansPerSecond;
        StepIndex = stepIndex;
        ElapsedTimeSeconds = elapsedTimeSeconds;
        _isInitialized = true;
    }
}
