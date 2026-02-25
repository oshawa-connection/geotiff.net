namespace Geotiff;

/// <summary>
/// This defines coordinates in pixel space 
/// </summary>
public class ImageWindow
{
    public uint Left { get; set; }
    public uint Bottom { get; set; }
    public uint Right { get; set; }
    public uint Top { get; set; }
}