# Celestial Mechanics Foundation

**Status:** one reusable Newtonian gravity-field evaluator is implemented and tested. A general N-body orbital integrator, collision/merger model, planetary impact solver, and celestial Experience persistence are not yet implemented by this work.

## What exists now

`TheSingularityWorkshop.Physics.Astronomy.NewtonianGravityField` evaluates the acceleration vector and specific gravitational potential at a requested point from a finite set of ideal point masses.

For source body \(i\), with mass \(m_i\), position \(\mathbf{x}_i\), sample position \(\mathbf{x}\), and explicitly supplied gravitational constant \(G\):

\[
\mathbf{g}(\mathbf{x}) = \sum_i Gm_i \frac{\mathbf{x}_i-\mathbf{x}}{|\mathbf{x}_i-\mathbf{x}|^3}
\]

\[
\Phi(\mathbf{x}) = -\sum_i \frac{Gm_i}{|\mathbf{x}_i-\mathbf{x}|}
\]

The returned acceleration is in m/s²; the specific potential is in m²/s². Positions are metres in one shared Cartesian frame, masses are kilograms, and the caller supplies \(G\) in m³/(kg·s²). The evaluator sorts sources by stable body ID before accumulation, rejects duplicate IDs, validates positive finite masses, and rejects a sample exactly at a point-mass singularity.

## What this enables

- A gravity-field overlay whose vectors and potential contours derive from the same body placements as the Experience.
- A foundation for comparing how moving or changing the mass of a body changes the field.
- Reproducible CPU reference samples that a renderer can turn into arrows, contours, heatmaps, or a gravity-well metaphor.
- A small test surface for validating units, direction, superposition, and deterministic source ordering.

## What it does not do

This evaluator does not advance bodies through time, calculate stable orbits, detect or resolve collisions, model finite body radii, simulate relativistic effects, or calculate shockwaves. A point-mass potential is singular at the source and is not a planetary interior model. The optional “gravity well” surface is a visual encoding of a scalar field, not a literal force law.

## Next physics gates

1. **Orbital state contract:** stable body identity, mass, radius, position, velocity, epoch, SI units, and model version.
2. **Deterministic N-body stepper:** a declared integrator and fixed timestep, with tests for symmetric two-body behavior, repeatability, and momentum/energy drift across a stated horizon.
3. **Finite-body encounters:** contact detection and an explicit collision policy (merge, bounce, or another modeled outcome); never assume a collision necessarily stabilizes a system.
4. **Impact handoff:** preserve the pre-impact state and provide a versioned initial-condition record for a distinct local impact model. Do not claim planetary-scale shock physics until a validated solver supports it.
5. **Experience evidence:** save and reopen a scenario, derive the field visualization from authoritative samples, and show event/timeline provenance.

The Experience can be authored in parallel, but it must negotiate these capabilities rather than imply they already exist.
