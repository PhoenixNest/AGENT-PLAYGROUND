const dv = new DataView(new ArrayBuffer(8));

export function hex(d) {
  dv.setFloat64(0, d);
  return dv.getBigUint64(0).toString(16).padStart(16, "0");
}

export function fromHex(h) {
  dv.setBigUint64(0, BigInt("0x" + h));
  return dv.getFloat64(0);
}

export function hexOrNaN(d) {
  return Number.isNaN(d) ? "nan" : hex(d);
}

export function bitsReplacer(_key, value) {
  if (typeof value === "number") return "#" + (Number.isNaN(value) ? "nan" : hex(value));
  return value;
}
