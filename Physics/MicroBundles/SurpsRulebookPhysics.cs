using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.SingularityUniversalRolePlayingSystem;

namespace TheSingularityWorkshop.SingularityUniversalRolePlayingSystem.Rulebooks;

/// <summary>
/// Stable identity for the SURPS Physics rulebook container.
/// </summary>
public static class SurpsRulebookPhysicsIdentity
{
    public const string CanonicalId =
        "TheSingularityWorkshop.SingularityUniversalRolePlayingSystem.SurpsRulebookPhysics";

    // FNV-1a 64-bit hash of CanonicalId.
    public const ulong Id = 0x0B5175163AE21E02UL;
}

/// <summary>
/// Presents the first implemented Physics capability slice as one SURPS rulebook container.
/// </summary>
/// <remarks>
/// This is a composition wrapper, not a Physics implementation. The three dependencies
/// remain authoritative for geometric contact observation, normal impulse response, and
/// prescribed kinematic motion respectively. Future Physics rulebooks can be split into
/// focused books without changing those domain providers.
/// </remarks>
public sealed class SurpsRulebookPhysics : ISurpsRulebook
{
    public MicroBundleDescriptor Descriptor { get; } = new(
        SurpsRulebookPhysicsIdentity.Id,
        "0.1.0-alpha.1",
        dependencies:
        [
            new MicroBundleDependency(PhysicsCapabilityDependencies.SphereSphereContactBundleId),
            new MicroBundleDependency(PhysicsCapabilityDependencies.NormalImpulseResponseBundleId),
            new MicroBundleDependency(PhysicsCapabilityDependencies.PrescribedKinematicMotionBundleId)
        ],
        providers:
        [
            new MicroBundleProvider("surps.rulebook.physics")
        ]);

    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; } =
    [
        MicroBundleDependencyRequest.Unconfigured(PhysicsCapabilityDependencies.SphereSphereContactBundleId),
        MicroBundleDependencyRequest.Unconfigured(PhysicsCapabilityDependencies.NormalImpulseResponseBundleId),
        MicroBundleDependencyRequest.Unconfigured(PhysicsCapabilityDependencies.PrescribedKinematicMotionBundleId)
    ];

    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }
}

/// <summary>Stable references to the first Physics MicroBundle capabilities.</summary>
internal static class PhysicsCapabilityDependencies
{
    public const ulong SphereSphereContactBundleId = 5201;
    public const ulong NormalImpulseResponseBundleId = 5202;
    public const ulong PrescribedKinematicMotionBundleId = 5203;
}
