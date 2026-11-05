using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Experiments;

[TestFixture]
public sealed class ConstantAccelerationSphereCollisionExperimentTests
{
    [Test]
    public void Predict_ConstantVelocityMatchesSweptSphereBaseline()
    {
        var a = Body("a", -5d, 1d);
        var b = Body("b", 5d, -1d);
        var baseline = OrbitalSweptCollisionDetector.TryDetect(a, b, 5d);
        var result = Predict(a, Vector3d.Zero, b, Vector3d.Zero, 5d);

        Assert.That(baseline, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(AcceleratedSpherePredictionStatus.SampledOverlap));
        Assert.That(result.EarliestPossibleSeconds, Is.LessThanOrEqualTo(4.5d));
        Assert.That(result.LatestPossibleSeconds, Is.GreaterThanOrEqualTo(4.5d));
    }

    [Test]
    public void Predict_AcceleratedApproachFindsBracketedOverlap()
    {
        var a = Body("a", -5d, 0d);
        var b = Body("b", 5d, 0d);
        var result = Predict(a, new Vector3d(1d, 0d, 0d), b, new Vector3d(-1d, 0d, 0d), 4d);

        Assert.That(result.Status, Is.EqualTo(AcceleratedSpherePredictionStatus.SampledOverlap));
        Assert.That(result.EarliestPossibleSeconds, Is.LessThanOrEqualTo(3d));
        Assert.That(result.LatestPossibleSeconds, Is.GreaterThanOrEqualTo(3d));
    }

    [Test]
    public void Predict_AcceleratedSeparationProvesNoContact()
    {
        var a = Body("a", -2d, -1d);
        var b = Body("b", 2d, 1d);
        var result = Predict(a, new Vector3d(-1d, 0d, 0d), b, new Vector3d(1d, 0d, 0d), 2d);

        Assert.That(result.Status, Is.EqualTo(AcceleratedSpherePredictionStatus.ProvenNoContact));
    }

    [Test]
    public void Predict_OffsetGrazingNearMissRemainsUnresolvedRatherThanInventingContact()
    {
        var a = new OrbitalBodyState("a", 1d, 0.5d,
            new Vector3d(-2d, 1.000001d, 0d), new Vector3d(1d, 0d, 0d));
        var b = new OrbitalBodyState("b", 1d, 0.5d, Vector3d.Zero, Vector3d.Zero);
        var result = Predict(a, Vector3d.Zero, b, Vector3d.Zero, 4d, tolerance: 1e-5);

        Assert.That(result.Status, Is.EqualTo(AcceleratedSpherePredictionStatus.UnresolvedPossibleContact));
    }

    [Test]
    public void Predict_BudgetExhaustionIsExplicitlyUnresolved()
    {
        var a = Body("a", -5d, 1d);
        var b = Body("b", 5d, -1d);
        var result = Predict(a, Vector3d.Zero, b, Vector3d.Zero, 5d, budget: 2);

        Assert.That(result.Status, Is.EqualTo(AcceleratedSpherePredictionStatus.UnresolvedPossibleContact));
        Assert.That(result.Evaluations, Is.EqualTo(2));
    }

    [Test]
    public void Predict_IsDeterministicAcrossRepeatedCalls()
    {
        var a = Body("a", -5d, 1d);
        var b = Body("b", 5d, -1d);
        var first = Predict(a, new Vector3d(0.1d, 0d, 0d), b, new Vector3d(-0.1d, 0d, 0d), 5d);
        var second = Predict(a, new Vector3d(0.1d, 0d, 0d), b, new Vector3d(-0.1d, 0d, 0d), 5d);

        Assert.That(second, Is.EqualTo(first));
    }

    private static AcceleratedSpherePrediction Predict(
        OrbitalBodyState a, Vector3d accelerationA,
        OrbitalBodyState b, Vector3d accelerationB,
        double interval, double tolerance = 1e-4, int budget = 4096) =>
        ConstantAccelerationSphereCollisionExperiment.Predict(
            a, accelerationA, b, accelerationB, interval, tolerance, budget);

    private static OrbitalBodyState Body(string id, double x, double velocity) =>
        new OrbitalBodyState(id, 1d, 0.5d,
            new Vector3d(x, 0d, 0d), new Vector3d(velocity, 0d, 0d)).EnsureValid();
}
