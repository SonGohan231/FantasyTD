# Product completeness release gates

A green compiler, PlayMode suite or APK BuildReport is not a product-completeness pass.

## Mandatory evidence

1. MAIN MENU with working GRAJ.
2. CAMPAIGN MAP SELECT with MAP1 playable and MAP2–MAP8 visibly locked/in production.
3. MAP-A entered from campaign flow and real Higgsfield map visible.
4. Enemy HP bar after damage.
5. Occupied tower opens level/stats panel with ULEPSZ and SPRZEDAJ.
6. Victory/defeat end state.
7. Existing geometry/plant/wave/boss/layout regressions remain green.
8. Visual acceptance uploads these screenshots:
   - 01-main-menu.png
   - 02-campaign-map-select.png
   - 03-map-a-higgsfield-gameplay.png
   - 04-enemy-hp-bar.png
   - 05-tower-panel-upgrade-sell.png
   - 06-victory-end-state.png
9. Android build contains scenes in order: MAIN_MENU, CAMPAIGN_MAP_SELECT, MAP_A_ANDROID_V2.
10. Before DONE, install/launch APK on emulator or physical Android and capture runtime evidence for cold start → menu → campaign → MAP-A → tower → wave → HP bar → return to campaign.

Until Android runtime evidence exists the release status is CANDIDATE, never DONE.
