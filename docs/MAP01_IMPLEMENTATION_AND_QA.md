# MAP-01 playable Unity Tower Defense — implementation and evidence

Mission: Agent OS `71eecb09-5c55-488a-8ed8-ea5c2dc493e4`
PR: https://github.com/SonGohan231/FantasyTD/pull/20
Target: Unity 6000.0.26f1, Android landscape
Status: **IN PROGRESS — NOT ACCEPTED** until target branch CI, screenshots and real gameplay pass.

## Roles and handoffs

1. Architect: preserve the verified MAP-A domain model (24×14, 12 planting sites, existing scene and UI).
2. Map art: deterministic colored meadow tiles, worn path shoulder, foliage; original runtime sprites without external copyright.
3. Tilemap builder: Unity Grid + 4 Tilemaps: Ground, PathEdge, Foliage, SolidObstacles.
4. Collision/nav: TilemapCollider2D on collision-only layer; geometry checks for enemy path and occupied terrain.
5. Enemy integrator: existing GameSession / EnemyUnit / WaveDirector / FantasyVisualFactory — retained unchanged; existing 4 regular types + boss, six waves.
6. Engine integrator: MapABootstrap.BuildMap invokes MapATilemapLayers.Build before scenery; project manifest enables built-in Tilemap module.
7. QA: MapAIntegrityValidator verifies 336 ground cells, four Tilemap layers, collider existence, waypoint navigation and planting clearance. Execution-dependent gates listed below.

These are responsibilities; Agent OS actually routed only the available qualified agents, with host handoffs for capacity gaps. A role description is not proof an independent agent performed the work.

## Scope and implemented behavior

- 24×14 logical world preserved. Existing road renderers, enemy waypoint route, portal, base, plant spots, four towers, economy and touch HUD are retained.
- MAP-A's existing WaveDirector has six waves (at least three required), normal enemies Wanderer, Runner, Brute, Armored and boss. No rebalancing introduced; no independent gameplay success claim on this new head.
- Background color tiles are deterministic to avoid seams across loads. Path-edge and foliage cells use route clearance. Collision tiles appear only around existing obstacles, not across the enemy lane; collider geometry is physically present without a visible overlay.
- Navigation assertions check every consecutive waypoint, segment clearance from obstacles, and each existing planting coordinate against blocked regions. These assertions are executed in the editor-side map integrity test.

## Changed files

1. `UnityProject/Assets/FantasyTD/Scripts/MapATilemapLayers.cs` — runtime art Tilemaps, obstacles, navigation checks.
2. `UnityProject/Assets/FantasyTD/Scripts/MapATilemapLayers.cs.meta` — asset GUID.
3. `UnityProject/Assets/FantasyTD/Scripts/MapABootstrap.cs` — real map initialization.
4. `UnityProject/Assets/FantasyTD/Editor/MapAIntegrityValidator.cs` — engine integration assertions.
5. `UnityProject/Packages/manifest.json` — Tilemap module.
6. `.github/workflows/unity-executor.yml` — feature branch CI trigger.
7. `docs/MAP01_IMPLEMENTATION_AND_QA.md` — this report.
8. `.dyrygent/unity-request.json` — pending GitHub Actions execution requests.

## Acceptance matrix — evidence required

| Gate | Required proof | Current state |
|---|---|---|
| Branch and source commits | GitHub commit SHA and changed C# diff | PRESENT |
| Draft PR | live GitHub PR with actual source diff | PRESENT: #20 |
| Unity compile | Unity Executor baseline run 37889510605 | PASS on preceding source head; rerun if critical code changes |
| Layer integrity | Unity MAP-A integrity run 37889816351 | PASS |
| Gameplay | Unity PlayMode waves 1–5 run 37890751455; plant combat, victory/defeat separately | PARTIAL: WAVES 1–5 PASS, OTHER GATES PENDING |
| Android | successful Android build on this head | PENDING |
| Screenshots/GIF | Unity rendered 3 PNG frames in Play Mode, run 37891025944; GIF assembled from authentic frames, provided in conversation | 3 PNG PASS; GIF produced as separate ChatGPT attachment |
| Physical device | device install/touch QA | OPTIONAL, PENDING |

## Known risks and limitations

- Earlier T0003–T0008 passes from prior branch do not automatically prove this change.
- Real on-device screenshot/GIF may require a display-capable Unity runner rather than `-nographics` batchmode.
- CI logs, artifacts, screenshots and GIF have not yet been verified on this branch. The first PR workflow invocation failed in its prepare job before Unity and requires investigation.
- Leave this PR in DRAFT, do not merge or mark mission DONE while any mandatory gate is missing.

## Verified workflow evidence on feature branch

- Unity baseline/compile PASS — https://github.com/SonGohan231/FantasyTD/actions/runs/37889510605 — artifact `unity-executor-baseline-37889510605`.
- Unity Tilemap/geometry/collision/route checks PASS — https://github.com/SonGohan231/FantasyTD/actions/runs/37889816351 — artifact `unity-executor-map-a-integrity-37889816351`.
- Unity Play Mode wave flow first run FAILED on Wanderer measured speed 0.885 versus 0.8 ± 0.08 — https://github.com/SonGohan231/FantasyTD/actions/runs/37890091178. Five waves spawned and completed; test measurement window (0.5 game-seconds at 16x speed) was too short. Increased test sampling to 2.5 game-seconds, no gameplay balance changes.
- Unity Play Mode enemy flow waves 1–5 PASS after fix — https://github.com/SonGohan231/FantasyTD/actions/runs/37890751455.
- Real in-engine GPU frame captures PASS — https://github.com/SonGohan231/FantasyTD/actions/runs/37891025944 — artifact `fantasytd-map01-real-gameplay-37891025944`, 3 × actual 1280×720 PNG screenshots captured in Unity 6 Play Mode under Xvfb. The GIF is assembled from these three frames externally and made available as a ChatGPT download; continuous live-video evidence is not claimed.

## Pending acceptance

- Boss wave 6, win/lose/restart validation and Android APK build on final head.
- Mobile/tower behavior regression on this branch; historical T0003/T0004 on older heads are not substitutes.
- Final PR independent review; keep PR **DRAFT** and Agent OS mission not DONE unless all required evidence exists.
