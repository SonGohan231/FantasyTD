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
- [x] AMBIENT_LOOP — run **37021780748**, artifact **11233877408** — APPROVED after visual QA (lantern/firefly/petal/mist loop overlay)
- [ ] SECRET_HINT
- [ ] SECRET_OPEN
- [!] MINIBOSS_DORMANT_SINGLE_SOURCE attempt 1 — run **37021346938**, artifact **11234095282** — FAILED generation
- [!] MINIBOSS_DORMANT_SINGLE_SOURCE attempt 2 — run **37022703183**, artifact **11233853514** — REJECTED (Ideogram preserved full source sheet instead of isolating one pose)
- [ ] MINIBOSS_DORMANT_SINGLE_SOURCE
- [!] MINIBOSS_ACTIVE_SINGLE_SOURCE attempt 1 — run **37021351122**, artifact **11233745986** — REJECTED after visual QA (identity drift from approved source sheet)
- [!] MINIBOSS_ACTIVE_SINGLE_SOURCE attempt 2 — run **37022707850**, artifact **11233838292** — REJECTED (Ideogram preserved full source sheet instead of isolating one pose)
- [!] MINIBOSS_ACTIVE_SINGLE_SOURCE attempt 2 — run **37025032911**, artifact **11234986551** — REJECTED after visual QA (3D/style and identity drift)
- [ ] MINIBOSS_ACTIVE_SINGLE_SOURCE — use awakening final state as reference if static isolation keeps drifting
- [x] MINIBOSS_DORMANT_SINGLE_SOURCE — run **37023874042**, artifact **11234650367** — APPROVED after visual QA
- [!] MINIBOSS_ACTIVE_SINGLE_SOURCE attempt 1 — run **37023879914**, artifact **11233164725** — REJECTED (identity drift vs approved source/dormant design)
- [ ] MINIBOSS_ACTIVE_SINGLE_SOURCE
- [x] MINIBOSS_DORMANT — run **37023874042**, artifact **11234650367** — APPROVED
- [ ] MINIBOSS_ACTIVE

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
- [!] RADIO_RELAY_ACTIVATION_LOOP attempt — run **37009129944**, artifact **11226993880** — REJECTED (source sheet caused state-jumping between different props); dedicated single relay source required
- [x] RADIO_RELAY_ACTIVATION_LOOP — run **37010301111**, artifact **11227881216** — APPROVED after visual QA (single-prop source preserved; blink→core pulse→dish scan→signal pulse)
- [x] MAINTENANCE_TUNNEL_OPEN_ANIM — run **37009133694**, artifact **11227182741** — APPROVED after visual QA (hint glow→panel open→stable tunnel)
- [x] MINIBOSS_SLAM_ATTACK_ANIM — run **37009138087**, artifact **11226964130** — APPROVED after visual QA (wind-up→slam→impact/recovery readable)
- [x] MINIBOSS_HIT_DEFEAT_ANIM — run **37009142068**, artifact **11226909606** — APPROVED after visual QA (hit→stagger→collapse→glow fade)
- [x] TERRAIN_ROAD_TILE_KIT — run **37009161826**, artifact **11227420838** — APPROVED after visual QA (modular asphalt/curves/Y-junctions/sidewalk/water/props)
- [x] RADIO_RELAY_SINGLE_SOURCE — run **37010048084**, artifact **11226614659** — APPROVED after visual QA
- [x] BACKGROUND_TERRAIN_PLATE — run **37010052305**, artifact **11227178308** — APPROVED after visual QA (road-free environment plate for deterministic Unity composition)
- [x] SPECIAL_SALVAGE_TOWER_SOURCE — run **37010305311**, artifact **11227492469** — APPROVED after visual QA
- [x] SPECIAL_SALVAGE_TOWER_ACTIVATION_ANIM — run **37010704739**, artifact **11228146221** — APPROVED after visual QA (deploy→core light→vine/flower unfold→recycling pulse)
- [!] SPECIAL_SALVAGE_TOWER_IDLE_LOOP attempt 1 — run **37010710427**, artifact **11227567439** — FAILED: Higgsfield API reported credit balance too low
- [!] SPECIAL_SALVAGE_TOWER_IDLE_LOOP attempt 2 — run **37011244408**, artifact **11227981779** — REJECTED after visual QA (multi-pose source sheet remained visible; unsuitable as gameplay idle)
- [x] SPECIAL_SALVAGE_TOWER_ACTIVE_SINGLE_SOURCE — run **37011702634**, artifact **11227554403** — APPROVED after visual QA
- [!] SPECIAL_SALVAGE_TOWER_IDLE_LOOP attempt 3 — run **37011886432**, artifact **11227953398** — BLOCKED: Higgsfield API returned `credit balance is too low`
- [ ] SPECIAL_SALVAGE_TOWER_IDLE_LOOP — waiting for API top-up

## MAP 3 — Zmierzch
**Difficulty:** Medium- / two persistent routes  
**Topology:** 1 shared entry → early split → 2 persistent routes → late merge → 1 exit  
**Target build spots:** 12  
**Ambient:** lanterns, fireflies, moonlit water, opening night flowers, moving shadows  
**Event:** bat swarm + shadow fog reduces visibility in moving map zones  
**Secret/miniboss:** moon gate / Night Gardener  
**Unlock:** Starbloom Lantern night-event tower

Previous API concept:
- [!] Old static concept — run **36999547601** — REJECTED as final layout

Production:
- [!] TOPOLOGY_SKETCH attempt 1 — run **37020773970**, artifact **11233275363** — REJECTED (two separate entries/exits; no true shared entry→split→late merge)
- [x] TOPOLOGY_SPEC_LOCKED — `.higgsfield/MAP3_TWILIGHT_TOPOLOGY.json`, commit **106607d8** (12 spots, persistent split)
- [x] SCENERY_PROP_PACK — run **37020777949**, artifact **11232579406** — APPROVED
- [!] BACKGROUND_TERRAIN_PLATE attempt 1 — run **37020783707**, artifact **11232965586** — REJECTED (road-like paths inserted)
- [x] BACKGROUND_TERRAIN_PLATE — run **37021337264**, artifact **11233790855** — APPROVED
- [ ] BASE_COMPOSITE_IN_UNITY_FROM_LOCKED_TOPOLOGY
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [x] AMBIENT_LOOP — run **37021780748**, artifact **11233877408** — APPROVED
- [x] FOG_START — run **37021787142**, artifact **11233812434** — APPROVED
- [x] FOG_HIGH — run **37021787142**, artifact **11233812434** — APPROVED
- [x] FOG_END — run **37021787142**, artifact **11233812434** — APPROVED
- [x] BAT_SWARM_ANIM — run **37021790530**, artifact **11233777438** — APPROVED
- [x] MOON_GATE_SINGLE_SOURCE — run **37021341072**, artifact **11233805827** — APPROVED
- [x] MOON_GATE_CLOSED — run **37021341072**, artifact **11233805827** — APPROVED
- [x] MOON_GATE_OPEN — final state from run **37021796734**, artifact **11233697507** — APPROVED
- [x] MOON_GATE_OPEN_ANIM — run **37021796734**, artifact **11233697507** — APPROVED
- [x] MINIBOSS_SOURCE — run **37020787886**, artifact **11232839080** — APPROVED
- [!] MINIBOSS_DORMANT_SINGLE_SOURCE attempt 1 — run **37021346938**, artifact **11234095282** — FAILED generation
- [!] MINIBOSS_DORMANT_SINGLE_SOURCE attempt 2 — run **37022703183**, artifact **11233853514** — REJECTED (full sheet preserved)
- [ ] MINIBOSS_DORMANT_SINGLE_SOURCE
- [!] MINIBOSS_ACTIVE_SINGLE_SOURCE attempt 1 — run **37021351122**, artifact **11233745986** — REJECTED (identity drift)
- [!] MINIBOSS_ACTIVE_SINGLE_SOURCE attempt 2 — run **37022707850**, artifact **11233838292** — REJECTED (full sheet preserved)
- [ ] MINIBOSS_ACTIVE_SINGLE_SOURCE
- [ ] MINIBOSS_DORMANT
- [x] MINIBOSS_ACTIVE — final active state from awaken run **37025524146**, artifact **11234909637** — APPROVED after visual QA
- [x] MINIBOSS_AWAKEN_ANIM — run **37025524146**, artifact **11234909637** — APPROVED after visual QA
- [x] MINIBOSS_ATTACK_ANIM — run **37025528882**, artifact **11235167715** — APPROVED after visual QA
- [x] SPECIAL_NIGHT_TOWER_SOURCE — run **37020792268**, artifact **11233385289** — APPROVED
- [x] SPECIAL_NIGHT_TOWER_ACTIVE_SINGLE_SOURCE — run **37021355425**, artifact **11233476155** — APPROVED
- [x] SPECIAL_NIGHT_TOWER_ACTIVATION_ANIM — run **37021801076**, artifact **11233497480** — APPROVED

## MAP 4 — Dno oceaniczne
**Difficulty:** Medium / split→shared chokepoint→re-split  
**Topology:** 1 entry → 2 routes → shared mid chokepoint → 2 routes → late merge → 1 exit  
**Target build spots:** 13 (compensation for projectile denial)  
**Ambient:** kelp, fish, particles, coral, hydrothermal vents  
**Event:** hot bubbles emerge, drift across lanes, absorb projectiles, then burst  
**Secret/miniboss:** ancient pearl shrine / Vent Crab  
**Unlock:** Pearl Lily / Tidebloom bubble-piercing tower

Previous API concept:
- [!] Old static concept — run **36999551504** — REJECTED as final layout

Production:
- [x] TOPOLOGY_SPEC_LOCKED — `.higgsfield/MAP4_ABYSSAL_TOPOLOGY.json`, commit **10d03dc3** (13 spots)
- [!] SCENERY_PROP_PACK attempt 1 — run **37023187009**, artifact **11233929092** — REJECTED (numeric labels)
- [x] SCENERY_PROP_PACK — run **37023868008**, artifact **11234470479** — APPROVED after visual QA (clean underwater prop sheet, no numeric labels)
- [x] BACKGROUND_TERRAIN_PLATE — run **37023191880**, artifact **11232874052** — APPROVED
- [ ] BASE_COMPOSITE_IN_UNITY_FROM_LOCKED_TOPOLOGY
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [x] AMBIENT_LOOP — run **37023891512**, artifact **11235070159** — APPROVED as compositing overlay (kelp/bubbles/particles, locked camera)
- [x] BUBBLE_FORM — run **37023897016**, artifact **11234044723** — APPROVED
- [x] BUBBLE_DRIFT — run **37023897016**, artifact **11234044723** — APPROVED
- [x] BUBBLE_ABSORB — run **37023897016**, artifact **11234044723** — APPROVED (impact/spark cue before burst)
- [x] BUBBLE_BURST — run **37023897016**, artifact **11234044723** — APPROVED
- [x] BUBBLE_EVENT_ANIM — run **37023897016**, artifact **11234044723** — APPROVED after visual QA
- [x] PEARL_SHRINE_SINGLE_SOURCE — run **37023197057**, artifact **11233644218** — APPROVED
- [x] PEARL_SHRINE_CLOSED — run **37023197057**, artifact **11233644218** — APPROVED
- [x] PEARL_SHRINE_OPEN — final state from run **37023903350**, artifact **11234391273** — APPROVED
- [x] PEARL_SHRINE_OPEN_ANIM — run **37023903350**, artifact **11234391273** — APPROVED after visual QA (shell opens, pearl rises, ring pulse)
- [x] MINIBOSS_SOURCE — run **37023204024**, artifact **11233674566** — APPROVED
- [x] MINIBOSS_DORMANT_SINGLE_SOURCE — run **37026731003**, artifact **11234883925** — APPROVED after visual QA
- [x] MINIBOSS_ACTIVE_SINGLE_SOURCE — run **37026734821**, artifact **11235118803** — APPROVED after visual QA
- [x] MINIBOSS_DORMANT — run **37026731003**, artifact **11234883925** — APPROVED
- [x] MINIBOSS_ACTIVE — run **37026734821**, artifact **11235118803** — APPROVED
- [ ] MINIBOSS_AWAKEN_ANIM
- [ ] MINIBOSS_ATTACK_ANIM
- [x] SPECIAL_AQUATIC_TOWER_SOURCE — run **37023208180**, artifact **11233419385** — APPROVED
- [!] SPECIAL_AQUATIC_TOWER_SINGLE_SOURCE attempt 1 — run **37025038914**, artifact **11234227742** — REJECTED after visual QA (identity drift from approved tower)
- [x] SPECIAL_AQUATIC_TOWER_SINGLE_SOURCE — final folded state from run **37025533799**, artifact **11235067913** — APPROVED after visual QA
- [x] SPECIAL_AQUATIC_TOWER_ACTIVE_SINGLE_SOURCE — run **37023885079**, artifact **11233999401** — APPROVED after visual QA
- [x] SPECIAL_AQUATIC_TOWER_ACTIVATION_ANIM — use reverse playback of approved fold animation run **37025533799**, artifact **11235067913**
- [x] SPECIAL_AQUATIC_TOWER_IDLE_LOOP — run **37025045929**, artifact **11235446525** — APPROVED after visual QA (subtle bubbles/tendril sway/core bob; loop-friendly)

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
- [x] TOPOLOGY_SPEC_LOCKED — `.higgsfield/MAP5_FUNGAL_TOPOLOGY.json`, commit **8e66fd09** (3 persistent branches, 12 spots)
- [x] SCENERY_PROP_PACK — run **37025266321**, artifact **11234731952** — APPROVED after visual QA
- [x] BACKGROUND_TERRAIN_PLATE — run **37025270764**, artifact **11234691977** — APPROVED after visual QA (road-free open corridors)
- [ ] BASE_COMPOSITE_IN_UNITY_FROM_LOCKED_TOPOLOGY
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [!] AMBIENT_LOOP attempt 1 — run **37025274825**, artifact **11234323555** — REJECTED after visual QA (too much baked terrain; unsuitable as compositing overlay)
- [x] AMBIENT_LOOP — run **37026222266**, artifact **11235173326** — APPROVED after visual QA
- [x] PSY_V0_NORMAL — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSY_V1_LIGHT_WARP — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSY_V2_GRID_VISIBLE — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSY_V3_STRONG — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSY_V4_PEAK — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSY_V5_FADE — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSY_V6_NORMAL — run **37025281280**, artifact **11235336865** — APPROVED
- [x] PSYCHEDELIC_SURGE_ANIM — run **37025281280**, artifact **11235336865** — APPROVED after visual QA
- [x] LAB_SINGLE_SOURCE — run **37025286566**, artifact **11235466241** — APPROVED after visual QA
- [x] LAB_CLOSED — run **37025286566**, artifact **11235466241** — APPROVED
- [x] LAB_OPEN — final state from run **37025981397**, artifact **11234759621** — APPROVED
- [x] LAB_OPEN_ANIM — run **37025981397**, artifact **11234759621** — APPROVED after visual QA
- [!] MINIBOSS_SOURCE attempt 1 — run **37025292120**, artifact **11235206778** — REJECTED after visual QA (too 3D/toy-like vs 2D casual-game style)
- [x] MINIBOSS_SOURCE — run **37025985108**, artifact **11234892986** — APPROVED after visual QA (clean 2D Mother Mycelium sheet)
- [ ] MINIBOSS_DORMANT_SINGLE_SOURCE
- [ ] MINIBOSS_ACTIVE_SINGLE_SOURCE
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE
- [ ] MINIBOSS_AWAKEN_ANIM
- [ ] MINIBOSS_ATTACK_ANIM
- [!] SPECIAL_MUTATION_TOWER_SOURCE attempt 1 — run **37025297595**, artifact **11234752032** — FAILED generation
- [x] SPECIAL_MUTATION_TOWER_SOURCE — run **37025989780**, artifact **11235363386** — APPROVED after visual QA
- [ ] SPECIAL_MUTATION_TOWER_ACTIVE_SINGLE_SOURCE
- [ ] SPECIAL_MUTATION_TOWER_ACTIVATION_ANIM

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
- [x] TOPOLOGY_SPEC_LOCKED — `.higgsfield/MAP6_BAMBOO_TOPOLOGY.json`, commit **2ac36d24** (3 entrances, 2 merge points, 13 spots)
- [x] SCENERY_PROP_PACK — run **37026212309**, artifact **11235063190** — APPROVED after visual QA
- [x] BACKGROUND_TERRAIN_PLATE — run **37026217305**, artifact **11235542085** — APPROVED after visual QA
- [ ] BASE_COMPOSITE_IN_UNITY_FROM_LOCKED_TOPOLOGY
- [ ] BASE_FINAL_NEW_TOPOLOGY
- [ ] AMBIENT_LOOP
- [x] PANDA_SOURCE — run **37026226845**, artifact **11234419605** — APPROVED after visual QA
- [x] PANDA_ROLL_SOURCE — run **37026714289**, artifact **11235810368** — APPROVED after visual QA
- [ ] PANDA_ROLL
- [ ] PANDA_STOP
- [x] PANDA_HOSTILE_SOURCE — run **37026718211**, artifact **11235099906** — APPROVED after visual QA
- [ ] PANDA_TRANSFORM
- [x] BAMBOO_LOG_HAZARD — run **37026248648**, artifact **11235547736** — APPROVED after visual QA
- [x] DOJO_SINGLE_SOURCE — run **37026232465**, artifact **11235078130** — APPROVED after visual QA
- [x] DOJO_CLOSED — run **37026232465**, artifact **11235078130** — APPROVED
- [x] DOJO_OPEN — final state from run **37026724501**, artifact **11235710967** — APPROVED
- [x] DOJO_OPEN_ANIM — run **37026724501**, artifact **11235710967** — APPROVED after visual QA
- [x] MINIBOSS_SOURCE — run **37026237201**, artifact **11235542099** — APPROVED after visual QA
- [ ] MINIBOSS_DORMANT_SINGLE_SOURCE
- [ ] MINIBOSS_ACTIVE_SINGLE_SOURCE
- [ ] MINIBOSS_DORMANT
- [ ] MINIBOSS_ACTIVE
- [ ] MINIBOSS_AWAKEN_ANIM
- [ ] MINIBOSS_ATTACK_ANIM
- [x] SPECIAL_BAMBOO_TOWER_SOURCE — run **37026242264**, artifact **11235162980** — APPROVED after visual QA
- [x] SPECIAL_BAMBOO_TOWER_ACTIVE_SINGLE_SOURCE — run **37026740118**, artifact **11235615614** — APPROVED after visual QA
- [ ] SPECIAL_BAMBOO_TOWER_ACTIVATION_ANIM

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
