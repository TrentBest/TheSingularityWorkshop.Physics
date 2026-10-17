# Physics Capability Catalog

**Purpose:** give contributors and experiment authors one evidence-oriented index of what the Physics repository can do today, what each model assumes, and what remains outside its contract.

This is a catalog of bounded capabilities, not a claim that the repository is a complete physics engine. The source and tests linked below are authoritative for behavior. A domain name, roadmap item, SVG, or future-facing design document is not proof that a solver exists. For the instructional progression that consumes these capabilities, see the [Physics Learning Path](LEARNING_PATH.md); the [apple free-fall lesson](experiments/apple-free-fall.md) is the first educational SVG slice driven by actual recorded states.

## Reading capability status

- **Implemented primitive** — a concrete model or data contract exists in source and has a focused test suite.
- **Analytical/reference model** — useful as a baseline or teaching experiment, but intentionally narrower than a production solver.
- **Design contract** — describes intended boundaries or acceptance criteria; it does not mean the described system is implemented.
- **Roadmap** — proposed future capability.

An implementation claim should always be read with its assumptions, units, valid input range, and known omissions. Passing tests establish the behavior covered by those tests; they do not prove unrestricted physical accuracy.

## Executable capability index

| Area | Source entry points | What is represented | Important limits |
|---|---|---|---|
| 1D kinematics | [`ConstantAccelerationSolver.cs`](../Physics/Kinematics/ConstantAccelerationSolver.cs), [`KinematicState.cs`](../Physics/Kinematics/KinematicState.cs), [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/ConstantAccelerationSolverTests.cs) | One-dimensional constant-acceleration state advancement using SI quantities and explicit elapsed time | Not a general force or rigid-body integrator |
| Repeatable experiments | `Physics/Experiments/ConstantAccelerationExperiment.cs`, `ConstantAccelerationRunRecord.cs` | Declared initial state, acceleration, fixed timestep, step count, every sampled state, and a versioned JSON record | Initial schema; not yet a universal provenance, uncertainty, or validation framework |
| 3D projectile kinematics | [`ProjectileState.cs`](../Physics/Kinematics/ProjectileState.cs) and `ConstantAccelerationProjectileSolver`; [experiment runner](../Physics/Experiments/ProjectileExperiment.cs), [versioned run record](../Physics/Experiments/ProjectileExperimentRunRecord.cs), [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/ProjectileExperimentTests.cs) | Analytical 3D point-state advancement; declared z-up launch-angle convention; deterministic fixed-step trajectory samples; run ID, SI units, model/schema identity, and JSON record | No drag, spin, shape, rigid-body rotation, or siege mechanism; run record is model-specific, not universal provenance |
| Projectile/AABB intersection | [`ProjectileTargetIntersection.cs`](../Physics/Collision/ProjectileTargetIntersection.cs), [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/ProjectileTargetIntersectionTests.cs) | Earliest point-projectile intersection with a fixed axis-aligned target over a run's duration, solved from constant-acceleration boundary roots rather than only sampled positions | Point projectile and fixed AABB only; target must share the declared world frame; no radius, rotated geometry, moving targets, or response solver |
| Projectile impact diagnostics | [`ProjectileImpactDiagnostics.cs`](../Physics/Collision/ProjectileImpactDiagnostics.cs), [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/ProjectileImpactDiagnosticsTests.cs) | At a confirmed intersection, reports incoming position/time/velocity, declared mass, momentum, and kinetic energy | Diagnostic only; does not calculate impulse response, energy transfer, penetration, material damage, or structural failure |
| Spatial primitives | `Spatial/Vector3d.cs`, `Quaterniond.cs`, `Pose3d.cs` | Double-precision vector, orientation, and pose value types | Primitives do not by themselves provide a complete spatial dynamics solver |
| Rigid-body state | `Physics/Dynamics/RigidBodyState.cs` | Bounded rigid-body state representation | Not a full six-degree-of-freedom integration and constraint engine |
| Collision candidates | `Physics/Collision/AxisAlignedBounds.cs`, `SweepAndPruneBroadphase.cs` | AABB bounds and broadphase candidate screening, including the documented swept-bound behavior | A candidate is not a confirmed contact; broadphase is not a complete collision world |
| Sphere contact | `SphereSphereContactDetector.cs`, `ContactObservation.cs` | Sphere–sphere narrowphase observation and explicit contact data | Not general mesh collision, persistent contact manifolds, or all shape pairs |
| Contact response | [`ContactImpulseResolver.cs`](../Physics/Collision/ContactImpulseResolver.cs), `ContactPositionCorrector.cs`, [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/Collision/ContactImpulseResolverTests.cs) | Bounded normal-impulse response and positional correction; [living contact-impulse experiment](experiments/contact-impulse.md) records and visualizes the actual resolver output | Not a complete frictional, angular, multi-contact, or articulated solver; the demonstrated contact response is not yet coupled to projectile impact diagnostics |
| Axial mechanics | `Physics/Mechanics/AxialSpringDamper.cs`, `SpatialDistanceSpringDamper.cs`, `AxialLinkFailureMonitor.cs` | Axial spring/damper force calculations and a bounded link-failure monitor | Not a general constitutive-material or structural solver |
| Thermal conduction | [`ThermalConduction.cs`](../Physics/Thermodynamics/ThermalConduction.cs), [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/Thermodynamics/ThermalConductionTests.cs), [living experiment](experiments/thermal-conduction.md) | Signed two-body energy transfer using (Q=kA(T_1-T_2)\Delta t/L), property validity-range checks, an explicit overshoot guard, and total sensible-energy accounting | Lumped bodies only; illustrative conductivity; no spatial gradients, contact resistance, phase change, convection, radiation, or temperature-dependent properties |
| Thermodynamics | `Physics/Thermodynamics/LumpedThermalState.cs` | Lumped sensible-energy/temperature accounting under declared constant-specific-heat assumptions | No spatial gradients, general heat-transfer solver, phase change, or reaction energy |
| Hydrodynamics | `LinearWaveBasin1D.cs`, `ShallowWaterBasin2D.cs`, `BathymetricShallowWaterBasin2D.cs` | A 1D linear-wave reference and bounded 2D shallow-water models, including terrain-aware reconstruction and wet/dry behavior where specified | Not full 3D Navier–Stokes, breaking-wave, turbulence, boat-coupling, rainfall/runoff, or erosion physics |
| Geology | `Physics/Geology/VolcanicTerrain2D.cs` | Deterministic prescribed-volume volcanic deposition on a bounded 2D terrain representation | Not a general magma, heat-transfer, erosion, or tectonic model |
| Electricity | `Physics/Electricity/ElectricalGrid.cs` | Bounded resistive-load/grid accounting described in the domain document | Not a full electromagnetic field solver, transient power-electronics model, or validated city-scale grid |
| Acoustics | `Physics/Acoustics/FreeFieldAcoustics.cs`, `AcousticRoomBox.cs`, `FirstOrderRoomReflections.cs`; focused tests in `FreeFieldAcousticsTests.cs` and `FirstOrderRoomReflectionsTests.cs` | Analytical free-field observations plus bounded first-order specular path geometry for six axis-aligned rectangular room faces; per-path delay and pressure-level estimate | No waveform, phase/interference, frequency-dependent absorption, occlusion/diffraction, scattering, multiple reflections, or binaural rendering |
| Logical time | `Physics/Universe/LogicalTimeLedger.cs` | Requested versus committed logical ticks, bounded contiguous batches, explicit pending time | It accounts for time; it does not execute solvers or decide that a batch succeeded |
| Universe addressing | `Physics/Universe/UniverseGrid.cs`, `DeterministicCellGenerator.cs` | Stable finite grid addressing and deterministic cell generation inputs | Addressing does not imply physical isolation at cell boundaries or an implemented infinite universe |
| History and timelines | `Physics/Universe/PhysicsCheckpoint.cs`, `TimelineBranch.cs`, `TimelineLineage.cs`, and replay/history types | Explicit checkpoint, lineage, replay-plan, and history data contracts with tests | These types do not yet constitute a universal world snapshot, arbitrary reverse integration, or guaranteed rollback of every coupled solver |

Tests are organized under `tests/TheSingularityWorkshop.Physics.Tests/`. For the exact current acceptance surface, use the test names alongside the source rather than relying on this summary alone.

## Current 3D projectile evidence

The current projectile primitive evaluates the constant-acceleration equations for one requested positive finite timestep:

\[\mathbf{x}_{1}=\mathbf{x}_{0}+\mathbf{v}_{0}\Delta t+\tfrac{1}{2}\mathbf{a}\Delta t^{2}\]

\[\mathbf{v}_{1}=\mathbf{v}_{0}+\mathbf{a}\Delta t\]

The focused tests check a three-dimensional gravity example against analytical component values, compare 100 repeated 0.01-second steps with one 1-second step within a stated tolerance, and exercise invalid elapsed time/timestep plus elapsed-time overflow. This is evidence for the tested analytical state step only. It does not yet establish a trajectory experiment, target intersection, or impact response. The test does not independently validate a real-world launch or gravity measurement.

The coordinate frame must be declared by the caller. The current test uses Z as the vertical axis and gravity `(0, 0, -9.81) m/s²`; that is a test convention, not a universal coordinate convention imposed on every consumer.

## Derived acoustics: sound follows the physical event

The intended Workshop model is **physics-based sound, not an authored SFX inventory**. A crafted object should not need a manually assigned “clash sound.” Its ontology supplies what the object is; its material definition supplies relevant properties; the interaction produces a physical event; and an acoustic model derives sound from that event.

For example, a sword's geometry, mass distribution, metallurgy, stiffness, damping, and condition combine with the swing or thrust state and the contact partner's material. The resulting contact mechanics determine quantities such as contact location, relative velocity, impulse, deformation, and possible fracture. Those quantities can drive an acoustic source model, while the same physical event determines deflection, penetration, wear, or damage. A hard steel blade meeting another hard metal should not be treated as equivalent to striking cloth or a compliant target simply because both events are labelled “hit.”

The causal contract is:

**Ontology/material properties → motion → contact event → mechanical outcome + acoustic excitation → propagated observation**

- **Material/ontology:** declare relevant physical properties and their provenance. Names such as “steel” are not enough by themselves; alloys, heat treatment, geometry, and condition can differ.
- **Interaction:** derive contact from the physical state and solver, not from an animation cue or a sound trigger.
- **Damage:** use the actual motion and contact response, with declared material/structural limits. A swing's visual speed alone is not a damage formula.
- **Acoustics:** derive excitation from the contact/deformation/fracture event, then account for vibration, resonance, damping, and propagation as model capabilities permit.
- **Evidence:** record the event inputs and derived observations so a sound can be explained, replayed, and tested—not merely played.

**Current status:** the repository has an analytical free-field acoustics primitive, but does not yet implement this full material-to-contact-to-sound chain. Material-driven acoustic excitation, coupling to contact/damage, and richer propagation remain planned work. Do not describe them as implemented until source, focused tests, and a reproducible demonstration support the claim.

## Cross-cutting rules

### 1. Units and coordinate meaning

Use SI units internally unless a model explicitly documents another convention. State the dimensions of inputs in XML documentation and experiment definitions. A numeric value without a quantity meaning is not a complete physical input: distinguish metres from metres per second, force from impulse, energy from power, and temperature from heat transfer.

Spatial types use an explicitly documented coordinate convention. Do not assume that a world origin, handedness, vertical axis, or reference frame can be inferred from a renderer.

### 2. State, model, and solver are different things

- **State** describes the modeled system at a declared logical time.
- **Model** specifies the physical relationship or approximation being represented.
- **Solver/stepper** determines how the model is evaluated over a time interval.
- **Observation** reports measured or derived results.
- **Presentation** visualizes observations; it does not authoritatively alter them.

A new domain should keep these roles inspectable. Avoid a monolithic class that silently mixes material data, numerical stepping, rendering, and user input.

### 3. Time and deterministic behavior

Every experiment must declare its timestep or time-advance rule. Wall-clock time, render frames, and browser timers must not implicitly become simulation time. A deterministic run also needs stable input and event ordering; floating-point calculations alone do not guarantee cross-platform bitwise identity.

The `LogicalTimeLedger` protects the distinction between requested and committed time, but its caller must only commit a batch after the corresponding work really completes. See [Determinism](DETERMINISM.md), [Logical Time Ledger](LOGICAL_TIME_LEDGER.md), and [Process-Group Scheduling](PROCESS_GROUP_SCHEDULING.md).

### 4. Coupling requires an owned exchange

When one domain affects another, specify the quantity, units, sign convention, timing, and ownership of the exchange. For example, electrical power delivered to a heater is not automatically a temperature change: the thermal model must account for energy over an interval and apply its own assumptions. Avoid applying the same transfer twice or making the result depend on incidental processing order.

### 5. Fidelity changes must not invent continuity

A cheaper distant representation may aggregate state, but it must not reset the physical system or silently discard pending events. A handoff between models should declare what is preserved, what is approximated, and the expected error. See [Universe & Observer Resolution](UNIVERSE_AND_OBSERVER_RESOLUTION.md).

## How to add a capability

1. State the physical question and intended use.
2. Define state, units, coordinate frame, initial/boundary conditions, valid range, and assumptions.
3. Separate the governing relationship from the numerical stepping method.
4. Implement the smallest useful model without introducing a host, renderer, Unity, or FSM_COS dependency into the core.
5. Add focused tests for normal behavior, boundaries, invalid inputs, limiting cases, and invariants or conservation quantities that apply.
6. Compare against an analytical solution, published reference, or independently derived expected result where feasible; document tolerances and provenance.
7. Expose observations with enough context to explain their meaning and limitations.
8. Update this catalog and the relevant domain document. Mark planned couplings as planned until code and tests exist.

A useful test suite should answer more than “did it run?” It should help establish whether the model behaves as specified, where the approximation is valid, and how a future change could break it.

## Related documentation

- [Architecture](ARCHITECTURE.md)
- [Theory and evidence](THEORY.md)
- [Physics domain map](PHYSICS_DOMAIN_MAP.md)
- [Determinism contract](DETERMINISM.md)
- [Living theory](LIVING_THEORY.md)
- [Roadmap](ROADMAP.md)

---

[Physics home](../README.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
