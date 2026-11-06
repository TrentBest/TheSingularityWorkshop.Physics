namespace TheSingularityWorkshop.Physics.Timing;

/// <summary>
/// Analytically compares two constant-frequency ideal timing oscillators over a
/// finite observation interval. This is a phase measurement, not a coupling model.
/// </summary>
public static class OscillatorPhaseComparison
{
    /// <summary>
    /// Compares unwrapped and wrapped relative phase at both interval endpoints.
    /// Wrapped phase difference uses the convention [-π, π).
    /// </summary>
    public static OscillatorPhaseComparisonResult Compare(
        IdealMetronomeDefinition first,
        IdealMetronomeDefinition second,
        double startTimeSeconds,
        double endTimeSeconds)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        first.Validate();
        second.Validate();

        if (!double.IsFinite(startTimeSeconds) ||
            startTimeSeconds < first.StartTimeSeconds ||
            startTimeSeconds < second.StartTimeSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startTimeSeconds),
                "Observation start must be finite and not precede either oscillator start.");
        }

        if (!double.IsFinite(endTimeSeconds) || endTimeSeconds <= startTimeSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endTimeSeconds),
                "Observation end must be finite and later than observation start.");
        }

        var firstStart = GetUnwrappedPhase(first, startTimeSeconds);
        var secondStart = GetUnwrappedPhase(second, startTimeSeconds);
        var firstEnd = GetUnwrappedPhase(first, endTimeSeconds);
        var secondEnd = GetUnwrappedPhase(second, endTimeSeconds);

        var unwrappedDifferenceStart = secondStart - firstStart;
        var unwrappedDifferenceEnd = secondEnd - firstEnd;
        var duration = endTimeSeconds - startTimeSeconds;
        var frequencyDifferenceHz =
            (second.TempoBeatsPerMinute - first.TempoBeatsPerMinute) / 60d;
        var phaseDriftRate = (unwrappedDifferenceEnd - unwrappedDifferenceStart) / duration;

        if (!double.IsFinite(unwrappedDifferenceStart) ||
            !double.IsFinite(unwrappedDifferenceEnd) ||
            !double.IsFinite(frequencyDifferenceHz) ||
            !double.IsFinite(phaseDriftRate))
        {
            throw new OverflowException("The phase comparison exceeds finite double range.");
        }

        return new OscillatorPhaseComparisonResult(
            startTimeSeconds,
            endTimeSeconds,
            firstStart,
            secondStart,
            firstEnd,
            secondEnd,
            unwrappedDifferenceStart,
            unwrappedDifferenceEnd,
            WrapSigned(unwrappedDifferenceStart),
            WrapSigned(unwrappedDifferenceEnd),
            frequencyDifferenceHz,
            phaseDriftRate);
    }

    /// <summary>Wraps a finite phase difference into [-π, π).</summary>
    public static double WrapSigned(double phaseRadians)
    {
        if (!double.IsFinite(phaseRadians))
        {
            throw new ArgumentOutOfRangeException(
                nameof(phaseRadians),
                "Phase must be finite.");
        }

        var wrapped = phaseRadians % (2d * Math.PI);
        if (wrapped < -Math.PI)
        {
            wrapped += 2d * Math.PI;
        }
        else if (wrapped >= Math.PI)
        {
            wrapped -= 2d * Math.PI;
        }
        else
        {
            // Modulo reduction of a large odd half-turn can land a few ULPs
            // below +π. Preserve the documented [-π, π) boundary convention.
            var boundaryTolerance = Math.Min(
                1e-12,
                16d * 2.2204460492503131e-16 * Math.Max(1d, Math.Abs(phaseRadians)));
            if (Math.PI - wrapped <= boundaryTolerance)
            {
                wrapped = -Math.PI;
            }
        }

        return wrapped;
    }

    private static double GetUnwrappedPhase(
        IdealMetronomeDefinition definition,
        double timeSeconds)
    {
        var phase = definition.InitialPhaseRadians +
            (definition.AngularFrequencyRadiansPerSecond *
             (timeSeconds - definition.StartTimeSeconds));

        if (!double.IsFinite(phase))
        {
            throw new OverflowException("The oscillator phase exceeds finite double range.");
        }

        return phase;
    }
}

/// <summary>
/// Endpoint measurements and constant-rate drift for two ideal oscillators.
/// Positive phase difference means the second oscillator is ahead of the first.
/// </summary>
public readonly record struct OscillatorPhaseComparisonResult(
    double StartTimeSeconds,
    double EndTimeSeconds,
    double FirstPhaseStartRadians,
    double SecondPhaseStartRadians,
    double FirstPhaseEndRadians,
    double SecondPhaseEndRadians,
    double UnwrappedDifferenceStartRadians,
    double UnwrappedDifferenceEndRadians,
    double WrappedDifferenceStartRadians,
    double WrappedDifferenceEndRadians,
    double FrequencyDifferenceHz,
    double PhaseDriftRateRadiansPerSecond);
