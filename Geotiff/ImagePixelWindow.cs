namespace Geotiff;

/// <summary>
/// An area defined in pixel space; for model space see BoundingBox 
/// </summary>
public class ImagePixelWindow
{
    public uint Left { get; set; }
    public uint Bottom { get; set; }
    public uint Right { get; set; }
    public uint Top { get; set; }
}