# Physics Benchmark Results — First CI Run

**Status:** Initial directional evidence; not a validated regression baseline.  
**Workflow:** [Physics Benchmarks run #2](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/42328000248)  
**Source / benchmark commit:** `109e475ebc4d0490751f6de1b8922a4ad9162238`  
**Runner:** Ubuntu 24.04.5, AMD EPYC 9V74 (2 physical / 4 logical cores exposed), .NET 8.0.33 runtime, BenchmarkDotNet 0.15.2.  
**Artifact:** [Raw BenchmarkDotNet evidence](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/42328000248)

## Results

### Vector3dBatch addition

| Vectors | Scalar SoA | Vector3dBatch | Relative time |
|---:|---:|---:|---:|
| 128 | 429.0 ns | 335.7 ns | 0.78x |
| 4,096 | 14.971 μs | 12.130 μs | 0.81x |
| 65,536 | 236.843 μs | 188.885 μs | 0.80x |

The batch operation was approximately 19–22% faster than the scalar reference in this run. Neither path reported managed allocation. This is a comparison against the harness's scalar SoA implementation, not a claim against every possible scalar or SIMD implementation.

### Sweep-and-prune candidate generation

| Proxies | Grid spacing | Mean | Allocated |
|---:|---:|---:|---:|
| 100 | 0.75 | 4.145 μs | 24.6 KB |
| 100 | 1.25 | 4.918 μs | 20.52 KB |
| 1,000 | 0.75 | 168.470 μs | 328.97 KB |
| 1,000 | 1.25 | 58.379 μs | 200.77 KB |
| 10,000 | 0.75 | 7.888 ms | 3,995.7 KB |
| 10,000 | 1.25 | 2.193 ms | 1,949.64 KB |

Input distribution / spacing materially changes candidate-generation cost and allocation. The current regular-grid workload is a controlled synthetic case, not a representative distribution of all physics scenes. A future extension should report candidate counts and include sparse, clustered, and adversarial overlap distributions before drawing algorithmic conclusions.

### Newtonian N-body complete fixed tick

| Bodies | Mean | Allocated |
|---:|---:|---:|
| 2 | 324.9 ns | 952 B |
| 8 | 2.754 μs | 2,560 B |
| 32 | 39.158 μs | 8,992 B |
| 128 | 600.875 μs | 34,720 B |

This measures the complete public `NewtonianNBodyStepper.Step` fixed-tick operation for the supplied deterministic bodies. It is not an isolated force/acceleration microbenchmark and does not establish general rigid-body solver performance.

## Interpretation and limits

- The workflow built the source solution, ran correctness tests, executed all benchmark cases, and uploaded the raw BenchmarkDotNet reports.
- This is one CI run on a virtualized runner with only two physical cores exposed. Treat all values as directional evidence, not portable hardware truth.
- Do not set regression thresholds from this single sample. Repeat on independent runs and compare equivalent runtime, CPU, power, and environment conditions.
- Keep raw BenchmarkDotNet CSV, Markdown, JSON, logs, and environment manifest with the workflow artifact. This document is a concise index and interpretation, not a replacement for raw evidence.
- The 10,000-proxy broadphase cases show substantial sensitivity to the synthetic spacing parameter. Add candidate-count diagnostics and more varied spatial distributions before tuning or declaring an optimal configuration.
- Future deterministic CPU world-stepper coverage remains tracked separately in [Physics issue #31](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/31).

## Next validation

1. Repeat the same suite in another CI run to estimate run-to-run variation.
2. Add broadphase output-count diagnostics and sparse / clustered / high-overlap datasets.
3. Preserve the current workload definitions while extending coverage, so historical comparisons remain interpretable.
