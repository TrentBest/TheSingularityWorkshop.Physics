# Thermodynamics: Equilibrium Phase Change

**Owner:** The Singularity Workshop · **Status:** bounded executable primitive, not a general phase-equilibrium solver.

This document describes the solid/liquid phase-change reference model in [`PhaseChangeThermalState`](../Physics/Thermodynamics/PhaseChangeThermalState.cs) and its focused tests in [`PhaseChangeThermalStateTests`](../tests/TheSingularityWorkshop.Physics.Tests/Thermodynamics/PhaseChangeThermalStateTests.cs).

## What the model computes

The state describes a fixed mass of one material with:
- a solid specific heat capacity (c_s), in J/(kg·K);
- a liquid specific heat capacity (c_l), in J/(kg·K);
- one declared transition temperature (T_m), in K;
- one specific latent heat (L), in J/kg;
- current temperature (T), in K, and liquid mass fraction (f_l), from 0 to 1.

All values are caller-supplied model parameters. The code does not look up or certify material constants. Mark test/example values as illustrative; use sourced values with their applicability conditions for physical experiments.

Energy is measured relative to the all-solid state at (T_m). For mass (m):

[
H(T,f_l)=
\begin{cases}
m c_s(T-T_m), & T<T_m,\
m L f_l, & T=T_m,\
m\left(L+c_l(T-T_m)\right), & T>T_m.
\end{cases}
]

The model inverts this enthalpy relation after a finite net energy transfer (Q), using (H_{next}=H_{current}+Q). Below the transition, energy changes solid temperature. Across the latent-heat plateau, temperature remains (T_m) while liquid fraction changes. After melting completes, energy changes liquid temperature. Negative energy follows the same relation in reverse, so freezing consumes released latent heat before the solid cools.

## Invariants and limits

- Mass, both heat capacities, and latent heat must be finite and positive.
- Absolute temperature and transition temperature must be finite and non-negative.
- Liquid fraction is in ([0,1]); below (T_m), it must be 0, and above (T_m), it must be 1.
- Non-finite transfers and overflow are rejected; a transfer that would cross below absolute zero is rejected.
- The model uses equilibrium at one fixed transition temperature and constant heat capacities. It does **not** model pressure-dependent phase boundaries, multiple solid phases, evaporation/condensation, nucleation, superheating, hysteresis, temperature-dependent properties, mixtures, spatial gradients, or phase-front motion.
- The model is not a water/ice solver by itself. A material profile must provide appropriate evidence and conditions before the result can be interpreted as a prediction for a real substance.

## Verification intent

Focused tests cover sensible heating, melting at constant temperature, liquid heating, freezing, solid cooling, energy round-trip consistency, invalid phase combinations, non-finite transfers, and the absolute-zero boundary. These tests establish the implemented accounting contract; they do not validate a real material dataset or prove general thermodynamic accuracy.

## Relationship to existing thermal primitives

[`LumpedThermalState`](../Physics/Thermodynamics/LumpedThermalState.cs) remains the simpler constant-specific-heat model when no phase transition is required. [`ThermalConduction`](../Physics/Thermodynamics/ThermalConduction.cs) moves energy between two lumped states under a declared conductivity and an explicit interval. This phase-change state is a separate primitive; the current conduction API does not yet couple automatically to phase change. A future coupled stepper must conserve total energy across conduction and latent/sensible storage and should be tested against independent analytical cases.

---

**Trent Best** · The Singularity Workshop
