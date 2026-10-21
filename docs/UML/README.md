# Source UML

This folder records manually maintained UML-style diagrams for selected C# source files. Each diagram must describe the current source contract, not a planned API. A diagram is evidence about structure, not a substitute for tests or implementation.

## Current diagrams

- [SeededPointSet2d](SeededPointSet2d.svg) — static generator, Squirrel3 dependencies, immutable read-only result, and finite half-open bounds.
- [MandelbrotEscapeTime2d](MandelbrotEscapeTime2d.svg) — bounded row-major escape-time grid, immutable sample records, finite complex-plane window, and explicit work limits.

## Maintenance rule

When a source type changes, update its diagram in the same change when its public contract or dependencies change. Expand coverage class by class; do not imply that this initial diagram covers every source file in the repository.
