# 10. Project Review — 2026-09-30

**Repository audit, Unity/web correctness review, and same-day fix pass**

- **Project:** _Orbital Salvage: Space Trash Clean_ (maze-puzzle)
- **Review Date:** 2026-09-30
- **Reviewer:** Lead reviewer with parallel reader agents
- **Document Version:** 1.0.1 (docs re-verification after parallel agent updates)
- **Purpose:** Record an independent repository and project review of the web prototype and Unity port, reconcile overstated parity claims, list findings with verified working-tree status, and recommend prioritized next steps.

**2026-10-04 note.** This review describes the uGUI port as it stood on 2026-09-30. That shell (`UIManager` and the Physics2D scene) has been removed. The game that launches is the Canvas2D `AppController`. The scores and "uncommitted fixes" below are not the current tree. Current harness numbers are in `09` and `tools/parity/README.md`. Strict still parity is not finished, and nothing here was checked on a device.

- **Companion Documents:**
  - [`01_REFERENCE_GAME_ANALYSIS.md`](./01_REFERENCE_GAME_ANALYSIS.md)
  - [`02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md`](./02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md)
  - [`03_GAME_DESIGN_DOCUMENT.md`](./03_GAME_DESIGN_DOCUMENT.md)
  - [`04_TECHNICAL_ARCHITECTURE.md`](./04_TECHNICAL_ARCHITECTURE.md)
  - [`05_GAME_UI_UX_DESIGN_SPECIFICATION.md`](./05_GAME_UI_UX_DESIGN_SPECIFICATION.md)
  - [`06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN.md`](./06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN.md)
  - [`07_AUDIO_DESIGN_SPECIFICATION.md`](./07_AUDIO_DESIGN_SPECIFICATION.md)
  - [`08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md`](./08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md)
  - [`09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md`](./09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md)
  - [`CHANGELOG.md`](./CHANGELOG.md)
  - [`README.md`](./README.md)

---

## 1. Executive Summary

The web prototype is a working game. The Unity port was a strong menu shell with a partial gameplay port. Project documentation overstated parity (including “100% 1:1” language), and iOS deployment is not configured.

A same-day fix pass resolved the Unity runtime blockers and web bugs listed below. Those fixes are currently uncommitted in the working tree. Unity `6000.4.6f1` batchmode compile is clean (0 errors, 0 warnings), and all 13 editor screenshot captures succeed with no visual regressions (max 2.89% pixel diff, all from intended label/button changes).

Honest visual parity scores remain well below full parity, especially gameplay (4.5/10). Worlds 3–5 mechanics and a physics-approach decision are still open. Docs `08`, `09`, `CHANGELOG.md`, and `README.md` were corrected in the working tree (honest scores, relative Unity screenshot links, current-status section, Unreleased / 0.10.0 changelog, README index of `09`/`10`). Cross-doc conflicts in `03`–`07` remain open.

---

## 2. Scope & Method

| Area          | Coverage                                                                                                                                  |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Git history   | All branches; stash inspection (no file contents listed here)                                                                             |
| Documentation | Docs `01`–`09`, plus [`README.md`](./README.md) and [`CHANGELOG.md`](./CHANGELOG.md)                                                      |
| Screenshots   | 26 total: 13 web at 1252×1528 (incl. device frame), 13 Unity at 860×1864 (= 2× 430×932)                                                   |
| Unity C#      | 19 scripts, ~7k lines under [`./unity/maze-puzzle/`](./unity/maze-puzzle/)                                                                |
| Web prototype | 8 files, ~9k lines under [`./web/`](./web/)                                                                                               |
| Tooling       | Unity MCP / Cursor MCP configs; batchmode compile                                                                                         |
| Verification  | Claims independently re-verified against source; Working-tree status column checked via `git diff HEAD` before marking items **Resolved** |

Status values used below:

- **Resolved in working tree** — fix present in uncommitted changes (spot-checked)
- **Open** — still outstanding
- **Needs owner decision** — blocked on product/owner choice

---

## 3. Findings

### 3.1 Repository & git

| Severity | Status               | Finding                                                                                                                                                                                                                         |
| -------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| High     | Needs owner decision | A git stash (`stash@{0}`, “WIP on office-work”) holds ~216 office documents (~187 MB uncompressed) unrelated to this project. It accounts for nearly all of the ~150 MB `.git` size and should be moved out of git and dropped. |
| Medium   | Open                 | Commit `c9487e5` added 423 files / +153k lines, mostly the full TextMesh Pro “Examples & Extras” (~285 files), which the game does not need.                                                                                    |
| Low      | Open                 | No Git LFS / `.gitattributes` despite binaries. Root `.gitignore` lacks `.venv` / `__pycache__` (currently only covered by a nested ignore under `mcp-servers/blender_mcp`).                                                    |
| Low      | Open                 | Commit scopes drift (`maze-puzzle` / `unity` / “ui, maps” / “mobile, physics, gameplay”). AGB git-workflow expects `<type>: <description>` messages and `agent/<role>/<task>` branches.                                         |

**Positives**

- No secrets found in history.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`, and `.venv` were never committed.
- All 550 Unity assets have `.meta` files.

### 3.2 Unity port

| Severity | Status                   | Finding                                                                                                                                                                                                                                                                                                                   |
| -------- | ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| —        | Resolved in working tree | Victory/defeat modals never opened at runtime (`SetState` did not notify UI). Fixed via `GameManager.OnStateChanged` plus `UIManager` subscription, with real stats in modals.                                                                                                                                            |
| —        | Resolved in working tree | No defeat path (`TriggerDefeat` had no callers). Fixed with spill detection via camera frustum bounds, an active-debris registry, defeat when no active debris and quota unmet, 1.5 s grace at sector start, and a Backup Pod (+8) button when ≥70% quota and unused.                                                     |
| —        | Resolved in working tree | Emoji sprites loaded via `File.ReadAllBytes` from `Application.dataPath` (would show circles on device). Moved to `Assets/Resources/Emojis/3D` (GUIDs preserved) and loaded with `Resources.Load<Sprite>`.                                                                                                                |
| —        | Resolved in working tree | Safe area discarded by UI rebuild. Fixed with `SafeContent` containers and `SafeAreaFitter`, keeping full-bleed backgrounds. Still needs verification on a notched device or simulator.                                                                                                                                   |
| —        | Resolved in working tree | HUD quota bar never filled (`fillAmount` on a Sliced image). Switched to anchor-driven fill.                                                                                                                                                                                                                              |
| —        | Resolved in working tree | Star ratings lost on save (`Dictionary` + `JsonUtility`). Replaced with a serializable list plus a runtime lookup.                                                                                                                                                                                                        |
| —        | Resolved in working tree | Header coin labels stuck at 250; they now update only when the value changes. Shop unlock labels no longer truncated.                                                                                                                                                                                                     |
| High     | Open                     | Worlds 3–5 mechanics in `sectors_50.json` (concentric rings / `gearRatio`, track walls, deflector pins, one-way flaps) are not built at runtime. DTO lacks `gearRatio` / `trackWalls` / `isFinale`; `FlapData.openDir` vs JSON `normalSign`; `PinData` lacks `radius`; “magnetic” debris maps to titanium.                |
| High     | Needs owner decision     | Physics is a Unity Physics2D reimplementation, not a port of [`./web/engine/physics.js`](./web/engine/physics.js) (no centrifugal term, no debris cap 320 / weight merge, no volatile detonation; max ω 25 vs 12, ratchet 15° vs π/16, RCS 4.0 vs 3.8). Decide: port the web integrator, or accept Physics2D and re-tune. |
| Medium   | Open                     | No debris cap or pooling (gate clones unbounded); per-frame `OverlapCircleAll` (`NibblesController`) and `FindObjectsByType` (`MascotEyeTracker`, whose eye references are unassigned in the scene); ring walls are `EdgeCollider2D` on a rotating transform with no kinematic `Rigidbody2D`.                             |
| Medium   | Open                     | Economy differs from web (start 150 vs 250; no cosmetics catalog / achievements / daily tiers in `EconomyManager`); shop purchase / sound / settings handlers are stubs; maps world tabs have no click handlers.                                                                                                          |
| Medium   | Open                     | iOS not configured — `applicationIdentifier` empty, iOS target version empty, `productName` `maze-puzzle`; Gamma color space vs plan’s Linear; URP / DOTween / Input System not installed; migration Phase 7 not started.                                                                                                 |
| —        | Known limitation         | Debris permanently stuck in the maze keeps the level running (same as web); the player can restart from the HUD.                                                                                                                                                                                                          |
| Low      | Open                     | `UIManager` (~2k lines) and `SpriteHelper` (~2k lines) are god classes; no `.asmdef`; no tests / Unity Test Framework; `StandaloneInputModule`; Fluent Emoji (MIT) has no attribution / `LICENSE` file next to the sprites.                                                                                               |

### 3.3 Web prototype

| Severity   | Status                   | Finding                                                                                                                                                                                                                                                                                                                                                               |
| ---------- | ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| —          | Resolved in working tree | `AudioEngine.playRcsThrust` was missing, so every BOOST gate hit threw and aborted that frame’s physics update. Implemented in [`./web/engine/audio.js`](./web/engine/audio.js).                                                                                                                                                                                      |
| —          | Resolved in working tree | `AudioContext` never resumed (iOS silence). Added `ensureRunning()` on init and play.                                                                                                                                                                                                                                                                                 |
| —          | Resolved / Changed       | Campaign levels had no `rewardCredits` (always 250). Now `250 + (level - 1) * 5` (L1 = 250, L50 = 495). This is a balance choice the designer should confirm; docs define no campaign reward table.                                                                                                                                                                   |
| —          | Resolved in working tree | SFX volume slider now applied. Ambience slider persists, but there are no ambience sounds yet. Escape / Hangar / `loadSector` reset pause and modals via `resetGameplaySessionUi()`.                                                                                                                                                                                  |
| Low/Medium | Open                     | Debris drag `0.994` per frame (frame-rate dependent); bumper restitution >1 adds energy; all 50 levels unlocked from the start; daily rewards keyed to local date (clock-change farming); Google Fonts network dependency vs offline claim; sparse accessibility (no reduced-motion, no modal focus management, `user-scalable=no`); hardcoded spill bound `x > 500`. |

### 3.4 Documentation

| Severity | Status                   | Finding                                                                                                                                                                                                                                                                                                                                                        |
| -------- | ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| —        | Resolved in working tree | [`09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md`](./09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md): no “100% 1:1” claims; relative Unity + web screenshot links; honest per-screen scores (6–8 range; gameplay 4.5).                                                                                                                                           |
| —        | Resolved in working tree | [`08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md`](./08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md): added **Current status (2026-09-30)** phase table; emoji/project paths corrected to `Assets/Resources/Emojis/3D/` and `./unity/maze-puzzle/`.                                                                                                              |
| —        | Resolved in working tree | [`CHANGELOG.md`](./CHANGELOG.md): `[Unreleased]` covers same-day Unity/web/tooling/docs fixes; `[0.10.0]` dated 2026-09-30 with hashes `c7ba2f5` / `c9487e5` / `3c0a048`.                                                                                                                                                                                      |
| —        | Resolved in working tree | [`README.md`](./README.md): indexes docs `09` and `10`.                                                                                                                                                                                                                                                                                                        |
| Medium   | Open                     | Cross-doc conflicts remain in `03`–`07`: audio 12 vs 15 cues and 16- vs 24-bit; debris masses in `06` vs code; shop 4 tabs in `05` vs 3 in product; `04` still describes a Capacitor/Cordova web deployment; version stamps (`07` “v1.4.0”, `06` “0.6.0”); `06` references a missing `scratch/audit_clean.mjs`; many absolute `file:///` links remain in `07`. |
| —        | Open                     | Guideline 4.3: zero ads/IAP and the Nibbles / cosmic-janitor theme are real differentiators, but the core loop (rotate ring → ×N gates → collector) is the saturated Multi Maze 3D archetype. The GDD’s “100% immunity to 4.3” is overconfident.                                                                                                               |

### 3.5 Screenshots / visual parity

Per-screen scores from the review (aligned with the updated table in doc `09`):

| #     | Screen   | Score | Main gap                                                    |
| ----- | -------- | ----- | ----------------------------------------------------------- |
| 01    | Home     | 6     | Diorama decor / ring / badge colour                         |
| 02    | Maps     | 7.5   | Layout close; polish gaps remain                            |
| 03    | Daily    | 6.5   | Content density and art differ                              |
| 04–06 | Shop     | ~7    | Item art is colour-dot placeholders; icon set inconsistency |
| 07    | Trophies | 8     | Strong shell; content polish                                |
| 08    | Settings | 7.5   | Icons flat vs web                                           |
| 09    | Gameplay | 4.5   | Sector 1 geometry, gate art, and FX differ from web         |
| 10    | Paused   | 7     | Modal backdrops dimmed not blurred                          |
| 11    | Victory  | 7.5   | Structure matches; celebration FX lighter                   |
| 12    | Defeat   | 7.5   | Structure matches; recovery UX now wired in working tree    |
| 13    | Loading  | 8     | Closest match among shells                                  |

Shared gaps: modal backdrops are dimmed rather than blurred; settings icons are flat; shop item art remains placeholder colour dots; icon set is inconsistent across screens.

### 3.6 Tooling

| Severity | Status                   | Finding                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| -------- | ------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| —        | Resolved in working tree | Unity MCP was configured only under `.agents/` (unpinned) and the Unity project lacked the package. MCP for Unity 10.2.0 is now pinned: `com.coplaydev.unity-mcp#v10.2.0` in [`./unity/maze-puzzle/Packages/manifest.json`](./unity/maze-puzzle/Packages/manifest.json), UnityMCP (`uvx mcpforunityserver==10.2.0`, stdio) in [`.cursor/mcp.json`](../../.cursor/mcp.json), and `.agents` configs pinned. To use: open the project in Unity `6000.4.6f1`, confirm the MCP for Unity bridge is running (**Window → MCP for Unity**), then reload MCP servers in Cursor. |
| Medium   | Open                     | `mcp-servers/blender_mcp` is the official Blender Lab server (GPL-3.0-or-later), vendored with a ~15 MB tracked RST docs corpus, unused by the game. It executes arbitrary Python over an unauthenticated localhost socket (port 9876) — keep it bound to localhost. No `LICENSE` file in the vendored tree.                                                                                                                                                                                                                                                           |

---

## 4. Recommended Next Steps (prioritized)

1. **Owner:** handle the unrelated office stash (`stash@{0}`) — move the documents out of git and drop the stash so `.git` size returns to a sane baseline.
2. **Commit** the working-tree fixes; prefer a split such as `unity-fixes`, `web-fixes`, `tooling`, and `docs`.
3. **Verify** safe area and emoji sprites on an iOS device or simulator build.
4. **Configure** iOS PlayerSettings (`applicationIdentifier`, target version, product name, color space decision).
5. **Decide** the physics approach (port web integrator vs accept/retune Physics2D), then implement or explicitly scope out worlds 3–5.
6. **Add** a debris cap and pooling for gate clones.
7. **Remove** TextMesh Pro Examples & Extras from the project.
8. **Reconcile** remaining cross-doc conflicts in `03`–`07` (audio cue counts/bit depth, debris masses, shop tabs, Capacitor/Cordova wording, version stamps, missing `scratch/audit_clean.mjs`, absolute `file:///` links in `07`).
9. **Add** Unity Test Framework coverage for JSON parsing, gate math, and economy persistence.

---

## 5. Verification Record

| Check                             | Result                                                                                                                                                                                                                       |
| --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unity batchmode compile           | Exit 0; 0 CS errors / 0 warnings (including with MCP for Unity 10.2.0 resolved)                                                                                                                                              |
| Screenshot capture                | 13/13 succeeded; max pixel diff 2.89% (intended label/button changes only)                                                                                                                                                   |
| Web syntax / smoke                | `node --check` pass; HTTP 200 smoke test on all modules                                                                                                                                                                      |
| Working-tree Resolved claims      | Spot-checked against `git diff HEAD` for GameManager, UIManager, EconomyManager, DebrisPiece, SpriteHelper, SafeAreaFitter, web audio/maze/main, Packages/manifest, `.cursor/mcp.json`                                       |
| Docs 08 / 09 / CHANGELOG / README | Re-verified 2026-09-30 after parallel agent update: no “100% 1:1” in `09`; relative Unity screenshots + scores; `08` Current status section; `CHANGELOG` Unreleased + 0.10.0 hashes; README indexes `09`/`10` → **Resolved** |

**Not verified in this review**

- No interactive Play Mode session
- No device build (iOS simulator or hardware)

---

## 6. Working-tree snapshot (review day)

Uncommitted areas that back the **Resolved** rows above include:

- Unity scripts: [`./unity/maze-puzzle/Assets/Scripts/Core/GameManager.cs`](./unity/maze-puzzle/Assets/Scripts/Core/GameManager.cs), [`EconomyManager.cs`](./unity/maze-puzzle/Assets/Scripts/Core/EconomyManager.cs), [`UIManager.cs`](./unity/maze-puzzle/Assets/Scripts/UI/UIManager.cs), [`SafeAreaFitter.cs`](./unity/maze-puzzle/Assets/Scripts/UI/SafeAreaFitter.cs), [`DebrisPiece.cs`](./unity/maze-puzzle/Assets/Scripts/Physics/DebrisPiece.cs), [`SpriteHelper.cs`](./unity/maze-puzzle/Assets/Scripts/Utils/SpriteHelper.cs)
- Emoji move: `Assets/Sprites/Emojis/3D` → `Assets/Resources/Emojis/3D`
- Web: [`./web/engine/audio.js`](./web/engine/audio.js), [`./web/engine/maze.js`](./web/engine/maze.js), [`./web/main.js`](./web/main.js)
- Tooling: [`./unity/maze-puzzle/Packages/manifest.json`](./unity/maze-puzzle/Packages/manifest.json), [`.cursor/mcp.json`](../../.cursor/mcp.json), `.agents` Unity MCP pins
- Docs: [`./08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md`](./08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md), [`./09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md`](./09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md), [`./CHANGELOG.md`](./CHANGELOG.md), [`./README.md`](./README.md)
