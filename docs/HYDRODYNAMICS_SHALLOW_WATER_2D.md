# Hydrodynamics: shallow-water basin and bathymetry

The flat-bed prototype and the terrain-aware increment are renderer-independent, depth-averaged two-dimensional shallow-water models on a regular rectangular grid. They advance water depth and depth-integrated horizontal momentum with finite-volume Rusanov fluxes and impermeable outer walls.

## Bathymetric increment

`BathymetricShallowWaterBasin2D` accepts a cell-centered bed-elevation field as well as initial water depth. It reconstructs the left and right water depths at each face against the higher of the two bed elevations, evaluates the Rusanov flux on those reconstructed states, then applies side-specific hydrostatic pressure corrections. This hydrostatic reconstruction is designed to preserve a still lake with a constant free-surface elevation over uneven terrain.

## State and units

- Bed elevation and free-surface elevation: metres.
- Water depth (h): metres; depth-integrated momenta (hu,hv): square metres per second.
- Cell dimensions: metres; time step: seconds; gravity: metres per second squared.
- Water volume: sum of cell depths multiplied by cell area, in cubic metres.
- Fixed-step stability guard: (Delta t sum_d (|u_d|+sqrt{gh})/Delta x_d le 0.45).

## Numerical guardrails and current evidence

- Outer faces impose zero normal mass flux and hydrostatic pressure flux.
- The terrain-aware model rejects non-finite bed elevations and invalid initial depths.
- Each step builds a candidate state and commits only after all cells pass finite/positive-depth checks.
- Automated tests cover still-water equilibrium over an uneven bed, evolution of a free-surface disturbance over terrain, closed-basin water-volume conservation, and defensive snapshots.
- The flat-bed solver's tests additionally cover disturbance propagation, repeatability, and CFL rejection.

## Wet/dry shoreline increment

The terrain-aware model now accepts zero depth for dry cells. Dry-cell velocity samples return zero, dry cells carry no momentum, and hydrostatic reconstruction supports a still free surface terminating against bed elevations above that surface. Automated coverage checks that a lake at rest remains balanced with a dry shoreline, and that a single controlled time step advances water into an adjacent dry cell while preserving non-negative finite depths and total water volume. This is an initial wetting-front sanity check, not yet a claim of validated multi-step wetting fronts under energetic transients.

## Explicit limitations

The terrain-aware model does not yet implement friction, obstacles, buildings, boats, moving terrain, erosion, sediment transport, open boundaries, breaking-wave physics, or three-dimensional flow. The terrain field is fixed during a run. Rusanov fluxes are robust but diffusive; grid-refinement and convergence evidence is still required before making stronger accuracy claims. This increment is a coupled numerical foundation, not a complete world or ocean simulator.
