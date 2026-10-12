#!/usr/bin/env python3
"""Narrow Nunito's U+02BB so Uzbek oʻ / gʻ sits on the letter.

Input is a static glyf font that already contains U+02BB (make_fonts.sh
instances Nunito[wght].ttf and subsets to that one character first).

Nunito draws uni02BB on a 500-unit advance with side bearings near 180 and
no GPOS pair. Fredoka has no U+02BB, and the outline renderer only kerns
glyphs that come from the same font, so the mark falls through to Nunito
and opens a gap on both sides.

This script shifts the outline left until the left side bearing is 24
(or less, if that is what keeps the advance under 38% of the reference
letter 'o') and sets the advance to the ink width plus that bearing plus
a short right bearing. The result is one glyph. Other letters stay in
Fredoka or Nunito.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from fontTools.ttLib import TTFont

MARK = 0x02BB
TARGET_LSB = 24
TARGET_RSB = 28
# Stay under the test's 40% cap with a little room.
ADVANCE_RATIO = 0.38


def _glyph_name(font: TTFont, cp: int) -> str:
    cmap = font.getBestCmap() or {}
    name = cmap.get(cp)
    if not name:
        raise SystemExit(f"U+{cp:04X} missing from {font}")
    return name


def _shift_and_set(font: TTFont, letter_font: TTFont) -> tuple[int, int, int]:
    glyf = font["glyf"]
    hmtx = font["hmtx"]
    gname = _glyph_name(font, MARK)
    glyph = glyf[gname]
    if glyph.isComposite():
        raise SystemExit("U+02BB is a composite; this script only shifts simple glyf outlines")
    letter = _glyph_name(letter_font, ord("o"))
    letter_adv = letter_font["hmtx"][letter][0]
    cap = int(letter_adv * ADVANCE_RATIO)

    delta = glyph.xMin - TARGET_LSB
    glyph.coordinates.translate((-delta, 0))
    glyph.xMin -= delta
    glyph.xMax -= delta

    lsb = glyph.xMin
    rsb = TARGET_RSB
    ink = glyph.xMax - glyph.xMin
    advance = lsb + ink + rsb
    if advance > cap:
        overflow = advance - cap
        take = min(rsb, overflow)
        rsb -= take
        overflow -= take
        if overflow > 0:
            lsb -= overflow
            glyph.coordinates.translate((-overflow, 0))
            glyph.xMin -= overflow
            glyph.xMax -= overflow
        advance = lsb + (glyph.xMax - glyph.xMin) + rsb
    if advance >= int(letter_adv * 0.4):
        raise SystemExit(f"advance {advance} is not under 40% of o ({letter_adv})")
    hmtx[gname] = (advance, lsb)
    for tag in ("fpgm", "prep", "cvt ", "hdmx", "LTSH"):
        if tag in font:
            del font[tag]
    return advance, lsb, letter_adv


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--font", required=True, type=Path)
    parser.add_argument("--letter-font", required=True, type=Path)
    parser.add_argument("--out", required=True, type=Path)
    args = parser.parse_args()
    font = TTFont(args.font)
    letter_font = TTFont(args.letter_font)
    if "glyf" not in font or "fvar" in font:
        raise SystemExit("need a static glyf font")
    advance, lsb, letter_adv = _shift_and_set(font, letter_font)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    font.save(args.out)
    font.close()
    letter_font.close()
    print(
        f"{args.out.name}\tadvance={advance}\tlsb={lsb}\t"
        f"letter_o={letter_adv}\tratio={advance / letter_adv:.3f}\tbytes={args.out.stat().st_size}"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
