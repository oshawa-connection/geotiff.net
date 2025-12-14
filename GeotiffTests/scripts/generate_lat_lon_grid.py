import numpy as np
import rasterio
from rasterio.transform import from_origin
from rasterio.crs import CRS

width = 50               # columns: longitudes 0 .. 50
height = 50              # rows: latitudes 50 .. 0 (north -> south)
pixel_size = 1           # 1 degree
transform = from_origin(west=0, north=50, xsize=pixel_size, ysize=pixel_size)
crs = CRS.from_epsg(4326)  # WGS84

# Build band data (int32)
# Pixel centers: lon = 0 + (col + 0.5), lat = 50 - (row + 0.5)
rows = np.arange(height).reshape(-1, 1)        # 0..49 as column vector
cols = np.arange(width).reshape(1, -1)         # 0..49 as row vector

lat_center = 50 - (rows + 0.5)                 # shape (50,1)
lon_center = 0 + (cols + 0.5)                  # shape (1,50)

# Band 1: latitude of the pixel (stored as int32).
# Since the file is int32, we store the integer latitude for the pixel's center (floor).
band1 = np.floor(lat_center).astype(np.int32) * np.ones((1, width), dtype=np.int32)

# Band 2: longitude of the pixel, rounded to nearest integer (per spec).
# Use "round half up": floor(x + 0.5) to avoid Python's bankers rounding.
band2 = np.floor(lon_center).astype(np.int32) * np.ones((height, 1), dtype=np.int32)

# Broadcast to full grids
band1 = np.repeat(band1, width, axis=1)        # (50,50)
band2 = np.repeat(band2, height, axis=0)       # (50,50)

# Write GeoTIFF
profile = {
    "driver": "GTiff",
    "width": width,
    "height": height,
    "count": 2,
    "dtype": "int32",
    "crs": crs,
    "transform": transform,
    "tiled": False
}

with rasterio.open("lat_lon_grid.tif", "w", **profile) as dst:
    dst.write(band1, 1)  # Band 1: latitude (int degrees)
    dst.write(band2, 2)  # Band 2: longitude (nearest int)

print("Wrote lat_lon_grid.tif")