using Geotiff;
using Shouldly;

namespace GeotiffTests;

[TestClass]
public class JPGTests : GeoTiffTestBaseClass
{
    [TestMethod]
    public async Task TestYCbCrJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "test_10x10_ycbcr_jpeg.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        image.GetPlanarConfiguration().ShouldBe((ushort)1);
        image.GetTag(TagFields.Compression).GetAsInt().ShouldBe(7);
        image.GetTag("PhotometricInterpretation").GetAsInt().ShouldBe(6);
        var readResult = await image.ReadRasterAsync();
        readResult.NumberOfSamples.ShouldBe(3);
        var rSample = readResult.GetSampleAt(0);
        var gSample = readResult.GetSampleAt(1);
        var bSample = readResult.GetSampleAt(2);
        
        rSample.GetByteArray().ShouldAllBe(d => d == 117);
        gSample.GetByteArray().ShouldAllBe(d => d == 134);
        bSample.GetByteArray().ShouldAllBe(d => d == 106);
    }
    
    
    [TestMethod]
    public async Task TestGrayscaleJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "test_10x10_grayscale_jpeg.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        image.GetTag(TagFields.Compression).GetUShort().ShouldBe((ushort)7);
        var readResult = await image.ReadRasterAsync();
        readResult.NumberOfSamples.ShouldBe(1);
        var rSample = readResult.GetSampleAt(0);
        
        
        rSample.GetByteArray().ShouldAllBe(d => d == 128);
    }

    [TestMethod]
    public async Task TestTJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "tjpeg.tif");
        
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        image.GetTag(TagFields.Compression).GetUShort().ShouldBe((ushort)7);

        var origin = image.GetOrigin();
        
        var readResult = await image.ReadRasterAsync(ImagePixelWindow.FromColumnRow(9835,7944));
        image.GetPlanarConfiguration().ShouldBe((ushort)1);
        // var readResult = await image.ReadRasterAsync(new ImagePixelWindow() {Bottom = 1, Left = 0, Right = 1, Top = 0});
        image.GetTag("PhotometricInterpretation").GetAsInt().ShouldBe(6);
        // var readResult = await image.ReadRasterAsync(new ImagePixelWindow() {Bottom = 8686, Left = 8685, Right = 8686, Top = 8685});
        readResult.NumberOfSamples.ShouldBe(3);
        readResult.GetSampleAt(0).GetAsIntArray().First().ShouldBe(113);
        readResult.GetSampleAt(1).GetAsIntArray().First().ShouldBe(123);
        readResult.GetSampleAt(2).GetAsIntArray().First().ShouldBe(124);
        
        
        
        Console.WriteLine("HELLO");
        // rSample.GetByteArray().ShouldAllBe(d => d == 128);
    }
    
    
    [TestMethod]
    public async Task TestCMYKJPG()
    {
        string jpgTiffPath = Path.Combine(GetDataFolderPath(), "test_10x10_cmyk_jpeg.tif");
        await using var fsSource = new FileStream(jpgTiffPath, FileMode.Open, FileAccess.Read);
        
        GeoTiff? geotiff = await GeoTiff.FromStreamAsync(fsSource);
        var image = await geotiff.GetImageAsync();
        
        var readResult = await image.ReadRasterAsync();
        readResult.NumberOfSamples.ShouldBe(4);
        var cyanSample = readResult.GetSampleAt(0);
        var magentaSample = readResult.GetSampleAt(1);
        var yellowSample = readResult.GetSampleAt(2);
        var blackSample = readResult.GetSampleAt(3);
        
        cyanSample.GetByteArray().ShouldAllBe(d => d == 255);
        magentaSample.GetByteArray().ShouldAllBe(d => d == 0);
        yellowSample.GetByteArray().ShouldAllBe(d => d == 0);
        blackSample.GetByteArray().ShouldAllBe(d => d == 0);
    }
}