namespace Geotiff;

/// <summary>
/// An area defined in pixel space; for model space see BoundingBox
/// <remarks>
/// Bottom is the lowest row, while top is the highest row index.
/// </remarks>
/// </summary>
public class ImagePixelWindow
{
    public int MinColumn { get; set; }
    public int MinRow { get; set; }
    public int MaxColumn { get; set; }
    public int MaxRow { get; set; }

    public static ImagePixelWindow FromColumnRow(int column, int row)
    {
        return new ImagePixelWindow()
        {
            MinColumn = column, MinRow = row, MaxColumn = column + 1, MaxRow = row + 1,
        };
    }
}