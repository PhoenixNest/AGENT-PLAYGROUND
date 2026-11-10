# clone-bucket — Internal Rebuild Specification

## 1. What this is

This repository holds the internal design and data specification for rebuilding **clone-bucket**, our own title (bundle ID `io.dev.clone-bucket`, version 1.0.0). The rebuild is based on the recovered v4.1.0.0 build of our earlier title, after its source code and project files were lost in a company system upgrade.

The game: a portrait puzzle. The player rotates a ball maze so balls roll through gates (multiply, subtract, divide). Balls must reach a target count to fill the bucket. Chains and padlocks gate some levels; a skin shop sits alongside the levels.

**Rebuild rule:** the recovered build is the source of truth. The specs describe what the recovered build contains. Where the recovered build does not show something, the specs say `[Unknown]`, not a guess.

**Engine:** the original was built with Unity 2022.3.62f2 (built-in render pipeline, IL2CPP). The rebuild follows **Route C**: the recovered project upgraded to Unity 6, keeping the built-in render pipeline and Gamma colour space. See `docs/08_IMPLEMENTATION_PLAN.md`.

**Platforms:** the rebuild targets iOS, desktop (Standalone) and WebGL. There is no Android support. The earlier title shipped on Android and iOS; only its Android build (v4.1.0.0) has been recovered, so the levels, physics and assets come from that APK. iOS-specific values come from the shared project data (for example the separate `BallsConfigIos`).

---

## 2. Sources

| Source                             | Location                                                | Notes                                                                                                                                                                                 |
| ---------------------------------- | ------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| APK, base + arm64 split (v4.1.0.0) | `games/clone-bucket/sources/multi-maze-3d/apk/4.1.0.0/` | Read-only. SHA-256 manifest: `tools/recovery/SHA256SUMS-apk-4.1.0.0.txt`.                                                                                                             |
| APK backup copy                    | `tools/recovery/apk-backup/4.1.0.0/`                    | Same files, same disk. Keep an off-machine copy.                                                                                                                                      |
| Recovery log                       | `tools/recovery/RECOVERY_LOG.md`                        | Extraction runs, tool versions, what is and is not recoverable.                                                                                                                       |
| Recovered facts (verified)         | `tools/recovery/FINDINGS.md`                            | Physics, balls, gates, locks, levels, controls, economy.                                                                                                                              |
| Recovered Unity project            | `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/`     | AssetRipper 2.0.0, IL2CPP metadata v31.1. Real script fields and config values; method bodies not recoverable. Rebuildable from the APKs via `tools/recovery/scripts/run-export.ps1`. |
| Level reference screenshots        | `games/clone-bucket/refer-levels/`                      | Used to confirm the on-screen level order (see doc 03 §4).                                                                                                                            |

---

## 3. Key recovered facts

**Levels** (`docs/03_LEVEL_DESIGN_AND_DATA_SPEC.md`)

- Bundled table `TextAsset/LevelsData.txt`: 107 rows, 20 columns. Remote override: `LevelsData.csv`, feature `LevelFunnel`.
- Playable order: CSV rows in `Level Index` order, minus rows with `2.8` in the name and minus `LevelSquareRein1`. That leaves 71 live levels. Checked against screenshots.
- Targets 10–1600; perfect amounts 1.2× to 50× target; start balls 5 (10 on levels 1–2).
- Boss levels on-screen 11, 22, 33, 44, 55 (moving gates).

**Locks**

- `Chain.UnlockNumber` in each level prefab. 26 locks on live levels; costs 30, 40, 80, 100, 120 among others.

**Gates**

- Types: `Multiply`, `Subtract`, `Divide` (no `Add`). Values come from the CSV, not from prefab labels.

**Physics and balls** (`docs/04_PHYSICS_AND_RENDERING_SPEC.md`)

- Gravity (0, −20, 0); fixed timestep 0.033 s. Ball mass 0.1, sphere collider radius 0.5466 at prefab scale 0.2.
- Ball pool: 200 pre-warmed bodies in the recovered build; higher-value balls represent large counts.

**Controls and camera**

- Rotation controller: multiplier 3.5, damping 16, axis Z. Main camera FOV 50.

**Economy** (`docs/06_SKIN_AND_ECONOMY_SPEC.md`)

- Shop: three variants, 6 / 7 / 7 skins, all priced 500 (currency `GameManager.GemCounter`). Unlock mechanic not recovered.

Items marked `[Unknown]` in the docs are the ones most likely to need a decision or a data request (see section 5).

---

## 4. Documentation index

Read in order. Each file marks facts `[Recovered]`, `[Inferred]`, or `[Unknown]`.

| #   | File                                     | Scope                                                                                                       |
| --- | ---------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| 01  | `docs/01_SYSTEM_ARCHITECTURE.md`         | Subsystems, managers, ball pooling, and how they map to the recovered classes.                              |
| 02  | `docs/02_GAMEPLAY_MECHANICS_SPEC.md`     | Rotation control, gate and multiplier behaviour, chains and locks, win and fail rules.                      |
| 03  | `docs/03_LEVEL_DESIGN_AND_DATA_SPEC.md`  | Level CSV schema, dataset plumbing, live order rule, lock and boss data, on-screen levels 1–50.             |
| 04  | `docs/04_PHYSICS_AND_RENDERING_SPEC.md`  | Physics settings, materials, collision layers, camera, and render settings.                                 |
| 05  | `docs/05_UML_DIAGRAMS_AND_SCHEMATICS.md` | Class, sequence, and state diagrams for the rebuild.                                                        |
| 06  | `docs/06_SKIN_AND_ECONOMY_SPEC.md`       | Skin shop, currency, and unlock rules.                                                                      |
| 07  | `docs/07_3D_ASSET_AND_MODELING_SPEC.md`  | Recovered asset inventory, reuse workflow, shader re-authoring, QA exceptions.                              |
| 08  | `docs/08_IMPLEMENTATION_PLAN.md`         | Route C decision record, target configuration, package map, work plan, verification, risks, open decisions. |
| 11  | `docs/11_LEADERBOARD_RANKING.md`         | Level-complete world ranking: rank formula, generated rows, climb animation, worked examples.               |
| —   | `docs/CHANGELOG.md`                      | Pre-recovery claims that the recovered build overturned (do not use).                                       |

---

## 5. Status and open decisions

**Status**

- Recovery of the APK, the Unity project export, and the level table is done. Hashes are recorded.
- Level and dataset specs (doc 03) are drafted against the recovered values.
- Method bodies of the recovered build were recovered from `libil2cpp.so` on 2026-11-07 (Ghidra decompile). The written record is in `tools/recovery/recovered/` (start with its `README.md`); scripts are in `tools/recovery/scripts/decompile/`. Gameplay, save format and shaders were reconciled with it; doc 09 §3.6 shows the status of each decision.

**Decided**

- **Engine route: C** (2026-11-06): Unity 6, built-in render pipeline, Gamma. Route A (2022.3) is the fallback if the CrazyLabs SDK does not support Unity 6; URP is an optional later step. Reasons and the full plan are in `docs/08_IMPLEMENTATION_PLAN.md`.
- **Product:** clone-bucket 1.0.0 with no ads, no in-app purchases, no attribution SDKs and no Android support. Target platforms are iOS, desktop and WebGL.

**Gates before work starts** (doc 08)

1. **G0:** CrazyLabs confirms which Unity 6 LTS its TTP SDK supports and supplies the SDK and this app's config. This pins the editor version.
2. **G1:** off-machine backup of the APKs and the Unity export, checked against the SHA-256 manifest.

**Data and rules still needed**

1. **Live data from the publisher** (or the remote-config backend), gate G3:
   - the current `LevelsData.csv` for each cohort (`LevelFunnel`), since the live order may differ by cohort;
   - the values of the remote A/B variables (`isMixedLevels`, `withBossLevels`, `bossLevelEveryFive`).
2. **Rules still unverified.** The star, win and fail rules were recovered (doc 09 §3.6). Items still UNVERIFIED there (for example the tutorial trigger, D32) and any rule that play-testing contradicts are settled against the original build.
3. Other open decisions (D1–D5) are listed in doc 08 §8.

**Risks**

- The APK copies and the Unity export are on the same disk and are not tracked by git. Keep at least one off-machine copy (G1).
- The full risk list is in doc 08 §7.

**Next steps**

1. Close gates G0–G1.
2. Play-test on-screen levels 1–50 on the device side by side with the recovered build (rotation feel, boss levels, multi-chunk levels).
3. Decide the product questions listed in `tools/recovery/recovered/INTEGRATION_NOTES.md` ("Product questions").

## Editor menu

Tools > Clone Bucket in the Unity editor:

- Tools > Clone Bucket > Play From Loading Scene: when ticked (default), Play always starts from the Loading scene.
- Tools > Clone Bucket > Export Xcode Project for iOS Simulator: exports an Xcode project built with the iOS Simulator SDK for ARM64 (Apple Silicon simulators) to `games/clone-bucket/output/clone-bucket-ios-emulator/`, and restores the previous setting afterwards.
- Tools > Clone Bucket > Export Xcode Project for iOS Device: exports an Xcode project built with the iOS device SDK to `games/clone-bucket/output/clone-bucket/`.
- Both exports need the iOS Build Support module for the project's Unity version (install it in Unity Hub). The first export switches the active platform to iOS. Each export replaces the previous project in its folder. The SDK choice is set only for the export; the project's iOS setting is put back afterwards. Open `Unity-iPhone.xcodeproj` in the exported folder with Xcode on a Mac; a device build also needs a signing team set in Xcode.
- `games/clone-bucket/output/` is git-ignored.
- Tools > Clone Bucket > Clear Cached Data: after a confirmation, deletes all PlayerPrefs for the project (current level, gems, scores, selected skin, sound and haptics settings), the contents of `Application.persistentDataPath` and `Application.temporaryCachePath`, and the asset bundle cache, so the next Play starts like a fresh install. It is disabled in Play mode. It does not touch the Library folder or editor preferences.
