using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class AnalyticalSolidVolume3dTests
{
    [Test]
    public void CreateEllipsoid_UsesVoxelCentersAndAnalyticInsideOutsideRule()
    {
        var volume = AnalyticalSolidVolume3d.CreateEllipsoid(
            origin: new Vector3d(-2d, -2d, -2d),
            voxelSize: 1d,
            sizeX: 4,
            sizeY: 4,
            sizeZ: 4,
            center: Vector3d.Zero,
            semiAxes: new Vector3d(2d, 2d, 2d),
            materialId: 7,
            chunkSize: 2);

        Assert.That(volume.SolidVoxelCount, Is.EqualTo(32));
        Assert.That(volume.GetMaterialId(new GridCoordinate3i(1, 1, 1)), Is.EqualTo(7));
        Assert.That(volume.GetMaterialId(new GridCoordinate3i(0, 0, 0)), Is.Zero);
        Assert.That(volume.GetMaterialId(new GridCoordinate3i(3, 3, 3)), Is.Zero);
        Assert.That(volume.GetMaterialVoxelCount(7), Is.EqualTo(volume.SolidVoxelCount));
    }

    [Test]
    public void CreateEllipsoid_IsDeterministicAcrossRepeatedGeneration()
    {
        SparseMaterialVoxelVolume3d Create() => AnalyticalSolidVolume3d.CreateEllipsoid(
            origin: new Vector3d(-3d, -2d, -1d),
            voxelSize: 0.5d,
            sizeX: 12,
            sizeY: 8,
            sizeZ: 4,
            center: new Vector3d(0d, 0d, 0d),
            semiAxes: new Vector3d(2.5d, 1.5d, 0.75d),
            materialId: 3,
            chunkSize: 4);

        var first = Create().CaptureSnapshot();
        var second = Create().CaptureSnapshot();

        Assert.That(second.Origin, Is.EqualTo(first.Origin));
        Assert.That(second.VoxelSize, Is.EqualTo(first.VoxelSize));
        Assert.That(second.Samples, Is.EqualTo(first.Samples));
    }

    [Test]
    public void CreateEllipsoid_RejectsInvalidInputsBeforeReturningVolume()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AnalyticalSolidVolume3d.CreateEllipsoid(
            Vector3d.Zero, 1d, 0, 2, 2, Vector3d.Zero, new Vector3d(1d, 1d, 1d), 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AnalyticalSolidVolume3d.CreateEllipsoid(
            Vector3d.Zero, 1d, 2, 2, 2, Vector3d.Zero, new Vector3d(1d, 0d, 1d), 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AnalyticalSolidVolume3d.CreateEllipsoid(
            Vector3d.Zero, 1d, 2, 2, 2, Vector3d.Zero, new Vector3d(1d, 1d, 1d), 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AnalyticalSolidVolume3d.CreateEllipsoid(
            Vector3d.Zero, 1d, 10, 10, 10, Vector3d.Zero, new Vector3d(1d, 1d, 1d), 1,
            maximumCandidateVoxelCount: 999));
    }

    [Test]
    public void PublishedAnalyticalFixture_MatchesTheGeneratorAndVersionedEnvelope()
    {
        var path = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Records",
            "analytical-ellipsoid-volume-example.json");
        var envelope = SparseMaterialVoxelVolumeEnvelope3d.Deserialize(File.ReadAllText(path));
        var generated = AnalyticalSolidVolume3d.CreateEllipsoid(
            envelope.Snapshot.Origin,
            envelope.Snapshot.VoxelSize,
            sizeX: 4,
            sizeY: 4,
            sizeZ: 4,
            center: Vector3d.Zero,
            semiAxes: new Vector3d(2d, 2d, 2d),
            materialId: 1,
            chunkSize: envelope.Snapshot.ChunkSize).CaptureSnapshot();

        Assert.That(envelope.DatasetId, Is.EqualTo("terrain.ellipsoid.reference.v1"));
        Assert.That(envelope.ModelId, Is.EqualTo("model.analytic-ellipsoid-solid"));
        Assert.That(envelope.Snapshot.SolidVoxelCount, Is.EqualTo(32));
        Assert.That(envelope.Snapshot.Samples, Is.EqualTo(generated.Samples));
        Assert.That(envelope.GenerationParametersJson, Does.Contain("\"notGeologicalEvidence\":true"));
    }
}
