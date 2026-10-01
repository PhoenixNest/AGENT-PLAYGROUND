import { Session, launchBrowser } from "../web/harness.mjs";

const BEZIER = [0.34, 1.56, 0.64, 1];
const DURATION_MS = 250;
const SAMPLES_MS = [16, 32, 64, 125, 200, 250, 300, 1000];
const FROM = 0;
const TO = 0.95;
const TOLERANCE = 0.01;

const bezier = (p1, p2, u) =>
  3 * (1 - u) ** 2 * u * p1 + 3 * (1 - u) * u * u * p2 + u ** 3;

const ease = (x) => {
  let lo = 0;
  let hi = 1;
  for (let i = 0; i < 60; i++) {
    const mid = (lo + hi) / 2;
    if (bezier(BEZIER[0], BEZIER[2], mid) < x) lo = mid;
    else hi = mid;
  }
  return bezier(BEZIER[1], BEZIER[3], (lo + hi) / 2);
};

const expected = (ms) =>
  FROM + (TO - FROM) * ease(Math.min(1, ms / DURATION_MS));

const browser = await launchBrowser();
const session = new Session(browser, {
  profile: "zero",
  stateId: "check_transition",
});
const setup = [
  { op: "boot" },
  { op: "loadSector", index: 0 },
  { op: "showPage", page: "game" },
  { op: "call", method: "dismissGestureHint", args: [] },
  { op: "advance", ms: 400 },
  { op: "setApp", field: "collectedMass", value: 19 },
  { op: "call", method: "updateQuotaDisplay", args: [] },
];
for (const step of setup) await session.runStep(step);

let elapsed = 0;
let failed = false;
for (const ms of SAMPLES_MS) {
  await session.runStep({ op: "advance", ms: ms - elapsed });
  elapsed = ms;
  const got = await session.page.evaluate(() => {
    const el = document.getElementById("hud-quota-bar");
    return el.getBoundingClientRect().width / el.parentElement.clientWidth;
  });
  const want = expected(ms);
  const ok = Math.abs(got - want) <= TOLERANCE;
  failed ||= !ok;
  console.log(
    `${ok ? "ok  " : "FAIL"} t=${String(ms).padStart(4)} ms  got=${got.toFixed(4)}  want=${want.toFixed(4)}`,
  );
}
await session.close();
await browser.close();
if (failed) process.exit(1);
