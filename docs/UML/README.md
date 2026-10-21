# Source UML

This folder records manually maintained UML-style diagrams for selected C# source files. Each diagram must describe the current source contract, not a planned API. A diagram is evidence about structure, not a substitute for tests or implementation.

## Current diagrams

- [SeededPointSet2d](SeededPointSet2dUML.md) — dedicated class diagram; [SVG rendering](SeededPointSet2d.svg) · [source companion and use case](SeededPointSet2d.cs.md).
- [MandelbrotEscapeTime2d](MandelbrotEscapeTime2dUML.md) — dedicated class diagram; [SVG rendering](MandelbrotEscapeTime2d.svg) · [source companion and use case](MandelbrotEscapeTime2d.cs.md).

## Maintenance rule

When a source type changes, update its diagram in the same change when its public contract or dependencies change. Expand coverage class by class; do not imply that this initial diagram covers every source file in the repository.
