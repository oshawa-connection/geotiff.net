using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// This is a single sample wtihin the read raster
/// </summary>
public class RasterSample
{
    public uint Height { get; set; }
    public uint Width { get; set; }
    private double[]? DoubleResult { get; set; }
    private float[]? FloatResult { get; set; }
    private int[]? IntResult { get; set; }
    private uint[]? UInt32Result { get; set; }
    private ushort[]? UInt16Result { get; set; }
    private short[]? Int16Result { get; set; }
    private byte[]? UInt8Result { get; set; }
    private sbyte[]? Int8Result { get; set; }
    private readonly GeoTiffImage ParentImage;
    public readonly GeotiffSampleDataType SampleType;
    public RasterSample(uint width, uint height, GeoTiffImage parentImage, GeotiffSampleDataType sampleType, int size)
    {
        this.Width = width;
        this.Height = height;
        this.ParentImage = parentImage;
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
                throw new NotImplementedException();
                break;
            case GeotiffSampleDataType.Int32:
                this.IntResult = new int[size];
                break;
            case GeotiffSampleDataType.Float32:
                this.FloatResult = new float[size];
                break;
            case GeotiffSampleDataType.Double:
                this.DoubleResult = new double[size];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sampleType), sampleType, null);
        }
    }

    public bool IsFloatingPoint()
    {
        return this.SampleType == GeotiffSampleDataType.Float32 || this.SampleType == GeotiffSampleDataType.Double;
    }

    public bool IsInteger()
    {
        return !IsFloatingPoint();
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
    public void SetFloat32(float value, int index)
    {
        CheckType(GeotiffSampleDataType.Float32);
        this.FloatResult[index] = value;
    }   
    public void SetDouble(double value, int index)
    {
        CheckType(GeotiffSampleDataType.Double);
        this.DoubleResult[index] = value;
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
        CheckType(GeotiffSampleDataType.Float32);
        return this.FloatResult;
    }
    
    public float[,] Get2DFloatArray()
    {
        CheckType(GeotiffSampleDataType.Float32);
        return this.To2DArray<float>(this.FloatResult);
    }
    
    public double[] GetDoubleArray()
    {
        CheckType(GeotiffSampleDataType.Double);
        return this.DoubleResult;
    }
    
    public double[,] Get2DDoubleArray()
    {
        CheckType(GeotiffSampleDataType.Double);
        return this.To2DArray<double>(this.DoubleResult);
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
                throw new NotImplementedException();
                break;
            case GeotiffSampleDataType.Int32:
                array = this.ConvertAllToDouble(this.IntResult);
                break;
            case GeotiffSampleDataType.Float32:
                array = this.ConvertAllToDouble(this.FloatResult);
                break;
            case GeotiffSampleDataType.Double:
                array = this.DoubleResult;
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
                throw new NotImplementedException();
                break;
            case GeotiffSampleDataType.Int32:
                array = this.ConvertAllToInt(this.IntResult);
                break;
            case GeotiffSampleDataType.Float32:
                array = this.ConvertAllToInt(this.FloatResult);
                break;
            case GeotiffSampleDataType.Double:
                array = this.ConvertAllToInt(this.DoubleResult);
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
    private T[,] To2DArray<T>(T[] array)
    {
        if (array.Length != Height * Width)
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
}