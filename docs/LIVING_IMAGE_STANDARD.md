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

## Current priorities

- **Paired moving-train projectile:** the comparison image now exists as an analytical reference graphic. Replace its manually maintained coordinates with output rendered from the actual paired C# run record and have CI verify every plotted point.
- **Hydrodynamics:** keep conceptual state diagrams separate from future solver-backed basin/terrain field images. Add living snapshots as model runs become serializable.
- **Geology:** the volcanic-deposition schematic explains the current prescribed-volume model; future erosion, tectonics, and stratigraphy images must come from bounded models rather than invented geological output.
- **Math/LUTs:** the interpolation diagram is conceptual. Add generated error plots and convergence checks tied to actual table fixtures when those test outputs are exposed.
- **Domain coverage:** use the capability catalog and learning path to expose both existing visuals and missing image/model pairs. The catalog must distinguish implemented visuals from concepts and roadmap.

This policy is an ongoing repository-wide documentation requirement, not a claim that every domain already has a working simulator or a run-backed image.
