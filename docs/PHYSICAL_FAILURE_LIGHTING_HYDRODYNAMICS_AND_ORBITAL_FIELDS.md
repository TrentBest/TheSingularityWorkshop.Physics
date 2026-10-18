# Physical Failure, Illumination, Hydrodynamics, and Orbital Fields

**Status:** design contract and implementation roadmap. This document does not claim that the listed coupled solvers or orbital tide generator are already implemented.

## Governing principle

Physical behavior is the source of truth. Presentation may deliberately stylize a result, but a stylized parameter must have a declared identity, scope, value, units, and provenance. The resulting behavior should then follow from the altered input through the same causal system. If the selected model cannot represent an effect, report that limitation or select an explicitly authored approximation; do not silently fake the state.

The forge is where expensive, slowly changing relationships can be solved, sampled, reduced, and stored. Runtime simulation consumes those products at their declared resolution and validity range. A lookup texture is a storage and sampling format—not a physical law, nor automatically a GPU-only feature.

## 1. Material failure: melting, shattering, and brittle fracture

These are related but distinct outcomes. They must not be one generic “damage” toggle.

- **Thermal softening and melting:** temperature and pressure, material composition, phase boundaries, latent heat, and heating/cooling history determine whether a material changes phase or loses strength. A melting-point threshold alone is only a bounded approximation; it does not account for latent heat or mixtures unless the selected model says so.
- **Brittle fracture:** stress/strain, flaw population or fracture toughness, geometry, loading rate, temperature, and boundary conditions govern crack initiation and propagation. A fracture event changes connectivity and redistributes forces and energy; it is not merely a crack texture.
- **Shattering:** a possible fragmentation outcome after fracture instability. Fragment sizes and velocities must follow a declared fracture/fragmentation approximation and account for momentum and energy. Artistic variation can decorate fragments, but cannot silently create unbounded kinetic energy.
- **Ductile failure and fatigue:** retain separate models for plastic deformation, damage accumulation, and cyclic loading; do not infer these from a brittle-fracture result.

A material profile supplies evidence-backed properties and validity conditions. A constitutive/failure model interprets those properties under a particular loading scenario. The solver produces an event/observation describing onset, affected region, energy transfers, and confidence or model limitations. Rendering may turn that observation into cracks, chips, dust, or debris, but those are downstream representations.

## 2. Illumination and radiative transfer

Lighting is physics when it represents energy transfer, absorption, scattering, reflection, transmission, emission, or shadowing. It is also a rendering concern, so the interface must distinguish physical radiometric quantities from artistic display controls.

- A light source has a declared emission model, spectrum or approximation, power/intensity convention, position/orientation where relevant, and time behavior.
- Materials expose optical properties with wavelength and condition scope where available: reflectance, absorptance, transmittance, refractive behavior, emissivity, and scattering assumptions.
- Surface orientation and visibility affect incident energy. Thermal absorption can feed the thermal state; thermal emission can feed an appropriate radiative model.
- A real-time renderer may use simplified illumination, precomputed probes, or stylized exposure. Those approximations must not be mistaken for a thermal-energy solver.
- If a cinematic control changes source power, material reflectance, atmospheric density, or exposure, it should be a named override. Where the changed physical quantity participates in other models, those consequences follow from it.

## 3. Gas expansion, pressure, and catastrophic water motion

Gas expansion must be coupled to thermodynamic state and the chosen equation of state. Pressure work, heat transfer, flow, compressibility, and shock behavior are different levels of fidelity; the selected model must state which it supports. A cheap educational or game model can be useful without claiming to resolve shock fronts.

A dam-break scenario should begin from geometry, water level, fluid properties, gravity, and boundary conditions. The release creates a transient flow; it can apply loads to terrain, structures, and occupants. A tsunami is not simply a tall surface wave: generation, long-wave propagation, bathymetry, shoaling, inundation, and coastal geometry matter. Select a suitable depth-averaged or higher-fidelity model for the intended scale and report the omitted physics. Do not imply that a local particle splash solver can predict an ocean basin.

Coupled domains exchange quantities through explicit interfaces: fluid pressure and traction load structures; deformation/failure changes boundaries; heat and phase change alter material/fluid properties where supported; gravity and planetary-scale forcing establish the environment. Use nested resolution or domain-specific time steps rather than forcing every process to update at one frequency.

## 4. Orbital forcing and precomputed tidal fields

Tides should be derived from the actual bodies and orbit defined for the simulated world, not hardcoded as an Earth schedule. The model inputs can include the primary body's mass and radius, the orbiting body's mass and ephemeris/orbital parameters, rotation, axial orientation, orbital phase, local geography/bathymetry, and the selected tide approximation. An Earth reference is a comparison frame only; it is not the source of truth for another planet.

### Forge pipeline

1. **Declare the world and time convention.** Record units, epoch, calendar/clock mapping, rotation period, orbit/ephemeris version, and the reference frame.
2. **Compute astronomical forcing.** Evaluate the selected orbital/rotational model over a declared time horizon and resolution. If multiple bodies matter, include their declared contributions rather than assuming a single moon.
3. **Apply the world response model.** Astronomical forcing alone is not a complete local ocean tide. The model must declare whether it produces a simplified tide potential/height, a calibrated local tide prediction, or a hydrodynamic response that includes basin shape, depth, coastlines, and dissipation.
4. **Bake a versioned lookup product.** Store axes, units, sample interval, epoch, coordinate convention, model identity/version, input digest, interpolation rule, validity horizon, provenance, and an error estimate. A texture may be the runtime encoding, but a canonical CPU-readable representation and reproducible bake are required.
5. **Sample and interpolate at runtime.** Local world time selects the time coordinate; spatial coordinates or region identifiers select the location. The evaluator returns values and quality/range status, not an unlabelled color.
6. **Derive semantic states.** Labels such as Incoming, Outgoing, KingTideIncoming, and KingTideOutgoing are derived classifications over the predicted level and its derivative, plus a declared local high/low/extreme definition. They are not substitutes for the underlying continuous values.

### Texture channels and time resolution

A texel can encode a sample or a packed sample, but the layout must be explicit. If each pixel represents an hour and channels represent half-hour samples, the document must specify how many distinct samples are stored per texel, what each channel means, and how interpolation crosses texel and day/year boundaries. Do not use RGBA channels as four independent values without a versioned encoding definition. Prefer a typed CPU table as the canonical source, then optionally encode it into a texture for fast sampling.

Store the continuous prediction needed by consumers—at minimum the modeled level/height and its rate of change if the selected model supports them—alongside validity and metadata. A runtime can classify rising/falling and high/low phases cheaply. “King tide” needs a defined local threshold or predicted extreme criterion, not a universal enum triggered by the moon being in a particular named phase.

The baked table is valid only for its declared world, forcing inputs, model, and horizon. An orbit or world parameter change invalidates the dependent bake and requests a rebuild or an explicit stale-data response. This dependency belongs in the forge/build pipeline; do not silently use an old table after changing the orbit.

## 5. Time is a first-class simulation coordinate

Do not assume Earth seconds, Earth days, or the Gregorian calendar are universal world rules. Separate:

- **Physical elapsed time:** the solver's base time unit, normally SI seconds for equations that use SI.
- **World clock:** the display/calendar mapping used by inhabitants and world systems.
- **Astronomical time:** epoch, reference frame, orbital/rotational phase, and ephemeris conventions.
- **Simulation scheduling:** logical step, process cadence, catch-up policy, and deterministic ordering.
- **Reference comparison:** an explicit mapping to Earth time/calendar for debugging or education, never a hidden conversion inside physics.

For a planet with a different rotation or orbit, local hours and days can differ while equations still use a consistent physical time unit. Every stored table declares its epoch and time mapping. Replays and observations record both physical/logical time and the world-clock interpretation where relevant.

## 6. Reality-to-story embellishment

Support intentional stylization as explicit model configuration, not as invisible magic. A configuration might select a faster-than-real crack-propagation scale, exaggerated wave height, or modified source output. Record the parameter's identity, value, unit, intended scope, and whether it changes authoritative simulation state or presentation only. The physical chain then uses that declared value; downstream consequences are recomputed consistently where the model supports them.

Keep three categories distinct:
1. **Physical model:** validated equations or measured/calibrated behavior within stated limits.
2. **Reduced-order approximation:** cheaper behavior with declared omissions and error expectations.
3. **Cinematic presentation:** artistic exaggeration that may affect only display, or may intentionally alter simulation inputs if explicitly configured.

Never label the second or third category as experimentally validated reality.

## 7. Implemented CPU reference slice: world time and orbital forcing

The following narrow, tested implementation now exists alongside this broader design contract:

- [WorldTimeDefinition](../Physics/Astronomy/WorldTimeDefinition.cs) declares a stable world ID, epoch ID, and positive rotation period in SI seconds. It reports local rotation cycles/phase separately from elapsed seconds and conventional Earth-reference days. Earth days are a comparison scale, not an assumption that the world uses Earth's calendar.
- [OrbitalTidePotentialBaker](../Physics/Astronomy/OrbitalTidePotentialBaker.cs) deterministically samples the quadrupole differential tidal potential for one perturber, circular orbit, and equatorial observer. Its inputs explicitly include perturber mass, orbital distance, world radius, gravitational constant, orbital and rotation periods, initial sub-body longitude, observer longitude, epoch, and sample grid.
- [Focused tests](../tests/TheSingularityWorkshop.Physics.Tests/Astronomy/OrbitalTidePotentialBakerTests.cs) cover determinism, endpoint inclusion, interpolation, closed-horizon rejection, defensive sample copying, an aligned turning point, and invalid input. World-clock tests are in [WorldTimeDefinitionTests](../tests/TheSingularityWorkshop.Physics.Tests/Astronomy/WorldTimeDefinitionTests.cs).

**Strict limits:** this table stores differential tidal potential (m²/s²) and its time derivative, not ocean height or a KingTideIncoming forecast. The current reference model omits eccentricity, orbital inclination, multiple perturbers, deformation/Love numbers, latitude variation, local bathymetry, basin response, weather, and coast geometry. Semantic Rising, Falling, and TurningPoint states are therefore states of this potential signal only. A later local-response model must combine the forcing with a declared physical response before classifying sea-level extremes. The gravitational constant is caller-supplied; test values are illustrative and are not an empirical planet definition.

The canonical output is CPU data first. A later texture encoder may pack samples into pixels/channels, but it must declare channel units, sample interval, quantization/error, interpolation, epoch, horizon, and out-of-range behavior. A channel packing convention must not silently reinterpret every channel as a different time slice unless that encoding is explicitly versioned.

## 8. Acceptance criteria

A capability is not “implemented” because a diagram or texture exists. It requires:
- explicit state, units, frames, initial/boundary conditions, and model validity;
- deterministic reference implementation and focused tests;
- conservation/error checks appropriate to the model;
- test cases against analytic solutions, published reference cases, or clearly labeled illustrative scenarios;
- provenance and reproducible build inputs for precomputed data;
- defined behavior for missing data, out-of-range queries, stale tables, and model changes;
- a clear contract separating authoritative physical outputs from rendering and cinematic controls.

## 9. Implementation sequence

1. Complete reusable material property, condition, provenance, and model-applicability contracts.
2. Add focused thermal phase-transition and brittle-failure reference cases with synthetic/illustrative parameters clearly marked.
3. Establish a small fluid free-surface reference case before dam-break and coastal inundation models.
4. Define world-time and orbital-input records; implement a deterministic analytical forcing baseline before generating any tide texture.
5. Bake and test a versioned CPU tidal table, including interpolation, time wrap/horizon behavior, digest invalidation, and semantic-state classification.
6. Add optical/radiative interfaces and explicitly separate renderer approximations from energy-coupled illumination.
7. Add higher-fidelity models only where requirements, data, and validation justify their cost.
