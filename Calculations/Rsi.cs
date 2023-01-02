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
    /// Calculates the relative strength index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="movingAvgType">Average type of the moving.</param>
    /// <param name="length">The length.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateRelativeStrengthIndex(this StockData stockData, MovingAvgType movingAvgType = MovingAvgType.WildersSmoothingMethod,
        int length = 14, int signalLength = 3)
    {
        List<double> rsiList = new();
        List<double> lossList = new();
        List<double> gainList = new();
        List<double> rsiHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double priceChg = MinPastValues(i, 1, currentValue - prevValue);

            double loss = priceChg < 0 ? Math.Abs(priceChg) : 0;
            lossList.AddRounded(loss);

            double gain = priceChg > 0 ? priceChg : 0;
            gainList.AddRounded(gain);
        }

        var avgGainList = GetMovingAverageList(stockData, movingAvgType, length, gainList);
        var avgLossList = GetMovingAverageList(stockData, movingAvgType, length, lossList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double avgGain = avgGainList[i];
            double avgLoss = avgLossList[i];
            double rs = avgLoss != 0 ? avgGain / avgLoss : 0;

            double rsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.AddRounded(rsi);
        }

        var rsiSignalList = GetMovingAverageList(stockData, movingAvgType, signalLength, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;
            double rsiSignal = rsiSignalList[i];

            double prevRsiHistogram = rsiHistogramList.LastOrDefault();
            double rsiHistogram = rsi - rsiSignal;
            rsiHistogramList.AddRounded(rsiHistogram);

            var signal = GetRsiSignal(rsiHistogram, prevRsiHistogram, rsi, prevRsi, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsi", rsiList },
            { "Signal", rsiSignalList },
            { "Histogram", rsiHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.RelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the connors relative strength.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length1">Length of the streak.</param>
    /// <param name="length2">Length of the rsi.</param>
    /// <param name="length3">Length of the roc.</param>
    /// <returns></returns>
    public static StockData CalculateConnorsRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length1 = 2, int length2 = 3, int length3 = 100)
    {
        List<double> streakList = new();
        List<double> tempList = new();
        List<double> pctRankList = new();
        List<double> connorsRsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length2, length2).CustomValuesList;
        var rocList = CalculateRateOfChange(stockData, length3).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double roc = rocList[i];
            tempList.AddRounded(roc);

            var lookBackList = tempList.TakeLastExt(length3).Take(length3 - 1).ToList();
            int count = lookBackList.Where(x => x <= roc).Count();
            double pctRank = MinOrMax((double)count / length3 * 100, 100, 0);
            pctRankList.AddRounded(pctRank);

            double prevStreak = streakList.LastOrDefault();
            double streak = currentValue > prevValue ? prevStreak >= 0 ? prevStreak + 1 : 1 : currentValue < prevValue ? prevStreak <= 0 ?
                prevStreak - 1 : -1 : 0;
            streakList.AddRounded(streak);
        }

        stockData.CustomValuesList = streakList;
        var rsiStreakList = CalculateRelativeStrengthIndex(stockData, maType, length1, length1).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentRsi = rsiList[i];
            double percentRank = pctRankList[i];
            double streakRsi = rsiStreakList[i];
            double prevConnorsRsi1 = i >= 1 ? connorsRsiList[i - 1] : 0;
            double prevConnorsRsi2 = i >= 2 ? connorsRsiList[i - 2] : 0;

            double connorsRsi = MinOrMax((currentRsi + percentRank + streakRsi) / 3, 100, 0);
            connorsRsiList.AddRounded(connorsRsi);

            var signal = GetRsiSignal(connorsRsi - prevConnorsRsi1, prevConnorsRsi1 - prevConnorsRsi2, connorsRsi, prevConnorsRsi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsi", rsiList },
            { "PctRank", pctRankList },
            { "StreakRsi", rsiStreakList },
            { "ConnorsRsi", connorsRsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = connorsRsiList;
        stockData.IndicatorName = IndicatorName.ConnorsRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the asymmetrical relative strength.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAsymmetricalRelativeStrengthIndex(this StockData stockData, int length = 14)
    {
        List<double> rocList = new();
        List<double> upSumList = new();
        List<double> downSumList = new();
        List<double> arsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevArsi1 = i >= 1 ? arsiList[i - 1] : 0;
            double prevArsi2 = i >= 2 ? arsiList[i - 2] : 0;
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double roc = prevValue != 0 ? MinPastValues(i, 1, currentValue - prevValue) / prevValue * 100 : 0;
            rocList.AddRounded(roc);

            double upCount = rocList.TakeLastExt(length).Where(x => x >= 0).Count();
            double upAlpha = upCount != 0 ? 1 / upCount : 0;
            double posRoc = roc > 0 ? roc : 0;
            double negRoc = roc < 0 ? Math.Abs(roc) : 0;

            double prevUpSum = upSumList.LastOrDefault();
            double upSum = (upAlpha * posRoc) + ((1 - upAlpha) * prevUpSum);
            upSumList.AddRounded(upSum);

            double downCount = length - upCount;
            double downAlpha = downCount != 0 ? 1 / downCount : 0;

            double prevDownSum = downSumList.LastOrDefault();
            double downSum = (downAlpha * negRoc) + ((1 - downAlpha) * prevDownSum);
            downSumList.AddRounded(downSum);

            double ars = downSum != 0 ? upSum / downSum : 0;
            double arsi = downSum == 0 ? 100 : upSum == 0 ? 0 : MinOrMax(100 - (100 / (1 + ars)), 100, 0);
            arsiList.AddRounded(arsi);

            var signal = GetRsiSignal(arsi - prevArsi1, prevArsi1 - prevArsi2, arsi, prevArsi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Arsi", arsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = arsiList;
        stockData.IndicatorName = IndicatorName.AsymmetricalRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Adaptive Relative Strength Index
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14)
    {
        List<double> arsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double alpha = 2 * Math.Abs((rsi / 100) - 0.5);

            double prevArsi = arsiList.LastOrDefault();
            double arsi = (alpha * currentValue) + ((1 - alpha) * prevArsi);
            arsiList.AddRounded(arsi);

            var signal = GetCompareSignal(currentValue - arsi, prevValue - prevArsi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Arsi", arsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = arsiList;
        stockData.IndicatorName = IndicatorName.AdaptiveRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the average absolute error normalization.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAverageAbsoluteErrorNormalization(this StockData stockData, int length = 14)
    {
        List<double> yList = new();
        List<double> eList = new();
        List<double> eAbsList = new();
        List<double> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevY = i >= 1 ? yList[i - 1] : currentValue;
            double prevA1 = i >= 1 ? aList[i - 1] : 0;
            double prevA2 = i >= 2 ? aList[i - 2] : 0;

            double e = currentValue - prevY;
            eList.AddRounded(e);

            double eAbs = Math.Abs(e);
            eAbsList.AddRounded(eAbs);

            double eAbsSma = eAbsList.TakeLastExt(length).Average();
            double eSma = eList.TakeLastExt(length).Average();

            double a = eAbsSma != 0 ? MinOrMax(eSma / eAbsSma, 1, -1) : 0;
            aList.AddRounded(a);

            double y = currentValue + (a * eAbsSma);
            yList.AddRounded(y);

            var signal = GetRsiSignal(a - prevA1, prevA1 - prevA2, a, prevA1, 0.8, -0.8);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Aaen", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.AverageAbsoluteErrorNormalization;

        return stockData;
    }

    /// <summary>
    /// Calculates the Apirine Slow Relative Strength Index
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateApirineSlowRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14, int smoothLength = 6)
    {
        List<double> r2List = new();
        List<double> r3List = new();
        List<double> rrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double r1 = emaList[i];

            double r2 = currentValue > r1 ? currentValue - r1 : 0;
            r2List.AddRounded(r2);

            double r3 = currentValue < r1 ? r1 - currentValue : 0;
            r3List.AddRounded(r3);
        }

        var r4List = GetMovingAverageList(stockData, maType, length, r2List);
        var r5List = GetMovingAverageList(stockData, maType, length, r3List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double r4 = r4List[i];
            double r5 = r5List[i];
            double prevRr1 = i >= 1 ? rrList[i - 1] : 0;
            double prevRr2 = i >= 2 ? rrList[i - 2] : 0;
            double rs = r5 != 0 ? r4 / r5 : 0;

            double rr = r5 == 0 ? 100 : r4 == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rrList.AddRounded(rr);

            var signal = GetRsiSignal(rr - prevRr1, prevRr1 - prevRr2, rr, prevRr1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Asrsi", rrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rrList;
        stockData.IndicatorName = IndicatorName.ApirineSlowRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Breakout Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <returns></returns>
    public static StockData CalculateBreakoutRelativeStrengthIndex(this StockData stockData, InputName inputName = InputName.FullTypicalPrice,
        int length = 14, int lbLength = 2)
    {
        List<double> brsiList = new();
        List<double> posPowerList = new();
        List<double> boPowerList = new();
        List<double> negPowerList = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = closeList[i];
            double currentOpen = openList[i];
            double prevBrsi1 = i >= 1 ? brsiList[i - 1] : 0;
            double prevBrsi2 = i >= 2 ? brsiList[i - 2] : 0;

            double currentVolume = volumeList[i];
            tempList.AddRounded(currentVolume);

            double boVolume = tempList.TakeLastExt(lbLength).Sum();
            double boStrength = currentHigh - currentLow != 0 ? (currentClose - currentOpen) / (currentHigh - currentLow) : 0;

            double prevBoPower = boPowerList.LastOrDefault();
            double boPower = currentValue * boStrength * boVolume;
            boPowerList.AddRounded(boPower);

            double posPower = boPower > prevBoPower ? Math.Abs(boPower) : 0;
            posPowerList.AddRounded(posPower);

            double negPower = boPower < prevBoPower ? Math.Abs(boPower) : 0;
            negPowerList.AddRounded(negPower);

            double posPowerSum = posPowerList.TakeLastExt(length).Sum();
            double negPowerSum = negPowerList.TakeLastExt(length).Sum();
            double boRatio = negPowerSum != 0 ? posPowerSum / negPowerSum : 0;

            double brsi = negPowerSum == 0 ? 100 : posPowerSum == 0 ? 0 : MinOrMax(100 - (100 / (1 + boRatio)), 100, 0);
            brsiList.AddRounded(brsi);

            var signal = GetRsiSignal(brsi - prevBrsi1, prevBrsi1 - prevBrsi2, brsi, prevBrsi1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Brsi", brsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = brsiList;
        stockData.IndicatorName = IndicatorName.BreakoutRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Liquid Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateLiquidRelativeStrengthIndex(this StockData stockData, int length = 14)
    {
        List<double> numEmaList = new();
        List<double> denEmaList = new();
        List<double> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        double k = (double)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentVolume = volumeList[i];
            double prevVolume = i >= 1 ? volumeList[i - 1] : 0;
            double a = MinPastValues(i, 1, currentValue - prevValue);
            double b = MinPastValues(i, 1, currentVolume - prevVolume);
            double prevC1 = i >= 1 ? cList[i - 1] : 0;
            double prevC2 = i >= 2 ? cList[i - 2] : 0;
            double num = Math.Max(a, 0) * Math.Max(b, 0);
            double den = Math.Abs(a) * Math.Abs(b);

            double prevNumEma = numEmaList.LastOrDefault();
            double numEma = (num * k) + (prevNumEma * (1 - k));
            numEmaList.AddRounded(numEma);

            double prevDenEma = denEmaList.LastOrDefault();
            double denEma = (den * k) + (prevDenEma * (1 - k));
            denEmaList.AddRounded(denEma);

            double c = denEma != 0 ? MinOrMax(100 * numEma / denEma, 100, 0) : 0;
            cList.AddRounded(c);

            var signal = GetRsiSignal(c - prevC1, prevC1 - prevC2, c, prevC1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lrsi", cList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cList;
        stockData.IndicatorName = IndicatorName.LiquidRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Folded Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFoldedRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<double> absRsiList = new();
        List<double> frsiList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];

            double absRsi = 2 * Math.Abs(rsi - 50);
            absRsiList.AddRounded(absRsi);

            double frsi = absRsiList.TakeLastExt(length).Sum();
            frsiList.AddRounded(frsi);
        }

        var frsiMaList = GetMovingAverageList(stockData, maType, length, frsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double frsi = frsiList[i];
            double frsiMa = frsiMaList[i];
            double prevFrsi = i >= 1 ? frsiList[i - 1] : 0;
            double prevFrsiMa = i >= 1 ? frsiMaList[i - 1] : 0;

            var signal = GetRsiSignal(frsi - frsiMa, prevFrsi - prevFrsiMa, frsi, prevFrsi, 50, 10);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Frsi", frsiList },
            { "Signal", frsiMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = frsiList;
        stockData.IndicatorName = IndicatorName.FoldedRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Weighted Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateVolumeWeightedRelativeStrengthIndex(this StockData stockData,
        MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 10, int smoothLength = 3)
    {
        List<double> maxList = new();
        List<double> minList = new();
        List<double> rsiScaledList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double volume = volumeList[i];

            double max = Math.Max(MinPastValues(i, 1, currentValue - prevValue) * volume, 0);
            maxList.AddRounded(max);

            double min = -Math.Min(MinPastValues(i, 1, currentValue - prevValue) * volume, 0);
            minList.AddRounded(min);
        }

        var upList = GetMovingAverageList(stockData, maType, length, maxList);
        var dnList = GetMovingAverageList(stockData, maType, length, minList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double up = upList[i];
            double dn = dnList[i];
            double rsiRaw = dn == 0 ? 100 : up == 0 ? 0 : 100 - (100 / (1 + (up / dn)));

            double rsiScale = (rsiRaw * 2) - 100;
            rsiScaledList.AddRounded(rsiScale);
        }

        var rsiList = GetMovingAverageList(stockData, maType, smoothLength, rsiScaledList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double prevRsi1 = i >= 1 ? rsiList[i - 1] : 0;
            double prevRsi2 = i >= 2 ? rsiList[i - 2] : 0;

            var signal = GetCompareSignal(rsi - prevRsi1, prevRsi1 - prevRsi2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vwrsi", rsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.VolumeWeightedRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Rapid Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRapidRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<double> upChgList = new();
        List<double> downChgList = new();
        List<double> rapidRsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double chg = MinPastValues(i, 1, currentValue - prevValue);

            double upChg = i >= 1 && chg > 0 ? chg : 0;
            upChgList.AddRounded(upChg);

            double downChg = i >= 1 && chg < 0 ? Math.Abs(chg) : 0;
            downChgList.AddRounded(downChg);

            double upChgSum = upChgList.TakeLastExt(length).Sum();
            double downChgSum = downChgList.TakeLastExt(length).Sum();
            double rs = downChgSum != 0 ? upChgSum / downChgSum : 0;

            double rapidRsi = downChgSum == 0 ? 100 : upChgSum == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rapidRsiList.AddRounded(rapidRsi);
        }

        var rrsiEmaList = GetMovingAverageList(stockData, maType, length, rapidRsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rapidRsi = rrsiEmaList[i];
            double prevRapidRsi1 = i >= 1 ? rrsiEmaList[i - 1] : 0;
            double prevRapidRsi2 = i >= 2 ? rrsiEmaList[i - 2] : 0;

            var signal = GetRsiSignal(rapidRsi - prevRapidRsi1, prevRapidRsi1 - prevRapidRsi2, rapidRsi, prevRapidRsi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rrsi", rapidRsiList },
            { "Signal", rrsiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rapidRsiList;
        stockData.IndicatorName = IndicatorName.RapidRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Recursive Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRecursiveRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14)
    {
        List<double> chgList = new();
        List<double> bList = new();
        List<double> avgRsiList = new();
        List<double> avgList = new();
        List<double> gainList = new();
        List<double> lossList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;

            double chg = MinPastValues(i, length, currentValue - prevValue);
            chgList.AddRounded(chg);
        }

        var srcList = GetMovingAverageList(stockData, maType, length, chgList);
        stockData.CustomValuesList = srcList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, length: length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double src = srcList[i];
            double prevB1 = i >= 1 ? bList[i - 1] : 0;
            double prevB2 = i >= 2 ? bList[i - 2] : 0;

            double b = 0, avg = 0, gain = 0, loss = 0, avgRsi = 0;
            for (int j = 1; j <= length; j++)
            {
                double prevB = i >= j ? bList[i - j] : src;
                double prevAvg = i >= j ? avgList[i - j] : 0;
                double prevGain = i >= j ? gainList[i - j] : 0;
                double prevLoss = i >= j ? lossList[i - j] : 0;
                double k = (double)j / length;
                double a = rsi * ((double)length / j);
                avg = (a + prevB) / 2;
                double avgChg = avg - prevAvg;
                gain = avgChg > 0 ? avgChg : 0;
                loss = avgChg < 0 ? Math.Abs(avgChg) : 0;
                double avgGain = (gain * k) + (prevGain * (1 - k));
                double avgLoss = (loss * k) + (prevLoss * (1 - k));
                double rs = avgLoss != 0 ? avgGain / avgLoss : 0;
                avgRsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 1, 0);
                b = avgRsiList.Count >= length ? avgRsiList.TakeLastExt(length).Average() : avgRsi;
            }
            bList.AddRounded(b);
            avgList.AddRounded(avg);
            gainList.AddRounded(gain);
            lossList.AddRounded(loss);
            avgRsiList.AddRounded(avgRsi);

            var signal = GetRsiSignal(b - prevB1, prevB1 - prevB2, b, prevB1, 0.8, 0.2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rrsi", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.RecursiveRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Momenta Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateMomentaRelativeStrengthIndex(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 2, int length2 = 14)
    {
        List<double> rsiList = new();
        List<double> srcLcList = new();
        List<double> hcSrcList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double hc = highestList[i];
            double lc = lowestList[i];

            double srcLc = currentValue - lc;
            srcLcList.AddRounded(srcLc);

            double hcSrc = hc - currentValue;
            hcSrcList.AddRounded(hcSrc);
        }

        var topList = GetMovingAverageList(stockData, maType, length2, srcLcList);
        var botList = GetMovingAverageList(stockData, maType, length2, hcSrcList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double top = topList[i];
            double bot = botList[i];
            double rs = bot != 0 ? MinOrMax(top / bot, 1, 0) : 0;

            double rsi = bot == 0 ? 100 : top == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.AddRounded(rsi);
        }

        var rsiEmaList = GetMovingAverageList(stockData, maType, length2, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double rsiEma = rsiEmaList[i];
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;
            double prevRsiEma = i >= 1 ? rsiEmaList[i - 1] : 0;

            var signal = GetRsiSignal(rsi - rsiEma, prevRsi - prevRsiEma, rsi, prevRsi, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mrsi", rsiList },
            { "Signal", rsiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.MomentaRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Double Smoothed Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateDoubleSmoothedRelativeStrengthIndex(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 2, int length2 = 5, int length3 = 25)
    {
        List<double> rsiList = new();
        List<double> srcLcList = new();
        List<double> hcSrcList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double hc = highestList[i];
            double lc = lowestList[i];

            double srcLc = currentValue - lc;
            srcLcList.AddRounded(srcLc);

            double hcSrc = hc - currentValue;
            hcSrcList.AddRounded(hcSrc);
        }

        var topEma1List = GetMovingAverageList(stockData, maType, length2, srcLcList);
        var topEma2List = GetMovingAverageList(stockData, maType, length3, topEma1List);
        var botEma1List = GetMovingAverageList(stockData, maType, length2, hcSrcList);
        var botEma2List = GetMovingAverageList(stockData, maType, length3, botEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double top = topEma2List[i];
            double bot = botEma2List[i];
            double rs = bot != 0 ? MinOrMax(top / bot, 1, 0) : 0;

            double rsi = bot == 0 ? 100 : top == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.AddRounded(rsi);
        }

        var rsiEmaList = GetMovingAverageList(stockData, maType, length3, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double rsiEma = rsiEmaList[i];
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;
            double prevRsiEma = i >= 1 ? rsiEmaList[i - 1] : 0;

            var signal = GetRsiSignal(rsi - rsiEma, prevRsi - prevRsiEma, rsi, prevRsi, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dsrsi", rsiList },
            { "Signal", rsiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.DoubleSmoothedRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Dominant Cycle Tuned Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDominantCycleTunedRelativeStrengthIndex(this StockData stockData, int length = 5)
    {
        List<double> aList = new();
        List<double> bList = new();
        List<double> rsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var v1List = CalculateEhlersAdaptiveCyberCycle(stockData, length).OutputValues["Period"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double v1 = v1List[i];
            double p = v1 != 0 ? 1 / v1 : 0.07;
            double price = inputList[i];
            double prevPrice = i >= 1 ? inputList[i - 1] : 0;
            double aChg = price > prevPrice ? Math.Abs(price - prevPrice) : 0;
            double bChg = price < prevPrice ? Math.Abs(price - prevPrice) : 0;
            double prevRsi1 = i >= 1 ? rsiList[i - 1] : 0;
            double prevRsi2 = i >= 2 ? rsiList[i - 2] : 0;

            double prevA = i >= 1 ? aList[i - 1] : aChg;
            double a = (p * aChg) + ((1 - p) * prevA);
            aList.AddRounded(a);

            double prevB = i >= 1 ? bList[i - 1] : bChg;
            double b = (p * bChg) + ((1 - p) * prevB);
            bList.AddRounded(b);

            double r = b != 0 ? a / b : 0;
            double rsi = b == 0 ? 100 : a == 0 ? 0 : MinOrMax(100 - (100 / (1 + r)), 100, 0);
            rsiList.AddRounded(rsi);

            var signal = GetRsiSignal(rsi - prevRsi1, prevRsi1 - prevRsi2, rsi, prevRsi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "DctRsi", rsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.DominantCycleTunedRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Self Adjusting Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothingLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateSelfAdjustingRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14, int smoothingLength = 21, double mult = 2)
    {
        List<double> obList = new();
        List<double> osList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;
        stockData.CustomValuesList = rsiList;
        var rsiStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var rsiSmaList = GetMovingAverageList(stockData, maType, smoothingLength, rsiList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsiStdDev = rsiStdDevList[i];
            double rsi = rsiList[i];
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;
            double adjustingStdDev = mult * rsiStdDev;
            double rsiSma = rsiSmaList[i];
            double prevRsiSma = i >= 1 ? rsiSmaList[i - 1] : 0;

            double obStdDev = 50 + adjustingStdDev;
            obList.AddRounded(obStdDev);

            double osStdDev = 50 - adjustingStdDev;
            osList.AddRounded(osStdDev);

            var signal = GetRsiSignal(rsi - rsiSma, prevRsi - prevRsiSma, rsi, prevRsi, obStdDev, osStdDev);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "SaRsi", rsiList },
            { "Signal", rsiSmaList },
            { "ObLevel", obList },
            { "OsLevel", osList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.SelfAdjustingRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stochastic Connors Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="smoothLength1"></param>
    /// <param name="smoothLength2"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticConnorsRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length1 = 2, int length2 = 3, int length3 = 100, int smoothLength1 = 3, int smoothLength2 = 3)
    {
        List<Signal> signalsList = new();

        var connorsRsiList = CalculateConnorsRelativeStrengthIndex(stockData, maType, length1, length2, length3).CustomValuesList;
        stockData.CustomValuesList = connorsRsiList;
        var stochasticList = CalculateStochasticOscillator(stockData, maType, length2, smoothLength1, smoothLength2);
        var fastDList = stochasticList.OutputValues["FastD"];
        var slowDList = stochasticList.OutputValues["SlowD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double smaK = fastDList[i];
            double smaD = slowDList[i];
            double prevSmak = i >= 1 ? fastDList[i - 1] : 0;
            double prevSmad = i >= 1 ? slowDList[i - 1] : 0;

            var signal = GetRsiSignal(smaK - smaD, prevSmak - prevSmad, smaK, prevSmak, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "SaRsi", fastDList },
            { "Signal", slowDList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fastDList;
        stockData.IndicatorName = IndicatorName.StochasticConnorsRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the CCT Stochastic Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="smoothLength1"></param>
    /// <param name="smoothLength2"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateCCTStochRSI(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 5, int length2 = 8, int length3 = 13, int length4 = 14, int length5 = 21, int smoothLength1 = 3, int smoothLength2 = 8,
        int signalLength = 9)
    {
        List<double> type1List = new();
        List<double> type2List = new();
        List<double> type3List = new();
        List<double> type4List = new();
        List<double> type5List = new();
        List<double> type6List = new();
        List<double> tempRsi21List = new();
        List<double> tempRsi14List = new();
        List<double> tempRsi13List = new();
        List<double> tempRsi5List = new();
        List<double> tempRsi8List = new();
        List<double> typeCustomList = new();
        List<Signal> signalsList = new();

        var rsi5List = CalculateRelativeStrengthIndex(stockData, maType, length: length1).CustomValuesList;
        var rsi8List = CalculateRelativeStrengthIndex(stockData, maType, length: length2).CustomValuesList;
        var rsi13List = CalculateRelativeStrengthIndex(stockData, maType, length: length3).CustomValuesList;
        var rsi14List = CalculateRelativeStrengthIndex(stockData, maType, length: length4).CustomValuesList;
        var rsi21List = CalculateRelativeStrengthIndex(stockData, maType, length: length5).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentRSI5 = rsi5List[i];
            tempRsi5List.AddRounded(currentRSI5);

            double currentRSI8 = rsi8List[i];
            tempRsi8List.AddRounded(currentRSI8);

            double currentRSI13 = rsi13List[i];
            tempRsi13List.AddRounded(currentRSI13);

            double currentRSI14 = rsi14List[i];
            tempRsi14List.AddRounded(currentRSI14);

            double currentRSI21 = rsi21List[i];
            tempRsi21List.AddRounded(currentRSI21);

            double lowestX1 = tempRsi21List.TakeLastExt(length2).Min();
            double lowestZ1 = tempRsi21List.TakeLastExt(length3).Min();
            double highestY1 = tempRsi21List.TakeLastExt(length3).Max();
            double lowestX2 = tempRsi21List.TakeLastExt(length5).Min();
            double lowestZ2 = tempRsi21List.TakeLastExt(length5).Min();
            double highestY2 = tempRsi21List.TakeLastExt(length5).Max();
            double lowestX3 = tempRsi14List.TakeLastExt(length4).Min();
            double lowestZ3 = tempRsi14List.TakeLastExt(length4).Min();
            double highestY3 = tempRsi14List.TakeLastExt(length4).Max();
            double lowestX4 = tempRsi21List.TakeLastExt(length3).Min();
            double lowestZ4 = tempRsi21List.TakeLastExt(length3).Min();
            double highestY4 = tempRsi21List.TakeLastExt(length2).Max();
            double lowestX5 = tempRsi5List.TakeLastExt(length1).Min();
            double lowestZ5 = tempRsi5List.TakeLastExt(length1).Min();
            double highestY5 = tempRsi5List.TakeLastExt(length1).Max();
            double lowestX6 = tempRsi13List.TakeLastExt(length3).Min();
            double lowestZ6 = tempRsi13List.TakeLastExt(length3).Min();
            double highestY6 = tempRsi13List.TakeLastExt(length3).Max();
            double lowestCustom = tempRsi8List.TakeLastExt(length2).Min();
            double highestCustom = tempRsi8List.TakeLastExt(length2).Max();

            double stochRSI1 = highestY1 - lowestZ1 != 0 ? (currentRSI21 - lowestX1) / (highestY1 - lowestZ1) * 100 : 0;
            type1List.AddRounded(stochRSI1);

            double stochRSI2 = highestY2 - lowestZ2 != 0 ? (currentRSI21 - lowestX2) / (highestY2 - lowestZ2) * 100 : 0;
            type2List.AddRounded(stochRSI2);

            double stochRSI3 = highestY3 - lowestZ3 != 0 ? (currentRSI14 - lowestX3) / (highestY3 - lowestZ3) * 100 : 0;
            type3List.AddRounded(stochRSI3);

            double stochRSI4 = highestY4 - lowestZ4 != 0 ? (currentRSI21 - lowestX4) / (highestY4 - lowestZ4) * 100 : 0;
            type4List.AddRounded(stochRSI4);

            double stochRSI5 = highestY5 - lowestZ5 != 0 ? (currentRSI5 - lowestX5) / (highestY5 - lowestZ5) * 100 : 0;
            type5List.AddRounded(stochRSI5);

            double stochRSI6 = highestY6 - lowestZ6 != 0 ? (currentRSI13 - lowestX6) / (highestY6 - lowestZ6) * 100 : 0;
            type6List.AddRounded(stochRSI6);

            double stochCustom = highestCustom - lowestCustom != 0 ? (currentRSI8 - lowestCustom) / (highestCustom - lowestCustom) * 100 : 0;
            typeCustomList.AddRounded(stochCustom);
        }

        var rsiEma4List = GetMovingAverageList(stockData, maType, smoothLength2, type4List);
        var rsiEma5List = GetMovingAverageList(stockData, maType, smoothLength1, type5List);
        var rsiEma6List = GetMovingAverageList(stockData, maType, smoothLength1, type6List);
        var rsiEmaCustomList = GetMovingAverageList(stockData, maType, smoothLength1, typeCustomList);
        var rsiSignalList = GetMovingAverageList(stockData, maType, signalLength, type1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            var rsi = type1List[i];
            var prevRsi = i >= 1 ? type1List[i - 1] : 0;
            var rsiSignal = rsiSignalList[i];
            var prevRsiSignal = i >= 1 ? rsiSignalList[i - 1] : 0;

            var signal = GetRsiSignal(rsi - rsiSignal, prevRsi - prevRsiSignal, rsi, prevRsi, 90, 10);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Type1", type1List },
            { "Type2", type2List },
            { "Type3", type3List },
            { "Type4", rsiEma4List },
            { "Type5", rsiEma5List },
            { "Type6", rsiEma6List },
            { "TypeCustom", rsiEmaCustomList },
            { "Signal", rsiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = type1List;
        stockData.IndicatorName = IndicatorName.CCTStochRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stochastic Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength1"></param>
    /// <param name="smoothLength2"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14, int smoothLength1 = 3, int smoothLength2 = 3)
    {
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;
        stockData.CustomValuesList = rsiList;
        var stoRsiList = CalculateStochasticOscillator(stockData, maType, length, smoothLength1, smoothLength2);
        var stochRsiList = stoRsiList.OutputValues["FastD"];
        var stochRsiSignalList = stoRsiList.OutputValues["SlowD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentRSI = stochRsiList[i];
            double prevStochRsi = i >= 1 ? stochRsiList[i - 1] : 0;
            double currentRsiSignal = stochRsiSignalList[i];
            double prevRsiSignal = i >= 1 ? stochRsiSignalList[i - 1] : 0;

            var signal = GetRsiSignal(currentRSI - currentRsiSignal, prevStochRsi - prevRsiSignal, currentRSI, prevStochRsi, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "StochRsi", stochRsiList },
            { "Signal", stochRsiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stochRsiList;
        stockData.IndicatorName = IndicatorName.StochasticRelativeStrengthIndex;

        return stockData;
    }
}