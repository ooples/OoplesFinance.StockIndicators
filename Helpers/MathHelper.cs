namespace OoplesFinance.StockIndicators.Helpers;

public static class MathHelper
{
    public const decimal Pi = 3.1415926535897931m;

    /// <summary>
    /// Logs the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Log(decimal value)
    {
        return (decimal)Math.Log((double)value);
    }

    /// <summary>
    /// Log10s the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Log10(decimal value)
    {
        return (decimal)Math.Log10((double)value);
    }

    /// <summary>
    /// SQRTs the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Sqrt(decimal value)
    {
        return (decimal)Math.Sqrt((double)value);
    }

    /// <summary>
    /// Sins the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Sin(decimal value)
    {
        return (decimal)Math.Sin((double)value);
    }

    /// <summary>
    /// Coses the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Cos(decimal value)
    {
        return (decimal)Math.Cos((double)value);
    }

    /// <summary>
    /// Atans the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Atan(decimal value)
    {
        return (decimal)Math.Atan((double)value);
    }

    /// <summary>
    /// Calculates the Acos for the specified value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static decimal Acos(decimal value)
    {
        return (decimal)Math.Acos((double)value);
    }

    /// <summary>
    /// Convert to Degrees.
    /// </summary>
    /// <param name="val">The value to convert to degrees</param>
    /// <returns>The value in degrees</returns>
    public static decimal ToDegrees(this decimal radianValue)
    {
        return 180 / Pi * radianValue;
    }

    /// <summary>
    /// Convert to Radians.
    /// </summary>
    /// <param name="val">The value to convert to radians</param>
    /// <returns>The value in radians</returns>
    public static decimal ToRadians(this decimal val)
    {
        return Pi / 180 * val;
    }

    /// <summary>
    /// Pows the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="power">The power.</param>
    /// <returns></returns>
    public static decimal Pow(decimal value, decimal power)
    {
        decimal result;

        try
        {
            result = (decimal)Math.Pow((double)value, (double)power);
        }
        catch (OverflowException)
        {
            result = decimal.MaxValue;
        }

        return result;
    }

    /// <summary>
    /// Exps the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Exp(decimal value)
    {
        decimal result;

        try
        {
            result = (decimal)Math.Exp((double)Math.Min(100, value));
        }
        catch (OverflowException)
        {
            result = decimal.MaxValue - 1;
        }
        catch (Exception)
        {
            throw;
        }

        return result;
    }

    /// <summary>
    /// Determines whether [is value null or infinity] [the specified value].
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [is value null or infinity] [the specified value]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValueNullOrInfinity(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value);
    }

    /// <summary>
    /// Minimums the or maximum.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <returns></returns>
    public static decimal MinOrMax(decimal value, decimal maxValue, decimal minValue)
    {
        return Math.Min(Math.Max(value, minValue), maxValue);
    }

    /// <summary>
    /// Minimums the or maximum.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static int MinOrMax(int value)
    {
        int result = double.IsNaN(value) ? 0 : double.IsPositiveInfinity(value) ? int.MaxValue : double.IsNegativeInfinity(value) ? int.MinValue : value;
        return Math.Min(Math.Max(result, 2), 530);
    }
}
