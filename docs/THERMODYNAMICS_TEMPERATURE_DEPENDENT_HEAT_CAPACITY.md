**Owner:** Trent Best · The Singularity Workshop

# Temperature-Dependent Heat Capacity

A material's specific heat capacity can change with temperature. When it does, the constant-capacity relation

\[
Q = m c_p (T_2-T_1)
\]

is not generally energy-consistent across a wide temperature interval. The correct sensible-energy relation is

\[
Q = m\int_{T_1}^{T_2} c_p(T)\,dT.
\]

The Physics thermodynamics library provides `TemperatureDependentHeatCapacity` for a bounded, tabulated curve. It interpolates specific heat capacity linearly between adjacent samples, integrates each segment analytically, and inverts the integrated enthalpy relation to find a final temperature after an energy transfer.

This is a numerical primitive, not a material database. Supply measured, sourced, estimated, or illustrative samples and preserve their evidence identity and validity conditions.

## 1. Define a curve

Each sample pairs an absolute temperature with a mass-specific heat capacity:

- Temperature: kelvin (K)
- Specific heat capacity: joules per kilogram-kelvin (J/(kg·K))
- Mass: kilograms (kg)
- Energy: joules (J)

The constructor requires at least two samples, strictly increasing temperatures, finite non-negative temperatures, and finite positive heat capacities. Evidence classification, source identifier, source version, and validity conditions are required.

```csharp
using TheSingularityWorkshop.Physics.Materials;
using TheSingularityWorkshop.Physics.Thermodynamics;

var curve = new TemperatureDependentHeatCapacity(
    materialId: "example-alloy",
    samples:
    [
        new HeatCapacitySample(200d, 450d),
        new HeatCapacitySample(300d, 510d),
        new HeatCapacitySample(400d, 590d)
    ],
    evidenceStatus: PropertyEvidenceStatus.Illustrative,
    evidenceSource: "workshop-example",
    evidenceVersion: "1",
    evidenceConditions: "Synthetic teaching data; not measured material properties.");
```

Replace the illustrative values and provenance with the actual data source before using the curve as a material claim.

## 2. Evaluate heat capacity

`GetSpecificHeatCapacity(temperatureKelvin)` performs linear interpolation between the surrounding samples. Both endpoints of the declared range are inclusive. Temperatures outside the range throw; the implementation never silently extrapolates.

For adjacent samples \((T_0,c_0)\) and \((T_1,c_1)\), the interpolated value is

\[
c_p(T)=c_0+\frac{T-T_0}{T_1-T_0}(c_1-c_0).
\]

## 3. Calculate sensible enthalpy change

For a piecewise-linear curve, the integral on one segment is exactly the trapezoid area:

\[
\Delta h_i = \frac{c_{p,i}+c_{p,i+1}}{2}(T_{i+1}-T_i).
\]

The implementation splits an interval at every sample boundary and sums these segment integrals. The result is specific enthalpy change in J/kg. Reversing the endpoints reverses the sign.

```csharp
double specificEnthalpyChange =
    curve.GetSpecificEnthalpyChangeJoulesPerKilogram(
        referenceTemperatureKelvin: 200d,
        targetTemperatureKelvin: 350d);
```

## 4. Apply energy to a lumped body

```csharp
double finalTemperatureKelvin = curve.ApplyNetEnergyTransfer(
    massKilograms: 2d,
    initialTemperatureKelvin: 250d,
    netEnergyTransferJoules: 25_000d);
```

Positive energy enters the body; negative energy leaves it. The method divides by mass to obtain specific energy and uses a fixed 80-iteration bisection over the declared temperature interval to invert the integrated enthalpy curve. The fixed iteration count bounds work and makes the search deterministic for identical inputs.

The final temperature is constrained to the curve's declared range. If the requested energy would require extrapolation, the operation fails with `ArgumentOutOfRangeException`; it does not guess what the material does beyond the supplied evidence.

## 5. Evidence and applicability

The curve records:

| Field | Meaning |
|---|---|
| `MaterialId` | Stable material or reference-material identity |
| `EvidenceStatus` | Measured, sourced, estimated, or illustrative |
| `EvidenceSource` | Source identifier or stable fixture identifier |
| `EvidenceVersion` | Source revision or fixture version |
| `EvidenceConditions` | Declared conditions and limitations |

The code validates that these fields are present; it cannot independently verify the source or interpret free-text conditions. The caller remains responsible for checking that the data applies to the material state, composition, pressure, phase, and process being simulated.

## 6. Model boundaries

This implementation deliberately does not:

- Extrapolate outside the tabulated temperature range.
- Model latent heat or phase transitions; use [equilibrium phase change](THERMODYNAMICS_PHASE_CHANGE.md) for the separate fixed-transition model.
- Apply pressure, composition, anisotropy, or heating-rate corrections.
- Propagate measurement uncertainty or reconcile conflicting sources.
- Claim that an illustrative curve represents a real material.
- Model spatial temperature gradients; it is a lumped-body energy update.

The curve must describe one continuous phase and one applicable material condition over its declared range. Split the model at a phase transition rather than interpolating a heat-capacity curve through latent heat.

## 7. Validation

The thermodynamics test suite checks interpolation, exact segment integration, integration across a knot, heating and cooling sign convention, inversion, energy round-trip, range enforcement, and invalid sample/provenance rejection.

The synthetic test curves are explicitly marked illustrative. They verify the mathematics and software contract, not empirical material accuracy.

---

**Trent Best** · The Singularity Workshop
