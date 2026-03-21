import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create a simple 10x10 CMYK image (4 bands)
# uint8 is required for JPEG

# Example: pure cyan (C=255, others 0)
cyan = np.full((10, 10), 255, dtype=np.uint8)
magenta = np.zeros((10, 10), dtype=np.uint8)
yellow = np.zeros((10, 10), dtype=np.uint8)
black = np.zeros((10, 10), dtype=np.uint8)

# Stack into (bands, height, width)
data = np.stack([cyan, magenta, yellow, black])

# Define a simple transform
transform = from_origin(0, 10, 1, 1)

output_file = "test_10x10_cmyk_jpeg.tif"

with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=10,
    width=10,
    count=4,                 # four bands for CMYK
    dtype="uint8",
    crs="EPSG:4326",
    transform=transform,
    compress="JPEG",
    photometric="CMYK",      # CMYK color space
    interleave="pixel"       # required for JPEG
) as dst:
    dst.write(data)

print(f"Created {output_file}")