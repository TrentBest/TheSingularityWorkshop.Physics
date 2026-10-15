# Castle Siege: A Destructive-Physics Showcase

**Status:** design and integration brief, not evidence that the described scene or solver exists.  
**Experience owner:** WebPageLLM (presentation and composition). **Physics contract owner:** Physics implementation LLM. **Design/coordination owner:** this Physics-side documentation track.

## 1. The experience

The visitor stands at the commander's position just behind and slightly to one side of a trebuchet, looking toward a stone castle. The castle is the target and the physics laboratory. The visitor can inspect and change the siege setup, aim, choose supported launch/mechanism parameters, fire, and observe a calculated trajectory strike a specific wall section. As the model matures, the impact should produce explainable material damage and structural failure.

This must not be a pre-rendered destruction animation presented as simulation. Every stage distinguishes authored visuals from computed outcomes. Start with one trebuchet, one projectile, one castle, one target wall section, and one camera.

The showcase should demonstrate deterministic repeatable experiments; inspectable inputs, units, assumptions and model versions; baseline-versus-variant comparison; materials and bounded damage before broad fracture claims; multirate scheduling separate from render cadence; and history/replay only when authoritative reconstruction is implemented.

## 2. Scene composition and camera

- **Foreground:** one readable trebuchet/catapult, arm and release area, projectile, compact controls.
- **Camera:** commander viewpoint behind and offset to one side of the engine; high enough to read the mechanism and flight path while keeping the target wall visible.
- **Midground:** terrain, approach/impact area and a marked target zone.
- **Background:** recognizable stone castle silhouette with curtain wall, at least one tower, battlements, and a distinct gate or wall section.
- **Initial physical target:** one wall section, not every stone and every castle component.
- Keep camera, lighting, dust, decals and other presentation-only state out of authoritative physics state.

Controls should expose azimuth/yaw, elevation, projectile mass and supported geometry preset, launch state, gravity, and baseline/variant comparison. A draw-back/tension/counterweight control is only physically meaningful once a tested mechanism model maps it to launch state. Before that, it must be disabled/planned or explicitly visual-only; never imply a slider is a validated trebuchet solver.

Show units, legal ranges, selected model/version, assumptions, target intersection, flight time, impact velocity, kinetic energy, momentum and damage-model status. Keep the scene primary and measurements compact.

## 3. Physics ladder

### A — Scene-only composition

Castle, siege engine, camera and aiming affordances. Without a real Physics result, any trajectory or impact is an **illustrative fixture — not Physics-backed**. No simulated-damage claim.

### B — Deterministic ideal projectile

Input record: initial position (m), launch speed (m/s), elevation and azimuth (canonical stored angle units), projectile mass (kg), gravity (m/s²), logical timestep (s), target geometry/coordinate frame, model/schema version and run identity.

For a declared z-up world frame and azimuth measured in the x-y plane:
- (v_x=v\cos(\theta)\cos(\phi))
- (v_y=v\cos(\theta)\sin(\phi))
- (v_z=v\sin(\theta))

With constant gravity vector, integrate (v(t)=v_0+gt) and (x(t)=x_0+v_0t+\frac12gt^2). Compare stepping against the analytical solution with a declared tolerance. Compute target intersection from the path and target geometry, not a scripted hit. (R=v^2\sin(2\theta)/g) is only a cross-check for ideal motion with equal launch and landing heights, not a universal answer for an elevated or uneven target.

Explicitly exclude drag, wind, spin, sling release geometry, arm inertia, rope elasticity, counterweight motion and contact unless implemented and tested.

### C — Impact accounting

Report computed impact velocity and diagnostics (p=mv), (E=\frac12m|v|^2), impact point, target normal and any supported impulse estimate. **Energy and momentum are not by themselves a masonry damage law.** Do not infer crater size or collapse from them alone.

### D — Bounded material/wall response

Choose one small wall section or small set of structural blocks with declared material parameters. Define what the model means by material identity, mass/density, strength or failure threshold, impact transfer, damage accumulation and state transitions. Include provenance, units, valid ranges, assumptions and limitations. Keep impact observation separate from the damage decision. If no supported response exists, show impact diagnostics without pretending the wall failed.

### E — Mechanism and structural failure

Replace initial-speed proxy with a tested trebuchet mechanism: counterweight/tension energy, arm geometry/inertia, sling/release condition and losses within declared scope. Then add failure detection, topology changes and debris as separate contracts. Dust, sound and fragments consume physical events rather than standing in for them.

### F — History and replay

Capture exact initial conditions, parameter changes, ordered events, model versions and checkpoints. Keep reality, timeline and checkpoint identities distinct. Existing replay seams are model-driven contracts, not a complete world serializer or restoration engine. Do not advertise authoritative replay/scrubbing until a concrete solver/world reconstruction adapter is implemented and tested.

## 4. Materials are part of the lesson

The castle should eventually demonstrate why materials differ, not merely show stone-colored meshes. Begin with a small supported material set and state which properties the wall response actually consumes. Potential future comparisons include stone/mortar, timber, iron and earth, but do not expose properties or damage behavior the model does not use.

Each outcome should answer: which material/model; which parameters and units; what event occurred; which rule caused the damage transition; what was measured versus approximated; and whether the run is repeatable. Material property data needs source/version/provenance and valid ranges before being presented as authoritative. A visual preset without a physics contract is cosmetic, not material simulation.

## 5. Constant exploration

Preserve a baseline and change one input or supported constant at a time. Compare arc, target intersection, flight time, impact speed, energy/momentum diagnostics and supported damage state. Record exact parameters and model version. Changing gravity or another constant creates an alternate model configuration; it does not claim the alternate value describes our actual universe. Label results as computed, analytically checked, illustrative, experimental or planned. Avoid false precision.

## 6. Ownership and boundaries

- **Physics repository:** physical models, SI-unit contracts, solver state, target intersection, material/impact models, validation, repeatability tests and authoritative run records. No WebPage/Blazor/Unity/renderer types.
- **WebPage:** scene composition, camera and inputs, explanations, overlays, plots and lazy loading. Use the existing spatial composition/Experience architecture; do not create parallel scene/navigation architecture or embed a second physics engine in Razor.
- **Visualize/Renderer:** eventual consumers, not dependencies of Physics.
- **Gestures:** optional crew choreography consuming declared observations/events; Physics must not depend on Gestures.
- **FSM_API/FSM_COS:** process scheduling/composition at proper boundaries. Physics logical time is distinct from browser frame timing.
- **Communication:** WebPageLLM may update only root `PhysicsCastlesLLM.md` in this repository. Physics implementation LLM should use root `PhysicsLLM.md` and/or implementation issue/PR. This design and coordination log remain owned by the documentation coordinator.

## 7. Exact ask for WebPageLLM

Read this design, the WebPage spatial README, and the existing Physics Replay/Moments handoff before proposing changes.

1. Identify the best existing WebPage spatial composition/Experience extension point, citing exact source paths and types.
2. Propose the smallest castle/trebuchet/commander-camera composition and exact files to touch. Keep it lazy-loaded and consistent with current navigation/experience architecture.
3. Separate authored visual composition from inputs/outputs requiring a Physics adapter.
4. Propose the WebPage-facing adapter contract: SI-unit inputs; coordinate frame; run/model identity; sampled trajectory; target intersection; impact diagnostics; model version; capability and limitation flags.
5. If a visual prototype precedes Physics integration, label it **illustrative fixture — not Physics-backed**. Do not imply real aiming, launch, impact, damage or replay calculations when they are not connected.
6. Propose minimal baseline-versus-variant UI and explain how it fits existing replay/moments boundaries without claiming authoritative replay prematurely.
7. List exact files/types, risks, acceptance tests, dependencies and the smallest next step.
8. Write findings/questions/proposal to root `PhysicsCastlesLLM.md` only, as a dated append-only log. Do not edit this Physics repository, the WebPage coordination handoff, or any WebPage files. Do not publish/deploy/release.

## 8. Acceptance gates

- [ ] Scene uses existing WebPage composition architecture and has the intended camera.
- [ ] Any fixture is clearly labelled illustrative until connected to Physics.
- [ ] Ideal projectile has explicit coordinate conventions, SI units, deterministic stepping and analytical comparison.
- [ ] Target intersection is computed from trajectory and target geometry.
- [ ] Impact diagnostics are separate from the damage law.
- [ ] Material/wall response has explicit assumptions, thresholds and repeatability tests.
- [ ] Mechanism controls do not claim validated physics before the trebuchet model exists.
- [ ] Baseline/variant records exact inputs and model versions.
- [ ] Replay labels match actual persistence/reconstruction capability.
- [ ] No release/deployment without explicit authorization.

## Related documents

- [Physics roadmap](ROADMAP.md)
- [Physics coordination log](COORDINATION.md)
- [Timeline and multiverse design](TIMELINES_AND_MULTIVERSE.md)
- [Articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md)
- [WebPage spatial architecture](https://github.com/TrentBest/WebPage/blob/master/TheSingularityWorkshop/Workshop/Spatial/README.md)
- [WebPage Physics Replay handoff](https://github.com/TrentBest/WebPage/blob/master/docs/integrations/PHYSICS_REPLAY_WEBPAGELLM_HANDOFF.md)
- [Projectile experiment issue #23](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/23)
