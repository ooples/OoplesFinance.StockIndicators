global using OoplesFinance.StockIndicators.Enums;
global using OoplesFinance.StockIndicators.Exceptions;
global using OoplesFinance.StockIndicators.Models;
global using static OoplesFinance.StockIndicators.Helpers.MathHelper;
global using static OoplesFinance.StockIndicators.Helpers.SignalHelper;
global using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
global using MathNet.Numerics;
global using MathNet.Numerics.Statistics;

namespace OoplesFinance.StockIndicators.Helpers;

public static class CalculationsHelper
{
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
                movingAvgList = stockData.Calculate1LCLeastSquaresMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType._3HMA:
                movingAvgList = stockData.Calculate3HMA(length: length).CustomValuesList;
                break;
            case MovingAvgType.AdaptiveAutonomousRecursiveMovingAverage:
                movingAvgList = stockData.CalculateAdaptiveAutonomousRecursiveMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.AdaptiveExponentialMovingAverage:
                movingAvgList = stockData.CalculateAdaptiveExponentialMovingAverage(length: length).CustomValuesList;
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
                movingAvgList = stockData.CalculateAtrFilteredExponentialMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.AutoFilter:
                movingAvgList = stockData.CalculateAutoFilter(length: length).CustomValuesList;
                break;
            case MovingAvgType.AutonomousRecursiveMovingAverage:
                movingAvgList = stockData.CalculateAutonomousRecursiveMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.BryantAdaptiveMovingAverage:
                movingAvgList = stockData.CalculateBryantAdaptiveMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.CompoundRatioMovingAverage:
                movingAvgList = stockData.CalculateCompoundRatioMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.CorrectedMovingAverage:
                movingAvgList = stockData.CalculateCorrectedMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.CubedWeightedMovingAverage:
                movingAvgList = stockData.CalculateCubedWeightedMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.DoubleExponentialMovingAverage:
                movingAvgList = stockData.CalculateDoubleExponentialMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.EndPointWeightedMovingAverage:
                movingAvgList = stockData.CalculateEndPointMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.ExponentialMovingAverage:
                movingAvgList = stockData.CalculateExponentialMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.FallingRisingFilter:
                movingAvgList = stockData.CalculateFallingRisingFilter(length).CustomValuesList;
                break;
            case MovingAvgType.FareySequenceWeightedMovingAverage:
                movingAvgList = stockData.CalculateFareySequenceWeightedMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.FibonacciWeightedMovingAverage:
                movingAvgList = stockData.CalculateFibonacciWeightedMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.FisherLeastSquaresMovingAverage:
                movingAvgList = stockData.CalculateFisherLeastSquaresMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.FollowingAdaptiveMovingAverage:
                movingAvgList = stockData.CalculateEhlersMotherOfAdaptiveMovingAverages().OutputValues["Fama"];
                break;
            case MovingAvgType.FractalAdaptiveMovingAverage:
                movingAvgList = stockData.CalculateFractalAdaptiveMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.GeneralFilterEstimator:
                movingAvgList = stockData.CalculateGeneralFilterEstimator(length: length).CustomValuesList;
                break;
            case MovingAvgType.GeneralizedDoubleExponentialMovingAverage:
                movingAvgList = stockData.CalculateGeneralizedDoubleExponentialMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.HampelFilter:
                movingAvgList = stockData.CalculateHampelFilter(length: length).CustomValuesList;
                break;
            case MovingAvgType.HendersonWeightedMovingAverage:
                movingAvgList = stockData.CalculateHendersonWeightedMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.HoltExponentialMovingAverage:
                movingAvgList = stockData.CalculateHoltExponentialMovingAverage(length, length).CustomValuesList;
                break;
            case MovingAvgType.HullEstimate:
                movingAvgList = stockData.CalculateHullEstimate(length).CustomValuesList;
                break;
            case MovingAvgType.HullMovingAverage:
                movingAvgList = stockData.CalculateHullMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.HybridConvolutionFilter:
                movingAvgList = stockData.CalculateHybridConvolutionFilter(length).CustomValuesList;
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
            case MovingAvgType.KalmanSmoother:
                movingAvgList = stockData.CalculateKalmanSmoother(length).CustomValuesList;
                break;
            case MovingAvgType.KaufmanAdaptiveLeastSquaresMovingAverage:
                movingAvgList = stockData.CalculateKaufmanAdaptiveLeastSquaresMovingAverage(length: length).CustomValuesList;
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
            case MovingAvgType.McNichollMovingAverage:
                movingAvgList = stockData.CalculateMcNichollMovingAverage(length: length).CustomValuesList;
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
                movingAvgList = stockData.CalculateT3MovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.TriangularMovingAverage:
                movingAvgList = stockData.CalculateTriangularMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.Trimean:
                movingAvgList = stockData.CalculateTrimean(length).CustomValuesList;
                break;
            case MovingAvgType.TripleExponentialMovingAverage:
                movingAvgList = stockData.CalculateTripleExponentialMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.UltimateMovingAverage:
                movingAvgList = stockData.CalculateUltimateMovingAverage().CustomValuesList;
                break;
            case MovingAvgType.VariableAdaptiveMovingAverage:
                movingAvgList = stockData.CalculateVariableAdaptiveMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.VariableIndexDynamicAverage:
                movingAvgList = stockData.CalculateVariableIndexDynamicAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.VariableLengthMovingAverage:
                movingAvgList = stockData.CalculateVariableLengthMovingAverage().CustomValuesList;
                break;
            case MovingAvgType.VariableMovingAverage:
                movingAvgList = stockData.CalculateVariableMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.VerticalHorizontalMovingAverage:
                movingAvgList = stockData.CalculateVerticalHorizontalMovingAverage(length).CustomValuesList;
                break;
            case MovingAvgType.VolatilityMovingAverage:
                movingAvgList = stockData.CalculateVolatilityMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.VolatilityWaveMovingAverage:
                movingAvgList = stockData.CalculateVolatilityWaveMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.VolumeAdjustedMovingAverage:
                movingAvgList = stockData.CalculateVolumeAdjustedMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.VolumeWeightedAveragePrice:
                movingAvgList = stockData.CalculateVolumeWeightedAveragePrice().CustomValuesList;
                break;
            case MovingAvgType.VolumeWeightedMovingAverage:
                movingAvgList = stockData.CalculateVolumeWeightedMovingAverage(length: length).CustomValuesList;
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
                movingAvgList = stockData.CalculateZeroLagExponentialMovingAverage(length: length).CustomValuesList;
                break;
            case MovingAvgType.ZeroLagTripleExponentialMovingAverage:
                movingAvgList = stockData.CalculateZeroLagTripleExponentialMovingAverage(length: length).CustomValuesList;
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

        foreach (var item in result)
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

    /// <summary>
    /// Calculates the median of a sequence of doubles.
    /// </summary>
    /// <param name="sequence">The sequence to operate on.</param>
    /// <returns>The median of the sequence.</returns>
    public static decimal Median(this IEnumerable<decimal> sequence)
    {
        var list = sequence.ToList();
        var mid = (list.Count - 1) / 2;
        return list.NthOrderStatistic(mid);
    }

    /// <summary>
    /// Calculates the median of a sequence of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in sequence.</typeparam>
    /// <param name="sequence">The sequence to operate on.</param>
    /// <param name="getValue">Logic to get a double from each element.</param>
    /// <returns>The median of the sequence.</returns>
    public static decimal Median<T>(this IEnumerable<T> sequence, Func<T, decimal> getValue) => Median(sequence.Select(getValue));

    /// <summary>
    /// Gets the median member of a list of elements.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list.</typeparam>
    /// <param name="list">The list to operate on.</param>
    /// <returns>The median of the list.</returns>
    public static T Median<T>(this IList<T> list) where T : IComparable<T> => list.NthOrderStatistic((list.Count - 1) / 2);

    /// <summary>
    /// Partitions the given list around a pivot element such that all elements on left of pivot are less than or equal to pivot
    /// Elements to right of the pivot are guaranteed greater than the pivot. Can be used for sorting N-order statistics such
    /// as median finding algorithms.
    /// Pivot is selected randomly if random number generator is supplied else its selected as last element in the list.
    /// </summary>
    private static int Partition<T>(this IList<T> list, int start, int end, Random? rnd = null) where T : IComparable<T>
    {
        if (rnd != null) list.Swap(end, rnd.Next(start, end));
        var pivot = list[end];
        var lastLow = start - 1;
        for (var i = start; i < end; i++)
            if (list[i].CompareTo(pivot) <= 0) list.Swap(i, ++lastLow);
        list.Swap(end, ++lastLow);
        return lastLow;
    }

    /// <summary>
    /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
    /// Note: specified list would be mutated in the process.
    /// </summary>
    public static T NthOrderStatistic<T>(this IList<T> list, int n, Random? rnd = null) where T : IComparable<T> =>
        NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
    /// <summary>
    /// Gets Nth smallest element from a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="n"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="rnd"></param>
    /// <returns></returns>
    private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random? rnd) where T : IComparable<T>
    {
        while (true)
        {
            var pivotIndex = list.Partition(start, end, rnd);
            if (pivotIndex == n) return list[pivotIndex];
            if (n < pivotIndex) end = pivotIndex - 1;
            else start = pivotIndex + 1;
        }
    }

    /// <summary>
    /// Swap two elements positions in a list.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    /// <param name="list">The list to swap on.</param>
    /// <param name="i">The first element position to swap.</param>
    /// <param name="j">The second element position to swap.</param>
    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        if (i == j) return;
        (list[j], list[i]) = (list[i], list[j]);
    }

    /// <summary>
    /// Rescales a value between a min and a max
    /// </summary>
    /// <param name="value"></param>
    /// <param name="oldMax"></param>
    /// <param name="oldMin"></param>
    /// <param name="newMax"></param>
    /// <param name="newMin"></param>
    /// <param name="isReversed"></param>
    /// <returns></returns>
    public static decimal RescaleValue(decimal value, decimal oldMax, decimal oldMin, decimal newMax, decimal newMin, bool isReversed = false)
    {
        decimal d = isReversed ? (oldMax - value) : (value - oldMin);
        decimal dRatio = oldMax - oldMin != 0 ? d / (oldMax - oldMin) : 0;

        return (dRatio * (newMax - newMin)) + newMin;
    }
}