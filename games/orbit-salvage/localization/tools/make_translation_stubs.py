#!/usr/bin/env python3
"""Write skeleton translation tables. Default output is a scratch directory.

The files copy every spec and str key from en.json with the English text and
fill meta (code, endonym, source hash). They are not translations. The default
directory is /tmp/maze-i18n/translation-stubs so an unfinished table is not
committed by accident. Writing inside this repo requires --allow-repo.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

import validate_table as kit

DEFAULT_OUT = Path("/tmp/maze-i18n/translation-stubs")


def inside_worktree(path: Path) -> bool:
    try:
        path.resolve().relative_to(kit.WORKTREE.resolve())
    except ValueError:
        return False
    return True


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description="Create skeleton translation tables from en.json.")
    parser.add_argument("--lang", action="append", help="registry code; repeatable. Default: all 16 targets")
    parser.add_argument("--en", help="English source JSON")
    parser.add_argument("--out", default=str(DEFAULT_OUT), help="scratch directory (default /tmp/maze-i18n/translation-stubs)")
    parser.add_argument("--allow-repo", action="store_true", help="allow --out inside the git worktree")
    args = parser.parse_args(argv)

    langs = args.lang or list(kit.TARGET_CODES)
    for lang in langs:
        if lang not in kit.LANGUAGES or lang == "en":
            print(f"unknown language {lang}", file=sys.stderr)
            return 2
    en_path = Path(args.en) if args.en else kit.DEFAULT_EN
    if not en_path.is_file():
        print(f"English table not found: {en_path}", file=sys.stderr)
        return 2
    out = Path(args.out)
    if inside_worktree(out) and not args.allow_repo:
        print(f"refusing to write stubs inside the repo: {out}", file=sys.stderr)
        print("pass --out to a scratch directory, or --allow-repo if you really mean it", file=sys.stderr)
        return 2
    _raw, english = kit.load_json(en_path)
    out.mkdir(parents=True, exist_ok=True)
    for lang in langs:
        table = kit.skeleton(lang, english, hashes=True)
        dest = out / f"{lang}.json"
        dest.write_text(kit.dump_table(table), encoding="utf-8", newline="\n")
        print(dest)
    return 0


if __name__ == "__main__":
    sys.exit(main())
