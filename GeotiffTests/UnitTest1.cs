using System.Text;
using Shouldly;
using Geotiff;

namespace GeotiffTests;

[TestClass]
public class UnitTest1
{
    private CancellationTokenSource cts = new();

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
    public async Task TestQuebec()
    {
        string quebec = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");

        await using var fsSource = new FileStream(quebec, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);

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

         var readResult = await image.ReadRastersAsync<float>(cancellationToken: cts.Token);
        // Console.WriteLine(readResult.Count);

        Console.WriteLine(image.GetProjectionString());
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

        var readResult = await image.ReadRastersAsync<float>(cancellationToken: cts.Token);
        Console.WriteLine(readResult.SampleData.Count());
        // var result = await image.ReadValueAtCoordinate(-83.464, 28.542);
        Console.WriteLine("HELLO");
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
        for (int lon = 0; lon < 50; lon++)
        {
            for (int lat = 0; lat < 50; lat++)
            {
                var
                    result = await image.ReadValueAtCoordinateAsync<int>(lon + 0.5,
                        lat + 0.5); // add 0.5 to be in the centre of the pixel.
                var xSample = result.GetSampleResultAt(1);
                var ySample =  result.GetSampleResultAt(0);
                
                object? x = xSample.FlatData.GetValue(0);
                object? y = ySample.FlatData.GetValue(0);
                Console.WriteLine($"LAT was {lat} rLAT {x}. LON: {lon} rLON {y}");
                x.ShouldBe(lon);
                y.ShouldBe(lat);
            }
        }
    }

    [TestMethod]
    public async Task TestReadMultiBand()
    {
        string multiBandTif = Path.Combine(GetDataFolderPath(), "bands_100.tif");
        await using var fsSource = new FileStream(multiBandTif, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        count.ShouldBe(1);
        GeoTiffImage? image = await geotiff.GetImageAsync();

        var samples = image.GetSamplesPerPixel();
        samples.ShouldBe(100UL);
        var readResult = await image.ReadRastersAsync<int>(cancellationToken: cts.Token);
        for (int bandIndex = 2; bandIndex < 100; bandIndex++)
        {
            var sample = readResult.GetSampleResultAt(bandIndex);
            sample.FlatData[10].ShouldBe(bandIndex + 1);
        }
    }
    


    [TestMethod]
    public async Task TestSPCS27()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "spcs27.tif");
        await using var fsSource = new FileStream(lonLatTif, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        int count = await geotiff.GetImageCountAsync();
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRastersAsync<byte>();
        

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
        var readResult1 = await image.ReadRastersAsync<int>(cancellationToken: cts.Token);
        var readResult2 = await image.ReadRastersAsync<int>(cancellationToken: cts.Token);
        var readResult3 = await image.ReadRastersAsync<int>(cancellationToken: cts.Token);
        var readResult4 = await image.ReadRastersAsync<int>(cancellationToken: cts.Token);
        
        readResult1.GetSampleResultAt(0).FlatData[0].ShouldBe(readResult4.GetSampleResultAt(0).FlatData[0]);
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


    public async Task MaskedMultiTiffReader()
    {
        // TODO: would be nice to have this in the main code someplace.
        string externalOverviewTifPath = Path.Combine(GetDataFolderPath(), "masked.tif");
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
}