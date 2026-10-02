# Full Design Document — Fantasy TD

## 1. Product vision

Fantasy TD is a mobile 2D tower-defense game for Android.

The intended identity combines:
- strong readability and playful personality,
- colorful garden/organic fantasy,
- winding authored maps,
- fixed strategic planting spots,
- wave composition and tower specialization,
- map geometry as a meaningful balance variable.

The game should feel approachable on a phone but support deeper strategy through placement, counters, economy, control, and map-specific strengths.

This document describes the long-term direction. It is **not** an implementation checklist for the current milestone.

## 2. Platform and presentation

Target:
- Android
- landscape
- touch-first
- true top-down / readable 2D battle map
- whole tactical map visible without mandatory camera scrolling on benchmark maps

Visual language:
- hand-painted/cartoon
- colorful
- organic
- slightly humorous
- large, recognizable silhouettes
- exaggerated readable reactions
- natural terrain and fantasy creatures

Forbidden default language:
- hard sci-fi
- lasers
- reactors
- drones
- futuristic machines
- metallic/cyberpunk HUD
- technological super-weapons

## 3. Core gameplay loop

1. Inspect map and available planting spots.
2. Spend Essence to plant defenders.
3. Start or advance enemy waves.
4. Plants automatically attack/control enemies.
5. Enemies follow authored routes toward the base.
6. Kills reward Essence.
7. Economy plants trade short-term defense for long-term income.
8. The player reacts by building, specializing, and eventually upgrading.
9. Survive all waves to win.
10. Losing all base HP ends the level.

## 4. Map philosophy

Maps are authored, not tile-grid levels.

Logical world scale:
- 1 RU = one standard road width = one Unity world unit in the current prototype.

The rendered map should look natural. Gameplay data underneath may include:
- path splines/waypoints,
- planting spots,
- trap zones,
- barricade locations,
- water zones,
- elevation/high-ground zones,
- junctions,
- no-build scenery,
- special map interactions.

The normal player should not see a technical grid.

### Benchmark maps

Long-term balance is built around three benchmark map archetypes:

**MAP-A — Long Road**
- single route
- baseline DPS/economy/single-target benchmark

**MAP-B — Serpent**
- high curvature and overlap
- tests AoE, slow, piercing, and geometry scaling

**MAP-C — Divide**
- split routes that merge
- tests multi-target, coverage allocation, artillery, and route flexibility

Only MAP-A is currently being implemented.

## 5. MAP-A Android v2

Working name: **Zielony Trakt**

Core data:
- logical bounds: 24 × 14 RU
- route length: approximately 46.211 RU
- Spawn: (0, 11)
- Exit/Base: (24, 2)
- 12 fixed planting spots
- starting Essence: 300
- base HP: 20
- 6 waves
- boss in wave 6

Waypoints:
1. (0,11)
2. (5,11)
3. (10,11)
4. (13,9)
5. (13,6)
6. (10,4)
7. (5,4)
8. (5,2)
9. (10,2)
10. (16,2)
11. (22,2)
12. (24,2)

Planting spots:
- A1 (4,10), EPC 6.08
- A2 (11,9), EPC 7.60
- A3 (12,7), EPC 8.19
- A4 (9,5.5), EPC 6.47
- A5 (7,3), EPC 12.08
- A6 (12.5,3.5), EPC 10.58
- A7 (18,3.5), EPC 5.65
- A8 (21,4), EPC 5.00
- A9 (2.5,12.5), approximate EPC 5.20
- A10 (15,4.4), approximate EPC 6.40
- A11 (2.8,8.5), approximate EPC 5.10
- A12 (22.2,5.1), approximate EPC 4.80

EPC means Effective Path Coverage: approximate total road length covered by a standard-range defender.

## 6. Current vertical-slice plants

### Pędostrzał
Role: baseline sustained DPS.

Current prototype target:
- cost 100
- damage 22
- cooldown 1.0 s
- range 3.2 RU
- physical projectile
- target priority: First

### Kwiat Esencji
Role: economy.

Current prototype target:
- cost 100
- +20 Essence every 20 s
- theoretical ROI 100 s

### Dębowy Strażnik
Role: control.

Current prototype target:
- cost 100
- range 2.4 RU
- periodic root
- 8 root-type damage
- 1.75 s root
- approximately 5 s cadence
- boss receives reduced control duration

### Mroźny Pąk
Role: control DPS.

Current prototype target:
- cost 150
- damage 18
- cooldown 1.0 s
- range 3.2 RU
- 35% slow for 2.5 s
- boss receives reduced slow effectiveness

## 7. Current enemies

### Chochlik
- HP 170
- speed 0.80 RU/s
- reward 12
- base damage 1

### Wichrowy Skrzat
- HP 110
- speed 1.15 RU/s
- reward 12
- base damage 1

### Ogr Zarośli
- HP 550
- speed 0.45 RU/s
- reward 26
- base damage 2

### Żelazny Goblin
- HP 260
- speed 0.55 RU/s
- reward 22
- 15% physical resistance
- base damage 1

### Król Cierni
Boss:
- HP 2200
- speed 0.30 RU/s
- reward 150
- 20% physical resistance
- base damage 5
- reduced control duration/effect

## 8. Current wave plan

Wave 1:
- 6 Chochlik

Wave 2:
- 4 Chochlik
- 4 Wichrowy Skrzat

Wave 3:
- 5 Chochlik
- 4 Żelazny Goblin

Wave 4:
- 6 Wichrowy Skrzat
- 3 Ogr Zarośli
- 4 Chochlik

Wave 5:
- 4 Żelazny Goblin
- 3 Ogr Zarośli
- 8 Wichrowy Skrzat

Wave 6:
- 4 Żelazny Goblin
- 5 Wichrowy Skrzat
- 2 Ogr Zarośli
- 1 Król Cierni

## 9. Long-term campaign structure

The intended campaign remains 30 authored maps divided into six thematic chapters:

1–5: Zielona Dolina  
Focus: core TD fundamentals.

6–10: Nocny Las  
Focus: specialization, darkness, altered routes, night-biome mechanics.

11–15: Zalane Ruiny  
Focus: water, bridges, islands, constrained placement.

16–20: Mgielne Mokradła  
Focus: information, fog, air, utility.

21–25: Cytadela  
Focus: height, artillery, siege pressure.

26–30: Pęknięty Świat  
Focus: combined systems, multiple entrances, dynamic route pressure.

This structure is a roadmap, not current scope.

## 10. Difficulty

Normal:
- teaches systems,
- allows multiple viable strategies,
- should not require perfect play.

Hard:
- should be harder primarily through wave/map composition and route pressure,
- should not rely only on inflated HP,
- modest stat increases are acceptable after real playtesting.

## 11. Upgrade philosophy

Long-term tower progression should be easy to read on mobile.

Preferred high-level structure:
- base plant
- basic upgrades
- later specialization branch
- limited final abilities/upgrades per branch

The earlier very wide upgrade tree is not part of the current vertical slice.

Do not implement final upgrade architecture until baseline combat is proven in real Unity playtests.

## 12. Economy philosophy

Economy choices must create opportunity cost.

A generator that pays itself back too quickly makes early defense decisions trivial.

Current Kwiat Esencji target:
- cost 100
- +20 / 20 s
- ROI 100 s

This remains subject to Balance v1.1 after actual playtests.

## 13. Armor and damage philosophy

The long-term game may use damage types and resistances to create counters.

Current prototype uses:
- Physical
- Frost
- Root/control

Physical armor should reduce physical damage rather than behave as an unrelated second HP bar.

Full damage-type design is stored in the balance workbook and should not be expanded during MAP-A stabilization.

## 14. Future modes

Planned after campaign fundamentals are stable:

### Endless
Escalating waves and mutators.

### Reverse
Player constructs attack waves while AI builds defenses.

Reverse is intentionally late scope because it requires:
- AI placement,
- AI economy,
- reaction logic,
- offensive-unit pricing,
- anti-cheat/fair information rules.

Do not implement it during MAP-A work.

## 15. Balance source of truth

Detailed numeric balance, benchmark maps, enemy pool, and future roster data are stored in:

`ProjectData/Tower_Defense_Design_Balance_v1_0.xlsx`

That workbook is a design baseline, not proof of runtime balance.

Actual runtime playtesting will produce Balance v1.1.
