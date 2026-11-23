# Geological terrain revision transactions (2D)

## Purpose

A geological process must not write directly into the current world. It should consume one identified immutable revision, produce a candidate, validate that candidate, and only then attempt an atomic commit. This is the first in-memory transaction boundary over the existing `TerrainHeightField2D` API.

## Contract

- `GeologicalTerrainRevision2D` gives an immutable terrain snapshot a stable revision identity. Committed revisions retain their parent revision and the event that produced them.
- `GeologicalTerrainCandidate2D.Create` requires event provenance to identify the exact source and result revisions. It runs a caller-supplied transform against the immutable source and may run a domain validation callback.
- The first candidate contract preserves dimensions, spacing, origin, coordinate frame, vertical datum, units, and positive vertical direction. It permits elevation changes only; reprojection/resampling are separate future operations.
- Candidate creation does not change the source revision or any store.
- `GeologicalTerrainRevisionStore2D.TryCommit` checks the expected source ID and exact source-revision object under a lock. Only one candidate derived from the current revision can win. Stale candidates and reused revision IDs are rejected without changing current state.
- A successful commit creates a new immutable revision with parentage and event provenance.

## Validation

Focused tests cover source immutability, event/source/result identity mismatch, geometry mismatch, caller validation rejection, successful commit and parentage, stale competing candidates, wrong expected source IDs, and revision-ID reuse.

## Limits

This is an in-memory, single-current-revision commit point. It is not durable persistence, a multi-branch event ledger, distributed concurrency, a geological replay adapter, or a physical process solver. The caller's transform and validator must be deterministic and must not mutate external authoritative state. The store can guarantee atomic replacement of its own current revision, not rollback of arbitrary side effects inside caller callbacks.
