import numpy as np
import rasterio
from rasterio.enums import Resampling
from rasterio.transform import from_origin

# Output path
out_tif = "internal_overviews.tif"

# Raster dimensions
width = height = 256

# Example data (int32)
data = np.arange(width * height, dtype=np.int32).reshape((height, width))

# Simple georeferencing (optional but recommended)
transform = from_origin(0, 256, 1, 1)  # (west, north, xsize, ysize)

profile = {
    "driver": "GTiff",
    "width": width,
    "height": height,
    "count": 1,
    "dtype": "int32",
    "crs": "EPSG:3857",     # arbitrary CRS
    "transform": transform,
    "tiled": True,         # required for internal overviews
    "blockxsize": 256,
    "blockysize": 256,
    "compress": "deflate"
}

with rasterio.open(out_tif, "w", **profile) as dst:
    dst.write(data, 1)

    # Define overview levels (downsampling factors)
    overviews = [2, 4, 8]

    # Build internal overviews
    dst.build_overviews(overviews, Resampling.nearest)

    # Mark how overviews were generated
    dst.update_tags(ns="rio_overview", resampling="nearest")

print("GeoTIFF with internal overviews written:", out_tif)