# Galilean Frames: the Cannonball on a Moving Train

**Status:** implemented mathematical primitive with focused tests. This is a reference-frame lesson, not a complete cannon/vehicle experiment or a relativistic model.

## The question

A train moves steadily at 20 m/s. A passenger launches a ball straight upward at 8 m/s relative to the train. Ignoring air resistance, what does a passenger see—and what does a person standing beside the track see?

The isolated transform unit test uses a supplied state at t = 1 s with position (0, 0, 1.5) m and velocity (0, 0, 8) m/s. Those are test inputs to the coordinate transform, not a gravity-consistent trajectory. The paired experiment separately launches at 12 m/s under −9.81 m/s² acceleration and derives the ground view at every sampled time.

![Living moving-train projectile: animated markers follow paired train and ground trajectories generated from the C# run record](experiments/moving-train-projectile.svg)

*This is a run-backed living image. CI executes the paired C# experiment, checks the Galilean frame relationship, and validates each plotted train/ground coordinate against the actual serialized samples.*

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

The paired experiment now exists: [MovingTrainProjectileExperiment.cs](../Physics/Experiments/MovingTrainProjectileExperiment.cs) integrates one train-frame trajectory and transforms every state into the ground frame at the same logical timestamp. [Its focused tests](../tests/TheSingularityWorkshop.Physics.Tests/MovingTrainProjectileExperimentTests.cs) check preserved vertical motion, horizontal train motion, and a non-zero frame origin.

The paired states now have a versioned JSON run record: [MovingTrainProjectileRunRecord.cs](../Physics/Experiments/MovingTrainProjectileRunRecord.cs). Its focused test checks the actual paired samples, SI units, schema/model versions, and repeatable JSON serialization. A paired SVG now explains the two observations; the remaining visualization step is to generate it from this record and validate it against the recorded C# output. The lesson contract is documented in [moving-train projectile](experiments/moving-train-projectile.md). A forward-fired cannonball can then be a separate declared-input variant.
