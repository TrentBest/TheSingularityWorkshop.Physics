# Living acoustic demonstration: source, space, listener

The README should not stop at a diagram when the subject is sound. **Show the causal picture and let the reader hear a source.** The experience should progressively compare what reaches a listener in different spaces, while stating which parts are measured, modeled, or illustrative.

## What can be demonstrated honestly today

Physics currently implements an analytical free-field observation: source/listener separation, propagation delay at a declared sound speed, direction, and a point-source distance-level estimate. It does **not** yet synthesize or transform a waveform. See the [acoustic world model](ACOUSTIC_WORLD_MODEL.md) and [free-field implementation](../Physics/Acoustics/FreeFieldAcoustics.cs).

A reference recording can still make the page audible. It is a **playback/UI reference**, not output from the Physics solver. For example, MDN provides a CC0 sample that can be played in a browser:

<audio controls preload="none">
  <source src="https://interactive-examples.mdn.mozilla.net/media/cc0-audio/t-rex-roar.mp3" type="audio/mpeg">
  Your browser does not support embedded audio. <a href="https://interactive-examples.mdn.mozilla.net/media/cc0-audio/t-rex-roar.mp3">Open the sample audio</a>.
</audio>

[Open the reference audio directly](https://interactive-examples.mdn.mozilla.net/media/cc0-audio/t-rex-roar.mp3).

GitHub's repository Markdown renderer may sanitize or omit embedded media elements. Keep the direct link as a fallback; a WebPage/HTML host can render the player reliably. The audio player belongs to the documentation host, not the physics core or SVG.

## The intended comparison

Use one fixed source recording or synthesized dry signal, one listener position, and a clearly declared environment per comparison. Change one environmental variable at a time:

| Comparison | What the listener should hear | What the visual should explain |
|---|---|---|
| Open/free field | Direct sound, without modeled room reflections | Source-to-listener path, distance, propagation delay, and level estimate |
| Small furnished room | Short reflections and faster decay, when a room response model exists | Nearby boundaries and absorption; early reflection paths |
| Large hall | More widely spaced reflections and a longer reverberant tail, when modeled | Longer paths, reflection density, and decay |
| Barrier / doorway | Attenuation or changed paths only when the chosen model supports it | Occlusion, opening geometry, and model limitations |
| Different listener positions | Different arrival delay and distance-level estimate today; directional/room differences when supported | Move the receiver and recompute the observation |

The room rows above describe the intended acoustic comparisons; they are **not current output**. Do not apply a generic reverb effect and label it a physical simulation unless the impulse response/filter, geometry/material inputs, algorithm, and limitations are declared. A real measured room impulse response is valid reference data if its provenance is recorded.

## The living SVG

The SVG should make source, boundaries, listener, direct path, reflected paths, and observation values legible. Generate the static version from declared scenario data. Where the host can run bounded controls, changing the source/listener position or environment should update the displayed measurements from the same model inputs.

SVG remains the diagram and visualization layer. Do not depend on SVG itself to play sound: browser support for audio embedded in SVG is not a dependable cross-browser contract. Use ordinary HTML audio controls alongside it, with captions and a direct link.

## Evidence gate for calling a sound “Physics-generated”

Before claiming the system generated a sound from physics, the implementation needs at least:

1. A versioned time-varying source signal or a defined synthesis model.
2. Explicit sample rate, channel layout, amplitude convention, and units/reference where applicable.
3. A propagation/room transformation with declared assumptions and bounded inputs.
4. A deterministic output or a documented source of nondeterminism.
5. Tests for signal delay, attenuation, reflection/decay, and edge cases appropriate to the model.
6. A generated audio artifact or live output whose provenance identifies the model and scenario.
7. A visual generated from the same scenario and measurements, not a separately invented illustration.

The first useful vertical slice is intentionally modest: **one audible source, a free-field baseline, and a measured/modelled room response comparison**. The existing free-field observation can anchor distance and delay now; room-dependent waveform changes must wait for a validated signal/room-response model. Do not convert lost mechanical energy into sound until a physical source-coupling model explicitly accounts for it.

## Related work

- [Acoustic world model](ACOUSTIC_WORLD_MODEL.md)
- [Living theory](LIVING_THEORY.md)
- [Acoustic causal map](images/acoustic-causality.svg)
- [Physics capability catalog](PHYSICS_CAPABILITY_CATALOG.md)
