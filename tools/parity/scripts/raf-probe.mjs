import { chromium } from 'playwright';
import { dirname, join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const url = pathToFileURL(join(here, 'raf-probe.html')).href;
const T0 = new Date('2026-10-01T09:00:00-07:00');

async function run(label, setup) {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: 'America/Los_Angeles', locale: 'en-US', viewport: { width: 430, height: 932 }, deviceScaleFactor: 3 });
  const page = await context.newPage();
  await setup(page);
  await page.goto(url);
  const read = () => page.evaluate(() => ({ ...JSON.parse(JSON.stringify(window.__probe)), cssLeft: getComputedStyle(document.getElementById('anim')).left, timeline: document.timeline.currentTime }));
  const out = { label, version: browser.version() };
  out.afterLoad = await read();
  await new Promise((r) => setTimeout(r, 300));
  const idle = await read();
  out.realWaitAdvancedFrames = idle.raf.length - out.afterLoad.raf.length;
  await page.clock.runFor(1000);
  const p = await read();
  out.cssAfterRealWait = { left: idle.cssLeft, timeline: idle.timeline };
  out.afterRunFor1000 = { cssLeft: p.cssLeft, timeline: p.timeline, frames: p.raf.length, timeouts: p.timeouts.length, intervals: p.intervals.length };
  const args = p.raf.map((f) => f.arg);
  const deltas = args.slice(1).map((v, i) => +(v - args[i]).toFixed(6));
  out.rafArgs = args.slice(0, 8);
  out.rafArgDeltas = [...new Set(deltas)];
  out.rafPerfEqualsArg = p.raf.every((f) => f.perf === f.arg);
  out.rafDateDeltas = [...new Set(p.raf.slice(1).map((f, i) => f.date - p.raf[i].date))];
  out.boot = p.boot;
  out.timeoutAt = p.timeouts[0];
  const before = (await read()).raf.length;
  await page.clock.runFor(16);
  const a16 = (await read()).raf.length - before;
  await page.clock.runFor(17);
  const a17 = (await read()).raf.length - before - a16;
  out.runFor16Frames = a16;
  out.runFor17Frames = a17;
  const t = await page.evaluate(() => ({ perf: performance.now(), date: Date.now(), iso: new Date().toString(), tz: Intl.DateTimeFormat().resolvedOptions().timeZone, locale: navigator.language }));
  out.final = t;
  await browser.close();
  return out;
}

const results = [];
results.push(await run('install-only (clock running)', async (page) => {
  await page.clock.install({ time: T0 });
}));
results.push(await run('install + pauseAt(T0+0 via setFixed? no) -> pauseAt(T0+1s) before goto', async (page) => {
  await page.clock.install({ time: T0 });
  await page.clock.pauseAt(new Date(T0.getTime() + 1000));
}));
console.log(JSON.stringify(results, null, 2));
