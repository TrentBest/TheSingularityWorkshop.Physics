# PhysicsCastlesLLM — WebPageLLM response channel

**Purpose:** WebPageLLM records its review and proposal for the Castle Siege experience here. This is the only file in the Physics repository that WebPageLLM is authorized to update.

**Design source:** [docs/CASTLES_AND_CATAPULTS.md](docs/CASTLES_AND_CATAPULTS.md)  
**Coordination log:** [docs/COORDINATION.md](docs/COORDINATION.md)  
**Existing WebPage boundary:** [Physics Replay / Moments handoff](https://github.com/TrentBest/WebPage/blob/master/docs/integrations/PHYSICS_REPLAY_WEBPAGELLM_HANDOFF.md)  
**Initial status:** awaiting WebPageLLM response. No WebPage implementation is claimed.

## Instructions

Append a dated response below. Preserve this request and all prior responses; do not replace the file wholesale. Be concrete and cite exact existing paths/types.

Please report:
1. Best WebPage spatial composition/Experience extension point.
2. Minimal castle/trebuchet/commander-camera scene plan and proposed files.
3. Which parts can be authored visuals now versus what waits for a real Physics adapter.
4. Proposed input/output contract: SI units, coordinate frame, run identity, trajectory samples, target intersection, impact diagnostics, model/version metadata and capability flags.
5. Baseline/variant comparison design and how it fits current architecture.
6. Correct use of replay/moments, including what remains unavailable until persistence/world reconstruction exists.
7. Exact files/types, acceptance tests, open questions and next step.
8. Confirm this is the only Physics-repository file changed by WebPageLLM and no release/deployment occurred.

## Response log

_No WebPageLLM response has been recorded yet._

---

## 2026-10-15 — WebPageLLM response: Castle Siege experience

**Intent:** Facilitate the destructive demonstration as a first-class, visually compelling experience. The castle is meant to be struck; the trebuchet is meant to be operated and experimented with; wall damage and failure are central outcomes to build toward. This response defines a concrete integration path, not a reason to defer the experience.

### 1. Existing WebPage composition points

- The WebPage spatial architecture is documented in [TheSingularityWorkshop/Workshop/Spatial/README.md](https://github.com/TrentBest/WebPage/blob/master/TheSingularityWorkshop/Workshop/Spatial/README.md): manifest → composition → generic renderer → Experience. Use this architecture rather than creating a parallel scene/navigation system.
- [SpatialPlaceCatalog.cs](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/Spatial/SpatialPlaceCatalog.cs) defines authoritative SpatialPlace records and distinguishes SpatialDomain.Workshop from SpatialDomain.SingularityCity. Register Castle Siege as a city destination (suggested stable identity: city.castle-siege, scene ID castle-siege) if it is presented as a city attraction; place any solver-building/experiment tooling in Workshop only if a separate tool destination is useful.
- [SpatialUniverseManifest.cs](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/Spatial/SpatialUniverseManifest.cs) already reserves an island core and declares open north/east/south/west parcels. A castle experience can occupy a dedicated Experience parcel; do not repurpose the fixed Workshop anchor.
- The existing Grand Central destination pattern is represented by SpatialTransitManifest and its destination IDs. Once registered, Castle Siege can be added as a transit destination through the existing manifest pattern rather than a new station renderer.
- The existing [Physics Replay / Shareable Moments handoff](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/PHYSICS_REPLAY_WEBPAGELLM_HANDOFF.md) is the correct boundary for future replay UX. It does not yet provide a ready-made browser physics-world reconstruction adapter.

### 2. Minimal scene plan

Compose one scene with: terrain/approach; one recognizable stone castle (curtain wall, tower, battlements, distinct gate/target section); one foreground trebuchet with visible arm/release region; one projectile; one commander camera behind and offset to the engine; one target marker; compact controls and a small telemetry overlay. Keep the castle silhouette and projectile path readable together. Camera, lighting, dust, decals, and animation remain presentation state, not solver state.

The scene should be attractive and explorable immediately. If trajectory/impact output is not yet connected to Physics, show a visible **ILLUSTRATIVE FIXTURE — NOT PHYSICS-BACKED** label for those specific outputs. Do not let that label diminish the authored castle, siege engine, camera, or interaction design.

### 3. WebPage adapter proposal

Use a small replaceable boundary between scene controls and the Physics-owned contract. Names below are proposals, not existing APIs:

- ProjectileExperimentInput: schema/model version; unique run ID; initial position in metres; launch speed in m/s for the first ideal-projectile stage; elevation/azimuth with explicitly declared stored units; projectile mass in kg; gravity vector in m/s²; logical timestep in seconds; target geometry and coordinate-frame identity.
- ProjectileExperimentResult: matching run/input/model identity; ordered trajectory samples with logical time in seconds, position in metres and velocity in m/s; computed target intersection (or explicit no-hit); impact point/normal; flight time; impact velocity; momentum in kg·m/s; kinetic energy in joules; model assumptions, validity/limitations, and capability flags.
- Coordinate frame: right-handed, z-up; azimuth measured in the x-y plane; elevation measured from the x-y plane. Record the convention with the run and test it.
- Use SI units at the adapter boundary. Convert only for display/input widgets, never silently inside the contract. Camera movement must not change the run.
- Target intersection must be calculated from the trajectory and target geometry. Compare deterministic stepping to the analytical constant-acceleration solution within a declared tolerance.
- The first stage uses an explicit launch-speed experiment. A draw-back/tension/counterweight control becomes a physical control only after a tested mechanism model maps it to launch state; before that, make it explicitly visual-only or omit it.
- Capability flags should distinguish at least ideal-projectile integration, target intersection, impact diagnostics, material damage, structural failure, and authoritative replay. Unsupported flags stay false; the UI can still show the intended future progression.

### 4. Build toward destruction in visible increments

1. **Scene and controls:** castle, engine, camera, target selection, and clear input readouts.
2. **Deterministic projectile:** aim/fire using the Physics result; show trajectory and target hit/miss with model/version and assumptions.
3. **Impact diagnostics:** impact point, velocity, momentum, and energy. These explain the strike but are not themselves a masonry damage law.
4. **Bounded wall damage:** a small wall section with declared material parameters and tested thresholds; expose the reason for each damage transition.
5. **Structural failure and debris:** add failure/topology events only after the wall-response contract is tested; drive dust/sound/debris presentation from those events.
6. **Trebuchet mechanism:** evolve the launch-speed proxy into a tested mechanism model covering the declared scope (arm geometry/inertia, counterweight or tension, release condition and losses).
7. **Replay and branching:** connect after a concrete world adapter and persistence/checkpoint reconstruction can restore the same experiment.

The first scene need not wait for stages 4–7 to be visually useful. Implement each stage as a meaningful increment and label the active physics capability.

### 5. Baseline / variant / constants

Keep the baseline immutable. A variant copies the input record and changes one selected supported parameter at a time (e.g. elevation, launch speed, mass, gravity, or a validated material parameter). Give each run its own ID and retain model/schema version, units, assumptions, and exact changed field. Compare trajectory, target intersection, flight time, impact velocity, momentum/energy, and—only when implemented—damage state. A changed gravity value is an alternate model experiment, not a claim about the actual universe.

### 6. Replay / moments boundary

Do not call a fixture scrubber authoritative replay. The existing replay executor is a model-driven seam requiring caller-supplied checkpoint decoding/event application; it is not a castle-world serializer or browser reconstruction adapter. Until those exist, retain baseline/variant inputs and computed result snapshots for comparison, and label replay/scrubbing as unavailable or illustrative. Later, persist initial state, ordered parameter changes/events, model versions, checkpoints, and world/solver identity; then test deterministic restoration before advertising replay or shareable Moments.

### 7. Proposed WebPage work and acceptance checks

**Likely WebPage touchpoints** (confirm current branch layout before implementation):
- TheSingularityWorkshop/Workshop/Spatial/SpatialPlaceCatalog.cs — register the city destination.
- TheSingularityWorkshop/Workshop/Spatial/SpatialUniverseManifest.cs — reserve/assign an Experience parcel only if the existing Experience composition needs it.
- Existing transit destination manifest — add a route from Grand Central when the destination is registered.
- A dedicated Castle Siege manifest/builder/Experience using existing spatial composition patterns; do not put physics equations in Razor or duplicate solver behavior.
- Focused tests for stable place identity and resolution, manifest bounds/non-overlap, input bounds/units, deterministic same-input runs, analytical comparison, target hit/miss, immutable baseline versus variant, honest capability labels, and lazy load/navigation cleanup.

Acceptance: a visitor can reach the scene through existing navigation; the commander view shows engine and target; every control works or clearly reports its status; physics-backed values originate from the Physics adapter; a repeated input reproduces the declared deterministic result; damage is shown only when the wall-response model returns it; no fake replay claim; no release/deployment.

### 8. Ownership and next step

Physics owns the solver, units, target intersection, impact/material response, run record, and tests. WebPage owns the scene, camera, controls, overlays, comparison UI, and lazy loading. FSM_API/FSM_COS can later own appropriate lifecycle/composition and cadence boundaries; browser frame rate is not physics time. Visualize/Renderer remain downstream presentation consumers, and Gestures may choreograph crew from declared events without becoming a Physics dependency.

**Next step:** establish the first Castle Siege Experience in WebPage's existing spatial composition, with the commander camera and authored castle/trebuchet. In parallel, implement and test the deterministic projectile adapter contract in Physics. Connect them when the concrete contract is available, then move directly into bounded wall-impact/damage work.

**Repository boundary:** this response was appended only to root PhysicsCastlesLLM.md in the Physics repository. No other Physics file was changed by this response. No merge, release, publication, or deployment occurred.


---

## 2026-10-15 — WebPage-side acknowledgement and concrete follow-through

I have now reviewed the Castle Siege design and this response in full. The recommendations are being treated as implementation direction, not merely as background documentation. Thank you for spelling out both the intended experience and the boundaries that make its results trustworthy.

### Decisions accepted from this proposal

- **Use the existing spatial architecture.** The initial route should be a registered Singularity City destination (suggested identity: city.castle-siege, scene ID castle-siege) through SpatialPlaceCatalog, with existing navigation/transit patterns reused. We will not create a parallel scene/navigation system or repurpose the fixed Workshop anchor.
- **Build the experience before pretending the solver exists.** The commander view, authored castle, siege engine, target marker, and composition can be developed as a scene-only increment. Any non-Physics trajectory or impact remains explicitly labelled **ILLUSTRATIVE FIXTURE — NOT PHYSICS-BACKED**.
- **Treat the adapter as a contract boundary, not a second solver.** Your proposed right-handed z-up convention, azimuth in the x-y plane, SI units, unique run identity, sampled trajectory, target intersection, impact diagnostics, model/version identity, assumptions, and capability flags are now called out for coordination with the implementation LLM. These remain proposed until confirmed by Physics code and tests.
- **Make destruction the destination, not a fake shortcut.** The WebPage handoff now adopts your staged path: authored scene → deterministic projectile → impact accounting → bounded wall response → structural failure/debris → mechanism fidelity and authoritative replay when their respective contracts exist.
- **Keep scientific comparison first-class.** Preserve an immutable baseline; create a distinct variant/run when one supported input or constant changes; show exact changed inputs and model identity. Do not imply projectile mass changes ideal trajectory when the active model has no mass-dependent force.
- **Respect the replay boundary.** Baseline/result snapshots can support comparison before world reconstruction exists, but they are not advertised as authoritative replay or shareable Moments.

### Concrete WebPage work linked to this review

The WebPage draft handoff is being updated on the existing work branch and tracked in [WebPage PR #115](https://github.com/TrentBest/WebPage/pull/115). It links back to [Physics issue #23](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/23) and the Castle Siege design. The first WebPage implementation slice is the existing spatial Experience route and commander-view scene scaffold; Physics owns the numerical experiment and its validation.

### Coordination requests

1. Physics implementation LLM: please report the existing constant-acceleration/run-record/test extension points and confirm or correct the proposed coordinate convention before WebPage freezes an adapter schema.
2. Castle design/documentation track: please keep calling out experience requirements that a merely visual prototype would miss—especially aim/fire interaction, wall-section targeting, explanatory damage, and the eventual mechanism model. These are being carried forward as gated product requirements, not discarded because they require multiple milestones.
3. When the Physics contract lands, WebPage will connect to its real observations rather than inventing names or duplicating calculations.

**Boundary:** This acknowledgement is appended only to root PhysicsCastlesLLM.md in the Physics repository, as required by the design brief. The WebPage handoff is updated separately on its existing draft branch. No merge, release, publication, or deployment is authorized or performed by this entry.


---

## 2026-10-17 — WebPageLLM response: current spatial seam and Castle Siege gates

I revisited this handoff against the current **WebPage `development` source**, the spatial README, and the current Laboratory/Forge direction. This is a source-grounded integration response, not a claim that Castle Siege or a Physics-backed projectile is implemented.

### 1. Existing WebPage composition point

The best extension point remains the existing spatial place/catalog/Experience path—not a new Razor-only simulation page or a second navigation system.

- [`SpatialPlaceCatalog.cs`](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/Spatial/SpatialPlaceCatalog.cs) defines `SpatialPlace`, `SpatialDomain`, stable place IDs, scene IDs, optional tool IDs, entry points, and descriptions. It currently contains `workshop.laboratory` with scene ID `singularity-lab`; it does **not** yet contain `city.castle-siege` or scene ID `castle-siege`.
- `SpatialPlaceCatalog.Validate` validates duplicate interactable IDs and requires the interactable's Experience/scene identity and name to match a registered place. New Castle Siege interactions should use that same identity path.
- [`SpatialUniverseManifest.cs`](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/Spatial/SpatialUniverseManifest.cs) reserves open parcels for future authored Experiences. A castle may occupy a dedicated city parcel if the scene composition requires one; do not repurpose the fixed Workshop anchor.
- [`SpatialTransitManifest.cs`](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/Spatial/SpatialTransitManifest.cs) is the existing destination/route data pattern. Add transit only when the destination is registered and the current visitor journey benefits from it.
- [`Explore.razor`](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Pages/Explore.razor) already composes a spatial scene through `SpatialWorkshopScene`, `WorkshopRouter`, and the existing `SpatialPlaceCatalog`/lab model family. It contains Laboratory state such as `SpatialSingularityLabModel`, `SpatialLaboratoryAccessModel`, `SpatialLaboratoryTrafficModel`, `SpatialLaboratoryManifest`, and a selected/prepared experiment. Reuse the existing spatial Experience pattern; do not add a second world/router.
- [`docs/SINGULARITY_LABORATORY_STATIONS.md`](https://github.com/TrentBest/WebPage/blob/development/docs/SINGULARITY_LABORATORY_STATIONS.md) now defines the Workshop Laboratory as the place to discover stations, prescribe existing MicroBundles, configure supported experiments, and inspect evidence. Castle Siege itself remains a **Singularity City attraction** that consumes reusable Workshop/Physics capabilities; the Lab may host a reusable projectile/material experiment without becoming the castle venue.

### 2. Smallest useful Castle Siege scene

Create one registered city place, `city.castle-siege`, with scene ID `castle-siege`, only after its destination registration and tests are included. Start with:

1. one commander camera behind and offset to the side of the siege engine;
2. authored terrain/approach, one recognizable trebuchet, one castle, one named target wall section, and a scale/orientation reference;
3. explicit launch inputs and selected target, with current values visible;
4. a clear run state and a result panel that distinguishes **authored scene**, **illustrative fixture**, and **Physics-backed result**;
5. baseline/variant comparison controls only for inputs the active model actually supports.

This scene can be composed before a solver adapter exists. In that phase, trajectory/impact content must be absent or visibly marked **ILLUSTRATIVE FIXTURE — NOT PHYSICS-BACKED**. Do not animate damage, debris, wall collapse, or a successful strike as though Physics computed it.

### 3. Proposed input/output contract (not frozen)

The WebPage adapter should be a thin boundary around a Physics-owned model/run contract. Proposed input fields:

- unique run ID and stable experiment/scene ID;
- model ID/version and schema version;
- SI units and an explicitly agreed coordinate-frame convention;
- launch state (position and supported velocity/angle inputs), gravity/model parameters, and target-wall bounds/identity;
- assumptions, supported parameter ranges, and requested output/sample interval.

Proposed result fields:

- the same run ID and model/schema identity;
- declared units, coordinate frame, assumptions, and capability flags;
- ordered time/tick and position samples when the model supplies a trajectory;
- target intersection/hit-or-miss and impact point/velocity only when actually calculated;
- momentum/energy diagnostics only when their definitions and inputs are supported by the active model;
- explicit unavailable/invalid/out-of-range status instead of fabricated values.

The Castle design note proposed a right-handed, z-up convention with azimuth in the x-y plane. I have **not** treated that as an established Physics contract: the Physics implementation owner must confirm or correct it against current code and tests before WebPage freezes any adapter or scene-coordinate schema. Likewise, a launch-speed proxy must not be described as a validated trebuchet mechanism.

### 4. Baseline and variant workflow

Keep the baseline immutable. A variant should have its own run ID and copied inputs, and should change one supported field at a time. Record the exact changed field, units, model/version, assumptions, and outputs for every run.

Compare only outputs returned by the active model: trajectory samples, flight time, target intersection, and supported impact diagnostics. If the model does not depend on mass for the current idealized trajectory, the UI must not imply that changing mass altered that trajectory. A changed gravity value is an alternate-model experiment, not a claim about the actual universe. This workflow belongs in the Laboratory's experiment model; WebPage owns controls and comparison presentation, not the calculations.

### 5. Replay and destruction boundary

The current Physics replay executor is a model-driven single-segment seam that relies on caller-provided checkpoint decoding, event application, and model advancement. It is not a Castle Siege world serializer, durable history store, or browser-ready world reconstruction adapter.

Before those pieces exist, store/compare input and result snapshots where supported, but do not call the comparison UI authoritative replay, scrubbing, branching, or shareable Moments. Impact diagnostics are not a masonry damage law. Add bounded wall damage only when Physics returns a tested material/wall response; add structural failure/debris only after the owning model exposes validated events for them.

### 6. Acceptance checks and next step

WebPage-side acceptance should include:

- stable catalog resolution for `city.castle-siege` / `castle-siege` and no duplicate interactable identities;
- route through the existing city/Explore experience, with no competing router;
- a useful commander view at desktop and narrow viewports;
- explicit input units, coordinate convention, model/version, and capability labels;
- baseline immutability and distinct variant/run identity;
- repeat-input determinism only to the degree Physics documents and tests it;
- hit/miss and impact readouts sourced from the Physics adapter, never recomputed in Razor;
- no damage, debris, or replay claim before the relevant contract exists.

Physics-side acceptance should include analytic/reference comparisons, input bounds and units, same-input repeatability, ordered samples, target hit/miss edge cases, model/version metadata, and tests for unsupported or invalid inputs. The mechanism model and wall-response law remain separate milestones.

**Next step:** WebPage can prepare the minimal authored city scene and stable destination scaffold in its own `development` branch. Physics should confirm the coordinate frame and expose the narrowest tested deterministic projectile/run-record contract; then WebPage can connect the adapter to actual observations. Please reply here with the authoritative type names, units/frame, and tests once confirmed.

**Repository boundary:** this response was appended only to `PhysicsCastlesLLM.md` in the Physics repository, as required by this handoff. No other Physics file was changed. No merge, publication, release, or deployment occurred.
