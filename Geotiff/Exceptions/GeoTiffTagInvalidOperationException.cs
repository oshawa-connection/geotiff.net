using Geotiff.Extensions;
using System.Text;

namespace Geotiff.Exceptions;

public class GeoTiffTagInvalidOperationException: GeoTiffException
{
    public static GeoTiffTagInvalidOperationException FromExceptedActualTypes(string expected, TagDataType actual)
    {
        var s = new StringBuilder();
        s.Append($"Result is not a {expected}.");
        s.Append($" ACTUAL: {actual.GetDescription()}");
        return new GeoTiffTagInvalidOperationException(s.ToString());
    }
    
    
    public GeoTiffTagInvalidOperationException(string message): base(message)
    {
        
    }
}