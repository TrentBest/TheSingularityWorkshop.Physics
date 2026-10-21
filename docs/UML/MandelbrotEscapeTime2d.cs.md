# MandelbrotEscapeTime2d.cs

**User moniker:** Bounded Mandelbrot escape-time field

## Purpose

Generate a finite, renderer-independent grid of Mandelbrot escape-time samples. This source belongs to the mathematical model layer; SVG, raster, Renderer, and Visualize presentation are downstream consumers.

## Source and executable evidence

- Implementation: [Physics/Spatial/MandelbrotEscapeTime2d.cs](../../Physics/Spatial/MandelbrotEscapeTime2d.cs)
- Tests: [MandelbrotEscapeTime2dTests.cs](../../tests/TheSingularityWorkshop.Physics.Tests/Spatial/MandelbrotEscapeTime2dTests.cs)
- Executable run-record exporter: [sample recipe](../../samples/MandelbrotEscapeTime2dExport/README.md)
- Dedicated diagram: [MandelbrotEscapeTime2dUML.md](MandelbrotEscapeTime2dUML.md) · [existing SVG rendering](MandelbrotEscapeTime2d.svg)

## Class card

| Type | Contract |
|---|---|
| `MandelbrotEscapeTime2d` | Static bounded evaluator; `Generate(width, height, minReal, minImaginary, maxReal, maxImaginary, maxIterations)` |
| `MandelbrotSample2d` | Immutable value record containing column, row, complex coordinate, iteration count, and escaped flag |
| Output | Read-only samples in stable row-major order, evaluated at cell centers |
| Limits | 262,144 samples, 4,096 iterations per sample, and 50,000,000 worst-case sample-iterations |
| Failure behavior | Invalid dimensions, work budget, iteration cap, or non-finite/non-increasing bounds throw `ArgumentOutOfRangeException` |

## Observed constraints

- Each orbit starts at (z=0) and iterates (z leftarrow z^2+c); an orbit is marked escaped when squared magnitude exceeds four or becomes non-finite.
- A sample that does not escape is only known to have reached the configured iteration cap; it is not a proof of membership in the Mandelbrot set.
- Grid coordinates are sampled at cell centers in a half-open complex-plane window.
- The evaluator does not depend on presentation projects and does not emit SVG, raster output, or GPU commands.
- The sample exporter re-reads its versioned run record and compares each recorded sample against the in-memory output.

## Use case

Choose a finite complex-plane window, grid dimensions, and iteration cap; call `Generate`; then pass the immutable records to a separate presentation or export stage. The exporter demonstrates serialization round-trip validation without asking consumers to recompute the field.

## Evidence boundary

The unit tests cover row-major cell centers, known escaping/interior examples, repeatability, read-only output, and input/work limits. This is evidence for the CPU-side mathematical contract, not for GPU acceleration or deep-zoom numerical guarantees.
