# SeededPointSet2d — UML

**Source:** [`Physics/Spatial/SeededPointSet2d.cs`](../../Physics/Spatial/SeededPointSet2d.cs)  
**Companion and use case:** [SeededPointSet2d.cs.md](SeededPointSet2d.cs.md)

```mermaid
classDiagram
    direction TB
    class Squirrel3 {
        +Hash(uint index, uint seed) uint
    }
    class SeededPointSet2d {
        <<static>>
        +MaximumPointCount int = 65536
        +Generate(int count, double minX, double minY, double maxX, double maxY, uint seed) IReadOnlyList~Vector2d~
        -ValidateRange(double minimum, double maximum, string minimumName, string maximumName) void
        -MapToHalfOpenRange(double minimum, double maximum, double sample) double
    }
    class Vector2d {
        +X double
        +Y double
    }
    Squirrel3 ..> SeededPointSet2d : supplies deterministic hashes
    SeededPointSet2d ..> Vector2d : creates ordered values
```

## Structural contract

The public operation is static and bounded. Each point index is hashed independently for X and Y with separate domain constants; values are mapped to finite half-open coordinate ranges. The output is read-only and unit-agnostic. Duplicate points are permitted.
