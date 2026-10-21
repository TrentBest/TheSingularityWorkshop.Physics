# Mandelbrot Escape-Time Sampler

Mathematics stays inside TheSingularityWorkshop.Physics. This API samples the Mandelbrot escape-time classification at cell centers in a finite complex-plane window. It returns the evaluated coordinates, stable grid indices, iteration count, and whether the orbit escaped.

The implementation enforces three independent budgets: maximum sample count, maximum iterations per sample, and maximum worst-case sample-iterations. Inputs and coordinate windows are validated before allocation.

This is mathematical data, not physical behavior. The same records can be consumed by GUI.SVG, Renderer, or Visualize, but none of those consumers becomes the source of truth.

See the [source class](../Physics/Spatial/MandelbrotEscapeTime2d.cs), [UML class card and use cases](UML/MandelbrotEscapeTime2d.md), and [UML diagram](UML/MandelbrotEscapeTime2d.svg). Focused tests are in `tests/TheSingularityWorkshop.Physics.Tests/Spatial/MandelbrotEscapeTime2dTests.cs`.
