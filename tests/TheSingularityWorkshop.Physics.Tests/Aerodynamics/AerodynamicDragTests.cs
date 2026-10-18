using NUnit.Framework;
using TheSingularityWorkshop.Physics.Aerodynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Aerodynamics;

[TestFixture]
public sealed class AerodynamicDragTests
{
    [Test]
    public void CalculateForce_OpposesRelativeAirflow()
    {
        var force = AerodynamicDrag.CalculateForce(
            new Vector3d(10d, 0d, 0d),
            Vector3d.Zero,
            airDensity: 1.2d,
            dragCoefficient: 0.5d,
            referenceArea: 2d);

        // 1/2 * 1.2 * 10² * 0.5 * 2 = 60 N.
        Assert.That(force, Is.EqualTo(new Vector3d(-60d, 0d, 0d)));
    }

    [Test]
    public void CalculateForce_UsesBodyVelocityRelativeToWind()
    {
        var bodyVelocity = new Vector3d(12d, 0d, 0d);
        var windVelocity = new Vector3d(5d, 0d, 0d);

        var force = AerodynamicDrag.CalculateForce(
            bodyVelocity, windVelocity, 1d, 1d, 1d);

        Assert.That(force, Is.EqualTo(new Vector3d(-24.5d, 0d, 0d)));
    }

    [Test]
    public void CalculateForce_ReturnsZeroWhenBodyMovesWithAir()
    {
        var velocity = new Vector3d(8d, -3d, 2d);

        var force = AerodynamicDrag.CalculateForce(
            velocity, velocity, 1.225d, 0.8d, 4d);

        Assert.That(force, Is.EqualTo(Vector3d.Zero));
    }

    [Test]
    public void CalculateForce_ReversesDirectionWhenRelativeVelocityReverses()
    {
        var forward = AerodynamicDrag.CalculateForce(
            new Vector3d(10d, 0d, 0d), Vector3d.Zero, 1d, 1d, 1d);
        var reverse = AerodynamicDrag.CalculateForce(
            new Vector3d(-10d, 0d, 0d), Vector3d.Zero, 1d, 1d, 1d);

        Assert.That(forward, Is.EqualTo(new Vector3d(-50d, 0d, 0d)));
        Assert.That(reverse, Is.EqualTo(new Vector3d(50d, 0d, 0d)));
    }

    [Test]
    public void CalculateForce_ScalesWithSquareOfRelativeSpeed()
    {
        var atFive = AerodynamicDrag.CalculateForce(
            new Vector3d(5d, 0d, 0d), Vector3d.Zero, 1d, 1d, 1d);
        var atTen = AerodynamicDrag.CalculateForce(
            new Vector3d(10d, 0d, 0d), Vector3d.Zero, 1d, 1d, 1d);

        Assert.That(atTen.X, Is.EqualTo(4d * atFive.X));
    }

    [TestCase(0d, 1d, 1d)]
    [TestCase(1d, 0d, 1d)]
    [TestCase(1d, 1d, 0d)]
    public void CalculateForce_ReturnsZeroWhenAnyScaleFactorIsZero(
        double density, double coefficient, double area)
    {
        var force = AerodynamicDrag.CalculateForce(
            new Vector3d(10d, 2d, 0d), Vector3d.Zero, density, coefficient, area);

        Assert.That(force, Is.EqualTo(Vector3d.Zero));
    }

    [Test]
    public void CalculateForce_RejectsNegativeOrNonFiniteParameters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AerodynamicDrag.CalculateForce(Vector3d.Zero, Vector3d.Zero, -1d, 1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AerodynamicDrag.CalculateForce(Vector3d.Zero, Vector3d.Zero, 1d, -1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AerodynamicDrag.CalculateForce(Vector3d.Zero, Vector3d.Zero, 1d, 1d, -1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AerodynamicDrag.CalculateForce(Vector3d.Zero, Vector3d.Zero, double.NaN, 1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AerodynamicDrag.CalculateForce(Vector3d.Zero, Vector3d.Zero, 1d, double.PositiveInfinity, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AerodynamicDrag.CalculateForce(Vector3d.Zero, Vector3d.Zero, 1d, 1d, double.NegativeInfinity));
    }
}
