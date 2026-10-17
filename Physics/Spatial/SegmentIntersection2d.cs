using System.Numerics;

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
/// Topology is determined by <see cref="Geometry2dSegments.ClassifyIntersection"/>. General proper-crossing
/// coordinates are constructed from exact integer representations of the supplied binary64 coordinates,
/// then rounded back to binary64. This avoids losing small segments solely because another endpoint has
/// a much larger magnitude. The returned coordinate is still rounded and does not remove measurement
/// uncertainty; an unrepresentable result throws <see cref="ArithmeticException"/>.
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
        // Axis-aligned crossings have coordinates already present in the input.
        if (a.Y == b.Y && c.X == d.X) return new Vector2d(c.X, a.Y);
        if (c.Y == d.Y && a.X == b.X) return new Vector2d(a.X, c.Y);

        // Represent each finite binary64 input exactly as an integer times a shared power of two.
        double[] values = { a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y };
        var parts = new (BigInteger Mantissa, int Exponent)[values.Length];
        int minExponent = int.MaxValue;
        for (int i = 0; i < values.Length; i++)
        {
            parts[i] = Decompose(values[i]);
            if (!parts[i].Mantissa.IsZero && parts[i].Exponent < minExponent)
                minExponent = parts[i].Exponent;
        }
        if (minExponent == int.MaxValue)
            throw new ArithmeticException("Cannot construct an intersection from zero coordinates.");

        var q = new BigInteger[values.Length];
        for (int i = 0; i < parts.Length; i++)
            q[i] = parts[i].Mantissa.IsZero ? BigInteger.Zero : parts[i].Mantissa << (parts[i].Exponent - minExponent);

        // Implicit lines A*x + B*y + C = 0; determinants stay exact for mixed-scale inputs.
        BigInteger a1 = q[1] - q[3], b1 = q[2] - q[0], c1 = q[0] * q[3] - q[2] * q[1];
        BigInteger a2 = q[5] - q[7], b2 = q[6] - q[4], c2 = q[4] * q[7] - q[6] * q[5];
        BigInteger denominator = a1 * b2 - a2 * b1;
        if (denominator.IsZero)
            throw new ArithmeticException("The proper intersection has a zero exact line determinant.");

        double x = RationalToDouble(b1 * c2 - b2 * c1, denominator, minExponent);
        double y = RationalToDouble(c1 * a2 - c2 * a1, denominator, minExponent);
        if (!double.IsFinite(x) || !double.IsFinite(y))
            throw new ArithmeticException("The intersection coordinate is outside the finite binary64 range.");
        return new Vector2d(x, y);
    }

    private static (BigInteger Mantissa, int Exponent) Decompose(double value)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);
        bool negative = bits < 0;
        int exponentBits = (int)((bits >> 52) & 0x7ffL);
        long fraction = bits & 0x000f_ffff_ffff_ffffL;
        if (exponentBits == 0 && fraction == 0) return (BigInteger.Zero, 0);
        BigInteger mantissa;
        int exponent;
        if (exponentBits == 0) { mantissa = new BigInteger(fraction); exponent = -1074; }
        else { mantissa = new BigInteger((1L << 52) | fraction); exponent = exponentBits - 1023 - 52; }
        return (negative ? -mantissa : mantissa, exponent);
    }

    private static double RationalToDouble(BigInteger numerator, BigInteger denominator, int binaryExponent)
    {
        if (numerator.IsZero) return 0d;
        bool negative = numerator.Sign != denominator.Sign;
        BigInteger n = BigInteger.Abs(numerator), d = BigInteger.Abs(denominator);
        long nBits = n.GetBitLength(), dBits = d.GetBitLength();
        int nShift = (int)System.Math.Max(0L, nBits - 53L);
        int dShift = (int)System.Math.Max(0L, dBits - 53L);
        double nHead = (double)(n >> nShift), dHead = (double)(d >> dShift);
        int exponent = checked(nShift - dShift + binaryExponent);
        double magnitude = System.Math.ScaleB(nHead / dHead, exponent);
        return negative ? -magnitude : magnitude;
    }

}
