---
name: unity-web-prototyping
description: >-
  Workflows, conventions, and standards for developing web-based prototypes directly in Unity 6.
  Covers the Dual-Loop rapid iteration cycle (Editor PlayMode + WebGL/WebGPU builds), in-engine
  greyboxing (ProBuilder/Tilemaps), Blender MCP asset co-development, and the 4-Pillar Model Quality Assurance framework.
---

# Unity Web Prototyping & Asset Co-Development Workflow

This skill defines the standardized architecture for developing prototypes directly inside Unity for web preview, completely eliminating the "translation tax" of rewriting standalone HTML/JS prototypes into Unity.

---

## 1. Core Philosophy: The "Dual-Loop" Iteration Architecture

Never attempt to re-export a full WebGL build on every prompt or code tweak. IL2CPP and Emscripten linking introduce a 2–5+ minute compilation floor that destroys conversational AI flow. Instead, separate development into two distinct loops:

```
┌─────────────────────────────────────────────────────────────┐
│                   INNER LOOP (1–3 Seconds)                  │
│  AI writes C# / UI Toolkit  ──>  Unity Editor PlayMode      │
│  Instant Roslyn recompile    ──>  Observe frames / console  │
│                                  via Unity CLI / MCP        │
└──────────────────────────────┬──────────────────────────────┘
                               │ Milestone Reached
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                   OUTER LOOP (Automated)                    │
│  `unity build --profile WebGL` (Build Profile: Fast/WebGPU) │
│  `unity build run` (Serves on loopback HTTP with headers)   │
│                                                             │
│   ==> Instant browser playtesting with 100% native assets   │
└─────────────────────────────────────────────────────────────┘
```

### A. Inner Loop: Live In-Editor Verification

- **Tooling**: Use `unity-cli` and `com.unity.pipeline` over local stdio MCP / CLI (available in `.agents/plugins/unity-mcp/`).
- **Ticking while Unfocused**: Run `unity command editor set_auto_tick` once per session so the Editor keeps advancing frames when headless or backgrounded.
- **Run in Background**: Ensure `UnityEngine.Application.runInBackground = true;` (or use `unity command editor.playmode set_run_in_background`).
- **Frame & Log Verification**: Enter PlayMode with `unity command editor.playmode start`, confirm `Time.frameCount` advances via `job wait`, check console logs, and capture screenshots for visual verification.

### B. Outer Loop: WebGL / WebGPU Milestone Build

When a prototype feature or mechanic is verified:

1. Create/Select the WebGL Build Profile:
   ```powershell
   unity build <project-path> --create-profile WebGL
   ```
2. Build with rapid iteration settings (Code Optimization: _Shorter Build Time_, Compression: _Disabled_, WebAssembly 2023 enabled):
   ```powershell
   unity build <project-path> --profile WebGL --output-path ./Build/web
   ```
3. Launch the local loopback server to preview in browser:
   ```powershell
   unity build run <project-path>
   ```
   _(The CLI automatically configures CORS, COOP/COEP headers, and SharedArrayBuffer support)._

---

## 2. 2D vs. 3D In-Engine Prototyping

Avoid importing heavy external models during initial concept validation. Use Unity's native in-engine authoring tools first:

### 3D Rapid Blockout (ProBuilder)

- **Package**: `com.unity.probuilder`
- **Workflow**:
  1. Construct rooms, stairs, ramps, and obstacles directly in the Scene view using ProBuilder primitives and face extrusion/beveling.
  2. Assign basic placeholder materials and verify player movement, sightlines, scale, and camera angles.
  3. ProBuilder objects automatically carry accurate colliders for instant PhysX testing.

### 2D Rapid Blockout (Sprite Shape & Tilemaps)

- **Packages**: `com.unity.2d.spriteshape`, `com.unity.2d.tilemap`
- **Workflow**:
  1. For organic terrain / platformer hills: Draw bezier curves with Sprite Shape controllers; borders and colliders generate automatically.
  2. For grid/dungeon layouts: Paint with Tilemaps using Rule Tiles for automated wall/corner connections.
  3. Lock uniform Pixels Per Unit (PPU) across all sprites and use `PixelPerfectCamera` to prevent sub-pixel jitter.

---

## 3. Blender MCP Co-Development Pipeline

When the gameplay mechanics are validated in blockout form, transition to high-fidelity asset modeling using the workspace's **Blender MCP server**:

```
[ProBuilder Greybox / Blockout] ──> Test Gameplay Scale in Unity
                 │
                 ▼
[Blender MCP (Python bpy)]     ──> Procedural Modeling / Sculpting / UV Unwrap / Bake
                 │
                 ▼
[Export .fbx / .gltf]           ──> Import to Unity Assets/Models/
                 │
                 ▼
[Prefab Asset Swap]             ──> Replace Greybox Mesh, Retain Colliders & Script Logic
```

- **Blender MCP Responsibilities**:
  - Procedural mesh modeling, beveling, and modifier application via Python scripts.
  - Automated UV unwrapping (Smart Project or seam-based) with uniform island margin spacing.
  - Baking high-to-low poly normal maps and ambient occlusion maps.
  - Mesh decimation for LODs (Level of Detail).
  - Direct export to Unity's `Assets/` directory.

---

## 4. The 4-Pillar Model Quality Assurance Framework

Every 2D or 3D model integrated into the project must pass the following four checks before production sign-off:

### Pillar 1: Geometric Integrity & Polygon Budgeting (3D)

- **Topology**: Clean, quad-dominant topology. Strictly zero non-manifold geometry, zero inverted normals, and no loose vertices or edges.
- **Target Budgets (Web/Mobile Optimized)**:
  - _Hero / Player characters_: 10,000 – 25,000 tris.
  - _Environment props / structures_: 500 – 3,000 tris.
  - _Background scenery_: < 500 tris.
- **Automated LOD Chains**:
  - **LOD0 (100%)**: Full visual fidelity.
  - **LOD1 (40–50%)**: Decimated mesh for mid-distance viewing.
  - **LOD2 (10–20% or Impostor)**: Low-poly proxy for distant horizon.
- **Physics Collision Proxies**: Never use dynamic complex mesh colliders. Use compound primitives (Capsule + Box) or simplified convex hulls.

### Pillar 2: Texture, UV, and Material Standards

- **Consistent Texel Density (3D)**: Standardize on a uniform density (e.g. 512 px/m or 1024 px/m) across props so visual resolution remains harmonious.
- **Strict PPU Consistency (2D)**: Uniform Pixels Per Unit (e.g. 16, 32, or 100 PPU) throughout all sprites in the project.
- **Channel-Packed PBR Maps (URP)**: Pack grayscale masks into single textures (R = Metallic, G = Occlusion, B = Detail Mask, A = Smoothness) to reduce draw calls and texture bandwidth.
- **Draw Call Batching**: Pack 2D sprites into `SpriteAtlas` by screen/biome; enable GPU Instancing on 3D environment materials.

### Pillar 3: Scale, Pivot, and Transform Normalization

- **Scale Standard**: Exactly $1.0\text{ Unity Unit} = 1.0\text{ Meter}$.
- **Standardized Pivots**:
  - Grounded assets (characters, props, trees): Pivot placed precisely at base/ground contact $(0, 0, 0)$.
  - Rotating / projectile / dynamic assets: Pivot placed at geometric center / center of mass.
- **Freeze Transforms**: Ensure Scale is $(1, 1, 1)$ and Rotation is $(0, 0, 0)$ prior to export.

### Pillar 4: In-Engine & Web Performance Audit

- **AssetPostprocessor**: Configure automated texture compression (`ASTC` for Web/mobile, `DXT/BC` for desktop) and disable unused Read/Write flags.
- **Optimize-Web Audit**: Apply the `optimize-web` skill checks (shader variant stripping, unused package removal, Brotli compression for release builds).
