using System.Text;

namespace Geotiff.Exceptions;

public class GeoTiffTagInvalidOperationException: GeoTiffException
{
    public static GeoTiffTagInvalidOperationException FromExceptedActualTypes(string expected,string actual)
    {
        var s = new StringBuilder();
        s.Append($"Result is not a {expected}");
        s.Append($" Actual: {actual}");
        return new GeoTiffTagInvalidOperationException(s.ToString());
    }
    
    
    public GeoTiffTagInvalidOperationException(string message): base(message)
    {
        
    }
}