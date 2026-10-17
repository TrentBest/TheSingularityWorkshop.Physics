# First-order room reflection: one source, one listener, one wall

**Status:** reproducible geometric path experiment; not an audio-waveform or full room-acoustics simulation.

## Question

For a fixed point source and listener inside a rectangular studio room, can the Physics model identify the specular path reflected once by a declared wall and compute its path length, propagation delay, and per-path level estimate?

## Controlled inputs

- Room: axis-aligned 10 m × 10 m × 10 m box.
- Source: (2, 5, 5) m, reference level 80 dB SPL at 1 m.
- Listener: (8, 7, 5) m.
- Speed of sound: 343 m/s.
- Maximum-X wall pressure-amplitude reflection coefficient: 0.5.
- All other wall coefficients: 0, so this run isolates one wall.
- Model: `acoustics.first-order-room-reflection`, version 1.

## Computed observations

The direct free-field path is 6.3246 m long, arrives after 18.4389 ms, and has an estimated level of 63.9794 dB SPL.

The model identifies one first-order reflection on the maximum-X wall at (10, 6.6, 5) m. Its path length is 10.1980 m, delay is 29.7319 ms, and per-path level estimate is 53.8091 dB SPL after applying the declared 0.5 pressure-amplitude coefficient.

These are separate path estimates. They are **not** summed into a combined listener level because the model has no waveform, phase, or interference representation.

The [versioned run record](first-order-room-reflection.run.json) captures the inputs, direct observation, reflected path, model identity, and limitations. The source of truth is the C# implementation in `Physics/Acoustics/FirstOrderRoomReflections.cs`; CI verifies that the committed record can be reproduced from that implementation.

## Reproduce

```bash
dotnet run --project tools/FirstOrderRoomReflectionRunRecordGenerator/FirstOrderRoomReflectionRunRecordGenerator.csproj --configuration Release -- /tmp/first-order-room-reflection.actual.json
dotnet run --project tools/FirstOrderRoomReflectionRunRecordGenerator/FirstOrderRoomReflectionRunRecordGenerator.csproj --configuration Release -- docs/experiments/first-order-room-reflection.run.json --check
cmp /tmp/first-order-room-reflection.actual.json docs/experiments/first-order-room-reflection.run.json
dotnet test TheSingularityWorkshop.Physics.sln --configuration Release
```

## Limits and next step

The room is an axis-aligned box with frequency-independent coefficients. Only first-order specular paths are considered; edge/corner hits are excluded. This does not model absorption by frequency, diffraction, scattering, transmission, occlusion, multiple reflections, reverberation, phase, binaural reception, or audible output. The reflected-path estimate is not a total room level.

The [SVG](first-order-room-reflection.svg) is rendered from the same run record and CI compares it with a fresh render from the actual C# output. Do not describe the MDN reference recording or a generic reverb effect as output from this model.
