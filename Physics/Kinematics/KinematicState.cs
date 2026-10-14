namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>
/// One-dimensional kinematic state using SI base units: metres, metres per second,
/// and seconds. The state is immutable so each solver step produces a new snapshot.
/// </summary>
/// <param name="Position">Position in metres.</param>
/// <param name="Velocity">Velocity in metres per second.</param>
/// <param name="ElapsedTime">Elapsed logical simulation time in seconds.</param>
public readonly record struct KinematicState(
    double Position,
    double Velocity,
    double ElapsedTime)
{
    /// <summary>Returns true when every state component is finite and elapsed time is non-negative.</summary>
    public bool IsValid =>
        double.IsFinite(Position) &&
        double.IsFinite(Velocity) &&
        double.IsFinite(ElapsedTime) &&
        ElapsedTime >= 0d;
}
