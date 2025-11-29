namespace Geotiff.Exceptions;

public class GeoTiffImageIndexError : Exception
{
    public GeoTiffImageIndexError(int index) : base($"No image at index {index}")
    {
    }
}