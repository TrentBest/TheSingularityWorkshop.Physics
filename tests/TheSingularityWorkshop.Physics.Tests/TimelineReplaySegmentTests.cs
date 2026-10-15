using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineReplaySegmentTests
{
    [Test]
    public void SegmentAcceptsEventsInsideItsTimelineOwnedInterval()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var child = TimelineBranch.Fork("child", root, 10).WithHeadTick(15);
        var lineage = new TimelineLineage([root, child]);
        var rootCheckpoint = MakeCheckpoint("root", 5);
        var plan = new TimelineReplayPlan(lineage, rootCheckpoint, 15, "schema-1", "model-1");
        var childInterval = plan.CopyIntervals()[1];
        var childCheckpoint = MakeCheckpoint("child", 10);
        var history = new TimelineHistorySegment(
            childCheckpoint,
            [new TimelineEvent(0, 11, "input", "move"), new TimelineEvent(1, 15, "input", "jump")],
            15);

        var segment = new TimelineReplaySegment(plan, childInterval, history);

        Assert.That(segment.EventCount, Is.EqualTo(2));
        Assert.That(segment.CopyEvents().Select(item => item.Tick), Is.EqualTo(new long[] { 11, 15 }));
    }

    [Test]
    public void SegmentRejectsCheckpointFromAnotherTimeline()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var child = TimelineBranch.Fork("child", root, 10).WithHeadTick(15);
        var plan = new TimelineReplayPlan(
            new TimelineLineage([root, child]), MakeCheckpoint("root", 5), 15, "schema-1", "model-1");
        var interval = plan.CopyIntervals()[1];
        var history = new TimelineHistorySegment(MakeCheckpoint("root", 10), [], 15);

        Assert.Throws<ArgumentException>(() => new TimelineReplaySegment(plan, interval, history));
    }

    [Test]
    public void SegmentRejectsEventAtExclusiveStartTick()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var plan = new TimelineReplayPlan(
            new TimelineLineage([root]), MakeCheckpoint("root", 5), 10, "schema-1", "model-1");
        var interval = plan.CopyIntervals()[0];
        var history = new TimelineHistorySegment(
            MakeCheckpoint("root", 5), [new TimelineEvent(0, 5, "input", "stale")], 10);

        Assert.Throws<ArgumentException>(() => new TimelineReplaySegment(plan, interval, history));
    }

    [Test]
    public void SegmentRejectsHistoryThatDoesNotEndAtIntervalBoundary()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var plan = new TimelineReplayPlan(
            new TimelineLineage([root]), MakeCheckpoint("root", 5), 10, "schema-1", "model-1");
        var interval = plan.CopyIntervals()[0];
        var history = new TimelineHistorySegment(MakeCheckpoint("root", 5), [], 9);

        Assert.Throws<ArgumentException>(() => new TimelineReplaySegment(plan, interval, history));
    }

    private static PhysicsCheckpoint MakeCheckpoint(string timelineId, long tick) =>
        new("cp-" + timelineId + "-" + tick, "reality-1", timelineId, tick, "schema-1", "model-1", new byte[] { 1, 2, 3 });
}
