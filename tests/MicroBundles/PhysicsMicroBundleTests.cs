using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Physics.MicroBundles;
using TheSingularityWorkshop.Physics.Spatial;
using TheSingularityWorkshop.Physics.Dynamics;
using Xunit;

namespace TheSingularityWorkshop.Physics.MicroBundles.Tests;

public sealed class PhysicsMicroBundleTests
{
    [Fact]
    public void BundlesExposeStableDistinctProviderIdentities()
    {
        var detector = new SphereSphereContactMicroBundle();
        var response = new NormalImpulseResponseMicroBundle();
        Assert.NotEqual(detector.Id, response.Id);
        Assert.Equal("physics.contact.sphere-sphere.v1", detector.Descriptor.Providers.Single().Id);
        Assert.Equal("physics.contact.normal-impulse.v1", response.Descriptor.Providers.Single().Id);
        Assert.Empty(detector.Dependencies);
        Assert.Empty(response.Dependencies);
    }

    [Fact]
    public void SphereProviderReturnsContactEvidenceWithoutClaimingImpulse()
    {
        var detector = new SphereSphereContactMicroBundle();
        var a = Body("a", new Vector3d(0, 0, 0), new Vector3d(0, 0, 0));
        var b = Body("b", new Vector3d(0.9, 0, 0), new Vector3d(-1, 0, 0));
        var contact = detector.Detect("contact-a-b", a, 0.5, b, 0.5, 1, 0.01);
        Assert.NotNull(contact);
        Assert.True(contact.Value.IsValid);
        Assert.Null(contact.Value.NormalImpulseNewtonSeconds);
        Assert.Null(contact.Value.NormalForceNewtons);
    }

    [Fact]
    public void NormalImpulseProviderUsesCoreResolverAndPreservesStep()
    {
        var detector = new SphereSphereContactMicroBundle();
        var response = new NormalImpulseResponseMicroBundle();
        var a = Body("a", new Vector3d(0, 0, 0), new Vector3d(0, 0, 0));
        var b = Body("b", new Vector3d(0.9, 0, 0), new Vector3d(-1, 0, 0));
        var contact = detector.Detect("contact-a-b", a, 0.5, b, 0.5, 3, 0.03)!.Value;
        var result = response.Resolve(contact, a, 1, b, 1);
        Assert.Equal(3, result.Contact.StepIndex);
        Assert.True(result.Contact.NormalImpulseNewtonSeconds >= 0);
        Assert.Equal(0, result.BodyA.LinearVelocity.X, 10);
        Assert.Equal(-1, result.BodyB.LinearVelocity.X, 10);
    }

    private static RigidBodyState Body(string id, Vector3d position, Vector3d velocity) =>
        new(id, new Pose3d(position, Quaterniond.Identity), velocity, new Vector3d(0, 0, 0), 0, 0);
}
