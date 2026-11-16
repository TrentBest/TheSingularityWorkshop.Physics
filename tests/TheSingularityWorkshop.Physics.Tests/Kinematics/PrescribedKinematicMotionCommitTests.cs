using NUnit.Framework;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Kinematics;

[TestFixture]
public sealed class PrescribedKinematicMotionCommitTests
{
    [Test]
    public void Commit_ReportsArrivalOnlyForCommittedDestinationWithinTolerance()
    {
        var definition = Definition();
        var requested = PrescribedKinematicMotion.Evaluate(definition, 11d, 1);
        var achieved = new RigidBodyState(
            definition.ActorId,
            new Pose3d(new Vector3d(1.0005d, 0d, 0d), Quaterniond.Identity),
            requested.State.LinearVelocity,
            requested.State.AngularVelocity,
            1,
            11d);

        var commit = PrescribedKinematicMotion.Commit(
            definition, requested, achieved, positionToleranceMeters: 0.001d,
            angularToleranceRadians: 0.001d);

        Assert.Multiple(() =>
        {
            Assert.That(commit.IsArrived, Is.False);
            Assert.That(commit.Achieved, Is.EqualTo(achieved));
            Assert.That(commit.Requested.State, Is.Not.EqualTo(commit.Achieved));
        });
    }

    [Test]
    public void Commit_ReportsArrivalOnlyWhenDestinationSampleIsCommitted()
    {
        var definition = Definition();
        var beforeDestination = PrescribedKinematicMotion.Evaluate(definition, 10.5d, 1);
        var beforeCommit = PrescribedKinematicMotion.Commit(
            definition, beforeDestination, beforeDestination.State, 0d, 0d);

        var destination = PrescribedKinematicMotion.Evaluate(definition, 12d, 2);
        var arrivalCommit = PrescribedKinematicMotion.Commit(
            definition, destination, destination.State, 0d, 0d);

        Assert.Multiple(() =>
        {
            Assert.That(beforeCommit.IsArrived, Is.False);
            Assert.That(arrivalCommit.IsArrived, Is.True);
            Assert.That(arrivalCommit.Achieved, Is.EqualTo(destination.State));
        });
    }

    [Test]
    public void Commit_RejectsMismatchedActorStepOrTime()
    {
        var definition = Definition();
        var requested = PrescribedKinematicMotion.Evaluate(definition, 12d, 2);
        var mismatched = new RigidBodyState(
            "another-actor", requested.State.Pose, Vector3d.Zero, Vector3d.Zero, 2, 12d);

        Assert.Throws<ArgumentException>(() =>
            PrescribedKinematicMotion.Commit(definition, requested, mismatched, 0d, 0d));
    }

    [Test]
    public void InterruptedMotionCanRetainLastImmutableCommittedState()
    {
        var definition = Definition();
        var first = PrescribedKinematicMotion.Evaluate(definition, 10.5d, 1).State;
        var laterUncommittedTarget = PrescribedKinematicMotion.Evaluate(definition, 11.5d, 3);

        // Cancellation means declining to commit the later target. The earlier
        // immutable snapshot remains the authoritative state.
        Assert.Multiple(() =>
        {
            Assert.That(first.StepIndex, Is.EqualTo(1));
            Assert.That(first.ElapsedTime, Is.EqualTo(10.5d));
            Assert.That(first.Pose.Position, Is.EqualTo(new Vector3d(0.5d, 0d, 0d)));
            Assert.That(laterUncommittedTarget.State.StepIndex, Is.EqualTo(3));
            Assert.That(first.Pose.Position, Is.EqualTo(new Vector3d(0.5d, 0d, 0d)));
        });
    }

    [Test]
    public void Commit_RejectsForgedOrMismatchedRequestedSample()
    {
        var definition = Definition();
        var legitimate = PrescribedKinematicMotion.Evaluate(definition, 12d, 2);
        var forged = new PrescribedKinematicMotionSample(
            definition.DefinitionId,
            definition.Version,
            new RigidBodyState(
                definition.ActorId,
                new Pose3d(new Vector3d(99d, 0d, 0d), Quaterniond.Identity),
                Vector3d.Zero,
                Vector3d.Zero,
                2,
                12d),
            true);

        Assert.Throws<ArgumentException>(() =>
            PrescribedKinematicMotion.Commit(definition, forged, legitimate.State, 0d, 0d));
    }

    private static PrescribedKinematicMotionDefinition Definition() =>
        new(
            "motion:commit-test",
            "actor:commit-test",
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(new Vector3d(2d, 0d, 0d), Quaterniond.Identity),
            10d,
            2d,
            2d,
            1d);
}
