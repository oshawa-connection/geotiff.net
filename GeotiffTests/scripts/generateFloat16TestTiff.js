import fs from "fs";
import GeoTIFF, { writeArrayBuffer } from "geotiff";

// Helper to convert Float32 → Float16 (IEEE 754 half precision)
function float32ToFloat16(val) {
    const floatView = new Float32Array(1);
    const int32View = new Int32Array(floatView.buffer);

    floatView[0] = val;
    const x = int32View[0];

    const sign = (x >> 16) & 0x8000;
    const mantissa = x & 0x7fffff;
    const exp = (x >> 23) & 0xff;

    if (exp === 0xff) {
        // NaN or Inf
        return sign | 0x7c00 | (mantissa ? 1 : 0);
    }

    let halfExp = exp - 127 + 15;

    if (halfExp >= 0x1f) {
        return sign | 0x7c00; // Inf
    } else if (halfExp <= 0) {
        if (halfExp < -10) {
            return sign; // underflow → 0
        }
        let m = (mantissa | 0x800000) >> (1 - halfExp);
        return sign | (m >> 13);
    }

    return sign | (halfExp << 10) | (mantissa >> 13);
}

async function createGeoTIFF() {
    const width = 10;
    const height = 10;

    // Create some test Float32 data
    const float32Data = new Float32Array(width * height);
    for (let i = 0; i < float32Data.length; i++) {
        float32Data[i] = i / 10; // simple gradient
    }

    // Convert to Float16 (Uint16Array backing)
    const float16Data = new Uint16Array(width * height);
    for (let i = 0; i < float32Data.length; i++) {
        float16Data[i] = float32ToFloat16(float32Data[i]);
    }



    // Create GeoTIFF
    const arrayBuffer = await writeArrayBuffer(float16Data,
        {
            width: 10,
            height : 10,
            SampleFormat: [3],      // 3 = IEEE floating point
            BitsPerSample: [16],    // 16-bit (Float16)
            SamplesPerPixel: 1,
            PlanarConfiguration: 1, // chunky
            PhotometricInterpretation: 1, // BlackIsZero
            ModelPixelScale: [1, 1, 0],
            ModelTiepoint: [0, 0, 0, 0, 0, 0],
            GeographicTypeGeoKey: 4326 // WGS84 (optional)
        });

    fs.writeFileSync("float16_10x10.tif", Buffer.from(arrayBuffer));
    console.log("GeoTIFF written: float16_10x10.tif");
}

createGeoTIFF().catch(console.error);