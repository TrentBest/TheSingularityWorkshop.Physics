using NUnit.Framework;
using TheSingularityWorkshop.Physics.Hydrodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ShallowWaterBasin2DTests
{
    [Test]
    public void Advance_UniformStillWaterRemainsAtRest()
    {
        var basin = new ShallowWaterBasin2D(8, 6, 1d, 1d, 0.01d, Enumerable.Repeat(2d, 48).ToArray());
        var initialVolume = basin.WaterVolume;

        for (var i = 0; i < 100; i++)
            basin.Advance();

        Assert.That(basin.SnapshotDepth(), Is.All.EqualTo(2d));
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-12));
        Assert.That(basin.SampleVelocityX(3, 3), Is.EqualTo(0d));
        Assert.That(basin.SampleVelocityY(3, 3), Is.EqualTo(0d));
    }

    [Test]
    public void Advance_DepthDisturbanceEvolvesAndConservesClosedBasinVolume()
    {
        const int width = 25;
        const int height = 17;
        var depth = Enumerable.Repeat(1d, width * height).ToArray();
        depth[8 * width + 12] = 1.1d;
        var basin = new ShallowWaterBasin2D(width, height, 1d, 1d, 0.01d, depth);
        var initialVolume = basin.WaterVolume;

        for (var i = 0; i < 50; i++)
            basin.Advance();

        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-10));
        Assert.That(basin.SnapshotDepth(), Is.Not.EqualTo(depth));
        Assert.That(basin.SnapshotDepth().All(value => double.IsFinite(value) && value > 0d), Is.True);
        Assert.That(basin.StepCount, Is.EqualTo(50));
    }

    [Test]
    public void Advance_LocalizedDisturbancePropagatesAlongBothAxes()
    {
        const int width = 9;
        const int height = 7;
        const int centerX = 4;
        const int centerY = 3;
        var depth = Enumerable.Repeat(1d, width * height).ToArray();
        depth[centerY * width + centerX] = 1.1d;
        var basin = new ShallowWaterBasin2D(width, height, 1d, 1d, 0.01d, depth);

        basin.Advance();

        Assert.That(basin.SampleDepth(centerX, centerY), Is.LessThan(1.1d));
        Assert.That(basin.SampleDepth(centerX - 1, centerY), Is.GreaterThan(1d));
        Assert.That(basin.SampleDepth(centerX + 1, centerY), Is.GreaterThan(1d));
        Assert.That(basin.SampleDepth(centerX, centerY - 1), Is.GreaterThan(1d));
        Assert.That(basin.SampleDepth(centerX, centerY + 1), Is.GreaterThan(1d));
    }

    [Test]
    public void Advance_IdenticalInitialStatesRemainDeterministic()
    {
        const int width = 13;
        const int height = 9;
        var depth = Enumerable.Repeat(1d, width * height).ToArray();
        depth[4 * width + 6] = 1.08d;
        depth[3 * width + 5] = 1.02d;
        var first = new ShallowWaterBasin2D(width, height, 0.8d, 1.2d, 0.005d, depth);
        var second = new ShallowWaterBasin2D(width, height, 0.8d, 1.2d, 0.005d, depth);

        for (var i = 0; i < 40; i++)
        {
            first.Advance();
            second.Advance();
        }

        Assert.That(first.SnapshotDepth(), Is.EqualTo(second.SnapshotDepth()));
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            Assert.That(first.SampleVelocityX(x, y), Is.EqualTo(second.SampleVelocityX(x, y)));
            Assert.That(first.SampleVelocityY(x, y), Is.EqualTo(second.SampleVelocityY(x, y)));
        }
    }

    [Test]
    public void Constructor_RejectsUnsafeCflBound()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ShallowWaterBasin2D(4, 4, 1d, 1d, 0.2d, Enumerable.Repeat(10d, 16).ToArray()));
    }

    [Test]
    public void Snapshot_IsDefensiveCopy()
    {
        var basin = new ShallowWaterBasin2D(2, 2, 1d, 1d, 0.01d, new[] { 1d, 1d, 1d, 1d });
        var snapshot = basin.SnapshotDepth();
        snapshot[0] = 100d;

        Assert.That(basin.SampleDepth(0, 0), Is.EqualTo(1d));
    }
}
