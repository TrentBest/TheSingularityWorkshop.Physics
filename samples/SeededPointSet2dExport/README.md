# SeededPointSet2d run-record exporter

This small console sample calls the public SeededPointSet2d.Generate API from the Physics project and exports its actual output as a versioned JSON run record. It is an interchange fixture for downstream consumers such as GUI.SVG; it is not a second implementation of the sampler.

## Generate

From the Physics repository root:

```bash
dotnet run --project samples/SeededPointSet2dExport/SeededPointSet2dExport.csproj --configuration Release -- ./artifacts/seeded-point-set-2d.run.json
```

The default record contains 128 points with seed 20261021 inside the half-open bounds [-10, 10) × [-10, 10). The exporter re-reads the written JSON and compares every serialized index and coordinate against the in-memory Physics output before reporting success.

## Contract

- modelId: procedural.seeded-point-set-2d
- modelVersion: 1
- schemaVersion: 1
- seed: unsigned 32-bit seed
- bounds: finite coordinate limits and explicit inclusive/exclusive convention
- pointCount: must equal the length of points
- points: ordered index, x, and y records in the unit-agnostic Physics coordinate space

Consumers must reject unsupported identity/schema versions, mismatched counts, non-finite coordinates, and coordinates outside the declared half-open bounds. They must render these recorded points rather than independently regenerating them.
