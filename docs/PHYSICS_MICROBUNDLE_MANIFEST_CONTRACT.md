# Physics MicroBundle Manifest Contract

**Status:** schema proposal v1.0, descriptive metadata only.  
**Scope:** Physics adapter manifests in this repository. This schema does not change MicroBundleDomain runtime behavior.

## Why this boundary exists

The runtime contract in MicroBundleDomain currently exposes a numeric bundle ID, a version string, provider ID declarations, numeric bundle dependencies, load/arbitration methods, and dependency requests that may carry opaque configuration bytes. The Physics adapter assembly implements that contract and exposes typed CLR provider interfaces. The runtime does not automatically resolve a provider ID to a typed interface, interpret Physics ontology, load this JSON, or validate equations.

Therefore, a manifest is a **catalog and traceability record**, not an executable registration, service locator, proof of compatibility, or proof of scientific validity.

## Contract mapping

| Manifest field | Meaning | Runtime relationship |
|---|---|---|
| `bundle.id` | Stable non-zero numeric bundle identity | Must equal `MicroBundleDescriptor.Id` |
| `bundle.version` | Semantic bundle version | Must equal `MicroBundleDescriptor.Version` |
| `bundle.providerIds` | Declared provider identifiers | Must equal descriptor provider IDs; typed interface resolution remains host-owned |
| `ontology` | Physics/Mathematics placement | Metadata only; MicroBundleDomain does not interpret it |
| `capability` | Human/machine-readable capability statement | Metadata only |
| `model` | Formulation, assumptions, exclusions, units, frame and validity | Metadata only; must not imply unimplemented phenomena |
| `execution` | Call model and host responsibilities | Metadata only; explicitly distinguishes a provider from a simulation loop |
| `evidence` | Source, test and validation traceability | References must exist; status must reflect actual evidence |
| `education` | Theory and experiment entry points | Makes professor-facing material discoverable |

The current neutral descriptor's dependency list contains bundle IDs only. The executable `IMicroBundle.Dependencies` surface uses `MicroBundleDependencyRequest`, which can carry opaque configuration bytes. Schema v1 does not invent a richer dependency graph or claim that these two surfaces are interchangeable. A later schema can add typed dependency semantics only alongside a runtime contract that can enforce them.

## Scientific declaration rules

1. Every model lists assumptions and exclusions. A narrow analytical function must not be described as a general solver.
2. Units are explicit. Dimensional quantities must not silently mix unit systems.
3. Coordinate frame and time model are declared whenever they affect interpretation.
4. Determinism is a claim about the actual implementation and inputs, not a property inferred from a manifest.
5. Validation names the evidence: analytical comparison, conservation residual, convergence, invariants, deterministic replay, or reference data as applicable.
6. `implemented` means the provider and its relevant behavior exist in source and are covered by tests. It does not mean all adjacent domain phenomena are implemented.
7. Source, test, theory and experiment paths are repository-relative and case-sensitive.
8. Missing metadata is not permission to guess. Use explicit known gaps or leave optional details out.

## Versioning

- `schemaVersion` versions this manifest structure independently of the bundle's semantic version.
- Additive optional fields may be introduced in a compatible minor schema revision.
- Renaming/removing required fields or changing their meaning requires a new major schema version.
- Runtime identity fields must remain aligned with the actual descriptor.
- A manifest validator should fail closed on malformed JSON, unknown fields, missing required fields, duplicate provider IDs, invalid ontology domains, and mismatched runtime identity.

## Current adapters

The first three examples are:
- **5201** sphere-sphere geometric contact observation.
- **5202** frictionless translational normal-impulse response for a supplied active contact.
- **5203** prescribed kinematic motion evaluation and achieved-state commit validation.

They are deliberately separate capabilities. They do not collectively constitute a rigid-body solver or world-step loop.

## Teaching and laboratory pipeline

A professor-facing chapter should link to a manifest, derivation, implementation, tests, and at least one reproducible experiment. A laboratory experience should select the same capability and model versions; it must not implement a second hidden version of the physics. Experiment run records should retain input values, units, coordinate/time conventions, model identity, numerical settings, output samples, and validation evidence.

The manifest makes the teaching links discoverable; it does not replace the textbook chapter or the experiment's run record.
