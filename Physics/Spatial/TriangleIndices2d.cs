using System;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Indices of three distinct vertices forming one triangle in an input polygon boundary.</summary>
/// <remarks>
/// Indices refer to the original effective boundary supplied to <see cref="Geometry2dPolygon.Triangulate"/>.
/// A repeated closing vertex is not part of that effective boundary.
/// </remarks>
public readonly struct TriangleIndices2d : IEquatable<TriangleIndices2d>
{
    /// <summary>Creates a triangle index triple with distinct, non-negative indices.</summary>
    /// <param name="aIndex">First vertex index.</param>
    /// <param name="bIndex">Second vertex index.</param>
    /// <param name="cIndex">Third vertex index.</param>
    /// <exception cref="ArgumentOutOfRangeException">An index is negative.</exception>
    /// <exception cref="ArgumentException">Two or more indices are equal.</exception>
    public TriangleIndices2d(int aIndex, int bIndex, int cIndex)
    {
        if (aIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(aIndex));
        if (bIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(bIndex));
        if (cIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(cIndex));
        if (aIndex == bIndex || bIndex == cIndex || cIndex == aIndex)
            throw new ArgumentException("Triangle vertex indices must be distinct.");

        AIndex = aIndex;
        BIndex = bIndex;
        CIndex = cIndex;
    }

    /// <summary>Gets the first vertex index in the triangle's preserved winding order.</summary>
    public int AIndex { get; }
    /// <summary>Gets the second vertex index in the triangle's preserved winding order.</summary>
    public int BIndex { get; }
    /// <summary>Gets the third vertex index in the triangle's preserved winding order.</summary>
    public int CIndex { get; }

    /// <inheritdoc />
    public bool Equals(TriangleIndices2d other) =>
        AIndex == other.AIndex && BIndex == other.BIndex && CIndex == other.CIndex;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TriangleIndices2d other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(AIndex, BIndex, CIndex);

    /// <summary>Returns whether all three ordered indices are equal.</summary>
    public static bool operator ==(TriangleIndices2d left, TriangleIndices2d right) => left.Equals(right);
    /// <summary>Returns whether any ordered index differs.</summary>
    public static bool operator !=(TriangleIndices2d left, TriangleIndices2d right) => !left.Equals(right);
}
