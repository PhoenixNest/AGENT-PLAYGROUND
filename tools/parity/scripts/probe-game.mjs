import { launchBrowser, Session } from "../web/harness.mjs";

const mode = process.argv[2] ?? "idle";
const sector = Number(process.argv[3] ?? 0);
const amp = Number(process.argv[4] ?? 25);
const period = Number(process.argv[5] ?? 30);
const maxMs = Number(process.argv[6] ?? 20000);

const browser = await launchBrowser();
const s = new Session(browser, { profile: "zero", stateId: "probe" });
await s.runStep({ op: "boot" });
await s.runStep({ op: "loadSector", index: sector });
await s.runStep({ op: "showPage", page: "game" });
await s.runStep({ op: "call", method: "dismissGestureHint", args: [] });
await s.advance(400);

const info = () =>
  s.page.evaluate(() => {
    const a = window.app;
    const p = a.physics;
    const counts = {};
    for (const d of p.debrisList) counts[d.state] = (counts[d.state] ?? 0) + 1;
    return {
      t: performance.now(),
      mass: a.collectedMass,
      quota: a.activeSector.quota,
      cleared: a.isLevelCleared,
      failed: a.isLevelFailed,
      ft: a.failureCheckTimer,
      ring: p.ringAngle,
      av: p.angularVelocity,
      counts,
      floats: a.renderer.floatingTexts ? a.renderer.floatingTexts.length : -1,
    };
  });

let x = 215;
const y = 500;
let down = false;
const log = [];
const frames = Math.floor(maxMs / 16);
for (let k = 0; k < frames; k++) {
  if (mode === "sine") {
    const nx = 215 + amp * 4 * Math.sin((2 * Math.PI * k) / period);
    if (!down) {
      await s.page.mouse.move(x, y);
      await s.page.mouse.down();
      down = true;
    }
    await s.page.mouse.move(Math.round(nx), y);
    x = Math.round(nx);
  }
  await s.advance(16);
  if (k % 10 === 0 || k === frames - 1) {
    const i = await info();
    log.push(i);
    if (i.cleared || i.failed) break;
  }
}
for (const i of log.filter((_, n) => n % 5 === 0))
  console.log(JSON.stringify(i));
console.log("last", JSON.stringify(log.at(-1)));
await s.close();
await browser.close();
