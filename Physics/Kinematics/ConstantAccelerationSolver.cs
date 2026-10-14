namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>
/// Advances one-dimensional motion under constant acceleration using the exact
/// kinematic equations for the declared constant-acceleration model.
/// This is an analytical model, not a general-purpose numerical integrator.
/// </summary>
public static class ConstantAccelerationSolver
{
    /// <summary>
    /// Advances a state by a positive logical timestep under constant acceleration.
    /// All values use SI units: metres, metres per second, metres per second squared,
    /// and seconds.
    /// </summary>
    /// <param name="state">The initial state snapshot.</param>
    /// <param name="acceleration">Constant acceleration in metres per second squared.</param>
    /// <param name="deltaTime">Positive logical timestep in seconds.</param>
    /// <returns>The state at the end of the timestep.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The timestep is not positive and finite.</exception>
    /// <exception cref="ArgumentException">The initial state or acceleration is invalid.</exception>
    /// <exception cref="OverflowException">The calculated state is outside the finite double range.</exception>
    public static KinematicState Advance(
        in KinematicState state,
        double acceleration,
        double deltaTime)
    {
        if (!state.IsValid)
        {
            throw new ArgumentException("The initial kinematic state must be finite and have non-negative elapsed time.", nameof(state));
        }

        if (!double.IsFinite(acceleration))
        {
            throw new ArgumentException("Acceleration must be finite.", nameof(acceleration));
        }

        if (!double.IsFinite(deltaTime) || deltaTime <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "The logical timestep must be positive and finite.");
        }

        var nextPosition = state.Position + (state.Velocity * deltaTime) +
            (0.5d * acceleration * deltaTime * deltaTime);
        var nextVelocity = state.Velocity + (acceleration * deltaTime);
        var nextElapsedTime = state.ElapsedTime + deltaTime;

        if (!double.IsFinite(nextPosition) ||
            !double.IsFinite(nextVelocity) ||
            !double.IsFinite(nextElapsedTime))
        {
            throw new OverflowException("The calculated kinematic state is outside the finite double range.");
        }

        return new KinematicState(nextPosition, nextVelocity, nextElapsedTime);
    }
}
