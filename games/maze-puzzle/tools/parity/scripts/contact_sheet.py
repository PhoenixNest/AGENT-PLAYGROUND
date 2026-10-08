import argparse
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
THUMB_W = 172
COLS = 14
LABEL_H = 30
PAD = 8
BG = (24, 24, 28)


def wrap(label, width=27):
    parts = label.split("_")
    lines, cur = [], ""
    for p in parts:
        nxt = f"{cur}_{p}" if cur else p
        if len(nxt) > width and cur:
            lines.append(cur)
            cur = p
        else:
            cur = nxt
    lines.append(cur)
    return lines[:2]


def build(directory, profile, order):
    keys = [k for k in order if (directory / f"{k}__{profile}.png").exists()]
    if not keys:
        return None
    with Image.open(directory / f"{keys[0]}__{profile}.png") as probe:
        thumb_h = round(THUMB_W * probe.height / probe.width)
    rows = (len(keys) + COLS - 1) // COLS
    cell_w, cell_h = THUMB_W + PAD, thumb_h + LABEL_H + PAD
    sheet = Image.new("RGB", (COLS * cell_w + PAD, rows * cell_h + PAD), BG)
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default(size=11)
    for i, key in enumerate(keys):
        x = PAD + (i % COLS) * cell_w
        y = PAD + (i // COLS) * cell_h
        with Image.open(directory / f"{key}__{profile}.png") as im:
            thumb = im.convert("RGB").resize((THUMB_W, thumb_h), Image.LANCZOS)
        sheet.paste(thumb, (x, y + LABEL_H))
        for n, line in enumerate(wrap(key)):
            draw.text((x, y + n * 13), line, fill=(235, 235, 235), font=font)
    return sheet, len(keys)


def main():
    p = argparse.ArgumentParser(description="Labelled thumbnail contact sheets per profile")
    p.add_argument("--dir", default="out/web")
    p.add_argument("--states", default="states.json")
    args = p.parse_args()
    directory = Path(args.dir)
    directory = directory if directory.is_absolute() else ROOT / directory
    states_path = Path(args.states)
    states_path = states_path if states_path.is_absolute() else ROOT / states_path
    order = [s["id"] for s in json.loads(states_path.read_text(encoding="utf-8"))]
    status = 0
    for profile in ("zero", "iphone"):
        built = build(directory, profile, order)
        if built is None:
            print(f"no captures for profile {profile}", file=sys.stderr)
            status = 1
            continue
        sheet, n = built
        out = directory / f"contact_sheet_{profile}.png"
        sheet.save(out, compress_level=6)
        print(f"{out} ({n} states, {sheet.width}x{sheet.height})")
    return status


if __name__ == "__main__":
    sys.exit(main())
