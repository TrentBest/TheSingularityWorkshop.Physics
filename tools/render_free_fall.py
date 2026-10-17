#!/usr/bin/env python3
"""Render an educational SVG from recorded constant-acceleration run samples.

The renderer only visualizes the recorded states. It never solves the physics.
"""
import argparse
import html
import json
import math
from pathlib import Path


def render(record):
    definition = record.get("definition", {})
    samples = record.get("samples")
    if record.get("schemaVersion") != 1 or not isinstance(samples, list) or len(samples) != 11:
        raise ValueError("expected schema v1 with 11 recorded free-fall samples")
    initial = definition.get("initialState", {})
    height0 = initial.get("position")
    acceleration = definition.get("acceleration")
    delta_time = definition.get("deltaTime")
    if not all(isinstance(v, (int, float)) and math.isfinite(v) for v in (height0, acceleration, delta_time)):
        raise ValueError("free-fall inputs must be finite numbers")
    if height0 <= 0 or acceleration >= 0 or delta_time <= 0:
        raise ValueError("expected positive initial height, downward acceleration, and positive timestep")
    for i, s in enumerate(samples):
        if not all(isinstance(s.get(k), (int, float)) and math.isfinite(s[k])
                   for k in ("position", "velocity", "elapsedTime")):
            raise ValueError(f"invalid state at sample {i}")
    times = [s["elapsedTime"] - samples[0]["elapsedTime"] for s in samples]
    heights = [s["position"] for s in samples]
    velocities = [s["velocity"] for s in samples]
    duration = times[-1]
    if abs(duration - 1.0) > 1e-10 or abs(heights[-1]) > 1e-8:
        raise ValueError("teaching record must reach ground level at exactly one second")
    esc = lambda s: html.escape(str(s), quote=True)
    x0, x1 = 410.0, 950.0
    ht, hb, vt, vb = 145.0, 275.0, 360.0, 490.0
    sx = lambda t: x0 + t / duration * (x1 - x0)
    sh = lambda h: hb - h / height0 * (hb - ht)
    sv = lambda v: vb - (v / abs(acceleration) + 1.0) * (vb - vt) / 2.0
    hpoints = [(sx(t), sh(h)) for t, h in zip(times, heights)]
    vpoints = [(sx(t), sv(v)) for t, v in zip(times, velocities)]
    poly = lambda pts: " ".join(f"{x:.2f},{y:.2f}" for x, y in pts)
    model = esc(record.get("modelId", "unknown"))
    version = esc(record.get("modelVersion", "unknown"))
    p = [
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 600" role="img" aria-labelledby="title desc">',
        '<title id="title">An apple falls: the motion, recorded and explained</title>',
        f'<desc id="desc">An apple is shown at each of {len(samples)} recorded times beside height-time and velocity-time graphs generated from the same run record. Model {model}; no physics is recomputed by this illustration.</desc>',
        '<rect width="1000" height="600" rx="18" fill="#f7fafc"/>',
        '<text x="32" y="42" font-family="Segoe UI,Arial,sans-serif" font-size="25" font-weight="700" fill="#183047">An apple falls · one model, many ways to see it</text>',
        f'<text x="32" y="70" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#526578">Recorded C# states · {len(samples)} samples · {duration:.2f} s · {model} v{version}</text>',
        '<rect x="24" y="95" width="330" height="450" rx="14" fill="#edf6ed" stroke="#d3e5d2"/>',
        '<text x="45" y="125" font-family="Segoe UI,Arial,sans-serif" font-size="17" font-weight="700" fill="#254b36">The scene</text>',
        '<path d="M35 455 Q100 445 170 455 T340 455 L340 490 L35 490 Z" fill="#a9cf91"/>',
        '<rect x="82" y="235" width="24" height="220" rx="8" fill="#805438"/>',
        '<path d="M94 300 L145 240 M94 332 L53 290 M94 365 L143 322" fill="none" stroke="#805438" stroke-width="10" stroke-linecap="round"/>',
        '<circle cx="82" cy="207" r="47" fill="#69a96b"/><circle cx="128" cy="195" r="53" fill="#7cbb79"/><circle cx="166" cy="228" r="39" fill="#5e9d61"/><circle cx="57" cy="244" r="32" fill="#82c47e"/>',
        '<path d="M200 155 L200 455" stroke="#6f8fa3" stroke-width="1.5" stroke-dasharray="4 5"/>',
        '<text x="211" y="175" font-family="Segoe UI,Arial,sans-serif" font-size="11" fill="#526578">recorded height</text>',
        f'<polyline points="{poly([(200, 455 - h / height0 * 300) for h in heights])}" fill="none" stroke="#c65b35" stroke-width="2" stroke-dasharray="4 4"/>',
    ]
    for i, s in enumerate(samples):
        y = 455 - heights[i] / height0 * 300
        radius = 7 if i in (0, len(samples) - 1) else 3.5
        color = "#c94435" if i in (0, len(samples) - 1) else "#e77c4b"
        p.append(f'<circle cx="200" cy="{y:.2f}" r="{radius}" fill="{color}" stroke="#fff" stroke-width="1.2"><title>t={times[i]:.2f} s; height={heights[i]:.6g} m; velocity={velocities[i]:.6g} m/s</title></circle>')
    p.extend([
        '<text x="42" y="518" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#254b36">Dots = recorded states.</text>',
        '<text x="42" y="535" font-family="Segoe UI,Arial,sans-serif" font-size="10" fill="#526578">Tree and apple artwork are illustrative.</text>',
        '<rect x="375" y="95" width="600" height="450" rx="14" fill="#fff" stroke="#d9e2ea"/>',
        '<text x="400" y="125" font-family="Segoe UI,Arial,sans-serif" font-size="17" font-weight="700" fill="#183047">Read the motion</text>',
        '<text x="400" y="145" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">Dots are recorded values; connecting lines guide the eye.</text>',
        '<text x="410" y="133" font-family="Segoe UI,Arial,sans-serif" font-size="13" font-weight="700" fill="#226b9a">Height above ground (m)</text>',
        f'<rect x="{x0}" y="{ht}" width="{x1-x0}" height="{hb-ht}" fill="#f8fbfd" stroke="#a9b9c8"/>',
        f'<polyline points="{poly(hpoints)}" fill="none" stroke="#226b9a" stroke-width="3" stroke-linejoin="round"/>',
        '<text x="410" y="348" font-family="Segoe UI,Arial,sans-serif" font-size="13" font-weight="700" fill="#b35434">Vertical velocity (m/s)</text>',
        f'<rect x="{x0}" y="{vt}" width="{x1-x0}" height="{vb-vt}" fill="#fffaf7" stroke="#a9b9c8"/>',
        f'<line x1="{x0}" y1="{sv(0):.2f}" x2="{x1}" y2="{sv(0):.2f}" stroke="#c6b8ae" stroke-dasharray="4 4"/>',
        f'<polyline points="{poly(vpoints)}" fill="none" stroke="#b35434" stroke-width="3" stroke-linejoin="round"/>',
    ])
    for i, (t, h, v) in enumerate(zip(times, heights, velocities)):
        x = sx(t)
        p.append(f'<circle cx="{x:.2f}" cy="{sh(h):.2f}" r="3.2" fill="#226b9a"><title>t={t:.2f} s; height={h:.6g} m</title></circle>')
        p.append(f'<circle cx="{x:.2f}" cy="{sv(v):.2f}" r="3.2" fill="#b35434"><title>t={t:.2f} s; velocity={v:.6g} m/s</title></circle>')
    for i in range(6):
        t = duration * i / 5
        x = sx(t)
        p.append(f'<text x="{x:.2f}" y="511" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="10" fill="#526578">{t:.1f}</text>')
    p.extend([
        '<text x="680" y="530" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="11" fill="#526578">Elapsed time (s)</text>',
        '<text x="400" y="570" font-family="Segoe UI,Arial,sans-serif" font-size="15" font-weight="700" fill="#183047">h(t) = h₀ + v₀t + ½at²</text>',
        f'<text x="400" y="590" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">v(t) = v₀ + at · a = {acceleration:g} {esc(record.get("units", {}).get("acceleration", "m/s²"))} · Δt = {delta_time} s</text>',
        '<text x="32" y="580" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">Ideal free fall · no air resistance · constant gravity · SI units</text>',
        '</svg>',
        ''
    ])
    return "\n".join(p)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path)
    parser.add_argument("output_svg", type=Path)
    parser.add_argument("--check", action="store_true")
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
