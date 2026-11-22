# SURPS Physics Rulebook Integration

**Status:** integration proposal for SURPS issue [#2](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRolePlayingSystem/issues/2). The SURPS `ISurpsRulebook` name and responsibilities are documented, but its CLR signature and package contract are not yet committed. This document does not define a competing Physics-owned version of that interface.

## Ownership boundary

- **Physics** owns physical laws, state, numerical models, simulation behavior, assumptions, validation, and scientific evidence.
- **SURPS** owns the shared rulebook contract, ontology-facing presentation, rulebook composition, and compatibility conventions.
- **MicroBundleDomain / FSM_COS** own bundle composition and lifecycle.
- **The host** resolves loaded typed provider instances and orchestrates calls.
- **GUI / GUI.SVG / Pages** present theory and observations; they do not calculate physical outcomes.

A SURPS rulebook is descriptive and discoverable. It identifies the applicable Physics capability; it does not replace or duplicate that capability.

## First supported Physics rulebook slice

The Physics MicroBundles adapter assembly currently exposes these tested provider contracts:

| Bundle ID | Provider ID | Rulebook subject | Executable behavior |
|---:|---|---|---|
| 5201 | `physics.contact.sphere-sphere.v1` | Mechanics / geometric contact | Detects one sphere–sphere geometric contact observation from supplied body snapshots and radii |
| 5202 | `physics.contact.normal-impulse.v1` | Mechanics / collision response | Resolves one supplied active contact using a frictionless translational normal impulse |
| 5203 | `physics.kinematics.prescribed-motion.v1` | Mechanics / prescribed motion | Evaluates constant-velocity or quintic smooth-step motion and validates a separately supplied achieved state |

These are separate capability identities because their inputs, outputs, assumptions, and evidence differ. They are not a general rigid-body engine:

- 5201 does not generate broadphase candidates or resolve an impact.
- 5202 does not provide friction, angular impulse, persistent contacts, multiple-contact solving, or force integration.
- 5203 is prescribed kinematics, not force-driven dynamics or collision handling.
- None owns a world-step loop or logical clock.

The first SURPS-facing Physics book should expose these three existing capabilities and their linked theory/evidence. The Physics domain atlas contains many additional mapped domains; their appearance in a rulebook index must retain their real maturity state and must not imply implementation.

## Rulebook metadata expected by the integration

The proposed minimum metadata is:

- Stable rulebook identity, display name, semantic version, and Physics domain placement.
- Scope, assumptions, supported regimes, and explicit exclusions.
- Ontology concepts and terminology.
- Structured, inspectable rules and constraints, with units and applicable conditions.
- Required/provided capability references, including bundle ID, provider ID, and compatible version.
- Dependency and compatibility declarations.
- Theory chapters, derivations, experiments, living visuals, and source references.
- Evidence status, source/test paths, validation methods, and provenance.

The common `ISurpsRulebook` should expose descriptive metadata and capability references. Executable calculations remain on typed Physics provider contracts such as `ISphereSphereContactProvider`, `INormalImpulseResponseProvider`, and `IPrescribedKinematicMotionProvider`.

## Example interaction: a physical collision

1. A SURPS world identifies two entities and the physical interaction context.
2. The SURPS Physics rulebook identifies the relevant contact-observation and response capabilities, including their limitations.
3. The composition host resolves the loaded Physics provider instances.
4. The host supplies authoritative body snapshots to the sphere contact provider.
5. If a valid active contact is observed, the host may supply it to the normal-impulse provider.
6. Physics returns the resulting contact evidence and achieved body snapshots. The host owns world-step ordering and committing the resulting state.

The rulebook does not invent contact geometry, choose unvalidated material properties, or execute the collision itself.

## Contract alignment gate

Before implementing the SURPS wrapper against a CLR interface, SURPS must confirm:

1. The final `ISurpsRulebook` interface and metadata types.
2. How a SURPS-enabled container declares dependencies on domain MicroBundles.
3. Rulebook and bundle identity/version allocation.
4. How rulebook metadata is validated and how provenance references are represented.

The Physics integration response and current adapter inventory are recorded in [SURPS issue #2](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRolePlayingSystem/issues/2).

## Implementation sequence

1. Align the CLR contract with SURPS without introducing a duplicate interface in Physics.
2. Implement `SurpsRulebookPhysics` as the SURPS-enabled container over the existing Physics provider MicroBundles.
3. Add tests for bundle identity, declared dependencies, rulebook metadata, provider compatibility, and coexistence with other SURPS rulebooks.
4. Link each rulebook entry to Physics source, tests, theory, and experiments.
5. Expand the catalog only as additional Physics capabilities satisfy their own implementation and evidence gates.

No NuGet publication, release, or promotion to `master` is implied by this integration work.
