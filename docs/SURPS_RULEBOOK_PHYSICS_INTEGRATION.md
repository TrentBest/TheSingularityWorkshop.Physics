# SURPS Physics Rulebook Integration

**Status:** first executable SURPS-facing Physics container implemented on `development`. The Physics adapter assembly carries a source copy of SURPS's minimal `ISurpsRulebook : IMicroBundle` marker so it can integrate without depending on the DungeonsAndUnderground rulebook assembly. Keep the copied contract aligned with SURPS as the shared contract evolves.

## Ownership boundary

- **Physics** owns physical laws, state, numerical models, simulation behavior, assumptions, validation, and scientific evidence.
- **SURPS** owns the rulebook contract, ontology-facing presentation, rulebook composition, and compatibility conventions.
- **MicroBundleDomain / FSM_COS** own bundle composition and lifecycle.
- **The host** resolves loaded typed provider instances and orchestrates calls.
- **GUI / GUI.SVG / Pages** present theory and observations; they do not calculate physical outcomes.

A SURPS rulebook is descriptive and discoverable. It identifies applicable Physics capabilities; it does not replace or duplicate them.

## First implemented container

`SurpsRulebookPhysics` is the initial SURPS-facing Physics container. It declares dependencies on three already implemented Physics MicroBundles:

| Bundle ID | Provider ID | Subject | Implemented behavior |
|---:|---|---|---|
| 5201 | `physics.contact.sphere-sphere.v1` | Mechanics / geometric contact | Detects one sphere–sphere geometric contact observation from supplied body snapshots and radii |
| 5202 | `physics.contact.normal-impulse.v1` | Mechanics / collision response | Resolves one supplied active contact using a frictionless translational normal impulse |
| 5203 | `physics.kinematics.prescribed-motion.v1` | Mechanics / prescribed motion | Evaluates constant-velocity or quintic smooth-step motion and validates a separately supplied achieved state |

These are distinct capabilities because their inputs, outputs, assumptions, and evidence differ. The wrapper is composition metadata only; it does not run a simulation or implement physical behavior.

### Explicit limits

- 5201 does not generate broadphase candidates or resolve an impact.
- 5202 does not provide friction, angular impulse, persistent contacts, multiple-contact solving, or force integration.
- 5203 is prescribed kinematics, not force-driven dynamics or collision handling.
- None owns a world-step loop or logical clock.

## Physics sourcebook family

The descriptor-backed sourcebook catalogue now begins at [`docs/books/index.md`](books/index.md). Its first book is [Power and Industrial Systems — LOD 1](books/power-and-industrial-systems.md), with an original cover and a descriptor using the shared SURPS LOD badge. The book maps batteries, grids, thermal and nuclear educational plants, renewables, hydroelectric generation, and manufacturing while explicitly separating existing Physics capabilities from planned models. Candidate specialist books remain planned until their own scopes, LODs, covers, descriptors, chapters, and evidence are ready.

## Physics rulebook family

Physics is much larger than this first capability slice. The SURPS-facing catalog should grow as a family of focused rulebooks, each linked to its authoritative Physics models, theory, experiments, tests, and evidence. Initial subject divisions to develop include:

| Rulebook area | Intended scope | Current status |
|---|---|---|
| Foundations and measurement | Dimensions, units, frames, uncertainty, measurement and reference conventions | Catalog/planning |
| Classical mechanics | Kinematics, dynamics, forces, momentum, energy, work and power | Partial implemented capabilities |
| Contact and collision | Contact geometry, impact, restitution, friction, constraints and contact persistence | Partial; 5201–5202 |
| Gravitation and orbital mechanics | Gravitational fields, N-body motion, orbits, encounters and tides | Existing domain models; SURPS wrapper pending |
| Fluids and hydrodynamics | Pressure, flow, buoyancy, waves, rainfall and transport | Existing domain models; SURPS wrapper pending |
| Thermodynamics and heat transfer | Temperature, energy, conduction, thermal transport and coupled systems | Existing domain models; SURPS wrapper pending |
| Acoustics and waves | Sound propagation, reflections, resonance and wave behavior | Existing domain models; SURPS wrapper pending |
| Electricity and magnetism | Charge, fields, circuits, grids and electromagnetic behavior | Existing domain models; SURPS wrapper pending |
| Optics and radiation | Light, propagation, reflection, refraction and radiation | Catalog/planning |
| Geology and terrain | Materials, terrain fields, geological processes and environmental coupling | Partial domain models; SURPS wrapper pending |
| Materials and constitutive behavior | Material properties, stress, strain, deformation and failure | Catalog/planning |
| Chemistry and physical properties | Atomic/molecular properties, states, reactions and thermochemical links | Existing domain models; SURPS wrapper pending |

The status column is intentionally conservative: an existing model is not automatically a complete rulebook, and a planned subject is not represented as implemented physics. Rulebook boundaries can be refined with SURPS as the ontology and LOD conventions mature.

## Ownership and interaction example

1. A SURPS world identifies two entities and the physical interaction context.
2. The Physics rulebook identifies applicable contact-observation and response capabilities, including limitations.
3. The composition host resolves the loaded Physics provider instances.
4. The host supplies authoritative body snapshots to the sphere contact provider.
5. If a valid active contact is observed, the host may supply it to the normal-impulse provider.
6. Physics returns contact evidence and achieved body snapshots. The host owns world-step ordering and committing resulting state.

The rulebook does not invent contact geometry, choose unvalidated material properties, or execute the collision itself.

## Contract alignment

The minimal marker copied into the Physics adapter assembly is intentionally small:

```csharp
public interface ISurpsRulebook : IMicroBundle
{
}
```

This avoids a compile-time dependency from the Physics package to the DungeonsAndUnderground rulebook package. It is a coordinated source-level copy of the working SURPS contract, not a proposal to fork or independently expand the interface. When SURPS establishes a shared contracts package or changes the canonical contract, update this copy and the Physics integration together.

Rulebook metadata, ontology contributions, structured rules, source-book references, provenance, and LOD resolution should be added in coordination with SURPS rather than invented as a Physics-only contract.

## Next implementation sequence

1. Add focused rulebook wrappers for existing Physics domains, starting with gravity/orbital mechanics, fluids, heat transfer, acoustics, and electricity.
2. Give each book a stable identity, explicit dependencies, scope, assumptions, exclusions, and links to tests/evidence.
3. Introduce richer shared metadata only when SURPS and multiple domain owners agree on the contract.
4. Keep the SURPS container opt-in at the experience level; Physics remains independently usable.

No NuGet publication, release, Pages deployment, or promotion to `master` is implied by this integration work.
