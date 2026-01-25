namespace Geotiff;

public class SampleReadResult<T>(T[] flatData, uint width, uint height, GeoTiffImage parentImage)
{
    public uint Height { get; set; } = height;
    public uint Width { get; set; } = width;
    public T[] FlatData { get; set; } = flatData;
    private readonly GeoTiffImage ParentImage = parentImage;
    
    /// <summary>
    /// This rearranges the data into a 2D array, indexed by result[pixelColumn, pixelRow] (x, y)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T[,] To2DArray()
    {
        if (FlatData.Length != Height * Width)
        {
            throw new InvalidOperationException("RawArrayData length does not match Height * Width.");    
        }

        var result = new T[Width, Height];
        for (uint col = 0; col < Width; col++)
        {
            for (uint row = 0; row < Height; row++)
            {
                result[col, row] = FlatData[row * Width + col];
            }
        }
        return result;
    }
}