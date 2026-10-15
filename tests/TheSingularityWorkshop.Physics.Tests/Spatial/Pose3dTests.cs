using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class Pose3dTests
{
    [Test]
    public void Constructor_NormalizesOrientation()
    {
        var pose = new Pose3d(
            new Vector3d(1d, 2d, 3d),
            new Quaterniond(2d, 0d, 0d, 0d));

        Assert.That(pose.Position, Is.EqualTo(new Vector3d(1d, 2d, 3d)));
        Assert.That(pose.Orientation, Is.EqualTo(Quaterniond.Identity));
        Assert.That(pose.IsValid, Is.True);
    }

    [Test]
    public void DefaultPose_IsExplicitlyInvalid()
    {
        Assert.That(default(Pose3d).IsValid, Is.False);
    }

    [Test]
    public void Constructor_RejectsZeroOrientation()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new Pose3d(Vector3d.Zero, new Quaterniond(0d, 0d, 0d, 0d)));
    }

    [Test]
    public void DirectionTransforms_AreInverseForQuarterTurn()
    {
        var halfAngle = Math.PI / 4d;
        var pose = new Pose3d(
            Vector3d.Zero,
            new Quaterniond(Math.Cos(halfAngle), 0d, 0d, Math.Sin(halfAngle)));

        var world = pose.TransformDirectionToWorld(new Vector3d(1d, 0d, 0d));
        var local = pose.TransformDirectionToLocal(world);

        Assert.That(world.X, Is.EqualTo(0d).Within(1e-14));
        Assert.That(world.Y, Is.EqualTo(1d).Within(1e-14));
        Assert.That(local.X, Is.EqualTo(1d).Within(1e-14));
        Assert.That(local.Y, Is.EqualTo(0d).Within(1e-14));
        Assert.That(local.Z, Is.EqualTo(0d).Within(1e-14));
    }
}
