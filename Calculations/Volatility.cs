namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the standard deviation volatility.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateStandardDeviationVolatility(this StockData stockData, int length = 20)
    {
        List<decimal> stdDevVolatilityList = new();
        List<decimal> deviationSquaredList = new();
        List<decimal> divisionOfSumList = new();
        List<decimal> stdDevEmaList = new();
        List<Signal> signalsList = new();

        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var smaList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length, inputList);
        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal avgPrice = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal currentDeviation = currentValue - avgPrice;

            decimal deviationSquared = Pow(currentDeviation, 2);
            deviationSquaredList.AddRounded(deviationSquared);

            decimal divisionOfSum = deviationSquaredList.TakeLastExt(length).Average();
            divisionOfSumList.AddRounded(divisionOfSum);

            decimal stdDevVolatility = divisionOfSum >= 0 ? Sqrt(divisionOfSum) : 0;
            stdDevVolatilityList.AddRounded(stdDevVolatility);

            decimal stdDevEma = CalculateEMA(stdDevVolatility, stdDevEmaList.LastOrDefault(), length);
            stdDevEmaList.AddRounded(stdDevEma);

            Signal signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, stdDevVolatility, stdDevEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "StdDev", stdDevVolatilityList },
            { "Variance", divisionOfSumList },
            { "Signal", stdDevEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stdDevVolatilityList;
        stockData.IndicatorName = IndicatorName.StandardDeviationVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the historical volatility.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateHistoricalVolatility(this StockData stockData, MovingAvgType maType, int length = 20)
    {
        List<decimal> hvList = new();
        List<decimal> tempLogList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal annualSqrt = Sqrt(365);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal temp = prevValue != 0 ? currentValue / prevValue : 0;

            decimal tempLog = temp > 0 ? Log(temp) : 0;
            tempLogList.Add(tempLog);
        }

        stockData.CustomValuesList = tempLogList;
        var stdDevLogList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            var stdDevLog = stdDevLogList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevHv = hvList.LastOrDefault();
            decimal hv = 100 * stdDevLog * annualSqrt;
            hvList.Add(hv);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, hv, prevHv);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hv", hvList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hvList;
        stockData.IndicatorName = IndicatorName.HistoricalVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average BandWidth
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageBandWidth(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 10, int slowLength = 50, decimal mult = 1)
    {
        List<decimal> mabwList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mabList = CalculateMovingAverageBands(stockData, maType, fastLength, slowLength, mult);
        var ubList = mabList.OutputValues["UpperBand"];
        var lbList = mabList.OutputValues["LowerBand"];
        var maList = mabList.OutputValues["MiddleBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mb = maList.ElementAtOrDefault(i);
            decimal ub = ubList.ElementAtOrDefault(i);
            decimal lb = lbList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMb = i >= 1 ? maList.ElementAtOrDefault(i - 1) : 0;
            decimal prevUb = i >= 1 ? ubList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLb = i >= 1 ? lbList.ElementAtOrDefault(i - 1) : 0;

            decimal mabw = mb != 0 ? (ub - lb) / mb * 100 : 0;
            mabwList.Add(mabw);

            var signal = GetBollingerBandsSignal(currentValue - mb, prevValue - prevMb, currentValue, prevValue, ub, prevUb, lb, prevLb);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mabw", mabwList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mabwList;
        stockData.IndicatorName = IndicatorName.MovingAverageBandWidth;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Adaptive Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="filter"></param>
    /// <param name="fastAlpha"></param>
    /// <param name="slowAlpha"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageAdaptiveFilter(this StockData stockData, int length = 10, decimal filter = 0.15m, 
        decimal fastAlpha = 0.667m, decimal slowAlpha = 0.0645m)
    {
        List<decimal> amaList = new();
        List<decimal> amaDiffList = new();
        List<decimal> maafList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];
        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevAma = i >= 1 ? amaList.ElementAtOrDefault(i - 1) : currentValue;
            decimal er = erList.ElementAtOrDefault(i);
            decimal sm = Pow((er * (fastAlpha - slowAlpha)) + slowAlpha, 2);

            decimal ama = prevAma + (sm * (currentValue - prevAma));
            amaList.Add(ama);

            decimal amaDiff = ama - prevAma;
            amaDiffList.Add(amaDiff);
        }

        stockData.CustomValuesList = amaDiffList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal prevMaaf = maafList.LastOrDefault();
            decimal maaf = stdDev * filter;
            maafList.Add(maaf);

            var signal = GetVolatilitySignal(currentValue - ema, prevValue - prevEma, maaf, prevMaaf);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Maaf", maafList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = maafList;
        stockData.IndicatorName = IndicatorName.MovingAverageAdaptiveFilter;

        return stockData;
    }
}