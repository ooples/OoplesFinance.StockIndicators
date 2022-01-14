using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Exceptions;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Helpers.MathHelper;

namespace OoplesFinance.StockIndicators.Helpers
{
    public static class CalculationsHelper
    {
        public const decimal Pi = 3.1415926535897931m;

        /// <summary>
        /// Gets the moving average list.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="movingAvgType">Average type of the moving.</param>
        /// <param name="length">The length.</param>
        /// <param name="customValuesList">The custom values list.</param>
        /// <returns></returns>
        public static List<decimal> GetMovingAverageList(StockData stockData, MovingAvgType movingAvgType, int length, 
            List<decimal>? customValuesList = null)
        {
            List<decimal> movingAvgList = new();

            if (customValuesList != null)
            {
                stockData.CustomValuesList = customValuesList;
            }

            switch (movingAvgType)
            {
                case MovingAvgType._1LCLeastSquaresMovingAverage:
                    movingAvgList = stockData.Calculate1LCLeastSquaresMovingAverage(MovingAvgType.SimpleMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType._3HMA:
                    movingAvgList = stockData.Calculate3HMA(MovingAvgType.WeightedMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.AdaptiveAutonomousRecursiveMovingAverage:
                    movingAvgList = stockData.CalculateAdaptiveAutonomousRecursiveMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.AdaptiveExponentialMovingAverage:
                    movingAvgList = stockData.CalculateAdaptiveExponentialMovingAverage(MovingAvgType.SimpleMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.AdaptiveLeastSquares:
                    movingAvgList = stockData.CalculateAdaptiveLeastSquares(length: length).CustomValuesList;
                    break;
                case MovingAvgType.AdaptiveMovingAverage:
                    movingAvgList = stockData.CalculateAdaptiveMovingAverage(slowLength: length, length: length).CustomValuesList;
                    break;
                case MovingAvgType.AhrensMovingAverage:
                    movingAvgList = stockData.CalculateAhrensMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.AlphaDecreasingExponentialMovingAverage:
                    movingAvgList = stockData.CalculateAlphaDecreasingExponentialMovingAverage().CustomValuesList;
                    break;
                case MovingAvgType.ArnaudLegouxMovingAverage:
                    movingAvgList = stockData.CalculateArnaudLegouxMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.AtrFilteredExponentialMovingAverage:
                    movingAvgList = stockData.CalculateAtrFilteredExponentialMovingAverage(MovingAvgType.SimpleMovingAverage, length: length).CustomValuesList;
                    break;
                case MovingAvgType.AutoFilter:
                    movingAvgList = stockData.CalculateAutoFilter(MovingAvgType.SimpleMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.AutonomousRecursiveMovingAverage:
                    movingAvgList = stockData.CalculateAutonomousRecursiveMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.BryantAdaptiveMovingAverage:
                    movingAvgList = stockData.CalculateBryantAdaptiveMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.EndPointWeightedMovingAverage:
                    movingAvgList = stockData.CalculateEndPointMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.ExponentialMovingAverage:
                    movingAvgList = stockData.CalculateExponentialMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.HullMovingAverage:
                    movingAvgList = stockData.CalculateHullMovingAverage(MovingAvgType.WeightedMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.IIRLeastSquaresEstimate:
                    movingAvgList = stockData.CalculateIIRLeastSquaresEstimate(length).CustomValuesList;
                    break;
                case MovingAvgType.InverseDistanceWeightedMovingAverage:
                    movingAvgList = stockData.CalculateInverseDistanceWeightedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.JsaMovingAverage:
                    movingAvgList = stockData.CalculateJsaMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.JurikMovingAverage:
                    movingAvgList = stockData.CalculateJurikMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.KaufmanAdaptiveMovingAverage:
                    movingAvgList = stockData.CalculateKaufmanAdaptiveMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.LeastSquaresMovingAverage:
                    movingAvgList = stockData.CalculateLeastSquaresMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.LeoMovingAverage:
                    movingAvgList = stockData.CalculateLeoMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.LightLeastSquaresMovingAverage:
                    movingAvgList = stockData.CalculateLightLeastSquaresMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.LinearExtrapolation:
                    movingAvgList = stockData.CalculateLinearExtrapolation(length).CustomValuesList;
                    break;
                case MovingAvgType.LinearRegression:
                    movingAvgList = stockData.CalculateLinearRegression(length).CustomValuesList;
                    break;
                case MovingAvgType.LinearRegressionLine:
                    movingAvgList = stockData.CalculateLinearRegressionLine(length: length).CustomValuesList;
                    break;
                case MovingAvgType.LinearWeightedMovingAverage:
                    movingAvgList = stockData.CalculateLinearWeightedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.MesaAdaptiveMovingAverage:
                    movingAvgList = stockData.CalculateEhlersMotherOfAdaptiveMovingAverages().CustomValuesList;
                    break;
                case MovingAvgType.NaturalMovingAverage:
                    movingAvgList = stockData.CalculateNaturalMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.OptimalWeightedMovingAverage:
                    movingAvgList = stockData.CalculateOptimalWeightedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.OvershootReductionMovingAverage:
                    movingAvgList = stockData.CalculateOvershootReductionMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.PoweredKaufmanAdaptiveMovingAverage:
                    movingAvgList = stockData.CalculatePoweredKaufmanAdaptiveMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.QuadraticLeastSquaresMovingAverage:
                    movingAvgList = stockData.CalculateQuadraticLeastSquaresMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.QuadraticMovingAverage:
                    movingAvgList = stockData.CalculateQuadraticMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.QuadraticRegression:
                    movingAvgList = stockData.CalculateQuadraticRegression(length: length).CustomValuesList;
                    break;
                case MovingAvgType.QuadrupleExponentialMovingAverage:
                    movingAvgList = stockData.CalculateQuadrupleExponentialMovingAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.QuickMovingAverage:
                    movingAvgList = stockData.CalculateQuickMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.SimpleMovingAverage:
                    movingAvgList = stockData.CalculateSimpleMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.SymmetricallyWeightedMovingAverage:
                    movingAvgList = stockData.CalculateSymmetricallyWeightedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.T3MovingAverage:
                    movingAvgList = stockData.CalculateT3MovingAverage(MovingAvgType.ExponentialMovingAverage, length: length).CustomValuesList;
                    break;
                case MovingAvgType.TriangularMovingAverage:
                    movingAvgList = stockData.CalculateTriangularMovingAverage(MovingAvgType.SimpleMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.Trimean:
                    movingAvgList = stockData.CalculateTrimean(length).CustomValuesList;
                    break;
                case MovingAvgType.TripleExponentialMovingAverage:
                    movingAvgList = stockData.CalculateTripleExponentialMovingAverage(MovingAvgType.ExponentialMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.UltimateMovingAverage:
                    movingAvgList = stockData.CalculateUltimateMovingAverage(MovingAvgType.SimpleMovingAverage).CustomValuesList;
                    break;
                case MovingAvgType.VariableIndexDynamicAverage:
                    movingAvgList = stockData.CalculateVariableIndexDynamicAverage(length: length).CustomValuesList;
                    break;
                case MovingAvgType.VariableLengthMovingAverage:
                    movingAvgList = stockData.CalculateVariableLengthMovingAverage(MovingAvgType.SimpleMovingAverage).CustomValuesList;
                    break;
                case MovingAvgType.VolumeWeightedAveragePrice:
                    movingAvgList = stockData.CalculateVolumeWeightedAveragePrice().CustomValuesList;
                    break;
                case MovingAvgType.VolumeWeightedMovingAverage:
                    movingAvgList = stockData.CalculateVolumeWeightedMovingAverage(MovingAvgType.SimpleMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.WeightedMovingAverage:
                    movingAvgList = stockData.CalculateWeightedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.WellRoundedMovingAverage:
                    movingAvgList = stockData.CalculateWellRoundedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.WildersSmoothingMethod:
                    movingAvgList = stockData.CalculateWellesWilderMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.WildersSummationMethod:
                    movingAvgList = stockData.CalculateWellesWilderSummation(length).CustomValuesList;
                    break;
                case MovingAvgType.WindowedVolumeWeightedMovingAverage:
                    movingAvgList = stockData.CalculateWindowedVolumeWeightedMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.ZeroLagExponentialMovingAverage:
                    movingAvgList = stockData.CalculateZeroLagExponentialMovingAverage(MovingAvgType.ExponentialMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.ZeroLagTripleExponentialMovingAverage:
                    movingAvgList = stockData.CalculateZeroLagTripleExponentialMovingAverage(MovingAvgType.TripleExponentialMovingAverage, length).CustomValuesList;
                    break;
                case MovingAvgType.ZeroLowLagMovingAverage:
                    movingAvgList = stockData.CalculateZeroLowLagMovingAverage(length: length).CustomValuesList;
                    break;
                default:
                    Console.WriteLine($"Moving Avg Name: {movingAvgType} not supported!");
                    break;
            }

            return movingAvgList;
        }

        /// <summary>
        /// Gets the input values list.
        /// </summary>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static (List<decimal> inputList, List<decimal> highList, List<decimal> lowList, List<decimal> openList, List<decimal> closeList,
            List<decimal> volumeList) GetInputValuesList(InputName inputName, StockData stockData)
        {
            List<decimal> highList;
            List<decimal> lowList;
            List<decimal> openList;
            List<decimal> closeList;
            List<decimal> volumeList;
            List<decimal> inputList = inputName switch
            {
                InputName.Close => stockData.ClosePrices,
                InputName.Low => stockData.LowPrices,
                InputName.High => stockData.HighPrices,
                InputName.Volume => stockData.Volumes,
                InputName.TypicalPrice => stockData.CalculateTypicalPrice().CustomValuesList,
                InputName.FullTypicalPrice => stockData.CalculateFullTypicalPrice().CustomValuesList,
                InputName.MedianPrice => stockData.CalculateMedianPrice().CustomValuesList,
                InputName.WeightedClose => stockData.CalculateWeightedClose().CustomValuesList,
                InputName.Open => stockData.OpenPrices,
                InputName.AdjustedClose => stockData.ClosePrices,
                _ => stockData.ClosePrices,
            };

            if (inputList.Count > 0)
            {
                decimal sum = inputList.Sum();

                if (inputList.SequenceEqual(stockData.Volumes) || sum < stockData.LowPrices.Sum() || sum > stockData.HighPrices.Sum())
                {
                    var minMaxList = GetMaxAndMinValuesList(inputList, 0);
                    highList = minMaxList.Item1;
                    lowList = minMaxList.Item2;
                }
                else
                {
                    highList = stockData.HighPrices;
                    lowList = stockData.LowPrices;
                }
            }
            else
            {
                highList = stockData.HighPrices;
                lowList = stockData.LowPrices;
            }

            openList = stockData.OpenPrices;
            closeList = stockData.ClosePrices;
            volumeList = stockData.Volumes;

            return (inputList, highList, lowList, openList, closeList, volumeList);
        }

        /// <summary>
        /// Gets the input values list.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        /// <exception cref="OoplesFinance.StockIndicators.Exceptions.CalculationException">Calculations based off of {stockData.IndicatorName} can't be completed because this indicator doesn't have a single output.</exception>
        public static (List<decimal> inputList, List<decimal> highList, List<decimal> lowList, List<decimal> openList, List<decimal> volumeList) GetInputValuesList(StockData stockData)
        {
            List<decimal> inputList;
            List<decimal> highList;
            List<decimal> lowList;
            List<decimal> openList;
            List<decimal> volumeList;

            if (stockData.CustomValuesList != null && stockData.CustomValuesList.Count > 0)
            {
                inputList = stockData.CustomValuesList;
            }
            else if ((stockData.CustomValuesList == null || (stockData.CustomValuesList != null && stockData.CustomValuesList.Count == 0)) &&
                stockData.SignalsList != null && stockData.SignalsList.Count > 0)
            {
                throw new CalculationException($"Calculations based off of {stockData.IndicatorName} can't be completed because this indicator doesn't have a single output.");
            }
            else
            {
                inputList = stockData.InputValues;
            }

            if (inputList.Count > 0)
            {
                decimal sum = inputList.Sum();

                if (inputList.SequenceEqual(stockData.Volumes) || sum < stockData.LowPrices.Sum() || sum > stockData.HighPrices.Sum())
                {
                    var minMaxList = GetMaxAndMinValuesList(inputList, 0);
                    highList = minMaxList.Item1;
                    lowList = minMaxList.Item2;
                }
                else
                {
                    highList = stockData.HighPrices;
                    lowList = stockData.LowPrices;
                }
            }
            else
            {
                highList = stockData.HighPrices;
                lowList = stockData.LowPrices;
            }

            openList = stockData.OpenPrices;
            volumeList = stockData.Volumes;

            return (inputList, highList, lowList, openList, volumeList);
        }

        /// <summary>
        /// Calculates the ema.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="prevEma">The previous ema.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static decimal CalculateEMA(decimal currentValue, decimal prevEma, int length = 14)
        {
            decimal k = MinOrMax((decimal)2 / (length + 1), 0.99m, 0.01m);
            decimal ema = (currentValue * k) + (prevEma * (1 - k));

            return ema;
        }

        /// <summary>
        /// Calculates the true range.
        /// </summary>
        /// <param name="currentHigh">The current high.</param>
        /// <param name="currentLow">The current low.</param>
        /// <param name="prevClose">The previous close.</param>
        /// <returns></returns>
        public static decimal CalculateTrueRange(decimal currentHigh, decimal currentLow, decimal prevClose)
        {
            return Math.Max(currentHigh - currentLow, Math.Max(Math.Abs(currentHigh - prevClose), Math.Abs(currentLow - prevClose)));
        }

        /// <summary>
        /// Calculates the percent change.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="previousValue">The previous value.</param>
        /// <returns></returns>
        public static decimal CalculatePercentChange(decimal currentValue, decimal previousValue)
        {
            return previousValue != 0 ? (currentValue - previousValue) / Math.Abs(previousValue) * 100 : 0;
        }

        /// <summary>
        /// Gets the maximum and minimum values list.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static (List<decimal>, List<decimal>) GetMaxAndMinValuesList(List<decimal> inputs, int length)
        {
            List<decimal> highestValuesList = new();
            List<decimal> lowestValuesList = new();
            List<decimal> inputList = new();

            for (int i = 0; i < inputs.Count; i++)
            {
                decimal input = inputs.ElementAt(i);
                inputList.Add(input);

                var list = inputList.TakeLastExt(Math.Max(length, 2)).ToList();

                decimal highestValue = list.Max();
                highestValuesList.Add(highestValue);

                decimal lowestValue = list.Min();
                lowestValuesList.Add(lowestValue);
            }

            return (highestValuesList, lowestValuesList);
        }

        /// <summary>
        /// Gets the maximum and minimum values list.
        /// </summary>
        /// <param name="highList">The high list.</param>
        /// <param name="lowList">The low list.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static (List<decimal>, List<decimal>) GetMaxAndMinValuesList(List<decimal> highList, List<decimal> lowList, int length)
        {
            List<decimal> highestList = new();
            List<decimal> lowestList = new();
            List<decimal> tempHighList = new();
            List<decimal> tempLowList = new();
            var count = highList.Count == lowList.Count ? highList.Count : 0;

            for (int i = 0; i < count; i++)
            {
                decimal high = highList.ElementAt(i);
                tempHighList.Add(high);

                decimal low = lowList.ElementAt(i);
                tempLowList.Add(low);

                decimal highest = tempHighList.TakeLastExt(length).Max();
                highestList.AddRounded(highest);

                decimal lowest = tempLowList.TakeLastExt(length).Min();
                lowestList.AddRounded(lowest);
            }

            return (highestList, lowestList);
        }

        /// <summary>
        /// Adds the rounded.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        public static void AddRounded(this List<decimal> list, decimal value)
        {
            list.Add(Math.Round(value, 4));
        }

        /// <summary>
        /// Extension for the default TakeLast method that works for older versions of .Net
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> TakeLastExt<T>(this IEnumerable<T> source, int count)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (0 == count)
                yield break;

            if (source is ICollection<T> collection)
            {
                foreach (T item in source.Skip(collection.Count))
                    yield return item;

                yield break;
            }

            if (source is IReadOnlyCollection<T> collection1)
            {
                foreach (T item in source.Skip(collection1.Count))
                    yield return item;

                yield break;
            }

            Queue<T> result = new();

            foreach (T item in source)
            {
                if (result.Count == count)
                    result.Dequeue();

                result.Enqueue(item);
            }

            foreach (T item in result)
                yield return result.Dequeue();
        }

        /// <summary>
        /// Gets the Percentile Nearest Rank
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public static decimal PercentileNearestRank(this IEnumerable<decimal> sequence, decimal percentile)
        {
            var list = sequence.OrderBy(i => i).ToList();
            var n = list.Count;
            int rank = n > 0 ? (int)Math.Ceiling(percentile / 100 * n) : 0;

            return list.ElementAtOrDefault(Math.Max(rank - 1, 0));
        }
    }
}
