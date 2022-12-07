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
    /// Calculates the Trend Trigger Factor
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrendTriggerFactor(this StockData stockData, int length = 15)
    {
        List<double> ttfList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double prevHighest = i >= length ? highestList[i - length] : 0;
            double prevLowest = i >= length ? lowestList[i - length] : 0;
            double buyPower = highest - prevLowest;
            double sellPower = prevHighest - lowest;
            double prevTtf1 = i >= 1 ? ttfList[i - 1] : 0;
            double prevTtf2 = i >= 2 ? ttfList[i - 2] : 0;

            double ttf = buyPower + sellPower != 0 ? 200 * (buyPower - sellPower) / (buyPower + sellPower) : 0;
            ttfList.AddRounded(ttf);

            var signal = GetRsiSignal(ttf - prevTtf1, prevTtf1 - prevTtf2, ttf, prevTtf1, 100, -100);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ttf", ttfList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ttfList;
        stockData.IndicatorName = IndicatorName.TrendTriggerFactor;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Persistence Rate
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <param name="mult"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static StockData CalculateTrendPersistenceRate(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20, int smoothLength = 5, double mult = 0.01, double threshold = 1)
    {
        List<double> ctrPList = new();
        List<double> ctrMList = new();
        List<double> tprList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevMa1 = i >= 1 ? maList[i - 1] : 0;
            double prevMa2 = i >= 2 ? maList[i - 2] : 0;
            double diff = (prevMa1 - prevMa2) / mult;

            double ctrP = diff > threshold ? 1 : 0;
            ctrPList.AddRounded(ctrP);

            double ctrM = diff < -threshold ? 1 : 0;
            ctrMList.AddRounded(ctrM);

            double ctrPSum = ctrPList.TakeLastExt(length).Sum();
            double ctrMSum = ctrMList.TakeLastExt(length).Sum();

            double tpr = length != 0 ? Math.Abs(100 * (ctrPSum - ctrMSum) / length) : 0;
            tprList.AddRounded(tpr);
        }

        var tprMaList = GetMovingAverageList(stockData, maType, smoothLength, tprList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tpr = tprList[i];
            double tprMa = tprMaList[i];
            double prevTpr = i >= 1 ? tprList[i - 1] : 0;
            double prevTprMa = i >= 1 ? tprMaList[i - 1] : 0;

            var signal = GetCompareSignal(tpr - tprMa, prevTpr - prevTprMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tpr", tprList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tprList;
        stockData.IndicatorName = IndicatorName.TrendPersistenceRate;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Step
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrendStep(this StockData stockData, int length = 50)
    {
        List<double> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double dev = stdDevList[i] * 2;

            double prevA = i >= 1 ? aList[i - 1] : currentValue;
            double a = i < length ? currentValue : currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue : prevA;
            aList.AddRounded(a);

            var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ts", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.TrendStep;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Exhaustion Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrendExhaustionIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 10)
    {
        List<double> teiList = new();
        List<double> aCountList = new();
        List<double> hCountList = new();
        List<double> aList = new();
        List<double> hList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, _, _, _) = GetInputValuesList(stockData);
        var (highestList, _) = GetMaxAndMinValuesList(highList, length);

        double sc = (double)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevHighest = i >= 1 ? highestList[i - 1] : 0;
            double currentHigh = highList[i];

            double a = currentValue > prevValue ? 1 : 0;
            aList.AddRounded(a);

            double h = currentHigh > prevHighest ? 1 : 0;
            hList.AddRounded(h);

            double aCount = aList.Sum();
            aCountList.AddRounded(aCount);

            double hCount = hList.Sum();
            hCountList.AddRounded(hCount);

            double haRatio = aCount != 0 ? hCount / aCount : 0;
            double prevTei = teiList.LastOrDefault();
            double tei = prevTei + (sc * (haRatio - prevTei));
            teiList.AddRounded(tei);
        }

        var teiSignalList = GetMovingAverageList(stockData, maType, length, teiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tei = teiList[i];
            double teiSignal = teiSignalList[i];
            double prevTei = i >= 1 ? teiList[i - 1] : 0;
            double prevTeiSignal = i >= 1 ? teiSignalList[i - 1] : 0;

            var signal = GetCompareSignal(tei - teiSignal, prevTei - prevTeiSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tei", teiList },
            { "Signal", teiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = teiList;
        stockData.IndicatorName = IndicatorName.TrendExhaustionIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Impulse Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTrendImpulseFilter(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 100, int length2 = 10)
    {
        List<double> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevB = i >= 1 ? bList[i - 1] : currentValue;
            double highest = i >= 1 ? highestList[i - 1] : 0;
            double lowest = i >= 1 ? lowestList[i - 1] : 0;
            double a = currentValue > highest || currentValue < lowest ? 1 : 0;

            double b = (a * currentValue) + ((1 - a) * prevB);
            bList.AddRounded(b);
        }

        var bEmaList = GetMovingAverageList(stockData, maType, length2, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double bEma = bEmaList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevBEma = i >= 1 ? bEmaList[i - 1] : 0;

            var signal = GetCompareSignal(currentValue - bEma, prevValue - prevBEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tif", bEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bEmaList;
        stockData.IndicatorName = IndicatorName.TrendImpulseFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Analysis Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTrendAnalysisIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 28, int length2 = 5)
    {
        List<double> taiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(smaList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];

            double tai = currentValue != 0 ? (highest - lowest) * 100 / currentValue : 0;
            taiList.AddRounded(tai);
        }

        var taiMaList = GetMovingAverageList(stockData, maType, length2, taiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentSma = smaList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevSma = i >= 1 ? smaList[i - 1] : 0;
            double tai = taiList[i];
            double taiSma = taiMaList[i];

            var signal = GetVolatilitySignal(currentValue - currentSma, prevValue - prevSma, tai, taiSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tai", taiList },
            { "Signal", taiMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = taiList;
        stockData.IndicatorName = IndicatorName.TrendAnalysisIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Analysis Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTrendAnalysisIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 21, int length2 = 4)
    {
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var slowMaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var fastMaList = GetMovingAverageList(stockData, maType, length2, inputList);
        stockData.CustomValuesList = slowMaList;
        var taiList = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        var taiSmaList = GetMovingAverageList(stockData, maType, length1, taiList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double tai = taiList[i];
            double fastMa = fastMaList[i];
            double slowMa = slowMaList[i];
            double taiMa = taiSmaList[i];
            double prevFastMa = i >= 1 ? fastMaList[i - 1] : 0;
            double prevSlowMa = i >= 1 ? slowMaList[i - 1] : 0;

            var signal = GetVolatilitySignal(fastMa - slowMa, prevFastMa - prevSlowMa, tai, taiMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tai", taiList },
            { "Signal", taiSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = taiList;
        stockData.IndicatorName = IndicatorName.TrendAnalysisIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trender
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="atrMult"></param>
    /// <returns></returns>
    public static StockData CalculateTrender(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14,
        double atrMult = 2)
    {
        List<double> adList = new();
        List<double> trndDnList = new();
        List<double> trndUpList = new();
        List<double> trndrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = atrList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double mpEma = emaList[i];
            double trEma = atrList[i];

            double ad = currentValue > prevValue ? mpEma + (trEma / 2) : currentValue < prevValue ? mpEma - (trEma / 2) : mpEma;
            adList.AddRounded(ad);
        }

        var admList = GetMovingAverageList(stockData, maType, length, adList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double adm = admList[i];
            double prevAdm = i >= 1 ? admList[i - 1] : 0;
            double mpEma = emaList[i];
            double prevMpEma = i >= 1 ? emaList[i - 1] : 0;
            double prevHigh = i >= 2 ? highList[i - 2] : 0;
            double prevLow = i >= 2 ? lowList[i - 2] : 0;
            double stdDev = stdDevList[i];

            double prevTrndDn = trndDnList.LastOrDefault();
            double trndDn = adm < mpEma && prevAdm > prevMpEma ? prevHigh : currentValue < prevValue ? currentValue + (stdDev * atrMult) : prevTrndDn;
            trndDnList.AddRounded(trndDn);

            double prevTrndUp = trndUpList.LastOrDefault();
            double trndUp = adm > mpEma && prevAdm < prevMpEma ? prevLow : currentValue > prevValue ? currentValue - (stdDev * atrMult) : prevTrndUp;
            trndUpList.AddRounded(trndUp);

            double prevTrndr = trndrList.LastOrDefault();
            double trndr = adm < mpEma ? trndDn : adm > mpEma ? trndUp : prevTrndr;
            trndrList.AddRounded(trndr);

            var signal = GetCompareSignal(currentValue - trndr, prevValue - prevTrndr);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "TrendUp", trndUpList },
            { "TrendDn", trndDnList },
            { "Trender", trndrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trndrList;
        stockData.IndicatorName = IndicatorName.Trender;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Direction Force Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTrendDirectionForceIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 10, int length2 = 30)
    {
        List<double> srcList = new();
        List<double> absTdfList = new();
        List<double> tdfiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int halfLength = MinOrMax((int)Math.Ceiling((double)length1 / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i] * 1000;
            srcList.AddRounded(currentValue);
        }

        var ema1List = GetMovingAverageList(stockData, maType, halfLength, srcList);
        var ema2List = GetMovingAverageList(stockData, maType, halfLength, ema1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ema1 = ema1List[i];
            double ema2 = ema2List[i];
            double prevEma1 = i >= 1 ? ema1List[i - 1] : 0;
            double prevEma2 = i >= 1 ? ema2List[i - 1] : 0;
            double ema1Diff = ema1 - prevEma1;
            double ema2Diff = ema2 - prevEma2;
            double emaDiffAvg = (ema1Diff + ema2Diff) / 2;

            double tdf;
            try
            {
                tdf = Math.Abs(ema1 - ema2) * Pow(emaDiffAvg, 3);
            }
            catch (OverflowException)
            {
                tdf = double.MaxValue;
            }

            double absTdf = Math.Abs(tdf);
            absTdfList.AddRounded(absTdf);

            double tdfh = absTdfList.TakeLastExt(length2).Max();
            double prevTdfi = tdfiList.LastOrDefault();
            double tdfi = tdfh != 0 ? tdf / tdfh : 0;
            tdfiList.AddRounded(tdfi);

            var signal = GetCompareSignal(tdfi, prevTdfi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tdfi", tdfiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tdfiList;
        stockData.IndicatorName = IndicatorName.TrendDirectionForceIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Intensity Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateTrendIntensityIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 30, int slowLength = 60)
    {
        List<double> tiiList = new();
        List<double> deviationUpList = new();
        List<double> deviationDownList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentSma = smaList[i];
            double prevTii1 = i >= 1 ? tiiList[i - 1] : 0;
            double prevTii2 = i >= 2 ? tiiList[i - 2] : 0;

            double deviationUp = currentValue > currentSma ? currentValue - currentSma : 0;
            deviationUpList.AddRounded(deviationUp);

            double deviationDown = currentValue < currentSma ? currentSma - currentValue : 0;
            deviationDownList.AddRounded(deviationDown);

            double sdPlus = deviationUpList.TakeLastExt(fastLength).Sum();
            double sdMinus = deviationDownList.TakeLastExt(fastLength).Sum();
            double tii = sdPlus + sdMinus != 0 ? sdPlus / (sdPlus + sdMinus) * 100 : 0;
            tiiList.AddRounded(tii);

            var signal = GetRsiSignal(tii - prevTii1, prevTii1 - prevTii2, tii, prevTii1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tii", tiiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tiiList;
        stockData.IndicatorName = IndicatorName.TrendIntensityIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Force Histogram
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrendForceHistogram(this StockData stockData, int length = 14)
    {
        List<double> aList = new();
        List<double> bList = new();
        List<double> cList = new();
        List<double> dList = new();
        List<double> avgList = new();
        List<double> oscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double ema = emaList[i];
            double prevEma = i >= 1 ? emaList[i - 1] : 0;
            double highest = i >= 1 ? highestList[i - 1] : 0;
            double lowest = i >= 1 ? lowestList[i - 1] : 0;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevA = aList.LastOrDefault();
            double a = currentValue > highest ? 1 : 0;
            aList.AddRounded(a);

            double prevB = bList.LastOrDefault();
            double b = currentValue < lowest ? 1 : 0;
            bList.AddRounded(b);

            double prevC = cList.LastOrDefault();
            double c = a == 1 ? prevC + 1 : b - prevB == 1 ? 0 : prevC;
            cList.AddRounded(c);

            double prevD = dList.LastOrDefault();
            double d = b == 1 ? prevD + 1 : a - prevA == 1 ? 0 : prevD;
            dList.AddRounded(d);

            double avg = (c + d) / 2;
            avgList.AddRounded(avg);

            double rmean = i != 0 ? avgList.Sum() / i : 0;
            double osc = avg - rmean;
            oscList.AddRounded(osc);

            var signal = GetVolatilitySignal(currentValue - ema, prevValue - prevEma, osc, 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tfh", oscList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oscList;
        stockData.IndicatorName = IndicatorName.TrendForceHistogram;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Detection Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTrendDetectionIndex(this StockData stockData, int length1 = 20, int length2 = 40)
    {
        List<double> tdiList = new();
        List<double> momList = new();
        List<double> tdiDirectionList = new();
        List<double> momAbsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length1 ? inputList[i - length1] : 0;

            double mom = MinPastValues(i, length1, currentValue - prevValue);
            momList.AddRounded(mom);

            double momAbs = Math.Abs(mom);
            momAbsList.AddRounded(momAbs);

            double prevTdiDirection = tdiDirectionList.LastOrDefault();
            double tdiDirection = momList.TakeLastExt(length1).Sum();
            tdiDirectionList.AddRounded(tdiDirection);

            double momAbsSum1 = momAbsList.TakeLastExt(length1).Sum();
            double momAbsSum2 = momAbsList.TakeLastExt(length2).Sum();

            double prevTdi = tdiList.LastOrDefault();
            double tdi = Math.Abs(tdiDirection) - momAbsSum2 + momAbsSum1;
            tdiList.AddRounded(tdi);

            var signal = GetCompareSignal(tdiDirection - tdi, prevTdiDirection - prevTdi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tdi", tdiList },
            { "TdiDirection", tdiDirectionList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tdiList;
        stockData.IndicatorName = IndicatorName.TrendDetectionIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Continuation Factor
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrendContinuationFactor(this StockData stockData, int length = 35)
    {
        List<double> tcfPlusList = new();
        List<double> tcfMinusList = new();
        List<double> cfPlusList = new();
        List<double> cfMinusList = new();
        List<double> diffPlusList = new();
        List<double> diffMinusList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double priceChg = MinPastValues(i, 1, currentValue - prevValue);
            double chgPlus = priceChg > 0 ? priceChg : 0;
            double chgMinus = priceChg < 0 ? Math.Abs(priceChg) : 0;

            double prevCfPlus = cfPlusList.LastOrDefault();
            double cfPlus = chgPlus == 0 ? 0 : chgPlus + prevCfPlus;
            cfPlusList.AddRounded(cfPlus);

            double prevCfMinus = cfMinusList.LastOrDefault();
            double cfMinus = chgMinus == 0 ? 0 : chgMinus + prevCfMinus;
            cfMinusList.AddRounded(cfMinus);

            double diffPlus = chgPlus - cfMinus;
            diffPlusList.AddRounded(diffPlus);

            double diffMinus = chgMinus - cfPlus;
            diffMinusList.AddRounded(diffMinus);

            double prevTcfPlus = tcfPlusList.LastOrDefault();
            double tcfPlus = diffPlusList.TakeLastExt(length).Sum();
            tcfPlusList.AddRounded(tcfPlus);

            double prevTcfMinus = tcfMinusList.LastOrDefault();
            double tcfMinus = diffMinusList.TakeLastExt(length).Sum();
            tcfMinusList.AddRounded(tcfMinus);

            var signal = GetCompareSignal(tcfPlus - tcfMinus, prevTcfPlus - prevTcfMinus);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "TcfPlus", tcfPlusList },
            { "TcfMinus", tcfMinusList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.TrendContinuationFactor;

        return stockData;
    }

    /// <summary>
    /// Calculates the Super Trend
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="atrMult">The atr mult.</param>
    /// <returns></returns>
    public static StockData CalculateSuperTrend(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 22, double atrMult = 3)
    {
        List<double> longStopList = new();
        List<double> shortStopList = new();
        List<double> dirList = new();
        List<double> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentAtr = atrList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double atrValue = atrMult * currentAtr;
            double tempLongStop = currentValue - atrValue;
            double tempShortStop = currentValue + atrValue;

            double prevLongStop = longStopList.LastOrDefault();
            double longStop = prevValue > prevLongStop ? Math.Max(tempLongStop, prevLongStop) : tempLongStop;
            longStopList.AddRounded(longStop);

            double prevShortStop = shortStopList.LastOrDefault();
            double shortStop = prevValue < prevShortStop ? Math.Max(tempShortStop, prevShortStop) : tempShortStop;
            shortStopList.AddRounded(shortStop);

            double prevDir = dirList.LastOrDefault();
            double dir = prevDir == -1 && currentValue > prevShortStop ? 1 : prevDir == 1 && currentValue < prevLongStop ? -1 : prevDir;
            dirList.AddRounded(dir);

            double prevTrend = trendList.LastOrDefault();
            double trend = dir > 0 ? longStop : shortStop;
            trendList.AddRounded(trend);

            var signal = GetCompareSignal(currentValue - trend, prevValue - prevTrend);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Trend", trendList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trendList;
        stockData.IndicatorName = IndicatorName.SuperTrend;

        return stockData;
    }

    /// <summary>
    /// Calculates the Schaff Trend Cycle
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="cycleLength">Length of the cycle.</param>
    /// <returns></returns>
    public static StockData CalculateSchaffTrendCycle(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 23, int slowLength = 50, int cycleLength = 10)
    {
        List<double> macdList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema23List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var ema50List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentEma23 = ema23List[i];
            double currentEma50 = ema50List[i];

            double macd = currentEma23 - currentEma50;
            macdList.AddRounded(macd);
        }

        stockData.CustomValuesList = macdList;
        var stcList = CalculateStochasticOscillator(stockData, maType, length: cycleLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double stc = stcList[i];
            double prevStc1 = i >= 1 ? stcList[i - 1] : 0;
            double prevStc2 = i >= 2 ? stcList[i - 2] : 0;

            var signal = GetRsiSignal(stc - prevStc1, prevStc1 - prevStc2, stc, prevStc1, 75, 25);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Stc", stcList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stcList;
        stockData.IndicatorName = IndicatorName.SchaffTrendCycle;

        return stockData;
    }

    /// <summary>
    /// Calculates the Uber Trend Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateUberTrendIndicator(this StockData stockData, int length = 14)
    {
        List<double> advList = new();
        List<double> decList = new();
        List<double> advVolList = new();
        List<double> decVolList = new();
        List<double> utiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentVolume = volumeList[i];
            double prevUti1 = i >= 1 ? utiList[i - 1] : 0;
            double prevUti2 = i >= 2 ? utiList[i - 2] : 0;

            double adv = i >= 1 && currentValue > prevValue ? MinPastValues(i, 1, currentValue - prevValue) : 0;
            advList.AddRounded(adv);

            double dec = i >= 1 && currentValue < prevValue ? MinPastValues(i, 1, prevValue - currentValue) : 0;
            decList.AddRounded(dec);

            double advSum = advList.TakeLastExt(length).Sum();
            double decSum = decList.TakeLastExt(length).Sum();

            double advVol = i >= 1 && currentValue > prevValue && advSum != 0 ? currentVolume / advSum : 0;
            advVolList.AddRounded(advVol);

            double decVol = i >= 1 && currentValue < prevValue && decSum != 0 ? currentVolume / decSum : 0;
            decVolList.AddRounded(decVol);

            double advVolSum = advVolList.TakeLastExt(length).Sum();
            double decVolSum = decVolList.TakeLastExt(length).Sum();
            double top = decSum != 0 ? advSum / decSum : 0;
            double bot = decVolSum != 0 ? advVolSum / decVolSum : 0;
            double ut = bot != 0 ? top / bot : 0;

            double uti = ut + 1 != 0 ? (ut - 1) / (ut + 1) : 0;
            utiList.AddRounded(uti);

            var signal = GetCompareSignal(uti - prevUti1, prevUti1 - prevUti2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Uti", utiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = utiList;
        stockData.IndicatorName = IndicatorName.UberTrendIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Wave Trend Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateWaveTrendOscillator(this StockData stockData, InputName inputName = InputName.FullTypicalPrice,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 10, int length2 = 21, int smoothLength = 4)
    {
        List<double> absApEsaList = new();
        List<double> ciList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double ap = inputList[i];
            double esa = emaList[i];

            double absApEsa = Math.Abs(ap - esa);
            absApEsaList.AddRounded(absApEsa);
        }

        var dList = GetMovingAverageList(stockData, maType, length1, absApEsaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ap = inputList[i];
            double esa = emaList[i];
            double d = dList[i];

            double ci = d != 0 ? (ap - esa) / (0.015 * d) : 0;
            ciList.AddRounded(ci);
        }

        var tciList = GetMovingAverageList(stockData, maType, length2, ciList);
        var wt2List = GetMovingAverageList(stockData, maType, smoothLength, tciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tci = tciList[i];
            double wt2 = wt2List[i];
            double prevTci = i >= 1 ? tciList[i - 1] : 0;
            double prevWt2 = i >= 1 ? wt2List[i - 1] : 0;

            var signal = GetRsiSignal(tci - wt2, prevTci - prevWt2, tci, prevTci, 53, -53);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wto", tciList },
            { "Signal", wt2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tciList;
        stockData.IndicatorName = IndicatorName.WaveTrendOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Optimized Trend Tracker
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    public static StockData CalculateOptimizedTrendTracker(this StockData stockData, MovingAvgType maType = MovingAvgType.VariableIndexDynamicAverage,
        int length = 2, double percent = 1.4)
    {
        List<double> longStopList = new();
        List<double> shortStopList = new();
        List<double> ottList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double ma = maList[i];
            double fark = ma * percent * 0.01;

            double prevLongStop = longStopList.LastOrDefault();
            double longStop = ma - fark;
            longStop = ma > prevLongStop ? Math.Max(longStop, prevLongStop) : longStop;
            longStopList.AddRounded(longStop);

            double prevShortStop = shortStopList.LastOrDefault();
            double shortStop = ma + fark;
            shortStopList.AddRounded(shortStop);

            double prevOtt = ottList.LastOrDefault();
            double mt = ma > prevShortStop ? longStop : ma < prevLongStop ? shortStop : 0;
            double ott = ma > mt ? mt * (200 + percent) / 200 : mt * (200 - percent) / 200;
            ottList.AddRounded(ott);

            var signal = GetCompareSignal(currentValue - ott, prevValue - prevOtt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ott", ottList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ottList;
        stockData.IndicatorName = IndicatorName.OptimizedTrendTracker;

        return stockData;
    }

    /// <summary>
    /// Calculates the Gann Trend Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGannTrendOscillator(this StockData stockData, int length = 3)
    {
        List<double> gannTrendOscillatorList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highestHigh = highestList[i];
            double lowestLow = lowestList[i];
            double prevHighest1 = i >= 1 ? highestList[i - 1] : 0;
            double prevLowest1 = i >= 1 ? lowestList[i - 1] : 0;
            double prevHighest2 = i >= 2 ? highestList[i - 2] : 0;
            double prevLowest2 = i >= 2 ? lowestList[i - 2] : 0;

            double prevGto = gannTrendOscillatorList.LastOrDefault();
            double gto = prevHighest2 > prevHighest1 && highestHigh > prevHighest1 ? 1 : prevLowest2 < prevLowest1 && lowestLow < prevLowest1 ? -1 : prevGto;
            gannTrendOscillatorList.AddRounded(gto);

            var signal = GetCompareSignal(gto, prevGto);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gto", gannTrendOscillatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gannTrendOscillatorList;
        stockData.IndicatorName = IndicatorName.GannTrendOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Grand Trend Forecasting
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="forecastLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateGrandTrendForecasting(this StockData stockData, int length = 100, int forecastLength = 200, double mult = 2)
    {
        List<double> upperList = new();
        List<double> lowerList = new();
        List<double> tList = new();
        List<double> trendList = new();
        List<double> chgList = new();
        List<double> fcastList = new();
        List<double> diffList = new();
        List<double> bullSlopeList = new();
        List<double> bearSlopeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevT = i >= length ? tList[i - length] : currentValue;
            double priorT = i >= forecastLength ? tList[i - forecastLength] : 0;
            double prevFcast = i >= forecastLength ? fcastList[i - forecastLength] : 0;
            double prevChg = i >= length ? chgList[i - length] : currentValue;

            double chg = 0.9 * prevT;
            chgList.AddRounded(chg);

            double t = (0.9 * prevT) + (0.1 * currentValue) + (chg - prevChg);
            tList.AddRounded(t);

            double trend = tList.TakeLastExt(length).Average();
            trendList.AddRounded(trend);

            double fcast = t + (t - priorT);
            fcastList.AddRounded(fcast);

            double diff = Math.Abs(currentValue - prevFcast);
            diffList.AddRounded(diff);

            double diffSma = diffList.TakeLastExt(forecastLength).Average();
            double dev = diffSma * mult;

            double upper = fcast + dev;
            upperList.AddRounded(upper);

            double lower = fcast - dev;
            lowerList.AddRounded(lower);

            double prevBullSlope = bullSlopeList.LastOrDefault();
            double bullSlope = currentValue - Math.Max(fcast, Math.Max(t, trend));
            bullSlopeList.AddRounded(bullSlope);

            double prevBearSlope = bearSlopeList.LastOrDefault();
            double bearSlope = currentValue - Math.Min(fcast, Math.Min(t, trend));
            bearSlopeList.AddRounded(bearSlope);

            var signal = GetBullishBearishSignal(bullSlope, prevBullSlope, bearSlope, prevBearSlope);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gtf", trendList },
            { "UpperBand", upperList },
            { "MiddleBand", fcastList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trendList;
        stockData.IndicatorName = IndicatorName.GrandTrendForecasting;

        return stockData;
    }

    /// <summary>
    /// Calculates the Coral Trend Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="cd"></param>
    /// <returns></returns>
    public static StockData CalculateCoralTrendIndicator(this StockData stockData, int length = 21, double cd = 0.4)
    {
        List<double> i1List = new();
        List<double> i2List = new();
        List<double> i3List = new();
        List<double> i4List = new();
        List<double> i5List = new();
        List<double> i6List = new();
        List<double> bfrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double di = ((double)(length - 1) / 2) + 1;
        double c1 = 2 / (di + 1);
        double c2 = 1 - c1;
        double c3 = 3 * ((cd * cd) + (cd * cd * cd));
        double c4 = -3 * ((2 * cd * cd) + cd + (cd * cd * cd));
        double c5 = (3 * cd) + 1 + (cd * cd * cd) + (3 * cd * cd);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevI1 = i1List.LastOrDefault();
            double i1 = (c1 * currentValue) + (c2 * prevI1);
            i1List.AddRounded(i1);

            double prevI2 = i2List.LastOrDefault();
            double i2 = (c1 * i1) + (c2 * prevI2);
            i2List.AddRounded(i2);

            double prevI3 = i3List.LastOrDefault();
            double i3 = (c1 * i2) + (c2 * prevI3);
            i3List.AddRounded(i3);

            double prevI4 = i4List.LastOrDefault();
            double i4 = (c1 * i3) + (c2 * prevI4);
            i4List.AddRounded(i4);

            double prevI5 = i5List.LastOrDefault();
            double i5 = (c1 * i4) + (c2 * prevI5);
            i5List.AddRounded(i5);

            double prevI6 = i6List.LastOrDefault();
            double i6 = (c1 * i5) + (c2 * prevI6);
            i6List.AddRounded(i6);

            double prevBfr = bfrList.LastOrDefault();
            double bfr = (-1 * cd * cd * cd * i6) + (c3 * i5) + (c4 * i4) + (c5 * i3);
            bfrList.AddRounded(bfr);

            var signal = GetCompareSignal(currentValue - bfr, prevValue - prevBfr);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cti", bfrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bfrList;
        stockData.IndicatorName = IndicatorName.CoralTrendIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Volume Trend
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePriceVolumeTrend(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<double> priceVolumeTrendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevPvt = priceVolumeTrendList.LastOrDefault();
            double pvt = prevValue != 0 ? prevPvt + (currentVolume * (MinPastValues(i, 1, currentValue - prevValue) / prevValue)) : prevPvt;
            priceVolumeTrendList.AddRounded(pvt);
        }

        var pvtEmaList = GetMovingAverageList(stockData, maType, length, priceVolumeTrendList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double pvt = priceVolumeTrendList[i];
            double pvtEma = pvtEmaList[i];
            double prevPvt = i >= 1 ? priceVolumeTrendList[i - 1] : 0;
            double prevPvtEma = i >= 1 ? pvtEmaList[i - 1] : 0;

            var signal = GetCompareSignal(pvt - pvtEma, prevPvt - prevPvtEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pvt", priceVolumeTrendList },
            { "Signal", pvtEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = priceVolumeTrendList;
        stockData.IndicatorName = IndicatorName.PriceVolumeTrend;

        return stockData;
    }

    /// <summary>
    /// Calculates the Percentage Trend
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="pct"></param>
    /// <returns></returns>
    public static StockData CalculatePercentageTrend(this StockData stockData, int length = 20, double pct = 0.15)
    {
        List<double> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentValue = inputList[i];

            int period = 0;
            double prevTrend = trendList.LastOrDefault();
            double trend = currentValue;
            for (int j = 1; j <= length; j++)
            {
                double prevC = i >= j - 1 ? inputList[i - (j - 1)] : 0;
                double currC = i >= j ? inputList[i - j] : 0;
                period = (prevC <= trend && currC > trend) || (prevC >= trend && currC < trend) ? 0 : period;

                double highest1 = currC, lowest1 = currC;
                for (int k = j - period; k <= j; k++)
                {
                    double c = i >= j - k ? inputList[i - (j - k)] : 0;
                    highest1 = Math.Max(highest1, c);
                    lowest1 = Math.Min(lowest1, c);
                }

                double highest2 = currC, lowest2 = currC;
                for (int k = i - length; k <= j; k++)
                {
                    double c = i >= j - k ? inputList[i - (j - k)] : 0;
                    highest2 = Math.Max(highest2, c);
                    lowest2 = Math.Min(lowest2, c);
                }

                if (period < length)
                {
                    period += 1;
                    trend = currC > trend ? highest1 * (1 - pct) : lowest1 * (1 + pct);
                }
                else
                {
                    trend = currC > trend ? highest2 * (1 - pct) : lowest2 * (1 + pct);
                }
            }
            trendList.AddRounded(trend);

            var signal = GetCompareSignal(currentValue - trend, prevValue - prevTrend);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pti", trendList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trendList;
        stockData.IndicatorName = IndicatorName.PercentageTrend;

        return stockData;
    }

    /// <summary>
    /// Calculates the Modified Price Volume Trend
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateModifiedPriceVolumeTrend(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 23)
    {
        List<double> mpvtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentVolume = volumeList[i];
            double rv = currentVolume / 50000;

            double prevMpvt = mpvtList.LastOrDefault();
            double mpvt = prevValue != 0 ? prevMpvt + (rv * MinPastValues(i, 1, currentValue - prevValue) / prevValue) : 0;
            mpvtList.AddRounded(mpvt);
        }

        var mpvtSignalList = GetMovingAverageList(stockData, maType, length, mpvtList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mpvt = mpvtList[i];
            double mpvtSignal = mpvtSignalList[i];
            double prevMpvt = i >= 1 ? mpvtList[i - 1] : 0;
            double prevMpvtSignal = i >= 1 ? mpvtSignalList[i - 1] : 0;

            var signal = GetCompareSignal(mpvt - mpvtSignal, prevMpvt - prevMpvtSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mpvt", mpvtList },
            { "Signal", mpvtSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mpvtList;
        stockData.IndicatorName = IndicatorName.ModifiedPriceVolumeTrend;

        return stockData;
    }
}
