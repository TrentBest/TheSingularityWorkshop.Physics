namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Stable identity for a caller-defined two-dimensional coordinate frame.</summary>
/// <remarks>
/// A frame identifier distinguishes coordinate spaces; it does not define units, axis orientation,
/// origin, or conversion. The owner must document those conventions. Identifiers use ordinal,
/// case-sensitive equality.
/// </remarks>
public readonly struct CoordinateFrame2d : IEquatable<CoordinateFrame2d>
{
    /// <summary>Gets the stable, non-empty identifier for this frame.</summary>
    public string Id { get; }

    /// <summary>Creates a frame identity from a stable, non-empty identifier.</summary>
    public CoordinateFrame2d(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Frame identifier must be non-empty.", nameof(id));
        Id = id;
    }

    /// <inheritdoc />
    public bool Equals(CoordinateFrame2d other) => string.Equals(Id, other.Id, StringComparison.Ordinal);
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CoordinateFrame2d other && Equals(other);
    /// <inheritdoc />
    public override int GetHashCode() => Id is null ? 0 : StringComparer.Ordinal.GetHashCode(Id);
    /// <summary>Compares frame identities.</summary>
    public static bool operator ==(CoordinateFrame2d left, CoordinateFrame2d right) => left.Equals(right);
    /// <summary>Compares frame identities.</summary>
    public static bool operator !=(CoordinateFrame2d left, CoordinateFrame2d right) => !left.Equals(right);
}
