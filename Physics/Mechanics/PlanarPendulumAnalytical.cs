namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>
/// Evaluates the small-angle analytical approximation for an ideal planar pendulum.
/// </summary>
/// <remarks>
/// Linearizing sin(θ) ≈ θ gives θ'' + (g/L)θ = 0. This is an analytical
/// reference model, not the exact finite-amplitude solution of a nonlinear pendulum.
/// Time is measured from the supplied initial conditions.
/// </remarks>
public static class PlanarPendulumAnalytical
{
    /// <summary>Calculates the small-angle natural angular frequency in radians per second.</summary>
    public static double NaturalAngularFrequencyRadiansPerSecond(
        PlanarPendulumDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        var frequency = Math.Sqrt(
            definition.GravitationalAccelerationMetresPerSecondSquared /
            definition.LengthMetres);

        if (!double.IsFinite(frequency) || frequency <= 0d)
            throw new OverflowException("The pendulum natural angular frequency is outside the finite double range.");

        return frequency;
    }

    /// <summary>Calculates the small-angle natural period in seconds.</summary>
    public static double SmallAnglePeriodSeconds(PlanarPendulumDefinition definition)
    {
        var period = 2d * Math.PI /
            NaturalAngularFrequencyRadiansPerSecond(definition);

        if (!double.IsFinite(period) || period <= 0d)
            throw new OverflowException("The pendulum small-angle period is outside the finite double range.");

        return period;
    }

    /// <summary>
    /// Evaluates angle and angular velocity at a non-negative time from the initial conditions.
    /// </summary>
    /// <param name="definition">Physical pendulum parameters.</param>
    /// <param name="initialAngleRadians">Initial angular displacement from the downward vertical.</param>
    /// <param name="initialAngularVelocityRadiansPerSecond">Initial angular velocity.</param>
    /// <param name="elapsedTimeSeconds">Non-negative elapsed time from the initial conditions.</param>
    /// <returns>The analytical angle and angular velocity at the requested time.</returns>
    public static PlanarPendulumAnalyticalState Evaluate(
        PlanarPendulumDefinition definition,
        double initialAngleRadians,
        double initialAngularVelocityRadiansPerSecond,
        double elapsedTimeSeconds)
    {
        var angularFrequency = NaturalAngularFrequencyRadiansPerSecond(definition);

        if (!double.IsFinite(initialAngleRadians))
            throw new ArgumentOutOfRangeException(nameof(initialAngleRadians), "Initial angle must be finite.");
        if (!double.IsFinite(initialAngularVelocityRadiansPerSecond))
            throw new ArgumentOutOfRangeException(
                nameof(initialAngularVelocityRadiansPerSecond),
                "Initial angular velocity must be finite.");
        if (!double.IsFinite(elapsedTimeSeconds) || elapsedTimeSeconds < 0d)
            throw new ArgumentOutOfRangeException(
                nameof(elapsedTimeSeconds),
                "Elapsed time must be finite and non-negative.");

        var phase = angularFrequency * elapsedTimeSeconds;
        if (!double.IsFinite(phase))
            throw new OverflowException("The analytical pendulum phase is outside the finite double range.");

        var cosine = Math.Cos(phase);
        var sine = Math.Sin(phase);
        var angle = initialAngleRadians * cosine +
            (initialAngularVelocityRadiansPerSecond / angularFrequency) * sine;
        var angularVelocity =
            -initialAngleRadians * angularFrequency * sine +
            initialAngularVelocityRadiansPerSecond * cosine;

        if (!double.IsFinite(angle) || !double.IsFinite(angularVelocity))
            throw new OverflowException("The analytical pendulum state is outside the finite double range.");

        return new PlanarPendulumAnalyticalState(angle, angularVelocity);
    }
}

/// <summary>An analytical pendulum observation; it is not a committed integration step.</summary>
public readonly record struct PlanarPendulumAnalyticalState(
    double AngleRadians,
    double AngularVelocityRadiansPerSecond);
