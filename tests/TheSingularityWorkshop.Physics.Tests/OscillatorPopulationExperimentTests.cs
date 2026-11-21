using NUnit.Framework;
using TheSingularityWorkshop.Physics.Timing;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class OscillatorPopulationExperimentTests
{
    private static OscillatorPopulationParticipant Timing(
        string id,
        double bpm,
        double phase = 0d,
        double start = 0d) =>
        new(id, OscillatorModelKind.IdealTiming,
            new IdealMetronomeDefinition(id, bpm, phase, 1, start));

    [Test]
    public void Evaluate_ProducesStableSortedParticipantsAndHalfOpenSamples()
    {
        var definition = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("zeta", 60d), Timing("alpha", 60d) },
            0d, 2d, 0.5d);

        var result = OscillatorPopulationExperiment.Evaluate(definition);

        Assert.That(result.OrderedParticipantIds, Is.EqualTo(new[] { "alpha", "zeta" }));
        Assert.That(result.Samples.Select(sample => sample.TimeSeconds),
            Is.EqualTo(new[] { 0d, 0.5d, 1d, 1.5d }));
        Assert.That(result.Samples.All(sample =>
            sample.Participants.Select(participant => participant.ParticipantId)
                .SequenceEqual(new[] { "alpha", "zeta" })), Is.True);
    }

    [Test]
    public void Evaluate_ReportsFullOrderParameterForInPhasePopulation()
    {
        var definition = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("a", 60d), Timing("b", 60d), Timing("c", 60d) },
            0d, 3d, 0.25d);

        var result = OscillatorPopulationExperiment.Evaluate(definition);

        Assert.That(result.Samples.All(sample =>
            sample.OrderParameterMagnitude == 1d ||
            Math.Abs(sample.OrderParameterMagnitude - 1d) < 1e-12), Is.True);
    }

    [Test]
    public void Evaluate_ReportsZeroOrderParameterForEquallySpacedPhases()
    {
        var definition = new OscillatorPopulationExperimentDefinition(
            new[]
            {
                Timing("a", 60d, 0d),
                Timing("b", 60d, 2d * Math.PI / 3d),
                Timing("c", 60d, 4d * Math.PI / 3d)
            },
            0d, 1d, 0.5d);

        var result = OscillatorPopulationExperiment.Evaluate(definition);

        Assert.That(result.Samples.All(sample =>
            sample.OrderParameterMagnitude < 1e-12), Is.True);
    }

    [Test]
    public void Evaluate_DistinguishesWrappedCoincidenceFromUnwrappedDrift()
    {
        var definition = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("slow", 60d), Timing("fast", 90d) },
            0d, 9d, 1d);

        var result = OscillatorPopulationExperiment.Evaluate(definition);
        var final = result.Samples[^1];

        Assert.That(final.TimeSeconds, Is.EqualTo(8d));
        Assert.That(final.Participants[0].WrappedPhaseRadians,
            Is.EqualTo(final.Participants[1].WrappedPhaseRadians).Within(1e-11));
        Assert.That(final.Participants[0].PhaseRadians,
            Is.Not.EqualTo(final.Participants[1].PhaseRadians));
        Assert.That(final.OrderParameterMagnitude, Is.EqualTo(1d).Within(1e-11));
    }

    [Test]
    public void Evaluate_IsDeterministicAcrossInputOrderAndRepeatedRuns()
    {
        var first = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("b", 73d, 0.2d), Timing("a", 91d, 1.1d) },
            0d, 10d, 0.125d);
        var second = first with { Participants = first.Participants.Reverse().ToArray() };

        var firstResult = OscillatorPopulationExperiment.Evaluate(first);
        var secondResult = OscillatorPopulationExperiment.Evaluate(second);

        Assert.That(secondResult.OrderedParticipantIds, Is.EqualTo(firstResult.OrderedParticipantIds));
        Assert.That(secondResult.Samples.Select(sample => sample.TimeSeconds),
            Is.EqualTo(firstResult.Samples.Select(sample => sample.TimeSeconds)));
        Assert.That(secondResult.Samples.Select(sample => sample.OrderParameterMagnitude),
            Is.EqualTo(firstResult.Samples.Select(sample => sample.OrderParameterMagnitude)));
        for (var i = 0; i < firstResult.Samples.Count; i++)
        {
            Assert.That(secondResult.Samples[i].Participants,
                Is.EqualTo(firstResult.Samples[i].Participants));
        }
    }

    [Test]
    public void Validate_RejectsDuplicateIdentityUnsupportedKindAndInvalidWindows()
    {
        var duplicate = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("same", 60d), Timing("same", 90d) }, 0d, 2d, 0.5d);
        var unsupported = new OscillatorPopulationExperimentDefinition(
            new[]
            {
                new OscillatorPopulationParticipant(
                    "body", (OscillatorModelKind)99,
                    new IdealMetronomeDefinition("body", 60d, 0d, 1, 0d))
            }, 0d, 2d, 0.5d);
        var beforeStart = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("later", 60d, start: 2d) }, 1d, 3d, 0.5d);

        Assert.Throws<ArgumentException>(() => OscillatorPopulationExperiment.Evaluate(duplicate));
        Assert.Throws<ArgumentOutOfRangeException>(() => OscillatorPopulationExperiment.Evaluate(unsupported));
        Assert.Throws<ArgumentOutOfRangeException>(() => OscillatorPopulationExperiment.Evaluate(beforeStart));
        Assert.Throws<ArgumentOutOfRangeException>(() => OscillatorPopulationExperiment.Evaluate(
            new OscillatorPopulationExperimentDefinition(
                new[] { Timing("a", 60d) }, 0d, 1d, 0d)));
    }

    [Test]
    public void Validate_EnforcesSampleBudget()
    {
        var definition = new OscillatorPopulationExperimentDefinition(
            new[] { Timing("a", 60d) }, 0d, 10d, 0.1d, MaximumSamples: 10);

        Assert.Throws<InvalidOperationException>(() => OscillatorPopulationExperiment.Evaluate(definition));
    }

    [Test]
    public void WrapPositive_HandlesNegativeAndLargePhases()
    {
        Assert.That(OscillatorPopulationExperiment.WrapPositive(-Math.PI),
            Is.EqualTo(Math.PI).Within(1e-12));
        Assert.That(OscillatorPopulationExperiment.WrapPositive(4d * Math.PI),
            Is.EqualTo(0d).Within(1e-12));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OscillatorPopulationExperiment.WrapPositive(double.NaN));
    }
}
