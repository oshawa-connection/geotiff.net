import numpy as np
import rasterio
from rasterio.transform import from_bounds

# Output file
output_path = "two_band_int32.tif"

# Raster dimensions
width = 50
height = 50
count = 2
dtype = "int32"

# Geographic bounds: left, bottom, right, top
bounds = (0, 0, 50, 50)

# Create affine transform
transform = from_bounds(*bounds, width=width, height=height)

# Total number of pixels
num_pixels = width * height

# Create band data
odd_numbers = np.arange(1, 2 * num_pixels, 2, dtype=np.int32).reshape((height, width))
even_numbers = np.arange(2, 2 * num_pixels + 1, 2, dtype=np.int32).reshape((height, width))

# Write raster
with rasterio.open(
    output_path,
    "w",
    driver="GTiff",
    width=width,
    height=height,
    count=count,
    dtype=dtype,
    crs=None,
    transform=transform,
) as dst:
    dst.write(odd_numbers, 1)
    dst.write(even_numbers, 2)

print(f"Raster written to {output_path}")