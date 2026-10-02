# MVP Technical Design — MAP-A Android v2

## 1. Scope

This document describes the current technical vertical slice only.

The current MVP milestone is complete when MAP-A can be:
- opened in Unity,
- compiled without blocking errors,
- played in Play Mode,
- controlled with touch-style interaction,
- completed or lost,
- built to Android APK,
- smoke-tested on an Android target when available.

No campaign expansion is part of this milestone.

## 2. Unity project

Unity project root:

`UnityProject/`

Declared Unity version:

`6000.0.26f1`

Current package manifest explicitly includes:
- `com.unity.ugui`
- built-in Unity modules for animation, audio, image conversion, IMGUI, JSON, Physics2D, UI and UIElements.

Current project has no evidence yet of:
- a committed `packages-lock.json`,
- assembly definition files,
- Unity Test Framework tests,
- CI build workflow.

These are not automatically defects at this project size, but they are audit/roadmap items.

## 3. Current code layout

First-party code is under:

`UnityProject/Assets/FantasyTD/`

Key areas:

### Data
`Data/MAP_A_ANDROID_V2.json`

Contains a serializable/reference form of map data and current prototype balance values.

### Runtime scripts
`Scripts/`

Current responsibilities include:
- `MapAData.cs` — MAP-A geometry and fixed spot definitions
- `GameTypes.cs` — plant/enemy enums, display names, costs
- `GameSession.cs` — authoritative runtime session ownership
- `Economy.cs` — Essence
- `BaseHealth.cs` — base HP and defeat trigger
- `EnemyUnit.cs` — route movement, health, resistance, slow/root
- `WaveDirector.cs` — 6 wave definitions and sequencing
- `Targeting.cs` — First-target selection
- `SeedProjectile.cs` — projectile movement/hit
- `PlantBehaviours.cs` — current four plant behaviors
- `PlantingSpot.cs` — planting-slot state
- `PlantingSystem.cs` — touch/mouse spot selection and build actions
- `MobileHud.cs` — runtime-generated mobile UI and radial menu
- `SafeAreaFitter.cs` — safe-area anchors
- `MobileCameraFitter.cs` — map framing for aspect ratios
- `FantasyVisualFactory.cs` — temporary runtime-generated organic placeholders
- `RuntimeArt.cs` — procedural prototype sprites/UI primitives
- `OrganicBob.cs` — lightweight placeholder animation
- `MapABootstrap.cs` — runtime creation of map/camera/spots/session

### Editor scripts
`Editor/`

- `MapAEditorSetup.cs`
  - creates/opens MAP-A scene
  - validates path length and spot count
  - configures Android landscape/player identifier
  - ensures scene is in Editor Build Settings

- `AndroidBuildMenu.cs`
  - attempts Android target switch
  - builds APK to:
    `Builds/Android/FantasyTD_MAP_A_v2.apk`

## 4. Scene model

The current repository does not contain a committed MAP-A `.unity` scene before Unity Editor executes the editor setup.

`MapAEditorSetup` is designed to generate:

`Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`

when Unity loads and the scene does not exist.

This must be validated in T0001/T0002.

Do not assume scene generation works until observed in Unity.

## 5. Runtime bootstrap

MAP-A currently uses a runtime bootstrap approach:
- creates camera,
- creates map background and road from waypoints,
- creates scenery placeholders,
- creates 12 planting spots,
- creates a `GameSession`,
- attaches economy, base, wave, planting, and HUD systems.

This is intentionally simple for the vertical slice.

Do not replace it with a large prefab/DI framework during MAP-A stabilization.

## 6. Input

Current input implementation uses:
- `Input.touchCount` / `Input.GetTouch` on mobile,
- mouse fallback in Editor/Standalone,
- UGUI `EventSystem`,
- `StandaloneInputModule`.

This must be validated against the actual Unity 6 project input settings.

Do not migrate to the newer Input System during an unrelated ticket.

If the current approach blocks Android touch, create a dedicated input migration ticket.

## 7. Mobile UI

UI is runtime-generated with UGUI.

Current target:
- CanvasScaler reference 1920×1080
- safe-area fitting
- large touch targets
- top status HUD
- wave-start button
- radial build menu
- end-state panel

Mandatory validation:
- 16:9
- approximately 20:9
- a notched/safe-area case if available

Edge planting spots require special attention because radial-menu clamping is computed at runtime.

## 8. Combat

### Pędostrzał
- target priority: First
- range 3.2
- 22 physical damage
- 1.0 s cooldown
- projectile speed 7

### Kwiat Esencji
- +20 Essence per 20 s

### Dębowy Strażnik
- range 2.4
- applies root
- 8 root damage
- 5 s cadence
- boss root duration reduced

### Mroźny Pąk
- range 3.2
- 18 frost damage
- 1.0 s cooldown
- 35% slow for 2.5 s
- boss slow limited

## 9. Enemy movement

Enemies move through the fixed waypoint sequence in `MapAData.Waypoints`.

Current movement is direct segment-to-segment movement.

There is no NavMesh/pathfinding requirement for MAP-A.

Do not introduce navigation middleware for this milestone.

## 10. State ownership

Intended current ownership:

`GameSession`
- owns references to Economy, BaseHealth, WaveDirector, PlantingSystem, MobileHud
- tracks active enemies
- owns victory/defeat state

This should remain the central runtime state boundary for MAP-A unless a confirmed defect requires a small change.

## 11. Android build

Current editor command:

`Tools → Fantasy TD → Build Android APK`

Expected artifact:

`UnityProject/Builds/Android/FantasyTD_MAP_A_v2.apk`

Build prerequisites may include:
- Android Build Support
- Android SDK
- Android NDK
- OpenJDK

Their availability is currently unverified.

## 12. Definition of Done for MAP-A milestone

The milestone is ready only when there is direct evidence that:

- Unity import finishes
- C# compile has no blocker errors
- MAP-A scene exists and opens
- MAP-A validation passes
- 12 planting spots exist
- radial menu works
- all 4 plants work
- all 4 basic enemies work
- boss works
- 6 waves work
- Essence updates correctly
- base damage works
- victory works
- defeat works
- restart works
- 16:9 and 20:9 UI are usable
- safe area is acceptable
- Android build succeeds
- APK exists
- if device tooling is available, APK launch/smoke test passes

Unrun checks must remain NOT RUN or BLOCKED.
