# Thermodynamics: first bounded primitive

## What is implemented

`LumpedThermalState` represents one well-mixed thermal body using:

- mass in kilograms;
- absolute temperature in kelvin;
- constant specific heat capacity in J/(kg·K);
- a declared reference temperature in kelvin;
- sensible energy relative to that reference, in joules.

For this deliberately limited model:

`E_sensible = m × c × (T − T_reference)`

Applying a net energy transfer `Q` updates the temperature through the inverse relation. Positive `Q` heats the body; negative `Q` cools it. State updates return a new immutable snapshot.

## Assumptions and exclusions

- The body is lumped: there are no spatial temperature gradients.
- Specific heat capacity is constant over the modeled temperature range.
- No phase change, latent heat, chemical reaction, pressure dependence, thermal expansion, or heat-transfer rate is modeled.
- The reported energy is **relative sensible energy**, not absolute thermodynamic internal energy.
- This primitive does not decide where energy comes from or goes. A future heat-transfer or reaction model must supply an explicit net energy transfer and account for its other side.

The implementation rejects invalid/non-finite properties, non-finite transfers, overflowed state, and transfers that would place the body below absolute zero.

## Why start here

A fire scenario needs energy accounting before it needs a flame representation. This primitive gives later conduction, convection, radiation, and reaction models a small, testable thermal state to exchange energy with. It is not yet a heat-transfer solver or a combustion model.

## Validation

Unit tests cover the reference-energy relation, heating, cooling, zero transfer, invalid inputs, overflow, and the absolute-zero boundary. The CI run for the PR is the authoritative validation result; this document alone does not mark the capability complete.
