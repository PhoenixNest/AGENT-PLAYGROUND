import { createHash } from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { codepointsKey, scanGraphemes } from './emoji-scan.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const ASSETS = join(here, '..', 'assets');

const GOOGLE_FONTS = { repo: 'google/fonts', commit: '9710da1eacb3be272583c3224dcb70f9da6eadbb' };
const FLUENT = { repo: 'microsoft/fluentui-emoji', commit: '1ffb34c752ecf5d402f04cfb4b392c77f57c54bc' };

const FONT_SOURCES = [
  {
    family: 'Fredoka',
    dir: 'ofl/fredoka',
    file: 'Fredoka[wdth,wght].ttf',
    license: 'Fredoka-OFL.txt',
    instances: [400, 600, 700, 800, 900].map((w) => ({
      out: `Fredoka-${w}.ttf`,
      axes: [`wght=${w}`, 'wdth=100'],
      ...(w > 700 ? { note: 'source wght axis max is 700; instancer clamps to 700 outlines' } : {}),
    })),
  },
  {
    family: 'Nunito',
    dir: 'ofl/nunito',
    file: 'Nunito[wght].ttf',
    license: 'Nunito-OFL.txt',
    instances: [400, 700, 800, 900].map((w) => ({ out: `Nunito-${w}.ttf`, axes: [`wght=${w}`] })),
  },
  {
    family: 'Noto Sans Symbols 2',
    dir: 'ofl/notosanssymbols2',
    file: 'NotoSansSymbols2-Regular.ttf',
    license: 'NotoSansSymbols2-OFL.txt',
    instances: [],
    staticAs: 'NotoSansSymbols2-Regular.ttf',
  },
  {
    family: 'Noto Sans Symbols',
    dir: 'ofl/notosanssymbols',
    file: 'NotoSansSymbols[wght].ttf',
    license: 'NotoSansSymbols-OFL.txt',
    instances: [{ out: 'NotoSansSymbols-Regular.ttf', axes: ['wght=400'] }],
  },
];

const manifest = [];
const sha256 = (buf) => createHash('sha256').update(buf).digest('hex');
const rawUrl = (src, path) =>
  `https://raw.githubusercontent.com/${src.repo}/${src.commit}/${path.split('/').map(encodeURIComponent).join('/')}`;

async function get(url, as = 'buffer') {
  let lastErr;
  for (let attempt = 0; attempt < 5; attempt++) {
    try {
      const headers = { 'User-Agent': 'parity-fetch-assets' };
      if (process.env.GITHUB_TOKEN && url.startsWith('https://api.github.com')) {
        headers.Authorization = `Bearer ${process.env.GITHUB_TOKEN}`;
      }
      const res = await fetch(url, { headers });
      if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
      return as === 'json' ? await res.json() : Buffer.from(await res.arrayBuffer());
    } catch (err) {
      lastErr = err;
      await new Promise((r) => setTimeout(r, 500 * (attempt + 1)));
    }
  }
  throw new Error(`GET ${url} failed: ${lastErr.message}`);
}

function save(rel, buf, meta) {
  const abs = join(ASSETS, rel);
  mkdirSync(dirname(abs), { recursive: true });
  writeFileSync(abs, buf);
  manifest.push({ path: rel, bytes: buf.length, sha256: sha256(buf), ...meta });
}

async function pool(items, limit, fn) {
  const out = new Array(items.length);
  let next = 0;
  await Promise.all(
    Array.from({ length: limit }, async () => {
      while (next < items.length) {
        const i = next++;
        out[i] = await fn(items[i], i);
      }
    }),
  );
  return out;
}

function runPython(args) {
  return execFileSync('python', args, {
    encoding: 'utf8',
    stdio: ['ignore', 'pipe', 'pipe'],
    env: { ...process.env, SOURCE_DATE_EPOCH: '1767225600' },
  }).trim();
}

async function fetchFonts() {
  const fontToolsVersion = runPython(['-c', 'import fontTools;print(fontTools.version)']);
  for (const src of FONT_SOURCES) {
    const path = `${src.dir}/${src.file}`;
    const buf = await get(rawUrl(GOOGLE_FONTS, path));
    const srcRel = `fonts/src/${src.file.replace(/[\[\],]/g, (c) => ({ '[': '_', ']': '', ',': '-' })[c])}`;
    if (!src.staticAs) save(srcRel, buf, { kind: 'download', url: rawUrl(GOOGLE_FONTS, path), repo: GOOGLE_FONTS.repo, commit: GOOGLE_FONTS.commit });
    const lic = await get(rawUrl(GOOGLE_FONTS, `${src.dir}/OFL.txt`));
    save(`fonts/${src.license}`, lic, { kind: 'download', url: rawUrl(GOOGLE_FONTS, `${src.dir}/OFL.txt`), repo: GOOGLE_FONTS.repo, commit: GOOGLE_FONTS.commit });
    if (src.staticAs) {
      save(`fonts/${src.staticAs}`, buf, { kind: 'download', url: rawUrl(GOOGLE_FONTS, path), repo: GOOGLE_FONTS.repo, commit: GOOGLE_FONTS.commit });
    }
    for (const inst of src.instances) {
      const outAbs = join(ASSETS, 'fonts', inst.out);
      runPython(['-m', 'fontTools.varLib.instancer', join(ASSETS, srcRel), ...inst.axes, '-o', outAbs, '--update-name-table']);
      const out = readFileSync(outAbs);
      manifest.push({
        path: `fonts/${inst.out}`,
        bytes: out.length,
        sha256: sha256(out),
        kind: 'generated',
        generator: `fontTools ${fontToolsVersion} varLib.instancer ${inst.axes.join(' ')} --update-name-table`,
        from: srcRel,
        ...(inst.note ? { note: inst.note } : {}),
      });
    }
  }
}

function chooseFluentPng(treePaths, folder) {
  const prefix = `assets/${folder}/`;
  const pngs = treePaths.filter((p) => p.startsWith(prefix) && p.endsWith('.png'));
  const d3 = pngs.filter((p) => /\/3D\/[^/]+$/.test(p) && !/\/(Light|Medium|Dark|Medium-Light|Medium-Dark)\//.test(p));
  const pick = d3.find((p) => p.startsWith(`${prefix}3D/`)) ?? d3.find((p) => p.startsWith(`${prefix}Default/3D/`));
  if (pick) return { path: pick, style: '3D' };
  const color = pngs.find((p) => /\/Color\/[^/]+$/.test(p));
  return color ? { path: color, style: 'Color' } : null;
}

async function fetchEmoji() {
  const { emoji } = scanGraphemes();
  const graphemes = [...emoji.keys()].sort((a, b) => codepointsKey(a).localeCompare(codepointsKey(b), 'en', { numeric: false }));

  const tree = await get(`https://api.github.com/repos/${FLUENT.repo}/git/trees/${FLUENT.commit}?recursive=1`, 'json');
  if (tree.truncated) throw new Error('fluentui-emoji tree truncated');
  const treePaths = tree.tree.filter((n) => n.type === 'blob').map((n) => n.path);
  const folders = treePaths.filter((p) => /^assets\/[^/]+\/metadata\.json$/.test(p)).map((p) => p.split('/')[1]);

  const byUnicode = new Map();
  await pool(folders, 24, async (folder) => {
    const meta = JSON.parse((await get(rawUrl(FLUENT, `assets/${folder}/metadata.json`))).toString('utf8'));
    if (!meta.unicode) return;
    const key = meta.unicode.toLowerCase().split(/\s+/).filter((cp) => cp !== 'fe0f').join('-');
    if (!byUnicode.has(key)) byUnicode.set(key, folder);
  });

  const index = {};
  const missing = [];
  const fallbacks = [];
  await pool(graphemes, 8, async (g) => {
    const key = codepointsKey(g);
    const folder = byUnicode.get(key);
    const pick = folder ? chooseFluentPng(treePaths, folder) : null;
    if (!pick) {
      missing.push(g);
      return;
    }
    if (pick.style !== '3D') fallbacks.push(g);
    const url = rawUrl(FLUENT, pick.path);
    const buf = await get(url);
    const file = `${key}.png`;
    save(`emoji/${file}`, buf, { kind: 'download', url, repo: FLUENT.repo, commit: FLUENT.commit });
    index[g] = { file, folder, style: pick.style };
  });

  const ordered = {};
  for (const g of graphemes) if (index[g]) ordered[g] = index[g];
  const indexBuf = Buffer.from(`${JSON.stringify(ordered, null, 2)}
`, 'utf8');
  save('emoji/index.json', indexBuf, { kind: 'generated', generator: 'fetch-assets.mjs' });

  const license = await get(rawUrl(FLUENT, 'LICENSE'));
  save('emoji/Fluent-LICENSE.txt', license, { kind: 'download', url: rawUrl(FLUENT, 'LICENSE'), repo: FLUENT.repo, commit: FLUENT.commit });

  console.log(`emoji graphemes: ${graphemes.length}, mapped: ${Object.keys(ordered).length}`);
  console.log(`missing: ${missing.length ? missing.join(' ') : 'none'}`);
  console.log(`color fallbacks: ${fallbacks.length ? fallbacks.join(' ') : 'none'}`);
  return { missing, fallbacks };
}

await fetchFonts();
await fetchEmoji();

manifest.sort((a, b) => a.path.localeCompare(b.path));
const doc = {
  sources: { 'google/fonts': GOOGLE_FONTS, 'microsoft/fluentui-emoji': FLUENT },
  files: manifest,
};
writeFileSync(join(ASSETS, 'manifest.json'), `${JSON.stringify(doc, null, 2)}
`);
console.log(`manifest: ${manifest.length} files`);
