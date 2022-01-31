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
        List<decimal> rsiList = new();
        List<decimal> rsList = new();
        List<decimal> lossList = new();
        List<decimal> gainList = new();
        List<decimal> rsiHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priceChg = currentValue - prevValue;

            decimal loss = priceChg < 0 ? Math.Abs(priceChg) : 0;
            lossList.AddRounded(loss);

            decimal gain = priceChg > 0 ? priceChg : 0;
            gainList.AddRounded(gain);
        }

        var avgGainList = GetMovingAverageList(stockData, movingAvgType, length, gainList);
        var avgLossList = GetMovingAverageList(stockData, movingAvgType, length, lossList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal avgGain = avgGainList.ElementAtOrDefault(i);
            decimal avgLoss = avgLossList.ElementAtOrDefault(i);

            decimal rs = avgLoss != 0 ? MinOrMax(avgGain / avgLoss, 1, 0) : 0;
            rsList.AddRounded(rs);

            decimal rsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.AddRounded(rsi);
        }

        var rsiSignalList = GetMovingAverageList(stockData, movingAvgType, signalLength, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal prevRsi = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
            decimal rsiSignal = rsiSignalList.ElementAtOrDefault(i);

            decimal prevRsiHistogram = rsiHistogramList.LastOrDefault();
            decimal rsiHistogram = rsi - rsiSignal;
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
    /// <param name="streakLength">Length of the streak.</param>
    /// <param name="rsiLength">Length of the rsi.</param>
    /// <param name="rocLength">Length of the roc.</param>
    /// <returns></returns>
    public static StockData CalculateConnorsRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int streakLength = 2, int rsiLength = 3, int rocLength = 100)
    {
        List<decimal> streakList = new();
        List<decimal> tempList = new();
        List<decimal> pctRankList = new();
        List<decimal> connorsRsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, rsiLength, rsiLength).CustomValuesList;
        var rocList = CalculateRateOfChange(stockData, rocLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal roc = rocList.ElementAtOrDefault(i);
            tempList.Add(roc);

            var lookBackList = tempList.TakeLastExt(rocLength).Take(rocLength - 1).ToList();
            int count = lookBackList.Where(x => x <= roc).Count();
            decimal pctRank = MinOrMax((decimal)count / rocLength * 100, 100, 0);
            pctRankList.Add(pctRank);

            decimal prevStreak = streakList.LastOrDefault();
            decimal streak = currentValue > prevValue ? prevStreak >= 0 ? prevStreak + 1 : 1 : currentValue < prevValue ? prevStreak <= 0 ?
                prevStreak - 1 : -1 : 0;
            streakList.Add(streak);
        }

        stockData.CustomValuesList = streakList;
        var rsiStreakList = CalculateRelativeStrengthIndex(stockData, maType, streakLength, streakLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentRsi = rsiList.ElementAtOrDefault(i);
            decimal percentRank = pctRankList.ElementAtOrDefault(i);
            decimal streakRsi = rsiStreakList.ElementAtOrDefault(i);
            decimal prevConnorsRsi1 = i >= 1 ? connorsRsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevConnorsRsi2 = i >= 2 ? connorsRsiList.ElementAtOrDefault(i - 2) : 0;

            decimal connorsRsi = MinOrMax((currentRsi + percentRank + streakRsi) / 3, 100, 0);
            connorsRsiList.Add(connorsRsi);

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
        List<decimal> rocList = new();
        List<decimal> upSumList = new();
        List<decimal> downSumList = new();
        List<decimal> arsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevArsi1 = i >= 1 ? arsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevArsi2 = i >= 2 ? arsiList.ElementAtOrDefault(i - 2) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal roc = prevValue != 0 ? (currentValue - prevValue) / prevValue * 100 : 0;
            rocList.Add(roc);

            decimal upCount = rocList.TakeLastExt(length).Where(x => x >= 0).Count();
            decimal upAlpha = upCount != 0 ? 1 / upCount : 0;
            decimal posRoc = roc > 0 ? roc : 0;
            decimal negRoc = roc < 0 ? Math.Abs(roc) : 0;

            decimal prevUpSum = upSumList.LastOrDefault();
            decimal upSum = (upAlpha * posRoc) + ((1 - upAlpha) * prevUpSum);
            upSumList.Add(upSum);

            decimal downCount = length - upCount;
            decimal downAlpha = downCount != 0 ? 1 / downCount : 0;

            decimal prevDownSum = downSumList.LastOrDefault();
            decimal downSum = (downAlpha * negRoc) + ((1 - downAlpha) * prevDownSum);
            downSumList.Add(downSum);

            decimal ars = downSum != 0 ? upSum / downSum : 0;
            decimal arsi = downSum == 0 ? 100 : upSum == 0 ? 0 : MinOrMax(100 - (100 / (1 + ars)), 100, 0);
            arsiList.Add(arsi);

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
    /// Calculates the index of the adaptive relative strength.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveRelativeStrengthIndex(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> arsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal alpha = 2 * Math.Abs((rsi / 100) - 0.5m);

            decimal prevArsi = arsiList.LastOrDefault();
            decimal arsi = (alpha * currentValue) + ((1 - alpha) * prevArsi);
            arsiList.Add(arsi);

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
        List<decimal> yList = new();
        List<decimal> eList = new();
        List<decimal> eAbsList = new();
        List<decimal> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevY = i >= 1 ? yList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevA1 = i >= 1 ? aList.ElementAtOrDefault(i - 1) : 0;
            decimal prevA2 = i >= 2 ? aList.ElementAtOrDefault(i - 2) : 0;

            decimal e = currentValue - prevY;
            eList.Add(e);

            decimal eAbs = Math.Abs(e);
            eAbsList.Add(eAbs);

            decimal eAbsSma = eAbsList.TakeLastExt(length).Average();
            decimal eSma = eList.TakeLastExt(length).Average();

            decimal a = eAbsSma != 0 ? MinOrMax(eSma / eAbsSma, 1, -1) : 0;
            aList.Add(a);

            decimal y = currentValue + (a * eAbsSma);
            yList.Add(y);

            var signal = GetRsiSignal(a - prevA1, prevA1 - prevA2, a, prevA1, 0.8m, -0.8m);
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
    /// Calculates the index of the apirine slow relative strength.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateApirineSlowRelativeStrengthIndex(this StockData stockData, MovingAvgType maType, int length = 14,
        int smoothLength = 6)
    {
        List<decimal> r2List = new();
        List<decimal> r3List = new();
        List<decimal> rrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal r1 = emaList.ElementAtOrDefault(i);

            decimal r2 = currentValue > r1 ? currentValue - r1 : 0;
            r2List.Add(r2);

            decimal r3 = currentValue < r1 ? r1 - currentValue : 0;
            r3List.Add(r3);
        }

        var r4List = GetMovingAverageList(stockData, maType, length, r2List);
        var r5List = GetMovingAverageList(stockData, maType, length, r3List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal r4 = r4List.ElementAtOrDefault(i);
            decimal r5 = r5List.ElementAtOrDefault(i);
            decimal prevRr1 = i >= 1 ? rrList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRr2 = i >= 2 ? rrList.ElementAtOrDefault(i - 2) : 0;
            decimal rs = r5 != 0 ? r4 / r5 : 0;

            decimal rr = r5 == 0 ? 100 : r4 == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rrList.Add(rr);

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
        List<decimal> brsiList = new();
        List<decimal> posPowerList = new();
        List<decimal> boPowerList = new();
        List<decimal> negPowerList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal prevBrsi1 = i >= 1 ? brsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBrsi2 = i >= 2 ? brsiList.ElementAtOrDefault(i - 2) : 0;

            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            tempList.Add(currentVolume);

            decimal boVolume = tempList.TakeLastExt(lbLength).Sum();
            decimal boStrength = currentHigh - currentLow != 0 ? (currentClose - currentOpen) / (currentHigh - currentLow) : 0;

            decimal prevBoPower = boPowerList.LastOrDefault();
            decimal boPower = currentValue * boStrength * boVolume;
            boPowerList.Add(boPower);

            decimal posPower = boPower > prevBoPower ? Math.Abs(boPower) : 0;
            posPowerList.Add(posPower);

            decimal negPower = boPower < prevBoPower ? Math.Abs(boPower) : 0;
            negPowerList.Add(negPower);

            decimal posPowerSum = posPowerList.TakeLastExt(length).Sum();
            decimal negPowerSum = negPowerList.TakeLastExt(length).Sum();
            decimal boRatio = negPowerSum != 0 ? posPowerSum / negPowerSum : 0;

            decimal brsi = negPowerSum == 0 ? 100 : posPowerSum == 0 ? 0 : MinOrMax(100 - (100 / (1 + boRatio)), 100, 0);
            brsiList.Add(brsi);

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
        List<decimal> numEmaList = new();
        List<decimal> denEmaList = new();
        List<decimal> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        decimal k = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevVolume = i >= 1 ? volumeList.ElementAtOrDefault(i - 1) : 0;
            decimal a = currentValue - prevValue;
            decimal b = currentVolume - prevVolume;
            decimal prevC1 = i >= 1 ? cList.ElementAtOrDefault(i - 1) : 0;
            decimal prevC2 = i >= 2 ? cList.ElementAtOrDefault(i - 2) : 0;
            decimal num = Math.Max(a, 0) * Math.Max(b, 0);
            decimal den = Math.Abs(a) * Math.Abs(b);

            decimal prevNumEma = numEmaList.LastOrDefault();
            decimal numEma = (num * k) + (prevNumEma * (1 - k));
            numEmaList.Add(numEma);

            decimal prevDenEma = denEmaList.LastOrDefault();
            decimal denEma = (den * k) + (prevDenEma * (1 - k));
            denEmaList.Add(denEma);

            decimal c = denEma != 0 ? MinOrMax(100 * numEma / denEma, 100, 0) : 0;
            cList.Add(c);

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
        List<decimal> absRsiList = new();
        List<decimal> frsiList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);

            decimal absRsi = 2 * Math.Abs(rsi - 50);
            absRsiList.Add(absRsi);

            decimal frsi = absRsiList.TakeLastExt(length).Sum();
            frsiList.Add(frsi);
        }

        var frsiMaList = GetMovingAverageList(stockData, maType, length, frsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal frsi = frsiList.ElementAtOrDefault(i);
            decimal frsiMa = frsiMaList.ElementAtOrDefault(i);
            decimal prevFrsi = i >= 1 ? frsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFrsiMa = i >= 1 ? frsiMaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> maxList = new();
        List<decimal> minList = new();
        List<decimal> rsiScaledList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal volume = volumeList.ElementAtOrDefault(i);

            decimal max = Math.Max((currentValue - prevValue) * volume, 0);
            maxList.Add(max);

            decimal min = -Math.Min((currentValue - prevValue) * volume, 0);
            minList.Add(min);
        }

        var upList = GetMovingAverageList(stockData, maType, length, maxList);
        var dnList = GetMovingAverageList(stockData, maType, length, minList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal up = upList.ElementAtOrDefault(i);
            decimal dn = dnList.ElementAtOrDefault(i);
            decimal rsiRaw = dn == 0 ? 100 : up == 0 ? 0 : 100 - (100 / (1 + (up / dn)));

            decimal rsiScale = (rsiRaw * 2) - 100;
            rsiScaledList.Add(rsiScale);
        }

        var rsiList = GetMovingAverageList(stockData, maType, smoothLength, rsiScaledList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal prevRsi1 = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRsi2 = i >= 2 ? rsiList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> upChgList = new();
        List<decimal> downChgList = new();
        List<decimal> rapidRsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal chg = currentValue - prevValue;

            decimal upChg = chg > 0 ? chg : 0;
            upChgList.Add(upChg);

            decimal downChg = chg < 0 ? Math.Abs(chg) : 0;
            downChgList.Add(downChg);

            decimal upChgSum = upChgList.TakeLastExt(length).Sum();
            decimal downChgSum = downChgList.TakeLastExt(length).Sum();
            decimal rs = downChgSum != 0 ? upChgSum / downChgSum : 0;

            decimal rapidRsi = downChgSum == 0 ? 100 : upChgSum == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rapidRsiList.Add(rapidRsi);
        }

        var rrsiEmaList = GetMovingAverageList(stockData, maType, length, rapidRsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rapidRsi = rrsiEmaList.ElementAtOrDefault(i);
            decimal prevRapidRsi1 = i >= 1 ? rrsiEmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRapidRsi2 = i >= 2 ? rrsiEmaList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> chgList = new();
        List<decimal> bList = new();
        List<decimal> avgRsiList = new();
        List<decimal> avgList = new();
        List<decimal> gainList = new();
        List<decimal> lossList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal chg = currentValue - prevValue;
            chgList.Add(chg);
        }

        var srcList = GetMovingAverageList(stockData, maType, length, chgList);
        stockData.CustomValuesList = srcList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, length: length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal src = srcList.ElementAtOrDefault(i);
            decimal prevB1 = i >= 1 ? bList.ElementAtOrDefault(i - 1) : 0;
            decimal prevB2 = i >= 2 ? bList.ElementAtOrDefault(i - 2) : 0;

            decimal b = 0, avg = 0, gain = 0, loss = 0, avgRsi = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevB = i >= j ? bList.ElementAtOrDefault(i - j) : src;
                decimal prevAvg = i >= j ? avgList.ElementAtOrDefault(i - j) : 0;
                decimal prevGain = i >= j ? gainList.ElementAtOrDefault(i - j) : 0;
                decimal prevLoss = i >= j ? lossList.ElementAtOrDefault(i - j) : 0;
                decimal k = (decimal)j / length;
                decimal a = rsi * (decimal)length / j;
                avg = (a + prevB) / 2;
                decimal avgChg = avg - prevAvg;
                gain = avgChg > 0 ? avgChg : 0;
                loss = avgChg < 0 ? Math.Abs(avgChg) : 0;
                decimal avgGain = (gain * k) + (prevGain * (1 - k));
                decimal avgLoss = (loss * k) + (prevLoss * (1 - k));
                decimal rs = avgLoss != 0 ? avgGain / avgLoss : 0;
                avgRsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 1, 0);
                b = avgRsiList.Count >= length ? avgRsiList.TakeLastExt(length).Average() : avgRsi;
            }
            bList.Add(b);
            avgList.Add(avg);
            gainList.Add(gain);
            lossList.Add(loss);
            avgRsiList.Add(avgRsi);

            var signal = GetRsiSignal(b - prevB1, prevB1 - prevB2, b, prevB1, 0.8m, 0.2m);
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
        List<decimal> rsiList = new();
        List<decimal> srcLcList = new();
        List<decimal> hcSrcList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal hc = highestList.ElementAtOrDefault(i);
            decimal lc = lowestList.ElementAtOrDefault(i);

            decimal srcLc = currentValue - lc;
            srcLcList.Add(srcLc);

            decimal hcSrc = hc - currentValue;
            hcSrcList.Add(hcSrc);
        }

        var topList = GetMovingAverageList(stockData, maType, length2, srcLcList);
        var botList = GetMovingAverageList(stockData, maType, length2, hcSrcList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal top = topList.ElementAtOrDefault(i);
            decimal bot = botList.ElementAtOrDefault(i);
            decimal rs = bot != 0 ? MinOrMax(top / bot, 1, 0) : 0;

            decimal rsi = bot == 0 ? 100 : top == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.Add(rsi);
        }

        var rsiEmaList = GetMovingAverageList(stockData, maType, length2, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal rsiEma = rsiEmaList.ElementAtOrDefault(i);
            decimal prevRsi = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRsiEma = i >= 1 ? rsiEmaList.ElementAtOrDefault(i - 1) : 0;

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
}