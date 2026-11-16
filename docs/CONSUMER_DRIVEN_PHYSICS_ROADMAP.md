# Consumer-Driven Physics Roadmap: Lenny and raWWar

**Purpose:** use two real Experience demands to prioritize reusable Physics capabilities without making Physics depend on either Experience or duplicating their domain logic.

This is a demand map, not a claim that the listed capabilities exist. Status is grounded in the current Physics development contracts and the linked consumer designs. Update implementation status only after code, tests, and reproducible evidence exist.

## Ownership boundary

| Concern | Owner |
|---|---|
| Story, character identity, cleanup lifecycle, litter identity, deposit count | Experiences / Lenny |
| Formation plans, membership, stable slots, command/readiness, campaign and organization semantics | raWWar |
| Intent, authored movement procedures, character-specific coordination | Gestures / Experience controllers |
| Units, coordinate/time semantics, physical state evolution, contact, loads, energy transfer, model provenance, achieved observations | Physics |
| Provider discovery, host lifecycle, authoritative world commits, runtime bridge | Host / MicroBundle composition |
| Camera, visual interpolation, artwork-relative targets, scene presentation | Renderer / Visualization / host |

Physics must remain independent of Experiences, raWWar, Gestures, Renderer, Visualization, Foundry, FSM_COS, and host frameworks. Consumer-specific adapters translate intent and consume observations.

## Consumer demand: Little Lenny and the Big Ocean Problem

Lenny's existing living-world model describes optional cleanup interactions: approach a story-grounded litter item, pick it up, carry it to the receptacle, and deposit it. The Experience-owned reducer protects the held/deposited transaction and increments the count only after a matching deposit-completion event.

### Already present

- Experience-owned actor/scene/cleanup model and deterministic reducer tests.
- Physics prescribed root-motion definitions with fixed-step evaluation, constant-velocity and quintic smooth-step profiles, speed/acceleration ceiling validation, analytical per-sample acceleration observations, and requested-versus-achieved commit validation.
- Typed Physics MicroBundle provider.
- Experiences-owned C# host-composition sample that resolves the typed provider and validates fixed-step commits using a synthetic no-contact path.

These do **not** yet constitute a Physics-driven beach scene. The sample accepts its prescribed path as achieved under an explicit no-contact host policy. The static browser reader has no live C# runtime bridge or Physics-driven animation.

### Physical capabilities the scene will eventually need

1. **Root motion and interruption:** achieved actor pose/velocity, bounded acceleration, deterministic cancellation, and stable logical time. Prescribed kinematics now covers the bounded path-evaluation part.
2. **World stepping and static geometry:** one authoritative step order for actor and object state; explicit scene bounds and supported collision shapes.
3. **Contact and support:** distinguish touching litter from supporting it, grasping it, or carrying it. Contact alone must never imply a successful pickup.
4. **Grasp/attachment:** a declared hand-to-item constraint or equivalent host-owned attachment with explicit release and interruption semantics. A visual parent-child transform is not physical proof.
5. **Obstacle-aware movement:** beachgoers, towels, sandcastles, receptacles, and other obstacles must be represented by physical geometry before avoidance or clearance can be claimed.
6. **Environmental coupling:** sand/ground support and, for the ocean sequence, water-relative motion, buoyancy, drag, waves/currents, and safe human/dolphin interaction models. Existing shallow-water demonstrators are bounded models over flat/fixed beds; they are not coupled actor locomotion or general surf simulation.
7. **Evidence:** fixed-step replay, different render-cadence equivalence, contact transition tests, interruption during approach/carry/deposit, and exactly-once Experience deposit commits.

Do not add visible litter hit regions until the author approves the relevant artwork and placements. Do not change the canonical manuscript to store runtime interaction state.

## Consumer demand: raWWar

raWWar's authored direction is a continuous, persistent Experience with first-person embodiment and multiple ways to participate: military base life, ground engagements, training, logistics, spacecraft and lived-in ships, docking, freight/passenger service, exploration, piracy, commerce, and organizations. Scale progresses from person and exoskeleton through craft, ship, capital ship, fleet, alliance, and galaxy. Organizational control changes player agency; it does not create a separate detached universe.

Its current formation issue requires stable formation-local slots and Experience-owned formation lifecycle. The coordinator issues targets and observes authoritative physical results; it must not synthesize locomotion, collision, or occupancy.

### Physical capability families implied by that design

1. **Embodied actors and exoskeletons:** force/torque-driven rigid bodies, articulated constraints, joint reaction loads, contact support, friction, and declared limits. Structural failure, body topology, tissue injury, and gameplay capability changes remain distinct models/owners.
2. **Ground movement and formations:** achieved root/body state, collision-aware motion, contact clearance, support, interruption, and repeatable fixed-step execution. Formation membership and slot policy stay in raWWar.
3. **Weapons and impacts:** projectile trajectories, gravity, declared drag/wind where supported, time-of-impact detection, contact impulses, and eventually material response. A broadphase candidate is not a hit; a hit is not automatically damage or destruction.
4. **Vehicles and spacecraft:** six-degree-of-freedom state, mass/inertia, force/torque application, thruster and control inputs, and declared gravity/flight regimes. Existing orbital and free-flight primitives are not a complete spacecraft flight or combat model.
5. **Ships, docking, and cargo:** stable body/shape identity, swept collision bounds, docking/contact constraints, cargo mass properties, and explicit load/structural observations. Lived-in interiors add navigation and interaction geometry but remain Experience content.
6. **Structures and destruction:** constraint reaction loads with declared temporal semantics, structural thresholds, material response, fracture/debris topology, and energy accounting. Current single-contact frictionless sphere response and compliant-link experiments do not provide this system.
7. **World and environmental continuity:** terrain, water, weather, gravity, and material models with persistent region state, checkpoints, coarse/fine handoff, and elapsed-time reconciliation while unobserved. Observation may change resolution, never the authoritative history.
8. **Scale and performance:** deterministic CPU reference behavior first; then measured multi-rate execution, spatial partitioning, conservative bounds, and optional acceleration. GPU readiness or a visual demo is not evidence of a GPU solver or large-population capability.

Fictional or magical technology may introduce declared model extensions, but must not silently redefine ordinary physical quantities or claim validated real-world physics.

## Shared implementation order

The two consumers point to a common dependency sequence. Build and validate it once:

### 1. Deterministic CPU world stepper

**Existing anchor:** Physics issue [#31](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/31).

Compose supported free-body integration, stable body ordering, broadphase candidate generation, supported sphere-sphere narrowphase, the currently bounded single-contact normal response, and immutable step observations. Reject unsupported shapes and response modes explicitly. Produce a reproducible small-world record and correctness/benchmark baseline.

### 2. Contact mechanics beyond the current single-contact baseline

Add angular contact response, friction, persistent contact identity/manifolds, deterministic multi-contact solving, and a clearly scoped support classification. Add conservative swept bounds and time-of-impact refinement for accelerated, curved, and rotating motion before claiming continuous collision detection. Keep candidate, predicted, confirmed, and committed interaction states distinct.

### 3. Articulated constraints and structural loads

**Existing anchor:** Physics issue [#1](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/1).

Implement one small articulated scenario with declared masses/inertias and solver assumptions. Emit per-step constraint reaction force/torque with reference frame, point/axis, and temporal semantics. Validate threshold crossings. Keep failure detection separate from topology mutation and injury.

### 4. Grasping and attachment as a constrained physical interaction

Build only after contact and articulated constraints have credible load observations. A pickup should be supported by a validated contact/attachment state; release and interruption should preserve the last committed state. The Experience still owns the pickup/deposit story transaction.

### 5. Environmental and material coupling

Connect mechanics to existing material, thermal, hydrodynamic, and terrain capabilities through explicit boundary conditions and conservative exchanges. Start with bounded, independently validated couplings such as actor/rigid-body buoyancy and drag in a declared water model; do not claim general surf, soft tissue, or fracture behavior prematurely.

### 6. Persistent multi-rate world

Complete region/cell persistence, checkpoint and event history, observer-driven resolution tiers, coarse/fine model handoffs, and deterministic leave/revisit reconciliation. Establish measured error/cost trade-offs before scaling population or world claims.

## Immediate engineering focus

- Finish the prescribed-motion acceleration observation and its tests (now implemented on Physics development).
- Complete issue #31's deterministic CPU world-stepper vertical slice.
- Then advance issue #1's articulated constraint/load proof.
- Use Lenny as the first constrained interaction consumer: root movement → supported contact → attachment → carry/release → Experience deposit transaction.
- Use raWWar as the demanding integration consumer: formation clearance → embodied articulated actors → impacts/structural loads → vehicles/ships → persistent shared-world continuity.

The sequence is intentionally dependency-driven. Do not skip to a visually impressive destruction or combat demo before the underlying contact, load, and state-transition evidence exists.

## Sources

- [Physics roadmap](ROADMAP.md)
- [Prescribed kinematic motion](PRESCRIBED_KINEMATIC_MOTION.md)
- [Articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md)
- [Motion-driven interaction pipeline](MOTION_DRIVEN_INTERACTION_PIPELINE.md)
- [Deterministic CPU world stepper issue #31](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/31)
- [Articulated mechanics issue #1](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/1)
- [raWWar formation lifecycle issue #7](https://github.com/TrentBest/raWWar/issues/7)
- [raWWar shared-Experience architecture issue #8](https://github.com/TrentBest/raWWar/issues/8)
- [Lenny Physics integration contract](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/blob/development/experiences/Lenny/PHYSICS_INTEGRATION.md)
- [Lenny living-world model](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/blob/development/experiences/Lenny/living-world.json)

---

*The Singularity Workshop — edify, not mystify.*
