![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# SURPS — Physics Domain — LOD 1 — Geology and Planetary Evolution

![SURPS Rulebook LOD 1](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/development/docs/assets/rulebook-lod-1.svg)

![Geology and Planetary Evolution cover](covers/geology-and-planetary-evolution.svg)

**Descriptor:** [geology-and-planetary-evolution.book.json](geology-and-planetary-evolution.book.json)  
**Status:** Foundational sourcebook; theory and simulation contracts are defined here. Individual numerical solvers are identified separately and are not implied by this book.

## 1. A planet has a history

A mountain range is not a terrain asset. It is the visible remainder of a long causal history: internal heat, mantle and lithosphere behavior, plate interactions, crustal deformation, fault motion, uplift, subsidence, magmatic activity, surface processes, and repeated burial and exposure.

This book establishes the physical vocabulary and rules for reconstructing that history. Physics owns physical quantities, equations, numerical models, solvers, and validation evidence. SURPS owns shared rulebook identity and composition. A game or world repository owns its setting, narrative, encounter design, and presentation.

## 2. Planetary structure and interior state

A planetary model should declare, rather than assume:
- radius, mass, shape, gravity model, rotation, and reference frame;
- bulk composition and the composition assigned to core, mantle, crust, and surface reservoirs;
- density, pressure, temperature, phase, and material state as distinct quantities;
- internal energy sources and thermal history;
- layer boundaries, thicknesses, and uncertainty;
- initial conditions and external boundary conditions.

The core is not a magic piston that pushes mountains upward. Interior pressure and temperature participate in a coupled mechanical and thermal system. Mantle buoyancy, convection, rheology, lithospheric strength, gravitational potential energy, and plate boundary forces influence deformation. A reduced-order model may prescribe mantle forcing, but it must declare that approximation and its limits.

## 3. Heat, mantle, and lithosphere

Heat moves by conduction, convection, and advection. A model must identify which mechanisms it resolves, which it approximates, and the constitutive assumptions used for density, viscosity, heat capacity, conductivity, and phase behavior.

The lithosphere is mechanically distinct from the underlying mantle. Its thickness, temperature, composition, strain rate, and deformation history affect how it responds to loading. Isostatic response is a model with declared density and compensation assumptions—not a universal elevation adjustment.

## 4. Plates and boundaries

Represent plates as regions with stable identities, geometry, reference frames, and motion histories. Their relative motion defines boundary behavior:

- **Convergent:** shortening, subduction, collision, crustal thickening, folding, thrusting, uplift, and associated magmatism may occur.
- **Divergent:** extension, normal faulting, rifting, thinning, and new crust formation may occur.
- **Transform:** predominantly lateral relative motion; displacement sense must use an explicit coordinate and viewing convention.
- **Complex boundaries:** oblique convergence, distributed deformation, microplates, and linked boundary segments require more than a single label.

A plate boundary classification is not itself a solver. Store the relative velocity or prescribed motion, boundary geometry, temporal interval, and source of the motion.

## 5. Faults and earthquakes

Fault geometry and slip are physical data, not decorative names. Distinguish:
- normal faults;
- reverse faults and low-angle thrust faults;
- left-lateral and right-lateral strike-slip faults;
- oblique-slip faults combining strike and dip components;
- listric, segmented, branching, curved, blind, buried, and detachment structures as richer geometries become supported.

A fault record should declare its coordinate frame, surface or segment geometry, strike/dip convention, hanging-wall definition, slip vector, event time, rupture extent, and whether displacement is prescribed or dynamically computed. Distinguish slip along the fault from vertical throw and horizontal heave.

An earthquake cycle is more than an offset operation. A dynamic model must address loading history, stress, constitutive response or friction, failure criterion, rupture propagation, slip rate, and post-event behavior. A prescribed coseismic offset is a useful event input, but it is not a prediction of rupture.

Earthquakes can offset strata and channels, uplift one block while subsiding another, alter drainage, expose buried materials, and trigger landslides. Record these as causally linked but separately validated consequences.

## 6. Rock cycle, structure, and geological memory

Track material through formation, alteration, transport, deposition, burial, compaction, metamorphism, melting, intrusion, eruption, and exposure. Rock names should not erase provenance or previous history.

A stratigraphic record should preserve layer identity, composition, age interval, depositional context, orientation, thickness, alteration, and cross-cutting relationships. A fault may offset an older layer; an intrusion may cut multiple layers; erosion may truncate both; a younger deposit may bury the resulting surface.

Geological age, simulation elapsed time, and numerical step count are different quantities. Use explicit units and calendars or timescales where appropriate. Never treat a million-year interval as a million solver iterations.

## 7. Geological events and terrain consequences

Model events as versioned state transitions with inputs, time interval, affected region, algorithm/model identity, and evidence. Examples include:
- folding, fault slip, uplift, subsidence, and basin formation;
- intrusion, dike/sill emplacement, eruption, lava and tephra deposition;
- caldera collapse and volcanic flank failure;
- impacts and ejecta deposition;
- landslides, debris flows, and earthquake-triggered failures.

Each process must declare what it changes and what it does not. Terrain elevation alone is not the geological state; the state also includes materials, structures, layers, event history, and any thermal or mechanical fields the model actually tracks.

## 8. Discrete simulation rules

Use immutable input revisions and candidate outputs. A process declares units, coordinate frame, boundary conditions, time interval, numerical method, valid range, and diagnostics. Coupled updates commit atomically only after required outputs validate. Failed steps leave the prior state intact.

Different processes need different clocks: thermal and tectonic evolution may use long intervals; surface processes may require shorter intervals; rupture and fluid transients may require subcycling. Temporal aggregation and event ordering must be explicit and replayable. Any stochastic input must include a recorded seed.

## 9. Evidence and validation

A model is not realistic merely because its rendered result looks convincing. Validate:
- fault displacement against analytic geometry and sign conventions;
- plate motion against relative velocity and boundary conditions;
- thermal models against energy budgets and benchmark problems;
- earthquake models against appropriate analytical or published rupture benchmarks;
- stratigraphy against ordering and cross-cutting relations;
- coupled histories against mass, volume, energy, and material residuals for quantities actually tracked.

Record uncertainty, resolution, assumptions, and known failure modes. Do not claim calibration or predictive capability without comparison to appropriate observations or published datasets.

## 10. Connected books and implementation

- [Geomorphology and Surface Processes](geomorphology-and-surface-processes.md) covers the surface processes that modify relief.
- [Weather and Climate](weather-and-climate.md) covers atmospheric state and forcing.
- [Ocean, Atmosphere, and Maritime Phenomena](ocean-atmosphere-maritime-phenomena.md) covers coupled ocean/atmosphere and maritime applications.
- [Geological formation and terrain evolution architecture](../GEOLOGICAL_FORMATION_AND_TERRAIN_EVOLUTION.md) records Physics implementation boundaries and staged work.
- [Physics capability catalogue](../PHYSICS_CAPABILITY_CATALOG.md) records implemented capabilities and limitations.

This book defines the domain contract. It does not claim that mantle convection, a full plate solver, earthquake rupture, or a complete planetary evolution engine is already implemented.
