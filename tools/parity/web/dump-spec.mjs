import { mkdirSync, writeFileSync } from "node:fs";
import { isAbsolute, join, resolve } from "node:path";
import { ROOT, Session, launchBrowser } from "./harness.mjs";

const args = process.argv.slice(2);
let outArg = "out/web";
let only = null;
for (let i = 0; i < args.length; i++) {
  if (args[i] === "--out") outArg = args[++i];
  else if (args[i] === "--only") only = args[++i].split(",");
  else {
    console.error(`unknown argument ${args[i]}`);
    process.exit(2);
  }
}
const outDir = join(
  isAbsolute(outArg) ? outArg : resolve(ROOT, outArg),
  "spec",
);
mkdirSync(outDir, { recursive: true });

const TODAY = "2026-10-01";
const adv = (ms) => ({ op: "advance", ms });
const click = (target) => ({ op: "click", target });
const sel = (selector) => ({ op: "clickSelector", selector });
const prof = (path, value) => ({ op: "setProfile", path, value });
const app = (field, value) => ({ op: "setApp", field, value });
const call = (method, ...a) => ({ op: "call", method, args: a });
const gameStart = (index) => [
  { op: "loadSector", index },
  { op: "showPage", page: "game" },
  call("dismissGestureHint"),
  adv(400),
];
const done = (tiers) => [
  prof("lastDailyShiftDate", TODAY),
  prof("completedDailyTiers", tiers),
];

const ENTRIES = [
  {
    name: "loading",
    root: "#page-loading",
    steps: [{ op: "boot", completeLoading: false }],
  },
  { name: "home", root: "#page-home", steps: [{ op: "boot" }] },
  {
    name: "home_badges_ready",
    root: "#page-home",
    steps: [
      { op: "boot" },
      ...done(["bronze", "silver", "gold"]),
      call("updateHomeView"),
      adv(300),
    ],
  },
  {
    name: "home_badges_claimed",
    root: "#page-home",
    steps: [
      { op: "boot" },
      ...done(["bronze", "silver", "gold"]),
      prof("dailyMasterClaimedDate", TODAY),
      prof("lastDailyGiftDate", TODAY),
      call("updateHomeView"),
      adv(300),
    ],
  },
  {
    name: "game",
    root: "#page-game",
    steps: [
      { op: "boot" },
      { op: "loadSector", index: 0 },
      { op: "showPage", page: "game" },
      adv(16),
    ],
  },
  {
    name: "game_hint_hidden",
    root: "#page-game",
    steps: [{ op: "boot" }, ...gameStart(0)],
  },
  {
    name: "maps",
    root: "#page-maps",
    steps: [
      { op: "boot" },
      prof("completedSectors", ["level_1", "level_2", "level_3", "level_5"]),
      { op: "loadSector", index: 3 },
      click("btn-open-maps-page"),
      adv(240),
    ],
  },
  {
    name: "daily",
    root: "#page-daily",
    steps: [{ op: "boot" }, click("btn-open-daily-shift"), adv(400)],
  },
  {
    name: "daily_bronze_cleared",
    root: "#page-daily",
    steps: [
      { op: "boot" },
      ...done(["bronze"]),
      click("btn-open-daily-shift"),
      adv(400),
    ],
  },
  {
    name: "daily_master_ready",
    root: "#page-daily",
    steps: [
      { op: "boot" },
      ...done(["bronze", "silver", "gold"]),
      click("btn-open-daily-shift"),
      adv(400),
    ],
  },
  {
    name: "daily_master_claimed",
    root: "#page-daily",
    steps: [
      { op: "boot" },
      ...done(["bronze", "silver", "gold"]),
      prof("dailyMasterClaimedDate", TODAY),
      click("btn-open-daily-shift"),
      adv(400),
    ],
  },
  {
    name: "shop",
    root: "#page-shop",
    steps: [{ op: "boot" }, click("btn-open-shop-page"), adv(400)],
  },
  {
    name: "shop_singularities",
    root: "#page-shop",
    steps: [
      { op: "boot" },
      prof("unlocked.singularities", [
        "singularity_nibbles",
        "singularity_strawberry",
      ]),
      prof("equipped.singularity", "singularity_strawberry"),
      click("btn-open-shop-page"),
      adv(400),
      sel(".tab-btn[data-tab=singularities]"),
      adv(150),
    ],
  },
  {
    name: "shop_skyboxes",
    root: "#page-shop",
    steps: [
      { op: "boot" },
      click("btn-open-shop-page"),
      adv(400),
      sel(".tab-btn[data-tab=skyboxes]"),
      adv(150),
    ],
  },
  {
    name: "shop_gift_claimed",
    root: "#page-shop",
    steps: [
      { op: "boot" },
      prof("lastDailyGiftDate", TODAY),
      click("btn-open-shop-page"),
      adv(400),
    ],
  },
  {
    name: "achievements",
    root: "#page-achievements",
    steps: [
      { op: "boot" },
      prof("achievements", ["ach_first_drop", "ach_cleaner_50"]),
      click("btn-open-trophies-page"),
      adv(240),
    ],
  },
  {
    name: "settings",
    root: "#page-settings",
    steps: [{ op: "boot" }, click("btn-home-settings"), adv(240)],
  },
  {
    name: "modal_pause",
    root: "#modal-pause",
    steps: [{ op: "boot" }, ...gameStart(0), click("btn-game-pause"), adv(300)],
  },
  {
    name: "modal_privacy",
    root: "#modal-privacy",
    steps: [
      { op: "boot" },
      click("btn-home-settings"),
      adv(240),
      click("btn-open-privacy"),
      adv(300),
    ],
  },
  {
    name: "modal_reset_confirm",
    root: "#modal-reset-confirm",
    steps: [
      { op: "boot" },
      click("btn-home-settings"),
      adv(240),
      click("btn-reset-game-data"),
      adv(300),
    ],
  },
  {
    name: "modal_debrief",
    root: "#modal-debrief",
    steps: [
      { op: "boot" },
      ...gameStart(0),
      app("collectedMass", 33),
      call("handleLevelVictory"),
      adv(900),
    ],
  },
  {
    name: "modal_defeat",
    root: "#modal-defeat",
    steps: [
      { op: "boot" },
      ...gameStart(0),
      app("collectedMass", 15),
      app("spilledMass", 4),
      call("handleLevelFailure"),
      adv(700),
    ],
  },
  {
    name: "toast",
    root: "#toast-container",
    steps: [{ op: "boot" }, call("showToast", "Item Equipped! ✨"), adv(1000)],
  },
];

const STYLE_PROPS = [
  "display",
  "position",
  "top",
  "right",
  "bottom",
  "left",
  "z-index",
  "float",
  "clear",
  "box-sizing",
  "width",
  "height",
  "min-width",
  "min-height",
  "max-width",
  "max-height",
  "margin-top",
  "margin-right",
  "margin-bottom",
  "margin-left",
  "padding-top",
  "padding-right",
  "padding-bottom",
  "padding-left",
  "border-top-width",
  "border-right-width",
  "border-bottom-width",
  "border-left-width",
  "border-top-style",
  "border-right-style",
  "border-bottom-style",
  "border-left-style",
  "border-top-color",
  "border-right-color",
  "border-bottom-color",
  "border-left-color",
  "border-top-left-radius",
  "border-top-right-radius",
  "border-bottom-right-radius",
  "border-bottom-left-radius",
  "background-color",
  "background-image",
  "background-position",
  "background-size",
  "background-repeat",
  "background-clip",
  "background-origin",
  "background-attachment",
  "-webkit-background-clip",
  "box-shadow",
  "opacity",
  "transform",
  "transform-origin",
  "transform-style",
  "translate",
  "rotate",
  "scale",
  "filter",
  "backdrop-filter",
  "clip-path",
  "mix-blend-mode",
  "isolation",
  "overflow-x",
  "overflow-y",
  "visibility",
  "pointer-events",
  "cursor",
  "color",
  "font-family",
  "font-size",
  "font-weight",
  "font-style",
  "font-variant",
  "line-height",
  "letter-spacing",
  "word-spacing",
  "text-align",
  "text-transform",
  "text-decoration-line",
  "text-decoration-color",
  "text-indent",
  "text-overflow",
  "text-shadow",
  "white-space",
  "word-break",
  "overflow-wrap",
  "vertical-align",
  "-webkit-text-stroke-width",
  "-webkit-text-stroke-color",
  "-webkit-text-fill-color",
  "perspective",
  "perspective-origin",
  "backface-visibility",
  "flex-direction",
  "flex-wrap",
  "flex-grow",
  "flex-shrink",
  "flex-basis",
  "order",
  "align-items",
  "align-self",
  "align-content",
  "justify-content",
  "justify-items",
  "justify-self",
  "row-gap",
  "column-gap",
  "grid-template-columns",
  "grid-template-rows",
  "grid-column-start",
  "grid-column-end",
  "grid-row-start",
  "grid-row-end",
  "grid-auto-flow",
  "object-fit",
  "appearance",
  "outline-width",
  "outline-style",
  "outline-color",
  "outline-offset",
  "list-style-type",
  "content",
  "animation-name",
  "animation-duration",
  "animation-timing-function",
  "animation-delay",
  "animation-iteration-count",
  "animation-direction",
  "animation-fill-mode",
  "animation-play-state",
  "transition-property",
  "transition-duration",
  "transition-timing-function",
  "transition-delay",
];

function pageDump({ rootSel, props }) {
  const root = document.querySelector(rootSel);
  if (!root) return { error: `missing ${rootSel}` };

  const PSEUDO_RE =
    /:(hover|active|disabled|focus-visible|focus-within|focus|checked)\b/g;
  const topLevelSplit = (text) => {
    const parts = [];
    let depth = 0;
    let cur = "";
    for (const ch of text) {
      if (ch === "(" || ch === "[") depth++;
      if (ch === ")" || ch === "]") depth--;
      if (ch === "," && depth === 0) {
        parts.push(cur.trim());
        cur = "";
      } else cur += ch;
    }
    if (cur.trim()) parts.push(cur.trim());
    return parts;
  };
  const stateRules = [];
  const walkRules = (rules) => {
    for (const rule of rules) {
      if (rule.type === CSSRule.STYLE_RULE) {
        for (const part of topLevelSplit(rule.selectorText)) {
          const states = [...part.matchAll(PSEUDO_RE)].map((m) => `:${m[1]}`);
          if (!states.length) continue;
          const pe =
            /::?(before|after|-webkit-slider-thumb|-webkit-slider-runnable-track)\s*$/.exec(
              part,
            );
          let stripped = part.replace(PSEUDO_RE, "");
          if (pe)
            stripped = stripped.replace(
              /::?(before|after|-webkit-slider-thumb|-webkit-slider-runnable-track)\s*$/,
              "",
            );
          stripped = stripped.trim();
          const lastCompound = part.split(/[\s>+~]+/).pop();
          const onSelf = new RegExp(PSEUDO_RE.source).test(lastCompound);
          const decls = {};
          for (let i = 0; i < rule.style.length; i++) {
            const p = rule.style[i];
            decls[p] =
              rule.style.getPropertyValue(p) +
              (rule.style.getPropertyPriority(p) ? " !important" : "");
          }
          stateRules.push({
            selector: part,
            states: [...new Set(states)],
            on: onSelf ? "self" : "ancestor",
            pseudoElement: pe ? pe[1] : null,
            stripped,
            cssText: rule.style.cssText,
            declarations: decls,
          });
        }
      } else if (rule.type === CSSRule.MEDIA_RULE) {
        if (window.matchMedia(rule.conditionText).matches)
          walkRules(rule.cssRules);
      } else if (rule.cssRules && rule.type !== CSSRule.KEYFRAMES_RULE) {
        walkRules(rule.cssRules);
      }
    }
  };
  for (const sheet of document.styleSheets) {
    try {
      walkRules(sheet.cssRules);
    } catch {}
  }

  const styleOf = (cs) => {
    const o = {};
    for (const p of props) o[p] = cs.getPropertyValue(p);
    return o;
  };

  const elements = [];
  const indexOf = new Map();
  const walk = (el, parentIndex) => {
    if (el.nodeName === "IMG" && el.classList.contains("pe")) {
      const r = el.getBoundingClientRect();
      const idx = elements.length;
      indexOf.set(el, idx);
      elements.push({
        index: idx,
        parent: parentIndex,
        tag: "img",
        id: "",
        classes: ["pe"],
        emoji: el.alt,
        file: el.getAttribute("src"),
        rect: { x: r.x, y: r.y, w: r.width, h: r.height },
      });
      return;
    }
    const cs = getComputedStyle(el);
    const r = el.getBoundingClientRect();
    const idx = elements.length;
    indexOf.set(el, idx);
    const entry = {
      index: idx,
      parent: parentIndex,
      tag: el.tagName.toLowerCase(),
      id: el.id,
      classes: (el.getAttribute("class") || "").split(/\s+/).filter(Boolean),
      attrs: {},
      inlineStyle: el.getAttribute("style") || "",
      rect: { x: r.x, y: r.y, w: r.width, h: r.height },
      visible:
        el.checkVisibility({ visibilityProperty: true }) &&
        r.width > 0 &&
        r.height > 0,
      style: styleOf(cs),
      textRuns: [],
      pseudoRules: [],
    };
    for (const a of el.attributes) {
      if (a.name === "class" || a.name === "style" || a.name === "id") continue;
      entry.attrs[a.name] = a.value;
    }
    if (el instanceof HTMLInputElement) {
      entry.input = {
        type: el.type,
        value: el.value,
        checked: el.checked,
        min: el.min,
        max: el.max,
      };
    }
    if (el.scrollHeight > el.clientHeight + 1 && cs.overflowY !== "visible") {
      entry.scroll = {
        scrollHeight: el.scrollHeight,
        clientHeight: el.clientHeight,
        scrollTop: el.scrollTop,
      };
    }
    for (const which of ["::before", "::after"]) {
      const pcs = getComputedStyle(el, which);
      const content = pcs.getPropertyValue("content");
      if (content && content !== "none" && content !== "normal") {
        entry[which === "::before" ? "before" : "after"] = styleOf(pcs);
      }
    }
    if (el instanceof HTMLInputElement && el.type === "range") {
      entry.sliderThumb = styleOf(
        getComputedStyle(el, "::-webkit-slider-thumb"),
      );
      entry.sliderTrack = styleOf(
        getComputedStyle(el, "::-webkit-slider-runnable-track"),
      );
    }
    for (const rule of stateRules) {
      let hit = false;
      try {
        hit = rule.stripped ? el.matches(rule.stripped) : false;
      } catch {}
      if (!hit) continue;
      entry.pseudoRules.push({
        selector: rule.selector,
        states: rule.states,
        on: rule.on,
        pseudoElement: rule.pseudoElement,
        cssText: rule.cssText,
        declarations: rule.declarations,
      });
    }
    elements.push(entry);
    for (let c = el.firstChild; c; c = c.nextSibling) {
      if (c.nodeType === 3) {
        if (!c.nodeValue.trim()) continue;
        const range = document.createRange();
        range.selectNodeContents(c);
        const rr = range.getBoundingClientRect();
        entry.textRuns.push({
          kind: "text",
          text: c.nodeValue,
          rect: { x: rr.x, y: rr.y, w: rr.width, h: rr.height },
          fontSize: cs.fontSize,
        });
      } else if (c.nodeType === 1) {
        if (c.nodeName === "IMG" && c.classList.contains("pe")) {
          const ir = c.getBoundingClientRect();
          entry.textRuns.push({
            kind: "emoji",
            grapheme: c.alt,
            size: ir.width,
            rect: { x: ir.x, y: ir.y, w: ir.width, h: ir.height },
          });
        }
        if (c.nodeName === "SCRIPT") continue;
        walk(c, idx);
      }
    }
  };
  walk(root, -1);
  return { root: rootSel, count: elements.length, elements };
}

function sheetDump() {
  const keyframes = [];
  const vars = [];
  const walkRules = (rules, sheetName) => {
    for (const rule of rules) {
      if (rule.type === CSSRule.KEYFRAMES_RULE) {
        keyframes.push({
          name: rule.name,
          sheet: sheetName,
          cssText: rule.cssText,
          keyframes: [...rule.cssRules].map((k) => {
            const declarations = [];
            for (let i = 0; i < k.style.length; i++) {
              const p = k.style[i];
              declarations.push([p, k.style.getPropertyValue(p)]);
            }
            return {
              keyText: k.keyText,
              cssText: k.style.cssText,
              declarations,
            };
          }),
        });
      } else if (rule.type === CSSRule.STYLE_RULE) {
        for (let i = 0; i < rule.style.length; i++) {
          const p = rule.style[i];
          if (p.startsWith("--"))
            vars.push({
              selector: rule.selectorText,
              name: p,
              value: rule.style.getPropertyValue(p).trim(),
            });
        }
      } else if (rule.cssRules) walkRules(rule.cssRules, sheetName);
    }
  };
  [...document.styleSheets].forEach((s, i) => {
    try {
      walkRules(
        s.cssRules,
        s.ownerNode && s.ownerNode.id ? s.ownerNode.id : s.href || `sheet${i}`,
      );
    } catch {}
  });
  const cs = getComputedStyle(document.documentElement);
  const computed = {};
  for (const v of vars.filter((x) => x.selector === ":root"))
    computed[v.name] = cs.getPropertyValue(v.name).trim();
  return { keyframes, vars, computed };
}

const browser = await launchBrowser();
const written = [];
let sheets = null;
for (const profile of ["zero", "iphone"]) {
  for (const entry of ENTRIES) {
    if (only && !only.includes(entry.name)) continue;
    const session = new Session(browser, {
      profile,
      stateId: `spec_${entry.name}`,
    });
    try {
      for (const step of entry.steps) await session.runStep(step);
      await session.page.evaluate(() => window.__parity.waitEmoji());
      await session.page.evaluate(() => window.__parity.sweep());
      const dump = await session.page.evaluate(pageDump, {
        rootSel: entry.root,
        props: STYLE_PROPS,
      });
      if (dump.error) throw new Error(dump.error);
      if (!sheets) sheets = await session.page.evaluate(sheetDump);
      writeFileSync(
        join(outDir, `${entry.name}__${profile}.json`),
        JSON.stringify(
          {
            page: entry.name,
            profile,
            viewport: { w: 430, h: 932, scale: 3 },
            ...dump,
          },
          null,
          1,
        ),
      );
      written.push(`${entry.name}__${profile} (${dump.count} elements)`);
      console.log(`ok   ${entry.name}__${profile} (${dump.count} elements)`);
    } catch (e) {
      console.error(`FAIL ${entry.name}__${profile}: ${e.message}`);
      process.exitCode = 1;
    } finally {
      await session.close();
    }
  }
}
await browser.close();
if (sheets) {
  writeFileSync(
    join(outDir, "keyframes.json"),
    JSON.stringify(sheets.keyframes, null, 1),
  );
  writeFileSync(
    join(outDir, "css-vars.json"),
    JSON.stringify(
      { declared: sheets.vars, computedRoot: sheets.computed },
      null,
      1,
    ),
  );
  console.log(
    `keyframes: ${sheets.keyframes.length}, vars: ${sheets.vars.length}`,
  );
}
