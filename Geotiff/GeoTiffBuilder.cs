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
    
    public GeoTiffBuilder()
    {
        
    }

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
            tiff._strategy = MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE;
            tiff.MaskImageIndex = 1;
        }
        
        return tiff;
    }
}