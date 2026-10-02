# Manual Verification Guide

## Purpose

A successful compile is not sufficient evidence that the game works.

Every ticket should have a short, explicit manual verification plan.

Use:
- Unity Console,
- Play Mode,
- Game View,
- Build Report,
- emulator/device evidence

as appropriate to that ticket.

Do not run the entire full-game checklist for every small change.

---

# Baseline principles

## Preserve baseline

Before changing code:
1. let Unity compile,
2. record existing errors/warnings,
3. do not clear Console before capturing them.

## After implementation

1. let Unity recompile,
2. run only relevant checks,
3. verify no new blocker Console messages,
4. update repo state,
5. report unrun checks as NOT RUN.

## Status vocabulary

- PASS
- PASS WITH ISSUES
- FAIL
- BLOCKED
- NOT RUN

---

# T0001 — Unity baseline

1. Open `UnityProject/`.
2. Wait until Editor import/compile is idle.
3. Record Unity version.
4. Record Console errors/warnings.
5. Verify MAP-A scene existence.
6. Inspect Build Settings / Build Profiles.
7. Record Android module availability if visible.
8. Do not enter Play Mode.

---

# T0002 — MAP-A scene/geometry

1. Open MAP-A.
2. Run:
   `Tools → Fantasy TD → Validate MAP-A`
3. Confirm validation output.
4. Confirm path visually exists.
5. Confirm spawn and base are at opposite route ends.
6. Confirm exactly 12 planting spots.
7. Confirm no normal gameplay grid is visible.
8. Confirm map is framed within Game View.
9. Check Console for new errors.

---

# T0003 — mobile planting/radial menu

For all 12 spots:
1. Enter Play Mode.
2. Tap/click empty planting spot.
3. Confirm the spot selects.
4. Confirm radial menu opens.
5. Confirm all four plant options appear.
6. Confirm menu does not leave screen bounds.
7. Close menu.
8. Repeat for edge spots.

Additional:
9. Attempt unaffordable purchase.
10. Confirm purchase is blocked.
11. Confirm tapping UI does not also select a world spot.
12. Confirm occupied spots cannot accept another plant.
13. Check Console.

---

# T0004 — plants

## Pędostrzał
1. Plant Pędostrzał.
2. Start a wave.
3. Confirm it targets only enemies within range.
4. Confirm target priority behaves as First.
5. Confirm projectile reaches target.
6. Confirm damage is applied.
7. Confirm physical resistance reduces damage where expected.

## Kwiat Esencji
1. Plant Kwiat Esencji.
2. Record current Essence.
3. Wait one income interval.
4. Confirm +20 Essence.
5. Confirm repeated interval behavior.

## Dębowy Strażnik
1. Plant Dębowy Strażnik.
2. Bring enemy into range.
3. Confirm root/control occurs.
4. Confirm movement resumes afterward.
5. Confirm boss control is reduced.

## Mroźny Pąk
1. Plant Mroźny Pąk.
2. Confirm damage.
3. Confirm visible/observable speed reduction.
4. Confirm slow expires.
5. Confirm boss slow is reduced.

---

# T0005 — enemies/waves

For each basic enemy:
1. Spawn through normal wave flow if possible.
2. Confirm movement from spawn to base.
3. Confirm speed differences are observable.
4. Confirm reward on death.
5. Confirm base damage on leak.
6. Confirm armored enemy physical resistance.

Wave flow:
7. Run waves 1–5.
8. Confirm sequence and counts.
9. Confirm next-wave button availability transitions correctly.
10. Check active enemy count HUD.
11. Check Console.

---

# T0006 — boss/states

1. Reach wave 6.
2. Confirm Król Cierni spawns.
3. Confirm boss durability/control rules.
4. Kill boss and remaining enemies.
5. Confirm victory.
6. Restart.
7. Intentionally leak enough enemies to reduce base to 0.
8. Confirm defeat.
9. Confirm restart from defeat.
10. Check Console.

---

# T0007 — mobile layout

Test at minimum:
- 1920×1080 or equivalent 16:9
- ~2400×1080 or similar 20:9
- safe-area/notch simulation if available

Verify:
1. full tactical map remains readable,
2. top HUD remains inside safe area,
3. wave button is reachable,
4. build menu at edge spots remains on screen,
5. no text overlaps,
6. touch targets are reasonably large,
7. no mandatory camera panning is required for MAP-A.

---

# T0008 — Android build

1. Verify Android Build Support.
2. Verify SDK, NDK, JDK availability.
3. Verify MAP-A is included in build.
4. Run Android build.
5. Capture Build Report status.
6. Confirm the actual file exists:
   `Builds/Android/FantasyTD_MAP_A_v2.apk`
7. Record file size.
8. Do not claim device success yet.

---

# T0009 — device smoke

On emulator or physical Android device:

1. Install APK.
2. Launch.
3. Confirm landscape.
4. Confirm no startup crash.
5. Confirm HUD is visible.
6. Tap a planting spot.
7. Build one plant.
8. Start wave 1.
9. Play through at least wave 2.
10. Confirm touch remains responsive.
11. Record obvious FPS/layout/crash issues.
12. If possible, inspect logcat for blockers.

---

# T0010 — balance playtest

Do not rebalance by intuition alone.

Record:
- towers purchased
- purchase timings
- Essence at key moments
- enemy leaks
- base HP
- wave where player fails/wins
- value of Kwiat Esencji
- observed slow/root usefulness
- whether one planting spot dominates
- whether one plant is mandatory

Use several runs before changing multiple variables.
