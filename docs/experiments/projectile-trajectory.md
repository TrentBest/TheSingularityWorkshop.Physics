# Projectile trajectory: first recorded 3D experiment

This page documents one deliberately narrow, reproducible run of `ProjectileTrajectoryExperiment`. The chart is the X/Z side projection of a three-dimensional state; the Y coordinate remains part of the solver state but is zero for this example.

## Declared experiment

| Input | Value |
|---|---:|
| Initial position | (0, 0, 0) m |
| Initial velocity | (10, 0, 12) m/s |
| Constant acceleration | (0, 0, -9.81) m/s² |
| Coordinate frame | world, Z-up |
| Fixed timestep | 0.25 s |
| Completed steps | 8 |
| Recorded samples | 9, including the initial state |
| Simulated duration | 2.0 s |

The plotted fixture uses the constant-acceleration equations implemented by the existing solver:

- Position: **p(t) = p₀ + v₀t + ½at²**
- Velocity: **v(t) = v₀ + at**

The nine sample timestamps are 0, 0.25, 0.50, 0.75, 1.00, 1.25, 1.50, 1.75, and 2.00 seconds. Markers represent sampled logical states; the connecting line is a visual guide between samples.

![X/Z projection of the recorded-example trajectory](projectile-trajectory.svg)

## Evidence and limitations

- [Experiment implementation](../../Physics/Experiments/ProjectileTrajectoryExperiment.cs)
- [Acceptance tests](../../tests/TheSingularityWorkshop.Physics.Tests/ProjectileTrajectoryExperimentTests.cs)
- [Living theory and evidence contract](../LIVING_THEORY.md)

The acceptance tests compare each sample with the analytical solution and check repeatability, invalid inputs, and serialization. This illustration is a declared analytical example, not a claim that the SVG is automatically emitted by the runtime yet. The next documentation step should render directly from a serialized run record so the visual and recorded samples share a single generated source.

This model does **not** implement aerodynamic drag, spin, a launch mechanism, collisions, terrain intersection, or impact physics. It should not be interpreted as evidence of real-world flight accuracy.
