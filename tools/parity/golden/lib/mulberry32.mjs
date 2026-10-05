export const PARITY_SEED = 0x5eed1234;

export function mulberry32(a) {
  return function () {
    a |= 0;
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export function countingRandom(seed = PARITY_SEED) {
  const inner = mulberry32(seed);
  const state = { calls: 0, tag: null, log: null };
  const fn = function () {
    state.calls++;
    return inner();
  };
  fn.state = state;
  return fn;
}
