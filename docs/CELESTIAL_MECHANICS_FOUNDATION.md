# Celestial Mechanics Foundation

**Status:** a Newtonian gravity-field evaluator, fixed-step N-body integrator, explicit pairwise perfectly inelastic merge policy, linear swept-sphere contact detector, and deterministic linear-encounter coordinator are implemented with focused tests. Bounce/fragmentation, angular-momentum storage, planetary impact physics, and Experience persistence remain out of scope.

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

This is a simple deterministic O(N²) reference stepper. Input states are validated and sorted by stable ID. The caller is responsible for choosing a suitable timestep and for deciding how often to step. Radius is physical metadata here; it does not soften gravity or resolve contact. Coincident body centres are rejected as singular rather than silently merged.

The tests include a symmetric circular binary, centre-of-mass and momentum behaviour, input-order determinism, invalid-state handling, empty state, bounded energy/angular-momentum drift over a full orbit, and timestep-convergence comparisons. These are focused numerical checks, not a claim of production-grade long-horizon orbital accuracy.

## Explicit pairwise merge policy

`OrbitalMergeResolver.TryMerge` is an opt-in policy for one pair of spherical bodies whose endpoint spheres overlap or touch. It does not mutate inputs or automatically alter the N-body collection.

For an accepted pair, it:
- conserves total mass, centre of mass, and linear momentum;
- computes a volume-additive equivalent spherical radius;
- emits a stable, pair-derived `OrbitalMergeEvent`; and
- reports the relative translational kinetic energy removed by the perfectly inelastic merge.

The lost kinetic energy is an accounting quantity, **not** a solved thermal field. This policy has no spin/deformation model and does not conserve angular momentum by storing it in the merged body's rotation. It is not a general-purpose asteroid-impact outcome model. Because it checks endpoint overlap only, fast bodies can tunnel through each other unless the caller chooses a sufficiently small timestep or uses swept collision detection. Multi-body collision ordering is not handled here.

## Linear swept-sphere contact detection

`OrbitalSweptCollisionDetector.TryDetect` tests two spherical bodies over a positive, finite interval under a constant-velocity (straight-line) trajectory assumption. It solves the relative-motion sphere-contact quadratic and returns the earliest contact time in the interval, including time zero for initial overlap/touch. Results order the two body IDs ordinally so reversing the arguments does not change event identity. No body state is mutated, and detection does not select a response policy.

This prevents tunnelling for the linear trajectories supplied to the detector, but it is not an exact continuous collision solver for the curved paths of gravitationally accelerated bodies. Its accuracy depends on the interval and the trajectory approximation. A future acceleration-aware detector may need to bound curvature or inspect the integrator's trajectory rather than extrapolate a straight line. The detector also does not coordinate multiple candidate encounters: a caller must sort events deterministically and revalidate later events after an earlier merger changes the body set.

## Deterministic encounter orchestration

`OrbitalEncounterCoordinator.ResolveLinearMergers` coordinates a finite, constant-velocity interval without mutating the caller's input states. It sorts body identities, evaluates swept contacts for each unique pair, and chooses the earliest candidate by time of impact; exact time ties are broken by ordinal `BodyAId` and then `BodyBId`.

After selecting a contact, it linearly drifts all current bodies to that event time, applies the explicit perfectly inelastic merge policy, removes the two source bodies, inserts the merged body, and rebuilds the candidate set for the remaining interval. Rebuilding is essential: a merger changes the body's identity, position, velocity, radius, and possible future contacts, so later candidates from the previous body set would be stale. Simultaneous contacts are resolved sequentially using the stable tie-break and recalculated after each outcome; this is deterministic, but it is not a claim that the chosen order models every possible multi-contact material outcome.

The coordinator is a **linear-drift encounter experiment**, not a gravity integrator. It does not apply gravitational acceleration during the interval, and therefore should be coupled to short integration steps or replaced by trajectory-aware event handling for curved gravitational paths. Every contact currently uses the merger policy; alternative response policies are a separate future layer. A tiny geometric correction is applied only if floating-point rounding leaves a predicted contact infinitesimally outside the merger's endpoint-overlap check; the correction preserves the pair's centre of mass.

## What this enables

- A gravity-field overlay whose vectors and potential contours derive from the same body placements as the Experience.
- Repeatable orbital previews and testable integration drift/convergence.
- A deterministic, explicitly selected baseline for low-complexity merger experiments.
- Reproducible CPU reference samples that renderers can turn into arrows, contours, heatmaps, or a gravity-well metaphor.
- A future impact handoff that records source states and the selected encounter policy rather than disguising visual effects as computed physics.

## What it does not do

These components do not advance finite planetary interiors, provide acceleration-aware continuous collision detection, resolve elastic bounces or fragmentation, conserve energy exactly, model relativistic effects, or calculate shockwaves. Point-mass gravity is singular at the source and is not a planetary interior model. The optional “gravity well” surface is a visual encoding of a scalar field, not a literal force law. A collision does not necessarily stabilize a system.

## Next physics gates

1. **Trajectory-aware encounter handling:** integrate event detection with gravity-step trajectories or bound curvature error instead of relying only on straight-line drift over each interval.
2. **Alternative outcomes:** define bounce and fragmentation as separate policies with explicit momentum/angular-momentum/energy accounting rather than assuming every contact merges.
3. **Impact handoff:** preserve pre-impact state and provide a versioned initial-condition record for a distinct local impact model. Do not claim planetary-scale shock physics until a validated solver supports it.
4. **Experience evidence:** save and reopen a scenario, derive field visualization from authoritative samples, and show event/timeline provenance.

The Experience can be authored in parallel, but it must negotiate these capabilities rather than imply they already exist.
