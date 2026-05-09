import numpy as np
import rasterio
from rasterio.transform import from_origin

# Output file path
output_tif = "no_data_outline_float32.tif"

# Raster dimensions
width = 10
height = 10

# Define nodata value for float32
nodata_value = -9999.0

# Create a 10x10 array filled with 10.0
data = np.full((height, width), 10.0, dtype=np.float32)

# Set outer perimeter pixels to nodata
data[0, :] = nodata_value        # top row
data[-1, :] = nodata_value       # bottom row
data[:, 0] = nodata_value        # left column
data[:, -1] = nodata_value       # right column

# Define an arbitrary affine transform
transform = from_origin(0, 10, 1, 1)

# Write GeoTIFF
with rasterio.open(
    output_tif,
    "w",
    driver="GTiff",
    height=height,
    width=width,
    count=1,
    dtype=rasterio.float32,
    crs="EPSG:4326",
    transform=transform,
    nodata=nodata_value,
) as dst:
    dst.write(data, 1)

print(f"Created {output_tif}")