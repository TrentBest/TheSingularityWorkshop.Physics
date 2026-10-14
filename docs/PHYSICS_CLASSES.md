![Ontology-aligned physics capability map](images/physics-ontology-map.svg)

# Physics Classes & Ontology Map

Physics is organized into interoperable capability domains. These are not a universal fixed-layer ontology: they are addressable concepts that can coexist, be extended, and be composed into MicroBundles.

## Foundational classes

| Class / domain | What it owns | Common dependencies |
|---|---|---|
| **Quantities & Units** | Dimensions, units, conversions, tolerances | None |
| **Time & Integration** | Logical time, fixed steps, solver policy, checkpoints | Quantities & Units |
| **Spatial Substrate** | Coordinate conventions, dimensions, frames, regions, boundaries | Quantities & Units |
| **State & Conservation** | State snapshots, invariants, mass/energy/momentum accounting | Quantities & Units |
| **Materials & Composition** | Elements, compounds, mixtures, alloys, phase and property data | Quantities & Units |
| **Measurements & Provenance** | Observations, uncertainty, source references, model lineage | Quantities & Units |

## Physical process classes

| Class / domain | Representative phenomena | Typical composition |
|---|---|---|
| **Mechanics** | Motion, forces, momentum, rigid bodies, constraints, collisions | Time, spatial substrate, state |
| **Gravity** | Fields, weight, orbital interaction, potential energy | Mechanics, spatial substrate |
| **Thermodynamics** | Internal energy, heat capacity, entropy models, equilibrium assumptions | Materials, energy accounting |
| **Heat Transfer** | Conduction, convection, radiation, thermal contact | Thermodynamics, spatial substrate |
| **Hydrodynamics** | Pressure, flow, channels, free surfaces, dams, spillways, erosion | Materials, mechanics, boundaries |
| **Continuum Material Response** | Stress, strain, elasticity, plasticity, deformation | Materials, mechanics |
| **Fracture & Deconstruction** | Failure criteria, cracks, fragments, topology change, energy transfer | Material response, mechanics |
| **Electromagnetism** | Charge, current, fields, circuits, electromagnetic energy | Materials, state, spatial substrate |
| **Waves & Acoustics** | Oscillation, propagation, resonance, sound | Mechanics or field model, spatial substrate |
| **Phase Transitions** | Melting, freezing, boiling, condensation, phase-dependent properties | Materials, thermodynamics |
| **Chemistry & Reactions** | Reaction rates, species, stoichiometry, reaction energy | Materials, thermodynamics |
| **Combustion** | Fuel/oxidizer reaction, ignition, extinction, heat release | Reactions, heat transfer, materials |
| **Ionization & Plasma** | Charged species and plasma approximations within stated ranges | Electromagnetism, thermodynamics, reaction models |
| **Transport & Mixing** | Diffusion, advection, species/heat transport | Spatial substrate, process-specific laws |
| **Geophysics** | Geological materials, pressure, heat, fluids, stress over scale | Materials and relevant process models |
| **Optical Response** | Emission, absorption, reflection, transmission | Material properties and energy input |

## Composing capabilities

A capability states its identity, model version, supported domain/range, required quantities, assumptions, boundary conditions, outputs, measurements, and validation evidence. It requests only what it needs.

Illustrative composition, not an implemented bundle:

**Experiment → Units + Time + Mechanics + Material data + Measurements**

A coupled fire experiment might request:

**Experiment → Units + Time + Materials + Heat transfer + Reactions + Combustion + Gas transport + Measurements**

More components do not automatically mean more accuracy. Coupling requires compatible units, state definitions, timestep constraints, and validation.

## Material classes are not process classes

An element is not an alloy; a material record is not a solver. Iron, copper, bronze, and steel need distinct identity and property records with provenance and temperature/phase ranges. Their behavior emerges from the process models that consume those records.

Do not encode “copper sword” as a one-off animation or infer every property from a material name. Where composition or heat treatment changes behavior, represent the missing information or disclose a simplified model.

## Ontology identity

The Workshop ontology supports addressable definitions, relationships, and indexing. Physics domains should map into that system through explicit identities and versioned definitions; they must not invent a competing fixed nine-layer hierarchy. Numeric/index forms may be optimized later while semantic identity and meaning remain inspectable.

## First vertical slices

1. **Mechanics:** fixed-step motion/ramp experiment with analytical checks.
2. **Materials:** property records for iron, copper, bronze, and steel with source lineage.
3. **Thermal:** heating/cooling and one declared phase-transition model.
4. **Hydrodynamics:** a deliberately bounded water-flow scenario.
5. **Coupled phenomena:** volcano lifecycle, combustion, and Ring of Fire demonstrations as their models become supportable.
