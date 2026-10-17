#!/usr/bin/env python3
"""Render a contact-response SVG from the C# resolver's serialized output."""
import argparse
import html
import json
import math
from pathlib import Path


def finite(value, label):
    if not isinstance(value, (int, float)) or not math.isfinite(value):
        raise ValueError(f"{label} must be finite")
    return value


def render(record):
    if record.get("schemaVersion") != 1:
        raise ValueError("unsupported or missing run-record schemaVersion")
    if record.get("modelId") != "collision.single-contact-normal-impulse":
        raise ValueError("unexpected model identity")
    definition = record.get("definition", {})
    result = record.get("result", {})
    mass_a = finite(definition.get("massAKilograms"), "mass A")
    mass_b = finite(definition.get("massBKilograms"), "mass B")
    restitution = finite(definition.get("restitution"), "restitution")
    separation = finite(definition.get("signedSeparationMetres"), "signed separation")
    impulse = finite(result.get("normalImpulseNewtonSeconds"), "normal impulse")
    if mass_a <= 0 or mass_b <= 0 or not 0 <= restitution <= 1:
        raise ValueError("masses must be positive and restitution must be in [0, 1]")
    if not isinstance(result.get("posePreserved"), bool) or not result["posePreserved"]:
        raise ValueError("this response example expects preserved body poses")
    a0 = finite(definition["bodyAInitialVelocityMetresPerSecond"]["x"], "A initial x velocity")
    b0 = finite(definition["bodyBInitialVelocityMetresPerSecond"]["x"], "B initial x velocity")
    a1 = finite(result["bodyAFinalVelocityMetresPerSecond"]["x"], "A final x velocity")
    b1 = finite(result["bodyBFinalVelocityMetresPerSecond"]["x"], "B final x velocity")
    esc = lambda value: html.escape(str(value), quote=True)
    # Arrow lengths encode the actual recorded x-velocity; zero is shown as a point.
    center = 480
    scale = 105
    def arrow(y, velocity, label, color):
        end = center + velocity * scale
        if abs(velocity) < 1e-12:
            return (
                f'<circle cx="{center}" cy="{y}" r="5" fill="{color}"/>'
                f'<text x="{center + 14}" y="{y + 5}" font-size="15" fill="#23334a">0 m/s</text>'
            )
        direction = 1 if velocity > 0 else -1
        start = center
        tip = end
        base = tip - direction * 13
        return (
            f'<line x1="{start}" y1="{y}" x2="{tip}" y2="{y}" stroke="{color}" stroke-width="6" stroke-linecap="round"/>'
            f'<path d="M {tip} {y} L {base} {y-8} L {base} {y+8} Z" fill="{color}"/>'
            f'<text x="{(start + tip) / 2:.1f}" y="{y-13}" text-anchor="middle" font-size="15" font-weight="700" fill="{color}">{velocity:.3g} m/s</text>'
        )
    return "\n".join([
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 960 650" role="img" aria-labelledby="title desc">',
        '<title id="title">A single contact, resolved by a normal impulse</title>',
        f'<desc id="desc">Actual resolver output: equal masses {mass_a:g} and {mass_b:g} kilograms, initial normal velocities {a0:g} and {b0:g} metres per second, restitution {restitution:g}, resulting impulse {impulse:g} newton-seconds and final normal velocities {a1:g} and {b1:g} metres per second. No time is advanced.</desc>',
        '<rect width="960" height="650" rx="20" fill="#f7f9fc"/>',
        '<text x="44" y="48" font-family="Segoe UI, Arial, sans-serif" font-size="25" font-weight="700" fill="#14253d">Contact response · one instantaneous normal impulse</text>',
        f'<text x="44" y="78" font-size="14" fill="#475569">Model {esc(record["modelId"])} · v{record["modelVersion"]} · actual C# resolver output</text>',
        '<rect x="40" y="105" width="880" height="225" rx="14" fill="#ffffff" stroke="#cbd5e1"/>',
        '<text x="65" y="139" font-size="18" font-weight="700" fill="#14253d">Before contact response</text>',
        '<line x1="480" y1="160" x2="480" y2="300" stroke="#cbd5e1" stroke-dasharray="4 5"/>',
        '<text x="65" y="198" font-size="16" font-weight="700" fill="#2563a6">Body A</text>',
        arrow(220, a0, "A", "#2563a6"),
        '<text x="65" y="258" font-size="16" font-weight="700" fill="#bd5a23">Body B</text>',
        arrow(280, b0, "B", "#bd5a23"),
        '<text x="150" y="181" font-size="13" fill="#64748b">−x</text><text x="820" y="181" font-size="13" fill="#64748b">+x</text>',
        '<rect x="40" y="350" width="880" height="92" rx="14" fill="#eaf2fb" stroke="#a8c7e7"/>',
        '<text x="65" y="380" font-size="17" font-weight="700" fill="#14253d">Resolver result</text>',
        f'<text x="65" y="408" font-size="16" fill="#14253d">Normal impulse magnitude: {impulse:.6g} N·s</text>',
        f'<text x="490" y="408" font-size="15" fill="#14253d">Equal and opposite impulse: A −{impulse:.6g} N·s · B +{impulse:.6g} N·s</text>',
        '<rect x="40" y="462" width="880" height="142" rx="14" fill="#ffffff" stroke="#cbd5e1"/>',
        '<text x="65" y="495" font-size="18" font-weight="700" fill="#14253d">After response · pose and logical time preserved</text>',
        '<line x1="480" y1="510" x2="480" y2="578" stroke="#cbd5e1" stroke-dasharray="4 5"/>',
        '<text x="65" y="535" font-size="15" font-weight="700" fill="#2563a6">A</text>',
        arrow(540, a1, "A", "#2563a6"),
        '<text x="65" y="575" font-size="15" font-weight="700" fill="#bd5a23">B</text>',
        arrow(580, b1, "B", "#bd5a23"),
        f'<text x="44" y="630" font-size="13" fill="#475569">mA={mass_a:g} kg · mB={mass_b:g} kg · e={restitution:g} · signed separation={separation:g} m · timestep advanced=0 s</text>',
        '</svg>',
        ''
    ])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path)
    parser.add_argument("output_svg", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    generated = render(json.loads(args.run_record.read_text(encoding="utf-8")))
    if args.check:
        if not args.output_svg.is_file() or args.output_svg.read_text(encoding="utf-8") != generated:
            raise SystemExit("SVG is stale: regenerate it from the resolver run record")
        print("SVG matches the resolver run record.")
        return
    args.output_svg.parent.mkdir(parents=True, exist_ok=True)
    args.output_svg.write_text(generated, encoding="utf-8")
    print(f"Wrote {args.output_svg}")


if __name__ == "__main__":
    main()
