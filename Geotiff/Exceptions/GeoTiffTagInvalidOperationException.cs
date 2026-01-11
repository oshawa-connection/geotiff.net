using System.Text;

namespace Geotiff.Exceptions;

public class GeoTiffTagInvalidOperationException: GeoTiffException
{
    public static GeoTiffTagInvalidOperationException FromExceptedActualTypes(string expected,string actual)
    {
        var s = new StringBuilder();
        s.Append($"Result is not a {expected}");
        s.Append($" Actual: {actual}");
        //whatever you want to do with your string before passing it in
        return new GeoTiffTagInvalidOperationException(s.ToString());
    }
    
    
    public GeoTiffTagInvalidOperationException(string message): base(message)
    {
        
    }
}