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
