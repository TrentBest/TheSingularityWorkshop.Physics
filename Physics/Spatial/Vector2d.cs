namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>An immutable finite two-dimensional vector in a caller-defined coordinate frame.</summary>
/// <remarks>
/// Components are unit-agnostic. This type does not decide whether coordinates are metres, pixels,
/// or another quantity. It uses exact floating-point equality; geometric tolerances belong to the
/// predicate or solver that defines the relevant scale.
/// </remarks>
public readonly struct Vector2d : IEquatable<Vector2d>
{
    /// <summary>Gets the horizontal or first-axis component.</summary>
    public double X { get; }
    /// <summary>Gets the vertical or second-axis component.</summary>
    public double Y { get; }

    /// <summary>Creates a vector with finite components.</summary>
    public Vector2d(double x, double y)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x), "Component must be finite.");
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y), "Component must be finite.");
        X = x;
        Y = y;
    }

    /// <summary>Gets the squared Euclidean length without a square root.</summary>
    public double LengthSquared => RequireFinite((X * X) + (Y * Y), "LengthSquared");

    /// <summary>Gets the Euclidean length.</summary>
    public double Length => Math.Sqrt(LengthSquared);

    /// <summary>Computes the dot product.</summary>
    public double Dot(Vector2d other) => RequireFinite((X * other.X) + (Y * other.Y), "Dot");

    /// <summary>Computes the signed 2D cross product z component.</summary>
    /// <remarks>Positive values indicate a counter-clockwise turn from this vector to the other vector.</remarks>
    public double Cross(Vector2d other) => RequireFinite((X * other.Y) - (Y * other.X), "Cross");

    /// <summary>Computes squared Euclidean distance to another vector interpreted as a point.</summary>
    public double DistanceSquaredTo(Vector2d other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return RequireFinite((dx * dx) + (dy * dy), "DistanceSquaredTo");
    }

    /// <summary>Adds two vectors.</summary>
    public static Vector2d operator +(Vector2d left, Vector2d right) =>
        new(RequireFinite(left.X + right.X, "Add.X"), RequireFinite(left.Y + right.Y, "Add.Y"));

    /// <summary>Subtracts the right vector from the left vector.</summary>
    public static Vector2d operator -(Vector2d left, Vector2d right) =>
        new(RequireFinite(left.X - right.X, "Subtract.X"), RequireFinite(left.Y - right.Y, "Subtract.Y"));

    /// <summary>Scales a vector by a finite scalar.</summary>
    public static Vector2d operator *(Vector2d vector, double scalar)
    {
        if (!double.IsFinite(scalar)) throw new ArgumentOutOfRangeException(nameof(scalar), "Scalar must be finite.");
        return new Vector2d(RequireFinite(vector.X * scalar, "Scale.X"), RequireFinite(vector.Y * scalar, "Scale.Y"));
    }

    /// <summary>Scales a vector by a finite scalar.</summary>
    public static Vector2d operator *(double scalar, Vector2d vector) => vector * scalar;

    /// <summary>Divides a vector by a finite, nonzero scalar.</summary>
    public static Vector2d operator /(Vector2d vector, double scalar)
    {
        if (!double.IsFinite(scalar) || scalar == 0d)
            throw new ArgumentOutOfRangeException(nameof(scalar), "Scalar must be finite and nonzero.");
        return new Vector2d(RequireFinite(vector.X / scalar, "Divide.X"), RequireFinite(vector.Y / scalar, "Divide.Y"));
    }

    /// <inheritdoc />
    public bool Equals(Vector2d other) => X.Equals(other.X) && Y.Equals(other.Y);
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Vector2d other && Equals(other);
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);
    /// <summary>Compares two vectors using exact component equality.</summary>
    public static bool operator ==(Vector2d left, Vector2d right) => left.Equals(right);
    /// <summary>Compares two vectors using exact component inequality.</summary>
    public static bool operator !=(Vector2d left, Vector2d right) => !left.Equals(right);

    private static double RequireFinite(double value, string operation)
    {
        if (!double.IsFinite(value)) throw new ArithmeticException($"Vector2d {operation} produced a non-finite result.");
        return value;
    }
}
