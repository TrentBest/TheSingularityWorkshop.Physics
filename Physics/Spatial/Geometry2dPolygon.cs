using System;
using System.Collections.Generic;
using System.Numerics;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Point-location predicates for 2D polygon boundaries.</summary>
/// <remarks>
/// The polygon is expected to be a simple polygon described by at least three vertices in boundary
/// order. The first vertex need not be repeated at the end. Boundary membership uses exact orientation
/// of the supplied binary64 coordinates; no tolerance or measurement uncertainty is implied.
/// </remarks>
public static class Geometry2dPolygon
{
    /// <summary>Computes the signed shoelace area of a closed polygon boundary.</summary>
    /// <param name="vertices">At least three effective vertices in boundary order; the first may optionally be repeated at the end.</param>
    /// <returns>Positive area for counter-clockwise order, negative for clockwise order, or zero for a degenerate/algebraically cancelling boundary.</returns>
    /// <exception cref="ArgumentNullException">The vertex collection is null.</exception>
    /// <exception cref="ArgumentException">The boundary has fewer than three effective vertices.</exception>
    /// <exception cref="ArithmeticException">The exact area magnitude exceeds the finite binary64 range.</exception>
    /// <remarks>
    /// The shoelace sum is accumulated exactly over the supplied binary64 coordinates using integer
    /// arithmetic. The final area is rounded to binary64 (nearest, ties-to-even); a nonzero exact area
    /// smaller than the representable range may underflow to zero. This is algebraic area for any
    /// ordered boundary, including self-intersecting boundaries; call <see cref="IsSimple"/> separately
    /// when a simple polygon is required. Complexity is O(n) arithmetic operations, with allocation
    /// proportional to the exact integer sizes.
    /// </remarks>
    public static double SignedArea(IReadOnlyList<Vector2d> vertices)
    {
        if (vertices is null)
            throw new ArgumentNullException(nameof(vertices));

        int count = vertices.Count;
        if (count > 1 && vertices[count - 1] == vertices[0])
            count--;

        if (count < 3)
            throw new ArgumentException("A polygon requires at least three effective vertices.", nameof(vertices));

        BigInteger twiceAreaUnits = ComputeTwiceAreaUnits(vertices, count);

        // Each coordinate is represented in units of 2^-1074. The shoelace sum
        // therefore has units of 2^-2148; dividing by two gives area units 2^-2149.
        return ScaledIntegerToDouble(twiceAreaUnits, -2149);
    }

    private static BigInteger ComputeTwiceAreaUnits(IReadOnlyList<Vector2d> vertices, int count)
    {
        BigInteger twiceAreaUnits = BigInteger.Zero;
        for (int i = 0; i < count; i++)
        {
            Vector2d current = vertices[i];
            Vector2d next = vertices[(i + 1) % count];
            BigInteger x1 = ToSubnormalUnits(current.X);
            BigInteger y1 = ToSubnormalUnits(current.Y);
            BigInteger x2 = ToSubnormalUnits(next.X);
            BigInteger y2 = ToSubnormalUnits(next.Y);
            twiceAreaUnits += x1 * y2 - y1 * x2;
        }

        return twiceAreaUnits;
    }

    private static BigInteger ToSubnormalUnits(double value)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);
        bool negative = bits < 0;
        ulong magnitudeBits = unchecked((ulong)bits) & 0x7FFFFFFFFFFFFFFFUL;
        int exponentBits = (int)(magnitudeBits >> 52);
        ulong fraction = magnitudeBits & 0x000FFFFFFFFFFFFFUL;

        if (exponentBits == 0)
        {
            BigInteger subnormal = fraction;
            return negative ? -subnormal : subnormal;
        }

        BigInteger significand = (1UL << 52) | fraction;
        BigInteger units = significand << (exponentBits - 1);
        return negative ? -units : units;
    }

    private static double ScaledIntegerToDouble(BigInteger value, int binaryExponent)
    {
        if (value.IsZero)
            return 0d;

        bool negative = value.Sign < 0;
        BigInteger magnitude = BigInteger.Abs(value);
        long bitLength = magnitude.GetBitLength();
        int shift = bitLength > 53 ? checked((int)(bitLength - 53)) : 0;
        BigInteger significand = magnitude >> shift;

        if (shift > 0)
        {
            BigInteger remainder = magnitude - (significand << shift);
            BigInteger halfway = BigInteger.One << (shift - 1);
            if (remainder > halfway || (remainder == halfway && !significand.IsEven))
                significand += BigInteger.One;
        }

        double rounded = Math.ScaleB((double)significand, checked(binaryExponent + shift));
        if (double.IsInfinity(rounded))
            throw new ArithmeticException("The polygon area exceeds the finite binary64 range.");

        return negative ? -rounded : rounded;
    }

    /// <summary>Locates a point relative to a closed polygon using the non-zero winding rule.</summary>
    /// <param name="vertices">At least three polygon vertices in boundary order.</param>
    /// <param name="point">The point to classify.</param>
    /// <returns>Inside, Outside, or Boundary.</returns>
    /// <exception cref="ArgumentNullException">The vertex collection is null.</exception>
    /// <exception cref="ArgumentException">The polygon contains fewer than three vertices.</exception>
    public static PointPolygonRelation2d LocatePoint(
        IReadOnlyList<Vector2d> vertices, Vector2d point)
    {
        if (vertices is null)
            throw new ArgumentNullException(nameof(vertices));
        if (vertices.Count < 3)
            throw new ArgumentException("A polygon requires at least three vertices.", nameof(vertices));

        int windingNumber = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2d a = vertices[i];
            Vector2d b = vertices[(i + 1) % vertices.Count];

            if (IsOnClosedSegment(point, a, b))
                return PointPolygonRelation2d.Boundary;

            if (a.Y <= point.Y)
            {
                if (b.Y > point.Y &&
                    Geometry2d.Orient(a, b, point) == Orientation2d.CounterClockwise)
                    windingNumber++;
            }
            else if (b.Y <= point.Y &&
                     Geometry2d.Orient(a, b, point) == Orientation2d.Clockwise)
            {
                windingNumber--;
            }
        }

        return windingNumber == 0
            ? PointPolygonRelation2d.Outside
            : PointPolygonRelation2d.Inside;
    }

    /// <summary>Triangulates a simple polygon into non-overlapping triangles using its original vertex indices.</summary>
    /// <param name="vertices">At least three effective vertices in boundary order; the first may optionally be repeated at the end.</param>
    /// <returns>Exactly n - 2 triangles for the effective n-vertex boundary, preserving its winding direction.</returns>
    /// <exception cref="ArgumentNullException">The vertex collection is null.</exception>
    /// <exception cref="ArgumentException">The boundary is not simple, has fewer than three effective vertices, has zero signed area, or contains a collinear consecutive triple.</exception>
    /// <remarks>
    /// Ear clipping uses exact-sign orientation predicates and O(n²) worst-case work. Consecutive collinear
    /// vertices are rejected rather than silently removed, so every returned index refers to the original
    /// effective boundary and each triangle has nonzero area. A repeated closing vertex is excluded from
    /// the effective boundary. No proximity tolerance is applied.
    /// </remarks>
    public static IReadOnlyList<TriangleIndices2d> Triangulate(IReadOnlyList<Vector2d> vertices)
    {
        if (vertices is null)
            throw new ArgumentNullException(nameof(vertices));

        int count = vertices.Count;
        if (count > 1 && vertices[count - 1] == vertices[0])
            count--;

        if (count < 3)
            throw new ArgumentException("A polygon requires at least three effective vertices.", nameof(vertices));
        if (!IsSimple(vertices))
            throw new ArgumentException("The polygon boundary must be simple.", nameof(vertices));

        int winding = ComputeTwiceAreaUnits(vertices, count).Sign;
        if (winding == 0)
            throw new ArgumentException("The polygon must have nonzero signed area.", nameof(vertices));

        for (int i = 0; i < count; i++)
        {
            Vector2d previous = vertices[(i + count - 1) % count];
            Vector2d current = vertices[i];
            Vector2d next = vertices[(i + 1) % count];
            if (Geometry2d.Orient(previous, current, next) == Orientation2d.Collinear)
                throw new ArgumentException("Consecutive polygon vertices must not be collinear.", nameof(vertices));
        }

        var remaining = new List<int>(count);
        for (int i = 0; i < count; i++)
            remaining.Add(i);

        var triangles = new List<TriangleIndices2d>(count - 2);
        while (remaining.Count > 3)
        {
            bool clippedEar = false;
            for (int i = 0; i < remaining.Count; i++)
            {
                int previousIndex = remaining[(i + remaining.Count - 1) % remaining.Count];
                int currentIndex = remaining[i];
                int nextIndex = remaining[(i + 1) % remaining.Count];
                Vector2d previous = vertices[previousIndex];
                Vector2d current = vertices[currentIndex];
                Vector2d next = vertices[nextIndex];

                if (Geometry2d.Orient(previous, current, next).Sign() != winding)
                    continue;

                bool containsVertex = false;
                for (int j = 0; j < remaining.Count; j++)
                {
                    int candidateIndex = remaining[j];
                    if (candidateIndex == previousIndex ||
                        candidateIndex == currentIndex ||
                        candidateIndex == nextIndex)
                        continue;

                    if (PointInTriangleInclusive(vertices[candidateIndex], previous, current, next, winding))
                    {
                        containsVertex = true;
                        break;
                    }
                }

                if (containsVertex)
                    continue;

                triangles.Add(new TriangleIndices2d(previousIndex, currentIndex, nextIndex));
                remaining.RemoveAt(i);
                clippedEar = true;
                break;
            }

            if (!clippedEar)
                throw new InvalidOperationException("No valid ear was found for a boundary accepted as simple.");
        }

        triangles.Add(new TriangleIndices2d(remaining[0], remaining[1], remaining[2]));
        return Array.AsReadOnly(triangles.ToArray());
    }

    private static bool PointInTriangleInclusive(
        Vector2d point, Vector2d a, Vector2d b, Vector2d c, int winding)
    {
        int ab = Geometry2d.Orient(a, b, point).Sign();
        int bc = Geometry2d.Orient(b, c, point).Sign();
        int ca = Geometry2d.Orient(c, a, point).Sign();
        return winding > 0
            ? ab >= 0 && bc >= 0 && ca >= 0
            : ab <= 0 && bc <= 0 && ca <= 0;
    }

    /// <summary>Determines whether the vertices describe a simple closed polygon boundary.</summary>
    /// <param name="vertices">Vertices in boundary order; the first vertex may optionally be repeated at the end.</param>
    /// <returns>
    /// True when the boundary has at least three distinct vertices, no zero-length edges, and no
    /// intersections except the shared endpoints of adjacent edges; otherwise false.
    /// </returns>
    /// <exception cref="ArgumentNullException">The vertex collection is null.</exception>
    /// <remarks>
    /// The check is O(n²), uses exact represented-coordinate segment predicates, and does not apply
    /// a proximity tolerance. Non-adjacent touches are rejected as well as crossings and overlaps.
    /// </remarks>
    public static bool IsSimple(IReadOnlyList<Vector2d> vertices)
    {
        if (vertices is null)
            throw new ArgumentNullException(nameof(vertices));

        int count = vertices.Count;
        if (count > 1 && vertices[count - 1] == vertices[0])
            count--;

        if (count < 3)
            return false;

        // Repeated vertices imply a zero-length edge or a non-adjacent boundary touch.
        for (int i = 0; i < count; i++)
        {
            Vector2d current = vertices[i];
            Vector2d next = vertices[(i + 1) % count];
            if (current == next)
                return false;

            for (int j = i + 1; j < count; j++)
            {
                if (current == vertices[j])
                    return false;
            }
        }

        // Every edge pair must be disjoint unless the edges are adjacent and meet only
        // at their shared endpoint. Adjacent collinear backtracking is an overlap, not simple.
        for (int i = 0; i < count; i++)
        {
            Vector2d a = vertices[i];
            Vector2d b = vertices[(i + 1) % count];

            for (int j = i + 1; j < count; j++)
            {
                bool adjacent = j == i + 1 || (i == 0 && j == count - 1);
                SegmentIntersectionKind2d kind = Geometry2dSegments.ClassifyIntersection(
                    a, b, vertices[j], vertices[(j + 1) % count]);

                if (adjacent)
                {
                    if (kind != SegmentIntersectionKind2d.EndpointTouch)
                        return false;
                }
                else if (kind != SegmentIntersectionKind2d.Disjoint)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsOnClosedSegment(Vector2d point, Vector2d a, Vector2d b) =>
        Geometry2d.Orient(a, b, point) == Orientation2d.Collinear &&
        point.X >= Math.Min(a.X, b.X) && point.X <= Math.Max(a.X, b.X) &&
        point.Y >= Math.Min(a.Y, b.Y) && point.Y <= Math.Max(a.Y, b.Y);
}
