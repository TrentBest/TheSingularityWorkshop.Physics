using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;

namespace TheSingularityWorkshop.Physics.MicroBundles;

/// <summary>Typed host-neutral surface for sphere-sphere geometric contact observations.</summary>
public interface ISphereSphereContactProvider
{
    /// <summary>Detects a contact candidate between two spherical body snapshots.</summary>
    ContactObservation? Detect(string contactId, RigidBodyState bodyA, double radiusA,
        RigidBodyState bodyB, double radiusB, long stepIndex, double elapsedTime, double contactTolerance = 0d);
}

/// <summary>Typed host-neutral surface for frictionless translational normal impulse response.</summary>
/// <remarks>This contract does not claim friction, angular impulse, rigid-body integration, or resting-contact support.</remarks>
public interface INormalImpulseResponseProvider
{
    /// <summary>Resolves one active contact and returns achieved body snapshots.</summary>
    ContactImpulseResolver.Result Resolve(ContactObservation contact, RigidBodyState bodyA, double massA,
        RigidBodyState bodyB, double massB, double restitution = 0d);
}

/// <summary>MicroBundle adapter for the existing sphere-sphere contact detector.</summary>
public sealed class SphereSphereContactMicroBundle : IMicroBundle, ISphereSphereContactProvider
{
    /// <summary>Stable MicroBundle ID.</summary>
    public const ulong BundleId = 5201;
    /// <summary>Stable provider ID for sphere-sphere contact observations.</summary>
    public const string ProviderId = "physics.contact.sphere-sphere.v1";
    /// <summary>Gets the bundle identity, version and provider declaration.</summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);
    /// <summary>Gets the stable bundle identity.</summary>
    public ulong Id => Descriptor.Id;
    /// <summary>Gets required bundle dependencies.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>Validates the supplied host context; this capability needs no load-time resources.</summary>
    public void Load(IMicroBundleLoadContext context) => ArgumentNullException.ThrowIfNull(context);

    /// <summary>Participates in deterministic arbitration without changing this static capability.</summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }

    /// <inheritdoc />
    public ContactObservation? Detect(string contactId, RigidBodyState bodyA, double radiusA,
        RigidBodyState bodyB, double radiusB, long stepIndex, double elapsedTime, double contactTolerance = 0d) =>
        SphereSphereContactDetector.Detect(contactId, bodyA, radiusA, bodyB, radiusB, stepIndex, elapsedTime, contactTolerance);
}

/// <summary>MicroBundle adapter for the existing translational normal impulse resolver.</summary>
public sealed class NormalImpulseResponseMicroBundle : IMicroBundle, INormalImpulseResponseProvider
{
    /// <summary>Stable MicroBundle ID.</summary>
    public const ulong BundleId = 5202;
    /// <summary>Stable provider ID for translational normal impulse response.</summary>
    public const string ProviderId = "physics.contact.normal-impulse.v1";
    /// <summary>Gets the bundle identity, version and provider declaration.</summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);
    /// <summary>Gets the stable bundle identity.</summary>
    public ulong Id => Descriptor.Id;
    /// <summary>Gets required bundle dependencies.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>Validates the supplied host context; this capability needs no load-time resources.</summary>
    public void Load(IMicroBundleLoadContext context) => ArgumentNullException.ThrowIfNull(context);

    /// <summary>Participates in deterministic arbitration without changing this static capability.</summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }

    /// <inheritdoc />
    public ContactImpulseResolver.Result Resolve(ContactObservation contact, RigidBodyState bodyA, double massA,
        RigidBodyState bodyB, double massB, double restitution = 0d) =>
        ContactImpulseResolver.Resolve(contact, bodyA, massA, bodyB, massB, restitution);
}
