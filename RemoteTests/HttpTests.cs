using Geotiff;
using Geotiff.RemoteClients;
using Shouldly;

namespace RemoteTests;

[TestClass]
public class HttpTests
{
    [TestMethod]
    public async Task TestServerResponsePartial()
    {
        using var client = new HttpClient();
        string baseURL = "http://localhost:8002/TCI.tif";
        var httpClient = new GeotiffHTTPClient(baseURL, client);
        GeoTIFF? cog = await GeoTIFF.FromRemoteClientAsync(httpClient);
        
        var hasOverviews = await cog.HasOverviewsAsync();
        hasOverviews.ShouldBeTrue();
        GeoTiffImage? image = await cog.GetImageAsync();
        
        var bbox = new BoundingBox() { XMin = 585640, YMax = 1818911, XMax = 609070, YMin = 1791662 };
        var readResult = await image.ReadRasterBoundingBoxAsync(bbox);
        readResult.GetNumberOfSamples().ShouldBe(3); // RGB
        var res = readResult.GetResolution();
        readResult.Width.ShouldBe((uint)((bbox.XMax - bbox.XMin) / res.X));
        readResult.Height.ShouldBe((uint) Math.Abs(((bbox.YMax - bbox.YMin) / res.Y)) + 1);

        var sample1 = readResult.GetSampleAt(0);
        var ushorts = sample1.GetByteArray();
        Console.WriteLine("HELLO");
    }

    [TestMethod]
    public async Task TestServerResponseWithFullFile()
    {
        using var client = new HttpClient();
        string baseURL = "http://localhost:8000/TCI.tif";
        var httpClient = new GeotiffHTTPClient(baseURL, client, true);
        GeoTIFF? cog = await GeoTIFF.FromRemoteClientAsync(httpClient);
        GeoTiffImage? image = await cog.GetImageAsync();
        Console.WriteLine("HELLO WORLD");
    }

    [TestMethod]
    public async Task TestServerReturnsOnlyFirstSlice()
    {
        string baseURL = "http://localhost:8001/TCI.tif";
        using var client = new HttpClient();

        var httpClient = new GeotiffHTTPClient(baseURL, client, true);
        GeoTIFF? cog = await GeoTIFF.FromRemoteClientAsync(httpClient);
        GeoTiffImage? image = await cog.GetImageAsync();
    }
}