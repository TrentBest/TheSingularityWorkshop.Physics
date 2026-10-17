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
    public void LocatePoint_HandlesExtremeFiniteCoordinatesWithoutOverflowingPredicates()
    {
        Vector2d[] square =
        {
            new(-1e308, -1e308), new(1e308, -1e308),
            new(1e308, 1e308), new(-1e308, 1e308)
        };

        Assert.That(Geometry2dPolygon.LocatePoint(square, new Vector2d(0, 0)),
            Is.EqualTo(PointPolygonRelation2d.Inside));
        Assert.That(Geometry2dPolygon.LocatePoint(square, new Vector2d(1e308, 0)),
            Is.EqualTo(PointPolygonRelation2d.Boundary));
        Assert.That(Geometry2dPolygon.LocatePoint(square, new Vector2d(1.7e308, 0)),
            Is.EqualTo(PointPolygonRelation2d.Outside));
    }

    [Test]
    public void LocatePoint_HandlesSubnormalScalePolygon()
    {
        double tiny = 1e-320;
        Vector2d[] square =
        {
            new(0, 0), new(tiny, 0), new(tiny, tiny), new(0, tiny)
        };

        Assert.That(Geometry2dPolygon.LocatePoint(square, new Vector2d(tiny / 2, tiny / 2)),
            Is.EqualTo(PointPolygonRelation2d.Inside));
        Assert.That(Geometry2dPolygon.LocatePoint(square, new Vector2d(tiny, tiny / 2)),
            Is.EqualTo(PointPolygonRelation2d.Boundary));
        Assert.That(Geometry2dPolygon.LocatePoint(square, new Vector2d(tiny * 2, tiny / 2)),
            Is.EqualTo(PointPolygonRelation2d.Outside));
    }


    [Test]
    public void LocatePoint_HandlesRepeatedClosingVertexAndRayThroughVertices()
    {
        Vector2d[] closedSquare =
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4), new(0, 0)
        };

        // The horizontal ray shares a y-coordinate with two polygon vertices.
        // Crossing-count rules must not double-count those vertices.
        Assert.That(Geometry2dPolygon.LocatePoint(closedSquare, new Vector2d(2, 2)),
            Is.EqualTo(PointPolygonRelation2d.Inside));
        Assert.That(Geometry2dPolygon.LocatePoint(closedSquare, new Vector2d(-1, 0)),
            Is.EqualTo(PointPolygonRelation2d.Outside));
        Assert.That(Geometry2dPolygon.LocatePoint(closedSquare, new Vector2d(2, 0)),
            Is.EqualTo(PointPolygonRelation2d.Boundary));
        Assert.That(Geometry2dPolygon.LocatePoint(closedSquare, new Vector2d(4, 2)),
            Is.EqualTo(PointPolygonRelation2d.Boundary));
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


    [Test]
    public void SignedArea_UsesWindingDirectionAndSupportsRepeatedClosingVertex()
    {
        Vector2d[] counterClockwise = { new(0, 0), new(4, 0), new(4, 4), new(0, 4) };
        Vector2d[] clockwise = { new(0, 0), new(0, 4), new(4, 4), new(4, 0) };
        Vector2d[] explicitlyClosed =
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4), new(0, 0)
        };

        Assert.That(Geometry2dPolygon.SignedArea(counterClockwise), Is.EqualTo(16d));
        Assert.That(Geometry2dPolygon.SignedArea(clockwise), Is.EqualTo(-16d));
        Assert.That(Geometry2dPolygon.SignedArea(explicitlyClosed), Is.EqualTo(16d));
    }

    [Test]
    public void SignedArea_ComputesConcaveAndAlgebraicallyCancellingBoundaries()
    {
        Vector2d[] concave =
        {
            new(0, 0), new(4, 0), new(4, 4), new(2, 2), new(0, 4)
        };
        Vector2d[] bowTie =
        {
            new(0, 0), new(4, 4), new(0, 4), new(4, 0)
        };

        Assert.That(Geometry2dPolygon.SignedArea(concave), Is.EqualTo(12d));
        Assert.That(Geometry2dPolygon.SignedArea(bowTie), Is.EqualTo(0d));
    }

    [Test]
    public void SignedArea_AccumulatesExtremeCoordinatesWithoutIntermediateOverflow()
    {
        Vector2d[] square =
        {
            new(0, 0), new(1e154, 0), new(1e154, 1e154), new(0, 1e154)
        };

        Assert.That(Geometry2dPolygon.SignedArea(square),
            Is.EqualTo(1e308).Within(2e292));
    }

    [Test]
    public void SignedArea_ThrowsWhenExactAreaExceedsFiniteBinary64Range()
    {
        Vector2d[] square =
        {
            new(-1e154, -1e154), new(1e154, -1e154),
            new(1e154, 1e154), new(-1e154, 1e154)
        };

        Assert.Throws<ArithmeticException>(() => Geometry2dPolygon.SignedArea(square));
    }

    [Test]
    public void SignedArea_RoundsTinyRepresentableAreaAndRejectsTooFewEffectiveVertices()
    {
        Vector2d[] tinySquare =
        {
            new(0, 0), new(1e-160, 0), new(1e-160, 1e-160), new(0, 1e-160)
        };

        Assert.That(Geometry2dPolygon.SignedArea(tinySquare),
            Is.EqualTo(1e-320).Within(1e-323));
        Assert.Throws<ArgumentException>(() => Geometry2dPolygon.SignedArea(new[]
        {
            new Vector2d(0, 0), new Vector2d(1, 0), new Vector2d(0, 0)
        }));
        Assert.Throws<ArgumentNullException>(() => Geometry2dPolygon.SignedArea(null!));
    }

    [Test]
    public void IsSimple_AcceptsConvexConcaveAndRepeatedClosingVertex()
    {
        Vector2d[] square = { new(0, 0), new(4, 0), new(4, 4), new(0, 4) };
        Vector2d[] concave =
        {
            new(0, 0), new(4, 0), new(4, 4), new(2, 2), new(0, 4)
        };
        Vector2d[] explicitlyClosed =
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4), new(0, 0)
        };

        Assert.That(Geometry2dPolygon.IsSimple(square), Is.True);
        Assert.That(Geometry2dPolygon.IsSimple(concave), Is.True);
        Assert.That(Geometry2dPolygon.IsSimple(explicitlyClosed), Is.True);
    }

    [Test]
    public void IsSimple_RejectsCrossingsNonAdjacentTouchesAndOverlappingAdjacentEdges()
    {
        Vector2d[] bowTie =
        {
            new(0, 0), new(4, 4), new(0, 4), new(4, 0)
        };
        Vector2d[] nonAdjacentTouch =
        {
            new(0, 0), new(4, 0), new(4, 4), new(2, 0), new(0, 4)
        };
        Vector2d[] adjacentBacktrack =
        {
            new(0, 0), new(4, 0), new(2, 0), new(2, 2), new(0, 2)
        };

        Assert.That(Geometry2dPolygon.IsSimple(bowTie), Is.False);
        Assert.That(Geometry2dPolygon.IsSimple(nonAdjacentTouch), Is.False);
        Assert.That(Geometry2dPolygon.IsSimple(adjacentBacktrack), Is.False);
    }

    [Test]
    public void IsSimple_RejectsTooFewVerticesAndRepeatedVertices()
    {
        Assert.That(Geometry2dPolygon.IsSimple(Array.Empty<Vector2d>()), Is.False);
        Assert.That(Geometry2dPolygon.IsSimple(new[] { new Vector2d(0, 0), new Vector2d(1, 0) }),
            Is.False);
        Assert.That(Geometry2dPolygon.IsSimple(new[]
        {
            new Vector2d(0, 0), new Vector2d(4, 0), new Vector2d(4, 4),
            new Vector2d(0, 4), new Vector2d(4, 0)
        }), Is.False);
        Assert.Throws<ArgumentNullException>(() => Geometry2dPolygon.IsSimple(null!));
    }

    private static PointPolygonRelation2d Locate(
        Vector2d[] vertices, (double X, double Y) point) =>
        Geometry2dPolygon.LocatePoint(vertices, new Vector2d(point.X, point.Y));
}
