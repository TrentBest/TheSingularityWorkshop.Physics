using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;

namespace TheSingularityWorkshop.Physics.MicroBundles;

/// <summary>Typed host-neutral surface for sphere-sphere geometric contact observations.</summary>
public interface ISphereSphereContactProvider
{
    ContactObservation? Detect(string contactId, RigidBodyState bodyA, double radiusA,
        RigidBodyState bodyB, double radiusB, long stepIndex, double elapsedTime, double contactTolerance = 0d);
}

/// <summary>Typed host-neutral surface for the currently supported frictionless translational normal impulse.</summary>
/// <remarks>This contract does not claim friction, angular impulse, rigid-body integration, or resting-contact support.</remarks>
public interface INormalImpulseResponseProvider
{
    ContactImpulseResolver.Result Resolve(ContactObservation contact, RigidBodyState bodyA, double massA,
        RigidBodyState bodyB, double massB, double restitution = 0d);
}

/// <summary>MicroBundle adapter for the existing sphere-sphere contact detector.</summary>
public sealed class SphereSphereContactMicroBundle : IMicroBundle, ISphereSphereContactProvider
{
    public const ulong BundleId = 5201;
    public const string ProviderId = "physics.contact.sphere-sphere.v1";
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    public void Load(IMicroBundleLoadContext context) => ArgumentNullException.ThrowIfNull(context);

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }

    public ContactObservation? Detect(string contactId, RigidBodyState bodyA, double radiusA,
        RigidBodyState bodyB, double radiusB, long stepIndex, double elapsedTime, double contactTolerance = 0d) =>
        SphereSphereContactDetector.Detect(contactId, bodyA, radiusA, bodyB, radiusB, stepIndex, elapsedTime, contactTolerance);
}

/// <summary>MicroBundle adapter for the existing translational normal impulse resolver.</summary>
public sealed class NormalImpulseResponseMicroBundle : IMicroBundle, INormalImpulseResponseProvider
{
    public const ulong BundleId = 5202;
    public const string ProviderId = "physics.contact.normal-impulse.v1";
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    public void Load(IMicroBundleLoadContext context) => ArgumentNullException.ThrowIfNull(context);

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }

    public ContactImpulseResolver.Result Resolve(ContactObservation contact, RigidBodyState bodyA, double massA,
        RigidBodyState bodyB, double massB, double restitution = 0d) =>
        ContactImpulseResolver.Resolve(contact, bodyA, massA, bodyB, massB, restitution);
}
