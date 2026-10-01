// Renders every stream/scene JSON in a directory (or a single file) in Chromium at DPR 3 -> PNG.
//   node replay.mjs <dir|file> [--out out/web] [--check-ref out/ref]
import fs from "node:fs";
import path from "node:path";
import {
  launch, newContext, installRoutes, confDir, outDir, ORIGIN,
} from "./lib/harness-lite.mjs";

const args = process.argv.slice(2);
const opt = (name, def) => {
  const i = args.indexOf("--" + name);
  return i >= 0 ? args[i + 1] : def;
};
const target = path.resolve(args[0] && !args[0].startsWith("--") ? args[0] : path.join(confDir, "scenes"));
const outPng = path.resolve(opt("out", path.join(outDir, "web")));
const checkRef = opt("check-ref", null);
fs.mkdirSync(outPng, { recursive: true });

const files = fs.statSync(target).isDirectory()
  ? fs.readdirSync(target).filter((f) => f.endsWith(".json") && f !== "index.json").map((f) => path.join(target, f))
  : [target];

const browser = await launch();
const contexts = new Map();
let bad = 0;
for (const file of files) {
  const stream = JSON.parse(fs.readFileSync(file, "utf8"));
  const vp = [stream.meta.width, stream.meta.height];
  const key = vp.join("x");
  if (!contexts.has(key)) {
    const context = await newContext(browser, { viewport: vp, dpr: stream.meta.dpr ?? 3 });
    await installRoutes(context);
    contexts.set(key, context);
  }
  const page = await contexts.get(key).newPage();
  page.on("pageerror", (e) => console.error("[pageerror]", e.message));
  await page.goto(ORIGIN + "/__cc/replay.html");
  const dataUrl = await page.evaluate((s) => window.__replay(s), stream);
  const buf = Buffer.from(dataUrl.split(",")[1], "base64");
  const name = path.basename(file, ".json");
  fs.writeFileSync(path.join(outPng, name + ".png"), buf);
  if (checkRef) {
    const ref = path.join(path.resolve(checkRef), name + ".png");
    if (fs.existsSync(ref)) {
      const same = Buffer.compare(fs.readFileSync(ref), buf) === 0;
      if (!same) bad++;
      console.log(name, same ? "identical to recorded reference" : "DIFFERS from recorded reference");
    }
  } else console.log(name);
  await page.close();
}
await browser.close();
if (checkRef && bad) process.exitCode = 1;
