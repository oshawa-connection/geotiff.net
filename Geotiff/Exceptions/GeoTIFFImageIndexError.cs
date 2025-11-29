namespace Geotiff.Exceptions;

public class GeoTIFFImageIndexError : Exception
{
    public GeoTIFFImageIndexError(int index) : base($"No image at index {index}")
    {
    }
}