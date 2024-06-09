# ---
# jupyter:
#   jupytext:
#     formats: ipynb,py:light
#     text_representation:
#       extension: .py
#       format_name: light
#       format_version: '1.5'
#       jupytext_version: 1.15.0
#   kernelspec:
#     display_name: Python 3 (ipykernel)
#     language: python
#     name: python3
# ---

# +
import os
import aspose.words as aw
import os.path as osp
from glob import glob
from pathlib import Path

try: 
    if os.environ["GITHUB_WORKSPACE"] != None:
        root = os.environ["GITHUB_WORKSPACE"]
        inPath = os.path.join(root, "TestGraphs", "Outputs")

except: 
    rootfrags = os.path.abspath('WS2.py').split("\\")
    root = ""
    for d in rootfrags:
        if d == "FieldNBalance":
            break
        else:
            root += d + "\\"
    inPath = os.path.join(root,"FieldNBalance","TestGraphs", "Outputs")   
    

if not osp.isdir(inPath):
    raise FileNotFoundError(f"Directory does not exist: {inPath}")

imgs = glob(osp.join(inPath, "*.png"))
if len(imgs) == 0:
    raise FileNotFoundError(f"No images found in directory: {inPath}")

doc = aw.Document()
builder = aw.DocumentBuilder(doc)

for img in imgs:
    builder.insert_image(img)

outdir = "html"
Path(outdir).mkdir(parents=True, exist_ok=True) 
doc.save(osp.join(outdir, "index.html"))

