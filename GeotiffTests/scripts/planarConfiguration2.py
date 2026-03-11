import numpy as np
import rasterio
from rasterio.transform import from_origin

# First 5x5 array
band1 = np.array([
    [1,  1,  1,  1, 1],
    [1,  1, 22,  1, 1],
    [1, 11, 55, 33, 1],
    [1,  1, 44,  1, 1],
    [1,  1,  1,  1, 1]
], dtype=np.uint16)

# Second 5x5 array
band2 = np.array([
    [2,  2,  2,  2, 2],
    [2,  2, 24,  2, 2],
    [2, 12, 60, 36, 2],
    [2,  2, 48,  2, 2],
    [2,  2,  2,  2, 2]
], dtype=np.uint16)

# Define an arbitrary geotransform (top-left x, top-left y, pixel width, pixel height)
transform = from_origin(0, 5, 1, 1)

# Output file
output_file = "two_band_planar_separate.tif"

with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=5,
    width=5,
    count=2,
    dtype=np.uint16,
    crs="EPSG:4326",  # arbitrary CRS
    transform=transform,
    tiled=False,
    compress="none",
    planarconfig="SEPARATE"  # PLANARCONFIG=2
) as dst:
    dst.write(band1, 1)
    dst.write(band2, 2)

print(f"GeoTIFF written to {output_file}")