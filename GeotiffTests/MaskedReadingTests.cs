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
        var maskedReader = await MaskedGeoTiffReader.FromMultiGeoTiff(maskedMultiTiff);
        var rightReadResult = await maskedReader.ReadMaskedRasterAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 25, MaxColumn = 50});

        var rightSample = rightReadResult.GetSampleAt(0);
        var rightPixelArray = rightSample.GetAsDoubleArray();
        rightPixelArray.ShouldAllBe(d => d.IsMasked == true);
        
        
        var leftReadResult = await maskedReader.ReadMaskedRasterAsync(new ImagePixelWindow() {MinRow = 0, MaxRow = 50, MinColumn = 0, MaxColumn = 24});

        var leftSample = leftReadResult.GetSampleAt(0);
        var leftPixelArray = leftSample.GetAsDoubleArray();
        leftPixelArray.ShouldAllBe(d => d.IsMasked == false);

    }
}