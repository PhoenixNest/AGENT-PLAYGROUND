// Writes out/ref/manifest.local.json for a set of reference WAVs that was NOT produced by render-ref.mjs on this
// machine (for example copied from another worktree). Usage: node hash-ref.mjs [refDir]
// The hashes only detect later corruption of the files; staleness is checked against ref-manifest.json statistics.
import { createHash } from "node:crypto";
import { readdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const refDir = resolve(process.argv[2] ?? join(here, "out", "ref"));
const files = {};
for (const name of readdirSync(refDir).filter((n) => n.endsWith(".wav")).sort()) {
  files[name] = { sha256: createHash("sha256").update(readFileSync(join(refDir, name))).digest("hex") };
}
writeFileSync(join(refDir, "manifest.local.json"), JSON.stringify({ chromium: "unknown (hashed in place)", sampleRate: 44100, files }, null, 1) + "\n");
console.log(`wrote ${join(refDir, "manifest.local.json")} (${Object.keys(files).length} files)`);
