// Runs unmodified web engine modules under a seeded Math.random. Executed inside Chromium
// (authoritative) or Node (cross-check). Everything it returns is JSON-serialisable and
// every double that matters is either hex bits or a base64 Float64Array.

const dvh = new DataView(new ArrayBuffer(8));

export function hex(d) {
  dvh.setFloat64(0, d);
  return dvh.getBigUint64(0).toString(16).padStart(16, "0");
}

export function makeRng(seed) {
  let a = seed | 0;
  const fn = function () {
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    fn.calls++;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
  fn.calls = 0;
  fn.getState = () => a >>> 0;
  fn.setState = (s) => {
    a = s | 0;
  };
  return fn;
}

export function toBase64(f64) {
  const bytes = new Uint8Array(f64.buffer, f64.byteOffset, f64.byteLength);
  let s = "";
  for (let i = 0; i < bytes.length; i += 0x8000) s += String.fromCharCode.apply(null, bytes.subarray(i, i + 0x8000));
  return btoa(s);
}

export function frameHash(v) {
  const u = new Uint32Array(v.buffer, v.byteOffset, v.length * 2);
  let h1 = 0x811c9dc5 | 0;
  let h2 = 0x01000193 ^ 0x5bd1e995;
  for (let i = 0; i < u.length; i++) {
    h1 = Math.imul(h1 ^ u[i], 16777619);
    h2 = Math.imul(h2 ^ u[i], 0x85ebca6b) ^ (h2 >>> 13);
  }
  return (h1 >>> 0).toString(16).padStart(8, "0") + (h2 >>> 0).toString(16).padStart(8, "0");
}

const TYPE_CODE = { titanium: 0, solar: 1, volatile: 2, magnetic: 3 };
const GATE_CODE = { MULTIPLY: 0, ADD: 1, BOOST: 2 };
const STATE_CODE = { IN_RING: 0, FALLING: 1, INGESTING: 2, REMOVED: 3 };

export function captureStatic(e) {
  const v = [
    e.center.x, e.center.y, e.ringRadius, e.corridorWidth, e.innerRadius, e.apertureAngle,
    e.concentricRings ? 1 : 0, e.gearRatio, e.trackRadius, e.blackHole.x, e.blackHole.y,
    e.gates.length, e.dividers.length, e.oneWayFlaps.length, e.deflectorPins.length, e.trackWalls.length,
  ];
  for (const g of e.gates) v.push(g.rMin, g.rMax, g.angle, g.isInner ? 1 : 0, g.value, GATE_CODE[g.type] ?? 3);
  for (const d of e.dividers) v.push(d.rMin, d.rMax, d.angle, d.isInner ? 1 : 0);
  for (const f of e.oneWayFlaps) v.push(f.rMin, f.rMax, f.angle, f.maxSwing, f.normalSign, f.isInner ? 1 : 0);
  for (const p of e.deflectorPins) v.push(p.r, p.angle, p.radius, p.restitution, p.isInner ? 1 : 0);
  for (const t of e.trackWalls) v.push(t.r, t.gapAngle, t.gapSize);
  return Float64Array.from(v);
}

export function capture(e) {
  const v = [
    e.ringAngle, e.angularVelocity, e.innerRingAngle, e.rcsBoostTimer, e.lastRatchetAngle,
    e.rcsThrusting ? 1 : 0, e.rcsDirection, e.nextDebrisId, e.debrisList.length, e.oneWayFlaps.length,
    e.deflectorPins.length, e.gates.length,
  ];
  for (const d of e.debrisList) {
    v.push(d.id, d.x, d.y, d.vx, d.vy, d.radius, d.mass, d.weight, STATE_CODE[d.state], d.ingestProgress,
      d.gateCooldown, TYPE_CODE[d.type] ?? 4);
  }
  for (const f of e.oneWayFlaps) v.push(f.swing, f.swingVel);
  for (const p of e.deflectorPins) v.push(p.hitTimer);
  for (const g of e.gates) {
    const ids = [...g.processedDebris].sort((a, b) => a - b);
    v.push(ids.length, ...ids);
  }
  return Float64Array.from(v);
}

class StubParam {
  constructor() {
    this.value = 0;
  }
  setValueAtTime(v) {
    this.value = v;
    return this;
  }
  linearRampToValueAtTime() {
    return this;
  }
  exponentialRampToValueAtTime() {
    return this;
  }
  setTargetAtTime() {
    return this;
  }
  cancelScheduledValues() {
    return this;
  }
}

class StubNode {
  constructor(params) {
    for (const p of params) this[p] = new StubParam();
  }
  connect(n) {
    return n;
  }
  disconnect() {}
  start() {}
  stop() {}
}

export function makeAudioContextClass(clock) {
  return class ParityAudioContext {
    constructor() {
      this.sampleRate = 44100;
      this.state = "suspended";
      this._created = clock.ms;
      this.destination = new StubNode([]);
    }
    get currentTime() {
      return (clock.ms - this._created) / 1000;
    }
    resume() {
      this.state = "running";
      return Promise.resolve();
    }
    createOscillator() {
      return new StubNode(["frequency"]);
    }
    createGain() {
      return new StubNode(["gain"]);
    }
    createBiquadFilter() {
      return new StubNode(["frequency", "Q"]);
    }
    createBufferSource() {
      return new StubNode([]);
    }
    createDynamicsCompressor() {
      return new StubNode(["threshold", "knee", "ratio", "attack", "release"]);
    }
    createBuffer(channels, length, sampleRate) {
      const data = [];
      for (let c = 0; c < channels; c++) data.push(new Float32Array(length));
      return { numberOfChannels: channels, length, sampleRate, duration: length / sampleRate, getChannelData: (c) => data[c] };
    }
  };
}

export function computeLayout(w = 430, h = 932) {
  const centerX = w * 0.5;
  const centerY = Math.max(175, h * 0.38);
  const blackHoleY = Math.min(h - 110, Math.max(centerY + 240, h * 0.76));
  return { w, h, centerX, centerY, blackHoleY };
}

export const FRAME_DT = 16 / 1000;

export const PROGRAMS = {
  idle: () => [],
  drag: () => [{ k: "drag", dx: 40 * 1.3 }],
  bursts: (f) => {
    const out = [];
    if (f >= 10 && f <= 40) out.push({ k: "drag", dx: 30 * 1.3 });
    if (f >= 100 && f <= 130) out.push({ k: "drag", dx: -30 * 1.3 });
    if (f >= 250 && f <= 300) out.push({ k: "drag", dx: 20 * 1.3 });
    if (f === 60 || f === 330 || f === 481) out.push({ k: "burst", dir: 1 });
    if (f === 200 || f === 480) out.push({ k: "burst", dir: -1 });
    return out;
  },
};

function installGlobals(rng, clock) {
  const saved = { random: Math.random, win: globalThis.window };
  Math.random = rng;
  const AC = makeAudioContextClass(clock);
  if (typeof globalThis.window === "undefined") globalThis.window = globalThis;
  globalThis.window.AudioContext = AC;
  return () => {
    Math.random = saved.random;
  };
}

export const RESYNC_FRAMES = [0, 50, 200, 400];

export function runPhysicsScenario(mods, spec) {
  const { PhysicsEngine, AudioEngine, MazeLevelBuilder } = mods;
  const frames = spec.frames ?? 600;
  const layout = computeLayout();
  const rng = makeRng(0x5eed1234);
  const clock = { ms: 0 };
  const restore = installGlobals(rng, clock);
  try {
    const log = [];
    let curFrame = 0;
    const rec = (text) => log.push([curFrame, text]);

    class RecordingAudio extends AudioEngine {
      playSpinRatchet(p = 1.0) {
        rec("cue:playSpinRatchet|" + hex(p));
        return super.playSpinRatchet(p);
      }
      playDebrisCollision(i = 1.0, t = "titanium") {
        rec("cue:playDebrisCollision|" + hex(i) + "," + t);
        return super.playDebrisCollision(i, t);
      }
      playDebrisSwallowed() {
        rec("cue:playDebrisSwallowed|");
        return super.playDebrisSwallowed();
      }
      playGatePass(t = "MULTIPLY", v = 2) {
        rec("cue:playGatePass|" + t + "," + hex(v));
        return super.playGatePass(t, v);
      }
      playRcsThrust() {
        rec("cue:playRcsThrust|");
        return super.playRcsThrust();
      }
      playBumperBounce() {
        rec("cue:playBumperBounce|");
        return super.playBumperBounce();
      }
      playHazardExplosion() {
        rec("cue:playHazardExplosion|");
        return super.playHazardExplosion();
      }
    }

    const audio = new RecordingAudio();
    const engine = new PhysicsEngine(audio);
    engine.onDebrisSwallowed = (d) => rec("ev:swallowed:" + d.id);
    engine.onDebrisExploded = (d) => rec("ev:exploded:" + d.id);
    engine.onDebrisSpilled = (d) => rec("ev:spilled:" + d.id);
    engine.onGatePassed = (g, d) => rec("ev:gate:" + g.id + ":" + d.id);
    engine.onBumperHit = (h) => rec("ev:bump:" + h.pin.id + ":" + h.debris.id + ":" + hex(h.x) + ":" + hex(h.y));
    engine.onBoostTriggered = (dir) => rec("ev:boost:" + hex(dir));

    const sector = MazeLevelBuilder.buildLevel(spec.level);
    if (spec.audio) audio.publicInit();
    curFrame = 0;
    engine.initSector(sector, { x: layout.centerX, y: layout.centerY }, { x: layout.centerX, y: layout.blackHoleY });

    const audioState = () => [
      audio.ctx ? 1 : 0, audio.isMuted ? 1 : 0, audio.ctx ? audio.ctx._created : 0, audio.lastRatchetTime,
      audio.lastCollisionTime, audio.lastGatePassTime, audio.lastRcsThrustTime, audio.lastBumperTime || 0,
      audio.lastSwallowTime,
    ];

    const statics = captureStatic(engine);
    const snapshots = [capture(engine)];
    const rngCalls = [rng.calls];
    const resync = [];
    const program = PROGRAMS[spec.program];

    for (let f = 1; f <= frames; f++) {
      const wantResync = spec.resync && RESYNC_FRAMES.includes(f - 1) && f - 1 < frames;
      const pre = wantResync ? { frame: f - 1, state: snapshots[f - 1], audio: Float64Array.from(audioState()), rng: rng.getState(), rngCalls: rng.calls, logStart: log.length } : null;
      curFrame = f;
      clock.ms = 16 * (f - 1);
      const inputs = program(f);
      for (const inp of inputs) {
        if (inp.k === "drag") engine.applyInputDrag(inp.dx);
        else engine.applyRcsBurst(inp.dir);
      }
      clock.ms = 16 * f;
      engine.update(FRAME_DT);
      snapshots.push(capture(engine));
      rngCalls.push(rng.calls);
      if (pre) {
        resync.push({
          frame: pre.frame,
          inputs: inputs.map((i) => (i.k === "drag" ? { k: "drag", dx: hex(i.dx) } : { k: "burst", dir: hex(i.dir) })),
          nowMsBefore: 16 * (f - 1),
          nowMsAfter: 16 * f,
          pre: toBase64(pre.state),
          preAudio: Array.from(pre.audio, hex),
          preRng: pre.rng,
          preRngCalls: pre.rngCalls,
          post: toBase64(snapshots[f]),
          postRng: rng.getState(),
          postRngCalls: rng.calls,
          log: log.slice(pre.logStart).map(([, t]) => t),
        });
      }
    }

    const lengths = snapshots.map((s) => s.length);
    const total = lengths.reduce((a, b) => a + b, 0);
    const all = new Float64Array(total);
    let off = 0;
    for (const s of snapshots) {
      all.set(s, off);
      off += s.length;
    }
    return {
      header: {
        name: spec.name,
        level: spec.level,
        program: spec.program,
        audio: !!spec.audio,
        frames,
        dt: hex(FRAME_DT),
        center: [hex(layout.centerX), hex(layout.centerY)],
        blackHole: [hex(layout.centerX), hex(layout.blackHoleY)],
        statics: Array.from(statics, hex),
        frameLengths: lengths,
        frameHashes: snapshots.map(frameHash),
        rngCalls,
        rngStateEnd: rng.getState(),
        log,
        resync,
        peakDebris: Math.max(...snapshots.map((s) => s[8])),
      },
      framesF64: all,
    };
  } finally {
    restore();
  }
}

export function dumpMaze(mods) {
  const { MazeLevelBuilder } = mods;
  const levels = [];
  for (let i = 1; i <= 50; i++) levels.push({ n: i, text: JSON.stringify(MazeLevelBuilder.buildLevel(i)) });
  const all = MazeLevelBuilder.getSectors().map((s) => JSON.stringify(s));
  const worlds = JSON.stringify(MazeLevelBuilder.getSectorWorlds());
  const daily = [];
  const seeds = [undefined, 0, 1, 12345, 777777, 16777215, 4294967295, 99];
  const tiers = ["bronze", "silver", "gold", undefined, "platinum", ""];
  for (const seed of seeds) {
    for (const tier of tiers) {
      const info = { id: tier ? "daily_" + tier : undefined, tier, title: "Shift " + (tier ?? "none"), quota: 30 + (seed ?? 0) % 7, rewardCredits: 200, hazardModifier: "Mod " + (seed ?? "u"), seed };
      let text;
      try {
        text = JSON.stringify(MazeLevelBuilder.buildDailyShift(info));
      } catch (e) {
        text = "ERROR";
      }
      daily.push({
        info: {
          id: info.id ?? null, tier: tier ?? null, title: info.title, quota: info.quota, rewardCredits: info.rewardCredits,
          hazardModifier: info.hazardModifier, seed: seed === undefined ? null : hex(seed),
        },
        text,
      });
    }
  }
  const raw = [];
  const noTitle = { tier: "gold", seed: 5, quota: 10, rewardCredits: 20 };
  raw.push({ info: { id: null, tier: "gold", title: null, quota: 10, rewardCredits: 20, hazardModifier: null, seed: hex(5) }, text: JSON.stringify(MazeLevelBuilder.buildDailyShift(noTitle)) });
  return { levels, sectors: all, worlds, daily, dailyNoTitle: raw };
}
