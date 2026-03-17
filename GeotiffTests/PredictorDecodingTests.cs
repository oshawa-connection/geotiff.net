using Geotiff;
using Shouldly;

namespace GeotiffTests;

[TestClass]
public class PredictorDecodingTests : GeoTiffTestBaseClass
{
    
    [TestMethod]
    public async Task TestTinyDeflateNoPredictor()
    {
        string tinyDeflate = Path.Combine(GetDataFolderPath(), "tiny_4x4_deflate.tif");
        await using var fsSource = new FileStream(tinyDeflate, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var result = readResult.GetSampleAt(0).Get2DIntArray();
    }
    
    [TestMethod]
    public async Task TestTinyDeflateFloatWith3Predictor()
    {
        string tinyDeflate = Path.Combine(GetDataFolderPath(), "tiny_4x4_deflate_predictor3.tif");
        await using var fsSource = new FileStream(tinyDeflate, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var result = readResult.GetSampleAt(0).GetFloatArray();
        result.ShouldBe([1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4]);
    }

    [TestMethod]
    public async Task TestTinyDeflateInt32With2Predictor()
    {
        string tinyDeflate = Path.Combine(GetDataFolderPath(), "tiny_4x4_deflate_int_predictor2.tif");
        await using var fsSource = new FileStream(tinyDeflate, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var result = readResult.GetSampleAt(0).GetIntArray();
        result.ShouldBe(new []{1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4});
    }


    [TestMethod]
    public async Task TestTinyDeflateInt16With2Predictor()
    {
        string tinyDeflate = Path.Combine(GetDataFolderPath(), "tiny_4x4_deflate_int16_predictor2.tif");
        await using var fsSource = new FileStream(tinyDeflate, FileMode.Open, FileAccess.Read);
        GeoTIFF? geotiff = await GeoTIFF.FromStreamAsync(fsSource);
        GeoTiffImage? image = await geotiff.GetImageAsync();
        var readResult = await image.ReadRasterAsync(cancellationToken: cts.Token);
        var result = readResult.GetSampleAt(0).GetShortArray();
        result.ShouldBe([1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4]);
    }
}