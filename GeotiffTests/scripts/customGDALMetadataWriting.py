import rasterio
from rasterio.transform import from_origin
import numpy as np

# Create some dummy raster data
width, height = 100, 100
data = np.random.randint(0, 255, (height, width)).astype("uint8")

transform = from_origin(0, 0, 1, 1)

output_file = "custom_gdal_metadata_writing.tif"

with rasterio.open(
    output_file,
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


print(f"GeoTIFF written to {output_file}")

with rasterio.open(output_file) as src:
    print(src.tags())