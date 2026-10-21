# MandelbrotEscapeTime2d run-record exporter

This console sample calls Physics' public `MandelbrotEscapeTime2d.Generate` implementation and exports the returned samples as a versioned JSON run record. It does not implement the Mandelbrot iteration separately. The exporter re-reads the file and compares every grid index, coordinate, iteration count, and escape flag against the in-memory Physics output.

## Generate

```bash
dotnet run --project samples/MandelbrotEscapeTime2dExport/MandelbrotEscapeTime2dExport.csproj --configuration Release -- ./artifacts/mandelbrot-escape-time-2d.run.json
```

The default run is a 160 × 100 grid with an 80-iteration cap in the complex-plane window [-2.5, 1) × [-1.25, 1.25). Samples are evaluated at cell centers.

## Contract

- modelId: `mathematics.mandelbrot-escape-time-2d`
- modelVersion: `1`
- schemaVersion: `1`
- `bounds`: finite half-open complex-plane window and sampling convention
- `sampleCount`: must match the sample array and width × height
- `samples`: row-major column/row, evaluated real/imaginary coordinates, iteration count, and escaped flag

Consumers must reject unsupported schema identity, mismatched dimensions/counts, non-finite or out-of-window coordinates, duplicate/missing grid indices, invalid iteration counts, and inconsistent classifications. Consumers render the recorded values; they must not recompute the fractal.
