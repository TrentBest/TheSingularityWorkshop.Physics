using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class SparseMaterialVoxelVolume3dTests
{
    [Test]
    public void Constructor_RejectsInvalidGridConfiguration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(Vector3d.Zero, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(Vector3d.Zero, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 65));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(new Vector3d(double.NaN, 0d, 0d), 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(new Vector3d(0d, double.PositiveInfinity, 0d), 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseMaterialVoxelVolume3d(new Vector3d(0d, 0d, double.NegativeInfinity), 1d));
    }

    [Test]
    public void EmptyVolume_DoesNotAllocateChunksOrMaterialCounts()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 0.25d);

        Assert.That(volume.ChunkCount, Is.Zero);
        Assert.That(volume.SolidVoxelCount, Is.Zero);
        Assert.That(volume.GetMaterialId(new GridCoordinate3i(0, 0, 0)), Is.Zero);
        Assert.That(volume.GetMaterialVoxelCount(1), Is.Zero);
        Assert.That(volume.SetEmpty(new GridCoordinate3i(0, 0, 0)), Is.False);
    }

    [Test]
    public void SetMaterial_TracksOccupancyAndPerMaterialCounts()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var marble = new GridCoordinate3i(1, 2, 3);
        var granite = new GridCoordinate3i(2, 2, 3);

        Assert.That(volume.SetMaterial(marble, 10), Is.True);
        Assert.That(volume.SetMaterial(marble, 10), Is.False);
        Assert.That(volume.SetMaterial(granite, 20), Is.True);

        Assert.That(volume.GetMaterialId(marble), Is.EqualTo(10));
        Assert.That(volume.GetMaterialId(granite), Is.EqualTo(20));
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(2));
        Assert.That(volume.GetMaterialVoxelCount(10), Is.EqualTo(1));
        Assert.That(volume.GetMaterialVoxelCount(20), Is.EqualTo(1));
        Assert.That(volume.ChunkCount, Is.EqualTo(1));
    }

    [Test]
    public void ReplacingMaterial_UpdatesCountsWithoutChangingOccupancy()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d);
        var voxel = new GridCoordinate3i(0, 0, 0);
        volume.SetMaterial(voxel, 4);

        Assert.That(volume.SetMaterial(voxel, 9), Is.True);

        Assert.That(volume.SolidVoxelCount, Is.EqualTo(1));
        Assert.That(volume.GetMaterialVoxelCount(4), Is.Zero);
        Assert.That(volume.GetMaterialVoxelCount(9), Is.EqualTo(1));
        Assert.That(volume.GetMaterialId(voxel), Is.EqualTo(9));
    }

    [Test]
    public void NegativeVoxelCoordinates_UseFloorBasedChunkAddressing()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var negative = new GridCoordinate3i(-1, -4, -5);
        var neighboring = new GridCoordinate3i(0, -4, -5);

        volume.SetMaterial(negative, 3);
        volume.SetMaterial(neighboring, 3);

        Assert.That(volume.GetMaterialId(negative), Is.EqualTo(3));
        Assert.That(volume.GetMaterialId(neighboring), Is.EqualTo(3));
        Assert.That(volume.ChunkCount, Is.EqualTo(2));
        Assert.That(volume.GetMaterialVoxelCount(3), Is.EqualTo(2));
    }

    [Test]
    public void SetEmpty_ReleasesMaterialCountsAndLastChunk()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var voxel = new GridCoordinate3i(-1, 0, 0);
        volume.SetMaterial(voxel, 12);

        Assert.That(volume.SetEmpty(voxel), Is.True);
        Assert.That(volume.SetEmpty(voxel), Is.False);
        Assert.That(volume.SolidVoxelCount, Is.Zero);
        Assert.That(volume.GetMaterialVoxelCount(12), Is.Zero);
        Assert.That(volume.ChunkCount, Is.Zero);
    }

    [Test]
    public void WorldMapping_UsesFloorAndIncludesOriginOffset()
    {
        var volume = new SparseMaterialVoxelVolume3d(new Vector3d(10d, -2d, 4d), 0.5d, 8);

        Assert.That(volume.WorldToVoxel(new Vector3d(9.999d, -2d, 4d)), Is.EqualTo(new GridCoordinate3i(-1, 0, 0)));
        Assert.That(volume.WorldToVoxel(new Vector3d(10d, -2.001d, 4d)), Is.EqualTo(new GridCoordinate3i(0, -1, 0)));
        Assert.That(volume.GetVoxelCenter(new GridCoordinate3i(2, 4, -2)), Is.EqualTo(new Vector3d(11.25d, 0.25d, 3.25d)));
    }

    [Test]
    public void SetMaterial_RejectsReservedEmptyIdentityWithoutMutation()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d);
        var voxel = new GridCoordinate3i(0, 0, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => volume.SetMaterial(voxel, 0));
        Assert.That(volume.ChunkCount, Is.Zero);
        Assert.That(volume.SolidVoxelCount, Is.Zero);
    }

    [Test]
    public void ApplyEdits_UpdatesMaterialsAndReturnsSortedDirtyChunks()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var edits = new[]
        {
            new MaterialVoxelEdit3d(new GridCoordinate3i(8, 0, 0), 2),
            new MaterialVoxelEdit3d(new GridCoordinate3i(-1, 0, 0), 3),
            new MaterialVoxelEdit3d(new GridCoordinate3i(0, 0, 0), 0),
            new MaterialVoxelEdit3d(new GridCoordinate3i(1, 0, 0), 4)
        };

        var result = volume.ApplyEdits(edits);

        Assert.That(result.ChangedVoxelCount, Is.EqualTo(3));
        Assert.That(result.AffectedSurfaceChunks, Is.EqualTo(new[]
        {
            new GridCoordinate3i(-1, -1, 0),
            new GridCoordinate3i(-1, 0, -1),
            new GridCoordinate3i(-1, 0, 0),
            new GridCoordinate3i(0, -1, 0),
            new GridCoordinate3i(0, 0, -1),
            new GridCoordinate3i(0, 0, 0),
            new GridCoordinate3i(1, 0, 0),
            new GridCoordinate3i(2, -1, 0),
            new GridCoordinate3i(2, 0, -1),
            new GridCoordinate3i(2, 0, 0)
        }));
        Assert.That(result.DirtyChunks, Is.EqualTo(new[]
        {
            new GridCoordinate3i(-1, 0, 0),
            new GridCoordinate3i(0, 0, 0),
            new GridCoordinate3i(2, 0, 0)
        }));
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(3));
        Assert.That(volume.GetMaterialVoxelCount(2), Is.EqualTo(1));
        Assert.That(volume.GetMaterialVoxelCount(3), Is.EqualTo(1));
        Assert.That(volume.GetMaterialVoxelCount(4), Is.EqualTo(1));
    }

    [Test]
    public void ApplyEdits_RejectsInvalidBatchBeforeChangingVolume()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var existing = new GridCoordinate3i(0, 0, 0);
        volume.SetMaterial(existing, 7);

        var invalid = new[]
        {
            new MaterialVoxelEdit3d(new GridCoordinate3i(1, 0, 0), 8),
            new MaterialVoxelEdit3d(new GridCoordinate3i(2, 0, 0), -1)
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => volume.ApplyEdits(invalid));
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(1));
        Assert.That(volume.GetMaterialId(existing), Is.EqualTo(7));
        Assert.That(volume.GetMaterialId(new GridCoordinate3i(1, 0, 0)), Is.Zero);
    }

    [Test]
    public void ApplyEdits_RejectsDuplicateVoxelCoordinatesBeforeChangingVolume()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d);
        var voxel = new GridCoordinate3i(0, 0, 0);
        var edits = new[]
        {
            new MaterialVoxelEdit3d(voxel, 1),
            new MaterialVoxelEdit3d(voxel, 2)
        };

        Assert.Throws<ArgumentException>(() => volume.ApplyEdits(edits));
        Assert.That(volume.ChunkCount, Is.Zero);
        Assert.That(volume.SolidVoxelCount, Is.Zero);
    }

    [Test]
    public void ApplyEdits_ReportsOnlyActualChanges()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var voxel = new GridCoordinate3i(0, 0, 0);
        volume.SetMaterial(voxel, 5);

        var result = volume.ApplyEdits(new[]
        {
            new MaterialVoxelEdit3d(voxel, 5),
            new MaterialVoxelEdit3d(new GridCoordinate3i(1, 0, 0), 0)
        });

        Assert.That(result.ChangedVoxelCount, Is.Zero);
        Assert.That(result.DirtyChunks, Is.Empty);
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveSphere_RemovesVoxelCentersInsideBrushAndReportsDirtyChunks()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        var nearA = new GridCoordinate3i(0, 0, 0);
        var nearB = new GridCoordinate3i(1, 0, 0);
        var far = new GridCoordinate3i(3, 0, 0);
        volume.SetMaterial(nearA, 1);
        volume.SetMaterial(nearB, 2);
        volume.SetMaterial(far, 1);

        var result = volume.RemoveSphere(new Vector3d(0.5d, 0.5d, 0.5d), 1d);

        Assert.That(result.ChangedVoxelCount, Is.EqualTo(2));
        Assert.That(result.DirtyChunks, Is.EqualTo(new[]
        {
            new GridCoordinate3i(0, 0, 0)
        }));
        Assert.That(volume.GetMaterialId(nearA), Is.Zero);
        Assert.That(volume.GetMaterialId(nearB), Is.Zero);
        Assert.That(volume.GetMaterialId(far), Is.EqualTo(1));
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(1));
        Assert.That(volume.GetMaterialVoxelCount(1), Is.EqualTo(1));
        Assert.That(volume.GetMaterialVoxelCount(2), Is.Zero);
    }

    [Test]
    public void RemoveSphere_RejectsInvalidBrushWithoutChangingVolume()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d);
        var voxel = new GridCoordinate3i(0, 0, 0);
        volume.SetMaterial(voxel, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            volume.RemoveSphere(new Vector3d(0d, 0d, 0d), double.NaN));
        Assert.That(volume.GetMaterialId(voxel), Is.EqualTo(3));
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveSphere_EmptyVolumeDoesNotAllocateChunks()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d);

        var result = volume.RemoveSphere(Vector3d.Zero, 10d);

        Assert.That(result.ChangedVoxelCount, Is.Zero);
        Assert.That(result.DirtyChunks, Is.Empty);
        Assert.That(volume.ChunkCount, Is.Zero);
    }

    [Test]
    public void ExtractBoundarySurface_SingleVoxelProducesSixOutwardFaces()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        volume.SetMaterial(new GridCoordinate3i(0, 0, 0), 17);

        var mesh = volume.ExtractBoundarySurface();

        Assert.That(mesh.Vertices, Has.Count.EqualTo(8));
        Assert.That(mesh.TriangleCount, Is.EqualTo(12));
        Assert.That(mesh.TriangleIndices, Has.Count.EqualTo(36));
        Assert.That(mesh.TriangleMaterialIds, Has.All.EqualTo(17));
        Assert.That(mesh.TriangleNormals, Has.All.Matches<Vector3d>(n =>
            Math.Abs(n.Magnitude - 1d) < 1e-12));
        Assert.That(mesh.TriangleNormals.Distinct(), Is.EquivalentTo(new[]
        {
            new Vector3d(1d, 0d, 0d),
            new Vector3d(-1d, 0d, 0d),
            new Vector3d(0d, 1d, 0d),
            new Vector3d(0d, -1d, 0d),
            new Vector3d(0d, 0d, 1d),
            new Vector3d(0d, 0d, -1d)
        }));

        for (var triangle = 0; triangle < mesh.TriangleCount; triangle++)
        {
            var offset = triangle * 3;
            var a = mesh.Vertices[mesh.TriangleIndices[offset]];
            var b = mesh.Vertices[mesh.TriangleIndices[offset + 1]];
            var c = mesh.Vertices[mesh.TriangleIndices[offset + 2]];
            var windingNormal = (b - a).Cross(c - a).Normalized();

            Assert.That(windingNormal.Dot(mesh.TriangleNormals[triangle]), Is.GreaterThan(0.999999d));
        }
    }

    [Test]
    public void ExtractBoundarySurface_OmitsSharedFacesAcrossChunkBoundaries()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        volume.SetMaterial(new GridCoordinate3i(1, 0, 0), 1);
        volume.SetMaterial(new GridCoordinate3i(2, 0, 0), 2);

        var mesh = volume.ExtractBoundarySurface();

        Assert.That(volume.ChunkCount, Is.EqualTo(2));
        Assert.That(mesh.TriangleCount, Is.EqualTo(20));
        Assert.That(mesh.Vertices, Has.Count.EqualTo(12));
        Assert.That(mesh.TriangleMaterialIds.Count(id => id == 1), Is.EqualTo(10));
        Assert.That(mesh.TriangleMaterialIds.Count(id => id == 2), Is.EqualTo(10));
    }

    [Test]
    public void ExtractBoundarySurface_EmitsCavityWallsAndIsDeterministic()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 0.5d, 2);
        for (var z = 0; z < 3; z++)
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            volume.SetMaterial(new GridCoordinate3i(x, y, z), 5);
        volume.SetEmpty(new GridCoordinate3i(1, 1, 1));

        var first = volume.ExtractBoundarySurface();
        var second = volume.ExtractBoundarySurface();

        Assert.That(first.TriangleCount, Is.EqualTo(120));
        Assert.That(first.Vertices, Is.EqualTo(second.Vertices));
        Assert.That(first.TriangleIndices, Is.EqualTo(second.TriangleIndices));
        Assert.That(first.TriangleMaterialIds, Is.EqualTo(second.TriangleMaterialIds));

        for (var triangle = 0; triangle < first.TriangleCount; triangle++)
        {
            var offset = triangle * 3;
            var a = first.Vertices[first.TriangleIndices[offset]];
            var b = first.Vertices[first.TriangleIndices[offset + 1]];
            var c = first.Vertices[first.TriangleIndices[offset + 2]];
            var windingNormal = (b - a).Cross(c - a).Normalized();

            Assert.That(windingNormal.Dot(first.TriangleNormals[triangle]), Is.GreaterThan(0.999999d));
        }
    }

    [Test]
    public void ExtractBoundarySurfaceCached_ReusesMeshUntilAnyMutation()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        var firstVoxel = new GridCoordinate3i(0, 0, 0);
        var secondVoxel = new GridCoordinate3i(2, 0, 0);
        volume.SetMaterial(firstVoxel, 1);

        var first = volume.ExtractBoundarySurfaceCached();
        Assert.That(volume.ExtractBoundarySurfaceCached(), Is.SameAs(first));

        volume.SetMaterial(secondVoxel, 2);
        var afterSet = volume.ExtractBoundarySurfaceCached();
        Assert.That(afterSet, Is.Not.SameAs(first));
        Assert.That(afterSet.TriangleCount, Is.EqualTo(volume.ExtractBoundarySurface().TriangleCount));

        volume.SetEmpty(firstVoxel);
        var afterRemove = volume.ExtractBoundarySurfaceCached();
        Assert.That(afterRemove, Is.Not.SameAs(afterSet));
        Assert.That(afterRemove.TriangleCount, Is.EqualTo(volume.ExtractBoundarySurface().TriangleCount));

        volume.Clear();
        var afterClear = volume.ExtractBoundarySurfaceCached();
        Assert.That(afterClear, Is.Not.SameAs(afterRemove));
        Assert.That(afterClear.TriangleCount, Is.Zero);
    }

    [Test]
    public void ExtractBoundarySurfaceCached_IsInvalidatedByBatchEditsAndSphereRemoval()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        volume.SetMaterial(new GridCoordinate3i(0, 0, 0), 1);
        var initial = volume.ExtractBoundarySurfaceCached();

        volume.ApplyEdits(new[]
        {
            new MaterialVoxelEdit3d(new GridCoordinate3i(1, 0, 0), 2)
        });
        var afterBatch = volume.ExtractBoundarySurfaceCached();
        Assert.That(afterBatch, Is.Not.SameAs(initial));
        Assert.That(afterBatch.TriangleCount, Is.EqualTo(volume.ExtractBoundarySurface().TriangleCount));

        volume.RemoveSphere(new Vector3d(0.5d, 0.5d, 0.5d), 0d);
        var afterBrush = volume.ExtractBoundarySurfaceCached();
        Assert.That(afterBrush, Is.Not.SameAs(afterBatch));
        Assert.That(afterBrush.TriangleCount, Is.EqualTo(volume.ExtractBoundarySurface().TriangleCount));
    }

    [Test]
    public void ExtractBoundarySurfaceIncremental_MatchesReferenceAcrossChunkEditsAndReusesUnchangedResult()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, 2);
        volume.SetMaterial(new GridCoordinate3i(1, 0, 0), 1);
        volume.SetMaterial(new GridCoordinate3i(2, 0, 0), 2);
        volume.SetMaterial(new GridCoordinate3i(4, 0, 0), 3);
        volume.SetMaterial(new GridCoordinate3i(4, 1, 0), 3);

        var first = volume.ExtractBoundarySurfaceIncremental();
        AssertMeshesEquivalent(first, volume.ExtractBoundarySurface());
        Assert.That(volume.ExtractBoundarySurfaceIncremental(), Is.SameAs(first));

        // Removing a voxel on the seam invalidates both owner chunks, not the distant chunk.
        volume.SetEmpty(new GridCoordinate3i(2, 0, 0));
        var afterSeamEdit = volume.ExtractBoundarySurfaceIncremental();
        Assert.That(afterSeamEdit, Is.Not.SameAs(first));
        AssertMeshesEquivalent(afterSeamEdit, volume.ExtractBoundarySurface());
        Assert.That(volume.ExtractBoundarySurfaceIncremental(), Is.SameAs(afterSeamEdit));

        // Direct writes and batch/brush edits must also keep the chunk cache coherent.
        volume.SetMaterial(new GridCoordinate3i(5, 0, 0), 4);
        AssertMeshesEquivalent(volume.ExtractBoundarySurfaceIncremental(), volume.ExtractBoundarySurface());

        volume.ApplyEdits(new[]
        {
            new MaterialVoxelEdit3d(new GridCoordinate3i(1, 0, 0), 0),
            new MaterialVoxelEdit3d(new GridCoordinate3i(8, 0, 0), 5)
        });
        AssertMeshesEquivalent(volume.ExtractBoundarySurfaceIncremental(), volume.ExtractBoundarySurface());

        volume.RemoveSphere(volume.GetVoxelCenter(new GridCoordinate3i(4, 0, 0)), 0d);
        AssertMeshesEquivalent(volume.ExtractBoundarySurfaceIncremental(), volume.ExtractBoundarySurface());

        volume.Clear();
        var empty = volume.ExtractBoundarySurfaceIncremental();
        Assert.That(empty.TriangleCount, Is.Zero);
        AssertMeshesEquivalent(empty, volume.ExtractBoundarySurface());
    }

    private static void AssertMeshesEquivalent(VoxelSurfaceMesh3d actual, VoxelSurfaceMesh3d expected)
    {
        Assert.That(actual.TriangleCount, Is.EqualTo(expected.TriangleCount));
        Assert.That(actual.TriangleMaterialIds.OrderBy(id => id), Is.EqualTo(expected.TriangleMaterialIds.OrderBy(id => id)));
        Assert.That(actual.TriangleNormals.OrderBy(n => n.X).ThenBy(n => n.Y).ThenBy(n => n.Z),
            Is.EqualTo(expected.TriangleNormals.OrderBy(n => n.X).ThenBy(n => n.Y).ThenBy(n => n.Z)));
        Assert.That(actual.Vertices.OrderBy(v => v.X).ThenBy(v => v.Y).ThenBy(v => v.Z),
            Is.EqualTo(expected.Vertices.OrderBy(v => v.X).ThenBy(v => v.Y).ThenBy(v => v.Z)));
    }

    [Test]
    public void Clear_ResetsOccupancyChunksAndMaterialCounts()
    {
        var volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d);
        volume.SetMaterial(new GridCoordinate3i(0, 0, 0), 1);
        volume.SetMaterial(new GridCoordinate3i(20, 0, 0), 2);

        volume.Clear();

        Assert.That(volume.SolidVoxelCount, Is.Zero);
        Assert.That(volume.ChunkCount, Is.Zero);
        Assert.That(volume.GetMaterialVoxelCount(1), Is.Zero);
        Assert.That(volume.GetMaterialVoxelCount(2), Is.Zero);
    }
}
