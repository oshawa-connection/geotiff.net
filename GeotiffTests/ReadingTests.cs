using System.Text;
using Shouldly;
using Geotiff;
using Geotiff.Exceptions;
using Geotiff.Resampling;
using System.Xml.Linq;

namespace GeotiffTests;

[TestClass]
[DoNotParallelize]
public class ReadingTests : GeoTiffTestBaseClass
{
    private string GetDataFolderPath()
    {
        // Start from the directory where the test assembly is located
        string? dir = AppContext.BaseDirectory;

        // Walk up until we find the project root (i.e., contains .csproj or known marker)
        while (!Directory.GetFiles(dir, "*.csproj").Any())
        {
            dir = Directory.GetParent(dir)?.FullName
                  ?? throw new Exception("Could not locate project root.");
        }

        // Now construct path to Data folder
        string? dataPath = Path.Combine(dir, "Data");

        if (!Directory.Exists(dataPath))
        {
            throw new DirectoryNotFoundException($"Data folder not found at {dataPath}");
        }

        return dataPath;
    }

    [TestMethod]
    public async Task LoopedReading()
    {
        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 20
        };
        var token = cts.Token;
        var range = Enumerable.Range(0, 10);
        Parallel.ForEachAsync(range,options, async (_,token) =>
        {
            for (int i = 0; i < 10; i++)
            {
                string quebec = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");
                await using var fsSource = new FileStream(quebec, FileMode.Open, FileAccess.Read);
                GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        
                GeoTiffImage? image = await geotiff.GetImageAsync();
                var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
                    
            }
        });
    }
    
    
    [TestMethod]
    public async Task TestQuebec()
    {
        string quebec = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");

        await using var fsSource = new FileStream(quebec, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(14);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        VectorXYZ? origin = image.GetOrigin();
        BoundingBox? bbox = image.GetBoundingBox();
        origin.X.ShouldBe(-80);
        origin.Y.ShouldBe(63);
        bbox.XMin.ShouldBe(-80, 0.001);
        bbox.YMin.ShouldBe(44.833, 0.001);
        bbox.XMax.ShouldBe(-55.916, 0.001);
        bbox.YMax.ShouldBe(63, 0.001);

        var resolution = image.GetResolution();
        resolution.X.ShouldBe(0.08333333333333333d);
        resolution.Y.ShouldBe(-0.08333333333333333d);
        resolution.Z.ShouldBe(0d);
        
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        readResult.NumberOfSamples.ShouldBe(4);
        var doubleArray = readResult.GetSampleAt(0).GetAs2DDoubleArray();
        Console.WriteLine(doubleArray[0,0]);
    }
    
    [TestMethod]
    public async Task TestMaskedSimple()
    {
        string masked = Path.Combine(GetDataFolderPath(), "masked_image.tif");

        await using var fsSource = new FileStream(masked, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        //
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        readResult.NumberOfSamples.ShouldBe(1);
        var doubleArray = readResult.GetSampleAt(0).GetAs2DDoubleArray();
        Console.WriteLine(doubleArray[0,0]);

        // Console.WriteLine(image.GetProjectionString());
    }
    
    [TestMethod]
    public async Task TestFlorida()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "us_noaa_FL.tif");
        await using var fsSource = new FileStream(usNoaaTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        VectorXYZ? origin = image.GetOrigin();
        BoundingBox? bbox = image.GetBoundingBox();
        origin.X.ShouldBe(-88);
        origin.Y.ShouldBe(32);
        bbox.XMin.ShouldBe(-88, 0.001);
        bbox.YMin.ShouldBe(23.75, 0.001);
        bbox.XMax.ShouldBe(-79.75, 0.001);
        bbox.YMax.ShouldBe(32, 0.001);
        var nPixels = image.Height * image.Width;
    }
    
    [TestMethod]
    public async Task TestTagReading()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "us_noaa_FL.tif");
        await using var fsSource = new FileStream(usNoaaTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        VectorXYZ? origin = image.GetOrigin();
        BoundingBox? bbox = image.GetBoundingBox();
        
        image.Width.ShouldBe((uint)33);
        image.Height.ShouldBe((uint)33);
        image.BitsPerSample.ShouldAllBe(d => d == 32);
        image.GetTag("Compression").GetUShort().ShouldBe((ushort)8);
        image.GetTag("PhotometricInterpretation").GetUShort().ShouldBe((ushort)1);
        image.GetTag("ImageDescription").GetString().ShouldBe("NAD83 (EPSG:4269) to NAD83(HARN) (EPSG:4152). Converted from FL");
        image.GetTag("StripOffsets").GetUIntArray().ShouldBe(new uint[] {1094u, 4726u});
        image.GetTag("SamplesPerPixel").GetUShort().ShouldBe((ushort)2);
        image.GetTag("RowsPerStrip").GetUShort().ShouldBe((ushort)33);
        image.GetTag("StripByteCounts").GetUShortArray().ShouldBe(new ushort[] {3632, 3648});
        image.GetPlanarConfiguration().ShouldBe((ushort)2);
        image.GetTag("DateTime").GetString().ShouldBe("2019:12:28 00:00:00");
        image.GetPredictor().ShouldBe((ushort)3);
        image.GetTag("ExtraSamples").GetUShort().ShouldBe((ushort)0);
        image.GetTag("SampleFormat").GetUShortArray().ShouldBe(new ushort[] {3,3});
        image.GetTag("ModelPixelScale").GetDoubleArray().ShouldBe(new double[] {0.25, 0.25, 0});
        image.GetTag("ModelTiepoint").GetDoubleArray().ShouldBe(new double[] {0,0,0,-88,32,0});
        image.GetTag("GeoKeyDirectory").GetUShortArray().ShouldBe(new ushort[] {1,1,1,3,1024,0,1,2,1025,0,1,2,2048,0,1,4269});
        image.GetTag("GDAL_METADATA").GetString().ShouldBe("<GDALMetadata>\n  <Item name=\"area_of_use\">USA - Florida</Item>\n  <Item name=\"target_crs_epsg_code\">4152</Item>\n  <Item name=\"TYPE\">HORIZONTAL_OFFSET</Item>\n  <Item name=\"UNITTYPE\" sample=\"0\" role=\"unittype\">arc-second</Item>\n  <Item name=\"DESCRIPTION\" sample=\"0\" role=\"description\">latitude_offset</Item>\n  <Item name=\"positive_value\" sample=\"1\">east</Item>\n  <Item name=\"UNITTYPE\" sample=\"1\" role=\"unittype\">arc-second</Item>\n  <Item name=\"DESCRIPTION\" sample=\"1\" role=\"description\">longitude_offset</Item>\n</GDALMetadata>\n");
        
        origin.X.ShouldBe(-88);
        origin.Y.ShouldBe(32);
        bbox.XMin.ShouldBe(-88, 0.001);
        bbox.YMin.ShouldBe(23.75, 0.001);
        bbox.XMax.ShouldBe(-79.75, 0.001);
        bbox.YMax.ShouldBe(32, 0.001);
        
        // Now test that users are able to cast these values if they don't mind what type it is too much 
        image.GetTag("Compression").GetAsInt().ShouldBe(8);
        image.GetTag("PhotometricInterpretation").GetAsInt().ShouldBe(1);
        image.GetTag("ImageDescription").GetString().ShouldBe("NAD83 (EPSG:4269) to NAD83(HARN) (EPSG:4152). Converted from FL");
        image.GetTag("StripOffsets").GetAsIntArray().ShouldBe(new int[] {1094, 4726});
        image.GetTag("SamplesPerPixel").GetAsInt().ShouldBe(2);
        image.GetTag("RowsPerStrip").GetAsInt().ShouldBe(33);
        image.GetTag("StripByteCounts").GetAsIntArray().ShouldBe(new int[] {3632, 3648});
        image.GetPlanarConfiguration().ShouldBe((ushort)2);
        
        image.GetPredictor().ShouldBe((ushort)3);
        image.GetTag("ExtraSamples").GetAsInt().ShouldBe(0);
        image.GetTag("SampleFormat").GetAsIntArray().ShouldBe(new int[] {3,3});
        image.GetTag("ModelPixelScale").GetAsDoubleArray().ShouldBe(new double[] {0.25, 0.25, 0});
        image.GetTag("ModelTiepoint").GetAsDoubleArray().ShouldBe(new double[] {0,0,0,-88,32,0});
        image.GetTag("GeoKeyDirectory").GetAsIntArray().ShouldBe(new int[] {1,1,1,3,1024,0,1,2,1025,0,1,2,2048,0,1,4269});
        
        image.GetTag("GDAL_METADATA").GetString().ShouldBe("<GDALMetadata>\n  <Item name=\"area_of_use\">USA - Florida</Item>\n  <Item name=\"target_crs_epsg_code\">4152</Item>\n  <Item name=\"TYPE\">HORIZONTAL_OFFSET</Item>\n  <Item name=\"UNITTYPE\" sample=\"0\" role=\"unittype\">arc-second</Item>\n  <Item name=\"DESCRIPTION\" sample=\"0\" role=\"description\">latitude_offset</Item>\n  <Item name=\"positive_value\" sample=\"1\">east</Item>\n  <Item name=\"UNITTYPE\" sample=\"1\" role=\"unittype\">arc-second</Item>\n  <Item name=\"DESCRIPTION\" sample=\"1\" role=\"description\">longitude_offset</Item>\n</GDALMetadata>\n");
        image.GetTag("DateTime").GetString().ShouldBe("2019:12:28 00:00:00");
        image.GetTag("ImageDescription").GetString().ShouldBe("NAD83 (EPSG:4269) to NAD83(HARN) (EPSG:4152). Converted from FL");
        var allTags = image.GetAllKnownTags();
        allTags.Count().ShouldBe(20);
        
        var rawTags = image.GetAllRawTags();
        rawTags.Count().ShouldBe(20);
    }

    [TestMethod]
    public async Task GDALMetadata()
    {
        string customGDALMetadataTag = Path.Combine(GetDataFolderPath(), "custom_gdal_metadata_writing.tif");
        await using var fsSource = new FileStream(customGDALMetadataTag, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var gdalMetadataTag = image.GetTag("GDAL_METADATA");
        
        gdalMetadataTag.DataType.ShouldBe(TagDataType.ASCII);
        var s = gdalMetadataTag.GetString();
        
        XDocument doc = XDocument.Parse(s);
        
        doc.Descendants("Item").Count().ShouldBe(2);
        doc.Descendants("Item").First().FirstAttribute.Value.ShouldBe("DESCRIPTION");
        doc.Descendants("Item").First().Value.ShouldBe("HELLO WORLD");
        
        doc.Descendants("Item").Skip(1).First().FirstAttribute.Value.ShouldBe("string_tag");
        doc.Descendants("Item").Skip(1).First().Value.ShouldBe("This is a custom tag value");
    }
    
    
    [TestMethod]
    public async Task CustomTagReading()
    {
        string customTagsTiff = Path.Combine(GetDataFolderPath(), "custom_tag.tif");
        await using var fsSource = new FileStream(customTagsTiff, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        
        var knownTags = image.GetAllKnownTags();
        var rawTags = image.GetAllRawTags();
        
        knownTags.Count().ShouldBe(18);
        rawTags.Count().ShouldBe(19, "Should contain one more tag because there is an unrecognized tag that we're still able to parse");
        
        image.GetTag(65000).GetString().ShouldBe("hello world");
    }
    
    
    [TestMethod]
    public async Task TestPackBitsDecompression()
    {
        string packbits = Path.Combine(GetDataFolderPath(), "packbits.tif");
        await using var fsSource = new FileStream(packbits, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        VectorXYZ? origin = image.GetOrigin();
        BoundingBox? bbox = image.GetBoundingBox();
        
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var sample0 = readResult.GetSampleAt(0);
        sample0.GetByteArray()[0].ShouldBe((byte)0);
        sample0.GetByteArray().Last().ShouldBe((byte)99);
    }
    

    [TestMethod]
    public async Task TestRawTiffNoCompression()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "no_compression.tif");
        await using var fsSource = new FileStream(usNoaaTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        VectorXYZ? origin = image.GetOrigin();
        BoundingBox? bbox = image.GetBoundingBox();

        var nPixels = image.Height * image.Width;

        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
    }

    /// <summary>
    /// Test that we read all the bands and the correct number of them when
    /// there are multiple, and read in the correct order.
    /// </summary>
    [TestMethod]
    public async Task TestMultiBand()
    {
        string multiBand = Path.Combine(GetDataFolderPath(), "ten_band_2x2.tif");
        await using var fsSource = new FileStream(multiBand, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRasterAsync();
        resultAll.NumberOfSamples.ShouldBe(10);
        for (int i = 0; i < resultAll.NumberOfSamples; i++)
        {
            var sample = resultAll.GetSampleAt(i);
            var ints = sample.GetByteArray();
            ints.ShouldAllBe(d => d == i + 1);
        }
    }
    
    
    /// <summary>
    /// Test that a user can select the samples that they want during reading
    /// </summary>
    [TestMethod]
    public async Task TestMultiBandSampleSelection()
    {
        string multiBand = Path.Combine(GetDataFolderPath(), "ten_band_2x2.tif");
        await using var fsSource = new FileStream(multiBand, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRasterAsync(null, new [] {5,6});
        resultAll.NumberOfSamples.ShouldBe(2);

        int i = 0;
        
        foreach(var sampleIndex in resultAll.ListSampleIndices())
        {
            var sample = resultAll.GetSampleAt(sampleIndex);
            var ints = sample.GetByteArray();
            ints.ShouldAllBe(d => d == sampleIndex + 1);
            i++;
        }
    }
    
    
    [TestMethod]
    public async Task Test2DReshaping()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync();
        var reshaped = readResult.GetSampleAt(0).GetAs2DDoubleArray();
        reshaped[0,0].ShouldBe(49);
        reshaped[0,1].ShouldBe(49);
        reshaped[1,1].ShouldBe(48);

    }
    
    [TestMethod]
    public async Task TestWindowedReading()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        var bbox = image.GetBoundingBox();
        var resolution = image.GetResolution();
        bbox.XMin += resolution.X;
        bbox.YMin -= resolution.Y;
        bbox.XMax -= resolution.X;
        bbox.YMax += resolution.Y;
        
        var imagePixelWindow = image.BoundingBoxToPixelWindow(bbox);
        var height = image.Height;
        var width = image.Width;
        
        var readResult = await image.ReadRasterAsync(imagePixelWindow);
        var xSample = readResult.GetSampleAt(1).Get2DIntArray();
        var ySample = readResult.GetSampleAt(0).Get2DIntArray();
        
        for (int lon = (int)imagePixelWindow.Left - 1; lon < imagePixelWindow.Right - 1; lon++)
        {
            for (int lat = (int)imagePixelWindow.Top - 1; lat < imagePixelWindow.Bottom - 1; lat++)
            {
                var x = xSample[lon, lat];
                var y = (double)ySample[lon, lat];
                var shouldBeLat = height - (ulong)lat - 1 + resolution.Y;
                var shouldBeLon = lon + 1;
                
                x.ShouldBe(shouldBeLon);
                y.ShouldBe(shouldBeLat);
            }
        }
    }
    
    [TestMethod]
    public async Task TestWindowedReadingGeographic()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        var bbox = image.GetBoundingBox();
        var resolution = image.GetResolution();
        bbox.XMin += resolution.X;
        bbox.YMin -= resolution.Y;
        bbox.XMax -= resolution.X;
        bbox.YMax += resolution.Y;
        
        
        var height = image.Height;
        var width = image.Width;
        
        var readResult = await image.ReadRasterBoundingBoxAsync(bbox);
        var xSample = readResult.GetSampleAt(1).Get2DIntArray();
        var ySample = readResult.GetSampleAt(0).Get2DIntArray();
        
        for (int lon = (int)bbox.XMin - 1; lon < bbox.XMax - 1; lon++)
        {
            for (int lat = (int)bbox.YMax - 1; lat < bbox.YMin - 1; lat++)
            {
                var x = xSample[lon, lat];
                var y = (double)ySample[lon, lat];
                var shouldBeLat = height - (ulong)lat - 1 + resolution.Y;
                var shouldBeLon = lon + 1;
                
                x.ShouldBe(shouldBeLon);
                y.ShouldBe(shouldBeLat);
            }
        }
    }

    [TestMethod]
    public async Task BigLoop()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "int32_2band.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        
        for (var i = 0; i < 100; i++)
        {
            var resultAll = await image.ReadRasterAsync();
            var ones = resultAll.GetSampleAt(0);
            var twos = resultAll.GetSampleAt(1);
            var badIndex = ones.GetIntArray().ToList().FindIndex(d => d == 0);
            ones.GetIntArray().ShouldAllBe(d => d == 1);
            twos.GetIntArray().ShouldAllBe(d => d == 2);
        }
    }

    
    [TestMethod]
    public async Task TestReadEvenOdd()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "two_band_even_odd_int32.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        GeoTiffImage? image = await geotiff.GetImageAsync();
        for (var i = 0; i < 1000; i++)
        {
            for (int lon = 0; lon < 50; lon++)
            {
                for (int lat = 0; lat < 50; lat++)
                {
                    
                    // Flip Y because raster origin is top-left
                    int rowFromTop = 49 - lat;
        
                    int linearIndex = rowFromTop * 50 + lon;
        
                    int expectedOdd  = 2 * linearIndex + 1; // band 0
                    int expectedEven = 2 * linearIndex + 2; // band 1
                    
                    // add 0.5 to be in the centre of the pixel.
                    Raster
                        result = await image.ReadPixelSamplesAtCoordinateAsync(lon + 0.5,
                            lat + 0.5); 
                    
                    RasterSample xSample = result.GetSampleAt(1);
                    RasterSample ySample =  result.GetSampleAt(0);
                    
                    
                    var x = xSample.GetIntArray()[0];
                    var y = ySample.GetIntArray()[0];
        
                    y.ShouldBe(expectedOdd);
                    x.ShouldBe(expectedEven);
        
        
                }
            }
        }
    }
    
    [TestMethod]
    public async Task TestReadAtLonLat()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRasterAsync();
        for (var i = 0; i < 1000; i++)
        {
            for (int lon = 0; lon < 50; lon++)
            {
                for (int lat = 0; lat < 50; lat++)
                {
                    Raster
                        result = await image.ReadPixelSamplesAtCoordinateAsync(lon + 0.5,
                            lat + 0.5); // add 0.5 to be in the centre of the pixel.
                    
                    RasterSample xSample = result.GetSampleAt(1);
                    RasterSample ySample =  result.GetSampleAt(0);
                    
                    var x = xSample.GetIntArray()[0];
                    var y = ySample.GetIntArray()[0];

                    if (lat != y || lon != x)
                    {
                        Console.WriteLine($"LAT was {lat} rLAT {y}. LON: {lon} rLON {x}");    
                    }
                    
                    x.ShouldBe(lon);
                    y.ShouldBe(lat);
                }
            }
        }
    }
    
    [TestMethod]
    public async Task TwoBitPie()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRasterAsync();
        for (var i = 0; i < 100_000; i++)
        {
            Raster
                result = await image.ReadPixelSamplesAtCoordinateAsync(17 + 0.5,
                    0 + 0.5); // add 0.5 to be in the centre of the pixel.
            
            RasterSample xSample = result.GetSampleAt(1);
            RasterSample ySample =  result.GetSampleAt(0);
            
            var x = xSample.GetIntArray()[0];
            var y = ySample.GetIntArray()[0];
            
            Console.WriteLine($"LAT was 10 rLAT {y}. LON: 10 rLON {x}");
            x.ShouldBe(17);
            y.ShouldBe(0);
                
        }
    }
    

    [TestMethod]
    public async Task TestReadMultiBand()
    {
        // string multiBandTif = Path.Combine(GetDataFolderPath(), "bands_100.tif");
        // await using var fsSource = new FileStream(multiBandTif, FileMode.Open, FileAccess.Read);
        // GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        // int count = await geotiff.GetImageCountAsync();
        // count.ShouldBe(1);
        // GeoTiffImage? image = await geotiff.GetImageAsync();
        //
        // var samples = image.GetSamplesPerPixel();
        // samples.ShouldBe(100UL);
        // var readResult = await image.ReadRastersAsync<int>(cancellationToken: cts.Token);
        // for (int bandIndex = 2; bandIndex < 100; bandIndex++)
        // {
        //     var sample = readResult.GetSampleResultAt(bandIndex);
        //     sample._doubleData[10].ShouldBe(bandIndex + 1);
        // }
    }
    


    [TestMethod]
    public async Task TestSPCS27()
    {
        // string lonLatTif = Path.Combine(GetDataFolderPath(), "spcs27.tif");
        // await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        // GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        // int count = await geotiff.GetImageCountAsync();
        // GeoTiffImage? image = await geotiff.GetImageAsync();
        // var readResult = await image.ReadRastersAsync<byte>();
        

    }


    [TestMethod]
    public async Task InternalOverviews()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "internal_overviews.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(4); // tiff has 4 images, 3 of which are pyramids
        GeoTiffImage? image = await geotiff.GetImageAsync(0);

        var result = await geotiff.HasOverviewsAsync();
        result.ShouldBe(true, "Has overviews");


        string multiDatasetImage = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");
        await using var fsSource2 = new FileStream(multiDatasetImage, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff2 = await GeoTiff.FromStreamAsync(fsSource2);
        int count2 = await geotiff2.GetImageCountAsync();
        count2.ShouldBe(14); // tiff has 4 images, 3 of which are pyramids
        
        var result2 = await geotiff2.HasOverviewsAsync();
        result2.ShouldBe(false, "Has subdatasets, but they are not ordered properly to be considered overviews"); 
        
        
        string singleDatasetImage = Path.Combine(GetDataFolderPath(), "spcs27.tif");
        await using var fsSource3 = new FileStream(singleDatasetImage, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff3 = await GeoTiff.FromStreamAsync(fsSource3);
        int count3 = await geotiff3.GetImageCountAsync();
        count3.ShouldBe(1); // tiff has 4 images, 3 of which are pyramids
        
        var result3 = await geotiff3.HasOverviewsAsync();
        result3.ShouldBe(false,"Has only one subdataset");
    }

    /// <summary>
    /// Make sure stream seeking etc works ok
    /// </summary>
    [TestMethod]
    public async Task RepeatedReadsFromDisk()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "internal_overviews.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(4);
        GeoTiffImage? image = await geotiff.GetImageAsync(0);
        var readResult1 = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var readResult2 = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var readResult3 = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var readResult4 = await image.ReadRasterAsync(cancellationToken: cts.Token);
        
        // readResult1.GetSampleResultAt(0)._doubleData[0].ShouldBe(readResult4.GetSampleResultAt(0)._doubleData[0]);
    }


    [TestMethod]
    public async Task OverviewReader()
    {
        // TODO: would be nice to have this in the main code someplace.
        string externalOverviewTifPath = Path.Combine(GetDataFolderPath(), "external_overviews.tif");
        var ovrFilePath = externalOverviewTifPath + ".ovr";
        
        if (File.Exists(ovrFilePath) is false)
        {
            throw new FileNotFoundException($"No file .ovr file found at {ovrFilePath}");
        }

        await using var mainStream = File.OpenRead(externalOverviewTifPath);
        await using var ovrStream = File.OpenRead(ovrFilePath);

        var overviewMultiTiff = await MultiGeoTiff.FromStreams(mainStream, new[] { ovrStream });

        var imageCount = await overviewMultiTiff.GetImageCountAsync();
        imageCount.ShouldBe(3, "1 main, 2 from overview");

        var hasOverviews = await overviewMultiTiff.HasOverviewsAsync();
        hasOverviews.ShouldBe(true);
    }

    [TestMethod]
    public async Task MaskedMultiTiffReader()
    {
        // TODO: would be nice to have this in the main code someplace.
        string externalOverviewTifPath = Path.Combine(GetDataFolderPath(), "masked_image.tif");
        var mskFilePath = externalOverviewTifPath + ".msk";
        
        if (File.Exists(mskFilePath) is false)
        {
            throw new FileNotFoundException($"No file .msk file found at {mskFilePath}");
        }
        
        await using var mainStream = File.OpenRead(externalOverviewTifPath);
        await using var ovrStream = File.OpenRead(mskFilePath);

        var maskedMultiTiff = await MultiGeoTiff.FromStreams(mainStream, new[] { ovrStream });
        var maskedReader = await MaskedGeoTiffReader.FromMultiGeoTiff(maskedMultiTiff);
        var maskedReadResult = await maskedReader.ReadMaskedRasters<int>();
        var sample1 = maskedReadResult.GetSampleResultAt(0);
        var maskedValue = sample1.MaskedValues.ElementAt(0);
        maskedValue.Masked.ShouldBe(true);
        maskedValue.Value.ShouldBe(10);
    }

    [TestMethod]
    public async Task BiLinearResample()
    {
        string resampleTestTif = Path.Combine(GetDataFolderPath(), "resampleTest.tif");
        await using var stream = File.OpenRead(resampleTestTif);
        
        GeoTiff geotiff = await GeoTiff.FromStreamAsync(stream);
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync();
        var firstSampleOriginal = readResult.GetSampleAt(0);
        var firstSampleData= firstSampleOriginal.Get2DDoubleArray(); // A 5 * 5 array
        RasterResamplerBaseClass resamplerBaseClass = new BiLinearRasterResampler();
        var resampledResult = resamplerBaseClass.Resample(readResult, 3, 3);
        
        var first = resampledResult.GetSampleAt(0);
        var final = first.Get2DDoubleArray(); // A 3 * 3 array
        final[1,1].ShouldBe(31.888888888888893);
    }
    
    [TestMethod]
    public async Task NearestNeighbourResamplingRes()
    {
        string resampleTestTif = Path.Combine(GetDataFolderPath(), "resampleTest.tif");
        await using var stream = File.OpenRead(resampleTestTif);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(stream);
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync();
        ((int)readResult.Height).ShouldBe(5); // This is just verifying the test data hasn't changed
        ((int)readResult.Width).ShouldBe(5);
        var originalRes = readResult.GetResolution();
        
        
        RasterResamplerBaseClass resamplerBaseClass = new NearestNeighbourRasterResampler();
        var resampledResult = resamplerBaseClass.Resample(readResult, 3, 3);
        ((int)resampledResult.Height).ShouldBe(3);
        ((int)resampledResult.Width).ShouldBe(3);
        
        var resampledRes = resampledResult.GetResolution();
        resampledRes.X.ShouldBe(1.6666666666666667d);
        resampledRes.Y.ShouldBe(-1.6666666666666667d);
        resampledRes.Z.ShouldBe(0);
        
        
        var first = resampledResult.GetSampleAt(0);
        var final = first.Get2DDoubleArray();
        final[1,1].ShouldBe(55);
    }


    [TestMethod]
    public async Task PlanarConfiguration2()
    {
        string resampleTestTif = Path.Combine(GetDataFolderPath(), "two_band_planar_separate.tif");
        await using var stream = File.OpenRead(resampleTestTif);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(stream);
        
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync();
        var firstSampleOriginal = readResult.GetSampleAt(0);

        var firstSample = readResult.GetSampleAt(0).Get2DUShortArray();
        var secondSample = readResult.GetSampleAt(1).Get2DUShortArray();
        
    }

    [TestMethod]
    public async Task TestModelTransformationTag()
    {
        string transform = Path.Combine(GetDataFolderPath(), "model_transform.tif");
        await using var stream = File.OpenRead(transform);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(stream);
        
        var image = await geotiff.GetImageAsync();

        var x = image.GetResolution();
        x.X.ShouldBe(1);
        x.Y.ShouldBe(-1);
        var bbox = image.GetBoundingBox();
        bbox.XMax.ShouldBe(50);
        bbox.YMax.ShouldBe(50);
        bbox.XMin.ShouldBe(0);
        bbox.YMin.ShouldBe(0);

        var origin = image.GetOrigin();
        origin.X.ShouldBe(0);
        origin.Y.ShouldBe(50);

        var affine = image.GetOrCalculateAffineTransformation();
        affine.b.ShouldBe(0,"No rotation");
        affine.e.ShouldBe(0,"No rotation");

    }

    [TestMethod]
    public async Task TestNoAffineTiffBehavior()
    {
        string noAffineTif = Path.Combine(GetDataFolderPath(), "no_affine.tif");
        await using var fsSource = new FileStream(noAffineTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();

        // Resolution should not throw, but may return null or default
        var resolution = image.GetResolution();
        Console.WriteLine($"Resolution: {resolution}");
        resolution.ShouldBeNull();

        // Bounding box should not throw, but may return null or default
        var bbox = image.GetBoundingBox();
        Console.WriteLine($"BoundingBox: {bbox}");
        bbox.ShouldBeNull();

        // Reading a pixel value at a coordinate should not throw, but may return null or empty Raster
        Raster? result = null;
        Exception? ex = null;
        try
        {
            result = await image.ReadPixelSamplesAtCoordinateAsync(10, 10);
        }
        catch (Exception e)
        {
            ex = e;
        }
        ex.ShouldBeNull();
        result.ShouldBeNull();

        // Reading with a bounding box should not throw, but may return null or empty Raster
        Raster? bboxResult = null;
        Exception? bboxEx = null;
        try
        {
            bboxResult = await image.ReadRasterBoundingBoxAsync(bbox);
        }
        catch (Exception e)
        {
            bboxEx = e;
        }
        bboxEx.ShouldBeNull();
        bboxResult.ShouldBeNull();
        
    }
    
    
    
    [TestMethod]
    public async Task TestWindowedReadingOutOfBounds()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        var bbox = new BoundingBox() { XMin = 0, YMin = 0, XMax = 80, YMax = 80};
        var resolution = image.GetResolution();

        
        var height = image.Height;
        var width = image.Width;
        GeoTiffException? ex = null;
        try
        {
            var readResult = await image.ReadRasterBoundingBoxAsync(bbox);
        }
        catch (GeoTiffException exception)
        {
            ex = exception;
        }

        ex.ShouldNotBeNull();
    }


    [TestMethod]
    public async Task TestBigTiff()
    {
        string bigTiffPath = Path.Combine("/home/james/Documents/temp/geotiff/bigger_cog.tif");
        await using var fsSource = new FileStream(bigTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var height = image.Height;
        var width = image.Width;
        
        var tileWidth = image.GetTileWidth();
        var tileHeight = image.GetTileHeight();
        var readResult = await image.ReadRasterAsync();
    }


    [TestMethod]
    public async Task TestFloat16()
    {
        string bigTiffPath = Path.Combine(GetDataFolderPath(), "float16_10x10.tif");
        await using var fsSource = new FileStream(bigTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var height = image.Height;
        var width = image.Width;
        
        var tileWidth = image.GetTileWidth();
        var tileHeight = image.GetTileHeight();
        var readResult = await image.ReadRasterAsync();
        var firstSample = readResult.GetSampleAt(0);
        firstSample.SampleType.ShouldBe(GeotiffSampleDataType.Float16);
        firstSample.GetFloatArray().Last().ShouldBe(9.8984375f);
    }
    
        
    [TestMethod]
    public async Task TestInt16()
    {
        string bigTiffPath = Path.Combine(GetDataFolderPath(), "int16_10x10.tif");
        await using var fsSource = new FileStream(bigTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var height = image.Height;
        var width = image.Width;
        
        var tileWidth = image.GetTileWidth();
        var tileHeight = image.GetTileHeight();
        var readResult = await image.ReadRasterAsync();
        var firstSample = readResult.GetSampleAt(0);
        firstSample.SampleType.ShouldBe(GeotiffSampleDataType.Int16);
        firstSample.GetShortArray().Last().ShouldBe((short)-8121);
    }
    
    
    [TestMethod]
    public async Task TestUInt16()
    {
        string bigTiffPath = Path.Combine(GetDataFolderPath(), "uint16_10x10.tif");
        await using var fsSource = new FileStream(bigTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var height = image.Height;
        var width = image.Width;
        
        var tileWidth = image.GetTileWidth();
        var tileHeight = image.GetTileHeight();
        var readResult = await image.ReadRasterAsync();
        var firstSample = readResult.GetSampleAt(0);
        firstSample.SampleType.ShouldBe(GeotiffSampleDataType.UInt16);
        firstSample.GetUShortArray().Last().ShouldBe((ushort)56582);
    }

    [TestMethod]
    public async Task TestRGBJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "test_10x10_rgb_jpeg.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var readResult = await image.ReadRasterAsync();
        readResult.NumberOfSamples.ShouldBe(3);
        var rSample = readResult.GetSampleAt(0);
        var gSample = readResult.GetSampleAt(1);
        var bSample = readResult.GetSampleAt(2);
        
        rSample.GetByteArray().ShouldAllBe(d => d == 254);
        gSample.GetByteArray().ShouldAllBe(d => d == 0);
        bSample.GetByteArray().ShouldAllBe(d => d == 0);
    }
    
    
    [TestMethod]
    public async Task TestGrayscaleJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "test_10x10_grayscale_jpeg.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();

        var readResult = await image.ReadRasterAsync();
        readResult.NumberOfSamples.ShouldBe(1);
        var rSample = readResult.GetSampleAt(0);
        
        
        rSample.GetByteArray().ShouldAllBe(d => d == 128);
    }
    
    
    [TestMethod]
    public async Task TestCMYKJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "test_10x10_cmyk_jpeg.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        
        var readResult = await image.ReadRasterAsync();
        readResult.NumberOfSamples.ShouldBe(4);
        var cyanSample = readResult.GetSampleAt(0);
        var magentaSample = readResult.GetSampleAt(1);
        var yellowSample = readResult.GetSampleAt(2);
        var blackSample = readResult.GetSampleAt(3);
        
        cyanSample.GetByteArray().ShouldAllBe(d => d == 255);
        magentaSample.GetByteArray().ShouldAllBe(d => d == 0);
        yellowSample.GetByteArray().ShouldAllBe(d => d == 0);
        blackSample.GetByteArray().ShouldAllBe(d => d == 0);
    }
    
    
    [TestMethod]
    public async Task TestEckert()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "eckert4.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        
        var readResult = await image.ReadRasterAsync(new ImagePixelWindow() {Bottom = 1, Left = 0, Right = 1, Top = 0});
        readResult.NumberOfSamples.ShouldBe(1);
        var cyanSample = readResult.GetSampleAt(0);

        
        cyanSample.GetByteArray().ShouldAllBe(d => d == 40);

    }
}