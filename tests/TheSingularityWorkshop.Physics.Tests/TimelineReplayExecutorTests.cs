using System.Buffers.Binary;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineReplayExecutorTests
{
    [Test]
    public void ReconstructAppliesOrderedEventsAndAdvancesToRequestedScrubTick()
    {
        TimelineReplaySegment segment = MakeSegment();

        int state = TimelineReplayExecutor.Reconstruct(
            segment,
            targetTick: 5,
            Restore,
            ApplyEvent,
            Advance);

        // Tick progression contributes 5; the events at ticks 2 and 4 contribute 3 and 10.
        Assert.That(state, Is.EqualTo(18));
    }

    [Test]
    public void ScrubbingBeforeLaterEventDoesNotApplyThatEvent()
    {
        TimelineReplaySegment segment = MakeSegment();

        int state = TimelineReplayExecutor.Reconstruct(
            segment,
            targetTick: 3,
            Restore,
            ApplyEvent,
            Advance);

        // Advance to tick 2, apply +3, then advance to tick 3. The tick-4 event is excluded.
        Assert.That(state, Is.EqualTo(6));
    }

    [Test]
    public void ReconstructRejectsTargetOutsideValidatedInterval()
    {
        TimelineReplaySegment segment = MakeSegment();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TimelineReplayExecutor.Reconstruct(
                segment,
                targetTick: 6,
                Restore,
                ApplyEvent,
                Advance));
    }

    private static TimelineReplaySegment MakeSegment()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(10);
        var checkpoint = new PhysicsCheckpoint(
            "cp-root-0", "reality-1", "root", 0, "schema-1", "model-1", Encode(0));
        var plan = new TimelineReplayPlan(new TimelineLineage([root]), checkpoint, 5, "schema-1", "model-1");
        var history = new TimelineHistorySegment(
            checkpoint,
            [
                new TimelineEvent(0, 2, "add", "3"),
                new TimelineEvent(1, 4, "add", "10")
            ],
            5);

        return new TimelineReplaySegment(plan, plan.CopyIntervals()[0], history);
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
