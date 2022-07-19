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
        List<decimal> rawForceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal rawForce = (currentValue - prevValue) * currentVolume;
            rawForceList.AddRounded(rawForce);
        }

        var forceList = GetMovingAverageList(stockData, maType, length, rawForceList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal force = forceList[i];
            decimal prevForce1 = i >= 1 ? forceList[i - 1] : 0;
            decimal prevForce2 = i >= 2 ? forceList[i - 2] : 0;

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
        List<decimal> mfiList = new();
        List<decimal> posMoneyFlowList = new();
        List<decimal> negMoneyFlowList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentVolume = volumeList[i];
            decimal typicalPrice = inputList[i];
            decimal prevTypicalPrice = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMfi1 = i >= 1 ? mfiList[i - 1] : 0;
            decimal prevMfi2 = i >= 2 ? mfiList[i - 2] : 0;
            decimal rawMoneyFlow = typicalPrice * currentVolume;

            decimal posMoneyFlow = typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
            posMoneyFlowList.AddRounded(posMoneyFlow);

            decimal negMoneyFlow = typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
            negMoneyFlowList.AddRounded(negMoneyFlow);

            decimal posMoneyFlowTotal = posMoneyFlowList.TakeLastExt(length).Sum();
            decimal negMoneyFlowTotal = negMoneyFlowList.TakeLastExt(length).Sum();
            decimal mfiRatio = negMoneyFlowTotal != 0 ? MinOrMax(posMoneyFlowTotal / negMoneyFlowTotal, 1, 0) : 0;

            decimal mfi = negMoneyFlowTotal == 0 ? 100 : posMoneyFlowTotal == 0 ? 0 : MinOrMax(100 - (100 / (1 + mfiRatio)), 100, 0);
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
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal mom = currentValue - prevValue;

            decimal prevTrend = trendList.LastOrDefault();
            decimal trend = mom > 0 ? 1 : mom < 0 ? -1 : prevTrend;
            trendList.AddRounded(trend);

            decimal prevDm = dmList.LastOrDefault();
            decimal dm = currentHigh - currentLow;
            dmList.AddRounded(dm);

            decimal prevCm = cmList.LastOrDefault();
            decimal cm = trend == prevTrend ? prevCm + dm : prevDm + dm;
            cmList.AddRounded(cm);

            decimal temp = cm != 0 ? Math.Abs((2 * (dm / cm)) - 1) : -1;
            decimal vf = currentVolume * temp * trend * 100;
            vfList.AddRounded(vf);
        }

        var ema34List = GetMovingAverageList(stockData, maType, fastLength, vfList);
        var ema55List = GetMovingAverageList(stockData, maType, slowLength, vfList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema34 = ema34List[i];
            decimal ema55 = ema55List[i];

            decimal klingerOscillator = ema34 - ema55;
            kvoList.AddRounded(klingerOscillator);
        }

        var kvoSignalList = GetMovingAverageList(stockData, maType, signalLength, kvoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal klingerOscillator = kvoList[i];
            decimal koSignalLine = kvoSignalList[i];

            decimal prevKlingerOscillatorHistogram = kvoHistoList.LastOrDefault();
            decimal klingerOscillatorHistogram = klingerOscillator - koSignalLine;
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
        List<decimal> obvList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevObv = obvList.LastOrDefault();
            decimal obv = currentValue > prevValue ? prevObv + currentVolume : currentValue < prevValue ? prevObv - currentVolume : prevObv;
            obvList.AddRounded(obv);
        }

        var obvSignalList = GetMovingAverageList(stockData, maType, length, obvList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal obv = obvList[i];
            decimal prevObv = i >= 1 ? obvList[i - 1] : 0;
            decimal obvSig = obvSignalList[i];
            decimal prevObvSig = i >= 1 ? obvSignalList[i - 1] : 0;

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
        List<decimal> nviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal prevVolume = i >= 1 ? volumeList[i - 1] : 0;
            decimal pctChg = CalculatePercentChange(currentClose, prevClose);

            decimal prevNvi = nviList.LastOrDefault();
            decimal nvi = currentVolume >= prevVolume ? prevNvi : prevNvi + (prevNvi * pctChg);
            nviList.AddRounded(nvi);
        }

        var nviSignalList = GetMovingAverageList(stockData, maType, length, nviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nvi = nviList[i];
            decimal prevNvi = i >= 1 ? nviList[i - 1] : 0;
            decimal nviSignal = nviSignalList[i];
            decimal prevNviSignal = i >= 1 ? nviSignalList[i - 1] : 0;

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
        List<decimal> pviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal prevVolume = i >= 1 ? volumeList[i - 1] : 0;
            decimal pctChg = CalculatePercentChange(currentClose, prevClose);

            decimal prevPvi = pviList.LastOrDefault();
            decimal pvi = currentVolume <= prevVolume ? prevPvi : prevPvi + (prevPvi * pctChg);
            pviList.AddRounded(pvi);
        }

        var pviSignalList = GetMovingAverageList(stockData, maType, length, pviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pvi = pviList[i];
            decimal prevPvi = i >= 1 ? pviList[i - 1] : 0;
            decimal pviSignal = pviSignalList[i];
            decimal prevPviSignal = i >= 1 ? pviSignalList[i - 1] : 0;

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
        List<decimal> chaikinMoneyFlowList = new();
        List<decimal> tempVolumeList = new();
        List<decimal> moneyFlowVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentLow = lowList[i];
            decimal currentHigh = highList[i];
            decimal currentClose = inputList[i];
            decimal moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
            decimal prevCmf1 = i >= 1 ? chaikinMoneyFlowList[i - 1] : 0;
            decimal prevCmf2 = i >= 2 ? chaikinMoneyFlowList[i - 2] : 0;

            decimal currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            decimal moneyFlowVolume = moneyFlowMultiplier * currentVolume;
            moneyFlowVolumeList.AddRounded(moneyFlowVolume);

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
    public static StockData CalculateAccumulationDistributionLine(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 14)
    {
        List<decimal> adlList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentLow = lowList[i];
            decimal currentHigh = highList[i];
            decimal currentClose = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
            decimal moneyFlowVolume = moneyFlowMultiplier * currentVolume;

            decimal prevAdl = adlList.LastOrDefault();
            decimal adl = prevAdl + moneyFlowVolume;
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
        List<decimal> chgList = new();
        List<decimal> rList = new();
        List<decimal> kList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var avgvList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal chg = currentValue - prevValue;
            chgList.AddRounded(chg);
        }

        var avgcList = GetMovingAverageList(stockData, maType, length, chgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal avgv = avgvList[i];
            decimal avgc = avgcList[i];

            decimal r = Math.Abs(avgv * avgc) > 0 ? Log(Math.Abs(avgv * avgc)) * Math.Sign(avgc) : 0;
            rList.AddRounded(r);

            var list = rList.TakeLastExt(length).ToList();
            decimal rh = list.Max();
            decimal rl = list.Min();
            decimal rs = rh != rl ? (r - rl) / (rh - rl) * 100 : 0;

            decimal k = (rs * 2) - 100;
            kList.AddRounded(k);
        }

        var ksList = GetMovingAverageList(stockData, maType, smoothLength, kList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ks = ksList[i];
            decimal prevKs = i >= 1 ? ksList[i - 1] : 0;

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
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentVolume = volumeList[i];
            decimal currentOpen = openList[i];
            decimal currentClose = inputList[i];
            decimal highLowRange = highest - lowest;
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal prevOpen = i >= 1 ? openList[i - 1] : 0;
            decimal range = CalculateTrueRange(currentHigh, currentLow, prevClose);

            decimal prevV1 = v1List.LastOrDefault();
            decimal v1 = currentClose > currentOpen ? range / ((2 * range) + currentOpen - currentClose) * currentVolume :
                currentClose < currentOpen ? (range + currentClose - currentOpen) / ((2 * range) + currentClose - currentOpen) * currentVolume :
                0.5m * currentVolume;
            v1List.AddRounded(v1);

            decimal prevV2 = v2List.LastOrDefault();
            decimal v2 = currentVolume - v1;
            v2List.AddRounded(v2);

            decimal prevV3 = v3List.LastOrDefault();
            decimal v3 = v1 + v2;
            v3List.AddRounded(v3);

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
            decimal currentValue = inputList[i];

            decimal currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            decimal priceVol = currentValue * currentVolume;
            priceVolList.AddRounded(priceVol);

            decimal fastBuffNum = priceVolList.TakeLastExt(fastLength).Sum();
            decimal fastBuffDenom = tempVolumeList.TakeLastExt(fastLength).Sum();

            decimal prevFastBuff = fastBuffList.LastOrDefault();
            decimal fastBuff = fastBuffDenom != 0 ? fastBuffNum / fastBuffDenom : 0;
            fastBuffList.AddRounded(fastBuff);

            decimal slowBuffNum = priceVolList.TakeLastExt(slowLength).Sum();
            decimal slowBuffDenom = tempVolumeList.TakeLastExt(slowLength).Sum();

            decimal prevSlowBuff = slowBuffList.LastOrDefault();
            decimal slowBuff = slowBuffDenom != 0 ? slowBuffNum / slowBuffDenom : 0;
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
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> upVolList = new();
        List<decimal> downVolList = new();
        List<decimal> upDownVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal upVol = currentValue > prevValue ? currentVolume : 0;
            upVolList.AddRounded(upVol);

            decimal downVol = currentValue < prevValue ? currentVolume * -1 : 0;
            downVolList.AddRounded(downVol);

            decimal upVolSum = upVolList.TakeLastExt(length).Sum();
            decimal downVolSum = downVolList.TakeLastExt(length).Sum();

            decimal prevUpDownVol = upDownVolumeList.LastOrDefault();
            decimal upDownVol = downVolSum != 0 ? upVolSum / downVolSum : 0;
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
        decimal divisor = 1000000)
    {
        List<decimal> halfRangeList = new();
        List<decimal> midpointMoveList = new();
        List<decimal> emvList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentVolume = volumeList[i];
            decimal prevHalfRange = halfRangeList.LastOrDefault();
            decimal halfRange = (currentHigh - currentLow) * 0.5m;
            decimal boxRatio = currentHigh - currentLow != 0 ? currentVolume / (currentHigh - currentLow) : 0;

            decimal prevMidpointMove = midpointMoveList.LastOrDefault();
            decimal midpointMove = halfRange - prevHalfRange;
            midpointMoveList.AddRounded(midpointMove);

            decimal emv = boxRatio != 0 ? divisor * ((midpointMove - prevMidpointMove) / boxRatio) : 0;
            emvList.AddRounded(emv);
        }

        var emvSmaList = GetMovingAverageList(stockData, maType, length, emvList);
        var emvSignalList = GetMovingAverageList(stockData, maType, length, emvSmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal emv = emvList[i];
            decimal emvSignal = emvSignalList[i];
            decimal prevEmv = i >= 1 ? emvList[i - 1] : 0;
            decimal prevEmvSignal = i >= 1 ? emvSignalList[i - 1] : 0;

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
            decimal obvm = obvmList[i];
            decimal sig = sigList[i];
            decimal prevObvm = i >= 1 ? obvmList[i - 1] : 0;
            decimal prevSig = i >= 1 ? sigList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;

            decimal prevOvr = ovrList.LastOrDefault();
            decimal ovr = currentValue > prevValue ? prevOvr + currentVolume : currentValue < prevValue ? prevOvr - currentVolume : prevOvr;
            ovrList.AddRounded(ovr);
        }

        var ovrSmaList = GetMovingAverageList(stockData, maType, signalLength, ovrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ovr = ovrList[i];
            decimal ovrEma = ovrSmaList[i];
            decimal prevOvr = i >= 1 ? ovrList[i - 1] : 0;
            decimal prevOvrEma = i >= 1 ? ovrSmaList[i - 1] : 0;

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = obvList;
        var obvStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal sma = smaList[i];
            decimal stdDev = stdDevList[i];
            decimal obvSma = obvSmaList[i];
            decimal obvStdDev = obvStdDevList[i];
            decimal aTop = currentValue - (sma - (2 * stdDev));
            decimal aBot = currentValue + (2 * stdDev) - (sma - (2 * stdDev));
            decimal obv = obvList[i];
            decimal a = aBot != 0 ? aTop / aBot : 0;
            decimal bTop = obv - (obvSma - (2 * obvStdDev));
            decimal bBot = obvSma + (2 * obvStdDev) - (obvSma - (2 * obvStdDev));
            decimal b = bBot != 0 ? bTop / bBot : 0;

            decimal obvdi = 1 + b != 0 ? (1 + a) / (1 + b) : 0;
            obvdiList.AddRounded(obvdi);
        }

        var obvdiEmaList = GetMovingAverageList(stockData, maType, signalLength, obvdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal obvdi = obvdiList[i];
            decimal obvdiEma = obvdiEmaList[i];
            decimal prevObvdi = i >= 1 ? obvdiList[i - 1] : 0;

            decimal prevBsc = bscList.LastOrDefault();
            decimal bsc = (prevObvdi < bottom && obvdi > bottom) || obvdi > obvdiEma ? 1 : (prevObvdi > top && obvdi < top) ||
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
        int length = 33, int signalLength = 4, decimal top = 1.1m, decimal bottom = 0.9m)
    {
        List<decimal> nvdiList = new();
        List<decimal> bscList = new();
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
            decimal currentValue = inputList[i];
            decimal sma = smaList[i];
            decimal stdDev = stdDevList[i];
            decimal nviSma = nviSmaList[i];
            decimal nviStdDev = nviStdDevList[i];
            decimal aTop = currentValue - (sma - (2 * stdDev));
            decimal aBot = (currentValue + (2 * stdDev)) - (sma - (2 * stdDev));
            decimal nvi = nviList[i];
            decimal a = aBot != 0 ? aTop / aBot : 0;
            decimal bTop = nvi - (nviSma - (2 * nviStdDev));
            decimal bBot = (nviSma + (2 * nviStdDev)) - (nviSma - (2 * nviStdDev));
            decimal b = bBot != 0 ? bTop / bBot : 0;

            decimal nvdi = 1 + b != 0 ? (1 + a) / (1 + b) : 0;
            nvdiList.AddRounded(nvdi);
        }

        var nvdiEmaList = GetMovingAverageList(stockData, maType, signalLength, nvdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nvdi = nvdiList[i];
            decimal nvdiEma = nvdiEmaList[i];
            decimal prevNvdi = i >= 1 ? nvdiList[i - 1] : 0;

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
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentValue = closeList[i];

            decimal currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            decimal range = currentHigh - currentLow;
            tempRangeList.AddRounded(range);

            decimal volumeSma = tempVolumeList.TakeLastExt(length).Average();
            decimal rangeSma = tempRangeList.TakeLastExt(length).Average();
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal prevMidpoint = i >= 1 ? inputList[i - 1] : 0;
            decimal prevVolume = i >= 1 ? volumeList[i - 1] : 0;

            decimal u1 = divisor != 0 ? prevMidpoint + ((prevHigh - prevLow) / divisor) : prevMidpoint;
            u1List.AddRounded(u1);

            decimal d1 = divisor != 0 ? prevMidpoint - ((prevHigh - prevLow) / divisor) : prevMidpoint;
            d1List.AddRounded(d1);

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
            decimal currentClose = closeList[i];
            decimal currentOpen = openList[i];
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevClose = i >= 1 ? closeList[i - 1] : 0;
            decimal prevOpen = i >= 1 ? openList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevK = kList.LastOrDefault();
            decimal absDiff = Math.Abs(currentClose - prevClose);
            decimal g = Math.Min(currentOpen, prevOpen);
            decimal k = (currentValue - prevValue) * pointValue * currentVolume;
            decimal temp = g != 0 ? currentValue < prevValue ? 1 - (absDiff / 2 / g) : 1 + (absDiff / 2 / g) : 1;

            k *= temp;
            kList.AddRounded(k);

            decimal prevHpic = hpicList.LastOrDefault();
            decimal hpic = prevK + (k - prevK);
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
            decimal medianPrice = medianPriceList[i];
            decimal typicalPrice = typicalPriceList[i];
            decimal prevTypicalPrice = i >= 1 ? typicalPriceList[i - 1] : 0;
            decimal volumeSma = volumeSmaList[i];
            decimal volume = volumeList[i];
            decimal close = inputList[i];
            decimal nmf = close - medianPrice + typicalPrice - prevTypicalPrice;
            decimal nvlm = nmf > factor * close / 100 ? volume : nmf < -factor * close / 100 ? -volume : 0;

            decimal prevFve = fveList.LastOrDefault();
            decimal prevFve2 = i >= 2 ? fveList[i - 2] : 0;
            decimal fve = volumeSma != 0 && length != 0 ? prevFve + (nvlm / volumeSma / length * 100) : prevFve;
            fveList.AddRounded(fve);

            decimal prevBullSlope = bullList.LastOrDefault();
            decimal bullSlope = fve - Math.Max(prevFve, prevFve2);
            bullList.AddRounded(bullSlope);

            decimal prevBearSlope = bearList.LastOrDefault();
            decimal bearSlope = fve - Math.Min(prevFve, prevFve2);
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
        List<decimal> relVolList = new();
        List<decimal> dplList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var smaVolumeList = GetMovingAverageList(stockData, maType, length, volumeList);
        stockData.CustomValuesList = volumeList;
        var stdDevVolumeList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentVolume = volumeList[i];
            decimal currentValue = inputList[i];
            decimal av = smaVolumeList[i];
            decimal sd = stdDevVolumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal relVol = sd != 0 ? (currentVolume - av) / sd : 0;
            relVolList.AddRounded(relVol);

            decimal prevDpl = dplList.LastOrDefault();
            decimal dpl = relVol >= 2 ? prevValue : i >= 1 ? prevDpl : currentValue;
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal currentRelVol = relVolList[i];
            tempList.AddRounded(currentRelVol);

            decimal aMove = prevValue != 0 ? Math.Abs((currentValue - prevValue) / prevValue) : 0;
            aMoveList.AddRounded(aMove);

            var list = aMoveList.TakeLastExt(length).ToList();
            decimal aMoveMax = list.Max();
            decimal aMoveMin = list.Min();
            decimal theMove = aMoveMax - aMoveMin != 0 ? (1 + ((aMove - aMoveMin) * (10 - 1))) / (aMoveMax - aMoveMin) : 0;
            var tList = tempList.TakeLastExt(length).ToList();
            decimal relVolMax = tList.Max();
            decimal relVolMin = tList.Min();
            decimal theVol = relVolMax - relVolMin != 0 ? (1 + ((currentRelVol - relVolMin) * (10 - 1))) / (relVolMax - relVolMin) : 0;

            decimal vBym = theMove != 0 ? theVol / theMove : 0;
            vBymList.AddRounded(vBym);

            decimal avf = vBymList.TakeLastExt(length).Average();
            avfList.AddRounded(avf);
        }

        stockData.CustomValuesList = vBymList;
        var sdfList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal vBym = vBymList[i];
            decimal avf = avfList[i];
            decimal sdf = sdfList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal theFom = sdf != 0 ? (vBym - avf) / sdf : 0;
            theFomList.AddRounded(theFom);

            decimal prevDpl = dplList.LastOrDefault();
            decimal dpl = theFom >= 2 ? prevValue : i >= 1 ? prevDpl : currentValue;
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
            decimal vwmaLong = vwmaLongList[i];
            decimal vwmaShort = vwmaShortList[i];
            decimal volumeSmaLong = volumeSmaLongList[i];
            decimal volumeSmaShort = volumeSmaShortList[i];
            decimal smaLong = smaLongList[i];
            decimal smaShort = smaShortList[i];
            decimal vpc = vwmaLong - smaLong;
            decimal vpr = smaShort != 0 ? vwmaShort / smaShort : 0;
            decimal vm = volumeSmaLong != 0 ? volumeSmaShort / volumeSmaLong : 0;

            decimal vpci = vpc * vpr * vm;
            vpciList.AddRounded(vpci);
        }

        var vpciSmaList = GetMovingAverageList(stockData, maType, length, vpciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vpci = vpciList[i];
            decimal vpciSma = vpciSmaList[i];
            decimal prevVpci = i >= 1 ? vpciList[i - 1] : 0;
            decimal prevVpciSma = i >= 1 ? vpciSmaList[i - 1] : 0;

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
            decimal mav = mavList[i];
            mav = mav > 0 ? mav : 1;
            decimal tp = inputList[i];
            decimal prevTp = i >= 1 ? inputList[i - 1] : 0;
            decimal atr = atrList[i];
            decimal currentVolume = volumeList[i];
            decimal mf = tp - prevTp;
            decimal mc = 0.1m * atr;

            decimal vmp = mf > mc ? currentVolume : 0;
            vmpList.AddRounded(vmp);

            decimal vmn = mf < -mc ? currentVolume : 0;
            vmnList.AddRounded(vmn);

            decimal vn = vmnList.TakeLastExt(length).Sum();
            decimal vp = vmpList.TakeLastExt(length).Sum();

            decimal vpn = mav != 0 && length != 0 ? (vp - vn) / mav / length * 100 : 0;
            vpnList.AddRounded(vpn);
        }

        var vpnEmaList = GetMovingAverageList(stockData, maType, smoothLength, vpnList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vpnEma = vpnEmaList[i];
            decimal prevVpnEma = i >= 1 ? vpnEmaList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentVolume = volumeList[i];
            decimal medianValue = (currentHigh + currentLow) / 2;

            decimal vao = currentValue != medianValue ? currentVolume * (currentValue - medianValue) : currentVolume;
            vaoList.AddRounded(vao);

            decimal prevVaoSum = vaoSumList.LastOrDefault();
            decimal vaoSum = vaoList.TakeLastExt(length).Average();
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
        List<decimal> vapcList = new();
        List<decimal> tvaList = new();
        List<decimal> tempVolumeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentClose = inputList[i];

            decimal currentVolume = volumeList[i];
            tempVolumeList.AddRounded(currentVolume);

            decimal xt = currentHigh - currentLow != 0 ? ((2 * currentClose) - currentHigh - currentLow) / (currentHigh - currentLow) : 0;
            decimal tva = currentVolume * xt;
            tvaList.AddRounded(tva);

            decimal volumeSum = tempVolumeList.TakeLastExt(length).Sum();
            decimal tvaSum = tvaList.TakeLastExt(length).Sum();

            decimal prevVapc = vapcList.LastOrDefault();
            decimal vapc = volumeSum != 0 ? MinOrMax(100 * tvaSum / volumeSum, 100, 0) : 0;
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal inter = currentValue > 0 && prevValue > 0 ? Log(currentValue) - Log(prevValue) : 0;
            interList.AddRounded(inter);
        }

        stockData.CustomValuesList = interList;
        var vinterList = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vinter = vinterList[i];
            decimal currentVolume = volumeList[i];
            decimal currentClose = closeList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevVave = tempList.LastOrDefault();
            decimal vave = smaVolumeList[i];
            tempList.AddRounded(vave);

            decimal cutoff = currentClose * vinter * coef;
            decimal vmax = prevVave * vcoef;
            decimal vc = Math.Min(currentVolume, vmax);
            decimal mf = currentValue - prevValue;

            decimal vcp = mf > cutoff ? vc : mf < cutoff * -1 ? vc * -1 : mf > 0 ? vc : mf < 0 ? vc * -1 : 0;
            vcpList.AddRounded(vcp);

            decimal vcpSum = vcpList.TakeLastExt(length1).Sum();
            decimal vcpVaveSum = vave != 0 ? vcpSum / vave : 0;
            vcpVaveSumList.AddRounded(vcpVaveSum);
        }

        var vfiList = GetMovingAverageList(stockData, maType, smoothLength, vcpVaveSumList);
        var vfiEmaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, signalLength, vfiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vfi = vfiList[i];
            decimal vfima = vfiEmaList[i];

            decimal prevD = dList.LastOrDefault();
            decimal d = vfi - vfima;
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
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> absAList = new();
        List<decimal> absBList = new();
        List<decimal> oscAList = new();
        List<decimal> oscBList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= length1 ? inputList[i - length1] : 0;
            decimal prevVolume = i >= length2 ? volumeList[i - length2] : 0;

            decimal a = currentValue - prevValue;
            aList.AddRounded(a);

            decimal b = currentVolume - prevVolume;
            bList.AddRounded(b);

            decimal absA = Math.Abs(a);
            absAList.AddRounded(absA);

            decimal absB = Math.Abs(b);
            absBList.AddRounded(absB);

            decimal aSum = aList.TakeLastExt(length1).Sum();
            decimal bSum = bList.TakeLastExt(length2).Sum();
            decimal absASum = absAList.TakeLastExt(length1).Sum();
            decimal absBSum = absBList.TakeLastExt(length2).Sum();

            decimal oscA = absASum != 0 ? aSum / absASum : 0;
            oscAList.AddRounded(oscA);

            decimal oscB = absBSum != 0 ? bSum / absBSum : 0;
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
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> pvrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevVolume = i >= 1 ? volumeList[i - 1] : 0;

            decimal pvr = currentValue > prevValue && currentVolume > prevVolume ? 1 : currentValue > prevValue && currentVolume <= prevVolume ? 2 :
                currentValue <= prevValue && currentVolume <= prevVolume ? 3 : 4;
            pvrList.AddRounded(pvr);
        }

        var pvrFastSmaList = GetMovingAverageList(stockData, maType, fastLength, pvrList);
        var pvrSlowSmaList = GetMovingAverageList(stockData, maType, slowLength, pvrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastSma = pvrFastSmaList[i];
            decimal slowSma = pvrSlowSmaList[i];
            decimal prevFastSma = i >= 1 ? pvrFastSmaList[i - 1] : 0;
            decimal prevSlowSma = i >= 1 ? pvrSlowSmaList[i - 1] : 0;

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
        List<decimal> adList = new();
        List<decimal> tmfList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        var volumeEmaList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList[i];
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentVolume = volumeList[i];
            decimal prevPrice = i >= 1 ? inputList[i - 1] : 0;
            decimal trh = Math.Max(currentHigh, prevPrice);
            decimal trl = Math.Min(currentLow, prevPrice);

            decimal ad = trh - trl != 0 && currentVolume != 0 ? (currentPrice - trl - (trh - currentPrice)) / (trh - trl) * currentVolume : 0;
            adList.AddRounded(ad);
        }

        var smoothAdList = GetMovingAverageList(stockData, maType, length, adList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEmaVolume = volumeEmaList[i];
            decimal smoothAd = smoothAdList[i];
            decimal prevTmf1 = i >= 1 ? tmfList[i - 1] : 0;
            decimal prevTmf2 = i >= 2 ? tmfList[i - 2] : 0;

            decimal tmf = currentEmaVolume != 0 ? MinOrMax(smoothAd / currentEmaVolume, 1, -1) : 0;
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
        int length = 14, decimal minTickValue = 0.5m)
    {
        List<decimal> tviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevPrice = i >= 1 ? inputList[i - 1] : 0;
            decimal priceChange = currentPrice - prevPrice;

            decimal prevTvi = tviList.LastOrDefault();
            decimal tvi = priceChange > minTickValue ? prevTvi + currentVolume : priceChange < -minTickValue ?
                prevTvi - currentVolume : prevTvi;
            tviList.AddRounded(tvi);
        }

        var tviSignalList = GetMovingAverageList(stockData, maType, length, tviList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tvi = tviList[i];
            decimal tviSignal = tviSignalList[i];
            decimal prevTvi = i >= 1 ? tviList[i - 1] : 0;
            decimal prevTviSignal = i >= 1 ? tviSignalList[i - 1] : 0;

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
        List<decimal> totvList = new();
        List<decimal> tfsvoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal open = openList[i];
            decimal close = inputList[i];
            decimal volume = volumeList[i];

            decimal totv = close > open ? volume : close < open ? -volume : 0;
            totvList.AddRounded(totv);

            decimal totvSum = totvList.TakeLastExt(length).Sum();
            decimal prevTfsvo = tfsvoList.LastOrDefault();
            decimal tfsvo = length != 0 ? totvSum / length : 0;
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
        List<decimal> mfiList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentVolume = volumeList[i];
            decimal prevVolume = i >= 1 ? volumeList[i - 1] : 0;

            decimal prevMfi = mfiList.LastOrDefault();
            decimal mfi = currentVolume != 0 ? (currentHigh - currentLow) / currentVolume : 0;
            mfiList.AddRounded(mfi);

            decimal mfiDiff = mfi - prevMfi;
            decimal volDiff = currentVolume - prevVolume;

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
        List<decimal> mvoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentClose = inputList[i];
            decimal currentLow = lowList[i];
            decimal currentVolume = volumeList[i] / 1000000;
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal highVote = currentHigh > prevHigh ? 1 : currentHigh < prevHigh ? -1 : 0;
            decimal lowVote = currentLow > prevLow ? 1 : currentLow < prevLow ? -1 : 0;
            decimal closeVote = currentClose > prevClose ? 1 : currentClose < prevClose ? -1 : 0;
            decimal totalVotes = highVote + lowVote + closeVote;

            decimal prevMvo = mvoList.LastOrDefault();
            decimal mvo = prevMvo + (currentVolume * totalVotes);
            mvoList.AddRounded(mvo);
        }

        var mvoEmaList = GetMovingAverageList(stockData, maType, length, mvoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mvo = mvoList[i];
            decimal mvoEma = mvoEmaList[i];
            decimal prevMvo = i >= 1 ? mvoList[i - 1] : 0;
            decimal prevMvoEma = i >= 1 ? mvoEmaList[i - 1] : 0;

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
