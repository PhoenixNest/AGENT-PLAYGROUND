// Writes scenes/*.json: small command streams that isolate one primitive or state combination each.
// The stream format is identical to the recorded streams (see lib/player.js): ops are
// [name, ...args] for method calls and ["=prop", value] for property sets; gradients are created with
// ["createLinearGradient"|"createRadialGradient", id, ...args], ["addColorStop", id, offset, colour] and referenced
// as {"g": id}; images as {"img": name} (procedural) or {"emoji": grapheme}.
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const dir = path.join(here, "scenes");
fs.rmSync(dir, { recursive: true, force: true });
fs.mkdirSync(dir, { recursive: true });

const DPR = 3;
const BG = "#1e1b4b";

class Scene {
  constructor(name, desc, w = 160, h = 160, bg = BG) {
    this.name = name;
    this.desc = desc;
    this.w = w;
    this.h = h;
    this.ops = [["setTransform", DPR, 0, 0, DPR, 0, 0]];
    this.images = undefined;
    this.gid = 0;
    if (bg) this.ops.push(["=fillStyle", bg], ["fillRect", 0, 0, w, h]);
  }
  op(...a) { this.ops.push(a); return this; }
  set(p, v) { this.ops.push(["=" + p, v]); return this; }
  save() { return this.op("save"); }
  restore() { return this.op("restore"); }
  circle(x, y, r) { return this.op("beginPath").op("arc", x, y, r, 0, Math.PI * 2); }
  fillCircle(x, y, r, c) { if (c) this.set("fillStyle", c); return this.circle(x, y, r).op("fill"); }
  line(x0, y0, x1, y1) { return this.op("beginPath").op("moveTo", x0, y0).op("lineTo", x1, y1).op("stroke"); }
  poly(pts, close = false) {
    this.op("beginPath").op("moveTo", pts[0][0], pts[0][1]);
    for (const p of pts.slice(1)) this.op("lineTo", p[0], p[1]);
    if (close) this.op("closePath");
    return this;
  }
  linear(x0, y0, x1, y1, stops) {
    const id = this.gid++;
    this.op("createLinearGradient", id, x0, y0, x1, y1);
    for (const [o, c] of stops) this.op("addColorStop", id, o, c);
    return { g: id };
  }
  radial(x0, y0, r0, x1, y1, r1, stops) {
    const id = this.gid++;
    this.op("createRadialGradient", id, x0, y0, r0, x1, y1, r1);
    for (const [o, c] of stops) this.op("addColorStop", id, o, c);
    return { g: id };
  }
  write() {
    const o = {
      version: 1,
      name: this.name,
      desc: this.desc,
      meta: { width: this.w, height: this.h, dpr: DPR },
      ops: this.ops,
    };
    if (this.images) o.images = this.images;
    fs.writeFileSync(path.join(dir, this.name + ".json"), JSON.stringify(o));
  }
}

const scenes = [];
const add = (s) => { scenes.push(s); return s; };
const PI = Math.PI;
const TAU = PI * 2;

// ---------------------------------------------------------------- circles
{
  const s = add(new Scene("circles_small", "AA discs of radius 0.5..21 at integer and fractional centres", 200, 120));
  const radii = [0.5, 0.75, 1, 1.5, 2, 3, 4, 5, 6.5, 8, 10, 13, 17, 21];
  let x = 14;
  radii.forEach((r, i) => {
    const fx = (i % 4) * 0.25;
    s.fillCircle(x + fx, 22 + fx, r, "#fde047");
    s.fillCircle(x + 0.37, 62.6 + fx, r, "rgba(244, 114, 182, 0.7)");
    x += r * 2 + 6;
    if (x > 190) x = 14;
  });
  for (let i = 0; i < 14; i++) s.fillCircle(10 + i * 13.3, 100.5 + (i % 3) * 0.33, 0.5 + i * 0.35, "#38bdf8");
}
{
  const s = add(new Scene("circles_large", "Discs radius 34, 55, 89, 144 and 200 (large AA edge length)", 430, 560));
  s.fillCircle(80, 80, 34, "#4ade80").fillCircle(215.3, 90.2, 55, "#f472b6").fillCircle(360, 100, 34.5, "#fb923c");
  s.fillCircle(100.5, 280, 89, "#c084fc").fillCircle(335.7, 300.1, 89.3, "#fde047");
  s.fillCircle(215, 400, 200, "rgba(56, 189, 248, 0.55)");
}
{
  const s = add(new Scene("circle_strokes", "Stroked circles, widths 0.25..14, radius 3..60", 200, 200));
  const widths = [0.25, 0.5, 1, 1.5, 2, 3, 4.5, 7, 10, 14];
  s.set("strokeStyle", "#ffffff");
  widths.forEach((w, i) => {
    s.set("lineWidth", w);
    const r = 3 + i * 1.2;
    s.circle(18 + (i % 5) * 36.3, 22 + Math.floor(i / 5) * 38.7, r).op("stroke");
  });
  s.set("strokeStyle", "#fde047");
  [60, 45, 30].forEach((r, i) => { s.set("lineWidth", [3, 5.5, 1.3][i]); s.circle(100.4, 140.3, r).op("stroke"); });
}

// ---------------------------------------------------------------- lines
{
  const s = add(new Scene("lines_thin", "Hairline-to-thick strokes: axis aligned at fractional positions, diagonals", 200, 140));
  s.set("strokeStyle", "#ffffff");
  [0.25, 0.5, 0.75, 1, 1.5, 2, 3, 5].forEach((w, i) => {
    s.set("lineWidth", w);
    s.line(10, 10.0 + i * 5.13, 90, 10.0 + i * 5.13);
    s.line(110.5 + i * 8.3, 10, 110.5 + i * 8.3, 50.4);
    s.line(10, 60 + i * 9, 80, 60 + i * 9 + 7.3 - i);
  });
  s.set("lineWidth", 1);
  for (let a = 0; a < 16; a++) s.line(150, 100, 150 + Math.cos(a * TAU / 16) * 40, 100 + Math.sin(a * TAU / 16) * 30);
}
{
  const s = add(new Scene("caps_joins", "All 3 caps x 3 joins on sharp, right, obtuse polylines, miter limits, closed paths", 300, 330));
  const joins = ["miter", "round", "bevel"];
  const caps = ["butt", "round", "square"];
  s.set("strokeStyle", "#fde047").set("lineWidth", 10);
  let n = 0;
  for (const j of joins) {
    for (const c of caps) {
      const ox = 12 + (n % 3) * 96, oy = 10 + Math.floor(n / 3) * 62;
      s.set("lineJoin", j).set("lineCap", c);
      s.poly([[ox, oy + 40], [ox + 22, oy + 4], [ox + 44, oy + 40]]).op("stroke");
      s.poly([[ox + 50, oy + 8], [ox + 80, oy + 8], [ox + 80, oy + 40]]).op("stroke");
      n++;
    }
  }
  s.set("lineCap", "butt").set("strokeStyle", "#4ade80").set("lineWidth", 6);
  [1, 2, 4, 10].forEach((ml, i) => {
    s.set("miterLimit", ml).set("lineJoin", "miter");
    const ox = 12 + i * 70, oy = 205;
    s.poly([[ox, oy + 50], [ox + 14, oy], [ox + 28, oy + 50]]).op("stroke");
    s.poly([[ox, oy + 62], [ox + 40, oy + 56], [ox, oy + 70]]).op("stroke");
  });
  s.set("miterLimit", 10).set("lineJoin", "round").set("strokeStyle", "#f472b6").set("lineWidth", 5);
  s.poly([[20, 285], [60, 285], [60, 320], [20, 320]], true).op("stroke");
  s.set("lineJoin", "miter").poly([[100, 285], [140, 320], [180, 285]], true).op("stroke");
  s.set("lineJoin", "bevel").poly([[200, 320], [220, 285], [240, 320], [260, 285]]).op("stroke");
}
{
  const s = add(new Scene("zero_length", "Degenerate subpaths: dot with round/square cap, nothing with butt, zero-length closed", 160, 60));
  s.set("strokeStyle", "#ffffff").set("lineWidth", 12);
  ["butt", "round", "square"].forEach((c, i) => {
    s.set("lineCap", c).op("beginPath").op("moveTo", 24 + i * 50, 30).op("lineTo", 24 + i * 50, 30).op("stroke");
  });
  s.set("lineCap", "round").set("strokeStyle", "#fde047");
  s.op("beginPath").op("moveTo", 20, 10).op("closePath").op("stroke");
  s.set("lineWidth", 0).op("beginPath").op("moveTo", 80, 10).op("lineTo", 140, 10).op("stroke");
}

// ---------------------------------------------------------------- dashes
{
  const s = add(new Scene("dashes", "setLineDash patterns and offsets on lines, circles, ellipses, closed rect, caps", 260, 300));
  s.set("strokeStyle", "#ffffff").set("lineWidth", 3.5);
  const pats = [[12, 8], [14, 16], [6, 8], [1, 3], [5, 10, 15], [20], [3, 0], [0, 6]];
  pats.forEach((p, i) => {
    s.set("lineCap", i % 2 ? "round" : "butt").op("setLineDash", p).set("lineDashOffset", 0);
    s.line(10, 10 + i * 8, 120, 10 + i * 8 + 3);
  });
  s.set("lineCap", "butt").op("setLineDash", [12, 8]);
  [0, 5, 10.5, -7, 200.3].forEach((o, i) => {
    s.set("lineDashOffset", o).set("strokeStyle", ["#fde047", "#f472b6", "#38bdf8", "#4ade80", "#c084fc"][i]);
    s.line(135, 10 + i * 9, 250, 10 + i * 9);
  });
  s.set("lineCap", "round").op("setLineDash", [10, 14]).set("lineDashOffset", -37.2).set("lineWidth", 4.5).set("strokeStyle", "#fde047");
  s.op("beginPath").op("ellipse", 65, 120, 50, 38, 0.4, 0, TAU).op("stroke");
  s.op("setLineDash", [6, 8]).set("lineWidth", 3.2).set("strokeStyle", "#38bdf8").set("lineDashOffset", 3).circle(190, 120, 36).op("stroke");
  s.set("lineCap", "butt").op("setLineDash", [8, 5]).set("lineDashOffset", 2).set("lineWidth", 2).set("strokeStyle", "#ffffff");
  s.op("beginPath").op("rect", 20, 180, 90, 50).op("stroke");
  s.op("beginPath").op("moveTo", 130, 180).op("lineTo", 170, 230).op("lineTo", 210, 180).op("closePath").op("stroke");
  s.op("setLineDash", [12, 8]).set("lineDashOffset", -20).set("lineCap", "round").set("lineWidth", 3.5).set("strokeStyle", "#38bdf8");
  s.op("beginPath").op("moveTo", 20, 250).op("quadraticCurveTo", 100, 300, 240, 250).op("stroke");
  s.op("setLineDash", []);
}

// ---------------------------------------------------------------- arcs/ellipses/curves
{
  const s = add(new Scene("ellipses_arcs", "Rotated ellipses, partial arcs, anticlockwise, sweeps > 2pi, arc after lineTo", 260, 220));
  s.set("fillStyle", "rgba(250, 204, 21, 0.85)").set("strokeStyle", "#ffffff").set("lineWidth", 2);
  [0, 0.3, 0.8, 1.5707963267948966, 2.4].forEach((rot, i) => {
    s.op("beginPath").op("ellipse", 30 + i * 48, 30, 20, 9, rot, 0, TAU).op("fill").op("stroke");
  });
  s.op("beginPath").op("ellipse", 40, 80, 30, 14, -0.3, 0.5, 4.2).op("stroke");
  s.op("beginPath").op("ellipse", 110, 80, 30, 14, 0.2, 0.5, 4.2, true).op("stroke");
  s.op("beginPath").op("ellipse", 180, 80, 14, 30, 0, 0, TAU * 1.5).op("stroke");
  s.set("lineWidth", 4).set("lineCap", "round");
  s.op("beginPath").op("arc", 40, 140, 26, PI * 0.2, PI * 1.7).op("stroke");
  s.op("beginPath").op("arc", 110, 140, 26, PI * 1.7, PI * 0.2).op("stroke");
  s.op("beginPath").op("arc", 180, 140, 26, PI * 1.7, PI * 0.2, true).op("stroke");
  s.set("lineCap", "butt").set("lineWidth", 1.5);
  s.op("beginPath").op("moveTo", 10, 190).op("arc", 50, 200, 12, 0, PI).op("lineTo", 90, 175).op("arc", 120, 200, 10, 0, TAU).op("fill").op("stroke");
  s.op("beginPath").op("arc", 200, 200, 15, 1, 1).op("stroke");
  s.op("beginPath").op("arc", 235, 200, 10, 0, 100).op("stroke");
}
{
  const s = add(new Scene("curves", "quadratic star (as in the app), bezier, arcTo, nested closed paths, nonzero winding", 260, 180));
  const star = (x, y, size, c) => {
    s.set("fillStyle", c).op("beginPath").op("moveTo", x, y - size)
      .op("quadraticCurveTo", x, y, x + size, y).op("quadraticCurveTo", x, y, x, y + size)
      .op("quadraticCurveTo", x, y, x - size, y).op("quadraticCurveTo", x, y, x, y - size).op("fill");
  };
  star(30, 30, 8, "#fde047"); star(60.4, 30.3, 5.1, "#ffffff"); star(90, 30, 12.7, "#f472b6"); star(130, 32, 22, "#38bdf8");
  s.set("strokeStyle", "#ffffff").set("lineWidth", 3);
  s.op("beginPath").op("moveTo", 160, 50).op("bezierCurveTo", 170, -10, 220, 80, 250, 20).op("stroke");
  s.op("beginPath").op("moveTo", 160, 75).op("bezierCurveTo", 190, 60, 220, 90, 250, 70).op("lineTo", 250, 80).op("bezierCurveTo", 210, 120, 180, 90, 160, 100).op("closePath").op("stroke");
  s.set("lineWidth", 5).set("lineJoin", "round");
  s.op("beginPath").op("moveTo", 20, 90).op("arcTo", 70, 90, 70, 140, 25).op("lineTo", 70, 150).op("stroke");
  s.set("fillStyle", "#4ade80").op("beginPath").op("rect", 100, 100, 60, 60).op("rect", 115, 115, 30, 30).op("fill");
  s.set("fillStyle", "#fb923c").op("beginPath").op("rect", 180, 100, 60, 60).op("rect", 195, 115, 30, 30).op("closePath").op("fill");
}
{
  const s = add(new Scene("roundrects", "roundRect: radii 0..huge, array radii, negative size, fill/stroke, shadows off", 280, 190));
  s.set("fillStyle", "#f472b6").set("strokeStyle", "#fde047").set("lineWidth", 2);
  [0, 2.5, 4.5, 8.5, 12, 30].forEach((r, i) => {
    s.op("beginPath").op("roundRect", 10 + i * 44.5, 10.25, 38, 28, r).op("fill").op("stroke");
  });
  s.op("beginPath").op("roundRect", 10, 60, 80, 50, [20, 0, 20, 0]).op("fill").op("stroke");
  s.op("beginPath").op("roundRect", 100, 60, 80, 50, [30, 10]).op("fill").op("stroke");
  s.op("beginPath").op("roundRect", 190, 60, 80, 50, [5, 10, 20]).op("fill").op("stroke");
  s.op("beginPath").op("roundRect", 90, 160, -70, -40, 6).op("fill").op("stroke");
  s.op("beginPath").op("roundRect", 100, 120, 10, 60, 40).op("fill");
  s.op("beginPath").op("roundRect", 130, 130, 140, 50, { x: 10, y: 25 }).op("fill").op("stroke");
}

// ---------------------------------------------------------------- clip
{
  const s = add(new Scene("clipping", "clip rect, circle, nested, evenodd, with transforms, strokes and gradients inside", 260, 200));
  s.save().op("beginPath").op("rect", 10, 10, 60, 40).op("clip");
  s.fillCircle(30, 30, 40, "#fde047").set("strokeStyle", "#fff").set("lineWidth", 6).line(0, 0, 80, 60);
  s.restore();
  s.save().circle(110, 30, 22).op("clip");
  s.set("fillStyle", s.linear(88, 8, 132, 52, [[0, "#f472b6"], [1, "#38bdf8"]])).op("fillRect", 80, 0, 80, 60);
  s.restore();
  s.save().circle(200, 30, 24).op("clip").circle(215, 30, 20).op("clip");
  s.fillCircle(200, 30, 50, "#4ade80");
  s.restore();
  s.save().op("beginPath").op("rect", 10, 70, 60, 60).op("arc", 40, 100, 20, 0, TAU).op("clip", "evenodd");
  s.set("fillStyle", "#c084fc").op("fillRect", 0, 60, 90, 90);
  s.restore();
  s.save().op("translate", 120, 100).op("rotate", 0.5).op("beginPath").op("roundRect", -30, -20, 60, 40, 10).op("clip").op("rotate", -0.9);
  s.set("fillStyle", "#fb923c").op("fillRect", -50, -50, 100, 100).set("fillStyle", "#ffffff").op("fillRect", -50, -5, 100, 10);
  s.restore();
  s.save().circle(215, 100, 28).op("clip").set("shadowColor", "#38bdf8").set("shadowBlur", 8);
  s.fillCircle(200, 95, 30, "#ec4899");
  s.restore();
  s.save().op("beginPath").op("rect", 20, 150, 220, 30).op("clip").set("strokeStyle", "#ffffff").set("lineWidth", 8).set("lineCap", "round").op("setLineDash", [10, 12]).line(0, 165, 260, 165);
  s.restore();
}

// ---------------------------------------------------------------- gradients
{
  const s = add(new Scene("gradients_linear", "2-3 stop, alpha stops (transparent black), reversed, equal offsets, padding, degenerate, under CTM", 260, 230));
  const rect = (x, y, w, h, g) => { s.set("fillStyle", g).op("fillRect", x, y, w, h); };
  rect(10, 10, 70, 40, s.linear(10, 0, 80, 0, [[0, "#f472b6"], [1, "#38bdf8"]]));
  rect(95, 10, 70, 40, s.linear(95, 10, 95, 50, [[0, "#1e1b4b"], [0.5, "#312e81"], [1, "#4338ca"]]));
  rect(180, 10, 70, 40, s.linear(180, 10, 250, 50, [[0, "#fde047"], [1, "#ef4444"]]));
  rect(10, 60, 70, 40, s.linear(10, 60, 80, 60, [[0, "rgba(0, 0, 0, 0)"], [1, "#ffffff"]]));
  rect(95, 60, 70, 40, s.linear(95, 60, 165, 60, [[0, "rgba(255, 255, 255, 0)"], [1, "#ef4444"]]));
  rect(180, 60, 70, 40, s.linear(180, 60, 250, 60, [[0, "#818cf855"], [0.55, "#818cf822"], [1, "rgba(56, 189, 248, 0.04)"]]));
  rect(10, 110, 70, 40, s.linear(80, 110, 10, 110, [[0, "#4ade80"], [1, "#c084fc"]]));
  rect(95, 110, 70, 40, s.linear(115, 110, 145, 110, [[0, "#ef4444"], [0.5, "#ef4444"], [0.5, "#38bdf8"], [1, "#38bdf8"]]));
  rect(180, 110, 70, 40, s.linear(200, 110, 230, 110, [[0.2, "#fde047"], [0.8, "#f472b6"]]));
  rect(10, 160, 70, 30, s.linear(40, 160, 40, 160, [[0, "#ffffff"], [1, "#000000"]]));
  s.save().op("translate", 100, 170).op("rotate", 0.6).op("scale", 1.5, 0.7);
  rect(0, 0, 50, 40, s.linear(0, 0, 50, 0, [[0, "#ffffff"], [1, "#ef4444"]]));
  s.restore();
  rect(180, 160, 70, 30, s.linear(180, 160, 250, 190, [[0, "#ffffffcc"], [1, "#00000000"]]));
  s.set("strokeStyle", s.linear(10, 205, 250, 205, [[0, "#fde047"], [1, "#ec4899"]])).set("lineWidth", 8).set("lineCap", "round").line(14, 210, 246, 210);
}
{
  const s = add(new Scene("gradients_radial", "concentric, off-centre inner circle, r0=0, alpha halo ending in transparent black, r0>r1", 260, 200));
  const disc = (x, y, r, g) => { s.set("fillStyle", g).circle(x, y, r).op("fill"); };
  disc(40, 40, 30, s.radial(40, 40, 0, 40, 40, 30, [[0, "#ffffff"], [1, "#ef4444"]]));
  disc(110, 40, 22, s.radial(110 - 6.6, 40 - 6.6, 1, 110, 40, 22, [[0, "#ffffff"], [0.45, "#f43f5e"], [1, "#be123c"]]));
  disc(190, 40, 40, s.radial(190, 40, 8, 190, 40, 40, [[0, "#818cf866"], [0.45, "#818cf828"], [1, "rgba(0, 0, 0, 0)"]]));
  disc(40, 120, 35, s.radial(40, 120, 22, 40, 120, 8, [[0, "#fde047"], [1, "#4ade80"]]));
  disc(120, 120, 35, s.radial(130, 120, 4, 120, 120, 34, [[0, "#38bdf8"], [1, "rgba(56, 189, 248, 0)"]]));
  s.set("fillStyle", s.radial(210, 120, 10, 210, 120, 34, [[0.3, "#f472b6"], [0.7, "#fde047"]])).op("fillRect", 175, 85, 70, 70);
  s.set("strokeStyle", s.radial(40, 175, 0, 40, 175, 40, [[0, "#ffffff"], [1, "#ef4444"]])).set("lineWidth", 7).circle(40, 175, 20).op("stroke");
  s.save().op("translate", 130, 175).op("scale", 1.6, 0.8);
  s.set("fillStyle", s.radial(0, 0, 3, 0, 0, 22, [[0, "#ffffff"], [1, "#6366f1"]])).circle(0, 0, 22).op("fill");
  s.restore();
}

// ---------------------------------------------------------------- shadows
{
  const s = add(new Scene("shadows_fill", "shadowBlur 0/6/8/18 on fills, coloured and translucent shadows, offsets, gradient fill, alpha", 260, 190));
  const cfg = [[0, "#38bdf8"], [6, "#38bdf8"], [8, "#fde047"], [18, "#38bdf8"]];
  cfg.forEach(([b, c], i) => {
    s.save().set("shadowColor", c).set("shadowBlur", b).set("fillStyle", "#ef4444");
    s.op("beginPath").op("arc", 35 + i * 62, 35, 14, 0, TAU).op("fill").restore();
  });
  s.save().set("shadowColor", "rgba(0, 0, 0, 0.6)").set("shadowBlur", 6).set("shadowOffsetX", 5).set("shadowOffsetY", 7).set("fillStyle", "#fde047");
  s.op("beginPath").op("roundRect", 20, 75, 50, 30, 8).op("fill").restore();
  s.save().set("shadowColor", "#ec4899").set("shadowBlur", 0).set("shadowOffsetX", 6).set("shadowOffsetY", 6).set("fillStyle", "#ffffff");
  s.op("beginPath").op("roundRect", 95, 75, 50, 30, 8).op("fill").restore();
  s.save().set("shadowColor", "#38bdf8").set("shadowBlur", 12).set("fillStyle", "rgba(250, 204, 21, 0.4)").circle(190, 90, 18).op("fill").restore();
  s.save().set("shadowColor", "#000000").set("shadowBlur", 10).set("globalAlpha", 0.5).set("fillStyle", "#4ade80").circle(35, 150, 16).op("fill").restore();
  s.save().set("shadowColor", "#ffffff").set("shadowBlur", 8).set("fillStyle", s.linear(80, 130, 130, 170, [[0, "#f472b6"], [1, "#c084fc"]]));
  s.op("fillRect", 80, 135, 50, 32).restore();
  s.save().op("scale", 2, 2).set("shadowColor", "#38bdf8").set("shadowBlur", 6).set("shadowOffsetX", 3).set("shadowOffsetY", 3).set("fillStyle", "#ef4444").circle(80, 78, 8).op("fill").restore();
  s.save().op("rotate", 0.3).set("shadowColor", "#fde047").set("shadowBlur", 6).set("shadowOffsetX", 10).set("fillStyle", "#ffffff").op("fillRect", 190, 100, 30, 20).restore();
}
{
  const s = add(new Scene("shadows_stroke", "stroked shadows: round caps, dashed rails with glow, rim glow with blur 18", 260, 190));
  s.save().set("strokeStyle", "#38bdf8").set("shadowColor", "#38bdf8").set("shadowBlur", 8).set("lineWidth", 3.5).set("lineCap", "round").op("setLineDash", [12, 8]).set("lineDashOffset", -17);
  s.line(20, 15, 60, 120).line(120, 15, 80, 120).restore();
  s.save().set("strokeStyle", "#ec4899").set("shadowColor", "#38bdf8").set("shadowBlur", 18).set("lineWidth", 14).set("lineCap", "round");
  s.op("beginPath").op("arc", 190, 70, 40, 0.3, 3.9).op("stroke").restore();
  s.save().set("strokeStyle", "#ffffff").set("shadowColor", "#ef4444").set("shadowBlur", 6).set("lineWidth", 1.5);
  s.op("beginPath").op("rect", 20, 140, 60, 30).op("stroke").restore();
  s.save().set("strokeStyle", "#fde047").set("shadowColor", "rgba(255, 255, 255, 0.5)").set("shadowBlur", 4).set("lineWidth", 6).set("shadowOffsetY", 4);
  s.op("beginPath").op("moveTo", 110, 170).op("quadraticCurveTo", 150, 120, 190, 170).op("stroke").restore();
  s.save().set("strokeStyle", "rgba(236, 72, 153, 0.5)").set("shadowColor", "#38bdf8").set("shadowBlur", 8).set("lineWidth", 8);
  s.line(205, 135, 245, 175).restore();
}

// ---------------------------------------------------------------- alpha / compositing
{
  const s = add(new Scene("alpha_stacking", "globalAlpha steps, nested alpha x colour alpha, overlapping fill+stroke, rgba quantisation", 220, 150));
  for (let i = 0; i < 10; i++) {
    s.save().set("globalAlpha", 0.1 + i * 0.1).set("fillStyle", "#ffffff").op("fillRect", 8 + i * 20, 8, 18, 30).restore();
  }
  [0.1, 0.25, 0.35, 0.4, 0.5, 0.65, 0.85, 0.99].forEach((a, i) => {
    s.set("fillStyle", `rgba(244, 114, 182, ${a})`).op("fillRect", 8 + i * 25, 45, 22, 22);
  });
  s.set("fillStyle", "#fde047");
  for (let i = 0; i < 5; i++) s.save().set("globalAlpha", 0.5).set("fillStyle", "rgba(56, 189, 248, 0.5)").circle(30 + i * 14, 95, 20).op("fill").restore();
  s.save().set("globalAlpha", 0.6).set("fillStyle", "rgba(239, 68, 68, 0.7)").set("strokeStyle", "rgba(255, 255, 255, 0.8)").set("lineWidth", 8);
  s.op("beginPath").op("roundRect", 120, 80, 80, 50, 12).op("fill").op("stroke").restore();
  s.set("globalAlpha", 1);
  s.set("fillStyle", "#00000001").op("fillRect", 100, 100, 5, 5);
  s.set("fillStyle", "#ffffff08").op("fillRect", 108, 100, 5, 5);
  s.set("fillStyle", "rgba(255,255,255,0.004)").op("fillRect", 108, 106, 5, 5);
}
{
  const s = add(new Scene("shear_transforms", "transform(1,0,-0.22,1,0,0), nested rotate/scale/translate on rects, strokes, circles", 200, 150));
  s.save().op("translate", 60, 30).op("transform", 1, 0, -0.22, 1, 0, 0);
  s.set("fillStyle", "#f472b6").set("strokeStyle", "#1e1b4b").set("lineWidth", 2).op("beginPath").op("roundRect", -40, -14, 80, 28, 4.5).op("fill").op("stroke");
  s.restore();
  s.save().op("translate", 150, 35).op("transform", 1, 0, -0.22, 1, 0, 0).set("fillStyle", "#38bdf8").circle(0, 0, 18).op("fill").set("strokeStyle", "#ffffff").set("lineWidth", 4).op("stroke").restore();
  s.save().op("translate", 50, 100).op("rotate", 0.4).op("scale", 1.3, 1.3).op("rotate", -0.9);
  s.set("fillStyle", "#fde047").op("fillRect", -20, -10, 40, 20).set("strokeStyle", "#ffffff").set("lineWidth", 1.7).op("strokeRect", -20, -10, 40, 20);
  s.restore();
  s.save().op("setTransform", 4.5, 1, -2, 3.2, 90, 80).set("strokeStyle", "#4ade80").set("lineWidth", 3).set("lineCap", "round").line(0, 0, 20, 10).op("beginPath").op("arc", 30, 20, 8, 0, TAU).op("stroke").restore();
  s.save().op("scale", 1.7, 0.6).set("strokeStyle", "#fb923c").set("lineWidth", 5).circle(100, 150, 26).op("stroke").restore();
}

// ---------------------------------------------------------------- misc state
{
  const s = add(new Scene("state_stack", "save/restore of style, dash, shadow, alpha, transform; rect/clearRect; invalid lineWidth ignored", 160, 120));
  s.set("fillStyle", "#f472b6").op("fillRect", 10, 10, 30, 30);
  s.save().set("fillStyle", "#fde047").op("translate", 50, 0).set("globalAlpha", 0.5).op("fillRect", 10, 10, 30, 30);
  s.save().set("fillStyle", "#38bdf8").op("translate", 50, 0).op("fillRect", 10, 10, 30, 30).restore();
  s.restore();
  s.op("fillRect", 10, 50, 30, 30);
  s.set("strokeStyle", "#ffffff").set("lineWidth", 4).set("lineWidth", 0).set("lineWidth", -2).set("lineWidth", "NaN").line(60, 60, 140, 60);
  s.set("fillStyle", "#4ade80").op("fillRect", 10, 90, 140, 20).op("clearRect", 30, 95, 20.5, 10.5).op("clearRect", 90.25, 92.75, 33.5, 5);
  s.op("beginPath").op("rect", 60, 70, 50, 10).op("fill");
}

// ---------------------------------------------------------------- text
const FONTS = [
  "bold 13px Fredoka, sans-serif", "11px Nunito, sans-serif", "bold 10px Fredoka, sans-serif",
  "900 10.5px Fredoka, sans-serif", "bold 9.5px Fredoka, sans-serif", "900 16px Fredoka, sans-serif",
  "800 14px Nunito, sans-serif", "400 14px Fredoka, sans-serif",
];
{
  const s = add(new Scene("text_fonts", "every ctx.font used by the app (and Nunito 800, Fredoka 400): centre aligned, middle and alphabetic", 240, 260));
  s.set("fillStyle", "#ffffff").set("textAlign", "center");
  FONTS.forEach((f, i) => {
    s.set("font", f).set("textBaseline", i % 2 ? "alphabetic" : "middle");
    s.op("fillText", "SORT RIG 0123456789 ×•", 120, 18 + i * 20);
  });
  s.set("font", FONTS[0]).set("textBaseline", "middle").set("fillStyle", "#fde047");
  s.op("fillText", "ORBITAL x2 +3 ✓ ▶ ←", 120, 190).op("fillText", "Hamburgefonstiv AVATAR To Wa", 120, 210);
  s.set("font", FONTS[1]).op("fillText", "SPARKLE PLANETARY COSMIC RIG fi fl", 120, 232);
}
{
  const s = add(new Scene("text_align_baseline", "left/center/right/start/end and alphabetic/middle/top/bottom/ideographic/hanging at fractional positions", 260, 200));
  s.set("font", "bold 13px Fredoka, sans-serif").set("fillStyle", "#ffffff");
  s.set("strokeStyle", "#ef4444").set("lineWidth", 0.5);
  s.line(130, 0, 130, 200);
  ["left", "center", "right", "start", "end"].forEach((a, i) => {
    s.set("textAlign", a).set("textBaseline", "alphabetic").op("fillText", "Align " + a, 130, 16 + i * 18);
  });
  s.set("textAlign", "left");
  ["alphabetic", "middle", "top", "bottom", "ideographic", "hanging"].forEach((b, i) => {
    const y = 125 + (i % 3) * 22;
    const x = 8 + Math.floor(i / 3) * 124;
    s.set("strokeStyle", "#ef4444").line(x, y, x + 110, y);
    s.set("textBaseline", b).op("fillText", b.slice(0, 5) + " Hxg", x + 2, y);
  });
  [0, 0.25, 0.5, 0.75].forEach((f, i) => {
    s.set("textAlign", "left").set("textBaseline", "alphabetic").set("font", "11px Nunito, sans-serif").op("fillText", "subpx", 10 + f, 185 + i * 0.0).op("fillText", "subpx", 60 + i * 50 + f * 0, 185 + f);
  });
}
{
  const s = add(new Scene("text_stroke_scale", "strokeText + fillText with scale 1.3 as the floating text does, rotated text, thin/wide outlines", 260, 190));
  [[0.85, 1.3], [0.5, 1.15], [0.2, 1.0]].forEach(([a, sc], i) => {
    s.save().set("globalAlpha", a).op("translate", 130, 30 + i * 40).op("scale", sc, sc);
    s.set("font", "900 16px Fredoka, sans-serif").set("textAlign", "center").set("strokeStyle", "#1e1b4b").set("lineWidth", 3.5).op("strokeText", "NOM! YUM 8", 0, 0);
    s.set("fillStyle", "#fde047").op("fillText", "NOM! YUM 8", 0, 0).restore();
  });
  s.save().op("translate", 60, 160).op("rotate", -0.35).set("font", "bold 13px Fredoka, sans-serif").set("textAlign", "center").set("fillStyle", "#ffffff").op("fillText", "ROTATED", 0, 0).restore();
  s.save().op("translate", 190, 160).op("transform", 1, 0, -0.22, 1, 0, 0).set("font", "bold 9.5px Fredoka, sans-serif").set("textAlign", "center").set("textBaseline", "middle").set("fillStyle", "#ffffff").op("fillText", "BOOST", 0, 0.5).restore();
}
{
  const s = add(new Scene("text_emoji", "emoji runs (SPEC 6.2): hub labels, gate labels, floating text, all baselines, strokeText skips emoji", 280, 330));
  s.set("textAlign", "center").set("textBaseline", "middle").set("fillStyle", "#ffffff");
  const labels = [
    ["bold 13px Fredoka, sans-serif", "✨ ORBITAL ✨"], ["bold 13px Fredoka, sans-serif", "🦆 DUCKIE 🦆"], ["bold 13px Fredoka, sans-serif", "⚙️ GEAR RIG ⚙️"],
    ["bold 10px Fredoka, sans-serif", "⚡ ×4"], ["900 10.5px Fredoka, sans-serif", "💥 ×12!"], ["bold 10px Fredoka, sans-serif", "🧲 +8"], ["bold 9.5px Fredoka, sans-serif", "⏩ BOOST"],
  ];
  labels.forEach(([f, t], i) => { s.set("font", f).op("fillText", t, 140, 14 + i * 20); });
  [["alphabetic", 0], ["top", 1], ["bottom", 2], ["ideographic", 3], ["hanging", 4], ["middle", 5]].forEach(([b, i]) => {
    const y = 170 + (i % 3) * 22, x = 70 + Math.floor(i / 3) * 140;
    s.set("textBaseline", b).set("font", "bold 13px Fredoka, sans-serif").set("strokeStyle", "#ef4444").set("lineWidth", 0.5).line(x - 60, y, x + 60, y);
    s.set("textAlign", "center").op("fillText", "🍩 a" + b.slice(0, 3), x, y);
  });
  s.set("textBaseline", "alphabetic").set("textAlign", "left").set("font", "bold 13px Fredoka, sans-serif").op("fillText", "left 😋 run", 10, 250).set("textAlign", "right").op("fillText", "right ⭐ run", 270, 250);
  FONTS.length;
  [["NOM! 😋", 1.3, 0.9], ["⏪ TURBO!", 1.3, 0.5], ["×5! SHRED ⚡", 1.1, 1]].forEach(([t, sc, a], i) => {
    s.save().set("globalAlpha", a).op("translate", 140, 280 + i * 22).op("scale", sc, sc).set("font", "900 16px Fredoka, sans-serif").set("textAlign", "center").set("textBaseline", "alphabetic");
    s.set("strokeStyle", "#1e1b4b").set("lineWidth", 3.5).op("strokeText", t, 0, 0).set("fillStyle", "#fde047").op("fillText", t, 0, 0).restore();
  });
}

// ---------------------------------------------------------------- images
{
  const s = add(new Scene("drawimage", "drawImage 3/5/9-arg forms: upscale, downscale 64->13px (as the emoji), rotation, alpha, smoothing off, emoji sprites", 240, 160));
  s.images = { proc64: { type: "proc", size: 64 } };
  s.op("drawImage", { img: "proc64" }, 10, 10);
  s.op("drawImage", { img: "proc64" }, 80, 10, 40, 40);
  s.op("drawImage", { img: "proc64" }, 130.3, 10.6, 13, 13);
  s.op("drawImage", { img: "proc64" }, 150.5, 10.2, 21.7, 21.7);
  s.op("drawImage", { img: "proc64" }, 8, 8, 32, 32, 180, 10, 50, 30);
  s.save().op("translate", 40, 110).op("rotate", 0.5).set("globalAlpha", 0.6).op("drawImage", { img: "proc64" }, -20, -20, 40, 40).restore();
  s.save().set("imageSmoothingEnabled", false).op("drawImage", { img: "proc64" }, 80, 70, 40, 40).op("drawImage", { img: "proc64" }, 130, 70, 13, 13).restore();
  s.op("drawImage", { emoji: "🍩" }, 10, 125, 13, 13);
  s.op("drawImage", { emoji: "🍩" }, 30.4, 125.4, 26, 26);
  s.op("drawImage", { emoji: "😋" }, 70, 120, 39, 39);
  s.op("drawImage", { emoji: "⚡" }, 120, 128, 10, 10);
  s.save().op("translate", 160, 140).op("scale", 1.3, 1.3).op("drawImage", { emoji: "⭐" }, -8, -16, 16, 16).restore();
}

scenes.forEach((s) => s.write());
fs.writeFileSync(path.join(dir, "index.json"), JSON.stringify(scenes.map((s) => ({ name: s.name, desc: s.desc })), null, 1));
console.log("wrote", scenes.length, "scenes");
