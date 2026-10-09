# Evidence and known limitations — 2026-10-09

- Verified by GitHub connector: branch `feature/heroes3-quest3-prototype-20261009` exists and source files committed.
- Confirmed in CI run #37884401996: Unity 6000.0.26f1 import and Editor tests pass, OpenXR Android project setup pass, ARM64 IL2CPP APK build succeeded and artifact uploaded.
- Remaining unverified: physical Quest operation, frame rate, controller ray input, runtime stereo XR, VCMI app transition, native VCMI game-state integration. A later source change adds the VCMI launch portal and must pass its own CI run.
- Unity Editor, Android SDK, ADB and a Quest headset were not available in the execution container.
- The procedural map/battle is *not* HoMM III gameplay. The VCMI portal starts the separately installed original VCMI Android app rather than passing real game state into XR.
- Unity XR configuration must be completed before APK generation.
- Known functional limits: no persistence, no town/campaign/AI simulation; tabletop encounter enters battle on tapping enemy regardless of path proximity; placeholder battle lacks enemy turns.
- Android VCMI remains the currently practical path to real Heroes III on a Quest as a 2D panel.
