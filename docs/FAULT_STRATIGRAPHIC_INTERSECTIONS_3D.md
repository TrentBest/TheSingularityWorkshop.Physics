# Fault/stratigraphy cross-cutting intersection (3D)

## Status

**Implemented:** a deterministic geometric query that tests whether a finite prescribed fault plane intersects the interior of one layer in a sampled vertical stratigraphic column.

**Not implemented:** layer displacement, fault throw/heave calculation, deformation of neighboring columns, folding, fault-tip deformation, rupture, stress transfer, or a volumetric faulted-strata solver.

## Contract

`FaultStratigraphicIntersection3d.TryIntersectColumn`:

- Requires exact coordinate-frame and linear-unit identity between the finite fault and sampled column. The column's increasing elevation must align with positive Z in the fault frame; arbitrary vertical-axis orientations are not supported by this first contract.
- Solves the plane equation along the column's vertical line when the plane has a non-negligible vertical normal component.
- Requires the resulting point to lie within the finite segment's strike/down-dip footprint.
- Reports a layer only when the point lies strictly inside its vertical interval. A point on a layer boundary is an interface intersection, not arbitrarily assigned to either layer. A point in a hiatus is not a layer intersection.
- Retains fault, column, layer, material-catalogue, and original formation-event identities.
- Does not mutate the fault, column, or layer and does not apply the fault's displacement.

A vertical plane parallel to or coincident with the sampled vertical column has no unique point intersection; this query returns false. Such geometries require a different intersection representation (for example, a fault trace/plane intersection against a 3D stratigraphic volume).

## Tests

`FaultStratigraphicIntersection3dTests` covers layer/provenance identity, boundary and hiatus handling, finite footprint limits, vertical-plane degeneracy, coordinate-frame/unit mismatch, and invalid arguments.
