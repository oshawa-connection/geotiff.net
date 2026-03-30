using System.Collections.Generic;
using System.Text.Json;

namespace ConformanceTests;

using System.Text.Json.Serialization;

public class PixelJsonInfo
{
    [JsonPropertyName("x")] public int X { get; set; }

    [JsonPropertyName("y")] public int Y { get; set; }

    [JsonPropertyName("bandInfo")] public List<double> BandInfo { get; set; } = new();
}

public class GeotiffImageJsonInfo
{
    [JsonPropertyName("tags")] public Dictionary<string, JsonElement> Tags { get; set; } = new();

    [JsonPropertyName("pixels")] public List<PixelJsonInfo> Pixels { get; set; } = new();
}

public class GeotiffJsonDump
{
    [JsonPropertyName("fileName")] public string FileName { get; set; }

    [JsonPropertyName("images")] public List<GeotiffImageJsonInfo> Images { get; set; } = new();
}