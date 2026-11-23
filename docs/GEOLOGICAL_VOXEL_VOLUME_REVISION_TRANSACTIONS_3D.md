# Geological Voxel-Volume Revision Transactions (3D)

## Purpose

A sparse geological volume is not only voxel occupancy and material IDs. Each occupied voxel also carries layer and formation-event provenance. Those two structures describe one state and must advance together.

`GeologicalVoxelVolumeRevision3d` makes the pairing explicit:

- `Envelope` is the immutable, spatially explicit sparse material-volume snapshot.
- `Provenance` is the complete immutable mapping for every occupied voxel.
- The provenance object must retain the exact same envelope object; equal metadata or matching revision strings are not sufficient.

This prevents a valid provenance map from an earlier snapshot being accidentally paired with a later volume.

## Candidate construction

`GeologicalVoxelVolumeCandidate3d.Create` accepts an exact source revision, a result envelope, the result's complete provenance mapping, and a `GeologicalEventRecord`. It validates:

1. The event source revision equals the source revision identity.
2. The event result revision equals the result envelope identity.
3. The result identity differs from the source identity.
4. The provenance map is bound to the exact result envelope.
5. Region, dataset, coordinate frame, linear unit, axis convention, vertical datum, vertical direction, material catalogue, grid origin, voxel size, and chunk size remain stable.

The occupied voxel set and material IDs may change, but every occupied result voxel must have exactly one annotation and each annotation's material ID must agree with the result snapshot. A process may not silently change coordinate systems, resample the grid, or switch material catalogues inside this transaction. Those operations need explicit conversion/migration contracts.

Candidate construction is side-effect free with respect to the source revision and store. The process that generated the candidate is outside this transaction boundary.

## Atomic commit

`GeologicalVoxelVolumeRevisionStore3d` is a thread-safe in-memory store with one current revision. `TryCommit` uses optimistic concurrency and a lock:

- the candidate must reference the exact current source revision object;
- the caller's expected source revision ID must match current;
- the event source revision ID must match current;
- the result revision ID must never have been committed before.

On success, a single new immutable revision is created with parentage and event provenance, and the complete envelope/provenance pair becomes current together. On rejection, current state remains unchanged and is returned to the caller. Two candidates prepared from the same source cannot both advance the store.

## Validation coverage

Focused tests cover:

- exact envelope/provenance object binding;
- source/result event identity validation;
- spatial-grid compatibility;
- successful atomic advancement of voxel state and provenance;
- stale competing candidates;
- incorrect expected source identity;
- revision identity reuse;
- unchanged current state after rejected commits.

## Deliberate limits

This is an in-memory single-current-revision boundary. It is not durable persistence, distributed consensus, a multi-branch history store, or an event replay executor. It does not generate geological processes, infer layer assignments, deform strata, or validate physical conservation. A caller must produce a physically valid candidate and complete provenance before offering it for commit.

The store also preserves a fixed spatial grid and material catalogue. Reprojection, resampling, grid refinement, region migration, and material-catalogue migration must be explicit operations with their own provenance and validation contracts.

## Relationship to the 2D terrain transaction

`GeologicalTerrainRevisionStore2D` protects the immutable exterior height field and its event metadata. This 3D store extends the same optimistic-commit principle to the paired sparse material volume and per-voxel geological provenance. Neither store is durable persistence or a substitute for a physical process solver.
