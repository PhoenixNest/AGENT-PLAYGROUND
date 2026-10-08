import { launchBrowser, Session } from "../web/harness.mjs";
import { spinProgram } from "./program-lib.mjs";

const sector = Number(process.argv[2] ?? 0);
const candidates = JSON.parse(process.argv[3]);
const maxTicks = Number(process.argv[4] ?? 700);

const browser = await launchBrowser();

async function trial(params) {
  const s = new Session(browser, { profile: "zero", stateId: "probe" });
  try {
    await s.runStep({ op: "boot" });
    await s.runStep({ op: "loadSector", index: sector });
    await s.runStep({ op: "showPage", page: "game" });
    await s.runStep({ op: "call", method: "dismissGestureHint", args: [] });
    await s.advance(400);
    const prog = spinProgram({ ...params, ticks: maxTicks });
    let tick = 0;
    let last = null;
    let first = null;
    const events = [];
    for (const step of prog) {
      await s.runStep(step);
      if (step.op === "advance") {
        tick++;
        last = await s.page.evaluate(() => ({
          t: performance.now(),
          mass: window.app.collectedMass,
          cleared: window.app.isLevelCleared,
          failed: window.app.isLevelFailed,
          n: window.app.physics.debrisList.length,
          ft: (window.app.renderer.floatingTexts || []).map((f) => f.text),
        }));
        if (last.ft.length && params.log)
          events.push({ t: last.t, ft: last.ft });
        if (last.mass > 0 && !first) first = last.t;
        if (last.cleared || last.failed) break;
      }
    }
    return {
      params,
      firstSwallow: first,
      last,
      events: params.log ? events : undefined,
    };
  } finally {
    await s.close();
  }
}

const results = await Promise.all(candidates.map(trial));
for (const r of results) console.log(JSON.stringify(r));
await browser.close();
