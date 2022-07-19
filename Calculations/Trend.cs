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
        List<decimal> ttfList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal prevHighest = i >= length ? highestList[i - length] : 0;
            decimal prevLowest = i >= length ? lowestList[i - length] : 0;
            decimal buyPower = highest - prevLowest;
            decimal sellPower = prevHighest - lowest;
            decimal prevTtf1 = i >= 1 ? ttfList[i - 1] : 0;
            decimal prevTtf2 = i >= 2 ? ttfList[i - 2] : 0;

            decimal ttf = buyPower + sellPower != 0 ? 200 * (buyPower - sellPower) / (buyPower + sellPower) : 0;
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
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20, int smoothLength = 5, decimal mult = 0.01m, decimal threshold = 1)
    {
        List<decimal> ctrPList = new();
        List<decimal> ctrMList = new();
        List<decimal> tprList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevMa1 = i >= 1 ? maList[i - 1] : 0;
            decimal prevMa2 = i >= 2 ? maList[i - 2] : 0;
            decimal diff = (prevMa1 - prevMa2) / mult;

            decimal ctrP = diff > threshold ? 1 : 0;
            ctrPList.AddRounded(ctrP);

            decimal ctrM = diff < -threshold ? 1 : 0;
            ctrMList.AddRounded(ctrM);

            decimal ctrPSum = ctrPList.TakeLastExt(length).Sum();
            decimal ctrMSum = ctrMList.TakeLastExt(length).Sum();

            decimal tpr = length != 0 ? Math.Abs(100 * (ctrPSum - ctrMSum) / length) : 0;
            tprList.AddRounded(tpr);
        }

        var tprMaList = GetMovingAverageList(stockData, maType, smoothLength, tprList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tpr = tprList[i];
            decimal tprMa = tprMaList[i];
            decimal prevTpr = i >= 1 ? tprList[i - 1] : 0;
            decimal prevTprMa = i >= 1 ? tprMaList[i - 1] : 0;

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
        List<decimal> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal dev = stdDevList[i] * 2;

            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal a = i < length ? currentValue : currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue : prevA;
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
        List<decimal> teiList = new();
        List<decimal> aCountList = new();
        List<decimal> hCountList = new();
        List<decimal> aList = new();
        List<decimal> hList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, _, _, _) = GetInputValuesList(stockData);
        var (highestList, _) = GetMaxAndMinValuesList(highList, length);

        decimal sc = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevHighest = i >= 1 ? highestList[i - 1] : 0;
            decimal currentHigh = highList[i];

            decimal a = currentValue > prevValue ? 1 : 0;
            aList.AddRounded(a);

            decimal h = currentHigh > prevHighest ? 1 : 0;
            hList.AddRounded(h);

            decimal aCount = aList.Sum();
            aCountList.AddRounded(aCount);

            decimal hCount = hList.Sum();
            hCountList.AddRounded(hCount);

            decimal haRatio = aCount != 0 ? hCount / aCount : 0;
            decimal prevTei = teiList.LastOrDefault();
            decimal tei = prevTei + (sc * (haRatio - prevTei));
            teiList.AddRounded(tei);
        }

        var teiSignalList = GetMovingAverageList(stockData, maType, length, teiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tei = teiList[i];
            decimal teiSignal = teiSignalList[i];
            decimal prevTei = i >= 1 ? teiList[i - 1] : 0;
            decimal prevTeiSignal = i >= 1 ? teiSignalList[i - 1] : 0;

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
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal highest = i >= 1 ? highestList[i - 1] : 0;
            decimal lowest = i >= 1 ? lowestList[i - 1] : 0;
            decimal a = currentValue > highest || currentValue < lowest ? 1 : 0;

            decimal b = (a * currentValue) + ((1 - a) * prevB);
            bList.AddRounded(b);
        }

        var bEmaList = GetMovingAverageList(stockData, maType, length2, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal bEma = bEmaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevBEma = i >= 1 ? bEmaList[i - 1] : 0;

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
        List<decimal> taiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(smaList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];

            decimal tai = currentValue != 0 ? (highest - lowest) * 100 / currentValue : 0;
            taiList.AddRounded(tai);
        }

        var taiMaList = GetMovingAverageList(stockData, maType, length2, taiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentSma = smaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;
            decimal tai = taiList[i];
            decimal taiSma = taiMaList[i];

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
            decimal tai = taiList[i];
            decimal fastMa = fastMaList[i];
            decimal slowMa = slowMaList[i];
            decimal taiMa = taiSmaList[i];
            decimal prevFastMa = i >= 1 ? fastMaList[i - 1] : 0;
            decimal prevSlowMa = i >= 1 ? slowMaList[i - 1] : 0;

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
        decimal atrMult = 2)
    {
        List<decimal> adList = new();
        List<decimal> trndDnList = new();
        List<decimal> trndUpList = new();
        List<decimal> trndrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = atrList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal mpEma = emaList[i];
            decimal trEma = atrList[i];

            decimal ad = currentValue > prevValue ? mpEma + (trEma / 2) : currentValue < prevValue ? mpEma - (trEma / 2) : mpEma;
            adList.AddRounded(ad);
        }

        var admList = GetMovingAverageList(stockData, maType, length, adList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal adm = admList[i];
            decimal prevAdm = i >= 1 ? admList[i - 1] : 0;
            decimal mpEma = emaList[i];
            decimal prevMpEma = i >= 1 ? emaList[i - 1] : 0;
            decimal prevHigh = i >= 2 ? highList[i - 2] : 0;
            decimal prevLow = i >= 2 ? lowList[i - 2] : 0;
            decimal stdDev = stdDevList[i];

            decimal prevTrndDn = trndDnList.LastOrDefault();
            decimal trndDn = adm < mpEma && prevAdm > prevMpEma ? prevHigh : currentValue < prevValue ? currentValue + (stdDev * atrMult) : prevTrndDn;
            trndDnList.AddRounded(trndDn);

            decimal prevTrndUp = trndUpList.LastOrDefault();
            decimal trndUp = adm > mpEma && prevAdm < prevMpEma ? prevLow : currentValue > prevValue ? currentValue - (stdDev * atrMult) : prevTrndUp;
            trndUpList.AddRounded(trndUp);

            decimal prevTrndr = trndrList.LastOrDefault();
            decimal trndr = adm < mpEma ? trndDn : adm > mpEma ? trndUp : prevTrndr;
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
        List<decimal> srcList = new();
        List<decimal> absTdfList = new();
        List<decimal> tdfiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int halfLength = MinOrMax((int)Math.Ceiling((decimal)length1 / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i] * 1000;
            srcList.AddRounded(currentValue);
        }

        var ema1List = GetMovingAverageList(stockData, maType, halfLength, srcList);
        var ema2List = GetMovingAverageList(stockData, maType, halfLength, ema1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema1 = ema1List[i];
            decimal ema2 = ema2List[i];
            decimal prevEma1 = i >= 1 ? ema1List[i - 1] : 0;
            decimal prevEma2 = i >= 1 ? ema2List[i - 1] : 0;
            decimal ema1Diff = ema1 - prevEma1;
            decimal ema2Diff = ema2 - prevEma2;
            decimal emaDiffAvg = (ema1Diff + ema2Diff) / 2;

            decimal tdf;
            try
            {
                tdf = Math.Abs(ema1 - ema2) * Pow(emaDiffAvg, 3);
            }
            catch (OverflowException)
            {
                tdf = decimal.MaxValue;
            }

            decimal absTdf = Math.Abs(tdf);
            absTdfList.AddRounded(absTdf);

            decimal tdfh = absTdfList.TakeLastExt(length2).Max();
            decimal prevTdfi = tdfiList.LastOrDefault();
            decimal tdfi = tdfh != 0 ? tdf / tdfh : 0;
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
        List<decimal> tiiList = new();
        List<decimal> deviationUpList = new();
        List<decimal> deviationDownList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentSma = smaList[i];
            decimal prevTii1 = i >= 1 ? tiiList[i - 1] : 0;
            decimal prevTii2 = i >= 2 ? tiiList[i - 2] : 0;

            decimal deviationUp = currentValue > currentSma ? currentValue - currentSma : 0;
            deviationUpList.AddRounded(deviationUp);

            decimal deviationDown = currentValue < currentSma ? currentSma - currentValue : 0;
            deviationDownList.AddRounded(deviationDown);

            decimal sdPlus = deviationUpList.TakeLastExt(fastLength).Sum();
            decimal sdMinus = deviationDownList.TakeLastExt(fastLength).Sum();
            decimal tii = sdPlus + sdMinus != 0 ? sdPlus / (sdPlus + sdMinus) * 100 : 0;
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
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> cList = new();
        List<decimal> dList = new();
        List<decimal> avgList = new();
        List<decimal> oscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal ema = emaList[i];
            decimal prevEma = i >= 1 ? emaList[i - 1] : 0;
            decimal highest = i >= 1 ? highestList[i - 1] : 0;
            decimal lowest = i >= 1 ? lowestList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevA = aList.LastOrDefault();
            decimal a = currentValue > highest ? 1 : 0;
            aList.AddRounded(a);

            decimal prevB = bList.LastOrDefault();
            decimal b = currentValue < lowest ? 1 : 0;
            bList.AddRounded(b);

            decimal prevC = cList.LastOrDefault();
            decimal c = a == 1 ? prevC + 1 : b - prevB == 1 ? 0 : prevC;
            cList.AddRounded(c);

            decimal prevD = dList.LastOrDefault();
            decimal d = b == 1 ? prevD + 1 : a - prevA == 1 ? 0 : prevD;
            dList.AddRounded(d);

            decimal avg = (c + d) / 2;
            avgList.AddRounded(avg);

            decimal rmean = i != 0 ? avgList.Sum() / i : 0;
            decimal osc = avg - rmean;
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
        List<decimal> tdiList = new();
        List<decimal> momList = new();
        List<decimal> tdiDirectionList = new();
        List<decimal> momAbsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length1 ? inputList[i - length1] : 0;

            decimal mom = currentValue - prevValue;
            momList.AddRounded(mom);

            decimal momAbs = Math.Abs(mom);
            momAbsList.AddRounded(momAbs);

            decimal prevTdiDirection = tdiDirectionList.LastOrDefault();
            decimal tdiDirection = momList.TakeLastExt(length1).Sum();
            tdiDirectionList.AddRounded(tdiDirection);

            decimal momAbsSum1 = momAbsList.TakeLastExt(length1).Sum();
            decimal momAbsSum2 = momAbsList.TakeLastExt(length2).Sum();

            decimal prevTdi = tdiList.LastOrDefault();
            decimal tdi = Math.Abs(tdiDirection) - momAbsSum2 + momAbsSum1;
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
        List<decimal> tcfPlusList = new();
        List<decimal> tcfMinusList = new();
        List<decimal> cfPlusList = new();
        List<decimal> cfMinusList = new();
        List<decimal> diffPlusList = new();
        List<decimal> diffMinusList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priceChg = currentValue - prevValue;
            decimal chgPlus = priceChg > 0 ? priceChg : 0;
            decimal chgMinus = priceChg < 0 ? Math.Abs(priceChg) : 0;

            decimal prevCfPlus = cfPlusList.LastOrDefault();
            decimal cfPlus = chgPlus == 0 ? 0 : chgPlus + prevCfPlus;
            cfPlusList.AddRounded(cfPlus);

            decimal prevCfMinus = cfMinusList.LastOrDefault();
            decimal cfMinus = chgMinus == 0 ? 0 : chgMinus + prevCfMinus;
            cfMinusList.AddRounded(cfMinus);

            decimal diffPlus = chgPlus - cfMinus;
            diffPlusList.AddRounded(diffPlus);

            decimal diffMinus = chgMinus - cfPlus;
            diffMinusList.AddRounded(diffMinus);

            decimal prevTcfPlus = tcfPlusList.LastOrDefault();
            decimal tcfPlus = diffPlusList.TakeLastExt(length).Sum();
            tcfPlusList.AddRounded(tcfPlus);

            decimal prevTcfMinus = tcfMinusList.LastOrDefault();
            decimal tcfMinus = diffMinusList.TakeLastExt(length).Sum();
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
        stockData.CustomValuesList = new List<decimal>();
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
        int length = 22, decimal atrMult = 3)
    {
        List<decimal> longStopList = new();
        List<decimal> shortStopList = new();
        List<decimal> dirList = new();
        List<decimal> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentAtr = atrList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal atrValue = atrMult * currentAtr;
            decimal tempLongStop = currentValue - atrValue;
            decimal tempShortStop = currentValue + atrValue;

            decimal prevLongStop = longStopList.LastOrDefault();
            decimal longStop = prevValue > prevLongStop ? Math.Max(tempLongStop, prevLongStop) : tempLongStop;
            longStopList.AddRounded(longStop);

            decimal prevShortStop = shortStopList.LastOrDefault();
            decimal shortStop = prevValue < prevShortStop ? Math.Max(tempShortStop, prevShortStop) : tempShortStop;
            shortStopList.AddRounded(shortStop);

            decimal prevDir = dirList.LastOrDefault();
            decimal dir = prevDir == -1 && currentValue > prevShortStop ? 1 : prevDir == 1 && currentValue < prevLongStop ? -1 : prevDir;
            dirList.AddRounded(dir);

            decimal prevTrend = trendList.LastOrDefault();
            decimal trend = dir > 0 ? longStop : shortStop;
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
        List<decimal> macdList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema23List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var ema50List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma23 = ema23List[i];
            decimal currentEma50 = ema50List[i];

            decimal macd = currentEma23 - currentEma50;
            macdList.AddRounded(macd);
        }

        stockData.CustomValuesList = macdList;
        var stcList = CalculateStochasticOscillator(stockData, maType, length: cycleLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stc = stcList[i];
            decimal prevStc1 = i >= 1 ? stcList[i - 1] : 0;
            decimal prevStc2 = i >= 2 ? stcList[i - 2] : 0;

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
        List<decimal> advList = new();
        List<decimal> decList = new();
        List<decimal> advVolList = new();
        List<decimal> decVolList = new();
        List<decimal> utiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentVolume = volumeList[i];
            decimal prevUti1 = i >= 1 ? utiList[i - 1] : 0;
            decimal prevUti2 = i >= 2 ? utiList[i - 2] : 0;

            decimal adv = currentValue > prevValue ? currentValue - prevValue : 0;
            advList.AddRounded(adv);

            decimal dec = currentValue < prevValue ? prevValue - currentValue : 0;
            decList.AddRounded(dec);

            decimal advSum = advList.TakeLastExt(length).Sum();
            decimal decSum = decList.TakeLastExt(length).Sum();

            decimal advVol = currentValue > prevValue && advSum != 0 ? currentVolume / advSum : 0;
            advVolList.AddRounded(advVol);

            decimal decVol = currentValue < prevValue && decSum != 0 ? currentVolume / decSum : 0;
            decVolList.AddRounded(decVol);

            decimal advVolSum = advVolList.TakeLastExt(length).Sum();
            decimal decVolSum = decVolList.TakeLastExt(length).Sum();
            decimal top = decSum != 0 ? advSum / decSum : 0;
            decimal bot = decVolSum != 0 ? advVolSum / decVolSum : 0;
            decimal ut = bot != 0 ? top / bot : 0;

            decimal uti = ut + 1 != 0 ? (ut - 1) / (ut + 1) : 0;
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
        List<decimal> absApEsaList = new();
        List<decimal> ciList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ap = inputList[i];
            decimal esa = emaList[i];

            decimal absApEsa = Math.Abs(ap - esa);
            absApEsaList.AddRounded(absApEsa);
        }

        var dList = GetMovingAverageList(stockData, maType, length1, absApEsaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ap = inputList[i];
            decimal esa = emaList[i];
            decimal d = dList[i];

            decimal ci = d != 0 ? (ap - esa) / (0.015m * d) : 0;
            ciList.AddRounded(ci);
        }

        var tciList = GetMovingAverageList(stockData, maType, length2, ciList);
        var wt2List = GetMovingAverageList(stockData, maType, smoothLength, tciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tci = tciList[i];
            decimal wt2 = wt2List[i];
            decimal prevTci = i >= 1 ? tciList[i - 1] : 0;
            decimal prevWt2 = i >= 1 ? wt2List[i - 1] : 0;

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
        int length = 2, decimal percent = 1.4m)
    {
        List<decimal> longStopList = new();
        List<decimal> shortStopList = new();
        List<decimal> ottList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ma = maList[i];
            decimal fark = ma * percent * 0.01m;

            decimal prevLongStop = longStopList.LastOrDefault();
            decimal longStop = ma - fark;
            longStop = ma > prevLongStop ? Math.Max(longStop, prevLongStop) : longStop;
            longStopList.AddRounded(longStop);

            decimal prevShortStop = shortStopList.LastOrDefault();
            decimal shortStop = ma + fark;
            shortStopList.AddRounded(shortStop);

            decimal prevOtt = ottList.LastOrDefault();
            decimal mt = ma > prevShortStop ? longStop : ma < prevLongStop ? shortStop : 0;
            decimal ott = ma > mt ? mt * (200 + percent) / 200 : mt * (200 - percent) / 200;
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
        List<decimal> gannTrendOscillatorList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];
            decimal prevHighest1 = i >= 1 ? highestList[i - 1] : 0;
            decimal prevLowest1 = i >= 1 ? lowestList[i - 1] : 0;
            decimal prevHighest2 = i >= 2 ? highestList[i - 2] : 0;
            decimal prevLowest2 = i >= 2 ? lowestList[i - 2] : 0;

            decimal prevGto = gannTrendOscillatorList.LastOrDefault();
            decimal gto = prevHighest2 > prevHighest1 && highestHigh > prevHighest1 ? 1 : prevLowest2 < prevLowest1 && lowestLow < prevLowest1 ? -1 : prevGto;
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
    public static StockData CalculateGrandTrendForecasting(this StockData stockData, int length = 100, int forecastLength = 200, decimal mult = 2)
    {
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<decimal> tList = new();
        List<decimal> trendList = new();
        List<decimal> chgList = new();
        List<decimal> fcastList = new();
        List<decimal> diffList = new();
        List<decimal> bullSlopeList = new();
        List<decimal> bearSlopeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevT = i >= length ? tList[i - length] : currentValue;
            decimal priorT = i >= forecastLength ? tList[i - forecastLength] : 0;
            decimal prevFcast = i >= forecastLength ? fcastList[i - forecastLength] : 0;
            decimal prevChg = i >= length ? chgList[i - length] : currentValue;

            decimal chg = 0.9m * prevT;
            chgList.AddRounded(chg);

            decimal t = (0.9m * prevT) + (0.1m * currentValue) + (chg - prevChg);
            tList.AddRounded(t);

            decimal trend = tList.TakeLastExt(length).Average();
            trendList.AddRounded(trend);

            decimal fcast = t + (t - priorT);
            fcastList.AddRounded(fcast);

            decimal diff = Math.Abs(currentValue - prevFcast);
            diffList.AddRounded(diff);

            decimal diffSma = diffList.TakeLastExt(forecastLength).Average();
            decimal dev = diffSma * mult;

            decimal upper = fcast + dev;
            upperList.AddRounded(upper);

            decimal lower = fcast - dev;
            lowerList.AddRounded(lower);

            decimal prevBullSlope = bullSlopeList.LastOrDefault();
            decimal bullSlope = currentValue - Math.Max(fcast, Math.Max(t, trend));
            bullSlopeList.AddRounded(bullSlope);

            decimal prevBearSlope = bearSlopeList.LastOrDefault();
            decimal bearSlope = currentValue - Math.Min(fcast, Math.Min(t, trend));
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
    public static StockData CalculateCoralTrendIndicator(this StockData stockData, int length = 21, decimal cd = 0.4m)
    {
        List<decimal> i1List = new();
        List<decimal> i2List = new();
        List<decimal> i3List = new();
        List<decimal> i4List = new();
        List<decimal> i5List = new();
        List<decimal> i6List = new();
        List<decimal> bfrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal di = ((decimal)(length - 1) / 2) + 1;
        decimal c1 = 2 / (di + 1);
        decimal c2 = 1 - c1;
        decimal c3 = 3 * ((cd * cd) + (cd * cd * cd));
        decimal c4 = -3 * ((2 * cd * cd) + cd + (cd * cd * cd));
        decimal c5 = (3 * cd) + 1 + (cd * cd * cd) + (3 * cd * cd);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevI1 = i1List.LastOrDefault();
            decimal i1 = (c1 * currentValue) + (c2 * prevI1);
            i1List.AddRounded(i1);

            decimal prevI2 = i2List.LastOrDefault();
            decimal i2 = (c1 * i1) + (c2 * prevI2);
            i2List.AddRounded(i2);

            decimal prevI3 = i3List.LastOrDefault();
            decimal i3 = (c1 * i2) + (c2 * prevI3);
            i3List.AddRounded(i3);

            decimal prevI4 = i4List.LastOrDefault();
            decimal i4 = (c1 * i3) + (c2 * prevI4);
            i4List.AddRounded(i4);

            decimal prevI5 = i5List.LastOrDefault();
            decimal i5 = (c1 * i4) + (c2 * prevI5);
            i5List.AddRounded(i5);

            decimal prevI6 = i6List.LastOrDefault();
            decimal i6 = (c1 * i5) + (c2 * prevI6);
            i6List.AddRounded(i6);

            decimal prevBfr = bfrList.LastOrDefault();
            decimal bfr = (-1 * cd * cd * cd * i6) + (c3 * i5) + (c4 * i4) + (c5 * i3);
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
        List<decimal> priceVolumeTrendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevPvt = priceVolumeTrendList.LastOrDefault();
            decimal pvt = prevValue != 0 ? prevPvt + (currentVolume * ((currentValue - prevValue) / prevValue)) : prevPvt;
            priceVolumeTrendList.AddRounded(pvt);
        }

        var pvtEmaList = GetMovingAverageList(stockData, maType, length, priceVolumeTrendList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pvt = priceVolumeTrendList[i];
            decimal pvtEma = pvtEmaList[i];
            decimal prevPvt = i >= 1 ? priceVolumeTrendList[i - 1] : 0;
            decimal prevPvtEma = i >= 1 ? pvtEmaList[i - 1] : 0;

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
    public static StockData CalculatePercentageTrend(this StockData stockData, int length = 20, decimal pct = 0.15m)
    {
        List<decimal> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentValue = inputList[i];

            int period = 0;
            decimal prevTrend = trendList.LastOrDefault();
            decimal trend = currentValue;
            for (int j = 1; j <= length; j++)
            {
                decimal prevC = i >= j - 1 ? inputList[i - (j - 1)] : 0;
                decimal currC = i >= j ? inputList[i - j] : 0;
                period = (prevC <= trend && currC > trend) || (prevC >= trend && currC < trend) ? 0 : period;

                decimal highest1 = currC, lowest1 = currC;
                for (int k = j - period; k <= j; k++)
                {
                    decimal c = i >= j - k ? inputList[i - (j - k)] : 0;
                    highest1 = Math.Max(highest1, c);
                    lowest1 = Math.Min(lowest1, c);
                }

                decimal highest2 = currC, lowest2 = currC;
                for (int k = i - length; k <= j; k++)
                {
                    decimal c = i >= j - k ? inputList[i - (j - k)] : 0;
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
        List<decimal> mpvtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentVolume = volumeList[i];
            decimal rv = currentVolume / 50000;

            decimal prevMpvt = mpvtList.LastOrDefault();
            decimal mpvt = prevValue != 0 ? prevMpvt + (rv * (currentValue - prevValue) / prevValue) : 0;
            mpvtList.AddRounded(mpvt);
        }

        var mpvtSignalList = GetMovingAverageList(stockData, maType, length, mpvtList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mpvt = mpvtList[i];
            decimal mpvtSignal = mpvtSignalList[i];
            decimal prevMpvt = i >= 1 ? mpvtList[i - 1] : 0;
            decimal prevMpvtSignal = i >= 1 ? mpvtSignalList[i - 1] : 0;

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
