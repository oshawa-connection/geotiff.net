using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// This class is used for both cases (both ModelPixelScaleTag + tiepoints vs ModelTransformationTag). This allows users
/// to do whatever they like vis-a-vis rotation. E.g. they could read a raster that uses tiepoints and then rotate + write it.
/// Very useful reference http://geotiff.maptools.org/spec/geotiff2.6.html
/// </summary>
public class AffineTransformation
{
    public double a {get;set;} // resolution x
    public double b {get;set;}
    public double c {get;set;}
    public double d {get;set;} // origin X

    public double OriginX
    {
        get => this.d;
        set => this.d = value;
    }
    public double e {get;set;}
    public double f {get;set;} // resolution y
    public double g {get;set;}
    public double h {get;set;} // origin Y
    public double OriginY
    {
        get => this.h;
        set => this.h = value;
    }
    public double i {get;set;}
    public double j {get;set;}
    public double k {get;set;} // resolution z
    public double l {get;set;} // origin Z
    public double OriginZ
    {
        get => this.l;
        set => this.l = value;
    }
    public double m {get;set;} // 0
    public double n {get;set;} // 0
    public double o {get;set;} // 0
    public double p {get;set;} // 1
    public static AffineTransformation FromModelTransformation(double[] matrix)
    {
        if (matrix.Length < 12)
        {
            throw new GeoTiffException("Invalid model pixel scale");
        }

        // a b c d e f g h i j k  l  m  n  o  p
        // 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15
        //                           0  0  0  1
        var affine = new AffineTransformation()
        {
            a = matrix[0],
            b = matrix[1],
            c = matrix[2],
            d = matrix[3],
            e = matrix[4],
            f = matrix[5],
            g = matrix[6],
            h = matrix[7],
            i = matrix[8],
            j = matrix[9],
            k = matrix[10],
            l = matrix[11],

        };
        
        if (matrix.Length == 16)
        {
            affine.m = matrix[12];
            affine.n = matrix[13];
            affine.o = matrix[14];
            affine.p = matrix[15];
        }
        else
        {
            affine.m = 0;
            affine.n = 0;
            affine.o = 0;
            affine.p = 1;
        }

        return affine;
    }

    /// <summary>
    /// If used, origin will not be set, only resolution
    /// </summary>
    /// <param name="modelPixelScale"></param>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public static AffineTransformation FromModelPixelScale(double[] modelPixelScale)
    {
        if (modelPixelScale.Length != 3)
        {
            throw new GeoTiffException("Invalid model pixel scale");
        }

        return new AffineTransformation()
        {
            a = modelPixelScale[0], 
            f = -1 * modelPixelScale[1], 
            k = modelPixelScale[2]
        };
    }

    public static AffineTransformation FromTiepoint(double[] tiepoint)
    {
        var affine = new AffineTransformation();
        affine.OriginX = tiepoint[3];
        affine.OriginY = tiepoint[4];
        affine.OriginZ = tiepoint[5];

        return affine;
    }
    
    public static AffineTransformation FromModelPixelScaleAndTiePoints(double[] modelPixelScale, double[] tiePoints)
    {
        var affine = new AffineTransformation
        {
            a = modelPixelScale[0], 
            f = -1 * modelPixelScale[1], 
            k = modelPixelScale[2],
            OriginX = tiePoints[3],
            OriginY = tiePoints[4],
            OriginZ = tiePoints[5]
        };
        
        return affine;
    }

    public VectorXYZ GetResolution()
    {
        if (b == 0 && e == 0)
        {
            return new VectorXYZ(a, f, k);
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

    public VectorXYZ GetOrigin()
    {
        return new VectorXYZ() { X = this.d, Y = this.h, Z = this.l};
    }
    
    public VectorXYZ PixelToModel(double I, double J, double K = 0)
    {
        double x = a * I + b * J + c * K + d;
        double y = e * I + f * J + g * K + h;
        double z = I * i + J * j + K * k + l;

        return new VectorXYZ(x, y, z);
    }
    
    public VectorXYZ ModelToPixel(double x, double y, double z = 0)
    {
        double xPrime = x - d;
        double yPrime = y - h;

        double det = a * f - b * e;
        // TODO: One optimisation would be to calculate the affine transform determinant once during construction/ change.
        if (Math.Abs(det) < 1e-12)
            throw new GeoTiffException("Affine transform is not invertible.");

        double i = ( f * xPrime - b * yPrime) / det;
        double j = (-e * xPrime + a * yPrime) / det;

        double k = this.k != 0 ? (z - l) / this.k : 0;

        return new VectorXYZ(i, j, k);
    }

    private AffineTransformation()
    {
        
    }

    public AffineTransformation Copy()
    {
        return new AffineTransformation()
        {
            a = a,
            b = b,
            c = c,
            d = d,
            e = e,
            f = f,
            g = g,
            h = h,
            i = i,
            j = j,
            k = k,
            l = l,
            m = m,
            n = n,
            o = o,
            p = p
        };
    }
}