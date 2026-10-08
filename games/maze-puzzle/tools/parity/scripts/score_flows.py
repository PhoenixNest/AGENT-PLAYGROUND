"""Score Unity flow shots against the web flow shots.

Strict bar: pct >= 99 and SSIM >= 0.985, and neither frame is flat.
A flat frame (unique colours < 50, or one colour covering > 99.5% of pixels)
is printed as BLANK and is never a pass. The web harness used to screenshot
pageEnter at 0% opacity, which is a solid --bg-deep fill on both sides.

Usage (from the worktree root, or anywhere):
  python tools/parity/scripts/score_flows.py
"""
import json
from pathlib import Path
import numpy as np
from PIL import Image
from skimage.metrics import structural_similarity as ssim

root = Path(__file__).resolve().parents[1]
web = root / "out" / "app-flows" / "web"
uni = root / "out" / "app-flows" / "unity"
golden = root / "golden" / "app"


def colour_stats(im):
    packed = (im[:, :, 0].astype(np.int32) << 16) | (im[:, :, 1].astype(np.int32) << 8) | im[:, :, 2].astype(np.int32)
    vals, counts = np.unique(packed, return_counts=True)
    dom = float(counts.max()) / float(packed.size)
    return int(len(vals)), dom


def is_blank(unique, dom):
    return unique < 50 or dom > 0.995


pages = {}
for p in sorted(golden.glob("*.json")):
    trace = json.loads(p.read_text(encoding="utf-8"))["trace"]
    for i, step in enumerate(trace, 1):
        snap = step["snap"]
        mods = ",".join(snap.get("modals") or []) or "-"
        pages[f"{p.stem}_{i:02d}"] = f"{snap.get('currentPage', '?')}/{mods}"

rows = []
for mine in sorted(uni.glob("*.png")):
    ref = web / mine.name
    if not ref.exists():
        print("NOWEB", mine.name)
        continue
    a = np.asarray(Image.open(mine).convert("RGB"))
    b = np.asarray(Image.open(ref).convert("RGB"))
    if a.shape != b.shape:
        print("SHAPE", mine.name, a.shape, b.shape)
        continue
    ua, da = colour_stats(a)
    ub, db = colour_stats(b)
    blank = is_blank(ua, da) or is_blank(ub, db)
    d = np.abs(a.astype(np.int16) - b.astype(np.int16)).max(2)
    pct = float((d <= 16).mean() * 100)
    sc = float(ssim(a, b, channel_axis=2, data_range=255))
    flow, step = mine.stem.rsplit("_", 1)
    page = pages.get(mine.stem, "?")
    # A blank web frame is not the page the trace names.
    shown = "no" if is_blank(ub, db) else "yes"
    good = (not blank) and pct >= 99 and sc >= 0.985
    rows.append((pct, sc, flow, step, page, ua, ub, shown, blank, good))

ok = sum(1 for r in rows if r[9])
blank_n = sum(1 for r in rows if r[8])
print(f"steps {len(rows)}  >=bar {ok}  BLANK {blank_n}")
print("flow | step | page | match% | SSIM | uniColors | webColors | webShowsPage | pass")
for pct, sc, flow, step, page, ua, ub, shown, blank, good in sorted(rows, key=lambda r: (r[2], r[3])):
    flag = "BLANK" if blank else ("yes" if good else "no")
    print(f"{flow} | {step} | {page} | {pct:.2f} | {sc:.4f} | {ua} | {ub} | {shown} | {flag}")
print("--- worst 10 (non-blank first, then blank) ---")
ranked = sorted(rows, key=lambda r: (r[8], r[0], r[1]))
for pct, sc, flow, step, page, ua, ub, shown, blank, good in ranked[:10]:
    tag = " BLANK" if blank else ""
    print(f"{flow} {step} {page} {pct:.2f} {sc:.4f} uni={ua} web={ub}{tag}")
