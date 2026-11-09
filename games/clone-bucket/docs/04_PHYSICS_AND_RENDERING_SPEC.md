# 04. Physics and Rendering Specification (clone-bucket)

Status: draft rewrite. Internal rebuild of clone-bucket 1.0.0, based on the recovered v4.1.0.0 build of our earlier title. The recovered build is the source of truth.

Tags: **[Recovered]** (read from the export, path given), **[Inferred]** (deduced, reasoning given), **[Unknown]** (not recoverable; design and tune in PlayMode).

Export root: `tools/export/MultiMaze3D_4.1.0.0/ExportedProject/`. Paths are relative to it.

---

## 1. Engine and Project

| Setting                | Value                                                                              | Source                                                        |
| ---------------------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| Editor                 | 2022.3.62f2                                                                        | `ProjectSettings/ProjectVersion.txt` [Recovered]              |
| Render pipeline        | Built-in (`m_CustomRenderPipeline: {fileID: 0}`)                                   | `GraphicsSettings.asset`, `QualitySettings.asset` [Recovered] |
| Default rendering path | 1 (Forward)                                                                        | `GraphicsSettings.asset` [Recovered]                          |
| Colour space           | Gamma (`m_ActiveColorSpace: 0`)                                                    | `ProjectSettings/ProjectSettings.asset` [Recovered]           |
| Orientation            | Portrait (`defaultScreenOrientation: 0`)                                           | same [Recovered]                                              |
| Graphics API override  | none (`m_BuildTargetGraphicsAPIs: []`)                                             | same [Recovered]                                              |
| Scripting backend      | IL2CPP, arm64                                                                      | export metadata [Recovered]                                   |
| Post-processing        | none (no PostProcessLayer/Volume/Profile references in any scene, prefab or asset) | [Recovered]                                                   |

---

## 2. Physics

### 2.1 Global

From `ProjectSettings/DynamicsManager.asset` and `TimeManager.asset` [Recovered]:

| Setting                              | Value                   |
| ------------------------------------ | ----------------------- |
| Gravity                              | (0, -20, 0)             |
| Fixed timestep                       | 0.033 s (about 30.3 Hz) |
| Default solver iterations / velocity | 2 / 1                   |
| Bounce threshold                     | 2                       |
| Max depenetration velocity           | 5                       |
| Sleep threshold                      | 0.01                    |
| Contact offset                       | 0.04                    |
| Max angular speed                    | 7                       |
| Broadphase type                      | 0                       |

### 2.2 Ball body

`GameObject/Ball.prefab` [Recovered]:

| Component               | Value                                                                      |
| ----------------------- | -------------------------------------------------------------------------- |
| Rigidbody mass          | 0.1                                                                        |
| Linear / angular drag   | 0 / 0                                                                      |
| Interpolation           | none (0)                                                                   |
| Collision detection     | Continuous (1)                                                             |
| Constraints             | 56 = FreezePositionZ + FreezeRotationX + FreezeRotationY (Rotation Z free) |
| Use gravity / kinematic | on / off                                                                   |
| SphereCollider radius   | 0.54659843 (local)                                                         |
| SphereCollider centre   | (0, -0.0038836, 0)                                                         |
| Collider material       | `Assets/PhysicMaterial/Ball.physicMaterial`                                |
| Prefab root scale       | 0.2 (base radius about 0.109 m)                                            |
| Layer                   | `Ball` (9)                                                                 |

Runtime size tiers from `MonoBehaviour/BallsConfig.asset` [Recovered]: small 0.17, medium 0.19, big 0.22. Radius per tier is 0.5466 times the scale: 0.093 m, 0.104 m, 0.120 m [Inferred; confirm in PlayMode that the scale applies to the root transform].

[Inferred from the constraints above] Balls stay in the Z = 0 plane and roll around Z.

### 2.3 Physic materials

Only three exist in `Assets/PhysicMaterial/` [Recovered]:

| Asset      | Dynamic friction | Static friction | Bounciness | Friction combine | Bounce combine |
| ---------- | ---------------- | --------------- | ---------- | ---------------- | -------------- |
| `Ball`     | 0.005            | 0               | 0          | 0 (Average)      | 0 (Average)    |
| `Guide`    | 0                | 0               | 0          | 0 (Average)      | 0 (Average)    |
| `Slippery` | 0                | 0               | 0          | 1 (Multiply)     | 1 (Multiply)   |

Combine enum (Unity): 0 Average, 1 Multiply, 2 Minimum, 3 Maximum.

### 2.4 Maze rotators

`GameObject/MazeRotator_OpenDown.prefab` and `MazeRotator_OpenUp.prefab` [Recovered]: no Rigidbody, about 40 MeshColliders, 41 MeshRenderers. Driven by `RotateController` (doc 02, section 1). Whether a kinematic body is needed for ball friction is **[Unknown]**; test in PlayMode.

### 2.5 Layers and tags

From `ProjectSettings/TagManager.asset` [Recovered]:

| Index | Layer          | Index | Layer        |
| ----- | -------------- | ----- | ------------ |
| 0     | Default        | 8     | ChainSegment |
| 1     | TransparentFX  | 9     | Ball         |
| 2     | Ignore Raycast | 10    | Ball_1       |
| 4     | Water          | 11    | Ball_2       |
| 5     | UI             | 12    | Ball_3       |

Tags: Obstacle, Multiplicator, DeadTrigger, Ball, MazeSpace, DestroyTrigger, CupInTrigger, BucketTrigger, BucketFillTrigger, BucketDestroyTrigger, BucketShakeTrigger, SubGate, DivideGate, ChainTrigger, FinishSpace.

### 2.6 Collision matrix (provisional)

Decoded from the 128-byte `m_LayerCollisionMatrix` in `DynamicsManager.asset` [Recovered blob; **[Inferred]** decode, bit order not checked against Unity's documented format].

| Layer                                    | Collides with (named layers)                                          |
| ---------------------------------------- | --------------------------------------------------------------------- |
| Default (0)                              | Ball (9), Ball_1 (10), Ball_2 (11), Ball_3 (12); not ChainSegment (8) |
| ChainSegment (8)                         | Ball (9), Ball_1 (10), Ball_2 (11), Ball_3 (12); not Default (0)      |
| Ball (9)                                 | Default (0), ChainSegment (8), Ball (9), Ball_3 (12)                  |
| Ball_1 (10)                              | Default (0), ChainSegment (8), Ball_1 (10), Ball_3 (12)               |
| Ball_2 (11)                              | Default (0), ChainSegment (8), Ball_2 (11), Ball_3 (12)               |
| Ball_3 (12)                              | every layer 0 to 31                                                   |
| TransparentFX, Ignore Raycast, Water, UI | Ball_3 (12) and layers 12 to 31 only                                  |

- [Inferred]: Ball, Ball_1 and Ball_2 do not collide with one another; Ball_3 collides with all. This matches the layer escalation in doc 02 (section 2.5).
- Verify in PlayMode before relying on it.

---

## 3. Camera

### 3.1 Main Camera

`Scenes/Main.unity`, GameObject "Main Camera" [Recovered]:

| Setting              | Value                                                        |
| -------------------- | ------------------------------------------------------------ |
| Projection           | Perspective                                                  |
| Field of view        | 50                                                           |
| Near / far           | 0.3 / 1000                                                   |
| Local position       | (0, -0.4, -16.93)                                            |
| Local rotation       | quaternion (0.026177, 0, 0, 0.999657), about 3 degrees pitch |
| Clear flags          | 2 (Solid colour)                                             |
| Background           | (0.2924528, 0.2924528, 0.2924528, 1)                         |
| Culling mask / depth | 791 / -1                                                     |
| HDR / allow MSAA     | on / on                                                      |

### 3.2 UI Camera

GameObject "UICamera" [Recovered]: orthographic, size 5, near 0.1, far 2, clear flags 3 (Depth only), culling mask 32, depth 0, position (10, 0, 0).

### 3.3 CameraMove

`Scenes/Main.unity` [Recovered]: Damping 2, ZoomPadding 7.25, MinZoomSize 3, MaxZoomSize 15, BossLevelBucketOffset 6.17.

Level base camera from `MonoBehaviour/LevelsConfig.asset` [Recovered]: local position (0, -0.4, -16.93), rotation x = 3 degrees.

---

## 4. Lighting and Atmosphere

### 4.1 Directional lights

`Scenes/Main.unity` [Recovered]:

| Light | GameObject | Colour                | Intensity | Shadows            |
| ----- | ---------- | --------------------- | --------- | ------------------ |
| A     | 51         | (0.811, 0.811, 0.811) | 1         | none (`m_Type: 0`) |
| B     | 75         | (0.811, 0.811, 0.811) | 1         | not captured       |

Light B rotation quaternion (0.352, 0.184, -0.071, 0.915), about (42, 23, 0) degrees [Inferred: converted from the quaternion]. Shadow settings for Light B: **[Unknown]**.

### 4.2 Ambient and fog

`RenderSettings` in `Scenes/Main.unity` [Recovered]: fog off; ambient mode 0 (Skybox); sky about 0.491 grey; equator (0.114, 0.125, 0.133); ground (0.047, 0.043, 0.035).

---

## 5. Quality

`ProjectSettings/QualitySettings.asset` [Recovered]. Active level index 2, **Medium**.

| Field (Medium)                          | Value       |
| --------------------------------------- | ----------- |
| Pixel light count                       | 1           |
| Shadow resolution / cascades / distance | 1 / 1 / 20  |
| Anti-aliasing                           | 4 (4x MSAA) |
| Soft particles / vsync                  | 0 / 0       |
| LOD bias                                | 0.7         |

Shadow distance by level: Very Low 15, Low 20, Medium 20, High 40. Medium shadow type: not captured in this pass.

---

## 6. Shaders

17 `.shader` files are exported under `Assets/` [Recovered]:

- CrazyLabs_MazeShader
- Custom_BuiltIn_FleshBall_CenterGlow, Custom_BuiltIn_SoftColorGradient
- Custom_Fog, Custom_Fog2, Custom_GradientSkybox, Custom_Skybox2
- Custom_SliderRamp, Custom_SliderRamp_maze, Custom_SliderRampSpecular, Custom_SliderRampTransparent
- Lean_Common_Alpha
- TextMeshPro_Distance Field, TextMeshPro_Mobile_Distance Field, TextMeshPro_Sprite
- UI_GrayButton, WalkingFat_LiquidBottle_SparkleLiquid02

**The exported shaders are placeholders.** Each carries `//DummyShaderTextExporter`. Property names are preserved (for example `CrazyLabs/MazeShader`: `_ColorFront`, `_ColorBack`, `_FrontDirection`), but the bodies are stubs. Re-author shader code; do not copy export code.

**Port status (2026-11-07):** the 13 custom shaders in the rebuild were ported from the original compiled GLSL, decoded from the shipped build's GLES3 programs, not from the placeholder text. The maths and pass state per shader are in `tools/recovery/recovered/ORIGINAL_SHADERS.md`. The render comparison is in `tools/recovery/recovered/COMPARISON.md`.

---

## 7. Colour Schemes

Five schemes [Recovered: `MonoBehaviour/ColorSchemeConfig_*.asset`]: Grey (default), Black, Blue (file `ColorSchemeConfig_Blue2`), Blue_Old (file `ColorSchemeConfig_Blue`), Orange. Wall and bucket colours are listed in doc 06 (colour schemes); `ballsMaterials` is empty in all five. The level table uses Grey for every row.

---

## 8. Performance

Performance budgets: **[Unknown]** until measured on device. Ball pool: `initialPoolAmount` 200 pre-warmed [Recovered]; the live physics-body count and any growth beyond 200 are **[Unknown]**. Large counts are represented by higher-value balls rather than more bodies [Inferred] (doc 02, sections 2.3–2.4).

---

## 9. Rebuild Target

**Route C** (`08_IMPLEMENTATION_PLAN.md`): Unity 6, **built-in render pipeline**, Forward, **Gamma**. That is the same pipeline and colour space as the original, so every value in this document applies unchanged.

- Physics, layers, tags and the collision matrix are carried over in the recovered `ProjectSettings/` (not re-typed), then checked against §2 (`10_ENGINE_BASELINE_CHECK.md`).
- Quality, camera and lighting stay as recovered (§3–§5).
- The 13 custom shaders in §6 are re-authored as built-in RP shaders with the same names and property names.
- New code uses the Unity 6 physics API names (`Rigidbody.linearVelocity`, `linearDamping`). The PhysX settings themselves are unchanged.
- URP is an optional later step; this document does not apply to it without re-checking §4–§6.

---

## 10. Open Questions

1. Confirm the collision-matrix decode in PlayMode.
2. Does the maze need a kinematic body to couple ball friction?
3. Medium shadow type and bias; Light B shadow settings.
4. Per-scheme colours and ball skin materials.
5. Performance budgets (draw calls, triangles) on target devices.
