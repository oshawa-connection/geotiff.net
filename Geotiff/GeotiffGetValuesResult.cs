using System;
using System.Collections;
using Geotiff.Primitives;

namespace Geotiff;

public class GeotiffGetValuesResult
{
    private string? _decodedAsciiResult;
    
    private double[]? _resultFloat64;
    private float[]? _resultFloat32;
    private ulong[]? _resultUInt64;
    private uint[]? _resultUInt32;
    public ushort[]? _resultUInt16;
    private Rational[]? _resultRational;
    
    public bool IsString => _decodedAsciiResult is not null;
    public bool IsFloat64 => _resultFloat64 is not null;
    public bool IsFloat32 => _resultFloat32 is not null;
    public bool IsUint16 => _resultUInt16 is not null;
    public bool IsUInt64 => _resultUInt64 != null;
    public bool IsUInt32 => _resultUInt32 != null;
    public bool IsRational => _resultRational != null;

    public ulong[] ResultUInt64 => _resultUInt64 ?? throw new InvalidOperationException("Result is not a UInt64 array.");
    public uint[] ResultUInt32 => _resultUInt32 ?? throw new InvalidOperationException("Result is not a UInt32 array.");

    public static GeotiffGetValuesResult FromUInt64(ulong[] data) => new GeotiffGetValuesResult { _resultUInt64 = data };
    public static GeotiffGetValuesResult FromUInt16(ushort[] data) => new GeotiffGetValuesResult { _resultUInt16 = data };
    public static GeotiffGetValuesResult FromUInt32(uint[] data) => new GeotiffGetValuesResult { _resultUInt32 = data };
    public static GeotiffGetValuesResult FromFloat32(float[] data) => new GeotiffGetValuesResult { _resultFloat32 = data };
    public static GeotiffGetValuesResult FromFloat64(double[] data) => new GeotiffGetValuesResult { _resultFloat64 = data };
    public static GeotiffGetValuesResult FromRational(Rational[] data) => new GeotiffGetValuesResult { _resultRational = data };

    public static GeotiffGetValuesResult FromString(string data) =>
        new GeotiffGetValuesResult() { _decodedAsciiResult = data };

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
            return new string[] { this._decodedAsciiResult };
        }

        if (IsRational)
        {
            return this._resultRational;
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
        foreach (var o in GetList())
        {
            objs.Add(o);
        }

        return objs;
    }

    // public GeotiffGetValuesResult CopyToNewSize(int size)
    // {
    //
    // }

    private GeotiffGetValuesResult() { }

}