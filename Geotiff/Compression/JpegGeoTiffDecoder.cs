using BitMiracle.LibJpeg;
using Geotiff.JavaScriptCompatibility;


namespace Geotiff.Compression;

public class JpegGeoTiffDecoder: GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 7 };
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override async Task<ArrayBuffer> DecodeBlockAsync(ArrayBuffer buffer, GeoTiffImage image)
    {
        using var inputStreamManipulated = new MemoryStream();
        byte[] imageData = buffer.GetAllBytes();
        int imageStart = 0;
        
        var jpegTables = image.FileDirectory.JpegTables;
        if (jpegTables is not null)
        {
            // Remove SOI (FFD8) and EOI (FFD9) from JpegTables
            int tablesStart = (jpegTables[0] == 0xFF && jpegTables[1] == 0xD8) ? 2 : 0;
            int tablesEnd = (jpegTables[^2] == 0xFF && jpegTables[^1] == 0xD9)
                ? jpegTables.Length - 2
                : jpegTables.Length;

            // Remove SOI from image data
            imageStart = (imageData[0] == 0xFF && imageData[1] == 0xD8) ? 2 : 0;
            
            // Write SOI
            inputStreamManipulated.WriteByte(0xFF);
            inputStreamManipulated.WriteByte(0xD8);
            
            // Write tables (without SOI/EOI)
            inputStreamManipulated.Write(jpegTables, tablesStart, tablesEnd - tablesStart);
        }
        
        // Write image data (without SOI)
        inputStreamManipulated.Write(imageData, imageStart, imageData.Length - imageStart);
        inputStreamManipulated.Position = 0;
        
        using var jpgImage = new JpegImage(inputStreamManipulated);
        
        if (jpgImage.ComponentsPerSample != 3)
        {
            throw new NotImplementedException("JPG encoded Tiffs are only supported if they are in RGB colorspace");
        }

        using var outStream = new MemoryStream();
        for (var i = 0; i < jpgImage.Height; i++)
        {
            var row = jpgImage.GetRow(i);
            await outStream.WriteAsync(row.ToBytes());
        }
        
        return new ArrayBuffer(outStream.ToArray());
    }
}