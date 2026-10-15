using System.Buffers.Binary;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineReplayRouteExecutorTests
{
    [Test]
    public void ReconstructCarriesStateAcrossForkAndValidatesBoundaryCheckpoint()
    {
        TimelineReplayPlan plan = MakePlan();
        IReadOnlyList<TimelineReplaySegment> segments = MakeSegments(plan, childCheckpointValue: 44);

        int result = TimelineReplayRouteExecutor.Reconstruct(
            plan, segments, Restore, ApplyEvent, Advance, (left, right) => left == right);

        // Parent reaches fork tick 4 with value 44; child advances to tick 6, adds 7, then advances to tick 7.
        Assert.That(result, Is.EqualTo(54));
    }

    [Test]
    public void ReconstructRejectsForkCheckpointThatDoesNotMatchInheritedState()
    {
        TimelineReplayPlan plan = MakePlan();
        IReadOnlyList<TimelineReplaySegment> segments = MakeSegments(plan, childCheckpointValue: 999);

        Assert.Throws<InvalidOperationException>(() =>
            TimelineReplayRouteExecutor.Reconstruct(
                plan, segments, Restore, ApplyEvent, Advance, (left, right) => left == right));
    }

    [Test]
    public void ReconstructRequiresEveryIntervalInPlanOrder()
    {
        TimelineReplayPlan plan = MakePlan();
        IReadOnlyList<TimelineReplaySegment> segments = MakeSegments(plan, childCheckpointValue: 40);

        Assert.Throws<ArgumentException>(() =>
            TimelineReplayRouteExecutor.Reconstruct(
                plan, [segments[1]], Restore, ApplyEvent, Advance, (left, right) => left == right));
    }

    private static TimelineReplayPlan MakePlan()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(10);
        var child = TimelineBranch.Fork("child", root, 4).WithHeadTick(7);
        var lineage = new TimelineLineage([root, child]);
        var checkpoint = new PhysicsCheckpoint("root-0", "reality-1", "root", 0, "schema-1", "model-1", Encode(0));
        return new TimelineReplayPlan(lineage, checkpoint, 7, "schema-1", "model-1");
    }

    private static IReadOnlyList<TimelineReplaySegment> MakeSegments(TimelineReplayPlan plan, int childCheckpointValue)
    {
        var rootHistory = new TimelineHistorySegment(
            plan.Checkpoint,
            [new TimelineEvent(0, 2, "add", "20"), new TimelineEvent(1, 4, "add", "20")],
            4);
        var childCheckpoint = new PhysicsCheckpoint(
            "child-4", "reality-1", "child", 4, "schema-1", "model-1", Encode(childCheckpointValue));
        var childHistory = new TimelineHistorySegment(
            childCheckpoint,
            [new TimelineEvent(0, 6, "add", "7")],
            7);

        var intervals = plan.CopyIntervals();
        return [
            new TimelineReplaySegment(plan, intervals[0], rootHistory),
            new TimelineReplaySegment(plan, intervals[1], childHistory)
        ];
    }

    private static int Restore(PhysicsCheckpoint checkpoint) =>
        BinaryPrimitives.ReadInt32LittleEndian(checkpoint.CopyStatePayload());

    private static int ApplyEvent(int state, TimelineEvent item) =>
        state + int.Parse(item.Payload, System.Globalization.CultureInfo.InvariantCulture);

    private static int Advance(int state, long fromTick, long toTick) =>
        state + checked((int)(toTick - fromTick));

    private static byte[] Encode(int value)
    {
        var bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        return bytes;
    }
}
