# Castle Siege: Payloads, Deconstruction, and Competitive Siege

**Purpose:** expand Castle Siege from a single projectile demonstration into a reusable physics sandbox and a foundation for an eventual turn-based/versus siege game.

**Status:** domain and game design proposal. This document does not claim payload fracture, biological soft-body response, multiplayer, castle construction, or an FPS mode is implemented.

Related: [Physics domain](CASTLE_SIEGE_PHYSICS_DOMAIN.md) · [Experiment catalog](CASTLE_SIEGE_EXPERIMENT_CATALOG.md) · [LUT policy](PHYSICS_MATH_LUTS.md) · [WebPageLLM build plan](CASTLE_SIEGE_WEBPAGELLM_BUILD_PLAN.md).

## 1. The actual question is not "does a rock follow an arc?"

The useful experiment is a causal chain:

**choose a machine → choose a payload → choose or build a target → launch → observe contact → compute the response → simulate settling → inspect and compare the outcome.**

A stone hitting masonry is one test case, not the definition of the domain. The domain should support any payload that fits the chosen machine's declared constraints and has a valid physical profile. As users add content to the ontology, they should be able to define new payload profiles by composing reusable physical/material capabilities rather than writing a bespoke catapult simulation for every asset.

The simulation must distinguish what an object looks like, what physical properties it has, and which response model is available. A mesh named "pumpkin" does not automatically supply mass, inertia, deformation, fracture, moisture, or impact behavior.

## 2. Machine envelope and payload eligibility

A machine profile describes the launch apparatus and its supported envelope.

Proposed machine properties:
- mechanism type: hand-operated catapult, torsion engine, trebuchet, spring/elastic launcher, or future machine;
- launch envelope: permitted payload mass, dimensions, shape envelope and mounting/release constraints;
- mechanism parameters: arm geometry/inertia, counterweight/tension, release conditions and declared losses when the model supports them;
- operating state and reload cycle;
- model identity/version, supported fidelity, assumptions and calibration status.

Payload eligibility is a model query, not a hard-coded list of object names. For example, an apple, pumpkin, stone, clay sphere, water-filled vessel or user-created prop can be eligible if its mass/geometry fits the machine envelope and it has enough physical metadata for the selected experiment. The interface should explain why a profile is ineligible: too heavy, too large, incompatible shape, missing mass, or unsupported response model.

Living-creature profiles can be represented as user-created ontology entries for fictional sandbox experiments, but they must not be treated as equivalent to inert solids. Their mass and collision geometry alone do not provide a biological injury/deformation model. The default experience should offer a non-graphic abstract response unless a separately justified, explicitly scoped model is implemented; never imply clinical or biological realism from a generic particle effect.

## 3. Payload capability profile

The following schema is a conceptual contract, not a current C# API.

| Field | Meaning |
|---|---|
| Asset identity/version | ontology identity and immutable profile version |
| Geometry | collision shape, dimensions, local origin and orientation |
| Mass properties | mass, center of mass, and inertia where supported |
| Material/composition | named components and provenance, not just a display label |
| Response capabilities | rigid, deformable, brittle, granular, fluid-containing, fragmenting, or unsupported |
| Parameter provenance | measured, sourced, calibrated, assumed, or illustrative |
| Model validity | valid input ranges, limitations and known exclusions |
| Machine constraints | compatible launch envelopes and release requirements |
| Presentation profile | optional appearance/effects, strictly downstream of physical results |

Capability selection should be compositional. A payload can use a rigid-body trajectory model but have no supported fracture model; a fluid-filled vessel might have a validated vessel-break event but only approximate liquid dispersion; a pumpkin may use a bounded soft-body/deformation approximation. Do not force every asset into one universal "impact damage" function.

## 4. The one-to-many impact pipeline

The visually satisfying moment is an event with physical consequences, not a particle effect triggered by proximity.

### Stage A — Incoming state
Record projectile identity, mass properties, position, orientation, linear/angular velocity where available, model version and logical tick.

### Stage B — Confirmed contact
The collision/target system identifies the actual contact point(s), normal(s), participating body IDs and pre-contact state. A visual mesh overlap is not enough.

### Stage C — Impact impulse and energy
Calculate only the quantities supported by the contact model. For a simple point-mass diagnostic, momentum is (p=mv) and kinetic energy is (E_k=\frac12m|v|^2). These are useful incoming diagnostics, not a complete contact law. A real response needs the selected model's contact, restitution, deformation or failure assumptions.

### Stage D — Payload response
Select the payload's declared response model:
- **Rigid/rebounding:** velocity and angular state change under the supported contact model.
- **Brittle/fracturing:** one body may become many fragments when the fracture model supports a criterion and topology change.
- **Deformable/soft:** shape or internal state changes within a declared approximation.
- **Dispersing material:** a bounded number of physical or aggregate parcels can be emitted by a declared model.
- **Container plus contents:** the container and contents are distinct entities; vessel failure and contents release are separate events.
- **Unsupported:** retain the valid impact observation and report that response simulation is unavailable.

### Stage E — Target response
Wall, gate, soil, timber, metal, water or other targets have their own material and structural response models. Payload response and target response are separate. A payload can break while a wall remains intact, or the target can fail while a payload remains mostly intact.

### Stage F — Secondary entities
A response may create fragments, droplets, clumps, chips, dust proxies, or other entities. Each physical entity needs stable identity, mass/size/state and declared approximation. Effects-only particles may be numerous, but they must be explicitly presentation-only and excluded from authoritative mass, collision and persistence claims.

### Stage G — Settling and aftermath
Continue simulating the created entities after the main impact:
- fragments bounce, roll, slide, pile, lodge, or come to rest according to supported contact/friction rules;
- wall or tower sections may settle or trigger a bounded secondary collapse if structural topology is supported;
- a few final pieces may fall after the main event;
- a small avalanche can be represented as a sequence of causally linked contact/failure events, not a timed animation;
- a sleep/rest criterion records when bodies can stop receiving frequent updates;
- the event timeline retains the causal chain so a user can inspect why each secondary movement occurred.

This delayed settling matters: the final state communicates mass, gravity, contact and structural response after the spectacular impact. It is a separate test target, not an optional cosmetic flourish.

## 5. LUT opportunities in this domain

Potential uses include bounded angle/force curves, validated material-response curves, payload response profiles, debris distribution approximations, and presentation/LOD mappings. Keep those families distinct. A table may select an approximate fragmentation distribution only if its physical meaning, calibration, domain and error are documented. A presentation LUT may select a splash silhouette or dust tier but must never determine whether a body fractured or how much mass exists afterward.

For every table-backed physical result, record table/model identity and version in the run record. If a payload profile or LUT changes, a run made with the previous version must remain interpretable.

## 6. Castle construction as a first-class experiment

A castle should be assembled from editable components rather than a single opaque mesh if the experiment is expected to study damage.

Initial construction vocabulary:
- foundation/terrain interface;
- wall sections and corners;
- towers and parapets;
- gate, doors and hinges;
- timber platforms and braces;
- material/structural profiles;
- attachment/support relationships;
- target IDs and optional damage zones.

For the first slice, an authored castle with independently addressable wall sections is sufficient. Later, a Workshop construction tool can let players place blocks/sections, select material and dimensions, inspect estimated mass and support relationships, and run tests against the resulting build. A visual building tool must not imply structural stability until the relevant support/load model exists.

## 7. Competitive siege mode: the back-and-forth game

The natural next layer is an alternating-turn game in which two players construct and attack. Treat this as a game/session domain consuming Physics, not as logic buried inside the solver.

### Proposed round structure

1. **Create match:** record ruleset, map/terrain version, machine set, allowed payload profiles, turn limit and win conditions.
2. **Build phase:** player A constructs or modifies a castle within material, space, time and resource constraints.
3. **Defender lock:** validate and lock the build snapshot for the round; assign stable IDs to components and supports.
4. **Attacker placement:** player B selects a permitted siege point and machine configuration from legal locations.
5. **Payload selection:** choose an eligible payload profile; show mass/size and any missing/unsupported response capabilities.
6. **Fire phase:** the server-authoritative simulation advances the declared experiment and stores the event/run record.
7. **Resolve aftermath:** continue contact, damage, structural changes and settling to a declared end-of-turn criterion or bounded simulation horizon.
8. **Review:** present trajectory, contact, target changes, secondary events and resulting castle state.
9. **Swap roles:** exchange builder/attacker roles or apply the declared round rules.
10. **Win check:** evaluate only explicit conditions, such as target objective, breach state, surviving structure, or turn limit.

This is a proposal, not an assertion that multiplayer or turn orchestration exists. The Physics kernel should not own accounts, matchmaking, turn UI, networking or game victory policy.

### Fairness and reproducibility

- A match records the exact physics/model versions, payload profiles, table digests, castle build snapshot, ruleset and random seeds for any stochastic model.
- The authoritative simulation validates the launch and all consequential events. Clients may predict or interpolate for responsiveness, but cannot decide hit, damage, failure or victory.
- If any model is nondeterministic or hardware-sensitive, declare that boundary and use an agreed authoritative result protocol; do not promise bit-identical replay without evidence.
- The turn snapshot is immutable once the attack begins. Later edits create a new version.
- Each shot produces a run record that can be inspected independently of the match.
- Define timeouts and bounded debris/body budgets to prevent a pathological castle or payload from creating unbounded work.

### Castle builder versus attacker

The asymmetry is a feature:
- the builder optimizes geometry, materials, supports, layers, gates and protected objectives;
- the attacker optimizes machine placement, angle, payload selection, repeatability and the information gained from earlier shots;
- both sides can use Workshop tools to inspect assumptions and test candidate designs before committing a match action, subject to ruleset constraints.

## 8. Path toward first-person immersion

An FPS view is a presentation/control mode on the same authoritative siege world, not a second physics implementation.

Potential stages:
1. **Observer/commander view:** see the machine, castle and whole shot.
2. **Crew-level view:** approach the launcher, operate controls, reload and inspect the mechanism; Gestures can animate crew actions from supported mechanism states.
3. **Defender view:** walk the battlements and inspect damage/settling at human scale.
4. **First-person siege phase:** navigate the venue and interact with objects while the same Physics state remains authoritative.
5. **Multiplayer presence:** replicate validated world state and events; clients interpolate presentation.
6. **Persistent campaign:** retain castle versions, damage, resources and match history only after persistence and ownership contracts exist.

The simulation must not depend on the camera. Changing viewpoint cannot change gravity, target intersection, damage or debris. First-person collision and locomotion are additional consumers of Physics, not exceptions to it.

## 9. Build sequence and gates

| Stage | Deliverable | Exit evidence |
|---|---|---|
| P0 | payload profile and machine envelope schema | validation and eligibility tests |
| P1 | deterministic projectile and confirmed target intersection | analytical/reference and boundary tests |
| P2 | rigid-body contact/impact observation | contact and impulse/energy tests |
| P3 | one nonliving deformable/brittle payload profile | explicit model limits and response tests |
| P4 | wall-section response and structural event ordering | threshold/repeatability tests |
| P5 | fragment initialization and settling | stable IDs, bounded work, sleep/rest tests |
| P6 | castle component builder snapshot | geometry/support identity and immutable version tests |
| P7 | alternating-turn match coordinator | turn rules, snapshot isolation, authoritative run record |
| P8 | first-person presentation | camera independence and host/input tests |
| P9 | broader ontology-authored payload library | schema compatibility and profile-validation tests |

Start with an inert, nonliving payload that has a defensible response model. Add other ontology payloads through explicit profiles, not special-case strings. Keep the model extensible without making the first release depend on a general-purpose soft-body, fluid, or biological simulation.

## 10. Acceptance checklist

- [ ] Machine and payload eligibility is data-driven and validated against declared envelopes.
- [ ] An asset name/mesh cannot silently imply physical parameters.
- [ ] Launch, confirmed contact, payload response, target response, fragment creation, settling and presentation are separate stages.
- [ ] Every physical fragment has explicit state and identity; effects-only particles are labelled and non-authoritative.
- [ ] Delayed settling and secondary movement have a measurable termination/rest criterion.
- [ ] LUTs are versioned and their provenance/validity/error are captured in run provenance.
- [ ] Castle components are individually addressable before player construction is enabled.
- [ ] Versus rules and turn orchestration are outside the Physics solver.
- [ ] The authoritative match stores model versions, profile/table digests, build snapshots and event history.
- [ ] FPS/crew views consume the same physical state; the camera never decides physical outcomes.
- [ ] Unsupported payloads or model capabilities fail explicitly and leave earlier valid observations intact.


## 11. Equal-kit castle duel: build your own, then attack theirs

This is the primary competitive variant alongside realistic siege experiments. It should be immediately legible like an Angry Birds-style turn-based destruction game, while allowing physical fidelity to grow toward realistic siege engineering. Do not force a choice between accessible game feel and inspectable physics: keep the rule layer simple and the simulation contract honest.

### Match promise

- Two players receive the same versioned inventory of construction parts, build-space constraints, placement rules, construction budget, and build time or move limit.
- Each independently designs and locks a castle. Neither player can edit the opponent's castle after lock.
- Both players receive the same declared siege-machine and ammunition access unless a named ruleset intentionally defines an asymmetry.
- Players alternate attacks against the opposing player's locked castle. A turn includes aiming, choosing an allowed payload, firing, and resolving the declared aftermath window.
- A round is complete when both players have attacked. Repeat until a declared victory condition, ammunition limit, turn limit, or draw condition is reached.
- Each player must defend and attack. The castle is not a fixed campaign target.

### Build phase: identical parts, different engineering

The inventory is a versioned ruleset, not a separate list generated for each player. A starter kit could contain a fixed count of blocks/segments, foundations, wall pieces, towers, braces, gate components, and a bounded number of material variants. Exact counts belong to a named match profile and must be identical for both sides.

Every construction part has stable profile identity/version, geometry and placement constraints, physical/material parameters only where supported, attachment/support semantics only where implemented, an inventory count or cost, and explicit capability/validity labels.

The builder places parts into a bounded construction volume on a defined terrain/foundation. Validation checks inventory use, legal placement, overlaps, bounds and declared attachment rules. **Do not call a castle structurally stable just because it passes placement validation.** If no load/stability solver is enabled, report build legality separately from structural stability. A castle can be legal but vulnerable; that is part of the design challenge.

Locking creates an immutable, versioned build snapshot: ordered part IDs, poses, material/profile versions, support links, terrain/map identity and validation warnings. The match records the inventory and ruleset digest. Editing after lock creates a new snapshot and is disallowed during the active match.

### Attack phase: arcade controls over a real result pipeline

The first playable control scheme should be familiar and quick:

1. Select a permitted machine/payload.
2. Aim with a trajectory preview when the active model can calculate one.
3. Set supported launch parameters (initially explicit launch speed plus elevation/azimuth, or a clearly labelled pull-and-release game control mapped to those inputs).
4. Fire one projectile.
5. Observe computed path, confirmed contact/miss, and aftermath.
6. Review a compact result: hit/miss, target section, impact speed/energy, supported damage/failure events, and remaining ammunition.

A pull-back gesture can provide satisfying Angry Birds-like interaction without pretending to be a physical sling or trebuchet. In the earliest model, the gesture maps to a declared launch-state input; its mapping belongs to the ruleset and experiment record. A future mechanism solver may replace that mapping behind the same adapter, with a new model identity and tests.

Aim assistance must be explicit. An arc can be a prediction from the active Physics model, a coarse guide, or a presentation-only hint, but its status must be visible. The preview must not secretly use a more capable model than the shot resolver. The shot result is authoritative.

### Turn resolution and win conditions

Every shot is a bounded experiment. A turn record references the match/ruleset version, attacker/defender slots, immutable castle snapshots, machine/payload profile versions and eligibility result, exact launch inputs, model versions, logical timestep, run ID, ordered observations/events, target-response status, aftermath termination reason, remaining ammunition/resources, and resulting castle-state version.

A turn ends when a supported settling/rest criterion is met, or when a declared maximum logical duration/work budget is reached. If settling is not supported, the ruleset must state a bounded fixed horizon and label aftermath incomplete; do not infer that debris has settled. Enforce maximum body count, event count, simulation steps and payload size.

Victory policy belongs to the match coordinator, not the Physics kernel. Rulesets may define a marked objective, breach/remaining-structure score, or limited-ammunition scoring. Define tie-breaks and draws before a match starts. Never derive victory from a visual explosion, arbitrary health bar, or hidden heuristic. If arcade scoring precedes structural physics, identify it as a game rule over explicit target-state events, not a real-world collapse prediction.

### Fairness, determinism and anti-cheating

- Both players receive the same build inventory, constraints, machine access, payload availability, turn policy, and simulation budget for the same ruleset.
- Fix and record map, terrain, gravity/model settings, profile/table digests and random seeds before building.
- Alternate who fires first across matches or use a declared deterministic first-player rule.
- Lock both castle snapshots before the first shot. Never regenerate or rebalance one build after seeing the opponent's design.
- The authoritative host validates construction, launch inputs, contact, damage, score and victory. Clients may predict/render but cannot decide truth.
- Freeze each turn's initial conditions and inputs. Re-runs must meet the model's declared determinism tolerance; do not promise bit-identical cross-hardware outcomes without evidence.
- Any random scatter, wind, material variation or procedural generation uses an explicit recorded seed and the same policy for both sides.

### Separate game feel from physical fidelity

| Layer | Arcade-first option | Higher-fidelity evolution |
|---|---|---|
| Build | snap-to-grid pieces and identical inventory | measured geometry, materials, support and load checks |
| Aim | direct speed/angle controls or documented pull mapping | mechanism derives launch state |
| Shot | clear arc preview and one projectile | validated forces, mechanism losses and supported payload state |
| Damage | transparent target-state/score rule | bounded material response, structural failure and topology mutation |
| Aftermath | bounded, declared turn horizon | contact/friction, fragments and tested settling/rest |
| Replay | immutable turn result and parameter comparison | checkpointed authoritative world reconstruction |
| Presentation | punchy impact camera, sound and dust cues | cues driven by supported event/energy contracts |

Arcade-first does not mean fabricated physics. A simplified rule can be fun if named, versioned, repeatable and visible. The game may award points for hitting a marked target before full structural damage exists, but must not label those points as measured collapse.

### Acceptance tests for the equal-kit duel

- [ ] Both player slots receive equivalent inventory/ruleset definitions for a symmetric match.
- [ ] Inventory counts, placement limits, resource budget and legal build volume are validated identically.
- [ ] A locked castle snapshot cannot be mutated by later builder edits or by the opponent.
- [ ] Both castles are locked before either attack begins.
- [ ] Turns alternate and target the opposing castle; ammunition is deducted once per accepted shot.
- [ ] Invalid payloads and launch inputs are rejected before simulation.
- [ ] Every result references exact run/model/profile/snapshot versions.
- [ ] Duplicate shot/impact delivery cannot double-apply damage or score.
- [ ] Timeout/work-budget termination cannot masquerade as settled aftermath.
- [ ] Victory/draw decisions derive only from declared versioned rules.
- [ ] Arcade target scoring is distinguished from computed damage or structural collapse.
- [ ] Identical versioned inputs reproduce outcomes within documented tolerance.

