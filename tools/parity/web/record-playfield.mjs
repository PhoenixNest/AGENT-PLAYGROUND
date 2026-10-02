// Records the last Canvas2D frame of each screenshot state that shows the playfield.
// Uses the same Session clock as capture, so the stream matches the web PNG.
//   node web/record-playfield.mjs
import { readFileSync, writeFileSync, mkdirSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { ROOT, Session, launchBrowser } from "./harness.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const wrap = readFileSync(join(ROOT, "canvas-conformance/lib/wrap.js"), "utf8");
const emojiIndex = JSON.parse(readFileSync(join(ROOT, "assets/emoji/index.json"), "utf8"));
const emojiByFile = {};
for (const [grapheme, info] of Object.entries(emojiIndex)) {
  emojiByFile[info.file.replace(/\.png$/i, "").toLowerCase()] = grapheme;
}
const states = JSON.parse(readFileSync(join(ROOT, "unity/screenshot-states.json"), "utf8"));
const want = new Set([
  "game_lvl01_t1000ms",
  "modal_pause",
  "modal_debrief_exact",
  "modal_defeat_12_of_20",
]);
const outDir = join(
  ROOT,
  "../../games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Parity/Playfield",
);
mkdirSync(outDir, { recursive: true });

const hook = () => {
  const r = window.app.renderer;
  const orig = r.render.bind(r);
  r.render = function (...args) {
    const cc = window.__cc;
    const pre = cc.snapshotState(r.ctx);
    cc.startRecording(r.ctx);
    try {
      return orig(...args);
    } finally {
      window.__lastFrame = {
        preamble: pre,
        ops: cc.stopRecording(),
        meta: {
          width: r.width,
          height: r.height,
          dpr: r.dpr,
          canvasW: r.canvas.width,
          canvasH: r.canvas.height,
        },
      };
    }
  };
};

const browser = await launchBrowser();
try {
  for (const state of states) {
    if (!want.has(state.id)) continue;
    const session = new Session(browser, { profile: "zero", stateId: state.id });
    session.extraInit = wrap + `\n;__cc.installRecorder(); __cc.emojiByFile = ${JSON.stringify(emojiByFile)};`;
    let booted = false;
    for (const step of state.steps) {
      if (step.op === "capture") continue;
      if (step.op === "boot") {
        await session.boot(step);
        await session.page.evaluate(hook);
        booted = true;
        continue;
      }
      if (!booted) throw new Error(state.id + " stepped before boot");
      await session.runStep(step);
    }
    const frame = await session.page.evaluate(() => window.__lastFrame);
    if (!frame || !frame.ops) throw new Error(state.id + " recorded no frame");
    const doc = {
      version: 1,
      name: state.id,
      meta: frame.meta,
      preamble: frame.preamble,
      ops: frame.ops,
    };
    const path = join(outDir, state.id + ".json");
    writeFileSync(path, JSON.stringify(doc));
    console.log("ok", state.id, frame.ops.length, "ops", frame.meta.canvasW + "x" + frame.meta.canvasH);
    await session.context.close();
  }
} finally {
  await browser.close();
}
