# MandelbrotEscapeTime2d

**User moniker:** The Singularity Workshop / Trent Best

**Purpose:** Evaluate a bounded Mandelbrot escape-time grid in Physics, returning renderer-independent samples with stable row-major order and explicit iteration/work limits.

## UML class card

- **Type:** `MandelbrotEscapeTime2d` (static)
- **Public operation:** `Generate(width, height, minReal, minImaginary, maxReal, maxImaginary, maxIterations)`
- **Output:** read-only `MandelbrotSample2d` records containing grid indices, evaluated complex coordinate, iteration count, and escape classification.
- **Algorithm:** start z = 0, iterate z = z² + c, classify as escaped when |z|² > 4; a non-escaped result reached the configured iteration cap.
- **Limits:** 262,144 samples; 4,096 iterations/sample; 50,000,000 worst-case sample-iterations.
- **Presentation dependencies:** none.

See the [source UML diagram](MandelbrotEscapeTime2d.svg) and [focused tests](../../tests/TheSingularityWorkshop.Physics.Tests/Spatial/MandelbrotEscapeTime2dTests.cs).

## Use cases

1. Produce a deterministic finite escape-time field for a mathematical visualization.
2. Compare detail at explicit grid resolutions and iteration caps.
3. Hand the exact returned records to GUI.SVG, Renderer, or Visualize without moving the algorithm into those projects.

## Evidence boundary

This computes a sampled mathematical set. It is not a physical model, does not promise unlimited deep zoom or cross-platform bitwise identity, and does not itself create SVG, raster images, or GPU output.
