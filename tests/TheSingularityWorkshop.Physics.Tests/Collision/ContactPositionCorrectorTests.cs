using NUnit.Framework;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Collision;

[TestFixture]
public sealed class ContactPositionCorrectorTests
{
    [Test]
    public void Correct_EqualMassPenetration_AppliesBoundedOppositeTranslations()
    {
        var a = Body("a", new Vector3d(1d, 2d, 3d), new Vector3d(1d, 2d, 3d));
        var b = Body("b", new Vector3d(4d, 5d, 6d), new Vector3d(4d, 5d, 6d));
        var contact = Contact(-0.2d);

        var result = ContactPositionCorrector.Correct(
            contact, a, 1d, b, 1d,
            penetrationSlopMetres: 0.02d,
            correctionFraction: 0.8d,
            maximumCorrectionMetres: 0.1d);

        Assert.Multiple(() =>
        {
            Assert.That(result.AppliedCorrectionMetres, Is.EqualTo(0.1d).Within(1e-12));
            Assert.That(result.BodyA.Pose.Position.X, Is.EqualTo(0.95d).Within(1e-12));
            Assert.That(result.BodyB.Pose.Position.X, Is.EqualTo(4.05d).Within(1e-12));
            Assert.That(result.Contact.SignedSeparation, Is.EqualTo(-0.1d).Within(1e-12));
            Assert.That(result.BodyA.LinearVelocity, Is.EqualTo(a.LinearVelocity));
            Assert.That(result.BodyB.LinearVelocity, Is.EqualTo(b.LinearVelocity));
            Assert.That(result.BodyA.Pose.Orientation, Is.EqualTo(a.Pose.Orientation));
            Assert.That(result.BodyB.Pose.Orientation, Is.EqualTo(b.Pose.Orientation));
            Assert.That(result.BodyA.StepIndex, Is.EqualTo(a.StepIndex));
            Assert.That(result.BodyB.ElapsedTime, Is.EqualTo(b.ElapsedTime));
        });
    }

    [Test]
    public void Correct_UnequalMasses_MovesLighterBodyFarther()
    {
        var a = Body("a", Vector3d.Zero);
        var b = Body("b", Vector3d.Zero);

        var result = ContactPositionCorrector.Correct(
            Contact(-0.1d), a, 1d, b, 3d,
            penetrationSlopMetres: 0d,
            correctionFraction: 1d,
            maximumCorrectionMetres: 1d);

        Assert.Multiple(() =>
        {
            Assert.That(result.BodyA.Pose.Position.X, Is.EqualTo(-0.075d).Within(1e-12));
            Assert.That(result.BodyB.Pose.Position.X, Is.EqualTo(0.025d).Within(1e-12));
            Assert.That(result.AppliedCorrectionMetres, Is.EqualTo(0.1d).Within(1e-12));
        });
    }

    [TestCase(0d)]
    [TestCase(0.01d)]
    public void Correct_TouchingOrPositiveGap_DoesNotMoveBodies(double separation)
    {
        var a = Body("a", Vector3d.Zero);
        var b = Body("b", Vector3d.Zero);

        var result = ContactPositionCorrector.Correct(Contact(separation), a, 1d, b, 1d);

        Assert.Multiple(() =>
        {
            Assert.That(result.AppliedCorrectionMetres, Is.Zero);
            Assert.That(result.BodyA.Pose.Position, Is.EqualTo(a.Pose.Position));
            Assert.That(result.BodyB.Pose.Position, Is.EqualTo(b.Pose.Position));
            Assert.That(result.Contact.SignedSeparation, Is.EqualTo(separation));
        });
    }

    [Test]
    public void Correct_RejectsInvalidConfigurationAndMismatchedIdentity()
    {
        var a = Body("a", Vector3d.Zero);
        var b = Body("b", Vector3d.Zero);
        var wrong = Body("other", Vector3d.Zero);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ContactPositionCorrector.Correct(Contact(-0.1d), a, 0d, b, 1d));
            Assert.Throws<ArgumentException>(() =>
                ContactPositionCorrector.Correct(Contact(-0.1d), wrong, 1d, b, 1d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ContactPositionCorrector.Correct(Contact(-0.1d), a, 1d, b, 1d, correctionFraction: 0d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ContactPositionCorrector.Correct(Contact(-0.1d), a, 1d, b, 1d, maximumCorrectionMetres: 0d));
        });
    }

    private static ContactObservation Contact(double separation) =>
        new("a-b", "a", "b", Vector3d.Zero, new Vector3d(1d, 0d, 0d),
            Vector3d.Zero, separation, true, 3, 0.3d);

    private static RigidBodyState Body(string id, Vector3d position, Vector3d? velocity = null) =>
        new(id, new Pose3d(position, Quaterniond.Identity), velocity ?? Vector3d.Zero,
            new Vector3d(0.1d, 0.2d, 0.3d), 2, 0.2d);
}
