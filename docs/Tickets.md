# Tickets

## Status rules

Allowed statuses:
- ACTIVE
- PLANNED
- BLOCKED
- DONE
- CANCELLED

Only one implementation ticket should be ACTIVE at a time.

Do not implement PLANNED tickets unless the PM/user explicitly activates them.

---

# T0001 — Unity Project Baseline

Status: **DONE**

## Goal

Open the existing Unity project and establish an evidence-based technical baseline.

This ticket is diagnostic only. Do not implement gameplay changes.

## Dependencies

None.

## Allowed areas

Read:
- entire repository
- Unity project configuration
- Unity Console
- Build Settings

Write only when necessary:
- `docs/Repo_Current_State.md`
- `docs/Known_Issues_And_Followups.md`

Avoid source-code changes unless a trivial blocker prevents the project from opening and the user/PM explicitly approves remediation.

## Do not touch

- gameplay balance
- map geometry
- plant behavior
- enemy behavior
- wave definitions
- visuals
- architecture
- packages
- Android Player Settings
- build target
- scenes/prefabs unless Unity creates expected generated state automatically

## Requirements

1. Identify the actual Unity root.
2. Confirm Unity Editor version used to open the project.
3. Inspect `Packages/manifest.json`.
4. Let Unity import and compile.
5. Preserve/record baseline Console errors and warnings.
6. Confirm whether `MAP_A_ANDROID_V2.unity` is generated/present.
7. Confirm whether MAP-A is enabled in Build Settings.
8. Record Git status and current branch if Git exists.
9. Record available Android tooling without changing build target.

## Non-goals

- no Play Mode feature validation
- no gameplay fixes
- no UI redesign
- no Android build
- no package migration
- no new tests

## Acceptance criteria

- Unity project root confirmed.
- Unity Editor version recorded.
- Compile state recorded.
- Console error/warning baseline recorded.
- MAP-A scene presence recorded.
- Build Settings state recorded.
- Android module/tool availability recorded if inspectable.
- Repo-state document updated with evidence.
- Known issues updated.
- No unrelated implementation performed.

## Manual verification

1. Open `UnityProject/` in Unity.
2. Wait until import/compilation is idle.
3. Open Console.
4. Record errors and warnings.
5. Verify scene path:
   `Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity`
6. Inspect Build Settings / Build Profiles.
7. Do not enter Play Mode.
8. Do not clear Console before capturing baseline.

## Completion report

Return:
- Status
- Unity version
- compile result
- Console errors
- Console warnings
- MAP-A scene state
- Build Settings state
- Android tooling state
- Git branch/status
- files changed
- blockers
- recommended next ticket

---

# T0002 — MAP-A Scene and Geometry Integrity

Status: **DONE**

Purpose:
Validate the generated scene, path, spawn/base, 12 planting spots, map bounds, and editor validation command.

Activation condition:
T0001 completed and compile blockers resolved.

Do not implement mobile UI or combat in this ticket.

---

# T0003 — Mobile Planting and Radial Menu

Status: **DONE**

Purpose:
Validate touch selection of all 12 spots, radial-menu placement, edge clamping, cost availability states, and safe interaction with UI.

Do not implement new plants or upgrades.

---

# T0004 — Four Plant Behaviors

Status: **DONE**

Purpose:
Validate Pędostrzał, Kwiat Esencji, Dębowy Strażnik and Mroźny Pąk individually against acceptance criteria.

Do not add other towers.

---

# T0005 — Enemy Runtime and Wave Flow

Status: **DONE**

Purpose:
Validate Chochlik, Wichrowy Skrzat, Ogr Zarośli, Żelazny Goblin, wave sequencing, base leaks, and rewards.

Boss validation may be split into T0006 if this ticket becomes too large.

---

# T0006 — Boss, Win/Lose, Restart

Status: **DONE**

Purpose:
Validate Król Cierni, wave 6 completion, victory, defeat, and restart.

---

# T0007 — Android UI and Device Layout

Status: **DONE**

Purpose:
Validate 16:9, 20:9, safe area, touch target sizing, full-map framing, and mobile usability.

---

# T0008 — Android Build

Status: **DONE**

Purpose:
Configure/verify Android build prerequisites and create a real APK.

Expected artifact:
`Builds/Android/FantasyTD_MAP_A_v2.apk`

---

# T0009 — Android APK Smoke Test

Status: **PLANNED**

Purpose:
Install/launch the APK on an emulator or physical Android device and perform the smallest useful runtime smoke test.

---

# T0010 — Balance v1.1 Playtest Pass

Status: **PLANNED**

Purpose:
Use actual runtime evidence to revise only the MAP-A vertical-slice balance.

Do not expand the roster.

---

# T0011 — MAP-A Visual Production Pass + Release APK

Status: **DONE**

Purpose:
Replace prototype-looking MAP-A visuals with a cohesive original organic-fantasy production pass while preserving validated gameplay, balance, path geometry, planting coordinates, touch behavior, wave flow and combat values.

Scope:
- authored-looking terrain/path dressing without changing gameplay geometry,
- polished silhouettes for all 4 plants,
- polished silhouettes for all 4 basic enemies and Król Cierni,
- readable projectile/control VFX,
- cohesive HUD, radial menu, planting indicators and victory/defeat presentation,
- retain Android landscape support at 16:9 and 20:9,
- add ticket-specific render-capable Play Mode visual smoke validation when practical,
- create a final Android APK from the final T0011 head.

Constraints:
- no balance changes,
- no wave changes,
- no map-coordinate/path/spot changes,
- no new gameplay roster or upgrade systems,
- no T0009 device smoke test,
- no T0010 balance pass,
- no automatic merge.

Acceptance:
- gameplay invariants remain unchanged,
- all plants/enemies/boss are visually distinct and readable,
- MAP-A no longer reads as debug/prototype geometry,
- UI remains touch-safe and unclipped,
- Unity compile/Play Mode validation passes with no introduced errors,
- final Android BuildReport succeeds on Unity 6000.0.26f1,
- final APK artifact is downloadable with recorded checksum.

Completion evidence:
- render-capable mobile layout regression: run `37059744377` — PASS,
- post-fix MAP-A integrity: runs `37061361432` and `37104978893` — PASS,
- plant behavior regression after VFX integration: run `37062061398` — PASS,
- boss/victory/defeat/restart regression: run `37105044037` — PASS,
- Android candidate build: run `37060044752` — BuildReport Succeeded, 0 errors, 0 warnings,
- final Android build is requested only after these documentation commits so the APK evidence comes from the final branch HEAD,
- T0009 and T0010 remain PLANNED and were not implemented.


## T0011 completion evidence

- render-capable mobile layout regression: run `37059744377` — PASS at 1920×1080, 2400×1080 and synthetic safe area
- corrected MAP-A geometry regression: run `37104978893` — PASS
- four plant behaviors after VFX integration: run `37062061398` — PASS
- boss / victory / defeat / restart regression: run `37105044037` — PASS
- corrected Android build: run `37105211817` — PASS
- corrected Android build head: `98539046e3cab8ff24572d6b401cd61579f895b0`
- Unity: `6000.0.26f1`
- BuildReport: `Succeeded`, errors `0`, warnings `0`
- APK: `Builds/Android/FantasyTD_MAP_A_v2.apk`
- APK bytes: `18888441`
- APK SHA-256: `aaccb09c2e8a358eeaf814a4f07d110e770ebf7bb5d8d547dc3d54f52673b6f1`
- artifact: `fantasy-td-android-37105211817` (ID `11267512151`)
- T0009 device/emulator smoke: **NOT RUN**
- T0010 balance pass: **NOT STARTED**
- no automatic merge


---

# T0012 — MAP-A Actual Playable Release

Status: **ACTIVE**

Purpose:
Replace the rejected tech-demo presentation with the real Higgsfield MAP-A artwork and complete the minimum gameplay interactions expected from a playable tower-defense level.

Authorized by user after direct APK review.

Scope:
- bundle and render the existing Higgsfield MAP-A environment from run `36991331980`, artifact `11219593382`,
- align the invisible enemy route and 12 active planting zones to the visible road/soil patches in that artwork,
- keep the map grid invisible,
- add readable HP bars to every enemy and a larger boss bar,
- add visible slow/root state cues,
- tapping an occupied tower opens a tower panel,
- tower panel shows name, level, concise stats, upgrade cost and sell refund,
- add exactly three tower levels for the four existing plants; L1 keeps existing validated base values,
- add tower selling with 70% floor refund of total Essence invested,
- selling clears the planting spot and allows rebuilding,
- upgrade immediately affects production behavior and updates a visible level marker,
- use existing Higgsfield character assets where available and materialize them through the same build pipeline; missing character art may be generated through the existing prepaid Higgsfield API bridge,
- add T0012 Play Mode validation for HP UI, tower selection, upgrade, sell/refund and rebuild,
- rerun affected existing gameplay/layout regressions,
- create a final Android APK from final T0012 head.

T0012 upgrade values:
- Pędostrzał: L1 22 dmg / 1.0 s; L2 30 dmg / 1.0 s; L3 40 dmg / 0.85 s; range 3.2
- Kwiat Esencji: L1 +20 / 20 s; L2 +25 / 20 s; L3 +30 / 18 s
- Dębowy Strażnik: L1 8 dmg / 1.75 s root / 5.0 s; L2 12 / 1.9 / 5.0; L3 16 / 2.1 / 4.5; range 2.4
- Mroźny Pąk: L1 18 dmg / 35% slow / 1.0 s; L2 24 / 37.5% / 1.0; L3 30 / 40% / 0.9; slow duration 2.5 s; range 3.2
- L1→L2 upgrade cost = nearest 25 to 75% of base cost
- L2→L3 upgrade cost = nearest 25 to 100% of base cost
- sell refund = floor(70% × total Essence invested)

Explicitly allowed geometry change:
The old prototype MAP-A route/spot coordinates may be replaced only as needed to align gameplay with the visible Higgsfield road and planting patches. This is an art-to-gameplay alignment correction, not T0010 balance tuning.

Constraints:
- no new plant/enemy roster,
- no branch/final upgrade tree,
- no T0009 device-smoke implementation,
- no T0010 balance-playtest pass,
- no automatic merge.
