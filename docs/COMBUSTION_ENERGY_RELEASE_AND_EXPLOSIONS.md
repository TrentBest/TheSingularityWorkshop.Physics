# Combustion Energy Release and Explosion Boundaries

**Status:** bounded energy-ledger contract; not an explosion or blast solver.

**Owner:** Physics owns the energy accounting and physical event state. Rendering and cached Workshop effects consume the resulting observations; they do not determine how much energy exists or how a vehicle fails.

## 1. Separate the energy ledger from the event mechanics

A vehicle's stored fuel energy is an upper bound on chemical energy available from that fuel under the declared specific-energy model:

\[
E_{\mathrm{stored}} = m_{\mathrm{fuel}}\, e_{\mathrm{chemical}}
\]

The energy attributed to fuel that has actually reacted is:

\[
E_{\mathrm{released}} = m_{\mathrm{reacted}}\, e_{\mathrm{chemical}}
\]

The released energy is not automatically blast energy. It may be partitioned among mechanical work, thermal energy, radiation, and effects not yet modelled. A fire may release energy over many steps; a rapid pressure event needs a distinct event model with an explicit duration and pressure/impulse contract.

CombustionEnergyReleaseCalculator implements this limited energy ledger. It requires a material-specific, source-attributed specific-chemical-energy property in J/kg, checks fuel temperature against the property's declared validity range, rejects reacted mass greater than stored mass, and requires explicit energy-partition fractions whose sum is at most one. Any remainder stays visible as unallocated released energy; the calculator does not silently turn it into mechanical damage.

Its illustrative test fixture is bookkeeping-only and is not evidence for a real fuel property.

## 2. Explosion modelling is a separate next layer

The next event model must distinguish at least:

- **Stored chemical energy:** fuel still available, with material and provenance identity.
- **Reaction progress:** how much fuel has reacted and when; chemistry/kinetics and oxygen availability govern this.
- **Energy partition:** what fraction is available for pressure work, thermal loading, radiation, and other modelled sinks.
- **Pressure and impulse:** a pressure-time/impulse field acting on geometry; energy alone does not determine peak pressure or impulse.
- **Structural response:** panel deformation, joint failure, fragment release, inward collapse, and part motion under loads, with energy and momentum accounting.
- **Residual fire:** remaining fuel, oxygen, hot surfaces, and ignition opportunities after the initial event.

A missile impact is an initiating interaction, not an instruction to explode the full fuel tank. Penetration, tank rupture, fuel release, mixing with oxidizer, ignition, confinement, and event timescale must be explicit state and model inputs. If those are not available, the model should report a bounded or unsupported result rather than inventing a blast.

## 3. Simulation truth versus cached presentation

A Workshop may cache a family of less-focused explosion presentations, with LUT-driven variations in fragments, deformation, flame, smoke, and lighting. Cache selection must be driven by recorded physical event descriptors—such as released-energy band, impulse class, affected geometry, and event phase—and must not feed forces or energy back into Physics.

For correctness and replayability, a simulation step should publish one versioned observation snapshot. Visible-light rendering, heat-vision rendering, and mathematical diagnostics read that same snapshot. Changing the view, LUT, or cached effect must not change fuel consumption, impulse, fragment trajectories, or structural state.

## 4. Current limits

This ledger does not calculate detonation, deflagration, gas expansion, peak pressure, blast-wave propagation, fragment launch, armor penetration, joint failure, or crumpling. It is an accounting foundation for those separate models, not proof that the repository can yet explode vehicles physically.

## 5. Verification target

Tests must establish that stored energy is not released twice; released energy never exceeds available energy; all partitions remain non-negative and bounded; repeated presentation changes do not alter simulation state; and any future mechanical work, heat, radiation, and residual terms reconcile to the declared released-energy budget within documented numerical tolerance.
