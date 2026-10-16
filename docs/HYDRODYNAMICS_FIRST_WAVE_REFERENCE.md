# Hydrodynamics: first executable wave reference


This milestone adds an executable, renderer-independent one-dimensional linear wave-equation basin. It is deliberately small: it makes a disturbance evolve over time, supports repeatable samples, and uses mirrored ghost cells to model reflecting walls. It is an analytical/numerical reference step toward the larger world demonstrator, not the 2D shallow-water solver.

## Model and units

- Surface elevation: metres.
- Cell width and wave speed: metres and metres per second.
- Fixed time step and simulation clock: seconds.
- Governing reference equation: \(\partial^2\eta/\partial t^2 = c^2\partial^2\eta/\partial x^2\).
- Discretization: centered explicit second-order update on a uniform one-dimensional grid.
- Closed-wall condition: zero normal derivative, implemented with mirrored ghost values at both ends.
- Stability gate: Courant number \(C=c\Delta t/\Delta x\) must be at most 1.
- Initial velocity is zero; the first previous-time level is initialized with the second-order Taylor approximation.

The solver exposes the current elevation profile, individual cell samples, step count, and simulation time. The snapshot API returns a copy so renderers and plotting tools cannot mutate solver state.

## Verified behavior in this increment

Tests cover deterministic replay from identical inputs, nontrivial evolution of a localized pulse, long-running finite values with reflecting boundaries, rejection of an unsafe Courant number, rejection of non-finite input, and defensive snapshots.

## Explicit limitations

This is not yet a conservative finite-volume fluid solver: it does not model water depth, horizontal momentum, bathymetry, wet/dry cells, volume balance, boats, obstacles, 2D interactions, or nonlinear shallow-water effects. Those are subsequent milestones; visuals must not imply they already exist.
