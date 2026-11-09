# 07. Assets: Recovered Inventory, Reuse Workflow and QA

**Status:** Draft for _clone-bucket_. Assets are **recovered** from the v4.1.0.0 build of our earlier title, not modelled from scratch.
**Rebuild target:** Route C, Unity 6 with the built-in render pipeline and Gamma (`08_IMPLEMENTATION_PLAN.md`). Recovered materials are reused unchanged; custom shaders are re-authored for the built-in pipeline.
**Tags:** `[Recovered]` (path cited), `[Inferred]`, `[Unknown]`.

---

## 1. Where the assets live

- Root: `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/Assets/` (AssetRipper 2.0.0, Unity 2022.3.62f2, built-in pipeline).
- Each asset has a `.meta` sidecar. **Counts below are asset files, not files including metas.**
- Keep GUIDs intact. Renaming or moving a recovered file breaks every prefab and asset that references it.

---

## 2. Inventory

| Folder                          | Assets     | Notes                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Tag         |
| ------------------------------- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| `Mesh/`                         | 2,199      | Named `Object.*` (997), `Obj.*` (603), `Cube.*` (539), `BossLevel*` (28), `horn*` (about 15), `Guide` (4), `Bucket` (2) + `Bucket_shadow`, `Lock` / `Lock1`, `Background` (2), `pCube` (2), `Plane` (2), `Ball1`, `CPlane`. No `SM_` names.                                                                                                                                                                                                                      | [Recovered] |
| `Texture2D/`                    | 103        | Includes 36 PNGs `Ball01_01`–`Ball06_06` (skin families), `coin`, `skin_icon`, `shadow`, `counter_pad`, `star_pad_blue`, particle sprites, and ETC2 atlases (`shopaltas`, `loading`).                                                                                                                                                                                                                                                                            | [Recovered] |
| `Sprite/`                       | 207        | Including `balls_skin#blue`, `balls_skin#colorful`, `ball1`–`ball6`, `ball_0x`, `coin`, `Skin`.                                                                                                                                                                                                                                                                                                                                                                  | [Recovered] |
| `Material/`                     | 162        | Shader use: `Custom_SliderRamp` 62, unresolved/built-in 28, `Custom_SliderRampTransparent` 18, `Custom_SliderRampSpecular` 8, `Custom_BuiltIn_SoftColorGradient` 8, `WalkingFat_LiquidBottle_SparkleLiquid02` 8, `Custom_Skybox2` 6, `Custom_Fog2` 6, TMP Distance Field 5, others 1–3.                                                                                                                                                                          | [Recovered] |
| `Shader/`                       | 17         | `CrazyLabs_MazeShader`, `Custom_BuiltIn_FleshBall_CenterGlow`, `Custom_BuiltIn_SoftColorGradient`, `Custom_Fog`, `Custom_Fog2`, `Custom_GradientSkybox`, `Custom_Skybox2`, `Custom_SliderRamp`, `Custom_SliderRamp_maze`, `Custom_SliderRampSpecular`, `Custom_SliderRampTransparent`, `Lean_Common_Alpha`, `TextMeshPro_Distance Field`, `TextMeshPro_Mobile_Distance Field`, `TextMeshPro_Sprite`, `UI_GrayButton`, `WalkingFat_LiquidBottle_SparkleLiquid02`. | [Recovered] |
| `AudioClip/`                    | 9          | Not catalogued by name here.                                                                                                                                                                                                                                                                                                                                                                                                                                     | [Recovered] |
| `AnimationClip/`                | 15         | Includes `Intro`, `Idle`.                                                                                                                                                                                                                                                                                                                                                                                                                                        | [Recovered] |
| `GameObject/`                   | 38 prefabs | `Ball`, `Sport`, `LevelView`, `LevelGenerative`, `MazeRotator_OpenDown`, `MazeRotator_OpenUp`, `Multiplicator`, `SubGate`, `DivideGate`, `BucketFX`, `StarFX`, `GemImage`, `BossWall_*` (7 variants), and debug-tool widgets (the 14 debug-tool widgets are removed from the rebuild).                                                                                                                                                                           | [Recovered] |
| `Resources/prefabs/minilevels/` | 115        | One prefab per level row (`Level*.prefab`, `BossLevel*.prefab`, `Level_*_2.8.prefab`). Maze geometry is inside these.                                                                                                                                                                                                                                                                                                                                            | [Recovered] |
| `Scenes/`                       | 7          | `Main.unity` holds the shop wiring.                                                                                                                                                                                                                                                                                                                                                                                                                              | [Recovered] |

### 2.1 Ball

- `Mesh/Ball1.asset`: **21 vertices, 84 indices = 28 triangles**, one submesh. [Recovered] (re-checked)
- Stored bounds half-extent about 0.571 × 0.571 × 0.294 units.
- `GameObject/Ball.prefab`: `SphereCollider` radius 0.5466 on prefab scale 0.2 (world about 0.109). Runtime scale small 0.17 / medium 0.19 / big 0.22 (`BallsConfig`). [Recovered]
- Skins: each skin is a material family in `Material/Ball0x_0y`, selected by `ShopConfig` (see `06`). The mesh is shared.

### 2.2 Maze geometry

- Walls, funnels and chassis are **inside level prefabs**, not separate reusable meshes. Rotating mazes are `MazeRotator_OpenDown` / `MazeRotator_OpenUp`: no Rigidbody, about 40 `MeshCollider`s each. [Recovered]
- Boss walls: `BossWall_Center_Funnel`, `BossWall_Center_Spike`, `BossWall_Left_*` (3), `BossWall_Right_*` (3). [Recovered]
- No standalone `SM_Chassis_*` or `SM_Wall_*` assets exist.

### 2.3 Placeholder status

- Quality of each recovered asset is [Unknown] until reviewed. Treat them as **recovered placeholders**: reuse by default, flag for re-authoring where they are engine- or shader-dependent.
- Shaders: the 17 exported shaders are placeholders (names and properties kept, code lost). 13 custom shaders must be re-authored; 4 come back with their packages (TextMeshPro ×3, Lean). They are re-authored as built-in RP shaders (Route C; plan in `08_IMPLEMENTATION_PLAN.md`). [Decided]

---

## 3. Workflow

1. **Reuse first.** Start from the recovered asset in `tools/export/.../Assets/`. Copy it into the working project; do not rename it in place.
2. **Blender MCP only for new or replacement assets.** Use it when no recovered asset fits, or when a placeholder must be re-authored. Keep the source `.blend` out of the Unity import tree (`Source~/`).
3. **Unity CLI for in-engine work.** Use the Unity Editor PlayMode loop for iteration; build WebGL only at milestones (per `AGENTS.md` §1).
4. **Wire new assets by GUID through prefabs.** Do not hard-code paths.
5. **Log every replaced recovered asset** in the rebuild changelog with its original path and the reason.

---

## 4. QA rules and exceptions

`AGENTS.md` §3 (4-Pillar QA) applies to **new** assets. Recovered assets get the exceptions below.

| Rule (AGENTS.md §3)                                   | Applies to recovered assets?                                          | Proposed handling                                                                                                                       |
| ----------------------------------------------------- | --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Quad-dominant, manifold, poly budget                  | Partly                                                                | Recovered meshes are low-poly (e.g. Ball1 = 28 triangles). Budget check is a warning, not a fail.                                       |
| Automated LOD chains (LOD0/1/2)                       | **No** (exception)                                                    | Recovered meshes are already low-poly; LODs add no value. Propose: LOD required only above a poly threshold TBD by the team. [Proposed] |
| Compound colliders, no mesh colliders on moving parts | Yes, but existing `MeshCollider`s in `MazeRotator_*` must be reviewed | Replace with compound colliders during re-authoring. [Proposed]                                                                         |
| Uniform texel density 512–1024 px/m                   | **No** (exception)                                                    | Recovered textures were authored for their own scale. Apply to new assets only. [Proposed]                                              |
| Uniform PPU (2D)                                      | Yes for sprites                                                       | Check the 207 sprite assets for mixed PPU before use.                                                                                   |
| Scale 1 unit = 1 m; pivots at base; frozen transforms | Yes, verify                                                           | Ball prefab scale 0.2 needs a documented reason (see 04).                                                                               |
| Texture compression, shader stripping, `optimize-web` | Yes                                                                   | Applies to the whole build.                                                                                                             |

---

## 5. Open questions

- **Q1.** Which recovered maze geometry is kept as-is, and which is re-authored? (Needs a level-by-level review.)
- **Q2.** The 17 exported shaders are placeholders (`//DummyShaderTextExporter`: names and properties kept, code lost). 4 come back with their packages (3× TextMeshPro, Lean/Common/Alpha); the other 13 custom shaders are re-authored as built-in RP shaders (Route C; order and checks in `08_IMPLEMENTATION_PLAN.md`). Keeping each shader's name and property names unchanged lets all 162 materials work without edits.
- **Q3.** Are the 36 `Ball0x` textures the full skin set, or are some skins still missing art?
- **Q4.** Target polygon threshold for LOD (Section 4) and the texel-density exception.
- **Q5.** Confirm the ball prefab scale 0.2 and runtime scale 0.17–0.22 units with the physics owner.
- **Q6.** Audio: which of the 9 clips are used, and do we re-author them?
