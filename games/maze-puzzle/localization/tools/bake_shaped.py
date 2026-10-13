#!/usr/bin/env python3
"""Bake HarfBuzz glyph runs for Arabic, Hindi, and Khmer.

Reads spec and str values from a language table. The translators own
``Assets/Resources/Localization/<code>.json``; until those files exist,
pass ``--table`` pointing at a fixture. Endonyms are always included so
the language buttons can shape.

Writes ``Assets/Resources/Localization/shaped/<code>.bytes``.

Examples
--------
  python3 bake_shaped.py --lang ar
  python3 bake_shaped.py --lang hi --table localization/tools/fixtures/shaped-ui.json
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from shaped_engine import ENDONYMS, LANGS, BakeError, Face, endonym_strings, shape_string
from shaped_format import pack_table, sha256_file

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]
UNITY = REPO / "games" / "maze-puzzle" / "unity" / "maze-puzzle"
FONTS = UNITY / "Assets" / "Fonts"
OUT_DIR = UNITY / "Assets" / "Resources" / "Localization" / "shaped"
TABLES = UNITY / "Assets" / "Resources" / "Localization"


def load_strings(path: Path, lang: str) -> list[str]:
    data = json.loads(path.read_text(encoding="utf-8"))
    found: list[str] = []
    if lang in data and isinstance(data[lang], dict) and "strings" in data[lang]:
        found.extend(data[lang]["strings"])
    elif lang in data and isinstance(data[lang], list):
        found.extend(data[lang])
    else:
        for section in ("spec", "str"):
            block = data.get(section) or {}
            if isinstance(block, dict):
                found.extend(v for v in block.values() if isinstance(v, str))
    # Keep order, drop empties and duplicates.
    out = []
    seen = set()
    for text in found:
        if not isinstance(text, str) or text == "" or text in seen:
            continue
        seen.add(text)
        out.append(text)
    return out


def bake(lang: str, table: Path, font: Path, dest: Path) -> dict:
    if lang not in LANGS:
        raise BakeError(f"unknown lang {lang}")
    strings = load_strings(table, lang)
    for extra in endonym_strings(lang):
        if extra not in strings:
            strings.append(extra)
    if not strings:
        raise BakeError(f"no strings for {lang} in {table}")
    face = Face(str(font), lang)
    entries = []
    for text in strings:
        try:
            entries.append(shape_string(face, text))
        except BakeError as exc:
            raise BakeError(f"{lang}: {exc}") from exc
    digest = sha256_file(font)
    blob = pack_table(LANGS[lang]["font"], 700, face.upem, digest, entries)
    dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_bytes(blob)
    return {
        "lang": lang,
        "strings": len(entries),
        "bytes": len(blob),
        "font": font.name,
        "font_bytes": font.stat().st_size,
        "upem": face.upem,
        "out": str(dest),
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--lang", required=True, choices=sorted(LANGS))
    parser.add_argument("--table", type=Path, help="language table or fixture JSON")
    parser.add_argument("--font", type=Path, help="pinned glyf instance (defaults to Assets/Fonts)")
    parser.add_argument("--out", type=Path, help="output .bytes")
    args = parser.parse_args(argv)
    table = args.table
    if table is None:
        table = TABLES / f"{args.lang}.json"
    if not table.is_file():
        print(
            f"no table at {table}. Translators own Assets/Resources/Localization/{args.lang}.json. "
            "Pass --table with a fixture until that file exists.",
            file=sys.stderr,
        )
        return 1
    font = args.font
    if font is None:
        font = FONTS / f"{LANGS[args.lang]['font']}.ttf"
    if not font.is_file():
        print(f"missing font {font}. Run build_shaped_fonts.py first.", file=sys.stderr)
        return 1
    dest = args.out or (OUT_DIR / f"{args.lang}.bytes")
    try:
        info = bake(args.lang, table, font, dest)
    except BakeError as exc:
        print(f"bake failed: {exc}", file=sys.stderr)
        return 1
    print(
        f"baked {info['lang']}: {info['strings']} strings, {info['bytes']} bytes, "
        f"font {info['font']} {info['font_bytes']} bytes, upem {info['upem']}"
    )
    print(info["out"])
    return 0


if __name__ == "__main__":
    sys.exit(main())
