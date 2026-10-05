# Changelog

All notable changes to the **Orbital Salvage: Space Trash Clean** project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Changed

#### Unity (2026-10-04, `agent/parity/ship`)

- Play, with no command-line flags, boots `ParityViewer` and `AppController` (Canvas2D). The main scene is a camera. Pointer clicks, drags, and holds go through the letterboxed 430×932 page. Profile is `orbital-salvage-profile.json` under `Application.persistentDataPath` (key `ORBITAL_SALVAGE_CARTOON_PROFILE_V1`). Audio on that path is the C# DSP player. `--parity` and the parity self-tests keep a frozen clock and silent audio.
- Retired the uGUI shell: `UIManager`, `SpriteHelper`, `SafeAreaFitter`, the old `OrbitalSalvage.Core` managers, Physics2D gameplay scripts, TMP Examples & Extras. Fluent emoji and Fredoka/Nunito assets stay. `Assets/Audio/*.wav` stays; the DSP path does not play those files.
- Harness re-measure (same machine, not a device): 11 stills unchanged from the listed baseline (5 pass the 99.0%/0.985 bar, 6 fail, mostly text edges). Game and pause SHA256 unchanged. Live streams 30/30 byte-identical. Controller goldens 9/9. Flow shots 55, 6 at the bar, 0 blank. HomeChrome.Draw one D3D11 run: avg 3.049 ms, p95 4.409 ms. Live frame p95 14.32 ms. EditMode 380/380. This is not a completed UI restoration.

#### Unity (2026-10-04, `agent/parity/lag`)

- Editor Play at Simulator size (1284×2778) was ~16 fps and could paint a mustard page with a blue hero. Cause: live frames unbaked a reused local rect every tick; the 3× raster plus vSync filled the rest of the frame. Product path now unbakes once, turns vSync off, targets 60 fps, and drops raster 3→2→1.5→1 when p95 exceeds 22 ms. After, D3D12 p95 24.69 ms with a purple page and yellow face. Parity stills stay frozen 3× Draw (game/pause hashes unchanged). Phones untested.

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
