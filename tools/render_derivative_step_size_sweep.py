#!/usr/bin/env python3
"""Render actual-vs-indicated derivative error from a generated JSON sweep."""
import argparse
import json
import math
from pathlib import Path
from xml.sax.saxutils import escape


def render(record):
    samples = record["samples"]
    if len(samples) != 12:
        raise ValueError("Expected the twelve-point logarithmic step-size sweep.")
    steps = [float(item["step"]) for item in samples]
    actual = [float(item["actualAbsoluteError"]) for item in samples]
    indicator = [float(item["richardsonCorrectionIndicator"]) for item in samples]
    if any(not math.isfinite(v) or v < 0 for v in steps + actual + indicator) or any(v <= 0 for v in actual):
        raise ValueError("Steps and actual errors must be positive and finite; indicators must be finite and non-negative.")
    # A zero correction is valid. Plot it at a visual floor instead of taking log10(0).
    positive = [v for v in actual + indicator if v > 0]
    indicator_floor = min(positive) / 10
    plotted_indicator = [max(v, indicator_floor) for v in indicator]

    left, right, top, bottom = 100, 1060, 100, 610
    x_min, x_max = math.log10(min(steps)), math.log10(max(steps))
    y_min = math.floor(math.log10(min(actual + plotted_indicator))) - 1
    y_max = math.ceil(math.log10(max(actual + plotted_indicator))) + 1

    def sx(value):
        return left + (math.log10(value) - x_min) / (x_max - x_min) * (right - left)

    def sy(value):
        return bottom - (math.log10(value) - y_min) / (y_max - y_min) * (bottom - top)

    def path(values):
        return " ".join(("M" if i == 0 else "L") + f" {sx(steps[i]):.2f} {sy(values[i]):.2f}" for i in range(len(values)))

    actual_points = "\n".join(f'<circle cx="{sx(steps[i]):.2f}" cy="{sy(actual[i]):.2f}" r="5" fill="#1d4ed8"/>' for i in range(len(samples)))
    indicator_points = "\n".join(f'<rect x="{sx(steps[i])-4:.2f}" y="{sy(plotted_indicator[i])-4:.2f}" width="8" height="8" fill="#c2410c"/>' for i in range(len(samples)))
    grid = []
    for exponent in range(y_min, y_max + 1):
        y = sy(10 ** exponent)
        grid.append(f'<path d="M {left} {y:.2f} H {right}" stroke="#cbd5e1" stroke-dasharray="3 5"/>')
        grid.append(f'<text x="88" y="{y+5:.2f}" text-anchor="end" font-size="14" fill="#475569">1e{exponent}</text>')
    ticks = []
    for exponent in range(int(x_min), int(x_max) + 1):
        x = sx(10 ** exponent)
        ticks.append(f'<path d="M {x:.2f} {top} V {bottom}" stroke="#e2e8f0"/>')
        ticks.append(f'<text x="{x:.2f}" y="640" text-anchor="middle" font-size="14" fill="#475569">1e{exponent}</text>')

    return f'''<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="1160" height="720" viewBox="0 0 1160 720" role="img" aria-labelledby="title desc">
<title id="title">Derivative step-size sweep: actual error and Richardson indicator</title>
<desc id="desc">Twelve model-backed sine derivative estimates compared with the analytical cosine derivative. Blue circles show actual absolute error; orange squares show the Richardson correction indicator. Both axes are logarithmic. Zero correction indicators are displayed at a visual floor below the smallest positive value.</desc>
<rect width="1160" height="720" fill="#ffffff"/>
<text x="100" y="42" font-family="Arial,sans-serif" font-size="25" font-weight="700" fill="#0f172a">Derivative step-size sweep</text>
<text x="100" y="70" font-family="Arial,sans-serif" font-size="16" fill="#475569">{escape(record["functionExpression"])} at x={record["coordinate"]} rad · reference: {escape(record["reference"])}</text>
{''.join(grid)}
{''.join(ticks)}
<path d="M {left} {top} V {bottom} H {right}" fill="none" stroke="#334155" stroke-width="2"/>
<path d="{path(actual)}" fill="none" stroke="#1d4ed8" stroke-width="3"/>
<path d="{path(indicator)}" fill="none" stroke="#c2410c" stroke-width="3" stroke-dasharray="8 5"/>
{actual_points}
{indicator_points}
<circle cx="150" cy="682" r="5" fill="#1d4ed8"/><text x="164" y="687" font-family="Arial,sans-serif" font-size="15" fill="#1e293b">Actual absolute error vs cos(x)</text>
<rect x="445" y="678" width="8" height="8" fill="#c2410c"/><text x="463" y="687" font-family="Arial,sans-serif" font-size="15" fill="#1e293b">Richardson correction indicator</text>
<text x="580" y="710" text-anchor="middle" font-family="Arial,sans-serif" font-size="14" fill="#475569">Coarse step h (log scale)</text>
<text x="24" y="355" transform="rotate(-90 24 355)" text-anchor="middle" font-family="Arial,sans-serif" font-size="14" fill="#475569">Absolute magnitude (log scale)</text>
<text x="100" y="125" font-family="Arial,sans-serif" font-size="14" fill="#64748b">Model-backed numerical experiment · correction is not a guaranteed error bound</text>
</svg>
'''


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("record", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    expected = render(json.loads(args.record.read_text(encoding="utf-8")))
    if args.check:
        if args.output.read_text(encoding="utf-8") != expected:
            raise SystemExit(f"{args.output} is stale; regenerate it from {args.record}.")
        print(f"Verified {args.output} against actual derivative sweep data.")
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(expected, encoding="utf-8")
        print(f"Rendered {args.output} from {args.record}.")


if __name__ == "__main__":
    main()
