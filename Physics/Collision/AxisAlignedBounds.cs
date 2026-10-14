namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>
/// Immutable axis-aligned bounds used for conservative spatial screening.
/// Bounds identify possible interaction; overlap alone does not prove contact.
/// </summary>
public readonly record struct AxisAlignedBounds
{
    /// <summary>Minimum corner.</summary>
    public (double X, double Y, double Z) Min { get; }
    /// <summary>Maximum corner.</summary>
    public (double X, double Y, double Z) Max { get; }

    /// <summary>Creates finite, ordered bounds.</summary>
    public AxisAlignedBounds(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
    {
        if (!double.IsFinite(minX)) throw new ArgumentOutOfRangeException(nameof(minX));
        if (!double.IsFinite(minY)) throw new ArgumentOutOfRangeException(nameof(minY));
        if (!double.IsFinite(minZ)) throw new ArgumentOutOfRangeException(nameof(minZ));
        if (!double.IsFinite(maxX)) throw new ArgumentOutOfRangeException(nameof(maxX));
        if (!double.IsFinite(maxY)) throw new ArgumentOutOfRangeException(nameof(maxY));
        if (!double.IsFinite(maxZ)) throw new ArgumentOutOfRangeException(nameof(maxZ));
        if (minX > maxX) throw new ArgumentException("Minimum X must not exceed maximum X.");
        if (minY > maxY) throw new ArgumentException("Minimum Y must not exceed maximum Y.");
        if (minZ > maxZ) throw new ArgumentException("Minimum Z must not exceed maximum Z.");

        Min = (minX, minY, minZ);
        Max = (maxX, maxY, maxZ);
    }

    /// <summary>Returns true when the closed bounds intersect, including touching faces.</summary>
    public bool Intersects(in AxisAlignedBounds other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Returns the smallest axis-aligned bounds containing both inputs.</summary>
    public AxisAlignedBounds Union(in AxisAlignedBounds other) =>
        new(
            Math.Min(Min.X, other.Min.X),
            Math.Min(Min.Y, other.Min.Y),
            Math.Min(Min.Z, other.Min.Z),
            Math.Max(Max.X, other.Max.X),
            Math.Max(Max.Y, other.Max.Y),
            Math.Max(Max.Z, other.Max.Z));

    /// <summary>
    /// Expands the bounds symmetrically by non-negative finite margins.
    /// Margins can conservatively cover known motion or numerical tolerance.
    /// </summary>
    public AxisAlignedBounds Expanded(double x, double y, double z)
    {
        ValidateMargin(x, nameof(x));
        ValidateMargin(y, nameof(y));
        ValidateMargin(z, nameof(z));
        return new AxisAlignedBounds(Min.X - x, Min.Y - y, Min.Z - z, Max.X + x, Max.Y + y, Max.Z + z);
    }

    /// <summary>
    /// Conservative swept bounds for linear translation by the supplied displacement.
    /// This encloses the start and end bounds; it does not model rotation or curved paths.
    /// </summary>
    public AxisAlignedBounds SweptBy(double deltaX, double deltaY, double deltaZ)
    {
        if (!double.IsFinite(deltaX)) throw new ArgumentOutOfRangeException(nameof(deltaX));
        if (!double.IsFinite(deltaY)) throw new ArgumentOutOfRangeException(nameof(deltaY));
        if (!double.IsFinite(deltaZ)) throw new ArgumentOutOfRangeException(nameof(deltaZ));
        var moved = new AxisAlignedBounds(
            Min.X + deltaX, Min.Y + deltaY, Min.Z + deltaZ,
            Max.X + deltaX, Max.Y + deltaY, Max.Z + deltaZ);
        return Union(moved);
    }

    private static void ValidateMargin(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0d)
            throw new ArgumentOutOfRangeException(name, "Margin must be finite and non-negative.");
    }
}
