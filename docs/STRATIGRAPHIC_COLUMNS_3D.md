# Stratigraphic columns (3D)

## Status

**Implemented:** immutable stratigraphic layer and vertical-column contracts with material identity, vertical bounds, formation-age intervals, and event provenance.

**Not implemented:** material-property catalogue, lateral layer correlation, depositional or erosional process solver, layer displacement, folding, intrusions, compaction, porosity, or a complete 3D stratigraphic volume. A local finite-fault-plane intersection query is implemented, but it does not deform strata.

## Contract

- `StratigraphicLayer3d` identifies a positive caller-defined material ID, finite top and bottom elevations, an older-to-younger formation-age interval in years before present, and the event ID that formed or deposited the layer.
- `StratigraphicColumn3d` identifies one horizontal sample location and explicit coordinate frame, vertical datum, linear unit, and positive vertical direction.
- Layers are supplied from highest to lowest elevation. They may touch but cannot overlap.
- An upper layer cannot be older than the layer beneath it. Age gaps are allowed and represent unresolved hiatuses.
- Layer identities are unique within a column. Input collections are defensively copied and exposed read-only.

## Deliberate limits

A material ID is a catalogue key, not density, porosity, strength, thermal properties, or composition. The age and event fields preserve provenance but do not prove the event's physical validity. The column is a local vertical stack sampled at (WorldX, WorldY); it does not imply lateral continuity or interpolate neighboring columns.

The finite-fault intersection query is documented in [Fault/stratigraphy cross-cutting intersection (3D)](FAULT_STRATIGRAPHIC_INTERSECTIONS_3D.md). It solves the plane/vertical-column intersection, checks the finite segment footprint, and identifies a layer only when the point lies strictly inside that layer. It retains the layer's material and formation-event identity. It does not apply the fault displacement or deform neighboring columns. A subsequent volume-level operation must preserve original layer identity/provenance while representing displaced geometry; it must not imply stress, rupture, or plate dynamics that are not implemented.

## Tests

`tests/TheSingularityWorkshop.Physics.Tests/Geology/StratigraphicColumn3dTests.cs` covers layer identity/material/provenance, finite positive thickness, geological age ordering, stack order, non-overlap, allowed hiatuses, duplicate IDs, and invalid coordinates. `tests/TheSingularityWorkshop.Physics.Tests/Geology/FaultStratigraphicIntersection3dTests.cs` covers finite-plane/column intersection, layer selection, provenance preservation, interfaces, hiatuses, segment bounds, vertical-plane degeneracy, and coordinate/unit validation.
