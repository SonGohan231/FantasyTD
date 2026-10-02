# FantasyTD — Visual Production Checklist (Higgsfield API)

> Source of truth for generated visual assets.  
> **Only assets with a GitHub Actions run ID from `Higgsfield API Asset Generator` count as Higgsfield API output.**  
> ChatGPT Images/DALL-E concepts do not count toward this checklist and are never marked READY.
>
> **Topology-locked composition rule:** final gameplay routes/build pads are deterministic Unity/vector layers. Higgsfield generates scenery, props, ambient loops, event overlays, secrets and miniboss visuals around that locked topology. A beautiful AI image is never allowed to change the route graph.

## Status legend
- [ ] PLANNED — not generated yet
- [~] GENERATED — API artifact exists, needs visual QA
- [x] APPROVED — passed visual QA and can enter Unity staging
- [!] REJECTED — generated but not suitable for production
- [>] IN PROGRESS — API run active/queued

## Global acceptance gates for every map pack
- [ ] Distinct topology from previous maps; difficulty rises through route complexity, not only enemy stats.
- [ ] Entire battlefield readable on a landscape phone without camera scrolling.
- [ ] Build-spot count is a **design target only** in artwork; exact count/coordinates are enforced later by Unity.
- [ ] No single placement dominates every route.
- [ ] No unavoidable-loss hazard.
- [ ] Secret/miniboss content is optional or clearly telegraphed.
- [ ] Special tower/upgrade unlock is a reward, not mandatory to clear base map.
- [ ] BASE artwork exists.
- [ ] AMBIENT animation exists.
- [ ] EVENT_START animation exists.
- [ ] EVENT_HIGH animation exists.
- [ ] EVENT_END animation exists.
- [ ] SECRET_CLOSED state exists.
- [ ] SECRET_OPEN state exists.
- [ ] MINIBOSS_DORMANT state exists.
- [ ] MINIBOSS_ACTIVE state exists.
- [ ] All Higgsfield API run IDs recorded.
- [ ] Final pack visually reviewed before Unity integration.

---

# Campaign map roster

## MAP 1 — Zielony Trakt
**Difficulty:** Intro / 1 route  
**Topology:** one route, simple turns  
**Target build spots:** 12  
**Ambient:** grass/flowers sway, water, butterflies, falling leaves, mushroom glow  
**Event:** small forest creature hints at a secret  
**Secret/miniboss:** hidden druid / forest guardian  
**Unlock:** first special tower variant

Existing API evidence:
- [~] MAP-A style-lock environment — run **36991331980** — concept only, not full animation pack

Production:
- [ ] BASE_FINAL
- [x] AMBIENT_LOOP — run **37001697251**, artifact **11224247023** — APPROVED (steam, debris/leaves, beacon, smoke; loop-friendly overlay)
- [x] SECRET_HINT — run **37007483328**, artifact **11226805427** — APPROVED (maintenance tunnel states)
- [ ] SECRET_OPEN
- [~] MINIBOSS_DORMANT — source sheet run **37007487900**, artifact **11226875660** — APPROVED as design/source; dedicated animation source still required
- [x] MINIBOSS_ACTIVE — awakening animation run **37007997174**, artifact **11226746833** — APPROVED after visual QA (identity preserved; dormant→combat-idle transition readable)

## MAP 2 — Postapo
**Difficulty:** Easy+ / first split  
**Topology:** single entry → 2 routes for ~30–35% of path → merge → single exit  
**Target build spots:** 12  
**Ambient:** flickering hazard lights, antenna rotation, sewer water, smoke, vines  
**Event:** toxic dust storm crosses one section; visibility and local hazard pressure rise temporarily  
**Secret/miniboss:** locked bunker / underground mutant  
**Unlock:** salvage-energy tower / scrap upgrade

Previous API concept:
- [!] Old static concept — run **36999324448** — REJECTED as final layout (topology too similar to prior maps)

Production:
- [!] BASE attempt run **37000229167** — REJECTED (style too realistic, pseudo-text/signage, no clear Y split)
- [!] BASE attempt run **37000430159** — REJECTED (still no proper Y split, too much pseudo-signage / wrong style)
- [~] TOPOLOGY_SKETCH — run **37000812091** — PASS as route reference (clear Y split + merge, 12 pads)
- [!] Soul I2I render — run **37000967282** — REJECTED (did not preserve topology closely enough)
- [!] Ideogram weight-95 render — run **37001202601** — REJECTED as final art (preserved topology but barely stylized)
- [!] Ideogram weight-60 render — run **37001433593** — REJECTED as final art (still mostly schematic; geometry stays deterministic)
- [x] SCENERY_PROP_PACK — run **37001692492**, artifact **11223803916** — APPROVED after visual QA
- [ ] BASE_COMPOSITE_IN_UNITY_FROM_LOCKED_TOPOLOGY
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [x] AMBIENT_LOOP — run **37001697251**, artifact **11224247023** — APPROVED after visual QA
- [x] TOXIC_START — run **37001700869**, artifact **11223664631** — APPROVED after visual QA
- [x] TOXIC_HIGH — run **37001700869**, artifact **11223664631** — APPROVED after visual QA
- [x] TOXIC_END — run **37001700869**, artifact **11223664631** — APPROVED after visual QA
- [!] BUNKER state sheet — run **37007479958**, artifact **11226826000** — REJECTED for final use (unwanted text labels); usable only as design reference
- [x] BUNKER_CLOSED — run **37007753837**, artifact **11226696907** — APPROVED after visual QA
- [x] BUNKER_OPEN — run **37007991254**, artifact **11226332484** — APPROVED after visual QA (closed→open transition, steam, lit tunnel, stable final open state)
- [x] SECRET_HINT — run **37007483328**, artifact **11226805427** — APPROVED after visual QA (maintenance tunnel hidden→hint→open states)
- [x] RADIO_RELAY_NODE — run **37007483328**, artifact **11226805427** — APPROVED after visual QA (dormant→hint→active relay states)
- [x] MINIBOSS_DORMANT — run **37007759601**, artifact **11226217050** — APPROVED after visual QA
- [x] MINIBOSS_ACTIVE — run **37007997174**, artifact **11226746833** — APPROVED after visual QA (dormant→active combat-idle awakening)

### MAP2 ANIMATION EXTRAS
- [ ] RADIO_RELAY_ACTIVATION_LOOP
- [ ] MAINTENANCE_TUNNEL_OPEN_ANIM
- [ ] MINIBOSS_SLAM_ATTACK_ANIM
- [ ] MINIBOSS_HIT_DEFEAT_ANIM
- [ ] TERRAIN_ROAD_TILE_KIT

## MAP 3 — Zmierzch
**Difficulty:** Medium- / two persistent routes  
**Topology:** 2 routes split enemy traffic and reconnect late  
**Target build spots:** 12  
**Ambient:** lanterns, fireflies, moonlit water, opening night flowers, moving shadows  
**Event:** bat swarm + shadow fog reduces visibility in moving map zones  
**Secret/miniboss:** moon gate / night gardener or living shadow  
**Unlock:** night-event tower

Previous API concept:
- [!] Old static concept — run **36999547601** — REJECTED as final layout

Production:
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [ ] FOG_START
- [ ] FOG_HIGH
- [ ] FOG_END
- [ ] MOON_GATE_CLOSED
- [ ] MOON_GATE_OPEN
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE

## MAP 4 — Dno oceaniczne
**Difficulty:** Medium / two routes + shared crossing  
**Topology:** 2 parallel routes with one shared crossover/chokepoint  
**Target build spots:** 13 (compensation for projectile denial)  
**Ambient:** kelp, fish, particles, coral, hydrothermal vents  
**Event:** hot bubbles emerge, drift across lanes, absorb projectiles, then burst  
**Secret/miniboss:** ancient pearl / giant crab cave  
**Unlock:** aquatic tower or bubble-piercing attack

Previous API concept:
- [!] Old static concept — run **36999551504** — REJECTED as final layout

Production:
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [ ] BUBBLE_FORM
- [ ] BUBBLE_DRIFT
- [ ] BUBBLE_ABSORB
- [ ] BUBBLE_BURST
- [ ] PEARL_SHRINE_CLOSED
- [ ] PEARL_SHRINE_OPEN
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE

## MAP 5 — Grzybowe Zainfekowane Miasto
**Difficulty:** Medium+ / route network  
**Topology:** 2–3 branches; different enemy groups may choose different branches  
**Target build spots:** 12  
**Ambient:** pulsing mycelium, spores, mushroom growth, fluorescent runoff  
**Event:** psychedelic surge blocks placement/upgrades but existing towers keep firing  
**Secret/miniboss:** underground lab / Mother Mycelium  
**Unlock:** mutation tower or status-resistance upgrade

Previous API concept:
- [!] Old static concept — run **36999555620** — REJECTED as final layout

Production:
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [ ] PSY_V0_NORMAL
- [ ] PSY_V1_LIGHT_WARP
- [ ] PSY_V2_GRID_VISIBLE
- [ ] PSY_V3_STRONG
- [ ] PSY_V4_PEAK
- [ ] PSY_V5_FADE
- [ ] PSY_V6_NORMAL
- [ ] LAB_CLOSED
- [ ] LAB_OPEN
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE

## MAP 6 — Las Bambusowy
**Difficulty:** Hard- / three approach lanes  
**Topology:** 3 entrances/branches with two merge points  
**Target build spots:** 13 (compensation for surprise enemy pressure)  
**Ambient:** swaying bamboo, falling leaves, koi, waterwheel, lanterns  
**Event:** pandas roll across map; some stop and transform into enemies; bamboo logs roll across marked lanes  
**Secret/miniboss:** hidden dojo / Great Panda Warrior  
**Unlock:** rapid-fire bamboo tower

Previous API concept:
- [!] Old static concept — run **36999559587** — REJECTED as final layout

Production:
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [ ] PANDA_ROLL
- [ ] PANDA_STOP
- [ ] PANDA_TRANSFORM
- [ ] BAMBOO_LOG_HAZARD
- [ ] DOJO_CLOSED
- [ ] DOJO_OPEN
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE

## MAP 7 — Świątynia Słońca i Księżyca
**Difficulty:** Hard / three routes + route mechanisms  
**Topology:** 3 routes; mechanisms open/close side transitions  
**Target build spots:** 12  
**Ambient:** waterfalls, flame bowls, rotating disks, moving statues, light shafts  
**Event:** mirrors rotate → beam sweeps → one gate closes → another opens → route changes  
**Secret/miniboss:** guardian altar / awakened temple guardian  
**Unlock:** light/support tower

Previous API concept:
- [!] Old static concept — run **36999563641** — REJECTED as final layout

Production:
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [ ] MIRROR_ROTATE
- [ ] BEAM_SWEEP
- [ ] GATE_A_CLOSE
- [ ] GATE_B_OPEN
- [ ] ROUTE_STATE_A
- [ ] ROUTE_STATE_B
- [ ] SANCTUM_CLOSED
- [ ] SANCTUM_OPEN
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE

## MAP 8 — Lochy Smoka
**Difficulty:** Hard+ / dynamic route network  
**Topology:** 3–4 routes, multiple gates/bridges, two route-change events  
**Target build spots:** 13  
**Ambient:** lava, embers, chains, mine carts, crystals, treasure light  
**Event:** dragon wakes and changes passages exactly 2× during the level  
**Secret/miniboss:** dragon vault / young dragon or treasure guardian  
**Unlock:** dragon-fire tower or permanent economy upgrade

Previous API concept:
- [!] Old static concept — run **36999345781** — REJECTED as final layout

Production:
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [ ] DRAGON_WAKE
- [ ] DRAGON_FLY
- [ ] ROUTE_CHANGE_1
- [ ] ROUTE_STATE_B
- [ ] DRAGON_FLY_2
- [ ] ROUTE_CHANGE_2
- [ ] ROUTE_STATE_C
- [ ] VAULT_CLOSED
- [ ] VAULT_OPEN
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE

---

# Character/source assets already generated through Higgsfield API
- [~] Master style board — run **36990806385**
- [~] Pędostrzał source sheet — run **36991021820**
- [~] Chochlik source sheet — run **36991173273**

> Sprite-sheet animations created outside the GitHub→Higgsfield API bridge are intentionally not counted as API-complete here.
