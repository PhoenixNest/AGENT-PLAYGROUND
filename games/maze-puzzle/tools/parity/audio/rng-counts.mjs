import { writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { buildScenarios, mulberry32, SEED, SAMPLE_RATE } from "./cues.mjs";

import { webRoot } from "../lib/roots.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const audioJs = join(webRoot(here), "engine", "audio.js");

const r6 = (x) => (typeof x === "number" ? Math.round(x * 1e6) / 1e6 : x);

let trace = [];
let clock = 0;
let nextId = 1;

function log(name, args) {
  trace.push([name, ...args.map(r6)]);
}

class StubParam {
  constructor(nodeId, name, initial) {
    this.nodeId = nodeId;
    this.name = name;
    this.value = initial;
  }
  setValueAtTime(v, t) {
    log("param.setValueAtTime", [this.nodeId, this.name, v, t]);
    this.value = v;
    return this;
  }
  linearRampToValueAtTime(v, t) {
    log("param.linearRampToValueAtTime", [this.nodeId, this.name, v, t]);
    return this;
  }
  exponentialRampToValueAtTime(v, t) {
    log("param.exponentialRampToValueAtTime", [this.nodeId, this.name, v, t]);
    return this;
  }
  setTargetAtTime(v, t, c) {
    log("param.setTargetAtTime", [this.nodeId, this.name, v, t, c]);
    return this;
  }
  cancelScheduledValues(t) {
    log("param.cancelScheduledValues", [this.nodeId, this.name, t]);
    return this;
  }
}

class StubNode {
  constructor(kind, params) {
    this.id = nextId++;
    this.kind = kind;
    log("node.create", [this.id, kind]);
    for (const [name, initial] of Object.entries(params)) {
      this[name] = new StubParam(this.id, name, initial);
    }
  }
  connect(target) {
    log("node.connect", [this.id, target.id]);
    return target;
  }
  disconnect() {}
  start(when) {
    log("node.start", [this.id, when]);
  }
  stop(when) {
    log("node.stop", [this.id, when]);
  }
}

function typed(node, prop) {
  let value;
  Object.defineProperty(node, prop, {
    get: () => value,
    set: (v) => {
      value = v;
      log("node.set", [node.id, prop, String(v)]);
    },
  });
  return node;
}

class ParityAudioContext {
  constructor() {
    nextId = 1;
    this.sampleRate = SAMPLE_RATE;
    this.state = "suspended";
    this.destination = { id: 0 };
    log("ctx.create", [SAMPLE_RATE]);
  }
  get currentTime() {
    return clock;
  }
  resume() {
    this.state = "running";
    log("ctx.resume", []);
    return Promise.resolve();
  }
  createOscillator() {
    return typed(new StubNode("oscillator", { frequency: 440 }), "type");
  }
  createGain() {
    return new StubNode("gain", { gain: 1 });
  }
  createBiquadFilter() {
    return typed(new StubNode("biquad", { frequency: 350, Q: 1 }), "type");
  }
  createBufferSource() {
    const n = new StubNode("bufferSource", {});
    let buffer = null;
    Object.defineProperty(n, "buffer", {
      get: () => buffer,
      set: (b) => {
        buffer = b;
        log("node.set", [n.id, "buffer", { length: b.length, sampleRate: b.sampleRate, channels: b.numberOfChannels }]);
      },
    });
    return n;
  }
  createDynamicsCompressor() {
    return new StubNode("compressor", { threshold: -24, knee: 30, ratio: 12, attack: 0.003, release: 0.25 });
  }
  createBuffer(channels, length, sampleRate) {
    const data = Array.from({ length: channels }, () => new Float32Array(length));
    return {
      numberOfChannels: channels,
      length,
      sampleRate,
      duration: length / sampleRate,
      getChannelData: (c) => data[c],
    };
  }
}

globalThis.window = globalThis;
globalThis.AudioContext = ParityAudioContext;

const { AudioEngine } = await import(pathToFileURL(audioJs).href);

let draws = 0;
const rnd = mulberry32(SEED);
Math.random = () => {
  draws++;
  return rnd();
};

const results = {};
const traces = {};

for (const sc of buildScenarios()) {
  draws = 0;
  trace = [];
  const seeded = mulberry32(SEED);
  Math.random = () => {
    draws++;
    return seeded();
  };
  const audio = new AudioEngine();
  const run = (c) => {
    const args = c.a.map((x) => (x === "@now" ? clock : x));
    trace.push(["call", c.m, ...args.map(r6)]);
    audio[c.m](...args);
  };

  if (!sc.noInit) {
    clock = 0;
    trace.push(["call", "publicInit"]);
    audio.publicInit();
  }
  clock = 0.01;
  for (const s of sc.setup ?? []) run(s);
  for (const c of [...sc.calls].sort((a, b) => a.t - b.t)) {
    clock = c.t;
    run(c);
  }
  for (const s of sc.suspends ?? []) {
    clock = s.at;
    for (const c of s.calls) run(c);
  }

  results[sc.id] = {
    draws,
    calls: sc.calls.length + (sc.suspends ?? []).reduce((n, s) => n + s.calls.length, 0),
    scenario: sc,
  };
  traces[sc.id] = trace;
}

writeFileSync(join(here, "rng-counts.json"), JSON.stringify({ seed: SEED, scenarios: results }, null, 1) + "\n");
writeFileSync(join(here, "trace-golden.json"), JSON.stringify({ seed: SEED, traces }) + "\n");
console.log(`scenarios: ${Object.keys(results).length}`);
for (const [id, r] of Object.entries(results)) console.log(`${id}\tdraws=${r.draws}`);
