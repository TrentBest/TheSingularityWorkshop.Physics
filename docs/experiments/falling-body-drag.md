# Falling body: what air resistance changes

This experiment compares the same one-kilogram body under the same gravity, starting height, and initial velocity. The only model switch is quadratic aerodynamic drag.

The height and velocity SVG is generated from the recorded run JSON by the renderer command below. CI executes the C# model first, then renders and parses the resulting SVG; no separate physics implementation exists in the renderer.

## Declared inputs

| Quantity | Value |
|---|---:|
| Initial height | 100 m |
| Initial vertical velocity | 0 m/s |
| Mass | 1 kg |
| Gravitational acceleration | 9.81 m/s² downward |
| Air density | 1.225 kg/m³ |
| Drag coefficient | 0.47 (constant) |
| Reference area | 0.05 m² |
| Integration timestep | 0.01 s |
| Duration | 1 s |
| Recorded states per case | 101 |

Upward is positive. In still air, the one-dimensional drag force is

\[
F_d=-\frac12\rho C_d A |v|v
\]

The model updates velocity from gravity plus drag, then updates height using the new velocity (semi-implicit Euler). The no-drag run uses the same integration schedule and differs only by disabling the drag term.

## Reproduce the experiment

From the repository root:

```bash
dotnet run --project tools/FallingBodyDragRunGenerator/FallingBodyDragRunGenerator.csproj --configuration Release -- /tmp/falling-body-drag.json
python3 tools/render_falling_body_drag.py /tmp/falling-body-drag.json /tmp/falling-body-drag.svg
```

The generator records every state from both C# runs and checks that drag reduces both the distance fallen and the downward speed for this declared scenario. The SVG renderer consumes that JSON; it does not implement a second physics solver.

## What this establishes—and what it does not

This is a deterministic numerical demonstration of the expected qualitative effect of drag, not empirical validation of a real object's trajectory. It is one-dimensional, assumes still air and a constant drag coefficient/reference area, and uses a fixed-step numerical integrator. Ground contact is not resolved; the scenario is intentionally short enough that both trajectories remain above ground. Lift, rotation, changing air density, wakes, and wind are outside this model.

The drag coefficient and area are scenario inputs, not values inferred from geometry. The plotted lines connect recorded samples as visual guides; no intermediate state is presented as a solver output.
