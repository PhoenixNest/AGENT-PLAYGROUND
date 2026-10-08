const Y = 500;

export function spinProgram({
  dx,
  tickFrames = 1,
  strokeTicks = 13,
  flipEvery = 0,
  ticks,
  startDir = 1,
}) {
  const ops = [];
  let dir = startDir;
  let stroke = 0;
  let x = 0;
  let down = false;
  for (let k = 0; k < ticks; k++) {
    if (k % strokeTicks === 0) {
      if (down) ops.push({ op: "pointer", kind: "up", x, y: Y });
      if (flipEvery && k > 0 && k % flipEvery === 0) dir = -dir;
      const alt = stroke++ % 2 === 1;
      x = dir > 0 ? (alt ? 120 : 40) : alt ? 310 : 390;
      ops.push({ op: "pointer", kind: "down", x, y: Y });
      down = true;
    }
    x += dir * dx * tickFrames;
    ops.push({ op: "pointer", kind: "move", x, y: Y });
    ops.push({ op: "advance", ms: 16 * tickFrames });
  }
  if (down) ops.push({ op: "pointer", kind: "up", x, y: Y });
  return ops;
}
