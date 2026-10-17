namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>An explicit absolute Euclidean-distance tolerance for 2D point comparisons.</summary>
/// <remarks>
/// The tolerance uses the same caller-defined coordinate units as the points. It represents a
/// modelling/measurement decision, not floating-point predicate error. Exact equality and
/// <see cref="Geometry2d.Orient(Vector2d, Vector2d, Vector2d)"/> remain exact with respect to
/// represented binary64 values. No global default tolerance is supplied because scale and units
/// belong to the caller's problem domain.
/// </remarks>
public readonly struct GeometryTolerance2d
{
    /// <summary>Gets the maximum permitted Euclidean distance, in the points' coordinate units.</summary>
    public double AbsoluteDistance { get; }

    /// <summary>Creates a tolerance with a finite, non-negative absolute distance.</summary>
    public GeometryTolerance2d(double absoluteDistance)
    {
        if (!double.IsFinite(absoluteDistance) || absoluteDistance < 0d)
            throw new ArgumentOutOfRangeException(nameof(absoluteDistance),
                "Tolerance must be finite and non-negative.");
        AbsoluteDistance = absoluteDistance;
    }

    /// <summary>Returns whether two points are no farther apart than this tolerance.</summary>
    /// <remarks>
    /// Uses a scaled distance comparison to avoid squaring large coordinate differences. If a
    /// coordinate subtraction itself overflows, the points are treated as outside any finite tolerance.
    /// </remarks>
    public bool IsNear(Vector2d left, Vector2d right)
    {
        double dx = Math.Abs(left.X - right.X);
        double dy = Math.Abs(left.Y - right.Y);
        if (!double.IsFinite(dx) || !double.IsFinite(dy))
            return false;

        double largest = Math.Max(dx, dy);
        if (largest == 0d)
            return true;
        if (largest > AbsoluteDistance)
            return false;

        // The largest component is now positive and <= tolerance, so normalized terms stay bounded.
        double normalizedDistance = Math.Sqrt(
            ((dx / largest) * (dx / largest)) +
            ((dy / largest) * (dy / largest)));
        return normalizedDistance <= AbsoluteDistance / largest;
    }
}
