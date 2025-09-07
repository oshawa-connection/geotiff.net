using System.Text;
using Shouldly;
using Geotiff;

namespace GeotiffTests;

[TestClass]
public class UnitTest1
{
    CancellationTokenSource cts = new CancellationTokenSource();
    string GetDataFolderPath()
    {
        // Start from the directory where the test assembly is located
        var dir = AppContext.BaseDirectory;

        // Walk up until we find the project root (i.e., contains .csproj or known marker)
        while (!Directory.GetFiles(dir, "*.csproj").Any())
        {
            dir = Directory.GetParent(dir)?.FullName
                  ?? throw new Exception("Could not locate project root.");
        }

        // Now construct path to Data folder
        var dataPath = Path.Combine(dir, "Data");

        if (!Directory.Exists(dataPath))
            throw new DirectoryNotFoundException($"Data folder not found at {dataPath}");

        return dataPath;
    }
    
    [TestMethod]
    public async Task TestQuebec()
    {
        
        string quebec = Path.Combine(GetDataFolderPath(), "ca_nrc_NA83SCRS.tif");
        
        await using var fsSource = new FileStream(quebec,FileMode.Open, FileAccess.Read);
        var geotiff = await GeoTIFF.FromStream(fsSource);
        
        var count = await geotiff.GetImageCount();
        count.ShouldBe(14);
        var image = await geotiff.GetImage();
        var origin = image.GetOrigin();
        var bbox = image.GetBoundingBox();
        origin.X.ShouldBe(-80);
        origin.Y.ShouldBe(63);
        bbox.XMin.ShouldBe(-80, 0.001);
        bbox.YMin.ShouldBe(44.833, 0.001);
        bbox.XMax.ShouldBe(-55.916, 0.001);
        bbox.YMax.ShouldBe(63, 0.001);
        
        var readResult = await image.ReadRasters(cancellationToken: cts.Token);
        // Console.WriteLine(readResult.Count);
        
        Console.WriteLine(image.GetProjectionString());
        
    }

    [TestMethod]
    public async Task TestFlorida()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "us_noaa_FL.tif");
        await using var fsSource = new FileStream(usNoaaTif,FileMode.Open, FileAccess.Read);
        var geotiff = await GeoTIFF.FromStream(fsSource);
        var count = await geotiff.GetImageCount();
        count.ShouldBe(1);
        
        var image = await geotiff.GetImage();
        var origin = image.GetOrigin();
        var bbox = image.GetBoundingBox();
        origin.X.ShouldBe(-88);
        origin.Y.ShouldBe(32);
        bbox.XMin.ShouldBe(-88, 0.001);
        bbox.YMin.ShouldBe(23.75, 0.001);
        bbox.XMax.ShouldBe(-79.75, 0.001);
        bbox.YMax.ShouldBe(32, 0.001);
        var nPixels = image.GetHeight() * image.GetWidth();
    }
    
    [TestMethod]
    public async Task TestRawTiffNoCompression()
    {
        string usNoaaTif = Path.Combine(GetDataFolderPath(), "no_compression.tif");
        await using var fsSource = new FileStream(usNoaaTif,FileMode.Open, FileAccess.Read);
        var geotiff = await GeoTIFF.FromStream(fsSource);
        var count = await geotiff.GetImageCount();
        count.ShouldBe(1);
        
        var image = await geotiff.GetImage();
        var origin = image.GetOrigin();
        var bbox = image.GetBoundingBox();
        
        var nPixels = image.GetHeight() * image.GetWidth();

        var readResult = await image.ReadRasters(cancellationToken: cts.Token);
        Console.WriteLine(readResult.Count);
        // var result = await image.ReadValueAtCoordinate(-83.464, 28.542);
        Console.WriteLine("HELLO");
    }


    [TestMethod]
    public async Task TestReadAtLonLat()
    {
        
        string lonLatTif = Path.Combine(GetDataFolderPath(), "lat_lon_grid.tif");
        await using var fsSource = new FileStream(lonLatTif,FileMode.Open, FileAccess.Read);
        var geotiff = await GeoTIFF.FromStream(fsSource);
        var count = await geotiff.GetImageCount();
        count.ShouldBe(1);
        
        var image = await geotiff.GetImage();
        for (var lon = 0; lon < 50; lon++)
        {
            for (var lat = 0; lat < 50; lat++)
            {
                    var result = await image.ReadValueAtCoordinate(lon + 0.5, lat + 0.5); // add 0.5 to be in the centre of the pixel.
                    var x = result[1].GetValue(0);
                    var y = result[0].GetValue(0);
                    x.ShouldBe(lon);
                    y.ShouldBe(lat);
            }
        }
    }
    
    
    [TestMethod]
    public async Task TestSPCS27()
    {
        string lonLatTif = Path.Combine(GetDataFolderPath(), "spcs27.tif");
        await using var fsSource = new FileStream(lonLatTif,FileMode.Open, FileAccess.Read);
        var geotiff = await GeoTIFF.FromStream(fsSource);
        var count = await geotiff.GetImageCount();
        var image =await geotiff.GetImage();
        var readResult = await image.ReadRasters();
        Console.WriteLine("HELLO");
        // count.ShouldBe(1);
        //
        // var image = await geotiff.GetImage();
        // for (var lon = 0; lon < 50; lon++)
        // {
        //     for (var lat = 0; lat < 50; lat++)
        //     {
        //         var result = await image.ReadValueAtCoordinate(lon + 0.5, lat + 0.5); // add 0.5 to be in the centre of the pixel.
        //         var x = result[1].GetValue(0);
        //         var y = result[0].GetValue(0);
        //         x.ShouldBe(lon);
        //         y.ShouldBe(lat);
        //     }
        // }
    }
}