# Geological formation and terrain evolution

**Status:** Physics domain architecture and implementation sequence. This document distinguishes the processes that create relief from the processes that later modify it. It does not claim that every process listed below has a numerical solver.

## The governing distinction

Weathering is not the origin story of a mountain. It breaks down and alters material already exposed to the environment. Erosion transports that material; deposition places it elsewhere. Those processes can radically reshape relief, but the initial uplift, intrusion, accumulation, or displacement must come from formation processes.

The terrain system therefore needs two connected but distinct questions:

1. **How did this landform come to exist?** Formation and deformation: tectonics, volcanism, intrusion, folding, faulting, uplift, subsidence, impact, and mass accumulation.
2. **How has it changed since?** Evolution: weathering, erosion, transport, deposition, glaciation, dissolution, compaction, and continuing deformation.

A terrain snapshot should be explainable as the result of a sequence of identified processes, not merely as a plausible elevation texture.

## Process families

### 1. Deep and regional formation

- Plate convergence, divergence, and transform motion establish regional boundary conditions.
- Crustal thickening and isostatic response create broad uplift or subsidence.
- Folding bends layered rock under distributed deformation.
- Faulting offsets rock blocks; normal, reverse/thrust, and strike-slip faults have different displacement geometry.
- Magmatic intrusion adds material below the surface and can dome, fracture, or thermally alter surrounding rock.
- Volcanism transfers material to the surface through vents, flows, domes, and tephra accumulation.

### 2. Sudden events and catastrophic change

- Earthquakes are rapid slip events on faults, not a generic mountain-raising brush. They can uplift one block, drop another, offset channels, expose or bury strata, and trigger secondary failures.
- Landslides and debris flows redistribute existing mass downslope.
- Caldera or cave-roof collapse creates negative relief and displaced material.
- Impacts excavate a transient cavity and redistribute ejecta.
- Floods, lahars, and glacial outburst events transport large sediment loads.
- Volcanic eruptions can add lava/tephra, remove material, collapse vents, and alter drainage.

### 3. Surface evolution

- Weathering changes material in place: mechanical, chemical, and biological.
- Erosion removes and transports material through water, ice, wind, and gravity.
- Deposition adds transported sediment and builds fans, deltas, floodplains, dunes, moraines, and coastal forms.
- Dissolution and groundwater processes can create subsurface voids and sinkholes.
- Compaction, cementation, and metamorphism change material properties and geological history.

These families interact. An earthquake can trigger a landslide; a landslide can dam a river; a lake can accumulate sediment; later incision can expose the deposit. A volcano can build a cone, then glaciers and rivers cut valleys into it. Formation and evolution are not competing generators; they are a causal history.

## A model hierarchy that keeps the science honest

### Level A — deterministic geometric operations

Use existing height fields and volume representations to apply explicit, bounded geometry changes. These are useful for authoring and tests, but must be labeled geometric rather than geophysical. Examples include a radial uplift field, a prescribed fault offset, or a deposited volume.

### Level B — process-specific physical models

A process model must declare its assumptions, inputs, units, boundary conditions, time scale, numerical method, and valid range. For example:
- a fault-displacement model defines fault geometry, slip vector, hanging-wall/footwall convention, and displacement field;
- a volcanic emplacement model defines source volume, emplacement footprint, and mass/volume accounting;
- an erosion model defines the transport law, material response, water/ice/wind forcing, and sediment boundary conditions;
- an isostatic model defines load, density assumptions, compensation model, and response time.

### Level C — coupled geological history

Only couple models when the exchange variables and conservation behavior are explicit. A fault event may update terrain, expose material layers, change drainage, and trigger slope stability checks. Each subsystem should retain its own versioned model and evidence.

## Representation boundaries

- TerrainHeightField2D remains the efficient single-valued exterior surface. It cannot represent overhangs, caves, vertical fault faces, or multiple surfaces.
- SparseMaterialVoxelVolume3d represents bounded solid occupancy and material identity. It is suitable for a first 3D geological mass and cutaway, but categorical voxels do not by themselves model stress, fracture, or strength.
- Geological layers, faults, intrusions, and event histories should be explicit data, not inferred from a rendered color.
- A height-field operation that shifts elevations is not automatically a fault mechanics solver. A prescribed displacement is an input, not a prediction of earthquake physics.
- Visualize/Renderer may show the results; it does not author authoritative geology or silently invent geological history.

## Recommended implementation sequence

1. **Formation foundation:** formalize a versioned terrain-process record and immutable operation result: process identity, algorithm/version, inputs, units, affected bounds, before/after revision, and validation evidence.
2. **Tectonic primitives:** add deterministic, explicitly prescribed uplift/subsidence and fault-displacement operations on height fields, with exact coordinate and sign conventions. Keep them distinct from dynamic stress/rupture simulation.
3. **Geological structure:** add material layers and region labels to the 3D solid, preserving stratigraphic order and provenance.
4. **Volcanic emplacement:** evolve the existing volume-accounted deposition beyond the idealized cone, while keeping eruption dynamics separate until modeled.
5. **Surface evolution:** introduce erosion/transport/deposition with material and sediment accounting, rather than treating weathering as a noise filter.
6. **Event composition:** connect earthquakes, slope failures, drainage changes, and deposition through explicit event outputs and stable revisions.
7. **Validation:** compare simple cases with analytic expectations and published benchmark scenarios; test determinism, bounds, conservation, and atomic failure. State uncertainty and limits.

## First proving scenarios

- **Volcanic island:** submarine construction, successive lava/tephra emplacement, cooling/settling assumptions, drainage incision, and coastal erosion.
- **Faulted mountain range:** regional uplift plus a declared fault network; show hanging-wall/footwall displacement, offset streams, and later erosion. The initial model is prescribed kinematics, not a stress solver.
- **Earthquake valley:** a fault event creates differential uplift/subsidence; slope failures and channel rerouting are subsequent processes with separate ledgers.
- **Glaciated massif:** tectonic/volcanic formation creates the massif; ice erosion cuts cirques and valleys; transported sediment forms moraines and outwash.

## Acceptance rules

- Never describe a geometry stamp as a physical prediction.
- Never call a material label a strength model.
- Never infer tectonic plate motion, fault sense, or coordinate handedness from axis names.
- Preserve source snapshots; invalid operations fail atomically.
- Record process parameters, algorithm versions, source revisions, units, bounds, and material/volume accounting.
- Keep each process independently testable before coupling.
