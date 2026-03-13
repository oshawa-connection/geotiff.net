import numpy as np
import rasterio

# Create some dummy raster data
width = 100
height = 100
data = np.random.randint(0, 255, (height, width)).astype("uint8")

# Define profile WITHOUT transform or CRS
profile = {
    "driver": "GTiff",
    "width": width,
    "height": height,
    "count": 1,
    "dtype": "uint8"
}

output_path = "no_affine.tif"

with rasterio.open(output_path, "w", **profile) as dst:
    dst.write(data, 1)

print("GeoTIFF created without affine transform or tiepoints.")