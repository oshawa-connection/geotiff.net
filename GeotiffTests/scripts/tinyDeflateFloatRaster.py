import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create 4x4 int32 data
data = np.array(
    [
        [1, 2, 3, 4],
        [1, 2, 3, 4],
        [1, 2, 3, 4],
        [1, 2, 3, 4],
    ],
    dtype=np.float32
)

# Arbitrary georeferencing
transform = from_origin(
    west=0.0,
    north=4.0,
    xsize=1.0,
    ysize=1.0
)

profile = {
    "driver": "GTiff",
    "height": 4,
    "width": 4,
    "count": 1,
    "dtype": "float32",
    "crs": "EPSG:4326",
    "transform": transform,
    "compress": "deflate",
}

with rasterio.open("tiny_4x4_deflate_float.tif", "w", **profile) as dst:
    dst.write(data, 1)