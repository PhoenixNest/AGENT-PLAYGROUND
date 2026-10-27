# 09. Visual Parity & Screenshot Benchmark Comparison Report

> **Project:** Orbital Salvage (Space Trash Clean)  
> **Source Prototype:** Web Application (`games/maze-puzzle/web`)  
> **Target Production Engine:** Unity 6 LTS (`6000.4.6f1`)  
> **Mobile Target Viewport:** $430\text{pt} \times 932\text{pt}$ (Apple iPhone 16 Pro Max Logical Resolution)  
> **Screenshot Archive Directories:** [`./web/screenshots/`](./web/screenshots/), [`./unity/screenshots/`](./unity/screenshots/)  
> **Capture note:** Web captures are $1252 \times 1528$ (device frame included). Unity captures are $860 \times 1864$ ($2\times$ of $430 \times 932$). They are **not** pixel-overlay comparable.  
> **Emoji note:** Fluent 3D emoji sprites now live in `unity/maze-puzzle/Assets/Resources/Emojis/3D/` and load via `Resources.Load` (the previous file-path loader would have shown circles on device builds).

### Measured harness status (2026-10-04)

The 6/10-style scores in section 1 are the 2026-09-30 uGUI review. They are not the current pixel comparison. Re-capture of the 11 harness stills (zero profile, bar 99.0% within 16 and SSIM 0.985) on the Canvas2D player:

| Still    | pix16 % |   SSIM | bar  |
| -------- | ------: | -----: | ---- |
| loading  |   99.55 | 0.9908 | pass |
| game     |   99.51 | 0.9919 | pass |
| pause    |   99.25 | 0.9877 | pass |
| settings |   99.20 | 0.9890 | pass |
| debrief  |   99.05 | 0.9856 | pass |
| trophy   |   98.80 | 0.9815 | fail |
| skies    |   98.72 | 0.9782 | fail |
| home     |   98.67 | 0.9799 | fail |
| daily    |   98.05 | 0.9706 | fail |
| hats     |   98.01 | 0.9649 | fail |
| donuts   |   97.78 | 0.9614 | fail |

Game PNG SHA256 `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`. Pause PNG SHA256 `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. Failures are mostly text-edge anti-aliasing (Chromium DirectWrite vs exact-area coverage). Flow shots: 6 of 55 at the bar, 0 blank. Live renderer vs recorded canvas streams: 30/30 byte-identical. Controller goldens: 9/9. UI restoration is not complete. No phone or tablet was used.

---

## 1. Visual Benchmark Archive Summary

Using Antigravity's built-in browser engine, 13 full-resolution reference screenshots were captured from the active web prototype at $430 \times 932$ viewport dimension and archived for parity review. Matching Unity screenshots were captured via runtime editor tooling. Scores below are from the 2026-09-30 visual review (web vs Unity), not claims of pixel identity.

| #   | Web Capture                                                                      | Unity Capture                                                                          | Web Screen / Modal Name    | Route / State ID     | Core UI & Visual Elements                                                                              | Status (web vs Unity) | Main gap                                                                       |
| --- | -------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- | -------------------------- | -------------------- | ------------------------------------------------------------------------------------------------------ | --------------------- | ------------------------------------------------------------------------------ |
| 01  | [`01_home_screen.png`](./web/screenshots/01_home_screen.png)                     | [`01_unity_home_screen.png`](./unity/screenshots/01_unity_home_screen.png)             | Main Home Lobby            | `#page-home`         | Top bar (Coins pill, sound, settings), 3D logo, mascot diorama, launch console, bottom dock            | 6/10                  | Diorama / mascot 3D presence and launch-console polish lag the web hero.       |
| 02  | [`02_maps_screen.png`](./web/screenshots/02_maps_screen.png)                     | [`02_unity_maps_screen.png`](./unity/screenshots/02_unity_maps_screen.png)             | Cleanup Maps Page          | `#page-maps`         | 50-sector campaign matrix, star rating indicators, world categorization                                | 7.5/10                | Grid/tile chrome and star badges are close; world tab art still thinner.       |
| 03  | [`03_daily_operations.png`](./web/screenshots/03_daily_operations.png)           | [`03_unity_daily_operations.png`](./unity/screenshots/03_unity_daily_operations.png)   | Daily Operations Shift     | `#page-daily`        | 3 rotating bounty missions, coin rewards, countdown timer                                              | 6.5/10                | Mission cards and tier chrome lack web density and illustration detail.        |
| 04  | [`04_toy_shop_donuts.png`](./web/screenshots/04_toy_shop_donuts.png)             | [`04_unity_toy_shop_donuts.png`](./unity/screenshots/04_unity_toy_shop_donuts.png)     | Toy Shop (Donuts Tab)      | `#page-shop` (rings) | Daily stipend banner (+100 🪙), Donut skin cards, unlock prices                                        | ~7/10                 | Structure matches; item art is color-dot placeholders, not skin previews.      |
| 05  | [`05_toy_shop_hats.png`](./web/screenshots/05_toy_shop_hats.png)                 | [`05_unity_toy_shop_hats.png`](./unity/screenshots/05_unity_toy_shop_hats.png)         | Toy Shop (Hats Tab)        | `#page-shop` (hats)  | Singularity skins: Nibbles the Void / Berry Swirl Vortex / Sleepy Star Cloud                           | ~7/10                 | Labels match; swatches are color dots, not singularity art.                    |
| 06  | [`06_toy_shop_skies.png`](./web/screenshots/06_toy_shop_skies.png)               | [`06_unity_toy_shop_skies.png`](./unity/screenshots/06_unity_toy_shop_skies.png)       | Toy Shop (Skies Tab)       | `#page-shop` (skies) | Cosmic skybox themes: Cotton Candy Nebula / Cheese Moon Orbit / Mint Soda Lagoon                       | ~7/10                 | Same structure gap — color-dot placeholders instead of sky previews.           |
| 07  | [`07_trophy_room.png`](./web/screenshots/07_trophy_room.png)                     | [`07_unity_trophy_room.png`](./unity/screenshots/07_unity_trophy_room.png)             | Trophy Room                | `#page-achievements` | Achievement badges and scrap claim actions (no progress bars)                                          | 8/10                  | Closest meta screen; minor badge/layout spacing differences.                   |
| 08  | [`08_settings_screen.png`](./web/screenshots/08_settings_screen.png)             | [`08_unity_settings_screen.png`](./unity/screenshots/08_unity_settings_screen.png)     | Game Settings              | `#page-settings`     | Master sound toggle, SFX slider, Ambience slider, Haptic toggle, Data Reset                            | 7.5/10                | Controls present; handlers for sound/settings purchases remain stubs in Unity. |
| 09  | [`09_gameplay_sector_1.png`](./web/screenshots/09_gameplay_sector_1.png)         | [`09_unity_gameplay_sector_1.png`](./unity/screenshots/09_unity_gameplay_sector_1.png) | Active Sector 1 Gameplay   | `#page-game`         | Top HUD (Pause, Level Capsule, Restart), Centrifuge ring, gates, debris, Nibbles                       | 4.5/10                | Real Physics2D maze, but Sector 1 layout, gate art, and FX diverge from web.   |
| 10  | [`10_gameplay_paused.png`](./web/screenshots/10_gameplay_paused.png)             | [`10_unity_gameplay_paused.png`](./unity/screenshots/10_unity_gameplay_paused.png)     | Pause Overlay Modal        | `#modal-pause`       | Resume, Restart, Sound, Choose Sector, Return to Home                                                  | 7/10                  | Action list matches; backdrop is dimmed, not blurred like the web overlay.     |
| 11  | [`11_victory_debrief.png`](./web/screenshots/11_victory_debrief.png)             | [`11_unity_victory_debrief.png`](./unity/screenshots/11_unity_victory_debrief.png)     | Victory Debrief Modal      | `#modal-debrief`     | Gold celebration header, snacks/coins stats, **Next Map** button (no 3-star rating UI)                 | 7.5/10                | Layout close; celebration art/confetti thinner than web.                       |
| 12  | [`12_defeat_modal.png`](./web/screenshots/12_defeat_modal.png)                   | [`12_unity_defeat_modal.png`](./unity/screenshots/12_unity_defeat_modal.png)           | Defeat / Retry Modal       | `#modal-defeat`      | Quota failure status; Backup Pod when ≥70% and unused; Retry / Hangar                                  | 7.5/10                | Backup Pod wiring now exists; illustration density still below web.            |
| 13  | [`13_loading_screen.png`](./web/screenshots/13_loading_screen.png)               | [`13_unity_loading_screen.png`](./unity/screenshots/13_unity_loading_screen.png)       | Initial App Boot Screen    | `#page-loading`      | Spinning donut mascot, animated progress bar, cosmic calibration tips                                  | 8/10                  | Strong structural match; spinner/tip animation timing differs.                 |
| 14  | [`14_tutorial_phase1_swipe.png`](./web/screenshots/14_tutorial_phase1_swipe.png) | —                                                                                      | Onboarding Phase 1 (Swipe) | `#tutorial-overlay`  | `↻ SWIPE TO ROTATE` top badge, dashed rotating sweep track, animated finger pointer, bottom comms card | 9/10                  | Bottom docked comms card with mascot avatar and live broadcast dot.            |
| 15  | [`15_tutorial_phase2_gate.png`](./web/screenshots/15_tutorial_phase2_gate.png)   | —                                                                                      | Onboarding Phase 2 (Gate)  | `#tutorial-overlay`  | Multiplier lock reticle, `⚡ ×2 GATE` badge, pointing hand pointer, cyan comms card (`2. MULTIPLY`)    | 9/10                  | Precise gate tracking with crosshair reticle.                                  |
| 16  | [`16_tutorial_phase3_feed.png`](./web/screenshots/16_tutorial_phase3_feed.png)   | —                                                                                      | Onboarding Phase 3 (Feed)  | `#tutorial-overlay`  | Cascading green chevrons (`▼ ▼ ▼`), target funnel ring, green comms card (`3. FEED`)                   | 9/10                  | Animated chevron cascade into Nibbles.                                         |

### Defeat / Backup Pod (verified)

- **Web (`main.js`):** `usedBackupPod` resets per sector. On defeat, `#defeat-backup-container` is shown only when `progressRatio >= 0.70 && !usedBackupPod`. Clicking `#btn-defeat-backup` sets `usedBackupPod = true`, calls `physics.spawnBackupScrap(8)`, and plays `playBackupPodSound`.
- **Unity (`UIManager` + `GameManager`):** Defeat modal shows a **Launch Backup Pod (+8)** button only when `IsBackupPodEligible` (`!backupPodUsed` and swallowed/quota ≥ 0.7). `LaunchBackupPod()` spawns eight debris pieces and plays the recovery-pod clip.

---

## 2. Feature & Mechanics Parity Matrix (honest)

### A. Rotational Centrifuge Kinematics — **known divergence**

- **Doc 06 canonical:** RCS thruster burst $\omega \mathrel{+}= \text{direction} \times 3.8\text{ rad/s}$.
- **Web (`web/engine/physics.js`):** damping $0.94$; RCS burst $\pm 3.8\text{ rad/s}$; drag clamp $\omega \in [-12, 12]$.
- **Unity (`CentrifugeController.cs`):** damping $0.94$; `thrusterBurstImpulse = 4.0`; `maxAngularVelocity = 25`.
- These are **not** 1:1. Treat thruster impulse and max $\omega$ as known divergences pending a fidelity decision.
- **Deliberate divergence (2026-10-14), drag mapping:** the web prototype turns the ring by the sideways finger movement only. The Unity game (`LiveGameSession.GrabPixels`) makes the part of the ring under the finger follow the finger. `PhysicsEngine.ApplyInputDrag` and every physics golden are unchanged. Design record: GDD section 3.1.
- **Note:** the Unity bullet above names `CentrifugeController.cs`, the retired uGUI shell. The product physics is `Assets/Scripts/Logic/PhysicsEngine.cs`, which matches the web values for damping, burst and clamp.

### B. Sector Maze Geometry & Topology — **known divergence**

- **Docs 03/04/06 canonical clearance:** $\ge 0.16\pi$ gate/divider separation.
- **Web sanitizer (`maze.js`):** `MIN_CLEARANCE = 0.12π` (dividers), `MIN_GATE_CLEARANCE = 0.18π` (gates).
- **Unity sanitizer (`MazeSanitizer.cs`):** same $0.12\pi$ / $0.18\pi$ constants as web; runs in the **editor importer only** (`LevelImporter`), not at runtime.
- Unity builds Sector 1 via Physics2D colliders from JSON — real maze, but gate art/FX and worlds 3–5 mechanics (concentric rings / gearRatio, track walls, deflector pins, one-way flaps) are not fully constructed at runtime.

### C. Space Debris Pieces & Art Assets

- **Web Reference:** Titanium / Solar / Volatile debris with distinct mass and scoring.
- **Unity:** [`DebrisPiece.cs`](./unity/maze-puzzle/Assets/Scripts/Physics/DebrisPiece.cs) & [`SpriteHelper.cs`](./unity/maze-puzzle/Assets/Scripts/Utils/SpriteHelper.cs). 34 Microsoft Fluent 3D Emojis under `Assets/Resources/Emojis/3D/`, loaded with `Resources.Load<Sprite>("Emojis/3D/…")`. Only `Debris_Titanium` prefab exists; magnetic debris maps to titanium.

### D. Mascot Gravitational Attractor & Animation

- **Web Reference:** Inverse-square gravitational suction, event horizon radius, dynamic quota scale growth ($1.0\times \to 1.65\times$), elastic wobble damping (0.84), pupil gaze tracking.
- **Unity Implementation:** [`NibblesController.cs`](./unity/maze-puzzle/Assets/Scripts/Physics/NibblesController.cs) & [`MascotEyeTracker.cs`](./unity/maze-puzzle/Assets/Scripts/Animation/MascotEyeTracker.cs). Physics2D tractor field and gaze tracking exist; juice/FX still short of web canvas polish (no DOTween yet).

### E. User Interface & Screen Flow

- **Web Reference:** 7 screens (Home, Maps, Daily, Shop, Trophies, Settings, In-Game HUD) and 3 modals (Pause, Victory, Defeat).
- **Unity Implementation:** [`UIManager.cs`](./unity/maze-puzzle/Assets/Scripts/UI/UIManager.cs). Procedural uGUI for all 13 reviewed surfaces. Runtime win/lose wiring and Backup Pod path are implemented; shop purchase / sound / settings handlers remain stubs. Victory uses **Next Map** (no 3-star rating strip). Pause actions: Resume, Restart, Sound, Choose Sector, Return to Home. Trophy room has no progress bars (matches web).

---

## 3. Audio Soundscape (15 WAV assets on disk)

Actual files under `unity/maze-puzzle/Assets/Audio/` (same set also mirrored under `Assets/Resources/Audio/` for runtime load):

1. `sfx_centrifuge_ratchet.wav`
2. `sfx_debris_wall_bounce.wav`
3. `sfx_multiplier_pass.wav`
4. `sfx_adder_pass.wav`
5. `sfx_booster_pass.wav`
6. `sfx_singularity_swallow.wav`
7. `sfx_flapper_clack.wav`
8. `sfx_quota_growth.wav`
9. `sfx_victory_fanfare.wav`
10. `sfx_defeat_whimper.wav`
11. `sfx_recovery_pod_launch.wav`
12. `sfx_thruster_burst.wav`
13. `sfx_ui_button_click.wav`
14. `sfx_ui_tab_switch.wav`
15. `sfx_debris_pinball_bumper.wav`

Web synthesis in `audio.js` remains procedural; Unity plays these baked clips via `AudioManager` (no AudioMixer / CoreHaptics yet).

---

## 4. Unity Custom Menu Hierarchy & Edit-Mode UI Previews

To eliminate menu clutter and allow instant inspection of all UI views without entering Play Mode, the Unity menu has been restructured into clean hierarchical submenus under `Orbital Salvage`:

```text
Orbital Salvage
  ├── UI Previews (Edit Mode)
  │     ├── 1. Home Lobby (3D Mascot Diorama & Launch Console)
  │     ├── 2. Cleanup Maps (50 Levels Grid)
  │     ├── 3. Daily Operations (Missions & Rewards)
  │     ├── 4. Toy Shop & Hats (Donuts, Hats, Allowances)
  │     ├── 5. Trophy Room (Achievements & Badges)
  │     ├── 6. Game Settings (Audio, Haptics & Preferences)
  │     ├── 7. In-Game HUD (Level 1 & Centrifuge World)
  │     ├── Modals
  │     │     ├── Show: Pause Modal
  │     │     ├── Show: Victory Debrief
  │     │     ├── Show: Defeat Modal (Emergency Backup Pod)
  │     │     └── Hide All Modals
  │     ├── Hide All UI (World Only)
  │     └── Force Refresh UI Hierarchy
  ├── Build & Setup
  │     ├── Rebuild Complete Game Scene & Prefabs
  │     ├── Configure Emoji Textures As 2D Sprites
  │     └── Batch Import 50 Sectors
  └── Documentation
        └── Open Visual Parity Guide
```

### Visual Parity Improvements & Root Cause Analysis

1. **Zero Missing Glyph Boxes (`□`):**
   - _Issue Identified:_ TextMeshPro's default font `LiberationSans SDF` cannot render Unicode emoji characters, resulting in white boxes (`□`).
   - _Fix:_ Replaced Unicode emoji text strings with native UI `Image` components using Microsoft Fluent 3D Emoji Sprites from [`Assets/Resources/Emojis/3D/`](./unity/maze-puzzle/Assets/Resources/Emojis/3D/) via `Resources.Load`.
2. **Decoupled 2D Centrifuge from Home & Menus:**
   - _Issue Identified:_ The physics centrifuge, debris, and guide rails rendered directly behind the Home screen card simultaneously.
   - _Fix:_ `GameManager` and `UIManager` now control `SetGameplayWorldVisible(bool visible)` to completely hide the 2D world during Home and Menu navigation, and re-enable it exclusively for Sector gameplay.
3. **Multiplier Gate Upright Rotation:**
   - _Issue Identified:_ Multiplier labels (`x2`, `+3`) rotated with the parent gate, rendering upside down.
   - _Fix:_ `MultiplierGate.LateUpdate()` enforces `labelText.transform.rotation = Quaternion.identity` at all times.
4. **Edit-Mode UI Previews Without Play Mode:**
   - Selecting any screen under `Orbital Salvage / UI Previews (Edit Mode)` immediately switches the active screen, configures world visibility, and repaints both the Scene and Game Views without requiring Play Mode.

---

## 5. Verification & Testing Instructions

1. **Open Unity 6:** The project is loaded at [`./unity/maze-puzzle`](./unity/maze-puzzle).
2. **Instant Edit-Mode UI Previews (No Play Button Needed):**
   - Click `Orbital Salvage -> UI Previews (Edit Mode) -> 1. Home Lobby` to view the 3D diorama and launch console.
   - Click `Orbital Salvage -> UI Previews (Edit Mode) -> 2. Cleanup Maps (50 Levels)` to inspect the level selection grid.
   - Click `Orbital Salvage -> UI Previews (Edit Mode) -> Modals -> Show: Victory Debrief` to preview the victory screen (Next Map; no 3-star strip).
   - Click `Orbital Salvage -> UI Previews (Edit Mode) -> Force Refresh UI Hierarchy` to re-instantiate all sprite-driven elements.
3. **Play Mode Execution:**
   - Press the **Play** button `[ ▶ ]` to test live centrifuge rotation, scrap swallowing, and level progression.
   - Compare web vs Unity screenshots side-by-side using the relative links in §1; do not expect pixel overlay given the different capture resolutions.
