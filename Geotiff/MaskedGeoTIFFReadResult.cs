namespace Geotiff;

public class MaskedGeoTIFFReadResult<T>(IEnumerable<T[]> flatData, uint width, uint height, GeoTiffImage parentImage) : GeoTIFFReadResult<T>(flatData, width, height, parentImage) where T : struct 
{
    
}