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
T0001 verified the Editor/import/compile baseline. T0002 verified MAP-A scene/geometry. T0003 Unity Executor run `36986249467` verified mobile planting/radial interaction. T0004 Unity Executor run `36988172698` verified all four production plant behaviors. T0005 Unity Executor run `36990840649` verified all four basic enemy types and production wave flow 1–5. T0006 Unity Executor run `36994046983` verified production wave 6, Król Cierni runtime/control interactions, production victory, natural-leak defeat, terminal-state input gating, and real UI restart after both outcomes. T0007 Unity Executor run `36997417372` verified exact 1920×1080 and 2400×1080 Game View layouts plus an Editor-only synthetic safe-area case through production `SafeAreaFitter`. T0008 Unity Executor run `36999815943` completed a real Android build and produced the expected APK with a successful BuildReport.

Still unverified:
- physical Android device runtime,
- device-reported safe-area/touch behavior and installed-APK smoke behavior.

Impact:
The MAP-A gameplay loop, required Editor mobile layout and Android build pipeline are verified. The remaining milestone evidence gap is physical/device runtime rather than build generation.

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

## KI-003 — Android build toolchain completeness

Status: **RESOLVED**  
Confidence: Confirmed by real Unity Android build  
Severity: High for Android milestone

T0008 evidence from Unity Executor run `36999815943`:
- Android Build Support: `True`,
- active build target: `Android`,
- Unity-reported SDK root exists,
- installed SDK platforms: `android-33;android-34;android-35`,
- installed build-tools: `34.0.0`,
- Unity-reported NDK root exists; `Pkg.Revision=23.1.7779620`,
- Unity-reported JDK root exists; `JAVA_VERSION=17.0.9`,
- target SDK setting: `AndroidApiLevelAuto`,
- minimum SDK setting: `AndroidApiLevel23`,
- application identifier: `com.indie.fantasytd`,
- BuildReport: `Succeeded`, zero errors and zero BuildReport warnings,
- physical APK produced at `Builds/Android/FantasyTD_MAP_A_v2.apk`,
- APK size `18869167` bytes,
- APK SHA-256 `13029c48866753493aa23fe7aab321c2f94dd7e76e43de49791c5c243ccc8bb5`.

Limitation:
Target SDK is configured as Auto and the Unity build log does not expose a single resolved API level. Installed API 35 is therefore not claimed as the resolved target.

Action:
No T0008 toolchain remediation required. Physical install/launch remains outside this issue and outside T0008.

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

## KI-005 — Mobile UI device-layout verification

Status: **RESOLVED**  
Confidence: Confirmed for T0007 Editor Play Mode acceptance  
Severity: Medium

T0003 evidence:
Unity Executor run `36986249467` verified the runtime radial menu at Game View `640 × 480` and exercised all 12 spots plus edge clamping.

T0007 evidence:
Unity Executor run `36997417372` entered real Play Mode with a render-capable Xvfb Game View and verified:
- exact `1920×1080` 16:9 layout,
- exact `2400×1080` approximately 20:9 layout,
- full MAP-A framing with no panning required,
- HUD and wave button contained and reachable,
- all 12 planting spots accessible through production screen-point input,
- edge radial menus A1, A8, A9, A11 and A12 fully contained,
- no independent text overlap or clipping detected,
- measured touch-target sizes recorded rather than judged against an invented threshold,
- zero runtime validation errors.

Safe-area limitation:
The project does not include `com.unity.device-simulator`. The T0007 notch/cutout case therefore used an explicit Editor-only synthetic safe area `Rect(120,0,2160,1080)` routed through the production `SafeAreaFitter`. The SafeArea RectTransform matched that effective rect exactly and all required UI/radial checks passed.

Impact:
The layout evidence gap scoped to T0007 is closed. This does not prove physical-device inset reporting or real Android touch; those remain device-smoke concerns.

Action:
No T0007 production-layout remediation required. Preserve the current UI/layout unless later Android-device evidence identifies a concrete defect.

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

Status: **RESOLVED**  
Confidence: Confirmed by T0008 compile/build evidence  
Severity: Low

Original evidence:
Unity 6000.0.26f1 emitted CS0618 for `PlayerSettings.SetApplicationIdentifier(BuildTargetGroup, string)`.

T0008 resolution:
The build-tooling code now uses:
`PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.indie.fantasytd")`.

Evidence:
- T0008 baseline compile succeeded after the change,
- real Android BuildReport succeeded,
- no C# CS0618 warning appears in the T0008 Android build log,
- BuildReport warnings: `0`.

Note:
The IL2CPP portion of the log still includes internal TextCore code-generation messages prefixed `Warning:`; Unity BuildReport does not count them as build warnings and they are unrelated to KI-010.

Action:
No further KI-010 remediation required.

---

## T0008 verified evidence

Unity Executor:
- final successful run: `37001334070`
- branch: `codex/t0008-android-build`
- validated final head: `99351f43cce3039a38c64c7020de9e540a76b864`
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- Android job: **success**
- BuildReport: **Succeeded**
- BuildReport duration: `468.893 s`
- BuildReport errors: `0`
- BuildReport warnings: `0`
- MAP-A present/enabled/in actual build options: **YES**
- application identifier: `com.indie.fantasytd`
- exact APK: `Builds/Android/FantasyTD_MAP_A_v2.apk`
- physical APK bytes: `18869164`
- APK SHA-256: `b99b3f85b29453a76e2f698927183ad5fc8d1f5eb62e57079c86b7676b8c6c57`
- artifact: `fantasy-td-android-37001334070` (ID `11224123199`)
- artifact contains APK: **YES**
- install/emulator/physical-device smoke: **NOT RUN**
- T0009: **NOT STARTED**

Toolchain:
- Android Build Support: `True`
- SDK root: Unity embedded AndroidPlayer SDK
- installed APIs: `33;34;35`
- build-tools: `34.0.0`
- NDK: `23.1.7779620`
- JDK: `17.0.9`
- target SDK setting: `AndroidApiLevelAuto`
- minimum SDK setting: `AndroidApiLevel23`

Final Android build completed successfully on the documented T0008 head; no device install/emulator/physical-device smoke was performed.

---

## T0007 verified evidence

Unity Executor:
- successful run: `36997417372`
- branch: `codex/t0007-android-ui-device-layout`
- validated head before documentation commits: `7b99c11501a81a3f1526d57958d7c2d72ab386e4`
- Unity: `6000.0.26f1 (ccb7c73d2c02)`
- Play Mode entered: `True`
- executor report: **PASS**
- runtime warnings captured by harness: `0`
- validation errors: `0`
- Android APK build: **NOT RUN**
- T0008: **NOT STARTED**
- artifact: `unity-executor-mobile-layout-36997417372` (ID `11222780203`)

16:9 native:
- observed exact `1920×1080`
- map fully visible / camera centered / no panning: **PASS**
- HUD and wave button: **PASS**
- all 12 planting spots accessible: **PASS**
- A1/A8/A9/A11/A12 radial menus: **PASS**
- text overlap/clipping: **PASS**
- smallest measured target: `74×74 px` radial close button
- Console validation errors: `0`

20:9 native:
- observed exact `2400×1080`
- map fully visible / camera centered / no panning: **PASS**
- HUD and wave button: **PASS**
- all 12 planting spots accessible: **PASS**
- A1/A8/A9/A11/A12 radial menus: **PASS**
- text overlap/clipping: **PASS**
- smallest measured target: `82.735×82.735 px`
- Console validation errors: `0`

20:9 synthetic safe area:
- observed exact `2400×1080`
- synthetic effective safe area `Rect(120,0,2160,1080)`
- actual SafeArea RectTransform matched exactly
- map/HUD/wave/all spots/edge radials/text checks: **PASS**
- no panning required
- Console validation errors: `0`

The first run `36997235403` failed only because the executor used `-nographics`, which left Unity without a graphics device and prevented exact Game View sizing. The executor was changed to render-capable Xvfb; no production UI change was made from that failure.

The Device Simulator package is absent, so the safe-area case is explicitly synthetic Editor-only validation rather than Device Simulator evidence.

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
- continue art/content expansion only as a separately activated future ticket; the MAP-A prototype-art replacement was completed in T0011.


---

## T0011 verified evidence — visual production pass

Status: **RESOLVED / PASS**

T0011 replaced the earlier prototype-style presentation with original procedural organic-fantasy runtime art. No external paid asset generation was used because the connected Higgsfield account rejected generation with `Requires basic plan or higher`.

Regression evidence:
- mobile layout / safe area: `37059744377` — PASS,
- MAP-A geometry after final visual anchor correction: `37061361432`, `37104978893` — PASS,
- four plant behaviors after VFX integration: `37062061398` — PASS,
- boss, victory, natural defeat, restart: `37105044037` — PASS,
- Android candidate build: `37060044752` — SUCCESS, BuildReport 0 errors / 0 warnings.

One issue was found and resolved during T0011:
- the decorative HomeTrunk sprite had been moved from the historically validated route-end visual anchor by 0.10 RU,
- only that presentation offset was restored,
- MapAData path and planting coordinates were never changed.

No new known gameplay defect was introduced by T0011 evidence.

T0009 device smoke and T0010 balance remain intentionally outside T0011.


---

## T0011 — Visual production follow-up status

Status: **NO OPEN T0011 BLOCKER**

Observed during validation:
- the first MAP-A integrity regression after visual dressing caught a HomeTree presentation anchor offset from the authoritative route endpoint;
- the visual anchor was corrected without changing `MapAData`;
- corrected MAP-A integrity then passed in run `37104978893`;
- plant behavior regression passed in run `37062061398`;
- boss/victory/defeat/restart regression passed in run `37105044037`;
- corrected Android BuildReport passed in run `37105211817` with 0 errors and 0 warnings.

External generation:
Higgsfield asset generation was unavailable on the connected account (`Requires basic plan or higher`). This is not a runtime/release blocker because T0011's original visual pass is implemented in-project with cached procedural sprites and lightweight VFX.

Remaining planned work is outside T0011:
- T0009 Android install/launch smoke test,
- T0010 balance v1.1 playtest pass.
## Recovery findings 2026-10-03

- Approved source checklist on `visual/higgsfield-api-bridge` records MAP7 artwork blocked by insufficient credits and MAP8 not produced, despite topology specifications. No generation is authorized now; full seven-map artwork remains unconfirmed.
- MAP2–6 backgrounds are road-free terrain plates, with unfinished BASE_COMPOSITE_IN_UNITY stages and incomplete animation sets. Preserved source files are not playable-map evidence.
- Four approved artifact IDs are shared between maps; identity needs visual verification before gameplay import.
- MAP-A genuine soil-pad alignment leaves only A1 within Oak Guardian's 2.4 RU range of the authored road. Other distant pads support Essence Flower but may provide poor combat utility. Balance is unchanged; this must be playtested before final acceptance.
- Chochlik source walk poses overlap on a concept sheet and lack complete attack/death frames. Candidate authored walk crops require rendered review; no limbs or new frames were invented.
- Functional UI now uses standard uGUI rectangles/text and approved portraits. A complete approved decorative UI art pack is not present in the verified MAP-A sources.
