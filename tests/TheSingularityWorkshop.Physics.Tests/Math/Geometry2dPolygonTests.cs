using System;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class Geometry2dPolygonTests
{
    [Test]
    public void LocatePoint_ClassifiesInsideOutsideEdgesAndVertices()
    {
        Vector2d[] square = { new(0, 0), new(4, 0), new(4, 4), new(0, 4) };

        Assert.That(Locate(square, (2, 2)), Is.EqualTo(PointPolygonRelation2d.Inside));
        Assert.That(Locate(square, (5, 2)), Is.EqualTo(PointPolygonRelation2d.Outside));
        Assert.That(Locate(square, (2, 0)), Is.EqualTo(PointPolygonRelation2d.Boundary));
        Assert.That(Locate(square, (0, 0)), Is.EqualTo(PointPolygonRelation2d.Boundary));
    }

    [Test]
    public void LocatePoint_IsIndependentOfPolygonWindingDirection()
    {
        Vector2d[] clockwise = { new(0, 0), new(0, 4), new(4, 4), new(4, 0) };

        Assert.That(Locate(clockwise, (2, 2)), Is.EqualTo(PointPolygonRelation2d.Inside));
        Assert.That(Locate(clockwise, (5, 2)), Is.EqualTo(PointPolygonRelation2d.Outside));
    }

    [Test]
    public void LocatePoint_HandlesConcavePolygon()
    {
        Vector2d[] concave =
        {
            new(0, 0), new(4, 0), new(4, 4),
            new(2, 2), new(0, 4)
        };

        Assert.That(Locate(concave, (1, 2)), Is.EqualTo(PointPolygonRelation2d.Inside));
        Assert.That(Locate(concave, (2, 3)), Is.EqualTo(PointPolygonRelation2d.Outside));
        Assert.That(Locate(concave, (2, 2)), Is.EqualTo(PointPolygonRelation2d.Boundary));
    }

    [Test]
    public void LocatePoint_RejectsNullAndTooFewVertices()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Geometry2dPolygon.LocatePoint(null!, new Vector2d(0, 0)));
        Assert.Throws<ArgumentException>(() =>
            Geometry2dPolygon.LocatePoint(new[] { new Vector2d(0, 0), new Vector2d(1, 0) },
                new Vector2d(0, 0)));
    }

    private static PointPolygonRelation2d Locate(
        Vector2d[] vertices, (double X, double Y) point) =>
        Geometry2dPolygon.LocatePoint(vertices, new Vector2d(point.X, point.Y));
}
