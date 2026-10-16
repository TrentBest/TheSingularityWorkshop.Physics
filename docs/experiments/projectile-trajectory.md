# Projectile trajectory: first recorded 3D experiment

This page documents one deliberately narrow run of `ProjectileTrajectoryExperiment`. The X/Z chart is generated from the serialized sample states, not from a second implementation of the physics equations. The Y coordinate remains part of the recorded state and is zero for this example.

<!-- BEGIN GENERATED RUN SUMMARY -->
## Declared experiment

| Input / outcome | Recorded value |
|---|---:|
| Model identity | `kinematics.constant-acceleration-projectile` (model version 1; schema 1) |
| Initial position | (0, 0, 0) m |
| Initial velocity | (10, 0, 12) m/s |
| Constant acceleration | (0, 0, -9.81) m/s^2 |
| Coordinate frame | world, Z-up |
| Fixed timestep | 0.25 s |
| Completed steps | 8 |
| Recorded samples | 9, including the initial state |
| Simulated duration | 2 s |
| Final position | (20, 0, 4.38) m |
| Final velocity | (10, 0, -7.62) m/s |

<!-- Record-derived: model=kinematics.constant-acceleration-projectile; samples=9; duration=2 s. -->
<!-- END GENERATED RUN SUMMARY -->

The solver's constant-acceleration relationships are:

- Position: **p(t) = p₀ + v₀t + ½at²**
- Velocity: **v(t) = v₀ + at**

Markers correspond to the exact recorded sample states. The connecting line is only a visual guide between those samples.

![X/Z projection generated from the serialized trajectory run record](projectile-trajectory.svg)

## Reproduce or vary the experiment

The fixture at [projectile-trajectory.run.json](projectile-trajectory.run.json) is a checked-in run record for this example. The renderer reads positions and timestamps from the record; it does not recalculate the trajectory.

From the repository root, run the C# experiment with its documented default inputs:

```bash
dotnet run --project tools/ProjectileRunRecordGenerator/ProjectileRunRecordGenerator.csproj --configuration Release -- /tmp/projectile-trajectory.json
```

To try a different, bounded experiment, provide any of the following options. Values use invariant dot-decimal notation:

| Option | Allowed range | Default |
|---|---:|---:|
| `--delta-time` | 0.001–1 second | 0.25 |
| `--steps` | 1–400 | 8 |
| `--vx`, `--vy`, `--vz` | −1000–1000 m/s | 10, 0, 12 |
| `--ax`, `--ay`, `--az` | −100–100 m/s² | 0, 0, −9.81 |

For example:

```bash
dotnet run --project tools/ProjectileRunRecordGenerator/ProjectileRunRecordGenerator.csproj --configuration Release -- /tmp/projectile-custom.json --delta-time=0.1 --steps=20 --vx=3 --vy=1 --vz=4 --ax=0.5 --ay=0 --az=-9.81
python3 tools/render_projectile_trajectory.py /tmp/projectile-custom.json /tmp/projectile-custom.svg
```

This writes the actual C# experiment output to JSON and renders its recorded X/Z samples as SVG. The optional `--check` flag compares an existing JSON file against a fresh run using the same options, providing a deterministic reproducibility check. The build exercises a non-default input set, verifies its inputs and sample count, and repeats it in `--check` mode.

The checked-in SVG and generated Markdown summary remain the default example. CI regenerates the SVG from the actual default C# run and checks both the illustration and summary for drift.

## Evidence and limitations

- [Experiment implementation](../../Physics/Experiments/ProjectileTrajectoryExperiment.cs)
- [Acceptance tests](../../tests/TheSingularityWorkshop.Physics.Tests/ProjectileTrajectoryExperimentTests.cs)
- [Run-record generator and bounded inputs](../../tools/ProjectileRunRecordGenerator/Program.cs)
- [SVG renderer source](../../tools/render_projectile_trajectory.py)
- [Living theory and evidence contract](../LIVING_THEORY.md)

The acceptance tests compare each solver sample with the analytical solution and check repeatability, invalid inputs, and serialization. The fixture represents a deterministic analytical example, not external measurements.

This model does **not** implement aerodynamic drag, spin, a launch mechanism, collisions, terrain intersection, or impact physics. It should not be interpreted as evidence of real-world flight accuracy.
