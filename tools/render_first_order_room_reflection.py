#!/usr/bin/env python3
"""Render first-order room-reflection metadata as a source-derived SVG."""
import argparse
import json
import math
from pathlib import Path


def finite(value, label):
    if not isinstance(value, (int, float)) or not math.isfinite(value):
        raise ValueError(f"{label} must be finite")
    return float(value)


def render(record):
    if record.get("schemaVersion") != 1 or record.get("modelId") != "acoustics.first-order-room-reflection":
        raise ValueError("unsupported first-order room-reflection record")
    direct = record.get("directObservation")
    reflections = record.get("reflections")
    if not isinstance(direct, dict) or not isinstance(reflections, list) or len(reflections) != 1:
        raise ValueError("this controlled visualization requires a direct path and exactly one reflected path")
    source = record["source"]["positionMeters"]
    listener = record["listener"]["positionMeters"]
    reflection = reflections[0]
    bounce = reflection["reflectionPointMeters"]
    room_min = record["room"]["minimumMeters"]
    room_max = record["room"]["maximumMeters"]
    if any(finite(room_max[k], f"room max {k}") <= finite(room_min[k], f"room min {k}") for k in ("x", "y", "z")):
        raise ValueError("room bounds must have positive dimensions")
    values = {
        "direct_distance": finite(direct["distanceMeters"], "direct distance"),
        "direct_delay_ms": finite(direct["propagationDelaySeconds"], "direct delay") * 1000,
        "direct_level": finite(direct["receivedLevelDbSpl"], "direct level"),
        "reflection_distance": finite(reflection["pathLengthMeters"], "reflection path length"),
        "reflection_delay_ms": finite(reflection["propagationDelaySeconds"], "reflection delay") * 1000,
        "reflection_level": finite(reflection["receivedLevelDbSpl"], "reflection level"),
        "coefficient": finite(reflection["reflectionCoefficient"], "reflection coefficient"),
    }
    for point_name, point in (("source", source), ("listener", listener), ("reflection point", bounce)):
        for axis in ("x", "y", "z"):
            finite(point[axis], f"{point_name} {axis}")
    if values["direct_distance"] <= 0 or values["reflection_distance"] <= 0:
        raise ValueError("path lengths must be positive")
    if not 0 < values["coefficient"] <= 1:
        raise ValueError("reflection coefficient must be in (0, 1]")

    # Top-down X/Y plan at the fixed scenario height.
    def px(x):
        return 90 + (finite(x, "x") - finite(room_min["x"], "room min x")) / (finite(room_max["x"], "room max x") - finite(room_min["x"], "room min x")) * 500
    def py(y):
        return 335 - (finite(y, "y") - finite(room_min["y"], "room min y")) / (finite(room_max["y"], "room max y") - finite(room_min["y"], "room min y")) * 250

    sx, sy = px(source["x"]), py(source["y"])
    lx, ly = px(listener["x"]), py(listener["y"])
    bx, by = px(bounce["x"]), py(bounce["y"])
    width = finite(room_max["x"], "room max x") - finite(room_min["x"], "room min x")
    height = finite(room_max["y"], "room max y") - finite(room_min["y"], "room min y")
    # Coordinate grid and room wall are derived from the same declared room bounds.
    grid = []
    for index in range(1, 10):
        gx = 90 + index * 50
        gy = 335 - index * 25
        grid.append(f'<line x1="{gx:.2f}" y1="85" x2="{gx:.2f}" y2="335" stroke="#263947"/>')
        grid.append(f'<line x1="90" y1="{gy:.2f}" x2="590" y2="{gy:.2f}" stroke="#263947"/>')
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="720" height="470" viewBox="0 0 720 470" role="img" aria-labelledby="title desc">
  <title id="title">First-order room reflection path</title>
  <desc id="desc">Source-derived top-down view of one direct path and one first-order reflection from the maximum-X wall. Path levels are separate estimates, not combined audio.</desc>
  <rect width="720" height="470" rx="18" fill="#101923"/>
  <text x="28" y="34" fill="#8de8ff" font-family="sans-serif" font-size="17" font-weight="700">SINGULARITY STUDIOS · FIRST-ORDER ROOM PATH</text>
  <text x="28" y="55" fill="#a6b8c7" font-family="sans-serif" font-size="12">C# Physics model output · 10 m × 10 m room plan at z = {source["z"]:g} m</text>
  <rect x="90" y="85" width="500" height="250" fill="#152633" stroke="#90a4b5" stroke-width="2"/>
  {''.join(grid)}
  <line x1="590" y1="85" x2="590" y2="335" stroke="#ffbf69" stroke-width="6"/>
  <text x="578" y="76" text-anchor="end" fill="#ffcf8a" font-family="sans-serif" font-size="11">Reflecting wall · x-max</text>
  <line x1="{sx:.2f}" y1="{sy:.2f}" x2="{lx:.2f}" y2="{ly:.2f}" stroke="#8de8ff" stroke-width="3"/>
  <polyline points="{sx:.2f},{sy:.2f} {bx:.2f},{by:.2f} {lx:.2f},{ly:.2f}" fill="none" stroke="#ffbf69" stroke-width="3" stroke-dasharray="8 5"/>
  <circle cx="{sx:.2f}" cy="{sy:.2f}" r="7" fill="#8de8ff" stroke="#eafaff" stroke-width="2"/>
  <circle cx="{lx:.2f}" cy="{ly:.2f}" r="7" fill="#b8f7c4" stroke="#f1fff4" stroke-width="2"/>
  <rect x="{bx-5:.2f}" y="{by-5:.2f}" width="10" height="10" fill="#ffbf69" stroke="#fff0d5" stroke-width="1.5"/>
  <text x="{sx+10:.2f}" y="{sy+18:.2f}" fill="#dff8ff" font-family="sans-serif" font-size="12">Source (2, 5)</text>
  <text x="{lx-10:.2f}" y="{ly-13:.2f}" text-anchor="end" fill="#d6ffe0" font-family="sans-serif" font-size="12">Listener (8, 7)</text>
  <text x="{bx-10:.2f}" y="{by+22:.2f}" text-anchor="end" fill="#ffcf8a" font-family="sans-serif" font-size="12">Bounce (10, 6.6)</text>
  <line x1="28" y1="365" x2="64" y2="365" stroke="#8de8ff" stroke-width="3"/>
  <text x="73" y="369" fill="#e4edf4" font-family="sans-serif" font-size="12">Direct path · {values["direct_distance"]:.3f} m · {values["direct_delay_ms"]:.2f} ms · {values["direct_level"]:.2f} dB SPL</text>
  <line x1="28" y1="390" x2="64" y2="390" stroke="#ffbf69" stroke-width="3" stroke-dasharray="8 5"/>
  <text x="73" y="394" fill="#e4edf4" font-family="sans-serif" font-size="12">One reflection · {values["reflection_distance"]:.3f} m · {values["reflection_delay_ms"]:.2f} ms · {values["reflection_level"]:.2f} dB SPL</text>
  <text x="28" y="425" fill="#a6b8c7" font-family="sans-serif" font-size="11">Wall pressure-amplitude coefficient: {values["coefficient"]:.2f}. Per-path estimates are not summed.</text>
  <text x="28" y="447" fill="#a6b8c7" font-family="sans-serif" font-size="11">No waveform, phase/interference, frequency-dependent absorption, or multiple reflections.</text>
</svg>
'''


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("record", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    actual = render(json.loads(args.record.read_text(encoding="utf-8")))
    if args.check:
        if args.output.read_text(encoding="utf-8") != actual:
            raise SystemExit(f"{args.output} differs from SVG rendered from {args.record}")
        print(f"{args.output} matches the first-order room-reflection record.")
        return
    args.output.write_text(actual, encoding="utf-8")
    print(f"Wrote first-order room-reflection SVG: {args.output}")


if __name__ == "__main__":
    main()
