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


## Planetary-scale target: simulate the history, not just the final surface

The long-term objective is a time-evolving geological model whose terrain is the accumulated consequence of a declared planetary history. A mountain range must be explainable through the interior and surface processes that built and modified it. The first volcano is one test case among many, not the scope of the system.

The conceptual dependency chain is:

1. **Planetary initial conditions and interior structure** — mass distribution, radius, gravity field, composition, thermal state, core/mantle/crust boundaries, and initial energy inventory.
2. **Thermal and mantle dynamics** — internal heat sources and transport, mantle convection or explicitly reduced approximations, viscosity/rheology, buoyancy, and heat exchange.
3. **Lithosphere and plate system** — plate geometry, thickness, density, motion, boundary conditions, and interactions at convergent, divergent, and transform boundaries.
4. **Crustal deformation and fault systems** — distributed strain, folding, fault nucleation/geometry where modeled, slip, block rotation, uplift, subsidence, and accumulated displacement.
5. **Discrete geological events** — earthquakes, after-slip, triggered failures, intrusions, eruptions, caldera collapse, impacts, and other abrupt state transitions.
6. **Surface and hydrologic systems** — atmospheric weather and climate forcing, precipitation, runoff, rivers, groundwater, ice, wind, waves, sediment transport, and weathering.
7. **Stratigraphy and geological memory** — deposited, displaced, altered, buried, exposed, and removed materials with age, provenance, and event history.

This is a dependency architecture, not a claim that all seven levels already have solvers. Each level must have an independently testable model and an explicit exchange contract before coupling.

### Planetary interior and heat

Do not collapse "pressure from the core" into a single scalar pushing mountains upward. Core and mantle conditions establish a thermal and mechanical environment; the lithosphere responds through buoyancy, convection-driven tractions, plate boundary forces, gravitational potential energy, and rheology. Pressure, temperature, density, gravity, viscosity, and strain rate are distinct fields or constitutive quantities with units and spatial domains.

A first implementation may use prescribed or reduced-order mantle forcing, but it must identify that approximation and its boundary conditions. A later continuum model can resolve more of the interior. The interface between levels should permit replacing a reduced model without rewriting terrain storage or geological event history.

### Fault taxonomy and mechanics

Fault names are not cosmetic labels. The fault model must preserve geometry, orientation, slip direction, displacement components, and the mechanical regime that produced the motion.

- **Normal fault:** hanging wall moves down relative to footwall; commonly associated with extension. Include dip, strike, rake/slip vector, throw, heave, and block geometry.
- **Reverse fault:** hanging wall moves up relative to footwall under shortening.
- **Thrust fault:** a low-angle reverse fault. Treat dip as geometry, not as a separate displacement law; support ramps, flats, and linked segments when the representation allows them.
- **Strike-slip fault:** predominantly horizontal motion along strike; distinguish left-lateral and right-lateral sense from the declared viewing/coordinate convention.
- **Oblique-slip fault:** combines strike-slip and dip-slip components; preserve the full slip vector rather than forcing one category.
- **Listric, segmented, branching, and curved faults:** require geometry beyond an infinite straight plane. Start with bounded planar segments, then add richer surfaces and network topology.
- **Blind faults and buried faults:** may deform overlying strata without a surface trace; these require volumetric or layered representations rather than a surface-only line.
- **Detachment faults and subduction interfaces:** are system-scale structures with coupled boundary conditions, not merely isolated stamps.

A fault event should record the fault surface/segment identity, coordinate frame, dip/strike convention, hanging-wall definition, slip vector, event time or step, rupture extent, and the source of the displacement (prescribed kinematics or a dynamic solver). Distinguish **slip** along a fault from **throw** (vertical separation) and **heave** (horizontal separation). Never infer left/right lateral sense from an unqualified axis name.

Earthquakes should ultimately emerge from or be driven by a fault-state model: stress/loading history, frictional or other constitutive law, failure criterion, rupture propagation assumptions, slip-rate history, and post-event response. A prescribed coseismic displacement is still valuable, but it is a kinematic event input—not an earthquake prediction.

### Discrete time and geological history

Use the API's discrete execution model deliberately:

- A **geological step** has a declared start/end time, units, duration, and process schedule. Do not assume one universal timestep works for mantle evolution, earthquake rupture, river flow, and weather.
- Each process consumes a specific immutable state revision and emits a candidate result, diagnostics, and a change set.
- A coordinator commits a coupled step only when required process outputs validate. Failed steps leave the prior revision intact.
- Processes can use different clocks: long thermal/tectonic intervals, shorter erosion/climate intervals, and very short rupture or flow intervals. Coupling requires explicit temporal aggregation or subcycling rules.
- Every event is replayable from its inputs, algorithm version, initial revision, and random seed (if any). Randomness must be explicit and deterministic for reproducible runs.
- Geological age, simulation elapsed time, and numerical step count are separate values. Never conflate a million-year model interval with a million iterations.
- Preserve stratigraphic ordering and cross-cutting relationships: a fault can offset an older layer; an intrusion can cut through existing strata; erosion can truncate both; a later deposit can bury the erosional surface.

The output is not just an elevation field. It is a geological state: terrain surface, solid volumes/layers, material properties, fault structures, thermal/mechanical fields when modeled, hydrologic state when coupled, and a provenance-bearing event history. Surface elevation is a query over that state.

### Weather, climate, and weathering are different concerns

Weather is atmospheric state and forcing (for example pressure, temperature, humidity, wind, cloud, and precipitation). Climate describes statistical behavior of weather over longer intervals. Weathering is the physical/chemical/biological alteration of exposed material. Erosion transports material; deposition stores it. Keep these contracts separate while allowing weather/climate to drive runoff, ice, wind erosion, and weathering models.

This distinction also informs educational content: weather phenomena, climate, weathering, erosion, and geological formation deserve connected but distinct explanations and reference material. A weather event may trigger a flood or landslide; that does not make atmospheric weather the same model as the resulting geomorphic process.

## Implemented first fault-kinematics contract

The initial code slice is now present in `Physics/Geology/FaultKinematics3d.cs` with focused tests in `tests/TheSingularityWorkshop.Physics.Tests/Geology/FaultKinematics3dTests.cs`.

It defines a caller-identified planar fault, explicit coordinate frame and linear unit, normalized strike and down-dip basis, oriented plane normal, explicit hanging-wall side, declared mechanism, and signed strike/dip slip components. The resulting hanging-wall displacement is a full 3D vector; the footwall is stationary in this prescribed block model. The contract does not infer handedness, stress regime, or fault sense, and it does not claim to model rupture or earthquake dynamics.

Current limitations are intentional: the mathematical plane is infinite, the displacement is a discontinuous block offset, and there is no finite rupture patch, segmented/curved surface, distributed elastic deformation, layer intersection, or material-volume update yet. This is the shared kinematic foundation, not the finished fault system. CI validation is running for this first code increment.

## Expanded implementation program

The sequence below is deliberately staged so that the first numerical models are useful without pretending to solve the entire planet.

1. **Canonical geological state and history:** define immutable state revisions, geological time, process/event records, coordinate and unit contracts, material/stratigraphic identity, and deterministic replay.
2. **Fault geometry and kinematics:** implement a shared fault-surface/segment representation and explicit slip vector. Cover normal, reverse/thrust, strike-slip (both senses), and oblique slip with analytic displacement tests. Begin with prescribed kinematics and clearly label it.
3. **Layered crust and volumetric deformation:** represent strata and material regions in 3D so faults can offset layers and blind faults can deform overburden. Preserve volume/material identity and event provenance.
4. **Tectonic plate system:** represent plates and boundary segments with relative motion and boundary classification; derive or prescribe local deformation under stated assumptions.
5. **Interior thermal/mechanical forcing:** introduce thermal state, gravity/density assumptions, reduced mantle forcing, and a replaceable solver boundary. Expand toward convection/rheology only with benchmarked numerical methods.
6. **Earthquake cycle:** add loading, constitutive friction/failure, rupture and post-slip models as separate levels; validate against analytical and published benchmark problems before claiming predictive behavior.
7. **Magmatic and volcanic systems:** distinguish intrusion, magma transport, eruption, lava/tephra emplacement, thermal evolution, and collapse. Retain the current volume-accounted cone as a geometric/deposition primitive.
8. **Weather/climate forcing:** establish atmospheric forcing records and timescale-aware weather/climate inputs without coupling an atmospheric solver prematurely.
9. **Surface-process suite:** weathering, slope failure, runoff, river incision, sediment transport/deposition, glaciation, wind, and coastal processes with explicit material and water budgets.
10. **Coupled geological histories:** run proving scenarios over increasing time spans and resolutions; expose diagnostics, conservation residuals, uncertainty, and replayable state snapshots.

### Validation expectations for the planetary model

- Fault displacement tests must cover both sides of the fault, sign conventions, dip/strike/rake, slip/throw/heave relationships, segment endpoints, and displacement continuity where the chosen model promises it.
- Plate motion tests must verify relative velocity and boundary classification without coordinate-frame ambiguity.
- Interior models must declare and test units, equations of state or reduced constitutive assumptions, energy/heat budgets, boundary conditions, and numerical stability.
- Earthquake models must distinguish loading, failure, rupture, and post-event response; compare against appropriate analytical or published benchmark cases.
- Coupled steps must report mass, volume, energy, and material-accounting residuals for the quantities they actually model. Do not demand conservation of a quantity the model does not track; do not silently omit a tracked quantity.
- Multi-timescale runs must test subcycling, event ordering, deterministic replay, and atomic commit on failure.
- Geological histories must preserve relative ages, stratigraphic order, cross-cutting relations, and provenance through deformation and erosion.
- Compare outputs with observed geological structures or published datasets before using terms such as realistic, calibrated, or predictive.

## Acceptance rules

- Never describe a geometry stamp as a physical prediction.
- Never call a material label a strength model.
- Never infer tectonic plate motion, fault sense, or coordinate handedness from axis names.
- Preserve source snapshots; invalid operations fail atomically.
- Record process parameters, algorithm versions, source revisions, units, bounds, and material/volume accounting.
- Keep each process independently testable before coupling.

## Current finite-fault implementation slice

`FaultKinematics3d` remains the infinite-plane prescribed block-displacement primitive. `FiniteFaultSegment3d` now bounds that prescription to a caller-sized rectangular footprint centred on the plane origin, aligned to the declared strike and down-dip basis. Its tests cover inclusive segment edges, both finite extents, hanging-wall-only displacement, and zero displacement outside the footprint, on the footwall, and on the plane.

This is a geometric finite-footprint extension, not a finite-fault elastic solution. The rectangular edge has a discontinuous displacement boundary by design. Stress transfer, rupture propagation, curved/irregular surfaces, segment interaction, layered-crust response, and earthquake-cycle mechanics remain unimplemented. See [Finite fault segment 3D](FINITE_FAULT_SEGMENT_3D.md) for the precise contract and limitations.


## Player-facing world creation: planetary history as a replayable simulation

The intended world-creation experience begins before terrain exists. The player supplies or discovers a planetary system, watches its geological history unfold, and then explores the resulting surface and subsurface. The final world is not a random height map: it is a reproducible state reached through a sequence of modeled causes.

### Stage 1 — Establish the planetary system

Capture the star or stars and the planet's initial conditions before generating continents:

- stellar identities and properties relevant to the planet's energy input;
- orbital elements and relative position to each star, including multi-star configuration where supported;
- planetary radius, mass, bulk composition, rotation, axial orientation, and initial thermal state;
- atmosphere/ocean inventory and other initial reservoirs only where the chosen model has explicit quantities and units.

These inputs establish boundary conditions and plausible ranges; they do not by themselves determine a unique climate or geological future. The model must state which stellar, orbital, tidal, radiative, and rotational effects it actually computes and which it approximates.

### Stage 2 — Choose a tractable planetary representation

Derive a declared simulation resolution from planetary scale and available material/initial-condition data. The first world-generation experience may simplify the surface into a manageable number of major plates and broad continental/oceanic regions. Record this as a model-resolution decision, with stable identities and a mapping back to the underlying spatial grid.

Do not hard-code a universal continent count or imply that a simple formula predicts the real number of continents. Plate count, plate geometry, crustal composition, and continental regions are related but distinct. A planet may have oceanic lithosphere without a continent, and a continental region is not simply one plate.

### Stage 3 — Evolve the interior and plate system

Initialize core/mantle/crust thermal and compositional state, then evolve the lithosphere under the selected interior model. Plate interactions should be represented by relative motion and classified boundaries:

- convergence can thicken and shorten crust, build mountain belts, and form subduction systems;
- divergence can thin and separate lithosphere, form rifts, and create new oceanic crust/basins;
- transform motion accommodates lateral relative movement and can accumulate fault loading.

These are process families, not guaranteed one-step terrain stamps. The causal chain from interior forcing through plate motion, deformation, uplift/subsidence, and surface relief must remain inspectable. Begin with a clearly named reduced-order model if a full mantle/plate solver is not yet practical.

### Stage 4 — Advance geological time at multiple resolutions

A player-visible history step might summarize one million years, but that interval is not a universal numerical timestep. A million-year display interval may aggregate many internal steps, while abrupt events can trigger finer substeps. Mantle evolution, plate motion, climate statistics, river processes, and earthquake rupture operate on very different timescales.

Each visible history checkpoint should be a durable, replayable snapshot with geological age and elapsed simulation time; model/algorithm versions and input revision; events and process summaries during the interval; before/after state identities and validation diagnostics; and a concise narrative suitable for the world-history timeline.

The player should be able to scrub through these checkpoints, inspect a mountain belt as it forms, see an ocean basin open, and select an event to understand its cause and consequences. StoryBuilder can turn validated event records into narration and presentation; it must not invent physical events or alter Physics state.

### Stage 5 — Catastrophes become permanent world history

A large impact is a first-class geological event, not merely a cinematic effect. Its model may produce a crater cavity, uplifted rim, ejecta distribution, shocked or altered material where modeled, and downstream atmospheric/oceanic consequences only when those models exist. The event record links the crater and deposits to the impact, and later erosion or burial modifies them without erasing their provenance.

The same pattern applies to major eruptions, caldera collapse, megafloods, glaciations, and mass-wasting events: a causal event record, physical state changes, persistent geological evidence, and a player-facing historical account. A dinosaur-extinction-scale scenario is an Earth-inspired scenario, not a universal outcome that every impact must produce.

### Stage 6 — Let climate drive surface evolution

Keep stellar/orbital forcing, atmospheric weather, long-term climate statistics, and geological weathering as separate contracts. Climate and weather patterns provide forcing to surface processes. Weathering alters exposed material; erosion removes and transports it; deposition stores it in new places.

The terrain texture can be a GPU-resident acceleration representation for repeated local updates such as moisture, temperature exposure, erodibility, weathering progress, and sediment flux. Physics remains authoritative: define a CPU reference and deterministic input/output contract first, preserve material/water budgets, and make GPU results validate against declared tolerances. GPU texture values are not a substitute for stratigraphy or 3D solid state.

Surface evolution must move material, not simply darken pixels or displace a height texture. Sediment removed from a mountain should be accounted for as transported, deposited, exported, or otherwise explicitly budgeted. Rivers, lakes, ponds, and groundwater require water storage/routing contracts; deposition can build deltas, fans, floodplains, and lake beds.

### Stage 7 — Build the underground world from geology

Caves, caverns, sinkholes, and connected underground passages belong to the same geological history as the surface. They require 3D solid/material representations, not a height field. Candidate processes include dissolution in soluble rock, fracture-controlled flow, lava tubes, and collapse; each needs its own assumptions and validation.

The first dungeon-ready result should be a geological volume with stable connectivity, material identity, entrances, water state where modeled, and provenance explaining how each void formed. Game-specific dungeon dressing, encounters, traps, and authored rooms belong to consuming experiences. Physics supplies the physically derived spaces and their queryable geometry.

### Architectural boundary

The intended pipeline is:

planetary system → initial interior → plate/continent representation → geological time steps → tectonic and catastrophic events → climate/weather forcing → weathering/erosion/transport/deposition → hydrology and 3D subsurface evolution → replayable world

This is an implementation roadmap, not a claim that these coupled solvers already exist. Implement each stage as a separately testable Physics capability with explicit units, resolution, timescale, conservation rules, provenance, and limitations. The player-facing timeline and StoryBuilder narration consume validated event records; renderers and GPU kernels accelerate or display derived state without becoming the source of truth.


## Implemented geological time and event provenance contract

`GeologicalTimeInterval` now records geological age in years before present separately from monotonically increasing simulation elapsed seconds and a non-negative step index. Geological age decreases toward the present; simulation time increases. Equal geological start/end ages are valid for instantaneous events. This makes a million-year player-facing interval representable without implying that it is one solver iteration or the only clock in the system.

`GeologicalEventRecord` captures a stable event/type identity, interval, source/result revision identities, model and algorithm identities/versions, JSON-object parameters, and a player-readable summary. The record is immutable metadata: it does not execute a process, prove that referenced revisions exist, or commit world state. A future coordinator must validate candidate outputs and referenced revisions before atomic commit. Focused tests cover clock direction, a one-million-year interval, instantaneous events, invalid clocks, and event provenance validation.
