using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// Very useful reference http://geotiff.maptools.org/spec/geotiff2.6.html
/// </summary>
public class AffineTransformation
{
    public double a {get;set;}
    public double b {get;set;}
    public double c {get;set;}
    public double d {get;set;}
    public double e {get;set;}
    public double f {get;set;}
    public double g {get;set;}
    public double h {get;set;}
    public double i {get;set;}
    public double j {get;set;}
    public double k {get;set;}
    public double l {get;set;}
    public double m {get;set;}
    public double n {get;set;}
    public double o {get;set;}
    public double p {get;set;}
    public static AffineTransformation FromMatrix(double[] matrix)
    {
        // a b c d e f g h i j k  l  m  n  o  p
        // 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15
        //                           0  0  0  1
        return new AffineTransformation()
        {
            a = matrix[0],
            b = matrix[1],
            c = matrix[2],
            d = matrix[3],
            e = matrix[3],
            f = matrix[4],
            g = matrix[5],
            h = matrix[6],
            i = matrix[7],
            j = matrix[8],
            k = matrix[9],
            l = matrix[10],
            m = matrix[11],
            n = matrix[12],
            o = matrix[13],
            p = matrix[14]
        };
    }

    public static AffineTransformation FromModelPixelScale(double[] modelPixelScale)
    {
        if (modelPixelScale.Length != 3)
        {
            throw new GeoTiffException("Invalid model pixel scale");
        }

        return new AffineTransformation()
        {
            a = modelPixelScale[0], 
            f = modelPixelScale[1], 
            k = modelPixelScale[2]
        };
    }

    public VectorXYZ GetResolution()
    {
        if (b == 0 && e == 0)
        {
            return new VectorXYZ(a, -f, k);
        }
        
        return new VectorXYZ(
            Math.Sqrt((a * a) + (e * e)), -Math.Sqrt((b * b) + (f * f)), k);
    }

    public void SetResolution(VectorXYZ resolution)
    {
        this.a = resolution.X;
        this.f = resolution.Y;
        this.k = resolution.Z;
    }
}