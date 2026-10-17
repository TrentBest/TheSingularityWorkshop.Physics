using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class Matrix3x3dTests
{
    [Test]
    public void Identity_LeavesVectorAndMatrixUnchanged()
    {
        var vector = new Vector3d(2d, -3d, 4d);
        var matrix = new Matrix3x3d(1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d);

        Assert.That(Matrix3x3d.Identity.Transform(vector), Is.EqualTo(vector));
        Assert.That(Matrix3x3d.Identity.Multiply(matrix), Is.EqualTo(matrix));
        Assert.That(matrix.Multiply(Matrix3x3d.Identity), Is.EqualTo(matrix));
    }

    [Test]
    public void Transform_UsesRowMajorMatrixAndColumnVectorConvention()
    {
        var matrix = new Matrix3x3d(1d, 2d, 3d, 0d, 1d, 4d, 5d, 6d, 0d);

        Assert.That(matrix.Transform(new Vector3d(1d, 2d, 3d)),
            Is.EqualTo(new Vector3d(14d, 14d, 17d)));
    }

    [Test]
    public void DeterminantAndTranspose_AreCorrect()
    {
        var matrix = new Matrix3x3d(1d, 2d, 3d, 0d, 1d, 4d, 5d, 6d, 0d);

        Assert.That(matrix.Determinant, Is.EqualTo(1d));
        Assert.That(matrix.Transpose(), Is.EqualTo(
            new Matrix3x3d(1d, 0d, 5d, 2d, 1d, 6d, 3d, 4d, 0d)));
    }

    [Test]
    public void Constructor_RejectsNonFiniteComponents()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Matrix3x3d(double.NaN, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 1d));
    }

    [Test]
    public void Operations_RejectNonFiniteResults()
    {
        var matrix = new Matrix3x3d(double.MaxValue, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 1d);
        Assert.Throws<ArithmeticException>(() => matrix.Transform(new Vector3d(2d, 0d, 0d)));
    }
}
