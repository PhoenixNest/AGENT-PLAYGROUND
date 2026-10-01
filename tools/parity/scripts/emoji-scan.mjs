import { readFileSync, readdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
export const WEB_DIR = join(here, '..', '..', '..', 'games', 'maze-puzzle', 'web');

const PICTO = /\p{Extended_Pictographic}/u;
const EMOJI_PRES = /\p{Emoji_Presentation}/u;

export function webSourceFiles() {
  const files = ['index.html', 'main.js', 'styles.css'].map((f) => join(WEB_DIR, f));
  for (const f of readdirSync(join(WEB_DIR, 'engine')).sort()) {
    if (f.endsWith('.js')) files.push(join(WEB_DIR, 'engine', f));
  }
  return files;
}

export function isEmojiGrapheme(g) {
  if (!PICTO.test(g)) return false;
  if (g.includes('\uFE0F')) return true;
  const base = String.fromCodePoint(g.codePointAt(0));
  return EMOJI_PRES.test(base);
}

export function scanGraphemes() {
  const seg = new Intl.Segmenter('en', { granularity: 'grapheme' });
  const emoji = new Map();
  const other = new Map();
  for (const file of webSourceFiles()) {
    const text = readFileSync(file, 'utf8');
    for (const { segment } of seg.segment(text)) {
      if (/^[\x00-\x7f]*$/.test(segment)) continue;
      const bucket = isEmojiGrapheme(segment) ? emoji : other;
      bucket.set(segment, (bucket.get(segment) ?? 0) + 1);
    }
  }
  return { emoji, other };
}

export function codepointsKey(g) {
  return [...g]
    .map((c) => c.codePointAt(0))
    .filter((cp) => cp !== 0xfe0f)
    .map((cp) => cp.toString(16))
    .join('-');
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const { emoji, other } = scanGraphemes();
  console.log(`emoji graphemes: ${emoji.size}`);
  for (const [g, n] of [...emoji].sort((a, b) => a[0].localeCompare(b[0]))) {
    console.log(`${g}\t${codepointsKey(g)}\t${[...g].map((c) => c.codePointAt(0).toString(16)).join(' ')}\tx${n}`);
  }
  console.log(`\nnon-emoji non-ASCII graphemes: ${other.size}`);
  for (const [g, n] of other) {
    console.log(`${g}\tU+${[...g].map((c) => c.codePointAt(0).toString(16).toUpperCase().padStart(4, '0')).join(' U+')}\tx${n}`);
  }
}
