import math
import rasterio
from rasterio.transform import from_origin
import numpy as np

import os
from pathlib import Path
outdir = Path(os.environ['TIF_OUTPUT_DIR'])


# Target file
output_tif = outdir / "big_int64_4gb.tif"

# Desired approximate size in bytes
TARGET_SIZE_BYTES = 4 * 1024**3  # 4 GiB

# int64 max value
INT64_MAX = np.iinfo(np.int64).max

# GeoTIFF settings
block_size = 512
count = 1
nodata = None

# -------------------------------------------------------------------
# Calculate raster dimensions.
#
# int64 = 8 bytes per pixel.
# We create a square raster close to 4 GiB.
# -------------------------------------------------------------------
bytes_per_pixel = 8
num_pixels = TARGET_SIZE_BYTES // bytes_per_pixel
side = int(math.sqrt(num_pixels))

width = side
height = side

estimated_size = width * height * bytes_per_pixel
print(f"Creating raster:")
print(f"  Width:  {width}")
print(f"  Height: {height}")
print(f"  Estimated size: {estimated_size / (1024**3):.2f} GiB")

# Affine geotransform
# Origin: upper-left corner at (100000, 200000)
# Pixel size: 30 x 30
# North-up raster (negative y pixel size)
transform = from_origin(
    west=100000,
    north=200000,
    xsize=30,
    ysize=30,
)

profile = {
    "driver": "GTiff",
    "width": width,
    "height": height,
    "count": count,
    "dtype": "int64",
    "crs": None,
    "transform": transform,
    "tiled": True,
    "blockxsize": block_size,
    "blockysize": block_size,
    "BIGTIFF": "YES",
    "compress": None,
}

# -------------------------------------------------------------------
# Write tiled blocks filled with int64 max.
# -------------------------------------------------------------------
with rasterio.open(output_tif, "w", **profile) as dst:

    tile = np.full(
        (block_size, block_size),
        INT64_MAX,
        dtype=np.int64,
    )

    for row in range(0, height, block_size):
        for col in range(0, width, block_size):

            win_height = min(block_size, height - row)
            win_width = min(block_size, width - col)

            window = rasterio.windows.Window(
                col_off=col,
                row_off=row,
                width=win_width,
                height=win_height,
            )

            # Trim tile for edge windows
            data = tile[:win_height, :win_width]

            dst.write(data, 1, window=window)

