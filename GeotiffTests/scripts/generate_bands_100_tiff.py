import numpy as np
import rasterio
from rasterio.transform import from_origin
from rasterio.crs import CRS

width = 50               # columns: longitudes 0 .. 50
height = 50              # rows: latitudes 50 .. 0 (north -> south)
pixel_size = 1           # 1 degree
transform = from_origin(west=0, north=50, xsize=pixel_size, ysize=pixel_size)
crs = CRS.from_epsg(4326)  # WGS84

# Write GeoTIFF
profile = {
    "driver": "GTiff",
    "width": width,
    "height": height,
    "count": 100,
    "dtype": "int32",
    "crs": crs,
    "transform": transform,
    "tiled": False
}

with rasterio.open("bands_100.tif", "w", **profile) as dst:
    for i in range(0,100):
        arr = np.full((height, width), i+1, dtype=np.int32)
        dst.write(arr, i+1)

print("Wrote bands_100.tif")