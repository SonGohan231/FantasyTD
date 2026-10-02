# Prompt Context Pack — Fantasy TD

Use this only when an agent needs concise project context.

## Project

Fantasy TD is a Unity 6 LTS Android landscape 2D tower-defense game.

Current milestone:
**MAP-A Android v2 vertical slice**

## Product direction

Gameplay:
- authored winding path
- fixed planting spots
- waves
- strategic tower composition
- mobile touch controls

Art:
- colorful organic fantasy
- living plants
- humorous fantasy enemies
- roots, leaves, flowers, frost, pollen, natural magic
- no sci-fi
- no futuristic machines
- no technological super-weapons

## Current scope

MAP-A:
- 24 × 14 RU
- ~46.211 RU route
- 12 fixed planting spots
- 300 starting Essence
- 20 base HP
- 6 waves
- one boss

Plants:
- Pędostrzał
- Kwiat Esencji
- Dębowy Strażnik
- Mroźny Pąk

Enemies:
- Chochlik
- Wichrowy Skrzat
- Ogr Zarośli
- Żelazny Goblin
- Król Cierni

## Out of scope now

- MAP-B/MAP-C implementation
- 30-map campaign implementation
- full roster
- full upgrade trees
- Endless
- Reverse
- heroes
- final art production
- broad refactors

## Workflow

- ChatGPT: PM/design
- Astra: review/decomposition/debug/QA advisor
- Codex: repo implementation
- Unity/Android tooling: validation source of truth
- user: product approval

One active ticket at a time.

Read:
1. AGENTS.md
2. Repo_Current_State
3. active ticket
4. only relevant technical/design docs

## Current truth level

Source code exists for the MAP-A prototype, but at handoff time:
- Unity compilation is not yet verified,
- Play Mode is not yet verified,
- Android APK build is not yet verified,
- device smoke test is not yet verified.

T0001 exists specifically to establish that baseline.
