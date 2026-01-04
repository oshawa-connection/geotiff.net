import numpy as np
import rasterio
from rasterio.enums import Resampling
from rasterio.env import Env

# Output filename
path = "example.tif"

# Create some data
data = np.arange(1000 * 1000, dtype=np.int32).reshape((1000, 1000))

# Create the GeoTIFF
with rasterio.open(
    path,
    "w",
    driver="GTiff",
    width=1000,
    height=1000,
    count=1,
    dtype="int32",
    crs="EPSG:3857",
    transform=rasterio.transform.from_origin(0, 0, 1, 1),
) as dst:
    dst.write(data, 1)

# Build EXTERNAL overviews
with Env(GDAL_TIFF_INTERNAL_OVERVIEWS="NO"):
    with rasterio.open(path, "r+") as dst:
        dst.build_overviews(
            [2, 4, 8, 16],
            Resampling.nearest
        )
        dst.update_tags(ns="rio_overview", resampling="nearest")