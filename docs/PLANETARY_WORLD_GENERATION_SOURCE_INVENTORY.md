# Planetary world-generation source inventory

**Status:** source-to-capability audit of `development`. This inventory distinguishes implemented reusable foundations from specified or missing planetary/geological models. It is not a claim that a complete planet generator exists.

## Architectural target

A generated world is a versioned, replayable physical history:

`stellar system and orbit → planetary initial conditions → interior and thermal state → lithosphere and plates → tectonic/event history → climate and hydrology → weathering/erosion/transport/deposition → stratigraphy and 3D subsurface → queryable world revisions`

The sequence is a dependency map, not a requirement that every subsystem use the same numerical timestep. Player-facing intervals may span millions of years while individual solvers subcycle at shorter, model-appropriate intervals.

## Existing source-to-capability inventory

| Capability | Existing source | Status and boundary |
|---|---|---|
| Orbital bodies and Newtonian gravity | `Physics/Astronomy/OrbitalBodyState.cs`, `NewtonianGravityField.cs`, `NewtonianNBodyStepper.cs` | Implemented orbital state and N-body stepping. Does not model stellar evolution, planet formation, radiation balance, or planetary interior structure. |
| Orbital encounters and tides | `Physics/Astronomy/OrbitalEncounterCoordinator.cs`, `OrbitalGravityEncounterCoordinator.cs`, `OrbitalGravityEncounterResult.cs`, `OrbitalMergeResolver.cs`, `OrbitalSweptCollisionDetector.cs`, `OrbitalTidePotentialBaker.cs` | Existing specialized encounter/tidal foundations. Not a complete star-system formation pipeline. |
| Time definitions | `Physics/Astronomy/WorldTimeDefinition.cs`, `Physics/Universe/LogicalTimeLedger.cs` | Existing world-time and logical-time primitives. Geological age needs to remain distinct from elapsed simulation time. |
| Deterministic universe addressing and initial conditions | `Physics/Universe/UniverseGrid.cs`, `DeterministicCellGenerator.cs` | Implemented deterministic grid addressing and seeded cell initial-condition generation. Not a planetary plate or geology generator. |
| Replay and checkpoints | `Physics/Universe/TimelineEvent.cs`, `PhysicsCheckpoint.cs`, `TimelineHistorySegment.cs`, `TimelineReplayPlan.cs`, `TimelineReplaySegment.cs`, `TimelineReplayExecutor.cs`, `TimelineReplayRouteExecutor.cs`, `TimelineBranch.cs`, `TimelineLineage.cs` | Generic immutable event/checkpoint/replay foundations. Event payload interpretation and model-specific execution remain caller-owned. Geological event records are not yet wired into this replay pipeline. |
| 2D exterior terrain | `Physics/Geology/TerrainHeightField2D.cs`, `TerrainAuthoringOperation2D.cs` | Implemented immutable cell-centred height field; plane, ramp, bowl, bilinear sample, finite-difference normal, identified smooth radial brush, dirty sample bounds. 2.5D only. |
| Prescribed volcanic deposition | `Physics/Geology/VolcanicTerrain2D.cs`, `VolcanicTerrain2DExport.cs` | Implemented bounded, volume-accounted prescribed deposition. Not eruption dynamics, magma transport, thermal evolution, or a general volcanism model. |
| Prescribed fault kinematics | `Physics/Geology/FaultKinematics3d.cs`, `FiniteFaultSegment3d.cs` | Implemented displacement geometry for declared fault motion and finite footprint. Not plate dynamics, stress prediction, friction, rupture nucleation, or earthquake simulation. |
| 3D solid/material storage | `Spatial/SparseVoxelVolume3d.cs`, `SparseMaterialVoxelVolume3d.cs`, `SparseMaterialVoxelVolumeSnapshot3d.cs`, `SparseMaterialVoxelVolumeEnvelope3d.cs` | Implemented sparse voxel and material snapshots with spatial/provenance envelope. Generic solid representation; material IDs are not rock properties and voxel samples do not yet retain stratigraphic layer/event identities. |
| Analytic and height-field-derived solids | `Spatial/AnalyticalSolidVolume3d.cs`, `Physics/Geology/MountainSolidVolume3d.cs` | Implemented bounded analytic ellipsoid and a deterministic height-field-bounded mountain extrusion. Neither is a geological formation solver. |
| Derived voxel surface | `Spatial/VoxelSurfaceMesh3d.cs` | Existing geometry extraction foundation. A derived mesh is presentation/geometry output, not authoritative physical state. |
| Stratigraphic column representation | `Physics/Geology/StratigraphicColumn3d.cs` | Implemented immutable local vertical columns with material IDs, elevation bounds, formation-age intervals, and deposition/formation event provenance. A column alone does not imply lateral continuity. |
| Sampled 3D stratigraphic grid | `Physics/Geology/StratigraphicColumnGrid3d.cs` | Implemented immutable regular XY sampling of vertical columns with shared spatial conventions, row-major ordering, and strict sample-position validation. This is a sampled dataset, not a continuous solid or interpolated layer-surface model. |
| Fault/stratigraphy cross-cutting query | `Physics/Geology/FaultStratigraphicIntersection3d.cs` | Implemented geometric intersection of a finite prescribed fault plane with a sampled vertical column; identifies a strictly intersected layer and retains its material and formation-event identity. Does not offset strata or model deformation. |
| Geological time and event metadata | `Physics/Geology/GeologicalEventRecord.cs` | Implemented validated separation of geological age from elapsed simulation seconds and immutable event provenance metadata. Not yet an atomic world-state transaction or connected geological replay executor. |
| Atmosphere, climate, hydrology, and terrain processes | `Physics/Hydrodynamics`, `Physics/Thermodynamics`, `Physics/Energy`; books and domain documents under `docs/books` and `docs` | Reusable neighboring physics foundations and documented domain contracts exist. A coupled planetary climate/weathering/erosion/sediment system is not implemented by those documents alone. |

## What the current foundations can already do

1. Define and evolve orbital bodies with the existing Newtonian models.
2. Generate deterministic universe-cell initial conditions.
3. Author and query a bounded 2D exterior surface with explicit coordinate frame, datum, units, spacing, and origin.
4. Apply deterministic localized elevation edits without mutating the source snapshot.
5. Generate prescribed volcanic deposits and bounded 3D solid geometry.
6. Represent a prescribed finite fault displacement.
7. Retain generic checkpoints/events/replay primitives and geological event/time metadata.
8. Represent local stratigraphic columns with material identity, elevation bounds, formation intervals, and event provenance.
9. Arrange those columns on a regular, immutable XY sample grid and query them by integer grid coordinate.

## Explicit gaps before a complete procedural planet

- Planetary body definition that joins radius, mass, composition, rotation, internal energy/temperature, and orbital/radiative boundary conditions into a versioned initial-state record.
- A validated planetary interior model: core/mantle/crust structure, heat sources and transport, material/rheology assumptions, and mantle dynamics or explicitly reduced approximations.
- Lithosphere/plate state: plate geometry, thickness, density, motion, boundary conditions, and convergent/divergent/transform interaction. The current fault primitive is prescribed kinematics, not this solver.
- A material-property catalogue, lateral stratigraphic correlation, process-derived layer creation, fault displacement of strata, and volumetric fault/intrusion cross-cutting relationships. The column grid provides regular samples only; it does not interpolate between columns, infer continuity, or deform strata.
- Physical weathering, erosion, sediment transport, and deposition with water and solid-material budgets.
- Atmospheric weather and long-term climate models with declared coupling to surface processes.
- Hydrologic routing and groundwater/karst processes coupled to terrain and material changes.
- A geological state transaction that verifies the source revision, produces a candidate revision, validates it, and commits atomically; failed candidates must leave source state unchanged.
- A geological replay adapter that maps validated event records to deterministic model execution and checkpoints.
- 3D cave/void generation from declared geological processes. Generic voxel storage can represent void/solid geometry, but it does not create caves geologically.
- GPU acceleration only after a deterministic CPU reference, parity tolerances, and material/water accounting are established.

## Implementation order

1. Finish the source inventory and canonical terrain query/authoring contract without duplicating existing APIs.
2. Add an immutable geological revision/transaction boundary over existing terrain snapshots; test stale source revisions, validation rejection, and failure atomicity. (Initial in-memory 2D boundary now implemented in `GeologicalTerrainRevision2D.cs`; durable persistence and geological replay remain future work.)
3. Connect prescribed fault events to an explicit terrain/stratigraphy representation while preserving layer provenance and clearly stating the kinematic limits. (The first finite-fault/column intersection query now identifies a cut layer; applying deformation and propagating it through a volume remain open.)
4. Add a minimal layered-material column/volume contract before claiming strata or fault cross-cutting. (Immutable local columns and the regular sampled grid now exist in `StratigraphicColumn3d.cs` and `StratigraphicColumnGrid3d.cs`; continuous layer geometry, lateral correlation, and fault offsets remain open.)
5. Build process-specific plate/interior, climate, hydrology, and sediment models as separate validated increments.
6. Connect geological events to the existing timeline/checkpoint/replay infrastructure through a model-specific adapter.
7. Only then compose a planetary history generator and accelerate measured bottlenecks on GPU.

## Evidence and limitations

The existing terrain authoring API is documented in `docs/TERRAIN_HEIGHT_FIELD_2D.md` and tested by `tests/TheSingularityWorkshop.Physics.Tests/TerrainHeightField2DTests.cs`. The 3D material-volume contract is documented in `docs/SPARSE_MATERIAL_VOXEL_VOLUME_3D.md`. The sampled column-grid contract is documented in `docs/STRATIGRAPHIC_COLUMN_GRIDS_3D.md`. Mountain extrusion is documented by its source and tests. Geological fault, event, revision-transaction, and stratigraphic-column primitives have focused tests.

This inventory is deliberately conservative: a documented future capability is not an implemented solver, a geometric operation is not a physical process, and a renderable surface is not authoritative geological state.
