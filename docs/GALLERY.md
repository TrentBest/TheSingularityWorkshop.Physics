# Physics Gallery

**Make physical rules inspectable.** This gallery connects deterministic Physics implementations to reproducible samples and visual explanations. A diagram or SVG is a proof surface only to the extent stated by its recipe; it is not a substitute for numerical assertions.

## Start here — Deterministic physical models

![Physics paradigm: same reality, a different way to build it](images/physics-paradigm.svg)

Physics provides composable models for forces, state changes, interactions, and observation. Start with the [capability catalog](PHYSICS_CAPABILITY_CATALOG.md) and [mathematics domain map](MATHEMATICS_DOMAIN_MAP.md).

## 01 — Celestial mechanics

**See:** a Newtonian gravity-field evaluator and fixed-step N-body integration using velocity Verlet, with explicit limits and validation cases.

**Recipe and evidence:** [Celestial Mechanics Foundation](CELESTIAL_MECHANICS_FOUNDATION.md) · [gravity-field source](../Physics/Astronomy/NewtonianGravityField.cs) · [N-body stepper](../Physics/Astronomy/NewtonianNBodyStepper.cs)

The documented solver is a deterministic O(N²) reference implementation. Its tests establish selected numerical properties; they do not establish production-grade long-horizon accuracy.

## 02 — Seeded procedural point sets

Generate and verify a versioned run record from the public sampler:

```bash
dotnet run --project samples/SeededPointSet2dExport/SeededPointSet2dExport.csproj --configuration Release -- ./artifacts/seeded-point-set-2d.run.json
```

**What it proves:** the exporter serializes the actual Physics output, re-reads it, and checks every index and coordinate against the in-memory result.

**Recipe:** [SeededPointSet2d exporter](../samples/SeededPointSet2dExport/README.md) · [sampler source](../Physics/Spatial/SeededPointSet2d.cs)

## 03 — Mandelbrot escape-time sampling

Generate the default bounded fractal run record:

```bash
dotnet run --project samples/MandelbrotEscapeTime2dExport/MandelbrotEscapeTime2dExport.csproj --configuration Release -- ./artifacts/mandelbrot-default.run.json
```

Or generate a detail window with explicit dimensions, iteration cap, and complex-plane bounds:

```bash
dotnet run --project samples/MandelbrotEscapeTime2dExport/MandelbrotEscapeTime2dExport.csproj --configuration Release -- ./artifacts/mandelbrot-detail.run.json 120 80 160 -0.9 -0.35 -0.2 0.35
```

**What it proves:** the sample calls the public sampler and validates the serialized grid against its computed output. It does not claim GPU rendering.

**Recipe:** [Mandelbrot exporter](../samples/MandelbrotEscapeTime2dExport/README.md) · [procedural geometry and fractal data contract](PROCEDURAL_GEOMETRY_AND_FRACTAL_DATA.md)

## 04 — Deterministic randomness and experiments

Explore [deterministic randomness](DETERMINISTIC_RANDOMNESS.md), the [living scale-factor experiment](SCALE_FACTOR_LIVING.md), and the [experiments index](experiments/). Each recipe should identify the actual model path and state whether its visualization is generated from solver output, an analytic calculation, or an explanatory fixture.

## Current evidence boundary

Recent development CI has successful runs for the seeded point-set sampler and exporter and the Mandelbrot sampler and parameterized exporter. See [Physics build and tests](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/workflows/build.yml). Generated run records are interchange data, not a promise that downstream consumers have already rendered them or that a GPU computation path is complete. See [GPU Computation Readiness](GPU_COMPUTATION_READINESS.md).
