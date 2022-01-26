namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the moving average convergence divergence.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageConvergenceDivergence(this StockData stockData)
    {
        int fastLength = 12, slowLength = 26, signalLength = 9;

        return CalculateMovingAverageConvergenceDivergence(stockData, MovingAvgType.ExponentialMovingAverage, fastLength, slowLength, signalLength);
    }

    /// <summary>
    /// Calculates the moving average convergence divergence.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="movingAvgType">Average type of the moving.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageConvergenceDivergence(this StockData stockData,
        MovingAvgType movingAvgType = MovingAvgType.ExponentialMovingAverage, int fastLength = 12, int slowLength = 26, int signalLength = 9)
    {
        List<decimal> macdList = new();
        List<decimal> macdHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, movingAvgType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, movingAvgType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal macd = fastEma - slowEma;
            macdList.AddRounded(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, movingAvgType, signalLength, macdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal macd = macdList.ElementAtOrDefault(i);
            decimal macdSignalLine = macdSignalLineList.ElementAtOrDefault(i);

            decimal prevMacdHistogram = macdHistogramList.LastOrDefault();
            decimal macdHistogram = macd - macdSignalLine;
            macdHistogramList.AddRounded(macdHistogram);

            var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macd", macdList },
            { "Signal", macdSignalLineList },
            { "Histogram", macdHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = macdList;
        stockData.IndicatorName = IndicatorName.MovingAverageConvergenceDivergence;

        return stockData;
    }

    /// <summary>
    /// Calculates the 4 Moving Average Convergence Divergence (Macd)
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="length6"></param>
    /// <param name="blueMult"></param>
    /// <param name="yellowMult"></param>
    /// <returns></returns>
    public static StockData Calculate4MovingAverageConvergenceDivergence(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 5, int length2 = 8, int length3 = 10, int length4 = 17,
        int length5 = 14, int length6 = 16, decimal blueMult = 4.3m, decimal yellowMult = 1.4m)
    {
        List<decimal> macd1List = new();
        List<decimal> macd2List = new();
        List<decimal> macd3List = new();
        List<decimal> macd4List = new();
        List<decimal> macd2HistogramList = new();
        List<decimal> macd4HistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema5List = GetMovingAverageList(stockData, maType, length1, inputList);
        var ema8List = GetMovingAverageList(stockData, maType, length2, inputList);
        var ema10List = GetMovingAverageList(stockData, maType, length3, inputList);
        var ema17List = GetMovingAverageList(stockData, maType, length4, inputList);
        var ema14List = GetMovingAverageList(stockData, maType, length5, inputList);
        var ema16List = GetMovingAverageList(stockData, maType, length6, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema5 = ema5List.ElementAtOrDefault(i);
            decimal ema8 = ema8List.ElementAtOrDefault(i);
            decimal ema10 = ema10List.ElementAtOrDefault(i);
            decimal ema14 = ema14List.ElementAtOrDefault(i);
            decimal ema16 = ema16List.ElementAtOrDefault(i);
            decimal ema17 = ema17List.ElementAtOrDefault(i);

            decimal macd1 = ema17 - ema14;
            macd1List.Add(macd1);

            decimal macd2 = ema17 - ema8;
            macd2List.Add(macd2);

            decimal macd3 = ema10 - ema16;
            macd3List.Add(macd3);

            decimal macd4 = ema5 - ema10;
            macd4List.Add(macd4);
        }

        var macd1SignalLineList = GetMovingAverageList(stockData, maType, length1, macd1List);
        var macd2SignalLineList = GetMovingAverageList(stockData, maType, length1, macd2List);
        var macd3SignalLineList = GetMovingAverageList(stockData, maType, length1, macd3List);
        var macd4SignalLineList = GetMovingAverageList(stockData, maType, length1, macd4List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal macd1 = macd1List.ElementAtOrDefault(i);
            decimal macd1SignalLine = macd1SignalLineList.ElementAtOrDefault(i);
            decimal macd2 = macd2List.ElementAtOrDefault(i);
            decimal macd2SignalLine = macd2SignalLineList.ElementAtOrDefault(i);
            decimal macd3 = macd3List.ElementAtOrDefault(i);
            decimal macd3SignalLine = macd3SignalLineList.ElementAtOrDefault(i);
            decimal macd4 = macd4List.ElementAtOrDefault(i);
            decimal macd4SignalLine = macd4SignalLineList.ElementAtOrDefault(i);
            decimal macd1Histogram = macd1 - macd1SignalLine;
            decimal macdBlue = blueMult * macd1Histogram;

            decimal prevMacd2Histogram = macd2HistogramList.LastOrDefault();
            decimal macd2Histogram = macd2 - macd2SignalLine;
            macd2HistogramList.Add(macd2Histogram);

            decimal macd3Histogram = macd3 - macd3SignalLine;
            decimal macdYellow = yellowMult * macd3Histogram;

            decimal prevMacd4Histogram = macd4HistogramList.LastOrDefault();
            decimal macd4Histogram = macd4 - macd4SignalLine;
            macd4HistogramList.Add(macd4Histogram);

            var signal = GetCompareSignal(macd4Histogram - macd2Histogram, prevMacd4Histogram - prevMacd2Histogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macd1", macd4List },
            { "Signal1", macd4SignalLineList },
            { "Histogram1", macd4HistogramList },
            { "Macd2", macd2List },
            { "Signal2", macd2SignalLineList },
            { "Histogram2", macd2HistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName._4MovingAverageConvergenceDivergence;

        return stockData;
    }

    /// <summary>
    /// Calculates the Impulse Moving Average Convergence Divergence
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateImpulseMovingAverageConvergenceDivergence(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 34, int signalLength = 9)
    {
        List<decimal> macdList = new();
        List<decimal> macdSignalLineList = new();
        List<decimal> macdHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _, _) = GetInputValuesList(inputName, stockData);

        var typicalPriceZeroLagEmaList = GetMovingAverageList(stockData, MovingAvgType.ZeroLagExponentialMovingAverage, length, inputList);
        var wellesWilderHighMovingAvgList = GetMovingAverageList(stockData, maType, length, highList);
        var wellesWilderLowMovingAvgList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hi = wellesWilderHighMovingAvgList.ElementAtOrDefault(i);
            decimal lo = wellesWilderLowMovingAvgList.ElementAtOrDefault(i);
            decimal mi = typicalPriceZeroLagEmaList.ElementAtOrDefault(i);

            decimal macd = mi > hi ? mi - hi : mi < lo ? mi - lo : 0;
            macdList.Add(macd);

            decimal macdSignalLine = macdList.TakeLastExt(signalLength).Average();
            macdSignalLineList.Add(macdSignalLine);

            decimal prevMacdHistogram = macdHistogramList.LastOrDefault();
            decimal macdHistogram = macd - macdSignalLine;
            macdHistogramList.Add(macdHistogram);

            var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macd", macdList },
            { "Signal", macdSignalLineList },
            { "Histogram", macdHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = macdList;
        stockData.IndicatorName = IndicatorName.ImpulseMovingAverageConvergenceDivergence;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Convergence Divergence
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateKaseConvergenceDivergence(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 30, int length2 = 3, int length3 = 8)
    {
        List<decimal> kcdList = new();
        List<Signal> signalsList = new();

        var pkList = CalculateKasePeakOscillatorV1(stockData, length1, length2).OutputValues["Pk"];
        var pkSignalList = GetMovingAverageList(stockData, maType, length3, pkList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pk = pkList.ElementAtOrDefault(i);
            decimal pkSma = pkSignalList.ElementAtOrDefault(i);

            decimal prevKcd = kcdList.LastOrDefault();
            decimal kcd = pk - pkSma;
            kcdList.Add(kcd);

            var signal = GetCompareSignal(kcd, prevKcd);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kcd", kcdList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kcdList;
        stockData.IndicatorName = IndicatorName.KaseConvergenceDivergence;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Convergence Divergence Leader
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageConvergenceDivergenceLeader(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int fastLength = 12, int slowLength = 26, int signalLength = 9)
    {
        List<decimal> macdList = new();
        List<decimal> macdHistogramList = new();
        List<decimal> diff12List = new();
        List<decimal> diff26List = new();
        List<decimal> i1List = new();
        List<decimal> i2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var ema26List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ema12 = emaList.ElementAtOrDefault(i);
            decimal ema26 = ema26List.ElementAtOrDefault(i);

            decimal diff12 = currentValue - ema12;
            diff12List.Add(diff12);

            decimal diff26 = currentValue - ema26;
            diff26List.Add(diff26);
        }

        var diff12EmaList = GetMovingAverageList(stockData, maType, fastLength, diff12List);
        var diff26EmaList = GetMovingAverageList(stockData, maType, slowLength, diff26List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema12 = emaList.ElementAtOrDefault(i);
            decimal ema26 = ema26List.ElementAtOrDefault(i);
            decimal diff12Ema = diff12EmaList.ElementAtOrDefault(i);
            decimal diff26Ema = diff26EmaList.ElementAtOrDefault(i);

            decimal i1 = ema12 + diff12Ema;
            i1List.Add(i1);

            decimal i2 = ema26 + diff26Ema;
            i2List.Add(i2);

            decimal macd = i1 - i2;
            macdList.Add(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, maType, signalLength, macdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal macd = macdList.ElementAtOrDefault(i);
            decimal macdSignalLine = macdSignalLineList.ElementAtOrDefault(i);

            decimal prevMacdHistogram = macdHistogramList.LastOrDefault();
            decimal macdHistogram = macd - macdSignalLine;
            macdHistogramList.Add(macdHistogram);

            var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macd", macdList },
            { "I1", i1List },
            { "I2", i2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = macdList;
        stockData.IndicatorName = IndicatorName.MovingAverageConvergenceDivergenceLeader;

        return stockData;
    }

    /// <summary>
    /// Calculates the TFS Mbo Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateTFSMboIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 25, int slowLength = 200, int signalLength = 18)
    {
        List<decimal> tfsMobList = new();
        List<decimal> tfsMobHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mob1List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var mob2List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mob1 = mob1List.ElementAtOrDefault(i);
            decimal mob2 = mob2List.ElementAtOrDefault(i);

            decimal tfsMob = mob1 - mob2;
            tfsMobList.Add(tfsMob);
        }

        var tfsMobSignalLineList = GetMovingAverageList(stockData, maType, signalLength, tfsMobList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tfsMob = tfsMobList.ElementAtOrDefault(i);
            decimal tfsMobSignalLine = tfsMobSignalLineList.ElementAtOrDefault(i);

            decimal prevTfsMobHistogram = tfsMobHistogramList.LastOrDefault();
            decimal tfsMobHistogram = tfsMob - tfsMobSignalLine;
            tfsMobHistogramList.Add(tfsMobHistogram);

            var signal = GetCompareSignal(tfsMobHistogram, prevTfsMobHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "TfsMob", tfsMobList },
            { "Signal", tfsMobSignalLineList },
            { "Histogram", tfsMobHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tfsMobList;
        stockData.IndicatorName = IndicatorName.TFSMboIndicator;

        return stockData;
    }
}
