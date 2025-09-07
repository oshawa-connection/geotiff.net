using System.Text.Json;
using ConformanceTests.Exceptions;
using Geotiff;
using Shouldly;

namespace ConformanceTests;

class Program
{
    static string GetDataFolderPath()
    {
        // Start from the directory where the test assembly is located
        var dir = AppContext.BaseDirectory;

        // Walk up until we find the project root (i.e., contains .csproj or known marker)
        while (!Directory.GetFiles(dir, "*.csproj").Any())
        {
            dir = Directory.GetParent(dir)?.FullName
                  ?? throw new Exception("Could not locate project root.");
        }
        
        return dir;
    }

    static void ShouldBeWarn<T>(string type, T value, T other)
    {
        if (!EqualityComparer<T>.Default.Equals(value, other))
        {
            Console.WriteLine($"WARNING: {type} {value} does not equal {other}");
        }
    }
    
    static void ShouldBeError<T>(string type, T value, T other)
    {
        if (!EqualityComparer<T>.Default.Equals(value, other))
        {
            Console.WriteLine($"ERROR: {type} {value} does not equal {other}");
            throw new AssertationException();
        }
    }

    async static Task ConformanceTests()
    {
        var dir = GetDataFolderPath();
        var jsonFilePath = Path.Combine(dir, "writeResult.json");
        var text = File.ReadAllText(jsonFilePath);
        var result = JsonSerializer.Deserialize<List<GeotiffDump>>(text);

        foreach (var r in result)
        {
            var tiffPath = Path.Combine(dir, r.FileName);
            await using var fsSource = new FileStream(tiffPath,FileMode.Open, FileAccess.Read);
            var geotiff = await GeoTIFF.FromStream(fsSource);
            var count = await geotiff.GetImageCount();
            Console.WriteLine($"Messages for {r.FileName}");
            try
            {
                ShouldBeError("ImageCount", count, r.Images.Count);
                for (var i = 0; i < r.Images.Count; i++)
                {
                    var csharpImage = await geotiff.GetImage(i);
                    var resultImage = r.Images[i];
                    foreach (var tag in resultImage.Tags)
                    {
                        if (csharpImage.fileDirectory.FileDirectory.ContainsKey(tag.Key) is false)
                        {
                            Console.WriteLine($"Tag {tag.Key}");
                        }
                        else
                        {
                           
                        }
                    }
                    ShouldBeError($"Tag Count on image {i};", csharpImage.fileDirectory.FileDirectory.Count, resultImage.Tags.Count);
                    
                    
                }
            }
            catch (AssertationException e)
            {
                
            }
        }
    }

    async static Task DebugSingle()
    {
        var dir = GetDataFolderPath();
        var tiffPath = Path.Combine(dir, "tiffData", "erdas_spnad83.tif");
        
        await using var fsSource = new FileStream(tiffPath,FileMode.Open, FileAccess.Read);
        var geotiff = await GeoTIFF.FromStream(fsSource);
        var count = await geotiff.GetImageCount();
    }
    
    async static Task Main(string[] args)
    {
        await ConformanceTests();
    }
}