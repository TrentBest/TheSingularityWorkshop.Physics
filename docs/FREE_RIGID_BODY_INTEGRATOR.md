# Free Rigid-Body Reference Integrator

**Status:** implemented bounded numerical primitive; not a collision or constraint solver.

The free-body integrator is the first state-advancing primitive that consumes the shared spatial and rigid-body state contracts. It exists to give experiments a small, repeatable baseline before contacts, rolling friction, domino tipping, or high-count optimization are added.

## State transition

For a fixed logical timestep \(\Delta t > 0\), constant world-frame linear acceleration \(\mathbf{a}\), and incoming state \((\mathbf{x}_n, \mathbf{v}_n)\), the implementation uses semi-implicit Euler:

\[
\mathbf{v}_{n+1} = \mathbf{v}_n + \mathbf{a}\Delta t
\]

\[
\mathbf{x}_{n+1} = \mathbf{x}_n + \mathbf{v}_{n+1}\Delta t
\]

The method advances the logical step by one, increments elapsed logical time by \(\Delta t\), preserves stable body identity, and returns a new immutable snapshot. It does not mutate the input.

Angular velocity is interpreted in world coordinates and held constant during this step. The integrator constructs the axis-angle incremental rotation for \(\boldsymbol{\omega}\Delta t\) and composes it on the left of the existing body-local-to-world orientation. Zero angular velocity preserves the existing orientation.

## What this proves

- Invalid snapshots and non-positive/non-finite timesteps are rejected.
- Translational updates follow the declared semi-implicit Euler order.
- Orientation advances for a constant world-frame angular velocity and remains normalized.
- Repeated identical fixed-step runs produce identical snapshots in the tested runtime.
- Logical step and elapsed time travel with the resulting state.

These tests establish the local numerical contract, not cross-platform bitwise determinism, global energy conservation, convergence for every physical system, or real-world accuracy. Semi-implicit Euler is an approximation; choose timestep based on convergence/error measurements for the model being simulated.

## What this does not do

No force accumulation, mass/inertia model, torque integration, collision detection, contact resolution, friction, sleeping, continuous collision detection, or constraint solving is performed. In particular, this primitive alone cannot make a marble roll down a track or make a domino tip into its neighbor. Those behaviors require validated shape/contact models and a documented world-step ordering.

## Next validation steps

1. Compare free fall and constant-acceleration trajectories against analytical solutions across multiple timestep sizes; report error rather than just visual plausibility.
2. Add plane/surface contact observation and a tested response loop.
3. Add deterministic multi-body ordering and replay records before adding broadphase optimization.
4. Add sphere rolling/friction and finite-size domino rotation as separate capabilities, each with analytical or independently checkable tests.

Related: [Numerical Validation Playbook](NUMERICAL_VALIDATION.md) · [Physics Roadmap](ROADMAP.md) · [Marble Run and Domino Studio boundary](MARBLE_RUN_AND_DOMINO_STUDIO.md)
