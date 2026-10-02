# Repo Current State

Last updated: 2026-10-02

Evidence level:
- Static repository inspection: AVAILABLE
- Unity Editor compilation: NOT RUN in this handoff environment
- Play Mode: NOT RUN
- Android build: NOT RUN
- Device smoke test: NOT RUN

## Current milestone

MAP-A Android v2 vertical slice.

## Current active ticket

T0001 — Unity Project Baseline.

## Repository layout

Top level:
- `AGENTS.md`
- `README.md`
- `docs/`
- `ProjectData/`
- `UnityProject/`

Unity root:
- `UnityProject/`

## Declared Unity version

From `UnityProject/ProjectSettings/ProjectVersion.txt`:

`6000.0.26f1`

The file also contains an all-zero revision field in the generated project handoff. Treat exact editor-revision compatibility as unverified until Unity opens the project.

## Packages

`UnityProject/Packages/manifest.json` currently declares:
- `com.unity.ugui`
- built-in Unity modules

No committed `packages-lock.json` was present in the source handoff at the time this document was generated.

## First-party source

Located under:

`UnityProject/Assets/FantasyTD/`

Observed code includes:
- map data/geometry
- runtime bootstrap
- touch/mouse planting selection
- radial UGUI menu
- four plant behaviors
- four basic enemy types
- one boss type
- six wave definitions
- Essence economy
- base HP
- victory/defeat
- mobile camera fitting
- safe-area fitting
- editor scene/bootstrap tooling
- Android build editor command

## Implemented in source code, not runtime-verified

The repository contains code intended to provide:

### MAP-A
- 24 × 14 RU logical bounds
- route length target approximately 46.211 RU
- 12 planting spots
- portal-style spawn
- tree-style base

### Plants
- Pędostrzał
- Kwiat Esencji
- Dębowy Strażnik
- Mroźny Pąk

### Enemies
- Chochlik
- Wichrowy Skrzat
- Ogr Zarośli
- Żelazny Goblin
- Król Cierni

### Waves
Six hard-coded prototype waves in `WaveDirector.cs`.

### Mobile
- touch input path
- Editor mouse fallback
- safe area
- CanvasScaler
- landscape autorotation intent
- Android build menu item

## Scene state

No committed MAP-A `.unity` scene existed in the source handoff before Unity Editor execution.

`MapAEditorSetup.cs` is intended to create:

`Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`

on Editor load if missing, and to add it to Build Settings.

This behavior is NOT YET VERIFIED in Unity.

## Testing

No first-party Unity automated tests were observed in the current source handoff.

This is acceptable for the first vertical-slice baseline but should be revisited after the project compiles and the architecture stabilizes.

## Build state

Editor code exists to build:

`Builds/Android/FantasyTD_MAP_A_v2.apk`

Actual Android build success is NOT VERIFIED.

Android Build Support / SDK / NDK / JDK availability is UNKNOWN.

## Input state

Current code uses legacy Unity `Input` touch APIs plus UGUI `StandaloneInputModule`.

Actual project input settings and Unity 6 Android behavior are NOT VERIFIED.

Do not migrate input systems until there is evidence that migration is necessary.

## Architecture state

Current architecture is intentionally small and centralized around `GameSession`.

No assembly definitions are present in the source handoff.

No architecture rewrite is currently justified.

## Current known blockers

None confirmed because Unity Editor has not yet been run against this handoff.

See:
`docs/Known_Issues_And_Followups.md`

## Next recommended action

Complete T0001 only.
