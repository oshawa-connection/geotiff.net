using Geotiff;

namespace GeotiffInfo;

class Program
{
    static async Task Main(string[] args)
    {
        await using var fsSource = new FileStream(args.First(), FileMode.Open, FileAccess.Read);

        var tiff = await GeoTIFF.FromStream(fsSource);
        Console.WriteLine(tiff.IsBifTIFF);
        var imageCount = await tiff.GetImageCount();
        Console.WriteLine(imageCount);

        var image = await tiff.GetImage();
        var origin = image.GetOrigin();
        var geotiffSampleType = image.GetSampleType();
        var readResult = await image.ReadRasters<double>();
        // Console.WriteLine(readResult.First()[0]);
    }
}