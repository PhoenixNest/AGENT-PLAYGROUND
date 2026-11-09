# 08. Implementation Plan — Route C (Unity 6 + built-in render pipeline)

**Status:** Route approved; plan draft 1 (2026-11-06).
**2026-11-07:** Phases 1–5 implemented; gameplay, save format and shaders reconciled with the decompiled recovered build; 173 EditMode + 3 PlayMode tests passed.
**2026-11-08:** renamed to clone-bucket 1.0.0; ads, in-app purchases, attribution SDKs and Android support removed (see `CHANGELOG.md`). Targets are iOS, desktop (Standalone) and WebGL. 164 EditMode + 3 PlayMode tests pass. Phases 6–7 are blocked on gates G0, G3 and G4.
**Scope:** Rebuild _clone-bucket_ 1.0.0 from the recovered project (the v4.1.0.0 build of our earlier title) into a shippable build that plays the same as the recovered build.
**Tags:** `[Recovered]` facts come from the export or `tools/recovery/FINDINGS.md`. `[Decision]` marks a choice made in this plan. `[Gate]` marks something outside our code that must happen first.

---

## 1. Engine decision record

|                                     |                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Chosen**                          | **Route C**: upgrade the recovered project to Unity 6 and keep the **built-in render pipeline** (Forward, Gamma).                                                                                                                                                                                                                                                                                                             |
| **Original**                        | Unity 2022.3.62f2, built-in RP, Forward, Gamma, portrait, IL2CPP arm64 [Recovered].                                                                                                                                                                                                                                                                                                                                           |
| **Why C**                           | (1) No method bodies survived, so there is no code to port. Upgrading the engine now costs only package upgrades; upgrading later means porting every re-implemented method. (2) The renderer, shaders and colour space stay the same, so any difference from the recovered build during parity testing points to gameplay code. (3) Unity 6 matches the workspace tooling (`AGENTS.md` §1 build profiles, Unity MCP skills). |
| **Rejected: A** (stay on 2022.3)    | Past its standard support window, so a later forced migration would hit a finished codebase. Also conflicts with the Unity 6 workspace tooling.                                                                                                                                                                                                                                                                               |
| **Rejected: B** (Unity 6 + URP now) | +30–50 % effort. It changes rendering while gameplay is still unverified, so parity bugs would have two possible causes. URP remains available as Phase 8.                                                                                                                                                                                                                                                                    |
| **Fallback**                        | If gate G0 finds that the CrazyLabs SDK supports no Unity 6 LTS, run this plan on **Route A**. Phases 1–7 stay the same apart from the editor version.                                                                                                                                                                                                                                                                        |
| **Revisit when**                    | Unity announces built-in RP removal for the pinned LTS, or a visual refresh is scheduled (then run Phase 8).                                                                                                                                                                                                                                                                                                                  |

---

## 2. Target configuration

| Setting              | Target                                                                                                                                                                    | Source / note                                                                                                                                                                            |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Editor               | The newest **Unity 6 LTS** that the CrazyLabs SDK supports, pinned at G0. Candidate: 6000.4.0f1 (the old scaffold's version).                                             | [Decision]                                                                                                                                                                               |
| Render pipeline      | Built-in, Forward rendering path                                                                                                                                          | Same as the recovered build [Recovered]                                                                                                                                                  |
| Colour space         | **Gamma**                                                                                                                                                                 | Same as the recovered build. Linear would shift all 5 colour schemes.                                                                                                                    |
| Orientation          | Portrait only                                                                                                                                                             | [Recovered]                                                                                                                                                                              |
| Physics              | Gravity (0, −20, 0); fixed step 0.033 s; solver 2/1; bounce threshold 2; max depenetration 5; sleep 0.01; contact offset 0.04; layers 8–12; collision matrix as recovered | doc 04 §2 [Recovered]. Carried over in `ProjectSettings/`, not re-typed.                                                                                                                 |
| Quality              | Medium as default (1 pixel light, 4× MSAA, shadow distance 20)                                                                                                            | doc 04 §5                                                                                                                                                                                |
| Identity             | Product `clone-bucket`, bundle ID `io.dev.clone-bucket`, version 1.0.0                                                                                                    | [Decision] (2026-11-08)                                                                                                                                                                  |
| iOS                  | Target. Uses `BallsConfigIos` [Recovered].                                                                                                                                | —                                                                                                                                                                                        |
| Desktop (Standalone) | Target. Uses the default balls config.                                                                                                                                    | [Decision]                                                                                                                                                                               |
| WebGL                | Target for optional preview builds with null services (§6).                                                                                                               | [Decision]                                                                                                                                                                               |
| Scripting            | C# against Unity 6 APIs (`Rigidbody.linearVelocity` / `linearDamping`, not the old `velocity` / `drag`)                                                                   | New code, nothing to port                                                                                                                                                                |
| Project path         | `games/clone-bucket/unity/clone-bucket/`                                                                                                                                  | [Decision]. The old empty URP scaffold at the same path exists only in the main checkout (untracked); it must be moved or deleted before this branch is checked out there (decision D3). |

---

## 3. What the recovered project gives us

From `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/` [Recovered]:

| Input                                                                                  | Count / size                                                                                                                  | Use                                                                                                                             |
| -------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| All assets                                                                             | 7,213 files (including metas), 268 MB                                                                                         | Copied into the new project in Phase 1                                                                                          |
| Level prefabs (`Resources/prefabs/minilevels/`)                                        | 115 prefabs, 220 MB of YAML                                                                                                   | Reused as-is. All maze geometry, gates, locks and rotator values live here.                                                     |
| Meshes / materials / textures / sprites                                                | 2,199 / 162 / 103 / 207 assets                                                                                                | Reused as-is (doc 07)                                                                                                           |
| Scenes                                                                                 | 7 (`Loading`, `Main`, …)                                                                                                      | Reused. `Main.unity` holds the camera, lights, UI and shop wiring.                                                              |
| Game scripts (`Scripts/Assembly-CSharp/`, excluding the SDK and `UnityEngine` folders) | **149 `.cs` files**: root 96, `Home` 18, `Scripts` 13, `Levels` 9, `Balls` 7, `CustomEventSystem` 3, `Game` 2, `Properties` 1 | **Bodies re-implemented in place.** Classes, fields, signatures and `.meta` GUIDs are kept, so every prefab reference survives. |
| SDK script stubs                                                                       | 172 `.cs` (`Tabtale` 111, `CrazyLabs` 50, `TTPluginsAssetBundles` 11)                                                         | Deleted; replaced by the real CrazyLabs TTP SDK (Phase 6)                                                                       |
| `UnityEngine` script stubs                                                             | 174 `.cs` (UI and TMP sources recreated by the exporter)                                                                      | Deleted; replaced by packages (Phase 1, with GUID remap)                                                                        |
| Plugin DLL stubs (`Assets/Plugins/`)                                                   | 35 DLLs                                                                                                                       | Deleted; replaced by real packages (§4)                                                                                         |
| Shaders                                                                                | 17 placeholders (`//DummyShaderTextExporter`)                                                                                 | 4 return with packages; **13 re-authored** (Phase 3)                                                                            |
| Level table                                                                            | `TextAsset/LevelsData.txt`, 107 rows                                                                                          | Bundled fallback, reused as-is (doc 03)                                                                                         |

**Not recoverable.** Every method body is gone: gate arithmetic, tier override, win/fail and stars, input mapping, save format and rewards (doc 01 §6). These are re-implemented and tuned by play-testing.

---

## 4. Package map (35 stub DLLs → real packages)

| Stub DLL(s) in `Assets/Plugins/`                                             | Replacement on Unity 6                                                    | Source                   | Notes                                                                  |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------- | ------------------------ | ---------------------------------------------------------------------- |
| `DOTween.dll`                                                                | DOTween (free or Pro, whichever the team licensed)                        | Asset Store              | Stable API                                                             |
| `LeanCommon`, `LeanCommonPlus`, `LeanPool`, `LeanTouch`, `LeanTouchPlus` (5) | Lean Common, Lean Pool, Lean Touch (+ the Plus versions if licensed)      | Asset Store (CW)         | The Plus versions are paid; check the team's licences.                 |
| `Zenject.dll`, `Zenject-usage.dll`                                           | Extenject (maintained Zenject fork)                                       | OpenUPM / GitHub         | `Zenject` namespace unchanged                                          |
| `Lofelt.NiceVibrations.dll`, `…Demo.dll`                                     | Nice Vibrations (Lofelt's open-source release) or another haptics library | GitHub (Lofelt archived) | Only `HapticManager` wraps it (doc 01 §3.10); 1–3 scripts              |
| `ParticleImage.dll`                                                          | Particle Image                                                            | Asset Store              | UI particle effects                                                    |
| `Newtonsoft.Json.dll`                                                        | `com.unity.nuget.newtonsoft-json`                                         | Unity registry           |                                                                        |
| `Unity.TextMeshPro.dll`, `UnityEngine.UI.dll`                                | `com.unity.ugui` 2.x (TMP is part of uGUI in Unity 6)                     | Unity registry           | Brings back the 3 TMP shaders                                          |
| `System.Runtime.CompilerServices.Unsafe.dll`, `__Generated.dll`              | none (engine or build artefacts)                                          | —                        | Delete                                                                 |
| SDK sources (`Tabtale`, `CrazyLabs`, `TTPluginsAssetBundles`)                | CrazyLabs TTP SDK, a Unity 6–compatible release                           | **CrazyLabs (G0)**       | Analytics, remote config (`LevelFunnel`), privacy settings and rate us |

**GUID remap (Phase 1 risk).** Scenes and prefabs point to the stub scripts and stub DLL classes by GUID, and the real packages use different GUIDs. A plain swap would leave "Missing Script" on every UI image, TMP label, Lean component and pool. Phase 1 therefore includes a remap step:

1. List each stub script or DLL class (GUID + fileID) next to its package equivalent.
2. Rewrite those references across `*.unity`, `*.prefab` and `*.asset` with a script kept in `tools/recovery/scripts/`.
3. Phase 2 cannot finish until there are **zero Missing Script references**.

---

## 5. Phases

| #   | Phase                              | Depends on             | Size                          | Exit criterion                                                           |
| --- | ---------------------------------- | ---------------------- | ----------------------------- | ------------------------------------------------------------------------ |
| 0   | Gates and safeguards               | —                      | S (mostly user and publisher) | G0 and G1 closed; G3–G4 under way                                        |
| 1   | Project bootstrap and package swap | 0 (G0, G1)             | M                             | The project opens in the pinned Unity 6, compiles, and is in git         |
| 2   | Engine baseline check              | 1                      | S                             | Settings match the recovered values; 0 missing scripts                   |
| 3   | Shader re-authoring (13)           | 2                      | M                             | Side-by-side screenshots match the reference device                      |
| 4   | Core gameplay re-implementation    | 2                      | **L** (largest)               | On-screen levels 1–50 play like the reference device                     |
| 5   | Meta, UI and economy               | 4 (partly in parallel) | M                             | Full loop: menu → level → win/fail → shop → save                         |
| 6   | SDK and services                   | 0 (G0), 5              | M                             | Remote config and analytics work in a test build                         |
| 7   | Parity QA and release              | 3, 4, 5, 6             | M                             | All 71 live levels pass; release builds for iOS and desktop are produced |
| 8   | (Later, optional) URP migration    | 7                      | M                             | Separate decision (D5)                                                   |

Phases 3 and 4 run in parallel after Phase 2.

### Phase 0 — Gates and safeguards

| ID            | Item                                                                                                                                                                                   | Owner            | Why it matters                                                                                           |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- | -------------------------------------------------------------------------------------------------------- |
| **G0** [Gate] | CrazyLabs confirms which **Unity 6 LTS** its current TTP SDK supports, and delivers the SDK package plus this app's SDK config (remote-config keys).                                   | User → CrazyLabs | Pins the editor version (§2). If the answer is "none", fall back to Route A.                             |
| **G1** [Gate] | **Off-machine backup** of `games/clone-bucket/sources/` (APKs) and `tools/export/MultiMaze3D_4.1.0.0/`, checked against `tools/recovery/SHA256SUMS-apk-4.1.0.0.txt`.                   | User             | Every copy is on one disk today. Finish this before Phase 1 writes anything.                             |
| G3            | Live data request: `LevelsData.csv` per `LevelFunnel` cohort and the A/B values (`isMixedLevels`, `withBossLevels`, `bossLevelEveryFive`).                                             | User → CrazyLabs | Phases 1–5 use the bundled table, so this is not blocking there. Phase 7 needs it for live-order parity. |
| G4            | Keep a reference device running the recovered 4.1.0.0 build (installed from the APK, auto-update off). Record a reference video of on-screen levels 1–10, one boss level and the shop. | User             | The device is the parity reference for Phases 3–7.                                                       |

### Phase 1 — Project bootstrap and package swap

1. **Copy, never move**, `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/` to `games/clone-bucket/unity/clone-bucket/`. The export stays untouched as the reference.
2. **Set up version control first** (the lesson of the original loss):
   - A Unity `.gitignore` (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/`).
   - Git LFS for binaries (`*.png`, `*.jpg`, `*.wav`, `*.mp3`, `*.ogg`, `*.ttf`, `*.otf`, `*.fbx`). YAML assets (`.prefab`, `.unity`, `.asset`, `.mat`) stay as text.
   - Visible meta files and Force Text serialization.
   - Commit before the first editor launch. That commit is the untouched 2022.3-format baseline.
3. Delete the 35 stub DLLs, the 172 SDK script stubs and the 174 `UnityEngine` script stubs (§3, §4).
4. Install the packages from §4. The CrazyLabs SDK waits until Phase 6; null services stand in until then.
5. Run the **GUID remap** (§4) and commit the remap script under `tools/recovery/scripts/`.
6. Open the project in the pinned Unity 6 editor. Accept the API updater and the asset upgrade. Commit the result separately so the engine-upgrade diff can be reviewed on its own.
7. Make the 149 game scripts compile. Fix only what the package swap breaks (for example, `[Inject]` sites typed against SDK classes move to interfaces). Remove any exporter-only attributes left on the stubs.
8. Add a null remote-config provider and a null analytics service behind the existing interfaces, bound in the Zenject installers. PlayMode and WebGL previews then work without the SDK.

**Exit:** the project opens and compiles in the pinned editor with no errors, it is committed, and the recovered APK hashes still verify.

### Phase 2 — Engine baseline check

1. Diff `ProjectSettings/` against the export: physics (doc 04 §2.1), tags and layers (§2.5), collision matrix (§2.6), time, quality and player settings (Gamma, portrait).
2. **Zero Missing Script** references across all scenes and the 115 level prefabs. Add an editor test that fails on any missing script.
3. Open `Main.unity` and three level prefabs (`Level1`, `Level24`, `BossLevel1`). Custom-shader materials will render pink until Phase 3; that is expected.
4. Check the ~28 materials on built-in or unresolved shaders one by one. Point each unresolved one at its built-in shader.
5. Note where each `RotateController` setting is serialized (prefabs 3.5 / 16, `GameManager` 3 / 20) for step 4.2.

**Exit:** a checklist with every recovered setting ticked, and the missing-script test passes.

### Phase 3 — Shader re-authoring (13 shaders, built-in RP)

Keep each **shader name and every property name** unchanged so the 162 materials need no edits (doc 07 Q2). Write built-in RP shaders (vertex/fragment or surface) and compare against reference-device screenshots in Gamma space.

| Order | Shader                                    | Materials using it | Note                                                                                                               |
| ----- | ----------------------------------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------ |
| 1     | `Custom_SliderRamp`                       | 62                 | Main maze and object look; do it first                                                                             |
| 2     | `Custom_SliderRampTransparent`            | 18                 |                                                                                                                    |
| 3     | `Custom_SliderRampSpecular`               | 8                  |                                                                                                                    |
| 4     | `Custom_SliderRamp_maze`                  | 1–3                |                                                                                                                    |
| 5     | `CrazyLabs_MazeShader`                    | 1–3                | Properties `_ColorFront`, `_ColorBack`, `_FrontDirection`                                                          |
| 6     | `Custom_BuiltIn_SoftColorGradient`        | 8                  |                                                                                                                    |
| 7     | `WalkingFat_LiquidBottle_SparkleLiquid02` | 8                  | Bucket liquid. If the team still holds the Asset Store licence, re-import the original instead of re-authoring it. |
| 8     | `Custom_Skybox2`, `Custom_GradientSkybox` | 6, 1–3             | Backgrounds                                                                                                        |
| 9     | `Custom_Fog`, `Custom_Fog2`               | 1–3, 6             |                                                                                                                    |
| 10    | `Custom_BuiltIn_FleshBall_CenterGlow`     | 1–3                | Ball glow                                                                                                          |
| 11    | `UI_GrayButton`                           | 1–3                | UI                                                                                                                 |

The other 4 shaders (`TextMeshPro_Distance Field`, `TextMeshPro_Mobile_Distance Field`, `TextMeshPro_Sprite`, `Lean_Common_Alpha`) return with their packages.

**Exit:** screenshots of `Main` and of on-screen levels 1, 11 (boss) and 24 (lock), taken from the same camera, match the reference device in colour and gradient direction. Remaining differences are logged.

### Phase 4 — Core gameplay re-implementation

Write method bodies **inside the existing classes**, so fields and GUIDs stay intact (doc 01 §3 lists the classes and signatures). The steps follow the dependency order. Each step has an **automated check** (EditMode test for pure logic, PlayMode test for physics) and a **device check** (side by side with the reference video).

| Step           | Scope (classes)                                                                                                                                                                                                                                                                                      | Spec                                  | Automated check                                                                       | Device check                                                      |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- | ------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| 4.0 (optional) | **Done (2026-11-07).** Targeted disassembly of the critical methods in `libil2cpp.so` (Il2CppDumper symbols plus Ghidra 11.4.2): gate arithmetic, tier override, `OnFillBucket`, `UpdateStars`, `HandleMissionFail`, and more. See `tools/recovery/recovered/` and the §3.6 Status column in doc 09. | doc 01 §6                             | —                                                                                     | Turned [Unknown] rules into [Recovered]. Decided before 4.4 (D2). |
| 4.1            | Level data: `DatabaseController`, `DataSetConfig`, `LevelsController.Spawn`, `LevelsConfig`, prefab load from `Resources/prefabs/minilevels/`                                                                                                                                                        | doc 03                                | The live-order filter yields the 71 rows in order, and each row's prefab exists       | Levels 1–5 load in the same order                                 |
| 4.2            | Rotation and input: `RotateController` (`UpdateRotation`, `RotateAB`, `RotateToDefault`), LeanTouch drag                                                                                                                                                                                             | doc 02 §1                             | Damping response (angle over time for a fixed drag)                                   | Drag feel matches on levels 1–3                                   |
| 4.3            | Balls: `BallsPool`, `BallsController.Spawn/Despawn`, `BallView`, sizes, max speeds, layer switch at 200 / 400 / 600, value tiers                                                                                                                                                                     | doc 02 §2, doc 04 §2.2                | Pool prewarm = 200; tier and layer chosen by count; live bodies never exceed the pool | Ball count and flow on levels 1–5                                 |
| 4.4            | Gates: `GateBase` (with DOTween movement), `Multiplicator`, `SubGate`, `DivideGate`, `GatesController`, re-trigger guard `_ignoredMultiplicatorsByBalls`                                                                                                                                             | doc 02 §3                             | Arithmetic per gate type; a ball counts once per gate                                 | Gate outcomes on levels 1–10 and BossLevel1                       |
| 4.5            | Chains and locks: `Chain`, `ChainTrigger`, `ChainSegment`                                                                                                                                                                                                                                            | doc 02 §4                             | Unlocks at `UnlockNumber`                                                             | Level 24 (cost 40) and one 120 lock                               |
| 4.6            | Bucket, win/fail, stars: `Bucket`, `LevelsController.OnFillBucket` / `UpdateStars` / `HandleMissionFail` / `StartNextChunk`                                                                                                                                                                          | doc 02 §5, §7                         | Target and perfect thresholds read from the CSV; fail when no balls remain in play    | Win, perfect and fail on 3 levels each                            |
| 4.7            | Camera and boss levels: `CameraMove`, `HandleBossLevelRotation`, boss `Targets` lists, moving gates                                                                                                                                                                                                  | doc 02 §3.5, §6; doc 03 §7; doc 04 §3 | —                                                                                     | Boss levels at on-screen 11, 22, 33                               |

**Exit:** on-screen levels 1–50 (doc 03 §8) play through with the same outcomes as on the reference device, and all EditMode and PlayMode tests pass.

### Phase 5 — Meta, UI and economy

- `GameManager` screen flow (`mainScreen` to `shopScreen`) and `CompleteLevel`, `FailLevel`, `NextLevel`, `DelayedNextLevel`. Fail leads to Restart only; complete leads to Next.
- Save and load (`SaveData`, `LoadData`) need a **new** save format, since the recovered build's save format cannot be recovered. It stores a format version, the level, gems, skins and settings. Whether to migrate existing players' saves is open decision D4.
- Gems and shop per doc 06: `GemCollect`, the `ShopConfig` variants (6 / 7 / 7 skins, price 500), `SkinManager`, and `ColorSchemeController` (5 schemes).
- Settings, `AudioManager` (9 clips) and `HapticManager`.
- No debug code: no debug screens, cheat buttons or debug-only logging (removed 2026-11-08, doc 09 §3.8).

**Exit:** a full session works in PlayMode and on a development build: boot, play a level, win or fail, move to the next level, buy skins with gems in the shop, and keep progress after a restart.

### Phase 6 — SDK and services

1. Import the CrazyLabs TTP SDK from G0 and bind the real services in the Zenject installers. Editor and WebGL builds keep the null services behind a scripting define.
2. Remote config: `LevelFunnel` delivers the remote `LevelsData.csv` (with the bundled table as fallback), and the `ABTestVariables` flags are mapped as in doc 01 §3.3.
3. Analytics for levels, missions and gems, and the privacy settings, forget-me, age gate, policy and terms links, and rate us that the SDK provides.
4. **Secrets.** Keys and identifiers come from the SDK config CrazyLabs supplies. Do not copy them from the recovered build, and keep them out of docs and commit messages (`AGENTS.md` §5).

**Exit:** a test-mode build receives remote config and sends analytics events that CrazyLabs can see.

### Phase 7 — Parity QA and release

1. **Level parity:** check all 71 live levels (doc 03 §4) on the reference device for the same order, target, perfect, start balls, gates and lock costs. Use the G3 cohort tables once they arrive.
2. **Feel parity:** blind A/B between the recovered build and the rebuild on levels 1, 5, 11, 24 and 50 (rotation response, ball flow, gate timing).
3. **Performance:** frame time and live body count on the reference device at the heaviest live level (perfect 2880). Set the budget from the recovered build's measured numbers.
4. **Release builds:** iOS and desktop (Standalone) builds at version 1.0.0, with the bundle ID `io.dev.clone-bucket`.
5. **iOS:** apply `BallsConfigIos` (layers off; tiers [1, 2, 3] at [450, 650, 775]) and build.

**Exit:** the iOS and desktop release builds run, and the team signs off the parity checklist.

### Phase 8 — URP migration (later, optional)

This is a separate decision after Phase 7 (D5). Scope: rewrite the 13 shaders for URP (same names and properties), convert the ~28 built-in-shader materials, add URP assets and retune visuals. Keep Gamma if the URP version allows it; otherwise retune the 5 colour schemes for Linear.

---

## 6. Verification approach

- **Reference:** a device running the recovered 4.1.0.0 build (for parity), plus the screenshots in `games/clone-bucket/refer-levels/` and the G4 video.
- **Inner loop:** Unity Editor PlayMode via the Unity CLI or MCP (`AGENTS.md` §1).
- **Outer loop (milestones):** a **development build on the reference device** (iOS or desktop) is the main milestone build, because parity is measured there. WebGL builds (`AGENTS.md` §1) are optional previews running with null services.
- **Tests:** EditMode tests for pure logic (level-order filter, tier and layer selection, gate arithmetic, star thresholds). PlayMode tests for physics flows and the missing-script check. Run them on every commit to the project.
- **Feeding back into the specs:** when play-testing or disassembly settles a rule, update the doc that marks it `[Unknown]` and record how it was confirmed.

---

## 7. Risks

| Risk                                                                                                        | Impact                                                  | Mitigation                                                                             |
| ----------------------------------------------------------------------------------------------------------- | ------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| The CrazyLabs SDK lacks Unity 6 support                                                                     | Blocks Route C                                          | G0 before Phase 1; fallback Route A (§1)                                               |
| The GUID remap misses references                                                                            | Silent "Missing Script" on prefabs                      | Phase 2 missing-script test is a hard gate                                             |
| Gameplay rules guessed wrong (method bodies lost)                                                           | Levels play differently and the difficulty curve breaks | Device side-by-side at every step; optional targeted disassembly (4.0)                 |
| Paid Asset Store packages (LeanTouchPlus, DOTween Pro, Particle Image, liquid shader) cannot be re-licensed | Missing features                                        | Check the team's Asset Store account early; fall back to the free tier or re-implement |
| Nice Vibrations is archived                                                                                 | Haptics may break on newer OS versions                  | Keep the thin `HapticManager` wrapper and swap the library behind it                   |
| Built-in RP deprecated in a later Unity 6 release                                                           | Forced URP move                                         | Pin the LTS; Phase 8 is already scoped                                                 |
| APKs and export exist only on one disk                                                                      | Permanent loss                                          | G1 off-machine backup; the project goes into git, with LFS, from its first commit      |
| Live order differs by cohort                                                                                | Wrong level order for some players                      | G3 cohort tables; remote `LevelFunnel` keeps the order data-driven                     |

---

## 8. Open decisions

| ID  | Decision                                                                                                                                      | Needed by     |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------- | ------------- |
| D1  | Pinned: 6000.4.0f1 (installed; confirm against CrazyLabs support at G0)                                                                       | Phase 1       |
| D2  | Done: Ghidra 11.4.2 headless in Docker decompiled the target methods. Results in `tools/recovery/recovered/` and doc 09 §3.6 (Status column). | Step 4.4      |
| D3  | Move or delete the old URP scaffold at `games/clone-bucket/unity/clone-bucket/` in the main checkout                                          | Phase 1       |
| D4  | Migrate existing players' save data, or start fresh                                                                                           | Phase 5       |
| D5  | Whether to schedule Phase 8 (URP)                                                                                                             | After Phase 7 |
