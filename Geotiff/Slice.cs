namespace Geotiff;

/// <summary>
/// This is an implicit type (i.e. an object) that gets passed as an argument to some functions
/// and as a return type. It is NOT the same as DataSlice
/// </summary>
public class Slice
{
    public bool CheckByteLength { get; set; } = true;
    public ulong Offset { get; set; }
    public ulong Length { get; set; }
    
    public Slice(ulong offset, ulong length)
    {
        this.Offset = offset;
        this.Length = length;
    }

    public Slice(ulong offset, ulong length, bool checkByteLength) : this(offset, length)
    {
        this.CheckByteLength = checkByteLength;
    }
}