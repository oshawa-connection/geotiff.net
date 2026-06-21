using Geotiff.Masking;
using Geotiff.RemoteClients;

namespace Geotiff;

public class GeoTiffBuilder
{
    private Stream? _mainFileStream;
    private IGeoTiffRemoteClient _remoteClient;
    private Stream? _externalMaskStream;
    private Stream? _tfwFileStream;
    private double? _noDataValue;
    private AffineTransformation? _affineTransformation;
    private MaskStrategyABC maskStrategy;
    
    public static GeoTiffBuilder FromStream(Stream stream)
    {
        return new GeoTiffBuilder() { _mainFileStream = stream };
    }

    public GeoTiffBuilder AddExternalMaskStream(Stream stream)
    {
        this._externalMaskStream = stream;
        return this;
    }


    public GeoTiffBuilder AddTFWFileStream(Stream stream)
    {
        this._tfwFileStream = stream;
        return this;
    }

    public GeoTiffBuilder SetNoDataValue(double value)
    {
        this._noDataValue = value;
        return this;
    }
    
    public GeoTiffBuilder SetAffineTransformation(AffineTransformation affineTransformation)
    {
        this._affineTransformation = affineTransformation;
        return this;
    }

    /// <summary>
    /// Override auto-detected masking strategy or set a custom one.
    /// </summary>
    /// <param name="maskStrategy"></param>
    /// <returns></returns>
    public GeoTiffBuilder SetMaskStrategy(MaskStrategyABC maskStrategy)
    {
        this.maskStrategy = maskStrategy;
        return this;
    }

    public async Task<GeoTiff> Build()
    {
        GeoTiff tiff;
        if (this._remoteClient is not null)
        {
            tiff = await GeoTiff.FromRemoteClientAsync(this._remoteClient);
        }
        else
        {
            tiff = await GeoTiff.FromStreamAsync(this._mainFileStream);
        }

        if (this._externalMaskStream is not null)
        {
            GeoTiff mskStream = await GeoTiff.FromStreamAsync(this._externalMaskStream);
            tiff = new MultiGeoTiff(tiff, [mskStream]);
            var strat = new MaskImageMaskStrategy(1, Constant.EXTERNAL_MASK_YES_DATA_VALUE);
            tiff.SetMaskStrategy(strat);
        }

        if (this.maskStrategy is not null)
        {
            tiff.SetMaskStrategy(this.maskStrategy);
        }
        
        return tiff;
    }
}