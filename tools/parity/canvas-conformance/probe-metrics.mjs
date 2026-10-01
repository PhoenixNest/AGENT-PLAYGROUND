// Dumps Chromium's canvas text metrics for the fonts used by the app: baseline offsets per textBaseline,
// em/font bounding boxes and advance widths. Output: out/metrics.json (consumed when calibrating Canvas2D.Text).
import fs from "node:fs";
import path from "node:path";
import { launch, newContext, installRoutes, ORIGIN, outDir } from "./lib/harness-lite.mjs";

const fonts = [
  "bold 13px Fredoka, sans-serif", "900 10.5px Fredoka, sans-serif", "bold 10px Fredoka, sans-serif", "bold 9.5px Fredoka, sans-serif",
  "900 16px Fredoka, sans-serif", "400 14px Fredoka, sans-serif", "11px Nunito, sans-serif", "800 14px Nunito, sans-serif", "bold 40px Fredoka, sans-serif",
];
const browser = await launch();
const context = await newContext(browser, { viewport: [200, 200], dpr: 3 });
await installRoutes(context);
const page = await context.newPage();
await page.goto(ORIGIN + "/__cc/replay.html");
const result = await page.evaluate(async (fonts) => {
  const c = document.createElement("canvas");
  c.width = c.height = 300;
  const ctx = c.getContext("2d");
  ctx.setTransform(3, 0, 0, 3, 0, 0);
  const out = {};
  for (const f of fonts) {
    const fam = /(Fredoka|Nunito)/.exec(f)[1];
    await document.fonts.load(f.replace(/, sans-serif$/, ""), "Hx");
    const rows = {};
    ctx.font = f.replace(/, sans-serif$/, ', "Noto Sans Symbols 2", "Noto Sans Symbols", sans-serif');
    for (const b of ["alphabetic", "top", "hanging", "middle", "ideographic", "bottom"]) {
      ctx.textBaseline = b;
      const m = ctx.measureText("Hxg");
      rows[b] = {
        alphabeticBaseline: m.alphabeticBaseline, hangingBaseline: m.hangingBaseline, ideographicBaseline: m.ideographicBaseline,
        emHeightAscent: m.emHeightAscent, emHeightDescent: m.emHeightDescent,
        fontBoundingBoxAscent: m.fontBoundingBoxAscent, fontBoundingBoxDescent: m.fontBoundingBoxDescent,
        actualAscent: m.actualBoundingBoxAscent, actualDescent: m.actualBoundingBoxDescent,
      };
    }
    ctx.textBaseline = "alphabetic";
    rows.widths = {};
    for (const s of ["Hxg", "SORT RIG 0123456789 ×•", "To Wa", "AVATAR", "fi fl", "ORBITAL x2 +3 ✓ ▶ ←"]) rows.widths[s] = ctx.measureText(s).width;
    out[f] = rows;
  }
  return out;
}, fonts);
fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(path.join(outDir, "metrics.json"), JSON.stringify(result, null, 1));
for (const [f, r] of Object.entries(result)) {
  console.log(f);
  for (const b of ["alphabetic", "top", "middle", "bottom", "hanging"]) console.log("  " + b.padEnd(10), JSON.stringify(r[b], (k, v) => (typeof v === "number" ? +v.toFixed(3) : v)));
  console.log("  widths", JSON.stringify(r.widths));
}
await browser.close();
