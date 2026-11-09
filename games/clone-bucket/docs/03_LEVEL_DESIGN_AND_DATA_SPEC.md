# 03. Level Data & Dataset Specification

**Scope:** level table schema, dataset plumbing, level-to-prefab mapping, the live playable order, lock and boss data, and the on-screen level list for clone-bucket (recovered from the v4.1.0.0 build of our earlier title).
**Source of truth:** the recovered build (v4.1.0.0, the Android APK of our earlier title). This is an internal rebuild after source loss. The original engine was Unity 2022.3.62f2, built-in render pipeline. The rebuild targets Unity 6 with the built-in render pipeline (Route C, `08_IMPLEMENTATION_PLAN.md`). The level table and prefabs are reused unchanged.

**Tags:** `[Recovered]` = read from the recovered build, path given. `[Inferred]` = deduced from recovered data, not confirmed by code. `[Unknown]` = not recoverable from what we have.

**Recovery root (used in all paths below):** `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/Assets/` (AssetRipper 2.0.0 export of the merged APK). C# method bodies are compiler stubs in this export; only signatures, fields, enums, and serialized values are recovered.

---

## 1. Dataset plumbing

| Item              | Value                                                                                                                                                                                                              | Tag                                   |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------- |
| Bundled table     | `TextAsset/LevelsData.txt` (107 data rows + header, 20 columns)                                                                                                                                                    | [Recovered]                           |
| Dataset config    | `MonoBehaviour/DataSetConfig_Levels.asset`: `id` = `LevelsData`, `_dataAsset` = `TextAsset/LevelsData.txt`, `_separatorType` = 0 (`Csv`), `remoteFileName` = `LevelsData.csv`, `remoteFeatureName` = `LevelFunnel` | [Recovered]                           |
| Serialized keys   | 19 entries in `_serializedKeys` (the CSV header minus `UniqueRowIndex`)                                                                                                                                            | [Recovered]                           |
| Database config   | `MonoBehaviour/DatabaseConfig.asset`: `mainDataSetId` = `LevelData`, `allDataSets` = [DataSetConfig_Levels, DataSetConfig_Camera]                                                                                  | [Recovered]                           |
| Levels lookup key | `LevelsController` const `DataSetId` = `"LevelsData"`                                                                                                                                                              | [Recovered]                           |
| Mismatch          | `mainDataSetId` = `LevelData` does not equal the levels dataset id `LevelsData`. Effect on runtime lookup not confirmed.                                                                                           | [Unknown]                             |
| Remote override   | `DatabaseController.TryOverrideDataSetsWithRemoteFiles()` exists. Its body is a stub, so whether a remote `LevelsData.csv` replaces the bundled file, and when, is not confirmed.                                  | [Recovered] name, [Unknown] behaviour |
| Remote A/B flags  | `ABTestVariables` private bools `isMixedLevels`, `withBossLevels`, `bossLevelEveryFive`. Not serialized; values come from remote config.                                                                           | [Recovered] name, [Unknown] values    |

**Fallback rule (proposed for our rebuild):** ship the 107-row bundled table; accept a remote table of the same schema if the publisher supplies one. Remote values per cohort are not known to us (see README, open items).

---

## 2. CSV schema (20 columns)

Header line, verbatim: `UniqueRowIndex,Level Index,Mini Level Index,Level Name,Level Type,Level Difficulty,Is Looping,Is Boss Level,Start Ball Amount,Target Ball Amount,Perfect Ball Amount,Gate Types,Gate Values,ColorSchemeBalls,ColorSchemeMazeWalls,ColorSchemeMazeBackground,ColorSchemeStageBackground,ColorSchemeBucket,ColorSchemeBucketGuides,ColorSchemeBucketStand` [Recovered]

| #     | Column                                                                                                                                                            | Observed values                                                                 | Meaning                                                                                                             | Tag                                    |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| 1     | UniqueRowIndex                                                                                                                                                    | 1–107, unique                                                                   | Row id; equals row number                                                                                           | [Recovered]                            |
| 2     | Level Index                                                                                                                                                       | equal to UniqueRowIndex in every row                                            | Index key. Read by `LevelsController` (const `LevelIndex`). Does not equal the on-screen number (see §4).           | [Recovered] values, [Inferred] meaning |
| 3     | Mini Level Index                                                                                                                                                  | always `1`                                                                      | Index of a mini-level inside a level. Always 1 here.                                                                | [Recovered] values, [Unknown] meaning  |
| 4     | Level Name                                                                                                                                                        | prefab name; `BossLevel1` appears twice (rows 14 and 99, identical data)        | Prefab under `Resources/prefabs/minilevels/`                                                                        | [Recovered]                            |
| 5     | Level Type                                                                                                                                                        | always `Prefab`                                                                 | `LevelType` enum: `Prefab = 0`, `Json = 1`                                                                          | [Recovered]                            |
| 6     | Level Difficulty                                                                                                                                                  | always `0`                                                                      | Unused in this table                                                                                                | [Recovered] values, [Unknown] meaning  |
| 7     | Is Looping                                                                                                                                                        | `TRUE` for 100 rows; `FALSE` for rows 1–6 and 33                                | Maps to `LevelData.isLooping`. Runtime behaviour is not recovered.                                                  | [Recovered] values, [Inferred] meaning |
| 8     | Is Boss Level                                                                                                                                                     | `TRUE` for rows 14, 27, 42, 60, 74, 99                                          | Boss flag; boss prefabs use moving gates (§7)                                                                       | [Recovered]                            |
| 9     | Start Ball Amount                                                                                                                                                 | `5` (102 rows), `10` (rows 1–2), `7` (row 46), `20` (row 81), `100` (row 83)    | Balls spawned into the maze at level start (at `LevelsConfig.ballSpawnPositions`)                                   | [Recovered] values, [Inferred] meaning |
| 10    | Target Ball Amount                                                                                                                                                | 10 to 1600                                                                      | Balls required to complete the level (the `Target` key)                                                             | [Recovered] values, [Inferred] meaning |
| 11    | Perfect Ball Amount                                                                                                                                               | 1.2× to 50× target; always greater than target                                  | Second threshold, likely for the top star. Star logic not recovered.                                                | [Recovered] values, [Inferred] meaning |
| 12    | Gate Types                                                                                                                                                        | `;`-separated, parallel to column 13; values `Multiply`, `Subtract`, `Divide`   | `GateType` enum: `Multiply = 0`, `Subtract = 1`, `Divide = 2`. No `Add`.                                            | [Recovered]                            |
| 13    | Gate Values                                                                                                                                                       | `;`-separated integers; 2 to 11 gates per row; 24 distinct values from 2 to 150 | Operand for the gate in column 12 at the same index                                                                 | [Recovered]                            |
| 14–20 | ColorSchemeBalls, ColorSchemeMazeWalls, ColorSchemeMazeBackground, ColorSchemeStageBackground, ColorSchemeBucket, ColorSchemeBucketGuides, ColorSchemeBucketStand | all `Grey` in all 107 rows                                                      | Colour scheme name. Five schemes exist as assets: `ColorSchemeConfig_Black`, `_Blue`, `_Blue2`, `_Grey`, `_Orange`. | [Recovered] values, [Inferred] meaning |

---

## 3. Level = prefab

- Each row maps to `Resources/prefabs/minilevels/<Level Name>.prefab`. All 106 unique names resolve. [Recovered]
- The folder holds 115 prefabs. Nine are not in the CSV: `BossLevel6`–`BossLevel13` and `StartLevel5M_2.8`. [Recovered]
- A level prefab contains the maze, a `Multiplicators` container, gate objects (`Multiplicator (N)`, `SubGate`, `DivideGate`), and at most one `Chain` (lock). The `LevelView` prefab is the configured `prefab` in `LevelsConfig`. [Recovered]
- `LevelData` has `miniLevelNames` and `LevelsController.StartNextChunk()` exists, so one level can be several mini-levels joined along `distanceBetweenMiniLevels` (`18, 0, 0`). Every CSV row has `Mini Level Index = 1`, so the shipped table uses one mini-level per level. [Recovered] fields, [Inferred] behaviour.

---

## 4. Live playable order

**Rule:** the playable sequence is the CSV rows in `Level Index` order, with two kinds of row removed:

1. every row whose `Level Name` contains `2.8` (35 rows), and
2. the row `LevelSquareRein1` (Level Index 34).

That leaves **71 live rows**. On-screen level N = live row N. [Recovered: screenshot check, lead; the rule itself is computed from the CSV.]

**Verification:** the lead checked this rule against screenshots for on-screen levels 1–5, 9, 15, 20, 26–29, 31, 37, 42 and 50 (16 levels). Independently, the doc-03 matrix levels 13, 16, 17, 26, 27, 28, 29, 45 and 50 match the live list on target and gate set (for example, live 45 = `Level41`, target 800, gates ×6 ×3 ×8 ×4 ×10 ÷2 −40).

**Not verified:** other A/B cohorts. `ABTestVariables` can switch mixed levels, boss levels, and boss-every-five, all from remote config. Whether the live order changes per cohort is [Unknown].

**Why the on-screen number differs from `Level Index`:** the on-screen number is a position in the live list, not the row index. For example, on-screen 9 = row 11 (`Level5`, target 500). [Recovered] fact; mechanism [Inferred].

---

## 5. Difficulty curve

No difficulty tiers are stored: `Level Difficulty` is 0 in every row [Recovered]. Read difficulty from the targets, gates and locks in the live table (§8) [Inferred].

---

## 6. Lock costs

- Each lock is a `Chain` MonoBehaviour with serialized `int UnlockNumber` (cost). The `Chain` also has `CounterText`, `counterBG`, `P_Lock`. `ChainTrigger` (tag `Ball`) and `ChainSegment` complete the lock. [Recovered] (`Assets/Scripts/Assembly-CSharp/Chain.cs`)
- 38 level prefabs used by the table contain a lock: 26 on the live list, 12 on `2.8` rows (not live). The folder has 41 prefabs with a `Chain`; the other 3 are the unused `BossLevel11`–`13`. [Recovered]
- Costs present: 15, 30, 40, 45, 50, 60, 80, 100, 120. (15 is on a 2.8 row only.) [Recovered]
- Cross-check: on-screen 9 (`Level5`) = cost 30, matching the screenshot counter `0/30`. On-screen 27 and 28 = cost 100, which agrees with the earlier doc-03 matrix (levels 27 and 28 listed as padlock 100); that matrix is not a screenshot check. Counters `0/40` and `0/120` also appear in the screenshots and match costs 40 and 120 in the table. [Recovered] values, [Inferred] matches.
- Live lock positions are in §8. Positions 51–68: 51 `Level52` = 80; 53 `Level54` = 40; 60 `Level40` = 100; 64 `Level49` = 120; 68 `Level57` = 30. [Recovered]

---

## 7. Boss levels

- Boss rows: 14 `BossLevel1`, 27 `BossLevel2`, 42 `BossLevel3`, 60 `BossLevel4`, 74 `BossLevel5` (CSV `Level Index`). Row 99 is a duplicate of `BossLevel1`. [Recovered]
- On the live list, boss levels are on-screen 11, 22, 33, 44 and 55. [Recovered]
- Targets: `BossLevel1` 600, `BossLevel2` 800, `BossLevel3` 800, `BossLevel4` 600, `BossLevel5` 600. [Recovered]
- Boss prefabs contain moving gates (`isMoveX = 1`, `MoveTime` 3 or 5 for the CSV bosses). `BossLevel6`–`BossLevel13` have moving gates too, but they are not in the table. [Recovered]
- Boss-specific rules (camera offset, end-level behaviour) are not in the recovered code. `CameraMove.BossLevelBucketOffset` = 6.17 is serialized. [Recovered] value, [Unknown] rule

---

## 8. On-screen levels 1–50 (live order)

Columns: on-screen number (live position), CSV row, name, start balls, target, perfect, gates in CSV order (`×` multiply, `−` subtract, `÷` divide), lock cost (`Chain.UnlockNumber`, `–` = none), notes. Loop = `Is Looping`. Boss = `Is Boss Level`.

| On-screen | CSV row | Name        | Start | Target | Perfect | Gates                             | Lock | Notes      |
| --------: | ------: | ----------- | ----: | -----: | ------: | --------------------------------- | ---: | ---------- |
|         1 |       1 | StartLevel1 |    10 |     60 |     110 | ×11 ×6                            |    – |            |
|         2 |       2 | StartLevel2 |    10 |     40 |      60 | ×4 ×6                             |    – |            |
|         3 |       3 | StartLevel3 |     5 |    120 |     240 | ×4 ×3 ×6 ×4                       |    – |            |
|         4 |       4 | StartLevel4 |     5 |    180 |     300 | ×4 ×3 ×3 ×3 ×3 ×7                 |    – |            |
|         5 |       6 | StartLevel5 |     5 |    660 |     900 | −15 ×4 ×3 ×4 ×11 ×5               |    – |            |
|         6 |       7 | Level1      |     5 |    420 |    1200 | −50 ×4 ×3 ×6 ×4 ×7                |    – | Loop       |
|         7 |       9 | Level2      |     5 |    330 |    1300 | ×6 ×4 ×6 ×11                      |  100 | Loop       |
|         8 |      10 | Level3      |     5 |    600 |     960 | −20 ×4 ×3 ×8 ×15 ×3 −20           |   40 | Loop       |
|         9 |      11 | Level5      |     5 |    500 |    1000 | ÷2 ×7 ×9 ×3 ×11 −70 ×6            |   30 | Loop       |
|        10 |      13 | Level4      |     5 |    600 |     900 | ×4 ×6 ×5 ×6                       |   30 | Loop       |
|        11 |      14 | BossLevel1  |     5 |    600 |     900 | ×4 ×3 ×6 ×3                       |  100 | Loop, boss |
|        12 |      15 | Level11     |     5 |    800 |    1080 | −40 ÷2 ×10 ×4 ×8 ×3 ×6            |    – | Loop       |
|        13 |      16 | Level14     |     5 |    360 |    1400 | −50 ÷2 ×6 ×5 ×9 ×3 ×9             |    – | Loop       |
|        14 |      17 | Level13     |     5 |    540 |     900 | ×4 ×3 ×3 ×5 ×3                    |   80 | Loop       |
|        15 |      18 | Level12     |     5 |    900 |    1080 | ×6 ×3 ×3 ×5 ×6 ×4                 |    – | Loop       |
|        16 |      20 | Level16     |     5 |    600 |     840 | ×11 ×11 ×7 ×6 ×4                  |    – | Loop       |
|        17 |      22 | Level21     |     5 |    500 |     900 | −15 ×4 ×5 ×6 ×11                  |    – | Loop       |
|        18 |      23 | Level22     |     5 |    360 |     650 | ×4 ×8 ×6 ×3 ×3                    |   80 | Loop       |
|        19 |      24 | Level26     |     5 |    380 |     700 | ÷2 −70 ×9 ×3 ×11 ×3 ×11 ÷2        |    – | Loop       |
|        20 |      25 | Level24     |     5 |   1400 |    2000 | ×9 ×5 ×10 ×4 ×8                   |   40 | Loop       |
|        21 |      26 | Level25     |     5 |    500 |    1200 | ×3 ×3 ×3 ×3 ×3                    |    – | Loop       |
|        22 |      27 | BossLevel2  |     5 |    800 |    1200 | ×4 ×3 ×7 ×5 ×3                    |  100 | Loop, boss |
|        23 |      28 | Level6      |     5 |   1600 |    2300 | ×5 ×7 ×6 ×11                      |    – | Loop       |
|        24 |      29 | Level7      |     5 |    500 |    1100 | ÷2 ×9 ×4 ×12                      |    – | Loop       |
|        25 |      30 | Level8      |     5 |    300 |     850 | ×4 ×5 ×9 ×5 ×3                    |    – | Loop       |
|        26 |      31 | Level9      |     5 |    750 |    1400 | −60 ×4 ×8 ×9 ×13 ×6               |    – | Loop       |
|        27 |      32 | Level10     |     5 |   1500 |    2000 | ×5 ×3 ×3 ×7 ×9                    |  100 | Loop       |
|        28 |      36 | Level15     |     5 |    950 |    2200 | ×4 ×3 ×3 ×6 ×16 ×3                |  100 | Loop       |
|        29 |      37 | Level17     |     5 |    800 |    1100 | ÷2 −5 ×5 ×3 ×4 ×6 ×11 ×4 −5 −5 −5 |    – | Loop       |
|        30 |      38 | Level18     |     5 |    660 |    1300 | ÷2 −20 ×3 ×4 ×8 ×11 ×6 ×4         |    – | Loop       |
|        31 |      40 | Level19     |     5 |   1400 |    2880 | ×3 ×3 ×4 ×9 ×4 ×3                 |  120 | Loop       |
|        32 |      41 | Level20     |     5 |    450 |     780 | ×4 ×3 ×5 ×8 ×13                   |    – | Loop       |
|        33 |      42 | BossLevel3  |     5 |    800 |    1200 | ×4 ×3 ×7 ×3 ×4 ×4                 |  100 | Loop, boss |
|        34 |      44 | Level23     |     5 |    750 |    1200 | ×6 ×5 ×11 ×3 ×4 ×3 ×4 ×5          |    – | Loop       |
|        35 |      47 | Level27     |     5 |    400 |     600 | ×3 ×5 ×4 ×5 ×6                    |   30 | Loop       |
|        36 |      49 | Level28     |     5 |   1000 |    1500 | ÷2 ×9 ×5 ×3 ×4                    |    – | Loop       |
|        37 |      50 | Level29     |     5 |    500 |     900 | ×16 ×7 ×3 ×4 ×3 ×5                |    – | Loop       |
|        38 |      51 | Level30     |     5 |    300 |     420 | ÷2 ×11 ×11 ×16                    |    – | Loop       |
|        39 |      52 | Level31     |     5 |    420 |    1200 | ×7 ×4 ×6 ×3 ×4 −50                |    – | Loop       |
|        40 |      53 | Level32     |     5 |    330 |    1300 | ×11 ×6 ×4 ×6                      |  100 | Loop       |
|        41 |      54 | Level33     |     5 |    600 |     960 | −20 ×3 ×15 ×8 ×3 ×4 −20           |   40 | Loop       |
|        42 |      58 | Level34     |     5 |    600 |     900 | ×6 ×5 ×6 ×4                       |   30 | Loop       |
|        43 |      59 | Level35     |     5 |    500 |    1000 | ×6 −70 ×11 ×3 ×9 ×7 ÷2            |   30 | Loop       |
|        44 |      60 | BossLevel4  |     5 |    600 |    1100 | ×4 ×4 ×3 ×7 ×4                    |  100 | Loop, boss |
|        45 |      61 | Level41     |     5 |    800 |    1080 | ×6 ×3 ×8 ×4 ×10 ÷2 −40            |    – | Loop       |
|        46 |      62 | Level42     |     5 |    900 |    1080 | ×4 ×6 ×5 ×3 ×3 ×6                 |    – | Loop       |
|        47 |      63 | Level43     |     5 |    540 |     900 | ×3 ×5 ×3 ×3 ×4                    |   80 | Loop       |
|        48 |      64 | Level44     |     5 |    360 |    1400 | ×9 ×3 ×9 ×5 ×6 ÷2 −50             |    – | Loop       |
|        49 |      65 | Level45     |     5 |    950 |    2200 | ×3 ×16 ×6 ×3 ×3 ×4                |  100 | Loop       |
|        50 |      66 | Level51     |     5 |    500 |     900 | ×11 ×6 ×5 ×4 −15                  |    – | Loop       |

**Lock positions beyond 50:** 51 (`Level52`) 80; 53 (`Level54`) 40; 60 (`Level40`) 100; 64 (`Level49`) 120; 68 (`Level57`) 30. [Recovered]

**Loop on-screen 1–5:** `Is Looping` = FALSE for on-screen 1–5 (rows 1, 2, 3, 4, 6). It is TRUE from on-screen 6. [Recovered] values.

Full live list (71 rows) is generated from `TextAsset/LevelsData.txt` by the rule in §4.

---

## 9. LevelsConfig (recovered values)

`MonoBehaviour/LevelsConfig.asset` (class `Levels.LevelsConfig : ScriptableObject`). [Recovered]

| Field                     | Value                                                     |
| ------------------------- | --------------------------------------------------------- |
| jsonBaseLevelPrefab       | `GameObject/LevelGenerative.prefab`                       |
| prefab                    | `GameObject/LevelView.prefab`                             |
| cameraLocalPosition       | (0, −0.4, −16.93)                                         |
| cameraLocalRotation       | (3, 0, 0)                                                 |
| ballSpawnPositions        | 20 Vector3 entries; first (0, 0, 0), last (0.27, 0.49, 0) |
| distanceBetweenMiniLevels | (18, 0, 0)                                                |
| ballGoalPercentTarget     | 0.25                                                      |
| ballGoalPercentPerfect    | 0.75                                                      |

`LevelsController` also has constants `Ball Goal Percent Target` / `Ball Goal Percent Perfect`, and methods `SetDynamicBallGoals(name, maxPossibleBalls)` and `FloorToMultipleOf25`. The CSV has no such columns and the method bodies are stubs, so whether these percentages set the target and perfect amounts at runtime is [Unknown]. The rebuild has no `SetDynamicBallGoals`: the table's Target and Perfect are the goals (doc 09 D2).

---

## 10. Gates in level data

- Gate types: `Multiply`, `Subtract`, `Divide` only. [Recovered]
- Prefab classes: `Multiplicator`, `SubGate` (serialized `startValue`), `DivideGate`; all inherit `GateBase` (`isMoveX`, `MoveTime`, `stopped`, `startPosX`). [Recovered]
- Gate values come from the CSV at runtime. TMP labels inside level prefabs disagree with the CSV on every level checked (for example, `Level1` labels read X3, X2, X5, X6, −50; the CSV reads −50 ×4 ×3 ×6 ×4 ×7). Treat the CSV as authoritative. [Recovered] labels, [Inferred] authority
- No cooldown or trigger-timing field is serialized on gates [Recovered]. The re-trigger guard is the dictionary `BallsController._ignoredMultiplicatorsByBalls` [Recovered field], i.e. not a timer [Inferred].

---

## 11. Procedural maze generator (not used by CSV levels)

`MonoBehaviour/MazePathGeneratorConfig.asset` (class `MazePathGeneratorConfig`): [Recovered]

| Gate          | Prefab                            | Spawn chance | Count (min–max) | Value (min, max, step) |
| ------------- | --------------------------------- | -----------: | --------------- | ---------------------- |
| subGate       | `GameObject/SubGate.prefab`       |          0.2 | 1–2             | 10, 50, 10             |
| divideGate    | `GameObject/DivideGate.prefab`    |          0.1 | 1–2             | 1, 1, 1                |
| multiplicator | `GameObject/Multiplicator.prefab` |          0.7 | 1–5             | 2, 8, 1                |

- Also: `dynamicWallsPrefabs` (8 refs), `rotatorPrefabs` (2 refs).
- The generator is wired to `MiniLevelView.mazeJsonGenerator` and used with `LevelType.Json`. All 107 CSV rows are `Prefab`, so these settings are not used by the shipped levels. [Recovered] wiring, [Inferred] unused

---

## 12. Open items

- Live `LevelsData.csv` per cohort: request from the publisher or the remote-config backend. [Unknown]
- Whether the live order rule holds for all cohorts. [Unknown]
- Star thresholds (what the second and third stars require), `Perfect Ball Amount` semantics, and fail/complete rules. [Unknown]
- Meaning of `Mini Level Index`, `Level Difficulty`, and `Is Looping` at runtime. [Unknown]
- Whether `Ball Goal Percent` settings change target and perfect at runtime. [Unknown]
- Physical gate and maze behaviour in the rebuild: see `02_GAMEPLAY_MECHANICS_SPEC.md` and `04_PHYSICS_AND_RENDERING_SPEC.md`.
