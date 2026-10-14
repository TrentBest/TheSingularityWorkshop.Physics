# Logical Time Ledger

## Why this exists

A persistent world cannot equate “not currently observed” with “does not exist.” A region may be processed less frequently, but when it is revisited the runtime needs an explicit account of logical time that remains to be processed. It must not silently jump its authoritative tick forward or discard overdue work.

## Contract implemented

`LogicalTimeLedger` tracks three things:

- **Current tick** — the latest logical tick whose work the caller has committed as complete.
- **Target tick** — the furthest requested logical time.
- **Pending ticks** — `TargetTick - CurrentTick`, the catch-up debt still outstanding.

Ticks are non-negative integer positions in a model's declared logical timebase. This type does not define seconds-per-tick, execute physics, schedule process groups, or choose a catch-up policy.

The caller requests a horizon with `RequestThrough`, asks for a bounded contiguous range with `PlanNextBatch`, processes that range, and calls `Commit` only after successful completion.

Planning does not mutate the ledger. A batch must begin at the current tick, cannot extend beyond the target, and cannot overwrite progress made since it was planned. Extending the target is allowed while debt remains; lowering it is rejected so an ordinary scheduling request cannot erase history. Represent an alternate past with a separate timeline branch instead.

## Example

```csharp
var time = new LogicalTimeLedger();
time.RequestThrough(100);

var batch = time.PlanNextBatch(maximumTicks: 25);
// Process every tick in [batch.StartTick, batch.EndTick) successfully.
time.Commit(batch);

// CurrentTick = 25; PendingTicks = 75.
// The region is not caught up yet.
```

The batch range is start-inclusive and end-exclusive. This makes its tick count equal to `EndTick - StartTick`, including at the zero tick.

## Boundaries and next work

The ledger is a time-accounting primitive, not a simulation scheduler. It does not prove that the caller really executed the model, make a failed physical step transactional, or decide whether old work can be approximated by a coarse model. Those guarantees require model-specific integration and tests.

Still outstanding:

- fixed logical heartbeat and process-group scheduling;
- per-model catch-up policy, event ordering, and coupled-domain coordination;
- persistence of ledger state alongside world checkpoints;
- replaying recorded inputs/events to reconstruct a historical tick;
- waking dormant regions and reconciling transitions that occurred during absence;
- evidence that coarse catch-up preserves required conservation and error bounds.

See [process-group scheduling](PROCESS_GROUP_SCHEDULING.md), [universe and observer resolution](UNIVERSE_AND_OBSERVER_RESOLUTION.md), and [timelines and multiverse](TIMELINES_AND_MULTIVERSE.md).
