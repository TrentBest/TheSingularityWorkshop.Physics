using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class GeologicalTerrainRevisionStore2DTests
{
    private static TerrainHeightField2D Terrain(double elevation = 10d) =>
        new(3, 3, 1d, 1d, -1d, -1d, "local", "sea-level", "m", "up",
            Enumerable.Repeat(elevation, 9).ToArray());

    private static GeologicalEventRecord Event(string source = "r0", string result = "r1") =>
        new("event-001", "geology.surface-uplift",
            new GeologicalTimeInterval(1_000_000d, 999_000d, 0d, 1d, 1),
            source, result, "model.terrain-test", "1.0",
            "algorithm.uniform-uplift", "1.0", "{\"delta\":2}", "Uniform prescribed uplift");

    private static GeologicalTerrainCandidate2D Candidate(
        GeologicalTerrainRevision2D source,
        string result = "r1",
        GeologicalEventRecord? geologicalEvent = null) =>
        GeologicalTerrainCandidate2D.Create(
            source, result, geologicalEvent ?? Event(source.RevisionId, result),
            terrain => terrain.ApplyRadialBrush(0d, 0d, 2d, 2d));

    [Test]
    public void Create_BuildsCandidateWithoutChangingSourceOrStore()
    {
        var source = new GeologicalTerrainRevision2D("r0", Terrain());
        var store = new GeologicalTerrainRevisionStore2D(source);

        var candidate = Candidate(source);

        Assert.That(store.Current, Is.SameAs(source));
        Assert.That(source.Terrain.SampleCell(1, 1), Is.EqualTo(10d));
        Assert.That(candidate.Terrain.SampleCell(1, 1), Is.EqualTo(12d));
        Assert.That(candidate.GeologicalEvent.SourceRevisionId, Is.EqualTo("r0"));
        Assert.That(candidate.GeologicalEvent.ResultRevisionId, Is.EqualTo("r1"));
    }

    [Test]
    public void Create_RejectsEventRevisionMismatchAndGeometryChanges()
    {
        var source = new GeologicalTerrainRevision2D("r0", Terrain());
        Assert.Throws<ArgumentException>(() => Candidate(source, geologicalEvent: Event("other", "r1")));
        Assert.Throws<InvalidOperationException>(() => GeologicalTerrainCandidate2D.Create(
            source, "r1", Event(), _ => Terrain(12d) is var t
                ? new TerrainHeightField2D(2, 2, 1d, 1d, -1d, -1d, "local", "sea-level", "m", "up",
                    new[] { 1d, 1d, 1d, 1d })
                : t));
    }

    [Test]
    public void Create_RejectsDomainValidationWithoutMutatingSource()
    {
        var source = new GeologicalTerrainRevision2D("r0", Terrain());
        Assert.Throws<InvalidOperationException>(() => GeologicalTerrainCandidate2D.Create(
            source, "r1", Event(), terrain => terrain.ApplyRadialBrush(0d, 0d, 2d, 2d),
            _ => false));
        Assert.That(source.Terrain.SampleCell(1, 1), Is.EqualTo(10d));
    }

    [Test]
    public void TryCommit_AdvancesCurrentRevisionAndPreservesParentage()
    {
        var source = new GeologicalTerrainRevision2D("r0", Terrain());
        var store = new GeologicalTerrainRevisionStore2D(source);
        var candidate = Candidate(source);

        Assert.That(store.TryCommit(candidate, "r0", out var committed), Is.True);
        Assert.That(store.Current, Is.SameAs(committed));
        Assert.That(committed.RevisionId, Is.EqualTo("r1"));
        Assert.That(committed.ParentRevisionId, Is.EqualTo("r0"));
        Assert.That(committed.AppliedEvent, Is.SameAs(candidate.GeologicalEvent));
        Assert.That(committed.Terrain.SampleCell(1, 1), Is.EqualTo(12d));
    }

    [Test]
    public void TryCommit_RejectsStaleConcurrentCandidateWithoutChangingCurrent()
    {
        var source = new GeologicalTerrainRevision2D("r0", Terrain());
        var store = new GeologicalTerrainRevisionStore2D(source);
        var first = Candidate(source, "r1");
        var competing = Candidate(source, "r2");

        Assert.That(store.TryCommit(first, "r0", out var firstCommit), Is.True);
        Assert.That(store.TryCommit(competing, "r0", out var rejectedCurrent), Is.False);
        Assert.That(rejectedCurrent, Is.SameAs(firstCommit));
        Assert.That(store.Current, Is.SameAs(firstCommit));
    }

    [Test]
    public void TryCommit_RejectsWrongExpectedRevisionAndReusedRevisionIdentity()
    {
        var source = new GeologicalTerrainRevision2D("r0", Terrain());
        var store = new GeologicalTerrainRevisionStore2D(source);
        var candidate = Candidate(source);

        Assert.That(store.TryCommit(candidate, "stale", out var unchanged), Is.False);
        Assert.That(unchanged, Is.SameAs(source));
        Assert.That(store.TryCommit(candidate, "r0", out var committed), Is.True);

        var next = Candidate(committed, "r0", Event("r1", "r0"));
        Assert.That(store.TryCommit(next, "r1", out var stillCurrent), Is.False);
        Assert.That(stillCurrent, Is.SameAs(committed));
    }
}
