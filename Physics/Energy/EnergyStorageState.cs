namespace TheSingularityWorkshop.Physics.Energy;

/// <summary>
/// Immutable energy-storage state with explicit capacity, charge/discharge power limits, and efficiencies.
/// </summary>
/// <remarks>
/// This is a generic bounded energy reservoir, not an electrochemical battery model. It accounts
/// for energy crossing the storage boundary and energy added to or removed from the stored inventory.
/// Energy not stored or not delivered is not automatically heat; a technology-specific model must
/// describe its destination. Temperature, voltage, chemistry, state-of-health, degradation, and
/// transient electrical behavior are outside this contract.
/// </remarks>
public readonly record struct EnergyStorageState(
    double StoredEnergyJoules,
    double CapacityJoules,
    double MaximumChargePowerWatts,
    double MaximumDischargePowerWatts,
    double ChargeEfficiencyFraction,
    double DischargeEfficiencyFraction)
{
    /// <summary>Gets the fraction of declared storage capacity currently occupied.</summary>
    public double StateOfChargeFraction => IsValid ? StoredEnergyJoules / CapacityJoules : double.NaN;

    /// <summary>Returns true when stored energy, capacity, power limits, and efficiencies are finite and admissible.</summary>
    public bool IsValid =>
        double.IsFinite(StoredEnergyJoules) && StoredEnergyJoules >= 0d &&
        double.IsFinite(CapacityJoules) && CapacityJoules > 0d &&
        StoredEnergyJoules <= CapacityJoules &&
        double.IsFinite(MaximumChargePowerWatts) && MaximumChargePowerWatts > 0d &&
        double.IsFinite(MaximumDischargePowerWatts) && MaximumDischargePowerWatts > 0d &&
        double.IsFinite(ChargeEfficiencyFraction) && ChargeEfficiencyFraction > 0d && ChargeEfficiencyFraction <= 1d &&
        double.IsFinite(DischargeEfficiencyFraction) && DischargeEfficiencyFraction > 0d && DischargeEfficiencyFraction <= 1d;

    /// <summary>
    /// Applies a constant requested charging power over an explicit interval, bounded by charge power
    /// and remaining capacity. The returned state is new; this value is unchanged.
    /// </summary>
    public EnergyStorageChargeResult Charge(double requestedInputPowerWatts, double durationSeconds)
    {
        EnsureValid();
        ValidateTransfer(requestedInputPowerWatts, durationSeconds, nameof(requestedInputPowerWatts));

        var requestedInputEnergy = requestedInputPowerWatts * durationSeconds;
        if (!double.IsFinite(requestedInputEnergy))
            throw new OverflowException("Requested charging energy is outside finite double range.");

        var powerLimitedInputEnergy = requestedInputPowerWatts <= MaximumChargePowerWatts
            ? requestedInputEnergy
            : MaximumChargePowerWatts * durationSeconds;
        var remainingCapacity = CapacityJoules - StoredEnergyJoules;
        var capacityLimitedInputEnergy = remainingCapacity / ChargeEfficiencyFraction;
        var acceptedInputEnergy = Math.Min(powerLimitedInputEnergy, capacityLimitedInputEnergy);
        var storedEnergyIncrease = acceptedInputEnergy * ChargeEfficiencyFraction;
        var energyNotAccepted = requestedInputEnergy - acceptedInputEnergy;

        if (!double.IsFinite(powerLimitedInputEnergy) ||
            double.IsNaN(capacityLimitedInputEnergy) ||
            double.IsNegativeInfinity(capacityLimitedInputEnergy) ||
            !double.IsFinite(acceptedInputEnergy) ||
            !double.IsFinite(storedEnergyIncrease) ||
            !double.IsFinite(energyNotAccepted))
            throw new OverflowException("Charging energy accounting is outside finite double range.");

        var nextStoredEnergy = Math.Min(CapacityJoules, StoredEnergyJoules + storedEnergyIncrease);
        var actualStoredEnergyIncrease = nextStoredEnergy - StoredEnergyJoules;
        var nextState = this with { StoredEnergyJoules = nextStoredEnergy };
        return new EnergyStorageChargeResult(
            this,
            nextState,
            requestedInputPowerWatts,
            durationSeconds,
            requestedInputEnergy,
            acceptedInputEnergy,
            actualStoredEnergyIncrease,
            acceptedInputEnergy - actualStoredEnergyIncrease,
            energyNotAccepted);
    }

    /// <summary>
    /// Applies a constant requested delivered-output power over an explicit interval, bounded by
    /// discharge power and available stored energy. The returned state is new; this value is unchanged.
    /// </summary>
    public EnergyStorageDischargeResult Discharge(double requestedOutputPowerWatts, double durationSeconds)
    {
        EnsureValid();
        ValidateTransfer(requestedOutputPowerWatts, durationSeconds, nameof(requestedOutputPowerWatts));

        var requestedOutputEnergy = requestedOutputPowerWatts * durationSeconds;
        if (!double.IsFinite(requestedOutputEnergy))
            throw new OverflowException("Requested discharge energy is outside finite double range.");

        var powerLimitedOutputEnergy = requestedOutputPowerWatts <= MaximumDischargePowerWatts
            ? requestedOutputEnergy
            : MaximumDischargePowerWatts * durationSeconds;
        var availableOutputEnergy = StoredEnergyJoules * DischargeEfficiencyFraction;
        var deliveredOutputEnergy = Math.Min(powerLimitedOutputEnergy, availableOutputEnergy);
        var storedEnergyRemoved = Math.Min(StoredEnergyJoules, deliveredOutputEnergy / DischargeEfficiencyFraction);
        var outputEnergyNotDelivered = requestedOutputEnergy - deliveredOutputEnergy;

        if (!double.IsFinite(powerLimitedOutputEnergy) ||
            !double.IsFinite(availableOutputEnergy) ||
            !double.IsFinite(deliveredOutputEnergy) ||
            !double.IsFinite(storedEnergyRemoved) ||
            !double.IsFinite(outputEnergyNotDelivered) ||
            !double.IsFinite(storedEnergyNotDeliveredAsOutput))
            throw new OverflowException("Discharging energy accounting is outside finite double range.");

        var nextStoredEnergy = Math.Max(0d, StoredEnergyJoules - storedEnergyRemoved);
        var actualStoredEnergyRemoved = StoredEnergyJoules - nextStoredEnergy;
        var actualStoredEnergyNotDeliveredAsOutput = Math.Max(0d, actualStoredEnergyRemoved - deliveredOutputEnergy);
        var nextState = this with { StoredEnergyJoules = nextStoredEnergy };
        return new EnergyStorageDischargeResult(
            this,
            nextState,
            requestedOutputPowerWatts,
            durationSeconds,
            requestedOutputEnergy,
            deliveredOutputEnergy,
            actualStoredEnergyRemoved,
            outputEnergyNotDelivered,
            actualStoredEnergyNotDeliveredAsOutput);
    }

    private void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("The energy-storage state must be valid.", nameof(EnergyStorageState));
    }

    private static void ValidateTransfer(double powerWatts, double durationSeconds, string powerParameterName)
    {
        if (!double.IsFinite(powerWatts) || powerWatts < 0d)
            throw new ArgumentOutOfRangeException(powerParameterName, "Requested power must be finite and non-negative.");
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be finite and positive.");
    }
}

/// <summary>Immutable result of one bounded energy-storage charge operation.</summary>
/// <param name="InitialState">Storage state before charging.</param>
/// <param name="FinalState">Storage state after charging.</param>
/// <param name="RequestedInputPowerWatts">Requested input power, in watts.</param>
/// <param name="DurationSeconds">Explicit charging interval, in seconds.</param>
/// <param name="RequestedInputEnergyJoules">Energy requested at the input boundary, in joules.</param>
/// <param name="AcceptedInputEnergyJoules">Input energy accepted within the power and capacity limits, in joules.</param>
/// <param name="StoredEnergyIncreaseJoules">Energy added to the stored inventory, in joules.</param>
/// <param name="InputEnergyNotStoredJoules">Accepted input energy not retained in the stored inventory; destination is not inferred.</param>
/// <param name="InputEnergyNotAcceptedJoules">Requested input energy not accepted by the storage boundary due to power or capacity limits.</param>
public sealed record EnergyStorageChargeResult(
    EnergyStorageState InitialState,
    EnergyStorageState FinalState,
    double RequestedInputPowerWatts,
    double DurationSeconds,
    double RequestedInputEnergyJoules,
    double AcceptedInputEnergyJoules,
    double StoredEnergyIncreaseJoules,
    double InputEnergyNotStoredJoules,
    double InputEnergyNotAcceptedJoules);

/// <summary>Immutable result of one bounded energy-storage discharge operation.</summary>
/// <param name="InitialState">Storage state before discharging.</param>
/// <param name="FinalState">Storage state after discharging.</param>
/// <param name="RequestedOutputPowerWatts">Requested delivered-output power, in watts.</param>
/// <param name="DurationSeconds">Explicit discharging interval, in seconds.</param>
/// <param name="RequestedOutputEnergyJoules">Requested energy at the output boundary, in joules.</param>
/// <param name="DeliveredOutputEnergyJoules">Energy delivered within the power and stored-energy limits, in joules.</param>
/// <param name="StoredEnergyRemovedJoules">Energy removed from the stored inventory, in joules.</param>
/// <param name="OutputEnergyNotDeliveredJoules">Requested output energy not delivered because of power or inventory limits.</param>
/// <param name="StoredEnergyNotDeliveredAsOutputJoules">Stored energy removed but not delivered as output; destination is not inferred.</param>
public sealed record EnergyStorageDischargeResult(
    EnergyStorageState InitialState,
    EnergyStorageState FinalState,
    double RequestedOutputPowerWatts,
    double DurationSeconds,
    double RequestedOutputEnergyJoules,
    double DeliveredOutputEnergyJoules,
    double StoredEnergyRemovedJoules,
    double OutputEnergyNotDeliveredJoules,
    double StoredEnergyNotDeliveredAsOutputJoules);
