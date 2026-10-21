# Operation Sith Branch Reconciliation

**Audit date:** 2026-10-21  
**Canonical branches:** `master`, `development`  
**Current disposition:** Do not manually delete the six non-canonical branches until all unique documentation and workflow changes have been reconciled and the branch inventory is refreshed.

## Branches reviewed

| Branch | Observed difference from `development` | Evidence-based disposition |
|---|---|---|
| `docs/procedural-geometry-data-contract` | Three commits ahead, nine behind; differs in `README.md`, `docs/MATHEMATICS_DOMAIN_MAP.md`, and `docs/PROCEDURAL_GEOMETRY_AND_FRACTAL_DATA.md` | The branch document labels itself a design proposal and its proof sequence predates the implementation now documented on `development`; still review all three file diffs before clearing |
| `feat/mandelbrot-escape-time-2d` | Nine commits ahead, six behind; sampler, tests, guide and UML appear in compare history | Sampler and test blob SHAs match `development`; README and several docs differ relative to the merge base. Review the exact current branch contents and confirm all useful docs/UML are retained |
| `feat/mandelbrot-run-record-exporter` | Four commits ahead, five behind; workflow, exporter project, program and README differ from the merge base | `development` has a parameterized exporter that validates and round-trips the generated record, while this branch has a simpler fixed-default implementation. Workflow still differs; compare workflow purpose and preserve any unique checks before clearing |
| `feat/parameterized-mandelbrot-export` | Three commits ahead, four behind; workflow, exporter program and README differ from the merge base | Current development exporter contains parameter parsing and explicit work/bounds limits. Review README/workflow changes to confirm no useful checks or instructions are lost |
| `feat/seeded-point-set-2d` | Nine commits ahead, eight behind; implementation, tests, UML, README and domain docs appear in compare history | Implementation and test blob SHAs match `development`; review UML and documentation differences against the current branch tree before clearing |
| `feat/seeded-point-set-run-record` | Seven commits ahead, seven behind; workflow, README, project file, domain docs and exporter files differ from merge base | Exporter source is present on `development`; inspect project reference and workflow differences to confirm no build/test contract is unique |

## Full compare snapshots (2026-10-21)

Compared each non-canonical branch against `development` using GitHub's compare endpoint. Compare counts are relative to the merge base, not a claim that every listed file is currently missing from `development`.

- `docs/procedural-geometry-data-contract`: 3 ahead / 9 behind; 3 changed paths.
- `feat/mandelbrot-escape-time-2d`: 9 ahead / 6 behind; 9 changed paths in compare history.
- `feat/mandelbrot-run-record-exporter`: 4 ahead / 5 behind; 4 changed paths.
- `feat/parameterized-mandelbrot-export`: 3 ahead / 4 behind; 3 changed paths.
- `feat/seeded-point-set-2d`: 9 ahead / 8 behind; 8 changed paths.
- `feat/seeded-point-set-run-record`: 7 ahead / 7 behind; 7 changed paths.

These counts describe divergent histories. The current `development` tree already contains the core sampler and exporter implementations, but workflow and documentation differences still need explicit disposition. Do not interpret a compare file marked `added` relative to the merge base as proof it is absent from `development`.

## CI evidence

The following recent `development` runs completed successfully:

- [Parameterized Mandelbrot run-record generation — 39675344162](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39675344162)
- [Mandelbrot exporter — 39674140147](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39674140147)
- [Bounded Mandelbrot sampler — 39673643986](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39673643986)
- [Seeded point-set run-record export — 39670687016](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39670687016)
- [Bounded deterministic 2D point sampling — 39668611183](https://github.com/TrentBest/TheSingularityWorkshop.Physics/actions/runs/39668611183)

## Cleanup gate

This is an evidence ledger, not authorization to delete branches. Before manual deletion, finish reviewing each branch's complete file list and workflow diff, preserve any genuinely unique content on `development`, and refresh the branch inventory. No branch should be considered cleared solely because selected source files have matching blob SHAs.
