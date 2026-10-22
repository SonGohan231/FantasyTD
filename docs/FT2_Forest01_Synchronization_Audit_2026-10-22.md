# FantasyTD — synchronization audit (2026-10-22)

## Reconciled locations
| Source | Revision | Interpretation |
|---|---|---|
| GitHub main | 17689b8 | Default branch, not changed |
| PR #27 | 46fa121 | Existing 12-site 3-wave V2 Forest01 |
| PR #28 | b85a1fe | New ten-unit artwork and runtime changes on separate branch stack |
| PR #31 | 8110d57 | Artwork importer repair based on PR #28, not yet in terrain |
| PR #33 | 155b95c | Separate 28x20 terrain generator, source-level render, core docs |
| PR #34 | 03ed60f | Portable QA and advisory visual workflow; PR #33 ancestor |
| Historical local MAP1 | 91a625a | User worktree preserved |
| Local TERR-01 | 604200f | Unity-generated scene and failed smoke preserved |
| Synced integration | 03ed60f + site script correction | New worktree, isolated from old projects |

## Completed, with scope of evidence
- Legacy MAP-A: existing Unity evidence for 12 planting spots, 4 plant mechanics, basic enemies, boss, 6 waves, UI 16:9/20:9 and a real historical APK (T0008 run 37001334070). No confirmed physical Android test.
- Existing V2 Forest01: 12-site / 3-wave playable prototype with historical source and tests. This is distinct from the new terrain.
- Terrain: 28x20=560 ground cells, one 40-cell road, 18 pond cells, 10 planned sites, 2 gates, bridge, vegetation. Geometry contract checks are green.
- Real Unity/FEX compiled and generated/opened terrain scene. The test FAILED due to 10 site components not persisting after serialization; screenshot and PASS marker were NOT produced.
- A focused regression test reproduced the missing dedicated MonoBehaviour script file, then passed after splitting BuildSite and Interaction into matching source files. 7/7 static Forest01 tests pass; Unity revalidation still required.
- PR #34 quality gate success proves portable geometry/core/unit checks only, not Unity gameplay, renderer or Android.
- Approved artwork reference illustrations are not a production-grade Tilemap atlas. Ten additional unit assets exist in a different unmerged PR #28/#31 branch stack.

## Outstanding work, in strict order
1. TERR-01: actual Unity smoke on integrated SHA, saved/reloaded scene with ten site components, explicit PASS and real 1280x720/1600x720 PNG outputs.
2. TERR-02/TERR-03: production grass, road, water tile atlases, lighting, decoration, depth and legibility.
3. TERR-04: adapter between 40-cell path and 10 sites and existing deterministic battle simulator.
4. TERR-05: real playable 3-wave loop: placing plants, enemies, costs, attacks, UI, win/lose, restart.
5. TERR-06: reproducible new Android APK, physical install, input and FPS/memory.
6. TERR-07: plant upgrades and selling, additional HUD quality, accessibility and visual acceptance.
7. PR #28/#31 compatibility review; isolate source images from runtime and prevent asset import regressions.

## Environment and safety
- ARM64/FEX Unity is experimental and slow. Unity log in the earlier TERR-01 worktree proves actual scene generation, not completion.
- GitHub hosted Mac Unity runner requires secrets; previous preflight failed, self-hosted jobs remain queued.
- Owner forbids the dyrygent-unity runner; do not reactivate it.
- Pre-sync copies include modified AGENTS.md/STATE.md and Unity-generated meta files. These worktrees were preserved, not reset.
- New integration worktree is based on all PR #33 and #34 commits; no PR was merged into main.
- Do not claim completion without evidence from the same tested Git SHA.
