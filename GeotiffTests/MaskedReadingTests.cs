using Geotiff;
using Shouldly;

namespace GeotiffTests;

[TestClass]
public class MaskedReadingTests : GeoTiffTestBaseClass
{
    /// <summary>
    /// This is a tif where the left half is valid, and the right half is masked off.
    /// TODO: Might be good to create another, more explicit test where we read bit raster data. 
    /// </summary>
    [TestMethod]
    public async Task InternalMaskFileReading()
    {
        var tifPath = Path.Combine(GetDataFolderPath(), "internal_masked_image.tif");
        await using var mainStream = File.OpenRead(tifPath);
        var file = await GeoTiff.FromStreamAsync(mainStream);
        file.IsMasked.ShouldBeTrue();
        var image = await file.GetImageAsync();
        
        var leftReadResult = await image.ReadRasterMaskedAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 0, MaxColumn = 24});
        var leftSample = leftReadResult.GetSampleAt(0);
        var leftPixelArray = leftSample.GetAsMaskedDoubleArray();

        for (int i = 0; i < leftPixelArray.Length; i++)
        {
            leftSample.IsMaskedAtIndex(i).ShouldBe(false);
        }
        
        leftPixelArray.ShouldAllBe(d => d.IsMasked == false);
        
        var rightReadResult = await image.ReadRasterMaskedAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 25, MaxColumn = 50});
        
        var rightSample = rightReadResult.GetSampleAt(0);
        var rightMaskedSampleValues = rightSample.GetAsMaskedDoubleArray();
        rightMaskedSampleValues.ShouldAllBe(d => d.IsMasked == true);
    }


    [TestMethod]
    public async Task NoDataReading()
    {
        var tifPath = Path.Combine(GetDataFolderPath(), "no_data_outline_float32.tif");
        await using var mainStream = File.OpenRead(tifPath);
        var file = await GeoTiff.FromStreamAsync(mainStream);
        var image = await file.GetImageAsync(0);
        
        var readResult = await image.ReadRasterMaskedAsync();
        var sample = readResult.GetSampleAt(0);
        var maskedDoubleArray = sample.GetAsMaskedDoubleArray();
        foreach (var v in maskedDoubleArray)
        {
            if (v.Value == -9999)
            {
                v.IsMasked.ShouldBeTrue();
            }
            else
            {
                v.IsMasked.ShouldBeFalse();
            }
        }
    }


    [TestMethod]
    public async Task ExternalMaskReading()
    {
        await using var mainStream = File.OpenRead(Path.Combine(GetDataFolderPath(), "masked_image.tif"));
        await using var mskStream = File.OpenRead(Path.Combine(GetDataFolderPath(), "masked_image.tif.msk"));

        var file = await GeoTiffBuilder
            .FromStream(mainStream)
            .AddExternalMaskStream(mskStream)
            .Build();
        
        file.IsMasked.ShouldBeTrue();
        
        var leftReadResult = await file.ReadRasterMaskedAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 0, MaxColumn = 24});
        var rightReadResult = await file.ReadRasterMaskedAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 25, MaxColumn = 50});
        
        leftReadResult.GetSampleAt(0).GetAsMaskedDoubleArray().ShouldAllBe(d => d.IsMasked == false);
        rightReadResult.GetSampleAt(0).GetAsMaskedDoubleArray().ShouldAllBe(d => d.IsMasked == true);
    }
    
    
    /// <summary>
    /// Case where user thinks its masked, but it's not.
    /// </summary>
    [TestMethod]
    public async Task MaskedReadingOfNonMaskedRaster()
    {
        await using var mainStream = File.OpenRead(Path.Combine(GetDataFolderPath(), "masked_image.tif"));
        
        var file = await GeoTiffBuilder
            .FromStream(mainStream)
            .Build();
        
        file.IsMasked.ShouldBeFalse();
        
        var leftReadResult = await file.ReadRasterMaskedAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 0, MaxColumn = 24});
        var rightReadResult = await file.ReadRasterMaskedAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 25, MaxColumn = 50});
        
        leftReadResult.GetSampleAt(0).GetAsMaskedDoubleArray().ShouldAllBe(d => d.IsMasked == false);
        rightReadResult.GetSampleAt(0).GetAsMaskedDoubleArray().ShouldAllBe(d => d.IsMasked == false);
    }
}