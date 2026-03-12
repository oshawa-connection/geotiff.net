import numpy as np
import rasterio
from rasterio.transform import Affine

width = 50
height = 50
res = 1.0

# top-left origin (0,50)
transform = Affine(res, 0, 0,
                   0, -res, 50)

data = np.zeros((height, width), dtype=np.float32)

with rasterio.open(
    "model_transform.tif",
    "w",
    driver="GTiff",
    width=width,
    height=height,
    count=1,
    dtype="float32",
    transform=transform,   # IMPORTANT: set here
) as dst:
    dst.write(data, 1)