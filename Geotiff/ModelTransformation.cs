namespace Geotiff;

public class ModelTransformation
{
    public double a { get; set; }
    public double b { get; set; }
    public double c { get; set; }
    public double d { get; set; }
    public double e { get; set; }
    public double f { get; set; }
    public double g { get; set; }
    public double h { get; set; }
    
    public static ModelTransformation FromIEnumerable(IEnumerable<double> list)
    {
        return new ModelTransformation()
        {
            a= list.ElementAt(0),
            b= list.ElementAt(1),
            c= list.ElementAt(2),
            d= list.ElementAt(3),
            e= list.ElementAt(4),
            f= list.ElementAt(5),
            g= list.ElementAt(6),
            h= list.ElementAt(7),
        };
    }
}