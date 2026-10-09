# VCMI ↔ Quest 3 integration plan (engineering, not a completed integration)

## Baseline that already exists
- VCMI is an open-source GPL-2.0-or-later recreation of the Heroes III engine.
- VCMI offers Android ARM64 builds; it requires the user's lawful Heroes III Complete / SoD data.
- Horizon OS can run standard Android applications as **2D panels**; this is presently the direct route to *actual* Heroes III game mechanics on a Quest.
- 2D app-in-headset is not stereo 3D Heroes terrain. The Unity tabletop scene currently runs placeholder local rules.

## Architecture A: native gameplay on 2D panel — shortest path to a real game
1. Install current VCMI Android APK (ARM64-v8a) using developer mode.
2. Import the user's licensed GOG offline installer (`.exe` + `.bin`) or existing `Data/Maps/Mp3` directories.
3. Smoke test on Quest: game menu, map, visit town, hero orders, spell UI, tactical combat, save/reload; also test physical controller click, long press/right click.
4. Identify HID limitations; implement scoped adjustments to VCMI Android input and/or platform launcher, with GPL source compliance.
5. Deliver headset-installable APK *only after* an actual Android build and device or simulator verification.

## Architecture B: hybrid spatial app — immersive canvas with embedded VCMI panel
Meta Spatial SDK hybrid Android apps allow a 2D panel activity to be embedded within an immersive activity. Research whether VCMI Qt-based activity (as built in modern VCMI Android) can be hosted as the hybrid panel in one Android package. Confirm that:
- Android activity lifecycle, JNI/native libraries, Qt surface and required permissions work together.
- an immersive OpenXR 3D tabletop is allowed to host the needed 2D panel in the same app;
- controller pointers/multi-touch map correctly into native VCMI input.
Proof required: device-captured video and Android logcat with real game map, not mocked game screenshots.

**Do not assume** embedding an external APK's activity is automatically possible. A fork/build integration is likely necessary.

## Architecture C: full 2.5D VR game with VCMI source as authority
- Separate the VCMI *game simulation*, adventure-map state, turn-based actions and save/load from renderer/Qt platform code.
- Design a versioned in-process native JNI bridge or local IPC service for `GameSnapshot` and `Command` (hero path, end turn, buy unit, cast spell, battle action).
- Use an explicit capability negotiation: world, town, battle, spells, inventory, multiplayer, campaign/story.
- Render snapshots as VR dioramas (Unity OpenXR or native engine), never maintain a separate "copy" of Heroes rules.
- Every command must be validated by the authoritative VCMI simulation. Save/restore remains on VCMI side.
- No claim of native bridge until source patches, build, integration tests and gameplay logs exist.

### Suggested minimal bridge contract (DESIGN ONLY)
`GET /capabilities` -> protocol version and supported domains.
`GET /snapshot` -> revision, selected hero, visible map tiles, legal actions.
`POST /commands` -> request UUID, expected revision, action and parameters.
`GET /events?since=revision` -> ordered authoritative game events.
HTTP shown for clarity; choose JNI/IPC based on measured overhead/security/runtime constraints.

## Legal boundaries
- GPL-2.0-or-later applies to VCMI code; fork/modifications require review of redistribution/source obligations.
- The proprietary Heroes III assets (3DO/Ubisoft/GOG or others) must not be committed or packaged publicly; import from user's licensed files.
- Use own demo meshes, colors and UI until an asset owner or lawful import is established.

## Tech references
- https://vcmi.eu/players/Installation_Android/
- https://github.com/vcmi/vcmi
- https://github.com/vcmi/vcmi/blob/develop/docs/developers/Building_Android.md
- https://developers.meta.com/vr/documentation/spatial-sdk/hybrid-apps-overview/
- https://docs.unity.cn/Packages/com.unity.xr.meta-openxr%402.0/manual/project-setup.html
