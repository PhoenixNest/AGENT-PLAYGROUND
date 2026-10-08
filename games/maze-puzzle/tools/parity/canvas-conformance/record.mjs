// Records the CanvasRenderingContext2D command stream of single rendered frames of the real web app,
// together with the Chromium pixels of that canvas (reference PNGs, git-ignored).
//   node record.mjs [--sectors 1,11,21,31,41] [--out streams] [--only s01_f001]
import fs from "node:fs";
import path from "node:path";
import {
  launch, newContext, installRoutes, emojiIndex, confDir, outDir, INIT_SCRIPT, FONT_LOADS,
} from "./lib/harness-lite.mjs";

const args = process.argv.slice(2);
const opt = (name, def) => {
  const i = args.indexOf("--" + name);
  return i >= 0 ? args[i + 1] : def;
};
const sectors = opt("sectors", "1,11,21,31,41").split(",").map(Number);
const streamsDir = path.resolve(confDir, opt("out", "streams"));
const refDir = path.join(outDir, "ref");
const only = opt("only", null);
fs.mkdirSync(streamsDir, { recursive: true });
fs.mkdirSync(refDir, { recursive: true });

const wrapSource = fs.readFileSync(path.join(confDir, "lib/wrap.js"), "utf8");
const T0 = new Date("2026-10-01T09:00:00-07:00").getTime();

const VFX_TEXTS = [
  ["NOM! 😋", "#fde047"], ["YUM! ✨", "#fde047"], ["GULP! 🍩", "#fde047"], ["TASTY! ⭐", "#fde047"],
  ["×5! SHRED ⚡", "#fde047"], ["+7! COMPACT ✨", "#4ade80"], ["BOOST! ⏩", "#38bdf8"],
  ["POOF! 💨", "#f472b6"], ["TURBO! ⏩", "#38bdf8"], ["⏪ TURBO!", "#38bdf8"],
];

// frame plan per sector: [label, framesToAdvanceBefore, action]
const PLAN = [
  { label: "f001", advance: 1, arm: true },
  { label: "f060", advance: 59, arm: true },
  { label: "f180", advance: 120, arm: true },
  { label: "vfx1", advance: 1, arm: true, before: "vfx", forceRcs: true },
  { label: "vfx2", advance: 9, arm: true, forceRcs: true },
  { label: "pause", advance: 12, arm: true, before: "pause" },
];

async function recordSector(browser, sectorNumber) {
  const context = await newContext(browser);
  await installRoutes(context);
  await context.clock.install({ time: new Date(T0) });
  await context.clock.pauseAt(new Date(T0 + 1000));
  await context.addInitScript({ content: INIT_SCRIPT });
  await context.addInitScript({ content: wrapSource + "\n;__cc.installParity(); __cc.installRecorder();" });
  const page = await context.newPage();
  page.on("pageerror", (e) => console.error("[pageerror]", e.message));
  await page.goto("http://parity.test/index.html");
  await page.waitForFunction(() => !!window.app);
  await page.evaluate(async ({ index, fonts }) => {
    await Promise.all(fonts.map((f) => document.fonts.load(f, "AaBb✓▶←×•")));
    await window.__cc.preloadEmoji(index);
    await document.fonts.ready;
  }, { index: emojiIndex(), fonts: FONT_LOADS });
  await page.clock.runFor(1312);
  const home = await page.evaluate(() => window.app.currentPage);
  if (home !== "home") throw new Error("boot did not reach home: " + home);

  await page.evaluate((idx) => {
    const app = window.app;
    const r = app.renderer;
    const orig = r.render.bind(r);
    window.__frameNo = 0;
    r.render = (...a) => {
      window.__frameNo++;
      if (window.__forceRcs) app.physics.rcsThrusting = true;
      if (!window.__arm) return orig(...a);
      const label = window.__arm;
      window.__arm = null;
      const cc = window.__cc;
      const pre = cc.snapshotState(r.ctx);
      cc.startRecording(r.ctx);
      let ops;
      try {
        orig(...a);
      } finally {
        ops = cc.stopRecording();
      }
      window.__captured = {
        label,
        frame: window.__frameNo,
        meta: { width: r.width, height: r.height, dpr: r.dpr, canvasW: r.canvas.width, canvasH: r.canvas.height },
        preamble: pre,
        ops,
        png: r.canvas.toDataURL("image/png"),
      };
    };
    app.loadSector(app.sectors[idx], false);
    app.showPage("game");
  }, sectorNumber - 1);

  const pad = String(sectorNumber).padStart(2, "0");
  const results = [];
  for (const step of PLAN) {
    if (step.before === "vfx") {
      await page.evaluate((texts) => {
        const app = window.app, r = app.renderer, p = app.physics;
        const cx = p.center.x, cy = p.center.y;
        r.addConfetti(cx, cy, 24);
        r.addConfetti(p.blackHole.x, p.blackHole.y - 30, 12);
        r.addBoostShockwave(cx, cy, 120, "#38bdf8");
        r.addPopRing(cx + 60, cy + 80, 50, "#fde047");
        r.addThrusterPuff(cx - 40, cy + 30, 10, 40, 7, "#ffffff");
        r.addThrusterPuff(cx + 40, cy + 30, -10, 40, 9, "#fde047");
        r.addRimSpark(cx, cy - 100, 20, -30, "#fde047");
        r.triggerNibblesChew();
        texts.forEach(([t, c], i) => r.addFloatingText(60 + (i % 3) * 150, 120 + Math.floor(i / 3) * 70, t, c));
        window.__forceRcs = true;
      }, VFX_TEXTS);
    }
    if (step.before === "pause") {
      await page.evaluate(() => { window.__forceRcs = false; window.app.isPaused = true; });
    }
    if (step.advance > 1) await page.clock.runFor(16 * (step.advance - 1));
    await page.evaluate((label) => { window.__arm = label; window.__captured = null; }, step.label);
    await page.clock.runFor(16);
    const cap = await page.evaluate(() => window.__captured);
    if (!cap) throw new Error(`no capture for ${pad}_${step.label}`);
    const name = `s${pad}_${step.label}`;
    if (only && only !== name) continue;
    const stream = { version: 1, name, sector: sectorNumber, frame: cap.frame, meta: cap.meta, preamble: cap.preamble, ops: cap.ops };
    fs.writeFileSync(path.join(streamsDir, name + ".json"), JSON.stringify(stream));
    fs.writeFileSync(path.join(refDir, name + ".png"), Buffer.from(cap.png.split(",")[1], "base64"));
    results.push({ name, ops: cap.ops.length, frame: cap.frame });
    console.log(name, "frame", cap.frame, "ops", cap.ops.length);
  }
  await context.close();
  return results;
}

const browser = await launch();
const summary = [];
for (const s of sectors) summary.push(...(await recordSector(browser, s)));
await browser.close();

const hist = { methods: {}, props: {}, colors: {}, fonts: {}, values: {} };
const bump = (o, k) => { o[k] = (o[k] || 0) + 1; };
const colorKind = (v) => {
  if (typeof v !== "string") return typeof v === "object" ? "gradient" : typeof v;
  if (/^#[0-9a-f]{6}$/i.test(v)) return "hex6";
  if (/^#[0-9a-f]{8}$/i.test(v)) return "hex8";
  if (/^#[0-9a-f]{3}$/i.test(v)) return "hex3";
  if (/^rgba\(/.test(v)) return "rgba()";
  if (/^rgb\(/.test(v)) return "rgb()";
  return "other:" + v;
};
for (const f of fs.readdirSync(streamsDir).filter((n) => n.endsWith(".json") && n !== "index.json")) {
  const s = JSON.parse(fs.readFileSync(path.join(streamsDir, f), "utf8"));
  for (const op of s.ops) {
    const n = op[0];
    if (n.startsWith("=")) {
      bump(hist.props, n.slice(1));
      if (["fillStyle", "strokeStyle", "shadowColor"].includes(n.slice(1))) bump(hist.colors, n.slice(1) + ":" + colorKind(op[1]));
      else if (["font", "lineCap", "lineJoin", "textAlign", "textBaseline", "globalCompositeOperation"].includes(n.slice(1))) bump(hist.values, n.slice(1) + "=" + op[1]);
    } else {
      bump(hist.methods, n);
      if (n === "addColorStop") bump(hist.colors, "addColorStop:" + colorKind(op[3]));
    }
  }
}
fs.writeFileSync(path.join(streamsDir, "index.json"), JSON.stringify({ streams: summary, histogram: hist }, null, 1));
console.log("done", summary.length, "streams");
