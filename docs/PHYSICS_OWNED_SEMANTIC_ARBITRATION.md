# Physics-owned semantic arbitration: materials and assemblies

**Status:** material identity, declared assembly/part validation, and material-property evidence mapping are executable and unit-tested. Automatic discovery from arbitrary MicroBundle data, typed provider resolution, capability matching, and install/remove re-arbitration remain unimplemented.

## Ownership

When the Physics MicroBundle is installed, **Physics owns arbitration over available composition data for physical meaning**. It should not require a central physics registry to enumerate every possible object in advance.

MicroBundles remain independently authored and may describe the same thing in different ways. Physics interprets declarations and evidence exposed through supported contracts, resolves candidates into Physics-domain concepts, and determines which physical data may apply. A generic host resolves declared provider IDs to loaded, typed, compatible, invokable capabilities; Physics resolves what the available data means physically. These are distinct responsibilities.

Physics must not depend on FSM_COS, Gestures, a renderer, or a particular experience to perform its domain reasoning. A composition host supplies the participating bundle set and lifecycle seam; Physics owns physical interpretation and material/part arbitration. See the [MicroBundle provider-resolution boundary](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/PROVIDER_RESOLUTION_CONTRACT.md).

## Executable slice currently present

The following pieces are executable and separate by responsibility:

- `Physics/Materials/PhysicalAssemblyDescription.cs` declares an assembly's stable identity and source provenance, plus independently addressable parts with source provenance and optional parent-part relationships. Validation rejects invalid/duplicate part IDs, missing parents, self-parenting, and cycles. It does not invent geometry or physical properties.
- `Physics/Materials/MaterialIdentityArbitrator.cs` resolves material identity per part from caller-supplied evidence. Explicit references outrank structured declarations and weaker alias/descriptive evidence. Equally strong conflicts remain ambiguous; unknown material IDs are not silently overridden by weaker known clues. Results and evidence are deterministically ordered and retain provenance.
- `Physics/Materials/MaterialPropertyDefinition.cs` represents a property value with units, evidence status, source/version, validity quantity/range/conditions, and optional uncertainty. It is a declaration of evidence, not a constitutive law or solver.
- `Physics/Materials/PhysicalAssemblyMaterialPropertyMapper.cs` maps each resolved part to property records whose material ID matches exactly. Ambiguous or unresolved parts receive no properties by default; unrelated catalog entries are ignored. Results are validated and sorted deterministically. The mapper preserves property records rather than combining conflicting values or selecting a solver.
- `Physics/Materials/MaterialPropertyRangeEvaluator.cs` compares an observed validity quantity, unit, and value with a property's declared numeric range. It reports within-range, outside-range, or incomparable; quantity and unit must match exactly. It does not convert units, interpret free-text conditions, or claim that all model assumptions are satisfied.

The tests under `tests/TheSingularityWorkshop.Physics.Tests/Materials/` cover independent steel-tip / wood-shaft / unresolved-fletching identities, assembly relationship validation, rejection of evidence for undeclared parts, explicit-reference precedence, conflicts, unknown identities, deterministic ordering, provenance, and mapping property evidence to the correct parts.

These contracts accept declarations/evidence from their caller. They **do not yet** discover arbitrary MicroBundle data, resolve declared provider IDs to typed objects, maintain a cross-provider search index, select compatible model/solver capabilities, or automatically react to bundle installation/removal. The mapping layer also does not decide whether a property's conditions fit a particular simulation state.

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

## Intended composition lifecycle

`installed bundle set → discover exposed descriptions → normalize semantic candidates → resolve ontology/material/part relationships → map property evidence → match model requirements → validate capability and solver compatibility → publish an inspectable resolution result`

Arbitration should be repeatable for the same bundle set, source data, rule/catalog versions, and configuration. Bundle enumeration order must not determine the answer. When bundles or descriptions change, the composition host should trigger reconsideration of affected results while preserving unrelated stable identities where possible.

This is domain arbitration, not a mandate to scan arbitrary private implementation fields. Bundles must expose inspectable metadata/data through supported contracts; Physics should not scrape inaccessible internals or execute untrusted bundle code merely to guess meaning. Provider discovery and invocation belong at the composition/host boundary, not in a universal service locator.

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

The existing unit tests cover identity resolution, assembly validation, exact-ID property mapping, and explicit range checks that do not silently convert units or infer unstated conditions. The remaining end-to-end proof must demonstrate:

- discovery from supported MicroBundle-exposed data and relationships;
- host-level typed provider resolution and compatibility checks;
- model/solver matching against required properties, units, conditions, and state inputs;
- adding/removing or updating a bundle causes relevant arbitration to be reconsidered without corrupting unrelated results;
- the Physics core continues to work without direct dependencies on FSM_COS, Gestures, or presentation packages.

See also [Physics architecture](ARCHITECTURE.md), [terrain interactions and MicroBundles](TERRAIN_INTERACTIONS_AND_MICROBUNDLES.md), and [composable Physics models, solvers, and experiences](COMPOSABLE_PHYSICS_MODELS_SOLVERS_EXPERIENCES.md).
