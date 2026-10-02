# Dyrygent Unity Executor

## Purpose

Dyrygent Unity Executor is the runtime-verification layer for Fantasy TD.

Codex may edit repository files and implement tickets. The Unity Executor is the source of truth for claims that require a real Unity process: import, compilation, Build Settings state, Unity tests and Android APK generation.

## Architecture

```text
ChatGPT / Dyrygent
        |
        +-- Codex -> repository implementation
        |
        +-- GitHub push / PR
                |
                v
        GitHub Actions
                |
                v
        self-hosted runner: dyrygent-unity
                |
                v
        Unity 6000.0.26f1
                |
                +-- baseline / compile
                +-- Unity tests
                +-- Android APK build
                |
                v
        logs + reports + artifacts
```

Unity command-line automation is intentionally isolated from the Cloud Run MCP service. Cloud Run remains a lightweight orchestrator; Unity runs in CI where the editor, Android modules and build tooling can be installed.

## Workflow

Workflow file:

`.github/workflows/unity-executor.yml`

Modes:

- `baseline` — import/compile project and run `FantasyTD.CI.UnityExecutorCi.Baseline`.
- `tests` — run Unity EditMode/PlayMode tests through GameCI.
- `android-build` — build `Builds/Android/FantasyTD_MAP_A_v2.apk`.

Pushes to `main`, `codex/**` and `dyrygent/**` automatically run the baseline when Unity project files change.

### Dispatch without a backend PAT

Dyrygent/ChatGPT can request any executor mode by updating:

`.dyrygent/unity-request.json`

with a new `requestId` and one of the modes above. The commit itself triggers GitHub Actions. This is the preferred host-action path because the Cloud Run backend does not need a GitHub write token.

## Unity license and runner

Unity runs on the persistent self-hosted runner `dyrygent-unity`.

The Unity Personal license is activated once in Unity Hub on that VM. The GitHub Actions workflow does not require `UNITY_LICENSE`, `UNITY_EMAIL` or `UNITY_PASSWORD` repository secrets.

The runner service must run as the same Linux user that activated Unity Personal, so the Editor sees the local license.


## Evidence rules

A Dyrygent/Codex report may say:

- Unity compiled — only when the Unity workflow reached the executor successfully or test/build output proves it.
- MAP-A scene exists — only when the baseline report says `sceneExists=true`.
- MAP-A is in Build Settings — only when `sceneEnabledInBuildSettings=true`.
- APK exists — only when `android-build.txt` says `apkExists=true` and the artifact is uploaded.

A failed or unrun GitHub Action is not a PASS.

## Fantasy TD project

Unity root:

`UnityProject/`

Expected editor:

`6000.0.26f1`

Expected scene:

`Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`

Expected APK:

`UnityProject/Builds/Android/FantasyTD_MAP_A_v2.apk`
