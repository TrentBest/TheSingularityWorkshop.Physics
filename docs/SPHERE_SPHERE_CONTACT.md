# Sphere–sphere narrowphase contact detection

`SphereSphereContactDetector` is the first contact-producing geometric model in Physics. It tests two spheres whose centres are their rigid-body world origins and emits the existing `ContactObservation` contract when the surfaces touch, overlap, or lie within an explicit tolerance.

## Model and conventions

- Position, radius, signed separation, and contact tolerance are in metres.
- Normal points from body A's centre toward body B's centre.
- Relative velocity is body B minus body A in world-frame metres per second.
- Negative signed separation means penetration; zero means touching; positive separation is emitted only when it is no greater than the supplied tolerance.
- The contact point is the midpoint between the two sphere surface points along the centre line.
- Normal force and normal impulse remain `null`: this detector computes geometry, not response or load.

## Explicit limits

This is a narrowphase test for spheres only. It does not consume broadphase candidate pairs, handle arbitrary collider shapes, resolve penetration, compute friction, apply impulses, infer support, or mutate body state. Coincident centres return no observation because geometry alone provides no unique normal direction. The caller owns stable contact-ID policy, simulation-step ordering, and contact persistence across steps.

See [Contact observation](CONTACT_OBSERVATION.md) and the [articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md).
