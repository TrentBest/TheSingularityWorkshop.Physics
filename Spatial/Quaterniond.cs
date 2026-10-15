namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// An immutable quaternion for representing three-dimensional orientation.
/// </summary>
/// <remarks>
/// Components are stored in scalar-first form (W, X, Y, Z). Rotation methods
/// interpret a normalized quaternion as an active rotation in a right-handed
/// coordinate frame. The type does not define a world frame or physical units.
/// Normalize before treating a value as an orientation.
/// </remarks>
public readonly struct Quaterniond : IEquatable<Quaterniond>
{
    /// <summary>Gets the scalar component.</summary>
    public double W { get; }

    /// <summary>Gets the first vector component.</summary>
    public double X { get; }

    /// <summary>Gets the second vector component.</summary>
    public double Y { get; }

    /// <summary>Gets the third vector component.</summary>
    public double Z { get; }

    /// <summary>Creates a quaternion with finite components.</summary>
    public Quaterniond(double w, double x, double y, double z)
    {
        if (!double.IsFinite(w))
            throw new ArgumentOutOfRangeException(nameof(w), "Quaternion components must be finite.");
        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x), "Quaternion components must be finite.");
        if (!double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(y), "Quaternion components must be finite.");
        if (!double.IsFinite(z))
            throw new ArgumentOutOfRangeException(nameof(z), "Quaternion components must be finite.");

        W = w;
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>Gets the identity orientation.</summary>
    public static Quaterniond Identity => new(1d, 0d, 0d, 0d);

    /// <summary>Gets the squared norm, throwing if the result is not finite.</summary>
    public double NormSquared => CheckedFinite((W * W) + (X * X) + (Y * Y) + (Z * Z), "Squared norm");

    /// <summary>Gets the norm using scaled arithmetic to reduce intermediate overflow.</summary>
    public double Norm
    {
        get
        {
            var scale = Math.Max(Math.Max(Math.Abs(W), Math.Abs(X)), Math.Max(Math.Abs(Y), Math.Abs(Z)));
            if (scale == 0d)
                return 0d;

            var w = W / scale;
            var x = X / scale;
            var y = Y / scale;
            var z = Z / scale;
            return CheckedFinite(scale * Math.Sqrt((w * w) + (x * x) + (y * y) + (z * z)), "Norm");
        }
    }

    /// <summary>Returns this quaternion normalized to unit norm.</summary>
    /// <exception cref="InvalidOperationException">The quaternion has zero norm.</exception>
    public Quaterniond Normalized()
    {
        var scale = Math.Max(Math.Max(Math.Abs(W), Math.Abs(X)), Math.Max(Math.Abs(Y), Math.Abs(Z)));
        if (scale == 0d)
            throw new InvalidOperationException("A zero quaternion cannot represent an orientation.");

        var w = W / scale;
        var x = X / scale;
        var y = Y / scale;
        var z = Z / scale;
        var norm = Math.Sqrt((w * w) + (x * x) + (y * y) + (z * z));
        return new Quaterniond(w / norm, x / norm, y / norm, z / norm);
    }

    /// <summary>Returns the quaternion conjugate.</summary>
    public Quaterniond Conjugate() => new(W, -X, -Y, -Z);

    /// <summary>Rotates a vector using this quaternion as an orientation.</summary>
    /// <remarks>The quaternion is normalized internally. The input and output vectors must use the same frame convention.</remarks>
    public Vector3d Rotate(Vector3d value)
    {
        var rotation = Normalized();
        var scale = Math.Max(Math.Abs(value.X), Math.Max(Math.Abs(value.Y), Math.Abs(value.Z)));
        if (scale == 0d)
            return Vector3d.Zero;

        // Rotate a bounded vector so intermediate cross products cannot overflow
        // merely because the original finite vector is close to double.MaxValue.
        // Scaling the result back may still throw if a true output component is unrepresentable.
        var scaledValue = value / scale;
        var vectorPart = new Vector3d(rotation.X, rotation.Y, rotation.Z);
        var twiceCross = 2d * vectorPart.Cross(scaledValue);
        var rotated = scaledValue + (rotation.W * twiceCross) + vectorPart.Cross(twiceCross);
        return rotated * scale;
    }

    /// <summary>Multiplies quaternions using the Hamilton product.</summary>
    /// <remarks>For active rotations, the product represents composition; the right operand is applied first.</remarks>
    public static Quaterniond operator *(Quaterniond left, Quaterniond right) => new(
        CheckedFinite((left.W * right.W) - (left.X * right.X) - (left.Y * right.Y) - (left.Z * right.Z), "Hamilton product W"),
        CheckedFinite((left.W * right.X) + (left.X * right.W) + (left.Y * right.Z) - (left.Z * right.Y), "Hamilton product X"),
        CheckedFinite((left.W * right.Y) - (left.X * right.Z) + (left.Y * right.W) + (left.Z * right.X), "Hamilton product Y"),
        CheckedFinite((left.W * right.Z) + (left.X * right.Y) - (left.Y * right.X) + (left.Z * right.W), "Hamilton product Z"));

    /// <inheritdoc />
    public bool Equals(Quaterniond other) =>
        W.Equals(other.W) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Quaterniond other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(W, X, Y, Z);

    /// <summary>Tests exact component equality.</summary>
    public static bool operator ==(Quaterniond left, Quaterniond right) => left.Equals(right);

    /// <summary>Tests exact component inequality.</summary>
    public static bool operator !=(Quaterniond left, Quaterniond right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => $"({W:R}, {X:R}, {Y:R}, {Z:R})";

    private static double CheckedFinite(double value, string operation)
    {
        if (!double.IsFinite(value))
            throw new ArithmeticException($"{operation} produced a non-finite result.");
        return value;
    }
}
