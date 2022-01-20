namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the index of the commodity channel.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateCommodityChannelIndex(this StockData stockData)
    {
        int length = 20;
        var maType = MovingAvgType.SimpleMovingAverage;
        var inputType = InputName.TypicalPrice;
        var constant = 0.015m;

        return CalculateCommodityChannelIndex(stockData, inputType, maType, length, constant);
    }

    /// <summary>
    /// Calculates the index of the commodity channel.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <returns></returns>
    public static StockData CalculateCommodityChannelIndex(this StockData stockData, MovingAvgType maType, int length)
    {
        var inputType = InputName.TypicalPrice;
        var constant = 0.015m;

        return CalculateCommodityChannelIndex(stockData, inputType, maType, length, constant);
    }

    /// <summary>
    /// Calculates the index of the commodity channel.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="constant">The constant.</param>
    /// <returns></returns>
    public static StockData CalculateCommodityChannelIndex(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20, decimal constant = 0.015m)
    {
        List<decimal> cciList = new();
        List<decimal> tpDevDiffList = new();
        List<Signal> signalsList = new();

        var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);
        var tpSmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal tpSma = tpSmaList.ElementAtOrDefault(i);

            decimal tpDevDiff = Math.Abs(currentValue - tpSma);
            tpDevDiffList.Add(tpDevDiff);
        }

        var tpMeanDevList = GetMovingAverageList(stockData, maType, length, tpDevDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevCci1 = i >= 1 ? cciList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCci2 = i >= 2 ? cciList.ElementAtOrDefault(i - 2) : 0;
            decimal tpMeanDev = tpMeanDevList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal tpSma = tpSmaList.ElementAtOrDefault(i);

            decimal cci = tpMeanDev != 0 ? (currentValue - tpSma) / (constant * tpMeanDev) : 0;
            cciList.Add(cci);

            var signal = GetRsiSignal(cci - prevCci1, prevCci1 - prevCci2, cci, prevCci1, 100, -100);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cci", cciList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cciList;
        stockData.IndicatorName = IndicatorName.CommodityChannelIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the awesome oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateAwesomeOscillator(this StockData stockData)
    {
        int fastLength = 5, slowLength = 34;
        var maType = MovingAvgType.SimpleMovingAverage;
        var inputName = InputName.MedianPrice;

        return CalculateAwesomeOscillator(stockData, maType, inputName, fastLength, slowLength);
    }

    /// <summary>
    /// Calculates the awesome oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateAwesomeOscillator(this StockData stockData, MovingAvgType maType, InputName inputName,
        int fastLength, int slowLength)
    {
        List<decimal> aoList = new();
        List<Signal> signalsList = new();

        var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);
        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastSma = fastSmaList.ElementAtOrDefault(i);
            decimal slowSma = slowSmaList.ElementAtOrDefault(i);

            decimal prevAo = aoList.LastOrDefault();
            decimal ao = fastSma - slowSma;
            aoList.Add(ao);

            var signal = GetCompareSignal(ao, prevAo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ao", aoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aoList;
        stockData.IndicatorName = IndicatorName.AwesomeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the accelerator oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateAcceleratorOscillator(this StockData stockData)
    {
        int fastLength = 5, slowLength = 34, smoothLength = 5;
        var maType = MovingAvgType.SimpleMovingAverage;
        var inputName = InputName.MedianPrice;

        return CalculateAcceleratorOscillator(stockData, maType, inputName, fastLength, slowLength, smoothLength);
    }

    /// <summary>
    /// Calculates the accelerator oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAcceleratorOscillator(this StockData stockData, MovingAvgType maType, InputName inputName,
        int fastLength, int slowLength, int smoothLength)
    {
        List<decimal> acList = new();
        List<Signal> signalsList = new();

        var awesomeOscList = CalculateAwesomeOscillator(stockData, maType, inputName, fastLength, slowLength).CustomValuesList;
        var awesomeOscMaList = GetMovingAverageList(stockData, maType, smoothLength, awesomeOscList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ao = awesomeOscList.ElementAtOrDefault(i);
            decimal aoSma = awesomeOscMaList.ElementAtOrDefault(i);

            decimal prevAc = acList.LastOrDefault();
            decimal ac = ao - aoSma;
            acList.Add(ac);

            var signal = GetCompareSignal(ac, prevAc);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ac", acList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = acList;
        stockData.IndicatorName = IndicatorName.AcceleratorOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the choppiness index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateChoppinessIndex(this StockData stockData)
    {
        var maType = MovingAvgType.ExponentialMovingAverage;
        var length = 14;

        return CalculateChoppinessIndex(stockData, maType, length);
    }

    /// <summary>
    /// Calculates the choppiness index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateChoppinessIndex(this StockData stockData, MovingAvgType maType, int length)
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
    /// Calculates the ulcer index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateUlcerIndex(this StockData stockData)
    {
        int length = 14;

        return CalculateUlcerIndex(stockData, length);
    }

    /// <summary>
    /// Calculates the ulcer index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateUlcerIndex(this StockData stockData, int length)
    {
        List<decimal> ulcerIndexList = new();
        List<decimal> pctDrawdownSquaredList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, _) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal maxValue = highestList.ElementAtOrDefault(i);
            decimal prevUlcerIndex1 = i >= 1 ? ulcerIndexList.ElementAtOrDefault(i - 1) : 0;
            decimal prevUlcerIndex2 = i >= 2 ? ulcerIndexList.ElementAtOrDefault(i - 2) : 0;

            decimal pctDrawdownSquared = maxValue != 0 ? Pow((currentValue - maxValue) / maxValue * 100, 2) : 0;
            pctDrawdownSquaredList.Add(pctDrawdownSquared);

            decimal squaredAvg = pctDrawdownSquaredList.TakeLastExt(length).Average();

            decimal ulcerIndex = squaredAvg >= 0 ? Sqrt(squaredAvg) : 0;
            ulcerIndexList.Add(ulcerIndex);

            var signal = GetCompareSignal(ulcerIndex - prevUlcerIndex1, prevUlcerIndex1 - prevUlcerIndex2, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ui", ulcerIndexList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ulcerIndexList;
        stockData.IndicatorName = IndicatorName.UlcerIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the force.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateForceIndex(this StockData stockData)
    {
        var maType = MovingAvgType.ExponentialMovingAverage;
        var length = 14;

        return CalculateForceIndex(stockData, maType, length);
    }

    /// <summary>
    /// Calculates the index of the force.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateForceIndex(this StockData stockData, MovingAvgType maType, int length)
    {
        List<decimal> rawForceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal rawForce = (currentValue - prevValue) * currentVolume;
            rawForceList.Add(rawForce);
        }

        var forceList = GetMovingAverageList(stockData, maType, length, rawForceList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal force = forceList.ElementAtOrDefault(i);
            decimal prevForce1 = i >= 1 ? forceList.ElementAtOrDefault(i - 1) : 0;
            decimal prevForce2 = i >= 2 ? forceList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(force - prevForce1, prevForce1 - prevForce2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fi", forceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = forceList;
        stockData.IndicatorName = IndicatorName.ForceIndex;

        return stockData;
    }

    public static StockData CalculateMoneyFlowIndex(this StockData stockData)
    {
        var inputName = InputName.TypicalPrice;
        var length = 14;

        return CalculateMoneyFlowIndex(stockData, inputName, length);
    }

    /// <summary>
    /// Calculates the index of the money flow.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateMoneyFlowIndex(this StockData stockData, InputName inputName = InputName.TypicalPrice, int length = 14)
    {
        List<decimal> mfiList = new();
        List<decimal> posMoneyFlowList = new();
        List<decimal> negMoneyFlowList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal typicalPrice = inputList.ElementAtOrDefault(i);
            decimal prevTypicalPrice = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMfi1 = i >= 1 ? mfiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMfi2 = i >= 2 ? mfiList.ElementAtOrDefault(i - 2) : 0;
            decimal rawMoneyFlow = typicalPrice * currentVolume;

            decimal posMoneyFlow = typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
            posMoneyFlowList.Add(posMoneyFlow);

            decimal negMoneyFlow = typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
            negMoneyFlowList.Add(negMoneyFlow);

            decimal posMoneyFlowTotal = posMoneyFlowList.TakeLastExt(length).Sum();
            decimal negMoneyFlowTotal = negMoneyFlowList.TakeLastExt(length).Sum();
            decimal mfiRatio = negMoneyFlowTotal != 0 ? MinOrMax(posMoneyFlowTotal / negMoneyFlowTotal, 1, 0) : 0;

            decimal mfi = negMoneyFlowTotal == 0 ? 100 : posMoneyFlowTotal == 0 ? 0 : MinOrMax(100 - (100 / (1 + mfiRatio)), 100, 0);
            mfiList.Add(mfi);

            var signal = GetRsiSignal(mfi - prevMfi1, prevMfi1 - prevMfi2, mfi, prevMfi1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mfi", mfiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mfiList;
        stockData.IndicatorName = IndicatorName.MoneyFlowIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the klinger volume oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateKlingerVolumeOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 34, int slowLength = 55,
        int signalLength = 13)
    {
        List<decimal> kvoList = new();
        List<decimal> trendList = new();
        List<decimal> dmList = new();
        List<decimal> cmList = new();
        List<decimal> vfList = new();
        List<decimal> kvoHistoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal mom = currentValue - prevValue;

            decimal prevTrend = trendList.LastOrDefault();
            decimal trend = mom > 0 ? 1 : mom < 0 ? -1 : prevTrend;
            trendList.Add(trend);

            decimal prevDm = dmList.LastOrDefault();
            decimal dm = currentHigh - currentLow;
            dmList.Add(dm);

            decimal prevCm = cmList.LastOrDefault();
            decimal cm = trend == prevTrend ? prevCm + dm : prevDm + dm;
            cmList.Add(cm);

            decimal temp = cm != 0 ? Math.Abs((2 * (dm / cm)) - 1) : -1;
            decimal vf = currentVolume * temp * trend * 100;
            vfList.Add(vf);
        }

        var ema34List = GetMovingAverageList(stockData, maType, fastLength, vfList);
        var ema55List = GetMovingAverageList(stockData, maType, slowLength, vfList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema34 = ema34List.ElementAtOrDefault(i);
            decimal ema55 = ema55List.ElementAtOrDefault(i);

            decimal klingerOscillator = ema34 - ema55;
            kvoList.Add(klingerOscillator);
        }

        var kvoSignalList = GetMovingAverageList(stockData, maType, signalLength, kvoList);
        for (int k = 0; k < stockData.Count; k++)
        {
            decimal klingerOscillator = kvoList.ElementAtOrDefault(k);
            decimal koSignalLine = kvoSignalList.ElementAtOrDefault(k);

            decimal prevKlingerOscillatorHistogram = kvoHistoList.LastOrDefault();
            decimal klingerOscillatorHistogram = klingerOscillator - koSignalLine;
            kvoHistoList.Add(klingerOscillatorHistogram);

            var signal = GetCompareSignal(klingerOscillatorHistogram, prevKlingerOscillatorHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kvo", kvoList },
            { "KvoSignal", kvoSignalList },
            { "KvoHistogram", kvoHistoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kvoList;
        stockData.IndicatorName = IndicatorName.KlingerVolumeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the on balance volume.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateOnBalanceVolume(this StockData stockData, MovingAvgType maType, int length = 20)
    {
        List<decimal> obvList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevObv = obvList.LastOrDefault();
            decimal obv = currentValue > prevValue ? prevObv + currentVolume : currentValue < prevValue ? prevObv - currentVolume : prevObv;
            obvList.Add(obv);
        }

        var obvSignalList = GetMovingAverageList(stockData, maType, length, obvList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal obv = obvList.ElementAtOrDefault(i);
            decimal prevObv = i >= 1 ? obvList.ElementAtOrDefault(i - 1) : 0;
            decimal obvSig = obvSignalList.ElementAtOrDefault(i);
            decimal prevObvSig = i >= 1 ? obvSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(obv - obvSig, prevObv - prevObvSig);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Obv", obvList },
            { "ObvSignal", obvSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = obvList;
        stockData.IndicatorName = IndicatorName.OnBalanceVolume;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the negative volume.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateNegativeVolumeIndex(this StockData stockData, MovingAvgType maType, int length = 255)
    {
        List<decimal> nviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVolume = i >= 1 ? volumeList.ElementAtOrDefault(i - 1) : 0;
            decimal pctChg = CalculatePercentChange(currentClose, prevClose);

            decimal prevNvi = nviList.LastOrDefault();
            decimal nvi = currentVolume >= prevVolume ? prevNvi : prevNvi + (prevNvi * pctChg);
            nviList.Add(nvi);
        }

        var nviSignalList = GetMovingAverageList(stockData, maType, length, nviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nvi = nviList.ElementAtOrDefault(i);
            decimal prevNvi = i >= 1 ? nviList.ElementAtOrDefault(i - 1) : 0;
            decimal nviSignal = nviSignalList.ElementAtOrDefault(i);
            decimal prevNviSignal = i >= 1 ? nviSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(nvi - nviSignal, prevNvi - prevNviSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nvi", nviList },
            { "NviSignal", nviSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nviList;
        stockData.IndicatorName = IndicatorName.NegativeVolumeIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the positive volume.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculatePositiveVolumeIndex(this StockData stockData, MovingAvgType maType, int length = 255)
    {
        List<decimal> pviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVolume = i >= 1 ? volumeList.ElementAtOrDefault(i - 1) : 0;
            decimal pctChg = CalculatePercentChange(currentClose, prevClose);

            decimal prevPvi = pviList.LastOrDefault();
            decimal pvi = currentVolume <= prevVolume ? prevPvi : prevPvi + (prevPvi * pctChg);
            pviList.Add(pvi);
        }

        var pviSignalList = GetMovingAverageList(stockData, maType, length, pviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pvi = pviList.ElementAtOrDefault(i);
            decimal prevPvi = i >= 1 ? pviList.ElementAtOrDefault(i - 1) : 0;
            decimal pviSignal = pviSignalList.ElementAtOrDefault(i);
            decimal prevPviSignal = i >= 1 ? pviSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(pvi - pviSignal, prevPvi - prevPviSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pvi", pviList },
            { "PviSignal", pviSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pviList;
        stockData.IndicatorName = IndicatorName.PositiveVolumeIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the balance of power.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateBalanceOfPower(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> balanceOfPowerList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal balanceOfPower = currentHigh - currentLow != 0 ? (currentClose - currentOpen) / (currentHigh - currentLow) : 0;
            balanceOfPowerList.Add(balanceOfPower);
        }

        var bopSignalList = GetMovingAverageList(stockData, maType, length, balanceOfPowerList);
        for (int i = 0; i < stockData.ClosePrices.Count; i++)
        {
            decimal bop = balanceOfPowerList.ElementAtOrDefault(i);
            decimal bopMa = bopSignalList.ElementAtOrDefault(i);
            decimal prevBop = i >= 1 ? balanceOfPowerList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBopMa = i >= 1 ? bopSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(bop - bopMa, prevBop - prevBopMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Bop", balanceOfPowerList },
            { "BopSignal", bopSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = balanceOfPowerList;
        stockData.IndicatorName = IndicatorName.BalanceOfPower;

        return stockData;
    }

    /// <summary>
    /// Calculates the rate of change.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateRateOfChange(this StockData stockData, int length = 12)
    {
        List<decimal> rocList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal prevRoc1 = i >= 1 ? rocList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRoc2 = i >= 2 ? rocList.ElementAtOrDefault(i - 2) : 0;

            decimal roc = prevValue != 0 ? (currentValue - prevValue) / prevValue * 100 : 0;
            rocList.Add(roc);

            var signal = GetCompareSignal(roc - prevRoc1, prevRoc1 - prevRoc2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Roc", rocList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rocList;
        stockData.IndicatorName = IndicatorName.RateOfChange;

        return stockData;
    }

    /// <summary>
    /// Calculates the percentage price oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculatePercentagePriceOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 12,
        int slowLength = 26, int signalLength = 9)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal ppo = slowEma != 0 ? 100 * (fastEma - slowEma) / slowEma : 0;
            ppoList.Add(ppo);
        }

        var ppoSignalList = GetMovingAverageList(stockData, maType, signalLength, ppoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo = ppoList.ElementAtOrDefault(i);
            decimal ppoSignalLine = ppoSignalList.ElementAtOrDefault(i);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.PercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the percentage volume oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculatePercentageVolumeOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 12,
        int slowLength = 26, int signalLength = 9)
    {
        List<decimal> pvoList = new();
        List<decimal> pvoHistogramList = new();
        List<Signal> signalsList = new();
        var (_, _, _, _, volumeList) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, volumeList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal pvo = slowEma != 0 ? 100 * (fastEma - slowEma) / slowEma : 0;
            pvoList.Add(pvo);
        }

        var pvoSignalList = GetMovingAverageList(stockData, maType, signalLength, pvoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pvo = pvoList.ElementAtOrDefault(i);
            decimal pvoSignalLine = pvoSignalList.ElementAtOrDefault(i);

            decimal prevPvoHistogram = pvoHistogramList.LastOrDefault();
            decimal pvoHistogram = pvo - pvoSignalLine;
            pvoHistogramList.Add(pvoHistogram);

            var signal = GetCompareSignal(pvoHistogram, prevPvoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pvo", pvoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pvoList;
        stockData.IndicatorName = IndicatorName.PercentageVolumeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the chaikin money flow.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateChaikinMoneyFlow(this StockData stockData, int length = 20)
    {
        List<decimal> chaikinMoneyFlowList = new();
        List<decimal> tempVolumeList = new();
        List<decimal> moneyFlowVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
            decimal prevCmf1 = i >= 1 ? chaikinMoneyFlowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCmf2 = i >= 2 ? chaikinMoneyFlowList.ElementAtOrDefault(i - 2) : 0;

            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            tempVolumeList.AddRounded(currentVolume);

            decimal moneyFlowVolume = moneyFlowMultiplier * currentVolume;
            moneyFlowVolumeList.Add(moneyFlowVolume);

            decimal volumeSum = tempVolumeList.TakeLastExt(length).Sum();
            decimal mfVolumeSum = moneyFlowVolumeList.TakeLastExt(length).Sum();

            decimal cmf = volumeSum != 0 ? mfVolumeSum / volumeSum : 0;
            chaikinMoneyFlowList.AddRounded(cmf);

            var signal = GetCompareSignal(cmf - prevCmf1, prevCmf1 - prevCmf2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmf", chaikinMoneyFlowList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = chaikinMoneyFlowList;
        stockData.IndicatorName = IndicatorName.ChaikinMoneyFlow;

        return stockData;
    }

    /// <summary>
    /// Calculates the accumulation distribution line.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAccumulationDistributionLine(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> adlList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
            decimal moneyFlowVolume = moneyFlowMultiplier * currentVolume;

            decimal prevAdl = adlList.LastOrDefault();
            decimal adl = prevAdl + moneyFlowVolume;
            adlList.Add(adl);
        }

        var adlSignalList = GetMovingAverageList(stockData, maType, length, adlList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var adl = adlList.ElementAtOrDefault(i);
            var prevAdl = i >= 1 ? adlList.ElementAtOrDefault(i - 1) : 0;
            var adlSignal = adlSignalList.ElementAtOrDefault(i);
            var prevAdlSignal = i >= 1 ? adlSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(adl - adlSignal, prevAdl - prevAdlSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Adl", adlList },
            { "AdlSignal", adlSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = adlList;
        stockData.IndicatorName = IndicatorName.AccumulationDistributionLine;

        return stockData;
    }

    /// <summary>
    /// Calculates the chaikin oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateChaikinOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 3, int slowLength = 10)
    {
        List<decimal> chaikinOscillatorList = new();
        List<Signal> signalsList = new();

        var adlList = CalculateAccumulationDistributionLine(stockData, maType, fastLength).CustomValuesList;
        var adl3EmaList = GetMovingAverageList(stockData, maType, fastLength, adlList);
        var adl10EmaList = GetMovingAverageList(stockData, maType, slowLength, adlList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal adl3Ema = adl3EmaList.ElementAtOrDefault(i);
            decimal adl10Ema = adl10EmaList.ElementAtOrDefault(i);

            decimal prevChaikinOscillator = chaikinOscillatorList.LastOrDefault();
            decimal chaikinOscillator = adl3Ema - adl10Ema;
            chaikinOscillatorList.Add(chaikinOscillator);

            var signal = GetCompareSignal(chaikinOscillator, prevChaikinOscillator);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "ChaikinOsc", chaikinOscillatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = chaikinOscillatorList;
        stockData.IndicatorName = IndicatorName.ChaikinOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the ichimoku cloud.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="tenkanLength">Length of the tenkan.</param>
    /// <param name="kiiunLength">Length of the kiiun.</param>
    /// <param name="senkouLength">Length of the senkou.</param>
    /// <returns></returns>
    public static StockData CalculateIchimokuCloud(this StockData stockData, int tenkanLength = 9, int kiiunLength = 26, int senkouLength = 52)
    {
        List<decimal> tenkanSenList = new();
        List<decimal> kiiunSenList = new();
        List<decimal> senkouSpanAList = new();
        List<decimal> senkouSpanBList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        var (tenkanHighList, tenkanLowList) = GetMaxAndMinValuesList(highList, lowList, tenkanLength);
        var (kiiunHighList, kiiunLowList) = GetMaxAndMinValuesList(highList, lowList, kiiunLength);
        var (senkouHighList, senkouLowList) = GetMaxAndMinValuesList(highList, lowList, senkouLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest1 = tenkanHighList.ElementAtOrDefault(i);
            decimal lowest1 = tenkanLowList.ElementAtOrDefault(i);
            decimal highest2 = kiiunHighList.ElementAtOrDefault(i);
            decimal lowest2 = kiiunLowList.ElementAtOrDefault(i);
            decimal highest3 = senkouHighList.ElementAtOrDefault(i);
            decimal lowest3 = senkouLowList.ElementAtOrDefault(i);

            decimal prevTenkanSen = tenkanSenList.LastOrDefault();
            decimal tenkanSen = (highest1 + lowest1) / 2;
            tenkanSenList.Add(tenkanSen);

            decimal prevKiiunSen = kiiunSenList.LastOrDefault();
            decimal kiiunSen = (highest2 + lowest2) / 2;
            kiiunSenList.Add(kiiunSen);

            decimal senkouSpanA = (tenkanSen + kiiunSen) / 2;
            senkouSpanAList.Add(senkouSpanA);

            decimal senkouSpanB = (highest3 + lowest3) / 2;
            senkouSpanBList.Add(senkouSpanB);

            var signal = GetCompareSignal(tenkanSen - kiiunSen, prevTenkanSen - prevKiiunSen);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "TenkanSen", tenkanSenList },
            { "KiiunSen", kiiunSenList },
            { "SenkouSpanA", senkouSpanAList },
            { "SenkouSpanB", senkouSpanBList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.IchimokuCloud;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the alligator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="iawLength">Length of the iaw.</param>
    /// <param name="iawOffset">The iaw offset.</param>
    /// <param name="teethLength">Length of the teeth.</param>
    /// <param name="teethOffset">The teeth offset.</param>
    /// <param name="lipsLength">Length of the lips.</param>
    /// <param name="lipsOffset">The lips offset.</param>
    /// <returns></returns>
    public static StockData CalculateAlligatorIndex(this StockData stockData, InputName inputName, MovingAvgType maType, int iawLength = 13,
        int iawOffset = 8, int teethLength = 8, int teethOffset = 5, int lipsLength = 5, int lipsOffset = 3)
    {
        List<decimal> displacedJawList = new();
        List<decimal> displacedTeethList = new();
        List<decimal> displacedLipsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var iawList = GetMovingAverageList(stockData, maType, iawLength, inputList);
        var teethList = GetMovingAverageList(stockData, maType, teethLength, inputList);
        var lipsList = GetMovingAverageList(stockData, maType, lipsLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevJaw = displacedJawList.LastOrDefault();
            decimal displacedJaw = i >= iawOffset ? iawList.ElementAtOrDefault(i - iawOffset) : 0;
            displacedJawList.Add(displacedJaw);

            decimal prevTeeth = displacedTeethList.LastOrDefault();
            decimal displacedTeeth = i >= teethOffset ? teethList.ElementAtOrDefault(i - teethOffset) : 0;
            displacedTeethList.Add(displacedTeeth);

            decimal prevLips = displacedLipsList.LastOrDefault();
            decimal displacedLips = i >= lipsOffset ? lipsList.ElementAtOrDefault(i - lipsOffset) : 0;
            displacedLipsList.Add(displacedLips);

            var signal = GetBullishBearishSignal(displacedLips - Math.Max(displacedJaw, displacedTeeth), prevLips - Math.Max(prevJaw, prevTeeth),
                displacedLips - Math.Min(displacedJaw, displacedTeeth), prevLips - Math.Min(prevJaw, prevTeeth));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lips", displacedLipsList },
            { "Teeth", displacedTeethList },
            { "Jaws", displacedJawList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.AlligatorIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the gator oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="iawLength">Length of the iaw.</param>
    /// <param name="iawOffset">The iaw offset.</param>
    /// <param name="teethLength">Length of the teeth.</param>
    /// <param name="teethOffset">The teeth offset.</param>
    /// <param name="lipsLength">Length of the lips.</param>
    /// <param name="lipsOffset">The lips offset.</param>
    /// <returns></returns>
    public static StockData CalculateGatorOscillator(this StockData stockData, InputName inputName, MovingAvgType maType, int iawLength = 13,
        int iawOffset = 8, int teethLength = 8, int teethOffset = 5, int lipsLength = 5, int lipsOffset = 3)
    {
        List<decimal> topList = new();
        List<decimal> bottomList = new();
        List<Signal> signalsList = new();

        var alligatorList = CalculateAlligatorIndex(stockData, inputName, maType, iawLength, iawOffset, teethLength, teethOffset, lipsLength,
            lipsOffset).OutputValues;
        var iawList = alligatorList["Jaw"];
        var teethList = alligatorList["Teeth"];
        var lipsList = alligatorList["Lips"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal iaw = iawList.ElementAtOrDefault(i);
            decimal teeth = teethList.ElementAtOrDefault(i);
            decimal lips = lipsList.ElementAtOrDefault(i);

            decimal prevTop = topList.LastOrDefault();
            decimal top = Math.Abs(iaw - teeth);
            topList.AddRounded(top);

            decimal prevBottom = bottomList.LastOrDefault();
            decimal bottom = -Math.Abs(teeth - lips);
            bottomList.AddRounded(bottom);

            var signal = GetCompareSignal(top - bottom, prevTop - prevBottom);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Top", topList },
            { "Bottom", bottomList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.GatorOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the ultimate oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length1">The length1.</param>
    /// <param name="length2">The length2.</param>
    /// <param name="length3">The length3.</param>
    /// <returns></returns>
    public static StockData CalculateUltimateOscillator(this StockData stockData, int length1 = 7, int length2 = 14, int length3 = 28)
    {
        List<decimal> uoList = new();
        List<decimal> bpList = new();
        List<decimal> trList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal minValue = Math.Min(currentLow, prevClose);
            decimal maxValue = Math.Max(currentHigh, prevClose);
            decimal prevUo1 = i >= 1 ? uoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevUo2 = i >= 2 ? uoList.ElementAtOrDefault(i - 2) : 0;

            decimal buyingPressure = currentClose - minValue;
            bpList.Add(buyingPressure);

            decimal trueRange = maxValue - minValue;
            trList.Add(trueRange);

            decimal bp7Sum = bpList.TakeLastExt(length1).Sum();
            decimal bp14Sum = bpList.TakeLastExt(length2).Sum();
            decimal bp28Sum = bpList.TakeLastExt(length3).Sum();
            decimal tr7Sum = trList.TakeLastExt(length1).Sum();
            decimal tr14Sum = trList.TakeLastExt(length2).Sum();
            decimal tr28Sum = trList.TakeLastExt(length3).Sum();
            decimal avg7 = tr7Sum != 0 ? bp7Sum / tr7Sum : 0;
            decimal avg14 = tr14Sum != 0 ? bp14Sum / tr14Sum : 0;
            decimal avg28 = tr28Sum != 0 ? bp28Sum / tr28Sum : 0;

            decimal ultimateOscillator = MinOrMax(100 * (((4 * avg7) + (2 * avg14) + avg28) / (4 + 2 + 1)), 100, 0);
            uoList.Add(ultimateOscillator);

            var signal = GetRsiSignal(ultimateOscillator - prevUo1, prevUo1 - prevUo2, ultimateOscillator, prevUo1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Uo", uoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = uoList;
        stockData.IndicatorName = IndicatorName.UltimateOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the vortex indicator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateVortexIndicator(this StockData stockData, int length = 14)
    {
        List<decimal> vmPlusList = new();
        List<decimal> trueRangeList = new();
        List<decimal> vmMinusList = new();
        List<decimal> viPlus14List = new();
        List<decimal> viMinus14List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;

            decimal vmPlus = Math.Abs(currentHigh - prevLow);
            vmPlusList.Add(vmPlus);

            decimal vmMinus = Math.Abs(currentLow - prevHigh);
            vmMinusList.Add(vmMinus);

            decimal trueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trueRangeList.Add(trueRange);

            decimal vmPlus14 = vmPlusList.TakeLastExt(length).Sum();
            decimal vmMinus14 = vmMinusList.TakeLastExt(length).Sum();
            decimal trueRange14 = trueRangeList.TakeLastExt(length).Sum();

            decimal prevViPlus14 = viPlus14List.LastOrDefault();
            decimal viPlus14 = trueRange14 != 0 ? vmPlus14 / trueRange14 : 0;
            viPlus14List.Add(viPlus14);

            decimal prevViMinus14 = viMinus14List.LastOrDefault();
            decimal viMinus14 = trueRange14 != 0 ? vmMinus14 / trueRange14 : 0;
            viMinus14List.Add(viMinus14);

            var signal = GetCompareSignal(viPlus14 - viMinus14, prevViPlus14 - prevViMinus14);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "ViPlus", viPlus14List },
            { "ViMinus", viMinus14List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.VortexIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the super trend.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="atrMult">The atr mult.</param>
    /// <returns></returns>
    public static StockData CalculateSuperTrend(this StockData stockData, MovingAvgType maType, int length = 22, decimal atrMult = 3)
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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentAtr = atrList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal atrValue = atrMult * currentAtr;
            decimal tempLongStop = currentValue - atrValue;
            decimal tempShortStop = currentValue + atrValue;

            decimal prevLongStop = longStopList.LastOrDefault();
            decimal longStop = prevValue > prevLongStop ? Math.Max(tempLongStop, prevLongStop) : tempLongStop;
            longStopList.Add(longStop);

            decimal prevShortStop = shortStopList.LastOrDefault();
            decimal shortStop = prevValue < prevShortStop ? Math.Max(tempShortStop, prevShortStop) : tempShortStop;
            shortStopList.Add(shortStop);

            decimal prevDir = dirList.LastOrDefault();
            decimal dir = prevDir == -1 && currentValue > prevShortStop ? 1 : prevDir == 1 && currentValue < prevLongStop ? -1 : prevDir;
            dirList.Add(dir);

            decimal prevTrend = trendList.LastOrDefault();
            decimal trend = dir > 0 ? longStop : shortStop;
            trendList.Add(trend);

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
    /// Calculates the trix.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateTrix(this StockData stockData, MovingAvgType maType, int length = 15, int signalLength = 9)
    {
        List<decimal> trixList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
        var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema3 = ema3List.ElementAtOrDefault(i);
            decimal prevEma3 = i >= 1 ? ema3List.ElementAtOrDefault(i - 1) : 0;

            decimal trix = CalculatePercentChange(ema3, prevEma3);
            trixList.Add(trix);
        }

        var trixSignalList = GetMovingAverageList(stockData, maType, signalLength, trixList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal trix = trixList.ElementAtOrDefault(i);
            decimal trixSignal = trixSignalList.ElementAtOrDefault(i);
            decimal prevTrix = i >= 1 ? trixList.ElementAtOrDefault(i - 1) : 0;
            decimal prevTrixSignal = i >= 1 ? trixSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(trix - trixSignal, prevTrix - prevTrixSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Trix", trixList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trixList;
        stockData.IndicatorName = IndicatorName.Trix;

        return stockData;
    }

    /// <summary>
    /// Calculates the williams r.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateWilliamsR(this StockData stockData, int length = 14)
    {
        List<decimal> williamsRList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal prevWilliamsR1 = i >= 1 ? williamsRList.ElementAtOrDefault(i - 1) : 0;
            decimal prevWilliamsR2 = i >= 2 ? williamsRList.ElementAtOrDefault(i - 2) : 0;

            decimal williamsR = highestHigh - lowestLow != 0 ? -100 * (highestHigh - currentClose) / (highestHigh - lowestLow) : -100;
            williamsRList.Add(williamsR);

            var signal = GetRsiSignal(williamsR - prevWilliamsR1, prevWilliamsR1 - prevWilliamsR2, williamsR, prevWilliamsR1, -20, -80);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Williams%R", williamsRList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = williamsRList;
        stockData.IndicatorName = IndicatorName.WilliamsR;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the true strength.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length1">The length1.</param>
    /// <param name="length2">The length2.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateTrueStrengthIndex(this StockData stockData, MovingAvgType maType, int length1 = 25, int length2 = 13,
        int signalLength = 7)
    {
        List<decimal> pcList = new();
        List<decimal> absPCList = new();
        List<decimal> tsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal pc = currentValue - prevValue;
            pcList.Add(pc);

            decimal absPC = Math.Abs(pc);
            absPCList.Add(absPC);
        }

        var pcSmooth1List = GetMovingAverageList(stockData, maType, length1, pcList);
        var pcSmooth2List = GetMovingAverageList(stockData, maType, length2, pcSmooth1List);
        var absPCSmooth1List = GetMovingAverageList(stockData, maType, length1, absPCList);
        var absPCSmooth2List = GetMovingAverageList(stockData, maType, length2, absPCSmooth1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal absSmooth2PC = absPCSmooth2List.ElementAtOrDefault(i);
            decimal smooth2PC = pcSmooth2List.ElementAtOrDefault(i);

            decimal tsi = absSmooth2PC != 0 ? MinOrMax(100 * smooth2PC / absSmooth2PC, 100, -100) : 0;
            tsiList.Add(tsi);
        }

        var tsiSignalList = GetMovingAverageList(stockData, maType, signalLength, tsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tsi = tsiList.ElementAtOrDefault(i);
            decimal tsiSignal = tsiSignalList.ElementAtOrDefault(i);
            decimal prevTsi = i >= 1 ? tsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevTsiSignal = i >= 1 ? tsiSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetRsiSignal(tsi - tsiSignal, prevTsi - prevTsiSignal, tsi, prevTsi, 25, -25);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tsi", tsiList },
            { "Signal", tsiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsiList;
        stockData.IndicatorName = IndicatorName.TrueStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the stochastic oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="kLength">Length of the K signal.</param>
    /// <param name="dLength">Length of the D signal.</param>
    /// <returns></returns>
    public static StockData CalculateStochasticOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, int kLength = 3, int dLength = 3)
    {
        List<decimal> fastKList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);

            decimal fastK = highestHigh - lowestLow != 0 ? MinOrMax((currentValue - lowestLow) / (highestHigh - lowestLow) * 100, 100, 0) : 0;
            fastKList.Add(fastK);
        }

        var fastDList = GetMovingAverageList(stockData, maType, kLength, fastKList);
        var slowDList = GetMovingAverageList(stockData, maType, dLength, fastDList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal slowK = fastDList.ElementAtOrDefault(i);
            decimal slowD = slowDList.ElementAtOrDefault(i);
            decimal prevSlowk = i >= 1 ? fastDList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSlowd = i >= 1 ? slowDList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetRsiSignal(slowK - slowD, prevSlowk - prevSlowd, slowK, prevSlowk, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastK", fastKList },
            { "FastD", fastDList },
            { "SlowD", slowDList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fastKList;
        stockData.IndicatorName = IndicatorName.StochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the price momentum oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length1">The length1.</param>
    /// <param name="length2">The length2.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculatePriceMomentumOscillator(this StockData stockData, MovingAvgType maType, int length1 = 35,
        int length2 = 20, int signalLength = 10)
    {
        List<decimal> pmoList = new();
        List<decimal> rocMaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal sc1 = 2 / (decimal)length1;
        decimal sc2 = 2 / (decimal)length2;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal roc = prevValue != 0 ? (currentValue - prevValue) / prevValue * 100 : 0;

            decimal prevRocMa1 = rocMaList.LastOrDefault();
            decimal rocMa = prevRocMa1 + ((roc - prevRocMa1) * sc1);
            rocMaList.AddRounded(rocMa);

            decimal prevPmo = pmoList.LastOrDefault();
            decimal pmo = prevPmo + (((rocMa * 10) - prevPmo) * sc2);
            pmoList.Add(pmo);
        }

        var pmoSignalList = GetMovingAverageList(stockData, maType, signalLength, pmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pmo = pmoList.ElementAtOrDefault(i);
            decimal prevPmo = i >= 1 ? pmoList.ElementAtOrDefault(i - 1) : 0;
            decimal pmoSignal = pmoSignalList.ElementAtOrDefault(i);
            decimal prevPmoSignal = i >= 1 ? pmoSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(pmo - pmoSignal, prevPmo - prevPmoSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pmo", pmoList },
            { "Signal", pmoSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pmoList;
        stockData.IndicatorName = IndicatorName.PriceMomentumOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the schaff trend cycle.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="cycleLength">Length of the cycle.</param>
    /// <returns></returns>
    public static StockData CalculateSchaffTrendCycle(this StockData stockData, MovingAvgType maType, int fastLength = 23, int slowLength = 50,
        int cycleLength = 10)
    {
        List<decimal> macdList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema23List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var ema50List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma23 = ema23List.ElementAtOrDefault(i);
            decimal currentEma50 = ema50List.ElementAtOrDefault(i);

            decimal macd = currentEma23 - currentEma50;
            macdList.Add(macd);
        }

        stockData.CustomValuesList = macdList;
        var stcList = CalculateStochasticOscillator(stockData, maType, length: cycleLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stc = stcList.ElementAtOrDefault(i);
            decimal prevStc1 = i >= 1 ? stcList.ElementAtOrDefault(i - 1) : 0;
            decimal prevStc2 = i >= 2 ? stcList.ElementAtOrDefault(i - 2) : 0;

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
    /// Calculates the index of the elder ray.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateElderRayIndex(this StockData stockData, MovingAvgType maType, int length = 13)
    {
        List<decimal> bullPowerList = new();
        List<decimal> bearPowerList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);

            decimal prevBullPower = bullPowerList.LastOrDefault();
            decimal bullPower = currentHigh - currentEma;
            bullPowerList.Add(bullPower);

            decimal prevBearPower = bearPowerList.LastOrDefault();
            decimal bearPower = currentLow - currentEma;
            bearPowerList.Add(bearPower);

            var signal = GetCompareSignal(bullPower - bearPower, prevBullPower - prevBearPower);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BullPower", bullPowerList },
            { "BearPower", bearPowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ElderRayIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the absolute price oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateAbsolutePriceOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 10, int slowLength = 20)
    {
        List<decimal> apoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal prevApo = apoList.LastOrDefault();
            decimal apo = fastEma - slowEma;
            apoList.Add(apo);

            var signal = GetCompareSignal(apo, prevApo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Apo", apoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = apoList;
        stockData.IndicatorName = IndicatorName.AbsolutePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the aroon oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAroonOscillator(this StockData stockData, int length = 25)
    {
        List<decimal> aroonOscillatorList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList.ElementAtOrDefault(i);
            tempList.Add(currentPrice);

            decimal maxPrice = highestList.ElementAtOrDefault(i);
            int maxIndex = tempList.LastIndexOf(maxPrice);
            decimal minPrice = lowestList.ElementAtOrDefault(i);
            int minIndex = tempList.LastIndexOf(minPrice);
            int daysSinceMax = i - maxIndex;
            int daysSinceMin = i - minIndex;
            decimal aroonUp = (decimal)(length - daysSinceMax) / length * 100;
            decimal aroonDown = (decimal)(length - daysSinceMin) / length * 100;

            decimal prevAroonOscillator = aroonOscillatorList.LastOrDefault();
            decimal aroonOscillator = aroonUp - aroonDown;
            aroonOscillatorList.Add(aroonOscillator);

            var signal = GetCompareSignal(aroonOscillator, prevAroonOscillator);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Aroon", aroonOscillatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aroonOscillatorList;
        stockData.IndicatorName = IndicatorName.AroonOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the absolute strength.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="maLength">Length of the ma.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateAbsoluteStrengthIndex(this StockData stockData, int length = 10, int maLength = 21, int signalLength = 34)
    {
        List<decimal> AList = new();
        List<decimal> MList = new();
        List<decimal> DList = new();
        List<decimal> mtList = new();
        List<decimal> utList = new();
        List<decimal> abssiEmaList = new();
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alp = (decimal)2 / (signalLength + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevA = AList.LastOrDefault();
            decimal A = currentValue > prevValue && prevValue != 0 ? prevA + ((currentValue / prevValue) - 1) : prevA;
            AList.Add(A);

            decimal prevM = MList.LastOrDefault();
            decimal M = currentValue == prevValue ? prevM + ((decimal)1 / length) : prevM;
            MList.Add(M);

            decimal prevD = DList.LastOrDefault();
            decimal D = currentValue < prevValue && currentValue != 0 ? prevD + ((prevValue / currentValue) - 1) : prevD;
            DList.Add(D);

            decimal abssi = (D + M) / 2 != 0 ? 1 - (1 / (1 + ((A + M) / 2 / ((D + M) / 2)))) : 1;
            decimal abssiEma = CalculateEMA(abssi, abssiEmaList.LastOrDefault(), maLength);
            abssiEmaList.Add(abssiEma);

            decimal abssio = abssi - abssiEma;
            decimal prevMt = mtList.LastOrDefault();
            decimal mt = (alp * abssio) + ((1 - alp) * prevMt);
            mtList.Add(mt);

            decimal prevUt = utList.LastOrDefault();
            decimal ut = (alp * mt) + ((1 - alp) * prevUt);
            utList.Add(ut);

            decimal s = (2 - alp) * (mt - ut) / (1 - alp);
            decimal prevd = dList.LastOrDefault();
            decimal d = abssio - s;
            dList.Add(d);

            var signal = GetCompareSignal(d, prevd);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Asi", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dList;
        stockData.IndicatorName = IndicatorName.AbsoluteStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the anchored momentum.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <param name="momentumLength">Length of the momentum.</param>
    /// <returns></returns>
    public static StockData CalculateAnchoredMomentum(this StockData stockData, MovingAvgType maType, int smoothLength = 7,
        int signalLength = 8, int momentumLength = 10)
    {
        List<decimal> tempList = new();
        List<decimal> amomList = new();
        List<decimal> amomsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int p = MinOrMax((2 * momentumLength) + 1);

        var emaList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma = emaList.ElementAtOrDefault(i);

            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal sma = tempList.TakeLastExt(p).Average();
            decimal prevAmom = amomList.LastOrDefault();
            decimal amom = sma != 0 ? 100 * ((currentEma / sma) - 1) : 0;
            amomList.Add(amom);

            decimal prevAmoms = amomsList.LastOrDefault();
            decimal amoms = amomList.TakeLastExt(signalLength).Average();
            amomsList.Add(amoms);

            var signal = GetCompareSignal(amom - amoms, prevAmom - prevAmoms);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Amom", amomList },
            { "Signal", amomsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = amomList;
        stockData.IndicatorName = IndicatorName.AnchoredMomentum;

        return stockData;
    }

    /// <summary>
    /// Calculates the index of the accumulative swing.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAccumulativeSwingIndex(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> accumulativeSwingIndexList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHighCurrentClose = prevHigh - currentClose;
            decimal prevLowCurrentClose = prevLow - currentClose;
            decimal prevClosePrevOpen = prevClose - prevOpen;
            decimal currentHighPrevClose = currentHigh - prevClose;
            decimal currentLowPrevClose = currentLow - prevClose;
            decimal t = currentHigh - currentLow;
            decimal k = Math.Max(Math.Abs(prevHighCurrentClose), Math.Abs(prevLowCurrentClose));
            decimal r = currentHighPrevClose > Math.Max(currentLowPrevClose, t) ? currentHighPrevClose - (0.5m * currentLowPrevClose) + (0.25m * prevClosePrevOpen) :
                currentLowPrevClose > Math.Max(currentHighPrevClose, t) ? currentLowPrevClose - (0.5m * currentHighPrevClose) + (0.25m * prevClosePrevOpen) :
                t > Math.Max(currentHighPrevClose, currentLowPrevClose) ? t + (0.25m * prevClosePrevOpen) : 0;
            decimal swingIndex = r != 0 && t != 0 ? 50 * ((prevClose - currentClose + (0.5m * prevClosePrevOpen) +
                (0.25m * (currentClose - currentOpen))) / r) * (k / t) : 0;

            decimal prevSwingIndex = accumulativeSwingIndexList.LastOrDefault();
            decimal accumulativeSwingIndex = prevSwingIndex + swingIndex;
            accumulativeSwingIndexList.Add(accumulativeSwingIndex);
        }

        var asiOscillatorList = GetMovingAverageList(stockData, maType, length, accumulativeSwingIndexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var asi = accumulativeSwingIndexList.ElementAtOrDefault(i);
            var prevAsi = i >= 1 ? accumulativeSwingIndexList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(asi, prevAsi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Asi", accumulativeSwingIndexList },
            { "Signal", asiOscillatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = accumulativeSwingIndexList;
        stockData.IndicatorName = IndicatorName.AccumulativeSwingIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive stochastic.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveStochastic(this StockData stockData, int length = 50, int fastLength = 50, int slowLength = 200)
    {
        List<decimal> stcList = new();
        List<Signal> signalsList = new();

        var srcList = CalculateLinearRegression(stockData, Math.Abs(slowLength - fastLength)).CustomValuesList;
        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];
        var (highest1List, lowest1List) = GetMaxAndMinValuesList(srcList, fastLength);
        var (highest2List, lowest2List) = GetMaxAndMinValuesList(srcList, slowLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal er = erList.ElementAtOrDefault(i);
            decimal src = srcList.ElementAtOrDefault(i);
            decimal highest1 = highest1List.ElementAtOrDefault(i);
            decimal lowest1 = lowest1List.ElementAtOrDefault(i);
            decimal highest2 = highest2List.ElementAtOrDefault(i);
            decimal lowest2 = lowest2List.ElementAtOrDefault(i);
            decimal prevStc1 = i >= 1 ? stcList.ElementAtOrDefault(i - 1) : 0;
            decimal prevStc2 = i >= 2 ? stcList.ElementAtOrDefault(i - 2) : 0;
            decimal a = (er * highest1) + ((1 - er) * highest2);
            decimal b = (er * lowest1) + ((1 - er) * lowest2);

            decimal stc = a - b != 0 ? MinOrMax((src - b) / (a - b), 1, 0) : 0;
            stcList.Add(stc);

            var signal = GetRsiSignal(stc - prevStc1, prevStc1 - prevStc2, stc, prevStc1, 0.8m, 0.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ast", stcList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stcList;
        stockData.IndicatorName = IndicatorName.AdaptiveStochastic;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive ergodic candlestick oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <param name="stochLength">Length of the stoch.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveErgodicCandlestickOscillator(this StockData stockData, MovingAvgType maType, int smoothLength = 5,
        int stochLength = 14, int signalLength = 9)
    {
        List<decimal> came1List = new();
        List<decimal> came2List = new();
        List<decimal> came11List = new();
        List<decimal> came22List = new();
        List<decimal> ecoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        decimal mep = (decimal)2 / (smoothLength + 1);
        decimal ce = (stochLength + smoothLength) * 2;

        var stochList = CalculateStochasticOscillator(stockData, maType, length: stochLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stoch = stochList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal vrb = Math.Abs(stoch - 50) / 50;

            decimal prevCame1 = came1List.LastOrDefault();
            decimal came1 = i < ce ? currentClose - currentOpen : prevCame1 + (mep * vrb * (currentClose - currentOpen - prevCame1));
            came1List.Add(came1);

            decimal prevCame2 = came2List.LastOrDefault();
            decimal came2 = i < ce ? currentHigh - currentLow : prevCame2 + (mep * vrb * (currentHigh - currentLow - prevCame2));
            came2List.Add(came2);

            decimal prevCame11 = came11List.LastOrDefault();
            decimal came11 = i < ce ? came1 : prevCame11 + (mep * vrb * (came1 - prevCame11));
            came11List.Add(came11);

            decimal prevCame22 = came22List.LastOrDefault();
            decimal came22 = i < ce ? came2 : prevCame22 + (mep * vrb * (came2 - prevCame22));
            came22List.Add(came22);

            decimal eco = came22 != 0 ? came11 / came22 * 100 : 0;
            ecoList.Add(eco);
        }

        var seList = GetMovingAverageList(stockData, maType, signalLength, ecoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var eco = ecoList.ElementAtOrDefault(i);
            var se = seList.ElementAtOrDefault(i);
            var prevEco = i >= 1 ? ecoList.ElementAtOrDefault(i - 1) : 0;
            var prevSe = i >= 1 ? seList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(eco - se, prevEco - prevSe);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eco", ecoList },
            { "Signal", seList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ecoList;
        stockData.IndicatorName = IndicatorName.AdaptiveErgodicCandlestickOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the average money flow oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAverageMoneyFlowOscillator(this StockData stockData, MovingAvgType maType, int length = 5, int smoothLength = 3)
    {
        List<decimal> chgList = new();
        List<decimal> rList = new();
        List<decimal> kList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var avgvList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal chg = currentValue - prevValue;
            chgList.Add(chg);
        }

        var avgcList = GetMovingAverageList(stockData, maType, length, chgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal avgv = avgvList.ElementAtOrDefault(i);
            decimal avgc = avgcList.ElementAtOrDefault(i);

            decimal r = Math.Abs(avgv * avgc) > 0 ? Log(Math.Abs(avgv * avgc)) * Math.Sign(avgc) : 0;
            rList.Add(r);

            var list = rList.TakeLastExt(length).ToList();
            decimal rh = list.Max();
            decimal rl = list.Min();
            decimal rs = rh != rl ? (r - rl) / (rh - rl) * 100 : 0;

            decimal k = (rs * 2) - 100;
            kList.Add(k);
        }

        var ksList = GetMovingAverageList(stockData, maType, smoothLength, kList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ks = ksList.ElementAtOrDefault(i);
            decimal prevKs = i >= 1 ? ksList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(ks, prevKs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Amfo", ksList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ksList;
        stockData.IndicatorName = IndicatorName.AverageMoneyFlowOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the absolute strength MTF indicator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAbsoluteStrengthMTFIndicator(this StockData stockData, MovingAvgType maType, int length = 50, int smoothLength = 25)
    {
        List<decimal> prevValuesList = new();
        List<decimal> bulls0List = new();
        List<decimal> bears0List = new();
        List<decimal> bulls1List = new();
        List<decimal> bears1List = new();
        List<decimal> bulls2List = new();
        List<decimal> bears2List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            prevValuesList.Add(prevValue);
        }

        var price1List = GetMovingAverageList(stockData, maType, length, inputList);
        var price2List = GetMovingAverageList(stockData, maType, length, prevValuesList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal price1 = price1List.ElementAtOrDefault(i);
            decimal price2 = price2List.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;

            decimal bulls0 = 0.5m * (Math.Abs(price1 - price2) + (price1 - price2));
            bulls0List.Add(bulls0);

            decimal bears0 = 0.5m * (Math.Abs(price1 - price2) - (price1 - price2));
            bears0List.Add(bears0);

            decimal bulls1 = price1 - lowest;
            bulls1List.Add(bulls1);

            decimal bears1 = highest - price1;
            bears1List.Add(bears1);

            decimal bulls2 = 0.5m * (Math.Abs(high - prevHigh) + (high - prevHigh));
            bulls2List.Add(bulls2);

            decimal bears2 = 0.5m * (Math.Abs(prevLow - low) + (prevLow - low));
            bears2List.Add(bears2);
        }

        var smthBulls0List = GetMovingAverageList(stockData, maType, smoothLength, bulls0List);
        var smthBears0List = GetMovingAverageList(stockData, maType, smoothLength, bears0List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bulls = smthBulls0List.ElementAtOrDefault(i);
            decimal bears = smthBears0List.ElementAtOrDefault(i);
            decimal prevBulls = i >= 1 ? smthBulls0List.ElementAtOrDefault(i - 1) : 0;
            decimal prevBears = i >= 1 ? smthBears0List.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(bulls - bears, prevBulls - prevBears);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Bulls", smthBulls0List },
            { "Bears", smthBears0List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.AbsoluteStrengthMTFIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the 4 Percentage Price Oscillator
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
    public static StockData Calculate4PercentagePriceOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 5, int length2 = 8, int length3 = 10, int length4 = 17,
        int length5 = 14, int length6 = 16, decimal blueMult = 4.3m, decimal yellowMult = 1.4m)
    {
        List<decimal> ppo1List = new();
        List<decimal> ppo2List = new();
        List<decimal> ppo3List = new();
        List<decimal> ppo4List = new();
        List<decimal> ppo2HistogramList = new();
        List<decimal> ppo4HistogramList = new();
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
            decimal ema5 = ema5List.ElementAtOrDefault(i);
            decimal ema8 = ema8List.ElementAtOrDefault(i);
            decimal ema10 = ema10List.ElementAtOrDefault(i);
            decimal ema14 = ema14List.ElementAtOrDefault(i);
            decimal ema16 = ema16List.ElementAtOrDefault(i);
            decimal ema17 = ema17List.ElementAtOrDefault(i);
            decimal macd1 = ema17 - ema14;
            decimal macd2 = ema17 - ema8;
            decimal macd3 = ema10 - ema16;
            decimal macd4 = ema5 - ema10;

            decimal ppo1 = ema14 != 0 ? macd1 / ema14 * 100 : 0;
            ppo1List.Add(ppo1);

            decimal ppo2 = ema8 != 0 ? macd2 / ema8 * 100 : 0;
            ppo2List.Add(ppo2);

            decimal ppo3 = ema16 != 0 ? macd3 / ema16 * 100 : 0;
            ppo3List.Add(ppo3);

            decimal ppo4 = ema10 != 0 ? macd4 / ema10 * 100 : 0;
            ppo4List.Add(ppo4);
        }

        var ppo1SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo1List);
        var ppo2SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo2List);
        var ppo3SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo3List);
        var ppo4SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo4List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo1 = ppo1List.ElementAtOrDefault(i);
            decimal ppo1SignalLine = ppo1SignalLineList.ElementAtOrDefault(i);
            decimal ppo2 = ppo2List.ElementAtOrDefault(i);
            decimal ppo2SignalLine = ppo2SignalLineList.ElementAtOrDefault(i);
            decimal ppo3 = ppo3List.ElementAtOrDefault(i);
            decimal ppo3SignalLine = ppo3SignalLineList.ElementAtOrDefault(i);
            decimal ppo4 = ppo4List.ElementAtOrDefault(i);
            decimal ppo4SignalLine = ppo4SignalLineList.ElementAtOrDefault(i);
            decimal ppo1Histogram = ppo1 - ppo1SignalLine;
            decimal ppoBlue = blueMult * ppo1Histogram;

            decimal prevPpo2Histogram = ppo2HistogramList.LastOrDefault();
            decimal ppo2Histogram = ppo2 - ppo2SignalLine;
            ppo2HistogramList.Add(ppo2Histogram);

            decimal ppo3Histogram = ppo3 - ppo3SignalLine;
            decimal ppoYellow = yellowMult * ppo3Histogram;

            decimal prevPpo4Histogram = ppo4HistogramList.LastOrDefault();
            decimal ppo4Histogram = ppo4 - ppo4SignalLine;
            ppo4HistogramList.Add(ppo4Histogram);

            decimal maxPpo = Math.Max(ppoBlue, Math.Max(ppoYellow, Math.Max(ppo2Histogram, ppo4Histogram)));
            decimal minPpo = Math.Min(ppoBlue, Math.Min(ppoYellow, Math.Min(ppo2Histogram, ppo4Histogram)));
            decimal currentPpo = (ppoBlue + ppoYellow + ppo2Histogram + ppo4Histogram) / 4;
            decimal ppoStochastic = maxPpo - minPpo != 0 ? MinOrMax((currentPpo - minPpo) / (maxPpo - minPpo) * 100, 100, 0) : 0;

            var signal = GetCompareSignal(ppo4Histogram - ppo2Histogram, prevPpo4Histogram - prevPpo2Histogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo1", ppo4List },
            { "Signal1", ppo4SignalLineList },
            { "Histogram1", ppo4HistogramList },
            { "Ppo2", ppo2List },
            { "Signal2", ppo2SignalLineList },
            { "Histogram2", ppo2HistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName._4PercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Japanese Correlation Coefficient
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateJapaneseCorrelationCoefficient(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 50)
    {
        List<decimal> joList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var hList = GetMovingAverageList(stockData, maType, length1, highList);
        var lList = GetMovingAverageList(stockData, maType, length1, lowList);
        var cList = GetMovingAverageList(stockData, maType, length1, inputList);
        var highestList = GetMaxAndMinValuesList(hList, length1).Item1;
        var lowestList = GetMaxAndMinValuesList(lList, length1).Item2;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal c = cList.ElementAtOrDefault(i);
            decimal prevC = i >= length ? cList.ElementAtOrDefault(i - length) : 0;
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal prevJo1 = i >= 1 ? joList.ElementAtOrDefault(i - 1) : 0;
            decimal prevJo2 = i >= 2 ? joList.ElementAtOrDefault(i - 2) : 0;
            decimal cChg = c - prevC;

            decimal jo = highest - lowest != 0 ? cChg / (highest - lowest) : 0;
            joList.Add(jo);

            var signal = GetCompareSignal(jo - prevJo1, prevJo1 - prevJo2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Jo", joList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = joList;
        stockData.IndicatorName = IndicatorName.JapaneseCorrelationCoefficient;

        return stockData;
    }

    /// <summary>
    /// Calculates the Jma Rsx Clone
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateJmaRsxClone(this StockData stockData, int length = 14)
    {
        List<decimal> rsxList = new();
        List<decimal> f8List = new();
        List<decimal> f28List = new();
        List<decimal> f30List = new();
        List<decimal> f38List = new();
        List<decimal> f40List = new();
        List<decimal> f48List = new();
        List<decimal> f50List = new();
        List<decimal> f58List = new();
        List<decimal> f60List = new();
        List<decimal> f68List = new();
        List<decimal> f70List = new();
        List<decimal> f78List = new();
        List<decimal> f80List = new();
        List<decimal> f88List = new();
        List<decimal> f90_List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal f18 = (decimal)3 / (length + 2);
        decimal f20 = 1 - f18;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevRsx1 = i >= 1 ? rsxList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRsx2 = i >= 2 ? rsxList.ElementAtOrDefault(i - 2) : 0;

            decimal prevF8 = f8List.LastOrDefault();
            decimal f8 = 100 * currentValue;
            f8List.Add(f8);

            decimal f10 = prevF8;
            decimal v8 = f8 - f10;

            decimal prevF28 = f28List.LastOrDefault();
            decimal f28 = (f20 * prevF28) + (f18 * v8);
            f28List.Add(f28);

            decimal prevF30 = f30List.LastOrDefault();
            decimal f30 = (f18 * f28) + (f20 * prevF30);
            f30List.Add(f30);

            decimal vC = (f28 * 1.5m) - (f30 * 0.5m);
            decimal prevF38 = f38List.LastOrDefault();
            decimal f38 = (f20 * prevF38) + (f18 * vC);
            f38List.Add(f38);

            decimal prevF40 = f40List.LastOrDefault();
            decimal f40 = (f18 * f38) + (f20 * prevF40);
            f40List.Add(f40);

            decimal v10 = (f38 * 1.5m) - (f40 * 0.5m);
            decimal prevF48 = f48List.LastOrDefault();
            decimal f48 = (f20 * prevF48) + (f18 * v10);
            f48List.Add(f48);

            decimal prevF50 = f50List.LastOrDefault();
            decimal f50 = (f18 * f48) + (f20 * prevF50);
            f50List.Add(f50);

            decimal v14 = (f48 * 1.5m) - (f50 * 0.5m);
            decimal prevF58 = f58List.LastOrDefault();
            decimal f58 = (f20 * prevF58) + (f18 * Math.Abs(v8));
            f58List.Add(f58);

            decimal prevF60 = f60List.LastOrDefault();
            decimal f60 = (f18 * f58) + (f20 * prevF60);
            f60List.Add(f60);

            decimal v18 = (f58 * 1.5m) - (f60 * 0.5m);
            decimal prevF68 = f68List.LastOrDefault();
            decimal f68 = (f20 * prevF68) + (f18 * v18);
            f68List.Add(f68);

            decimal prevF70 = f70List.LastOrDefault();
            decimal f70 = (f18 * f68) + (f20 * prevF70);
            f70List.Add(f70);

            decimal v1C = (f68 * 1.5m) - (f70 * 0.5m);
            decimal prevF78 = f78List.LastOrDefault();
            decimal f78 = (f20 * prevF78) + (f18 * v1C);
            f78List.Add(f78);

            decimal prevF80 = f80List.LastOrDefault();
            decimal f80 = (f18 * f78) + (f20 * prevF80);
            f80List.Add(f80);

            decimal v20 = (f78 * 1.5m) - (f80 * 0.5m);
            decimal prevF88 = f88List.LastOrDefault();
            decimal prevF90_ = f90_List.LastOrDefault();
            decimal f90_ = prevF90_ == 0 ? 1 : prevF88 <= prevF90_ ? prevF88 + 1 : prevF90_ + 1;
            f90_List.Add(f90_);

            decimal f88 = prevF90_ == 0 && length - 1 >= 5 ? length - 1 : 5;
            decimal f0 = f88 >= f90_ && f8 != f10 ? 1 : 0;
            decimal f90 = f88 == f90_ && f0 == 0 ? 0 : f90_;
            decimal v4_ = f88 < f90 && v20 > 0 ? MinOrMax(((v14 / v20) + 1) * 50, 100, 0) : 50;
            decimal rsx = v4_ > 100 ? 100 : v4_ < 0 ? 0 : v4_;
            rsxList.Add(rsx);

            var signal = GetRsiSignal(rsx - prevRsx1, prevRsx1 - prevRsx2, rsx, prevRsx1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsx", rsxList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsxList;
        stockData.IndicatorName = IndicatorName.JmaRsxClone;

        return stockData;
    }

    /// <summary>
    /// Calculates the Jrc Fractal Dimension
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateJrcFractalDimension(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 20, int length2 = 5, int smoothLength = 5)
    {
        List<decimal> smallSumList = new();
        List<decimal> smallRangeList = new();
        List<decimal> fdList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        int wind1 = MinOrMax((length2 - 1) * length1);
        int wind2 = MinOrMax(length2 * length1);
        decimal nLog = Log(length2);

        var (highest1List, lowest1List) = GetMaxAndMinValuesList(highList, lowList, length1);
        var (highest2List, lowest2List) = GetMaxAndMinValuesList(highList, lowList, wind2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest1 = highest1List.ElementAtOrDefault(i);
            decimal lowest1 = lowest1List.ElementAtOrDefault(i);
            decimal prevValue1 = i >= length1 ? inputList.ElementAtOrDefault(i - length1) : 0;
            decimal highest2 = highest2List.ElementAtOrDefault(i);
            decimal lowest2 = lowest2List.ElementAtOrDefault(i);
            decimal prevValue2 = i >= wind2 ? inputList.ElementAtOrDefault(i - wind2) : 0;
            decimal bigRange = Math.Max(prevValue2, highest2) - Math.Min(prevValue2, lowest2);

            decimal prevSmallRange = i >= wind1 ? smallRangeList.ElementAtOrDefault(i - wind1) : 0;
            decimal smallRange = Math.Max(prevValue1, highest1) - Math.Min(prevValue1, lowest1);
            smallRangeList.AddRounded(smallRange);

            decimal prevSmallSum = i >= 1 ? smallSumList.LastOrDefault() : smallRange;
            decimal smallSum = prevSmallSum + smallRange - prevSmallRange;
            smallSumList.AddRounded(smallSum);

            decimal value1 = wind1 != 0 ? smallSum / wind1 : 0;
            decimal value2 = value1 != 0 ? bigRange / value1 : 0;
            decimal temp = value2 > 0 ? Log(value2) : 0;

            decimal fd = nLog != 0 ? 2 - (temp / nLog) : 0;
            fdList.AddRounded(fd);
        }

        var jrcfdList = GetMovingAverageList(stockData, maType, smoothLength, fdList);
        var jrcfdSignalList = GetMovingAverageList(stockData, maType, smoothLength, jrcfdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var jrcfd = jrcfdList.ElementAtOrDefault(i);
            var jrcfdSignal = jrcfdSignalList.ElementAtOrDefault(i);
            var prevJrcfd = i >= 1 ? jrcfdList.ElementAtOrDefault(i - 1) : 0;
            var prevJrcfdSignal = i >= 1 ? jrcfdSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(jrcfd - jrcfdSignal, prevJrcfd - prevJrcfdSignal, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Jrcfd", jrcfdList },
            { "Signal", jrcfdSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = jrcfdList;
        stockData.IndicatorName = IndicatorName.JrcFractalDimension;

        return stockData;
    }

    /// <summary>
    /// Calculates the Zweig Market Breadth Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateZweigMarketBreadthIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 10)
    {
        List<decimal> advDiffList = new();
        List<decimal> advancesList = new();
        List<decimal> declinesList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal advance = currentValue > prevValue ? 1 : 0;
            advancesList.Add(advance);

            decimal decline = currentValue < prevValue ? 1 : 0;
            declinesList.Add(decline);

            decimal advSum = advancesList.TakeLastExt(length).Sum();
            decimal decSum = declinesList.TakeLastExt(length).Sum();

            decimal advDiff = advSum + decSum != 0 ? advSum / (advSum + decSum) : 0;
            advDiffList.Add(advDiff);
        }

        var zmbtiList = GetMovingAverageList(stockData, maType, length, advDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevZmbti1 = i >= 1 ? zmbtiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevZmbti2 = i >= 2 ? zmbtiList.ElementAtOrDefault(i - 2) : 0;
            decimal zmbti = zmbtiList.ElementAtOrDefault(i);

            var signal = GetRsiSignal(zmbti - prevZmbti1, prevZmbti1 - prevZmbti2, zmbti, prevZmbti1, 0.615m, 0.4m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Zmbti", zmbtiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zmbtiList;
        stockData.IndicatorName = IndicatorName.ZweigMarketBreadthIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Z Distance From Vwap Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateZDistanceFromVwapIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.VolumeWeightedAveragePrice, int length = 20)
    {
        List<decimal> zscoreList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var vwapList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = vwapList;
        var vwapSdList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevZScore1 = i >= 1 ? zscoreList.ElementAtOrDefault(i - 1) : 0;
            decimal prevZScore2 = i >= 2 ? zscoreList.ElementAtOrDefault(i - 2) : 0;
            decimal mean = vwapList.ElementAtOrDefault(i);
            decimal vwapsd = vwapSdList.ElementAtOrDefault(i);

            decimal zscore = vwapsd != 0 ? (currentValue - mean) / vwapsd : 0;
            zscoreList.Add(zscore);

            var signal = GetRsiSignal(zscore - prevZScore1, prevZScore1 - prevZScore2, zscore, prevZScore1, 2, -2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Zscore", zscoreList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zscoreList;
        stockData.IndicatorName = IndicatorName.ZDistanceFromVwap;

        return stockData;
    }

    /// <summary>
    /// Calculates the Z Score
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="matype"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateZScore(this StockData stockData, MovingAvgType matype = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> zScorePopulationList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, matype, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal dev = currentValue - sma;
            decimal stdDevPopulation = stdDevList.ElementAtOrDefault(i);

            decimal prevZScorePopulation = zScorePopulationList.LastOrDefault();
            decimal zScorePopulation = stdDevPopulation != 0 ? dev / stdDevPopulation : 0;
            zScorePopulationList.Add(zScorePopulation);

            var signal = GetCompareSignal(zScorePopulation, prevZScorePopulation);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Zscore", zScorePopulationList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zScorePopulationList;
        stockData.IndicatorName = IndicatorName.ZScore;

        return stockData;
    }

    /// <summary>
    /// Calculates the Zero Lag Smoothed Cycle
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateZeroLagSmoothedCycle(this StockData stockData, int length = 100)
    {
        List<decimal> ax1List = new();
        List<decimal> lx1List = new();
        List<decimal> ax2List = new();
        List<decimal> lx2List = new();
        List<decimal> ax3List = new();
        List<decimal> lcoList = new();
        List<decimal> filterList = new();
        List<decimal> lcoSma1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal linreg = linregList.ElementAtOrDefault(i);

            decimal ax1 = currentValue - linreg;
            ax1List.Add(ax1);
        }

        stockData.CustomValuesList = ax1List;
        var ax1LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ax1 = ax1List.ElementAtOrDefault(i);
            decimal ax1Linreg = ax1LinregList.ElementAtOrDefault(i);

            decimal lx1 = ax1 + (ax1 - ax1Linreg);
            lx1List.Add(lx1);
        }

        stockData.CustomValuesList = lx1List;
        var lx1LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal lx1 = lx1List.ElementAtOrDefault(i);
            decimal lx1Linreg = lx1LinregList.ElementAtOrDefault(i);

            decimal ax2 = lx1 - lx1Linreg;
            ax2List.Add(ax2);
        }

        stockData.CustomValuesList = ax2List;
        var ax2LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ax2 = ax2List.ElementAtOrDefault(i);
            decimal ax2Linreg = ax2LinregList.ElementAtOrDefault(i);

            decimal lx2 = ax2 + (ax2 - ax2Linreg);
            lx2List.Add(lx2);
        }

        stockData.CustomValuesList = lx2List;
        var lx2LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal lx2 = lx2List.ElementAtOrDefault(i);
            decimal lx2Linreg = lx2LinregList.ElementAtOrDefault(i);

            decimal ax3 = lx2 - lx2Linreg;
            ax3List.Add(ax3);
        }

        stockData.CustomValuesList = ax3List;
        var ax3LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ax3 = ax3List.ElementAtOrDefault(i);
            decimal ax3Linreg = ax3LinregList.ElementAtOrDefault(i);

            decimal prevLco = lcoList.LastOrDefault();
            decimal lco = ax3 + (ax3 - ax3Linreg);
            lcoList.Add(lco);

            decimal lcoSma1 = lcoList.TakeLastExt(length1).Average();
            lcoSma1List.Add(lcoSma1);

            decimal lcoSma2 = lcoSma1List.TakeLastExt(length1).Average();
            decimal prevFilter = filterList.LastOrDefault();
            decimal filter = -lcoSma2 * 2;
            filterList.Add(filter);

            var signal = GetCompareSignal(lco - filter, prevLco - prevFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lco", lcoList },
            { "Filter", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.ZeroLagSmoothedCycle;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bayesian Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="stdDevMult"></param>
    /// <param name="lowerThreshold"></param>
    /// <returns></returns>
    public static StockData CalculateBayesianOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20, decimal stdDevMult = 2.5m, decimal lowerThreshold = 15)
    {
        List<decimal> probBbUpperUpSeqList = new();
        List<decimal> probBbUpperDownSeqList = new();
        List<decimal> probBbBasisUpSeqList = new();
        List<decimal> probBbBasisUpList = new();
        List<decimal> probBbBasisDownSeqList = new();
        List<decimal> probBbBasisDownList = new();
        List<decimal> sigmaProbsDownList = new();
        List<decimal> sigmaProbsUpList = new();
        List<decimal> probPrimeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var bbList = CalculateBollingerBands(stockData, maType, length, stdDevMult);
        var upperBbList = bbList.OutputValues["UpperBand"];
        var basisList = bbList.OutputValues["MiddleBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal upperBb = upperBbList.ElementAtOrDefault(i);
            decimal basis = basisList.ElementAtOrDefault(i);

            decimal probBbUpperUpSeq = currentValue > upperBb ? 1 : 0;
            probBbUpperUpSeqList.Add(probBbUpperUpSeq);

            decimal probBbUpperUp = probBbUpperUpSeqList.TakeLastExt(length).Average();

            decimal probBbUpperDownSeq = currentValue < upperBb ? 1 : 0;
            probBbUpperDownSeqList.Add(probBbUpperDownSeq);

            decimal probBbUpperDown = probBbUpperDownSeqList.TakeLastExt(length).Average();
            decimal probUpBbUpper = probBbUpperUp + probBbUpperDown != 0 ? probBbUpperUp / (probBbUpperUp + probBbUpperDown) : 0;
            decimal probDownBbUpper = probBbUpperUp + probBbUpperDown != 0 ? probBbUpperDown / (probBbUpperUp + probBbUpperDown) : 0;

            decimal probBbBasisUpSeq = currentValue > basis ? 1 : 0;
            probBbBasisUpSeqList.Add(probBbBasisUpSeq);

            decimal probBbBasisUp = probBbBasisUpSeqList.TakeLastExt(length).Average();
            probBbBasisUpList.Add(probBbBasisUp);

            decimal probBbBasisDownSeq = currentValue < basis ? 1 : 0;
            probBbBasisDownSeqList.Add(probBbBasisDownSeq);

            decimal probBbBasisDown = probBbBasisDownSeqList.TakeLastExt(length).Average();
            probBbBasisDownList.Add(probBbBasisDown);

            decimal probUpBbBasis = probBbBasisUp + probBbBasisDown != 0 ? probBbBasisUp / (probBbBasisUp + probBbBasisDown) : 0;
            decimal probDownBbBasis = probBbBasisUp + probBbBasisDown != 0 ? probBbBasisDown / (probBbBasisUp + probBbBasisDown) : 0;

            decimal prevSigmaProbsDown = sigmaProbsDownList.LastOrDefault();
            decimal sigmaProbsDown = probUpBbUpper != 0 && probUpBbBasis != 0 ? ((probUpBbUpper * probUpBbBasis) / (probUpBbUpper * probUpBbBasis)) +
                ((1 - probUpBbUpper) * (1 - probUpBbBasis)) : 0;
            sigmaProbsDownList.Add(sigmaProbsDown);

            decimal prevSigmaProbsUp = sigmaProbsUpList.LastOrDefault();
            decimal sigmaProbsUp = probDownBbUpper != 0 && probDownBbBasis != 0 ? ((probDownBbUpper * probDownBbBasis) / (probDownBbUpper * probDownBbBasis)) +
                ((1 - probDownBbUpper) * (1 - probDownBbBasis)) : 0;
            sigmaProbsUpList.Add(sigmaProbsUp);

            decimal prevProbPrime = probPrimeList.LastOrDefault();
            decimal probPrime = sigmaProbsDown != 0 && sigmaProbsUp != 0 ? ((sigmaProbsDown * sigmaProbsUp) / (sigmaProbsDown * sigmaProbsUp)) +
                ((1 - sigmaProbsDown) * (1 - sigmaProbsUp)) : 0;
            probPrimeList.Add(probPrime);

            bool longUsingProbPrime = probPrime > lowerThreshold / 100 && prevProbPrime == 0;
            bool longUsingSigmaProbsUp = sigmaProbsUp < 1 && prevSigmaProbsUp == 1;
            bool shortUsingProbPrime = probPrime == 0 && prevProbPrime > lowerThreshold / 100;
            bool shortUsingSigmaProbsDown = sigmaProbsDown < 1 && prevSigmaProbsDown == 1;

            var signal = GetConditionSignal(longUsingProbPrime || longUsingSigmaProbsUp, shortUsingProbPrime || shortUsingSigmaProbsDown);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "SigmaProbsDown", sigmaProbsDownList },
            { "SigmaProbsUp", sigmaProbsUpList },
            { "ProbPrime", probPrimeList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.BayesianOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bear Power Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateBearPowerIndicator(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> bpiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal open = openList.ElementAtOrDefault(i);
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);

            decimal bpi = close < open ? high - low : prevClose > open ? Math.Max(close - open, high - low) :
                close > open ? Math.Max(open - low, high - close) : prevClose > open ? Math.Max(prevClose - low, high - close) :
                high - close > close - low ? high - low : prevClose > open ? Math.Max(prevClose - open, high - low) :
                high - close < close - low ? open - low : close > open ? Math.Max(close - low, high - close) :
                close > open ? Math.Max(prevClose - open, high - close) : prevClose < open ? Math.Max(open - low, high - close) : high - low;
            bpiList.Add(bpi);
        }

        var bpiEmaList = GetMovingAverageList(stockData, maType, length, bpiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var bpi = bpiList.ElementAtOrDefault(i);
            var bpiEma = bpiEmaList.ElementAtOrDefault(i);
            var prevBpi = i >= 1 ? bpiList.ElementAtOrDefault(i - 1) : 0;
            var prevBpiEma = i >= 1 ? bpiEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(bpi - bpiEma, prevBpi - prevBpiEma, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BearPower", bpiList },
            { "Signal", bpiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bpiList;
        stockData.IndicatorName = IndicatorName.BearPowerIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bull Power Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateBullPowerIndicator(this StockData stockData, MovingAvgType maType, int length = 14)
    {
        List<decimal> bpiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal open = openList.ElementAtOrDefault(i);
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);

            decimal bpi = close < open ? Math.Max(high - open, close - low) : prevClose < open ? Math.Max(high - prevClose, close - low) :
                close > open ? Math.Max(open - prevClose, high - low) : prevClose > open ? high - low :
                high - close > close - low ? high - open : prevClose < open ? Math.Max(high - prevClose, close - low) :
                high - close < close - low ? Math.Max(open - close, high - low) : prevClose > open ? high - low :
                prevClose > open ? Math.Max(high - open, close - low) : prevClose < open ? Math.Max(open - close, high - low) : high - low;
            bpiList.Add(bpi);
        }

        var bpiEmaList = GetMovingAverageList(stockData, maType, length, bpiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var bpi = bpiList.ElementAtOrDefault(i);
            var bpiEma = bpiEmaList.ElementAtOrDefault(i);
            var prevBpi = i >= 1 ? bpiList.ElementAtOrDefault(i - 1) : 0;
            var prevBpiEma = i >= 1 ? bpiEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(bpi - bpiEma, prevBpi - prevBpiEma, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BullPower", bpiList },
            { "Signal", bpiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bpiList;
        stockData.IndicatorName = IndicatorName.BullPowerIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Belkhayate Timing
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateBelkhayateTiming(this StockData stockData)
    {
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevHigh1 = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow1 = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh2 = i >= 2 ? highList.ElementAtOrDefault(i - 2) : 0;
            decimal prevLow2 = i >= 2 ? lowList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHigh3 = i >= 3 ? highList.ElementAtOrDefault(i - 3) : 0;
            decimal prevLow3 = i >= 3 ? lowList.ElementAtOrDefault(i - 3) : 0;
            decimal prevHigh4 = i >= 4 ? highList.ElementAtOrDefault(i - 4) : 0;
            decimal prevLow4 = i >= 4 ? lowList.ElementAtOrDefault(i - 4) : 0;
            decimal prevB1 = i >= 1 ? bList.ElementAtOrDefault(i - 1) : 0;
            decimal prevB2 = i >= 2 ? bList.ElementAtOrDefault(i - 2) : 0;
            decimal middle = (((currentHigh + currentLow) / 2) + ((prevHigh1 + prevLow1) / 2) + ((prevHigh2 + prevLow2) / 2) +
                ((prevHigh3 + prevLow3) / 2) + ((prevHigh4 + prevLow4) / 2)) / 5;
            decimal scale = ((currentHigh - currentLow + (prevHigh1 - prevLow1) + (prevHigh2 - prevLow2) + (prevHigh3 - prevLow3) +
                (prevHigh4 - prevLow4)) / 5) * 0.2m;

            decimal b = scale != 0 ? (currentValue - middle) / scale : 0;
            bList.Add(b);

            var signal = GetRsiSignal(b - prevB1, prevB1 - prevB2, b, prevB1, 4, -4);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Belkhayate", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.BelkhayateTiming;

        return stockData;
    }

    /// <summary>
    /// Calculates the Better Volume Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <returns></returns>
    public static StockData CalculateBetterVolumeIndicator(this StockData stockData, int length = 8, int lbLength = 2)
    {
        List<decimal> v1List = new();
        List<decimal> v2List = new();
        List<decimal> v3List = new();
        List<decimal> v4List = new();
        List<decimal> v5List = new();
        List<decimal> v6List = new();
        List<decimal> v7List = new();
        List<decimal> v8List = new();
        List<decimal> v9List = new();
        List<decimal> v10List = new();
        List<decimal> v11List = new();
        List<decimal> v12List = new();
        List<decimal> v13List = new();
        List<decimal> v14List = new();
        List<decimal> v15List = new();
        List<decimal> v16List = new();
        List<decimal> v17List = new();
        List<decimal> v18List = new();
        List<decimal> v19List = new();
        List<decimal> v20List = new();
        List<decimal> v21List = new();
        List<decimal> v22List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, lbLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal highLowRange = highest - lowest;
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;
            decimal range = CalculateTrueRange(currentHigh, currentLow, prevClose);

            decimal prevV1 = v1List.LastOrDefault();
            decimal v1 = currentClose > currentOpen ? range / ((2 * range) + currentOpen - currentClose) * currentVolume :
                currentClose < currentOpen ? (range + currentClose - currentOpen) / ((2 * range) + currentClose - currentOpen) * currentVolume :
                0.5m * currentVolume;
            v1List.Add(v1);

            decimal prevV2 = v2List.LastOrDefault();
            decimal v2 = currentVolume - v1;
            v2List.Add(v2);

            decimal prevV3 = v3List.LastOrDefault();
            decimal v3 = v1 + v2;
            v3List.Add(v3);

            decimal v4 = v1 * range;
            v4List.AddRounded(v4);

            decimal v5 = (v1 - v2) * range;
            v5List.AddRounded(v5);

            decimal v6 = v2 * range;
            v6List.AddRounded(v6);

            decimal v7 = (v2 - v1) * range;
            v7List.AddRounded(v7);

            decimal v8 = range != 0 ? v1 / range : 0;
            v8List.AddRounded(v8);

            decimal v9 = range != 0 ? (v1 - v2) / range : 0;
            v9List.AddRounded(v9);

            decimal v10 = range != 0 ? v2 / range : 0;
            v10List.AddRounded(v10);

            decimal v11 = range != 0 ? (v2 - v1) / range : 0;
            v11List.AddRounded(v11);

            decimal v12 = range != 0 ? v3 / range : 0;
            v12List.AddRounded(v12);

            decimal v13 = v3 + prevV3;
            v13List.AddRounded(v13);

            decimal v14 = (v1 + prevV1) * highLowRange;
            v14List.AddRounded(v14);

            decimal v15 = (v1 + prevV1 - v2 - prevV2) * highLowRange;
            v15List.AddRounded(v15);

            decimal v16 = (v2 + prevV2) * highLowRange;
            v16List.AddRounded(v16);

            decimal v17 = (v2 + prevV2 - v1 - prevV1) * highLowRange;
            v17List.AddRounded(v17);

            decimal v18 = highLowRange != 0 ? (v1 + prevV1) / highLowRange : 0;
            v18List.AddRounded(v18);

            decimal v19 = highLowRange != 0 ? (v1 + prevV1 - v2 - prevV2) / highLowRange : 0;
            v19List.AddRounded(v19);

            decimal v20 = highLowRange != 0 ? (v2 + prevV2) / highLowRange : 0;
            v20List.AddRounded(v20);

            decimal v21 = highLowRange != 0 ? (v2 + prevV2 - v1 - prevV1) / highLowRange : 0;
            v21List.AddRounded(v21);

            decimal v22 = highLowRange != 0 ? v13 / highLowRange : 0;
            v22List.AddRounded(v22);

            bool c1 = v3 == v3List.TakeLastExt(length).Min();
            bool c2 = v4 == v4List.TakeLastExt(length).Max() && currentClose > currentOpen;
            bool c3 = v5 == v5List.TakeLastExt(length).Max() && currentClose > currentOpen;
            bool c4 = v6 == v6List.TakeLastExt(length).Max() && currentClose < currentOpen;
            bool c5 = v7 == v7List.TakeLastExt(length).Max() && currentClose < currentOpen;
            bool c6 = v8 == v8List.TakeLastExt(length).Min() && currentClose < currentOpen;
            bool c7 = v9 == v9List.TakeLastExt(length).Min() && currentClose < currentOpen;
            bool c8 = v10 == v10List.TakeLastExt(length).Min() && currentClose > currentOpen;
            bool c9 = v11 == v11List.TakeLastExt(length).Min() && currentClose > currentOpen;
            bool c10 = v12 == v12List.TakeLastExt(length).Max();
            bool c11 = v13 == v13List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose > prevOpen;
            bool c12 = v14 == v14List.TakeLastExt(length).Max() && currentClose > currentOpen && prevClose > prevOpen;
            bool c13 = v15 == v15List.TakeLastExt(length).Max() && currentClose > currentOpen && prevClose < prevOpen;
            bool c14 = v16 == v16List.TakeLastExt(length).Min() && currentClose < currentOpen && prevClose < prevOpen;
            bool c15 = v17 == v17List.TakeLastExt(length).Min() && currentClose < currentOpen && prevClose < prevOpen;
            bool c16 = v18 == v18List.TakeLastExt(length).Min() && currentClose < currentOpen && prevClose < prevOpen;
            bool c17 = v19 == v19List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose < prevOpen;
            bool c18 = v20 == v20List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose > prevOpen;
            bool c19 = v21 == v21List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose > prevOpen;
            bool c20 = v22 == v22List.TakeLastExt(length).Min();
            bool climaxUp = c2 || c3 || c8 || c9 || c12 || c13 || c18 || c19;
            bool climaxDown = c4 || c5 || c6 || c7 || c14 || c15 || c16 || c17;
            bool churn = c10 || c20;
            bool lowVolue = c1 || c11;

            var signal = GetConditionSignal(climaxUp, climaxDown);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Bvi", v1List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = v1List;
        stockData.IndicatorName = IndicatorName.BetterVolumeIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bilateral Stochastic Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateBilateralStochasticOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100, int signalLength = 20)
    {
        List<decimal> bullList = new();
        List<decimal> bearList = new();
        List<decimal> rangeList = new();
        List<decimal> maxList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(smaList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);

            decimal range = highest - lowest;
            rangeList.Add(range);
        }

        var rangeSmaList = GetMovingAverageList(stockData, maType, length, rangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal rangeSma = rangeSmaList.ElementAtOrDefault(i);

            decimal bull = rangeSma != 0 ? (sma / rangeSma) - (lowest / rangeSma) : 0;
            bullList.Add(bull);

            decimal bear = rangeSma != 0 ? Math.Abs((sma / rangeSma) - (highest / rangeSma)) : 0;
            bearList.Add(bear);

            decimal max = Math.Max(bull, bear);
            maxList.Add(max);
        }

        var signalList = GetMovingAverageList(stockData, maType, signalLength, maxList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bull = bullList.ElementAtOrDefault(i);
            decimal bear = bearList.ElementAtOrDefault(i);
            decimal sig = signalList.ElementAtOrDefault(i);

            var signal = GetConditionSignal(bull > bear || bull > sig, bear > bull || bull < sig);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Bull", bullList },
            { "Bear", bearList },
            { "Bso", maxList },
            { "Signal", signalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = maxList;
        stockData.IndicatorName = IndicatorName.BilateralStochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Buff Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateBuffAverage(this StockData stockData, int fastLength = 5, int slowLength = 20)
    {
        List<decimal> priceVolList = new();
        List<decimal> fastBuffList = new();
        List<decimal> slowBuffList = new();
        List<decimal> tempVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            tempVolumeList.Add(currentVolume);

            decimal priceVol = currentValue * currentVolume;
            priceVolList.Add(priceVol);

            decimal fastBuffNum = priceVolList.TakeLastExt(fastLength).Sum();
            decimal fastBuffDenom = tempVolumeList.TakeLastExt(fastLength).Sum();

            decimal prevFastBuff = fastBuffList.LastOrDefault();
            decimal fastBuff = fastBuffDenom != 0 ? fastBuffNum / fastBuffDenom : 0;
            fastBuffList.Add(fastBuff);

            decimal slowBuffNum = priceVolList.TakeLastExt(slowLength).Sum();
            decimal slowBuffDenom = tempVolumeList.TakeLastExt(slowLength).Sum();

            decimal prevSlowBuff = slowBuffList.LastOrDefault();
            decimal slowBuff = slowBuffDenom != 0 ? slowBuffNum / slowBuffDenom : 0;
            slowBuffList.Add(slowBuff);

            var signal = GetCompareSignal(fastBuff - slowBuff, prevFastBuff - prevSlowBuff);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastBuff", fastBuffList },
            { "SlowBuff", slowBuffList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.BuffAverage;

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevUti1 = i >= 1 ? utiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevUti2 = i >= 2 ? utiList.ElementAtOrDefault(i - 2) : 0;

            decimal adv = currentValue > prevValue ? currentValue - prevValue : 0;
            advList.Add(adv);

            decimal dec = currentValue < prevValue ? prevValue - currentValue : 0;
            decList.Add(dec);

            decimal advSum = advList.TakeLastExt(length).Sum();
            decimal decSum = decList.TakeLastExt(length).Sum();

            decimal advVol = currentValue > prevValue && advSum != 0 ? currentVolume / advSum : 0;
            advVolList.Add(advVol);

            decimal decVol = currentValue < prevValue && decSum != 0 ? currentVolume / decSum : 0;
            decVolList.Add(decVol);

            decimal advVolSum = advVolList.TakeLastExt(length).Sum();
            decimal decVolSum = decVolList.TakeLastExt(length).Sum();
            decimal top = decSum != 0 ? advSum / decSum : 0;
            decimal bot = decVolSum != 0 ? advVolSum / decVolSum : 0;
            decimal ut = bot != 0 ? top / bot : 0;
            decimal utRev = top != 0 ? -1 * bot / top : 0;

            decimal uti = ut + 1 != 0 ? (ut - 1) / (ut + 1) : 0;
            utiList.Add(uti);

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
    /// Calculates the Ultimate Trader Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <param name="smoothLength"></param>
    /// <param name="rangeLength"></param>
    /// <returns></returns>
    public static StockData CalculateUltimateTraderOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 10, int lbLength = 5, int smoothLength = 4, int rangeLength = 2)
    {
        List<decimal> dxList = new();
        List<decimal> dxiList = new();
        List<decimal> trList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, rangeLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trList.AddRounded(tr);
        }

        stockData.CustomValuesList = trList;
        var trStoList = CalculateStochasticOscillator(stockData, maType, length: lbLength).CustomValuesList;
        stockData.CustomValuesList = volumeList;
        var vStoList = CalculateStochasticOscillator(stockData, maType, length: lbLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal body = close - openList.ElementAtOrDefault(i);
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);
            decimal range = high - low;
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal c = close - prevClose;
            decimal sign = Math.Sign(c);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal vSto = vStoList.ElementAtOrDefault(i);
            decimal trSto = trStoList.ElementAtOrDefault(i);
            decimal k1 = range != 0 ? body / range * 100 : 0;
            decimal k2 = range == 0 ? 0 : ((close - low) / range * 100 * 2) - 100;
            decimal k3 = c == 0 || highest - lowest == 0 ? 0 : ((close - lowest) / (highest - lowest) * 100 * 2) - 100;
            decimal k4 = highest - lowest != 0 ? c / (highest - lowest) * 100 : 0;
            decimal k5 = sign * trSto;
            decimal k6 = sign * vSto;
            decimal bullScore = Math.Max(0, k1) + Math.Max(0, k2) + Math.Max(0, k3) + Math.Max(0, k4) + Math.Max(0, k5) + Math.Max(0, k6);
            decimal bearScore = -1 * (Math.Min(0, k1) + Math.Min(0, k2) + Math.Min(0, k3) + Math.Min(0, k4) + Math.Min(0, k5) + Math.Min(0, k6));

            decimal dx = bearScore != 0 ? bullScore / bearScore : 0;
            dxList.Add(dx);

            decimal dxi = (2 * (100 - (100 / (1 + dx)))) - 100;
            dxiList.Add(dxi);
        }

        var dxiavgList = GetMovingAverageList(stockData, maType, lbLength, dxiList);
        var dxisList = GetMovingAverageList(stockData, maType, smoothLength, dxiavgList);
        var dxissList = GetMovingAverageList(stockData, maType, smoothLength, dxisList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dxis = dxisList.ElementAtOrDefault(i);
            decimal dxiss = dxissList.ElementAtOrDefault(i);
            decimal prevDxis = i >= 1 ? dxisList.ElementAtOrDefault(i - 1) : 0;
            decimal prevDxiss = i >= 1 ? dxissList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(dxis - dxiss, prevDxis - prevDxiss);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Uto", dxisList },
            { "Signal", dxissList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dxisList;
        stockData.IndicatorName = IndicatorName.UltimateTraderOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Upside Potential Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateUpsidePotentialRatio(this StockData stockData, int length = 30, decimal bmk = 0.05m)
    {
        List<decimal> retList = new();
        List<decimal> upsidePotentialList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal barMin = 60 * 24;
        decimal minPerYr = 60 * 24 * 30 * 12;
        decimal barsPerYr = minPerYr / barMin;
        decimal ratio = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal bench = Pow(1 + bmk, length / (double)barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);

            decimal downSide = 0, upSide = 0;
            for (int j = i - (length + 1); j <= i; j++)
            {
                decimal iValue = j >= 0 && i >= j ? retList.ElementAtOrDefault(i - j) : 0;

                if (iValue < bench)
                {
                    downSide += Pow(iValue - bench, 2) * ratio;
                }
                if (iValue > bench)
                {
                    upSide += (iValue - bench) * ratio;
                }
            }

            decimal prevUpsidePotential = upsidePotentialList.LastOrDefault();
            decimal upsidePotential = downSide >= 0 ? upSide / Sqrt(downSide) : 0;
            upsidePotentialList.Add(upsidePotential);

            var signal = GetCompareSignal(upsidePotential - 5, prevUpsidePotential - 5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Upr", upsidePotentialList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = upsidePotentialList;
        stockData.IndicatorName = IndicatorName.UpsidePotentialRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ultimate Momentum Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="length6"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateUltimateMomentumIndicator(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 13, int length2 = 19, int length3 = 21, int length4 = 39,
        int length5 = 50, int length6 = 200, decimal stdDevMult = 1.5m)
    {
        List<decimal> utmList = new();
        List<Signal> signalsList = new();

        var moVar = CalculateMcClellanOscillator(stockData, maType, fastLength: length2, slowLength: length4);
        var advSumList = moVar.OutputValues["AdvSum"];
        var decSumList = moVar.OutputValues["DecSum"];
        var moList = moVar.OutputValues["Mo"];
        var bbPctList = CalculateBollingerBandsPercentB(stockData, stdDevMult, maType, length5).CustomValuesList;
        var mfi1List = CalculateMoneyFlowIndex(stockData, inputName, length2).CustomValuesList;
        var mfi2List = CalculateMoneyFlowIndex(stockData, inputName, length3).CustomValuesList;
        var mfi3List = CalculateMoneyFlowIndex(stockData, inputName, length4).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mo = moList.ElementAtOrDefault(i);
            decimal bbPct = bbPctList.ElementAtOrDefault(i);
            decimal mfi1 = mfi1List.ElementAtOrDefault(i);
            decimal mfi2 = mfi2List.ElementAtOrDefault(i);
            decimal mfi3 = mfi3List.ElementAtOrDefault(i);
            decimal advSum = advSumList.ElementAtOrDefault(i);
            decimal decSum = decSumList.ElementAtOrDefault(i);
            decimal ratio = decSum != 0 ? advSum / decSum : 0;

            decimal utm = (200 * bbPct) + (100 * ratio) + (2 * mo) + (1.5m * mfi3) + (3 * mfi2) + (3 * mfi1);
            utmList.Add(utm);
        }

        stockData.CustomValuesList = utmList;
        var utmRsiList = CalculateRelativeStrengthIndex(stockData, maType, length1, length1).CustomValuesList;
        var utmiList = GetMovingAverageList(stockData, maType, length1, utmRsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal utmi = utmiList.ElementAtOrDefault(i);
            decimal prevUtmi1 = i >= 1 ? utmiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevUtmi2 = i >= 2 ? utmiList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(utmi - prevUtmi1, prevUtmi1 - prevUtmi2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Utm", utmiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = utmiList;
        stockData.IndicatorName = IndicatorName.UltimateMomentumIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Upside Downside Volume
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateUpsideDownsideVolume(this StockData stockData, int length = 50)
    {
        List<decimal> upVolList = new();
        List<decimal> downVolList = new();
        List<decimal> upDownVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal upVol = currentValue > prevValue ? currentVolume : 0;
            upVolList.Add(upVol);

            decimal downVol = currentValue < prevValue ? currentVolume * -1 : 0;
            downVolList.Add(downVol);

            decimal upVolSum = upVolList.TakeLastExt(length).Sum();
            decimal downVolSum = downVolList.TakeLastExt(length).Sum();

            decimal prevUpDownVol = upDownVolumeList.LastOrDefault();
            decimal upDownVol = downVolSum != 0 ? upVolSum / downVolSum : 0;
            upDownVolumeList.Add(upDownVol);

            var signal = GetCompareSignal(upDownVol, prevUpDownVol);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Udv", upDownVolumeList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = upDownVolumeList;
        stockData.IndicatorName = IndicatorName.UpsideDownsideVolume;

        return stockData;
    }

    /// <summary>
    /// Calculates the Uhl Ma Crossover System
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateUhlMaCrossoverSystem(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100)
    {
        List<decimal> cmaList = new();
        List<decimal> ctsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var varList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal prevVar = i >= length ? varList.ElementAtOrDefault(i - length) : 0;
            decimal prevCma = i >= 1 ? cmaList.LastOrDefault() : currentValue;
            decimal prevCts = i >= 1 ? ctsList.LastOrDefault() : currentValue;
            decimal secma = Pow(sma - prevCma, 2);
            decimal sects = Pow(currentValue - prevCts, 2);
            decimal ka = prevVar < secma && secma != 0 ? 1 - (prevVar / secma) : 0;
            decimal kb = prevVar < sects && sects != 0 ? 1 - (prevVar / sects) : 0;

            decimal cma = (ka * sma) + ((1 - ka) * prevCma);
            cmaList.Add(cma);

            decimal cts = (kb * currentValue) + ((1 - kb) * prevCts);
            ctsList.Add(cts);

            var signal = GetCompareSignal(cts - cma, prevCts - prevCma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cts", ctsList },
            { "Cma", cmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.UhlMaCrossoverSystem;

        return stockData;
    }

    /// <summary>
    /// Calculates the McClellan Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateMcClellanOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int fastLength = 19, int slowLength = 39, int signalLength = 9, decimal mult = 1000)
    {
        List<decimal> advancesList = new();
        List<decimal> declinesList = new();
        List<decimal> advancesSumList = new();
        List<decimal> declinesSumList = new();
        List<decimal> ranaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal advance = currentValue > prevValue ? 1 : 0;
            advancesList.Add(advance);

            decimal decline = currentValue < prevValue ? 1 : 0;
            declinesList.Add(decline);

            decimal advanceSum = advancesList.TakeLastExt(fastLength).Sum();
            advancesSumList.Add(advanceSum);

            decimal declineSum = declinesList.TakeLastExt(fastLength).Sum();
            declinesSumList.Add(declineSum);

            decimal rana = advanceSum + declineSum != 0 ? mult * (advanceSum - declineSum) / (advanceSum + declineSum) : 0;
            ranaList.Add(rana);
        }

        stockData.CustomValuesList = ranaList;
        var moList = CalculateMovingAverageConvergenceDivergence(stockData, maType, fastLength, slowLength, signalLength);
        var mcclellanOscillatorList = moList.OutputValues["Macd"];
        var mcclellanSignalLineList = moList.OutputValues["Signal"];
        var mcclellanHistogramList = moList.OutputValues["Histogram"];
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mcclellanHistogram = mcclellanHistogramList.ElementAtOrDefault(i);
            decimal prevMcclellanHistogram = i >= 1 ? mcclellanHistogramList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(mcclellanHistogram, prevMcclellanHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "AdvSum", advancesSumList },
            { "DecSum", declinesSumList },
            { "Mo", mcclellanOscillatorList },
            { "Signal", mcclellanSignalLineList },
            { "Histogram", mcclellanHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mcclellanOscillatorList;
        stockData.IndicatorName = IndicatorName.McClellanOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Woodie Commodity Channel Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateWoodieCommodityChannelIndex(this StockData stockData, MovingAvgType maType, int fastLength = 6,
        int slowLength = 14)
    {
        List<decimal> histogramList = new();
        List<Signal> signalsList = new();

        var cciList = CalculateCommodityChannelIndex(stockData, maType, slowLength).CustomValuesList;
        var turboCciList = CalculateCommodityChannelIndex(stockData, maType, fastLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal cci = cciList.ElementAtOrDefault(i);
            decimal cciTurbo = turboCciList.ElementAtOrDefault(i);

            decimal prevCciHistogram = histogramList.LastOrDefault();
            decimal cciHistogram = cciTurbo - cci;
            histogramList.AddRounded(cciHistogram);

            var signal = GetCompareSignal(cciHistogram, prevCciHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastCci", turboCciList },
            { "SlowCci", cciList },
            { "Histogram", histogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.WoodieCommodityChannelIndex;

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
            decimal ap = inputList.ElementAtOrDefault(i);
            decimal esa = emaList.ElementAtOrDefault(i);

            decimal absApEsa = Math.Abs(ap - esa);
            absApEsaList.Add(absApEsa);
        }

        var dList = GetMovingAverageList(stockData, maType, length1, absApEsaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ap = inputList.ElementAtOrDefault(i);
            decimal esa = emaList.ElementAtOrDefault(i);
            decimal d = dList.ElementAtOrDefault(i);

            decimal ci = d != 0 ? (ap - esa) / (0.015m * d) : 0;
            ciList.Add(ci);
        }

        var tciList = GetMovingAverageList(stockData, maType, length2, ciList);
        var wt2List = GetMovingAverageList(stockData, maType, smoothLength, tciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tci = tciList.ElementAtOrDefault(i);
            decimal wt2 = wt2List.ElementAtOrDefault(i);
            decimal prevTci = i >= 1 ? tciList.ElementAtOrDefault(i - 1) : 0;
            decimal prevWt2 = i >= 1 ? wt2List.ElementAtOrDefault(i - 1) : 0;

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
    /// Calculates the Williams Fractals
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateWilliamsFractals(this StockData stockData, int length = 2)
    {
        List<decimal> upFractalList = new();
        List<decimal> dnFractalList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevHigh = i >= length - 2 ? highList.ElementAtOrDefault(i - (length - 2)) : 0;
            decimal prevHigh1 = i >= length - 1 ? highList.ElementAtOrDefault(i - (length - 1)) : 0;
            decimal prevHigh2 = i >= length ? highList.ElementAtOrDefault(i - length) : 0;
            decimal prevHigh3 = i >= length + 1 ? highList.ElementAtOrDefault(i - (length + 1)) : 0;
            decimal prevHigh4 = i >= length + 2 ? highList.ElementAtOrDefault(i - (length + 2)) : 0;
            decimal prevHigh5 = i >= length + 3 ? highList.ElementAtOrDefault(i - (length + 3)) : 0;
            decimal prevHigh6 = i >= length + 4 ? highList.ElementAtOrDefault(i - (length + 4)) : 0;
            decimal prevHigh7 = i >= length + 5 ? highList.ElementAtOrDefault(i - (length + 5)) : 0;
            decimal prevHigh8 = i >= length + 8 ? highList.ElementAtOrDefault(i - (length + 6)) : 0;
            decimal prevLow = i >= length - 2 ? lowList.ElementAtOrDefault(i - (length - 2)) : 0;
            decimal prevLow1 = i >= length - 1 ? lowList.ElementAtOrDefault(i - (length - 1)) : 0;
            decimal prevLow2 = i >= length ? lowList.ElementAtOrDefault(i - length) : 0;
            decimal prevLow3 = i >= length + 1 ? lowList.ElementAtOrDefault(i - (length + 1)) : 0;
            decimal prevLow4 = i >= length + 2 ? lowList.ElementAtOrDefault(i - (length + 2)) : 0;
            decimal prevLow5 = i >= length + 3 ? lowList.ElementAtOrDefault(i - (length + 3)) : 0;
            decimal prevLow6 = i >= length + 4 ? lowList.ElementAtOrDefault(i - (length + 4)) : 0;
            decimal prevLow7 = i >= length + 5 ? lowList.ElementAtOrDefault(i - (length + 5)) : 0;
            decimal prevLow8 = i >= length + 8 ? lowList.ElementAtOrDefault(i - (length + 6)) : 0;

            decimal prevUpFractal = upFractalList.LastOrDefault();
            decimal upFractal = (prevHigh4 < prevHigh2 && prevHigh3 < prevHigh2 && prevHigh1 < prevHigh2 && prevHigh < prevHigh2) ||
                (prevHigh5 < prevHigh2 && prevHigh4 < prevHigh2 && prevHigh3 == prevHigh2 && prevHigh1 < prevHigh2 && prevHigh1 < prevHigh2) ||
                (prevHigh6 < prevHigh2 && prevHigh5 < prevHigh2 && prevHigh4 == prevHigh2 && prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 &&
                prevHigh < prevHigh2) || (prevHigh7 < prevHigh2 && prevHigh6 < prevHigh2 && prevHigh5 == prevHigh2 && prevHigh4 == prevHigh2 &&
                prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 && prevHigh < prevHigh2) || (prevHigh8 < prevHigh2 && prevHigh7 < prevHigh2 &&
                prevHigh6 == prevHigh2 && prevHigh5 <= prevHigh2 && prevHigh4 == prevHigh2 && prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 &&
                prevHigh < prevHigh2) ? 1 : 0;
            upFractalList.Add(upFractal);

            decimal prevDnFractal = dnFractalList.LastOrDefault();
            decimal dnFractal = (prevLow4 > prevLow2 && prevLow3 > prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow5 > prevLow2 &&
                prevLow4 > prevLow2 && prevLow3 == prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow6 > prevLow2 &&
                prevLow5 > prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) ||
                (prevLow7 > prevLow2 && prevLow6 > prevLow2 && prevLow5 == prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 &&
                prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow8 > prevLow2 && prevLow7 > prevLow2 && prevLow6 == prevLow2 &&
                prevLow5 >= prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) ? 1 : 0;
            dnFractalList.Add(dnFractal);

            var signal = GetCompareSignal(upFractal - dnFractal, prevUpFractal - prevDnFractal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpFractal", upFractalList },
            { "DnFractal", dnFractalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.WilliamsFractals;

        return stockData;
    }

    /// <summary>
    /// Calculates the Williams Accumulation Distribution
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateWilliamsAccumulationDistribution(this StockData stockData)
    {
        List<decimal> wadList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;

            decimal prevWad = wadList.LastOrDefault();
            decimal wad = close > prevClose ? prevWad + close - prevLow : close < prevClose ? prevWad + close - prevHigh : 0;
            wadList.Add(wad);

            var signal = GetCompareSignal(wad, prevWad);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wad", wadList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wadList;
        stockData.IndicatorName = IndicatorName.WilliamsAccumulationDistribution;

        return stockData;
    }

    /// <summary>
    /// Calculates the Wami Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateWamiOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 13, int length2 = 4)
    {
        List<decimal> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal diff = currentValue - prevValue;
            diffList.Add(diff);
        }

        var wma1List = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, length2, diffList);
        var ema2List = GetMovingAverageList(stockData, maType, length1, wma1List);
        var wamiList = GetMovingAverageList(stockData, maType, length1, ema2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wami = wamiList.ElementAtOrDefault(i);
            decimal prevWami = i >= 1 ? wamiList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(wami, prevWami);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wami", wamiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wamiList;
        stockData.IndicatorName = IndicatorName.WamiOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Waddah Attar Explosion
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="sensitivity"></param>
    /// <returns></returns>
    public static StockData CalculateWaddahAttarExplosion(this StockData stockData, int fastLength = 20, int slowLength = 40, decimal sensitivity = 150)
    {
        List<decimal> t1List = new();
        List<decimal> t2List = new();
        List<decimal> e1List = new();
        List<decimal> temp1List = new();
        List<decimal> temp2List = new();
        List<decimal> temp3List = new();
        List<decimal> trendUpList = new();
        List<decimal> trendDnList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var macd1List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        var bbList = CalculateBollingerBands(stockData, length: fastLength);
        var upperBollingerBandList = bbList.OutputValues["UpperBand"];
        var lowerBollingerBandList = bbList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            temp1List.Add(prevValue1);

            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            temp2List.Add(prevValue2);

            decimal prevValue3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            temp3List.Add(prevValue3);
        }

        stockData.CustomValuesList = temp1List;
        var macd2List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        stockData.CustomValuesList = temp2List;
        var macd3List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        stockData.CustomValuesList = temp3List;
        var macd4List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentMacd1 = macd1List.ElementAtOrDefault(i);
            decimal currentMacd2 = macd2List.ElementAtOrDefault(i);
            decimal currentMacd3 = macd3List.ElementAtOrDefault(i);
            decimal currentMacd4 = macd4List.ElementAtOrDefault(i);
            decimal currentUpperBB = upperBollingerBandList.ElementAtOrDefault(i);
            decimal currentLowerBB = lowerBollingerBandList.ElementAtOrDefault(i);

            decimal t1 = (currentMacd1 - currentMacd2) * sensitivity;
            t1List.AddRounded(t1);

            decimal t2 = (currentMacd3 - currentMacd4) * sensitivity;
            t2List.AddRounded(t2);

            decimal prevE1 = e1List.LastOrDefault();
            decimal e1 = currentUpperBB - currentLowerBB;
            e1List.Add(e1);

            decimal prevTrendUp = trendUpList.LastOrDefault();
            decimal trendUp = (t1 >= 0) ? t1 : 0;
            trendUpList.Add(trendUp);

            decimal trendDown = (t1 < 0) ? (-1 * t1) : 0;
            trendDnList.Add(trendDown);

            var signal = GetConditionSignal(trendUp > prevTrendUp && trendUp > e1 && e1 > prevE1 && trendUp > fastLength && e1 > fastLength,
                trendUp < e1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "T1", t1List },
            { "T2", t2List },
            { "E1", e1List },
            { "TrendUp", trendUpList },
            { "TrendDn", trendDnList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.WaddahAttarExplosion;

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
    /// Calculates the Quantitative Qualitative Estimation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <param name="fastFactor"></param>
    /// <param name="slowFactor"></param>
    /// <returns></returns>
    public static StockData CalculateQuantitativeQualitativeEstimation(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14, int smoothLength = 5, decimal fastFactor = 2.618m,
        decimal slowFactor = 4.236m)
    {
        List<decimal> atrRsiList = new();
        List<decimal> fastAtrRsiList = new();
        List<decimal> slowAtrRsiList = new();
        List<Signal> signalsList = new();

        int wildersLength = (length * 2) - 1;

        var rsiValueList = CalculateRelativeStrengthIndex(stockData, maType, length, smoothLength);
        var rsiEmaList = rsiValueList.OutputValues["Signal"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentRsiEma = rsiEmaList.ElementAtOrDefault(i);
            decimal prevRsiEma = i >= 1 ? rsiEmaList.ElementAtOrDefault(i - 1) : 0;

            decimal atrRsi = Math.Abs(currentRsiEma - prevRsiEma);
            atrRsiList.Add(atrRsi);
        }

        var atrRsiEmaList = GetMovingAverageList(stockData, maType, wildersLength, atrRsiList);
        var atrRsiEmaSmoothList = GetMovingAverageList(stockData, maType, wildersLength, atrRsiEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal atrRsiEmaSmooth = atrRsiEmaSmoothList.ElementAtOrDefault(i);
            decimal prevAtrRsiEmaSmooth = i >= 1 ? atrRsiEmaSmoothList.ElementAtOrDefault(i - 1) : 0;

            decimal prevFastTl = fastAtrRsiList.LastOrDefault();
            decimal fastTl = atrRsiEmaSmooth * fastFactor;
            fastAtrRsiList.Add(fastTl);

            decimal prevSlowTl = slowAtrRsiList.LastOrDefault();
            decimal slowTl = atrRsiEmaSmooth * slowFactor;
            slowAtrRsiList.Add(slowTl);

            var signal = GetBullishBearishSignal(atrRsiEmaSmooth - Math.Max(fastTl, slowTl), prevAtrRsiEmaSmooth - Math.Max(prevFastTl, prevSlowTl),
                atrRsiEmaSmooth - Math.Min(fastTl, slowTl), prevAtrRsiEmaSmooth - Math.Min(prevFastTl, prevSlowTl));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastAtrRsi", fastAtrRsiList },
            { "SlowAtrRsi", slowAtrRsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.QuantitativeQualitativeEstimation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Quasi White Noise
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="noiseLength"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static StockData CalculateQuasiWhiteNoise(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 20, int noiseLength = 500, decimal divisor = 40)
    {
        List<decimal> whiteNoiseList = new();
        List<decimal> whiteNoiseVarianceList = new();
        List<Signal> signalsList = new();

        var connorsRsiList = CalculateConnorsRelativeStrengthIndex(stockData, maType, noiseLength, noiseLength, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal connorsRsi = connorsRsiList.ElementAtOrDefault(i);
            decimal prevConnorsRsi1 = i >= 1 ? connorsRsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevConnorsRsi2 = i >= 2 ? connorsRsiList.ElementAtOrDefault(i - 2) : 0;

            decimal whiteNoise = (connorsRsi - 50) * (1 / divisor);
            whiteNoiseList.Add(whiteNoise);

            var signal = GetRsiSignal(connorsRsi - prevConnorsRsi1, prevConnorsRsi1 - prevConnorsRsi2, connorsRsi, prevConnorsRsi1, 70, 30);
            signalsList.Add(signal);
        }

        var whiteNoiseSmaList = GetMovingAverageList(stockData, maType, noiseLength, whiteNoiseList);
        stockData.CustomValuesList = whiteNoiseList;
        var whiteNoiseStdDevList = CalculateStandardDeviationVolatility(stockData, noiseLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal whiteNoiseStdDev = whiteNoiseStdDevList.ElementAtOrDefault(i);

            decimal whiteNoiseVariance = Pow(whiteNoiseStdDev, 2);
            whiteNoiseVarianceList.Add(whiteNoiseVariance);
        }

        stockData.OutputValues = new()
        {
            { "WhiteNoise", whiteNoiseList },
            { "WhiteNoiseMa", whiteNoiseSmaList },
            { "WhiteNoiseStdDev", whiteNoiseStdDevList },
            { "WhiteNoiseVariance", whiteNoiseVarianceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = whiteNoiseList;
        stockData.IndicatorName = IndicatorName.QuasiWhiteNoise;

        return stockData;
    }

    /// <summary>
    /// Calculates the LBR Paint Bars
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <param name="atrMult"></param>
    /// <returns></returns>
    public static StockData CalculateLBRPaintBars(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 9,
        int lbLength = 16, decimal atrMult = 2.5m)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<decimal> aatrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, lbLength);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal currentAtr = atrList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal aatr = atrMult * currentAtr;
            aatrList.Add(aatr);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = lowest + aatr;
            lowerBandList.Add(lowerBand);

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = highest - aatr;
            upperBandList.Add(upperBand);

            var signal = GetBullishBearishSignal(currentValue - Math.Max(lowerBand, upperBand), prevValue - Math.Max(prevLowerBand, prevUpperBand),
                currentValue - Math.Min(lowerBand, upperBand), prevValue - Math.Min(prevLowerBand, prevUpperBand));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "LowerBand", lowerBandList },
            { "MiddleBand", aatrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.LBRPaintBars;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linear Quadratic Convergence Divergence Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateLinearQuadraticConvergenceDivergenceOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 50, int signalLength = 25)
    {
        List<decimal> lqcdList = new();
        List<decimal> histList = new();
        List<Signal> signalsList = new();

        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        var yList = CalculateQuadraticRegression(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal linreg = linregList.ElementAtOrDefault(i);
            decimal y = yList.ElementAtOrDefault(i);

            decimal lqcd = y - linreg;
            lqcdList.Add(lqcd);
        }

        var signList = GetMovingAverageList(stockData, maType, signalLength, lqcdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sign = signList.ElementAtOrDefault(i);
            decimal lqcd = lqcdList.ElementAtOrDefault(i);
            decimal osc = lqcd - sign;

            decimal prevHist = histList.LastOrDefault();
            decimal hist = osc - sign;
            histList.Add(hist);

            var signal = GetCompareSignal(hist, prevHist);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lqcdo", histList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = histList;
        stockData.IndicatorName = IndicatorName.LinearQuadraticConvergenceDivergenceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Logistic Correlation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static StockData CalculateLogisticCorrelation(this StockData stockData, int length = 100, decimal k = 10)
    {
        List<decimal> tempList = new();
        List<decimal> indexList = new();
        List<decimal> logList = new();
        List<decimal> corrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal index = i;
            indexList.Add(index);

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.Add((decimal)corr);
        }

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal prevLog1 = i >= 1 ? logList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLog2 = i >= 2 ? logList.ElementAtOrDefault(i - 2) : 0;

            decimal log = 1 / (1 + Exp(k * -corr));
            logList.Add(log);

            var signal = GetCompareSignal(log - prevLog1, prevLog1 - prevLog2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LogCorr", logList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = logList;
        stockData.IndicatorName = IndicatorName.LogisticCorrelation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linda Raschke 3/10 Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateLindaRaschke3_10Oscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 3, int slowLength = 10, int smoothLength = 16)
    {
        List<decimal> macdList = new();
        List<decimal> macdHistogramList = new();
        List<decimal> ppoList = new();
        List<decimal> ppoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma3 = fastSmaList.ElementAtOrDefault(i);
            decimal sma10 = slowSmaList.ElementAtOrDefault(i);

            decimal ppo = sma10 != 0 ? (sma3 - sma10) / sma10 * 100 : 0;
            ppoList.Add(ppo);

            decimal macd = sma3 - sma10;
            macdList.Add(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, maType, smoothLength, macdList);
        var ppoSignalLineList = GetMovingAverageList(stockData, maType, smoothLength, ppoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ppo = ppoList.ElementAtOrDefault(i);
            decimal ppoSignalLine = ppoSignalLineList.ElementAtOrDefault(i);
            decimal macd = macdList.ElementAtOrDefault(i);
            decimal macdSignalLine = macdSignalLineList.ElementAtOrDefault(i);

            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            decimal prevMacdHistogram = macdHistogramList.LastOrDefault();
            decimal macdHistogram = macd - macdSignalLine;
            macdHistogramList.Add(macdHistogram);

            var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LindaMacd", macdList },
            { "LindaMacdSignal", macdSignalLineList },
            { "LindaMacdHistogram", macdHistogramList },
            { "LindaPpo", ppoList },
            { "LindaPpoSignal", ppoSignalLineList },
            { "LindaPpoHistogram", ppoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = macdList;
        stockData.IndicatorName = IndicatorName.LindaRaschke3_10Oscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Information Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateInformationRatio(this StockData stockData, int length = 30, decimal bmk = 0.05m)
    {
        List<decimal> infoList = new();
        List<decimal> retList = new();
        List<decimal> deviationSquaredList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal barMin = 60 * 24;
        decimal minPerYr = 60 * 24 * 30 * 12;
        decimal barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal bench = Pow(1 + bmk, length / (double)barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);

            decimal retSma = retList.TakeLastExt(length).Average();
            decimal deviationSquared = Pow(ret - retSma, 2);
            deviationSquaredList.Add(deviationSquared);

            decimal divisionOfSum = deviationSquaredList.TakeLastExt(length).Average();
            decimal stdDeviation = divisionOfSum >= 0 ? Sqrt(divisionOfSum) : 0;

            decimal prevInfo = infoList.LastOrDefault();
            decimal info = stdDeviation != 0 ? (retSma - bench) / stdDeviation : 0;
            infoList.Add(info);

            var signal = GetCompareSignal(info - 5, prevInfo - 5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ir", infoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = infoList;
        stockData.IndicatorName = IndicatorName.InformationRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Impulse Percentage Price Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateImpulsePercentagePriceOscillator(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 34, int signalLength = 9)
    {
        List<decimal> ppoList = new();
        List<decimal> ppoSignalLineList = new();
        List<decimal> ppoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _, _) = GetInputValuesList(inputName, stockData);

        var typicalPriceZeroLagEmaList = GetMovingAverageList(stockData, MovingAvgType.ZeroLagExponentialMovingAverage, length, inputList);
        var wellesWilderHighMovingAvgList = GetMovingAverageList(stockData, maType, length, highList);
        var wellesWilderLowMovingAvgList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hi = wellesWilderHighMovingAvgList.ElementAtOrDefault(i);
            decimal lo = wellesWilderLowMovingAvgList.ElementAtOrDefault(i);
            decimal mi = typicalPriceZeroLagEmaList.ElementAtOrDefault(i);
            decimal macd = mi > hi ? mi - hi : mi < lo ? mi - lo : 0;

            decimal ppo = mi > hi && hi != 0 ? macd / hi * 100 : mi < lo && lo != 0 ? macd / lo * 100 : 0;
            ppoList.Add(ppo);

            decimal ppoSignalLine = ppoList.TakeLastExt(signalLength).Average();
            ppoSignalLineList.Add(ppoSignalLine);

            decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
            decimal ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.Add(ppoHistogram);

            var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ppo", ppoList },
            { "Signal", ppoSignalLineList },
            { "Histogram", ppoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ppoList;
        stockData.IndicatorName = IndicatorName.ImpulsePercentagePriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Volatility Index V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeVolatilityIndexV1(this StockData stockData,
        MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 10, int smoothLength = 14)
    {
        List<decimal> upList = new();
        List<decimal> downList = new();
        List<decimal> rviOriginalList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDeviationList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentStdDeviation = stdDeviationList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal up = currentValue > prevValue ? currentStdDeviation : 0;
            upList.Add(up);

            decimal down = currentValue < prevValue ? currentStdDeviation : 0;
            downList.Add(down);
        }

        var upAvgList = GetMovingAverageList(stockData, maType, smoothLength, upList);
        var downAvgList = GetMovingAverageList(stockData, maType, smoothLength, downList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal avgUp = upAvgList.ElementAtOrDefault(i);
            decimal avgDown = downAvgList.ElementAtOrDefault(i);
            decimal prevRvi1 = i >= 1 ? rviOriginalList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRvi2 = i >= 1 ? rviOriginalList.ElementAtOrDefault(i - 2) : 0;
            decimal rs = avgDown != 0 ? avgUp / avgDown : 0;

            decimal rvi = avgDown == 0 ? 100 : avgUp == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rviOriginalList.Add(rvi);

            var signal = GetRsiSignal(rvi - prevRvi1, prevRvi1 - prevRvi2, rvi, prevRvi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rvi", rviOriginalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rviOriginalList;
        stockData.IndicatorName = IndicatorName.RelativeVolatilityIndexV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Volatility Index V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeVolatilityIndexV2(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 10, int smoothLength = 14)
    {
        List<decimal> rviList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        stockData.CustomValuesList = highList;
        var rviHighList = CalculateRelativeVolatilityIndexV1(stockData, maType, length, smoothLength).CustomValuesList;
        stockData.CustomValuesList = lowList;
        var rviLowList = CalculateRelativeVolatilityIndexV1(stockData, maType, length, smoothLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rviOriginalHigh = rviHighList.ElementAtOrDefault(i);
            decimal rviOriginalLow = rviLowList.ElementAtOrDefault(i);
            decimal prevRvi1 = i >= 1 ? rviList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRvi2 = i >= 2 ? rviList.ElementAtOrDefault(i - 2) : 0;

            decimal rvi = (rviOriginalHigh + rviOriginalLow) / 2;
            rviList.Add(rvi);

            var signal = GetRsiSignal(rvi - prevRvi1, prevRvi1 - prevRvi2, rvi, prevRvi1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rvi", rviList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rviList;
        stockData.IndicatorName = IndicatorName.RelativeVolatilityIndexV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Inertia Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateInertiaIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.LinearRegression,
        int length = 20)
    {
        List<Signal> signalsList = new();

        var rviList = CalculateRelativeVolatilityIndexV2(stockData).CustomValuesList;
        var inertiaList = GetMovingAverageList(stockData, maType, length, rviList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal inertiaIndicator = inertiaList.ElementAtOrDefault(i);
            decimal prevInertiaIndicator1 = i >= 1 ? inertiaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevInertiaIndicator2 = i >= 2 ? inertiaList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(inertiaIndicator - prevInertiaIndicator1, prevInertiaIndicator1 - prevInertiaIndicator2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Inertia", inertiaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = inertiaList;
        stockData.IndicatorName = IndicatorName.InertiaIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Internal Bar Strength Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateInternalBarStrengthIndicator(this StockData stockData, int length = 14, int smoothLength = 3)
    {
        List<decimal> ibsList = new();
        List<decimal> ibsiList = new();
        List<decimal> ibsEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);

            decimal ibs = high - low != 0 ? (close - low) / (high - low) * 100 : 0;
            ibsList.Add(ibs);

            decimal prevIbsi = ibsiList.LastOrDefault();
            decimal ibsi = ibsList.TakeLastExt(length).Average();
            ibsiList.Add(ibsi);

            decimal prevIbsiEma = ibsEmaList.LastOrDefault();
            decimal ibsiEma = CalculateEMA(ibsi, prevIbsiEma, smoothLength);
            ibsEmaList.Add(ibsiEma);

            var signal = GetRsiSignal(ibsi - ibsiEma, prevIbsi - prevIbsiEma, ibsi, prevIbsi, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ibs", ibsiList },
            { "Signal", ibsEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ibsiList;
        stockData.IndicatorName = IndicatorName.InternalBarStrengthIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Inverse Fisher Fast Z Score
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateInverseFisherFastZScore(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 50)
    {
        List<decimal> ifzList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = smaList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        stockData.CustomValuesList = smaList;
        var linreg1List = CalculateLinearRegression(stockData, length).CustomValuesList;
        stockData.CustomValuesList = smaList;
        var linreg2List = CalculateLinearRegression(stockData, length1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal linreg1 = linreg1List.ElementAtOrDefault(i);
            decimal linreg2 = linreg2List.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal fz = stdDev != 0 ? (linreg2 - linreg1) / stdDev / 2 : 0;
            decimal prevIfz1 = i >= 1 ? ifzList.ElementAtOrDefault(i - 1) : 0;
            decimal prevIfz2 = i >= 2 ? ifzList.ElementAtOrDefault(i - 2) : 0;

            decimal ifz = Exp(10 * fz) + 1 != 0 ? (Exp(10 * fz) - 1) / (Exp(10 * fz) + 1) : 0;
            ifzList.Add(ifz);

            var signal = GetCompareSignal(ifz - prevIfz1, prevIfz1 - prevIfz2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Iffzs", ifzList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ifzList;
        stockData.IndicatorName = IndicatorName.InverseFisherFastZScore;

        return stockData;
    }

    /// <summary>
    /// Calculates the Inverse Fisher Z Score
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateInverseFisherZScore(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100)
    {
        List<decimal> fList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal prevF1 = i >= 1 ? fList.ElementAtOrDefault(i - 1) : 0;
            decimal prevF2 = i >= 2 ? fList.ElementAtOrDefault(i - 2) : 0;
            decimal z = stdDev != 0 ? (currentValue - sma) / stdDev : 0;
            decimal expZ = Exp(2 * z);

            decimal f = expZ + 1 != 0 ? MinOrMax((((expZ - 1) / (expZ + 1)) + 1) * 50, 100, 0) : 0;
            fList.Add(f);

            var signal = GetRsiSignal(f - prevF1, prevF1 - prevF2, f, prevF1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ifzs", fList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fList;
        stockData.IndicatorName = IndicatorName.InverseFisherZScore;

        return stockData;
    }

    /// <summary>
    /// Calculates the Insync Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <param name="emoLength"></param>
    /// <param name="mfiLength"></param>
    /// <param name="bbLength"></param>
    /// <param name="cciLength"></param>
    /// <param name="dpoLength"></param>
    /// <param name="rocLength"></param>
    /// <param name="rsiLength"></param>
    /// <param name="stochLength"></param>
    /// <param name="stochKLength"></param>
    /// <param name="stochDLength"></param>
    /// <param name="smaLength"></param>
    /// <param name="stdDevMult"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static StockData CalculateInsyncIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 12, int slowLength = 26, int signalLength = 9, int emoLength = 14, int mfiLength = 20, int bbLength = 20,
        int cciLength = 14, int dpoLength = 18, int rocLength = 10, int rsiLength = 14, int stochLength = 14, int stochKLength = 1,
        int stochDLength = 3, int smaLength = 10, decimal stdDevMult = 2, decimal divisor = 10000)
    {
        List<decimal> iidxList = new();
        List<decimal> tempMacdList = new();
        List<decimal> tempDpoList = new();
        List<decimal> tempRocList = new();
        List<decimal> pdoinsbList = new();
        List<decimal> pdoinssList = new();
        List<decimal> emoList = new();
        List<decimal> emoSmaList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, length: rsiLength).CustomValuesList;
        var cciList = CalculateCommodityChannelIndex(stockData, length: cciLength).CustomValuesList;
        var mfiList = CalculateMoneyFlowIndex(stockData, length: mfiLength).CustomValuesList;
        var macdList = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength,
            signalLength: signalLength).CustomValuesList;
        var bbIndicatorList = CalculateBollingerBands(stockData, stdDevMult: stdDevMult, length: bbLength).CustomValuesList;
        var dpoList = CalculateDetrendedPriceOscillator(stockData, length: dpoLength).CustomValuesList;
        var rocList = CalculateRateOfChange(stockData, length: rocLength).CustomValuesList;
        var stochasticList = CalculateStochasticOscillator(stockData, length: stochLength, kLength: stochKLength, dLength: stochDLength);
        var stochKList = stochasticList.OutputValues["FastD"];
        var stochDList = stochasticList.OutputValues["SlowD"];
        var emvList = CalculateEaseOfMovement(stockData, length: emoLength, divisor: divisor).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bolins2 = bbIndicatorList.ElementAtOrDefault(i);
            decimal prevPdoinss10 = i >= smaLength ? pdoinssList.ElementAtOrDefault(i - smaLength) : 0;
            decimal prevPdoinsb10 = i >= smaLength ? pdoinsbList.ElementAtOrDefault(i - smaLength) : 0;
            decimal cci = cciList.ElementAtOrDefault(i);
            decimal mfi = mfiList.ElementAtOrDefault(i);
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal stochD = stochDList.ElementAtOrDefault(i);
            decimal stochK = stochKList.ElementAtOrDefault(i);
            decimal prevIidx1 = i >= 1 ? iidxList.ElementAtOrDefault(i - 1) : 0;
            decimal prevIidx2 = i >= 2 ? iidxList.ElementAtOrDefault(i - 2) : 0;
            decimal bolinsll = bolins2 < 0.05m ? -5 : bolins2 > 0.95m ? 5 : 0;
            decimal cciins = cci > 100 ? 5 : cci < -100 ? -5 : 0;

            decimal emo = emvList.ElementAtOrDefault(i);
            emoList.Add(emo);

            decimal emoSma = emoList.TakeLastExt(smaLength).Average();
            emoSmaList.Add(emoSma);

            decimal emvins2 = emo - emoSma;
            decimal emvinsb = emvins2 < 0 ? emoSma < 0 ? -5 : 0 : emoSma > 0 ? 5 : 0;

            decimal macd = macdList.ElementAtOrDefault(i);
            tempMacdList.Add(macd);

            decimal macdSma = tempMacdList.TakeLastExt(smaLength).Average();
            decimal macdins2 = macd - macdSma;
            decimal macdinsb = macdins2 < 0 ? macdSma < 0 ? -5 : 0 : macdSma > 0 ? 5 : 0;
            decimal mfiins = mfi > 80 ? 5 : mfi < 20 ? -5 : 0;

            decimal dpo = dpoList.ElementAtOrDefault(i);
            tempDpoList.Add(dpo);

            decimal dpoSma = tempDpoList.TakeLastExt(smaLength).Average();
            decimal pdoins2 = dpo - dpoSma;
            decimal pdoinsb = pdoins2 < 0 ? dpoSma < 0 ? -5 : 0 : dpoSma > 0 ? 5 : 0;
            pdoinsbList.Add(pdoinsb);

            decimal pdoinss = pdoins2 > 0 ? dpoSma > 0 ? 5 : 0 : dpoSma < 0 ? -5 : 0;
            pdoinssList.Add(pdoinss);

            decimal roc = rocList.ElementAtOrDefault(i);
            tempRocList.Add(roc);

            decimal rocSma = tempRocList.TakeLastExt(smaLength).Average();
            decimal rocins2 = roc - rocSma;
            decimal rocinsb = rocins2 < 0 ? rocSma < 0 ? -5 : 0 : rocSma > 0 ? 5 : 0;
            decimal rsiins = rsi > 70 ? 5 : rsi < 30 ? -5 : 0;
            decimal stopdins = stochD > 80 ? 5 : stochD < 20 ? -5 : 0;
            decimal stopkins = stochK > 80 ? 5 : stochK < 20 ? -5 : 0;

            decimal iidx = 50 + cciins + bolinsll + rsiins + stopkins + stopdins + mfiins + emvinsb + rocinsb + prevPdoinss10 + prevPdoinsb10 + macdinsb;
            iidxList.Add(iidx);

            var signal = GetRsiSignal(iidx - prevIidx1, prevIidx1 - prevIidx2, iidx, prevIidx1, 95, 5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Iidx", iidxList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = iidxList;
        stockData.IndicatorName = IndicatorName.InsyncIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ease Of Movement
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static StockData CalculateEaseOfMovement(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14,
        decimal divisor = 1000000)
    {
        List<decimal> halfRangeList = new();
        List<decimal> midpointMoveList = new();
        List<decimal> emvList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevHalfRange = halfRangeList.LastOrDefault();
            decimal halfRange = (currentHigh - currentLow) * 0.5m;
            decimal boxRatio = currentHigh - currentLow != 0 ? currentVolume / (currentHigh - currentLow) : 0;

            decimal prevMidpointMove = midpointMoveList.LastOrDefault();
            decimal midpointMove = halfRange - prevHalfRange;
            midpointMoveList.Add(midpointMove);

            decimal emv = boxRatio != 0 ? divisor * ((midpointMove - prevMidpointMove) / boxRatio) : 0;
            emvList.Add(emv);
        }

        var emvSmaList = GetMovingAverageList(stockData, maType, length, emvList);
        var emvSignalList = GetMovingAverageList(stockData, maType, length, emvSmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal emv = emvList.ElementAtOrDefault(i);
            decimal emvSignal = emvSignalList.ElementAtOrDefault(i);
            decimal prevEmv = i >= 1 ? emvList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEmvSignal = i >= 1 ? emvSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(emv - emvSignal, prevEmv - prevEmvSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eom", emvList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emvList;
        stockData.IndicatorName = IndicatorName.EaseOfMovement;

        return stockData;
    }

    /// <summary>
    /// Calculates the Detrended Price Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDetrendedPriceOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20)
    {
        List<decimal> dpoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int prevPeriods = MinOrMax((int)Math.Ceiling(((decimal)length / 2) + 1));

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentSma = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= prevPeriods ? inputList.ElementAtOrDefault(i - prevPeriods) : 0;

            decimal prevDpo = dpoList.LastOrDefault();
            decimal dpo = prevValue - currentSma;
            dpoList.Add(dpo);

            var signal = GetCompareSignal(dpo, prevDpo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dpo", dpoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dpoList;
        stockData.IndicatorName = IndicatorName.DetrendedPriceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ocean Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOceanIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<decimal> lnList = new();
        List<decimal> oiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevLn = i >= length ? lnList.ElementAtOrDefault(i - length) : 0;

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.Add(ln);

            decimal oi = (ln - prevLn) / Sqrt(length) * 100;
            oiList.Add(oi);
        }

        var oiEmaList = GetMovingAverageList(stockData, maType, length, oiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal oiEma = oiEmaList.ElementAtOrDefault(i);
            decimal prevOiEma1 = i >= 1 ? oiEmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOiEma2 = i >= 2 ? oiEmaList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(oiEma - prevOiEma1, prevOiEma1 - prevOiEma2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Oi", oiList },
            { "Signal", oiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oiList;
        stockData.IndicatorName = IndicatorName.OceanIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Omega Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateOmegaRatio(this StockData stockData, int length = 30, decimal bmk = 0.05m)
    {
        List<decimal> omegaList = new();
        List<decimal> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal barMin = 60 * 24;
        decimal minPerYr = 60 * 24 * 30 * 12;
        decimal barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal bench = Pow(1 + bmk, length / (double)barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);

            decimal downSide = 0;
            decimal upSide = 0;
            for (int j = i - length + 1; j <= i && j >= 0; j++)
            {
                decimal iValue = retList.ElementAtOrDefault(j);

                downSide += iValue < bench ? bench - iValue : 0;
                upSide += iValue > bench ? iValue - bench : 0;
            }

            decimal prevOmega = omegaList.LastOrDefault();
            decimal omega = downSide != 0 ? upSide / downSide : 0;
            omegaList.Add(omega);

            var signal = GetCompareSignal(omega - 5, prevOmega - 5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Or", omegaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = omegaList;
        stockData.IndicatorName = IndicatorName.OmegaRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the On Balance Volume Modified
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateOnBalanceVolumeModified(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 7, int length2 = 10)
    {
        List<Signal> signalsList = new();

        var obvList = CalculateOnBalanceVolume(stockData, maType, length1).CustomValuesList;
        var obvmList = GetMovingAverageList(stockData, maType, length1, obvList);
        var sigList = GetMovingAverageList(stockData, maType, length2, obvmList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal obvm = obvmList.ElementAtOrDefault(i);
            decimal sig = sigList.ElementAtOrDefault(i);
            decimal prevObvm = i >= 1 ? obvmList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSig = i >= 1 ? sigList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(obvm - sig, prevObvm - prevSig);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Obvm", obvmList },
            { "Signal", sigList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = obvmList;
        stockData.IndicatorName = IndicatorName.OnBalanceVolumeModified;

        return stockData;
    }

    /// <summary>
    /// Calculates the On Balance Volume Reflex
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateOnBalanceVolumeReflex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 4, int signalLength = 14)
    {
        List<decimal> ovrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal prevOvr = ovrList.LastOrDefault();
            decimal ovr = currentValue > prevValue ? prevOvr + currentVolume : currentValue < prevValue ? prevOvr - currentVolume : prevOvr;
            ovrList.Add(ovr);
        }

        var ovrSmaList = GetMovingAverageList(stockData, maType, signalLength, ovrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ovr = ovrList.ElementAtOrDefault(i);
            decimal ovrEma = ovrSmaList.ElementAtOrDefault(i);
            decimal prevOvr = i >= 1 ? ovrList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOvrEma = i >= 1 ? ovrSmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(ovr - ovrEma, prevOvr - prevOvrEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Obvr", ovrList },
            { "Signal", ovrSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ovrList;
        stockData.IndicatorName = IndicatorName.OnBalanceVolumeReflex;

        return stockData;
    }

    /// <summary>
    /// Calculates the On Balance Volume Disparity Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    /// <returns></returns>
    public static StockData CalculateOnBalanceVolumeDisparityIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 33, int signalLength = 4, decimal top = 1.1m, decimal bottom = 0.9m)
    {
        List<decimal> obvdiList = new();
        List<decimal> bscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var obvList = CalculateOnBalanceVolume(stockData, maType, length).CustomValuesList;
        var obvSmaList = GetMovingAverageList(stockData, maType, length, obvList);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        stockData.CustomValuesList = obvList;
        var obvStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal obvSma = obvSmaList.ElementAtOrDefault(i);
            decimal obvStdDev = obvStdDevList.ElementAtOrDefault(i);
            decimal aTop = currentValue - (sma - (2 * stdDev));
            decimal aBot = currentValue + (2 * stdDev) - (sma - (2 * stdDev));
            decimal obv = obvList.ElementAtOrDefault(i);
            decimal a = aBot != 0 ? aTop / aBot : 0;
            decimal bTop = obv - (obvSma - (2 * obvStdDev));
            decimal bBot = obvSma + (2 * obvStdDev) - (obvSma - (2 * obvStdDev));
            decimal b = bBot != 0 ? bTop / bBot : 0;

            decimal obvdi = 1 + b != 0 ? (1 + a) / (1 + b) : 0;
            obvdiList.Add(obvdi);
        }

        var obvdiEmaList = GetMovingAverageList(stockData, maType, signalLength, obvdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal obvdi = obvdiList.ElementAtOrDefault(i);
            decimal obvdiEma = obvdiEmaList.ElementAtOrDefault(i);
            decimal prevObvdi = i >= 1 ? obvdiList.ElementAtOrDefault(i - 1) : 0;

            decimal prevBsc = bscList.LastOrDefault();
            decimal bsc = (prevObvdi < bottom && obvdi > bottom) || obvdi > obvdiEma ? 1 : (prevObvdi > top && obvdi < top) ||
                obvdi < bottom ? -1 : prevBsc;
            bscList.Add(bsc);

            var signal = GetCompareSignal(bsc, prevBsc);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Obvdi", obvdiList },
            { "Signal", obvdiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = obvdiList;
        stockData.IndicatorName = IndicatorName.OnBalanceVolumeDisparityIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Oscar Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOscarIndicator(this StockData stockData, int length = 8)
    {
        List<decimal> oscarList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal rough = highest - lowest != 0 ? MinOrMax((currentValue - lowest) / (highest - lowest) * 100, 100, 0) : 0;
            decimal prevOscar1 = i >= 1 ? oscarList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOscar2 = i >= 2 ? oscarList.ElementAtOrDefault(i - 2) : 0;

            decimal oscar = (prevOscar1 / 6) + (rough / 3);
            oscarList.Add(oscar);

            var signal = GetRsiSignal(oscar - prevOscar1, prevOscar1 - prevOscar2, oscar, prevOscar1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Oscar", oscarList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oscarList;
        stockData.IndicatorName = IndicatorName.OscarIndicator;

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ma = maList.ElementAtOrDefault(i);
            decimal fark = ma * percent * 0.01m;

            decimal prevLongStop = longStopList.LastOrDefault();
            decimal longStop = ma - fark;
            longStop = ma > prevLongStop ? Math.Max(longStop, prevLongStop) : longStop;
            longStopList.Add(longStop);

            decimal prevShortStop = shortStopList.LastOrDefault();
            decimal shortStop = ma + fark;
            shortStopList.Add(shortStop);

            decimal prevOtt = ottList.LastOrDefault();
            decimal mt = ma > prevShortStop ? longStop : ma < prevLongStop ? shortStop : 0;
            decimal ott = ma > mt ? mt * (200 + percent) / 200 : mt * (200 - percent) / 200;
            ottList.Add(ott);

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
    /// Calculates the OC Histogram
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOCHistogram(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 10)
    {
        List<decimal> ocHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        var openEmaList = GetMovingAverageList(stockData, maType, length, openList);
        var closeEmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentCloseEma = closeEmaList.ElementAtOrDefault(i);
            decimal currentOpenEma = openEmaList.ElementAtOrDefault(i);

            decimal prevOcHistogram = ocHistogramList.LastOrDefault();
            decimal ocHistogram = currentCloseEma - currentOpenEma;
            ocHistogramList.Add(ocHistogram);

            var signal = GetCompareSignal(ocHistogram, prevOcHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "OcHistogram", ocHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ocHistogramList;
        stockData.IndicatorName = IndicatorName.OCHistogram;

        return stockData;
    }

    /// <summary>
    /// Calculates the Osc Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateOscOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 7, int slowLength = 14)
    {
        List<decimal> oscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastSma = fastSmaList.ElementAtOrDefault(i);
            decimal slowSma = slowSmaList.ElementAtOrDefault(i);
            decimal prevOsc1 = i >= 1 ? oscList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOsc2 = i >= 2 ? oscList.ElementAtOrDefault(i - 2) : 0;

            decimal osc = slowSma - fastSma;
            oscList.Add(osc);

            var signal = GetCompareSignal(osc - prevOsc1, prevOsc1 - prevOsc2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "OscOscillator", oscList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oscList;
        stockData.IndicatorName = IndicatorName.OscOscillator;

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal diff = currentValue - prevValue;

            decimal negChg = diff < 0 ? Math.Abs(diff) : 0;
            cmoNegChgList.Add(negChg);

            decimal posChg = diff > 0 ? diff : 0;
            cmoPosChgList.Add(posChg);

            decimal negSum = cmoNegChgList.TakeLastExt(length).Sum();
            decimal posSum = cmoPosChgList.TakeLastExt(length).Sum();

            decimal cmo = posSum + negSum != 0 ? MinOrMax((posSum - negSum) / (posSum + negSum) * 100, 100, -100) : 0;
            cmoList.Add(cmo);
        }

        var cmoSignalList = GetMovingAverageList(stockData, maType, signalLength, cmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal cmo = cmoList.ElementAtOrDefault(i);
            decimal cmoSignal = cmoSignalList.ElementAtOrDefault(i);
            decimal prevCmo = i >= 1 ? cmoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCmoSignal = i >= 1 ? cmoSignalList.ElementAtOrDefault(i - 1) : 0;

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

    /// <summary>
    /// Calculates the Natural Directional Combo
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalDirectionalCombo(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 40, int smoothLength = 20)
    {
        List<decimal> nxcList = new();
        List<Signal> signalsList = new();

        var ndxList = CalculateNaturalDirectionalIndex(stockData, maType, length, smoothLength).CustomValuesList;
        var nstList = CalculateNaturalStochasticIndicator(stockData, maType, length, smoothLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ndx = ndxList.ElementAtOrDefault(i);
            decimal nst = nstList.ElementAtOrDefault(i);
            decimal prevNxc1 = i >= 1 ? nxcList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNxc2 = i >= 2 ? nxcList.ElementAtOrDefault(i - 2) : 0;
            decimal v3 = Math.Sign(ndx) != Math.Sign(nst) ? ndx * nst : ((Math.Abs(ndx) * nst) + (Math.Abs(nst) * ndx)) / 2;

            decimal nxc = Math.Sign(v3) * Sqrt(Math.Abs(v3));
            nxcList.Add(nxc);

            var signal = GetCompareSignal(nxc - prevNxc1, prevNxc1 - prevNxc2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nxc", nxcList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nxcList;
        stockData.IndicatorName = IndicatorName.NaturalDirectionalCombo;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Directional Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalDirectionalIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 40, int smoothLength = 20)
    {
        List<decimal> lnList = new();
        List<decimal> rawNdxList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.Add(ln);

            decimal weightSum = 0, denomSum = 0, absSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal prevLn = i >= j + 1 ? lnList.ElementAtOrDefault(i - (j + 1)) : 0;
                decimal currLn = i >= j ? lnList.ElementAtOrDefault(i - j) : 0;
                decimal diff = prevLn - currLn;
                absSum += Math.Abs(diff);
                decimal frac = absSum != 0 ? (ln - currLn) / absSum : 0;
                decimal ratio = 1 / Sqrt(j + 1);
                weightSum += frac * ratio;
                denomSum += ratio;
            }

            decimal rawNdx = denomSum != 0 ? weightSum / denomSum * 100 : 0;
            rawNdxList.Add(rawNdx);
        }

        var ndxList = GetMovingAverageList(stockData, maType, smoothLength, rawNdxList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ndx = ndxList.ElementAtOrDefault(i);
            decimal prevNdx1 = i >= 1 ? ndxList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNdx2 = i >= 2 ? ndxList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(ndx - prevNdx1, prevNdx1 - prevNdx2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ndx", ndxList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ndxList;
        stockData.IndicatorName = IndicatorName.NaturalDirectionalIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Stochastic Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalStochasticIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 20, int smoothLength = 10)
    {
        List<decimal> rawNstList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal weightSum = 0, denomSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal hh = i >= j ? highestList.ElementAtOrDefault(i - j) : 0;
                decimal ll = i >= j ? lowestList.ElementAtOrDefault(i - j) : 0;
                decimal c = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                decimal range = hh - ll;
                decimal frac = range != 0 ? (c - ll) / range : 0;
                decimal ratio = 1 / Sqrt(j + 1);
                weightSum += frac * ratio;
                denomSum += ratio;
            }

            decimal rawNst = denomSum != 0 ? (200 * weightSum / denomSum) - 100 : 0;
            rawNstList.Add(rawNst);
        }

        var nstList = GetMovingAverageList(stockData, maType, smoothLength, rawNstList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nst = nstList.ElementAtOrDefault(i);
            decimal prevNst1 = i >= 1 ? nstList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNst2 = i >= 2 ? nstList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(nst - prevNst1, prevNst1 - prevNst2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nst", nstList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nstList;
        stockData.IndicatorName = IndicatorName.NaturalStochasticIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Market Mirror
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalMarketMirror(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 40)
    {
        List<decimal> lnList = new();
        List<decimal> oiAvgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.Add(ln);

            decimal oiSum = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevLn = i >= j ? lnList.ElementAtOrDefault(i - j) : 0;
                oiSum += (ln - prevLn) / Sqrt(j) * 100;
            }

            decimal oiAvg = oiSum / length;
            oiAvgList.Add(oiAvg);
        }

        var nmmList = GetMovingAverageList(stockData, maType, length, oiAvgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nmm = nmmList.ElementAtOrDefault(i);
            decimal prevNmm1 = i >= 1 ? nmmList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNmm2 = i >= 2 ? nmmList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(nmm - prevNmm1, prevNmm1 - prevNmm2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nmm", nmmList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nmmList;
        stockData.IndicatorName = IndicatorName.NaturalMarketMirror;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Market River
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalMarketRiver(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 40)
    {
        List<decimal> lnList = new();
        List<decimal> oiSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.Add(ln);

            decimal oiSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal currentLn = i >= j ? lnList.ElementAtOrDefault(i - j) : 0;
                decimal prevLn = i >= j + 1 ? lnList.ElementAtOrDefault(i - (j + 1)) : 0;

                oiSum += (prevLn - currentLn) * (Sqrt(j) - Sqrt(j + 1));
            }
            oiSumList.Add(oiSum);
        }

        var nmrList = GetMovingAverageList(stockData, maType, length, oiSumList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nmr = nmrList.ElementAtOrDefault(i);
            decimal prevNmr1 = i >= 1 ? nmrList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNmr2 = i >= 2 ? nmrList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(nmr - prevNmr1, prevNmr1 - prevNmr2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nmr", nmrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nmrList;
        stockData.IndicatorName = IndicatorName.NaturalMarketRiver;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Market Combo
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalMarketCombo(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 40, int smoothLength = 20)
    {
        List<decimal> nmcList = new();
        List<Signal> signalsList = new();

        var nmrList = CalculateNaturalMarketRiver(stockData, maType, length).CustomValuesList;
        var nmmList = CalculateNaturalMarketMirror(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nmr = nmrList.ElementAtOrDefault(i);
            decimal nmm = nmmList.ElementAtOrDefault(i);
            decimal v3 = Math.Sign(nmm) != Math.Sign(nmr) ? nmm * nmr : ((Math.Abs(nmm) * nmr) + (Math.Abs(nmr) * nmm)) / 2;

            decimal nmc = Math.Sign(v3) * Sqrt(Math.Abs(v3));
            nmcList.Add(nmc);
        }

        var nmcMaList = GetMovingAverageList(stockData, maType, smoothLength, nmcList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nmc = nmcMaList.ElementAtOrDefault(i);
            decimal prevNmc1 = i >= 1 ? nmcMaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNmc2 = i >= 2 ? nmcMaList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(nmc - prevNmc1, prevNmc1 - prevNmc2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nmc", nmcList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nmcList;
        stockData.IndicatorName = IndicatorName.NaturalMarketCombo;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Market Slope
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalMarketSlope(this StockData stockData, int length = 40)
    {
        List<decimal> lnList = new();
        List<decimal> nmsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.Add(ln);
        }

        stockData.CustomValuesList = lnList;
        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal linReg = linRegList.ElementAtOrDefault(i);
            decimal prevLinReg = i >= 1 ? linRegList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNms1 = i >= 1 ? nmsList.ElementAtOrDefault(i - 1) : 0;
            decimal prevNms2 = i >= 2 ? nmsList.ElementAtOrDefault(i - 2) : 0;

            decimal nms = (linReg - prevLinReg) * Log(length);
            nmsList.Add(nms);

            var signal = GetCompareSignal(nms - prevNms1, prevNms1 - prevNms2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nms", nmsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nmsList;
        stockData.IndicatorName = IndicatorName.NaturalMarketSlope;

        return stockData;
    }

    /// <summary>
    /// Calculates the Narrow Bandpass Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNarrowBandpassFilter(this StockData stockData, int length = 50)
    {
        List<decimal> sumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevSum1 = i >= 1 ? sumList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSum2 = i >= 2 ? sumList.ElementAtOrDefault(i - 2) : 0;

            decimal sum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                decimal x = j / (decimal)(length - 1);
                decimal win = 0.42m - (0.5m * Cos(2 * Pi * x)) + (0.08m * Cos(4 * Pi * x));
                decimal w = Sin(2 * Pi * j / length) * win;
                sum += prevValue * w;
            }
            sumList.Add(sum);

            var signal = GetCompareSignal(sum - prevSum1, prevSum1 - prevSum2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nbpf", sumList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sumList;
        stockData.IndicatorName = IndicatorName.NarrowBandpassFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Nth Order Differencing Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <returns></returns>
    public static StockData CalculateNthOrderDifferencingOscillator(this StockData stockData, int length = 14, int lbLength = 2)
    {
        List<decimal> nodoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal sum = 0, w = 1;
            for (int j = 0; j <= lbLength; j++)
            {
                decimal prevValue = i >= length * (j + 1) ? inputList.ElementAtOrDefault(i - (length * (j + 1))) : 0;
                decimal x = Math.Sign(((j + 1) % 2) - 0.5);
                w *= (lbLength - j) / (decimal)(j + 1);
                sum += prevValue * w * x;
            }

            decimal prevNodo = nodoList.LastOrDefault();
            decimal nodo = currentValue - sum;
            nodoList.Add(nodo);

            var signal = GetCompareSignal(nodo, prevNodo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nodo", nodoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nodoList;
        stockData.IndicatorName = IndicatorName.NthOrderDifferencingOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Negative Volume Disparity Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    /// <returns></returns>
    public static StockData CalculateNegativeVolumeDisparityIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 33, int signalLength = 4, decimal top = 1.1m, decimal bottom = 0.9m)
    {
        List<decimal> nvdiList = new();
        List<decimal> bscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var nviList = CalculateNegativeVolumeIndex(stockData, maType, length).CustomValuesList;
        var nviSmaList = GetMovingAverageList(stockData, maType, length, nviList);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        stockData.CustomValuesList = nviList;
        var nviStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal nviSma = nviSmaList.ElementAtOrDefault(i);
            decimal nviStdDev = nviStdDevList.ElementAtOrDefault(i);
            decimal aTop = currentValue - (sma - (2 * stdDev));
            decimal aBot = (currentValue + (2 * stdDev)) - (sma - (2 * stdDev));
            decimal nvi = nviList.ElementAtOrDefault(i);
            decimal a = aBot != 0 ? aTop / aBot : 0;
            decimal bTop = nvi - (nviSma - (2 * nviStdDev));
            decimal bBot = (nviSma + (2 * nviStdDev)) - (nviSma - (2 * nviStdDev));
            decimal b = bBot != 0 ? bTop / bBot : 0;

            decimal nvdi = 1 + b != 0 ? (1 + a) / (1 + b) : 0;
            nvdiList.Add(nvdi);
        }

        var nvdiEmaList = GetMovingAverageList(stockData, maType, signalLength, nvdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nvdi = nvdiList.ElementAtOrDefault(i);
            decimal nvdiEma = nvdiEmaList.ElementAtOrDefault(i);
            decimal prevNvdi = i >= 1 ? nvdiList.ElementAtOrDefault(i - 1) : 0;

            decimal prevBsc = bscList.LastOrDefault();
            decimal bsc = (prevNvdi < bottom && nvdi > bottom) || nvdi > nvdiEma ? 1 : (prevNvdi > top && nvdi < top) ||
                nvdi < bottom ? -1 : prevBsc;
            bscList.AddRounded(bsc);

            var signal = GetCompareSignal(bsc, prevBsc);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nvdi", nvdiList },
            { "Signal", nvdiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nvdiList;
        stockData.IndicatorName = IndicatorName.NegativeVolumeDisparityIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Normalized Relative Vigor Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNormalizedRelativeVigorIndex(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SymmetricallyWeightedMovingAverage, int length = 10)
    {
        List<decimal> closeOpenList = new();
        List<decimal> highLowList = new();
        List<decimal> tempCloseOpenList = new();
        List<decimal> tempHighLowList = new();
        List<decimal> swmaCloseOpenSumList = new();
        List<decimal> swmaHighLowSumList = new();
        List<decimal> rvgiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal closeOpen = currentClose - currentOpen;
            closeOpenList.Add(closeOpen);

            decimal highLow = currentHigh - currentLow;
            highLowList.Add(highLow);
        }

        var swmaCloseOpenList = GetMovingAverageList(stockData, maType, length, closeOpenList);
        var swmaHighLowList = GetMovingAverageList(stockData, maType, length, highLowList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal swmaCloseOpen = swmaCloseOpenList.ElementAtOrDefault(i);
            tempCloseOpenList.Add(swmaCloseOpen);

            decimal closeOpenSum = tempCloseOpenList.TakeLastExt(length).Sum();
            swmaCloseOpenSumList.Add(closeOpenSum);

            decimal swmaHighLow = swmaHighLowList.ElementAtOrDefault(i);
            tempHighLowList.Add(swmaHighLow);

            decimal highLowSum = tempHighLowList.TakeLastExt(length).Sum();
            swmaHighLowSumList.Add(highLowSum);

            decimal rvgi = highLowSum != 0 ? closeOpenSum / highLowSum * 100 : 0;
            rvgiList.Add(rvgi);
        }

        var rvgiSignalList = GetMovingAverageList(stockData, maType, length, rvgiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rvgi = rvgiList.ElementAtOrDefault(i);
            decimal rvgiSig = rvgiSignalList.ElementAtOrDefault(i);
            decimal prevRvgi = i >= 1 ? rvgiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRvgiSig = i >= 1 ? rvgiSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(rvgi - rvgiSig, prevRvgi - prevRvgiSig);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nrvi", rvgiList },
            { "Signal", rvgiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rvgiList;
        stockData.IndicatorName = IndicatorName.NormalizedRelativeVigorIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Gann Swing Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGannSwingOscillator(this StockData stockData, int length = 5)
    {
        List<decimal> gannSwingOscillatorList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal prevHighest1 = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowest1 = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHighest2 = i >= 2 ? highestList.ElementAtOrDefault(i - 2) : 0;
            decimal prevLowest2 = i >= 2 ? lowestList.ElementAtOrDefault(i - 2) : 0;

            decimal prevGso = gannSwingOscillatorList.LastOrDefault();
            decimal gso = prevHighest2 > prevHighest1 && highestHigh > prevHighest1 ? 1 :
                prevLowest2 < prevLowest1 && lowestLow < prevLowest1 ? -1 : prevGso;
            gannSwingOscillatorList.Add(gso);

            var signal = GetCompareSignal(gso, prevGso);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gso", gannSwingOscillatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gannSwingOscillatorList;
        stockData.IndicatorName = IndicatorName.GannSwingOscillator;

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
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal prevHighest1 = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowest1 = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHighest2 = i >= 2 ? highestList.ElementAtOrDefault(i - 2) : 0;
            decimal prevLowest2 = i >= 2 ? lowestList.ElementAtOrDefault(i - 2) : 0;

            decimal prevGto = gannTrendOscillatorList.LastOrDefault();
            decimal gto = prevHighest2 > prevHighest1 && highestHigh > prevHighest1 ? 1 : prevLowest2 < prevLowest1 && lowestLow < prevLowest1 ? -1 : prevGto;
            gannTrendOscillatorList.Add(gto);

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
    /// Calculates the Gann HiLo Activator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGannHiLoActivator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 3)
    {
        List<decimal> ghlaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var highMaList = GetMovingAverageList(stockData, maType, length, highList);
        var lowMaList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highMa = highMaList.ElementAtOrDefault(i);
            decimal lowMa = lowMaList.ElementAtOrDefault(i);
            decimal prevHighMa = i >= 1 ? highMaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowMa = i >= 1 ? lowMaList.ElementAtOrDefault(i - 1) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevGhla = ghlaList.LastOrDefault();
            decimal ghla = currentValue > prevHighMa ? lowMa : currentValue < prevLowMa ? highMa : prevGhla;
            ghlaList.Add(ghla);

            var signal = GetCompareSignal(currentValue - ghla, prevValue - prevGhla);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ghla", ghlaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ghlaList;
        stockData.IndicatorName = IndicatorName.GannHiLoActivator;

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevT = i >= length ? tList.ElementAtOrDefault(i - length) : currentValue;
            decimal priorT = i >= forecastLength ? tList.ElementAtOrDefault(i - forecastLength) : 0;
            decimal prevFcast = i >= forecastLength ? fcastList.ElementAtOrDefault(i - forecastLength) : 0;
            decimal prevChg = i >= length ? chgList.ElementAtOrDefault(i - length) : currentValue;

            decimal chg = 0.9m * prevT;
            chgList.Add(chg);

            decimal t = (0.9m * prevT) + (0.1m * currentValue) + (chg - prevChg);
            tList.Add(t);

            decimal trend = tList.TakeLastExt(length).Average();
            trendList.Add(trend);

            decimal fcast = t + (t - priorT);
            fcastList.Add(fcast);

            decimal diff = Math.Abs(currentValue - prevFcast);
            diffList.Add(diff);

            decimal diffSma = diffList.TakeLastExt(forecastLength).Average();
            decimal dev = diffSma * mult;

            decimal upper = fcast + dev;
            upperList.Add(upper);

            decimal lower = fcast - dev;
            lowerList.Add(lower);

            decimal prevBullSlope = bullSlopeList.LastOrDefault();
            decimal bullSlope = currentValue - Math.Max(fcast, Math.Max(t, trend));
            bullSlopeList.Add(bullSlope);

            decimal prevBearSlope = bearSlopeList.LastOrDefault();
            decimal bearSlope = currentValue - Math.Min(fcast, Math.Min(t, trend));
            bearSlopeList.Add(bearSlope);

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
    /// Calculates the Grover Llorens Cycle Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateGroverLlorensCycleOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 100, int smoothLength = 20, decimal mult = 10)
    {
        List<decimal> tsList = new();
        List<decimal> oscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal atr = atrList.ElementAtOrDefault(i);
            decimal prevTs = i >= 1 ? tsList.ElementAtOrDefault(i - 1) : currentValue;
            decimal diff = currentValue - prevTs;

            decimal ts = diff > 0 ? prevTs - (atr * mult) : diff < 0 ? prevTs + (atr * mult) : prevTs;
            tsList.AddRounded(ts);

            decimal osc = currentValue - ts;
            oscList.Add(osc);
        }

        var smoList = GetMovingAverageList(stockData, maType, smoothLength, oscList);
        stockData.CustomValuesList = smoList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: smoothLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal prevRsi1 = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRsi2 = i >= 2 ? rsiList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetRsiSignal(rsi - prevRsi1, prevRsi1 - prevRsi2, rsi, prevRsi1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Glco", rsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.GroverLlorensCycleOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Grover Llorens Activator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateGroverLlorensActivator(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 100, decimal mult = 5)
    {
        List<decimal> tsList = new();
        List<decimal> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal atr = atrList.ElementAtOrDefault(i);
            decimal prevTs = i >= 1 ? tsList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            prevTs = prevTs == 0 ? prevValue : prevTs;

            decimal prevDiff = diffList.LastOrDefault();
            decimal diff = currentValue - prevTs;
            diffList.Add(diff);

            decimal ts = diff > 0 ? prevTs - (atr * mult) : diff < 0 ? prevTs + (atr * mult) : prevTs;
            tsList.Add(ts);

            var signal = GetCompareSignal(diff, prevDiff);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gla", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.GroverLlorensActivator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Guppy Count Back Line
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGuppyCountBackLine(this StockData stockData, int length = 21)
    {
        List<decimal> cblList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal hh = highestList.ElementAtOrDefault(i);
            decimal ll = lowestList.ElementAtOrDefault(i);

            decimal prevCbl = cblList.LastOrDefault();
            int hCount = 0, lCount = 0;
            decimal cbl = currentValue;
            for (int j = 0; j <= length; j++)
            {
                decimal currentLow = i >= j ? lowList.ElementAtOrDefault(i - j) : 0;
                decimal currentHigh = i >= j ? highList.ElementAtOrDefault(i - j) : 0;

                if (currentLow == ll)
                {
                    for (int k = j + 1; k <= j + length; k++)
                    {
                        decimal prevHigh = i >= k ? highList.ElementAtOrDefault(i - k) : 0;
                        lCount += prevHigh > currentHigh ? 1 : 0;
                        if (lCount == 2)
                        {
                            cbl = prevHigh;
                            break;
                        }
                    }
                }

                if (currentHigh == hh)
                {
                    for (int k = j + 1; k <= j + length; k++)
                    {
                        decimal prevLow = i >= k ? lowList.ElementAtOrDefault(i - k) : 0;
                        hCount += prevLow > currentLow ? 1 : 0;
                        if (hCount == 2)
                        {
                            cbl = prevLow;
                            break;
                        }
                    }
                }
            }
            cblList.Add(cbl);

            var signal = GetCompareSignal(currentValue - cbl, prevValue - prevCbl);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cbl", cblList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cblList;
        stockData.IndicatorName = IndicatorName.GuppyCountBackLine;

        return stockData;
    }

    /// <summary>
    /// Calculates the Guppy Multiple Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="length6"></param>
    /// <param name="length7"></param>
    /// <param name="length8"></param>
    /// <param name="length9"></param>
    /// <param name="length10"></param>
    /// <param name="length11"></param>
    /// <param name="length12"></param>
    /// <param name="length13"></param>
    /// <param name="length14"></param>
    /// <param name="length15"></param>
    /// <param name="length16"></param>
    /// <param name="length17"></param>
    /// <param name="length18"></param>
    /// <param name="length19"></param>
    /// <param name="length20"></param>
    /// <param name="length21"></param>
    /// <param name="length22"></param>
    /// <param name="length23"></param>
    /// <param name="length24"></param>
    /// <param name="length25"></param>
    /// <param name="length26"></param>
    /// <param name="length27"></param>
    /// <param name="length28"></param>
    /// <param name="length29"></param>
    /// <param name="length30"></param>
    /// <param name="length31"></param>
    /// <param name="length32"></param>
    /// <param name="length33"></param>
    /// <param name="length34"></param>
    /// <param name="length35"></param>
    /// <param name="smoothLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateGuppyMultipleMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 3, int length2 = 5, int length3 = 7, int length4 = 8, int length5 = 9, int length6 = 10, int length7 = 11, int length8 = 12,
        int length9 = 13, int length10 = 15, int length11 = 17, int length12 = 19, int length13 = 21, int length14 = 23, int length15 = 25,
        int length16 = 28, int length17 = 30, int length18 = 31, int length19 = 34, int length20 = 35, int length21 = 37, int length22 = 40,
        int length23 = 43, int length24 = 45, int length25 = 46, int length26 = 49, int length27 = 50, int length28 = 52, int length29 = 55,
        int length30 = 58, int length31 = 60, int length32 = 61, int length33 = 64, int length34 = 67, int length35 = 70, int smoothLength = 1,
        int signalLength = 13)
    {
        List<decimal> superGmmaFastList = new();
        List<decimal> superGmmaSlowList = new();
        List<decimal> superGmmaOscRawList = new();
        List<decimal> superGmmaOscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema3List = GetMovingAverageList(stockData, maType, length1, inputList);
        var ema5List = GetMovingAverageList(stockData, maType, length2, inputList);
        var ema7List = GetMovingAverageList(stockData, maType, length3, inputList);
        var ema9List = GetMovingAverageList(stockData, maType, length5, inputList);
        var ema11List = GetMovingAverageList(stockData, maType, length7, inputList);
        var ema13List = GetMovingAverageList(stockData, maType, length9, inputList);
        var ema15List = GetMovingAverageList(stockData, maType, length10, inputList);
        var ema17List = GetMovingAverageList(stockData, maType, length11, inputList);
        var ema19List = GetMovingAverageList(stockData, maType, length12, inputList);
        var ema21List = GetMovingAverageList(stockData, maType, length13, inputList);
        var ema23List = GetMovingAverageList(stockData, maType, length14, inputList);
        var ema25List = GetMovingAverageList(stockData, maType, length15, inputList);
        var ema28List = GetMovingAverageList(stockData, maType, length16, inputList);
        var ema31List = GetMovingAverageList(stockData, maType, length18, inputList);
        var ema34List = GetMovingAverageList(stockData, maType, length19, inputList);
        var ema37List = GetMovingAverageList(stockData, maType, length21, inputList);
        var ema40List = GetMovingAverageList(stockData, maType, length22, inputList);
        var ema43List = GetMovingAverageList(stockData, maType, length23, inputList);
        var ema46List = GetMovingAverageList(stockData, maType, length25, inputList);
        var ema49List = GetMovingAverageList(stockData, maType, length26, inputList);
        var ema52List = GetMovingAverageList(stockData, maType, length28, inputList);
        var ema55List = GetMovingAverageList(stockData, maType, length29, inputList);
        var ema58List = GetMovingAverageList(stockData, maType, length30, inputList);
        var ema61List = GetMovingAverageList(stockData, maType, length32, inputList);
        var ema64List = GetMovingAverageList(stockData, maType, length33, inputList);
        var ema67List = GetMovingAverageList(stockData, maType, length34, inputList);
        var ema70List = GetMovingAverageList(stockData, maType, length35, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal emaF1 = ema3List.ElementAtOrDefault(i);
            decimal emaF2 = ema5List.ElementAtOrDefault(i);
            decimal emaF3 = ema7List.ElementAtOrDefault(i);
            decimal emaF4 = ema9List.ElementAtOrDefault(i);
            decimal emaF5 = ema11List.ElementAtOrDefault(i);
            decimal emaF6 = ema13List.ElementAtOrDefault(i);
            decimal emaF7 = ema15List.ElementAtOrDefault(i);
            decimal emaF8 = ema17List.ElementAtOrDefault(i);
            decimal emaF9 = ema19List.ElementAtOrDefault(i);
            decimal emaF10 = ema21List.ElementAtOrDefault(i);
            decimal emaF11 = ema23List.ElementAtOrDefault(i);
            decimal emaS1 = ema25List.ElementAtOrDefault(i);
            decimal emaS2 = ema28List.ElementAtOrDefault(i);
            decimal emaS3 = ema31List.ElementAtOrDefault(i);
            decimal emaS4 = ema34List.ElementAtOrDefault(i);
            decimal emaS5 = ema37List.ElementAtOrDefault(i);
            decimal emaS6 = ema40List.ElementAtOrDefault(i);
            decimal emaS7 = ema43List.ElementAtOrDefault(i);
            decimal emaS8 = ema46List.ElementAtOrDefault(i);
            decimal emaS9 = ema49List.ElementAtOrDefault(i);
            decimal emaS10 = ema52List.ElementAtOrDefault(i);
            decimal emaS11 = ema55List.ElementAtOrDefault(i);
            decimal emaS12 = ema58List.ElementAtOrDefault(i);
            decimal emaS13 = ema61List.ElementAtOrDefault(i);
            decimal emaS14 = ema64List.ElementAtOrDefault(i);
            decimal emaS15 = ema67List.ElementAtOrDefault(i);
            decimal emaS16 = ema70List.ElementAtOrDefault(i);

            decimal superGmmaFast = (emaF1 + emaF2 + emaF3 + emaF4 + emaF5 + emaF6 + emaF7 + emaF8 + emaF9 + emaF10 + emaF11) / 11;
            superGmmaFastList.Add(superGmmaFast);

            decimal superGmmaSlow = (emaS1 + emaS2 + emaS3 + emaS4 + emaS5 + emaS6 + emaS7 + emaS8 + emaS9 + emaS10 + emaS11 + emaS12 + emaS13 +
                emaS14 + emaS15 + emaS16) / 16;
            superGmmaSlowList.Add(superGmmaSlow);

            decimal superGmmaOscRaw = superGmmaSlow != 0 ? (superGmmaFast - superGmmaSlow) / superGmmaSlow * 100 : 0;
            superGmmaOscRawList.Add(superGmmaOscRaw);

            decimal superGmmaOsc = superGmmaOscRawList.TakeLastExt(smoothLength).Average();
            superGmmaOscList.Add(superGmmaOsc);
        }

        var superGmmaSignalList = GetMovingAverageList(stockData, maType, signalLength, superGmmaOscRawList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal superGmmaOsc = superGmmaOscList.ElementAtOrDefault(i);
            decimal superGmmaSignal = superGmmaSignalList.ElementAtOrDefault(i);
            decimal prevSuperGmmaOsc = i >= 1 ? superGmmaOscList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSuperGmmaSignal = i >= 1 ? superGmmaSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(superGmmaOsc - superGmmaSignal, prevSuperGmmaOsc - prevSuperGmmaSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "SuperGmmaOsc", superGmmaOscList },
            { "SuperGmmaSignal", superGmmaSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = superGmmaOscList;
        stockData.IndicatorName = IndicatorName.GuppyMultipleMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Guppy Distance Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="length6"></param>
    /// <param name="length7"></param>
    /// <param name="length8"></param>
    /// <param name="length9"></param>
    /// <param name="length10"></param>
    /// <param name="length11"></param>
    /// <param name="length12"></param>
    /// <returns></returns>
    public static StockData CalculateGuppyDistanceIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 3, int length2 = 5, int length3 = 8, int length4 = 10, int length5 = 12, int length6 = 15, int length7 = 30, int length8 = 35,
        int length9 = 40, int length10 = 45, int length11 = 11, int length12 = 60)
    {
        List<decimal> fastDistanceList = new();
        List<decimal> slowDistanceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema3List = GetMovingAverageList(stockData, maType, length1, inputList);
        var ema5List = GetMovingAverageList(stockData, maType, length2, inputList);
        var ema8List = GetMovingAverageList(stockData, maType, length3, inputList);
        var ema10List = GetMovingAverageList(stockData, maType, length4, inputList);
        var ema12List = GetMovingAverageList(stockData, maType, length5, inputList);
        var ema15List = GetMovingAverageList(stockData, maType, length6, inputList);
        var ema30List = GetMovingAverageList(stockData, maType, length7, inputList);
        var ema35List = GetMovingAverageList(stockData, maType, length8, inputList);
        var ema40List = GetMovingAverageList(stockData, maType, length9, inputList);
        var ema45List = GetMovingAverageList(stockData, maType, length10, inputList);
        var ema50List = GetMovingAverageList(stockData, maType, length11, inputList);
        var ema60List = GetMovingAverageList(stockData, maType, length12, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema1 = ema3List.ElementAtOrDefault(i);
            decimal ema2 = ema5List.ElementAtOrDefault(i);
            decimal ema3 = ema8List.ElementAtOrDefault(i);
            decimal ema4 = ema10List.ElementAtOrDefault(i);
            decimal ema5 = ema12List.ElementAtOrDefault(i);
            decimal ema6 = ema15List.ElementAtOrDefault(i);
            decimal ema7 = ema30List.ElementAtOrDefault(i);
            decimal ema8 = ema35List.ElementAtOrDefault(i);
            decimal ema9 = ema40List.ElementAtOrDefault(i);
            decimal ema10 = ema45List.ElementAtOrDefault(i);
            decimal ema11 = ema50List.ElementAtOrDefault(i);
            decimal ema12 = ema60List.ElementAtOrDefault(i);
            decimal diff12 = Math.Abs(ema1 - ema2);
            decimal diff23 = Math.Abs(ema2 - ema3);
            decimal diff34 = Math.Abs(ema3 - ema4);
            decimal diff45 = Math.Abs(ema4 - ema5);
            decimal diff56 = Math.Abs(ema5 - ema6);
            decimal diff78 = Math.Abs(ema7 - ema8);
            decimal diff89 = Math.Abs(ema8 - ema9);
            decimal diff910 = Math.Abs(ema9 - ema10);
            decimal diff1011 = Math.Abs(ema10 - ema11);
            decimal diff1112 = Math.Abs(ema11 - ema12);

            decimal fastDistance = diff12 + diff23 + diff34 + diff45 + diff56;
            fastDistanceList.Add(fastDistance);

            decimal slowDistance = diff78 + diff89 + diff910 + diff1011 + diff1112;
            slowDistanceList.Add(slowDistance);

            bool colFastL = ema1 > ema2 && ema2 > ema3 && ema3 > ema4 && ema4 > ema5 && ema5 > ema6;
            bool colFastS = ema1 < ema2 && ema2 < ema3 && ema3 < ema4 && ema4 < ema5 && ema5 < ema6;
            bool colSlowL = ema7 > ema8 && ema8 > ema9 && ema9 > ema10 && ema10 > ema11 && ema11 > ema12;
            bool colSlowS = ema7 < ema8 && ema8 < ema9 && ema9 < ema10 && ema10 < ema11 && ema11 < ema12;

            var signal = GetConditionSignal(colSlowL || colFastL && colSlowL, colSlowS || colFastS && colSlowS);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastDistance", fastDistanceList },
            { "SlowDistance", slowDistanceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.GuppyDistanceIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the G Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGOscillator(this StockData stockData, int length = 14)
    {
        List<decimal> bList = new();
        List<decimal> bSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBSum1 = i >= 1 ? bSumList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBSum2 = i >= 2 ? bSumList.ElementAtOrDefault(i - 2) : 0;

            decimal b = currentValue > prevValue ? (decimal)100 / length : 0;
            bList.Add(b);

            decimal bSum = bList.TakeLastExt(length).Sum();
            bSumList.Add(bSum);

            var signal = GetRsiSignal(bSum - prevBSum1, prevBSum1 - prevBSum2, bSum, prevBSum1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "GOsc", bSumList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bSumList;
        stockData.IndicatorName = IndicatorName.GOscillator;

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
            decimal gcv = Sqrt(i / length * logSum);
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
    /// Calculates the Gain Loss Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateGainLossMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 14, int signalLength = 7)
    {
        List<decimal> gainLossList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal gainLoss = currentValue + prevValue != 0 ? (currentValue - prevValue) / ((currentValue + prevValue) / 2) * 100 : 0;
            gainLossList.Add(gainLoss);
        }

        var gainLossAvgList = GetMovingAverageList(stockData, maType, length, gainLossList);
        var gainLossAvgSignalList = GetMovingAverageList(stockData, maType, signalLength, gainLossAvgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var gainLossSignal = gainLossAvgSignalList.ElementAtOrDefault(i);
            var prevGainLossSignal1 = i >= 1 ? gainLossAvgSignalList.ElementAtOrDefault(i - 1) : 0;
            var prevGainLossSignal2 = i >= 2 ? gainLossAvgSignalList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(gainLossSignal - prevGainLossSignal1, prevGainLossSignal1 - prevGainLossSignal2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Glma", gainLossAvgList },
            { "Signal", gainLossAvgSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gainLossAvgList;
        stockData.IndicatorName = IndicatorName.GainLossMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the High Low Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateHighLowIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 10)
    {
        List<decimal> hliList = new();
        List<decimal> advList = new();
        List<decimal> loList = new();
        List<decimal> advDiffList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevHighest = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowest = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);

            decimal adv = highest > prevHighest ? 1 : 0;
            advList.Add(adv);

            decimal lo = lowest < prevLowest ? 1 : 0;
            loList.Add(lo);

            decimal advSum = advList.TakeLastExt(length).Sum();
            decimal loSum = loList.TakeLastExt(length).Sum();

            decimal advDiff = advSum + loSum != 0 ? MinOrMax(advSum / (advSum + loSum) * 100, 100, 0) : 0;
            advDiffList.Add(advDiff);
        }

        var zmbtiList = GetMovingAverageList(stockData, maType, length, advDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal zmbti = zmbtiList.ElementAtOrDefault(i);
            decimal prevZmbti1 = i >= 1 ? hliList.ElementAtOrDefault(i - 1) : 0;
            decimal prevZmbti2 = i >= 2 ? hliList.ElementAtOrDefault(i - 2) : 0;

            var signal = GetRsiSignal(zmbti - prevZmbti1, prevZmbti1 - prevZmbti2, zmbti, prevZmbti1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Zmbti", zmbtiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zmbtiList;
        stockData.IndicatorName = IndicatorName.HighLowIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hawkeye Volume Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static StockData CalculateHawkeyeVolumeIndicator(this StockData stockData, InputName inputName = InputName.MedianPrice, int length = 200,
        decimal divisor = 3.6m)
    {
        List<decimal> tempRangeList = new();
        List<decimal> tempVolumeList = new();
        List<decimal> u1List = new();
        List<decimal> d1List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentValue = closeList.ElementAtOrDefault(i);

            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            tempVolumeList.Add(currentVolume);

            decimal range = currentHigh - currentLow;
            tempRangeList.Add(range);

            decimal volumeSma = tempVolumeList.TakeLastExt(length).Average();
            decimal rangeSma = tempRangeList.TakeLastExt(length).Average();
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMidpoint = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVolume = i >= 1 ? volumeList.ElementAtOrDefault(i - 1) : 0;

            decimal u1 = divisor != 0 ? prevMidpoint + ((prevHigh - prevLow) / divisor) : prevMidpoint;
            u1List.Add(u1);

            decimal d1 = divisor != 0 ? prevMidpoint - ((prevHigh - prevLow) / divisor) : prevMidpoint;
            d1List.Add(d1);

            bool rEnabled1 = range > rangeSma && currentValue < d1 && currentVolume > volumeSma;
            bool rEnabled2 = currentValue < prevMidpoint;
            bool rEnabled = rEnabled1 || rEnabled2;

            bool gEnabled1 = currentValue > prevMidpoint;
            bool gEnabled2 = range > rangeSma && currentValue > u1 && currentVolume > volumeSma;
            bool gEnabled3 = currentHigh > prevHigh && range < rangeSma / 1.5m && currentVolume < volumeSma;
            bool gEnabled4 = currentLow < prevLow && range < rangeSma / 1.5m && currentVolume > volumeSma;
            bool gEnabled = gEnabled1 || gEnabled2 || gEnabled3 || gEnabled4;

            bool grEnabled1 = range > rangeSma && currentValue > d1 && currentValue < u1 && currentVolume > volumeSma && currentVolume < volumeSma * 1.5m && currentVolume > prevVolume;
            bool grEnabled2 = range < rangeSma / 1.5m && currentVolume < volumeSma / 1.5m;
            bool grEnabled3 = currentValue > d1 && currentValue < u1;
            bool grEnabled = grEnabled1 || grEnabled2 || grEnabled3;

            var signal = GetConditionSignal(gEnabled, rEnabled);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Up", u1List },
            { "Dn", d1List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.HawkeyeVolumeIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Herrick Payoff Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="pointValue"></param>
    /// <returns></returns>
    public static StockData CalculateHerrickPayoffIndex(this StockData stockData, InputName inputName = InputName.MedianPrice, decimal pointValue = 100)
    {
        List<decimal> kList = new();
        List<decimal> hpicList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? closeList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevK = kList.LastOrDefault();
            decimal absDiff = Math.Abs(currentClose - prevClose);
            decimal g = Math.Min(currentOpen, prevOpen);
            decimal k = (currentValue - prevValue) * pointValue * currentVolume;
            decimal temp = g != 0 ? currentValue < prevValue ? 1 - (absDiff / 2 / g) : 1 + (absDiff / 2 / g) : 1;

            k *= temp;
            kList.Add(k);

            decimal prevHpic = hpicList.LastOrDefault();
            decimal hpic = prevK + (k - prevK);
            hpicList.Add(hpic);

            var signal = GetCompareSignal(hpic, prevHpic);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hpi", hpicList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hpicList;
        stockData.IndicatorName = IndicatorName.HerrickPayoffIndex;

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
    /// Calculates the Forecast Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateForecastOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 3)
    {
        List<decimal> pfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal pf = currentValue != 0 ? 100 * (currentValue - prevValue) / currentValue : 0;
            pfList.Add(pf);
        }

        var pfSmaList = GetMovingAverageList(stockData, maType, length, pfList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var pfSma = pfSmaList.ElementAtOrDefault(i);
            var prevPfSma = i >= 1 ? pfSmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(pfSma, prevPfSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fo", pfList },
            { "Signal", pfSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pfList;
        stockData.IndicatorName = IndicatorName.ForecastOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fisher Transform Stochastic Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="stochLength"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateFisherTransformStochasticOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 2, int stochLength = 30, int smoothLength = 5)
    {
        List<decimal> rbwList = new();
        List<decimal> ftsoList = new();
        List<decimal> numList = new();
        List<decimal> denomList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var wmaList = GetMovingAverageList(stockData, maType, length, inputList);
        var wma2List = GetMovingAverageList(stockData, maType, length, wmaList);
        var wma3List = GetMovingAverageList(stockData, maType, length, wma2List);
        var wma4List = GetMovingAverageList(stockData, maType, length, wma3List);
        var wma5List = GetMovingAverageList(stockData, maType, length, wma4List);
        var wma6List = GetMovingAverageList(stockData, maType, length, wma5List);
        var wma7List = GetMovingAverageList(stockData, maType, length, wma6List);
        var wma8List = GetMovingAverageList(stockData, maType, length, wma7List);
        var wma9List = GetMovingAverageList(stockData, maType, length, wma8List);
        var wma10List = GetMovingAverageList(stockData, maType, length, wma9List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wma1 = wmaList.ElementAtOrDefault(i);
            decimal wma2 = wma2List.ElementAtOrDefault(i);
            decimal wma3 = wma3List.ElementAtOrDefault(i);
            decimal wma4 = wma4List.ElementAtOrDefault(i);
            decimal wma5 = wma5List.ElementAtOrDefault(i);
            decimal wma6 = wma6List.ElementAtOrDefault(i);
            decimal wma7 = wma7List.ElementAtOrDefault(i);
            decimal wma8 = wma8List.ElementAtOrDefault(i);
            decimal wma9 = wma9List.ElementAtOrDefault(i);
            decimal wma10 = wma10List.ElementAtOrDefault(i);

            decimal rbw = ((wma1 * 5) + (wma2 * 4) + (wma3 * 3) + (wma4 * 2) + wma5 + wma6 + wma7 + wma8 + wma9 + wma10) / 20;
            rbwList.Add(rbw);
        }

        var (highestList, lowestList) = GetMaxAndMinValuesList(rbwList, stochLength);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal rbw = rbwList.ElementAtOrDefault(i);
            decimal prevFtso1 = i >= 1 ? ftsoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFtso2 = i >= 2 ? ftsoList.ElementAtOrDefault(i - 2) : 0;

            decimal num = rbw - lowest;
            numList.Add(num);

            decimal denom = highest - lowest;
            denomList.Add(denom);

            decimal numSum = numList.TakeLastExt(smoothLength).Sum();
            decimal denomSum = denomList.TakeLastExt(smoothLength).Sum();
            decimal rbws = denomSum + 0.0001m != 0 ? MinOrMax(numSum / (denomSum + 0.0001m) * 100, 100, 0) : 0;
            decimal x = 0.1m * (rbws - 50);

            decimal ftso = MinOrMax((((Exp(2 * x) - 1) / (Exp(2 * x) + 1)) + 1) * 50, 100, 0);
            ftsoList.Add(ftso);

            var signal = GetRsiSignal(ftso - prevFtso1, prevFtso1 - prevFtso2, ftso, prevFtso1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ftso", ftsoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ftsoList;
        stockData.IndicatorName = IndicatorName.FisherTransformStochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fast and Slow Stochastic Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateFastandSlowStochasticOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length1 = 3, int length2 = 6, int length3 = 9, int length4 = 9)
    {
        List<decimal> fsstList = new();
        List<Signal> signalsList = new();

        var fskList = CalculateFastandSlowKurtosisOscillator(stockData, maType, length1).CustomValuesList;
        var v4List = GetMovingAverageList(stockData, maType, length2, fskList);
        var fastKList = CalculateStochasticOscillator(stockData, maType, length: length3).CustomValuesList;
        var slowKList = GetMovingAverageList(stockData, maType, length3, fastKList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal v4 = v4List.ElementAtOrDefault(i);
            decimal slowK = slowKList.ElementAtOrDefault(i);

            decimal fsst = (500 * v4) + slowK;
            fsstList.Add(fsst);
        }

        var wfsstList = GetMovingAverageList(stockData, maType, length4, fsstList);
        for (int i = 0; i < wfsstList.Count; i++)
        {
            decimal fsst = fsstList.ElementAtOrDefault(i);
            decimal wfsst = wfsstList.ElementAtOrDefault(i);
            decimal prevFsst = i >= 1 ? fsstList.ElementAtOrDefault(i - 1) : 0;
            decimal prevWfsst = i >= 1 ? wfsstList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(fsst - wfsst, prevFsst - prevWfsst);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fsst", fsstList },
            { "Signal", wfsstList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fsstList;
        stockData.IndicatorName = IndicatorName.FastandSlowStochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fast and Slow Kurtosis Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public static StockData CalculateFastandSlowKurtosisOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 3, decimal ratio = 0.03m)
    {
        List<decimal> fskList = new();
        List<decimal> momentumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal prevMomentum = momentumList.LastOrDefault();
            decimal momentum = currentValue - prevValue;
            momentumList.Add(momentum);

            decimal prevFsk = fskList.LastOrDefault();
            decimal fsk = (ratio * (momentum - prevMomentum)) + ((1 - ratio) * prevFsk);
            fskList.Add(fsk);
        }

        var fskSignalList = GetMovingAverageList(stockData, maType, length, fskList);
        for (int i = 0; i < fskSignalList.Count; i++)
        {
            decimal fsk = fskList.ElementAtOrDefault(i);
            decimal fskSignal = fskSignalList.ElementAtOrDefault(i);
            decimal prevFsk = i >= 1 ? fskList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFskSignal = i >= 1 ? fskSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(fsk - fskSignal, prevFsk - prevFskSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fsk", fskList },
            { "Signal", fskSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fskList;
        stockData.IndicatorName = IndicatorName.FastandSlowKurtosisOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fast and Slow Relative Strength Index Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateFastandSlowRelativeStrengthIndexOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length1 = 3, int length2 = 6, int length3 = 9, int length4 = 6)
    {
        List<decimal> fsrsiList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length3).CustomValuesList;
        var fskList = CalculateFastandSlowKurtosisOscillator(stockData, maType, length: length1).CustomValuesList;
        var v4List = GetMovingAverageList(stockData, maType, length2, fskList);

        for (int i = 0; i < v4List.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal v4 = v4List.ElementAtOrDefault(i);

            decimal fsrsi = (10000 * v4) + rsi;
            fsrsiList.Add(fsrsi);
        }

        var fsrsiSignalList = GetMovingAverageList(stockData, maType, length4, fsrsiList);
        for (int j = 0; j < fsrsiSignalList.Count; j++)
        {
            decimal fsrsi = fsrsiList.ElementAtOrDefault(j);
            decimal fsrsiSignal = fsrsiSignalList.ElementAtOrDefault(j);
            decimal prevFsrsi = j >= 1 ? fsrsiList.ElementAtOrDefault(j - 1) : 0;
            decimal prevFsrsiSignal = j >= 1 ? fsrsiSignalList.ElementAtOrDefault(j - 1) : 0;

            var signal = GetCompareSignal(fsrsi - fsrsiSignal, prevFsrsi - prevFsrsiSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fsrsi", fsrsiList },
            { "Signal", fsrsiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fsrsiList;
        stockData.IndicatorName = IndicatorName.FastandSlowRelativeStrengthIndexOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fast Slow Degree Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateFastSlowDegreeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 100, int fastLength = 3, int slowLength = 2, int signalLength = 14)
    {
        List<decimal> fastF1bList = new();
        List<decimal> fastF2bList = new();
        List<decimal> fastVWList = new();
        List<decimal> slowF1bList = new();
        List<decimal> slowF2bList = new();
        List<decimal> slowVWList = new();
        List<decimal> slowVWSumList = new();
        List<decimal> osList = new();
        List<decimal> histList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal fastF1x = (decimal)(i + 1) / length;
            decimal fastF1b = 1 / (i + 1) * Sin(fastF1x * (i + 1) * Pi);
            fastF1bList.Add(fastF1b);

            decimal fastF1bSum = fastF1bList.TakeLastExt(fastLength).Sum();
            decimal fastF1pol = (fastF1x * fastF1x) + fastF1bSum;
            decimal fastF2x = (decimal)i / length;
            decimal fastF2b = 1 / (i + 1) * Sin(fastF2x * (i + 1) * Pi);
            fastF2bList.Add(fastF2b);

            decimal fastF2bSum = fastF2bList.TakeLastExt(fastLength).Sum();
            decimal fastF2pol = (fastF2x * fastF2x) + fastF2bSum;
            decimal fastW = fastF1pol - fastF2pol;
            decimal fastVW = prevValue * fastW;
            fastVWList.Add(fastVW);

            decimal fastVWSum = fastVWList.TakeLastExt(length).Sum();
            decimal slowF1x = (decimal)(i + 1) / length;
            decimal slowF1b = 1 / (i + 1) * Sin(slowF1x * (i + 1) * Pi);
            slowF1bList.Add(slowF1b);

            decimal slowF1bSum = slowF1bList.TakeLastExt(slowLength).Sum();
            decimal slowF1pol = (slowF1x * slowF1x) + slowF1bSum;
            decimal slowF2x = (decimal)i / length;
            decimal slowF2b = 1 / (i + 1) * Sin(slowF2x * (i + 1) * Pi);
            slowF2bList.Add(slowF2b);

            decimal slowF2bSum = slowF2bList.TakeLastExt(slowLength).Sum();
            decimal slowF2pol = (slowF2x * slowF2x) + slowF2bSum;
            decimal slowW = slowF1pol - slowF2pol;
            decimal slowVW = prevValue * slowW;
            slowVWList.Add(slowVW);

            decimal slowVWSum = slowVWList.TakeLastExt(length).Sum();
            slowVWSumList.Add(slowVWSum);

            decimal os = fastVWSum - slowVWSum;
            osList.Add(os);
        }

        var osSignalList = GetMovingAverageList(stockData, maType, signalLength, osList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal os = osList.ElementAtOrDefault(i);
            decimal osSignal = osSignalList.ElementAtOrDefault(i);

            decimal prevHist = histList.LastOrDefault();
            decimal hist = os - osSignal;
            histList.Add(hist);

            var signal = GetCompareSignal(hist, prevHist);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fsdo", osList },
            { "Signal", osSignalList },
            { "Histogram", histList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = osList;
        stockData.IndicatorName = IndicatorName.FastSlowDegreeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fractal Chaos Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateFractalChaosOscillator(this StockData stockData)
    {
        List<decimal> fcoList = new();
        List<Signal> signalsList = new();

        var fractalChaosBandsList = CalculateFractalChaosBands(stockData);
        var upperBandList = fractalChaosBandsList.OutputValues["UpperBand"];
        var lowerBandList = fractalChaosBandsList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal upperBand = upperBandList.ElementAtOrDefault(i);
            decimal prevUpperBand = i >= 1 ? upperBandList.ElementAtOrDefault(i - 1) : 0;
            decimal lowerBand = lowerBandList.ElementAtOrDefault(i);
            decimal prevLowerBand = i >= 1 ? lowerBandList.ElementAtOrDefault(i - 1) : 0;

            decimal prevFco = fcoList.LastOrDefault();
            decimal fco = upperBand != prevUpperBand ? 1 : lowerBand != prevLowerBand ? -1 : 0;
            fcoList.Add(fco);

            var signal = GetCompareSignal(fco, prevFco);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fco", fcoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fcoList;
        stockData.IndicatorName = IndicatorName.FractalChaosOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Firefly Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateFireflyOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ZeroLagExponentialMovingAverage,
        int length = 10, int smoothLength = 3)
    {
        List<decimal> v2List = new();
        List<decimal> v5List = new();
        List<decimal> wwList = new();
        List<decimal> mmList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);

            decimal v2 = (currentHigh + currentLow + (currentClose * 2)) / 4;
            v2List.Add(v2);
        }

        var v3List = GetMovingAverageList(stockData, maType, length, v2List);
        stockData.CustomValuesList = v2List;
        var v4List = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal v2 = v2List.ElementAtOrDefault(i);
            decimal v3 = v3List.ElementAtOrDefault(i);
            decimal v4 = v4List.ElementAtOrDefault(i);

            decimal v5 = v4 == 0 ? (v2 - v3) * 100 : (v2 - v3) * 100 / v4;
            v5List.Add(v5);
        }

        var v6List = GetMovingAverageList(stockData, maType, smoothLength, v5List);
        var v7List = GetMovingAverageList(stockData, maType, smoothLength, v6List);
        var wwZLagEmaList = GetMovingAverageList(stockData, maType, length, v7List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wwZlagEma = wwZLagEmaList.ElementAtOrDefault(i);
            decimal prevWw1 = i >= 1 ? wwList.ElementAtOrDefault(i - 1) : 0;
            decimal prevWw2 = i >= 2 ? wwList.ElementAtOrDefault(i - 2) : 0;

            decimal ww = ((wwZlagEma + 100) / 2) - 4;
            wwList.Add(ww);

            decimal mm = wwList.TakeLastExt(smoothLength).Max();
            mmList.Add(mm);

            var signal = GetRsiSignal(ww - prevWw1, prevWw1 - prevWw2, ww, prevWw1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fo", wwList },
            { "Signal", mmList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wwList;
        stockData.IndicatorName = IndicatorName.FireflyOscillator;

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
        var smaStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

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
    /// Calculates the Fibonacci Retrace
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateFibonacciRetrace(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length1 = 15, int length2 = 50, decimal factor = 0.382m)
    {
        List<decimal> hretList = new();
        List<decimal> lretList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        var wmaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wma = wmaList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal prevWma = i >= 1 ? wmaList.ElementAtOrDefault(i - 1) : 0;
            decimal retrace = (highest - lowest) * factor;

            decimal prevHret = hretList.LastOrDefault();
            decimal hret = highest - retrace;
            hretList.Add(hret);

            decimal prevLret = lretList.LastOrDefault();
            decimal lret = lowest + retrace;
            lretList.Add(lret);

            var signal = GetBullishBearishSignal(wma - hret, prevWma - prevHret, wma - lret, prevWma - prevLret);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", hretList },
            { "LowerBand", lretList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.FibonacciRetrace;

        return stockData;
    }

    /// <summary>
    /// Calculates the FX Sniper Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="cciLength"></param>
    /// <param name="t3Length"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static StockData CalculateFXSniperIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int cciLength = 14, int t3Length = 5, decimal b = 0.618m)
    {
        List<decimal> e1List = new();
        List<decimal> e2List = new();
        List<decimal> e3List = new();
        List<decimal> e4List = new();
        List<decimal> e5List = new();
        List<decimal> e6List = new();
        List<decimal> fxSniperList = new();
        List<Signal> signalsList = new();

        decimal b2 = b * b;
        decimal b3 = b2 * b;
        decimal c1 = -b3;
        decimal c2 = 3 * (b2 + b3);
        decimal c3 = -3 * ((2 * b2) + b + b3);
        decimal c4 = 1 + (3 * b) + b3 + (3 * b2);
        decimal nr = 1 + (0.5m * (t3Length - 1));
        decimal w1 = 2 / (nr + 1);
        decimal w2 = 1 - w1;

        var cciList = CalculateCommodityChannelIndex(stockData, maType, cciLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal cci = cciList.ElementAtOrDefault(i);

            decimal prevE1 = e1List.LastOrDefault();
            decimal e1 = (w1 * cci) + (w2 * prevE1);
            e1List.Add(e1);

            decimal prevE2 = e2List.LastOrDefault();
            decimal e2 = (w1 * e1) + (w2 * prevE2);
            e2List.Add(e2);

            decimal prevE3 = e3List.LastOrDefault();
            decimal e3 = (w1 * e2) + (w2 * prevE3);
            e3List.Add(e3);

            decimal prevE4 = e4List.LastOrDefault();
            decimal e4 = (w1 * e3) + (w2 * prevE4);
            e4List.Add(e4);

            decimal prevE5 = e5List.LastOrDefault();
            decimal e5 = (w1 * e4) + (w2 * prevE5);
            e5List.Add(e5);

            decimal prevE6 = e6List.LastOrDefault();
            decimal e6 = (w1 * e5) + (w2 * prevE6);
            e6List.Add(e6);

            decimal prevFxSniper = fxSniperList.LastOrDefault();
            decimal fxsniper = (c1 * e6) + (c2 * e5) + (c3 * e4) + (c4 * e3);
            fxSniperList.Add(fxsniper);

            var signal = GetCompareSignal(fxsniper, prevFxSniper);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FXSniper", fxSniperList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fxSniperList;
        stockData.IndicatorName = IndicatorName.FXSniperIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Finite Volume Elements
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateFiniteVolumeElements(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 22, decimal factor = 0.3m)
    {
        List<decimal> fveList = new();
        List<decimal> bullList = new();
        List<decimal> bearList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var medianPriceList = CalculateMedianPrice(stockData).CustomValuesList;
        var typicalPriceList = CalculateTypicalPrice(stockData).CustomValuesList;
        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal medianPrice = medianPriceList.ElementAtOrDefault(i);
            decimal typicalPrice = typicalPriceList.ElementAtOrDefault(i);
            decimal prevTypicalPrice = i >= 1 ? typicalPriceList.ElementAtOrDefault(i - 1) : 0;
            decimal volumeSma = volumeSmaList.ElementAtOrDefault(i);
            decimal volume = volumeList.ElementAtOrDefault(i);
            decimal close = inputList.ElementAtOrDefault(i);
            decimal nmf = close - medianPrice + typicalPrice - prevTypicalPrice;
            decimal nvlm = nmf > factor * close / 100 ? volume : nmf < -factor * close / 100 ? -volume : 0;

            decimal prevFve = fveList.LastOrDefault();
            decimal prevFve2 = i >= 2 ? fveList.ElementAtOrDefault(i - 2) : 0;
            decimal fve = volumeSma != 0 && length != 0 ? prevFve + (nvlm / volumeSma / length * 100) : prevFve;
            fveList.Add(fve);

            decimal prevBullSlope = bullList.LastOrDefault();
            decimal bullSlope = fve - Math.Max(prevFve, prevFve2);
            bullList.Add(bullSlope);

            decimal prevBearSlope = bearList.LastOrDefault();
            decimal bearSlope = fve - Math.Min(prevFve, prevFve2);
            bearList.Add(bearSlope);

            var signal = GetBullishBearishSignal(bullSlope, prevBullSlope, bearSlope, prevBearSlope);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fve", fveList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fveList;
        stockData.IndicatorName = IndicatorName.FiniteVolumeElements;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Volume Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeVolumeIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 60)
    {
        List<decimal> relVolList = new();
        List<decimal> dplList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var smaVolumeList = GetMovingAverageList(stockData, maType, length, volumeList);
        stockData.CustomValuesList = volumeList;
        var stdDevVolumeList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal av = smaVolumeList.ElementAtOrDefault(i);
            decimal sd = stdDevVolumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal relVol = sd != 0 ? (currentVolume - av) / sd : 0;
            relVolList.Add(relVol);

            decimal prevDpl = dplList.LastOrDefault();
            decimal dpl = relVol >= 2 ? prevValue : i >= 1 ? prevDpl : currentValue;
            dplList.Add(dpl);

            var signal = GetCompareSignal(currentValue - dpl, prevValue - prevDpl);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rvi", relVolList },
            { "Dpl", dplList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = relVolList;
        stockData.IndicatorName = IndicatorName.RelativeVolumeIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Freedom of Movement
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFreedomOfMovement(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 60)
    {
        List<decimal> aMoveList = new();
        List<decimal> vBymList = new();
        List<decimal> theFomList = new();
        List<decimal> avfList = new();
        List<decimal> dplList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var relVolList = CalculateRelativeVolumeIndicator(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal currentRelVol = relVolList.ElementAtOrDefault(i);
            tempList.Add(currentRelVol);

            decimal aMove = prevValue != 0 ? Math.Abs((currentValue - prevValue) / prevValue) : 0;
            aMoveList.Add(aMove);

            var list = aMoveList.TakeLastExt(length).ToList();
            decimal aMoveMax = list.Max();
            decimal aMoveMin = list.Min();
            decimal theMove = aMoveMax - aMoveMin != 0 ? (1 + ((aMove - aMoveMin) * (10 - 1))) / (aMoveMax - aMoveMin) : 0;
            var tList = tempList.TakeLastExt(length).ToList();
            decimal relVolMax = tList.Max();
            decimal relVolMin = tList.Min();
            decimal theVol = relVolMax - relVolMin != 0 ? (1 + ((currentRelVol - relVolMin) * (10 - 1))) / (relVolMax - relVolMin) : 0;

            decimal vBym = theMove != 0 ? theVol / theMove : 0;
            vBymList.Add(vBym);

            decimal avf = vBymList.TakeLastExt(length).Average();
            avfList.Add(avf);
        }

        stockData.CustomValuesList = vBymList;
        var sdfList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal vBym = vBymList.ElementAtOrDefault(i);
            decimal avf = avfList.ElementAtOrDefault(i);
            decimal sdf = sdfList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal theFom = sdf != 0 ? (vBym - avf) / sdf : 0;
            theFomList.Add(theFom);

            decimal prevDpl = dplList.LastOrDefault();
            decimal dpl = theFom >= 2 ? prevValue : i >= 1 ? prevDpl : currentValue;
            dplList.Add(dpl);

            var signal = GetCompareSignal(currentValue - dpl, prevValue - prevDpl);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fom", theFomList },
            { "Dpl", dplList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = theFomList;
        stockData.IndicatorName = IndicatorName.FreedomOfMovement;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fear and Greed Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateFearAndGreedIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int fastLength = 10, int slowLength = 30, int smoothLength = 2)
    {
        List<decimal> trUpList = new();
        List<decimal> trDnList = new();
        List<decimal> fgiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

            decimal trUp = currentValue > prevValue ? tr : 0;
            trUpList.Add(trUp);

            decimal trDn = currentValue < prevValue ? tr : 0;
            trDnList.Add(trDn);
        }

        var fastTrUpList = GetMovingAverageList(stockData, maType, fastLength, trUpList);
        var fastTrDnList = GetMovingAverageList(stockData, maType, fastLength, trDnList);
        var slowTrUpList = GetMovingAverageList(stockData, maType, slowLength, trUpList);
        var slowTrDnList = GetMovingAverageList(stockData, maType, slowLength, trDnList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastTrUp = fastTrUpList.ElementAtOrDefault(i);
            decimal fastTrDn = fastTrDnList.ElementAtOrDefault(i);
            decimal slowTrUp = slowTrUpList.ElementAtOrDefault(i);
            decimal slowTrDn = slowTrDnList.ElementAtOrDefault(i);
            decimal fastDiff = fastTrUp - fastTrDn;
            decimal slowDiff = slowTrUp - slowTrDn;

            decimal fgi = fastDiff - slowDiff;
            fgiList.Add(fgi);
        }

        var fgiEmaList = GetMovingAverageList(stockData, maType, smoothLength, fgiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fgiEma = fgiEmaList.ElementAtOrDefault(i);
            decimal prevFgiEma = i >= 1 ? fgiEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(fgiEma, prevFgiEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fgi", fgiList },
            { "Signal", fgiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fgiList;
        stockData.IndicatorName = IndicatorName.FearAndGreedIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Function To Candles
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFunctionToCandles(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 14)
    {
        List<decimal> tpList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        stockData.CustomValuesList = inputList;
        var rsiCList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;
        stockData.CustomValuesList = openList;
        var rsiOList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;
        stockData.CustomValuesList = highList;
        var rsiHList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;
        stockData.CustomValuesList = lowList;
        var rsiLList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsiC = rsiCList.ElementAtOrDefault(i);
            decimal rsiO = rsiOList.ElementAtOrDefault(i);
            decimal rsiH = rsiHList.ElementAtOrDefault(i);
            decimal rsiL = rsiLList.ElementAtOrDefault(i);
            decimal prevTp1 = i >= 1 ? tpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevTp2 = i >= 2 ? tpList.ElementAtOrDefault(i - 2) : 0;

            decimal tp = (rsiC + rsiO + rsiH + rsiL) / 4;
            tpList.Add(tp);

            var signal = GetCompareSignal(tp - prevTp1, prevTp1 - prevTp2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Close", rsiCList },
            { "Open", rsiOList },
            { "High", rsiHList },
            { "Low", rsiLList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.FunctionToCandles;

        return stockData;
    }

    /// <summary>
    /// Calculates the Karobein Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKarobeinOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 50)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal a = ema < prevEma && prevEma != 0 ? ema / prevEma : 0;
            aList.Add(a);

            decimal b = ema > prevEma && prevEma != 0 ? ema / prevEma : 0;
            bList.Add(b);
        }

        var aEmaList = GetMovingAverageList(stockData, maType, length, aList);
        var bEmaList = GetMovingAverageList(stockData, maType, length, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal a = aEmaList.ElementAtOrDefault(i);
            decimal b = bEmaList.ElementAtOrDefault(i);
            decimal prevD1 = i >= 1 ? dList.ElementAtOrDefault(i - 1) : 0;
            decimal prevD2 = i >= 2 ? dList.ElementAtOrDefault(i - 2) : 0;
            decimal c = prevEma != 0 && ema != 0 ? MinOrMax(ema / prevEma / ((ema / prevEma) + b), 1, 0) : 0;

            decimal d = prevEma != 0 && ema != 0 ? MinOrMax((2 * (ema / prevEma / ((ema / prevEma) + (c * a)))) - 1, 1, 0) : 0;
            dList.Add(d);

            var signal = GetRsiSignal(d - prevD1, prevD1 - prevD2, d, prevD1, 0.8m, 0.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ko", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dList;
        stockData.IndicatorName = IndicatorName.KarobeinOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Peak Oscillator V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateKasePeakOscillatorV1(this StockData stockData, int length = 30, int smoothLength = 3)
    {
        List<decimal> diffList = new();
        List<decimal> lnList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        decimal sqrt = Sqrt(length);

        var atrList = CalculateAverageTrueRange(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentAtr = atrList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevLow = i >= length ? lowList.ElementAtOrDefault(i - length) : 0;
            decimal prevHigh = i >= length ? highList.ElementAtOrDefault(i - length) : 0;
            decimal rwh = currentAtr != 0 ? (currentHigh - prevLow) / currentAtr * sqrt : 0;
            decimal rwl = currentAtr != 0 ? (prevHigh - currentLow) / currentAtr * sqrt : 0;

            decimal diff = rwh - rwl;
            diffList.Add(diff);
        }

        var pkList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, smoothLength, diffList);
        var mnList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length, pkList);
        stockData.CustomValuesList = pkList;
        var sdList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pk = pkList.ElementAtOrDefault(i);
            decimal mn = mnList.ElementAtOrDefault(i);
            decimal sd = sdList.ElementAtOrDefault(i);
            decimal prevPk = i >= 1 ? pkList.ElementAtOrDefault(i - 1) : 0;
            decimal v1 = mn + (1.33m * sd) > 2.08m ? mn + (1.33m * sd) : 2.08m;
            decimal v2 = mn - (1.33m * sd) < -1.92m ? mn - (1.33m * sd) : -1.92m;

            decimal prevLn = lnList.LastOrDefault();
            decimal ln = prevPk >= 0 && pk > 0 ? v1 : prevPk <= 0 && pk < 0 ? v2 : 0;
            lnList.Add(ln);

            var signal = GetCompareSignal(ln, prevLn);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kpo", lnList },
            { "Pk", pkList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lnList;
        stockData.IndicatorName = IndicatorName.KasePeakOscillatorV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Peak Oscillator V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="smoothLength"></param>
    /// <param name="devFactor"></param>
    /// <param name="sensitivity"></param>
    /// <returns></returns>
    public static StockData CalculateKasePeakOscillatorV2(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 8, int slowLength = 65, int length1 = 9, int length2 = 30, int length3 = 50, int smoothLength = 3, decimal devFactor = 2,
        decimal sensitivity = 40)
    {
        List<decimal> ccLogList = new();
        List<decimal> xpAbsAvgList = new();
        List<decimal> kpoBufferList = new();
        List<decimal> x1List = new();
        List<decimal> x2List = new();
        List<decimal> xpList = new();
        List<decimal> xpAbsList = new();
        List<decimal> kppBufferList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal temp = prevValue != 0 ? currentValue / prevValue : 0;

            decimal ccLog = temp > 0 ? Log(temp) : 0;
            ccLogList.Add(ccLog);
        }

        stockData.CustomValuesList = ccLogList;
        var ccDevList = CalculateStandardDeviationVolatility(stockData, length1).CustomValuesList;
        var ccDevAvgList = GetMovingAverageList(stockData, maType, length2, ccDevList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal avg = ccDevAvgList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);

            decimal max1 = 0, max2 = 0;
            for (int j = fastLength; j < slowLength; j++)
            {
                decimal sqrtK = Sqrt(j);
                decimal prevLow = i >= j ? lowList.ElementAtOrDefault(i - j) : 0;
                decimal prevHigh = i >= j ? highList.ElementAtOrDefault(i - j) : 0;
                decimal temp1 = prevLow != 0 ? currentHigh / prevLow : 0;
                decimal log1 = temp1 > 0 ? Log(temp1) : 0;
                max1 = Math.Max(log1 / sqrtK, max1);
                decimal temp2 = currentLow != 0 ? prevHigh / currentLow : 0;
                decimal log2 = temp2 > 0 ? Log(temp2) : 0;
                max2 = Math.Max(log2 / sqrtK, max2);
            }

            decimal x1 = avg != 0 ? max1 / avg : 0;
            x1List.Add(x1);

            decimal x2 = avg != 0 ? max2 / avg : 0;
            x2List.Add(x2);

            decimal xp = sensitivity * (x1List.TakeLastExt(smoothLength).Average() - x2List.TakeLastExt(smoothLength).Average());
            xpList.Add(xp);

            decimal xpAbs = Math.Abs(xp);
            xpAbsList.Add(xpAbs);

            decimal xpAbsAvg = xpAbsList.TakeLastExt(length3).Average();
            xpAbsAvgList.Add(xpAbsAvg);
        }

        stockData.CustomValuesList = xpAbsList;
        var xpAbsStdDevList = CalculateStandardDeviationVolatility(stockData, length3).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal xpAbsAvg = xpAbsAvgList.ElementAtOrDefault(i);
            decimal xpAbsStdDev = xpAbsStdDevList.ElementAtOrDefault(i);
            decimal prevKpoBuffer1 = i >= 1 ? kpoBufferList.ElementAtOrDefault(i - 1) : 0;
            decimal prevKpoBuffer2 = i >= 2 ? kpoBufferList.ElementAtOrDefault(i - 2) : 0;

            decimal tmpVal = xpAbsAvg + (devFactor * xpAbsStdDev);
            decimal maxVal = Math.Max(90, tmpVal);
            decimal minVal = Math.Min(90, tmpVal);

            decimal prevKpoBuffer = kpoBufferList.LastOrDefault();
            decimal kpoBuffer = xpList.ElementAtOrDefault(i);
            kpoBufferList.Add(kpoBuffer);

            decimal kppBuffer = prevKpoBuffer1 > 0 && prevKpoBuffer1 > kpoBuffer && prevKpoBuffer1 >= prevKpoBuffer2 &&
                prevKpoBuffer1 >= maxVal ? prevKpoBuffer1 : prevKpoBuffer1 < 0 && prevKpoBuffer1 < kpoBuffer &&
                prevKpoBuffer1 <= prevKpoBuffer2 && prevKpoBuffer1 <= maxVal * -1 ? prevKpoBuffer1 :
                prevKpoBuffer1 > 0 && prevKpoBuffer1 > kpoBuffer && prevKpoBuffer1 >= prevKpoBuffer2 ? prevKpoBuffer1 :
                prevKpoBuffer1 < 0 && prevKpoBuffer1 < kpoBuffer && prevKpoBuffer1 <= prevKpoBuffer2 ? prevKpoBuffer1 : 0;
            kppBufferList.Add(kppBuffer);

            var signal = GetCompareSignal(kpoBuffer, prevKpoBuffer);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kpo", xpList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = xpList;
        stockData.IndicatorName = IndicatorName.KasePeakOscillatorV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Serial Dependency Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKaseSerialDependencyIndex(this StockData stockData, int length = 14)
    {
        List<decimal> ksdiUpList = new();
        List<decimal> ksdiDownList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal temp = prevValue != 0 ? currentValue / prevValue : 0;

            decimal tempLog = temp > 0 ? Log(temp) : 0;
            tempList.Add(tempLog);
        }

        stockData.CustomValuesList = tempList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal volatility = stdDevList.ElementAtOrDefault(i);
            decimal prevHigh = i >= length ? highList.ElementAtOrDefault(i - length) : 0;
            decimal prevLow = i >= length ? lowList.ElementAtOrDefault(i - length) : 0;
            decimal ksdiUpTemp = prevLow != 0 ? currentHigh / prevLow : 0;
            decimal ksdiDownTemp = prevHigh != 0 ? currentLow / prevHigh : 0;
            decimal ksdiUpLog = ksdiUpTemp > 0 ? Log(ksdiUpTemp) : 0;
            decimal ksdiDownLog = ksdiDownTemp > 0 ? Log(ksdiDownTemp) : 0;

            decimal prevKsdiUp = ksdiUpList.LastOrDefault();
            decimal ksdiUp = volatility != 0 ? ksdiUpLog / volatility : 0;
            ksdiUpList.Add(ksdiUp);

            decimal prevKsdiDown = ksdiDownList.LastOrDefault();
            decimal ksdiDown = volatility != 0 ? ksdiDownLog / volatility : 0;
            ksdiDownList.Add(ksdiDown);

            var signal = GetCompareSignal(ksdiUp - ksdiDown, prevKsdiUp - prevKsdiDown);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "KsdiUp", ksdiUpList },
            { "KsdiDn", ksdiDownList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.KaseSerialDependencyIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kaufman Binary Wave
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="fastSc"></param>
    /// <param name="slowSc"></param>
    /// <param name="filterPct"></param>
    /// <returns></returns>
    public static StockData CalculateKaufmanBinaryWave(this StockData stockData, int length = 20, decimal fastSc = 0.6022m, decimal slowSc = 0.0645m,
        decimal filterPct = 10)
    {
        List<decimal> amaList = new();
        List<decimal> diffList = new();
        List<decimal> amaLowList = new();
        List<decimal> amaHighList = new();
        List<decimal> bwList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var efRatioList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal efRatio = efRatioList.ElementAtOrDefault(i);
            decimal prevAma = i >= 1 ? amaList.ElementAtOrDefault(i - 1) : currentValue;
            decimal smooth = Pow((efRatio * fastSc) + slowSc, 2);

            decimal ama = prevAma + (smooth * (currentValue - prevAma));
            amaList.Add(ama);

            decimal diff = ama - prevAma;
            diffList.Add(diff);
        }

        stockData.CustomValuesList = diffList;
        var diffStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ama = amaList.ElementAtOrDefault(i);
            decimal diffStdDev = diffStdDevList.ElementAtOrDefault(i);
            decimal prevAma = i >= 1 ? amaList.ElementAtOrDefault(i - 1) : currentValue;
            decimal filter = filterPct / 100 * diffStdDev;

            decimal prevAmaLow = amaLowList.LastOrDefault();
            decimal amaLow = ama < prevAma ? ama : prevAmaLow;
            amaLowList.Add(amaLow);

            decimal prevAmaHigh = amaHighList.LastOrDefault();
            decimal amaHigh = ama > prevAma ? ama : prevAmaHigh;
            amaHighList.Add(amaHigh);

            decimal prevBw = bwList.LastOrDefault();
            decimal bw = ama - amaLow > filter ? 1 : amaHigh - ama > filter ? -1 : 0;
            bwList.Add(bw);

            var signal = GetCompareSignal(bw, prevBw);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kbw", bwList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bwList;
        stockData.IndicatorName = IndicatorName.KaufmanBinaryWave;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kurtosis Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateKurtosisIndicator(this StockData stockData, int length1 = 3, int length2 = 1, int fastLength = 3,
        int slowLength = 65)
    {
        List<decimal> diffList = new();
        List<decimal> kList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length1 ? inputList.ElementAtOrDefault(i - length1) : 0;
            decimal prevDiff = i >= length2 ? diffList.ElementAtOrDefault(i - length2) : 0;

            decimal diff = currentValue - prevValue;
            diffList.Add(diff);

            decimal k = diff - prevDiff;
            kList.Add(k);
        }

        var fkList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, slowLength, kList);
        var fskList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, fastLength, fkList);
        for (int j = 0; j < stockData.Count; j++)
        {
            decimal fsk = fskList.ElementAtOrDefault(j);
            decimal prevFsk = j >= 1 ? fskList.ElementAtOrDefault(j - 1) : 0;

            var signal = GetCompareSignal(fsk, prevFsk);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fk", fkList },
            { "Signal", fskList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fkList;
        stockData.IndicatorName = IndicatorName.KurtosisIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kaufman Adaptive Correlation Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKaufmanAdaptiveCorrelationOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.KaufmanAdaptiveMovingAverage, int length = 14)
    {
        List<decimal> indexList = new();
        List<decimal> index2List = new();
        List<decimal> src2List = new();
        List<decimal> srcStList = new();
        List<decimal> indexStList = new();
        List<decimal> indexSrcList = new();
        List<decimal> rList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var kamaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal index = i;
            indexList.Add(index);

            decimal indexSrc = i * currentValue;
            indexSrcList.Add(indexSrc);

            decimal srcSrc = currentValue * currentValue;
            src2List.Add(srcSrc);

            decimal indexIndex = index * index;
            index2List.Add(indexIndex);
        }

        var indexMaList = GetMovingAverageList(stockData, maType, length, indexList);
        var indexSrcMaList = GetMovingAverageList(stockData, maType, length, indexSrcList);
        var index2MaList = GetMovingAverageList(stockData, maType, length, index2List);
        var src2MaList = GetMovingAverageList(stockData, maType, length, src2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal srcMa = kamaList.ElementAtOrDefault(i);
            decimal indexMa = indexMaList.ElementAtOrDefault(i);
            decimal indexSrcMa = indexSrcMaList.ElementAtOrDefault(i);
            decimal index2Ma = index2MaList.ElementAtOrDefault(i);
            decimal src2Ma = src2MaList.ElementAtOrDefault(i);
            decimal prevR1 = i >= 1 ? rList.ElementAtOrDefault(i - 1) : 0;
            decimal prevR2 = i >= 2 ? rList.ElementAtOrDefault(i - 2) : 0;

            decimal indexSqrt = index2Ma - Pow(indexMa, 2);
            decimal indexSt = indexSqrt >= 0 ? Sqrt(indexSqrt) : 0;
            indexStList.Add(indexSt);

            decimal srcSqrt = src2Ma - Pow(srcMa, 2);
            decimal srcSt = srcSqrt >= 0 ? Sqrt(srcSqrt) : 0;
            srcStList.Add(srcSt);

            decimal a = indexSrcMa - (indexMa * srcMa);
            decimal b = indexSt * srcSt;

            decimal r = b != 0 ? a / b : 0;
            rList.Add(r);

            var signal = GetRsiSignal(r - prevR1, prevR1 - prevR2, r, prevR1, 0.5m, -0.5m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "IndexSt", indexStList },
            { "SrcSt", srcStList },
            { "Kaco", rList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rList;
        stockData.IndicatorName = IndicatorName.KaufmanAdaptiveCorrelationOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Know Sure Thing Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="rocLength1"></param>
    /// <param name="rocLength2"></param>
    /// <param name="rocLength3"></param>
    /// <param name="rocLength4"></param>
    /// <param name="signalLength"></param>
    /// <param name="weight1"></param>
    /// <param name="weight2"></param>
    /// <param name="weight3"></param>
    /// <param name="weight4"></param>
    /// <returns></returns>
    public static StockData CalculateKnowSureThing(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 10,
        int length2 = 10, int length3 = 10, int length4 = 15, int rocLength1 = 10, int rocLength2 = 15, int rocLength3 = 20, int rocLength4 = 30,
        int signalLength = 9, decimal weight1 = 1, decimal weight2 = 2, decimal weight3 = 3, decimal weight4 = 4)
    {
        List<decimal> kstList = new();
        List<Signal> signalsList = new();

        var roc1List = CalculateRateOfChange(stockData, rocLength1).CustomValuesList;
        var roc2List = CalculateRateOfChange(stockData, rocLength2).CustomValuesList;
        var roc3List = CalculateRateOfChange(stockData, rocLength3).CustomValuesList;
        var roc4List = CalculateRateOfChange(stockData, rocLength4).CustomValuesList;
        var roc1SmaList = GetMovingAverageList(stockData, maType, length1, roc1List);
        var roc2SmaList = GetMovingAverageList(stockData, maType, length2, roc2List);
        var roc3SmaList = GetMovingAverageList(stockData, maType, length3, roc3List);
        var roc4SmaList = GetMovingAverageList(stockData, maType, length4, roc4List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal roc1 = roc1SmaList.ElementAtOrDefault(i);
            decimal roc2 = roc2SmaList.ElementAtOrDefault(i);
            decimal roc3 = roc3SmaList.ElementAtOrDefault(i);
            decimal roc4 = roc4SmaList.ElementAtOrDefault(i);

            decimal kst = (roc1 * weight1) + (roc2 * weight2) + (roc3 * weight3) + (roc4 * weight4);
            kstList.Add(kst);
        }

        var kstSignalList = GetMovingAverageList(stockData, maType, signalLength, kstList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal kst = kstList.ElementAtOrDefault(i);
            decimal kstSignal = kstSignalList.ElementAtOrDefault(i);
            decimal prevKst = i >= 1 ? kstList.ElementAtOrDefault(i - 1) : 0;
            decimal prevKstSignal = i >= 1 ? kstSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(kst - kstSignal, prevKst - prevKstSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kst", kstList },
            { "Signal", kstSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kstList;
        stockData.IndicatorName = IndicatorName.KnowSureThing;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kase Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKaseIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 10)
    {
        List<decimal> kUpList = new();
        List<decimal> kDownList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        decimal sqrtPeriod = Sqrt(length);

        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal avgTrueRange = atrList.ElementAtOrDefault(i);
            decimal avgVolSma = volumeSmaList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal ratio = avgVolSma * sqrtPeriod;

            decimal prevKUp = kUpList.LastOrDefault();
            decimal kUp = avgTrueRange > 0 && ratio != 0 && currentLow != 0 ? prevHigh / currentLow / ratio : prevKUp;
            kUpList.Add(kUp);

            decimal prevKDown = kDownList.LastOrDefault();
            decimal kDown = avgTrueRange > 0 && ratio != 0 && prevLow != 0 ? currentHigh / prevLow / ratio : prevKDown;
            kDownList.Add(kDown);

            var signal = GetCompareSignal(kUp - kDown, prevKUp - prevKDown);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "KaseUp", kUpList },
            { "KaseDn", kDownList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.KaseIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kendall Rank Correlation Coefficient
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKendallRankCorrelationCoefficient(this StockData stockData, int length = 20)
    {
        List<decimal> tempList = new();
        List<decimal> numeratorList = new();
        List<decimal> tempLinRegList = new();
        List<decimal> pearsonCorrelationList = new();
        List<decimal> kendallCorrelationList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevKendall1 = i >= 1 ? kendallCorrelationList.ElementAtOrDefault(i - 1) : 0;
            decimal prevKendall2 = i >= 2 ? kendallCorrelationList.ElementAtOrDefault(i - 2) : 0;

            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal linReg = linRegList.ElementAtOrDefault(i);
            tempLinRegList.Add(linReg);

            var pearsonCorrelation = Correlation.Pearson(tempLinRegList.TakeLastExt(length).Select(x => (double)x),
                tempList.TakeLastExt(length).Select(x => (double)x));
            pearsonCorrelation = IsValueNullOrInfinity(pearsonCorrelation) ? 0 : pearsonCorrelation;
            pearsonCorrelationList.Add((decimal)pearsonCorrelation);

            decimal totalPairs = length * (length - 1) / 2;
            decimal numerator = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                for (int k = 0; k <= j; k++)
                {
                    decimal prevValueJ = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                    decimal prevValueK = i >= k ? inputList.ElementAtOrDefault(i - k) : 0;
                    decimal prevLinRegJ = i >= j ? linRegList.ElementAtOrDefault(i - j) : 0;
                    decimal prevLinRegK = i >= k ? linRegList.ElementAtOrDefault(i - k) : 0;
                    numerator += Math.Sign(prevLinRegJ - prevLinRegK) * Math.Sign(prevValueJ - prevValueK);
                }
            }

            decimal kendallCorrelation = numerator / totalPairs;
            kendallCorrelationList.Add(kendallCorrelation);

            var signal = GetCompareSignal(kendallCorrelation - prevKendall1, prevKendall1 - prevKendall2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Krcc", kendallCorrelationList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kendallCorrelationList;
        stockData.IndicatorName = IndicatorName.KendallRankCorrelationCoefficient;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kwan Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateKwanIndicator(this StockData stockData, MovingAvgType maType, int length = 9, int smoothLength = 2)
    {
        List<decimal> vrList = new();
        List<decimal> prevList = new();
        List<decimal> knrpList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal priorClose = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal mom = priorClose != 0 ? currentClose / priorClose * 100 : 0;
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal hh = highestList.ElementAtOrDefault(i);
            decimal ll = lowestList.ElementAtOrDefault(i);
            decimal sto = hh - ll != 0 ? (currentClose - ll) / (hh - ll) * 100 : 0;
            decimal prevVr = i >= smoothLength ? vrList.ElementAtOrDefault(i - smoothLength) : 0;
            decimal prevKnrp1 = i >= 1 ? knrpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevKnrp2 = i >= 2 ? knrpList.ElementAtOrDefault(i - 2) : 0;

            decimal vr = mom != 0 ? sto * rsi / mom : 0;
            vrList.Add(vr);

            decimal prev = prevVr;
            prevList.Add(prev);

            decimal vrSum = prevList.Sum();
            decimal knrp = vrSum / smoothLength;
            knrpList.Add(knrp);

            var signal = GetCompareSignal(knrp - prevKnrp1, prevKnrp1 - prevKnrp2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ki", knrpList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = knrpList;
        stockData.IndicatorName = IndicatorName.KwanIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kaufman Stress Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="marketData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKaufmanStressIndicator(this StockData stockData, StockData marketData, int length = 60)
    {
        List<decimal> svList = new();
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList1, highList1, lowList1, _, _) = GetInputValuesList(stockData);
        var (inputList2, highList2, lowList2, _, _) = GetInputValuesList(marketData);
        var (highestList1, lowestList1) = GetMaxAndMinValuesList(highList1, lowList1, length);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(highList2, lowList2, length);

        if (stockData.Count == marketData.Count)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal highestHigh1 = highestList1.ElementAtOrDefault(i);
                decimal lowestLow1 = lowestList1.ElementAtOrDefault(i);
                decimal highestHigh2 = highestList2.ElementAtOrDefault(i);
                decimal lowestLow2 = lowestList2.ElementAtOrDefault(i);
                decimal currentValue1 = inputList1.ElementAtOrDefault(i);
                decimal currentValue2 = inputList2.ElementAtOrDefault(i);
                decimal prevSv1 = i >= 1 ? svList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSv2 = i >= 2 ? svList.ElementAtOrDefault(i - 2) : 0;
                decimal r1 = highestHigh1 - lowestLow1;
                decimal r2 = highestHigh2 - lowestLow2;
                decimal s1 = r1 != 0 ? (currentValue1 - lowestLow1) / r1 : 50;
                decimal s2 = r2 != 0 ? (currentValue2 - lowestLow2) / r2 : 50;

                decimal d = s1 - s2;
                dList.Add(d);

                var list = dList.TakeLastExt(length).ToList();
                decimal highestD = list.Max();
                decimal lowestD = list.Min();
                decimal r11 = highestD - lowestD;

                decimal sv = r11 != 0 ? MinOrMax(100 * (d - lowestD) / r11, 100, 0) : 50;
                svList.Add(sv);

                var signal = GetRsiSignal(sv - prevSv1, prevSv1 - prevSv2, sv, prevSv1, 90, 10);
                signalsList.Add(signal);
            }
        }

        stockData.OutputValues = new()
        {
            { "Ksi", svList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = svList;
        stockData.IndicatorName = IndicatorName.KaufmanStressIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vostro Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static StockData CalculateVostroIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length1 = 5, int length2 = 100, decimal level = 8)
    {
        List<decimal> tempList = new();
        List<decimal> rangeList = new();
        List<decimal> iBuff116List = new();
        List<decimal> iBuff112List = new();
        List<decimal> iBuff109List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var wmaList = GetMovingAverageList(stockData, maType, length2, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal wma = wmaList.ElementAtOrDefault(i);
            decimal prevBuff109_1 = i >= 1 ? iBuff109List.ElementAtOrDefault(i - 1) : 0;
            decimal prevBuff109_2 = i >= 2 ? iBuff109List.ElementAtOrDefault(i - 2) : 0;

            decimal medianPrice = inputList.ElementAtOrDefault(i);
            tempList.Add(medianPrice);

            decimal range = currentHigh - currentLow;
            rangeList.Add(range);

            decimal gd120 = tempList.TakeLastExt(length1).Sum();
            decimal gd128 = gd120 * 0.2m;
            decimal gd121 = rangeList.TakeLastExt(length1).Sum();
            decimal gd136 = gd121 * 0.2m * 0.2m;

            decimal prevIBuff116 = iBuff116List.LastOrDefault();
            decimal iBuff116 = gd136 != 0 ? (currentLow - gd128) / gd136 : 0;
            iBuff116List.Add(iBuff116);

            decimal prevIBuff112 = iBuff112List.LastOrDefault();
            decimal iBuff112 = gd136 != 0 ? (currentHigh - gd128) / gd136 : 0;
            iBuff112List.Add(iBuff112);

            decimal iBuff108 = iBuff112 > level && currentHigh > wma ? 90 : iBuff116 < -level && currentLow < wma ? -90 : 0;
            decimal iBuff109 = (iBuff112 > level && prevIBuff112 > level) || (iBuff116 < -level && prevIBuff116 < -level) ? 0 : iBuff108;
            iBuff109List.Add(iBuff109);

            var signal = GetRsiSignal(iBuff109 - prevBuff109_1, prevBuff109_1 - prevBuff109_2, iBuff109, prevBuff109_1, 80, -80);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vi", iBuff109List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = iBuff109List;
        stockData.IndicatorName = IndicatorName.VostroIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Price Confirmation Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVolumePriceConfirmationIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int fastLength = 5, int slowLength = 20, int length = 8)
    {
        List<decimal> vpciList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var vwmaShortList = GetMovingAverageList(stockData, MovingAvgType.VolumeWeightedMovingAverage, fastLength, inputList);
        var vwmaLongList = GetMovingAverageList(stockData, MovingAvgType.VolumeWeightedMovingAverage, slowLength, inputList);
        var volumeSmaShortList = GetMovingAverageList(stockData, maType, fastLength, volumeList);
        var volumeSmaLongList = GetMovingAverageList(stockData, maType, slowLength, volumeList);
        var smaShortList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var smaLongList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vwmaLong = vwmaLongList.ElementAtOrDefault(i);
            decimal vwmaShort = vwmaShortList.ElementAtOrDefault(i);
            decimal volumeSmaLong = volumeSmaLongList.ElementAtOrDefault(i);
            decimal volumeSmaShort = volumeSmaShortList.ElementAtOrDefault(i);
            decimal smaLong = smaLongList.ElementAtOrDefault(i);
            decimal smaShort = smaShortList.ElementAtOrDefault(i);
            decimal vpc = vwmaLong - smaLong;
            decimal vpr = smaShort != 0 ? vwmaShort / smaShort : 0;
            decimal vm = volumeSmaLong != 0 ? volumeSmaShort / volumeSmaLong : 0;

            decimal vpci = vpc * vpr * vm;
            vpciList.Add(vpci);
        }

        var vpciSmaList = GetMovingAverageList(stockData, maType, length, vpciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vpci = vpciList.ElementAtOrDefault(i);
            decimal vpciSma = vpciSmaList.ElementAtOrDefault(i);
            decimal prevVpci = i >= 1 ? vpciList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVpciSma = i >= 1 ? vpciSmaList.ElementAtOrDefault(i - 1) : 0;

            Signal signal = GetCompareSignal(vpci - vpciSma, prevVpci - prevVpciSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vpci", vpciList },
            { "Signal", vpciSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vpciList;
        stockData.IndicatorName = IndicatorName.VolumePriceConfirmationIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Positive Negative Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateVolumePositiveNegativeIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, InputName inputName = InputName.TypicalPrice, int length = 30,
        int smoothLength = 3)
    {
        List<decimal> vmpList = new();
        List<decimal> vmnList = new();
        List<decimal> vpnList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

        var mavList = GetMovingAverageList(stockData, maType, length, volumeList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mav = mavList.ElementAtOrDefault(i);
            mav = mav > 0 ? mav : 1;
            decimal tp = inputList.ElementAtOrDefault(i);
            decimal prevTp = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal atr = atrList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal mf = tp - prevTp;
            decimal mc = 0.1m * atr;

            decimal vmp = mf > mc ? currentVolume : 0;
            vmpList.Add(vmp);

            decimal vmn = mf < -mc ? currentVolume : 0;
            vmnList.Add(vmn);

            decimal vn = vmnList.TakeLastExt(length).Sum();
            decimal vp = vmpList.TakeLastExt(length).Sum();

            decimal vpn = mav != 0 ? (vp - vn) / mav / length * 100 : 0;
            vpnList.Add(vpn);
        }

        var vpnEmaList = GetMovingAverageList(stockData, maType, smoothLength, vpnList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vpnEma = vpnEmaList.ElementAtOrDefault(i);
            decimal prevVpnEma = i >= 1 ? vpnEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(vpnEma, prevVpnEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vpni", vpnList },
            { "Signal", vpnEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vpnList;
        stockData.IndicatorName = IndicatorName.VolumePositiveNegativeIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Accumulation Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVolumeAccumulationOscillator(this StockData stockData, int length = 14)
    {
        List<decimal> vaoList = new();
        List<decimal> vaoSumList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal medianValue = (currentHigh + currentLow) / 2;

            decimal vao = currentValue != medianValue ? currentVolume * (currentValue - medianValue) : currentVolume;
            vaoList.Add(vao);

            decimal prevVaoSum = vaoSumList.LastOrDefault();
            decimal vaoSum = vaoList.TakeLastExt(length).Average();
            vaoSumList.Add(vaoSum);

            var signal = GetCompareSignal(vaoSum, prevVaoSum);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vao", vaoSumList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vaoSumList;
        stockData.IndicatorName = IndicatorName.VolumeAccumulationOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Accumulation Percent
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVolumeAccumulationPercent(this StockData stockData, int length = 10)
    {
        List<decimal> vapcList = new();
        List<decimal> tvaList = new();
        List<decimal> tempVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);

            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            tempVolumeList.Add(currentVolume);

            decimal xt = currentHigh - currentLow != 0 ? ((2 * currentClose) - currentHigh - currentLow) / (currentHigh - currentLow) : 0;
            decimal tva = currentVolume * xt;
            tvaList.Add(tva);

            decimal volumeSum = tempVolumeList.TakeLastExt(length).Sum();
            decimal tvaSum = tvaList.TakeLastExt(length).Sum();

            decimal prevVapc = vapcList.LastOrDefault();
            decimal vapc = volumeSum != 0 ? MinOrMax(100 * tvaSum / volumeSum, 100, 0) : 0;
            vapcList.Add(vapc);

            var signal = GetCompareSignal(vapc, prevVapc);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vapc", vapcList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vapcList;
        stockData.IndicatorName = IndicatorName.VolumeAccumulationPercent;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Flow Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="signalLength"></param>
    /// <param name="smoothLength"></param>
    /// <param name="coef"></param>
    /// <param name="vcoef"></param>
    /// <returns></returns>
    public static StockData CalculateVolumeFlowIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        InputName inputName = InputName.TypicalPrice, int length1 = 130, int length2 = 30, int signalLength = 5, int smoothLength = 3,
        decimal coef = 0.2m, decimal vcoef = 2.5m)
    {
        List<decimal> interList = new();
        List<decimal> tempList = new();
        List<decimal> vcpList = new();
        List<decimal> vcpVaveSumList = new();
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        var smaVolumeList = GetMovingAverageList(stockData, maType, length1, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal inter = currentValue > 0 && prevValue > 0 ? Log(currentValue) - Log(prevValue) : 0;
            interList.Add(inter);
        }

        stockData.CustomValuesList = interList;
        var vinterList = CalculateStandardDeviationVolatility(stockData, length2).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vinter = vinterList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevVave = tempList.LastOrDefault();
            decimal vave = smaVolumeList.ElementAtOrDefault(i);
            tempList.Add(vave);

            decimal cutoff = currentClose * vinter * coef;
            decimal vmax = prevVave * vcoef;
            decimal vc = Math.Min(currentVolume, vmax);
            decimal mf = currentValue - prevValue;

            decimal vcp = mf > cutoff ? vc : mf < cutoff * -1 ? vc * -1 : mf > 0 ? vc : mf < 0 ? vc * -1 : 0;
            vcpList.Add(vcp);

            decimal vcpSum = vcpList.TakeLastExt(length1).Sum();
            decimal vcpVaveSum = vave != 0 ? vcpSum / vave : 0;
            vcpVaveSumList.Add(vcpVaveSum);
        }

        var vfiList = GetMovingAverageList(stockData, maType, smoothLength, vcpVaveSumList);
        var vfiEmaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, signalLength, vfiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vfi = vfiList.ElementAtOrDefault(i);
            decimal vfima = vfiEmaList.ElementAtOrDefault(i);

            decimal prevD = dList.LastOrDefault();
            decimal d = vfi - vfima;
            dList.Add(d);

            var signal = GetCompareSignal(d, prevD);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vfi", vfiList },
            { "Signal", vfiEmaList },
            { "Histogram", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vfiList;
        stockData.IndicatorName = IndicatorName.VolumeFlowIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Value Chart Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateValueChartIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
       InputName inputName = InputName.MedianPrice, int length = 5)
    {
        List<decimal> vOpenList = new();
        List<decimal> vHighList = new();
        List<decimal> vLowList = new();
        List<decimal> vCloseList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        int varp = MinOrMax((int)Math.Ceiling((decimal)length / 5));

        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, varp);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal prevClose1 = i >= 1 ? closeList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHighest1 = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowest1 = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevClose2 = i >= 2 ? closeList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHighest2 = i >= 2 ? highestList.ElementAtOrDefault(i - 2) : 0;
            decimal prevLowest2 = i >= 2 ? lowestList.ElementAtOrDefault(i - 2) : 0;
            decimal prevClose3 = i >= 3 ? closeList.ElementAtOrDefault(i - 3) : 0;
            decimal prevHighest3 = i >= 3 ? highestList.ElementAtOrDefault(i - 3) : 0;
            decimal prevLowest3 = i >= 3 ? lowestList.ElementAtOrDefault(i - 3) : 0;
            decimal prevClose4 = i >= 4 ? closeList.ElementAtOrDefault(i - 4) : 0;
            decimal prevHighest4 = i >= 4 ? highestList.ElementAtOrDefault(i - 4) : 0;
            decimal prevLowest4 = i >= 4 ? lowestList.ElementAtOrDefault(i - 4) : 0;
            decimal prevClose5 = i >= 5 ? closeList.ElementAtOrDefault(i - 5) : 0;
            decimal mba = smaList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal vara = highest - lowest;
            decimal varr1 = vara == 0 && varp == 1 ? Math.Abs(currentClose - prevClose1) : vara;
            decimal varb = prevHighest1 - prevLowest1;
            decimal varr2 = varb == 0 && varp == 1 ? Math.Abs(prevClose1 - prevClose2) : varb;
            decimal varc = prevHighest2 - prevLowest2;
            decimal varr3 = varc == 0 && varp == 1 ? Math.Abs(prevClose2 - prevClose3) : varc;
            decimal vard = prevHighest3 - prevLowest3;
            decimal varr4 = vard == 0 && varp == 1 ? Math.Abs(prevClose3 - prevClose4) : vard;
            decimal vare = prevHighest4 - prevLowest4;
            decimal varr5 = vare == 0 && varp == 1 ? Math.Abs(prevClose4 - prevClose5) : vare;
            decimal cdelta = Math.Abs(currentClose - prevClose1);
            decimal var0 = cdelta > currentHigh - currentLow || currentHigh == currentLow ? cdelta : currentHigh - currentLow;
            decimal lRange = (varr1 + varr2 + varr3 + varr4 + varr5) / 5 * 0.2m;

            decimal vClose = lRange != 0 ? (currentClose - mba) / lRange : 0;
            vCloseList.Add(vClose);

            decimal vOpen = lRange != 0 ? (currentOpen - mba) / lRange : 0;
            vOpenList.Add(vOpen);

            decimal vHigh = lRange != 0 ? (currentHigh - mba) / lRange : 0;
            vHighList.Add(vHigh);

            decimal vLow = lRange != 0 ? (currentLow - mba) / lRange : 0;
            vLowList.Add(vLow);
        }

        var vValueEmaList = GetMovingAverageList(stockData, maType, length, vCloseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vValue = vCloseList.ElementAtOrDefault(i);
            decimal vValueEma = vValueEmaList.ElementAtOrDefault(i);
            decimal prevVvalue = i >= 1 ? vCloseList.ElementAtOrDefault(i - 1) : 0;
            decimal prevVValueEma = i >= 1 ? vValueEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetRsiSignal(vValue - vValueEma, prevVvalue - prevVValueEma, vValue, prevVvalue, 4, -4);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "vClose", vCloseList },
            { "vOpen", vOpenList },
            { "vHigh", vHighList },
            { "vLow", vLowList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ValueChartIndicator;

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
        var volaList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
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
    /// Calculates the Volatility Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="breakoutLevel"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilityRatio(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14, decimal breakoutLevel = 0.5m)
    {
        List<decimal> vrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length - 1);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevHighest = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowest = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;
            decimal priorValue = i >= length + 1 ? inputList.ElementAtOrDefault(i - (length + 1)) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            decimal max = priorValue != 0 ? Math.Max(prevHighest, priorValue) : prevHighest;
            decimal min = priorValue != 0 ? Math.Min(prevLowest, priorValue) : prevLowest;

            decimal vr = max - min != 0 ? tr / (max - min) : 0;
            vrList.Add(vr);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, vr, breakoutLevel);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vr", vrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vrList;
        stockData.IndicatorName = IndicatorName.VolatilityRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vervoort Smoothed Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="smoothLength"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateVervoortSmoothedOscillator(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        int length1 = 18, int length2 = 30, int length3 = 2, int smoothLength = 3, decimal stdDevMult = 2)
    {
        List<decimal> rainbowList = new();
        List<decimal> zlrbList = new();
        List<decimal> zlrbpercbList = new();
        List<decimal> rbcList = new();
        List<decimal> fastKList = new();
        List<decimal> skList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        var r1List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, closeList);
        var r2List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r1List);
        var r3List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r2List);
        var r4List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r3List);
        var r5List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r4List);
        var r6List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r5List);
        var r7List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r6List);
        var r8List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r7List);
        var r9List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r8List);
        var r10List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, r9List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal r1 = r1List.ElementAtOrDefault(i);
            decimal r2 = r2List.ElementAtOrDefault(i);
            decimal r3 = r3List.ElementAtOrDefault(i);
            decimal r4 = r4List.ElementAtOrDefault(i);
            decimal r5 = r5List.ElementAtOrDefault(i);
            decimal r6 = r6List.ElementAtOrDefault(i);
            decimal r7 = r7List.ElementAtOrDefault(i);
            decimal r8 = r8List.ElementAtOrDefault(i);
            decimal r9 = r9List.ElementAtOrDefault(i);
            decimal r10 = r10List.ElementAtOrDefault(i);

            decimal rainbow = ((5 * r1) + (4 * r2) + (3 * r3) + (2 * r4) + r5 + r6 + r7 + r8 + r9 + r10) / 20;
            rainbowList.Add(rainbow);
        }

        var ema1List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, smoothLength, rainbowList);
        var ema2List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, smoothLength, ema1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema1 = ema1List.ElementAtOrDefault(i);
            decimal ema2 = ema2List.ElementAtOrDefault(i);

            decimal zlrb = (2 * ema1) - ema2;
            zlrbList.Add(zlrb);
        }

        var tzList = GetMovingAverageList(stockData, MovingAvgType.TripleExponentialMovingAverage, smoothLength, zlrbList);
        stockData.CustomValuesList = tzList;
        var hwidthList = CalculateStandardDeviationVolatility(stockData, length1).CustomValuesList;
        var wmatzList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, length1, tzList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentTypicalPrice = inputList.ElementAtOrDefault(i);
            decimal rainbow = rainbowList.ElementAtOrDefault(i);
            decimal tz = tzList.ElementAtOrDefault(i);
            decimal hwidth = hwidthList.ElementAtOrDefault(i);
            decimal wmatz = wmatzList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);

            decimal prevZlrbpercb = zlrbpercbList.LastOrDefault();
            decimal zlrbpercb = hwidth != 0 ? (tz + (stdDevMult * hwidth) - wmatz) / (2 * stdDevMult * hwidth * 100) : 0;
            zlrbpercbList.Add(zlrbpercb);

            decimal rbc = (rainbow + currentTypicalPrice) / 2;
            rbcList.Add(rbc);

            decimal lowestRbc = rbcList.TakeLastExt(length2).Min();
            decimal nom = rbc - lowest;
            decimal den = highest - lowestRbc;

            decimal fastK = den != 0 ? MinOrMax(100 * nom / den, 100, 0) : 0;
            fastKList.Add(fastK);

            decimal prevSk = skList.LastOrDefault();
            decimal sk = fastKList.TakeLastExt(smoothLength).Average();
            skList.Add(sk);

            var signal = GetConditionSignal(sk > prevSk && zlrbpercb > prevZlrbpercb, sk < prevSk && zlrbpercb < prevZlrbpercb);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vso", zlrbpercbList },
            { "Sk", skList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zlrbpercbList;
        stockData.IndicatorName = IndicatorName.VervoortSmoothedOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vervoort Heiken Ashi Long Term Candlestick Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateVervoortHeikenAshiLongTermCandlestickOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.TripleExponentialMovingAverage, InputName inputName = InputName.FullTypicalPrice, int length = 55,
        decimal factor = 1.1m)
    {
        List<decimal> haoList = new();
        List<decimal> hacList = new();
        List<decimal> medianPriceList = new();
        List<bool> keepN1List = new();
        List<bool> keepAll1List = new();
        List<bool> keepN2List = new();
        List<bool> keepAll2List = new();
        List<bool> utrList = new();
        List<bool> dtrList = new();
        List<decimal> hacoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevHao = haoList.LastOrDefault();
            decimal hao = (prevValue + prevHao) / 2;
            haoList.Add(hao);

            decimal hac = (currentValue + hao + Math.Max(currentHigh, hao) + Math.Min(currentLow, hao)) / 4;
            hacList.Add(hac);

            decimal medianPrice = (currentHigh + currentLow) / 2;
            medianPriceList.Add(medianPrice);
        }

        var tacList = GetMovingAverageList(stockData, maType, length, hacList);
        var thl2List = GetMovingAverageList(stockData, maType, length, medianPriceList);
        var tacTemaList = GetMovingAverageList(stockData, maType, length, tacList);
        var thl2TemaList = GetMovingAverageList(stockData, maType, length, thl2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tac = tacList.ElementAtOrDefault(i);
            decimal tacTema = tacTemaList.ElementAtOrDefault(i);
            decimal thl2 = thl2List.ElementAtOrDefault(i);
            decimal thl2Tema = thl2TemaList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal hac = hacList.ElementAtOrDefault(i);
            decimal hao = haoList.ElementAtOrDefault(i);
            decimal prevHac = i >= 1 ? hacList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHao = i >= 1 ? haoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevClose = i >= 1 ? closeList.ElementAtOrDefault(i - 1) : 0;
            decimal hacSmooth = (2 * tac) - tacTema;
            decimal hl2Smooth = (2 * thl2) - thl2Tema;

            bool shortCandle = Math.Abs(currentClose - currentOpen) < (currentHigh - currentLow) * factor;
            bool prevKeepN1 = keepN1List.LastOrDefault();
            bool keepN1 = ((hac >= hao) && (prevHac >= prevHao)) || currentClose >= hac || currentHigh > prevHigh || currentLow > prevLow || hl2Smooth >= hacSmooth;
            keepN1List.Add(keepN1);

            bool prevKeepAll1 = keepAll1List.LastOrDefault();
            bool keepAll1 = keepN1 || (prevKeepN1 && (currentClose >= currentOpen || currentClose >= prevClose));
            keepAll1List.Add(keepAll1);

            bool keep13 = shortCandle && currentHigh >= prevLow;
            bool prevUtr = utrList.LastOrDefault();
            bool utr = keepAll1 || (prevKeepAll1 && keep13);
            utrList.Add(utr);

            bool prevKeepN2 = keepN2List.LastOrDefault();
            bool keepN2 = (hac < hao && prevHac < prevHao) || hl2Smooth < hacSmooth;
            keepN2List.Add(keepN2);

            bool keep23 = shortCandle && currentLow <= prevHigh;
            bool prevKeepAll2 = keepAll2List.LastOrDefault();
            bool keepAll2 = keepN2 || (prevKeepN2 && (currentClose < currentOpen || currentClose < prevClose));
            keepAll2List.Add(keepAll2);

            bool prevDtr = dtrList.LastOrDefault();
            bool dtr = keepAll2 || prevKeepAll2 && keep23;
            dtrList.Add(dtr);

            bool upw = dtr == false && prevDtr && utr;
            bool dnw = utr == false && prevUtr && dtr;
            decimal prevHaco = hacoList.LastOrDefault();
            decimal haco = upw ? 1 : dnw ? -1 : prevHaco;
            hacoList.Add(haco);

            var signal = GetCompareSignal(haco, prevHaco);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vhaltco", hacoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hacoList;
        stockData.IndicatorName = IndicatorName.VervoortHeikenAshiLongTermCandlestickOscillator;

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
    /// Calculates the Vix Trading System
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="maxCount"></param>
    /// <param name="minCount"></param>
    /// <returns></returns>
    public static StockData CalculateVixTradingSystem(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 50, decimal maxCount = 11, decimal minCount = -11)
    {
        List<decimal> countList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal vixts = smaList.ElementAtOrDefault(i);

            decimal prevCount = countList.LastOrDefault();
            decimal count = currentValue > vixts && prevCount >= 0 ? prevCount + 1 : currentValue <= vixts && prevCount <= 0 ?
                prevCount - 1 : prevCount;
            countList.Add(count);

            var signal = GetBullishBearishSignal(count - maxCount - 1, prevCount - maxCount - 1, count - minCount + 1,
                prevCount - minCount + 1, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vix", countList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = countList;
        stockData.IndicatorName = IndicatorName.VixTradingSystem;

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
    /// Calculates the Varadi Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVaradiOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14)
    {
        List<decimal> dvoList = new();
        List<decimal> ratioList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal median = (currentHigh + currentLow) / 2;

            decimal ratio = median != 0 ? currentValue / median : 0;
            ratioList.Add(ratio);
        }

        var aList = GetMovingAverageList(stockData, maType, length, ratioList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = aList.ElementAtOrDefault(i);
            decimal prevDvo1 = i >= 1 ? dvoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevDvo2 = i >= 2 ? dvoList.ElementAtOrDefault(i - 2) : 0;

            decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : 0;
            tempList.Add(prevA);

            decimal dvo = MinOrMax((decimal)tempList.TakeLastExt(length).Where(i => i <= a).Count() / length * 100, 100, 0);
            dvoList.Add(dvo);

            var signal = GetRsiSignal(dvo - prevDvo1, prevDvo1 - prevDvo2, dvo, prevDvo1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vo", dvoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dvoList;
        stockData.IndicatorName = IndicatorName.VaradiOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vanilla ABCD Pattern
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateVanillaABCDPattern(this StockData stockData)
    {
        List<decimal> osList = new();
        List<decimal> fList = new();
        List<decimal> dosList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevValue3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            decimal up = prevValue3 > prevValue2 && prevValue1 > prevValue2 && currentValue < prevValue2 ? 1 : 0;
            decimal dn = prevValue3 < prevValue2 && prevValue1 < prevValue2 && currentValue > prevValue2 ? 1 : 0;

            decimal prevOs = osList.LastOrDefault();
            decimal os = up == 1 ? 1 : dn == 1 ? 0 : prevOs;
            osList.Add(os);

            decimal prevF = fList.LastOrDefault();
            decimal f = os == 1 && currentValue > currentOpen ? 1 : os == 0 && currentValue < currentOpen ? 0 : prevF;
            fList.Add(f);

            decimal prevDos = dosList.LastOrDefault();
            decimal dos = os - prevOs;
            dosList.Add(dos);

            var signal = GetCompareSignal(dos, prevDos);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vabcd", dosList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dosList;
        stockData.IndicatorName = IndicatorName.VanillaABCDPattern;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vervoort Heiken Ashi Candlestick Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVervoortHeikenAshiCandlestickOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ZeroLagTripleExponentialMovingAverage, InputName inputName = InputName.FullTypicalPrice, int length = 34)
    {
        List<decimal> haoList = new();
        List<decimal> hacList = new();
        List<decimal> medianPriceList = new();
        List<bool> dnKeepingList = new();
        List<bool> dnKeepAllList = new();
        List<bool> dnTrendList = new();
        List<bool> upKeepingList = new();
        List<bool> upKeepAllList = new();
        List<bool> upTrendList = new();
        List<decimal> hacoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevHao = haoList.LastOrDefault();
            decimal hao = (prevValue + prevHao) / 2;
            haoList.Add(hao);

            decimal hac = (currentValue + hao + Math.Max(currentHigh, hao) + Math.Min(currentLow, hao)) / 4;
            hacList.Add(hac);

            decimal medianPrice = (currentHigh + currentLow) / 2;
            medianPriceList.Add(medianPrice);
        }

        var tma1List = GetMovingAverageList(stockData, maType, length, hacList);
        var tma2List = GetMovingAverageList(stockData, maType, length, tma1List);
        var tma12List = GetMovingAverageList(stockData, maType, length, medianPriceList);
        var tma22List = GetMovingAverageList(stockData, maType, length, tma12List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tma1 = tma1List.ElementAtOrDefault(i);
            decimal tma2 = tma2List.ElementAtOrDefault(i);
            decimal tma12 = tma12List.ElementAtOrDefault(i);
            decimal tma22 = tma22List.ElementAtOrDefault(i);
            decimal hao = haoList.ElementAtOrDefault(i);
            decimal hac = hacList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal prevHao = i >= 1 ? haoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHac = i >= 1 ? hacList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevClose = i >= 1 ? closeList.ElementAtOrDefault(i - 1) : 0;
            decimal diff = tma1 - tma2;
            decimal zlHa = tma1 + diff;
            decimal diff2 = tma12 - tma22;
            decimal zlCl = tma12 + diff2;
            decimal zlDiff = zlCl - zlHa;
            bool dnKeep1 = hac < hao && prevHac < prevHao;
            bool dnKeep2 = zlDiff < 0;
            bool dnKeep3 = Math.Abs(currentClose - currentOpen) < (currentHigh - currentLow) * 0.35m && currentLow <= prevHigh;

            bool prevDnKeeping = dnKeepingList.LastOrDefault();
            bool dnKeeping = dnKeep1 || dnKeep2;
            dnKeepingList.Add(dnKeeping);

            bool prevDnKeepAll = dnKeepAllList.LastOrDefault();
            bool dnKeepAll = (dnKeeping || prevDnKeeping) && ((currentClose < currentOpen) || (currentClose < prevClose));
            dnKeepAllList.Add(dnKeepAll);

            bool prevDnTrend = dnTrendList.LastOrDefault();
            bool dnTrend = dnKeepAll || (prevDnKeepAll && dnKeep3);
            dnTrendList.Add(dnTrend);

            bool upKeep1 = hac >= hao && prevHac >= prevHao;
            bool upKeep2 = zlDiff >= 0;
            bool upKeep3 = Math.Abs(currentClose - currentOpen) < (currentHigh - currentLow) * 0.35m && currentHigh >= prevLow;

            bool prevUpKeeping = upKeepingList.LastOrDefault();
            bool upKeeping = upKeep1 || upKeep2;
            upKeepingList.Add(upKeeping);

            bool prevUpKeepAll = upKeepAllList.LastOrDefault();
            bool upKeepAll = (upKeeping || prevUpKeeping) && ((currentClose >= currentOpen) || (currentClose >= prevClose));
            upKeepAllList.Add(upKeepAll);

            bool prevUpTrend = upTrendList.LastOrDefault();
            bool upTrend = upKeepAll || (prevUpKeepAll && upKeep3);
            upTrendList.Add(upTrend);

            bool upw = dnTrend == false && prevDnTrend && upTrend;
            bool dnw = upTrend == false && prevUpTrend && dnTrend;

            decimal prevHaco = hacoList.LastOrDefault();
            decimal haco = upw ? 1 : dnw ? -1 : prevHaco;
            hacoList.Add(haco);

            var signal = GetCompareSignal(haco, prevHaco);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vhaco", hacoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hacoList;
        stockData.IndicatorName = IndicatorName.VervoortHeikenAshiCandlestickOscillator;

        return stockData;
    }
}
