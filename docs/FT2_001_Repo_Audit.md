# FT2-001 — First-party repository audit and reuse decision
Date: 2026-10-09 · Status: DOCUMENTED, independent runtime verification outstanding
Branch under audit: main at tree SHA `17689b8c5154d434b07df1e58682fc8df7ec25d3` (not a new Unity test)
New product GDD: `docs/FantasyTD_2_0_GDD.md` on `design/fantasytd-2-rebuild-20261009`.

## Direct observations (via connected GitHub)
- Repo: `SonGohan231/FantasyTD`, private, writable, default `main`.
- Main tree: 54 tracked blob files; 31 C#; 15 Markdown; three JSON, one Unity CI workflow, one XLSX balance workbook, one Unity version text file. **No tracked raster sprite, source painting, imported audio, `.unity` scene, `.prefab`, animation clip or animator controller** at this main-tree snapshot. Absence from GitHub does NOT establish absence from historical external Higgsfield/media accounts.
- Unity version `6000.0.26f1`; Unity Packages manifest includes uGUI 2.0.0, modules.animation, audio, physics2d. No full 2D lighting/art pipeline is currently evidenced.
- Legacy gameplay under `UnityProject/Assets/FantasyTD/Scripts/`: `GameSession`, `WaveDirector`, `EnemyUnit`, `PlantingSystem`, `PlantBehaviours`, economy/base/HUD. Existing MAP-A single road, 12 spots, 4 plants, 4 normal enemy types and one boss, six waves.
- `WaveDirector.cs` hardcodes `TotalWaves => 6` and a `switch` returning each wave composition. This is useful as a reference encounter but cannot author 30 level datasets without redesign.
- `RuntimeArt.cs` creates a 1×1 white pixel Sprite, a procedural 96×96 circular Sprite and runtime UGUI controls. `FantasyVisualFactory.cs` assembles creature/plant shapes via `RuntimeArt.SpriteObject`, not imported illustrated art.
- `GameSession.cs` couples Unity MonoBehaviour gameplay state with HUD calls, Unity instantiation and enemy List. Move to explicit testable domain state for V2, while reusing verified behavior as reference.
- `AndroidBuildMenu.cs` calls `MapAEditorSetup.OpenMap()` and builds the MAP-A-only scene to `Builds/Android/FantasyTD_MAP_A_v2.apk`. A V2 campaign must introduce V2 bootstrap/build config; do not silently repurpose historical MAP-A build menu.
- Editor validators exist for geometry, planting, behaviors, waves, boss/end-states, screen layout, Android build; a Unity Executor GitHub workflow is checked in.
- `docs/Repo_Current_State.md` (last updated 2026-10-02) records successful Unity Executor PlayMode stages and a real prior MAP-A Android APK build (run `36999815943`, SHA256 `13029c48866753493aa23fe7aab321c2f94dd7e76e43de49791c5c243ccc8bb5`). These are **historical evidence for the old prototype**, not new 2026-10-09 tests and not proof the APK was installed on a physical device.

## Reuse / replace decision matrix
| Existing part | Decision | Reason |
|---|---|---|
| Repo history and existing Unity version | KEEP | Avoid losing working baseline and evidence. |
| Existing `UnityProject` root and CI Unity Executor | KEEP + EXTEND | Reproducible Unity 6/Android validation foundation; add new V2 profile + test jobs. |
| MAP-A world values/geometry | PORT as optional level-design reference | New campaign authored from content definitions, no forced legacy topology. |
| Existing unit behaviours, economy formulas | REFERENCE/TEST | Individual rule ideas are helpful; refactor into model-state with fixed-step deterministic simulation and authoritative content data. |
| Legacy uGUI and touch/radial menu | REUSE patterns after QA | Safe-area/touch handling valuable, visual implementation not release-ready. |
| `RuntimeArt` / `FantasyVisualFactory` | REMOVE from V2 release assets only | Useful developer placeholders; must be replaced with licensed coherent sprites/animation. Retain historical code under MapA. |
| Legacy `WaveDirector` switch | REPLACE in V2 | 30 levels need versioned, validated wave definitions and encounter authoring. |
| Existing build menu | KEEP legacy, ADD separate V2 build target | Avoid false positive produced by building only MAP-A. |
| Excel balance workbook | DESIGN REFERENCE | Actual game balance must be derived from runtime telemetry/simulations and real human testing. |
| GDD/old tickets | ARCHIVE; new scope docs | User explicitly overrides previous MAP-A-only scope; old evidence remains accessible. |

## Precise entry points to code M1
1. Add `UnityProject/Assets/FantasyTD/V2/Runtime/` domain types with no required UnityEngine imports: `GameState`, `CombatClock`, `LevelDefinition`, `WaveDefinition`, `TowerDefinition`, `EnemyDefinition`, `StatusEffectDefinition`, deterministic RNG, route coordinates and deterministic combat tick. Avoid changing existing `MapA` namespace.
2. Introduce `V2/Unity/` adapters for scene load, visual prefabs, sprites/animation, object pools, audio and touch. Build no content-generation assumptions into simulation.
3. Design Content Manifest (`schema_version`, globally stable IDs, strict validation, localization keys, asset provenance) and a self-consistent L01 fixture.
4. Implement V2 tests for stable seeds, two different valid strategies, placement/cost, damage, status interaction, wave completion, defeat/restart and save deserialize/migration. Real Unity EditMode/PlayMode tests before calling a feature complete.
5. Wire original hand-painted art, sprite atlases/animation states, hit/audio feedback, 16:9 and 20:9 touch HUD. Single production-quality L01 only; then expand content in a data-driven pipeline.
6. Launch GitHub Actions Unity Executor on exact V2 HEAD for baseline/tests and Android APK, inspect logs and artifacts and independently test launch on device/emulator if available.

## Risk register
R1 — Original production art is missing from tracked Git files. Highest risk to professional look; asset generation may cost credits and requires cost approval. No placeholder circles in release.
R2 — Hard-coded MAP-A waves, base/viewport and MonoBehaviour coupling cannot form a campaign without a new data domain. Preserve legacy code and add V2 architecture.
R3 — Current design target 30 maps/24 enemies/6 bosses is a **scope target**, not a deliverable yet.
R4 — The Agent OS routing preflight returned multiple semantic misassignments (art to evidence judge, content to mission-architect); require explicit role checks and honest blocked status before executable work.
R5 — Real device installation, FPS, save/restore and game-feel remain unverified for new V2. Report builds and gameplay separately.
R6 — Historical `docs/Repo_Current_State.md` references previous run artifacts; do not assume old APK still downloadable without querying current GitHub artifacts.

## Evidence and scope
Performed: GitHub tree/file inspection; this audit report; branch creation.
Not performed: new Unity import or compilation, emulator install, new APK build, unit tests, rendered visual review, asset generation. Therefore the audit does not certify the new game.
Acceptance: FT2-001 static code inventory PASS; fresh Unity baseline and agent-independent audit review remain FOLLOW-UPS.

## Next
Begin FT2-002 V2 architecture ticket on its own branch after independent review; use versioned structured source content and deterministic test cases first. Do not merge code into `main` without real Unity verification.
