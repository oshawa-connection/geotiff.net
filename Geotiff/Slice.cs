namespace Geotiff;

/// <summary>
/// This is an implicit type (i.e. an object) that gets passed as an argument to some functions
/// and as a return type. It is NOT the same as DataSlice
/// </summary>
public class Slice
{
    public Slice(int offset, int length)
    {
        this.Offset = offset;
        this.Length = length;
    }

    public Slice(int offset, int length, bool checkByteLength) : this(offset, length)
    {
        this.CheckByteLength = checkByteLength;
    }

    public bool CheckByteLength { get; set; } = true;
    public int Offset { get; set; }
    public int Length { get; set; }
}