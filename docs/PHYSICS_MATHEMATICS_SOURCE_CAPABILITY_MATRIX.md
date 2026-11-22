# Physics and Mathematics Source-to-Capability Coverage Matrix

**Snapshot:** 2026-11-22 UTC  
**Branch:** `development`  
**Purpose:** connect concrete source to independently consumable capabilities and their evidence. This is an implementation audit, not a claim of scientific completeness.

## Coverage rules

A capability is not considered MicroBundle-ready merely because a class, test, or manifest exists. The evidence chain is:

1. **Source:** concrete implementation and its owning domain.
2. **Contract:** stable typed input/output semantics, units, frames, time model, validity and exclusions.
3. **Adapter:** host-neutral provider that delegates to the real implementation.
4. **Manifest:** machine-readable identity, ontology placement, model and execution declarations.
5. **Tests:** focused behavioral and edge-case tests.
6. **Theory:** derivation and assumptions at textbook depth.
7. **Experiment:** reproducible question, controls, run record and observations.
8. **Validation:** analytical/reference checks, conservation or convergence evidence where applicable.

Statuses below describe repository artifacts, not scientific endorsement:
- **Present** means a corresponding artifact was located.
- **Partial** means some chain elements exist, but the capability boundary or evidence is incomplete.
- **Gap** means no dedicated artifact was identified in this first pass.
- **Not audited** means detailed implementation and scientific validity remain to be reviewed.

## Existing MicroBundle vertical slices

| Capability | Source | Typed adapter | Manifest | Tests | Theory / experiment | Assessment |
|---|---|---|---|---|---|---|
| Sphere-sphere narrowphase contact | `Physics/Collision/SphereSphereContactDetector.cs` | `SphereSphereContactMicroBundle` in `Physics/MicroBundles/ContactCapabilityMicroBundles.cs` | `microbundles/SphereSphereContact/microbundle.json` | `tests/TheSingularityWorkshop.Physics.Tests/Collision/SphereSphereContactDetectorTests.cs`; adapter tests in `tests/MicroBundles/PhysicsMicroBundleTests.cs` | `docs/SPHERE_SPHERE_CONTACT.md`; `docs/experiments/contact-impulse.md` | **Present / partial evidence:** detector contract is bounded to spherical narrowphase contact; experiment emphasis is broader impulse response and should not be mistaken for detector-specific validation. |
| Frictionless translational normal impulse | `Physics/Collision/ContactImpulseResolver.cs` | `NormalImpulseResponseMicroBundle` in `Physics/MicroBundles/ContactCapabilityMicroBundles.cs` | `microbundles/NormalImpulseResponse/microbundle.json` | `tests/TheSingularityWorkshop.Physics.Tests/Collision/ContactImpulseResolverTests.cs`; adapter tests in `tests/MicroBundles/PhysicsMicroBundleTests.cs` | `docs/experiments/contact-impulse.md` | **Present:** supplied-contact, translational normal response; does not claim friction, angular impulse, integration, or resting-contact support. |
| Prescribed kinematic motion | `Physics/Kinematics/PrescribedKinematicMotion.cs` | `PrescribedKinematicMotionMicroBundle` in `Physics/MicroBundles/Kinematics/PrescribedKinematicMotionMicroBundle.cs` | `microbundles/PrescribedKinematicMotion/microbundle.json` | `tests/TheSingularityWorkshop.Physics.Tests/Kinematics/PrescribedKinematicMotionTests.cs`; `tests/TheSingularityWorkshop.Physics.Tests/Kinematics/PrescribedKinematicMotionCommitTests.cs`; adapter tests | `docs/PRESCRIBED_KINEMATIC_MOTION.md` | **Present:** prescribed trajectory generation and achieved-state commit validation; not a force-driven dynamics solver. |

## High-value source families awaiting capability decomposition

These are candidate capability groups, not promises that every class becomes a separate bundle.

| Domain / family | Concrete source examples | Existing test signals | Initial next step |
|---|---|---|---|
| Mathematics — vector and matrix foundations | `Physics/Spatial/Vector2d.cs`, `Physics/Math/Matrix3x3d.cs`, `Spatial/Vector3d.cs`, `Physics/Math/Vector3dBatch.cs` | `tests/TheSingularityWorkshop.Physics.Tests/Math/Vector2dTests.cs`, `Matrix3x3dTests.cs`, `Vector3dBatchTests.cs`, `Spatial/Vector3dTests.cs` | Define vector, transform and batch-computation contracts with explicit frame/unit semantics; distinguish scalar and batch capabilities. |
| Mathematics — numerical methods | `Physics/Math/FiniteDifferences.cs`, `Quadrature.cs`, `OnlineStatistics.cs`, `LookupTable1D.cs` | Focused Math tests exist for finite differences, quadrature and online statistics | Group by coherent numerical method families; document error behavior, convergence assumptions and domain restrictions before adapter work. |
| Mathematics — deterministic sampling | `Physics/Math/Squirrel3Random.cs`, `Squirrel3Keyed.cs` | `Squirrel3RandomTests.cs` | Specify stream/key determinism, reproducibility and statistical claims separately; deterministic does not imply statistically validated. |
| Spatial geometry — 2D | `Physics/Spatial/Geometry2d.cs`, `Geometry2dSegments.cs`, `Geometry2dPolygon.cs`, `SegmentIntersection2d.cs`, `RigidTransform2d.cs` | Focused geometry, predicate, transform and tolerance tests exist | Form a geometry/predicate capability family with scale-aware tolerance policy and degeneracy semantics. |
| Spatial geometry — 3D primitives | `Spatial/Vector3d.cs`, `Quaterniond.cs`, `Pose3d.cs`, `Bounds3d.cs` | Focused 3D spatial tests exist | Audit shared primitive contracts and resolve folder ownership before defining adapters. |
| Kinematics and elementary mechanics | `Physics/Kinematics/ConstantAccelerationSolver.cs`, `InclinedPlaneSolver.cs`, `Physics/Mechanics/PlanarPendulumStepper.cs` | Focused kinematics and pendulum tests exist | Separate analytical state evaluation from time-stepping and force/dynamics models; document assumptions and validity envelopes. |
| Collision and contact | `Physics/Collision/SweepAndPruneBroadphase.cs`, `ContactPositionCorrector.cs`, `ProjectileTargetIntersection.cs` | Focused collision/contact tests exist | Keep broadphase, narrowphase, response and position correction as distinct composable stages. |

## Spatial ownership and namespace audit

The recursive tree contains both `Spatial/` and `Physics/Spatial/` folders. Inspected files in both declare the same namespace, `TheSingularityWorkshop.Physics.Spatial`.

- Root `Spatial/` currently contains 3D primitives: `Vector3d`, `Quaterniond`, `Pose3d`, `Bounds3d`, and `SparseVoxelVolume3d`.
- `Physics/Spatial/` currently contains 2D geometry and spatial algorithms: `Vector2d`, `Point2d`, coordinate frames, transforms, polygon/segment operations, seeded point sets, and Mandelbrot escape-time generation.
- `Physics/Math/` contains general numerical/mathematical algorithms under `TheSingularityWorkshop.Physics.Numerics`; for example, `Matrix3x3d` consumes the shared spatial `Vector3d`.

**Finding:** this is not currently a namespace boundary. Both folders contribute types to one public namespace, and at least one mathematical primitive depends on that shared namespace. Folder placement alone cannot define independent package or MicroBundle ownership.

**Decision deferred:** do not move files or rename namespaces as part of this inventory. First inspect project compile inclusion, public API consumers, serialization and cross-repository dependencies. Then choose a single coherent ownership convention (for example, all shared dimensional primitives under one spatial foundation with dimension-specific geometry capabilities above it). Any relocation must preserve public API compatibility or be explicitly versioned.

## Gaps before broad adapter expansion

1. Complete source/test/theory/experiment mapping for all 21 observed source domains.
2. Verify actual test coverage and assertions, not merely matching test filenames.
3. Review units, coordinate frames, time semantics, numerical stability, boundary conditions and invalid-input behavior per model.
4. Add dedicated experiments where they materially validate a model; avoid using a neighboring experiment as evidence for a different capability.
5. Define capability IDs and ontology mappings before assigning bundle IDs at scale.
6. Add adapter/manifest parity checks against runtime descriptors; the current validator checks schema, uniqueness, provider inclusion and referenced-file existence, but not source-code descriptor parity.
7. Add validation evidence levels that distinguish unit tests, analytical identities, convergence, conservation, reference comparisons and empirical calibration.

## Immediate sequencing

1. Resolve shared Spatial ownership by API/dependency audit, not file movement.
2. Build the Mathematics foundation vertical slice around vectors/transforms and numerical methods, choosing coherent capabilities rather than class-per-bundle wrappers.
3. Build mechanics/kinematics vertical slices with analytical references and reproducible experiments.
4. Expand collision stages only after each stage has a precise contract and independent evidence.
5. Keep the scientific core independent of MicroBundleDomain; expose providers from the adapter assembly.
