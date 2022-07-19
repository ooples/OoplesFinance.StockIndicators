//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

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
        if (value > 0)
        {
            return (decimal)Math.Log((double)value);
        }
        else
        {
            return 0;
        }
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
        if (value < 0)
        {
            return 0;
        }
        else
        {
            return (decimal)Math.Sqrt((double)value);
        }
    }

    /// <summary>
    /// Calculates the Sine of a given value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Sin(decimal value)
    {
        return (decimal)Math.Sin((double)value);
    }

    /// <summary>
    /// Calculates the Cosine for a given value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Cos(decimal value)
    {
        return (decimal)Math.Cos((double)value);
    }

    /// <summary>
    /// Calculates the Arc Tangent for a given value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static decimal Atan(decimal value)
    {
        return (decimal)Math.Atan((double)value);
    }

    /// <summary>
    /// Calculates the Arc Sine for a given value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static decimal Asin(decimal value)
    {
        return (decimal)Math.Asin((double)value);
    }

    /// <summary>
    /// Calculates the Arc Cosine for a given value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static decimal Acos(decimal value)
    {
        return (decimal)Math.Acos((double)value);
    }

    /// <summary>
    /// Convert to Degrees From Radians
    /// </summary>
    /// <param name="val">The value to convert to degrees</param>
    /// <returns>The value in degrees</returns>
    public static decimal ToDegrees(this decimal radianValue)
    {
        return 180 / Pi * radianValue;
    }

    /// <summary>
    /// Convert to Radians From Degrees
    /// </summary>
    /// <param name="val">The value to convert to radians</param>
    /// <returns>The value in radians</returns>
    public static decimal ToRadians(this decimal degreeValue)
    {
        return Pi / 180 * degreeValue;
    }

    /// <summary>
    /// Calculates the value raised to a certain power
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
    /// Calculates E raised to a certain power
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
