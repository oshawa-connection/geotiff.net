using BitMiracle.LibJpeg.Classic;

namespace Geotiff.Compression;

public class JpegGeoTiffDecoder2 : GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 7 };
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    protected override async Task<byte[]> DecodeBlockAsync(byte[] imageData, GeoTiffImage image)
    {
        using var inputStreamManipulated = new MemoryStream();
        int imageStart = 0;
        
        var jpegTables = image.JpegTables;
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
        
        jpeg_error_mgr errorManager = new jpeg_error_mgr();
        jpeg_decompress_struct cinfo = new jpeg_decompress_struct(errorManager);
        
        cinfo.jpeg_stdio_src(inputStreamManipulated);
        cinfo.jpeg_read_header(true);
        // TODO: base this off of photo interpretation, leave hard coded for now for testing.
        // cinfo.Jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
        cinfo.Out_color_space = cinfo.Jpeg_color_space;
        cinfo.Do_fancy_upsampling = false;
        cinfo.Do_block_smoothing = false;
        cinfo.Dct_method = J_DCT_METHOD.JDCT_ISLOW;
        cinfo.jpeg_start_decompress();
        
        int width = cinfo.Output_width;
        
        int components = cinfo.Num_components; // usually 3 (RGB)

        int rowStride = width * components;
        byte[] output = new byte[cinfo.Output_height * rowStride];

        // Buffer for one scanline
        byte[][] buffer = new byte[1][];
        buffer[0] = new byte[rowStride];

        int offset = 0;

        while (cinfo.Output_scanline < cinfo.Output_height)
        {
            cinfo.jpeg_read_scanlines(buffer, 1);

            Buffer.BlockCopy(buffer[0], 0, output, offset, rowStride);
            offset += rowStride;
        }

        cinfo.jpeg_finish_decompress();
        cinfo.jpeg_destroy(); // important cleanup

        return output;
        
    }
}