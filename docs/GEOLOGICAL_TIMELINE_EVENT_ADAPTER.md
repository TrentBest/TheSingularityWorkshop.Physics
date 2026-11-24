# Geological Timeline Event Adapter

GeologicalTimelineEventAdapter bridges immutable GeologicalEventRecord provenance into the repository's existing ordered timeline history.

## Contract

- Encode(sequence, event) creates a TimelineEvent using the stable kind geology.event-record.
- The timeline tick is the event's GeologicalTimeInterval.StepIndex; geological age and simulation-seconds clocks remain in the payload and are not conflated with the logical timeline tick.
- The payload is a versioned JSON envelope (schemaVersion: 1) containing the complete geological event record.
- Decode rejects another event kind, malformed or unsupported payloads, and any mismatch between the timeline tick and geological step index.
- Timeline sequence ordering remains owned by TimelineHistorySegment; this adapter does not sort or rewrite events.

## Replay boundary

Use the existing TimelineReplayPlan, TimelineReplaySegment, and TimelineReplayExecutor for checkpoint eligibility, lineage routing, event ordering, and deterministic reconstruction. A geological replay callback can call Decode when it receives a TimelineEvent, then dispatch the typed record to the appropriate versioned geological process implementation.

This adapter is a transport and validation boundary only. It does not execute a geological process, load revisions, prove model determinism, or commit state. The geological model adapter remains responsible for validating its inputs and producing a deterministic candidate; GeologicalVoxelVolumeRevisionStore3d remains the atomic in-memory commit boundary.

## Example

```csharp
TimelineEvent timelineEvent = GeologicalTimelineEventAdapter.Encode(sequence, geologicalEvent);

GeologicalEventRecord replayEvent =
    GeologicalTimelineEventAdapter.Decode(timelineEvent);

var reconstructed = TimelineReplayExecutor.Reconstruct(
    segment,
    targetTick,
    restoreCheckpoint,
    (state, item) => ApplyGeologicalEvent(
        state,
        GeologicalTimelineEventAdapter.Decode(item)),
    advanceBetweenEvents);
```

The callbacks above are model-specific. They must define checkpoint serialization, event-to-process dispatch, deterministic advancement, and physical conservation checks.
