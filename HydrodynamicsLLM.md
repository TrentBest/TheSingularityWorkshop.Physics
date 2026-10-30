<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# HydrodynamicsLLM — implementation onboarding

**Audience:** AI contributors working on water, hydrodynamics, and fluid/terrain coupling in `TrentBest/TheSingularityWorkshop.Physics`.
**Authority:** the checked-out Physics source and tests, the FSM_API source and tests, then maintained documentation. This document is an onboarding contract, not proof that planned integrations already exist.

## 00. Mission and ownership

Own the water domain: shallow-water equations, waves, bathymetry, wet/dry interfaces, rainfall and runoff, water/structure interaction, and later validated fluid-body coupling.

The Physics library owns physical state, governing equations, numerical discretization, physical timestep, boundary conditions, diagnostics, and observations. A host or experience supplies configuration and external inputs. A renderer observes and presents computed state; it must not invent physical behavior.

Keep the numerical core renderer-independent and free of Unity, WPF, Visualize, or other presentation dependencies. Do not modify thermodynamics, combustion, or fire models as part of hydrodynamics work; coordinate through explicit, unit-bearing interfaces when coupling becomes necessary.

## 01. Read the current implementation before extending it

Start with:

- [Shallow-water model and current evidence](docs/HYDRODYNAMICS_SHALLOW_WATER_2D.md)
- [World demonstrator roadmap](docs/HYDRODYNAMICS_WORLD_DEMONSTRATOR.md)
- [Numerical validation playbook](docs/NUMERICAL_VALIDATION.md)
- [First wave reference](docs/HYDRODYNAMICS_FIRST_WAVE_REFERENCE.md)
- [Bathymetric solver](Physics/Hydrodynamics/BathymetricShallowWaterBasin2D.cs)
- [Flat-bed solver](Physics/Hydrodynamics/ShallowWaterBasin2D.cs)
- [Linear wave reference](Physics/Hydrodynamics/LinearWaveBasin1D.cs)
- [Rainfall rate field](Physics/Hydrodynamics/RainfallRateField2D.cs)
- [Focused solver tests](tests/TheSingularityWorkshop.Physics.Tests/BathymetricShallowWaterBasin2DTests.cs)
- [Rainfall tests](tests/TheSingularityWorkshop.Physics.Tests/RainfallRateField2DTests.cs)

Current source-checked capabilities include a fixed-step, depth-averaged 2D shallow-water solver; terrain-aware hydrostatic reconstruction; dry-cell handling; closed-wall fluxes; CFL validation; and a cell-centred precipitation-rate input that adds water depth without adding horizontal momentum. Tests exercise still-water balance over uneven terrain and a dry shoreline, wetting, volume accounting, rainfall input validation, and deterministic replay.

Do not overstate this as a general 3D fluid solver. The model does not currently establish turbulence, breaking waves, spray, infiltration, runoff routing, friction, erosion, moving terrain, open boundaries, or validated boat hydrodynamics.

## 02. Mathematical and numerical contract

- State the governing equations and conserved variables before changing a solver.
- Keep SI units, row-major grid layout, coordinate orientation, sign conventions, and boundary conditions explicit.
- For shallow water, distinguish bed elevation, free-surface elevation, depth, and depth-integrated momentum. Flow is not determined by bed slope alone.
- Preserve a still lake over uneven terrain, including dry shoreline cells above the free surface.
- In a closed basin without sources or sinks, account for water volume. For source operations, quantify the introduced volume separately from solver conservation.
- Keep dry-cell depth finite and non-negative and dry-cell momentum zero.
- Respect the solver's CFL guard. FSM update cadence is not physical time and cannot replace the solver's timestep or stability restriction.
- Build candidate state and validate it before committing mutations.
- Add analytical/reference comparisons and grid/timestep refinement evidence before making accuracy or convergence claims.

A visually plausible result is not numerical evidence. Use the evidence ladder and experiment declaration in [NUMERICAL_VALIDATION.md](docs/NUMERICAL_VALIDATION.md).

## 03. Use FSM_API for real lifecycle and orchestration

Read the authoritative [FSM_API LLM Usage Reference](https://github.com/TrentBest/FSM_API/blob/development/FSM_API_LLM_USAGE.md) and inspect the exact source/tests on the target branch before writing integration code.

Use FSM_API when behavior has meaningful persistent modes or an independently meaningful process lifecycle. Good candidates include a simulation controller (Created → Configured → Ready → Running → Paused/Faulted/Completed), a pump/gate/vessel lifecycle, or explicit coordination between validated physical subsystems.

Do not use an FSM per grid cell, calculate numerical fluxes through transitions, or replace the finite-volume solver with state-machine logic. A small stateless decision should remain ordinary code.

The Physics core currently has no FSM_API project/package reference. Do not add one merely to make the dependency visible. The likely integration boundary is an optional adapter/host-facing assembly, subject to a concrete use case and tests; the core hydrodynamic types must remain independently usable.

### Verified FSM_API surface

The string-backed API uses the actual builder/factory path:

- `FSM_API.Create.CreateFiniteStateMachine(...)`
- `.State(...)`, `.WithInitialState(...)`, `.Transition(...)`, `.BuildDefinition()`
- `FSM_API.Create.CreateInstance(...)`, retaining the returned `FSMHandle`
- `FSM_API.Interaction.Update(processingGroup)` from the host's chosen update loop

The integer-backed implementation is a separate API under `TheSingularityWorkshop.FSM_API.IntegerBacked`. Its source exposes `FSMBuilderInt`, `FSMRuntime`, `FSMHandleInt`, `Register`, `CreateInstance`, `Update(processingGroupID)`, and `UpdateAll`. Integer FSM and state identities are array-index identities, not string names. Choose this path only where stable integer identity and its runtime ownership are appropriate; do not assume it is a drop-in replacement for the string-backed API.

The application owns its context and chooses when to update. FSM_API owns registered definitions, handles, transition evaluation, and its supported lifecycle. Treat `IStateContext.IsValid` as a computation-eligibility contract only after checking the exact target API implementation; do not infer destruction or disposal from an invalid context. Keep physical simulation time, solver step count, and FSM processing cadence distinct.

### Integration acceptance criteria

Any new integration must demonstrate:

1. The exact FSM_API symbols and package/project reference used.
2. A real persistent lifecycle that benefits from an FSM, not a decorative state wrapper.
3. Explicit ownership of physical state and context data.
4. A documented mapping from FSM actions to physical operations and their simulation-time intervals.
5. No duplicate scheduler, event bus, generic state-machine abstraction, or hidden numerical lifecycle.
6. Correct pause/resume/fault behavior without partial physical-state commits.
7. Tests proving the host update cadence cannot silently advance or skip physical time.
8. A core-only build/test path that does not require the optional adapter.

If the integration does not meet these criteria, explain the specific mismatch before adding an abstraction.

## 04. Work discipline and evidence

- Work on `development`; retain only `master` and `development` as permanent branches. Do not create persistent task branches.
- Never publish a NuGet package or replace `master` without explicit owner authorization.
- Reuse existing types and tests; inspect the target branch instead of relying on memory or examples from another revision.
- Label claims as **Verified** (built/run on a named revision), **Source-checked** (inspected but not executed), or **Conceptual** (illustrative).
- Report exact tests/builds actually run. Do not convert an unrun test into a success claim.
- Keep end-user documentation focused on what the model does, why it exists, how to use it, how to verify it, and what it cannot yet claim.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
