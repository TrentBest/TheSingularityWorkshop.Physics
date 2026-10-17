# Surface Combustion: Materials, Elements, and the Flame System

**Status:** implementation contract and staged work plan. This document does not claim a complete combustion solver exists.

**Owner:** `TheSingularityWorkshop.Physics` owns physical state, material-response models, reaction accounting, and validation. MicroBundle composition discovers and arbitrates providers. A presentation client may visualize recorded state but must not authoritatively ignite, consume fuel, or add heat.

Related: [Physics + Math Domain Expansion](PHYSICS_MATH_CHEMISTRY_COMBUSTION.md) · [Physics capability catalog](PHYSICS_CAPABILITY_CATALOG.md) · [deterministic math and random](DETERMINISTIC_MATH_AND_RANDOM.md) · [LUT policy](PHYSICS_MATH_LUTS.md).

## 1. The central behavior

A lighter contributes a reusable flame source. A candle contributes material, geometry, and environmental state. The Physics model determines whether the flame transfers enough energy to ignite the candle's exposed wick/wax; the same model can act on paper, wood, or another material when its data and reaction capabilities support that case.

There must be no candle-specific imperative such as `LightCandle()` that bypasses the physical model. Interactions are requests to the model; ignition, fuel consumption, heat transfer, and emitted products are consequences of a supported calculation. If the required material data or reaction model is unavailable, return an explicit unsupported/insufficient-evidence result instead of inventing a plausible outcome.

This is a physical model with visible output derived from it, not a flame shader that happens to look convincing.

## 2. Element and material data are first-class inputs

Element identities and material properties are related but not interchangeable:

- An **element record** identifies the chemical element by atomic number and symbol and carries name, source, source version, and licensing/provenance metadata.
- An **isotope record** identifies a specific nuclide and its mass data; isotope mass is not the same quantity as an element's standard atomic weight.
- A **material record** describes a material or mixture. It can refer to elemental/chemical constituents, but bulk properties such as density, heat capacity, thermal conductivity, emissivity, moisture, and ignition/reaction behavior belong to the material/model evidence—not to a generic element identity.
- A **reaction profile** declares the participating species, balanced stoichiometry, reaction conditions/range, heat release, source, version, uncertainty, and applicability limits.
- A **physical model** consumes these declared records. It does not silently infer missing properties from names such as `wood`, `wax`, or `steel`.

Use stable identifiers and retain source provenance down to the property or reaction value. Values may be measured, sourced, estimated, or illustrative; these evidence classes must remain visible. Illustrative values are useful for tests and teaching but must never be presented as measured physical truth.

The periodic table is a view over versioned element data, not a second hard-coded truth in WebPage. MicroBundles can distribute and compose datasets and specialized providers; the Physics NuGet contracts remain usable without adopting the MicroBundle host.

## 3. Surface fields and texture representation

The initial GPU-oriented design uses three logical scalar fields sampled on a surface grid:

1. **Fuel field:** remaining fuel mass per cell (kg), with a declared surface-cell area and material/profile identity.
2. **Oxidizer field:** available oxygen mass per cell (kg) in a declared near-surface control volume. A future volume/transport model must define the control-volume depth and exchanges with ambient air; a texture channel is not itself a physical unit.
3. **Thermal field:** thermal energy per cell (J) relative to a declared reference/state model. Derive temperature only when mass, heat capacity, phase assumptions, and state are available.

A GPU texture is a storage/transport encoding of these logical fields. It is not the domain contract. Precision, quantization, channel packing, grid dimensions, coordinate mapping, and upload/readback policy must be declared independently so CPU reference calculations can validate GPU results.

### Normals, gravity, and “up”

Each surface cell carries a normal in a declared coordinate frame. Normals describe local surface orientation and help determine exposure, directional flux, and how flame-source energy reaches a surface. Validate finite components and a documented normalization tolerance.

World gravity defines the up direction. Buoyant hot-gas transport tends upward relative to gravity; conduction follows temperature gradients and is not globally “upward.” Radiation depends on geometry and view factors. Wind/advection may transport heat and oxygen sideways or downward. Keep these mechanisms distinct, even if an early demonstrator deliberately supports only one.

## 4. Reaction and energy accounting

For each reaction step, calculate the reaction extent from the declared rate law and available reactants, then cap it by every limiting reactant. Consume fuel and oxygen according to balanced stoichiometry; release heat from the declared reaction enthalpy/profile; account for products and losses in explicit terms.

The minimum accepted invariants are:

- no negative fuel, oxygen, or other species inventory;
- reaction extent cannot exceed available reactants;
- reactant consumption and product generation satisfy the selected reaction's stoichiometry;
- energy additions and removals are recorded, and no heat appears merely because a flame is rendered;
- timestep, cell area/volume, transport method, boundary conditions, and stability limits are explicit;
- results are deterministic for identical inputs, model versions, and random keys under the documented execution contract.

The first implementation should begin with a single-cell accounting reference model and tests. Neighbor transport, spatial flame feedback, smoke/soot, expansion, buoyancy, and GPU execution are separate increments. Do not claim full combustion because a surface field can be updated.

## 5. Three-texture concept and the fire triangle

The three fields are the user's proposed compact working representation of the fire triangle:

- Fuel availability constrains reaction.
- Oxygen availability constrains reaction.
- Heat/temperature determines whether ignition and reaction-rate conditions are met.

They are not three independent artistic controls. Their coupling is governed by the reaction profile and physical model. Additional fields (moisture, gas products, soot, smoke, or phase) must be introduced when omitting them would break the model's intended accounting or validity.

A source flame is a boundary/source condition with its own temperature/energy, geometry, duration, and supported transfer model. A flame particle system is generated from solved hot/reaction regions as a presentation effect. Particle variation may be seeded by Squirrel3, but random variation may perturb presentation or declared stochastic submodels only; it must not override stoichiometry, create fuel, or conceal non-conservation.

## 6. Squirrel3 and non-repeating but replayable variation

Use existing `Squirrel3` / `Squirrel3Keyed` primitives rather than a new unrelated random implementation. Prefer keyed samples using stable surface-cell identity, logical tick, and a named purpose ID. This prevents a new particle or unrelated cell from reshuffling every other flame's result.

Record algorithm identity/version, seed, key schema, and logical tick when randomness affects authoritative simulation. Use randomness for explicitly modeled variability (e.g. bounded local transport/combustion fluctuations once their distribution is justified) and visual sampling. It is not a substitute for a rate law or physical uncertainty model.

## 7. Staged acceptance plan

1. **Data contracts:** validate element identity and provenance; preserve material-property evidence and validity ranges; define surface-cell quantities, units, normal, and source identity.
2. **Single-cell reaction reference:** use a named, balanced reaction profile and sourced or explicitly illustrative values. Test limiting reagents, non-negativity, mass bookkeeping, heat release, invalid inputs, and determinism.
3. **Surface ignition:** model source-to-surface heat transfer, ignition conditions, and material response. Test ignition/no-ignition boundaries and ensure the source does not directly set target state.
4. **Neighbor transport:** add heat and oxidizer transport with grid spacing, timestep, boundary conditions, stability checks, and conservation/positivity tests.
5. **Flame and smoke outputs:** derive emission/particles from reaction and thermal state. Presentation output cannot feed back as untracked energy.
6. **GPU parity:** only after a deterministic CPU reference and tolerances exist, add packed texture layouts and compare GPU outputs against the reference. GPU work belongs behind renderer-independent Physics contracts.

Every stage updates the capability catalog only when source and tests support the new status. This plan does not imply the later stages already exist.
