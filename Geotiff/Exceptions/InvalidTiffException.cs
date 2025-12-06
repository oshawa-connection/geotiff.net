namespace Geotiff.Exceptions;

public class InvalidTiffException: GeoTiffException
{
    public InvalidTiffException(string message) : base(message)
    {
    }
}