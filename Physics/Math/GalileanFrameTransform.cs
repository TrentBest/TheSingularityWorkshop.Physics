using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Math;

/// <summary>
/// Galilean state transforms between inertial frames with constant relative velocity.
/// </summary>
/// <remarks>
/// Positions use metres, velocities metres per second, and elapsed time seconds. The
/// supplied origin is the world-frame position of the moving frame's origin at t = 0.
/// This is a classical, non-relativistic coordinate transform; it does not model forces
/// or prove that either frame is inertial. All frames must share axis orientation.
/// </remarks>
public static class GalileanFrameTransform
{
    /// <summary>
    /// Converts a state measured in a frame moving at constant velocity into the
    /// parallel-axis world frame.
    /// </summary>
    /// <param name="stateInMovingFrame">Position and velocity measured in the moving frame.</param>
    /// <param name="frameVelocityInWorld">Constant velocity of the moving frame relative to world, in m/s.</param>
    /// <param name="frameOriginAtEpoch">World position of the moving frame origin at t = 0, in metres.</param>
    public static ProjectileState ToWorld(
        ProjectileState stateInMovingFrame,
        Vector3d frameVelocityInWorld,
        Vector3d frameOriginAtEpoch)
    {
        ValidateState(stateInMovingFrame);

        var worldPosition = frameOriginAtEpoch
            + stateInMovingFrame.Position
            + (frameVelocityInWorld * stateInMovingFrame.ElapsedTime);
        var worldVelocity = stateInMovingFrame.Velocity + frameVelocityInWorld;

        return new ProjectileState(worldPosition, worldVelocity, stateInMovingFrame.ElapsedTime);
    }

    /// <summary>Converts a world-frame state into a parallel-axis frame moving at constant velocity.</summary>
    public static ProjectileState ToMovingFrame(
        ProjectileState stateInWorld,
        Vector3d frameVelocityInWorld,
        Vector3d frameOriginAtEpoch)
    {
        ValidateState(stateInWorld);

        var movingPosition = stateInWorld.Position
            - frameOriginAtEpoch
            - (frameVelocityInWorld * stateInWorld.ElapsedTime);
        var movingVelocity = stateInWorld.Velocity - frameVelocityInWorld;

        return new ProjectileState(movingPosition, movingVelocity, stateInWorld.ElapsedTime);
    }

    private static void ValidateState(ProjectileState state)
    {
        if (!state.IsValid)
            throw new ArgumentException("State must contain finite position, velocity, and non-negative elapsed time.", nameof(state));
    }
}
