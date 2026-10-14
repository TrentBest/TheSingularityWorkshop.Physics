namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Defines a finite, axis-aligned address grid for deterministic universe fixtures and
/// bounded regions. Coordinates are zero-based and X varies fastest in linear order.
/// This address helper does not imply that physical influence is limited to a cell.
/// </summary>
public sealed class UniverseGrid
{
    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }
    public int CellCount { get; }

    public UniverseGrid(int width, int height, int depth)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be positive.");

        Width = width;
        Height = height;
        Depth = depth;
        CellCount = checked(checked(width * height) * depth);
    }

    /// <summary>Maps coordinates to a stable linear index with X as the fastest axis.</summary>
    public int GetLinearIndex(int x, int y, int z)
    {
        ValidateCoordinates(x, y, z);
        return checked(x + Width * checked(y + Height * z));
    }

    /// <summary>Maps a stable linear index back to its coordinates.</summary>
    public (int X, int Y, int Z) GetCoordinates(int index)
    {
        if ((uint)index >= (uint)CellCount)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must identify a cell in this grid.");

        var x = index % Width;
        var yz = index / Width;
        var y = yz % Height;
        var z = yz / Height;
        return (x, y, z);
    }

    private void ValidateCoordinates(int x, int y, int z)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        if ((uint)z >= (uint)Depth) throw new ArgumentOutOfRangeException(nameof(z));
    }
}
