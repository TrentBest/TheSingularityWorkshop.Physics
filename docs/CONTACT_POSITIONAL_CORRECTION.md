# Bounded Contact Positional Correction

## Purpose

`ContactPositionCorrector` is a small positional-stabilization operation for one already-detected, active contact. It translates body origins apart along the contact normal to reduce excessive penetration. It complements, but does not replace, the normal-impulse response.

This operation changes achieved positions only. It is not a force, an impulse, a time integrator, or a rigid-joint solver.

## Model

Given signed separation (s), allowed penetration slop (sigma), correction fraction (eta), and maximum correction (c_{max}):

[
p_{excess} = max(0, -s - sigma)
]

[
c = min(eta p_{excess}, c_{max})
]

where (0 < eta leq 1), (sigma geq 0), and (c_{max} > 0). The total requested separation change is capped by (c_{max}).

For masses (m_A,m_B > 0), the correction is distributed by inverse mass. With unit normal (n) directed from A to B:

[
Delta x_A = -n c \frac{1/m_A}{1/m_A+1/m_B}
]

[
Delta x_B = +n c \frac{1/m_B}{1/m_A+1/m_B}
]

Thus the lighter body moves farther; equal masses move equally in opposite directions. The sum of the two normal displacements is (c), subject to finite floating-point arithmetic.

## Contract and validation

- Requires a valid, active contact and matching body identities.
- Requires positive finite masses, finite non-negative slop, a finite correction fraction in ((0,1]), and a positive finite maximum correction.
- Touching contacts and positive gaps do not move bodies. Penetration within the configured slop does not move bodies.
- Preserves linear velocity, angular velocity, and orientation. It changes body positions only and preserves the contact's logical step/time.
- Returns new immutable body snapshots and a contact observation whose signed separation is adjusted by the applied correction. The point and relative velocity remain the original observation values; the observation is not a replacement for narrowphase recomputation.
- Detects non-finite requested corrections, inverse-mass sums, and resulting positions instead of returning corrupted state.

## Deliberate limits

This is a **single-contact, translational correction**, not a coupled constraint solve. It does not account for angular leverage, local contact anchors, friction, restitution, or interactions among multiple contacts. The caller should recompute narrowphase contacts after moving bodies. Applying it repeatedly without a coherent simulation-step policy is not a substitute for a solver.

Structural failure and constraint reaction-load semantics remain separate concerns. Positional correction magnitude is not a force, a sustained load, or a failure threshold.

## Validation

The NUnit tests cover capped penetration correction, equal-mass translation, inverse-mass weighting, no movement at touch or across a positive gap, preservation of velocities/orientations and logical time, and invalid masses/configuration/identity.

---

[Physics roadmap](ROADMAP.md) · [Contact observation](CONTACT_OBSERVATION.md) · [Sphere-sphere contact detection](SPHERE_SPHERE_CONTACT.md) · [Normal contact impulse response](CONTACT_IMPULSE_RESPONSE.md)
