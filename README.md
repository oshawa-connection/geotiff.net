# geotiff.net

A port of [geotiff.js](https://geotiffjs.github.io/) to .Net.

This project adds native .Net handling of geotiff files, the benefits being:
- Easier cross platform compatibility (over GDAL which requires native dependencies to be compiled on the target platform)
- Asynchronous and streamed reads + writes
- Easier debugging
- Extensibility in C# (e.g. define your own source types, decoders, sidecar file handlers)

It also opens up the .Net ecosystem to GIS developers, for example, desktop applications, ASP.Net apps and game engines. Here's a cool screenshot of a geotiff visualised in 3D using Unity using this library:

![Screenshot of some mountains rendered in 3D in Unity using geotiff.net.](https://raw.githubusercontent.com/oshawa-connection/geotiff.net/refs/heads/master/readmeAssets/Unity.png)

## Examples

Read everything as a 2D array from the first sample of the first image from a file:
```csharp
var lonLatTif = Path.Combine(GetDataFolderPath(), "elevationData.tif");
await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
GeoTIFF geotiff = await GeoTIFF.FromStream(fsSource);
GeoTIFFImage image = await geotiff.GetImage(0);
Raster readResult = await image.ReadRastersAsync();
RasterSample sample0 = readResult.GetSampleAt(0);
ushort[] result = sample0.GetUShortArray();
```

Read data within a bounding box from the first overview of a cloud optimised GeoTiff.

```csharp
using var httpclient = new HttpClient();
var baseURL = "http://localhost:8002/TCI.tif";
var geoTiffHttpClient = new GeotiffHTTPClient(baseURL, client);
GeoTIFF? cog = await GeoTIFF.FromRemoteClientAsync(geoTiffHttpClient);

var hasOverviews = await cog.HasOverviewsAsync(); // true
GeoTiffImage? firstOverview = await cog.GetImageAsync();

// Note that coordinates are in the same coordinate system as the Tiff itself.
var bbox = new BoundingBox() { XMin = 585640, YMax = 1818911, XMax = 609070, YMin = 1791662 };
var readResult = await firstOverview.ReadRasterBoundingBoxAsync(bbox);
var sample1 = readResult.GetSampleAt(0);
var ushorts = sample1.GetByteArray();
```
note that if your bounding box extends past the edge of the bounding box of the tiff, it will throw an exception. PR accepted to allow reading past the edge and padding!

Read data at the pixel at a coordinate from an AWS S3 bucket:
```csharp
using var client = new AmazonS3Client(new AmazonS3Config { ... });
var gtAWSClient = new GeotiffAWSClient("testbucket", "modelOutputDataCOG.tif", client);
GeoTIFF? geotiff = await GeoTIFF.FromRemoteClient(gtAWSClient);
var image = await geotiff.GetImage();
var result = await image.ReadValueAtCoordinate<double>(lon, lat); 
var waterVelocityAtCoordinate =  result.GetSampleResultAt(0).GetDoubleArray()[0];
var waterDepthAtCoordinate = result.GetSampleResultAt(1).GetDoubleArray()[0];
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

Read a tif file, then resample it:

```csharp
string resampleTestTif = Path.Combine(GetDataFolderPath(), "resampleTest.tif");
await using var stream = File.OpenRead(resampleTestTif);

GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(stream);
GeoTiffImage image = await geotiff.GetImageAsync();
Raster readResult = await image.ReadRastersAsync();
RasterSample firstSampleOriginal = readResult.GetSampleAt(0);
double[,] firstSampleData= firstSampleOriginal.Get2DDoubleArray();
IRasterResampler resampler = new BiLinearRasterResampler();
Raster resampledResult = resampler.Resample(readResult, 3, 3);

RasterSample first = resampledResult.GetSampleAt(0);
double[,] final = first.Get2DDoubleArray(); // Default BiLinearRasterResampler converts results to double.

```

There is also `NearestNeighbourRasterResampler` which is better suited to integer data. However, you can also implement your own resampling algorithms if these don't fit your use case. *The default resamplers do not account for masked data.*

Note that conceptually, a `Raster` is independent of the GeoTiffImage that it was read from, and so it stores its own information on its bounding box, affine transformation and resolution. So the affine transformation and resolution will change when resampling.


Read data from a file that you don't know the data type of ahead of time (e.g. a user uploads a file, or iterating over files in a folder)
```csharp
var unknownDataTypeTif = Path.Combine(GetDataFolderPath(), "mystery.tif");
await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
var geotiff = await GeoTIFF.FromStream(fsSource);
int count = await geotiff.GetImageCount();
var image = await geotiff.GetImage();
var readResult = await image.ReadRastersAsync();

var sample0 = readResult.GetSampleAt(0);

var result = sample0.GetAsDoubleArray(); // Get As -> Converts your datatype, useful if you don't mind much about the datatype.
var result = sample0.GetAsIntArray();


if (result.IsInteger()) // any signed or unsigned integer type.
{
    var result = sample0.GetAsIntArray();
}

if (result.IsFloatingPoint) // either a double of float
{
        var result = sample0.GetAsDoubleArray();
}

// Lastly, if precision is important or your data values are close to the upper/ lower limits of the storage type, you can do:
switch (sample0.SampleType)
{
    case GeotiffSampleDataType.UInt8:
        break;
    case GeotiffSampleDataType.Int8:
        break;
    case GeotiffSampleDataType.Int16:
        break;
    case GeotiffSampleDataType.UInt16:
        break;
    case GeotiffSampleDataType.UInt32:
        break;
    case GeotiffSampleDataType.UInt64:
        break;
    case GeotiffSampleDataType.Int32:
        break;
    case GeotiffSampleDataType.Float32:
        break;
    case GeotiffSampleDataType.Double:
        break;
    default:
        throw new ArgumentOutOfRangeException();
}

```

Lastly, if you want to reshape the data into a 2D array organised so that the first element is at the top left (as per geotiff convention, at its origin):

```csharp
string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
var image = await geotiff.GetImageAsync(0);
var readResult = await image.ReadRastersAsync();
var reshaped = readResult.GetSampleAt(0).GetAs2DDoubleArray();

Console.WriteLine(reshaped[0,0]); // value at the geotiff origin
```


## Alternatives libraries

Other than GDAL, there are several packages for reading (and possibly writing) geotiffs.

- [GDAL through gdal.netcore](https://github.com/MaxRev-Dev/gdal.netcore) - [Quite low level](https://github.com/OSGeo/gdal/blob/master/doc/source/api/csharp/csharp_raster.rst). Synchronous. Packages up GDAL for you so you don't have to compile it + the C# bindings yourself.
- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Does not support geotiff tags, COGs or spatial operations, synchronous.
- [Mission controller](https://github.com/ArduPilot/MissionPlanner/blob/cedabf7b610c0e54b8fe4409d903963faa69ab90/ExtLibs/Utilities/GeoTiff.cs) - Does not support COGs or spatial operations, synchronous. Tailored to the requirements of Ardupilot itself.
- [DEM.NET](https://github.com/dem-net/DEM.Net) - a wrapper around LibTiff.Net. Synchronous. Only filesystem files supported.

## Contributing

New contributors are very welcome. If you’d like to get involved, please open an early PR or start a discussion to share your ideas. Some ideas of good items to work on:

Before release, the bare minimum:

- Find and resolve all [Obsolete] attributes
- Normalisation needs testing
- JPG support
- Better tag reading API
- Merge ArrayBuffer + DataView into a new type called "GeoTiffStream"
- Ideally, remove all JavaScriptCompatibility namespace extensions and classes
- Test to cover packbits decoder
- Merge GeoTiffTagValueResult with Tag class
- Don't cast tag numeric values to double during parsing

Post initial release:

- Some tests with rasters that have rotation elements of their affine matrix.
- More friendly handling of NO_DATA values in general through `MaskedGeoTIFFReader`. This needs to handle resampling too.
- Some tests for cases where values are close to the limits of their respective types, e.g. int32.Max, float32.Max
- Writing, particularly to COG format with overviews
- Offer up a synchronous API too for legacy codebases and environments where that is preferred.
- Benchmarking
- GeotiffAzureClient
- JPEG compression and decompression
- More spatial operation support
- Support multi-threading/ parallel?
- Way to stream over all the tiles or strips if the file is tiled/ stripped. This one needs some thought; different planar configurations require different handling.

## Compliance tests

The Compliance tests are a set of that compare the read tag and pixel read values between geotiff.js and geotiff.net. The tifs are not kept under version control, but are downloaded from [OSGeo's website](https://download.osgeo.org/geotiff/samples/) which is a good sample set to test against.
