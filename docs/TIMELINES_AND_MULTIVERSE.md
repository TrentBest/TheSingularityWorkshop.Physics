# Timelines, Time Travel, and Parallel Realities

**Status:** validated lineage/replay contracts plus model-driven segment and fork-aware route executors. Durable checkpoint storage, solver-specific restoration, portable moment packages, and reality orchestration are not implemented yet.

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

The `TimelineBranch` type represents validated lineage metadata and a represented logical head. `TimelineLineage` validates a root-to-target ancestry within one reality and can assess whether a checkpoint is eligible as a starting point for a requested tick, including the shared-history boundary at each fork. This is only an ancestry/range check: it does not establish schema compatibility, event continuity, or replay fidelity. `PhysicsCheckpoint` provides an immutable envelope for opaque serialized state bytes, identity/version metadata, and a SHA-256 payload digest. `TimelineEvent` represents one ordered input/event record, and `TimelineHistorySegment` validates an event sequence against a checkpoint and target tick. `TimelineReplayPlan` adds a forward route from an eligible checkpoint through timeline-owned intervals, splitting at each fork and requiring exact schema/model version labels. Child-owned events begin strictly after the fork tick; the fork tick remains part of inherited history. The route is still a plan rather than a history store. `TimelineReplaySegment` validates a history fragment against one route interval. `TimelineReplayExecutor` now restores that segment's checkpoint through a caller-provided decoder, applies its events in sequence, and advances the caller's model to an inclusive target tick; the target can be earlier than the interval end, which provides a tested foundation for scrubbing. The caller supplies event application and deterministic advancement, so this seam does not make a model deterministic or prove physical fidelity. `TimelineReplayRouteExecutor` now composes one validated history segment per route interval in order, restores each child checkpoint at its fork boundary, and rejects a boundary whose state does not match the preceding reconstructed state according to a caller-provided comparer. This is a generic, tested coordination seam, not a concrete solver adapter. A focused integration test now exercises `TimelineReplayExecutor` with the repository's actual `ConstantAccelerationSolver`: it restores a kinematic checkpoint, applies a recorded velocity-change event at its tick, and checks position, velocity, and elapsed time at the target tick. That test demonstrates one model-specific adapter contract, not a general world serializer or proof of cross-platform bitwise determinism. Durable storage, general model-specific serialization, and physical-fidelity verification remain future work. See [Deterministic history and optional extensions](DETERMINISTIC_HISTORY_AND_EXTENSIONS.md) for the distinction between replaying forward and reversing operations, plus the proposed seams for prediction and rollback.

## Shareable moments: replay, scrub, and remix

A replay should be more than a developer log. It should become a creator-owned, portable way to return to a clutch moment, inspect how it unfolded, and invite someone else to experience or remix it.

Treat a **Moment** as a named, bounded view into a timeline, not as a screenshot and not necessarily as a whole saved world. A moment package should eventually carry:

- **Identity and provenance:** stable moment ID, creator/attribution, source reality and timeline, capture tick, and parent/remix reference where applicable.
- **A reconstruction anchor:** a compatible checkpoint reference or embedded checkpoint, plus the fork-aware replay route and the ordered inputs/events needed to reach the selected moment.
- **A playable window:** a start tick, highlight tick, and end tick so a viewer can scrub before and after the clutch moment rather than land on one frozen frame.
- **Compatibility declaration:** physics/model, schema, timestep, solver settings, deterministic random-stream identity, and required content/assets with versions or content hashes.
- **Presentation hints:** optional camera/view state, annotations, captions, and a thumbnail or short preview. These enrich presentation but are not authoritative physical state.
- **Integrity and limits:** payload digests, bounded resource declarations, and explicit status when content is missing, incompatible, or only approximately reproducible.

A share link should resolve to a stable moment identifier and a versioned manifest. Depending on size and licensing, the manifest can reference content-addressed assets hosted by a provider or package a small self-contained replay. A link is not a substitute for durable storage, and a digest detects changed bytes but does not prove that a simulation is physically correct.

### A creator-first sharing loop

1. **Capture:** mark the moment, choose the scrub window, and optionally add a title, description, and annotations.
2. **Preview:** reconstruct it locally and verify that the highlight and surrounding window behave as expected.
3. **Share:** create a public, unlisted, or private share according to host capability; show required downloads and compatibility before loading.
4. **Experience:** let the recipient scrub, inspect events, and understand the context without needing a developer toolchain.
5. **Remix:** let the recipient fork from an authorized point, make changes on a child timeline, and credit/link back to the source without mutating the original.
6. **Publish a derivative:** produce a new moment manifest with its own identity and explicit ancestry, so interesting branches can be discovered and shared again.

Privacy and ownership are product requirements, not afterthoughts. A share must not expose private world state, hidden assets, personal data, or credentials merely because those bytes happened to be captured in a checkpoint. Hosts need access controls, retention/deletion rules, quota and abuse controls, and clear licensing/attribution. Private by default is safer until the creator chooses otherwise.

For adoption, optimize the path from “that was amazing” to “watch this” to “try my version.” Keep a small replay lightweight enough to share, disclose large dependencies before download, and offer a clear fallback when exact reconstruction is unavailable. Do not silently substitute a different simulation and call it the same replay.

This is an intended product direction, not a claim that the full experience exists. A validated single-segment history can now be replayed through caller-supplied model operations to an intermediate tick, but Physics does not yet load durable history, coordinate execution across a forked route, provide a concrete world/solver adapter, render a scrubber, generate share links, or host user content. The hosting and social layer should remain separable from the renderer-independent physics core.

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

- restoring a checkpoint and replaying the same event stream reproduces the declared deterministic contract (initial generic executor tests cover ordered events and intermediate target ticks; model-specific fidelity remains to be demonstrated);
- forking at tick *t* leaves the parent unchanged and both branches share only pre-fork history;
- events after divergence affect only their own timeline;
- a missing or incompatible checkpoint/model is surfaced explicitly;
- parallel realities do not leak mutable state into each other;
- logical-time catch-up and pending events are reconciled before a dormant region or branch is interacted with;
- storage cost and replay cost are measured against checkpoint interval, event volume, and branch count.

The user-facing “Time Travel Photo Bomb” concept is an excellent application of this model: visit a historical moment, introduce deliberate anachronisms, preserve the original history, and score the creativity of the resulting alternate history. Artistic scoring and game rules belong above this domain contract; they must not be confused with physical history persistence.
