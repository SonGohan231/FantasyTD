# Garden Defense VR — isolated Quest 3 build candidate

This is a PRIVATE proof-of-build standalone 6DoF tabletop tower-defense prototype, not the original Plants vs Zombies APK port. Original proprietary art, code and assets are not included. Contains a 9x5 lawn, controller ray, five plant types, zombies, sunlight, five waves and endgame.

Runs from Unity 6.0.26f1 in QuestGardenVR/ with XR Management and OpenXR. Main branch of FantasyTD is untouched. Branch quest3/pvz-garden-vr-build isolated to avoid modifying FantasyTD gameplay. Do NOT merge as a production feature.

Run in Unity: open QuestGardenVR as a separate project, import packages, call QuestGardenVR.QuestBuilder.Build from CLI or self-hosted GitHub Actions.
The CI job uses the existing self-hosted dyrygent-unity runner; if it is offline or lacks Android SDK, a build cannot happen.

Acceptance: APK must be non-empty and valid ZIP, Android OpenXR loader & Oculus Touch profile enabled, run on physical Quest 3 with stable stereo rendering and 6DoF input. Until evidence of that exists, not a finished VR game.

Next required work for the ORIGINAL PvZ APK: establish lawful local native interop with its Cocos2d-x engine, verify structure ABI and state sync and render actual 3D assets. This branch does not integrate the original binary.
