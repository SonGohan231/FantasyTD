# Known Issues And Followups

This file tracks discoveries that should not silently expand an active ticket.

Use statuses:
- OPEN
- VERIFY
- DEFERRED
- RESOLVED

Do not list speculative issues as confirmed defects.

---

## KI-001 — Unity gameplay runtime is only partially verified

Status: **VERIFY**  
Confidence: Confirmed as an evidence gap  
Severity: High for milestone readiness

Evidence:
T0001 verified the Editor/import/compile baseline. T0002 verified MAP-A scene/geometry. T0003 Unity Executor run `36986249467` verified mobile planting/radial interaction. T0004 Unity Executor run `36988172698` verified all four production plant behaviors. T0005 Unity Executor run `36990840649` verified all four basic enemy types and production wave flow 1–5. T0006 Unity Executor run `36994046983` verified production wave 6, Król Cierni runtime/control interactions, production victory, natural-leak defeat, terminal-state input gating, and real UI restart after both outcomes.

Still unverified:
- physical Android device runtime,
- Android layout/build/device-specific behavior covered by later tickets.

Impact:
The MAP-A gameplay loop is now verified in real Unity Play Mode through wave 6, victory, defeat and restart. Remaining milestone evidence gaps are Android-specific rather than the core Editor Play Mode loop.

Action:
Continue runtime verification only through separately activated tickets.

---

## KI-002 — MAP-A scene generation and geometry integrity

Status: **RESOLVED**  
Confidence: Confirmed by Unity Executor  
Severity: Medium

T0001 evidence:
Unity Executor run `36979277994` reported:
- `scenePath=Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`
- `sceneExists=True`
- `sceneEnabledInBuildSettings=True`

T0002 evidence:
Unity Executor run `36982186182` reported PASS and confirmed:
- MAP-A scene opened successfully,
- editor validation command emitted PASS,
- logical map is 24 × 14 RU,
- road renderers exist and match all route waypoints,
- spawn/portal is anchored at route start,
- base/tree is anchored at route end,
- exactly 12 runtime planting spots match their definitions,
- no gameplay grid is present,
- the configured camera contains the logical map bounds,
- validation Console error count is 0.

Impact:
The scene-generation and geometry-integrity risks tracked by KI-002 are now directly verified in real Unity.

Remaining scope:
Touch/UI behavior, combat, waves, gameplay state and device behavior remain separate later-ticket concerns.

Action:
No T0002 remediation required. Preserve the current generated-scene workflow unless later runtime evidence identifies a concrete defect.

---

## KI-003 — Android build toolchain completeness not fully verified

Status: **VERIFY**  
Confidence: Partially verified  
Severity: High for Android milestone

Verified in T0001:
- Unity reports `androidModuleSupported=True`,
- the Android platform support module registered from the installed Unity editor,
- an ADB device scan executed during editor initialization.

Still unknown / not independently verified:
- Android SDK version/path,
- Android NDK version/path,
- JDK version/path,
- actual Android player build success.

Action:
Verify exact SDK/NDK/JDK configuration and build output in the dedicated Android build ticket. Do not switch build target or build an APK as part of T0001.

---

## KI-004 — Touch/input configuration and UI/world routing

Status: **VERIFY**  
Confidence: Partially verified in real Unity Play Mode  
Severity: Medium

T0003 evidence:
Unity Executor run `36986249467` entered Play Mode and verified:
- all 12 planting spots through the production screen-point planting path,
- uGUI interaction blocks the underlying world planting press,
- radial close interaction clears selection,
- occupied spots do not reopen planting selection.

T0003 also replaced dependence on EventSystem/GraphicRaycaster registration for world-input blocking with direct active uGUI Graphic hit rules because headless Play Mode evidence showed the raycaster registry did not report the temporary runtime control reliably.

Still unknown:
- physical Android touch behavior,
- device-specific input handling.

Action:
Keep T0003 Play Mode evidence as PASS. Validate actual device touch in T0009. Do not migrate input packages without device evidence.

---

## KI-005 — Mobile UI device-layout verification remains incomplete

Status: **VERIFY**  
Confidence: Partially verified in real Unity Play Mode  
Severity: Medium

T0003 evidence:
Unity Executor run `36986249467` verified the runtime radial menu at Game View `640 × 480`:
- all 12 spots opened the menu,
- all four options remained visible,
- every radial menu stayed within Game View bounds,
- edge spots A1, A8, A9, A11 and A12 were exercised,
- A9 and A12 required and received clamp shifts.

Still unverified:
- required 16:9 and approximately 20:9 layouts,
- safe-area/notch cases,
- final text/touch-target usability on device.

Action:
Keep those device-layout checks for T0007.

---

## KI-006 — No automated Unity tests observed

Status: **DEFERRED**  
Confidence: Confirmed from current source handoff  
Severity: Low at current prototype stage

Impact:
Regression detection is mostly manual.

T0001 note:
The configured EditMode + PlayMode job was intentionally skipped in run `36979277994` because automated tests and Play Mode are explicit non-goals of T0001.

Action:
After MAP-A architecture and runtime stabilize, evaluate targeted EditMode tests for:
- path length,
- wave composition,
- balance constants,
- planting spot count.

Do not add a testing framework migration during T0001.

---

## KI-007 — No committed package lock observed

Status: **VERIFY**  
Confidence: Confirmed from repository state  
Severity: Low/Medium

Evidence:
No committed `Packages/packages-lock.json` is part of the current source baseline. The real Unity import resolved packages successfully during T0001.

Impact:
Dependency reproducibility may still be weaker until the project decides whether generated package-lock state should be committed.

Action:
Do not fabricate or hand-edit a lock file. Revisit only in a dedicated maintenance/reproducibility task if needed.

---

## KI-008 — Declared editor revision is placeholder-like

Status: **RESOLVED**  
Confidence: Confirmed by real Unity process  
Severity: Low

Original evidence:
`ProjectVersion.txt` declares Unity `6000.0.26f1` with an all-zero revision field.

T0001 evidence:
Unity Executor run `36979277994` actually used:

`6000.0.26f1 (ccb7c73d2c02)`

The project opened/imported/compiled successfully with that editor.

Impact:
The placeholder-like revision field did not block the baseline. Actual editor provenance for T0001 is now recorded.

Action:
No T0001 remediation required.

---

## KI-009 — Balance v1.0 is not runtime-proven

Status: **DEFERRED**  
Confidence: Confirmed  
Severity: Informational until runtime works

Evidence:
Balance workbook is based on design mathematics and benchmark assumptions. T0004 verified that the current plant constants execute as implemented in runtime (damage, income interval, root and slow), but it did not evaluate overall game balance or playtest outcomes.

Action:
Do not change broad balance during technical stabilization.
Create Balance v1.1 only after playtest evidence in T0010.

---

## KI-010 — Obsolete Unity API warning in MapAEditorSetup

Status: **OPEN**  
Confidence: Confirmed by Unity compiler  
Severity: Low

Evidence:
Unity 6000.0.26f1 emitted one unique compiler warning, twice during import/compilation:

```text
Assets/FantasyTD/Editor/MapAEditorSetup.cs(46,13): warning CS0618:
'PlayerSettings.SetApplicationIdentifier(BuildTargetGroup, string)' is obsolete:
'Use SetApplicationIdentifier(NamedBuildTarget buildTarget, string identifier) instead'
```

Impact:
Non-blocking. T0001 completed with zero C# compiler errors and the Unity Executor baseline succeeded.

Action:
Update the obsolete API in a separately scoped maintenance/fix ticket if desired. Do not modify source code during T0001 solely to remove this warning.

---

## T0006 verified evidence

Unity Executor:
- run: `36994046983`
- branch: `codex/t0006-boss-win-lose-restart`
- validated head before documentation commits: `c391c72cf7b23db613e5efd418b5df83aac29e22`
- T0006 Play Mode job: **success**
- executor report status: **PASS**
- Play Mode entered: `True`
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- runtime warnings captured by harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning: `CS0618` in `MapAEditorSetup.cs:46`
- artifact: `unity-executor-boss-end-states-36994046983` (ID `11220923927`)
- Android build/toolchain/layout: **NOT RUN**

Wave 6:
- started through production uGUI next-wave button: **PASS**
- CurrentWave `5 → 6`: **PASS**
- exact observed sequence `Armored×4 → Runner×5 → Brute×2 → Boss×1`: **PASS**
- boss included in active-enemy HUD (`WROGOWIE  12` at registration): **PASS**
- wave 7 unavailable after completion: **PASS**

Król Cierni:
- HP `2200`
- configured/measured speed `0.3 / 0.318 RU/s`
- reward `150`
- base damage `5`
- physical resistance `0.20`
- Seed physical raw `22` resolved to `17.6`: **PASS**
- Frost raw/final `18 / 18`, requested slow `35%`, boss-applied `20%`, speed `0.318 → 0.24 → 0.3`: **PASS**
- Oak root expected/observed `0.6125 / 0.596 s`, speed `0.318 → 0 → 0.3`: **PASS**

Victory:
- boss was defeated by production plant/projectile behavior: **PASS**
- isolated boss reward `+150`, Base unchanged `20 → 20`
- final state: `IsFinished=True`, `IsVictory=True`, `CurrentWave=6`, enemies `0`, Ready false, Running false
- HUD `WROGOWIE  0`, `FALA  6/6`
- victory EndPanel/title/restart: **PASS**
- no wave 7: **PASS**

Restart after victory:
- actual EndPanel restart pointer click: **PASS**
- genuinely new GameSession: **PASS**
- Base/Essence/wave/ready/HUD reset: **PASS**
- occupied spots/plants/projectiles/enemy objects cleared: **PASS**
- wave 1 starts again through production button: **PASS**

Defeat:
- generated only through natural production movement + `ReachBase`, no direct Base HP assignment/damage by harness
- `20` natural leaks, Base `20 → 0`
- defeat occurred during wave `3`
- final leak Armored at progress `46.2 RU` on `46.211 RU` path
- `IsFinished=True`, `IsVictory=False`, Ready false, wave button inactive, planting input blocked
- defeat EndPanel/title/restart: **PASS**

Restart after defeat:
- actual restart click and new GameSession: **PASS**
- full gameplay/UI/occupancy/combat reset: **PASS**
- second-session wave 1 start and positive enemy HUD: **PASS**

Waves 1–5 were fast-forwarded with direct Root kills only as T0006 setup after being independently validated by T0005. Wave 6 acceptance enemies were never direct-damaged by the T0006 harness.

---

## T0005 verified evidence

Unity Executor:
- run: `36990840649`
- branch: `codex/t0005-enemy-runtime-wave-flow`
- validated head before documentation commits: `a11ceba14b55c84d586881ac45baf356c0018daf`
- T0005 Play Mode job: **success**
- executor report status: **PASS**
- Play Mode entered: `True`
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- runtime warnings captured by harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning: `CS0618` in `MapAEditorSetup.cs:46`
- artifact: `unity-executor-enemy-wave-flow-36990840649` (ID `11219706930`)

Basic enemies:
- Chochlik: HP `170`, measured speed `0.875` vs configured `0.8`, reward `12`, leak damage `1`, resistance `0`: **PASS**
- Wichrowy Skrzat: HP `110`, speed `1.15`, reward `12`, leak damage `1`, resistance `0`: **PASS**
- Ogr Zarośli: HP `550`, speed `0.45`, reward `26`, leak damage `2`, resistance `0`: **PASS**
- Żelazny Goblin: HP `260`, speed `0.55`, reward `22`, leak damage `1`, resistance `0.15`: **PASS**
- Armored raw physical damage `100` resolved to `85`, HP `260 → 175`: **PASS**
- one untouched specimen of every basic kind traversed the production route naturally and leaked to the base: **PASS**

Waves:
- W1: 6 Wanderer — **PASS**
- W2: 4 Wanderer + 4 Runner — **PASS**
- W3: 5 Wanderer + 4 Armored — **PASS**
- W4: 6 Runner + 3 Brute + 4 Wanderer — **PASS**
- W5: 4 Armored + 3 Brute + 8 Runner — **PASS**
- total observed production spawns: `51`
- exact observed sequence/counts: **PASS**
- production next-wave button state before/during/after each wave: **PASS**
- active-enemy HUD count positive during every wave and final `WROGOWIE  0`: **PASS**
- Base `20 → 15`, Essence `300 → 1004`
- wave 6 ready but not started; Boss not observed; `sessionFinished=False`

The first dedicated run `36990073053` failed only a first-frame harness speed measurement for Chochlik (`1.073`). Codex review and subsequent evidence identified it as a sampling artifact around the time-scale/spawn startup boundary. Only the harness sample timing was adjusted; no production enemy/wave behavior or balance was changed. Run `36990840649` passed.

T0005 did not start wave 6 or validate full boss behavior, victory/defeat/restart, Android build/toolchain, device smoke, mobile layout, or T0006+. No graphical assets changed.

---

## T0004 verified evidence

Unity Executor:
- run: `36988172698`
- branch: `codex/t0004-four-plant-behaviors`
- validated head before documentation commits: `21cac36b4d2902451af65aa9134f4c9d0f4f8c90`
- T0004 Play Mode job: **success**
- executor report status: **PASS**
- Play Mode entered: `True`
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- runtime warnings captured by harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning: `CS0618` in `MapAEditorSetup.cs:46`
- artifact: `unity-executor-plant-behaviors-36988172698` (ID `11218576224`)

Pędostrzał:
- range/First targeting: **PASS**
- projectile reaches intended target: **PASS**
- unarmored physical damage `22`: **PASS**
- Armored resistance `15%` reduced `22 → 18.7`: **PASS**

Kwiat Esencji:
- `+20` after first `20 s` game-time interval: **PASS**
- another `+20` after the second interval: **PASS**

Dębowy Strażnik:
- normal root `1.75 s`, movement stop/resume: **PASS**
- boss root `~0.6125 s` from `0.35×` multiplier: **PASS**

Mroźny Pąk:
- Frost damage `18`: **PASS**
- normal slow `35%`, speed `0.8 → 0.52 → 0.8`: **PASS**
- boss slow capped at `20%`, speed `0.3 → 0.24 → 0.3`: **PASS**
- slow expiry normal/boss: **PASS**

T0004 did not run wave sequencing, Android build/toolchain, device smoke, mobile aspect/layout tests, new plants, upgrades, or T0005+ implementation. No graphical asset changes were needed.

---

## T0003 verified evidence

Unity Executor:
- run: `36986249467`
- branch: `codex/t0003-mobile-planting-radial-menu`
- validated head before documentation commits: `7b1ea26bf6d62aa268c21d5f59e3096cb2078dc8`
- T0003 Play Mode job: **success**
- executor report status: **PASS**
- Play Mode entered: `True`
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- Game View: `640 × 480`
- 12/12 planting spots: **PASS**
- edge clamping: **PASS**
- unavailable state/purchase blocking: **PASS**
- UI/world input blocking: **PASS**
- occupied-spot second placement blocking: **PASS**
- runtime warnings captured by harness: `0`
- validation errors: `0`
- C# compiler errors: `0`
- existing unique compiler warning: `CS0618` in `MapAEditorSetup.cs:46`
- artifact: `unity-executor-mobile-planting-36986249467` (ID `11217666657`)
- Android build/toolchain validation: **NOT RUN**
- plant combat validation: **NOT RUN**
- no graphical asset changes were needed

No T0004 implementation was performed.

---

## T0002 verified evidence

Unity Executor:
- run: `36982186182`
- branch: `codex/t0002-map-a-integrity`
- validated head before documentation commits: `9976edd0643214572b96fe38cb8c5710b029b937`
- MAP-A integrity job: **success**
- executor report status: **PASS**
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- validation Console errors: `0`
- artifact: `unity-executor-map-a-integrity-36982186182` (ID `11215922210`)
- Android APK job: **skipped**
- EditMode + PlayMode tests job: **skipped**
- no new graphical assets were needed

No T0003 implementation, Android build, mobile interaction validation, combat validation, balance change, or asset generation was performed.

---

## T0001 verified evidence

Unity Executor:
- run: `36979277994`
- branch: `codex/t0001-unity-baseline-v2`
- validated head before documentation commits: `e6e91bc9a0835128165c0d8539f614163a546cf7`
- baseline job: **success**
- tests job: **skipped**
- Android APK job: **skipped**
- artifact: `unity-executor-baseline-36979277994` (ID `11214941802`)

No Play Mode, Android build, gameplay change, visual change, balance change, package migration, or T0002 implementation was performed.

---

## Follow-up candidates

Do not activate automatically:
- fix the confirmed CS0618 editor warning,
- add focused EditMode tests after baseline stability,
- decide whether generated scene/package-lock state should be committed once workflows are proven,
- validate exact Android SDK/NDK/JDK configuration during the Android build ticket,
- split assemblies only if project growth creates a measurable need,
- replace procedural placeholder art only after gameplay vertical slice works.
