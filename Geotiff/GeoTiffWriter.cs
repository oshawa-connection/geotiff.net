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
        byte[] buffer = new byte[2];
        Span<byte> span = buffer;
        // Number of entries in IFD
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], 17);
        output.Write(buffer);
        var imageLength = Tag.FromUShort("ImageLength", 0);
        WriteTag(imageLength);
    }

    public void WriteTag(Tag tag)
    {
        byte[] buffer = new byte[12]; // Always 12 for alignment.
        Span<byte> span = buffer;
        
        // Tag id -> ushort
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], tag.RawId);
        // Tag data type -> ushort
        BinaryPrimitives.WriteUInt16LittleEndian(span[2..4], 3);
        // Count -> uint32
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..8], 1);
        // Value -> defined by Tag data type
        BinaryPrimitives.WriteUInt16LittleEndian(span[8..10], 16);
        output.Write(buffer);
    }
}