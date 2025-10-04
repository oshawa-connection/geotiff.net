using System.Text.Json;
using System.Text.Json.Nodes;
using ConformanceTests.Exceptions;
using Geotiff;
using Geotiff.Primitives;
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
            // throw new AssertationException();
        }
    }

    public static void CompareNumberTags(JsonElement element, string key, ImageFileDirectory fileDirectory)
    {
        var doubleValue = fileDirectory.GetFileDirectoryValue<double>(key);// promote to double, GDAL style.
        var jsonValue = element.GetDouble();
        if (doubleValue != element.GetDouble())
        {
            ShouldBeError($"Number tag with key {key} did not match", jsonValue, doubleValue);
        }
    }
    
    public static void CompareRationalTags(JsonElement element, string key, ImageFileDirectory fileDirectory)
    {
        var csharpValue = fileDirectory.GetFileDirectoryValue<Rational>(key);// promote to double, GDAL style.
        var jsonValue = element.EnumerateArray().ToArray();
        
        var x = new Rational(jsonValue[0].GetInt32(), jsonValue[1].GetInt32());
        
        if (csharpValue != x)
        {
            ShouldBeError($"Number tag with key {key} did not match", x, csharpValue);
        }
    }
    
    
    public static void CompareArrayTags(JsonElement[] jsonArray, string key, ImageFileDirectory fileDirectory)
    {
        if (key == "JPEGTables")
        {
            Console.WriteLine("JPEGTables tag encountered; skipping");
            return;
        }
        
        var lengthCheck = fileDirectory.GetFileDirectoryListValue<double>(key);

        if (lengthCheck.Count() != jsonArray.Length)
        {
            ShouldBeError($"Array of key {key} did not match length;", jsonArray.Length, lengthCheck.Count());
            return;
        }
        
        // Check lengths match here
        if (jsonArray.Length == 0)
        {
            return; // return, all ok.
        }

        switch (jsonArray[0].ValueKind)
        {
            case JsonValueKind.String:
                var strArray = fileDirectory.GetFileDirectoryListValue<string>(key);
                throw new NotImplementedException(); // 99% sure all array tags are arrays of numbers, at least 
                break;
            case JsonValueKind.Number:
                var doubleArray = fileDirectory.GetFileDirectoryListValue<double>(key);// promote to double, GDAL style.
                for (var i = 0; i < jsonArray.Length; i++)
                {
                    if (doubleArray.ElementAt(i) != jsonArray[i].GetDouble())
                    {
                        ShouldBeError($"Array tag with key {key}; element at {i} did not match", jsonArray[i].GetDouble(), doubleArray.ElementAt(i));   
                    }
                }
                break;
            default:
                throw new Exception();
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
            Console.WriteLine($"Messages for {r.FileName}");
            var geotiff = await GeoTIFF.FromStream(fsSource);
            var count = await geotiff.GetImageCount();
            
            try
            {
                ShouldBeError("ImageCount", count, r.Images.Count);
                for (var i = 0; i < r.Images.Count; i++)
                {
                    var csharpImage = await geotiff.GetImage(i);
                    var resultImage = r.Images[i];
                    foreach (var tag in resultImage.Tags)
                    {
                        if (csharpImage.fileDirectory.FileDirectory.ContainsKey(tag.Key) is false )
                        {
                            Console.WriteLine($"Tag {tag.Key} was missing in csharp read image");
                        }
                        else
                        {
                            switch (tag.Value.ValueKind)
                            {
                                case JsonValueKind.Array:
                                    if (FieldTypes.ArrayTypeFields.Contains(FieldTypes.FieldTags.GetByValue(tag.Key)) ==
                                        false)
                                    {
                                        CompareRationalTags(tag.Value, tag.Key, csharpImage.fileDirectory);
                                        break;// probably a rational
                                    }
                                    var array = tag.Value.EnumerateArray().ToArray();
                                    CompareArrayTags(array, tag.Key, csharpImage.fileDirectory);
                                    break;
                                case JsonValueKind.Number:
                                    CompareNumberTags(tag.Value, tag.Key, csharpImage.fileDirectory);
                                    break;
                                case JsonValueKind.String:
                                    break;
                                default:
                                    throw new Exception();
                            }
                        }
                    }

                    // var tagCount = csharpImage.fileDirectory.FileDirectory.Count;
                    //
                    // if (csharpImage.fileDirectory.GeoKeyDirectory is not null)
                    // {
                    //     tagCount += csharpImage.fileDirectory.GeoKeyDirectory.Count;
                    // }
                    
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
        double x;
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