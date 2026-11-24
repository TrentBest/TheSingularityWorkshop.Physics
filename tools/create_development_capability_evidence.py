#!/usr/bin/env python3
"""Create a run-scoped, non-release development-capability evidence envelope for CiAi."""

from __future__ import annotations

import hashlib
import json
import os
import re
import shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path.cwd()
OUT = ROOT / "TestResults" / "development-capability"
OUT.mkdir(parents=True, exist_ok=True)

def env(name: str, default: str = "") -> str:
    return os.environ.get(name, default)

def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()

source_sha = env("PHYSICS_SOURCE_SHA")
if not re.fullmatch(r"[0-9a-f]{40}", source_sha):
    raise SystemExit("PHYSICS_SOURCE_SHA must be a full lowercase 40-character commit SHA.")

try:
    step_outcomes = json.loads(env("PHYSICS_STEP_OUTCOMES_JSON") or "{}")
    pull_request = json.loads(env("PHYSICS_PULL_REQUEST_JSON") or "null")
    dependencies = json.loads(env("PHYSICS_DEPENDENCY_CHECKOUTS_JSON") or "[]")
except json.JSONDecodeError as exception:
    raise SystemExit(f"Invalid JSON workflow evidence input: {exception}") from exception

required_steps = [
    "validate_microbundles",
    "build_microbundles",
    "test_microbundles",
    "package_microbundles",
    "test_physics_solution",
    "package_core",
    "package_adapter",
]
required_step_outcomes = {name: step_outcomes.get(name, "skipped") for name in required_steps}

source_files = [
    "Physics/Kinematics/PrescribedKinematicMotion.cs",
    "TheSingularityWorkshop.Physics.MicroBundles.csproj",
]
source_file_evidence = []
for name in source_files:
    path = ROOT / name
    if path.is_file():
        source_file_evidence.append({"path": name, "sha256": sha256(path), "sizeBytes": path.stat().st_size})

expected_files = [
    ("microbundle", Path("artifacts/physics-prescribed-kinematic-motion.mbundle"), "physics-prescribed-kinematic-motion.mbundle"),
    ("microbundleTests", Path("TestResults/MicroBundles/physics-microbundles.trx"), "physics-microbundles.trx"),
    ("physicsTests", Path("TestResults/Physics/physics-tests.trx"), "physics-tests.trx"),
    ("workflowProvenance", Path("TestResults/provenance/workflow-provenance.json"), "provenance.json"),
]
artifacts = []
for kind, relative, archive_name in expected_files:
    source = ROOT / relative
    if source.is_file():
        destination = OUT / archive_name
        shutil.copy2(source, destination)
        artifacts.append({
            "kind": kind,
            "path": archive_name,
            "sha256": sha256(destination),
            "sizeBytes": destination.stat().st_size,
        })

present_kinds = {item["kind"] for item in artifacts}
required_artifact_kinds = {"microbundle", "microbundleTests", "physicsTests", "workflowProvenance"}
artifacts_complete = required_artifact_kinds.issubset(present_kinds)
steps_passed = all(required_step_outcomes[name] == "success" for name in required_steps)
steps_failed = any(required_step_outcomes[name] == "failure" for name in required_steps)
checkout_sha = env("PHYSICS_CHECKOUT_SHA")
source_ref = env("PHYSICS_SOURCE_REF")
checkout_ref = env("PHYSICS_CHECKOUT_REF")
workflow_sha = env("PHYSICS_WORKFLOW_SHA")
run_id = int(env("PHYSICS_RUN_ID", "0"))
run_attempt = int(env("PHYSICS_RUN_ATTEMPT", "0"))
identity_valid = (
    checkout_sha == source_sha
    and bool(source_ref)
    and checkout_ref == source_ref
    and re.fullmatch(r"[0-9a-fA-F]{40}", workflow_sha) is not None
    and run_id > 0
    and run_attempt > 0
    and len(dependencies) == 2
    and all(re.fullmatch(r"[0-9a-fA-F]{40}", item.get("checkoutSha", "")) for item in dependencies)
)
status = "satisfied" if steps_passed and artifacts_complete and identity_valid else ("blocked" if steps_failed or not identity_valid else "incomplete")

workflow_path = ".github/workflows/build.yml"
workflow = {
    "path": workflow_path,
    "ref": env("PHYSICS_WORKFLOW_REF"),
    "sha": env("PHYSICS_WORKFLOW_SHA"),
    "runId": run_id,
    "runAttempt": run_attempt,
    "eventName": env("PHYSICS_EVENT_NAME"),
    "eventRef": env("PHYSICS_EVENT_REF"),
    "eventSha": env("PHYSICS_EVENT_SHA"),
}
checkout = {
    "ref": checkout_ref,
    "sha": checkout_sha,
    "dependencies": dependencies,
}
evidence = {
    "schema": "ciai.development-capability-evidence.v1",
    "evidenceContract": "ciai.development-capability-evidence.v1",
    "providerRepository": env("PHYSICS_REPOSITORY"),
    "artifactName": "physics-development-capability-" + env("PHYSICS_RUN_ID") + "-" + env("PHYSICS_RUN_ATTEMPT"),
    "capability": {
        "id": "physics.kinematics.prescribed-motion.v1",
        "version": "1",
        "channel": "development",
        "status": status,
        "implementationPaths": source_files,
        "validationProjects": [
            "tests/MicroBundles/TheSingularityWorkshop.Physics.MicroBundles.Tests.csproj",
            "TheSingularityWorkshop.Physics.sln",
        ],
    },
    "candidate": {
        "id": f"{env('PHYSICS_REPOSITORY')}@{source_sha}",
        "sourceRef": source_ref,
        "sourceSha": source_sha,
    },
    "workflow": workflow,
    "checkout": checkout,
    "pullRequest": pull_request,
    "requiredJobs": [{"name": "test", "workflowPath": workflow_path}],
    "requiredStepIds": required_steps,
    "stepOutcomes": required_step_outcomes,
    "requiredArtifacts": sorted(required_artifact_kinds),
    "artifactsComplete": artifacts_complete,
    "identityValid": identity_valid,
    "artifacts": artifacts,
    "sourceFiles": source_file_evidence,
    "microBundle": {
        "bundleId": 5203,
        "version": "0.1.0-alpha.1",
        "file": "physics-prescribed-kinematic-motion.mbundle",
        "isNuGetPackage": False,
        "isPublished": False,
    },
    "policy": {"id": "physics.development-capability", "version": 1},
    "observedAtUtc": datetime.now(timezone.utc).isoformat(),
    "satisfied": status == "satisfied",
}

path = OUT / "evidence.json"
path.write_text(json.dumps(evidence, indent=2, sort_keys=True) + "\n", encoding="utf-8")
print(json.dumps({
    "path": str(path),
    "status": status,
    "sourceSha": source_sha,
    "stepOutcomes": required_step_outcomes,
    "artifactsComplete": artifacts_complete,
    "artifactCount": len(artifacts),
}, indent=2))

if status != "satisfied":
    print("Development capability evidence is not satisfied; the workflow must fail closed.")
    raise SystemExit(1)
