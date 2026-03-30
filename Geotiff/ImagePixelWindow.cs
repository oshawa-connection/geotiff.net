namespace Geotiff;

/// <summary>
/// An area defined in pixel space; for model space see BoundingBox 
/// </summary>
public class ImagePixelWindow
{
    public int Left { get; set; }
    public int Bottom { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
}