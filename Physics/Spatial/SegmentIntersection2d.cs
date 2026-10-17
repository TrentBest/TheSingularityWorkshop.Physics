namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Describes a segment intersection and its optional unique coordinate.</summary>
/// <remarks>
/// <see cref="SegmentIntersectionKind2d.Disjoint"/> and
/// <see cref="SegmentIntersectionKind2d.CollinearOverlap"/> have no unique intersection point.
/// For point intersections, <see cref="Point"/> is populated. The coordinate is a floating-point
/// construction, not an exact representation of the mathematical intersection.
/// </remarks>
public readonly struct SegmentIntersection2d
{
    /// <summary>Gets the topological classification of the intersection.</summary>
    public SegmentIntersectionKind2d Kind { get; }

    /// <summary>
    /// Gets the unique intersection point when one exists; otherwise null.
    /// A collinear overlap is an interval, not a single point.
    /// </summary>
    public Vector2d? Point { get; }

    /// <summary>Gets whether this result has one unique intersection coordinate.</summary>
    public bool HasUniquePoint => Point.HasValue;

    internal SegmentIntersection2d(SegmentIntersectionKind2d kind, Vector2d? point)
    {
        if ((kind == SegmentIntersectionKind2d.Disjoint ||
             kind == SegmentIntersectionKind2d.CollinearOverlap) && point.HasValue)
            throw new ArgumentException("This intersection kind cannot have a unique point.", nameof(point));

        if ((kind == SegmentIntersectionKind2d.ProperIntersection ||
             kind == SegmentIntersectionKind2d.EndpointTouch ||
             kind == SegmentIntersectionKind2d.DegeneratePointIntersection) && !point.HasValue)
            throw new ArgumentException("A point intersection requires a coordinate.", nameof(point));

        Kind = kind;
        Point = point;
    }
}

/// <summary>Constructs coordinates for intersections of closed 2D segments.</summary>
/// <remarks>
/// Topology is determined by <see cref="Geometry2dSegments.ClassifyIntersection"/>. Proper-crossing
/// coordinates are calculated in scaled coordinates to reduce overflow risk. The construction still
/// uses binary64 arithmetic: exceptionally ill-conditioned intersections or a result that cannot be
/// represented as finite binary64 coordinates throw <see cref="ArithmeticException"/>. Exact
/// predicates do not remove measurement uncertainty or guarantee coordinate accuracy.
/// </remarks>
public static partial class Geometry2dSegments
{
    /// <summary>Classifies two closed segments and returns a coordinate only for a unique point intersection.</summary>
    public static SegmentIntersection2d Intersect(
        Vector2d a, Vector2d b, Vector2d c, Vector2d d)
    {
        SegmentIntersectionKind2d kind = ClassifyIntersection(a, b, c, d);
        switch (kind)
        {
            case SegmentIntersectionKind2d.Disjoint:
            case SegmentIntersectionKind2d.CollinearOverlap:
                return new SegmentIntersection2d(kind, null);

            case SegmentIntersectionKind2d.DegeneratePointIntersection:
                if (a == b && PointOnSegment(a, c, d))
                    return new SegmentIntersection2d(kind, a);
                if (c == d && PointOnSegment(c, a, b))
                    return new SegmentIntersection2d(kind, c);
                if (a == c)
                    return new SegmentIntersection2d(kind, a);
                throw new ArithmeticException("The degenerate intersection could not be resolved consistently.");

            case SegmentIntersectionKind2d.EndpointTouch:
                if (PointOnSegment(a, c, d)) return new SegmentIntersection2d(kind, a);
                if (PointOnSegment(b, c, d)) return new SegmentIntersection2d(kind, b);
                if (PointOnSegment(c, a, b)) return new SegmentIntersection2d(kind, c);
                if (PointOnSegment(d, a, b)) return new SegmentIntersection2d(kind, d);
                throw new ArithmeticException("The endpoint intersection could not be resolved consistently.");

            case SegmentIntersectionKind2d.ProperIntersection:
                return new SegmentIntersection2d(kind, ConstructProperIntersection(a, b, c, d));

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown segment intersection kind.");
        }
    }

    private static Vector2d ConstructProperIntersection(Vector2d a, Vector2d b, Vector2d c, Vector2d d)
    {
        // For axis-aligned crossings, the intersection coordinates are input coordinates themselves.
        // Reuse them directly: normalizing every endpoint together can erase a tiny segment next to
        // an enormous one, even though the crossing is exactly representable.
        if (a.Y == b.Y && c.X == d.X)
            return new Vector2d(c.X, a.Y);
        if (c.Y == d.Y && a.X == b.X)
            return new Vector2d(a.X, c.Y);

        double scale = MaxAbs(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y);
        if (scale == 0d || !double.IsFinite(scale))
            throw new ArithmeticException("Cannot scale segment coordinates for intersection construction.");

        // Normalize before subtraction and cross products to avoid overflow from large finite inputs.
        double ax = a.X / scale, ay = a.Y / scale;
        double bx = b.X / scale, by = b.Y / scale;
        double cx = c.X / scale, cy = c.Y / scale;
        double dx = d.X / scale, dy = d.Y / scale;

        double rx = bx - ax, ry = by - ay;
        double sx = dx - cx, sy = dy - cy;
        double qx = cx - ax, qy = cy - ay;
        double denominator = Cross(rx, ry, sx, sy);
        double numerator = Cross(qx, qy, sx, sy);

        if (!double.IsFinite(denominator) || denominator == 0d || !double.IsFinite(numerator))
            throw new ArithmeticException("The proper intersection is too ill-conditioned to construct reliably.");

        double t = numerator / denominator;
        double x = (ax + (t * rx)) * scale;
        double y = (ay + (t * ry)) * scale;
        if (!double.IsFinite(x) || !double.IsFinite(y))
            throw new ArithmeticException("The intersection coordinate is outside the finite binary64 range.");

        return new Vector2d(x, y);
    }

    private static double MaxAbs(
        double x1, double y1, double x2, double y2,
        double x3, double y3, double x4, double y4) =>
        System.Math.Max(
            System.Math.Max(System.Math.Abs(x1), System.Math.Abs(y1)),
            System.Math.Max(
                System.Math.Max(System.Math.Abs(x2), System.Math.Abs(y2)),
                System.Math.Max(
                    System.Math.Max(System.Math.Abs(x3), System.Math.Abs(y3)),
                    System.Math.Max(System.Math.Abs(x4), System.Math.Abs(y4)))));

    private static double Cross(double ax, double ay, double bx, double by) =>
        (ax * by) - (ay * bx);
}
