using NUnit.Framework;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Math;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class GalileanFrameTransformTests
{
    [Test]
    public void ToWorld_TransformsVerticalLaunchFromMovingTrain()
    {
        // The ball is launched straight up relative to a train moving at 20 m/s.
        // In the train frame x remains zero; in the ground frame x advances at 20 m/s.
        var trainVelocity = new Vector3d(20d, 0d, 0d);
        var origin = Vector3d.Zero;
        var trainState = new ProjectileState(
            new Vector3d(0d, 0d, 1.5d),
            new Vector3d(0d, 0d, 8d),
            1d);

        var groundState = GalileanFrameTransform.ToWorld(trainState, trainVelocity, origin);

        Assert.That(groundState.Position.X, Is.EqualTo(20d).Within(1e-12));
        Assert.That(groundState.Position.Y, Is.EqualTo(0d).Within(1e-12));
        Assert.That(groundState.Position.Z, Is.EqualTo(1.5d).Within(1e-12));
        Assert.That(groundState.Velocity.X, Is.EqualTo(20d).Within(1e-12));
        Assert.That(groundState.Velocity.Y, Is.EqualTo(0d).Within(1e-12));
        Assert.That(groundState.Velocity.Z, Is.EqualTo(8d).Within(1e-12));
        Assert.That(groundState.ElapsedTime, Is.EqualTo(trainState.ElapsedTime));
    }

    [Test]
    public void ToMovingFrame_InvertsToWorldTransform()
    {
        var origin = new Vector3d(100d, -5d, 2d);
        var frameVelocity = new Vector3d(20d, 3d, 0d);
        var moving = new ProjectileState(
            new Vector3d(2d, 4d, 6d),
            new Vector3d(-1d, 2d, 8d),
            2.5d);

        var world = GalileanFrameTransform.ToWorld(moving, frameVelocity, origin);
        var recovered = GalileanFrameTransform.ToMovingFrame(world, frameVelocity, origin);

        AssertVector(recovered.Position, moving.Position);
        AssertVector(recovered.Velocity, moving.Velocity);
        Assert.That(recovered.ElapsedTime, Is.EqualTo(moving.ElapsedTime));
    }

    [Test]
    public void ToWorld_RejectsInvalidElapsedTime()
    {
        var invalid = new ProjectileState(Vector3d.Zero, Vector3d.Zero, double.NaN);

        Assert.Throws<ArgumentException>(() =>
            GalileanFrameTransform.ToWorld(invalid, Vector3d.Zero, Vector3d.Zero));
    }

    private static void AssertVector(Vector3d actual, Vector3d expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(1e-12));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1e-12));
        Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(1e-12));
    }
}
