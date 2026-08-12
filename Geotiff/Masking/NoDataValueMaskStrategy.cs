namespace Geotiff.Masking;

public class NoDataValueMaskStrategy : MaskStrategyABC
{
    private double NoDataValue;
    public NoDataValueMaskStrategy(double noDataValue)
    {
        this.NoDataValue = noDataValue;
    }

    public override async Task SetMaskValues(GeoTiffReader parentFile,Raster mainReadResult, ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null)
    {
        // TODO: Write this out fully with all types
        foreach (RasterSample sample in mainReadResult.GetAllReadSamples())
        {
            var doubles = sample.GetAsDoubleArray();
            for (var i = 0; i < doubles.Length; i++)
            {
                var sampleValue = doubles[i];
                if (doubles[i] == NoDataValue)
                {
                    sample.SetMaskedAtIndex(i);
                }
            }
        }
    }
}