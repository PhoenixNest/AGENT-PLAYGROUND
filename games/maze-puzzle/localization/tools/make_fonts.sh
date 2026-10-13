#!/usr/bin/env bash
# Rebuild the outline faces shipped for the font registry.
# Downloads live in /tmp/maze-i18n/fonts/dl (see localization/spikes/fonts-per-language.md).
# CJK subsets are make_cjk_fonts.py (table + safety set). Do not commit variable fonts.
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../../../.." && pwd)"
PY="${PY:-/tmp/maze-i18n/fonts-venv/bin/python}"
DL="${DL:-/tmp/maze-i18n/fonts/dl}"
SUB="$HERE/subset_fonts.py"
FONTS="$ROOT/games/maze-puzzle/unity/maze-puzzle/Assets/Fonts"
BYTES="$ROOT/games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Fonts/Ttf"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

if [[ ! -x "$PY" ]]; then
  echo "missing $PY (python -m venv /tmp/maze-i18n/fonts-venv && pip install fonttools brotli)" >&2
  exit 1
fi

# CJK subsets: table characters (if the json exists) plus ASCII, punctuation,
# the endonym, and cjk_safety.json. A missing translation table is fine.
"$PY" "$HERE/make_cjk_fonts.py"

cp -f "$DL/licenses/ZCOOLKuaiLe-OFL.txt" "$FONTS/ZCOOLKuaiLe-OFL.txt"
cp -f "$DL/licenses/Huninn-OFL.txt" "$FONTS/Huninn-OFL.txt"
cp -f "$DL/licenses/Jua-OFL.txt" "$FONTS/Jua-OFL.txt"
# Google Fonts ships no OFL.txt for M PLUS Rounded 1c. Name IDs 0 and 13 say OFL 1.1.
# Copyright line is name ID 0. The body is the standard OFL 1.1 text.
python3 - "$DL/licenses/Jua-OFL.txt" "$FONTS/MPLUSRounded1c-OFL.txt" << 'PY'
import pathlib, sys
src, dest = sys.argv[1], sys.argv[2]
text = pathlib.Path(src).read_text(encoding="utf-8")
body = text.split("-----------------------------------------------------------", 1)[1]
body = "-----------------------------------------------------------" + body
header = (
    "Copyright 2016 The Rounded M+ Project Authors.\n"
    "\n"
    "This Font Software is licensed under the SIL Open Font License, Version 1.1.\n"
    "This license is copied below, and is also available with a FAQ at:\n"
    "http://scripts.sil.org/OFL\n"
    "\n"
    "\n"
)
pathlib.Path(dest).write_text(header + body, encoding="utf-8")
PY

# Uzbek modifier. Fredoka has no U+02BB. Nunito's glyph is advance 500 with
# ~180 side bearings and no kern pair, so oʻ / gʻ draws as a detached apostrophe.
# These four files contain only the corrected mark. latin-uz puts the matching
# weight first. Display 600/700 follow Fredoka's face weight. Body 700/800/900
# follow Nunito. 700 is checked against Fredoka-700 'o', the tighter of the two.
PATCH="$HERE/patch_uz_modifier.py"
NUNITO_VAR="$DL/Nunito[wght].ttf"
patch_mark() {
  local weight="$1" letter="$2"
  echo "uz patch $weight"
  "$PY" "$SUB" --font "$NUNITO_VAR" --variations "wght=$weight" \
    --unicodes U+02BB --keep-os2 --out "$WORK/nunito-$weight.ttf"
  "$PY" "$PATCH" --font "$WORK/nunito-$weight.ttf" --letter-font "$letter" \
    --out "$FONTS/Nunito-uz-patch-$weight.ttf"
  cp -f "$FONTS/Nunito-uz-patch-$weight.ttf" "$BYTES/Nunito-uz-patch-$weight.bytes"
}
patch_mark 600 "$FONTS/Fredoka-600.ttf"
patch_mark 700 "$FONTS/Fredoka-700.ttf"
patch_mark 800 "$FONTS/Nunito-800.ttf"
patch_mark 900 "$FONTS/Nunito-900.ttf"
cp -f "$DL/licenses/Nunito-OFL.txt" "$FONTS/Nunito-uz-patch-OFL.txt"

echo "make_fonts done"
