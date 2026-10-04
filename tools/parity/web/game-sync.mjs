/** FNV-1a 64 over little-endian IEEE-754 doubles. Shared with PhysicsSnapshot.HashDoubles. */
export function hashDoubles(values) {
  let h = 14695981039346656037n;
  const prime = 1099511628211n;
  const mask = 0xffffffffffffffffn;
  const buf = new ArrayBuffer(8);
  const view = new DataView(buf);
  const bytes = new Uint8Array(buf);
  for (const d of values) {
    view.setFloat64(0, d, true);
    for (let i = 0; i < 8; i++) {
      h ^= BigInt(bytes[i]);
      h = (h * prime) & mask;
    }
  }
  return h.toString(16).padStart(16, "0");
}
