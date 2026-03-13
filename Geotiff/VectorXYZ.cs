namespace Geotiff;

public class VectorXYZ
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public VectorXYZ()
    {
        
    }

    public VectorXYZ(double x, double y, double z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
    
    public VectorXYZ(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }
}