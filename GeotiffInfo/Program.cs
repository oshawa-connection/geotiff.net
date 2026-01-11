using Geotiff;

namespace GeotiffInfo;

class Program
{
    static async Task Main(string[] args)
    {
        await using var fsSource = new FileStream(args.First(), FileMode.Open, FileAccess.Read);

        var tiff = await GeoTIFF.FromStreamAsync(fsSource);
        Console.WriteLine(tiff.IsBifTIFF);
        
        var imageCount = await tiff.GetImageCountAsync();
        Console.WriteLine(imageCount);

        var image = await tiff.GetImageAsync();
        var knownTags = image.GetAllKnownTags();
        Console.WriteLine("Known Tags:");
        foreach (var knownTag in knownTags.Where(d => d.IsArray is false))
        {
            Console.WriteLine($"{knownTag.TagName}: {knownTag.GetFirstObject()}");
        }

        foreach (var knownTag in knownTags.Where(d => d.IsArray))
        {
            Console.WriteLine($"{knownTag.TagName}");
            
            foreach (var lv in knownTag.GetArrayOfObjects())
            {
                Console.WriteLine();
            }
        }
        var origin = image.GetOrigin();
        var geotiffSampleType = image.GetSampleType();
        
        Console.WriteLine(geotiffSampleType);
        var readResult = await image.ReadRastersAsync<byte>();
        var sampleResult = readResult.GetSampleResultAt(0);
        Console.WriteLine(sampleResult.FlatData.GetValue(0));
        Console.WriteLine(sampleResult.To2DArray()[0,0]);
        Console.WriteLine(sampleResult.To2DArray()[sampleResult.Width - 1, 0]);
    }
}