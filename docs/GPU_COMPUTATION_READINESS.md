# GPU Computation Readiness: Data, Numerics, and Backend Contracts

**Status:** preparation contract plus a small CPU reference batch API. This document does not claim that this repository currently executes physics or mathematics on a GPU.

## Objective

Prepare the same mathematical model to run on scalar CPU code, vectorized/batched CPU code, and eventually a GPU backend without allowing hardware choice to silently change the meaning of the model. Keep host-independent Physics free of renderer, windowing, Unity, or graphics API dependencies.

## Separation of responsibilities

1. **Model contract** — quantities, equations, boundary/initial conditions, and validity range.
2. **State layout** — logical fields and their memory layout, including array-of-structures (AoS) or structure-of-arrays (SoA).
3. **Reference implementation** — simple, testable CPU code that defines expected behavior for bounded inputs.
4. **Accelerated backend** — CPU SIMD, compute shader, or other GPU implementation selected by a consumer or an adapter.
5. **Verification** — compare accelerated output to reference output using declared absolute/relative tolerances, invariants, residuals, and conservation checks.

The physics package must not take a graphics dependency merely to gain GPU access. A renderer may share a device or buffers in an integration layer, but rendering resources must not become the physical state authority.

## Data layout principles

- Prefer contiguous batches and explicit counts over per-element allocation for large homogeneous workloads.
- Use stable entity/cell indices, not object addresses, allocation order, thread IDs, or runtime hash codes.
- Keep SoA layouts available for wide operations: separate x, y, and z arrays rather than requiring one object per vector.
- Keep double-precision CPU reference semantics where the current model uses double; define float32, float64, or mixed precision per operation before implementing a backend.
- Avoid hidden allocations and polymorphic dispatch inside per-cell/per-particle inner loops.
- Make strides, dimensions, bounds, alignment assumptions, and ownership explicit when external buffers are introduced.
- Treat data transfer and kernel launch costs as part of performance measurements; a GPU is not automatically faster for small batches.

## Numerical equivalence contract

GPU and CPU outputs need not be bit-identical unless an operation explicitly promises it. Floating-point addition is not associative; parallel reductions can change summation order. Each accelerated operation should declare one of:

- **Bitwise deterministic:** exact same representation required on the supported target/runtime matrix.
- **Numerically equivalent:** result agrees with a reference under stated absolute/relative tolerance.
- **Statistically equivalent:** distributional agreement is required and tested for a stochastic algorithm; use only where appropriate.

For stochastic work, use Squirrel3/Squirrel3Keyed from TheSingularityWorkshop.Physics.Numerics. Prefer keyed sampling by stable seed/entity/tick/purpose for work that may execute in parallel or in a different order. Document 32-bit key/output collision limits. Do not replace the source with an unspecified platform RNG in a GPU kernel. If a future GPU implementation reimplements Squirrel3, add CPU/GPU known-answer vectors before calling it compatible.

## Kernel contract checklist

Every kernel/batch operation should identify:

- operation and version;
- input/output element type and precision;
- shape, stride, and SoA/AoS layout;
- index domain and boundary behavior;
- input validity and non-finite-value policy;
- in-place/aliasing guarantees;
- deterministic and reduction policy;
- CPU reference implementation;
- error tolerances and invariant checks;
- workload size and measured transfer/dispatch/kernel timing;
- backend, device, driver/runtime, and code version in benchmark evidence.

## Current first step

Physics/Math/Vector3dBatch.cs is a deliberately small CPU reference API for SoA vector addition, scaling, and dot products. It writes into caller-owned spans, allocates no output buffers, and validates matching lengths. It is not a SIMD implementation, GPU kernel, or benchmark result. It is a starting contract that can be checked against future implementations.

## Validation ladder

1. Unit-test each batch operation against scalar Vector3d results.
2. Add randomized-but-replayable cases generated with Squirrel3 and fixed seeds; include edge cases separately.
3. Establish numerical tolerances and conservation/residual checks per physical model.
4. Benchmark scalar, batch CPU, and future GPU paths across multiple workload sizes; include allocation and transfer costs.
5. Add GPU known-answer tests for keyed random generation if that algorithm is ported.
6. Enable accelerated execution only when correctness evidence and a useful measured workload justify it.

## Explicit non-goals for this stage

- No GPU API dependency in the core package.
- No claim of GPU acceleration or SIMD speedup.
- No implicit precision downgrade from double to float.
- No hidden random generator.
- No rendering output used as proof of numerical correctness.
