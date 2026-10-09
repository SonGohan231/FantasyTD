# Heroes III Quest — VR tabletop technical prototype

**Status: Unity/OpenXR demonstration APK successfully built in GitHub Actions; real VCMI Android application launch handoff is implemented in source and pending new build/device test. This is NOT yet a state-integrated Heroes III VR game and has NOT been tested on physical Quest 3.** Built inside a separate folder and unmerged branch to avoid changing FantasyTD.

## Implemented in source
- Procedural elevated hex-based tabletop terrain; no commercial 3DO/GOG art or sounds.
- XR HMD position and right-controller ray/trigger via `UnityEngine.XR`, mouse fallback.
- Hero movement with pathfinding, inaccessible mountain hexes, red enemy encounter.
- Small independent tactical demo board and sample melee-range victory.
- Unity 6 scene generator, Android ARM64 player configuration, editor APK build command.
- Five NUnit EditMode tests for tested axial geometry/pathfinding.

**The encounter is placeholder code**, not Heroes III combat or AI. The source never contains VCMI binaries or original commercial assets. A working demonstration APK was built in CI run #37884401996; the new launcher integration is awaiting independent CI validation.

## Building the prototype
1. Install **Unity 6000.0.26f1** with **Android Build Support**, **OpenJDK**, Android SDK and NDK (or update the project version and packages after testing).
2. Open **only the `Heroes3QuestPrototype/` subfolder** in Unity Hub as a separate project, not the parent FantasyTD project.
3. In Unity, allow Package Manager to resolve `Packages/manifest.json`. If incompatibilities arise, resolve exact package versions before continuing.
4. Use `Tools → Heroes III Quest → Create Prototype Scene`. It generates `Assets/Scenes/HeroesQuestBootstrap.unity` and adds it to Build Settings.
5. Use `Tools → Heroes III Quest → Configure Android Player`.
6. In **XR Plug-in Management → Android**, enable **OpenXR**. In **OpenXR → Android**, enable **Meta Quest Support** and **Oculus Touch Controller Profile**. Select Vulkan if supported by the project, use Android ARM64, and verify any OpenXR project validation warnings. These settings are *not* automatically enabled by the present build menu.
7. Under **Player → Other Settings**, verify **Active Input Handling: Both** (desktop fallback uses `UnityEngine.Input`).
8. Switch build platform to **Android** or, on Unity 6.1+, the Meta Quest build platform. Run EditMode tests from Test Runner.
9. Use `Tools → Heroes III Quest → Build Quest APK (requires configured OpenXR)`. If successful, APK is written to `Builds/Heroes3Quest-Prototype.apk` within the Unity project.
10. Install via Meta Quest Developer Hub / `adb install -r Builds/Heroes3Quest-Prototype.apk` on an authorized, developer-mode Quest 3, and test in headset.

Unity build, Quest input, CPU/GPU thermal performance and APK installation **remain unverified**. Do not publish a release based solely on reading the code.

## How to play this prototype
Look toward the miniature table and aim the right controller at green terrain; pull trigger to move. Enter the red patrol to open a placeholder battle. Move adjacent to the enemy and select its red hex to win. In the Unity desktop Game view, the mouse can select hexes.

## VCMI launch handoff — implemented in source, awaiting device test
The spatial **OPEN REAL VCMI** portal invokes the separately installed VCMI Android package `is.xyz.vcmi`. This is a real Android app handoff (if Quest supports app switching), **not** game-state / Unity rendering integration. See [VCMI_ANDROID_LAUNCH.md](VCMI_ANDROID_LAUNCH.md), then [VCMI_INTEGRATION.md](VCMI_INTEGRATION.md) for the unsolved engine-state bridge.

## Primary sources
- https://github.com/vcmi/vcmi
- https://vcmi.eu/players/Installation_Android/
- https://github.com/vcmi/vcmi/blob/develop/docs/developers/Building_Android.md
- https://developers.meta.com/vr/documentation/spatial-sdk/hybrid-apps-overview/
- https://docs.unity.cn/Packages/com.unity.xr.meta-openxr%402.0/manual/project-setup.html

## QA acceptance gates
- **S0 Source:** files committed on isolated branch — yes.
- **S1 Import:** Unity compiled baseline through GitHub Actions #37884401996 — PASS for that commit.
- **S2 Logic:** five Editor NUnit tests passed in GitHub Actions #37884401996 — PASS for that commit.
- **S3 XR:** OpenXR Android project validation and APK build passed; *visual/stereo/controller runtime* remains pending device or simulator test.
- **S4 Device:** install both APKs, switch apps, play battle, suspend/resume on Quest 3 — pending physical headset test.
- **S5 Real game:** separate VCMI app handoff implemented; VCMI map/towns/battles/saves inside immersive VR are **not implemented**.
