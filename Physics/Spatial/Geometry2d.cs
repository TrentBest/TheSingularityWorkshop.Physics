using System.Numerics;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Classifies the exact orientation of three finite 2D points represented by binary64 values.</summary>
public enum Orientation2d
{
    /// <summary>The points are exactly collinear in their represented binary64 coordinates.</summary>
    Collinear = 0,
    /// <summary>The third point is to the left of the directed line from the first point to the second.</summary>
    CounterClockwise = 1,
    /// <summary>The third point is to the right of the directed line from the first point to the second.</summary>
    Clockwise = -1
}

/// <summary>Robust predicates for two-dimensional geometry.</summary>
/// <remarks>
/// The orientation predicate evaluates the determinant exactly for the supplied finite binary64
/// coordinates. It does not claim that measured or rounded input coordinates are physically exact;
/// it classifies the values actually supplied. BigInteger fallback arithmetic prioritizes correctness
/// over throughput and is not intended as a bulk GPU kernel.
/// </remarks>
public static class Geometry2d
{
    /// <summary>
    /// Classifies the orientation of the ordered points a, b, c using the exact sign of
    /// (b.X-a.X)*(c.Y-a.Y) - (b.Y-a.Y)*(c.X-a.X).
    /// </summary>
    /// <returns>CounterClockwise for a positive determinant, Clockwise for a negative determinant, otherwise Collinear.</returns>
    public static Orientation2d Orient(Vector2d a, Vector2d b, Vector2d c)
    {
        // Fast path is deliberately conservative. If subtraction, multiplication, or the error
        // bound is not safely finite, use exact integer arithmetic over the binary64 inputs.
        double abx = b.X - a.X;
        double aby = b.Y - a.Y;
        double acx = c.X - a.X;
        double acy = c.Y - a.Y;
        double left = abx * acy;
        double right = aby * acx;
        double determinant = left - right;
        const double minNormal = 2.2250738585072014e-308;
        if (double.IsFinite(abx) && double.IsFinite(aby) &&
            double.IsFinite(acx) && double.IsFinite(acy) &&
            double.IsFinite(left) && double.IsFinite(right) && double.IsFinite(determinant) &&
            Math.Abs(left) >= minNormal && Math.Abs(right) >= minNormal &&
            Math.Abs(determinant) >= minNormal)
        {
            const double unitRoundoff = 1.1102230246251565e-16; // 2^-53; use unit roundoff, not double.Epsilon.
            double errorBound = (Math.Abs(left) + Math.Abs(right)) * (3d + (16d * unitRoundoff)) * unitRoundoff;
            if (double.IsFinite(errorBound) && Math.Abs(determinant) > errorBound)
                return determinant > 0d ? Orientation2d.CounterClockwise : Orientation2d.Clockwise;
        }

        return ExactOrientation(a, b, c);
    }

    private static Orientation2d ExactOrientation(Vector2d a, Vector2d b, Vector2d c)
    {
        double[] values = { a.X, a.Y, b.X, b.Y, c.X, c.Y };
        int minExponent = int.MaxValue;
        var parts = new (BigInteger Mantissa, int Exponent)[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            parts[i] = Decompose(values[i]);
            if (!parts[i].Mantissa.IsZero && parts[i].Exponent < minExponent)
                minExponent = parts[i].Exponent;
        }

        if (minExponent == int.MaxValue)
            return Orientation2d.Collinear;

        BigInteger[] scaled = new BigInteger[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Mantissa.IsZero)
            {
                scaled[i] = BigInteger.Zero;
                continue;
            }

            scaled[i] = parts[i].Mantissa << (parts[i].Exponent - minExponent);
        }

        BigInteger abx = scaled[2] - scaled[0];
        BigInteger aby = scaled[3] - scaled[1];
        BigInteger acx = scaled[4] - scaled[0];
        BigInteger acy = scaled[5] - scaled[1];
        BigInteger exactDeterminant = (abx * acy) - (aby * acx);
        return exactDeterminant.Sign switch
        {
            > 0 => Orientation2d.CounterClockwise,
            < 0 => Orientation2d.Clockwise,
            _ => Orientation2d.Collinear
        };
    }

    private static (BigInteger Mantissa, int Exponent) Decompose(double value)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);
        bool negative = bits < 0;
        int exponentBits = (int)((bits >> 52) & 0x7ffL);
        long fraction = bits & 0x000f_ffff_ffff_ffffL;

        if (exponentBits == 0 && fraction == 0)
            return (BigInteger.Zero, 0);

        BigInteger mantissa;
        int exponent;
        if (exponentBits == 0)
        {
            mantissa = new BigInteger(fraction);
            exponent = -1074;
        }
        else
        {
            mantissa = new BigInteger((1L << 52) | fraction);
            exponent = exponentBits - 1023 - 52;
        }

        return (negative ? -mantissa : mantissa, exponent);
    }
}
