# Physics Pages Experience

**Status:** static client, projectile evidence inspector, terrain height-field experience, acoustic evidence card, and navigation FSM inspector implemented on `development`; automated checks are being expanded, while browser validation and publication remain outstanding. Work is tracked by [issue #53](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/53).

## The visitor experience

The Physics Pages site should feel like entering a small, living physics website—not opening a repository manual. A visitor arrives at a home/atlas, chooses a physical domain, enters an experiment, changes permitted inputs, advances or runs the model, and inspects the resulting observations and evidence.

The site's own navigation is an FSM-driven experience. Pages, domains, experiments, panels, and modal/inspection states are not unrelated booleans scattered through UI code. They are explicit states and validated transitions with stable identities. The browser URL is a projection of navigable state; browser back/forward requests transitions through the same validation path.

A useful first journey:

1. **Enter Physics** — see the domain atlas and a clear distinction between implemented, experimental, proposed, and future capabilities.
2. **Choose an experience** — for example constant acceleration, projectile motion, or the first-order room-reflection model.
3. **Set up the experiment** — inspect the declared inputs, units, coordinate convention, timestep, and bounds.
4. **Run or replay** — execute a supported deterministic model in the browser where practical, or load a run record generated from the actual Physics source.
5. **Observe** — inspect state samples, measurements, charts/visuals, and the exact logical step/time.
6. **Explain** — connect the observed result to the governing model, assumptions, equations, and source tests.
7. **Navigate elsewhere** — retain a meaningful browser history and return to the atlas without losing the distinction between navigation state and experiment state.

## Architecture decision: Pages first, WebApp-style client, ServerApp later

### GitHub Pages

Pages is the first host and public entry point. It can serve the application shell, static assets, versioned run records, generated diagrams, and learning content. It is sufficient for navigation and local deterministic experiments; a visitor should not need an account or server just to explore Physics.

### WebApp

The browser experience should be composed as a WebApp-style client. Its finite navigation domain and experience lifecycle are driven by FSMs. UI components render the current state and submit semantic actions; they do not own hidden parallel state machines. The browser history adapter translates validated navigation transitions to/from URLs and must handle malformed, stale, or unsupported deep links safely.

The initial client can be hosted as static files on Pages. "WebApp" describes the browser application boundary, not a requirement to provision a server.

### ServerApp

Do not couple the first slice to ServerApp. Add ServerApp only when a demonstrated use case requires authoritative shared state or server-side capabilities, such as persistent learner identity, shared/collaborative experiments, saved work across devices, access control, or coordinated computation. When introduced, it should own those server responsibilities; it should not replace the client's FSM navigation model.

## State model

The initial navigation state should be small and explicit. The implemented client currently includes `HOME`, `PROJECTILE`, `INSPECTOR`, `TERRAIN`, and `FSM`; the broader experience lifecycle below remains a candidate model. Candidate conceptual states:

- PhysicsHome
- DomainOverview(domainId)
- ExperienceOverview(experienceId)
- ExperimentSetup(experimentId)
- ExperimentRunning(runId)
- ExperimentResult(runId)
- EvidenceInspector(runId, evidenceId)

Candidate semantic actions include EnterDomain, OpenExperience, ConfigureExperiment, RunExperiment, ReplayRun, InspectEvidence, ReturnToAtlas, and NavigateBack. This list is a design vocabulary, not a claim that these FSM contracts already exist.

Keep separate state domains for:
- **Navigation:** where the visitor is in the site.
- **Experiment lifecycle:** selected definition, run identity, status, and current logical step.
- **Presentation:** selected panel, visualization mode, and inspector visibility.

They may coordinate through explicit events/contracts, but one domain must not silently mutate another.

## First experiences

Prefer a small number of real, verifiable experiences over a broad catalog of decorative simulations:

1. **Constant acceleration** — a simple one-dimensional model with explicit initial state, acceleration, timestep, step count, and recorded samples.
2. **3D projectile** — declared launch parameters, z-up coordinate convention, gravity vector, fixed logical timestep, and trajectory samples.
3. **First-order room reflection** — a bounded analytical acoustic path model with source/listener/room assumptions and source-generated run record.

The Pages UI must not fabricate physical outcomes. A model runs from the actual Physics implementation, or the site loads a checked-in/generated run record whose producing source commit is recorded. If the experience is a visualization of a record rather than a live solver, say so.

## Living evidence

Every experiment should expose, as applicable:

- Model and schema identity/version.
- Run ID and Physics source commit.
- Inputs, parameter bounds, units, and coordinate/frame conventions.
- Logical timestep, step index, and elapsed model time.
- Actual recorded observations and derived quantities.
- Validation/test references and known limitations.
- A clear capability status: implemented, bounded/experimental, proposed, or not implemented.

The state inspector should show the current navigation/experience state, available transitions, and the reason an action is unavailable. It should make the website itself inspectable as an FSM-driven system.

## Build and deployment

Use the Learning Center Pages workflow as a reference: check out exact source repositories, generate or validate artifacts from real source, record the source SHAs, build/test the browser app, assemble a static Pages artifact, and upload it.

- Build and test on pushes and pull requests to development.
- Deploy only through an explicit workflow_dispatch from development.
- Do not automatically deploy master.
- Preserve source provenance for every generated run record or catalog.
- Test browser-history/deep-link reconciliation and invalid URL handling.
- Browser/device rendering claims require actual browser/device validation; a successful CI build is not proof of runtime support.

## Dependency boundaries

- Physics core remains independent of UI, browser, WebApp, ServerApp, Renderer, and Visualization.
- The Pages application consumes Physics APIs, source-backed catalogs, and validated run records.
- Renderer and Visualization remain MicroBundle-consumed capabilities, not NuGet consumption targets.
- ServerApp is optional infrastructure, not the owner of the Physics model or the visitor's navigation state.

## Non-goals for the first slice

- User accounts, cloud saves, multiplayer, shared sessions, or server orchestration.
- A general-purpose website builder or CMS.
- A claim that every domain in the Physics atlas is implemented.
- A general rigid-body/articulated mechanics engine before its bounded model and validation gates are met.
- A visual-only simulation that invents results independently of Physics.

## First implementation slice

The initial static client now lives in `site/`. Its landing page follows the evidence-led showcase pattern established in GUI: an explorable domain atlas, source-backed artifact previews, explicit proof/limitation statements, and separate visitor paths for exploration, study, and contribution.

- `site/index.html` and `site/styles.css` provide the Pages-hosted shell and responsive presentation.
- `site/app.js` consumes the pure navigation machine in `site/navigation-fsm.js`; implemented states are `HOME`, `PROJECTILE`, `INSPECTOR`, `TERRAIN`, and `FSM`. Browser hash/history navigation uses the same route parser, and `site/tests/navigation-fsm.test.mjs` checks deep links, route mappings, valid transitions, and rejected actions.
- `site/data/terrain-height-field.run.json` is generated from `TerrainHeightField2D`; the terrain experience compares the original bowl and a smooth radial brush edit, renders the recorded grid as an isometric SVG, and exposes per-cell values and a centre-row profile. The browser does not regenerate terrain.
- `site/data/projectile-trajectory.run.json` is the checked-in copy of the canonical projectile fixture. The browser reads and visualizes recorded samples; it does not recalculate the trajectory.
- `site/data/acoustic-observation.run.json` and `site/data/acoustic-observation.svg` expose the actual `FreeFieldAcoustics.Observe` output as a source-backed gallery card. The Pages workflow compares the site record with fresh C# generator output and both SVGs with a fresh render. This is a propagation-metadata observation, not generated audio.
- The sample selector changes the inspected record index, and the evidence inspector exposes position, velocity, units, model/schema identity, timestep, frame, and all recorded samples.
- `.github/workflows/physics-pages.yml` reproduces projectile, terrain, and acoustic evidence with their actual .NET generators, compares generated output with canonical/site fixtures, checks JavaScript syntax and navigation tests, and packages the static site. Deployment is restricted to an explicit `workflow_dispatch` on `development`.

**Current boundary:** this is a first working implementation slice, not yet a published Pages deployment or a complete Physics atlas. The checked-in projectile record does not currently contain a unique run ID or source revision, so the site deliberately does not invent either. Provenance enrichment belongs in a future versioned run-record contract. The terrain record identifies its Physics source, focused test, and generator path; neither record claims an unrecorded source commit. Browser/device validation and the actual Pages deployment remain outstanding.


## Planned experience: vertical cannon and synchronized graphs

Track this as [issue #54](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/54). This is a planned experience, not an implemented model yet.

A cannon fires a projectile vertically from a fixed launch point. In the first ideal model, gravity is constant and air resistance is absent. The visitor watches the projectile rise, momentarily reach zero vertical velocity at the apex, and return to its launch height. At that return, the speed magnitude matches the launch speed while the velocity direction is reversed.

The animation and every graph must be driven by the same selected logical time and source-generated run record:

- Height versus time: a symmetric parabola for the ideal constant-gravity model.
- Vertical velocity versus time: a straight line with constant negative slope, crossing zero at the apex.
- Vertical acceleration versus time: constant negative gravity.
- Optional energy graphs: kinetic, gravitational potential, and total mechanical energy, only after the quantities and conservation checks are implemented and tested.

The experience should mark launch, apex, and return events; support play/pause/step/replay/scrubbing; and expose the exact selected state in the evidence inspector. The ideal model must remain distinct from any later air-resistance model. A future drag comparison should explicitly expose its drag assumptions and parameters rather than quietly changing the ideal result.

**Scientific boundary:** this is an idealized vertical point-projectile experiment, not a model of cannon combustion, recoil, barrel mechanics, wind, aerodynamic stability, or real-world artillery. The return-to-launch-speed result follows from the ideal model's assumptions and must not be presented as an unconditional real-world prediction.

## Planned experience: train-mounted cannon, air resistance, and curved track

Track this as [issue #55](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/55). This is a proposed experiment, not an implemented simulation.

The straight-track experiment makes the idealization visible. A train travels at constant horizontal velocity and launches a cannonball vertically relative to itself. With air resistance excluded, the horizontal drag term is visibly crossed out/zeroed in the displayed equation. The ball retains the train's horizontal velocity while gravity governs its vertical motion, so it returns to the tube when the train remains on a straight path at constant velocity and the launch/landing geometry is compatible.

Air resistance is a user-controlled model term, not a hidden caveat. Enabling it restores an explicitly parameterized drag force based on the ball's velocity relative to ambient air. For a moving train in stationary air, the ball loses horizontal speed relative to the air and falls behind the continuing train. Disabling drag again restores the ideal result. The UI must regenerate/recompute a validated Physics run; it must never fake the miss by applying a presentation-only offset.

Bounded controls should include train speed, vertical launch speed, launch height, gravity, and the air-resistance toggle. Drag parameters should be exposed when relevant. Animation, world/train-relative positions and velocities, graphs, events, and evidence inspection all derive from one run and one selected logical time.

A curved-track mode then changes the lesson: the train is constrained to turn and therefore accelerates, while the airborne ball initially carries the train's instantaneous velocity and follows its declared free-flight model. The train can move away from the ball's inertial path even with drag disabled. Begin with a bounded planar curve; a globe-wrapping track requires an explicit geometry, reference frame, gravity field, and train constraint model before it can be described as physically validated.

**Scientific boundary:** the ideal straight-track alignment assumes constant train velocity, no horizontal forces on the ball, compatible launch/landing geometry, and a declared ambient-air state. Curved-track behavior must be calculated from specified train constraints and projectile dynamics, not inferred from visual curvature alone.
