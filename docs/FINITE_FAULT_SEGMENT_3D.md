# Finite fault segment 3D

## Implemented contract

`FiniteFaultSegment3d` bounds an existing `FaultKinematics3d` prescription to a finite rectangular footprint. The rectangle is centred on the kinematic plane origin, with its long axis along the declared strike direction and its short axis along the declared down-dip direction. Both half-extents are positive finite values in the fault's caller-declared linear unit.

For a query point, the segment projects the point relative to the plane origin onto the unit strike and down-dip basis. The point is inside the footprint when both absolute projected coordinates are less than or equal to their respective half-extents. Points on the boundary are included. A point receives the underlying kinematic displacement only when it is inside the footprint and on the declared hanging-wall side. The footwall and the mathematical fault plane receive zero displacement.

The segment retains the original `FaultKinematics3d` instance, so its stable fault ID, coordinate frame, unit, mechanism label, hanging-wall convention, and slip components remain authoritative.

## Interpretation boundary

This is a finite-footprint, prescribed block-offset model. It is not an elastic dislocation solution and does not compute stress, strain, rupture propagation, friction, earthquake magnitude, or distributed deformation. Its displacement is discontinuous at the rectangular footprint boundary by construction. Do not use it as a substitute for a finite-fault elastic displacement field or infer a physically realistic near-field displacement from it.

The footprint is rectangular in the fault's strike/down-dip coordinates. Irregular fault surfaces, curved segments, segment networks, junction mechanics, layered crust response, and stress accumulation/failure remain future models.

## Validation

Focused tests cover constructor validation, inclusive footprint edges, finite bounds along both basis directions, hanging-wall-only displacement, zero displacement on the footwall and plane, and preservation of the underlying fault identity and slip semantics.
