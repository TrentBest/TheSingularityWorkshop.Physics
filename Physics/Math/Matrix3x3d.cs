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
    /// <summary>Gets matrix component M11 in row-major coordinates.</summary>
    public double M11 { get; }
    /// <summary>Gets matrix component M12 in row-major coordinates.</summary>
    public double M12 { get; }
    /// <summary>Gets matrix component M13 in row-major coordinates.</summary>
    public double M13 { get; }
    /// <summary>Gets matrix component M21 in row-major coordinates.</summary>
    public double M21 { get; }
    /// <summary>Gets matrix component M22 in row-major coordinates.</summary>
    public double M22 { get; }
    /// <summary>Gets matrix component M23 in row-major coordinates.</summary>
    public double M23 { get; }
    /// <summary>Gets matrix component M31 in row-major coordinates.</summary>
    public double M31 { get; }
    /// <summary>Gets matrix component M32 in row-major coordinates.</summary>
    public double M32 { get; }
    /// <summary>Gets matrix component M33 in row-major coordinates.</summary>
    public double M33 { get; }

    /// <summary>Creates a matrix from nine row-major finite components.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A component is not finite.</exception>
    public Matrix3x3d(
        double m11, double m12, double m13,
        double m21, double m22, double m23,
        double m31, double m32, double m33)
    {
        M11 = RequireFiniteComponent(m11, nameof(m11));
        M12 = RequireFiniteComponent(m12, nameof(m12));
        M13 = RequireFiniteComponent(m13, nameof(m13));
        M21 = RequireFiniteComponent(m21, nameof(m21));
        M22 = RequireFiniteComponent(m22, nameof(m22));
        M23 = RequireFiniteComponent(m23, nameof(m23));
        M31 = RequireFiniteComponent(m31, nameof(m31));
        M32 = RequireFiniteComponent(m32, nameof(m32));
        M33 = RequireFiniteComponent(m33, nameof(m33));
    }

    /// <summary>Gets the 3-by-3 identity matrix.</summary>
    public static Matrix3x3d Identity => new(
        1d, 0d, 0d,
        0d, 1d, 0d,
        0d, 0d, 1d);

    /// <summary>Gets the determinant using the explicit cofactor expansion.</summary>
    public double Determinant => RequireFinite(
        (M11 * ((M22 * M33) - (M23 * M32))) -
        (M12 * ((M21 * M33) - (M23 * M31))) +
        (M13 * ((M21 * M32) - (M22 * M31))),
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
    /// <summary>Returns the matrix product this multiplied by other.</summary>
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

    /// <inheritdoc />
    public bool Equals(Matrix3x3d other) =>
        M11.Equals(other.M11) && M12.Equals(other.M12) && M13.Equals(other.M13) &&
        M21.Equals(other.M21) && M22.Equals(other.M22) && M23.Equals(other.M23) &&
        M31.Equals(other.M31) && M32.Equals(other.M32) && M33.Equals(other.M33);

    public override bool Equals(object? obj) => obj is Matrix3x3d other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(
        HashCode.Combine(M11, M12, M13),
        HashCode.Combine(M21, M22, M23),
        HashCode.Combine(M31, M32, M33));

    /// <summary>Determines whether two matrices have equal components.</summary>
    public static bool operator ==(Matrix3x3d left, Matrix3x3d right) => left.Equals(right);
    /// <summary>Determines whether two matrices have different components.</summary>
    public static bool operator !=(Matrix3x3d left, Matrix3x3d right) => !left.Equals(right);

    private static double DotRowColumn(
        double a1, double a2, double a3,
        double b1, double b2, double b3) =>
        RequireFinite((a1 * b1) + (a2 * b2) + (a3 * b3), "Matrix product");

    private static double RequireFiniteComponent(double value, string name)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(name, "Matrix components must be finite.");
        return value;
    }

    private static double RequireFinite(double value, string name)
    {
        if (!double.IsFinite(value))
            throw new ArithmeticException($"{name} must be finite.");
        return value;
    }
}
