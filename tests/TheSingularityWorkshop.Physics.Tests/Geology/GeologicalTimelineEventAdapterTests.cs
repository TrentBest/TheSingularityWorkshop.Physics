using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class GeologicalTimelineEventAdapterTests
{
    [Test]
    public void EncodeAndDecode_PreserveCompleteEventProvenanceAndStep()
    {
        GeologicalEventRecord original = MakeEvent(stepIndex: 12);
        TimelineEvent timelineEvent = GeologicalTimelineEventAdapter.Encode(sequence: 7, original);
        GeologicalEventRecord decoded = GeologicalTimelineEventAdapter.Decode(timelineEvent);

        Assert.That(timelineEvent.Sequence, Is.EqualTo(7));
        Assert.That(timelineEvent.Tick, Is.EqualTo(12));
        Assert.That(timelineEvent.Kind, Is.EqualTo(GeologicalTimelineEventAdapter.EventKind));
        Assert.That(decoded, Is.EqualTo(original));
    }

    [Test]
    public void Decode_RejectsDifferentTimelineEventKind()
    {
        var timelineEvent = new TimelineEvent(0, 12, "unrelated.event", "{}");
        Assert.Throws<ArgumentException>(() => GeologicalTimelineEventAdapter.Decode(timelineEvent));
    }

    [Test]
    public void Decode_RejectsTimelineTickThatDiffersFromGeologicalStep()
    {
        TimelineEvent encoded = GeologicalTimelineEventAdapter.Encode(0, MakeEvent(stepIndex: 12));
        var mismatched = new TimelineEvent(encoded.Sequence, 13, encoded.Kind, encoded.Payload);
        Assert.Throws<ArgumentException>(() => GeologicalTimelineEventAdapter.Decode(mismatched));
    }

    [Test]
    public void Decode_RejectsUnsupportedPayloadSchema()
    {
        var timelineEvent = new TimelineEvent(0, 12, GeologicalTimelineEventAdapter.EventKind,
            """{"schemaVersion":99,"event":{}}""");
        Assert.Throws<ArgumentException>(() => GeologicalTimelineEventAdapter.Decode(timelineEvent));
    }

    [Test]
    public void Decode_RejectsMalformedPayload()
    {
        var timelineEvent = new TimelineEvent(0, 12, GeologicalTimelineEventAdapter.EventKind,
            """{"schemaVersion":1,"event":""");
        Assert.Throws<ArgumentException>(() => GeologicalTimelineEventAdapter.Decode(timelineEvent));
    }


    [Test]
    public void GeologicalEvent_CanBeDecodedThroughValidatedTimelineReplay()
    {
        GeologicalEventRecord original = MakeEvent(stepIndex: 12);
        TimelineEvent encoded = GeologicalTimelineEventAdapter.Encode(sequence: 0, original);
        var checkpoint = new PhysicsCheckpoint(
            "checkpoint/start",
            "reality/earth",
            "timeline/main",
            tick: 0,
            "checkpoint-schema-1",
            "geology-model-1",
            new byte[] { 4 });
        TimelineBranch root = TimelineBranch.CreateRoot("timeline/main", "reality/earth")
            .WithHeadTick(12);
        var lineage = new TimelineLineage(new[] { root });
        var plan = new TimelineReplayPlan(
            lineage,
            checkpoint,
            targetTick: 12,
            requiredSchemaVersion: "checkpoint-schema-1",
            requiredModelVersion: "geology-model-1");
        TimelineReplayInterval interval = plan.CopyIntervals().Single();
        var history = new TimelineHistorySegment(checkpoint, new[] { encoded }, throughTick: 12);
        var segment = new TimelineReplaySegment(plan, interval, history);

        GeologicalEventRecord? appliedEvent = null;
        int reconstructedState = TimelineReplayExecutor.Reconstruct(
            segment,
            targetTick: 12,
            restore: saved => saved.CopyStatePayload()[0],
            applyEvent: (state, item) =>
            {
                appliedEvent = GeologicalTimelineEventAdapter.Decode(item);
                return state + 100;
            },
            advance: (state, fromTick, toTick) => state + checked((int)(toTick - fromTick)));

        Assert.That(reconstructedState, Is.EqualTo(116));
        Assert.That(appliedEvent, Is.EqualTo(original));
    }

    private static GeologicalEventRecord MakeEvent(long stepIndex) =>
        new(
            "event/erosion-001",
            "geomorphology.river-incision",
            new GeologicalTimeInterval(2_000_000d, 1_999_000d, 10d, 11d, stepIndex),
            "world/revision-20",
            "world/revision-21",
            "physics.geology.surface-processes",
            "0.1.0",
            "geomorphology.river-incision",
            "1.0.0",
            """{"catchmentId":"basin-7","waterVolumeM3":1250.5}""",
            "River incision modifies the valley floor.");
}
