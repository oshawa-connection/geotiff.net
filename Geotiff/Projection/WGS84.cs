namespace Geotiff.Projection;

internal static class WGS84
{
    
    public static double Haversine(double lat1, double lat2, double lon1, double lon2)
    {
        lat1 = DegreesToRadians(lat1);
        lat2 = DegreesToRadians(lat2);
        lon1 = DegreesToRadians(lon1);
        lon2 = DegreesToRadians(lon2);
        const double r = 6378100; // meters
            
        var sdlat = Math.Sin((lat2 - lat1) / 2);
        var sdlon = Math.Sin((lon2 - lon1) / 2);
        var q = sdlat * sdlat + Math.Cos(lat1) * Math.Cos(lat2) * sdlon * sdlon;
        var d = 2 * r * Math.Asin(Math.Sqrt(q));

        return d;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="pointLat"></param>
    /// <param name="pointLon"></param>
    /// <returns></returns>
    public static (double xPercent, double yPercent) GetRelativePosition(
        BoundingBox bbox,
        double pointLat, double pointLon)
    {
        // Compute full width (west -> east at midpoint latitude)
        double midLat = (bbox.YMin + bbox.YMax) / 2.0;
        double width = Haversine(midLat, bbox.XMin, midLat, bbox.XMax);

        // Compute full height (south -> north at midpoint longitude)
        double midLon = (bbox.XMin + bbox.XMax) / 2.0;
        double height = Haversine(bbox.YMin, midLon, bbox.YMax, midLon);

        // Compute horizontal distance (west -> pointLon at midpoint latitude)
        double xDist = Haversine(midLat, bbox.XMin, midLat, pointLon);

        // Compute vertical distance (south -> pointLat at midpoint longitude)
        double yDist = Haversine(bbox.YMin, midLon, pointLat, midLon);

        // Clamp to [0,1]
        double xPercent = Math.Min(1.0, Math.Max(0.0, xDist / width));
        double yPercent = Math.Min(1.0, Math.Max(0.0, yDist / height));

        return (xPercent, yPercent);
    }
    
    
    public static double DegreesToRadians(double degrees) {
        return degrees * Math.PI / 180;
    }
}