# Changelog

All notable changes to the **Orbital Salvage: Space Trash Clean** project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

Paths written as `tools/parity/...` in entries dated before 2026-10-08 are the location on that day. Those files now live under `games/maze-puzzle/tools/parity/...`. Entries that name `tools/parity/unity/*.ps1` or `canvas-conformance/run-unity.ps1` record the path used that day. Those Windows wrappers now live under `games/maze-puzzle/tools/parity/windows/`.

### Fixed

#### Identical English copies per language (2026-10-12)

- `notes/<code>-identical-ok.json` lets a key stay equal to English without an `untranslated` error. German `Hangar` and `Reset` use that file. `merge_table.py` recopies `doNotTranslate` from English and no longer keeps language-specific entries.

#### German wording after the layout audit (2026-10-12)

- Refine German wording after the layout audit: daily pill FERTIG, Nochmal, FREI, Hangar, Reset, Weltraum-Melodie, Spielladen, Putz-Paket. Validator errors 0.

#### Translation kit validator (2026-10-12)

- `validate_table.py` counts `kg`, `/`, and `+` as tokens, so a word such as Background is not a unit. `{0} kg` may be copied. Parenthesis counts follow English, including fragments cut off mid-token. Glossary aliases cover `LVL`, `space junk`, and Auxiliary Scrap Pod, and a term inside the logo is not a hit. Accepted fit warnings live in `localization/notes/<code>-fit-exceptions.json`. The German table stays at 0 errors; fit warnings on `LVL 1`–`LVL 50` are suppressed (78 warnings before, 28 after).
- The translator brief now names `Assets/Resources/Localization/<code>.json` as the deliverable. Close-type buttons are verbs. The glossary notes that Backup Pod and Auxiliary Scrap Pod are one thing.

#### Unity localisation run stacking and single-line controls (2026-10-12)

- A translated run that grows shifts the next run in the same element down by the extra line height. The ceiling is the element bottom, the next same-parent sibling, and the iPhone home-indicator band, minus the captured height of the runs below. The settings footer lines no longer draw on top of each other.
- A single-line control (a button, or a run no taller than `1.5 * font + 2` inside an element shorter than `2.2 * font`) shrinks to 72% and ellipsizes on that one line. Wrapping is only for a run that was already multi-line, or a text block taller than two lines. The audit kind `wrapped-in-control` is hard. `soft-control-ellipsis` is the soft category for a control that ellipsized.
- An icon that already overlaps the English run, including an emoji wrapped in a span, is reserved only when the translation is wider. A shorter translation stays on the captured x. The home play ▶ stays put.
- `qps` audit: 90 spec files, 2006 runs, 88 fragment elements, hard 0 (outside 0, overlap 0, band 0, wider 0, wrapped 0), soft 1914 (shrink 998, ellipsis 118, wrap 0, control ellipsis 798). Focused EditMode (`TextFitTests|LocRenderHookTests|LocPseudoStillTests`): 10 passed, 0 failed. Results `/tmp/maze-i18n/loc-render/focused.xml`, log `/tmp/maze-i18n/loc-render/focused.log`. Full EditMode: 501 passed, 83 failed, 3 skipped, 587 total, exit 2. Results `/tmp/maze-i18n/loc-render/full.xml`, log `/tmp/maze-i18n/loc-render/full.log`. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). The skips are `FrameRateHarness.Measure`, `FrameRateHarness.MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.

### Added

#### German translation table (2026-10-12)

- German translation table (de): 355 spec + 81 str keys, validator errors 0.

#### Translated data names in C# overrides (2026-10-12)

- `Loc.TrName` translates an English data name that a C# override inserts. English returns that string and does not open a table. Any other language checks a `data.*` registry row, then the normalised spec table. The home mission title, pause level name, world title, HUD sector code, cleanup toast, and trophy toast pass their names through it. The per-frame rebuild caches the lookup by text and language version.
- Every `LVL n` code, level name, world title, and achievement title that those overrides insert is already a spec key, so no `data.*` rows and no `home.level.code` format were added. The str table stays 81 keys. Context notes on the four format strings were regenerated into `key-index.csv`. The spec section of `en.json` is unchanged.
- Viewer toast-proof literals stay English and are listed in `localization/hardcoded-allowlist.txt`. `MasterAction` stays allowlisted.
- Verified with Unity 6000.4.0f1 EditMode. Extract wrote spec=355, str=81, skip=50, files=45. Focused filter (LocStringTests, LocHardcodedStringTests, SpecExtractorTests, AppFlowTests, GameplayLabelTests): 24 passed, 0 failed. Full suite: 510 passed, 83 failed, 3 skipped, 596 total, exit 2. The 83 failures are the known set (73 ReferenceComparisonTests, 9 ReferenceGateTests, 1 QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes).

#### C# string registry (2026-10-12)

- User-visible C# literals (toasts, loading tips, mission and pause lines, shop footers, gift copy, HUD quota, daily countdown, ring hub words, gate badges, and floating words) now go through `Loc.Tr`. English is the `LocStrings` registry, so it does not read `en.json`. A missing translation falls back to that English. Placeholder formatting stays invariant, and numbers still use `JsNumber.ToString`.
- The extractor writes the `str` section of `en.json` from the registry in ordinal key order, and appends `kind=str` rows to `localization/key-index.csv` (context note, placeholder count, max-character hint, emoji flag). Spec keys are unchanged. Two CSV columns, `placeholder_count` and `max_chars`, are blank on spec rows.
- `LocHardcodedStringTests` scans the UI sources and fails on a new English literal that is not `Loc.Tr` or in `localization/hardcoded-allowlist.txt`. `LocStringTests` checks registry use, byte-identical English, and that `qps` changes the app overrides while keeping numeric arguments. Gate badges and the HUD quota cache the resolved string per language version so a steady English frame does not allocate.
- The registry is 81 keys (loading 5, toast 13, home 13, hud 3, unit 3, dock 4, gift 8, pause 1, shop 4, daily 1, fx 26). Logic catalog, level, world, achievement, and daily-shift tables are still English.
- Verified with Unity 6000.4.0f1 EditMode. Extract wrote spec=355, str=81, skip=50, files=45. Focused filter (AppFlowTests, ShopGiftTests, StartLevelTests, EconomyGoldenTests, SpecExtractorTests, LocTableTests, LocStringTests, LocHardcodedStringTests, GameplayLabelTests): 45 passed, 0 failed. Full suite: 507 passed, 83 failed, 3 skipped, 593 total. The 83 failures are the known set (73 ReferenceComparisonTests, 9 ReferenceGateTests, 1 QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes).

#### Per-language outline font chains (2026-10-12)

- `Assets/Resources/Localization/fonts.json` maps each font group to display and body faces and a fallback list. `LocFonts.GetChain` reads it. A missing or broken file keeps the Fredoka and Nunito names and does not throw. `OutlineFonts.ChainFor` skips a face whose `.bytes` file is missing.
- English `PaintRuns` still takes the captured loop when `Loc.EnglishActive` is set and `TextLangOverrides` is empty. `ChainFor("en")` returns the same font array as `Chain(Primary)`. Latin languages other than English insert Nunito at the same CSS weight after Fredoka, which covers `ế`, `ệ`, and U+02BB.
- The four CJK picker endonyms ship as temporary subsets: `ZCOOLKuaiLe-endonym` 7004 bytes, `Huninn-endonym` 13708, `MPLUSRounded1c-endonym` 12208, `Jua-endonym` 22868, each with its OFL text. Baloo 2, Kantumruy Pro, and Cairo are named in the manifest and not shipped. `km`, `hi`, and `ar` still draw boxes.
- The 17 `btn-lang-<code>` runs use that language's display chain while the settings page or the language modal is open. A `missing-glyph` audit finding is hard. Rebuild with `localization/tools/make_fonts.sh`.
- Focused EditMode (`LocFontRegistryTests|LocFontChainTests|LocFontPickerTests`): 10 passed, 0 failed. Results `/tmp/maze-i18n/loc-fonts/focused.xml`, log `/tmp/maze-i18n/loc-fonts/focused.log`. Full EditMode: 511 passed, 83 failed, 3 skipped, 597 total, exit 2. Results `/tmp/maze-i18n/loc-fonts/full.xml`, log `/tmp/maze-i18n/loc-fonts/full.log`. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). The skips are `FrameRateHarness.Measure`, `FrameRateHarness.MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.

#### Translation kit for the 16 target languages (2026-10-12)

- `localization/glossary.md` is the controlled glossary (49 terms) with a recommended draft for each target language, do-not-translate flags, and the per-language style rules.
- `localization/translator-brief.md` is the prompt for a per-language translation agent. `localization/process.md` is the path from extract and string freeze through validation, a 30-key sample, the pseudo-locale audit, and a Translation Verification Report.
- `localization/tools/validate_table.py` checks a table against `en.json` using the `LocValidator` rules plus empty strings, untranslated copies, script, placeholders, emoji, digits, glossary warnings, expansion, line-box fit, stale source hashes, and missing glyphs. `fonts-chains.json` names the shipped Latin faces and the pending faces. Stubs go to a scratch directory. `merge_table.py` folds a partial table back in and writes `meta.sourceHashes`.

#### Settings language picker and legal rows (2026-10-12)

- Settings gains a Language row and a User Agreement row. Change opens `settings_modal_language`, a two-column grid of the 17 endonyms with no scroll. The current language is a leading `✓ ` (`U+2713`) on that button. The English capture marks English. Unity overrides `settings-language-value` and the two marker buttons from the saved code. `HomeChrome.PaintRuns` is unchanged, so a non-English marker stays at the captured run origin.
- Privacy Policy and User Agreement both open `https://www.bing.com` from `LegalLinks` (App) and `LEGAL_LINKS` (`web/main.js`). The privacy modal markup and its specs stay. `btn-open-privacy` no longer opens the modal. Capture calls `openPrivacyModal`. A Kids Category parental gate is a TODO and is not built.
- A tap saves the registry code and closes the picker, including a code whose table is missing. The painted chrome stays English until that table loads. The marker follows the saved code.
- The settings page is tightened (`#page-settings .page-scroll-body` gap 12px, footer margin 2px) so the iPhone footer second line clears the home-indicator band.
- Recaptured `settings_default`, `settings_toggles_off`, `settings_sliders_moved`, `settings_modal_privacy`, and `settings_modal_reset_confirm` for zero and iphone, and added `settings_modal_language` for both profiles. No other spec was regenerated. `screenshot-states.json` and `golden/app/settings_reset.json` are unchanged. `out/unity-before` is not in this checkout, so the settings player still was not repainted.
- The extractor treats every `LanguageRegistry` endonym, and that endonym with the marker, as never-translate. `en.json` is 355 spec keys and 50 do-not-translate runs (was 348 and 32). `localization/key-index.csv` was regenerated from the same run.
- Focused EditMode (`SettingsLanguageFlowTests|SpecExtractorTests|TemplateSelectorTests|IphoneSafeAreaTests|MenuPathTests|GamePageModals_AreLiveTemplates|CapturePose_MatchesTheEditModePaintPath|Catalog_GroupsSpecs`): 20 passed, 0 failed. Results `/tmp/maze-i18n/loc-settings/focused.xml`, log `/tmp/maze-i18n/loc-settings/focused.log`. Full EditMode: 484 passed, 83 failed, 3 skipped, 570 total, exit 2. Results `/tmp/maze-i18n/loc-settings/full.xml`, log `/tmp/maze-i18n/loc-settings/full.log`. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). The skips include `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.

#### Unity localisation render hook (2026-10-12)

- Non-English paint looks up each spec run and lays it out with `TextFit` inside the captured element box. English `PaintRuns` still takes the captured path after one `Loc.EnglishActive` check per run. A language change bumps `Loc.Version`. The next frame drops chrome bakes and rebuilds one resident page per frame.
- Line breaking is Latin (same splits as `WrapWords`), CJK with kinsoku, or Korean. `km`, `hi`, and `ar` still use the Latin breaker. Non-English `text-transform: uppercase` goes through `LocCase.Upper`. German `ß` becomes `SS`. Caseless scripts are not uppercased.
- `qps` is a hidden pseudo-locale built in memory from `en.json`. It is not in the language list and is not saved. Boot reads PlayerPrefs `os.parity.lang` only when that key is set.
- The overflow audit is `Orbital Salvage/Tools/Localization Overflow Audit` (priority 401) and batch `OrbitalSalvage.Editor.LocOverflowMenu.RunBatch`. On `qps`: 88 spec files, 1886 runs, 86 fragment elements, hard 0 (outside 0, overlap 0, band 0, wider 0), soft 1454 (shrink 618, ellipsis 723, wrap 113).
- Full EditMode: 495 passed, 83 failed, 3 skipped, 581 total, exit 2. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` stayed skipped. Results: `/tmp/maze-i18n/loc-render/full.xml`.

#### Unity localisation runtime (2026-10-12)

- The localisation runtime is `OrbitalSalvage.Loc` (no Unity engine reference, so Logic and App can call it). The Resources loader and the device-locale provider are `OrbitalSalvage.Loc.Unity`. Nothing in the paint path reads a table. English spec lookup returns false, and `settings.language` is omitted while empty, so an unset profile matches the economy golden byte for byte.
- Seventeen languages are registered. `en` is the fallback. A language other than English is active only when its table loads. Boot stores a detected non-English code only in that case, and that stored value is then authoritative. A progress reset keeps the language.
- The spec extractor writes `Assets/Resources/Localization/en.json` (348 identity spec keys, 32 do-not-translate runs) and `localization/key-index.csv`. The menu is `Orbital Salvage/Tools/Extract Localization Spec Strings` at priority 401, next to Reset Saved Profile. Boot does not rewrite a stored language tag; `SetLanguage` is what writes a registry code.
- Focused EditMode `OrbitalSalvage.Loc.Tests`: 15 passed, 0 failed. Full EditMode: 480 passed, 83 failed, 3 skipped, 566 total, exit 2. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` stayed skipped because the parity stills are not in this checkout. The other PagePreview tests passed.

#### Unity app icon (2026-10-12)

- The default app icon is now the Orbital Salvage icon: a cute black-hole mascot inside a pink and cyan vortex, pulling in a cheese wedge, a spiky gold star and a donut. The source is `Assets/Icons/AppIcon.png` (1024x1024, opaque RGB, no pre-rounded corners). The three concepts Gemini 3.8 Flash produced, and the chosen one (`orbital-salvage-icon-alt1-1024.png`), are under `resources/app-icon/`.
- `AppIconSetup` (menu `Orbital Salvage/Build/Apply App Icon`, batch `OrbitalSalvage.Editor.AppIconSetup.ApplyBatch`) configures the importer (no alpha, no mipmaps, uncompressed) and assigns the texture as the project default icon. iOS generates every size from it. The `ProjectSettings.asset` change is only the `m_BuildTargetIcons` entry; other fields Unity reserialises on open are left out.
- The iOS device export writes `AppIcon.appiconset` with the 1024 store icon and the 180, 120, 87, 80, 60, 58, 40 and 29 px icons (all RGB). The icon on the iPhone X home screen was read back through SpringBoard and matches. The bundle id stays `io.dev.maze`.
- `AppIconTests` (4) check the 1024 size, that the source has no alpha, that the default icon is the project icon, and that the bundle id is unchanged.

#### Unity GPU cost, playbatch A11 fix (2026-10-11, `agent/perf/gpu-cost`)

- The phone corruption is in the batch path that `c2fafd0` took off the default. `playbatch` stays off that default (`rrshadow,rrplus,backdroplow,playhalf,fastshape,fastarc`) and remains a pref token. The phone still needs a screenshot pass.
- Pass 1 of `FastShape.shader` at `c2fafd0` (lines 202–215) indexed `_C2D_FastBase` with `SV_InstanceID` and carried the param index to the fragment as one float `TEXCOORD`. A11 stores that interpolator as fp16, which is exact only through 2048. Param blocks are 28 float4s, so a later quad keeps the position the vertex computed from the uint and shades a rounded index. Mode 0 forces a solid quad (the white and pink debris squares), an arc mode draws the crescents, and a neighbor block paints the mission card black.
- `FlushFastBatch` at `c2fafd0` (`Canvas2D.Opt.cs` 835–840) recorded `SetBufferData` into the same buffer at index 0, rewound the CPU array, and drew 6 vertices by `n` instances. That recorded copy reads the array when the command buffer runs, so an earlier batch sees the last upload. A11 `instance_id` also omits a base-instance offset, so a non-zero base would still read index 0.
- `AccumulateTo` bound the coverage target and then `Draw` flushed the batch (`Canvas2D.Gpu.cs` 866 at `c2fafd0`, flush at `Canvas2D.Opt.cs` 838). The flush rebound the canvas, the coverage pass landed there, and the composite sampled an empty accum. On the Mac split-batch test that gradient pixel was clear until the flush moved to before the target switch.
- Indices now append for the whole submit and upload once with `GraphicsBuffer.SetData` before execute. Each batch records `_C2D_FastOffset` and draws `6 * n` vertices as one instance, so the shader uses `SV_VertexID` only. The index crosses the fragment as two bytes, each exact in fp16. The batch flushes before a coverage pass, a clip, a shadow slot, or a backdrop copy leaves the canvas.
- `PlayBatch_SecondBatchDoesNotReuseIndexZero` draws two batches split by a gradient, requires the second start offset to be above 0, and compares pixels within 8. `PlayBatch_IndexPastFp16MantissaMatchesUnbatched` stacks 300 circles (last index 8372). Focused `Canvas2DGpuTests|GpuOptTests`: 26 passed. Full EditMode: 461 passed, 83 failed, 3 skipped, 547 total, exit 2. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). Batch iOS export exited 0, log `/tmp/maze-gpu/ios7.log`, no `error CS`, `[IosExport] result=Succeeded`.

#### Unity GPU cost, playbatch off the phone default (2026-10-11, `agent/perf/gpu-cost`)

- iPhone X on `79400e9` (defaults on, footprint 461 MB, avail 1388 MB): home 42 ms / 29 fps, game 41.5 ms / 25 fps. `playbatch` corrupts that A11 frame. The mission card goes solid black, settings and sound buttons go dark with crescents, pills and taglines pick up crescent glyphs, debris becomes white or pink squares, the HUD pill and black-hole face garble, and rotated quads gain dark boxes. The same screens are correct with `rrshadow,rrplus,backdroplow,playhalf,fastshape,fastarc`. The Mac editor still matched, so the suite did not catch it.
- The iOS and Android default is now that set. `playbatch` remains a pref token. The first `[Perf] boot` line's `defaults=` follows the new string.

#### Unity GPU cost, phone defaults (2026-10-11, `agent/perf/gpu-cost`)

- iPhone X after `9a21a48` (scale 2): game with `rrshadow,rrplus,backdroplow,playhalf,fastshape` was 55 ms. Adding `fastarc` was 48 ms. Adding `playbatch` was 39–42 ms (25–27 fps, p95 50 ms). Home with `rrshadow,rrplus,backdroplow` stayed about 63 ms; `dirty`, `dirtyclip`, `bdhold`, and `noprewarm` still did not move it. One playbatch launch dropped the device connection with no crash report; the retry ran.
- iOS and Android players now start from `rrshadow,rrplus,backdroplow,playhalf,fastshape,fastarc,playbatch` when `os.parity.opt` is empty. The Mac and Windows editor, the stills tooling, and the hash tests keep `GpuOpt.Defaults` empty, so their output does not change. A saved pref replaces that set entirely. `none` and `default` turn every option off for an on-device A/B. The first `[Perf] boot` line logs `active=` and `defaults=`.
- Phones whose `SystemInfo.systemMemorySize` is 1–3500 MB, including iPhone X, cannot select raster scale 3. Fresh fit, a saved 3, and a governor step-up all stay at 2. A reported size of 0 is treated as unknown and is not capped. Higher-RAM phones still use `{2, 3}`.
- `[Perf]` adds `footprint=` and `avail=` from an iOS plugin (`task_vm_info.phys_footprint` and `os_proc_available_memory`). Both are 0 in the editor and on every other player. `clang -fsyntax-only` against the iPhoneOS SDK accepted `Assets/Plugins/iOS/OSMemory.m`. The device export copies it into the Xcode Sources build phase.
- Options off, Mac Metal, raster scale 2: the four stills still match the known hashes (`home_default` `546655D06D8526B2E1C7725239DDEFE5648F1538A2C23250D7291CC18EF5F4BA`, `maps_world_1_fresh` `EB3056E32FEF8EB28F306667D35B6438504BA5BB631357573741D6A56B44E6FF`, `game_lvl01_t1000ms` `5A8DBCDD9D3E156B0835038CF254664BDCCDD212F9ECE6C6EA84174CE480A106`, `modal_pause` `8E56BF68DF4A3427AD7B2DAAB11456B0EDDCF47666A02C0E626F9FE2EB6DA257`). The recorded game still does not go through `GameRenderer`, so `playhalf` does not change it.
- Live level-1 playfield, same seed, one 1/60 s step (a 60-step readback in this batch session came back empty): backing 1290×2796 at scale 1, 645×1398 with `playhalf`. Point-sampled into the raster-2 page (860×1864), which is how the page draws the playfield: mean channel abs 0.714, max 218, 129544 of 1603040 pixels differ (8.08%), 38968 differ by more than 8 (2.43%). The HUD stays at the page raster. PNGs: `/tmp/maze-gpu/playfield-full.png`, `playfield-half.png`, `playfield-full-page.png`, `playfield-half-page.png`. `rrshadow` on the home still was already max channel 10. `backdroplow` is the quarter-resolution backdrop blur.
- Focused `Canvas2DGpuTests|GpuOptTests|GpuDirtyTests|DevicePerfTests|QualityGovernorTests`: 51 passed. Full EditMode: 459 passed, 83 failed, 3 skipped, 545 total, exit 2. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). Batch iOS device export exited 0, log `/tmp/maze-gpu/ios6.log`, no `error CS`, `[IosExport] result=Succeeded`.

#### Unity GPU cost, playfield round (2026-10-10, `agent/perf/gpu-cost`)

- iPhone X after `c9b6ff3` (scale 2, ±10 ms): home `rrshadow,backdroplow` 88, adding `dirtyclip` or `bdhold` did not move it and `bd` stayed 0. `rrplus` on that pair was 63. Scale 1.5 was 43 ms and scale 1 was 20 ms. Game `fastshape` on the earlier stack was 55 ms (19 fps); `noprewarm` did not move the steady frame. `nogamehud`, `nogamefx`, and `nocache` stayed 55–59 ms, and chrome raster scale did not change the game number. The remaining cost is the playfield canvas.
- `[Perf]` field order, unchanged: `shp` and `se` are rect/circle/ellipse/roundrect/stroke/other. The sample `se=1836/3405/0/1368/108398/30037` has 108398 **stroke** edges (fifth field), about 5.7k per frame, not ellipses. `fast` is rect/circle/ellipse/roundrect/line/arc. The sample `fast=0/2682/0/38/608/0` means circles were already analytic and the fat strokes were not. `cmds` counts draw calls (about 880 per frame). Each non-analytic shape switches to the full-size accumulation target and back; on an A11 that load/store is the expensive part, and each stroke edge also shades a quad out to the shape's right bound, so a ring arc shades its bounding disk once per edge.
- The playfield `BeginFrame` clears the full target. Shapes that miss the analytic path then use the full-size accum target. The HUD is a second canvas, but this run was one submit per frame and hiding it did not move the time. `PlayfieldComposite.BlitGpu` is a still-export path, not the live frame.
- New tokens, default off: `fastarc` draws a lone circular arc stroke, and a circle or ellipse under a uniform rotate+scale, as one quad (mode 5 for a partial arc, round/butt/square caps). Dashes, shadows, and gradients stay on the edge path. `playbatch` queues those analytic quads and emits one instanced draw per run. Options off never sets the arc tag and never queues, so the recorded draws stay the previous ones.
- `[Perf]` `sec` is draws/edges for sky (includes the full clear), black hole, ring, flappers, bumpers, gates, debris, vfx. `batch` is instanced draws/instances. `alloc` counts new render textures and graphics-buffer growth. `readpx` counts canvas and chrome readbacks. A CPU hitch over 80 ms now appends `alloc`, `readpx`, `gc`, and `bake` for the previous frame. The 729 ms base GPU spike is not pinned to one call; those four say whether that hitch allocated, read pixels back, collected, or baked.
- Expected, not measured: `fastarc` should remove the ring's multi-hundred-edge strokes from the accum target. That is the path behind the 108k stroke edges and the plausible cut from ~55 ms toward 25 ms. `playbatch` should collapse the remaining analytic draws (the ~140 fast circles per frame, plus the new arcs) into a handful of draws. Neither number has been timed on a phone. Suggested game prefs, one at a time on top of `playhalf,fastshape`: `fastarc`, then `fastarc,playbatch`.
- Options off stills matched the known Mac hashes (`home_default`, `maps_world_1_fresh`, `game_lvl01_t1000ms`, `modal_pause`, `home_live`). Focused `Canvas2DGpuTests|GpuOptTests|GpuDirtyTests|DevicePerfTests`: 35 passed. Full EditMode: 457 passed, 83 failed, 3 skipped, 543 total, exit 2. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). Batch iOS device export exited 0, log `/tmp/maze-gpu/ios5.log`, no `error CS`, `[IosExport] result=Succeeded`.

#### Unity GPU cost, third round (2026-10-10, `agent/perf/gpu-cost`)

- iPhone X (A11, Release IL2CPP, Metal, raster scale 2, 10 s windows) after `rrshadow`: home base 263 ms/frame this run (thermal drift vs 215 earlier; compare only inside that table), `rrshadow` 157, `rrshadow,backdroplow` 109, `rrshadow,backdroplow,dirty` 113, `dirty` alone 271. Game base 137, `playhalf` 110, `rrshadow,backdroplow,playhalf` 86. Home with `rrshadow,backdroplow` still logged `bd=0/2`, five `shadows` on the old blur, and `dirty` always bailed with `area`. Game logged ~1300 cmds, ~20k edges, 2 submits, and `prewarm` of 30–70 ms several times a second.
- The Home bail is `play-btn-glow` / `shineSweep`. The captured box is about 685×399 CSS and `BoundsOf` adds at least 48 px of shadow reach, which is ~96% of the 430×932 page, so every ambient frame full-painted. The painted band is clipped to the play button. `BackdropHitsMotion` uses that same AABB, so it overlaps every backdrop and `BackdropReuseId` stayed 0. The five old-path Home shadows are rotated or inset (diorama ring, pedestal rim, nibbles), which `rrshadow` rejects. Game `prewarm` is `PumpTabPrewarm` baking or repairing a resident chrome during play, not a playfield shader. The second submit is the playfield `EndFrame` plus the HUD chrome `EndFrame`.
- New tokens, default off, same `os.parity.opt` list: `dirtyclip` repaints per mover and uses the play-button clip (plus a 36 px pad) for the shine instead of the captured AABB. A backdrop whose rect overlaps a mover is repainted with its descendants, so the console and dock frost do not cover their labels. `bdhold` arms backdrop reuse even when a mover overlaps and refreshes that cache on every 4th hit. `rrplus` extends `rrshadow` to a rotated card (SDF in local CSS), an inward-gaussian inset, and an unblurred offset for blurred text-shadow. `fastshape` draws an axis-aligned rect, ellipse, rounded rect, or thick ellipse stroke as one quad and flattens circles under 12 px at tolerance 0.35. `noprewarm` skips tab prewarm and resident repair while the page is `game`.
- `[Perf]` appends `shp`, `se`, `gshp`, `gse` (rect/circle/ellipse/roundrect/stroke/other command and edge counts; `g` is the playfield caller), `fast` (rect/circle/ellipse/roundrect/line/other analytic quads), `rrwhy` (xform/inset/text/shader rejects), `bake` (tab bakes during the sample), and `wide` (the mover that exceeded 85%, or `-`). `dirty` alone logs `wide=play-btn-glow`.
- Options off stays on the previous paths. `ApplyClip` ignores a dirty scissor only while it builds the offscreen clip mask; with the scissor off (options off) that path is unchanged. A scale-1 live home pair, full repaint vs `dirtyclip`, differed by max channel 124, mean 0.44, 1461 of 100190 samples over 8. The shine band itself matched after the clip-mask fix. What remains is the pedestal glow fringe and a few glyph edges (the coin readout). Not a byte match, and not timed on a phone.
- Expected, not measured: `dirtyclip` skips static cards outside the holes but still pays the diorama shadows and the console/dock blurs, so it will not by itself reach a 40 ms home frame. `bdhold` should drop about three quarters of the remaining low-res backdrop cost (the asked-for 30 ms is that bet). `rrplus` should take the five old shadows if `rrwhy` shows xform and inset; those blurs are smaller than the mobile-frame shadow that made `rrshadow` worth 105 ms. `fastshape` removes per-edge fragments for the shapes it accepts; `playhalf` was only 27 ms, so the game frame may still be draw-call heavy — read `gshp` / `gse` / `fast`. `noprewarm` should remove the 30–70 ms game prewarm hitches and the bakes behind the rare long GPU spikes. Suggested prefs: `dirtyclip`; `rrshadow,backdroplow,bdhold`; `rrshadow,rrplus`; game `fastshape` and `noprewarm` separately.
- Focused `Canvas2DGpuTests|GpuOptTests|GpuDirtyTests|DevicePerfTests`: 33 passed, 0 failed. Full EditMode: 455 passed, 83 failed, 3 skipped, 541 total, exit 2. The 83 are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). Batch iOS device export exited 0, log `/tmp/maze-gpu/ios4.log`, no `error CS`, `[IosExport] result=Succeeded`. None of the new tokens was timed on a phone.

#### Unity GPU cost follow-up (2026-10-10, `agent/perf/gpu-cost`)

- Device measurement of the first switch set (iPhone X, A11, Release IL2CPP, Metal, raster scale 2) showed `shadowscope` and `shadowlow` unchanged (home ~213 ms vs base ~215 ms) and `dirty` unchanged (~215 ms). `backdroplow` was the only measured win in the combined run (~215 ms to ~165 ms). This change explains those two misses and adds two new tokens, still default off.
- Product blurred box-shadows are drawn inside an even-odd clip (`CssBox.Draw` builds the clip, then fills the rounded rect). `RenderShadow` only entered the scoped path when `state.clip` was null, so every product shadow stayed on the full-target gaussian. `RenderStats.Shadows` increments before that branch, which is why the shadows counter did not move. With `shadowscope` or `shadowlow` on, a clipped shadow now counts `missclip` and still takes the old path.
- `dirty` used one bounding union of every mover. Home ambient movers are small and scattered (debris, pulses, the mascot), and each bounds includes the shadow reach, so the union exceeded 85% of the page and every ambient frame full-painted. The same token now keeps one rect per motion source (cap 24), plus the previous frame's rects. One rect over 85% of the page, more than 24 sources, a cold cache, or a page transform/opacity still full-paints and counts a reason (`cold`, `fullpage`, `area`).
- New tokens, parsed by `GpuOpt` like the others, default off: `rrshadow` draws an analytic rounded-rect gaussian (erfc of a rounded-box SDF, `Resources/Canvas2D/RoundShadow.shader`) in one quad and skips the mask blur when the CTM is an axis-aligned scale+translate and the shader loads. It is still multiplied by the existing even-odd clip. `playhalf` sets `GameRenderer.RenderScale` to 0.5 once, which resizes the playfield backing store. The bench flags `scale1` / `scale2` only change the chrome raster scale; they do not resize `GameRenderer`.
- The `[Perf]` line appends `edges`, `opt ss/sl/scache/missclip/missfmt/misssmall`, `bd=hit/miss`, `rr`, `dirty=frames/full`, `cold`, `fullpage`, `area`, and `areapct` when a dirty frame ran. The first playing `GpuOpt.Apply` logs once: `[Perf] boot r8= rhalf= rfloat= argb32= argbhalf= shadowblit= premul= roundshadow= fill= active=`. Shader values are `resources`, `find`, or `missing`.
- Options off, Mac Metal, raster scale 2 (860×1864), compared with the merged tree (`agent/chk/ios-device` at `1b94d86`) before this diff: the five stills are byte-identical. `home_default` `546655D06D8526B2E1C7725239DDEFE5648F1538A2C23250D7291CC18EF5F4BA`, `maps_world_1_fresh` `EB3056E32FEF8EB28F306667D35B6438504BA5BB631357573741D6A56B44E6FF`, `game_lvl01_t1000ms` `5A8DBCDD9D3E156B0835038CF254664BDCCDD212F9ECE6C6EA84174CE480A106`, `modal_pause` `8E56BF68DF4A3427AD7B2DAAB11456B0EDDCF47666A02C0E626F9FE2EB6DA257`, live home `010E178B02D70ED4DBA3DE8502AAD224028274294315EC98DAF8D2593F28C77C`. The pause hash differs from the earlier `E6083D8F…` entry because the pause-sheet fix is in that merge. PNGs: `/tmp/maze-gpu/mergebase-*.png`, `off2-*.png`.
- `rrshadow` on, same stills vs that baseline: home max channel 10, mean 0.0717, 500 of 1603040 pixels over 8; maps identical; game identical; pause max 7, mean 0.0155, 0 pixels over 8; live home max 10, mean 0.0477, 408 over 8. `dirty` on matches all five, including live home. `playhalf` is not exercised by these chrome stills. None of the new tokens was timed on a phone.
- Full EditMode before the `[Perf]` assertion update: 449 passed, 84 failed, 3 skipped, 536 total, exit 2, duration 78.5 s. The extra failure was `DevicePerfTests.Lines_UseKeyValueAndOmitThermalWhenMissing` (the line gained keys). After the expected tail was extended, `DevicePerfTests` re-ran 9 passed, 0 failed. The other 83 failures are the known set (`ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`). A second full suite was not run. Focused `Canvas2DGpuTests|GpuOptTests|GpuDirtyTests` before the suite: 19 passed, 0 failed.
- Batch iOS device export of this tree exited 0, log `/tmp/maze-gpu/ios2.log`, no `error CS`, `[IosExport] result=Succeeded`. That export predates the test-only `[Perf]` assertion edit. The phone was not installed.

#### Unity GPU cost switches (2026-10-10, `agent/perf/gpu-cost`)

- PlayerPrefs `os.parity.opt` is a comma-separated list, parsed once by `GpuOpt.Apply` and pushed into `RenderOpts`. Tokens: `shadowscope`, `shadowlow`, `backdropscope`, `backdroplow`, `dirty`, `gameone`, `ambhz=<1-60>`. Unknown tokens are ignored. `GpuOpt.Defaults` is `""`, so every option stays off until the pref is set or that one string is changed. `DeviceExp`, `DeviceBench`, and `ParityViewer.Bench` were not edited.
- `shadowscope` renders each blurred shadow into bounds-sized render targets (DontCare load, Store) and composites that rect. `shadowlow` uses 1/4 of that size when the blur radius is at least 8 px, otherwise 1/2, with an R8 mask when the format is supported (else RHalf, else the old full-target path), an ARGB32 tint, a bilinear blit, and a bitmap cache keyed by shape, radius, and colour. `backdropscope` caches the cropped GPU backdrop while no mover overlaps it. `backdroplow` blurs that crop at 1/4 resolution and upsamples. `dirty` repaints an ambient frame only inside the union of mover bounds (and the previous frame's union), skipping elements and static layers fully outside it. `gameone` composites the playfield under the HUD in one blit and one `GUI.DrawTexture`, and uses the dirty HUD path on the game page when the playfield is not bound into the chrome. `ambhz=<n>` overrides the ambient draw interval; it is separate from the bench flags `amb30` / `amb20` / `amb15`.
- These target the iPhone X home idle (gpu about 630 ms, 11 blurred shadows about 400 ms, two backdrop blurs about 150 ms) and the game page (about 120–150 ms). Expected gains are from that table and from the render-target sizes in code. Nothing in this change was timed on a phone.
- Options off, Mac Metal, raster scale 2 (860×1864), capture pose: the four stills match the hashes taken before this change. `home_default` `546655D06D8526B2E1C7725239DDEFE5648F1538A2C23250D7291CC18EF5F4BA`, `maps_world_1_fresh` `EB3056E32FEF8EB28F306667D35B6438504BA5BB631357573741D6A56B44E6FF`, `game_lvl01_t1000ms` `5A8DBCDD9D3E156B0835038CF254664BDCCDD212F9ECE6C6EA84174CE480A106`, `modal_pause` `E6083D8F43331D120F622606DCF99370CA117A15DA7909BBB816A46053C2208A`. PNGs are under `/tmp/maze-gpu/` (`before-*.png`, `off-*.png`, `on-*.png`).
- Options on, same stills, max channel abs / mean abs / pixels whose max channel exceeds 8: `shadowscope` is byte-identical except the game still (1 pixel, max 1). `shadowlow` home max 2 mean 0.0048 gt8 0; maps identical; game max 12 mean 0.0075 gt8 23; pause max 1 gt8 0; live home max 3 gt8 0. `backdropscope,backdroplow` leaves the four capture stills identical (capture pose uses the CPU blur) and the live home frame differs by max 49, mean 0.1548, gt8 1740 of 1603040, on the mission-card backdrop. `dirty` matches all five frames including live home. All six options together follow `shadowlow` on the capture stills and reach max 49 / gt8 1740 on live home.
- Full EditMode (GPU, no `-nographics`, no filter): 442 passed, 83 failed, 3 skipped, 528 total, exit 2, duration 76.5 s. The 9 new tests passed (`GpuOptTests` 4, `GpuDirtyTests` 1, `Canvas2DGpuTests` shadow/backdrop/dirty-rect 4). The 83 failures are the same set as HEAD `51ab510`: `ReferenceComparisonTests` (73), `ReferenceGateTests` (9), `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`. Skips: `FrameRateHarness.Measure`, `MeasureAccumulated`, `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.
- Batch iOS device export (`-buildTarget iOS -executeMethod OrbitalSalvage.Editor.IosExport.ExportDevice`) exited 0. The log has no `error CS`. `[IosExport] result=Succeeded`. The Xcode project was not opened and the phone was not installed.

#### Unity on-device bench harness (2026-10-10, `agent/perf/device-bench`)

- PlayerPrefs `os.parity.log=1` still logs one `[Perf]` line per second. The line now ends with `gpu_ms=avg/max` when `FrameTimingManager` reports a GPU time, or `gpu_ms=na` when it does not, plus per-second Canvas2D deltas: `submits`, `cmds`, `draws`, `backdrop=exact/gpu`, `shadows`, `blits`, `cachebuilds`. The counters are static ints. With the pref absent, nothing extra is allocated per frame.
- iOS export turns `PlayerSettings.enableFrameTimingStats` on before `BuildPipeline.BuildPlayer` and restores the previous value afterwards. `ProjectSettings.asset` is still written back to the bytes from before the export.
- PlayerPrefs `os.parity.exp` is a comma-separated experiment string, read once at product boot. Flags: `noshadow`, `nobackdrop`, `noloops`, `nocache`, `nogamehud`, `nogamefx`, `noimgui`, `scale1`, `scale1.5`, `scale2`, `scale3`, `f30`, `f15`, `rt16`. All default off. The table is on `DevicePerf`.
- PlayerPrefs `os.parity.bench` (`auto`, or `home`, `maps`, `daily`, `shop`, `achievements`, `game`, `pause`, `tabs`, `gamestart`, `pauseopen`) drives the app with the same click path as a tap. `os.parity.bench.secs` sets the measured window (default 12; the first 2 seconds are warm-up). Tabs and gamestart dwell 3 seconds and log `[Bench] switch` lines. pauseopen dwells 2 seconds. The run logs `[Bench] done`. Bench uses an in-memory profile, so `orbital-salvage-profile.json` is not written, and the governor does not load or save a scale pref.
- Each bench cell closes modals, returns to Home, and reloads level 1 with a full quota before it measures. The chrome page id has to match (pause ids start with `modal_pause`). A mismatch logs `[Bench] state mismatch` and retries once. Game and pause cells restart the sector short of a win or an empty-board fail, so a debrief cannot leak into the next cell. Tabs switches start from Home through the dock click ids.
- `amb30`, `amb20`, `amb15`, and `amb10` cap ambient loop repaints. `os.parity.boot.exp` applies creation-time flags such as `rt16` before the first canvas, including during a bench. `os.parity.bench.exps` is a semicolon-separated list that replaces the default matrix.

### Fixed

#### Unity pause sheet (2026-10-10, `agent/fix/pause-modal`)

- The pause sheet's top corners were square. `.modal-card` is `border-radius: 32px 32px 0 0` with `overflow: hidden`, and the spec has no overflow key, so the header (same purple as the card) painted a square rect into the corner. Descendants of a rounded `modal-card` are now clipped to the card's border box after the card's own shadow, fill, and border. That applies to every modal card, including defeat and debrief headers that are a different colour from the card.
- The live playfield is drawn under the chrome texture, so the overlay's backdrop readback never saw it and the game stayed sharp. While a modal is open the playfield is paused, so the product path downscales `ExternalPlayfield` once (width at most 240), box-blurs that snapshot on the CPU, and draws the cached texture for the rest of the modal. A new texture or a size change builds it again. Closing the modal (or disposing the chrome) releases it. Settled frames do not run a full-screen blur pass.
- The sheet intro faded every `.modal-overlay` in the spec, not only the one with `active`. Pause, privacy, reset, debrief, and defeat dims stacked, and the later overlays painted over the sheet. On this Mac the frame went flat `rgb(15,12,41)` by about 120 ms. The intro now fades the active overlay and slides that overlay's card. The layer cache stays off during the slide, because a baked layer is the settled pose and was blitting the sheet fully open on frame 0.
- The device recording's white rounded rectangle (`/tmp/maze-ios/vid/full_55.png`, about CSS x 3–354, y 188 to the bottom) was not reproduced on this Mac. After the fix, intro frame 0 is the unpaused playfield (the sheet is off the bottom) and the halfway frame is the sheet over a blurred scene, with no opaque white slab. Nothing was measured on a physical iPhone.
- Button lips are the captured `0 4px 0 rgb(30,27,75)` shadow (the resume button's bottom row is that navy). Title, level box, close button, and card width come from the spec (card x 50–380, width 330 on the 430×932 iPhone page). Remaining differences: the web screenshot includes the phone bezel; Unity text is the outline glyph raster rather than Chromium's Fredoka; the one-shot blur is the playfield only, so HUD glyphs are covered by that blur instead of being inside it; emoji sizes follow the captured images.
- `PauseModalVisualTests` (EditMode, GPU, no `-nographics`): 4 passed, 0 failed, exit 0. Corner outside the 32px radius is backdrop `RGBA(28,26,52)` and the flat header is `RGBA(49,46,129)`. A green/magenta playfield is blurred once and reused (`PlayfieldBlurBuilds` stays 1, then 2 after a size change). Intro frames 0–400 step 40 stay under a quarter white (the donut is the white) and the sheet is off-screen at 0 ms and on-screen at 200 ms.
- Full EditMode (no filter): 429 passed, 83 failed, 3 skipped, 515 total, duration 80.3 s, exit 2. Before this change the same suite on this branch was described as 511 total / 425 passed / 83 failed / 3 skipped. The 4 new tests are the difference in the totals. The 83 failures are still `ReferenceComparisonTests` (73), `ReferenceGateTests` (9), and `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`. The skips are unchanged. No new failure.
- `GameAndPauseStills_MatchKnownHashes` still fails the Windows game pin. On this Mac, `game_lvl01_t1000ms` hashed to `CC0F525EF93107760CA4026A2F5C975986CC49D969B02F6432F87FCECFEC28A3` (expected `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`). The pause assert is not reached. Those pins were not edited. `PagePreviewPainter` capture-pose hashes after the clip, on this Mac: `modal_pause` zero `1334D4B208A7D18FFC7846EB566F399888E7948F7C661984C70BA8BC867AAB67`, iphone `F195A4187D3C93879A391D1EAE44CBA4ADC5676211373497621992B8A4E68C3C`. The scale-2 iphone settled paint's custom hash went from `FF7ABF6BF74D828297C644B2171983F0BBD486DF640AE3B5490F50B1F76CAA77` to `ED909D0D2DB753828418D8894433783778D71B17B1DE785B52C6E668F380969F`. The Windows pause pin `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078` will not match once that assert is reached. Defeat and debrief cards use the same clip; their Mac hashes were not measured.
- iOS device export (`IosExport.ExportDevice`, batch, Metal project): `result=Succeeded`, no `error CS`. Output `games/maze-puzzle/output/maze-puzzle` (gitignored). Xcode was not run.
- Renders: `/tmp/maze-pause/before/iphone_settled.png` (square corners), `/tmp/maze-pause/after/iphone_settled.png`, `/tmp/maze-pause/after/iphone_intro_000.png`, `/tmp/maze-pause/after/iphone_intro_080.png`, `/tmp/maze-pause/after/iphone_intro_160.png`, `/tmp/maze-pause/after/iphone_intro_400.png`, `/tmp/maze-pause/after/iphone_blur_underlay.png`. `JsRandom.Global` was not touched.

#### Unity maps tile state (2026-10-10, `agent/fix/maps-tiles`)

- Maps pages were painted from a captured spec. `StampMapsCurrent` moved the PLAY marker and the world tab and left every other tile in the capture's completion state. `maps_world_2` therefore showed levels 11, 12, 13, and 15 complete on a profile that had only finished level 1. A completed tile is 82 px tall (header 20 px) and an unplayed tile is 87 px (header 25 px). A row takes the taller of the pair and the next row starts 10 px below. The old stamp did not apply that.
- The web `renderSectorsPage` rule is the one used: the active sector is PLAY (green tile, star plus " PLAY"), a completed sector is the check star and no status text, anything else is ▶. There is no locked tile. Current wins when a sector is both current and completed. The world banner is ` n/10` for sectors completed in the shown world. The pill uses the captured 0/10 or 10/10 width and keeps the card's right edge.
- `StampMapsCurrent` now takes `completedSectors`. The selected template stays the base. Per-tile state comes from the live profile. Donor geometry is measured from the captured specs, keeping the shortest tile of each kind. Both `zero` and `iphone` are handled. A page that already matches is not mutated, so a fresh profile leaves `maps_world_1_fresh` byte-identical to the unstamped paint. Re-stamping the same profile is a no-op. Cache drop still goes through `DropMapsCaches`.
- EditMode before the fix, filter `MapsTileStampTests`: 4 passed, 10 failed, 14 total, exit 2. Those failures were `LVL 1 state Expected: Completed But was: Unplayed` on both profiles. After the fix, filter `MapsTileStampTests|StartLevelTests`: 20 passed, 0 failed, exit 0.
- Full EditMode (GPU, no `-nographics`, no filter): 416 passed, 83 failed, 3 skipped, 502 total, exit 2. Before this change (HEAD `8348266`): 400 passed, 83 failed, 3 skipped, 486 total. The 16 new tests passed (`MapsTileStampTests` 15, zero and iphone, and `PagePreviewTests.MapsWorld1Fresh_PainterMatchesDirectReadback`). The 83 failures are `ReferenceComparisonTests` (73), `ReferenceGateTests` (9), and `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`. The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.
- Unstamped `maps_world_1_fresh` (zero, `PagePreviewPainter`, outline cache cleared) hashed to `3D1B6909A62B362292A8C2C8D3ABF04BDD9BD33E8672FB0FB79CB814EE76CEFB` on this Mac. A direct HomeChrome readback matched those bytes, and stamping the fresh profile (level 1 current, nothing completed) did not change them. The previously published pin `E8DB19615D6FC8D0299B4990E79BDD36F9600E15E4DB3D5034002DB812D94590` was not reproduced. It was recorded in the 2026-10-09 entry next to the Windows game hash, which Metal also does not reproduce. That pin was not edited. `GameAndPauseStills_MatchKnownHashes` still fails on the Windows game pin `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`. The pause pin `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078` is not reached, because the game assert fails first. This run did not remeasure those two paints.
- iPhone previews (430×932) from the EditMode test: `/tmp/maze-maps/iphone-level1-world1.png` (level_1 done, world 1: 1/10, LVL 1 star, LVL 2 PLAY, 3–10 ▶), `/tmp/maze-maps/iphone-level1-world2.png` (same profile, world 2: 0/10, LVL 11–20 ▶), `/tmp/maze-maps/iphone-levels1-12-world1.png` (10/10, all stars), `/tmp/maze-maps/iphone-levels1-12-world2.png` (2/10, LVL 11–12 stars, LVL 13 PLAY, 14–20 ▶, including 15). Nothing was measured on a physical iPhone. The user's layout screenshots were not available.

#### Unity Mac test run (2026-10-10, `agent/fix/mac-replay-no-render`)

- Flow replay still runs `GameRenderer.Render`, so `GameDraw.Rand` and the physics engine keep sharing `JsRandom.Global` and the golden `randomCalls` count is unchanged. `Canvas2D.SkipSubmit` makes `EndFrame` return before buffer upload, `Shader.SetGlobal*`, `UploadText`, and `Graphics.ExecuteCommandBuffer`. The next `BeginFrame` still clears the command buffer, edge and param counts, glyph records, and backdrop snapshots. Edge, param, and glyph arrays keep a high-water capacity. `Flush` and `Replay` throw, because they execute in order to read pixels back. `GameRenderer.Render` only calls `BeginFrame` and `EndFrame`. `LiveGameSession.SubmitEnabled` (default true) forwards to that canvas. `AppFlowTests.Flow_MatchesWeb` sets it false after `CreateBound`. Play mode and stills keep the default.
- Player Settings `metalAPIValidation` is 0, from the previous commit on this branch. A Mac EditMode run rewrites the rest of `ProjectSettings.asset`; that rewrite is not kept.
- `Canvas2DGpuTests`: 8 passed, 0 failed, duration 0.284 s, exit 0. The five existing pixel tests passed with the switch off. `SkipSubmitOff_EndFrameStillExecutes` recorded one `ExecuteCommandBuffer` and one `PaintProfile` flush, and the filled pixel was still readable. `SkipSubmit_EndFrameDoesNotExecuteAndReusesCpuState` drew 20 frames with zero executes and zero flushes; edge, param, and glyph array lengths stayed at the first frame's size and the snapshot list stayed empty. `Flush` and `Replay` threw. `SkipSubmit_RenderedFramesKeepTheSameRngStream`: 24 `StepWebFrame` calls on one seed, `JsRandom.Global.CallCount` matched the submitting run (execute count 24 versus 0).
- `AppFlowTests` (EditMode, GPU, no `-nographics`): 10 passed, 0 failed, 0 skipped, duration 3.103 s, exit 0. The editor did not abort. `randomCalls` assertions were left as they were. `boot_home` 0.037 s, `claim_gift` 0.010 s, `daily_chain` 1.738 s, `defeat_backup` 0.703 s, `level_win` 0.441 s, `maps_start` 0.084 s, `settings_reset` 0.010 s, `shop_buy` 0.010 s, `shop_equip` 0.010 s, `ReplayPlay_StepsEachRecordedFrame` 0.046 s.
- Full EditMode (no filter): 372 passed, 83 failed, 3 skipped, 458 total, duration 72.918 s, exit 2. The editor did not abort. Failures: `ReferenceComparisonTests` 73, `ReferenceGateTests` 9, `QuotaAndModalTests` 1. The three new canvas tests are why this is 458 and 372 rather than the prior 455 and 369. Skips: `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.
- `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes` still fails on the game still. Metal `game_lvl01_t1000ms` was `CC0F525EF93107760CA4026A2F5C975986CC49D969B02F6432F87FCECFEC28A3`; the Windows pin is `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`. That hash was not edited. The pause assert in the same test is not reached. The other `QuotaAndModalTests` passed. The 82 audio failures stay expected because the reference WAVs are absent.

#### Unity quality governor (2026-10-10, `agent/fix/quality-governor`)

- The settled raster scale is stored under `os.parity.scale.v2.{width}x{height}`. Values written by the old `os.parity.scale.` key are left in place and are not read. A scale is saved only after 10 s of stable warmed frames, and a phone does not save a scale below 2.
- Boot, a page change, and a raster resize drop frame samples for 1.5 s. Stepping down requires p95 above 22 ms for 1.5 s, so one hitch does not walk the scale 3→2→1.5→1. Phones use steps {2, 3}. Desktop stays on {1, 1.5, 2, 3}. `PlayQuality.Forced` and `-parity-quality` can still select 1 or 1.5 on a phone. Backdrop blur is not disabled on a phone just because the scale is low.
- Step-up treats a frame as comfortably at the cap when p95 is at or under `1/targetFrameRate * 1.1` (18.3 ms at 60 fps) and steps up at most once per 10 s. After a climb falls back, that scale and anything above it are not tried again until the next launch. The saved scale is the lower one, and only after it has been stable for 10 s.
- F3 still toggles the quality HUD. A four-finger tap does the same in the Editor, in a Development build, or when PlayerPrefs `os.parity.hud` is 1. The label font is `clamp(round(14 * dpi/160), 14, 48)` and the text is fps, p95, scale, cap, fit, resolution, and the saved v2 scale.
- `GovernQuality` calls `NoteTransition` when `currentId` changes, so `Show` does not need an edit. Call `ParityViewer.NoteTransition()` for a transition that keeps the same id.
- EditMode (GPU, no `-nographics`), filter `!OrbitalSalvage.App.Tests.AppFlowTests`: 373 passed, 83 failed, 3 skipped, 459 total. Unmodified baseline: 359 passed, 83 failed, 3 skipped, 445 total. `QualityGovernorTests` is 14 and passed, including `Fallback_DoesNotReclimbForTheRestOfTheSession` and `FreshGovernor_MayClimbAgain`. The 83 failures are `ReferenceComparisonTests` (73), `ReferenceGateTests` (9), and `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`. The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` because `tools/parity/out/unity-before` is not in this checkout.
- `GameAndPauseStills_MatchKnownHashes` still fails the Windows pin. On this Mac, `game_lvl01_t1000ms` hashed to `CC0F525EF93107760CA4026A2F5C975986CC49D969B02F6432F87FCECFEC28A3`, the same Metal value as before this change. The 23-still comparison did not run. Nothing was measured on a physical iPhone.

#### Unity iPhone safe area (2026-10-09, `agent/fix/iphone-safe-area`)

- iOS Player Settings `targetDevice` is iPhone only. `IosExport` and `IosXcodePostProcess` do not set a device family. The page-switch hint starts visible in the Editor and in Development builds, and hidden in a release player. F1 still toggles it.
- Every page that has a `__zero` spec now has a `__iphone` spec (44). The profile is the parity tool's fixed insets, 59 CSS px top and 34 CSS px bottom. A device with a different inset is off by a few px. Regenerating `__zero` matched the committed specs for 43 of 44 pages (some files differ only in the key order of root `t0`). `home__zero` also gains animation fields on 18 elements, so the committed zero specs were left in place. `home__iphone` was replaced with that distill: element rects are unchanged and the animation baseline is filled in.
- Play mode loads `__iphone` when `Screen.safeArea` has a top or bottom inset, and `__zero` otherwise. A missing iphone spec falls back to zero. Page Preview's iPhone toggle is offered for every page and still defaults to zero. The page stays contain-fitted in the full screen. The web game canvas fills the frame and does not follow the insets, so game pages keep the zero playfield recordings.
- EditMode (GPU, no `-nographics`): 370 passed, 82 failed, 3 skipped, 455 total. Before: 363 passed, 82 failed, 3 skipped, 448 total. The seven new tests passed. The 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9). The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` because `tools/parity/out/unity-before` is not in this checkout.
- The 23 zero stills were repainted with `PagePreviewPainter` (outline cache cleared, same pose as that test). `game_lvl01_t1000ms` is `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`, `modal_pause` is `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`, and `maps_world_1_fresh` is `E8DB19615D6FC8D0299B4990E79BDD36F9600E15E4DB3D5034002DB812D94590`. No device or simulator run.

#### Unity quota bar and modal backdrop (2026-10-09, `agent/fix/quota-bar-modal-backdrop`)

- The level HUD quota fill paints when mass is collected. The level-1 capture stores `hud-quota-bar` at width 0 (`v:0`), and HomeChrome skipped that element before the live width override. A width of 0 hides the fill again, including after a restart that leaves the quota label unchanged. The web green-to-yellow gradient used once the goal is met is not ported; the captured yellow-to-pink gradient stays.
- Every game-page sheet (`modal_*`, including the surplus debrief and the other debrief and defeat variants) binds the live playfield under the overlay, including the first frame when the session has not rendered yet. The sheet intro uses the exact backdrop blur. `settings_modal_*` is unchanged. Game and pause stills stay `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. All 23 `screenshot-states.json` stills matched a GPU paint taken on this branch before the fix.
- EditMode: 354 passed, 82 failed, 2 skipped, 438 total. The 6 new tests passed. The 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9), the same missing-audio-reference set as before. The skips are `FrameRateHarness.Measure` and `MeasureAccumulated`. `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` ran and passed; it had been skipping because `tools/parity/out/unity-before` was absent.

### Changed

#### Settings English baselines (2026-10-12)

- The English settings specs listed under Added are the intended pixel baseline for those pages. Home, game, shop, maps, daily, achievements, loading, pause, debrief, and defeat specs were not rewritten. `settings_default` in `tools/parity/unity/screenshot-states.json` still names the old capture steps. The matching still PNG lives under `tools/parity/out/unity-before`, which this worktree does not have, so that PNG was not replaced.

#### Unity on-device perf log (2026-10-10, `agent/perf/device-log`)

- Playback can emit `[Perf]` lines through `Debug.Log` (forwarded by `idevicedebug run` on a Release IL2CPP player). The log is off in the editor and in tests until PlayerPrefs int `os.parity.log` is 1. The HUD pref `os.parity.hud` is unchanged. The governor still decides the raster scale; this path only reads it.
- Once a second, one line carries the product page (`home` / `maps` / `game`, or `modal_*` while a sheet is up), raster scale, cap, fit, frames in that second, fps, frame ms avg/p50/p95/p99/max from `Time.unscaledDeltaTime`, counts of frames over 22 ms and over 50 ms, `GC.CollectionCount(0)` delta, `GC.GetTotalMemory(false)` delta, `Profiler` allocated/mono/reserved, resident chrome bytes, the byte budget, and `SystemInfo.systemMemorySize`. iOS thermal state is appended only in an iOS player build. A frame over 80 ms logs `[Perf] hitch` on the next frame with the finished frame's phase times: `update`, `prewarm` (PumpTabPrewarm), `draw` (HomeChrome), `sim` (physics step), `submit` (GameRenderer / Canvas2D present), `gui` (OnGUI). A page change logs `[Perf] page a -> b ms=… bake_ms=…` where `ms` is the time until the first draw and `bake_ms` is `HomeChrome.Load` plus layer-cache baking during that switch. A governor step logs `[Perf] governor step=down|up …`.
- With the pref off, a frame does not allocate for this and does not start those stopwatches. `LiveGameSession.PhaseTiming` and `HomeChrome.ProbeBake` stay false, so physics and paint call order are unchanged. `JsRandom.Global` is not touched. With the pref on, the steady cost is a few timestamp reads and a reused `StringBuilder`; the log line is allocated when it is emitted.
- `DevicePerfTests` (EditMode): 9 passed, 0 failed, duration 0.020 s. Full EditMode (no filter, GPU, no `-nographics`): 409 passed, 83 failed, 3 skipped, 495 total, duration 76.936 s, exit 2. Before this change the same suite on this Mac was 400 passed, 83 failed, 3 skipped, 486 total. The 83 failures are still `ReferenceComparisonTests` (73), `ReferenceGateTests` (9), and `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes`. The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.
- `GameAndPauseStills_MatchKnownHashes` still fails the Windows pin. On this Mac, `game_lvl01_t1000ms` hashed to `CC0F525EF93107760CA4026A2F5C975986CC49D969B02F6432F87FCECFEC28A3`, the same Metal value as before this change. The pause assert in that test is not reached. The 23 zero-profile stills were not repainted as a set. Nothing was measured on a physical iPhone, and the Profiler memory counters were not checked in a Release IL2CPP player (they are logged either way; a non-development player may report 0).

#### Unity tab page pre-bake (2026-10-10, `agent/perf/prebake-tabs`)

- The five main-tab templates are the pre-bake list, home first. On a fresh profile `AppController.PeekPageTemplate` (the same `ChooseTemplate` path, with any open modal ignored) resolves `home_default`, `maps_world_1_fresh`, `daily_default`, `shop_rings_default`, and `achievements_none`. `ParityPages.PrewarmTemplates` is the only list. The profile is `iphone` when `Screen.safeArea` has a top or bottom inset, and `zero` otherwise.
- Resident baked layers plus canvas are capped by `ParityPages.ResidentBudgetBytes(systemMemoryMb, phone)`. `systemMemoryMb` is `SystemInfo.systemMemorySize`. `phone` is `QualityGovernor.IsPhone`. Phones take the lesser of 35% of RAM (`PhoneBudgetPercent`) and a hard cap: `Phone4GbBudgetBytes` = 320 MiB when memory is under 6144 MB, otherwise `Phone6GbBudgetBytes` = 480 MiB. Desktop and the editor use `DesktopBudgetBytes` = 1536 MiB (1.5 GiB), including a 4 GB desktop. A 512 MB phone therefore gets 179 MiB (the 35% share), not the 320 MiB cap. The page on screen is advisory: it stays even when it alone is over the cap. `ParityViewer.ResidentCacheBytes` is the live total for a later HUD; the HUD itself was not edited.
- The count cap is still 8 (five tabs plus `ModalCacheMargin` 3). Without a byte cost, pins are hard, so extra modals evict the oldest unpinned chrome and cannot push a tab out. With the byte budget, pins are soft: eviction takes the oldest unpinned page first (modals), then the oldest pinned tab, and never the current page. `Dispose` still releases that chrome's RenderTextures.
- Pre-bake spends the budget in order and stops when the next page would exceed it. Pages that do not fit bake on first visit. Baking waits until the loading bar has been written, so the override set matches later draws, and it waits while `HomeChrome.DeferLayerCache` is set. It does not add time to the loading clock (240 ms ticks until the bar reaches 100, then 350 ms, then home). One template per frame. Self-tests leave this off. `PlayQuality.Forced` does not: those pages bake at the forced step. Page Preview stills do not boot the viewer.
- `ResizeRaster` drops that chrome's layer cache. The current page is resized to the live scale before it is painted, so a stale-scale layer is not drawn. After the governor's defer, one background page per frame may be re-baked. A scale-up is skipped when the other resident pages plus the projected bytes would pass the budget. A scale-down is baked, because it frees memory. `DesiredRasterScale` reads `QualityGovernor.SnapStep` and `governor.Cap` (the governor no longer lives on `ParityViewer`).
- Per-page bytes were measured in EditMode on this Mac (Metal), one template at a time, and the budget uses those totals. Layer bytes are the allocated layer RenderTextures (ARGB32, 4 bytes per pixel). Canvas bytes are the four permanent Canvas2D buffers (ARGB32 + RFloat + two RHalf, 12 bytes per pixel). Blur-scratch and clip textures are not included. At 3× (1290×2796): `home_default` 21 layers / 302,974,560 layer bytes / 43,282,080 canvas bytes / 346,256,640 total; `maps_world_1_fresh` 20 / 288,547,200 / 43,282,080 / 331,829,280; `daily_default`, `shop_rings_default`, and `achievements_none` 19 / 274,119,840 / 43,282,080 / 317,401,920 each. All five at 3× would be 1,630,291,680 bytes (1554.8 MiB). At 2× (860×1864): home 21 / 134,655,360 / 19,236,480 / 153,891,840; maps 20 / 128,243,200 / 19,236,480 / 147,479,680; daily, shop, and achievements 19 / 121,831,040 / 19,236,480 / 141,067,520 each. All five at 2× would be 724,574,080 bytes (691.0 MiB). Scale² holds: the 3× totals are 2.25× the 2× totals.
- A 4 GB phone (budget 335,544,320 bytes) therefore pre-bakes nothing at 3×, because home alone is 346,256,640. The current page is still shown. At 2× it pre-bakes home and maps, 301,371,520 bytes, and stops before daily (the next page would make 442,439,040). A 6 GB phone (503,316,480) keeps three tabs at 2× and home only at 3×. Desktop keeps all five at 2× and four at 3×.
- Layers were not merged. Skipped slots (never allocated) at both scales: home 2, maps 3, daily 4, shop 4, trophies 4, out of 23 slots. At 2× a full RGBA32 readback of the allocated layers found many that are all zeros: home 14 of 21, maps 17 of 20, daily 16 of 19, shop 14 of 19, trophies 16 of 19. Those clear layers share one hash per page. Maps, daily, shop, and trophies each also have one non-clear pair with the same hash (layers that match another layer: 19, 18, 16, and 18). Home's only duplicates are the clear layers (14). A clear layer is still blitted, and dropping or sharing it was not proven against a composite, so the bake still allocates every live layer.
- The 23 `screenshot-states.json` zero stills painted with `PagePreviewPainter` (outline cache cleared, same pose as `PagePreviewTests`) were SHA256-identical to the Mac capture taken before the pre-bake, including after this budget change, and including `game_lvl01_t1000ms` `CC0F525EF93107760CA4026A2F5C975986CC49D969B02F6432F87FCECFEC28A3`. Nothing was measured on a physical iPhone.
- EditMode (GPU, no `-nographics`, no test filter): 400 passed, 83 failed, 3 skipped, 486 total. Before the byte budget, the same unfiltered suite is 396 passed, 83 failed, 3 skipped, 482 total (the earlier filtered 455, plus 10 `AppFlowTests`, plus 3 from the Mac replay fix and 14 from the quality governor). The 4 new budget tests in `TabPrewarmTests` passed, and the previous 10 pre-bake tests still pass. The 83 failures are `ReferenceComparisonTests` (73), `ReferenceGateTests` (9), and `QuotaAndModalTests.GameAndPauseStills_MatchKnownHashes` (the Windows-pinned game hash; on this Mac the paint stayed `CC0F525EF93107760CA4026A2F5C975986CC49D969B02F6432F87FCECFEC28A3`). The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`.

#### Unity play page fit (2026-10-09, `agent/fix/fit-full-screen`)

- Play mode contain-fits the 430×932 page into the full screen and keeps that aspect. `Screen.safeArea` no longer shrinks the rect. Bands left by an aspect mismatch are rgb(30, 27, 75), the `#mobile-frame` background. Pointer mapping uses the same rect. Notch and home-indicator padding inside the page is not added.
- EditMode, GPU: 351 passed, 82 failed, 3 skipped, 436 total. The 4 new `PageFitTests` passed. The 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9). The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault`. The 23 stills were not recaptured. Page Preview and HomeChrome were not changed.

#### Unity start level (2026-10-09, `agent/fix/start-level-one`)

- A fresh profile still boots at level 1 (20 kg). An empty profile starting at level 11 was not reproduced. `AdoptSector` now records `SectorIndex`, so Next advances from the adopted level. Play loads the first sector that is not in `completedSectors` (level 1 when the list is empty, level 11 after levels 1–10). Tools → Reset Saved Profile resets a running app to that level 1 state and still deletes the profile file.
- The maps capture `maps_world_1_all_complete_other_active` is the world 2 tab with level 11 marked current, not the world 1 tab. It is selected only for that live view. Other maps pages keep their captured spec and stamp the PLAY tile and world tab only when live state differs, so `maps_world_1_fresh` is left untouched.
- EditMode (GPU, no `-nographics`): 353 passed, 82 failed, 3 skipped, 438 total. Before this change: 347 passed, 82 failed, 3 skipped, 432 total. The six new tests are `StartLevelTests` (5) and `ProfileResetTests.Confirm_ResetsARunningAppAndDeletesTheTempProfile`. The 82 failures are still `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9). The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` because `tools/parity/out/unity-before` is not in this checkout.
- The 23 `screenshot-states.json` stills were repainted with `PagePreviewPainter` (same path as that test, outline cache cleared) and match the 2026-10-08 profile-zero SHA256 list, including `maps_world_1_fresh` `E8DB19615D6FC8D0299B4990E79BDD36F9600E15E4DB3D5034002DB812D94590`, game `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`, and pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. The other `maps_world_*` specs are not in those 23 stills.

#### Unity iOS GameAssembly id (2026-10-09, `agent/feat/ios-gameassembly-id`)

- After every iOS export, `IosXcodePostProcess` sets the Xcode target `GameAssembly` bundle identifier to `com.unity3d.gameassembly`. The app identifier stays `io.dev.maze`. `UnityFramework` stays `com.unity3d.framework`. Player Settings are unchanged. The step is untested against a real Unity-generated Xcode project: this machine has no iOS Build Support module. On a Mac, run an export and check `[IosExport] GameAssembly bundle id set to com.unity3d.gameassembly`. If the console warns `target GameAssembly not found`, send the listed target names.
- EditMode: 347 passed, 82 failed, 3 skipped, 432 total. The 20 new `IosGameAssemblyBundleIdTests` passed. The 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9). The skips are `FrameRateHarness.Measure`, `MeasureAccumulated`, and `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` because `tools/parity/out/unity-before` is not in this checkout. The 23 stills were not recaptured. `IosExport.ExportSimulator` exited 1 with the missing-module message and wrote no Xcode project.

#### Unity Editor follow-up (2026-10-08, `agent/feat/menu-ios`)

- Page Preview clears the outline-glyph cache before a page. After other editor paints, `shop_rings_default` had differed from the parity still by 9 pixels (max channel delta 2, bbox (956,384)-(986,401)) on the "Claim +100" edge. With a fresh cache, all 23 `screenshot-states.json` states painted by `PagePreviewPainter` matched those stills byte for byte. HomeChrome was not changed.
- The iOS menu test follows `IosExport.ModuleInstalled` instead of asserting that the module is absent. EditMode: 328 passed, 82 failed, 2 skipped, 412 total. The 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9). A recapture of the 23 stills had 0 SHA256 mismatches, including game `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`.

#### Unity Editor (2026-10-08, `agent/feat/menu-ios`)

- The custom Editor menu is `Orbital Salvage/Preview`, `Build`, `Play`, and `Tools`. Page Preview paints stored specs with HomeChrome outside Play mode. The Windows play player is `games/maze-puzzle/output/maze-puzzle-windows/OrbitalSalvage.exe`. iOS simulator and device export menu items exist and exit 1 until the iOS Build Support module is installed; no Xcode project was produced. The iOS application identifier is `io.dev.maze` and the minimum iOS version is 15.0. Apple team ID and signing are still empty. The menu map is in `unity/maze-puzzle/README.md`.
- EditMode: 327 passed, 82 failed, 2 skipped, 411 total. The 82 failures are `ReferenceComparisonTests` and `ReferenceGateTests`. The skips are `FrameRateHarness.Measure` and `MeasureAccumulated`. The 10 new tests passed. Before and after captures of the 23 `screenshot-states.json` stills are SHA256-identical, including game `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. `PlayBuild.BuildPlayer` exited 0 and wrote the exe above; `ProjectSettings.asset` hash after that run matched the hash taken before it.

#### Tooling (2026-10-08, `agent/chore/tools-platform`)

- Windows PowerShell launchers moved to `games/maze-puzzle/tools/parity/windows/unity/` (`build.ps1`, `capture.ps1`, `perf.ps1`, `run-framerate.ps1`, `build-play.ps1`) and `games/maze-puzzle/tools/parity/windows/canvas-conformance/run-unity.ps1`. They stay tracked. `macos/` is a placeholder only. Screenshot baselines stay the Windows D3D12 hashes until a Mac recapture. Classification: `tools/parity/README.md` (Platforms) and `tools/parity/CLEANUP.md`.
- After that move, `windows/unity/build.ps1` (Unity exit 0, 10.1 s) and `windows/unity/capture.ps1` (player exit 0, 17.5 s, profile zero, `screenshot-states.json`) wrote 23 PNGs SHA256-identical to the capture taken before the move, including game `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. `LiveConformanceBatch.Run` wrote 30 live PNGs and 30 stream PNGs, 0 mismatches, `deterministic true`. EditMode 318/402; the same 82 audio reference tests fail; 2 skipped.

#### Tooling (2026-10-08, `agent/chore/tools-move`)

- The parity toolset moved from repo-root `tools/parity` to `games/maze-puzzle/tools/parity` (`git mv`, 346 paths). Repo-root `tools/` is gone. Scripts and Unity tests find it by walking up to the directory that contains both `tools/parity` and `web`. Inventory and the files removed in that move are in `games/maze-puzzle/tools/parity/CLEANUP.md`.

#### Web + Unity (2026-10-08, `agent/fix/funnel`)

- The funnel centering term is now `(dx * 16 - vx * 8) * dt` in `web/engine/physics.js` and `PhysicsEngine.ResolveMagneticFunnel`. c = 8 is critical damping for k = 16. Rails, aperture, gravity, and the black hole are unchanged. Idle debris on the measured levels no longer misses the hole along the rail; a swipe or a held spin can still spill. Report: `tools/parity/reports/funnel-fix-2026-10-08.md`.

#### Unity (2026-10-08, reminder toast)

- Pressing the claimed daily-gift button now shows one "🎁 Claimed today. Come back tomorrow!" toast (never stacked while it is showing). No credits, sound or haptic. This is a deliberate difference from the web, whose claimed button is `disabled` and silent. `ShopGiftTests` updated; EditMode 318/402, the same 82 audio reference tests fail.

#### Unity (2026-10-08, `agent/fix/shop-gift`)

- After the daily gift is claimed, every Toy Shop tab paints the claimed banner taken from `shop_gift_claimed`: muted banner, green stipend tag, grey "Claimed Today" button. The gift still pays +100 once per UTC day. A second press on that button does not add a toast or a sound. Profile-zero stills stayed SHA256-identical to the capture taken before this change, including game `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. EditMode 316/400; the same 82 audio reference tests fail. Report: `tools/parity/reports/shop-gift-2026-10-08.md`.

#### Unity (2026-10-07, `agent/fix/physics`)

- Live play (`ParityViewer.TickLive`) accumulates real frame time and integrates physics at 1/60 s, at most 3 steps per frame (50 ms). Time left over that cap is dropped. Pause and leaving the game page clear the accumulator so the next frame does not catch up. Recorded 16 ms conformance (`FixedStep`, `StepWebFrame`) is unchanged. The 1/60 step is a steady browser frame; the 16 ms figure is the recorded-stream cadence.

#### Unity (2026-10-07, `agent/fix/ui`)

- Play Quality menu path is `Orbital Salvage/Play Quality/`. The spaceless `OrbitalSalvage/` root is gone, so the Editor no longer shows a second top-level menu.
- Product-path toasts refresh age every frame and get one redraw after the last toast expires. Flow capture and the parity self-test keep the previous clock. EditMode `ToastAgeTests` on `settings_default`.
- Pause name and goal that do not fit the 256 px centered box wrap and grow the card upward. One-line sample text is unchanged. Gift description wraps inside its two-line box. A quota string wider than the captured run stays right-aligned in the capsule. `PauseReflowTests`.
- Backdrop blur at 1× stays skipped. Editor Play probe, D3D12, RTX 4060 Laptop, `-parity-quality 1`: added `drawMs` average was under 1 ms in eight home/pause pairs at 430×932 and 1284×2778, but frame medians were 8.0–11.9 ms (vSync 0, window unfocused, not the 15.5–16.5 ms band) and two frame-p95 pairs worsened by more than 2 ms. Raster stayed 430×932 at both Game view sizes. D3D11 was not measured. Report: `tools/parity/reports/ui-fixes-2026-10-07.md`.
- Gesture hint is shown on the first entry to the game page and stays hidden after that. A new app still shows it. Leaving via Return to Home zeros `FailureCheckTimer` and keeps collected mass, spilled mass, and debris. `GameResumeTests`.
- Profile-zero capture after the player rebuild: 23 stills SHA256-identical to the capture taken before these edits, including game `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. Web PNGs were not in the tree, so `modal_pause` was not re-scored. Flow shots were not re-captured. EditMode 307/389; the same 82 audio reference tests fail.

#### Unity (2026-10-04, `agent/parity/ship`)

- Play, with no command-line flags, boots `ParityViewer` and `AppController` (Canvas2D). The main scene is a camera. Pointer clicks, drags, and holds go through the letterboxed 430×932 page. Profile is `orbital-salvage-profile.json` under `Application.persistentDataPath` (key `ORBITAL_SALVAGE_CARTOON_PROFILE_V1`). Audio on that path is the C# DSP player. `--parity` and the parity self-tests keep a frozen clock and silent audio.
- Retired the uGUI shell: `UIManager`, `SpriteHelper`, `SafeAreaFitter`, the old `OrbitalSalvage.Core` managers, Physics2D gameplay scripts, TMP Examples & Extras. Fluent emoji and Fredoka/Nunito assets stay. `Assets/Audio/*.wav` stays; the DSP path does not play those files.
- Harness re-measure (same machine, not a device): 11 stills unchanged from the listed baseline (5 pass the 99.0%/0.985 bar, 6 fail, mostly text edges). Game and pause SHA256 unchanged. Live streams 30/30 byte-identical. Controller goldens 9/9. Flow shots 55, 6 at the bar, 0 blank. HomeChrome.Draw one D3D11 run: avg 3.049 ms, p95 4.409 ms. Live frame p95 14.32 ms. EditMode 380/380. This is not a completed UI restoration.

#### Unity (2026-10-04, `agent/parity/lag`)

- Editor Play at Simulator size (1284×2778) was ~16 fps and could paint a mustard page with a blue hero. Cause: live frames unbaked a reused local rect every tick; the 3× raster plus vSync filled the rest of the frame. Product path now unbakes once, turns vSync off, targets 60 fps, and drops raster 3→2→1.5→1 when p95 exceeds 22 ms. After, D3D12 p95 24.69 ms with a purple page and yellow face. Parity stills stay frozen 3× Draw (game/pause hashes unchanged). Phones untested.

#### Unity (2026-10-05, `agent/parity/homefix-clean`)

- Home lag in Editor Play (D3D12, Game view 1284×2778). The page cached 37 full-raster layers (534 MB at 3×) through `ReadPixels` and rebuilt all of them on every quality step and every text change. A Nibbles tap froze 2.9–3.6 s at 3× and 0.4–0.5 s at 1×; the 3→1 ramp froze 0.45–1.9 s three times.
- Text and width values no longer drop the cache; only the set of overridden ids does. Layers copy GPU-to-GPU into render textures, and hidden-only runs are not baked (29 layers). The governor waits 0.9 s after a resize before baking and remembers the settled scale per Game-view size. Backdrop blur copies and blurs only the element rect plus its reach. Per-frame class splitting, shadow/colour string round-trips, regex and override signatures are cached.
- After: Nibbles tap max 33–158 ms with no rebuild; ramp rebuilds 2.7–4.6 ms; 3× steady median 20–23 ms (was 41–44); 1× median 16.0–16.3 ms; home GC ~107–112 KB/frame at 1× (was ~234). Frames over 100 ms still appear in some forced-3× runs, with and without the fix; paint is 7–30 ms of each. All 16 zero-profile stills, game and pause hashes, and animated home dumps at 0/500/1500/4000 ms (3× and 1×) are SHA256-identical to before. EditMode 300/382: the 82 failures are audio reference tests whose Chromium reference files are not present in that checkout.

The notes below are the 2026-09-30 uGUI changelog. Those types are gone.

### Fixed

#### Unity

- Victory / defeat modals now open from gameplay via `GameManager.OnStateChanged` with real stats.
- Defeat path: spilled debris counted as lost; defeat when no active debris and quota unmet; 1.5s sector-start grace.
- Backup Pod button when ≥70% quota and unused (`Launch Backup Pod (+8)`).
- Emoji sprites moved to `Assets/Resources/Emojis/3D/` and loaded via `Resources.Load` (fixes device builds that previously fell back to circles).
- Safe area applied via `SafeContent` + `SafeAreaFitter`.
- HUD quota bar fill updates correctly.
- Star ratings persisted with `List<SectorStarEntry>` instead of `Dictionary` (JsonUtility-safe).
- Header coin labels update live and only when changed.
- Shop unlock labels corrected (e.g. `Unlock (500)` / `Unlock (1200)`).

#### Web

- Missing `AudioEngine.playRcsThrust` — BOOST gates aborted the physics frame when the method was absent.
- `AudioContext` resumed on interaction (iOS mute / suspended context).
- SFX volume slider now applied (`setSfxVolume`); ambience bus has gain nodes but no ambience sources yet.
- Escape / Hangar / `loadSector` share `resetGameplaySessionUi` so modals and pause state reset cleanly.

### Changed

#### Web

- Campaign reward = `250 + (level - 1) * 5` (L1 = 250, L50 = 495).

### Added

#### Tooling

- MCP for Unity **10.2.0** (Unity package `com.coplaydev.unity-mcp` `#v10.2.0` + Cursor `.cursor/mcp.json` entry; `.agents` configs pinned to `mcpforunityserver==10.2.0`).

#### Docs

- `08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md` and `09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md` corrected against repo reality.
- `10_PROJECT_REVIEW_2026-09-30.md` added.

---

## [0.10.0] - 2026-09-30 (`c7ba2f5`, `c9487e5`, `3c0a048`)

### Added

- **Complete game scene, prefabs & build settings (`c7ba2f5`):** `MainGameScene`, `Debris_Titanium` / `Gate_Multiplier` prefabs, Resources audio + `sectors_50.json` mirrors, and Editor build settings wiring.
- **Edit-mode UI preview tooling & visual spec parity (`c9487e5`):** `OrbitalSalvageEditorMenu` UI Previews, `ConfigureEmojiSprites`, expanded procedural `UIManager`, `SpriteHelper`, and initial web screenshot archive under `web/screenshots/`. Imported TextMesh Pro (including Examples & Extras).
- **UI fidelity, SDF sprites & runtime screen capture (`3c0a048`):** Major `UIManager` / `SpriteHelper` visual iteration, SDF-oriented UI chrome, `RuntimeScreenshotCapture` editor tooling, and Unity screenshot archive under `unity/screenshots/`.

---

## [0.9.0] - 2026-09-28 (`54ec4d8`)

### Added

- **Native Unity 6 Project Scaffolding (`games/maze-puzzle/unity/maze-puzzle/`):** Initialized the native Unity 6 (`6000.4.6f1`) project `maze-puzzle` via Unity Batchmode CLI, configured for iOS Portrait orientation.
- **Git Worktree Collaborative Development (`.worktrees/unity-migration/`):** Utilized isolated git worktree branch `unity-migration` for parallel multi-agent development and seamless merging into `maze-puzzle`.
- **Pre-baked Asset Pipelines:**
  - Archived all 34 pre-rendered Microsoft Fluent 3D Emoji PNG assets directly into `Assets/Sprites/Emojis/3D/` (later relocated to `Assets/Resources/Emojis/3D/` in Unreleased).
  - Synthesized all 15 discrete uncompressed 16-bit PCM `.wav` audio cues into `Assets/Audio/` matching the studio Audio Design Specification.
- **50-Level Campaign Data & Level Sanitizer:**
  - Exported complete 50-sector campaign matrix from `web/engine/maze.js` to `Assets/Scripts/Data/sectors_50.json`.
  - Implemented C# `SectorData.cs` and `MazeSanitizer.cs` guaranteeing zero gate-wall overlaps.
  - Verified 50/50 level import via `-executeMethod OrbitalSalvage.Editor.LevelImporter.BatchImportAll50Levels` with exit code 0.
- **Complete Core C# Gameplay Architecture:**
  - `CentrifugeController.cs`: tactile drag, Euler integration, 0.94 inertia damping, and double-tap attitude thrusters.
  - `DebrisPiece.cs`: Rigid body dynamics, material restitution, and scrap valuation.
  - `MultiplierGate.cs`: Multiplier, adder, booster, and flapper gates with radial clone fanning.
  - `NibblesController.cs`: Mascot micro-black hole with gravitational pull, event horizon ingestion, dynamic quota growth ($1.0\times \to 1.65\times$), and squash-and-stretch wobble.
  - `MascotEyeTracker.cs`: Real-time pupil gaze tracking towards falling scrap with natural blinking routines.
  - `GameManager.cs` & `EconomyManager.cs`: Game state machine, offline JSON persistence, scrap credits, and backup pod recovery flow.
  - `AudioManager.cs`: Master audio manager pooling AudioSources for the 15 sound cues.
  - `SafeAreaFitter.cs` & `UIManager.cs`: Screen safe area adaptation for iPhone 16 Pro Max ($430\text{pt} \times 932\text{pt}$).

---

## [0.8.0] - 2026-09-27 (`0428369`)

### Added

- **Unity Migration & iOS Deployment Plan (`08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md`):** Complete architectural transition and deployment specification for migrating from the HTML5 Canvas web prototype to Unity (URP 2D/C#) targeting Apple iPhone iOS. Includes:
  - Technical feasibility analysis of Unity's native Metal rendering, 120 FPS ProMotion, and CoreHaptics.
  - Module-by-module JavaScript-to-C# code translation assessment across physics, level schemas, UI, audio, and economy.
  - **1:1 Sound & Image Asset Inventory (Section 4):** Comprehensive catalogs covering all 15 discrete `.wav` audio cues (exact waveforms, DSP envelopes, and master compressor limiter specs) and all visual/image assets (4 ring skins, 4 black hole skins, 4 multiplier gates, 3 debris variants, 3 celestial skyboxes, deflector bumpers, and 8 mobile UI screens).
  - **Microsoft Fluent 3D Emoji Asset Integration (Section 4.3):** Integrated the MIT-licensed open-source `microsoft/fluentui-emoji` library. Resolved cross-platform emoji rendering divergence between Windows and iOS, bypassed Apple IP emoji restrictions, and provided turnkey fetcher `scripts/download_fluent_3d_emojis.js`.
  - **Asset Verification:** Successfully downloaded all 34 pre-rendered 3D emoji PNG assets into `unity/maze-puzzle/Assets/Sprites/Emojis/3D/` for TextMeshPro sprite asset generation (`TMP_SpriteAsset_Emojis.asset`).
  - **Complete Animation Migration Architecture (Section 5):** Detailed 1:1 translation specification converting Canvas 2D trigonometric math and particle loops into Unity animation systems (DOTween squash-and-stretch wobble, pupil gaze tracking, dynamic quota growth, Shuriken Keplerian accretion vortex, UV-scrolling booster chevrons, and HingeJoint2D one-way flappers).
  - **Unity Tooling & Automation Stack (Section 6):** Confirmed local Unity 6 (`6000.4.6f1`) and Unity Hub CLI paths, documented Unity Model Context Protocol (MCP) bridge integration (`CoplayDev/unity-mcp`), Batchmode CLI headless workflows, OpenUPM package automation, and a 4-phase hybrid velocity strategy.
  - **Unity MCP Server Setup & Verification:** Configured workspace-level MCP server (`.agents/mcp_config.json`, `.agents/plugins/unity-mcp/`), implemented in-workspace skill runbook (`.agents/skills/unity-mcp/SKILL.md`), and successfully executed FastMCP stdio connection tests and resource discovery.
  - 7-phase Gantt execution roadmap spanning project scaffolding, ScriptableObject level imports, physics kinematics, UGUI systems, and Xcode TestFlight deployment.
  - Risk analysis and App Store Review Guideline 4.3 defense alignment.

---

## [0.7.0] - 2026-09-24 (`85aefc7`)

### Added

- **Audio Design Specification (`07_AUDIO_DESIGN_SPECIFICATION.md`):** Complete studio-grade Audio Design Document detailing the "Tactile Toybox Skeuomorphism" creative direction, zero-asset Web Audio DSP synthesis chains, master dynamics compressor limiter specs (-14dBFS threshold, 8:1 ratio, 3ms attack), complete 12-cue acoustic recipe catalog, voice priority hierarchy, and millisecond-accurate haptic-audio sensory synchronization.
- **Studio Documentation System Architecture:** Established a 4-tier game studio documentation hierarchy aligning with the AGB Casual Games Studio framework:
  1. Strategic & Creative Vision (`01_REFERENCE_GAME_ANALYSIS`, `02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3`, `03_SPACE_DEBRIS_CLEANUP_GDD`).
  2. Functional Domain Specifications (`04_TECHNICAL_ARCHITECTURE`, `05_GAME_UI_UX_DESIGN_SPECIFICATION`, `06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN`, `07_AUDIO_DESIGN_SPECIFICATION`).
  3. Implementation & Data Schemas (`web/main.js`, `web/engine/`).
  4. Operational Traceability & Lifecycle (`CHANGELOG.md`, `README.md`).
- **Comprehensive Cross-Discipline Documentation Index:** Updated master `README.md` index linking all technical, design, level, and audio specifications.

---

## [0.6.0] - 2026-09-24 (`bd96ef3`)

### Added

- **iOS 430pt × 932pt Standard Viewport:** Standardized root viewport to the native iPhone 16 Pro Max logical resolution ($430\text{pt} \times 932\text{pt}$), incorporating dynamic Safe Area Insets (`env(safe-area-inset-top)` / `env(safe-area-inset-bottom)`) and enforcing Apple HIG minimum 44pt touch targets across all HUD buttons.
- **Magnetic Containment Guide Funnel:** Implemented physical guide rails narrowing from the ring exit aperture ($95\text{px}$ half-width) down to Nibbles' mouth ($46\text{px}$ half-width), preventing scrap from flinging past the black hole.
- **Aperture Inertial Dampers:** Horizontal velocity is automatically dampened by $65\%$ (`d.vx *= 0.35`) and downward escape momentum is ensured (`d.vy = Math.max(d.vy, 95)`) upon exiting the centrifuge ring.
- **Dynamic Centering Tractor Beam:** Continuous inward acceleration ($F_x = (\text{bh.x} - d.x) \cdot 16$) pulling falling scrap toward the central gravity axis.
- **Animated Neon Funnel VFX:** Visual containment field with animated downward dashed guide rails, aperture mouth emitter pylons, and directional magnetic chevrons flowing toward Nibbles.
- **Out-of-Scrap Defeat Flow:** Added soft-lock elimination with a $1.2\text{s}$ grace timer that detects when all scrap has left the ring and funnel without achieving the sector quota.
- **Cartoon Defeat Modal (`#modal-defeat`):** Featuring hungry Nibbles (`🥺🥣✨`), collected/target quota stats, lost scrap accounting, instant retry (`🔁 Try Again`), and hangar navigation (`🏠 Hangar`).
- **Tactical Auxiliary Ejector Pod (+8 Scrap):** Players reaching $\ge 70\%$ of the required quota unlock a one-time emergency scrap pod deployment, injecting 8 fresh debris pieces into the centrifuge with procedural rocket audio (`playBackupPodSound`) and haptic feedback.
- **Procedural Defeat Audio (`playDefeatSound`):** Whimsical descending cartoon triangle-wave jingle with gentle pitch bend.
- **Anti-Slide Retaining Shelves (World 1):** Introduced horizontal baffles (`angle: 0`, `angle: PI`, `rMin: 85, rMax: 125`) eliminating the passive rightward slide auto-pass issue in Level 5.
- **Deterministic 3-Pass Gate Separation (`sanitizeSector`):** Automated spatial optimizer guaranteeing $\ge 0.16\pi$ clearance between all multiplier gates, dividers, and bumpers, verified with 0 overlaps across all 50 levels.

### Fixed

- **Debris Re-Entry Bug:** Ring outer boundary collisions now strictly gate on `d.state === "IN_RING"`, preventing falling debris from being erroneously pulled back into the centrifuge.
- **Multiplier Explosion Scatters:** Clamped inter-debris collision impulse ($p \le 38$), capped velocity at $460\text{px/s}$, added $0.22\text{s}$ gate cooldowns, adjacent gate synchronization (`syncAdjacentGates`), and radial clone fanning.
- **Off-Screen Debris Accumulation:** Added clean boundary despawning (`d.y > bh.y + 140` or off-screen) with accurate spill tracking via `onDebrisSpilled`.

---

## [0.5.0] - 2026-09-24 (`4da1a58`)

### Added

- **Daily Janitor Care Package:** Complimentary $+100$ Gold Coin allowance claimable in the Toy Shop every 24 hours.
- **Dynamic Dock Badge:** Pulsing animated "FREE" badge on the Arcade Dock **SHOP** tab when an unclaimed stipend is available.
- **100% Offline Non-Monetized Architecture:** Guaranteed zero in-app purchases, zero advertisements, and zero user-tracking analytics for strict compliance with Apple Review Guidelines 4.3 & 3.1.1.

---

## [0.4.0] - 2026-09-24 (`174e610`)

### Added

- **Multi-Page App Router:** Implemented state-driven single-page navigation supporting Loading Screen, 3D Hero Diorama Home, Tabbed Campaign Maps, Toy Shop, Trophies, and Canvas Gameplay views.
- **Interactive 3D Hero Diorama:** Mascot diorama on homepage featuring tactile squash-and-stretch physics, heart/star particle bursts, and dynamic facial expressions.
- **In-Game Settings & Pause Overlays:** Integrated volume sliders for sound effects and ambient audio, haptic feedback toggle, privacy policy dossier, and reset progress confirmation.
- **Accretion Vortex Particle System:** Animated Keplerian orbit particles swirling into Nibbles with suction speedup when debris approaches the event horizon.

---

## [0.3.0] - 2026-09-23 (`c48b407`)

### Added

- **Concentric Dual-Ring Gearing:** Differential gear ratio rotation for advanced multi-track orbital centrifuges.
- **One-Way Gravity Flapper Valves:** Spring-loaded directional valves directing debris flow with acoustic click feedback.
- **Corridor Clearance Optimization:** Widened track corridors and optimized aperture angles across campaign levels.

---

## [0.2.0] - 2026-09-23 (`b7c11a9`)

### Added

- **50-Level Campaign Grid:** Complete 5-world progressive campaign (Worlds 1–5, Levels 1–50) concluding with _The Kessler Singularity_.
- **Differentiated Gate Archetypes:** Visual silhouettes and colors for Rotary Sawblade ($\times 2, \times 3$), Super Shredder ($\times 4$), Magnetic Compactor ($+N$), and Accelerator Booster ($160\text{px/s}$).
- **Pinball Deflector Bumpers:** Pachinko-style circular bumpers with high-energy elastic rebound ($e = 1.35$), shockwave rings, and boing sound effects.
- **Nibbles Dynamic Growth:** Visual scaling from $1.0\times$ to $1.65\times$ as mass is swallowed, without altering physical event horizon bounds.

---

## [0.1.0] - 2026-09-22 (`d7198f3`)

### Added

- **Initial Prototype:** Reverse-engineered _Multi Maze 3D_ and elevated mechanics into the _Space Trash Clean_ theme.
- **Pure Gesture Mobile Engine:** Full-screen drag rotation, double-tap RCS burst, and physics rigid-body debris simulation.
- **Hungry Cartoon Black Hole Mascot:** Nibbles with animated tracking eyes, cheeks, and gulping mouth.
- **Initial Documentation:** Created `01_REFERENCE_GAME_ANALYSIS.md`, `02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md`, and `03_GAME_DESIGN_DOCUMENT.md`.
