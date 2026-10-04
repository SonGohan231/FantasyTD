# FantasyTD Unity Project Context

Last verified: 2026-10-04

## Repository state

- Active ticket: T0013 — MAP1 production polish and release evidence.
- Working branch: `codex/t0013-map1-production-polish`.
- Baseline commit: `f67a032a56d32906b015f122de19801d027995df`.
- Unity version: `6000.0.26f1`.
- Render/UI stack: built-in 2D rendering and uGUI.
- Input: legacy Unity Input API with mouse/touch handling in runtime scripts.
- Assembly layout: default Unity assemblies; no asmdefs are committed.

## Scene and runtime architecture

- Product flow: main menu -> campaign map selection -> MAP1 -> victory/defeat -> replay or campaign return.
- Scenes are generated and maintained by editor setup scripts rather than committed as authored `.unity` files.
- `MapABootstrap` creates the MAP1 camera, authored map composition, route endpoints, planting spots, gameplay session, and HUD.
- `GameSession` owns the MAP1 economy, base health, waves, enemies, planting, HUD, and end state.
- `MobileHud`, `GameplayNavigationHud`, and `TowerPanelHud` build screen-space uGUI at runtime.
- `HiggsfieldSpriteFactory` loads approved source textures from `Resources/Higgsfield` and creates runtime sprites/cutouts.
- Production balance and coordinates live in `Balance`, `TowerUpgradeBalance`, and `MapAData`; presentation work must not duplicate or silently alter their values.

## Validation and build path

- No active Unity MCP bridge or local Unity executable was available at this checkpoint.
- Unity compilation, PlayMode validators, Game View captures, and Android builds run through the repository's GitHub Actions Unity executor.
- The release evidence contract requires real Game View screenshots, machine-readable test results, Android build metadata with SHA-256, and emulator/device smoke evidence for the exact APK.
- Existing run `37196919539` is a baseline only: visual acceptance and full campaign passed, while several older validators failed or used stale expectations. It does not establish a release-ready build.
- Existing candidate APK `FantasyTD_MAP1_test_f67a032.apk` has SHA-256 `de77755b516d493dd036259902a2c7913213eaad72064962c3c2e208669dc06c`; it is not accepted without matching device smoke evidence.

## Current confirmed defects

- The enemy counter overlaps the central wave pill in all validated mobile layouts.
- Camera fitting mixes an inset-adjusted aspect ratio with a full-screen camera viewport, causing invalid containment/centering results at unsupported aspect ratios.
- RGB source illustrations are cut out at runtime using a single averaged background colour; textured paper backgrounds can survive as opaque rectangular blocks.
- The visual acceptance validator does not yet capture every screenshot required by the product completeness gate, especially damaged enemy HP, the tower management panel, and victory/defeat states.

## Change constraints

- Preserve MAP1 route coordinates, planting coordinates, wave definitions, economy values, enemy stats, and tower stats unless a separate gameplay ticket authorizes a change.
- Preserve validator-facing object names such as `Button_GRAJ`, `MapButton_MAP1`, `Button_START`, `PlantOption_*`, `Button_ULEPSZ`, and `Button_SPRZEDAJ`.
- Prefer small, reversible commits and validate in this order: compile, map integrity/layout, gameplay gates, EditMode/PlayMode XML, visual acceptance, Android build, exact-APK device smoke.
- Treat Mission Control as a reporting surface. Only verified artifacts and measured results may be marked PASS or READY.
