# Physics Pages client

This directory is the first static WebApp-style client for the Physics repository. It is designed for GitHub Pages hosting and does not require ServerApp.

## Run locally

Serve this directory over HTTP (for example, `python3 -m http.server 8000 --directory site`) and open `http://localhost:8000`. A local HTTP server is required because the client fetches the run-record JSON.

## First vertical slice

- **Atlas:** a small set of domain entry points with explicit implementation-status language.
- **Projectile experience:** a recorded 3D point-projectile trajectory from the actual Physics generator.
- **Evidence inspector:** selected sample state and the complete recorded sample table.
- **Navigation FSM:** `HOME`, `PROJECTILE`, and `INSPECTOR`, with semantic transitions and hash-based browser navigation.
- **No browser-side solver:** the chart and inspector read the checked-in JSON record. They do not independently calculate physical outcomes.

## Evidence contract

The Pages copy at `data/projectile-trajectory.run.json` must remain structurally equal to `../docs/experiments/projectile-trajectory.run.json` and the output of `tools/ProjectileRunRecordGenerator`. The Pages workflow verifies all three on every relevant push and pull request.

The current record has no unique run ID or source-revision field. The client does not fabricate these values. Add them only through a deliberate, versioned provenance contract.

## Deployment

`.github/workflows/physics-pages.yml` builds and validates the static artifact on development changes and pull requests. Deployment occurs only when the workflow is manually dispatched against `development`. No automatic deployment from `master` is configured.

The first release should wait for successful CI, browser validation, confirmation of the repository's Pages settings, and explicit deployment authorization.
