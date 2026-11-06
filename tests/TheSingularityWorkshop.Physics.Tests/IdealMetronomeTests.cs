using NUnit.Framework;
using TheSingularityWorkshop.Physics.Timing;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class IdealMetronomeTests
{
    [Test]
    public void GetEvents_ProducesBeatBoundariesAtDeclaredTempo()
    {
        var definition = new IdealMetronomeDefinition(
            "metronome-a", 120d, 0d, 1, 0d);

        var events = IdealMetronome.GetEvents(definition, 0d, 2.1d);

        Assert.That(events.Select(item => item.TimeSeconds),
            Is.EqualTo(new[] { 0.5d, 1d, 1.5d, 2d }).Within(1e-12));
        Assert.That(events.All(item => item.IsBeat), Is.True);
        Assert.That(events.Select(item => item.BeatIndex),
            Is.EqualTo(new long[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void GetEvents_UsesSubdivisionPhaseCrossings()
    {
        var definition = new IdealMetronomeDefinition(
            "metronome-b", 60d, 0d, 4, 0d);

        var events = IdealMetronome.GetEvents(definition, 0d, 1d);

        Assert.That(events.Select(item => item.TimeSeconds),
            Is.EqualTo(new[] { 0.25d, 0.5d, 0.75d }).Within(1e-12));
        Assert.That(events.Select(item => item.SubdivisionIndex),
            Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(events.Any(item => item.IsBeat), Is.False);
    }

    [Test]
    public void GetEvents_RespectsInitialPhaseAndStartTime()
    {
        var definition = new IdealMetronomeDefinition(
            "offset", 60d, Math.PI, 2, 5d);

        var events = IdealMetronome.GetEvents(definition, 5d, 7d);

        Assert.That(events.Select(item => item.TimeSeconds),
            Is.EqualTo(new[] { 5.5d, 6d, 6.5d }).Within(1e-12));
    }

    [Test]
    public void GetEvents_UsesHalfOpenIntervalsWithoutDuplicatingBoundaryEvents()
    {
        var definition = new IdealMetronomeDefinition(
            "boundary", 60d, 0d, 1, 0d);

        var first = IdealMetronome.GetEvents(definition, 0d, 1d);
        var second = IdealMetronome.GetEvents(definition, 1d, 2d);

        Assert.That(first.Select(item => item.TimeSeconds),
            Is.EqualTo(new[] { 1d }).Within(1e-12));
        Assert.That(second.Select(item => item.TimeSeconds),
            Is.EqualTo(new[] { 2d }).Within(1e-12));
        Assert.That(first[0].EventIndex, Is.EqualTo(1));
        Assert.That(second[0].EventIndex, Is.EqualTo(2));
    }

    [Test]
    public void GetEvents_IsDeterministicAcrossRepeatedQueries()
    {
        var definition = new IdealMetronomeDefinition(
            "repeatable", 137d, 0.37d, 8, 2d);

        var first = IdealMetronome.GetEvents(definition, 2d, 600d);
        var second = IdealMetronome.GetEvents(definition, 2d, 600d);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void GetEvents_RejectsInvalidDefinitionAndIntervals()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IdealMetronome.GetEvents(
                new IdealMetronomeDefinition("bad", 0d, 0d, 1, 0d), 0d, 1d));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IdealMetronome.GetEvents(
                new IdealMetronomeDefinition("bad", 60d, 0d, 0, 0d), 0d, 1d));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IdealMetronome.GetEvents(
                new IdealMetronomeDefinition("bad", 60d, 0d, 1, 2d), 1d, 3d));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IdealMetronome.GetEvents(
                new IdealMetronomeDefinition("bad", 60d, 0d, 1, 0d), 2d, 1d));
    }

    [Test]
    public void GetEvents_EnforcesExplicitEventBudget()
    {
        var definition = new IdealMetronomeDefinition(
            "bounded", 600d, 0d, 4, 0d);

        Assert.Throws<InvalidOperationException>(() =>
            IdealMetronome.GetEvents(definition, 0d, 10d, maxEvents: 8));
    }
}
