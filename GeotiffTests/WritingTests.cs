using Geotiff;

namespace GeotiffTests;

[TestClass]
public class WritingTests
{
    [TestMethod]
    public void TestWriting()
    {
        using var ms = new MemoryStream();
        var ss = new StreamSource(ms);
        var image = new GeoTiffImage();
        
        var geotiff = new GeoTiff(ss, true, false);
        
        
        using var fsSource = new FileStream("/Users/jamesfleming/Documents/tiff/hello.tiff", FileMode.Create, FileAccess.Write);
        var writer = new GeoTiffWriter(fsSource, true, 1);
        writer.WriteMagicHeader();
        writer.WriteIFD();
    }
}