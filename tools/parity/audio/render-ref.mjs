import { createServer } from "node:http";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync, existsSync } from "node:fs";
import { dirname, join, resolve, extname } from "node:path";
import { fileURLToPath } from "node:url";
import { chromium } from "playwright";
import { buildScenarios, SAMPLE_RATE } from "./cues.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const webRoot = resolve(here, "../../../games/maze-puzzle/web");
const outRef = join(here, "out", "ref");
const manifestPath = join(here, "ref-manifest.json");

const SELF_DIFF_LIMIT = 1e-6;
const MIME = { ".js": "text/javascript", ".mjs": "text/javascript", ".html": "text/html" };

function serve() {
  const server = createServer((req, res) => {
    const url = decodeURIComponent(req.url.split("?")[0]);
    let file = null;
    if (url.startsWith("/web/")) file = join(webRoot, url.slice(5));
    else if (url.startsWith("/audio/")) file = join(here, url.slice(7));
    if (!file || !existsSync(file)) {
      res.writeHead(404).end();
      return;
    }
    res.writeHead(200, { "content-type": MIME[extname(file)] ?? "application/octet-stream" });
    res.end(readFileSync(file));
  });
  return new Promise((ok) => server.listen(0, "127.0.0.1", () => ok(server)));
}

function writeWavFloat32(path, channels, sampleRate) {
  const n = channels[0].length;
  const numCh = channels.length;
  const dataBytes = n * numCh * 4;
  const buf = Buffer.alloc(44 + dataBytes);
  buf.write("RIFF", 0);
  buf.writeUInt32LE(36 + dataBytes, 4);
  buf.write("WAVE", 8);
  buf.write("fmt ", 12);
  buf.writeUInt32LE(16, 16);
  buf.writeUInt16LE(3, 20);
  buf.writeUInt16LE(numCh, 22);
  buf.writeUInt32LE(sampleRate, 24);
  buf.writeUInt32LE(sampleRate * numCh * 4, 28);
  buf.writeUInt16LE(numCh * 4, 32);
  buf.writeUInt16LE(32, 34);
  buf.write("data", 36);
  buf.writeUInt32LE(dataBytes, 40);
  let o = 44;
  for (let i = 0; i < n; i++) {
    for (let c = 0; c < numCh; c++) {
      buf.writeFloatLE(channels[c][i], o);
      o += 4;
    }
  }
  writeFileSync(path, buf);
}

function decode(b64) {
  const b = Buffer.from(b64, "base64");
  return new Float32Array(b.buffer, b.byteOffset, b.byteLength / 4);
}

function stats(channels) {
  let sum = 0;
  let peak = 0;
  let count = 0;
  for (const ch of channels) {
    for (let i = 0; i < ch.length; i++) {
      const v = ch[i];
      sum += v * v;
      const a = Math.abs(v);
      if (a > peak) peak = a;
      count++;
    }
  }
  return { rms: Math.sqrt(sum / count), peak };
}

function sha(channels) {
  const h = createHash("sha256");
  for (const ch of channels) h.update(Buffer.from(ch.buffer, ch.byteOffset, ch.byteLength));
  return h.digest("hex");
}

const only = process.argv.slice(2).filter((a) => !a.startsWith("--"));
const scenarios = buildScenarios().filter((s) => !s.rngOnly && (only.length === 0 || only.some((o) => s.id.includes(o))));

mkdirSync(outRef, { recursive: true });
const server = await serve();
const port = server.address().port;
const browser = await chromium.launch();
const page = await browser.newPage();
page.on("console", (m) => console.log(`[page] ${m.text()}`));
page.on("pageerror", (e) => console.log(`[page error] ${e.message}`));
await page.goto(`http://127.0.0.1:${port}/audio/render-page.html`);
await page.waitForFunction("window.__ready === true");

const withTimeout = (p, ms, what) =>
  Promise.race([p, new Promise((_, rej) => setTimeout(() => rej(new Error(`timeout: ${what}`)), ms))]);

const entries = [];
for (const sc of scenarios) {
  const payload = { ...sc };
  const first = await withTimeout(page.evaluate((s) => window.__render(s), payload), 60000, sc.id);
  const second = await withTimeout(page.evaluate((s) => window.__render(s), payload), 60000, sc.id);
  const a = first.channels.map(decode);
  const b = second.channels.map(decode);
  const hashA = sha(a);
  const hashB = sha(b);
  let selfDiff = 0;
  if (hashA !== hashB) {
    for (let c = 0; c < a.length; c++) {
      for (let i = 0; i < a[c].length; i++) selfDiff = Math.max(selfDiff, Math.abs(a[c][i] - b[c][i]));
    }
    if (selfDiff > SELF_DIFF_LIMIT) {
      throw new Error(`non-deterministic render for ${sc.id}: max |a-b| = ${selfDiff}`);
    }
  }
  const { rms, peak } = stats(a);
  writeWavFloat32(join(outRef, `${sc.id}.wav`), a, SAMPLE_RATE);
  entries.push({
    id: sc.id,
    cue: sc.cue,
    args: sc.args,
    calls: sc.calls,
    setup: sc.setup ?? [],
    suspends: sc.suspends ?? [],
    noInit: sc.noInit ?? false,
    seconds: sc.seconds,
    channels: a.length,
    sampleRate: SAMPLE_RATE,
    frames: first.frames,
    rngDraws: first.draws,
    rms: Number(rms.toPrecision(9)),
    peak: Number(peak.toPrecision(9)),
    sha256: hashA,
    bitExactRepeat: hashA === hashB,
    repeatMaxAbsDiff: selfDiff,
  });
  console.log(`${sc.id}  frames=${first.frames} rms=${rms.toExponential(3)} peak=${peak.toFixed(4)} draws=${first.draws}${selfDiff ? ` repeatDiff=${selfDiff.toExponential(2)}` : ""}`);
}

const version = browser.version();
await browser.close();
server.close();

if (only.length === 0) {
  writeFileSync(
    manifestPath,
    JSON.stringify({ chromium: version, sampleRate: SAMPLE_RATE, entries }, null, 1) + "\n",
  );
  console.log(`wrote ${manifestPath} (${entries.length} entries, chromium ${version})`);
}
