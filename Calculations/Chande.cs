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
    /// Calculates the Chande Quick Stick
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateChandeQuickStick(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<double> openCloseList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentOpen = openList[i];
            double currentClose = inputList[i];

            double openClose = currentClose - currentOpen;
            openCloseList.AddRounded(openClose);
        }

        var smaList = GetMovingAverageList(stockData, maType, length, openCloseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double sma = smaList[i];
            double prevSma = i >= 1 ? smaList[i - 1] : 0;

            var signal = GetCompareSignal(sma, prevSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cqs", smaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = smaList;
        stockData.IndicatorName = IndicatorName.ChandeQuickStick;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Momentum Oscillator Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static StockData CalculateChandeMomentumOscillatorFilter(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 9, double filter = 3)
    {
        List<double> cmoList = new();
        List<double> diffList = new();
        List<double> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double diff = MinPastValues(i, 1, currentValue - prevValue);
            double absDiff = Math.Abs(diff);
            if (absDiff > filter)
            {
                diff = 0; absDiff = 0;
            }
            diffList.AddRounded(diff);
            absDiffList.AddRounded(absDiff);

            double diffSum = diffList.TakeLastExt(length).Sum();
            double absDiffSum = absDiffList.TakeLastExt(length).Sum();

            double cmo = absDiffSum != 0 ? MinOrMax(100 * diffSum / absDiffSum, 100, -100) : 0;
            cmoList.AddRounded(cmo);
        }

        var cmoSignalList = GetMovingAverageList(stockData, maType, length, cmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double cmo = cmoList[i];
            double cmoSignal = cmoSignalList[i];
            double prevCmo = i >= 1 ? cmoList[i - 1] : 0;
            double prevCmoSignal = i >= 1 ? cmoSignalList[i - 1] : 0;

            var signal = GetRsiSignal(cmo - cmoSignal, prevCmo - prevCmoSignal, cmo, prevCmo, 70, -70);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmof", cmoList },
            { "Signal", cmoSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cmoList;
        stockData.IndicatorName = IndicatorName.ChandeMomentumOscillatorFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Momentum Oscillator Absolute
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateChandeMomentumOscillatorAbsolute(this StockData stockData, int length = 9)
    {
        List<double> cmoAbsList = new();
        List<double> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double priorValue = i >= length ? inputList[i - length] : 0;
            double prevCmoAbs1 = i >= 1 ? cmoAbsList[i - 1] : 0;
            double prevCmoAbs2 = i >= 2 ? cmoAbsList[i - 2] : 0;

            double absDiff = Math.Abs(MinPastValues(i, 1, currentValue - prevValue));
            absDiffList.AddRounded(absDiff);

            double num = Math.Abs(100 * MinPastValues(i, length, currentValue - priorValue));
            double denom = absDiffList.TakeLastExt(length).Sum();

            double cmoAbs = denom != 0 ? MinOrMax(num / denom, 100, 0) : 0;
            cmoAbsList.AddRounded(cmoAbs);

            var signal = GetRsiSignal(cmoAbs - prevCmoAbs1, prevCmoAbs1 - prevCmoAbs2, cmoAbs, prevCmoAbs1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmoa", cmoAbsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cmoAbsList;
        stockData.IndicatorName = IndicatorName.ChandeMomentumOscillatorAbsolute;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Momentum Oscillator Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateChandeMomentumOscillatorAverage(this StockData stockData, int length1 = 5, int length2 = 10, int length3 = 20)
    {
        List<double> cmoAvgList = new();
        List<double> diffList = new();
        List<double> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentPrice = inputList[i];
            double prevPrice = i >= 1 ? inputList[i - 1] : 0;
            double prevCmoAvg1 = i >= 1 ? cmoAvgList[i - 1] : 0;
            double prevCmoAvg2 = i >= 2 ? cmoAvgList[i - 2] : 0;

            double diff = currentPrice - prevPrice;
            diffList.AddRounded(diff);

            double absDiff = Math.Abs(diff);
            absDiffList.AddRounded(absDiff);

            double diffSum1 = diffList.TakeLastExt(length1).Sum();
            double absSum1 = absDiffList.TakeLastExt(length1).Sum();
            double diffSum2 = diffList.TakeLastExt(length2).Sum();
            double absSum2 = absDiffList.TakeLastExt(length2).Sum();
            double diffSum3 = diffList.TakeLastExt(length3).Sum();
            double absSum3 = absDiffList.TakeLastExt(length3).Sum();
            double temp1 = absSum1 != 0 ? MinOrMax(diffSum1 / absSum1, 1, -1) : 0;
            double temp2 = absSum2 != 0 ? MinOrMax(diffSum2 / absSum2, 1, -1) : 0;
            double temp3 = absSum3 != 0 ? MinOrMax(diffSum3 / absSum3, 1, -1) : 0;

            double cmoAvg = 100 * ((temp1 + temp2 + temp3) / 3);
            cmoAvgList.AddRounded(cmoAvg);

            var signal = GetRsiSignal(cmoAvg - prevCmoAvg1, prevCmoAvg1 - prevCmoAvg2, cmoAvg, prevCmoAvg1, 50, -50);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmoa", cmoAvgList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cmoAvgList;
        stockData.IndicatorName = IndicatorName.ChandeMomentumOscillatorAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Momentum Oscillator Absolute Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateChandeMomentumOscillatorAbsoluteAverage(this StockData stockData, int length1 = 5, int length2 = 10, int length3 = 20)
    {
        List<double> cmoAbsAvgList = new();
        List<double> diffList = new();
        List<double> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentPrice = inputList[i];
            double prevPrice = i >= 1 ? inputList[i - 1] : 0;
            double prevCmoAbsAvg1 = i >= 1 ? cmoAbsAvgList[i - 1] : 0;
            double prevCmoAbsAvg2 = i >= 2 ? cmoAbsAvgList[i - 2] : 0;

            double diff = currentPrice - prevPrice;
            diffList.AddRounded(diff);

            double absDiff = Math.Abs(diff);
            absDiffList.AddRounded(absDiff);

            double diffSum1 = diffList.TakeLastExt(length1).Sum();
            double absSum1 = absDiffList.TakeLastExt(length1).Sum();
            double diffSum2 = diffList.TakeLastExt(length2).Sum();
            double absSum2 = absDiffList.TakeLastExt(length2).Sum();
            double diffSum3 = diffList.TakeLastExt(length3).Sum();
            double absSum3 = absDiffList.TakeLastExt(length3).Sum();
            double temp1 = absSum1 != 0 ? MinOrMax(diffSum1 / absSum1, 1, -1) : 0;
            double temp2 = absSum2 != 0 ? MinOrMax(diffSum2 / absSum2, 1, -1) : 0;
            double temp3 = absSum3 != 0 ? MinOrMax(diffSum3 / absSum3, 1, -1) : 0;

            double cmoAbsAvg = Math.Abs(100 * ((temp1 + temp2 + temp3) / 3));
            cmoAbsAvgList.AddRounded(cmoAbsAvg);

            var signal = GetRsiSignal(cmoAbsAvg - prevCmoAbsAvg1, prevCmoAbsAvg1 - prevCmoAbsAvg2, cmoAbsAvg, prevCmoAbsAvg1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmoaa", cmoAbsAvgList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cmoAbsAvgList;
        stockData.IndicatorName = IndicatorName.ChandeMomentumOscillatorAbsoluteAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Composite Momentum Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateChandeCompositeMomentumIndex(this StockData stockData,
        MovingAvgType maType = MovingAvgType.DoubleExponentialMovingAverage, int length1 = 5, int length2 = 10, int length3 = 20, int smoothLength = 3)
    {
        List<double> valueDiff1List = new();
        List<double> valueDiff2List = new();
        List<double> dmiList = new();
        List<double> eList = new();
        List<double> sList = new();
        List<double> cmo5RatioList = new();
        List<double> cmo10RatioList = new();
        List<double> cmo20RatioList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
        var stdDev10List = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        var stdDev20List = CalculateStandardDeviationVolatility(stockData, maType, length3).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double valueDiff1 = currentValue > prevValue ? MinPastValues(i, 1, currentValue - prevValue) : 0;
            valueDiff1List.AddRounded(valueDiff1);

            double valueDiff2 = currentValue < prevValue ? MinPastValues(i, 1, prevValue - currentValue) : 0;
            valueDiff2List.AddRounded(valueDiff2);

            double cmo51 = valueDiff1List.TakeLastExt(length1).Sum();
            double cmo52 = valueDiff2List.TakeLastExt(length1).Sum();
            double cmo101 = valueDiff1List.TakeLastExt(length2).Sum();
            double cmo102 = valueDiff2List.TakeLastExt(length2).Sum();
            double cmo201 = valueDiff1List.TakeLastExt(length3).Sum();
            double cmo202 = valueDiff2List.TakeLastExt(length3).Sum();

            double cmo5Ratio = cmo51 + cmo52 != 0 ? MinOrMax(100 * (cmo51 - cmo52) / (cmo51 + cmo52), 100, -100) : 0;
            cmo5RatioList.AddRounded(cmo5Ratio);

            double cmo10Ratio = cmo101 + cmo102 != 0 ? MinOrMax(100 * (cmo101 - cmo102) / (cmo101 + cmo102), 100, -100) : 0;
            cmo10RatioList.AddRounded(cmo10Ratio);

            double cmo20Ratio = cmo201 + cmo202 != 0 ? MinOrMax(100 * (cmo201 - cmo202) / (cmo201 + cmo202), 100, -100) : 0;
            cmo20RatioList.AddRounded(cmo20Ratio);
        }

        var cmo5List = GetMovingAverageList(stockData, maType, smoothLength, cmo5RatioList);
        var cmo10List = GetMovingAverageList(stockData, maType, smoothLength, cmo10RatioList);
        var cmo20List = GetMovingAverageList(stockData, maType, smoothLength, cmo20RatioList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double stdDev5 = stdDevList[i];
            double stdDev10 = stdDev10List[i];
            double stdDev20 = stdDev20List[i];
            double cmo5 = cmo5List[i];
            double cmo10 = cmo10List[i];
            double cmo20 = cmo20List[i];

            double dmi = stdDev5 + stdDev10 + stdDev20 != 0 ?
                MinOrMax(((stdDev5 * cmo5) + (stdDev10 * cmo10) + (stdDev20 * cmo20)) / (stdDev5 + stdDev10 + stdDev20), 100, -100) : 0;
            dmiList.AddRounded(dmi);

            double prevS = sList.LastOrDefault();
            double s = dmiList.TakeLastExt(length1).Average();
            sList.AddRounded(s);

            double prevE = eList.LastOrDefault();
            double e = CalculateEMA(dmi, prevE, smoothLength);
            eList.AddRounded(e);

            var signal = GetRsiSignal(e - s, prevE - prevS, e, prevE, 70, -70);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ccmi", eList },
            { "Signal", sList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = eList;
        stockData.IndicatorName = IndicatorName.ChandeCompositeMomentumIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Momentum Oscillator Average Disparity Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateChandeMomentumOscillatorAverageDisparityIndex(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 200, int length2 = 50, int length3 = 20)
    {
        List<double> avgDisparityIndexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var firstEmaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var secondEmaList = GetMovingAverageList(stockData, maType, length2, inputList);
        var thirdEmaList = GetMovingAverageList(stockData, maType, length3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double firstEma = firstEmaList[i];
            double secondEma = secondEmaList[i];
            double thirdEma = thirdEmaList[i];
            double firstDisparityIndex = currentValue != 0 ? (currentValue - firstEma) / currentValue * 100 : 0;
            double secondDisparityIndex = currentValue != 0 ? (currentValue - secondEma) / currentValue * 100 : 0;
            double thirdDisparityIndex = currentValue != 0 ? (currentValue - thirdEma) / currentValue * 100 : 0;

            double prevAvgDisparityIndex = avgDisparityIndexList.LastOrDefault();
            double avgDisparityIndex = (firstDisparityIndex + secondDisparityIndex + thirdDisparityIndex) / 3;
            avgDisparityIndexList.AddRounded(avgDisparityIndex);

            var signal = GetCompareSignal(avgDisparityIndex, prevAvgDisparityIndex);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmoadi", avgDisparityIndexList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = avgDisparityIndexList;
        stockData.IndicatorName = IndicatorName.ChandeMomentumOscillatorAverageDisparityIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Kroll Rsquared Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateChandeKrollRSquaredIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, int smoothLength = 3)
    {
        List<double> r2RawList = new();
        List<double> tempValueList = new();
        List<double> indexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double index = i;
            indexList.AddRounded(index);

            double currentValue = inputList[i];
            tempValueList.AddRounded(currentValue);

            var r2 = GoodnessOfFit.RSquared(indexList.TakeLastExt(length).Select(x => (double)x),
                tempValueList.TakeLastExt(length).Select(x => (double)x));
            r2 = IsValueNullOrInfinity(r2) ? 0 : r2;
            r2RawList.AddRounded((double)r2);
        }

        var r2SmoothedList = GetMovingAverageList(stockData, maType, smoothLength, r2RawList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double r2Sma = r2SmoothedList[i];
            double prevR2Sma1 = i >= 1 ? r2SmoothedList[i - 1] : 0;
            double prevR2Sma2 = i >= 2 ? r2SmoothedList[i - 2] : 0;

            var signal = GetCompareSignal(r2Sma - prevR2Sma1, prevR2Sma1 - prevR2Sma2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ckrsi", r2SmoothedList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = r2SmoothedList;
        stockData.IndicatorName = IndicatorName.ChandeKrollRSquaredIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Volatility Index Dynamic Average Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="alpha1"></param>
    /// <param name="alpha2"></param>
    /// <returns></returns>
    public static StockData CalculateChandeVolatilityIndexDynamicAverageIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20, double alpha1 = 0.2, double alpha2 = 0.04)
    {
        List<double> vidya1List = new();
        List<double> vidya2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var stdDevEmaList = GetMovingAverageList(stockData, maType, length, stdDevList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentStdDev = stdDevList[i];
            double currentStdDevEma = stdDevEmaList[i];
            double prevVidya1 = i >= 1 ? vidya1List.LastOrDefault() : currentValue;
            double prevVidya2 = i >= 1 ? vidya2List.LastOrDefault() : currentValue;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double ratio = currentStdDevEma != 0 ? currentStdDev / currentStdDevEma : 0;

            double vidya1 = (alpha1 * ratio * currentValue) + ((1 - (alpha1 * ratio)) * prevVidya1);
            vidya1List.AddRounded(vidya1);

            double vidya2 = (alpha2 * ratio * currentValue) + ((1 - (alpha2 * ratio)) * prevVidya2);
            vidya2List.AddRounded(vidya2);

            var signal = GetBullishBearishSignal(currentValue - Math.Max(vidya1, vidya2), prevValue - Math.Max(prevVidya1, prevVidya2),
                currentValue - Math.Min(vidya1, vidya2), prevValue - Math.Min(prevVidya1, prevVidya2));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cvida1", vidya1List },
            { "Cvida2", vidya2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ChandeVolatilityIndexDynamicAverageIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Trend Score
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="startLength"></param>
    /// <param name="endLength"></param>
    /// <returns></returns>
    public static StockData CalculateChandeTrendScore(this StockData stockData, int startLength = 11, int endLength = 20)
    {
        List<double> tsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double prevTs = tsList.LastOrDefault();
            double ts = 0;
            for (int j = startLength; j <= endLength; j++)
            {
                double prevValue = i >= j ? inputList[i - j] : 0;
                ts += currentValue >= prevValue ? 1 : -1;
            }
            tsList.AddRounded(ts);

            var signal = GetCompareSignal(ts, prevTs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cts", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.ChandeTrendScore;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Forecast Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateChandeForecastOscillator(this StockData stockData, int length = 14)
    {
        List<double> pfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentLinReg = linRegList[i];

            double prevPf = pfList.LastOrDefault();
            double pf = currentValue != 0 ? (currentValue - currentLinReg) * 100 / currentValue : 0;
            pfList.AddRounded(pf);

            var signal = GetCompareSignal(pf, prevPf);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cfo", pfList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pfList;
        stockData.IndicatorName = IndicatorName.ChandeForecastOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Intraday Momentum Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateChandeIntradayMomentumIndex(this StockData stockData, int length = 14)
    {
        List<double> imiUnfilteredList = new();
        List<double> gainsList = new();
        List<double> lossesList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentOpen = openList[i];
            double prevImi1 = i >= 1 ? imiUnfilteredList[i - 1] : 0;
            double prevImi2 = i >= 2 ? imiUnfilteredList[i - 2] : 0;

            double prevGains = gainsList.LastOrDefault();
            double gains = currentClose > currentOpen ? prevGains + (currentClose - currentOpen) : 0;
            gainsList.AddRounded(gains);

            double prevLosses = lossesList.LastOrDefault();
            double losses = currentClose < currentOpen ? prevLosses + (currentOpen - currentClose) : 0;
            lossesList.AddRounded(losses);

            double upt = gainsList.TakeLastExt(length).Sum();
            double dnt = lossesList.TakeLastExt(length).Sum();

            double imiUnfiltered = upt + dnt != 0 ? MinOrMax(100 * upt / (upt + dnt), 100, 0) : 0;
            imiUnfilteredList.AddRounded(imiUnfiltered);

            var signal = GetRsiSignal(imiUnfiltered - prevImi1, prevImi1 - prevImi2, imiUnfiltered, prevImi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cimi", imiUnfilteredList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = imiUnfilteredList;
        stockData.IndicatorName = IndicatorName.ChandeIntradayMomentumIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chande Momentum Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateChandeMomentumOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14, int signalLength = 3)
    {
        List<double> cmoList = new();
        List<double> cmoPosChgList = new();
        List<double> cmoNegChgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double diff = MinPastValues(i, 1, currentValue - prevValue);

            double negChg = i >= 1 && diff < 0 ? Math.Abs(diff) : 0;
            cmoNegChgList.AddRounded(negChg);

            double posChg = i >= 1 && diff > 0 ? diff : 0;
            cmoPosChgList.AddRounded(posChg);

            double negSum = cmoNegChgList.TakeLastExt(length).Sum();
            double posSum = cmoPosChgList.TakeLastExt(length).Sum();

            double cmo = posSum + negSum != 0 ? MinOrMax((posSum - negSum) / (posSum + negSum) * 100, 100, -100) : 0;
            cmoList.AddRounded(cmo);
        }

        var cmoSignalList = GetMovingAverageList(stockData, maType, signalLength, cmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double cmo = cmoList[i];
            double cmoSignal = cmoSignalList[i];
            double prevCmo = i >= 1 ? cmoList[i - 1] : 0;
            double prevCmoSignal = i >= 1 ? cmoSignalList[i - 1] : 0;

            var signal = GetRsiSignal(cmo - cmoSignal, prevCmo - prevCmoSignal, cmo, prevCmo, 50, -50);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmo", cmoList },
            { "Signal", cmoSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cmoList;
        stockData.IndicatorName = IndicatorName.ChandeMomentumOscillator;

        return stockData;
    }
}
