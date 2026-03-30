using System.Text.Json;
using System.Text.Json.Nodes;
using ConformanceTests.Exceptions;
using Geotiff;
using Rationals;

namespace ConformanceTests;

internal class Program
{
     private static string GetDataFolderPath()
     {
         // Start from the directory where the test assembly is located
         string? dir = AppContext.BaseDirectory;

         // Walk up until we find the project root (i.e., contains .csproj or known marker)
         while (!Directory.GetFiles(dir, "*.csproj").Any())
         {
             dir = Directory.GetParent(dir)?.FullName
                   ?? throw new Exception("Could not locate project root.");
         }

         return dir;
     }
     
     private static void LogWarning(string msg)
     {
         // Console.BackgroundColor = ConsoleColor.White;
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine(msg);
         Console.ResetColor();
     }
     
     /// <summary>
     /// Warning as in, continue execution to find more problems. Other things like not being
     /// able to find a file should cause real errors.
     /// </summary>
     /// <param name="type"></param>
     /// <param name="value"></param>
     /// <param name="other"></param>
     /// <typeparam name="T"></typeparam>
     private static void ShouldBeWarning<T>(string type, T value, T other)
     {
         if (!EqualityComparer<T>.Default.Equals(value, other))
         {
             LogWarning($"ERROR: {type} {value} does not equal {other}");
         }
     }

     private static void TagNotFoundWarning(Tag tag)
     {
         if (tag.TagName is null)
         {
             LogWarning($"Tag not found: {tag.TagName}");    
         }
         else
         {
             LogWarning($"Tag not found: {tag.RawId}");
         }
     }
     
     public static void CompareNumberTags(JsonElement element, string key, GeoTiffImage csharpImageJsonInfo)
     {
         var tag = csharpImageJsonInfo.GetTag(key);
         if (tag is null)
         {
             TagNotFoundWarning(tag);
         }

         double doubleValue = tag.GetAsDouble();
         
         double jsonValue = element.GetDouble();
         if (doubleValue != element.GetDouble())
         {
             ShouldBeWarning($"Number tag with key {key} did not match", jsonValue, doubleValue);
         }
     }

     public static void CompareRationalTags(JsonElement element, string key, GeoTiffImage csharpImageJsonInfo)
     {
         var tag = csharpImageJsonInfo.GetTag(key);
         if (tag is null)
         {
             TagNotFoundWarning(tag);
         }

         var csharpValue = tag.GetRational();
         JsonElement[]? jsonValue = element.EnumerateArray().ToArray();

         var x = new Rational(jsonValue[0].GetInt32(), jsonValue[1].GetInt32());

         if (csharpValue != x)
         {
             ShouldBeWarning($"Rational tag with key {key} did not match", x, csharpValue);
         }
     }


     public static void CompareArrayTags(JsonElement[] jsonArray, string key, GeoTiffImage fileDirectory)
     {
         var tag = fileDirectory.GetTag(key);
         
         if (tag.Length != jsonArray.Length)
         {
             ShouldBeWarning($"Array of key {key} did not match length;", jsonArray.Length, tag.Length);
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
                 throw new NotImplementedException(); // 99% sure all array tags are arrays of numbers, at least in conformance test dataset
                 break;
             case JsonValueKind.Number:
                 var doubleArray = tag.GetAsDoubleArray();
                 for (int i = 0; i < jsonArray.Length; i++)
                 {
                     if (doubleArray.ElementAt(i) != jsonArray[i].GetDouble())
                     {
                         ShouldBeWarning($"Array tag with key {key}; element at {i} did not match",
                             jsonArray[i].GetDouble(), doubleArray.ElementAt(i));
                     }
                 }

                 break;
             default:
                 throw new Exception();
         }
     }


     private static async Task ConformanceTests()
     {
         string? dir = GetDataFolderPath();
         string? jsonFilePath = Path.Combine(dir, "writeResult.json");
         string? text = File.ReadAllText(jsonFilePath);
         var fileReadResult = JsonSerializer.Deserialize<List<GeotiffJsonDump>>(text);

         foreach (GeotiffJsonDump geotiffJsonDump in fileReadResult)
         {
             string? tiffPath = Path.Combine(dir, geotiffJsonDump.FileName);
             await using var fsSource = new FileStream(tiffPath, FileMode.Open, FileAccess.Read);
             Console.WriteLine($"Messages for {geotiffJsonDump.FileName}");
             GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
             int count = await geotiff.GetImageCountAsync();

             try
             {
                 ShouldBeWarning("ImageCount", count, geotiffJsonDump.Images.Count);
                 for (int i = 0; i < geotiffJsonDump.Images.Count; i++)
                 {
                     GeoTiffImage? csharpImage = await geotiff.GetImageAsync(i);
                     GeotiffImageJsonInfo? jsonDumpImage = geotiffJsonDump.Images[i];
                     foreach (KeyValuePair<string, JsonElement> jsonTag in jsonDumpImage.Tags)
                     {
                         var csharpImageTag = csharpImage.GetTag(jsonTag.Key);
                         
                         if (csharpImageTag is null)
                         {
                             LogWarning($"Tag: {jsonTag.Key} not found in image");
                         }
                         else
                         {
                             switch (jsonTag.Value.ValueKind)
                             {
                                 case JsonValueKind.Array:
                                     
                                     // If the tag is an array, but the tag isn't a known array type, it's probably a rational.
                                     if (csharpImageTag.DataType == TagDataType.RATIONAL) // todo also handle srational
                                     {
                                         CompareRationalTags(jsonTag.Value, jsonTag.Key, csharpImage);
                                         break;
                                     }
                                     
                                     JsonElement[]? array = jsonTag.Value.EnumerateArray().ToArray();
                                     CompareArrayTags(array, jsonTag.Key, csharpImage);
                                     break;
                                 case JsonValueKind.Number:
                                     CompareNumberTags(jsonTag.Value, jsonTag.Key, csharpImage);
                                     break;
                                 case JsonValueKind.String:
                                     break;
                                 default:
                                     throw new Exception();
                             }
                         }
                     }

                     // Only compare standard tags; private tags aren't included in geotiff.js output.
                     if (csharpImage.GetAllRawTags().Where(d => d.TagName is not null).Count() != jsonDumpImage.Tags.Count)
                     {
                         var charpTagNames = csharpImage.GetAllRawTags();
                         var resultImageTagNames = jsonDumpImage.Tags.Select(d => d.Key);

                         foreach (var csharpname in charpTagNames)
                         {
                             if (resultImageTagNames.Contains(csharpname.TagName) is false)
                             {
                                 LogWarning($"Tag: {csharpname} was not included in the JSON output");
                             }
                         }
                         
                         foreach (var jsonTagName in resultImageTagNames)
                         {
                             if (charpTagNames.Select(d => d.TagName).Contains(jsonTagName) is false)
                             {
                                 LogWarning($"Tag: {jsonTagName} was not included in the csharp image");
                             }
                         }
                     }
                     
                     foreach (var jsonPixel in jsonDumpImage.Pixels)
                     {
                         var wnd = new ImagePixelWindow()
                         {
                             Top = jsonPixel.Y, 
                             Bottom = jsonPixel.Y + 1, 
                             Left = jsonPixel.X, 
                             Right = jsonPixel.X + 1
                         };
                         
                         var result = await csharpImage.ReadRasterAsync(wnd);
                         for (var sampleIndex = 0; sampleIndex < result.NumberOfSamples; sampleIndex++)
                         {
                             var currentJsonSamplePixel = jsonPixel.BandInfo[sampleIndex];
                             var csharpSample = result.GetSampleAt(sampleIndex);
                             var doubleArray = csharpSample.GetAsDoubleArray();
                             if (doubleArray.Length != 1)
                             {
                                 throw new Exception("Read result was longer than expected");
                             }

                             if (currentJsonSamplePixel != doubleArray[0])
                             {
                                 LogWarning($"Read result was {doubleArray[0]} when it should be {currentJsonSamplePixel}");
                             }
                         }
                     }
                 }
             }
             catch (Exception e)
             {
                 LogWarning(e.ToString());
             }
         }
     }

     private static async Task DebugSingle()
     {
         double x;
         string? dir = GetDataFolderPath();
         string? tiffPath = Path.Combine(dir, "tiffData", "erdas_spnad83.tif");

         await using var fsSource = new FileStream(tiffPath, FileMode.Open, FileAccess.Read);
         GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
         int count = await geotiff.GetImageCountAsync();
     }

     private static async Task Main(string[] args)
     {
         await ConformanceTests();
    }
}