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
    private int _endOfIFDOffset = 0;
    private int _bytesPerTag = 12;

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
        var arraySectionStartOffset = 2 + tags.Count() * _bytesPerTag + 4 + 8;
        var totalLength = arraySectionStartOffset;
        
        foreach (var tag in tags)
        {
            if (tag.IsArray)
            {
                totalLength += TagFields.TagDataTypeToBytes[tag.NonArrayDataType] * tag.Length;
            }
        }
        var ifdBytes = new byte[totalLength];
        Span<byte> ifdSpan = ifdBytes;
        var currentTagSectionOffset = 2;
        var currentArraySectionOffset = arraySectionStartOffset;
        // Number of entries in IFD
        BinaryPrimitives.WriteUInt16LittleEndian(ifdSpan[..currentTagSectionOffset], (ushort)tags.Count());
        
        foreach (var tag in tags)
        {
            // Tag id -> ushort
            BinaryPrimitives.WriteUInt16LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset+2)], tag.RawId);
            currentTagSectionOffset += 2;
            // Tag data type -> ushort
            BinaryPrimitives.WriteUInt16LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset+2)], (ushort)TagFields.TagTypeLookup.GetByValue(tag.NonArrayDataType)); // todo do not hardcode this
            currentTagSectionOffset += 2;
            // Count -> uint32
            BinaryPrimitives.WriteUInt32LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset + 4)], (uint)tag.Length);
            currentTagSectionOffset += 4;
            
            var tagType = tag.NonArrayDataType;
            if (tag.Length == 1 && tagType != TagDataType.ASCII)
            {
                // Value -> defined by Tag data type
                switch (tagType)
                {
                    case TagDataType.UINT:
                        BinaryPrimitives.WriteUInt32LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset + 4)], tag.GetUInt());
                        currentTagSectionOffset += 4;
                        break; 
                    case TagDataType.USHORT:
                        BinaryPrimitives.WriteUInt16LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset + 2)], tag.GetUShort());
                        currentTagSectionOffset += 4;
                        break;
                    case TagDataType.LONG:
                        if (this._bigTiff is false)
                        {
                            if (tag.GetULong() > Int32.MaxValue)
                            {
                                throw new GeoTiffException($"{tag.TagNameOrId} with value {tag.GetLong()} could not be represented as a classic tiff. Try again as a bigtiff");
                            }
                        }
                        BinaryPrimitives.WriteUInt32LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset + 4)], (uint)tag.GetULong());
                        currentTagSectionOffset += 4;
                        break;
                    // default:
                    //     break;
                }
            }
            else
            {
                BinaryPrimitives.WriteUInt32LittleEndian(ifdSpan[currentTagSectionOffset..(currentTagSectionOffset + 4)], (uint)currentArraySectionOffset);
                currentTagSectionOffset += 4;
                switch (tagType)
                {
                    case TagDataType.UINT:
                        foreach (var element in tag.GetUIntArray())
                        {
                            BinaryPrimitives.WriteUInt32LittleEndian(ifdSpan[currentArraySectionOffset..(currentArraySectionOffset+4)], element);
                            currentArraySectionOffset += 4;
                        }
                        break;
                    case TagDataType.USHORT:
                        foreach (var element in tag.GetUShortArray())
                        {
                            BinaryPrimitives.WriteUInt16LittleEndian(ifdSpan[currentArraySectionOffset..(currentArraySectionOffset+2)], element);
                            currentArraySectionOffset += 2;
                        }
                        break;
                    case TagDataType.LONG:
                        foreach (var element in tag.GetLongArray())
                        {
                            BinaryPrimitives.WriteInt64LittleEndian(ifdSpan[currentArraySectionOffset..(currentArraySectionOffset+2)], element);
                            currentArraySectionOffset += 2;
                        }
                        break;
                    case TagDataType.ASCII:
                        var stringBytes = ASCIIEncoding.ASCII.GetBytes(tag.GetRawString());
                        stringBytes.CopyTo(ifdSpan[currentArraySectionOffset..(currentArraySectionOffset+stringBytes.Length)]); 
                        currentArraySectionOffset += stringBytes.Length;
                        
                        break;
                }
            }
        }
        output.Write(ifdBytes);
    }

    private void WriteTag(Tag tag)
    {
        
    }
}