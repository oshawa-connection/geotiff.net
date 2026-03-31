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

    public static ImagePixelWindow FromColumnRow(int column, int row)
    {
        return new ImagePixelWindow()
        {
            Left = column, Bottom = row + 1, Right = column + 1, Top = row,
        };
    }
}