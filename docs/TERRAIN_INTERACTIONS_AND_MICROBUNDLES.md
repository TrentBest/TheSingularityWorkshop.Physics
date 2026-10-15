# Terrain interactions, impact effects, and composable physics

## Purpose: teach the cause, not just show the effect

A physical result should help the user understand *why* it happened. When a raindrop lands, a boot strikes the ground, or a projectile hits a wall, the engine should not need a separate hand-authored visual effect for every possible surface. The physical model should expose the relevant event and state changes; an observing renderer or effects system may then present those consequences.

This is a design and implementation guide, not a claim that every interaction below is implemented today. The currently validated terrain-aware shallow-water model and its limits are documented in [Hydrodynamics: shallow-water basin and bathymetry](HYDRODYNAMICS_SHALLOW_WATER_2D.md).

## 1. One event, several possible outcomes

An impact does not have one universal result. Outcomes depend on the incoming state and the contacted material and environment. Useful inputs include:

- **Impact state:** mass, velocity, direction, contact area, and available kinetic energy.
- **Surface geometry:** local orientation, curvature, roughness, and nearby height gradients.
- **Material response:** stiffness, cohesion, porosity, permeability, grain size, moisture, and fracture behavior.
- **Environmental state:** existing water, loose dust or sediment, temperature, and surrounding air.
- **Scale and resolution:** whether the model resolves individual particles or represents their aggregate effect in a cell or field.

Surface normals are useful geometric evidence, but they are not a complete physical model. A normal can help estimate the normal and tangential components of an impact; it cannot alone determine whether material splashes, absorbs water, fractures, compacts, or emits dust.

## 2. Educational examples: a drop of water

The following cases describe desired model behavior and explain which variables matter. They are not claims that the current solver already implements every effect.

| Situation | Physical question | Model consequence to represent |
| --- | --- | --- |
| Drop on dry, porous soil | How much water enters the material? | A defined infiltration or storage transfer, bounded by available water and material capacity. |
| Drop on saturated soil | Is additional infiltration possible? | More water remains at the surface or runs off, according to the material model. |
| Drop on a steep, hard surface | What are the impact direction and available energy? | Surface-water input and any resolved splash response depend on impact and material rules; the normal is only one input. |
| Drop on loose dry dust | Can the impact mobilize grains? | A dust-release model may transfer material from a loose-surface reservoir into airborne particulate state, bounded by available material and energy. |
| Drop on an already wet surface | Does the impact disturb standing water? | A resolved splash or an aggregate momentum/energy source may be appropriate, depending on the chosen scale. |
| Runoff entering a depression | Is there an outlet at the current water level? | Water accumulates; once the free surface reaches a spill path, overflow can continue downhill. |
| Flow across erodible soil | Can the flow detach or transport grains? | A separately accounted erosion/sediment exchange changes bed material while tracking water and sediment budgets. |
| Flow over impermeable rock | Is infiltration or erosion allowed? | Material parameters constrain the response; water may collect, accelerate, or run off without invented soil loss. |

These cases teach a critical distinction: **bed elevation, free-surface elevation, material properties, and impact state are related but not interchangeable**. A flat free surface can be at rest over uneven terrain. Conversely, water can advance into a lower dry cell when the water state and gravity-driven pressure response permit it.

## 3. From a raindrop to bulk rainfall

The engine should not require a simulated object for every real-world drop to account for rainfall. A bulk representation can accumulate many small events into source terms over a simulation cell and time interval.

A future rainfall model must define, rather than hide, its budgets:

1. **Water volume:** rainfall rate multiplied by cell area and elapsed time adds a measurable amount of water.
2. **Momentum:** any injected momentum has a declared model and direction; it must not appear as an unexplained velocity change.
3. **Infiltration:** water transferred into soil leaves the surface-water budget and enters a separately tracked material-water store.
4. **Runoff:** surface flow redistributes water through the conservative flux model.
5. **Sediment:** erosion and deposition exchange material between the bed and transported sediment with their own accounting.
6. **Resolution and determinism:** aggregate updates should be reproducible and compared against small-scale reference experiments where such a comparison is meaningful.

This is how an efficient cell-based or future GPU implementation can represent the collective effect of many drops without changing the governing rules merely to achieve a visual result. Individual droplets remain useful when their trajectories or impacts are themselves important to the experiment.

## 4. Dust from a final battle: an emergent consequence

Imagine a hero and villain fighting on a dry, dusty arena. A boot scrapes the ground, a body lands, and a heavy object strikes a loose patch. The desired result is not a pre-authored “battle dust” animation triggered by the character's name or a scene script. It is a chain of physical events:

1. The mechanics/contact model reports an impact or tangential slip with its state and energy.
2. The terrain/material model determines whether the contacted material can be disturbed and how much loose material is available.
3. A particulate or airborne-material model accounts for the released dust and its momentum.
4. An air-flow model, when selected, transports and disperses that dust.
5. The renderer observes the resulting dust state and draws it at the available level of detail.

If the surface is wet, compacted, paved, or depleted of loose dust, the outcome should differ. If the impact is too weak, the model should not emit arbitrary dust. If the selected application does not include an airborne-material model, it should not pretend to simulate a dust cloud; it can still use the underlying contact and terrain results it does have.

The same principle generalizes to water spray, sparks, fractured debris, heat, smoke, and sound: effects are observations of selected physical models and their state changes, not a requirement to hand-script every visual combination.

## 5. Microbundles: compose the required physical capabilities

The Physics NuGet package is the reusable catalog of implementations. A runtime experiment, game, or world should compose only the distinct capabilities it requires.

A conceptual dependency chain for the battle example might be:

- **Contact/mechanics** — impact, contact force, slip, and energy transfer.
- **Terrain/material response** — surface type, loose-material inventory, moisture, and resistance to disturbance.
- **Particulate release** — dust or debris emitted from available material under a defined rule.
- **Air transport** — optional transport and dispersion of airborne material.
- **Rendering adapter** — observes the authoritative state and presents it; it does not own the physics.

A rain-and-river experiment might instead compose:

- **Terrain field** and **shallow-water hydrodynamics**.
- **Rainfall forcing**, when implemented, with explicit volume and momentum budgets.
- **Infiltration/material-water exchange**, when implemented.
- **Erosion/sediment transport**, when implemented and validated.
- **Observation or rendering adapters**, only for the outputs the host needs.

These are capability boundaries to guide composition, not assertions that each listed microbundle already exists as a separate package or implementation. Keep the core model independent of Unity and any renderer. FSM_API can coordinate lifecycle and state transitions; numerical models remain responsible for calculating physical change.

The package may contain a broad set of reusable code while the assembled runtime uses a small subset. That is the practical distinction between *shipping a toolbox* and *instantiating every tool*. Package size and runtime composition are different concerns; actual package size, dependency cost, memory use, and per-tick cost should be measured rather than used as slogans.

## 6. An edifying implementation standard

For each new capability, documentation should answer:

1. **What does it represent?** State, units, and physical scope.
2. **Why does the result occur?** Governing rule and assumptions.
3. **How is it computed?** Algorithm, inputs, outputs, and time-step behavior.
4. **What does it conserve or transfer?** Water, momentum, energy, mass, or material inventory.
5. **What tests support it?** Equilibrium, bounds, conservation, deterministic repeatability, and relevant reference cases.
6. **What does it not model?** Explicit limits, including effects that are only planned.
7. **How can a host compose it?** Required dependencies, optional dependencies, and observable outputs.

Where possible, explanations should link to tests and measured results. A green test suite proves the tested claims, not every real-world behavior suggested by an example.

## 7. Incremental validation path

1. Preserve the current shallow-water invariants: hydrostatic still-water balance, finite non-negative depths, wet/dry behavior, and closed-basin volume conservation.
2. Define and test rainfall volume input as an explicit source term before adding visual splash behavior.
3. Define material-water exchange and verify both sides of the water budget.
4. Add sediment and bed-change accounting as a separate coupled increment.
5. Define a small contact-to-loose-material experiment for dust release, with a material inventory and energy bounds.
6. Add air transport only when its state, units, and validation are ready.
7. Compare CPU and future GPU implementations against the same behavioral invariants and reference scenarios.

The destination is not a pile of convincing effects. It is a set of understandable, testable physical causes from which useful effects can emerge when an application composes the appropriate capabilities.
