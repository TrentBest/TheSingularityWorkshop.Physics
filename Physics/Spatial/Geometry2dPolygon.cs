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

    private static bool IsOnClosedSegment(Vector2d point, Vector2d a, Vector2d b) =>
        Geometry2d.Orient(a, b, point) == Orientation2d.Collinear &&
        point.X >= Math.Min(a.X, b.X) && point.X <= Math.Max(a.X, b.X) &&
        point.Y >= Math.Min(a.Y, b.Y) && point.Y <= Math.Max(a.Y, b.Y);
}
