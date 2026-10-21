# Operation Sith Branch Reconciliation

**Audit date:** 2026-10-21  
**Canonical branches:** `master`, `development`  
**Current disposition:** All six non-canonical branches are reconciled and ready for the user's manual deletion. Final live inventory was refreshed on 2026-10-21 and the latest ledger CI run (39679952669) passed.

## Branches reviewed

| Branch | Observed difference from `development` | Evidence-based disposition |
|---|---|---|
| `docs/procedural-geometry-data-contract` | Three commits ahead, nine behind; differs in `README.md`, `docs/MATHEMATICS_DOMAIN_MAP.md`, and `docs/PROCEDURAL_GEOMETRY_AND_FRACTAL_DATA.md` | Reviewed all three current file contents. The branch's proposal/status language is superseded by the development documentation, which accurately records the implemented bounded samplers and remaining unimplemented scope. Current development retains the link and expands the contract. **Ready for user manual deletion after a final live inventory check.** |
| `feat/mandelbrot-escape-time-2d` | Nine commits ahead, nine behind; compare reports nine paths relative to merge base | Sampler, focused tests, sampler guide, UML class card, SVG, and UML index all match development. The only meaningful current README difference is that development now links the Gallery and Sith ledger; domain docs match. No branch-only current-tree content remains. **Ready for user manual deletion after final inventory check.** |
| `feat/mandelbrot-run-record-exporter` | Four commits ahead, eight behind; workflow, exporter project, program and README differ from merge base | Project file is identical. The fixed-default exporter program and README are superseded by development's parameterized exporter, which adds invariant-culture parsing, explicit work/bounds limits, and default plus parameterized workflow validation. Branch workflow validates the default record; development retains and strengthens that contract while adding parameterized validation and seeded-point-set checks. No unique current-tree checks/content identified. **Ready for user manual deletion after final inventory check.** |
| `feat/parameterized-mandelbrot-export` | Three commits ahead, four behind; workflow, exporter program and README differ from the merge base | Current workflow and README are identical to development. Development's exporter program extends the fixed-default branch version with optional parameter parsing and explicit work/bounds limits. **Tree-level content reconciled; ready for user manual deletion after a final live inventory check.** |
| `feat/seeded-point-set-2d` | Nine commits ahead, eight behind; implementation, tests, UML, README and domain docs appear in compare history | Implementation, tests, seeded-point-set SVG UML, and geometry foundation guide have matching blob SHAs. The UML index difference is additive on development: it also indexes the already-implemented Mandelbrot UML. **Ready for user manual deletion after a final live inventory check.** |
| `feat/seeded-point-set-run-record` | Seven commits ahead, seven behind; workflow, README, project file, domain docs and exporter files differ from merge base | Current workflow, project file, and exporter implementation are present on development; no branch-only workflow lines remain in the current compare. Development additionally runs the Mandelbrot export and parameterized-record validation. **No unique current tree content identified; ready for user manual deletion after final inventory check.** |

## Full compare snapshots (2026-10-21)

Compared each non-canonical branch against `development` using GitHub's compare endpoint. Compare counts are relative to the merge base, not a claim that every listed file is currently missing from `development`.

- `docs/procedural-geometry-data-contract`: 3 ahead / 9 behind; 3 changed paths.
- `feat/mandelbrot-escape-time-2d`: 9 ahead / 9 behind; 9 changed paths in compare history.
- `feat/mandelbrot-run-record-exporter`: 4 ahead / 8 behind; 4 changed paths.
- `feat/parameterized-mandelbrot-export`: 3 ahead / 4 behind; 3 changed paths.
- `feat/seeded-point-set-2d`: 9 ahead / 8 behind; 8 changed paths.
- `feat/seeded-point-set-run-record`: 7 ahead / 7 behind; 7 changed paths.

These counts describe divergent histories. The current `development` tree contains the sampler, tests, UML, exporter, parameterized inputs, and strengthened validation workflow. The branch-specific content was checked against current development files; no remaining branch-only content was identified. Do not interpret a compare file marked `added` relative to the merge base as proof it is absent from `development`.

## CI evidence

The following recent `development` runs completed successfully:

- [Parameterized Mandelbrot run-record generation — 39675344162](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39675344162)
- [Mandelbrot exporter — 39674140147](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39674140147)
- [Bounded Mandelbrot sampler — 39673643986](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39673643986)
- [Seeded point-set run-record export — 39670687016](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39670687016)
- [Bounded deterministic 2D point sampling — 39668611183](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39668611183)

## Cleanup gate

This is an evidence ledger, not authorization to delete branches. All six branches have now had their compare file lists and the relevant differing current files reviewed. No branch has been deleted by this audit. Final verification complete: live inventory still shows the six reviewed non-canonical branches plus `master` and `development`; latest documentation CI passed. All six are ready for user-led manual deletion. No branch was deleted by this audit.
