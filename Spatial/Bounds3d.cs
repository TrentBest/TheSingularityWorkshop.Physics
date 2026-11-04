namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// An axis-aligned three-dimensional box in a caller-defined coordinate frame.
/// </summary>
/// <remarks>
/// Bounds are closed: points on a face, edge, or corner are contained, and boxes
/// that only touch are considered intersecting. This type assigns no physical
/// units; the owning model must use one consistent unit and frame.
/// </remarks>
public readonly struct Bounds3d : IEquatable<Bounds3d>
{
    /// <summary>Gets the minimum corner on each axis.</summary>
    public Vector3d Min { get; }

    /// <summary>Gets the maximum corner on each axis.</summary>
    public Vector3d Max { get; }

    /// <summary>Creates bounds from component-wise minimum and maximum corners.</summary>
    /// <exception cref="ArgumentException">A minimum component exceeds its maximum component.</exception>
    public Bounds3d(Vector3d min, Vector3d max)
    {
        if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
            throw new ArgumentException("Minimum bounds must not exceed maximum bounds on any axis.", nameof(min));

        Min = min;
        Max = max;
    }

    /// <summary>Gets the extent along each axis.</summary>
    public Vector3d Size => Max - Min;

    /// <summary>Gets the center point.</summary>
    public Vector3d Center => Min + (Size * 0.5d);

    /// <summary>Gets the volume, throwing if the result is not representable as a finite double.</summary>
    public double Volume
    {
        get
        {
            var size = Size;
            var volume = size.X * size.Y * size.Z;
            if (!double.IsFinite(volume))
                throw new ArithmeticException("Bounds volume is outside the finite double range.");
            return volume;
        }
    }

    /// <summary>Returns whether the closed bounds contain a point.</summary>
    public bool Contains(Vector3d point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;

    /// <summary>Returns whether these closed bounds intersect another closed bounds.</summary>
    public bool Intersects(Bounds3d other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Returns the smallest bounds containing both boxes.</summary>
    public Bounds3d Union(Bounds3d other) => new(
        new Vector3d(
            Math.Min(Min.X, other.Min.X),
            Math.Min(Min.Y, other.Min.Y),
            Math.Min(Min.Z, other.Min.Z)),
        new Vector3d(
            Math.Max(Max.X, other.Max.X),
            Math.Max(Max.Y, other.Max.Y),
            Math.Max(Max.Z, other.Max.Z)));

    /// <summary>Tests exact corner equality.</summary>
    public bool Equals(Bounds3d other) => Min == other.Min && Max == other.Max;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Bounds3d other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Min, Max);

    /// <summary>Tests exact bounds equality.</summary>
    public static bool operator ==(Bounds3d left, Bounds3d right) => left.Equals(right);

    /// <summary>Tests exact bounds inequality.</summary>
    public static bool operator !=(Bounds3d left, Bounds3d right) => !left.Equals(right);
}
