namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>A finite 2D point tagged with a coordinate-frame identity.</summary>
/// <remarks>
/// The frame tag helps reject comparisons across differently named spaces. It does not convert
/// coordinates or prove compatible units/conventions. Conversion requires an explicit transform.
/// </remarks>
public readonly struct Point2d : IEquatable<Point2d>
{
    /// <summary>Gets this point's coordinate-frame identity.</summary>
    public CoordinateFrame2d Frame { get; }
    /// <summary>Gets this point's finite coordinates.</summary>
    public Vector2d Coordinates { get; }

    /// <summary>Creates a point from coordinates and an explicit frame identity.</summary>
    public Point2d(CoordinateFrame2d frame, Vector2d coordinates)
    {
        if (string.IsNullOrWhiteSpace(frame.Id))
            throw new ArgumentException("A valid coordinate-frame identity is required.", nameof(frame));
        Frame = frame;
        Coordinates = coordinates;
    }

    /// <inheritdoc />
    public bool Equals(Point2d other) => Frame == other.Frame && Coordinates == other.Coordinates;
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Point2d other && Equals(other);
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Frame, Coordinates);
    /// <summary>Compares frame identity and exact coordinates.</summary>
    public static bool operator ==(Point2d left, Point2d right) => left.Equals(right);
    /// <summary>Compares frame identity and exact coordinates.</summary>
    public static bool operator !=(Point2d left, Point2d right) => !left.Equals(right);

    /// <summary>Rejects a comparison between points with different frame identities.</summary>
    public void RequireSameFrame(Point2d other)
    {
        if (Frame != other.Frame)
            throw new InvalidOperationException(
                $"Cannot compare points from different coordinate frames ('{Frame.Id}' and '{other.Frame.Id}').");
    }
}
