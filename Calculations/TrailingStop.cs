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
    public static StockData CalculateAdaptiveTrailingStop(this StockData stockData, int length = 100, decimal factor = 3)
    {
        List<decimal> upList = new();
        List<decimal> dnList = new();
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> osList = new();
        List<decimal> tsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var perList = CalculatePoweredKaufmanAdaptiveMovingAverage(stockData, length, factor).OutputValues["Per"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal per = perList[i];

            decimal prevA = i >= 1 ? aList.LastOrDefault() : currentValue;
            decimal a = Math.Max(currentValue, prevA) - (Math.Abs(currentValue - prevA) * per);
            aList.AddRounded(a);

            decimal prevB = i >= 1 ? bList.LastOrDefault() : currentValue;
            decimal b = Math.Min(currentValue, prevB) + (Math.Abs(currentValue - prevB) * per);
            bList.AddRounded(b);

            decimal prevUp = upList.LastOrDefault();
            decimal up = a > prevA ? a : a < prevA && b < prevB ? a : prevUp;
            upList.AddRounded(up);

            decimal prevDn = dnList.LastOrDefault();
            decimal dn = b < prevB ? b : b > prevB && a > prevA ? b : prevDn;
            dnList.AddRounded(dn);

            decimal prevOs = osList.LastOrDefault();
            decimal os = up > currentValue ? 1 : dn > currentValue ? 0 : prevOs;
            osList.AddRounded(os);

            decimal prevTs = tsList.LastOrDefault();
            decimal ts = (os * dn) + ((1 - os) * up);
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
    public static StockData CalculateAdaptiveAutonomousRecursiveTrailingStop(this StockData stockData, int length = 14, decimal gamma = 3)
    {
        List<decimal> tsList = new();
        List<decimal> osList = new();
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var aamaList = CalculateAdaptiveAutonomousRecursiveMovingAverage(stockData, length, gamma);
        var ma2List = aamaList.CustomValuesList;
        var dList = aamaList.OutputValues["D"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ma2 = ma2List[i];
            decimal d = dList[i];

            decimal prevUpper = upperList.LastOrDefault();
            decimal upper = ma2 + d;
            upperList.AddRounded(upper);

            decimal prevLower = lowerList.LastOrDefault();
            decimal lower = ma2 - d;
            lowerList.AddRounded(lower);

            decimal prevOs = osList.LastOrDefault();
            decimal os = currentValue > prevUpper ? 1 : currentValue < prevLower ? 0 : prevOs;
            osList.AddRounded(os);

            decimal prevTs = tsList.LastOrDefault();
            decimal ts = (os * lower) + ((1 - os) * upper);
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
    public static StockData CalculateParabolicSAR(this StockData stockData, decimal start = 0.02m, decimal increment = 0.02m, decimal maximum = 0.2m)
    {
        List<decimal> sarList = new();
        List<decimal> nextSarList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevHigh1 = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow1 = i >= 1 ? lowList[i - 1] : 0;
            decimal prevHigh2 = i >= 2 ? highList[i - 2] : 0;
            decimal prevLow2 = i >= 2 ? lowList[i - 2] : 0;

            bool uptrend;
            decimal ep, prevSAR, prevEP, SAR, af = start;
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

            decimal prevNextSar = nextSarList.LastOrDefault();
            decimal nextSar = SAR + (af * (ep - SAR));
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
        decimal mult = 3)
    {
        List<decimal> chandelierExitLongList = new();
        List<decimal> chandelierExitShortList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentAvgTrueRange = atrList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevExitLong = chandelierExitLongList.LastOrDefault();
            decimal chandelierExitLong = highestHigh - (currentAvgTrueRange * mult);
            chandelierExitLongList.AddRounded(chandelierExitLong);

            decimal prevExitShort = chandelierExitShortList.LastOrDefault();
            decimal chandelierExitShort = lowestLow + (currentAvgTrueRange * mult);
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
        stockData.CustomValuesList = new List<decimal>();
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
        int length1 = 63, int length2 = 21, decimal factor = 3)
    {
        List<decimal> atrtsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;
        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentEma = emaList[i];
            decimal currentAtr = atrList[i];
            decimal prevAtrts = i >= 1 ? atrtsList.LastOrDefault() : currentValue;
            var upTrend = currentValue > currentEma;
            var dnTrend = currentValue <= currentEma;

            decimal atrts = upTrend ? Math.Max(currentValue - (factor * currentAtr), prevAtrts) : dnTrend ?
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
    public static StockData CalculateLinearTrailingStop(this StockData stockData, int length = 14, decimal mult = 28)
    {
        List<decimal> aList = new();
        List<decimal> osList = new();
        List<decimal> tsList = new();
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal s = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevA2 = i >= 2 ? aList[i - 2] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal x = currentValue + ((prevA - prevA2) * mult);

            decimal a = x > prevA + s ? prevA + s : x < prevA - s ? prevA - s : prevA;
            aList.AddRounded(a);

            decimal up = a + (Math.Abs(a - prevA) * mult);
            decimal dn = a - (Math.Abs(a - prevA) * mult);

            decimal prevUpper = upperList.LastOrDefault();
            decimal upper = up == a ? prevUpper : up;
            upperList.AddRounded(upper);

            decimal prevLower = lowerList.LastOrDefault();
            decimal lower = dn == a ? prevLower : dn;
            lowerList.AddRounded(lower);

            decimal prevOs = osList.LastOrDefault();
            decimal os = currentValue > upper ? 1 : currentValue > lower ? 0 : prevOs;
            osList.AddRounded(os);

            decimal prevTs = tsList.LastOrDefault();
            decimal ts = (os * lower) + ((1 - os) * upper);
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
        List<decimal> nrtrList = new();
        List<decimal> hpList = new();
        List<decimal> lpList = new();
        List<decimal> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal pct = length * 0.01m;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevTrend = trendList.LastOrDefault();
            decimal prevHp = hpList.LastOrDefault();
            decimal prevLp = lpList.LastOrDefault();
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevNrtr = nrtrList.LastOrDefault();
            decimal nrtr, hp = 0, lp = 0, trend = 0;
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
        List<decimal> trendList = new();
        List<decimal> nextTrendList = new();
        List<decimal> upList = new();
        List<decimal> downList = new();
        List<decimal> htList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var atrList = CalculateAverageTrueRange(stockData, maType, atrLength).CustomValuesList;
        var highMaList = GetMovingAverageList(stockData, maType, length, highList);
        var lowMaList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentAvgTrueRange = atrList[i];
            decimal high = highestList[i];
            decimal low = lowestList[i];
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal highMa = highMaList[i];
            decimal lowMa = lowMaList[i];
            decimal maxLow = i >= 1 ? prevLow : low;
            decimal minHigh = i >= 1 ? prevHigh : high;
            decimal prevNextTrend = nextTrendList.LastOrDefault();
            decimal prevTrend = trendList.LastOrDefault();
            decimal prevUp = upList.LastOrDefault();
            decimal prevDown = downList.LastOrDefault();
            decimal atr = currentAvgTrueRange / 2;
            decimal dev = length * atr;

            decimal trend = 0, nextTrend = 0;
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

            decimal up = 0, down = 0, arrowUp = 0, arrowDown = 0;
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

            decimal ht = trend == 0 ? up : down;
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
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int fastLength = 5, int slowLength = 21, int length = 20, decimal stdDev1 = 0,
        decimal stdDev2 = 1, decimal stdDev3 = 2.2m, decimal stdDev4 = 3.6m)
    {
        List<decimal> warningLineList = new();
        List<decimal> dev1List = new();
        List<decimal> dev2List = new();
        List<decimal> dev3List = new();
        List<decimal> dtrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevClose = i >= 2 ? closeList[i - 2] : 0;
            decimal prevLow = i >= 2 ? lowList[i - 2] : 0;

            decimal dtr = Math.Max(Math.Max(currentHigh - prevLow, Math.Abs(currentHigh - prevClose)), Math.Abs(currentLow - prevClose));
            dtrList.AddRounded(dtr);
        }

        var dtrAvgList = GetMovingAverageList(stockData, maType, length, dtrList);
        var smaSlowList = GetMovingAverageList(stockData, maType, slowLength, inputList);
        var smaFastList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        stockData.CustomValuesList = dtrList;
        var dtrStdList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal maFast = smaFastList[i];
            decimal maSlow = smaSlowList[i];
            decimal dtrAvg = dtrAvgList[i];
            decimal dtrStd = dtrStdList[i];
            decimal currentTypicalPrice = inputList[i];
            decimal prevMaFast = i >= 1 ? smaFastList[i - 1] : 0;
            decimal prevMaSlow = i >= 1 ? smaSlowList[i - 1] : 0;

            decimal warningLine = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev1 * dtrStd) :
                currentTypicalPrice - dtrAvg - (stdDev1 * dtrStd);
            warningLineList.AddRounded(warningLine);

            decimal dev1 = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev2 * dtrStd) : currentTypicalPrice - dtrAvg - (stdDev2 * dtrStd);
            dev1List.AddRounded(dev1);

            decimal dev2 = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev3 * dtrStd) : currentTypicalPrice - dtrAvg - (stdDev3 * dtrStd);
            dev2List.AddRounded(dev2);

            decimal dev3 = maFast < maSlow ? currentTypicalPrice + dtrAvg + (stdDev4 * dtrStd) : currentTypicalPrice - dtrAvg - (stdDev4 * dtrStd);
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
        stockData.CustomValuesList = new List<decimal>();
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
        int fastLength = 10, int slowLength = 21, int length = 20, decimal stdDev1 = 0, decimal stdDev2 = 1, decimal stdDev3 = 2.2m,
        decimal stdDev4 = 3.6m)
    {
        List<decimal> valList = new();
        List<decimal> val1List = new();
        List<decimal> val2List = new();
        List<decimal> val3List = new();
        List<decimal> rrangeList = new();
        List<decimal> priceList = new();
        List<decimal> trendList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var smaFastList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var smaSlowList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal maFast = smaFastList[i];
            decimal maSlow = smaSlowList[i];
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal prevClose = i >= 2 ? inputList[i - 2] : 0;

            decimal trend = maFast > maSlow ? 1 : -1;
            trendList.AddRounded(trend);

            decimal price = trend == 1 ? currentHigh : currentLow;
            price = trend > 0 ? Math.Max(price, currentHigh) : Math.Min(price, currentLow);
            priceList.AddRounded(price);

            decimal mmax = Math.Max(Math.Max(currentHigh, prevHigh), prevClose);
            decimal mmin = Math.Min(Math.Min(currentLow, prevLow), prevClose);
            decimal rrange = mmax - mmin;
            rrangeList.AddRounded(rrange);
        }

        var rangeAvgList = GetMovingAverageList(stockData, maType, length, rrangeList);
        stockData.CustomValuesList = rrangeList;
        var rangeStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal price = priceList[i];
            decimal trend = trendList[i];
            decimal avg = rangeAvgList[i];
            decimal dev = rangeStdDevList[i];
            decimal prevPrice = i >= 1 ? priceList[i - 1] : 0;

            decimal val = (price + ((-1) * trend)) * (avg + (stdDev1 * dev));
            valList.AddRounded(val);

            decimal val1 = (price + ((-1) * trend)) * (avg + (stdDev2 * dev));
            val1List.AddRounded(val1);

            decimal val2 = (price + ((-1) * trend)) * (avg + (stdDev3 * dev));
            val2List.AddRounded(val2);

            decimal prevVal3 = val3List.LastOrDefault();
            decimal val3 = (price + ((-1) * trend)) * (avg + (stdDev4 * dev));
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
        stockData.CustomValuesList = new List<decimal>();
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
    public static StockData CalculatePercentageTrailingStops(this StockData stockData, int length = 100, decimal pct = 10)
    {
        List<decimal> stopSList = new();
        List<decimal> stopLList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentClose = inputList[i];
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal prevHH = i >= 1 ? highestList[i - 1] : currentClose;
            decimal prevLL = i >= 1 ? lowestList[i - 1] : currentClose;
            decimal pSS = i >= 1 ? stopSList.LastOrDefault() : currentClose;
            decimal pSL = i >= 1 ? stopLList.LastOrDefault() : currentClose;

            decimal stopL = currentHigh > prevHH ? currentHigh - (pct * currentHigh) : pSL;
            stopLList.AddRounded(stopL);

            decimal stopS = currentLow < prevLL ? currentLow + (pct * currentLow) : pSS;
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
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> osList = new();
        List<decimal> tsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mtaList = CalculateMotionToAttractionChannels(stockData, length);
        var aList = mtaList.OutputValues["UpperBand"];
        var bList = mtaList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal a = aList[i];
            decimal b = bList[i];

            decimal prevOs = osList.LastOrDefault();
            decimal os = currentValue > prevA ? 1 : currentValue < prevB ? 0 : prevOs;
            osList.AddRounded(os);

            decimal prevTs = tsList.LastOrDefault();
            decimal ts = (os * b) + ((1 - os) * a);
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
        int length1 = 63, int length2 = 22, int length3 = 3, decimal factor = 2.5m)
    {
        List<decimal> safeZPlusList = new();
        List<decimal> safeZMinusList = new();
        List<decimal> dmPlusCountList = new();
        List<decimal> dmMinusCountList = new();
        List<decimal> dmMinusList = new();
        List<decimal> dmPlusList = new();
        List<decimal> stopList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentLow = lowList[i];
            decimal currentHigh = highList[i];
            decimal currentEma = emaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal prevEma = i >= 1 ? emaList[i - 1] : 0;

            decimal dmMinus = prevLow > currentLow ? prevLow - currentLow : 0;
            dmMinusList.AddRounded(dmMinus);

            decimal dmMinusCount = prevLow > currentLow ? 1 : 0;
            dmMinusCountList.AddRounded(dmMinusCount);

            decimal dmPlus = currentHigh > prevHigh ? currentHigh - prevHigh : 0;
            dmPlusList.AddRounded(dmPlus);

            decimal dmPlusCount = currentHigh > prevHigh ? 1 : 0;
            dmPlusCountList.AddRounded(dmPlusCount);

            decimal countM = dmMinusCountList.TakeLastExt(length2).Sum();
            decimal dmMinusSum = dmMinusList.TakeLastExt(length2).Sum();
            decimal dmAvgMinus = countM != 0 ? dmMinusSum / countM : 0;
            decimal countP = dmPlusCountList.TakeLastExt(length2).Sum();
            decimal dmPlusSum = dmPlusList.TakeLastExt(length2).Sum();
            decimal dmAvgPlus = countP != 0 ? dmPlusSum / countP : 0;

            decimal safeZMinus = prevLow - (factor * dmAvgMinus);
            safeZMinusList.AddRounded(safeZMinus);

            decimal safeZPlus = prevHigh + (factor * dmAvgPlus);
            safeZPlusList.AddRounded(safeZPlus);

            decimal highest = safeZMinusList.TakeLastExt(length3).Max();
            decimal lowest = safeZPlusList.TakeLastExt(length3).Min();

            decimal prevStop = stopList.LastOrDefault();
            decimal stop = currentValue >= currentEma ? highest : lowest;
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