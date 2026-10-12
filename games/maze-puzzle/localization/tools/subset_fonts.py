#!/usr/bin/env python3
"""Subset a font for the Orbital Salvage outline renderer.

The Unity loader (Assets/Scripts/Render/TrueTypeFont.cs) draws TrueType
``glyf`` outlines only. It reads cmap formats 4 and 12, and it does not
read CFF, ``fvar``, or GSUB scripts other than ``latn`` / ``DFLT``.
Variable fonts must be pinned with ``--variations``. CFF/OpenType-CFF
fonts must be converted with ``--cff-to-ttf`` (quadratic approximation
via fontTools ``Cu2QuPen``) before the loader can open them.

No font binaries are written into the repo by this tool; pass ``--out``.

Examples
--------
  python3 subset_fonts.py --font Baloo2[wght].ttf --variations wght=700 \\
      --text-file latin.txt --out Baloo2-700.subset.ttf

  python3 subset_fonts.py --font NotoSansArabic[wdth,wght].ttf \\
      --variations wdth=100,wght=700 \\
      --unicodes U+0020-007E,U+0600-06FF,U+0750-077F,U+08A0-08FF \\
      --layout keep --out NotoSansArabic-700.subset.ttf

  python3 subset_fonts.py --font SourceSans3-Bold.otf --cff-to-ttf \\
      --unicodes U+0020-007E --out SourceSans3-Bold.glyf.ttf --inspect
"""

from __future__ import annotations

import argparse
import gzip
import subprocess
import sys
import tempfile
from pathlib import Path

from fontTools.pens.cu2quPen import Cu2QuPen
from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.subset import main as subset_main
from fontTools.ttLib import TTFont, newTable


# Tables the loader never reads. GSUB/GPOS/GDEF are added only for --layout drop.
# OS/2 is dropped unless --keep-os2: the loader uses it for typo metrics, and the
# table is tiny, so shipping builds should pass --keep-os2.
_ALWAYS_DROP = (
    "DSIG",
    "meta",
    "name",
    "post",
    "gasp",
    "kern",
    "STAT",
    "BASE",
    "JSTF",
    "vhea",
    "vmtx",
    "VORG",
    "hdmx",
    "LTSH",
    "CFF ",
    "CFF2",
)
_LAYOUT_TABLES = ("GSUB", "GPOS", "GDEF")


def _parse_unicodes(text: str) -> list[int]:
    out: list[int] = []
    for part in text.replace(";", ",").split(","):
        part = part.strip()
        if not part or part.startswith("#"):
            continue
        part = part.replace("U+", "").replace("u+", "").replace("0x", "").replace("0X", "")
        if "-" in part:
            a, b = part.split("-", 1)
            start, end = int(a, 16), int(b, 16)
            if end < start:
                raise argparse.ArgumentTypeError(f"bad range U+{a}-U+{b}")
            out.extend(range(start, end + 1))
        else:
            out.append(int(part, 16))
    return out


def _codepoints_from_text(text: str) -> list[int]:
    return sorted({ord(ch) for ch in text if ch not in "\n\r"})


def _load_requested(args: argparse.Namespace) -> list[int]:
    cps: set[int] = set()
    if args.unicodes:
        cps.update(_parse_unicodes(args.unicodes))
    if args.unicodes_file:
        raw = Path(args.unicodes_file).read_text(encoding="utf-8")
        for line in raw.splitlines():
            line = line.split("#", 1)[0].strip()
            if line:
                cps.update(_parse_unicodes(line))
    if args.text_file:
        cps.update(_codepoints_from_text(Path(args.text_file).read_text(encoding="utf-8")))
    if args.text:
        cps.update(_codepoints_from_text(args.text))
    return sorted(cps)


def _variation_map(font: TTFont, spec: str | None, *, required: bool) -> dict[str, float]:
    """Pin every axis. Unspecified axes stay at their default (upright, normal width)."""
    if "fvar" not in font:
        if spec:
            raise SystemExit("font has no fvar axis; do not pass --variations")
        return {}
    loc = {axis.axisTag: float(axis.defaultValue) for axis in font["fvar"].axes}
    if not spec:
        if required:
            # A variable font left unpinned draws the default master in our loader
            # (fvar/gvar are ignored).
            raise SystemExit(
                "variable font: pass --variations (for example wght=700). "
                "The loader ignores fvar and would draw the default master."
            )
        return {}
    for item in spec.split(","):
        item = item.strip()
        if not item:
            continue
        if "=" not in item:
            raise SystemExit(f"bad --variations item {item!r} (want tag=value)")
        tag, value = item.split("=", 1)
        tag = tag.strip()
        if tag not in loc:
            raise SystemExit(f"font has no axis {tag!r}; has {sorted(loc)}")
        loc[tag] = float(value)
    return loc


def cff_to_glyf(font: TTFont, max_err: float = 1.0) -> None:
    """Replace a CFF table with quadratic glyf outlines. max_err is in font units."""
    if font.sfntVersion != "OTTO" or "CFF " not in font:
        raise SystemExit(" --cff-to-ttf requires an OTTO/CFF font (no glyf yet)")
    glyph_order = font.getGlyphOrder()
    font["loca"] = newTable("loca")
    glyf = newTable("glyf")
    glyf.glyphOrder = glyph_order
    quads = {}
    glyphs = font.getGlyphSet()
    for name in glyph_order:
        pen = TTGlyphPen(glyphs)
        glyphs[name].draw(Cu2QuPen(pen, max_err, reverse_direction=True))
        quads[name] = pen.glyph()
    glyf.glyphs = quads
    font["glyf"] = glyf
    del font["CFF "]
    glyf.compile(font)

    hmtx = font["hmtx"]
    for name, glyph in glyf.glyphs.items():
        if hasattr(glyph, "xMin"):
            hmtx[name] = (hmtx[name][0], glyph.xMin)

    maxp = newTable("maxp")
    maxp.tableVersion = 0x00010000
    maxp.maxZones = 1
    maxp.maxTwilightPoints = 0
    maxp.maxStorage = 0
    maxp.maxFunctionDefs = 0
    maxp.maxInstructionDefs = 0
    maxp.maxStackElements = 0
    maxp.maxSizeOfInstructions = 0
    components = [
        len(g.components) if getattr(g, "isComposite", lambda: False)() else 0
        for g in glyf.glyphs.values()
    ]
    maxp.maxComponentElements = max(components) if components else 0
    maxp.maxComponentDepth = 1 if maxp.maxComponentElements else 0
    font["maxp"] = maxp
    maxp.compile(font)

    if "post" in font:
        post = font["post"]
        post.formatType = 3.0
        post.extraNames = []
        post.mapping = {}
        post.glyphOrder = glyph_order
    font.sfntVersion = "\x00\x01\x00\x00"


def _script_tags(font: TTFont, table: str) -> list[str]:
    if table not in font:
        return []
    records = font[table].table.ScriptList.ScriptRecord
    return [rec.ScriptTag for rec in records]


def _cmap_formats(font: TTFont) -> list[str]:
    seen: list[str] = []
    if "cmap" not in font:
        return seen
    for sub in font["cmap"].tables:
        label = f"{sub.format}(plat{sub.platformID}/enc{sub.platEncID})"
        if label not in seen:
            seen.append(label)
    return seen


def _instantiate(src: Path, variations: dict[str, float], dest: Path) -> None:
    """Pin a variable font to a static instance. pyftsubset has no --variations flag."""
    cmd = [
        sys.executable,
        "-m",
        "fontTools.varLib.instancer",
        "--static",
        "-o",
        str(dest),
        str(src),
    ]
    cmd.extend(f"{tag}={value:g}" for tag, value in variations.items())
    print("command:")
    print(" ".join(cmd))
    subprocess.check_call(cmd)


def describe(path: Path) -> dict:
    font = TTFont(path, lazy=False)
    data = path.read_bytes()
    packed = gzip.compress(data, compresslevel=9)
    # GlyphOrder is synthesized by fontTools on load; report on-disk tables only.
    tables = sorted(font.reader.tables)
    info = {
        "path": str(path),
        "bytes": len(data),
        "gzip": len(packed),
        "sfnt": font.sfntVersion,
        "glyphs": int(font["maxp"].numGlyphs) if "maxp" in font else 0,
        "has_glyf": "glyf" in font,
        "has_cff": "CFF " in font or "CFF2" in font,
        "variable": "fvar" in font,
        "cmap": _cmap_formats(font),
        "tables": tables,
        "gsub_scripts": _script_tags(font, "GSUB"),
        "gpos_scripts": _script_tags(font, "GPOS"),
    }
    font.close()
    return info


def _print_info(info: dict, missing: list[int] | None) -> None:
    kb = info["bytes"] / 1024
    gzip_kb = info["gzip"] / 1024
    print(f"file: {info['path']}")
    print(f"sfnt: {info['sfnt']!r}  glyf: {info['has_glyf']}  cff: {info['has_cff']}  fvar: {info['variable']}")
    print(f"glyphs: {info['glyphs']}")
    print(f"ttf: {info['bytes']} bytes ({kb:.1f} KB)")
    print(f"gzip: {info['gzip']} bytes ({gzip_kb:.1f} KB)")
    print(f"cmap: {', '.join(info['cmap']) or '(none)'}")
    print(f"GSUB scripts: {', '.join(info['gsub_scripts']) or '(none)'}")
    print(f"GPOS scripts: {', '.join(info['gpos_scripts']) or '(none)'}")
    print("tables: " + " ".join(info["tables"]))
    if missing is not None:
        preview = " ".join(f"U+{cp:04X}" for cp in missing[:24])
        extra = "" if len(missing) <= 24 else f" … +{len(missing) - 24}"
        print(f"missing codepoints: {len(missing)} {preview}{extra}")
    print(
        "STATS\t{bytes}\t{gzip}\t{glyphs}\t{cmap}\t{gsub}\t{gpos}\t{missing}".format(
            bytes=info["bytes"],
            gzip=info["gzip"],
            glyphs=info["glyphs"],
            cmap=",".join(info["cmap"]),
            gsub=",".join(info["gsub_scripts"]),
            gpos=",".join(info["gpos_scripts"]),
            missing=0 if missing is None else len(missing),
        )
    )


def _missing(path: Path, cps: list[int]) -> list[int]:
    font = TTFont(path, lazy=True)
    cmap = font.getBestCmap() or {}
    missing = [cp for cp in cps if cp not in cmap]
    font.close()
    return missing


def build_subset_args(font_path: Path, out_path: Path, args: argparse.Namespace, variations: dict[str, float]) -> list[str]:
    cmd = [
        str(font_path),
        f"--output-file={out_path}",
        "--no-hinting",
        "--ignore-missing-unicodes",
        "--notdef-glyph",
        "--notdef-outline",
        "--recommended-glyphs",
        "--no-glyph-names",
        "--no-legacy-cmap",
        "--no-symbol-cmap",
    ]
    if args.unicodes:
        cmd.append(f"--unicodes={args.unicodes}")
    if args.unicodes_file:
        cmd.append(f"--unicodes-file={args.unicodes_file}")
    if args.text_file:
        cmd.append(f"--text-file={args.text_file}")
    if args.text:
        cmd.append(f"--text={args.text}")
    if variations:
        spec = ",".join(f"{tag}={value:g}" for tag, value in variations.items())
        cmd.append(f"--variations={spec}")
    if args.retain_gids:
        cmd.append("--retain-gids")
    if args.layout == "keep":
        cmd.append("--layout-features=*")
    else:
        # Empty feature list plus an explicit table drop. Closure would otherwise
        # keep alternate glyphs that this loader cannot shape (latn/DFLT only).
        cmd.append("--layout-features=")
        cmd.append("--no-layout-closure")
    drop = list(_ALWAYS_DROP)
    if args.layout != "keep":
        drop.extend(_LAYOUT_TABLES)
    if not args.keep_os2:
        drop.append("OS/2")
    # pyftsubset wants comma-separated tags. Tags that contain a slash (OS/2)
    # are accepted as a single token.
    cmd.append("--drop-tables+=" + ",".join(drop))
    return cmd


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--font", required=True, help="input .ttf/.otf")
    parser.add_argument("--out", help="output .ttf (required unless --inspect and no charset)")
    parser.add_argument("--text-file", help="UTF-8 file; every code point except CR/LF is kept")
    parser.add_argument("--text", help="literal characters to keep")
    parser.add_argument("--unicodes", help="hex code points and ranges, e.g. U+0020-007E,U+0600-06FF")
    parser.add_argument("--unicodes-file", help="text file of hex code points / ranges (# comments ok)")
    parser.add_argument("--variations", help="axis pins, e.g. wght=700 or wdth=100,wght=700")
    parser.add_argument("--layout", choices=("drop", "keep"), default="drop",
                        help="keep GSUB/GPOS/GDEF (ar/hi/km) or drop them (default)")
    parser.add_argument("--retain-gids", action="store_true",
                        help="keep source glyph ids (larger; only useful if pre-shaped ids must match)")
    parser.add_argument("--keep-os2", action="store_true",
                        help="keep the OS/2 table (typo ascender/descender; recommended for shipping)")
    parser.add_argument("--cff-to-ttf", action="store_true",
                        help="convert CFF outlines to quadratic glyf before subsetting")
    parser.add_argument("--cff-max-error", type=float, default=1.0,
                        help="cu2qu error in font units (default 1)")
    parser.add_argument("--inspect", action="store_true", help="print tables, cmap, gzip size")
    args = parser.parse_args(argv)

    src = Path(args.font)
    if not src.is_file():
        print(f"not a file: {src}", file=sys.stderr)
        return 1

    charset = any((args.text_file, args.text, args.unicodes, args.unicodes_file))
    if not charset and not args.inspect:
        print("pass a charset (--text-file / --unicodes / …) or --inspect", file=sys.stderr)
        return 1
    if charset and not args.out:
        print("--out is required when subsetting", file=sys.stderr)
        return 1

    work = src
    tmp_dir = None
    try:
        probe = TTFont(src, lazy=True)
        variations = _variation_map(probe, args.variations, required=charset)
        is_cff = probe.sfntVersion == "OTTO" or "CFF " in probe or "CFF2" in probe
        probe.close()
        if is_cff and not args.cff_to_ttf:
            print(
                "CFF/OTTO font: the loader requires glyf. Re-run with --cff-to-ttf.",
                file=sys.stderr,
            )
            return 1
        if args.cff_to_ttf:
            font = TTFont(src, lazy=False)
            cff_to_glyf(font, max_err=args.cff_max_error)
            tmp_dir = tempfile.TemporaryDirectory(prefix="subset-cff-")
            converted = Path(tmp_dir.name) / (src.stem + ".glyf.ttf")
            font.save(converted)
            font.close()
            work = converted
            print(f"converted CFF -> glyf: {converted}")

        if variations:
            if tmp_dir is None:
                tmp_dir = tempfile.TemporaryDirectory(prefix="subset-var-")
            pinned = Path(tmp_dir.name) / (src.stem + ".static.ttf")
            _instantiate(work, variations, pinned)
            work = pinned

        if not charset:
            if args.inspect:
                _print_info(describe(work if work != src else src), None)
            return 0

        out = Path(args.out)
        out.parent.mkdir(parents=True, exist_ok=True)
        # Variations are already pinned by varLib.instancer. pyftsubset has no
        # --variations switch, so the subset command sees a static glyf font.
        subset_args = build_subset_args(work, out, args, {})
        printable = "fonttools subset " + " ".join(subset_args)
        print("command:")
        print(printable)
        try:
            subset_main(subset_args)
        except SystemExit as exc:
            code = exc.code
            if code not in (0, None):
                print(f"subset failed with status {code}", file=sys.stderr)
                return int(code) if isinstance(code, int) else 1
        if not out.is_file():
            print("subset produced no output", file=sys.stderr)
            return 1

        cps = _load_requested(args)
        missing = _missing(out, cps) if cps else []
        _print_info(describe(out), missing if cps else None)
    finally:
        if tmp_dir is not None:
            tmp_dir.cleanup()
    return 0


if __name__ == "__main__":
    sys.exit(main())
