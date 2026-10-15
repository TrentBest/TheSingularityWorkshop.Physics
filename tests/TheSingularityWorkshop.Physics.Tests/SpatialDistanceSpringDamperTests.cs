using NUnit.Framework;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Mechanics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class SpatialDistanceSpringDamperTests
{
    private static RigidBodyState Body(
        string id,
        Vector3d position,
        Vector3d? velocity = null,
        Vector3d? angularVelocity = null,
        long step = 0,
        double time = 0d) =>
        new(id, new Pose3d(position, Quaterniond.Identity),
            velocity ?? Vector3d.Zero, angularVelocity ?? Vector3d.Zero, step, time);

    private static SpatialDistanceSpringDamper.Definition Link(
        double restLength = 1d,
        double stiffness = 100d,
        double damping = 0d) =>
        new("link-3d", restLength, stiffness, damping);

    [Test]
    public void Advance_StretchedLink_ComputesAxialForceAndEqualOppositeMotion()
    {
        var a = Body("A", Vector3d.Zero);
        var b = Body("B", new Vector3d(2d, 0d, 0d));

        var result = SpatialDistanceSpringDamper.Advance(Link(), a, 2d, b, 4d, 7, 0.6d, 0.1d);

        Assert.That(result.LinkId, Is.EqualTo("link-3d"));
        Assert.That(result.StepIndex, Is.EqualTo(7));
        Assert.That(result.ElapsedTime, Is.EqualTo(0.7d).Within(1e-15));
        Assert.That(result.ForceOnBodyA, Is.EqualTo(new Vector3d(100d, 0d, 0d)));
        Assert.That(result.ForceOnBodyB, Is.EqualTo(new Vector3d(-100d, 0d, 0d)));
        Assert.That(result.BodyA.LinearVelocity.X, Is.EqualTo(5d).Within(1e-14));
        Assert.That(result.BodyB.LinearVelocity.X, Is.EqualTo(-2.5d).Within(1e-14));
        Assert.That(result.BodyA.Pose.Position.X, Is.EqualTo(0.5d).Within(1e-14));
        Assert.That(result.BodyB.Pose.Position.X, Is.EqualTo(1.75d).Within(1e-14));
        Assert.That(result.BodyA.StepIndex, Is.EqualTo(7));
        Assert.That(result.BodyB.ElapsedTime, Is.EqualTo(0.7d).Within(1e-15));
    }

    [Test]
    public void Advance_UsesThreeDimensionalLineAndRadialDamping()
    {
        var a = Body("A", Vector3d.Zero);
        var b = Body("B", new Vector3d(0d, 3d, 4d), new Vector3d(0d, 0d, 2d));

        var result = SpatialDistanceSpringDamper.Advance(
            Link(restLength: 4d, stiffness: 10d, damping: 2d),
            a, 1d, b, 1d, 1, 0d, 0.1d);

        // Distance is 5 m, radial speed is 1.6 m/s, so load is 10 + 3.2 = 13.2 N.
        Assert.That(result.ForceOnBodyA.X, Is.Zero);
        Assert.That(result.ForceOnBodyA.Y, Is.EqualTo(7.92d).Within(1e-12));
        Assert.That(result.ForceOnBodyA.Z, Is.EqualTo(10.56d).Within(1e-12));
        Assert.That(result.ForceOnBodyA + result.ForceOnBodyB, Is.EqualTo(Vector3d.Zero));
    }

    [Test]
    public void Advance_PreservesOrientationAndAngularVelocity()
    {
        var angular = new Vector3d(0.1d, 0.2d, 0.3d);
        var a = Body("A", Vector3d.Zero, angularVelocity: angular);
        var b = Body("B", new Vector3d(2d, 0d, 0d), angularVelocity: angular);

        var result = SpatialDistanceSpringDamper.Advance(Link(), a, 1d, b, 1d, 1, 0d, 0.01d);

        Assert.That(result.BodyA.Pose.Orientation, Is.EqualTo(a.Pose.Orientation));
        Assert.That(result.BodyA.AngularVelocity, Is.EqualTo(angular));
        Assert.That(result.BodyB.AngularVelocity, Is.EqualTo(angular));
    }

    [Test]
    public void Advance_CoincidentOriginsHaveNoDefinedAxialDirection()
    {
        var a = Body("A", Vector3d.Zero);
        var b = Body("B", Vector3d.Zero);

        var result = SpatialDistanceSpringDamper.Advance(Link(), a, 1d, b, 1d, 0, 0d, 0.01d);

        Assert.That(result.ForceOnBodyA, Is.EqualTo(Vector3d.Zero));
        Assert.That(result.ForceOnBodyB, Is.EqualTo(Vector3d.Zero));
    }

    [Test]
    public void Advance_IsRepeatableForIdenticalInputs()
    {
        var a = Body("A", new Vector3d(-0.2d, 0.1d, 0d), new Vector3d(0.2d, 0d, 0d));
        var b = Body("B", new Vector3d(1.5d, 0.8d, 0.4d), new Vector3d(-0.1d, 0.3d, 0d));

        var first = SpatialDistanceSpringDamper.Advance(Link(damping: 2d), a, 2d, b, 3d, 12, 2d, 0.01d);
        var second = SpatialDistanceSpringDamper.Advance(Link(damping: 2d), a, 2d, b, 3d, 12, 2d, 0.01d);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Advance_RejectsInvalidMassIdentityAndTimestep()
    {
        var a = Body("A", Vector3d.Zero);
        var b = Body("B", new Vector3d(1d, 0d, 0d));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SpatialDistanceSpringDamper.Advance(Link(), a, 0d, b, 1d, 0, 0d, 0.01d));
        Assert.Throws<ArgumentException>(() =>
            SpatialDistanceSpringDamper.Advance(Link(), a, 1d, a, 1d, 0, 0d, 0.01d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SpatialDistanceSpringDamper.Advance(Link(), a, 1d, b, 1d, 0, 0d, 0d));
    }
}
