// Writes fixtures/jsmath-vectors.json: input/output pairs for every JsMath function as IEEE-754 hex bit patterns.
// Usage: node jsmath-vectors.mjs
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { hex } from "./lib/bits.mjs";
import { mulberry32 } from "./lib/mulberry32.mjs";
import { engineLabel, runInEngine } from "./lib/engine.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const outDir = process.env.JSMATH_OUT ? resolve(process.env.JSMATH_OUT) : resolve(here, "fixtures");
mkdirSync(outDir, { recursive: true });

const rnd = mulberry32(0xc0ffee);
const N = Number(process.env.JSMATH_VECTORS ?? 1500);

const bitView = new DataView(new ArrayBuffer(8));
function randomFiniteBits() {
  for (;;) {
    bitView.setUint32(0, Math.floor(rnd() * 4294967296));
    bitView.setUint32(4, Math.floor(rnd() * 4294967296));
    const d = bitView.getFloat64(0);
    if (Number.isFinite(d)) return d;
  }
}
const uni = (lo, hi) => lo + (hi - lo) * rnd();
const logUni = (loExp, hiExp) => (rnd() < 0.5 ? -1 : 1) * Math.pow(10, uni(loExp, hiExp));

const PI = Math.PI;
const trigSpecials = [
  0, -0, 1, -1, 0.5, 2, PI, -PI, PI / 2, -PI / 2, PI / 4, 3 * PI / 2, 2 * PI, -2 * PI, 100 * PI, 1e5 * PI,
  Math.pow(2, -26), Math.pow(2, -27), Math.pow(2, -28), 1e-300, 5e-324, -5e-324, 0.126, 0.125, 0.855469, 0.8554687,
  2.426265, 2.4262647, 105414350, 105414349.9, 105414351, 1e9, 1e15, 1e22, 1e300, Number.MAX_VALUE,
  Infinity, -Infinity, NaN, Number.MIN_VALUE, 0.7853981633974483, 1.5707963267948966, 3.141592653589793,
];

function trigInputs() {
  const v = trigSpecials.slice();
  for (let i = 0; i < N; i++) {
    switch (i % 7) {
      case 0: v.push(uni(-2 * PI, 2 * PI)); break;
      case 1: v.push(uni(-1, 1)); break;
      case 2: v.push(uni(-300, 300)); break;
      case 3: v.push(uni(-1e6, 1e6)); break;
      case 4: v.push(logUni(-30, 30)); break;
      case 5: v.push(uni(-0.13, 0.13)); break;
      default: v.push(randomFiniteBits());
    }
  }
  return v;
}

function atanInputs() {
  const v = [0, -0, 1, -1, 0.4375, 0.6875, 1.1875, 2.4375, 1e-30, 1e30, Math.pow(2, 66), Math.pow(2, -27), Infinity, -Infinity, NaN];
  for (let i = 0; i < N; i++) {
    switch (i % 5) {
      case 0: v.push(uni(-3, 3)); break;
      case 1: v.push(uni(-1000, 1000)); break;
      case 2: v.push(logUni(-20, 20)); break;
      case 3: v.push(uni(-0.5, 0.5)); break;
      default: v.push(randomFiniteBits());
    }
  }
  return v;
}

const pairSpecials = (() => {
  const s = [0, -0, 1, -1, 2, -2, 0.5, 1e-300, 1e300, Infinity, -Infinity, NaN, PI, 5e-324];
  const out = [];
  for (const a of s) for (const b of s) out.push([a, b]);
  return out;
})();

function atan2Inputs() {
  const v = pairSpecials.slice();
  for (let i = 0; i < N; i++) {
    switch (i % 5) {
      case 0: v.push([uni(-300, 300), uni(-300, 300)]); break;
      case 1: v.push([uni(-1, 1), uni(-1, 1)]); break;
      case 2: v.push([logUni(-20, 20), logUni(-20, 20)]); break;
      case 3: v.push([uni(-1e3, 1e3) * 1e-12, uni(-1e3, 1e3)]); break;
      default: v.push([randomFiniteBits(), randomFiniteBits()]);
    }
  }
  return v;
}

function powInputs() {
  const v = pairSpecials.slice();
  v.push([0.94, 0.96], [0.94, 0.016 * 60], [0.94, 0.05 * 60], [0.94, 0.05], [2, 10], [10, -3], [0, 0], [-8, 1 / 3], [-2, 3], [-2, 0.5], [1, Infinity], [-1, -Infinity], [0.5, Infinity], [2, Infinity], [Infinity, 0.5], [-0, 0.5], [3, 2], [-3, 2], [4, 0.5]);
  for (let i = 0; i < N; i++) {
    switch (i % 6) {
      case 0: v.push([0.94, uni(0, 3)]); break;
      case 1: v.push([uni(0, 2), uni(-5, 5)]); break;
      case 2: v.push([uni(-10, 10), Math.round(uni(-8, 8))]); break;
      case 3: v.push([Math.abs(logUni(-5, 5)), uni(-20, 20)]); break;
      case 4: v.push([0.94, (1 + Math.floor(rnd() * 5)) * 0.016 * 60 * rnd()]); break;
      default: v.push([randomFiniteBits(), uni(-3, 3)]);
    }
  }
  return v;
}

function hypotInputs() {
  const v = pairSpecials.slice();
  for (let i = 0; i < N; i++) {
    switch (i % 5) {
      case 0: v.push([uni(-400, 400), uni(-600, 600)]); break;
      case 1: v.push([uni(-1, 1), uni(-1, 1)]); break;
      case 2: v.push([logUni(-200, 200), logUni(-200, 200)]); break;
      case 3: v.push([uni(-30, 30), uni(-30, 30)]); break;
      default: v.push([randomFiniteBits(), randomFiniteBits()]);
    }
  }
  return v;
}

function genericInputs() {
  const v = [0, -0, 0.5, -0.5, 1.5, -1.5, 2.5, -2.5, 0.49999999999999994, -0.49999999999999994, 4503599627370495.5, 4503599627370497, 9007199254740991, 1e21, 1e300, -1e300, Infinity, -Infinity, NaN, 5e-324, -5e-324, 2147483647, 2147483648, -2147483648, -2147483649, 4294967295, 4294967296, 4294967297, 1e10, -1e10, 123456789012345680000];
  for (let i = 0; i < N; i++) {
    switch (i % 5) {
      case 0: v.push(uni(-10, 10)); break;
      case 1: v.push(Math.round(uni(-1e6, 1e6)) + (rnd() < 0.5 ? 0.5 : 0)); break;
      case 2: v.push(logUni(-5, 20)); break;
      case 3: v.push(uni(-5e9, 5e9)); break;
      default: v.push(randomFiniteBits());
    }
  }
  return v;
}

function numToStringInputs() {
  const v = [0, -0, 1, -1, 0.1, 0.2, 0.30000000000000004, 1e21, 1e-7, 1e-6, 123456789012345680000, 1.7976931348623157e308, 5e-324, -5e-324, 4294967296, 0.000001, 100, 1e20, 999999999999999900000, 0.5, 250, 1234.5678, NaN, Infinity, -Infinity, 2 ** 53, 2 ** 53 + 2, 1 / 3, 2 / 3, 1e22, 1.5e-10, 12345678.9];
  for (let i = 0; i < N; i++) {
    switch (i % 6) {
      case 0: v.push(Math.floor(uni(0, 1e6))); break;
      case 1: v.push(uni(-1000, 1000)); break;
      case 2: v.push(logUni(-30, 30)); break;
      case 3: v.push(Math.round(uni(-1e9, 1e9)) / 100); break;
      case 4: v.push(Math.floor(uni(0, 1e5)) * 0.016); break;
      default: v.push(randomFiniteBits());
    }
  }
  return v;
}

const g = genericInputs();
const gp = [];
for (let i = 0; i + 1 < g.length; i++) gp.push([g[i], g[i + 1]], [g[i], g[g.length - 1 - i]]);
for (const a of [0, -0, 1, -1, NaN, Infinity, -Infinity]) for (const b of [0, -0, 1, -1, NaN, Infinity, -Infinity]) gp.push([a, b]);

const spec = {
  unary: {
    sin: trigInputs(),
    cos: trigInputs(),
    atan: atanInputs(),
    sqrt: g.map((x) => Math.abs(x)).concat(g),
    floor: g,
    ceil: g,
    trunc: g,
    round: g,
    sign: g,
    abs: g,
    toInt32: g,
    toUint32: g,
  },
  binary: {
    atan2: atan2Inputs(),
    pow: powInputs(),
    hypot: hypotInputs(),
    min: gp,
    max: gp,
    mod: gp,
  },
};
for (const k of Object.keys(spec.unary)) spec.unary[k] = spec.unary[k].map(hex);
for (const k of Object.keys(spec.binary)) spec.binary[k] = spec.binary[k].map(([a, b]) => [hex(a), hex(b)]);

function computeVectors(s) {
  const dv = new DataView(new ArrayBuffer(8));
  const toD = (h) => {
    dv.setBigUint64(0, BigInt("0x" + h));
    return dv.getFloat64(0);
  };
  const toH = (d) => {
    if (Number.isNaN(d)) return "nan";
    dv.setFloat64(0, d);
    return dv.getBigUint64(0).toString(16).padStart(16, "0");
  };
  const U = {
    sin: Math.sin, cos: Math.cos, atan: Math.atan, sqrt: Math.sqrt, floor: Math.floor, ceil: Math.ceil,
    trunc: Math.trunc, round: Math.round, sign: Math.sign, abs: Math.abs,
    toInt32: (x) => x | 0, toUint32: (x) => x >>> 0,
  };
  const B = {
    atan2: Math.atan2, pow: Math.pow, hypot: Math.hypot, min: Math.min, max: Math.max,
    mod: (a, b) => a % b,
  };
  const out = { unary: {}, binary: {} };
  for (const k of Object.keys(s.unary)) out.unary[k] = s.unary[k].map((h) => toH(U[k](toD(h))));
  for (const k of Object.keys(s.binary)) out.binary[k] = s.binary[k].map(([a, b]) => toH(B[k](toD(a), toD(b))));
  return out;
}

const primary = process.env.JSMATH_ENGINE ?? "chromium";
const label = await engineLabel(primary);
const primaryOut = await runInEngine(primary, computeVectors, spec);
const nodeOut = await runInEngine("node", computeVectors, spec);

const engineDiff = {};
const unary = {};
const binary = {};
for (const k of Object.keys(spec.unary)) {
  unary[k] = spec.unary[k].map((h, i) => `${h}:${primaryOut.unary[k][i]}`);
  engineDiff[k] = primaryOut.unary[k].filter((o, i) => o !== nodeOut.unary[k][i]).length;
}
for (const k of Object.keys(spec.binary)) {
  binary[k] = spec.binary[k].map(([a, b], i) => `${a}:${b}:${primaryOut.binary[k][i]}`);
  engineDiff[k] = primaryOut.binary[k].filter((o, i) => o !== nodeOut.binary[k][i]).length;
}

const nodeSinCos = {
  sin: spec.unary.sin.map((h, i) => `${h}:${nodeOut.unary.sin[i]}`),
  cos: spec.unary.cos.map((h, i) => `${h}:${nodeOut.unary.cos[i]}`),
};

const imul = [];
for (let i = 0; i < N; i++) imul.push([Math.floor(rnd() * 4294967296) | 0, Math.floor(rnd() * 4294967296) | 0]);
imul.push([0x7fffffff, 0x7fffffff], [-2147483648, -1], [-2147483648, -2147483648], [0, -1], [65535, 65537]);
const imulOut = imul.map(([a, b]) => `${a}:${b}:${Math.imul(a, b)}`);

const numStr = numToStringInputs().map((x) => `${hex(x)}:${String(x)}`);

const doc = {
  generatedBy: "jsmath-vectors.mjs",
  engine: label,
  nodeReference: `node ${process.version} (v8 ${process.versions.v8})`,
  platform: `${process.platform}-${process.arch}`,
  engineDiffVsNode: engineDiff,
  unary,
  binary,
  nodeSinCos,
  imul: imulOut,
  numberToString: numStr,
};
const ref = mulberry32(0x5eed1234);
const refValues = [];
for (let i = 0; i < 4000; i++) refValues.push(hex(ref()));
writeFileSync(resolve(outDir, "rng-reference.json"), JSON.stringify({ seed: "0x5eed1234", values: refValues }));

const path = resolve(outDir, "jsmath-vectors.json");
writeFileSync(path, JSON.stringify(doc));
let total = 0;
for (const k of Object.keys(unary)) total += unary[k].length;
for (const k of Object.keys(binary)) total += binary[k].length;
console.log(`wrote ${path}: ${total + imulOut.length + numStr.length} vectors, engine ${label}`);
console.log("vectors differing between engine and node:", JSON.stringify(engineDiff));
