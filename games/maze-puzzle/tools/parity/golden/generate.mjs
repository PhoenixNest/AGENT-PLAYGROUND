// Usage: node tools/parity/golden/generate.mjs [--engine chromium|node] [--out <dir>] [--only maze,economy,physics] [--filter <substr>] [--full]
// Without --full only every 100th frame snapshot is stored (plus a hash of every frame); --full stores all 601 snapshots per run.
// Golden fixtures are produced by the unmodified web engine modules under mulberry32(0x5EED1234).
// Chromium (Playwright, timezone America/Los_Angeles) is authoritative; Node is a cross-check only
// because V8 in Node uses a different libm for sin/cos/atan2 than Chromium 153.
import { createServer } from "node:http";
import { readFileSync, existsSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { gzipSync } from "node:zlib";
import { dirname, extname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const webRoot = resolve(here, "../../../games/maze-puzzle/web");
const browserDir = resolve(here, "browser");

const args = process.argv.slice(2);
const opt = (name, def) => {
  const i = args.indexOf("--" + name);
  return i >= 0 ? args[i + 1] : def;
};
const engine = opt("engine", "chromium");
const outDir = resolve(opt("out", join(here, "fixtures")));
const only = new Set(opt("only", "maze,economy,physics").split(","));
const filter = opt("filter", "");
const full = args.includes("--full");
const SAMPLE_EVERY = 100;

const LEVELS = [1, 11, 21, 31, 38, 41, 50];
const RUNS = [
  { program: "idle", audio: false, resync: false },
  { program: "drag", audio: false, resync: false },
  { program: "bursts", audio: false, resync: false },
  { program: "drag", audio: true, resync: true },
  { program: "bursts", audio: true, resync: true },
];
const specs = [];
for (const level of LEVELS) {
  for (const r of RUNS) {
    specs.push({
      name: `L${String(level).padStart(2, "0")}-${r.program}-${r.audio ? "audio" : "silent"}`,
      level,
      program: r.program,
      audio: r.audio,
      resync: r.resync,
      frames: 600,
    });
  }
}
const selected = specs.filter((s) => s.name.includes(filter));

function mime(p) {
  return { ".js": "text/javascript", ".mjs": "text/javascript", ".html": "text/html", ".css": "text/css" }[extname(p)] ?? "application/octet-stream";
}

function startServer() {
  const server = createServer((req, res) => {
    const url = decodeURIComponent(req.url.split("?")[0]);
    const file = url.startsWith("/__golden/") ? join(browserDir, url.slice(10)) : join(webRoot, url);
    if (!file.startsWith(webRoot) && !file.startsWith(browserDir)) {
      res.writeHead(403).end();
      return;
    }
    if (!existsSync(file)) {
      res.writeHead(404).end();
      return;
    }
    res.writeHead(200, { "content-type": mime(file) });
    res.end(readFileSync(file));
  });
  return new Promise((ok) => server.listen(0, "127.0.0.1", () => ok(server)));
}

async function runChromium() {
  const { chromium } = await import("playwright");
  const server = await startServer();
  const base = `http://127.0.0.1:${server.address().port}`;
  const browser = await chromium.launch({ args: ["--force-color-profile=srgb"] });
  try {
    const context = await browser.newContext({ timezoneId: "America/Los_Angeles", locale: "en-US" });
    const page = await context.newPage();
    page.on("pageerror", (e) => console.error("page error", e));
    await page.goto(base + "/__golden/blank.html");
    const label = `chromium ${browser.version()}`;
    const load = `async () => {
      const economy = await import("/engine/economy.js");
      const physics = await import("/engine/physics.js");
      const audio = await import("/engine/audio.js");
      const maze = await import("/engine/maze.js");
      const collect = await import("/__golden/collect.mjs");
      const mods = { ...economy, PhysicsEngine: physics.PhysicsEngine, AudioEngine: audio.AudioEngine, MazeLevelBuilder: maze.MazeLevelBuilder };
      return { collect, mods };
    }`;
    const stat = only.has("maze") || only.has("economy")
      ? await page.evaluate(`(${load})().then(({collect, mods}) => collect.collectStatic(mods))`)
      : null;
    const phys = [];
    if (only.has("physics")) {
      for (const spec of selected) {
        const r = await page.evaluate(`(${load})().then(({collect, mods}) => collect.collectPhysics(mods, ${JSON.stringify(spec)}))`);
        phys.push(r);
        console.log(`physics ${spec.name}: peak debris ${r.header.peakDebris}, log ${r.header.log.length}`);
      }
    }
    return { label, stat, phys };
  } finally {
    await browser.close();
    server.close();
  }
}

async function runNode() {
  const economy = await import(pathToFileURL(join(webRoot, "engine/economy.js")).href);
  const physics = await import(pathToFileURL(join(webRoot, "engine/physics.js")).href);
  const audio = await import(pathToFileURL(join(webRoot, "engine/audio.js")).href);
  const maze = await import(pathToFileURL(join(webRoot, "engine/maze.js")).href);
  const collect = await import(pathToFileURL(join(browserDir, "collect.mjs")).href);
  const mods = { ...economy, PhysicsEngine: physics.PhysicsEngine, AudioEngine: audio.AudioEngine, MazeLevelBuilder: maze.MazeLevelBuilder };
  const stat = only.has("maze") || only.has("economy") ? collect.collectStatic(mods) : null;
  const phys = [];
  if (only.has("physics")) {
    for (const spec of selected) phys.push(collect.collectPhysics(mods, spec));
  }
  return { label: `node ${process.version} (v8 ${process.versions.v8})`, stat, phys };
}

const result = engine === "node" ? await runNode() : await runChromium();

mkdirSync(join(outDir, "physics"), { recursive: true });
const sizes = {};
const write = (rel, data) => {
  writeFileSync(join(outDir, rel), data);
  sizes[rel] = data.length;
};

if (result.stat) {
  if (only.has("maze")) write("maze.json", JSON.stringify(result.stat.maze));
  if (only.has("economy")) {
    write("economy.json", JSON.stringify({ sequences: result.stat.economy, catalog: result.stat.catalog }));
  }
}
const runNames = [];
for (const r of result.phys) {
  const name = r.header.name;
  runNames.push(name);
  const raw = Buffer.from(r.framesB64, "base64");
  const lengths = r.header.frameLengths;
  const stored = [];
  const chunks = [];
  let off = 0;
  for (let i = 0; i < lengths.length; i++) {
    const bytes = lengths[i] * 8;
    if (full || i % SAMPLE_EVERY === 0 || i === lengths.length - 1) {
      stored.push(i);
      chunks.push(raw.subarray(off, off + bytes));
    }
    off += bytes;
  }
  r.header.frameStored = stored;
  write(`physics/${name}.json.gz`, gzipSync(Buffer.from(JSON.stringify(r.header)), { level: 9 }));
  write(`physics/${name}.frames.bin.gz`, gzipSync(Buffer.concat(chunks), { level: 9 }));
}
if (only.has("physics") && !filter) {
  write("physics/manifest.json", JSON.stringify({ engine: result.label, runs: runNames }));
}
write("engine.txt", Buffer.from(result.label + "\n"));

let total = 0;
for (const v of Object.values(sizes)) total += v;
console.log(`engine: ${result.label}`);
console.log(`wrote ${Object.keys(sizes).length} files, ${(total / 1048576).toFixed(2)} MiB to ${outDir}`);
