#!/usr/bin/env python3
"""Merge a partial translator table into the canonical table for one language.

Keys are the English key set. A value in --partial replaces that key. Any
other key stays as it is in --into, or as the English source when --into is
omitted. Keys that are not in English are dropped. Output is UTF-8, LF,
2-space indent, ordinal-sorted. meta.sourceHashes is recomputed from the
current English values (section, key, and value).
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

import validate_table as kit


def as_map(table, section) -> dict:
    mapping = table.get(section) if isinstance(table, dict) else None
    if not isinstance(mapping, dict):
        return {}
    return {key: value for key, value in mapping.items() if isinstance(value, str)}


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description="Merge a partial translation into the canonical table.")
    parser.add_argument("--lang", required=True)
    parser.add_argument("--partial", required=True, help="translator JSON (meta, spec, str)")
    parser.add_argument("--en", help="English source JSON")
    parser.add_argument("--into", help="canonical table to update; default is a fresh English skeleton")
    parser.add_argument("--out", required=True, help="where to write the merged table")
    args = parser.parse_args(argv)

    if args.lang not in kit.LANGUAGES or args.lang == "en":
        print(f"unknown language {args.lang}", file=sys.stderr)
        return 2
    en_path = Path(args.en) if args.en else kit.DEFAULT_EN
    partial_path = Path(args.partial)
    if not en_path.is_file() or not partial_path.is_file():
        print("English table or partial table was not found", file=sys.stderr)
        return 2
    _raw, english = kit.load_json(en_path)
    _praw, partial = kit.load_json(partial_path)
    if args.into:
        into_path = Path(args.into)
        if not into_path.is_file():
            print(f"canonical table not found: {into_path}", file=sys.stderr)
            return 2
        _iraw, into = kit.load_json(into_path)
    else:
        into = kit.skeleton(args.lang, english, hashes=False)

    info = kit.LANGUAGES[args.lang]
    version = 1
    into_meta = into.get("meta") if isinstance(into.get("meta"), dict) else {}
    if isinstance(into_meta.get("version"), int):
        version = into_meta["version"]
    partial_meta = partial.get("meta") if isinstance(partial.get("meta"), dict) else {}
    if partial_meta.get("name") not in (None, "", info[1]):
        print(f"note: partial meta.name is {partial_meta.get('name')!r}; writing endonym {info[1]!r}", file=sys.stderr)

    merged = {}
    for section in ("spec", "str"):
        base = as_map(into, section)
        overlay = as_map(partial, section)
        source = as_map(english, section)
        values = {}
        for key in source:
            if key in overlay:
                values[key] = overlay[key]
            elif key in base:
                values[key] = base[key]
            else:
                values[key] = source[key]
        for key in overlay:
            if key not in source:
                print(f"dropped extra {section} key: {key}", file=sys.stderr)
        merged[section] = kit.sorted_map(values)

    table = {
        "meta": {
            "code": args.lang,
            "name": info[1],
            "version": version,
            "source": "en",
            "sourceHashes": kit.build_hashes(english),
        },
        "spec": merged["spec"],
        "str": merged["str"],
    }
    listed = english.get("doNotTranslate")
    if isinstance(listed, list):
        table["doNotTranslate"] = sorted((item for item in listed if isinstance(item, str)), key=kit.ordinal_bytes)
    out = Path(args.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(kit.dump_table(table), encoding="utf-8", newline="\n")
    print(out)
    return 0


if __name__ == "__main__":
    sys.exit(main())
