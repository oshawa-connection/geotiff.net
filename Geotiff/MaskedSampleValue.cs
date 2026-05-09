namespace Geotiff;

public class MaskedSampleValue<T>
{
    public T Value { get; set; }
    public bool IsMasked { get; set; }
    

    public MaskedSampleValue(T value, bool isMasked)
    {
        this.Value = value;
        this.IsMasked = isMasked;
    }
}