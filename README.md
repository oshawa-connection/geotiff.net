# geotiff.net

A (WIP) port of [geotiff.js](https://geotiffjs.github.io/) to .Net. 

This project adds native .Net handling of geotiff files, the benefits being:
- Easier cross platform compatibility (over GDAL which requires native dependencies to be compiled on the target platform)
- Asynchronous and streamed reads + writes
- Easier debugging
- Extensability in C# (e.g. define your own source types, decoders, sidecar file handlers)

## Examples

Read everything as a 2D array from the first sample of the first image from a file:
```csharp
var lonLatTif = Path.Combine(GetDataFolderPath(), "elevationData.tif");
await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
var geotiff = await GeoTIFF.FromStream(fsSource);
int count = await geotiff.GetImageCount();
var image = await geotiff.GetImage();
var readResult = await image.ReadRasters<int>();
var sampleResult = readResult.GetSampleResultAt(0).To2DArray();
```


Read data at the pixel at a coordinate from an AWS S3 bucket:
```csharp
using var client = new AmazonS3Client(new AmazonS3Config { ... });
var gtAWSClient = new GeotiffAWSClient("testbucket", "modelOutputDataCOG.tif", client);
GeoTIFF? geotiff = await GeoTIFF.FromRemoteClient(gtAWSClient);
var image = await geotiff.GetImage();
var result = await image.ReadValueAtCoordinate<double>(lon, lat); 
var waterVelocityAtCoordinate =  result.GetSampleResultAt(0).FlatData.GetValue(0);
var waterDepthAtCoordinate = result.GetSampleResultAt(1).FlatData.GetValue(0);
```

Read data from a file that has external overviews in an `.ovr` sidecar file
```csharp
var externalOverviewTifPath = Path.Combine(GetDataFolderPath(), "external_overviews.tif");
var ovrFilePath = externalOverviewTifPath + ".ovr";

await using var mainStream = File.OpenRead(externalOverviewTifPath);
await using var ovrStream = File.OpenRead(ovrFilePath);

var overviewMultiTiff = await MultiGeoTIFF.FromStreams(mainStream, new[] { ovrStream });
var imageCount = await overviewMultiTiff.GetImageCount();// 4; 1 from the main file and 3 from the overviews.
var hasOverviews = await overviewMultiTiff.HasOverviews(); // true
```

## Alternatives libraries

Other than GDAL, there are several packages for reading (and possibly writing) geotiffs.

- [GDAL through gdal.netcore](https://github.com/MaxRev-Dev/gdal.netcore) - [Quite low level](https://github.com/OSGeo/gdal/blob/master/doc/source/api/csharp/csharp_raster.rst). Synchronous. Packages up GDAL for you so you don't have to compile it + the C# bindings yourself.
- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Does not support geotiff tags, COGs or spatial operations, synchronous.
- [Mission controller](https://github.com/ArduPilot/MissionPlanner/blob/cedabf7b610c0e54b8fe4409d903963faa69ab90/ExtLibs/Utilities/GeoTiff.cs) - Does not support COGs or spatial operations, synchronous. Tailored to the requirements of Ardupilot itself.
- [DEM.NET](https://github.com/dem-net/DEM.Net) - a wrapper around LibTiff.Net. Synchronous. Only filesystem files supported.

## Contributing

This project is a WIP, new contributors are very welcome. If you’d like to get involved, please open an early PR or start a discussion to share your ideas. Some ideas of good items to work on:

Before release, the bare minimum:

- Handle case where the user does not know the type of the raster data before reading it, e.g. in cases where they are reading a user-passed tiff file.
- Image resampling
- User examples
- Benchmarking
- .msk file handling, and more friendly handling of NO_DATA values in general through `MaskedGeoTIFFReader`
- BigTIFF is working well, but needs some tests to cover it. 
- Also some tests for cases where precision is important.

Post initial release:

- Writing, particularly to COG format with overviews
- GeotiffAzureClient
- JPEG compression and decompression
- More spatial operation support - currently only `ReadValueAtCoordinate` is supported
- Support multi-threading/ parallel?

## Compliance tests

The Compliance tests are a set of that compare the read tag and pixel read values between geotiff.js and geotiff.net. The tifs are not kept under version control, but are downloaded from [OSGeo's website](https://download.osgeo.org/geotiff/samples/) which is a good sample set to test against.
