#!/usr/bin/env python3
"""Validate the versioned Physics inclined-plane run record, including vector mechanics."""
import json
import math
import os
import sys
from pathlib import Path

record = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8"))
definition = record["definition"]
result = record["result"]
vectors = result["forceVectors"]

assert record["modelId"] == "mechanics.frictionless-inclined-plane"
assert record["modelVersion"] == "2"
assert record["schemaVersion"] == 2
assert record["forceVectorFrame"] == (
    "local incline frame: +X horizontal toward foot of incline; "
    "+Y vertically upward; +Z out of incline plane"
)
assert record["units"]["force"] == "N"
assert record["units"]["acceleration"] == "m/s^2"

mass = definition["massKilograms"]
angle = definition["angleRadians"]
gravity = definition["gravitationalAccelerationMetersPerSecondSquared"]
drop = definition["verticalDropMeters"]
sine = math.sin(angle)
cosine = math.cos(angle)

def close(actual, expected, label):
    assert math.isclose(actual, expected, rel_tol=0, abs_tol=1e-12), (
        label, actual, expected
    )

def vector(name):
    value = vectors[name]
    return (value["x"], value["y"], value["z"])

def add(left, right):
    return tuple(a + b for a, b in zip(left, right))

def magnitude(value):
    return math.sqrt(sum(component * component for component in value))

weight = vector("weightNewtons")
parallel = vector("parallelGravityNewtons")
perpendicular = vector("perpendicularGravityNewtons")
normal = vector("normalForceNewtons")
acceleration = vector("accelerationMetersPerSecondSquared")

close(weight[0], 0.0, "weight.x")
close(weight[1], -mass * gravity, "weight.y")
close(weight[2], 0.0, "weight.z")

for index, axis in enumerate(("x", "y", "z")):
    close(add(parallel, perpendicular)[index], weight[index], f"gravity decomposition {axis}")
    close(add(perpendicular, normal)[index], 0.0, f"normal balance {axis}")

expected_parallel_magnitude = mass * gravity * sine
expected_normal_magnitude = mass * gravity * cosine
expected_acceleration = gravity * sine
close(magnitude(parallel), expected_parallel_magnitude, "parallel force magnitude")
close(magnitude(perpendicular), expected_normal_magnitude, "perpendicular gravity magnitude")
close(magnitude(normal), expected_normal_magnitude, "normal force magnitude")
close(magnitude(acceleration), expected_acceleration, "acceleration magnitude")
close(parallel[0], expected_parallel_magnitude * cosine, "parallel.x")
close(parallel[1], -expected_parallel_magnitude * sine, "parallel.y")
close(normal[0], expected_normal_magnitude * sine, "normal.x")
close(normal[1], expected_normal_magnitude * cosine, "normal.y")
close(acceleration[0], expected_acceleration * cosine, "acceleration.x")
close(acceleration[1], -expected_acceleration * sine, "acceleration.y")

expected_scalars = {
    "pathLengthMeters": drop / sine,
    "accelerationMetersPerSecondSquared": expected_acceleration,
    "parallelGravityForceNewtons": expected_parallel_magnitude,
    "normalForceNewtons": expected_normal_magnitude,
    "travelTimeSeconds": math.sqrt(2 * drop / gravity) / sine,
    "finalSpeedMetersPerSecond": math.sqrt(2 * gravity * drop),
}
for key, expected in expected_scalars.items():
    close(result[key], expected, key)

source_revision = os.environ.get("PHYSICS_SOURCE_REVISION")
if source_revision:
    assert record["provenance"]["sourceRevision"] == source_revision

print("PASS: scalar solution, SI units, declared vector frame, weight decomposition, normal reaction, acceleration vector, and provenance.")
