namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// An immutable three-dimensional vector of finite, unit-agnostic components.
/// </summary>
/// <remarks>
/// This type does not assign physical units or a coordinate frame to its values.
/// The owning model must document those semantics (for example, metres in a
/// world-space frame). Positions, velocities, forces, and torques should not be
/// interchanged merely because they can all be represented by three numbers.
/// </remarks>
public readonly struct Vector3d : IEquatable<Vector3d>
{
    /// <summary>Gets the first component.</summary>
    public double X { get; }

    /// <summary>Gets the second component.</summary>
    public double Y { get; }

    /// <summary>Gets the third component.</summary>
    public double Z { get; }

    /// <summary>Creates a vector whose components must all be finite.</summary>
    public Vector3d(double x, double y, double z)
    {
        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x), "Vector components must be finite.");
        if (!double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(y), "Vector components must be finite.");
        if (!double.IsFinite(z))
            throw new ArgumentOutOfRangeException(nameof(z), "Vector components must be finite.");

        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>Gets the zero vector.</summary>
    public static Vector3d Zero => new(0d, 0d, 0d);

    /// <summary>Gets the squared Euclidean magnitude.</summary>
    public double MagnitudeSquared => CheckedFinite((X * X) + (Y * Y) + (Z * Z), "Magnitude squared");

    /// <summary>Gets the Euclidean magnitude, using scaled arithmetic to reduce overflow risk.</summary>
    public double Magnitude => Hypotenuse3(X, Y, Z);

    /// <summary>Returns the dot product of this vector and another.</summary>
    public double Dot(Vector3d other) =>
        CheckedFinite((X * other.X) + (Y * other.Y) + (Z * other.Z), "Dot product");

    /// <summary>Returns the cross product of this vector and another.</summary>
    public Vector3d Cross(Vector3d other) => new(
        CheckedFinite((Y * other.Z) - (Z * other.Y), "Cross product X"),
        CheckedFinite((Z * other.X) - (X * other.Z), "Cross product Y"),
        CheckedFinite((X * other.Y) - (Y * other.X), "Cross product Z"));

    /// <summary>Returns a unit vector in the same direction.</summary>
    /// <exception cref="InvalidOperationException">The vector has zero magnitude.</exception>
    public Vector3d Normalized()
    {
        var magnitude = Magnitude;
        if (magnitude == 0d)
            throw new InvalidOperationException("The zero vector has no defined direction.");

        return new Vector3d(X / magnitude, Y / magnitude, Z / magnitude);
    }

    /// <summary>Returns the distance between two vectors interpreted as points in the same frame.</summary>
    public static double Distance(Vector3d left, Vector3d right) => (left - right).Magnitude;

    /// <summary>Adds two vectors component-wise.</summary>
    public static Vector3d operator +(Vector3d left, Vector3d right) =>
        new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    /// <summary>Subtracts two vectors component-wise.</summary>
    public static Vector3d operator -(Vector3d left, Vector3d right) =>
        new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    /// <summary>Negates a vector.</summary>
    public static Vector3d operator -(Vector3d value) => new(-value.X, -value.Y, -value.Z);

    /// <summary>Scales a vector by a finite scalar.</summary>
    public static Vector3d operator *(Vector3d value, double scalar)
    {
        if (!double.IsFinite(scalar))
            throw new ArgumentOutOfRangeException(nameof(scalar), "Scale must be finite.");

        return new Vector3d(value.X * scalar, value.Y * scalar, value.Z * scalar);
    }

    /// <summary>Scales a vector by a finite scalar.</summary>
    public static Vector3d operator *(double scalar, Vector3d value) => value * scalar;

    /// <summary>Divides a vector by a finite, nonzero scalar.</summary>
    public static Vector3d operator /(Vector3d value, double scalar)
    {
        if (!double.IsFinite(scalar) || scalar == 0d)
            throw new ArgumentOutOfRangeException(nameof(scalar), "Divisor must be finite and nonzero.");

        return new Vector3d(value.X / scalar, value.Y / scalar, value.Z / scalar);
    }

    /// <inheritdoc />
    public bool Equals(Vector3d other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Vector3d other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    /// <summary>Tests exact component equality.</summary>
    public static bool operator ==(Vector3d left, Vector3d right) => left.Equals(right);

    /// <summary>Tests exact component inequality.</summary>
    public static bool operator !=(Vector3d left, Vector3d right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => $"({X:R}, {Y:R}, {Z:R})";

    private static double CheckedFinite(double value, string operation)
    {
        if (!double.IsFinite(value))
            throw new ArithmeticException($"{operation} produced a non-finite result.");
        return value;
    }

    private static double Hypotenuse3(double x, double y, double z)
    {
        var scale = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
        if (scale == 0d)
            return 0d;

        var sx = x / scale;
        var sy = y / scale;
        var sz = z / scale;
        return CheckedFinite(scale * Math.Sqrt((sx * sx) + (sy * sy) + (sz * sz)), "Magnitude");
    }
}
