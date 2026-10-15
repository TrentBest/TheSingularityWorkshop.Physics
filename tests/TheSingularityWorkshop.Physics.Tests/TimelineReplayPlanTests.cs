using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineReplayPlanTests
{
    [Test]
    public void PlanSplitsReplayAtForkBoundariesWithoutAssigningChildEventsToParent()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(100);
        var child = TimelineBranch.Fork("child", root, 40).WithHeadTick(70);
        var grandchild = TimelineBranch.Fork("grandchild", child, 55).WithHeadTick(60);
        var lineage = new TimelineLineage([root, child, grandchild]);
        var checkpoint = MakeCheckpoint("root", 20);

        var plan = new TimelineReplayPlan(lineage, checkpoint, 60, "schema-1", "model-1");
        var intervals = plan.CopyIntervals();

        Assert.That(plan.IntervalCount, Is.EqualTo(3));
        AssertInterval(intervals[0], "root", 20, 40);
        AssertInterval(intervals[1], "child", 40, 55);
        AssertInterval(intervals[2], "grandchild", 55, 60);
    }

    [Test]
    public void PlanCanStartFromCheckpointInsideAnAncestorBranch()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(100);
        var child = TimelineBranch.Fork("child", root, 40).WithHeadTick(70);
        var grandchild = TimelineBranch.Fork("grandchild", child, 55).WithHeadTick(60);
        var lineage = new TimelineLineage([root, child, grandchild]);

        var plan = new TimelineReplayPlan(lineage, MakeCheckpoint("child", 50), 60, "schema-1", "model-1");
        var intervals = plan.CopyIntervals();

        Assert.That(plan.IntervalCount, Is.EqualTo(2));
        AssertInterval(intervals[0], "child", 50, 55);
        AssertInterval(intervals[1], "grandchild", 55, 60);
    }

    [Test]
    public void PlanRejectsIneligibleTargetAndIncompatibleVersions()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var child = TimelineBranch.Fork("child", root, 10).WithHeadTick(15);
        var lineage = new TimelineLineage([root, child]);
        var checkpoint = MakeCheckpoint("root", 5);

        Assert.Throws<ArgumentException>(() =>
            new TimelineReplayPlan(lineage, checkpoint, 8, "schema-1", "model-1"));
        Assert.Throws<ArgumentException>(() =>
            new TimelineReplayPlan(lineage, checkpoint, 12, "schema-2", "model-1"));
        Assert.Throws<ArgumentException>(() =>
            new TimelineReplayPlan(lineage, checkpoint, 12, "schema-1", "model-2"));
    }

    [Test]
    public void CopyIntervalsCannotMutatePlan()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var lineage = new TimelineLineage([root]);
        var plan = new TimelineReplayPlan(lineage, MakeCheckpoint("root", 5), 10, "schema-1", "model-1");
        var intervals = plan.CopyIntervals();

        Assert.Throws<NotSupportedException>(() =>
            ((IList<TimelineReplayInterval>)intervals).Add(new TimelineReplayInterval("other", 0, 1)));
        Assert.That(plan.CopyIntervals(), Has.Count.EqualTo(1));
    }

    private static void AssertInterval(
        TimelineReplayInterval interval,
        string timelineId,
        long startExclusive,
        long endInclusive)
    {
        Assert.That(interval.TimelineId, Is.EqualTo(timelineId));
        Assert.That(interval.StartExclusiveTick, Is.EqualTo(startExclusive));
        Assert.That(interval.EndInclusiveTick, Is.EqualTo(endInclusive));
    }

    private static PhysicsCheckpoint MakeCheckpoint(string timelineId, long tick) =>
        new("cp-" + timelineId + "-" + tick, "reality-1", timelineId, tick, "schema-1", "model-1", new byte[] { 1, 2, 3 });
}
