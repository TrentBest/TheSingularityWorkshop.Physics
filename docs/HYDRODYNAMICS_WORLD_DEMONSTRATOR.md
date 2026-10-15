# Hydrodynamics World Demonstrator

**Goal:** build a renderer-independent physical world that can be shown as a vast bathtub first, then grow into a coastal/terrain/buildings scene. The visible signature is persistent, interacting waves: waves propagate, reflect, cross, encounter obstacles and shorelines, move boats, and respond to disturbances. Water must be simulation state, not a scrolling shader that pretends to be fluid dynamics.

This is a design and implementation contract, not a claim that the simulator already exists. The development branch currently has no hydrodynamics solver.

## The experience we are building toward

The first public-facing scene should be understandable in seconds and rewarding to explore:

- A large enclosed basin with water, a visible scale, a camera that can pull far back and zoom in, and a boat whose motion can be inspected.
- A user-triggered disturbance that launches a wave train. Watch it travel, reach the basin wall, reflect, cross other waves, and move the boat.
- A height/velocity view and a small live measurement panel: simulation time, water level, wave height, boat position/attitude, and conservation/error diagnostics.
- Then expand the same world to a variable bottom, a sloped beach, an island, a breakwater, and buildings that occupy space and alter flow. Waves remain the star in every scene.
- Controls change initial conditions and physical parameters; a replayable run record captures them so two experiments can be compared.

The bathtub is a controlled laboratory, not a toy-only model. Its closed walls make mass balance and wave reflection testable before adding open boundaries, terrain, and complex scenes.

## Physical model progression

### H0 — Analytical wave reference

Build a small, one-dimensional linear shallow-water/wave reference case with known wave speed and reflecting boundary behavior. This is a verification reference, not the final world solver. It establishes sign conventions, units, expected travel time, reflection timing, and a measurable error baseline.

### H1 — Two-dimensional shallow-water solver

Implement a bounded, renderer-independent, depth-averaged free-surface model on a regular grid. The conservative state per cell is water depth and depth-integrated horizontal momentum (or an explicitly documented equivalent). Bed elevation is separate static geometry. Use a finite-volume flux formulation, explicit boundary conditions, and a declared numerical flux/limiter.

Initial regime:
- incompressible water represented by depth-averaged shallow-water equations;
- hydrostatic pressure approximation;
- free surface and bottom topography;
- initially still water with a localized disturbance;
- closed basin boundaries for the first verification scenario;
- no claim to resolve vertical wave structure, breaking, spray, turbulence, or arbitrary 3D flow.

Required controls and checks:
- positive water depth and explicit dry-cell/wet-dry policy;
- CFL-based timestep bound and failure/diagnostic behavior when the requested step is unsafe;
- stable, deterministic update order;
- water-volume accounting (including declared boundary fluxes);
- momentum/energy diagnostics appropriate to the discretization, including the fact that numerical dissipation is possible;
- convergence checks against the analytical reference as the grid is refined;
- reproducible input, parameters, model version, and sampled outputs.

### H2 — Obstacles, terrain, and shorelines

Represent bottom elevation and solid occupancy as explicit fields, not renderer meshes queried as hidden truth. Add submerged terrain, sloped shorelines, islands, walls, and buildings in small steps. State how solid boundaries block normal flow, how partially wet cells are represented, and how shoreline cells transition between wet and dry. Test reflection, flow around obstacles, and water-volume behavior at each step.

Buildings are obstacles to the water model in this stage; they are not yet structural models. Flooding inside a building, porous materials, debris, and building failure are later coupled capabilities.

### H3 — Boats and fluid/body coupling

Begin with a deliberately bounded floating-body model, not a promise of arbitrary boat hydrodynamics:
- stable body identity, mass, footprint, draft/displacement assumptions, and center of buoyancy;
- buoyancy derived from displaced water within the supported approximation;
- wave sampling at the hull footprint rather than a single visually convenient point;
- heave and pitch/roll only when their supported equations and numerical behavior are specified;
- momentum exchange between boat and water only through explicit, tested coupling terms;
- documented limits for grounding, capsizing, slamming, propulsion, and complex hull shapes.

First prove a passive boat responds consistently to a passing wave. Add boat-generated wakes only after a measured momentum/energy exchange path exists. Do not animate the boat independently of the simulated water.

### H4 — Open world and multiple scales

Only after the bounded basin is verified, extend to open boundaries, larger terrain, river/coastal flow, multiple disturbances, moving boats, and local refinement. For distant regions, coarse representations may be used only with declared conservative handoffs and measurable error. The renderer may use level of detail; it must not silently become the authoritative water solver.

## Architecture boundary

- **Physics core:** state, equations, discretization, timestep limits, boundary conditions, conservation diagnostics, and observations. No renderer or Unity dependency.
- **World/scene description:** bathymetry, solid masks, basin boundaries, and object/material identifiers supplied as explicit inputs.
- **Scheduling/composition:** later FSM_API/FSM_COS adapters can schedule simulation groups, but FSM_API is not a numerical fluid solver and Physics must not depend upward on FSM_COS.
- **Renderer:** visualizes actual water surface/velocity/obstacle state and diagnostic overlays. It does not invent wave propagation, buoyancy, or collisions.
- **Experiments:** versioned input and run record make a scene reproducible, testable, and explainable.

A scene should be capable of running headlessly in tests and exporting sampled state for a renderer or plotter.

## Validation gates

Do not call a milestone complete because it looks convincing. Every solver increment needs:

1. explicit units, state variables, supported regime, assumptions, and model version;
2. analytical or independently computed reference cases;
3. tests for invalid parameters, dry/wet transitions, boundaries, and timestep limits;
4. deterministic repeat runs under the same supported runtime and inputs;
5. water-volume accounting and other applicable conservation/error diagnostics;
6. grid-refinement evidence showing how error changes, not only one passing example;
7. a clear list of phenomena omitted from the model.

The first milestone should pass a closed-basin wave-propagation/reflection test and publish enough sampled values to plot water height over space and time. The first visually compelling demo comes after that verified core is usable.

## Demo acceptance sequence

- [ ] **A: Basin:** define dimensions, grid spacing, still-water depth, gravity, boundaries, and deterministic initial conditions.
- [ ] **B: Waves:** launch a disturbance; observe propagation and reflection; compare against the analytical reference.
- [ ] **C: Instruments:** expose water-level samples, volume balance, wave speed, timestep/CFL status, and repeat-run comparison.
- [ ] **D: Terrain:** add a sloped bottom, shallow region, island, and wall; verify behavior and accounting.
- [ ] **E: Buildings:** add explicit solid obstacles and inspect how waves/flow respond.
- [ ] **F: Boats:** add a floating body and verify its response to passing waves with declared coupling limits.
- [ ] **G: Scale:** expand the domain and add multiple wave sources, boundaries, and eventually local refinement.
- [ ] **H: Living explanation:** show the governing assumptions, live measurements, and run record beside the scene.

## What this does not claim

This plan is not yet a 3D Navier–Stokes solver, a breaking-wave simulator, a turbulence model, or a production naval architecture package. Depth-averaged shallow-water equations are a deliberate first regime for long waves, inundation, and large scenes. Deep-water dispersion, steep/breaking waves, spray, detailed hull flow, and full 3D effects require different or extended models and must be added explicitly rather than implied by graphics.

---

[Physics Domain Map](PHYSICS_DOMAIN_MAP.md) · [Physics roadmap](ROADMAP.md) · [Theory and evidence](THEORY.md)
