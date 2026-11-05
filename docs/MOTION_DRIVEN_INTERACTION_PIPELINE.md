# Motion-driven interaction pipeline

**Status:** proposed cross-package integration contract and research plan. This document does not claim that arbitrary-trajectory collision prediction, deformable contact, or the Gestures handoff is implemented.

## 1. Ownership and dependency direction

The physical world is changed by physical causes, not by presentation or by moving objects to make a scene look right.

- **Gestures** describes a procedure or motion intent and, where its evaluator supports it, evaluates the intended pose at a declared logical time.
- **A host-side adapter** maps stable target identities and motion semantics between Gestures and Physics. The Physics core must not reference Gestures.
- **Physics** owns authoritative physical state, trajectory prediction, collision confirmation, contact/structural/destructive response, and committed outcomes.
- **Visualize / Renderer** presents measured state and solver evidence. It does not author the physical result.

A requested path is not automatically an achieved path. Keep these motion modes distinct:

| Mode | Meaning | Physical consequence |
|---|---|---|
| Prescribed kinematic trajectory | An actuator or external mechanism owns the requested motion | Physics follows the declared path subject to the explicit kinematic policy and records reaction/load evidence where modeled |
| Controller target | A desired path or pose that the physical system attempts to track | Physics may deviate because of contact, force limits, constraints, or other dynamics |
| Force / torque contribution | A physical input with units, frame, application point, and activation semantics | Physics integrates it with the other declared physical contributions |
| Constraint / impulse / state transition | A typed non-force contribution | Physics applies its own corresponding solver or transition contract; it is not summed as an ordinary force |

## 2. Motion handoff data

The adapter should exchange a versioned, immutable description with at least:

- stable motion-definition and target-body identities;
- definition/model version and deterministic input identity;
- logical time domain, origin, duration or validity interval, and evaluation policy;
- coordinate frame and units;
- motion mode (prescribed, target, or typed physical contribution);
- pose evaluation and, only when defined, linear/angular velocity and acceleration;
- declared derivative continuity at segment boundaries;
- conservative bounds or a documented way to derive them over a requested interval;
- uncertainty, validity limits, and failure/unsupported-state semantics.

Do not infer acceleration by blindly differentiating arbitrary rendered frames. A piecewise-linear keyframe path has velocity discontinuities at keyframes and does not have a finite classical acceleration there. Quaternion interpolation also needs a declared angular-velocity convention. If derivatives are unavailable or discontinuous, the adapter must expose that fact; Physics can use bounded subdivision or a conservative envelope, or reject a prediction that cannot be bounded safely.

A stateful spring-damper integrator is not automatically an arbitrary-time function. Its contract must include state, initialization, integration method, timestep, and evaluation ordering. A pure absolute-time trajectory and a stateful dynamic response are different interfaces.

## 3. Predict occupied volumes, not path lines

A centerline or origin-path intersection is insufficient for finite bodies. Prediction operates on the transformed occupied shape (or a conservative enclosing volume) over a bounded time interval.

1. Establish a finite prediction interval from the next relevant simulation event and declared error/feature-scale limits.
2. Obtain conservative translation and rotation bounds over that interval.
3. Build swept-volume broadphase bounds. A broadphase overlap is only a candidate, never proof of contact.
4. Refine candidates with the cheapest valid narrowphase: primitive/analytic shape, then convex representation, then mesh or another expensive representation only when required.
5. Refine a possible first-contact time within the interval. Endpoint overlap alone is insufficient because two bodies can pass through one another and finish separated.
6. Produce a stable interaction record before selecting a solver.
7. Apply the selected contact, structural, deformation, or destructive model and commit the resulting physical state.
8. Publish observations from the committed result for presentation and replay.

Existing `AxisAlignedBounds.SweptBy` encloses linear translation only; it does not account for curved trajectories or rotation. Existing sweep-and-prune is a candidate generator. Existing orbital swept-sphere detection is a constant-velocity sphere reference, not a general motion API. Preserve those limits.

## 4. Adaptive prediction horizon

Do not predict an arbitrary number of minutes into the future. Choose a bounded interval based on spatial scale, relative motion, curvature/rotation, and model validity.

For a pair with a known relative speed bound (v_r), acceleration bound (a_r), and a local feature scale (L), one useful translational constraint is:

\[
v_r T + \frac{1}{2}a_r T^2 \leq \eta L
\]

where (0 < \eta < 1) is a declared conservative fraction. This is a horizon bound, not a complete collision algorithm. Include angular swept distance (for example, a bound based on shape radius and angular speed), curvature, uncertainty, and solver stability. If reliable bounds are unavailable, shorten the interval or report that prediction is unsupported rather than inventing safety.

Higher speed can require a larger spatial lookahead while requiring a shorter time interval and finer temporal subdivision. The system should select the interval from bounded spatial error and event proximity, not from a fixed global duration.

## 5. Interaction record

A confirmed interaction should be represented independently of a particular solver. The record should carry:

- stable participant IDs and relevant geometry/material/model versions;
- prediction interval and confirmed time of impact (or explicit uncertainty);
- contact locations, normals, relative velocities, and feature identities where available;
- classification of interaction type and the evidence used to classify it;
- solver/model identity and version;
- forces, impulses, work/energy transfer, structural stress, or damage only when that model actually computes them;
- pre/post state references, committed tick, and diagnostics/limitations.

A broadphase candidate must not be reported as a confirmed collision. A predicted contact must not be confused with a committed response. Participant identity must survive transfers and geometry/material version changes.

## 6. Research and implementation sequence

A. Define the adapter contract and motion-mode semantics without adding a Physics-to-Gestures dependency.

B. Establish analytical reference trajectories: stationary, constant velocity, constant acceleration, and bounded curved/rotating motion. Record exact or independently computed first-contact times.

C. Compare the existing constant-velocity swept-sphere detector as a baseline against bounded subdivision and time-of-impact refinement. Measure missed contacts, false candidates, time error, and work.

D. Add conservative swept-volume bounds for finite shapes. Include rotation and curvature; do not label endpoint union as continuous collision detection.

E. Add a deterministic multi-participant interaction record, keeping detection separate from response.

F. Integrate one narrowly scoped response model, with explicit limitations. Do not claim stable dense piles, general mesh CCD, deformation, or fracture until their solvers and tests exist.

G. Benchmark candidate count, narrowphase escalation, prediction subdivisions, time-of-impact refinements, allocations, and numerical error across sparse and dense workloads.

## 7. Required validation cases

- fast head-on crossing where endpoints are separated;
- grazing/tangential contact and near miss;
- accelerated approach and accelerated separation;
- curved path whose endpoint chord misses the swept volume;
- rotating elongated body whose translation-only bounds miss a possible contact;
- keyframe boundary with a declared derivative discontinuity;
- deterministic replay under identical definition/version/seed/configuration;
- permuted input enumeration with stable participant and pair ordering;
- bounded-horizon expiry and a case where no safe bound can be established;
- distinction between broadphase candidate, predicted contact, confirmed contact, and committed physical response.

## 8. Current implementation boundary

The repository already has reusable AABB/sweep-and-prune screening, a constant-velocity swept-sphere reference in the astronomy domain, rigid-body free-flight integration, endpoint sphere contact detection, and a single-contact frictionless impulse resolver. These are useful pieces, not a complete trajectory-aware world stepper.

The first experiment should compare the existing constant-velocity baseline with a bounded, deterministic trajectory evaluator on analytical cases. Keep the experiment observational: it predicts interactions but does not move bodies or pretend to solve structural/destructive consequences.

---

*The Singularity Workshop — edify, not mystify.*

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
