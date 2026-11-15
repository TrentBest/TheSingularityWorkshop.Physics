# Prescribed kinematic actor motion

**Status:** first bounded implementation on `development`. This is a deterministic trajectory evaluator, not a character controller or general physics solver.

## Contract

`PrescribedKinematicMotionDefinition` is immutable and identifies a motion and actor. It contains start/destination poses, an absolute logical start time, positive duration, and declared maximum linear/angular speeds. Position uses right-handed world-frame metres. Orientation is a normalized active body-local-to-world quaternion, stored scalar-first (W, X, Y, Z). Time is seconds and angular values are radians.

`PrescribedKinematicMotion.Evaluate` is a pure absolute-time function. It linearly interpolates position and uses shortest-arc spherical quaternion interpolation for orientation. It returns a `PrescribedKinematicMotionObservation` containing a `RigidBodyState` snapshot, the source motion identity, actor identity, and endpoint completion flag. Repeating the same definition/time/step produces the same observation; render-frame cadence is not an input.

The trajectory has constant linear and angular velocity over its open interval. It has a velocity discontinuity at the endpoints if the actor is stationary before/after the motion. Acceleration is therefore not represented as a finite continuous quantity at those boundaries. A future piecewise trajectory contract must define segment-boundary derivative semantics explicitly.

## Commit and cancellation boundary

The evaluator has no mutable state and does not commit to a world. The host/controller owns monotonic sample ordering and decides which observation becomes authoritative. Cancellation means retaining the last accepted observation and ceasing to accept later samples for that motion. The evaluator does not mutate or rewind a previously committed snapshot.

The observation's `StepIndex` and `ElapsedTime` are caller-supplied logical sample identity. The API does not impose a fixed-step scheduler; the host must provide its fixed logical step sequence. It is independent of display refresh rate.

## Explicit limits

- No collision detection, contact resolution, friction, obstacle avoidance, balance, gait, limb articulation, force response, or environmental-medium interaction.
- No acceleration-limited easing or continuous acceleration profile; endpoint velocity discontinuity is explicit.
- No multi-segment path, path curvature, or route planning.
- No hidden clamping: time outside the declared interval and motion exceeding declared speed bounds are rejected.
- This is prescribed kinematic motion, not a force-driven dynamic response.

For a host such as an Experience, the host owns intent and action lifecycle; an adapter creates definitions and commits observations; renderers consume the achieved snapshots. Story progression and application counters remain outside Physics.
