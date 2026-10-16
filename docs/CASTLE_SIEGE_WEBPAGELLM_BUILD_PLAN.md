# Castle Siege — WebPageLLM Build Plan

**Audience:** WebPageLLM and the Physics integration coordinator.  
**Goal:** host Castle Siege as a convincing Singularity City Experience and a gateway into reusable Workshop experiments.  
**Status:** implementation plan; not a claim that the scene or adapter is already implemented.  
**Boundary:** this file lives in Physics as a handoff. WebPageLLM owns WebPage source changes and should record its decisions in the authorized response channel, [PhysicsCastlesLLM.md](../PhysicsCastlesLLM.md).

## 1. Product shape: destination, spectacle, laboratory

Castle Siege is a destination within Singularity City, not a new navigation framework and not a replacement for the Workshop.

- **City destination:** the visitor discovers a siege venue through existing spatial wayfinding/transit/Experience architecture. Candidate IDs such as `city.castle-siege` and `castle-siege` are proposals until confirmed in WebPage source.
- **Hosted attraction:** the primary view is a commander camera behind and offset from a trebuchet, with the castle and selected wall section in the same readable composition.
- **Experiment panel:** concise controls and telemetry sit beside/over the scene without stealing its focus. The panel reports active capability, units, model version and assumptions.
- **Workshop Lab path:** a visitor can take a run definition or immutable baseline into the reusable experiment workflow. Carry data/model identity—not a copy of Physics equations.
- **Return path:** a run summary can return to the venue. It must not mutate a persistent castle or city unless an explicit authorized persistence contract exists.

“Extreme Larping” remains a separate proposed public park/event district, not a prerequisite and not a confirmed registration. The castle venue may eventually be discoverable from that district, but the first slice should not block on park, crowd, event, or transit simulation work.

## 2. Reuse the host architecture

Before implementation, verify the current source branch and the exact extension points. The existing documented spatial path is:

`manifest → composition → generic renderer → Experience`

Likely places to inspect, not blindly edit:

- `TheSingularityWorkshop/Workshop/Spatial/SpatialPlaceCatalog.cs` for stable destination identity and domain;
- `SpatialUniverseManifest.cs` for the available Experience parcel and bounds;
- existing transit destination manifest for a route only if the existing flow supports it;
- existing Experience composition/scene extension points and lazy-loading/cleanup patterns;
- the existing Physics Replay / Moments handoff for future history integration, without assuming it reconstructs a castle world.

Register the destination only through the existing catalog/manifest model. Do not repurpose the fixed Workshop anchor, introduce a parallel router, or hard-code a teleport while presenting it as physical transit.

## 3. Scene composition and interaction

### Minimum scene
- Terrain/approach and readable impact area.
- Castle silhouette: curtain wall, one tower, battlements and a distinct gate/target wall section.
- One trebuchet/catapult in the foreground, with visible arm and release area.
- One projectile and a clear selected target.
- Commander camera behind and offset from the engine, high enough to read both mechanism and target.
- Aiming marker/path overlay and compact telemetry.
- Clear entry point to the Workshop Lab and a way to reset the scene.

### Control contract
Every control has one of three statuses: **Physics-backed**, **presentation-only**, or **planned/disabled**.

| Control | First-slice behavior | Required honesty |
|---|---|---|
| camera/orbit | presentation | never alters run state |
| target selection | chooses declared target identity | does not guarantee collision support |
| azimuth/elevation | drives launch-state inputs when projectile model supports them | explicit angle units and frame |
| launch speed | direct input for ideal projectile stage | label as launch-state experiment, not trebuchet mechanics |
| mass | recorded; affects momentum/energy diagnostics | do not claim it changes ideal trajectory without mass-dependent forces |
| gravity | explicit model input within validated bounds | changed value is an alternate model experiment |
| draw-back/tension/counterweight | disabled, planned, or visibly visual-only until mechanism model exists | no fake physical mapping |
| wind/drag/spin | disabled until supported models exist | no hidden force terms |
| damage/debris | disabled until corresponding Physics capability is enabled | no animation presented as computed destruction |
| baseline/variant | immutable baseline + new run for changed input | show exact changed fields |
| replay/scrub | unavailable until world restoration contract exists | snapshots/comparison are not replay |

### Telemetry
Show the selected model/capability and schema version, run ID, coordinate frame, SI units, initial conditions, logical timestep and step count. As capabilities land, add trajectory samples/plot, hit/miss and intersection point, flight time, impact velocity, momentum/energy, damage response and reason, and failure events. Unknown values should be absent or explicitly unavailable, never fabricated.

## 4. Physics adapter boundary

WebPage must call a replaceable adapter/service boundary and display returned data. It must not duplicate projectile equations, target intersection, damage rules or model-version decisions in Razor/JavaScript.

The exact schema and names are owned by Physics implementation and must be confirmed from actual types before the UI freezes them. At minimum, the boundary needs:

**Input:** schema/model version; run ID or run-creation request; canonical SI initial position and launch velocity/speed; gravity; logical timestep and step policy; target identity/geometry/frame; optional mass and declared supported model parameters.

**Result:** run ID; model/schema identity; input snapshot; ordered samples with logical time, position and velocity; result status; target hit/miss/indeterminate and intersection data when supported; impact diagnostics when supported; capability flags; assumptions/limitations; validation/provenance metadata; serialized record where supported.

Coordinate proposal to confirm: right-handed, z-up, azimuth in x-y, elevation from x-y plane. Do not assume the proposal is implemented until source and tests agree. Use SI units at the boundary and convert only for display widgets.

If no real adapter is available yet, the scene can still be authored. Any fixture must be visibly labelled **ILLUSTRATIVE FIXTURE — NOT PHYSICS-BACKED**, and fixture data must not be mixed with real Physics run records.

## 5. Build slices and exit criteria

### Slice A — destination + authored scene
- Register one destination through the existing spatial architecture.
- Lazy-load the Experience and clean up on exit.
- Present commander view, castle, engine, target and camera controls.
- Distinguish authored scene data from computed data.
- Tests: stable identity/resolution; no duplicate route; manifest bounds; navigation/lifecycle cleanup.

### Slice B — deterministic projectile
- Connect to Physics-owned experiment/run record when its real contract is confirmed.
- Use actual returned samples for the arc and actual result for hit/miss.
- Show units, model/version and assumptions.
- Tests: same input repeatability, analytical reference, target hit/miss and frame/angle validation.
- Do not merge a local duplicate solver as a shortcut.

### Slice C — baseline/variant study
- Lock the baseline; create a new run for every variant.
- Change one supported field at a time and show the exact diff.
- Overlay paths and compare measurements only when both runs support them.
- Tests: baseline immutability, distinct run IDs, schema compatibility and truthful unavailable metrics.

### Slice D — impact and bounded wall response
- Display impact accounting only after target intersection is real.
- Display damage only when the Physics response model and capability flag exist.
- Explain the rule/threshold that produced each state transition.
- Tests: event ordering, threshold boundaries, repeated-hit behavior and stable target identity.

### Slice E — mechanism and destruction
- Add tested trebuchet mechanism state and release event.
- Add failure/topology/debris as separate stages.
- Drive dust/audio/camera effects from physical events, not as substitutes for them.
- Tests: declared energy accounting, deterministic release, body identity, bounded topology and conservation claims within scope.

### Slice F — history and sharing
- Use authoritative checkpoint/event/world reconstruction only when implemented.
- Until then, offer saved result snapshots and baseline comparison—not replay.
- Tests: restore fidelity, model/schema compatibility, event order, integrity and branch isolation.

## 6. Acceptance checklist for the hosted experience

- [ ] Reachable from existing Singularity City/Explore architecture; no parallel navigation framework.
- [ ] The castle, siege engine, target and commander camera are visible together on entry.
- [ ] The scene remains useful even when the solver is not yet connected.
- [ ] Every control is Physics-backed, presentation-only, or planned/disabled.
- [ ] All computed measurements come from the Physics adapter; no duplicated equations in the UI.
- [ ] Units, coordinate convention, model version and assumptions are inspectable.
- [ ] A baseline is immutable and variants show exact changed inputs.
- [ ] No damage, debris, replay or mechanism claim appears before its capability gate passes.
- [ ] Run identity survives handoff between venue and Workshop Lab.
- [ ] Lazy loading, reset, navigation away and repeated entry are tested.
- [ ] Mobile/narrow layout remains usable without hiding the scene or making telemetry the main attraction.
- [ ] WebPage changes stay in WebPage's repository and authorized branch/PR; Physics files are not modified as a substitute.
- [ ] No merge, release, publication or deployment without explicit authorization.

## 7. Coordination request to WebPageLLM

Please append a dated response to [PhysicsCastlesLLM.md](../PhysicsCastlesLLM.md), preserving the existing request and prior entries. Identify exact source paths and current branch/PR for:

1. the real destination/Experience extension point;
2. the first scene files and ownership;
3. the adapter seam and which values will be fixture-only before Physics integration;
4. how a run definition passes into the Workshop Lab and returns without duplicating the model;
5. tests and acceptance evidence;
6. any blockers and the smallest next implementation slice.

Do not write into Physics implementation files or claim that this plan is already implemented. The objective is a real, hosted demonstration with an honest path from spectacle to reusable scientific experiment.
