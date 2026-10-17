using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class SegmentIntersection2dTests
{
    [Test]
    public void Intersect_ProperCrossingReturnsConstructedPoint()
    {
        var result = Intersect((0, 0), (4, 4), (0, 4), (4, 0));

        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.ProperIntersection));
        Assert.That(result.HasUniquePoint, Is.True);
        Assert.That(result.Point!.Value.X, Is.EqualTo(2).Within(1e-12));
        Assert.That(result.Point!.Value.Y, Is.EqualTo(2).Within(1e-12));
    }

    [Test]
    public void Intersect_EndpointTouchReturnsOriginalEndpointExactly()
    {
        var result = Intersect((0, 0), (2, 0), (2, 0), (2, 3));

        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.EndpointTouch));
        Assert.That(result.Point, Is.EqualTo(new Vector2d(2, 0)));
    }

    [Test]
    public void Intersect_DegeneratePointReturnsThatPoint()
    {
        var result = Intersect((1, 1), (1, 1), (0, 0), (2, 2));

        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.DegeneratePointIntersection));
        Assert.That(result.Point, Is.EqualTo(new Vector2d(1, 1)));
    }

    [Test]
    public void Intersect_CollinearOverlapHasNoUniquePoint()
    {
        var result = Intersect((0, 0), (5, 0), (2, 0), (8, 0));

        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.CollinearOverlap));
        Assert.That(result.HasUniquePoint, Is.False);
        Assert.That(result.Point, Is.Null);
    }

    [Test]
    public void Intersect_DisjointSegmentsHaveNoPoint()
    {
        var result = Intersect((0, 0), (1, 0), (2, -1), (2, 1));

        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.Disjoint));
        Assert.That(result.HasUniquePoint, Is.False);
    }

    [Test]
    public void Intersect_VerticalAndHorizontalSegmentsReturnCrossing()
    {
        var result = Intersect((3, -4), (3, 8), (-2, 2), (9, 2));

        Assert.That(result.Point, Is.EqualTo(new Vector2d(3, 2)));
    }

    [Test]
    public void Intersect_ExtremeFiniteCoordinatesAvoidIntermediateOverflow()
    {
        var result = Intersect((-1e308, -1e308), (1e308, 1e308),
            (-1e308, 1e308), (1e308, -1e308));

        Assert.That(result.Point!.Value.X, Is.EqualTo(0).Within(1e292));
        Assert.That(result.Point!.Value.Y, Is.EqualTo(0).Within(1e292));
    }

    [Test]
    public void Intersect_ExtremeMixedScaleAxisAlignedCrossingReturnsExactRepresentablePoint()
    {
        // The shared-scale general path would erase the tiny vertical segment. Since this is an
        // axis-aligned crossing, the intersection can instead reuse x from the vertical segment
        // and y from the horizontal segment without numerical construction.
        var result = Intersect((-1e308, 0), (1e308, 0), (0, -1e-300), (0, 1e-300));

        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.ProperIntersection));
        Assert.That(result.Point, Is.EqualTo(new Vector2d(0, 0)));
    }

    [Test]
    public void Intersect_TinyScaleCrossingReturnsFinitePoint()
    {
        // Exercise a crossing with subnormal-scale components; the coordinate construction must
        // return finite output rather than overflow or silently emit NaN/infinity.
        var result = Intersect((0, 0), (1, 1e-320), (0, 1e-320), (1, 0));
        Assert.That(result.Kind, Is.EqualTo(SegmentIntersectionKind2d.ProperIntersection));
        Assert.That(result.HasUniquePoint, Is.True);
        Assert.That(double.IsFinite(result.Point!.Value.X), Is.True);
        Assert.That(double.IsFinite(result.Point!.Value.Y), Is.True);
    }

    private static SegmentIntersection2d Intersect(
        (double X, double Y) a, (double X, double Y) b,
        (double X, double Y) c, (double X, double Y) d) =>
        Geometry2dSegments.Intersect(
            new Vector2d(a.X, a.Y), new Vector2d(b.X, b.Y),
            new Vector2d(c.X, c.Y), new Vector2d(d.X, d.Y));
}
