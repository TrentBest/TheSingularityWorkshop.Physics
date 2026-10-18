using NUnit.Framework;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Dynamics;

[TestFixture]
public sealed class RigidBodyIntegratorTests
{
    [Test]
    public void Advance_UsesSemiImplicitEulerAndCommitsLogicalTime()
    {
        var initial = Body(
            position: new Vector3d(1d, 2d, 3d),
            velocity: new Vector3d(2d, 0d, -1d),
            step: 7,
            elapsed: 0.35d);

        var next = RigidBodyIntegrator.Advance(
            initial,
            new Vector3d(0d, -9.81d, 0d),
            deltaTimeSeconds: 0.5d);

        Assert.Multiple(() =>
        {
            Assert.That(next.BodyId, Is.EqualTo(initial.BodyId));
            Assert.That(next.StepIndex, Is.EqualTo(8));
            Assert.That(next.ElapsedTime, Is.EqualTo(0.85d).Within(1e-14));
            Assert.That(next.LinearVelocity, Is.EqualTo(new Vector3d(2d, -4.905d, -1d)));
            Assert.That(next.Pose.Position, Is.EqualTo(new Vector3d(2d, -0.4525d, 2.5d)));
            Assert.That(next.AngularVelocity, Is.EqualTo(initial.AngularVelocity));
            Assert.That(initial.StepIndex, Is.EqualTo(7), "The input snapshot must remain immutable.");
            Assert.That(initial.Pose.Position, Is.EqualTo(new Vector3d(1d, 2d, 3d)));
        });
    }

    [Test]
    public void Advance_IntegratesWorldAngularVelocityIntoOrientation()
    {
        var initial = Body(angularVelocity: new Vector3d(0d, 0d, Math.PI / 2d));

        var next = RigidBodyIntegrator.Advance(initial, Vector3d.Zero, 1d);
        var rotated = next.Pose.TransformDirectionToWorld(new Vector3d(1d, 0d, 0d));

        Assert.Multiple(() =>
        {
            Assert.That(rotated.X, Is.EqualTo(0d).Within(1e-12));
            Assert.That(rotated.Y, Is.EqualTo(1d).Within(1e-12));
            Assert.That(rotated.Z, Is.EqualTo(0d).Within(1e-12));
            Assert.That(next.Pose.Orientation.Norm, Is.EqualTo(1d).Within(1e-12));
        });
    }

    [Test]
    public void Advance_RepeatedIdenticalRunsProduceIdenticalSnapshots()
    {
        var first = Body(velocity: new Vector3d(0.25d, 1d, -2d));
        var second = first;

        for (var i = 0; i < 100; i++)
        {
            first = RigidBodyIntegrator.Advance(first, new Vector3d(0d, -9.81d, 0d), 1d / 120d);
            second = RigidBodyIntegrator.Advance(second, new Vector3d(0d, -9.81d, 0d), 1d / 120d);
        }

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first.StepIndex, Is.EqualTo(100));
    }

    [Test]
    public void Advance_RejectsInvalidSnapshotAndNonPositiveOrNonFiniteTimestep()
    {
        var body = Body();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
                RigidBodyIntegrator.Advance(default, Vector3d.Zero, 0.01d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RigidBodyIntegrator.Advance(body, Vector3d.Zero, 0d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RigidBodyIntegrator.Advance(body, Vector3d.Zero, -0.01d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RigidBodyIntegrator.Advance(body, Vector3d.Zero, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RigidBodyIntegrator.Advance(body, Vector3d.Zero, double.PositiveInfinity));
        });
    }

    private static RigidBodyState Body(
        Vector3d? position = null,
        Vector3d? velocity = null,
        Vector3d? angularVelocity = null,
        long step = 0,
        double elapsed = 0d) =>
        new(
            "body:reference",
            new Pose3d(position ?? Vector3d.Zero, Quaterniond.Identity),
            velocity ?? Vector3d.Zero,
            angularVelocity ?? Vector3d.Zero,
            step,
            elapsed);
}
