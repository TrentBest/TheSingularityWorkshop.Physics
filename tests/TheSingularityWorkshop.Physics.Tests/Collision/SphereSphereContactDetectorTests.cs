using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Collision;

[TestFixture]
public sealed class SphereSphereContactDetectorTests
{
    [Test]
    public void Detect_OverlappingSpheres_ReportsPenetrationAndWorldGeometry()
    {
        var a = Body("a", new Vector3d(0d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(1.5d, 0d, 0d), new Vector3d(-2d, 3d, 0d));

        var contact = SphereSphereContactDetector.Detect("a-b", a, 1d, b, 1d, 4, 0.4d);

        Assert.That(contact, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(contact!.Value.IsActive, Is.True);
            Assert.That(contact.Value.SignedSeparation, Is.EqualTo(-0.5d).Within(1e-12));
            Assert.That(contact.Value.NormalFromAToB, Is.EqualTo(new Vector3d(1d, 0d, 0d)));
            Assert.That(contact.Value.Point, Is.EqualTo(new Vector3d(0.75d, 0d, 0d)));
            Assert.That(contact.Value.RelativeVelocity, Is.EqualTo(new Vector3d(-3d, 3d, 0d)));
            Assert.That(contact.Value.NormalForceNewtons, Is.Null);
            Assert.That(contact.Value.NormalImpulseNewtonSeconds, Is.Null);
            Assert.That(contact.Value.StepIndex, Is.EqualTo(4));
            Assert.That(contact.Value.ElapsedTime, Is.EqualTo(0.4d));
            Assert.That(contact.Value.IsValid, Is.True);
        });
    }

    [Test]
    public void Detect_WithinExplicitTolerance_ReportsPositiveSeparation()
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", new Vector3d(2.05d, 0d, 0d), Vector3d.Zero);

        var contact = SphereSphereContactDetector.Detect("a-b", a, 1d, b, 1d, 0, 0d, 0.1d);

        Assert.That(contact, Is.Not.Null);
        Assert.That(contact!.Value.SignedSeparation, Is.EqualTo(0.05d).Within(1e-12));
    }

    [Test]
    public void Detect_SeparatedBeyondTolerance_ReturnsNull()
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", new Vector3d(2.2d, 0d, 0d), Vector3d.Zero);

        Assert.That(SphereSphereContactDetector.Detect("a-b", a, 1d, b, 1d, 0, 0d, 0.1d), Is.Null);
    }

    [Test]
    public void Detect_CoincidentCentres_ReturnsNullBecauseNormalIsUndefined()
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", Vector3d.Zero, Vector3d.Zero);

        Assert.That(SphereSphereContactDetector.Detect("a-b", a, 1d, b, 1d, 0, 0d), Is.Null);
    }

    [Test]
    public void Detect_RejectsInvalidRadiusAndSameBody()
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", new Vector3d(1d, 0d, 0d), Vector3d.Zero);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                SphereSphereContactDetector.Detect("a-b", a, -1d, b, 1d, 0, 0d));
            Assert.Throws<ArgumentException>(() =>
                SphereSphereContactDetector.Detect("a-a", a, 1d, a, 1d, 0, 0d));
        });
    }

    private static RigidBodyState Body(string id, Vector3d position, Vector3d velocity) =>
        new(id, new Pose3d(position, Quaterniond.Identity), velocity, Vector3d.Zero, 0, 0d);
}
