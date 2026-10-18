# Physics Across Scales: Capacity and Fidelity Contract

This document defines how Physics should grow from the smallest modeled interactions to universe-scale persistent simulation. It is an architecture and validation contract, not a claim that every tier is implemented.

## The governing rule

**Scale changes the representation and work budget; it must not silently change what the model claims to know.**

A renderer may simplify appearance. A distant region may use a coarser physical model. Neither may silently erase authoritative state, invent an event, or claim fine-scale accuracy it did not compute.

Physics owns physical state transitions and model validity. Experiences own authored scenarios, objectives, scoring, and presentation. Hosts choose scheduling and resource budgets without redefining the physical laws of a model.

## Scale is multidimensional

There is no single meaningful “maximum number of objects.” Capacity depends on at least:

- **Count:** bodies, particles, cells, constraints, contacts, field samples, and pending events.
- **Coupling:** how many entities can influence one another within a step.
- **Stiffness and timescale:** the smallest stable step required by the active model.
- **Fidelity:** which state variables and interactions are represented.
- **Persistence:** how much authoritative state and history must be retained.
- **Observation:** how much detail an observer or experience requests.
- **Determinism:** whether identical inputs, model versions, ordering, and seeds reproduce the same result.
- **Hardware budget:** memory, CPU/GPU throughput, storage, and network bandwidth.

Any capacity statement must name the workload, model, timestep, hardware, fidelity, and measured error. “Millions of entities” without these conditions is not a benchmark.

## Proposed resolution tiers

These are design tiers, not currently implemented automatic LOD.

| Tier | Representation | Typical responsibility | Required invariant |
|---|---|---|---|
| Local interaction | Explicit bodies, contacts, constraints, and short timesteps | A marble touching a rail; a domino striking its neighbor | Contact and momentum/energy behavior are validated within the model's stated scope |
| Active scene | Spatially partitioned bodies and local interactions | A domino chain, marble track, vehicle, room | Broadphase only proposes candidates; narrowphase confirms contacts |
| Region | Aggregate or reduced-order state with selected local detail | A building, terrain patch, lake, industrial process | Handoffs declare conserved quantities, approximation error, and boundary conditions |
| World | Persistent cells, region boundaries, queued events, and logical time | A city, watershed, planetary surface | Leaving observation does not delete authoritative state or pause causality accidentally |
| Universe / timeline | Versioned checkpoints, ordered inputs/events, model identities, and branch lineage | Long-lived worlds, replay, historical branches | Replay compatibility and branch isolation are checked; unsupported inverse operations are not guessed |

A model may span tiers. For example, a river's broad flow can use a coarse model while a boat, shoreline, and nearby waves receive finer resolution. The interface between those models must be explicit and testable.

## Smallest-to-largest state contract

Every authoritative model should identify:

1. **Quantity and unit** — dimensioned values use declared units; conversions are explicit.
2. **State** — the minimum values needed to resume the model without reconstructing them from pixels.
3. **Model identity and version** — the equations, parameters, and schema that interpret the state.
4. **Logical time** — tick or interval represented, independent of render frame rate.
5. **Inputs and boundaries** — applied forces, energy/material transfers, external conditions, and event order.
6. **Validity domain** — assumptions, supported ranges, stability limits, and failure behavior.
7. **Diagnostics** — conservation residuals, constraint/contact evidence, approximation error, and invalid-state counters where applicable.
8. **Persistence/replay policy** — checkpoint cadence, event retention, deterministic ordering, and migration/compatibility rules.
9. **Observation contract** — which values are exact, estimated, aggregated, stale, or unavailable.
10. **Resource accounting** — work and memory cost measured for a named workload.

Models that cannot satisfy all ten immediately must document the omissions and must not claim universe-scale persistence or exact replay.

## Scheduling without making observation the physics

Simulation work should be scheduled by model requirements and relevance, not by rendering alone.

- A collision or threshold crossing may require immediate work even when no observer is nearby.
- A slow thermal model may advance at a lower cadence only when its error and coupling permit it.
- An unobserved region may use a validated aggregate evolution, or accrue logical-time debt for later catch-up; it must not simply freeze unless freezing is an explicit model rule.
- On approach, refinement must reconcile with the coarse state and report any non-zero handoff residual.
- Rendering may interpolate presentation between physical states but must never feed interpolated pixels back as authoritative state.

Each cadence choice needs tests at boundaries, delayed-event tests, and comparison against a finer-step reference.

## Capacity benchmark contract

Benchmarks must report more than frame rate. For each workload, record:

- hardware, runtime, build configuration, model/version, deterministic seed, and run duration;
- entity/cell/contact/constraint/event counts and spatial distribution;
- timestep and scheduler policy;
- wall-clock time, allocations, peak memory, and throughput;
- physical diagnostics and error relative to a declared reference;
- replay hash or equivalent reproducibility evidence where determinism is claimed.

Scale tests should grow one dimension at a time before testing combined stress. Examples: increase body count at fixed contact density; increase contact density at fixed body count; increase world size with fixed active observer region; increase timeline branches with shared history; increase particle count while measuring field approximation error.

A faster result that violates the physical error budget is not an improvement.

## Implementation gates

### Gate A — Trusted single-step kernel
- Finite, validated states and explicit units.
- Fixed logical timestep and deterministic operation order.
- Analytical/reference cases, repeatability tests, and invalid-input behavior.
- No collision/contact capabilities implied by a free-body integrator.

### Gate B — Small interacting world
- A world stepper owns the ordered stages for integration, broadphase, narrowphase, response, correction, observations, and logical-time advancement.
- Stable body/contact identities and reproducible ordering.
- Sphere/track contact and friction/rolling behavior validated before the marble experience claims successful ramp or loop traversal.
- A domino's collision and support behavior validated before claiming record-scale chain reactions.

### Gate C — Measured scaling
- Broadphase and spatial partitioning benchmarked against known workloads.
- Sleeping/waking and active-region selection preserve state and event semantics.
- Capacity claims include error, memory, and hardware.
- GPU acceleration is compared with the CPU reference for correctness and determinism; it is not a substitute for a trusted reference.

### Gate D — Persistent multiscale world
- Stable cell/region identities, versioned state, pending events, and checkpoints.
- Conservative or explicitly approximate coarse/fine handoffs.
- Approach/leave/revisit tests including events that occur while unobserved.
- Logical time advances consistently across active, inactive, and restored regions.

### Gate E — History and branching
- Ordered input/event history and durable checkpoints.
- Replay validates model/schema compatibility and fork boundaries.
- Parallel branches do not mutate parent history or each other's state.
- Reverse stepping refuses operations for which no valid inverse or recorded trace exists.

## Experience proofs

Experiences should challenge the shared tools without becoming special-case physics engines.

- **Marble Run:** one marble first; then ramps, ring crossings, loop traversal, competing routes, rolling/friction, and bounded score-driven physical evolution. Stunts are scored from physical observations, not trigger animations.
- **Domino Studio:** validate support, overlap, collision, and chain propagation at small scale before progressively increasing layout size. A layout-authoring count is not a simulated-body benchmark.
- **Universe experiments:** persist and revisit regions, compare coarse and fine models, and replay a recorded event sequence against the same model version.

Each experience publishes its setup, model/version, run record, diagnostics, and limitations so that a reader can reproduce the proof.

## Current implementation boundary

The repository currently has bounded mechanics and domain primitives, a tested free rigid-body reference integrator, broadphase candidate generation, limited sphere-sphere contact and frictionless normal impulse response, a logical-time ledger, and an initial replay executor. These pieces do **not** yet constitute a general-purpose world stepper, full rolling/track solver, persistent universe, general coarse/fine handoff system, or complete time-travel runtime.

The next engineering milestone is a deterministic CPU world stepper with explicit stage ordering and tests. Until that exists, experiences must not imply a complete interacting world merely by combining separate primitives in a scene description.

---

## Resources & Support

[Physics home](../README.md) · [Physics domain map](PHYSICS_DOMAIN_MAP.md) · [Roadmap](ROADMAP.md) · [Deterministic history](DETERMINISTIC_HISTORY_AND_EXTENSIONS.md) · [Marble stunt and evolving-physics contract](MARBLE_RUN_STUNT_SCORING_AND_EVOLUTION.md)

*If a picture is worth a thousand words… then what is a living picture worth?*
