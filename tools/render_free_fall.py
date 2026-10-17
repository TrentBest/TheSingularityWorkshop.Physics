#!/usr/bin/env python3
"""Render a teaching SVG from recorded ConstantAccelerationRunRecord samples.

The renderer consumes stored position, velocity, and time values. It does not
implement or re-run the equations of motion.
"""
import argparse
import html
import json
import math
from pathlib import Path


def render(record):
    if record.get("schemaVersion") != 1:
        raise ValueError("unsupported or missing run-record schemaVersion")
    definition = record.get("definition", {})
    initial = definition.get("initialState", {})
    samples = record.get("samples")
    if not isinstance(samples, list) or len(samples) < 2:
        raise ValueError("run record must contain at least two samples")
    acceleration = definition.get("acceleration")
    if not isinstance(acceleration, (int, float)) or not math.isfinite(acceleration):
        raise ValueError("record must contain finite acceleration")
    for i, sample in enumerate(samples):
        for key in ("position", "velocity", "elapsedTime"):
            if not isinstance(sample.get(key), (int, float)) or not math.isfinite(sample[key]):
                raise ValueError(f"sample {i} has invalid {key}")
    height0 = initial.get("position")
    if not isinstance(height0, (int, float)) or not math.isfinite(height0) or height0 <= 0:
        raise ValueError("teaching scenario requires a positive initial height")
    times = [s["elapsedTime"] - samples[0]["elapsedTime"] for s in samples]
    heights = [s["position"] for s in samples]
    velocities = [s["velocity"] for s in samples]
    duration = times[-1]
    if duration <= 0:
        raise ValueError("run duration must be positive")
    if min(heights) < -1e-9 or abs(heights[-1]) > 1e-8:
        raise ValueError("this illustration expects the recorded run to end at ground level")

    esc = lambda value: html.escape(str(value), quote=True)
    W, H = 1100, 650
    ground_y = 465
    top_y = 145
    pixels_per_metre = (ground_y - top_y) / height0
    apple_x = 165
    apple_points = [(apple_x, ground_y - h * pixels_per_metre) for h in heights]

    # Two small plots share the exact timestamps and values from the C# run record.
    x0, x1 = 480.0, 1030.0
    ptop, pbottom = 160.0, 300.0
    vtop, vbottom = 365.0, 505.0
    max_height = max(height0, max(heights)) * 1.08
    min_velocity = min(velocities + [acceleration * duration])
    max_velocity = max(0.0, max(velocities))
    if min_velocity == max_velocity:
        min_velocity -= 1
        max_velocity += 1
    sx = lambda t: x0 + t / duration * (x1 - x0)
    sy_height = lambda h: pbottom - h / max_height * (pbottom - ptop)
    sy_velocity = lambda v: vbottom - (v - min_velocity) / (max_velocity - min_velocity) * (vbottom - vtop)
    height_points = [(sx(t), sy_height(h)) for t, h in zip(times, heights)]
    velocity_points = [(sx(t), sy_velocity(v)) for t, v in zip(times, velocities)]
    poly = lambda points: " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
    model = esc(record.get("modelId", "unknown-model"))
    version = esc(record.get("modelVersion", "unknown"))
    dt = definition.get("deltaTime", "unspecified")
    units = record.get("units", {})
    position_unit = esc(units.get("position", "m"))
    velocity_unit = esc(units.get("velocity", "m/s"))
    acceleration_unit = esc(units.get("acceleration", "m/s^2"))

    lines = [
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" role="img" aria-labelledby="title desc">',
        '<title id="title">An apple falls: recorded states become a lesson in motion</title>',
        f'<desc id="desc">A simple tree and an apple shown at every recorded time, beside height-versus-time and velocity-versus-time plots. The {len(samples)} states come from {model}; the drawing and plots read the stored record and do not recompute the physics.</desc>',
        '<rect width="1100" height="650" rx="20" fill="#f7fafc"/>',
        '<text x="40" y="45" font-family="Segoe UI,Arial,sans-serif" font-size="28" font-weight="700" fill="#183047">An apple falls · one model, many ways to see it</text>',
        f'<text x="40" y="76" font-family="Segoe UI,Arial,sans-serif" font-size="15" fill="#526578">Recorded C# states · {len(samples)} samples · {duration:.3f} s · model {model} v{version}</text>',
        '<rect x="28" y="100" width="405" height="475" rx="16" fill="#edf6ed" stroke="#d3e5d2"/>',
        '<text x="52" y="132" font-family="Segoe UI,Arial,sans-serif" font-size="18" font-weight="700" fill="#254b36">The scene</text>',
        # Ground and trunk / canopy are explanatory scenery, not physical geometry.
        '<path d="M45 465 Q120 455 205 465 T420 465 L420 505 L45 505 Z" fill="#a9cf91"/>',
        '<rect x="83" y="235" width="27" height="232" rx="9" fill="#805438"/>',
        '<path d="M95 295 L151 230 M97 330 L53 285 M98 365 L150 315" fill="none" stroke="#805438" stroke-width="12" stroke-linecap="round"/>',
        '<circle cx="83" cy="207" r="55" fill="#69a96b"/><circle cx="135" cy="195" r="62" fill="#7cbb79"/><circle cx="174" cy="231" r="45" fill="#5e9d61"/><circle cx="60" cy="246" r="37" fill="#82c47e"/>',
        '<path d="M200 165 L200 465" stroke="#6f8fa3" stroke-width="1.5" stroke-dasharray="4 5"/>',
        '<text x="215" y="184" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">recorded path</text>',
        f'<polyline points="{poly(apple_points)}" fill="none" stroke="#c65b35" stroke-width="2.5" stroke-dasharray="5 5"/>',
    ]
    for i, (sample, (x, y)) in enumerate(zip(samples, apple_points)):
        radius = 8 if i in (0, len(samples)-1) else 4
        fill = "#c94435" if i in (0, len(samples)-1) else "#e77c4b"
        lines.append(
            f'<circle cx="{x:.2f}" cy="{y:.2f}" r="{radius}" fill="{fill}" stroke="#fff" stroke-width="1.5">'
            f'<title>t={times[i]:.6g} s; height={sample["position"]:.6g} m; velocity={sample["velocity"]:.6g} m/s</title></circle>'
        )
    lines.extend([
        '<text x="52" y="535" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#254b36">Illustrative tree; measured points are the dots.</text>',
        '<text x="52" y="554" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">Tree, apple size and ground art are not collision geometry.</text>',
        '<rect x="455" y="100" width="617" height="475" rx="16" fill="#fff" stroke="#d9e2ea"/>',
        '<text x="480" y="132" font-family="Segoe UI,Arial,sans-serif" font-size="18" font-weight="700" fill="#183047">Read the motion</text>',
        '<text x="480" y="151" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">Dots are stored solver samples; the connecting lines are visual guides.</text>',
        '<text x="480" y="176" font-family="Segoe UI,Arial,sans-serif" font-size="14" font-weight="700" fill="#226b9a">Height above ground (m)</text>',
        f'<rect x="{x0}" y="{ptop}" width="{x1-x0}" height="{pbottom-ptop}" fill="#f8fbfd" stroke="#a9b9c8"/>',
        f'<polyline points="{poly(height_points)}" fill="none" stroke="#226b9a" stroke-width="3" stroke-linejoin="round"/>',
        '<text x="480" y="344" font-family="Segoe UI,Arial,sans-serif" font-size="14" font-weight="700" fill="#b35434">Vertical velocity (m/s)</text>',
        f'<rect x="{x0}" y="{vtop}" width="{x1-x0}" height="{vbottom-vtop}" fill="#fffaf7" stroke="#a9b9c8"/>',
        f'<line x1="{x0}" y1="{sy_velocity(0):.2f}" x2="{x1}" y2="{sy_velocity(0):.2f}" stroke="#c6b8ae" stroke-dasharray="4 4"/>',
        f'<polyline points="{poly(velocity_points)}" fill="none" stroke="#b35434" stroke-width="3" stroke-linejoin="round"/>',
    ])
    for i, (t, h, v) in enumerate(zip(times, heights, velocities)):
        x = sx(t)
        lines.append(f'<circle cx="{x:.2f}" cy="{sy_height(h):.2f}" r="3.5" fill="#226b9a"><title>t={t:.3f} s; height={h:.6g} m</title></circle>')
        lines.append(f'<circle cx="{x:.2f}" cy="{sy_velocity(v):.2f}" r="3.5" fill="#b35434"><title>t={t:.3f} s; velocity={v:.6g} m/s</title></circle>')
    for i in range(6):
        t = duration * i / 5
        x = sx(t)
        lines.append(f'<line x1="{x:.2f}" y1="{vbottom}" x2="{x:.2f}" y2="{vbottom+5}" stroke="#526578"/>')
        lines.append(f'<text x="{x:.2f}" y="526" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="11" fill="#526578">{t:.2f}</text>')
    lines.extend([
        '<text x="755" y="550" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">Elapsed time (s)</text>',
        f'<text x="480" y="596" font-family="Segoe UI,Arial,sans-serif" font-size="15" font-weight="700" fill="#183047">Model: h(t) = h₀ + v₀t + ½at²</text>',
        f'<text x="480" y="620" font-family="Segoe UI,Arial,sans-serif" font-size="14" fill="#526578">v(t) = v₀ + at · a = {acceleration:g} {acceleration_unit} · Δt = {dt} s</text>',
        f'<text x="40" y="615" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#526578">Ideal free fall · no air resistance · constant gravity · {position_unit}, {velocity_unit}</text>',
        '</svg>',
        ''
    ])
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path, help="JSON produced by the C# free-fall generator")
    parser.add_argument("output_svg", type=Path, help="destination SVG path")
    parser.add_argument("--check", action="store_true", help="fail if the existing SVG differs")
    args = parser.parse_args()
    record = json.loads(args.run_record.read_text(encoding="utf-8"))
    generated = render(record)
    if args.check:
        if args.output_svg.read_text(encoding="utf-8") != generated:
            raise SystemExit("SVG is stale: regenerate it from the supplied run record")
        print("SVG matches the supplied run record.")
        return
    args.output_svg.parent.mkdir(parents=True, exist_ok=True)
    args.output_svg.write_text(generated, encoding="utf-8", newline="\n")
    print(f"Rendered {len(record['samples'])} recorded states to {args.output_svg}")


if __name__ == "__main__":
    main()
