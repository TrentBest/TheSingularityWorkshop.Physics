using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Math;

[TestFixture]
public sealed class Geometry2dSegmentsTests
{
    [Test]
    public void ClassifyIntersection_DistinguishesProperCrossingAndDisjointSegments()
    {
        Assert.That(Classify((0, 0), (4, 4), (0, 4), (4, 0)),
            Is.EqualTo(SegmentIntersectionKind2d.ProperIntersection));
        Assert.That(Classify((0, 0), (2, 0), (3, -1), (3, 1)),
            Is.EqualTo(SegmentIntersectionKind2d.Disjoint));
        Assert.That(Classify((0, 0), (1, 0), (2, -1), (2, 1)),
            Is.EqualTo(SegmentIntersectionKind2d.Disjoint));
    }

    [Test]
    public void ClassifyIntersection_DetectsEndpointTouches()
    {
        Assert.That(Classify((0, 0), (2, 0), (2, 0), (2, 3)),
            Is.EqualTo(SegmentIntersectionKind2d.EndpointTouch));
        Assert.That(Classify((0, 0), (4, 0), (2, 0), (2, 2)),
            Is.EqualTo(SegmentIntersectionKind2d.EndpointTouch));
        Assert.That(Classify((0, 0), (2, 2), (0, 2), (2, 0)),
            Is.EqualTo(SegmentIntersectionKind2d.ProperIntersection));
    }

    [Test]
    public void ClassifyIntersection_DistinguishesCollinearOverlapTouchAndGap()
    {
        Assert.That(Classify((0, 0), (5, 0), (2, 0), (8, 0)),
            Is.EqualTo(SegmentIntersectionKind2d.CollinearOverlap));
        Assert.That(Classify((0, 0), (2, 0), (2, 0), (5, 0)),
            Is.EqualTo(SegmentIntersectionKind2d.EndpointTouch));
        Assert.That(Classify((0, 0), (2, 0), (3, 0), (5, 0)),
            Is.EqualTo(SegmentIntersectionKind2d.Disjoint));
        Assert.That(Classify((3, 5), (3, -2), (3, 1), (3, 8)),
            Is.EqualTo(SegmentIntersectionKind2d.CollinearOverlap));
    }

    [Test]
    public void ClassifyIntersection_HandlesZeroLengthSegmentsExplicitly()
    {
        Assert.That(Classify((1, 1), (1, 1), (0, 0), (2, 2)),
            Is.EqualTo(SegmentIntersectionKind2d.DegeneratePointIntersection));
        Assert.That(Classify((1, 1), (1, 1), (1, 1), (1, 1)),
            Is.EqualTo(SegmentIntersectionKind2d.DegeneratePointIntersection));
        Assert.That(Classify((1, 2), (1, 2), (0, 0), (2, 2)),
            Is.EqualTo(SegmentIntersectionKind2d.Disjoint));
        Assert.That(Classify((1, 1), (1, 1), (2, 2), (2, 2)),
            Is.EqualTo(SegmentIntersectionKind2d.Disjoint));
    }

    [Test]
    public void ClassifyIntersection_HandlesExtremeFiniteCoordinates()
    {
        Assert.That(Classify((-1e308, 0), (1e308, 0), (0, -1e308), (0, 1e308)),
            Is.EqualTo(SegmentIntersectionKind2d.ProperIntersection));
    }

    private static SegmentIntersectionKind2d Classify(
        (double X, double Y) a, (double X, double Y) b,
        (double X, double Y) c, (double X, double Y) d) =>
        Geometry2dSegments.ClassifyIntersection(
            new Vector2d(a.X, a.Y), new Vector2d(b.X, b.Y),
            new Vector2d(c.X, c.Y), new Vector2d(d.X, d.Y));
}
