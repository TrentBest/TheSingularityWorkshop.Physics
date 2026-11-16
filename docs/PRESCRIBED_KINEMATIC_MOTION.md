# Prescribed Kinematic Motion

**Status:** implemented bounded absolute-logical-time evaluation; commit validation is available. A fixed-step evaluation entry point is available; a MicroBundle/host adapter remains integration work.

## Ownership

A prescribed trajectory is a kinematic request, not a force-driven physical response.

- The Experience/controller owns intent, destination, and cancellation.
- A host adapter creates a versioned PrescribedKinematicMotionDefinition.
- Physics evaluates the trajectory at an explicit logical time and step.
- The host/world supplies the separately achieved RigidBodyState.
- PrescribedKinematicMotion.Commit pairs the request with that achieved snapshot and reports arrival only when the committed snapshot is within the supplied destination tolerances.
- Renderer and Visualization consume achieved observations; they do not calculate root motion.

Physics core has no dependency on Experiences, Gestures, Renderer, Visualization, Foundry, FSM_COS, or a host framework.

## Current mathematical contract

The current bounded profile is linear translation and shortest-arc quaternion interpolation:

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

The profile has constant velocity inside the interval and a derivative discontinuity at its boundaries. It therefore does **not** claim a finite start/stop acceleration bound. A trajectory requiring bounded acceleration or continuous start/stop velocity needs a separately declared profile and must not be represented by this profile.

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

- EvaluateFixedStep derives logical time from the step index, fixed timestep, and total step count. The host still owns scheduling and commit ordering; there is no persistent world-step scheduler in this API.
- The current profile has a boundary derivative discontinuity and no finite acceleration bound. Do not use it for motion requiring bounded acceleration.
- The commit validator pairs a request with an achieved snapshot; it is not a collision solver or a world-step transaction coordinator.
- The host must enforce commit ordering and retain the last committed state on interruption.
- No articulated limb motion, gait, balance, contact, collision avoidance, friction, obstacle response, or environmental resistance is provided.
- A MicroBundle-facing adapter and executable host sample are not yet implemented. A static HTML page cannot directly invoke this C# assembly without an explicit runtime/host bridge.

Focused tests cover analytical translation/orientation samples, stationary motion, speed-limit rejection, deterministic replay, render-sampling independence, requested-versus-achieved separation, arrival tolerances, and immutable interruption state.

## Related contracts

- [Motion-driven interaction pipeline](MOTION_DRIVEN_INTERACTION_PIPELINE.md)
- [Articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md)
- [Determinism](DETERMINISM.md)
