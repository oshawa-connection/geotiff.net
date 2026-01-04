using Geotiff.Primitives;

namespace Geotiff;

public class GeotiffGetValuesResult
{
    private string? _decodedAsciiResult;
    private short[]? _resultInt16;
    private sbyte[]? _resultSByte;
    private Int64[]? _resultInt64;
    private double[]? _resultFloat64;
    private float[]? _resultFloat32;
    private ulong[]? _resultUInt64;
    private uint[]? _resultUInt32;
    private int[]? _resultInt32;
    private ushort[]? _resultUInt16;
    private Rational[]? _resultRational;
    private int[]? _resultSRational;

    public bool IsInt16 => _resultInt16 is not null;
    public bool IsSByte => _resultSByte is not null;
    public bool IsInt64 => _resultInt64 is not null;
    public bool IsString => _decodedAsciiResult is not null;
    public bool IsFloat64 => _resultFloat64 is not null;
    public bool IsFloat32 => _resultFloat32 is not null;
    public bool IsUint16 => _resultUInt16 is not null;
    public bool IsUInt64 => _resultUInt64 != null;
    public bool IsUInt32 => _resultUInt32 != null;
    public bool IsInt32 => _resultInt32 != null; 
    public bool IsRational => _resultRational != null;
    public bool IsSRational => _resultSRational != null;
    
    public ulong[] ResultUInt64 =>
        _resultUInt64 ?? throw new InvalidOperationException("Result is not a UInt64 array.");

    public uint[] ResultUInt32 => _resultUInt32 ?? throw new InvalidOperationException("Result is not a UInt32 array.");

    public static GeotiffGetValuesResult FromSBytes(sbyte[] data)
    {
        return new GeotiffGetValuesResult { _resultSByte = data };
    }
    
    public static GeotiffGetValuesResult FromUInt64(ulong[] data)
    {
        return new GeotiffGetValuesResult { _resultUInt64 = data };
    }

    public static GeotiffGetValuesResult FromInt64(Int64[] data)
    {
        return new GeotiffGetValuesResult { _resultInt64 = data };
    }

    public static GeotiffGetValuesResult FromUInt16(ushort[] data)
    {
        return new GeotiffGetValuesResult { _resultUInt16 = data };
    }
    
    public static GeotiffGetValuesResult FromInt16(short[] data)
    {
        return new GeotiffGetValuesResult { _resultInt16 = data };
    }
    
    public static GeotiffGetValuesResult FromUInt32(uint[] data)
    {
        return new GeotiffGetValuesResult { _resultUInt32 = data };
    }

    public static GeotiffGetValuesResult FromInt32(int[] data)
    {
        return new GeotiffGetValuesResult() { _resultInt32 = data };
    }

    public static GeotiffGetValuesResult FromFloat32(float[] data)
    {
        return new GeotiffGetValuesResult { _resultFloat32 = data };
    }

    public static GeotiffGetValuesResult FromFloat64(double[] data)
    {
        return new GeotiffGetValuesResult { _resultFloat64 = data };
    }

    public static GeotiffGetValuesResult FromRational(Rational[] data)
    {
        return new GeotiffGetValuesResult { _resultRational = data };
    }

    public static GeotiffGetValuesResult FromSRational(int[] data)
    {
        return new GeotiffGetValuesResult { _resultSRational = data };
    }

    public static GeotiffGetValuesResult FromString(string data)
    {
        return new GeotiffGetValuesResult() { _decodedAsciiResult = data };
    }

    private Array GetList()
    {
        if (IsFloat64 is true)
        {
            return _resultFloat64;
        }

        if (IsFloat32)
        {
            return _resultFloat32;
        }

        if (IsUInt64)
        {
            return _resultUInt64;
        }

        if (IsUInt32)
        {
            return _resultUInt32;
        }

        if (IsUint16)
        {
            return _resultUInt16;
        }

        if (IsString)
        {
            return new string[] { _decodedAsciiResult };
        }

        if (IsRational)
        {
            return _resultRational;
        }

        if (IsSByte)
        {
            return _resultSByte;
        }

        if (IsSRational)
        {
            return _resultSRational;
        }

        if (IsInt64)
        {
            return _resultInt64;
        }

        if (IsInt16)
        {
            return _resultInt16;
        }

        throw new InvalidOperationException("No result array is set.");
    }

    public object GetFirstElement()
    {
        return GetList().GetValue(0);
    }

    public List<object> GetListOfElements()
    {
        var objs = new List<object>();
        foreach (object? o in GetList())
        {
            objs.Add(o);
        }

        return objs;
    }
    
    private GeotiffGetValuesResult() { }
}