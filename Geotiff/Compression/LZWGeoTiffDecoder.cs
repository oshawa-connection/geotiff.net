using Geotiff.JavaScriptCompatibility;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Lzw;

namespace Geotiff.Compression;

public class LZWGeoTiffDecoder: GeoTiffDecoder
{
    private const int MIN_BITS = 9;
    private const int CLEAR_CODE = 256;
    private const int EOI_CODE = 257;
    private const int MAX_BYTELENGTH = 12;

    private static int GetByte(byte[] array, int position, int length)
    {
        int d = position % 8;
        int a = position / 8;
        int de = 8 - d;
        int ef = (position + length) - ((a + 1) * 8);
        int fg = (8 * (a + 2)) - (position + length);
        int dg = ((a + 2) * 8) - position;

        fg = Math.Max(0, fg);

        if (a >= array.Length)
        {
            Console.WriteLine("ran off the end of the buffer before finding EOI_CODE");
            return EOI_CODE;
        }

        int chunk1 = array[a] & ((1 << (8 - d)) - 1);
        chunk1 <<= (length - de);
        int chunks = chunk1;

        if (a + 1 < array.Length)
        {
            int chunk2 = array[a + 1] >> fg;
            chunk2 <<= Math.Max(0, (length - dg));
            chunks += chunk2;
        }

        if (ef > 8 && a + 2 < array.Length)
        {
            int hi = ((a + 3) * 8) - (position + length);
            int chunk3 = array[a + 2] >> hi;
            chunks += chunk3;
        }

        return chunks;
    }

    private static void AppendReversed(List<byte> dest, List<byte> source)
    {
        for (int i = source.Count - 1; i >= 0; i--)
        {
            dest.Add(source[i]);
        }
    }

    private static byte[] Decompress(byte[] input)
    {
        ushort[] dictionaryIndex = new ushort[4093];
        byte[] dictionaryChar = new byte[4093];

        for (int i = 0; i <= 257; i++)
        {
            dictionaryIndex[i] = 4096;
            dictionaryChar[i] = (byte)i;
        }

        int dictionaryLength = 258;
        int byteLength = MIN_BITS;
        int position = 0;

        void InitDictionary()
        {
            dictionaryLength = 258;
            byteLength = MIN_BITS;
        }

        int GetNext(byte[] array)
        {
            int b = GetByte(array, position, byteLength);
            position += byteLength;
            return b;
        }

        int AddToDictionary(int i, byte c)
        {
            dictionaryChar[dictionaryLength] = c;
            dictionaryIndex[dictionaryLength] = (ushort)i;
            dictionaryLength++;
            return dictionaryLength - 1;
        }

        List<byte> GetDictionaryReversed(int n)
        {
            List<byte> rev = new List<byte>();
            for (int i = n; i != 4096; i = dictionaryIndex[i])
            {
                rev.Add(dictionaryChar[i]);
            }
            return rev;
        }

        List<byte> result = new List<byte>();
        InitDictionary();

        int code = GetNext(input);
        int? oldCode = null;

        while (code != EOI_CODE)
        {
            if (code == CLEAR_CODE)
            {
                InitDictionary();
                code = GetNext(input);

                while (code == CLEAR_CODE)
                {
                    code = GetNext(input);
                }

                if (code == EOI_CODE)
                {
                    break;
                }
                else if (code > CLEAR_CODE)
                {
                    throw new Exception($"corrupted code at scanline {code}");
                }
                else
                {
                    var val = GetDictionaryReversed(code);
                    AppendReversed(result, val);
                    oldCode = code;
                }
            }
            else if (code < dictionaryLength)
            {
                var val = GetDictionaryReversed(code);
                AppendReversed(result, val);

                if (oldCode.HasValue)
                {
                    AddToDictionary(oldCode.Value, val[val.Count - 1]);
                }

                oldCode = code;
            }
            else
            {
                if (!oldCode.HasValue)
                {
                    throw new Exception($"Invalid LZW code: {code} with no previous code");
                }

                var oldVal = GetDictionaryReversed(oldCode.Value);

                if (oldVal == null || oldVal.Count == 0)
                {
                    throw new Exception($"Bogus entry. Not in dictionary, {oldCode} / {dictionaryLength}, position: {position}");
                }

                AppendReversed(result, oldVal);
                result.Add(oldVal[oldVal.Count - 1]);

                AddToDictionary(oldCode.Value, oldVal[oldVal.Count - 1]);
                oldCode = code;
            }

            if (dictionaryLength + 1 >= (1 << byteLength))
            {
                if (byteLength == MAX_BYTELENGTH)
                {
                    oldCode = null;
                }
                else
                {
                    byteLength++;
                }
            }

            code = GetNext(input);
        }

        return result.ToArray();
    }

    public byte[] DecodeBlock(byte[] buffer)
    {
        return Decompress(buffer);
    }


    public override IEnumerable<int> codes => new[] { 5 };

    protected override async Task<ArrayBuffer> DecodeBlockAsync(ArrayBuffer buffer, GeoTiffImage image)
    {
        var bytes = buffer.GetAllBytes();
        var decompressed = Decompress(bytes);
        return new ArrayBuffer(decompressed);
    }
}


