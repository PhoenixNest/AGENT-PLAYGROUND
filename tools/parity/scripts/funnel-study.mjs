// Headless study of where debris is lost between the ring aperture and the black hole.
// Runs the unmodified web engine (games/maze-puzzle/web/engine) under Node at a steady 1/60 s.
// Usage: node tools/parity/scripts/funnel-study.mjs [--variant name] [--levels 1,2,3] [--seconds 40]
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

class Variant extends PhysicsEngine {
  resolveMagneticFunnel(d, dt) {
    if (variant === "web") return super.resolveMagneticFunnel(d, dt);
    const topY = this.center.y + this.ringRadius - 15;
    const bottomY = this.blackHole.y;
    const extend = variant === "rails-extend" || variant === "combo" ? 60 : 25;
    if (d.y >= topY && d.y <= bottomY + extend) {
      const t = Math.max(0, Math.min(1, (d.y - topY) / Math.max(1, bottomY - topY)));
      const halfWidth = 95 * (1 - t) + (variant === "narrow" || variant === "combo" ? 30 : 46) * t;
      const centerXf = this.blackHole.x;
      const dx = centerXf - d.x;
      const k = variant === "strong-pull" || variant === "combo" ? 40 : 16;
      d.vx += dx * k * dt;
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
  const out = { swallowed: 0, spilled: 0, spillLeft: 0, spillRight: 0, crossLeft: 0, crossRight: 0, offsets: [], spillOffsets: [], swallowOffsets: [], spillVx: [], spillWhy: { side: 0, below: 0 } };
  eng.onDebrisSwallowed = (d) => {
    out.swallowed++;
    const t = track.get(d.id);
    if (t && t.crossX !== undefined) out.swallowOffsets.push(t.crossX - centerX);
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
  };
  const steps = Math.round(seconds / DT);
  let dir = 1;
  for (let f = 1; f <= steps; f++) {
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

console.log(`variant=${variant} layout center=(${centerX},${centerY}) blackHoleY=${bhY} seconds=${seconds} seeds=${seeds}`);
console.log("style,level,swallowed,spilled,spillPct,spillLeft,spillRight,spillOffMean,spillOffP10,spillOffP90,swalOffMean,swalOffAbsMax,spillBelow,spillSide");
const total = {};
for (const style of ["idle", "swipe", "spinleft", "spinright"]) {
  for (const level of levels) {
    const agg = { swallowed: 0, spilled: 0, spillLeft: 0, spillRight: 0, spillOffsets: [], swallowOffsets: [], below: 0, side: 0 };
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
    }
    const tot = agg.swallowed + agg.spilled;
    console.log(
      [style, level, agg.swallowed, agg.spilled, tot ? ((100 * agg.spilled) / tot).toFixed(1) : "-", agg.spillLeft, agg.spillRight,
        f1(mean(agg.spillOffsets)), f1(pct(agg.spillOffsets, 10)), f1(pct(agg.spillOffsets, 90)),
        f1(mean(agg.swallowOffsets)), f1(Math.max(0, ...agg.swallowOffsets.map(Math.abs))), agg.below, agg.side].join(","),
    );
    const t = (total[style] ||= { swallowed: 0, spilled: 0, left: 0, right: 0 });
    t.swallowed += agg.swallowed;
    t.spilled += agg.spilled;
    t.left += agg.spillLeft;
    t.right += agg.spillRight;
  }
}
console.log("\nTOTALS by style: style, swallowed, spilled, spill%, spilledLeftOfHole, spilledRightOfHole");
for (const [style, t] of Object.entries(total)) {
  const tot = t.swallowed + t.spilled;
  console.log([style, t.swallowed, t.spilled, tot ? ((100 * t.spilled) / tot).toFixed(1) + "%" : "-", t.left, t.right].join(", "));
}
