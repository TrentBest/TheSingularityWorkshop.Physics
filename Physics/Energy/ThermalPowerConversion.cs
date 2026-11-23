namespace TheSingularityWorkshop.Physics.Energy;

/// <summary>
/// Observes a bounded thermal-to-shaft-to-electrical conversion chain using a Carnot upper bound.
/// </summary>
/// <remarks>
/// This is an energy ledger, not a heat-engine cycle solver. The caller supplies heat input,
/// hot- and cold-reservoir temperatures, the declared fraction of the Carnot limit achieved by
/// the thermal cycle, and generator efficiency. The model reports rejected thermal energy,
/// shaft work, electrical output, and energy not delivered as electrical output. It does not
/// infer that generator losses are all heat, nor does it model working-fluid states, pressure,
/// flow, turbine maps, generator dynamics, transients, or a particular facility.
/// </remarks>
public static class ThermalPowerConversion
{
    /// <summary>
    /// Calculates one thermal-to-shaft-to-electrical energy conversion observation.
    /// </summary>
    /// <param name="heatInputJoules">Thermal energy supplied to the heat-engine boundary, in joules.</param>
    /// <param name="hotReservoirTemperatureKelvin">Absolute hot-reservoir temperature, in kelvin.</param>
    /// <param name="coldReservoirTemperatureKelvin">Absolute cold-reservoir temperature, in kelvin.</param>
    /// <param name="carnotLimitFraction">Fraction of the Carnot efficiency achieved by the declared thermal cycle, in [0, 1].</param>
    /// <param name="generatorEfficiencyFraction">Fraction of shaft work delivered as electrical energy, in [0, 1].</param>
    /// <returns>Energy accounting across the heat-engine and generator boundaries.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An input is outside its valid physical or declared range.</exception>
    public static ThermalPowerConversionObservation Observe(
        double heatInputJoules,
        double hotReservoirTemperatureKelvin,
        double coldReservoirTemperatureKelvin,
        double carnotLimitFraction,
        double generatorEfficiencyFraction)
    {
        if (!double.IsFinite(heatInputJoules) || heatInputJoules < 0d)
            throw new ArgumentOutOfRangeException(nameof(heatInputJoules), "Heat input must be finite and non-negative.");
        if (!double.IsFinite(hotReservoirTemperatureKelvin) || hotReservoirTemperatureKelvin <= 0d)
            throw new ArgumentOutOfRangeException(nameof(hotReservoirTemperatureKelvin), "Hot-reservoir temperature must be finite and above absolute zero.");
        if (!double.IsFinite(coldReservoirTemperatureKelvin) || coldReservoirTemperatureKelvin < 0d
            || coldReservoirTemperatureKelvin >= hotReservoirTemperatureKelvin)
            throw new ArgumentOutOfRangeException(nameof(coldReservoirTemperatureKelvin), "Cold-reservoir temperature must be finite, non-negative, and below hot-reservoir temperature.");
        if (!double.IsFinite(carnotLimitFraction) || carnotLimitFraction < 0d || carnotLimitFraction > 1d)
            throw new ArgumentOutOfRangeException(nameof(carnotLimitFraction), "The Carnot-limit fraction must be finite and within [0, 1].");
        if (!double.IsFinite(generatorEfficiencyFraction) || generatorEfficiencyFraction < 0d || generatorEfficiencyFraction > 1d)
            throw new ArgumentOutOfRangeException(nameof(generatorEfficiencyFraction), "Generator efficiency must be finite and within [0, 1].");

        var carnotEfficiency = 1d - coldReservoirTemperatureKelvin / hotReservoirTemperatureKelvin;
        var thermalCycleEfficiency = carnotEfficiency * carnotLimitFraction;
        var shaftWork = heatInputJoules * thermalCycleEfficiency;
        var rejectedThermalEnergy = heatInputJoules - shaftWork;
        var electricalEnergy = shaftWork * generatorEfficiencyFraction;
        var energyNotDeliveredAsElectricalOutput = shaftWork - electricalEnergy;

        return new ThermalPowerConversionObservation(
            heatInputJoules,
            hotReservoirTemperatureKelvin,
            coldReservoirTemperatureKelvin,
            carnotEfficiency,
            carnotLimitFraction,
            thermalCycleEfficiency,
            rejectedThermalEnergy,
            shaftWork,
            generatorEfficiencyFraction,
            electricalEnergy,
            energyNotDeliveredAsElectricalOutput);
    }
}

/// <summary>Immutable energy ledger for one bounded thermal-to-electrical conversion observation.</summary>
/// <param name="HeatInputJoules">Thermal energy entering the heat-engine boundary, in joules.</param>
/// <param name="HotReservoirTemperatureKelvin">Hot-reservoir temperature, in kelvin.</param>
/// <param name="ColdReservoirTemperatureKelvin">Cold-reservoir temperature, in kelvin.</param>
/// <param name="CarnotEfficiencyUpperBound">Carnot upper bound for the declared reservoir temperatures.</param>
/// <param name="CarnotLimitFraction">Declared fraction of the Carnot upper bound achieved by the cycle.</param>
/// <param name="ThermalCycleEfficiencyFraction">Heat-to-shaft efficiency implied by the two preceding values.</param>
/// <param name="RejectedThermalEnergyJoules">Heat input not converted to shaft work, in joules.</param>
/// <param name="ShaftWorkJoules">Mechanical work delivered to the generator shaft, in joules.</param>
/// <param name="GeneratorEfficiencyFraction">Declared fraction of shaft work delivered as electrical energy.</param>
/// <param name="ElectricalEnergyJoules">Electrical energy delivered, in joules.</param>
/// <param name="EnergyNotDeliveredAsElectricalOutputJoules">Shaft energy not delivered as electrical output; its destination is not inferred.</param>
public sealed record ThermalPowerConversionObservation(
    double HeatInputJoules,
    double HotReservoirTemperatureKelvin,
    double ColdReservoirTemperatureKelvin,
    double CarnotEfficiencyUpperBound,
    double CarnotLimitFraction,
    double ThermalCycleEfficiencyFraction,
    double RejectedThermalEnergyJoules,
    double ShaftWorkJoules,
    double GeneratorEfficiencyFraction,
    double ElectricalEnergyJoules,
    double EnergyNotDeliveredAsElectricalOutputJoules);
