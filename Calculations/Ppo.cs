namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the percentage price oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculatePercentagePriceOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int fastLength = 12, int slowLength = 26, int signalLength = 9)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal ppo = slowEma != 0 ? 100 * (fastEma - slowEma) / slowEma : 0;
            ppoList.Add(ppo);
        }

        var ppoSignalList = GetMovingAverageList(stockData, maType, signalLength, ppoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo = ppoList.ElementAtOrDefault(i);
            decimal ppoSignalLine = ppoSignalList.ElementAtOrDefault(i);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList },
            { "Signal", ppoSignalList },
            { "Histogram", ppoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.PercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the percentage volume oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculatePercentageVolumeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int fastLength = 12, int slowLength = 26, int signalLength = 9)
    {
        List<decimal> pvoList = new();
        List<decimal> pvoHistogramList = new();
        List<Signal> signalsList = new();
        var (_, _, _, _, volumeList) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, volumeList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal pvo = slowEma != 0 ? 100 * (fastEma - slowEma) / slowEma : 0;
            pvoList.Add(pvo);
        }

        var pvoSignalList = GetMovingAverageList(stockData, maType, signalLength, pvoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pvo = pvoList.ElementAtOrDefault(i);
            decimal pvoSignalLine = pvoSignalList.ElementAtOrDefault(i);

            decimal prevPvoHistogram = pvoHistogramList.LastOrDefault();
            decimal pvoHistogram = pvo - pvoSignalLine;
            pvoHistogramList.Add(pvoHistogram);

            var signal = GetCompareSignal(pvoHistogram, prevPvoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pvo", pvoList },
            { "Signal", pvoSignalList },
            { "Histogram", pvoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pvoList;
        stockData.IndicatorName = IndicatorName.PercentageVolumeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the 4 Percentage Price Oscillator
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
    public static StockData Calculate4PercentagePriceOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 5, int length2 = 8, int length3 = 10, int length4 = 17,
        int length5 = 14, int length6 = 16, decimal blueMult = 4.3m, decimal yellowMult = 1.4m)
    {
        List<decimal> ppo1List = new();
        List<decimal> ppo2List = new();
        List<decimal> ppo3List = new();
        List<decimal> ppo4List = new();
        List<decimal> ppo2HistogramList = new();
        List<decimal> ppo4HistogramList = new();
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
            decimal macd2 = ema17 - ema8;
            decimal macd3 = ema10 - ema16;
            decimal macd4 = ema5 - ema10;

            decimal ppo1 = ema14 != 0 ? macd1 / ema14 * 100 : 0;
            ppo1List.Add(ppo1);

            decimal ppo2 = ema8 != 0 ? macd2 / ema8 * 100 : 0;
            ppo2List.Add(ppo2);

            decimal ppo3 = ema16 != 0 ? macd3 / ema16 * 100 : 0;
            ppo3List.Add(ppo3);

            decimal ppo4 = ema10 != 0 ? macd4 / ema10 * 100 : 0;
            ppo4List.Add(ppo4);
        }

        var ppo1SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo1List);
        var ppo2SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo2List);
        var ppo3SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo3List);
        var ppo4SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo4List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo1 = ppo1List.ElementAtOrDefault(i);
            decimal ppo1SignalLine = ppo1SignalLineList.ElementAtOrDefault(i);
            decimal ppo2 = ppo2List.ElementAtOrDefault(i);
            decimal ppo2SignalLine = ppo2SignalLineList.ElementAtOrDefault(i);
            decimal ppo3 = ppo3List.ElementAtOrDefault(i);
            decimal ppo3SignalLine = ppo3SignalLineList.ElementAtOrDefault(i);
            decimal ppo4 = ppo4List.ElementAtOrDefault(i);
            decimal ppo4SignalLine = ppo4SignalLineList.ElementAtOrDefault(i);
            decimal ppo1Histogram = ppo1 - ppo1SignalLine;
            decimal ppoBlue = blueMult * ppo1Histogram;

            decimal prevPpo2Histogram = ppo2HistogramList.LastOrDefault();
            decimal ppo2Histogram = ppo2 - ppo2SignalLine;
            ppo2HistogramList.Add(ppo2Histogram);

            decimal ppo3Histogram = ppo3 - ppo3SignalLine;
            decimal ppoYellow = yellowMult * ppo3Histogram;

            decimal prevPpo4Histogram = ppo4HistogramList.LastOrDefault();
            decimal ppo4Histogram = ppo4 - ppo4SignalLine;
            ppo4HistogramList.Add(ppo4Histogram);

            decimal maxPpo = Math.Max(ppoBlue, Math.Max(ppoYellow, Math.Max(ppo2Histogram, ppo4Histogram)));
            decimal minPpo = Math.Min(ppoBlue, Math.Min(ppoYellow, Math.Min(ppo2Histogram, ppo4Histogram)));
            decimal currentPpo = (ppoBlue + ppoYellow + ppo2Histogram + ppo4Histogram) / 4;
            decimal ppoStochastic = maxPpo - minPpo != 0 ? MinOrMax((currentPpo - minPpo) / (maxPpo - minPpo) * 100, 100, 0) : 0;

            var signal = GetCompareSignal(ppo4Histogram - ppo2Histogram, prevPpo4Histogram - prevPpo2Histogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo1", ppo4List },
            { "Signal1", ppo4SignalLineList },
            { "Histogram1", ppo4HistogramList },
            { "Ppo2", ppo2List },
            { "Signal2", ppo2SignalLineList },
            { "Histogram2", ppo2HistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName._4PercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Impulse Percentage Price Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateImpulsePercentagePriceOscillator(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 34, int signalLength = 9)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoSignalLineList = new();
        List<decimal> ppoHistogramList = new();
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

            decimal ppo = mi > hi && hi != 0 ? macd / hi * 100 : mi < lo && lo != 0 ? macd / lo * 100 : 0;
            ppoList.Add(ppo);

            decimal ppoSignalLine = ppoList.TakeLastExt(signalLength).Average();
            ppoSignalLineList.Add(ppoSignalLine);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList },
            { "Signal", ppoSignalLineList },
            { "Histogram", ppoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.ImpulsePercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Percentage Price Oscillator Leader
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculatePercentagePriceOscillatorLeader(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int fastLength = 12, int slowLength = 26, int signalLength = 9)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoHistogramList = new();
        List<Signal> signalsList = new();

        var macdLeaderList = CalculateMovingAverageConvergenceDivergenceLeader(stockData, maType, fastLength, slowLength, signalLength);
        var i1List = macdLeaderList.OutputValues["I1"];
        var i2List = macdLeaderList.OutputValues["I2"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal i1 = i1List.ElementAtOrDefault(i);
            decimal i2 = i2List.ElementAtOrDefault(i);
            decimal macd = i1 - i2;

            decimal ppo = i2 != 0 ? macd / i2 * 100 : 0;
            ppoList.Add(ppo);
        }

        var ppoSignalLineList = GetMovingAverageList(stockData, maType, signalLength, ppoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo = ppoList.ElementAtOrDefault(i);
            decimal ppoSignalLine = ppoSignalLineList.ElementAtOrDefault(i);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList },
            { "Signal", ppoSignalLineList },
            { "Histogram", ppoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.PercentagePriceOscillatorLeader;

        return stockData;
    }

    /// <summary>
    /// Calculates the TFS Mbo Percentage Price Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateTFSMboPercentagePriceOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 25, int slowLength = 200, int signalLength = 18)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mob1List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var mob2List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mob1 = mob1List.ElementAtOrDefault(i);
            decimal mob2 = mob2List.ElementAtOrDefault(i);
            decimal tfsMob = mob1 - mob2;

            decimal ppo = mob2 != 0 ? tfsMob / mob2 * 100 : 0;
            ppoList.Add(ppo);
        }

        var ppoSignalLineList = GetMovingAverageList(stockData, maType, signalLength, ppoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo = ppoList.ElementAtOrDefault(i);
            decimal ppoSignalLine = ppoSignalLineList.ElementAtOrDefault(i);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList },
            { "Signal", ppoSignalLineList },
            { "Histogram", ppoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.TFSMboPercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mirrored Percentage Price Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateMirroredPercentagePriceOscillator(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20, int signalLength = 9)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoHistogramList = new();
        List<decimal> ppoMirrorList = new();
        List<decimal> ppoMirrorHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        var emaOpenList = GetMovingAverageList(stockData, maType, length, openList);
        var emaCloseList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mao = emaOpenList.ElementAtOrDefault(i);
            decimal mac = emaCloseList.ElementAtOrDefault(i);
            decimal macd = mac - mao;
            decimal macdMirror = mao - mac;

            decimal ppo = mao != 0 ? macd / mao * 100 : 0;
            ppoList.Add(ppo);

            decimal ppoMirror = mac != 0 ? macdMirror / mac * 100 : 0;
            ppoMirrorList.Add(ppoMirror);
        }

        var ppoSignalLineList = GetMovingAverageList(stockData, maType, signalLength, ppoList);
        var ppoMirrorSignalLineList = GetMovingAverageList(stockData, maType, signalLength, ppoMirrorList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo = ppoList.ElementAtOrDefault(i);
            decimal ppoSignalLine = ppoSignalLineList.ElementAtOrDefault(i);
            decimal ppoMirror = ppoMirrorList.ElementAtOrDefault(i);
            decimal ppoMirrorSignalLine = ppoMirrorSignalLineList.ElementAtOrDefault(i);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            decimal ppoMirrorHistogram = ppoMirror - ppoMirrorSignalLine;
            ppoMirrorHistogramList.Add(ppoMirrorHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList },
            { "Signal", ppoSignalLineList },
            { "Histogram", ppoHistogramList },
            { "MirrorPpo", ppoMirrorList },
            { "MirrorSignal", ppoMirrorSignalLineList },
            { "MirrorHistogram", ppoMirrorHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.MirroredPercentagePriceOscillator;

        return stockData;
    }
}
