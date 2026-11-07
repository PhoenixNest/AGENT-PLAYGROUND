# Workspace Guidelines: Unity & Web Prototyping

This repository follows standardized conventions for game development, web-based prototyping in Unity, and asset co-development with Blender MCP.

---

## 1. Unity Web Prototyping Workflow (Dual-Loop Strategy)

When developing games and interactive prototypes in Unity:

- **Inner Loop (Active Development & Code Iteration)**:
  - Do not trigger a full WebGL/WebGPU build on individual code prompts.
  - Use the **Unity Editor PlayMode** via the Unity CLI / MCP (`unity status`, `unity command editor.playmode start`, auto-tick) for 1–2 second rapid iteration.
- **Outer Loop (Milestone Web Builds & Previews)**:
  - When gameplay features reach a reviewable state, build to WebGL/WebGPU using the CLI:
    `unity build <project> --profile WebGL --output-path ./Build/web`
  - Test and preview using the built-in loopback server: `unity build run <project>`.
  - For full details and build profile configuration, activate the [unity-web-prototyping](file:///.agents/skills/unity-web-prototyping/SKILL.md) skill.

---

## 2. In-Engine Blockout First, Blender MCP for Production Assets

- **Phase 1 (Greybox)**: Use Unity's native in-engine tools (**ProBuilder** for 3D, **Sprite Shape & Tilemaps** for 2D) to block out levels, movement scale, and camera angles with automatic colliders.
- **Phase 2 (Asset Creation via Blender MCP)**: Use the local Blender MCP server to script procedural 3D models, organic sculpting, UV unwrapping, and texture baking.
- **Phase 3 (Prefab Swap)**: Replace ProBuilder greybox meshes in Unity with polished FBX/glTF assets while preserving colliders, physics, and gameplay scripts.

---

## 3. 4-Pillar Model Quality Assurance

All 2D and 3D assets added to the project must adhere to:

1. **Geometry & Topology**: Quad-dominant, manifold meshes; strict poly budgets; automated LOD chains (LOD0/1/2); compound physics colliders instead of dynamic mesh colliders.
2. **Texel Density & PPU**: Uniform Texel Density ($512\text{--}1024\text{ px/m}$) in 3D; uniform Pixels Per Unit (PPU) in 2D; channel-packed PBR textures in URP.
3. **Scale & Transforms**: Exactly $1.0\text{ Unit} = 1.0\text{ Meter}$; grounded pivots at base $(0, 0, 0)$; frozen transforms ($Scale=(1,1,1), Rotation=(0,0,0)$).
4. **Performance & Stripping**: Automated texture compression, shader variant stripping, and adherence to the `optimize-web` skill standards.

---

## 4. Workspace Tooling & Plugins

- **Unity Plugin & MCP**: Located at [`.agents/plugins/unity-mcp/`](file:///.agents/plugins/unity-mcp/README.md), providing 32 curated skills (`unity-cli`, `optimize-web`, `ui-uitk`, etc.) and native stdio MCP server support (`unity mcp`).
- **Blender MCP**: Configured in `mcp-servers/blender_mcp/` and registered in [`.mcp.json`](file:///.mcp.json) for procedural modeling and asset generation.

---

## 5. Project Exception: `games/clone-bucket` (Multi Maze 3D rebuild)

This project rebuilds our own shipped title from a recovered build, so some workspace defaults do not apply:

- **Source of truth**: `games/clone-bucket/docs/` and `tools/recovery/FINDINGS.md`. The recovered Unity project lives at `tools/export/MultiMaze3D_4.1.0.0/` (git-ignored; regenerate it from the APKs with `tools/recovery/scripts/run-export.ps1 -ExportName <new-folder>`).
- **Section 2 does not apply**: reuse the recovered meshes, prefabs and materials instead of ProBuilder greybox → Blender. Use Blender MCP only for new or replacement assets.
- **Section 3 QA**: recovered assets are exempt from the LOD-chain and texel-density rules (they are intentionally low-poly, e.g. the 28-triangle ball). New assets follow Section 3.
- **Original binaries**: never modify, move or commit `games/clone-bucket/sources/` (read-only APKs). Verify copies against `tools/recovery/SHA256SUMS-apk-4.1.0.0.txt`.
- **Secrets**: do not copy API keys, ad unit IDs or app IDs from the recovered build into docs, code comments or commits.
