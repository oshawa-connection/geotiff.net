using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

internal class TileOrStripResult
{
    public int x { get; set; }
    public int y { get; set; }
    public byte[] data { get; set; }
}