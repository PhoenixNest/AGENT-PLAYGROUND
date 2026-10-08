import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { WEB_DIR } from './emoji-scan.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const used = new Set();
for (const f of ['index.html', 'main.js']) {
  const text = readFileSync(join(WEB_DIR, f), 'utf8');
  for (const m of text.matchAll(/class(?:Name)?\s*=\s*\\?["']([^"'`$]*?)\\?["']/g)) {
    for (const c of m[1].split(/\s+/)) if (/^[a-z][\w-]*$/i.test(c)) used.add(c);
  }
}
const css = readFileSync(join(WEB_DIR, 'styles.css'), 'utf8');
const defined = new Set([...css.matchAll(/\.([a-zA-Z][\w-]*)/g)].map((m) => m[1]));
const tracked = [...used].filter((c) => defined.has(c)).sort();
writeFileSync(join(here, '..', 'tracked-classes.json'), `${JSON.stringify(tracked, null, 2)}\n`);
console.log(`tracked classes: ${tracked.length}`);
