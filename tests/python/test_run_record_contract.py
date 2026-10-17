"""Regression tests for the shared physics run-record envelope and catalog."""
from __future__ import annotations

import importlib.util
import json
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def load_tool(name: str, filename: str):
    spec = importlib.util.spec_from_file_location(name, ROOT / "tools" / filename)
    module = importlib.util.module_from_spec(spec)
    assert spec and spec.loader
    spec.loader.exec_module(module)
    return module


validator = load_tool("physics_record_validator", "validate_physics_run_records.py")
catalog = load_tool("physics_record_catalog", "catalog_physics_run_records.py")


class RunRecordEnvelopeTests(unittest.TestCase):
    def write_record(self, record: object) -> Path:
        temp = tempfile.NamedTemporaryFile(mode="w", encoding="utf-8", suffix=".json", delete=False)
        with temp:
            json.dump(record, temp)
        self.addCleanup(Path(temp.name).unlink, missing_ok=True)
        return Path(temp.name)

    def test_representative_records_validate_across_three_domains(self):
        paths = [
            ROOT / "docs/experiments/acoustic-observation.run.json",
            ROOT / "docs/experiments/first-order-room-reflection.run.json",
            ROOT / "docs/experiments/electrical-thermal-coupling.run.json",
        ]
        for path in paths:
            with self.subTest(path=path.name):
                self.assertEqual(validator.validate(path), [])

    def test_boolean_is_not_a_valid_schema_version_or_model_version(self):
        record = {
            "schemaVersion": True,
            "modelId": "test.model",
            "modelVersion": True,
            "status": "computed",
            "definition": {},
            "result": {},
            "limitations": ["test fixture"],
        }
        errors = validator.validate(self.write_record(record))
        self.assertTrue(any("schemaVersion must be a positive integer" in e for e in errors))
        self.assertTrue(any("modelVersion must be a positive integer" in e for e in errors))

    def test_unknown_status_and_empty_limitations_are_rejected(self):
        record = {
            "schemaVersion": 1,
            "modelId": "test.model",
            "modelVersion": 1,
            "status": "probably-real",
            "definition": {},
            "result": {},
            "limitations": [""],
        }
        errors = validator.validate(self.write_record(record))
        self.assertTrue(any("recognized epistemic status" in e for e in errors))
        self.assertTrue(any("limitations must be an array" in e for e in errors))

    def test_catalog_discovers_shared_fields_without_rewriting_domain_payloads(self):
        paths = [
            ROOT / "docs/experiments/acoustic-observation.run.json",
            ROOT / "docs/experiments/first-order-room-reflection.run.json",
            ROOT / "docs/experiments/electrical-thermal-coupling.run.json",
        ]
        entries = [catalog.load_record(path) for path in paths]
        rendered = catalog.render(entries)
        self.assertIn("`observation`", rendered)
        self.assertIn("`directObservation`, `reflections`", rendered)
        self.assertIn("`result`, `electricalObservation`", rendered)
        self.assertIn("./acoustic-observation.run.json", rendered)
        self.assertIn("not a universal physics result schema", rendered)


if __name__ == "__main__":
    unittest.main()
