using Geotiff;

namespace RemoteTests;

[TestClass]
public class UnitTest1
{
    public string baseURL = "http://localhost:8000/TCI.tif";
    [TestMethod]
    public async Task TestMethod1()
    {
        using var client = new HttpClient();


        var cog = await GeoTIFF.FromCOGURL(client, baseURL);
        var image = await cog.GetImage();
        Console.WriteLine("HELLO WORLD");
    }
}