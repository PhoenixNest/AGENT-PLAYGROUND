#!/usr/bin/env python3
"""Per-line ink bounding boxes and ink mass of reference vs Unity for text scenes.

usage: textprobe.py SCENE [--bands y0,y1;y0,y1 ...] (device px; default: split by blank rows)
"""
import argparse
import os
import numpy as np
from PIL import Image

MODE = "any"
here = os.path.dirname(os.path.abspath(__file__))


def ink(path, bg):
    a = np.asarray(Image.open(path).convert("RGB")).astype(np.int16)
    if MODE == "white":
        return np.clip(a.min(axis=2) - 90, 0, None).astype(np.float64)
    return np.abs(a - bg).max(axis=2).astype(np.float64)


def bands(mask):
    rows = mask.sum(axis=1) > 0
    out, start = [], None
    for i, r in enumerate(rows):
        if r and start is None:
            start = i
        if not r and start is not None:
            out.append((start, i))
            start = None
    if start is not None:
        out.append((start, len(rows)))
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("scene")
    ap.add_argument("--unity", default=os.path.join(here, "out", "unity"))
    ap.add_argument("--web", default=os.path.join(here, "out", "web"))
    ap.add_argument("--white", action="store_true")
    args = ap.parse_args()
    global MODE
    MODE = "white" if args.white else "any"
    ref = Image.open(os.path.join(args.web, args.scene + ".png")).convert("RGB")
    bg = np.asarray(ref)[0, 0].astype(np.int16)
    r = ink(os.path.join(args.web, args.scene + ".png"), bg)
    u = ink(os.path.join(args.unity, args.scene + ".png"), bg)
    for y0, y1 in bands(r > 8):
        rb, ub = r[y0:y1], u[y0:y1]
        xs = np.where(rb.sum(axis=0) > 0)[0]
        xu = np.where(ub.sum(axis=0) > 0)[0]
        ysu = np.where(u[max(0, y0 - 6):y1 + 6].sum(axis=1) > 0)[0]
        print(f"rows {y0:4d}-{y1:4d} h={y1 - y0:3d}  ref x {xs.min():4d}-{xs.max():4d} w={xs.max() - xs.min() + 1:4d} mass {rb.sum() / 255:9.1f}"
              f" | unity x {xu.min() if len(xu) else -1:4d}-{xu.max() if len(xu) else -1:4d} y {max(0, y0 - 6) + (ysu.min() if len(ysu) else 0)}-{max(0, y0 - 6) + (ysu.max() if len(ysu) else 0)} mass {ub.sum() / 255:9.1f}")


if __name__ == "__main__":
    main()
