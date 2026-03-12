import numpy as np
import rasterio
from rasterio.transform import from_origin

# Create the 5x5 array (dtype = double / float64)
data = np.array([
    [1,  1,  1,  1, 1],
    [1,  1, 22,  1, 1],
    [1, 11, 55, 33, 1],
    [1,  1, 44,  1, 1],
    [1,  1,  1,  1, 1]
], dtype=np.float64)

# Define an arbitrary geotransform (top-left corner at (0,5), pixel size = 1x1)
transform = from_origin(0, 5, 1, 1)

# Output file name
output_file = "output.tif"

# Create GeoTIFF
with rasterio.open(
    output_file,
    "w",
    driver="GTiff",
    height=data.shape[0],
    width=data.shape[1],
    count=1,
    dtype="float64",
    crs="EPSG:4326",   # you can change CRS if needed
    transform=transform,
) as dst:
    dst.write(data, 1)

print(f"GeoTIFF created: {output_file}")