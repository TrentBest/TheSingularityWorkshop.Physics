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
