# Timelines, Time Travel, and Parallel Realities

**Status:** design contract with an initial immutable timeline-lineage type. Checkpoint storage, event replay, physical restoration, and reality orchestration are not implemented yet.

## The core idea

A timeline is a versioned history of a reality, not merely a clock that can be set backward. To revisit the past, restore a known physical checkpoint at or before the requested logical tick, replay the recorded inputs/events to that tick, then fork a new timeline for subsequent changes.

The original timeline remains available. A child timeline shares the parent's history up to its fork point and owns its own subsequent history. The storage implementation should use immutable/shared history segments or equivalent references so branching does not require copying every byte of the past.

## Three separate identities

- **Reality** identifies a world-level set of physical rules and initial conditions. Different realities may vary laws, constants, topology, or initial state, within declared model support.
- **Timeline** identifies one history within a reality, including its ancestry and divergence point.
- **Checkpoint** identifies a serialized authoritative state at a specific reality, timeline, logical tick, schema version, and model/configuration version.

These identities must not be collapsed into a single ID. A timeline fork does not automatically mean a new reality; a user can create another reality independently, or use a timeline fork for ordinary alternate history.

## Reconstructing a moment

A reconstruction request needs enough information to reproduce the target state:

1. Select a reality and timeline.
2. Find the nearest compatible checkpoint at or before the target logical tick.
3. Load the checkpoint and replay ordered events and deterministic inputs to the target tick.
4. Verify model/schema compatibility and any available state hash or invariants.
5. Create a child timeline whose fork point is the target tick.
6. Apply new inputs only to the child timeline.

Replay must record model versions, input/event order, logical timestep, deterministic seeds/streams, solver configuration, and relevant external data. If an input is missing or a model version cannot be reproduced, the engine must report that limitation rather than claim exact restoration.

## Branching is not rewriting

A branch preserves its ancestry. It must not mutate the parent's event history or checkpoints. A branch may refer to shared immutable history until divergence and then store only its own events/checkpoints. Garbage collection of unreachable branches is a separate, explicit retention policy; it must not silently erase a branch the user still owns.

The `TimelineBranch` type represents validated lineage metadata and a represented logical head. `TimelineLineage` validates a root-to-target ancestry within one reality and can assess whether a checkpoint is eligible as a starting point for a requested tick, including the shared-history boundary at each fork. This is only an ancestry/range check: it does not establish schema compatibility, event continuity, or replay fidelity. `PhysicsCheckpoint` provides an immutable envelope for opaque serialized state bytes, identity/version metadata, and a SHA-256 payload digest. `TimelineEvent` represents one ordered input/event record, and `TimelineHistorySegment` validates an event sequence against a checkpoint and target tick. These are contracts and validation primitives only: there is still no durable store, model-specific serializer, replay executor, solver restoration, or proof of replay fidelity. See [Deterministic history and optional extensions](DETERMINISTIC_HISTORY_AND_EXTENSIONS.md) for the distinction between replaying forward and reversing operations, plus the proposed seams for prediction and rollback.

## Parallel realities and multiverse scale

A multiverse is a set of separately addressable realities and their timeline trees. It need not be a simultaneous full-fidelity simulation of every possibility.

- Share immutable assets, model definitions, and compatible historical data where safe.
- Keep mutable physical state and event histories isolated unless an explicit cross-reality interaction model says otherwise.
- Give each reality its own stable identity, initial-condition inputs, ruleset/model versions, and persistence namespace.
- Schedule computation according to declared relevance and required fidelity, while preserving logical time and causal events.
- Treat cross-reality travel, communication, or energy transfer as explicit modeled interactions, not accidental shared memory.

Reality creation and timeline branching are data-model operations; the physical consequences within each reality still require the relevant physics models to run.

## Validation gates

Initial data-contract tests now cover checkpoint payload isolation/digest, identity and tick validation, event ordering, and checkpoint-relative event bounds. Before claiming time travel or multiverse simulation is implemented, also test:

- restoring a checkpoint and replaying the same event stream reproduces the declared deterministic contract;
- forking at tick *t* leaves the parent unchanged and both branches share only pre-fork history;
- events after divergence affect only their own timeline;
- a missing or incompatible checkpoint/model is surfaced explicitly;
- parallel realities do not leak mutable state into each other;
- logical-time catch-up and pending events are reconciled before a dormant region or branch is interacted with;
- storage cost and replay cost are measured against checkpoint interval, event volume, and branch count.

The user-facing “Time Travel Photo Bomb” concept is an excellent application of this model: visit a historical moment, introduce deliberate anachronisms, preserve the original history, and score the creativity of the resulting alternate history. Artistic scoring and game rules belong above this domain contract; they must not be confused with physical history persistence.
