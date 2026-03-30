using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

public class PackBitsGeoTiffDecoder : GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 32773 };

    protected override async Task<byte[]> DecodeBlockAsync(byte[] buffer, GeoTiffImage image)
    {
        var dataView = new DataView(buffer);
        var outbytes = new List<byte>(); // TODO: possible to pre-allocate size here?

        for (int i = 0; i < buffer.Length; ++i)
        {
            sbyte header = dataView.GetInt8(i);
            if (header < 0)
            {
                byte next = dataView.GetUint8(i + 1);
                header = (sbyte)(header * -1);
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
                    outbytes.Add(dataView.GetUint8(i + j + 1));
                }

                i += header + 1;
            }
        }

        return outbytes.ToArray();
    }
}