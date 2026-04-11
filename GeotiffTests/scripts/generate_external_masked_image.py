import numpy as np
import rasterio
from rasterio.enums import Compression
from rasterio.transform import from_origin

width = 50
height = 50
data = np.ones((height, width), dtype=np.int32)

profile = {
    'driver': 'GTiff',
    'dtype': 'int32',
    'count': 1,
    'width': width,
    'height': height,
    'compress': Compression.none,
    'transform': from_origin(0, 50, 1, 1),  # (west, north, xsize, ysize)
    'crs': 'EPSG:4326'
}

# Create a mask: left 25 columns are valid (1), right 25 are masked (0)
mask = np.zeros((height, width), dtype=np.uint8)
mask[:, :25] = 1  # left half valid

# Write the raster with mask
with rasterio.open('masked_image.tif', 'w', **profile) as dst:
    dst.write(data, 1)
    dst.write_mask(mask * 255)  # rasterio expects mask as 0 (masked) or 255 (valid)

print("Wrote masked_image.tif")