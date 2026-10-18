using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class OrbitalMergeResolverTests
{
    [Test]
    public void TryMerge_SeparatedBodies_ReturnsNull()
    {
        var a = Body("a", 2d, 1d, new Vector3d(-2d, 0d, 0d), Vector3d.Zero);
        var b = Body("b", 3d, 1d, new Vector3d(2d, 0d, 0d), Vector3d.Zero);

        Assert.That(OrbitalMergeResolver.TryMerge(a, b), Is.Null);
    }

    [Test]
    public void TryMerge_OverlappingBodies_ConservesMassCentreOfMassAndLinearMomentum()
    {
        var a = Body("a", 2d, 1d, new Vector3d(-0.5d, 0d, 0d), new Vector3d(3d, 1d, 0d));
        var b = Body("b", 3d, 2d, new Vector3d(0.5d, 0d, 0d), new Vector3d(-1d, 2d, 0d));

        var result = OrbitalMergeResolver.TryMerge(a, b);

        Assert.That(result, Is.Not.Null);
        var merged = result!.MergedBody;
        Assert.That(merged.MassKilograms, Is.EqualTo(5d));
        AssertVector(merged.PositionMeters, new Vector3d(-0.1d, 0d, 0d));
        AssertVector(merged.VelocityMetersPerSecond, new Vector3d(0.6d, 1.6d, 0d));

        var expectedRadius = Math.Cbrt(1d + 8d);
        Assert.That(merged.RadiusMeters, Is.EqualTo(expectedRadius).Within(1e-12));

        var momentumBefore = a.VelocityMetersPerSecond * a.MassKilograms +
                            b.VelocityMetersPerSecond * b.MassKilograms;
        var momentumAfter = merged.VelocityMetersPerSecond * merged.MassKilograms;
        AssertVector(momentumAfter, momentumBefore);
        Assert.That(result.RelativeKineticEnergyJoules, Is.GreaterThan(0d));
    }

    [Test]
    public void TryMerge_TouchingBodies_IsAnEncounterAndEventIsInputOrderIndependent()
    {
        var a = Body("alpha", 1d, 0.5d, new Vector3d(-0.5d, 0d, 0d), Vector3d.Zero);
        var b = Body("beta", 1d, 0.5d, new Vector3d(0.5d, 0d, 0d), new Vector3d(1d, 0d, 0d));

        var forward = OrbitalMergeResolver.TryMerge(a, b);
        var reverse = OrbitalMergeResolver.TryMerge(b, a);

        Assert.That(forward, Is.Not.Null);
        Assert.That(reverse, Is.EqualTo(forward));
        Assert.That(forward!.RelativeKineticEnergyJoules, Is.EqualTo(0.25d).Within(1e-12));
    }

    [Test]
    public void TryMerge_RejectsSameIdentity()
    {
        var a = Body("same", 1d, 1d, Vector3d.Zero, Vector3d.Zero);
        var b = Body("same", 2d, 1d, new Vector3d(1d, 0d, 0d), Vector3d.Zero);

        Assert.Throws<ArgumentException>(() => OrbitalMergeResolver.TryMerge(a, b));
    }

    private static OrbitalBodyState Body(
        string id, double mass, double radius, Vector3d position, Vector3d velocity) =>
        new OrbitalBodyState(id, mass, radius, position, velocity).EnsureValid();

    private static void AssertVector(Vector3d actual, Vector3d expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(1e-12));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1e-12));
        Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(1e-12));
    }
}
