# geotiff.net

A (WIP) port of [geotiff.js](https://geotiffjs.github.io/) to .Net.

## Alternatives:

- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Does not support geotiff tags, COGs or spatial operations, synchronous.
- [Mission controller](https://github.com/ArduPilot/MissionPlanner/blob/cedabf7b610c0e54b8fe4409d903963faa69ab90/ExtLibs/Utilities/GeoTiff.cs) - Does not support COGs or spatial operations, synchronous. Tailored to the requirements of Ardupilot itself.
which uses and extends LibTiff 

## TODO list
- Resampling
- Reading from projected or WGS84 rasters
- Casting/ promoting read values if not ulong.
- Support for more types other than float32
- More sources e.g. COG.
- Writing
