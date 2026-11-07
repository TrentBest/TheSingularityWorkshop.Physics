# Physics Benchmarking

This document defines the first reproducible performance workload family for Physics. It complements the [Numerical Validation Playbook](NUMERICAL_VALIDATION.md): a fast result is useful only when the operation remains numerically and semantically valid.

## Scope

The harness references the Physics project directly and measures development source, not a published package, renderer, or host application.

| Stable category | Workload | Parameters | Timed boundary |
|---|---|---|---|
| `PHY-VECTOR3D-BATCH` | Scalar SoA addition versus `Vector3dBatch.Add` | 128, 4,096, 65,536 vectors | Three component-wise additions plus identical output checksum |
| `PHY-COLLISION-BROADPHASE` | `SweepAndPruneBroadphase.FindCandidates` | 100, 1,000, 10,000 proxies; 0.75 and 1.25 grid spacing | Full candidate generation, ordering, overlap tests, pair sorting, result allocation |
| `PHY-ASTRONOMY-NBODY` | `NewtonianNBodyStepper.Step` | 2, 8, 32, 128 bodies | One complete fixed velocity-Verlet step, including both acceleration evaluations and result allocations |

## Interpretation rules

- BenchmarkDotNet runs in Release with memory diagnostics.
- Fixture construction is in global setup and excluded from timed operations.
- The vector comparison uses the same SoA buffers and checksum on both paths.
- Broadphase layouts are deterministic regular grids, not a claim to model arbitrary production distributions.
- N-body measures the complete public step. The private acceleration evaluator is not reported as an isolated stage.
- Results are workload- and environment-specific. A successful workflow proves the harness ran, not that a performance winner or repeatable baseline has been established.
- No regression thresholds are enabled until independent runs establish variance and comparability.
- Correctness, numerical validation, and determinism remain separate gates. Never remove validation or change physical semantics solely to improve timing.

## Run locally

```sh
dotnet run --project benchmarks/TheSingularityWorkshop.Physics.Benchmarks/TheSingularityWorkshop.Physics.Benchmarks.csproj -c Release -- --filter '*'
```

Filter one family with `*Vector3dBatch*`, `*SweepAndPrune*`, or `*NewtonianNBody*`.

## Evidence lifecycle

Each hosted run records the source/harness commit, SDK/runtime, operating system, CPU information, and memory. BenchmarkDotNet raw reports are uploaded as workflow artifacts. Findings should be promoted into a dated report only after the raw artifact and provenance have been inspected.

The deterministic CPU reference world-stepper tracked in [Physics issue #31](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/31) requires a later stage-separated world benchmark with body/contact counts, allocations, and correctness diagnostics. That future benchmark must distinguish implemented stages from unsupported solver capabilities; the current N-body integrator is not a general rigid-body world stepper.
