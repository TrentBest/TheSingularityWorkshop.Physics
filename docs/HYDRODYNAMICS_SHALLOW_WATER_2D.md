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

## Rules of the physical model

1. **Gravity-driven redistribution:** for a non-static water state, gravity and pressure gradients must be allowed to develop flow. Terrain elevation matters, but flow direction is not decided by bed slope alone: the relevant driving state includes the free-surface elevation (bed plus water depth), momentum, and boundary conditions.
2. **Water reaches lower terrain when the state permits it:** an initially wet upper region beside a lower, dry region must be able to wet the lower region over time. A level free surface at rest over uneven terrain is the essential counterexample: the solver must not invent flow downhill when hydrostatic pressure balances the bed.
3. **Conservation:** in a closed basin with impermeable walls and no sources or sinks, internal face fluxes redistribute water but must not create or destroy total water volume beyond documented floating-point tolerance.
4. **Wet/dry validity:** depth must remain finite and non-negative; dry cells have zero sampled depth-averaged velocity and zero stored momentum.
5. **Bulk representation must preserve the rules:** rain and runoff can later be introduced as aggregated cell/face source terms. A cell-based or GPU implementation may approximate the collective effect of many drops, but must not change the governing rules simply to make a visual effect. Source volume, momentum input, infiltration, and runoff must be separately accounted for when implemented.
6. **Normals are evidence about geometry, not a complete flow solver:** terrain/surface normals can describe local orientation and support impact/runoff models. They do not replace free-surface gradients, pressure, momentum, or material behavior. A raindrop's impact response should be a source term in the bulk model, not an invented per-drop trajectory when individual drops are not being represented.

These are behavioral invariants, not a claim that the current solver already models rain impacts, infiltration, splashes, or all runoff regimes.

## Numerical guardrails and current evidence

- Outer faces impose zero normal mass flux and hydrostatic pressure flux.
- The terrain-aware model rejects non-finite bed elevations and invalid initial depths.
- Each step builds a candidate state and commits only after all cells pass finite, non-negative-depth checks.
- Automated tests cover still-water equilibrium over an uneven bed, evolution of a free-surface disturbance over terrain, closed-basin water-volume conservation, defensive snapshots, still water against a dry shoreline, a one-step wetting front, and multi-step downhill wetting over an inclined bed.
- The flat-bed solver's tests additionally cover disturbance propagation, repeatability, and CFL rejection.

## Wet/dry shoreline increment

The terrain-aware model accepts zero depth for dry cells. Dry-cell velocity samples return zero, dry cells carry no momentum, and hydrostatic reconstruction supports a still free surface terminating against bed elevations above that surface. Tests check that a lake at rest remains balanced with a dry shoreline, and that water can advance into initially dry cells while keeping depths finite and non-negative and conserving total water volume. The inclined-bed test checks that a wet upper region loses water while an initially dry, lower-elevation cell becomes wet across multiple time steps.

This is initial wetting-front evidence, not yet a claim of validated wetting fronts under energetic transients, realistic rainfall, or a full river/lake model.

## Explicit limitations

The terrain-aware model does not yet implement rain or other water source terms, infiltration, friction, obstacles, buildings, boats, moving terrain, erosion, sediment transport, open boundaries, breaking-wave physics, or three-dimensional flow. The terrain field is fixed during a run. Rusanov fluxes are robust but diffusive; grid-refinement and convergence evidence is still required before making stronger accuracy claims. This increment is a coupled numerical foundation, not a complete world or ocean simulator.
