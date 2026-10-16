# Articulated Mechanics Integration Contract

**Status:** proposed cross-package contract; no articulated rigid-body solver is implemented by this document.

This document identifies the physics outputs that a stateful kinematics/controller layer such as Gestures needs. It is intentionally engine-neutral and does not require Physics to depend on Gestures.

## Responsibility split

| Concern | Owner |
|---|---|
| Requested motion, target selection, legal kinematic candidates, retained IK/controller state | Gestures or another controller |
| Mass, inertia, applied forces, constraint solving, collision detection/response, contact mechanics, physical integration | Physics |
| Structural joint failure thresholds and domain policy | Declared joint/material/domain model, evaluated against Physics observations |
| Physical body topology after a constraint fails | Articulated-body/constraint owner in Physics or its integration layer |
| Ligament/tissue injury, fracture severity, soldier capability changes, gameplay response | Explicit material/injury/domain model and the owning application |
| Rendering and visual effects | Renderer/host consuming authoritative outcomes |

These responsibilities may communicate through small data contracts. Physics must not depend on Gestures, a renderer, a game, or an application framework. A host integration can translate between the packages.

## Minimum useful physics observations

The first articulated mechanics slice should produce enough evidence for a controller to react without inferring physical truth from a desired pose.

### 1. Simulation step and achieved state

Every observation belongs to an explicit logical simulation step/time and identifies the model/run that produced it. Report the achieved body/joint pose and velocity as needed by the controller, distinct from its requested target. Units, coordinate frames, and frame conventions must be explicit.

Rendering-frame timing must not implicitly define physical time. The caller must be able to distinguish a fresh observation from a stale, missing, deferred, or invalid one.

### 2. Contact observations

For each detected contact, expose stable identities for the participating bodies/shapes and, where supported, a contact point, normal, relative velocity, and normal/tangential force or impulse. Report whether the contact remains active and whether the modeled contact is sticking, sliding, separating, or otherwise unresolved.

Do not report Supported merely because two shapes overlap. A load-bearing conclusion requires a declared contact/friction/support model and a result from that model. Optional or unavailable measurements must remain explicitly unavailable rather than being fabricated.

### 3. Articulated constraint loads

For each constraint/joint, expose the solver's measured or computed reaction loads for a specific step. At minimum, define force and torque units, the reference frame, sign/magnitude convention, and the point/axis about which torque is reported.

Before these observations can drive failure, the contract must decide whether a threshold applies to peak load in a step, an averaged/sustained load, an impulse, accumulated fatigue, or another explicit criterion. Force, torque, and impulse are different quantities and must not be silently substituted for one another.

A breakable joint must be able to declare its structural limits, but the physical model owns the loads used to assess them. The threshold comparison and the solver's reported load must use compatible units, frames, and temporal semantics.

### 4. Structural failure and topology

A structural failure observation should identify the failed constraint, the simulation step, the measured load evidence, and whether this is the first transition to failure. The body/constraint owner then decides when and how to remove, disable, or replace that constraint and how the resulting body topology is represented.

Do not automatically equate a broken kinematic link with detached rigid bodies. Likewise, a broken structural constraint does not by itself establish a ligament tear, bone fracture, or injury severity. Those are separate models with their own evidence and state transitions.

### 5. Deterministic execution and replay

The articulated slice must declare its integration timestep, solver configuration, constraint/contact ordering, and supported determinism level. It should provide reproducible test scenarios and record enough inputs and outputs to compare runs. Do not claim cross-platform bitwise determinism without testing it.

## Proposed first vertical slice

Keep the first implementation bounded and independently testable:

1. Define the minimal spatial/vector, rigid-body state, and force/torque conventions required by one articulated scenario. Reuse shared quantity/unit types if they exist; avoid building a general unit framework just for this slice.
2. Add a small constrained articulated model with explicit masses/inertias and a documented solver scope.
3. Emit achieved state, contact observations, and per-constraint reaction loads with step identity and coordinate conventions.
4. Add one threshold-driven structural constraint failure scenario. Preserve the distinction between detecting failure and applying a topology change.
5. Add analytical or independently checkable cases, invariants, invalid-input tests, and deterministic replay tests within the declared scope.
6. Build a tiny adapter/contract test that demonstrates how Gestures consumes those observations without introducing a Physics -> Gestures dependency.

The simplest model that validates the contract is preferable to an expansive but unvalidated rigid-body API.

## Acceptance checklist

- [ ] Requested and achieved pose are separate values.
- [ ] Every physical observation identifies its logical step/time.
- [ ] Units and coordinate frames are documented for every vector and torque.
- [ ] Contact presence is distinct from load-bearing support.
- [ ] Constraint reaction loads have declared temporal semantics.
- [ ] Structural failure is distinct from topology mutation and injury.
- [ ] Tests cover load threshold boundaries and the first-failure transition.
- [ ] Tests cover contact transitions and unsupported/sliding outcomes where modeled.
- [ ] Replay/determinism claims match measured test evidence.
- [ ] The Physics core remains independent of Gestures and presentation packages.

## Relationship to current implementation

The current development branch contains finite spatial vectors, normalized quaternion orientation, metre-based poses, timestamped rigid-body snapshots, a bounded 3D translational distance-link experiment, a validated per-step `ContactObservation` contract, sphere-sphere narrowphase detection, a limited frictionless normal-impulse response for one contact at a time, and bounded single-contact positional correction. These are deliberately separate primitives, not a general collision or articulated solver.

The contact resolver updates linear velocities only; it does not account for angular inertia, friction, simultaneous contacts, broadphase, persistent contact manifolds, or rigid joints. Positional correction is a separate stabilization operation and callers must recompute narrowphase contacts afterward. There is still no rigid-body time integrator, articulated constraint-reaction-load model, structural-failure/topology implementation, or completed Physics-to-Gestures acceptance suite. The complete proposed slice remains future work.
---

## Resources

[Physics architecture](ARCHITECTURE.md) · [Determinism contract](DETERMINISM.md) · [Contact observation contract](CONTACT_OBSERVATION.md) · [Sphere-sphere contact detection](SPHERE_SPHERE_CONTACT.md) · [Normal contact impulse response](CONTACT_IMPULSE_RESPONSE.md) · [Contact positional correction](CONTACT_POSITIONAL_CORRECTION.md) · [Physics roadmap](ROADMAP.md) · [Gestures](https://github.com/TrentBest/TheSingularityWorkshop.Gestures)
