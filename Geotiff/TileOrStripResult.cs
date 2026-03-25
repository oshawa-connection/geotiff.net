using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

internal class TileOrStripResult
{
    public ulong x { get; set; }
    public ulong y { get; set; }
    public byte[] data { get; set; }
}