import argparse
import json
import sys
from pathlib import Path

DT_MAX_MS = 16.7
AUDIO_EPS = 1e-6


def load_trace(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def values_equal(a, b, tol):
    if isinstance(a, bool) or isinstance(b, bool):
        return a is b
    if isinstance(a, (int, float)) and isinstance(b, (int, float)):
        return abs(a - b) <= tol if tol else a == b
    if isinstance(a, list) and isinstance(b, list):
        return len(a) == len(b) and all(values_equal(x, y, tol) for x, y in zip(a, b))
    if isinstance(a, dict) and isinstance(b, dict):
        return a.keys() == b.keys() and all(values_equal(a[k], b[k], tol) for k in a)
    return a == b


def describe(ev):
    return json.dumps(ev, separators=(",", ":"), ensure_ascii=False)


def compare_events(web_events, unity_events, max_details=10):
    mismatches = []
    max_dt = 0.0
    for i in range(min(len(web_events), len(unity_events))):
        w, u = web_events[i], unity_events[i]
        reasons = []
        if w.get("type") != u.get("type"):
            reasons.append("type")
        if w.get("name") != u.get("name"):
            reasons.append("name")
        tol = AUDIO_EPS if w.get("type") == "audio" else 0
        if not values_equal(w.get("args"), u.get("args"), tol):
            reasons.append("args")
        dt = abs(w.get("t", 0) - u.get("t", 0))
        max_dt = max(max_dt, dt)
        if dt > DT_MAX_MS:
            reasons.append("t")
        if reasons:
            mismatches.append({"index": i, "reasons": reasons, "web": w, "unity": u})
    count_mismatch = len(web_events) != len(unity_events)
    ok = not mismatches and not count_mismatch
    result = {
        "verdict": "PASS" if ok else "FAIL",
        "web_events": len(web_events),
        "unity_events": len(unity_events),
        "max_dt_ms": max_dt,
        "mismatch_count": len(mismatches),
        "first_mismatch": mismatches[0]["index"] if mismatches else None,
        "details": mismatches[:max_details],
    }
    if count_mismatch:
        result["length_mismatch"] = True
        n = min(len(web_events), len(unity_events))
        extra_side = "web" if len(web_events) > len(unity_events) else "unity"
        extra = (web_events if extra_side == "web" else unity_events)[n]
        result["first_extra"] = {"side": extra_side, "index": n, "event": extra}
    return result


def compare_files(web_path, unity_path):
    web_path, unity_path = Path(web_path), Path(unity_path)
    if not web_path.exists():
        return {"verdict": "MISSING", "missing": "web"}
    if not unity_path.exists():
        return {"verdict": "MISSING", "missing": "unity"}
    web, unity = load_trace(web_path), load_trace(unity_path)
    result = compare_events(web["events"], unity["events"])
    if web.get("sequence") != unity.get("sequence"):
        result["verdict"] = "FAIL"
        result["sequence_mismatch"] = [web.get("sequence"), unity.get("sequence")]
    return result


def summary_line(result):
    if result["verdict"] == "MISSING":
        return f"MISSING ({result['missing']})"
    if result["verdict"] == "PASS":
        return f"PASS ({result['web_events']} events, max dt {result['max_dt_ms']:g} ms)"
    parts = [f"FAIL ({result['web_events']} web / {result['unity_events']} unity events"]
    if result["first_mismatch"] is not None:
        d = result["details"][0]
        parts.append(f", first mismatch #{d['index']} [{','.join(d['reasons'])}]: web={describe(d['web'])} unity={describe(d['unity'])}")
    elif result.get("first_extra"):
        e = result["first_extra"]
        parts.append(f", first extra on {e['side']} #{e['index']}: {describe(e['event'])}")
    return "".join(parts) + ")"


def main(argv=None):
    p = argparse.ArgumentParser(description="Compare web and Unity trace JSON files (SPEC section 10)")
    p.add_argument("--web", required=True, help="web trace file or directory")
    p.add_argument("--unity", required=True, help="unity trace file or directory")
    p.add_argument("--json", action="store_true")
    args = p.parse_args(argv)
    web, unity = Path(args.web), Path(args.unity)
    if web.is_dir():
        names = sorted(x.name for x in web.glob("*.trace.json"))
        pairs = [(n, web / n, unity / n) for n in names]
    else:
        pairs = [(web.name, web, unity)]
    if not pairs:
        print("no traces found", file=sys.stderr)
        return 2
    code = 0
    results = {}
    for name, w, u in pairs:
        r = compare_files(w, u)
        results[name] = r
        if r["verdict"] == "MISSING":
            code = 2
        elif r["verdict"] == "FAIL" and code != 2:
            code = 1
        if not args.json:
            print(f"{name}: {summary_line(r)}")
    if args.json:
        print(json.dumps(results, indent=1))
    return code


if __name__ == "__main__":
    sys.exit(main())
