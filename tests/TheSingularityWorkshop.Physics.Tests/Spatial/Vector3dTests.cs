using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class Vector3dTests
{
    [Test]
    public void Constructor_RejectsNonFiniteComponents()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3d(double.NaN, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3d(0d, double.PositiveInfinity, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3d(0d, 0d, double.NegativeInfinity));
    }

    [Test]
    public void Arithmetic_ProducesExpectedComponents()
    {
        var left = new Vector3d(1d, 2d, 3d);
        var right = new Vector3d(4d, -2d, 0.5d);

        Assert.That(left + right, Is.EqualTo(new Vector3d(5d, 0d, 3.5d)));
        Assert.That(left - right, Is.EqualTo(new Vector3d(-3d, 4d, 2.5d)));
        Assert.That(left * 2d, Is.EqualTo(new Vector3d(2d, 4d, 6d)));
        Assert.That(left / 2d, Is.EqualTo(new Vector3d(0.5d, 1d, 1.5d)));
        Assert.That(-left, Is.EqualTo(new Vector3d(-1d, -2d, -3d)));
    }

    [Test]
    public void DotAndCross_RespectOrthogonalBasis()
    {
        var x = new Vector3d(1d, 0d, 0d);
        var y = new Vector3d(0d, 1d, 0d);

        Assert.That(x.Dot(y), Is.EqualTo(0d));
        Assert.That(x.Cross(y), Is.EqualTo(new Vector3d(0d, 0d, 1d)));
        Assert.That(y.Cross(x), Is.EqualTo(new Vector3d(0d, 0d, -1d)));
    }

    [Test]
    public void MagnitudeAndNormalization_AreCorrect()
    {
        var value = new Vector3d(3d, 4d, 0d);

        Assert.That(value.Magnitude, Is.EqualTo(5d));
        Assert.That(value.MagnitudeSquared, Is.EqualTo(25d));
        Assert.That(value.Normalized().Magnitude, Is.EqualTo(1d).Within(1e-14));
        Assert.That(Vector3d.Zero.Magnitude, Is.EqualTo(0d));
        Assert.Throws<InvalidOperationException>(() => Vector3d.Zero.Normalized());
    }

    [Test]
    public void Magnitude_UsesScaledArithmeticForLargeFiniteComponents()
    {
        var value = new Vector3d(1e200, 1e200, 0d);

        Assert.That(value.Magnitude, Is.EqualTo(Math.Sqrt(2d) * 1e200).Within(1e186));
    }

    [Test]
    public void Normalized_HandlesFiniteComponentsWhoseMagnitudeOverflows()
    {
        var value = new Vector3d(double.MaxValue, double.MaxValue, 0d);

        var normalized = value.Normalized();

        Assert.That(normalized.X, Is.EqualTo(Math.Sqrt(0.5d)).Within(1e-15));
        Assert.That(normalized.Y, Is.EqualTo(Math.Sqrt(0.5d)).Within(1e-15));
        Assert.That(normalized.Z, Is.EqualTo(0d));
        Assert.That(normalized.Magnitude, Is.EqualTo(1d).Within(1e-15));
    }

    [Test]
    public void InvalidScalarsAndOverflow_AreRejected()
    {
        var value = new Vector3d(1d, 2d, 3d);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = value * double.NaN);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = value / 0d);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = value / double.PositiveInfinity);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Vector3d(double.MaxValue, 0d, 0d) + new Vector3d(double.MaxValue, 0d, 0d));
        Assert.Throws<ArithmeticException>(() => _ = new Vector3d(double.MaxValue, 0d, 0d).Dot(new Vector3d(2d, 0d, 0d)));
    }

    [Test]
    public void Distance_UsesEuclideanMetric()
    {
        Assert.That(
            Vector3d.Distance(new Vector3d(1d, 2d, 3d), new Vector3d(4d, 6d, 3d)),
            Is.EqualTo(5d));
    }
}
