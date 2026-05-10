namespace Geotiff;

/// <summary>
/// Metadata about blocks (either tiles or strips)
/// </summary>
internal class GeoTiffBlockInfo
{
    public ulong MinXTile { get; set; }
    public ulong MaxXTile { get; set; }
    public ulong MinYTile { get; set; }
    public ulong MaxYTile { get; set; }
}