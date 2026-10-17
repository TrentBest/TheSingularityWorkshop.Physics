# Acoustic World Model: Sound Propagation, Listener Resolution, and Perception

## Purpose

Sound belongs in the physical world. A source vibrates, the environment carries and transforms that disturbance, surfaces reflect or absorb energy, barriers shape the path, and a listener receives a signal. Audio playback should present those results—not be a collection of unrelated clips attached to events.

This document defines composable acoustic capabilities in TheSingularityWorkshop.Physics. The branch now includes a first executable free-field observation baseline (source/listener positions, constant-speed propagation delay, normalized direction, and a distance-based dB SPL estimate) with tests. It is not yet a waveform delay line, wave solver, room-acoustics engine, binaural renderer, or acoustic perception system.

## The central idea: resolution follows the listener

In a dense science-fiction city, the world may contain thousands of simultaneous sound sources. Most do not need equal simulation detail. A listener needs the most accurate and timely result for nearby, relevant sound; distant sources can often use lower-cost propagation summaries, clustered sources, or precomputed environmental responses.

Use an adaptive, listener-centred hierarchy:

- **Ear and head region:** highest perceptual priority. Account for left/right arrival differences, head and pinna filtering, occlusion, reflections, and listener anatomy or species profile where supported.
- **Immediate surroundings:** fine spatial resolution around the listener and nearby interactive sources. A roughly metre-scale region is a useful starting design scale, not a universal fixed cell size.
- **Local environment:** room, corridor, street, vehicle cabin, or arena response. Model important reflections, barriers, openings, and material absorption.
- **Neighbourhood and city:** coarser propagation, source clustering, acoustic portals, cached responses, and precomputed or slowly updated environmental data.
- **Far field:** low-detail directional energy, ambience beds, and event summaries where exact waveform reconstruction would not materially affect perception.

Resolution should be adaptive, overlapping, and driven by perceptual error and events—not distance alone. A quiet floorboard creak just outside a doorway may matter more than a loud distant machine. A wall, open window, or sudden source movement can justify refinement.

## Numerical reality: centimetre cubes are not a universal sound grid

Audible sound spans a wide range of wavelengths. At ordinary air conditions, a 20 Hz tone has a wavelength on the order of 17 metres, while a 20 kHz tone is around 1.7 centimetres. A grid-based wave solver generally needs multiple samples per shortest wavelength it intends to resolve, plus a stable time step. A centimetre-scale 3D grid becomes extremely expensive if applied to a large volume and high frequencies.

Therefore, “centimetre-cubic volume around the ears” should be treated as a statement of *perceptual priority*, not a mandate to store one physical voxel per cubic centimetre throughout the listener volume.

Choose a representation appropriate to each frequency band and scale. Candidate methods include:

- Local time-domain wave simulation for selected small regions and frequency ranges.
- Geometric acoustic propagation (rays, beams, or paths) for high-frequency reflections and large environments.
- Frequency-band energy transport for coarse distant propagation and ambience.
- Measured or precomputed impulse responses for stable rooms and structures.
- Binaural filtering and head-related transfer functions to turn incoming direction and environment response into left/right ear signals.
- Hybrid methods, with explicit hand-offs and validation between them.

No single method is best for every frequency, distance, material, and moving scene. Keep the physical model separate from the output device and audio playback backend.

## What the model represents

An acoustic source should expose a physical or authored excitation with a clear contract. Depending on the capability, that may include source position and orientation, time-varying signal, frequency content, emission pattern, strength, and source identity. An event label alone is not an acoustic signal.

The environment contributes material and geometric response:

- Reflection and scattering.
- Frequency-dependent absorption.
- Transmission through or around barriers where modeled.
- Diffraction and doorway/window leakage at the selected approximation level.
- Occlusion and acoustic shadow regions.
- Propagation delay and distance-related attenuation.
- Doppler shift for moving source/listener pairs when enabled.
- Reverberation and decay from repeated reflections.
- Environmental state changes such as doors opening, vehicles moving, or structures being damaged.

The listener contributes position, orientation, motion, and a receiver model. A human, dog, or other simulated creature should not be represented merely by changing a volume slider. Hearing range, directional sensitivity, head/ear geometry, and perceptual processing may differ. A species-specific receiver profile can affect the signal rendered and the evidence available to an agent, while the underlying world acoustics remain consistent.

## Physical propagation is not sound playback

Keep these layers separate:

1. **Source excitation:** what vibrates and what signal is emitted.
2. **Propagation:** how the environment changes sound as it travels.
3. **Listener reception:** the signal arriving at each ear or receiver channel.
4. **Perceptual analysis:** which sources and features the listener can distinguish.
5. **Presentation:** audio-device output, visualization, or semantic descriptions for an agent.

This separation lets a host use the physical model without a specific audio engine, and lets different listeners perceive the same environment differently. A Renderer may own an update bucket or schedule acoustic work, but should not silently become the authority for physical propagation. Acoustic simulation is a composable physical capability; rendering and audio output are consumers.

## Adaptive acoustic buckets

A bucket is a scheduling and representation boundary, not a claim that sound stops at its edge. Propagation crosses bucket boundaries through defined transfers, cached responses, or source/listener interactions.

Candidate hierarchy:

- **Ear bucket:** high-priority receiver processing, left/right signal synthesis, and near-field refinement.
- **Listener-neighbourhood bucket:** nearby geometry, dynamic occluders, close sources, and rapidly changing contacts.
- **Room/street bucket:** environmental response, major reflections, doors, windows, and acoustic connections.
- **District bucket:** coarsened source groups, cached propagation, and slowly changing environmental summaries.
- **City/far-field bucket:** ambience and distant events, refined only when relevance or uncertainty requires it.

Each bucket needs an update policy, representation, maximum acceptable age, and hand-off rule to adjacent scales. Fast movement, an opening door, a nearby impact, or a sudden loud transient should trigger refinement. Stable distant machinery may be clustered or represented by a cached bed. Clustering must preserve perceptually important timing, direction, and spectral character rather than averaging away all identity.

## Quantifying sound

Sound can be quantified at several levels, and those levels should not be confused:

- **Signal level:** pressure or normalized sample amplitude, spectrum, phase where retained, and timing.
- **Propagation level:** path length, delay, reflection paths, absorption, directional energy, and uncertainty.
- **Receiver level:** left/right signals, interaural time and level differences, and spectral filtering.
- **Perceptual feature level:** loudness estimate, pitch or tonal character, onset, duration, direction confidence, masking, and source-class confidence.
- **Semantic level:** an interpreted event such as “a floorboard squeaked to your left.”

Decibels are logarithmic, not a linear measure of pressure or energy. Every stored quantity should identify its units, reference, frequency weighting, and whether it describes instantaneous pressure, band energy, estimated loudness, or a perceptual score.

The physical engine should expose computed evidence. A semantic layer can infer that a sound resembles a floorboard squeak, but should preserve confidence and uncertainty rather than turning a guess into guaranteed fact.

## From physical acoustics to agent awareness

Consider a floorboard squeaking to an agent's left:

1. A foot applies force to a board; a contact/material model or authored source model generates a time-varying excitation.
2. The structure and surrounding air carry the disturbance; room geometry and obstacles shape it.
3. The receiver model produces the signals arriving at the agent's ears.
4. A perception layer estimates direction, timing, and likely source class from those signals.
5. If the evidence is strong enough, the agent may receive a structured observation containing:
   - Event hypothesis: floorboard squeak.
   - Relative direction: left.
   - Estimated distance: bounded range or unknown.
   - Confidence: a calibrated score.
   - Observation time and, where available, first-arrival time.
   - Masking and uncertainty indicators.

This can give an agent time to react without granting it omniscience. It should not receive exact source identity through a side channel if simulated hearing would not reveal that identity. A loud nearby event may mask a quieter one; a wall may make direction or distance ambiguous; two similar sounds may remain indistinguishable.

A human-facing caption can be generated from the same structured evidence, but it is an interpretation of the received sound, not a substitute for simulating or rendering the sound itself.

## Baking and caching without losing the world

Stable environments are candidates for baking. Static geometry and material properties may be used to precompute room impulse responses, reflection summaries, or propagation data between acoustic regions. Runtime work can then focus on moving sources/listeners, changed doors, newly placed obstacles, and other dynamic departures from the baked state.

A baked result needs provenance: which geometry, materials, source/listener positions, frequency bands, and solver settings produced it. When relevant inputs change, the system must invalidate, update, or approximate the cached response with a declared error bound. Baking means precomputed environmental response—not hard-coded sound that ignores subsequent world changes.

## Microbundle composition

A host should assemble only the acoustic capabilities it needs. Candidate boundaries include:

- **Acoustic sources** — source excitations and emission patterns.
- **Propagation** — the selected wave, ray/beam, or band-energy model.
- **Acoustic materials** — absorption, reflection, scattering, and transmission parameters.
- **Environment response** — room/street response and reverberation.
- **Listener receiver** — head/ear or species-specific reception.
- **Binaural rendering adapter** — converts receiver results into left/right output.
- **Acoustic perception** — derives evidence and event hypotheses for agents.
- **Acoustic cache/bake** — stores stable environmental responses and invalidation metadata.

These are candidate capability boundaries, not a claim that these microbundles or solvers already exist as separate implementations. They may initially be namespaces or internal components and be separated only when the architecture justifies it. NuGet can provide a broad toolbox while a particular application composes only its required capabilities.

The core should remain independent of Unity, particular audio middleware, a specific GPU API, and the Renderer implementation. A Renderer bucket can schedule or consume acoustic work through an explicit contract; the physics package owns acoustic state and declared rules.

## Validation requirements

Acoustic claims should be supported by tests and reference scenarios. Initial validation should include:

1. **Free-field distance response:** compare against the chosen point-source approximation and state its assumptions.
2. **Propagation delay:** verify arrival timing against distance and configured speed of sound.
3. **Reflection and absorption:** verify expected energy changes in controlled simple geometries.
4. **Occlusion:** compare unobstructed and obstructed paths without claiming perfect blocking where diffraction/transmission is modeled.
5. **Binaural direction:** verify left/right timing and level cues for known source directions.
6. **Moving source/listener:** verify update continuity and Doppler behavior if implemented.
7. **Cache invalidation:** change a door, obstacle, or material and verify stale baked data is not silently treated as current.
8. **Scale transitions:** move a listener across bucket boundaries and check for audible discontinuities, lost events, or invalid timing.
9. **Deterministic mode:** where the chosen algorithm permits it, verify repeatability under fixed inputs and configuration.
10. **Perceptual event output:** verify semantic labels retain confidence and do not exceed evidence available to the receiver.

Benchmarks should report source count, geometry, frequency representation, update rate, memory use, latency, and quality/error criteria. “Thousands of sources” is a target workload to measure, not a performance claim until benchmarked.

## Incremental roadmap

1. **Implemented on this branch:** define metre-based positions, source/listener contracts, constant sound speed, free-field propagation delay, normalized source direction, and reference-level distance response; validate with deterministic unit tests.
2. Add an actual time-varying source signal and delayed receiver buffer, with explicit sample rate and fractional-delay policy.
3. Add occlusion and simple reflections using an explicitly selected method.
4. Add listener-centric resolution and acoustic-region interfaces.
5. Add binaural receiver output and verify direction cues.
6. Add caching/baking for stable environments with invalidation rules.
7. Add perception outputs for agents, including uncertainty and masking.
8. Profile dense-source scenes and refine scheduling, clustering, and GPU-friendly layouts against validated reference cases.
9. Introduce higher-cost local wave simulation only where it improves a defined listener-relevant error metric enough to justify its cost.

The goal is not to simulate every air molecule or store a tiny cube for every point in the city. It is to preserve important physical and perceptual consequences at the right scale, with a model that can explain its outputs and expose them to agents and applications.


## First executable baseline on `work/acoustic-free-field`

The initial implementation lives in `Physics/Acoustics/FreeFieldAcoustics.cs`:

- `AcousticPosition3` stores finite XYZ coordinates in metres.
- `AcousticSource` identifies a point source and its reference dB SPL at a declared reference distance.
- `AcousticListener` identifies the receiving position.
- `FreeFieldAcoustics.Observe` returns source/listener IDs, separation, propagation delay, direction from listener toward source, and estimated received dB SPL.

The baseline uses a constant 343 m/s sound speed by default and the idealized point-source pressure-spreading relation `L(r) = L(r₀) − 20 log₁₀(max(r, r₀)/r₀)`. The level is clamped at the reference distance for closer positions because this far-field approximation should not be extrapolated into the source's near field. Propagation delay still uses the actual separation.

This is an analytical observation model—not a simulated pressure waveform, material interaction, reflection, occlusion, Doppler, ear-canal model, or audible playback. Those require separate contracts and validation. Tests cover delay, inverse-distance pressure-level response, direction, coincident positions, near-field clamping, and invalid inputs.


## Implemented increment — bounded first-order room paths (2026-10-17)

The code now includes `AcousticRoomBox` and `FirstOrderRoomReflections.Observe` as a deliberately limited next step beyond free field. A room is an axis-aligned box in metres with one pressure-amplitude reflection coefficient per wall. The model mirrors the source across each wall, intersects the image-source path with the finite wall face, and reports valid first-order paths in a stable wall order.

Each `AcousticReflectionObservation` records the room/source/listener identities, wall ID, reflection point, path length, propagation delay, reflection coefficient, and estimated received dB SPL for that path. The path level applies the same inverse-distance point-source estimate plus `20 log10(reflectionCoefficient)`. A coefficient of zero suppresses that wall's path.

**This is path metadata, not room audio.** The result does not combine direct and reflected paths, and does not model phase, interference, spectrum, frequency-dependent absorption, scattering, diffraction, transmission, occlusion, repeated reflections, reverberant decay, or waveform generation. Source and listener must lie strictly inside the room, and edge/corner hits are excluded to avoid double-counting a path across adjacent faces. Tests cover a known reflection point, path length, delay, per-path level, deterministic wall ordering, and invalid bounds/coefficients/positions.

The next evidence gate is a reproducible experiment record and SVG generated from this model's actual output. Do not present the room as audible or the per-path level as a combined listener level.
