# Repo Current State

Last updated: 2026-10-02

## Evidence level

- Static repository inspection: AVAILABLE
- Unity Editor import/compilation: **VERIFIED — PASS**
- Unity Executor run: `36979277994`
- Validated branch: `codex/t0001-unity-baseline-v2`
- Validated head before documentation commits: `e6e91bc9a0835128165c0d8539f614163a546cf7`
- Unity Executor artifact: `unity-executor-baseline-36979277994` (artifact ID `11214941802`)
- T0002 Unity geometry validation: **VERIFIED — PASS**
- T0002 Unity Executor run: `36982186182`
- T0002 artifact: `unity-executor-map-a-integrity-36982186182` (artifact ID `11215922210`)
- T0003 Play Mode validation: **VERIFIED — PASS**
- T0003 Unity Executor run: `36986249467`
- T0003 validated head before documentation commits: `7b1ea26bf6d62aa268c21d5f59e3096cb2078dc8`
- T0003 artifact: `unity-executor-mobile-planting-36986249467` (artifact ID `11217666657`)
- T0004 plant behavior validation: **VERIFIED — PASS**
- T0004 Unity Executor run: `36988172698`
- T0004 validated head before documentation commits: `21cac36b4d2902451af65aa9134f4c9d0f4f8c90`
- T0004 artifact: `unity-executor-plant-behaviors-36988172698` (artifact ID `11218576224`)
- T0005 enemy/wave validation: **VERIFIED — PASS**
- T0005 Unity Executor run: `36990840649`
- T0005 validated head before documentation commits: `a11ceba14b55c84d586881ac45baf356c0018daf`
- T0005 artifact: `unity-executor-enemy-wave-flow-36990840649` (artifact ID `11219706930`)
- T0006 boss/end-state validation: **VERIFIED — PASS**
- T0006 Unity Executor run: `36994046983`
- T0006 validated head before documentation commits: `c391c72cf7b23db613e5efd418b5df83aac29e22`
- T0006 artifact: `unity-executor-boss-end-states-36994046983` (artifact ID `11220923927`)
- T0007 mobile-layout validation: **VERIFIED — PASS**
- T0007 Unity Executor run: `36997417372`
- T0007 validated head before documentation commits: `7b99c11501a81a3f1526d57958d7c2d72ab386e4`
- T0007 artifact: `unity-executor-mobile-layout-36997417372` (artifact ID `11222780203`)
- Play Mode: **VERIFIED — PASS for T0003 through T0007 scope**
- Android build: **NOT RUN** (T0008 not started)
- Device smoke test: **NOT RUN**

## Current milestone

MAP-A Android v2 vertical slice.

## Current active ticket

None.

T0001 through T0007 are DONE. T0008 remains PLANNED and was not started.

T0007 acceptance evidence is recorded from real Unity Play Mode in Unity Executor run `36997417372` on branch `codex/t0007-android-ui-device-layout`, validated head `7b99c11501a81a3f1526d57958d7c2d72ab386e4`.

## Repository layout

Top level:
- `AGENTS.md`
- `README.md`
- `docs/`
- `ProjectData/`
- `UnityProject/`

Unity root:
- repository-relative: `UnityProject/`
- executor absolute path: `/opt/actions-runner/_work/FantasyTD/FantasyTD/UnityProject`

## Unity Editor baseline

Actual Editor used by the executor:

`6000.0.26f1 (ccb7c73d2c02)`

The repository's `UnityProject/ProjectSettings/ProjectVersion.txt` declares:

`6000.0.26f1`

The previously placeholder-like all-zero revision in `ProjectVersion.txt` did not prevent the project from opening with the real editor. Exact runtime evidence comes from the Unity log, not from the placeholder revision field.

## Packages

`UnityProject/Packages/manifest.json` declares:

- `com.unity.ugui` `2.0.0`
- `com.unity.modules.animation` `1.0.0`
- `com.unity.modules.audio` `1.0.0`
- `com.unity.modules.imageconversion` `1.0.0`
- `com.unity.modules.imgui` `1.0.0`
- `com.unity.modules.jsonserialize` `1.0.0`
- `com.unity.modules.physics2d` `1.0.0`
- `com.unity.modules.ui` `1.0.0`
- `com.unity.modules.uielements` `1.0.0`

Unity Package Manager resolved the project during the baseline import.

## Compile and Console baseline

Unity Executor mode: `baseline`.

Result:
- workflow conclusion: **success**
- `compileReachedExecutor=true`
- C# compiler errors: **0**
- unique C# compiler warnings: **1**
- the warning was emitted twice during import/compilation

Compiler warning:

```text
Assets/FantasyTD/Editor/MapAEditorSetup.cs(46,13): warning CS0618:
'PlayerSettings.SetApplicationIdentifier(BuildTargetGroup, string)' is obsolete:
'Use SetApplicationIdentifier(NamedBuildTarget buildTarget, string identifier) instead'
```

Non-blocking headless-runner messages were also observed:
- 2 Gtk critical messages during startup,
- ALSA/FM0D audio-device failures followed by `FMOD initialized on nosound output`,
- a licensing access-token update message followed by successful license resolution/update,
- `Curl error 42: Callback aborted` during shutdown.

These messages did not prevent successful import, compilation, baseline execution, report generation, artifact upload, or successful batchmode exit.

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

Play Mode evidence now exists for T0003 mobile planting/radial interaction, T0004 four-plant behavior, T0005 basic-enemy/waves 1–5 runtime flow, T0006 wave 6/boss/victory/defeat/restart, and T0007 16:9/20:9 mobile layout plus synthetic safe-area behavior. Android APK build/toolchain and physical-device behavior remain separately unverified.

## Scene state

The expected scene path is:

`Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`

Real Unity Executor evidence:
- `sceneExists=True`
- `sceneEnabledInBuildSettings=True`
- the scene was generated/imported and opened successfully by the existing editor bootstrap

The scene is generated by Editor code rather than treated here as a manually maintained committed scene. T0001 did not modify scene geometry or gameplay.

## Build Settings state

Unity Executor reported:

- `sceneEnabledInBuildSettings=True`
- `activeBuildTarget=StandaloneLinux64`

T0001 did **not** change the active build target.

## Android tooling state

Unity Executor reported:

- `androidModuleSupported=True`
- Android platform support module registered from the Unity installation
- ADB device scan executed during editor initialization

This proves Android Build Support is available to Unity on the executor.

Not independently verified in T0001:
- exact SDK version/path
- exact NDK version/path
- exact JDK version/path
- Android player build success

Those checks remain for the Android build ticket; T0001 did not switch target or build an APK.

## Git state used for validation

Unity Executor checked out:

- branch: `codex/t0001-unity-baseline-v2`
- head: `e6e91bc9a0835128165c0d8539f614163a546cf7`

The Actions checkout performed a clean/reset before the Unity run. The validation therefore ran from a clean repository checkout of that head. Unity-generated local files were not committed as part of T0001.

## Testing

T0001 intentionally added no tests.

For run `36979277994`:
- baseline/compile job: **success**
- EditMode + PlayMode test job: **skipped**
- Android APK job: **skipped**

This matches the ticket's non-goals.

## T0002 — MAP-A scene and geometry integrity

Status: **PASS**

Real Unity evidence:
- Unity Executor run: `36982186182`
- validated head: `9976edd0643214572b96fe38cb8c5710b029b937`
- Unity version: `6000.0.26f1 (ccb7c73d2c02)`
- executor report status: `PASS`
- validation Console errors/exceptions/asserts: `0`

Acceptance evidence:
- MAP-A scene opened at `Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`
- `Tools → Fantasy TD → Validate MAP-A` logic was invoked and emitted PASS
- logical map size: `24 × 14 RU`
- path length: `46.211 RU`
- all route waypoints are inside logical bounds
- all planting definitions are inside logical bounds
- grass renderer matches the logical 24 × 14 RU bounds
- three road LineRenderers are present, enabled, material-backed, and the primary road geometry matches all waypoints
- portal/spawn visual exists at route start `(0,11)`
- home/base tree visual exists at route end `(24,2)`
- route endpoints span opposite sides of the map
- exactly `12` runtime `PlantingSpot` components exist
- planting IDs are unique and runtime positions/renderers match all 12 definitions
- no Grid/GridLayout component or visible Grid-named renderer is present
- MAP-A camera is orthographic, centered on the logical map and contains all logical bounds in the executor Game View
- executor Game View: `640 × 480`, aspect `1.333`, orthographic size `9.55`

Validation method:
The executor opened the generated MAP-A scene in a real Unity process and invoked the same geometry-building methods used by `MapABootstrap` (`BuildCamera`, `BuildMap`, `BuildPlantingSpots`) in editor validation context. This intentionally validates scene/geometry without entering gameplay Play Mode, mobile UI interaction, waves, combat, or Android build.

Compiler/runner notes:
- C# compiler errors: `0`
- existing warning remains: `CS0618` in `MapAEditorSetup.cs:46`
- non-blocking headless runner messages remain consistent with T0001 (licensing token refresh message followed by successful license update, Gtk warnings, FMOD no-device fallback)
- batchmode exited successfully

T0002 did not modify or validate:
- mobile planting/radial interaction (T0003)
- combat/plants/enemies/waves
- Android SDK/NDK/JDK or APK build
- gameplay balance
- graphical assets

Higgsfield was not used because T0002 required no new or changed art assets.

## T0003 — Mobile Planting and Radial Menu

Status: **PASS**

Real Unity evidence:
- Unity Executor run: `36986249467`
- validated head before documentation commits: `7b1ea26bf6d62aa268c21d5f59e3096cb2078dc8`
- Unity version: `6000.0.26f1 (ccb7c73d2c02)`
- executor report: `status=PASS`
- `playModeEntered=True`
- Game View during this ticket: `640 × 480`
- runtime warnings captured by T0003 harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- one existing unique compiler warning remains: `CS0618` in `MapAEditorSetup.cs:46` (emitted twice)

All 12 planting spots were exercised individually through the production screen-point input path. For every A1–A12:
- the world press was accepted,
- the expected spot became selected,
- the selection ring changed to selected state,
- the radial menu opened,
- all four existing plant options were visible,
- the complete radial button bounds remained inside the Game View,
- the close button was invoked through uGUI pointer-click handling,
- closing cleared both the radial menu and selected spot,
- result: **PASS**.

Edge/clamping evidence:
- edge-classified spots exercised: A1, A8, A9, A11, A12,
- all edge spots remained fully on-screen,
- A9 required a clamp shift of `+6.389 px` on X,
- A12 required a clamp shift of `-52.094 px` on X,
- A1, A8 and A11 required no shift at this Game View size.

Additional T0003 acceptance evidence:
- unaffordable state: Essence was reduced through the authoritative Economy API to `50`; all four plant buttons were disabled; clicking a disabled option did not occupy A2 and did not change Essence,
- UI/world blocking: a real runtime uGUI Button/Image was positioned over A3; its uGUI hit rules and click handler succeeded; the same screen press sent through production `TryHandleScreenPress` was rejected for world processing and did not select/open the spot beneath it,
- occupied blocking: A1 was occupied once with Pędostrzał as a placement-state proof only; a second press did not reopen planting selection and no second plant was created.

T0003 implementation changes were limited to:
- routing touch/mouse through one explicit screen-point planting path,
- conservative uGUI Graphic hit testing so any raycastable UI blocks world planting input,
- clamping the radial menu from its actual calculated child bounds rather than a fixed magic margin,
- stable radial button names/read-only state needed for evidence,
- a ticket-specific Play Mode validator and Unity Executor mode.

T0003 did not validate plant combat behavior, waves, Android build/toolchain, physical-device touch, 16:9/20:9 device layout, upgrades, new plants, balance, or T0004+ behavior. No graphical asset changes were required, so Higgsfield was not used.

## T0004 — Four Plant Behaviors

Status: **PASS**

Real Unity evidence:
- Unity Executor run: `36988172698`
- validated head before documentation commits: `21cac36b4d2902451af65aa9134f4c9d0f4f8c90`
- Unity version: `6000.0.26f1 (ccb7c73d2c02)`
- executor report: `status=PASS`
- `playModeEntered=True`
- runtime warnings captured by T0004 harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning remains: `CS0618` in `MapAEditorSetup.cs:46` (emitted twice)
- Android build/toolchain validation: **NOT RUN**
- wave flow: **NOT RUN**

Pędostrzał evidence:
- attack range: `3.2 RU`
- in-range First candidate: distance `2.236 RU`, progress `4.5`
- second in-range candidate: distance `1.414 RU`, progress `1.5`
- out-of-range candidate: distance `3.64 RU`, progress `10`
- projectile targeted the highest-progress in-range enemy and excluded the higher-progress out-of-range enemy
- projectile speed: `7 RU/s`
- raw physical damage: `22`
- unarmored HP: `170 → 148` (final damage `22`)
- back and out-of-range enemies remained `170 → 170`
- Armored HP: `260 → 241.3`
- Armored physical resistance: `0.15`
- expected/final resisted damage: `18.7`

Kwiat Esencji evidence:
- income: `+20 Essence`
- configured interval: `20 game seconds`
- Essence: `300 → 320 → 340`
- first payout: `20.133 game seconds`
- second payout: `40.133 game seconds`
- measured second interval: `20.0 game seconds`
- only this timing scenario used `Time.timeScale=8` to reduce wall-clock CI duration; game-time cadence remained production-driven.

Dębowy Strażnik evidence:
- range: `2.4 RU`, fixture distance `2.0 RU`
- normal enemy HP: `170 → 162`, damage `8`
- normal root: configured/observed `1.75 s / 1.75 s`
- normal speed: `0.8 → 0.0 during root → 0.8 after root`
- boss HP: `2200 → 2192`, damage `8`
- boss control multiplier: `0.35`
- boss expected/observed root: `0.6125 s / 0.612 s`
- boss speed: `0.3 → 0.0 during root → 0.3 after root`

Mroźny Pąk evidence:
- range: `3.2 RU`, fixture distance `2.0 RU`
- normal HP: `170 → 152`, raw/final Frost damage `18 / 18`
- normal slow: `35%` for `2.5 s`
- normal speed: `0.8 → 0.52 during slow → 0.8 after expiry`
- boss HP: `2200 → 2182`, raw/final Frost damage `18 / 18`
- boss physical resistance: `0.20`, correctly irrelevant to Frost damage
- requested slow `35%`, boss-applied slow cap `20%`
- boss slow duration: `2.5 s`
- boss speed: `0.3 → 0.24 during slow → 0.3 after expiry`

Validation method:
The T0004 harness entered genuine Unity Play Mode and instantiated the actual production plant components, production `SeedProjectile`, production `EnemyUnit`, `Economy`, `Targeting`, and `GameSession.SpawnEnemy`. Reflection was used only for deterministic fixture placement/state setup and evidence reads of private runtime fields; it did not call `TakeDamage`, `ApplyRoot`, or `ApplySlow` in place of plant behavior.

T0004 did not modify production plant balance/behavior. It added only ticket-specific Play Mode validation/executor infrastructure and documentation. T0005 enemy/wave flow, T0006 full boss/end states, Android build/toolchain, device smoke, mobile layout and later tickets were not started. No graphical changes were required; Higgsfield was not used.

## T0005 — Enemy Runtime and Wave Flow

Status: **PASS**

Real Unity evidence:
- Unity Executor run: `36990840649`
- validated head before documentation commits: `a11ceba14b55c84d586881ac45baf356c0018daf`
- Unity version: `6000.0.26f1 (ccb7c73d2c02)`
- executor report: `status=PASS`
- `playModeEntered=True`
- runtime warnings captured by T0005 harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning remains: `CS0618` in `MapAEditorSetup.cs:46` (emitted twice)
- Android build/toolchain validation: **NOT RUN**
- boss wave 6: **NOT STARTED**
- victory/defeat/restart: **NOT RUN**

Enemy evidence from normal production wave flow:

Chochlik / Wanderer:
- HP: `170`
- configured speed: `0.8 RU/s`
- measured speed: `0.875 RU/s` (within T0005 tolerance `±0.08`)
- reward: `12 Essence`
- reward observation: `300 → 312`
- base damage: `1`
- physical resistance: `0`
- natural leak: progress `0 → 46.08 RU`, Base `20 → 19`

Wichrowy Skrzat / Runner:
- HP: `110`
- configured/measured speed: `1.15 / 1.15 RU/s`
- reward: `12 Essence`
- reward observation: `408 → 420`
- base damage: `1`
- physical resistance: `0`
- natural leak: progress `0 → 46.0 RU`, Base `19 → 18`

Ogr Zarośli / Brute:
- HP: `550`
- configured/measured speed: `0.45 / 0.45 RU/s`
- reward: `26 Essence`
- reward observation: `642 → 668`
- base damage: `2`
- physical resistance: `0`
- natural leak: progress `0 → 46.2 RU`, Base `17 → 15`

Żelazny Goblin / Armored:
- HP: `260`
- configured/measured speed: `0.55 / 0.55 RU/s`
- reward: `22 Essence`
- reward observation: `504 → 526`
- base damage: `1`
- physical resistance: `0.15`
- physical resistance proof: raw physical damage `100`, HP `260 → 175`, final damage `85`
- natural leak: progress `0 → 46.2 RU`, Base `18 → 17`

Wave-flow evidence:
- wave 1: `6 × Wanderer` — observed exactly
- wave 2: `4 × Wanderer + 4 × Runner` — observed exactly
- wave 3: `5 × Wanderer + 4 × Armored` — observed exactly
- wave 4: `6 × Runner + 3 × Brute + 4 × Wanderer` — observed exactly
- wave 5: `4 × Armored + 3 × Brute + 8 × Runner` — observed exactly
- total production spawns observed: `51`
- controlled reward/cleanup deaths through production `EnemyUnit.Die`: `47`
- natural production leaks through `ReachBase`: `4`
- boss observed: `False`
- Base: `20 → 15`
- Essence: `300 → 1004`
- after wave 5: `CurrentWave=5`, wave 6 ready but not started, `sessionFinished=False`

Next-wave button and HUD:
- every wave 1–5 was started through the production HUD button using uGUI pointer-click dispatch
- before each wave: `ReadyForNextWave=True`, button active/interactable
- while each wave was running: `ReadyForNextWave=False`, button inactive
- after each wave completed: ready/button active again
- active-enemy HUD showed a positive count in every wave and returned to `WROGOWIE  0`
- final wave HUD: `FALA  5/6`

Validation method:
The T0005 harness entered genuine Unity Play Mode. All 51 acceptance enemies were spawned by the production `WaveDirector` through normal waves 1–5. It exercised production `EnemyUnit.Update` movement, `TakeDamage`, `Die`, `ReachBase`, `Economy`, `BaseHealth`, HUD refresh and the production next-wave button. No acceptance enemy was injected or teleported. One untouched specimen of each enemy kind traversed the full path naturally to the base.

The first dedicated run `36990073053` failed only one harness timing measurement: the first Chochlik speed sample included the startup/time-scale transition and measured `1.073`. Codex review identified this as a first-frame sampling artifact because later enemy speeds and full-path movement were correct. The harness was changed only to arm the speed sample after the first full player frame. No production enemy, wave, stat or balance code was modified. The subsequent run `36990840649` passed.

T0005 did not start wave 6, validate full boss behavior, victory, defeat, restart, Android build/toolchain, device smoke, mobile layout, or T0006+. No graphical changes were required; Higgsfield was not used.

## T0006 — Boss, Win/Lose, Restart

Status: **PASS**

Real Unity evidence:
- Unity Executor run: `36994046983`
- validated head before documentation commits: `c391c72cf7b23db613e5efd418b5df83aac29e22`
- Unity version: `6000.0.26f1 (ccb7c73d2c02)`
- executor report: `status=PASS`
- `playModeEntered=True`
- runtime warnings captured by T0006 harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning remains: `CS0618` in `MapAEditorSetup.cs:46` (emitted twice)
- Android build/toolchain validation: **NOT RUN**
- T0007 layout validation: **NOT RUN**

Wave 6 evidence:
- started through the production uGUI next-wave button
- `CurrentWave: 5 → 6`
- exact observed production spawn count: `12`
- exact observed order: `Armored ×4 → Runner ×5 → Brute ×2 → Boss ×1`
- boss registration HUD: `WROGOWIE  12`
- maximum active enemy count observed: `12`
- while wave 6 was active: `ReadyForNextWave=False`, next-wave button inactive
- no wave 7 became available after the final wave

Król Cierni runtime evidence:
- kind: `Boss`
- initial HP: `2200`
- configured movement speed: `0.3 RU/s`
- measured baseline speed: `0.318 RU/s`
- reward: `150 Essence`
- base damage: `5`
- physical resistance: `0.20`
- spawn progress: `0`

Boss damage/control evidence through production plants/projectiles:
- Pędostrzał at A9: distance `2.692 RU`, actual `MagicSeed`, projectile speed `7 RU/s`, raw Physical `22`, boss HP `2200 → 2182.4`, final damage `17.6`
- Mroźny Pąk at A1: distance `1.372 RU`, boss HP `2182.4 → 2164.4`, raw/final Frost damage `18 / 18`, requested slow `35%`, boss-applied slow `20%`, requested duration `2.5 s`, observed remaining duration at impact `2.483 s`, speed `0.318 → 0.24 → 0.3`, slow expired
- Dębowy Strażnik at A2: distance `2.25 RU`, boss HP `2164.4 → 2156.4`, root damage `8`, expected boss root `0.6125 s`, observed `0.596 s`, speed `0.318 → 0 → 0.3`

Victory evidence:
- wave 6 acceptance enemies were not direct-damaged by the harness; final boss kill and wave-6 cleanup came from production plant/projectile behavior
- isolated boss reward: Essence `126 → 276`, delta `+150`
- Base remained `20 → 20` while the boss was defeated
- `bossDefeatedByProduction=True`
- `sessionFinished=True`
- `isVictory=True`
- `CurrentWave=6`
- `ReadyForNextWave=False`
- `Running=False`
- active enemies: `0`
- HUD: `WROGOWIE  0`, `FALA  6/6`
- victory `EndPanel` present with title `ZIELONY TRAKT OBRONIONY!`
- restart button present
- next-wave button inactive
- `wave7Available=False`

Restart after victory:
- restart invoked through the real `ZAGRAJ PONOWNIE` uGUI button
- old/new GameSession instance IDs differed
- before restart: `11` occupied planting spots and `8` live plants
- after restart: `sessionFinished=False`, `isVictory=False`, Base `20`, Essence `300`, `CurrentWave=0`, ready `True`, active enemies `0`
- HUD restored to `WROGOWIE  0` and `FALA  0/6`
- occupied planting spots: `0`
- plant/projectile/enemy-object counts: `0 / 0 / 0`
- EndPanel absent
- wave button active
- production wave 1 could be started again through the real button

Natural defeat evidence:
- fresh restarted session, no plants used for defeat
- defeat came only through production enemy movement and `ReachBase`; Base HP was never directly assigned/damaged by the harness
- natural leak count: `20`
- Base: `20 → 0`
- defeat occurred during wave `3`
- waves started through production button: `1, 2, 3`
- final leaking enemy: `Armored`
- final observed progress: `46.2 RU` vs MAP-A path `46.211 RU`
- `sessionFinished=True`
- `isVictory=False`
- `ReadyForNextWave=False`
- `Running=False`
- wave button inactive
- planting input rejected after defeat
- defeat EndPanel present with title `KRÓL CIERNI PRZEDARŁ SIĘ DO BAZY`
- restart button present

Restart after defeat:
- restart invoked through the real EndPanel button
- a new GameSession instance was created
- Base `20`, Essence `300`, `CurrentWave=0`, ready `True`, active enemies `0`
- HUD restored to `WROGOWIE  0`, `FALA  0/6`
- occupied spots/plants/projectiles/enemy objects: `0 / 0 / 0 / 0`
- EndPanel absent
- wave button active
- wave 1 restarted successfully through the production button and HUD showed `WROGOWIE  1`, proving the second restarted session was playable

Validation method:
T0006 used an Editor-only persistent Play Mode driver. Waves 1–5 were started through the real next-wave button and were fast-forwarded with direct Root damage only as setup for the already-verified T0005 scope. Wave 6 acceptance enemies were not direct-damaged by the validation harness. Boss resistance/control evidence came from real Pędostrzał, Mroźny Pąk and Dębowy Strażnik production paths. Victory used production plant/projectile damage and normal `EnemyUnit.Die`/WaveDirector/GameSession flow. Defeat used only natural movement and `ReachBase`. Both restarts used the actual EndPanel restart button and scene reload.

No production boss, wave, GameSession, BaseHealth, Economy, MobileHud, plant balance or map behavior was changed. No graphical asset changes were required; Higgsfield was not used.

## T0007 — Android UI and Device Layout

Status: **PASS**

Real Unity evidence:
- Unity Executor run: `36997417372`
- validated head before documentation commits: `7b99c11501a81a3f1526d57958d7c2d72ab386e4`
- Unity version: `6000.0.26f1 (ccb7c73d2c02)`
- Play Mode: `True`
- executor report: `status=PASS`
- runtime warnings captured by T0007 harness: `0`
- validation errors: `0`
- Android APK build: **NOT RUN**
- T0008: **NOT STARTED**

Render-capable executor:
The first dedicated run `36997235403` failed before meaningful layout evidence because `-nographics` left Unity without a graphics device and `Screen` stayed at `640×480`. This was an executor-environment failure, not a production UI failure. The T0007 executor mode was changed only to use `xvfb-run` with a 2560×1440 virtual display and no `-nographics`. The subsequent run `36997417372` observed the exact requested Game View resolutions and passed.

16:9 native case:
- requested/observed resolution: `1920×1080`
- aspect ratio: `1.778`
- native/effective/actual safe area: `0,0,1920,1080`
- full MAP-A visible: **PASS**
- camera world bounds: `-1.422,-0.55,26.844,15.1`
- camera centered: **PASS**
- camera panning required: **NO**
- HUD inside safe area: **PASS**
- wave button: **PASS**
- wave button measured rect: `300×126 px`
- all 12 planting spots accessible through production `TryHandleScreenPress`: **PASS**
- edge radial menus A1/A8/A9/A11/A12: **PASS**
- text overlap: **none**
- text clipping: **none detected**
- smallest measured interactive target: `RadialCloseButton 74×74 px`
- Console/runtime validation errors: `0`

20:9 native case:
- requested/observed resolution: `2400×1080`
- aspect ratio: `2.222`
- native/effective/actual safe area: `0,0,2400,1080`
- full MAP-A visible: **PASS**
- camera world bounds: `-4.778,-0.55,33.556,15.1`
- camera centered: **PASS**
- camera panning required: **NO**
- HUD inside safe area: **PASS**
- wave button: **PASS**
- wave button measured rect: `335.41×140.872 px`
- all 12 planting spots accessible: **PASS**
- edge radial menus A1/A8/A9/A11/A12: **PASS**
- text overlap/clipping: **none detected**
- smallest measured interactive target: `RadialCloseButton 82.735×82.735 px`
- Console/runtime validation errors: `0`

20:9 synthetic safe-area case:
- requested/observed resolution: `2400×1080`
- native Editor safe area: full screen
- synthetic effective safe area: `Rect(120,0,2160,1080)`
- actual SafeArea RectTransform after production `SafeAreaFitter`: exactly `120,0,2160,1080`
- full MAP-A visible: **PASS**
- camera centered: **PASS**
- camera panning required: **NO**
- HUD inside synthetic safe area: **PASS**
- wave button: **PASS**
- wave button measured rect: `335.41×140.872 px`
- all 12 planting spots accessible: **PASS**
- edge radial menus A1/A8/A9/A11/A12 inside synthetic safe area: **PASS**
- text overlap/clipping: **none detected**
- smallest measured interactive target: `RadialCloseButton 82.735×82.735 px`
- Console/runtime validation errors: `0`

Safe-area limitation:
The repository does not contain `com.unity.device-simulator`, so T0007 does **not** claim a Device Simulator test. The notch/cutout case used an explicit Editor-only synthetic safe-area override routed through the production `SafeAreaFitter` normalization/anchor logic. Android/player builds remain on the native `Screen.safeArea` path. This validates layout response to the tested inset but does not prove a specific physical Android device reports that inset.

Touch target sizing:
The repository defines no numeric minimum touch-target threshold. T0007 therefore records measured screen-pixel sizes rather than inventing one. The measured controls were active, reachable, nonzero and unclipped in every tested layout. The smallest target was the radial close button at `74×74 px` in 16:9 and `82.735×82.735 px` in 20:9.

No production gameplay, balance, plant, enemy, wave, boss, map geometry or gameplay art changes were required. No graphical assets were generated.

## Build state

Android build: **NOT RUN**.

No APK success is claimed.

## Input state

Current code uses legacy Unity `Input` touch APIs plus UGUI `StandaloneInputModule`.

T0003 verified the touch-style screen-point planting path and uGUI/world-input blocking in real Unity Play Mode. Actual physical Android touch/device behavior remains **NOT VERIFIED** and belongs to the later device smoke ticket.

Do not migrate input systems without device/runtime evidence that migration is necessary.

## Architecture state

Current architecture is intentionally small and centralized around `GameSession`.

No architecture rewrite is justified by T0001, and none was performed.

## T0001 outcome

**PASS**

Evidence:
- real Unity 6000.0.26f1 editor process executed,
- import and compilation completed,
- no C# compiler errors,
- Console/log baseline preserved,
- MAP-A scene exists,
- MAP-A is enabled in Build Settings,
- Android module support is confirmed,
- branch/head and clean checkout are recorded,
- only T0001 diagnostic/documentation scope was performed.

See:
- Unity Executor run `36979277994`
- artifact `unity-executor-baseline-36979277994`
- `docs/Known_Issues_And_Followups.md`

## Next recommended action

Do not start another ticket automatically.

T0008 remains PLANNED and requires separate PM/user activation after T0007 review/merge.
