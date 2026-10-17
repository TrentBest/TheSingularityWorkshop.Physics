# Cross-LLM Coordination Log

This file is an append-only coordination record maintained from the Physics repository. Each entry records integration intent and boundaries; it does not claim that a capability exists until code and tests prove it.

## 2026-10-15 — Castle Siege / Destructive Physics Experience

**Owner:** Physics-side integration coordinator  
**Audience:** WebPageLLM, Physics implementation, and the Gestures integration where crew choreography is relevant  
**Target manifestation:** WebPage experience, composed through the Workshop's existing spatial/experience architecture  
**Status:** design brief and dependency map; no playable castle siege scene is claimed by this entry.

### User's intended first experience

Build a castle as the first visible target scene. Place the camera at the commander's viewpoint, just behind and slightly to one side of a trebuchet/catapult, looking toward the castle. The siege engine is a controllable physical apparatus, not a decorative prop. The visitor should be able to aim it, set the arm's pull-back/tension or release state, choose a launch angle, release a projectile, and then observe the resulting trajectory and impact on the castle.

The core value is an inspectable mathematical experiment: change one input or physical constant, run the same initial conditions, compare trajectories and damage, and understand why the outcome changed. Start with a single siege engine, one projectile, one castle wall section, and one fixed camera. Add breadth only after the first deterministic experiment is credible.

### Integration boundaries

- **Physics repository:** owns physical models, units, solver contracts, state evolution, event/history records, model validity, and testable damage outcomes. Keep the core renderer-independent and free of WebPage/Blazor types.
- **WebPage:** owns the scene manifestation, camera and input controls, user-facing explanation, plots/overlays, and lazy loading. Use the existing spatial composition/Experience architecture; do not invent a competing navigation or scene system.
- **Visualize / Renderer:** eventual rendering consumers, not dependencies to smuggle into Physics. Keep the initial adapter replaceable and do not assume browser GPU support without verification.
- **Gestures:** may provide ground-crew/commander/crew animation and interaction choreography. Gesture state must consume physics observations through a contract; Physics must not depend on Gestures.
- **FSM_API / FSM_COS:** schedule and compose process groups at their proper boundaries. The browser render cadence is not the physics clock. Do not claim the FSM scheduler integration is complete until the adapter and cadence tests exist.
- **Coordination policy:** record cross-repository intent in this Physics-side log. Do not edit WebPage's handoff documents as part of this coordination entry.

### Scene composition and camera

Initial scene contents:
1. Terrain and a simple approach/impact area.
2. A readable stone castle silhouette with curtain wall, at least one tower, battlements, and a gate or distinct wall section.
3. One trebuchet/catapult in the foreground.
4. A commander camera behind and offset from the siege engine, high enough to see the arm, projectile flight, and target wall in one composition.
5. A clear aiming reticle/arc and compact instrumentation: yaw/rotation, elevation angle, draw-back/tension setting, projectile mass, launch speed estimate, range/impact point, and current experiment/reality/timeline identity.

Keep the camera and decorative scene state separate from authoritative physical state. Camera movement must not change simulation results.

### First physics experiment: baseline before destruction

Start with a deliberately bounded projectile model and explicit SI units. For a first idealized test, record initial position, launch speed, elevation, azimuth, projectile mass, gravity, timestep, and solver/model version. With no air resistance and a level target plane, the baseline checks are:

- Horizontal components: v_x = v cos(theta) cos(phi), v_y = v cos(theta) sin(phi).
- Vertical component: v_z = v sin(theta).
- Flight state: integrate position and velocity under declared gravity and timestep; compare against the analytical constant-acceleration result.
- For equal launch/landing heights under ideal assumptions, the familiar range relation R = v^2 sin(2 theta) / g is a cross-check, not a universal answer for a raised or uneven castle target.
- State the limitations: the first projectile model does not automatically include drag, wind, projectile spin, sling release geometry, arm inertia, rope elasticity, counterweight motion, or contact with the target.

The actual trebuchet should become a second model stage: stored potential energy / counterweight or tension input, arm geometry and inertia, release condition, then measured projectile launch state. Do not fake a mechanical tension slider as if it were a validated trebuchet solver. If the mechanism model is not ready, label the initial control as an explicit launch-speed experiment or as a visual-only mechanism parameter.

### Destruction model: staged fidelity

Do not begin with unbounded full-castle rigid-body simulation. Introduce observable, testable increments:

1. **Impact accounting:** projectile mass and impact velocity determine kinetic energy (E = 1/2 m |v|^2) and momentum (p = m v); these are diagnostics, not by themselves a complete damage law.
2. **Wall section response:** declare material parameters and a bounded impact/damage rule, including thresholds and assumptions. Record impact location, normal, impulse/energy transfer estimate, damage state, and any model limitations.
3. **Failure and debris:** only after the wall response is tested, add fracture/deconstruction events and debris bodies. Keep failure detection distinct from topology mutation and from visual effects.
4. **Coupled effects:** material fracture, dust, sound, sparks, and heat are downstream consumers of supported physical events; visual effects must not masquerade as physical evidence.
5. **Replay:** capture initial conditions, parameter changes, ordered events, model versions, timestep, and checkpoints. Support deterministic rerun/compare before presenting a timeline scrubber as true physics replay.

For the first demo, a few authored wall sections with explicit material/structural parameters are preferable to claiming general masonry fracture. Every damage result should be explainable by the declared model and repeatable within its stated determinism contract.

### Experiment UI and constant exploration

Expose the controllable variables as instrumented inputs rather than hidden magic:
- siege-engine yaw / azimuth;
- elevation angle;
- draw-back or mechanism setting, only to the degree supported by the mechanism model;
- projectile mass and geometry preset;
- gravity and selected supported model constants;
- optional wind/drag only when those terms are implemented and validated.

Provide a baseline run, then allow one variable to change at a time and compare the resulting flight path, impact location, wall damage, and numeric measurements. Keep the reference run immutable; altered constants or inputs should create a distinct experiment/reality configuration or a child timeline as appropriate. Show units, bounds, model version, and assumptions beside each parameter. A changed constant is a changed model, not evidence that the alternate constant describes our physical universe.

### Connection to current Physics capability and gaps

The Physics repository already documents a renderer-independent constant-acceleration kernel and tested model-driven replay seams. Those are useful foundations for the first projectile experiment and later experiment history. The existing replay executors accept caller-supplied model restoration/advance operations; they do not constitute a ready-made world serializer, rigid-body engine, trebuchet solver, masonry fracture solver, or browser-playable scene.

The roadmap still identifies spatial/pose conventions, rigid-body and constraint solving, contact observations and response, structural failure, durable checkpoints/history, and a concrete physics-world reconstruction adapter as future work. The siege experience must be decomposed to fit those real contracts instead of implying these gaps are already solved.

### Proposed delivery gates

- **Gate A — Scene composition:** castle + siege engine + commander camera, with no destructive-physics claims; lazy-loaded in WebPage.
- **Gate B — Projectile math:** deterministic baseline launch inputs and trajectory; analytical/reference tests and units.
- **Gate C — Aim-and-compare:** azimuth/elevation controls, predicted arc, target intersection, baseline-versus-variant plots.
- **Gate D — First damage response:** bounded, explicitly labeled wall-section impact model with repeatability tests and visible damage state.
- **Gate E — Mechanism fidelity:** tested trebuchet energy/geometry/release model replaces any proxy draw-back control.
- **Gate F — Fracture/debris and replay:** only after the corresponding contact, topology, persistence, and reconstruction contracts pass tests.

At every gate, the page must distinguish implemented, experimental, illustrative, and planned capabilities. Controls must not report success for operations that are not implemented. No release or deployment is authorized by this note.

### Immediate coordination ask for WebPageLLM

Use the existing WebPage spatial composition architecture and its established Physics Replay / Moments integration boundaries. Treat this castle as a concrete new experiment/Experience that can eventually feed the same truthful timeline/replay system, not as a separate physics engine inside Razor. First report the best existing extension point and the smallest scene/camera prototype; then identify the adapter contract required to consume Physics outputs. Keep an illustrative fixture clearly labeled until a real Physics-backed run is connected. Leave publishing and deployment untouched.

### Immediate Physics-side next step

Prioritize a small, tested projectile experiment adapter over attempting general destructive physics all at once: explicit SI-unit initial conditions, constant-acceleration stepping, target-plane intersection, run metadata, and a reference comparison. In parallel, define the minimal material/wall damage contract without claiming a general rigid-body or fracture solver. Revisit the current roadmap before marking any stage complete.

---

## 2026-10-15 — Castle design and LLM handoff files

The full design is now in [`docs/CASTLES_AND_CATAPULTS.md`](CASTLES_AND_CATAPULTS.md): commander camera, scene composition, physics/material fidelity ladder, scientific labels, ownership boundaries, acceptance gates and the exact WebPageLLM ask.

- [`PhysicsCastlesLLM.md`](../PhysicsCastlesLLM.md) is the sole Physics-repository file WebPageLLM is authorized to update. It is an append-only response channel with concrete questions about WebPage extension points, scene files, Physics adapter, illustrative fixture labels, comparison UI, replay limitations and tests.
- [`PhysicsLLM.md`](../PhysicsLLM.md) requests a tested deterministic projectile experiment, target intersection and then a bounded material/wall-impact contract, with mechanism, fracture and replay gated on evidence.

Documentation/coordination only: no Physics implementation code or WebPage files changed, and no release/deployment occurred. The roadmap remains authoritative for capability status.

---


## 2026-10-15 — Castle Siege cross-repository design convergence

WebPageLLM has appended its design response to [PhysicsCastlesLLM.md](../PhysicsCastlesLLM.md). The WebPage-side integration brief is also present in [WebPage PR #115](https://github.com/TrentBest/WebPage/pull/115), with the castle-specific handoff at [CASTLE_SIEGE_PHYSICS_HANDOFF.md](https://github.com/TrentBest/WebPage/blob/work/singularity-labs-electricity-handoff/docs/integrations/CASTLE_SIEGE_PHYSICS_HANDOFF.md). PR #115 is a draft handoff/documentation PR, not evidence that the castle scene or Physics integration has been implemented.

### Design consensus to carry forward

- **Experience:** the commander’s shoulder/offset view behind the siege engine, with the engine, projectile path, and castle target readable together. Use WebPage’s existing spatial composition/navigation; do not invent a parallel scene architecture.
- **Physics authority:** Physics owns equations, deterministic samples, target intersection, impact observations, model assumptions, and validation. WebPage presents those observations and must not recreate projectile calculations in browser code.
- **First model:** treat launch speed as an explicit input/proxy. Use a documented right-handed z-up frame, azimuth in the x-y plane, elevation from the x-y plane, and SI units at the boundary. Confirm these conventions against the implementation before treating the adapter contract as final.
- **Experiment design:** preserve an immutable baseline; create a distinct run for each variant; change one supported parameter at a time; retain run/model/schema identity, exact inputs, assumptions, and output. Projectile mass may be exposed as an experimental parameter, but the UI must not imply it changes ideal trajectory when the model has no mass-dependent force.
- **Trust labels:** authored scene visuals can proceed before Physics integration. Trajectory/impact fixtures must be labelled “ILLUSTRATIVE FIXTURE — NOT PHYSICS-BACKED” until they consume actual Physics outputs. Target contact is computed from target geometry and path data, never inferred from visual overlap.
- **Destruction ladder:** impact diagnostics first; bounded, explainable wall response next; structural failure/debris only after their own tested contracts; validated trebuchet mechanics and authoritative replay are separate gated milestones. Energy and momentum are diagnostics, not a masonry damage law.
- **Replay:** baseline/variant result snapshots are acceptable before world persistence exists. Do not call them authoritative timeline replay or claim a reconstructable castle world until checkpoint persistence and deterministic restoration are implemented and tested.
- **Ownership:** WebPage owns scene, camera, supported controls, comparison UI, charts and presentation. Physics owns simulation. FSM_API/FSM_COS coordinate at their appropriate boundaries; Visualize/Renderer are downstream; Gestures may consume physical events but must not become a Physics dependency.

### Open coordination gates

1. Physics implementation LLM: report existing kernel/run-record/test extension points, then implement the deterministic projectile milestone tracked by [issue #23](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/23), or identify a concrete blocker. The final API/schema and tolerances must be grounded in that implementation and tests.
2. WebPageLLM: keep the scene/integration proposal aligned with the Physics contract; do not assume proposed type names are existing APIs or claim a Physics-backed scene before the adapter is usable.
3. Design coordinator: reconcile any disagreement in coordinate convention, target-intersection semantics, run identity, model capabilities, or integration ownership here before calling the contract stable.

This entry records design coordination only. It does not claim that the projectile model, castle scene, wall damage, mechanism solver, or replay restoration is implemented. No merge, publication, release, or deployment was performed by this update.

## 2026-10-15 — Castle Siege placement within Singularity City

The design now explicitly separates the **Workshop as reusable tooling** from **Singularity City as the persistent world** and **Castle Siege as a city attraction**. The visitor should travel to an experience, encounter the immersive siege scene, and have a clear path into the Workshop Lab to inspect and reuse the underlying experiment. This preserves the user's intended relationship: Workshop laboratories let people reuse Physics capabilities without binding them to the constraints or presentation of one venue.

### Proposed city concept

- **“Extreme Larping”** is a proposed name for a central downtown park/public gathering district. It is a design concept, not a registered or implemented WebPage destination.
- **Castle Siege** is a separate attraction, potentially located in or near that district and discoverable from the park, city wayfinding, events, or transit. Keep park and venue as separate place concepts so the park can host other activities later.
- The Medieval Times comparison refers only to the visitor pattern of travelling to a purpose-built attraction for a spectacle; it is not a branding or content-copy request.
- Digitens may attend, staff, and gather at these places when supported by City behavior, but crowd/economy simulation is not a prerequisite for the first physics-backed projectile slice.
- Existing WebPage SpatialPlaceCatalog.cs has separate SpatialDomain.Workshop and SpatialDomain.SingularityCity domains and currently includes Singularity Transit and Singularity Station. At inspection time it does not register Castle Siege or Extreme Larping. Proposed IDs such as city.castle-siege / castle-siege must be verified and registered by WebPageLLM, not assumed to exist.

### Visitor path and reuse

1. Discover the park/attraction through existing Explore, wayfinding, events, or transit architecture.
2. Resolve Castle Siege through the authoritative spatial place/Experience path; use shared city transit where appropriate and do not invent a parallel navigation system.
3. Arrive at the commander’s view and encounter the castle as an immersive, Physics-led experiment.
4. Offer an explicit route to “Explore this experiment in the Workshop Lab,” carrying the same run/model identity or input configuration rather than copying solver equations into WebPage.
5. Return a run summary/variant to the attraction only as a referenced experiment result. Do not claim it mutates persistent city/castle state without an implemented persistence contract.

### Ownership

- Physics owns equations, model state, authoritative run records, and observations.
- WebPage owns city-place registration, navigation, venue scene, camera, controls, presentation, and the handoff into/out of the Lab.
- Workshop Lab owns reusable experiment workflows and must consume the same Physics contracts as other experiences.
- FSM_API/FSM_COS remain at their proper state/composition boundaries; Visualize/Renderer are downstream consumers. Physics stays independent of WebPage and rendering.

### Follow-up gate

WebPageLLM should respond in the existing authorized append-only PhysicsCastlesLLM.md channel with exact Explore/SpatialPlace/Experience extension points, whether the park should be separately registered, how Castle Siege is discoverable via wayfinding/transit, and how a run configuration is handed to the Lab. The response must distinguish verified current paths from proposed registrations. No other LLM response file or WebPage repository file was edited here.

This entry records design intent only. It does not claim the park, venue, route, Lab handoff, Digitens attendance, or Physics-backed scene exists. No merge, publication, release, or deployment occurred.

## 2026-10-16 — Castle Siege domain expanded into an implementation-ready document set

The original Castle Siege showcase brief is now supported by three linked documents on `development`:

- [Castle Siege Physics Domain](CASTLE_SIEGE_PHYSICS_DOMAIN.md) defines the causal pipeline, domain identities, units/coordinate/time boundaries, fidelity ladder from authored scene through mechanism, wall response, debris, coupled effects and history, capability declarations, validation principles, architecture fit, and explicit non-goals for the first slice.
- [Castle Siege Experiment Catalog](CASTLE_SIEGE_EXPERIMENT_CATALOG.md) defines CS-00 through CS-12, from the authored venue and trusted constant-acceleration experiment to target intersection, impact accounting, baseline/variant studies, wall response, structural failure, mechanism modeling, multi-rate simulation, and authoritative restoration. Each entry states its question, inputs, outputs, gate, and acceptance evidence.
- [Castle Siege WebPageLLM Build Plan](CASTLE_SIEGE_WEBPAGELLM_BUILD_PLAN.md) gives WebPageLLM a host-side plan for the existing spatial Experience architecture, commander-view scene, control truth labels, Physics adapter seam, build slices, acceptance checklist, and explicit coordination request.

The README now links these as a coherent entry point. This is a documentation expansion and design contract, not a claim that the hosted scene, projectile adapter, trebuchet solver, damage/fracture models, or replay restoration are implemented. WebPageLLM remains the owner of WebPage source changes and should record its response only in the authorized append-only `PhysicsCastlesLLM.md` channel. Physics implementation remains responsible for grounding proposed contracts in actual types and tests.

**Next concrete gates:** confirm the existing kernel/run-record extension points; implement and validate the deterministic projectile/target-intersection slice; then build the WebPage scene against the actual contract. No merge, release, publication, or deployment was performed by this entry.


## 2026-10-16 — LUTs, ontology-defined payloads, aftermath, and versus siege

Castle Siege has been expanded beyond stone-only projectile tests. Two new documents define the next domain layer:

- [Physics and Math LUT policy](PHYSICS_MATH_LUTS.md) distinguishes mathematical/numerical tables, physical/empirical response tables, ontology/content profiles, and presentation/LOD tables. Physical tables require explicit units, provenance, valid ranges, interpolation/boundary behavior, error evidence, version/digest and run provenance. Presentation LUTs may not decide authoritative physical outcomes.
- [Castle Siege payloads and versus play](CASTLE_SIEGE_PAYLOADS_AND_VERSUS.md) specifies a data-driven machine/payload envelope, capability-based payload profiles, the causal impact-to-many pipeline, fragments and delayed settling, modular castle construction, alternating builder/attacker rounds, match reproducibility, and a staged path toward first-person immersion.

The user described Math and Physics as mutually informative: mathematical relationships help describe physical observations, while physical experiments motivate, constrain, and validate mathematical models. The architecture preserves that relationship without collapsing the ownership boundary: generic mathematical/numerical primitives may be provided by Math; Physics owns physical meanings, response models, validity limits and authoritative outcomes.

**Dependency discovery remains an explicit gate:** current context does not establish the exact Math MicroBundle repository/package or its current contract. Do not add a package/runtime dependency or invent a bundle identity. Identify the real artifact and inspect its types before proposing integration. Existing Physics kernel is currently a constant-acceleration experiment and run record; these docs do not claim that general LUT evaluation, soft-body/fracture, fluid dispersion, settling, castle building, multiplayer, or FPS support already exists.

**Implementation order:** (1) deterministic projectile and target intersection; (2) payload profile and machine eligibility contract; (3) confirmed contact and impact observations; (4) one defensible inert payload response; (5) bounded fragment initialization and settling; (6) independently addressable castle sections; (7) immutable alternating-turn match snapshots; (8) first-person presentation on the same authoritative state. Profile and table versions must be recorded for each run.

No WebPage source, release, merge, publication, or deployment was changed/performed by this documentation update.


## 2026-10-16 — Operation Sith: single active branch and preservation before cleanup

**Priority:** branch consolidation before further feature expansion.  
**Active integration branch:** `development`.  
**Stable branch:** `master`.  
**Policy:** do not create additional feature/work branches for this effort. Carry one active work branch only. Keep `main` and the existing `work/*` refs until their unique work has been classified and either preserved on `development`, deliberately rebuilt, or explicitly identified as disposable. This inventory is not permission to delete branches and is not evidence that their contents have been integrated.

### Branch audit

The GitHub branch search returned 25 refs: `development`, `master`, `main`, and 22 `work/*` branches. Comparisons against `development` show that `main` and `master` are each 157 commits behind the current development head. The work branches are diverged from development, with non-zero commits and/or distinct files; they cannot be assumed safe to delete merely because their names are temporary.

### Unique work that must be classified

- **Spatial and mechanics foundation:** `work/spatial-primitives`, `work/spatial-orientation`, `work/rigid-body-state`, `work/spatial-spring-damper`, `work/contact-observation-contract`, `work/sphere-sphere-narrowphase`, `work/sphere-normal-impulse-response`, `work/contact-positional-correction`, `work/axial-spring-damper`, and `work/axial-link-failure`. These contain overlapping stacks of vector/quaternion/pose/body-state, spring-damper, contact, impulse, positional-correction, and failure-monitor work. Integrate by dependency and tests, not by blindly merging each branch head.
- **Water and terrain:** `work/hydrodynamics-linear-wave`, `work/hydrodynamics-shallow-water-2d`, `work/hydrodynamics-bathymetry`, `work/hydrodynamics-wet-dry`, `work/volcanic-terrain-source`, and `work/stellar-physics-roadmap`. These overlap and stack wave, shallow-water, bathymetry, wet/dry shoreline, geology, domain-map, and roadmap material. Preserve the newest complete, validated slice for each capability and retain its tests and scope/limitation docs.
- **Domain planning and thermal state:** `work/physics-domain-map` and `work/lumped-thermal-state`. Preserve the domain atlas and the thermal primitive/test/documentation set if they remain part of the agreed Physics scope.
- **Electricity:** `work/electrical-grid-loads` contains an electrical grid model, tests, and a domain document.
- **Acoustics:** `work/acoustic-world-model` contains the listener-centric acoustic world-model document; `work/acoustic-free-field` includes an executable free-field acoustics baseline and tests as well as accumulated water/terrain files. The acoustics PRs are separate reviewable units; avoid treating the combined branch as one indivisible feature.
- **World-scale planning:** `work/stellar-physics-roadmap` also carries the stellar/planetary-energy roadmap; retain it only after reconciling the overlapping terrain/hydrodynamics stack.

### Open review queue observed during this audit

The repository has open draft PRs representing stacked increments, including PRs #3–#10 for spatial/mechanics/contact foundations, #11–#12 for the physics-domain map and thermal state, #13–#19 for wave/water/terrain, and #20–#21 for acoustics. Their bodies describe prerequisites and test scope. These PRs are evidence of review history and proposed changes, not proof that all work is merged or that every head is current. Before retiring refs, inspect each PR's latest head, checks, base, and dependency order; avoid merging stacked PRs in a way that duplicates or bypasses prerequisites.

### Sith consolidation sequence

1. Freeze branch creation: continue integration directly on `development`; do not open another work branch for convenience.
2. Preserve the branch/PR inventory and current documentation before any cleanup.
3. For each domain, compare the branch's unique file changes and commits against current `development`; inspect actual file contents and latest CI results. Branches that contain overlapping stacks must be consolidated in dependency order, with conflict resolution and tests.
4. Keep a capability only when its code, tests, documentation, and limitations agree. If a branch is stale or redundant, record why its unique work is already preserved or intentionally discarded before recommending deletion.
5. Reconcile `development` against `master` only through the normal reviewed promotion path. Do not force-reset the stable branch or merge all of development into master merely to make branch names align.
6. Once the preservation audit is complete, report the exact remaining refs and any unresolved risk to the repository owner. Branch deletion is a separate explicit cleanup action; this log does not perform or authorize it.

### Guardrails

- Do not trade away source, tests, docs, PR context, or useful implementation just to reduce the branch count quickly.
- Do not merge all diverged branch heads wholesale; repeated files and stacked ancestry make duplicate/conflicting changes likely.
- Do not create a replacement feature branch or a succession of temporary branches. `development` is the one active integration branch for this operation.
- Do not release, publish, deploy, or alter WebPage source as part of this branch audit.
- Castle Siege / payload / LUT documents are design intent only. The eventual relocation of that experience to an Experiences repository is a later operation; do not widen this cleanup into that migration.

This audit is based on live branch comparisons and the open-PR listing inspected on 2026-10-16. Branches were not deleted and no PR was merged by this entry.


## Operation Sith — integration and deletion-readiness audit (2026-10-16, pass 2)

### Result

- All unique implementation, test, and documentation slices from the stacked spatial/mechanics/contact, thermal, hydrodynamics, geology, stellar-roadmap, and acoustic branches have been ported to `development` or superseded by a newer equivalent already on `development`.
- The spatial/contact stack now includes `Vector3d`, `Quaterniond`, `Pose3d`, `RigidBodyState`, spatial distance spring-damper, `ContactObservation`, sphere-sphere narrowphase, bounded normal impulse response, and bounded positional correction. These are separate limited primitives, not a general rigid-body/articulated solver.
- Thermodynamics includes the bounded lumped thermal-state primitive. Hydrodynamics includes a 1D linear-wave reference, a flat-bed 2D shallow-water model, and a terrain-aware shallow-water model with bounded wet-dry shoreline behavior. Geology includes prescribed-volume volcanic deposition. Acoustics includes an analytical free-field observation baseline. Stellar physics remains a roadmap only.
- README links, domain atlas, roadmap, and scope documents were reconciled to distinguish implemented slices from future work. No Unity/WebPage dependency was introduced into the Physics core.
- The integrated Physics Build and Tests workflow passed at development commit `8f0341870ac5d498e10f5bf6576cbd335b87ef75` (run [#268](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/38966595047)).
- All pull requests in this work stack are now closed; five were merged (#2, #3, #5, #11, #20), and the remaining stacked PRs were closed after their unique changes were ported directly to `development`. No release or package publication occurred.
- No work branch has been deleted. This is intentional: the user will perform the final branch deletion after reviewing the inventory.

### Branch deletion readiness

The repository currently has 26 branches: `development`, `master`, `main`, and 23 `work/*` branches. A final recheck found `work/axial-mechanics-current` (already two commits behind `development`) and `work/axial-spring-damper` (its implementation/tests/docs match `development`; its roadmap is stale); both are also safe to delete. The `work/*` branches are now eligible for deletion after the user’s review; their unique slices are integrated or superseded on `development`.

**Important default-branch blocker:** GitHub still reports `main` as the repository default branch, and `master` is the intended stable branch under Sith policy. Before deleting `main`, change the repository default branch to `master` in GitHub Settings → Branches. This connector cannot change that repository setting or delete branch refs. Do not force-update `master` from `development`; no release/promotion was authorized.

### Remaining truthfully incomplete

- The code does not yet provide a general rigid-body time integrator, broadphase/contact manifold persistence, angular/frictional/multi-contact solving, rigid-joint reaction loads, or a complete articulated-mechanics acceptance scenario.
- Hydrodynamics remains a bounded shallow-water/linear-wave foundation, not full 3D fluid dynamics, breaking waves, rainfall/runoff, boat coupling, or erosion.
- Thermal transfer, phase change, material constitutive response, chemical reactions/combustion, erosion, stellar physics, and binaural/reflective/occluded acoustic propagation remain future work.
- Castle-siege and payload/versus integration remains documented future work; no Experiences migration was performed.


## 2026-10-16 — Equal-kit castle duel: arcade-readable, physics-honest

The competitive target is now explicitly **two builders, one shared parts kit, two castles, alternating attacks**. Each player independently designs a castle from the same versioned inventory and constraints. Both builds are locked before combat; players then take turns firing at the opponent's castle. Both sides get the same declared siege-machine/payload access and simulation budget unless a named asymmetric ruleset says otherwise.

The game should be approachable like an Angry Birds-style destruction game: fast aim/fire loop, clear arc preview, satisfying impact, and simple turn/result feedback. The underlying pipeline remains Physics-owned and staged: declared launch input → trajectory → confirmed contact → impact observation → supported target/payload response → aftermath. Early pull-and-release input may map to explicit launch speed/angle under a versioned game-control mapping; it must not be described as a validated trebuchet/sling mechanism. Arcade target scoring is not structural-collapse evidence.

The Physics-domain design is expanded in docs/CASTLE_SIEGE_PAYLOADS_AND_VERSUS.md with build snapshots, symmetric inventories, turn lifecycle, fairness, bounded work, authoritative result ownership and acceptance tests. docs/CASTLE_SIEGE_EXPERIMENT_CATALOG.md adds CS-13 through CS-15 for equal-kit construction, alternating-turn play and pull-control mapping.

**Boundary:** this is a design/acceptance update only. No playable match coordinator, multiplayer transport, construction UI, damage solver, or WebPage scene is claimed. Physics remains the authority for modeled outcomes; match rules/turn UI belong to the host Experience/coordinator. Continue working directly on development under Operation Sith; no new branch, merge to master, release, publication or deployment is authorized.


## 2026-10-16 — First executable 3D projectile step

**Status:** code and regression tests added on `development`; CI result pending at time of entry.

- Added `ProjectileState` and `ConstantAccelerationProjectileSolver` as a renderer-independent, 3D analytical kinematic step using `Vector3d`. Position, velocity, acceleration, and logical time have documented SI semantics.
- Added tests for a gravity-driven analytical state, repeated fixed steps versus one equivalent step, invalid elapsed time/timestep, and elapsed-time overflow.
- Updated the experiment catalog and roadmap to distinguish this implemented step from the still-missing launch-angle adapter, immutable trajectory runner/run record, target intersection, impact response, and structural damage.
- This is deliberately not a rigid-body integrator, collision detector, trebuchet mechanism, drag model, or playable castle duel. The next useful increment is a validated launch-state/trajectory experiment with samples and run identity; target geometry should follow as a separately tested gate.
- Scope remains Physics repository only. No WebPage, Gestures, Visualize, or Renderer files were changed. No new branch, merge, release, package publication, or deployment was performed.


## 2026-10-17 — Singularity Studios acoustic and production-data coordination

**Owner:** Physics-side integration coordinator  
**Status:** first evidence-backed acoustic baseline and distance sweep are implemented; studio production-data fixture and source-rendering integration are being verified in WebPage. This entry does not claim a room-acoustics solver or a completed studio simulator.

### Repositories and handoffs checked

- Physics: [PhysicsLLM.md](../PhysicsLLM.md), [acoustic world model](ACOUSTIC_WORLD_MODEL.md), [living acoustic demonstration](ACOUSTIC_LIVING_DEMO.md), [single-source observation](experiments/acoustic-observation.md), and [controlled distance sweep](experiments/acoustic-distance-sweep.md).
- WebPage: [Singularity Studios WebPageLLM handoff](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_STUDIOS_WEBPAGELLM_HANDOFF.md), [SFX/acoustics handoff](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_STUDIOS_SFX_WEBPAGELLM.md), and [production data model](https://github.com/TrentBest/WebPage/blob/development/docs/SINGULARITY_STUDIOS_PRODUCTION_MODEL.md).
- AnyApp: [Singularity Studios desktop handoff](https://github.com/TrentBest/AnyApp/blob/development/docs/integrations/SINGULARITY_STUDIOS_ANYAPP_HANDOFF.md); AnyApp and WebPage should consume the same versioned production fixture and semantics.
- Gestures: [Singularity Studios Gesture Plan](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/blob/development/docs/SINGULARITY_STUDIOS_GESTURE_PLAN.md).

### Shared contract

Physics is authoritative for modeled motion, contact, material outcomes, and acoustic observations. Gestures supplies reusable procedures and evaluated motion, but does not claim physical contact. WebPage/AnyApp own host interactions and presentation. Visualize/Renderer are presentation consumers, not Physics dependencies. Studio source data, camera placement, and SVG presentation must remain separate from the physical acoustic calculation.

The current free-field experiment computes source/listener distance, propagation delay, direction, and a distance-based SPL estimate. The distance sweep varies only listener distance and is generated from the C# model. These are analytical outputs—not audio samples, room reflections, absorption, occlusion, or a validated room response. The MDN sample remains explicitly reference audio.

### Verification and next gate

- Physics Build and Tests passed at [run 431](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/38995591642), commit `49e971af7cdd79a7cb8d4de0e4b55ec261acd941`.
- WebPage source-integration run [38995728981](https://github.com/TrentBest/WebPage/actions/runs/38995728981) exposed a missing project reference for the Visualize GUI.SVG backend in the Singularity Studios plan generator. The reference was corrected on `development` in commit `037be531894c2697b9fb8e99e31c12ee0ae36bb7`; the follow-up CI result still needs verification.
- Next Physics gate is a reviewed, bounded room-boundary and first-order reflected-path contract with explicit assumptions and tests. Only after that contract is defensible should room geometry be allowed to affect displayed measurements. Do not use an unvalidated reverb effect as a substitute.
- Preserve repository ownership and branch policy: work on `development`, leave `master` stable, and do not publish/release/deploy without explicit authorization.


## 2026-10-17 — first-order room reflection implementation

The bounded acoustic room-path increment has now moved from design to code on `development`:

- `AcousticRoomBox` defines a finite axis-aligned room and six frequency-independent pressure-amplitude coefficients.
- `FirstOrderRoomReflections` computes first-order image-source paths and returns each wall path separately.
- Focused tests cover a known reflected path, point/delay/level calculations, stable ordering, and invalid geometry/coefficients/positions.
- A C# run-record generator, committed JSON fixture, experiment note, and SVG renderer were added. CI checks the generated record and SVG against committed artifacts. The exact serialized fixture needed a formatting correction; the latest rerun must still confirm all artifact checks and the full test suite.
- WebPage's [Singularity Studios page](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Pages/SingularityStudios.razor) and [SFX integration brief](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_STUDIOS_SFX_WEBPAGELLM.md) now expose the room-path evidence with explicit non-waveform limitations.

This is not room audio. The direct and reflected paths are independent estimates; no phase-aware combination, multiple reflections, frequency-dependent absorption, scattering, diffraction, transmission, occlusion, reverberation, or waveform synthesis is implemented. Continue monitoring WebPage and AnyApp handoffs for new integration requests. No publication/release/deployment or promotion to `master` is authorized.
