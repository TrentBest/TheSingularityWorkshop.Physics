using System;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Indices of three distinct vertices forming one triangle in an input polygon boundary.</summary>
/// <remarks>
/// Indices refer to the original effective boundary supplied to <see cref="Geometry2dPolygon.Triangulate"/>.
/// A repeated closing vertex is not part of that effective boundary.
/// </remarks>
public readonly struct TriangleIndices2d : IEquatable<TriangleIndices2d>
{
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

    public int AIndex { get; }
    public int BIndex { get; }
    public int CIndex { get; }

    public bool Equals(TriangleIndices2d other) =>
        AIndex == other.AIndex && BIndex == other.BIndex && CIndex == other.CIndex;

    public override bool Equals(object? obj) => obj is TriangleIndices2d other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(AIndex, BIndex, CIndex);

    public static bool operator ==(TriangleIndices2d left, TriangleIndices2d right) => left.Equals(right);
    public static bool operator !=(TriangleIndices2d left, TriangleIndices2d right) => !left.Equals(right);
}
