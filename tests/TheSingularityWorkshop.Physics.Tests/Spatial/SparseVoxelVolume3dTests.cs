using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class SparseVoxelVolume3dTests
{
    [Test]
    public void Constructor_RejectsInvalidGridConfiguration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseVoxelVolume3d(Vector3d.Zero, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseVoxelVolume3d(Vector3d.Zero, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseVoxelVolume3d(Vector3d.Zero, 1d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SparseVoxelVolume3d(Vector3d.Zero, 1d, 65));
    }

    [Test]
    public void EmptyVolume_DoesNotAllocateChunks()
    {
        var volume = new SparseVoxelVolume3d(Vector3d.Zero, 0.25d);

        Assert.That(volume.ChunkCount, Is.Zero);
        Assert.That(volume.SolidVoxelCount, Is.Zero);
        Assert.That(volume.IsSolid(new GridCoordinate3i(0, 0, 0)), Is.False);
        Assert.That(volume.SetSolid(new GridCoordinate3i(0, 0, 0), false), Is.False);
        Assert.That(volume.ChunkCount, Is.Zero);
    }

    [Test]
    public void SetSolid_AllocatesOnceAndCountsOnlyOccupancyChanges()
    {
        var volume = new SparseVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var voxel = new GridCoordinate3i(1, 2, 3);

        Assert.That(volume.SetSolid(voxel, true), Is.True);
        Assert.That(volume.SetSolid(voxel, true), Is.False);
        Assert.That(volume.ChunkCount, Is.EqualTo(1));
        Assert.That(volume.SolidVoxelCount, Is.EqualTo(1));
        Assert.That(volume.IsSolid(voxel), Is.True);
    }

    [Test]
    public void NegativeVoxelCoordinates_UseFloorBasedChunkAddressing()
    {
        var volume = new SparseVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var negative = new GridCoordinate3i(-1, -4, -5);
        var neighboring = new GridCoordinate3i(0, -4, -5);

        volume.SetSolid(negative, true);
        volume.SetSolid(neighboring, true);

        Assert.That(volume.IsSolid(negative), Is.True);
        Assert.That(volume.IsSolid(neighboring), Is.True);
        Assert.That(volume.ChunkCount, Is.EqualTo(2));
    }

    [Test]
    public void RemovingLastSolidVoxel_ReleasesItsChunk()
    {
        var volume = new SparseVoxelVolume3d(Vector3d.Zero, 1d, 4);
        var voxel = new GridCoordinate3i(-1, 0, 0);
        volume.SetSolid(voxel, true);

        Assert.That(volume.SetSolid(voxel, false), Is.True);
        Assert.That(volume.SolidVoxelCount, Is.Zero);
        Assert.That(volume.ChunkCount, Is.Zero);
    }

    [Test]
    public void WorldMapping_UsesFloorAndIncludesOriginOffset()
    {
        var volume = new SparseVoxelVolume3d(new Vector3d(10d, -2d, 4d), 0.5d, 8);

        Assert.That(volume.WorldToVoxel(new Vector3d(9.999d, -2d, 4d)), Is.EqualTo(new GridCoordinate3i(-1, 0, 0)));
        Assert.That(volume.WorldToVoxel(new Vector3d(10d, -2.001d, 4d)), Is.EqualTo(new GridCoordinate3i(0, -1, 0)));
        Assert.That(volume.GetVoxelCenter(new GridCoordinate3i(2, 4, -2)), Is.EqualTo(new Vector3d(11.25d, 0.25d, 3.25d)));
    }

    [Test]
    public void WorldMapping_RejectsIndicesOutsideIntegerRange()
    {
        var volume = new SparseVoxelVolume3d(Vector3d.Zero, double.Epsilon);

        Assert.Throws<ArgumentOutOfRangeException>(() => volume.WorldToVoxel(new Vector3d(1d, 0d, 0d)));
    }

    [Test]
    public void Clear_ResetsOccupancyAndAllocatedChunks()
    {
        var volume = new SparseVoxelVolume3d(Vector3d.Zero, 1d);
        volume.SetSolid(new GridCoordinate3i(0, 0, 0), true);
        volume.SetSolid(new GridCoordinate3i(20, 0, 0), true);

        volume.Clear();

        Assert.That(volume.SolidVoxelCount, Is.Zero);
        Assert.That(volume.ChunkCount, Is.Zero);
    }
}
