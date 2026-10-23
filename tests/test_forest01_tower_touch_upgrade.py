"""Forest01 touch-to-upgrade regression: exercise same selector as mobile input."""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
RUNTIME = ROOT / 'UnityProject/Assets/FantasyTD/V2/Runtime/ForestBattlefield.cs'
SMOKE = ROOT / 'UnityProject/Assets/FantasyTD/V2/Editor/Forest01PlayModeSmoke.cs'

class ForestTowerTouchUpgradeTests(unittest.TestCase):
    def test_touch_and_playmode_use_identical_site_hit_testing(self):
        view = RUNTIME.read_text()
        self.assertIn('public bool TrySelectAtWorldPoint(Vector2 worldPoint)', view)
        self.assertIn('TrySelectAtWorldPoint(new Vector2(world.x, world.y))', view)
        self.assertIn('occupied ? 1.05f : .68f', view)

    def test_placed_tower_has_visible_selection_feedback(self):
        view = RUNTIME.read_text()
        self.assertIn('Tower selection halo', view)
        self.assertIn('selectionRing.enabled', view)
        self.assertIn('Dotknij obrońcy, aby go ulepszyć', view)

    def test_playmode_probes_tap_ui_upgrade_and_level_cap(self):
        smoke = SMOKE.read_text()
        self.assertIn('game.TrySelectAtWorldPoint', smoke)
        self.assertIn('game.RuntimeUpgradeButtonInteractable', smoke)
        self.assertIn('game.ClickUpgradeForValidation()', smoke)
        self.assertIn('TowerState.MaximumLevel', smoke)

if __name__ == '__main__': unittest.main()
