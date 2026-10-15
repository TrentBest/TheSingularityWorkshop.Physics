# Deterministic History and Optional Simulation Extensions

**Status:** architectural contract and proposed extension seams. This document does not claim that reverse stepping, prediction, rollback, or replay execution are implemented.

## The key distinction: repeatable update versus reversible update

A deterministic simulation should define one authoritative logical-tick transition. Given the same initial state, model/configuration versions, ordered inputs, timestep, and deterministic random streams, the transition should produce the same declared result within the model's numerical contract.

That does not make the transition invertible. Collision response, clamping, destruction, lossy approximations, and external side effects may discard information. Calling a reversed update order cannot recover information that was never retained.

Therefore, ReverseUpdateOrder() is a useful operation only as part of a stronger contract:

- **Reverse execution order** walks the recorded operation/dependency order backward.
- **Inverse operation** restores the prior state of one operation and declares what history it needs.
- **Reverse step** succeeds only when every operation in the selected step can be reversed with sufficient retained information.
- **Reconstruction** restores a compatible checkpoint and deterministically replays forward to the target tick. This remains the general fallback when inverse execution is unavailable.

The engine must not silently label reconstruction as a mathematical inverse. Both can return to an earlier state, but they have different cost and guarantees.

## Authoritative tick contract

A future tick executor should make its boundaries explicit rather than letting extension order emerge from incidental registration order:

1. Select the logical tick and immutable model/configuration identity.
2. Gather inputs and pending events for that tick in a stable, recorded order.
3. Execute the declared model/update dependency order with deterministic tie-breaking.
4. Resolve coupled operations using documented ordering and solver rules.
5. Commit the resulting authoritative state and generated events as one logical transition.
6. Record enough provenance to reproduce the transition: inputs, event sequence, timestep, model/schema versions, solver settings, and deterministic random-stream identity/state.
7. Optionally emit a state digest and measurements for verification.

The precise phases will depend on the supported physics models. A phase list is not a substitute for a tested executor, and a hash verifies equality of bytes rather than physical correctness.

## Reverse-order semantics

A reverse-order capability should operate on a recorded execution trace or declared dependency graph—not merely reverse the order of a collection and invoke the same methods.

For a step with operations A → B → C, reverse execution considers C → B → A, but it can restore the prior state only if each operation has an applicable inverse and the inverse's required data was retained. If C destroyed information needed by B, reverse ordering alone is insufficient.

A future capability should report whether reversal is supported for a particular trace and why it is unavailable. It must not silently skip non-reversible operations or pretend a partial reversal is a complete state restoration.

## History and timeline reconstruction

The baseline history path is:

1. Resolve the target reality and timeline.
2. Select the nearest compatible checkpoint at or before the target tick.
3. Validate schema/model compatibility and history continuity.
4. Replay recorded inputs/events forward in deterministic order.
5. Verify available state hashes and model invariants.
6. If the user changes the past, fork a child timeline at that tick. Keep the parent immutable.

This is why the current checkpoint and event types are data contracts first. They do not yet implement a serializer, durable history store, replay executor, or state restoration.

## Optional extensions, not mandatory dependencies

The core physics package should define authoritative model behavior and stable seams. Optional capabilities can be separate packages or adapters, selected only when a host needs them. A consumer that only advances physics must not need to reference prediction, time travel, a renderer, a UI, or FSM_API-specific integration.

Potential capability families include:

- **History/replay:** checkpoint storage, event capture, reconstruction, compatibility checks, and state verification.
- **Reverse execution:** inverse-operation contracts and trace-based reverse stepping for models that support it.
- **Branching:** isolated child timelines that share immutable pre-fork history and own post-fork changes.
- **Prediction:** execute a speculative branch from a known state and input assumptions, then keep or discard it without mutating the authoritative timeline.
- **Rollback/resimulation:** restore an earlier checkpoint, incorporate corrected inputs, and replay the affected interval.
- **Inspection and analytics:** measurements, causal traces, state diffs, sensitivity experiments, and explainers.
- **Presentation adapters:** interpolation, visualization, and host-specific update scheduling, kept outside the authoritative physics model.

These are candidate extension families, not a promise that every one must be implemented. Each should have a clear capability contract, version compatibility, cost model, and failure mode.

## Prediction is a branch, not a fact

Prediction should be opt-in and explicitly scoped by starting state, model versions, input assumptions, horizon, and fidelity. It explores a possible continuation; it does not overwrite the authoritative world or claim certainty about unprovided future inputs.

A prediction result should be labeled with its assumptions and status. If the assumptions change, the result may be invalidated or recomputed. Multiple candidate futures can be compared without requiring the system to persist every hypothetical reality forever.

## Isolation and ownership rules

- The authoritative timeline owns committed physical state.
- A prediction or alternate-history branch must not mutate its parent.
- Shared history may be immutable and shared; mutable state must be isolated.
- External side effects should be emitted through explicit commit boundaries so speculative runs do not accidentally send messages, grant rewards, write files, or affect unrelated realities.
- The core package must not require every consumer to implement every extension.
- A capability must state whether it guarantees exact replay, bounded numerical agreement, approximate prediction, or only metadata validation.

## Validation gates before implementation claims

Before marking a capability implemented, tests should establish its actual contract:

- identical initial state and ordered inputs reproduce the declared deterministic result;
- update order is explicit and stable when models are registered in different incidental orders;
- reverse execution refuses traces containing unsupported non-reversible operations;
- reversible operations restore prior state, including retained side information;
- replay from a compatible checkpoint reaches the expected state and detects incompatible history;
- speculative branches leave authoritative and sibling state unchanged;
- prediction records assumptions and never silently commits to the authoritative timeline;
- failures during a tick do not leave a partially committed state;
- deterministic claims identify their scope, numerical tolerance, platform limits, and external dependencies.

## Relationship to FSM_API

FSM_API can coordinate discrete lifecycle and process-group scheduling through a separate adapter. Its Update/processing-group cadence is not itself a physics timestep, numerical integration algorithm, or reverse-history engine. Physics must own its logical tick and solver semantics; adapters may coordinate when those capabilities run without making the core depend on FSM_API.
