using Geotiff;

namespace GeotiffTests;

[TestClass]
public class WritingTests
{
    [TestMethod]
    public async Task TestWriting()
    {
        using var ms = new MemoryStream();
        var ss = new StreamSource(ms);
        // var image = new GeoTiffImage();

        await using var f = new FileStream("/Users/jamesfleming/RiderProjects/geotiff.net/GeotiffTests/Data/tiny_4x4_zstd_float.tif", FileMode.Open, FileAccess.ReadWrite);
        var geotiff = await GeoTiff.FromStreamAsync(f);
        // var geotiff = new GeoTiff(f, true, false);
        
        
        using var fsSource = new FileStream("/Users/jamesfleming/Documents/tiff/hello.tiff", FileMode.Create, FileAccess.Write);
        var writer = new GeoTiffWriter(fsSource, true, geotiff);
        await writer.Write();
    }
}