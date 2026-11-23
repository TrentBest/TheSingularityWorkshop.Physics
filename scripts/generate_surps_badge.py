#!/usr/bin/env python3
"""Validate SURPS enrollment and generate the repository's README badge."""

from __future__ import annotations

import argparse
import html
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / ".surps" / "participation.json"
README = ROOT / "README.md"
BADGE = ROOT / "docs" / "assets" / "badges" / "surps-participation.svg"
START = "<!-- SURPS:PARTICIPATION:START -->"
END = "<!-- SURPS:PARTICIPATION:END -->"
PARTICIPATION_STATES = {"draft", "active", "suspended", "retired"}
BOOK_STATES = {"draft", "active", "suspended", "retired"}


def repo_path(value: str, label: str) -> Path:
    candidate = (ROOT / value).resolve()
    if candidate != ROOT and ROOT not in candidate.parents:
        raise ValueError(f"{label} escapes the repository: {value}")
    if not candidate.is_file():
        raise ValueError(f"{label} does not exist: {value}")
    return candidate


def load_and_validate() -> dict:
    if not MANIFEST.is_file():
        raise ValueError(f"Missing enrollment manifest: {MANIFEST.relative_to(ROOT)}")
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    if data.get("schemaVersion") != 1:
        raise ValueError("schemaVersion must be 1")
    for key in ("repository", "domain", "contributionSummary", "cataloguePath"):
        if not isinstance(data.get(key), str) or not data[key].strip():
            raise ValueError(f"{key} must be a non-empty string")
    if data.get("participationStatus") not in PARTICIPATION_STATES:
        raise ValueError(f"participationStatus must be one of {sorted(PARTICIPATION_STATES)}")
    repo_path(data["cataloguePath"], "cataloguePath")

    books = data.get("books", [])
    if not isinstance(books, list):
        raise ValueError("books must be an array")
    seen = set()
    for book in books:
        for key in ("id", "title", "category", "version", "status", "bookPath", "descriptorPath", "coverPath"):
            if not isinstance(book.get(key), str) or not book[key].strip():
                raise ValueError(f"Each book requires non-empty {key}")
        if book["id"] in seen:
            raise ValueError(f"Duplicate book id: {book['id']}")
        seen.add(book["id"])
        if book.get("lod") not in range(0, 10):
            raise ValueError(f"Invalid LOD for {book['id']}: {book.get('lod')}")
        if book["status"] not in BOOK_STATES:
            raise ValueError(f"Invalid status for {book['id']}: {book['status']}")
        repo_path(book["bookPath"], f"bookPath for {book['id']}")
        cover_path = repo_path(book["coverPath"], f"coverPath for {book['id']}")
        cover_source = cover_path.read_text(encoding="utf-8")
        if "<svg" not in cover_source or "<title" not in cover_source or "<desc" not in cover_source:
            raise ValueError(f"Cover must be an accessible SVG with title and description: {book['coverPath']}")
        if "Asset role:" not in cover_source or "Provenance:" not in cover_source:
            raise ValueError(f"Cover is missing its asset role/provenance record: {book['coverPath']}")
        descriptor_path = repo_path(book["descriptorPath"], f"descriptorPath for {book['id']}")
        descriptor = json.loads(descriptor_path.read_text(encoding="utf-8"))
        expected = {
            "id": book["id"],
            "title": book["title"],
            "category": book["category"],
            "lod": book["lod"],
            "version": book["version"],
            "cover": book["coverPath"],
        }
        for key, value in expected.items():
            if descriptor.get(key) != value:
                raise ValueError(
                    f"Descriptor mismatch for {book['id']} at {key}: "
                    f"manifest={value!r}, descriptor={descriptor.get(key)!r}"
                )
    return data


def badge_svg(data: dict) -> str:
    books = data.get("books", [])
    contribution = ", ".join(data.get("contributionType", [])) or "domain contribution"
    rows = max(1, (len(books) + 2) // 3)
    height = 112 + rows * 72
    width = 1420
    domain_label = data["domain"].upper()
    title = html.escape("SURPS PARTICIPANT" if domain_label == "SURPS" else domain_label + " · SURPS PARTICIPANT")
    status = data["participationStatus"].upper()
    summary = f"{len(books)} DECLARED BOOKS · PARTICIPANT {status}" if books else f"PROVIDER / DOMAIN CONTRIBUTOR · PARTICIPANT {status}"
    out = [
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width} {height}" role="img" aria-labelledby="title desc">',
        f'<title id="title">{title}</title>',
        f'<desc id="desc">{html.escape(data["contributionSummary"])} {len(books)} declared book outputs.</desc>',
        '<defs><linearGradient id="panel" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#182536"/><stop offset="1" stop-color="#101820"/></linearGradient><linearGradient id="accent" x1="0" y1="0" x2="1" y2="0"><stop stop-color="#8bd5ca"/><stop offset="1" stop-color="#78a9d4"/></linearGradient></defs>',
        f'<rect x="2" y="2" width="{width-4}" height="{height-4}" rx="18" fill="url(#panel)" stroke="#577082" stroke-width="2"/>',
        f'<rect x="18" y="18" width="8" height="{height-36}" rx="4" fill="url(#accent)"/>',
        f'<text x="48" y="51" fill="#f2f6f8" font-family="Segoe UI,Arial,sans-serif" font-size="29" font-weight="700" letter-spacing="1.4">{title}</text>',
        f'<text x="49" y="79" fill="#a9c2d0" font-family="Segoe UI,Arial,sans-serif" font-size="15" letter-spacing="1.1">{html.escape(summary)}</text>',
    ]
    if books:
        for i, book in enumerate(books):
            row, col = divmod(i, 3)
            x, y = 42 + col * 455, 96 + row * 72
            label = f'{book["title"]} · LOD {book["lod"]}'
            meta = f'{book["category"]} · {book["status"].upper()}'
            out.extend([
                f'<rect x="{x}" y="{y}" width="435" height="58" rx="9" fill="#294454"/>',
                f'<text x="{x+14}" y="{y+25}" fill="#f1f5f7" font-family="Segoe UI,Arial,sans-serif" font-size="16" font-weight="600" textLength="405" lengthAdjust="spacingAndGlyphs">{html.escape(label)}</text>',
                f'<text x="{x+14}" y="{y+45}" fill="#b9d0dc" font-family="Segoe UI,Arial,sans-serif" font-size="11">{html.escape(meta)}</text>',
            ])
    else:
        out.append(f'<text x="48" y="130" fill="#f1f5f7" font-family="Segoe UI,Arial,sans-serif" font-size="16">{html.escape(contribution)}</text>')
    out.append("</svg>")
    return "\n".join(out) + "\n"


def readme_content(data: dict, current: str) -> str:
    block = (
        f"{START}\n"
        f"[![SURPS participation and sourcebooks](docs/assets/badges/surps-participation.svg)]({data['cataloguePath']})\n"
        f"{END}"
    )
    starts, ends = current.count(START), current.count(END)
    if starts != ends or starts > 1:
        raise ValueError("README must contain at most one balanced SURPS badge marker pair")
    if starts == 1:
        pattern = re.compile(re.escape(START) + r".*?" + re.escape(END), re.S)
        return pattern.sub(lambda _: block, current)
    match = re.search(r"^# .+$", current, re.M)
    if not match:
        raise ValueError("README needs a top-level heading before first-time badge insertion")
    return current[:match.end()] + "\n\n" + block + current[match.end():]


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true", help="verify generated files without writing")
    args = parser.parse_args()
    try:
        data = load_and_validate()
        expected_badge = badge_svg(data)
        current_readme = README.read_text(encoding="utf-8")
        expected_readme = readme_content(data, current_readme)
        if args.check:
            stale = []
            if not BADGE.is_file() or BADGE.read_text(encoding="utf-8") != expected_badge:
                stale.append("docs/assets/badges/surps-participation.svg")
            if current_readme != expected_readme:
                stale.append("README.md SURPS badge block")
            if stale:
                print("Generated SURPS enrollment output is stale: " + ", ".join(stale), file=sys.stderr)
                return 1
            print(f"Validated {len(data.get('books', []))} SURPS book outputs.")
            return 0
        BADGE.parent.mkdir(parents=True, exist_ok=True)
        BADGE.write_text(expected_badge, encoding="utf-8")
        README.write_text(expected_readme, encoding="utf-8")
        print(f"Generated SURPS badge for {len(data.get('books', []))} book outputs.")
        return 0
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"SURPS enrollment validation failed: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
