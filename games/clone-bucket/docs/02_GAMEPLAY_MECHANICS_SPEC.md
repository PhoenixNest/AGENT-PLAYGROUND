# 02. Gameplay Mechanics Specification (clone-bucket)

Status: draft rewrite. Internal rebuild of clone-bucket 1.0.0, based on the recovered v4.1.0.0 build of our earlier title. The recovered build is the source of truth.

Tags: **[Recovered]** (read from the export, path given), **[Inferred]** (deduced, reasoning given), **[Unknown]** (logic not recoverable; design and tune in PlayMode).

Export root: `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/`. Paths below are relative to `Assets/`. Recovered fields and serialized values are real; C# method bodies are stubs.

---

## 1. Rotation

### 1.1 Recovered parameters

`RotateController` on every level prefab (115 prefabs under `Resources/prefabs/minilevels/`) [Recovered]:

| Field       | Value                                                                                                                                           |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Multiplier  | 3.5 (all 115)                                                                                                                                   |
| Damping     | 16 (all 115)                                                                                                                                    |
| ScaleByTime | 0                                                                                                                                               |
| Space       | 0 (World)                                                                                                                                       |
| Axis        | (0, 0, 1)                                                                                                                                       |
| Targets     | empty on 102 prefabs; a list of 2–4 rotators on the 13 `BossLevel1`–`13` prefabs                                                                |
| Target      | one maze rotator on the 102 non-boss prefabs (`MazeRotator_OpenDown` / `OpenUp`; fileID not resolved, [Inferred]); unset on the 13 boss prefabs |

### 1.2 Conflicting controller values

`GameManager` in `Scenes/Main.unity` also holds `ControllerMultiplier = 3` and `ControllerDamping = 20` [Recovered]. These are the live pair. `RotateController.Start` copies them over the prefab values on every level (decompile `RotateController_Start`, `decomp-named.c` line 4000), so the prefab values 3.5 and 16 do not govern play. Closed (Open Questions, item 1).

### 1.3 Rotator body

The rotator prefabs have **no Rigidbody** and about 40 `MeshCollider`s each [Recovered: `GameObject/MazeRotator_OpenDown.prefab`]. Whether this couples ball friction is **[Unknown]**.

### 1.4 Input mapping

Drag to angle is linear: the finger's drag (`LeanFinger.GetDeltaDegrees`) × `Multiplier`, scaled by the finger's distance from the target (decompile `RotateController_RotateAB`). The rotation is applied at once, and then eased by `LeanHelper.GetDampenFactor(Damping, dt)` (decompile `RotateController_UpdateRotation`; `docs/09_RUNTIME_CONTRACTS.md` §1.3). Touch tracking is in the Lean Touch shim (`docs/09_RUNTIME_CONTRACTS.md` §2.7). Camera follow is in section 6.

---

## 2. Balls

### 2.1 Speed caps

`MonoBehaviour/BallsConfig.asset` and `BallsConfigIos.asset` [Recovered]: `inMazeMaxSpeed` 10, `outMazeMaxSpeed` 40 (both platforms). Units: **[Unknown]**.

### 2.2 Size tiers

`BallsConfig` [Recovered]: small 0.17, medium 0.19, big 0.22 scale. Base collider radius 0.5466 on prefab scale 0.2 (`GameObject/Ball.prefab`). Tier assignment: **[Unknown]**.

### 2.3 Pool

- `initialPoolAmount` = 200 on both platforms [Recovered].
- `BallsPool`: static `ObjectPool<BallView>` with `PreWarm`, `Get`, `Release` [Recovered: `Scripts/Assembly-CSharp/Balls/BallsPool.cs`].
- Runtime growth past 200: **[Unknown]**.

### 2.4 Value tiers

| Field                   | Android (recovered)  | iOS (recovered)  |
| ----------------------- | -------------------- | ---------------- |
| `useBallValueTiers`     | 1                    | 1                |
| `valuePerTier`          | [1, 2, 3, 999]       | [1, 2, 3]        |
| `totalBallsForNextTier` | [300, 450, 650, 775] | [450, 650, 775]  |
| `chancesToOverrideTier` | [0.6, 0.5, 0.2, 0.1] | [0.5, 0.5, 0.35] |

[Recovered]. [Inferred]: large counts are represented by higher-value balls, not more physics bodies. How `chancesToOverrideTier` is applied: **[Unknown]**.

### 2.5 Layer escalation

| Field                   | Android (recovered) | iOS (recovered) |
| ----------------------- | ------------------- | --------------- |
| `useBallLayers`         | 1                   | 0               |
| `ballLayers`            | [9, 10, 11]         | [9, 10]         |
| `minimumBallsForLayers` | [200, 400, 600]     | [300, 600]      |

[Recovered]. Layers `Ball` (9), `Ball_1` (10), `Ball_2` (11). [Inferred]: as the count crosses each threshold, balls move to the next layer. Per the provisional matrix (doc 04, section 2.6), layers 9, 10 and 11 do not collide with each other. Verify in PlayMode.

### 2.6 Ignore capacity

`ignoreMultiplicatorCapacity` = 10 [Recovered]. Meaning **[Unknown]**; likely the size limit of the per-ball ignored-gate list (section 3.4).

---

## 3. Gates

### 3.1 Types

- Enum `GateType { Multiply, Subtract, Divide }`. There is **no Add gate** [Recovered].
- Classes: `Multiplicator`, `SubGate` (`startValue`, sub-effect counters), `DivideGate` (`ballsCounter`, per-gate counter) [Recovered].
- Tags: `Multiplicator`, `SubGate`, `DivideGate` [Recovered: `TagManager.asset`].

### 3.2 Values in the level table

`TextAsset/LevelsData.txt` (107 rows; live override `LevelsData.csv`) [Recovered]:

| Type     | Gate slots | Values seen                                                                                                              |
| -------- | ---------- | ------------------------------------------------------------------------------------------------------------------------ |
| Multiply | 478        | 2 (3), 3 (141), 4 (100), 5 (51), 6 (66), 7 (18), 8 (14), 9 (26), 10 (5), 11 (38), 12 (2), 13 (5), 15 (2), 16 (6), 20 (1) |
| Subtract | 55         | 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 90, 100, 150                                                                      |
| Divide   | 34         | 2 only                                                                                                                   |

Gate values come from the table at runtime. Labels inside prefabs are stale placeholders [Recovered: `Multiplicator.Text` references].

### 3.3 Behaviour

| Gate     | Status                                                                                                         |
| -------- | -------------------------------------------------------------------------------------------------------------- |
| Multiply | Count scaling: **[Unknown]** logic; the name suggests multiplying ball count.                                  |
| Subtract | Consumes balls until `startValue` runs out: **[Unknown]** logic; only `startValue` and counters are recovered. |
| Divide   | About one ball passes per value: **[Unknown]**. The old "alternating counter" is not recoverable.              |

### 3.4 Re-trigger guard

`BallsController._ignoredMultiplicatorsByBalls : Dictionary<BallView, List<Collider>>` [Recovered]. [Inferred]: a ball records the gates it has passed. No timed-cooldown field exists on gates or in `BallsConfig` [Recovered].

### 3.5 Moving gates

`GateBase.isMoveX` and `GateBase.MoveTime` [Recovered]. Moving gates (`isMoveX: 1`) appear only in boss prefabs: all 13 `BossLevel1`–`BossLevel13.prefab` have them, with `MoveTime` between 1 and 5 [Recovered; count verified]. Only `BossLevel1`–`BossLevel5` are referenced by the level table; 6–13 are unused in this build.

### 3.6 Procedural generator

`MazePathGeneratorConfig` [Recovered]. Used only by `LevelType.Json`; current table rows are `Prefab`.

| Gate          | Spawn chance | Count  | Values            |
| ------------- | ------------ | ------ | ----------------- |
| sub           | 0.2          | 1 to 2 | 10 to 50, step 10 |
| divide        | 0.1          | 1 to 2 | 1                 |
| multiplicator | 0.7          | 1 to 5 | 2 to 8, step 1    |

---

## 4. Locks and Chains

- `Chain`: `UnlockNumber` (int), `CounterText`, `counterBG`, `P_Lock`, `ChainCount` [Recovered: `Assets/Scripts/Assembly-CSharp/Chain.cs`].
- `ChainTrigger` (tag `Ball`), `ChainSegment` [Recovered].
- Cost is set per level prefab. `Level2` has `UnlockNumber = 100` [Recovered]. 38 level prefabs used by the table have a lock (41 in the folder; the other 3 are the unused `BossLevel11`–`13`). Costs: 30, 40, 45, 50, 60, 80, 100, 120, plus 15 on the non-live `Level_Segmented2_Intro_3_2.8` [Recovered: prefab scan].
- Feeding, break and release behaviour: **[Unknown]**.

---

## 5. Bucket

- `Bucket` fields: `CurrentFillCount`, `DestroyTrigger`, `FillTrigger`, `bucketCollectFX` [Recovered].
- Tags: `BucketFillTrigger`, `BucketDestroyTrigger`, `BucketShakeTrigger` [Recovered].
- Methods: `OnFill(filledBefore, amount)`, `OnOut()`, `Shake()`, `OverFillCheck()` [Recovered signatures].
- Fill thresholds, overflow and shake timing: **[Unknown]**.

---

## 6. Camera Follow

`CameraMove` on Main Camera [Recovered: `Scenes/Main.unity`]: Damping 2, ZoomPadding 7.25, MinZoomSize 3, MaxZoomSize 15, BossLevelBucketOffset 6.17, yOffset 0, yOffsetDefault 0.

Base transform from `LevelsConfig`: local position (0, -0.4, -16.93), pitch 3 degrees [Recovered]. Follow and zoom behaviour: **[Unknown]**.

---

## 7. Levels and goals

### 7.1 Level table

- 107 rows. Each names one prefab `Resources/prefabs/minilevels/<Level Name>.prefab` [Recovered].
- Start ball amounts: 5 (102 rows), 10 (2), 20 (1), 7 (1), 100 (1) [Recovered].
- Targets up to 1600; perfect up to 2880 on the live list (`Level19`, `Level49`) and 5000 in the full table (`Level_Onboarding1_2.8`, not live) [Recovered].
- Boss rows: 6, covering `BossLevel1` to `BossLevel5` (`BossLevel1` appears twice) [Recovered].

### 7.2 Goal percentages

`LevelsConfig` [Recovered]: `ballGoalPercentTarget` 0.25, `ballGoalPercentPerfect` 0.75.

- How the percentages relate to the table's Target and Perfect columns: **[Unknown]**.

### 7.3 Win, fail and rewards

Victory and defeat conditions, settle time, timeouts, and coin or gem rewards: **[Unknown]**. The economy is in doc 06.

---

## 8. Open Design Questions

1. Closed. `GameManager` (3 / 20) governs live play, because `RotateController.Start` copies it over the prefab values (3.5 / 16) on all 115 levels, as in the original (decompile `RotateController_Start`).
2. Swipe-to-angular-velocity mapping and smoothing: design fresh, or match a feel target measured in PlayMode.
3. Does ball friction need a kinematic maze body, given no Rigidbody on the rotators?
4. Multiply, subtract and divide gate behaviour, including ordering.
5. Tier override maths for `chancesToOverrideTier`.
6. Layer escalation: confirm that balls on layers 9, 10 and 11 pass through each other.
7. Is a per-ball ignored-gate list enough, or is a timed guard needed?
8. Units of `inMazeMaxSpeed` and `outMazeMaxSpeed`.
9. Mapping of `ballGoalPercent*` to Target and Perfect ball amounts.
10. Win and fail conditions, settle time, defeat timeout.
11. Lock feeding and break rules.
12. Bucket fill, overflow and shake behaviour.
13. Which A/B cohort applies to level order (`ABTestVariables` is remote).
14. Ball counts above 1000 and the live pool cap: measure on the test device during parity QA (`08_IMPLEMENTATION_PLAN.md`).
