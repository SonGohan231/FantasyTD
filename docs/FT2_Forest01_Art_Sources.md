# Forest01 artwork integration — initial batch

Ten recovered previously approved image sources have been **reused by original Git blob SHA** from `recovery/t0012-higgsfield-product-completeness`. They are not newly generated assets and are not equivalent to the ten new concept illustrations produced in the conversation.

| Current FT2 asset | Original blob SHA |
|---|---|
| `Units/ThornShooter.png` | `7a473d861bd2c8e36d1c0a9fe1ce4b8750ca2528` |
| `Units/FrostBud.png` | `add88663217a12f0fcc1a6af1a244aa1df04492c` |
| `Units/EssenceFlower.png` | `4d41b02b2c06197ac62c78674aab2d3261d7f451` |
| `Units/Wanderer.png` | `bff359a02fb3c0478126e36ca334bb2da5ec16bb` |
| `Units/Runner.png` | `7685d3d1bc37bb8edf91e1d08d41e212a99c8aad` |
| `Units/Armored.png` | `f8c59fcce42011e307b34359d363d59393c0debf` |
| `Units/OakGuardian.png` | `b2fc2524ca0e775ea9efa5e3b54210e28cb59926` |
| `Units/ForestBoss.png` | `850cedb185cbb85cfe98889adc4e7750b575da35` |
| `Units/Portal.png` | `fd449c66597b15afb992bdd10574ee2b30b37399` |
| `Units/HomeTree.png` | `8a6e60f6fab5c655cfdbf42ae3ce41a04334a236` |

- Runtime uses ThornShooter/FrostBud/EssenceFlower, Wanderer/Runner/Armored only in the initial forest encounter.
- Other source images are staged for visual continuity and **not** claims of implemented additional character behavior.
- Import is restricted to `Resources/FT2/`, isolated from legacy MAP-A art and settings.
- These are illustrated source sheets: runtime must crop/isolate subjects; no polished rigged animations are claimed.
- The **ten newly generated** character concept sheets are delivered as a separate `FantasyTD_10_Concept_Art.zip` in the conversation and are **not yet present in GitHub**. They need further cropping and approval before replacing the existing source artwork.
- Keep PR draft until scene import, APK and physical Android evidence.

## New generated set — October 21, 2026

Added the **ten user-generated character illustrations** from this conversation,
separately from the ten older, approved MAP1 pictures. Source files:
`Assets/Resources/FT2/NewUnits/{ThornbloomRanger,FrostbudOracle,EssenceAlchemist,MushroomMortar,VineGuardian,SunseedDruid,BrambleGoblin,SlimeSnail,EmberBeetle,HollowRootOgre}.png`.

- Runtime maps `thorn/frost/bloom` to Thornbloom Ranger, Frostbud Oracle and Essence Alchemist.
- Initial `imp/runner/armored` enemies use Bramble Goblin, Ember Beetle and Slime Snail.
- Vine Guardian, Mushroom Mortar and Sunseed Druid have **art only**; no new combat rules or promised units.
- Hollow Root Ogre has an optional boss mapping; this is not a new boss encounter.
- All ten are visible from the **Atlas postaci** button in Forest01.
- The ten `NewUnits/*.png` assets are **transparent 512×512 RGBA sprites** pre-cropped from the original full concept sheets, with no right-side panels. Their detailed original concept sheets remain available in the conversation art pack. Runtime uses them directly, without costly texture readbacks or repeated alpha cutting.
- Existing sources remain available as runtime fallbacks. Legacy MAP-A untouched.
- Recheck Android performance and cutout quality in device screenshots. Static 2D sprites do not constitute rigged animation or new combat behaviors.
