using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class MountainSolidVolume3dTests
{
    private static TerrainHeightField2D CreateTerrain() => new(
        width: 3, height: 2, cellWidth: 1d, cellHeight: 1d,
        originX: 10d, originY: 20d,
        coordinateFrameId: "local-mountain",
        verticalDatum: "model-base",
        linearUnit: "metre",
        positiveVerticalDirection: "up",
        elevations: new[] { 2d, 3d, 4d, 2d, 3d, 4d });

    private static SparseMaterialVoxelVolumeEnvelope3d Generate() => MountainSolidVolume3d.Generate(
        CreateTerrain(), baseElevation: 0d, materialId: 7,
        regionId: "fortress-region", datasetId: "mountain.test.v1", revisionId: "r1",
        materialCatalogId: "rock.catalog.v1", sourceRepository: "TheSingularityWorkshop.Physics",
        sourceRevision: "test-revision", sourceRunId: "mountain-run-001", chunkSize: 2);

    [Test]
    public void Generate_AlignsHorizontalVoxelCentresAndBoundsSolidByHeightField()
    {
        var result = Generate();
        var snapshot = result.Snapshot;

        Assert.That(snapshot.Origin, Is.EqualTo(new Vector3d(9.5d, 19.5d, 0d)));
        Assert.That(snapshot.VoxelSize, Is.EqualTo(1d));
        Assert.That(snapshot.SolidVoxelCount, Is.EqualTo(18));
        Assert.That(snapshot.GetMaterialId(new GridCoordinate3i(0, 0, 0)), Is.EqualTo(7));
        Assert.That(snapshot.GetMaterialId(new GridCoordinate3i(0, 0, 1)), Is.EqualTo(7));
        Assert.That(snapshot.GetMaterialId(new GridCoordinate3i(0, 0, 2)), Is.Zero);
        Assert.That(snapshot.GetMaterialId(new GridCoordinate3i(2, 1, 3)), Is.EqualTo(7));
        Assert.That(snapshot.GetMaterialId(new GridCoordinate3i(2, 1, 4)), Is.Zero);
        Assert.That(result.CoordinateFrameId, Is.EqualTo("local-mountain"));
        Assert.That(result.AxisConvention, Does.Contain("handedness=caller-declared-not-inferred"));
        Assert.That(result.VerticalDatum, Is.EqualTo("model-base"));
        Assert.That(result.AlgorithmId, Is.EqualTo(MountainSolidVolume3d.AlgorithmId));
        Assert.That(result.GenerationParametersJson, Does.Contain("\"topBoundaryMaximumQuantizationError\":0.5"));
    }

    [Test]
    public void Generate_IsDeterministicAndProducesVersionedRoundTrip()
    {
        var first = Generate();
        var second = Generate();

        Assert.That(second.Snapshot.Samples, Is.EqualTo(first.Snapshot.Samples));
        Assert.That(second.Serialize(), Is.EqualTo(first.Serialize()));
        var restored = SparseMaterialVoxelVolumeEnvelope3d.Deserialize(first.Serialize());
        Assert.That(restored.Snapshot.Samples, Is.EqualTo(first.Snapshot.Samples));
        Assert.That(restored.DatasetId, Is.EqualTo(first.DatasetId));
    }

    [Test]
    public void Generate_RejectsUnsupportedSpacingAndCandidateBudget()
    {
        var nonCubic = new TerrainHeightField2D(2, 2, 1d, 2d, 0d, 0d,
            "frame", "datum", "m", "up", new[] { 1d, 1d, 1d, 1d });
        Assert.Throws<ArgumentException>(() => MountainSolidVolume3d.Generate(
            nonCubic, 0d, 1, "r", "d", "v", "m", "repo", "rev", "run"));

        Assert.Throws<ArgumentOutOfRangeException>(() => MountainSolidVolume3d.Generate(
            CreateTerrain(), 0d, 1, "r", "d", "v", "m", "repo", "rev", "run",
            maximumCandidateVoxelCount: 1));
    }

    [Test]
    public void Generate_RejectsInvalidMaterialAndBaseElevation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MountainSolidVolume3d.Generate(
            CreateTerrain(), 0d, 0, "r", "d", "v", "m", "repo", "rev", "run"));
        Assert.Throws<ArgumentException>(() => MountainSolidVolume3d.Generate(
            CreateTerrain(), 4d, 1, "r", "d", "v", "m", "repo", "rev", "run"));
    }
}
