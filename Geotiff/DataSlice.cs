using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;
using Rationals;

namespace Geotiff;

internal class DataSlice
{
    private readonly byte[] _arrayBuffer;
    private readonly DataView _dataView;
    private readonly ulong _sliceOffset;
    private readonly bool _littleEndian;
    private readonly bool _bigTiff;

    public DataSlice(byte[] arrayBuffer, ulong sliceOffset, bool littleEndian, bool bigTiff)
    {
        _dataView = new DataView(arrayBuffer);
        _sliceOffset = sliceOffset;
        _littleEndian = littleEndian;
        _bigTiff = bigTiff;
    }

    public ulong SliceOffset => _sliceOffset;

    public ulong SliceTop => _sliceOffset + (ulong)_dataView.Length;

    public bool LittleEndian => _littleEndian;

    public bool BigTiff => _bigTiff;

    public byte[] Buffer => _arrayBuffer;

    public bool Covers(ulong offset, ulong length)
    {
        return _sliceOffset <= offset && SliceTop >= offset + length;
    }

    public byte ReadByte(ulong offset)
    {
        return _dataView.GetUInt8((int)(offset - _sliceOffset));
    }

    public sbyte ReadSByte(ulong offset)
    {
        return _dataView.GetInt8((int)(offset - _sliceOffset));
    }

    public float ReadFloat32(ulong offset)
    {
        return _dataView.GetFloat32((int)(offset - _sliceOffset), LittleEndian);
    }


    public double ReadFloat64(ulong offset)
    {
        return _dataView.GetFloat64((int)(offset - _sliceOffset), LittleEndian);
    }

    public Rational ReadRational(ulong offset)
    {
        uint numer = ReadUInt32(offset);
        uint denom = ReadUInt32(offset + 4);

        return new Rational(numer, denom);
    }

    public ushort ReadUInt16(ulong offset)
    {
        return _dataView.GetUInt16((int)(offset - _sliceOffset), LittleEndian);
    }

    public uint ReadUInt32(ulong offset)
    {
        return _dataView.GetUInt32((int)(offset - _sliceOffset), LittleEndian);
    }

    public int ReadInt32(ulong offset)
    {
        return _dataView.GetInt32((int)(offset - _sliceOffset), LittleEndian);
    }

    public short ReadInt16(ulong offset)
    {
        return _dataView.GetInt16((int)(offset - _sliceOffset), LittleEndian);
    }
    
    public ulong ReadUInt64(ulong offset)
    {
        return _dataView.GetUInt64(
            (int)(offset - _sliceOffset),
            LittleEndian
        );
    }
    
    public long ReadInt64(ulong offset)
    {
        return _dataView.GetInt64(
            (int)(offset - _sliceOffset),
            LittleEndian
        );
    }

    public ulong ReadOffset(ulong offset)
    {
        return _bigTiff ? ReadUInt64(offset) : ReadUInt32(offset);
    }

    public T[] ReadAll<T>(Func<ulong, T> a, int count, ulong offset, int fieldTypeLength)
    {
        var values = new T[count];
        for (int i = 0; i < count; ++i)
        {
            values[i] = a(offset + (ulong)(i * fieldTypeLength));
        }

        return values;
    }


    public GeoTiffTagValueResult GetValues(ushort fieldType, int count, ulong offset)
    {
        GeoTiffTagValueResult finalResult;

        int fieldTypeLength = TagFields.GetFieldTypeLength(fieldType);
        GeotiffFieldDataType fieldTypeStr = TagFields.FieldTypeLookup.GetByKey(fieldType);

        switch (fieldTypeStr)
        {
            case GeotiffFieldDataType.BYTE:
            case GeotiffFieldDataType.UNDEFINED:
                byte[]? bytes = ReadAll(ReadByte, count, offset, fieldTypeLength);
                finalResult = GeoTiffTagValueResult.FromByte(bytes);
                break;
            case GeotiffFieldDataType.ASCII:
                byte[]? asciibytes = ReadAll(ReadByte, count, offset, fieldTypeLength);
                finalResult = GeoTiffTagValueResult.FromAscii(asciibytes);
                break;
            case GeotiffFieldDataType.SBYTE:
                finalResult = GeoTiffTagValueResult.FromSBytes(ReadAll(ReadSByte, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.SHORT:
                finalResult = GeoTiffTagValueResult.FromUInt16(ReadAll(ReadUInt16, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.SSHORT:
                finalResult = GeoTiffTagValueResult.FromInt16(ReadAll(ReadInt16, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.LONG:
            case GeotiffFieldDataType.IFD:
                finalResult = GeoTiffTagValueResult.FromUInt32(ReadAll(ReadUInt32, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.SLONG:
                finalResult = GeoTiffTagValueResult.FromInt32(ReadAll(ReadInt32, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.LONG8: 
            case GeotiffFieldDataType.IFD8:
                finalResult = GeoTiffTagValueResult.FromUInt64(ReadAll(ReadUInt64, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.SLONG8:
                finalResult = GeoTiffTagValueResult.FromInt64(ReadAll(ReadInt64, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.RATIONAL:
                finalResult =
                    GeoTiffTagValueResult.FromRational(ReadAll(ReadRational, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.SRATIONAL:
                finalResult =
                    GeoTiffTagValueResult.FromSRational(ReadAll(ReadInt32, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.FLOAT:
                finalResult = GeoTiffTagValueResult.FromFloat32(ReadAll(ReadFloat32, count, offset, fieldTypeLength));
                break;
            case GeotiffFieldDataType.DOUBLE:
                finalResult = GeoTiffTagValueResult.FromFloat64(ReadAll(ReadFloat64, count, offset, fieldTypeLength));
                break;
            default:
                throw new GeoTiffException($"Invalid field type: {fieldTypeStr}");
        }

        return finalResult;
    }
}