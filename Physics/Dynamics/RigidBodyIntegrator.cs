using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Dynamics;

/// <summary>Advances one free rigid-body state through a single fixed-duration step.</summary>
/// <remarks>
/// This is a deterministic, semi-implicit Euler reference integrator for unconstrained
/// motion. Linear acceleration is supplied in world coordinates and is constant over
/// the step. Angular velocity is held constant over the step and integrated as a
/// world-frame rotation. The method does not apply forces, calculate acceleration,
/// detect collisions, resolve contacts, model friction, or enforce constraints.
/// Callers must perform those operations in an explicitly documented order.
/// </remarks>
public static class RigidBodyIntegrator
{
    /// <summary>Advances a valid body snapshot by one positive-duration logical step.</summary>
    /// <param name="state">The achieved state at the beginning of the step.</param>
    /// <param name="linearAcceleration">Constant world-frame linear acceleration in metres per second squared.</param>
    /// <param name="deltaTimeSeconds">Positive fixed logical timestep in seconds.</param>
    /// <returns>The body snapshot at the next step and elapsed logical time.</returns>
    /// <exception cref="ArgumentException">The supplied snapshot is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timestep is not finite and positive.</exception>
    /// <exception cref="OverflowException">The step index cannot be incremented.</exception>
    public static RigidBodyState Advance(
        RigidBodyState state,
        Vector3d linearAcceleration,
        double deltaTimeSeconds)
    {
        if (!state.IsValid)
            throw new ArgumentException("A valid rigid-body state is required.", nameof(state));

        if (!double.IsFinite(deltaTimeSeconds) || deltaTimeSeconds <= 0d)
            throw new ArgumentOutOfRangeException(
                nameof(deltaTimeSeconds),
                "The logical timestep must be finite and positive.");

        var nextStep = checked(state.StepIndex + 1);
        var nextTime = state.ElapsedTime + deltaTimeSeconds;
        if (!double.IsFinite(nextTime))
            throw new ArithmeticException("Advancing logical time produced a non-finite result.");

        // Semi-implicit Euler: update velocity first, then advance position using
        // that achieved velocity. This choice is part of the numerical contract.
        var nextVelocity = state.LinearVelocity + (linearAcceleration * deltaTimeSeconds);
        var nextPosition = state.Pose.Position + (nextVelocity * deltaTimeSeconds);
        var nextOrientation = IntegrateWorldAngularVelocity(
            state.Pose.Orientation,
            state.AngularVelocity,
            deltaTimeSeconds);

        return new RigidBodyState(
            state.BodyId,
            new Pose3d(nextPosition, nextOrientation),
            nextVelocity,
            state.AngularVelocity,
            nextStep,
            nextTime);
    }

    private static Quaterniond IntegrateWorldAngularVelocity(
        Quaterniond orientation,
        Vector3d angularVelocity,
        double deltaTimeSeconds)
    {
        var speed = angularVelocity.Magnitude;
        if (speed == 0d)
            return orientation;

        var angle = speed * deltaTimeSeconds;
        if (!double.IsFinite(angle))
            throw new ArithmeticException("Angular displacement is outside the finite numeric range.");

        var halfAngle = angle * 0.5d;
        var axis = angularVelocity / speed;
        var sine = Math.Sin(halfAngle);
        var delta = new Quaterniond(
            Math.Cos(halfAngle),
            axis.X * sine,
            axis.Y * sine,
            axis.Z * sine);

        // Angular velocity is world-frame, so the incremental world rotation
        // composes on the left of the existing body-local-to-world orientation.
        return (delta * orientation).Normalized();
    }
}
