# Bounded normal contact impulse response

`ContactImpulseResolver` is the first contact-response slice after sphere-sphere geometric detection. Given a valid active `ContactObservation`, body snapshots, positive masses, and a restitution coefficient, it applies an instantaneous frictionless impulse along the observed normal and returns new immutable body snapshots plus an observation carrying the computed impulse magnitude.

## Rule and conventions

Let **n** be the unit normal from A to B, **vᵣ = vB − vA**, and **u = vᵣ · n**. For a penetrating or touching contact with closing normal speed (**u < 0**), the impulse magnitude is

`j = −(1 + e)u / (1/mA + 1/mB)`

where masses are in kilograms, velocity in metres per second, restitution `e` is dimensionless in [0, 1], and impulse is in newton-seconds. The velocity changes are `vA' = vA − n j/mA` and `vB' = vB + n j/mB`. The impulse is equal and opposite, and the operation does not advance physical time.

A separating pair receives no pulling impulse. A positive separation reported only because of detector tolerance also receives no impulse. Restitution zero removes closing relative normal speed in the ideal two-body case; restitution one reverses it.

## Deliberate limits

This is not a general collision solver. It assumes the supplied contact normal is suitable for response and treats bodies as translational point masses. It does not account for angular inertia or contact-point torque, friction/tangential impulse, positional penetration correction, contact persistence, multi-contact coupling, joints, or solver iteration. It must not be interpreted as a measured sustained normal force or as proof of support. The returned body snapshots preserve pose and angular velocity and carry the contact step/time; callers remain responsible for ordering and committing results.

See [sphere-sphere contact detection](SPHERE_SPHERE_CONTACT.md), the [contact observation contract](CONTACT_OBSERVATION.md), and the [articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md).
