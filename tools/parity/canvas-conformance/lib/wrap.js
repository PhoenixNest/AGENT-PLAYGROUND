// Browser-side helpers shared by record.mjs (real app) and replay.html (synthetic scenes and stream replay).
// Layer 1 (installParity) is the SPEC section 5.2 / 6.2 behaviour: font-chain rewrite and emoji substitution.
// Layer 2 (installRecorder) serialises the application-level call stream on top of layer 1.
(() => {
  const proto = CanvasRenderingContext2D.prototype;
  const cc = (window.__cc = window.__cc || {});

  const METHODS = [
    "save", "restore", "scale", "rotate", "translate", "transform", "setTransform", "resetTransform",
    "createLinearGradient", "createRadialGradient", "createConicGradient", "createPattern",
    "clearRect", "fillRect", "strokeRect", "beginPath", "fill", "stroke", "clip", "isPointInPath",
    "fillText", "strokeText", "drawImage", "setLineDash", "moveTo", "lineTo", "closePath", "rect",
    "roundRect", "arc", "arcTo", "ellipse", "quadraticCurveTo", "bezierCurveTo",
  ];
  const PROPS = [
    "fillStyle", "strokeStyle", "lineWidth", "lineCap", "lineJoin", "miterLimit", "lineDashOffset",
    "shadowBlur", "shadowColor", "shadowOffsetX", "shadowOffsetY", "globalAlpha",
    "globalCompositeOperation", "font", "textAlign", "textBaseline", "imageSmoothingEnabled",
    "imageSmoothingQuality", "direction", "fontKerning",
  ];

  const native = {};
  for (const m of METHODS) native[m] = proto[m];
  const nativeProps = {};
  for (const p of PROPS) nativeProps[p] = Object.getOwnPropertyDescriptor(proto, p);
  const nativeMeasure = proto.measureText;
  const nativeAddColorStop = CanvasGradient.prototype.addColorStop;
  cc.native = native;

  // ------------------------------------------------------------------ emoji
  const emojiImages = new Map();
  cc.emojiImages = emojiImages;
  const segmenter = new Intl.Segmenter("en", { granularity: "grapheme" });
  const reExt = /\p{Extended_Pictographic}/u;
  const reEmojiPres = /^\p{Emoji_Presentation}/u;
  function isEmoji(g) {
    if (!reExt.test(g)) return false;
    return g.includes("\uFE0F") || reEmojiPres.test(g);
  }
  cc.isEmoji = isEmoji;
  function splitRuns(s) {
    const runs = [];
    let cur = "";
    for (const { segment } of segmenter.segment(s)) {
      if (isEmoji(segment)) {
        if (cur) runs.push({ text: cur });
        cur = "";
        runs.push({ emoji: segment });
      } else cur += segment;
    }
    if (cur) runs.push({ text: cur });
    return runs;
  }
  cc.splitRuns = splitRuns;

  cc.preloadEmoji = async (index, base = "/__parity/emoji/") => {
    const jobs = [];
    for (const [g, v] of Object.entries(index)) {
      const img = new Image();
      img.src = base + v.file;
      emojiImages.set(g, img);
      jobs.push(img.decode());
    }
    await Promise.all(jobs);
  };

  const withNative = (ctx, p, v) => nativeProps[p].set.call(ctx, v);
  const readNative = (ctx, p) => nativeProps[p].get.call(ctx);

  function parityText(kind, ctx, s, x, y, maxWidth) {
    s = String(s);
    const runs = splitRuns(s);
    if (!runs.some((r) => r.emoji)) return native[kind].call(ctx, s, x, y);
    const fontPx = parseFloat(/(\d+(?:\.\d+)?)px/.exec(readNative(ctx, "font"))[1]);
    let total = 0;
    for (const r of runs) total += r.emoji ? fontPx : nativeMeasure.call(ctx, r.text).width;
    const align = readNative(ctx, "textAlign");
    const baseline = readNative(ctx, "textBaseline");
    let cx = x;
    if (align === "center") cx = x - total / 2;
    else if (align === "right" || align === "end") cx = x - total;
    for (const r of runs) {
      if (r.emoji) {
        if (kind === "fillText") {
          const img = emojiImages.get(r.emoji);
          if (!img) throw new Error("emoji image missing: " + r.emoji);
          let top;
          if (baseline === "top") top = y;
          else if (baseline === "middle") top = y - fontPx / 2;
          else top = y + 0.125 * fontPx - fontPx;
          native.drawImage.call(ctx, img, cx, top, fontPx, fontPx);
        }
        cx += fontPx;
      } else {
        withNative(ctx, "textAlign", "left");
        native[kind].call(ctx, r.text, cx, y);
        withNative(ctx, "textAlign", align);
        cx += nativeMeasure.call(ctx, r.text).width;
      }
    }
  }

  cc.installParity = () => {
    proto.fillText = function (s, x, y, mw) { return parityText("fillText", this, s, x, y, mw); };
    proto.strokeText = function (s, x, y, mw) { return parityText("strokeText", this, s, x, y, mw); };
    Object.defineProperty(proto, "font", {
      configurable: true,
      enumerable: true,
      get() { return nativeProps.font.get.call(this); },
      set(v) {
        let f = String(v);
        if (/(Fredoka|Nunito)/.test(f)) f = f.replace(/,\s*sans-serif\s*$/, ', "Noto Sans Symbols 2", "Noto Sans Symbols", sans-serif');
        nativeProps.font.set.call(this, f);
      },
    });
  };

  // ------------------------------------------------------------------ recorder
  let rec = null;
  const gradIds = new WeakMap();
  let gradCount = 0;
  const imgNames = new WeakMap();
  cc.imageName = (img, name) => imgNames.set(img, name);

  const ser = (v) => {
    if (typeof v === "number") return Number.isFinite(v) ? v : String(v);
    if (v && typeof v === "object") {
      if (gradIds.has(v)) return { g: gradIds.get(v) };
      if (imgNames.has(v)) return { img: imgNames.get(v) };
      if (v instanceof HTMLImageElement) {
        for (const [g, e] of emojiImages) if (e === v) return { emoji: g };
        const src = String(v.currentSrc || v.src || "");
        const m = /([0-9a-f]{4,}(?:-[0-9a-f]{4,})*)\./i.exec(src);
        if (m && cc.emojiByFile) {
          const g = cc.emojiByFile[m[1].toLowerCase()];
          if (g) return { emoji: g };
        }
      }
      if (Array.isArray(v)) return v.map(ser);
      if (typeof v.x === "number") return { x: v.x, y: v.y, w: v.width, h: v.height };
    }
    return v;
  };

  cc.installRecorder = () => {
    for (const m of METHODS) {
      const inner = proto[m];
      proto[m] = function (...a) {
        if (rec && rec.ctx === this) {
          if (m === "createLinearGradient" || m === "createRadialGradient") {
            const g = inner.apply(this, a);
            const id = gradCount++;
            gradIds.set(g, id);
            rec.ops.push([m, id, ...a.map(ser)]);
            return g;
          }
          rec.ops.push([m, ...a.map(ser)]);
        }
        return inner.apply(this, a);
      };
    }
    for (const p of PROPS) {
      const d = Object.getOwnPropertyDescriptor(proto, p);
      Object.defineProperty(proto, p, {
        configurable: true,
        enumerable: true,
        get() { return d.get.call(this); },
        set(v) {
          if (rec && rec.ctx === this) rec.ops.push(["=" + p, ser(v)]);
          d.set.call(this, v);
        },
      });
    }
    CanvasGradient.prototype.addColorStop = function (o, c) {
      if (rec && gradIds.has(this)) rec.ops.push(["addColorStop", gradIds.get(this), o, c]);
      return nativeAddColorStop.call(this, o, c);
    };
  };

  cc.snapshotState = (ctx) => {
    const ops = [];
    const t = ctx.getTransform();
    ops.push(["setTransform", t.a, t.b, t.c, t.d, t.e, t.f]);
    for (const p of PROPS) {
      const v = ctx[p];
      if (typeof v === "string" || typeof v === "number" || typeof v === "boolean") ops.push(["=" + p, v]);
    }
    ops.push(["setLineDash", ctx.getLineDash()]);
    return ops;
  };

  cc.startRecording = (ctx) => {
    gradCount = 0;
    rec = { ctx, ops: [] };
  };
  cc.stopRecording = () => {
    const r = rec;
    rec = null;
    return r ? r.ops : null;
  };
  cc.isRecording = () => !!rec;
})();
