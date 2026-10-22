"""Static 28x20 Forest01 geometry contract; runs on GitHub-hosted Linux without Unity."""
import pathlib
import re
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[1]
SOURCE = (ROOT / "UnityProject/Assets/FantasyTD/V2/Terrain/Forest01TerrainLayout.cs").read_text(encoding="utf-8")
GENERATOR = (ROOT / "UnityProject/Assets/FantasyTD/V2/Editor/Forest01TerrainGenerator.cs").read_text(encoding="utf-8")

def cells(name):
    match = re.search(r"public static readonly Cell\[\] " + re.escape(name)
                      + r"\s*=\s*\{(.*?)\};", SOURCE, re.S)
    if not match:
        raise AssertionError("Missing array " + name)
    found = [(int(x), int(y)) for x, y in re.findall(
        r"new Cell\((\d+),\s*(\d+)\)", match.group(1))]
    if not found:
        raise AssertionError("Empty array " + name)
    return found

def road():
    checkpoints = cells("RoadCheckpoints")
    result = [checkpoints[0]]
    for (ax, ay), (bx, by) in zip(checkpoints, checkpoints[1:]):
        if ax != bx and ay != by:
            raise AssertionError("Diagonal checkpoint segment")
        if ax == bx and ay == by:
            raise AssertionError("Zero checkpoint segment")
        dx = (bx > ax) - (bx < ax)
        dy = (by > ay) - (by < ay)
        while (ax, ay) != (bx, by):
            ax += dx
            ay += dy
            result.append((ax, ay))
    return result

class Forest01TerrainLayoutTests(unittest.TestCase):
    def test_isometric_28_by_20(self):
        self.assertRegex(SOURCE, r"const int Width = 28")
        self.assertRegex(SOURCE, r"const int Height = 20")
        self.assertIn("GridLayout.CellLayout.Isometric", GENERATOR)
        self.assertIn("Ground_560_Tiles", GENERATOR)
        self.assertIn("x < Forest01TerrainLayout.Width", GENERATOR)
        self.assertIn("y < Forest01TerrainLayout.Height", GENERATOR)

    def test_path_is_continuous_and_not_repeated(self):
        route = road()
        self.assertEqual(40, len(route))
        self.assertEqual(len(route), len(set(route)))
        self.assertEqual((0, 13), route[0])
        self.assertEqual((27, 9), route[-1])
        for a, b in zip(route, route[1:]):
            self.assertEqual(1, abs(a[0] - b[0]) + abs(a[1] - b[1]))
        for x, y in route:
            self.assertTrue(0 <= x < 28 and 0 <= y < 20)

    def test_build_sites_separated_from_route(self):
        slots = cells("Sites")
        route = road()
        self.assertEqual(10, len(slots))
        self.assertEqual(10, len(set(slots)))
        for x, y in slots:
            self.assertTrue(0 <= x < 28 and 0 <= y < 20)
            self.assertGreaterEqual(min(abs(x - a) + abs(y - b) for a, b in route), 2)

    def test_water_and_decorations_have_no_conflicts(self):
        slots = set(cells("Sites"))
        route = set(road())
        water = set(cells("Water"))
        trees = cells("Trees")
        rocks = cells("Rocks")
        self.assertIn((18, 4), water)
        self.assertEqual(18, len(water))
        self.assertTrue(water.isdisjoint(route))
        self.assertTrue(water.isdisjoint(slots))
        self.assertEqual(len(trees) + len(rocks), len(set(trees + rocks)))
        for c in trees + rocks:
            self.assertNotIn(c, slots | route | water)
            self.assertTrue(0 <= c[0] < 28 and 0 <= c[1] < 20)

    def test_old_gameplay_art_is_not_replaced(self):
        self.assertIn("Forest01_TerrainTest.unity", GENERATOR)
        self.assertIn("Assets/FantasyTD/V2/TerrainTest/Generated", GENERATOR)
        self.assertNotIn("Resources/FT2/Forest01Art.png", GENERATOR)
        self.assertNotIn("ScenePath = \"Assets/FantasyTD/V2/Scenes/Forest01.unity\"", GENERATOR)

    def test_camera_capture_has_real_render_readback_order(self):
        """Static contract: Unity camera must render before reading the correct RT."""
        self.assertIn('Forest01_TerrainTest_Preview.png', GENERATOR)
        self.assertIn('Forest01_TerrainTest_Preview_20x9.png', GENERATOR)
        render = GENERATOR.index('camera.Render();')
        activate = GENERATOR.index('RenderTexture.active = target;', render)
        readback = GENERATOR.index('screenshot.ReadPixels(', activate)
        self.assertLess(render, activate)
        self.assertLess(activate, readback)
        self.assertIn('different < 10', GENERATOR)
        self.assertIn('Unity camera output is blank/uniform', GENERATOR)

    def test_site_component_has_its_own_unity_script_asset(self):
        """The Unity scene must retain each build-site MonoBehaviour after serialization/reload."""
        scripts = ROOT / "UnityProject/Assets/FantasyTD/V2/Terrain"
        site_file = scripts / "Forest01TerrainBuildSite.cs"
        self.assertTrue(site_file.is_file(),
                        "Unity cannot persist Forest01TerrainBuildSite without its own matching C# script asset")
        self.assertRegex(site_file.read_text(encoding="utf-8"),
                         r"class\s+Forest01TerrainBuildSite\s*:\s*MonoBehaviour")
        interaction = (scripts / "Forest01TerrainInteraction.cs").read_text(encoding="utf-8")
        self.assertNotIn("class Forest01TerrainBuildSite", interaction,
                         "Only Forest01TerrainInteraction should be defined in its script asset")
        meta = (scripts / "Forest01TerrainBuildSite.cs.meta").read_text(encoding="utf-8")
        self.assertRegex(meta, r"(?m)^guid: [0-9a-f]{32}$")

if __name__ == "__main__":
    unittest.main()
