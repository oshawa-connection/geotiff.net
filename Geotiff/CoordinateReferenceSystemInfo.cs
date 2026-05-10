namespace Geotiff;


/// <summary>
/// Experimental. Contains some basic coordinate reference system information.
/// </summary>
public class CoordinateReferenceSystemInfo
{
    public ushort ModelType { get; set; }
    public ushort VerticalModelCRS { get; set; }
    public ushort GeographicCRS { get; set; }
    public ushort ProjectedCRS { get; set; }
}