# 06. Currency, Shop and Save Data

**Status:** Draft rebuild spec for _clone-bucket_. Internal rebuild from the recovered v4.1.0.0 build of our earlier title, which is the source of truth.
**Tags:** `[Recovered]` = read from the recovered build (path cited). `[Inferred]` = reasoned from recovered names, not confirmed. `[Unknown]` = not in the export; needs the publisher or the remote-config backend.
**Sources:** `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/Assets/`; `games/clone-bucket/docs/`; `tools/recovery/FINDINGS.md`.

---

## 1. Currency

| Item                      | Tag         | Value / evidence                                                                                                                                         |
| ------------------------- | ----------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Premium currency name     | [Recovered] | Field `GemCounter` on the game manager component (`Scenes/Main.unity`, line 112084 reference, line 65816 default `0`). Related event/field `GemCollect`. |
| Coin naming in older docs | [Recovered] | Doc drafts call the currency "coins". The build uses **gems**. See open question Q1.                                                                     |
| Gems earned per level     | [Unknown]   | No per-level reward value is serialized.                                                                                                                 |
| Gem spend rules           | [Unknown]   | No cost field besides the shop price below.                                                                                                              |

**Design note:** do not hard-code a per-level reward. Expose it as a tunable value once the team confirms the source (Q2).

---

## 2. Shop (skins)

### 2.1 Three shop config variants

All three are `MonoBehaviour` assets with fields `price`, `totalSkinCount`, `balls[]`. Each `balls[]` entry has `Name`, `materials[]` (6 per skin, except: the last skin in each variant has 7, adding `Ball06_05`; `ShopConfig_2` Ball1 has 3), `icon` and `mazeIcon`.

| Asset                              | `price` | `totalSkinCount` | Skins listed                | Tag         |
| ---------------------------------- | ------- | ---------------- | --------------------------- | ----------- |
| `MonoBehaviour/ShopConfig.asset`   | 500     | 6                | Ball1–Ball6                 | [Recovered] |
| `MonoBehaviour/ShopConfig_1.asset` | 500     | 7                | Ball1–Ball7                 | [Recovered] |
| `MonoBehaviour/ShopConfig_2.asset` | 500     | 7                | Ball1–Ball7 (Ball1 differs) | [Recovered] |

- All three are referenced from one `_configs` list in `Scenes/Main.unity` (lines 66292–66294 and 92126–92128), in the order ShopConfig, _1, _2.
- **Which variant is live** is [Unknown]. It is either an A/B cohort, a staged rollout, or a version list. Selection logic is in code, which is not recoverable from this export.

### 2.2 Skin → material family

Each skin's `materials[]` points at one texture family in `Material/`. Names are as-recovered.

| Skin  | ShopConfig (v1 set)             | ShopConfig_1                                                                        | ShopConfig_2                    |
| ----- | ------------------------------- | ----------------------------------------------------------------------------------- | ------------------------------- |
| Ball1 | `Ball04_01`–`_06`               | `Ball_SoftBlue 3/4/6`, `Ball_Colorful_Green 1`, `Ball_Colorful_Red`, `Ball_Flesh_3` | `Ball_SoftBlue 1/2/5`           |
| Ball2 | `Ball06_01`–`_06`               | `Ball04_01`–`_06`                                                                   | `Ball04_01`–`_06`               |
| Ball3 | `Ball01_01`–`_06`               | `Ball06_01`–`_06`                                                                   | `Ball06_01`–`_06`               |
| Ball4 | `Ball02_01`–`_06`               | `Ball01_01`–`_06`                                                                   | `Ball01_01`–`_06`               |
| Ball5 | `Ball03_01`–`_06`               | `Ball02_01`–`_06`                                                                   | `Ball02_01`–`_06`               |
| Ball6 | `Ball05_01`–`_06` + `Ball06_05` | `Ball03_01`–`_06`                                                                   | `Ball03_01`–`_06`               |
| Ball7 | —                               | `Ball05_01`–`_06` + `Ball06_05`                                                     | `Ball05_01`–`_06` + `Ball06_05` |

Icons: each skin has `icon` (shop card) and `mazeIcon` (in-maze preview) sprites, referenced by GUID in the same asset.

### 2.3 Unlock mechanic

| Question                           | Tag        | Notes                                                        |
| ---------------------------------- | ---------- | ------------------------------------------------------------ |
| Is a skin bought with gems at 500? | [Inferred] | `price: 500` exists; the currency it spends is not recorded. |
| Random (gacha) or player choice?   | [Unknown]  | No flag or pool field is serialized.                         |
| Skin equipped state, where stored? | [Unknown]  | Save layer not verified (see §4).                            |

---

## 3. Colour schemes

Five `ColorSchemeConfig` assets. Hex values are read from the wall and bucket materials (`_Color`). Balls use no scheme material (`ballsMaterials` is empty in all five).

| Scheme (`id`)  | Asset                      | Maze walls | Bucket    | Skybox ambient | Tag         |
| -------------- | -------------------------- | ---------- | --------- | -------------- | ----------- |
| Grey (default) | `ColorSchemeConfig_Grey`   | `#7B7B7B`  | `#3185FF` | on             | [Recovered] |
| Black          | `ColorSchemeConfig_Black`  | `#666C7A`  | `#3185FF` | off            | [Recovered] |
| Blue           | `ColorSchemeConfig_Blue2`  | `#5391FF`  | `#3185FF` | on             | [Recovered] |
| Blue_Old       | `ColorSchemeConfig_Blue`   | `#6B7599`  | `#3084FE` | off            | [Recovered] |
| Orange         | `ColorSchemeConfig_Orange` | `#F6B979`  | `#FFBD79` | off            | [Recovered] |

- `ColorSchemesConfig` lists the five schemes. `ColorSchemeInstaller` references it.
- In the level table, all seven scheme columns are `Grey` in all 107 rows [Recovered: full-table check].
- Background and stage colours exist per scheme (`mazeBackgroundMaterials`, `stageBackgroundMaterials`) and are not tabulated here.

---

## 4. Save data

| Item                                                                     | Tag                                    | Value                                                                                                                                                                                                              |
| ------------------------------------------------------------------------ | -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `PermanentSaveConfig`                                                    | [Recovered] values; purpose [Inferred] | Likely a debug save tool. `maxNameLenght` = 20 (key is misspelled in the build). `saveData` and `preMadeSaveHolders` are empty. Not a production save schema. Removed from the rebuild with the other debug tools. |
| `DatabaseConfig`                                                         | [Recovered]                            | `mainDataSetId: LevelData`. Two datasets: Levels and Camera.                                                                                                                                                       |
| `DataSetConfig_Levels`                                                   | [Recovered]                            | Remote `LevelsData.csv`, feature `LevelFunnel`. Local bundle `TextAsset/LevelsData.txt` (107 rows).                                                                                                                |
| Player progress keys (gems, unlocked levels, owned skins, equipped skin) | [Unknown]                              | Not serialized in configs. Storage backend not verified.                                                                                                                                                           |
| Reset rules                                                              | [Unknown]                              | —                                                                                                                                                                                                                  |

**Design rule for the rebuild:** store gems, owned skins and equipped skin in one versioned save object.

---

## 5. Related tunables (reference only)

| Asset                            | Fields                                                     | Tag                                                                         |
| -------------------------------- | ---------------------------------------------------------- | --------------------------------------------------------------------------- |
| `TimeMachineConfig`              | `smallEffectorCoefficient` 0.1, `bigEffectorCoefficient` 1 | [Recovered] (used in `Scenes/Loading.unity`)                                |
| `BallsConfig` / `BallsConfigIos` | Pool and tier values                                       | See `02_GAMEPLAY_MECHANICS_SPEC.md` and `04_PHYSICS_AND_RENDERING_SPEC.md`. |

---

## 6. Open questions for the publisher / team

- **Q1.** Is the premium currency **gems** (build) or **coins** (old docs)? Confirm the player-facing name.
- **Q2.** Source of the gem reward per level: fixed, tiered, or remote?
- **Q3.** Which shop variant (ShopConfig, _1, _2) is live, and what drives the choice (A/B, rollout, version)?
- **Q4.** Skin unlock: random pool of 500-gem purchases, or player choice?
- **Q7.** Save backend (local, cloud, or SDK) and reset rules.
- **Q8.** Equipped-skin and owned-skin storage keys.
- **Q9.** Are the five colour schemes all player-selectable, or are some level-gated?

---

## 7. Acceptance (rebuild)

- Shop shows the agreed skin count, each with price, state (owned / equipped / locked) and preview.
- Skin purchases with gems write to the same save object.
- No key or account identifier is committed in plain text in the repo.
- Every `[Unknown]` above has an owner in Q1–Q9.
