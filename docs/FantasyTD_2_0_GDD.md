# FantasyTD 2.0 — Game Design Document (GDD)
Status: DESIGN BASELINE v0.1 · 2026-10-09
Owner: FantasyTD product · Repository: SonGohan231/FantasyTD
Source of authority: User directive 2026-10-09 to replace MAP-A-only scope with a complete professional Android tower-defense game. This GDD proposes a new product; it is NOT proof of implementation.

## Vision
An original, offline-first, premium-feeling, hand-painted 2D landscape Android tower-defense game: organic magical defenders vs invading fantasy creatures. Strategic clarity and playful units are inspirations, not copied IP. Keep the "living fantasy" theme, recognizable silhouettes, rich effects and satisfying feedback. Keep control simple on a phone, decision depth substantial.

## Product promise / release scope
- 30 authored campaign levels in six worlds (five per world), with replay medals and Normal/Hard/Veteran difficulty.
- 12 distinct buildable defender families, 3 upgrade tiers plus two mutually exclusive final specializations per family (24 specialization endpoints); every family has utility and counters.
- 24 distinct ordinary enemy archetypes, six bespoke bosses (one/world), three playable heroes, four player spells, five curated environmental mechanic families.
- Intro/tutorial, campaign map, polished menus, pause/2x speed, codex, achievements, challenges, optional Endless after campaign, reliable local save.
- Works fully offline after install; no pay-to-win, account mandate, intrusive ads, or backend dependency. No online economy needed; Railway/Neon/WP Agent are not gameplay dependencies.
- Android landscape 16:9–20:9 safe-area adaptation; target 60 FPS on a documented mid-range Android test device, 30 FPS as minimum supported runtime threshold under stress; no invented performance claims.
- Target initial APK/AAB budget under 200 MB; measured, not presumed. Google Play compliance, accessibility, privacy and legal/IP audit before distribution.

## Core gameplay loop
Scout the winding road, routes and tactical planting points -> select build/upgrade -> commit Essence -> choose wave timing -> trigger abilities / reposition hero -> collect rewards -> adapt to armor, flying, burrowing and resistances -> survive -> earn medals/upgrades -> advance campaign. Combat is real-time with pause and optional 2x speed.

## Identity and differentiation
Avoid literal PvZ lanes, Kingdom Rush characters/buildings, copied UI, audio, naming, sprites, card art or level layouts. Distinctive mechanics: a Living Terrain system (limited in-level interactions with roots, fungal bridges, moving flora), Essence economy with strategic opportunity cost, plant morph specialization, and readable elemental interactions. Fixed authored planting nodes; no free placement by default.

## Six worlds
1. 01–05 Zielona Dolina: single road, fork preview, economy, intro boss "Zarządca Korzeni".
2. 06–10 Nocny Las: stealth/reveal and ambush, boss "Sowa Zmierzchu".
3. 11–15 Zalane Ruiny: timed flood/bridges, amphibious lanes, boss "Bagienny Kolos".
4. 16–20 Mgielne Mokradła: fog-of-war, flyers/spotters, boss "Królowa Oparów".
5. 21–25 Cytadela Cierni: barricades, siege and split lanes, boss "Generał Kolców".
6. 26–30 Pęknięty Świat: multi-entry, living terrain, combined counters, final boss "Serce Rozłamu".
Boss names are working originals; validate uniqueness before release. Every level must have authored waypoint paths, tower nodes, wave datasets, scripted event hooks and per-level win/lose telemetry. No duplicate maps with enemy HP multipliers alone.

## 12 defender families (names provisional)
01 Pędostrzał — single-target physical sustained DPS.
02 Kwiat Esencji — income and harvest timing.
03 Dębowy Strażnik — roots, area block.
04 Mroźny Pąk — frost DPS/slow.
05 Grzyb Zarodnikowy — poison, damage over time.
06 Ostrokrzew — anti-armor piercing.
07 Paproć Burzowa — chain lightning with target caps.
08 Lilia Światła — reveal, anti-stealth support.
09 Dynia Strażnicza — wall/taunt, sustain.
10 Róża Płomienia — splash/fire and DOT.
11 Wierzba Wiatru — anti-air and knockback (boss-safe limits).
12 Kaktus Runiczny — sniper/long range, delayed shots.
Specializations branch into strategically different roles, not only +damage. Each tower includes asset specification: 3 world sprites/states, wind-up, attack, hit, idle, death/sell, upgrade FX, selection UI and consistent pivot/scale.

## Enemy roster and encounter design
24 archetypes, 4 new enemies per world. Taxonomy: runner, swarm, armored, resistant, stealth, flying, healer, summoner, shield, burrower, splitter, siege, elite and mixed role. Each enemy has a clear telegraph/silhouette, single documented counter, weakness, threat budget, movement rules and interactions. Bosses include at least two mechanics and 3 readable phases but avoid unavoidable damage. No invincibility loops, indefinite disabling, or unwinnable wave scripts.

## Heroes and spells
Three hero archetypes: melee guardian, ranged trickster, nature mage. One active hero at a time, smart pathing and touch reposition, three active abilities and cooldown displays. Four spells: root trap, storm burst, restorative bloom, temporary time dilation. Hero/spell damage bounded by level wave budgets; difficulty must not assume a specific hero.

## Data and balance (hypotheses, NOT verified)
Unity ScriptableObject or validated JSON content: TowerDefinition/UpgradeDefinition/EnemyDefinition/WaveDefinition/LevelDefinition/StatusEffectDefinition. Stable content IDs, schema versioning and migration. No hardcoded per-level rules inside view MonoBehaviours.
Balance metrics: effective DPS vs armor/coverage, area throughput, target acquisition, slow uptime and diminishing returns, lifetime Essence ROI, leak expectations, wave duration, hero contribution, win rate and tower diversity. Simulate each level at least 100 deterministic seeds per strategy archetype: starter-balanced, economy-first, control, burst, anti-air, low-spend. Instrument real playtests; simulations do not prove fun.
Provisional target: Normal 60–85% successful novice attempts after tutorial; Hard roughly 25–50% success for competent testers; no mandatory single tower/specialization. Median campaign clear time determined by playtests rather than asserted. No hidden adaptive difficulty altering combat outcomes mid-wave.
Define threats via explicit weighted point budgets (HP × resistance × speed × path exposure × special ability multipliers) and verify against available resources + node coverage. Boss resistances cap hard crowd-control to avoid stun-lock.

## Architecture
Unity 6 (current 6000.0.26f1 baseline) Android landscape. Preserve existing working project and executor until migration tests pass; create a V2 namespace/folder and assembly boundaries instead of deleting legacy code upfront. Simulation core: tick-driven reproducible game state, deterministic random seed, events/commands, separate render/FX/UI. Components: LevelLoader, RouteGraph, TowerPlacement, TowerCombat, WaveSpawner, EnemyMovement, CombatResolver, Economy, Abilities, HeroController, EncounterDirector, ProgressionSave, Audio/VFX, Localization, Telemetry, ConfigValidator. Pools for bullets/enemies/FX. Avoid per-unit allocations in Update. Testable headless simulation separate from Unity UI.
Content pipeline must import licensed/owned source artwork with manifest including license, provenance, generated prompt/job ID, dimensions, sprite pivot, atlas and animation clips. Do not include assets from PvZ or Kingdom Rush.

## Visual/audio quality bar
Art bible: organic bright fantasy with painterly modular terrain, animated vegetation, readable road and planting zones, distinct silhouettes at phone resolution, light-shadow hierarchy and controlled VFX clutter. Choose coherent 2D production pipeline (spritesheets + layered rig) rather than stylistically mixed AI images. Minimum animation states for every unit. Visual QA screenshots at 16:9 and 20:9 and on-device screen. Fully owned/licensed SFX and looping original music (at least one track per biome plus combat/boss stingers). Accessibility: reduced motion option, readable contrast, non-color-only enemy status cues, configurable music/SFX.

## Android acceptance / evidence
All builds must come from reproducible Unity Editor/Android toolchain, pass import/compile, EditMode, PlayMode, battle simulations, content schema and save migrations. CI artifacts include logs, report, commit SHA, Unity version, APK hash; device emulator/real phone install launch, touch, pause/resume, background/foreground, orientation, game-over/restart, 30-stage progression, memory/perf trace and crash-free soak. Do not claim real-device pass based on headless Unity.
Release gate: all 30 levels implementable and actually playable, no blocking defects, all bosses/abilities/UI/audio wired, progression robust across cold starts, Android distribution signing checked. Incomplete scope = IN PROGRESS, never "finished" by document-only tests.

## Production sequencing / non-negotiables
1. M0 audit and architecture contract (preserve old branch/binaries; verified baseline).
2. M1 original vertical slice: first level complete with production-quality art/audio and fully tested Android APK.
3. M2 content/data pipeline, wave/balance automation, engine systems and two fully playable levels.
4. M3 6-world campaign waves/maps/bosses, 12 defenders, enemies, heroes, progression.
5. M4 polish, accessibility, audio, monetization-free offline UX and exhaustive device QA.
6. M5 reproducible signed release candidate APK/AAB, acceptance evidence and download handoff.
Independent agents may work simultaneously on design, 2D art concepts and tests, but merge implementation by ticket with dependency and architecture gates; never issue concurrent conflicting Unity file edits. Reject placeholders, auto-generated geometric sprites and menu-only shells as production completion.

## Current evidence boundary
Existing repository has MAP-A prototype and previous Unity Editor/PlayMode + Android build evidence cited in docs/Repo_Current_State.md, not device smoke. This GDD begins a new product. All new art, gameplay and release claims require fresh evidence from the V2 branch.
