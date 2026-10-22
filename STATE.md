# STATE — FantasyTD / Forest01 integration
Date: 2026-10-22. Status: LOCAL EDITOR AND PLAY MODE TESTED; RENDER/APK NOT VERIFIED.

## Working baseline
- New clean worktree: /home/adambrdon/Repos/FTD/FantasyTD-Forest01-Synced
- Branch: codex/forest01-integrated-20261022 (forked from PR #34 commit 03ed60f).
- Covers both PR #33 (terrain) and PR #34 (portable CI/gates).
- Main branch remains unchanged. Existing worktrees were not touched.
- Previous local dirty changes backed up at /home/adambrdon/Repos/FTD/.sync-backups/FantasyTD-before-sync-20261022-1945.tar.gz.
- Primary project core: docs/CORE_FantasyTD_PELNA_EKSTRAKCJA_KR_PVZ_2026-10-22.md
- Visual scope: docs/FT2_Forest01_Approved_Production_Spec.md
- Geometry scope: docs/FT2_Forest01_28x20_Layout.md

## Actual Unity evidence
- Unity 6000.0.26f1 runs through FEX and reports x86_64 guest architecture with the accelerated Virtio-GPU Venus (Mali-G52) Vulkan device.
- The fixed terrain smoke generated and reopened `Forest01_TerrainTest.unity` (278642 bytes), retained all 10 build-site MonoBehaviours and selected a real site.
- Local camera capture remains FAILED: Unity aborts in `Camera.Render` with `Missing Vulkan framebuffer attachment image` inside the FEX/Venus Vulkan path. Both the original ReadPixels path and AsyncGPUReadback reach the same driver failure before a PNG can be produced.
- The first local Play Mode attempt failed because the Unity Licensing Client timed out before Package Manager registered UI/Audio. A fresh run restored all 12 packages, compiled successfully, opened Forest01 and entered real Play Mode. `UnityProject/Logs/forest01-playmode-integration.txt` reports `status=PASS`, `playModeEntered=True`; the launcher returned exit code 0. This validation did not capture a frame.
- A separate local Play Mode camera-render attempt is in progress; no PNG has been verified yet. x86_64 GitHub CI remains the intended render/APK path.

## Implemented Forest01 integration
- `StarterLevel` now adapts the approved 28x20 terrain route and exactly B01-B10 into the playable scene.
- Four defenders, six waves, four ordinary enemy profiles plus Król Cierni, 300 starting Essence and 20 base HP are configured.
- `BattleSimulator` owns deterministic level 1-3 upgrades, costs, damage/range/cooldown/income effects and upgrade events.
- `ForestBattlefield` supports touch selection of empty or occupied sites, build, visible upgrade cost/level, wave flow, economy, pause, victory, defeat, responsive HUD, VFX and generated audio cues.
- `Forest01PlayModeSmoke` verifies the actual scene, 10 sites, touch-sized controls, audio, build, upgrade, damage/kills, six-wave victory and unopposed defeat.
- The pre-existing `ft2-android-apk.yml` job uses an unauthorized `dyrygent-unity` runner and has been disabled. New `ft2-android-apk-hosted.yml` targets GitHub-hosted Ubuntu x86_64, checks Edit/Play Mode, requires a real camera PNG, builds Android IL2CPP ARM64 APK and uploads artifacts. It is configured but has not run.

## Android host evidence
- Local Unity installation: `/home/adambrdon/Unity/Hub/Editor/6000.0.26f1/Editor/Unity` (6000.0.26f1).
- Local Android Build Support is absent: `Editor/Data/PlaybackEngines/AndroidPlayer` does not exist.
- Existing ComputerMCP JDK: `/home/adambrdon/ComputerMCP/workspace/.computermcp/jdk`, Temurin 17.0.20.1+1, Linux aarch64.
- Existing SDK CLI only: `/home/adambrdon/ComputerMCP/workspace/.computermcp/android-sdk/cmdline-tools/latest`, revision 22.0. No installed platforms, build-tools, NDK or ADB were found.
- No local Android components were installed or duplicated. The authorized x86_64 GitHub runner must supply the matching Unity Android module.

## GitHub branches reviewed
- main 17689b8 — no merge.
- PR #33 155b95c — separate 28x20 terrain code and CORE.
- PR #34 03ed60f — advisory and portable quality gates, not Unity proof.
- PR #28 b85a1fe and PR #31 8110d57 — separate art/import stack, not integrated.
- Local historical MAP1 91a625a and terrain 604200f retained.

## Known boundaries and next actions
1. Finish the active local camera-render run and record its exact PASS/FAIL log and any PNG.
2. Commit the tested implementation without staging the user-owned `ssss.txt desktop` file or unrelated generated changes.
3. Reconcile the diverged remote integrated branch without force push, publish the exact commit, observe the hosted x86_64 workflow, and download its genuine PNG/APK/log artifacts. GitHub Unity license secrets may need to be configured.
4. If ADB and a device become available, install, launch and perform physical touch/audio/performance verification. No device is currently detected locally.
5. Do not merge automatically. Replace remaining procedural presentation only under a separately approved art ticket after the playable slice passes.
