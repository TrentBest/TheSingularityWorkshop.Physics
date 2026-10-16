# Physics Domain Map

**Purpose:** establish the broad physical territory before implementation narrows into individual solvers. This is an architecture map and build order, not a claim that these capabilities already exist.

A domain becomes implemented only when it has an explicit state model, units and conventions, governing assumptions, validated numerical behavior, tests, and observable outputs. A name on this page is a planned domain unless its implementation is linked and tested.

## The physical picture

A world contains matter and fields. They carry state and energy; models describe how that state changes; solvers advance it through declared logical time; observations report what happened. Coupled domains exchange quantities through explicit interfaces rather than secretly reaching into each other's internals.

```mermaid
flowchart TD
    A[Material identity and state] --> B[Constitutive properties]
    A --> C[Thermodynamic state]
    D[Geometry and motion] --> E[Mechanics and constraints]
    F[Fluid state and transport] <--> E
    F <--> C
    C <--> G[Heat transfer]
    C <--> H[Phase changes]
    I[Species and reaction models] <--> C
    I <--> F
    I --> J[Combustion and reactive flow]
    G --> J
    K[Fields: gravity, electromagnetic] <--> E
    K <--> F
    B --> E
    B --> C
    L[Logical time, boundaries, scheduling] --> M[Domain solvers]
    M --> N[Measurements, conservation checks, provenance]
    N --> O[Experiments and living explanations]
```

The diagram shows conceptual coupling, not a solver dependency graph or a statement that every coupling is implemented. A domain may be independently useful before all its neighbors exist.

## Shared primitives every domain needs

These are the cross-cutting foundations to establish once, then reuse:

1. **Quantities and units.** SI by default; quantity meaning is explicit. Temperature uses kelvin for calculations, with display conversions at boundaries. Distinguish mass, force, energy, power, pressure, impulse, heat transfer, and material-specific properties.
2. **State and identity.** Immutable or transactionally committed snapshots; stable identities; explicit spatial frame, logical step, elapsed time, and model/schema version where applicable.
3. **Material data.** Separate material identity from measured properties and from the constitutive model that interprets them. Track source, version, license, valid ranges, uncertainty, and temperature/pressure dependence when known.
4. **Boundary and initial conditions.** Every experiment declares its environment, boundaries, initial state, constraints, and assumptions. Missing physics must not be silently fabricated.
5. **Model versus solver.** A governing law or constitutive relation is not a numerical integrator. Keep the physical model, discretization, stepping strategy, and convergence/error checks distinguishable.
6. **Conservation and accounting.** Track relevant mass, momentum, and energy entering, leaving, being stored, or transformed. Report residuals; do not promise exact conservation where a chosen numerical method only approximates it.
7. **Determinism and scheduling.** Explicit logical time, stable ordering, bounded work, and reproducible inputs. Coupled domains declare their exchange cadence and how delayed work is handled.
8. **Observation and evidence.** Publish measured state and derived values with units, model identity, and limitations. Rendering and controllers consume observations; they do not become the source of physical truth.

## Domain atlas

### A. Mechanics and motion
- **Covers:** kinematics, forces, rigid bodies, constraints, contacts, deformation, fracture, and multibody systems.
- **Key exchanges:** momentum and energy; contact forces; constraint reactions; deformation and failure state.
- **First usable slice:** analytical motion followed by bounded rigid-body and constraint scenarios.
- **Current status:** constant-acceleration kernel and broadphase primitives exist on `development`; spatial/contact mechanics work is also being developed in open, unmerged PRs.

### B. Thermodynamics and energy
- **Covers:** internal energy, temperature, heat capacity, equations of state where supported, thermal equilibrium, phase change, and energy accounting.
- **Key exchanges:** heat and work; temperature-dependent material properties; latent heat; reaction energy.
- **First usable slice:** validated material thermal properties plus a closed, lumped thermal body whose energy/temperature relation is explicit. Then heating/cooling and phase-change cases.
- **Guardrail:** temperature is not energy; heat is energy transfer, not a stored substance. Do not assume constant heat capacity or a single phase outside the model's declared range.
- **Current status:** a bounded lumped thermal-state primitive is implemented on `development` ([first thermodynamics primitive](THERMODYNAMICS_FIRST_PRIMITIVE.md)). It calculates sensible energy relative to a declared reference temperature and applies net energy transfers under constant specific heat; heat transfer, phase change, spatial gradients, and reaction energy remain unimplemented.

### C. Heat transfer
- **Covers:** conduction, convection, and radiation as distinct mechanisms with explicit assumptions.
- **Key exchanges:** thermal boundary fluxes and energy balance with solids, fluids, and reacting material.
- **First usable slice:** a one-dimensional conduction case with specified boundary conditions and a conservation/residual check, then bounded convection/radiation models.
- **Guardrail:** do not combine all transfer mechanisms into one unexplained temperature adjustment.

### D. Hydrodynamics and fluid mechanics
- **Covers:** fluid state, density, pressure, velocity, flow, viscosity, compressibility where supported, buoyancy, and fluid/solid interaction.
- **Key exchanges:** mass and momentum flux; pressure and shear forces; advection of thermal energy and species.
- **First usable slice:** a documented low-complexity incompressible flow case with explicit geometry, boundary conditions, stability limits, and convergence checks. Add buoyancy and fluid-solid coupling only as separate validated increments.
- **Guardrail:** distinguish a fluid parcel's state from a spatial fluid field. State whether a model is incompressible, compressible, inviscid, viscous, laminar, or turbulent; do not imply general fluid simulation from one special case.
- **Current status:** planned; no hydrodynamics solver is implemented on `development`.

### E. Materials and continuum response
- **Covers:** elements, compounds, mixtures, alloys, density, thermal properties, elasticity, plasticity, damage, fracture, and phase-dependent behavior.
- **Key exchanges:** stress/strain, thermal expansion, phase changes, reaction species, and failure thresholds.
- **First usable slice:** provenance-aware property records with units/ranges; then one bounded constitutive response with tests.
- **Guardrail:** a material label alone does not determine every property. Properties can depend on temperature, pressure, composition, direction, and phase.

### F. Species, chemistry, and combustion
- **Covers:** species identity, stoichiometry, reaction rates, reactant/product accounting, ignition, flame/combustion approximations, and extinction.
- **Key exchanges:** species and mass fractions, reaction enthalpy/energy, temperature, pressure, and fluid transport.
- **First usable slice:** a closed reaction ledger with balanced stoichiometry and explicit heat release; only then couple it to heat transfer and gas transport.
- **Guardrail:** combustion is not a visual effect and ignition is not merely a temperature threshold. Rates and limits must come from a declared model and supported data.
- **Current status:** planned; no chemical reaction or combustion model is implemented on `development`.

### G. Fields and long-range interaction
- **Covers:** gravity, electromagnetic fields, and later supported field-matter coupling.
- **Key exchanges:** forces, field energy, charge/current, and energy transfer.
- **First usable slice:** declare a field representation and a simple analytically checkable interaction before adding field solvers.
- **Guardrail:** keep field equations, source terms, boundary conditions, and approximations explicit.

### H. Waves, acoustics, and optics
- **Covers:** propagation, oscillation, sound, reflection/refraction, and later more advanced wave phenomena.
- **Key exchanges:** energy/momentum transport, material response, and field coupling.
- **First usable slice:** a one-dimensional wave model with known propagation speed and measurable reflection/boundary behavior.
- **Guardrail:** visually moving patterns are not evidence of physically correct propagation.

### I. Statistical, quantum, and nuclear extensions
- **Covers:** microscopic statistical models and other regimes that cannot be honestly reduced to classical continuum rules.
- **First usable slice:** define the intended scale, observable, and approximation before choosing a model.
- **Guardrail:** keep these as explicit future regimes; do not present classical models as universally valid.

### J. Persistent worlds, scale, and history
- **Covers:** deterministic spatial addressing, multirate simulation, coarse/fine representations, persistence, checkpointing, replay, and isolated timeline branches.
- **Key exchanges:** conservative state handoffs, elapsed logical time, provenance, and authoritative state.
- **Current status:** deterministic grid, logical-time ledger, and timeline/replay contracts exist; durable world persistence, conservative multiscale handoffs, and complete world reconstruction remain future work.

## Recommended build order: broad strokes, then depth

1. **Map and name the territory** — this document plus the roadmap; expose dependencies and gaps without marking them implemented.
2. **Finish shared state conventions** — quantities, stable identity, logical time, model provenance, boundaries, observations, and conservation diagnostics.
3. **Establish a minimum viable vertical slice per major domain** — mechanics, thermal state/energy, fluid state/flow, material response, species/reaction accounting, and fields/waves. Each slice must run and test independently.
4. **Couple the slices through explicit exchanges** — first heat ↔ material, then fluid ↔ heat/material, then reaction ↔ heat/species/flow.
5. **Use fire as an integration experiment** — a deliberately bounded ignition/combustion scenario that accounts for fuel consumption, released energy, heat transfer, gas/species transport, and material change. State exactly what is modeled and omitted.
6. **Backfill numerical depth** — stronger solvers, stability/convergence studies, property databases, uncertainty, additional regimes, and validated models for effects not required by the first scenario.
7. **Complete the domain** — cover the wider capability space without making the initial demo a prerequisite for unrelated physics.

The fire experiment is a demanding integration test, not the definition of Physics. A capability that is not needed for it can still belong in the domain atlas and be implemented later.

## Definition of done for a physical capability

- Governing assumptions and supported regime are stated.
- State variables and units are explicit and validated.
- Initial/boundary conditions and time-stepping behavior are declared.
- Numerical limits, stability/convergence, and error behavior are tested where applicable.
- Relevant conservation balances or residuals are observable.
- Determinism/replay expectations are tested.
- Material/model provenance and version are recorded where data or empirical laws are used.
- Scope and omitted effects are documented; unsupported behavior fails clearly rather than silently inventing an answer.

## Current truth

This map is a plan, not a claim of completion. On `development`, the working executable proof remains the one-dimensional constant-acceleration model, alongside universe/time/replay and broadphase foundations. Thermodynamics, hydrodynamics, heat transfer, materials, chemistry, and combustion still need implementation. Spatial mechanics work in open pull requests remains unmerged until explicitly approved.

---
[Physics roadmap](ROADMAP.md) · [Physics classes & ontology](PHYSICS_CLASSES.md) · [Theory and evidence](THEORY.md)
