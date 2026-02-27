import rasterio
from rasterio.enums import Compression
import numpy as np

# Define raster properties
width, height = 100, 100
count = 2
dtype = 'int32'
filename = 'int32_2band.tif'

# Create data arrays
band1 = np.ones((height, width), dtype=np.int32)
band2 = np.full((height, width), 2, dtype=np.int32)

with rasterio.open(
    filename,
    'w',
    driver='GTiff',
    width=width,
    height=height,
    count=count,
    dtype=dtype,
    compress=Compression.none
) as dst:
    dst.write(band1, 1)
    dst.write(band2, 2)