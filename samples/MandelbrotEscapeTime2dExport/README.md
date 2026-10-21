# MandelbrotEscapeTime2d run-record exporter

This console sample calls Physics' public `MandelbrotEscapeTime2d.Generate` implementation and exports the returned samples as a versioned JSON run record. It re-reads the file and compares every grid index, coordinate, iteration count, and escape flag against the in-memory Physics output.

## Generate

Defaults:

```bash
dotnet run --project samples/MandelbrotEscapeTime2dExport/MandelbrotEscapeTime2dExport.csproj --configuration Release -- ./artifacts/mandelbrot-default.run.json
```

Supply all seven parameters after the output path to explore another resolution, iteration cap, or complex-plane window:

```bash
dotnet run --project samples/MandelbrotEscapeTime2dExport/MandelbrotEscapeTime2dExport.csproj --configuration Release -- ./artifacts/mandelbrot-detail.run.json 120 80 160 -0.9 -0.35 -0.2 0.35
```

Parameter order: `width height maxIterations minReal minImaginary maxReal maxImaginary`. Numeric bounds use invariant-culture decimal notation. Width and height are positive; sample count is capped at 262,144; iteration count is capped at 4,096; total worst-case sample-iterations are capped at 50 million; bounds must be finite and strictly increasing.

## Contract

- modelId: `mathematics.mandelbrot-escape-time-2d`
- modelVersion: `1`
- schemaVersion: `1`
- `bounds`: finite half-open complex-plane window and cell-center sampling convention
- `sampleCount`: must match the sample array and width × height
- `samples`: row-major column/row, evaluated real/imaginary coordinates, iteration count, and escaped flag

Consumers must reject unsupported schema identity, mismatched dimensions/counts, non-finite or out-of-window coordinates, duplicate/missing grid indices, invalid iteration counts, and inconsistent classifications. Consumers render the recorded values; they must not recompute the fractal.
