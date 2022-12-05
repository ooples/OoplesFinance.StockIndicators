//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
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
        List<double> macdList = new();
        List<double> macdHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, movingAvgType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, movingAvgType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastEma = fastEmaList[i];
            double slowEma = slowEmaList[i];

            double macd = fastEma - slowEma;
            macdList.AddRounded(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, movingAvgType, signalLength, macdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macd = macdList[i];
            double macdSignalLine = macdSignalLineList[i];

            double prevMacdHistogram = macdHistogramList.LastOrDefault();
            double macdHistogram = macd - macdSignalLine;
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
        int length5 = 14, int length6 = 16, double blueMult = 4.3m, double yellowMult = 1.4m)
    {
        List<double> macd1List = new();
        List<double> macd2List = new();
        List<double> macd3List = new();
        List<double> macd4List = new();
        List<double> macd2HistogramList = new();
        List<double> macd4HistogramList = new();
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
            double ema5 = ema5List[i];
            double ema8 = ema8List[i];
            double ema10 = ema10List[i];
            double ema14 = ema14List[i];
            double ema16 = ema16List[i];
            double ema17 = ema17List[i];

            double macd1 = ema17 - ema14;
            macd1List.AddRounded(macd1);

            double macd2 = ema17 - ema8;
            macd2List.AddRounded(macd2);

            double macd3 = ema10 - ema16;
            macd3List.AddRounded(macd3);

            double macd4 = ema5 - ema10;
            macd4List.AddRounded(macd4);
        }

        var macd1SignalLineList = GetMovingAverageList(stockData, maType, length1, macd1List);
        var macd2SignalLineList = GetMovingAverageList(stockData, maType, length1, macd2List); //-V3056
        var macd3SignalLineList = GetMovingAverageList(stockData, maType, length1, macd3List);
        var macd4SignalLineList = GetMovingAverageList(stockData, maType, length1, macd4List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macd1 = macd1List[i];
            double macd1SignalLine = macd1SignalLineList[i];
            double macd2 = macd2List[i];
            double macd2SignalLine = macd2SignalLineList[i];
            double macd3 = macd3List[i];
            double macd3SignalLine = macd3SignalLineList[i];
            double macd4 = macd4List[i];
            double macd4SignalLine = macd4SignalLineList[i];
            double macd1Histogram = macd1 - macd1SignalLine;
            double macdBlue = blueMult * macd1Histogram;

            double prevMacd2Histogram = macd2HistogramList.LastOrDefault();
            double macd2Histogram = macd2 - macd2SignalLine;
            macd2HistogramList.AddRounded(macd2Histogram);

            double macd3Histogram = macd3 - macd3SignalLine;
            double macdYellow = yellowMult * macd3Histogram;

            double prevMacd4Histogram = macd4HistogramList.LastOrDefault();
            double macd4Histogram = macd4 - macd4SignalLine;
            macd4HistogramList.AddRounded(macd4Histogram);

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
        stockData.CustomValuesList = new List<double>();
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
        List<double> macdList = new();
        List<double> macdSignalLineList = new();
        List<double> macdHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _, _) = GetInputValuesList(inputName, stockData);

        var typicalPriceZeroLagEmaList = GetMovingAverageList(stockData, MovingAvgType.ZeroLagExponentialMovingAverage, length, inputList);
        var wellesWilderHighMovingAvgList = GetMovingAverageList(stockData, maType, length, highList);
        var wellesWilderLowMovingAvgList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double hi = wellesWilderHighMovingAvgList[i];
            double lo = wellesWilderLowMovingAvgList[i];
            double mi = typicalPriceZeroLagEmaList[i];

            double macd = mi > hi ? mi - hi : mi < lo ? mi - lo : 0;
            macdList.AddRounded(macd);

            double macdSignalLine = macdList.TakeLastExt(signalLength).Average();
            macdSignalLineList.AddRounded(macdSignalLine);

            double prevMacdHistogram = macdHistogramList.LastOrDefault();
            double macdHistogram = macd - macdSignalLine;
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
        List<double> kcdList = new();
        List<Signal> signalsList = new();

        var pkList = CalculateKasePeakOscillatorV1(stockData, length1, length2).OutputValues["Pk"];
        var pkSignalList = GetMovingAverageList(stockData, maType, length3, pkList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double pk = pkList[i];
            double pkSma = pkSignalList[i];

            double prevKcd = kcdList.LastOrDefault();
            double kcd = pk - pkSma;
            kcdList.AddRounded(kcd);

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
        List<double> macdList = new();
        List<double> macdHistogramList = new();
        List<double> diff12List = new();
        List<double> diff26List = new();
        List<double> i1List = new();
        List<double> i2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var ema26List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double ema12 = emaList[i];
            double ema26 = ema26List[i];

            double diff12 = currentValue - ema12;
            diff12List.AddRounded(diff12);

            double diff26 = currentValue - ema26;
            diff26List.AddRounded(diff26);
        }

        var diff12EmaList = GetMovingAverageList(stockData, maType, fastLength, diff12List);
        var diff26EmaList = GetMovingAverageList(stockData, maType, slowLength, diff26List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ema12 = emaList[i];
            double ema26 = ema26List[i];
            double diff12Ema = diff12EmaList[i];
            double diff26Ema = diff26EmaList[i];

            double i1 = ema12 + diff12Ema;
            i1List.AddRounded(i1);

            double i2 = ema26 + diff26Ema;
            i2List.AddRounded(i2);

            double macd = i1 - i2;
            macdList.AddRounded(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, maType, signalLength, macdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macd = macdList[i];
            double macdSignalLine = macdSignalLineList[i];

            double prevMacdHistogram = macdHistogramList.LastOrDefault();
            double macdHistogram = macd - macdSignalLine;
            macdHistogramList.AddRounded(macdHistogram);

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
        List<double> tfsMobList = new();
        List<double> tfsMobHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mob1List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var mob2List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double mob1 = mob1List[i];
            double mob2 = mob2List[i];

            double tfsMob = mob1 - mob2;
            tfsMobList.AddRounded(tfsMob);
        }

        var tfsMobSignalLineList = GetMovingAverageList(stockData, maType, signalLength, tfsMobList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tfsMob = tfsMobList[i];
            double tfsMobSignalLine = tfsMobSignalLineList[i];

            double prevTfsMobHistogram = tfsMobHistogramList.LastOrDefault();
            double tfsMobHistogram = tfsMob - tfsMobSignalLine;
            tfsMobHistogramList.AddRounded(tfsMobHistogram);

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

    /// <summary>
    /// Calculates the MacZ Vwap Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="gamma"></param>
    /// <returns></returns>
    public static StockData CalculateMacZVwapIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int fastLength = 12, int slowLength = 25, int signalLength = 9, int length1 = 20, int length2 = 25, double gamma = 0.02m)
    {
        List<double> macztList = new();
        List<double> l0List = new();
        List<double> l1List = new();
        List<double> l2List = new();
        List<double> l3List = new();
        List<double> maczList = new();
        List<double> histList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);
        var zScoreList = CalculateZDistanceFromVwapIndicator(stockData, length: length1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double stdev = stdDevList[i];
            double fastMa = fastSmaList[i];
            double slowMa = slowSmaList[i];
            double zscore = zScoreList[i];

            double macd = fastMa - slowMa;
            double maczt = stdev != 0 ? zscore + (macd / stdev) : zscore;
            macztList.AddRounded(maczt);

            double prevL0 = i >= 1 ? l0List.LastOrDefault() : maczt;
            double l0 = ((1 - gamma) * maczt) + (gamma * prevL0);
            l0List.AddRounded(l0);

            double prevL1 = i >= 1 ? l1List.LastOrDefault() : maczt;
            double l1 = (-1 * gamma * l0) + prevL0 + (gamma * prevL1);
            l1List.AddRounded(l1);

            double prevL2 = i >= 1 ? l2List.LastOrDefault() : maczt;
            double l2 = (-1 * gamma * l1) + prevL1 + (gamma * prevL2);
            l2List.AddRounded(l2);

            double prevL3 = i >= 1 ? l3List.LastOrDefault() : maczt;
            double l3 = (-1 * gamma * l2) + prevL2 + (gamma * prevL3);
            l3List.AddRounded(l3);

            double macz = (l0 + (2 * l1) + (2 * l2) + l3) / 6;
            maczList.AddRounded(macz);
        }

        var maczSignalList = GetMovingAverageList(stockData, maType, signalLength, maczList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macz = maczList[i];
            double maczSignal = maczSignalList[i];

            double prevHist = histList.LastOrDefault();
            double hist = macz - maczSignal;
            histList.AddRounded(hist);

            var signal = GetCompareSignal(hist, prevHist);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macz", maczList },
            { "Signal", maczSignalList },
            { "Histogram", histList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = maczList;
        stockData.IndicatorName = IndicatorName.MacZVwapIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the MacZ Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <param name="length"></param>
    /// <param name="gamma"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateMacZIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int fastLength = 12, int slowLength = 25, int signalLength = 9, int length = 25, double gamma = 0.02m, double mult = 1)
    {
        List<double> maczList = new();
        List<double> histList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);
        var wilderMovingAvgList = GetMovingAverageList(stockData, MovingAvgType.WildersSmoothingMethod, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double stdev = stdDevList[i];
            double wima = wilderMovingAvgList[i];
            double fastMa = fastSmaList[i];
            double slowMa = slowSmaList[i];
            double zscore = stdev != 0 ? (currentValue - wima) / stdev : 0;

            double macd = fastMa - slowMa;
            double macz = stdev != 0 ? (zscore * mult) + (mult * macd / stdev) : zscore;
            maczList.AddRounded(macz);
        }

        var maczSignalList = GetMovingAverageList(stockData, maType, signalLength, maczList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macz = maczList[i];
            double maczSignal = maczSignalList[i];

            double prevHist = histList.LastOrDefault();
            double hist = macz - maczSignal;
            histList.AddRounded(hist);

            var signal = GetCompareSignal(hist, prevHist);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macz", maczList },
            { "Signal", maczSignalList },
            { "Histogram", histList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = maczList;
        stockData.IndicatorName = IndicatorName.MacZIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mirrored Moving Average Convergence Divergence
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateMirroredMovingAverageConvergenceDivergence(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20, int signalLength = 9)
    {
        List<double> macdList = new();
        List<double> macdHistogramList = new();
        List<double> macdMirrorList = new();
        List<double> macdMirrorHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        var emaOpenList = GetMovingAverageList(stockData, maType, length, openList);
        var emaCloseList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double mao = emaOpenList[i];
            double mac = emaCloseList[i];

            double macd = mac - mao;
            macdList.AddRounded(macd);

            double macdMirror = mao - mac;
            macdMirrorList.AddRounded(macdMirror);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, maType, signalLength, macdList);
        var macdMirrorSignalLineList = GetMovingAverageList(stockData, maType, signalLength, macdMirrorList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macd = macdList[i];
            double macdMirror = macdMirrorList[i];
            double macdSignalLine = macdSignalLineList[i];
            double macdMirrorSignalLine = macdMirrorSignalLineList[i];

            double prevMacdHistogram = macdHistogramList.LastOrDefault();
            double macdHistogram = macd - macdSignalLine;
            macdHistogramList.AddRounded(macdHistogram);

            double macdMirrorHistogram = macdMirror - macdMirrorSignalLine;
            macdMirrorHistogramList.AddRounded(macdMirrorHistogram);

            var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macd", macdList },
            { "Signal", macdSignalLineList },
            { "Histogram", macdHistogramList },
            { "MirrorMacd", macdMirrorList },
            { "MirrorSignal", macdMirrorSignalLineList },
            { "MirrorHistogram", macdMirrorHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = macdList;
        stockData.IndicatorName = IndicatorName.MirroredMovingAverageConvergenceDivergence;

        return stockData;
    }

    /// <summary>
    /// Calculates the DiNapoli Moving Average Convergence Divergence
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="lc"></param>
    /// <param name="sc"></param>
    /// <param name="sp"></param>
    /// <returns></returns>
    public static StockData CalculateDiNapoliMovingAverageConvergenceDivergence(this StockData stockData, double lc = 17.5185m, double sc = 8.3896m, 
        double sp = 9.0503m)
    {
        List<double> fsList = new();
        List<double> ssList = new();
        List<double> rList = new();
        List<double> sList = new();
        List<double> hList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double scAlpha = 2 / (1 + sc);
        double lcAlpha = 2 / (1 + lc);
        double spAlpha = 2 / (1 + sp);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double prevFs = fsList.LastOrDefault();
            double fs = prevFs + (scAlpha * (currentValue - prevFs));
            fsList.AddRounded(fs);

            double prevSs = ssList.LastOrDefault();
            double ss = prevSs + (lcAlpha * (currentValue - prevSs));
            ssList.AddRounded(ss);

            double r = fs - ss;
            rList.AddRounded(r);

            double prevS = sList.LastOrDefault();
            double s = prevS + (spAlpha * (r - prevS));
            sList.AddRounded(s);

            double prevH = hList.LastOrDefault();
            double h = r - s;
            hList.AddRounded(h);

            var signal = GetCompareSignal(h, prevH);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastS", fsList },
            { "SlowS", ssList },
            { "Macd", rList },
            { "Signal", sList },
            { "Histogram", hList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rList;
        stockData.IndicatorName = IndicatorName.DiNapoliMovingAverageConvergenceDivergence;

        return stockData;
    }
  
    /// <summary>
    /// Calculates the Stochastic Moving Average Convergence Divergence Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticMovingAverageConvergenceDivergenceOscillator(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 45, int fastLength = 12, int slowLength = 26, int signalLength = 9)
    {
        List<double> macdStochasticHistogramList = new();
        List<double> fastStochasticList = new();
        List<double> slowStochasticList = new();
        List<double> macdStochasticList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastEma = fastEmaList[i];
            double slowEma = slowEmaList[i];
            double hh = highestList[i];
            double ll = lowestList[i];
            double range = hh - ll;

            double fastStochastic = range != 0 ? (fastEma - ll) / range : 0;
            fastStochasticList.AddRounded(fastStochastic);

            double slowStochastic = range != 0 ? (slowEma - ll) / range : 0;
            slowStochasticList.AddRounded(slowStochastic);

            double macdStochastic = 10 * (fastStochastic - slowStochastic);
            macdStochasticList.AddRounded(macdStochastic);
        }

        var macdStochasticSignalLineList = GetMovingAverageList(stockData, maType, signalLength, macdStochasticList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macdStochastic = macdStochasticList[i];
            double macdStochasticSignalLine = macdStochasticSignalLineList[i];

            double prevMacdHistogram = macdStochasticHistogramList.LastOrDefault();
            double macdHistogram = macdStochastic - macdStochasticSignalLine;
            macdStochasticHistogramList.AddRounded(macdHistogram);

            var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Macd", macdStochasticList },
            { "Signal", macdStochasticSignalLineList },
            { "Histogram", macdStochasticHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = macdStochasticList;
        stockData.IndicatorName = IndicatorName.StochasticMovingAverageConvergenceDivergenceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ergodic Moving Average Convergence Divergence
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateErgodicMovingAverageConvergenceDivergence(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 32, int length2 = 5, int length3 = 5)
    {
        List<double> macdList = new();
        List<double> macdHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var period1EmaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var period2EmaList = GetMovingAverageList(stockData, maType, length2, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double ema1 = period1EmaList[i];
            double ema2 = period2EmaList[i];

            double macd = ema1 - ema2;
            macdList.AddRounded(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, maType, length3, macdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double macd = macdList[i];
            double macdSignalLine = macdSignalLineList[i];

            double prevMacdHistogram = macdHistogramList.LastOrDefault();
            double macdHistogram = macd - macdSignalLine;
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
        stockData.IndicatorName = IndicatorName.ErgodicMovingAverageConvergenceDivergence;

        return stockData;
    }
}
