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
        var httpClient = new GeoTiffHTTPClient(baseURL, client);
        GeoTiff? cog = await GeoTiff.FromRemoteClientAsync(httpClient);
        
        var hasOverviews = await cog.HasOverviewsAsync();
        hasOverviews.ShouldBeTrue();
        GeoTiffImage? image = await cog.GetImageAsync();
        
        var bbox = new BoundingBox() { XMin = 585640, YMax = 1818911, XMax = 609070, YMin = 1791662 };
        var readResult = await image.ReadRasterBoundingBoxAsync(bbox);
        readResult.NumberOfSamples.ShouldBe(3); // RGB
        var res = readResult.GetResolution();
        readResult.Width.ShouldBe((uint)((bbox.XMax - bbox.XMin) / res.X));
        readResult.Height.ShouldBe((uint) Math.Abs(((bbox.YMax - bbox.YMin) / res.Y)) + 1);

        var sample1 = readResult.SampleAt(0);
        var ushorts = sample1.GetByteArray();
    }

    [TestMethod]
    public async Task TestServerResponseWithFullFile()
    {
        using var client = new HttpClient();
        string baseURL = "http://localhost:8000/TCI.tif";
        var httpClient = new GeoTiffHTTPClient(baseURL, client, true);
        GeoTiff? cog = await GeoTiff.FromRemoteClientAsync(httpClient);
        GeoTiffImage? image = await cog.GetImageAsync();
    }

    [TestMethod]
    public async Task TestServerReturnsOnlyFirstSlice()
    {
        string baseURL = "http://localhost:8001/TCI.tif";
        using var client = new HttpClient();

        var httpClient = new GeoTiffHTTPClient(baseURL, client, true);
        GeoTiff? cog = await GeoTiff.FromRemoteClientAsync(httpClient);
        GeoTiffImage? image = await cog.GetImageAsync();
    }
}