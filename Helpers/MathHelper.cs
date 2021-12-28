namespace OoplesFinance.StockIndicators.Helpers
{
    public static class MathHelper
    {
        public static decimal Log(double value)
        {
            return (decimal)Math.Log(value);
        }

        public static decimal Log(decimal value)
        {
            return (decimal)Math.Log((double)value);
        }

        public static decimal Log10(double value)
        {
            return (decimal)Math.Log10(value);
        }

        public static decimal Log10(decimal value)
        {
            return (decimal)Math.Log10((double)value);
        }

        public static decimal Sqrt(double value)
        {
            return (decimal)Math.Sqrt(value);
        }

        public static decimal Sqrt(decimal value)
        {
            return (decimal)Math.Sqrt((double)value);
        }

        public static decimal Sin(double value)
        {
            return (decimal)Math.Sin(value);
        }

        public static decimal Sin(decimal value)
        {
            return (decimal)Math.Sin((double)value);
        }

        public static decimal Cos(double value)
        {
            return (decimal)Math.Cos(value);
        }

        public static decimal Cos(decimal value)
        {
            return (decimal)Math.Cos((double)value);
        }

        public static decimal Asin(double value)
        {
            return (decimal)Math.Asin(value);
        }

        public static decimal Asin(decimal value)
        {
            return (decimal)Math.Asin((double)value);
        }

        public static decimal Atan(double value)
        {
            return (decimal)Math.Atan(value);
        }

        public static decimal Atan(decimal value)
        {
            return (decimal)Math.Atan((double)value);
        }

        public static decimal Pow(decimal value, double power)
        {
            decimal result;

            try
            {
                result = (decimal)Math.Pow((double)value, power);
            }
            catch (OverflowException)
            {
                result = decimal.MaxValue;
            }

            return result;
        }

        public static double Power(double value, double power)
        {
            double valueToPower;

            if (value == 0 && power < 0)
            {
                valueToPower = 0;
            }
            else if (value == 0 && power == 0)
            {
                valueToPower = 0;
            }
            else if (IsValueNullOrInfinity(value))
            {
                valueToPower = 0;
            }
            else if (IsValueNullOrInfinity(power))
            {
                valueToPower = 0;
            }
            else
            {
                valueToPower = Math.Pow(value, power);

                if (IsValueNullOrInfinity(valueToPower))
                {
                    valueToPower = double.MaxValue;
                }
            }

            return valueToPower;
        }

        public static decimal Exp(double value)
        {
            decimal exp;

            if (IsValueNullOrInfinity(value))
            {
                exp = 0;
            }
            else
            {
                exp = (decimal)Math.Exp(Math.Min(100, value));
            }

            return exp;
        }

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

        public static bool IsValueNullOrInfinity(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value);
        }

        public static double MinOrMax(double value, double maxValue, double minValue)
        {
            double result = double.IsNaN(value) ? 0 : double.IsPositiveInfinity(value) ? double.MaxValue : double.IsNegativeInfinity(value) ? double.MinValue : value;
            return Math.Min(Math.Max(result, minValue), maxValue);
        }

        public static decimal MinOrMax(decimal value, decimal maxValue, decimal minValue)
        {
            return Math.Min(Math.Max(value, minValue), maxValue);
        }

        public static int MinOrMax(int value)
        {
            int result = double.IsNaN(value) ? 0 : double.IsPositiveInfinity(value) ? int.MaxValue : double.IsNegativeInfinity(value) ? int.MinValue : value;
            return Math.Min(Math.Max(result, 2), 530);
        }

        public static decimal ToRadians(this decimal val)
        {
            return (decimal)Math.PI / 180 * val;
        }

        public static decimal ToRadians(this double val)
        {
            return (decimal)(Math.PI / 180 * val);
        }

        public static decimal ToDegrees(this decimal radianValue)
        {
            return 180 / (decimal)Math.PI * radianValue;
        }
    }
}
