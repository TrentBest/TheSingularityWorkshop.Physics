namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Classifies how two closed 2D line segments intersect.</summary>
public enum SegmentIntersectionKind2d
{
    /// <summary>The segments have no point in common.</summary>
    Disjoint,
    /// <summary>The segments cross at a point interior to both segments.</summary>
    ProperIntersection,
    /// <summary>The segments share exactly one endpoint or one collinear boundary point.</summary>
    EndpointTouch,
    /// <summary>The segments share a collinear interval of positive length.</summary>
    CollinearOverlap,
    /// <summary>The intersection involves at least one zero-length segment.</summary>
    DegeneratePointIntersection
}

/// <summary>Closed-segment classification built on the exact-sign orientation predicate.</summary>
/// <remarks>
/// Coordinates are the finite binary64 values supplied by the caller. This API classifies whether
/// an intersection exists and its topological kind; it does not calculate an intersection coordinate.
/// Exact orientation does not account for measurement uncertainty or physical tolerances.
/// </remarks>
public static partial class Geometry2dSegments
{
    /// <summary>Classifies the intersection of closed segments AB and CD.</summary>
    public static SegmentIntersectionKind2d ClassifyIntersection(
        Vector2d a, Vector2d b, Vector2d c, Vector2d d)
    {
        bool abIsPoint = a == b;
        bool cdIsPoint = c == d;

        if (abIsPoint || cdIsPoint)
        {
            if (abIsPoint && cdIsPoint)
                return a == c
                    ? SegmentIntersectionKind2d.DegeneratePointIntersection
                    : SegmentIntersectionKind2d.Disjoint;

            if (abIsPoint)
                return PointOnSegment(a, c, d)
                    ? SegmentIntersectionKind2d.DegeneratePointIntersection
                    : SegmentIntersectionKind2d.Disjoint;

            return PointOnSegment(c, a, b)
                ? SegmentIntersectionKind2d.DegeneratePointIntersection
                : SegmentIntersectionKind2d.Disjoint;
        }

        Orientation2d abc = Geometry2d.Orient(a, b, c);
        Orientation2d abd = Geometry2d.Orient(a, b, d);
        Orientation2d cda = Geometry2d.Orient(c, d, a);
        Orientation2d cdb = Geometry2d.Orient(c, d, b);

        if (abc == Orientation2d.Collinear &&
            abd == Orientation2d.Collinear &&
            cda == Orientation2d.Collinear &&
            cdb == Orientation2d.Collinear)
        {
            return ClassifyCollinear(a, b, c, d);
        }

        if (Opposite(abc, abd) && Opposite(cda, cdb))
            return SegmentIntersectionKind2d.ProperIntersection;

        if ((abc == Orientation2d.Collinear && PointOnSegment(c, a, b)) ||
            (abd == Orientation2d.Collinear && PointOnSegment(d, a, b)) ||
            (cda == Orientation2d.Collinear && PointOnSegment(a, c, d)) ||
            (cdb == Orientation2d.Collinear && PointOnSegment(b, c, d)))
            return SegmentIntersectionKind2d.EndpointTouch;

        return SegmentIntersectionKind2d.Disjoint;
    }

    private static SegmentIntersectionKind2d ClassifyCollinear(
        Vector2d a, Vector2d b, Vector2d c, Vector2d d)
    {
        Vector2d abLow = Compare(a, b) <= 0 ? a : b;
        Vector2d abHigh = Compare(a, b) <= 0 ? b : a;
        Vector2d cdLow = Compare(c, d) <= 0 ? c : d;
        Vector2d cdHigh = Compare(c, d) <= 0 ? d : c;

        Vector2d low = Compare(abLow, cdLow) >= 0 ? abLow : cdLow;
        Vector2d high = Compare(abHigh, cdHigh) <= 0 ? abHigh : cdHigh;
        int order = Compare(low, high);

        if (order > 0) return SegmentIntersectionKind2d.Disjoint;
        if (order == 0) return SegmentIntersectionKind2d.EndpointTouch;
        return SegmentIntersectionKind2d.CollinearOverlap;
    }

    private static bool PointOnSegment(Vector2d point, Vector2d a, Vector2d b) =>
        Geometry2d.Orient(a, b, point) == Orientation2d.Collinear &&
        point.X >= Math.Min(a.X, b.X) && point.X <= Math.Max(a.X, b.X) &&
        point.Y >= Math.Min(a.Y, b.Y) && point.Y <= Math.Max(a.Y, b.Y);

    private static bool Opposite(Orientation2d left, Orientation2d right) =>
        (left == Orientation2d.CounterClockwise && right == Orientation2d.Clockwise) ||
        (left == Orientation2d.Clockwise && right == Orientation2d.CounterClockwise);

    // Lexicographic order is a consistent order along any straight line, including vertical lines.
    private static int Compare(Vector2d left, Vector2d right)
    {
        int xOrder = left.X.CompareTo(right.X);
        return xOrder != 0 ? xOrder : left.Y.CompareTo(right.Y);
    }
}
