namespace Geotiff.Primitives;

public readonly struct Rational : IEquatable<Rational>, IConvertible
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

    public bool Equals(Rational other)
    {
        return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rational other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }
    
    public static bool operator ==(Rational lhs, Rational rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Rational lhs, Rational rhs) => !(lhs == rhs);
    public TypeCode GetTypeCode()
    {
        return TypeCode.Object;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        return ToDouble() != 0.0;
    }

    public byte ToByte(IFormatProvider? provider)
    {
        return Convert.ToByte(ToDouble());
    }

    public char ToChar(IFormatProvider? provider)
    {
        return Convert.ToChar(ToDouble());
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return Convert.ToDateTime(ToDouble());
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        return Convert.ToDecimal(ToDouble());
    }

    public double ToDouble(IFormatProvider? provider)
    {
        return ToDouble();
    }

    public short ToInt16(IFormatProvider? provider)
    {
        return Convert.ToInt16(ToDouble());
    }

    public int ToInt32(IFormatProvider? provider)
    {
        return Convert.ToInt32(ToDouble());
    }

    public long ToInt64(IFormatProvider? provider)
    {
        return Convert.ToInt64(ToDouble());
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        return Convert.ToSByte(ToDouble());
    }

    public float ToSingle(IFormatProvider? provider)
    {
        return Convert.ToSingle(ToDouble());
    }

    public string ToString(IFormatProvider? provider)
    {
        return ToString();
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        if (conversionType == typeof(Rational))
            return this;
        if (conversionType == typeof(double))
            return ToDouble();
        if (conversionType == typeof(float))
            return (float)ToDouble();
        if (conversionType == typeof(decimal))
            return (decimal)ToDouble();
        if (conversionType == typeof(int))
            return (int)ToDouble();
        if (conversionType == typeof(uint))
            return (uint)ToDouble();
        if (conversionType == typeof(long))
            return (long)ToDouble();
        if (conversionType == typeof(ulong))
            return (ulong)ToDouble();
        if (conversionType == typeof(short))
            return (short)ToDouble();
        if (conversionType == typeof(ushort))
            return (ushort)ToDouble();
        if (conversionType == typeof(byte))
            return (byte)ToDouble();
        if (conversionType == typeof(sbyte))
            return (sbyte)ToDouble();
        if (conversionType == typeof(string))
            return ToString(provider);

        throw new InvalidCastException($"Cannot convert Rational to {conversionType.Name}.");
    }


    // public object ToType(Type conversionType, IFormatProvider? provider)
    // {
    //     if (conversionType == typeof(Rational))
    //         return this;
    //     return Convert.ChangeType(ToDouble(), conversionType, provider);
    // }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        return Convert.ToUInt16(ToDouble());
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        return Convert.ToUInt32(ToDouble());
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        return Convert.ToUInt64(ToDouble());
    }
}