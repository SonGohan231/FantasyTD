# STATE — FantasyTD / Forest01 integration
Date: 2026-10-22. Status: PARTIAL, no product-release approval.

## Working baseline
- New clean worktree: /home/adambrdon/Repos/FTD/FantasyTD-Forest01-Synced
- Branch: codex/forest01-integrated-20261022 (forked from PR #34 commit 03ed60f).
- Covers both PR #33 (terrain) and PR #34 (portable CI/gates).
- Main branch remains unchanged. Existing worktrees were not touched.
- Previous local dirty changes backed up at /home/adambrdon/Repos/FTD/.sync-backups/FantasyTD-before-sync-20261022-1945.tar.gz.
- Primary project core: docs/CORE_FantasyTD_PELNA_EKSTRAKCJA_KR_PVZ_2026-10-22.md
- Visual scope: docs/FT2_Forest01_Approved_Production_Spec.md
- Geometry scope: docs/FT2_Forest01_28x20_Layout.md

## Actual Unity evidence
- Unity 6000.0.26f1 via FEX successfully compiled and generated/opened a separate Forest01_TerrainTest.unity (~278 kB) in the old Codex worktree.
- Smoke failed after scene reopening: InvalidOperationException: Forest01 Terrain: Exactly 10 separate build objects.
- No 16:9 or 20:9 Unity camera PNG was produced, no TERR-01 PASS.
- Ten GameObjects were serialized, but build-site C# MonoBehaviour was defined in a file named for the interaction controller.
- Integration fix separates both components into matching script assets with stable meta GUIDs.
- Static regression first FAILED, then Python Forest01 geometry/source tests 7/7 PASS. Real Unity retest NOT RUN after the fix.

## GitHub branches reviewed
- main 17689b8 — no merge.
- PR #33 155b95c — separate 28x20 terrain code and CORE.
- PR #34 03ed60f — advisory and portable quality gates, not Unity proof.
- PR #28 b85a1fe and PR #31 8110d57 — separate art/import stack, not integrated.
- Local historical MAP1 91a625a and terrain 604200f retained.

## Known boundaries and next actions
1. Run real Unity scene validation on the exact integration commit: count ten persistent build site components, collect explicit PASS, scene and two genuine camera PNGs.
2. Keep old 12-site Forest01 and MAP-A stable; connect 10-site terrain to BattleSimulator only after TERR-01.
3. Replace procedural terrain placeholders with approved editable production assets.
4. Implement production build/upgrade/sell, full gameplay UI loop, play mode regression tests.
5. Build a new Android APK, install and test touch/render/performance on physical hardware.
6. Review PR #28/#31 only when terrain is verified; no automatic merge to main.
7. Hosted macOS render remains blocked by Unity license secrets; dyrygent-unity runner is excluded by owner request.
