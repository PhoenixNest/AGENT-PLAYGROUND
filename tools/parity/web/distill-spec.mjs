// Distils the full web layout dump (web/dump-spec.mjs -> out/web/spec/<page>__<profile>.json, ~130 computed properties per
// element) into the compact spec the Unity chrome painter reads (Assets/Resources/Parity/Spec/<page>__<profile>.json).
//
//   node web/distill-spec.mjs --page home [--out <dir>]
//
// One record per element in DOM order (= paint order for the unpositioned content of these pages). Rects are CSS px,
// top-left origin, as measured by getBoundingClientRect in Chromium (after transforms and animation at the capture time).
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, isAbsolute, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const parityDir = resolve(here, "..");
const args = process.argv.slice(2);
let page = "home";
let outDir = resolve(parityDir, "../../games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Parity/Spec");
for (let i = 0; i < args.length; i++) {
  if (args[i] === "--page") page = args[++i];
  else if (args[i] === "--out") {
    const o = args[++i];
    outDir = isAbsolute(o) ? o : resolve(process.cwd(), o);
  } else {
    console.error(`unknown argument ${args[i]}`);
    process.exit(2);
  }
}
mkdirSync(outDir, { recursive: true });

const round = (n) => Math.round(n * 1000) / 1000;
const rect = (r) => [round(r.x), round(r.y), round(r.w), round(r.h)];
const num = (s) => {
  const n = parseFloat(s);
  return Number.isFinite(n) ? n : 0;
};
const moved = (a, b) => a.some((v, i) => Math.abs(v - b[i]) > 0.02);

// Split a CSS list on commas that are not inside parentheses.
function splitList(text) {
  const parts = [];
  let depth = 0;
  let cur = "";
  for (const ch of String(text ?? "")) {
    if (ch === "(") depth++;
    else if (ch === ")") depth--;
    if (ch === "," && depth === 0) {
      parts.push(cur.trim());
      cur = "";
    } else cur += ch;
  }
  if (cur.trim()) parts.push(cur.trim());
  return parts;
}

function timeMs(s) {
  const str = String(s ?? "").trim();
  if (!str || str === "none") return 0;
  if (str.endsWith("ms")) return parseFloat(str) || 0;
  if (str.endsWith("s")) return (parseFloat(str) || 0) * 1000;
  const n = parseFloat(str);
  return Number.isFinite(n) ? n : 0;
}

// One animation (the first, when a shorthand lists several). it = 0 means infinite.
function compactAnim(anim) {
  if (!anim || !anim.name || anim.name === "none") return null;
  const name = splitList(anim.name)[0];
  if (!name || name === "none") return null;
  const iter = splitList(String(anim.iteration ?? "1"))[0];
  return {
    n: name,
    du: round(timeMs(splitList(anim.duration)[0])),
    ea: splitList(anim.timing || "ease")[0] || "ease",
    dl: round(timeMs(splitList(anim.delay)[0])),
    it: iter === "infinite" ? 0 : Number(iter) || 1,
    di: splitList(anim.direction || "normal")[0] || "normal",
    fi: splitList(anim.fill || "none")[0] || "none",
    ct: anim.currentTime == null ? 0 : round(anim.currentTime),
    xf: anim.transforms ? 1 : 0,
  };
}

// Keep the scored still-frame fields. Overlay only the animation baseline so Draw() stays put.
function mergePaint(path, fresh) {
  if (!existsSync(path)) return fresh;
  const base = JSON.parse(readFileSync(path, "utf8"));
  if (!base.elements || base.elements.length !== fresh.elements.length) {
    console.warn(`paint merge skipped for ${path}: element count ${base.elements && base.elements.length} vs ${fresh.elements.length}`);
    return fresh;
  }
  base.t0 = fresh.t0;
  base.elements.forEach((el, i) => {
    const src = fresh.elements[i];
    for (const key of ["am", "br", "bt", "dx", "to"]) {
      if (src[key] != null) el[key] = src[key];
      else delete el[key];
    }
    const runs = el.runs || [];
    const srcRuns = src.runs || [];
    if (runs.length === srcRuns.length) {
      runs.forEach((run, ri) => {
        if (srcRuns[ri].br) run.br = srcRuns[ri].br;
        else delete run.br;
      });
    }
  });
  return base;
}

function writeKeyframes(dir) {
  const srcPath = join(parityDir, "out/web/spec/keyframes.json");
  if (!existsSync(srcPath)) {
    console.warn(`no ${srcPath}; _keyframes.json not written`);
    return;
  }
  const raw = JSON.parse(readFileSync(srcPath, "utf8"));
  const doc = {};
  for (const rule of raw) {
    const stops = [];
    for (const k of rule.keyframes || []) {
      const props = {};
      for (const pair of k.declarations || []) {
        const p = pair[0];
        const v = pair[1];
        if (p === "transform") props.tf = v;
        else if (p === "opacity") props.o = v;
        else if (p === "box-shadow") props.sh = v;
        else if (p === "border-color") props.bc = v;
        else if (p === "animation-timing-function") props.ea = v;
      }
      for (const piece of String(k.keyText || "0%").split(",")) {
        const s = piece.trim();
        let t = 0;
        if (s === "from") t = 0;
        else if (s === "to") t = 1;
        else if (s.endsWith("%")) t = parseFloat(s) / 100;
        else t = parseFloat(s);
        stops.push({ t: round(t), ...props });
      }
    }
    stops.sort((a, b) => a.t - b.t);
    doc[rule.name] = stops;
  }
  const path = join(dir, "_keyframes.json");
  writeFileSync(path, JSON.stringify(doc));
  console.log(`wrote ${path} (${Object.keys(doc).length} animations)`);
}

function pseudoBox(host, p) {
  if (!p) return null;
  const w = num(p.width);
  const h = num(p.height);
  if (w <= 0 || h <= 0) return null;
  let x = host.rect.x + num(p.left);
  let y = p.top != null && p.top !== "auto" ? host.rect.y + num(p.top) : host.rect.y + host.rect.h - num(p.bottom) - h;
  const m = /matrix\(\s*[^,]+,[^,]+,[^,]+,[^,]+,\s*([-0-9.eE]+)\s*,\s*([-0-9.eE]+)\s*\)/.exec(p.transform || "");
  if (m) {
    x += Number(m[1]);
    y += Number(m[2]);
  }
  const rad = p["border-top-left-radius"] || (w === h ? `${w / 2}px` : "0px");
  const box = {
    r: [round(x), round(y), round(w), round(h)],
    bg: p["background-color"] || "transparent",
    rad: [rad, p["border-top-right-radius"] || rad, p["border-bottom-right-radius"] || rad, p["border-bottom-left-radius"] || rad],
    v: 1,
    o: Number(p.opacity || 1),
  };
  if (p["box-shadow"] && p["box-shadow"] !== "none") box.sh = p["box-shadow"];
  if (num(p["border-top-width"]) > 0) {
    box.bw = [p["border-top-width"], p["border-right-width"], p["border-bottom-width"], p["border-left-width"]];
    box.bc = [p["border-top-color"], p["border-right-color"], p["border-bottom-color"], p["border-left-color"]];
    box.bs = [p["border-top-style"], p["border-right-style"], p["border-bottom-style"], p["border-left-style"]];
  }
  return box;
}

for (const profile of ["zero", "iphone"]) {
  const srcPath = join(parityDir, "out/web/spec", `${page}__${profile}.json`);
  if (!existsSync(srcPath)) continue;
  const src = JSON.parse(readFileSync(srcPath, "utf8"));
  const elements = src.elements.map((e) => {
    const o = { i: e.index, p: e.parent, t: e.tag, r: rect(e.rect) };
    if (e.id) o.id = e.id;
    if (e.classes && e.classes.length) o.c = e.classes.join(" ");
    if (e.tag === "img") {
      o.emoji = e.emoji;
      return o;
    }
    const s = e.style;
    o.v = e.visible ? 1 : 0;
    o.d = s["display"];
    o.o = Number(s["opacity"]);
    if (s["visibility"] !== "visible") o.vis = s["visibility"];
    o.bg = s["background-color"];
    o.bi = s["background-image"];
    o.rad = [
      s["border-top-left-radius"],
      s["border-top-right-radius"],
      s["border-bottom-right-radius"],
      s["border-bottom-left-radius"],
    ];
    o.bw = [
      s["border-top-width"],
      s["border-right-width"],
      s["border-bottom-width"],
      s["border-left-width"],
    ];
    o.bc = [
      s["border-top-color"],
      s["border-right-color"],
      s["border-bottom-color"],
      s["border-left-color"],
    ];
    o.bs = [
      s["border-top-style"],
      s["border-right-style"],
      s["border-bottom-style"],
      s["border-left-style"],
    ];
    o.sh = s["box-shadow"];
    o.tf = s["transform"];
    o.flt = s["filter"];
    o.bf = s["backdrop-filter"];
    o.an = s["animation-name"];
    o.col = s["color"];
    o.fw = s["font-weight"];
    o.fz = s["font-size"];
    o.ff = s["font-family"];
    o.ls = s["letter-spacing"];
    o.ts = s["text-shadow"];
    o.tt = s["text-transform"];
    const knob = pseudoBox(e, e.before);
    if (knob) o.bef = knob;
    const tail = pseudoBox(e, e.after);
    if (tail) o.aft = tail;
    if (e.input && e.input.type === "range" && (e.classes || []).includes("cartoon-range") && e.rect.w > 0) {
      const min = Number(e.input.min || 0);
      const max = Number(e.input.max || 100);
      const val = Number(e.input.value);
      const tw = 22;
      const th = 22;
      const t = max === min ? 0 : (val - min) / (max - min);
      o.th = {
        r: [round(e.rect.x + t * (e.rect.w - tw)), round(e.rect.y + (e.rect.h - th) / 2), tw, th],
        bg: "#facc15",
        rad: ["11px", "11px", "11px", "11px"],
        bw: ["2.5px", "2.5px", "2.5px", "2.5px"],
        bc: ["#1e1b4b", "#1e1b4b", "#1e1b4b", "#1e1b4b"],
        bs: ["solid", "solid", "solid", "solid"],
        sh: "rgba(0, 0, 0, 0.3) 0px 2px 4px 0px",
        v: 1,
        o: 1,
      };
    }
    o.runs = e.textRuns
      .filter((t) => t.kind === "text")
      .map((t, ri) => {
        const run = { s: t.text, r: rect(t.rect) };
        const br = e.baseRuns && e.baseRuns[ri];
        if (br && moved(run.r, rect(br))) run.br = rect(br);
        return run;
      });
    const anim = compactAnim(e.anim);
    if (anim) o.am = anim;
    if (e.baseRect) {
      const br = rect(e.baseRect);
      if (moved(o.r, br)) o.br = br;
    }
    if (e.baseTransform && e.baseTransform !== "none") o.bt = e.baseTransform;
    if (e.dx) {
      const n = parseFloat(e.dx);
      if (Number.isFinite(n)) o.dx = round(n);
    }
    if (e.anim && e.anim.origin && e.anim.origin !== "50% 50%" && e.anim.origin !== "50% 50% 0px")
      o.to = e.anim.origin;
    return o;
  });
  // Hidden pages in the same frame report currentTime 0. The capture clock is the latest visible animation.
  const times = elements.filter((el) => el.v === 1 && el.am && el.am.ct > 0).map((el) => el.am.ct);
  const t0 = times.length ? round(Math.max(...times)) : 0;
  const fresh = {
    page,
    profile,
    viewport: src.viewport,
    t0,
    count: elements.length,
    elements,
  };
  const path = join(outDir, `${page}__${profile}.json`);
  const merged = mergePaint(path, fresh);
  writeFileSync(path, JSON.stringify(merged));
  console.log(`wrote ${path} (${merged.elements.length} elements, t0=${merged.t0}ms, anim=${merged.elements.filter((e) => e.am).length})`);
}

writeKeyframes(outDir);
