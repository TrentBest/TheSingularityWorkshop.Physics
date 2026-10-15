# Hydrodynamics: 2D shallow-water basin (first increment)

This increment introduces a renderer-independent, depth-averaged two-dimensional shallow-water model on a regular grid. It advances water depth and depth-integrated horizontal momentum using conservative Rusanov interface fluxes. All four outer boundaries are impermeable walls. The initial domain is a flat bed with strictly positive water depth in every cell.

## State and units

- Depth \(h\): metres.
- Depth-integrated momenta \(hu, hv\): metres squared per second.
- Cell dimensions: metres; time step: seconds; gravity: metres per second squared.
- Water volume diagnostic: sum of cell depth multiplied by cell area, in cubic metres.
- Flux update: conservative finite-volume form on a uniform rectangular grid.
- Stability: fixed time step must satisfy the implementation's 2D CFL rate bound, \(\Delta t \sum_d (|u_d|+\sqrt{gh})/\Delta x_d \le 0.45\).

## Boundaries and numerical guardrails

The outer faces have zero normal mass flux and hydrostatic pressure flux, representing an impermeable wall. Interior face fluxes use local Lax-Friedrichs/Rusanov dissipation. The model validates its CFL bound before each step and computes the candidate state before committing it; non-finite values or non-positive depth reject the step without replacing the current state.

## Demonstrated by tests

- Uniform still water remains still.
- A localized depth perturbation evolves.
- Closed-wall volume remains constant within floating-point tolerance.
- Depth remains positive and finite for the tested run.
- Unsafe CFL input is rejected.
- Snapshot arrays cannot mutate solver state.

## Explicit limitations

This is an initial shallow-water core, not a complete world simulator. It currently assumes a flat bed and positive depth everywhere. It does not yet implement variable terrain, dry cells/shorelines, friction, buildings, obstacles, boats, open boundaries, breaking waves, spray, or full three-dimensional fluid dynamics. The Rusanov flux is deliberately robust and diffusive; later accuracy work should add grid-refinement and convergence evidence before making stronger fidelity claims.
