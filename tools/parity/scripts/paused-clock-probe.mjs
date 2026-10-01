import { chromium } from 'playwright';
import { existsSync, mkdirSync, readFileSync } from 'node:fs';
import { dirname, extname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { WEB_DIR } from './emoji-scan.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const OUT = join(here, '..', 'out', 'probe');
mkdirSync(OUT, { recursive: true });
const MIME = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css', '.png': 'image/png' };
const ORIGIN = 'http://parity.test';
const T0 = new Date('2026-10-01T09:00:00-07:00');

const browser = await chromium.launch();
const context = await browser.newContext({ timezoneId: 'America/Los_Angeles', locale: 'en-US', viewport: { width: 430, height: 932 }, deviceScaleFactor: 3 });
const page = await context.newPage();
await page.route('**/*', (route) => {
  const u = new URL(route.request().url());
  if (u.origin !== ORIGIN) return route.abort();
  const file = join(WEB_DIR, u.pathname === '/' ? 'index.html' : u.pathname.slice(1));
  if (!existsSync(file)) return route.fulfill({ status: 404, body: '' });
  return route.fulfill({ status: 200, contentType: MIME[extname(file)] ?? 'application/octet-stream', body: readFileSync(file) });
});
await page.clock.install({ time: T0 });
await page.clock.pauseAt(new Date(T0.getTime() + 1000));
await page.addInitScript(() => {
  let a = 0x5eed1234;
  Math.random = () => {
    a |= 0; a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
  window.__vibrations = [];
  navigator.vibrate = (ms) => { window.__vibrations.push({ ms, t: performance.now() }); return true; };
});
await page.goto(`${ORIGIN}/`, { waitUntil: 'load' });

const result = { chromium: browser.version() };
let steps = 0;
while ((await page.evaluate(() => window.app.currentPage)) !== 'home' && steps < 2000) {
  await page.clock.runFor(16);
  steps++;
}
result.bootFramesToHome = steps;
result.perfAtHome = await page.evaluate(() => performance.now());

const box = await page.evaluate(() => {
  const r = document.getElementById('btn-open-maps-page').getBoundingClientRect();
  return { x: r.x + r.width / 2, y: r.y + r.height / 2 };
});
const tClick = Date.now();
await page.mouse.move(box.x, box.y);
await page.mouse.down();
await page.mouse.up();
result.mouseClickRealMs = Date.now() - tClick;
result.pageAfterClick = await page.evaluate(() => window.app.currentPage);

const tShot = Date.now();
const shot = await page.screenshot({ path: join(OUT, 'paused-clock-maps.png'), animations: 'allow' });
result.screenshotRealMs = Date.now() - tShot;
result.screenshotBytes = shot.length;
result.screenshotSize = { w: shot.readUInt32BE(16), h: shot.readUInt32BE(20) };

const tClick2 = Date.now();
let locatorClick = 'ok';
try {
  await page.locator('#btn-back-maps').click({ timeout: 3000 });
} catch (e) {
  locatorClick = `failed: ${String(e.message).split('\n')[0]}`;
}
result.locatorClick = locatorClick;
result.locatorClickRealMs = Date.now() - tClick2;
result.pageAfterLocatorClick = await page.evaluate(() => window.app.currentPage);
console.log(JSON.stringify(result, null, 2));
await browser.close();
