using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>
/// Immutable three-dimensional projectile state under a declared coordinate frame.
/// Position is in metres, velocity in metres per second, and elapsed time in seconds.
/// </summary>
/// <param name="Position">World-space position in metres.</param>
/// <param name="Velocity">World-space velocity in metres per second.</param>
/// <param name="ElapsedTime">Elapsed logical time in seconds.</param>
public readonly record struct ProjectileState(
    Vector3d Position,
    Vector3d Velocity,
    double ElapsedTime)
{
    /// <summary>Returns true when the elapsed time is finite and non-negative.</summary>
    public bool IsValid => double.IsFinite(ElapsedTime) && ElapsedTime >= 0d;
}

/// <summary>
/// Advances a point projectile under constant world-space acceleration using analytical
/// kinematics. This is not a rigid-body integrator and does not model drag, spin, shape,
/// collisions, or a launch mechanism.
/// </summary>
public static class ConstantAccelerationProjectileSolver
{
    /// <summary>
    /// Advances the projectile by a positive logical timestep.
    /// </summary>
    /// <param name="state">Initial position, velocity and elapsed time.</param>
    /// <param name="acceleration">Constant acceleration in metres per second squared.</param>
    /// <param name="deltaTime">Positive timestep in seconds.</param>
    /// <returns>The analytical state at the end of the timestep.</returns>
    /// <exception cref="ArgumentException">The initial state or acceleration is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timestep is not positive and finite.</exception>
    /// <exception cref="OverflowException">The calculated state is outside finite double range.</exception>
    public static ProjectileState Advance(
        in ProjectileState state,
        in Vector3d acceleration,
        double deltaTime)
    {
        if (!state.IsValid)
            throw new ArgumentException("The initial projectile state must have finite, non-negative elapsed time.", nameof(state));

        if (!double.IsFinite(deltaTime) || deltaTime <= 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "The logical timestep must be positive and finite.");

        var nextElapsedTime = state.ElapsedTime + deltaTime;
        if (!double.IsFinite(nextElapsedTime))
            throw new OverflowException("The calculated elapsed time is outside the finite double range.");

        try
        {
            var nextPosition = state.Position +
                (state.Velocity * deltaTime) +
                (acceleration * (0.5d * deltaTime * deltaTime));
            var nextVelocity = state.Velocity + (acceleration * deltaTime);
            return new ProjectileState(nextPosition, nextVelocity, nextElapsedTime);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new OverflowException("The calculated projectile state is outside the finite double range.", exception);
        }
    }
}
