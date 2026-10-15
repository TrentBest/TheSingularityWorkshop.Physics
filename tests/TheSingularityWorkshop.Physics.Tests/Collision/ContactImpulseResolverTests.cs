using NUnit.Framework;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Collision;

[TestFixture]
public sealed class ContactImpulseResolverTests
{
    [Test]
    public void Resolve_EqualMassClosingBodies_AppliesEqualAndOppositeImpulse()
    {
        var a = Body("a", new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(-1d, 0d, 0d));
        var contact = Contact(-0.1d);

        var result = ContactImpulseResolver.Resolve(contact, a, 1d, b, 1d);

        Assert.Multiple(() =>
        {
            Assert.That(result.Contact.NormalImpulseNewtonSeconds, Is.EqualTo(1d).Within(1e-12));
            Assert.That(result.BodyA.LinearVelocity, Is.EqualTo(Vector3d.Zero));
            Assert.That(result.BodyB.LinearVelocity, Is.EqualTo(Vector3d.Zero));
            Assert.That(result.BodyA.StepIndex, Is.EqualTo(contact.StepIndex));
            Assert.That(result.BodyA.ElapsedTime, Is.EqualTo(contact.ElapsedTime));
            Assert.That(result.BodyA.Pose.Position, Is.EqualTo(a.Pose.Position));
            Assert.That(result.BodyB.Pose.Position, Is.EqualTo(b.Pose.Position));
        });
    }

    [Test]
    public void Resolve_UnequalMasses_PreservesTangentialVelocityAndLinearMomentum()
    {
        var a = Body("a", new Vector3d(1d, 4d, 0d));
        var b = Body("b", new Vector3d(-1d, -3d, 0d));

        var result = ContactImpulseResolver.Resolve(Contact(-0.1d), a, 1d, b, 3d);

        Assert.Multiple(() =>
        {
            Assert.That(result.BodyA.LinearVelocity.X, Is.EqualTo(-0.5d).Within(1e-12));
            Assert.That(result.BodyB.LinearVelocity.X, Is.EqualTo(-0.5d).Within(1e-12));
            Assert.That(result.BodyA.LinearVelocity.Y, Is.EqualTo(4d));
            Assert.That(result.BodyB.LinearVelocity.Y, Is.EqualTo(-3d));
            Assert.That(
                result.BodyA.LinearVelocity.X + (3d * result.BodyB.LinearVelocity.X),
                Is.EqualTo(-2d).Within(1e-12));
        });
    }

    [Test]
    public void Resolve_ElasticImpact_ReversesRelativeNormalSpeed()
    {
        var a = Body("a", new Vector3d(2d, 0d, 0d));
        var b = Body("b", new Vector3d(-1d, 0d, 0d));

        var result = ContactImpulseResolver.Resolve(Contact(0d), a, 1d, b, 1d, restitution: 1d);

        Assert.That(result.Contact.NormalImpulseNewtonSeconds, Is.EqualTo(3d).Within(1e-12));
        Assert.That(result.BodyB.LinearVelocity.X - result.BodyA.LinearVelocity.X, Is.EqualTo(3d).Within(1e-12));
    }

    [Test]
    public void Resolve_SeparatingBodies_DoesNotApplyPullingImpulse()
    {
        var a = Body("a", new Vector3d(-1d, 0d, 0d));
        var b = Body("b", new Vector3d(1d, 0d, 0d));

        var result = ContactImpulseResolver.Resolve(Contact(-0.2d), a, 1d, b, 1d);

        Assert.That(result.Contact.NormalImpulseNewtonSeconds, Is.EqualTo(0d));
        Assert.That(result.BodyA.LinearVelocity, Is.EqualTo(a.LinearVelocity));
        Assert.That(result.BodyB.LinearVelocity, Is.EqualTo(b.LinearVelocity));
    }

    [Test]
    public void Resolve_PositiveToleranceGap_DoesNotApplyImpulse()
    {
        var a = Body("a", new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(-1d, 0d, 0d));

        var result = ContactImpulseResolver.Resolve(Contact(0.01d), a, 1d, b, 1d);

        Assert.That(result.Contact.NormalImpulseNewtonSeconds, Is.EqualTo(0d));
        Assert.That(result.BodyA.LinearVelocity, Is.EqualTo(a.LinearVelocity));
        Assert.That(result.BodyB.LinearVelocity, Is.EqualTo(b.LinearVelocity));
    }

    [Test]
    public void Resolve_RejectsInvalidMassAndMismatchedIdentity()
    {
        var a = Body("a", new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(-1d, 0d, 0d));
        var wrong = Body("other", Vector3d.Zero);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ContactImpulseResolver.Resolve(Contact(-0.1d), a, 0d, b, 1d));
            Assert.Throws<ArgumentException>(() =>
                ContactImpulseResolver.Resolve(Contact(-0.1d), wrong, 1d, b, 1d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ContactImpulseResolver.Resolve(Contact(-0.1d), a, 1d, b, 1d, restitution: 1.1d));
        });
    }

    private static ContactObservation Contact(double separation) =>
        new("a-b", "a", "b", Vector3d.Zero, new Vector3d(1d, 0d, 0d),
            new Vector3d(-2d, 0d, 0d), separation, true, 3, 0.3d);

    private static RigidBodyState Body(string id, Vector3d velocity) =>
        new(id, new Pose3d(Vector3d.Zero, Quaterniond.Identity), velocity, Vector3d.Zero, 2, 0.2d);
}
