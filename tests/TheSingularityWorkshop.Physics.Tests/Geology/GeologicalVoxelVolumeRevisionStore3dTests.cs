using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class GeologicalVoxelVolumeRevisionStore3dTests
{
    private static SparseMaterialVoxelVolumeEnvelope3d CreateEnvelope(
        string revisionId,
        int materialId = 4,
        GridCoordinate3i? voxel = null)
    {
        var volume = new SparseMaterialVoxelVolume3d(new Vector3d(-10d, 20d, -5d), 0.5d, 8);
        volume.SetMaterial(voxel ?? new GridCoordinate3i(0, 0, 0), materialId);
        return new SparseMaterialVoxelVolumeEnvelope3d(
            volume.CaptureSnapshot(), "region", "dataset", revisionId,
            "frame", "m", "X=east;Y=north;Z=up", "datum", "up",
            "materials.v1", "repo", "source", "model", "1",
            "algorithm", "1", "{}", "run");
    }

    private static GeologicalVoxelProvenance3d CreateProvenance(
        SparseMaterialVoxelVolumeEnvelope3d envelope,
        string layerId = "layer",
        string formationEventId = "formation")
    {
        var samples = envelope.Snapshot.Samples
            .Select(sample => new GeologicalVoxelProvenanceSample3d(
                sample.Voxel, layerId, sample.MaterialId, formationEventId))
            .ToArray();
        return new GeologicalVoxelProvenance3d(envelope, samples);
    }

    private static GeologicalEventRecord CreateEvent(string sourceId, string resultId) =>
        new("event-" + resultId, "geology.test", new GeologicalTimeInterval(100d, 99d, 0d, 1d, 1),
            sourceId, resultId, "model", "1", "algorithm", "1", "{}", "Test geological transition");

    [Test]
    public void Revision_RequiresProvenanceBoundToExactEnvelope()
    {
        var first = CreateEnvelope("revision-1");
        var other = CreateEnvelope("revision-1");
        var provenance = CreateProvenance(other);

        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelVolumeRevision3d(first, provenance));
        Assert.That(new GeologicalVoxelVolumeRevision3d(first, CreateProvenance(first)).RevisionId,
            Is.EqualTo("revision-1"));
    }

    [Test]
    public void Candidate_RequiresMatchingEventAndPreservesDatasetGridContract()
    {
        var sourceEnvelope = CreateEnvelope("revision-1");
        var source = new GeologicalVoxelVolumeRevision3d(sourceEnvelope, CreateProvenance(sourceEnvelope));
        var resultEnvelope = CreateEnvelope("revision-2", materialId: 7);
        var resultProvenance = CreateProvenance(resultEnvelope, "layer-new", "event-new");

        Assert.Throws<ArgumentException>(() =>
            GeologicalVoxelVolumeCandidate3d.Create(
                source, resultEnvelope, resultProvenance, CreateEvent("wrong-source", "revision-2")));
        Assert.Throws<ArgumentException>(() =>
            GeologicalVoxelVolumeCandidate3d.Create(
                source, resultEnvelope, resultProvenance, CreateEvent("revision-1", "wrong-result")));

        var changedGrid = CreateEnvelope("revision-2", voxel: new GridCoordinate3i(1, 0, 0));
        Assert.Throws<InvalidOperationException>(() =>
            GeologicalVoxelVolumeCandidate3d.Create(
                source, changedGrid, CreateProvenance(changedGrid), CreateEvent("revision-1", "revision-2")));

        var candidate = GeologicalVoxelVolumeCandidate3d.Create(
            source, resultEnvelope, resultProvenance, CreateEvent("revision-1", "revision-2"));
        Assert.That(candidate.SourceRevision, Is.SameAs(source));
        Assert.That(candidate.Provenance.Envelope, Is.SameAs(candidate.Envelope));
    }

    [Test]
    public void TryCommit_AtomicallyAdvancesEnvelopeAndProvenanceTogether()
    {
        var sourceEnvelope = CreateEnvelope("revision-1");
        var source = new GeologicalVoxelVolumeRevision3d(sourceEnvelope, CreateProvenance(sourceEnvelope));
        var store = new GeologicalVoxelVolumeRevisionStore3d(source);
        var resultEnvelope = CreateEnvelope("revision-2", materialId: 7);
        var resultProvenance = CreateProvenance(resultEnvelope, "layer-2", "event-2");
        var candidate = GeologicalVoxelVolumeCandidate3d.Create(
            source, resultEnvelope, resultProvenance, CreateEvent("revision-1", "revision-2"));

        var committed = store.TryCommit(candidate, "revision-1", out var revision);

        Assert.Multiple(() =>
        {
            Assert.That(committed, Is.True);
            Assert.That(revision, Is.SameAs(store.Current));
            Assert.That(revision.ParentRevisionId, Is.EqualTo("revision-1"));
            Assert.That(revision.AppliedEvent, Is.SameAs(candidate.GeologicalEvent));
            Assert.That(revision.Envelope, Is.SameAs(resultEnvelope));
            Assert.That(revision.Provenance, Is.SameAs(resultProvenance));
            Assert.That(revision.Provenance.Envelope, Is.SameAs(revision.Envelope));
            Assert.That(revision.Envelope.Snapshot.GetMaterialId(new GridCoordinate3i(0, 0, 0)), Is.EqualTo(7));
            Assert.That(revision.Provenance.TryGet(new GridCoordinate3i(0, 0, 0), out var sample), Is.True);
            Assert.That(sample.LayerId, Is.EqualTo("layer-2"));
        });
    }

    [Test]
    public void TryCommit_RejectsStaleCompetingCandidateWithoutChangingCurrentRevision()
    {
        var sourceEnvelope = CreateEnvelope("revision-1");
        var source = new GeologicalVoxelVolumeRevision3d(sourceEnvelope, CreateProvenance(sourceEnvelope));
        var store = new GeologicalVoxelVolumeRevisionStore3d(source);
        var firstEnvelope = CreateEnvelope("revision-2", materialId: 7);
        var secondEnvelope = CreateEnvelope("revision-3", materialId: 9);
        var first = GeologicalVoxelVolumeCandidate3d.Create(
            source, firstEnvelope, CreateProvenance(firstEnvelope), CreateEvent("revision-1", "revision-2"));
        var second = GeologicalVoxelVolumeCandidate3d.Create(
            source, secondEnvelope, CreateProvenance(secondEnvelope), CreateEvent("revision-1", "revision-3"));

        Assert.That(store.TryCommit(first, "revision-1", out _), Is.True);
        Assert.That(store.TryCommit(second, "revision-1", out var rejected), Is.False);
        Assert.That(rejected, Is.SameAs(store.Current));
        Assert.That(store.Current.RevisionId, Is.EqualTo("revision-2"));
        Assert.That(store.Current.Provenance.Envelope, Is.SameAs(store.Current.Envelope));
    }

    [Test]
    public void TryCommit_RejectsWrongExpectedSourceAndReusedRevisionIdentity()
    {
        var sourceEnvelope = CreateEnvelope("revision-1");
        var source = new GeologicalVoxelVolumeRevision3d(sourceEnvelope, CreateProvenance(sourceEnvelope));
        var store = new GeologicalVoxelVolumeRevisionStore3d(source);
        var resultEnvelope = CreateEnvelope("revision-2", materialId: 7);
        var candidate = GeologicalVoxelVolumeCandidate3d.Create(
            source, resultEnvelope, CreateProvenance(resultEnvelope), CreateEvent("revision-1", "revision-2"));

        Assert.That(store.TryCommit(candidate, "wrong-source", out var rejected), Is.False);
        Assert.That(rejected, Is.SameAs(source));
        Assert.That(store.TryCommit(candidate, "revision-1", out _), Is.True);
        Assert.That(store.TryCommit(candidate, "revision-2", out var duplicate), Is.False);
        Assert.That(duplicate, Is.SameAs(store.Current));
        Assert.That(store.Current.RevisionId, Is.EqualTo("revision-2"));
    }
}
