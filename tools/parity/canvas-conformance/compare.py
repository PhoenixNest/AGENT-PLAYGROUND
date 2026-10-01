#!/usr/bin/env python3
"""Compare Unity canvas renders against the Chromium references (SPEC section 2 metrics).

usage: compare.py [--unity out/unity] [--scenes out/web] [--streams out/ref] [--out out/compare]
                  [--report REPORT.md] [--filter substr] [--perf out/unity/perf.json]

Both images are straight-alpha PNGs; they are composited over mid grey before comparing.
Metrics per image:
  pix16  percent of pixels whose every RGB channel differs by <= 16 (SPEC bar >= 99.0, target >= 99.5)
  ssim   skimage structural_similarity(channel_axis=2, data_range=255) (SPEC bar >= 0.985, target >= 0.99)
  roi16 / roi_ssim   the same over the bounding box of non-background content (small scenes dilute the full-frame numbers)
  max    largest per-channel difference anywhere; edge_max the largest one on edge pixels of the reference
"""
import argparse
import json
import os
import sys

import numpy as np
from PIL import Image
from skimage.metrics import structural_similarity as ssim
from scipy import ndimage

BG = 128


def load(path):
    im = Image.open(path).convert("RGBA")
    a = np.asarray(im).astype(np.float64)
    alpha = a[..., 3:4] / 255.0
    rgb = a[..., :3] * alpha + BG * (1 - alpha)
    return np.rint(rgb).astype(np.uint8), a[..., 3].astype(np.uint8)


def edge_mask(ref):
    g = ref.astype(np.float64).mean(axis=2)
    gx = ndimage.sobel(g, axis=1)
    gy = ndimage.sobel(g, axis=0)
    mag = np.hypot(gx, gy)
    return ndimage.binary_dilation(mag > 8.0, iterations=1)


def compare(ref_path, test_path, heat_path):
    ref, ref_a = load(ref_path)
    test, test_a = load(test_path)
    if ref.shape != test.shape:
        return {"error": f"size mismatch ref {ref.shape} vs unity {test.shape}"}
    diff = np.abs(ref.astype(np.int16) - test.astype(np.int16))
    adiff = np.abs(ref_a.astype(np.int16) - test_a.astype(np.int16))
    worst = diff.max(axis=2)
    within = worst <= 16
    pix16 = 100.0 * within.mean()
    s = float(ssim(ref, test, channel_axis=2, data_range=255))

    content = (np.abs(ref.astype(np.int16) - BG).max(axis=2) > 0) | (np.abs(test.astype(np.int16) - BG).max(axis=2) > 0)
    # background colour = most common colour of the reference (scenes use a flat fill)
    flat = ref.reshape(-1, 3)
    vals, counts = np.unique(flat[:: max(1, len(flat) // 200000)], axis=0, return_counts=True)
    bgc = vals[counts.argmax()]
    content = (np.abs(ref.astype(np.int16) - bgc).max(axis=2) > 2) | (np.abs(test.astype(np.int16) - bgc).max(axis=2) > 2)
    if content.any():
        ys, xs = np.where(content)
        y0, y1, x0, x1 = max(ys.min() - 4, 0), min(ys.max() + 5, ref.shape[0]), max(xs.min() - 4, 0), min(xs.max() + 5, ref.shape[1])
    else:
        y0, y1, x0, x1 = 0, ref.shape[0], 0, ref.shape[1]
    roi_ref, roi_test = ref[y0:y1, x0:x1], test[y0:y1, x0:x1]
    roi16 = 100.0 * within[y0:y1, x0:x1].mean()
    win = min(7, roi_ref.shape[0], roi_ref.shape[1])
    if win % 2 == 0:
        win -= 1
    roi_ssim = float(ssim(roi_ref, roi_test, channel_axis=2, data_range=255, win_size=max(win, 3))) if win >= 3 else 1.0

    em = edge_mask(ref)
    edge_max = int(worst[em].max()) if em.any() else 0
    edge_mean = float(worst[em].mean()) if em.any() else 0.0

    heat = np.clip(worst.astype(np.float64) * 8, 0, 255).astype(np.uint8)
    hm = np.stack([heat, np.zeros_like(heat), 255 - heat], axis=2)
    hm[worst == 0] = (0, 0, 40)
    os.makedirs(os.path.dirname(heat_path), exist_ok=True)
    Image.fromarray(hm).save(heat_path)
    if ref.shape[1] <= 1400:
        tri = np.concatenate([ref, np.full((ref.shape[0], 6, 3), 60, np.uint8), test, np.full((ref.shape[0], 6, 3), 60, np.uint8), hm], axis=1)
        Image.fromarray(tri).save(heat_path.replace("_diff.png", "_cmp.png"))

    return {
        "pix16": pix16, "ssim": s, "roi16": roi16, "roi_ssim": roi_ssim,
        "max": int(diff.max()), "edge_max": edge_max, "edge_mean": edge_mean, "alpha_max": int(adiff.max()),
        "size": f"{ref.shape[1]}x{ref.shape[0]}",
    }


def main():
    ap = argparse.ArgumentParser()
    here = os.path.dirname(os.path.abspath(__file__))
    ap.add_argument("--unity", default=os.path.join(here, "out", "unity"))
    ap.add_argument("--scenes", default=os.path.join(here, "out", "web"))
    ap.add_argument("--streams", default=os.path.join(here, "out", "ref"))
    ap.add_argument("--out", default=os.path.join(here, "out", "compare"))
    ap.add_argument("--report", default=os.path.join(here, "REPORT.md"))
    ap.add_argument("--filter", default="")
    ap.add_argument("--perf", default=None)
    ap.add_argument("--no-report", action="store_true")
    args = ap.parse_args()

    scene_names = {os.path.splitext(f)[0] for f in os.listdir(os.path.join(here, "scenes")) if f.endswith(".json") and f != "index.json"}
    rows = []
    for f in sorted(os.listdir(args.unity)):
        if not f.endswith(".png"):
            continue
        name = os.path.splitext(f)[0]
        if args.filter and args.filter not in name:
            continue
        base = name.split("~")[0]
        kind = "scene" if base in scene_names else "stream"
        ref_dir = args.scenes if kind == "scene" else args.streams
        ref_path = os.path.join(ref_dir, base + ".png")
        if not os.path.exists(ref_path):
            print(f"{name}: no reference at {ref_path}", file=sys.stderr)
            continue
        r = compare(ref_path, os.path.join(args.unity, f), os.path.join(args.out, name + "_diff.png"))
        r["name"], r["kind"] = name, kind
        if "error" in r:
            r["pass"] = False
        else:
            r["pass"] = bool(r["pix16"] >= 99.0 and r["ssim"] >= 0.985)
            r["target"] = bool(r["pix16"] >= 99.5 and r["ssim"] >= 0.99)
        rows.append(r)
        if "error" in r:
            print(f"{name:28s} {r['error']}")
        else:
            print(f"{name:28s} pix16 {r['pix16']:7.3f}  ssim {r['ssim']:.4f}  roi16 {r['roi16']:7.3f}  roi_ssim {r['roi_ssim']:.4f}  max {r['max']:3d}  edge_max {r['edge_max']:3d}  {'PASS' if r['pass'] else 'FAIL'}")

    tags = {}
    for r in rows:
        if "~" in r["name"] and "error" not in r:
            tags.setdefault(r["name"].split("~", 1)[1], []).append(r)
    for tag, rs in sorted(tags.items()):
        print(f"{tag:50s} mean pix16 {np.mean([x['pix16'] for x in rs]):7.3f}  mean ssim {np.mean([x['ssim'] for x in rs]):.4f}  mean roi16 {np.mean([x['roi16'] for x in rs]):7.3f}  n={len(rs)}")

    with open(os.path.join(args.out, "results.json"), "w") as fh:
        json.dump(rows, fh, indent=1)
    if args.no_report:
        return

    perf = {}
    perf_path = args.perf or os.path.join(args.unity, "perf.json")
    if os.path.exists(perf_path):
        for p in json.load(open(perf_path)):
            perf[p["name"]] = p

    lines = ["# Canvas2D conformance report", "",
             "Metrics follow SPEC section 2: `pix16` = percent of pixels with every channel within 16 (bar 99.0, target 99.5), "
             "`SSIM` = skimage SSIM over the full frame (bar 0.985, target 0.99). `ROI` columns repeat both metrics over the bounding box of the drawn content "
             "(small synthetic scenes are mostly background, so the full-frame numbers flatter them). `edge max` is the largest channel difference on edge pixels of the reference.", ""]
    for kind, title in (("scene", "Synthetic scenes"), ("stream", "Gameplay streams")):
        sel = [r for r in rows if r["kind"] == kind and "error" not in r]
        if not sel:
            continue
        npass = sum(1 for r in sel if r["pass"])
        lines += [f"## {title} ({npass}/{len(sel)} pass)", "",
                  "| name | pix16 % | SSIM | ROI pix16 % | ROI SSIM | max diff | edge max | alpha max | pass |",
                  "|---|---:|---:|---:|---:|---:|---:|---:|:--:|"]
        for r in sel:
            mark = "PASS" if r["pass"] else "FAIL"
            if r["pass"] and not r["target"]:
                mark = "pass*"
            lines.append(f"| {r['name']} | {r['pix16']:.3f} | {r['ssim']:.4f} | {r['roi16']:.2f} | {r['roi_ssim']:.4f} | {r['max']} | {r['edge_max']} | {r['alpha_max']} | {mark} |")
        lines.append("")
    errs = [r for r in rows if "error" in r]
    if errs:
        lines += ["## Errors", ""] + [f"- {r['name']}: {r['error']}" for r in errs] + [""]
    if perf:
        lines += ["## Performance (Unity Editor, RTX 4060 Laptop, D3D11)", "",
                  "| stream | ops | edges | draws | record ms | submit ms | frame ms (CPU+GPU, synced) |", "|---|---:|---:|---:|---:|---:|---:|"]
        for name, p in perf.items():
            if name in scene_names:
                continue
            lines.append(f"| {name} | {p['ops']} | {p['edges']} | {p['draws']} | {p['avgRecordMs']:.2f} | {p['avgSubmitMs']:.2f} | {p['avgFrameMs']:.2f} |")
        lines.append("")
    with open(args.report, "w") as fh:
        fh.write("\n".join(lines) + "\n")
    print("wrote", args.report)


if __name__ == "__main__":
    main()
