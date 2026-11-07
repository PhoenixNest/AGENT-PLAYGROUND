# 06. Currency, Shop, IAP, Ads and Save Data

**Status:** Draft rebuild spec for *Multi Maze 3D* (our own title). Internal rebuild from the recovered v4.1.0.0 build, which is the source of truth.
**Tags:** `[Recovered]` = read from the recovered build (path cited). `[Inferred]` = reasoned from recovered names, not confirmed. `[Unknown]` = not in the export; needs the publisher or the remote-config backend.
**Sources:** `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/Assets/`; `games/clone-bucket/docs/`; `tools/recovery/FINDINGS.md`.
**Secrets:** API keys, ad unit IDs, app IDs and tokens exist in the build but are intentionally not recorded here.

---

## 1. Currency

| Item | Tag | Value / evidence |
|---|---|---|
| Premium currency name | [Recovered] | Field `GemCounter` on the game manager component (`Scenes/Main.unity`, line 112084 reference, line 65816 default `0`). Related event/field `GemCollect`. |
| Coin naming in older docs | [Recovered] | Doc drafts call the currency "coins". The build uses **gems**. See open question Q1. |
| Gems earned per level | [Unknown] | No per-level reward value is serialized. |
| Rewarded-video coin bonus | [Inferred] | Method signature `DelayedNextLevel(withCoinsRV)` implies a rewarded-video bonus on level completion. Amount unknown. |
| Gem spend rules | [Unknown] | No cost field besides the shop price below. |

**Design note:** do not hard-code a per-level reward. Expose it as a tunable value once the team confirms the source (Q2).

---

## 2. Shop (skins)

### 2.1 Three shop config variants

All three are `MonoBehaviour` assets with fields `price`, `totalSkinCount`, `balls[]`. Each `balls[]` entry has `Name`, `materials[]` (6 per skin, except: the last skin in each variant has 7, adding `Ball06_05`; `ShopConfig_2` Ball1 has 3), `icon` and `mazeIcon`.

| Asset | `price` | `totalSkinCount` | Skins listed | Tag |
|---|---|---|---|---|
| `MonoBehaviour/ShopConfig.asset` | 500 | 6 | Ball1–Ball6 | [Recovered] |
| `MonoBehaviour/ShopConfig_1.asset` | 500 | 7 | Ball1–Ball7 | [Recovered] |
| `MonoBehaviour/ShopConfig_2.asset` | 500 | 7 | Ball1–Ball7 (Ball1 differs) | [Recovered] |

- All three are referenced from one `_configs` list in `Scenes/Main.unity` (lines 66292–66294 and 92126–92128), in the order ShopConfig, _1, _2.
- **Which variant is live** is [Unknown]. It is either an A/B cohort, a staged rollout, or a version list. Selection logic is in code, which is not recoverable from this export.

### 2.2 Skin → material family

Each skin's `materials[]` points at one texture family in `Material/`. Names are as-recovered.

| Skin | ShopConfig (v1 set) | ShopConfig_1 | ShopConfig_2 |
|---|---|---|---|
| Ball1 | `Ball04_01`–`_06` | `Ball_SoftBlue 3/4/6`, `Ball_Colorful_Green 1`, `Ball_Colorful_Red`, `Ball_Flesh_3` | `Ball_SoftBlue 1/2/5` |
| Ball2 | `Ball06_01`–`_06` | `Ball04_01`–`_06` | `Ball04_01`–`_06` |
| Ball3 | `Ball01_01`–`_06` | `Ball06_01`–`_06` | `Ball06_01`–`_06` |
| Ball4 | `Ball02_01`–`_06` | `Ball01_01`–`_06` | `Ball01_01`–`_06` |
| Ball5 | `Ball03_01`–`_06` | `Ball02_01`–`_06` | `Ball02_01`–`_06` |
| Ball6 | `Ball05_01`–`_06` + `Ball06_05` | `Ball03_01`–`_06` | `Ball03_01`–`_06` |
| Ball7 | — | `Ball05_01`–`_06` + `Ball06_05` | `Ball05_01`–`_06` + `Ball06_05` |

Icons: each skin has `icon` (shop card) and `mazeIcon` (in-maze preview) sprites, referenced by GUID in the same asset.

### 2.3 Unlock mechanic

| Question | Tag | Notes |
|---|---|---|
| Is a skin bought with gems at 500? | [Inferred] | `price: 500` exists; the currency it spends is not recorded. |
| Random (gacha) or player choice? | [Unknown] | No flag or pool field is serialized. |
| Watch-ad unlock option? | [Unknown] | No ad-gate field in shop configs. |
| Skin equipped state, where stored? | [Unknown] | Save layer not verified (see §6). |

---

## 3. In-app purchases (remote)

| Item | Tag | Value |
|---|---|---|
| Source | [Recovered] | `MonoBehaviour/DataSetConfig_IAPs.asset`: `remoteFileName: IAPsData.txt`, `remoteFeatureName: IAPs`. The product table is fetched remotely. |
| Local copy | [Recovered] | `TextAsset/IAPsData.txt` is a header row only. |
| Header columns | [Recovered] | `Id`, `UnlockAtLevel`, `DisplayName`, `DisplayDescription`, `PriceInUsd`, `DiscountPercentAmount`, `IsOffer`, `OfferWindowId`, `OfferPopupTriggerEvent`, `OfferPopupTriggerLimitInMinutes` |
| Product identifiers | [Recovered] (names only) | `RemoveAds` and `NoAds` appear in `Scenes/Main.unity`. The offer icon is `Sprite/offer_noads_icon`. |
| Product IDs, types, prices | [Unknown] | Not in the local copy. Needs the remote file. |
| Offer queue | [Recovered] | `InAppPurchasesConfig.offerQueueCapacity` = 3. |
| Storage format | [Recovered] | Serialized keys match the header; `_separatorType` 1 = `Tsv` (tab-separated, enum `EDataSetSeparatorType`). The levels table uses 0 = `Csv`. |

---

## 4. Ads

| Item | Tag | Value |
|---|---|---|
| Ad surfaces present | [Recovered] | Prefabs under `Resources/prefabs/`: `rewardedAd`, `interstitial`, `bannerImage`, `BannerCanvas`, `BannersBackground`, `TTPRewardedAdsCanvas`, `TTPInterstitialsCanvas`, `TTPPromotionCanvas`, `TTPOpenAdsLoadingScreenCanvas`. |
| Config asset | [Recovered] | `MonoBehaviour/AdsConfig.asset`: `debugSkipInterstitials: 0`. Wired by `AdsInstaller`. |
| Placement → screen mapping | [Unknown] | Not serialized in these assets. |
| Frequency, cooldowns, level gating | [Unknown] | Not serialized in these assets. |
| Remote control | [Inferred] | TTP SDK remote config is present; ad rules may be remote. |
| Ad IDs, keys | — | Exist in the build; not recorded here. |

---

## 5. Colour schemes

Five `ColorSchemeConfig` assets. Hex values are read from the wall and bucket materials (`_Color`). Balls use no scheme material (`ballsMaterials` is empty in all five).

| Scheme (`id`) | Asset | Maze walls | Bucket | Skybox ambient | Tag |
|---|---|---|---|---|---|
| Grey (default) | `ColorSchemeConfig_Grey` | `#7B7B7B` | `#3185FF` | on | [Recovered] |
| Black | `ColorSchemeConfig_Black` | `#666C7A` | `#3185FF` | off | [Recovered] |
| Blue | `ColorSchemeConfig_Blue2` | `#5391FF` | `#3185FF` | on | [Recovered] |
| Blue_Old | `ColorSchemeConfig_Blue` | `#6B7599` | `#3084FE` | off | [Recovered] |
| Orange | `ColorSchemeConfig_Orange` | `#F6B979` | `#FFBD79` | off | [Recovered] |

- `ColorSchemesConfig` lists the five schemes. `ColorSchemeInstaller` references it.
- In the level table, all seven scheme columns are `Grey` in all 107 rows [Recovered: full-table check].
- Background and stage colours exist per scheme (`mazeBackgroundMaterials`, `stageBackgroundMaterials`) and are not tabulated here.

---

## 6. Save data

| Item | Tag | Value |
|---|---|---|
| `PermanentSaveConfig` | [Recovered] values; purpose [Inferred] | Likely a debug save tool. `maxNameLenght` = 20 (key is misspelled in the build). `saveData` and `preMadeSaveHolders` are empty. Not a production save schema. |
| `DatabaseConfig` | [Recovered] | `mainDataSetId: LevelData`. Three datasets: Levels, IAPs, Camera. |
| `DataSetConfig_Levels` | [Recovered] | Remote `LevelsData.csv`, feature `LevelFunnel`. Local bundle `TextAsset/LevelsData.txt` (107 rows). |
| Player progress keys (gems, unlocked levels, owned skins, equipped skin) | [Unknown] | Not serialized in configs. Storage backend not verified. |
| Reset rules | [Unknown] | — |

**Design rule for the rebuild:** store gems, owned skins and equipped skin in one versioned save object. Keep it separate from the debug save tool.

---

## 7. Related tunables (reference only)

| Asset | Fields | Tag |
|---|---|---|
| `TimeMachineConfig` | `smallEffectorCoefficient` 0.1, `bigEffectorCoefficient` 1 | [Recovered] (used in `Scenes/Loading.unity`) |
| `BallsConfig` / `BallsConfigIos` | Pool and tier values | See `02_GAMEPLAY_MECHANICS_SPEC.md` and `04_PHYSICS_AND_RENDERING_SPEC.md`. |

---

## 8. Open questions for the publisher / team

- **Q1.** Is the premium currency **gems** (build) or **coins** (old docs)? Confirm the player-facing name.
- **Q2.** Source of the gem reward per level: fixed, tiered, or remote?
- **Q3.** Which shop variant (ShopConfig, _1, _2) is live, and what drives the choice (A/B, rollout, version)?
- **Q4.** Skin unlock: random pool of 500-gem purchases, or player choice? Is there a watch-ad alternative?
- **Q5.** Live product IDs, types and prices from `IAPsData.txt` (remote). Which products are consumable vs non-consumable?
- **Q6.** Ad placements, frequencies, cooldowns and level gating. Are these remote?
- **Q7.** Save backend (local, cloud, or SDK) and reset rules.
- **Q8.** Equipped-skin and owned-skin storage keys.
- **Q9.** Are the five colour schemes all player-selectable, or are some level-gated?

---

## 9. Acceptance (rebuild)

- Shop shows the agreed skin count, each with price, state (owned / equipped / locked) and preview.
- Purchases and ad rewards write to the same save object.
- No product ID, ad unit ID or key is committed in plain text in the repo.
- Every `[Unknown]` above has an owner in Q1–Q9.
