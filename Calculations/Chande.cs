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
        List<decimal> openCloseList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentOpen = openList[i];
            decimal currentClose = inputList[i];

            decimal openClose = currentClose - currentOpen;
            openCloseList.AddRounded(openClose);
        }

        var smaList = GetMovingAverageList(stockData, maType, length, openCloseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList[i];
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;

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
        int length = 9, decimal filter = 3)
    {
        List<decimal> cmoList = new();
        List<decimal> diffList = new();
        List<decimal> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal diff = currentValue - prevValue;
            decimal absDiff = Math.Abs(currentValue - prevValue);
            if (absDiff > filter)
            {
                diff = 0; absDiff = 0;
            }
            diffList.AddRounded(diff);
            absDiffList.AddRounded(absDiff);

            decimal diffSum = diffList.TakeLastExt(length).Sum();
            decimal absDiffSum = absDiffList.TakeLastExt(length).Sum();

            decimal cmo = absDiffSum != 0 ? MinOrMax(100 * diffSum / absDiffSum, 100, -100) : 0;
            cmoList.AddRounded(cmo);
        }

        var cmoSignalList = GetMovingAverageList(stockData, maType, length, cmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal cmo = cmoList[i];
            decimal cmoSignal = cmoSignalList[i];
            decimal prevCmo = i >= 1 ? cmoList[i - 1] : 0;
            decimal prevCmoSignal = i >= 1 ? cmoSignalList[i - 1] : 0;

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
        List<decimal> cmoAbsList = new();
        List<decimal> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorValue = i >= length ? inputList[i - length] : 0;
            decimal prevCmoAbs1 = i >= 1 ? cmoAbsList[i - 1] : 0;
            decimal prevCmoAbs2 = i >= 2 ? cmoAbsList[i - 2] : 0;

            decimal absDiff = Math.Abs(currentValue - prevValue);
            absDiffList.AddRounded(absDiff);

            decimal num = Math.Abs(100 * (currentValue - priorValue));
            decimal denom = absDiffList.TakeLastExt(length).Sum();

            decimal cmoAbs = denom != 0 ? MinOrMax(num / denom, 100, 0) : 0;
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
        List<decimal> cmoAvgList = new();
        List<decimal> diffList = new();
        List<decimal> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList[i];
            decimal prevPrice = i >= 1 ? inputList[i - 1] : 0;
            decimal prevCmoAvg1 = i >= 1 ? cmoAvgList[i - 1] : 0;
            decimal prevCmoAvg2 = i >= 2 ? cmoAvgList[i - 2] : 0;

            decimal diff = currentPrice - prevPrice;
            diffList.AddRounded(diff);

            decimal absDiff = Math.Abs(diff);
            absDiffList.AddRounded(absDiff);

            decimal diffSum1 = diffList.TakeLastExt(length1).Sum();
            decimal absSum1 = absDiffList.TakeLastExt(length1).Sum();
            decimal diffSum2 = diffList.TakeLastExt(length2).Sum();
            decimal absSum2 = absDiffList.TakeLastExt(length2).Sum();
            decimal diffSum3 = diffList.TakeLastExt(length3).Sum();
            decimal absSum3 = absDiffList.TakeLastExt(length3).Sum();
            decimal temp1 = absSum1 != 0 ? MinOrMax(diffSum1 / absSum1, 1, -1) : 0;
            decimal temp2 = absSum2 != 0 ? MinOrMax(diffSum2 / absSum2, 1, -1) : 0;
            decimal temp3 = absSum3 != 0 ? MinOrMax(diffSum3 / absSum3, 1, -1) : 0;

            decimal cmoAvg = 100 * ((temp1 + temp2 + temp3) / 3);
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
        List<decimal> cmoAbsAvgList = new();
        List<decimal> diffList = new();
        List<decimal> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList[i];
            decimal prevPrice = i >= 1 ? inputList[i - 1] : 0;
            decimal prevCmoAbsAvg1 = i >= 1 ? cmoAbsAvgList[i - 1] : 0;
            decimal prevCmoAbsAvg2 = i >= 2 ? cmoAbsAvgList[i - 2] : 0;

            decimal diff = currentPrice - prevPrice;
            diffList.AddRounded(diff);

            decimal absDiff = Math.Abs(diff);
            absDiffList.AddRounded(absDiff);

            decimal diffSum1 = diffList.TakeLastExt(length1).Sum();
            decimal absSum1 = absDiffList.TakeLastExt(length1).Sum();
            decimal diffSum2 = diffList.TakeLastExt(length2).Sum();
            decimal absSum2 = absDiffList.TakeLastExt(length2).Sum();
            decimal diffSum3 = diffList.TakeLastExt(length3).Sum();
            decimal absSum3 = absDiffList.TakeLastExt(length3).Sum();
            decimal temp1 = absSum1 != 0 ? MinOrMax(diffSum1 / absSum1, 1, -1) : 0;
            decimal temp2 = absSum2 != 0 ? MinOrMax(diffSum2 / absSum2, 1, -1) : 0;
            decimal temp3 = absSum3 != 0 ? MinOrMax(diffSum3 / absSum3, 1, -1) : 0;

            decimal cmoAbsAvg = Math.Abs(100 * ((temp1 + temp2 + temp3) / 3));
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
        List<decimal> valueDiff1List = new();
        List<decimal> valueDiff2List = new();
        List<decimal> dmiList = new();
        List<decimal> eList = new();
        List<decimal> sList = new();
        List<decimal> cmo5RatioList = new();
        List<decimal> cmo10RatioList = new();
        List<decimal> cmo20RatioList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
        var stdDev10List = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        var stdDev20List = CalculateStandardDeviationVolatility(stockData, maType, length3).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal valueDiff1 = currentValue > prevValue ? currentValue - prevValue : 0;
            valueDiff1List.AddRounded(valueDiff1);

            decimal valueDiff2 = currentValue < prevValue ? prevValue - currentValue : 0;
            valueDiff2List.AddRounded(valueDiff2);

            decimal cmo51 = valueDiff1List.TakeLastExt(length1).Sum();
            decimal cmo52 = valueDiff2List.TakeLastExt(length1).Sum();
            decimal cmo101 = valueDiff1List.TakeLastExt(length2).Sum();
            decimal cmo102 = valueDiff2List.TakeLastExt(length2).Sum();
            decimal cmo201 = valueDiff1List.TakeLastExt(length3).Sum();
            decimal cmo202 = valueDiff2List.TakeLastExt(length3).Sum();

            decimal cmo5Ratio = cmo51 + cmo52 != 0 ? MinOrMax(100 * (cmo51 - cmo52) / (cmo51 + cmo52), 100, -100) : 0;
            cmo5RatioList.AddRounded(cmo5Ratio);

            decimal cmo10Ratio = cmo101 + cmo102 != 0 ? MinOrMax(100 * (cmo101 - cmo102) / (cmo101 + cmo102), 100, -100) : 0;
            cmo10RatioList.AddRounded(cmo10Ratio);

            decimal cmo20Ratio = cmo201 + cmo202 != 0 ? MinOrMax(100 * (cmo201 - cmo202) / (cmo201 + cmo202), 100, -100) : 0;
            cmo20RatioList.AddRounded(cmo20Ratio);
        }

        var cmo5List = GetMovingAverageList(stockData, maType, smoothLength, cmo5RatioList);
        var cmo10List = GetMovingAverageList(stockData, maType, smoothLength, cmo10RatioList);
        var cmo20List = GetMovingAverageList(stockData, maType, smoothLength, cmo20RatioList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDev5 = stdDevList[i];
            decimal stdDev10 = stdDev10List[i];
            decimal stdDev20 = stdDev20List[i];
            decimal cmo5 = cmo5List[i];
            decimal cmo10 = cmo10List[i];
            decimal cmo20 = cmo20List[i];

            decimal dmi = stdDev5 + stdDev10 + stdDev20 != 0 ?
                MinOrMax(((stdDev5 * cmo5) + (stdDev10 * cmo10) + (stdDev20 * cmo20)) / (stdDev5 + stdDev10 + stdDev20), 100, -100) : 0;
            dmiList.AddRounded(dmi);

            decimal prevS = sList.LastOrDefault();
            decimal s = dmiList.TakeLastExt(length1).Average();
            sList.AddRounded(s);

            decimal prevE = eList.LastOrDefault();
            decimal e = CalculateEMA(dmi, prevE, smoothLength);
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
        List<decimal> avgDisparityIndexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var firstEmaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var secondEmaList = GetMovingAverageList(stockData, maType, length2, inputList);
        var thirdEmaList = GetMovingAverageList(stockData, maType, length3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal firstEma = firstEmaList[i];
            decimal secondEma = secondEmaList[i];
            decimal thirdEma = thirdEmaList[i];
            decimal firstDisparityIndex = currentValue != 0 ? (currentValue - firstEma) / currentValue * 100 : 0;
            decimal secondDisparityIndex = currentValue != 0 ? (currentValue - secondEma) / currentValue * 100 : 0;
            decimal thirdDisparityIndex = currentValue != 0 ? (currentValue - thirdEma) / currentValue * 100 : 0;

            decimal prevAvgDisparityIndex = avgDisparityIndexList.LastOrDefault();
            decimal avgDisparityIndex = (firstDisparityIndex + secondDisparityIndex + thirdDisparityIndex) / 3;
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
        List<decimal> r2RawList = new();
        List<decimal> tempValueList = new();
        List<decimal> indexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal index = i;
            indexList.AddRounded(index);

            decimal currentValue = inputList[i];
            tempValueList.AddRounded(currentValue);

            var r2 = GoodnessOfFit.RSquared(indexList.TakeLastExt(length).Select(x => (double)x),
                tempValueList.TakeLastExt(length).Select(x => (double)x));
            r2 = IsValueNullOrInfinity(r2) ? 0 : r2;
            r2RawList.AddRounded((decimal)r2);
        }

        var r2SmoothedList = GetMovingAverageList(stockData, maType, smoothLength, r2RawList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal r2Sma = r2SmoothedList[i];
            decimal prevR2Sma1 = i >= 1 ? r2SmoothedList[i - 1] : 0;
            decimal prevR2Sma2 = i >= 2 ? r2SmoothedList[i - 2] : 0;

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
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20, decimal alpha1 = 0.2m, decimal alpha2 = 0.04m)
    {
        List<decimal> vidya1List = new();
        List<decimal> vidya2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var stdDevEmaList = GetMovingAverageList(stockData, maType, length, stdDevList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentStdDev = stdDevList[i];
            decimal currentStdDevEma = stdDevEmaList[i];
            decimal prevVidya1 = i >= 1 ? vidya1List.LastOrDefault() : currentValue;
            decimal prevVidya2 = i >= 1 ? vidya2List.LastOrDefault() : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ratio = currentStdDevEma != 0 ? currentStdDev / currentStdDevEma : 0;

            decimal vidya1 = (alpha1 * ratio * currentValue) + ((1 - (alpha1 * ratio)) * prevVidya1);
            vidya1List.AddRounded(vidya1);

            decimal vidya2 = (alpha2 * ratio * currentValue) + ((1 - (alpha2 * ratio)) * prevVidya2);
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
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> tsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];

            decimal prevTs = tsList.LastOrDefault();
            decimal ts = 0;
            for (int j = startLength; j <= endLength; j++)
            {
                decimal prevValue = i >= j ? inputList[i - j] : 0;
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
        List<decimal> pfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentLinReg = linRegList[i];

            decimal prevPf = pfList.LastOrDefault();
            decimal pf = currentValue != 0 ? (currentValue - currentLinReg) * 100 / currentValue : 0;
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
        List<decimal> imiUnfilteredList = new();
        List<decimal> gainsList = new();
        List<decimal> lossesList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList[i];
            decimal currentOpen = openList[i];
            decimal prevImi1 = i >= 1 ? imiUnfilteredList[i - 1] : 0;
            decimal prevImi2 = i >= 2 ? imiUnfilteredList[i - 2] : 0;

            decimal prevGains = gainsList.LastOrDefault();
            decimal gains = currentClose > currentOpen ? prevGains + (currentClose - currentOpen) : 0;
            gainsList.AddRounded(gains);

            decimal prevLosses = lossesList.LastOrDefault();
            decimal losses = currentClose < currentOpen ? prevLosses + (currentOpen - currentClose) : 0;
            lossesList.AddRounded(losses);

            decimal upt = gainsList.TakeLastExt(length).Sum();
            decimal dnt = lossesList.TakeLastExt(length).Sum();

            decimal imiUnfiltered = upt + dnt != 0 ? MinOrMax(100 * upt / (upt + dnt), 100, 0) : 0;
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
        List<decimal> cmoList = new();
        List<decimal> cmoPosChgList = new();
        List<decimal> cmoNegChgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal diff = currentValue - prevValue;

            decimal negChg = diff < 0 ? Math.Abs(diff) : 0;
            cmoNegChgList.AddRounded(negChg);

            decimal posChg = diff > 0 ? diff : 0;
            cmoPosChgList.AddRounded(posChg);

            decimal negSum = cmoNegChgList.TakeLastExt(length).Sum();
            decimal posSum = cmoPosChgList.TakeLastExt(length).Sum();

            decimal cmo = posSum + negSum != 0 ? MinOrMax((posSum - negSum) / (posSum + negSum) * 100, 100, -100) : 0;
            cmoList.AddRounded(cmo);
        }

        var cmoSignalList = GetMovingAverageList(stockData, maType, signalLength, cmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal cmo = cmoList[i];
            decimal cmoSignal = cmoSignalList[i];
            decimal prevCmo = i >= 1 ? cmoList[i - 1] : 0;
            decimal prevCmoSignal = i >= 1 ? cmoSignalList[i - 1] : 0;

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
