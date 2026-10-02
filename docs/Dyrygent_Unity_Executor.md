# Dyrygent Unity Executor

## Purpose

Dyrygent Unity Executor is the runtime-verification layer for Fantasy TD.

Codex edits repository files and implements tickets. Unity Executor is the source of truth for claims that require a real Unity process: import, compilation, Build Settings state, Unity tests and Android APK generation.

## Architecture

```text
Phone / ChatGPT / Dyrygent
        |
        +-- Codex -> repository implementation
        |
        +-- GitHub push / request file
                |
                v
        GitHub Actions
                |
                v
        self-hosted runner: dyrygent-unity
        Google Compute Engine / Ubuntu 22.04
                |
                v
        Unity 6000.0.26f1 + Android SDK/NDK/JDK
                |
                +-- baseline / compile
                +-- Unity tests
                +-- Android APK build
                |
                v
        logs + reports + artifacts
```

Cloud Run remains a lightweight orchestrator. Unity runs on a persistent Google Cloud VM because Unity Personal is activated through Unity Hub on that machine.

## Why self-hosted

Unity Personal is not copied into GitHub Secrets.

The same Google Cloud VM keeps:
- Unity Hub login,
- Unity Personal activation,
- exact Unity Editor installation,
- Android toolchain,
- GitHub self-hosted runner service.

The VM can be stopped between development sessions to reduce compute cost. Its persistent disk keeps the installation and local state.

## Workflow

Workflow file:

`.github/workflows/unity-executor.yml`

Runner labels:

`self-hosted, linux, x64, dyrygent-unity`

Modes:

- `baseline` — import/compile project and run `FantasyTD.CI.UnityExecutorCi.Baseline`.
- `tests` — run Unity EditMode and PlayMode tests.
- `android-build` — build `Builds/Android/FantasyTD_MAP_A_v2.apk`.

The workflow calls the real Editor executable:

`/opt/unity/editors/6000.0.26f1/Editor/Unity`

## Dispatch without a Cloud Run GitHub token

Dyrygent/ChatGPT can request any executor mode by updating:

`.dyrygent/unity-request.json`

with a new `requestId` and one of:
- `baseline`
- `tests`
- `android-build`

The commit triggers GitHub Actions. The Cloud Run MCP backend does not need a GitHub PAT.

## One-time setup

Create Google Cloud VM `dyrygent-unity` using the phone-only bootstrap package.

On the VM:
1. connect using Chrome Remote Desktop,
2. log in to Unity Hub,
3. activate Unity Personal,
4. ensure Unity 6000.0.26f1 + Android Build Support + SDK/NDK/JDK are installed,
5. register the VM as the GitHub self-hosted runner for `SonGohan231/FantasyTD` with label `dyrygent-unity`.

No `UNITY_LICENSE`, `UNITY_EMAIL` or `UNITY_PASSWORD` GitHub Actions secrets are required in this architecture.

## Evidence rules

A Dyrygent/Codex report may say:

- Unity compiled — only when the Unity workflow successfully executes the real Editor.
- MAP-A scene exists — only when the baseline report says `sceneExists=true`.
- MAP-A is in Build Settings — only when `sceneEnabledInBuildSettings=true`.
- Unity tests passed — only when the relevant test result exists and the workflow passed.
- APK exists — only when `android-build.txt` says `apkExists=true` and the APK artifact is uploaded.

A queued, failed, cancelled or unrun GitHub Action is not PASS.

## Fantasy TD project

Unity root:

`UnityProject/`

Expected editor:

`6000.0.26f1`

Expected scene:

`Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`

Expected APK:

`UnityProject/Builds/Android/FantasyTD_MAP_A_v2.apk`
