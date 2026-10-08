import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { isAbsolute, join, resolve } from "node:path";
import { ROOT, formatTrace, launchBrowser, runSequence } from "./harness.mjs";

const opts = {
  sequences: "sequences.json",
  out: "out/web",
  concurrency: 4,
  profile: "zero",
};
const argv = process.argv.slice(2);
for (let i = 0; i < argv.length; i++) {
  const a = argv[i];
  if (a === "--sequences") opts.sequences = argv[++i];
  else if (a === "--only") opts.only = argv[++i].split(",").filter(Boolean);
  else if (a === "--out") opts.out = argv[++i];
  else if (a === "--profile") opts.profile = argv[++i];
  else if (a === "--concurrency") opts.concurrency = Number(argv[++i]);
  else {
    console.error(`unknown argument ${a}`);
    process.exit(2);
  }
}
const abs = (p) => (isAbsolute(p) ? p : resolve(ROOT, p));
const sequences = JSON.parse(readFileSync(abs(opts.sequences), "utf8"));
const outDir = join(abs(opts.out), "traces");
mkdirSync(outDir, { recursive: true });

const jobs = sequences.filter((s) => !opts.only || opts.only.includes(s.id));
const browser = await launchBrowser();
const failures = [];
let next = 0;
await Promise.all(
  Array.from({ length: Math.min(opts.concurrency, jobs.length) }, async () => {
    while (next < jobs.length) {
      const seq = jobs[next++];
      const t = Date.now();
      try {
        const r = await runSequence(browser, seq, opts.profile);
        for (const want of seq.expect ?? []) {
          const hit = r.events.some(
            (e) =>
              e.type === want.type &&
              e.name === want.name &&
              (!want.args ||
                JSON.stringify(e.args) === JSON.stringify(want.args)),
          );
          if (!hit)
            throw new Error(`expected event missing: ${JSON.stringify(want)}`);
        }
        writeFileSync(join(outDir, `${seq.id}.trace.json`), formatTrace(r));
        const byType = {};
        for (const e of r.events) byType[e.type] = (byType[e.type] ?? 0) + 1;
        console.log(
          `ok   ${seq.id} (${Date.now() - t} ms) ${r.events.length} events ${JSON.stringify(byType)} end t=${r.meta.perf}`,
        );
      } catch (e) {
        failures.push(seq.id);
        console.error(`FAIL ${seq.id}: ${e.message}`);
      }
    }
  }),
);
await browser.close();
process.exit(failures.length ? 1 : 0);
