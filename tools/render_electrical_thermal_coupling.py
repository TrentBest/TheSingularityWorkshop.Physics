#!/usr/bin/env python3
"""Render an explanatory SVG from an electrical-thermal C# run record."""
import argparse
import html
import json
import math
from pathlib import Path


def number(value, label):
    if not isinstance(value, (int, float)) or not math.isfinite(value):
        raise ValueError(f"{label} must be a finite number")
    return float(value)


def render(record):
    if record.get("schemaVersion") != 1 or record.get("modelId") != "coupling.electrical-to-lumped-thermal":
        raise ValueError("unsupported electrical-thermal run record")
    definition = record["definition"]
    observed = record["electricalObservation"]
    result = record["result"]
    voltage = number(definition["busVoltageVolts"], "bus voltage")
    resistance = number(definition["loadResistanceOhms"], "resistance")
    duration = number(definition["durationSeconds"], "duration")
    fraction = number(definition["absorbedPowerFraction"], "absorbed fraction")
    power = number(observed["demandWatts"], "electrical power")
    energy = number(result["electricalEnergyJoules"], "electrical energy")
    transferred = number(result["thermalEnergyTransferJoules"], "thermal transfer")
    not_transferred = number(result["energyNotTransferredToThermalBodyJoules"], "untransferred energy")
    initial = number(definition["initialThermalState"]["temperatureKelvin"], "initial temperature")
    final = number(result["finalThermalState"]["temperatureKelvin"], "final temperature")
    if duration <= 0 or resistance <= 0 or not 0 <= fraction <= 1:
        raise ValueError("invalid experiment definition")
    if abs(transferred + not_transferred - energy) > 1e-8 * max(1.0, abs(energy)):
        raise ValueError("run record does not close the declared energy accounting")
    esc = lambda value: html.escape(str(value), quote=True)
    return "\n".join([
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 620" role="img" aria-labelledby="title desc">',
        '<title id="title">Electrical energy transferred into a thermal body</title>',
        f'<desc id="desc">Computed from the Physics model: {voltage:g} volt bus, {resistance:g} ohm load, {power:g} watts for {duration:g} seconds gives {energy:g} joules. A declared absorbed fraction of {fraction:g} transfers {transferred:g} joules to a lumped body, raising its temperature from {initial:g} kelvin to {final:g} kelvin.</desc>',
        '<rect width="1000" height="620" rx="24" fill="#f7f9fc"/>',
        '<text x="44" y="48" font-family="Segoe UI, Arial, sans-serif" font-size="27" font-weight="700" fill="#14253d">One causal bridge: electricity → thermal state</text>',
        f'<text x="44" y="78" font-size="15" fill="#475569">Model {esc(record["modelId"])} · v{record["modelVersion"]} · computed from actual C# model output</text>',
        '<rect x="44" y="110" width="260" height="150" rx="16" fill="#ffffff" stroke="#9bbce0" stroke-width="2"/>',
        '<text x="68" y="145" font-size="19" font-weight="700" fill="#1e4f85">Electrical grid</text>',
        f'<text x="68" y="180" font-size="17" fill="#14253d">{voltage:g} V ÷ {resistance:g} Ω = {voltage / resistance:g} A</text>',
        f'<text x="68" y="211" font-size="17" fill="#14253d">Power = {power:g} W</text>',
        f'<text x="68" y="238" font-size="15" fill="#475569">Energy = {energy:g} J</text>',
        '<path d="M 320 185 H 420" stroke="#2563a6" stroke-width="5" stroke-linecap="round"/><path d="M 420 185 l -14 -9 v 18 z" fill="#2563a6"/>',
        '<rect x="438" y="110" width="240" height="150" rx="16" fill="#fffaf0" stroke="#d9ad52" stroke-width="2"/>',
        '<text x="462" y="145" font-size="19" font-weight="700" fill="#805b16">Transfer rule</text>',
        f'<text x="462" y="180" font-size="17" fill="#14253d">{fraction:g} × {energy:g} J</text>',
        f'<text x="462" y="211" font-size="17" fill="#14253d">= {transferred:g} J to body</text>',
        f'<text x="462" y="238" font-size="15" fill="#475569">{100*fraction:g}% is caller-declared</text>',
        '<path d="M 694 185 H 794" stroke="#b7791f" stroke-width="5" stroke-linecap="round"/><path d="M 794 185 l -14 -9 v 18 z" fill="#b7791f"/>',
        '<rect x="812" y="110" width="144" height="150" rx="16" fill="#ffffff" stroke="#d9a58c" stroke-width="2"/>',
        '<text x="830" y="145" font-size="18" font-weight="700" fill="#9a4823">Thermal body</text>',
        f'<text x="830" y="183" font-size="16" fill="#14253d">{initial:g} K</text>',
        '<path d="M 868 192 v 20" stroke="#bd5a23" stroke-width="3"/><path d="M 861 205 l 7 8 7 -8" fill="none" stroke="#bd5a23" stroke-width="3"/>',
        f'<text x="830" y="239" font-size="16" font-weight="700" fill="#9a4823">{final:g} K</text>',
        '<rect x="44" y="292" width="912" height="118" rx="16" fill="#ffffff" stroke="#cbd5e1"/>',
        '<text x="68" y="326" font-size="20" font-weight="700" fill="#14253d">Energy accounting for this experiment</text>',
        f'<text x="68" y="363" font-size="18" fill="#14253d">{transferred:g} J transferred to the body + {not_transferred:g} J not transferred = {energy:g} J electrical</text>',
        '<text x="68" y="389" font-size="14" fill="#64748b">The untransferred portion is not assigned to a loss mechanism or destination.</text>',
        '<rect x="44" y="434" width="912" height="140" rx="16" fill="#fff7ed" stroke="#fdba74"/>',
        '<text x="68" y="469" font-size="19" font-weight="700" fill="#9a3412">What this picture does not claim</text>',
        '<text x="68" y="501" font-size="15" fill="#7c2d12">The absorbed fraction is an input, not a predicted efficiency. No changing resistance,</text>',
        '<text x="68" y="525" font-size="15" fill="#7c2d12">external heat exchange, phase change, or spatial temperature gradient is modeled.</text>',
        '<text x="44" y="602" font-size="13" fill="#475569">Deterministic analytical fixture · not a validated appliance or real-world heater model</text>',
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
            raise SystemExit("SVG is stale: regenerate it from the electrical-thermal run record")
        print("Electrical-thermal SVG matches the run record.")
        return
    args.output_svg.parent.mkdir(parents=True, exist_ok=True)
    args.output_svg.write_text(generated, encoding="utf-8")
    print(f"Wrote {args.output_svg}")


if __name__ == "__main__":
    main()
