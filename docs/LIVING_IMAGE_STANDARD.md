# Living Image Standard

**Policy:** if an image materially improves the explanation of a physical or mathematical concept, the image belongs in the page beside the explanation. Physics documentation is not text-first by default; equations, plots, diagrams, field views, and experiment comparisons are first-class explanatory artifacts.

## Image classes — label them honestly

1. **Run-backed living image** — generated from a serialized output of the actual C# model. CI reruns the model and checks that the visual still matches recorded samples or the committed fixture.
2. **Analytical/reference image** — calculated from an explicitly named analytical relationship or fixed example. State the equations and inputs. Do not imply that it came from a numerical solver if it did not.
3. **Conceptual schematic** — explains geometry, causal flow, units, or state relationships. Label it conceptual and do not present it as simulation output.
4. **Planned visualization** — a named gap where an image would materially help but the model, data, or rendering path is not yet available. Do not draw invented results to fill the gap.

## When an image is required

Add an image when it makes a phenomenon, model state, geometry, numerical method, comparison, conservation rule, uncertainty, or model limitation easier to understand than prose alone. This includes, where relevant:

- trajectories, reference frames, forces, collisions, and momentum/energy transfers;
- waves, fluid free surfaces, bathymetry, wet/dry boundaries, and fluxes;
- heat flow, phase changes, electrical circuits, fields, and coupled energy;
- terrain change, stratigraphy, erosion, tectonics, combustion, and material failure;
- numerical methods, interpolation, random sampling, convergence, error, and conservation residuals;
- process-group timing, observer resolution, model composition, and data provenance.

Do not force an image into a page when it adds no explanatory value. Prefer a clear absence/gap entry over a decorative but misleading picture.

## Minimum quality bar

- Embed the image in the relevant Markdown page; an orphan SVG in a folder is not documentation coverage.
- Give it useful alt text, units and legends where applicable, and an accessible SVG title/description.
- Identify model/run/version or explicitly mark the drawing as conceptual.
- Plot actual recorded states for model-backed visuals. A renderer may format data, but must not silently reimplement the physics.
- Show assumptions, boundaries, coordinate conventions, and relevant omissions.
- Validate numerical points, labels, data manifests, or generated-file equality in CI where feasible.
- Keep SVG/DOM/rendering dependencies outside the Physics core assembly.
- Link the model, test, run record, and renderer so a reader can reproduce the result.

## Current coverage and remaining priorities

- **Scale factor:** a conceptual living image now changes the object smoothly from 0.5× to 2× and advances the numeric label through intermediate values. It is a property illustration, not solver output.
- **Paired moving-train projectile:** the renderer consumes the actual paired C# run record; animated markers follow the plotted train/ground paths, and CI checks all four plotted curves against every recorded sample within SVG rounding tolerance.
- **Thermal conduction:** the displayed temperatures, 500 J transfer, and energy residual come from the actual run record. Animated markers show transfer direction; CI reruns the renderer and checks the committed SVG.
- **Spring-damper and process scheduling:** dedicated conceptual animations show spring compression/extension and distinct cadence pulses. Both disclose that they are illustrations, not numerical solver output or measured scheduler timing.
- **Hydrodynamics:** keep conceptual state diagrams separate from future solver-backed basin/terrain field images. Add living snapshots as model runs become serializable.
- **Geology:** the volcanic-deposition schematic explains the current prescribed-volume model; future erosion, tectonics, and stratigraphy images must come from bounded models rather than invented geological output.
- **Math/LUTs:** the interpolation diagram now has a living query marker, but its curve remains conceptual. Next, expose actual `LookupTable1D` error samples and render a run-backed error/convergence plot from them.
- **Deterministic randomness:** a conceptual living dataflow diagram contrasts order-sensitive stream consumption with identity-keyed samples; it is explicitly not numeric RNG output. The implementations and tests remain the authority for exact values.
- **Bipolar transistors:** the electricity theory page now embeds an accessible conceptual NPN/PNP cross-section that separates conventional-current arrows from electron motion and discloses forward-active assumptions. It is not solver output; the educational operating-region model and tests remain a gap.
- **Water freezing:** the thermodynamics page now embeds a conceptual liquid/freezing/ice-Ih sequence, including latent heat and the open lattice idea. The current lumped thermal primitive explicitly excludes phase change, so a phase-change model and model-backed lesson remain future work.
- **Domain coverage:** continue auditing every theory, capability, and experiment page. Where a living image would clarify a concept, embed it inline; where it cannot yet be honestly built, record the exact missing model/data/rendering dependency rather than silently leaving the page text-only.

This policy is an ongoing repository-wide documentation requirement, not a claim that every domain already has a working simulator or a run-backed image.

## Complex visual authoring with GUI.SVG

**GUI.SVG is an authorized companion for creating and simplifying complex explanatory SVGs.** Use it when hand-authoring paths, layered diagrams, motion, labels, or visual composition becomes needlessly difficult. The Physics repository should consume the exported SVG (and, where useful, the authoring source or reproducible generation steps); the Physics core must not acquire a GUI.SVG, DOM, or renderer dependency.

Choosing the authoring tool does not change the evidence standard:

- A run-backed image must still be generated from actual model/run-record data; GUI.SVG may arrange and style the data but must not recalculate or invent the physics.
- Conceptual visuals should remain explicitly labelled as conceptual. Animated movement can make relationships visible, but animation is not evidence that a model was simulated.
- Prefer reusable visual components and parameterized layouts for repeated diagrams instead of copying complicated path geometry by hand.
- Keep reduced-motion behavior, accessible SVG title/description, legible units, and a clear inline embedding in the relevant Markdown page.
- Where a visual has a model-backed source, retain a deterministic export/render step and CI check so editing tools cannot silently leave the committed image stale.

This makes GUI.SVG a visual-authoring aid in the documentation workflow, not a dependency of the Physics domain.
