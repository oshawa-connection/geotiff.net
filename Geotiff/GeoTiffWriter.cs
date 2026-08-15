using Geotiff.Exceptions;
using System.Buffers.Binary;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Geotiff;

public class GeoTiffWriter
{
    private bool littleEndian;
    private bool _bigTiff = false;
    private Stream output;

    /// <summary>
    /// Need to know this ahead of time before writing starts?
    /// </summary>
    /// <returns></returns>
    private int numberOfImages;

    private GeoTiff _geoTiff;
    public GeoTiffWriter(Stream output, bool littleEndian, GeoTiff geotiff)
    {
        this.output = output;
        this._geoTiff =  geotiff;
    }

    public async Task Write()
    {
        this.WriteMagicHeader();
        foreach (var image in await this._geoTiff.GetAllImagesAsync())
        {
            var tags = image.GetAllRawTags();
            WriteIFD(tags);
        }
    }

    private void WriteMagicHeader()
    {
        byte[] buffer = new byte[8];
        Span<byte> span = buffer;

        span[0] = (byte)'I';
        span[1] = (byte)'I';

        BinaryPrimitives.WriteUInt16LittleEndian(span[2..4], 42);
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..8], 8);

        output.Write(buffer);
        
    }

    private void WriteIFD(IEnumerable<Tag> tags)
    {
        byte[] buffer = new byte[2];
        Span<byte> span = buffer;
        // Number of entries in IFD
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], (ushort)tags.Count());
        output.Write(buffer);


        foreach (var tag in tags)
        {
            // var tag = Tag.FromUShort(TagFields.ImageLength, 0);
            WriteTag(tag);
        }
    }

    private void WriteTag(Tag tag)
    {
        byte[] buffer = new byte[12]; // Always 12 for alignment.
        Span<byte> span = buffer;
        
        // Tag id -> ushort
        BinaryPrimitives.WriteUInt16LittleEndian(span[..2], tag.RawId);
        // Tag data type -> ushort
        BinaryPrimitives.WriteUInt16LittleEndian(span[2..4], 3);
        // Count -> uint32
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..8], (uint)tag.Length);
        
        // Value -> defined by Tag data type
        switch (tag.DataType)
        {
            case TagDataType.UINT:
                BinaryPrimitives.WriteUInt32LittleEndian(span[8..12], tag.GetUInt());
                break;
            case TagDataType.USHORT:
                BinaryPrimitives.WriteUInt16LittleEndian(span[8..10], tag.GetUShort());
                break;
            case TagDataType.LONG:
                if (this._bigTiff is false)
                {
                    if (tag.GetULong() > Int32.MaxValue)
                    {
                        throw new GeoTiffException($"{tag.TagNameOrId} with value {tag.GetLong()} could not be represented as a classic tiff. Try again as a bigtiff");
                    }
                }
                BinaryPrimitives.WriteUInt32LittleEndian(span[8..12], (uint)tag.GetULong());
                break;
            default:
                break;
        }
        
        output.Write(buffer);
    }
}