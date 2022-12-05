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
    /// Calculates the adaptive trailing stop.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="factor">The factor.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveTrailingStop(this StockData stockData, int length = 100, double factor = 3)
    {
        List<double> upList = new();
        List<double> dnList = new();
        List<double> aList = new();
        List<double> bList = new();
        List<double> osList = new();
        List<double> tsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var perList = CalculatePoweredKaufmanAdaptiveMovingAverage(stockData, length, factor).OutputValues["Per"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double per = perList[i];

            double prevA = i >= 1 ? aList.LastOrDefault() : currentValue;
            double a = Math.Max(currentValue, prevA) - (Math.Abs(currentValue - prevA) * per);
            aList.AddRounded(a);

            double prevB = i >= 1 ? bList.LastOrDefault() : currentValue;
            double b = Math.Min(currentValue, prevB) + (Math.Abs(currentValue - prevB) * per);
            bList.AddRounded(b);

            double prevUp = upList.LastOrDefault();
            double up = a > prevA ? a : a < prevA && b < prevB ? a : prevUp;
            upList.AddRounded(up);

            double prevDn = dnList.LastOrDefault();
            double dn = b < prevB ? b : b > prevB && a > prevA ? b : prevDn;
            dnList.AddRounded(dn);

            double prevOs = osList.LastOrDefault();
            double os = up > currentValue ? 1 : dn > currentValue ? 0 : prevOs;
            osList.AddRounded(os);

            double prevTs = tsList.LastOrDefault();
            double ts = (os * dn) + ((1 - os) * up);
            tsList.AddRounded(ts);

            var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ts", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.AdaptiveTrailingStop;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive autonomous recursive trailing stop.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="gamma">The gamma.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveAutonomousRecursiveTrailingStop(this StockData stockData, int length = 14, double gamma = 3)
    {
        List<double> tsList = new();
        List<double> osList = new();
        List<double> upperList = new();
        List<double> lowerList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var aamaList = CalculateAdaptiveAutonomousRecursiveMovingAverage(stockData, length, gamma);
        var ma2List = aamaList.CustomValuesList;
        var dList = aamaList.OutputValues["D"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double ma2 = ma2List[i];
            double d = dList[i];

            double prevUpper = upperList.LastOrDefault();
            double upper = ma2 + d;
            upperList.AddRounded(upper);

            double prevLower = lowerList.LastOrDefault();
            double lower = ma2 - d;
            lowerList.AddRounded(lower);

            double prevOs = osList.LastOrDefault();
            double os = currentValue > prevUpper ? 1 : currentValue < prevLower ? 0 : prevOs;
            osList.AddRounded(os);

            double prevTs = tsList.LastOrDefault();
            double ts = (os * lower) + ((1 - os) * upper);
            tsList.AddRounded(ts);

            var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ts", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.AdaptiveAutonomousRecursiveTrailingStop;

        return stockData;
    }

    /// <summary>
    /// Calculates the parabolic sar.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="start">The start.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="maximum">The maximum.</param>
    /// <returns></returns>
    public static StockData CalculateParabolicSAR(this StockData stockData, double start = 0.02, double increment = 0.02, double maximum = 0.2)
    {
        List<double> sarList = new();
        List<double> nextSarList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh1 = i >= 1 ? highList[i - 1] : 0;
            double prevLow1 = i >= 1 ? lowList[i - 1] : 0;
            double prevHigh2 = i >= 2 ? highList[i - 2] : 0;
            double prevLow2 = i >= 2 ? lowList[i - 2] : 0;

            bool uptrend;
            double ep, prevSAR, prevEP, SAR, af = start;
            if (currentValue > prevValue)
            {
                uptrend = true;
                ep = currentHigh;
                prevSAR = prevLow1;
                prevEP = currentHigh;
            }
            else
            {
                uptrend = false;
                ep = currentLow;
                prevSAR = prevHigh1;
                prevEP = currentLow;
            }
            SAR = prevSAR + (start * (prevEP - prevSAR));

            if (uptrend)
            {
                if (SAR > currentLow)
                {
                    uptrend = false;
                    SAR = Math.Max(ep, currentHigh);
                    ep = currentLow;
                    af = start;
                }
            }
            else
            {
                if (SAR < currentHigh)
                {
                    uptrend = true;
                    SAR = Math.Min(ep, currentLow);
                    ep = currentHigh;
                    af = start;
                }
            }

            if (uptrend)
            {
                if (currentHigh > ep)
                {
                    ep = currentHigh;
                    af = Math.Min(af + increment, maximum);
                }
            }
            else
            {
                if (currentLow < ep)
                {
                    ep = currentLow;
                    af = Math.Min(af + increment, maximum);
                }
            }

            if (uptrend)
            {
                SAR = i > 1 ? Math.Min(SAR, prevLow2) : Math.Min(SAR, prevLow1);
            }
            else
            {
                SAR = i > 1 ? Math.Max(SAR, prevHigh2) : Math.Max(SAR, prevHigh1);
            }
            sarList.AddRounded(SAR);

            double prevNextSar = nextSarList.LastOrDefault();
            double nextSar = SAR + (af * (ep - SAR));
            nextSarList.AddRounded(nextSar);

            var signal = GetCompareSignal(currentHigh - nextSar, prevHigh1 - prevNextSar);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sar", nextSarList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nextSarList;
        stockData.IndicatorName = IndicatorName.ParabolicSAR;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chandelier Exit
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="mult">The mult.</param>
    /// <returns></returns>
    public static StockData CalculateChandelierExit(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 22, 
        double mult = 3)
    {
        List<double> chandelierExitLongList = new();
        List<double> chandelierExitShortList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentAvgTrueRange = atrList[i];
            double highestHigh = highestList[i];
            double lowestLow = lowestList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevExitLong = chandelierExitLongList.LastOrDefault();
            double chandelierExitLong = highestHigh - (currentAvgTrueRange * mult);
            chandelierExitLongList.AddRounded(chandelierExitLong);

            double prevExitShort = chandelierExitShortList.LastOrDefault();
            double chandelierExitShort = lowestLow + (currentAvgTrueRange * mult);
            chandelierExitShortList.AddRounded(chandelierExitShort);

            var signal = GetBullishBearishSignal(currentValue - chandelierExitLong, prevValue - prevExitLong, currentValue - chandelierExitShort, prevValue - prevExitShort);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "ExitLong", chandelierExitLongList },
            { "ExitShort", chandelierExitShortList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ChandelierExit;

        return stockData;
    }

    /// <summary>
    /// Calculates the Average True Range Trailing Stops
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length1">The length1.</param>
    /// <param name="length2">The length2.</param>
    /// <param name="factor">The factor.</param>
    /// <returns></returns>
    public static StockData CalculateAverageTrueRangeTrailingStops(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 63, int length2 = 21, double factor = 3)
    {
        List<double> atrtsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;
        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentEma = emaList[i];
            double currentAtr = atrList[i];
            double prevAtrts = i >= 1 ? atrtsList.LastOrDefault() : currentValue;
            var upTrend = currentValue > currentEma;
            var dnTrend = currentValue <= currentEma;

            double atrts = upTrend ? Math.Max(currentValue - (factor * currentAtr), prevAtrts) : dnTrend ?
                Math.Min(currentValue + (factor * currentAtr), prevAtrts) : prevAtrts;
            atrtsList.AddRounded(atrts);

            var signal = GetCompareSignal(currentValue - atrts, prevValue - prevAtrts);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Atrts", atrtsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = atrtsList;
        stockData.IndicatorName = IndicatorName.AverageTrueRangeTrailingStops;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linear Trailing Stop
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateLinearTrailingStop(this StockData stockData, int length = 14, double mult = 28)
    {
        List<double> aList = new();
        List<double> osList = new();
        List<double> tsList = new();
        List<double> upperList = new();
        List<double> lowerList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double s = (double)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevA = i >= 1 ? aList[i - 1] : currentValue;
            double prevA2 = i >= 2 ? aList[i - 2] : currentValue;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double x = currentValue + ((prevA - prevA2) * mult);

            double a = x > prevA + s ? prevA + s : x < prevA - s ? prevA - s : prevA;
            aList.AddRounded(a);

            double up = a + (Math.Abs(a - prevA) * mult);
            double dn = a - (Math.Abs(a - prevA) * mult);

            double prevUpper = upperList.LastOrDefault();
            double upper = up == a ? prevUpper : up;
            upperList.AddRounded(upper);

            double prevLower = lowerList.LastOrDefault();
            double lower = dn == a ? prevLower : dn;
            lowerList.AddRounded(lower);

            double prevOs = osList.LastOrDefault();
            double os = currentValue > upper ? 1 : currentValue > lower ? 0 : prevOs;
            osList.AddRounded(os);

            double prevTs = tsList.LastOrDefault();
            double ts = (os * lower) + ((1 - os) * upper);
            tsList.AddRounded(ts);

            var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ts", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.LinearTrailingStop;

        return stockData;
    }

    /// <summary>
    /// Calculates the Nick Rypock Trailing Reverse
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNickRypockTrailingReverse(this StockData stockData, int length = 2)
    {
        List<double> nrtrList = new();
        List<double> hpList = new();
        List<double> lpList = new();
        List<double> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double pct = length * 0.01;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevTrend = trendList.LastOrDefault();
            double prevHp = hpList.LastOrDefault();
            double prevLp = lpList.LastOrDefault();
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevNrtr = nrtrList.LastOrDefault();
            double nrtr, hp = 0, lp = 0, trend = 0;
            if (prevTrend >= 0)
            {
                hp = currentValue > prevHp ? currentValue : prevHp;
                nrtr = hp * (1 - pct);

                if (currentValue <= nrtr)
                {
                    trend = -1;
                    lp = currentValue;
                    nrtr = lp * (1 + pct);
                }
            }
            else
            {
                lp = currentValue < prevLp ? currentValue : prevLp;
                nrtr = lp * (1 + pct);

                if (currentValue > nrtr)
                {
                    trend = 1;
                    hp = currentValue;
                    nrtr = hp * (1 - pct);
                }
            }
            trendList.AddRounded(trend);
            hpList.AddRounded(hp);
            lpList.AddRounded(lp);
            nrtrList.AddRounded(nrtr);

            var signal = GetCompareSignal(currentValue - nrtr, prevValue - prevNrtr);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nrtr", nrtrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nrtrList;
        stockData.IndicatorName = IndicatorName.NickRypockTrailingReverse;

        return stockData;
    }

    /// <summary>
    /// Calculates the Half Trend
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="atrLength"></param>
    /// <returns></returns>
    public static StockData CalculateHalfTrend(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 2,
        int atrLength = 100)
    {
        List<double> trendList = new();
        List<double> nextTrendList = new();
        List<double> upList = new();
        List<double> downList = new();
        List<double> htList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var atrList = CalculateAverageTrueRange(stockData, maType, atrLength).CustomValuesList;
        var highMaList = GetMovingAverageList(stockData, maType, length, highList);
        var lowMaList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentAvgTrueRange = atrList[i];
            double high = highestList[i];
            double low = lowestList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double highMa = highMaList[i];
            double lowMa = lowMaList[i];
            double maxLow = i >= 1 ? prevLow : low;
            double minHigh = i >= 1 ? prevHigh : high;
            double prevNextTrend = nextTrendList.LastOrDefault();
            double prevTrend = trendList.LastOrDefault();
            double prevUp = upList.LastOrDefault();
            double prevDown = downList.LastOrDefault();
            double atr = currentAvgTrueRange / 2;
            double dev = length * atr;

            double trend = 0, nextTrend = 0;
            if (prevNextTrend == 1)
            {
                maxLow = Math.Max(low, maxLow);

                if (highMa < maxLow && currentValue < (prevLow != 0 ? prevLow : low))
                {
                    trend = 1;
                    nextTrend = 0;
                    minHigh = high;
                }
                else
                {
                    minHigh = Math.Min(high, minHigh);

                    if (lowMa > minHigh && currentValue > (prevHigh != 0 ? prevHigh : high))
                    {
                        trend = 0;
                        nextTrend = 1;
                        maxLow = low;
                    }
                }
            }
            trendList.AddRounded(trend);
            nextTrendList.AddRounded(nextTrend);

            double up = 0, down = 0, arrowUp = 0, arrowDown = 0;
            if (trend == 0)
            {
                if (prevTrend != 0)
                {
                    up = prevDown;
                    arrowUp = up - atr;
                }
                else
                {
                    up = Math.Max(maxLow, prevUp);
                }
            }
            else
            {
                if (prevTrend != 1)
                {
                    down = prevUp;
                    arrowDown = down + atr;
                }
                else
                {
                    down = Math.Min(minHigh, prevDown);
                }
            }
            upList.AddRounded(up);
            downList.AddRounded(down);

            double ht = trend == 0 ? up : down;
            htList.AddRounded(ht);

            var signal = GetConditionSignal(arrowUp != 0 && trend == 0 && prevTrend == 1, arrowDown != 0 && trend == 1 && prevTrend == 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ht", htList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = htList;
        stockData.IndicatorName = IndicatorName.HalfTrend;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Dev Stop V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="length"></param>
    /// <param name="stdDev1"></param>
    /// <param name="stdDev2"></param>
    /// <param name="stdDev3"></param>
    /// <param name="stdDev4"></param>
    /// <returns></returns>
    public static StockData CalculateKaseDevStopV1(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int fastLength = 5, int slowLength = 21, int length = 20, double stdDev1 = 0,
        double stdDev2 = 1, double stdDev3 = 2.2, double stdDev4 = 3.6)
    {
        List<double> warningLineList = new();
        List<double> dev1List = new();
        List<double> dev2List = new();
        List<double> dev3List = new();
        List<double> dtrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevClose = i >= 2 ? closeList[i - 2] : 0;
            double prevLow = i >= 2 ? lowList[i - 2] : 0;

            double dtr = Math.Max(Math.Max(currentHigh - prevLow, Math.Abs(currentHigh - prevClose)), Math.Abs(currentLow - prevClose));
            dtrList.AddRounded(dtr);
        }

        var dtrAvgList = GetMovingAverageList(stockData, maType, length, dtrList);
        var smaSlowList = GetMovingAverageList(stockData, maType, slowLength, inputList);
        var smaFastList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        stockData.CustomValuesList = dtrList;
        var dtrStdList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double maFast = smaFastList[i];
            double maSlow = smaSlowList[i];
            double dtrAvg = dtrAvgList[i];
            double dtrStd = dtrStdList[i];
            double currentTypicalPrice = inputList[i];
            double prevMaFast = i >= 1 ? smaFastList[i - 1] : 0;
            double prevMaSlow = i >= 1 ? smaSlowList[i - 1] : 0;

            double warningLine = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev1 * dtrStd) :
                currentTypicalPrice - dtrAvg - (stdDev1 * dtrStd);
            warningLineList.AddRounded(warningLine);

            double dev1 = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev2 * dtrStd) : currentTypicalPrice - dtrAvg - (stdDev2 * dtrStd);
            dev1List.AddRounded(dev1);

            double dev2 = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev3 * dtrStd) : currentTypicalPrice - dtrAvg - (stdDev3 * dtrStd);
            dev2List.AddRounded(dev2);

            double dev3 = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev4 * dtrStd) : currentTypicalPrice - dtrAvg - (stdDev4 * dtrStd);
            dev3List.AddRounded(dev3);

            var signal = GetCompareSignal(maFast - maSlow, prevMaFast - prevMaSlow);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dev1", dev1List },
            { "Dev2", dev2List },
            { "Dev3", dev3List },
            { "WarningLine", warningLineList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.KaseDevStopV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Dev Stop V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="length"></param>
    /// <param name="stdDev1"></param>
    /// <param name="stdDev2"></param>
    /// <param name="stdDev3"></param>
    /// <param name="stdDev4"></param>
    /// <returns></returns>
    public static StockData CalculateKaseDevStopV2(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 10, int slowLength = 21, int length = 20, double stdDev1 = 0, double stdDev2 = 1, double stdDev3 = 2.2,
        double stdDev4 = 3.6)
    {
        List<double> valList = new();
        List<double> val1List = new();
        List<double> val2List = new();
        List<double> val3List = new();
        List<double> rrangeList = new();
        List<double> priceList = new();
        List<double> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var smaFastList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var smaSlowList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double maFast = smaFastList[i];
            double maSlow = smaSlowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevClose = i >= 2 ? inputList[i - 2] : 0;

            double trend = maFast > maSlow ? 1 : -1;
            trendList.AddRounded(trend);

            double price = trend == 1 ? currentHigh : currentLow;
            price = trend > 0 ? Math.Max(price, currentHigh) : Math.Min(price, currentLow);
            priceList.AddRounded(price);

            double mmax = Math.Max(Math.Max(currentHigh, prevHigh), prevClose);
            double mmin = Math.Min(Math.Min(currentLow, prevLow), prevClose);
            double rrange = mmax - mmin;
            rrangeList.AddRounded(rrange);
        }

        var rangeAvgList = GetMovingAverageList(stockData, maType, length, rrangeList);
        stockData.CustomValuesList = rrangeList;
        var rangeStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double price = priceList[i];
            double trend = trendList[i];
            double avg = rangeAvgList[i];
            double dev = rangeStdDevList[i];
            double prevPrice = i >= 1 ? priceList[i - 1] : 0;

            double val = (price + ((-1) * trend)) * (avg + (stdDev1 * dev));
            valList.AddRounded(val);

            double val1 = (price + ((-1) * trend)) * (avg + (stdDev2 * dev));
            val1List.AddRounded(val1);

            double val2 = (price + ((-1) * trend)) * (avg + (stdDev3 * dev));
            val2List.AddRounded(val2);

            double prevVal3 = val3List.LastOrDefault();
            double val3 = (price + ((-1) * trend)) * (avg + (stdDev4 * dev));
            val3List.AddRounded(val3);

            var signal = GetCompareSignal(price - val3, prevPrice - prevVal3);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dev1", valList },
            { "Dev2", val1List },
            { "Dev3", val2List },
            { "Dev4", val3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.KaseDevStopV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Percentage Trailing Stops
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="pct"></param>
    /// <returns></returns>
    public static StockData CalculatePercentageTrailingStops(this StockData stockData, int length = 100, double pct = 10)
    {
        List<double> stopSList = new();
        List<double> stopLList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = inputList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevHH = i >= 1 ? highestList[i - 1] : currentClose;
            double prevLL = i >= 1 ? lowestList[i - 1] : currentClose;
            double pSS = i >= 1 ? stopSList.LastOrDefault() : currentClose;
            double pSL = i >= 1 ? stopLList.LastOrDefault() : currentClose;

            double stopL = currentHigh > prevHH ? currentHigh - (pct * currentHigh) : pSL;
            stopLList.AddRounded(stopL);

            double stopS = currentLow < prevLL ? currentLow + (pct * currentLow) : pSS;
            stopSList.AddRounded(stopS);

            var signal = GetConditionSignal(prevHigh < stopS && currentHigh > stopS, prevLow > stopL && currentLow < stopL);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LongStop", stopLList },
            { "ShortStop", stopSList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.PercentageTrailingStops;

        return stockData;
    }

    /// <summary>
    /// Calculates the Motion To Attraction Trailing Stop
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMotionToAttractionTrailingStop(this StockData stockData, int length = 14)
    {
        List<double> osList = new();
        List<double> tsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mtaList = CalculateMotionToAttractionChannels(stockData, length);
        var aList = mtaList.OutputValues["UpperBand"];
        var bList = mtaList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevA = i >= 1 ? aList[i - 1] : currentValue;
            double prevB = i >= 1 ? bList[i - 1] : currentValue;
            double a = aList[i];
            double b = bList[i];

            double prevOs = osList.LastOrDefault();
            double os = currentValue > prevA ? 1 : currentValue < prevB ? 0 : prevOs;
            osList.AddRounded(os);

            double prevTs = tsList.LastOrDefault();
            double ts = (os * b) + ((1 - os) * a);
            tsList.AddRounded(ts);

            var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ts", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.MotionToAttractionTrailingStop;

        return stockData;
    }

    /// <summary>
    /// Calculates the Elder Safe Zone Stops
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateElderSafeZoneStops(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 63, int length2 = 22, int length3 = 3, double factor = 2.5)
    {
        List<double> safeZPlusList = new();
        List<double> safeZMinusList = new();
        List<double> dmPlusCountList = new();
        List<double> dmMinusCountList = new();
        List<double> dmMinusList = new();
        List<double> dmPlusList = new();
        List<double> stopList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentLow = lowList[i];
            double currentHigh = highList[i];
            double currentEma = emaList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevEma = i >= 1 ? emaList[i - 1] : 0;

            double dmMinus = prevLow > currentLow ? prevLow - currentLow : 0;
            dmMinusList.AddRounded(dmMinus);

            double dmMinusCount = prevLow > currentLow ? 1 : 0;
            dmMinusCountList.AddRounded(dmMinusCount);

            double dmPlus = currentHigh > prevHigh ? currentHigh - prevHigh : 0;
            dmPlusList.AddRounded(dmPlus);

            double dmPlusCount = currentHigh > prevHigh ? 1 : 0;
            dmPlusCountList.AddRounded(dmPlusCount);

            double countM = dmMinusCountList.TakeLastExt(length2).Sum();
            double dmMinusSum = dmMinusList.TakeLastExt(length2).Sum();
            double dmAvgMinus = countM != 0 ? dmMinusSum / countM : 0;
            double countP = dmPlusCountList.TakeLastExt(length2).Sum();
            double dmPlusSum = dmPlusList.TakeLastExt(length2).Sum();
            double dmAvgPlus = countP != 0 ? dmPlusSum / countP : 0;

            double safeZMinus = prevLow - (factor * dmAvgMinus);
            safeZMinusList.AddRounded(safeZMinus);

            double safeZPlus = prevHigh + (factor * dmAvgPlus);
            safeZPlusList.AddRounded(safeZPlus);

            double highest = safeZMinusList.TakeLastExt(length3).Max();
            double lowest = safeZPlusList.TakeLastExt(length3).Min();

            double prevStop = stopList.LastOrDefault();
            double stop = currentValue >= currentEma ? highest : lowest;
            stopList.AddRounded(stop);

            var signal = GetBullishBearishSignal(currentValue - Math.Max(currentEma, stop), prevValue - Math.Max(prevEma, prevStop),
                currentValue - Math.Min(currentEma, stop), prevValue - Math.Min(prevEma, prevStop));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eszs", stopList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stopList;
        stockData.IndicatorName = IndicatorName.ElderSafeZoneStops;

        return stockData;
    }
}