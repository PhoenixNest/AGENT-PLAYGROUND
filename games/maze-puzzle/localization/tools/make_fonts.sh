#!/usr/bin/env bash
# Rebuild the outline faces shipped for the font registry.
# Downloads live in /tmp/maze-i18n/fonts/dl (see localization/spikes/fonts-per-language.md).
# CJK files here are temporary endonym subsets. A later wave replaces the file bytes
# (or the manifest filename) for the same FontGroup. Do not commit variable fonts.
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

# Printable ASCII (digits, punctuation, and letters) plus one endonym.
# Letters are included so a Latin token in the same run still has a glyph.
ascii="$(python3 -c 'print("".join(chr(c) for c in range(0x20, 0x7F)))')"

subset() {
  local src="$1" extra="$2" dest="$3"
  printf '%s%s' "$ascii" "$extra" > "$WORK/chars.txt"
  echo "subset $dest"
  "$PY" "$SUB" --font "$src" --text-file "$WORK/chars.txt" --keep-os2 --out "$dest"
}

subset "$DL/ZCOOLKuaiLe-Regular.ttf" "简体中文" "$FONTS/ZCOOLKuaiLe-endonym.ttf"
subset "$DL/Huninn-Regular.ttf" "繁體中文" "$FONTS/Huninn-endonym.ttf"
subset "$DL/MPLUSRounded1c-Bold.ttf" "日本語" "$FONTS/MPLUSRounded1c-endonym.ttf"
subset "$DL/Jua-Regular.ttf" "한국어" "$FONTS/Jua-endonym.ttf"

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

mkdir -p "$BYTES"
for name in ZCOOLKuaiLe-endonym Huninn-endonym MPLUSRounded1c-endonym Jua-endonym; do
  cp -f "$FONTS/$name.ttf" "$BYTES/$name.bytes"
done

"$PY" - "$FONTS" << 'PY'
import sys
from pathlib import Path
from fontTools.ttLib import TTFont
root = Path(sys.argv[1])
need = {
    "ZCOOLKuaiLe-endonym.ttf": "简体中文",
    "Huninn-endonym.ttf": "繁體中文",
    "MPLUSRounded1c-endonym.ttf": "日本語",
    "Jua-endonym.ttf": "한국어",
}
for name, text in need.items():
    path = root / name
    font = TTFont(path)
    cmap = font.getBestCmap() or {}
    missing = [ch for ch in text if ord(ch) not in cmap]
    print(f"{name}\t{path.stat().st_size}\tglyf={'glyf' in font}\tfvar={'fvar' in font}\tmissing={''.join(missing)}")
    if missing or "glyf" not in font or "fvar" in font:
        raise SystemExit("bad subset " + name)
    font.close()
PY
echo "make_fonts done"
