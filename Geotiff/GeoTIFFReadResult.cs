using Geotiff.Exceptions;

namespace Geotiff;

public class GeoTIFFReadResult(IEnumerable<GeoTiffSampleReadResult> sampleData, uint width, uint height, GeoTiffImage parentImage)
{
    public uint Height { get; set; } = height;
    public uint Width { get; set; } = width;
    /// <summary>
    /// A list of arrays of T. Each array represents a single sample.
    /// </summary>
    public IEnumerable<T[]> SampleData { get; set; } = sampleData;
    private readonly GeoTiffImage ParentImage = parentImage;

    public GeoTiffSampleReadResult<T> GetSampleResultAt(int sampleIndex)
    {
        return new GeoTiffSampleReadResult<T>(this.SampleData.ElementAt(sampleIndex), width, height, parentImage);
    }
}

public class GeoTiffSampleReadResult
{

    public GeoTiffSampleReadResult(uint width, uint height, GeoTiffImage parentImage)
    {
        
    }
    
    public static GeoTiffSampleReadResult FromDouble(double[] data, uint width, uint height, GeoTiffImage parentImage)
    {
        var result = new GeoTiffSampleReadResult(width, height, parentImage);
        result._doubleResult = data;
        return result;
    }

    public static GeoTiffSampleReadResult FromFloat(float[] data, uint width, uint height, GeoTiffImage parentImage)
    {
        var result = new GeoTiffSampleReadResult(width, height, parentImage);
        result._floatResult = data;
        return result;
    }
    
    public static GeoTiffSampleReadResult FromInt(int[] data, uint width, uint height, GeoTiffImage parentImage)
    {
        var result = new GeoTiffSampleReadResult(width, height, parentImage);
        result._IntResult = data;
        return result;
    }
    
    public uint Height { get; set; }
    public uint Width { get; set; }
    private double[] _doubleResult { get; set; }
    private float[] _floatResult { get; set; }
    private int[] _IntResult { get; set; }
    private long[] _Int64Result { get; set; }
    private uint[] _UInt32Result { get; set; }
    private ushort[] _uInt16Result { get; set; }
    private short[] _Int16Result { get; set; }
    private byte[] _UInt8Result { get; set; }
    private byte[] _Int8Result { get; set; }
    
    
    private readonly GeoTiffImage ParentImage;
    
    /// <summary>
    /// This rearranges the data into a 2D array, indexed by result[pixelColumn, pixelRow] (x, y)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T[,] To2DArray()
    {
        if (_doubleResult.Length != Height * Width)
        {
            throw new InvalidOperationException("RawArrayData length does not match Height * Width.");    
        }

        var result = new T[Width, Height];
        for (uint col = 0; col < Width; col++)
        {
            for (uint row = 0; row < Height; row++)
            {
                result[col, row] = _doubleResult[row * Width + col];
            }
        }
        return result;
    }
}