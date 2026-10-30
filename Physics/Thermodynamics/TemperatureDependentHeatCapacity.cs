namespace TheSingularityWorkshop.Physics.Thermodynamics;

/// <summary>A tabulated specific-heat-capacity observation at an absolute temperature.</summary>
/// <param name="TemperatureKelvin">Temperature in kelvin.</param>
/// <param name="SpecificHeatCapacityJoulesPerKilogramKelvin">Specific heat capacity in J/(kg·K).</param>
public readonly record struct HeatCapacitySample(
    double TemperatureKelvin,
    double SpecificHeatCapacityJoulesPerKilogramKelvin);

/// <summary>
/// A bounded, piecewise-linear specific-heat-capacity curve. Sensible enthalpy is integrated
/// exactly over each linear segment, so energy transfer does not approximate the curve by
/// sampling heat capacity at the starting temperature.
/// </summary>
/// <remarks>
/// Samples are caller-supplied evidence, not an embedded material database. The caller must
/// retain the source and conditions represented by <see cref="EvidenceSource"/> and
/// <see cref="EvidenceConditions"/>. No extrapolation, phase transition, pressure correction,
/// hysteresis, or uncertainty propagation is performed.
/// </remarks>
public sealed class TemperatureDependentHeatCapacity
{
    private readonly HeatCapacitySample[] _samples;

    /// <summary>Stable material or reference-material identity.</summary>
    public string MaterialId { get; }
    /// <summary>Evidence classification for the supplied curve.</summary>
    public Materials.PropertyEvidenceStatus EvidenceStatus { get; }
    /// <summary>Source identifier or stable illustrative-fixture identifier.</summary>
    public string EvidenceSource { get; }
    /// <summary>Source revision or fixture version.</summary>
    public string EvidenceVersion { get; }
    /// <summary>Conditions and limitations attached to the supplied curve.</summary>
    public string EvidenceConditions { get; }
    /// <summary>Inclusive lower temperature bound in kelvin.</summary>
    public double MinimumTemperatureKelvin => _samples[0].TemperatureKelvin;
    /// <summary>Inclusive upper temperature bound in kelvin.</summary>
    public double MaximumTemperatureKelvin => _samples[^1].TemperatureKelvin;
    /// <summary>Read-only copy of the curve's ordered samples.</summary>
    public IReadOnlyList<HeatCapacitySample> Samples => Array.AsReadOnly((HeatCapacitySample[])_samples.Clone());

    /// <summary>Creates a validated curve from at least two strictly ordered positive-capacity samples.</summary>
    public TemperatureDependentHeatCapacity(
        string materialId,
        IReadOnlyList<HeatCapacitySample> samples,
        Materials.PropertyEvidenceStatus evidenceStatus,
        string evidenceSource,
        string evidenceVersion,
        string evidenceConditions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(materialId);
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceSource);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceConditions);
        if (!Enum.IsDefined(evidenceStatus))
            throw new ArgumentOutOfRangeException(nameof(evidenceStatus));
        if (samples.Count < 2)
            throw new ArgumentException("At least two heat-capacity samples are required.", nameof(samples));

        _samples = new HeatCapacitySample[samples.Count];
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            if (!double.IsFinite(sample.TemperatureKelvin) || sample.TemperatureKelvin < 0d ||
                !double.IsFinite(sample.SpecificHeatCapacityJoulesPerKilogramKelvin) ||
                sample.SpecificHeatCapacityJoulesPerKilogramKelvin <= 0d)
                throw new ArgumentException("Every sample must have a finite non-negative temperature and finite positive specific heat capacity.", nameof(samples));
            if (i > 0 && sample.TemperatureKelvin <= samples[i - 1].TemperatureKelvin)
                throw new ArgumentException("Sample temperatures must be strictly increasing.", nameof(samples));
            _samples[i] = sample;
        }

        MaterialId = materialId;
        EvidenceStatus = evidenceStatus;
        EvidenceSource = evidenceSource;
        EvidenceVersion = evidenceVersion;
        EvidenceConditions = evidenceConditions;
    }

    /// <summary>Evaluates specific heat capacity by linear interpolation within the declared range.</summary>
    public double GetSpecificHeatCapacity(double temperatureKelvin)
    {
        EnsureTemperatureInRange(temperatureKelvin);
        var segment = FindSegment(temperatureKelvin);
        var left = _samples[segment];
        var right = _samples[segment + 1];
        var fraction = (temperatureKelvin - left.TemperatureKelvin) /
            (right.TemperatureKelvin - left.TemperatureKelvin);
        return left.SpecificHeatCapacityJoulesPerKilogramKelvin +
            fraction * (right.SpecificHeatCapacityJoulesPerKilogramKelvin -
                left.SpecificHeatCapacityJoulesPerKilogramKelvin);
    }

    /// <summary>
    /// Returns the signed specific enthalpy change from reference temperature to target temperature,
    /// in J/kg. Each piecewise-linear segment is integrated analytically using the trapezoid rule.
    /// </summary>
    public double GetSpecificEnthalpyChangeJoulesPerKilogram(
        double referenceTemperatureKelvin,
        double targetTemperatureKelvin)
    {
        EnsureTemperatureInRange(referenceTemperatureKelvin);
        EnsureTemperatureInRange(targetTemperatureKelvin);
        if (referenceTemperatureKelvin == targetTemperatureKelvin)
            return 0d;
        if (targetTemperatureKelvin < referenceTemperatureKelvin)
            return -IntegrateAscending(targetTemperatureKelvin, referenceTemperatureKelvin);
        return IntegrateAscending(referenceTemperatureKelvin, targetTemperatureKelvin);
    }

    /// <summary>
    /// Applies energy to a lumped body and solves for its new temperature using the integrated
    /// enthalpy curve. The result must remain inside the declared temperature range.
    /// </summary>
    public double ApplyNetEnergyTransfer(
        double massKilograms,
        double initialTemperatureKelvin,
        double netEnergyTransferJoules)
    {
        if (!double.IsFinite(massKilograms) || massKilograms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(massKilograms), "Mass must be finite and positive.");
        EnsureTemperatureInRange(initialTemperatureKelvin);
        if (!double.IsFinite(netEnergyTransferJoules))
            throw new ArgumentOutOfRangeException(nameof(netEnergyTransferJoules), "Energy transfer must be finite.");
        if (netEnergyTransferJoules == 0d)
            return initialTemperatureKelvin;

        var specificEnergy = netEnergyTransferJoules / massKilograms;
        if (!double.IsFinite(specificEnergy))
            throw new OverflowException("Specific energy transfer is outside finite double range.");
        var minimumEnergy = GetSpecificEnthalpyChangeJoulesPerKilogram(
            initialTemperatureKelvin, MinimumTemperatureKelvin);
        var maximumEnergy = GetSpecificEnthalpyChangeJoulesPerKilogram(
            initialTemperatureKelvin, MaximumTemperatureKelvin);
        if (!double.IsFinite(minimumEnergy) || !double.IsFinite(maximumEnergy) ||
            specificEnergy < minimumEnergy || specificEnergy > maximumEnergy)
            throw new ArgumentOutOfRangeException(nameof(netEnergyTransferJoules),
                "Energy transfer would move the body outside the heat-capacity curve's declared temperature range.");

        var low = MinimumTemperatureKelvin;
        var high = MaximumTemperatureKelvin;
        // Fixed iteration count makes inversion deterministic and bounds work independently of input.
        for (var i = 0; i < 80; i++)
        {
            var middle = low + (high - low) * 0.5d;
            var energyAtMiddle = GetSpecificEnthalpyChangeJoulesPerKilogram(initialTemperatureKelvin, middle);
            if (energyAtMiddle < specificEnergy)
                low = middle;
            else
                high = middle;
        }

        return low + (high - low) * 0.5d;
    }

    private double IntegrateAscending(double startKelvin, double endKelvin)
    {
        var total = 0d;
        var cursor = startKelvin;
        while (cursor < endKelvin)
        {
            var segment = FindSegment(cursor);
            var segmentEnd = Math.Min(endKelvin, _samples[segment + 1].TemperatureKelvin);
            var cpStart = GetSpecificHeatCapacity(cursor);
            var cpEnd = GetSpecificHeatCapacity(segmentEnd);
            var area = (cpStart * 0.5d + cpEnd * 0.5d) * (segmentEnd - cursor);
            total += area;
            if (!double.IsFinite(total))
                throw new OverflowException("Integrated specific enthalpy is outside finite double range.");
            cursor = segmentEnd;
        }
        return total;
    }

    private int FindSegment(double temperatureKelvin)
    {
        var low = 0;
        var high = _samples.Length - 2;
        while (low < high)
        {
            var middle = low + (high - low + 1) / 2;
            if (_samples[middle].TemperatureKelvin <= temperatureKelvin)
                low = middle;
            else
                high = middle - 1;
        }
        return low;
    }

    private void EnsureTemperatureInRange(double temperatureKelvin)
    {
        if (!double.IsFinite(temperatureKelvin) ||
            temperatureKelvin < MinimumTemperatureKelvin ||
            temperatureKelvin > MaximumTemperatureKelvin)
            throw new ArgumentOutOfRangeException(nameof(temperatureKelvin),
                "Temperature must be finite and within the inclusive declared curve range.");
    }
}
