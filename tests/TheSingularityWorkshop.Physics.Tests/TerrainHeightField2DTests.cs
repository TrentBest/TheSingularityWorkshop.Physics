using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TerrainHeightField2DTests
{
    private static TerrainHeightField2D Plane(double slopeX = 0d, double slopeY = 0d) =>
        TerrainHeightField2D.CreatePlane(5, 5, 2d, 3d, 100d, -20d,
            "local-east-north", "local-zero", "metres", "up", 10d, slopeX, slopeY);

    [Test]
    public void Plane_UsesExplicitOriginSpacingAndAnalyticalElevation()
    {
        var terrain = Plane(2d, -1d);
        Assert.That(terrain.TrySampleElevation(104d, -14d, out var elevation), Is.True);
        Assert.That(elevation, Is.EqualTo(24d).Within(1e-12));
        Assert.That(terrain.TrySampleElevation(99d, -14d, out _), Is.False);
        Assert.That(terrain.SampleCell(2, 2), Is.EqualTo(24d));
    }

    [Test]
    public void Ramp_HasDeclaredEndpointElevations()
    {
        var ramp = TerrainHeightField2D.CreateRamp(5, 3, 2d, 1d, 0d, 0d,
            "frame", "datum", "m", "up", 2d, 10d);
        Assert.That(ramp.SampleCell(0, 1), Is.EqualTo(2d));
        Assert.That(ramp.SampleCell(4, 1), Is.EqualTo(10d));
        Assert.That(ramp.GetSurfaceNormal(2, 1).X, Is.LessThan(0f));
    }

    [Test]
    public void Bowl_HasMinimumAtCenterAndRimAtRadius()
    {
        var bowl = TerrainHeightField2D.CreateBowl(9, 9, 1d, 1d, -4d, -4d,
            "frame", "datum", "m", "up", 20d, 8d, 0d, 0d, 4d);
        Assert.That(bowl.SampleCell(4, 4), Is.EqualTo(12d));
        Assert.That(bowl.SampleCell(8, 4), Is.EqualTo(20d));
    }

    [Test]
    public void RadialBrush_IsLocalizedDeterministicAndDoesNotMutateSource()
    {
        var source = Plane();
        var first = source.ApplyRadialBrush(104d, -14d, 2d, 5d);
        var second = source.ApplyRadialBrush(104d, -14d, 2d, 5d);
        Assert.That(first.SnapshotElevations(), Is.EqualTo(second.SnapshotElevations()));
        Assert.That(source.SampleCell(2, 2), Is.EqualTo(10d));
        Assert.That(first.SampleCell(2, 2), Is.EqualTo(15d));
        Assert.That(first.SampleCell(0, 0), Is.EqualTo(10d));
    }

    [Test]
    public void Constructor_RejectsNonFiniteSamplesAndWrongSampleCount()
    {
        Assert.Throws<ArgumentException>(() => new TerrainHeightField2D(2, 2, 1d, 1d, 0d, 0d,
            "frame", "datum", "m", "up", new[] { 0d, 0d, 0d, double.NaN }));
        Assert.Throws<ArgumentException>(() => new TerrainHeightField2D(2, 2, 1d, 1d, 0d, 0d,
            "frame", "datum", "m", "up", new[] { 0d }));
    }

    [Test]
    public void BrushOverflow_DoesNotChangeSource()
    {
        var source = new TerrainHeightField2D(2, 2, 1d, 1d, 0d, 0d,
            "frame", "datum", "m", "up", new[] { double.MaxValue, 0d, 0d, 0d });
        Assert.Throws<ArithmeticException>(() => source.ApplyRadialBrush(0d, 0d, 1d, double.MaxValue));
        Assert.That(source.SampleCell(0, 0), Is.EqualTo(double.MaxValue));
    }
}
