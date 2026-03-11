using System.Text;
using Shouldly;
using Geotiff;
using Geotiff.Resampling;

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
                GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        
                GeoTiffImage? image = await geotiff.GetImageAsync();
                var readResult = await image.ReadRastersAsync(cancellationToken: cts.Token);
                    
            }
        });
    }
    
    
    [TestMethod]
    public async Task TestQuebec()
    {
        string quebec = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");

        await using var fsSource = new FileStream(quebec, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        //
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

        var readResult = await image.ReadRastersAsync(cancellationToken: cts.Token);
        readResult.GetNumberOfSamples().ShouldBe(4);
        var doubleArray = readResult.GetSampleAt(0).GetAs2DDoubleArray();
        Console.WriteLine(doubleArray[0,0]);
    }
    
    [TestMethod]
    public async Task TestMaskedSimple()
    {
        string masked = Path.Combine(GetDataFolderPath(), "masked_image.tif");

        await using var fsSource = new FileStream(masked, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        //
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        
        var readResult = await image.ReadRastersAsync(cancellationToken: cts.Token);
        readResult.GetNumberOfSamples().ShouldBe(1);
        var doubleArray = readResult.GetSampleAt(0).GetAs2DDoubleArray();
        Console.WriteLine(doubleArray[0,0]);

        // Console.WriteLine(image.GetProjectionString());
    }
    
    [TestMethod]
    public async Task TestFlorida()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "us_noaa_FL.tif");
        await using var fsSource = new FileStream(usNoaaTif, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
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
        uint nPixels = image.GetHeight() * image.GetWidth();
    }
    
    [TestMethod]
    public async Task TestTagReading()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "us_noaa_FL.tif");
        await using var fsSource = new FileStream(usNoaaTif, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
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
        uint nPixels = image.GetHeight() * image.GetWidth();
    }
    

    [TestMethod]
    public async Task TestRawTiffNoCompression()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "no_compression.tif");
        await using var fsSource = new FileStream(usNoaaTif, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        VectorXYZ? origin = image.GetOrigin();
        BoundingBox? bbox = image.GetBoundingBox();

        uint nPixels = image.GetHeight() * image.GetWidth();

        var readResult = await image.ReadRastersAsync(cancellationToken: cts.Token);
        // Console.WriteLine(readResult.SampleData.Count());
        // var result = await image.ReadValueAtCoordinate(-83.464, 28.542);
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRastersAsync();
        resultAll.GetNumberOfSamples().ShouldBe(10);
        for (int i = 0; i < resultAll.GetNumberOfSamples(); i++)
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRastersAsync(null, new [] {5,6});
        resultAll.GetNumberOfSamples().ShouldBe(2);

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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRastersAsync();
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        var bbox = image.GetBoundingBox();
        var resolution = image.GetResolution();
        bbox.XMin += resolution.X;
        bbox.YMin -= resolution.Y;
        bbox.XMax -= resolution.X;
        bbox.YMax += resolution.Y;
        
        var imagePixelWindow = image.BoundingBoxToImageWindow(bbox);
        var height = image.GetHeight();
        var width = image.GetWidth();
        // 
        var readResult = await image.ReadRastersAsync(imagePixelWindow);
        var xSample = readResult.GetSampleAt(1).Get2DIntArray();
        var ySample = readResult.GetSampleAt(0).Get2DIntArray();
        
        for (int lon = (int)imagePixelWindow.Left - 1; lon < imagePixelWindow.Right - 1; lon++)
        {
            for (int lat = (int)imagePixelWindow.Top - 1; lat < imagePixelWindow.Bottom - 1; lat++)
            {
                var x = xSample[lon, lat];
                var y = (double)ySample[lon, lat];
                var shouldBeLat = height - lat - 1 + resolution.Y;
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        
        for (var i = 0; i < 100; i++)
        {
            var resultAll = await image.ReadRastersAsync();
            var ones = resultAll.GetSampleAt(0);
            var twos = resultAll.GetSampleAt(1);
            ones.GetIntArray().ShouldAllBe(d => d == 1);
            twos.GetIntArray().ShouldAllBe(d => d == 2);
        }
    }

    
    [TestMethod]
    public async Task TestReadEvenOdd()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "two_band_even_odd_int32.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
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
                        result = await image.ReadValueAtCoordinateAsync(lon + 0.5,
                            lat + 0.5,null, expectedOdd, expectedEven); 
                    
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRastersAsync();
        for (var i = 0; i < 1000; i++)
        {
            for (int lon = 0; lon < 50; lon++)
            {
                for (int lat = 0; lat < 50; lat++)
                {
                    Raster
                        result = await image.ReadValueAtCoordinateAsync(lon + 0.5,
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);

        GeoTiffImage? image = await geotiff.GetImageAsync();
        var resultAll = await image.ReadRastersAsync();
        for (var i = 0; i < 100_000; i++)
        {
            Raster
                result = await image.ReadValueAtCoordinateAsync(17 + 0.5,
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(4); // tiff has 4 images, 3 of which are pyramids
        GeoTiffImage? image = await geotiff.GetImageAsync(0);

        var result = await geotiff.HasOverviewsAsync();
        result.ShouldBe(true, "Has overviews");


        string multiDatasetImage = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");
        await using var fsSource2 = new FileStream(multiDatasetImage, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff2 = await GeoTIFF.FromStreamAsync(fsSource2);
        int count2 = await geotiff2.GetImageCountAsync();
        count2.ShouldBe(14); // tiff has 4 images, 3 of which are pyramids
        
        var result2 = await geotiff2.HasOverviewsAsync();
        result2.ShouldBe(false, "Has subdatasets, but they are not ordered properly to be considered overviews"); 
        
        
        string singleDatasetImage = Path.Combine(GetDataFolderPath(), "spcs27.tif");
        await using var fsSource3 = new FileStream(singleDatasetImage, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff3 = await GeoTIFF.FromStreamAsync(fsSource3);
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
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(4);
        GeoTiffImage? image = await geotiff.GetImageAsync(0);
        var readResult1 = await image.ReadRastersAsync(cancellationToken: cts.Token);
        var readResult2 = await image.ReadRastersAsync(cancellationToken: cts.Token);
        var readResult3 = await image.ReadRastersAsync(cancellationToken: cts.Token);
        var readResult4 = await image.ReadRastersAsync(cancellationToken: cts.Token);
        
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

        var overviewMultiTiff = await MultiGeoTIFF.FromStreams(mainStream, new[] { ovrStream });

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

        var maskedMultiTiff = await MultiGeoTIFF.FromStreams(mainStream, new[] { ovrStream });
        var maskedReader = await MaskedGeoTIFFReader.FromMultiGeoTiff(maskedMultiTiff);
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
        
        GeoTIFF geotiff = await GeoTIFF.FromStreamAsync(stream);
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRastersAsync();
        var firstSampleOriginal = readResult.GetSampleAt(0);
        var firstSampleData= firstSampleOriginal.Get2DDoubleArray(); // A 5 * 5 array
        IRasterResampler resampler = new BiLinearRasterResampler();
        var resampledResult = resampler.Resample(readResult, 3, 3);
        
        var first = resampledResult.GetSampleAt(0);
        var final = first.Get2DDoubleArray(); // A 3 * 3 array
        final[1,1].ShouldBe(31.888888888888893);
    }
    
    
    [TestMethod]
    public async Task NearestNeighbourResampling()
    {
        string resampleTestTif = Path.Combine(GetDataFolderPath(), "resampleTest.tif");
        await using var stream = File.OpenRead(resampleTestTif);
        
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(stream);
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRastersAsync();
        var firstSampleOriginal = readResult.GetSampleAt(0);
        var firstSampleData= firstSampleOriginal.Get2DDoubleArray();
        IRasterResampler resampler = new NearestNeighbourRasterResampler();
        var resampledResult = resampler.Resample(readResult, 3, 3);
        
        var first = resampledResult.GetSampleAt(0);
        var final = first.Get2DDoubleArray();
        final[1,1].ShouldBe(55);
    }


    [TestMethod]
    public async Task PlanarConfiguration2()
    {
        string resampleTestTif = Path.Combine(GetDataFolderPath(), "two_band_planar_separate.tif");
        await using var stream = File.OpenRead(resampleTestTif);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(stream);
        
        var image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRastersAsync();
        var firstSampleOriginal = readResult.GetSampleAt(0);

        var firstSample = readResult.GetSampleAt(0).Get2DUShortArray();
        var secondSample = readResult.GetSampleAt(1).Get2DUShortArray();

        Console.WriteLine("hELLo");
    }
}