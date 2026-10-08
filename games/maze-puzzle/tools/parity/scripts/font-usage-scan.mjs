// Enumerates every (family, weight) combination the web prototype actually uses and the face
// that CSS font matching resolves it to over the Google Fonts face set (SPEC section 5).
//   node scripts/font-usage-scan.mjs            -> prints the tables, writes out/font-usage.json
// DOM: computed style of every element that owns a text node (all pages, store tabs, world tabs, modals, toast).
// Canvas: every ctx.font assignment in engine/renderer.js.
import { chromium } from 'playwright';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const WEB_DIR = resolve(here, '..', '..', '..', 'games', 'maze-puzzle', 'web');
const ORIGIN = 'http://parity.test';
const MIME = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css', '.png': 'image/png' };

// Faces served by Google for the styles.css @import URL (SPEC 5.2, evidence in tools/parity/evidence).
export const FACES = { Fredoka: [400, 600, 700], Nunito: [700, 800, 900] };

// CSS Fonts 4 section 5.2 weight matching over the available weights of one family.
export function matchWeight(desired, available) {
  const asc = [...available].sort((a, b) => a - b);
  const desc = [...asc].reverse();
  let order;
  if (desired >= 400 && desired <= 500) {
    order = [
      ...asc.filter((w) => w >= desired && w <= 500),
      ...desc.filter((w) => w < desired),
      ...asc.filter((w) => w > 500),
    ];
  } else if (desired < 400) {
    order = [...desc.filter((w) => w <= desired), ...asc.filter((w) => w > desired)];
  } else {
    order = [...asc.filter((w) => w >= desired), ...desc.filter((w) => w < desired)];
  }
  return order[0];
}

export function resolveFace(family, weight) {
  const faces = FACES[family];
  return faces ? matchWeight(weight, faces) : null;
}

const firstFamily = (list) => list.split(',')[0].trim().replace(/^["']|["']$/g, '');

function canvasFonts() {
  const src = readFileSync(join(WEB_DIR, 'engine', 'renderer.js'), 'utf8').split('\n');
  const out = [];
  src.forEach((line, i) => {
    const m = line.match(/ctx\.font\s*=\s*"([^"]+)"/);
    if (!m) return;
    const font = m[1];
    const px = font.match(/(\d+(?:\.\d+)?)px/)[1];
    const tok = font.split(/\s+/)[0];
    const weight = tok === 'bold' ? 700 : /^\d+$/.test(tok) ? Number(tok) : 400;
    const family = firstFamily(font.slice(font.indexOf('px') + 2));
    out.push({ line: i + 1, font, px: Number(px), weight, family });
  });
  return out;
}

async function domFonts() {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: 'America/Los_Angeles', locale: 'en-US', viewport: { width: 430, height: 932 } });
  const page = await context.newPage();
  await page.route('**/*', (route) => {
    const u = new URL(route.request().url());
    if (u.origin !== ORIGIN) return route.abort();
    const rel = u.pathname === '/' ? 'index.html' : u.pathname.slice(1);
    const file = join(WEB_DIR, rel);
    if (!existsSync(file)) return route.fulfill({ status: 404, body: '' });
    return route.fulfill({ status: 200, contentType: MIME[extname(file)] ?? 'application/octet-stream', body: readFileSync(file) });
  });
  await page.goto(`${ORIGIN}/`, { waitUntil: 'load' });
  await page.waitForFunction(() => window.app);
  const rows = await page.evaluate(() => {
    const app = window.app;
    const seen = new Map();
    const elSeen = new WeakSet();
    const describe = (el) => el.tagName.toLowerCase() + (el.id ? `#${el.id}` : '') + (el.className && typeof el.className === 'string' ? `.${el.className.trim().split(/\s+/).join('.')}` : '');
    const collect = (tag) => {
      for (const el of document.querySelectorAll('body *')) {
        const own = [...el.childNodes].some((n) => n.nodeType === 3 && n.textContent.trim());
        const isInput = el.tagName === 'INPUT' && el.type !== 'range' && el.type !== 'checkbox';
        if (!own && !isInput) continue;
        if (elSeen.has(el)) continue;
        elSeen.add(el);
        const cs = getComputedStyle(el);
        const key = `${cs.fontFamily}|${cs.fontWeight}`;
        const e = seen.get(key) ?? { family: cs.fontFamily, weight: cs.fontWeight, count: 0, examples: [], texts: [], tags: new Set() };
        e.count++;
        const ownText = [...el.childNodes].filter((n) => n.nodeType === 3).map((n) => n.textContent.trim()).join(' ').trim();
        if (ownText && e.texts.length < 8 && !e.texts.includes(ownText.slice(0, 30))) e.texts.push(ownText.slice(0, 30));
        e.tags.add(tag);
        if (e.examples.length < 4 && !e.examples.includes(describe(el))) e.examples.push(describe(el));
        seen.set(key, e);
      }
    };
    for (const p of ['loading', 'home', 'game', 'maps', 'daily', 'shop', 'achievements', 'settings']) {
      app.showPage(p);
      if (p === 'shop') {
        for (const b of document.querySelectorAll('.tab-btn')) {
          b.click();
          collect(`shop:${b.dataset.tab}`);
        }
      }
      if (p === 'maps') {
        for (const w of app.worlds) {
          app.selectedWorldIndex = w.worldIndex;
          app.renderSectorsPage();
          collect(`maps:world${w.worldIndex}`);
        }
      }
      collect(p);
    }
    app.showToast('toast text');
    document.querySelectorAll('.modal-overlay').forEach((m) => m.classList.add('active'));
    collect('modals+toast');
    return [...seen.values()].map((e) => ({ ...e, tags: [...e.tags] }));
  });
  await browser.close();
  return rows;
}

const dom = await domFonts();
const canvas = canvasFonts();

const parseWeight = (w) => Number(w);
const domRows = dom
  .map((r) => {
    const fam = firstFamily(r.family);
    const weight = parseWeight(r.weight);
    const face = FACES[fam] ? resolveFace(fam, weight) : null;
    return { family: fam, familyList: r.family, weight, face, count: r.count, examples: r.examples, texts: r.texts };
  })
  .sort((a, b) => a.family.localeCompare(b.family) || a.weight - b.weight);

const combos = new Map();
for (const r of domRows) {
  const k = `${r.family}|${r.weight}`;
  const e = combos.get(k) ?? { family: r.family, weight: r.weight, face: r.face, dom: 0, canvas: 0, examples: [] };
  e.dom += r.count;
  e.examples.push(...r.examples.slice(0, 2));
  combos.set(k, e);
}
for (const c of canvas) {
  const k = `${c.family}|${c.weight}`;
  const e = combos.get(k) ?? { family: c.family, weight: c.weight, face: resolveFace(c.family, c.weight), dom: 0, canvas: 0, examples: [] };
  e.canvas++;
  e.examples.push(`renderer.js:${c.line}`);
  combos.set(k, e);
}
const table = [...combos.values()].sort((a, b) => a.family.localeCompare(b.family) || a.weight - b.weight);

mkdirSync(resolve(here, '..', 'out'), { recursive: true });
writeFileSync(resolve(here, '..', 'out', 'font-usage.json'), JSON.stringify({ faces: FACES, dom: domRows, canvas, table }, null, 2));
console.log('DOM computed (family list | weight):');
for (const r of domRows) console.log(`  ${r.familyList} | ${r.weight} -> ${r.family}${r.face ? ` face ${r.face}` : ' (NOT a web font)'}  x${r.count}  ${r.examples.join(' ')}  texts=${JSON.stringify(r.texts)}`);
console.log('Canvas ctx.font:');
for (const c of canvas) console.log(`  renderer.js:${c.line} ${c.font} -> ${c.family} ${c.weight} -> face ${resolveFace(c.family, c.weight)}`);
console.log('Combinations:');
for (const c of table) console.log(`  ${c.family} ${c.weight} -> ${c.face ?? 'n/a'}  dom=${c.dom} canvas=${c.canvas}`);
