# Cross-LLM Coordination Log

This file is an append-only coordination record maintained from the Physics repository. Each entry records integration intent and boundaries; it does not claim that a capability exists until code and tests prove it.

## 2026-10-16 — Operation Sith: single active branch and preservation before cleanup

**Priority:** branch consolidation before further feature expansion.  
**Active integration branch:** `development`.  
**Stable branch:** `master`.  
**Policy:** do not create additional feature/work branches for this effort. Carry one active work branch only. Keep `main` and the existing `work/*` refs until their unique work has been classified and either preserved on `development`, deliberately rebuilt, or explicitly identified as disposable. This inventory is not permission to delete branches and is not evidence that their contents have been integrated.

### Branch audit

The GitHub branch search returned 25 refs: `development`, `master`, `main`, and 22 `work/*` branches. Comparisons against `development` show that `main` and `master` are each 157 commits behind the current development head. The work branches are diverged from development, with non-zero commits and/or distinct files; they cannot be assumed safe to delete merely because their names are temporary.

### Unique work that must be classified

- **Spatial and mechanics foundation:** `work/spatial-primitives`, `work/spatial-orientation`, `work/rigid-body-state`, `work/spatial-spring-damper`, `work/contact-observation-contract`, `work/sphere-sphere-narrowphase`, `work/sphere-normal-impulse-response`, `work/contact-positional-correction`, `work/axial-spring-damper`, and `work/axial-link-failure`. These contain overlapping stacks of vector/quaternion/pose/body-state, spring-damper, contact, impulse, positional-correction, and failure-monitor work. Integrate by dependency and tests, not by blindly merging each branch head.
- **Water and terrain:** `work/hydrodynamics-linear-wave`, `work/hydrodynamics-shallow-water-2d`, `work/hydrodynamics-bathymetry`, `work/hydrodynamics-wet-dry`, `work/volcanic-terrain-source`, and `work/stellar-physics-roadmap`. These overlap and stack wave, shallow-water, bathymetry, wet/dry shoreline, geology, domain-map, and roadmap material. Preserve the newest complete, validated slice for each capability and retain its tests and scope/limitation docs.
- **Domain planning and thermal state:** `work/physics-domain-map` and `work/lumped-thermal-state`. Preserve the domain atlas and the thermal primitive/test/documentation set if they remain part of the agreed Physics scope.
- **Electricity:** `work/electrical-grid-loads` contains an electrical grid model, tests, and a domain document.
- **Acoustics:** `work/acoustic-world-model` contains the listener-centric acoustic world-model document; `work/acoustic-free-field` includes an executable free-field acoustics baseline and tests as well as accumulated water/terrain files. The acoustics PRs are separate reviewable units; avoid treating the combined branch as one indivisible feature.
- **World-scale planning:** `work/stellar-physics-roadmap` also carries the stellar/planetary-energy roadmap; retain it only after reconciling the overlapping terrain/hydrodynamics stack.

### Open review queue observed during this audit

The repository has open draft PRs representing stacked increments, including PRs #3–#10 for spatial/mechanics/contact foundations, #11–#12 for the physics-domain map and thermal state, #13–#19 for wave/water/terrain, and #20–#21 for acoustics. Their bodies describe prerequisites and test scope. These PRs are evidence of review history and proposed changes, not proof that all work is merged or that every head is current. Before retiring refs, inspect each PR's latest head, checks, base, and dependency order; avoid merging stacked PRs in a way that duplicates or bypasses prerequisites.

### Sith consolidation sequence

1. Freeze branch creation: continue integration directly on `development`; do not open another work branch for convenience.
2. Preserve the branch/PR inventory and current documentation before any cleanup.
3. For each domain, compare the branch's unique file changes and commits against current `development`; inspect actual file contents and latest CI results. Branches that contain overlapping stacks must be consolidated in dependency order, with conflict resolution and tests.
4. Keep a capability only when its code, tests, documentation, and limitations agree. If a branch is stale or redundant, record why its unique work is already preserved or intentionally discarded before recommending deletion.
5. Reconcile `development` against `master` only through the normal reviewed promotion path. Do not force-reset the stable branch or merge all of development into master merely to make branch names align.
6. Once the preservation audit is complete, report the exact remaining refs and any unresolved risk to the repository owner. Branch deletion is a separate explicit cleanup action; this log does not perform or authorize it.

### Guardrails

- Do not trade away source, tests, docs, PR context, or useful implementation just to reduce the branch count quickly.
- Do not merge all diverged branch heads wholesale; repeated files and stacked ancestry make duplicate/conflicting changes likely.
- Do not create a replacement feature branch or a succession of temporary branches. `development` is the one active integration branch for this operation.
- Do not release, publish, deploy, or alter WebPage source as part of this branch audit.
- Castle Siege / payload / LUT documents are design intent only. The eventual relocation of that experience to an Experiences repository is a later operation; do not widen this cleanup into that migration.

This audit is based on live branch comparisons and the open-PR listing inspected on 2026-10-16. Branches were not deleted and no PR was merged by this entry.

---

