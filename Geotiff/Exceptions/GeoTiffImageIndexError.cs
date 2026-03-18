namespace Geotiff.Exceptions;

public class GeoTiffImageIndexError : GeoTiffException
{
    public GeoTiffImageIndexError(int index) : base($"No image at index {index}")
    {
    }
}