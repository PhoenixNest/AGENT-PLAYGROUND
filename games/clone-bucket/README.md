# Multi Maze 3D — Internal Rebuild Specification

## 1. What this is

This repository holds the internal design and data specification for rebuilding **Multi Maze 3D** (version 4.1.0.0), our own title, after the source code and project files were lost in a company system upgrade.

The game: a portrait mobile puzzle. The player rotates a ball maze so balls roll through gates (multiply, subtract, divide). Balls must reach a target count to fill the bucket. Chains and padlocks gate some levels; a skin shop sits alongside the levels.

**Rebuild rule:** the recovered build is the source of truth. The specs describe what the recovered build contains. Where the recovered build does not show something, the specs say `[Unknown]`, not a guess.

**Engine:** the original was built with Unity 2022.3.62f2 (built-in render pipeline, IL2CPP). The engine for our rebuild is pending a decision; see §5.

**Platforms:** the game shipped on Android (Google Play, package `com.AlexandrJanashvili.MultiMaze3D`) and iOS (the reference screenshots are iPhone captures; the build carries a separate `BallsConfigIos`). Only the Android build (v4.1.0.0) has been recovered; iOS-specific values come from the shared project data.

---

## 2. Sources

| Source | Location | Notes |
|---|---|---|
| APK, base + arm64 split (v4.1.0.0) | `games/clone-bucket/sources/multi-maze-3d/apk/4.1.0.0/` | Read-only. SHA-256 manifest: `tools/recovery/SHA256SUMS-apk-4.1.0.0.txt`. |
| APK backup copy | `tools/recovery/apk-backup/4.1.0.0/` | Same files, same disk. Keep an off-machine copy. |
| Recovery log | `tools/recovery/RECOVERY_LOG.md` | Extraction runs, tool versions, what is and is not recoverable. |
| Recovered facts (verified) | `tools/recovery/FINDINGS.md` | Physics, balls, gates, locks, levels, controls, economy. |
| Recovered Unity project | `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/` | AssetRipper 2.0.0, IL2CPP metadata v31.1. Real script fields and config values; method bodies not recoverable. Rebuildable from the APKs via `tools/recovery/scripts/run-export.ps1`. |
| Level reference screenshots | `games/clone-bucket/refer-levels/` | Used to confirm the on-screen level order (see doc 03 §4). |

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
- Ball pool: 200 pre-warmed bodies on Android; higher-value balls represent large counts.

**Controls and camera**
- Rotation controller: multiplier 3.5, damping 16, axis Z. Main camera FOV 50.

**Economy** (`docs/06_SKIN_AND_ECONOMY_SPEC.md`)
- Shop: three variants, 6 / 7 / 7 skins, all priced 500 (currency `GameManager.GemCounter`). Unlock mechanic not recovered.

Items marked `[Unknown]` in the docs are the ones most likely to need a decision or a data request (see section 5).

---

## 4. Documentation index

Read in order. Each file marks facts `[Recovered]`, `[Inferred]`, or `[Unknown]`.

| # | File | Scope |
|---|---|---|
| 01 | `docs/01_SYSTEM_ARCHITECTURE.md` | Subsystems, managers, ball pooling, and how they map to the recovered classes. |
| 02 | `docs/02_GAMEPLAY_MECHANICS_SPEC.md` | Rotation control, gate and multiplier behaviour, chains and locks, win and fail rules. |
| 03 | `docs/03_LEVEL_DESIGN_AND_DATA_SPEC.md` | Level CSV schema, dataset plumbing, live order rule, lock and boss data, on-screen levels 1–50. |
| 04 | `docs/04_PHYSICS_AND_RENDERING_SPEC.md` | Physics settings, materials, collision layers, camera, and render settings. |
| 05 | `docs/05_UML_DIAGRAMS_AND_SCHEMATICS.md` | Class, sequence, and state diagrams for the rebuild. |
| 06 | `docs/06_SKIN_AND_ECONOMY_SPEC.md` | Skin shop, currency, and unlock rules. |
| 07 | `docs/07_3D_ASSET_AND_MODELING_SPEC.md` | Recovered asset inventory, reuse workflow, shader re-authoring, QA exceptions. |
| — | `docs/CHANGELOG.md` | Pre-recovery claims that the recovered build overturned (do not use). |

---

## 5. Status and open decisions

**Status**
- Recovery of the APK, the Unity project export, and the level table is done. Hashes are recorded.
- Level and dataset specs (doc 03) are drafted against the recovered values.
- Method bodies are not recoverable from the export (IL2CPP). Gameplay logic must be re-implemented from the recovered fields and values, and checked against screenshots and play.

**Decisions needed**
1. **Engine route** for the rebuild. Original is Unity 2022.3.62f2 (built-in RP). Options: **A** stay on 2022.3 built-in (fastest, most faithful); **B** Unity 6 + URP (shaders written for URP, colour retuning, package upgrades); **C** Unity 6 + built-in (A plus package upgrades). An implementation plan will be written once a route is chosen.
2. **Live data from the publisher** (or the remote-config backend):
   - the current `LevelsData.csv` for each cohort (`LevelFunnel`), since the live order may differ by cohort;
   - the values of the remote A/B variables (`isMixedLevels`, `withBossLevels`, `bossLevelEveryFive`);
   - `IAPsData.txt` for the `IAPs` feature.
3. **Star thresholds and fail or complete rules.** Not in the recovered code; need play-testing against the original build.

**Risks**
- The APK copies and the Unity export are on the same disk and are not tracked by git. Keep at least one off-machine copy.
- Disable Play Store auto-update for this app on the test device so the build stays at 4.1.0.0.

**Next steps**
1. Decide the engine route (Decisions needed, item 1).
2. Request the cohort data and A/B values (above).
3. Re-implement the gameplay scripts in the recovered project from docs 01–03, then verify on-screen levels 1–50 (doc 03 §8) against the reference screenshots.
