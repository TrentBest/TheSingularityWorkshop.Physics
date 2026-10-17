# Moving-train projectile: one motion, two observers

**Status:** paired train/ground trajectory experiment implemented and tested. The committed single-frame projectile SVG is still the existing experiment; a paired comparison SVG is the next visualization step.

## Question

A train moves at a constant 20 m/s. A ball is launched vertically at 8 m/s relative to the train. Ignoring air resistance, compare the description from a passenger with the description from someone beside the track.

The train-frame horizontal position stays at zero. The ground-frame horizontal position advances by 20 metres each second. The vertical position and velocity are equal in both frames at every shared timestamp.

## What runs

- [Paired experiment](../Physics/Experiments/MovingTrainProjectileExperiment.cs) runs the existing constant-acceleration trajectory once in train coordinates and derives the ground state for every sample using the Galilean transform.
- [Frame transform](../Physics/Math/GalileanFrameTransform.cs) converts positions and velocities between parallel axes with constant relative velocity.
- [Acceptance tests](../tests/TheSingularityWorkshop.Physics.Tests/MovingTrainProjectileExperimentTests.cs) check matching logical timestamps, horizontal train motion, preserved vertical motion, and a non-zero train origin.
- [Transform lesson](GALILEAN_FRAME_TRANSFORM.md) explains the equations and their assumptions.

The ground-frame trajectory is not independently re-simulated. That avoids two numerical integrations drifting apart and makes the relationship between the observations explicit.

## Equations

For train velocity \(\mathbf{u}\), train-frame position \(\mathbf{x}_{train}(t)\), and the ground position of the train-frame origin at the epoch \(\mathbf{o}_0\):

\[
\mathbf{x}_{ground}(t)=\mathbf{o}_0+\mathbf{x}_{train}(t)+\mathbf{u}t
\]

\[
\mathbf{v}_{ground}(t)=\mathbf{v}_{train}(t)+\mathbf{u}
\]

## Scope and next step

This is a classical, non-relativistic model. The frames have parallel axes and constant relative velocity. It does not model train acceleration, drag, launch mechanics, collision, or terrain.

Next: produce a versioned paired run record and SVG whose two plotted series are both derived from the same actual C# result. The illustration should expose shared timestamps and show the train and ground coordinate labels rather than presenting the two curves as unrelated trajectories.
