#!/usr/bin/env python3
"""Build the pinned glyf instances for Hindi, Khmer, and Arabic.

One instance per script, CSS weight 700.

- Baloo 2's axis is 400–800. It has no 900.
- Kantumruy Pro's axis tops out at 700.
- Cairo can go to 1000, but the manifest lists a single 700 face, and
  Fredoka already maps CSS 800 and 900 onto 700.

``LocFonts.MatchWeight`` therefore sends button and title CSS weights
600–900, and floating combat text at 900, to this one instance. Advances
in the shaped table are baked from that instance. A second file is not
shipped.

Glyph ids are retained (``--retain-gids``) and the bake shapes this same
file, so the ids in the table match the bytes under Resources.

Rebuild, from the repo root or from anywhere:

  /tmp/maze-i18n/shaping/venv/bin/python \\
      games/maze-puzzle/localization/tools/build_shaped_fonts.py
"""

from __future__ import annotations

import shutil
import subprocess
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]
UNITY = REPO / "games" / "maze-puzzle" / "unity" / "maze-puzzle"
FONTS = UNITY / "Assets" / "Fonts"
BYTES = UNITY / "Assets" / "Resources" / "Fonts" / "Ttf"
SUB = HERE / "subset_fonts.py"
DL = Path("/tmp/maze-i18n/fonts/dl")

# Stable Unity meta guids. Do not regenerate on rebuild.
METAS = {
    "Baloo2-700.ttf": "4c8e1a0b7d2f4e6a9b1c3d5e7f8091a2",
    "Baloo2-700.bytes": "5d9f2b1c8e304f7bac2d4e6f8091a2b3",
    "Baloo2-OFL.txt": "6e0a3c2d9f41508c1d3e5f708192a3b4",
    "Cairo-700.ttf": "7f1b4d3e0a52619d2e4f608193a4b5c6",
    "Cairo-700.bytes": "802c5e4f1b6372ae3f506192a4b5c6d7",
    "Cairo-OFL.txt": "913d6f502c7483bf406172a5b6c7d8e9",
    "KantumruyPro-700.ttf": "a24e70613d8594c0517283b6c7d8e9f0",
    "KantumruyPro-700.bytes": "b35f81724e96a5d1628394c7d8e9f0a1",
    "KantumruyPro-OFL.txt": "c46092835fa7b6e27394a5d8e9f0a1b2",
}

JOBS = [
    {
        "src": "Baloo2[wght].ttf",
        "variations": "wght=700",
        "unicodes": "U+0020-007E,U+00A0,U+0900-097F,U+1CD0-1CFF,U+200C-200D,U+25CC,U+A8E0-A8FF",
        "out": "Baloo2-700.ttf",
        "ofl": "Baloo2-OFL.txt",
    },
    {
        "src": "KantumruyPro[wght].ttf",
        "variations": "wght=700",
        "unicodes": "U+0020-007E,U+00A0,U+1780-17FF,U+19E0-19FF,U+200C-200D,U+25CC",
        "out": "KantumruyPro-700.ttf",
        "ofl": "KantumruyPro-OFL.txt",
    },
    {
        "src": "Cairo[slnt,wght].ttf",
        "variations": "wght=700,slnt=0",
        "unicodes": "U+0020-007E,U+00A0,U+0600-06FF,U+0750-077F,U+08A0-08FF,U+200C-200D,U+FB50-FDFF,U+FE70-FEFF",
        "out": "Cairo-700.ttf",
        "ofl": "Cairo-OFL.txt",
    },
]


def write_meta(path: Path, guid: str, importer: str) -> None:
    meta = path.with_name(path.name + ".meta")
    if meta.is_file():
        return
    if importer == "font":
        body = (
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
    else:
        body = (
            "fileFormatVersion: 2\n"
            f"guid: {guid}\n"
            "TextScriptImporter:\n"
            "  externalObjects: {}\n"
            "  userData: \n"
            "  assetBundleName: \n"
            "  assetBundleVariant: \n"
        )
    meta.write_text(body, encoding="utf-8")


def main() -> int:
    py = sys.executable
    FONTS.mkdir(parents=True, exist_ok=True)
    BYTES.mkdir(parents=True, exist_ok=True)
    for job in JOBS:
        src = DL / job["src"]
        if not src.is_file():
            print(f"missing download {src}", file=sys.stderr)
            return 1
        dest = FONTS / job["out"]
        cmd = [
            py,
            str(SUB),
            "--font",
            str(src),
            "--variations",
            job["variations"],
            "--unicodes",
            job["unicodes"],
            "--layout",
            "keep",
            "--keep-os2",
            "--retain-gids",
            "--out",
            str(dest),
        ]
        print(" ".join(cmd))
        subprocess.check_call(cmd)
        shutil.copyfile(DL / "licenses" / job["ofl"], FONTS / job["ofl"])
        raw = dest.read_bytes()
        (BYTES / (dest.stem + ".bytes")).write_bytes(raw)
        write_meta(dest, METAS[job["out"]], "font")
        write_meta(FONTS / job["ofl"], METAS[job["ofl"]], "text")
        write_meta(BYTES / (dest.stem + ".bytes"), METAS[dest.stem + ".bytes"], "text")
        print(f"{job['out']}\t{len(raw)} bytes")
    return 0


if __name__ == "__main__":
    sys.exit(main())
