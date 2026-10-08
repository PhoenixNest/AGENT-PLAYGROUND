// Headless study of where debris is lost between the ring aperture and the black hole.
// Runs the web engine (games/maze-puzzle/web/engine) under Node at a steady 1/60 s.
// Usage: node tools/parity/scripts/funnel-study.mjs [--variant name] [--k 16] [--c crit] [--seconds 40]
// --assert 1 fails unless idle spill is 0 on every releasing level, swipe/spin spill is in (0, 10], and no debris stays FALLING longer than 5 s.
import { dirname, join } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const webRoot = join(here, "..", "..", "..", "games", "maze-puzzle", "web");
const { PhysicsEngine } = await import(pathToFileURL(join(webRoot, "engine/physics.js")).href);
const { MazeLevelBuilder } = await import(pathToFileURL(join(webRoot, "engine/maze.js")).href);

const args = Object.fromEntries(process.argv.slice(2).reduce((a, v, i, all) => (v.startsWith("--") ? [...a, [v.slice(2), all[i + 1]]] : a), []));
const levels = (args.levels || "1,2,3,5,11,21,31,41").split(",").map(Number);
const seconds = Number(args.seconds || 40);
const variant = args.variant || "web";
const seeds = Number(args.seeds || 6);
const kOverride = args.k !== undefined ? Number(args.k) : null;
const mulOverride = args.mul !== undefined ? Number(args.mul) : null;
const postOverride = args.post !== undefined ? Number(args.post) : null;
const assertMode = args.assert === "1" || args.assert === "true";

const audio = new Proxy({}, { get: () => () => {} });
const W = 430;
const H = 932;
const centerX = W * 0.5;
const centerY = Math.max(175, H * 0.38);
const bhY = Math.min(H - 110, Math.max(centerY + 240, H * 0.76));
const DT = 1 / 60;

function mulberry32(a) {
  return () => {
    a |= 0;
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

function lateralK() {
  if (kOverride !== null) return kOverride;
  if (variant === "strong-pull" || variant === "combo") return 40;
  return 16;
}

function lateralC(k) {
  if (args.c === undefined) return 0;
  if (args.c === "crit") return 2 * Math.sqrt(k);
  return Number(args.c);
}

class Variant extends PhysicsEngine {
  resolveMagneticFunnel(d, dt) {
    const tuned = kOverride !== null || args.c !== undefined || mulOverride !== null || postOverride !== null;
    if (variant === "web" && !tuned) return super.resolveMagneticFunnel(d, dt);
    const topY = this.center.y + this.ringRadius - 15;
    const bottomY = this.blackHole.y;
    const extend = variant === "rails-extend" || variant === "combo" ? 60 : 25;
    if (d.y >= topY && d.y <= bottomY + extend) {
      const t = Math.max(0, Math.min(1, (d.y - topY) / Math.max(1, bottomY - topY)));
      const halfWidth = 95 * (1 - t) + (variant === "narrow" || variant === "combo" ? 30 : 46) * t;
      const centerXf = this.blackHole.x;
      const dx = centerXf - d.x;
      const k = lateralK();
      const c = lateralC(k);
      const mul = mulOverride === null ? 1 : mulOverride;
      if (mul !== 1) d.vx *= mul;
      if (c !== 0) d.vx += (dx * k - d.vx * c) * dt;
      else d.vx += dx * k * dt;
      if (postOverride !== null) d.vx *= postOverride;
      const left = centerXf - halfWidth + d.radius;
      const right = centerXf + halfWidth - d.radius;
      if (d.x < left) {
        d.x = left;
        if (d.vx < 0) d.vx = -d.vx * 0.45;
      } else if (d.x > right) {
        d.x = right;
        if (d.vx > 0) d.vx = -d.vx * 0.45;
      }
    }
  }
}

function runOne(level, seed, style) {
  const rng = mulberry32(seed * 7919 + level);
  const saved = Math.random;
  Math.random = rng;
  const eng = new Variant(audio);
  const sector = MazeLevelBuilder.buildLevel(level);
  eng.initSector(sector, { x: centerX, y: centerY }, { x: centerX, y: bhY });
  const track = new Map();
  const out = { swallowed: 0, spilled: 0, spillLeft: 0, spillRight: 0, crossLeft: 0, crossRight: 0, offsets: [], spillOffsets: [], swallowOffsets: [], spillVx: [], spillWhy: { side: 0, below: 0 }, maxFall: 0, stillFalling: 0, fallSum: 0, fallN: 0 };
  const noteFall = (t, frame) => {
    if (!t || t.fallStart === undefined) return;
    const dur = (frame - t.fallStart + 1) * DT;
    out.fallSum += dur;
    out.fallN++;
    if (dur > out.maxFall) out.maxFall = dur;
  };
  eng.onDebrisSwallowed = (d) => {
    out.swallowed++;
    const t = track.get(d.id);
    if (t && t.crossX !== undefined) out.swallowOffsets.push(t.crossX - centerX);
    noteFall(t, frame);
  };
  eng.onDebrisSpilled = (d) => {
    out.spilled++;
    if (d.x < centerX) out.spillLeft++;
    else out.spillRight++;
    const t = track.get(d.id);
    if (t && t.crossX !== undefined) out.spillOffsets.push(t.crossX - centerX);
    out.spillVx.push(t && t.crossVx !== undefined ? t.crossVx : 0);
    if (d.x < -60 || d.x > 500) out.spillWhy.side++;
    else out.spillWhy.below++;
    noteFall(t, frame);
  };
  const steps = Math.round(seconds / DT);
  let dir = 1;
  let frame = 0;
  for (let f = 1; f <= steps; f++) {
    frame = f;
    // Player styles: "swipe" alternates drag bursts, "spinleft"/"spinright" hold one direction, "idle" no input.
    if (style === "swipe") {
      if (f % 90 < 25) eng.applyInputDrag(dir * (20 + 25 * rng()) * 1.3);
      if (f % 90 === 0) dir = -dir;
    } else if (style === "spinleft") {
      if (f % 6 === 0) eng.applyInputDrag(-18 * 1.3);
    } else if (style === "spinright") {
      if (f % 6 === 0) eng.applyInputDrag(18 * 1.3);
    }
    eng.update(DT);
    for (const d of eng.debrisList) {
      let t = track.get(d.id);
      if (!t) track.set(d.id, (t = { prevY: d.y }));
      if (d.state === "FALLING" && t.fallStart === undefined) t.fallStart = f;
      if (d.state === "FALLING" && t.crossX === undefined && t.prevY < bhY && d.y >= bhY) {
        t.crossX = d.x;
        t.crossVx = d.vx;
        const off = d.x - centerX;
        out.offsets.push(off);
        if (off < 0) out.crossLeft++;
        else out.crossRight++;
      }
      t.prevY = d.y;
    }
  }
  for (const d of eng.debrisList) {
    if (d.state !== "FALLING") continue;
    const t = track.get(d.id);
    if (!t || t.fallStart === undefined) continue;
    out.stillFalling++;
    const dur = (steps - t.fallStart + 1) * DT;
    if (dur > out.maxFall) out.maxFall = dur;
  }
  Math.random = saved;
  return out;
}

const pct = (a, p) => {
  if (!a.length) return NaN;
  const s = [...a].sort((x, y) => x - y);
  return s[Math.min(s.length - 1, Math.floor((p / 100) * s.length))];
};
const mean = (a) => (a.length ? a.reduce((x, y) => x + y, 0) / a.length : NaN);
const f1 = (v) => (Number.isNaN(v) ? "-" : v.toFixed(1));

const kPrint = lateralK();
const cPrint = lateralC(kPrint);
const shipped = variant === "web" && kOverride === null && args.c === undefined && mulOverride === null && postOverride === null;
const label = shipped
  ? "web shipped (dx*16 - vx*8)*dt"
  : `${variant} k=${kPrint} c=${cPrint} mul=${mulOverride === null ? 1 : mulOverride} post=${postOverride === null ? 1 : postOverride}`;
console.log(`variant=${label} layout center=(${centerX},${centerY}) blackHoleY=${bhY} seconds=${seconds} seeds=${seeds}`);
console.log("style,level,swallowed,spilled,spillPct,spillLeft,spillRight,spillOffMean,spillOffP10,spillOffP90,swalOffMean,swalOffAbsMax,spillBelow,spillSide,maxFallSec,stillFalling,meanFallSec");
const total = {};
const idleLevels = [];
for (const style of ["idle", "swipe", "spinleft", "spinright"]) {
  for (const level of levels) {
    const agg = { swallowed: 0, spilled: 0, spillLeft: 0, spillRight: 0, spillOffsets: [], swallowOffsets: [], below: 0, side: 0, maxFall: 0, stillFalling: 0, fallSum: 0, fallN: 0 };
    for (let s = 1; s <= seeds; s++) {
      const r = runOne(level, s, style);
      agg.swallowed += r.swallowed;
      agg.spilled += r.spilled;
      agg.spillLeft += r.spillLeft;
      agg.spillRight += r.spillRight;
      agg.spillOffsets.push(...r.spillOffsets);
      agg.swallowOffsets.push(...r.swallowOffsets);
      agg.below += r.spillWhy.below;
      agg.side += r.spillWhy.side;
      agg.maxFall = Math.max(agg.maxFall, r.maxFall);
      agg.stillFalling += r.stillFalling;
      agg.fallSum += r.fallSum;
      agg.fallN += r.fallN;
    }
    const tot = agg.swallowed + agg.spilled;
    const spillPct = tot ? (100 * agg.spilled) / tot : NaN;
    console.log(
      [style, level, agg.swallowed, agg.spilled, tot ? spillPct.toFixed(1) : "-", agg.spillLeft, agg.spillRight,
        f1(mean(agg.spillOffsets)), f1(pct(agg.spillOffsets, 10)), f1(pct(agg.spillOffsets, 90)),
        f1(mean(agg.swallowOffsets)), f1(Math.max(0, ...agg.swallowOffsets.map(Math.abs))), agg.below, agg.side,
        agg.maxFall.toFixed(3), agg.stillFalling, agg.fallN ? (agg.fallSum / agg.fallN).toFixed(3) : "-"].join(","),
    );
    const t = (total[style] ||= { swallowed: 0, spilled: 0, left: 0, right: 0, maxFall: 0, stillFalling: 0, fallSum: 0, fallN: 0 });
    t.swallowed += agg.swallowed;
    t.spilled += agg.spilled;
    t.left += agg.spillLeft;
    t.right += agg.spillRight;
    t.maxFall = Math.max(t.maxFall, agg.maxFall);
    t.stillFalling += agg.stillFalling;
    t.fallSum += agg.fallSum;
    t.fallN += agg.fallN;
    if (style === "idle") idleLevels.push({ level, released: tot, spilled: agg.spilled, spillPct, maxFall: agg.maxFall, stillFalling: agg.stillFalling });
  }
}
console.log("\nTOTALS by style: style, swallowed, spilled, spill%, spilledLeftOfHole, spilledRightOfHole, maxFallSec, stillFalling, meanFallSec, swallowPerSec");
const styleSpill = {};
for (const [style, t] of Object.entries(total)) {
  const tot = t.swallowed + t.spilled;
  const spillPct = tot ? (100 * t.spilled) / tot : NaN;
  styleSpill[style] = spillPct;
  console.log([style, t.swallowed, t.spilled, tot ? spillPct.toFixed(1) + "%" : "-", t.left, t.right, t.maxFall.toFixed(3), t.stillFalling, t.fallN ? (t.fallSum / t.fallN).toFixed(3) : "-", (t.swallowed / (seconds * levels.length * seeds)).toFixed(4)].join(", "));
}
console.log("IDLE_LEVELS " + idleLevels.map((r) => `L${r.level}:${r.released ? r.spillPct.toFixed(1) + "%/" + r.released : "none"}`).join(" "));
const line = ["SUMMARY", label,
  ["idle", "swipe", "spinleft", "spinright"].map((s) => `${s}=${Number.isNaN(styleSpill[s]) ? "-" : styleSpill[s].toFixed(1)}`).join(" "),
  "maxFall=" + Math.max(...Object.values(total).map((t) => t.maxFall)).toFixed(3),
  "still=" + Object.values(total).reduce((n, t) => n + t.stillFalling, 0),
].join(" | ");
console.log(line);
if (assertMode) {
  const problems = [];
  for (const row of idleLevels) {
    if (row.released > 0 && row.spilled > 0) problems.push(`idle L${row.level} spilled ${row.spilled}/${row.released}`);
  }
  for (const style of ["swipe", "spinleft", "spinright"]) {
    const pct = styleSpill[style];
    if (!(pct > 0 && pct <= 10)) problems.push(`${style} spill ${Number.isNaN(pct) ? "none" : pct.toFixed(1)}% outside (0, 10]`);
  }
  const maxFall = Math.max(...Object.values(total).map((t) => t.maxFall));
  if (maxFall > 5) problems.push(`max FALLING ${maxFall.toFixed(3)}s > 5s`);
  if (problems.length) {
    console.error("ASSERT FAIL");
    for (const p of problems) console.error(p);
    process.exit(1);
  }
  console.log("ASSERT PASS");
}
