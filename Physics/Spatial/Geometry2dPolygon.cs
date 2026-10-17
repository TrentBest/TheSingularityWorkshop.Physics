using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Point-location predicates for 2D polygon boundaries.</summary>
/// <remarks>
/// The polygon is expected to be a simple polygon described by at least three vertices in boundary
/// order. The first vertex need not be repeated at the end. Boundary membership uses exact orientation
/// of the supplied binary64 coordinates; no tolerance or measurement uncertainty is implied.
/// </remarks>
public static class Geometry2dPolygon
{
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
