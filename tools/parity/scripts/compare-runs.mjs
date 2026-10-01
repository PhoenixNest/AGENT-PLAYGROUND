import { existsSync, readdirSync, readFileSync } from "node:fs";
import { join } from "node:path";

const [a, b, mode = "determinism"] = process.argv.slice(2);
const SUFFIXES = [".png", ".layout.json"];
const same = (x, y) => readFileSync(x).equals(readFileSync(y));
const ids = (dir) =>
  new Set(
    readdirSync(dir)
      .filter((f) => f.endsWith(".png"))
      .map((f) => f.slice(0, -4)),
  );

const A = ids(a);
const B = ids(b);
const changed = [];
const onlyA = [];
const onlyB = [];
for (const id of A) {
  if (!B.has(id)) {
    onlyA.push(id);
    continue;
  }
  const diff = SUFFIXES.filter((s) => !same(join(a, id + s), join(b, id + s)));
  const traceA = join(a, "traces", id.split("__")[0] + ".trace.json");
  const traceB = join(b, "traces", id.split("__")[0] + ".trace.json");
  if (existsSync(traceA) && existsSync(traceB) && !same(traceA, traceB))
    diff.push(".trace.json");
  if (diff.length) changed.push(`${id} (${diff.join(",")})`);
}
for (const id of B) if (!A.has(id)) onlyB.push(id);

console.log(`${mode}: ${A.size} vs ${B.size} captures`);
console.log(`differing: ${changed.length}`);
for (const c of changed) console.log(`  ${c}`);
console.log(`only in ${a}: ${onlyA.length}`);
for (const c of onlyA) console.log(`  ${c}`);
console.log(`only in ${b}: ${onlyB.length}`);
for (const c of onlyB) console.log(`  ${c}`);
process.exit(mode === "determinism" && changed.length ? 1 : 0);
