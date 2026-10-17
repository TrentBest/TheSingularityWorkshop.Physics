# Physics-owned semantic arbitration: materials and assemblies

**Status:** the Physics package provides deterministic domain evaluators for caller-supplied assembly/material evidence. The extended application behavior—MicroBundle discovery, cross-bundle arbitration orchestration, provider acquisition, composition, and re-arbitration when bundles change—belongs to the MicroBundle composition/arbitration layer and is not implemented by these Physics primitives.

## Ownership

When the Physics MicroBundle is installed, the **MicroBundle arbitration layer orchestrates discovery and composition**. It may call Physics-owned evaluators to interpret the physical meaning of the supplied declarations and evidence; the Physics NuGet package does not own the MicroBundle lifecycle or the application-level arbitration workflow.

MicroBundles remain independently authored and may describe the same thing in different ways. The MicroBundle arbitration/composition layer discovers the participating declarations, coordinates candidates, and assembles the higher-level result. Physics-domain functions can then interpret caller-supplied declarations and evidence, resolve physical identities, and assess which physical data may apply. A generic host resolves declared provider IDs to loaded, typed, compatible, invokable capabilities; these are distinct responsibilities from physical interpretation.

Physics must not depend on FSM_COS, Gestures, a renderer, or a particular experience to perform its domain reasoning. The composition host supplies the participating bundle set and lifecycle seam and owns the orchestration of MicroBundle discovery and re-arbitration. Physics provides deterministic domain-level identity and evidence evaluators that this orchestration may invoke. See the [MicroBundle provider-resolution boundary](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/PROVIDER_RESOLUTION_CONTRACT.md).

## Executable slice currently present

The following pieces are executable and separate by responsibility:

- `Physics/Materials/PhysicalAssemblyDescription.cs` declares an assembly's stable identity and source provenance, plus independently addressable parts with source provenance and optional parent-part relationships. Validation rejects invalid/duplicate part IDs, missing parents, self-parenting, and cycles. It does not invent geometry or physical properties.
- `Physics/Materials/MaterialIdentityArbitrator.cs` resolves material identity per part from caller-supplied evidence. Explicit references outrank structured declarations and weaker alias/descriptive evidence. Equally strong conflicts remain ambiguous; unknown material IDs are not silently overridden by weaker known clues. Results and evidence are deterministically ordered and retain provenance.
- `Physics/Materials/MaterialPropertyDefinition.cs` represents a property value with units, evidence status, source/version, validity quantity/range/conditions, and optional uncertainty. It is a declaration of evidence, not a constitutive law or solver.
- `Physics/Materials/PhysicalAssemblyMaterialPropertyMapper.cs` maps each resolved part to property records whose material ID matches exactly. Ambiguous or unresolved parts receive no properties by default; unrelated catalog entries are ignored. Results are validated and sorted deterministically. The mapper preserves property records rather than combining conflicting values or selecting a solver.
- `Physics/Materials/MaterialPropertyRangeEvaluator.cs` compares an observed validity quantity, unit, and value with a property's declared numeric range. It reports within-range, outside-range, or incomparable; quantity and unit must match exactly, and the assessment retains the property's evidence status, source/version, and numeric bounds. It does not convert units, interpret free-text conditions, or claim that all model assumptions are satisfied.
- `Physics/Materials/MaterialModelCapabilityMatcher.cs` compares a model capability's declared property prerequisites against one part's mapped catalog evidence. It reports ready, missing prerequisites, incompatible exact units, or both. It does not choose between competing property values, infer evidence quality, check free-text conditions, or execute a solver.

The tests under `tests/TheSingularityWorkshop.Physics.Tests/Materials/` cover independent steel-tip / wood-shaft / unresolved-fletching identities, assembly relationship validation, rejection of evidence for undeclared parts, explicit-reference precedence, conflicts, unknown identities, deterministic ordering, provenance, and mapping property evidence to the correct parts. `MaterialEvidencePipelineTests` additionally exercises the sequence from declared assembly and caller-supplied identity evidence through property mapping and range assessment, including an unresolved part and a unit mismatch. Model capability tests additionally verify required-property availability and exact-unit compatibility.

These Physics contracts accept declarations/evidence from their caller. They are domain evaluators, not the MicroBundle application workflow. The MicroBundle composition/arbitration layer must discover exposed bundle data, resolve provider IDs to typed objects, maintain any cross-provider index, coordinate capability matching, bind compatible providers, and trigger re-arbitration after installation/removal/update. None of that lifecycle or orchestration is implemented here. The mapping layer also does not decide whether a property's conditions fit a particular simulation state.

## Worked example: a steel-tipped arrow

An assembly declaration can identify an arrow and its tip, shaft, and fletching independently. Material evidence may resolve the tip to a known steel identity and the shaft to wood while leaving the fletching unresolved. A catalog can then supply property records for the resolved materials:

- the tip maps only to records whose material ID exactly matches steel;
- the shaft maps only to records whose material ID exactly matches wood;
- the unresolved fletching receives no material properties by default;
- each property retains its unit, evidence status, validity range, source, and version.

The result is **not** “the entire arrow is steel,” nor does a steel label imply known density, elasticity, friction, conductivity, heat capacity, strength, or fracture limits. Property evidence is not automatically suitable for every state or model. Geometry, applicable conditions, model assumptions, and solver capabilities must be established separately.

## Strings are evidence, not truth

Use the richest available source, where present:

1. stable semantic identifiers and explicit typed references;
2. structured properties and declared part/material relationships;
3. known ontology terms and aliases;
4. normalized names and descriptive strings as weaker evidence.

Matching must be deterministic and explainable. Keep source provenance and the rule used. If candidates conflict, a string is ambiguous, or required physical properties are missing, preserve that outcome or apply an explicitly named fallback policy; do not silently invent a fact. Defaults must be explicit, versioned, and distinguishable from measured or sourced values.

## Intended MicroBundle arbitration lifecycle

`installed bundle set → discover exposed descriptions → normalize semantic candidates → resolve ontology/material/part relationships → map property evidence → match model requirements → validate capability and solver compatibility → publish an inspectable resolution result`

The MicroBundle arbitration layer should make composition repeatable for the same installed bundle set, source data, rule/catalog versions, and configuration. Bundle enumeration order must not determine the answer. When bundles or descriptions change, that layer should trigger reconsideration of affected results while preserving unrelated stable identities where possible.

This is a MicroBundle composition/arbitration responsibility, not a mandate to scan arbitrary private implementation fields. Bundles must expose inspectable metadata/data through supported contracts; neither the host nor Physics should scrape inaccessible internals or execute untrusted bundle code merely to guess meaning. Provider discovery and invocation belong at the composition/host boundary, not in a universal service locator.

## Result and safety requirements

A complete resolution record should make inspectable:

- source bundle, source field, and property source/version;
- candidate material/part identity and relationship;
- resolved, ambiguous, or unresolved status, with the rule and explanation;
- property values, units, validity conditions, evidence status, and uncertainty when available;
- required inputs that are missing or outside their valid conditions;
- candidate models/capabilities selected, rejected, or unavailable, with reasons.

Unknown must remain unknown. A discovered label must not be upgraded into unsupported physical certainty. A missing property may prevent a solver from running while still allowing the object and its known parts to exist in the composition.

## Remaining executable proof

The existing unit tests cover identity resolution, assembly validation, exact-ID property mapping, explicit range checks that do not silently convert units or infer unstated conditions, model prerequisite matching, and an integrated material-evidence pipeline test. The remaining end-to-end proof must demonstrate:

- MicroBundle-layer discovery from supported bundle-exposed data and relationships;
- host-level typed provider resolution and compatibility checks;
- model/solver matching against required properties, units, conditions, and state inputs;
- MicroBundle installation/removal/update causes the composition layer to re-arbitrate affected results without corrupting unrelated results;
- the Physics core continues to work without direct dependencies on FSM_COS, Gestures, or presentation packages.

See also [Physics architecture](ARCHITECTURE.md), [terrain interactions and MicroBundles](TERRAIN_INTERACTIONS_AND_MICROBUNDLES.md), and [composable Physics models, solvers, and experiences](COMPOSABLE_PHYSICS_MODELS_SOLVERS_EXPERIENCES.md).
