import importlib.util,pathlib,tempfile,unittest
from PIL import Image
spec=importlib.util.spec_from_file_location("visual",pathlib.Path(__file__).with_name("visual_review.py"))
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
class Tests(unittest.TestCase):
    def test_missing_image_is_not_verified(self):
        with tempfile.TemporaryDirectory() as d:
            r,code=m.evaluate(pathlib.Path(d)/"none.png",pathlib.Path(d)/"baseline.png",.05)
            self.assertEqual(code,2)
            self.assertEqual(r["status"],"NOT_VERIFIED")
    def test_missing_baseline_requires_review(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"actual.png";Image.new("RGB",(8,8),(50,90,140)).save(p)
            r,code=m.evaluate(p,pathlib.Path(d)/"missing.png",.05)
            self.assertEqual(code,3)
            self.assertEqual(r["status"],"REVIEW_REQUIRED")
    def test_equal_images_pass_metric(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)
            for name in ["actual.png","base.png"]:Image.new("RGB",(8,8),(50,90,140)).save(p/name)
            r,code=m.evaluate(p/"actual.png",p/"base.png",.05)
            self.assertEqual(code,0);self.assertEqual(r["normalized_mean_absolute_difference"],0)
    def test_changed_images_request_review(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)
            Image.new("RGB",(8,8),(50,90,140)).save(p/"actual.png")
            Image.new("RGB",(8,8),(0,0,0)).save(p/"base.png")
            r,code=m.evaluate(p/"actual.png",p/"base.png",.05)
            self.assertEqual(code,1);self.assertEqual(r["status"],"REVIEW_REQUIRED")
if __name__=="__main__":unittest.main()
