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
