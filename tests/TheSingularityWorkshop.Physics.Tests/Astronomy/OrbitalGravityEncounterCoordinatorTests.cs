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
    public void ResolveMergers_HalvingStepConvergesOnEnergyDerivedRadialReference()
    {
        var coarse = ContactTime(maximumStepSeconds: 0.5d);
        var medium = ContactTime(maximumStepSeconds: 0.25d);
        var fine = ContactTime(maximumStepSeconds: 0.125d);
        var reference = EnergyDerivedRadialContactTimeReference();

        var coarseError = Math.Abs(coarse - reference);
        var mediumError = Math.Abs(medium - reference);
        var fineError = Math.Abs(fine - reference);

        Assert.That(mediumError, Is.LessThan(coarseError),
            $"Halving the step should improve contact-time convergence: coarse={coarse:R} (error {coarseError:R}), medium={medium:R} (error {mediumError:R}), analytic={reference:R}.");
        Assert.That(fineError, Is.LessThan(mediumError),
            $"Halving the step should improve contact-time convergence again: medium={medium:R} (error {mediumError:R}), fine={fine:R} (error {fineError:R}), analytic={reference:R}.");
    }

    private static double EnergyDerivedRadialContactTimeReference()
    {
        // Relative two-body motion obeys r'' = -G(m1 + m2)/r^2. Energy conservation
        // gives |dr/dt| = sqrt(v0^2 + 2G(m1 + m2)(1/r - 1/r0)); integrate dt = -dr/(dr/dt)
        // from the contact radius (1 m) to the initial separation (10 m).
        const double gravitationalParameter = 0.001d * (1000d + 1d);
        const double initialSeparation = 10d;
        const double contactSeparation = 1d;
        const double initialRadialSpeed = 1d;
        const int intervals = 10000; // even interval count for composite Simpson integration
        var step = (initialSeparation - contactSeparation) / intervals;

        double Integrand(double separation) =>
            1d / Math.Sqrt(initialRadialSpeed * initialRadialSpeed +
                           2d * gravitationalParameter * (1d / separation - 1d / initialSeparation));

        var sum = Integrand(contactSeparation) + Integrand(initialSeparation);
        for (var i = 1; i < intervals; i++)
        {
            var weight = i % 2 == 0 ? 2d : 4d;
            sum += weight * Integrand(contactSeparation + i * step);
        }

        return sum * step / 3d;
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
