using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Math;

[TestFixture]
public sealed class Geometry2dTests
{
    [Test]
    public void Orient_ClassifiesClockwiseCounterClockwiseAndCollinear()
    {
        var a = new Vector2d(0d, 0d);
        var b = new Vector2d(2d, 0d);

        Assert.That(Geometry2d.Orient(a, b, new Vector2d(1d, 1d)), Is.EqualTo(Orientation2d.CounterClockwise));
        Assert.That(Geometry2d.Orient(a, b, new Vector2d(1d, -1d)), Is.EqualTo(Orientation2d.Clockwise));
        Assert.That(Geometry2d.Orient(a, b, new Vector2d(1d, 0d)), Is.EqualTo(Orientation2d.Collinear));
    }

    [Test]
    public void Orient_HandlesFiniteCoordinatesWhoseDifferencesOverflowDouble()
    {
        var a = new Vector2d(1e308, 1e308);
        var b = new Vector2d(1e308, -1e308);
        var c = new Vector2d(-1e308, 1e308);

        Assert.That(Geometry2d.Orient(a, b, c), Is.EqualTo(Orientation2d.Clockwise));
    }

    [Test]
    public void Orient_DistinguishesRepresentableNearCollinearPoint()
    {
        var a = new Vector2d(0d, 0d);
        var b = new Vector2d(1d, 1d);
        var c = new Vector2d(2d, double.BitIncrement(2d));

        Assert.That(Geometry2d.Orient(a, b, c), Is.EqualTo(Orientation2d.CounterClockwise));
    }

    [Test]
    public void Orient_IsInvariantToSwappingFirstTwoPoints()
    {
        var a = new Vector2d(-4d, 2d);
        var b = new Vector2d(7d, 3d);
        var c = new Vector2d(1d, 9d);

        Assert.That(Geometry2d.Orient(a, b, c), Is.EqualTo(Orientation2d.Clockwise));
        Assert.That(Geometry2d.Orient(b, a, c), Is.EqualTo(Orientation2d.CounterClockwise));
    }
}
