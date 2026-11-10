# 10. Engine Baseline Check (Unity 6000.4.0f1)

Checked on 2026-11-07, after the project first opened in Unity 6000.4.0f1 (commits `067b124e`, `f87d0497`). Each recovered setting was compared with the read-only export at `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/ProjectSettings/` and with doc 04 sections 1 to 5. The comparison covered every line of the settings files that Unity rewrites: DynamicsManager, TimeManager, TagManager, GraphicsSettings, QualitySettings, EditorSettings, Physics2DSettings and InputManager were each identical to the export line by line. ProjectSettings.asset differs only by Unity 6 fields that the recovered build did not have. One recovered value changed on open (the graphics API override) and was restored. The only intended difference from doc 04 is the editor version.

| Setting                              | Expected (recovered)                                       | Actual                                                                                   | Result                                                                                                                             |
| ------------------------------------ | ---------------------------------------------------------- | ---------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Editor version                       | 2022.3.62f2                                                | 6000.4.0f1 (`ProjectVersion.txt`)                                                        | EXPECTED CHANGE (Unity 6 upgrade)                                                                                                  |
| Render pipeline                      | Built-in (`m_CustomRenderPipeline: {fileID: 0}`)           | same                                                                                     | OK                                                                                                                                 |
| Default rendering path               | 1 (Forward)                                                | `m_DefaultRenderingPath: 1`                                                              | OK                                                                                                                                 |
| Gravity                              | (0, -20, 0)                                                | `m_Gravity: {x: 0, y: -20, z: 0}`                                                        | OK                                                                                                                                 |
| Default solver iterations / velocity | 2 / 1                                                      | 2 / 1                                                                                    | OK                                                                                                                                 |
| Bounce threshold                     | 2                                                          | 2                                                                                        | OK                                                                                                                                 |
| Max depenetration velocity           | 5                                                          | 5                                                                                        | OK                                                                                                                                 |
| Sleep threshold                      | 0.01                                                       | 0.01                                                                                     | OK                                                                                                                                 |
| Contact offset                       | 0.04                                                       | 0.04                                                                                     | OK                                                                                                                                 |
| Max angular speed                    | 7                                                          | 7                                                                                        | OK                                                                                                                                 |
| Broadphase type                      | 0                                                          | `m_BroadphaseType: 0`                                                                    | OK                                                                                                                                 |
| Layer collision matrix               | recovered 128-byte blob                                    | identical blob                                                                           | OK                                                                                                                                 |
| Fixed timestep                       | 0.033                                                      | `Fixed Timestep: 0.033`                                                                  | OK                                                                                                                                 |
| Tags and layers 8 to 12              | recovered TagManager                                       | identical file                                                                           | OK                                                                                                                                 |
| Quality active level                 | 2 (Medium)                                                 | `m_CurrentQuality: 2`                                                                    | OK                                                                                                                                 |
| Quality Medium values                | recovered Medium block                                     | identical file                                                                           | OK                                                                                                                                 |
| Colour space                         | Gamma (`m_ActiveColorSpace: 0`)                            | 0                                                                                        | OK                                                                                                                                 |
| Orientation                          | Portrait (`defaultScreenOrientation: 0`, portrait allowed) | 0; `allowedAutorotateToPortrait: 1`                                                      | OK                                                                                                                                 |
| Graphics API override                | none (`m_BuildTargetGraphicsAPIs: []`)                     | Unity added a Windows Standalone entry on open (D3D11, D3D12, `m_Automatic: 0`)          | FIXED: restored to `[]`                                                                                                            |
| Scripting backend                    | IL2CPP, arm64 (export metadata)                            | `scriptingBackend: {}` and `il2cppCompilerConfiguration: {}`, identical to export        | OK for this check. The per-platform values are not in the recovered ProjectSettings and must be set in the release build settings. |
| Package id (`applicationIdentifier`) | not recovered                                              | `{}`, identical to export                                                                | NOTE: not set in the recovered data. Now set to `io.dev.clone-bucket`.                                                             |
| Product version and bundle version   | 4.1.0.0 (recovered)                                        | `bundleVersion: 1.0.0` (rebuild, changed on purpose)                                     | Changed on purpose                                                                                                                 |
| Product and company names            | Multi Maze, AlexandrJanashvili (recovered)                 | clone-bucket; company name `DefaultCompany` (Unity's default) in `ProjectSettings.asset` | Changed on purpose                                                                                                                 |
| Unity 6 `productGUID`                | blank in recovery                                          | fixed random 32-hex value (commit `5f97de95`)                                            | FIXED (Unity regenerated it on every open)                                                                                         |

Other FIXED or changed items: none beyond the two rows above.

## Test infrastructure

- EditMode tests in `Assets/Tests/Editor/` compile into `Assembly-CSharp-Editor` (no asmdef) and can reference game scripts. `GameAssemblySmokeTests` passes, which confirms this layout works in this project.
- Run them with `games/clone-bucket/unity/tools/unity-batch.ps1 -Mode editmode`.

## Missing-script test

`Assets/Tests/Editor/MissingScriptTests.cs` checks every scene (2), prefab (166) and `.asset` (2498) under `Assets/`. Result: 0 missing scripts.

- Not counted but noted: `Assets/Sprite/..asset` (a Sprite named "." whose texture reference resolves) does not load by its main object path in batch mode. It is not a script problem. The test logs it as a warning.

The test checks only that each script reference resolves to a script GUID. It does not check that the class loads at runtime. A runtime binding failure was found by a PlayMode test, `WP17_BossAndRotationPlayTests.MainSceneComponentsAllBindAtRuntime`, and fixed. The `ParticleImage` shim's namespace was `AssetKits.ParticleImage`, which ends with the class name, so Unity's `MonoScript.GetClass()` returned null. The 31 UI particle objects in `Main.unity` (the complete-screen star burst, the star-bar stars, the gem and coin effects and the shop skin-unlock sparkles) had a missing component and drew nothing. The namespace is now `AssetKits`.

## Material audit

`Assets/Editor/Diagnostics/MaterialAudit.cs` writes one row per material to `games/clone-bucket/output/diagnostics/materials.csv` (179 materials). Result:

- Unresolved shader references (missing GUID): 0. Nothing was repointed.
- Unity error shader: 0 materials.
- Built-in shader references: 28 materials. Standard 5, Unlit/Texture 6, Unlit/Color 2, Particles/Standard Surface 4, Particles/Standard Unlit 2, Legacy Shaders/Particles/Additive 4, Legacy Shaders/Particles/Alpha Blended 4, GUI/Text Shader 1.
- Exported placeholder shaders (contain `//DummyShaderTextExporter`): 151 materials, left untouched until the shaders are re-authored.
- `shader.isSupported` is False for every shader. The batch run uses `-nographics`, which has no GPU device, so this column is not meaningful here.

## Key assets

`Assets/Editor/Diagnostics/KeyAssetCheck.cs` opened `Main.unity` and loaded three mini-level prefabs.

| Asset      | RotateController (Multiplier / Damping) | GameManager (ControllerMultiplier / ControllerDamping) | Missing scripts |
| ---------- | --------------------------------------- | ------------------------------------------------------ | --------------- |
| Main.unity | 0 instances                             | 1 instance: 3 / 20                                     | 0               |
| Level1     | 3.5 / 16                                | not present                                            | 0               |
| Level24    | 3.5 / 16                                | not present                                            | 0               |
| BossLevel1 | 3.5 / 16                                | not present                                            | 0               |
