# Prescribed kinematic actor motion

**Status:** implemented first bounded root-motion capability on `development`, including fixed-step sampling, achieved-state validation, and a typed MicroBundle provider. This is not a character controller or a general physics solver.

## Contract

`PrescribedKinematicMotionDefinition` is an immutable, versioned request identifying a motion and actor. It contains start and destination poses, absolute logical start time, positive duration, and declared maximum linear/angular speeds. Position uses right-handed world-frame metres. Orientation is a normalized active body-local-to-world quaternion, stored scalar-first (W, X, Y, Z). Time is seconds and angular values are radians.

`PrescribedKinematicMotion.Evaluate` is a pure absolute-time function. It linearly interpolates position and uses shortest-arc quaternion interpolation for orientation. `EvaluateFixedStep` derives logical time from the step index, fixed timestep, and total step count; render-frame cadence is not an input. Both return immutable `PrescribedKinematicMotionSample` values with the definition identity/version and a `RigidBodyState` sample.

The current trajectory has constant linear and angular velocity over its open interval. It has a velocity discontinuity at the endpoints if the actor is stationary before or after the motion. It does not claim a finite continuous start/stop acceleration bound. A smooth, acceleration-limited profile remains future work.

## Commit and cancellation boundary

The evaluator does not mutate world state. The host/controller owns monotonic sample ordering and decides which state becomes authoritative. `PrescribedKinematicMotion.Commit` validates that a separately supplied achieved `RigidBodyState` matches the requested actor, definition version, step, and logical time, then reports arrival only when the destination sample is committed within both position and angular tolerances.

Cancellation means retaining the last committed immutable snapshot and declining to commit later samples. The evaluator does not rewind or mutate prior state.

## MicroBundle provider

`PrescribedKinematicMotionMicroBundle` exposes `IPrescribedKinematicMotionProvider` with a stable provider ID. The adapter is intentionally host-neutral: its provider declaration is not a global service locator. A composition host must resolve the loaded bundle instance and explicitly bind the typed provider contract.

The adapter has no dependency on Experiences, Gestures, Renderer, Visualization, Foundry, FSM_COS, or a host framework.

## Explicit limits

- No collision detection, contact resolution, friction, obstacle avoidance, balance, gait, limb articulation, force response, or environmental-medium interaction.
- No acceleration-limited easing, continuous start/stop velocity profile, multi-segment path, path curvature, or route planning.
- No hidden clamping: time outside the declared interval and motion exceeding declared speed bounds are rejected.
- No persistent world-step scheduler or host transaction coordinator; the host owns scheduling, commit ordering, and cancellation.
- No host-specific browser/runtime bridge. Static HTML cannot directly invoke this C# assembly without an explicit runtime bridge.

Focused Physics tests cover translation/orientation, stationary motion, speed limits, deterministic replay, fixed-step scheduling, achieved-state tolerances, forged-sample rejection, and interruption semantics. MicroBundle tests cover descriptor/provider metadata, typed contract exposure, fixed-step evaluation, and commit validation.

For Lenny, this capability can supply deterministic root-pose samples for approach, pickup staging, carrying, and travel to a receptacle. Experiences still owns the story, actor intent, cleanup lifecycle, and deposit counter; the counter must change only after the Experience confirms a completed deposit.
