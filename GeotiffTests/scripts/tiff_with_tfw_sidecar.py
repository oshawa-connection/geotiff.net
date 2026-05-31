import rasterio
from rasterio.transform import from_origin
import numpy as np
from pathlib import Path

# Example raster data
width = 100
height = 100
data = np.random.randint(0, 255, (height, width), dtype=np.uint8)

# Define georeferencing
transform = from_origin(
    west=500000,   # x coordinate of upper-left corner
    north=4100000, # y coordinate of upper-left corner
    xsize=10,      # pixel width
    ysize=10       # pixel height
)

tif_path = "output.tif"

# Write GeoTIFF
with rasterio.open(
    tif_path,
    "w",
    driver="GTiff",
    height=height,
    width=width,
    count=1,
    dtype=data.dtype,
    crs="EPSG:32633",
    transform=transform,
) as dst:
    dst.write(data, 1)

# Write TFW sidecar file
tfw_path = Path(tif_path).with_suffix(".tfw")

a = transform.a  # pixel size in x
d = transform.d  # rotation term
b = transform.b  # rotation term
e = transform.e  # pixel size in y (typically negative)
c = transform.c + a / 2.0 + b / 2.0  # center of upper-left pixel
f = transform.f + d / 2.0 + e / 2.0  # center of upper-left pixel

with open(tfw_path, "w") as tfw:
    tfw.write(f"{a}\n")
    tfw.write(f"{d}\n")
    tfw.write(f"{b}\n")
    tfw.write(f"{e}\n")
    tfw.write(f"{c}\n")
    tfw.write(f"{f}\n")

print(f"Wrote {tif_path}")
print(f"Wrote {tfw_path}")