using Geotiff.JavaScriptCompatibility;
using Geotiff.Primitives;

namespace Geotiff;

public class DataSlice
{
    private readonly byte[] _arrayBuffer;
    private readonly DataView _dataView;
    private readonly int _sliceOffset;
    private readonly bool _littleEndian;
    private readonly bool _bigTiff;

    public DataSlice(byte[] arrayBuffer, int sliceOffset, bool littleEndian, bool bigTiff)
    {
        _dataView = new DataView(arrayBuffer);
        _sliceOffset = sliceOffset;
        _littleEndian = littleEndian;
        _bigTiff = bigTiff;
    }

    public long SliceOffset => _sliceOffset;

    public long SliceTop => _sliceOffset + _dataView.length;

    public bool LittleEndian => _littleEndian;

    public bool BigTiff => _bigTiff;

    public byte[] Buffer => _arrayBuffer;

    public bool Covers(int offset, int length)
    {
        return _sliceOffset <= offset && SliceTop >= offset + length;
    }

    public byte ReadByte(int offset)
    {
        return _dataView.getUint8(offset - _sliceOffset);
    }

    public float ReadFloat32(int offset)
    {
        return _dataView.getFloat32(offset - _sliceOffset, LittleEndian);
    }


    public double ReadFloat64(int offset)
    {
        return _dataView.getFloat64(offset - _sliceOffset, LittleEndian);
    }

    public Rational ReadRational(int offset)
    {
        uint numer = ReadUInt32(offset);
        uint denom = ReadUInt32(offset + 4);

        return new Rational(numer, denom);
    }

    public ushort ReadUInt16(int offset)
    {
        return _dataView.getUint16(offset - _sliceOffset, LittleEndian);
    }

    public uint ReadUInt32(int offset)
    {
        return _dataView.getUint32(offset - _sliceOffset, LittleEndian);
    }

    public ulong ReadUInt64(int offset)
    {
        // TODO: this is the way its done for JS purposes; is there no built in dotnet equivalent that's more efficient?
        // They read two 32bit uints then combine them.
        uint left = ReadUInt32(offset); 
        uint right = ReadUInt32(offset + 4);
        ulong combined;

        if (LittleEndian)
        {
            combined = (ulong)left + ((ulong)right << 32);
        }
        else
        {
            combined = ((ulong)left << 32) + right;
        }

        if (combined > long.MaxValue)
        {
            throw new InvalidOperationException(
                $"{combined} exceeds MAX_SAFE_INTEGER. " +
                "Precision may be lost. Please report if you get this message to https://github.com/geotiffjs/geotiff.js/issues");
        }

        return combined;
    }

    /// <summary>
    /// adapted from https://stackoverflow.com/a/55338384/8060591
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public long ReadInt64(int offset)
    {
        long value = 0;
        bool isNegative = (_dataView.getUint8(offset + (_littleEndian ? 7 : 0)) & 0x80) > 0;
        bool carrying = true;

        for (int i = 0; i < 8; i++)
        {
            int index = (int)(offset + (_littleEndian ? i : 7 - i));
            byte b = _dataView.getUint8(index);
            if (isNegative)
            {
                if (carrying)
                {
                    if (b != 0x00)
                    {
                        b = (byte)(~(b - 1) & 0xff);
                        carrying = false;
                    }
                }
                else
                {
                    b = (byte)(~b & 0xff);
                }
            }

            value += (long)b << (8 * i);
        }

        if (isNegative)
        {
            value = -value;
        }

        return value;
    }

    public int ReadOffset(int offset)
    {
        return _bigTiff ? (int)ReadUInt64(offset) : (int)ReadUInt32(offset);
    }

    public T[] ReadAll<T>(Func<int, T> a, int count, int offset, int fieldTypeLength)
    {
        var values = new T[count];
        for (int i = 0; i < count; ++i)
        {
            values[i] = a(offset + (i * fieldTypeLength));
        }

        return values;
    }


    public GeotiffGetValuesResult getValues(ushort fieldType, int count, int offset)
    {
        GeotiffGetValuesResult finalResult;

        int fieldTypeLength = FieldTypes.GetFieldTypeLength(fieldType);
        string? fieldTypeStr = FieldTypes.FieldTypeLookup[fieldType];

        switch (fieldTypeStr)
        {
            case FieldTypes.FLOAT:
                finalResult = GeotiffGetValuesResult.FromFloat32(ReadAll(ReadFloat32, count, offset, fieldTypeLength));
                break;
            // case FieldTypes.SBYTE:
            //     values = new Int8Array(count); 
            //     readMethod = dataSlice.readInt8;
            //     break;
            case FieldTypes.SHORT:
                finalResult = GeotiffGetValuesResult.FromUInt16(ReadAll(ReadUInt16, count, offset, fieldTypeLength));
                break;
            // case FieldTypes.SSHORT:
            //     values = new Int16Array(count); 
            //     readMethod = dataSlice.readInt16;
            //     break;
            case FieldTypes.LONG:
            case FieldTypes.IFD:
                finalResult = GeotiffGetValuesResult.FromUInt32(ReadAll(ReadUInt32, count, offset, fieldTypeLength));
                break;
            // case FieldTypes.SLONG:
            //     values = new Int32Array(count); 
            //     readMethod = dataSlice.readInt32;
            //     break;
            case FieldTypes.LONG8: 
            case FieldTypes.IFD8:
                finalResult = GeotiffGetValuesResult.FromUInt64(ReadAll(ReadUInt64, count, offset, fieldTypeLength));
                break;
            // case FieldTypes.SLONG8:
            //     values = new Array(count); 
            //     readMethod = dataSlice.readInt64;
            //     break;
            case FieldTypes.RATIONAL:
                finalResult =
                    GeotiffGetValuesResult.FromRational(ReadAll(ReadRational, count, offset, fieldTypeLength));
                // values = new Uint32Array(count * 2); 
                // readMethod = dataSlice.readUint32;
                break;
            // case FieldTypes.SRATIONAL:
            //     values = new Int32Array(count * 2); 
            //     readMethod = dataSlice.readInt32;
            //     break;

            case FieldTypes.ASCII:
            case FieldTypes.BYTE:
            case FieldTypes.UNDEFINED:
                byte[]? asciiBytes = ReadAll(ReadByte, count, offset, fieldTypeLength);
                string? decodedString = System.Text.Encoding.ASCII.GetString(asciiBytes);
                finalResult = GeotiffGetValuesResult.FromString(decodedString);
                break;
            case FieldTypes.DOUBLE:
                finalResult = GeotiffGetValuesResult.FromFloat64(ReadAll(ReadFloat64, count, offset, fieldTypeLength));
                break;
            default:
                throw new Exception($"Invalid field type: {fieldTypeStr}");
        }

        return finalResult;
        // // normal fields
        // if (!(fieldType === FieldTypes.RATIONAL || fieldType === FieldTypes.SRATIONAL)) {
        // for (let i = 0; i < count; ++i) {
        // values[i] = readMethod.call(
        // dataSlice, offset + (i * fieldTypeLength),
        // );
        // }
        // } else { // RATIONAL or SRATIONAL
        // for (let i = 0; i < count; i += 2) {
        // values[i] = readMethod.call(
        // dataSlice, offset + (i * fieldTypeLength),
        // );
        // values[i + 1] = readMethod.call(
        // dataSlice, offset + ((i * fieldTypeLength) + 4),
        // );
        // }
        // }

        // if (fieldType === FieldTypes.ASCII) {
        //     return new TextDecoder('utf-8').decode(values);
        // }
        // return values;
    }
}