using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class QuaterniondTests
{
    [Test]
    public void Constructor_RejectsNonFiniteComponents()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Quaterniond(double.NaN, 0d, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Quaterniond(1d, 0d, double.PositiveInfinity, 0d));
    }

    [Test]
    public void Identity_NormalizesAndLeavesVectorUnchanged()
    {
        var vector = new Vector3d(2d, -3d, 4d);

        Assert.That(Quaterniond.Identity.Norm, Is.EqualTo(1d));
        Assert.That(Quaterniond.Identity.Rotate(vector), Is.EqualTo(vector));
        Assert.That(Quaterniond.Identity.Normalized(), Is.EqualTo(Quaterniond.Identity));
    }

    [Test]
    public void Normalized_HandlesLargeFiniteComponents()
    {
        var value = new Quaterniond(1e300, -1e300, 0d, 1e300).Normalized();

        Assert.That(value.Norm, Is.EqualTo(1d).Within(1e-14));
    }

    [Test]
    public void ZeroQuaternion_CannotBeNormalizedOrUsedAsRotation()
    {
        var zero = new Quaterniond(0d, 0d, 0d, 0d);

        Assert.Throws<InvalidOperationException>(() => zero.Normalized());
        Assert.Throws<InvalidOperationException>(() => zero.Rotate(Vector3d.Zero));
    }

    [Test]
    public void QuarterTurnAroundZ_RotatesXAxisToYAxis()
    {
        var halfAngle = Math.PI / 4d;
        var rotation = new Quaterniond(Math.Cos(halfAngle), 0d, 0d, Math.Sin(halfAngle));

        var rotated = rotation.Rotate(new Vector3d(1d, 0d, 0d));

        Assert.That(rotated.X, Is.EqualTo(0d).Within(1e-14));
        Assert.That(rotated.Y, Is.EqualTo(1d).Within(1e-14));
        Assert.That(rotated.Z, Is.EqualTo(0d).Within(1e-14));
    }

    [Test]
    public void HamiltonProduct_ComposesQuarterTurns()
    {
        var halfAngle = Math.PI / 4d;
        var quarterTurn = new Quaterniond(Math.Cos(halfAngle), 0d, 0d, Math.Sin(halfAngle));

        var rotated = (quarterTurn * quarterTurn).Rotate(new Vector3d(1d, 0d, 0d));

        Assert.That(rotated.X, Is.EqualTo(-1d).Within(1e-14));
        Assert.That(rotated.Y, Is.EqualTo(0d).Within(1e-14));
        Assert.That(rotated.Z, Is.EqualTo(0d).Within(1e-14));
    }

    [Test]
    public void NormSquared_RejectsOverflow()
    {
        var value = new Quaterniond(double.MaxValue, 0d, 0d, 0d);
        Assert.Throws<ArithmeticException>(() => _ = value.NormSquared);
    }
}
