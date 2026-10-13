# Spike: capture pipeline and Settings language UI

Phase 0 feasibility only. Throwaway web edits were reverted. This file is the only commit. Unity was not launched. Capture ran on this Mac (darwin, Node v22.18.0, Playwright 1.63.0, Chromium 153.0.8010.12 headless shell build 1243).

Scratch outputs: `/tmp/maze-i18n/capture/` (listed at the end).

## 1. Pipeline

`package.json` has no npm scripts. Run the Node CLIs from `games/maze-puzzle/tools/parity`. There are two Playwright producers. The Unity chrome painter reads the second one.

| Step              | Command                                   | Output                                                                                        |
| ----------------- | ----------------------------------------- | --------------------------------------------------------------------------------------------- |
| Pixels and layout | `node web/capture-web.mjs`                | `out/web/<stateId>__<profile>.png` (1290×2796), `.layout.json`, `traces/<stateId>.trace.json` |
| DOM dump          | `node web/dump-spec.mjs`                  | `out/web/spec/<name>__<profile>.json` (full computed style, text runs)                        |
| Distill           | `node web/distill-spec.mjs --page <name>` | `Assets/Resources/Parity/Spec/<name>__<profile>.json`                                         |

`HomeChrome.Load` (`Assets/Scripts/Render/HomeChrome.cs` lines 491–500) paints that distilled JSON with Canvas2D. Viewport in every committed settings spec is `{w:430,h:932,scale:3}`.

### One spec

The committed Unity names (`settings_default`, `settings_modal_privacy`, …) are `states.json` ids, not the short page names inside `dump-spec.mjs`. Pass `--states`:

```bash
cd games/maze-puzzle/tools/parity
node web/dump-spec.mjs --states states.json --only settings_modal_language
node web/distill-spec.mjs --page settings_modal_language
```

`--only` is a comma list. Default profiles are `zero` and `iphone` (`dump-spec.mjs` lines 9 and 754–784). `--profile zero` captures one. `distill-spec.mjs` is one `--page` per invocation and always reads `tools/parity/out/web/spec/<page>__<profile>.json` (lines 178–180). `--out` changes only the distill destination (default `Assets/Resources/Parity/Spec`, line 17). To keep the repo clean, dump with `--out /tmp/...` and symlink that `spec/` directory to `out/web/spec` before distilling.

`dump-spec.mjs` without `--states` writes a different, shorter set (`ENTRIES`, line 44) whose settings entry is named `settings` with root `#page-settings` (lines 188–191). That is not how the files in `Assets/Resources/Parity/Spec` were named. With `--states`, each state id becomes the page name and the root is `#mobile-frame` (lines 739–748), so the spec includes the whole phone, not just the page.

### All specs

```bash
node web/dump-spec.mjs --states states.json
# then one distill per state id
node web/distill-spec.mjs --page <stateId>
```

There is no loop script. A full `capture-web.mjs` run is the pixel set (README: about 10 minutes). It does not write Unity spec JSON.

### Declaring a state

Edit `scripts/gen-states.mjs`, not `states.json` by hand. `add(id, description, steps, profiles)` (line 32) appends a record. Settings today:

```518:566:games/maze-puzzle/tools/parity/scripts/gen-states.mjs
const settingsOpen = [boot(), click("btn-home-settings"), adv(240)];
add("settings_default", "Settings, default profile", [...settingsOpen, CAP]);
// ...
add("settings_modal_privacy", "Settings with the privacy policy modal open", [
  ...settingsOpen,
  click("btn-open-privacy"),
  adv(300),
  CAP,
]);
```

A language modal is the same shape: `[...settingsOpen, click("btn-open-language"), adv(300), CAP]`. Then:

```bash
node scripts/gen-states.mjs
npx --yes prettier@3 --write states.json
```

`gen-states.mjs` writes `JSON.stringify(states, null, 1)` (lines 1083–1086). Without Prettier that rewrites the whole committed file (this spike: 14,103 lines of churn). After Prettier the diff was the new object only (29 lines). `sequences.json` is the trace list (`scripts/gen-sequences.mjs`). A picker does not need a sequence unless a flow must be replayed. Existing settings sequence: `settings_toggles` in `sequences.json`.

Ops are SPEC §8. `click` targets a DOM id. Both profiles are the default (`BOTH` at `gen-states.mjs` line 7).

### iPhone profile

Not a second capture tool. `harness.mjs` `PROFILES` (lines 17–20) is `zero` all insets 0, `iphone` top 59, bottom 34. `Session.boot` calls CDP `Emulation.setSafeAreaInsetsOverride` (lines 286–308). This run logged `safeArea cdp`. The CSS fallback in `overridesCss` (lines 169–185) did not run. SPEC §3 is the contract. Both profiles are written by the same dump command.

### tracked-classes.json and hit testing

`scripts/tracked-classes.mjs` keeps classes that appear in `index.html` / `main.js` `class=` attributes and are also defined in `styles.css`. `capture-web.mjs` layout JSON includes an element only if it has an id or one of those classes (SPEC §9, 144 classes today). The distilled spec does not use that filter: `distill-spec.mjs` maps every element in the dump (lines 182–278), including `id`, `c` (classes), and `runs: [{s, r:[x,y,w,h]}]`.

Unity hit testing is a third list. `ParityPages.TryHitControl` (`Assets/Scripts/Viewer/ParityPages.cs` lines 458–481) walks parents for `InteractiveIds` (lines 437–450, already includes `btn-open-privacy`, `btn-close-privacy`, `btn-dismiss-privacy`, `btn-reset-game-data`) or `InteractiveClasses` (lines 452–455: shop tabs, level tiles, daily play). A new button is dead to the pointer until its id is added to `InteractiveIds`, or its class is added to `InteractiveClasses`. Regenerating `tracked-classes.json` does not do that. Nav edges are a separate table (`Edges`, lines 255–306); settings today is only `btn-home-settings` → `settings_default` and `btn-back-settings` → home (lines 265 and 285).

## 2. Determinism on this Mac

Toolchain: Node v22.18.0 (SPEC pins v24.15.0; the mismatch did not move metrics), Playwright 1.63.0, Chromium `153.0.8010.12`, launch args `--force-color-profile=srgb --font-render-hinting=none --disable-lcd-text` (`harness.mjs` lines 21–25). Unchanged tree. Commands:

```bash
node web/dump-spec.mjs --states states.json --only settings_default,settings_modal_privacy --out /tmp/maze-i18n/capture/web
# symlink that spec/ to tools/parity/out/web/spec
node web/distill-spec.mjs --page settings_default --out /tmp/maze-i18n/capture/distilled
node web/distill-spec.mjs --page settings_modal_privacy --out /tmp/maze-i18n/capture/distilled
```

Parsed JSON is equal for all four files (337 elements, `t0` 220, same viewport). SHA-256 of the distilled bytes:

| File                                  | Committed                                                          | Fresh distill                                                      | Bytes         |
| ------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------ | ------------- |
| `settings_default__iphone.json`       | `bfc789c6b85c0ee084a7d07f2ba969fc2648d759848d76625d30fe94eb33081f` | same                                                               | identical     |
| `settings_modal_privacy__zero.json`   | `0bb3c9e7b4279813fbad25b61b5458f8a8484826cd8f866f239f575466810eb2` | same                                                               | identical     |
| `settings_modal_privacy__iphone.json` | `cd04c2cd3b4815da54db461fd3e53d743551951d3c0414655e7a5b42277a3169` | same                                                               | identical     |
| `settings_default__zero.json`         | `1e78155f2b25e2c7ebe0b69835ea769d87cda0c854fc4a8cfe2ee25619a8ef1b` | `1b881fa8d1d749f7bafca366e6585129430de558f7d88ef3cf7628565e707f0d` | not identical |

The one byte difference is key order only. Committed `settings_default__zero.json` has `"t0":220` after `elements`. A fresh distill writes `t0` before `count` (`distill-spec.mjs` lines 282–288). `JSON.parse` equality holds; no font, run, or rect differs. Cause: `mergePaint` (lines 84–107) loads an existing file and assigns `base.t0`, which appends the key. That file was merge-painted once. The other three match a fresh stringify. No macOS-vs-Windows font-hinting delta showed up under the pinned launch args and the local fonts.

Network: a boot with the harness logged `blockedCount 0`. `resolveRoute` answers `fonts.googleapis.com` with local `@font-face` and aborts every other origin (`harness.mjs` lines 217–224 and 315–321). SPEC §5.2. A second dump with `browser.newContext({ offline: true })` (throwaway harness edit, reverted) was byte-identical to the online raw dump of `settings_default__zero` (SHA-256 `7a703831cc735de0d5b553cd1d8feef94265e2dcd695daf82318358812d16c49`). Offline does not change metrics. The Google Fonts `@import` in `web/styles.css` line 6 is never fetched.

This does not block new screens. Leave every existing spec file untouched and add new JSON beside it. Regenerating an unchanged page on this Mac reproduces the committed values. `settings_default` itself has to be regenerated once the rows exist, because the lower half of that page moves. Unchanged rows on that page still matched (see §3). Do not regenerate home, game, shop, and the rest just to pick up a hidden modal: a throwaway `home_default` distill grew 337 → 373 elements (the new nodes) while every shared id kept the same `r` and `runs`.

## 3. Dry-run (reverted)

Throwaway edits in `web/index.html`, `web/styles.css`, `web/main.js`, `scripts/gen-states.mjs`, `states.json`, `tracked-classes.json`, and `web/page-init.js` (`MODAL_IDS`). All restored with `git checkout`. Nothing from this section is in the commit.

`settings_default` went from 337 to 373 elements. These committed rects did not move on the zero profile: `GAME SETTINGS` `[148.5, 24.5, 161.422, 21]`, `Master Sound` `[34, 135, 156.172, 17]`, `Sound FX Volume` `[34, 183, 138.672, 17]`, `Cosmic Ambience` `[34, 231, 165.219, 17]`, `Haptic Vibration` `[34, 346, 205.453, 17]`. Privacy Policy moved from y=461 to y=576. The version line moved from y=650 to y=813.

Zero profile scroll: `#page-settings .page-scroll-body` `scrollHeight == clientHeight == 862`. Footer run bottoms 829 and 845, both above the iPhone band (898). iPhone profile is not that comfortable: the second footer run, `Pure Gesture Mobile Web Puzzle`, is y=888 h=16, bottom **904**. `IphoneSafeAreaTests` rejects text below 898.25 (`Assets/Tests/EditMode/Viewer/IphoneSafeAreaTests.cs` lines 111–115). Committed iPhone footer bottom is 741. The new rows cost about 163px and the last line enters the home-indicator band by 6px. The language modal itself does not: see below.

### DOM that captured

Language section after Gameplay, User Agreement as the second row of Privacy & Safety. Privacy still opens `#modal-privacy` so the existing state keeps working. The product plan (both policy rows open a URL) is a later switch; this spike only wires User Agreement to `window.open`.

```html
<div class="settings-section-card">
  <span class="settings-section-title">Language</span>
  <div class="settings-row">
    <div class="settings-label-box">
      <span class="settings-label">Language</span>
      <span class="settings-sublabel" id="settings-language-value">English</span>
    </div>
    <button class="btn-cartoon-primary" id="btn-open-language">Change</button>
  </div>
</div>
<!-- second row inside the existing Privacy & Safety card -->
<div class="settings-row">
  <div class="settings-label-box">
    <span class="settings-label">User Agreement</span>
    <span class="settings-sublabel">The rules for playing the game</span>
  </div>
  <button
    class="btn-cartoon-primary"
    id="btn-open-user-agreement"
    data-url="https://example.com/user-agreement"
  >
    View
  </button>
</div>

<div class="modal-overlay" id="modal-language">
  <div class="modal-card lang-modal-card">
    <div class="modal-header">
      <h2 class="modal-title" id="language-modal-title">Choose Language</h2>
      <button class="lang-close" id="btn-close-language" type="button">Close</button>
    </div>
    <div class="modal-body lang-modal-body">
      <div class="lang-grid" id="language-grid">
        <button type="button" class="lang-option active" id="btn-lang-en" data-lang="en" lang="en">
          English
        </button>
        <button
          type="button"
          class="lang-option"
          id="btn-lang-zh-hans"
          data-lang="zh-Hans"
          lang="zh-Hans"
        >
          简体中文
        </button>
        <button
          type="button"
          class="lang-option"
          id="btn-lang-zh-hant"
          data-lang="zh-Hant"
          lang="zh-Hant"
        >
          繁體中文
        </button>
        <button type="button" class="lang-option" id="btn-lang-ja" data-lang="ja" lang="ja">
          日本語
        </button>
        <button type="button" class="lang-option" id="btn-lang-ko" data-lang="ko" lang="ko">
          한국어
        </button>
        <button type="button" class="lang-option" id="btn-lang-de" data-lang="de" lang="de">
          Deutsch
        </button>
        <button type="button" class="lang-option" id="btn-lang-fr" data-lang="fr" lang="fr">
          Français
        </button>
        <button type="button" class="lang-option" id="btn-lang-es" data-lang="es" lang="es">
          Español
        </button>
        <button type="button" class="lang-option" id="btn-lang-it" data-lang="it" lang="it">
          Italiano
        </button>
        <button type="button" class="lang-option" id="btn-lang-pt" data-lang="pt" lang="pt">
          Português
        </button>
        <button type="button" class="lang-option" id="btn-lang-vi" data-lang="vi" lang="vi">
          Tiếng Việt
        </button>
        <button type="button" class="lang-option" id="btn-lang-fil" data-lang="fil" lang="fil">
          Filipino
        </button>
        <button type="button" class="lang-option" id="btn-lang-id" data-lang="id" lang="id">
          Bahasa Indonesia
        </button>
        <button type="button" class="lang-option" id="btn-lang-uz" data-lang="uz" lang="uz">
          Oʻzbekcha
        </button>
        <button type="button" class="lang-option" id="btn-lang-km" data-lang="km" lang="km">
          ភាសាខ្មែរ
        </button>
        <button type="button" class="lang-option" id="btn-lang-hi" data-lang="hi" lang="hi">
          हिन्दी
        </button>
        <button
          type="button"
          class="lang-option"
          id="btn-lang-ar"
          data-lang="ar"
          lang="ar"
          dir="rtl"
        >
          العربية
        </button>
      </div>
    </div>
  </div>
</div>
```

```css
#modal-language.modal-overlay {
  align-items: center;
  padding: 24px 16px;
}
#modal-language .lang-modal-card {
  width: 100%;
  max-height: none;
  border-radius: 28px;
  transform: translateY(16px);
}
#modal-language.modal-overlay.active .lang-modal-card {
  transform: translateY(0);
}
#modal-language .lang-modal-body {
  overflow: hidden;
  padding: 12px 12px 14px;
  gap: 0;
}
.lang-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}
.lang-option {
  font-family: var(--font-bubble);
  font-weight: 800;
  font-size: 14px;
  line-height: 1.15;
  padding: 8px 6px;
  min-height: 36px;
  border-radius: 14px;
  border: 2.5px solid var(--cartoon-border);
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  text-align: center;
  white-space: nowrap;
}
.lang-option.active {
  background: var(--yellow-main);
  color: var(--text-dark);
}
.lang-close {
  background: #fff;
  border: 2px solid var(--cartoon-border);
  color: var(--text-dark);
  font-family: var(--font-bubble);
  font-size: 13px;
  font-weight: 800;
  height: 32px;
  padding: 0 12px;
  border-radius: 999px;
  flex: 0 0 auto;
}
```

The stock `.modal-card` is a bottom sheet (`max-height: 82vh`, `align-items: flex-end`, `styles.css` lines 365–404) and `.modal-body` scrolls (`overflow-y: auto`, line 447). The picker overrides that: centered card, `overflow: hidden`, no scroll. `.btn-close` is a 32×32 circle (lines 422–430) and clips the word "Close", so the close control is `.lang-close`.

State declaration that captured:

```js
add(
  "settings_modal_language",
  "Settings with the language picker modal open (17 endonyms, no scroll)",
  [...settingsOpen, click("btn-open-language"), adv(300), CAP]
);
```

`page-init.js` `MODAL_IDS` (lines 511–517) must gain `"modal-language"` or the trace never records open/close. `tracked-classes.json` gained `lang-close`, `lang-grid`, `lang-modal-body`, `lang-modal-card`, `lang-option` (144 → 149).

### Spec proof

`settings_modal_language__zero.json`: 373 elements. Modal root `id=modal-language` `c=modal-overlay active` rect `[0,0,430,932]`. Grid `id=language-grid` `c=lang-grid` `[31, 303.578, 368, 388.844]`. Seventeen `btn-lang-*` buttons, class `lang-option` (English also `active`), each `h=36.094`, columns x=31 and x=219, width 180. Run text is the endonym. Examples: `简体中文` run `[281, 312.578, 56, 17]`; `Bahasa Indonesia` `[66.266, 577.141, 109.453, 17]` inside the 180px button; `العربية` `[101.141, 665.328, 39.703, 17]`. Last button bottom 692.422. Card bottom 709.42. Modal body `scrollHeight - clientHeight = 0`. The same button rects on the iPhone profile (the overlay is not safe-area padded; it is centered in the 430×932 frame, so top 222 and bottom 709 clear both the notch at 59 and the band at 898).

Settings row ids in `settings_default__zero`: `settings-language-value` "English" `[34, 480, 62.219, 15]`; `btn-open-language` "Change" `[319.219, 461.5, 76.781, 33]`; `btn-open-user-agreement` "View" `[333.641, 624.5, 62.359, 33]`.

### Screenshots

`node web/capture-web.mjs --states states.json --only settings_default,settings_modal_language --out /tmp/maze-i18n/capture/shots` wrote four PNGs. Read back:

- Zero settings: rows aligned, Language under Gameplay, User Agreement under Privacy Policy, Reset and the version line fully on screen, nothing clipped.
- Zero modal: 2-column grid, English selected (yellow), Close pill intact, 17 labels on screen, no scrollbar. Latin, CJK, kana, hangul, Arabic, and Devanagari read as real glyphs. Khmer read as poorly formed. Those non-Latin glyphs are macOS fallback, not Fredoka (see risks).
- iPhone settings PNG also shows the footer, but the JSON run bottom 904 still fails the safe-area test. The capture does not draw the home indicator, so the picture looks fine while the test would not.

### Capture trap

An unmapped emoji infinite-loops the harness. `page-init.js` replaces emoji text nodes (lines 440–466) and a `MutationObserver` rescans every added node (lines 534–547). A section title `🌐` (U+1F310) is not in the 57-grapheme `assets/emoji/index.json`. The unmapped path puts the same text back, the observer fires again, and `page.goto` never reaches `load` (timed out at 120s, then at 20s). Drop that emoji or add it to the pinned set before capturing. The shipped title in this spike is the word "Language" only.

## 4. Unity touchpoints

Naming: `ParityViewer.IsGamePage` treats every id that starts with `modal_` as a game-page sheet, and calls out that settings sheets are `settings_modal_*` (`Assets/Scripts/Viewer/ParityViewer.cs` lines 815–820). The new file must be `settings_modal_language`, not `modal_language`. `QuotaAndModalTests` already asserts `settings_modal_privacy` and `settings_modal_reset_confirm` are not game pages (lines 71–72).

| Place                                           | Change                                                                                                                                                                                                                                                                                                                                                        |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `TemplateSelector.Select`                       | `modal == "language"` returns `settings_modal_language` (privacy/reset are lines 72–73). `Settings()` (lines 212–216) stays `settings_sliders_moved` / `settings_toggles_off` / `settings_default`, but those three JSON files must be recaptured so the new rows are in the paint.                                                                           |
| `AppController.ModalOrder`                      | Add `"modal-language"` (lines 38–41).                                                                                                                                                                                                                                                                                                                         |
| `AppController.ChooseTemplate`                  | Map `_modals` contains `"modal-language"` to `modal = "language"` (lines 1152–1159).                                                                                                                                                                                                                                                                          |
| `AppController.Click`                           | `btn-open-language` opens the modal; `btn-close-language` closes it; each `btn-lang-*` sets the current language and closes; `btn-open-user-agreement` opens the URL. Privacy is lines 420–427. If both policy rows become URLs, `btn-open-privacy` stops calling `_modals.Add("modal-privacy")` and `settings_modal_privacy` is no longer the live template. |
| `ParityPages.InteractiveIds`                    | `btn-open-language`, `btn-close-language`, `btn-open-user-agreement`, and the 17 `btn-lang-*` ids. Class `lang-option` is optional if every button has an id.                                                                                                                                                                                                 |
| `ParityPages.Edges`                             | Only if the viewer keyboard nav should open the picker. Not required for `AppController`.                                                                                                                                                                                                                                                                     |
| `ParityPages.MainTabPages` / `PrewarmTemplates` | No change. Comment at line 41: prewarm is home, maps, daily, shop, achievements. Not settings.                                                                                                                                                                                                                                                                |
| `HomeChrome.Applies`                            | No switch edit required. After the explicit cases it returns true when `Resources/Parity/Spec/<id>__zero` or `__iphone` exists (lines 43–44).                                                                                                                                                                                                                 |
| `PageCatalog`                                   | `LoadZeroSpecs` discovers files. `GroupOf` already buckets `settings*` (line 58). `MenuSpecIds` (lines 32–45) stays on `settings_default` unless a menu entry for the picker is wanted.                                                                                                                                                                       |
| `PagePreviewMenu`                               | Optional menu item next to `settings_default` (lines 27–28).                                                                                                                                                                                                                                                                                                  |

### Tests

| Test                                                                     | What happens                                                                                                                                                                                                                                                                                                                 |
| ------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `IphoneSafeAreaTests.EveryZeroSpec_HasAnIphoneSpec`                      | Asserts count `>= 44` and a twin `__iphone` file (`IphoneSafeAreaTests.cs` lines 37–47). Adding a pair passes. The visible-text test (lines 111–115) **fails** on the new iPhone settings specs until the footer clears y=898. Text is not in `BoxMayCrossHomeIndicator` (lines 28–34); that list is only for control boxes. |
| `PagePreviewTests.Catalog_GroupsSpecsAndOffersIphoneForEveryPage`        | `>= MenuSpecIds.Length`, not an exact count. New `settings_*` group themselves.                                                                                                                                                                                                                                              |
| `PagePreviewTests.CapturePose_MatchesTheEditModePaintPath`               | Paints `settings_default` (lines 16–22) and compares two painters. It does not pin a stored PNG. It still passes if both painters see the new spec.                                                                                                                                                                          |
| `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` | Asserts `screenshot-states.json` length **23** (line 81) and byte-matches `tools/parity/out/unity-before/<id>__zero.png`. The only settings id in that list is `settings_default`. That still changes. The other 22 do not. The test ignores when the still directory is absent.                                             |
| `TemplateSelectorTests`                                                  | Lines 73–82 pin `settings_default`, `settings_toggles_off`, `settings_sliders_moved`, `settings_modal_privacy`, `settings_modal_reset_confirm`. Add a language case.                                                                                                                                                         |
| `ToastAgeTests`                                                          | Loads `settings_default` (line 24) as a toast host. It does not pin settings pixels. It keeps passing if the spec still loads.                                                                                                                                                                                               |
| `TabPrewarmTests`                                                        | Unchanged. Settings is not a prewarm tab.                                                                                                                                                                                                                                                                                    |
| `AppFlowTests`                                                           | Replays every `golden/app/*.json` (`AppFlowTests.cs` lines 14–19). Those traces do not include settings DOM labels.                                                                                                                                                                                                          |

### Goldens

`golden/app/settings_reset.json` records `bootHome`, clicks on `#btn-home-settings`, `#btn-reset-game-data`, `#btn-confirm-reset`, and a `settings` op (`sound`, `haptics`, `sfx`, `ambience`). No row labels. It does not change unless language is added to the profile snapshot or a flow clicks the new controls. The other eight `golden/app/*.json` files do not visit the new controls. `sequences.json` `settings_toggles` likewise stays valid.

### Baselines that change

Regenerate these distilled specs (zero and iphone). They are the settings paints that show the page:

- `settings_default`
- `settings_toggles_off`
- `settings_sliders_moved`
- `settings_modal_privacy` (blurred page behind the sheet includes the shifted rows)
- `settings_modal_reset_confirm`

Add:

- `settings_modal_language`

Player still that changes, when `out/unity-before` is present:

- `settings_default` inside `tools/parity/unity/screenshot-states.json` (one of 23)

Do not change: home, maps, daily, shop, achievements, game, loading, pause, debrief, defeat, toast stills, and the other 22 screenshot-states entries. Do not regenerate those spec JSON files. Hidden modal nodes would change the JSON element count without changing existing rects (`home_default` shared ids: 0 rect/run diffs, plus 24 new ids).

`gen-states.mjs` also has settings states that are not distilled into `Assets/Resources/Parity/Spec` today (`settings_haptics_clicked_off`, `settings_sound_toggle_t0100`, `settings_sliders_profile`, `settings_modal_privacy_t0150`, `pressed_back_settings`, `pressed_privacy_dismiss`). They have no Unity baseline until someone distills them. `settings_reset_confirmed_toast` captures home, not settings.

## 5. What is translated

One English capture is enough for the picker grid if Unity overstamps two strings.

| String                                                                                                                                                                      | id                        | Translate?                                                                                                                                                                                                                                                                                                                         |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| English, 简体中文, 繁體中文, 日本語, 한국어, Deutsch, Français, Español, Italiano, Português, Tiếng Việt, Filipino, Bahasa Indonesia, Oʻzbekcha, ភាសាខ្មែរ, हिन्दी, العربية | `btn-lang-*` runs         | Never. Own script, every locale.                                                                                                                                                                                                                                                                                                   |
| Current value under the Language row                                                                                                                                        | `settings-language-value` | Not a translation. It is the selected endonym. Overstamp it when the language changes. "Bahasa Indonesia" is the longest Latin label (run width 109px in a 180px grid button at 14px). On the settings row it is 11px Nunito; a long endonym can wrap and push the iPhone footer further. Not measured in a non-English selection. |
| "Choose Language"                                                                                                                                                           | `language-modal-title`    | Yes.                                                                                                                                                                                                                                                                                                                               |
| "Close"                                                                                                                                                                     | `btn-close-language`      | Yes. The pill grows with the word (`padding: 0 12px`). A long translation has to stay clear of the title in the header.                                                                                                                                                                                                            |
| "Language", "Change", "User Agreement", "View", "Privacy Policy", "View Policy"                                                                                             | settings labels           | Yes, when the settings page itself is localized. They are English in this capture.                                                                                                                                                                                                                                                 |

Do not capture 17 copies of the page for the endonyms. Capture English chrome once, keep the endonym runs, and replace title, Close, and `settings-language-value` by id. Privacy and User Agreement body copy is not in the game if those rows only open a URL.

## 6. Effort

| Work                   | Days | Notes                                                                                                                                                                                                                                     |
| ---------------------- | ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Web                    | 1.5  | Rows, centered grid, profile field for the selected language, URL handlers, iPhone footer tightened by at least 6px, no unmapped emoji.                                                                                                   |
| Capture and spec regen | 0.5  | This Mac can distill the five existing settings specs plus `settings_modal_language`. Do not regen other pages. Run Prettier after `gen-states.mjs`.                                                                                      |
| Unity wiring           | 2    | Template, modal order, click, 20 hit ids, `OpenURL`, overstamp of title / Close / current endonym.                                                                                                                                        |
| Tests and baselines    | 1.5  | Recapture the five settings specs (both profiles), add the new pair, update `TemplateSelectorTests`, fix the iPhone footer so `IphoneSafeAreaTests` passes, replace the `settings_default` player still if `out/unity-before` is in play. |

Fonts are extra and are the long pole: about 2–4 days on top of Unity wiring if the picker must match in-game rather than rely on the system. Not included in the 5.5 days above.

## Risks and questions for the lead

1. Pinned fonts do not cover the grid. `assets/fonts/Fredoka-700.ttf`, `Nunito-800.ttf`, and both Noto Symbols files lack every codepoint of 简体中文, 繁體中文, 日本語, 한국어, ភាសាខ្មែរ, हिन्दी, and العربية. Fredoka also lacks Vietnamese U+1EBF `ế` and U+1EC7 `ệ`, and U+02BB in Oʻzbekcha (Nunito has those three). Buttons use `--font-bubble` (Fredoka). The harness chain ends in `sans-serif`, so this Mac drew system fonts and the spec run widths are those widths. Unity's symbol fallback will draw `.notdef`. A language spec captured before the fonts are pinned will not match the Unity paint.
2. iPhone footer text bottom 904 vs limit 898.25. `IphoneSafeAreaTests` fails on every settings spec that shows that line until the page loses about 6px, more if a long endonym wraps the Language row.
3. Unmapped emoji hangs capture (`🌐` did). Stay inside `assets/emoji/index.json`.
4. `modal_*` vs `settings_modal_*`. The wrong prefix is treated as a game page.
5. Regenerating untouched pages rewrites JSON (new hidden nodes) even when existing rects match. Leave them.
6. `settings_default__zero.json` is value-identical and not byte-identical, because `t0` sits at the end of the committed file. A fresh rewrite moves `t0`. Not a pixel change.
7. Should Privacy Policy stay a modal or become `OpenURL` like User Agreement? If it becomes a URL, `settings_modal_privacy` is unused at runtime. The spec should still be regenerated or deleted so the blurred backdrop is not the old page.
8. Does `settings.language` enter the profile snapshot? If it does, `golden/app/settings_reset.json` and `sequences.json` `settings_toggles` need a decision. They do not change for a paint-only row.
9. Overstamp title and Close, or capture per locale? Endonyms must not be translated either way.

## What was verified, and what was not

Verified: harness boot, CDP safe area, offline dump equality, four-spec distill diff, home shared-id stability, dry-run dump and distill, scroll and button boxes, font cmap coverage via fontTools, and the three screenshots listed above (read back as images).

Not verified: Unity import, `HomeChrome` paint, EditMode tests, D3D12 still bytes, Khmer shaping quality beyond the screenshot read, and a settings row whose value is a long endonym. Node modules installed by `npm ci` under `tools/parity/node_modules` are gitignored and were left in place. Playwright browsers are in the user cache.

## Scratch

`/tmp/maze-i18n/capture/`

- `distilled/` — unchanged-tree distill of `settings_default` and `settings_modal_privacy` (zero and iphone), plus `_keyframes.json`
- `web/spec/` — raw dumps for that determinism run
- `offline/spec/settings_default__zero.json` — offline raw dump
- `new/spec/` and `new-distilled/` — dry-run `settings_default` and `settings_modal_language`
- `shots/` — PNGs, layout JSON, traces, `_meta.json` for those two states, both profiles
- `home-check/` — `home_default__zero` distill used to prove shared ids did not move
- `lang-probe.json` — per-button boxes and the modal scroll measurement
