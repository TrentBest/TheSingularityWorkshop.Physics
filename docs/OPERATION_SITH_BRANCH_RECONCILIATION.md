# Operation Sith Branch Reconciliation

**Audit date:** 2026-10-21  
**Canonical branches:** `master`, `development`  
**Current disposition:** Do not manually delete the six non-canonical branches until the remaining branch-specific workflow and documentation differences have been reviewed.

## Branches reviewed

| Branch | Observed difference from `development` | Evidence-based disposition |
|---|---|---|
| `docs/procedural-geometry-data-contract` | Adds/edits procedural geometry/fractal documentation; the same document exists on `development` with a different, newer-looking content revision | Compare full meaning and retain any unique details; not yet cleared |
| `feat/mandelbrot-escape-time-2d` | Adds sampler, tests, guide and UML | Implementation and test file blob SHAs match `development`; remaining docs/branch history still require review |
| `feat/mandelbrot-run-record-exporter` | Adds exporter and workflow steps | Exporter implementation is present on `development); workflow has diverged and needs final comparison |
| `feat/parameterized-mandelbrot-export` | Parameterized exporter/workflow changes | Exporter program blob SHA matches `development); workflow/documentation differences still need review |
| `feat/seeded-point-set-2d` | Adds deterministic point-set implementation, tests and UML | Implementation blob SHA matches `development); remaining docs/UML coverage still require review |
| `feat/seeded-point-set-run-record` | Adds seeded-point-set exporter and workflow steps | Exporter program blob SHA matches `development); workflow/documentation differences still need review |

## CI evidence

The following recent `development` runs completed successfully:

- [Parameterized Mandelbrot run-record generation — 39675344162](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39675344162)
- [Mandelbrot exporter — 39674140147](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39674140147)
- [Bounded Mandelbrot sampler — 39673643986](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39673643986)
- [Seeded point-set run-record export — 39670687016](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39670687016)
- [Bounded deterministic 2D point sampling — 39668611183](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39668611183)

## Cleanup gate

This is an evidence ledger, not authorization to delete branches. Before manual deletion, finish reviewing each branch's complete file list and workflow diff, preserve any genuinely unique content on `development), and refresh the branch inventory. No branch should be considered cleared solely because selected source files have matching blob SHAs.
