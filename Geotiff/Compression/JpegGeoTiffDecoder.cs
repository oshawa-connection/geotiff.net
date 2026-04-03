using BitMiracle.LibJpeg.Classic;

namespace Geotiff.Compression;

public class JpegGeoTiffDecoder : GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 7 };

    public bool DoFancyUpscaling = false;
    public bool DoBlockSmoothing = false;
    public J_DCT_METHOD DctMethod = J_DCT_METHOD.JDCT_ISLOW;  
    
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

        byte[] output;
        
        try
        {
            cinfo.jpeg_stdio_src(inputStreamManipulated);
            cinfo.jpeg_read_header(true);
            cinfo.Out_color_space = cinfo.Jpeg_color_space;
            cinfo.Do_fancy_upsampling = this.DoFancyUpscaling;
            cinfo.Do_block_smoothing = this.DoBlockSmoothing;
            cinfo.Dct_method = this.DctMethod;
            cinfo.jpeg_start_decompress();

            int width = cinfo.Output_width;

            int components = cinfo.Num_components;

            int rowStride = width * components;
            output = new byte[cinfo.Output_height * rowStride];

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
        }
        finally
        {
            cinfo.jpeg_destroy();    
        }
        
        return output;
    }
}