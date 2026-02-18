using Geotiff.Exceptions;
using System.Collections;

namespace Geotiff;

/// <summary>
/// Data structure for rasters, used for both reading and writing.
/// </summary>
/// <param name="sampleData"></param>
/// <param name="width"></param>
/// <param name="height"></param>
/// <param name="parentImage"></param>
public class Raster
{
    public Raster(SparseList<RasterSample> sampleData, uint width, uint height, GeoTiffImage parentImage)
    {
        this.Height = height;
        this.Width = width;
        this.SampleData = sampleData;
        this.ParentImage = parentImage;
    }
    
    public uint Height { get; set; }
    public uint Width { get; set; }
    /// <summary>
    /// A SparseList of samples. Samples are indexed by their index in the
    /// parent image. E.g. if you request samples 1 and 10, elements 1 and 10 will be set in this list and the
    /// rest of the samples won't be present.
    /// </summary>
    private SparseList<RasterSample> SampleData { get; set; }

    public int GetNumberOfSamples()
    {
        return this.SampleData.Count();
    }
    
    public IEnumerable<int> ListSampleIndices()
    {
        return this.SampleData.GetIndices();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sampleIndex">This is the index within the parent GeoTiff.</param>
    /// <returns></returns>
    public RasterSample GetSampleAt(int sampleIndex)
    {
        if (this.ListSampleIndices().Contains(sampleIndex) is false)
        {
            throw new GeoTiffException($"Sample with index {sampleIndex} was not found in the Raster.");
        }
        return this.SampleData[sampleIndex];
    }

    private readonly GeoTiffImage ParentImage;
}

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
    public readonly GeotiffSampleDataType SampleSampleType;
    public RasterSample(uint width, uint height, GeoTiffImage parentImage, GeotiffSampleDataType sampleType, int size)
    {
        this.Width = width;
        this.Height = height;
        this.ParentImage = parentImage;
        this.SampleSampleType = sampleType;
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
    
    public void CheckType(GeotiffSampleDataType sampleDataType)
    {
        if (this.SampleSampleType != sampleDataType)
        {
            throw new GeoTiffException($"Requested sample type: {sampleDataType} does not match the read result of: {this.SampleSampleType}");
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

    private double[] ConvertAllToDouble<T>(IEnumerable<T> array)
    {
        return array.Select(d => Convert.ToDouble(d)).ToArray();
    }
    
    /// <summary>
    /// This converts your results to double
    /// </summary>
    /// <returns></returns>
    public double[] GetDataAsDoubleArray()
    {
        double[] array;
        switch (this.SampleSampleType)
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

    public double[,] GetDataAs2DDoubleArray()
    {
        var array = this.GetDataAsDoubleArray();
        return To2DArray(array);
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
                result[col, row] = array[row * Width + col];
            }
        }
        return result;
    }
    
    
    // public GeotiffSampleDataType SampleType
    // {
    //     get
    //     {
    //         if (this.UInt8Result is not null) return GeotiffSampleDataType.UInt8;
    //         if (this.Int8Result is not null) return GeotiffSampleDataType.Int8;
    //         if (this.Int16Result is not null) return GeotiffSampleDataType.Int16;
    //         if (this.UInt16Result is not null) return GeotiffSampleDataType.UInt16;
    //         if (this.UInt32Result is not null) return GeotiffSampleDataType.UInt32;
    //         if (this.IntResult is not null) return GeotiffSampleDataType.Int32;
    //         if (this.DoubleResult is not null) return GeotiffSampleDataType.Double;
    //         if (this.FloatResult is not null) return GeotiffSampleDataType.Float32;
    //         
    //         throw new GeoTiffException("Unrecognised sample type");
    //     }
    // }
}