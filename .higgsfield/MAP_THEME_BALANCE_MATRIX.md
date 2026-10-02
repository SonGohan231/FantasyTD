# FantasyTD — themed map visual + balance constraints

This file is for visual-production concepts only. Final pad coordinates and difficulty are validated in Unity/playtests, not trusted from generated artwork.

## Placement policy
MAP-A baseline = 12 planting spots. The themed maps use a small compensation band around that baseline.

| Theme | Target build spots | Main dynamic hazard | Balance reason |
|---|---:|---|---|
| Post-apocalyptic city | 12 | toxic dust / temporary hazard zones | baseline difficulty |
| Twilight gardens | 12 | bat/fog dimming windows | mostly visibility pressure |
| Abyssal sea floor | 13 | hot bubbles can absorb projectiles | +1 compensation for projectile denial |
| Fungal infected city | 12 | psychedelic spore phase blocks upgrades temporarily | firing continues, so baseline |
| Bamboo forest | 13 | rolling pandas can turn into enemies | +1 compensation for surprise enemy pressure |
| Ancient temple | 12 | rotating sun/moon mechanisms alter safe lanes | baseline, mechanic is positional |
| Treasure dungeon | 13 | dragon changes passages twice per level | +1 compensation for route uncertainty |

## Final Unity acceptance rules
- Do not use image-generation counting as authority; Unity scene data owns the exact count.
- No single pad should dominate the entire route.
- Avoid a single unbeatable choke with excessive overlapping range.
- Every major path section must have at least one viable defensive response.
- Dynamic hazards must not create an unavoidable loss state.
- Hidden/miniboss content must be optional or clearly telegraphed.
- Special-tower unlocks are rewards, not required to finish the base map.
- Exact pad count/coordinates must be playtested and may be tuned by ±1 before lock.
