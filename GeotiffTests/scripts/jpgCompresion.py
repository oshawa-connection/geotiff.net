import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create a simple 10x10 RGB image (3 bands)
# Each band is uint8 (required for JPEG)
red = np.full((10, 10), 255, dtype=np.uint8)   # solid red
green = np.zeros((10, 10), dtype=np.uint8)     # no green
blue = np.zeros((10, 10), dtype=np.uint8)      # no blue

# Stack into (bands, height, width)
data = np.stack([red, green, blue])

# Define a simple transform
transform = from_origin(0, 10, 1, 1)

output_file = "test_10x10_rgb_jpeg.tif"

with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=10,
    width=10,
    count=3,
    dtype="uint8",
    crs="EPSG:4326",
    transform=transform,
    compress="JPEG",
    photometric="YCBCR",
    interleave="pixel"  # important for JPEG
) as dst:
    dst.write(data)

print(f"Created {output_file}")