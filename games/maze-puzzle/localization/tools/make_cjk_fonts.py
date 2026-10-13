#!/usr/bin/env python3
"""Build the four CJK outline subsets.

Character set for each language, in order:

1. Every character in Assets/Resources/Localization/<code>.json spec and str
   values, when that table exists. A missing table is not an error.
2. Printable ASCII (U+0020–U+007E), CJK punctuation (U+3000–U+303F),
   fullwidth forms (U+FF00–U+FF60), and … · × ← — (the breaker keeps
   …… and —— together).
3. That language's endonym.
4. The safety margin in cjk_safety.json (about 500 common characters).
   Japanese also includes the hiragana and katakana blocks.

Output is static glyf TrueType, cmap 4 (and 12 if a non-BMP scalar is
kept), OS/2 retained, no fvar. One file per language, well under 1.5 MB.

Weights. ZCOOL KuaiLe, Huninn, and Jua each have one master (OS/2 400).
Every CSS weight maps onto that file. ZCOOL and Jua are display drawings.
Huninn is a regular rounded gothic and has no heavier file. M PLUS
Rounded 1c ships the Bold (700) instance. CSS 900 becomes 700, the same
ceiling Fredoka already uses for 800 and 900. Black (900) exists upstream
and is not shipped: this one file also paints body text.

Jua's source has no U+0060. Nunito, later in the chain, has it.

    /tmp/maze-i18n/kit-venv/bin/python games/maze-puzzle/localization/tools/make_cjk_fonts.py

Run from the worktree root, or pass nothing and the script finds its
own paths. Source TTFs default to /tmp/maze-i18n/fonts/dl.
"""

from __future__ import annotations

import json
import subprocess
import sys
import tempfile
from pathlib import Path

from fontTools.ttLib import TTFont

TOOL = Path(__file__).resolve().parent
GAME = TOOL.parent.parent
UNITY = GAME / "unity" / "maze-puzzle"
FONTS = UNITY / "Assets" / "Fonts"
BYTES = UNITY / "Assets" / "Resources" / "Fonts" / "Ttf"
TABLES = UNITY / "Assets" / "Resources" / "Localization"
SAFETY = TOOL / "cjk_safety.json"
SUBSET = TOOL / "subset_fonts.py"
DL = Path("/tmp/maze-i18n/fonts/dl")

# Stable Unity import ids. Re-running the script must not churn .meta files.
METAS = {
    "ZCOOLKuaiLe-sc.ttf": "b7e4c1a09d3f4a6e8c2b15d07f9a4e31",
    "ZCOOLKuaiLe-sc.bytes": "c8f5d2b10e4a5b7f9d3c26e180ab5f42",
    "Huninn-tc.ttf": "d9a6e3c2215b6c809e4d37f291bc6053",
    "Huninn-tc.bytes": "e0b7f4d3326c7d91af5e4803a2cd7164",
    "MPLUSRounded1c-700.ttf": "f1c805e4437d8ea2b06f5914b3de8275",
    "MPLUSRounded1c-700.bytes": "a2d916f5548e9fb3c1706a25c4ef9386",
    "Jua-kr.ttf": "b3ea2706659fac4d2827b36d5f0aa497",
    "Jua-kr.bytes": "c4fb3817760abd5e3938c47e601bb5a8",
}

# Old picker-only subsets. Removed once the real files are written.
RETIRED = (
    "ZCOOLKuaiLe-endonym",
    "Huninn-endonym",
    "MPLUSRounded1c-endonym",
    "Jua-endonym",
)

SIZE_LIMIT = int(1.5 * 1024 * 1024)

# code, file stem, source ttf, OS/2 weight recorded in fonts.json, endonym
FACES = (
    ("zh-Hans", "ZCOOLKuaiLe-sc", "ZCOOLKuaiLe-Regular.ttf", 400, "简体中文"),
    ("zh-Hant", "Huninn-tc", "Huninn-Regular.ttf", 400, "繁體中文"),
    ("ja", "MPLUSRounded1c-700", "MPLUSRounded1c-Bold.ttf", 700, "日本語"),
    ("ko", "Jua-kr", "Jua-Regular.ttf", 400, "한국어"),
)

# Jua's source cmap has no backtick. Anything else missing fails the build.
ALLOWED_MISSING = {
    "ko": {0x60},
}


def printable_ascii() -> str:
    return "".join(chr(cp) for cp in range(0x20, 0x7F))


def punctuation() -> str:
    chars = []
    chars.extend(chr(cp) for cp in range(0x3000, 0x3040))
    chars.extend(chr(cp) for cp in range(0xFF00, 0xFF61))
    chars.append("\u2026")  # …
    chars.append("\u00B7")  # ·
    chars.append("\u00D7")  # ×
    chars.append("\u2190")  # ←
    chars.append("\u2014")  # —  so a —— pair has a glyph
    return "".join(chars)


def kana() -> str:
    """Hiragana and katakana letters, plus the long-vowel mark. Skips unassigned slots."""
    chars = []
    chars.extend(chr(cp) for cp in range(0x3041, 0x3097))
    chars.extend(chr(cp) for cp in (0x309D, 0x309E))
    chars.extend(chr(cp) for cp in range(0x30A1, 0x30FB))
    chars.extend(chr(cp) for cp in range(0x30FC, 0x30FF))
    return "".join(chars)


def table_text(code: str) -> str:
    path = TABLES / f"{code}.json"
    if not path.is_file():
        return ""
    data = json.loads(path.read_text(encoding="utf-8"))
    parts = []
    for section in ("spec", "str"):
        block = data.get(section) or {}
        if not isinstance(block, dict):
            continue
        for value in block.values():
            if isinstance(value, str):
                parts.append(value)
    return "".join(parts)


def load_safety() -> dict:
    data = json.loads(SAFETY.read_text(encoding="utf-8"))
    for code, _stem, _src, _weight, _endonym in FACES:
        text = data.get(code) or ""
        if len(text) < 400:
            raise SystemExit(f"cjk_safety.json {code} has {len(text)} characters")
    return data


def collect(code: str, safety: dict | None = None) -> str:
    """Characters to keep, stable order, duplicates removed later by the subsetter."""
    if safety is None:
        safety = load_safety()
    endonym = dict((row[0], row[4]) for row in FACES)[code]
    parts = [table_text(code), printable_ascii(), punctuation(), endonym, safety[code]]
    if code == "ja":
        parts.append(kana())
    return "".join(parts)


def _ttf_meta(guid: str) -> str:
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "TrueTypeFontImporter:\n"
        "  externalObjects: {}\n"
        "  serializedVersion: 4\n"
        "  fontSize: 16\n"
        "  forceTextureCase: -2\n"
        "  characterSpacing: 0\n"
        "  characterPadding: 1\n"
        "  includeFontData: 1\n"
        "  fontNames: []\n"
        "  fallbackFontReferences: []\n"
        "  customCharacters: \n"
        "  fontRenderingMode: 0\n"
        "  ascentCalculationMode: 1\n"
        "  useLegacyBoundsCalculation: 0\n"
        "  shouldRoundAdvanceValue: 1\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


def _bytes_meta(guid: str) -> str:
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "TextScriptImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


def write_meta(path: Path, guid: str, kind: str) -> None:
    meta = path.with_name(path.name + ".meta")
    if meta.is_file():
        return
    text = _ttf_meta(guid) if kind == "ttf" else _bytes_meta(guid)
    meta.write_text(text, encoding="utf-8")


def subset_one(src: Path, text: str, dest: Path) -> None:
    dest.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="cjk-subset-") as tmp:
        chars = Path(tmp) / "chars.txt"
        chars.write_text(text, encoding="utf-8")
        cmd = [
            sys.executable,
            str(SUBSET),
            "--font",
            str(src),
            "--text-file",
            str(chars),
            "--keep-os2",
            "--out",
            str(dest),
        ]
        print(" ".join(cmd))
        subprocess.check_call(cmd)


def check_font(path: Path, code: str, endonym: str, safety_text: str, requested: str) -> int:
    font = TTFont(path)
    try:
        if "glyf" not in font:
            raise SystemExit(f"{path.name}: no glyf table")
        if "fvar" in font:
            raise SystemExit(f"{path.name}: still a variable font")
        if "OS/2" not in font:
            raise SystemExit(f"{path.name}: OS/2 was dropped")
        cmap = font.getBestCmap() or {}
        formats = set()
        for table in font["cmap"].tables:
            formats.add(table.format)
        if 4 not in formats and 12 not in formats:
            raise SystemExit(f"{path.name}: cmap formats {sorted(formats)}")
        missing_endonym = [ch for ch in endonym if ord(ch) not in cmap]
        if missing_endonym:
            raise SystemExit(f"{path.name}: endonym missing {''.join(missing_endonym)}")
        missing_safety = [ch for ch in safety_text if ord(ch) not in cmap]
        if missing_safety:
            raise SystemExit(f"{path.name}: safety set missing {''.join(missing_safety[:24])}")
        allowed = ALLOWED_MISSING.get(code, set())
        missing = sorted({ord(ch) for ch in requested if ord(ch) not in cmap and ord(ch) not in allowed})
        size = path.stat().st_size
        if size > SIZE_LIMIT:
            raise SystemExit(f"{path.name}: {size} bytes exceeds {SIZE_LIMIT}")
        weight = font["OS/2"].usWeightClass
        print(
            f"{path.name}\t{size} bytes\tglyphs={font['maxp'].numGlyphs}"
            f"\tweight={weight}\tcmap={sorted(formats)}\tholes={len(missing)}"
        )
        if missing:
            preview = " ".join(f"U+{cp:04X}" for cp in missing[:16])
            print(f"  source has no glyph for {preview}")
        return size
    finally:
        font.close()


def retire() -> None:
    for stem in RETIRED:
        for folder, ext in ((FONTS, ".ttf"), (BYTES, ".bytes")):
            path = folder / (stem + ext)
            meta = path.with_name(path.name + ".meta")
            if path.is_file():
                path.unlink()
                print(f"removed {path}")
            if meta.is_file():
                meta.unlink()
                print(f"removed {meta}")


def main() -> int:
    if not SUBSET.is_file():
        print(f"missing {SUBSET}", file=sys.stderr)
        return 1
    safety = load_safety()
    BYTES.mkdir(parents=True, exist_ok=True)
    for code, stem, source_name, _weight, endonym in FACES:
        src = DL / source_name
        if not src.is_file():
            print(f"missing source font {src}", file=sys.stderr)
            return 1
        text = collect(code, safety)
        dest = FONTS / f"{stem}.ttf"
        print(f"subset {code} {len(set(text))} code points -> {dest.name}")
        subset_one(src, text, dest)
        must = safety[code] + (kana() if code == "ja" else "")
        check_font(dest, code, endonym, must, text)
        blob = BYTES / f"{stem}.bytes"
        blob.write_bytes(dest.read_bytes())
        write_meta(dest, METAS[dest.name], "ttf")
        write_meta(blob, METAS[blob.name], "bytes")
    retire()
    print("make_cjk_fonts done")
    return 0


if __name__ == "__main__":
    sys.exit(main())
