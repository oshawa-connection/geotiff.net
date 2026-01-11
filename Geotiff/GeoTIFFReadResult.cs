namespace Geotiff;

public class GeoTIFFReadResult<T>(IEnumerable<T[]> sampleData, uint width, uint height, GeoTiffImage parentImage) where T : struct
{
    public uint Height { get; set; } = height;
    public uint Width { get; set; } = width;
    /// <summary>
    /// A list of arrays of T. Each array represents a single sample.
    /// </summary>
    public IEnumerable<T[]> SampleData { get; set; } = sampleData;
    private readonly GeoTiffImage ParentImage = parentImage;

    public SampleReadResult<T> GetSampleResultAt(int sampleIndex)
    {
        return new SampleReadResult<T>(this.SampleData.ElementAt(sampleIndex), width, height, parentImage);
    }
}

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


public class GeoTIFFReadResultUnknownType(Array rawArrayData, uint height, uint width, GeoTiffSampleDataType sampleType)
{
    public uint Height { get; set; } = height;
    public uint Width { get; set; } = width;
    public Array RawArrayData { get; set; } = rawArrayData;
    public GeoTiffSampleDataType SampleType = sampleType;
}