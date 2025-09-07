using System.Collections.Generic;
namespace ConformanceTests;

using System.Text.Json.Serialization;

public class PixelInfo
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("bandInfo")]
    public List<double> BandInfo { get; set; } = new List<double>();
}

public class GeotiffImage
{
    [JsonPropertyName("tags")]
    public Dictionary<string, object> Tags { get; set; } = new Dictionary<string, object>();

    [JsonPropertyName("pixels")]
    public List<PixelInfo> Pixels { get; set; } = new List<PixelInfo>();
}

public class GeotiffDump
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; }

    [JsonPropertyName("images")]
    public List<GeotiffImage> Images { get; set; } = new List<GeotiffImage>();
}