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
    /// Calculates the index of the force.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateForceIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<double> rawForceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double rawForce = MinPastValues(i, 1, currentValue - prevValue) * currentVolume;
            rawForceList.AddRounded(rawForce);
        }

        var forceList = GetMovingAverageList(stockData, maType, length, rawForceList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double force = forceList[i];
            double prevForce1 = i >= 1 ? forceList[i - 1] : 0;
            double prevForce2 = i >= 2 ? forceList[i - 2] : 0;

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

    /// <summary>
    /// Calculates the Money Flow Index
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateMoneyFlowIndex(this StockData stockData, InputName inputName = InputName.TypicalPrice, int length = 14)
    {
        List<double> mfiList = new();
        List<double> posMoneyFlowList = new();
        List<double> negMoneyFlowList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentVolume = volumeList[i];
            double typicalPrice = inputList[i];
            double prevTypicalPrice = i >= 1 ? inputList[i - 1] : 0;
            double prevMfi1 = i >= 1 ? mfiList[i - 1] : 0;
            double prevMfi2 = i >= 2 ? mfiList[i - 2] : 0;
            double rawMoneyFlow = typicalPrice * currentVolume;

            double posMoneyFlow = i >= 1 && typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
            posMoneyFlowList.AddRounded(posMoneyFlow);

            double negMoneyFlow = i >= 1 && typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
            negMoneyFlowList.AddRounded(negMoneyFlow);

            double posMoneyFlowTotal = posMoneyFlowList.TakeLastExt(length).Sum();
            double negMoneyFlowTotal = negMoneyFlowList.TakeLastExt(length).Sum();
            double mfiRatio = negMoneyFlowTotal != 0 ? posMoneyFlowTotal / negMoneyFlowTotal : 0;

            double mfi = negMoneyFlowTotal == 0 ? 100 : posMoneyFlowTotal == 0 ? 0 : MinOrMax(100 - (100 / (1 + mfiRatio)), 100, 0);
            mfiList.AddRounded(mfi);

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
    /// Calculates the Klinger Volume Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateKlingerVolumeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 34, int slowLength = 55, int signalLength = 13)
    {
        List<double> kvoList = new();
        List<double> trendList = new();
        List<double> dmList = new();
        List<double> cmList = new();
        List<double> vfList = new();
        List<double> kvoHistoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double mom = MinPastValues(i, 1, currentValue - prevValue);

            double prevTrend = trendList.LastOrDefault();
            double trend = mom > 0 ? 1 : mom < 0 ? -1 : prevTrend;
            trendList.AddRounded(trend);

            double prevDm = dmList.LastOrDefault();
            double dm = currentHigh - currentLow;
            dmList.AddRounded(dm);

            double prevCm = cmList.LastOrDefault();
            double cm = trend == prevTrend ? prevCm + dm : prevDm + dm;
            cmList.AddRounded(cm);

            double temp = cm != 0 ? Math.Abs((2 * (dm / cm)) - 1) : -1;
            double vf = currentVolume * temp * trend * 100;
            vfList.AddRounded(vf);
        }

        var ema34List = GetMovingAverageList(stockData, maType, fastLength, vfList);
        var ema55List = GetMovingAverageList(stockData, maType, slowLength, vfList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ema34 = ema34List[i];
            double ema55 = ema55List[i];

            double klingerOscillator = ema34 - ema55;
            kvoList.AddRounded(klingerOscillator);
        }

        var kvoSignalList = GetMovingAverageList(stockData, maType, signalLength, kvoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double klingerOscillator = kvoList[i];
            double koSignalLine = kvoSignalList[i];

            double prevKlingerOscillatorHistogram = kvoHistoList.LastOrDefault();
            double klingerOscillatorHistogram = klingerOscillator - koSignalLine;
            kvoHistoList.AddRounded(klingerOscillatorHistogram);

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
    public static StockData CalculateOnBalanceVolume(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20)
    {
        List<double> obvList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevObv = obvList.LastOrDefault();
            double obv = currentValue > prevValue ? prevObv + currentVolume : currentValue < prevValue ? prevObv - currentVolume : prevObv;
            obvList.AddRounded(obv);
        }

        var obvSignalList = GetMovingAverageList(stockData, maType, length, obvList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double obv = obvList[i];
            double prevObv = i >= 1 ? obvList[i - 1] : 0;
            double obvSig = obvSignalList[i];
            double prevObvSig = i >= 1 ? obvSignalList[i - 1] : 0;

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
    public static StockData CalculateNegativeVolumeIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 255)
    {
        List<double> nviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentVolume = volumeList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevVolume = i >= 1 ? volumeList[i - 1] : 0;
            double pctChg = CalculatePercentChange(currentClose, prevClose);

            double prevNvi = nviList.LastOrDefault();
            double nvi = currentVolume >= prevVolume ? prevNvi : prevNvi + (prevNvi * pctChg);
            nviList.AddRounded(nvi);
        }

        var nviSignalList = GetMovingAverageList(stockData, maType, length, nviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double nvi = nviList[i];
            double prevNvi = i >= 1 ? nviList[i - 1] : 0;
            double nviSignal = nviSignalList[i];
            double prevNviSignal = i >= 1 ? nviSignalList[i - 1] : 0;

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
    public static StockData CalculatePositiveVolumeIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 255)
    {
        List<double> pviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentVolume = volumeList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevVolume = i >= 1 ? volumeList[i - 1] : 0;
            double pctChg = CalculatePercentChange(currentClose, prevClose);

            double prevPvi = pviList.LastOrDefault();
            double pvi = currentVolume <= prevVolume ? prevPvi : prevPvi + (prevPvi * pctChg);
            pviList.AddRounded(pvi);
        }

        var pviSignalList = GetMovingAverageList(stockData, maType, length, pviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double pvi = pviList[i];
            double prevPvi = i >= 1 ? pviList[i - 1] : 0;
            double pviSignal = pviSignalList[i];
            double prevPviSignal = i >= 1 ? pviSignalList[i - 1] : 0;

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
    /// Calculates the Chaikin Money Flow
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateChaikinMoneyFlow(this StockData stockData, int length = 20)
    {
        List<double> chaikinMoneyFlowList = new();
        List<double> tempVolumeList = new();
        List<double> moneyFlowVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentLow = lowList[i];
            double currentHigh = highList[i];
            double currentClose = inputList[i];
            double moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
            double prevCmf1 = i >= 1 ? chaikinMoneyFlowList[i - 1] : 0;
            double prevCmf2 = i >= 2 ? chaikinMoneyFlowList[i - 2] : 0;

            double currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            double moneyFlowVolume = moneyFlowMultiplier * currentVolume;
            moneyFlowVolumeList.AddRounded(moneyFlowVolume);

            double volumeSum = tempVolumeList.TakeLastExt(length).Sum();
            double mfVolumeSum = moneyFlowVolumeList.TakeLastExt(length).Sum();

            double cmf = volumeSum != 0 ? mfVolumeSum / volumeSum : 0;
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
    public static StockData CalculateAccumulationDistributionLine(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 14)
    {
        List<double> adlList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentLow = lowList[i];
            double currentHigh = highList[i];
            double currentClose = inputList[i];
            double currentVolume = volumeList[i];
            double moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
            double moneyFlowVolume = moneyFlowMultiplier * currentVolume;

            double prevAdl = adlList.LastOrDefault();
            double adl = prevAdl + moneyFlowVolume;
            adlList.AddRounded(adl);
        }

        var adlSignalList = GetMovingAverageList(stockData, maType, length, adlList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var adl = adlList[i];
            var prevAdl = i >= 1 ? adlList[i - 1] : 0;
            var adlSignal = adlSignalList[i];
            var prevAdlSignal = i >= 1 ? adlSignalList[i - 1] : 0;

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
    /// Calculates the average money flow oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAverageMoneyFlowOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
        int length = 5, int smoothLength = 3)
    {
        List<double> chgList = new();
        List<double> rList = new();
        List<double> kList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var avgvList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double chg = MinPastValues(i, 1, currentValue - prevValue);
            chgList.AddRounded(chg);
        }

        var avgcList = GetMovingAverageList(stockData, maType, length, chgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double avgv = avgvList[i];
            double avgc = avgcList[i];

            double r = Math.Abs(avgv * avgc) > 0 ? Math.Log(Math.Abs(avgv * avgc)) * Math.Sign(avgc) : 0;
            rList.AddRounded(r);

            var list = rList.TakeLastExt(length).ToList();
            double rh = list.Max();
            double rl = list.Min();
            double rs = rh != rl ? (r - rl) / (rh - rl) * 100 : 0;

            double k = (rs * 2) - 100;
            kList.AddRounded(k);
        }

        var ksList = GetMovingAverageList(stockData, maType, smoothLength, kList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ks = ksList[i];
            double prevKs = i >= 1 ? ksList[i - 1] : 0;

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
    /// Calculates the Better Volume Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <returns></returns>
    public static StockData CalculateBetterVolumeIndicator(this StockData stockData, int length = 8, int lbLength = 2)
    {
        List<double> v1List = new();
        List<double> v2List = new();
        List<double> v3List = new();
        List<double> v4List = new();
        List<double> v5List = new();
        List<double> v6List = new();
        List<double> v7List = new();
        List<double> v8List = new();
        List<double> v9List = new();
        List<double> v10List = new();
        List<double> v11List = new();
        List<double> v12List = new();
        List<double> v13List = new();
        List<double> v14List = new();
        List<double> v15List = new();
        List<double> v16List = new();
        List<double> v17List = new();
        List<double> v18List = new();
        List<double> v19List = new();
        List<double> v20List = new();
        List<double> v21List = new();
        List<double> v22List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, lbLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentVolume = volumeList[i];
            double currentOpen = openList[i];
            double currentClose = inputList[i];
            double highLowRange = highest - lowest;
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevOpen = i >= 1 ? openList[i - 1] : 0;
            double range = CalculateTrueRange(currentHigh, currentLow, prevClose);

            double prevV1 = v1List.LastOrDefault();
            double v1 = currentClose > currentOpen ? range / ((2 * range) + currentOpen - currentClose) * currentVolume :
                currentClose < currentOpen ? (range + currentClose - currentOpen) / ((2 * range) + currentClose - currentOpen) * currentVolume :
                0.5m * currentVolume;
            v1List.AddRounded(v1);

            double prevV2 = v2List.LastOrDefault();
            double v2 = currentVolume - v1;
            v2List.AddRounded(v2);

            double prevV3 = v3List.LastOrDefault();
            double v3 = v1 + v2;
            v3List.AddRounded(v3);

            double v4 = v1 * range;
            v4List.AddRounded(v4);

            double v5 = (v1 - v2) * range;
            v5List.AddRounded(v5);

            double v6 = v2 * range;
            v6List.AddRounded(v6);

            double v7 = (v2 - v1) * range;
            v7List.AddRounded(v7);

            double v8 = range != 0 ? v1 / range : 0;
            v8List.AddRounded(v8);

            double v9 = range != 0 ? (v1 - v2) / range : 0;
            v9List.AddRounded(v9);

            double v10 = range != 0 ? v2 / range : 0;
            v10List.AddRounded(v10);

            double v11 = range != 0 ? (v2 - v1) / range : 0;
            v11List.AddRounded(v11);

            double v12 = range != 0 ? v3 / range : 0;
            v12List.AddRounded(v12);

            double v13 = v3 + prevV3;
            v13List.AddRounded(v13);

            double v14 = (v1 + prevV1) * highLowRange;
            v14List.AddRounded(v14);

            double v15 = (v1 + prevV1 - v2 - prevV2) * highLowRange;
            v15List.AddRounded(v15);

            double v16 = (v2 + prevV2) * highLowRange;
            v16List.AddRounded(v16);

            double v17 = (v2 + prevV2 - v1 - prevV1) * highLowRange;
            v17List.AddRounded(v17);

            double v18 = highLowRange != 0 ? (v1 + prevV1) / highLowRange : 0;
            v18List.AddRounded(v18);

            double v19 = highLowRange != 0 ? (v1 + prevV1 - v2 - prevV2) / highLowRange : 0;
            v19List.AddRounded(v19);

            double v20 = highLowRange != 0 ? (v2 + prevV2) / highLowRange : 0;
            v20List.AddRounded(v20);

            double v21 = highLowRange != 0 ? (v2 + prevV2 - v1 - prevV1) / highLowRange : 0;
            v21List.AddRounded(v21);

            double v22 = highLowRange != 0 ? v13 / highLowRange : 0;
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
    /// Calculates the Buff Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateBuffAverage(this StockData stockData, int fastLength = 5, int slowLength = 20)
    {
        List<double> priceVolList = new();
        List<double> fastBuffList = new();
        List<double> slowBuffList = new();
        List<double> tempVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            double priceVol = currentValue * currentVolume;
            priceVolList.AddRounded(priceVol);

            double fastBuffNum = priceVolList.TakeLastExt(fastLength).Sum();
            double fastBuffDenom = tempVolumeList.TakeLastExt(fastLength).Sum();

            double prevFastBuff = fastBuffList.LastOrDefault();
            double fastBuff = fastBuffDenom != 0 ? fastBuffNum / fastBuffDenom : 0;
            fastBuffList.AddRounded(fastBuff);

            double slowBuffNum = priceVolList.TakeLastExt(slowLength).Sum();
            double slowBuffDenom = tempVolumeList.TakeLastExt(slowLength).Sum();

            double prevSlowBuff = slowBuffList.LastOrDefault();
            double slowBuff = slowBuffDenom != 0 ? slowBuffNum / slowBuffDenom : 0;
            slowBuffList.AddRounded(slowBuff);

            var signal = GetCompareSignal(fastBuff - slowBuff, prevFastBuff - prevSlowBuff);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastBuff", fastBuffList },
            { "SlowBuff", slowBuffList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.BuffAverage;

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
        List<double> upVolList = new();
        List<double> downVolList = new();
        List<double> upDownVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double upVol = currentValue > prevValue ? currentVolume : 0;
            upVolList.AddRounded(upVol);

            double downVol = currentValue < prevValue ? currentVolume * -1 : 0;
            downVolList.AddRounded(downVol);

            double upVolSum = upVolList.TakeLastExt(length).Sum();
            double downVolSum = downVolList.TakeLastExt(length).Sum();

            double prevUpDownVol = upDownVolumeList.LastOrDefault();
            double upDownVol = downVolSum != 0 ? upVolSum / downVolSum : 0;
            upDownVolumeList.AddRounded(upDownVol);

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
    /// Calculates the Ease Of Movement
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static StockData CalculateEaseOfMovement(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14,
        double divisor = 1000000)
    {
        List<double> halfRangeList = new();
        List<double> midpointMoveList = new();
        List<double> emvList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentVolume = volumeList[i];
            double prevHalfRange = halfRangeList.LastOrDefault();
            double halfRange = (currentHigh - currentLow) * 0.5m;
            double boxRatio = currentHigh - currentLow != 0 ? currentVolume / (currentHigh - currentLow) : 0;

            double prevMidpointMove = midpointMoveList.LastOrDefault();
            double midpointMove = halfRange - prevHalfRange;
            midpointMoveList.AddRounded(midpointMove);

            double emv = boxRatio != 0 ? divisor * ((midpointMove - prevMidpointMove) / boxRatio) : 0;
            emvList.AddRounded(emv);
        }

        var emvSmaList = GetMovingAverageList(stockData, maType, length, emvList);
        var emvSignalList = GetMovingAverageList(stockData, maType, length, emvSmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double emv = emvList[i];
            double emvSignal = emvSignalList[i];
            double prevEmv = i >= 1 ? emvList[i - 1] : 0;
            double prevEmvSignal = i >= 1 ? emvSignalList[i - 1] : 0;

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
            double obvm = obvmList[i];
            double sig = sigList[i];
            double prevObvm = i >= 1 ? obvmList[i - 1] : 0;
            double prevSig = i >= 1 ? sigList[i - 1] : 0;

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
        List<double> ovrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;

            double prevOvr = ovrList.LastOrDefault();
            double ovr = currentValue > prevValue ? prevOvr + currentVolume : currentValue < prevValue ? prevOvr - currentVolume : prevOvr;
            ovrList.AddRounded(ovr);
        }

        var ovrSmaList = GetMovingAverageList(stockData, maType, signalLength, ovrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ovr = ovrList[i];
            double ovrEma = ovrSmaList[i];
            double prevOvr = i >= 1 ? ovrList[i - 1] : 0;
            double prevOvrEma = i >= 1 ? ovrSmaList[i - 1] : 0;

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
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 33, int signalLength = 4, double top = 1.1m, double bottom = 0.9m)
    {
        List<double> obvdiList = new();
        List<double> bscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var obvList = CalculateOnBalanceVolume(stockData, maType, length).CustomValuesList;
        var obvSmaList = GetMovingAverageList(stockData, maType, length, obvList);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = obvList;
        var obvStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double stdDev = stdDevList[i];
            double obvSma = obvSmaList[i];
            double obvStdDev = obvStdDevList[i];
            double aTop = currentValue - (sma - (2 * stdDev));
            double aBot = currentValue + (2 * stdDev) - (sma - (2 * stdDev));
            double obv = obvList[i];
            double a = aBot != 0 ? aTop / aBot : 0;
            double bTop = obv - (obvSma - (2 * obvStdDev));
            double bBot = obvSma + (2 * obvStdDev) - (obvSma - (2 * obvStdDev));
            double b = bBot != 0 ? bTop / bBot : 0;

            double obvdi = 1 + b != 0 ? (1 + a) / (1 + b) : 0;
            obvdiList.AddRounded(obvdi);
        }

        var obvdiEmaList = GetMovingAverageList(stockData, maType, signalLength, obvdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double obvdi = obvdiList[i];
            double obvdiEma = obvdiEmaList[i];
            double prevObvdi = i >= 1 ? obvdiList[i - 1] : 0;

            double prevBsc = bscList.LastOrDefault();
            double bsc = (prevObvdi < bottom && obvdi > bottom) || obvdi > obvdiEma ? 1 : (prevObvdi > top && obvdi < top) ||
                obvdi < bottom ? -1 : prevBsc;
            bscList.AddRounded(bsc);

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
        int length = 33, int signalLength = 4, double top = 1.1m, double bottom = 0.9m)
    {
        List<double> nvdiList = new();
        List<double> bscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var nviList = CalculateNegativeVolumeIndex(stockData, maType, length).CustomValuesList;
        var nviSmaList = GetMovingAverageList(stockData, maType, length, nviList);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = nviList;
        var nviStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double stdDev = stdDevList[i];
            double nviSma = nviSmaList[i];
            double nviStdDev = nviStdDevList[i];
            double aTop = currentValue - (sma - (2 * stdDev));
            double aBot = (currentValue + (2 * stdDev)) - (sma - (2 * stdDev));
            double nvi = nviList[i];
            double a = aBot != 0 ? aTop / aBot : 0;
            double bTop = nvi - (nviSma - (2 * nviStdDev));
            double bBot = (nviSma + (2 * nviStdDev)) - (nviSma - (2 * nviStdDev));
            double b = bBot != 0 ? bTop / bBot : 0;

            double nvdi = 1 + b != 0 ? (1 + a) / (1 + b) : 0;
            nvdiList.AddRounded(nvdi);
        }

        var nvdiEmaList = GetMovingAverageList(stockData, maType, signalLength, nvdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double nvdi = nvdiList[i];
            double nvdiEma = nvdiEmaList[i];
            double prevNvdi = i >= 1 ? nvdiList[i - 1] : 0;

            double prevBsc = bscList.LastOrDefault();
            double bsc = (prevNvdi < bottom && nvdi > bottom) || nvdi > nvdiEma ? 1 : (prevNvdi > top && nvdi < top) ||
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
    /// Calculates the Hawkeye Volume Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static StockData CalculateHawkeyeVolumeIndicator(this StockData stockData, InputName inputName = InputName.MedianPrice, int length = 200,
        double divisor = 3.6m)
    {
        List<double> tempRangeList = new();
        List<double> tempVolumeList = new();
        List<double> u1List = new();
        List<double> d1List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentValue = closeList[i];

            double currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            double range = currentHigh - currentLow;
            tempRangeList.AddRounded(range);

            double volumeSma = tempVolumeList.TakeLastExt(length).Average();
            double rangeSma = tempRangeList.TakeLastExt(length).Average();
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevMidpoint = i >= 1 ? inputList[i - 1] : 0;
            double prevVolume = i >= 1 ? volumeList[i - 1] : 0;

            double u1 = divisor != 0 ? prevMidpoint + ((prevHigh - prevLow) / divisor) : prevMidpoint;
            u1List.AddRounded(u1);

            double d1 = divisor != 0 ? prevMidpoint - ((prevHigh - prevLow) / divisor) : prevMidpoint;
            d1List.AddRounded(d1);

            bool rEnabled1 = range > rangeSma && currentValue < d1 && currentVolume > volumeSma;
            bool rEnabled2 = currentValue < prevMidpoint;
            bool rEnabled = rEnabled1 || rEnabled2;

            bool gEnabled1 = currentValue > prevMidpoint;
            bool gEnabled2 = range > rangeSma && currentValue > u1 && currentVolume > volumeSma;
            bool gEnabled3 = currentHigh > prevHigh && range < rangeSma / 1.5 && currentVolume < volumeSma;
            bool gEnabled4 = currentLow < prevLow && range < rangeSma / 1.5 && currentVolume > volumeSma;
            bool gEnabled = gEnabled1 || gEnabled2 || gEnabled3 || gEnabled4;

            bool grEnabled1 = range > rangeSma && currentValue > d1 && currentValue < u1 && currentVolume > volumeSma && currentVolume < volumeSma * 1.5 && currentVolume > prevVolume;
            bool grEnabled2 = range < rangeSma / 1.5 && currentVolume < volumeSma / 1.5;
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
        stockData.CustomValuesList = new List<double>();
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
    public static StockData CalculateHerrickPayoffIndex(this StockData stockData, InputName inputName = InputName.MedianPrice, double pointValue = 100)
    {
        List<double> kList = new();
        List<double> hpicList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = closeList[i];
            double currentOpen = openList[i];
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevClose = i >= 1 ? closeList[i - 1] : 0;
            double prevOpen = i >= 1 ? openList[i - 1] : 0;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevK = kList.LastOrDefault();
            double absDiff = Math.Abs(currentClose - prevClose);
            double g = Math.Min(currentOpen, prevOpen);
            double k = MinPastValues(i, 1, currentValue - prevValue) * pointValue * currentVolume;
            double temp = g != 0 ? currentValue < prevValue ? 1 - (absDiff / 2 / g) : 1 + (absDiff / 2 / g) : 1;

            k *= temp;
            kList.AddRounded(k);

            double prevHpic = hpicList.LastOrDefault();
            double hpic = prevK + (k - prevK);
            hpicList.AddRounded(hpic);

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
    /// Calculates the Finite Volume Elements
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateFiniteVolumeElements(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 22, double factor = 0.3m)
    {
        List<double> fveList = new();
        List<double> bullList = new();
        List<double> bearList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var medianPriceList = CalculateMedianPrice(stockData).CustomValuesList;
        var typicalPriceList = CalculateTypicalPrice(stockData).CustomValuesList;
        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double medianPrice = medianPriceList[i];
            double typicalPrice = typicalPriceList[i];
            double prevTypicalPrice = i >= 1 ? typicalPriceList[i - 1] : 0;
            double volumeSma = volumeSmaList[i];
            double volume = volumeList[i];
            double close = inputList[i];
            double nmf = close - medianPrice + typicalPrice - prevTypicalPrice;
            double nvlm = nmf > factor * close / 100 ? volume : nmf < -factor * close / 100 ? -volume : 0;

            double prevFve = fveList.LastOrDefault();
            double prevFve2 = i >= 2 ? fveList[i - 2] : 0;
            double fve = volumeSma != 0 && length != 0 ? prevFve + (nvlm / volumeSma / length * 100) : prevFve;
            fveList.AddRounded(fve);

            double prevBullSlope = bullList.LastOrDefault();
            double bullSlope = fve - Math.Max(prevFve, prevFve2);
            bullList.AddRounded(bullSlope);

            double prevBearSlope = bearList.LastOrDefault();
            double bearSlope = fve - Math.Min(prevFve, prevFve2);
            bearList.AddRounded(bearSlope);

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
        List<double> relVolList = new();
        List<double> dplList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var smaVolumeList = GetMovingAverageList(stockData, maType, length, volumeList);
        stockData.CustomValuesList = volumeList;
        var stdDevVolumeList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentVolume = volumeList[i];
            double currentValue = inputList[i];
            double av = smaVolumeList[i];
            double sd = stdDevVolumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double relVol = sd != 0 ? (currentVolume - av) / sd : 0;
            relVolList.AddRounded(relVol);

            double prevDpl = dplList.LastOrDefault();
            double dpl = relVol >= 2 ? prevValue : i >= 1 ? prevDpl : currentValue;
            dplList.AddRounded(dpl);

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
        List<double> aMoveList = new();
        List<double> vBymList = new();
        List<double> theFomList = new();
        List<double> avfList = new();
        List<double> dplList = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var relVolList = CalculateRelativeVolumeIndicator(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double currentRelVol = relVolList[i];
            tempList.AddRounded(currentRelVol);

            double aMove = prevValue != 0 ? Math.Abs(MinPastValues(i, 1, currentValue - prevValue) / prevValue) : 0;
            aMoveList.AddRounded(aMove);

            var list = aMoveList.TakeLastExt(length).ToList();
            double aMoveMax = list.Max();
            double aMoveMin = list.Min();
            double theMove = aMoveMax - aMoveMin != 0 ? (1 + ((aMove - aMoveMin) * (10 - 1))) / (aMoveMax - aMoveMin) : 0;
            var tList = tempList.TakeLastExt(length).ToList();
            double relVolMax = tList.Max();
            double relVolMin = tList.Min();
            double theVol = relVolMax - relVolMin != 0 ? (1 + ((currentRelVol - relVolMin) * (10 - 1))) / (relVolMax - relVolMin) : 0;

            double vBym = theMove != 0 ? theVol / theMove : 0;
            vBymList.AddRounded(vBym);

            double avf = vBymList.TakeLastExt(length).Average();
            avfList.AddRounded(avf);
        }

        stockData.CustomValuesList = vBymList;
        var sdfList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double vBym = vBymList[i];
            double avf = avfList[i];
            double sdf = sdfList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double theFom = sdf != 0 ? (vBym - avf) / sdf : 0;
            theFomList.AddRounded(theFom);

            double prevDpl = dplList.LastOrDefault();
            double dpl = theFom >= 2 ? prevValue : i >= 1 ? prevDpl : currentValue;
            dplList.AddRounded(dpl);

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
        List<double> vpciList = new();
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
            double vwmaLong = vwmaLongList[i];
            double vwmaShort = vwmaShortList[i];
            double volumeSmaLong = volumeSmaLongList[i];
            double volumeSmaShort = volumeSmaShortList[i];
            double smaLong = smaLongList[i];
            double smaShort = smaShortList[i];
            double vpc = vwmaLong - smaLong;
            double vpr = smaShort != 0 ? vwmaShort / smaShort : 0;
            double vm = volumeSmaLong != 0 ? volumeSmaShort / volumeSmaLong : 0;

            double vpci = vpc * vpr * vm;
            vpciList.AddRounded(vpci);
        }

        var vpciSmaList = GetMovingAverageList(stockData, maType, length, vpciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double vpci = vpciList[i];
            double vpciSma = vpciSmaList[i];
            double prevVpci = i >= 1 ? vpciList[i - 1] : 0;
            double prevVpciSma = i >= 1 ? vpciSmaList[i - 1] : 0;

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
        List<double> vmpList = new();
        List<double> vmnList = new();
        List<double> vpnList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

        var mavList = GetMovingAverageList(stockData, maType, length, volumeList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double mav = mavList[i];
            mav = mav > 0 ? mav : 1;
            double tp = inputList[i];
            double prevTp = i >= 1 ? inputList[i - 1] : 0;
            double atr = atrList[i];
            double currentVolume = volumeList[i];
            double mf = tp - prevTp;
            double mc = 0.1m * atr;

            double vmp = mf > mc ? currentVolume : 0;
            vmpList.AddRounded(vmp);

            double vmn = mf < -mc ? currentVolume : 0;
            vmnList.AddRounded(vmn);

            double vn = vmnList.TakeLastExt(length).Sum();
            double vp = vmpList.TakeLastExt(length).Sum();

            double vpn = mav != 0 && length != 0 ? (vp - vn) / mav / length * 100 : 0;
            vpnList.AddRounded(vpn);
        }

        var vpnEmaList = GetMovingAverageList(stockData, maType, smoothLength, vpnList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double vpnEma = vpnEmaList[i];
            double prevVpnEma = i >= 1 ? vpnEmaList[i - 1] : 0;

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
        List<double> vaoList = new();
        List<double> vaoSumList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentVolume = volumeList[i];
            double medianValue = (currentHigh + currentLow) / 2;

            double vao = currentValue != medianValue ? currentVolume * (currentValue - medianValue) : currentVolume;
            vaoList.AddRounded(vao);

            double prevVaoSum = vaoSumList.LastOrDefault();
            double vaoSum = vaoList.TakeLastExt(length).Average();
            vaoSumList.AddRounded(vaoSum);

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
        List<double> vapcList = new();
        List<double> tvaList = new();
        List<double> tempVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = inputList[i];

            double currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            double xt = currentHigh - currentLow != 0 ? ((2 * currentClose) - currentHigh - currentLow) / (currentHigh - currentLow) : 0;
            double tva = currentVolume * xt;
            tvaList.AddRounded(tva);

            double volumeSum = tempVolumeList.TakeLastExt(length).Sum();
            double tvaSum = tvaList.TakeLastExt(length).Sum();

            double prevVapc = vapcList.LastOrDefault();
            double vapc = volumeSum != 0 ? MinOrMax(100 * tvaSum / volumeSum, 100, 0) : 0;
            vapcList.AddRounded(vapc);

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
        double coef = 0.2m, double vcoef = 2.5m)
    {
        List<double> interList = new();
        List<double> tempList = new();
        List<double> vcpList = new();
        List<double> vcpVaveSumList = new();
        List<double> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, closeList, volumeList) = GetInputValuesList(inputName, stockData);

        var smaVolumeList = GetMovingAverageList(stockData, maType, length1, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double inter = currentValue > 0 && prevValue > 0 ? Math.Log(currentValue) - Math.Log(prevValue) : 0;
            interList.AddRounded(inter);
        }

        stockData.CustomValuesList = interList;
        var vinterList = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double vinter = vinterList[i];
            double currentVolume = volumeList[i];
            double currentClose = closeList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevVave = tempList.LastOrDefault();
            double vave = smaVolumeList[i];
            tempList.AddRounded(vave);

            double cutoff = currentClose * vinter * coef;
            double vmax = prevVave * vcoef;
            double vc = Math.Min(currentVolume, vmax);
            double mf = MinPastValues(i, 1, currentValue - prevValue);

            double vcp = mf > cutoff ? vc : mf < cutoff * -1 ? vc * -1 : mf > 0 ? vc : mf < 0 ? vc * -1 : 0;
            vcpList.AddRounded(vcp);

            double vcpSum = vcpList.TakeLastExt(length1).Sum();
            double vcpVaveSum = vave != 0 ? vcpSum / vave : 0;
            vcpVaveSumList.AddRounded(vcpVaveSum);
        }

        var vfiList = GetMovingAverageList(stockData, maType, smoothLength, vcpVaveSumList);
        var vfiEmaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, signalLength, vfiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double vfi = vfiList[i];
            double vfima = vfiEmaList[i];

            double prevD = dList.LastOrDefault();
            double d = vfi - vfima;
            dList.AddRounded(d);

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
    /// Calculates the Price Volume Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculatePriceVolumeOscillator(this StockData stockData, int length1 = 50, int length2 = 14)
    {
        List<double> aList = new();
        List<double> bList = new();
        List<double> absAList = new();
        List<double> absBList = new();
        List<double> oscAList = new();
        List<double> oscBList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= length1 ? inputList[i - length1] : 0;
            double prevVolume = i >= length2 ? volumeList[i - length2] : 0;

            double a = MinPastValues(i, length1, currentValue - prevValue);
            aList.AddRounded(a);

            double b = MinPastValues(i, length2, currentVolume - prevVolume);
            bList.AddRounded(b);

            double absA = Math.Abs(a);
            absAList.AddRounded(absA);

            double absB = Math.Abs(b);
            absBList.AddRounded(absB);

            double aSum = aList.TakeLastExt(length1).Sum();
            double bSum = bList.TakeLastExt(length2).Sum();
            double absASum = absAList.TakeLastExt(length1).Sum();
            double absBSum = absBList.TakeLastExt(length2).Sum();

            double oscA = absASum != 0 ? aSum / absASum : 0;
            oscAList.AddRounded(oscA);

            double oscB = absBSum != 0 ? bSum / absBSum : 0;
            oscBList.AddRounded(oscB);

            var signal = GetConditionSignal(oscA > 0 && oscB > 0, oscA < 0 && oscB > 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Po", oscAList },
            { "Vo", oscBList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.PriceVolumeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Volume Rank
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculatePriceVolumeRank(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int fastLength = 5, int slowLength = 10)
    {
        List<double> pvrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentVolume = volumeList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevVolume = i >= 1 ? volumeList[i - 1] : 0;

            double pvr = currentValue > prevValue && currentVolume > prevVolume ? 1 : currentValue > prevValue && currentVolume <= prevVolume ? 2 :
                currentValue <= prevValue && currentVolume <= prevVolume ? 3 : 4;
            pvrList.AddRounded(pvr);
        }

        var pvrFastSmaList = GetMovingAverageList(stockData, maType, fastLength, pvrList);
        var pvrSlowSmaList = GetMovingAverageList(stockData, maType, slowLength, pvrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double fastSma = pvrFastSmaList[i];
            double slowSma = pvrSlowSmaList[i];
            double prevFastSma = i >= 1 ? pvrFastSmaList[i - 1] : 0;
            double prevSlowSma = i >= 1 ? pvrSlowSmaList[i - 1] : 0;

            var signal = GetCompareSignal(fastSma - slowSma, prevFastSma - prevSlowSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pvr", pvrList },
            { "SlowSignal", pvrSlowSmaList },
            { "FastSignal", pvrFastSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pvrList;
        stockData.IndicatorName = IndicatorName.PriceVolumeRank;

        return stockData;
    }

    /// <summary>
    /// Calculates the Twiggs Money Flow
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTwiggsMoneyFlow(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 21)
    {
        List<double> adList = new();
        List<double> tmfList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        var volumeEmaList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentPrice = inputList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentVolume = volumeList[i];
            double prevPrice = i >= 1 ? inputList[i - 1] : 0;
            double trh = Math.Max(currentHigh, prevPrice);
            double trl = Math.Min(currentLow, prevPrice);

            double ad = trh - trl != 0 && currentVolume != 0 ? (currentPrice - trl - (trh - currentPrice)) / (trh - trl) * currentVolume : 0;
            adList.AddRounded(ad);
        }

        var smoothAdList = GetMovingAverageList(stockData, maType, length, adList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentEmaVolume = volumeEmaList[i];
            double smoothAd = smoothAdList[i];
            double prevTmf1 = i >= 1 ? tmfList[i - 1] : 0;
            double prevTmf2 = i >= 2 ? tmfList[i - 2] : 0;

            double tmf = currentEmaVolume != 0 ? MinOrMax(smoothAd / currentEmaVolume, 1, -1) : 0;
            tmfList.AddRounded(tmf);

            var signal = GetRsiSignal(tmf - prevTmf1, prevTmf1 - prevTmf2, tmf, prevTmf1, 0.2m, -0.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tmf", tmfList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tmfList;
        stockData.IndicatorName = IndicatorName.TwiggsMoneyFlow;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trade Volume Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="minTickValue"></param>
    /// <returns></returns>
    public static StockData CalculateTradeVolumeIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, double minTickValue = 0.5m)
    {
        List<double> tviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentPrice = inputList[i];
            double currentVolume = volumeList[i];
            double prevPrice = i >= 1 ? inputList[i - 1] : 0;
            double priceChange = currentPrice - prevPrice;

            double prevTvi = tviList.LastOrDefault();
            double tvi = priceChange > minTickValue ? prevTvi + currentVolume : priceChange < -minTickValue ?
                prevTvi - currentVolume : prevTvi;
            tviList.AddRounded(tvi);
        }

        var tviSignalList = GetMovingAverageList(stockData, maType, length, tviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tvi = tviList[i];
            double tviSignal = tviSignalList[i];
            double prevTvi = i >= 1 ? tviList[i - 1] : 0;
            double prevTviSignal = i >= 1 ? tviSignalList[i - 1] : 0;

            var signal = GetCompareSignal(tvi - tviSignal, prevTvi - prevTviSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tvi", tviList },
            { "Signal", tviSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tviList;
        stockData.IndicatorName = IndicatorName.TradeVolumeIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the TFS Volume Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTFSVolumeOscillator(this StockData stockData, int length = 7)
    {
        List<double> totvList = new();
        List<double> tfsvoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double open = openList[i];
            double close = inputList[i];
            double volume = volumeList[i];

            double totv = close > open ? volume : close < open ? -volume : 0;
            totvList.AddRounded(totv);

            double totvSum = totvList.TakeLastExt(length).Sum();
            double prevTfsvo = tfsvoList.LastOrDefault();
            double tfsvo = length != 0 ? totvSum / length : 0;
            tfsvoList.AddRounded(tfsvo);

            var signal = GetCompareSignal(tfsvo, prevTfsvo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tfsvo", tfsvoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tfsvoList;
        stockData.IndicatorName = IndicatorName.TFSVolumeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Market Facilitation Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateMarketFacilitationIndex(this StockData stockData)
    {
        List<double> mfiList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentVolume = volumeList[i];
            double prevVolume = i >= 1 ? volumeList[i - 1] : 0;

            double prevMfi = mfiList.LastOrDefault();
            double mfi = currentVolume != 0 ? (currentHigh - currentLow) / currentVolume : 0;
            mfiList.AddRounded(mfi);

            double mfiDiff = mfi - prevMfi;
            double volDiff = currentVolume - prevVolume;

            var signal = GetConditionSignal(mfiDiff > 0, volDiff > 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mi", mfiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mfiList;
        stockData.IndicatorName = IndicatorName.MarketFacilitationIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Multi Vote On Balance Volume
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMultiVoteOnBalanceVolume(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<double> mvoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentClose = inputList[i];
            double currentLow = lowList[i];
            double currentVolume = volumeList[i] / 1000000;
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double highVote = currentHigh > prevHigh ? 1 : currentHigh < prevHigh ? -1 : 0;
            double lowVote = currentLow > prevLow ? 1 : currentLow < prevLow ? -1 : 0;
            double closeVote = currentClose > prevClose ? 1 : currentClose < prevClose ? -1 : 0;
            double totalVotes = highVote + lowVote + closeVote;

            double prevMvo = mvoList.LastOrDefault();
            double mvo = prevMvo + (currentVolume * totalVotes);
            mvoList.AddRounded(mvo);
        }

        var mvoEmaList = GetMovingAverageList(stockData, maType, length, mvoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mvo = mvoList[i];
            double mvoEma = mvoEmaList[i];
            double prevMvo = i >= 1 ? mvoList[i - 1] : 0;
            double prevMvoEma = i >= 1 ? mvoEmaList[i - 1] : 0;

            var signal = GetCompareSignal(mvo - mvoEma, prevMvo - prevMvoEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mvo", mvoList },
            { "Signal", mvoEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mvoList;
        stockData.IndicatorName = IndicatorName.MultiVoteOnBalanceVolume;

        return stockData;
    }
}
