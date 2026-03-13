namespace Geotiff;

/// <summary>
/// An area expressed in model space. For pixel space see ImagePixelWindow
/// </summary>
public class BoundingBox
{
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
}