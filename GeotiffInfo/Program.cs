using Geotiff;

namespace GeotiffInfo;

class Program
{
    public async static void ListKnownTags(string[] args)
    {
        await using var fsSource = new FileStream(args.First(), FileMode.Open, FileAccess.Read);

        var tiff = await GeoTiff.FromStreamAsync(fsSource);
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
                Console.WriteLine(knownTag.TagName);
            }
        }
        
        // var origin = image.GetOrigin();
        // var geotiffSampleType = image.GetSampleType();

        // Console.WriteLine(geotiffSampleType);
        // var readResult = await image.ReadRastersAsync();
        // var sampleResult = readResult.GetSampleResultAt(0);
        // Console.WriteLine(sampleResult._doubleData.GetValue(0));
        // Console.WriteLine(sampleResult.To2DArray()[0,0]);
        // Console.WriteLine(sampleResult.To2DArray()[sampleResult.Width - 1, 0]);
    }
    
    
    static async Task Main(string[] args)
    {
        var listOfFiles = Directory.GetFiles("/home/james/Documents/sharpFirehose/Geotiff/ConformanceTests/tiffData");
        foreach (var file in listOfFiles.Where(d => d.EndsWith(".tif")))
        {
            await using var fsSource = new FileStream(file, FileMode.Open, FileAccess.Read);
            var tiff = await GeoTiff.FromStreamAsync(fsSource);
            var image = await tiff.GetImageAsync();
            var knownTags = image.GetAllKnownTags();

            var found = knownTags.Where(d => d.IsArray is true);

            if (found.Any(d => d.TagName == TagFields.ModelTransformation))
            {
                Console.WriteLine(file);
                foreach (var arrTag in found)
                {
                    Console.WriteLine(arrTag.TagName);
                }
            }
            // Console.WriteLine(file);
        }

    }
}