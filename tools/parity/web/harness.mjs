import { chromium } from "playwright";
import { createHash } from "node:crypto";
import { existsSync, readFileSync } from "node:fs";
import { dirname, extname, join, normalize, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { pngToRgbPng } from "./png.mjs";

const here = dirname(fileURLToPath(import.meta.url));
export const ROOT = join(here, "..");
export const ASSETS_DIR = join(ROOT, "assets");
export const WEB_DIR = join(ROOT, "..", "..", "games", "maze-puzzle", "web");
export const ORIGIN = "http://parity.test";
export const T0 = new Date("2026-10-01T09:00:00-07:00");
export const VIEWPORT = { width: 430, height: 932 };
export const SCALE = 3;
export const PROFILES = {
  zero: { top: 0, right: 0, bottom: 0, left: 0 },
  iphone: { top: 59, right: 0, bottom: 34, left: 0 },
};
export const LAUNCH_ARGS = [
  "--force-color-profile=srgb",
  "--font-render-hinting=none",
  "--disable-lcd-text",
];
export const CALL_ALLOWLIST = new Set([
  "showPage",
  "showToast",
  "updateAllCreditDisplays",
  "updateShopDockBadge",
  "updateDailyDockBadge",
  "updateHomeView",
  "renderSectorsPage",
  "renderDailyPage",
  "renderStorePage",
  "renderAchievementsPage",
  "syncSettingsUI",
  "restartSector",
  "nextSector",
  "handleLevelVictory",
  "handleLevelFailure",
  "exitGameToHome",
  "resetGameplaySessionUi",
  "updateQuotaDisplay",
  "dismissGestureHint",
  "initLayout",
  "triggerHaptic",
]);
export const PAGES = [
  "loading",
  "home",
  "game",
  "maps",
  "daily",
  "shop",
  "achievements",
  "settings",
];
export const FONT_TABLE = [
  ["Fredoka", 400, 400],
  ["Fredoka", 700, 700],
  ["Fredoka", 800, 700],
  ["Fredoka", 900, 700],
  ["Nunito", 400, 700],
  ["Nunito", 700, 700],
  ["Nunito", 800, 800],
  ["Nunito", 900, 900],
];
const FONT_FACES = [
  ["Fredoka", 400, "Fredoka-400.ttf"],
  ["Fredoka", 600, "Fredoka-600.ttf"],
  ["Fredoka", 700, "Fredoka-700.ttf"],
  ["Nunito", 700, "Nunito-700.ttf"],
  ["Nunito", 800, "Nunito-800.ttf"],
  ["Nunito", 900, "Nunito-900.ttf"],
];
const SYMBOL_FACES = [
  ["Noto Sans Symbols 2", "NotoSansSymbols2-Regular.ttf"],
  ["Noto Sans Symbols", "NotoSansSymbols-Regular.ttf"],
];
const MIME = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".mjs": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json",
  ".png": "image/png",
  ".ttf": "font/ttf",
  ".svg": "image/svg+xml",
};
const GOOGLE_FONTS_HOST = "fonts.googleapis.com";

const SELECTOR_FORMS = [
  [
    /^\.([A-Za-z0-9_-]+)\[data-([A-Za-z0-9_-]+)=["']?([^"'\]]+)["']?\]$/,
    (m) => `${m[1]}--${m[3].replace(/[^A-Za-z0-9_-]/g, "-")}`,
  ],
  [/^\.([A-Za-z0-9_-]+):nth-child\((\d+)\)$/, (m) => `${m[1]}--${m[2]}`],
  [/^#([A-Za-z0-9_-]+)$/, (m) => m[1]],
];

export function unityNameForSelector(selector) {
  for (const [re, fn] of SELECTOR_FORMS) {
    const m = re.exec(selector);
    if (m) return fn(m);
  }
  throw new Error(`selector outside SPEC 8.1 forms: ${selector}`);
}

const sha256 = (buf) => createHash("sha256").update(buf).digest("hex");

let assetCheck = null;
export function verifyAssets() {
  if (assetCheck) return assetCheck;
  const manifest = JSON.parse(
    readFileSync(join(ASSETS_DIR, "manifest.json"), "utf8"),
  );
  const bad = [];
  for (const f of manifest.files) {
    const p = join(ASSETS_DIR, f.path);
    if (!existsSync(p) || sha256(readFileSync(p)) !== f.sha256)
      bad.push(f.path);
  }
  if (bad.length) throw new Error(`asset manifest mismatch: ${bad.join(", ")}`);
  assetCheck = { files: manifest.files.length };
  return assetCheck;
}

const fileCache = new Map();
function readCached(path) {
  let body = fileCache.get(path);
  if (!body) {
    body = readFileSync(path);
    fileCache.set(path, body);
  }
  return body;
}

function safeJoin(base, rel) {
  const p = normalize(join(base, rel));
  return p.startsWith(base + sep) ? p : null;
}

export function fontFaceCss() {
  const rules = [];
  for (const [family, weight, file] of FONT_FACES) {
    rules.push(
      `@font-face{font-family:'${family}';font-style:normal;font-weight:${weight};font-display:block;src:url(${ORIGIN}/__parity/fonts/${file}) format('truetype')}`,
    );
  }
  for (const [family, file] of SYMBOL_FACES) {
    rules.push(
      `@font-face{font-family:'${family}';font-style:normal;font-weight:100 900;font-display:block;src:url(${ORIGIN}/__parity/fonts/${file}) format('truetype')}`,
    );
  }
  return rules.join("\n");
}

export function overridesCss(profile, useFallback) {
  const css = [
    ':root{--font-bubble:"Fredoka","Noto Sans Symbols 2","Noto Sans Symbols",sans-serif;--font-body:"Nunito","Noto Sans Symbols 2","Noto Sans Symbols",sans-serif}',
    "html{font-synthesis:none}",
    "img.pe{width:1em;height:1em;vertical-align:-0.125em;display:inline-block;margin:0;padding:0;border:0}",
  ];
  if (useFallback) {
    const T = PROFILES[profile].top;
    const B = PROFILES[profile].bottom;
    css.push(
      `.hud-top-mobile{top:calc(${T}px + 14px)}`,
      `.page-header-bar{padding:calc(${T}px + 14px) 16px 12px}`,
      `.home-top-bar{padding:calc(${T}px + 14px) 16px 6px}`,
      `.home-arcade-dock{height:calc(68px + ${B}px);padding:4px 10px calc(${B}px + 6px)}`,
    );
  }
  return css.join("\n");
}

let emojiList = null;
function emojiEntries() {
  if (!emojiList) {
    const index = JSON.parse(
      readFileSync(join(ASSETS_DIR, "emoji", "index.json"), "utf8"),
    );
    emojiList = Object.entries(index).map(([g, v]) => [
      [...g]
        .map((c) => c.codePointAt(0))
        .filter((c) => c !== 0xfe0f)
        .map((c) => c.toString(16))
        .join("-"),
      v.file,
    ]);
  }
  return emojiList;
}

export function loadTrackedClasses() {
  return JSON.parse(readFileSync(join(ROOT, "tracked-classes.json"), "utf8"));
}

export async function launchBrowser() {
  verifyAssets();
  return chromium.launch({ args: LAUNCH_ARGS });
}

const initSource = readFileSync(join(here, "page-init.js"), "utf8");

function resolveRoute(url) {
  const u = new URL(url);
  if (u.hostname === GOOGLE_FONTS_HOST) {
    return {
      status: 200,
      contentType: "text/css; charset=utf-8",
      body: Buffer.from(fontFaceCss()),
    };
  }
  if (u.origin !== ORIGIN) return null;
  const path = decodeURIComponent(u.pathname);
  if (path.startsWith("/__parity/fonts/")) {
    const f = safeJoin(
      join(ASSETS_DIR, "fonts"),
      path.slice("/__parity/fonts/".length),
    );
    if (f && existsSync(f))
      return {
        status: 200,
        contentType: MIME[extname(f)],
        body: readCached(f),
      };
    return { status: 404, contentType: "text/plain", body: Buffer.alloc(0) };
  }
  if (path.startsWith("/__parity/emoji/")) {
    const f = safeJoin(
      join(ASSETS_DIR, "emoji"),
      path.slice("/__parity/emoji/".length),
    );
    if (f && existsSync(f))
      return {
        status: 200,
        contentType: MIME[extname(f)],
        body: readCached(f),
      };
    return { status: 404, contentType: "text/plain", body: Buffer.alloc(0) };
  }
  const rel = path === "/" ? "index.html" : path.slice(1);
  const f = safeJoin(WEB_DIR, rel);
  if (f && existsSync(f)) {
    return {
      status: 200,
      contentType: MIME[extname(f)] ?? "application/octet-stream",
      body: readCached(f),
    };
  }
  return { status: 404, contentType: "text/plain", body: Buffer.alloc(0) };
}

const CHUNK_MS = 4000;

export class Session {
  constructor(browser, { profile, stateId = "adhoc" }) {
    if (!PROFILES[profile]) throw new Error(`unknown profile ${profile}`);
    this.browser = browser;
    this.profile = profile;
    this.stateId = stateId;
    this.errors = [];
    this.blocked = [];
    this.held = false;
    this.booted = false;
    this.safeArea = "cdp";
    this.opLog = [];
  }

  fail(msg) {
    throw new Error(`[${this.stateId}/${this.profile}] ${msg}`);
  }

  async boot({ completeLoading = true } = {}) {
    const insets = PROFILES[this.profile];
    this.context = await this.browser.newContext({
      viewport: VIEWPORT,
      deviceScaleFactor: SCALE,
      timezoneId: "America/Los_Angeles",
      locale: "en-US",
      serviceWorkers: "block",
    });
    this.context.setDefaultTimeout(120000);
    const page = (this.page = await this.context.newPage());
    page.on("pageerror", (e) => this.errors.push(`pageerror: ${e.message}`));
    page.on("console", (m) => {
      if (m.type() === "error" && /Simulation loop error/.test(m.text())) {
        this.errors.push(`console: ${m.text()} ${m.args().length}`);
      }
    });

    try {
      const cdp = await this.context.newCDPSession(page);
      await cdp.send("Emulation.setSafeAreaInsetsOverride", { insets });
    } catch {
      this.safeArea = "css-fallback";
    }

    // install() leaves the fake clock running in real time until pauseAt(); installing 60 s early keeps pauseAt() in the future on a loaded machine (a 1 s margin failed with "Cannot fast-forward to the past").
    await page.clock.install({ time: new Date(T0.getTime() - 60000) });
    await page.clock.pauseAt(new Date(T0.getTime() + 1000));

    await page.route("**/*", (route) => {
      const hit = resolveRoute(route.request().url());
      if (!hit) {
        this.blocked.push(route.request().url());
        return route.abort();
      }
      return route.fulfill(hit);
    });

    const cfg = {
      emoji: emojiEntries(),
      overridesCss: overridesCss(
        this.profile,
        this.safeArea === "css-fallback",
      ),
    };
    await page.addInitScript({
      content: `window.__PARITY_CONFIG=${JSON.stringify(cfg)};\n${initSource}`,
    });
    await page.goto(`${ORIGIN}/index.html`, { waitUntil: "load" });

    await this.verifyBoot();
    await page.evaluate(() => window.__parity.sweep());

    if (completeLoading) {
      let steps = 0;
      while ((await page.evaluate(() => window.app.currentPage)) !== "home") {
        if (++steps > 2000) this.fail("boot: home not reached in 2000 frames");
        await page.clock.runFor(16);
      }
      this.bootFrames = steps;
      await this.advance(240);
    }
    this.booted = true;
    this.checkErrors();
  }

  async verifyBoot() {
    const page = this.page;
    const fonts = await page.evaluate(
      (table) => window.__parity.verifyFonts(table),
      FONT_TABLE,
    );
    const notLoaded = fonts.report.filter((f) => f.status !== "loaded");
    if (notLoaded.length)
      this.fail(`fonts not loaded: ${JSON.stringify(notLoaded)}`);
    if (fonts.report.length !== FONT_FACES.length + SYMBOL_FACES.length) {
      this.fail(`unexpected font face count ${fonts.report.length}`);
    }
    const wrong = fonts.effective.filter((e) => !e.ok);
    if (wrong.length)
      this.fail(`effective face mismatch: ${JSON.stringify(wrong)}`);
    if (fonts.sheetLast !== "parity-overrides")
      this.fail(`last stylesheet is ${fonts.sheetLast}`);
    await page.evaluate(() => window.__parity.waitEmoji());
    const state = await page.evaluate(() => window.__parity.assertBooted());
    if (state.lastTime !== 0)
      this.fail(`app.lastTime is ${state.lastTime}, expected 0`);
    if (state.perf !== 0)
      this.fail(`performance.now() is ${state.perf} at load, expected 0`);
    this.perfOffset = await page.evaluate(() => window.__parity.perfOffset);
    const insets = PROFILES[this.profile];
    const pad = await page.evaluate(() => {
      const el = document.querySelector(".home-top-bar");
      const dock = document.querySelector(".home-arcade-dock");
      return {
        top: getComputedStyle(el).paddingTop,
        dock: getComputedStyle(dock).height,
      };
    });
    if (
      pad.top !== `${insets.top + 14}px` ||
      pad.dock !== `${68 + insets.bottom}px`
    ) {
      this.fail(
        `safe-area (${this.safeArea}) not applied: ${JSON.stringify(pad)}`,
      );
    }
  }

  checkErrors() {
    if (this.blocked.length)
      this.fail(`blocked network requests: ${this.blocked.join(", ")}`);
    if (this.errors.length) this.fail(this.errors.join(" | "));
  }

  async now() {
    return this.page.evaluate(() => performance.now());
  }

  async endOp({ sweep = false } = {}) {
    await this.page.evaluate(
      (s) => (s ? window.__parity.sweep() : window.__parity.diff()),
      sweep,
    );
  }

  async advance(ms) {
    if (!Number.isInteger(ms) || ms < 0) this.fail(`advance: invalid ms ${ms}`);
    let left = ms;
    while (left > 0) {
      const n = Math.min(left, CHUNK_MS);
      await this.page.clock.runFor(n);
      left -= n;
    }
    await this.endOp({ sweep: true });
    this.checkErrors();
  }

  async locate({ target, selector, fx = 0.5, fy = 0.5 }) {
    let sel = selector;
    if (!sel) sel = `#${target}`;
    unityNameForSelector(sel);
    const hit = await this.page.evaluate(
      ({ sel, fx, fy }) => {
        let el = document.querySelector(sel);
        if (!el) return { error: "missing" };
        if (el instanceof HTMLInputElement && el.type === "checkbox")
          el = el.closest("label") || el;
        const r = el.getBoundingClientRect();
        if (
          !el.checkVisibility({ visibilityProperty: true }) ||
          r.width <= 0 ||
          r.height <= 0
        ) {
          return { error: "not visible" };
        }
        const x = r.x + r.width * fx;
        const y = r.y + r.height * fy;
        const top = document.elementFromPoint(x, y);
        if (!top || !(el === top || el.contains(top) || top.contains(el))) {
          return {
            error: `obscured by ${top ? top.tagName + "#" + top.id + "." + top.className : "nothing"}`,
          };
        }
        return { x, y };
      },
      { sel, fx, fy },
    );
    if (hit.error) this.fail(`${sel}: ${hit.error}`);
    return hit;
  }

  async click(step) {
    const { x, y } = await this.locate(step);
    const m = this.page.mouse;
    await m.move(x, y);
    await m.down();
    await m.up();
    // Chromium re-evaluates :hover under a resting cursor from a real-time timer after layout changes, which the fake clock cannot order; parking the pointer outside the viewport removes hover entirely (Unity has none).
    await m.move(-1, -1);
  }

  async runStep(step) {
    const page = this.page;
    switch (step.op) {
      case "boot":
        return this.boot({ completeLoading: step.completeLoading !== false });
      case "showPage":
        if (!PAGES.includes(step.page))
          this.fail(`showPage: bad page ${step.page}`);
        await page.evaluate((p) => window.app.showPage(p), step.page);
        break;
      case "setProfile":
        await page.evaluate(
          ({ path, value }) => {
            const keys = path.split(".");
            let o = window.app.economy.profile;
            for (const k of keys.slice(0, -1)) o = o[k];
            o[keys[keys.length - 1]] = value;
          },
          { path: step.path, value: step.value },
        );
        break;
      case "setApp":
        await page.evaluate(({ field, value }) => (window.app[field] = value), {
          field: step.field,
          value: step.value,
        });
        break;
      case "call":
        if (!CALL_ALLOWLIST.has(step.method))
          this.fail(`call: ${step.method} not in allow-list`);
        await page.evaluate(({ method, args }) => window.app[method](...args), {
          method: step.method,
          args: step.args ?? [],
        });
        break;
      case "click":
      case "clickSelector":
        await this.click(step);
        break;
      case "pointer": {
        const m = page.mouse;
        await m.move(step.x, step.y);
        if (step.kind === "down") await m.down();
        else if (step.kind === "up") await m.up();
        else if (step.kind !== "move")
          this.fail(`pointer: bad kind ${step.kind}`);
        break;
      }
      case "key":
        await page.keyboard.press(step.key);
        break;
      case "advance":
        return this.advance(step.ms);
      case "loadSector":
        await page.evaluate((i) => {
          window.app.loadSector(window.app.sectors[i], false);
          window.app.currentSectorIndex = i;
        }, step.index);
        break;
      case "scroll": {
        const ok = await page.evaluate(
          ({ target, y }) => {
            const el = target
              ? document.getElementById(target)
              : document.querySelector(
                  ".app-page.page-active .page-scroll-body",
                );
            if (!el) return false;
            el.scrollTop = y;
            return true;
          },
          { target: step.target, y: step.y },
        );
        if (!ok) this.fail("scroll: target missing");
        break;
      }
      case "holdPressed": {
        const { x, y } = await this.locate(step);
        const m = page.mouse;
        await m.move(x, y);
        await m.down();
        this.held = true;
        await this.advance(150);
        return;
      }
      case "capture":
        return this.capture();
      default:
        this.fail(`unknown op ${step.op}`);
    }
    await this.endOp();
    this.checkErrors();
  }

  async capture() {
    const page = this.page;
    await page.evaluate(() => window.__parity.waitEmoji());
    await page.evaluate(() => window.__parity.sweep());
    const tracked = loadTrackedClasses();
    const elements = await page.evaluate(
      (t) => window.__parity.layout(t),
      tracked,
    );
    const events = await this.traceEvents();
    const raw = await page.screenshot({ animations: "allow", caret: "hide" });
    this.checkErrors();
    const { png, width, height } = pngToRgbPng(raw);
    if (
      width !== VIEWPORT.width * SCALE ||
      height !== VIEWPORT.height * SCALE
    ) {
      this.fail(`screenshot is ${width}x${height}`);
    }
    return {
      png,
      layout: {
        state: this.stateId,
        profile: this.profile,
        viewport: { w: VIEWPORT.width, h: VIEWPORT.height, scale: SCALE },
        elements,
      },
      trace: { sequence: this.stateId, events },
      meta: await this.meta(),
    };
  }

  async traceEvents() {
    const events = await this.page.evaluate(() => window.__parity.events);
    for (const e of events) {
      if (!Number.isInteger(e.t))
        this.fail(`non-integer trace time ${e.t} (${e.type}/${e.name})`);
    }
    return events;
  }

  async meta() {
    const m = await this.page.evaluate(() => ({
      perf: performance.now(),
      random: window.__parity.randomCalls(),
      animations: window.__parity.animationCount(),
      errors: window.__parity.errors,
    }));
    if (m.errors.length) this.fail(`page errors: ${m.errors.join(" | ")}`);
    return {
      perfOffset: this.perfOffset,
      bootFrames: this.bootFrames,
      safeArea: this.safeArea,
      ...m,
    };
  }

  async releaseHeld() {
    if (this.held) {
      this.held = false;
      await this.page.mouse.up();
    }
  }

  async close() {
    try {
      await this.context?.close();
    } catch {}
  }
}

export function validateStates(states) {
  const ids = new Set();
  for (const s of states) {
    if (!/^[a-z0-9_]+$/.test(s.id)) throw new Error(`bad state id ${s.id}`);
    if (ids.has(s.id)) throw new Error(`duplicate state id ${s.id}`);
    ids.add(s.id);
    if (!s.steps?.length || s.steps[0].op !== "boot")
      throw new Error(`${s.id}: first step must be boot`);
    if (
      s.steps.filter((x) => x.op === "capture").length !== 1 ||
      s.steps.at(-1).op !== "capture"
    ) {
      throw new Error(`${s.id}: exactly one capture, last`);
    }
    for (const p of s.profiles)
      if (!PROFILES[p]) throw new Error(`${s.id}: bad profile ${p}`);
  }
}

export async function runState(browser, state, profile) {
  const session = new Session(browser, { profile, stateId: state.id });
  try {
    let result = null;
    for (const step of state.steps) {
      const r = await session.runStep(step);
      if (step.op === "capture") result = r;
    }
    await session.releaseHeld();
    return result;
  } finally {
    await session.close();
  }
}

export function formatTrace(trace) {
  const body = trace.events.map((e) => `  ${JSON.stringify(e)}`).join(",\n");
  return `{"sequence":${JSON.stringify(trace.sequence)},"events":[\n${body}\n]}\n`;
}

export async function runSequence(browser, seq, profile = "zero") {
  const session = new Session(browser, { profile, stateId: seq.id });
  try {
    for (const step of seq.steps) await session.runStep(step);
    await session.releaseHeld();
    const events = await session.traceEvents();
    session.checkErrors();
    return { sequence: seq.id, events, meta: await session.meta() };
  } finally {
    await session.close();
  }
}
