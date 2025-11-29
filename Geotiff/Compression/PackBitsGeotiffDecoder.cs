using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

public class PackBitsGeotiffDecoder : GeotiffDecoder
{
    public override IEnumerable<int> codes => new[] { 32773 };

    public override async Task<ArrayBuffer> DecodeBlock(ArrayBuffer buffer)
    {
        var dataView = new DataView(buffer);
        var outbytes = new List<byte>();

        for (int i = 0; i < buffer.Length; ++i)
        {
            byte header = dataView.getInt8(i);
            if (header < 0)
            {
                byte next = dataView.getUint8(i + 1);
                header = (byte)-(int)header; // Ignore narrowing conversion
                for (int j = 0; j <= header; ++j)
                {
                    outbytes.Add(next);
                }

                i += 1;
            }
            else
            {
                for (int j = 0; j <= header; ++j)
                {
                    outbytes.Add(dataView.getUint8(i + j + 1));
                }

                i += header + 1;
            }
        }

        return new ArrayBuffer(outbytes.ToArray());
        // return new Uint8Array(out).buffer;
    }
}