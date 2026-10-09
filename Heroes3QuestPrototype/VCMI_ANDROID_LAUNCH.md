# VCMI Android integration — operational launch bridge

**Scope:** this phase integrates the independently installed **real VCMI Android application** into the VR prototype via a selectable spatial portal. Pressing the portal triggers Android `PackageManager.getLaunchIntentForPackage("is.xyz.vcmi")` and `startActivity`. This runs original VCMI in **its own Android task/panel**; it is not game-state integration and does not turn VCMI's world or battles into 3D.

## Evidence
- Package ID confirmed by https://f-droid.org/packages/is.xyz.vcmi/
- Installation https://vcmi.eu/players/Installation_Android/
- Android launch mechanism https://developers.meta.com/vr/documentation/unity/ps-system-deep-linking/
- Android 11 package visibility declared via Unity Gradle postprocessor `VCMIQueryManifestPatcher.cs`, logged as `VCMI_VISIBILITY_PATCHED`.
- A Unity build passing proves compilation and manifest generation; Quest-specific app-switching needs hardware verification.

## Usage
1. Install both APKs on Quest 3 in developer mode: (a) Heroes3Quest prototype APK from our GitHub Actions (b) official VCMI 1.7.5 ARM64 APK from vcmi.eu / F-Droid.
2. Open VCMI separately, import legal Heroes III Complete or Shadow of Death assets via GOG offline installer (`.exe` + `.bin`) or your folders `Data/`, `Maps/`, `Mp3/`.
3. Open Heroes3Quest VR tabletop prototype.
4. Point controller at the blue **OPEN REAL VCMI** portal and trigger. VCMI should open as an Android 2D application if Horizon OS permits this app transition.
5. If launcher returns "missing", confirm VCMI package ID via `adb shell pm list packages | grep vcmi`.
6. If Quest OS refuses to switch apps, launch VCMI from Quest's Library/Unknown Sources: the installed game still works independently; app-switching feature requires Quest testing.

### Explicit current limits
- No VCMI game state, saved games or battles are transferred into our procedural VR map.
- Our VR map/battle remains demonstrative placeholder. Source code is independent of VCMI proprietary asset files.
- Launching a different Android package may exit, hide or pause our immersive VR app; Quest behaviour requires physical tests.
- End-to-end integration is *not* proven until launch and gameplay verified in headset.

## Required next integration
To render real Heroes III as VR diorama, create a VCMI engine-supported snapshot/action bridge (JNI/C++ or IPC) with versioned capability discovery, legality checks, event sequencing, state authority, save/load parity, and then render map/town/battle snapshots in OpenXR. The current Android launcher does not provide this bridge.
