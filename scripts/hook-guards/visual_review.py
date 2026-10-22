#!/usr/bin/env python3
"""Advisory real-render PNG comparison; never treats missing evidence as PASS."""
import argparse,json,sys
from pathlib import Path
def evaluate(actual,baseline,threshold):
    from PIL import Image,ImageChops,ImageStat
    a=Path(actual)
    if not a.is_file():
        return {"status":"NOT_VERIFIED","reason":"Unity screenshot not available"},2
    with Image.open(a) as im: img=im.convert("RGB")
    result={"actual":str(a),"actual_size":list(img.size),"status":"REVIEW_REQUIRED"}
    b=Path(baseline)
    if not b.is_file():
        result["reason"]="Approved actual-render PNG baseline missing; concept art is NOT a pixel-perfect baseline."
        return result,3
    with Image.open(b) as im: reference=im.convert("RGB")
    if img.size!=reference.size:
        result["reason"]="Dimensions differ"
        result["baseline_size"]=list(reference.size)
        return result,1
    diff=ImageChops.difference(img,reference)
    mean=sum(ImageStat.Stat(diff).mean)/3/255
    result["normalized_mean_absolute_difference"]=round(mean,6)
    result["threshold"]=threshold
    result["status"]="WITHIN_THRESHOLD" if mean<=threshold else "REVIEW_REQUIRED"
    result["reason"]="Statistical pixel comparison only; human approval remains necessary."
    return result,(0 if mean<=threshold else 1)
def main():
    p=argparse.ArgumentParser()
    p.add_argument("--actual",required=True)
    p.add_argument("--baseline",required=True)
    p.add_argument("--report",required=True)
    p.add_argument("--threshold",type=float,default=.035)
    args=p.parse_args()
    if not 0<=args.threshold<=1:
        p.error("threshold must be between zero and one")
    result,code=evaluate(args.actual,args.baseline,args.threshold)
    Path(args.report).parent.mkdir(parents=True,exist_ok=True)
    Path(args.report).write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
    print("VISUAL_REVIEW "+result["status"]+" "+result.get("reason",""))
    return code
if __name__=="__main__":sys.exit(main())
