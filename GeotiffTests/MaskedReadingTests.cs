using Geotiff;
using Shouldly;

namespace GeotiffTests;

[TestClass]
public class MaskedReadingTests : GeoTiffTestBaseClass
{
    [TestMethod]
    public async Task ExternalMaskFileReading()
    {
        // TODO: would be nice to have this in the main code someplace.
        string externalOverviewTifPath = Path.Combine(GetDataFolderPath(), "masked_image.tif");
        var mskFilePath = externalOverviewTifPath + ".msk";
        
        if (File.Exists(mskFilePath) is false)
        {
            throw new FileNotFoundException($"No file .msk file found at {mskFilePath}");
        }
        
        await using var mainStream = File.OpenRead(externalOverviewTifPath);
        await using var mskFileStream = File.OpenRead(mskFilePath);
        
        var maskedMultiTiff = await MultiGeoTiff.FromStreams(mainStream, new[] { mskFileStream });
        var maskedReader = await MaskedGeoTiffReader.FromExternalMaskGeoTiffAsync(maskedMultiTiff);
        var rightReadResult = await maskedReader.ReadMaskedRasterAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 25, MaxColumn = 50});

        var rightSample = rightReadResult.GetSampleAt(0);
        var rightPixelArray = rightSample.GetAsDoubleArray();
        rightPixelArray.ShouldAllBe(d => d.IsMasked == true);
        
        
        var leftReadResult = await maskedReader.ReadMaskedRasterAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 0, MaxColumn = 24});

        var leftSample = leftReadResult.GetSampleAt(0);
        var leftPixelArray = leftSample.GetAsDoubleArray();
        leftPixelArray.ShouldAllBe(d => d.IsMasked == false);

    }


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
        var maskedReader = await MaskedGeoTiffReader.FromInternalMaskGeoTiffAsync(file);
        
        var leftReadResult = await maskedReader.ReadMaskedRasterAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 0, MaxColumn = 24});
        var leftSample = leftReadResult.GetSampleAt(0);
        var leftPixelArray = leftSample.GetAsDoubleArray();
        leftPixelArray.ShouldAllBe(d => d.IsMasked == false);
        
        var rightReadResult = await maskedReader.ReadMaskedRasterAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 25, MaxColumn = 50});
        
        var rightSample = rightReadResult.GetSampleAt(0);
        var rightPixelArray = rightSample.GetAsDoubleArray();
        rightPixelArray.ShouldAllBe(d => d.IsMasked == true);
    }


    [TestMethod]
    public async Task NoDataReading()
    {
        var tifPath = Path.Combine(GetDataFolderPath(), "no_data_outline_float32.tif");
        await using var mainStream = File.OpenRead(tifPath);
        var file = await GeoTiff.FromStreamAsync(mainStream);
        var image = await file.GetImageAsync(0);
        // Console.WriteLine(image.GDAL_NODATA);
        var maskedReader = await MaskedGeoTiffReader.FromNoDataGeotiffAsync(file);
        var readResult = await maskedReader.ReadMaskedRasterAsync();
        
        readResult.GetSampleAt(0).GetAs2DDoubleArray()[0,0].IsMasked.ShouldBe(true);
    }
}