import rasterio
from rasterio.transform import from_origin
import numpy as np

import os
from pathlib import Path
outdir = Path(os.environ['TIF_OUTPUT_DIR'])

# Create some dummy raster data
width, height = 100, 100
data = np.random.randint(0, 255, (height, width)).astype("uint8")

transform = from_origin(0, 0, 1, 1)

output_file = "custom_gdal_metadata_writing3.tif"

outfile = str(outdir / output_file)


with rasterio.open(
    outfile,
    "w",
    driver="GTiff",
    height=height,
    width=width,
    count=1,
    dtype=data.dtype,
    crs="EPSG:4326",
    transform=transform,
) as dst:
    # Write raster band
    dst.write(data, 1)
    dst.update_tags(string_tag = "This is a custom tag value", DESCRIPTION= "HELLO WORLD")