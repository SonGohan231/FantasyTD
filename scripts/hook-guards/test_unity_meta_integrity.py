import importlib.util
import pathlib
import subprocess
import tempfile
import unittest

SPEC=importlib.util.spec_from_file_location("meta_guard", pathlib.Path(__file__).with_name("unity_meta_integrity.py"))
MODULE=importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)

class MetaGuardTests(unittest.TestCase):
    def setup_files(self,dir,items):
        root=pathlib.Path(dir)
        for name,contents in items.items():
            p=root/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(contents,encoding="utf-8")
        return root
    def test_valid_unique_guid(self):
        with tempfile.TemporaryDirectory() as dir:
            root=self.setup_files(dir,{"UnityProject/Assets/Maps/Forest01.unity":"scene",
                "UnityProject/Assets/Maps/Forest01.unity.meta":"fileFormatVersion: 2\nguid: "+"a"*32+"\n"})
            names={"UnityProject/Assets/Maps/Forest01.unity","UnityProject/Assets/Maps/Forest01.unity.meta"}
            self.assertEqual(MODULE.check(root,names,names),[])
    def test_duplicate_guid_is_rejected(self):
        with tempfile.TemporaryDirectory() as dir:
            root=self.setup_files(dir,{"UnityProject/Assets/A.png.meta":"guid: "+"a"*32+"\n",
                "UnityProject/Assets/B.png.meta":"guid: "+"a"*32+"\n"})
            issues=MODULE.check(root,{"UnityProject/Assets/A.png.meta","UnityProject/Assets/B.png.meta"},set())
            self.assertTrue(any("duplicate GUID" in msg for _,msg in issues))
    def test_invalid_guid_is_rejected(self):
        with tempfile.TemporaryDirectory() as dir:
            root=self.setup_files(dir,{"UnityProject/Assets/A.png.meta":"guid: malformed\n"})
            issues=MODULE.check(root,{"UnityProject/Assets/A.png.meta"},set())
            self.assertTrue(any("invalid" in msg for _,msg in issues))
    def test_missing_new_meta_is_rejected(self):
        with tempfile.TemporaryDirectory() as dir:
            root=self.setup_files(dir,{"UnityProject/Assets/Maps/New.unity":"scene"})
            issues=MODULE.check(root,{"UnityProject/Assets/Maps/New.unity"},{"UnityProject/Assets/Maps/New.unity"})
            self.assertTrue(any("lacks a tracked .meta" in msg for _,msg in issues))
    def test_old_asset_without_meta_is_not_new_regression(self):
        with tempfile.TemporaryDirectory() as dir:
            root=self.setup_files(dir,{"UnityProject/Assets/Maps/Old.unity":"scene"})
            self.assertEqual(MODULE.check(root,{"UnityProject/Assets/Maps/Old.unity"},set()),[])
if __name__=="__main__":unittest.main(verbosity=2)
