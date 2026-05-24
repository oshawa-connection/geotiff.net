import struct
import os
from pathlib import Path

outdir = Path(os.environ['TIF_OUTPUT_DIR'])
f = open(outdir / "big_single_strip_4gb_bigtiff.tif", "wb")

# =========================================================
# BIGTIFF HEADER
# =========================================================
f.write(b'II')
f.write(struct.pack('<H', 43))   # version
f.write(struct.pack('<H', 8))    # offsets are 8 bytes
f.write(struct.pack('<H', 0))    # reserved
f.write(struct.pack('<Q', 0))    # placeholder IFD offset

# =========================================================
# IMAGE STRIP (SPARSE 4+ GB)
# =========================================================

image_offset = f.tell()

# Make strip > 4GB (e.g. 4.2GB)
strip_size = 4 * 1024**3 + 200 * 1024**2  # 4.2 GB

# IMPORTANT: do NOT write bytes, just seek
f.seek(image_offset + strip_size - 1)
f.write(b'\0')

# =========================================================
# GEOTIFF AUX DATA (unchanged, but written BEFORE IFD)
# =========================================================

pixel_scale_offset = f.tell()
for v in (1.0, 1.0, 0.0):
    f.write(struct.pack('<d', v))

tiepoint_offset = f.tell()
for v in (0.0, 0.0, 0.0, 100.0, 200.0, 0.0):
    f.write(struct.pack('<d', v))

geokey_offset = f.tell()
geokeys = [
    1, 1, 0, 3,
    1024, 0, 1, 2,
    1025, 0, 1, 1,
    2048, 0, 1, 4326
]
for v in geokeys:
    f.write(struct.pack('<H', v))

# =========================================================
# IFD AT END
# =========================================================

ifd_offset = f.tell()
entries = []

# --- core tags ---
entries.append((256, 4, 1, 10))                 # width
entries.append((257, 4, 1, 10))                 # height
entries.append((258, 3, 1, 8))                  # bits
entries.append((259, 3, 1, 1))                  # no compression
entries.append((262, 3, 1, 1))                  # grayscale

# SINGLE STRIP BIGTIFF
entries.append((273, 16, 1, image_offset))      # StripOffsets
entries.append((277, 3, 1, 1))
entries.append((278, 4, 1, 10))

# CRITICAL FIX: StripByteCounts must be 16 (LONG8), not 4-byte LONG
entries.append((279, 16, 1, strip_size))        # 4+ GB strip

# GeoTIFF
entries.append((33550, 12, 3, pixel_scale_offset))
entries.append((33922, 12, 6, tiepoint_offset))
entries.append((34735, 3, len(geokeys), geokey_offset))

# =========================================================
# WRITE IFD
# =========================================================

f.write(struct.pack('<Q', len(entries)))

for tag, typ, count, value in entries:
    f.write(struct.pack('<HHQQ', tag, typ, count, value))

f.write(struct.pack('<Q', 0))  # next IFD

# PATCH HEADER
f.seek(8)
f.write(struct.pack('<Q', ifd_offset))

f.close()