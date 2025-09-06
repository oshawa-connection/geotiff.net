namespace Geotiff.Primitives;

public readonly struct Rational
{
    public uint Numerator { get; }
    public uint Denominator { get; }

    public Rational(uint numerator, uint denominator)
    {
        Numerator = numerator;
        Denominator = denominator == 0 
            ? throw new DivideByZeroException("Denominator cannot be zero.")
            : denominator;
    }
    
    
    public Rational(int numerator, int denominator): this((uint) numerator, (uint) denominator)
    {
    }

    public double ToDouble() => (double)Numerator / Denominator;

    public override string ToString() => $"{Numerator}/{Denominator}";
}