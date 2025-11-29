using Geotiff;

namespace GeotiffInfo;

class Program
{
    async static Task Main(string[] args)
    {
        await using var fsSource = new FileStream(args.First(), FileMode.Open, FileAccess.Read);

        var tiff = await GeoTIFF.FromStream(fsSource);
        var imageCount = await tiff.GetImageCount();
        Console.WriteLine(imageCount);
    }
}