# Spatial Distance Spring-Damper

**Status:** implemented in this branch; CI validation pending. This is a bounded three-dimensional compliant-link model, not a rigid-joint or general articulated solver.

![Living spring-damper illustration: the link smoothly compresses and extends while equal-and-opposite spring forces reverse direction](images/spring-damper-living.svg)

This is a conceptual view of the spring component of the force law, not a run produced by the solver. The actual implementation also accounts for radial damping and advances translation with semi-implicit Euler; those effects are not encoded in this illustration.

## Model

The link acts along the line joining two body origins. For separation length (r), rest length (r_0), unit direction (hat{n}) from body A to body B, and relative radial velocity (v_r), the force on A is

\[
\mathbf{F}_A = \hat{n}\left(k(r-r_0)+c v_r\right), \qquad \mathbf{F}_B=-\mathbf{F}_A
\]

where (k) is stiffness and (c) is damping. The radial speed is the projection of relative linear velocity onto \(\hat{n}\). Forces are evaluated from the pre-step state, and translation advances using semi-implicit Euler.

## Units and observations

- Position and rest length: metres.
- Linear velocity: metres per second.
- Mass: kilograms.
- Stiffness: newtons per metre.
- Damping: newton-seconds per metre.
- Force: newtons.
- Time: seconds, supplied as a logical step and positive timestep.
- Body identity and link identity: stable, caller-provided strings.

Each result carries the link identity, step index, end-of-step logical time, equal-and-opposite axial force vectors, and resulting body snapshots. The reported force is a spring-damper force, not an impulse or rigid-joint reaction.

## Deliberate limitations

- The force acts through body origins; no local anchor points or torque arms are modeled.
- Orientation and angular velocity are preserved but not integrated or coupled to the link.
- Coincident body origins have no defined axial direction; this model reports zero axial force for that step.
- There is no collision detection, contact response, friction, rigid-joint enforcement, or topology mutation.
- Explicit spring integration can become unstable for stiff links or large timesteps; callers must select and validate the timestep for their parameter range.
- This does not satisfy the complete articulated-mechanics milestone by itself.

## Validation cases

Tests cover an analytical axis-aligned extension, a non-axis-aligned link with radial damping, equal-and-opposite force, preservation of orientation/angular velocity, the coincident-origin singularity, deterministic repeatability, and invalid mass/timestep/identity.

---

[Physics roadmap](ROADMAP.md) · [Articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md) · [Determinism contract](DETERMINISM.md)
