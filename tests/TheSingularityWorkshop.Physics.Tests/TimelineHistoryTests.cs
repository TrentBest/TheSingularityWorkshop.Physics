using System.Security.Cryptography;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineHistoryTests
{
    [Test]
    public void CheckpointCopiesPayloadAndCalculatesStableDigest()
    {
        byte[] payload = [1, 2, 3, 4];
        var checkpoint = new PhysicsCheckpoint("cp-1", "reality-1", "timeline-1", 42, "schema-1", "model-1", payload);
        var expectedDigest = Convert.ToHexString(SHA256.HashData(payload));

        payload[0] = 99;
        var firstCopy = checkpoint.CopyStatePayload();
        Assert.That(firstCopy, Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        Assert.That(checkpoint.StateSha256, Is.EqualTo(expectedDigest));

        firstCopy[1] = 99;
        Assert.That(checkpoint.CopyStatePayload(), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        Assert.That(checkpoint.PayloadLength, Is.EqualTo(4));
    }

    [Test]
    public void CheckpointRejectsMissingIdentityInvalidTickAndEmptyPayload()
    {
        byte[] payload = [1];

        Assert.Throws<ArgumentException>(() =>
            new PhysicsCheckpoint("", "r", "t", 0, "schema", "model", payload));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PhysicsCheckpoint("cp", "r", "t", -1, "schema", "model", payload));
        Assert.Throws<ArgumentException>(() =>
            new PhysicsCheckpoint("cp", "r", "t", 0, "schema", "model", ReadOnlySpan<byte>.Empty));
    }

    [Test]
    public void HistorySegmentAcceptsOrderedEventsAndReturnsDefensiveListCopy()
    {
        var checkpoint = MakeCheckpoint(10);
        var source = new List<TimelineEvent>
        {
            new(1, 10, "input", "start"),
            new(2, 10, "contact", "body-a:body-b"),
            new(3, 15, "input", "force"),
        };

        var segment = new TimelineHistorySegment(checkpoint, source, 20);
        source.Clear();

        Assert.That(segment.EventCount, Is.EqualTo(3));
        Assert.That(segment.CopyEvents().Select(e => e.Sequence), Is.EqualTo(new long[] { 1, 2, 3 }));
    }

    [Test]
    public void HistorySegmentRejectsEventsOutsideRangeOrOutOfOrder()
    {
        var checkpoint = MakeCheckpoint(10);

        Assert.Throws<ArgumentException>(() => new TimelineHistorySegment(
            checkpoint,
            [new TimelineEvent(1, 9, "input", "too-early")],
            20));

        Assert.Throws<ArgumentException>(() => new TimelineHistorySegment(
            checkpoint,
            [new TimelineEvent(1, 12, "input", "later"), new TimelineEvent(2, 11, "input", "earlier")],
            20));

        Assert.Throws<ArgumentException>(() => new TimelineHistorySegment(
            checkpoint,
            [new TimelineEvent(1, 12, "input", "first"), new TimelineEvent(1, 13, "input", "duplicate-sequence")],
            20));

        Assert.Throws<ArgumentException>(() => new TimelineHistorySegment(
            checkpoint,
            [new TimelineEvent(1, 21, "input", "past-target")],
            20));
    }

    [Test]
    public void HistoryEndCannotPrecedeCheckpoint()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TimelineHistorySegment(MakeCheckpoint(10), Array.Empty<TimelineEvent>(), 9));
    }

    [Test]
    public void TimelineEventValidatesSequenceTickKindAndPayload()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimelineEvent(-1, 0, "input", "x"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimelineEvent(0, -1, "input", "x"));
        Assert.Throws<ArgumentException>(() => new TimelineEvent(0, 0, "", "x"));
        Assert.Throws<ArgumentNullException>(() => new TimelineEvent(0, 0, "input", null!));
    }

    private static PhysicsCheckpoint MakeCheckpoint(long tick) =>
        new("cp-1", "reality-1", "timeline-1", tick, "schema-1", "model-1", new byte[] { 7, 8, 9 });
}
