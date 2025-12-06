using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

internal class TileOrStripResult
{
    public int x { get; set; }
    public int y { get; set; }
    public int sample { get; set; }
    public ArrayBuffer data { get; set; }
}