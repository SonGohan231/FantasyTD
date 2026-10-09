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
| Unity compile | successful Unity Executor baseline job on this head | PENDING |
| Layer integrity | successful map-a-integrity on this head, machine-readable report | PENDING |
| Gameplay | genuine PlayMode waves 1–3+, tower attack, 2+ enemy types, victory/defeat/restart | PENDING |
| Android | successful Android build on this head | PENDING |
| Screenshots/GIF | PNG/GIF captured from real Unity Game View/runner; no mockups | PENDING |
| Physical device | device install/touch QA | OPTIONAL, PENDING |

## Known risks and limitations

- Earlier T0003–T0008 passes from prior branch do not automatically prove this change.
- Real on-device screenshot/GIF may require a display-capable Unity runner rather than `-nographics` batchmode.
- CI logs, artifacts, screenshots and GIF have not yet been verified on this branch. The first PR workflow invocation failed in its prepare job before Unity and requires investigation.
- Leave this PR in DRAFT, do not merge or mark mission DONE while any mandatory gate is missing.
