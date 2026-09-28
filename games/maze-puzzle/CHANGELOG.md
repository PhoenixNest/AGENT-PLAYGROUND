# Changelog

All notable changes to the **Orbital Salvage: Space Trash Clean** project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.8.0] - 2026-09-27

### Added

- **Unity Migration & iOS Deployment Plan (`08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md`):** Complete architectural transition and deployment specification for migrating from the HTML5 Canvas web prototype to Unity (URP 2D/C#) targeting Apple iPhone iOS. Includes:
  - Technical feasibility analysis of Unity's native Metal rendering, 120 FPS ProMotion, and CoreHaptics.
  - Module-by-module JavaScript-to-C# code translation assessment across physics, level schemas, UI, audio, and economy.
  - **1:1 Sound & Image Asset Inventory (Section 4):** Comprehensive catalogs covering all 15 discrete `.wav` audio cues (exact waveforms, DSP envelopes, and master compressor limiter specs) and all visual/image assets (4 ring skins, 4 black hole skins, 4 multiplier gates, 3 debris variants, 3 celestial skyboxes, deflector bumpers, and 8 mobile UI screens).
  - **Microsoft Fluent 3D Emoji Asset Integration (Section 4.3):** Integrated the MIT-licensed open-source `microsoft/fluentui-emoji` library. Resolved cross-platform emoji rendering divergence between Windows and iOS, bypassed Apple IP emoji restrictions, and provided turnkey fetcher `scripts/download_fluent_3d_emojis.js`.
  - **Asset Verification:** Successfully downloaded all 34 pre-rendered 3D emoji PNG assets into `unity/Assets/Sprites/Emojis/3D/` for TextMeshPro sprite asset generation (`TMP_SpriteAsset_Emojis.asset`).
  - **Complete Animation Migration Architecture (Section 5):** Detailed 1:1 translation specification converting Canvas 2D trigonometric math and particle loops into Unity animation systems (DOTween squash-and-stretch wobble, pupil gaze tracking, dynamic quota growth, Shuriken Keplerian accretion vortex, UV-scrolling booster chevrons, and HingeJoint2D one-way flappers).
  - **Unity Tooling & Automation Stack (Section 6):** Confirmed local Unity 6 (`6000.4.6f1`) and Unity Hub CLI paths, documented Unity Model Context Protocol (MCP) bridge integration (`CoplayDev/unity-mcp`), Batchmode CLI headless workflows, OpenUPM package automation, and a 4-phase hybrid velocity strategy.
  - **Unity MCP Server Setup & Verification:** Configured workspace-level MCP server (`.agents/mcp_config.json`, `.agents/plugins/unity-mcp/`), implemented in-workspace skill runbook (`.agents/skills/unity-mcp/SKILL.md`), and successfully executed FastMCP 3.4.7 stdio connection tests and resource discovery.
  - 7-phase Gantt execution roadmap spanning project scaffolding, ScriptableObject level imports, physics kinematics, UGUI systems, and Xcode TestFlight deployment.
  - Risk analysis and App Store Review Guideline 4.3 defense alignment.

---

## [0.7.0] - 2026-09-24

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
