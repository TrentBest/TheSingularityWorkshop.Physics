using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class OrbitalGravityEncounterCoordinatorTests
{
    private const double NegligibleGravity = 1e-20d;

    [Test]
    public void ResolveMergers_FastBodiesCrossBetweenStepEndpoints_MergesAtEstimatedContact()
    {
        var a = Body("a", new Vector3d(-5d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(5d, 0d, 0d), new Vector3d(-1d, 0d, 0d));

        var result = OrbitalGravityEncounterCoordinator.ResolveMergers(
            [a, b], intervalSeconds: 5d, maximumStepSeconds: 0.25d,
            gravitationalConstantM3PerKgSecondSquared: NegligibleGravity);

        Assert.That(result.MergeEvents, Has.Count.EqualTo(1));
        Assert.That(result.FinalBodies, Has.Count.EqualTo(1));
        Assert.That(result.FinalBodies[0].MassKilograms, Is.EqualTo(2d));
        Assert.That(result.FinalBodies[0].PositionMeters.X, Is.EqualTo(0d).Within(1e-8));
        Assert.That(result.ElapsedSeconds, Is.EqualTo(5d));
    }

    [Test]
    public void ResolveMergers_IsDeterministicAndIndependentOfInputOrder()
    {
        var a = Body("a", new Vector3d(-5d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("b", new Vector3d(5d, 0d, 0d), new Vector3d(-1d, 0d, 0d));
        var c = Body("c", new Vector3d(0d, 100d, 0d), Vector3d.Zero);

        var forward = OrbitalGravityEncounterCoordinator.ResolveMergers(
            [a, b, c], 5d, 0.25d, NegligibleGravity);
        var reverse = OrbitalGravityEncounterCoordinator.ResolveMergers(
            [c, b, a], 5d, 0.25d, NegligibleGravity);

        Assert.That(reverse.FinalBodies, Is.EqualTo(forward.FinalBodies));
        Assert.That(reverse.MergeEvents, Is.EqualTo(forward.MergeEvents));
    }

    [Test]
    public void ResolveMergers_InitiallyCoincidentBodiesMergeBeforeSingularGravityEvaluation()
    {
        var a = Body("a", Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", Vector3d.Zero, Vector3d.Zero);

        var result = OrbitalGravityEncounterCoordinator.ResolveMergers(
            [a, b], intervalSeconds: 1d, maximumStepSeconds: 0.1d,
            gravitationalConstantM3PerKgSecondSquared: 1d);

        Assert.That(result.MergeEvents, Has.Count.EqualTo(1));
        Assert.That(result.FinalBodies, Has.Count.EqualTo(1));
        Assert.That(result.FinalBodies[0].MassKilograms, Is.EqualTo(2d));
        Assert.That(result.FinalBodies[0].PositionMeters.X, Is.EqualTo(0d).Within(1e-12));
    }

    [Test]
    public void ResolveMergers_HalvingStepConvergesOnEstimatedGravitationalContactTime()
    {
        var coarse = ContactTime(maximumStepSeconds: 0.5d);
        var medium = ContactTime(maximumStepSeconds: 0.25d);
        var fine = ContactTime(maximumStepSeconds: 0.125d);
        var reference = ContactTime(maximumStepSeconds: 0.03125d);

        Assert.That(Math.Abs(medium - reference), Is.LessThan(Math.Abs(coarse - reference)),
            $"Halving the step should improve contact-time convergence: coarse={coarse:R}, medium={medium:R}, reference={reference:R}.");
        Assert.That(Math.Abs(fine - reference), Is.LessThan(Math.Abs(medium - reference)),
            $"Halving the step should improve contact-time convergence again: medium={medium:R}, fine={fine:R}, reference={reference:R}.");
    }

    private static double ContactTime(double maximumStepSeconds)
    {
        var central = new OrbitalBodyState(
            "central", 1000d, 0.75d, Vector3d.Zero, Vector3d.Zero);
        var incoming = new OrbitalBodyState(
            "incoming", 1d, 0.25d, new Vector3d(10d, 0d, 0d), new Vector3d(-1d, 0d, 0d));

        var result = OrbitalGravityEncounterCoordinator.ResolveMergers(
            [central, incoming],
            intervalSeconds: 10d,
            maximumStepSeconds,
            gravitationalConstantM3PerKgSecondSquared: 0.001d);

        Assert.That(result.TimedMergeEvents, Has.Count.EqualTo(1));
        Assert.That(result.TimedMergeEvents[0].TimeSeconds, Is.InRange(0d, 10d));
        Assert.That(result.TimedMergeEvents[0].MergeEvent, Is.EqualTo(result.MergeEvents[0]));
        return result.TimedMergeEvents[0].TimeSeconds;
    }

    [Test]
    public void ResolveMergers_RejectsInvalidStepDuration()
    {
        var body = Body("body", Vector3d.Zero, Vector3d.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OrbitalGravityEncounterCoordinator.ResolveMergers([body], 1d, 0d, 1d));
    }

    private static OrbitalBodyState Body(string id, Vector3d position, Vector3d velocity) =>
        new OrbitalBodyState(id, 1d, 0.5d, position, velocity);
}
