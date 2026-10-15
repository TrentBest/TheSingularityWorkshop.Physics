using NUnit.Framework;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Dynamics;

[TestFixture]
public sealed class RigidBodyStateTests
{
    private static Pose3d IdentityPose => new(Vector3d.Zero, Quaterniond.Identity);

    [Test]
    public void Constructor_PreservesIdentityAchievedStateAndLogicalTime()
    {
        var pose = new Pose3d(
            new Vector3d(1d, 2d, 3d),
            Quaterniond.Identity);
        var state = new RigidBodyState(
            "body:root",
            pose,
            new Vector3d(0d, 1d, 0d),
            new Vector3d(0d, 0d, 0.5d),
            stepIndex: 12,
            elapsedTime: 0.6d);

        Assert.That(state.BodyId, Is.EqualTo("body:root"));
        Assert.That(state.Pose, Is.EqualTo(pose));
        Assert.That(state.LinearVelocity, Is.EqualTo(new Vector3d(0d, 1d, 0d)));
        Assert.That(state.AngularVelocity, Is.EqualTo(new Vector3d(0d, 0d, 0.5d)));
        Assert.That(state.StepIndex, Is.EqualTo(12));
        Assert.That(state.ElapsedTime, Is.EqualTo(0.6d));
        Assert.That(state.IsValid, Is.True);
    }

    [Test]
    public void DefaultSnapshot_IsExplicitlyInvalid()
    {
        Assert.That(default(RigidBodyState).IsValid, Is.False);
    }

    [Test]
    public void Constructor_RejectsMissingIdentity()
    {
        Assert.Throws<ArgumentException>(() =>
            new RigidBodyState(" ", IdentityPose, Vector3d.Zero, Vector3d.Zero, 0, 0d));
    }

    [Test]
    public void Constructor_RejectsDefaultPose()
    {
        Assert.Throws<ArgumentException>(() =>
            new RigidBodyState("body", default, Vector3d.Zero, Vector3d.Zero, 0, 0d));
    }

    [Test]
    public void Constructor_RejectsNegativeStepOrInvalidTime()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RigidBodyState("body", IdentityPose, Vector3d.Zero, Vector3d.Zero, -1, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RigidBodyState("body", IdentityPose, Vector3d.Zero, Vector3d.Zero, 0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RigidBodyState("body", IdentityPose, Vector3d.Zero, Vector3d.Zero, 0, -0.1d));
    }
}
