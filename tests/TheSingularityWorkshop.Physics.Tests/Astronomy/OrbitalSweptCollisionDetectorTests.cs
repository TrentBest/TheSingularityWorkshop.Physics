using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class OrbitalSweptCollisionDetectorTests
{
    [Test]
    public void TryDetect_FastBodiesCrossBetweenEndpoints_FindsFirstContact()
    {
        var a = Body("a", new Vector3d(-5d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(5d, 0d, 0d), new Vector3d(-1d, 0d, 0d));

        var result = OrbitalSweptCollisionDetector.TryDetect(a, b, 5d);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TimeOfImpactSeconds, Is.EqualTo(4.5d).Within(1e-12));
        Assert.That(result.BodyAId, Is.EqualTo("a"));
        Assert.That(result.BodyBId, Is.EqualTo("b"));
    }

    [Test]
    public void TryDetect_IsIndependentOfInputOrder()
    {
        var a = Body("alpha", new Vector3d(-5d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("beta", new Vector3d(5d, 0d, 0d), new Vector3d(-1d, 0d, 0d));

        Assert.That(
            OrbitalSweptCollisionDetector.TryDetect(b, a, 5d),
            Is.EqualTo(OrbitalSweptCollisionDetector.TryDetect(a, b, 5d)));
    }

    [Test]
    public void TryDetect_TangentialTrajectory_FindsContactAtClosestApproach()
    {
        var moving = Body("moving", new Vector3d(-5d, 1d, 0d), new Vector3d(1d, 0d, 0d));
        var stationary = Body("stationary", Vector3d.Zero, Vector3d.Zero);

        var result = OrbitalSweptCollisionDetector.TryDetect(moving, stationary, 5d);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TimeOfImpactSeconds, Is.EqualTo(5d).Within(1e-12));
    }

    [Test]
    public void TryDetect_InitialOverlap_ReturnsZero()
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", new Vector3d(0.5d, 0d, 0d), Vector3d.Zero);

        var result = OrbitalSweptCollisionDetector.TryDetect(a, b, 1d);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TimeOfImpactSeconds, Is.Zero);
    }

    [Test]
    public void TryDetect_StationarySeparatedBodies_ReturnsNull()
    {
        var a = Body("a", new Vector3d(-2d, 0d, 0d), Vector3d.Zero);
        var b = Body("b", new Vector3d(2d, 0d, 0d), Vector3d.Zero);

        Assert.That(OrbitalSweptCollisionDetector.TryDetect(a, b, 10d), Is.Null);
    }

    [Test]
    public void TryDetect_ClosestApproachOutsideInterval_ReturnsNull()
    {
        var a = Body("a", new Vector3d(-5d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(5d, 0d, 0d), new Vector3d(-1d, 0d, 0d));

        Assert.That(OrbitalSweptCollisionDetector.TryDetect(a, b, 4d), Is.Null);
    }

    [Test]
    public void TryDetect_RejectsSameIdentity()
    {
        var a = Body("same", Vector3d.Zero, Vector3d.Zero);
        var b = Body("same", new Vector3d(1d, 0d, 0d), Vector3d.Zero);

        Assert.Throws<ArgumentException>(() => OrbitalSweptCollisionDetector.TryDetect(a, b, 1d));
    }

    [TestCase(0d)]
    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void TryDetect_RejectsInvalidInterval(double intervalSeconds)
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", new Vector3d(2d, 0d, 0d), Vector3d.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => OrbitalSweptCollisionDetector.TryDetect(a, b, intervalSeconds));
    }

    private static OrbitalBodyState Body(string id, Vector3d position, Vector3d velocity) =>
        new OrbitalBodyState(id, 1d, 0.5d, position, velocity).EnsureValid();
}
