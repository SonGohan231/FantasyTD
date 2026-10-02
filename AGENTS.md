# AGENTS.md — Fantasy TD

This file is repository-level law for coding agents.

## Working model

- ChatGPT is the user-facing project manager and game-design coordinator.
- Dyrygent may review architecture, ticket scope, debugging evidence, QA plans, and completion reports.
- Codex is the repository implementation agent.
- Unity Editor / Android build tooling are the source of truth for compilation, Play Mode, build, and device behavior.
- The user owns product decisions and visual/gameplay approval.

Dyrygent reviews are advisory. Do not change scope only because Astra suggests something. Scope changes require the active ticket or an explicit user/PM decision.

## One-ticket rule

Implement exactly one active ticket at a time.

Do not:
- implement future-ticket features,
- silently widen scope,
- refactor unrelated systems,
- rewrite architecture because a different pattern seems cleaner,
- add packages unless the active ticket requires them,
- change balance unless the active ticket explicitly allows it,
- fix unrelated discoveries automatically.

If you discover an issue outside the active ticket:
1. document it in `docs/Known_Issues_And_Followups.md`,
2. mention it in the completion report,
3. do not fix it unless it blocks the active ticket.

## Project identity

Fantasy TD is a 2D mobile tower-defense game.

Target:
- Unity 6 LTS,
- Android,
- landscape,
- touch-first,
- 60 FPS target for the vertical slice.

Current milestone:
- MAP-A Android v2 vertical slice.

## Gameplay direction

The game uses:
- fixed planting/build positions,
- winding enemy paths,
- waves,
- strategic tower composition,
- upgrade specialization later in development,
- map geometry as part of balance.

Current MAP-A is intentionally small. Do not add MAP-B, MAP-C, campaign content, Endless, Reverse, heroes, or full upgrade trees until MAP-A is verified.

## Art direction lock

The game is colorful, organic fantasy.

Use:
- living plants,
- roots,
- leaves,
- flowers,
- mushrooms,
- thorns,
- frost,
- pollen,
- natural magic,
- goblins,
- imps,
- ogres,
- forest creatures,
- hand-painted/cartoon readability.

Do not introduce:
- science-fiction weapons,
- lasers,
- reactors,
- drones,
- futuristic turrets,
- cyberpunk UI,
- technological super-weapons,
- military realism.

Do not copy protected characters, names, audio, or artwork from existing games.

## Current MAP-A requirements

MAP-A Android v2 requires:
- logical size 24 × 14 RU,
- 1 RU = 1 Unity world unit,
- one enemy path,
- 12 fixed planting spots,
- no free placement,
- no visible gameplay grid,
- touch selection of planting spots,
- radial build menu,
- 4 plants,
- 4 basic enemy types,
- 1 boss,
- 6 waves,
- Essence economy,
- 20 base HP,
- victory,
- defeat,
- restart,
- Android landscape UI.

## Current plants

Player-facing names:
- Pędostrzał
- Kwiat Esencji
- Dębowy Strażnik
- Mroźny Pąk

Do not rename them during implementation tickets unless the ticket explicitly asks for naming changes.

## Current enemies

Player-facing names:
- Chochlik
- Wichrowy Skrzat
- Ogr Zarośli
- Żelazny Goblin
- Boss: Król Cierni

## Sources of truth

Long-term design:
- `docs/Full_Design_Document.md`

Current vertical-slice technical design:
- `docs/MVP_Technical_Design.md`

Active work:
- `docs/Tickets.md`

Observed repository status:
- `docs/Repo_Current_State.md`

Manual QA:
- `docs/Manual_Verification_Guide.md`

Known problems:
- `docs/Known_Issues_And_Followups.md`

Balance:
- `ProjectData/Tower_Defense_Design_Balance_v1_0.xlsx`

Reusable prompts:
- `docs/Codex_Prompt_Playbook.md`
- `docs/Codex_Ticket_Handoff_Template.md`

## Evidence rules

Never claim that:
- Unity compiled,
- a scene opened,
- Play Mode passed,
- a touch interaction worked,
- an Android build succeeded,
- an APK exists,
- an APK launched on device,

unless that exact action was actually performed and observed.

Prefer evidence in this order:
1. reproducible runtime/build result,
2. Unity Console / Build Report,
3. Unity Editor state,
4. serialized project configuration,
5. direct first-party code evidence,
6. static inference.

Clearly label unverified assumptions.

## Unity ticket workflow

For implementation/runtime tickets, when the tools are available:

1. Read this file and the active ticket.
2. Read `docs/Repo_Current_State.md`.
3. Inspect current Git status/branch.
4. Open the Unity project root at `UnityProject/`.
5. Let import/compilation finish.
6. Record baseline Console messages before clearing anything.
7. Perform only the active ticket.
8. Recompile.
9. Run ticket-specific manual verification.
10. Check Console again.
11. Update repo-state and known-issues docs.
12. Return the completion report.

## Branch rule

Use one branch per ticket when Git is available.

Preferred pattern:

`codex/tXXXX-short-description`

Examples:
- `codex/t0001-unity-baseline`
- `codex/t0002-map-a-integrity`

Do not merge branches automatically unless explicitly asked.

## Required completion report

At the end of every ticket provide:

- Ticket ID
- Status: PASS / PASS WITH ISSUES / FAIL / BLOCKED
- Summary of changes
- Files changed
- Commands/actions run
- Unity compile result
- Tests actually run
- Manual verification result
- Console/build evidence
- Risks
- Newly discovered issues
- Documentation updated
- Recommended next ticket

Keep reports factual. Do not mark unrun checks as passed.
