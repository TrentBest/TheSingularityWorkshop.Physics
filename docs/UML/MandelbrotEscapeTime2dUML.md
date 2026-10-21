# MandelbrotEscapeTime2d — class diagram

```mermaid
classDiagram
    class MandelbrotEscapeTime2d {
      <<static>>
      +int MaximumSampleCount
      +int MaximumIterations
      +long MaximumWork
      +Generate(int width, int height, double minReal, double minImaginary, double maxReal, double maxImaginary, int maxIterations) IReadOnlyList~MandelbrotSample2d~
      -ValidateRange(double minimum, double maximum, string minimumName, string maximumName)
      -MapCellCenter(double minimum, double maximum, int index, int count) double
    }
    class MandelbrotSample2d {
      <<readonly record struct>>
      +int Column
      +int Row
      +double Real
      +double Imaginary
      +int Iterations
      +bool Escaped
    }
    MandelbrotEscapeTime2d ..> MandelbrotSample2d : returns row-major read-only samples
    MandelbrotEscapeTime2d ..> ReadOnlyCollection~MandelbrotSample2d~ : immutable collection wrapper
```

## Use case

A caller requests a bounded sample grid. The evaluator validates dimensions, finite half-open bounds, iteration cap, and worst-case work before allocating and computing samples. Downstream systems consume the recorded coordinates and classifications.

## Boundary

The type performs CPU mathematical evaluation only. It does not render pixels, emit SVG, or submit GPU work.
