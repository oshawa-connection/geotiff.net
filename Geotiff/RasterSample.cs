using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// This is a single sample wtihin the read raster
/// </summary>
public class RasterSample
{
    public ulong Height { get; set; }
    public ulong Width { get; set; }
    protected double[]? Float64Result { get; set; }
    protected float[]? Float32Result { get; set; }
    protected float[]? Float16Result { get; set; }
    protected int[]? IntResult { get; set; }
    protected uint[]? UInt32Result { get; set; }
    protected ushort[]? UInt16Result { get; set; }
    protected short[]? Int16Result { get; set; }
    protected ulong[]? UInt64Result { get; set; }
    protected long[]? Int64Result { get; set; }
    protected byte[]? UInt8Result { get; set; }
    protected sbyte[]? Int8Result { get; set; }
    protected readonly GeoTiffImage ParentImage;
    public readonly GeotiffSampleDataType SampleType;


    private RasterSample(ulong width, ulong height, GeoTiffImage parentImage)
    {
        this.Width = width;
        this.Height = height;
        this.ParentImage = parentImage;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        int[] intResult) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.Int32;
        this.IntResult = intResult;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        byte[] uInt8Result) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.UInt8;
        this.UInt8Result = uInt8Result;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        sbyte[] int8Result) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.Int8;
        this.Int8Result = int8Result;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        uint[] uIntResult) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.UInt32;
        this.UInt32Result = uIntResult;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        ushort[] uShortResult) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.UInt16;
        this.UInt16Result = uShortResult;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        short[] shortResult) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.Int16;
        this.Int16Result = shortResult;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        float[] float32Result) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.Float32;
        this.Float32Result = float32Result;
    }
    
    public RasterSample(uint width, uint height, GeoTiffImage parentImage,
        double[] float64Result) : this(width, height, parentImage)
    {
        this.SampleType = GeotiffSampleDataType.Float64;
        this.Float64Result = float64Result;
    }
    public RasterSample(ulong width, ulong height, GeoTiffImage parentImage, GeotiffSampleDataType sampleType, int size): this(width, height, parentImage)
    {
        this.SampleType = sampleType;
        switch (sampleType)
        {
            case GeotiffSampleDataType.UInt8:
                this.UInt8Result = new byte[size];
                break;
            case GeotiffSampleDataType.Int8:
                this.Int8Result = new sbyte[size];
                break;
            case GeotiffSampleDataType.Int16:
                this.Int16Result = new short[size];
                break;
            case GeotiffSampleDataType.UInt16:
                this.UInt16Result = new ushort[size];
                break;
            case GeotiffSampleDataType.UInt32:
                this.UInt32Result = new uint[size];
                break;
            case GeotiffSampleDataType.UInt64:
                this.UInt64Result = new ulong[size];
                break;
            case GeotiffSampleDataType.Int32:
                this.IntResult = new int[size];
                break;
            case GeotiffSampleDataType.Float16:
                this.Float16Result = new float[size];
                break;
            case GeotiffSampleDataType.Float32:
                this.Float32Result = new float[size];
                break;
            case GeotiffSampleDataType.Float64:
                this.Float64Result = new double[size];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sampleType), sampleType, null);
        }
    }

    public bool IsFloatingPoint
    {
        get
        {
            return this.SampleType == GeotiffSampleDataType.Float32 || this.SampleType == GeotiffSampleDataType.Float64;    
        }
    }

    public bool IsInteger
    {
        get
        {
            return !IsFloatingPoint;    
        }
    }
    
    public void CheckType(GeotiffSampleDataType sampleDataType)
    {
        if (this.SampleType != sampleDataType)
        {
            throw new GeoTiffException($"Requested sample type: {sampleDataType} does not match the read result of: {this.SampleType}");
        }
    }

    #region SetMethods
    
    public void SetUInt8(byte value, int index)
    {
        CheckType(GeotiffSampleDataType.UInt8);
        this.UInt8Result[index] = value;
    }   
    public void SetInt8(sbyte value, int index)
    {
        CheckType(GeotiffSampleDataType.Int8);
        this.Int8Result[index] = value;
    }   
    public void SetInt16(short value, int index)
    {
        CheckType(GeotiffSampleDataType.Int16);
        this.Int16Result[index] = value;
    }   
    public void SetUInt16(ushort value, int index)
    {
        CheckType(GeotiffSampleDataType.UInt16);
        this.UInt16Result[index] = value;
    }   
    public void SetUInt32(uint value, int index)
    {
        CheckType(GeotiffSampleDataType.UInt32);
        this.UInt32Result[index] = value;
    }   
    public void SetInt32(int value, int index)
    {
        CheckType(GeotiffSampleDataType.Int32);
        this.IntResult[index] = value;
    }   
    public void SetFloat16(float value, int index)
    {
        CheckType(GeotiffSampleDataType.Float16);
        this.Float16Result[index] = value;
    }   
    public void SetFloat32(float value, int index)
    {
        CheckType(GeotiffSampleDataType.Float32);
        this.Float32Result[index] = value;
    }   
    public void SetDouble(double value, int index)
    {
        CheckType(GeotiffSampleDataType.Float64);
        this.Float64Result[index] = value;
    }
    
    #endregion
    
    public byte[] GetByteArray()
    {
        CheckType(GeotiffSampleDataType.UInt8);
        return this.UInt8Result;
    }
    
    public byte[,] Get2DByteArray()
    {
        CheckType(GeotiffSampleDataType.UInt8);
        return this.To2DArray<byte>(this.UInt8Result);
    }
    
    
    public sbyte[] GetSByteArray()
    {
        CheckType(GeotiffSampleDataType.Int8);
        return this.Int8Result;
    }
    
    public sbyte[,] Get2DSByteArray()
    {
        CheckType(GeotiffSampleDataType.Int8);
        return this.To2DArray<sbyte>(this.Int8Result);
    }
    
    public uint[] GetUIntArray()
    {
        CheckType(GeotiffSampleDataType.UInt32);
        return this.UInt32Result;
    }
    
    public uint[,] Get2DUIntArray()
    {
        CheckType(GeotiffSampleDataType.UInt32);
        return this.To2DArray<uint>(this.UInt32Result);
    }
    
    
    public int[] GetIntArray()
    {
        CheckType(GeotiffSampleDataType.Int32);
        return this.IntResult;
    }
    
    public int[,] Get2DIntArray()
    {
        CheckType(GeotiffSampleDataType.Int32);
        return this.To2DArray<int>(this.IntResult);
    }

    public short[] GetShortArray()
    {
        CheckType(GeotiffSampleDataType.Int16);
        return this.Int16Result;
    }
    
    public short[,] Get2DShortArray()
    {
        CheckType(GeotiffSampleDataType.Int16);
        return this.To2DArray<short>(this.Int16Result);
    }
    
    public ushort[] GetUShortArray()
    {
        CheckType(GeotiffSampleDataType.UInt16);
        return this.UInt16Result;
    }
    
    public ushort[,] Get2DUShortArray()
    {
        CheckType(GeotiffSampleDataType.UInt16);
        return this.To2DArray<ushort>(this.UInt16Result);
    }
    
    public float[] GetFloatArray()
    {
        if (this.SampleType == GeotiffSampleDataType.Float32)
        {
            return this.Float32Result;
        };

        if (this.SampleType == GeotiffSampleDataType.Float16)
        {
            return this.Float16Result;
        }
        
        throw new GeoTiffException($"Requested sample type 'float32' or 'float16' does not match the read result of: {this.SampleType}");
    }
    
    public float[,] Get2DFloatArray()
    {
        var oneDResult = this.GetFloatArray();
        return this.To2DArray<float>(oneDResult);
    }
    
    public double[] GetDoubleArray()
    {
        CheckType(GeotiffSampleDataType.Float64);
        return this.Float64Result;
    }
    
    public double[,] Get2DDoubleArray()
    {
        CheckType(GeotiffSampleDataType.Float64);
        return this.To2DArray<double>(this.Float64Result);
    }
    
    private double[] ConvertAllToDouble<T>(IEnumerable<T> array)
    {
        return array.Select(d => Convert.ToDouble(d)).ToArray();
    }
    
    /// <summary>
    /// This converts your results to double
    /// </summary>
    /// <returns></returns>
    public double[] GetAsDoubleArray()
    {
        double[] array;
        switch (this.SampleType)
        {
            case GeotiffSampleDataType.UInt8:
                array = this.ConvertAllToDouble(this.UInt8Result);
                break;
            case GeotiffSampleDataType.Int8:
                array = this.ConvertAllToDouble(this.Int8Result);
                break;
            case GeotiffSampleDataType.Int16:
                array = this.ConvertAllToDouble(this.Int16Result);
                break;
            case GeotiffSampleDataType.UInt16:
                array = this.ConvertAllToDouble(this.UInt16Result);
                break;
            case GeotiffSampleDataType.UInt32:
                array = this.ConvertAllToDouble(this.UInt32Result);
                break;
            case GeotiffSampleDataType.UInt64:
                array = this.ConvertAllToDouble(this.UInt64Result);
                break;
            case GeotiffSampleDataType.Int64:
                array = this.ConvertAllToDouble(this.Int64Result);
                break;
            case GeotiffSampleDataType.Int32:
                array = this.ConvertAllToDouble(this.IntResult);
                break;
            case GeotiffSampleDataType.Float32:
                array = this.ConvertAllToDouble(this.Float32Result);
                break;
            case GeotiffSampleDataType.Float64:
                array = this.Float64Result;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return array;
    }

    public double[,] GetAs2DDoubleArray()
    {
        var doubles = this.GetAsDoubleArray();
        return this.To2DArray(doubles);
    }
    
    public int[,] GetAs2DIntArray()
    {
        var ints = this.GetAsIntArray();
        return this.To2DArray(ints);
    }
    
    
    private int[] ConvertAllToInt<T>(IEnumerable<T> array)
    {
        return array.Select(d => Convert.ToInt32(d)).ToArray();
    }
    
    /// <summary>
    /// This converts your results to int32
    /// </summary>
    /// <returns></returns>
    public int[] GetAsIntArray()
    {
        int[] array;
        switch (this.SampleType)
        {
            case GeotiffSampleDataType.UInt8:
                array = this.ConvertAllToInt(this.UInt8Result);
                break;
            case GeotiffSampleDataType.Int8:
                array = this.ConvertAllToInt(this.Int8Result);
                break;
            case GeotiffSampleDataType.Int16:
                array = this.ConvertAllToInt(this.Int16Result);
                break;
            case GeotiffSampleDataType.UInt16:
                array = this.ConvertAllToInt(this.UInt16Result);
                break;
            case GeotiffSampleDataType.UInt32:
                array = this.ConvertAllToInt(this.UInt32Result);
                break;
            case GeotiffSampleDataType.UInt64:
                array = this.ConvertAllToInt(this.UInt64Result);
                break;
            case GeotiffSampleDataType.Int32:
                array = this.ConvertAllToInt(this.IntResult);
                break;
            case GeotiffSampleDataType.Float32:
                array = this.ConvertAllToInt(this.Float32Result);
                break;
            case GeotiffSampleDataType.Float64:
                array = this.ConvertAllToInt(this.Float64Result);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return array;
    }
    
    /// <summary>
    /// This rearranges the data into a 2D array, indexed by result[pixelColumn, pixelRow] (x, y)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private T[,] To2DArrayReversed<T>(T[] array)
    {
        if ((ulong)array.Length != Height * Width)
        {
            throw new InvalidOperationException("RawArrayData length does not match Height * Width.");    
        }

        var result = new T[Width, Height];
        for (uint col = 0; col < Width; col++)
        {
            for (uint row = 0; row < Height; row++)
            {
                var x = array[row * Width + col];
                result[col, row] = x;
            }
        }
        return result;
    }
    
    
    /// <summary>
    /// Rearranges the data into a 2D array indexed by result[pixelRow, pixelColumn] (y, x)
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private T[,] To2DArray<T>(T[] array)
    {
        if ((ulong)array.Length != Height * Width)
        {
            throw new InvalidOperationException("RawArrayData length does not match Height * Width.");
        }

        var result = new T[Height, Width];

        for (uint row = 0; row < Height; row++)
        {
            for (uint col = 0; col < Width; col++)
            {
                result[row, col] = array[row * Width + col];
            }
        }

        return result;
    }
}