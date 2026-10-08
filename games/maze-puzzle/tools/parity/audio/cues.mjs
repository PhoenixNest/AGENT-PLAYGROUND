export const SAMPLE_RATE = 44100;
export const QUANTUM = 128;
export const SEED = 0x5eed1234;

export function mulberry32(a) {
  return function () {
    a |= 0;
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export const T0 = (QUANTUM * 25) / SAMPLE_RATE;
export const T0_UNALIGNED = (QUANTUM * 25 + 57.3) / SAMPLE_RATE;

const slug = (args) =>
  args.length === 0
    ? "none"
    : args
        .map((a) => String(a).replace(/[^A-Za-z0-9.+-]/g, "_").replace(/\./g, "p"))
        .join("_");

function single(method, args, length, t = T0, extra = {}) {
  return {
    id: `${method}__${slug(args)}${extra.suffix ?? ""}`,
    cue: method,
    args,
    calls: [{ t, m: method, a: args }],
    seconds: t + length,
    ...extra.fields,
  };
}

function sequence(id, calls, seconds, fields = {}) {
  return { id, cue: "sequence", args: [], calls, seconds, ...fields };
}

export function buildScenarios() {
  const out = [];

  for (const p of [0.6, 0.8, 1.0, 1.2, 1.5, 1.6, 2.5]) {
    out.push(single("playSpinRatchet", [p], 0.12));
  }
  out.push(single("playSpinRatchet", [], 0.12));

  for (const type of ["titanium", "solar", "volatile", "magnetic"]) {
    for (const intensity of [0.2, 0.6, 1.0, 1.25]) {
      out.push(single("playDebrisCollision", [intensity, type], 0.15));
    }
  }
  out.push(single("playDebrisCollision", [], 0.15));

  for (const v of [2, 3]) out.push(single("playGatePass", ["MULTIPLY", v], 0.4));
  for (const v of [4, 5]) out.push(single("playGatePass", ["MULTIPLY", v], 0.4));
  for (const v of [3, 8, 15]) out.push(single("playGatePass", ["ADD", v], 0.4));
  for (const v of [160, 180]) out.push(single("playGatePass", ["BOOST", v], 0.4));
  out.push(single("playGatePass", [], 0.4));

  out.push(single("playShredderPass", [null, 2], 0.4));
  out.push(single("playCompactorPass", [null, 3], 0.4));
  out.push(single("playSuperShredderPass", [null, 4], 0.4));
  out.push(single("playBoosterPass", [], 0.4));
  out.push(single("playBoosterPass", ["@now"], 0.4));

  out.push(single("playRcsThrust", [], 0.4));
  out.push(single("playBumperBounce", [], 0.2));
  out.push(single("playDebrisSwallowed", [], 0.6));
  out.push(single("playHazardExplosion", [], 0.4));
  out.push(single("playVictoryFanfare", [], 1.2));
  out.push(single("playDefeatSound", [], 1.2));
  out.push(single("playBackupPodSound", [], 0.8));
  out.push(single("setThrusterActive", [true], 0.12));
  out.push(single("setThrusterActive", [false], 0.12, T0, { fields: { rngOnly: true } }));

  const swallowSeq = (n, gap) =>
    Array.from({ length: n }, (_, i) => ({ t: T0 + i * gap, m: "playDebrisSwallowed", a: [] }));
  out.push(sequence("seq_swallow_4", swallowSeq(4, 0.04), T0 + 4 * 0.04 + 0.7));
  out.push(sequence("seq_swallow_12", swallowSeq(12, 0.05), T0 + 12 * 0.05 + 0.7));
  out.push(
    sequence(
      "seq_swallow_restart",
      [
        ...swallowSeq(3, 0.05),
        ...swallowSeq(2, 0.05).map((c) => ({ ...c, t: c.t + 0.9 })),
      ],
      T0 + 1.6,
    ),
  );

  out.push(
    sequence(
      "seq_ratchet_cooldown",
      Array.from({ length: 10 }, (_, i) => ({ t: T0 + i * 0.03, m: "playSpinRatchet", a: [0.8 + i * 0.1] })),
      T0 + 0.45,
    ),
  );
  out.push(
    sequence(
      "seq_collision_storm",
      Array.from({ length: 24 }, (_, i) => ({
        t: T0 + i * 0.02,
        m: "playDebrisCollision",
        a: [Math.min(1, 0.3 + (i % 5) * 0.2), ["titanium", "solar", "volatile"][i % 3]],
      })),
      T0 + 0.8,
    ),
  );
  out.push(
    sequence(
      "seq_gate_cascade",
      Array.from({ length: 14 }, (_, i) => ({
        t: T0 + i * 0.05,
        m: "playGatePass",
        a: [["MULTIPLY", "ADD", "BOOST", "MULTIPLY"][i % 4], [2, 5, 160, 4][i % 4]],
      })),
      T0 + 1.2,
    ),
  );
  out.push(
    sequence(
      "seq_rcs_overlap",
      [0, 0.06, 0.12, 0.2].map((d) => ({ t: T0 + d, m: "playRcsThrust", a: [] })),
      T0 + 0.7,
    ),
  );

  for (const seed of [0xc0ffee, 0xbeef01]) {
    const r = mulberry32(seed);
    const pool = [
      ["playSpinRatchet", () => [0.8 + r() * 0.8]],
      ["playDebrisCollision", () => [r(), ["titanium", "solar", "volatile"][Math.floor(r() * 3)]]],
      ["playGatePass", () => {
        const k = Math.floor(r() * 4);
        return [["MULTIPLY", "ADD", "BOOST", "MULTIPLY"][k], [2, 5, 160, 4][k]];
      }],
      ["playRcsThrust", () => []],
      ["playBumperBounce", () => []],
      ["playDebrisSwallowed", () => []],
      ["playHazardExplosion", () => []],
      ["playVictoryFanfare", () => []],
      ["playBackupPodSound", () => []],
    ];
    const calls = [];
    let t = T0;
    while (t < T0 + 3.0) {
      const [m, mk] = pool[Math.floor(r() * pool.length)];
      calls.push({ t: Number(t.toFixed(6)), m, a: mk() });
      t += 0.01 + r() * 0.09;
    }
    out.push(sequence(`mix_chaos_${seed.toString(16)}`, calls, T0 + 4.2));
  }

  out.push(
    single("playGatePass", ["MULTIPLY", 4], 0.4, T0, {
      suffix: "__sfx20",
      fields: { setup: [{ m: "setSfxVolume", a: [20] }] },
    }),
  );
  out.push(
    single("playSuperShredderPass", [null, 4], 0.4, T0, {
      suffix: "__sfx100",
      fields: { setup: [{ m: "setSfxVolume", a: [100] }] },
    }),
  );
  out.push(
    single("playBumperBounce", [], 0.2, T0, {
      suffix: "__sfx0",
      fields: { setup: [{ m: "setSfxVolume", a: [0] }] },
    }),
  );
  out.push(
    single("playBackupPodSound", [], 0.8, T0, {
      suffix: "__amb0",
      fields: { setup: [{ m: "setAmbienceVolume", a: [0] }] },
    }),
  );
  out.push(
    single("playHazardExplosion", [], 0.4, T0, {
      suffix: "__sfx150",
      fields: { setup: [{ m: "setSfxVolume", a: [150] }, { m: "setAmbienceVolume", a: ["abc"] }] },
    }),
  );
  out.push(
    sequence(
      "mute_before_cue",
      [
        { t: 0.02, m: "toggleMute", a: [] },
        { t: T0, m: "playGatePass", a: ["MULTIPLY", 2] },
        { t: T0 + 0.1, m: "playDebrisCollision", a: [1, "solar"] },
      ],
      T0 + 0.5,
    ),
  );
  const qa = (k) => (QUANTUM * k) / SAMPLE_RATE;
  out.push(
    sequence(
      "mute_cycle",
      [{ t: T0, m: "playVictoryFanfare", a: [] }],
      1.8,
      {
        suspends: [
          { at: qa(125), calls: [{ m: "toggleMute", a: [] }] },
          { at: qa(250), calls: [{ m: "toggleMute", a: [] }, { m: "playBoosterPass", a: [] }] },
        ],
      },
    ),
  );

  for (const [m, a, len] of [
    ["playSpinRatchet", [1.0], 0.12],
    ["playShredderPass", [null, 2], 0.4],
    ["playRcsThrust", [], 0.4],
    ["playDebrisSwallowed", [], 0.6],
    ["playVictoryFanfare", [], 1.2],
    ["playDebrisCollision", [0.8, "solar"], 0.15],
    ["playBumperBounce", [], 0.2],
  ]) {
    out.push(single(m, a, len, T0_UNALIGNED, { suffix: "__unaligned" }));
  }

  out.push(
    sequence(
      "pre_init_calls",
      [
        { t: 0.2, m: "playBoosterPass", a: [] },
        { t: 0.3, m: "playDebrisCollision", a: [1, "solar"] },
        { t: 0.4, m: "playRcsThrust", a: [] },
        { t: 0.5, m: "toggleMute", a: [] },
        { t: 0.6, m: "toggleMute", a: [] },
      ],
      0.8,
      { rngOnly: true, noInit: true },
    ),
  );
  out.push(
    sequence(
      "init_twice_then_cues",
      [
        { t: 0, m: "publicInit", a: [] },
        { t: T0, m: "playDebrisCollision", a: [1, "solar"] },
        { t: T0 + 0.01, m: "playDebrisCollision", a: [1, "solar"] },
        { t: T0 + 0.1, m: "playBumperBounce", a: [] },
        { t: T0 + 0.2, m: "playRcsThrust", a: [] },
      ],
      T0 + 0.7,
    ),
  );
  out.push(
    sequence(
      "init_late_cooldown",
      [
        { t: 0.01, m: "publicInit", a: [] },
        { t: 0.02, m: "playRcsThrust", a: [] },
        { t: 0.03, m: "playBumperBounce", a: [] },
        { t: 0.04, m: "playSpinRatchet", a: [1] },
      ],
      0.5,
      { rngOnly: true, noInit: true },
    ),
  );

  return out;
}
