import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { isAbsolute, join, resolve } from "node:path";
import {
  ROOT,
  formatTrace,
  launchBrowser,
  runState,
  validateStates,
  LAUNCH_ARGS,
} from "./harness.mjs";

function parseArgs(argv) {
  const opts = { states: "states.json", out: "out/web", concurrency: 4 };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--states") opts.states = argv[++i];
    else if (a === "--only") opts.only = argv[++i].split(",").filter(Boolean);
    else if (a === "--profile") opts.profile = argv[++i];
    else if (a === "--out") opts.out = argv[++i];
    else if (a === "--concurrency") opts.concurrency = Number(argv[++i]);
    else {
      console.error(`unknown argument ${a}`);
      process.exit(2);
    }
  }
  if (opts.profile && !["zero", "iphone"].includes(opts.profile)) {
    console.error("--profile must be zero or iphone");
    process.exit(2);
  }
  return opts;
}

const abs = (p) => (isAbsolute(p) ? p : resolve(ROOT, p));

async function pool(items, size, worker) {
  let next = 0;
  const lanes = Array.from(
    { length: Math.min(size, items.length) },
    async () => {
      while (next < items.length) {
        const item = items[next++];
        await worker(item);
      }
    },
  );
  await Promise.all(lanes);
}

const opts = parseArgs(process.argv.slice(2));
const states = JSON.parse(readFileSync(abs(opts.states), "utf8"));
validateStates(states);
const outDir = abs(opts.out);
mkdirSync(join(outDir, "traces"), { recursive: true });

const unknown = (opts.only ?? []).filter(
  (id) => !states.some((s) => s.id === id),
);
if (unknown.length) {
  console.error(`unknown state ids: ${unknown.join(", ")}`);
  process.exit(2);
}

const jobs = [];
for (const s of states) {
  if (opts.only && !opts.only.includes(s.id)) continue;
  for (const profile of s.profiles) {
    if (opts.profile && opts.profile !== profile) continue;
    jobs.push({ state: s, profile, traceOwner: profile === s.profiles[0] });
  }
}

const started = Date.now();
const browser = await launchBrowser();
const meta = {
  chromium: browser.version(),
  launchArgs: LAUNCH_ARGS,
  animationClock: "registry+sweep (SPEC 4.5)",
  states: {},
};
const failures = [];

await pool(jobs, opts.concurrency, async ({ state, profile, traceOwner }) => {
  const t = Date.now();
  try {
    const r = await runState(browser, state, profile);
    const base = join(outDir, `${state.id}__${profile}`);
    writeFileSync(`${base}.png`, r.png);
    writeFileSync(`${base}.layout.json`, JSON.stringify(r.layout, null, 1));
    if (traceOwner) {
      writeFileSync(
        join(outDir, "traces", `${state.id}.trace.json`),
        formatTrace(r.trace),
      );
    }
    meta.states[`${state.id}__${profile}`] = { ...r.meta, ms: Date.now() - t };
    console.log(`ok   ${state.id}__${profile} (${Date.now() - t} ms)`);
  } catch (e) {
    failures.push({
      id: `${state.id}__${profile}`,
      error: String(e.stack ?? e),
    });
    console.error(`FAIL ${state.id}__${profile}: ${e.message}`);
  }
});

await browser.close();
meta.totalMs = Date.now() - started;
meta.failures = failures;
writeFileSync(join(outDir, "_meta.json"), JSON.stringify(meta, null, 1));
console.log(
  `${jobs.length - failures.length}/${jobs.length} captured in ${((Date.now() - started) / 1000).toFixed(1)} s`,
);
process.exit(failures.length ? 1 : 0);
