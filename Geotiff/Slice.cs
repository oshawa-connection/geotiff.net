namespace Geotiff;

/// <summary>
/// This is an implicit type (i.e. an object) that gets passed as an argument to some functions
/// and as a return type. It is NOT the same as DataSlice
/// </summary>
public class Slice
{
    public Slice(int offset, int length)
    {
        this.offset = offset;
        this.length = length;
    }

    public int offset { get; set; }
    public int length { get; set; }
}