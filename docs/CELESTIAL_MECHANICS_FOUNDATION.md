# Celestial Mechanics Foundation

**Status:** a Newtonian gravity-field evaluator and a first fixed-step N-body integrator are implemented with focused tests. Finite-body collision/merger policy, planetary impact physics, and Experience persistence are not implemented by these components.

## Gravity field

`TheSingularityWorkshop.Physics.Astronomy.NewtonianGravityField` evaluates acceleration and specific gravitational potential at a point from ideal point masses.

For source body \(i\), with mass \(m_i\), position \(\mathbf{x}_i\), sample position \(\mathbf{x}\), and explicitly supplied gravitational constant \(G\):

\[
\mathbf{g}(\mathbf{x}) = \sum_i Gm_i \frac{\mathbf{x}_i-\mathbf{x}}{|\mathbf{x}_i-\mathbf{x}|^3}
\]

\[
\Phi(\mathbf{x}) = -\sum_i \frac{Gm_i}{|\mathbf{x}_i-\mathbf{x}|}
\]

Acceleration is in m/s²; specific potential is in m²/s². Positions are metres in one shared Cartesian frame, masses are kilograms, and the caller supplies \(G\) in m³/(kg·s²). Sources are sorted by stable body ID before accumulation; duplicate IDs and a sample exactly at a point-mass singularity are rejected.

## First N-body integrator

`OrbitalBodyState` carries stable ID, mass, radius, position, and velocity. `NewtonianNBodyStepper.Step` advances the entire state by one positive, caller-supplied fixed timestep using velocity Verlet:

1. Evaluate acceleration at the initial positions.
2. Advance positions with \(\mathbf{x}_{n+1}=\mathbf{x}_n+\mathbf{v}_n\Delta t+\frac12\mathbf{a}_n\Delta t^2\).
3. Re-evaluate acceleration at the new positions.
4. Advance velocities with \(\mathbf{v}_{n+1}=\mathbf{v}_n+\frac12(\mathbf{a}_n+\mathbf{a}_{n+1})\Delta t\).

This is a simple deterministic O(N²) reference stepper. Input states are validated and sorted by stable ID. The caller is responsible for choosing a suitable timestep and for deciding how often to step. The radius is carried as physical metadata but does not yet affect gravity or resolve contact. Coincident body centers are rejected as singular rather than silently merged. Current tests cover a symmetric circular binary, center-of-mass and momentum behavior, input-order determinism, invalid state, and empty state. These tests are a starting validation surface, not a claim of production-grade long-horizon orbital accuracy.

## What this enables

- A gravity-field overlay whose vectors and potential contours derive from the same body placements as the Experience.
- Repeatable orbital previews and a foundation for comparing integrator drift over declared horizons.
- A source of initial conditions for later encounter and impact models.
- Reproducible CPU reference samples that renderers can turn into arrows, contours, heatmaps, or a gravity-well metaphor.

## What it does not do

Neither component advances finite planetary interiors, detects or resolves collisions, conserves energy exactly, models relativistic effects, or calculates shockwaves. Point-mass gravity is singular at the source and is not a planetary interior model. The optional “gravity well” surface is a visual encoding of a scalar field, not a literal force law. A collision does not necessarily stabilize a system.

## Next physics gates

1. **Numerical evidence:** measure energy and angular-momentum drift over documented timestep/horizon sweeps; add convergence checks and near-encounter stress cases.
2. **Finite-body encounters:** define contact detection and explicit merge/bounce/fragmentation policy, with conservation accounting and deterministic event records.
3. **Impact handoff:** preserve pre-impact state and provide a versioned initial-condition record for a distinct local impact model. Do not claim planetary-scale shock physics until a validated solver supports it.
4. **Experience evidence:** save and reopen a scenario, derive field visualization from authoritative samples, and show event/timeline provenance.

The Experience can be authored in parallel, but it must negotiate these capabilities rather than imply they already exist.
