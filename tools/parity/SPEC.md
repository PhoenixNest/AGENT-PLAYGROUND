# Web-to-Unity Pixel Parity Contract

Version: 1.5 (see the changelog at the end)

Contract for the teams building the web harness, the Unity parity player, the canvas conformance layer and the diff tool. Values in this file are decided; details marked "probe" were measured on the pinned toolchain.

Pinned toolchain: Playwright `1.63.0`, Chromium `153.0.8010.12` (Playwright chromium build `1243`; `chromium.launch()` runs `chromium-headless-shell-1243`), Node `v24.15.0`, fontTools via `pip install --user fonttools`, Unity `6000.4.6f1` (built-in render pipeline, Gamma color space, uGUI 2.0, TextMesh Pro essentials).

## 1. Purpose and hard rules

1. `games/maze-puzzle/web/` is read-only reference. Nothing under it is ever edited, generated into, or formatted.
2. All web instrumentation (seeded RNG, fake clock, emoji substitution, canvas wrappers, fonts, safe-area, tracing) is injected at runtime by the harness through Playwright (`addInitScript`, `route`, CDP, `evaluate`).
3. The web implementation is the source of truth. Where `01`-`10` design docs, screenshots in `web/screenshots/`, or the existing Unity UI disagree with what the web build renders under the harness, the harness output wins.
4. The Unity side matches the harness output, not the reverse. Any deviation needed in Unity is recorded in the iteration report, never papered over in the harness.
5. No network access during capture. Everything is served from the repo.

## 2. Capture geometry and pass bar

| Item                | Value                                                                                     |
| ------------------- | ----------------------------------------------------------------------------------------- |
| Viewport            | 430 x 932 CSS px (= iOS points)                                                           |
| `deviceScaleFactor` | 3                                                                                         |
| PNG size            | 1290 x 2796                                                                               |
| Pixel format        | 8-bit RGB, alpha dropped                                                                  |
| Browser color       | `--force-color-profile=srgb`; Unity is Gamma color space, so no conversion on either side |

Recommended Chromium launch args (final list is recorded in each iteration report): `--force-color-profile=srgb --font-render-hinting=none --disable-lcd-text`.

Pass bar, evaluated per (state, profile):

1. At least 99.0% of pixels have max per-channel absolute difference <= 16.
2. SSIM >= 0.985 using `skimage.metrics.structural_similarity(a, b, channel_axis=2, data_range=255)` with default window.
3. Every matched element (section 9) has its box within 3 px (1 pt) of the web box on each of x, y, w, h.

A state/profile passes only when all three hold.

Probe: `page.screenshot()` on this setup returns exactly 1290 x 2796 and works while the fake clock is paused.

## 3. Safe-area profiles

| Profile  | top | right | bottom | left |
| -------- | --- | ----- | ------ | ---- |
| `zero`   | 0   | 0     | 0      | 0    |
| `iphone` | 59  | 0     | 34     | 0    |

Units are points (= CSS px).

Web: apply with CDP `Emulation.setSafeAreaInsetsOverride({ insets: { top, right, bottom, left } })`. Probe: on Chromium 153.0.8010.12 this works and `env(safe-area-inset-*)` resolves to the exact values. This is the primary mechanism. The CSS fallback below is secondary and is used only if the CDP call is unavailable.

`styles.css` reads only `env(safe-area-inset-top)` and `env(safe-area-inset-bottom)` (five places, listed below; verified by search). Nothing reads left or right insets, so those two values do not affect layout. `index.html` has `viewport-fit=cover`. The `@media (min-width: 480px)` phone-frame rule (`styles.css` line 69) does not apply at the 430 px viewport width.

Fallback: inject a `<style>` with exactly these four rules (T = top inset in px, B = bottom inset in px). The shorthand values are copied from the originals, so the whole `padding` shorthand is overridden and not just one side:

```css
.hud-top-mobile {
  top: calc(Tpx + 14px);
}
.page-header-bar {
  padding: calc(Tpx + 14px) 16px 12px;
}
.home-top-bar {
  padding: calc(Tpx + 14px) 16px 6px;
}
.home-arcade-dock {
  height: calc(68px + Bpx);
  padding: 4px 10px calc(Bpx + 6px);
}
```

| Rule                | Original declaration (`styles.css`)                                                                                                                                             |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.hud-top-mobile`   | line 105: `top: calc(env(safe-area-inset-top, 0px) + 14px);`                                                                                                                    |
| `.page-header-bar`  | line 991: `padding: calc(env(safe-area-inset-top, 0px) + 14px) 16px 12px;`                                                                                                      |
| `.home-top-bar`     | line 1230: `padding: calc(env(safe-area-inset-top, 0px) + 14px) 16px 6px;`                                                                                                      |
| `.home-arcade-dock` | line 1661: `height: calc(68px + env(safe-area-inset-bottom, 0px));` and line 1670: `padding: 4px 10px calc(env(safe-area-inset-bottom, 0px) + 6px);` (`min-height: 68px` stays) |

The fallback `<style>` has the same specificity as the originals, so it must come after `styles.css` in source order (section 5.2 "Injection order").

Unity: an `ISafeAreaProvider` supplies the insets; in parity mode a simulated provider returns the profile values. Layout code reads only the provider, never `Screen.safeArea`.

## 4. Determinism

### 4.1 Random

`Math.random` is replaced by mulberry32 with seed `0x5EED1234` (unsigned), created fresh at page load before any app script runs. Reference implementation:

```js
function mulberry32(a) {
  return function () {
    a |= 0;
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
Math.random = mulberry32(0x5eed1234);
```

Unity: a static `JsRandom` with the identical algorithm (32-bit wraparound integer math, `uint` output divided by `4294967296.0`), reset to the seed at app start in parity mode. Every web `Math.random` call site maps to exactly one `JsRandom.NextDouble()` call, in the same order. Call-site counts in the current web source: `main.js` 4, `engine/physics.js` 16, `engine/renderer.js` 36, `engine/audio.js` 5 (61 total). Call sites inside loops or per-frame code must consume the same number of values per frame as the web code.

### 4.2 Time

- Playwright context options: `timezoneId: 'America/Los_Angeles'`, `locale: 'en-US'`.
- `page.clock.install({ time: new Date('2026-10-01T09:00:00-07:00') })`, then the clock is paused before navigation with `page.clock.pauseAt(new Date(T0 + 1000))`. Between the two calls the fake clock runs in real time, so `install` is given `T0 - 60000` (the web harness) to keep `pauseAt` in the future on a loaded machine: with `time: T0` a 1 s margin was exceeded once in 276 captures and `pauseAt` failed with "Cannot fast-forward to the past". After `pauseAt` the clock is exactly `T0 + 1000`, so nothing observable changes. From then on only the harness advances time, with `page.clock.runFor(ms)`.
- Probe (4 loads): at the start of the document `Date.now()` is exactly `1790870401000` = `2026-10-01T16:00:01.000Z` = `2026-10-01T09:00:01.000-07:00` (T0 + 1000 ms), on every load. Under the normalised clock (4.4), `performance.now()` is exactly `0` at that instant, so **`Date` at `performance.now() === 0` is `2026-10-01T16:00:01.000Z`** and `Date.now() === 1790870401000 + performance.now()` afterwards (`Date` and `performance.now` advance together). The harness does not wrap `Date`.
- `engine/economy.js` builds every day key as `new Date().toISOString().slice(0, 10)` (8 sites, lines 189, 195, 217, 316, 335, 342, 348, 386). That is the **UTC** date `YYYY-MM-DD`, so gift and daily-shift dates are `2026-10-01` for the first 8 h of fake time after boot (until `2026-10-02T00:00Z`).
- `main.js getDailyResetCountdown()` (line 1077) uses local time: `tomorrow.setHours(24, 0, 0, 0)` on a copy of `new Date()`, so it counts down to the next **local midnight in `America/Los_Angeles`** and prints `Resets in ${hours}h ${mins}m`. At boot + 0 s the local time is 09:00:01 PDT (UTC-7, DST ends 2026-11-01), so the text is `Resets in 14h 59m` and stays so for the first 59 s of fake time.
- Unity parity mode must use the same frozen wall clock (UTC instant `2026-10-01T16:00:01.000Z` at virtual time 0, advancing with the virtual clock) and the same zone for the local-time computation: `TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")` on Windows (the id covers PDT too; `America/Los_Angeles` on IANA platforms). UTC day keys are computed from the UTC instant without any time-zone conversion. Unity never reads the real system clock in parity mode.

### 4.3 requestAnimationFrame under the fake clock (probe, native Playwright behaviour)

Probe files: `scripts/raf-probe.html`, `scripts/raf-probe.mjs` (generic page), `scripts/raf-probe-app.mjs` (real web app), `scripts/paused-clock-probe.mjs`. This section records what Playwright's fake clock does natively; the contract the harness enforces on top of it is in 4.4.

| Finding                                    | Value                                                                                                                                                                                                                                                |
| ------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Clock paused after `install` + `pauseAt`   | Real time passing does not advance frames, timers, `Date.now` or `performance.now` (0 frames in 300 ms real wait)                                                                                                                                    |
| Frame interval                             | Exactly 16 ms of fake time (not 16.667), fixed grid                                                                                                                                                                                                  |
| Frame grid                                 | Frames fire when `performance.now()` is a multiple of 16: first frame at 16, then 32, 48, ...                                                                                                                                                        |
| rAF callback argument                      | `performance.now()` at the frame, an integer, equal to `performance.now()` read inside the callback                                                                                                                                                  |
| `Date.now` vs `performance.now`            | Both advance together; `Date.now()` delta equals `performance.now()` delta                                                                                                                                                                           |
| `runFor(16)`                               | Fires exactly 1 frame; `runFor(1000)` from perf 1 fires 62 frames (frames at 16..992)                                                                                                                                                                |
| Timers                                     | `setTimeout`/`setInterval` fire at their exact fake-time due ms, ordered by due time, ties by creation order                                                                                                                                         |
| `performance.now()` when app scripts start | 0, 1 or 2 ms (observed 2, 2, 1, 2 in one probe and 2, 2, 0 in another), not reproducible. `Date.now()` at the same instant is always exactly `1790870401000`; only `performance.now()` jitters, so the offset between the two is not constant either |
| Without `pauseAt` (install only)           | Clock keeps running in real time; frames are 15-18 ms apart; unusable                                                                                                                                                                                |
| CSS animations / transitions               | **Not controlled by the fake clock.** `document.timeline.currentTime` follows real time (320 ms after a 300 ms real wait with the clock paused; `runFor(1000)` did not move it)                                                                      |

### 4.4 Clock normalisation and frame rule (decided)

The native behaviour in 4.3 makes `performance.now()` at app start 0, 1 or 2 ms depending on the load, so frame times, timer due times, trace `t` values and the first-frame `dt` would differ between runs. A `lastTime` patch only fixes the first `dt`; it does not make `performance.now()` at later ops deterministic. The harness therefore **re-bases `performance.now()` and owns requestAnimationFrame**. Mechanism, installed by `addInitScript` before any app script (and before the audio stub of 4.6):

1. `const rawNow = performance.now.bind(performance); const perfOffset = rawNow();` Read once, as the first statement. Observed 0, 1 or 2. `perfOffset` is recorded in the iteration report (not in any trace).
2. Redefine `performance.now = () => rawNow() - perfOffset` (own property on the `performance` instance). The app constructor then reads exactly `0`, so `app.lastTime === 0` after `load` (the harness asserts this and fails otherwise). No `lastTime` patch is needed or allowed.
3. Capture the original (fake-clock) `setTimeout`, then replace `requestAnimationFrame(cb)` with a one-shot fake timer: `delay = 16 - (performance.now() % 16)`, which calls `cb(performance.now())` when it fires; `cancelAnimationFrame(id)` clears it. Frames therefore fire at exactly `performance.now() = 16 * k` (k = 1, 2, ...), the callback argument equals `performance.now()` in the callback, and the first frame is at 16. These rAF timers are harness-created and are never reported as `timer` trace events.
4. `Date` is not wrapped (see 4.2): `Date.now() === 1790870401000 + performance.now()`.
5. Ordering rule, the same for the page and for Unity: events run in order of due time; at equal due time, in order of creation. Because `startBootSequence()` runs before the constructor's `requestAnimationFrame` call, the loading `setInterval(…, 240)` created at `t = 0` runs before the frame due at 240, 480, ...; a frame callback registers the next frame during frame k, so any timer created earlier than that registration and due at the same ms runs before the frame.

Probe (3 loads, `perfOffset` 2, 2, 0): `performance.now() = 0` and `app.lastTime = 0` at load; `lastTime` after `runFor(16)` x 3 is 16, 32, 48; the loading interval fires at 240, 480, 720, 960 and the final `setTimeout(…, 350)` fires at **1310**, which is when `showPage("home")` runs; stepping `runFor(16)` observes `currentPage === "home"` after 82 steps at `performance.now() = 1312`. All three loads give identical numbers (the earlier 1314 was the offset-2 value of the unnormalised clock).

**Frame rule** (the literal `OrbitalApp.loop` in `main.js`, lines 1360-1399; the failure-check body is elided):

```js
loop(currentTime) {
  const dt = Math.min((currentTime - this.lastTime) / 1000, 0.05);
  this.lastTime = currentTime;

  try {
    if (this.currentPage === "game") {
      // 1. Update Physics (only when not paused)
      if (!this.isPaused) {
        this.physics.update(dt);
        // ... failure check: failureCheckTimer += dt; handleLevelFailure() at >= 1.2 ...
      }

      // 2. Render Canvas with Dynamic Black Hole Growth Info
      this.renderer.render(this.physics, this.economy, dt, {
        collectedMass: this.collectedMass,
        quota: this.activeSector ? this.activeSector.quota : 30,
      });
    }
  } catch (err) {
    console.error("Simulation loop error:", err);
  }

  requestAnimationFrame((t) => this.loop(t));
}
```

Consequences, mirrored literally by Unity:

- Every rAF tick, on every page, sets `lastTime = currentTime` and registers the next tick. `dt = 16 / 1000 = 0.016` (the IEEE double, not a float) on every tick including the first; the `0.05` clamp is never reached. Unity passes the same double to physics and renderer code, and records in the iteration report any place where it has to narrow to `float`.
- `physics.update(dt)` and the defeat check run only when `currentPage === "game"` **and** `!isPaused`.
- `renderer.render(...)` (and every `Math.random` call it makes) runs only when `currentPage === "game"`, **including while `isPaused` is true**. A paused game keeps drawing every frame, so its per-frame random draws and its floating-text / particle ageing (which use `dt`) continue.
- Ticks while `currentPage !== "game"` run no physics and no render and consume no random numbers; they only advance timers and animation time.
- A thrown exception inside the `try` is logged and swallowed; the next tick is always registered.

Unity parity mode: a fixed frame clock driven by the runner (never `Time.deltaTime`, never `Time.unscaledDeltaTime`), 16 ms per frame, frame k at t = 16k ms, `lastTime` starts at 0. Unity-side timers (the equivalents of `setTimeout`/`setInterval`) are scheduled on the same virtual clock with the same due-time and creation-order tie-break (rule 5).

### 4.5 CSS animation clock (decided)

CSS animations and transitions run on the document timeline (real time, see 4.3), so the harness replaces their clock. It does **not** use CDP `Animation.setPlaybackRate` or any other CDP animation control.

**Registry.** The harness keeps a registry `Map<Animation, { startFake }>` of every CSS `Animation` / `CSSAnimation` / `CSSTransition` object returned by `document.getAnimations()` (it includes pseudo-element animations and transitions; today's styles produce none on pseudo-elements, the inventory is Appendix A).

**Diff points.** The harness diffs `document.getAnimations()` against the registry at all of these points, in this order of occurrence:

1. at the end of boot's load, before any clock advance (everything that exists at load is stamped `startFake = 0`);
2. after every rAF tick callback;
3. after every app timer callback: the init script wraps the callback passed to `setTimeout` and `setInterval` (the wrapper runs the original callback, then the diff; the same wrapper emits the `timer` trace event, section 10);
4. after every app event listener: the init script wraps listeners passed to `addEventListener` the same way (so that `click`, `pointerdown`, `keydown` and similar handlers are stamped in the dispatch that caused the change);
5. at the end of every op, including ops that do not advance time.

Each diff first forces a style flush with `getComputedStyle(document.body).opacity` and then calls `document.getAnimations()`. (Probe: `getAnimations()` itself flushes pending style, so the explicit read is redundant but kept as a guard.) A new `Animation` object is stamped `startFake = performance.now()` (the normalised clock of 4.4) at that moment and is **paused immediately** with `anim.pause()` and `anim.currentTime = 0`, so it cannot advance in real time between diffs. An object that is recreated (the same CSS rule restarted, for example `.squash` removed, reflowed and added again at `main.js` lines 297-299) is a new object and gets a new stamp. Objects that have been cancelled (play state `idle`) are dropped from the registry.

Stamp quantisation: the fake clock does not move while a callback runs, and the diff runs at the end of the callback that made the class or element change, so `startFake` equals the fake time at which the change was made. That is exactly the time Unity uses for the same change.

**Sweep.** Immediately before pixels are read (inside `capture`), at the end of every `advance` and at the end of boot's internal stepping, for each registered animation that is not yet finished on the fake clock: `anim.currentTime = performance.now() - startFake; anim.pause();`. An animation is finished on the fake clock when `performance.now() - startFake` is at or past its end time (`getComputedTiming().endTime`, finite animations only); the sweep sets `currentTime` to `min(local, endTime)` once for those and leaves them untouched afterwards (`forwards` fill keeps the end state; others fall back to the base style as in a browser). "Finished" therefore means finished on the **fake** clock, not `playState === "finished"`, which can also be reached through real time. Race guard: if a registered **transition** shows `playState === "finished"` while its fake local time is below its end time, real time overtook the fake clock (it was not paused in time) and the harness fails the state with `animation-race`; for a `CSSAnimation` the sweep simply re-seeks it.

Probe: on `#page-home` after boot, `pageEnter` with `currentTime` set to 0, 110 and 220 ms (paused each time) gives computed opacity `0`, `0.684643` and `1`.

**Screenshot.** `page.screenshot({ animations: "allow", caret: "hide" })`. `"allow"` is required so Playwright does not touch the (already paused) animations; `caret: "hide"` removes the text caret.

**Unity model.** Unity evaluates the same CSS model for each element: keyframes, `animation-delay`, iteration count, direction, fill mode and easing (including the default `ease` where Appendix A shows `ease` because the declaration omitted it), and transitions with their durations, delays and easings, at local time `fakeNow - startFake`, where `startFake` is the virtual-clock time at which the class or element change was made. Elements inside a `display: none` page have no animations until the page is shown; a transition needs a previous computed style, so an element that becomes visible and is changed in the same style recalculation does not transition.
**Boot settle.** After `currentPage === "home"` (step at 1312), `boot` advances a further **240 ms** (end of boot at `performance.now() = 1552`) so `pageEnter` (0.22 s, `forwards`) has settled; `completeLoading: false` ends boot at `performance.now() = 0` with no extra advance.

**Settle rules for states.** Each row gives the advance a state must perform before `capture`, derived from Appendix A:

| Situation                                                | Cause of motion                                                                                                                                           | Required advance before `capture`                                                                                              |
| -------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Page shown by `showPage` / click                         | `.app-page.page-active` `pageEnter` 220 ms (the `.app-page` opacity/transform transitions do not run because the page goes from `display: none` to shown) | `advance(240)`                                                                                                                 |
| Modal opened or closed (`.modal-overlay.active` toggled) | overlay opacity 250 ms, card transform 300 ms with overshoot easing                                                                                       | `advance(300)` after `active` is added or removed                                                                              |
| Toast shown (`showToast`)                                | `cartoonToast` 2600 ms: opacity 1 only between 15% (390 ms) and 82% (2132 ms); the element is removed by a 2500 ms timer                                  | capture inside the plateau and before the removal; recommended `advance(1000)` after `showToast`                               |
| Pressed control (`:active`)                              | button transitions are 100-150 ms (`.btn-hud-circle`, `.btn-hero-play-3d`, `.dock-tab-btn` 150 ms, `.level-tile` 120 ms)                                  | `holdPressed` (section 8) includes `advance(150)` while the pointer is held                                                    |
| Gesture hint hidden (`dismissGestureHint`)               | `.gesture-hint` opacity/transform 400 ms                                                                                                                  | `advance(400)` after the call                                                                                                  |
| Store tab / world tab clicked                            | `.tab-btn` / `.world-tab-btn` `all 0.15s ease`                                                                                                            | `advance(150)`                                                                                                                 |
| Daily gift / progress / master box                       | `.daily-gift-banner` 300 ms, `.daily-progress-fill` 400 ms, `.daily-master-box` 300 ms                                                                    | `advance(400)`                                                                                                                 |
| Infinite animations (mascot, debris, glows, badges)      | start when the page is shown                                                                                                                              | nothing to wait for; the capture shows local time `now - startFake`, so the advance since the page was shown defines the frame |

`:hover` rules exist (`.tab-nav-btn`, `.level-tile`, `.dock-tab-btn`, `.dock-tab-icon`, `.daily-shift-card`). `click` and `holdPressed` leave the pointer at the element centre, so those hover styles persist into the capture; Unity keeps its simulated pointer at the same position.

`heroPulse` is referenced by `.btn-hero-play-3d` (`styles.css` line 1630) but has no `@keyframes` rule: no Animation object is created, and both sides treat it as a no-op (confirmed by the keyframes list, Appendix A.1, and by the animation list at home in the probe).

### 4.6 Audio determinism (decided)

The web app creates a Web Audio context on the first user gesture and makes random draws while building sounds, so audio affects the `Math.random` sequence even though no audio is captured. To keep the random sequence, the cooldown behaviour and the trace identical on both sides, in **all** harness runs (captures and traces) the init script replaces `window.AudioContext` and `window.webkitAudioContext` with a recording stub `ParityAudioContext` before any app script runs.

`ParityAudioContext`:

- `sampleRate = 44100`.
- `state` starts `'suspended'`; `resume()` sets it to `'running'` synchronously and returns a resolved Promise (`audio.js` calls `this.ctx.resume().catch(...)`). `suspend()` and `close()` are not used by the app and are not implemented.
- `currentTime` is a getter: `(performance.now() - ctxCreatedFake) / 1000`, a double on the normalised fake clock of 4.4, where `ctxCreatedFake` is `performance.now()` at construction. It is **not** quantised to 128-frame render blocks as a real context is. Every cooldown below uses this value, so Unity must use the identical formula.
- `destination`: a stub node with id 0.
- Factory methods, restricted to what `engine/audio.js` actually calls (verified by search): `createOscillator`, `createGain`, `createBiquadFilter`, `createBufferSource`, `createDynamicsCompressor`, `createBuffer`. `createStereoPanner`, `createDelay`, `createConvolver`, `createWaveShaper` and every other factory are **not** used by the app today; the stub throws `Error("ParityAudioContext: unsupported <name>")` for them so that new engine code is noticed.
- Stub nodes: `connect(node)` (returns the target), `disconnect()`, and for source nodes `start(when)` and `stop(when)`. Properties used by the app: `OscillatorNode.type`, `.frequency`; `BiquadFilterNode.type`, `.frequency`, `.Q`; `GainNode.gain`; `DynamicsCompressorNode.threshold`, `.knee`, `.ratio`, `.attack`, `.release`; `AudioBufferSourceNode.buffer`.
- `AudioParam` stub with `value` and the methods `setValueAtTime(v, t)`, `linearRampToValueAtTime(v, t)`, `exponentialRampToValueAtTime(v, t)`, `setTargetAtTime(v, t, c)`, `cancelScheduledValues(t)` (each returns the param). `value` reads back the last value written by `value = x` or `setValueAtTime(x, t)`, applied immediately regardless of `t`; ramps and `setTargetAtTime` do not change it. The app reads `.value` exactly once, in `toggleMute()` (`audio.js` line 115: `setValueAtTime(this.masterGain.gain.value, t)`), so Unity must return the same value there: `0.85` after `publicInit` if not muted, `0` if muted before init, then whatever the last `setValueAtTime` wrote.
- `createBuffer(channels, length, sampleRate)` returns a stub `AudioBuffer` with `numberOfChannels`, `length`, `sampleRate`, `duration` and `getChannelData(c)` returning a **real, persistent `Float32Array(length)`** per channel, so the app's noise-fill loop runs and consumes `Math.random` exactly as in a real 44.1 kHz context.

**Trace.** Every context, node and param action is recorded as an event of `type: "audio"` (section 10) at the `performance.now()` of the calling code, with times in seconds on the fake clock, all numbers rounded to 1e-6:

| `name`                    | `args`                                                                                                                                                        |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ctx.create`              | `[44100]`                                                                                                                                                     |
| `ctx.resume`              | `[]`                                                                                                                                                          |
| `node.create`             | `[nodeId, kind]`, `kind` one of `oscillator`, `gain`, `biquad`, `bufferSource`, `compressor`; `nodeId` counts creations per context from 1 (destination is 0) |
| `node.set`                | `[nodeId, "type", "<value>"]` or `[nodeId, "buffer", { "length": n, "sampleRate": 44100, "channels": c }]`                                                    |
| `node.connect`            | `[fromId, toId]`                                                                                                                                              |
| `param.<method>`          | `[nodeId, "<paramName>", ...methodArgs]`, e.g. `param.setValueAtTime`, `[3, "gain", 0.85, 0]`                                                                 |
| `node.start`, `node.stop` | `[nodeId, when]`                                                                                                                                              |

The public-method events of section 10 (`name` = `playDebrisCollision` etc.) are still recorded for every call, whether or not the method returned early.

**Initialisation (`publicInit`).** `AudioEngine` leaves `ctx = null` in its constructor; `applyAudioVolumesFromProfile()` in the app constructor does nothing while `ctx` is null. The context is created only by `publicInit()`, which `main.js` calls from exactly these handlers:

| Handler (`main.js` line)                  | Trigger                                                                                                                     |
| ----------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| mascot stage click (289)                  | click on `#home-mascot-stage` (also plays `playBoosterPass`)                                                                |
| canvas `pointerdown` (388)                | pointer down on `#game-canvas`                                                                                              |
| window `keydown` (423)                    | any key                                                                                                                     |
| mute toggle (442)                         | click on `#btn-toggle-audio` or `#btn-home-audio`                                                                           |
| home navigation (462, 467, 472, 479, 485) | click on `#btn-home-play`, `#btn-open-maps-page`, `#btn-open-shop-page`, `#btn-open-daily-shift`, `#btn-open-trophies-page` |
| open settings (490)                       | click on `#btn-open-settings-page` or `#btn-home-settings`                                                                  |
| pause modal sound (563)                   | click on `#btn-pause-sound`                                                                                                 |
| settings sliders (756, 765)               | `input` on `#settings-sfx-slider`, `#settings-ambience-slider`                                                              |
| daily master bonus claim (1152)           | click on `#btn-claim-master-bonus` (when claimable)                                                                         |
| daily gift claim (1245)                   | click on `#btn-claim-daily-gift` (when claimable)                                                                           |

`call`, `setApp`, `setProfile`, `loadSector` and `showPage` never initialise audio. Once initialised, `publicInit()` only calls `ensureRunning()`, which `resume()`s a suspended context.

**Unity parity mode must consume `JsRandom` identically.** The Unity audio port keeps `ctx` (null until the same handlers run) and `isMuted`, the same cooldown fields, and `currentTime` computed with the formula above. Every `Math.random` call in `engine/audio.js` (5 sites) sits after these guards, so the method returns before any draw, and Unity must not draw, when:

- `audio.ctx` is null (never initialised), **or** `audio.isMuted` is true, **or**
- the method is inside its cooldown (`now - last < cooldown` returns immediately; the `last` field is updated only on the path that continues). Cooldowns in `audio.js`:

| Method                | Field               | Cooldown | Draws when it continues                                                                                                            |
| --------------------- | ------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `playSpinRatchet`     | `lastRatchetTime`   | 0.045 s  | none                                                                                                                               |
| `playDebrisCollision` | `lastCollisionTime` | 0.048 s  | exactly one (below)                                                                                                                |
| `playGatePass`        | `lastGatePassTime`  | 0.07 s   | none itself; delegates to `playBoosterPass`, `playCompactorPass`, `playShredderPass`, `playSuperShredderPass`, none of which draws |
| `playRcsThrust`       | `lastRcsThrustTime` | 0.05 s   | 9702 (below)                                                                                                                       |
| `playBumperBounce`    | `lastBumperTime`    | 0.045 s  | exactly one                                                                                                                        |

`playDebrisSwallowed` has no early-return cooldown: a 0.6 s window (`lastSwallowTime`) only resets the arpeggio index; it never draws. All `last*` fields start at 0 and `currentTime` starts at 0 at context creation, so any cooldown-guarded call within its cooldown after the context is created is dropped.

When initialised and not muted:

- `playDebrisCollision(intensity, debrisType)` draws exactly one value, selected by type: `solar` -> `620 + r * 80`; `volatile` -> `260 + r * 40`; any other type (including the default `titanium`) -> `460 + r * 60`.
- `playBumperBounce()` draws one value: `720 + r * 120`.
- `playRcsThrust()` builds a noise buffer of `Math.floor(44100 * 0.22) = 9702` samples (`createBuffer(1, 9702, 44100)`) and draws one value per sample in index order: `data[i] = (r * 2 - 1) * (1 - i / 9702)`. The general rule for a noise buffer of duration `d` seconds is `Math.floor(44100 * d)` draws.

**Out of scope.** Real waveform parity is not part of captures. It is tested separately by offline rendering (`tools/parity/audio`) of the recorded node graph with the same seeded RNG.

### 4.7 Numeric ground truth (1.5)

1. **Goldens come from Chromium, not Node.** Every numeric golden (logic fixtures, physics frame dumps, audio reference WAVs) is generated by the pinned Chromium 153 through Playwright, never by `node` alone. Chromium 153 `Math.sin/cos/atan/atan2` are correctly rounded (LLVM libc) whereas Node 24 uses fdlibm. Physics goldens produced under Node diverge from Chromium by 1 ulp at about frame 17 (`debris[11].vy`, 1.4e-14) and collision chaos then changes RNG call counts by frame ~265. Node output is not comparable to a Chromium golden beyond about 1e-4 and must not be used as one.
2. **`Math.pow` and `Math.hypot`.** `Math.pow` goes through the platform `pow` plus V8 special cases; .NET `Math.Pow` matches for the one parity argument (`0.94`, `dt * 60`) but differs on about 3 in 6000 random inputs. New `Math.pow` uses in physics are forbidden unless they go through `JsMath.Pow` and its tests. `Math.hypot` needs V8's exact scaling algorithm (`JsMath.Hypot`).
3. **IL2CPP and FMA contraction.** Mono EditMode results are bit-exact. A compiler that fuses `a * b + c` (IL2CPP, Burst, some LLVM settings) changes physics results and breaks the double-double trig. Parity must be re-verified in a **player build** (Mono Win64 as built by `tools/parity/unity/build.ps1` today) before sign-off, and again for any other scripting backend, or the build must disable FMA contraction.
4. **Number parsing and formatting.** Mono throws `OverflowException` for out-of-range numbers (for example `1.8E+308`) where JS returns `Infinity`; the port catches it. Mono's `ToString("E<n>")` and `double.Parse` are not correctly rounded (7 of 1532 JS `String(number)` vectors differed). Any Unity code that prints or parses JS numbers (JSON, UI text, ids) uses `JsNumber.ToString` and the same overflow handling, never the .NET runtime directly.
5. **Reference audio is not bit-reproducible.** Re-rendering `tools/parity/audio/render-ref.mjs` can change the sha256 of about 10 of 72 reference WAVs by float noise (about 3e-8 max absolute sample difference, recorded as `bitExactRepeat: false` in `ref-manifest.json`). `out/ref` must therefore be the set whose hashes match the committed `ref-manifest.json`; the audio tests fail on a stale reference by design.

## 5. Fonts

**Rule.** The reference reproduces what a real browser renders for the web prototype (section 1 rule 3). The app loads its fonts from Google Fonts, and Google serves only some of the requested weights (5.2, evidence). The harness therefore serves `@font-face` rules for **exactly** the faces Google serves and nothing else, and lets Chrome's normal CSS font matching resolve every other weight (no synthetic bold, no extra faces). Unity does the same with `OrbitalSalvage.Render.ParityFonts` (5.3).

Faces (single source of truth: the static files in `assets/fonts/`, produced by `scripts/fetch-assets.mjs` and pinned in `assets/manifest.json`):

| File                           | wght          | Source                                      | sha256                                                             |
| ------------------------------ | ------------- | ------------------------------------------- | ------------------------------------------------------------------ |
| `Fredoka-400.ttf`              | 400           | `Fredoka[wdth,wght].ttf`, wdth 100          | `4181ccb9fb9253b2a9ce037a1599589b9485904e6fdd6de97ca96e027b3ee30f` |
| `Fredoka-600.ttf`              | 600           | same                                        | `b24e06cf369855020b81ef1e9639f7b705d5348c96df96e875fc7d78bbbc6458` |
| `Fredoka-700.ttf`              | 700           | same                                        | `c570243acd47e10bc47913bd1b0b20ba36f53ec8663e093f02195cceaca25297` |
| `Nunito-700.ttf`               | 700           | `Nunito[wght].ttf`                          | `c3943d48246d348358a8c9cea93705d3ac83f64309876abc00d7b02275acea01` |
| `Nunito-800.ttf`               | 800           | same                                        | `f3ca46028e63fcfe1ba08376d64219451b9eb3b7f273d6854e289456576af55e` |
| `Nunito-900.ttf`               | 900           | same                                        | `01abe18ab593161e909b10a59673b222b89899225ce254d0f4723bcc30964b21` |
| `NotoSansSymbols2-Regular.ttf` | single weight | `NotoSansSymbols2-Regular.ttf` (unmodified) | `7d5fb73b7ca67a6798101741f5d280a3d016a56a197afcd4199dbb57b4b82a21` |
| `NotoSansSymbols-Regular.ttf`  | 400           | `NotoSansSymbols[wght].ttf`                 | `f7a733b390a5cc37bf60cd3131aa844ad0bef90e3f5d56cae60fec30c39617d2` |

There are deliberately **no** `Fredoka-800/900` and **no** `Nunito-400` files: `fetch-assets.mjs` cannot produce them, and neither the harness nor Unity may reference them. Instances are generated with `SOURCE_DATE_EPOCH=1767225600` so regeneration is byte-identical.

**Evidence (retrieved 2026-09-30).** The exact `@import` URL of `styles.css` line 6, `https://fonts.googleapis.com/css2?family=Fredoka:wght@400;600;700;800;900&family=Nunito:wght@700;800;900&display=swap`, was requested with a desktop Chrome `User-Agent`. Google answered HTTP 200 (9243 bytes) with `@font-face` blocks for Fredoka 400, 600, 700 and Nunito 700, 800, 900 only (the requested Fredoka 800 and 900 are silently dropped, and Nunito 400 was never requested). The raw response is committed as `evidence/google-fonts-css2.css`. Each family weight arrives as unicode-range subsets (hebrew, latin-ext, latin for Fredoka; cyrillic-ext, cyrillic, vietnamese, latin-ext, latin for Nunito); the pinned static files are full-coverage substitutes for those subsets and the app only uses Latin and symbol code points.

### 5.0 Effective faces (CSS font matching)

Matching follows CSS Fonts 4 section 5.2 over the available faces of the family:

- desired weight in 400-500 inclusive: try the desired weight, then ascending weights up to 500, then lighter weights descending, then heavier weights above 500 ascending;
- desired weight below 400: weights at or below desired descending, then heavier weights ascending;
- desired weight above 500: weights at or above desired ascending, then lighter weights descending.

The set of weights the app asks for was found by scanning `styles.css`, the inline `style` attributes of `index.html` and `main.js` (the declared `font-weight` values are only 700 (9 times), 800 (24) and 900 (22); the 400 comes from the initial value and from `ctx.font` strings without a weight), `engine/renderer.js` `ctx.font` strings, and the computed style of every text-bearing element of every page, store tab, world tab, modal and the toast (`scripts/font-usage-scan.mjs`, which also exports the reference implementation of the matching function and writes `out/font-usage.json`). Weight 600 is never requested by the app; its face is still served and built (Google serves it, and it is part of the face set).

Resolution table for every family and weight combination the app requests:

| Family  | Requested weight | Effective face | Why                                                  |
| ------- | ---------------- | -------------- | ---------------------------------------------------- |
| Fredoka | 400              | 400            | exact                                                |
| Fredoka | 700              | 700            | exact                                                |
| Fredoka | 800              | **700**        | above 500: nothing heavier than 700, nearest lighter |
| Fredoka | 900              | **700**        | same                                                 |
| Nunito  | 400              | **700**        | no 400/500 face, nothing lighter: nearest heavier    |
| Nunito  | 700              | 700            | exact                                                |
| Nunito  | 800              | 800            | exact                                                |
| Nunito  | 900              | 900            | exact                                                |

Weight 600 would resolve to Fredoka 600 and, for Nunito, to 700 (heavier ascending; no 600 face) but is not requested.

Where the combinations occur in the DOM (`font-family` computed, from the scan; selectors are examples):

| Family stack                 | Requested weight            | Face | Examples                                                                                                                                                       |
| ---------------------------- | --------------------------- | ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Fredoka, cursive, sans-serif | 400                         | 400  | `.stat-label`                                                                                                                                                  |
| Fredoka                      | 700                         | 700  | `.tab-btn` (store and world tabs), shop section titles (`Donut Rings`, `Nibbles Hats`, `Skies`), `Cute Cosmic Janitor`                                         |
| Fredoka                      | 800                         | 700  | `.home-logo-subtitle`, diorama ring label, mission world and goal pills, dock tab labels                                                                       |
| Fredoka                      | 900                         | 700  | `h1`, `.title-sub`, `.console-tag`, PLAY MISSION, the `3 NEW` badge                                                                                            |
| Nunito, sans-serif           | 400 (inherited from `body`) | 700  | `.bg-debris` and loading-screen emoji spans, any text with no explicit weight                                                                                  |
| Nunito                       | 700                         | 700  | `#loading-tip-text`, `strong`, `p`, `#defeat-message`                                                                                                          |
| Nunito                       | 800                         | 800  | daily tier labels (BRONZE, SILVER, GOLD), TODAY, inline `font-weight:800` spans                                                                                |
| Nunito                       | 900                         | 900  | LOCKED, inline `font-weight:900` spans                                                                                                                         |
| Arial (UA `button` font)     | 400                         | none | emoji-only `<span>`s inside buttons (`#home-audio-icon`, `.dock-tab-icon`, ...); the emoji is replaced by `img.pe` (6.1), so no glyph is ever drawn from Arial |

Canvas (`engine/renderer.js`; every font string ends in `, sans-serif`, rewritten per 5.2):

| Line | `ctx.font`           | Requested weight | Face    |
| ---- | -------------------- | ---------------- | ------- |
| 1053 | `bold 13px Fredoka`  | 700              | 700     |
| 1078 | `11px Nunito`        | 400              | **700** |
| 1457 | `bold 10px Fredoka`  | 700              | 700     |
| 1560 | `900 10.5px Fredoka` | 900              | **700** |
| 1647 | `bold 10px Fredoka`  | 700              | 700     |
| 1720 | `bold 9.5px Fredoka` | 700              | 700     |
| 1942 | `900 16px Fredoka`   | 900              | **700** |

In summary: Nunito 400 draws with the Nunito 700 outlines; Fredoka 800 and 900 draw with the Fredoka 700 outlines; Fredoka 600 is built but never selected by the app.

### 5.1 Glyph coverage and fallback chain

Characters checked (`scripts/font-coverage.py`): every non-ASCII, non-emoji code point in `index.html`, `main.js`, `styles.css` and `engine/*.js`, plus a required set. The scan of the sources found exactly U+00D7 `×`, U+2022 `•`, U+2190 `←`, U+2713 `✓` and U+25B6 `▶` (text presentation, not an emoji, section 6). U+2014 `—` and U+2019 `’` do **not** occur in the web sources; they stay in the required set as defensive coverage for future copy changes and are covered by Fredoka and Nunito. Digits and Latin are covered too.

| Code point                   | Fredoka | Nunito | Noto Sans Symbols 2 | Noto Sans Symbols |
| ---------------------------- | ------- | ------ | ------------------- | ----------------- |
| `×` `—` `’` `•` digits Latin | yes     | yes    | n/a                 | n/a               |
| `←` U+2190                   | no      | no     | no                  | **yes**           |
| `✓` U+2713                   | no      | no     | **yes**             | no                |
| `▶` U+25B6                   | no      | no     | **yes**             | no                |

Fallback chain, identical on both sides, in order:

1. Primary: `Fredoka` (display text, `--font-bubble`) or `Nunito` (body text, `--font-body`) at the effective face of the requested weight (5.0).
2. `Noto Sans Symbols 2` (`NotoSansSymbols2-Regular.ttf`): covers `✓` and `▶`, and is a broad symbol font that covers future additions in the same blocks.
3. `Noto Sans Symbols` (`NotoSansSymbols-Regular.ttf`, wght 400 instance): covers `←` (and `→`), which Symbols 2 lacks.

**The Noto symbol chain is a deliberate determinism choice, not browser fidelity.** A real device would fall through to whatever system symbol font it has (Segoe UI Symbol, Apple Symbols, DejaVu, ...), which differs per machine. The reference pins one chain instead so both sides draw identical glyphs. This is the only place where the reference intentionally differs from an unmodified browser's font selection.

Both fallbacks are OFL, from `google/fonts` at the pinned commit. No system font is ever consulted for a web-used character. The chain has no weight variants: the fallback fonts are declared with `font-weight: 100 900` in CSS (no synthetic bold), and Unity uses the single TMP asset for every weight.

### 5.2 Web delivery

- Intercept the request to `https://fonts.googleapis.com/css2?...` (it comes from the `@import` on `styles.css` line 6) and answer with local `@font-face` rules, `Content-Type: text/css`:
  - family `Fredoka`, **exactly** three rules, weights 400, 600 and 700, `src: url(<origin>/__parity/fonts/Fredoka-<W>.ttf) format('truetype')`, `font-weight: <W>`, `font-display: block`;
  - family `Nunito`, **exactly** three rules, weights 700, 800 and 900, same shape;
  - no other `Fredoka` or `Nunito` rule is ever emitted (no 800/900 for Fredoka, no 400 for Nunito) and `font-synthesis: none` stays on, so Chrome's own matching (5.0) picks the effective face and never fakes a bold;
  - family `Noto Sans Symbols 2` and `Noto Sans Symbols`, `font-weight: 100 900`, `font-display: block`.
- Serve `/__parity/fonts/*` and `/__parity/emoji/*` from `assets/` via `page.route`. The app itself is served from `web/` under a fake origin (`http://parity.test/`) with `page.route`, because `main.js` is an ES module and does not load from `file://`.
- Block every other request (`route.abort()`).
- **Injection order.** All harness `<style>` injections (the fonts `:root` variables above, the safe-area fallback of section 3, `img.pe` of 6.1) go into one `<style id="parity-overrides">` appended as the **last child of `<head>` after `styles.css` has loaded** (the `<link rel="stylesheet" href="styles.css">` is the last element of `<head>` in `index.html`; append in a `DOMContentLoaded` listener registered by the init script, and verify `document.styleSheets` ends with the harness sheet). `:root`, `.hud-top-mobile` and the other fallback selectors have the same specificity as the originals, so only later source order makes them win; an earlier `<style>` would silently lose. The CDP safe-area override (section 3) is primary, the CSS fallback secondary.
- **Body font.** The computed `font-family` of `body` is `var(--font-body)` and the app sets no `font-weight` on `body`, so body text requests Nunito **400** (verified in `styles.css` lines 36-45). The app's own `@import` asks Google for Nunito 700/800/900 only (line 6), so a real browser draws that text with the Nunito 700 face. The harness serves no Nunito 400 face, so Chrome's matching does the same (5.0).
- Chain injection (the chain cannot come from the app CSS): add a stylesheet `:root{--font-bubble:"Fredoka","Noto Sans Symbols 2","Noto Sans Symbols",sans-serif;--font-body:"Nunito","Noto Sans Symbols 2","Noto Sans Symbols",sans-serif} html{font-synthesis:none}`, and wrap the `CanvasRenderingContext2D.font` setter so a trailing `, sans-serif` after `Fredoka` or `Nunito` becomes `, "Noto Sans Symbols 2", "Noto Sans Symbols", sans-serif` (`renderer.js` sets e.g. `"bold 13px Fredoka, sans-serif"`).
- `boot` waits for `document.fonts.ready`, then verifies with `document.fonts.check` / `FontFace.status` that every served face is `loaded`: Fredoka 400/600/700, Nunito 700/800/900, both symbol fonts. Any miss fails the run. It also asserts the effective faces of 5.0: for each (family, weight) pair of the 5.0 table, `document.fonts.load('<weight> 16px <family>')` resolves to exactly the face listed (check `FontFace.weight` of the returned face), so a stray extra `@font-face` fails the run.

### 5.3 Unity

TMP SDF assets are generated by `OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll` from the same files in `assets/fonts/` (synced to `Assets/Fonts/*.ttf`) into `Assets/Resources/Fonts/TMP/<FontName> SDF.asset`: sampling point size 96, padding 12, render mode `SDFAA`, atlas 4096 x 4096, `AtlasPopulationMode.Dynamic`, multi-atlas enabled. The assets built are exactly the face set of section 5: `Fredoka-400/600/700`, `Nunito-700/800/900`, plus the fallback fonts `NotoSansSymbols2-Regular` and `NotoSansSymbols-Regular`. Each Fredoka/Nunito asset has fallback table `[NotoSansSymbols2-Regular SDF, NotoSansSymbols-Regular SDF]` in that order. `BuildAll` also deletes obsolete outputs of earlier layouts (the old `Assets/Fonts/TMP` folder, `Fredoka-800/900` and `Nunito-400` fonts and assets), so the project can never contain a face the web does not have.

**Registry decision: Resources, no ScriptableObject.** The TMP assets live under `Assets/Resources/Fonts/TMP/` and are loaded by name with `Resources.Load<TMP_FontAsset>("Fonts/TMP/<Family>-<faceWeight> SDF")`. This matches how emoji are already loaded (`Assets/Resources/Emojis`), needs no extra asset to keep in sync, and the file names are derived from the same constant face table that drives the builder, so builder and runtime cannot disagree.

**`OrbitalSalvage.Render.ParityFonts`** (`Assets/Scripts/Render/ParityFonts.cs`) is the only way gameplay and UI code selects a TMP font:

- `TMP_FontAsset Resolve(string family, int cssWeight)`: `family` is `Fredoka` or `Nunito`; `cssWeight` is the CSS weight the web code requests (use `ParseCssWeight` for `normal`/`bold`/numeric tokens, e.g. a canvas font string). It runs the 5.0 matching algorithm over that family's face weights (`FaceWeights`: Fredoka 400/600/700, Nunito 700/800/900) and loads and caches the asset of the resulting face. Examples: `Resolve("Nunito", 400)` returns `Nunito-700 SDF`; `Resolve("Fredoka", 900)` returns `Fredoka-700 SDF`.
- `int MatchWeight(int desired, IReadOnlyList<int> available)` implements exactly the three rules of 5.0 (400-500: desired, up to 500, then lighter descending, then heavier ascending; below 400: lighter descending, then heavier ascending; above 500: heavier ascending, then lighter descending) and `int ResolveWeight(string family, int cssWeight)` returns the face weight (or -1 for an unknown family).
- Resolved assets already carry the symbol fallback table, so no per-call fallback handling is needed. Unity never applies synthetic bold (`FontStyles.Bold` is not used for weight selection).
- Verification: `ParityAssetBuilder.VerifyFonts` (batchmode, `-executeMethod OrbitalSalvage.Editor.ParityAssetBuilder.VerifyFonts`) resolves every pair of the 5.0 table and exits with code 1 if any pair returns a different face than listed there.

## 6. Emoji substitution

Native emoji fonts are never used. Both sides draw the pinned Microsoft Fluent UI Emoji 3D PNG for every emoji grapheme.

Classification (both sides, identical): split text with `Intl.Segmenter` (grapheme granularity). A grapheme is emoji iff it contains an `Extended_Pictographic` code point AND (it contains U+FE0F OR its first code point has `Emoji_Presentation`). Text-presentation symbols without U+FE0F (`▶`, `✓`, `←`, `×`, `•`) are not emoji and render with text fonts. Unity implements the same rule over its own tables generated from the same Unicode data (`emoji_index.json` already lists every grapheme the app uses).

Files: `assets/emoji/<codepoints-lowercase-hyphen-joined-without-fe0f>.png`, 256 x 256, mapped by `assets/emoji/index.json` (`grapheme -> { file, folder, style }`). Unity copies: `Assets/Resources/Emojis/3D/<codepoints>.png` and `Assets/Resources/Emojis/emoji_index.json`.

The web prototype uses 57 distinct emoji graphemes (full list in section 14).

### 6.1 DOM

- Every emoji grapheme in a text node is replaced by `<img class="pe" src="/__parity/emoji/<file>.png" alt="<grapheme>">`.
- CSS (in the `parity-overrides` sheet, injection order in 5.2): `img.pe{width:1em;height:1em;vertical-align:-0.125em;display:inline-block;margin:0;padding:0;border:0}`.
- Applied before first paint and on every DOM mutation: a `MutationObserver` over `document` with `childList`, `subtree`, `characterData` and `attributes`; each mutation re-scans affected text nodes (including nodes created from `innerHTML`, `textContent` and `insertAdjacentHTML`). `img.pe` elements are skipped by the scan.
- `Node.prototype.textContent` (getter only) is patched on elements and fragments to return the concatenation of text nodes with every `img.pe` replaced by its `alt` grapheme, so app code that reads `textContent` back (`main.js` lines 303-308: the mascot click saves `#home-diorama-face.textContent`, shows `🥰`, and writes the saved value back 600 ms later; without the patch the face would be restored empty) sees the original string and not a string with the emoji removed. The setter is untouched.
- All 57 images are preloaded and decoded at boot; `capture` waits until every `img.pe` has `complete` and non-zero `naturalWidth`.

### 6.2 Canvas

`fillText` and `strokeText` are wrapped. For a string `s` drawn at `(x, y)`:

0. **Font parsing.** `fontPx` is the number immediately before `px` in `ctx.font` (regex `(\d+(?:\.\d+)?)px`), e.g. `900 10.5px Fredoka, sans-serif` -> `10.5`, `bold 13px Fredoka, sans-serif` -> `13`. The weight token gives the requested weight: `bold` = 700, a numeric token (`900`) is that weight, no token = 400. The face actually drawn is the effective face of section 5.0 (e.g. `900 ... Fredoka` draws the Fredoka 700 face, `11px Nunito` draws the Nunito 700 face; Unity: `ParityFonts.Resolve`). `fontPx` is in CSS px **of the current local coordinate system**: the context transform `setTransform(dpr, 0, 0, dpr, 0, 0)` with `dpr = min(devicePixelRatio, 3) = 3` (`renderer.js` lines 79 and 320) and any further `translate`/`scale`/`rotate` already apply to `drawImage`, so do not multiply `fontPx` by `dpr` or by any scale. (Example: floating text is drawn under `ctx.scale(ft.scale, ft.scale)` and its emoji scale with it.)
1. Split `s` into runs: maximal runs of non-emoji text, and single emoji graphemes.
2. `total = sum(ctx.measureText(textRun).width over text runs) + (#emoji * fontPx)`. The app never calls `measureText` (verified: no occurrence in the web sources); the wrapper calls the context's own, unwrapped `measureText` for the text runs, so it uses the font (including the fallback chain of 5.2) that the run is drawn with. Leading and trailing spaces of a text run are part of its width.
3. Start x from `textAlign`: `left`/`start` -> `x`; `center` -> `x - total/2`; `right`/`end` -> `x - total`.
4. Walk the runs left to right: text runs are drawn with the original call at the running x with `textAlign` temporarily `'left'` (restored afterwards); each emoji is `drawImage`d as a `fontPx x fontPx` square at the running x and advances by `fontPx`.
5. Vertical placement of each emoji by `textBaseline`: `top` -> square top = `y`; `middle` -> square center = `y`; `alphabetic`, `bottom`, `ideographic` -> square bottom = `y + 0.125 * fontPx`. The gate and hub labels use `middle` (square centre = `y`, `renderer.js` lines 1055, 1459, 1562, 1649, 1722). The floating text (`renderer.js` lines 1941-1950) never sets `textBaseline`, and every `textBaseline = "middle"` assignment in the file sits inside its own `ctx.save()` / `ctx.restore()` pair, so the floating text uses the default `alphabetic` baseline: square bottom = `y + 0.125 * fontPx`.
6. `strokeText` draws the **text runs only** and never an emoji. `fillText` draws the text runs and the emoji. (The app draws floating text with `strokeText` first, then `fillText` of the same string: the outline covers the text runs, the fill draws text and emoji once.) A string drawn only with `strokeText` therefore shows no emoji.
7. The canvas `globalAlpha`, transform and clip apply to the images as usual; emoji images are drawn with the context's current `fillStyle` ignored.

Every `fillText` / `strokeText` call site in the web sources is listed in Appendix B (nothing outside `engine/renderer.js` draws canvas text).

**Harness self-check (1.4).** The wrapper logs every call of the current frame (text, kind, emoji graphemes in the string, images drawn, pictographs left to the font). `capture` of a game-page state fails when the frame drew no canvas text, when a `fillText` string contains an emoji grapheme that was not drawn as an image, when a `strokeText` drew an image, when a pictograph was left to the font, or when the frame's strings contain emoji and no emoji image was drawn at all. The per-frame stats are written to `meta.canvas` of the state's meta. Appendix B lists the dedicated state for each call site.

Unity: the Canvas2D layer implements the same run algorithm (`canvas-conformance/` holds the conformance scenes). TMP emoji are sprites in a TMP sprite asset with glyph width = height = 1 em, baseline offset matching vertical-align `-0.125em` (the glyph bottom sits 0.125 em below the baseline), advance 1 em.

### 6.3 Emoji sprite import (Unity, 1.5)

Every `Assets/Resources/Emojis/3D/*.png` is imported by `ParityAssetBuilder.ConfigureEmojiImporters` as a Sprite (single) with: **uncompressed RGBA32 on every platform** (an explicit overridden `RGBA32` entry for `Standalone`, `iPhone`, `Android`, `WebGL` and `Server` plus an uncompressed default entry; a stale platform entry alone used to leave the editor and Windows player on DXT), no mipmaps, bilinear filter, clamp, sRGB, `alphaIsTransparency`, not readable, no crunch, no NPOT scaling. `maxTextureSize` is 512, not 256: the Fluent 3D PNGs are 256 px wide, but `1fa90` (ringed planet) is 256 x 264, and a 256 cap would resample it. `OrbitalSalvage.Render.Tests.EmojiImportTests` asserts these settings and that the loaded texture format is `RGBA32` with one mip.

### 6.4 Text parity bar on synthetic text scenes (OPEN, 1.5)

**OPEN - awaiting lead decision.** SPEC 6.1 mandates TextMeshPro SDF text. SDF glyphs rebuilt from a 96 px atlas cannot match FreeType/Skia coverage exactly (one- to two-pixel ring on glyph edges); the `TextBias`/`TextGamma` sweep is flat. The gameplay streams pass the bar (99.2 - 99.7 %), but the three canvas conformance scenes that are almost entirely text fail the 99.0 % pixel bar (`tools/parity/reports/spec-requests/processed/canvas.md`):

| scene | pix16 % | SSIM |
|---|---:|---:|
| text_align_baseline | 97.99 | 0.9876 |
| text_emoji | 98.61 | 0.9916 |
| text_fonts | 95.95 | 0.9746 |

Options put to the lead: (a) treat the synthetic text-only scenes as informational and gate on the gameplay streams, or (b) allow an outline-based glyph rasteriser (glyph outlines read from the .ttf/.otf files and filled by the same exact-area pipeline as every other canvas path) instead of the TMP SDF path, which needs a font-parsing component. Until decided, the 99.0 % bar of section 2 stays as written, those three scenes are reported as FAIL, and no implementation work toward (b) starts.

## 7. File and directory layout

```
tools/parity/
  SPEC.md
  package.json  package-lock.json  .gitignore
  states.json                      (state list, authored by the states team; section 8)
  tracked-classes.json             (class list for layout JSON; section 9)
  assets/                          (pinned fonts + emoji; manifest.json)
  evidence/                        (google-fonts-css2.css: raw Google Fonts response, 2026-09-30; section 5)
  scripts/                         (fetch-assets.mjs, emoji-scan.mjs, font-coverage.py, font-usage-scan.mjs, probes)
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
- `traces/<id>.trace.json` exists for two kinds of producer: every state writes `traces/<stateId>.trace.json` (once, from its first listed profile; both profiles produce the same events) and every entry of `sequences.json` (8.2) writes `traces/<sequenceId>.trace.json`. State ids and sequence ids are disjoint.
- `NN` is a zero-padded two-digit iteration number.

## 8. `states.json`

Array of states:

```json
[
  {
    "id": "home_default",
    "description": "Home screen after loading, default profile",
    "profiles": ["zero", "iphone"],
    "steps": [{ "op": "boot" }, { "op": "capture" }]
  }
]
```

Fields: `id` (snake_case, unique), `description`, `profiles` (subset of `["zero","iphone"]`), `steps` (ordered, non-empty, exactly one `capture`, last). Each (state, profile) runs in a fresh browser context / fresh Unity app start with empty storage. Ops are declarative; neither side executes raw JS from the file.

| Op              | Fields                                                                                                                        | Web semantics (exact)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `boot`          | `completeLoading` (bool, default `true`)                                                                                      | Apply the profile's safe area (section 3); install clock and pause (4.2); register routes (5.2); inject init scripts in this order, all before app scripts: clock normalisation and rAF (4.4), audio stub (4.6), RNG, timer / event-listener / animation registry wrappers (4.5), emoji, canvas wrappers, fonts chain, tracing; `goto http://parity.test/index.html` and wait for `load`; append the `parity-overrides` sheet (5.2); wait `document.fonts.ready` and verify fonts (5.2); wait for emoji images (6.1); assert `window.app.lastTime === 0` (4.4); stamp the load-time animations at 0 (4.5). If `completeLoading` is true, step `runFor(16)` until `app.currentPage === "home"` (at most 2000 steps, fails otherwise), then `advance(240)` so `pageEnter` settles, then run the sweep (4.5). Probe with the pinned seed: `showPage("home")` runs at `performance.now() = 1310`, the stepping sees it after 82 frames at 1312, and boot ends at 1552. With `completeLoading: false` the loading screen is left in its initial state and boot ends at `performance.now() = 0`. Exactly one `boot`, first.                                                   |
| `showPage`      | `page` (one of `loading`,`home`,`game`,`maps`,`daily`,`shop`,`achievements`,`settings`)                                       | `window.app.showPage(page)`; no time advance. `game` schedules the 3500 ms gesture-hint timer as in the app.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| `setProfile`    | `path` (dotted, into `app.economy.profile`), `value` (JSON)                                                                   | Walks `path` from `app.economy.profile` and assigns `value` in memory. Does not call `saveProfile`, does not re-render. A later `showPage`, `call` of a render method, or `call updateAllCreditDisplays` re-renders.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| `setApp`        | `field`, `value` (JSON)                                                                                                       | `app[field] = value`. No side effects: plain field assignment only. In particular `setApp currentStoreTab` changes only the field that the next `renderStorePage()` reads (`main.js` line 1260). It does **not** reproduce the click handler on `.tab-btn` (lines 589-595), which also moves the `active` class between the tab buttons, so after `setApp currentStoreTab` the highlighted tab button still belongs to the old tab. States that show the store must therefore select the tab with `clickSelector` (below), not `setApp`. Legal `currentStoreTab` values (keys of `COSMETIC_CATALOG`, `index.html` `data-tab`): `rings` (default), `singularities`, `skyboxes`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| `clickSelector` | `selector` (one of the forms in section 8.1)                                                                                  | For elements that have no id (store tabs, world tabs, level tiles, shop buttons): `document.querySelector(selector)`, then exactly as `click` (centre of `getBoundingClientRect()`, `mouse.move`, `down`, `up`, no auto-scroll, no actionability wait, fails if missing or not visible, no time advance). It exercises the real handler, including the `.tab-btn` side effects (`active` class move, `currentStoreTab`, `renderStorePage()`) and `world-tab-btn` (`selectedWorldIndex`, `renderSectorsPage()`). The Unity element id is derived from the selector by the rule in section 8.1.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| `scroll`        | `target` (DOM id; optional, default: the single `.page-scroll-body` inside the visible `.app-page.page-active`), `y` (CSS px) | Optional op. Sets `scrollTop = y` on the target and fails the state when the target cannot scroll (`scrollTop` stays 0 for `y > 0`): at 430 x 932 no page of the app overflows its `.page-scroll-body` (measured `scrollHeight == clientHeight` on maps, daily, shop, trophies and settings, 1.4), so no current state uses `scroll`. `.page-scroll-body` is the scroller of every sub-page (rule at `styles.css` line 1031, `overflow-y: auto`; one per page at `index.html` lines 75, 210, 226, 242, 295, 313; only `#sectors-page-container` and `#daily-page-container` have ids); `body` and `#mobile-frame` do not scroll (`overflow: hidden`). No time advance. Unity scrolls its equivalent `ScrollRect` to the same content offset in CSS px.                                                                                                                                                                                                                                                                                                                                                                                                                  |
| `call`          | `method`, `args` (JSON array)                                                                                                 | `app[method](...args)`, or `app[target][method](...args)` when the optional `target` is `renderer` or `physics` (1.4; `target` defaults to `app`). `method` must be in the allow-list of its target: `renderer`: `addFloatingText`; `physics`: `onGatePassed`, `onDebrisSwallowed`, `onDebrisExploded`, `onBoostTriggered` (the app's own callbacks, used to reach the floating-text call sites of Appendix B without playing for them). For `app`, the allow-list is: `showPage`, `showToast`, `updateAllCreditDisplays`, `updateShopDockBadge`, `updateDailyDockBadge`, `updateHomeView`, `renderSectorsPage`, `renderDailyPage`, `renderStorePage`, `renderAchievementsPage`, `syncSettingsUI`, `restartSector`, `nextSector`, `handleLevelVictory`, `handleLevelFailure`, `exitGameToHome`, `resetGameplaySessionUi`, `updateQuotaDisplay`, `dismissGestureHint`, `initLayout`, `triggerHaptic`. Any other name fails the state.                                                                                                                                                                                                                                    |
| `click`         | `target` (DOM id)                                                                                                             | Read `getBoundingClientRect()` of `#target`, take the center, then `page.mouse.move(x,y)`, `mouse.down()`, `mouse.up()`. No auto-scroll, no actionability wait. Fails if the element is missing or not visible. No time advance.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| `pointer`       | `kind` (`down`/`move`/`up`), `x`, `y` (CSS px, viewport coordinates)                                                          | `page.mouse.move(x,y)` then, for `down`, `mouse.down()`; for `up`, `mouse.up()`; for `move` only the move (the button state stays as set by earlier ops). Unity injects the same event at `(x*3, 2796 - y*3)` screen px through the EventSystem.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| `key`           | `key` (Playwright key name, e.g. `a`, `ArrowLeft`, `Space`, `Escape`)                                                         | `page.keyboard.press(key)` (keydown then keyup on the focused document).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| `advance`       | `ms` (integer >= 0)                                                                                                           | `page.clock.runFor(ms)` as one call, then the diff and sweep of the animation clock (4.5). Frames fire on the normalised 16 ms grid (4.4); timers fire at their due times; the registry is diffed after every callback inside the call. The harness may split into 16 ms steps internally but the observable effect must be identical.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| `loadSector`    | `index` (0-based into `app.sectors`, 0-49)                                                                                    | `app.loadSector(app.sectors[index], false)` **and then `app.currentSectorIndex = index`**; no toast; no time advance. `loadSector` itself never sets `currentSectorIndex` (`main.js` lines 833-854; only the map-tile click, line 1063, sets it), but `nextSector()` computes `(this.currentSectorIndex + 1) % this.sectors.length` (lines 880-882), so without the assignment `nextSector` would continue from sector 0. Levels 1 / 11 / 21 / 31 / 41 (the first level of each world) have indexes 0 / 10 / 20 / 30 / 40 (`sectors[l - 1]`, `levelRange` in `engine/maze.js`). Gameplay states: `loadSector`, then `showPage("game")` (this runs `initLayout()` and schedules the 3500 ms hint timer), then `call dismissGestureHint` (exists, `main.js` line 345; it adds `.hidden` to `#gesture-hint`) and `advance(400)` for its 400 ms transition; if the hint is to stay visible, skip those two. At least one `advance(16)` is required before `capture` so a frame runs `renderer.render` for the new sector (`currentPage === "game"`, section 4.4). The pending 3500 ms timer later calls `dismissGestureHint` again, which is a no-op once `.hidden` is set. |
| `holdPressed`   | `target` (DOM id)                                                                                                             | `mouse.move` to the centre of `#target`, `mouse.down()` and **keep it held**, then `advance(150)` internally (the longest `:active` transition is 150 ms, Appendix A.3) so `:active` styling has settled. The state's `capture` then runs while the button is still down; `mouse.up()` is sent only after `capture` has written the PNG, the layout JSON and the trace (the resulting `click` event is not captured). No explicit `advance` is needed after `holdPressed`. Unity holds the simulated pointer the same way and releases it after its capture.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| `capture`       | none                                                                                                                          | Exactly one per state, last. Wait for fonts and `img.pe` complete, run the diff and sweep of the animation clock (4.5), write layout JSON (section 9) and the trace (section 10), then `page.screenshot({ animations: 'allow', caret: 'hide' })` to `out/web/<stateId>__<profile>.png`, converted to 8-bit RGB. The sweep is the last thing before the pixels are read. On the game page the canvas self-check of 6.2 runs first. Optional assertions (1.4), each failing the state when false: `expectEmoji` (array of graphemes that must have been drawn as images in the captured frame), `expectFloating` (array of floating-text strings that must be on screen), `expectFraction` (array of `{selector, value, tol}`: `getBoundingClientRect().width / parentElement.clientWidth` of the element must equal `value` within `tol`, default 0.01; this is how a settled `width` transition is asserted).                                                                                                                                                                                                                                                           |

Unity implements each op over its own runtime with the same observable result: ids in `click`/`holdPressed`/layout are Unity element names that equal the web DOM ids (section 8.1); `app`/`economy` field and method names are mapped one-to-one by the Unity runner.

**Click targeting (1.3).** `click` and `clickSelector` accept optional `fx` and `fy` (default `0.5`, fractions of the border box measured from the top-left corner): the pointer goes to `(rect.x + rect.w * fx, rect.y + rect.h * fy)`. This is how range sliders are driven (`click settings-sfx-slider fx=0.2` sets the value from the pointer position; the harness never writes `.value`). A click fails the step when the point is covered by an element that is neither the target, a descendant of it, nor an ancestor of it (`document.elementFromPoint`), so a state can never silently click a modal backdrop instead of the button under it. A target that is an `input[type=checkbox]` uses the rect of its enclosing `label` (the real input is visually hidden); the click therefore lands on the label exactly as a user's tap does. After the `up`, `click` and `clickSelector` move the mouse to `(-1, -1)` (outside the viewport) so that no element is `:hover` afterwards: Chromium re-evaluates `:hover` under a resting cursor from a real-time timer after layout changes, which the fake clock cannot order, and a measured full-run comparison showed the dock tab under the cursor flipping between runs (`settings_reset_confirmed_toast`). Unity has no hover; `:hover` rules in `spec/*.json` are informational only. `pointer` and `holdPressed` do not park the pointer.

### 8.1 Unity id mapping

1. **DOM ids map 1:1.** Every element with an `id` attribute (static in `index.html` or generated by a template string in `main.js`, such as `btn-claim-master-bonus`, `daily-master-box`, `pause-sound-icon`) is a Unity GameObject whose `name` is that id, exactly, case-sensitive. `click`, `holdPressed`, `scroll`, `call` arguments and the `id` field of layout JSON (section 9) use these names.
2. **Class-only elements have two identifiers, both deterministic.**
   - Layout JSON key (section 9): `<first tracked class>#<ordinal>`, unchanged.
   - Click id for `clickSelector`: the selector is restricted to the forms below, and the Unity GameObject name is derived from it. The runner finds the GameObject by that name; the web finds it by `document.querySelector(selector)`.

| `clickSelector` form                                                             | Unity name         | Example (web selector -> Unity name)                                                                                            |
| -------------------------------------------------------------------------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
| `.<class>[data-<attr>=<value>]` (exactly one attribute, quotes optional)         | `<class>--<value>` | `.tab-btn[data-tab=skyboxes]` -> `tab-btn--skyboxes`; `.btn-buy-item[data-id=ring_candyland]` -> `btn-buy-item--ring_candyland` |
| `.<class>:nth-child(<n>)` (1-based position among the parent's element children) | `<class>--<n>`     | `.world-tab-btn:nth-child(3)` -> `world-tab-btn--3`; `.level-tile:nth-child(1)` -> `level-tile--1`                              |
| `#<id>`                                                                          | `<id>`             | same as `click`                                                                                                                 |

Any character of `<value>` outside `[A-Za-z0-9_-]` is replaced by `-`. Selectors outside these forms fail the state. Notes on the DOM: the three store tabs are `.tab-btn[data-tab=rings|singularities|skyboxes]` (the item buttons `.btn-equip-item` / `.btn-buy-item` also carry `data-tab` but not the class `tab-btn`, so always include the class); the world tabs `.world-tab-btn` have **no** data attributes, they are generated in world order (`main.js` lines 993-1005), so `:nth-child(n)` is the world index; the level tiles are generated in level order inside `.level-grid-50`, so `:nth-child(n)` is the n-th level of the selected world.

### 8.2 `sequences.json` (1.3)

`tools/parity/sequences.json` is an array of `{ "id", "description", "steps" }` with the op vocabulary of the table above, except that `capture` and `holdPressed` are not allowed and the first step is `boot`. Each sequence runs once, in the `zero` profile, in a fresh context and produces only a trace (section 10): `out/<side>/traces/<id>.trace.json`. Sequences are flat lists of primitive ops (no loops or macros) so the Unity runner replays them with the same op implementations as states. The scripted drag programs are expanded `pointer` ops (`down`, repeated `move` + `advance`, `up`) at fixed positions and 16 ms multiples; a sequence may carry `expect`, a list of `{ "type", "name", "args"? }` events that must appear in its web trace (`args` compared exactly when present), otherwise the web run fails; `play_sector_1` expects the `modal-debrief open` event, so a drag program that stops winning is noticed.

Defined sequences: `boot_to_home`, `play_sector_1`, `defeat_sector_1` (sector 1 cannot be lost with no input, so the out-of-scrap failure is forced with `call handleLevelFailure` after 5 s idle), `shop_purchase_and_equip`, `daily_gift_claim`, `settings_toggles`, `pause_resume_exit`.

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
- `rect`: `getBoundingClientRect()` border box in CSS px in **viewport coordinates** (origin at the top-left of the 430 x 932 viewport; no scroll offset is added), unrounded numbers. Content scrolled inside `.page-scroll-body` (the scroller of the sub-pages, section 8 `scroll` op) therefore reports negative `y` or `y` beyond 932 when it is outside the viewport; matching still applies to elements with `visible: true`. Unity converts its RectTransforms to the same viewport-coordinate CSS px.
- `visible`: true iff no ancestor-or-self has `display: none`, computed `visibility` is `visible`, and the box has `w > 0` and `h > 0`.
- `opacity`: the element's own computed opacity (not cumulative).
- `text`: element text with each `img.pe` replaced by its `alt` grapheme, whitespace collapsed, trimmed, at most 200 characters.
- `classes`: the full class list of the element (not filtered), in attribute order.
- Matching: by `id`; elements without id are keyed `<first tracked class>#<ordinal among elements sharing that class in document order>`. Unity emits the same ids and keys. Only elements with `visible: true` on the web side must match within 3 px; a visibility disagreement is a failure.
- Unity emits the same schema from its RectTransforms, converted to the same CSS-px viewport coordinates (origin top-left, y down).

## 10. Trace JSON

`out/<side>/traces/<sequenceId>.trace.json`:

```json
{
  "sequence": "pause_resume",
  "events": [{ "t": 1310, "type": "page", "name": "home", "args": [] }]
}
```

- `t`: integer ms since boot on the frozen clock. The time base is the normalised `performance.now()` of 4.4, whose origin is the start of the document (exactly 0 at app construction; frames are at multiples of 16). `t` is the `performance.now()` read inside the callback or handler that produced the event (for the loading sequence `page: home` is `1310`, the time of the timer, not the 1312 at which `boot`'s stepping notices it). Unity uses its virtual clock with frame k at 16k ms.
- Audio events additionally carry fake-clock seconds in `args` (4.6), rounded to 1e-6.
- `type` is one of `vibrate`, `audio`, `page`, `modal`, `toast`, `timer`, `credits`.

| Type      | `name`                                                                                                             | `args`                    | Web capture point                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| --------- | ------------------------------------------------------------------------------------------------------------------ | ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `vibrate` | `vibrate`                                                                                                          | `[ms]`                    | wrapped `navigator.vibrate` (the wrapper reports success and returns `true`)                                                                                                                                                                                                                                                                                                                                                                              |
| `audio`   | `AudioEngine` method name                                                                                          | JSON-cloned arguments     | every call of a public (non-underscore) method on `app.audio`, wrapped on the instance, recorded even when the method returns early (no context, muted, cooldown)                                                                                                                                                                                                                                                                                         |
| `audio`   | `ctx.create`, `ctx.resume`, `node.create`, `node.set`, `node.connect`, `node.start`, `node.stop`, `param.<method>` | table in 4.6              | the `ParityAudioContext` stub (4.6): every node creation, param automation call and start/stop, with times in fake-clock seconds rounded to 1e-6                                                                                                                                                                                                                                                                                                          |
| `page`    | page id                                                                                                            | `[]`                      | wrapped `app.showPage`                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| `modal`   | element id (`modal-pause`, `modal-privacy`, `modal-reset-confirm`, `modal-debrief`, `modal-defeat`)                | `["open"]` or `["close"]` | change of the `active` class on those elements, observed with a `MutationObserver` (a microtask, so it runs right after the toggling callback at the same fake time); `t` is the `performance.now()` of the callback that toggled `active`, not the next frame. The event is logged **after** the `timer` event of that same callback (for example `handleLevelFailure`'s 400 ms timeout logs `timer` and then `modal-defeat open`, both at the same `t`) |
| `toast`   | `show`                                                                                                             | `[message]`               | wrapped `app.showToast`                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| `timer`   | `timeout` or `interval`                                                                                            | `[delayMs]`               | fire of an app-created `setTimeout`/`setInterval` callback (harness-created timers, including the rAF timers of 4.4, excluded); logged when the callback starts, before any event the callback itself produces                                                                                                                                                                                                                                            |
| `credits` | `set`                                                                                                              | `[value]`                 | `app.economy.profile.credits` change, sampled on each `updateAllCreditDisplays` call                                                                                                                                                                                                                                                                                                                                                                      |

Comparison rule: identical event order, identical `type`, `name`, `args` (deep equality; numbers inside `audio` events compare with absolute tolerance 1e-6, everything else exactly), and `|t_web - t_unity| <= 16.7` ms for each event pair. Because both sides now share the normalised clock of 4.4, equal events are expected to have identical `t`; the 16.7 ms slack is a safety margin, not a licence for drift.

## 11. Unity parity player CLI

```
OrbitalSalvageParity.exe --parity <statesJson> <outDir> [--only id1,id2] [--profile zero|iphone] [--summary <dir>]
```

**Launch mode (1.5).** The Windows player (Unity 6000.4.6f1) is launched **windowed and minimised**, never with `-batchmode`, `-nographics` or a hidden window: under `-batchmode` and with a hidden window every capture of the offscreen RenderTexture comes back black, while a minimised or normal window renders correctly (measured by comparing the same states in each mode; the cause is not investigated). A minimised window needs neither focus nor visible desktop area. `tools/parity/unity/capture.ps1` passes `-screen-fullscreen 0 -screen-width 430 -screen-height 932`, the player runs with `runInBackground`, and `Screen` stays 430 x 932 while `ParityScreen.Override` reports 1290 x 2796 to layout code. A `-Batchmode` switch exists in `capture.ps1` only to reproduce the black output.

**Real-time flush (1.5).** Unity `Selectable` colour tweens run on unscaled real time, outside the virtual clock of 4.4. After every pointer op (`click`, `clickSelector`, `pointer`, `holdPressed`) the Unity runner flushes 0.25 s of real time before the capture; this is the only place where wall time is allowed on the Unity side, and it does not touch `performance.now()` equivalents, traces or `JsRandom`.

**Process mode (1.5).** The in-process run (one player process, scene reload between states) is the default. In-process runs of a subset can differ from the full run by Physics2D history (one observed 212 px difference in `game_sector_1__iphone`). `-Isolated` (one player process per (state, profile), results merged into `timings.json` / `errors.json`) is the **reference mode for byte-identity checks**; two isolated runs and the isolated-versus-in-process full smoke run were byte-identical.

- Reads `states.json` (section 8). For each selected state and profile, starts a fresh app (empty storage, `JsRandom` reset, virtual clock at 0, simulated safe area), executes steps, and writes `<outDir>/<stateId>__<profile>.png`, `<outDir>/<stateId>__<profile>.layout.json` and `<outDir>/traces/<stateId>.trace.json`.
- Renders offscreen at 1290 x 2796: every canvas is Screen Space - Camera targeting a camera that renders into a `RenderTexture` of that size (Gamma, no MSAA unless a state requires it); the PNG is read back as 8-bit RGB.
- Sequences (1.3): `OrbitalSalvageParity.exe --parity-sequences <sequencesJson> <outDir> [--only id1,id2]` replays `sequences.json` (8.2) in the `zero` profile and writes `<outDir>/traces/<sequenceId>.trace.json` only. Exit codes as below.
- `--only` limits states by id; `--profile` limits profiles (default: each state's `profiles`).
- Time is driven by the runner's fixed frame clock (4.4), never wall time; the frozen wall clock and zone of 4.2 apply. No `-nographics`: a GPU context is required for rendering.
- Exit code `0` when every selected state/profile was captured; `1` when any step or capture failed (details on stderr and in `<outDir>/errors.json`); `2` for CLI usage errors.

Asset sync (editor): `Unity.exe -batchmode -nographics -quit -projectPath games/maze-puzzle/unity/maze-puzzle -executeMethod OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll -logFile <log>`.

**Baseline UI (1.5).** Until the UI port renames objects per 8.1, the baseline adapter resolves ids through a path table (`BaselineIdMap.Entries`). 13 web ids have no Unity element on the baseline UI and are recorded in `layoutUnmappedIds` of each `.unity-notes.json`: `home-audio-icon`, `home-diorama-face`, `home-hearts-anchor`, `game-canvas`, `gesture-hint`, `modal-privacy`, `btn-close-privacy`, `btn-dismiss-privacy`, `modal-reset-confirm`, `btn-cancel-reset`, `btn-confirm-reset`, `defeat-backup-container`, `toast-container`. An op that cannot be applied (unmapped id, missing UI) never aborts a state: it is recorded in `unity-notes` and the capture still happens. The baseline Unity economy starts with 150 credits (web: 250), so states that depend on the balance must set it explicitly with `setApp`/`call` until the logic port lands.

## 12. Diff tool CLI

```
python tools/parity/diff/diff.py --web out/web --unity out/unity --iter NN
```

Paths are relative to `tools/parity` unless absolute. Requires `numpy`, `Pillow`, `scikit-image`.

Pairs `<stateId>__<profile>.png` (and `.layout.json`, traces) across the two directories and applies the pass bar of section 2 plus the trace rule of section 10.

Outputs:

- `reports/iter-NN/report.md`: one table row per (state, profile) with percent of pixels within tolerance, SSIM, worst element box delta (id and axis), trace verdict, and PASS/FAIL; a summary line with counts; the recorded harness settings (Chromium version, launch args, `perfOffset` per run, animation clock mechanism).
- `reports/iter-NN/report.json`: the same data, machine readable.
- `out/reports/iter-NN/<stateId>__<profile>.heatmap.png`: per-pixel max-channel difference, scaled so 16 maps to mid-intensity and 64+ saturates.
- `out/reports/iter-NN/<stateId>__<profile>.diff.png`: three panels (web, Unity, binary mask of pixels over tolerance).
- Exit code `0` if every pair passes, `1` if any fails, `2` if a counterpart file is missing or the arguments are invalid; `2` takes precedence over `1` (a run with holes is incomplete before it is wrong). A missing counterpart is reported with status `MISSING` (not `FAIL`) and reason `missing: <file>`, and is never counted as a pass.

Details fixed in 1.3:

- Options: `--only id1,id2` (state ids), `--profile zero|iphone`, `--workers N` (default `min(4, cpus)`), `--reports <dir>` (default `reports`), `--images <dir>` (default `out/reports`). `--iter` takes `NN` or `iter-NN`.
- Pairs come from the PNGs in `--web` (`<stateId>__<profile>.png`); trace pairs come from `<web>/traces/*.trace.json` (states and sequences). A state row also shows the verdict of its state trace; a trace `FAIL` fails the state row, a trace `MISSING` makes the run exit `2` but does not change the pixel verdict of the row.
- Pixel metric: percent of pixels whose max channel difference is `<= 16`. A size mismatch fails with reason `size` and skips the other metrics.
- Layout metric: web elements with `visible: true` are looked up by `id` (the layout key) in the Unity layout; a missing one fails with `missing_in_unity`; a web-hidden element that Unity shows fails with `visibility`; the box check is `|unity - web| <= 3` on each of `x`, `y`, `w`, `h`; unmatched Unity-only elements are ignored.
- `heatmap.png`: the max-channel difference mapped to an intensity (`d * 8` up to 16, then linear to 255 at 64) and coloured with the OpenCV `INFERNO` map. `diff.png`: web, Unity and over-tolerance mask panels at 50 % scale with a label strip.
- Traces are compared by `diff/trace_diff.py` (also runnable alone: `python diff/trace_diff.py --web out/web/traces --unity out/unity/traces`): equal length, same `type`, `name` and `args` per index (audio numbers within 1e-6), `|dt| <= 16.7` ms. The first mismatch is reported with both events.
- `report.json` records the harness block of each side from `<dir>/_meta.json` (Chromium version, launch args, animation clock, per-run `perfOffset`).

## 13. Pinned assets

| Source | Repository                            | Commit                                     |
| ------ | ------------------------------------- | ------------------------------------------ |
| Fonts  | `github.com/google/fonts`             | `9710da1eacb3be272583c3224dcb70f9da6eadbb` |
| Emoji  | `github.com/microsoft/fluentui-emoji` | `1ffb34c752ecf5d402f04cfb4b392c77f57c54bc` |

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

- `styles.css` loads Google Fonts through `@import` on line 6 with `Fredoka:wght@400;600;700;800;900` and `Nunito:wght@700;800;900`; Google serves only Fredoka 400/600/700 and Nunito 700/800/900 (evidence 2026-09-30, `evidence/google-fonts-css2.css`), so Fredoka 800/900 render with the 700 face and Nunito 400 body text renders with the 700 face (5.0).
- `renderer.js` uses `Fredoka`/`Nunito` with a bare `sans-serif` tail on canvas, so a system font would be reached for any glyph the primary lacks unless the harness rewrites the font string (5.2).
- CSS animations and transitions ignore the fake clock (4.3); the animation clock of 4.5 is mandatory.
- `performance.now()` at app construction is 0, 1 or 2 ms natively; the clock normalisation of 4.4 makes it exactly 0 and puts frames on multiples of 16.
- `engine/audio.js` creates its context lazily and draws from `Math.random`; the audio stub of 4.6 is mandatory in all runs.
- `getDailyResetCountdown()` uses local time while `engine/economy.js` day keys use UTC (4.2).
- `heroPulse` has no `@keyframes` (4.5); `styles.css` requests Nunito 700/800/900 only, so body text uses the 700 face (5.0, 5.2).
- `navigator.vibrate` exists in headless Chromium and the app gates it on `profile.settings.hapticsEnabled`; the wrapper in section 10 records calls and makes the result deterministic.

## Appendix A. CSS animation and transition inventory

Derived by parsing `games/maze-puzzle/web/styles.css` (comments stripped, every rule including those inside `@media` visited) and cross-checked against `rg` output for every `animation` / `transition` declaration. Line numbers are those of the declaration (keyframes: the `@keyframes` line). No `animation-*` or `transition-*` longhands are used anywhere, and none of `index.html`, `main.js` or `engine/*.js` declares an animation or transition (the only Web Animations usage is reading `getAnimations()` in the harness). The `@media (min-width: 480px)` block (line 69) contains none and does not apply at 430 px.

### A.1 `@keyframes` (19)

| #   | Name             | `styles.css` line | Stops (verbatim, whitespace collapsed)                                                                                                                                                                                                                                       |
| --- | ---------------- | ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | `handSwipe`      | 285               | `0% { transform: translateX(-24px) rotate(-12deg); } 50% { transform: translateX(24px) rotate(12deg); } 100% { transform: translateX(-24px) rotate(-12deg); }`                                                                                                               |
| 2   | `giftBounce`     | 793               | `0%, 100% { transform: translateY(0) rotate(0deg); } 30% { transform: translateY(-4px) rotate(-6deg); } 60% { transform: translateY(-2px) rotate(6deg); }`                                                                                                                   |
| 3   | `giftBtnPulse`   | 847               | `0%, 100% { transform: scale(1); } 50% { transform: scale(1.04); }`                                                                                                                                                                                                          |
| 4   | `cartoonToast`   | 925               | `0% { opacity: 0; transform: translateY(-16px) scale(0.85); } 15% { opacity: 1; transform: translateY(0) scale(1); } 82% { opacity: 1; } 100% { opacity: 0; transform: translateY(-12px) scale(0.9); }`                                                                      |
| 5   | `pageEnter`      | 975               | `0% { opacity: 0; transform: scale(0.96) translateY(8px); } 100% { opacity: 1; transform: scale(1) translateY(0); }`                                                                                                                                                         |
| 6   | `mascotFloat`    | 1079              | `0% { transform: translateY(-8px) rotate(-6deg) scale(1); } 100% { transform: translateY(10px) rotate(6deg) scale(1.05); }`                                                                                                                                                  |
| 7   | `floatDebris1`   | 1209              | `0% { transform: translateY(0px) rotate(0deg); } 50% { transform: translateY(-14px) rotate(15deg); } 100% { transform: translateY(8px) rotate(-10deg); }`                                                                                                                    |
| 8   | `floatDebris2`   | 1215              | `0% { transform: translateY(0px) rotate(0deg); } 50% { transform: translateY(16px) rotate(-20deg); } 100% { transform: translateY(-8px) rotate(12deg); }`                                                                                                                    |
| 9   | `pulseTwinkle`   | 1221              | `0%, 100% { opacity: 0.2; transform: scale(0.9); } 50% { opacity: 0.85; transform: scale(1.2); }`                                                                                                                                                                            |
| 10  | `rimPulse`       | 1370              | `0% { opacity: 0.65; box-shadow: 0 0 10px rgba(56, 189, 248, 0.45); border-color: rgba(56, 189, 248, 0.55); } 100% { opacity: 1; box-shadow: 0 0 22px rgba(56, 189, 248, 0.85), inset 0 0 8px rgba(56, 189, 248, 0.4); border-color: rgba(56, 189, 248, 0.95); }`            |
| 11  | `beamFlicker`    | 1394              | `0% { opacity: 0.6; } 100% { opacity: 1; }`                                                                                                                                                                                                                                  |
| 12  | `sunburstRotate` | 1423              | `0% { transform: rotate(0deg) scale(1); } 50% { transform: rotate(180deg) scale(1.05); } 100% { transform: rotate(360deg) scale(1); }`                                                                                                                                       |
| 13  | `ringFloat`      | 1445              | `0% { transform: translateY(-5px) rotate(-3deg); } 100% { transform: translateY(6px) rotate(3deg); }`                                                                                                                                                                        |
| 14  | `nibblesIdle`    | 1480              | `0% { transform: translateY(3px) scale(1); } 100% { transform: translateY(-5px) scale(1.04); }`                                                                                                                                                                              |
| 15  | `nibblesSquash`  | 1489              | `0% { transform: scale(1); } 35% { transform: scale(1.3, 0.72); } 65% { transform: scale(0.85, 1.25); } 100% { transform: scale(1); }`                                                                                                                                       |
| 16  | `floatUpFade`    | 1511              | `0% { opacity: 1; transform: translate(-50%, -50%) scale(0.5); } 50% { opacity: 1; transform: translate(calc(-50% + var(--dx, 0px)), calc(-50% - 40px)) scale(1.3); } 100% { opacity: 0; transform: translate(calc(-50% + var(--dx, 0px)), calc(-50% - 80px)) scale(0.8); }` |
| 17  | `shineSweep`     | 1651              | `0% { transform: translateX(-150%) rotate(25deg); } 25%, 100% { transform: translateX(150%) rotate(25deg); }`                                                                                                                                                                |
| 18  | `tagPulse`       | 1764              | `0%, 100% { transform: scale(1); } 50% { transform: scale(1.12); }`                                                                                                                                                                                                          |
| 19  | `masterGlow`     | 2098              | `0%, 100% { box-shadow: 0 0 12px rgba(250, 204, 21, 0.3); } 50% { box-shadow: 0 0 22px rgba(250, 204, 21, 0.6); }`                                                                                                                                                           |

`heroPulse` is referenced at line 1630 (`.btn-hero-play-3d`) but has **no** `@keyframes` rule (checked against the list above): no Animation object is created, both sides treat it as a no-op.

### A.2 `animation` declarations (26)

Defaults for omitted parts: easing `ease`, delay 0 ms, iterations 1, direction `normal`, fill `none`. Durations and delays in ms.

| #   | Selector                                       | Line | Keyframes                            | Duration | Delay | Easing                              | Iterations | Direction | Fill     |
| --- | ---------------------------------------------- | ---- | ------------------------------------ | -------- | ----- | ----------------------------------- | ---------- | --------- | -------- |
| 1   | `.hand-swipe-anim`                             | 282  | `handSwipe`                          | 1800     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 2   | `.gift-banner-icon`                            | 784  | `giftBounce`                         | 2000     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 3   | `.daily-gift-banner.claimed .gift-banner-icon` | 789  | none (cancels the animation)         | -        | -     | -                                   | -          | -         | -        |
| 4   | `.gift-claim-btn`                              | 844  | `giftBtnPulse`                       | 2200     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 5   | `.daily-gift-banner.claimed .gift-claim-btn`   | 858  | none (cancels the animation)         | -        | -     | -                                   | -          | -         | -        |
| 6   | `.toast`                                       | 922  | `cartoonToast`                       | 2600     | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` | 1          | normal    | forwards |
| 7   | `.app-page.page-active`                        | 972  | `pageEnter`                          | 220      | 0     | `ease-out`                          | 1          | normal    | forwards |
| 8   | `.loading-mascot-anim`                         | 1075 | `mascotFloat`                        | 1800     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 9   | `.debris-can`                                  | 1171 | `floatDebris1`                       | 8000     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 10  | `.debris-bolt`                                 | 1178 | `floatDebris2`                       | 9000     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 11  | `.debris-star-1`                               | 1185 | `pulseTwinkle`                       | 3000     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 12  | `.debris-star-2`                               | 1192 | `pulseTwinkle`                       | 2500     | 1000  | `ease-in-out`                       | infinite   | normal    | none     |
| 13  | `.debris-fish`                                 | 1199 | `floatDebris1`                       | 11000    | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 14  | `.debris-planet`                               | 1206 | `floatDebris2`                       | 14000    | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 15  | `.pedestal-disc-rim`                           | 1367 | `rimPulse`                           | 3500     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 16  | `.pedestal-glow-beam`                          | 1391 | `beamFlicker`                        | 3000     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 17  | `.diorama-sunburst`                            | 1419 | `sunburstRotate`                     | 22000    | 0     | `linear`                            | infinite   | normal    | none     |
| 18  | `.diorama-ring-floater`                        | 1440 | `ringFloat`                          | 3200     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 19  | `.diorama-nibbles`                             | 1475 | `nibblesIdle`                        | 2200     | 0     | `ease-in-out`                       | infinite   | alternate | none     |
| 20  | `.diorama-nibbles.squash`                      | 1486 | `nibblesSquash`                      | 400      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` | 1          | normal    | forwards |
| 21  | `.spawned-particle`                            | 1508 | `floatUpFade`                        | 1200     | 0     | `cubic-bezier(0.25, 1, 0.5, 1)`     | 1          | normal    | forwards |
| 22  | `.btn-hero-play-3d`                            | 1630 | `heroPulse` (no `@keyframes`, no-op) | 2400     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 23  | `.play-btn-glow`                               | 1647 | `shineSweep`                         | 3500     | 0     | `ease`                              | infinite   | normal    | none     |
| 24  | `.dock-badge-hot`                              | 1736 | `tagPulse`                           | 1600     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 25  | `.pulse-tag`                                   | 1761 | `tagPulse`                           | 1600     | 0     | `ease-in-out`                       | infinite   | normal    | none     |
| 26  | `.daily-master-box.unlocked`                   | 2034 | `masterGlow`                         | 2500     | 0     | `ease-in-out`                       | infinite   | normal    | none     |

### A.3 `transition` declarations (29 property groups)

Defaults for omitted parts: easing `ease`, delay 0 ms. `all` covers every animatable property that changes; `transition: 0.25s` means `all 0.25s ease 0s`. Transitions only run when the element had a computed style before the change (an element going from `display: none` to shown, such as `.app-page`, does not transition).

| #   | Selector                | Line | Property       | Duration | Delay | Easing                              |
| --- | ----------------------- | ---- | -------------- | -------- | ----- | ----------------------------------- |
| 1   | `.btn-hud-circle`       | 132  | `transform`    | 100      | 0     | `ease`                              |
| 2   | `.btn-hud-circle`       | 132  | `box-shadow`   | 100      | 0     | `ease`                              |
| 3   | `.capsule-bar-fill`     | 193  | `width`        | 250      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 4   | `.btn-sound-mini`       | 242  | `transform`    | 100      | 0     | `ease`                              |
| 5   | `.gesture-hint`         | 270  | `opacity`      | 400      | 0     | `ease`                              |
| 6   | `.gesture-hint`         | 270  | `transform`    | 400      | 0     | `ease`                              |
| 7   | `.tab-nav-btn`          | 347  | `transform`    | 100      | 0     | `ease`                              |
| 8   | `.modal-overlay`        | 380  | `opacity`      | 250      | 0     | `ease`                              |
| 9   | `.modal-card`           | 399  | `transform`    | 300      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 10  | `.tab-btn`              | 471  | `all`          | 150      | 0     | `ease`                              |
| 11  | `.world-tab-btn`        | 513  | `all`          | 150      | 0     | `ease`                              |
| 12  | `.level-tile`           | 601  | `transform`    | 120      | 0     | `ease`                              |
| 13  | `.level-tile`           | 601  | `border-color` | 120      | 0     | `ease`                              |
| 14  | `.daily-gift-banner`    | 755  | `all`          | 300      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 15  | `.app-page`             | 957  | `opacity`      | 240      | 0     | `ease`                              |
| 16  | `.app-page`             | 957  | `transform`    | 240      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 17  | `.loading-bar-fill`     | 1127 | `width`        | 350      | 0     | `ease`                              |
| 18  | `.diorama-ring-floater` | 1442 | `all`          | 300      | 0     | `ease`                              |
| 19  | `.diorama-nibbles`      | 1476 | `transform`    | 150      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 20  | `.btn-hero-play-3d`     | 1629 | `transform`    | 100      | 0     | `ease`                              |
| 21  | `.btn-hero-play-3d`     | 1629 | `box-shadow`   | 100      | 0     | `ease`                              |
| 22  | `.dock-tab-btn`         | 1689 | `all`          | 150      | 0     | `ease`                              |
| 23  | `.dock-tab-icon`        | 1704 | `transform`    | 150      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 24  | `.slider-round`         | 1857 | `all`          | 250      | 0     | `ease`                              |
| 25  | `.slider-round:before`  | 1869 | `all`          | 250      | 0     | `ease`                              |
| 26  | `.daily-progress-fill`  | 2014 | `width`        | 400      | 0     | `cubic-bezier(0.34, 1.56, 0.64, 1)` |
| 27  | `.daily-master-box`     | 2027 | `all`          | 300      | 0     | `ease`                              |
| 28  | `.daily-shift-card`     | 2067 | `transform`    | 200      | 0     | `ease`                              |
| 29  | `.daily-shift-card`     | 2067 | `box-shadow`   | 200      | 0     | `ease`                              |

## Appendix B. Canvas text call sites

All canvas text is drawn in `engine/renderer.js`; `main.js`, `physics.js`, `maze.js`, `economy.js` and `audio.js` contain no `fillText`, `strokeText` or `measureText`. Every `textBaseline = "middle"` assignment is inside its own `ctx.save()` / `ctx.restore()` pair. The context transform is `setTransform(dpr, 0, 0, dpr, 0, 0)` with `dpr = min(devicePixelRatio, 3)` (lines 79 and 320). Emoji column: whether the string can contain an emoji grapheme (section 6 classification).

| #   | Line | Call         | Context (function)             | Font (`ctx.font`)                           | fontPx | Weight (requested; face per 5.0) | `textAlign` | `textBaseline`                  | Position                                                             | Text                                                                                                       | Emoji?                        |
| --- | ---- | ------------ | ------------------------------ | ------------------------------------------- | ------ | -------------------------------- | ----------- | ------------------------------- | -------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- | ----------------------------- |
| 1   | 1077 | `fillText`   | `renderCartoonRing`, inner hub | `bold 13px Fredoka, sans-serif` (line 1053) | 13     | 700                              | center      | middle                          | (0, -7)                                                              | `hubEmoji`: `✨ ORBITAL ✨`, `🦆 DUCKIE 🦆`, `🍬 CANDY 🍬`, `🐱 KITTY 🐱`, `🍩 DONUT 🍩`, `⚙️ GEAR RIG ⚙️` | yes (2 per string)            |
| 2   | 1080 | `fillText`   | `renderCartoonRing`, inner hub | `11px Nunito, sans-serif` (line 1078)       | 11     | 400                              | center      | middle                          | (0, 9)                                                               | `hubSubtitle`: `SORT RIG`, `FLOAT RIG`, `MINT RIG`, `COSMIC RIG`, `SWEET RIG`, `PLANETARY`                 | no                            |
| 3   | 1460 | `fillText`   | `renderSawbladeGate`           | `bold 10px Fredoka, sans-serif` (1457)      | 10     | 700                              | center      | middle                          | (0, 0.5)                                                             | `⚡ ×${gate.value}`                                                                                        | yes (⚡)                      |
| 4   | 1563 | `fillText`   | `renderSuperShredderGate`      | `900 10.5px Fredoka, sans-serif` (1560)     | 10.5   | 900                              | center      | middle                          | (0, 0.5)                                                             | `💥 ×${gate.value}!`                                                                                       | yes (💥)                      |
| 5   | 1650 | `fillText`   | `renderCompactorGate`          | `bold 10px Fredoka, sans-serif` (1647)      | 10     | 700                              | center      | middle                          | (0, 0.5)                                                             | `🧲 +${gate.value}`                                                                                        | yes (🧲)                      |
| 6   | 1723 | `fillText`   | `renderBoosterGate`            | `bold 9.5px Fredoka, sans-serif` (1720)     | 9.5    | 700                              | center      | middle                          | (0, 0.5)                                                             | `⏩ BOOST`                                                                                                 | yes (⏩)                      |
| 7   | 1946 | `strokeText` | `renderVfx`, floating text     | `900 16px Fredoka, sans-serif` (1942)       | 16     | 900                              | center      | alphabetic (default, never set) | (0, 0) after `translate(ft.x, ft.y)` and `scale(ft.scale, ft.scale)` | `ft.text`; draws text runs only                                                                            | emoji skipped by `strokeText` |
| 8   | 1950 | `fillText`   | `renderVfx`, floating text     | same as 7                                   | 16     | 900                              | center      | alphabetic                      | same as 7                                                            | `ft.text`, from `main.js` (below); draws text runs and emoji                                               | yes                           |

Floating-text strings (`addFloatingText` callers in `main.js`): `NOM! 😋`, `YUM! ✨`, `GULP! 🍩`, `TASTY! ⭐` (line 651-652), `×${n}! SHRED ⚡`, `+${n}! COMPACT ✨`, `BOOST! ⏩` (lines 691-696), `POOF! 💨` (line 704), `TURBO! ⏩` and `⏪ TURBO!` (line 713). Every one contains exactly one emoji grapheme after or before the text, so rows 7 and 8 together exercise the stroke-without-emoji and fill-with-emoji paths.

Dedicated states per call site (1.4; each asserts the emoji or floating text with `expectEmoji` / `expectFloating`):

| Rows | States                                                                                                                                                                                                                                                                                                                                                                                                                              |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1, 2 | `canvas_hub_duckie`, `canvas_hub_candy`, `canvas_hub_kitty`, `canvas_hub_donut` (the default ring), `canvas_hub_gear` (concentric sector, level 21). `✨ ORBITAL ✨` / `SORT RIG` is unreachable: `ringSkin` falls back to `rings[0]`, which is `ring_donut`, and the catalog has exactly the four ring skins above, so it is dead code and has no state.                                                                           |
| 3    | `canvas_gate_bolt_x2` (level 1), `canvas_gate_bolt_x3` (level 5)                                                                                                                                                                                                                                                                                                                                                                    |
| 4    | `canvas_gate_blast_x4` (level 41)                                                                                                                                                                                                                                                                                                                                                                                                   |
| 5    | `canvas_gate_magnet_add` (level 1)                                                                                                                                                                                                                                                                                                                                                                                                  |
| 6    | `canvas_gate_boost` (level 33)                                                                                                                                                                                                                                                                                                                                                                                                      |
| 7, 8 | `canvas_float_shred`, `canvas_float_compact`, `canvas_float_boost` (via `physics.onGatePassed`), `canvas_float_poof` (`physics.onDebrisExploded`), `canvas_float_turbo`, `canvas_float_turbo_reverse` (`physics.onBoostTriggered`), `canvas_float_nom_words` (`renderer.addFloatingText` with the four strings of `main.js` 650; the word is `Math.random`-picked in the app, `game_lvl01_pickup` shows the seeded pick `GULP! 🍩`) |

## Changelog

### 1.5

Integration of the infra, unity-capture, logic, audio and canvas tracks on top of 1.4; the processed requests are in `reports/spec-requests/processed/`.

- 4.7 (new, from logic): goldens come from Chromium 153 through Playwright, not Node (Node is not comparable beyond ~1e-4); `Math.pow`/`Math.hypot` must go through `JsMath.Pow`/`JsMath.Hypot`; parity must be re-verified in a player build (IL2CPP/FMA contraction risk); Mono number parsing/formatting needs `JsNumber.ToString` and overflow handling. From the audio track's measurements: reference WAVs are not bit-reproducible, so the audio reference check is split into a local file-integrity hash and committed statistics/fingerprints.
- 6.3 (new, from canvas): emoji sprite import is uncompressed RGBA32 on every platform, no mips, bilinear, max size 512 (`1fa90` is 256 x 264), enforced by `ParityAssetBuilder` and `EmojiImportTests`.
- 6.4 (new, from canvas): **OPEN - awaiting lead decision** on the 99.0 % bar for the synthetic text scenes (`text_align_baseline`, `text_emoji`, `text_fonts`) versus an outline glyph rasteriser. Not decided here.
- 11 (from unity-capture): the player is launched minimised and windowed, not `-batchmode`/`-nographics`/hidden (those render black); the `-batchmode` wording is removed from the CLI lines; a documented 0.25 s real-time flush after pointer ops; `-Isolated` is the reference mode for byte-identity checks; baseline UI notes (13 unmapped ids, an op that cannot execute is recorded as unsupported and never aborts a state or changes the exit code, 150 vs 250 initial credits).
- Audio track: no SPEC requests.

### 1.4

- 6.2: canvas emoji self-check on every game-page capture (see above); Appendix B gains the per-call-site state table and the note that the `ORBITAL` hub string is dead code. No change to the substitution algorithm: the `game_lvl01_gate_pass` report of flat bolts was the Fluent 3D `26a1` asset (pixel-matched) and two separate gate passes.
- 8: `call` takes an optional `target` (`app` default, `renderer`, `physics`) with per-target allow-lists; `capture` takes `expectEmoji`, `expectFloating`, `expectFraction`; `scroll` fails when the target cannot scroll and is no longer used by any state (no page overflows).
- States: `daily_scrolled`, `shop_scrolled`, `settings_scrolled` removed (identical to their `_default` states); `maps_world_5_scrolled` renamed `maps_world_5_levels_1_44_done` (scroll dropped); `settings_toggles_clicked` replaced by `settings_haptics_clicked_off` and `settings_sound_toggle_t0100`; `game_lvl01_near_victory` and `game_lvl01_quota_met_hud` now sample 300 ms after the update (settled bar), with `game_lvl01_quota_bar_t0016`, `game_lvl01_quota_bar_t0125` and `game_lvl01_quota_met_t0016` for the transition frames; `maps_world_1_all_complete_other_active` added; descriptions of `loading_t0000`, `maps_world_1_all_complete`, `game_lvl01_gate_pass`, `game_lvl01_pickup` corrected; 17 `canvas_*` states added.
- `scripts/check-transition.mjs` compares the `.capsule-bar-fill` width with the analytic `cubic-bezier(0.34, 1.56, 0.64, 1)` over 0.25 s at 16 to 1000 ms on the fake clock (measured: 0.258 at 16 ms against 0.2575 expected).
### 1.3

- 4.2: the web harness installs the clock at `T0 - 60000` before `pauseAt(T0 + 1000)` (robustness only).
- 6.1: `Node.prototype.textContent` getter returns the original grapheme for `img.pe` (the mascot click at `main.js` 303-308 restores the saved `textContent`).
- 8: new click targeting rules (`fx`/`fy`, obscured-target failure, checkbox uses its label rect, pointer parked at (-1, -1) after `click`/`clickSelector` to make `:hover` deterministic); new 8.2 `sequences.json` (format, `expect`, the seven sequences).
- 7, 10: trace files are produced by states (once per state) and by sequences; ids are disjoint (was: `sequenceId` equals `stateId`).
- 11: `--parity-sequences` Unity CLI mode.
- 12: `MISSING` status (was `FAIL` with reason `missing`), exit code 2 beats 1, options, metric definitions, heatmap colouring and diff panel layout, trace rule in state rows, harness block in the report.

### 1.2

- Fonts (5): the reference mirrors what Google Fonts serves for the app's `@import`: exactly Fredoka 400/600/700 and Nunito 700/800/900, everything else resolved by CSS font matching, no synthetic bold. Evidence (2026-09-30) committed as `evidence/google-fonts-css2.css`.
- New 5.0 with the matching rules and the effective-face table for every family/weight the CSS, inline styles and canvas request (Nunito 400 -> 700 face; Fredoka 800/900 -> 700 face). Generated by `scripts/font-usage-scan.mjs`.
- Removed `Fredoka-800/900` and `Nunito-400` from the SPEC, `fetch-assets.mjs`, `manifest.json`, `font-coverage.py`, and (Unity) `Assets/Fonts` and the TMP assets.
- 5.1: the Noto symbol chain is documented as a deliberate determinism choice (a real device would use a system symbol font).
- 5.3 / Unity: TMP assets moved to `Assets/Resources/Fonts/TMP/` (Resources registry, documented); `ParityAssetBuilder` builds only the six primary faces plus the two fallbacks and removes obsolete outputs; new `OrbitalSalvage.Render.ParityFonts.Resolve(family, cssWeight)` implements the CSS weight matching; `ParityAssetBuilder.VerifyFonts` checks the table in batchmode.
- 5.2: harness `boot` asserts the effective faces; section 6.2 and Appendix B state requested weight vs. drawn face; section 13 and 15 updated.

### 1.1

- Clock: `performance.now()` is re-based to exactly 0 at app start and requestAnimationFrame is owned by the harness (4.4), replacing the `lastTime` patch; `Date` at `performance.now() === 0` is `2026-10-01T16:00:01.000Z` (4.2). Home is now reached at 1310 / 1312 (previously 1314, an offset-2 artefact).
- Frame rule quoted literally from `main.js loop()`: physics only on `game` and not paused, render (and its random draws) on `game` including while paused; `dt` is the double 0.016.
- Animation clock rewritten (4.5): registry, stamping after every rAF / timer / listener / op, pause-on-register, sweep with `currentTime`, no CDP playback rate, `animations: "allow", caret: "hide"`, race guard, boot settle (+240 ms), settle rules, `heroPulse` no-op; Appendix A lists all 19 keyframes, 26 animation and 29 transition declarations.
- Audio determinism (4.6): `ParityAudioContext` stub in all runs, trace events, `publicInit` caller list, cooldown list, exact `Math.random` consumption (collision 1, bumper 1, RCS 9702).
- Canvas emoji (6.2): font parsing, weight mapping, `strokeText` never draws emoji, baseline rules checked against `renderer.js`, no `dpr` multiplication; Appendix B lists every text call site.
- Ops (8): `setApp currentStoreTab` caveat and legal values, new `clickSelector` and `scroll` ops, `loadSector` also sets `currentSectorIndex`, gameplay-state recipe, `holdPressed` includes `advance(150)` and releases after capture; Unity id mapping (8.1).
- Safe-area fallback uses the full original `padding` shorthands; all harness styles are appended after `styles.css` (3, 5.2); body font note.
- Fonts (5.1): U+2014 and U+2019 are not present in the web sources (kept as defensive coverage only).
- Trace (10): modal events logged at the toggling callback's time after its `timer` event; layout rects are viewport coordinates.

### 1.0

- Initial contract.
