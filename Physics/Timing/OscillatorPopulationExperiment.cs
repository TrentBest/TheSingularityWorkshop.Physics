namespace TheSingularityWorkshop.Physics.Timing;

/// <summary>Supported oscillator models for population-level phase experiments.</summary>
public enum OscillatorModelKind
{
    /// <summary>An ideal, constant-frequency timing signal with no mechanical body.</summary>
    IdealTiming = 1
}

/// <summary>
/// One explicitly typed participant in an ideal timing population experiment.
/// The participant ID is the stable identity used in deterministic result ordering.
/// </summary>
public sealed record OscillatorPopulationParticipant(
    string ParticipantId,
    OscillatorModelKind ModelKind,
    IdealMetronomeDefinition TimingDefinition)
{
    /// <summary>Validates identity, model kind, and agreement with the timing model identity.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ParticipantId))
            throw new ArgumentException("A stable participant ID is required.", nameof(ParticipantId));

        if (ModelKind != OscillatorModelKind.IdealTiming)
            throw new ArgumentOutOfRangeException(nameof(ModelKind), "The selected oscillator model is not supported by this experiment.");

        ArgumentNullException.ThrowIfNull(TimingDefinition);
        TimingDefinition.Validate();

        if (!string.Equals(ParticipantId, TimingDefinition.ParticipantId, StringComparison.Ordinal))
            throw new ArgumentException("Participant ID must exactly match the timing definition ID.", nameof(TimingDefinition));
    }
}

/// <summary>
/// Reproducible sampling configuration for a population of independent ideal timing oscillators.
/// Samples are taken at start + i * SampleIntervalSeconds in the half-open interval [start, end).
/// </summary>
public sealed record OscillatorPopulationExperimentDefinition(
    IReadOnlyList<OscillatorPopulationParticipant> Participants,
    double StartTimeSeconds,
    double EndTimeSeconds,
    double SampleIntervalSeconds,
    int MaximumSamples = 100_000)
{
    /// <summary>Validates and returns participants in stable ordinal ID order.</summary>
    public IReadOnlyList<OscillatorPopulationParticipant> ValidateAndOrderParticipants()
    {
        ArgumentNullException.ThrowIfNull(Participants);

        if (Participants.Count == 0)
            throw new ArgumentException("At least one participant is required.", nameof(Participants));
        if (!double.IsFinite(StartTimeSeconds) || StartTimeSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(StartTimeSeconds), "Start time must be finite and non-negative.");
        if (!double.IsFinite(EndTimeSeconds) || EndTimeSeconds <= StartTimeSeconds)
            throw new ArgumentOutOfRangeException(nameof(EndTimeSeconds), "End time must be finite and later than start time.");
        if (!double.IsFinite(SampleIntervalSeconds) || SampleIntervalSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(SampleIntervalSeconds), "Sample interval must be finite and positive.");
        if (MaximumSamples <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumSamples), "Maximum sample count must be positive.");

        var ordered = Participants
            .OrderBy(participant => participant?.ParticipantId, StringComparer.Ordinal)
            .ToArray();

        for (var i = 0; i < ordered.Length; i++)
        {
            var participant = ordered[i] ??
                throw new ArgumentException("Participants cannot contain null entries.", nameof(Participants));
            participant.Validate();

            if (i > 0 && string.Equals(
                ordered[i - 1].ParticipantId,
                participant.ParticipantId,
                StringComparison.Ordinal))
            {
                throw new ArgumentException("Participant IDs must be unique using ordinal comparison.", nameof(Participants));
            }

            if (StartTimeSeconds < participant.TimingDefinition.StartTimeSeconds)
                throw new ArgumentOutOfRangeException(nameof(StartTimeSeconds), $"Observation starts before participant '{participant.ParticipantId}' exists.");
        }

        return Array.AsReadOnly(ordered);
    }
}

/// <summary>One participant's analytically evaluated phase at a population sample.</summary>
public readonly record struct OscillatorParticipantPhase(
    string ParticipantId,
    OscillatorModelKind ModelKind,
    double PhaseRadians,
    double WrappedPhaseRadians);

/// <summary>
/// One population observation. OrderParameterMagnitude is phase concentration in [0, 1]
/// (up to floating-point error); MeanPhaseRadians is the argument of the complex mean.
/// </summary>
public sealed record OscillatorPopulationSample(
    long SampleIndex,
    double TimeSeconds,
    IReadOnlyList<OscillatorParticipantPhase> Participants,
    double OrderParameterMagnitude,
    double MeanPhaseRadians);

/// <summary>Deterministic output of an ideal timing population experiment.</summary>
public sealed record OscillatorPopulationExperimentResult(
    IReadOnlyList<OscillatorPopulationSample> Samples,
    IReadOnlyList<string> OrderedParticipantIds);

/// <summary>
/// Evaluates an uncoupled population of ideal timing oscillators at deterministic sample times.
/// This is phase analysis only: it does not couple, synchronize, or modify participants.
/// </summary>
public static class OscillatorPopulationExperiment
{
    /// <summary>Runs a bounded, deterministic population observation.</summary>
    public static OscillatorPopulationExperimentResult Evaluate(
        OscillatorPopulationExperimentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var participants = definition.ValidateAndOrderParticipants();

        var samples = new List<OscillatorPopulationSample>();
        for (var sampleIndex = 0; ; sampleIndex++)
        {
            var time = definition.StartTimeSeconds + (sampleIndex * definition.SampleIntervalSeconds);
            if (!double.IsFinite(time))
                throw new OverflowException("A population sample time exceeds finite double range.");
            if (time >= definition.EndTimeSeconds)
                break;
            if (sampleIndex >= definition.MaximumSamples)
                throw new InvalidOperationException($"The experiment exceeds its configured sample budget of {definition.MaximumSamples}.");

            var phaseSamples = new OscillatorParticipantPhase[participants.Count];
            var cosineSum = 0d;
            var sineSum = 0d;

            for (var participantIndex = 0; participantIndex < participants.Count; participantIndex++)
            {
                var participant = participants[participantIndex];
                var timing = participant.TimingDefinition;
                var phase = timing.InitialPhaseRadians +
                    (timing.AngularFrequencyRadiansPerSecond * (time - timing.StartTimeSeconds));
                if (!double.IsFinite(phase))
                    throw new OverflowException($"Phase for participant '{participant.ParticipantId}' exceeds finite double range.");

                var wrappedPhase = WrapPositive(phase);
                phaseSamples[participantIndex] = new OscillatorParticipantPhase(
                    participant.ParticipantId,
                    participant.ModelKind,
                    phase,
                    wrappedPhase);
                cosineSum += Math.Cos(wrappedPhase);
                sineSum += Math.Sin(wrappedPhase);
            }

            var meanCosine = cosineSum / participants.Count;
            var meanSine = sineSum / participants.Count;
            var magnitude = Math.Sqrt((meanCosine * meanCosine) + (meanSine * meanSine));
            var meanPhase = magnitude <= 1e-15 ? 0d : Math.Atan2(meanSine, meanCosine);
            if (!double.IsFinite(magnitude) || !double.IsFinite(meanPhase))
                throw new OverflowException("The population order parameter is not finite.");

            samples.Add(new OscillatorPopulationSample(
                sampleIndex,
                time,
                Array.AsReadOnly(phaseSamples),
                Math.Clamp(magnitude, 0d, 1d),
                meanPhase));
        }

        return new OscillatorPopulationExperimentResult(
            Array.AsReadOnly(samples.ToArray()),
            Array.AsReadOnly(participants.Select(participant => participant.ParticipantId).ToArray()));
    }

    /// <summary>Wraps finite phase into [0, 2π).</summary>
    public static double WrapPositive(double phaseRadians)
    {
        if (!double.IsFinite(phaseRadians))
            throw new ArgumentOutOfRangeException(nameof(phaseRadians), "Phase must be finite.");

        var wrapped = phaseRadians % (2d * Math.PI);
        return wrapped < 0d ? wrapped + (2d * Math.PI) : wrapped;
    }
}
