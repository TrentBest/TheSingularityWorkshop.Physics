using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class GeologicalVoxelProvenance3dTests
{
    private static SparseMaterialVoxelVolumeEnvelope3d CreateEnvelope(out GridCoordinate3i first, out GridCoordinate3i second)
    {
        first = new GridCoordinate3i(0, 0, 0);
        second = new GridCoordinate3i(1, 0, 0);
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        volume.SetMaterial(first, 4);
        volume.SetMaterial(second, 7);
        return new SparseMaterialVoxelVolumeEnvelope3d(
            volume.CaptureSnapshot(), "region", "dataset", "revision-1",
            "world", "m", "x-east;y-north;z-up", "sea-level", "up",
            "rock-catalog-v1", "repo", "source-revision", "geology-model", "1",
            "voxel-generator", "1", "{}", "run-1");
    }

    private static GeologicalVoxelProvenanceSample3d[] Samples(GridCoordinate3i first, GridCoordinate3i second) =>
    [
        new(first, "layer-granite", 4, "intrusion-event"),
        new(second, "layer-basalt", 7, "lava-event")
    ];

    [Test]
    public void Constructor_BindsCompleteVoxelProvenanceToExactVolumeRevision()
    {
        var envelope = CreateEnvelope(out var first, out var second);
        var mapping = new GeologicalVoxelProvenance3d(envelope, Samples(first, second));

        Assert.Multiple(() =>
        {
            Assert.That(mapping.Envelope, Is.SameAs(envelope));
            Assert.That(mapping.Samples.Count, Is.EqualTo(2));
            Assert.That(mapping.TryGet(first, out var granite), Is.True);
            Assert.That(granite.LayerId, Is.EqualTo("layer-granite"));
            Assert.That(granite.MaterialId, Is.EqualTo(4));
            Assert.That(granite.FormationEventId, Is.EqualTo("intrusion-event"));
            Assert.That(mapping.TryGet(second, out var basalt), Is.True);
            Assert.That(basalt.LayerId, Is.EqualTo("layer-basalt"));
            Assert.That(mapping.TryGet(new GridCoordinate3i(9, 9, 9), out _), Is.False);
        });
    }

    [Test]
    public void Constructor_RequiresExactlyOneAnnotationPerOccupiedVoxel()
    {
        var envelope = CreateEnvelope(out var first, out var second);
        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelProvenance3d(envelope, new[] { new GeologicalVoxelProvenanceSample3d(first, "layer", 4, "event") }));
        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelProvenance3d(envelope, new[]
            {
                new GeologicalVoxelProvenanceSample3d(first, "layer", 4, "event"),
                new GeologicalVoxelProvenanceSample3d(first, "layer", 4, "event")
            }));
    }

    [Test]
    public void Constructor_RejectsEmptyVoxelsAndMaterialMismatch()
    {
        var envelope = CreateEnvelope(out var first, out var second);
        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelProvenance3d(envelope, new[]
            {
                new GeologicalVoxelProvenanceSample3d(first, "layer-a", 4, "event-a"),
                new GeologicalVoxelProvenanceSample3d(new GridCoordinate3i(5, 5, 5), "layer-b", 7, "event-b")
            }));
        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelProvenance3d(envelope, new[]
            {
                new GeologicalVoxelProvenanceSample3d(first, "layer-a", 7, "event-a"),
                new GeologicalVoxelProvenanceSample3d(second, "layer-b", 7, "event-b")
            }));
    }

    [Test]
    public void Constructor_RejectsMissingLayerOrEventIdentity()
    {
        var envelope = CreateEnvelope(out var first, out var second);
        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelProvenance3d(envelope, new[]
            {
                new GeologicalVoxelProvenanceSample3d(first, "", 4, "event-a"),
                new GeologicalVoxelProvenanceSample3d(second, "layer-b", 7, "event-b")
            }));
        Assert.Throws<ArgumentException>(() =>
            new GeologicalVoxelProvenance3d(envelope, new[]
            {
                new GeologicalVoxelProvenanceSample3d(first, "layer-a", 4, ""),
                new GeologicalVoxelProvenanceSample3d(second, "layer-b", 7, "event-b")
            }));
    }

    [Test]
    public void Constructor_DefensivelyCopiesAndSortsSamples()
    {
        var envelope = CreateEnvelope(out var first, out var second);
        var source = new[]
        {
            new GeologicalVoxelProvenanceSample3d(second, "layer-b", 7, "event-b"),
            new GeologicalVoxelProvenanceSample3d(first, "layer-a", 4, "event-a")
        };
        var mapping = new GeologicalVoxelProvenance3d(envelope, source);
        source[0] = new GeologicalVoxelProvenanceSample3d(second, "replacement", 7, "other-event");

        Assert.That(mapping.Samples[0].Voxel, Is.EqualTo(first));
        Assert.That(mapping.Samples[1].LayerId, Is.EqualTo("layer-b"));
    }
}
