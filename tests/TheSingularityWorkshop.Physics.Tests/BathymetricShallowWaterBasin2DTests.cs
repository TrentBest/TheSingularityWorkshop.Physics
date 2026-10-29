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
    public void Advance_WettingFrontMovesIntoDryCellsAndConservesWater()
    {
        const int width = 5;
        const int height = 3;
        var bed = new double[width * height];
        var depth = new double[bed.Length];
        for (var y = 0; y < height; y++)
        {
            depth[y * width] = 1d;
            depth[y * width + 1] = 1d;
        }

        var basin = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, 0.005d, depth, bed);
        var initialVolume = basin.WaterVolume;

        basin.Advance();

        Assert.That(basin.SampleDepth(2, 1), Is.GreaterThan(0d),
            "The initially dry cell next to the wet region should become wet.");
        var finalDepth = basin.SnapshotDepth();
        for (var i = 0; i < finalDepth.Length; i++)
            Assert.That(double.IsFinite(finalDepth[i]) && finalDepth[i] >= 0d, Is.True,
                $"Cell {i} has invalid wet/dry depth.");
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            Assert.That(double.IsFinite(basin.SampleVelocityX(x, y)), Is.True);
            Assert.That(double.IsFinite(basin.SampleVelocityY(x, y)), Is.True);
        }
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-10));
        Assert.That(basin.StepCount, Is.EqualTo(1));
    }

    [Test]
    public void Advance_WaterFlowsDownhillAcrossMultipleStepsAndConservesVolume()
    {
        const int width = 5;
        const int height = 3;
        var bed = new double[width * height];
        var depth = new double[bed.Length];
        var elevations = new[] { 0.4d, 0.3d, 0.2d, 0.1d, 0d };

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var i = y * width + x;
            bed[i] = elevations[x];
            if (x < 2)
                depth[i] = 0.3d;
        }

        var basin = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, 0.002d, depth, bed);
        var initialVolume = basin.WaterVolume;
        var initialHighGroundDepth = basin.SampleDepth(0, 1);

        for (var step = 0; step < 80; step++)
            basin.Advance();

        var finalDepth = basin.SnapshotDepth();
        for (var i = 0; i < finalDepth.Length; i++)
            Assert.That(double.IsFinite(finalDepth[i]) && finalDepth[i] >= 0d, Is.True,
                $"Cell {i} has invalid depth after multi-step wetting.");
        Assert.That(basin.SampleDepth(2, 1), Is.GreaterThan(0d),
            "Water should reach the initially dry, lower-elevation cell over multiple steps.");
        Assert.That(basin.SampleDepth(0, 1), Is.LessThan(initialHighGroundDepth),
            "The initially wet upper cell should lose some water as downhill flow develops.");
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-10),
            "Closed boundaries must conserve water during downhill redistribution.");
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
    public void ApplyRainfall_AddsCellDepthAndConservesExistingWaterMomentumAndTime()
    {
        var basin = new BathymetricShallowWaterBasin2D(
            2, 2, 2d, 3d, 0.005d,
            new[] { 0.2d, 0d, 0.1d, 0d },
            new[] { 0d, 0.2d, 0d, 0.2d });
        var initialVolume = basin.WaterVolume;
        var initialTime = basin.SimulationTime;
        var rain = new RainfallRateField2D(2, 2, new[] { 0.001d, 0.002d, 0d, 0.004d });

        basin.ApplyRainfall(rain, 10d);

        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume + (0.01d + 0.02d + 0d + 0.04d) * 6d).Within(1e-12));
        Assert.That(basin.SampleDepth(0, 0), Is.EqualTo(0.21d).Within(1e-12));
        Assert.That(basin.SampleDepth(1, 0), Is.EqualTo(0.02d).Within(1e-12), "Rain should wet a previously dry cell.");
        Assert.That(basin.SampleDepth(0, 1), Is.EqualTo(0.1d));
        Assert.That(basin.SampleDepth(1, 1), Is.EqualTo(0.04d).Within(1e-12));
        Assert.That(basin.SimulationTime, Is.EqualTo(initialTime));
        Assert.That(basin.StepCount, Is.EqualTo(0));
    }

    [Test]
    public void ApplyRainfallAndAdvance_ReplaysDeterministicallyAndAccountsForAddedVolume()
    {
        const int width = 3;
        const int height = 3;
        const double timeStep = 0.005d;
        const int steps = 20;
        var initialDepth = Enumerable.Repeat(0.2d, width * height).ToArray();
        var bed = new double[width * height];
        var rain = new RainfallRateField2D(width, height,
            new[] { 0d, 0d, 0d, 0d, 0.001d, 0d, 0d, 0d, 0d });
        var first = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, timeStep, initialDepth, bed);
        var second = new BathymetricShallowWaterBasin2D(width, height, 1d, 1d, timeStep, initialDepth, bed);
        var initialVolume = first.WaterVolume;

        for (var step = 0; step < steps; step++)
        {
            first.ApplyRainfall(rain, timeStep);
            first.Advance();
            second.ApplyRainfall(rain, timeStep);
            second.Advance();
        }

        Assert.That(first.WaterVolume, Is.EqualTo(initialVolume + 0.001d * timeStep * steps).Within(1e-10),
            "Closed boundaries should conserve the volume introduced by repeated rainfall.");
        Assert.That(first.SnapshotDepth(), Is.EqualTo(second.SnapshotDepth()),
            "Identical rainfall forcing and solver ordering should reproduce cell depths.");
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            Assert.That(first.SampleVelocityX(x, y), Is.EqualTo(second.SampleVelocityX(x, y)));
            Assert.That(first.SampleVelocityY(x, y), Is.EqualTo(second.SampleVelocityY(x, y)));
        }
        Assert.That(first.StepCount, Is.EqualTo(steps));
        Assert.That(first.SimulationTime, Is.EqualTo(steps * timeStep).Within(1e-12));
    }

    [Test]
    public void ApplyRainfall_RejectsMismatchedGridWithoutChangingState()
    {
        var basin = new BathymetricShallowWaterBasin2D(
            2, 2, 1d, 1d, 0.01d, new[] { 0.1d, 0.1d, 0.1d, 0.1d },
            new[] { 0d, 0d, 0d, 0d });
        var before = basin.SnapshotDepth();

        Assert.Throws<ArgumentException>(() => basin.ApplyRainfall(
            new RainfallRateField2D(1, 1, new[] { 0.001d }), 1d));

        Assert.That(basin.SnapshotDepth(), Is.EqualTo(before));
    }

    [Test]
    public void ApplyRainfall_RejectsCflViolationWithoutChangingState()
    {
        var basin = new BathymetricShallowWaterBasin2D(
            2, 2, 1d, 1d, 0.1d, new[] { 0.01d, 0.01d, 0.01d, 0.01d },
            new[] { 0d, 0d, 0d, 0d });
        var before = basin.SnapshotDepth();
        var rain = new RainfallRateField2D(2, 2, new[] { 100d, 100d, 100d, 100d });

        Assert.Throws<InvalidOperationException>(() => basin.ApplyRainfall(rain, 1d));

        Assert.That(basin.SnapshotDepth(), Is.EqualTo(before));
        Assert.That(basin.StepCount, Is.EqualTo(0));
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
