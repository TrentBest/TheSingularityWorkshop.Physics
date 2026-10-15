using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Dynamics;

/// <summary>
/// An immutable rigid-body state snapshot at one committed logical simulation step.
/// </summary>
/// <remarks>
/// Pose position is in world-frame metres; linear velocity is in world-frame metres
/// per second; angular velocity is in world-frame radians per second. The pose is
/// the state reported by the model at this step, not a requested controller target.
/// This snapshot does not itself implement integration, collision, or constraint solving.
/// </remarks>
public readonly record struct RigidBodyState
{
    /// <summary>Gets the stable identity assigned to this body by its owning model.</summary>
    public string BodyId { get; }

    /// <summary>Gets the achieved pose reported for this logical step.</summary>
    public Pose3d Pose { get; }

    /// <summary>Gets the world-frame linear velocity in metres per second.</summary>
    public Vector3d LinearVelocity { get; }

    /// <summary>Gets the world-frame angular velocity in radians per second.</summary>
    public Vector3d AngularVelocity { get; }

    /// <summary>Gets the zero-based committed logical simulation step.</summary>
    public long StepIndex { get; }

    /// <summary>Gets elapsed logical simulation time in seconds.</summary>
    public double ElapsedTime { get; }

    /// <summary>Creates a validated body-state snapshot.</summary>
    /// <param name="bodyId">Stable, non-empty identity within the owning model.</param>
    /// <param name="pose">Achieved pose in the declared world frame.</param>
    /// <param name="linearVelocity">World-frame velocity in metres per second.</param>
    /// <param name="angularVelocity">World-frame angular velocity in radians per second.</param>
    /// <param name="stepIndex">Zero-based committed logical simulation step.</param>
    /// <param name="elapsedTime">Elapsed logical time in seconds.</param>
    public RigidBodyState(
        string bodyId,
        Pose3d pose,
        Vector3d linearVelocity,
        Vector3d angularVelocity,
        long stepIndex,
        double elapsedTime)
    {
        if (string.IsNullOrWhiteSpace(bodyId))
            throw new ArgumentException("A stable body identity is required.", nameof(bodyId));
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex), "The logical step must be non-negative.");
        if (!double.IsFinite(elapsedTime) || elapsedTime < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedTime), "Elapsed logical time must be finite and non-negative.");

        BodyId = bodyId;
        Pose = pose;
        LinearVelocity = linearVelocity;
        AngularVelocity = angularVelocity;
        StepIndex = stepIndex;
        ElapsedTime = elapsedTime;
    }
}
