(() => {
  const cfg = window.__PARITY_CONFIG;
  delete window.__PARITY_CONFIG;

  const rawNow = performance.now.bind(performance);
  const perfOffset = rawNow();
  Object.defineProperty(performance, "now", {
    value: () => rawNow() - perfOffset,
    configurable: true,
    writable: true,
  });

  const origSetTimeout = window.setTimeout.bind(window);
  const origClearTimeout = window.clearTimeout.bind(window);
  const origSetInterval = window.setInterval.bind(window);
  const origAdd = EventTarget.prototype.addEventListener;
  const origRemove = EventTarget.prototype.removeEventListener;

  const P = { perfOffset, events: [], errors: [], emojiImgs: new Map() };
  Object.defineProperty(window, "__parity", { value: P });

  const round6 = (x) => Math.round(x * 1e6) / 1e6;
  const roundDeep = (v) => {
    if (typeof v === "number") return Number.isFinite(v) ? round6(v) : null;
    if (Array.isArray(v)) return v.map(roundDeep);
    if (v && typeof v === "object") {
      const o = {};
      for (const k of Object.keys(v)) o[k] = roundDeep(v[k]);
      return o;
    }
    return v;
  };
  const rec = (type, name, args) => {
    P.events.push({ t: performance.now(), type, name, args });
  };
  P.rec = rec;

  const registry = new Map();

  const diff = () => {
    if (!document.body) return;
    void getComputedStyle(document.body).opacity;
    const now = performance.now();
    for (const a of document.getAnimations()) {
      if (!registry.has(a)) {
        registry.set(a, { startFake: now, done: false });
        a.pause();
        a.currentTime = 0;
      }
    }
    for (const [a] of registry) if (a.playState === "idle") registry.delete(a);
  };

  P.diff = diff;
  P.sweep = () => {
    diff();
    const now = performance.now();
    for (const [a, r] of registry) {
      if (a.playState === "idle") {
        registry.delete(a);
        continue;
      }
      if (r.done) continue;
      const end = a.effect.getComputedTiming().endTime;
      const local = now - r.startFake;
      if (
        a instanceof CSSTransition &&
        a.playState === "finished" &&
        local < end
      ) {
        throw new Error(
          `animation-race: ${a.transitionProperty} finished at fake ${local} < ${end}`,
        );
      }
      if (Number.isFinite(end) && local >= end) {
        a.currentTime = end;
        a.pause();
        r.done = true;
      } else {
        a.currentTime = local;
        a.pause();
      }
    }
  };
  P.animationCount = () => registry.size;

  const frames = new Map();
  let frameSeq = 0;
  window.requestAnimationFrame = (cb) => {
    const id = ++frameSeq;
    const delay = 16 - (performance.now() % 16);
    const handle = origSetTimeout(() => {
      frames.delete(id);
      if (P.canvas) {
        P.canvas.last = P.canvas.cur;
        P.canvas.cur = [];
      }
      try {
        cb(performance.now());
      } finally {
        diff();
      }
    }, delay);
    frames.set(id, handle);
    return id;
  };
  window.cancelAnimationFrame = (id) => {
    const handle = frames.get(id);
    if (handle !== undefined) {
      origClearTimeout(handle);
      frames.delete(id);
    }
  };

  class ParityAudioParam {
    constructor(nodeId, name, initial) {
      this._nodeId = nodeId;
      this._name = name;
      this.value = initial;
    }
    _log(method, args) {
      rec(
        "audio",
        `param.${method}`,
        roundDeep([this._nodeId, this._name, ...args]),
      );
      return this;
    }
    setValueAtTime(v, t) {
      this.value = v;
      return this._log("setValueAtTime", [v, t]);
    }
    linearRampToValueAtTime(v, t) {
      return this._log("linearRampToValueAtTime", [v, t]);
    }
    exponentialRampToValueAtTime(v, t) {
      return this._log("exponentialRampToValueAtTime", [v, t]);
    }
    setTargetAtTime(v, t, c) {
      return this._log("setTargetAtTime", [v, t, c]);
    }
    cancelScheduledValues(t) {
      return this._log("cancelScheduledValues", [t]);
    }
  }

  class ParityAudioNode {
    constructor(ctx, id, kind) {
      this._ctx = ctx;
      this._id = id;
      this._kind = kind;
    }
    connect(target) {
      rec("audio", "node.connect", [this._id, target._id]);
      return target;
    }
    disconnect() {}
    _setProp(name, value) {
      rec("audio", "node.set", [this._id, name, value]);
    }
    start(when) {
      rec(
        "audio",
        "node.start",
        roundDeep([this._id, when === undefined ? 0 : when]),
      );
    }
    stop(when) {
      rec(
        "audio",
        "node.stop",
        roundDeep([this._id, when === undefined ? 0 : when]),
      );
    }
  }

  const typedNode = (kind, params) =>
    class extends ParityAudioNode {
      constructor(ctx, id) {
        super(ctx, id, kind);
        this._type = kind === "oscillator" ? "sine" : "lowpass";
        for (const [name, initial] of params)
          this[name] = new ParityAudioParam(id, name, initial);
      }
      get type() {
        return this._type;
      }
      set type(v) {
        this._type = v;
        this._setProp("type", v);
      }
    };

  const OscNode = typedNode("oscillator", [["frequency", 440]]);
  const BiquadNode = typedNode("biquad", [
    ["frequency", 350],
    ["Q", 1],
  ]);
  class GainNode extends ParityAudioNode {
    constructor(ctx, id) {
      super(ctx, id, "gain");
      this.gain = new ParityAudioParam(id, "gain", 1);
    }
  }
  class CompressorNode extends ParityAudioNode {
    constructor(ctx, id) {
      super(ctx, id, "compressor");
      this.threshold = new ParityAudioParam(id, "threshold", -24);
      this.knee = new ParityAudioParam(id, "knee", 30);
      this.ratio = new ParityAudioParam(id, "ratio", 12);
      this.attack = new ParityAudioParam(id, "attack", 0.003);
      this.release = new ParityAudioParam(id, "release", 0.25);
    }
  }
  class BufferSourceNode extends ParityAudioNode {
    constructor(ctx, id) {
      super(ctx, id, "bufferSource");
      this._buffer = null;
    }
    get buffer() {
      return this._buffer;
    }
    set buffer(b) {
      this._buffer = b;
      this._setProp("buffer", {
        length: b.length,
        sampleRate: b.sampleRate,
        channels: b.numberOfChannels,
      });
    }
  }
  class ParityAudioBuffer {
    constructor(channels, length, sampleRate) {
      this.numberOfChannels = channels;
      this.length = length;
      this.sampleRate = sampleRate;
      this.duration = length / sampleRate;
      this._data = Array.from(
        { length: channels },
        () => new Float32Array(length),
      );
    }
    getChannelData(c) {
      return this._data[c];
    }
  }

  class ParityAudioContext {
    constructor() {
      this.sampleRate = 44100;
      this.state = "suspended";
      this._created = performance.now();
      this._nodes = 0;
      this.destination = new ParityAudioNode(this, 0, "destination");
      rec("audio", "ctx.create", [44100]);
    }
    get currentTime() {
      return (performance.now() - this._created) / 1000;
    }
    resume() {
      this.state = "running";
      rec("audio", "ctx.resume", []);
      return Promise.resolve();
    }
    _make(Cls, kind) {
      const id = ++this._nodes;
      const node = new Cls(this, id);
      rec("audio", "node.create", [id, kind]);
      return node;
    }
    createOscillator() {
      return this._make(OscNode, "oscillator");
    }
    createGain() {
      return this._make(GainNode, "gain");
    }
    createBiquadFilter() {
      return this._make(BiquadNode, "biquad");
    }
    createBufferSource() {
      return this._make(BufferSourceNode, "bufferSource");
    }
    createDynamicsCompressor() {
      return this._make(CompressorNode, "compressor");
    }
    createBuffer(channels, length, sampleRate) {
      return new ParityAudioBuffer(channels, length, sampleRate);
    }
  }
  for (const name of [
    "createStereoPanner",
    "createDelay",
    "createConvolver",
    "createWaveShaper",
    "createAnalyser",
    "createPanner",
    "createChannelSplitter",
    "createChannelMerger",
    "createConstantSource",
    "createIIRFilter",
    "createPeriodicWave",
    "createScriptProcessor",
    "createMediaElementSource",
    "createMediaStreamSource",
    "createMediaStreamDestination",
  ]) {
    ParityAudioContext.prototype[name] = function () {
      throw new Error(`ParityAudioContext: unsupported ${name}`);
    };
  }
  window.AudioContext = ParityAudioContext;
  window.webkitAudioContext = ParityAudioContext;

  const mulberry32 = (a) => () => {
    a |= 0;
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
  let randomCalls = 0;
  const rng = mulberry32(0x5eed1234);
  Math.random = () => {
    randomCalls++;
    return rng();
  };
  P.randomCalls = () => randomCalls;

  window.setTimeout = function (cb, delay, ...args) {
    if (typeof cb !== "function") return origSetTimeout(cb, delay, ...args);
    return origSetTimeout(function () {
      rec("timer", "timeout", [delay === undefined ? 0 : delay]);
      try {
        return cb.apply(this, args);
      } finally {
        diff();
      }
    }, delay);
  };
  window.setInterval = function (cb, delay, ...args) {
    if (typeof cb !== "function") return origSetInterval(cb, delay, ...args);
    return origSetInterval(function () {
      rec("timer", "interval", [delay === undefined ? 0 : delay]);
      try {
        return cb.apply(this, args);
      } finally {
        diff();
      }
    }, delay);
  };

  const listenerWrappers = new WeakMap();
  const wrapKey = (type, options) =>
    `${type}|${typeof options === "boolean" ? options : !!(options && options.capture)}`;
  EventTarget.prototype.addEventListener = function (type, listener, options) {
    if (!listener) return origAdd.call(this, type, listener, options);
    let byKey = listenerWrappers.get(listener);
    if (!byKey) {
      byKey = new Map();
      listenerWrappers.set(listener, byKey);
    }
    const key = wrapKey(type, options);
    let wrapper = byKey.get(key);
    if (!wrapper) {
      if (typeof listener === "function") {
        wrapper = function (...a) {
          try {
            return listener.apply(this, a);
          } finally {
            diff();
          }
        };
      } else {
        wrapper = {
          handleEvent(e) {
            try {
              return listener.handleEvent(e);
            } finally {
              diff();
            }
          },
        };
      }
      byKey.set(key, wrapper);
    }
    return origAdd.call(this, type, wrapper, options);
  };
  EventTarget.prototype.removeEventListener = function (
    type,
    listener,
    options,
  ) {
    const byKey = listener && listenerWrappers.get(listener);
    const wrapper = byKey && byKey.get(wrapKey(type, options));
    return origRemove.call(this, type, wrapper || listener, options);
  };

  const segmenter = new Intl.Segmenter("en-US", { granularity: "grapheme" });
  const PICTO = /\p{Extended_Pictographic}/u;
  const EMOJI_PRES = /^\p{Emoji_Presentation}/u;
  const isEmojiGrapheme = (g) =>
    PICTO.test(g) && (g.includes("\uFE0F") || EMOJI_PRES.test(g));
  const emojiKey = (g) =>
    [...g]
      .map((c) => c.codePointAt(0))
      .filter((c) => c !== 0xfe0f)
      .map((c) => c.toString(16))
      .join("-");
  const emojiFiles = new Map(cfg.emoji.map(([key, file]) => [key, file]));
  const emojiFile = (g) => emojiFiles.get(emojiKey(g));
  const splitRuns = (s) => {
    const runs = [];
    let text = "";
    for (const { segment } of segmenter.segment(s)) {
      if (isEmojiGrapheme(segment)) {
        if (text) runs.push({ emoji: false, s: text });
        text = "";
        runs.push({ emoji: true, s: segment });
      } else text += segment;
    }
    if (text) runs.push({ emoji: false, s: text });
    return runs;
  };
  P.splitRuns = splitRuns;

  for (const [, file] of cfg.emoji) {
    const img = new Image();
    img.src = `/__parity/emoji/${file}`;
    P.emojiImgs.set(file, img);
  }

  const SKIP_PARENTS = new Set([
    "SCRIPT",
    "STYLE",
    "TITLE",
    "TEXTAREA",
    "NOSCRIPT",
    "OPTION",
  ]);
  const processTextNode = (node) => {
    const parent = node.parentNode;
    if (!parent || SKIP_PARENTS.has(parent.nodeName)) return;
    const s = node.nodeValue;
    if (!s || !PICTO.test(s)) return;
    const runs = splitRuns(s);
    if (!runs.some((r) => r.emoji)) return;
    const frag = document.createDocumentFragment();
    for (const r of runs) {
      if (!r.emoji) {
        frag.appendChild(document.createTextNode(r.s));
        continue;
      }
      const file = emojiFile(r.s);
      if (!file) {
        P.errors.push(`unmapped emoji ${JSON.stringify(r.s)}`);
        frag.appendChild(document.createTextNode(r.s));
        continue;
      }
      const img = document.createElement("img");
      img.className = "pe";
      img.src = `/__parity/emoji/${file}`;
      img.alt = r.s;
      frag.appendChild(img);
    }
    parent.replaceChild(frag, node);
  };
  const scanSubtree = (root) => {
    if (root.nodeType === 3) return processTextNode(root);
    if (root.nodeType !== 1 && root.nodeType !== 9 && root.nodeType !== 11)
      return;
    if (
      root.nodeType === 1 &&
      root.nodeName === "IMG" &&
      root.classList.contains("pe")
    )
      return;
    const texts = [];
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    while (walker.nextNode()) texts.push(walker.currentNode);
    for (const t of texts) processTextNode(t);
  };

  const textGetter = Object.getOwnPropertyDescriptor(
    Node.prototype,
    "textContent",
  );
  Object.defineProperty(Node.prototype, "textContent", {
    configurable: true,
    enumerable: textGetter.enumerable,
    set: textGetter.set,
    get() {
      if (this.nodeType === 9 || this.nodeType === 10) return null;
      if (this.nodeType !== 1 && this.nodeType !== 11)
        return textGetter.get.call(this);
      let out = "";
      const walk = (n) => {
        for (let c = n.firstChild; c; c = c.nextSibling) {
          if (c.nodeType === 3 || c.nodeType === 4) out += c.nodeValue;
          else if (c.nodeType === 1) {
            if (c.nodeName === "IMG" && c.classList.contains("pe"))
              out += c.alt;
            else walk(c);
          }
        }
      };
      walk(this);
      return out;
    },
  });

  const MODAL_IDS = [
    "modal-pause",
    "modal-privacy",
    "modal-language",
    "modal-reset-confirm",
    "modal-debrief",
    "modal-defeat",
  ];
  const modalActive = new Map();
  const checkModals = () => {
    for (const id of MODAL_IDS) {
      const el = document.getElementById(id);
      if (!el) continue;
      const on = el.classList.contains("active");
      if (modalActive.get(id) === undefined) {
        modalActive.set(id, on);
        if (on) rec("modal", id, ["open"]);
      } else if (modalActive.get(id) !== on) {
        modalActive.set(id, on);
        rec("modal", id, [on ? "open" : "close"]);
      }
    }
  };

  const observer = new MutationObserver((records) => {
    for (const r of records) {
      if (r.type === "childList") {
        for (const n of r.addedNodes) scanSubtree(n);
      } else if (r.type === "characterData") {
        processTextNode(r.target);
      }
    }
    checkModals();
  });
  observer.observe(document, {
    childList: true,
    subtree: true,
    characterData: true,
    attributes: true,
    attributeFilter: ["class"],
  });

  const CRC = CanvasRenderingContext2D.prototype;
  const fontDesc = Object.getOwnPropertyDescriptor(CRC, "font");
  const chainTail = '"Noto Sans Symbols 2", "Noto Sans Symbols", sans-serif';
  Object.defineProperty(CRC, "font", {
    configurable: true,
    enumerable: fontDesc.enumerable,
    get() {
      return fontDesc.get.call(this);
    },
    set(v) {
      fontDesc.set.call(
        this,
        typeof v === "string"
          ? v.replace(
              /\b(Fredoka|Nunito)\s*,\s*sans-serif\s*$/,
              `$1, ${chainTail}`,
            )
          : v,
      );
    },
  });

  const origFillText = CRC.fillText;
  const origStrokeText = CRC.strokeText;
  const origMeasure = CRC.measureText;
  const origDrawImage = CRC.drawImage;
  const parseFontPx = (font) => {
    const m = /(\d+(?:\.\d+)?)px/.exec(font);
    return m ? parseFloat(m[1]) : 16;
  };
  const EXPECT_EMOJI =
    /\p{Emoji_Presentation}|\p{Extended_Pictographic}\uFE0F/gu;
  const C = (P.canvas = { cur: [], last: [] });
  const logDraw = (ctx, isFill, s, images, flat) => {
    const m = s.match(EXPECT_EMOJI);
    C.cur.push({
      kind: isFill ? "fill" : "stroke",
      text: s,
      font: ctx.font,
      align: ctx.textAlign,
      baseline: ctx.textBaseline,
      expected: m ? m.length : 0,
      images,
      flat,
    });
  };
  const drawRuns = function (orig, isFill, text, x, y, rest) {
    const s = String(text);
    if (!PICTO.test(s)) {
      logDraw(this, isFill, s, [], []);
      return orig.call(this, s, x, y, ...rest);
    }
    const runs = splitRuns(s);
    const flat = runs
      .filter((r) => !r.emoji)
      .flatMap((r) => [...r.s].filter((c) => PICTO.test(c)));
    if (!runs.some((r) => r.emoji)) {
      logDraw(this, isFill, s, [], flat);
      return orig.call(this, s, x, y, ...rest);
    }
    const drawn = [];
    logDraw(this, isFill, s, drawn, flat);
    const fontPx = parseFontPx(this.font);
    let total = 0;
    for (const r of runs)
      total += r.emoji ? fontPx : origMeasure.call(this, r.s).width;
    const align = this.textAlign;
    let cursor = x;
    if (align === "center") cursor = x - total / 2;
    else if (align === "right" || align === "end") cursor = x - total;
    const baseline = this.textBaseline;
    this.textAlign = "left";
    try {
      for (const r of runs) {
        if (!r.emoji) {
          orig.call(this, r.s, cursor, y, ...rest);
          cursor += origMeasure.call(this, r.s).width;
          continue;
        }
        if (isFill) {
          const file = emojiFile(r.s);
          const img = file && P.emojiImgs.get(file);
          if (!img || !img.complete || !img.naturalWidth) {
            P.errors.push(`canvas emoji not ready ${JSON.stringify(r.s)}`);
          } else {
            let top;
            if (baseline === "top" || baseline === "hanging") top = y;
            else if (baseline === "middle") top = y - fontPx / 2;
            else top = y + 0.125 * fontPx - fontPx;
            origDrawImage.call(this, img, cursor, top, fontPx, fontPx);
            const t = this.getTransform();
            drawn.push({
              grapheme: r.s,
              file,
              x: cursor,
              y: top,
              size: fontPx,
              m: [t.a, t.b, t.c, t.d, t.e, t.f],
            });
          }
        }
        cursor += fontPx;
      }
    } finally {
      this.textAlign = align;
    }
  };
  CRC.fillText = function (text, x, y, ...rest) {
    return drawRuns.call(this, origFillText, true, text, x, y, rest);
  };
  CRC.strokeText = function (text, x, y, ...rest) {
    return drawRuns.call(this, origStrokeText, false, text, x, y, rest);
  };

  Object.defineProperty(navigator, "vibrate", {
    configurable: true,
    value: (ms) => {
      rec("vibrate", "vibrate", [Array.isArray(ms) ? ms[0] : ms]);
      return true;
    },
  });

  let appRef;
  Object.defineProperty(window, "app", {
    configurable: true,
    get: () => appRef,
    set(app) {
      appRef = app;
      instrumentApp(app);
    },
  });

  const instrumentApp = (app) => {
    const showPage = app.showPage;
    app.showPage = function (id) {
      rec("page", id, []);
      return showPage.call(this, id);
    };
    const showToast = app.showToast;
    app.showToast = function (msg) {
      rec("toast", "show", [msg]);
      return showToast.call(this, msg);
    };
    const updateCredits = app.updateAllCreditDisplays;
    let lastCredits;
    app.updateAllCreditDisplays = function () {
      const value = this.economy.profile.credits;
      if (value !== lastCredits) {
        lastCredits = value;
        rec("credits", "set", [value]);
      }
      return updateCredits.call(this);
    };
    const audio = app.audio;
    const proto = Object.getPrototypeOf(audio);
    for (const name of Object.getOwnPropertyNames(proto)) {
      if (
        name === "constructor" ||
        name.startsWith("_") ||
        typeof proto[name] !== "function"
      )
        continue;
      const fn = proto[name];
      audio[name] = function (...args) {
        rec("audio", name, roundDeep(JSON.parse(JSON.stringify(args))));
        return fn.apply(this, args);
      };
    }
    checkModals();
  };

  origAdd.call(document, "DOMContentLoaded", () => {
    const style = document.createElement("style");
    style.id = "parity-overrides";
    style.textContent = cfg.overridesCss;
    document.head.appendChild(style);
  });

  P.waitEmoji = async () => {
    await Promise.all(
      [...P.emojiImgs.values()].map((img) =>
        img.decode().catch(() => {
          P.errors.push(`emoji decode failed ${img.src}`);
        }),
      ),
    );
    const pending = [...document.querySelectorAll("img.pe")].filter(
      (i) => !i.complete || !i.naturalWidth,
    );
    await Promise.all(pending.map((i) => i.decode().catch(() => {})));
    const still = [...document.querySelectorAll("img.pe")].filter(
      (i) => !i.complete || !i.naturalWidth,
    );
    if (still.length) throw new Error(`emoji not loaded: ${still.length}`);
  };

  P.verifyFonts = async (table) => {
    await document.fonts.ready;
    const faces = [...document.fonts];
    await Promise.all(faces.map((f) => f.load()));
    const report = faces.map((f) => ({
      family: f.family.replace(/"/g, ""),
      weight: f.weight,
      status: f.status,
    }));
    const effective = [];
    for (const [family, weight, expected] of table) {
      const loaded = await document.fonts.load(`${weight} 16px ${family}`);
      effective.push({
        family,
        weight,
        expected,
        got: loaded.map((f) => f.weight),
        ok: loaded.length === 1 && loaded[0].weight === String(expected),
      });
    }
    return {
      report,
      effective,
      sheetLast:
        document.styleSheets[document.styleSheets.length - 1].ownerNode?.id,
    };
  };

  const visibleText = (n) => {
    let out = "";
    const walk = (e) => {
      for (let c = e.firstChild; c; c = c.nextSibling) {
        if (c.nodeType === 3) out += c.nodeValue;
        else if (c.nodeType === 1) {
          if (c.nodeName === "IMG" && c.classList.contains("pe")) out += c.alt;
          else walk(c);
        }
      }
    };
    walk(n);
    return out.replace(/\s+/g, " ").trim().slice(0, 200);
  };

  P.layout = (tracked) => {
    const trackedSet = new Set(tracked);
    const counters = new Map();
    const elements = [];
    const walker = document.createTreeWalker(
      document.body,
      NodeFilter.SHOW_ELEMENT,
    );
    while (walker.nextNode()) {
      const n = walker.currentNode;
      if (n.nodeName === "IMG" && n.classList.contains("pe")) continue;
      const classes = (n.getAttribute("class") || "")
        .split(/\s+/)
        .filter(Boolean);
      const ordinals = new Map();
      for (const c of new Set(classes)) {
        if (!trackedSet.has(c)) continue;
        const ordinal = (counters.get(c) || 0) + 1;
        counters.set(c, ordinal);
        ordinals.set(c, ordinal);
      }
      const firstTracked = classes.find((c) => trackedSet.has(c));
      if (!n.id && !firstTracked) continue;
      const key = n.id || `${firstTracked}#${ordinals.get(firstTracked)}`;
      const r = n.getBoundingClientRect();
      const visible =
        n.checkVisibility({ visibilityProperty: true }) &&
        r.width > 0 &&
        r.height > 0;
      elements.push({
        id: key,
        tag: n.tagName.toLowerCase(),
        classes,
        rect: { x: r.x, y: r.y, w: r.width, h: r.height },
        visible,
        opacity: parseFloat(getComputedStyle(n).opacity),
        text: visibleText(n),
      });
    }
    return elements;
  };

  P.canvasCheck = () => {
    const frame = P.canvas.cur;
    const bad = [];
    let emojiExpected = 0;
    let emojiDrawn = 0;
    for (const c of frame) {
      const drawn = c.kind === "fill" ? c.images.length : 0;
      emojiExpected += c.kind === "fill" ? c.expected : 0;
      emojiDrawn += drawn;
      if (c.kind === "stroke" && c.images.length)
        bad.push(`strokeText drew an emoji image: ${c.text}`);
      if (c.kind === "fill" && drawn !== c.expected) {
        bad.push(
          `fillText ${JSON.stringify(c.text)}: ${c.expected} emoji in string, ${drawn} images drawn`,
        );
      }
      if (c.flat.length)
        bad.push(
          `${c.kind}Text ${JSON.stringify(c.text)} left pictographs to the font: ${c.flat.join("")}`,
        );
    }
    if (emojiExpected > 0 && emojiDrawn === 0)
      bad.push("frame has emoji strings but no emoji image was drawn");
    const graphemes = [
      ...new Set(
        frame.flatMap((c) =>
          c.kind === "fill" ? c.images.map((i) => i.grapheme) : [],
        ),
      ),
    ];
    const floating =
      window.app && window.app.renderer && window.app.renderer.floatingTexts
        ? window.app.renderer.floatingTexts.map((f) => f.text)
        : [];
    return {
      page: window.app && window.app.currentPage,
      calls: frame.length,
      emojiExpected,
      emojiDrawn,
      graphemes,
      floating,
      bad,
    };
  };

  // Game-page rAF log for Unity lockstep. Installed only when app-flows set the flag
  // before this script (clock init already replaced requestAnimationFrame).
  const stateCode = (state) =>
    state === "IN_RING" ? 0 : state === "FALLING" ? 1 : state === "INGESTING" ? 2 : 3;
  const typeCode = (type) =>
    type === "titanium" ? 0 : type === "solar" ? 1 : type === "volatile" ? 2 : type === "magnetic" ? 3 : 4;
  const capturePhysics = (p) => {
    const v = [
      p.ringAngle, p.angularVelocity, p.innerRingAngle, p.rcsBoostTimer, p.lastRatchetAngle,
      p.rcsThrusting ? 1 : 0, p.rcsDirection, p.nextDebrisId, p.debrisList.length,
      p.oneWayFlaps.length, p.deflectorPins.length, p.gates.length,
    ];
    for (const d of p.debrisList) {
      v.push(d.id, d.x, d.y, d.vx, d.vy, d.radius, d.mass, d.weight, stateCode(d.state), d.ingestProgress, d.gateCooldown, typeCode(d.type));
    }
    for (const f of p.oneWayFlaps) v.push(f.swing, f.swingVel);
    for (const pin of p.deflectorPins) v.push(pin.hitTimer);
    for (const g of p.gates) {
      const ids = [...(g.processedDebris || [])].sort((a, b) => a - b);
      v.push(ids.length);
      for (const id of ids) v.push(id);
    }
    return v;
  };
  const hashDoubles = (values) => {
    let h = 14695981039346656037n;
    const prime = 1099511628211n;
    const mask = 0xffffffffffffffffn;
    const buf = new ArrayBuffer(8);
    const view = new DataView(buf);
    const bytes = new Uint8Array(buf);
    for (const d of values) {
      view.setFloat64(0, Number(d), true);
      for (let i = 0; i < 8; i++) {
        h ^= BigInt(bytes[i]);
        h = (h * prime) & mask;
      }
    }
    return h.toString(16).padStart(16, "0");
  };
  P.takePlay = () => {
    const bucket = window.__PARITY_GAME_SYNC;
    if (!bucket) return null;
    const frames = bucket.frames.splice(0, bucket.frames.length);
    const inputs = bucket.inputs.splice(0, bucket.inputs.length);
    const physics = window.app && window.app.physics;
    let physicsTicks = 0;
    for (const frame of frames) if (frame.sim) physicsTicks++;
    return {
      frames,
      inputs,
      hash: physics ? hashDoubles(capturePhysics(physics)) : null,
      physicsTicks,
      renderFrames: frames.length,
    };
  };
  const bucket = window.__PARITY_GAME_SYNC;
  if (bucket) {
    bucket.frames = bucket.frames || [];
    bucket.inputs = bucket.inputs || [];
    const origRaf = window.requestAnimationFrame;
    window.requestAnimationFrame = (cb) => origRaf((t) => {
      const app = window.app;
      const onGame = !!(app && app.currentPage === "game");
      const paused = !!(app && app.isPaused);
      const prev = bucket.lastT;
      bucket.lastT = t;
      cb(t);
      if (!onGame || prev == null) return;
      bucket.frames.push({ dt: t - prev, sim: !paused });
    });
    const note = (input) => {
      input.at = bucket.frames.length;
      bucket.inputs.push(input);
    };
    window.addEventListener("pointerdown", (e) => {
      if (!window.app || window.app.currentPage !== "game") return;
      const canvas = document.getElementById("game-canvas");
      if (!canvas || (e.target !== canvas && !canvas.contains(e.target))) return;
      const r = canvas.getBoundingClientRect();
      note({ kind: "down", x: e.clientX, y: e.clientY, left: r.left, width: r.width });
    }, true);
    window.addEventListener("pointermove", (e) => {
      if (!window.app || window.app.currentPage !== "game" || !window.app.isDragging) return;
      const canvas = document.getElementById("game-canvas");
      if (!canvas) return;
      note({ kind: "move", x: e.clientX, y: e.clientY });
    }, true);
    window.addEventListener("pointerup", () => {
      if (!window.app || window.app.currentPage !== "game") return;
      if (!bucket.inputs.length || bucket.inputs[bucket.inputs.length - 1].kind === "up") return;
      note({ kind: "up" });
    }, true);
    window.addEventListener("keydown", (e) => {
      if (!window.app || window.app.currentPage !== "game") return;
      const tag = e.target && e.target.tagName;
      if (tag === "INPUT" || tag === "TEXTAREA") return;
      note({ kind: "key", key: e.key });
    }, true);
  }

  P.assertBooted = () => ({
    lastTime: window.app && window.app.lastTime,
    perf: performance.now(),
    page: window.app && window.app.currentPage,
  });
})();
