# Deterministic CPU Reference World Stepper

## Purpose and supported model

`DeterministicCpuWorldStepper` composes the existing bounded mechanics primitives into one authoritative, fixed-step CPU reference path. It currently accepts finite-mass spherical bodies with constant world-frame linear acceleration over one fixed timestep.

It is a deliberately small interacting-world contract, not a universal rigid-body engine. It has no static/kinematic body kind, arbitrary collision shapes, friction, rolling resistance, angular contact response, persistent contact cache, simultaneous multi-contact solver, joints, CCD, or articulated mechanics.

## Authoritative stage order

For one call to `Step(configuration, bodies)`, the implementation performs:

1. **Validate and canonicalize input.** Require at least one body; unique ordinal string IDs; matching definition/state identities; positive finite mass and sphere radius; finite velocities; and exactly matching input logical step/time across all bodies.
2. **Advance free motion.** Integrate every body in ordinal body-ID order with `RigidBodyIntegrator.Advance`, using its declared constant acceleration and the one configured fixed timestep.
3. **Build conservative swept bounds.** Create a sphere AABB at each input pose, expand each side by half the configured contact tolerance, and linearly sweep it to the integrated endpoint. The sweep encloses the segment between endpoints; it does not model curved trajectories or rotation.
4. **Generate broadphase candidates.** Assign numeric proxy IDs from the sorted body order (never runtime hash codes), then use `SweepAndPruneBroadphase`. Candidate pairs are stable and sorted.
5. **Confirm endpoint contacts.** Run `SphereSphereContactDetector` on integrated endpoint states only. A swept candidate that is separated at the endpoint is not a contact.
6. **Fail closed on unsupported contact graphs.** If any body participates in more than one active endpoint contact, reject the entire step before response. This prevents order-dependent sequential resolution from being misrepresented as a coupled solver.
7. **Resolve each isolated contact.** Apply the existing frictionless translational normal impulse, then bounded inverse-mass-weighted positional correction.
8. **Rebuild final broadphase and recompute contact geometry.** Do not return stale pre-correction contact point, normal, separation, or relative velocity. If positional correction introduces a new contact with a previously separated body, reject the step; cascading response is not supported.
9. **Account and publish.** Compute momentum residual and translational kinetic-energy accounting, then return immutable, ordinally ordered body states and final contact observations. No partial result is returned if a stage fails.

The output tick is the common input tick plus one; elapsed time is the common input time plus exactly one configured timestep. Rendering cadence is not consulted.

## Numerical and diagnostic contract

- Free motion uses semi-implicit Euler as defined by `RigidBodyIntegrator`.
- Contact impulse is a single frictionless normal impulse using the configured coefficient of restitution.
- Position correction is stabilization, not a force or physical impulse; it can introduce non-conservative positional changes.
- Momentum residual is:

  `achieved total linear momentum - (input total linear momentum + declared constant-acceleration impulse)`.

  For isolated internal contacts, this should be near floating-point roundoff when no overflow occurs.
- Translational kinetic energy before and after is reported, along with its change. It is not called a conservation residual: external acceleration work and restitution dissipation affect it, and potential energy is not modeled here.
- The diagnostics include broadphase candidate count, endpoint contact count, resolved contact count, and total positional correction distance.
- Normal force remains unavailable. The contact resolver reports normal impulse only.

## Reproducible reference run

The companion record [cpu-world-two-sphere-reference.json](records/cpu-world-two-sphere-reference.json) specifies the complete model identity, timestep, body definitions, initial states, stable ordering, output states, contact observation, diagnostics, and limitations for a two-sphere inelastic impact. The test fixture `Step_EqualMassSphereImpact_ResolvesNormalVelocityAndReportsFinalGeometry` independently checks its numerical values against the executable implementation.

## Benchmark baseline

`DeterministicCpuWorldStepperBenchmarks` separates:
- free-body integration over the configured body count;
- broadphase candidate generation;
- one sphere-sphere narrowphase observation;
- one isolated normal contact response;
- the complete world-step call.

BenchmarkDotNet's memory diagnoser reports allocations alongside wall time. The benchmark suite is a baseline, not an optimization claim. Record the CI artifact's runner/runtime provenance and body count when comparing results. The current whole-world benchmark uses separated spheres, while the contact-stage microbenchmarks exercise a single supported pair; it does not claim dense-contact throughput.

## Explicit limitations

- **Discrete endpoint narrowphase:** fast bodies can cross and separate within one step without an endpoint contact. Swept AABB candidates do not provide continuous collision detection.
- **Linear swept bounds:** bounds enclose only start/end linear translation for the integrated state.
- **Single isolated contacts only:** any body in two or more active contacts rejects the step. No piles, chains, stacks, or simultaneous contact islands.
- **Sphere-only geometry:** no planes, ramps, rails, capsules, meshes, or arbitrary shapes.
- **Translational response only:** no friction, rolling, torque, angular inertia, angular contact impulse, or rotational penetration correction.
- **No persistence:** contacts are rediscovered each step; no warm starting, sleeping, or persistent manifolds.
- **No static or prescribed kinematic body type:** every accepted body has positive finite mass and is integrated as a dynamic body.

A marble-on-ramp, loop, domino pile, or escalator/elevator interaction must not be claimed from this slice. Those need additional shape, kinematic, contact, friction/rolling, and coupled-solver contracts with their own evidence.

---

## Resources & Support

[Physics home](../README.md) · [Physics Across Scales](PHYSICS_ACROSS_SCALES.md) · [Roadmap](ROADMAP.md)

*If a picture is worth a thousand words… then what is a living picture worth?*
