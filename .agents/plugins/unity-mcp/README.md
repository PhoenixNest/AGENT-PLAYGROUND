# Unity Agent Plugin (Workspace Scope)

This directory contains the workspace-level **Unity Plugin** for Antigravity, Claude Code, Cursor, and related AI agent tooling. It provides curated skills and MCP configuration to drive the Unity Editor, automate builds, and author game systems.

---

## Capabilities & Structure

- **`plugin.json`**: Plugin manifest declaring metadata, version (`0.1.8-beta`), and capabilities.
- **`mcp_config.json`**: Declares the native Model Context Protocol (MCP) server running via standard I/O (`unity mcp`).
- **`skills/`**: 32 specialized agent skills covering the end-to-end Unity lifecycle:
  - **Core & Projects**: `unity-cli`, `new-unity-project`, `unity-package-management`
  - **Optimization**: `optimize-web`, `optimize-audio`, `optimize-text-mesh-pro`
  - **2D Workflows**: `2d-pixel-perfect`, `create-2d-physics`, `create-tile-palette-or-rule-tiles`, `manage-sprite-atlas`, `sprite-editor`, `sprite-segment-3x3grid`
  - **3D & Rendering**: `asset-transformer-toolkit`, `physics-3d-collision`, `initialize-ai-navigation`, `urp-postprocessing`, `shader-graph-create-custom-node`, `migrate-birp-to-urp`, `validate-urp-render-graph-renderer-feature`
  - **UI Systems**: `ui`, `ui-uitk`, `ui-ugui`, `ui-imgui`
  - **Monetization & Services**: `implement-in-app-purchases`, `levelplay-unity-integration`, `build-live-game`, `setup-multiplayer-services`, `setup-vivox-voice-chat`, `localization`
- **`assets/` & `scripts/`**: Supporting icons, validation scripts, and metadata helpers.

---

## MCP Server Integration

The plugin connects to the running Unity Editor through the Unity CLI's built-in MCP server:

```json
{
  "mcpServers": {
    "unity": {
      "command": "unity",
      "args": ["mcp"]
    }
  }
}
```

When a Unity project with the `com.unity.pipeline` package is open in the Unity Editor, the MCP server automatically exposes live tools for inspecting GameObjects, evaluating C# scripts, toggling PlayMode, and capturing frames.

---

## Integration with Workspace Workflows

- **Prototyping Standards**: Follow the guidelines defined in [`AGENTS.md`](../../AGENTS.md) for the Dual-Loop rapid iteration cycle.
- **Prototyping Skill**: Refer to [`unity-web-prototyping`](../skills/unity-web-prototyping/SKILL.md) for detailed procedures on WebGL/WebGPU export and Blender MCP co-development.
