# Galilean Frames: the Cannonball on a Moving Train

**Status:** implemented mathematical primitive with focused tests. This is a reference-frame lesson, not a complete cannon/vehicle experiment or a relativistic model.

## The question

A train moves steadily at 20 m/s. A passenger launches a ball straight upward at 8 m/s relative to the train. Ignoring air resistance, what does a passenger see—and what does a person standing beside the track see?

At (t=1\) s, the ball's train-frame position is ((0,0,1.5)\) m in this test fixture. The corresponding ground-frame state is at ((20,0,1.5)\) m, moving at ((20,0,8)\) m/s. The ball's vertical motion is unchanged; the ground observer also sees the train's horizontal motion.

## The transform

For parallel axes and constant relative frame velocity (\mathbf{u}\), with the moving-frame origin at world position (\mathbf{o}_0\) at (t=0\):

\[
\mathbf{x}_{world}(t)=\mathbf{o}_0+\mathbf{x}_{moving}(t)+\mathbf{u}t
\]

\[
\mathbf{v}_{world}(t)=\mathbf{v}_{moving}(t)+\mathbf{u}
\]

The inverse subtracts the same translation and velocity. This is a classical Galilean transform: it assumes non-relativistic speeds and parallel, non-rotating axes. It does not model train acceleration, aerodynamic forces, launch mechanics, or prove that the chosen frames are inertial.

## Executable evidence

- Implementation: [GalileanFrameTransform.cs](../Physics/Math/GalileanFrameTransform.cs)
- Tests: [GalileanFrameTransformTests.cs](../tests/TheSingularityWorkshop.Physics.Tests/GalileanFrameTransformTests.cs)

The tests check the moving-train vertical-launch example, round-trip inversion with a nonzero origin, and invalid state rejection. These tests validate this transform contract; they are not an external measurement or a complete trajectory simulation.

## Next experiment

Compose the transform with the existing [recorded projectile trajectory](experiments/projectile-trajectory.md): preserve one trajectory in the train frame, derive a second ground-frame view, and compare both using the same logical timestamps. Then add the moving platform's own state only when acceleration is explicitly supported. A cannon fired forward from the train can be a separate variant that changes one declared input.

