namespace TheSingularityWorkshop.Physics.Thermodynamics;

/// <summary>
/// Equilibrium solid/liquid state for one material undergoing a single, fixed-temperature
/// phase transition. Energy is measured relative to the solid at the transition temperature.
/// </summary>
/// <remarks>
/// This model uses constant solid/liquid specific heat capacities and one specific latent heat.
/// During a mixed-phase transition, temperature remains at the declared transition temperature
/// while liquid fraction changes. It does not model pressure-dependent phase boundaries,
/// hysteresis, nucleation, superheating, spatial gradients, or more than one transition.
/// </remarks>
public readonly record struct PhaseChangeThermalState(
    double MassKilograms,
    double TemperatureKelvin,
    double SolidSpecificHeatCapacityJoulesPerKilogramKelvin,
    double LiquidSpecificHeatCapacityJoulesPerKilogramKelvin,
    double TransitionTemperatureKelvin,
    double SpecificLatentHeatJoulesPerKilogram,
    double LiquidFraction)
{
    /// <summary>Whether the state is finite, non-negative in absolute temperature, and phase-consistent.</summary>
    public bool IsValid
    {
        get
        {
            if (!double.IsFinite(MassKilograms) || MassKilograms <= 0d ||
                !double.IsFinite(TemperatureKelvin) || TemperatureKelvin < 0d ||
                !double.IsFinite(SolidSpecificHeatCapacityJoulesPerKilogramKelvin) ||
                SolidSpecificHeatCapacityJoulesPerKilogramKelvin <= 0d ||
                !double.IsFinite(LiquidSpecificHeatCapacityJoulesPerKilogramKelvin) ||
                LiquidSpecificHeatCapacityJoulesPerKilogramKelvin <= 0d ||
                !double.IsFinite(TransitionTemperatureKelvin) || TransitionTemperatureKelvin < 0d ||
                !double.IsFinite(SpecificLatentHeatJoulesPerKilogram) ||
                SpecificLatentHeatJoulesPerKilogram <= 0d ||
                !double.IsFinite(LiquidFraction) || LiquidFraction < 0d || LiquidFraction > 1d)
                return false;

            if (!double.IsFinite(MassKilograms * SolidSpecificHeatCapacityJoulesPerKilogramKelvin) ||
                !double.IsFinite(MassKilograms * LiquidSpecificHeatCapacityJoulesPerKilogramKelvin) ||
                !double.IsFinite(MassKilograms * SpecificLatentHeatJoulesPerKilogram) ||
                !double.IsFinite(EnthalpyRelativeToTransitionJoules))
                return false;

            return TemperatureKelvin < TransitionTemperatureKelvin
                ? LiquidFraction == 0d
                : TemperatureKelvin > TransitionTemperatureKelvin
                    ? LiquidFraction == 1d
                    : true;
        }
    }

    /// <summary>
    /// Sensible-plus-latent energy relative to the all-solid state at transition temperature, in joules.
    /// Negative values are solid sensible energy; mixed-phase values are latent energy; values above
    /// the latent plateau include liquid sensible energy.
    /// </summary>
    public double EnthalpyRelativeToTransitionJoules
    {
        get
        {
            var mass = MassKilograms;
            if (TemperatureKelvin < TransitionTemperatureKelvin)
                return mass * SolidSpecificHeatCapacityJoulesPerKilogramKelvin
                    * (TemperatureKelvin - TransitionTemperatureKelvin);
            if (TemperatureKelvin > TransitionTemperatureKelvin)
                return mass * (SpecificLatentHeatJoulesPerKilogram
                    + LiquidSpecificHeatCapacityJoulesPerKilogramKelvin
                    * (TemperatureKelvin - TransitionTemperatureKelvin));

            return mass * SpecificLatentHeatJoulesPerKilogram * LiquidFraction;
        }
    }

    /// <summary>
    /// Applies net energy and returns the equilibrium state. Positive energy heats/melts; negative
    /// energy cools/freezes. Energy is conserved under the declared constant-property model.
    /// </summary>
    /// <param name="netEnergyTransferJoules">Finite net energy entering the body, in joules.</param>
    /// <exception cref="ArgumentException">The state or transfer is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The resulting state would fall below absolute zero.</exception>
    /// <exception cref="OverflowException">An intermediate or resulting quantity is non-finite.</exception>
    public PhaseChangeThermalState ApplyNetEnergyTransfer(double netEnergyTransferJoules)
    {
        if (!IsValid)
            throw new ArgumentException("The phase-change thermal state must be valid.", nameof(PhaseChangeThermalState));
        if (!double.IsFinite(netEnergyTransferJoules))
            throw new ArgumentException("The energy transfer must be finite.", nameof(netEnergyTransferJoules));

        var targetEnthalpy = EnthalpyRelativeToTransitionJoules + netEnergyTransferJoules;
        var latentEnergy = MassKilograms * SpecificLatentHeatJoulesPerKilogram;
        var solidHeatCapacity = MassKilograms * SolidSpecificHeatCapacityJoulesPerKilogramKelvin;
        var liquidHeatCapacity = MassKilograms * LiquidSpecificHeatCapacityJoulesPerKilogramKelvin;
        if (!double.IsFinite(targetEnthalpy) || !double.IsFinite(latentEnergy) ||
            !double.IsFinite(solidHeatCapacity) || !double.IsFinite(liquidHeatCapacity))
            throw new OverflowException("Phase-change energy is outside finite double range.");

        double temperature;
        double liquidFraction;
        if (targetEnthalpy < 0d)
        {
            temperature = TransitionTemperatureKelvin + targetEnthalpy / solidHeatCapacity;
            liquidFraction = 0d;
            if (temperature < 0d)
                throw new ArgumentOutOfRangeException(nameof(netEnergyTransferJoules),
                    "The energy transfer would lower the body below absolute zero.");
        }
        else if (targetEnthalpy <= latentEnergy)
        {
            temperature = TransitionTemperatureKelvin;
            liquidFraction = targetEnthalpy / latentEnergy;
        }
        else
        {
            temperature = TransitionTemperatureKelvin + (targetEnthalpy - latentEnergy) / liquidHeatCapacity;
            liquidFraction = 1d;
        }

        if (!double.IsFinite(temperature) || !double.IsFinite(liquidFraction))
            throw new OverflowException("The resulting phase-change state is outside finite double range.");

        var result = this with { TemperatureKelvin = temperature, LiquidFraction = liquidFraction };
        if (!result.IsValid)
            throw new OverflowException("The resulting phase-change state is not finite or phase-consistent.");
        return result;
    }
}
