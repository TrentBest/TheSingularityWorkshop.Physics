using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>An immutable finite 3-by-3 matrix with explicit row-major component names.</summary>
/// <remarks>
/// The type is unit-agnostic. Callers define whether the matrix represents a basis, linear transform,
/// covariance-like object, or another mathematical quantity. This is a small dense primitive, not a
/// general matrix/decomposition package. Operations reject non-finite results rather than returning them.
/// </remarks>
public readonly struct Matrix3x3d : IEquatable<Matrix3x3d>
{
    public double M11 { get; }
    public double M12 { get; }
    public double M13 { get; }
    public double M21 { get; }
    public double M22 { get; }
    public double M23 { get; }
    public double M31 { get; }
    public double M32 { get; }
    public double M33 { get; }

    public Matrix3x3d(
        double m11, double m12, double m13,
        double m21, double m22, double m23,
        double m31, double m32, double m33)
    {
        M11 = RequireFinite(m11, nameof(m11));
        M12 = RequireFinite(m12, nameof(m12));
        M13 = RequireFinite(m13, nameof(m13));
        M21 = RequireFinite(m21, nameof(m21));
        M22 = RequireFinite(m22, nameof(m22));
        M23 = RequireFinite(m23, nameof(m23));
        M31 = RequireFinite(m31, nameof(m31));
        M32 = RequireFinite(m32, nameof(m32));
        M33 = RequireFinite(m33, nameof(m33));
    }

    public static Matrix3x3d Identity => new(
        1d, 0d, 0d,
        0d, 1d, 0d,
        0d, 0d, 1d);

    /// <summary>Gets the determinant using the explicit cofactor expansion.</summary>
    public double Determinant => RequireFinite(
        (M11 * ((M22 * M33) - (M23 * M32))) -
        (M12 * ((M21 * M33) - (M23 * M31))) +
        (M13 * ((M21 * M32) - (M22 * M31)),
        "Determinant");

    /// <summary>Multiplies this matrix by a column vector.</summary>
    public Vector3d Transform(Vector3d value) => new(
        RequireFinite((M11 * value.X) + (M12 * value.Y) + (M13 * value.Z), "Transform X"),
        RequireFinite((M21 * value.X) + (M22 * value.Y) + (M23 * value.Z), "Transform Y"),
        RequireFinite((M31 * value.X) + (M32 * value.Y) + (M33 * value.Z), "Transform Z"));

    /// <summary>Returns the transpose.</summary>
    public Matrix3x3d Transpose() => new(
        M11, M21, M31,
        M12, M22, M32,
        M13, M23, M33);

    /// <summary>Returns the matrix product this × other.</summary>
    public Matrix3x3d Multiply(Matrix3x3d other) => new(
        DotRowColumn(M11, M12, M13, other.M11, other.M21, other.M31),
        DotRowColumn(M11, M12, M13, other.M12, other.M22, other.M32),
        DotRowColumn(M11, M12, M13, other.M13, other.M23, other.M33),
        DotRowColumn(M21, M22, M23, other.M11, other.M21, other.M31),
        DotRowColumn(M21, M22, M23, other.M12, other.M22, other.M32),
        DotRowColumn(M21, M22, M23, other.M13, other.M23, other.M33),
        DotRowColumn(M31, M32, M33, other.M11, other.M21, other.M31),
        DotRowColumn(M31, M32, M33, other.M12, other.M22, other.M32),
        DotRowColumn(M31, M32, M33, other.M13, other.M23, other.M33));

    public bool Equals(Matrix3x3d other) =>
        M11.Equals(other.M11) && M12.Equals(other.M12) && M13.Equals(other.M13) &&
        M21.Equals(other.M21) && M22.Equals(other.M22) && M23.Equals(other.M23) &&
        M31.Equals(other.M31) && M32.Equals(other.M32) && M33.Equals(other.M33);

    public override bool Equals(object? obj) => obj is Matrix3x3d other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        HashCode.Combine(M11, M12, M13),
        HashCode.Combine(M21, M22, M23),
        HashCode.Combine(M31, M32, M33));

    public static bool operator ==(Matrix3x3d left, Matrix3x3d right) => left.Equals(right);
    public static bool operator !=(Matrix3x3d left, Matrix3x3d right) => !left.Equals(right);

    private static double DotRowColumn(
        double a1, double a2, double a3,
        double b1, double b2, double b3) =>
        RequireFinite((a1 * b1) + (a2 * b2) + (a3 * b3), "Matrix product");

    private static double RequireFinite(double value, string name)
    {
        if (!double.IsFinite(value))
            throw new ArithmeticException($"{name} must be finite.");
        return value;
    }
}
