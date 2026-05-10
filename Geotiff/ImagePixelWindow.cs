namespace Geotiff;

/// <summary>
/// An area defined in pixel space; for model space see BoundingBox
/// <remarks>
/// Bottom is the lowest row, while top is the highest row index.
/// </remarks>
/// </summary>
public class ImagePixelWindow
{
    public ulong MinColumn { get; set; }
    public ulong MinRow { get; set; }
    public ulong MaxColumn { get; set; }
    public ulong MaxRow { get; set; }

    public static ImagePixelWindow FromColumnRow(int column, int row)
    {
        return new ImagePixelWindow()
        {
            MinColumn = (ulong)column, MinRow = (ulong)row, MaxColumn = (ulong)column + 1, MaxRow = (ulong)row + 1,
        };
    }

    public static ImagePixelWindow FromArray(ulong[] arr)
    {
        return new ImagePixelWindow()
        {
            MinColumn = arr[0], 
            MinRow = arr[1], 
            MaxColumn = arr[2], 
            MaxRow = arr[3]
        };
    }

    public ulong[] ToArray()
    {
        return new ulong[] { this.MinColumn,this.MinRow,this.MaxColumn,this.MaxRow }; 
    }
}