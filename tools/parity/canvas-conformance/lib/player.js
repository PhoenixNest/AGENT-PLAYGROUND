// Replays a recorded stream or a hand-written scene on a 2D canvas (shared stream format, see README in scenes/).
(() => {
  const cc = (window.__cc = window.__cc || {});

  function proceduralImage(spec) {
    const n = spec.size || 64;
    const c = document.createElement("canvas");
    c.width = c.height = n;
    const x = c.getContext("2d");
    const d = x.createImageData(n, n);
    for (let j = 0; j < n; j++) {
      for (let i = 0; i < n; i++) {
        const o = (j * n + i) * 4;
        d.data[o] = Math.round((i * 255) / (n - 1));
        d.data[o + 1] = Math.round((j * 255) / (n - 1));
        d.data[o + 2] = (((i >> 3) ^ (j >> 3)) & 1) * 255;
        d.data[o + 3] = ((i + j) & 15) < 8 ? 255 : 128;
      }
    }
    x.putImageData(d, 0, 0);
    return c;
  }

  cc.replay = (stream, canvas) => {
    const meta = stream.meta;
    const dpr = meta.dpr ?? 3;
    canvas.width = Math.round(meta.width * dpr);
    canvas.height = Math.round(meta.height * dpr);
    canvas.style.width = meta.width + "px";
    canvas.style.height = meta.height + "px";
    const ctx = canvas.getContext("2d");
    const grads = new Map();
    const images = new Map();
    for (const [name, spec] of Object.entries(stream.images || {})) images.set(name, proceduralImage(spec));
    const resolve = (v) => {
      if (v && typeof v === "object" && !Array.isArray(v)) {
        if ("g" in v) return grads.get(v.g);
        if ("emoji" in v) return cc.emojiImages.get(v.emoji);
        if ("img" in v) return images.get(v.img);
      }
      return v;
    };
    const run = (op) => {
      const name = op[0];
      if (name[0] === "=") {
        ctx[name.slice(1)] = resolve(op[1]);
      } else if (name === "createLinearGradient" || name === "createRadialGradient") {
        grads.set(op[1], ctx[name](...op.slice(2)));
      } else if (name === "addColorStop") {
        grads.get(op[1]).addColorStop(op[2], op[3]);
      } else {
        ctx[name](...op.slice(1).map(resolve));
      }
    };
    for (const op of stream.preamble || []) run(op);
    for (const op of stream.ops) run(op);
  };
})();
