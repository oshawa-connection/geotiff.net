import numpy as np
import rasterio


import os
from pathlib import Path
outdir = Path(os.environ['TIF_OUTPUT_DIR'])

# Create a small 10x10 raster
data = np.arange(100, dtype=np.uint8).reshape(10, 10)

tif_path = "test.tif"
tfw_path = "test.tfw"

# Create TIFF WITHOUT CRS or affine transform
with rasterio.open(
    outdir / tif_path,
    "w",
    driver="GTiff",
    height=data.shape[0],
    width=data.shape[1],
    count=1,
    dtype=data.dtype,
) as dst:
    dst.write(data, 1)

# World file parameters
#
# Location: approximately central London
# Coordinate system intended to be EPSG:27700 (British National Grid)
#
# World file format:
# Line 1: pixel size in X direction
# Line 2: rotation term
# Line 3: rotation term
# Line 4: negative pixel size in Y direction
# Line 5: X coordinate of centre of upper-left pixel
# Line 6: Y coordinate of centre of upper-left pixel

pixel_size = 100.0  # metres

x_ul_center = 530000.0
y_ul_center = 180000.0

with open(outdir / tfw_path, "w") as f:
    f.write(f"{pixel_size}\n")   # A
    f.write("0.0\n")             # D
    f.write("0.0\n")             # B
    f.write(f"{-pixel_size}\n")  # E
    f.write(f"{x_ul_center}\n")  # C
    f.write(f"{y_ul_center}\n")  # F

print(f"Created {tif_path}")
print(f"Created {tfw_path}")