using Geotiff;
using Geotiff.RemoteClients;

namespace RemoteTests;

[TestClass]
public class HttpTests
{
    public string baseURL = "http://localhost:8000/TCI.tif";

    [TestMethod]
    public async Task TestMethod1()
    {
        using var client = new HttpClient();

        var httpClient = new GeotiffHTTPClient(baseURL, client, false);
        GeoTIFF? cog = await GeoTIFF.FromRemoteClient(httpClient);
        GeoTiffImage? image = await cog.GetImage();
        Console.WriteLine("HELLO WORLD");
    }
}