import numpy as np
import rasterio
from rasterio.transform import from_origin

width = 512
height = 512
tile_size = 256

profile = {
    "driver": "GTiff",
    "width": width,
    "height": height,
    "count": 1,
    "dtype": "int32",
    "crs": "EPSG:4326",
    "transform": from_origin(0, 0, 1, 1),
    "tiled": True,
    "blockxsize": tile_size,
    "blockysize": tile_size,
    "nodata": 0,
    "compress": "DEFLATE",
    "SPARSE_OK": "YES",
}

with rasterio.open("sparse_int32.tif", "w", **profile) as dst:
    # Write only the upper-left tile.
    data = np.full((tile_size, tile_size), 123, dtype=np.int32)

    dst.write(
        data,
        1,
        window=((0, tile_size), (0, tile_size))
    )