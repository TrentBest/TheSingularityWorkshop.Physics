namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>Advances an ideal planar pendulum using deterministic fourth-order Runge-Kutta integration.</summary>
/// <remarks>
/// The governing nonlinear equation is θ'' = -(g/L) sin(θ). Each call advances
/// one fixed positive timestep. The method models a point mass on a massless,
/// inextensible link with a fixed pivot in a uniform gravitational field.
/// </remarks>
public static class PlanarPendulumStepper
{
    /// <summary>Advances one valid pendulum state by one RK4 timestep.</summary>
    /// <param name="definition">Physical pendulum parameters.</param>
    /// <param name="state">State at the beginning of the step.</param>
    /// <param name="deltaTimeSeconds">Positive finite timestep in seconds.</param>
    /// <returns>The next committed pendulum state.</returns>
    /// <exception cref="ArgumentNullException">The definition is null.</exception>
    /// <exception cref="ArgumentException">The state is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timestep is not positive and finite.</exception>
    /// <exception cref="OverflowException">The step index or a derived result exceeds its finite range.</exception>
    public static PlanarPendulumState Advance(
        PlanarPendulumDefinition definition,
        PlanarPendulumState state,
        double deltaTimeSeconds)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        if (!state.IsValid)
            throw new ArgumentException("A valid pendulum state is required.", nameof(state));
        if (!double.IsFinite(deltaTimeSeconds) || deltaTimeSeconds <= 0d)
            throw new ArgumentOutOfRangeException(
                nameof(deltaTimeSeconds),
                "The timestep must be finite and positive.");

        var nextStep = checked(state.StepIndex + 1);
        var nextTime = state.ElapsedTimeSeconds + deltaTimeSeconds;
        if (!double.IsFinite(nextTime))
            throw new OverflowException("Advancing pendulum time produced a non-finite result.");

        var accelerationScale =
            definition.GravitationalAccelerationMetresPerSecondSquared / definition.LengthMetres;

        // RK4 integrates the coupled first-order system:
        // θ' = ω, ω' = -(g/L) sin(θ).
        var k1Angle = state.AngularVelocityRadiansPerSecond;
        var k1Velocity = Acceleration(state.AngleRadians, accelerationScale);

        var k2Angle = state.AngularVelocityRadiansPerSecond + (0.5d * deltaTimeSeconds * k1Velocity);
        var k2Velocity = Acceleration(
            state.AngleRadians + (0.5d * deltaTimeSeconds * k1Angle),
            accelerationScale);

        var k3Angle = state.AngularVelocityRadiansPerSecond + (0.5d * deltaTimeSeconds * k2Velocity);
        var k3Velocity = Acceleration(
            state.AngleRadians + (0.5d * deltaTimeSeconds * k2Angle),
            accelerationScale);

        var k4Angle = state.AngularVelocityRadiansPerSecond + (deltaTimeSeconds * k3Velocity);
        var k4Velocity = Acceleration(
            state.AngleRadians + (deltaTimeSeconds * k3Angle),
            accelerationScale);

        var nextAngle = state.AngleRadians + (deltaTimeSeconds / 6d) *
            (k1Angle + (2d * k2Angle) + (2d * k3Angle) + k4Angle);
        var nextVelocity = state.AngularVelocityRadiansPerSecond + (deltaTimeSeconds / 6d) *
            (k1Velocity + (2d * k2Velocity) + (2d * k3Velocity) + k4Velocity);

        if (!double.IsFinite(accelerationScale) ||
            !double.IsFinite(nextAngle) ||
            !double.IsFinite(nextVelocity))
            throw new OverflowException("The pendulum step produced a value outside finite double range.");

        return new PlanarPendulumState(nextAngle, nextVelocity, nextStep, nextTime);
    }

    /// <summary>Calculates gravitational potential energy relative to the lowest point, in joules.</summary>
    public static double PotentialEnergyJoules(
        PlanarPendulumDefinition definition,
        PlanarPendulumState state)
    {
        ValidateInputs(definition, state);
        var energy = definition.MassKilograms *
            definition.GravitationalAccelerationMetresPerSecondSquared *
            definition.LengthMetres *
            (1d - Math.Cos(state.AngleRadians));
        return EnsureFiniteEnergy(energy);
    }

    /// <summary>Calculates kinetic energy of the point mass, in joules.</summary>
    public static double KineticEnergyJoules(
        PlanarPendulumDefinition definition,
        PlanarPendulumState state)
    {
        ValidateInputs(definition, state);
        var inertia = definition.MassKilograms * definition.LengthMetres * definition.LengthMetres;
        var energy = 0.5d * inertia *
            state.AngularVelocityRadiansPerSecond *
            state.AngularVelocityRadiansPerSecond;
        return EnsureFiniteEnergy(energy);
    }

    /// <summary>Calculates total mechanical energy in joules for the ideal undamped pendulum.</summary>
    public static double TotalEnergyJoules(
        PlanarPendulumDefinition definition,
        PlanarPendulumState state)
    {
        var total = KineticEnergyJoules(definition, state) +
            PotentialEnergyJoules(definition, state);
        return EnsureFiniteEnergy(total);
    }

    private static double Acceleration(double angleRadians, double accelerationScale) =>
        -accelerationScale * Math.Sin(angleRadians);

    private static void ValidateInputs(
        PlanarPendulumDefinition definition,
        PlanarPendulumState state)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();
        if (!state.IsValid)
            throw new ArgumentException("A valid pendulum state is required.", nameof(state));
    }

    private static double EnsureFiniteEnergy(double energy)
    {
        if (!double.IsFinite(energy))
            throw new OverflowException("Pendulum energy is outside the finite double range.");
        return energy;
    }
}
