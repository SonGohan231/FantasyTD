# Unity MCP 10.3.0 — FantasyTD setup and verification

## Goal

Enable **MCP for Unity** (CoplayDev, MIT, version **10.3.0**) in the **Unity 6** FantasyTD editor without modifying the legacy MAP-A gameplay logic. It provides editor tools for scene hierarchy, object creation, runtime snapshots, script editing, tests, Play Mode control, profiler inspection and Android build orchestration.

## Trusted origin

- Source: https://github.com/CoplayDev/unity-mcp
- Pinned source: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.3.0`
- Required: Unity 6000.0.26f1, git available to Unity Package Manager, Python 3.10+ and **uv** for the separate Python MCP server.
- User supplied `unity-mcp-10.3.0.mcpb` is a **four-file manifest/documentation archive**, not a standalone server executable. Its declared server command is `uvx --from mcpforunityserver mcp-for-unity`. Installing `.mcpb` alone does not install the Unity Editor-side package.
- Package is included *only on this integration branch*. The original FT2 QA PR and `main` remain untouched.

## Unity installation and first-run validation

1. Open this branch's `UnityProject/` with Unity 6000.0.26f1. Unity Package Manager automatically downloads pinned 10.3.0 from GitHub specified in `Packages/manifest.json`.
2. Wait for package resolution/import; inspect Console for errors.
3. Open **Window → MCP for Unity**, use its setup wizard to install/check Python + uv on the **same machine that runs the MCP server**, choose **Configure Selected Clients**.
4. For an MCP client with explicit stdio config, server metadata from supplied MCPB:
   ```json
   {"mcpServers":{"unity-mcp":{"command":"uvx","args":["--from","mcpforunityserver","mcp-for-unity"]}}}
   ```
   Use the editor wizard's recommended configuration/transport rather than blindly assuming this command can attach to a Unity Editor on a different host.
5. Start server and Unity editor bridge locally; connect an approved MCP client, query editor state and list scene objects in **read-only mode**. Only then allow scene edits.
6. When using Unity on a CI runner, the package may be importable but interactive MCP **cannot** be considered online because no human editor session/client is connected.

## CI / tested versus pending

- `UnityMcpSetupSmoke.Run` checks that Unity Editor sees package name `com.coplaydev.unity-mcp`, version 10.3.0 and its imported Assets folder.
- CI job `FT2 Unity MCP 10.3 import smoke` is the authority for installed editor-package state.
- CI PASS does **not** prove Python stdio server, remote MCP connectivity, screenshots, or interaction with physical Android UI.
- Avoid exposing the MCP HTTP server to public internet. Do not run arbitrary `execute_code` tools against untrusted scenes or unauthorized project paths.
- If package import/regression fails, **do not merge** this branch. Preserve the last verified game APK and restore `manifest.json`.

## ComputerMCP-specific blocker observed 2026-10-21

ComputerMCP currently points to an **ARM64 Linux host** with allowed root `/home/adambrdon/ComputerMCP/workspace`. `computer_projects` returned an empty list. Its `computer_run_command` currently permits only `pwd`, `git_status`, `git_diff_stat`; Python/uv/Git install commands are **not authorized** by that tool. Consequently we **cannot** claim that Unity, uv, this MCPB, the Python MCP server, or the FantasyTD Unity project has been installed on the ComputerMCP host. The GitHub Unity Editor package configuration here is a separate deployable integration step tested on the existing Unity CI host, not a bypass of ComputerMCP restrictions.

## Why the plugin is needed for ART-001

Current Android screenshot reveals mismatched painterly background and simple circles, oversized HUD, no convincing combat VFX. The visual sign-off process must use real Unity Game View frames and touch interaction instead of code-only compilation PASS. Create a repeatable screenshot review for before/after redesign, compatible 16:9 and 19.5:9 Android layouts, sprite visibility and real combat effects.
