# Evidence and known limitations — 2026-10-09

- Verified by GitHub connector: branch `feature/heroes3-quest3-prototype-20261009` exists and source files committed.
- Untested: package restore, Unity C# compilation, Unity Editor execution, OpenXR initialization, frame rate, APK signing/build, real Quest operation.
- Unity Editor, Android SDK, ADB and a Quest headset were not available in the execution container.
- The project is *not* HoMM III gameplay; it is a VR interaction and spatial presentation demonstrator.
- Unity XR configuration must be completed before APK generation.
- Known functional limits: no persistence, no town/campaign/AI simulation; tabletop encounter enters battle on tapping enemy regardless of path proximity; placeholder battle lacks enemy turns.
- Android VCMI remains the currently practical path to real Heroes III on a Quest as a 2D panel.
