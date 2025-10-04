# geotiff.net

A (WIP) port of [geotiff.js](https://geotiffjs.github.io/) to .Net. 

This project adds native .Net handling of geotiff files, the benefits being:
- Easier cross platform compatibility (over GDAL which requires native dependencies to be compiled on the target platform)
- Asynchronous, streamed and parallel reads + writes
- Easier debugging
- Extensability in C# (e.g. define your own source types)


## Alternatives libraries

Other than GDAL, there are several packages for reading (and possibly writing) geotiffs.

- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Does not support geotiff tags, COGs or spatial operations, synchronous.
- [Mission controller](https://github.com/ArduPilot/MissionPlanner/blob/cedabf7b610c0e54b8fe4409d903963faa69ab90/ExtLibs/Utilities/GeoTiff.cs) - Does not support COGs or spatial operations, synchronous. Tailored to the requirements of Ardupilot itself.
- [DEM.NET](https://github.com/dem-net/DEM.Net) - a wrapper around LibTiff.Net. Synchronous. Only filesystem files supported.

## Contributing

This project is a WIP, new contributors are very welcome. If you’d like to get involved, please open an early PR or start a discussion to share your ideas. Some ideas of good items to work on:

- Image resampling
- JPEG compression and decompression
- More user friendly handling of overviews during reading
- Writing, particularly to COG format with overviews
- The AWS and Azure source classes
- More spatial operation support - currently only `ReadValueAtCoordinate` is supported
- Benchmarking

## Compliance tests

The Compliance tests are a set of that compare the read tag and pixel read values between geotiff.js and geotiff.net. The tifs are not kept under version control, but are downloaded from [OSGeo's website](https://download.osgeo.org/geotiff/samples/) which is a good sample set to test against.
