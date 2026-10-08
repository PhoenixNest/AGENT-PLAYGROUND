#!/usr/bin/env python3
"""Montage of web | unity side by side at 1/4 scale for a fixed set of states.

Usage (from games/maze-puzzle/tools/parity):
    python diff/montage.py --web out/web --unity out/unity --report <report.json> \
        --out out/reports/montage

Writes montage_zero.png and montage_iphone.png (one grid per profile; each cell: label with pixel % and SSIM
from the report, then web on the left and unity on the right).
"""
import argparse
import json
import os

from PIL import Image, ImageDraw, ImageFont

STATES = [
    "home_default",
    "maps_world_1",
    "daily_default",
    "shop_rings_default",
    "achievements_some",
    "settings_default",
    "modal_debrief_exact",
    "game_lvl01_t1000ms",
]
SCALE = 4
COLUMNS = 4
GAP = 12
LABEL_H = 64
BG = (24, 24, 32)


def load_font(size):
    for name in ("arial.ttf", "segoeui.ttf", "DejaVuSans.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--web", required=True)
    ap.add_argument("--unity", required=True)
    ap.add_argument("--report", required=True)
    ap.add_argument("--out", required=True)
    args = ap.parse_args()

    report = json.load(open(args.report, encoding="utf-8"))
    pairs = {p["key"]: p for p in report["pairs"]}
    os.makedirs(args.out, exist_ok=True)
    font = load_font(22)
    small = load_font(18)

    for profile in ("zero", "iphone"):
        cells = []
        for sid in STATES:
            key = f"{sid}__{profile}"
            w = Image.open(os.path.join(args.web, key + ".png")).convert("RGB")
            u = Image.open(os.path.join(args.unity, key + ".png")).convert("RGB")
            size = (w.width // SCALE, w.height // SCALE)
            w = w.resize(size, Image.LANCZOS)
            u = u.resize(size, Image.LANCZOS)
            cell = Image.new("RGB", (size[0] * 2 + GAP, size[1] + LABEL_H), BG)
            d = ImageDraw.Draw(cell)
            p = pairs.get(key)
            stat = "no report row" if p is None else f"pixels {p['pixel_pct']:.2f} %   SSIM {p['ssim']:.4f}   {p['status']}"
            d.text((4, 4), key, fill=(255, 255, 255), font=font)
            d.text((4, 32), stat, fill=(255, 210, 90), font=small)
            d.text((size[0] - 60, 34), "web", fill=(140, 220, 140), font=small)
            d.text((size[0] * 2 + GAP - 70, 34), "unity", fill=(140, 180, 255), font=small)
            cell.paste(w, (0, LABEL_H))
            cell.paste(u, (size[0] + GAP, LABEL_H))
            cells.append(cell)

        cw, ch = cells[0].size
        rows = (len(cells) + COLUMNS - 1) // COLUMNS
        sheet = Image.new("RGB", (COLUMNS * cw + (COLUMNS + 1) * GAP, rows * ch + (rows + 1) * GAP), BG)
        for i, cell in enumerate(cells):
            r, c = divmod(i, COLUMNS)
            sheet.paste(cell, (GAP + c * (cw + GAP), GAP + r * (ch + GAP)))
        path = os.path.join(args.out, f"montage_{profile}.png")
        sheet.save(path)
        print(path, sheet.size)


if __name__ == "__main__":
    main()
