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
