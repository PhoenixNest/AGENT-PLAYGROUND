---
name: unity-mcp
description: Guide and best practices for using the Unity MCP Server to automate Unity Editor interactions, inspect scenes, manipulate GameObjects, manage assets, compile scripts, and monitor console logs.
---

# Unity MCP Development Guide

This skill documents how to interact with the Unity Editor using the **Model Context Protocol (MCP)** server configured in this workspace (`mcp-for-unity-server`).

## 1. Architecture & Connection

The Unity MCP server acts as an intelligent bridge between the AI coding assistant and the active Unity Editor session via `stdio` transport.

- **MCP Runner Command**: `uvx --from mcpforunityserver mcp-for-unity --transport stdio`
- **Unity Package**: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
- **Startup Grace Period**: Configured with `UNITY_MCP_SKIP_STARTUP_CONNECT: "true"`, allowing the MCP server to start reliably even when Unity Editor is not currently open, automatically reconnecting once the project opens in Unity.

---

## 2. Setting Up in Unity Editor (1-Step Package Addition)

When creating or opening the Unity project in Unity 6:

1. Open **Window** > **Package Manager**.
2. Click the **+** button in the upper left corner.
3. Select **Add package from git URL...**.
4. Paste:
   ```text
   https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
   ```
5. Click **Add**. The Unity Editor bridge starts automatically and listens for MCP commands.

---

## 3. Core MCP Tools & Capabilities

### A. Scene & GameObject Management

- `manage_scene`: Inspect scene hierarchies, query nodes, create/save scenes.
  - _Best Practice_: Always use pagination (`page_size: 50`) when retrieving hierarchies to maintain concise token usage.
- `manage_gameobject`: Create GameObjects, add components, set transforms, and configure component properties.
  - _Best Practice_: Use `include_properties=false` initially for metadata inspection before requesting detailed component properties.

### B. Asset & Prefab Management

- `manage_asset`: Search project assets (`Texture2D`, `AudioClip`, `Prefab`, `Material`), import assets, and instantiate prefabs into the scene hierarchy.
  - _Best Practice_: Keep `generate_preview=false` unless visual thumbnails are explicitly required.

### C. Script & Compilation Workflow

- `manage_script`: Create and modify C# scripts inside `Assets/Scripts/`.
- `read_console`: Monitor compiler errors, warnings, and runtime logs.
  - _Crucial Rule_: Always check `read_console` after editing or creating C# scripts to verify compilation status before calling new component types or entering PlayMode.

### D. PlayMode & Runtime Verification

- `manage_editor`: Control PlayMode (Play, Pause, Step, Stop) to test game logic, event triggers, and state machines directly.

### E. API Reflection (Zero Hallucination)

- `unity_reflect`: Query exact Unity 6 C# API types and member signatures directly from the running engine.
- `unity_docs`: Fetch official Unity API docstrings and code examples.

---

## 4. Key MCP Resources

Resources are read-only state endpoints addressed via `mcpforunity://`:

- `mcpforunity://instances`: List all connected Unity Editor sessions (`Name@hash`).
- `mcpforunity://editor/state`: Current editor state (`ready_for_tools`, `is_compiling`, `is_playing`).
- `mcpforunity://project/info`: Project configuration, Unity version, and target platform settings.
- `mcpforunity://tests`: Discover and run Unity Test Framework (UTF) edit/play mode test suites.
