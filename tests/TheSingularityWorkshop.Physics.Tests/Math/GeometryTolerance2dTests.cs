using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class GeometryTolerance2dTests
{
    [Test]
    public void Constructor_RejectsNegativeAndNonFiniteTolerance()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeometryTolerance2d(-1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeometryTolerance2d(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeometryTolerance2d(double.PositiveInfinity));
    }

    [Test]
    public void IsNear_UsesInclusiveEuclideanDistance()
    {
        var tolerance = new GeometryTolerance2d(5d);
        Assert.That(tolerance.IsNear(new Vector2d(0, 0), new Vector2d(3, 4)), Is.True);
        Assert.That(tolerance.IsNear(new Vector2d(0, 0), new Vector2d(3, 4.0001)), Is.False);
        Assert.That(new GeometryTolerance2d(0d).IsNear(new Vector2d(1, 2), new Vector2d(1, 2)), Is.True);
        Assert.That(new GeometryTolerance2d(0d).IsNear(new Vector2d(1, 2), new Vector2d(1, 2.0001)), Is.False);
    }

    [Test]
    public void IsNear_AvoidsSquaredDistanceOverflow()
    {
        var tolerance = new GeometryTolerance2d(1e308);
        Assert.That(tolerance.IsNear(new Vector2d(0, 0), new Vector2d(8e307, 0)), Is.True);
        Assert.That(tolerance.IsNear(new Vector2d(-1e308, 0), new Vector2d(1e308, 0)), Is.False);
    }

    [Test]
    public void IsNear_HandlesTinyCoordinateDifferences()
    {
        Assert.That(new GeometryTolerance2d(1e-300).IsNear(
            new Vector2d(0, 0), new Vector2d(1e-300, 1e-300)), Is.False);
    }
}
