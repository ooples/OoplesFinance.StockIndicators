namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the standard deviation volatility.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateStandardDeviationVolatility(this StockData stockData)
    {
        return CalculateStandardDeviationVolatility(stockData, 20);
    }

    /// <summary>
    /// Calculates the standard deviation volatility.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateStandardDeviationVolatility(this StockData stockData, int length)
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
    /// Calculates the linear regression.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateLinearRegression(this StockData stockData, int length = 14)
    {
        List<decimal> slopeList = new();
        List<decimal> interceptList = new();
        List<decimal> predictedTomorrowList = new();
        List<decimal> predictedTodayList = new();
        List<decimal> xList = new();
        List<decimal> yList = new();
        List<decimal> xyList = new();
        List<decimal> x2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = yList.LastOrDefault();
            decimal currentValue = inputList.ElementAtOrDefault(i);
            yList.Add(currentValue);

            decimal x = i;
            xList.Add(x);

            decimal xy = x * currentValue;
            xyList.Add(xy);

            decimal sumX = xList.TakeLastExt(length).Sum();
            decimal sumY = yList.TakeLastExt(length).Sum();
            decimal sumXY = xyList.TakeLastExt(length).Sum();
            decimal sumX2 = x2List.TakeLastExt(length).Sum();
            decimal top = (length * sumXY) - (sumX * sumY);
            decimal bottom = (length * sumX2) - Pow(sumX, 2);

            decimal b = bottom != 0 ? top / bottom : 0;
            slopeList.Add(b);

            decimal a = (sumY - (b * sumX)) / length;
            interceptList.Add(a);

            decimal predictedToday = a + (b * x);
            predictedTodayList.Add(predictedToday);

            decimal prevPredictedNextDay = predictedTomorrowList.LastOrDefault();
            decimal predictedNextDay = a + (b * (x + 1));
            predictedTomorrowList.Add(predictedNextDay);

            var signal = GetCompareSignal(currentValue - predictedNextDay, prevValue - prevPredictedNextDay, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LinearRegression", predictedTodayList },
            { "PredictedTomorrow", predictedTomorrowList },
            { "Slope", slopeList },
            { "Intercept", interceptList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = predictedTodayList;
        stockData.IndicatorName = IndicatorName.LinearRegression;

        return stockData;
    }
}