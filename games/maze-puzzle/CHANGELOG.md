# Changelog

All notable changes to the **Orbital Salvage: Space Trash Clean** project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

Paths written as `tools/parity/...` in entries dated before 2026-10-08 are the location on that day. Those files now live under `games/maze-puzzle/tools/parity/...`. Entries that name `tools/parity/unity/*.ps1` or `canvas-conformance/run-unity.ps1` record the path used that day. Those Windows wrappers now live under `games/maze-puzzle/tools/parity/windows/`.

### Fixed

#### Unity quota bar and modal backdrop (2026-10-09, `agent/fix/quota-bar-modal-backdrop`)

- The level HUD quota fill paints when mass is collected. The level-1 capture stores `hud-quota-bar` at width 0 (`v:0`), and HomeChrome skipped that element before the live width override. A width of 0 hides the fill again, including after a restart that leaves the quota label unchanged. The web green-to-yellow gradient used once the goal is met is not ported; the captured yellow-to-pink gradient stays.
- Every game-page sheet (`modal_*`, including the surplus debrief and the other debrief and defeat variants) binds the live playfield under the overlay, including the first frame when the session has not rendered yet. The sheet intro uses the exact backdrop blur. `settings_modal_*` is unchanged. Game and pause stills stay `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A` and `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. All 23 `screenshot-states.json` stills matched a GPU paint taken on this branch before the fix.
- EditMode: 354 passed, 82 failed, 2 skipped, 438 total. The 6 new tests passed. The 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9), the same missing-audio-reference set as before. The skips are `FrameRateHarness.Measure` and `MeasureAccumulated`. `PagePreviewTests.Stills_MatchTheParityPlayer_IncludingShopRingsDefault` ran and passed; it had been skipping because `tools/parity/out/unity-before` was absent.

### Changed

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
