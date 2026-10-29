# Surface Combustion: Materials, Elements, and the Flame System

**Status:** implementation contract and staged work plan. This document does not claim a complete combustion solver exists.

**Owner:** `TheSingularityWorkshop.Physics` owns physical state, material-response models, reaction accounting, and validation. MicroBundle composition discovers and arbitrates providers. A presentation client may visualize recorded state but must not authoritatively ignite, consume fuel, or add heat.

Related: [Physics + Math Domain Expansion](PHYSICS_MATH_CHEMISTRY_COMBUSTION.md) · [Physics capability catalog](PHYSICS_CAPABILITY_CATALOG.md) · [deterministic math and random](DETERMINISTIC_MATH_AND_RANDOM.md) · [LUT policy](PHYSICS_MATH_LUTS.md).

## 1. The central behavior

A lighter contributes a reusable flame source. A candle contributes material, geometry, and environmental state. The Physics model determines whether the flame transfers enough energy to ignite the candle's exposed wick/wax; the same model can act on paper, wood, or another material when its data and reaction capabilities support that case.

There must be no candle-specific imperative such as `LightCandle()` that bypasses the physical model. Interactions are requests to the model; ignition, fuel consumption, heat transfer, and emitted products are consequences of a supported calculation. If the required material data or reaction model is unavailable, return an explicit unsupported/insufficient-evidence result instead of inventing a plausible outcome.

This is a physical model with visible output derived from it, not a flame shader that happens to look convincing.

## 2. Element and material data are first-class inputs

Element identities and material properties are related but not interchangeable:

- An **element record** identifies the chemical element by atomic number and symbol and carries name, source, source version, and licensing/provenance metadata.
- An **isotope record** identifies a specific nuclide and its mass data; isotope mass is not the same quantity as an element's standard atomic weight.
- A **material record** describes a material or mixture. It can refer to elemental/chemical constituents, but bulk properties such as density, heat capacity, thermal conductivity, emissivity, moisture, and ignition/reaction behavior belong to the material/model evidence—not to a generic element identity.
- A **reaction profile** declares the participating species, balanced stoichiometry, reaction conditions/range, heat release, source, version, uncertainty, and applicability limits.
- A **physical model** consumes these declared records. It does not silently infer missing properties from names such as `wood`, `wax`, or `steel`.

Use stable identifiers and retain source provenance down to the property or reaction value. Values may be measured, sourced, estimated, or illustrative; these evidence classes must remain visible. Illustrative values are useful for tests and teaching but must never be presented as measured physical truth.

The periodic table is a view over versioned element data, not a second hard-coded truth in WebPage. MicroBundles can distribute and compose datasets and specialized providers; the Physics NuGet contracts remain usable without adopting the MicroBundle host.

## 3. Surface fields and texture representation

The initial GPU-oriented design uses three logical scalar fields sampled on a surface grid:

1. **Fuel field:** remaining fuel mass per cell (kg), with a declared surface-cell area and material/profile identity.
2. **Oxidizer field:** available oxygen mass per cell (kg) in a declared near-surface control volume. A future volume/transport model must define the control-volume depth and exchanges with ambient air; a texture channel is not itself a physical unit.
3. **Thermal field:** thermal energy per cell (J) relative to a declared reference/state model. Derive temperature only when mass, heat capacity, phase assumptions, and state are available.

A GPU texture is a storage/transport encoding of these logical fields. It is not the domain contract. Precision, quantization, channel packing, grid dimensions, coordinate mapping, and upload/readback policy must be declared independently so CPU reference calculations can validate GPU results.

### Normals, gravity, and “up”

Each surface cell carries a normal in a declared coordinate frame. Normals describe local surface orientation and help determine exposure, directional flux, and how flame-source energy reaches a surface. Validate finite components and a documented normalization tolerance.

World gravity defines the up direction. Buoyant hot-gas transport tends upward relative to gravity; conduction follows temperature gradients and is not globally “upward.” Radiation depends on geometry and view factors. Wind/advection may transport heat and oxygen sideways or downward. Keep these mechanisms distinct, even if an early demonstrator deliberately supports only one.

## 4. Reaction and energy accounting

### Current executable reference slice

`SurfaceCombustionStepper.Step` now provides a deterministic CPU reference step over stable cell IDs. For each cell it accepts explicit parameters for maximum fuel-consumption rate, oxygen mass required per fuel mass, heat release per fuel mass, ignition-temperature threshold, lumped thermal capacity, and reference temperature. It can supply oxygen through explicitly declared ambient boundary conditions, then exchanges oxygen over declared graph connections before reaction. Blocked boundaries and edges supply/transport nothing; graph transport is limited to each source inventory. Reaction then caps fuel consumption by the rate limit, available fuel, and available oxygen. Results separately report external oxygen supplied and the internal transport residual. Fuel and oxygen are consumed together and declared heat release is added to the cell's thermal energy. Results include per-cell initial/final state, consumption, heat release, temperatures, totals, and an oxygen-transport residual.

This is intentionally a **bounded accounting model**, not validated material chemistry. The temperature-threshold/max-rate law is not Arrhenius kinetics; the graph exchange rule assumes equal oxygen control-volume sizes and is not airflow; thermal energy is lumped and does not transfer between cells. Ambient oxygen is now an explicit bounded mass-rate boundary condition, but it is not a model of pressure-driven airflow or oxygen diffusion through the atmosphere. A new `SurfaceReactionProfile` contract now validates declared integer stoichiometric equations against both elemental atom counts and an independent molar-mass balance, with source/version, evidence class, conditions, and explicit tolerance. Its methane-combustion fixture is explicitly illustrative. This is validated reaction metadata only: the current `SurfaceCombustionStepper` does not yet consume reaction profiles to determine runtime reaction extent or create product inventories. There is still no runtime reaction-product ledger, smoke/soot chemistry, flame-source transfer, or inter-cell heat propagation. Parameters must be explicit and their evidence/validity documented by the experiment.

For each reaction step, calculate the reaction extent from the declared rate law and available reactants, then cap it by every limiting reactant. Consume fuel and oxygen according to the selected reaction's declared stoichiometry; release heat from the declared reaction energy; account for products and losses in explicit terms when those ledgers are implemented.

The new `SurfaceReactionExtentCalculator` now applies a validated profile to a caller-supplied reactant mass inventory. It caps requested extent by each stoichiometric reactant, reports per-species reactant consumption and product generation, calculates declared heat release, and exposes a mass-balance residual. Missing reactant entries mean zero available mass; unknown inventory keys and negative/non-finite values are rejected. This is a pure single-reaction accounting operation: it does not update `SurfaceCombustionCellState`, persist product inventories between steps, select kinetics, or integrate time. The current illustrative methane fixture verifies accounting mechanics only, not the validity of a real material model.

`SurfaceReactionInventoryStepper.Apply` now composes that calculator with an immutable per-cell species inventory. It returns a new inventory after subtracting reactant masses and accumulating product masses, preserves unrelated species, caps extent against the cell's available reactants, and leaves the input inventory unchanged. Heat release is returned as part of the reaction result but is not applied to a thermal state here. This creates a tested persistent-product accounting primitive; it still does not select reaction kinetics, advance time, model transport of products between cells, or integrate with the existing `SurfaceCombustionStepper`.

The minimum accepted invariants are:

- no negative fuel, oxygen, or other species inventory;
- reaction extent cannot exceed available reactants;
- reactant consumption and product generation satisfy the selected reaction's stoichiometry;
- energy additions and removals are recorded, and no heat appears merely because a flame is rendered;
- timestep, cell area/volume, transport method, boundary conditions, and stability limits are explicit;
- results are deterministic for identical inputs, model versions, and random keys under the documented execution contract.

The current reference step includes a bounded graph oxygen-exchange pass and per-cell inventory/heat accounting. An additional `SurfaceCombustionThermalTransportStepper` now applies the existing lumped-body conduction primitive over explicit, stable-ID cell connections. It preserves internal energy to a checked tolerance, skips blocked connections, rejects duplicate/unknown edges, and rejects a step that would reverse temperature ordering or make a cell's reference-relative thermal energy negative. The cell model declares lumped capacity rather than physical mass and specific heat separately, so the adapter uses a documented mathematical equivalent (1 kg × the declared J/K capacity); this is not a claim about actual cell mass. Connections are processed in canonical order, but this is a sequential explicit scheme and can depend on topology; it is not a simultaneous spatial heat equation. The implementation and focused tests passed repository CI: [run 40942016218](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/40942016218) reports 524 core tests and 3 Physics MicroBundle tests passing, with no failures or skips. Next increments are: reference-grounded reaction profiles and product/mass ledgers; source-to-surface ignition; and only then richer propagation, gas expansion, buoyancy, or GPU parity. Do not claim full combustion because these scalar fields evolve.

## 5. Three-texture concept and the fire triangle

The three fields are the user's proposed compact working representation of the fire triangle:

- Fuel availability constrains reaction.
- Oxygen availability constrains reaction.
- Heat/temperature determines whether ignition and reaction-rate conditions are met.

They are not three independent artistic controls. Their coupling is governed by the reaction profile and physical model. Additional fields (moisture, gas products, soot, smoke, or phase) must be introduced when omitting them would break the model's intended accounting or validity.

A source flame is a boundary/source condition with its own temperature/energy, geometry, duration, and supported transfer model. A flame particle system is generated from solved hot/reaction regions as a presentation effect. Particle variation may be seeded by Squirrel3, but random variation may perturb presentation or declared stochastic submodels only; it must not override stoichiometry, create fuel, or conceal non-conservation.

## 6. Squirrel3 and non-repeating but replayable variation

Use existing `Squirrel3` / `Squirrel3Keyed` primitives rather than a new unrelated random implementation. Prefer keyed samples using stable surface-cell identity, logical tick, and a named purpose ID. This prevents a new particle or unrelated cell from reshuffling every other flame's result.

Record algorithm identity/version, seed, key schema, and logical tick when randomness affects authoritative simulation. Use randomness for explicitly modeled variability (e.g. bounded local transport/combustion fluctuations once their distribution is justified) and visual sampling. It is not a substitute for a rate law or physical uncertainty model.

## 7. Staged acceptance plan

1. **Data contracts — present:** stable cell identity, explicit inventories, area, thermal energy, and normalized surface normal are validated by `SurfaceCombustionCellState`.
2. **Bounded CPU step — implemented and CI-validated:** `SurfaceCombustionStepper` performs deterministic ordering, explicit graph oxygen exchange, blocked-edge handling, reactant-limited fuel/oxygen consumption, and heat accounting. Focused tests cover reaction limits, blocked/open transport, outgoing-inventory limits, input validation, and stable result order. The latest repository workflow passed on commit [`16f6fc9`](https://github.com/TrentBest/TheSingularityWorkshop.Physics/commit/16f6fc93d2281779cea4c5334f8ea7f67a8e9ff3): all 519 core tests and 3 Physics MicroBundle tests passed, with zero failures or skips. The workflow built both core and MicroBundle NuGet packages as CI artifacts only (not published).
3. **Reaction profile, bounded extent, and immutable per-cell species inventory transition — implemented as separate primitives; integration with the main combustion stepper remains:** `SurfaceReactionProfile` validates balanced elemental counts and molar mass with source/version and evidence classification. `SurfaceReactionExtentCalculator` caps a requested extent against available reactant masses and returns consumed/generated species masses, heat release, and a mass residual. Focused tests cover stoichiometric output, limiting-reactant behavior, empty inventory, and invalid input. Still missing: persistent per-cell product inventory, reaction-profile integration into `SurfaceCombustionStepper`, sourced material-specific kinetics, and a time-integrated species ledger. The current methane fixture is illustrative only. [CI run 40950065073](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/40950065073) passed the full Physics Build and Tests workflow on commit [`204f6b1`](https://github.com/TrentBest/TheSingularityWorkshop.Physics/commit/204f6b11c72bd3fc9aaf059cf514f510d005e2e1).
4. **Ambient oxygen boundary and inter-cell heat transfer — implemented and CI-validated:** explicit external oxygen supply rates can replenish selected cells, and blocked boundaries prevent replenishment. Boundary-supplied oxygen is reported separately from internally transported oxygen. Tests cover supply, blocked starvation, duplicate/unknown boundaries, and reaction consumption. `SurfaceCombustionThermalTransportStepper` transfers heat along explicit conducting connections using declared conductivity, contact area, path length, and timestep; blocked edges do nothing, transfer accounting is exposed, and energy conservation is checked. Tests cover heat flow, conservation, blocked paths, invalid topology, deterministic ordering, and timestep rejection. [Repository CI run 40942016218](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/40942016218) passed all 524 core tests and 3 Physics MicroBundle tests, with zero failures or skips. The model still does not simulate ambient flow or diffusion; current graph oxygen exchange assumes equal-sized control volumes, and heat transfer is sequential lumped conduction rather than a spatial heat equation.
5. **Ignition and source transfer — later:** model source-to-surface energy transfer and material response; the source must not directly set target state.
6. **Flame/smoke outputs and GPU parity — later:** derive presentation from authoritative recorded state; compare any packed/GPU field implementation against the deterministic CPU reference.

Every stage updates the capability catalog only when source and tests support the new status. This plan does not imply the later stages already exist.


## 8. Coupled conduction-driven ignition: current reference increment

`SurfaceCombustionSimulationStepper.Step` now composes the existing conduction and reaction primitives in an explicit order for one interval:

1. Apply conduction along the supplied `SurfaceThermalTransportConnection` graph, using declared conductivity evidence, contact area, separation, and duration.
2. Evaluate reaction against those updated cell temperatures, declared ignition thresholds, fuel inventories, and oxygen inventories/boundaries.
3. Return the conduction result and combustion result separately, so internal energy transfer is not confused with reaction heat release.

This makes a small deterministic form of fire spread possible without object-specific ignition scripts: a neighboring cell can react during the interval only if computed conduction raises it to the configured ignition threshold and reactants are available. Heat released by reaction is added after that interval's conduction and therefore influences later intervals. This is an integration convention, not a claim that a real flame spreads through conduction alone; convection, radiation, flame-source transfer, moisture/pyrolysis, airflow, and material-specific kinetics remain unmodeled in this slice.

Focused tests use explicitly illustrative parameters to show that a neighbor below threshold does not consume fuel, a neighbor heated above threshold does consume fuel, internal conduction energy is conserved separately from reaction heat, and an oxygen-starved neighbor remains unburned even when hot. These tests validate the software contract, not empirical ignition thresholds for wood, paper, wax, or any other real material.

**Important next step:** the orchestration still accepts `SurfaceCombustionCellParameters` directly. It does not yet derive these parameters from material composition or select a reaction profile based on elemental inventory. That mapping must preserve property units, evidence class, source/version, temperature applicability, and uncertainty; element identity alone cannot supply a material's ignition temperature or reaction kinetics. See [issue #37](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/37) for the staged implementation and acceptance criteria.
