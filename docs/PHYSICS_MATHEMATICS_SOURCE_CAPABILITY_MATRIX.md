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


## Focused mechanics and kinematics source review (2026-11-22)

This pass inspected the one-dimensional constant-acceleration solver, rigid-body state snapshot and free-body integrator, their focused tests, and the existing prescribed-motion implementation. This is a source-contract review, not a general dynamics or stability certification.

| Family | Observed implementation contract | Evidence and limitation | Readiness |
|---|---|---|---|
| Analytical 1D constant-acceleration kinematics | Immutable scalar state in SI units; exact closed-form position and velocity update for constant acceleration over a positive logical interval | Solver validates finite state/acceleration/time and rejects non-finite results. Focused tests and a separate experiment/run-record family exist. This is not a numerical integrator for varying acceleration and does not model forces or multi-dimensional motion. | **Present / bounded** |
| Free rigid-body state and stepping | Immutable snapshot with stable body ID, world-frame pose, linear velocity (m/s), angular velocity (rad/s), logical step and elapsed time; semi-implicit Euler translation and constant world-frame angular-velocity integration per step | Tests cover state validity/default rejection, analytical step values, orientation update, deterministic repeated runs and invalid timestep rejection. The integrator accepts acceleration as an input and does not derive forces, inertia, torque, contact, friction, constraints, or collision order. No convergence, energy/momentum conservation, or long-horizon stability evidence is claimed. | **Partial** |
| Prescribed trajectory kinematics | Bounded immutable trajectory definition; constant-velocity or quintic smooth-step profile; shortest-arc orientation progress; analytical speed/acceleration ceiling checks; absolute-time evaluation | Existing adapter and commit-validation slice has dedicated tests. This is commanded/prescribed motion, not force-driven response, gait, balance, or obstacle avoidance. | **Present / bounded** |

### Mechanics decomposition decision

Keep analytical kinematic evaluation, free rigid-body time stepping, prescribed trajectory generation, and collision/contact stages as distinct composable capabilities. They have different state contracts and numerical responsibilities. The rigid-body stepper is a useful bounded reference implementation, but it is not yet evidence for a general-purpose rigid-body dynamics provider. Before adapter work, define the host's force/acceleration ownership, fixed-step policy, angular-velocity frame convention, and explicit sequencing relative to collision/contact and constraints. Add reference or convergence experiments only for claims the implementation is intended to support.

## Mathematics implementation follow-up (2026-11-22)

The Squirrel3 tests now include five known-answer hash vectors and coverage for sequential stream reproducibility, bounded integer ranges, full signed-int range, stream fork, shuffle preservation, and keyed evaluation-order/parallel-scheduling invariance. They do not establish distribution quality, independence, cryptographic suitability, or suitability for every Monte Carlo workload. Any statistical-quality claim requires a separately designed battery and documented acceptance criteria.

A concrete half-open interval defect was found in Squirrel3Random.NextDouble(minimumInclusive, maximumExclusive): interpolation can round up to maximumExclusive even when the underlying unit sample is less than one. The implementation now returns the greatest representable value below the exclusive endpoint when that rounding occurs, and a regression test uses a reproducible seed and a large-magnitude narrow interval. The bounded-integer path already uses rejection sampling rather than direct modulo reduction.

Vector3dBatch is deliberately a lower-level raw span API: it does not enforce finite component inputs or outputs, whereas scalar vector APIs reject non-finite results. This distinction should remain explicit; it should not be described as equivalent scalar validation. No GPU backend exists in this implementation.


## Spatial ownership and namespace audit

The recursive tree contains both `Spatial/` and `Physics/Spatial/` folders. Inspected files in both declare the same namespace, `TheSingularityWorkshop.Physics.Spatial`.

- Root `Spatial/` currently contains 3D primitives: `Vector3d`, `Quaterniond`, `Pose3d`, `Bounds3d`, and `SparseVoxelVolume3d`.
- `Physics/Spatial/` currently contains 2D geometry and spatial algorithms: `Vector2d`, `Point2d`, coordinate frames, transforms, polygon/segment operations, seeded point sets, and Mandelbrot escape-time generation.
- `Physics/Math/` contains general numerical/mathematical algorithms under `TheSingularityWorkshop.Physics.Numerics`; for example, `Matrix3x3d` consumes the shared spatial `Vector3d`.

**Finding:** this is not currently a namespace boundary. Both folders contribute types to one public namespace, and at least one mathematical primitive depends on that shared namespace. Folder placement alone cannot define independent package or MicroBundle ownership.

**Decision deferred:** do not move files or rename namespaces as part of this inventory. First inspect project compile inclusion, public API consumers, serialization and cross-repository dependencies. Then choose a single coherent ownership convention (for example, all shared dimensional primitives under one spatial foundation with dimension-specific geometry capabilities above it). Any relocation must preserve public API compatibility or be explicitly versioned.

## Focused Mathematics source review (2026-11-22)

This review inspected the current implementations and focused tests for 2D/3D vectors, the 3×3 matrix primitive, scalar finite differences, composite Simpson quadrature, and Squirrel3. It is a source-contract review, not a complete numerical-analysis certification.

### Findings

| Family | Observed implementation contract | Evidence and limitation | Readiness |
|---|---|---|---|
| Scalar 2D/3D vectors | Immutable, finite-component, unit-agnostic values; exact equality; Euclidean operations; no frame or unit typing | Focused tests cover arithmetic, dot/cross products, normalization, invalid values, and selected overflow cases. Vector3d uses scaled magnitude/normalization; Vector2d uses direct squared-length arithmetic. Overflow exception behavior also differs: Vector2d explicitly throws ArithmeticException for non-finite operation results, while Vector3d routes many results through its finite-component constructor and throws ArgumentOutOfRangeException. Preserve or deliberately reconcile these public semantics before a shared provider contract. | **Partial** |
| Dense 3×3 matrix | Immutable finite row-major matrix; matrix-times-column-vector convention; determinant, transpose, product | Tests establish identity, transform convention, determinant, transpose, and selected overflow rejection. No inverse, decomposition, conditioning, or general matrix API is claimed. The source currently contains two adjacent XML summary comments on Multiply; remove the redundant one during cleanup. | **Partial** |
| Vector3d batch operations | Caller-owned structure-of-arrays spans; allocation-free CPU Add, Scale, Dot; matching-index aliasing supported | Tests cover basic values, in-place scale, mismatched lengths, and non-finite scalar rejection. Component inputs and arithmetic outputs follow raw IEEE-754 operations; unlike scalar Vector3d, they are not checked for finiteness. This is a meaningful contract distinction that must be explicit in provider metadata and tests. No GPU execution is implemented. | **Partial** |
| Finite differences | Scalar two-point central difference; optional Richardson refinement at h and h/2 | Tests compare against polynomial and sine derivatives, exercise invalid/non-finite paths, show a step-size tradeoff, and verify four function evaluations. The Richardson correction is explicitly an asymptotic indicator, not a rigorous bound; callers must provide a deterministic, sufficiently smooth function and choose a meaningful step. | **Present / bounded** |
| Numerical quadrature | Composite Simpson rule on finite intervals with positive even interval count; optional doubled-grid Richardson refinement | Tests cover exact cubic integration, sine, reversed/zero-width intervals, invalid inputs, overflow, and an exponential integral comparison. The correction magnitude is not a rigorous error bound and depends on smoothness/asymptotic behavior. The implementation limits work to one million refined intervals. | **Present / bounded** |
| Deterministic sampling | Squirrel3 32-bit deterministic hash, four-part keyed sampling, sequential stream, bounded integer selection, convenience methods, stream fork | Tests include known-answer hash vectors, stream repeatability, half-open range checks, full signed-int range support, endpoint consumption behavior, pick/shuffle preservation, keyed parallel-order invariance, and UnitDouble bounds. Bounded integers use rejection sampling to avoid modulo bias assuming a uniform 32-bit source; the Squirrel3 output's statistical quality is not established by these tests. Determinism is not statistical validation. A floating-point interpolation edge was found and fixed: rounding could return the exclusive upper bound for large-magnitude narrow intervals; a regression test now covers it. | **Partial** |

### Capability decomposition decision

Do not combine these into one broad "Mathematics" provider. The current evidence suggests at least three coherent capability families, subject to contract completion:

1. **Spatial linear algebra primitives:** scalar vectors, matrix transforms, and explicit frame/unit semantics. Keep scalar 2D/3D and batch execution as distinguishable execution surfaces under a coherent mathematical contract rather than assuming they have identical validation semantics.
2. **Numerical calculus:** derivative estimation and quadrature are separate numerical methods with different smoothness assumptions, work models, and error indicators. They may share a numerical-methods ontology parent, but should retain independent capability contracts.
3. **Deterministic sampling:** hash/random-access sampling and sequential stream generation need explicit reproducibility/version guarantees and separate statistical evidence.

Before creating adapters, close the identified contract gaps, inspect the remaining public methods and tests, and add independent experiments where they provide evidence beyond unit tests. No new bundle IDs are assigned by this audit.

## Gaps before broad adapter expansion

1. Complete source/test/theory/experiment mapping for all 21 observed source domains.
2. Verify actual test coverage and assertions, not merely matching test filenames.
3. Review units, coordinate frames, time semantics, numerical stability, boundary conditions and invalid-input behavior per model.
4. Add dedicated experiments where they materially validate a model; avoid using a neighboring experiment as evidence for a different capability.
5. Define capability IDs and ontology mappings before assigning bundle IDs at scale.
6. Adapter/manifest descriptor parity is now checked in CI: the validator maps each manifest bundle ID to an IMicroBundle adapter class and compares descriptor version and provider-ID set. The implementation uses a constrained C# source regex rather than a full parser; keep adapter declaration conventions stable and consider parser-backed validation if those declarations become more complex.
7. Add validation evidence levels that distinguish unit tests, analytical identities, convergence, conservation, reference comparisons and empirical calibration.

## Immediate sequencing

1. Resolve shared Spatial ownership by API/dependency audit, not file movement.
2. Build the Mathematics foundation vertical slice around vectors/transforms and numerical methods, choosing coherent capabilities rather than class-per-bundle wrappers.
3. Build mechanics/kinematics vertical slices with analytical references and reproducible experiments.
4. Expand collision stages only after each stage has a precise contract and independent evidence.
5. Keep the scientific core independent of MicroBundleDomain; expose providers from the adapter assembly.
