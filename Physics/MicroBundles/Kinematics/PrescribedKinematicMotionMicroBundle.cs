using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.MicroBundles.Kinematics;

/// <summary>
/// Typed provider contract for deterministic prescribed kinematic motion.
/// </summary>
/// <remarks>
/// Hosts resolve this interface from a loaded MicroBundle instance. The neutral
/// MicroBundleDomain provider ID is a declaration, not an automatic service locator.
/// </remarks>
public interface IPrescribedKinematicMotionProvider
{
    /// <summary>Evaluates a requested trajectory at an absolute logical time and step.</summary>
    PrescribedKinematicMotionSample Evaluate(
        PrescribedKinematicMotionDefinition definition,
        double logicalTimeSeconds,
        long stepIndex);

    /// <summary>Evaluates a requested trajectory at a declared fixed logical step.</summary>
    PrescribedKinematicMotionSample EvaluateFixedStep(
        PrescribedKinematicMotionDefinition definition,
        long stepIndex,
        double fixedTimeStepSeconds,
        long totalStepCount);

    /// <summary>Validates a separately supplied achieved state against a requested sample.</summary>
    PrescribedKinematicMotionCommit Commit(
        PrescribedKinematicMotionDefinition definition,
        PrescribedKinematicMotionSample requested,
        RigidBodyState achieved,
        double positionToleranceMeters,
        double angularToleranceRadians);
}

/// <summary>
/// MicroBundle adapter exposing the Physics prescribed-kinematics provider.
/// </summary>
/// <remarks>
/// This adapter contains no host registry and does not pretend that the provider
/// ID alone resolves a CLR contract. A composition host discovers the loaded
/// bundle instance and explicitly casts it to IPrescribedKinematicMotionProvider.
/// </remarks>
public sealed class PrescribedKinematicMotionMicroBundle :
    IMicroBundle,
    IPrescribedKinematicMotionProvider
{
    /// <summary>Stable provider identifier declared by this bundle.</summary>
    public const string ProviderId = "physics.kinematics.prescribed-motion.v1";

    /// <summary>Stable MicroBundle identity for prescribed kinematic motion.</summary>
    public const ulong BundleId = 5203;

    /// <inheritdoc />
    public MicroBundleDescriptor Descriptor { get; } = new(
        BundleId,
        "0.1.0-alpha.1",
        providers: [new MicroBundleProvider(ProviderId)]);

    /// <inheritdoc />
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; } =
        Array.Empty<MicroBundleDependencyRequest>();

    /// <inheritdoc />
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        // The host owns typed provider registration/resolution. The loaded bundle
        // instance itself is the provider object; this neutral context has no
        // registration API to call.
    }

    /// <inheritdoc />
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (roundIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(roundIndex));

        // This domain provider has no composition policy or mutable arbitration state.
        return false;
    }

    /// <inheritdoc />
    public PrescribedKinematicMotionSample Evaluate(
        PrescribedKinematicMotionDefinition definition,
        double logicalTimeSeconds,
        long stepIndex) =>
        PrescribedKinematicMotion.Evaluate(definition, logicalTimeSeconds, stepIndex);

    /// <inheritdoc />
    public PrescribedKinematicMotionSample EvaluateFixedStep(
        PrescribedKinematicMotionDefinition definition,
        long stepIndex,
        double fixedTimeStepSeconds,
        long totalStepCount) =>
        PrescribedKinematicMotion.EvaluateFixedStep(
            definition, stepIndex, fixedTimeStepSeconds, totalStepCount);

    /// <inheritdoc />
    public PrescribedKinematicMotionCommit Commit(
        PrescribedKinematicMotionDefinition definition,
        PrescribedKinematicMotionSample requested,
        RigidBodyState achieved,
        double positionToleranceMeters,
        double angularToleranceRadians) =>
        PrescribedKinematicMotion.Commit(
            definition, requested, achieved, positionToleranceMeters, angularToleranceRadians);
}
