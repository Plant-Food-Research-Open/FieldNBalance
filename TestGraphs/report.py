import aspose.words as aw
import os.path as osp
from glob import glob
from pathlib import Path

def main():
    img_path = osp.join("TestGraphs", "Outputs")
    if not osp.isdir(img_path):
        raise FileNotFoundError(f"Directory does not exist: {img_path}")
    
    imgs = glob(osp.join(img_path, "*.png"))
    if len(imgs) == 0:
        raise FileNotFoundError(f"No images found in directory: {img_path}")
    
    doc = aw.Document()
    builder = aw.DocumentBuilder(doc)

    for img in imgs:
        builder.insert_image(img)

    outdir = "html"
    Path(outdir).mkdir(parents=True, exist_ok=True) 
    doc.save(osp.join(outdir, "index.html"))

if __name__ == "__main__":
    main()