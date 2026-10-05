import argparse
import json
import os
import sys
from concurrent.futures import ProcessPoolExecutor
from datetime import datetime
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageDraw
from skimage.metrics import structural_similarity

sys.path.insert(0, str(Path(__file__).resolve().parent))
import trace_diff  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
PIXEL_TOL = 16
PIXEL_PASS_PCT = 99.0
SSIM_PASS = 0.985
BOX_PASS_PX = 3.0
EXPECTED_SIZE = (1290, 2796)
AXES = ("x", "y", "w", "h")


def resolve(p):
    p = Path(p)
    return p if p.is_absolute() else ROOT / p


def load_rgb(path):
    with Image.open(path) as im:
        return np.asarray(im.convert("RGB"))


def heatmap_image(maxdiff):
    d = maxdiff.astype(np.float32)
    scaled = np.where(d <= 16, d * 8.0, 128.0 + (np.minimum(d, 64) - 16) * (127.0 / 48.0))
    gray = np.clip(scaled, 0, 255).astype(np.uint8)
    colored = cv2.applyColorMap(gray, cv2.COLORMAP_INFERNO)
    return Image.fromarray(cv2.cvtColor(colored, cv2.COLOR_BGR2RGB))


def triptych(web, unity, mask, label):
    scale = 0.5
    h, w = web.shape[:2]
    size = (int(w * scale), int(h * scale))
    panels = [
        cv2.resize(web, size, interpolation=cv2.INTER_AREA),
        cv2.resize(unity, size, interpolation=cv2.INTER_AREA),
    ]
    m = cv2.resize(mask.astype(np.uint8) * 255, size, interpolation=cv2.INTER_AREA) > 0
    mask_panel = np.zeros((size[1], size[0], 3), np.uint8)
    mask_panel[m] = (255, 255, 255)
    panels.append(mask_panel)
    gap = 8
    header = 28
    sheet = Image.new("RGB", (size[0] * 3 + gap * 2, size[1] + header), (40, 40, 40))
    draw = ImageDraw.Draw(sheet)
    for i, (panel, name) in enumerate(zip(panels, ("web", "unity", "over tolerance"))):
        x = i * (size[0] + gap)
        sheet.paste(Image.fromarray(panel), (x, header))
        draw.text((x + 6, 8), f"{label}  |  {name}", fill=(255, 255, 255))
    return sheet


def compare_layouts(web_path, unity_path):
    if not web_path.exists():
        return {"status": "MISSING", "missing": "web"}
    if not unity_path.exists():
        return {"status": "MISSING", "missing": "unity"}
    web = json.loads(web_path.read_text(encoding="utf-8"))
    unity = json.loads(unity_path.read_text(encoding="utf-8"))
    uel = {}
    for e in unity.get("elements", []):
        uel.setdefault(e["id"], e)
    matched = 0
    failing = []
    worst = {"delta": 0.0, "id": None, "axis": None}
    missing_in_unity = []
    for w in web.get("elements", []):
        if not w.get("visible"):
            u = uel.get(w["id"])
            if u is not None and u.get("visible"):
                failing.append({"id": w["id"], "reason": "visibility", "web": False, "unity": True})
            continue
        u = uel.get(w["id"])
        if u is None:
            missing_in_unity.append(w["id"])
            failing.append({"id": w["id"], "reason": "missing_in_unity"})
            continue
        if not u.get("visible"):
            failing.append({"id": w["id"], "reason": "visibility", "web": True, "unity": False})
            continue
        matched += 1
        deltas = {a: u["rect"][a] - w["rect"][a] for a in AXES}
        axis = max(AXES, key=lambda a: abs(deltas[a]))
        if abs(deltas[axis]) > worst["delta"]:
            worst = {"delta": abs(deltas[axis]), "id": w["id"], "axis": axis}
        if abs(deltas[axis]) > BOX_PASS_PX:
            failing.append(
                {
                    "id": w["id"],
                    "reason": "box",
                    "axis": axis,
                    "delta": round(deltas[axis], 3),
                    "deltas": {a: round(v, 3) for a, v in deltas.items()},
                    "web": w["rect"],
                    "unity": u["rect"],
                }
            )
    failing.sort(key=lambda f: -abs(f.get("delta", 1e9)))
    return {
        "status": "FAIL" if failing else "PASS",
        "matched": matched,
        "worst": worst,
        "failing": failing,
        "failing_count": len(failing),
    }


def compare_pair(job):
    key, web_dir, unity_dir, heat_dir = job
    web_dir, unity_dir, heat_dir = Path(web_dir), Path(unity_dir), Path(heat_dir)
    res = {"key": key, "status": "PASS", "reasons": []}
    web_png, unity_png = web_dir / f"{key}.png", unity_dir / f"{key}.png"
    if not unity_png.exists():
        res["status"] = "MISSING"
        res["reasons"].append("missing: unity png")
        res["layout"] = compare_layouts(web_dir / f"{key}.layout.json", unity_dir / f"{key}.layout.json")
        return res
    web, unity = load_rgb(web_png), load_rgb(unity_png)
    if web.shape != unity.shape:
        res["status"] = "FAIL"
        res["reasons"].append(f"size {unity.shape[1]}x{unity.shape[0]} != {web.shape[1]}x{web.shape[0]}")
        return res
    maxdiff = np.abs(web.astype(np.int16) - unity.astype(np.int16)).max(axis=2)
    over = maxdiff > PIXEL_TOL
    pct = 100.0 * (1.0 - float(over.mean()))
    ssim = float(structural_similarity(web, unity, channel_axis=2, data_range=255))
    res["pixel_pct"] = pct
    res["ssim"] = ssim
    res["max_channel_diff"] = int(maxdiff.max())
    if pct < PIXEL_PASS_PCT:
        res["reasons"].append(f"pixels {pct:.2f}% < {PIXEL_PASS_PCT}%")
    if ssim < SSIM_PASS:
        res["reasons"].append(f"ssim {ssim:.4f} < {SSIM_PASS}")
    layout = compare_layouts(web_dir / f"{key}.layout.json", unity_dir / f"{key}.layout.json")
    res["layout"] = layout
    if layout["status"] == "MISSING":
        res["reasons"].append(f"missing: {layout['missing']} layout")
    elif layout["status"] == "FAIL":
        res["reasons"].append(f"boxes: {layout['failing_count']} failing")
    heat_dir.mkdir(parents=True, exist_ok=True)
    heatmap_image(maxdiff).save(heat_dir / f"{key}.heatmap.png", optimize=False, compress_level=3)
    triptych(web, unity, over, key).save(heat_dir / f"{key}.diff.png", optimize=False, compress_level=3)
    if any(r.startswith("missing") for r in res["reasons"]):
        res["status"] = "MISSING"
    elif res["reasons"]:
        res["status"] = "FAIL"
    return res


def read_json(path):
    try:
        return json.loads(Path(path).read_text(encoding="utf-8"))
    except (OSError, ValueError):
        return None


def collect_keys(web_dir, only, profile):
    keys = sorted(p.stem for p in web_dir.glob("*__*.png"))
    if only:
        keys = [k for k in keys if k.rsplit("__", 1)[0] in only]
    if profile:
        keys = [k for k in keys if k.endswith(f"__{profile}")]
    return keys


def collect_traces(web_dir, unity_dir, only):
    out = {}
    tdir = web_dir / "traces"
    if not tdir.is_dir():
        return out
    for p in sorted(tdir.glob("*.trace.json")):
        sid = p.name[: -len(".trace.json")]
        if only and sid not in only:
            continue
        out[sid] = trace_diff.compare_files(p, unity_dir / "traces" / p.name)
    return out


def fmt_worst(r):
    layout = r.get("layout")
    if not layout or layout.get("status") == "MISSING":
        return "-"
    w = layout["worst"]
    if w["id"] is None:
        return "-"
    return f"{w['delta']:.2f} ({w['id']} {w['axis']})"


def render_report(report):
    s = report["summary"]
    lines = [
        f"# Parity report iter-{report['iter']}",
        "",
        f"Generated {report['generated']}.  web: `{report['web']}`  unity: `{report['unity']}`",
        "",
        f"**{s['pass']} PASS, {s['fail']} FAIL, {s['missing']} MISSING** of {s['total']} (state, profile) pairs; "
        f"traces: {s['trace_pass']} PASS, {s['trace_fail']} FAIL, {s['trace_missing']} MISSING of {s['trace_total']}.",
        "",
        "Pass bar (SPEC 2): >= 99.0 % pixels within 16 per channel, SSIM >= 0.985, every visible matched element within 3 px on x, y, w, h.",
        "",
        "## States (worst first)",
        "",
        "| state | profile | pixels % | SSIM | worst box delta px | trace | verdict | reasons |",
        "| --- | --- | --- | --- | --- | --- | --- | --- |",
    ]
    for r in report["pairs"]:
        state, profile = r["key"].rsplit("__", 1)
        pct = f"{r['pixel_pct']:.2f}" if "pixel_pct" in r else "-"
        ssim = f"{r['ssim']:.4f}" if "ssim" in r else "-"
        trace = r.get("trace_verdict", "-")
        lines.append(
            f"| {state} | {profile} | {pct} | {ssim} | {fmt_worst(r)} | {trace} | {r['status']} | {'; '.join(r['reasons']) or '-'} |"
        )
    failing = [r for r in report["pairs"] if r.get("layout", {}).get("failing")]
    if failing:
        lines += ["", "## Failing elements", ""]
        for r in failing:
            lines.append(f"### {r['key']}")
            lines.append("")
            lines.append("| element | reason | axis | delta px | web rect | unity rect |")
            lines.append("| --- | --- | --- | --- | --- | --- |")
            for f in r["layout"]["failing"][:25]:
                fr = lambda v: "-" if v is None else json.dumps(v, separators=(",", ":"))
                lines.append(
                    f"| {f['id']} | {f['reason']} | {f.get('axis', '-')} | {f.get('delta', '-')} | {fr(f.get('web') if isinstance(f.get('web'), dict) else None)} | {fr(f.get('unity') if isinstance(f.get('unity'), dict) else None)} |"
                )
            extra = r["layout"]["failing_count"] - 25
            if extra > 0:
                lines.append(f"| ... {extra} more | | | | | |")
            lines.append("")
    lines += ["## Traces", "", "| sequence | verdict | detail |", "| --- | --- | --- |"]
    for sid, t in report["traces"].items():
        lines.append(f"| {sid} | {t['verdict']} | {trace_diff.summary_line(t).replace('|', '/')} |")
    lines += ["", "## Harness settings", ""]
    for side in ("web", "unity"):
        h = report["harness"].get(side)
        lines.append(f"### {side}")
        lines.append("")
        if not h:
            lines.append("`_meta.json` not found.")
        else:
            lines.append(f"- Chromium: {h.get('chromium', '-')}")
            lines.append(f"- Launch args: `{' '.join(h.get('launchArgs', []))}`")
            lines.append(f"- Animation clock: {h.get('animationClock', '-')}")
            offsets = sorted({str(v.get("perfOffset")) for v in h.get("states", {}).values()})
            lines.append(f"- perfOffset per run: {', '.join(offsets) if offsets else '-'} ({len(h.get('states', {}))} runs)")
        lines.append("")
    return "\n".join(lines)


def main(argv=None):
    p = argparse.ArgumentParser(description="Pixel, layout and trace diff (SPEC section 12)")
    p.add_argument("--web", required=True)
    p.add_argument("--unity", required=True)
    p.add_argument("--iter", required=True, dest="iteration")
    p.add_argument("--only")
    p.add_argument("--profile", choices=("zero", "iphone"))
    p.add_argument("--workers", type=int, default=min(4, os.cpu_count() or 1))
    p.add_argument("--reports", default="reports", help="directory for report.md/json")
    p.add_argument("--images", default="out/reports", help="directory for heatmap and diff PNGs")
    args = p.parse_args(argv)

    web_dir, unity_dir = resolve(args.web), resolve(args.unity)
    if not web_dir.is_dir():
        print(f"web directory not found: {web_dir}", file=sys.stderr)
        return 2
    iter_name = f"iter-{args.iteration}" if not str(args.iteration).startswith("iter-") else str(args.iteration)
    report_dir = resolve(args.reports) / iter_name
    image_dir = resolve(args.images) / iter_name
    report_dir.mkdir(parents=True, exist_ok=True)
    image_dir.mkdir(parents=True, exist_ok=True)
    only = set(args.only.split(",")) if args.only else None

    keys = collect_keys(web_dir, only, args.profile)
    if not keys:
        print("no web captures matched", file=sys.stderr)
        return 2
    jobs = [(k, str(web_dir), str(unity_dir), str(image_dir)) for k in keys]
    if args.workers > 1:
        with ProcessPoolExecutor(max_workers=args.workers) as ex:
            pairs = list(ex.map(compare_pair, jobs, chunksize=2))
    else:
        pairs = [compare_pair(j) for j in jobs]

    traces = collect_traces(web_dir, unity_dir, only)
    for r in pairs:
        t = traces.get(r["key"].rsplit("__", 1)[0])
        if t is not None:
            r["trace_verdict"] = t["verdict"]
            if t["verdict"] == "FAIL":
                r["reasons"].append("trace")
                if r["status"] == "PASS":
                    r["status"] = "FAIL"

    def severity(r):
        if r["status"] == "PASS":
            return (2, 0.0)
        if r["status"] == "MISSING":
            return (1, 0.0)
        return (0, r.get("pixel_pct", 0.0))

    pairs.sort(key=lambda r: (severity(r), r["key"]))
    count = lambda status: sum(1 for r in pairs if r["status"] == status)
    tcount = lambda v: sum(1 for t in traces.values() if t["verdict"] == v)
    report = {
        "iter": args.iteration,
        "generated": datetime.now().isoformat(timespec="seconds"),
        "web": str(web_dir),
        "unity": str(unity_dir),
        "thresholds": {
            "pixel_tolerance": PIXEL_TOL,
            "pixel_pass_pct": PIXEL_PASS_PCT,
            "ssim": SSIM_PASS,
            "box_px": BOX_PASS_PX,
        },
        "summary": {
            "total": len(pairs),
            "pass": count("PASS"),
            "fail": count("FAIL"),
            "missing": count("MISSING"),
            "trace_total": len(traces),
            "trace_pass": tcount("PASS"),
            "trace_fail": tcount("FAIL"),
            "trace_missing": tcount("MISSING"),
        },
        "harness": {"web": read_json(web_dir / "_meta.json"), "unity": read_json(unity_dir / "_meta.json")},
        "pairs": pairs,
        "traces": traces,
    }
    (report_dir / "report.json").write_text(json.dumps(report, indent=1), encoding="utf-8")
    (report_dir / "report.md").write_text(render_report(report), encoding="utf-8")
    s = report["summary"]
    print(
        f"{s['pass']} PASS, {s['fail']} FAIL, {s['missing']} MISSING of {s['total']}; "
        f"traces {s['trace_pass']}/{s['trace_total']} PASS; report: {report_dir / 'report.md'}"
    )
    if s["missing"] or s["trace_missing"]:
        return 2
    return 1 if (s["fail"] or s["trace_fail"]) else 0


if __name__ == "__main__":
    sys.exit(main())
