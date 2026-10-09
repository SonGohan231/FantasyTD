# FantasyTD 2.0 — Production & Agent OS Execution Contract
Status: ACTIVE PLANNING · 2026-10-09
Authoritative design: docs/FantasyTD_2_0_GDD.md
Target: playable finished Unity 6 Android game, not mockups or unverified APK.

## Supersession and protection
The 2026-10-09 user directive supersedes the old MAP-A-only product *scope*. Do not erase or overwrite the original MAP-A implementation or history. Existing AGENTS.md one-ticket, evidence, clean branch and no-unverified-claims rules remain binding until an explicitly reviewed workflow amendment. New V2 code lives in separate namespace/folders; old source remains test/reference.

Branch strategy: this design branch -> reviewed design PR -> one code PR per milestone ticket -> integration with verifiable Unity evidence -> Android release candidate. An agent may plan parallel tickets but must not cause concurrent edits to the same scene/manifest/prefab file. Check Git branch/status before every write.

## Team: qualified, execution-enabled from Agent OS registry 2026-10-09
- Overall product integration: Game Engineering Manager (ec0b9e63-da2f-4eda-887b-14c6275c0156).
- Game architecture/design: Game Architect & GDD Designer (553cbebb-0dab-4064-a7d0-0a6dbe263f4c).
- C# Unity game execution: Unity C# Gameplay Engineer (28ecaed3-871a-4980-bc43-a6e786804b50).
- Original asset direction: Game Art & Content Lead (76465b38-481b-437c-bd3c-ba2f3f58da27).
- UI/game-feel: 2D Game UI/UX & Game Feel Engineer (3c58fb4f-a568-4780-bb5b-65d7028ef0f6).
- Animation: Animation, Rigging & Physics Engineer (b642fffd-8c85-498c-8769-c08e219feec3).
- Android build: Android APK/AAB Build & Signing Engineer (70c84315-9452-47f1-922e-5ba14df0e654).
- Mobile device QA: Android Device & APK QA Tester (62bdf581-946e-4b63-8507-3a3ed252d722).
- CI regression: Automated Game Regression & CI Tester (de4203f4-2f6b-4842-a0ea-3a6fc04743df).
- Exploratory game QA: Gameplay & Exploratory QA Tester (c0b6d47c-4b1b-4b4d-830a-037c9bcada1b).
- Release QA: Game Testing QA Lead & Release Gatekeeper (c674f2ad-a1a6-443b-9ab1-1f623666a825).
- Save migration: Save Data & Migration QA Tester (6303cbf3-b568-441d-bf62-1e7609012919).
These are explicit preferred role assignments. The default plan_mission router currently misroutes some 2D art, balance and Android-release tasks to fallback or evidence agents; REQUIRE capability/routing preflight and explicit reassignment before executing. Never assert a fallback planner produced art or APK. Do not auto-enable unqualified ephemeral agents.

## Work orders / DAG
FT2-001 M0: Audit current code, art, toolchains, asset licenses, scene generating, GitHub work evidence; provide inventory (depends: none; game architect + Unity).
FT2-002 M0: Design C# deterministic simulation core + schemas + scene/runtime migration plan; create V2 namespaces/assembly tests (depends: FT2-001; Unity engineer).
FT2-003 M0: Complete art bible + asset sheets and quality reference frames, ownership license manifest (depends: FT2-001; art lead, animation lead).
FT2-004 M1: Implement data validated L01 complete battle + enemies, 4 defenders, economy, placement, victory/defeat and pause (depends: FT2-002; Unity engineer).
FT2-005 M1: Integrate original animated L01 environment, units, UI and effects; touch HUD 16:9 + 20:9 (depends: FT2-003,FT2-004; art/UI/animation).
FT2-006 M1: Automate simulated wave/economy strategy benchmarks and EditMode/PlayMode tests (depends: FT2-004; C# + test engineer).
FT2-007 M1: Unity real Android build + emulator/device smoke + artifact/hash after L01 complete (depends: FT2-005,FT2-006; build engineer + independent QA).
FT2-008 M2: Implement generic upgrades/specializations, statuses, heroes/spells, save progression and content authoring pipeline (depends: FT2-007).
FT2-009 M3: Add 30 original maps (5 per biome), encounter waves, 24 archetype enemies, six bosses, 12 defender families, three heroes (depends: FT2-008). Split content per biome on independent data/art branches; integrate with tested single-owner changes.
FT2-010 M4: Polish original music/SFX, menus, tutorial, accessibility, economy/balance, difficulty, achievement/codex and Endless (depends: FT2-009).
FT2-011 M5: Independent full 30-stage regression and Android FPS/memory/touch/resume/install tests; fix blocking bugs (depends: FT2-010).
FT2-012 M5: Versioned Android release candidate APK/AAB, SHA256, build log, tested installation evidence, downloadable artifacts and final report (depends: FT2-011).

## Mandatory technical acceptance for *each* executable ticket
- Isolated branch, one active code ticket, committed diff, human-readable acceptance criteria and PR.
- Unity import/compile + editor console evidence, affected EditMode/PlayMode tests + relevant actual runtime behavior.
- Original owned/licensed sprites, animation and SFX or documented blocking work; no geometric placeholders as finished quality.
- Per-event diagnosis. On failure: preserve successful subtasks/checkpoint, send defect to independent debugger, fix, resume from failed checkpoint. Never reset the entire mission because one subtask failed; do not treat unexecuted steps as failed.
- Independent verifier cannot be same execution agent.
- Each milestone response: mission ID, run IDs, GitHub commits/PRs, source files, build/test log IDs, APK artifact and SHA256 if generated, remaining blockers.
- No claims of background completion unless verified status from Agent OS, Unity Executor or GitHub Actions actually confirms it.

## Quality gates
G0 Architecture: simple Unity scene loads; zero C# compiler errors, deterministic core tests with repeatable seeds.
G1 Vertical slice: L01 plays start-to-finish with new original art and touch UX, Android APK builds and installs/launches in tester environment.
G2 System complete: economy and all core systems work, stable save and reliable data schema.
G3 Content complete: all 30 levels, 12 towers, 24 normal enemy archetypes, six bosses, three heroes, difficulty settings playable.
G4 Release: no progression-blocking bugs, full regression, on-device or marked explicitly unverified, performance/profiling evidence, original art/audio provenance, installable distributed APK/AAB.

## Service/tool policy
GitHub: canonical versioned source and PR evidence. Unity Executor: compile/build evidence. Dyrygent and Agent OS: coordinated execution and checkpoints. Higgsfield/Image Generator/LogoGenic: OPTIONAL original concepts and production assets only under approved credit/cost terms; no paid generation without quote/consent. Game Development Studio: use local CLI only if available and specifically authorized for file mutations; otherwise Unity asset pipeline. Railway/Neon: NOT required; offline game must not depend on paid hosted infrastructure. Engram: optional knowledge only, Git repo is authoritative. WP Agent: not game engine, may later support landing page only if expressly requested.

## First executable ticket
FT2-001 is ACTIVE after design branch creation; all other executable tickets PLANNED. Its acceptance requires a checked repo inventory and a concrete code migration plan, not claiming the new game exists.
