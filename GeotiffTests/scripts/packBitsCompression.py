import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create a small 10x10 raster with a simple pattern
width, height = 10, 10
data = np.arange(width * height, dtype=np.uint8).reshape((height, width))

# Define a basic georeferencing (top-left corner at (0, 10), pixel size 1x1)
transform = from_origin(0, 10, 1, 1)

# Output file
output_file = "packbits.tif"

# Write GeoTIFF with PackBits compression
with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=height,
    width=width,
    count=1,
    dtype=data.dtype,
    crs="EPSG:4326",          # WGS84
    transform=transform,
    compress="PACKBITS"
) as dst:
    dst.write(data, 1)

print(f"GeoTIFF written to {output_file}")