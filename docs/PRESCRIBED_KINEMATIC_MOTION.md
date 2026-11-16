# Prescribed Kinematic Motion

**Status:** implemented bounded absolute-logical-time and fixed-step evaluation, achieved-state commit validation, and a typed MicroBundle provider adapter. A concrete host composition sample and acceleration-limited profile remain future work.

## Ownership

A prescribed trajectory is a kinematic request, not a force-driven physical response.

- The Experience/controller owns intent, destination, and cancellation.
- A host adapter creates a versioned PrescribedKinematicMotionDefinition.
- Physics evaluates the trajectory at an explicit logical time and step, or derives time from a declared fixed-step schedule.
- The Physics MicroBundle exposes the evaluator and commit validator through `IPrescribedKinematicMotionProvider`; the composition host must explicitly resolve the loaded bundle instance and bind the typed provider.
- The host/world supplies the separately achieved RigidBodyState.
- PrescribedKinematicMotion.Commit pairs the request with that achieved snapshot and reports arrival only when the committed snapshot is within the supplied destination tolerances.
- Renderer and Visualization consume achieved observations; they do not calculate root motion.

Physics core has no dependency on Experiences, Gestures, Renderer, Visualization, Foundry, FSM_COS, or a host framework.

## Current mathematical contract

The bounded profiles are constant-velocity motion and quintic smooth-step motion. Both use shortest-arc quaternion interpolation for orientation. The default constant-velocity profile uses:

\[
\mathbf{x}(t)=\mathbf{x}_0+u(\mathbf{x}_1-\mathbf{x}_0),\qquad
\mathbf{v}=\frac{\mathbf{x}_1-\mathbf{x}_0}{T}
\]

where \(u=(t-t_0)/T\), for \(t_0\le t\le t_0+T\).

Orientation uses normalized scalar-first quaternions \((W,X,Y,Z)\), active body-local-to-world rotations, and the shortest quaternion arc. Angular velocity is constant about the corresponding world-frame rotation axis:

\[
\lVert\boldsymbol\omega\rVert=\frac{\theta}{T}
\]

Inputs and outputs use metres, seconds, and radians. Position is in a right-handed world frame. The absolute-time evaluator is a pure function of the definition, logical time, and caller-supplied step index. The EvaluateFixedStep entry point derives logical time from an explicit fixed timestep and total step count; render cadence is not an input.

The optional `QuinticSmoothStep` profile uses \(s(u)=10u^3-15u^4+6u^5\), with \(u=(t-t_0)/T\). Its first derivative is zero at both endpoints, and its second derivative is also zero there, so the prescribed root motion starts and stops with zero velocity and acceleration. Its peak normalized speed factor is 1.875 and its peak normalized acceleration factor is \(10/\sqrt{3}\). The definition validates the resulting peak linear/angular speed and acceleration against the declared ceilings. The default `ConstantVelocity` profile remains constant-speed in the open interval and has a velocity discontinuity at its boundaries; it does not claim a finite start/stop acceleration bound.

## API sketch

~~~csharp
var definition = new PrescribedKinematicMotionDefinition(
    definitionId: "actor.walk-to-marker.v1",
    actorId: "actor:lenny",
    startPose: currentAchievedPose,
    destinationPose: destination,
    startTimeSeconds: 12d,
    durationSeconds: 2d,
    maximumLinearSpeed: 2d,
    maximumAngularSpeed: 1d);

var requested = PrescribedKinematicMotion.Evaluate(
    definition,
    logicalTimeSeconds: 13d,
    stepIndex: 60);

// The world/host decides what was actually achieved.
// It supplies that committed snapshot; Physics never fabricates it.
var commit = PrescribedKinematicMotion.Commit(
    definition,
    requested,
    achievedState,
    positionToleranceMeters: 0.01d,
    angularToleranceRadians: 0.02d);
~~~

A sample is a requested state. It is not automatically committed and is not itself proof of arrival. The commit operation verifies actor, definition identity/version, step, and logical time, then compares the supplied achieved pose with the requested pose. IsArrived is true only for the destination sample and when both supplied tolerances are met.

Cancellation between steps is represented by not committing another requested sample. Since RigidBodyState is immutable, the last committed snapshot remains unchanged and authoritative.

## Explicit limitations and remaining acceptance work

- `EvaluateFixedStep` derives logical time from the step index, fixed timestep, and total step count. The host still owns scheduling, monotonic ordering, and authoritative world commits; there is no persistent world-step scheduler in this API.
- The typed MicroBundle adapter is implemented and tested, but it does not provide a universal service locator or a host-specific runtime bridge. The host must resolve the loaded bundle instance and explicitly bind its provider contract.
- The default `ConstantVelocity` profile has a boundary velocity discontinuity. Select `QuinticSmoothStep` when finite start/stop acceleration is required; its speed and acceleration ceilings are validated at definition construction.
- The commit validator pairs a request with an achieved snapshot; it is not a collision solver or a world-step transaction coordinator.
- The host must enforce commit ordering and retain the last committed state on interruption.
- No articulated limb motion, gait, balance, contact, collision avoidance, friction, obstacle response, or environmental resistance is provided.
- A concrete host integration sample is not yet implemented. A static HTML page cannot directly invoke this C# assembly without an explicit runtime/host bridge.

Focused NUnit tests cover both motion profiles, analytical translation/orientation samples, stationary motion, speed/acceleration-limit rejection, deterministic replay, render-sampling independence, fixed-step schedule validation, requested-versus-achieved separation, arrival tolerances, forged-sample rejection, and immutable interruption state. Separate MicroBundle tests verify provider metadata, typed contract exposure, fixed-step evaluation, and commit validation.

## Related contracts

- [Motion-driven interaction pipeline](MOTION_DRIVEN_INTERACTION_PIPELINE.md)
- [Articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md)
- [Determinism](DETERMINISM.md)
