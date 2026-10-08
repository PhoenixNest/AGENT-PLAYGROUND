// Minimal self-contained Chromium setup for the canvas conformance tools (does not depend on tools/parity/web).
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { chromium } from "playwright";
import { webRoot } from "../../lib/roots.mjs";

const here = path.dirname(fileURLToPath(import.meta.url));
export const confDir = path.resolve(here, "..");
export const parityDir = path.resolve(confDir, "..");
export const webDir = webRoot(here);
export const assetsDir = path.join(parityDir, "assets");
export const outDir = path.join(confDir, "out");
export const ORIGIN = "http://parity.test";

export const CHROMIUM_ARGS = ["--force-color-profile=srgb", "--font-render-hinting=none", "--disable-lcd-text"];

const MIME = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json",
  ".png": "image/png",
  ".ttf": "font/ttf",
};

export function fontCss() {
  const face = (family, weight, file) =>
    `@font-face{font-family:${family};font-weight:${weight};src:url(${ORIGIN}/__parity/fonts/${file}) format('truetype');font-display:block}`;
  return [
    face("Fredoka", 400, "Fredoka-400.ttf"),
    face("Fredoka", 600, "Fredoka-600.ttf"),
    face("Fredoka", 700, "Fredoka-700.ttf"),
    face("Nunito", 700, "Nunito-700.ttf"),
    face("Nunito", 800, "Nunito-800.ttf"),
    face("Nunito", 900, "Nunito-900.ttf"),
    face('"Noto Sans Symbols 2"', "100 900", "NotoSansSymbols2-Regular.ttf"),
    face('"Noto Sans Symbols"', "100 900", "NotoSansSymbols-Regular.ttf"),
    "html{font-synthesis:none}",
  ].join("\n");
}

export const INIT_SCRIPT = `
(() => {
  function mulberry32(a) {
    return function () {
      a |= 0; a = (a + 0x6d2b79f5) | 0;
      let t = Math.imul(a ^ (a >>> 15), 1 | a);
      t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
      return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
  }
  Math.random = mulberry32(0x5eed1234);
  const rawNow = performance.now.bind(performance);
  const perfOffset = rawNow();
  performance.now = () => rawNow() - perfOffset;
  const st = window.setTimeout.bind(window);
  const ct = window.clearTimeout.bind(window);
  let rafId = 0;
  const timers = new Map();
  window.requestAnimationFrame = (cb) => {
    const id = ++rafId;
    const delay = 16 - (performance.now() % 16);
    timers.set(id, st(() => { timers.delete(id); cb(performance.now()); }, delay));
    return id;
  };
  window.cancelAnimationFrame = (id) => { ct(timers.get(id)); timers.delete(id); };
})();
`;

export async function launch() {
  return chromium.launch({ args: CHROMIUM_ARGS });
}

export async function newContext(browser, { viewport = [430, 932], dpr = 3 } = {}) {
  const context = await browser.newContext({
    viewport: { width: viewport[0], height: viewport[1] },
    deviceScaleFactor: dpr,
    locale: "en-US",
    timezoneId: "America/Los_Angeles",
  });
  return context;
}

export async function installRoutes(context, { servePath = (p) => null } = {}) {
  await context.route("**/*", async (route) => {
    const url = new URL(route.request().url());
    if (url.hostname === "fonts.googleapis.com") {
      return route.fulfill({ status: 200, contentType: "text/css", body: fontCss() });
    }
    if (url.origin !== ORIGIN) return route.abort();
    const p = decodeURIComponent(url.pathname);
    let file = null;
    if (p.startsWith("/__parity/fonts/")) file = path.join(assetsDir, "fonts", p.slice("/__parity/fonts/".length));
    else if (p.startsWith("/__parity/emoji/")) file = path.join(assetsDir, "emoji", p.slice("/__parity/emoji/".length));
    else if (p === "/__parity/fonts.css") return route.fulfill({ status: 200, contentType: "text/css", body: fontCss() });
    else if (p.startsWith("/__cc/")) file = path.join(confDir, p.slice("/__cc/".length));
    else file = path.join(webDir, p === "/" ? "index.html" : p.slice(1));
    const custom = servePath(p);
    if (custom) file = custom;
    if (!file || !fs.existsSync(file) || !fs.statSync(file).isFile()) return route.fulfill({ status: 404, body: "not found" });
    return route.fulfill({ status: 200, contentType: MIME[path.extname(file)] || "application/octet-stream", body: fs.readFileSync(file) });
  });
}

export const emojiIndex = () => JSON.parse(fs.readFileSync(path.join(assetsDir, "emoji", "index.json"), "utf8"));

export const FONT_LOADS = [
  "400 16px Fredoka", "700 16px Fredoka", "700 16px Nunito", "800 16px Nunito", "900 16px Nunito",
  "16px \"Noto Sans Symbols 2\"", "16px \"Noto Sans Symbols\"",
];
