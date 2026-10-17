#!/usr/bin/env python3
"""Render the actual C# free-field acoustic distance-sweep run record as SVG."""
import argparse
import html
import json
import math
from pathlib import Path


def finite(value, label):
    if not isinstance(value, (int, float)) or not math.isfinite(value):
        raise ValueError(f"{label} must be finite")
    return float(value)


def render(record):
    if record.get("schemaVersion") != 1 or record.get("modelId") != "acoustics.free-field-distance-sweep":
        raise ValueError("unsupported acoustic distance-sweep record")
    observations = record.get("observations")
    if not isinstance(observations, list) or len(observations) < 2:
        raise ValueError("distance sweep must contain at least two observations")
    points = []
    for item in observations:
        points.append({
            "distance": finite(item["distanceMeters"], "distance"),
            "level": finite(item["receivedLevelDbSpl"], "received level"),
            "delay": finite(item["propagationDelaySeconds"], "propagation delay"),
            "id": html.escape(str(item["listenerId"]), quote=True),
        })
    if any(p["distance"] < 0 or p["delay"] < 0 for p in points):
        raise ValueError("distance and delay must be non-negative")
    points.sort(key=lambda p: p["distance"])
    min_x = min(0.0, points[0]["distance"])
    max_x = max(p["distance"] for p in points)
    min_y = min(p["level"] for p in points) - 4
    max_y = max(p["level"] for p in points) + 4
    px = lambda x: 90 + (x - min_x) / max(max_x - min_x, 1) * 560
    py = lambda y: 290 - (y - min_y) / max(max_y - min_y, 1) * 190
    polyline = " ".join(f"{px(p['distance']):.2f},{py(p['level']):.2f}" for p in points)
    dots = []
    rows = []
    for index, p in enumerate(points):
        x, y = px(p["distance"]), py(p["level"])
        dots.append(
            f'<circle cx="{x:.2f}" cy="{y:.2f}" r="6" fill="#8de8ff" stroke="#e3fbff" stroke-width="2"/>'
            f'<text x="{x:.2f}" y="{y - 12:.2f}" text-anchor="middle" fill="#dff8ff" font-family="sans-serif" font-size="12">{p["level"]:.2f} dB</text>'
        )
        rows.append(
            f'<text x="390" y="{340 + index * 20}" fill="#e4edf4" font-family="sans-serif" font-size="13">'
            f'{p["distance"]:g} m · {p["level"]:.2f} dB SPL · {p["delay"] * 1000:.2f} ms</text>'
        )
    grid = []
    for level in range(math.ceil(min_y / 5) * 5, math.floor(max_y / 5) * 5 + 1, 5):
        y = py(level)
        grid.append(
            f'<line x1="90" y1="{y:.2f}" x2="650" y2="{y:.2f}" stroke="#334452"/>'
            f'<text x="78" y="{y + 4:.2f}" text-anchor="end" fill="#94a6b6" font-family="sans-serif" font-size="11">{level}</text>'
        )
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="720" height="440" viewBox="0 0 720 440" role="img" aria-labelledby="title desc">
  <title id="title">Free-field acoustic level by distance</title>
  <desc id="desc">A plot generated from observations computed by FreeFieldAcoustics. It is analytical propagation metadata, not synthesized sound.</desc>
  <rect width="720" height="440" rx="18" fill="#101923"/>
  <text x="28" y="36" fill="#8de8ff" font-family="sans-serif" font-size="18" font-weight="700">SINGULARITY STUDIOS · CONTROLLED DISTANCE SWEEP</text>
  <text x="28" y="57" fill="#94a6b6" font-family="sans-serif" font-size="12">Actual FreeFieldAcoustics model output · reference 80 dB SPL at 1 m</text>
  {''.join(grid)}
  <line x1="90" y1="100" x2="90" y2="290" stroke="#637887"/>
  <line x1="90" y1="290" x2="650" y2="290" stroke="#637887"/>
  <polyline points="{polyline}" fill="none" stroke="#8de8ff" stroke-width="3"/>
  {''.join(dots)}
  <text x="370" y="318" text-anchor="middle" fill="#e4edf4" font-family="sans-serif" font-size="12">Distance from point source (metres)</text>
  {''.join(rows)}
  <text x="28" y="422" fill="#94a6b6" font-family="sans-serif" font-size="11">Ideal free field only. No room response, waveform synthesis, occlusion, or Doppler.</text>
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
        print(f"{args.output} matches the acoustic distance-sweep record.")
        return
    args.output.write_text(actual, encoding="utf-8")
    print(f"Wrote acoustic distance-sweep SVG: {args.output}")


if __name__ == "__main__":
    main()
