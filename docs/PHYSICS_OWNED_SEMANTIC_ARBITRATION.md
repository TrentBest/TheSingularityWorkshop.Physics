# Physics-owned semantic arbitration: materials and assemblies

**Status:** the identity-resolution slice is executable and unit-tested; the end-to-end MicroBundle discovery, assembly graph, and re-arbitration lifecycle are not yet implemented in this repository.

## Ownership

When the Physics MicroBundle is installed, **Physics owns arbitration over the available bundle data for physical meaning**. It should not wait for a separate central physics registry to enumerate every possible object in advance.

MicroBundles remain independently authored and may describe the same thing in different ways. Physics examines the declarations and data made available through the installed composition, identifies candidate meanings, and resolves those candidates into Physics-domain concepts and capabilities. It then uses the resulting composition to decide which physical models and material data apply.

This is different from a generic host resolving a provider ID. Provider resolution answers “which loaded capability implements this declared contract?” Physics arbitration answers “what physical thing does this available data describe, how do its parts relate, and which physical models are applicable?”

Physics must not depend on FSM_COS, Gestures, a renderer, or a particular experience to perform its domain reasoning. The host supplies the participating bundle set and lifecycle seam; Physics owns physical interpretation and material/model arbitration.

## Executable slice currently present

`Physics/Materials/MaterialIdentityArbitrator.cs` implements **material identity resolution only**. The caller supplies the known material IDs, optional known aliases, and evidence with a part ID plus source bundle/field provenance. The arbitrator:

- ranks explicit material references above structured declarations and configured aliases;
- resolves each part independently and sorts results/evidence deterministically;
- leaves conflicts at the strongest evidence rank ambiguous;
- leaves unknown material IDs and unsupported descriptions unresolved;
- preserves the evidence behind the outcome.

`tests/TheSingularityWorkshop.Physics.Tests/Materials/MaterialIdentityArbitratorTests.cs` exercises the steel-tip/wood-shaft example, unresolved fletching, explicit-reference precedence, conflicting explicit references, order independence, unknown materials, and provenance.

This is a deliberate first slice, **not** the complete lifecycle described below. It currently accepts evidence supplied by its caller; it does not discover arbitrary MicroBundle data, establish an assembly/part graph, resolve typed provider objects, load material property records into a solver, or react automatically to bundle installation/removal. The existing `MaterialPropertyDefinition` remains a separate provenance-aware declaration and is not silently populated by identity resolution.

## Worked example: a steel-tipped arrow

Imagine the installed composition contains descriptions from independently authored bundles:

- a weapon bundle describes an **arrow**;
- a parts bundle describes a **tip**, **shaft**, and **fletching**;
- the tip's data contains an explicit reference to the known steel material;
- the shaft and fletching have their own material or construction descriptions.

The intended full arbitration flow is:

1. **Discover candidates.** Find explicit material references, part names, relationships, tags, structured properties, and descriptive strings exposed by the participating bundles.
2. **Resolve the tip's material.** A strong explicit reference to the known steel material resolves the tip to Physics' steel material identity. A mere occurrence of the word “steel” in unrelated prose is only a candidate, not proof.
3. **Resolve part relationships.** Connect the tip, shaft, and fletching to the arrow only when the source data or a declared composition rule supports those relationships.
4. **Compose the physical object.** Represent the arrow as an assembly of parts, each with its own material, geometry, mass properties, and relevant model assumptions when those values are available.
5. **Select applicable capabilities.** The resolved materials and part relationships can make mechanics, contact, fracture, thermal, or other models relevant. Only capabilities actually installed and compatible with the required inputs can be selected.
6. **Retain the evidence.** Record which source bundle and field supplied each fact, which rule resolved it, and whether the result is explicit, inferred, defaulted, ambiguous, or unresolved.

The result is not “the entire arrow is steel.” It is “the tip resolves to steel; the shaft and fletching resolve independently; the arrow is an assembly whose physical behavior follows from its parts and the models available.”

## Strings are evidence, not truth

Arbitration should use the richest available source, in this order where available:

1. stable semantic identifiers and explicit typed references;
2. structured properties and declared part/material relationships;
3. known ontology terms and aliases;
4. normalized names and descriptive strings as weaker evidence.

Matching must be deterministic and explainable. Keep source provenance and the rule used. If two candidates conflict, a string is ambiguous, or required physical properties are missing, retain the ambiguity or use an explicitly named fallback policy; do not silently invent a fact.

A material name alone does not supply density, elastic response, friction, thermal conductivity, heat capacity, phase behavior, strength, or fracture limits. Physics can resolve “steel” as a material identity while still reporting that a particular calculation lacks the property data it needs. Defaults must be explicit, versioned, and distinguishable from measured or source-authored values.

## Arbitration lifecycle

The intended flow is:

`installed bundle set → discover exposed descriptions → normalize semantic candidates → resolve ontology/material/part relationships → check conflicts and required data → compose compatible Physics capabilities → publish an inspectable resolution result`

Arbitration should be repeatable for the same bundle set, source data, rule/catalog versions, and configuration. Bundle enumeration order must not determine the answer. When new bundles or updated descriptions arrive, re-arbitrate the affected portion and preserve stable identities for unchanged resolved entities where possible.

This is domain arbitration, not a mandate to scan arbitrary private implementation fields. Bundles must expose inspectable metadata/data through supported contracts; Physics should not scrape inaccessible internals or execute untrusted bundle code merely to guess meaning.

## Result and safety requirements

A useful resolution record should include:

- source bundle and source-field identity;
- candidate concept/material/part identity;
- explicit, inferred, defaulted, ambiguous, or unresolved status;
- rule/catalog version and explanation;
- confidence or evidence strength when the method supports it;
- property values, units, and provenance, or an explicit statement that they are unavailable;
- dependent capabilities selected, rejected, or left unavailable, with reasons.

Unknown must remain unknown. A discovered label must not be upgraded into unsupported physical certainty. An unresolved property may prevent a solver from running, while still allowing the object and its known parts to exist in the composition.

## Minimum executable proof

The identity-resolution slice currently tests:

- a steel tip resolves independently of a wood shaft and unresolved fletching;
- an explicit typed material reference outranks contradictory descriptive text;
- repeated arbitration produces equal results regardless of evidence enumeration order;
- conflicting explicit references produce an ambiguity outcome;
- an unknown material stays unresolved and does not fabricate physical properties;
- source provenance survives resolution.

The remaining end-to-end tests must prove:

- actual discovery from supported MicroBundle-exposed data and assembly-part relationships;
- adding/removing a bundle causes relevant arbitration to be reconsidered without corrupting unrelated resolved state;
- compatible capability selection uses the host's real composition seam;
- the Physics core works without direct dependencies on FSM_COS, Gestures, or presentation packages.

See also [Physics architecture](ARCHITECTURE.md), [terrain interactions and MicroBundles](TERRAIN_INTERACTIONS_AND_MICROBUNDLES.md), and the [MicroBundle provider-resolution boundary](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/PROVIDER_RESOLUTION_CONTRACT.md).
