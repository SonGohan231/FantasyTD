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

Status: **ACTIVE**

Purpose:
Validate Pędostrzał, Kwiat Esencji, Dębowy Strażnik and Mroźny Pąk individually against acceptance criteria.

Do not add other towers.

---

# T0005 — Enemy Runtime and Wave Flow

Status: **PLANNED**

Purpose:
Validate Chochlik, Wichrowy Skrzat, Ogr Zarośli, Żelazny Goblin, wave sequencing, base leaks, and rewards.

Boss validation may be split into T0006 if this ticket becomes too large.

---

# T0006 — Boss, Win/Lose, Restart

Status: **PLANNED**

Purpose:
Validate Król Cierni, wave 6 completion, victory, defeat, and restart.

---

# T0007 — Android UI and Device Layout

Status: **PLANNED**

Purpose:
Validate 16:9, 20:9, safe area, touch target sizing, full-map framing, and mobile usability.

---

# T0008 — Android Build

Status: **PLANNED**

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
