"""Fast source contracts for the integrated Forest01 gameplay slice."""

import pathlib
import re
import unittest


ROOT = pathlib.Path(__file__).resolve().parents[1]
STARTER = (ROOT / "UnityProject/Assets/FantasyTD/V2/Content/StarterLevel.cs").read_text(encoding="utf-8")
SIM = (ROOT / "UnityProject/Assets/FantasyTD/V2/Core/BattleSimulator.cs").read_text(encoding="utf-8")
VIEW = (ROOT / "UnityProject/Assets/FantasyTD/V2/Runtime/ForestBattlefield.cs").read_text(encoding="utf-8")
BUILD = (ROOT / "UnityProject/Assets/FantasyTD/V2/Editor/Forest01AndroidBuild.cs").read_text(encoding="utf-8")


class Forest01IntegratedGameplayTests(unittest.TestCase):
    def test_level_has_exactly_ten_named_build_sites(self):
        sites = re.findall(r'\{"(B\d\d)", new MapPoint\(', STARTER)
        self.assertEqual(sites, [f"B{i:02d}" for i in range(1, 11)])

    def test_level_has_four_defenders_and_six_waves(self):
        for tower in ("thorn", "frost", "bloom", "oak"):
            self.assertIn(f'{{"{tower}", new TowerDefinition', STARTER)
        self.assertEqual(STARTER.count("new WaveDefinition("), 6)
        self.assertIn('"root-warden"', STARTER)

    def test_upgrade_changes_authoritative_combat(self):
        self.assertIn("public bool TryUpgradeTower(string siteId)", SIM)
        self.assertIn("tower.Level++", SIM)
        self.assertIn("tower.Damage", SIM)
        self.assertIn("tower.RangeCentimeters", SIM)
        self.assertIn("BattleEventKind.TowerUpgraded", SIM)

    def test_touch_view_exposes_real_build_and_upgrade_flow(self):
        self.assertIn("public bool SelectSite(string siteId)", VIEW)
        self.assertIn("public bool TryBuildSelected(string towerId)", VIEW)
        self.assertIn("public bool TryUpgradeSelected()", VIEW)
        self.assertIn("public bool TryStartNextWave()", VIEW)
        self.assertIn('"Dębowy Strażnik · 105"', VIEW)

    def test_android_build_enforces_release_shape(self):
        self.assertIn("ScriptingImplementation.IL2CPP", BUILD)
        self.assertIn("AndroidArchitecture.ARM64", BUILD)
        self.assertIn('"com.indie.fantasytd"', BUILD)
        self.assertIn("AndroidApiLevel23", BUILD)


if __name__ == "__main__":
    unittest.main()
