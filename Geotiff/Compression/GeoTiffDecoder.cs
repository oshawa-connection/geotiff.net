using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

public abstract class GeoTiffDecoder
{
    public abstract IEnumerable<int> codes { get; }
    protected abstract Task<ArrayBuffer> DecodeBlockAsync(ArrayBuffer buffer, GeoTiffImage image);

    public async Task<ArrayBuffer> Decode(ArrayBuffer buffer, GeoTiffImage image, int predictor)
    {
        var decoded = await this.DecodeBlockAsync(buffer, image);
        
        if (predictor != 1) {
            var tileWidth = image.GetTileWidth();
            var tileHeight = image.GetTileHeight();
            var bitsPerSample = image.GetBitsPerSample();
            var planarConfiguration = image.GetPlanarConfiguration();
            return ApplyPredictor(decoded, (int)tileWidth, (int)tileHeight, predictor, bitsPerSample, planarConfiguration);
        }
        return decoded;
    }
    
    
    private void decodeRowAccByte(Span<byte> row, int stride) {
        var length = row.Length - stride;
        var offset = 0;
        do {
            for (var i = stride; i > 0; i--) {
                row[offset + stride] += row[offset];
                offset++;
            }

            length -= stride;
        } while (length > 0);
    }
    
    private void AddInt32Bytes(Span<byte> a, Span<byte> b)
    {
        if (a.Length != 4 || b.Length != 4)
        {
            throw new GeoTiffException("Both arrays must be 4 bytes long.");
        }
        
        byte[] result = new byte[4];
        int carry = 0;

        for (int i = 0; i < 4; i++)
        {
            int sum = a[i] + b[i] + carry;
            result[i] = (byte)(sum & 0xFF);
            carry = sum >> 8;  // either 0 or 1
        }

        result.CopyTo(a);
    }
    
    
    private void AddInt16Bytes(Span<byte> a, Span<byte> b)
    {
        if (a.Length != 2 || b.Length != 2)
        {
            throw new GeoTiffException("Both arrays must be 2 bytes long.");
        }

        int carry = 0;

        for (int i = 0; i < 2; i++)
        {
            int sum = a[i] + b[i] + carry;
            a[i] = (byte)(sum & 0xFF);
            carry = sum >> 8;  // either 0 or 1
        }
    }
    
    /// <summary>
    /// TODO: Check the implementation of this in geotiff.js with a big endian geotiff. JS UInt32Array takes the endianness of the machine (i.e. little)
    /// </summary>
    /// <param name="row"></param>
    /// <param name="stride"></param>
    private void decodeRowAccInt32(Span<byte> row, int stride) {
        var length = (row.Length / 4) - stride;
        var offset = 0;
        do {
            for (var i = stride; i > 0; i--)
            {
                var leftIntSpan = row.Slice((offset + stride) * 4, 4);
                var rightIntSpan = row.Slice(offset * 4, 4);
                AddInt32Bytes(leftIntSpan, rightIntSpan);
                offset++;
            }

            length -= stride;
        } while (length > 0);
    }
    
    private void decodeRowAccInt16(Span<byte> row, int stride) {
        var length = (row.Length / 2) - stride;
        var offset = 0;
        do {
            for (var i = stride; i > 0; i--)
            {
                var leftIntSpan = row.Slice((offset + stride) * 2, 2);
                var rightIntSpan = row.Slice(offset * 2, 2);
                AddInt16Bytes(leftIntSpan, rightIntSpan);
                offset++;
            }

            length -= stride;
        } while (length > 0);
    }

    private void decodeRowFloatingPoint(Span<byte> row,  int stride, int bytesPerSample) {
        var index = 0;
        var count = row.Length;
        var wc = count / bytesPerSample;

        while (count > stride) {
            for (var i = stride; i > 0; --i) {
                row[index + stride] += row[index];
                ++index;
            }
            count -= stride;
        }
        
        var copy = row.ToArray(); 
        for (var i = 0; i < wc; ++i) {
            for (var b = 0; b < bytesPerSample; ++b) {
                row[(bytesPerSample * i) + b] = copy[((bytesPerSample - b - 1) * wc) + i];
            }
        }
    }
    
   private ArrayBuffer ApplyPredictor(ArrayBuffer block,int tileWidth, int tileHeight, int predictor, int[] bitsPerSample, int planarConfiguration)
   {
       var width = tileWidth;
       
      if (predictor == 0 || predictor == 1) {
        return block;
      }
      
      for (var i = 0; i < bitsPerSample.Count(); ++i) {
        if (bitsPerSample[i] % 8 != 0) {
          throw new GeoTiffDecodingException("When decoding with predictor, only multiple of 8 bits are supported.");
        }
        if (bitsPerSample[i] != bitsPerSample[0]) {
          throw new GeoTiffDecodingException("When decoding with predictor, all samples must have the same size.");
        }
      }

      var bytesPerSample = bitsPerSample[0] / 8;
      var stride = planarConfiguration == 2 ? 1 : bitsPerSample.Length;

      for (var i = 0; i < tileHeight; ++i) {
        // Last strip will be truncated if height % stripHeight != 0
        if (i * stride * tileWidth * bytesPerSample >= block.Length) {
          break;
        }
        
        if (predictor == 2) { // horizontal prediction
            var rowbytes = block.SpanSlice(i * stride * width * bytesPerSample, stride * width * bytesPerSample);
          switch (bitsPerSample[0]) {
            case 8:
                decodeRowAccByte(rowbytes,stride);
              break;
            case 16:
                decodeRowAccInt16(rowbytes,stride);
              break;
            case 32:
                decodeRowAccInt32(rowbytes,stride);
              break;
            default:
              throw new GeoTiffDecodingException($"Predictor 2 not allowed with ${bitsPerSample[0]} bits per sample.");
          }
        } else if (predictor == 3) { // horizontal floating point
            var row = block.SpanSlice(i * stride * width * bytesPerSample, stride * width * bytesPerSample);
            // Todo check sample datatype; if its not a float then throw
            decodeRowFloatingPoint(row, stride, bytesPerSample);
        }
      }

      return block;
      //throw new GeoTiffDecodingException("Not supported or unrecognised predictor encountered during decoding");
   }
}