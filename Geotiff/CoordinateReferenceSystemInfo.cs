namespace Geotiff;


/// <summary>
/// Experimental.
/// </summary>
public class CoordinateReferenceSystemInfo
{
    public ushort ModelType { get; set; }
    public ushort VerticalModelCRS { get; set; }
    public ushort GeographicCRS { get; set; }
    public ushort ProjectedCRS { get; set; }
}