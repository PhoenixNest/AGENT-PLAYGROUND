#!/usr/bin/env python3
"""Lists the tiles of an image pair with the most out-of-tolerance pixels and writes zoomed crops of the worst ones.

usage: hotspots.py NAME [--tile 96] [--top 8] [--unity out/unity] [--ref out/ref|out/web]
"""
import argparse
import os
import numpy as np
from PIL import Image

here = os.path.dirname(os.path.abspath(__file__))
ap = argparse.ArgumentParser()
ap.add_argument("name")
ap.add_argument("--tile", type=int, default=96)
ap.add_argument("--top", type=int, default=8)
ap.add_argument("--unity", default=os.path.join(here, "out", "unity"))
ap.add_argument("--ref", default=None)
ap.add_argument("--zoom", type=int, default=4)
args = ap.parse_args()
base = args.name.split("~")[0]
ref_dir = args.ref
if ref_dir is None:
    ref_dir = os.path.join(here, "out", "ref") if os.path.exists(os.path.join(here, "out", "ref", base + ".png")) else os.path.join(here, "out", "web")


def comp(p):
    a = np.asarray(Image.open(p).convert("RGBA")).astype(np.float64)
    al = a[..., 3:4] / 255
    return np.rint(a[..., :3] * al + 128 * (1 - al)).astype(np.uint8)


ref = comp(os.path.join(ref_dir, base + ".png"))
test = comp(os.path.join(args.unity, args.name + ".png"))
worst = np.abs(ref.astype(int) - test.astype(int)).max(axis=2)
bad = worst > 16
T = args.tile
H, W = bad.shape
tiles = []
for y in range(0, H, T):
    for x in range(0, W, T):
        c = int(bad[y:y + T, x:x + T].sum())
        if c:
            tiles.append((c, x, y, int(worst[y:y + T, x:x + T].max())))
tiles.sort(reverse=True)
print(f"total bad {int(bad.sum())} of {bad.size} ({100 * bad.mean():.3f}%)")
out = os.path.join(here, "out", "compare")
for i, (c, x, y, m) in enumerate(tiles[:args.top]):
    print(f"tile x={x} y={y} bad={c} max={m}")
    box = (x, y, min(x + T, W), min(y + T, H))
    a = Image.fromarray(ref).crop(box).resize((T * args.zoom, T * args.zoom), Image.NEAREST)
    b = Image.fromarray(test).crop(box).resize((T * args.zoom, T * args.zoom), Image.NEAREST)
    d = Image.fromarray(np.clip(worst * 8, 0, 255).astype(np.uint8)).crop(box).resize((T * args.zoom, T * args.zoom), Image.NEAREST).convert("RGB")
    im = Image.new("RGB", (T * args.zoom * 3 + 8, T * args.zoom), (60, 60, 60))
    im.paste(a, (0, 0))
    im.paste(b, (T * args.zoom + 4, 0))
    im.paste(d, (2 * T * args.zoom + 8, 0))
    im.save(os.path.join(out, f"hot_{base}_{i}.png"))
