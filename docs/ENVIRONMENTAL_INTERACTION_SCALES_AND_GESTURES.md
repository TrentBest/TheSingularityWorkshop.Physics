# Environmental Interaction Scales and Role-Constrained Gestures

**Status:** domain contract and implementation agenda. This document does not claim a complete fluid, weather, vehicle, or gesture simulation exists.

## The world is not a bag of independent objects

A puddle, pool, lake, and ocean are all water, but they are not interchangeable interaction providers. Their useful behavior emerges from composition: water state and material properties, geometry and depth, boundaries, terrain, inflow/outflow, weather forcing, nearby objects, and the scale and time horizon of the question.

The same distinction applies to a person stepping into water, a boat crossing a lake, a submarine maneuvering below the surface, and an aircraft carrier responding to waves and wind. They share physical domains but ask different questions and require different models, state, constraints, and observations.

## Water bodies: shared substance, different regimes

| Context | Important composition/state | Example interactions and outputs |
|---|---|---|
| Puddle | Thin, shallow, bounded film; local surface and ground shape; wet/dry edge | Footfall splash, displacement, runoff, evaporation, wet-surface contact |
| Pool | Enclosed basin; depth and wall/floor boundaries; controlled or known water level | Body/prop displacement, waves reflecting from walls, swimming drag, pool circulation |
| Pond or lake | Larger bathymetry; shoreline; inflow/outflow; wind exposure; potentially stratified temperature | Shore waves, wakes, water-level changes, thermal layers, sediment and shoreline interaction |
| River or channel | Directional flow, slope, discharge, cross-section, banks, obstacles | Current, drag, transport, erosion, flooding and flow around structures |
| Ocean/coastal region | Large-scale forcing, tides, bathymetry, wind waves, currents, salinity and density structure | Vessel response, swell, tides, coastal inundation, coupled air/sea forcing |

These are useful scenario labels, not hard-coded physical laws. A shallow-water approximation can be useful for some lake or coastal questions but is not automatically suitable for a tiny splash, breaking wave, cavitation, or arbitrary three-dimensional turbulent flow. Choose a model from declared assumptions, geometry, scale, requested outputs, and valid regime—not from the label “lake.”

The interaction should be assembled from explicit components:
- **Fluid state:** free-surface elevation, velocity, pressure or depth-integrated state, temperature, composition, and uncertainty as required by the model.
- **Geometry and boundaries:** basin/terrain shape, solid surfaces, openings, shoreline, wet/dry state, and moving boundaries.
- **Forcing:** gravity, wind stress, rain/snow input, heat flux, inflows, outflows, and other declared sources.
- **Participants:** bodies, feet, wheels, hulls, propellers, tools, debris, or structures with contact and coupling contracts.
- **Scale and fidelity:** spatial resolution, time step, domain extent, tolerated error, and outputs the experience needs.
- **Accounting:** track mass and momentum, and energy where the chosen model supports it; report residuals rather than silently losing quantities.

A puddle splash should not require loading an ocean-scale solver. An ocean scene should not pretend that a single splash formula resolves coastal currents. A scene may combine models at different scales, but the exchange across each boundary must be explicit and tested.

## Weather is forcing and interacting state—not decoration

Rain, snow, wind, temperature, humidity, radiation, and pressure can affect multiple domains. The architecture should represent weather as time-varying conditions and forcing providers, each declaring units, coverage, cadence, provenance, uncertainty, and supported phenomena.

Examples of coupled effects:
- **Rain:** adds water to surfaces and basins; can form puddles, increase runoff, alter traction, and change visibility. In the current implementation, `RainfallRateField2D` converts rate to depth, and `BathymetricShallowWaterBasin2D.ApplyRainfall` adds that depth to matching cells without adding horizontal momentum or advancing simulation time. This is an explicit source-splitting step; the caller owns interval and ordering, and the basin rechecks its CFL bound before committing. It is not yet a runoff, infiltration, or atmospheric solver.
- **Snow:** accumulates as a phase-dependent material; compacts or melts under declared conditions; affects load, friction, insulation, and available liquid water.
- **Wind:** applies forces/stresses to exposed bodies and water surfaces, changes wave generation and drift, and can affect vehicle control and visibility.
- **Temperature and radiation:** change heat exchange, evaporation, freezing/melting, density, and material behavior within the chosen model's validity.
- **Storm systems:** are compositions across atmosphere, water, terrain, structures, and participants—not a single global “storm” toggle.

Weather data alone is not a solver. A forecast or authored weather field is an input with source, timestamp, spatial/temporal resolution, and uncertainty; a simulation's response must state which physics model used it and where that model is valid.

## From physical affordances to gestures

Physics and Gestures must remain separate packages. Physics describes environmental state, forces, contact observations, constraints, and the conditions under which an action is physically possible. Gestures describes authored or learned motion patterns and their execution/selection. A neutral Experience-level composition connects them.

The intended selection chain is:

1. **Intent and task:** what outcome is requested (board aircraft, cast off, taxi, launch, dive, maintain heading, rescue a person).
2. **Actor and qualification:** actor identity/class, role, training, skill, and currently assigned command track.
3. **Available platform:** vehicle capabilities, controls, access, safety interlocks, occupied stations, and supported operational modes.
4. **Environment:** wind, sea state, water depth/current, visibility, precipitation, surface condition, nearby actors, and current physical state.
5. **Gesture provider discovery:** gather candidate gestures and their declared prerequisites, required controls, role/skill constraints, platform constraints, and expected outcomes.
6. **Filtering and execution:** reject candidates whose requirements are unmet; select among eligible candidates; execute the motion; observe contact/control response; report success, failure, slip, collision, or interruption.
7. **Feedback:** physical results can invalidate the next step or cause replanning, but Physics does not author the gesture and Gestures does not invent physical support or vehicle capability.

This makes a gesture a **constrained behavior**, not merely an animation clip. A motion can be present in a provider yet unavailable to a given actor, role, or platform.

### Example: carrier operations and flight roles

A flight deck is a coordinated physical environment with aircraft, people, deck geometry, tie-downs, hatches, covers, hoses, moving equipment, wind, precipitation, sea state, and time-critical dependencies. Ground crew gestures prepare and inspect the aircraft; flight crew gestures board, secure, and operate controls; the pilot's eligible actions are constrained by the specific aircraft and its current configuration.

The carrier commander and a fighter pilot do not have one interchangeable gesture set. The commander role coordinates the carrier's larger operational track and its delegated stations. A pilot operates within a narrower aircraft-specific control envelope and follows carrier/deck coordination where required. A person may be assigned or qualified for a command track without being able to perform every specialist's physical actions. A qualification, permission, or assignment is not proof that the environment currently permits an action.

The same pattern generalizes to a submarine captain and dive/control stations, a ship's bridge and deck crew, or a civilian boat operator. The role hierarchy, platform controls, procedural sequencing, and physical constraints are independent filters that compose at the experience boundary.

This example is an architecture target, not a claim that real-world aviation or naval procedures have been modeled or validated. Any operational training use would require authoritative domain review, procedural sources, safety analysis, and validation.

## Data and status discipline

A domain topic should progress through explicit states: **mapped → explained → specified → implemented → validated → coupled**. A diagram of rain hitting a puddle is explanatory until the quantities, boundaries, assumptions, model, solver, tests, and conservation expectations are defined.

Each model/provider should declare:
- stable ID and version;
- inputs/outputs with dimensions and units;
- spatial and temporal scales;
- initial and boundary conditions;
- assumptions and valid regimes;
- supported interactions and unsupported phenomena;
- accuracy/error evidence and reproducible reference cases;
- dependencies and the coupling quantities exchanged with other providers.

Do not implement a monolithic “water/weather/world” solver or put vehicle-specific gesture logic in Physics. Start with bounded scenarios, validate their reference cases, and connect them only through explicit contracts.
