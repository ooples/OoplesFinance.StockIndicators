namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the Standard Deviation Volatility
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateStandardDeviationVolatility(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20)
    {
        List<decimal> stdDevVolatilityList = new();
        List<decimal> deviationSquaredList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);
            decimal currentDeviation = currentValue - currentSma;

            decimal deviationSquared = Pow(currentDeviation, 2);
            deviationSquaredList.AddRounded(deviationSquared);
        }

        var divisionOfSumList = GetMovingAverageList(stockData, maType, length, deviationSquaredList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal divisionOfSum = divisionOfSumList.ElementAtOrDefault(i);

            decimal stdDevVolatility = Sqrt(divisionOfSum);
            stdDevVolatilityList.AddRounded(stdDevVolatility);
        }

        var stdDevSmaList = GetMovingAverageList(stockData, maType, length, stdDevVolatilityList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;
            decimal stdDev = stdDevVolatilityList.ElementAtOrDefault(i);
            decimal stdDevMa = stdDevSmaList.ElementAtOrDefault(i);

            Signal signal = GetVolatilitySignal(currentValue - currentSma, prevValue - prevSma, stdDev, stdDevMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "StdDev", stdDevVolatilityList },
            { "Variance", divisionOfSumList },
            { "Signal", stdDevSmaList }
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
        var stdDevLogList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
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

    /// <summary>
    /// Calculates the Relative Normalized Volatility
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="marketData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeNormalizedVolatility(this StockData stockData, StockData marketData,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> absZsrcList = new();
        List<decimal> absZspList = new();
        List<decimal> rList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (spInputList, _, _, _, _) = GetInputValuesList(marketData);

        if (stockData.Count == marketData.Count)
        {
            var emaList = GetMovingAverageList(stockData, maType, length, inputList);
            var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
            var spStdDevList = CalculateStandardDeviationVolatility(marketData, maType, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal spValue = spInputList.ElementAtOrDefault(i);
                decimal prevSpValue = i >= 1 ? spInputList.ElementAtOrDefault(i - 1) : 0;
                decimal stdDev = stdDevList.ElementAtOrDefault(i);
                decimal spStdDev = spStdDevList.ElementAtOrDefault(i);
                decimal d = currentValue - prevValue;
                decimal sp = spValue - prevSpValue;
                decimal zsrc = stdDev != 0 ? d / stdDev : 0;
                decimal zsp = spStdDev != 0 ? sp / spStdDev : 0;

                decimal absZsrc = Math.Abs(zsrc);
                absZsrcList.Add(absZsrc);

                decimal absZsp = Math.Abs(zsp);
                absZspList.Add(absZsp);
            }

            var absZsrcSmaList = GetMovingAverageList(stockData, maType, length, absZsrcList);
            var absZspSmaList = GetMovingAverageList(marketData, maType, length, absZspList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal absZsrcSma = absZsrcSmaList.ElementAtOrDefault(i);
                decimal absZspSma = absZspSmaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

                decimal r = absZspSma != 0 ? absZsrcSma / absZspSma : 0;
                rList.Add(r);

                var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, r, 1);
                signalsList.Add(signal);
            }
        }

        stockData.OutputValues = new()
        {
            { "Rnv", rList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rList;
        stockData.IndicatorName = IndicatorName.RelativeNormalizedVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the Reversal Points
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateReversalPoints(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 100)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> bSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal c = length + (length / Sqrt(length) / 2);
        int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal max = Math.Max(currentValue, prevValue);
            decimal min = Math.Min(currentValue, prevValue);

            decimal a = max - min;
            aList.Add(a);
        }

        var aEma1List = GetMovingAverageList(stockData, maType, length1, aList);
        var aEma2List = GetMovingAverageList(stockData, maType, length1, aEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal aEma1 = aEma1List.ElementAtOrDefault(i);
            decimal aEma2 = aEma2List.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal b = aEma2 != 0 ? aEma1 / aEma2 : 0;
            bList.Add(b);

            decimal bSum = bList.TakeLastExt(length).Sum();
            bSumList.Add(bSum);

            var signal = GetVolatilitySignal(currentValue - ema, prevValue - prevEma, bSum, c);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rp", bSumList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bSumList;
        stockData.IndicatorName = IndicatorName.ReversalPoints;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mayer Multiple
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static StockData CalculateMayerMultiple(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 200,
        decimal threshold = 2.4m)
    {
        List<decimal> mmList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;

            decimal mm = currentSma != 0 ? currentValue / currentSma : 0;
            mmList.Add(mm);

            var signal = GetVolatilitySignal(currentValue - currentSma, prevValue - prevSma, mm, threshold);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mm", mmList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mmList;
        stockData.IndicatorName = IndicatorName.MayerMultiple;

        return stockData;
    }

    /// <summary>
    /// Calculates the Motion Smoothness Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMotionSmoothnessIndex(this StockData stockData, int length = 50)
    {
        List<decimal> bList = new();
        List<decimal> chgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal chg = currentValue - prevValue;
            chgList.Add(chg);
        }

        stockData.CustomValuesList = chgList;
        var aChgStdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal aChgStdDev = aChgStdDevList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal b = stdDev != 0 ? aChgStdDev / stdDev : 0;
            bList.Add(b);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, b, 0.5m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Msi", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.MotionSmoothnessIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the choppiness index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateChoppinessIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<decimal> ciList = new();
        List<decimal> trList = new();
        List<Signal> signalsList = new();

        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestHighList, lowestLowList) = GetMaxAndMinValuesList(highList, lowList, length);
        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal highestHigh = highestHighList.ElementAtOrDefault(i);
            decimal lowestLow = lowestLowList.ElementAtOrDefault(i);
            decimal range = highestHigh - lowestLow;

            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            trList.Add(tr);

            decimal trSum = trList.TakeLastExt(length).Sum();
            decimal ci = range > 0 ? 100 * Log10(trSum / range) / Log10(length) : 0;
            ciList.Add(ci);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, ci, 38.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ci", ciList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ciList;
        stockData.IndicatorName = IndicatorName.ChoppinessIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ultimate Volatility Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateUltimateVolatilityIndicator(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> uviList = new();
        List<decimal> absList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentMa = maList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMa = i >= 1 ? maList.ElementAtOrDefault(i - 1) : 0;

            decimal abs = Math.Abs(currentClose - currentOpen);
            absList.Add(abs);

            decimal uvi = (decimal)1 / length * absList.TakeLastExt(length).Sum();
            uviList.Add(uvi);

            var signal = GetVolatilitySignal(currentClose - currentMa, prevClose - prevMa, uvi, 1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Uvi", uviList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = uviList;
        stockData.IndicatorName = IndicatorName.UltimateVolatilityIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Qma Sma Difference
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateQmaSmaDifference(this StockData stockData, int length = 14)
    {
        List<decimal> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var qmaList = CalculateQuadraticMovingAverage(stockData, length).CustomValuesList;
        var smaList = CalculateSimpleMovingAverage(stockData, length).CustomValuesList;
        var emaList = CalculateExponentialMovingAverage(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal qma = qmaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal prevC = cList.LastOrDefault();
            decimal c = qma - sma;
            cList.Add(c);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, c, prevC);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "QmaSmaDiff", cList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cList;
        stockData.IndicatorName = IndicatorName.QmaSmaDifference;

        return stockData;
    }

    /// <summary>
    /// Calculates the Garman Klass Volatility
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateGarmanKlassVolatility(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14, int signalLength = 7)
    {
        List<decimal> gcvList = new();
        List<decimal> logList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal logHl = currentLow != 0 ? Log(currentHigh / currentLow) : 0;
            decimal logCo = currentOpen != 0 ? Log(currentClose / currentOpen) : 0;

            decimal log = (0.5m * Pow(logHl, 2)) - (((2 * Log(2)) - 1) * Pow(logCo, 2));
            logList.Add(log);

            decimal logSum = logList.TakeLastExt(length).Sum();
            decimal gcv = length != 0 && logSum != 0 ? Sqrt((decimal)i / length * logSum) : 0;
            gcvList.Add(gcv);
        }

        var gcvWmaList = GetMovingAverageList(stockData, maType, signalLength, gcvList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var currentClose = inputList.ElementAtOrDefault(i);
            var wma = wmaList.ElementAtOrDefault(i);
            var prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            var prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;
            var gcv = gcvList.ElementAtOrDefault(i);
            var gcvWma = i >= 1 ? gcvWmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetVolatilitySignal(currentClose - wma, prevClose - prevWma, gcv, gcvWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gcv", gcvList },
            { "Signal", gcvWmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gcvList;
        stockData.IndicatorName = IndicatorName.GarmanKlassVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the Gopalakrishnan Range Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGopalakrishnanRangeIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 5)
    {
        List<decimal> gapoList = new();
        List<decimal> gapoEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal range = highestHigh - lowestLow;
            decimal rangeLog = range > 0 ? Log(range) : 0;

            decimal gapo = rangeLog / Log(length);
            gapoList.Add(gapo);
        }

        var gapoWmaList = GetMovingAverageList(stockData, maType, length, gapoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var gapoWma = gapoWmaList.ElementAtOrDefault(i);
            var prevGapoWma = i >= 1 ? gapoWmaList.ElementAtOrDefault(i - 1) : 0;
            var currentValue = inputList.ElementAtOrDefault(i);
            var prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            var currentWma = wmaList.ElementAtOrDefault(i);
            var prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetVolatilitySignal(currentValue - currentWma, prevValue - prevWma, gapoWma, prevGapoWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gapo", gapoList },
            { "Signal", gapoEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gapoList;
        stockData.IndicatorName = IndicatorName.GopalakrishnanRangeIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Historical Volatility Percentile
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="annualLength"></param>
    /// <returns></returns>
    public static StockData CalculateHistoricalVolatilityPercentile(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 21, int annualLength = 252)
    {
        List<decimal> devLogSqList = new();
        List<decimal> devLogSqAvgList = new();
        List<decimal> hvList = new();
        List<decimal> hvpList = new();
        List<decimal> tempLogList = new();
        List<decimal> stdDevLogList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal temp = prevValue != 0 ? currentValue / prevValue : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal tempLog = temp > 0 ? Log(temp) : 0;
            tempLogList.Add(tempLog);

            decimal avgLog = tempLogList.TakeLastExt(length).Average();
            decimal devLogSq = Pow(tempLog - avgLog, 2);
            devLogSqList.Add(devLogSq);

            decimal devLogSqAvg = devLogSqList.TakeLastExt(length).Sum() / (length - 1);
            decimal stdDevLog = devLogSqAvg >= 0 ? Sqrt(devLogSqAvg) : 0;

            decimal hv = stdDevLog * Sqrt(annualLength);
            hvList.Add(hv);

            decimal count = hvList.TakeLastExt(annualLength).Where(i => i < hv).Count();
            decimal hvp = count / annualLength * 100;
            hvpList.Add(hvp);
        }

        var hvpEmaList = GetMovingAverageList(stockData, maType, length, hvpList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal hvp = hvpList.ElementAtOrDefault(i);
            decimal hvpEma = hvpEmaList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, hvp, hvpEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hvp", hvpList },
            { "Signal", hvpEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hvpList;
        stockData.IndicatorName = IndicatorName.HistoricalVolatilityPercentile;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fast Z Score
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFastZScore(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 200)
    {
        List<decimal> gsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length2 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = smaList;
        var smaLinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        stockData.CustomValuesList = smaList;
        var linreg2List = CalculateLinearRegression(stockData, length2).CustomValuesList;
        stockData.CustomValuesList = smaList;
        var smaStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = smaStdDevList.ElementAtOrDefault(i);
            decimal linreg = smaLinregList.ElementAtOrDefault(i);
            decimal linreg2 = linreg2List.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;

            decimal gs = stdDev != 0 ? (linreg2 - linreg) / stdDev / 2 : 0;
            gsList.Add(gs);

            var signal = GetVolatilitySignal(currentValue - sma, prevValue - prevSma, gs, 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fzs", gsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gsList;
        stockData.IndicatorName = IndicatorName.FastZScore;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volatility Switch Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilitySwitchIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14)
    {
        List<decimal> drList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal rocSma = (currentValue + prevValue) / 2;
            decimal dr = rocSma != 0 ? (currentValue - prevValue) / rocSma : 0;
            drList.Add(dr);
        }

        stockData.CustomValuesList = drList;
        var volaList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var vswitchList = GetMovingAverageList(stockData, maType, length, volaList);
        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentWma = wmaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;
            decimal vswitch14 = vswitchList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - currentWma, prevValue - prevWma, vswitch14, 0.5m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vsi", vswitchList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vswitchList;
        stockData.IndicatorName = IndicatorName.VolatilitySwitchIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vertical Horizontal Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateVerticalHorizontalFilter(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 18, int signalLength = 6)
    {
        List<decimal> vhfList = new();
        List<decimal> changeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal highestPrice = highestList.ElementAtOrDefault(i);
            decimal lowestPrice = lowestList.ElementAtOrDefault(i);
            decimal numerator = Math.Abs(highestPrice - lowestPrice);

            decimal priceChange = Math.Abs(currentValue - prevValue);
            changeList.Add(priceChange);

            decimal denominator = changeList.TakeLastExt(length).Sum();
            decimal vhf = denominator != 0 ? numerator / denominator : 0;
            vhfList.Add(vhf);
        }

        var vhfWmaList = GetMovingAverageList(stockData, maType, signalLength, vhfList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentWma = wmaList.ElementAtOrDefault(i);
            decimal prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;
            decimal vhfWma = vhfWmaList.ElementAtOrDefault(i);
            decimal vhf = vhfList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - currentWma, prevValue - prevWma, vhf, vhfWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vhf", vhfList },
            { "Signal", vhfWmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vhfList;
        stockData.IndicatorName = IndicatorName.VerticalHorizontalFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Closed Form Distance Volatility
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateClosedFormDistanceVolatility(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<decimal> tempHighList = new();
        List<decimal> tempLowList = new();
        List<decimal> hvList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal currentHigh = highList.ElementAtOrDefault(i);
            tempHighList.Add(currentHigh);

            decimal currentLow = lowList.ElementAtOrDefault(i);
            tempLowList.Add(currentLow);

            decimal a = tempHighList.TakeLastExt(length).Sum();
            decimal b = tempLowList.TakeLastExt(length).Sum();
            decimal abAvg = (a + b) / 2;

            decimal prevHv = hvList.LastOrDefault();
            decimal hv = abAvg != 0 && a != b ? Sqrt((1 - (Pow(a, 0.25m) * Pow(b, 0.25m) / Pow(abAvg, 0.5m)))) : 0;
            hvList.Add(hv);

            var signal = GetVolatilitySignal(currentValue - ema, prevValue - prevEma, hv, prevHv);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cfdv", hvList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hvList;
        stockData.IndicatorName = IndicatorName.ClosedFormDistanceVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the Projection Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateProjectionOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14, int smoothLength = 4)
    {
        List<decimal> pboList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var projectionBandsList = CalculateProjectionBands(stockData, length);
        var puList = projectionBandsList.OutputValues["UpperBand"];
        var plList = projectionBandsList.OutputValues["LowerBand"];
        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal pl = plList.ElementAtOrDefault(i);
            decimal pu = puList.ElementAtOrDefault(i);

            decimal pbo = pu - pl != 0 ? 100 * (currentValue - pl) / (pu - pl) : 0;
            pboList.Add(pbo);
        }

        var pboSignalList = GetMovingAverageList(stockData, maType, smoothLength, pboList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pbo = pboSignalList.ElementAtOrDefault(i);
            decimal prevPbo = i >= 1 ? pboSignalList.ElementAtOrDefault(i - 1) : 0;
            decimal wma = wmaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetVolatilitySignal(currentValue - wma, prevValue - prevWma, pbo, prevPbo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pbo", pboList },
            { "Signal", pboSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pboList;
        stockData.IndicatorName = IndicatorName.ProjectionOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Projection Bandwidth
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateProjectionBandwidth(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14)
    {
        List<decimal> pbwList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var projectionBandsList = CalculateProjectionBands(stockData, length);
        var puList = projectionBandsList.OutputValues["UpperBand"];
        var plList = projectionBandsList.OutputValues["LowerBand"];
        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pu = puList.ElementAtOrDefault(i);
            decimal pl = plList.ElementAtOrDefault(i);

            decimal pbw = pu + pl != 0 ? 200 * (pu - pl) / (pu + pl) : 0;
            pbwList.Add(pbw);
        }

        var pbwSignalList = GetMovingAverageList(stockData, maType, length, pbwList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pbw = pbwList.ElementAtOrDefault(i);
            decimal pbwSignal = pbwSignalList.ElementAtOrDefault(i);
            decimal wma = wmaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetVolatilitySignal(currentValue - wma, prevValue - prevWma, pbw, pbwSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pbw", pbwList },
            { "Signal", pbwSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pbwList;
        stockData.IndicatorName = IndicatorName.ProjectionBandwidth;

        return stockData;
    }

    /// <summary>
    /// Calculates the Donchian Channel Width
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateDonchianChannelWidth(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20,
        int smoothLength = 22)
    {
        List<decimal> donchianWidthList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal upper = highestList.ElementAtOrDefault(i);
            decimal lower = lowestList.ElementAtOrDefault(i);

            decimal donchianWidth = upper - lower;
            donchianWidthList.Add(donchianWidth);
        }

        var donchianWidthSmaList = GetMovingAverageList(stockData, maType, smoothLength, donchianWidthList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;
            decimal donchianWidth = donchianWidthList.ElementAtOrDefault(i);
            decimal donchianWidthSma = donchianWidthSmaList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - currentSma, prevValue - prevSma, donchianWidth, donchianWidthSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dcw", donchianWidthList },
            { "Signal", donchianWidthSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = donchianWidthList;
        stockData.IndicatorName = IndicatorName.DonchianChannelWidth;

        return stockData;
    }

    /// <summary>
    /// Calculates the Statistical Volatility
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateStatisticalVolatility(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 30, int length2 = 253)
    {
        List<decimal> volList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList1, lowestList1) = GetMaxAndMinValuesList(inputList, length1);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(highList, lowList, length1);

        decimal annualSqrt = Sqrt((decimal)length2 / length1);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal maxC = highestList1.ElementAtOrDefault(i);
            decimal minC = lowestList1.ElementAtOrDefault(i);
            decimal maxH = highestList2.ElementAtOrDefault(i);
            decimal minL = lowestList2.ElementAtOrDefault(i);
            decimal cLog = minC != 0 ? Log(maxC / minC) : 0;
            decimal hlLog = minL != 0 ? Log(maxH / minL) : 0;

            decimal vol = MinOrMax(((0.6m * cLog * annualSqrt) + (0.6m * hlLog * annualSqrt)) * 0.5m, 2.99m, 0);
            volList.Add(vol);
        }

        var volEmaList = GetMovingAverageList(stockData, maType, length1, volList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal vol = volList.ElementAtOrDefault(i);
            decimal volEma = volEmaList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, vol, volEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sv", volList },
            { "Signal", volEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = volList;
        stockData.IndicatorName = IndicatorName.StatisticalVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the Standard Deviation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateStandardDevation(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> cList = new();
        List<decimal> powList = new();
        List<decimal> tempList = new();
        List<decimal> sumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal sum = tempList.TakeLastExt(length).Sum();
            decimal sumPow = Pow(sum, 2);
            sumList.Add(sumPow);

            decimal pow = Pow(currentValue, 2);
            powList.Add(pow);
        }

        var powSmaList = GetMovingAverageList(stockData, maType, length, powList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = powSmaList.ElementAtOrDefault(i);
            decimal sum = sumList.ElementAtOrDefault(i);
            decimal b = sum / Pow(length, 2);

            decimal c = a - b >= 0 ? Sqrt(a - b) : 0;
            cList.Add(c);
        }

        var cSmaList = GetMovingAverageList(stockData, maType, length, cList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal c = cList.ElementAtOrDefault(i);
            decimal cSma = cSmaList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, c, cSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Std", cList },
            { "Signal", cSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cList;
        stockData.IndicatorName = IndicatorName.StandardDeviation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volatility Based Momentum
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilityBasedMomentum(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length1 = 22, int length2 = 65)
    {
        List<decimal> vbmList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentAtr = atrList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length1 ? inputList.ElementAtOrDefault(i - length1) : 0;
            decimal rateOfChange = currentValue - prevValue;

            decimal vbm = currentAtr != 0 ? rateOfChange / currentAtr : 0;
            vbmList.Add(vbm);
        }

        var vbmEmaList = GetMovingAverageList(stockData, maType, length1, vbmList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vbm = vbmList.ElementAtOrDefault(i);
            decimal vbmEma = vbmEmaList.ElementAtOrDefault(i);
            decimal prevVbm = i >= 1 ? vbmList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVbmEma = i >= 1 ? vbmEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(vbm - vbmEma, prevVbm - prevVbmEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vbm", vbmList },
            { "Signal", vbmEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vbmList;
        stockData.IndicatorName = IndicatorName.VolatilityBasedMomentum;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volatility Quality Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilityQualityIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 9, int slowLength = 200)
    {
        List<decimal> vqiList = new();
        List<decimal> vqiSumList = new();
        List<decimal> vqiTList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal trueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);

            decimal prevVqiT = vqiTList.LastOrDefault();
            decimal vqiT = trueRange != 0 && currentHigh - currentLow != 0 ?
                (((currentClose - prevClose) / trueRange) + ((currentClose - currentOpen) / (currentHigh - currentLow))) * 0.5m : prevVqiT;
            vqiTList.Add(vqiT);

            decimal vqi = Math.Abs(vqiT) * ((currentClose - prevClose + (currentClose - currentOpen)) * 0.5m);
            vqiList.Add(vqi);

            decimal vqiSum = vqiList.Sum();
            vqiSumList.Add(vqiSum);
        }

        var vqiSumFastSmaList = GetMovingAverageList(stockData, maType, fastLength, vqiSumList);
        var vqiSumSlowSmaList = GetMovingAverageList(stockData, maType, slowLength, vqiSumList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vqiSum = vqiSumList.ElementAtOrDefault(i);
            decimal vqiSumFastSma = vqiSumFastSmaList.ElementAtOrDefault(i);
            decimal prevVqiSum = i >= 1 ? vqiSumList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVqiSumFastSma = i >= 1 ? vqiSumFastSmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(vqiSum - vqiSumFastSma, prevVqiSum - prevVqiSumFastSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vqi", vqiSumList },
            { "FastSignal", vqiSumFastSmaList },
            { "SlowSignal", vqiSumSlowSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vqiSumList;
        stockData.IndicatorName = IndicatorName.VolatilityQualityIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sigma Spikes
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSigmaSpikes(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20)
    {
        List<decimal> retList = new();
        List<decimal> sigmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);
        }

        stockData.CustomValuesList = retList;
        var stdList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevStd = i >= 1 ? stdList.ElementAtOrDefault(i - 1) : 0;
            decimal ret = retList.ElementAtOrDefault(i);

            decimal sigma = prevStd != 0 ? ret / prevStd : 0;
            sigmaList.Add(sigma);
        }

        var ssList = GetMovingAverageList(stockData, maType, length, sigmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ss = ssList.ElementAtOrDefault(i);
            decimal prevSs = i >= 1 ? ssList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(ss, prevSs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ss", sigmaList },
            { "Signal", ssList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sigmaList;
        stockData.IndicatorName = IndicatorName.SigmaSpikes;

        return stockData;
    }

    /// <summary>
    /// Calculates the Surface Roughness Estimator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSurfaceRoughnessEstimator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 100)
    {
        List<decimal> aList = new();
        List<decimal> corrList = new();
        List<decimal> tempList = new();
        List<decimal> prevList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            prevList.Add(prevValue);

            var corr = GoodnessOfFit.R(prevList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            decimal a = 1 - (((decimal)corr + 1) / 2);
            aList.Add(a);
        }

        var aEmaList = GetMovingAverageList(stockData, maType, length, aList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal a = aList.ElementAtOrDefault(i);
            decimal aEma = aEmaList.ElementAtOrDefault(i);

            var signal = GetVolatilitySignal(currentValue - ema, prevValue - prevEma, a, aEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sre", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.SurfaceRoughnessEstimator;

        return stockData;
    }
}