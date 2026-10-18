# PhysicsLLM — implementation request: castle siege physics

**Audience:** Physics implementation LLM. **Scope:** Physics implementation and contracts only. This is not release/deployment authorization.

## Context

The castle/trebuchet experience is designed in [docs/CASTLES_AND_CATAPULTS.md](docs/CASTLES_AND_CATAPULTS.md). Current capability status is governed by [docs/ROADMAP.md](docs/ROADMAP.md). Existing work item: [issue #23 — deterministic projectile experiment](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/23).

## First milestone: deterministic projectile experiment

Inspect and extend the current renderer-independent constant-acceleration kernel and versioned JSON run record where appropriate. Do not duplicate existing abstractions without explaining why.

Implement and test:
- explicit SI-unit initial conditions: position (m), velocity/speed (m/s), mass (kg), gravity (m/s²), logical timestep (s);
- one canonical coordinate-frame convention, with azimuth/elevation semantics;
- deterministic position/velocity samples at logical ticks;
- target-plane/target-section intersection derived from path data and declared target geometry;
- run identity, model/schema version, exact parameters, timestep and assumptions in run record;
- analytical/reference comparison with stated numerical tolerance;
- invalid-input, angle, timestep, target geometry, target hit/miss and boundary tests;
- repeatability proving identical inputs/configuration yield identical results under the declared deterministic contract.

Keep core renderer-independent and free of WebPage/Blazor/Unity/Visualize types. Do not conflate logical simulation time with render frames. Avoid drag/wind/spin until their models are scoped and tested.

## Next milestone: bounded material/wall-impact contract

Do not leap directly to a general masonry fracture engine. Propose the smallest defensible wall-section response:
- stable target/body identity and target geometry/normal;
- explicit material identity, provenance, parameter units and valid ranges;
- impact observation separate from damage decision;
- explicit threshold/assumption and state transition for the first supported wall response;
- deterministic event/run record and tests for threshold boundaries, repeated impacts if supported, bookkeeping and limitations.

(E=\frac12m|v|^2) and (p=mv) are diagnostics, not a complete masonry failure law. If no defensible material/damage rule is ready, deliver the impact-observation contract and document the unresolved model decision rather than inventing authoritative stone properties.

## Gated follow-ons

1. Trebuchet mechanism: map draw-back/counterweight/tension to launch state using explicit energy, arm geometry/inertia and release assumptions. Until validated, WebPage controls must be labelled as launch-speed proxy or visual-only.
2. Structural failure/debris: separate failure detection, topology mutation, body creation and visual effects.
3. Coupled effects: heat/dust/sound only as consumers of supported events and energy transfers.
4. History/replay: connect to timeline seams only after authoritative checkpoint storage and concrete world reconstruction exist; state reconstruction limits.

## Deliverables

1. Report existing kernel/run-record/test extension points before duplicating types.
2. Implement the smallest complete projectile slice, or identify a concrete blocker and propose a narrow contract.
3. Add/extend tests and update roadmap only when code and tests support the status change.
4. Document coordinate conventions, units, tolerance, target-intersection semantics, limitations and run-record schema.
5. Identify model decisions/dependencies needing design-coordinator input.
6. Append dated progress/questions here or in issue/PR. Do not edit WebPage or its coordination file.
7. No publish/release/deploy without explicit authorization.

## Initial status

_No Physics implementation LLM response recorded yet._


## Follow-on design gates: Math/LUT, ontology payloads, and competitive siege

Use the following Physics-domain documents as the extended target, while implementing in evidence-backed increments:

- [Physics and Math LUT policy](docs/PHYSICS_MATH_LUTS.md): distinguish mathematical/numerical, physical/empirical, ontology/content, and presentation/LOD tables. Do not add a Math package/runtime dependency until the actual Math MicroBundle is identified and its contract inspected. Physical LUTs require valid ranges, units, provenance, interpolation/boundary policy, error evidence, version/digest, and run provenance.
- [Castle Siege payloads and versus play](docs/CASTLE_SIEGE_PAYLOADS_AND_VERSUS.md): model payload eligibility from machine envelope and explicit physical profiles, not asset-name strings; separate confirmed contact, impact observations, payload response, target response, fragment/body creation, settling, and presentation. The target domain includes delayed secondary movements and bounded settling/avalanche behavior when the requisite mechanics exist.
- [Castle Siege Physics Domain](docs/CASTLE_SIEGE_PHYSICS_DOMAIN.md) and [Experiment Catalog](docs/CASTLE_SIEGE_EXPERIMENT_CATALOG.md): remain the core capability and experiment gates.

Do not attempt a universal solver in one jump. Preferred sequence:
1. Finish and validate deterministic projectile plus target intersection.
2. Define the smallest payload-profile/machine-envelope contract and its eligibility tests.
3. Deliver confirmed contact and impact observations.
4. Implement one bounded inert payload-response case with explicit assumptions.
5. Add physical fragment identity/state and settling/rest criteria; bound entity count and work.
6. Add independently addressable castle sections and a bounded structural response.
7. Keep versus turn orchestration and match rules outside the Physics numerical kernel; provide immutable experiment/run snapshots for a future coordinator.
8. Record model, payload-profile and LUT/table versions/digests in run provenance wherever they influence results.

Animal-like ontology entries mentioned as possible user-authored content must not imply a biological injury model. Begin with inert test payloads and only claim the response fidelity actually implemented. Presentation particles/effects are never authoritative physical fragments. No release, merge, publication or deployment is authorized by this handoff.


## Operation Sith branch discipline — 2026-10-16

Operation Sith is the current priority. Work directly on the existing `development` branch as the sole active integration branch. Do not create another feature/work branch, even for an isolated implementation slice. Before touching an existing work branch, classify its unique code, tests, docs, CI evidence, and PR dependencies; preserve valuable work on `development` in reviewable increments. Do not blindly merge stacked branch heads or discard unique work to reduce the count. `master` remains the stable branch; promotion is a separate reviewed step. Branch deletion is not authorized by this note. Do not widen this operation into the later Castle Siege / Experiences migration, WebPage source edits, publication, release, or deployment.


## Operation Sith — consolidated implementation status (2026-10-16)

Work directly on `development` as the sole active integration branch. Do not create more work branches. The spatial/mechanics/contact, lumped thermal-state, hydrodynamics, geology, stellar-roadmap, and acoustic slices from the former stacked branches have now been ported or superseded on `development`; their focused tests are included. The integrated Physics Build and Tests workflow passed at `8f0341870ac5d498e10f5bf6576cbd335b87ef75` (run #268).

All PRs in the current work stack are closed. Do not reopen them merely to merge their full histories: several contain duplicated foundational files or branch-specific workflow edits. The unique implementation/test/doc files were ported directly and the current branch’s roadmap, domain atlas, and README were reconciled.

There are still 26 refs on GitHub: `development`, `master`, `main`, and 23 `work/*` refs. Final recheck found `work/axial-mechanics-current` already behind `development` and `work/axial-spring-damper` with implementation/tests/docs identical to `development` (only its roadmap is stale); both are redundant. The `work/*` refs are ready for user-reviewed deletion because their unique changes are integrated or superseded on `development`. Do not delete refs from this agent. GitHub still names `main` as the default branch; the user must switch the default to `master` in repository Settings → Branches before deleting `main`. Keep `master` stable; do not promote `development` or publish a package without explicit authorization.

Current scope is intentionally honest: the physics primitives are not a general rigid-body/articulated solver; hydrodynamics is not full 3D fluid dynamics; stellar physics remains documentation only; castle-siege payloads/versus remain a documented future integration and have not been moved to Experiences.


## Cross-repository watchlist — 2026-10-17

This is a coordination addendum, not a replacement for the castle-siege implementation request above.

### Singularity Studios and acoustics

- WebPage owns the Singularity Studios experience, stage-plan presentation, HTML audio controls, and evidence labels. Current handoffs: [Singularity Studios WebPageLLM brief](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_STUDIOS_WEBPAGELLM_HANDOFF.md), [SFX and acoustics brief](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_STUDIOS_SFX_WEBPAGELLM.md), and [reusable production model](https://github.com/TrentBest/WebPage/blob/development/docs/SINGULARITY_STUDIOS_PRODUCTION_MODEL.md).
- AnyApp is the desktop proving ground for the same reusable production data, not a second owner of studio semantics. Track its [Singularity Studios desktop handoff](https://github.com/TrentBest/AnyApp/blob/development/docs/integrations/SINGULARITY_STUDIOS_ANYAPP_HANDOFF.md) alongside WebPage's browser handoff.
- Gestures owns reusable, anchor-bound performer, crew, and equipment procedures. Its [Singularity Studios Gesture Plan](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/blob/development/docs/SINGULARITY_STUDIOS_GESTURE_PLAN.md) is a proposal, not evidence of an implemented studio production scheduler.
- Physics owns the physical observations and any validated acoustic source/propagation model. Current executable evidence is limited to the tested free-field observation and controlled distance sweep; both are analytical metadata, not waveform synthesis or room acoustics.
- Current Physics evidence: [acoustic observation](docs/experiments/acoustic-observation.md), [distance sweep](docs/experiments/acoustic-distance-sweep.md), [acoustic world model](docs/ACOUSTIC_WORLD_MODEL.md), and [living acoustic demonstration plan](docs/ACOUSTIC_LIVING_DEMO.md). The integrated Physics workflow passed at commit `49e971af7cdd79a7cb8d4de0e4b55ec261acd941` ([run 431](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/38995591642)).
- Next Physics acoustic gate: specify a bounded room-boundary representation and a first-order reflected-path model before implementing it. Declare geometry, reflection/absorption assumptions, path selection, units, validity bounds, and numerical/reference tests. Do not label a generic reverb effect or an illustrative room drawing as physical output.

### Cross-repository rules

- Keep the canonical physics implementation in Physics; WebPage and AnyApp consume versioned inputs/results and may render the same scenario but must not fork the numerical model.
- Keep the studio production data and its evidence labels separate from the acoustic solver. Camera views observe one scene; they do not clone scene state.
- Treat all current production/room-scale capacity statements as targets unless measured. Do not claim a GPU renderer, studio execution engine, room-acoustic solver, or Physics-generated audio where only an authored fixture/reference recording exists.
- Check `development` in Physics, WebPage, Gestures, AnyApp, Visualize, and Renderer for new `PhysicsLLM` / `WebPageLLM` / domain handoffs before changing a shared contract. Append coordination decisions here; do not edit another repository's handoff on its behalf.
- No merge to `master`, release, package publication, or deployment is authorized by this note.


## 2026-10-17 — first-order room reflection increment

Implemented on `development`:
- `AcousticRoomBox`: axis-aligned finite room bounds and six per-wall pressure-amplitude reflection coefficients.
- `FirstOrderRoomReflections.Observe`: image-source construction for first-order specular paths, rejecting source/listener positions outside or on the room boundary and excluding edge/corner hits.
- Focused tests for a known bounce point, path length, delay, per-path SPL estimate, deterministic wall ordering, and invalid inputs.
- Reproducible scenario: [run record](docs/experiments/first-order-room-reflection.run.json), [experiment notes](docs/experiments/first-order-room-reflection.md), and [SVG](docs/experiments/first-order-room-reflection.svg). The generator serializes current C# output; the SVG renderer uses that record, and CI compares the artifacts.
- WebPage's Singularity Studios page and SFX handoff now link to this evidence. AnyApp's [desktop consumer brief](https://github.com/TrentBest/AnyApp/blob/development/docs/integrations/SINGULARITY_STUDIOS_ANYAPP_HANDOFF.md) remains the desktop counterpart and should consume the same production data rather than fork the Physics calculation.

**Limitations remain explicit:** this is path metadata, not a waveform or room response. Direct and reflected levels are separate; there is no phase/interference, frequency-dependent absorption, scattering, diffraction, transmission, occlusion, multiple reflection, reverberation, or material-driven sound generation. Verify the newest Physics CI run before calling this increment green. Next, only after the reproducible record/SVG checks pass, consider a second reflection order or a frequency-banded material contract; do not combine path levels without a declared signal/energy model.


### Verification update — 2026-10-17

The bounded room-reflection increment is verified at [Physics Build and Tests run 38996272912](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/38996272912), commit `325e9db2eb70434b923c2b56edcf6f5f19c5a6f2`. The run passed the focused test suite, generator/fixture comparison, SVG render, exact SVG comparison, and SVG-against-record check. Later commits in this sequence update documentation/coordination only. Keep watching the newest development run for the final branch tip.


## 2026-10-17 — Sound Experiences / MicroBundle coordination

The Experiences repository now has a design proposal for composable sound-related Experience ingredients: [Sound Experiences — MicroBundle Design](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/blob/development/experiences/Sound/README.md). It is a taxonomy and contract proposal, not a claim that sound MicroBundles or an audio engine are implemented.

Boundary for Physics:
- Continue to own physical acoustic source/propagation/receiver models and reproducible results only where implemented and validated.
- Do not make Physics depend on Experiences, WebPage, AnyApp, Gestures, audio devices, Visualize, or Renderer.
- An orchestra, choir, band, instrument, score, venue, microphone, and listener are distinct semantic roles; the Experiences definition should compose them without turning each into a separate Physics model.
- An event-to-source mapping may describe how a supported collision/impact/fracture event requests sound, but the event label itself is not a waveform.
- Current first-order room reflections remain path metadata, not synthesized or mixed room audio. Do not report an authored sample or host playback as Physics-generated sound.
- Before any Physics implementation expands, coordinate the proposed source-excitation, environment, and receiver contracts with the existing [acoustic world model](docs/ACOUSTIC_WORLD_MODEL.md). Prefer one small, testable contract over a broad unvalidated solver.

The Experiences design asks the integration coordinator to inspect the existing MicroBundle descriptor/artifact contracts before defining a parallel schema. WebPage and AnyApp should eventually consume the same versioned Experience definition; neither should privately fork ensemble semantics or Physics calculations. No release, merge to master, publication, or deployment is authorized.


## Domain-wide Math / Chemistry / Combustion direction — 2026-10-16

The owner has assigned Physics as the home for the domain's physical and mathematical foundation. Read and follow [Physics + Math Domain Expansion](docs/PHYSICS_MATH_CHEMISTRY_COMBUSTION.md) alongside the existing [LUT policy](docs/PHYSICS_MATH_LUTS.md) and capability catalog. This is design direction, not a claim that these capabilities exist.

Priorities beyond the current Operation Sith integration work:
- inventory existing math/numerical primitives and repository/MicroBundle providers before creating a standalone Math package or adding dependencies;
- evaluate deterministic Squirrel3 RNG and convenient typed sampling helpers with golden vectors, bounded-integer correctness, replay metadata, and keyed per-entity/event streams;
- prefer justified LUTs with provenance, validity, interpolation/boundary policy, error bounds and content digest;
- start chemistry with a scientifically labelled, sourced H2 potential-energy curve and two-hydrogen/bond visualization contract; do not depict fixed classical electron orbits as quantum truth;
- design Burning Engine around explicit fuel, thermal-energy/temperature, and oxygen state, stoichiometric reaction accounting, and conservative mass/energy transfer before GPU texture optimization;
- start gas with a well-mixed compartment, then bounded transport; extend existing shallow-water foundations for conservative liquid/tracer mixing;
- require actual code, focused tests, and living visualizations generated from model output before updating capability status.

Keep working on the existing development branch; do not create more work branches. No merge to master, publication, release, deployment, or edits to unrelated repositories are authorized by this note.

## RNG audit — 2026-10-17

A source audit found that the first RNG slice already exists; do not recreate it or treat it as future-only work:

- `Physics/Math/Squirrel3Random.cs` defines `Squirrel3.Hash(uint position, uint seed)` and a stateful `Squirrel3Random` stream.
- Existing helpers cover unsigned values, `[0,1)` doubles, ranged doubles, half-open bounded integers using rejection sampling, booleans, probability checks, selection, Fisher–Yates shuffle, and deterministic stream forks.
- `tests/TheSingularityWorkshop.Physics.Tests/Math/Squirrel3RandomTests.cs` has five golden vectors plus repeatability, bounds, invalid-input, shuffle, selection, and fork tests.
- Current namespace is `TheSingularityWorkshop.Physics.Numerics`; retain compatibility unless an explicit API change is justified.
- The hash and stream are deterministic non-cryptographic tools. The stream is call-order-dependent; `Fork(streamId)` is a seed-derived stream, not yet a fully specified world-seed/entity-ID/event-counter keyed random-access contract.

### Audit conclusion and next checks

1. Verify the golden vectors against a canonical upstream/reference implementation and record exact attribution/licensing before making stronger statistical claims.
2. Add statistical sanity tests only as diagnostics, not as proof of quality. Keep cryptographic use explicitly out of scope.
3. Evaluate a keyed API for stable entity/event/tick sampling that is independent of unrelated entities and call ordering; specify key mixing and collision limits before exposing it publicly.
4. Add typed geometric sampling (including `Vector3d`) only with distribution tests and clear contracts; a normalized random vector is not automatically uniform over a sphere.
5. Inspect the existing numerical/LUT contracts before creating any Math package. The repository already contains `LookupTable1D`; a package split is not justified merely by the existence of a `Physics/Math` directory.

The latest development-head Physics Build and Tests workflow was successful at commit `c55edcd91d297ecd5276446e21add550fa358df2` ([run #566](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39006351119)). That verifies the current head, but does not replace targeted validation when code changes. Continue directly on `development`; no release, master promotion, or unrelated-repository edits are authorized.



## New follow-on: material-driven surface combustion and element data

**Priority:** after preserving the current projectile/material work, establish the first tested combustion reference slice. This is a Physics-domain task, not a WebPage, renderer, or MicroBundle runtime task.

Contracts already added on `development`:
- [`ChemicalElementDefinition`](Physics/Chemistry/ChemicalElementDefinition.cs): atomic identity plus explicit source/version/URL/license attribution. It intentionally does not conflate element identity with isotope mass, standard atomic weight, or bulk material properties.
- [`SurfaceCombustionCellState`](Physics/Combustion/SurfaceCombustionCellState.cs): stable cell/material identity, area, fuel mass, oxygen inventory, thermal energy, and unit surface normal. This is a state contract, not a solver.
- [Surface Combustion Model](docs/SURFACE_COMBUSTION_MODEL.md): ownership, field semantics, physics invariants, deterministic variation, and staged acceptance gates.

Implement next, in order:
1. Identify and document a legally usable, source-attributed element dataset before importing all 118 elements. Do not invent atomic weights or copy a dataset without checking its license and provenance. Keep the dataset distinct from the identity contract so providers can contribute updated versions. A condition-qualified property fact contract now exists at `Physics/Chemistry/PropertyValueRecord.cs`, with status states, bounds, units, conditions, uncertainty, evidence class and per-fact provenance. Its current validation deliberately does not implement source import, property-condition matching, conflict arbitration, or a populated database. Extend this contract with tests before adding bulk records.

Follow [Element and Material Data Architecture](docs/ELEMENT_AND_MATERIAL_DATA_ARCHITECTURE.md): keep element identity, nuclides, condition-qualified property facts, material definitions, reaction profiles, and models separate; support core and optional specialist profiles (including nuclear data); distinguish unknown, not applicable, omitted, and out-of-range. Implement the typed schema before bulk population, and review redistribution terms before importing data.
2. Add material/reaction-profile contracts only as needed for the first balanced, named reaction. Record reactants/products, stoichiometric coefficients, heat-release basis/units, source/version, validity range, and evidence status. Do not hard-code a generic “wood burns” reaction or treat a display name as a material model.
3. Implement a single-cell reaction-accounting reference model with explicit time step and a declared reaction-rate assumption. Bound reaction extent by the limiting reactant. Test non-negative inventories, stoichiometry, heat bookkeeping, limiting-reagent transitions, invalid inputs, and exact-input determinism. If empirical inputs are not yet sourced, label fixtures illustrative and keep the model scope narrow.
4. Add a surface-source/ignition slice that transfers energy from a reusable flame source into a target surface through a declared transfer model. Do not expose a `LightCandle()` shortcut that bypasses physics; candle, paper, and wood should be data-driven cases only where supported inputs and models exist.
5. Keep the proposed fuel, oxygen, and thermal textures as logical Physics fields first. A texture is a storage encoding, not the scientific contract. Validate CPU reference state before any GPU implementation. Track surface normals and coordinate frame explicitly; gravity/buoyancy, conduction, radiation, and wind transport are distinct mechanisms.
6. Use existing `Squirrel3`/`Squirrel3Keyed` only for deterministic variation with stable cell/tick/purpose keys. Randomness must not create fuel/oxygen/energy or mask failed accounting.
7. Add focused tests and update the [capability catalog](docs/PHYSICS_CAPABILITY_CATALOG.md) only to describe behavior proven by code and tests. Do not edit WebPage or any renderer integration; coordinate a later visual consumer through the existing owner/coordination document.
8. No package publishing or release without explicit authorization.

The target is a physically inspectable model whose flames are emitted from solved heat/reaction state, not a script or shader that merely looks plausible. Preserve the distinction between authoritative simulation and presentation particles.


## 2026-10-18 — world time and CPU orbital-forcing table

Implemented on `development`:
- `Physics/Astronomy/WorldTimeDefinition.cs` defines a world-local rotation period and epoch, with local rotation phase/cycles and a separate conventional Earth-reference day conversion.
- `Physics/Astronomy/OrbitalTidePotentialBaker.cs` bakes deterministic CPU samples for a bounded single-perturber model: circular orbit, equatorial observer, caller-supplied SI gravitational constant, orbital/rotation periods, phase, radius, mass, and distance. Each sample stores differential tidal potential (m²/s²), its analytical time derivative (m²/s³), and the derivative state `Rising`, `Falling`, or `TurningPoint`.
- Focused NUnit tests cover time conversion, determinism, endpoints, interpolation, bounds rejection, sample immutability, aligned turning points, and invalid input.
- The physical-effects design contract and capability catalog now distinguish this implemented reference slice from the planned local ocean-response model.

Important boundary: this is not a sea-level model and does not predict `KingTideIncoming`/`Outgoing`. The next physics layer should compose orbital forcing from all relevant perturbers, then apply a declared planet/ocean/basin response model. Only that response can support local sea-level extrema and semantic tide states. Texture encoding follows only after the CPU table is the canonical tested output; define units, quantization error, epoch, sample spacing, interpolation and out-of-range policy before assigning pixel channels. Test constants are illustrative, not a sourced world dataset. No package publication or release is authorized.


## 2026-10-18 — Marble Run and Domino Studio experience handoff

Design contract: [Marble Run and Domino Studio](docs/MARBLE_RUN_AND_DOMINO_STUDIO.md).

Ownership boundary:
- Marble Run and Domino Studio are experiences consuming Physics, not Physics MicroBundles. Their scene construction, image interpretation, image-to-layout conversion, editing tools, camera, presentation, and run controls belong to the experience/consumer layer.
- Physics owns shared physical state, geometry/collision contracts, contact observations and response, deterministic stepping, and measurable results. Keep Physics free of renderer, UI, image-processing, Unity, WebPage, and Experiences dependencies.
- Audit and reuse current rigid-body/contact primitives before creating duplicate types. Existing contact support is bounded; do not describe it as a complete frictional/angular multi-body solver.
- Build a CPU reference vertical slice first: falling sphere and standing box/domino, then supported contact/impact response, then small marble-track and two-domino fixtures. Add friction, resting contacts, scaling and acceleration as individually tested increments.
- For Marble Run, thousands of marbles is a benchmark target, not a promise. Record hardware, timestep, contact density, wall-clock cost, solver iterations and physical error diagnostics.
- For Domino Studio, image conversion creates an editable proposal, not uniquely inferred 3D truth. Save source digest, conversion version/parameters, units, coordinate frame, generated placements and user edits. The first push initializes a physical event; all later toppling must emerge from supported dynamics, and failed propagation is valid.
- Add no experience-specific MicroBundles to Physics and no renderer/image pipeline dependencies. Update capability claims only when code, focused tests and reproducible evidence exist.
- No package publication, release, master promotion or deployment is authorized.


## 2026-10-18 — contact capability MicroBundles

Added the separate project `TheSingularityWorkshop.Physics.MicroBundles.csproj` with two thin, typed adapters around existing tested core capabilities: sphere-sphere contact observation (bundle 5201) and translational normal impulse response (bundle 5202). Focused tests verify stable provider IDs, geometric evidence without invented force/impulse values, and wrapper behavior against the core resolver.

This is intentionally not a complete marble/domino engine. It does not add box/domino contact, rigid-body integration, friction, angular impulse, resting contacts, or a provider registry. Provider IDs are declarations; the host must resolve typed instances explicitly. Next implement and test the CPU rigid-body vertical slice in core Physics before building higher-level bundles or claiming the experiences run physically. The companion consumer contracts are in [Experiences](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/blob/development/docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md). No publication or release is authorized.
