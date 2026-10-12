#!/usr/bin/env python3
"""Write a 30-key side-by-side review sample for one language.

Selection, in order, capped at --limit (default 30):

1. The 5 strings with the most {n} placeholders (then the most digit runs).
2. The 10 longest strings (key-index chars, else the English length).
3. Round-robin through the first 5 keys of each screen area.

Areas come from the first file named in key-index.csv. The markdown columns
are area, English, translation, UTF-16 character ratio, and why the row was picked.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

import validate_table as kit

AREA_PREFIXES = (
    ("achievements", "trophies"),
    ("daily", "daily"),
    ("game_daily", "daily"),
    ("maps", "maps"),
    ("modal_debrief", "debrief"),
    ("modal_defeat", "defeat"),
    ("modal_pause", "pause"),
    ("settings", "settings"),
    ("shop", "shop"),
    ("game", "play"),
)


def area_of(files_field: str) -> str:
    name = (files_field or "").split(":", 1)[-1].split(";")[0].strip()
    stem = name.split("__")[0]
    for prefix, area in AREA_PREFIXES:
        if stem.startswith(prefix):
            return area
    return "other"


def placeholder_score(text: str) -> tuple:
    indexes = kit.placeholder_indexes(text)
    digits = sum(1 for ch in text if ch.isdigit())
    return (len(indexes), digits, kit.utf16_len(text))


def cell(text: str) -> str:
    return (text or "").replace("|", "\\|").replace("\n", " ")


def select(rows: list, limit: int) -> list:
    longest = sorted(rows, key=lambda row: (-row["chars"], row["key"]))[:10]
    rich = sorted(rows, key=lambda row: (-row["score"][0], -row["score"][1], row["key"]))[:5]
    buckets = {}
    for row in rows:
        buckets.setdefault(row["area"], [])
        if len(buckets[row["area"]]) < 5:
            buckets[row["area"]].append(row)
    areas = []
    for row in rows:
        if row["area"] not in areas:
            areas.append(row["area"])

    picked = []
    seen = set()

    def add(row, why):
        if row["key"] in seen or len(picked) >= limit:
            return
        seen.add(row["key"])
        item = dict(row)
        item["why"] = why
        picked.append(item)

    for row in rich:
        add(row, "placeholders")
    for row in longest:
        add(row, "longest")
    index = 0
    while len(picked) < limit:
        progressed = False
        for area in areas:
            bucket = buckets.get(area) or []
            if index < len(bucket):
                add(bucket[index], "area")
                progressed = True
            if len(picked) >= limit:
                break
        if not progressed:
            break
        index += 1
    return picked


def load_rows(index_path: Path, english: dict) -> list:
    spec = english.get("spec") or {}
    strings = english.get("str") or {}
    rows = []
    if index_path.is_file():
        indexed = kit.load_key_index(index_path)
        seen = set()
        for (kind, key), info in indexed.items():
            source_map = spec if kind == "spec" else strings
            if key not in source_map:
                continue
            text = source_map[key]
            try:
                chars = int(info.get("chars") or 0)
            except ValueError:
                chars = kit.utf16_len(text)
            rows.append(
                {
                    "section": kind,
                    "key": key,
                    "english": text,
                    "area": area_of(info.get("files") or ""),
                    "chars": chars or kit.utf16_len(text),
                    "score": placeholder_score(text),
                }
            )
            seen.add((kind, key))
        for section, mapping in (("spec", spec), ("str", strings)):
            for key, text in mapping.items():
                if (section, key) in seen or not isinstance(text, str):
                    continue
                rows.append(
                    {
                        "section": section,
                        "key": key,
                        "english": text,
                        "area": "str" if section == "str" else "other",
                        "chars": kit.utf16_len(text),
                        "score": placeholder_score(text),
                    }
                )
        return rows
    for section, mapping in (("spec", spec), ("str", strings)):
        for key, text in mapping.items():
            if not isinstance(text, str):
                continue
            rows.append(
                {
                    "section": section,
                    "key": key,
                    "english": text,
                    "area": section,
                    "chars": kit.utf16_len(text),
                    "score": placeholder_score(text),
                }
            )
    return rows


def render(lang: str, picked: list, translation: dict) -> str:
    spec = translation.get("spec") if isinstance(translation, dict) else {}
    strings = translation.get("str") if isinstance(translation, dict) else {}
    if not isinstance(spec, dict):
        spec = {}
    if not isinstance(strings, dict):
        strings = {}
    lines = [
        f"# Review sample — {lang}",
        "",
        "Thirty keys (or fewer): placeholder-heavy, then longest, then the first keys of each screen area.",
        "Ratio is UTF-16 length of the translation divided by the English length.",
        "",
        "| Area | English | Translation | Ratio | Why |",
        "| --- | --- | --- | ---: | --- |",
    ]
    for row in picked:
        mapping = spec if row["section"] == "spec" else strings
        target = mapping.get(row["key"])
        if not isinstance(target, str):
            shown = "(missing)"
            ratio = ""
        else:
            shown = target
            en_len = kit.utf16_len(row["english"])
            ratio = kit.format_ratio(kit.utf16_len(target) / en_len) if en_len else ""
        lines.append(
            f"| {cell(row['area'])} | {cell(row['english'])} | {cell(shown)} | {ratio} | {row['why']} |"
        )
    lines.append("")
    return "\n".join(lines)


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description="Write a side-by-side review sample.")
    parser.add_argument("--lang", required=True)
    parser.add_argument("--table", help="translation JSON; English is shown if the file is missing")
    parser.add_argument("--en")
    parser.add_argument("--key-index", dest="key_index")
    parser.add_argument("--out", help="markdown path; default is stdout")
    parser.add_argument("--limit", type=int, default=30)
    args = parser.parse_args(argv)
    if args.lang not in kit.LANGUAGES or args.lang == "en":
        print(f"unknown language {args.lang}", file=sys.stderr)
        return 2
    en_path = Path(args.en) if args.en else kit.DEFAULT_EN
    if not en_path.is_file():
        print(f"English table not found: {en_path}", file=sys.stderr)
        return 2
    _raw, english = kit.load_json(en_path)
    table_path = Path(args.table) if args.table else en_path.with_name(args.lang + ".json")
    translation = {"spec": {}, "str": {}}
    if table_path.is_file():
        _traw, translation = kit.load_json(table_path)
    index_path = Path(args.key_index) if args.key_index else kit.DEFAULT_INDEX
    rows = load_rows(index_path, english)
    picked = select(rows, args.limit)
    text = render(args.lang, picked, translation)
    if args.out:
        Path(args.out).parent.mkdir(parents=True, exist_ok=True)
        Path(args.out).write_text(text, encoding="utf-8", newline="\n")
        print(args.out)
    else:
        sys.stdout.write(text)
    return 0


if __name__ == "__main__":
    sys.exit(main())
