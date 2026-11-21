# Physics Pages Experience

**Status:** product and architecture direction; implementation tracked by [issue #53](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/53).

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

The initial navigation state should be small and explicit. Candidate conceptual states:

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

The initial static client now lives in `site/`:

- `site/index.html` and `site/styles.css` provide the Pages-hosted shell and responsive presentation.
- `site/app.js` owns an explicit navigation state machine (`HOME → PROJECTILE → INSPECTOR`) and semantic actions. Browser hash/history navigation is reconciled through the same route parser.
- `site/data/projectile-trajectory.run.json` is the checked-in copy of the canonical projectile fixture. The browser reads and visualizes recorded samples; it does not recalculate the trajectory.
- The sample selector changes the inspected record index, and the evidence inspector exposes position, velocity, units, model/schema identity, timestep, frame, and all recorded samples.
- `.github/workflows/physics-pages.yml` reproduces the run with the actual .NET generator, compares the generated output with both canonical and site fixtures, checks JavaScript syntax, and packages the static site. Deployment is restricted to an explicit `workflow_dispatch` on `development`.

**Current boundary:** this is a first working implementation slice, not yet a published Pages deployment or a complete Physics atlas. The checked-in projectile record does not currently contain a unique run ID or source revision, so the site deliberately does not invent either. Provenance enrichment belongs in a future versioned run-record contract. Browser/device validation and the actual Pages deployment remain outstanding.
