using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class Vector2dTests
{
    [Test]
    public void DotAndCross_UseTwoDimensionalConventions()
    {
        var x = new Vector2d(1d, 0d);
        var y = new Vector2d(0d, 1d);
        Assert.That(x.Dot(y), Is.EqualTo(0d));
        Assert.That(x.Cross(y), Is.EqualTo(1d));
        Assert.That(y.Cross(x), Is.EqualTo(-1d));
    }

    [Test]
    public void ArithmeticAndDistance_AreConsistent()
    {
        var a = new Vector2d(3d, 4d);
        var b = new Vector2d(0d, 0d);
        Assert.That(a.Length, Is.EqualTo(5d));
        Assert.That(a.DistanceSquaredTo(b), Is.EqualTo(25d));
        Assert.That(a + b, Is.EqualTo(a));
        Assert.That(a / 2d, Is.EqualTo(new Vector2d(1.5d, 2d)));
    }

    [Test]
    public void Constructor_RejectsNonFiniteComponents()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vector2d(double.NaN, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vector2d(0d, double.PositiveInfinity));
    }

    [Test]
    public void Arithmetic_RejectsOverflowInsteadOfReturningInfinity()
    {
        var huge = new Vector2d(double.MaxValue, 0d);
        Assert.Throws<ArithmeticException>(() => _ = huge.LengthSquared);
        Assert.Throws<ArithmeticException>(() => _ = huge * 2d);
    }

    [Test]
    public void Division_RejectsZeroAndNonFiniteScalars()
    {
        var value = new Vector2d(1d, 2d);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = value / 0d);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = value / double.NaN);
    }
}
