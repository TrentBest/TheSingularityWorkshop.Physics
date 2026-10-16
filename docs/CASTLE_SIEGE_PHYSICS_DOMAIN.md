# Castle Siege Physics Domain

**Purpose:** define Castle Siege as a reusable, layered physics test domain—not a one-off projectile animation.  
**Status:** design contract and test plan. The stages below are not claims that every model exists in code.  
**Owners:** Physics owns models, units, state transitions, records, and validation; WebPageLLM owns the hosted Experience; this document coordinates their boundary.

Related: [Castle Siege showcase](CASTLES_AND_CATAPULTS.md) · [Experiment catalog](CASTLE_SIEGE_EXPERIMENT_CATALOG.md) · [WebPage build plan](CASTLE_SIEGE_WEBPAGELLM_BUILD_PLAN.md) · [Physics roadmap](ROADMAP.md) · [Physics classes](PHYSICS_CLASSES.md).

## 1. Why this is the right proving ground

A castle and siege engine make cause and effect visible. A visitor changes a control, the system records that input, a model predicts motion, a projectile reaches or misses a target, the target responds according to a declared material/structural rule, and the result can be measured and compared. Every step can be independently tested.

The same scene can eventually exercise a broad cross-section of Physics:

- kinematics and gravity;
- rigid-body motion, constraints, impulses and contact;
- mechanisms, stored energy and release conditions;
- collision detection and target intersection;
- material identity, stress/strain approximations and bounded damage;
- structural failure and topology changes;
- fracture/deconstruction and debris;
- heat, sound, dust, air displacement and energy accounting;
- terrain, weather and optional fluid/soil interactions;
- deterministic scheduling, coarse/fine simulation, history, replay and branch comparison;
- presentation events consumed by WebPage, Visualize/Renderer and Gestures without making any of those systems the authority for physical outcomes.

This is not a demand to implement all domains at once. It is a coherent test ladder in which each capability has a useful visible outcome and an honest accuracy boundary.

## 2. Domain objects and identities

Use stable identities and explicit model boundaries. Names below describe concepts; they are not declarations that these exact C# types already exist.

| Concept | Responsibility | Required identity/data |
|---|---|---|
| Experiment definition | Immutable description of one proposed run | schema version, experiment ID, model ID/version, coordinate frame, SI inputs, timestep/policy |
| Run | One execution of one definition | unique run ID, start conditions, result status, sample count, deterministic settings |
| Siege engine | Mechanical system that eventually generates launch state | engine configuration, geometry/version, mass/inertia parameters when supported |
| Projectile | Moving body | stable body ID, mass, geometry, initial pose/velocity |
| Target | Selected wall/gate/tower section | target ID, geometry, coordinate frame, material/response model |
| Observation | A measured result from a model step | tick/time, source model, units, value, validity |
| Physical event | A state-changing event emitted by a model | event ID/order, logical tick, cause, affected identities, model version |
| Variant | A new experiment derived from a baseline | parent run/definition, exact changed fields, new run ID |
| Checkpoint/history segment | Future restoration and replay data | schema/model compatibility, state digest, ordered inputs/events |
| Presentation cue | Optional visual/audio response to a supported event | source event ID, presentation-only flag, no authority over solver state |

Never use a camera, mesh, animation, sound, or particle system as authoritative physical state. Rendering can interpolate or simplify; it cannot quietly decide whether a hit occurred or a wall failed.

## 3. Domain boundary and causal pipeline

```text
Visitor controls
    ↓ validated experiment definition
Run identity + model/version + coordinate frame + SI units
    ↓
Siege mechanism (future; or explicit launch-state input in first slice)
    ↓ launch position / velocity
Projectile integration ───────→ trajectory observations
    ↓ conservative candidate query
Target intersection / contact confirmation (separate stages)
    ↓
Impact observation: location, normal, velocity, momentum, energy
    ↓ material/structural response (only when implemented)
Damage state / failure event / topology change
    ↓
Presentation cues: dust, sound, debris, camera emphasis
    ↓
Measurements + explanation + baseline/variant comparison + exportable run record
```

Every arrow is a contract boundary. If a downstream capability is absent, the upstream result remains useful and must say where the chain stops. A broadphase candidate is not a confirmed hit. A confirmed impact is not a damage law. A damage threshold is not general masonry fracture. A visual debris effect is not proof that a structural solver produced debris bodies.

## 4. Coordinate, units and time contract

The proposed adapter convention is right-handed, z-up, with x-y as the horizontal plane. Azimuth is measured in the x-y plane; elevation is measured upward from that plane. Freeze this only after Physics implementation and tests confirm it against actual kernel conventions.

Use SI at the model boundary:
- position: metres (m);
- time and logical timestep: seconds (s);
- velocity: metres per second (m/s);
- acceleration/gravity: metres per second squared (m/s²);
- mass: kilograms (kg);
- force: newtons (N);
- impulse and momentum: kg·m/s (equivalent to N·s for impulse);
- energy/work: joules (J);
- angular quantities: explicitly named radians or degrees, never implicit;
- material quantities: named SI units and documented model assumptions.

UI-friendly units may be converted at the edge. Persist canonical values and units. Browser render frames are not physics time; camera movement must not alter a run. Logical tick, solver timestep, model cadence and render frame should remain separately identifiable.

## 5. Fidelity ladder: useful at every stage

### S0 — Authored venue

A recognizable castle, siege engine, commander camera, target selection and readable controls. This is a presentation deliverable only. If an arc or impact is hand-authored, label it illustrative.

### S1 — Ideal projectile

The user supplies or selects a launch state; the current constant-acceleration kernel advances it. Record initial state, gravity, timestep, samples and model identity. Validate against an analytical reference. No drag, wind, spin, arm, sling, counterweight or collision response is implied.

### S2 — Target intersection

Define target geometry and its coordinate frame. Compute intersection from the actual sampled/continuous path under a documented rule. Return hit, miss or indeterminate—not a guessed hit from visual overlap. Test grazing, edge, behind-origin and time-window cases.

### S3 — Impact accounting

For a confirmed impact, report impact location, normal if available, pre-impact velocity, momentum and kinetic energy. State whether the model computes an impulse or merely reports incoming diagnostics. Energy and momentum alone do not determine stone damage.

### S4 — Bounded wall response

Choose one small wall section with an explicitly declared response rule, parameter source, units, valid range and threshold semantics. Return a transparent response such as no visible state change, bounded damage increment, or threshold-crossing event. This is a teaching model, not a universal masonry constitutive law. Label it accordingly.

### S5 — Structural failure and debris bodies

Separate (1) failure detection, (2) topology mutation, (3) creation/initialization of debris bodies, and (4) visual effects. Define conservation/accounting and deterministic event ordering before increasing body count. Test repeat impacts, adjacent sections and unsupported cases.

### S6 — Trebuchet mechanism

Replace a direct launch-state input with a mechanism model: arm geometry/inertia, counterweight or tension, potential energy, pivot constraints, release condition, losses, and launch-state derivation. The exact mechanism and level of fidelity must be declared. A draw-back slider is not physically meaningful until its mapping is implemented and tested.

### S7 — Coupled phenomena

Only add secondary effects when supported by state/event contracts:
- dust and displaced air from impact/debris motion;
- sound as an estimated consequence of a supported event, with its approximation labeled;
- heat only when an energy transfer and material state model exist;
- weather/wind/drag only when explicit force models are enabled;
- soil/terrain response only when terrain mechanics exists;
- crew gestures as consumers of mechanism state and events, not as solver authority.

### S8 — Multirate world and history

Schedule independent domains at defensible rates; preserve elapsed logical time and pending events for distant/unobserved objects. Compare cost and error against a finer-step reference. Authoritative replay waits for compatible checkpoints, ordered input/event history and a concrete world/solver reconstruction adapter.

## 6. Capability declaration

Each run should declare a capability profile. The names below are conceptual and may be mapped to the final schema.

| Capability | False until evidence exists | Evidence required to enable |
|---|---|---|
| deterministic projectile integration | no trajectory claims from fixtures | analytical/reference and repeatability tests |
| target intersection | no hit/miss claims from mesh overlap | geometry-boundary and path-intersection tests |
| impact diagnostics | no impact metrics without a confirmed target event | units, frame and numerical tests |
| wall response | no damage claim from energy alone | declared response model and threshold tests |
| structural failure | no collapse claim from decals or animation | failure criterion, state transition and tests |
| debris as physical bodies | no physical debris claim from particles | body creation, state initialization and accounting tests |
| mechanism-derived launch | no validated trebuchet claim from a proxy slider | mechanism model, assumptions and calibration/reference tests |
| authoritative replay | no timeline scrubber called replay | checkpoint restoration, ordered event application and state-fidelity tests |
| cross-rate handoff | no claim that low-rate and high-rate states agree | convergence/error and conservation tests |

The UI must consume these flags from the run/adapter contract. It must not infer that a feature is implemented because a control is visible.

## 7. Validation principles

1. **Repeatability:** identical model version, canonical inputs, timestep policy and supported runtime contract produce identical declared outputs.
2. **Reference agreement:** compare the numerical path against the analytical constant-acceleration solution at matching logical times within an explicit tolerance.
3. **Boundary testing:** zero speed, near-vertical launch, invalid timestep, invalid mass, target misses, edge/grazing intersection, and target behind the launch direction.
4. **Dimensional integrity:** reject invalid/non-finite values; never mix degrees/radians or display units with stored units silently.
5. **Causal integrity:** target damage cannot precede a confirmed impact; presentation events refer to a supported physical event; unsupported capabilities stay disabled.
6. **Variant integrity:** baseline remains immutable; each variant records exact changed fields and has a distinct run identity.
7. **Ordering:** event order is explicit when multiple impacts or state transitions occur on the same logical tick.
8. **Conservation scope:** where a model claims momentum/energy conservation, state the system boundary, transfers, numerical losses and tolerance. Do not claim conservation for a model that does not track the necessary quantities.
9. **Honest degradation:** unsupported cases return a declared status or limitation, not invented precision.
10. **Evidence-linked explanation:** each result is connected to the model, assumptions, units, test/reference, and provenance of material parameters where applicable.

## 8. How this domain tests the wider Physics architecture

- **FSM_API:** lifecycle/state transitions for experiment, projectile, target response and failure; it does not replace numerical integration.
- **FSM_COS / process groups:** composition and cadence ownership when the adapter exists; no dependency is assumed complete today.
- **Deterministic time ledger:** distinguishes requested time from committed logical ticks; it does not itself schedule solvers.
- **Broadphase/narrowphase:** candidate discovery is followed by confirmation; a broadphase overlap cannot be treated as collision truth.
- **Materials/thermodynamics:** material identity and energy-transfer contracts are prerequisites for credible heating or damage.
- **Universe/observer resolution:** a distant siege may use lower-cost state evolution but must reconcile correctly when observed again.
- **Timelines:** immutable baseline and variants are useful now; authoritative restoration and shareable Moments are later gates.
- **Gestures:** crew choreography can respond to loading, release, impact and failure events, without Physics depending on Gestures.
- **Visualize/Renderer:** downstream consumers of physical observations and events; rendering detail can vary without changing physical truth.
- **WebPage:** owns the hosted venue, camera, controls, measurements and explanation; never duplicates solver equations in presentation code.

## 9. Explicit non-goals for the first public slice

Do not block the first compelling experience on a universal rigid-body engine, realistic full-castle fracture, a full trebuchet, a city-wide crowd, weather, multiplayer, time travel, or a complete digital twin. Also do not fake any of them. Deliver the authored scene and first deterministic experiment, then make each next physical claim earn its way into the capability profile through implementation and tests.

## 10. Completion checklist

- [ ] Actual kernel/run-record extension points identified; no duplicate solver introduced without rationale.
- [ ] Coordinate convention confirmed against code and tests.
- [ ] Projectile experiment and serialized run record tested.
- [ ] Target intersection is computed and boundary-tested.
- [ ] Capability profile is sourced from real model support.
- [ ] WebPage can show model identity, units, assumptions and limitation status.
- [ ] Baseline/variant comparison preserves immutable inputs and exact diffs.
- [ ] Wall response is a separate, testable model with explicit material assumptions.
- [ ] Destruction stages separate damage, failure, topology and presentation.
- [ ] Readme and experiment catalog link to this domain document.
- [ ] No release, merge or deployment without explicit authorization.
