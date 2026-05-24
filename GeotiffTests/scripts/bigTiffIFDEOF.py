import struct
import os
from pathlib import Path
outdir = Path(os.environ['TIF_OUTPUT_DIR'])

f = open(outdir / "end_ifd_bigtiff_geotiff.tif", "wb")
# =========================================================
# BIGTIFF HEADER
# =========================================================
# Little endian
f.write(b'II')
# BigTIFF version
f.write(struct.pack('<H', 43))
# Offset size = 8 bytes
f.write(struct.pack('<H', 8))
# Reserved
f.write(struct.pack('<H', 0))
# Placeholder for first IFD offset
f.write(struct.pack('<Q', 0))
# =========================================================
# IMAGE DATA
# =========================================================
image_offset = f.tell()
# 10x10 grayscale image
image_data = bytes([128] * 100)
f.write(image_data)

# =========================================================
# GEOTIFF AUXILIARY DATA
# =========================================================

# ---------------------------------------------------------
# ModelPixelScaleTag (33550)
# 3 doubles
# ---------------------------------------------------------

pixel_scale_offset = f.tell()

pixel_scale = (
    1.0,   # pixel width
    1.0,   # pixel height
    0.0    # z scale
)

for v in pixel_scale:
    f.write(struct.pack('<d', v))

# ---------------------------------------------------------
# ModelTiepointTag (33922)
# 6 doubles
# ---------------------------------------------------------

tiepoint_offset = f.tell()

tiepoints = (
    0.0, 0.0, 0.0,   # raster x,y,z
    100.0, 200.0, 0.0  # geographic x,y,z
)

for v in tiepoints:
    f.write(struct.pack('<d', v))

# ---------------------------------------------------------
# GeoKeyDirectoryTag (34735)
# Minimal geographic CRS definition
#
# Using:
#   GTModelTypeGeoKey       = Geographic
#   GTRasterTypeGeoKey      = PixelIsArea
#   GeographicTypeGeoKey    = EPSG:4326
# ---------------------------------------------------------

geokey_offset = f.tell()

geokeys = [
    # Header
    1,      # KeyDirectoryVersion
    1,      # KeyRevision
    0,      # MinorRevision
    3,      # NumberOfKeys
    # GTModelTypeGeoKey
    1024, 0, 1, 2,
    # GTRasterTypeGeoKey
    1025, 0, 1, 1,
    # GeographicTypeGeoKey
    2048, 0, 1, 4326
]

for v in geokeys:
    f.write(struct.pack('<H', v))

# =========================================================
# IFD AT END OF FILE
# =========================================================

ifd_offset = f.tell()
entries = []

# ---------------------------------------------------------
# Core TIFF tags
# ---------------------------------------------------------

entries.append((256, 4, 1, 10))                 # ImageWidth
entries.append((257, 4, 1, 10))                 # ImageLength
entries.append((258, 3, 1, 8))                  # BitsPerSample
entries.append((259, 3, 1, 1))                  # Compression
entries.append((262, 3, 1, 1))                  # PhotometricInterpretation

entries.append((273, 16, 1, image_offset))      # StripOffsets
entries.append((277, 3, 1, 1))                  # SamplesPerPixel
entries.append((278, 4, 1, 10))                 # RowsPerStrip
entries.append((279, 16, 1, len(image_data)))   # StripByteCounts

# ---------------------------------------------------------
# GeoTIFF tags
# ---------------------------------------------------------

entries.append((33550, 12, 3, pixel_scale_offset))
entries.append((33922, 12, 6, tiepoint_offset))
entries.append((34735, 3, len(geokeys), geokey_offset))

# =========================================================
# WRITE BIGTIFF IFD
# =========================================================

# Number of entries (uint64)
f.write(struct.pack('<Q', len(entries)))

for tag, typ, count, value in entries:

    # BigTIFF entry format:
    # uint16 tag
    # uint16 type
    # uint64 count
    # uint64 value_or_offset

    f.write(struct.pack(
        '<HHQQ',
        tag,
        typ,
        count,
        value
    ))
# Next IFD offset = none
f.write(struct.pack('<Q', 0))
# =========================================================
# PATCH HEADER WITH FIRST IFD OFFSET
# =========================================================
f.seek(8)
f.write(struct.pack('<Q', ifd_offset))
f.close()