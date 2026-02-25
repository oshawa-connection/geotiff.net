import numpy as np
import rasterio
from rasterio.transform import from_origin

# Output file name
output_file = "ten_band_2x2.tif"

# Raster dimensions
width, height = 2, 2
count = 10  # number of bands

# Define transform (arbitrary georeferencing)
transform = from_origin(0, 2, 1, 1)  # (west, north, xsize, ysize)

# Create the raster
with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=height,
    width=width,
    count=count,
    dtype=rasterio.uint8,
    transform=transform,
) as dst:
    for band in range(1, count + 1):
        data = np.full((height, width), band, dtype=np.uint8)
        dst.write(data, band)

print(f"Created {output_file}")