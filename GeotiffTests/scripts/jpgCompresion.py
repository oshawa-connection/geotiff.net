import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create a simple 10x10 image in YCbCr space
# Choose constant non-zero values for all bands
Y  = np.full((10, 10), 85, dtype=np.uint8)  # luminance (brightness)
Cb = np.full((10, 10), 131, dtype=np.uint8)  # blue-difference chroma
Cr = np.full((10, 10), 127, dtype=np.uint8)  # red-difference chroma

# Stack into (bands, height, width)
data = np.stack([Y, Cb, Cr])

# Define a simple transform
transform = from_origin(0, 10, 1, 1)

output_file = "test_10x10_ycbcr_jpeg.tif"

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
    interleave="pixel"  # required for JPEG YCbCr
) as dst:
    dst.write(data)

print(f"Created {output_file}")