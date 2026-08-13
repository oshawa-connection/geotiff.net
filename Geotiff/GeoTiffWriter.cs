using System.Buffers.Binary;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Geotiff;

public class GeoTiffWriter
{
    private bool littleEndian;
    private Stream output;

    /// <summary>
    /// Need to know this ahead of time before writing starts?
    /// </summary>
    /// <returns></returns>
    private int numberOfImages;
    public GeoTiffWriter(Stream output, bool littleEndian, int numberOfImages)
    {
        this.output = output;
        this.numberOfImages = numberOfImages;
    }

    public void WriteMagicHeader()
    {
        byte[] buffer = new byte[8];
        Span<byte> span = buffer;

        span[0] = (byte)'I';
        span[1] = (byte)'I';

        BinaryPrimitives.WriteUInt16LittleEndian(span[2..4], 42);
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..8], 8);

        output.Write(buffer);
        
    }

    public void WriteIFD()
    {
        byte[] buffer = new byte[210];
        Span<byte> span = buffer;
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], 17);
        
    }

    public void WriteTag(Tag tag)
    {
        byte[] buffer = new byte[12];
        Span<byte> span = buffer;
        
        // Tag id -> ushort
        // Tag data type -> ushort
        // Count -> uint32
        // Value -> defined by Tag data type 
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], tag.RawId);
        TagFields.FieldTypeLookup.GetByValue(tag.DataType)
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], tagTy tag.DataType);
        
        tag.DataType
    }
}