using NUnit.Framework;
using TheSingularityWorkshop.Physics.Hydrodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class BathymetricShallowWaterBasin2DTests
{
    [Test]
    public void Advance_StillLakeRemainsAtRestOverUnevenBed()
    {
        const int width = 9;
        const int height = 7;
        const double freeSurface = 2d;
        var bed = new double[width * height];
        var depth = new double[bed.Length];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var i = y * width + x;
            var dx = x - 4d;
            var dy = y - 3d;
            bed[i] = 0.35d * Math.Exp(-(dx * dx + dy * dy) / 5d) - 0.08d * Math.Exp(-((x - 1d) * (x - 1d) + (y - 5d) * (y - 5d)) / 2d);
            depth[i] = freeSurface - bed[i];
        }
        var basin = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, 0.005d, depth, bed);
        var initialVolume = basin.WaterVolume;
        var initialDepth = basin.SnapshotDepth();

        for (var step = 0; step < 100; step++)
            basin.Advance();

        var finalDepth = basin.SnapshotDepth();
        for (var i = 0; i < initialDepth.Length; i++)
            Assert.That(finalDepth[i], Is.EqualTo(initialDepth[i]).Within(1e-11), $"Cell {i} drifted from still-water equilibrium.");
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-10));
        Assert.That(basin.StepCount, Is.EqualTo(100));
        Assert.That(basin.SampleVelocityX(4, 3), Is.EqualTo(0d).Within(1e-10));
        Assert.That(basin.SampleVelocityY(4, 3), Is.EqualTo(0d).Within(1e-10));
    }

    [Test]
    public void Advance_StillLakePreservesDryShorelineAboveFreeSurface()
    {
        const int width = 6;
        const int height = 5;
        const double freeSurface = 1d;
        var bed = new double[width * height];
        var depth = new double[bed.Length];
        var elevations = new[] { -0.2d, 0.1d, 0.4d, 0.8d, 1.1d, 1.4d };
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var i = y * width + x;
            bed[i] = elevations[x];
            depth[i] = Math.Max(0d, freeSurface - bed[i]);
        }

        var basin = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, 0.003d, depth, bed);
        var initialVolume = basin.WaterVolume;
        var initialDepth = basin.SnapshotDepth();

        for (var step = 0; step < 80; step++)
            basin.Advance();

        var finalDepth = basin.SnapshotDepth();
        for (var i = 0; i < initialDepth.Length; i++)
            Assert.That(finalDepth[i], Is.EqualTo(initialDepth[i]).Within(1e-11), $"Cell {i} drifted from wet/dry equilibrium.");
        for (var y = 0; y < height; y++)
        {
            Assert.That(basin.SampleDepth(4, y), Is.EqualTo(0d));
            Assert.That(basin.SampleDepth(5, y), Is.EqualTo(0d));
            Assert.That(basin.SampleVelocityX(4, y), Is.EqualTo(0d));
            Assert.That(basin.SampleVelocityY(4, y), Is.EqualTo(0d));
        }
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-10));
        Assert.That(basin.StepCount, Is.EqualTo(80));
    }

    [Test]
    public void Advance_SurfaceDisturbanceEvolvesAndConservesWaterOverTerrain()
    {
        const int width = 15;
        const int height = 11;
        const double baseSurface = 1.5d;
        var bed = new double[width * height];
        var depth = new double[bed.Length];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var i = y * width + x;
            var hillX = x - 7d;
            var hillY = y - 5d;
            var pulseX = x - 4d;
            var pulseY = y - 5d;
            bed[i] = 0.25d * Math.Exp(-(hillX * hillX + hillY * hillY) / 12d);
            var freeSurface = baseSurface + 0.12d * Math.Exp(-(pulseX * pulseX + pulseY * pulseY) / 3d);
            depth[i] = freeSurface - bed[i];
        }

        var basin = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, 0.002d, depth, bed);
        var initialVolume = basin.WaterVolume;
        var initialDepth = basin.SnapshotDepth();

        for (var step = 0; step < 150; step++)
            basin.Advance();

        var finalDepth = basin.SnapshotDepth();
        var maximumDepthChange = 0d;
        var totalSpeed = 0d;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var i = y * width + x;
            Assert.That(double.IsFinite(finalDepth[i]) && finalDepth[i] > 0d, Is.True, $"Cell {i} has invalid depth.");
            maximumDepthChange = Math.Max(maximumDepthChange, Math.Abs(finalDepth[i] - initialDepth[i]));
            totalSpeed += Math.Abs(basin.SampleVelocityX(x, y)) + Math.Abs(basin.SampleVelocityY(x, y));
        }

        Assert.That(maximumDepthChange, Is.GreaterThan(1e-5), "The surface disturbance should evolve instead of remaining frozen.");
        Assert.That(totalSpeed, Is.GreaterThan(1e-4), "The surface disturbance should generate flow.");
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-9), "Closed-wall fluxes should conserve total water volume.");
        Assert.That(basin.StepCount, Is.EqualTo(150));
    }

    [Test]
    public void Constructor_RejectsNonFiniteBedElevation()
    {
        Assert.Throws<ArgumentException>(() => new BathymetricShallowWaterBasin2D(
            2, 2, 1d, 1d, 0.01d, new[] { 1d, 1d, 1d, 1d },
            new[] { 0d, double.NaN, 0d, 0d }));
    }

    [Test]
    public void Constructor_RejectsNegativeDepth()
    {
        Assert.Throws<ArgumentException>(() => new BathymetricShallowWaterBasin2D(
            2, 2, 1d, 1d, 0.01d, new[] { 1d, 1d, -0.1d, 1d },
            new[] { 0d, 0d, 0d, 0d }));
    }

    [Test]
    public void Snapshots_AreDefensiveCopies()
    {
        var basin = new BathymetricShallowWaterBasin2D(
            2, 2, 1d, 1d, 0.01d, new[] { 1d, 1d, 1d, 1d },
            new[] { 0d, 0.1d, 0.2d, 0d });
        var bed = basin.SnapshotBedElevation();
        var depth = basin.SnapshotDepth();
        bed[0] = 100d;
        depth[0] = 100d;
        Assert.That(basin.SampleBedElevation(0, 0), Is.EqualTo(0d));
        Assert.That(basin.SampleDepth(0, 0), Is.EqualTo(1d));
    }
}
