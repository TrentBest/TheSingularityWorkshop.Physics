using System.Text.Json.Nodes;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class SparseMaterialVoxelVolumeEnvelope3dTests
{
    [Test]
    public void Constructor_PreservesSnapshotAndDeclaredSpatialProvenance()
    {
        var volume = new SparseMaterialVoxelVolume3d(new Vector3d(-10d, 20d, -5d), 0.25d);
        volume.SetMaterial(new GridCoordinate3i(-2, 3, 4), 7);
        var snapshot = volume.CaptureSnapshot();

        var envelope = CreateEnvelope(snapshot);

        Assert.That(envelope.Snapshot, Is.SameAs(snapshot));
        Assert.That(envelope.RegionId, Is.EqualTo("region.test"));
        Assert.That(envelope.DatasetId, Is.EqualTo("terrain.volume.test"));
        Assert.That(envelope.RevisionId, Is.EqualTo("revision.1"));
        Assert.That(envelope.CoordinateFrameId, Is.EqualTo("frame.local.east-north-up"));
        Assert.That(envelope.LinearUnit, Is.EqualTo("m"));
        Assert.That(envelope.AxisConvention, Is.EqualTo("X=east;Y=north;Z=up"));
        Assert.That(envelope.VerticalDatum, Is.EqualTo("model-relative"));
        Assert.That(envelope.VerticalPositiveDirection, Is.EqualTo("up"));
        Assert.That(envelope.MaterialCatalogId, Is.EqualTo("materials.test.v1"));
        Assert.That(envelope.SourceRevision, Is.EqualTo(new string('a', 40)));
        Assert.That(envelope.Seed, Is.EqualTo(12345L));
        Assert.That(envelope.Snapshot.GetMaterialId(new GridCoordinate3i(-2, 3, 4)), Is.EqualTo(7));
        Assert.That(envelope.Snapshot.Origin, Is.EqualTo(new Vector3d(-10d, 20d, -5d)));
        Assert.That(envelope.Snapshot.VoxelSize, Is.EqualTo(0.25d));
    }

    [Test]
    public void EnvelopeSnapshot_RemainsIndependentOfLaterVolumeEdits()
    {
        var volume = new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, 0d), 1d);
        volume.SetMaterial(new GridCoordinate3i(0, 0, 0), 2);
        var envelope = CreateEnvelope(volume.CaptureSnapshot());

        volume.SetMaterial(new GridCoordinate3i(0, 0, 0), 9);
        volume.SetMaterial(new GridCoordinate3i(1, 0, 0), 4);

        Assert.That(envelope.Snapshot.GetMaterialId(new GridCoordinate3i(0, 0, 0)), Is.EqualTo(2));
        Assert.That(envelope.Snapshot.GetMaterialId(new GridCoordinate3i(1, 0, 0)), Is.Zero);
    }

    [Test]
    public void SerializeAndDeserialize_RoundTripPreservesSpatialMetadataAndSamples()
    {
        var volume = new SparseMaterialVoxelVolume3d(new Vector3d(-10d, 20d, -5d), 0.25d, chunkSize: 8);
        volume.SetMaterial(new GridCoordinate3i(-2, 3, 4), 7);
        volume.SetMaterial(new GridCoordinate3i(10, -4, 1), 12);
        var original = CreateEnvelope(volume.CaptureSnapshot());

        var json = original.Serialize();
        var restored = SparseMaterialVoxelVolumeEnvelope3d.Deserialize(json);

        Assert.That(json, Does.Contain(SparseMaterialVoxelVolumeEnvelope3d.SchemaId));
        Assert.That(restored.RegionId, Is.EqualTo(original.RegionId));
        Assert.That(restored.DatasetId, Is.EqualTo(original.DatasetId));
        Assert.That(restored.RevisionId, Is.EqualTo(original.RevisionId));
        Assert.That(restored.CoordinateFrameId, Is.EqualTo(original.CoordinateFrameId));
        Assert.That(restored.AxisConvention, Is.EqualTo(original.AxisConvention));
        Assert.That(restored.VerticalDatum, Is.EqualTo(original.VerticalDatum));
        Assert.That(restored.GenerationParametersJson, Is.EqualTo(original.GenerationParametersJson));
        Assert.That(restored.Snapshot.Origin, Is.EqualTo(original.Snapshot.Origin));
        Assert.That(restored.Snapshot.VoxelSize, Is.EqualTo(original.Snapshot.VoxelSize));
        Assert.That(restored.Snapshot.ChunkSize, Is.EqualTo(original.Snapshot.ChunkSize));
        Assert.That(restored.Snapshot.Samples, Is.EqualTo(original.Snapshot.Samples));
    }

    [Test]
    public void Deserialize_LoadsPublishedSyntheticVolumetricFixture()
    {
        var path = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Records",
            "sparse-material-voxel-volume-example.json");
        var json = File.ReadAllText(path);

        var envelope = SparseMaterialVoxelVolumeEnvelope3d.Deserialize(json);

        Assert.That(envelope.DatasetId, Is.EqualTo("dataset.example.sparse-volume"));
        Assert.That(envelope.Snapshot.SolidVoxelCount, Is.EqualTo(8));
        Assert.That(envelope.Snapshot.GetMaterialId(new GridCoordinate3i(1, 1, 1)), Is.EqualTo(1));
        Assert.That(envelope.Snapshot.GetMaterialId(new GridCoordinate3i(2, 1, 1)), Is.Zero);
        Assert.That(envelope.GenerationParametersJson, Does.Contain("\"synthetic\": true"));
    }

    [Test]
    public void Deserialize_RejectsUnsupportedSchemaAndSampleLimit()
    {
        var envelope = CreateEnvelope(
            new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, 0d), 1d).CaptureSnapshot());
        var json = envelope.Serialize();

        Assert.Throws<ArgumentException>(() =>
            SparseMaterialVoxelVolumeEnvelope3d.Deserialize(
                json.Replace(SparseMaterialVoxelVolumeEnvelope3d.SchemaId, "unknown.schema", StringComparison.Ordinal)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SparseMaterialVoxelVolumeEnvelope3d.Deserialize(json, maximumSampleCount: -1));

        var populated = new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, 0d), 1d);
        populated.SetMaterial(new GridCoordinate3i(0, 0, 0), 1);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SparseMaterialVoxelVolumeEnvelope3d.Deserialize(
                CreateEnvelope(populated.CaptureSnapshot()).Serialize(),
                maximumSampleCount: 0));
    }

    [Test]
    public void Deserialize_RejectsDuplicateVoxelCoordinates()
    {
        var volume = new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, 0d), 1d);
        volume.SetMaterial(new GridCoordinate3i(0, 0, 0), 1);
        var json = CreateEnvelope(volume.CaptureSnapshot()).Serialize();
        var root = JsonNode.Parse(json)!;
        var samples = root["samples"]!.AsArray();
        samples.Add(samples[0]!.DeepClone());

        Assert.Throws<ArgumentException>(() =>
            SparseMaterialVoxelVolumeEnvelope3d.Deserialize(root.ToJsonString()));
    }

    [Test]
    public void Deserialize_RejectsUnknownProperties()
    {
        var envelope = CreateEnvelope(
            new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, 0d), 1d).CaptureSnapshot());
        var root = JsonNode.Parse(envelope.Serialize())!;
        root["unexpected"] = true;

        Assert.Throws<ArgumentException>(() =>
            SparseMaterialVoxelVolumeEnvelope3d.Deserialize(root.ToJsonString()));
    }

    [Test]
    public void Constructor_RejectsMissingSpatialIdentityAndInvalidGenerationParameters()
    {
        var snapshot = new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, 0d), 1d).CaptureSnapshot();

        Assert.Throws<ArgumentException>(() => CreateEnvelope(snapshot, datasetId: " "));
        Assert.Throws<ArgumentException>(() => CreateEnvelope(snapshot, generationParametersJson: "not-json"));
        Assert.Throws<ArgumentException>(() => CreateEnvelope(snapshot, generationParametersJson: "[]"));
        Assert.Throws<ArgumentNullException>(() => new SparseMaterialVoxelVolumeEnvelope3d(
            null!, "region", "dataset", "revision", "frame", "m", "X=east;Y=north;Z=up",
            "model-relative", "up", "materials", "repo", "revision", "model", "1",
            "algorithm", "1", "{}", "run"));
    }

    private static SparseMaterialVoxelVolumeEnvelope3d CreateEnvelope(
        SparseMaterialVoxelVolumeSnapshot3d snapshot,
        string datasetId = "terrain.volume.test",
        string generationParametersJson = "{\"voxelSize\":0.25,\"chunkSize\":16}") =>
        new(
            snapshot,
            regionId: "region.test",
            datasetId: datasetId,
            revisionId: "revision.1",
            coordinateFrameId: "frame.local.east-north-up",
            linearUnit: "m",
            axisConvention: "X=east;Y=north;Z=up",
            verticalDatum: "model-relative",
            verticalPositiveDirection: "up",
            materialCatalogId: "materials.test.v1",
            sourceRepository: "TrentBest/TheSingularityWorkshop.Physics",
            sourceRevision: new string('a', 40),
            modelId: "model.terrain.volume",
            modelVersion: "1.0.0",
            algorithmId: "algorithm.sparse-material-voxel",
            algorithmVersion: "1.0.0",
            generationParametersJson: generationParametersJson,
            sourceRunId: "run.test",
            seed: 12345L);
}
