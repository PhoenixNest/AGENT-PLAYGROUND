import { chromium } from 'playwright';
import { readFileSync, existsSync } from 'node:fs';
import { dirname, extname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { WEB_DIR } from './emoji-scan.mjs';

const MIME = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css', '.png': 'image/png' };
const ORIGIN = 'http://parity.test';
const T0 = new Date('2026-10-01T09:00:00-07:00');

async function once() {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: 'America/Los_Angeles', locale: 'en-US', viewport: { width: 430, height: 932 }, deviceScaleFactor: 3 });
  const page = await context.newPage();
  await page.route('**/*', (route) => {
    const u = new URL(route.request().url());
    if (u.origin !== ORIGIN) return route.abort();
    const rel = u.pathname === '/' ? 'index.html' : u.pathname.slice(1);
    const file = join(WEB_DIR, rel);
    if (!existsSync(file)) return route.fulfill({ status: 404, body: '' });
    return route.fulfill({ status: 200, contentType: MIME[extname(file)] ?? 'application/octet-stream', body: readFileSync(file) });
  });
  await page.clock.install({ time: T0 });
  await page.clock.pauseAt(new Date(T0.getTime() + 1000));
  await page.addInitScript(() => {
    window.__frames = [];
    const raf = window.requestAnimationFrame.bind(window);
    window.requestAnimationFrame = (cb) => raf((t) => { window.__frames.push({ t, perf: performance.now() }); cb(t); });
  });
  await page.goto(`${ORIGIN}/`, { waitUntil: 'load' });
  const boot = await page.evaluate(() => ({ perf: performance.now(), lastTime: window.app?.lastTime, hasApp: !!window.app, frames: window.__frames.length }));
  const dts = [];
  for (let i = 0; i < 5; i++) {
    const before = await page.evaluate(() => window.app.lastTime);
    await page.clock.runFor(16);
    const after = await page.evaluate(() => ({ lastTime: window.app.lastTime, frames: window.__frames.length }));
    dts.push({ lastTimeBefore: before, lastTimeAfter: after.lastTime, dtMs: after.lastTime - before, frames: after.frames });
  }
  await browser.close();
  return { boot, dts };
}

const runs = [];
for (let i = 0; i < 3; i++) runs.push(await once());
console.log(JSON.stringify(runs, null, 1));
