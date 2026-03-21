import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create a simple 10x10 grayscale image (1 band)
# uint8 is required for JPEG
gray = np.full((10, 10), 128, dtype=np.uint8)  # mid-gray

# Stack into (bands, height, width)
data = np.expand_dims(gray, axis=0)

# Define a simple transform
transform = from_origin(0, 10, 1, 1)

output_file = "test_10x10_grayscale_jpeg.tif"

with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=10,
    width=10,
    count=1,                 # single band for grayscale
    dtype="uint8",
    crs="EPSG:4326",
    transform=transform,
    compress="JPEG",
    photometric="MINISBLACK",  # grayscale
    interleave="pixel"
) as dst:
    dst.write(data)

print(f"Created {output_file}")