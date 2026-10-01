# Web-to-Unity Pixel Parity Contract

Contract for the teams building the web harness, the Unity parity player, the canvas conformance layer and the diff tool. Values in this file are decided; details marked "probe" were measured on the pinned toolchain.

Pinned toolchain: Playwright `1.63.0`, Chromium `153.0.8010.12` (Playwright chromium build `1243`; `chromium.launch()` runs `chromium-headless-shell-1243`), Node `v24.15.0`, fontTools via `pip install --user fonttools`, Unity `6000.4.6f1` (built-in render pipeline, Gamma color space, uGUI 2.0, TextMesh Pro essentials).

## 1. Purpose and hard rules

1. `games/maze-puzzle/web/` is read-only reference. Nothing under it is ever edited, generated into, or formatted.
2. All web instrumentation (seeded RNG, fake clock, emoji substitution, canvas wrappers, fonts, safe-area, tracing) is injected at runtime by the harness through Playwright (`addInitScript`, `route`, CDP, `evaluate`).
3. The web implementation is the source of truth. Where `01`-`10` design docs, screenshots in `web/screenshots/`, or the existing Unity UI disagree with what the web build renders under the harness, the harness output wins.
4. The Unity side matches the harness output, not the reverse. Any deviation needed in Unity is recorded in the iteration report, never papered over in the harness.
5. No network access during capture. Everything is served from the repo.

## 2. Capture geometry and pass bar

| Item | Value |
| --- | --- |
| Viewport | 430 x 932 CSS px (= iOS points) |
| `deviceScaleFactor` | 3 |
| PNG size | 1290 x 2796 |
| Pixel format | 8-bit RGB, alpha dropped |
| Browser color | `--force-color-profile=srgb`; Unity is Gamma color space, so no conversion on either side |

Recommended Chromium launch args (final list is recorded in each iteration report): `--force-color-profile=srgb --font-render-hinting=none --disable-lcd-text`.

Pass bar, evaluated per (state, profile):

1. At least 99.0% of pixels have max per-channel absolute difference <= 16.
2. SSIM >= 0.985 using `skimage.metrics.structural_similarity(a, b, channel_axis=2, data_range=255)` with default window.
3. Every matched element (section 9) has its box within 3 px (1 pt) of the web box on each of x, y, w, h.

A state/profile passes only when all three hold.

Probe: `page.screenshot()` on this setup returns exactly 1290 x 2796 and works while the fake clock is paused.

## 3. Safe-area profiles

| Profile | top | right | bottom | left |
| --- | --- | --- | --- | --- |
| `zero` | 0 | 0 | 0 | 0 |
| `iphone` | 59 | 0 | 34 | 0 |

Units are points (= CSS px).

Web: apply with CDP `Emulation.setSafeAreaInsetsOverride({ insets: { top, right, bottom, left } })`. Probe: on Chromium 153.0.8010.12 this works and `env(safe-area-inset-*)` resolves to the exact values. If the call is unavailable, inject a `<style>` with exactly these four rules (T = top, B = bottom):

```css
.hud-top-mobile { top: calc(Tpx + 14px) }
.page-header-bar { padding-top: calc(Tpx + 14px) }
.home-top-bar { padding-top: calc(Tpx + 14px) }
.home-arcade-dock { height: calc(68px + Bpx); padding-bottom: calc(Bpx + 6px) }
```

Unity: an `ISafeAreaProvider` supplies the insets; in parity mode a simulated provider returns the profile values. Layout code reads only the provider, never `Screen.safeArea`.

## 4. Determinism

### 4.1 Random

`Math.random` is replaced by mulberry32 with seed `0x5EED1234` (unsigned), created fresh at page load before any app script runs. Reference implementation:

```js
function mulberry32(a){return function(){a|=0;a=a+0x6D2B79F5|0;let t=Math.imul(a^a>>>15,1|a);t=t+Math.imul(t^t>>>7,61|t)^t;return((t^t>>>14)>>>0)/4294967296;};}
Math.random = mulberry32(0x5EED1234);
```

Unity: a static `JsRandom` with the identical algorithm (32-bit wraparound integer math, `uint` output divided by `4294967296.0`), reset to the seed at app start in parity mode. Every web `Math.random` call site maps to exactly one `JsRandom.NextDouble()` call, in the same order. Call-site counts in the current web source: `main.js` 4, `engine/physics.js` 16, `engine/renderer.js` 36, `engine/audio.js` 5 (61 total). Call sites inside loops or per-frame code must consume the same number of values per frame as the web code.

### 4.2 Time

- Playwright context options: `timezoneId: 'America/Los_Angeles'`, `locale: 'en-US'`.
- `page.clock.install({ time: new Date('2026-10-01T09:00:00-07:00') })`, then the clock is paused before navigation with `page.clock.pauseAt(new Date(T0 + 1000))`. From then on only the harness advances time, with `page.clock.runFor(ms)`.
- `new Date()` is therefore always on 2026-10-01 local (`2026-10-01T16:00Z`), so `toISOString().slice(0, 10)` in `engine/economy.js` yields `2026-10-01`.

### 4.3 requestAnimationFrame under the fake clock (probe)

Probe files: `scripts/raf-probe.html`, `scripts/raf-probe.mjs` (generic page), `scripts/raf-probe-app.mjs` (real web app), `scripts/paused-clock-probe.mjs`.

| Finding | Value |
| --- | --- |
| Clock paused after `install` + `pauseAt` | Real time passing does not advance frames, timers, `Date.now` or `performance.now` (0 frames in 300 ms real wait) |
| Frame interval | Exactly 16 ms of fake time (not 16.667), fixed grid |
| Frame grid | Frames fire when `performance.now()` is a multiple of 16: first frame at 16, then 32, 48, ... |
| rAF callback argument | `performance.now()` at the frame, an integer, equal to `performance.now()` read inside the callback |
| `Date.now` vs `performance.now` | Both advance together; `Date.now()` delta equals `performance.now()` delta |
| `runFor(16)` | Fires exactly 1 frame; `runFor(1000)` from perf 1 fires 62 frames (frames at 16..992) |
| Timers | `setTimeout`/`setInterval` fire at their exact fake-time due ms, ordered by due time, ties by creation order |
| `performance.now()` when app scripts start | 1 or 2 ms (observed 2, 2, 1 across three loads), not reproducible |
| Without `pauseAt` (install only) | Clock keeps running in real time; frames are 15-18 ms apart; unusable |
| CSS animations / transitions | **Not controlled by the fake clock.** `document.timeline.currentTime` follows real time (320 ms after a 300 ms real wait with the clock paused; `runFor(1000)` did not move it) |

Resulting game-loop delta. `main.js` computes `dt = Math.min((currentTime - this.lastTime) / 1000, 0.05)` with `lastTime` seeded from `performance.now()` at construction. Because that seed is 1 or 2 ms, the first frame would get dt 0.014 or 0.015. The harness `boot` op normalizes this: after load it reads `p = performance.now()` and sets `window.app.lastTime = 16 * Math.floor(p / 16)` (which is `0` for p in {1, 2}) before any frame fires. Result:

- **dt = 0.016 s exactly on every frame, including the first.** The 0.05 clamp is never reached.
- Frame k fires at `t = 16 * k` ms on the harness time base (section 10).

Unity parity mode replicates this exactly: a fixed frame clock driven by the runner (never `Time.deltaTime`, never `Time.unscaledDeltaTime`), 16 ms per frame, `dt = 0.016f` passed to physics/renderer update, frame k at t = 16k ms. Unity-side timers (the equivalents of `setTimeout`/`setInterval`) are scheduled on the same virtual clock with the same due-time, creation-order tie-break; at equal due time a timer created earlier than the frame callback runs before it.

CSS animation policy (required because of the finding above): the harness must make CSS animations and transitions deterministic before every `capture`. Rule: at boot, send CDP `Animation.enable` and `Animation.setPlaybackRate({ playbackRate: 0 })`; during every `advance`, after each `runFor` step, set `animation.currentTime = nowFake - createdAtFake` for each animation returned by `document.getAnimations()`, where `createdAtFake` is the fake `performance.now()` at which the harness first saw that animation. If CDP freezing proves unreliable, the fallback is injected CSS `*{animation-play-state:paused!important}` plus explicit `currentTime` assignment. Whichever is used is recorded in the iteration report; Unity animates its equivalents from the same virtual clock with the same start time (the fake time at which the element was shown or the class was applied).

## 5. Fonts

Single source of truth: the static files in `assets/fonts/` (produced by `scripts/fetch-assets.mjs`, pinned in `assets/manifest.json`).

| File | wght | Source | sha256 |
| --- | --- | --- | --- |
| `Fredoka-400.ttf` | 400 | `Fredoka[wdth,wght].ttf`, wdth 100 | `4181ccb9fb9253b2a9ce037a1599589b9485904e6fdd6de97ca96e027b3ee30f` |
| `Fredoka-600.ttf` | 600 | same | `b24e06cf369855020b81ef1e9639f7b705d5348c96df96e875fc7d78bbbc6458` |
| `Fredoka-700.ttf` | 700 | same | `c570243acd47e10bc47913bd1b0b20ba36f53ec8663e093f02195cceaca25297` |
| `Fredoka-800.ttf` | 800 (clamped to 700 outlines) | same | `c570243acd47e10bc47913bd1b0b20ba36f53ec8663e093f02195cceaca25297` |
| `Fredoka-900.ttf` | 900 (clamped to 700 outlines) | same | `c570243acd47e10bc47913bd1b0b20ba36f53ec8663e093f02195cceaca25297` |
| `Nunito-400.ttf` | 400 | `Nunito[wght].ttf` | `e0ece6c997db47df512b21c010284f2fd25fad7df110f0e0b8978381b55dae51` |
| `Nunito-700.ttf` | 700 | same | `c3943d48246d348358a8c9cea93705d3ac83f64309876abc00d7b02275acea01` |
| `Nunito-800.ttf` | 800 | same | `f3ca46028e63fcfe1ba08376d64219451b9eb3b7f273d6854e289456576af55e` |
| `Nunito-900.ttf` | 900 | same | `01abe18ab593161e909b10a59673b222b89899225ce254d0f4723bcc30964b21` |
| `NotoSansSymbols2-Regular.ttf` | single weight | `NotoSansSymbols2-Regular.ttf` (unmodified) | `7d5fb73b7ca67a6798101741f5d280a3d016a56a197afcd4199dbb57b4b82a21` |
| `NotoSansSymbols-Regular.ttf` | 400 | `NotoSansSymbols[wght].ttf` | `f7a733b390a5cc37bf60cd3131aa844ad0bef90e3f5d56cae60fec30c39617d2` |

Fredoka's variable `wght` axis spans 300-700, so the requested 800 and 900 instances are the 700 outlines. This reproduces what a browser does for an out-of-range weight, and keeps all five declared weights loadable. Instances are generated with `SOURCE_DATE_EPOCH=1767225600` so regeneration is byte-identical.

### 5.1 Glyph coverage and fallback chain

Characters checked (every non-ASCII, non-emoji code point in `index.html`, `main.js`, `styles.css`, `engine/*.js`, plus the required set): U+00D7 `×`, U+2014 `—`, U+2019 `’`, U+2022 `•`, U+2190 `←`, U+25B6 `▶`, U+2713 `✓`, digits, Latin (`scripts/font-coverage.py`).

| Code point | Fredoka | Nunito | Noto Sans Symbols 2 | Noto Sans Symbols |
| --- | --- | --- | --- | --- |
| `×` `—` `’` `•` digits Latin | yes | yes | n/a | n/a |
| `←` U+2190 | no | no | no | **yes** |
| `✓` U+2713 | no | no | **yes** | no |
| `▶` U+25B6 | no | no | **yes** | no |

Fallback chain, identical on both sides, in order:

1. Primary: `Fredoka` (display text, `--font-bubble`) or `Nunito` (body text, `--font-body`) at the requested weight.
2. `Noto Sans Symbols 2` (`NotoSansSymbols2-Regular.ttf`): covers `✓` and `▶`, and is a broad symbol font that covers future additions in the same blocks.
3. `Noto Sans Symbols` (`NotoSansSymbols-Regular.ttf`, wght 400 instance): covers `←` (and `→`), which Symbols 2 lacks.

Both fallbacks are OFL, from `google/fonts` at the pinned commit. No system font is ever consulted for a web-used character. The chain has no weight variants: the fallback fonts are declared with `font-weight: 100 900` in CSS (no synthetic bold), and Unity uses the single TMP asset for every weight.

### 5.2 Web delivery

- Intercept the request to `https://fonts.googleapis.com/css2?...` (it comes from the `@import` on `styles.css` line 6) and answer with local `@font-face` rules, `Content-Type: text/css`:
  - family `Fredoka`, one rule per weight 400/600/700/800/900, `src: url(<origin>/__parity/fonts/Fredoka-<W>.ttf) format('truetype')`, `font-weight: <W>`, `font-display: block`;
  - family `Nunito`, same for 400/700/800/900;
  - family `Noto Sans Symbols 2` and `Noto Sans Symbols`, `font-weight: 100 900`, `font-display: block`.
- Serve `/__parity/fonts/*` and `/__parity/emoji/*` from `assets/` via `page.route`. The app itself is served from `web/` under a fake origin (`http://parity.test/`) with `page.route`, because `main.js` is an ES module and does not load from `file://`.
- Block every other request (`route.abort()`).
- Chain injection (the chain cannot come from the app CSS): add a stylesheet `:root{--font-bubble:"Fredoka","Noto Sans Symbols 2","Noto Sans Symbols",sans-serif;--font-body:"Nunito","Noto Sans Symbols 2","Noto Sans Symbols",sans-serif} html{font-synthesis:none}`, and wrap the `CanvasRenderingContext2D.font` setter so a trailing `, sans-serif` after `Fredoka` or `Nunito` becomes `, "Noto Sans Symbols 2", "Noto Sans Symbols", sans-serif` (`renderer.js` sets e.g. `"bold 13px Fredoka, sans-serif"`).
- `boot` waits for `document.fonts.ready`, then verifies with `document.fonts.check` / `FontFace.status` that every family and weight is `loaded`: Fredoka 400/600/700/800/900, Nunito 400/700/800/900, both symbol fonts. Any miss fails the run.

### 5.3 Unity

TMP SDF assets are generated by `OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll` into `Assets/Fonts/TMP/<FontName> SDF.asset` from the same files: sampling point size 96, padding 12, render mode `SDFAA`, atlas 4096 x 4096, `AtlasPopulationMode.Dynamic`, multi-atlas enabled. Each Fredoka/Nunito asset has fallback table `[NotoSansSymbols2-Regular SDF, NotoSansSymbols-Regular SDF]` in that order. Weight selection maps the CSS weight to the matching asset (400/600/700/800/900 for Fredoka, 400/700/800/900 for Nunito).

## 6. Emoji substitution

Native emoji fonts are never used. Both sides draw the pinned Microsoft Fluent UI Emoji 3D PNG for every emoji grapheme.

Classification (both sides, identical): split text with `Intl.Segmenter` (grapheme granularity). A grapheme is emoji iff it contains an `Extended_Pictographic` code point AND (it contains U+FE0F OR its first code point has `Emoji_Presentation`). Text-presentation symbols without U+FE0F (`▶`, `✓`, `←`, `×`, `•`) are not emoji and render with text fonts. Unity implements the same rule over its own tables generated from the same Unicode data (`emoji_index.json` already lists every grapheme the app uses).

Files: `assets/emoji/<codepoints-lowercase-hyphen-joined-without-fe0f>.png`, 256 x 256, mapped by `assets/emoji/index.json` (`grapheme -> { file, folder, style }`). Unity copies: `Assets/Resources/Emojis/3D/<codepoints>.png` and `Assets/Resources/Emojis/emoji_index.json`.

The web prototype uses 57 distinct emoji graphemes (full list in section 14).

### 6.1 DOM

- Every emoji grapheme in a text node is replaced by `<img class="pe" src="/__parity/emoji/<file>.png" alt="<grapheme>">`.
- CSS: `img.pe{width:1em;height:1em;vertical-align:-0.125em;display:inline-block;margin:0;padding:0;border:0}`.
- Applied before first paint and on every DOM mutation: a `MutationObserver` over `document` with `childList`, `subtree`, `characterData` and `attributes`; each mutation re-scans affected text nodes (including nodes created from `innerHTML`, `textContent` and `insertAdjacentHTML`). `img.pe` elements are skipped by the scan.
- All 57 images are preloaded and decoded at boot; `capture` waits until every `img.pe` has `complete` and non-zero `naturalWidth`.

### 6.2 Canvas

`fillText` and `strokeText` are wrapped. For a string `s` drawn with font size `fontPx` (px size parsed from `ctx.font`):

1. Split `s` into runs: maximal runs of non-emoji text, and single emoji graphemes.
2. `total = sum(ctx.measureText(textRun).width over text runs) + (#emoji * fontPx)`, measured with the original `measureText`.
3. Start x from `textAlign`: `left`/`start` -> `x`; `center` -> `x - total/2`; `right`/`end` -> `x - total`.
4. Walk the runs left to right: text runs are drawn with the original call at the running x with `textAlign` temporarily `'left'` (restored afterwards); each emoji is `drawImage`d as a `fontPx x fontPx` square at the running x and advances by `fontPx`.
5. Vertical placement of each emoji by `textBaseline`: `top` -> square top = `y`; `middle` -> square center = `y`; `alphabetic`, `bottom`, `ideographic` -> square bottom = `y + 0.125 * fontPx`.
6. `strokeText` draws only the text runs; emoji images are drawn once, by `fillText`. If only `strokeText` is called for a string, the emoji are drawn in that `strokeText` call.
7. The canvas `globalAlpha`, transform and clip apply to the images as usual; emoji images are drawn with the context's current `fillStyle` ignored.

Unity: the Canvas2D layer implements the same run algorithm (`canvas-conformance/` holds the conformance scenes). TMP emoji are sprites in a TMP sprite asset with glyph width = height = 1 em, baseline offset matching vertical-align `-0.125em` (the glyph bottom sits 0.125 em below the baseline), advance 1 em.

## 7. File and directory layout

```
tools/parity/
  SPEC.md
  package.json  package-lock.json  .gitignore
  states.json                      (state list, authored by the states team; section 8)
  tracked-classes.json             (class list for layout JSON; section 9)
  assets/                          (pinned fonts + emoji; manifest.json)
  scripts/                         (fetch-assets.mjs, emoji-scan.mjs, font-coverage.py, probes)
  web/                             (Playwright harness)
  golden/                          (curated reference captures, if any)
  audio/                           (audio trace tooling)
  canvas-conformance/              (Canvas2D layer conformance scenes)
  diff/                            (diff.py)
  reports/iter-NN/report.md        (committed)
  out/                             (git-ignored)
    web/<stateId>__<profile>.png
    web/<stateId>__<profile>.layout.json
    web/traces/<sequenceId>.trace.json
    unity/<stateId>__<profile>.png
    unity/<stateId>__<profile>.layout.json
    unity/traces/<sequenceId>.trace.json
    reports/iter-NN/                (heatmaps, diff images)
```

- `stateId` is snake_case; `profile` is `zero` or `iphone`.
- `sequenceId` equals the `stateId` of the state that produced the trace.
- `NN` is a zero-padded two-digit iteration number.

## 8. `states.json`

Array of states:

```json
[
  {
    "id": "home_default",
    "description": "Home screen after loading, default profile",
    "profiles": ["zero", "iphone"],
    "steps": [
      { "op": "boot" },
      { "op": "capture" }
    ]
  }
]
```

Fields: `id` (snake_case, unique), `description`, `profiles` (subset of `["zero","iphone"]`), `steps` (ordered, non-empty, exactly one `capture`, last). Each (state, profile) runs in a fresh browser context / fresh Unity app start with empty storage. Ops are declarative; neither side executes raw JS from the file.

| Op | Fields | Web semantics (exact) |
| --- | --- | --- |
| `boot` | `completeLoading` (bool, default `true`) | Apply the profile's safe area (section 3); install clock and pause (4.2); register routes (5.2); inject init scripts (RNG, emoji, canvas wrappers, fonts chain, tracing) so they run before app scripts; `goto http://parity.test/index.html` and wait for `load`; wait `document.fonts.ready` and verify fonts (5.2); wait for emoji images (6.1); normalize `window.app.lastTime = 16 * Math.floor(performance.now() / 16)` (4.3). If `completeLoading` is true, step `runFor(16)` until `app.currentPage === "home"` (at most 2000 steps, fails otherwise). Probe with the pinned seed: home is reached after 82 frames (`performance.now()` = 1314). With `completeLoading: false` the loading screen is left in its initial state. Exactly one `boot`, first. |
| `showPage` | `page` (one of `loading`,`home`,`game`,`maps`,`daily`,`shop`,`achievements`,`settings`) | `window.app.showPage(page)`; no time advance. `game` schedules the 3500 ms gesture-hint timer as in the app. |
| `setProfile` | `path` (dotted, into `app.economy.profile`), `value` (JSON) | Walks `path` from `app.economy.profile` and assigns `value` in memory. Does not call `saveProfile`, does not re-render. A later `showPage`, `call` of a render method, or `call updateAllCreditDisplays` re-renders. |
| `setApp` | `field`, `value` (JSON) | `app[field] = value`. No side effects. |
| `call` | `method`, `args` (JSON array) | `app[method](...args)`. `method` must be in the allow-list: `showPage`, `showToast`, `updateAllCreditDisplays`, `updateShopDockBadge`, `updateDailyDockBadge`, `updateHomeView`, `renderSectorsPage`, `renderDailyPage`, `renderStorePage`, `renderAchievementsPage`, `syncSettingsUI`, `restartSector`, `nextSector`, `handleLevelVictory`, `handleLevelFailure`, `exitGameToHome`, `resetGameplaySessionUi`, `updateQuotaDisplay`, `dismissGestureHint`, `initLayout`, `triggerHaptic`. Any other name fails the state. |
| `click` | `target` (DOM id) | Read `getBoundingClientRect()` of `#target`, take the center, then `page.mouse.move(x,y)`, `mouse.down()`, `mouse.up()`. No auto-scroll, no actionability wait. Fails if the element is missing or not visible. No time advance. |
| `pointer` | `kind` (`down`/`move`/`up`), `x`, `y` (CSS px, viewport coordinates) | `page.mouse.move(x,y)` then, for `down`, `mouse.down()`; for `up`, `mouse.up()`; for `move` only the move (the button state stays as set by earlier ops). Unity injects the same event at `(x*3, 2796 - y*3)` screen px through the EventSystem. |
| `key` | `key` (Playwright key name, e.g. `a`, `ArrowLeft`, `Space`, `Escape`) | `page.keyboard.press(key)` (keydown then keyup on the focused document). |
| `advance` | `ms` (integer >= 0) | `page.clock.runFor(ms)` as one call, then the CSS animation sweep (4.3). Frames fire on the 16 ms grid; timers fire at their due times. The harness may split into 16 ms steps internally but the observable effect must be identical. |
| `loadSector` | `index` (0-based into `app.sectors`) | `app.loadSector(app.sectors[index], false)`; no toast; no time advance. |
| `holdPressed` | `target` (DOM id) | Move the mouse to the center of `#target` and `mouse.down()` without releasing, so `:active` styling is captured. The button is released when the state ends. |
| `capture` | none | Exactly one per state, last. Wait for fonts and `img.pe` complete, run the animation sweep, write layout JSON (section 9) and the trace (section 10), then `page.screenshot({ animations: 'allow' })` to `out/web/<stateId>__<profile>.png`, converted to 8-bit RGB. |

Unity implements each op over its own runtime with the same observable result: ids in `click`/`holdPressed`/layout are Unity element names that equal the web DOM ids; `app`/`economy` field and method names are mapped one-to-one by the Unity runner.

## 9. Layout JSON

`out/<side>/<stateId>__<profile>.layout.json`:

```json
{
  "state": "home_default",
  "profile": "zero",
  "viewport": { "w": 430, "h": 932, "scale": 3 },
  "elements": [
    {
      "id": "btn-home-play",
      "tag": "button",
      "classes": ["btn-hero-play-3d"],
      "rect": { "x": 0, "y": 0, "w": 0, "h": 0 },
      "visible": true,
      "opacity": 1,
      "text": "PLAY"
    }
  ]
}
```

- `elements` lists, in document order, only elements that have an `id` or that have at least one class in `tracked-classes.json` (144 classes: every class used by `index.html` and `main.js` template strings that is defined in `styles.css`; regenerate with `node scripts/tracked-classes.mjs`).
- `rect`: `getBoundingClientRect()` border box in CSS px, page coordinates (viewport rect plus scroll offset), unrounded numbers.
- `visible`: true iff no ancestor-or-self has `display: none`, computed `visibility` is `visible`, and the box has `w > 0` and `h > 0`.
- `opacity`: the element's own computed opacity (not cumulative).
- `text`: element text with each `img.pe` replaced by its `alt` grapheme, whitespace collapsed, trimmed, at most 200 characters.
- `classes`: the full class list of the element (not filtered), in attribute order.
- Matching: by `id`; elements without id are keyed `<first tracked class>#<ordinal among elements sharing that class in document order>`. Unity emits the same ids and keys. Only elements with `visible: true` on the web side must match within 3 px; a visibility disagreement is a failure.
- Unity emits the same schema from its RectTransforms, converted to the same CSS-px page coordinates (origin top-left, y down).

## 10. Trace JSON

`out/<side>/traces/<sequenceId>.trace.json`:

```json
{
  "sequence": "pause_resume",
  "events": [
    { "t": 1314, "type": "page", "name": "home", "args": [] }
  ]
}
```

- `t`: integer ms since boot on the frozen clock. The time base is `performance.now()` of the fake clock, whose origin is the start of the document (frames are therefore at multiples of 16). Unity uses its virtual clock with frame k at 16k ms.
- `type` is one of `vibrate`, `audio`, `page`, `modal`, `toast`, `timer`, `credits`.

| Type | `name` | `args` | Web capture point |
| --- | --- | --- | --- |
| `vibrate` | `vibrate` | `[ms]` | wrapped `navigator.vibrate` (the wrapper reports success and returns `true`) |
| `audio` | `AudioEngine` method name | JSON-cloned arguments | every call of a public (non-underscore) method on `app.audio`, wrapped on the instance |
| `page` | page id | `[]` | wrapped `app.showPage` |
| `modal` | element id (`modal-pause`, `modal-privacy`, `modal-reset-confirm`, `modal-debrief`, `modal-defeat`) | `["open"]` or `["close"]` | visibility change of those elements, observed on the frame grid |
| `toast` | `show` | `[message]` | wrapped `app.showToast` |
| `timer` | `timeout` or `interval` | `[delayMs]` | fire of an app-created `setTimeout`/`setInterval` callback (harness-created timers excluded) |
| `credits` | `set` | `[value]` | `app.economy.profile.credits` change, sampled on each `updateAllCreditDisplays` call |

Comparison rule: identical event order, identical `type`, `name`, `args` (deep equality), and `|t_web - t_unity| <= 16.7` ms for each event pair.

## 11. Unity parity player CLI

```
OrbitalSalvageParity.exe -batchmode --parity <statesJson> <outDir> [--only id1,id2] [--profile zero|iphone]
```

- Reads `states.json` (section 8). For each selected state and profile, starts a fresh app (empty storage, `JsRandom` reset, virtual clock at 0, simulated safe area), executes steps, and writes `<outDir>/<stateId>__<profile>.png`, `<outDir>/<stateId>__<profile>.layout.json` and `<outDir>/traces/<stateId>.trace.json`.
- Renders offscreen at 1290 x 2796: every canvas is Screen Space - Camera targeting a camera that renders into a `RenderTexture` of that size (Gamma, no MSAA unless a state requires it); the PNG is read back as 8-bit RGB.
- `--only` limits states by id; `--profile` limits profiles (default: each state's `profiles`).
- Time is driven by the runner's fixed frame clock (4.3), never wall time. No `-nographics`: a GPU context is required for rendering.
- Exit code `0` when every selected state/profile was captured; `1` when any step or capture failed (details on stderr and in `<outDir>/errors.json`); `2` for CLI usage errors.

Asset sync (editor): `Unity.exe -batchmode -nographics -quit -projectPath games/maze-puzzle/unity/maze-puzzle -executeMethod OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll -logFile <log>`.

## 12. Diff tool CLI

```
python tools/parity/diff/diff.py --web out/web --unity out/unity --iter NN
```

Paths are relative to `tools/parity` unless absolute. Requires `numpy`, `Pillow`, `scikit-image`.

Pairs `<stateId>__<profile>.png` (and `.layout.json`, traces) across the two directories and applies the pass bar of section 2 plus the trace rule of section 10.

Outputs:

- `reports/iter-NN/report.md`: one table row per (state, profile) with percent of pixels within tolerance, SSIM, worst element box delta (id and axis), trace verdict, and PASS/FAIL; a summary line with counts; the recorded harness settings (Chromium version, launch args, animation policy).
- `reports/iter-NN/report.json`: the same data, machine readable.
- `out/reports/iter-NN/<stateId>__<profile>.heatmap.png`: per-pixel max-channel difference, scaled so 16 maps to mid-intensity and 64+ saturates.
- `out/reports/iter-NN/<stateId>__<profile>.diff.png`: three panels (web, Unity, binary mask of pixels over tolerance).
- Exit code `0` if every pair passes, `1` if any fails, `2` if a counterpart file is missing or the arguments are invalid. A missing counterpart is reported as FAIL with reason `missing`.

## 13. Pinned assets

| Source | Repository | Commit |
| --- | --- | --- |
| Fonts | `github.com/google/fonts` | `9710da1eacb3be272583c3224dcb70f9da6eadbb` |
| Emoji | `github.com/microsoft/fluentui-emoji` | `1ffb34c752ecf5d402f04cfb4b392c77f57c54bc` |

`assets/manifest.json` lists the source URL, commit, size and sha256 of every file. Regenerate with `node scripts/fetch-assets.mjs` (needs Node 24, network, `python` with fontTools). Licenses: fonts SIL OFL 1.1 (`assets/fonts/*-OFL.txt`), emoji MIT (`assets/emoji/Fluent-LICENSE.txt`); Unity-side notices in `Assets/THIRD_PARTY_NOTICES.md`.

## 14. Emoji graphemes used by the web prototype

Scan: `node scripts/emoji-scan.mjs` over `index.html`, `main.js`, `engine/*.js`, `styles.css`. 57 graphemes, all with a 3D asset (no Color-style fallback was needed). Output files are named by code points; the Fluent folder is in `assets/emoji/index.json`.

```
⏩ 23e9   ⏪ 23ea   ⏰ 23f0   ⏸️ 23f8   ⚙️ 2699   ⚠️ 26a0   ⚡ 26a1   ✅ 2705
✨ 2728   ❤️ 2764   ⭐ 2b50   🌌 1f30c  🌟 1f31f  🍓 1f353  🍩 1f369  🍬 1f36c
🎁 1f381  🎉 1f389  🎮 1f3ae  🏆 1f3c6  🏠 1f3e0  🐟 1f41f  🐱 1f431  👆 1f446
👍 1f44d  👑 1f451  💖 1f496  💤 1f4a4  💥 1f4a5  💨 1f4a8  💾 1f4be  📅 1f4c5
🔁 1f501  🔄 1f504  🔇 1f507  🔊 1f50a  🔒 1f512  🔩 1f529  🕳️ 1f573  🗑️ 1f5d1
🗺️ 1f5fa  😋 1f60b  🚀 1f680  🛡️ 1f6e1  🥇 1f947  🥈 1f948  🥉 1f949  🥣 1f963
🥫 1f96b  🥰 1f970  🥺 1f97a  🦆 1f986  🧀 1f9c0  🧲 1f9f2  🧹 1f9f9  🪐 1fa90
🪙 1fa99
```

## 15. Known facts discovered while building this contract

- `styles.css` loads Google Fonts through `@import` on line 6 with `Fredoka:wght@400;600;700;800;900`; the Fredoka family only spans 300-700, so weights 800/900 render with 700 outlines.
- `renderer.js` uses `Fredoka`/`Nunito` with a bare `sans-serif` tail on canvas, so a system font would be reached for any glyph the primary lacks unless the harness rewrites the font string (5.2).
- CSS animations and transitions ignore the fake clock (4.3); the animation policy there is mandatory.
- `performance.now()` at app construction is 1 or 2 ms; the `boot` normalization removes the effect on the first-frame dt.
- `navigator.vibrate` exists in headless Chromium and the app gates it on `profile.settings.hapticsEnabled`; the wrapper in section 10 records calls and makes the result deterministic.
