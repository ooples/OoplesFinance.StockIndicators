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
    /// Calculates the index of the commodity channel.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="constant">The constant.</param>
    /// <returns></returns>
    public static StockData CalculateCommodityChannelIndex(this StockData stockData, InputName inputName = InputName.TypicalPrice,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20, double constant = 0.015m)
    {
        List<double> cciList = new();
        List<double> tpDevDiffList = new();
        List<Signal> signalsList = new();

        var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);
        var tpSmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double tpSma = tpSmaList[i];

            double tpDevDiff = Math.Abs(currentValue - tpSma);
            tpDevDiffList.AddRounded(tpDevDiff);
        }

        var tpMeanDevList = GetMovingAverageList(stockData, maType, length, tpDevDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double prevCci1 = i >= 1 ? cciList[i - 1] : 0;
            double prevCci2 = i >= 2 ? cciList[i - 2] : 0;
            double tpMeanDev = tpMeanDevList[i];
            double currentValue = inputList[i];
            double tpSma = tpSmaList[i];

            double cci = tpMeanDev != 0 ? (currentValue - tpSma) / (constant * tpMeanDev) : 0;
            cciList.AddRounded(cci);

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
    /// Calculates the Awesome Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateAwesomeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        InputName inputName = InputName.MedianPrice, int fastLength = 5, int slowLength = 34)
    {
        List<double> aoList = new();
        List<Signal> signalsList = new();

        var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);
        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastSma = fastSmaList[i];
            double slowSma = slowSmaList[i];

            double prevAo = aoList.LastOrDefault();
            double ao = fastSma - slowSma;
            aoList.AddRounded(ao);

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
    /// Calculates the Accelerator Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAcceleratorOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, InputName inputName = InputName.MedianPrice,
        int fastLength = 5, int slowLength = 34, int smoothLength = 5)
    {
        List<double> acList = new();
        List<Signal> signalsList = new();

        var awesomeOscList = CalculateAwesomeOscillator(stockData, maType, inputName, fastLength, slowLength).CustomValuesList;
        var awesomeOscMaList = GetMovingAverageList(stockData, maType, smoothLength, awesomeOscList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double ao = awesomeOscList[i];
            double aoSma = awesomeOscMaList[i];

            double prevAc = acList.LastOrDefault();
            double ac = ao - aoSma;
            acList.AddRounded(ac);

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
    /// Calculates the ulcer index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateUlcerIndex(this StockData stockData, int length = 14)
    {
        List<double> ulcerIndexList = new();
        List<double> pctDrawdownSquaredList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, _) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double maxValue = highestList[i];
            double prevUlcerIndex1 = i >= 1 ? ulcerIndexList[i - 1] : 0;
            double prevUlcerIndex2 = i >= 2 ? ulcerIndexList[i - 2] : 0;

            double pctDrawdownSquared = maxValue != 0 ? Pow((currentValue - maxValue) / maxValue * 100, 2) : 0;
            pctDrawdownSquaredList.AddRounded(pctDrawdownSquared);

            double squaredAvg = pctDrawdownSquaredList.TakeLastExt(length).Average();

            double ulcerIndex = squaredAvg >= 0 ? Sqrt(squaredAvg) : 0;
            ulcerIndexList.AddRounded(ulcerIndex);

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
    /// Calculates the balance of power.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateBalanceOfPower(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<double> balanceOfPowerList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentOpen = openList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];

            double balanceOfPower = currentHigh - currentLow != 0 ? (currentClose - currentOpen) / (currentHigh - currentLow) : 0;
            balanceOfPowerList.AddRounded(balanceOfPower);
        }

        var bopSignalList = GetMovingAverageList(stockData, maType, length, balanceOfPowerList);
        for (int i = 0; i < stockData.ClosePrices.Count; i++)
        {
            double bop = balanceOfPowerList[i];
            double bopMa = bopSignalList[i];
            double prevBop = i >= 1 ? balanceOfPowerList[i - 1] : 0;
            double prevBopMa = i >= 1 ? bopSignalList[i - 1] : 0;

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
        List<double> rocList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double prevRoc1 = i >= 1 ? rocList[i - 1] : 0;
            double prevRoc2 = i >= 2 ? rocList[i - 2] : 0;

            double roc = prevValue != 0 ? MinPastValues(i, length, currentValue - prevValue) / prevValue * 100 : 0;
            rocList.AddRounded(roc);

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
    /// Calculates the Chaikin Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateChaikinOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 3, int slowLength = 10)
    {
        List<double> chaikinOscillatorList = new();
        List<Signal> signalsList = new();

        var adlList = CalculateAccumulationDistributionLine(stockData, maType, fastLength).CustomValuesList;
        var adl3EmaList = GetMovingAverageList(stockData, maType, fastLength, adlList);
        var adl10EmaList = GetMovingAverageList(stockData, maType, slowLength, adlList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double adl3Ema = adl3EmaList[i];
            double adl10Ema = adl10EmaList[i];

            double prevChaikinOscillator = chaikinOscillatorList.LastOrDefault();
            double chaikinOscillator = adl3Ema - adl10Ema;
            chaikinOscillatorList.AddRounded(chaikinOscillator);

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
    public static StockData CalculateIchimokuCloud(this StockData stockData, int tenkanLength = 9, int kijunLength = 26, int senkouLength = 52)
    {
        List<double> tenkanSenList = new();
        List<double> kijunSenList = new();
        List<double> senkouSpanAList = new();
        List<double> senkouSpanBList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        var (tenkanHighList, tenkanLowList) = GetMaxAndMinValuesList(highList, lowList, tenkanLength);
        var (kijunHighList, kijunLowList) = GetMaxAndMinValuesList(highList, lowList, kijunLength);
        var (senkouHighList, senkouLowList) = GetMaxAndMinValuesList(highList, lowList, senkouLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest1 = tenkanHighList[i];
            double lowest1 = tenkanLowList[i];
            double highest2 = kijunHighList[i];
            double lowest2 = kijunLowList[i];
            double highest3 = senkouHighList[i];
            double lowest3 = senkouLowList[i];

            double prevTenkanSen = tenkanSenList.LastOrDefault();
            double tenkanSen = (highest1 + lowest1) / 2;
            tenkanSenList.AddRounded(tenkanSen);

            double prevKijunSen = kijunSenList.LastOrDefault();
            double kijunSen = (highest2 + lowest2) / 2;
            kijunSenList.AddRounded(kijunSen);

            double senkouSpanA = (tenkanSen + kijunSen) / 2;
            senkouSpanAList.AddRounded(senkouSpanA);

            double senkouSpanB = (highest3 + lowest3) / 2;
            senkouSpanBList.AddRounded(senkouSpanB);

            var signal = GetCompareSignal(tenkanSen - kijunSen, prevTenkanSen - prevKijunSen);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "TenkanSen", tenkanSenList },
            { "KijunSen", kijunSenList },
            { "SenkouSpanA", senkouSpanAList },
            { "SenkouSpanB", senkouSpanBList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
    public static StockData CalculateAlligatorIndex(this StockData stockData, InputName inputName = InputName.MedianPrice, 
        MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int jawLength = 13, int jawOffset = 8, int teethLength = 8, int teethOffset = 5, 
        int lipsLength = 5, int lipsOffset = 3)
    {
        List<double> displacedJawList = new();
        List<double> displacedTeethList = new();
        List<double> displacedLipsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var jawList = GetMovingAverageList(stockData, maType, jawLength, inputList);
        var teethList = GetMovingAverageList(stockData, maType, teethLength, inputList);
        var lipsList = GetMovingAverageList(stockData, maType, lipsLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevJaw = displacedJawList.LastOrDefault();
            double displacedJaw = i >= jawOffset ? jawList[i - jawOffset] : 0;
            displacedJawList.AddRounded(displacedJaw);

            double prevTeeth = displacedTeethList.LastOrDefault();
            double displacedTeeth = i >= teethOffset ? teethList[i - teethOffset] : 0;
            displacedTeethList.AddRounded(displacedTeeth);

            double prevLips = displacedLipsList.LastOrDefault();
            double displacedLips = i >= lipsOffset ? lipsList[i - lipsOffset] : 0;
            displacedLipsList.AddRounded(displacedLips);

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
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.AlligatorIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the gator oscillator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="jawLength">Length of the jaw.</param>
    /// <param name="jawOffset">The jaw offset.</param>
    /// <param name="teethLength">Length of the teeth.</param>
    /// <param name="teethOffset">The teeth offset.</param>
    /// <param name="lipsLength">Length of the lips.</param>
    /// <param name="lipsOffset">The lips offset.</param>
    /// <returns></returns>
    public static StockData CalculateGatorOscillator(this StockData stockData, InputName inputName = InputName.MedianPrice, 
        MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int jawLength = 13, int jawOffset = 8, int teethLength = 8, int teethOffset = 5, 
        int lipsLength = 5, int lipsOffset = 3)
    {
        List<double> topList = new();
        List<double> bottomList = new();
        List<Signal> signalsList = new();

        var alligatorList = CalculateAlligatorIndex(stockData, inputName, maType, jawLength, jawOffset, teethLength, teethOffset, lipsLength, lipsOffset).OutputValues;
        var jawList = alligatorList["Jaw"];
        var teethList = alligatorList["Teeth"];
        var lipsList = alligatorList["Lips"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double jaw = jawList[i];
            double teeth = teethList[i];
            double lips = lipsList[i];

            double prevTop = topList.LastOrDefault();
            double top = Math.Abs(jaw - teeth);
            topList.AddRounded(top);

            double prevBottom = bottomList.LastOrDefault();
            double bottom = -Math.Abs(teeth - lips);
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
        stockData.CustomValuesList = new List<double>();
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
        List<double> uoList = new();
        List<double> bpList = new();
        List<double> trList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double minValue = Math.Min(currentLow, prevClose);
            double maxValue = Math.Max(currentHigh, prevClose);
            double prevUo1 = i >= 1 ? uoList[i - 1] : 0;
            double prevUo2 = i >= 2 ? uoList[i - 2] : 0;

            double buyingPressure = currentClose - minValue;
            bpList.AddRounded(buyingPressure);

            double trueRange = maxValue - minValue;
            trList.AddRounded(trueRange);

            double bp7Sum = bpList.TakeLastExt(length1).Sum();
            double bp14Sum = bpList.TakeLastExt(length2).Sum();
            double bp28Sum = bpList.TakeLastExt(length3).Sum();
            double tr7Sum = trList.TakeLastExt(length1).Sum();
            double tr14Sum = trList.TakeLastExt(length2).Sum();
            double tr28Sum = trList.TakeLastExt(length3).Sum();
            double avg7 = tr7Sum != 0 ? bp7Sum / tr7Sum : 0;
            double avg14 = tr14Sum != 0 ? bp14Sum / tr14Sum : 0;
            double avg28 = tr28Sum != 0 ? bp28Sum / tr28Sum : 0;

            double ultimateOscillator = MinOrMax(100 * (((4 * avg7) + (2 * avg14) + avg28) / (4 + 2 + 1)), 100, 0);
            uoList.AddRounded(ultimateOscillator);

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
        List<double> vmPlusList = new();
        List<double> trueRangeList = new();
        List<double> vmMinusList = new();
        List<double> viPlus14List = new();
        List<double> viMinus14List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;

            double vmPlus = Math.Abs(currentHigh - prevLow);
            vmPlusList.AddRounded(vmPlus);

            double vmMinus = Math.Abs(currentLow - prevHigh);
            vmMinusList.AddRounded(vmMinus);

            double trueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trueRangeList.AddRounded(trueRange);

            double vmPlus14 = vmPlusList.TakeLastExt(length).Sum();
            double vmMinus14 = vmMinusList.TakeLastExt(length).Sum();
            double trueRange14 = trueRangeList.TakeLastExt(length).Sum();

            double prevViPlus14 = viPlus14List.LastOrDefault();
            double viPlus14 = trueRange14 != 0 ? vmPlus14 / trueRange14 : 0;
            viPlus14List.AddRounded(viPlus14);

            double prevViMinus14 = viMinus14List.LastOrDefault();
            double viMinus14 = trueRange14 != 0 ? vmMinus14 / trueRange14 : 0;
            viMinus14List.AddRounded(viMinus14);

            var signal = GetCompareSignal(viPlus14 - viMinus14, prevViPlus14 - prevViMinus14);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "ViPlus", viPlus14List },
            { "ViMinus", viMinus14List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.VortexIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trix Indicator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateTrix(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 15, int signalLength = 9)
    {
        List<double> trixList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
        var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            double ema3 = ema3List[i];
            double prevEma3 = i >= 1 ? ema3List[i - 1] : 0;

            double trix = CalculatePercentChange(ema3, prevEma3);
            trixList.AddRounded(trix);
        }

        var trixSignalList = GetMovingAverageList(stockData, maType, signalLength, trixList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double trix = trixList[i];
            double trixSignal = trixSignalList[i];
            double prevTrix = i >= 1 ? trixList[i - 1] : 0;
            double prevTrixSignal = i >= 1 ? trixSignalList[i - 1] : 0;

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
        List<double> williamsRList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double highestHigh = highestList[i];
            double lowestLow = lowestList[i];
            double prevWilliamsR1 = i >= 1 ? williamsRList[i - 1] : 0;
            double prevWilliamsR2 = i >= 2 ? williamsRList[i - 2] : 0;

            double williamsR = highestHigh - lowestLow != 0 ? -100 * (highestHigh - currentClose) / (highestHigh - lowestLow) : -100;
            williamsRList.AddRounded(williamsR);

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
    /// Calculates the True Strength Index
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length1">The length1.</param>
    /// <param name="length2">The length2.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateTrueStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 25, int length2 = 13, int signalLength = 7)
    {
        List<double> pcList = new();
        List<double> absPCList = new();
        List<double> tsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double pc = MinPastValues(i, 1, currentValue - prevValue);
            pcList.AddRounded(pc);

            double absPC = Math.Abs(pc);
            absPCList.AddRounded(absPC);
        }

        var pcSmooth1List = GetMovingAverageList(stockData, maType, length1, pcList);
        var pcSmooth2List = GetMovingAverageList(stockData, maType, length2, pcSmooth1List);
        var absPCSmooth1List = GetMovingAverageList(stockData, maType, length1, absPCList);
        var absPCSmooth2List = GetMovingAverageList(stockData, maType, length2, absPCSmooth1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double absSmooth2PC = absPCSmooth2List[i];
            double smooth2PC = pcSmooth2List[i];

            double tsi = absSmooth2PC != 0 ? MinOrMax(100 * smooth2PC / absSmooth2PC, 100, -100) : 0;
            tsiList.AddRounded(tsi);
        }

        var tsiSignalList = GetMovingAverageList(stockData, maType, signalLength, tsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tsi = tsiList[i];
            double tsiSignal = tsiSignalList[i];
            double prevTsi = i >= 1 ? tsiList[i - 1] : 0;
            double prevTsiSignal = i >= 1 ? tsiSignalList[i - 1] : 0;

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
    /// Calculates the index of the elder ray.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateElderRayIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 13)
    {
        List<double> bullPowerList = new();
        List<double> bearPowerList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentEma = emaList[i];

            double prevBullPower = bullPowerList.LastOrDefault();
            double bullPower = currentHigh - currentEma;
            bullPowerList.AddRounded(bullPower);

            double prevBearPower = bearPowerList.LastOrDefault();
            double bearPower = currentLow - currentEma;
            bearPowerList.AddRounded(bearPower);

            var signal = GetCompareSignal(bullPower - bearPower, prevBullPower - prevBearPower);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BullPower", bullPowerList },
            { "BearPower", bearPowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ElderRayIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Absolute Price Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateAbsolutePriceOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 10, int slowLength = 20)
    {
        List<double> apoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastEma = fastEmaList[i];
            double slowEma = slowEmaList[i];

            double prevApo = apoList.LastOrDefault();
            double apo = fastEma - slowEma;
            apoList.AddRounded(apo);

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
        List<double> aroonOscillatorList = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentPrice = inputList[i];
            tempList.AddRounded(currentPrice);

            double maxPrice = highestList[i];
            int maxIndex = tempList.LastIndexOf(maxPrice);
            double minPrice = lowestList[i];
            int minIndex = tempList.LastIndexOf(minPrice);
            int daysSinceMax = i - maxIndex;
            int daysSinceMin = i - minIndex;
            double aroonUp = (double)(length - daysSinceMax) / length * 100;
            double aroonDown = (double)(length - daysSinceMin) / length * 100;

            double prevAroonOscillator = aroonOscillatorList.LastOrDefault();
            double aroonOscillator = aroonUp - aroonDown;
            aroonOscillatorList.AddRounded(aroonOscillator);

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
        List<double> AList = new();
        List<double> MList = new();
        List<double> DList = new();
        List<double> mtList = new();
        List<double> utList = new();
        List<double> abssiEmaList = new();
        List<double> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double alp = (double)2 / (signalLength + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevA = AList.LastOrDefault();
            double A = currentValue > prevValue && prevValue != 0 ? prevA + ((currentValue / prevValue) - 1) : prevA;
            AList.AddRounded(A);

            double prevM = MList.LastOrDefault();
            double M = currentValue == prevValue ? prevM + ((double)1 / length) : prevM;
            MList.AddRounded(M);

            double prevD = DList.LastOrDefault();
            double D = currentValue < prevValue && currentValue != 0 ? prevD + ((prevValue / currentValue) - 1) : prevD;
            DList.AddRounded(D);

            double abssi = (D + M) / 2 != 0 ? 1 - (1 / (1 + ((A + M) / 2 / ((D + M) / 2)))) : 1;
            double abssiEma = CalculateEMA(abssi, abssiEmaList.LastOrDefault(), maLength);
            abssiEmaList.AddRounded(abssiEma);

            double abssio = abssi - abssiEma;
            double prevMt = mtList.LastOrDefault();
            double mt = (alp * abssio) + ((1 - alp) * prevMt);
            mtList.AddRounded(mt);

            double prevUt = utList.LastOrDefault();
            double ut = (alp * mt) + ((1 - alp) * prevUt);
            utList.AddRounded(ut);

            double s = (2 - alp) * (mt - ut) / (1 - alp);
            double prevd = dList.LastOrDefault();
            double d = abssio - s;
            dList.AddRounded(d);

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
    /// Calculates the Accumulative Swing Index
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAccumulativeSwingIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<double> accumulativeSwingIndexList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevOpen = i >= 1 ? openList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevHighCurrentClose = prevHigh - currentClose;
            double prevLowCurrentClose = prevLow - currentClose;
            double prevClosePrevOpen = prevClose - prevOpen;
            double currentHighPrevClose = currentHigh - prevClose;
            double currentLowPrevClose = currentLow - prevClose;
            double t = currentHigh - currentLow;
            double k = Math.Max(Math.Abs(prevHighCurrentClose), Math.Abs(prevLowCurrentClose));
            double r = currentHighPrevClose > Math.Max(currentLowPrevClose, t) ? currentHighPrevClose - (0.5m * currentLowPrevClose) + (0.25m * prevClosePrevOpen) :
                currentLowPrevClose > Math.Max(currentHighPrevClose, t) ? currentLowPrevClose - (0.5m * currentHighPrevClose) + (0.25m * prevClosePrevOpen) :
                t > Math.Max(currentHighPrevClose, currentLowPrevClose) ? t + (0.25m * prevClosePrevOpen) : 0;
            double swingIndex = r != 0 && t != 0 ? 50 * ((prevClose - currentClose + (0.5m * prevClosePrevOpen) +
                (0.25m * (currentClose - currentOpen))) / r) * (k / t) : 0;

            double prevSwingIndex = accumulativeSwingIndexList.LastOrDefault();
            double accumulativeSwingIndex = prevSwingIndex + swingIndex;
            accumulativeSwingIndexList.AddRounded(accumulativeSwingIndex);
        }

        var asiOscillatorList = GetMovingAverageList(stockData, maType, length, accumulativeSwingIndexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var asi = accumulativeSwingIndexList[i];
            var prevAsi = i >= 1 ? accumulativeSwingIndexList[i - 1] : 0;

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
    /// Calculates the Adaptive Ergodic Candlestick Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <param name="stochLength">Length of the stoch.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveErgodicCandlestickOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int smoothLength = 5, int stochLength = 14, int signalLength = 9)
    {
        List<double> came1List = new();
        List<double> came2List = new();
        List<double> came11List = new();
        List<double> came22List = new();
        List<double> ecoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        double mep = (double)2 / (smoothLength + 1);
        double ce = (stochLength + smoothLength) * 2;

        var stochList = CalculateStochasticOscillator(stockData, maType, length: stochLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double stoch = stochList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double currentClose = inputList[i];
            double vrb = Math.Abs(stoch - 50) / 50;

            double prevCame1 = came1List.LastOrDefault();
            double came1 = i < ce ? currentClose - currentOpen : prevCame1 + (mep * vrb * (currentClose - currentOpen - prevCame1));
            came1List.AddRounded(came1);

            double prevCame2 = came2List.LastOrDefault();
            double came2 = i < ce ? currentHigh - currentLow : prevCame2 + (mep * vrb * (currentHigh - currentLow - prevCame2));
            came2List.AddRounded(came2);

            double prevCame11 = came11List.LastOrDefault();
            double came11 = i < ce ? came1 : prevCame11 + (mep * vrb * (came1 - prevCame11));
            came11List.AddRounded(came11);

            double prevCame22 = came22List.LastOrDefault();
            double came22 = i < ce ? came2 : prevCame22 + (mep * vrb * (came2 - prevCame22));
            came22List.AddRounded(came22);

            double eco = came22 != 0 ? came11 / came22 * 100 : 0;
            ecoList.AddRounded(eco);
        }

        var seList = GetMovingAverageList(stockData, maType, signalLength, ecoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var eco = ecoList[i];
            var se = seList[i];
            var prevEco = i >= 1 ? ecoList[i - 1] : 0;
            var prevSe = i >= 1 ? seList[i - 1] : 0;

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
    /// Calculates the Absolute Strength MTF Indicator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAbsoluteStrengthMTFIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 50, int smoothLength = 25)
    {
        List<double> prevValuesList = new();
        List<double> bulls0List = new();
        List<double> bears0List = new();
        List<double> bulls1List = new();
        List<double> bears1List = new();
        List<double> bulls2List = new();
        List<double> bears2List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            prevValuesList.AddRounded(prevValue);
        }

        var price1List = GetMovingAverageList(stockData, maType, length, inputList);
        var price2List = GetMovingAverageList(stockData, maType, length, prevValuesList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double price1 = price1List[i];
            double price2 = price2List[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double high = highList[i];
            double low = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;

            double bulls0 = 0.5m * (Math.Abs(price1 - price2) + (price1 - price2));
            bulls0List.AddRounded(bulls0);

            double bears0 = 0.5m * (Math.Abs(price1 - price2) - (price1 - price2));
            bears0List.AddRounded(bears0);

            double bulls1 = price1 - lowest;
            bulls1List.AddRounded(bulls1);

            double bears1 = highest - price1;
            bears1List.AddRounded(bears1);

            double bulls2 = 0.5m * (Math.Abs(high - prevHigh) + (high - prevHigh));
            bulls2List.AddRounded(bulls2);

            double bears2 = 0.5m * (Math.Abs(prevLow - low) + (prevLow - low));
            bears2List.AddRounded(bears2);
        }

        var smthBulls0List = GetMovingAverageList(stockData, maType, smoothLength, bulls0List);
        var smthBears0List = GetMovingAverageList(stockData, maType, smoothLength, bears0List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double bulls = smthBulls0List[i];
            double bears = smthBears0List[i];
            double prevBulls = i >= 1 ? smthBulls0List[i - 1] : 0;
            double prevBears = i >= 1 ? smthBears0List[i - 1] : 0;

            var signal = GetCompareSignal(bulls - bears, prevBulls - prevBears);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Bulls", smthBulls0List },
            { "Bears", smthBears0List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.AbsoluteStrengthMTFIndicator;

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
        List<double> joList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((double)length / 2));

        var hList = GetMovingAverageList(stockData, maType, length1, highList);
        var lList = GetMovingAverageList(stockData, maType, length1, lowList);
        var cList = GetMovingAverageList(stockData, maType, length1, inputList);
        var highestList = GetMaxAndMinValuesList(hList, length1).Item1;
        var lowestList = GetMaxAndMinValuesList(lList, length1).Item2;

        for (int i = 0; i < stockData.Count; i++)
        {
            double c = cList[i];
            double prevC = i >= length ? cList[i - length] : 0;
            double highest = highestList[i];
            double lowest = lowestList[i];
            double prevJo1 = i >= 1 ? joList[i - 1] : 0;
            double prevJo2 = i >= 2 ? joList[i - 2] : 0;
            double cChg = c - prevC;

            double jo = highest - lowest != 0 ? cChg / (highest - lowest) : 0;
            joList.AddRounded(jo);

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
        List<double> rsxList = new();
        List<double> f8List = new();
        List<double> f28List = new();
        List<double> f30List = new();
        List<double> f38List = new();
        List<double> f40List = new();
        List<double> f48List = new();
        List<double> f50List = new();
        List<double> f58List = new();
        List<double> f60List = new();
        List<double> f68List = new();
        List<double> f70List = new();
        List<double> f78List = new();
        List<double> f80List = new();
        List<double> f88List = new();
        List<double> f90_List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double f18 = (double)3 / (length + 2);
        double f20 = 1 - f18;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevRsx1 = i >= 1 ? rsxList[i - 1] : 0;
            double prevRsx2 = i >= 2 ? rsxList[i - 2] : 0;

            double prevF8 = f8List.LastOrDefault();
            double f8 = 100 * currentValue;
            f8List.AddRounded(f8);

            double f10 = prevF8;
            double v8 = f8 - f10;

            double prevF28 = f28List.LastOrDefault();
            double f28 = (f20 * prevF28) + (f18 * v8);
            f28List.AddRounded(f28);

            double prevF30 = f30List.LastOrDefault();
            double f30 = (f18 * f28) + (f20 * prevF30);
            f30List.AddRounded(f30);

            double vC = (f28 * 1.5) - (f30 * 0.5);
            double prevF38 = f38List.LastOrDefault();
            double f38 = (f20 * prevF38) + (f18 * vC);
            f38List.AddRounded(f38);

            double prevF40 = f40List.LastOrDefault();
            double f40 = (f18 * f38) + (f20 * prevF40);
            f40List.AddRounded(f40);

            double v10 = (f38 * 1.5) - (f40 * 0.5);
            double prevF48 = f48List.LastOrDefault();
            double f48 = (f20 * prevF48) + (f18 * v10);
            f48List.AddRounded(f48);

            double prevF50 = f50List.LastOrDefault();
            double f50 = (f18 * f48) + (f20 * prevF50);
            f50List.AddRounded(f50);

            double v14 = (f48 * 1.5) - (f50 * 0.5);
            double prevF58 = f58List.LastOrDefault();
            double f58 = (f20 * prevF58) + (f18 * Math.Abs(v8));
            f58List.AddRounded(f58);

            double prevF60 = f60List.LastOrDefault();
            double f60 = (f18 * f58) + (f20 * prevF60);
            f60List.AddRounded(f60);

            double v18 = (f58 * 1.5) - (f60 * 0.5);
            double prevF68 = f68List.LastOrDefault();
            double f68 = (f20 * prevF68) + (f18 * v18);
            f68List.AddRounded(f68);

            double prevF70 = f70List.LastOrDefault();
            double f70 = (f18 * f68) + (f20 * prevF70);
            f70List.AddRounded(f70);

            double v1C = (f68 * 1.5) - (f70 * 0.5);
            double prevF78 = f78List.LastOrDefault();
            double f78 = (f20 * prevF78) + (f18 * v1C);
            f78List.AddRounded(f78);

            double prevF80 = f80List.LastOrDefault();
            double f80 = (f18 * f78) + (f20 * prevF80);
            f80List.AddRounded(f80);

            double v20 = (f78 * 1.5) - (f80 * 0.5);
            double prevF88 = f88List.LastOrDefault();
            double prevF90_ = f90_List.LastOrDefault();
            double f90_ = prevF90_ == 0 ? 1 : prevF88 <= prevF90_ ? prevF88 + 1 : prevF90_ + 1;
            f90_List.AddRounded(f90_);

            double f88 = prevF90_ == 0 && length - 1 >= 5 ? length - 1 : 5;
            double f0 = f88 >= f90_ && f8 != f10 ? 1 : 0;
            double f90 = f88 == f90_ && f0 == 0 ? 0 : f90_;
            double v4_ = f88 < f90 && v20 > 0 ? MinOrMax(((v14 / v20) + 1) * 50, 100, 0) : 50;
            double rsx = v4_ > 100 ? 100 : v4_ < 0 ? 0 : v4_;
            rsxList.AddRounded(rsx);

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
        List<double> smallSumList = new();
        List<double> smallRangeList = new();
        List<double> fdList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        int wind1 = MinOrMax((length2 - 1) * length1);
        int wind2 = MinOrMax(length2 * length1);
        double nLog = Math.Log(length2);

        var (highest1List, lowest1List) = GetMaxAndMinValuesList(highList, lowList, length1);
        var (highest2List, lowest2List) = GetMaxAndMinValuesList(highList, lowList, wind2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest1 = highest1List[i];
            double lowest1 = lowest1List[i];
            double prevValue1 = i >= length1 ? inputList[i - length1] : 0;
            double highest2 = highest2List[i];
            double lowest2 = lowest2List[i];
            double prevValue2 = i >= wind2 ? inputList[i - wind2] : 0;
            double bigRange = Math.Max(prevValue2, highest2) - Math.Min(prevValue2, lowest2);

            double prevSmallRange = i >= wind1 ? smallRangeList[i - wind1] : 0;
            double smallRange = Math.Max(prevValue1, highest1) - Math.Min(prevValue1, lowest1);
            smallRangeList.AddRounded(smallRange);

            double prevSmallSum = i >= 1 ? smallSumList.LastOrDefault() : smallRange;
            double smallSum = prevSmallSum + smallRange - prevSmallRange;
            smallSumList.AddRounded(smallSum);

            double value1 = wind1 != 0 ? smallSum / wind1 : 0;
            double value2 = value1 != 0 ? bigRange / value1 : 0;
            double temp = value2 > 0 ? Math.Log(value2) : 0;

            double fd = nLog != 0 ? 2 - (temp / nLog) : 0;
            fdList.AddRounded(fd);
        }

        var jrcfdList = GetMovingAverageList(stockData, maType, smoothLength, fdList);
        var jrcfdSignalList = GetMovingAverageList(stockData, maType, smoothLength, jrcfdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var jrcfd = jrcfdList[i];
            var jrcfdSignal = jrcfdSignalList[i];
            var prevJrcfd = i >= 1 ? jrcfdList[i - 1] : 0;
            var prevJrcfdSignal = i >= 1 ? jrcfdSignalList[i - 1] : 0;

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
        List<double> advDiffList = new();
        List<double> advancesList = new();
        List<double> declinesList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double advance = currentValue > prevValue ? 1 : 0;
            advancesList.AddRounded(advance);

            double decline = currentValue < prevValue ? 1 : 0;
            declinesList.AddRounded(decline);

            double advSum = advancesList.TakeLastExt(length).Sum();
            double decSum = declinesList.TakeLastExt(length).Sum();

            double advDiff = advSum + decSum != 0 ? advSum / (advSum + decSum) : 0;
            advDiffList.AddRounded(advDiff);
        }

        var zmbtiList = GetMovingAverageList(stockData, maType, length, advDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double prevZmbti1 = i >= 1 ? zmbtiList[i - 1] : 0;
            double prevZmbti2 = i >= 2 ? zmbtiList[i - 2] : 0;
            double zmbti = zmbtiList[i];

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
        List<double> zscoreList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var vwapList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = vwapList;
        var vwapSdList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevZScore1 = i >= 1 ? zscoreList[i - 1] : 0;
            double prevZScore2 = i >= 2 ? zscoreList[i - 2] : 0;
            double mean = vwapList[i];
            double vwapsd = vwapSdList[i];

            double zscore = vwapsd != 0 ? (currentValue - mean) / vwapsd : 0;
            zscoreList.AddRounded(zscore);

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
    public static StockData CalculateZScore(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<double> zScorePopulationList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double dev = currentValue - sma;
            double stdDevPopulation = stdDevList[i];

            double prevZScorePopulation = zScorePopulationList.LastOrDefault();
            double zScorePopulation = stdDevPopulation != 0 ? dev / stdDevPopulation : 0;
            zScorePopulationList.AddRounded(zScorePopulation);

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
        List<double> ax1List = new();
        List<double> lx1List = new();
        List<double> ax2List = new();
        List<double> lx2List = new();
        List<double> ax3List = new();
        List<double> lcoList = new();
        List<double> filterList = new();
        List<double> lcoSma1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((double)length / 2));

        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double linreg = linregList[i];

            double ax1 = currentValue - linreg;
            ax1List.AddRounded(ax1);
        }

        stockData.CustomValuesList = ax1List;
        var ax1LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double ax1 = ax1List[i];
            double ax1Linreg = ax1LinregList[i];

            double lx1 = ax1 + (ax1 - ax1Linreg);
            lx1List.AddRounded(lx1);
        }

        stockData.CustomValuesList = lx1List;
        var lx1LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double lx1 = lx1List[i];
            double lx1Linreg = lx1LinregList[i];

            double ax2 = lx1 - lx1Linreg;
            ax2List.AddRounded(ax2);
        }

        stockData.CustomValuesList = ax2List;
        var ax2LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double ax2 = ax2List[i];
            double ax2Linreg = ax2LinregList[i];

            double lx2 = ax2 + (ax2 - ax2Linreg);
            lx2List.AddRounded(lx2);
        }

        stockData.CustomValuesList = lx2List;
        var lx2LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double lx2 = lx2List[i];
            double lx2Linreg = lx2LinregList[i];

            double ax3 = lx2 - lx2Linreg;
            ax3List.AddRounded(ax3);
        }

        stockData.CustomValuesList = ax3List;
        var ax3LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double ax3 = ax3List[i];
            double ax3Linreg = ax3LinregList[i];

            double prevLco = lcoList.LastOrDefault();
            double lco = ax3 + (ax3 - ax3Linreg);
            lcoList.AddRounded(lco);

            double lcoSma1 = lcoList.TakeLastExt(length1).Average();
            lcoSma1List.AddRounded(lcoSma1);

            double lcoSma2 = lcoSma1List.TakeLastExt(length1).Average();
            double prevFilter = filterList.LastOrDefault();
            double filter = -lcoSma2 * 2;
            filterList.AddRounded(filter);

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
        int length = 20, double stdDevMult = 2.5m, double lowerThreshold = 15)
    {
        List<double> probBbUpperUpSeqList = new();
        List<double> probBbUpperDownSeqList = new();
        List<double> probBbBasisUpSeqList = new();
        List<double> probBbBasisUpList = new();
        List<double> probBbBasisDownSeqList = new();
        List<double> probBbBasisDownList = new();
        List<double> sigmaProbsDownList = new();
        List<double> sigmaProbsUpList = new();
        List<double> probPrimeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var bbList = CalculateBollingerBands(stockData, maType, length, stdDevMult);
        var upperBbList = bbList.OutputValues["UpperBand"];
        var basisList = bbList.OutputValues["MiddleBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double upperBb = upperBbList[i];
            double basis = basisList[i];

            double probBbUpperUpSeq = currentValue > upperBb ? 1 : 0;
            probBbUpperUpSeqList.AddRounded(probBbUpperUpSeq);

            double probBbUpperUp = probBbUpperUpSeqList.TakeLastExt(length).Average();

            double probBbUpperDownSeq = currentValue < upperBb ? 1 : 0;
            probBbUpperDownSeqList.AddRounded(probBbUpperDownSeq);

            double probBbUpperDown = probBbUpperDownSeqList.TakeLastExt(length).Average();
            double probUpBbUpper = probBbUpperUp + probBbUpperDown != 0 ? probBbUpperUp / (probBbUpperUp + probBbUpperDown) : 0;
            double probDownBbUpper = probBbUpperUp + probBbUpperDown != 0 ? probBbUpperDown / (probBbUpperUp + probBbUpperDown) : 0;

            double probBbBasisUpSeq = currentValue > basis ? 1 : 0;
            probBbBasisUpSeqList.AddRounded(probBbBasisUpSeq);

            double probBbBasisUp = probBbBasisUpSeqList.TakeLastExt(length).Average();
            probBbBasisUpList.AddRounded(probBbBasisUp);

            double probBbBasisDownSeq = currentValue < basis ? 1 : 0;
            probBbBasisDownSeqList.AddRounded(probBbBasisDownSeq);

            double probBbBasisDown = probBbBasisDownSeqList.TakeLastExt(length).Average();
            probBbBasisDownList.AddRounded(probBbBasisDown);

            double probUpBbBasis = probBbBasisUp + probBbBasisDown != 0 ? probBbBasisUp / (probBbBasisUp + probBbBasisDown) : 0;
            double probDownBbBasis = probBbBasisUp + probBbBasisDown != 0 ? probBbBasisDown / (probBbBasisUp + probBbBasisDown) : 0;

            double prevSigmaProbsDown = sigmaProbsDownList.LastOrDefault();
            double sigmaProbsDown = probUpBbUpper != 0 && probUpBbBasis != 0 ? ((probUpBbUpper * probUpBbBasis) / (probUpBbUpper * probUpBbBasis)) +
                ((1 - probUpBbUpper) * (1 - probUpBbBasis)) : 0;
            sigmaProbsDownList.AddRounded(sigmaProbsDown);

            double prevSigmaProbsUp = sigmaProbsUpList.LastOrDefault();
            double sigmaProbsUp = probDownBbUpper != 0 && probDownBbBasis != 0 ? ((probDownBbUpper * probDownBbBasis) / (probDownBbUpper * probDownBbBasis)) +
                ((1 - probDownBbUpper) * (1 - probDownBbBasis)) : 0;
            sigmaProbsUpList.AddRounded(sigmaProbsUp);

            double prevProbPrime = probPrimeList.LastOrDefault();
            double probPrime = sigmaProbsDown != 0 && sigmaProbsUp != 0 ? ((sigmaProbsDown * sigmaProbsUp) / (sigmaProbsDown * sigmaProbsUp)) +
                ((1 - sigmaProbsDown) * (1 - sigmaProbsUp)) : 0;
            probPrimeList.AddRounded(probPrime);

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
        stockData.CustomValuesList = new List<double>();
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
    public static StockData CalculateBearPowerIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<double> bpiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double open = openList[i];
            double high = highList[i];
            double low = lowList[i];

            double bpi = close < open ? high - low : prevClose > open ? Math.Max(close - open, high - low) :
                close > open ? Math.Max(open - low, high - close) : prevClose > open ? Math.Max(prevClose - low, high - close) :
                high - close > close - low ? high - low : prevClose > open ? Math.Max(prevClose - open, high - low) :
                high - close < close - low ? open - low : close > open ? Math.Max(close - low, high - close) :
                close > open ? Math.Max(prevClose - open, high - close) : prevClose < open ? Math.Max(open - low, high - close) : high - low;
            bpiList.AddRounded(bpi);
        }

        var bpiEmaList = GetMovingAverageList(stockData, maType, length, bpiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var bpi = bpiList[i];
            var bpiEma = bpiEmaList[i];
            var prevBpi = i >= 1 ? bpiList[i - 1] : 0;
            var prevBpiEma = i >= 1 ? bpiEmaList[i - 1] : 0;

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
    public static StockData CalculateBullPowerIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<double> bpiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double open = openList[i];
            double high = highList[i];
            double low = lowList[i];

            double bpi = close < open ? Math.Max(high - open, close - low) : prevClose < open ? Math.Max(high - prevClose, close - low) :
                close > open ? Math.Max(open - prevClose, high - low) : prevClose > open ? high - low :
                high - close > close - low ? high - open : prevClose < open ? Math.Max(high - prevClose, close - low) :
                high - close < close - low ? Math.Max(open - close, high - low) : prevClose > open ? high - low :
                prevClose > open ? Math.Max(high - open, close - low) : prevClose < open ? Math.Max(open - close, high - low) : high - low;
            bpiList.AddRounded(bpi);
        }

        var bpiEmaList = GetMovingAverageList(stockData, maType, length, bpiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var bpi = bpiList[i];
            var bpiEma = bpiEmaList[i];
            var prevBpi = i >= 1 ? bpiList[i - 1] : 0;
            var prevBpiEma = i >= 1 ? bpiEmaList[i - 1] : 0;

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
        List<double> bList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh1 = i >= 1 ? highList[i - 1] : 0;
            double prevLow1 = i >= 1 ? lowList[i - 1] : 0;
            double prevHigh2 = i >= 2 ? highList[i - 2] : 0;
            double prevLow2 = i >= 2 ? lowList[i - 2] : 0;
            double prevHigh3 = i >= 3 ? highList[i - 3] : 0;
            double prevLow3 = i >= 3 ? lowList[i - 3] : 0;
            double prevHigh4 = i >= 4 ? highList[i - 4] : 0;
            double prevLow4 = i >= 4 ? lowList[i - 4] : 0;
            double prevB1 = i >= 1 ? bList[i - 1] : 0;
            double prevB2 = i >= 2 ? bList[i - 2] : 0;
            double middle = (((currentHigh + currentLow) / 2) + ((prevHigh1 + prevLow1) / 2) + ((prevHigh2 + prevLow2) / 2) +
                ((prevHigh3 + prevLow3) / 2) + ((prevHigh4 + prevLow4) / 2)) / 5;
            double scale = ((currentHigh - currentLow + (prevHigh1 - prevLow1) + (prevHigh2 - prevLow2) + (prevHigh3 - prevLow3) +
                (prevHigh4 - prevLow4)) / 5) * 0.2;

            double b = scale != 0 ? (currentValue - middle) / scale : 0;
            bList.AddRounded(b);

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
        List<double> dxList = new();
        List<double> dxiList = new();
        List<double> trList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, rangeLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;

            double tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trList.AddRounded(tr);
        }

        stockData.CustomValuesList = trList;
        var trStoList = CalculateStochasticOscillator(stockData, maType, length: lbLength).CustomValuesList;
        stockData.CustomValuesList = volumeList;
        var vStoList = CalculateStochasticOscillator(stockData, maType, length: lbLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double body = close - openList[i];
            double high = highList[i];
            double low = lowList[i];
            double range = high - low;
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double c = close - prevClose;
            double sign = Math.Sign(c);
            double highest = highestList[i];
            double lowest = lowestList[i];
            double vSto = vStoList[i];
            double trSto = trStoList[i];
            double k1 = range != 0 ? body / range * 100 : 0;
            double k2 = range == 0 ? 0 : ((close - low) / range * 100 * 2) - 100;
            double k3 = c == 0 || highest - lowest == 0 ? 0 : ((close - lowest) / (highest - lowest) * 100 * 2) - 100;
            double k4 = highest - lowest != 0 ? c / (highest - lowest) * 100 : 0;
            double k5 = sign * trSto;
            double k6 = sign * vSto;
            double bullScore = Math.Max(0, k1) + Math.Max(0, k2) + Math.Max(0, k3) + Math.Max(0, k4) + Math.Max(0, k5) + Math.Max(0, k6);
            double bearScore = -1 * (Math.Min(0, k1) + Math.Min(0, k2) + Math.Min(0, k3) + Math.Min(0, k4) + Math.Min(0, k5) + Math.Min(0, k6));

            double dx = bearScore != 0 ? bullScore / bearScore : 0;
            dxList.AddRounded(dx);

            double dxi = (2 * (100 - (100 / (1 + dx)))) - 100;
            dxiList.AddRounded(dxi);
        }

        var dxiavgList = GetMovingAverageList(stockData, maType, lbLength, dxiList);
        var dxisList = GetMovingAverageList(stockData, maType, smoothLength, dxiavgList);
        var dxissList = GetMovingAverageList(stockData, maType, smoothLength, dxisList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double dxis = dxisList[i];
            double dxiss = dxissList[i];
            double prevDxis = i >= 1 ? dxisList[i - 1] : 0;
            double prevDxiss = i >= 1 ? dxissList[i - 1] : 0;

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
    /// Calculates the Uhl Ma Crossover System
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateUhlMaCrossoverSystem(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100)
    {
        List<double> cmaList = new();
        List<double> ctsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var varList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double prevVar = i >= length ? varList[i - length] : 0;
            double prevCma = i >= 1 ? cmaList.LastOrDefault() : currentValue;
            double prevCts = i >= 1 ? ctsList.LastOrDefault() : currentValue;
            double secma = Pow(sma - prevCma, 2);
            double sects = Pow(currentValue - prevCts, 2);
            double ka = prevVar < secma && secma != 0 ? 1 - (prevVar / secma) : 0;
            double kb = prevVar < sects && sects != 0 ? 1 - (prevVar / sects) : 0;

            double cma = (ka * sma) + ((1 - ka) * prevCma);
            cmaList.AddRounded(cma);

            double cts = (kb * currentValue) + ((1 - kb) * prevCts);
            ctsList.AddRounded(cts);

            var signal = GetCompareSignal(cts - cma, prevCts - prevCma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cts", ctsList },
            { "Cma", cmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
        int fastLength = 19, int slowLength = 39, int signalLength = 9, double mult = 1000)
    {
        List<double> advancesList = new();
        List<double> declinesList = new();
        List<double> advancesSumList = new();
        List<double> declinesSumList = new();
        List<double> ranaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double advance = currentValue > prevValue ? 1 : 0;
            advancesList.AddRounded(advance);

            double decline = currentValue < prevValue ? 1 : 0;
            declinesList.AddRounded(decline);

            double advanceSum = advancesList.TakeLastExt(fastLength).Sum();
            advancesSumList.AddRounded(advanceSum);

            double declineSum = declinesList.TakeLastExt(fastLength).Sum();
            declinesSumList.AddRounded(declineSum);

            double rana = advanceSum + declineSum != 0 ? mult * (advanceSum - declineSum) / (advanceSum + declineSum) : 0;
            ranaList.AddRounded(rana);
        }

        stockData.CustomValuesList = ranaList;
        var moList = CalculateMovingAverageConvergenceDivergence(stockData, maType, fastLength, slowLength, signalLength);
        var mcclellanOscillatorList = moList.OutputValues["Macd"];
        var mcclellanSignalLineList = moList.OutputValues["Signal"];
        var mcclellanHistogramList = moList.OutputValues["Histogram"];
        for (int i = 0; i < stockData.Count; i++)
        {
            double mcclellanHistogram = mcclellanHistogramList[i];
            double prevMcclellanHistogram = i >= 1 ? mcclellanHistogramList[i - 1] : 0;

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
    public static StockData CalculateWoodieCommodityChannelIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int fastLength = 6, int slowLength = 14)
    {
        List<double> histogramList = new();
        List<Signal> signalsList = new();

        var cciList = CalculateCommodityChannelIndex(stockData, maType: maType, length: slowLength).CustomValuesList;
        var turboCciList = CalculateCommodityChannelIndex(stockData, maType: maType, length: fastLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double cci = cciList[i];
            double cciTurbo = turboCciList[i];

            double prevCciHistogram = histogramList.LastOrDefault();
            double cciHistogram = cciTurbo - cci;
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
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.WoodieCommodityChannelIndex;

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
        List<double> upFractalList = new();
        List<double> dnFractalList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevHigh = i >= length - 2 ? highList[i - (length - 2)] : 0;
            double prevHigh1 = i >= length - 1 ? highList[i - (length - 1)] : 0;
            double prevHigh2 = i >= length ? highList[i - length] : 0;
            double prevHigh3 = i >= length + 1 ? highList[i - (length + 1)] : 0;
            double prevHigh4 = i >= length + 2 ? highList[i - (length + 2)] : 0;
            double prevHigh5 = i >= length + 3 ? highList[i - (length + 3)] : 0;
            double prevHigh6 = i >= length + 4 ? highList[i - (length + 4)] : 0;
            double prevHigh7 = i >= length + 5 ? highList[i - (length + 5)] : 0;
            double prevHigh8 = i >= length + 8 ? highList[i - (length + 6)] : 0;
            double prevLow = i >= length - 2 ? lowList[i - (length - 2)] : 0;
            double prevLow1 = i >= length - 1 ? lowList[i - (length - 1)] : 0;
            double prevLow2 = i >= length ? lowList[i - length] : 0;
            double prevLow3 = i >= length + 1 ? lowList[i - (length + 1)] : 0;
            double prevLow4 = i >= length + 2 ? lowList[i - (length + 2)] : 0;
            double prevLow5 = i >= length + 3 ? lowList[i - (length + 3)] : 0;
            double prevLow6 = i >= length + 4 ? lowList[i - (length + 4)] : 0;
            double prevLow7 = i >= length + 5 ? lowList[i - (length + 5)] : 0;
            double prevLow8 = i >= length + 8 ? lowList[i - (length + 6)] : 0;

            double prevUpFractal = upFractalList.LastOrDefault();
            double upFractal = (prevHigh4 < prevHigh2 && prevHigh3 < prevHigh2 && prevHigh1 < prevHigh2 && prevHigh < prevHigh2) ||
                (prevHigh5 < prevHigh2 && prevHigh4 < prevHigh2 && prevHigh3 == prevHigh2 && prevHigh1 < prevHigh2) ||
                (prevHigh6 < prevHigh2 && prevHigh5 < prevHigh2 && prevHigh4 == prevHigh2 && prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 &&
                prevHigh < prevHigh2) || (prevHigh7 < prevHigh2 && prevHigh6 < prevHigh2 && prevHigh5 == prevHigh2 && prevHigh4 == prevHigh2 &&
                prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 && prevHigh < prevHigh2) || (prevHigh8 < prevHigh2 && prevHigh7 < prevHigh2 &&
                prevHigh6 == prevHigh2 && prevHigh5 <= prevHigh2 && prevHigh4 == prevHigh2 && prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 &&
                prevHigh < prevHigh2) ? 1 : 0;
            upFractalList.AddRounded(upFractal);

            double prevDnFractal = dnFractalList.LastOrDefault();
            double dnFractal = (prevLow4 > prevLow2 && prevLow3 > prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow5 > prevLow2 &&
                prevLow4 > prevLow2 && prevLow3 == prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow6 > prevLow2 &&
                prevLow5 > prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) ||
                (prevLow7 > prevLow2 && prevLow6 > prevLow2 && prevLow5 == prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 &&
                prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow8 > prevLow2 && prevLow7 > prevLow2 && prevLow6 == prevLow2 &&
                prevLow5 >= prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) ? 1 : 0;
            dnFractalList.AddRounded(dnFractal);

            var signal = GetCompareSignal(upFractal - dnFractal, prevUpFractal - prevDnFractal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpFractal", upFractalList },
            { "DnFractal", dnFractalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
        List<double> wadList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;

            double prevWad = wadList.LastOrDefault();
            double wad = close > prevClose ? prevWad + close - prevLow : close < prevClose ? prevWad + close - prevHigh : 0;
            wadList.AddRounded(wad);

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
        List<double> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double diff = MinPastValues(i, 1, currentValue - prevValue);
            diffList.AddRounded(diff);
        }

        var wma1List = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, length2, diffList);
        var ema2List = GetMovingAverageList(stockData, maType, length1, wma1List);
        var wamiList = GetMovingAverageList(stockData, maType, length1, ema2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double wami = wamiList[i];
            double prevWami = i >= 1 ? wamiList[i - 1] : 0;

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
    public static StockData CalculateWaddahAttarExplosion(this StockData stockData, int fastLength = 20, int slowLength = 40, double sensitivity = 150)
    {
        List<double> t1List = new();
        List<double> t2List = new();
        List<double> e1List = new();
        List<double> temp1List = new();
        List<double> temp2List = new();
        List<double> temp3List = new();
        List<double> trendUpList = new();
        List<double> trendDnList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var macd1List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        var bbList = CalculateBollingerBands(stockData, length: fastLength);
        var upperBollingerBandList = bbList.OutputValues["UpperBand"];
        var lowerBollingerBandList = bbList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            temp1List.AddRounded(prevValue1);

            double prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            temp2List.AddRounded(prevValue2);

            double prevValue3 = i >= 3 ? inputList[i - 3] : 0;
            temp3List.AddRounded(prevValue3);
        }

        stockData.CustomValuesList = temp1List;
        var macd2List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        stockData.CustomValuesList = temp2List;
        var macd3List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        stockData.CustomValuesList = temp3List;
        var macd4List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentMacd1 = macd1List[i];
            double currentMacd2 = macd2List[i];
            double currentMacd3 = macd3List[i];
            double currentMacd4 = macd4List[i];
            double currentUpperBB = upperBollingerBandList[i];
            double currentLowerBB = lowerBollingerBandList[i];

            double t1 = (currentMacd1 - currentMacd2) * sensitivity;
            t1List.AddRounded(t1);

            double t2 = (currentMacd3 - currentMacd4) * sensitivity;
            t2List.AddRounded(t2);

            double prevE1 = e1List.LastOrDefault();
            double e1 = currentUpperBB - currentLowerBB;
            e1List.AddRounded(e1);

            double prevTrendUp = trendUpList.LastOrDefault();
            double trendUp = (t1 >= 0) ? t1 : 0;
            trendUpList.AddRounded(trendUp);

            double trendDown = (t1 < 0) ? (-1 * t1) : 0;
            trendDnList.AddRounded(trendDown);

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
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.WaddahAttarExplosion;

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
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14, int smoothLength = 5, double fastFactor = 2.618m,
        double slowFactor = 4.236m)
    {
        List<double> atrRsiList = new();
        List<double> fastAtrRsiList = new();
        List<double> slowAtrRsiList = new();
        List<Signal> signalsList = new();

        int wildersLength = (length * 2) - 1;

        var rsiValueList = CalculateRelativeStrengthIndex(stockData, maType, length, smoothLength);
        var rsiEmaList = rsiValueList.OutputValues["Signal"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentRsiEma = rsiEmaList[i];
            double prevRsiEma = i >= 1 ? rsiEmaList[i - 1] : 0;

            double atrRsi = Math.Abs(currentRsiEma - prevRsiEma);
            atrRsiList.AddRounded(atrRsi);
        }

        var atrRsiEmaList = GetMovingAverageList(stockData, maType, wildersLength, atrRsiList);
        var atrRsiEmaSmoothList = GetMovingAverageList(stockData, maType, wildersLength, atrRsiEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double atrRsiEmaSmooth = atrRsiEmaSmoothList[i];
            double prevAtrRsiEmaSmooth = i >= 1 ? atrRsiEmaSmoothList[i - 1] : 0;

            double prevFastTl = fastAtrRsiList.LastOrDefault();
            double fastTl = atrRsiEmaSmooth * fastFactor;
            fastAtrRsiList.AddRounded(fastTl);

            double prevSlowTl = slowAtrRsiList.LastOrDefault();
            double slowTl = atrRsiEmaSmooth * slowFactor;
            slowAtrRsiList.AddRounded(slowTl);

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
        stockData.CustomValuesList = new List<double>();
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
        int length = 20, int noiseLength = 500, double divisor = 40)
    {
        List<double> whiteNoiseList = new();
        List<double> whiteNoiseVarianceList = new();
        List<Signal> signalsList = new();

        var connorsRsiList = CalculateConnorsRelativeStrengthIndex(stockData, maType, noiseLength, noiseLength, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double connorsRsi = connorsRsiList[i];
            double prevConnorsRsi1 = i >= 1 ? connorsRsiList[i - 1] : 0;
            double prevConnorsRsi2 = i >= 2 ? connorsRsiList[i - 2] : 0;

            double whiteNoise = (connorsRsi - 50) * (1 / divisor);
            whiteNoiseList.AddRounded(whiteNoise);

            var signal = GetRsiSignal(connorsRsi - prevConnorsRsi1, prevConnorsRsi1 - prevConnorsRsi2, connorsRsi, prevConnorsRsi1, 70, 30);
            signalsList.Add(signal);
        }

        var whiteNoiseSmaList = GetMovingAverageList(stockData, maType, noiseLength, whiteNoiseList);
        stockData.CustomValuesList = whiteNoiseList;
        var whiteNoiseStdDevList = CalculateStandardDeviationVolatility(stockData, maType, noiseLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double whiteNoiseStdDev = whiteNoiseStdDevList[i];

            double whiteNoiseVariance = Pow(whiteNoiseStdDev, 2);
            whiteNoiseVarianceList.AddRounded(whiteNoiseVariance);
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
        int lbLength = 16, double atrMult = 2.5m)
    {
        List<double> upperBandList = new();
        List<double> lowerBandList = new();
        List<double> aatrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, lbLength);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double currentAtr = atrList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double aatr = atrMult * currentAtr;
            aatrList.AddRounded(aatr);

            double prevLowerBand = lowerBandList.LastOrDefault();
            double lowerBand = lowest + aatr;
            lowerBandList.AddRounded(lowerBand);

            double prevUpperBand = upperBandList.LastOrDefault();
            double upperBand = highest - aatr;
            upperBandList.AddRounded(upperBand);

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
        stockData.CustomValuesList = new List<double>();
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
        List<double> lqcdList = new();
        List<double> histList = new();
        List<Signal> signalsList = new();

        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        var yList = CalculateQuadraticRegression(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double linreg = linregList[i];
            double y = yList[i];

            double lqcd = y - linreg;
            lqcdList.AddRounded(lqcd);
        }

        var signList = GetMovingAverageList(stockData, maType, signalLength, lqcdList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double sign = signList[i];
            double lqcd = lqcdList[i];
            double osc = lqcd - sign;

            double prevHist = histList.LastOrDefault();
            double hist = osc - sign;
            histList.AddRounded(hist);

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
    public static StockData CalculateLogisticCorrelation(this StockData stockData, int length = 100, double k = 10)
    {
        List<double> tempList = new();
        List<double> indexList = new();
        List<double> logList = new();
        List<double> corrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            double index = i;
            indexList.AddRounded(index);

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((double)corr);
        }

        for (int i = 0; i < stockData.Count; i++)
        {
            double corr = corrList[i];
            double prevLog1 = i >= 1 ? logList[i - 1] : 0;
            double prevLog2 = i >= 2 ? logList[i - 2] : 0;

            double log = 1 / (1 + Exp(k * -corr));
            logList.AddRounded(log);

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
        List<double> macdList = new();
        List<double> macdHistogramList = new();
        List<double> ppoList = new();
        List<double> ppoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double sma3 = fastSmaList[i];
            double sma10 = slowSmaList[i];

            double ppo = sma10 != 0 ? (sma3 - sma10) / sma10 * 100 : 0;
            ppoList.AddRounded(ppo);

            double macd = sma3 - sma10;
            macdList.AddRounded(macd);
        }

        var macdSignalLineList = GetMovingAverageList(stockData, maType, smoothLength, macdList);
        var ppoSignalLineList = GetMovingAverageList(stockData, maType, smoothLength, ppoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ppo = ppoList[i];
            double ppoSignalLine = ppoSignalLineList[i];
            double macd = macdList[i];
            double macdSignalLine = macdSignalLineList[i];

            double ppoHistogram = ppo - ppoSignalLine;
            ppoHistogramList.AddRounded(ppoHistogram);

            double prevMacdHistogram = macdHistogramList.LastOrDefault();
            double macdHistogram = macd - macdSignalLine;
            macdHistogramList.AddRounded(macdHistogram);

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
        List<double> upList = new();
        List<double> downList = new();
        List<double> rviOriginalList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDeviationList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentStdDeviation = stdDeviationList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double up = currentValue > prevValue ? currentStdDeviation : 0;
            upList.AddRounded(up);

            double down = currentValue < prevValue ? currentStdDeviation : 0;
            downList.AddRounded(down);
        }

        var upAvgList = GetMovingAverageList(stockData, maType, smoothLength, upList);
        var downAvgList = GetMovingAverageList(stockData, maType, smoothLength, downList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double avgUp = upAvgList[i];
            double avgDown = downAvgList[i];
            double prevRvi1 = i >= 1 ? rviOriginalList[i - 1] : 0;
            double prevRvi2 = i >= 2 ? rviOriginalList[i - 2] : 0;
            double rs = avgDown != 0 ? avgUp / avgDown : 0;

            double rvi = avgDown == 0 ? 100 : avgUp == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rviOriginalList.AddRounded(rvi);

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
        List<double> rviList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        stockData.CustomValuesList = highList;
        var rviHighList = CalculateRelativeVolatilityIndexV1(stockData, maType, length, smoothLength).CustomValuesList;
        stockData.CustomValuesList = lowList;
        var rviLowList = CalculateRelativeVolatilityIndexV1(stockData, maType, length, smoothLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double rviOriginalHigh = rviHighList[i];
            double rviOriginalLow = rviLowList[i];
            double prevRvi1 = i >= 1 ? rviList[i - 1] : 0;
            double prevRvi2 = i >= 2 ? rviList[i - 2] : 0;

            double rvi = (rviOriginalHigh + rviOriginalLow) / 2;
            rviList.AddRounded(rvi);

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
            double inertiaIndicator = inertiaList[i];
            double prevInertiaIndicator1 = i >= 1 ? inertiaList[i - 1] : 0;
            double prevInertiaIndicator2 = i >= 2 ? inertiaList[i - 2] : 0;

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
        List<double> ibsList = new();
        List<double> ibsiList = new();
        List<double> ibsEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double high = highList[i];
            double low = lowList[i];

            double ibs = high - low != 0 ? (close - low) / (high - low) * 100 : 0;
            ibsList.AddRounded(ibs);

            double prevIbsi = ibsiList.LastOrDefault();
            double ibsi = ibsList.TakeLastExt(length).Average();
            ibsiList.AddRounded(ibsi);

            double prevIbsiEma = ibsEmaList.LastOrDefault();
            double ibsiEma = CalculateEMA(ibsi, prevIbsiEma, smoothLength);
            ibsEmaList.AddRounded(ibsiEma);

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
        List<double> ifzList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((double)length / 2));

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = smaList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = smaList;
        var linreg1List = CalculateLinearRegression(stockData, length).CustomValuesList;
        stockData.CustomValuesList = smaList;
        var linreg2List = CalculateLinearRegression(stockData, length1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double linreg1 = linreg1List[i];
            double linreg2 = linreg2List[i];
            double stdDev = stdDevList[i];
            double fz = stdDev != 0 ? (linreg2 - linreg1) / stdDev / 2 : 0;
            double prevIfz1 = i >= 1 ? ifzList[i - 1] : 0;
            double prevIfz2 = i >= 2 ? ifzList[i - 2] : 0;

            double ifz = Exp(10 * fz) + 1 != 0 ? (Exp(10 * fz) - 1) / (Exp(10 * fz) + 1) : 0;
            ifzList.AddRounded(ifz);

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
        List<double> fList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double stdDev = stdDevList[i];
            double prevF1 = i >= 1 ? fList[i - 1] : 0;
            double prevF2 = i >= 2 ? fList[i - 2] : 0;
            double z = stdDev != 0 ? (currentValue - sma) / stdDev : 0;
            double expZ = Exp(2 * z);

            double f = expZ + 1 != 0 ? MinOrMax((((expZ - 1) / (expZ + 1)) + 1) * 50, 100, 0) : 0;
            fList.AddRounded(f);

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
        int stochDLength = 3, int smaLength = 10, double stdDevMult = 2, double divisor = 10000)
    {
        List<double> iidxList = new();
        List<double> tempMacdList = new();
        List<double> tempDpoList = new();
        List<double> tempRocList = new();
        List<double> pdoinsbList = new();
        List<double> pdoinssList = new();
        List<double> emoList = new();
        List<double> emoSmaList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, length: rsiLength).CustomValuesList;
        var cciList = CalculateCommodityChannelIndex(stockData, length: cciLength).CustomValuesList;
        var mfiList = CalculateMoneyFlowIndex(stockData, length: mfiLength).CustomValuesList;
        var macdList = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength,
            signalLength: signalLength).CustomValuesList;
        var bbIndicatorList = CalculateBollingerBands(stockData, stdDevMult: stdDevMult, length: bbLength).CustomValuesList;
        var dpoList = CalculateDetrendedPriceOscillator(stockData, length: dpoLength).CustomValuesList;
        var rocList = CalculateRateOfChange(stockData, length: rocLength).CustomValuesList;
        var stochasticList = CalculateStochasticOscillator(stockData, length: stochLength, smoothLength1: stochKLength, smoothLength2: stochDLength);
        var stochKList = stochasticList.OutputValues["FastD"];
        var stochDList = stochasticList.OutputValues["SlowD"];
        var emvList = CalculateEaseOfMovement(stockData, length: emoLength, divisor: divisor).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double bolins2 = bbIndicatorList[i];
            double prevPdoinss10 = i >= smaLength ? pdoinssList[i - smaLength] : 0;
            double prevPdoinsb10 = i >= smaLength ? pdoinsbList[i - smaLength] : 0;
            double cci = cciList[i];
            double mfi = mfiList[i];
            double rsi = rsiList[i];
            double stochD = stochDList[i];
            double stochK = stochKList[i];
            double prevIidx1 = i >= 1 ? iidxList[i - 1] : 0;
            double prevIidx2 = i >= 2 ? iidxList[i - 2] : 0;
            double bolinsll = bolins2 < 0.05 ? -5 : bolins2 > 0.95 ? 5 : 0;
            double cciins = cci > 100 ? 5 : cci < -100 ? -5 : 0;

            double emo = emvList[i];
            emoList.AddRounded(emo);

            double emoSma = emoList.TakeLastExt(smaLength).Average();
            emoSmaList.AddRounded(emoSma);

            double emvins2 = emo - emoSma;
            double emvinsb = emvins2 < 0 ? emoSma < 0 ? -5 : 0 : emoSma > 0 ? 5 : 0;

            double macd = macdList[i];
            tempMacdList.AddRounded(macd);

            double macdSma = tempMacdList.TakeLastExt(smaLength).Average();
            double macdins2 = macd - macdSma;
            double macdinsb = macdins2 < 0 ? macdSma < 0 ? -5 : 0 : macdSma > 0 ? 5 : 0;
            double mfiins = mfi > 80 ? 5 : mfi < 20 ? -5 : 0;

            double dpo = dpoList[i];
            tempDpoList.AddRounded(dpo);

            double dpoSma = tempDpoList.TakeLastExt(smaLength).Average();
            double pdoins2 = dpo - dpoSma;
            double pdoinsb = pdoins2 < 0 ? dpoSma < 0 ? -5 : 0 : dpoSma > 0 ? 5 : 0;
            pdoinsbList.AddRounded(pdoinsb);

            double pdoinss = pdoins2 > 0 ? dpoSma > 0 ? 5 : 0 : dpoSma < 0 ? -5 : 0;
            pdoinssList.AddRounded(pdoinss);

            double roc = rocList[i];
            tempRocList.AddRounded(roc);

            double rocSma = tempRocList.TakeLastExt(smaLength).Average();
            double rocins2 = roc - rocSma;
            double rocinsb = rocins2 < 0 ? rocSma < 0 ? -5 : 0 : rocSma > 0 ? 5 : 0;
            double rsiins = rsi > 70 ? 5 : rsi < 30 ? -5 : 0;
            double stopdins = stochD > 80 ? 5 : stochD < 20 ? -5 : 0;
            double stopkins = stochK > 80 ? 5 : stochK < 20 ? -5 : 0;

            double iidx = 50 + cciins + bolinsll + rsiins + stopkins + stopdins + mfiins + emvinsb + rocinsb + prevPdoinss10 + prevPdoinsb10 + macdinsb;
            iidxList.AddRounded(iidx);

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
    /// Calculates the Detrended Price Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDetrendedPriceOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20)
    {
        List<double> dpoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int prevPeriods = MinOrMax((int)Math.Ceiling(((double)length / 2) + 1));

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentSma = smaList[i];
            double prevValue = i >= prevPeriods ? inputList[i - prevPeriods] : 0;

            double prevDpo = dpoList.LastOrDefault();
            double dpo = prevValue - currentSma;
            dpoList.AddRounded(dpo);

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
        List<double> lnList = new();
        List<double> oiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevLn = i >= length ? lnList[i - length] : 0;

            double ln = currentValue > 0 ? Math.Log(currentValue) * 1000 : 0;
            lnList.AddRounded(ln);

            double oi = (ln - prevLn) / Sqrt(length) * 100;
            oiList.AddRounded(oi);
        }

        var oiEmaList = GetMovingAverageList(stockData, maType, length, oiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double oiEma = oiEmaList[i];
            double prevOiEma1 = i >= 1 ? oiEmaList[i - 1] : 0;
            double prevOiEma2 = i >= 2 ? oiEmaList[i - 2] : 0;

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
    /// Calculates the Oscar Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOscarIndicator(this StockData stockData, int length = 8)
    {
        List<double> oscarList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double rough = highest - lowest != 0 ? MinOrMax((currentValue - lowest) / (highest - lowest) * 100, 100, 0) : 0;
            double prevOscar1 = i >= 1 ? oscarList[i - 1] : 0;
            double prevOscar2 = i >= 2 ? oscarList[i - 2] : 0;

            double oscar = (prevOscar1 / 6) + (rough / 3);
            oscarList.AddRounded(oscar);

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
    /// Calculates the OC Histogram
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOCHistogram(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 10)
    {
        List<double> ocHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        var openEmaList = GetMovingAverageList(stockData, maType, length, openList);
        var closeEmaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentCloseEma = closeEmaList[i];
            double currentOpenEma = openEmaList[i];

            double prevOcHistogram = ocHistogramList.LastOrDefault();
            double ocHistogram = currentCloseEma - currentOpenEma;
            ocHistogramList.AddRounded(ocHistogram);

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
        List<double> oscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastSma = fastSmaList[i];
            double slowSma = slowSmaList[i];
            double prevOsc1 = i >= 1 ? oscList[i - 1] : 0;
            double prevOsc2 = i >= 2 ? oscList[i - 2] : 0;

            double osc = slowSma - fastSma;
            oscList.AddRounded(osc);

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
        List<double> nxcList = new();
        List<Signal> signalsList = new();

        var ndxList = CalculateNaturalDirectionalIndex(stockData, maType, length, smoothLength).CustomValuesList;
        var nstList = CalculateNaturalStochasticIndicator(stockData, maType, length, smoothLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double ndx = ndxList[i];
            double nst = nstList[i];
            double prevNxc1 = i >= 1 ? nxcList[i - 1] : 0;
            double prevNxc2 = i >= 2 ? nxcList[i - 2] : 0;
            double v3 = Math.Sign(ndx) != Math.Sign(nst) ? ndx * nst : ((Math.Abs(ndx) * nst) + (Math.Abs(nst) * ndx)) / 2;

            double nxc = Math.Sign(v3) * Sqrt(Math.Abs(v3));
            nxcList.AddRounded(nxc);

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
        List<double> lnList = new();
        List<double> rawNdxList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double ln = currentValue > 0 ? Math.Log(currentValue) * 1000 : 0;
            lnList.AddRounded(ln);

            double weightSum = 0, denomSum = 0, absSum = 0;
            for (int j = 0; j < length; j++)
            {
                double prevLn = i >= j + 1 ? lnList[i - (j + 1)] : 0;
                double currLn = i >= j ? lnList[i - j] : 0;
                double diff = prevLn - currLn;
                absSum += Math.Abs(diff);
                double frac = absSum != 0 ? (ln - currLn) / absSum : 0;
                double ratio = 1 / Sqrt(j + 1);
                weightSum += frac * ratio;
                denomSum += ratio;
            }

            double rawNdx = denomSum != 0 ? weightSum / denomSum * 100 : 0;
            rawNdxList.AddRounded(rawNdx);
        }

        var ndxList = GetMovingAverageList(stockData, maType, smoothLength, rawNdxList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ndx = ndxList[i];
            double prevNdx1 = i >= 1 ? ndxList[i - 1] : 0;
            double prevNdx2 = i >= 2 ? ndxList[i - 2] : 0;

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
    /// Calculates the Natural Market Mirror
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalMarketMirror(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 40)
    {
        List<double> lnList = new();
        List<double> oiAvgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double ln = currentValue > 0 ? Math.Log(currentValue) * 1000 : 0;
            lnList.AddRounded(ln);

            double oiSum = 0;
            for (int j = 1; j <= length; j++)
            {
                double prevLn = i >= j ? lnList[i - j] : 0;
                oiSum += (ln - prevLn) / Sqrt(j) * 100;
            }

            double oiAvg = oiSum / length;
            oiAvgList.AddRounded(oiAvg);
        }

        var nmmList = GetMovingAverageList(stockData, maType, length, oiAvgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double nmm = nmmList[i];
            double prevNmm1 = i >= 1 ? nmmList[i - 1] : 0;
            double prevNmm2 = i >= 2 ? nmmList[i - 2] : 0;

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
        List<double> lnList = new();
        List<double> oiSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double ln = currentValue > 0 ? Math.Log(currentValue) * 1000 : 0;
            lnList.AddRounded(ln);

            double oiSum = 0;
            for (int j = 0; j < length; j++)
            {
                double currentLn = i >= j ? lnList[i - j] : 0;
                double prevLn = i >= j + 1 ? lnList[i - (j + 1)] : 0;

                oiSum += (prevLn - currentLn) * (Sqrt(j) - Sqrt(j + 1));
            }
            oiSumList.AddRounded(oiSum);
        }

        var nmrList = GetMovingAverageList(stockData, maType, length, oiSumList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double nmr = nmrList[i];
            double prevNmr1 = i >= 1 ? nmrList[i - 1] : 0;
            double prevNmr2 = i >= 2 ? nmrList[i - 2] : 0;

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
        List<double> nmcList = new();
        List<Signal> signalsList = new();

        var nmrList = CalculateNaturalMarketRiver(stockData, maType, length).CustomValuesList;
        var nmmList = CalculateNaturalMarketMirror(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double nmr = nmrList[i];
            double nmm = nmmList[i];
            double v3 = Math.Sign(nmm) != Math.Sign(nmr) ? nmm * nmr : ((Math.Abs(nmm) * nmr) + (Math.Abs(nmr) * nmm)) / 2;

            double nmc = Math.Sign(v3) * Sqrt(Math.Abs(v3));
            nmcList.AddRounded(nmc);
        }

        var nmcMaList = GetMovingAverageList(stockData, maType, smoothLength, nmcList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double nmc = nmcMaList[i];
            double prevNmc1 = i >= 1 ? nmcMaList[i - 1] : 0;
            double prevNmc2 = i >= 2 ? nmcMaList[i - 2] : 0;

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
        List<double> lnList = new();
        List<double> nmsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double ln = currentValue > 0 ? Math.Log(currentValue) * 1000 : 0;
            lnList.AddRounded(ln);
        }

        stockData.CustomValuesList = lnList;
        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double linReg = linRegList[i];
            double prevLinReg = i >= 1 ? linRegList[i - 1] : 0;
            double prevNms1 = i >= 1 ? nmsList[i - 1] : 0;
            double prevNms2 = i >= 2 ? nmsList[i - 2] : 0;

            double nms = (linReg - prevLinReg) * Math.Log(length);
            nmsList.AddRounded(nms);

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
        List<double> sumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevSum1 = i >= 1 ? sumList[i - 1] : 0;
            double prevSum2 = i >= 2 ? sumList[i - 2] : 0;

            double sum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                double prevValue = i >= j ? inputList[i - j] : 0;
                double x = j / (double)(length - 1);
                double win = 0.42m - (0.5 * Math.Cos(2 * Math.PI * x)) + (0.08 * Math.Cos(4 * Math.PI * x));
                double w = Math.Sin(2 * Math.PI * j / length) * win;
                sum += prevValue * w;
            }
            sumList.AddRounded(sum);

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
        List<double> nodoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double sum = 0, w = 1;
            for (int j = 0; j <= lbLength; j++)
            {
                double prevValue = i >= length * (j + 1) ? inputList[i - (length * (j + 1))] : 0;
                double x = Math.Sign(((j + 1) % 2) - 0.5);
                w *= (lbLength - j) / (double)(j + 1);
                sum += prevValue * w * x;
            }

            double prevNodo = nodoList.LastOrDefault();
            double nodo = currentValue - sum;
            nodoList.AddRounded(nodo);

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
    /// Calculates the Normalized Relative Vigor Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNormalizedRelativeVigorIndex(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SymmetricallyWeightedMovingAverage, int length = 10)
    {
        List<double> closeOpenList = new();
        List<double> highLowList = new();
        List<double> tempCloseOpenList = new();
        List<double> tempHighLowList = new();
        List<double> swmaCloseOpenSumList = new();
        List<double> swmaHighLowSumList = new();
        List<double> rvgiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentOpen = openList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];

            double closeOpen = currentClose - currentOpen;
            closeOpenList.AddRounded(closeOpen);

            double highLow = currentHigh - currentLow;
            highLowList.AddRounded(highLow);
        }

        var swmaCloseOpenList = GetMovingAverageList(stockData, maType, length, closeOpenList);
        var swmaHighLowList = GetMovingAverageList(stockData, maType, length, highLowList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double swmaCloseOpen = swmaCloseOpenList[i];
            tempCloseOpenList.AddRounded(swmaCloseOpen);

            double closeOpenSum = tempCloseOpenList.TakeLastExt(length).Sum();
            swmaCloseOpenSumList.AddRounded(closeOpenSum);

            double swmaHighLow = swmaHighLowList[i];
            tempHighLowList.AddRounded(swmaHighLow);

            double highLowSum = tempHighLowList.TakeLastExt(length).Sum();
            swmaHighLowSumList.AddRounded(highLowSum);

            double rvgi = highLowSum != 0 ? closeOpenSum / highLowSum * 100 : 0;
            rvgiList.AddRounded(rvgi);
        }

        var rvgiSignalList = GetMovingAverageList(stockData, maType, length, rvgiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rvgi = rvgiList[i];
            double rvgiSig = rvgiSignalList[i];
            double prevRvgi = i >= 1 ? rvgiList[i - 1] : 0;
            double prevRvgiSig = i >= 1 ? rvgiSignalList[i - 1] : 0;

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
        List<double> gannSwingOscillatorList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highestHigh = highestList[i];
            double lowestLow = lowestList[i];
            double prevHighest1 = i >= 1 ? highestList[i - 1] : 0;
            double prevLowest1 = i >= 1 ? lowestList[i - 1] : 0;
            double prevHighest2 = i >= 2 ? highestList[i - 2] : 0;
            double prevLowest2 = i >= 2 ? lowestList[i - 2] : 0;

            double prevGso = gannSwingOscillatorList.LastOrDefault();
            double gso = prevHighest2 > prevHighest1 && highestHigh > prevHighest1 ? 1 :
                prevLowest2 < prevLowest1 && lowestLow < prevLowest1 ? -1 : prevGso;
            gannSwingOscillatorList.AddRounded(gso);

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
    /// Calculates the Gann HiLo Activator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGannHiLoActivator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 3)
    {
        List<double> ghlaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var highMaList = GetMovingAverageList(stockData, maType, length, highList);
        var lowMaList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highMa = highMaList[i];
            double lowMa = lowMaList[i];
            double prevHighMa = i >= 1 ? highMaList[i - 1] : 0;
            double prevLowMa = i >= 1 ? lowMaList[i - 1] : 0;
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevGhla = ghlaList.LastOrDefault();
            double ghla = currentValue > prevHighMa ? lowMa : currentValue < prevLowMa ? highMa : prevGhla;
            ghlaList.AddRounded(ghla);

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
    /// Calculates the Grover Llorens Cycle Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateGroverLlorensCycleOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length = 100, int smoothLength = 20, double mult = 10)
    {
        List<double> tsList = new();
        List<double> oscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double atr = atrList[i];
            double prevTs = i >= 1 ? tsList[i - 1] : currentValue;
            double diff = currentValue - prevTs;

            double ts = diff > 0 ? prevTs - (atr * mult) : diff < 0 ? prevTs + (atr * mult) : prevTs;
            tsList.AddRounded(ts);

            double osc = currentValue - ts;
            oscList.AddRounded(osc);
        }

        var smoList = GetMovingAverageList(stockData, maType, smoothLength, oscList);
        stockData.CustomValuesList = smoList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: smoothLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double prevRsi1 = i >= 1 ? rsiList[i - 1] : 0;
            double prevRsi2 = i >= 2 ? rsiList[i - 2] : 0;

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
        int length = 100, double mult = 5)
    {
        List<double> tsList = new();
        List<double> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double atr = atrList[i];
            double prevTs = i >= 1 ? tsList[i - 1] : currentValue;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            prevTs = prevTs == 0 ? prevValue : prevTs;

            double prevDiff = diffList.LastOrDefault();
            double diff = currentValue - prevTs;
            diffList.AddRounded(diff);

            double ts = diff > 0 ? prevTs - (atr * mult) : diff < 0 ? prevTs + (atr * mult) : prevTs;
            tsList.AddRounded(ts);

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
        List<double> cblList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double hh = highestList[i];
            double ll = lowestList[i];

            double prevCbl = cblList.LastOrDefault();
            int hCount = 0, lCount = 0;
            double cbl = currentValue;
            for (int j = 0; j <= length; j++)
            {
                double currentLow = i >= j ? lowList[i - j] : 0;
                double currentHigh = i >= j ? highList[i - j] : 0;

                if (currentLow == ll)
                {
                    for (int k = j + 1; k <= j + length; k++)
                    {
                        double prevHigh = i >= k ? highList[i - k] : 0;
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
                        double prevLow = i >= k ? lowList[i - k] : 0;
                        hCount += prevLow > currentLow ? 1 : 0;
                        if (hCount == 2)
                        {
                            cbl = prevLow;
                            break;
                        }
                    }
                }
            }
            cblList.AddRounded(cbl);

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
        List<double> superGmmaFastList = new();
        List<double> superGmmaSlowList = new();
        List<double> superGmmaOscRawList = new();
        List<double> superGmmaOscList = new();
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
            double emaF1 = ema3List[i];
            double emaF2 = ema5List[i];
            double emaF3 = ema7List[i];
            double emaF4 = ema9List[i];
            double emaF5 = ema11List[i];
            double emaF6 = ema13List[i];
            double emaF7 = ema15List[i];
            double emaF8 = ema17List[i];
            double emaF9 = ema19List[i];
            double emaF10 = ema21List[i];
            double emaF11 = ema23List[i];
            double emaS1 = ema25List[i];
            double emaS2 = ema28List[i];
            double emaS3 = ema31List[i];
            double emaS4 = ema34List[i];
            double emaS5 = ema37List[i];
            double emaS6 = ema40List[i];
            double emaS7 = ema43List[i];
            double emaS8 = ema46List[i];
            double emaS9 = ema49List[i];
            double emaS10 = ema52List[i];
            double emaS11 = ema55List[i];
            double emaS12 = ema58List[i];
            double emaS13 = ema61List[i];
            double emaS14 = ema64List[i];
            double emaS15 = ema67List[i];
            double emaS16 = ema70List[i];

            double superGmmaFast = (emaF1 + emaF2 + emaF3 + emaF4 + emaF5 + emaF6 + emaF7 + emaF8 + emaF9 + emaF10 + emaF11) / 11;
            superGmmaFastList.AddRounded(superGmmaFast);

            double superGmmaSlow = (emaS1 + emaS2 + emaS3 + emaS4 + emaS5 + emaS6 + emaS7 + emaS8 + emaS9 + emaS10 + emaS11 + emaS12 + emaS13 +
                emaS14 + emaS15 + emaS16) / 16;
            superGmmaSlowList.AddRounded(superGmmaSlow);

            double superGmmaOscRaw = superGmmaSlow != 0 ? (superGmmaFast - superGmmaSlow) / superGmmaSlow * 100 : 0;
            superGmmaOscRawList.AddRounded(superGmmaOscRaw);

            double superGmmaOsc = superGmmaOscRawList.TakeLastExt(smoothLength).Average();
            superGmmaOscList.AddRounded(superGmmaOsc);
        }

        var superGmmaSignalList = GetMovingAverageList(stockData, maType, signalLength, superGmmaOscRawList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double superGmmaOsc = superGmmaOscList[i];
            double superGmmaSignal = superGmmaSignalList[i];
            double prevSuperGmmaOsc = i >= 1 ? superGmmaOscList[i - 1] : 0;
            double prevSuperGmmaSignal = i >= 1 ? superGmmaSignalList[i - 1] : 0;

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
        List<double> fastDistanceList = new();
        List<double> slowDistanceList = new();
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
            double ema1 = ema3List[i];
            double ema2 = ema5List[i];
            double ema3 = ema8List[i];
            double ema4 = ema10List[i];
            double ema5 = ema12List[i];
            double ema6 = ema15List[i];
            double ema7 = ema30List[i];
            double ema8 = ema35List[i];
            double ema9 = ema40List[i];
            double ema10 = ema45List[i];
            double ema11 = ema50List[i];
            double ema12 = ema60List[i];
            double diff12 = Math.Abs(ema1 - ema2);
            double diff23 = Math.Abs(ema2 - ema3);
            double diff34 = Math.Abs(ema3 - ema4);
            double diff45 = Math.Abs(ema4 - ema5);
            double diff56 = Math.Abs(ema5 - ema6);
            double diff78 = Math.Abs(ema7 - ema8);
            double diff89 = Math.Abs(ema8 - ema9);
            double diff910 = Math.Abs(ema9 - ema10);
            double diff1011 = Math.Abs(ema10 - ema11);
            double diff1112 = Math.Abs(ema11 - ema12);

            double fastDistance = diff12 + diff23 + diff34 + diff45 + diff56;
            fastDistanceList.AddRounded(fastDistance);

            double slowDistance = diff78 + diff89 + diff910 + diff1011 + diff1112;
            slowDistanceList.AddRounded(slowDistance);

            bool colFastL = ema1 > ema2 && ema2 > ema3 && ema3 > ema4 && ema4 > ema5 && ema5 > ema6;
            bool colFastS = ema1 < ema2 && ema2 < ema3 && ema3 < ema4 && ema4 < ema5 && ema5 < ema6;
            bool colSlowL = ema7 > ema8 && ema8 > ema9 && ema9 > ema10 && ema10 > ema11 && ema11 > ema12;
            bool colSlowS = ema7 < ema8 && ema8 < ema9 && ema9 < ema10 && ema10 < ema11 && ema11 < ema12;

            var signal = GetConditionSignal(colSlowL || colFastL, colSlowS || colFastS);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastDistance", fastDistanceList },
            { "SlowDistance", slowDistanceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
        List<double> bList = new();
        List<double> bSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevBSum1 = i >= 1 ? bSumList[i - 1] : 0;
            double prevBSum2 = i >= 2 ? bSumList[i - 2] : 0;

            double b = currentValue > prevValue ? (double)100 / length : 0;
            bList.AddRounded(b);

            double bSum = bList.TakeLastExt(length).Sum();
            bSumList.AddRounded(bSum);

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
        List<double> gainLossList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double gainLoss = currentValue + prevValue != 0 ? MinPastValues(i, 1, currentValue - prevValue) / ((currentValue + prevValue) / 2) * 100 : 0;
            gainLossList.AddRounded(gainLoss);
        }

        var gainLossAvgList = GetMovingAverageList(stockData, maType, length, gainLossList);
        var gainLossAvgSignalList = GetMovingAverageList(stockData, maType, signalLength, gainLossAvgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var gainLossSignal = gainLossAvgSignalList[i];
            var prevGainLossSignal1 = i >= 1 ? gainLossAvgSignalList[i - 1] : 0;
            var prevGainLossSignal2 = i >= 2 ? gainLossAvgSignalList[i - 2] : 0;

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
        List<double> hliList = new();
        List<double> advList = new();
        List<double> loList = new();
        List<double> advDiffList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevHighest = i >= 1 ? highestList[i - 1] : 0;
            double prevLowest = i >= 1 ? lowestList[i - 1] : 0;
            double highest = highestList[i];
            double lowest = lowestList[i];

            double adv = highest > prevHighest ? 1 : 0;
            advList.AddRounded(adv);

            double lo = lowest < prevLowest ? 1 : 0;
            loList.AddRounded(lo);

            double advSum = advList.TakeLastExt(length).Sum();
            double loSum = loList.TakeLastExt(length).Sum();

            double advDiff = advSum + loSum != 0 ? MinOrMax(advSum / (advSum + loSum) * 100, 100, 0) : 0;
            advDiffList.AddRounded(advDiff);
        }

        var zmbtiList = GetMovingAverageList(stockData, maType, length, advDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double zmbti = zmbtiList[i];
            double prevZmbti1 = i >= 1 ? hliList[i - 1] : 0;
            double prevZmbti2 = i >= 2 ? hliList[i - 2] : 0;

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
    /// Calculates the Forecast Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateForecastOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 3)
    {
        List<double> pfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double pf = currentValue != 0 ? 100 * MinPastValues(i, 1, currentValue - prevValue) / currentValue : 0;
            pfList.AddRounded(pf);
        }

        var pfSmaList = GetMovingAverageList(stockData, maType, length, pfList);
        for (int i = 0; i < stockData.Count; i++)
        {
            var pfSma = pfSmaList[i];
            var prevPfSma = i >= 1 ? pfSmaList[i - 1] : 0;

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
    /// Calculates the Fast and Slow Kurtosis Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public static StockData CalculateFastandSlowKurtosisOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 3, double ratio = 0.03m)
    {
        List<double> fskList = new();
        List<double> momentumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;

            double prevMomentum = momentumList.LastOrDefault();
            double momentum = MinPastValues(i, length, currentValue - prevValue);
            momentumList.AddRounded(momentum);

            double prevFsk = fskList.LastOrDefault();
            double fsk = (ratio * (momentum - prevMomentum)) + ((1 - ratio) * prevFsk);
            fskList.AddRounded(fsk);
        }

        var fskSignalList = GetMovingAverageList(stockData, maType, length, fskList);
        for (int i = 0; i < fskSignalList.Count; i++)
        {
            double fsk = fskList[i];
            double fskSignal = fskSignalList[i];
            double prevFsk = i >= 1 ? fskList[i - 1] : 0;
            double prevFskSignal = i >= 1 ? fskSignalList[i - 1] : 0;

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
        List<double> fsrsiList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length3).CustomValuesList;
        var fskList = CalculateFastandSlowKurtosisOscillator(stockData, maType, length: length1).CustomValuesList;
        var v4List = GetMovingAverageList(stockData, maType, length2, fskList);

        for (int i = 0; i < v4List.Count; i++)
        {
            double rsi = rsiList[i];
            double v4 = v4List[i];

            double fsrsi = (10000 * v4) + rsi;
            fsrsiList.AddRounded(fsrsi);
        }

        var fsrsiSignalList = GetMovingAverageList(stockData, maType, length4, fsrsiList);
        for (int i = 0; i < fsrsiSignalList.Count; i++)
        {
            double fsrsi = fsrsiList[i];
            double fsrsiSignal = fsrsiSignalList[i];
            double prevFsrsi = i >= 1 ? fsrsiList[i - 1] : 0;
            double prevFsrsiSignal = i >= 1 ? fsrsiSignalList[i - 1] : 0;

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
        List<double> fastF1bList = new();
        List<double> fastF2bList = new();
        List<double> fastVWList = new();
        List<double> slowF1bList = new();
        List<double> slowF2bList = new();
        List<double> slowVWList = new();
        List<double> slowVWSumList = new();
        List<double> osList = new();
        List<double> histList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double fastF1x = (double)(i + 1) / length;
            double fastF1b = (double)1 / (i + 1) * Math.Sin(fastF1x * (i + 1) * Math.PI);
            fastF1bList.AddRounded(fastF1b);

            double fastF1bSum = fastF1bList.TakeLastExt(fastLength).Sum();
            double fastF1pol = (fastF1x * fastF1x) + fastF1bSum;
            double fastF2x = length != 0 ? (double)i / length : 0;
            double fastF2b = (double)1 / (i + 1) * Math.Sin(fastF2x * (i + 1) * Math.PI);
            fastF2bList.AddRounded(fastF2b);

            double fastF2bSum = fastF2bList.TakeLastExt(fastLength).Sum();
            double fastF2pol = (fastF2x * fastF2x) + fastF2bSum;
            double fastW = fastF1pol - fastF2pol;
            double fastVW = prevValue * fastW;
            fastVWList.AddRounded(fastVW);

            double fastVWSum = fastVWList.TakeLastExt(length).Sum();
            double slowF1x = length != 0 ? (double)(i + 1) / length : 0;
            double slowF1b = (double)1 / (i + 1) * Math.Sin(slowF1x * (i + 1) * Math.PI);
            slowF1bList.AddRounded(slowF1b);

            double slowF1bSum = slowF1bList.TakeLastExt(slowLength).Sum();
            double slowF1pol = (slowF1x * slowF1x) + slowF1bSum;
            double slowF2x = length != 0 ? (double)i / length : 0;
            double slowF2b = (double)1 / (i + 1) * Math.Sin(slowF2x * (i + 1) * Math.PI);
            slowF2bList.AddRounded(slowF2b);

            double slowF2bSum = slowF2bList.TakeLastExt(slowLength).Sum();
            double slowF2pol = (slowF2x * slowF2x) + slowF2bSum;
            double slowW = slowF1pol - slowF2pol;
            double slowVW = prevValue * slowW;
            slowVWList.AddRounded(slowVW);

            double slowVWSum = slowVWList.TakeLastExt(length).Sum();
            slowVWSumList.AddRounded(slowVWSum);

            double os = fastVWSum - slowVWSum;
            osList.AddRounded(os);
        }

        var osSignalList = GetMovingAverageList(stockData, maType, signalLength, osList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double os = osList[i];
            double osSignal = osSignalList[i];

            double prevHist = histList.LastOrDefault();
            double hist = os - osSignal;
            histList.AddRounded(hist);

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
        List<double> fcoList = new();
        List<Signal> signalsList = new();

        var fractalChaosBandsList = CalculateFractalChaosBands(stockData);
        var upperBandList = fractalChaosBandsList.OutputValues["UpperBand"];
        var lowerBandList = fractalChaosBandsList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double upperBand = upperBandList[i];
            double prevUpperBand = i >= 1 ? upperBandList[i - 1] : 0;
            double lowerBand = lowerBandList[i];
            double prevLowerBand = i >= 1 ? lowerBandList[i - 1] : 0;

            double prevFco = fcoList.LastOrDefault();
            double fco = upperBand != prevUpperBand ? 1 : lowerBand != prevLowerBand ? -1 : 0;
            fcoList.AddRounded(fco);

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
        List<double> v2List = new();
        List<double> v5List = new();
        List<double> wwList = new();
        List<double> mmList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = inputList[i];

            double v2 = (currentHigh + currentLow + (currentClose * 2)) / 4;
            v2List.AddRounded(v2);
        }

        var v3List = GetMovingAverageList(stockData, maType, length, v2List);
        stockData.CustomValuesList = v2List;
        var v4List = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double v2 = v2List[i];
            double v3 = v3List[i];
            double v4 = v4List[i];

            double v5 = v4 == 0 ? (v2 - v3) * 100 : (v2 - v3) * 100 / v4;
            v5List.AddRounded(v5);
        }

        var v6List = GetMovingAverageList(stockData, maType, smoothLength, v5List);
        var v7List = GetMovingAverageList(stockData, maType, smoothLength, v6List);
        var wwZLagEmaList = GetMovingAverageList(stockData, maType, length, v7List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double wwZlagEma = wwZLagEmaList[i];
            double prevWw1 = i >= 1 ? wwList[i - 1] : 0;
            double prevWw2 = i >= 2 ? wwList[i - 2] : 0;

            double ww = ((wwZlagEma + 100) / 2) - 4;
            wwList.AddRounded(ww);

            double mm = wwList.TakeLastExt(smoothLength).Max();
            mmList.AddRounded(mm);

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
    /// Calculates the Fibonacci Retrace
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateFibonacciRetrace(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length1 = 15, int length2 = 50, double factor = 0.382)
    {
        List<double> hretList = new();
        List<double> lretList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        var wmaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double wma = wmaList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double prevWma = i >= 1 ? wmaList[i - 1] : 0;
            double retrace = (highest - lowest) * factor;

            double prevHret = hretList.LastOrDefault();
            double hret = highest - retrace;
            hretList.AddRounded(hret);

            double prevLret = lretList.LastOrDefault();
            double lret = lowest + retrace;
            lretList.AddRounded(lret);

            var signal = GetBullishBearishSignal(wma - hret, prevWma - prevHret, wma - lret, prevWma - prevLret);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", hretList },
            { "LowerBand", lretList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
        int cciLength = 14, int t3Length = 5, double b = 0.618)
    {
        List<double> e1List = new();
        List<double> e2List = new();
        List<double> e3List = new();
        List<double> e4List = new();
        List<double> e5List = new();
        List<double> e6List = new();
        List<double> fxSniperList = new();
        List<Signal> signalsList = new();

        double b2 = b * b;
        double b3 = b2 * b;
        double c1 = -b3;
        double c2 = 3 * (b2 + b3);
        double c3 = -3 * ((2 * b2) + b + b3);
        double c4 = 1 + (3 * b) + b3 + (3 * b2);
        double nr = 1 + (0.5 * (t3Length - 1));
        double w1 = 2 / (nr + 1);
        double w2 = 1 - w1;

        var cciList = CalculateCommodityChannelIndex(stockData, maType: maType, length: cciLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double cci = cciList[i];

            double prevE1 = e1List.LastOrDefault();
            double e1 = (w1 * cci) + (w2 * prevE1);
            e1List.AddRounded(e1);

            double prevE2 = e2List.LastOrDefault();
            double e2 = (w1 * e1) + (w2 * prevE2);
            e2List.AddRounded(e2);

            double prevE3 = e3List.LastOrDefault();
            double e3 = (w1 * e2) + (w2 * prevE3);
            e3List.AddRounded(e3);

            double prevE4 = e4List.LastOrDefault();
            double e4 = (w1 * e3) + (w2 * prevE4);
            e4List.AddRounded(e4);

            double prevE5 = e5List.LastOrDefault();
            double e5 = (w1 * e4) + (w2 * prevE5);
            e5List.AddRounded(e5);

            double prevE6 = e6List.LastOrDefault();
            double e6 = (w1 * e5) + (w2 * prevE6);
            e6List.AddRounded(e6);

            double prevFxSniper = fxSniperList.LastOrDefault();
            double fxsniper = (c1 * e6) + (c2 * e5) + (c3 * e4) + (c4 * e3);
            fxSniperList.AddRounded(fxsniper);

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
        List<double> trUpList = new();
        List<double> trDnList = new();
        List<double> fgiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

            double trUp = currentValue > prevValue ? tr : 0;
            trUpList.AddRounded(trUp);

            double trDn = currentValue < prevValue ? tr : 0;
            trDnList.AddRounded(trDn);
        }

        var fastTrUpList = GetMovingAverageList(stockData, maType, fastLength, trUpList);
        var fastTrDnList = GetMovingAverageList(stockData, maType, fastLength, trDnList);
        var slowTrUpList = GetMovingAverageList(stockData, maType, slowLength, trUpList);
        var slowTrDnList = GetMovingAverageList(stockData, maType, slowLength, trDnList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double fastTrUp = fastTrUpList[i];
            double fastTrDn = fastTrDnList[i];
            double slowTrUp = slowTrUpList[i];
            double slowTrDn = slowTrDnList[i];
            double fastDiff = fastTrUp - fastTrDn;
            double slowDiff = slowTrUp - slowTrDn;

            double fgi = fastDiff - slowDiff;
            fgiList.AddRounded(fgi);
        }

        var fgiEmaList = GetMovingAverageList(stockData, maType, smoothLength, fgiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double fgiEma = fgiEmaList[i];
            double prevFgiEma = i >= 1 ? fgiEmaList[i - 1] : 0;

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
        List<double> tpList = new();
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
            double rsiC = rsiCList[i];
            double rsiO = rsiOList[i];
            double rsiH = rsiHList[i];
            double rsiL = rsiLList[i];
            double prevTp1 = i >= 1 ? tpList[i - 1] : 0;
            double prevTp2 = i >= 2 ? tpList[i - 2] : 0;

            double tp = (rsiC + rsiO + rsiH + rsiL) / 4;
            tpList.AddRounded(tp);

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
        stockData.CustomValuesList = new List<double>();
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
        List<double> aList = new();
        List<double> bList = new();
        List<double> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double ema = emaList[i];
            double prevEma = i >= 1 ? emaList[i - 1] : 0;

            double a = ema < prevEma && prevEma != 0 ? ema / prevEma : 0;
            aList.AddRounded(a);

            double b = ema > prevEma && prevEma != 0 ? ema / prevEma : 0;
            bList.AddRounded(b);
        }

        var aEmaList = GetMovingAverageList(stockData, maType, length, aList);
        var bEmaList = GetMovingAverageList(stockData, maType, length, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ema = emaList[i];
            double prevEma = i >= 1 ? emaList[i - 1] : 0;
            double a = aEmaList[i];
            double b = bEmaList[i];
            double prevD1 = i >= 1 ? dList[i - 1] : 0;
            double prevD2 = i >= 2 ? dList[i - 2] : 0;
            double c = prevEma != 0 && ema != 0 ? MinOrMax(ema / prevEma / ((ema / prevEma) + b), 1, 0) : 0;

            double d = prevEma != 0 && ema != 0 ? MinOrMax((2 * (ema / prevEma / ((ema / prevEma) + (c * a)))) - 1, 1, 0) : 0;
            dList.AddRounded(d);

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
        List<double> diffList = new();
        List<double> lnList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        double sqrt = Sqrt(length);

        var atrList = CalculateAverageTrueRange(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentAtr = atrList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevLow = i >= length ? lowList[i - length] : 0;
            double prevHigh = i >= length ? highList[i - length] : 0;
            double rwh = currentAtr != 0 ? (currentHigh - prevLow) / currentAtr * sqrt : 0;
            double rwl = currentAtr != 0 ? (prevHigh - currentLow) / currentAtr * sqrt : 0;

            double diff = rwh - rwl;
            diffList.AddRounded(diff);
        }

        var pkList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, smoothLength, diffList);
        var mnList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length, pkList);
        stockData.CustomValuesList = pkList;
        var sdList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double pk = pkList[i];
            double mn = mnList[i];
            double sd = sdList[i];
            double prevPk = i >= 1 ? pkList[i - 1] : 0;
            double v1 = mn + (1.33m * sd) > 2.08m ? mn + (1.33m * sd) : 2.08m;
            double v2 = mn - (1.33m * sd) < -1.92m ? mn - (1.33m * sd) : -1.92m;

            double prevLn = lnList.LastOrDefault();
            double ln = prevPk >= 0 && pk > 0 ? v1 : prevPk <= 0 && pk < 0 ? v2 : 0;
            lnList.AddRounded(ln);

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
        int fastLength = 8, int slowLength = 65, int length1 = 9, int length2 = 30, int length3 = 50, int smoothLength = 3, double devFactor = 2,
        double sensitivity = 40)
    {
        List<double> ccLogList = new();
        List<double> xpAbsAvgList = new();
        List<double> kpoBufferList = new();
        List<double> x1List = new();
        List<double> x2List = new();
        List<double> xpList = new();
        List<double> xpAbsList = new();
        List<double> kppBufferList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double temp = prevValue != 0 ? currentValue / prevValue : 0;

            double ccLog = temp > 0 ? Math.Log(temp) : 0;
            ccLogList.AddRounded(ccLog);
        }

        stockData.CustomValuesList = ccLogList;
        var ccDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
        var ccDevAvgList = GetMovingAverageList(stockData, maType, length2, ccDevList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double avg = ccDevAvgList[i];
            double currentLow = lowList[i];
            double currentHigh = highList[i];

            double max1 = 0, max2 = 0;
            for (int j = fastLength; j < slowLength; j++)
            {
                double sqrtK = Sqrt(j);
                double prevLow = i >= j ? lowList[i - j] : 0;
                double prevHigh = i >= j ? highList[i - j] : 0;
                double temp1 = prevLow != 0 ? currentHigh / prevLow : 0;
                double log1 = temp1 > 0 ? Math.Log(temp1) : 0;
                max1 = Math.Max(log1 / sqrtK, max1);
                double temp2 = currentLow != 0 ? prevHigh / currentLow : 0;
                double log2 = temp2 > 0 ? Math.Log(temp2) : 0;
                max2 = Math.Max(log2 / sqrtK, max2);
            }

            double x1 = avg != 0 ? max1 / avg : 0;
            x1List.AddRounded(x1);

            double x2 = avg != 0 ? max2 / avg : 0;
            x2List.AddRounded(x2);

            double xp = sensitivity * (x1List.TakeLastExt(smoothLength).Average() - x2List.TakeLastExt(smoothLength).Average());
            xpList.AddRounded(xp);

            double xpAbs = Math.Abs(xp);
            xpAbsList.AddRounded(xpAbs);

            double xpAbsAvg = xpAbsList.TakeLastExt(length3).Average();
            xpAbsAvgList.AddRounded(xpAbsAvg);
        }

        stockData.CustomValuesList = xpAbsList;
        var xpAbsStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length3).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double xpAbsAvg = xpAbsAvgList[i];
            double xpAbsStdDev = xpAbsStdDevList[i];
            double prevKpoBuffer1 = i >= 1 ? kpoBufferList[i - 1] : 0;
            double prevKpoBuffer2 = i >= 2 ? kpoBufferList[i - 2] : 0;

            double tmpVal = xpAbsAvg + (devFactor * xpAbsStdDev);
            double maxVal = Math.Max(90, tmpVal);

            double prevKpoBuffer = kpoBufferList.LastOrDefault();
            double kpoBuffer = xpList[i];
            kpoBufferList.AddRounded(kpoBuffer);

            double kppBuffer = prevKpoBuffer1 > 0 && prevKpoBuffer1 > kpoBuffer && prevKpoBuffer1 >= prevKpoBuffer2 &&
                prevKpoBuffer1 >= maxVal ? prevKpoBuffer1 : prevKpoBuffer1 < 0 && prevKpoBuffer1 < kpoBuffer &&
                prevKpoBuffer1 <= prevKpoBuffer2 && prevKpoBuffer1 <= maxVal * -1 ? prevKpoBuffer1 :
                prevKpoBuffer1 > 0 && prevKpoBuffer1 > kpoBuffer && prevKpoBuffer1 >= prevKpoBuffer2 ? prevKpoBuffer1 :
                prevKpoBuffer1 < 0 && prevKpoBuffer1 < kpoBuffer && prevKpoBuffer1 <= prevKpoBuffer2 ? prevKpoBuffer1 : 0;
            kppBufferList.AddRounded(kppBuffer);

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
        List<double> ksdiUpList = new();
        List<double> ksdiDownList = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double temp = prevValue != 0 ? currentValue / prevValue : 0;

            double tempLog = temp > 0 ? Math.Log(temp) : 0;
            tempList.AddRounded(tempLog);
        }

        stockData.CustomValuesList = tempList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double volatility = stdDevList[i];
            double prevHigh = i >= length ? highList[i - length] : 0;
            double prevLow = i >= length ? lowList[i - length] : 0;
            double ksdiUpTemp = prevLow != 0 ? currentHigh / prevLow : 0;
            double ksdiDownTemp = prevHigh != 0 ? currentLow / prevHigh : 0;
            double ksdiUpLog = ksdiUpTemp > 0 ? Math.Log(ksdiUpTemp) : 0;
            double ksdiDownLog = ksdiDownTemp > 0 ? Math.Log(ksdiDownTemp) : 0;

            double prevKsdiUp = ksdiUpList.LastOrDefault();
            double ksdiUp = volatility != 0 ? ksdiUpLog / volatility : 0;
            ksdiUpList.AddRounded(ksdiUp);

            double prevKsdiDown = ksdiDownList.LastOrDefault();
            double ksdiDown = volatility != 0 ? ksdiDownLog / volatility : 0;
            ksdiDownList.AddRounded(ksdiDown);

            var signal = GetCompareSignal(ksdiUp - ksdiDown, prevKsdiUp - prevKsdiDown);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "KsdiUp", ksdiUpList },
            { "KsdiDn", ksdiDownList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
    public static StockData CalculateKaufmanBinaryWave(this StockData stockData, int length = 20, double fastSc = 0.6022m, double slowSc = 0.0645m,
        double filterPct = 10)
    {
        List<double> amaList = new();
        List<double> diffList = new();
        List<double> amaLowList = new();
        List<double> amaHighList = new();
        List<double> bwList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var efRatioList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double efRatio = efRatioList[i];
            double prevAma = i >= 1 ? amaList[i - 1] : currentValue;
            double smooth = Pow((efRatio * fastSc) + slowSc, 2);

            double ama = prevAma + (smooth * (currentValue - prevAma));
            amaList.AddRounded(ama);

            double diff = ama - prevAma;
            diffList.AddRounded(diff);
        }

        stockData.CustomValuesList = diffList;
        var diffStdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double ama = amaList[i];
            double diffStdDev = diffStdDevList[i];
            double prevAma = i >= 1 ? amaList[i - 1] : currentValue;
            double filter = filterPct / 100 * diffStdDev;

            double prevAmaLow = amaLowList.LastOrDefault();
            double amaLow = ama < prevAma ? ama : prevAmaLow;
            amaLowList.AddRounded(amaLow);

            double prevAmaHigh = amaHighList.LastOrDefault();
            double amaHigh = ama > prevAma ? ama : prevAmaHigh;
            amaHighList.AddRounded(amaHigh);

            double prevBw = bwList.LastOrDefault();
            double bw = ama - amaLow > filter ? 1 : amaHigh - ama > filter ? -1 : 0;
            bwList.AddRounded(bw);

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
        List<double> diffList = new();
        List<double> kList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length1 ? inputList[i - length1] : 0;
            double prevDiff = i >= length2 ? diffList[i - length2] : 0;

            double diff = MinPastValues(i, length1, currentValue - prevValue);
            diffList.AddRounded(diff);

            double k = MinPastValues(i, length2, diff - prevDiff);
            kList.AddRounded(k);
        }

        var fkList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, slowLength, kList);
        var fskList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, fastLength, fkList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double fsk = fskList[i];
            double prevFsk = i >= 1 ? fskList[i - 1] : 0;

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
        List<double> indexList = new();
        List<double> index2List = new();
        List<double> src2List = new();
        List<double> srcStList = new();
        List<double> indexStList = new();
        List<double> indexSrcList = new();
        List<double> rList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var kamaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double index = i;
            indexList.AddRounded(index);

            double indexSrc = i * currentValue;
            indexSrcList.AddRounded(indexSrc);

            double srcSrc = currentValue * currentValue;
            src2List.AddRounded(srcSrc);

            double indexIndex = index * index;
            index2List.AddRounded(indexIndex);
        }

        var indexMaList = GetMovingAverageList(stockData, maType, length, indexList);
        var indexSrcMaList = GetMovingAverageList(stockData, maType, length, indexSrcList);
        var index2MaList = GetMovingAverageList(stockData, maType, length, index2List);
        var src2MaList = GetMovingAverageList(stockData, maType, length, src2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double srcMa = kamaList[i];
            double indexMa = indexMaList[i];
            double indexSrcMa = indexSrcMaList[i];
            double index2Ma = index2MaList[i];
            double src2Ma = src2MaList[i];
            double prevR1 = i >= 1 ? rList[i - 1] : 0;
            double prevR2 = i >= 2 ? rList[i - 2] : 0;

            double indexSqrt = index2Ma - Pow(indexMa, 2);
            double indexSt = indexSqrt >= 0 ? Sqrt(indexSqrt) : 0;
            indexStList.AddRounded(indexSt);

            double srcSqrt = src2Ma - Pow(srcMa, 2);
            double srcSt = srcSqrt >= 0 ? Sqrt(srcSqrt) : 0;
            srcStList.AddRounded(srcSt);

            double a = indexSrcMa - (indexMa * srcMa);
            double b = indexSt * srcSt;

            double r = b != 0 ? a / b : 0;
            rList.AddRounded(r);

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
        int signalLength = 9, double weight1 = 1, double weight2 = 2, double weight3 = 3, double weight4 = 4)
    {
        List<double> kstList = new();
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
            double roc1 = roc1SmaList[i];
            double roc2 = roc2SmaList[i];
            double roc3 = roc3SmaList[i];
            double roc4 = roc4SmaList[i];

            double kst = (roc1 * weight1) + (roc2 * weight2) + (roc3 * weight3) + (roc4 * weight4);
            kstList.AddRounded(kst);
        }

        var kstSignalList = GetMovingAverageList(stockData, maType, signalLength, kstList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double kst = kstList[i];
            double kstSignal = kstSignalList[i];
            double prevKst = i >= 1 ? kstList[i - 1] : 0;
            double prevKstSignal = i >= 1 ? kstSignalList[i - 1] : 0;

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
        List<double> kUpList = new();
        List<double> kDownList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        double sqrtPeriod = Sqrt(length);

        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double avgTrueRange = atrList[i];
            double avgVolSma = volumeSmaList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double ratio = avgVolSma * sqrtPeriod;

            double prevKUp = kUpList.LastOrDefault();
            double kUp = avgTrueRange > 0 && ratio != 0 && currentLow != 0 ? prevHigh / currentLow / ratio : prevKUp;
            kUpList.AddRounded(kUp);

            double prevKDown = kDownList.LastOrDefault();
            double kDown = avgTrueRange > 0 && ratio != 0 && prevLow != 0 ? currentHigh / prevLow / ratio : prevKDown;
            kDownList.AddRounded(kDown);

            var signal = GetCompareSignal(kUp - kDown, prevKUp - prevKDown);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "KaseUp", kUpList },
            { "KaseDn", kDownList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
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
        List<double> tempList = new();
        List<double> numeratorList = new();
        List<double> tempLinRegList = new();
        List<double> pearsonCorrelationList = new();
        List<double> kendallCorrelationList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevKendall1 = i >= 1 ? kendallCorrelationList[i - 1] : 0;
            double prevKendall2 = i >= 2 ? kendallCorrelationList[i - 2] : 0;

            double currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            double linReg = linRegList[i];
            tempLinRegList.AddRounded(linReg);

            var pearsonCorrelation = Correlation.Pearson(tempLinRegList.TakeLastExt(length).Select(x => (double)x),
                tempList.TakeLastExt(length).Select(x => (double)x));
            pearsonCorrelation = IsValueNullOrInfinity(pearsonCorrelation) ? 0 : pearsonCorrelation;
            pearsonCorrelationList.AddRounded((double)pearsonCorrelation);

            double totalPairs = length * (double)(length - 1) / 2;
            double numerator = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                for (int k = 0; k <= j; k++)
                {
                    double prevValueJ = i >= j ? inputList[i - j] : 0;
                    double prevValueK = i >= k ? inputList[i - k] : 0;
                    double prevLinRegJ = i >= j ? linRegList[i - j] : 0;
                    double prevLinRegK = i >= k ? linRegList[i - k] : 0;
                    numerator += Math.Sign(prevLinRegJ - prevLinRegK) * Math.Sign(prevValueJ - prevValueK);
                }
            }

            double kendallCorrelation = numerator / totalPairs;
            kendallCorrelationList.AddRounded(kendallCorrelation);

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
    public static StockData CalculateKwanIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 9, 
        int smoothLength = 2)
    {
        List<double> vrList = new();
        List<double> prevList = new();
        List<double> knrpList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double priorClose = i >= length ? inputList[i - length] : 0;
            double mom = priorClose != 0 ? currentClose / priorClose * 100 : 0;
            double rsi = rsiList[i];
            double hh = highestList[i];
            double ll = lowestList[i];
            double sto = hh - ll != 0 ? (currentClose - ll) / (hh - ll) * 100 : 0;
            double prevVr = i >= smoothLength ? vrList[i - smoothLength] : 0;
            double prevKnrp1 = i >= 1 ? knrpList[i - 1] : 0;
            double prevKnrp2 = i >= 2 ? knrpList[i - 2] : 0;

            double vr = mom != 0 ? sto * rsi / mom : 0;
            vrList.AddRounded(vr);

            double prev = prevVr;
            prevList.AddRounded(prev);

            double vrSum = prevList.Sum();
            double knrp = vrSum / smoothLength;
            knrpList.AddRounded(knrp);

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
        List<double> svList = new();
        List<double> dList = new();
        List<Signal> signalsList = new();
        var (inputList1, highList1, lowList1, _, _) = GetInputValuesList(stockData);
        var (inputList2, highList2, lowList2, _, _) = GetInputValuesList(marketData);
        var (highestList1, lowestList1) = GetMaxAndMinValuesList(highList1, lowList1, length);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(highList2, lowList2, length);

        if (stockData.Count == marketData.Count)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                double highestHigh1 = highestList1[i];
                double lowestLow1 = lowestList1[i];
                double highestHigh2 = highestList2[i];
                double lowestLow2 = lowestList2[i];
                double currentValue1 = inputList1[i];
                double currentValue2 = inputList2[i];
                double prevSv1 = i >= 1 ? svList[i - 1] : 0;
                double prevSv2 = i >= 2 ? svList[i - 2] : 0;
                double r1 = highestHigh1 - lowestLow1;
                double r2 = highestHigh2 - lowestLow2;
                double s1 = r1 != 0 ? (currentValue1 - lowestLow1) / r1 : 50;
                double s2 = r2 != 0 ? (currentValue2 - lowestLow2) / r2 : 50;

                double d = s1 - s2;
                dList.AddRounded(d);

                var list = dList.TakeLastExt(length).ToList();
                double highestD = list.Max();
                double lowestD = list.Min();
                double r11 = highestD - lowestD;

                double sv = r11 != 0 ? MinOrMax(100 * (d - lowestD) / r11, 100, 0) : 50;
                svList.AddRounded(sv);

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
        int length1 = 5, int length2 = 100, double level = 8)
    {
        List<double> tempList = new();
        List<double> rangeList = new();
        List<double> iBuff116List = new();
        List<double> iBuff112List = new();
        List<double> iBuff109List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var wmaList = GetMovingAverageList(stockData, maType, length2, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double wma = wmaList[i];
            double prevBuff109_1 = i >= 1 ? iBuff109List[i - 1] : 0;
            double prevBuff109_2 = i >= 2 ? iBuff109List[i - 2] : 0;

            double medianPrice = inputList[i];
            tempList.AddRounded(medianPrice);

            double range = currentHigh - currentLow;
            rangeList.AddRounded(range);

            double gd120 = tempList.TakeLastExt(length1).Sum();
            double gd128 = gd120 * 0.2;
            double gd121 = rangeList.TakeLastExt(length1).Sum();
            double gd136 = gd121 * 0.2 * 0.2;

            double prevIBuff116 = iBuff116List.LastOrDefault();
            double iBuff116 = gd136 != 0 ? (currentLow - gd128) / gd136 : 0;
            iBuff116List.AddRounded(iBuff116);

            double prevIBuff112 = iBuff112List.LastOrDefault();
            double iBuff112 = gd136 != 0 ? (currentHigh - gd128) / gd136 : 0;
            iBuff112List.AddRounded(iBuff112);

            double iBuff108 = iBuff112 > level && currentHigh > wma ? 90 : iBuff116 < -level && currentLow < wma ? -90 : 0;
            double iBuff109 = (iBuff112 > level && prevIBuff112 > level) || (iBuff116 < -level && prevIBuff116 < -level) ? 0 : iBuff108;
            iBuff109List.AddRounded(iBuff109);

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
        List<double> vOpenList = new();
        List<double> vHighList = new();
        List<double> vLowList = new();
        List<double> vCloseList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        int varp = MinOrMax((int)Math.Ceiling((double)length / 5));

        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, varp);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = closeList[i];
            double prevClose1 = i >= 1 ? closeList[i - 1] : 0;
            double prevHighest1 = i >= 1 ? highestList[i - 1] : 0;
            double prevLowest1 = i >= 1 ? lowestList[i - 1] : 0;
            double prevClose2 = i >= 2 ? closeList[i - 2] : 0;
            double prevHighest2 = i >= 2 ? highestList[i - 2] : 0;
            double prevLowest2 = i >= 2 ? lowestList[i - 2] : 0;
            double prevClose3 = i >= 3 ? closeList[i - 3] : 0;
            double prevHighest3 = i >= 3 ? highestList[i - 3] : 0;
            double prevLowest3 = i >= 3 ? lowestList[i - 3] : 0;
            double prevClose4 = i >= 4 ? closeList[i - 4] : 0;
            double prevHighest4 = i >= 4 ? highestList[i - 4] : 0;
            double prevLowest4 = i >= 4 ? lowestList[i - 4] : 0;
            double prevClose5 = i >= 5 ? closeList[i - 5] : 0;
            double mba = smaList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double vara = highest - lowest;
            double varr1 = vara == 0 && varp == 1 ? Math.Abs(currentClose - prevClose1) : vara;
            double varb = prevHighest1 - prevLowest1;
            double varr2 = varb == 0 && varp == 1 ? Math.Abs(prevClose1 - prevClose2) : varb;
            double varc = prevHighest2 - prevLowest2;
            double varr3 = varc == 0 && varp == 1 ? Math.Abs(prevClose2 - prevClose3) : varc;
            double vard = prevHighest3 - prevLowest3;
            double varr4 = vard == 0 && varp == 1 ? Math.Abs(prevClose3 - prevClose4) : vard;
            double vare = prevHighest4 - prevLowest4;
            double varr5 = vare == 0 && varp == 1 ? Math.Abs(prevClose4 - prevClose5) : vare;
            double cdelta = Math.Abs(currentClose - prevClose1);
            double var0 = cdelta > currentHigh - currentLow || currentHigh == currentLow ? cdelta : currentHigh - currentLow;
            double lRange = (varr1 + varr2 + varr3 + varr4 + varr5) / 5 * 0.2;

            double vClose = lRange != 0 ? (currentClose - mba) / lRange : 0;
            vCloseList.AddRounded(vClose);

            double vOpen = lRange != 0 ? (currentOpen - mba) / lRange : 0;
            vOpenList.AddRounded(vOpen);

            double vHigh = lRange != 0 ? (currentHigh - mba) / lRange : 0;
            vHighList.AddRounded(vHigh);

            double vLow = lRange != 0 ? (currentLow - mba) / lRange : 0;
            vLowList.AddRounded(vLow);
        }

        var vValueEmaList = GetMovingAverageList(stockData, maType, length, vCloseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double vValue = vCloseList[i];
            double vValueEma = vValueEmaList[i];
            double prevVvalue = i >= 1 ? vCloseList[i - 1] : 0;
            double prevVValueEma = i >= 1 ? vValueEmaList[i - 1] : 0;

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
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ValueChartIndicator;

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
        int length1 = 18, int length2 = 30, int length3 = 2, int smoothLength = 3, double stdDevMult = 2)
    {
        List<double> rainbowList = new();
        List<double> zlrbList = new();
        List<double> zlrbpercbList = new();
        List<double> rbcList = new();
        List<double> fastKList = new();
        List<double> skList = new();
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
            double r1 = r1List[i];
            double r2 = r2List[i];
            double r3 = r3List[i];
            double r4 = r4List[i];
            double r5 = r5List[i];
            double r6 = r6List[i];
            double r7 = r7List[i];
            double r8 = r8List[i];
            double r9 = r9List[i];
            double r10 = r10List[i];

            double rainbow = ((5 * r1) + (4 * r2) + (3 * r3) + (2 * r4) + r5 + r6 + r7 + r8 + r9 + r10) / 20;
            rainbowList.AddRounded(rainbow);
        }

        var ema1List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, smoothLength, rainbowList);
        var ema2List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, smoothLength, ema1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ema1 = ema1List[i];
            double ema2 = ema2List[i];

            double zlrb = (2 * ema1) - ema2;
            zlrbList.AddRounded(zlrb);
        }

        var tzList = GetMovingAverageList(stockData, MovingAvgType.TripleExponentialMovingAverage, smoothLength, zlrbList);
        stockData.CustomValuesList = tzList;
        var hwidthList = CalculateStandardDeviationVolatility(stockData, length: length1).CustomValuesList;
        var wmatzList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, length1, tzList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentTypicalPrice = inputList[i];
            double rainbow = rainbowList[i];
            double tz = tzList[i];
            double hwidth = hwidthList[i];
            double wmatz = wmatzList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];

            double prevZlrbpercb = zlrbpercbList.LastOrDefault();
            double zlrbpercb = hwidth != 0 ? (tz + (stdDevMult * hwidth) - wmatz) / (2 * stdDevMult * hwidth * 100) : 0;
            zlrbpercbList.AddRounded(zlrbpercb);

            double rbc = (rainbow + currentTypicalPrice) / 2;
            rbcList.AddRounded(rbc);

            double lowestRbc = rbcList.TakeLastExt(length2).Min();
            double nom = rbc - lowest;
            double den = highest - lowestRbc;

            double fastK = den != 0 ? MinOrMax(100 * nom / den, 100, 0) : 0;
            fastKList.AddRounded(fastK);

            double prevSk = skList.LastOrDefault();
            double sk = fastKList.TakeLastExt(smoothLength).Average();
            skList.AddRounded(sk);

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
        double factor = 1.1m)
    {
        List<double> haoList = new();
        List<double> hacList = new();
        List<double> medianPriceList = new();
        List<bool> keepN1List = new();
        List<bool> keepAll1List = new();
        List<bool> keepN2List = new();
        List<bool> keepAll2List = new();
        List<bool> utrList = new();
        List<bool> dtrList = new();
        List<double> hacoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevHao = haoList.LastOrDefault();
            double hao = (prevValue + prevHao) / 2;
            haoList.AddRounded(hao);

            double hac = (currentValue + hao + Math.Max(currentHigh, hao) + Math.Min(currentLow, hao)) / 4;
            hacList.AddRounded(hac);

            double medianPrice = (currentHigh + currentLow) / 2;
            medianPriceList.AddRounded(medianPrice);
        }

        var tacList = GetMovingAverageList(stockData, maType, length, hacList);
        var thl2List = GetMovingAverageList(stockData, maType, length, medianPriceList);
        var tacTemaList = GetMovingAverageList(stockData, maType, length, tacList);
        var thl2TemaList = GetMovingAverageList(stockData, maType, length, thl2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tac = tacList[i];
            double tacTema = tacTemaList[i];
            double thl2 = thl2List[i];
            double thl2Tema = thl2TemaList[i];
            double currentOpen = openList[i];
            double currentClose = closeList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double hac = hacList[i];
            double hao = haoList[i];
            double prevHac = i >= 1 ? hacList[i - 1] : 0;
            double prevHao = i >= 1 ? haoList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevClose = i >= 1 ? closeList[i - 1] : 0;
            double hacSmooth = (2 * tac) - tacTema;
            double hl2Smooth = (2 * thl2) - thl2Tema;

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
            bool dtr = (keepAll2 || prevKeepAll2) && keep23;
            dtrList.Add(dtr);

            bool upw = dtr == false && prevDtr && utr;
            bool dnw = utr == false && prevUtr && dtr;
            double prevHaco = hacoList.LastOrDefault();
            double haco = upw ? 1 : dnw ? -1 : prevHaco;
            hacoList.AddRounded(haco);

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
    /// Calculates the Vix Trading System
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="maxCount"></param>
    /// <param name="minCount"></param>
    /// <returns></returns>
    public static StockData CalculateVixTradingSystem(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 50, double maxCount = 11, double minCount = -11)
    {
        List<double> countList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double vixts = smaList[i];

            double prevCount = countList.LastOrDefault();
            double count = currentValue > vixts && prevCount >= 0 ? prevCount + 1 : currentValue <= vixts && prevCount <= 0 ?
                prevCount - 1 : prevCount;
            countList.AddRounded(count);

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
    /// Calculates the Varadi Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVaradiOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14)
    {
        List<double> dvoList = new();
        List<double> ratioList = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double median = (currentHigh + currentLow) / 2;

            double ratio = median != 0 ? currentValue / median : 0;
            ratioList.AddRounded(ratio);
        }

        var aList = GetMovingAverageList(stockData, maType, length, ratioList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double a = aList[i];
            double prevDvo1 = i >= 1 ? dvoList[i - 1] : 0;
            double prevDvo2 = i >= 2 ? dvoList[i - 2] : 0;

            double prevA = i >= 1 ? aList[i - 1] : 0;
            tempList.AddRounded(prevA);

            double dvo = MinOrMax((double)tempList.TakeLastExt(length).Where(i => i <= a).Count() / length * 100, 100, 0);
            dvoList.AddRounded(dvo);

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
        List<double> osList = new();
        List<double> fList = new();
        List<double> dosList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentOpen = openList[i];
            double currentValue = inputList[i];
            double prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            double prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            double prevValue3 = i >= 3 ? inputList[i - 3] : 0;
            double up = prevValue3 > prevValue2 && prevValue1 > prevValue2 && currentValue < prevValue2 ? 1 : 0;
            double dn = prevValue3 < prevValue2 && prevValue1 < prevValue2 && currentValue > prevValue2 ? 1 : 0;

            double prevOs = osList.LastOrDefault();
            double os = up == 1 ? 1 : dn == 1 ? 0 : prevOs;
            osList.AddRounded(os);

            double prevF = fList.LastOrDefault();
            double f = os == 1 && currentValue > currentOpen ? 1 : os == 0 && currentValue < currentOpen ? 0 : prevF;
            fList.AddRounded(f);

            double prevDos = dosList.LastOrDefault();
            double dos = os - prevOs;
            dosList.AddRounded(dos);

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
        List<double> haoList = new();
        List<double> hacList = new();
        List<double> medianPriceList = new();
        List<bool> dnKeepingList = new();
        List<bool> dnKeepAllList = new();
        List<bool> dnTrendList = new();
        List<bool> upKeepingList = new();
        List<bool> upKeepAllList = new();
        List<bool> upTrendList = new();
        List<double> hacoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevHao = haoList.LastOrDefault();
            double hao = (prevValue + prevHao) / 2;
            haoList.AddRounded(hao);

            double hac = (currentValue + hao + Math.Max(currentHigh, hao) + Math.Min(currentLow, hao)) / 4;
            hacList.AddRounded(hac);

            double medianPrice = (currentHigh + currentLow) / 2;
            medianPriceList.AddRounded(medianPrice);
        }

        var tma1List = GetMovingAverageList(stockData, maType, length, hacList);
        var tma2List = GetMovingAverageList(stockData, maType, length, tma1List);
        var tma12List = GetMovingAverageList(stockData, maType, length, medianPriceList);
        var tma22List = GetMovingAverageList(stockData, maType, length, tma12List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tma1 = tma1List[i];
            double tma2 = tma2List[i];
            double tma12 = tma12List[i];
            double tma22 = tma22List[i];
            double hao = haoList[i];
            double hac = hacList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double currentClose = closeList[i];
            double prevHao = i >= 1 ? haoList[i - 1] : 0;
            double prevHac = i >= 1 ? hacList[i - 1] : 0;
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevClose = i >= 1 ? closeList[i - 1] : 0;
            double diff = tma1 - tma2;
            double zlHa = tma1 + diff;
            double diff2 = tma12 - tma22;
            double zlCl = tma12 + diff2;
            double zlDiff = zlCl - zlHa;
            bool dnKeep1 = hac < hao && prevHac < prevHao;
            bool dnKeep2 = zlDiff < 0;
            bool dnKeep3 = Math.Abs(currentClose - currentOpen) < (currentHigh - currentLow) * 0.35 && currentLow <= prevHigh;

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
            bool upKeep3 = Math.Abs(currentClose - currentOpen) < (currentHigh - currentLow) * 0.35 && currentHigh >= prevLow;

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

            double prevHaco = hacoList.LastOrDefault();
            double haco = upw ? 1 : dnw ? -1 : prevHaco;
            hacoList.AddRounded(haco);

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

    /// <summary>
    /// Calculates the Chartmill Value Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateChartmillValueIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        InputName inputName = InputName.MedianPrice, int length = 5)
    {
        List<double> cmvCList = new();
        List<double> cmvOList = new();
        List<double> cmvHList = new();
        List<double> cmvLList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        var fList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double v = atrList[i];
            double f = fList[i];
            double prevCmvc1 = i >= 1 ? cmvCList[i - 1] : 0;
            double prevCmvc2 = i >= 2 ? cmvCList[i - 2] : 0;
            double currentClose = closeList[i];
            double currentOpen = openList[i];

            double cmvC = v != 0 ? MinOrMax((currentClose - f) / (v * Pow(length, 0.5)), 1, -1) : 0;
            cmvCList.AddRounded(cmvC);

            double cmvO = v != 0 ? MinOrMax((currentOpen - f) / (v * Pow(length, 0.5)), 1, -1) : 0;
            cmvOList.AddRounded(cmvO);

            double cmvH = v != 0 ? MinOrMax((currentHigh - f) / (v * Pow(length, 0.5)), 1, -1) : 0;
            cmvHList.AddRounded(cmvH);

            double cmvL = v != 0 ? MinOrMax((currentLow - f) / (v * Pow(length, 0.5)), 1, -1) : 0;
            cmvLList.AddRounded(cmvL);

            var signal = GetRsiSignal(cmvC - prevCmvc1, prevCmvc1 - prevCmvc2, cmvC, prevCmvc1, 0.5, -0.5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cmvc", cmvCList },
            { "Cmvo", cmvOList },
            { "Cmvh", cmvHList },
            { "Cmvl", cmvLList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ChartmillValueIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Conditional Accumulator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="increment"></param>
    /// <returns></returns>
    public static StockData CalculateConditionalAccumulator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14, double increment = 1)
    {
        List<double> valueList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;

            double prevValue = valueList.LastOrDefault();
            double value = currentLow >= prevHigh ? prevValue + increment : currentHigh <= prevLow ? prevValue - increment : prevValue;
            valueList.AddRounded(value);
        }

        var valueEmaList = GetMovingAverageList(stockData, maType, length, valueList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double value = valueList[i];
            double valueEma = valueEmaList[i];
            double prevValue = i >= 1 ? valueList[i - 1] : 0;
            double prevValueEma = i >= 1 ? valueEmaList[i - 1] : 0;

            var signal = GetCompareSignal(value - valueEma, prevValue - prevValueEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ca", valueList },
            { "Signal", valueEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = valueList;
        stockData.IndicatorName = IndicatorName.ConditionalAccumulator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Contract High Low Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateContractHighLow(this StockData stockData)
    {
        List<double> conHiList = new();
        List<double> conLowList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];

            double prevConHi = conHiList.LastOrDefault();
            double conHi = i >= 1 ? Math.Max(prevConHi, currentHigh) : currentHigh;
            conHiList.AddRounded(conHi);

            double prevConLow = conLowList.LastOrDefault();
            double conLow = i >= 1 ? Math.Min(prevConLow, currentLow) : currentLow;
            conLowList.AddRounded(conLow);

            var signal = GetConditionSignal(conHi > prevConHi, conLow < prevConLow);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ch", conHiList },
            { "Cl", conLowList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ContractHighLow;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chop Zone Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateChopZone(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        InputName inputName = InputName.TypicalPrice, int length1 = 30, int length2 = 34)
    {
        List<double> emaAngleList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        var emaList = GetMovingAverageList(stockData, maType, length2, closeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double ema = emaList[i];
            double prevEma = i >= 1 ? emaList[i - 1] : 0;
            double range = highest - lowest != 0 ? 25 / (highest - lowest) * lowest : 0;
            double avg = inputList[i];
            double y = avg != 0 && range != 0 ? (prevEma - ema) / avg * range : 0;
            double c = Sqrt(1 + (y * y));
            double emaAngle1 = c != 0 ? Math.Round(Math.Acos(1 / c).ToDegrees()) : 0;

            double prevEmaAngle = emaAngleList.LastOrDefault();
            double emaAngle = y > 0 ? -emaAngle1 : emaAngle1;
            emaAngleList.AddRounded(emaAngle);

            var signal = GetCompareSignal(emaAngle, prevEmaAngle);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cz", emaAngleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emaAngleList;
        stockData.IndicatorName = IndicatorName.ChopZone;

        return stockData;
    }

    /// <summary>
    /// Calculates the Center of Linearity
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateCenterOfLinearity(this StockData stockData, int length = 14)
    {
        List<double> aList = new();
        List<double> colList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double priorValue = i >= length ? inputList[i - length] : 0;

            double a = (i + 1) * (priorValue - prevValue);
            aList.AddRounded(a);

            double prevCol = colList.LastOrDefault();
            double col = aList.TakeLastExt(length).Sum();
            colList.AddRounded(col);

            var signal = GetCompareSignal(col, prevCol);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Col", colList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = colList;
        stockData.IndicatorName = IndicatorName.CenterOfLinearity;

        return stockData;
    }

    /// <summary>
    /// Calculates the Chaikin Volatility
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateChaikinVolatility(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 10, int length2 = 12)
    {
        List<double> chaikinVolatilityList = new();
        List<double> highLowList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];

            double highLow = currentHigh - currentLow;
            highLowList.AddRounded(highLow);
        }

        var highLowEmaList = GetMovingAverageList(stockData, maType, length1, highLowList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double highLowEma = highLowEmaList[i];
            double prevHighLowEma = i >= length2 ? highLowEmaList[i - length2] : 0;

            double prevChaikinVolatility = chaikinVolatilityList.LastOrDefault();
            double chaikinVolatility = prevHighLowEma != 0 ? (highLowEma - prevHighLowEma) / prevHighLowEma * 100 : 0;
            chaikinVolatilityList.AddRounded(chaikinVolatility);

            var signal = GetCompareSignal(chaikinVolatility, prevChaikinVolatility, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cv", chaikinVolatilityList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = chaikinVolatilityList;
        stockData.IndicatorName = IndicatorName.ChaikinVolatility;

        return stockData;
    }

    /// <summary>
    /// Calculates the Confluence Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateConfluenceIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        InputName inputName = InputName.FullTypicalPrice, int length = 10)
    {
        List<double> value5List = new();
        List<double> value6List = new();
        List<double> value7List = new();
        List<double> momList = new();
        List<double> sumList = new();
        List<double> errSumList = new();
        List<double> value70List = new();
        List<double> confluenceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, closeList, _) = GetInputValuesList(inputName, stockData);

        int stl = (int)Math.Ceiling((length * 2) - 1 - 0.5m);
        int itl = (int)Math.Ceiling((stl * 2) - 1 - 0.5m);
        int ltl = (int)Math.Ceiling((itl * 2) - 1 - 0.5m);
        int hoff = (int)Math.Ceiling(((double)length / 2) - 0.5);
        int soff = (int)Math.Ceiling(((double)stl / 2) - 0.5);
        int ioff = (int)Math.Ceiling(((double)itl / 2) - 0.5);
        int hLength = MinOrMax(length - 1);
        int sLength = stl - 1;
        int iLength = itl - 1;
        int lLength = ltl - 1;

        var hAvgList = GetMovingAverageList(stockData, maType, length, closeList);
        var sAvgList = GetMovingAverageList(stockData, maType, stl, closeList);
        var iAvgList = GetMovingAverageList(stockData, maType, itl, closeList);
        var lAvgList = GetMovingAverageList(stockData, maType, ltl, closeList);
        var h2AvgList = GetMovingAverageList(stockData, maType, hLength, closeList);
        var s2AvgList = GetMovingAverageList(stockData, maType, sLength, closeList);
        var i2AvgList = GetMovingAverageList(stockData, maType, iLength, closeList);
        var l2AvgList = GetMovingAverageList(stockData, maType, lLength, closeList);
        var ftpAvgList = GetMovingAverageList(stockData, maType, lLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double sAvg = sAvgList[i];
            double priorSAvg = i >= soff ? sAvgList[i - soff] : 0;
            double priorHAvg = i >= hoff ? hAvgList[i - hoff] : 0;
            double iAvg = iAvgList[i];
            double priorIAvg = i >= ioff ? iAvgList[i - ioff] : 0;
            double lAvg = lAvgList[i];
            double hAvg = hAvgList[i];
            double prevSAvg = i >= 1 ? sAvgList[i - 1] : 0;
            double prevHAvg = i >= 1 ? hAvgList[i - 1] : 0;
            double prevIAvg = i >= 1 ? iAvgList[i - 1] : 0;
            double prevLAvg = i >= 1 ? lAvgList[i - 1] : 0;
            double h2 = h2AvgList[i];
            double s2 = s2AvgList[i];
            double i2 = i2AvgList[i];
            double l2 = l2AvgList[i];
            double ftpAvg = ftpAvgList[i];
            double priorValue5 = i >= hoff ? value5List[i - hoff] : 0;
            double priorValue6 = i >= soff ? value6List[i - soff] : 0;
            double priorValue7 = i >= ioff ? value7List[i - ioff] : 0;
            double priorSum = i >= soff ? sumList[i - soff] : 0;
            double priorHAvg2 = i >= soff ? hAvgList[i - soff] : 0;
            double prevErrSum = i >= 1 ? errSumList[i - 1] : 0;
            double prevMom = i >= 1 ? momList[i - 1] : 0;
            double prevValue70 = i >= 1 ? value70List[i - 1] : 0;
            double prevConfluence1 = i >= 1 ? confluenceList[i - 1] : 0;
            double prevConfluence2 = i >= 2 ? confluenceList[i - 2] : 0;
            double value2 = sAvg - priorHAvg;
            double value3 = iAvg - priorSAvg;
            double value12 = lAvg - priorIAvg;
            double momSig = value2 + value3 + value12;
            double derivH = (hAvg * 2) - prevHAvg;
            double derivS = (sAvg * 2) - prevSAvg;
            double derivI = (iAvg * 2) - prevIAvg;
            double derivL = (lAvg * 2) - prevLAvg;
            double sumDH = length * derivH;
            double sumDS = stl * derivS;
            double sumDI = itl * derivI;
            double sumDL = ltl * derivL;
            double n1h = h2 * hLength;
            double n1s = s2 * sLength;
            double n1i = i2 * iLength;
            double n1l = l2 * lLength;
            double drh = sumDH - n1h;
            double drs = sumDS - n1s;
            double dri = sumDI - n1i;
            double drl = sumDL - n1l;
            double hSum = h2 * (length - 1);
            double sSum = s2 * (stl - 1);
            double iSum = i2 * (itl - 1);
            double lSum = ftpAvg * (ltl - 1);

            double value5 = (hSum + drh) / length;
            value5List.AddRounded(value5);

            double value6 = (sSum + drs) / stl;
            value6List.AddRounded(value6);

            double value7 = (iSum + dri) / itl;
            value7List.AddRounded(value7);

            double value13 = (lSum + drl) / ltl;
            double value9 = value6 - priorValue5;
            double value10 = value7 - priorValue6;
            double value14 = value13 - priorValue7;

            double mom = value9 + value10 + value14;
            momList.AddRounded(mom);

            double ht = Math.Sin(value5 * 2 * Math.PI / 360) + Math.Cos(value5 * 2 * Math.PI / 360);
            double hta = Math.Sin(hAvg * 2 * Math.PI / 360) + Math.Cos(hAvg * 2 * Math.PI / 360);
            double st = Math.Sin(value6 * 2 * Math.PI / 360) + Math.Cos(value6 * 2 * Math.PI / 360);
            double sta = Math.Sin(sAvg * 2 * Math.PI / 360) + Math.Cos(sAvg * 2 * Math.PI / 360);
            double it = Math.Sin(value7 * 2 * Math.PI / 360) + Math.Cos(value7 * 2 * Math.PI / 360);
            double ita = Math.Sin(iAvg * 2 * Math.PI / 360) + Math.Cos(iAvg * 2 * Math.PI / 360);

            double sum = ht + st + it;
            sumList.AddRounded(sum);

            double err = hta + sta + ita;
            double cond2 = (sum > priorSum && hAvg < priorHAvg2) || (sum < priorSum && hAvg > priorHAvg2) ? 1 : 0;
            double phase = cond2 == 1 ? -1 : 1;

            double errSum = (sum - err) * phase;
            errSumList.AddRounded(errSum);

            double value70 = value5 - value13;
            value70List.AddRounded(value70);

            double errSig = errSumList.TakeLastExt(soff).Average();
            double value71 = value70List.TakeLastExt(length).Average();
            double errNum = errSum > 0 && errSum < prevErrSum && errSum < errSig ? 1 : errSum > 0 && errSum < prevErrSum && errSum > errSig ? 2 :
                errSum > 0 && errSum > prevErrSum && errSum < errSig ? 2 : errSum > 0 && errSum > prevErrSum && errSum > errSig ? 3 :
                errSum < 0 && errSum > prevErrSum && errSum > errSig ? -1 : errSum < 0 && errSum < prevErrSum && errSum > errSig ? -2 :
                errSum < 0 && errSum > prevErrSum && errSum < errSig ? -2 : errSum < 0 && errSum < prevErrSum && errSum < errSig ? -3 : 0;
            double momNum = mom > 0 && mom < prevMom && mom < momSig ? 1 : mom > 0 && mom < prevMom && mom > momSig ? 2 :
                mom > 0 && mom > prevMom && mom < momSig ? 2 : mom > 0 && mom > prevMom && mom > momSig ? 3 :
                mom < 0 && mom > prevMom && mom > momSig ? -1 : mom < 0 && mom < prevMom && mom > momSig ? -2 :
                mom < 0 && mom > prevMom && mom < momSig ? -2 : mom < 0 && mom < prevMom && mom < momSig ? -3 : 0;
            double tcNum = value70 > 0 && value70 < prevValue70 && value70 < value71 ? 1 : value70 > 0 && value70 < prevValue70 && value70 > value71 ? 2 :
                value70 > 0 && value70 > prevValue70 && value70 < value71 ? 2 : value70 > 0 && value70 > prevValue70 && value70 > value71 ? 3 :
                value70 < 0 && value70 > prevValue70 && value70 > value71 ? -1 : value70 < 0 && value70 < prevValue70 && value70 > value71 ? -2 :
                value70 < 0 && value70 > prevValue70 && value70 < value71 ? -2 : value70 < 0 && value70 < prevValue70 && value70 < value71 ? -3 : 0;
            double value42 = errNum + momNum + tcNum;

            double confluence = value42 > 0 && value70 > 0 ? value42 : value42 < 0 && value70 < 0 ? value42 :
                (value42 > 0 && value70 < 0) || (value42 < 0 && value70 > 0) ? value42 / 10 : 0;
            confluenceList.AddRounded(confluence);

            double res1 = confluence >= 1 ? confluence : 0;
            double res2 = confluence <= -1 ? confluence : 0;
            double res3 = confluence == 0 ? 0 : confluence > -1 && confluence < 1 ? 10 * confluence : 0;

            var signal = GetCompareSignal(confluence - prevConfluence1, prevConfluence1 - prevConfluence2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ci", confluenceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = confluenceList;
        stockData.IndicatorName = IndicatorName.ConfluenceIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Coppock Curve
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateCoppockCurve(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 10,
        int fastLength = 11, int slowLength = 14)
    {
        List<double> rocTotalList = new();
        List<Signal> signalsList = new();

        var roc11List = CalculateRateOfChange(stockData, fastLength).CustomValuesList;
        var roc14List = CalculateRateOfChange(stockData, slowLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentRoc11 = roc11List[i];
            double currentRoc14 = roc14List[i];

            double rocTotal = currentRoc11 + currentRoc14;
            rocTotalList.AddRounded(rocTotal);
        }

        var coppockCurveList = GetMovingAverageList(stockData, maType, length, rocTotalList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double coppockCurve = coppockCurveList[i];
            double prevCoppockCurve = i >= 1 ? coppockCurveList[i - 1] : 0;

            var signal = GetCompareSignal(coppockCurve, prevCoppockCurve);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cc", coppockCurveList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = coppockCurveList;
        stockData.IndicatorName = IndicatorName.CoppockCurve;

        return stockData;
    }

    /// <summary>
    /// Calculates the Constance Brown Composite Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateConstanceBrownCompositeIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int fastLength = 13, int slowLength = 33, int length1 = 14, int length2 = 9, int smoothLength = 3)
    {
        List<double> sList = new();
        List<double> bullSlopeList = new();
        List<double> bearSlopeList = new();
        List<Signal> signalsList = new();

        var rsi1List = CalculateRelativeStrengthIndex(stockData, length: length1).CustomValuesList;
        var rsi2List = CalculateRelativeStrengthIndex(stockData, length: smoothLength).CustomValuesList;
        var rsiSmaList = GetMovingAverageList(stockData, maType, smoothLength, rsi2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsiSma = rsiSmaList[i];
            double rsiDelta = i >= length2 ? rsi1List[i - length2] : 0;

            double s = rsiDelta + rsiSma;
            sList.AddRounded(s);
        }

        var sFastSmaList = GetMovingAverageList(stockData, maType, fastLength, sList);
        var sSlowSmaList = GetMovingAverageList(stockData, maType, slowLength, sList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double s = sList[i];
            double sFastSma = sFastSmaList[i];
            double sSlowSma = sSlowSmaList[i];

            double prevBullSlope = bullSlopeList.LastOrDefault();
            double bullSlope = s - Math.Max(sFastSma, sSlowSma);
            bullSlopeList.AddRounded(bullSlope);

            double prevBearSlope = bearSlopeList.LastOrDefault();
            double bearSlope = s - Math.Min(sFastSma, sSlowSma);
            bearSlopeList.AddRounded(bearSlope);

            var signal = GetBullishBearishSignal(bullSlope, prevBullSlope, bearSlope, prevBearSlope);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cbci", sList },
            { "FastSignal", sFastSmaList },
            { "SlowSignal", sSlowSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sList;
        stockData.IndicatorName = IndicatorName.ConstanceBrownCompositeIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Commodity Selection Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="pointValue"></param>
    /// <param name="margin"></param>
    /// <param name="commission"></param>
    /// <returns></returns>
    public static StockData CalculateCommoditySelectionIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14, double pointValue = 50, double margin = 3000, double commission = 10)
    {
        List<double> csiList = new();
        List<double> csiSmaList = new();
        List<Signal> signalsList = new();

        double k = 100 * (pointValue / Sqrt(margin) / (150 + commission));

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        var adxList = CalculateAverageDirectionalIndex(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double atr = atrList[i];
            double adxRating = adxList[i];

            double prevCsi = csiList.LastOrDefault();
            double csi = k * atr * adxRating;
            csiList.AddRounded(csi);

            double prevCsiSma = csiSmaList.LastOrDefault();
            double csiSma = csiList.TakeLastExt(length).Average();
            csiSmaList.AddRounded(csiSma);

            var signal = GetCompareSignal(csi - csiSma, prevCsi - prevCsiSma, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Csi", csiList },
            { "Signal", csiSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = csiList;
        stockData.IndicatorName = IndicatorName.CommoditySelectionIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Math.PIvot Detector Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculatePivotDetectorOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length1 = 200, int length2 = 14)
    {
        List<double> pdoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length2).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double rsi = rsiList[i];
            double prevPdo1 = i >= 1 ? pdoList[i - 1] : 0;
            double prevPdo2 = i >= 2 ? pdoList[i - 2] : 0;

            double pdo = currentValue > sma ? (rsi - 35) / (85 - 35) * 100 : currentValue <= sma ? (rsi - 20) / (70 - 20) * 100 : 0;
            pdoList.AddRounded(pdo);

            var signal = GetCompareSignal(pdo - prevPdo1, prevPdo1 - prevPdo2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pdo", pdoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pdoList;
        stockData.IndicatorName = IndicatorName.PivotDetectorOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Percent Change Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePercentChangeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14)
    {
        List<double> percentChangeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevPcc = percentChangeList.LastOrDefault();
            double pcc = prevValue - 1 != 0 ? prevPcc + (currentValue / (prevValue - 1)) : 0;
            percentChangeList.AddRounded(pcc);
        }

        var pctChgWmaList = GetMovingAverageList(stockData, maType, length, percentChangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double pcc = percentChangeList[i];
            double pccWma = pctChgWmaList[i];
            double prevPcc = i >= 1 ? percentChangeList[i - 1] : 0;
            double prevPccWma = i >= 1 ? pctChgWmaList[i - 1] : 0;

            var signal = GetCompareSignal(pcc - pccWma, prevPcc - prevPccWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pcco", percentChangeList },
            { "Signal", pctChgWmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = percentChangeList;
        stockData.IndicatorName = IndicatorName.PercentChangeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Prime Number Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePrimeNumberOscillator(this StockData stockData, int length = 5)
    {
        List<double> pnoList = new();
        List<double> pno1List = new();
        List<double> pno2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double ratio = currentValue * length / 100;
            long convertedValue = (long)Math.Round(currentValue);
            long sqrtValue = currentValue >= 0 ? (long)Math.Round(Sqrt(currentValue)) : 0;
            long maxValue = (long)Math.Round(currentValue + ratio);
            long minValue = (long)Math.Round(currentValue - ratio);

            double pno1 = 0, pno2 = 0;
            for (long j = convertedValue; j <= maxValue; j++)
            {
                pno1 = j;
                for (int k = 2; k <= sqrtValue; k++)
                {
                    pno1 = j % k == 0 ? 0 : j;
                    if (pno1 == 0)
                    {
                        break;
                    }
                }

                if (pno1 > 0)
                {
                    break;
                }
            }
            pno1 = pno1 == 0 ? pno1List.LastOrDefault() : pno1;
            pno1List.AddRounded(pno1);

            for (long l = convertedValue; l >= minValue; l--)
            {
                pno2 = l;
                for (int m = 2; m <= sqrtValue; m++)
                {
                    pno2 = l % m == 0 ? 0 : l;
                    if (pno2 == 0)
                    {
                        break;
                    }
                }

                if (pno2 > 0)
                {
                    break;
                }
            }
            pno2 = pno2 == 0 ? pno2List.LastOrDefault() : pno2;
            pno2List.AddRounded(pno2);

            double prevPno = pnoList.LastOrDefault();
            double pno = pno1 - currentValue < currentValue - pno2 ? pno1 - currentValue : pno2 - currentValue;
            pno = pno == 0 ? prevPno : pno;
            pnoList.AddRounded(pno);

            var signal = GetCompareSignal(pno, prevPno);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pno", pnoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pnoList;
        stockData.IndicatorName = IndicatorName.PrimeNumberOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Pring Special K
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
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculatePringSpecialK(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 10,
        int length2 = 15, int length3 = 20, int length4 = 30, int length5 = 40, int length6 = 50, int length7 = 65, int length8 = 75, int length9 = 100,
        int length10 = 130, int length11 = 195, int length12 = 265, int length13 = 390, int length14 = 530, int smoothLength = 10)
    {
        List<double> specialKList = new();
        List<Signal> signalsList = new();

        var rocList = CalculateRateOfChange(stockData, length1).CustomValuesList;
        var roc15List = CalculateRateOfChange(stockData, length2).CustomValuesList;
        var roc20List = CalculateRateOfChange(stockData, length3).CustomValuesList;
        var roc30List = CalculateRateOfChange(stockData, length4).CustomValuesList;
        var roc40List = CalculateRateOfChange(stockData, length5).CustomValuesList;
        var roc65List = CalculateRateOfChange(stockData, length7).CustomValuesList;
        var roc75List = CalculateRateOfChange(stockData, length8).CustomValuesList;
        var roc100List = CalculateRateOfChange(stockData, length9).CustomValuesList;
        var roc195List = CalculateRateOfChange(stockData, length11).CustomValuesList;
        var roc265List = CalculateRateOfChange(stockData, length12).CustomValuesList;
        var roc390List = CalculateRateOfChange(stockData, length13).CustomValuesList;
        var roc530List = CalculateRateOfChange(stockData, length14).CustomValuesList;
        var roc10SmaList = GetMovingAverageList(stockData, maType, length1, rocList);
        var roc15SmaList = GetMovingAverageList(stockData, maType, length1, roc15List);
        var roc20SmaList = GetMovingAverageList(stockData, maType, length1, roc20List);
        var roc30SmaList = GetMovingAverageList(stockData, maType, length2, roc30List);
        var roc40SmaList = GetMovingAverageList(stockData, maType, length6, roc40List);
        var roc65SmaList = GetMovingAverageList(stockData, maType, length7, roc65List);
        var roc75SmaList = GetMovingAverageList(stockData, maType, length8, roc75List);
        var roc100SmaList = GetMovingAverageList(stockData, maType, length9, roc100List);
        var roc195SmaList = GetMovingAverageList(stockData, maType, length10, roc195List);
        var roc265SmaList = GetMovingAverageList(stockData, maType, length10, roc265List);
        var roc390SmaList = GetMovingAverageList(stockData, maType, length10, roc390List);
        var roc530SmaList = GetMovingAverageList(stockData, maType, length11, roc530List);

        for (int i = 0; i < stockData.Count; i++)
        {
            double roc10Sma = roc10SmaList[i];
            double roc15Sma = roc15SmaList[i];
            double roc20Sma = roc20SmaList[i];
            double roc30Sma = roc30SmaList[i];
            double roc40Sma = roc40SmaList[i];
            double roc65Sma = roc65SmaList[i];
            double roc75Sma = roc75SmaList[i];
            double roc100Sma = roc100SmaList[i];
            double roc195Sma = roc195SmaList[i];
            double roc265Sma = roc265SmaList[i];
            double roc390Sma = roc390SmaList[i];
            double roc530Sma = roc530SmaList[i];

            double specialK = (roc10Sma * 1) + (roc15Sma * 2) + (roc20Sma * 3) + (roc30Sma * 4) + (roc40Sma * 1) + (roc65Sma * 2) + (roc75Sma * 3) +
                (roc100Sma * 4) + (roc195Sma * 1) + (roc265Sma * 2) + (roc390Sma * 3) + (roc530Sma * 4);
            specialKList.AddRounded(specialK);
        }

        var specialKSignalList = GetMovingAverageList(stockData, maType, smoothLength, specialKList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double specialK = specialKList[i];
            double specialKSignal = specialKSignalList[i];
            double prevSpecialK = i >= 1 ? specialKList[i - 1] : 0;
            double prevSpecialKSignal = i >= 1 ? specialKSignalList[i - 1] : 0;

            var signal = GetCompareSignal(specialK - specialKSignal, prevSpecialK - prevSpecialKSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "PringSpecialK", specialKList },
            { "Signal", specialKSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = specialKList;
        stockData.IndicatorName = IndicatorName.PringSpecialK;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Zone Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePriceZoneOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 20)
    {
        List<double> pzoList = new();
        List<double> dvolList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double dvol = Math.Sign(MinPastValues(i, 1, currentValue - prevValue)) * currentValue;
            dvolList.AddRounded(dvol);
        }

        var dvmaList = GetMovingAverageList(stockData, maType, length, dvolList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double vma = emaList[i];
            double dvma = dvmaList[i];
            double prevPzo1 = i >= 1 ? pzoList[i - 1] : 0;
            double prevPzo2 = i >= 2 ? pzoList[i - 2] : 0;

            double pzo = vma != 0 ? MinOrMax(100 * dvma / vma, 100, -100) : 0;
            pzoList.AddRounded(pzo);

            var signal = GetRsiSignal(pzo - prevPzo1, prevPzo1 - prevPzo2, pzo, prevPzo1, 40, -40);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pzo", pzoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pzoList;
        stockData.IndicatorName = IndicatorName.PriceZoneOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Performance Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePerformanceIndex(this StockData stockData, int length = 14)
    {
        List<double> kpiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;

            double prevKpi = kpiList.LastOrDefault();
            double kpi = prevValue != 0 ? MinPastValues(i, length, currentValue - prevValue) * 100 / prevValue : 0;
            kpiList.AddRounded(kpi);

            var signal = GetCompareSignal(kpi, prevKpi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Math.PI", kpiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kpiList;
        stockData.IndicatorName = IndicatorName.PerformanceIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Polarized Fractal Efficiency
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculatePolarizedFractalEfficiency(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 9, int smoothLength = 5)
    {
        List<double> c2cList = new();
        List<double> fracEffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double priorValue = i >= length ? inputList[i - length] : 0;
            double pfe = Sqrt(Pow(MinPastValues(i, length, currentValue - priorValue), 2) + 100);

            double c2c = Sqrt(Pow(MinPastValues(i, 1, currentValue - prevValue), 2) + 1);
            c2cList.AddRounded(c2c);

            double c2cSum = c2cList.TakeLastExt(length).Sum();
            double efRatio = c2cSum != 0 ? pfe / c2cSum * 100 : 0;

            double fracEff = i >= length && currentValue - priorValue > 0 ? efRatio : -efRatio;
            fracEffList.AddRounded(fracEff);
        }

        var emaList = GetMovingAverageList(stockData, maType, smoothLength, fracEffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ema = emaList[i];
            double prevEma = i >= 1 ? emaList[i - 1] : 0;

            var signal = GetCompareSignal(ema, prevEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pfe", emaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emaList;
        stockData.IndicatorName = IndicatorName.PolarizedFractalEfficiency;

        return stockData;
    }

    /// <summary>
    /// Calculates the Pretty Good Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePrettyGoodOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14)
    {
        List<double> pgoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double atr = atrList[i];

            double prevPgo = pgoList.LastOrDefault();
            double pgo = atr != 0 ? (currentValue - sma) / atr : 0;
            pgoList.AddRounded(pgo);

            var signal = GetCompareSignal(pgo, prevPgo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pgo", pgoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pgoList;
        stockData.IndicatorName = IndicatorName.PrettyGoodOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Cycle Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePriceCycleOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 22)
    {
        List<double> pcoList = new();
        List<double> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, lowList, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentLow = lowList[i];
            
            double diff = currentClose - currentLow;
            diffList.AddRounded(diff);
        }

        var diffSmaList = GetMovingAverageList(stockData, maType, length, diffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentAtr = atrList[i];
            double prevPco1 = i >= 1 ? pcoList[i - 1] : 0;
            double prevPco2 = i >= 2 ? pcoList[i - 2] : 0;
            double diffSma = diffSmaList[i];

            double pco = currentAtr != 0 ? diffSma / currentAtr * 100 : 0;
            pcoList.AddRounded(pco);

            var signal = GetCompareSignal(pco - prevPco1, prevPco1 - prevPco2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pco", pcoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pcoList;
        stockData.IndicatorName = IndicatorName.PriceCycleOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Phase Change Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculatePhaseChangeIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 35, int smoothLength = 3)
    {
        List<double> pciList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double mom = MinPastValues(i, length, currentValue - prevValue);

            double positiveSum = 0, negativeSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                double prevValue2 = i >= length - j ? inputList[i - (length - j)] : 0;
                double gradient = prevValue + (mom * (length - j) / (length - 1));
                double deviation = prevValue2 - gradient;
                positiveSum = deviation > 0 ? positiveSum + deviation : positiveSum + 0;
                negativeSum = deviation < 0 ? negativeSum - deviation : negativeSum + 0;
            }
            double sum = positiveSum + negativeSum;

            double pci = sum != 0 ? MinOrMax(100 * positiveSum / sum, 100, 0) : 0;
            pciList.AddRounded(pci);
        }

        var pciSmoothedList = GetMovingAverageList(stockData, maType, smoothLength, pciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double pciSmoothed = pciSmoothedList[i];
            double prevPciSmoothed1 = i >= 1 ? pciSmoothedList[i - 1] : 0;
            double prevPciSmoothed2 = i >= 2 ? pciSmoothedList[i - 2] : 0;

            var signal = GetRsiSignal(pciSmoothed - prevPciSmoothed1, prevPciSmoothed1 - prevPciSmoothed2, pciSmoothed, prevPciSmoothed1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pci", pciList },
            { "Signal", pciSmoothedList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pciList;
        stockData.IndicatorName = IndicatorName.PhaseChangeIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Peak Valley Estimation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculatePeakValleyEstimation(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 500, int smoothLength = 100)
    {
        List<double> sign1List = new();
        List<double> sign2List = new();
        List<double> sign3List = new();
        List<double> absOsList = new();
        List<double> osList = new();
        List<double> hList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];

            double os = currentValue - sma;
            osList.AddRounded(os);

            double absOs = Math.Abs(os);
            absOsList.AddRounded(absOs);
        }

        stockData.CustomValuesList = absOsList;
        var pList = CalculateLinearRegression(stockData, smoothLength).CustomValuesList;
        var (highestList, _) = GetMaxAndMinValuesList(pList, length);
        for (int i = 0; i < stockData.Count; i++)
        {
            double os = osList[i];
            double p = pList[i];
            double highest = highestList[i];

            double prevH = i >= 1 ? hList[i - 1] : 0;
            double h = highest != 0 ? p / highest : 0;
            hList.AddRounded(h);

            double mod1 = h == 1 && prevH != 1 ? 1 : 0;
            double mod2 = h < 0.8 ? 1 : 0;
            double mod3 = prevH == 1 && h < prevH ? 1 : 0;

            double sign1 = mod1 == 1 && os < 0 ? 1 : mod1 == 1 && os > 0 ? -1 : 0;
            sign1List.AddRounded(sign1);

            double sign2 = mod2 == 1 && os < 0 ? 1 : mod2 == 1 && os > 0 ? -1 : 0;
            sign2List.AddRounded(sign2);

            double sign3 = mod3 == 1 && os < 0 ? 1 : mod3 == 1 && os > 0 ? -1 : 0;
            sign3List.AddRounded(sign3);

            var signal = GetConditionSignal(sign1 > 0, sign1 < 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sign1", sign1List },
            { "Sign2", sign2List },
            { "Sign3", sign3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sign1List;
        stockData.IndicatorName = IndicatorName.PeakValleyEstimation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Psychological Line
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePsychologicalLine(this StockData stockData, int length = 20)
    {
        List<double> condList = new();
        List<double> psyList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevPsy1 = i >= 1 ? psyList[i - 1] : 0;
            double prevPsy2 = i >= 2 ? psyList[i - 2] : 0;

            double cond = currentValue > prevValue ? 1 : 0;
            condList.AddRounded(cond);

            double condSum = condList.TakeLastExt(length).Sum();
            double psy = length != 0 ? condSum / length * 100 : 0;
            psyList.AddRounded(psy);

            var signal = GetCompareSignal(psy - prevPsy1, prevPsy1 - prevPsy2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pl", psyList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = psyList;
        stockData.IndicatorName = IndicatorName.PsychologicalLine;

        return stockData;
    }

    /// <summary>
    /// Calculates the Turbo Trigger
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateTurboTrigger(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 100,
        int smoothLength = 2)
    {
        List<double> avgList = new();
        List<double> hyList = new();
        List<double> ylList = new();
        List<double> abList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        var cList = GetMovingAverageList(stockData, maType, smoothLength, inputList);
        var oList = GetMovingAverageList(stockData, maType, smoothLength, openList);
        var hList = GetMovingAverageList(stockData, maType, smoothLength, highList);
        var lList = GetMovingAverageList(stockData, maType, smoothLength, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double c = cList[i];
            double o = oList[i];

            double avg = (c + o) / 2;
            avgList.AddRounded(avg);
        }

        var yList = GetMovingAverageList(stockData, maType, length, avgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double y = yList[i];
            double h = hList[i];
            double l = lList[i];

            double hy = h - y;
            hyList.AddRounded(hy);

            double yl = y - l;
            ylList.AddRounded(yl);
        }

        var aList = GetMovingAverageList(stockData, maType, length, hyList);
        var bList = GetMovingAverageList(stockData, maType, length, ylList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double a = aList[i];
            double b = bList[i];

            double ab = a - b;
            abList.AddRounded(ab);
        }

        var oscList = GetMovingAverageList(stockData, maType, length, abList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double osc = oscList[i];
            double prevOsc = i >= 1 ? oscList[i - 1] : 0;
            double a = aList[i];
            double prevA = i >= 1 ? aList[i - 1] : 0;

            var signal = GetCompareSignal(osc - a, prevOsc - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BullLine", aList },
            { "Trigger", oscList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.TurboTrigger;

        return stockData;
    }

    /// <summary>
    /// Calculates the Total Power Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTotalPowerIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 45, int length2 = 10)
    {
        List<double> bullCountList = new();
        List<double> bearCountList = new();
        List<double> totalPowerList = new();
        List<double> adjBullCountList = new();
        List<double> adjBearCountList = new();
        List<Signal> signalsList = new();

        var elderPowerList = CalculateElderRayIndex(stockData, maType, length2);
        var bullPowerList = elderPowerList.OutputValues["BullPower"];
        var bearPowerList = elderPowerList.OutputValues["BearPower"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double bullPower = bullPowerList[i];
            double bearPower = bearPowerList[i];

            double bullCount = bullPower > 0 ? 1 : 0;
            bullCountList.AddRounded(bullCount);

            double bearCount = bearPower < 0 ? 1 : 0;
            bearCountList.AddRounded(bearCount);

            double bullCountSum = bullCountList.TakeLastExt(length1).Sum();
            double bearCountSum = bearCountList.TakeLastExt(length1).Sum();

            double totalPower = length1 != 0 ? 100 * Math.Abs(bullCountSum - bearCountSum) / length1 : 0;
            totalPowerList.AddRounded(totalPower);

            double prevAdjBullCount = adjBullCountList.LastOrDefault();
            double adjBullCount = length1 != 0 ? 100 * bullCountSum / length1 : 0;
            adjBullCountList.AddRounded(adjBullCount);

            double prevAdjBearCount = adjBearCountList.LastOrDefault();
            double adjBearCount = length1 != 0 ? 100 * bearCountSum / length1 : 0;
            adjBearCountList.AddRounded(adjBearCount);

            var signal = GetCompareSignal(adjBullCount - adjBearCount, prevAdjBullCount - prevAdjBearCount);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "TotalPower", totalPowerList },
            { "BullCount", adjBullCountList },
            { "BearCount", adjBearCountList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = totalPowerList;
        stockData.IndicatorName = IndicatorName.TotalPowerIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Turbo Scaler
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static StockData CalculateTurboScaler(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 50, 
        double alpha = 0.5m)
    {
        List<double> aList = new();
        List<double> bList = new();
        List<double> smoList = new();
        List<double> smoSmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var sma2List = GetMovingAverageList(stockData, maType, length, smaList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double sma2 = sma2List[i];

            double smoSma = (alpha * sma) + ((1 - alpha) * sma2);
            smoSmaList.AddRounded(smoSma);

            double smo = (alpha * currentValue) + ((1 - alpha) * sma);
            smoList.AddRounded(smo);

            double smoSmaHighest = smoSmaList.TakeLastExt(length).Max();
            double smoSmaLowest = smoSmaList.TakeLastExt(length).Min();
            double smoHighest = smoList.TakeLastExt(length).Max();
            double smoLowest = smoList.TakeLastExt(length).Min();

            double a = smoHighest - smoLowest != 0 ? (currentValue - smoLowest) / (smoHighest - smoLowest) : 0;
            aList.AddRounded(a);

            double b = smoSmaHighest - smoSmaLowest != 0 ? (sma - smoSmaLowest) / (smoSmaHighest - smoSmaLowest) : 0;
            bList.AddRounded(b);
        }

        var aSmaList = GetMovingAverageList(stockData, maType, length, aList);
        var bSmaList = GetMovingAverageList(stockData, maType, length, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double a = aSmaList[i];
            double b = bSmaList[i];
            double prevA = i >= 1 ? aSmaList[i - 1] : 0;
            double prevB = i >= 1 ? bSmaList[i - 1] : 0;

            var signal = GetCompareSignal(a - b, prevA - prevB);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ts", aList },
            { "Trigger", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.TurboScaler;

        return stockData;
    }

    /// <summary>
    /// Calculates the Technical Rank
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="length6"></param>
    /// <param name="length7"></param>
    /// <param name="length8"></param>
    /// <param name="length9"></param>
    /// <returns></returns>
    public static StockData CalculateTechnicalRank(this StockData stockData, int length1 = 200, int length2 = 125, int length3 = 50, int length4 = 20,
        int length5 = 12, int length6 = 26, int length7 = 9, int length8 = 3, int length9 = 14)
    {
        List<double> trList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ma1List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, inputList);
        var ma2List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length3, inputList);
        var ltRocList = CalculateRateOfChange(stockData, length2).CustomValuesList;
        var mtRocList = CalculateRateOfChange(stockData, length4).CustomValuesList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, length: length9).CustomValuesList;
        var ppoHistList = CalculatePercentagePriceOscillator(stockData, MovingAvgType.ExponentialMovingAverage, length5, length6, length7).
            OutputValues["Histogram"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentEma200 = ma1List[i];
            double currentEma50 = ma2List[i];
            double currentRoc125 = ltRocList[i];
            double currentRoc20 = mtRocList[i];
            double currentPpoHistogram = ppoHistList[i];
            double currentRsi = rsiList[i];
            double currentPrice = inputList[i];
            double prevTr1 = i >= 1 ? trList[i - 1] : 0;
            double prevTr2 = i >= 2 ? trList[i - 2] : 0;
            double ltMa = currentEma200 != 0 ? 0.3 * 100 * (currentPrice - currentEma200) / currentEma200 : 0;
            double ltRoc = 0.3 * 100 * currentRoc125;
            double mtMa = currentEma50 != 0 ? 0.15 * 100 * (currentPrice - currentEma50) / currentEma50 : 0;
            double mtRoc = 0.15 * 100 * currentRoc20;
            double currentValue = currentPpoHistogram;
            double prevValue = i >= length8 ? ppoHistList[i - length8] : 0;
            double slope = length8 != 0 ? MinPastValues(i, length8, currentValue - prevValue) / length8 : 0;
            double stPpo = 0.05 * 100 * slope;
            double stRsi = 0.05 * currentRsi;

            double tr = Math.Min(100, Math.Max(0, ltMa + ltRoc + mtMa + mtRoc + stPpo + stRsi));
            trList.AddRounded(tr);

            var signal = GetCompareSignal(tr - prevTr1, prevTr1 - prevTr2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tr", trList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trList;
        stockData.IndicatorName = IndicatorName.TechnicalRank;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trigonometric Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrigonometricOscillator(this StockData stockData, int length = 200)
    {
        List<double> uList = new();
        List<double> oList = new();
        List<Signal> signalsList = new();

        var sList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double s = sList[i];
            double prevS = i >= 1 ? sList[i - 1] : 0;
            double wa = Math.Asin(Math.Sign(s - prevS)) * 2;
            double wb = Math.Asin(Math.Sign(1)) * 2;

            double u = wa + (2 * Math.PI * Math.Round((wa - wb) / (2 * Math.PI)));
            uList.AddRounded(u);
        }

        stockData.CustomValuesList = uList;
        var uLinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double u = uLinregList[i];
            double prevO1 = i >= 1 ? oList[i - 1] : 0;
            double prevO2 = i >= 2 ? oList[i - 2] : 0;

            double o = Math.Atan(u);
            oList.AddRounded(o);

            var signal = GetRsiSignal(o - prevO1, prevO1 - prevO2, o, prevO1, 1, -1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "To", oList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oList;
        stockData.IndicatorName = IndicatorName.TrigonometricOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trading Made More Simpler Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="smoothLength"></param>
    /// <param name="threshold"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public static StockData CalculateTradingMadeMoreSimplerOscillator(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length1 = 14, int length2 = 8, int length3 = 12, int smoothLength = 3,
        double threshold = 50, double limit = 0)
    {
        List<double> bufHistNoList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length1).CustomValuesList;
        var stochastic1List = CalculateStochasticOscillator(stockData, maType, length: length2, smoothLength, smoothLength).OutputValues["FastD"];
        var stochastic2List = CalculateStochasticOscillator(stockData, maType, length: length1, smoothLength, smoothLength).OutputValues["FastD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double stoch1 = stochastic1List[i];
            double stoch2 = stochastic2List[i];
            double bufRsi = rsi - threshold;
            double bufStoch1 = stoch1 - threshold;
            double bufStoch2 = stoch2 - threshold;
            double bufHistUp = bufRsi > limit && bufStoch1 > limit && bufStoch2 > limit ? bufStoch2 : 0;
            double bufHistDn = bufRsi < limit && bufStoch1 < limit && bufStoch2 < limit ? bufStoch2 : 0;

            double prevBufHistNo = bufHistNoList.LastOrDefault();
            double bufHistNo = bufHistUp - bufHistDn;
            bufHistNoList.AddRounded(bufHistNo);

            var signal = GetCompareSignal(bufHistNo, prevBufHistNo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tmmso", bufHistNoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bufHistNoList;
        stockData.IndicatorName = IndicatorName.TradingMadeMoreSimplerOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Traders Dynamic Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateTradersDynamicIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length1 = 13, int length2 = 34, int length3 = 2, int length4 = 7)
    {
        List<double> upList = new();
        List<double> dnList = new();
        List<double> midList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length1, length2);
        var rList = rsiList.CustomValuesList;
        var maList = rsiList.OutputValues["Signal"];
        stockData.CustomValuesList = rList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        var mabList = GetMovingAverageList(stockData, maType, length3, rList);
        var mbbList = GetMovingAverageList(stockData, maType, length4, rList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsiSma = maList[i];
            double stdDev = stdDevList[i];
            double mab = mabList[i];
            double mbb = mbbList[i];
            double prevMab = i >= 1 ? mabList[i - 1] : 0;
            double prevMbb = i >= 1 ? mbbList[i - 1] : 0;
            double offs = 1.6185m * stdDev;

            double prevUp = upList.LastOrDefault();
            double up = rsiSma + offs;
            upList.AddRounded(up);

            double prevDn = dnList.LastOrDefault();
            double dn = rsiSma - offs;
            dnList.AddRounded(dn);

            double mid = (up + dn) / 2;
            midList.AddRounded(mid);

            var signal = GetBollingerBandsSignal(mab - mbb, prevMab - prevMbb, mab, prevMab, up, prevUp, dn, prevDn);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upList },
            { "MiddleBand", midList },
            { "LowerBand", dnList },
            { "Tdi", mabList },
            { "Signal", mbbList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mabList;
        stockData.IndicatorName = IndicatorName.TradersDynamicIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Tops and Bottoms Finder
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTopsAndBottomsFinder(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 50)
    {
        List<double> bList = new();
        List<double> cList = new();
        List<double> upList = new();
        List<double> dnList = new();
        List<double> osList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double a = emaList[i];
            double prevA = i >= 1 ? emaList[i - 1] : 0;

            double b = a > prevA ? a : 0;
            bList.AddRounded(b);

            double c = a < prevA ? a : 0;
            cList.AddRounded(c);
        }

        stockData.CustomValuesList = bList;
        var bStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = cList;
        var cStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double a = emaList[i];
            double b = bStdDevList[i];
            double c = cStdDevList[i];

            double prevUp = upList.LastOrDefault();
            double up = a + b != 0 ? a / (a + b) : 0;
            upList.AddRounded(up);

            double prevDn = dnList.LastOrDefault();
            double dn = a + c != 0 ? a / (a + c) : 0;
            dnList.AddRounded(dn);

            double os = prevUp == 1 && up != 1 ? 1 : prevDn == 1 && dn != 1 ? -1 : 0;
            osList.AddRounded(os);

            var signal = GetConditionSignal(os > 0, os < 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tabf", osList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = osList;
        stockData.IndicatorName = IndicatorName.TopsAndBottomsFinder;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trader Pressure Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateTraderPressureIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
        int length1 = 7, int length2 = 2, int smoothLength = 3)
    {
        List<double> bullsList = new();
        List<double> bearsList = new();
        List<double> netList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double high = highList[i];
            double low = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double hiup = Math.Max(high - prevHigh, 0);
            double loup = Math.Max(low - prevLow, 0);
            double hidn = Math.Min(high - prevHigh, 0);
            double lodn = Math.Min(low - prevLow, 0);
            double highest = highestList[i];
            double lowest = lowestList[i];
            double range = highest - lowest;

            double bulls = range != 0 ? Math.Min((hiup + loup) / range, 1) * 100 : 0;
            bullsList.AddRounded(bulls);

            double bears = range != 0 ? Math.Max((hidn + lodn) / range, -1) * -100 : 0;
            bearsList.AddRounded(bears);
        }

        var avgBullsList = GetMovingAverageList(stockData, maType, length1, bullsList);
        var avgBearsList = GetMovingAverageList(stockData, maType, length1, bearsList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double avgBulls = avgBullsList[i];
            double avgBears = avgBearsList[i];

            double net = avgBulls - avgBears;
            netList.AddRounded(net);
        }

        var tpxList = GetMovingAverageList(stockData, maType, smoothLength, netList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tpx = tpxList[i];
            double prevTpx = i >= 1 ? tpxList[i - 1] : 0;

            var signal = GetCompareSignal(tpx, prevTpx);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tpx", tpxList },
            { "Bulls", avgBullsList },
            { "Bears", avgBearsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tpxList;
        stockData.IndicatorName = IndicatorName.TraderPressureIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Technical Ratings
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="aoLength1"></param>
    /// <param name="aoLength2"></param>
    /// <param name="rsiLength"></param>
    /// <param name="stochLength1"></param>
    /// <param name="stochLength2"></param>
    /// <param name="stochLength3"></param>
    /// <param name="ultOscLength1"></param>
    /// <param name="ultOscLength2"></param>
    /// <param name="ultOscLength3"></param>
    /// <param name="ichiLength1"></param>
    /// <param name="ichiLength2"></param>
    /// <param name="ichiLength3"></param>
    /// <param name="vwmaLength"></param>
    /// <param name="cciLength"></param>
    /// <param name="adxLength"></param>
    /// <param name="momLength"></param>
    /// <param name="macdLength1"></param>
    /// <param name="macdLength2"></param>
    /// <param name="macdLength3"></param>
    /// <param name="bullBearLength"></param>
    /// <param name="williamRLength"></param>
    /// <param name="maLength1"></param>
    /// <param name="maLength2"></param>
    /// <param name="maLength3"></param>
    /// <param name="maLength4"></param>
    /// <param name="maLength5"></param>
    /// <param name="maLength6"></param>
    /// <param name="hullMaLength"></param>
    /// <returns></returns>
    public static StockData CalculateTechnicalRatings(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int aoLength1 = 55, int aoLength2 = 34, int rsiLength = 14, int stochLength1 = 14, int stochLength2 = 3, int stochLength3 = 3, 
        int ultOscLength1 = 7, int ultOscLength2 = 14, int ultOscLength3 = 28, int ichiLength1 = 9, int ichiLength2 = 26, int ichiLength3 = 52, 
        int vwmaLength = 20, int cciLength = 20, int adxLength = 14, int momLength = 10, int macdLength1 = 12, int macdLength2 = 26, 
        int macdLength3 = 9, int bullBearLength = 13, int williamRLength = 14, int maLength1 = 10, int maLength2 = 20, int maLength3 = 30, 
        int maLength4 = 50, int maLength5 = 100, int maLength6 = 200, int hullMaLength = 9)
    {
        List<double> maRatingList = new();
        List<double> oscRatingList = new();
        List<double> totalRatingList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, length: rsiLength).CustomValuesList;
        var aoList = CalculateAwesomeOscillator(stockData, fastLength: aoLength1, slowLength: aoLength2).CustomValuesList;
        var macdItemsList = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: macdLength1, slowLength: macdLength2, 
            signalLength: macdLength3);
        var macdList = macdItemsList.CustomValuesList;
        var macdSignalList = macdItemsList.OutputValues["Signal"];
        var uoList = CalculateUltimateOscillator(stockData, ultOscLength1, ultOscLength2, ultOscLength3).CustomValuesList;
        var ichiMokuList = CalculateIchimokuCloud(stockData, tenkanLength: ichiLength1, kijunLength: ichiLength2, senkouLength: ichiLength3);
        var tenkanList = ichiMokuList.OutputValues["TenkanSen"];
        var kijunList = ichiMokuList.OutputValues["KijunSen"];
        var senkouAList = ichiMokuList.OutputValues["SenkouSpanA"];
        var senkouBList = ichiMokuList.OutputValues["SenkouSpanB"];
        var adxItemsList = CalculateAverageDirectionalIndex(stockData, length: adxLength);
        var adxList = adxItemsList.CustomValuesList;
        var adxPlusList = adxItemsList.OutputValues["DiPlus"];
        var adxMinusList = adxItemsList.OutputValues["DiMinus"];
        var cciList = CalculateCommodityChannelIndex(stockData, length: cciLength).CustomValuesList;
        var bullBearPowerList = CalculateElderRayIndex(stockData, length: bullBearLength);
        var bullPowerList = bullBearPowerList.OutputValues["BullPower"];
        var bearPowerList = bullBearPowerList.OutputValues["BearPower"];
        var hullMaList = CalculateHullMovingAverage(stockData, length: hullMaLength).CustomValuesList;
        var williamsPctList = CalculateWilliamsR(stockData, length: williamRLength).CustomValuesList;
        var vwmaList = CalculateVolumeWeightedMovingAverage(stockData, length: vwmaLength).CustomValuesList;
        var stoList = CalculateStochasticOscillator(stockData, length: stochLength1, smoothLength1: stochLength2, smoothLength2: stochLength3);
        var stoKList = stoList.CustomValuesList;
        var stoDList = stoList.OutputValues["FastD"];
        var ma10List = GetMovingAverageList(stockData, maType, maLength1, inputList);
        var ma20List = GetMovingAverageList(stockData, maType, maLength2, inputList);
        var ma30List = GetMovingAverageList(stockData, maType, maLength3, inputList);
        var ma50List = GetMovingAverageList(stockData, maType, maLength4, inputList);
        var ma100List = GetMovingAverageList(stockData, maType, maLength5, inputList);
        var ma200List = GetMovingAverageList(stockData, maType, maLength6, inputList);
        var momentumList = CalculateMomentumOscillator(stockData, length: momLength).CustomValuesList;
        stockData.CustomValuesList = rsiList;
        var stoRsiList = CalculateStochasticOscillator(stockData, length: stochLength1, smoothLength1: stochLength2, smoothLength2: stochLength3);
        var stoRsiKList = stoRsiList.CustomValuesList;
        var stoRsiDList = stoRsiList.OutputValues["FastD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double rsi = rsiList[i];
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;
            double ma10 = ma10List[i];
            double ma20 = ma20List[i];
            double ma30 = ma30List[i];
            double ma50 = ma50List[i];
            double ma100 = ma100List[i];
            double ma200 = ma200List[i];
            double hullMa = hullMaList[i];
            double vwma = vwmaList[i];
            double conLine = tenkanList[i];
            double baseLine = kijunList[i];
            double leadLine1 = senkouAList[i];
            double leadLine2 = senkouBList[i];
            double kSto = stoKList[i];
            double prevKSto = i >= 1 ? stoKList[i - 1] : 0;
            double dSto = stoDList[i];
            double prevDSto = i >= 1 ? stoDList[i - 1] : 0;
            double cci = cciList[i];
            double prevCci = i >= 1 ? cciList[i - 1] : 0;
            double adx = adxList[i];
            double adxPlus = adxPlusList[i];
            double prevAdxPlus = i >= 1 ? adxPlusList[i - 1] : 0;
            double adxMinus = adxMinusList[i];
            double prevAdxMinus = i >= 1 ? adxMinusList[i - 1] : 0;
            double ao = aoList[i];
            double prevAo1 = i >= 1 ? aoList[i - 1] : 0;
            double prevAo2 = i >= 2 ? aoList[i - 2] : 0;
            double mom = momentumList[i];
            double prevMom = i >= 1 ? momentumList[i - 1] : 0;
            double macd = macdList[i];
            double macdSig = macdSignalList[i];
            double kStoRsi = stoRsiKList[i];
            double prevKStoRsi = i >= 1 ? stoRsiKList[i - 1] : 0;
            double dStoRsi = stoRsiDList[i];
            double prevDStoRsi = i >= 1 ? stoRsiDList[i - 1] : 0;
            bool upTrend = currentValue > ma50;
            bool dnTrend = currentValue < ma50;
            double wr = williamsPctList[i];
            double prevWr = i >= 1 ? williamsPctList[i - 1] : 0;
            double bullPower = bullPowerList[i];
            double prevBullPower = i >= 1 ? bullPowerList[i - 1] : 0;
            double bearPower = bearPowerList[i];
            double prevBearPower = i >= 1 ? bearPowerList[i - 1] : 0;
            double uo = uoList[i];

            double maRating = 0;
            maRating += currentValue > ma10 ? 1 : currentValue < ma10 ? -1 : 0;
            maRating += currentValue > ma20 ? 1 : currentValue < ma20 ? -1 : 0;
            maRating += currentValue > ma30 ? 1 : currentValue < ma30 ? -1 : 0;
            maRating += currentValue > ma50 ? 1 : currentValue < ma50 ? -1 : 0;
            maRating += currentValue > ma100 ? 1 : currentValue < ma100 ? -1 : 0;
            maRating += currentValue > ma200 ? 1 : currentValue < ma200 ? -1 : 0;
            maRating += currentValue > hullMa ? 1 : currentValue < hullMa ? -1 : 0;
            maRating += currentValue > vwma ? 1 : currentValue < vwma ? -1 : 0;
            maRating += leadLine1 > leadLine2 && currentValue > leadLine1 && currentValue < baseLine && prevValue < conLine && 
                currentValue > conLine ? 1 : leadLine2 > leadLine1 &&
                currentValue < leadLine2 && currentValue > baseLine && prevValue > conLine && currentValue < conLine ? -1 : 0;
            maRating /= 9;
            maRatingList.AddRounded(maRating);

            double oscRating = 0;
            oscRating += rsi < 30 && prevRsi < rsi ? 1 : rsi > 70 && prevRsi > rsi ? -1 : 0;
            oscRating += kSto < 20 && dSto < 20 && kSto > dSto && prevKSto < prevDSto ? 1 : kSto > 80 && dSto > 80 && kSto < dSto && 
                prevKSto > prevDSto ? -1 : 0;
            oscRating += cci < -100 && cci > prevCci ? 1 : cci > 100 && cci < prevCci ? -1 : 0;
            oscRating += adx > 20 && prevAdxPlus < prevAdxMinus && adxPlus > adxMinus ? 1 : adx > 20 && prevAdxPlus > prevAdxMinus && 
                adxPlus < adxMinus ? -1 : 0;
            oscRating += (ao > 0 && prevAo1 < 0) || (ao > 0 && prevAo1 > 0 && ao > prevAo1 && prevAo2 > prevAo1) ? 1 : 
                (ao < 0 && prevAo1 > 0) || (ao < 0 && prevAo1 < 0 && ao < prevAo1 && prevAo2 < prevAo1) ? -1 : 0;
            oscRating += mom > prevMom ? 1 : mom < prevMom ? -1 : 0;
            oscRating += macd > macdSig ? 1 : macd < macdSig ? -1 : 0;
            oscRating += dnTrend && kStoRsi < 20 && dStoRsi < 20 && kStoRsi > dStoRsi && prevKStoRsi < prevDStoRsi ? 1 : upTrend && 
                kStoRsi > 80 && dStoRsi > 80 && kStoRsi < dStoRsi && prevKStoRsi > prevDStoRsi ? -1 : 0;
            oscRating += wr < -80 && wr > prevWr ? 1 : wr > -20 && wr < prevWr ? -1 : 0;
            oscRating += upTrend && bearPower < 0 && bearPower > prevBearPower ? 1 : dnTrend && bullPower > 0 && bullPower < prevBullPower ? -1 : 0;
            oscRating += uo > 70 ? 1 : uo < 30 ? -1 : 0;
            oscRating /= 11;
            oscRatingList.AddRounded(oscRating);

            double totalRating = (maRating + oscRating) / 2;
            totalRatingList.AddRounded(totalRating);

            var signal = GetConditionSignal(totalRating > 0.1, totalRating < -0.1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tr", totalRatingList },
            { "Or", oscRatingList },
            { "Mr", maRatingList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = totalRatingList;
        stockData.IndicatorName = IndicatorName.TechnicalRatings;

        return stockData;
    }

    /// <summary>
    /// Calculates the TTM Scalper Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateTTMScalperIndicator(this StockData stockData)
    {
        List<double> buySellSwitchList = new();
        List<double> sbsList = new();
        List<double> clrsList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double prevClose1 = i >= 1 ? inputList[i - 1] : 0;
            double prevClose2 = i >= 2 ? inputList[i - 2] : 0;
            double prevClose3 = i >= 3 ? inputList[i - 3] : 0;
            double high = highList[i];
            double low = lowList[i];
            double triggerSell = prevClose1 < close && (prevClose2 < prevClose1 || prevClose3 < prevClose1) ? 1 : 0;
            double triggerBuy = prevClose1 > close && (prevClose2 > prevClose1 || prevClose3 > prevClose1) ? 1 : 0;

            double prevBuySellSwitch = buySellSwitchList.LastOrDefault();
            double buySellSwitch = triggerSell == 1 ? 1 : triggerBuy == 1 ? 0 : prevBuySellSwitch;
            buySellSwitchList.AddRounded(buySellSwitch);

            double prevSbs = sbsList.LastOrDefault();
            double sbs = triggerSell == 1 && prevBuySellSwitch == 0 ? high : triggerBuy == 1 && prevBuySellSwitch == 1 ? low : prevSbs;
            sbsList.AddRounded(sbs);

            double prevClrs = clrsList.LastOrDefault();
            double clrs = triggerSell == 1 && prevBuySellSwitch == 0 ? 1 : triggerBuy == 1 && prevBuySellSwitch == 1 ? -1 : prevClrs;
            clrsList.AddRounded(clrs);

            var signal = GetCompareSignal(clrs, prevClrs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sbs", sbsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sbsList;
        stockData.IndicatorName = IndicatorName.TTMScalperIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the TFS Tether Line Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTFSTetherLineIndicator(this StockData stockData, int length = 50)
    {
        List<double> tetherLineList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevTetherLine = tetherLineList.LastOrDefault();
            double tetherLine = (highest + lowest) / 2;
            tetherLineList.AddRounded(tetherLine);

            var signal = GetCompareSignal(currentValue - tetherLine, prevValue - prevTetherLine);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tether", tetherLineList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tetherLineList;
        stockData.IndicatorName = IndicatorName.TFSTetherLineIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates The Range Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateTheRangeIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage
        , int length = 10, int smoothLength = 3)
    {
        List<double> v1List = new();
        List<double> stochList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

            double v1 = i >= 1 && currentValue > prevValue ? tr / MinPastValues(i, 1, currentValue - prevValue) : tr;
            v1List.AddRounded(v1);

            var lbList = v1List.TakeLastExt(length).ToList();
            double v2 = lbList.Min();
            double v3 = lbList.Max();

            double stoch = v3 - v2 != 0 ? MinOrMax(100 * (v1 - v2) / (v3 - v2), 100, 0) : MinOrMax(100 * (v1 - v2), 100, 0);
            stochList.AddRounded(stoch);
        }

        var triList = GetMovingAverageList(stockData, maType, length, stochList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tri = triList[i];
            double prevTri1 = i >= 1 ? triList[i - 1] : 0;
            double prevTri2 = i >= 2 ? triList[i - 2] : 0;

            var signal = GetRsiSignal(tri - prevTri1, prevTri1 - prevTri2, tri, prevTri1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tri", triList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = triList;
        stockData.IndicatorName = IndicatorName.TheRangeIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Time Price Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTimePriceIndicator(this StockData stockData, int length = 50)
    {
        List<double> upperList = new();
        List<double> lowerList = new();
        List<double> risingList = new();
        List<double> fallingList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double highest = i >= 1 ? highestList[i - 1] : 0;
            double lowest = i >= 1 ? lowestList[i - 1] : 0;

            double rising = currentHigh > highest ? 1 : 0;
            risingList.AddRounded(rising);

            double falling = currentLow < lowest ? 1 : 0;
            fallingList.AddRounded(falling);

            double a = i - risingList.LastIndexOf(1);
            double b = i - fallingList.LastIndexOf(1);

            double prevUpper = upperList.LastOrDefault();
            double upper = length != 0 ? ((a > length ? length : a) / length) - 0.5 : 0;
            upperList.AddRounded(upper);

            double prevLower = lowerList.LastOrDefault();
            double lower = length != 0 ? ((b > length ? length : b) / length) - 0.5 : 0;
            lowerList.AddRounded(lower);

            var signal = GetCompareSignal((lower * -1) - (upper * -1), (prevLower * -1) - (prevUpper * -1));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.TimePriceIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Rahul Mohindar Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateRahulMohindarOscillator(this StockData stockData, int length1 = 2, int length2 = 10, int length3 = 30, 
        int length4 = 81)
    {
        List<double> swingTrd1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length2);

        var r1List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, inputList);
        var r2List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r1List); //-V3056
        var r3List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r2List);
        var r4List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r3List);
        var r5List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r4List);
        var r6List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r5List);
        var r7List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r6List);
        var r8List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r7List);
        var r9List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r8List);
        var r10List = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length1, r9List);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double r1 = r1List[i];
            double r2 = r2List[i];
            double r3 = r3List[i];
            double r4 = r4List[i];
            double r5 = r5List[i];
            double r6 = r6List[i];
            double r7 = r7List[i];
            double r8 = r8List[i];
            double r9 = r9List[i];
            double r10 = r10List[i];

            double swingTrd1 = highest - lowest != 0 ? 100 * (currentValue - ((r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8 + r9 + r10) / 10)) / 
                (highest - lowest) : 0;
            swingTrd1List.AddRounded(swingTrd1);
        }

        var swingTrd2List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length3, swingTrd1List);
        var swingTrd3List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length3, swingTrd2List);
        var rmoList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length4, swingTrd1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rmo = rmoList[i];
            double prevRmo = i >= 1 ? rmoList[i - 1] : 0;

            var signal = GetCompareSignal(rmo, prevRmo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rmo", rmoList },
            { "SwingTrade1", swingTrd1List },
            { "SwingTrade2", swingTrd2List },
            { "SwingTrade3", swingTrd3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rmoList;
        stockData.IndicatorName = IndicatorName.RahulMohindarOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Rainbow Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateRainbowOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length1 = 2, int length2 = 10)
    {
        List<double> rainbowOscillatorList = new();
        List<double> upperBandList = new();
        List<double> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length2);

        var r1List = GetMovingAverageList(stockData, maType, length1, inputList);
        var r2List = GetMovingAverageList(stockData, maType, length1, r1List); //-V3056
        var r3List = GetMovingAverageList(stockData, maType, length1, r2List);
        var r4List = GetMovingAverageList(stockData, maType, length1, r3List);
        var r5List = GetMovingAverageList(stockData, maType, length1, r4List);
        var r6List = GetMovingAverageList(stockData, maType, length1, r5List);
        var r7List = GetMovingAverageList(stockData, maType, length1, r6List);
        var r8List = GetMovingAverageList(stockData, maType, length1, r7List);
        var r9List = GetMovingAverageList(stockData, maType, length1, r8List);
        var r10List = GetMovingAverageList(stockData, maType, length1, r9List);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double currentValue = inputList[i];
            double r1 = r1List[i];
            double r2 = r2List[i];
            double r3 = r3List[i];
            double r4 = r4List[i];
            double r5 = r5List[i];
            double r6 = r6List[i];
            double r7 = r7List[i];
            double r8 = r8List[i];
            double r9 = r9List[i];
            double r10 = r10List[i];
            double highestRainbow = Math.Max(r1, Math.Max(r2, Math.Max(r3, Math.Max(r4, Math.Max(r5, Math.Max(r6, Math.Max(r7, Math.Max(r8, 
                Math.Max(r9, r10)))))))));
            double lowestRainbow = Math.Min(r1, Math.Min(r2, Math.Min(r3, Math.Min(r4, Math.Min(r5, Math.Min(r6, Math.Min(r7, Math.Min(r8, 
                Math.Min(r9, r10)))))))));

            double prevRainbowOscillator = rainbowOscillatorList.LastOrDefault();
            double rainbowOscillator = highest - lowest != 0 ? 100 * ((currentValue - ((r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8 + r9 + r10) / 10)) / 
                (highest - lowest)) : 0;
            rainbowOscillatorList.AddRounded(rainbowOscillator);

            double upperBand = highest - lowest != 0 ? 100 * ((highestRainbow - lowestRainbow) / (highest - lowest)) : 0;
            upperBandList.AddRounded(upperBand);

            double lowerBand = -upperBand;
            lowerBandList.AddRounded(lowerBand);

            var signal = GetCompareSignal(rainbowOscillator, prevRainbowOscillator);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ro", rainbowOscillatorList },
            { "UpperBand", upperBandList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rainbowOscillatorList;
        stockData.IndicatorName = IndicatorName.RainbowOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Random Walk Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRandomWalkIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14)
    {
        List<double> rwiLowList = new();
        List<double> rwiHighList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        double sqrt = Sqrt(length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentAtr = atrList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh = i >= length ? highList[i - length] : 0;
            double prevLow = i >= length ? lowList[i - length] : 0;
            double bottom = currentAtr * sqrt;

            double prevRwiLow = rwiLowList.LastOrDefault();
            double rwiLow = bottom != 0 ? (prevHigh - currentLow) / bottom : 0;
            rwiLowList.AddRounded(rwiLow);

            double prevRwiHigh = rwiHighList.LastOrDefault();
            double rwiHigh = bottom != 0 ? (currentHigh - prevLow) / bottom : 0;
            rwiHighList.AddRounded(rwiHigh);

            var signal = GetCompareSignal(rwiHigh - rwiLow, prevRwiHigh - prevRwiLow);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "RwiHigh", rwiHighList },
            { "RwiLow", rwiLowList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.RandomWalkIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Range Action Verification Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateRangeActionVerificationIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int fastLength = 7, int slowLength = 65)
    {
        List<double> raviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaFastList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var smaSlowList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastMA = smaFastList[i];
            double slowMA = smaSlowList[i];
            double prevRavi1 = i >= 1 ? raviList[i - 1] : 0;
            double prevRavi2 = i >= 2 ? raviList[i - 2] : 0;

            double ravi = slowMA != 0 ? (fastMA - slowMA) / slowMA * 100 : 0;
            raviList.AddRounded(ravi);

            var signal = GetCompareSignal(ravi - prevRavi1, prevRavi1 - prevRavi2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ravi", raviList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = raviList;
        stockData.IndicatorName = IndicatorName.RangeActionVerificationIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Really Simple Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateReallySimpleIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 21, int smoothLength = 10)
    {
        List<double> rsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, lowList, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentLow = lowList[i];
            double currentMa = maList[i];

            double rsi = currentValue != 0 ? (currentLow - currentMa) / currentValue * 100 : 0;
            rsiList.AddRounded(rsi);
        }

        var rsiMaList = GetMovingAverageList(stockData, maType, smoothLength, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiMaList[i];
            double prevRsiMa = i >= 1 ? rsiMaList[i - 1] : 0;
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;
            double rsiMa = rsiMaList[i];

            var signal = GetCompareSignal(rsi - rsiMa, prevRsi - prevRsiMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsi", rsiList },
            { "Signal", rsiMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.ReallySimpleIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Recursive Differenciator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static StockData CalculateRecursiveDifferenciator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 14, double alpha = 0.6m)
    {
        List<double> bList = new();
        List<double> bChgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = emaList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double priorB = i >= length ? bList[i - length] : 0;
            double a = rsi / 100;
            double prevBChg1 = i >= 1 ? bChgList[i - 1] : a;
            double prevBChg2 = i >= 2 ? bChgList[i - 2] : 0;

            double b = (alpha * a) + ((1 - alpha) * prevBChg1);
            bList.AddRounded(b);

            double bChg = b - priorB;
            bChgList.AddRounded(bChg);

            var signal = GetCompareSignal(bChg - prevBChg1, prevBChg1 - prevBChg2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rd", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.RecursiveDifferenciator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Regression Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRegressionOscillator(this StockData stockData, int length = 63)
    {
        List<double> roscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentLinReg = linRegList[i];

            double prevRosc = roscList.LastOrDefault();
            double rosc = currentLinReg != 0 ? 100 * ((currentValue / currentLinReg) - 1) : 0;
            roscList.AddRounded(rosc);

            var signal = GetCompareSignal(rosc, prevRosc);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rosc", roscList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = roscList;
        stockData.IndicatorName = IndicatorName.RegressionOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Difference Of Squares Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeDifferenceOfSquaresOscillator(this StockData stockData, int length = 20)
    {
        List<double> rdosList = new();
        List<double> aList = new();
        List<double> dList = new();
        List<double> nList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double a = currentValue > prevValue ? 1 : 0;
            aList.AddRounded(a);

            double d = currentValue < prevValue ? 1 : 0;
            dList.AddRounded(d);

            double n = currentValue == prevValue ? 1 : 0;
            nList.AddRounded(n);

            double prevRdos = rdosList.LastOrDefault();
            double aSum = aList.TakeLastExt(length).Sum();
            double dSum = dList.TakeLastExt(length).Sum();
            double nSum = nList.TakeLastExt(length).Sum();
            double rdos = aSum > 0 || dSum > 0 || nSum > 0 ? (Pow(aSum, 2) - Pow(dSum, 2)) / Pow(aSum + nSum + dSum, 2) : 0;
            rdosList.AddRounded(rdos);

            var signal = GetCompareSignal(rdos, prevRdos);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rdos", rdosList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rdosList;
        stockData.IndicatorName = IndicatorName.RelativeDifferenceOfSquaresOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Spread Strength
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeSpreadStrength(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 10, int slowLength = 40, int length = 14, int smoothLength = 5)
    {
        List<double> spreadList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double fastEma = fastEmaList[i];
            double slowEma = slowEmaList[i];

            double spread = fastEma - slowEma;
            spreadList.AddRounded(spread);
        }

        stockData.CustomValuesList = spreadList;
        var rsList = CalculateRelativeStrengthIndex(stockData, length: length).CustomValuesList;
        var rssList = GetMovingAverageList(stockData, maType, smoothLength, rsList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rss = rssList[i];
            double prevRss1 = i >= 1 ? rssList[i - 1] : 0;
            double prevRss2 = i >= 2 ? rssList[i - 2] : 0;

            var signal = GetRsiSignal(rss - prevRss1, prevRss1 - prevRss2, rss, prevRss1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rss", rssList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rssList;
        stockData.IndicatorName = IndicatorName.RelativeSpreadStrength;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Strength 3D Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="marketData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeStrength3DIndicator(this StockData stockData,
            StockData marketData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 4, int length2 = 7, int length3 = 10,
            int length4 = 15, int length5 = 30)
    {
        List<double> r1List = new();
        List<double> rs3List = new();
        List<double> rs2List = new();
        List<double> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (spInputList, _, _, _, _) = GetInputValuesList(marketData);

        if (stockData.Count == marketData.Count)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                double currentValue = inputList[i];
                double currentSp = spInputList[i];

                double prevR1 = r1List.LastOrDefault();
                double r1 = currentSp != 0 ? currentValue / currentSp * 100 : prevR1;
                r1List.AddRounded(r1);
            }

            var fastMaList = GetMovingAverageList(stockData, maType, length3, r1List);
            var medMaList = GetMovingAverageList(stockData, maType, length2, fastMaList);
            var slowMaList = GetMovingAverageList(stockData, maType, length4, fastMaList);
            var vSlowMaList = GetMovingAverageList(stockData, maType, length5, slowMaList);
            for (int i = 0; i < stockData.Count; i++)
            {
                double fastMa = fastMaList[i];
                double medMa = medMaList[i];
                double slowMa = slowMaList[i];
                double vSlowMa = vSlowMaList[i];
                double t1 = fastMa >= medMa && medMa >= slowMa && slowMa >= vSlowMa ? 10 : 0;
                double t2 = fastMa >= medMa && medMa >= slowMa && slowMa < vSlowMa ? 9 : 0;
                double t3 = fastMa < medMa && medMa >= slowMa && slowMa >= vSlowMa ? 9 : 0;
                double t4 = fastMa < medMa && medMa >= slowMa && slowMa < vSlowMa ? 5 : 0;

                double rs2 = t1 + t2 + t3 + t4;
                rs2List.AddRounded(rs2);
            }

            var rs2MaList = GetMovingAverageList(stockData, maType, length1, rs2List);
            for (int i = 0; i < stockData.Count; i++)
            {
                double rs2 = rs2List[i];
                double rs2Ma = rs2MaList[i];
                double prevRs3_1 = i >= 1 ? rs3List[i - 1] : 0;
                double prevRs3_2 = i >= 2 ? rs3List[i - 1] : 0;

                double x = rs2 >= 5 ? 1 : 0;
                xList.AddRounded(x);

                double rs3 = rs2 >= 5 || rs2 > rs2Ma ? xList.TakeLastExt(length4).Sum() / length4 * 100 : 0;
                rs3List.AddRounded(rs3);

                var signal = GetCompareSignal(rs3 - prevRs3_1, prevRs3_1 - prevRs3_2);
                signalsList.Add(signal);
            }
        }

        stockData.OutputValues = new()
        {
            { "Rs3d", rs3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rs3List;
        stockData.IndicatorName = IndicatorName.RelativeStrength3DIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Vigor Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeVigorIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14)
    {
        List<double> rviList = new();
        List<double> numeratorList = new();
        List<double> denominatorList = new();
        List<double> signalLineList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double currentOpen = openList[i];
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevOpen1 = i >= 1 ? openList[i - 1] : 0;
            double prevClose1 = i >= 1 ? inputList[i - 1] : 0;
            double prevHigh1 = i >= 1 ? highList[i - 1] : 0;
            double prevOpen2 = i >= 2 ? openList[i - 2] : 0;
            double prevClose2 = i >= 2 ? inputList[i - 2] : 0;
            double prevHigh2 = i >= 2 ? highList[i - 2] : 0;
            double prevOpen3 = i >= 3 ? openList[i - 3] : 0;
            double prevClose3 = i >= 3 ? inputList[i - 3] : 0;
            double prevHigh3 = i >= 3 ? highList[i - 3] : 0;
            double a = currentClose - currentOpen;
            double b = prevClose1 - prevOpen1;
            double c = prevClose2 - prevOpen2;
            double d = prevClose3 - prevOpen3;
            double e = currentHigh - currentLow;
            double f = prevHigh1 - prevOpen1;
            double g = prevHigh2 - prevOpen2;
            double h = prevHigh3 - prevOpen3;

            double numerator = (a + (2 * b) + (2 * c) + d) / 6;
            numeratorList.AddRounded(numerator);

            double denominator = (e + (2 * f) + (2 * g) + h) / 6;
            denominatorList.AddRounded(denominator);
        }

        var numeratorAvgList = GetMovingAverageList(stockData, maType, length, numeratorList);
        var denominatorAvgList = GetMovingAverageList(stockData, maType, length, denominatorList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double numeratorAvg = numeratorAvgList[i];
            double denominatorAvg = denominatorAvgList[i];
            double k = i >= 1 ? rviList[i - 1] : 0;
            double l = i >= 2 ? rviList[i - 2] : 0;
            double m = i >= 3 ? rviList[i - 3] : 0;

            double rvi = denominatorAvg != 0 ? numeratorAvg / denominatorAvg : 0;
            rviList.AddRounded(rvi);

            double prevSignalLine = signalLineList.LastOrDefault();
            double signalLine = (rvi + (2 * k) + (2 * l) + m) / 6;
            signalLineList.AddRounded(signalLine);

            var signal = GetCompareSignal(rvi - signalLine, k - prevSignalLine);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rvi", rviList },
            { "Signal", signalLineList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rviList;
        stockData.IndicatorName = IndicatorName.RelativeVigorIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Repulse
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRepulse(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 5)
    {
        List<double> bullPowerList = new();
        List<double> bearPowerList = new();
        List<double> repulseList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double lowestLow = lowestList[i];
            double highestHigh = highestList[i];
            double prevOpen = i >= 1 ? openList[i - 1] : 0;

            double bullPower = currentClose != 0 ? 100 * ((3 * currentClose) - (2 * lowestLow) - prevOpen) / currentClose : 0;
            bullPowerList.AddRounded(bullPower);

            double bearPower = currentClose != 0 ? 100 * (prevOpen + (2 * highestHigh) - (3 * currentClose)) / currentClose : 0;
            bearPowerList.AddRounded(bearPower);
        }

        var bullPowerEmaList = GetMovingAverageList(stockData, maType, length * 5, bullPowerList);
        var bearPowerEmaList = GetMovingAverageList(stockData, maType, length * 5, bearPowerList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double bullPowerEma = bullPowerEmaList[i];
            double bearPowerEma = bearPowerEmaList[i];

            double repulse = bullPowerEma - bearPowerEma;
            repulseList.AddRounded(repulse);
        }

        var repulseEmaList = GetMovingAverageList(stockData, maType, length, repulseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double repulse = repulseList[i];
            double prevRepulse = i >= 1 ? repulseList[i - 1] : 0;
            double repulseEma = repulseEmaList[i];
            double prevRepulseEma = i >= 1 ? repulseEmaList[i - 1] : 0;

            var signal = GetCompareSignal(repulse - repulseEma, prevRepulse - prevRepulseEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Repulse", repulseList },
            { "Signal", repulseEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = repulseList;
        stockData.IndicatorName = IndicatorName.Repulse;

        return stockData;
    }

    /// <summary>
    /// Calculates the Retrospective Candlestick Chart
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRetrospectiveCandlestickChart(this StockData stockData, int length = 100)
    {
        List<double> absChgList = new();
        List<double> kList = new();
        List<double> cList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double currentOpen = openList[i];
            double prevK1 = i >= 1 ? kList[i - 1] : 0;
            double prevK2 = i >= 2 ? kList[i - 2] : 0;

            double absChg = Math.Abs(currentClose - prevClose);
            absChgList.AddRounded(absChg);

            var lbList = absChgList.TakeLastExt(length).ToList();
            double highest = lbList.Max();
            double lowest = lbList.Min();
            double s = highest - lowest != 0 ? (absChg - lowest) / (highest - lowest) * 100 : 0;
            double weight = s / 100;

            double prevC = i >= 1 ? cList[i - 1] : currentClose;
            double c = (weight * currentClose) + ((1 - weight) * prevC);
            cList.AddRounded(c);

            double prevH = i >= 1 ? prevC : currentHigh;
            double h = (weight * currentHigh) + ((1 - weight) * prevH);
            double prevL = i >= 1 ? prevC : currentLow;
            double l = (weight * currentLow) + ((1 - weight) * prevL);
            double prevO = i >= 1 ? prevC : currentOpen;
            double o = (weight * currentOpen) + ((1 - weight) * prevO);

            double k = (c + h + l + o) / 4;
            kList.AddRounded(k);

            var signal = GetCompareSignal(k - prevK1, prevK1 - prevK2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rcc", kList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kList;
        stockData.IndicatorName = IndicatorName.RetrospectiveCandlestickChart;

        return stockData;
    }

    /// <summary>
    /// Calculates the Rex Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRexOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 14)
    {
        List<double> tvbList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double close = inputList[i];
            double open = openList[i];
            double high = highList[i];
            double low = lowList[i];

            double tvb = (3 * close) - (low + open + high);
            tvbList.AddRounded(tvb);
        }

        var roList = GetMovingAverageList(stockData, maType, length, tvbList);
        var roEmaList = GetMovingAverageList(stockData, maType, length, roList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ro = roList[i];
            double roEma = roEmaList[i];
            double prevRo = i >= 1 ? roList[i - 1] : 0;
            double prevRoEma = i >= 1 ? roEmaList[i - 1] : 0;

            var signal = GetCompareSignal(ro - roEma, prevRo - prevRoEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ro", roList },
            { "Signal", roEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = roList;
        stockData.IndicatorName = IndicatorName.RexOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Robust Weighting Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRobustWeightingOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 200)
    {
        List<double> indexList = new();
        List<double> tempList = new();
        List<double> corrList = new();
        List<double> lList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double index = i;
            indexList.AddRounded(index);

            double currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((double)corr);
        }

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double corr = corrList[i];
            double stdDev = stdDevList[i];
            double indexStdDev = indexStdDevList[i];
            double sma = smaList[i];
            double indexSma = indexSmaList[i];
            double a = indexStdDev != 0 ? corr * (stdDev / indexStdDev) : 0;
            double b = sma - (a * indexSma);

            double l = currentValue - a - (b * currentValue);
            lList.AddRounded(l);
        }

        var lSmaList = GetMovingAverageList(stockData, maType, length, lList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double l = lSmaList[i];
            double prevL1 = i >= 1 ? lSmaList[i - 1] : 0;
            double prevL2 = i >= 2 ? lSmaList[i - 2] : 0;

            var signal = GetCompareSignal(l - prevL1, prevL1 - prevL2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rwo", lSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lSmaList;
        stockData.IndicatorName = IndicatorName.RobustWeightingOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the RSING Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRSINGIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 20)
    {
        List<double> rsingList = new();
        List<double> upList = new();
        List<double> dnList = new();
        List<double> rangeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double high = highList[i];
            double low = lowList[i];

            double range = high - low;
            rangeList.AddRounded(range);
        }

        stockData.CustomValuesList = rangeList;
        var stdevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentVolume = volumeList[i];
            double ma = maList[i];
            double stdev = stdevList[i];
            double range = rangeList[i];
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double vwr = ma != 0 ? currentVolume / ma : 0;
            double blr = stdev != 0 ? range / stdev : 0;
            bool isUp = currentValue > prevValue;
            bool isDn = currentValue < prevValue;
            bool isEq = currentValue == prevValue;

            double prevUpCount = upList.LastOrDefault();
            double upCount = isEq ? 0 : isUp ? (prevUpCount <= 0 ? 1 : prevUpCount + 1) : (prevUpCount >= 0 ? -1 : prevUpCount - 1);
            upList.AddRounded(upCount);

            double prevDnCount = dnList.LastOrDefault();
            double dnCount = isEq ? 0 : isDn ? (prevDnCount <= 0 ? 1 : prevDnCount + 1) : (prevDnCount >= 0 ? -1 : prevDnCount - 1);
            dnList.AddRounded(dnCount);

            double pmo = MinPastValues(i, length, currentValue - prevValue);
            double rsing = vwr * blr * pmo;
            rsingList.AddRounded(rsing);
        }

        var rsingMaList = GetMovingAverageList(stockData, maType, length, rsingList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsing = rsingMaList[i];
            double prevRsing1 = i >= 1 ? rsingMaList[i - 1] : 0;
            double prevRsing2 = i >= 2 ? rsingMaList[i - 2] : 0;

            var signal = GetCompareSignal(rsing - prevRsing1, prevRsing1 - prevRsing2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsing", rsingList },
            { "Signal", rsingMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsingList;
        stockData.IndicatorName = IndicatorName.RSINGIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the RSMK Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="marketData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateRSMKIndicator(this StockData stockData, StockData marketData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 90, int smoothLength = 3)
    {
        List<double> rsmkList = new();
        List<double> logRatioList = new();
        List<double> logDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (spInputList, _, _, _, _) = GetInputValuesList(marketData);

        if (stockData.Count == marketData.Count)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                double currentValue = inputList[i];
                double spValue = spInputList[i];
                double prevLogRatio = i >= length ? logRatioList[i - length] : 0;

                double logRatio = spValue != 0 ? currentValue / spValue : 0;
                logRatioList.AddRounded(logRatio);

                double logDiff = logRatio - prevLogRatio;
                logDiffList.AddRounded(logDiff);
            }

            var logDiffEmaList = GetMovingAverageList(stockData, maType, smoothLength, logDiffList);
            for (int i = 0; i < stockData.Count; i++)
            {
                double logDiffEma = logDiffEmaList[i];

                double prevRsmk = rsmkList.LastOrDefault();
                double rsmk = logDiffEma * 100;
                rsmkList.AddRounded(rsmk);

                var signal = GetCompareSignal(rsmk, prevRsmk);
                signalsList.Add(signal);
            }
        }

        stockData.OutputValues = new()
        {
            { "Rsmk", rsmkList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsmkList;
        stockData.IndicatorName = IndicatorName.RSMKIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Running Equity
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRunningEquity(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 100)
    {
        List<double> chgXList = new();
        List<double> reqList = new();
        List<double> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double sma = smaList[i];

            double prevX = xList.LastOrDefault();
            double x = Math.Sign(currentValue - sma);
            xList.AddRounded(x);

            double chgX = MinPastValues(i, 1, currentValue - prevValue) * prevX;
            chgXList.AddRounded(chgX);

            double prevReq = reqList.LastOrDefault();
            double req = chgXList.TakeLastExt(length).Sum();
            reqList.AddRounded(req);

            var signal = GetCompareSignal(req, prevReq);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Req", reqList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = reqList;
        stockData.IndicatorName = IndicatorName.RunningEquity;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mass Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateMassIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 21, int length2 = 21, int length3 = 25, int signalLength = 9)
    {
        List<double> highLowList = new();
        List<double> ratioList = new();
        List<double> massIndexList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];

            double highLow = currentHigh - currentLow;
            highLowList.AddRounded(highLow);
        }

        var firstEmaList = GetMovingAverageList(stockData, maType, length1, highLowList);
        var secondEmaList = GetMovingAverageList(stockData, maType, length2, firstEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double firstEma = firstEmaList[i];
            double secondEma = secondEmaList[i];

            double ratio = secondEma != 0 ? firstEma / secondEma : 0;
            ratioList.AddRounded(ratio);

            double massIndex = ratioList.TakeLastExt(length3).Sum();
            massIndexList.AddRounded(massIndex);
        }

        var massIndexSignalList = GetMovingAverageList(stockData, maType, signalLength, massIndexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double massIndex = massIndexList[i];
            double massIndexEma = massIndexSignalList[i];
            double prevMassIndex = i >= 1 ? massIndexList[i - 1] : 0;
            double prevMassIndexEma = i >= 1 ? massIndexSignalList[i - 1] : 0;

            var signal = GetCompareSignal(massIndex - massIndexEma, prevMassIndex - prevMassIndexEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mi", massIndexList },
            { "Signal", massIndexSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = massIndexList;
        stockData.IndicatorName = IndicatorName.MassIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mass Thrust Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMassThrustOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 14)
    {
        List<double> topList = new();
        List<double> botList = new();
        List<double> mtoList = new();
        List<double> advList = new();
        List<double> decList = new();
        List<double> advVolList = new();
        List<double> decVolList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentVolume = volumeList[i];

            double adv = i >= 1 && currentValue > prevValue ? MinPastValues(i, 1, currentValue - prevValue) : 0;
            advList.AddRounded(adv);

            double dec = i >= 1 && currentValue < prevValue ? MinPastValues(i, 1, prevValue - currentValue) : 0;
            decList.AddRounded(dec);

            double advSum = advList.TakeLastExt(length).Sum();
            double decSum = decList.TakeLastExt(length).Sum();

            double advVol = currentValue > prevValue && advSum != 0 ? currentVolume / advSum : 0;
            advVolList.AddRounded(advVol);

            double decVol = currentValue < prevValue && decSum != 0 ? currentVolume / decSum : 0;
            decVolList.AddRounded(decVol);

            double advVolSum = advVolList.TakeLastExt(length).Sum();
            double decVolSum = decVolList.TakeLastExt(length).Sum();

            double top = (advSum * advVolSum) - (decSum * decVolSum);
            topList.AddRounded(top);

            double bot = (advSum * advVolSum) + (decSum * decVolSum);
            botList.AddRounded(bot);

            double mto = bot != 0 ? 100 * top / bot : 0;
            mtoList.AddRounded(mto);
        }

        var mtoEmaList = GetMovingAverageList(stockData, maType, length, mtoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mto = mtoList[i];
            double mtoEma = mtoEmaList[i];
            double prevMto = i >= 1 ? mtoList[i - 1] : 0;
            double prevMtoEma = i >= 1 ? mtoEmaList[i - 1] : 0;

            var signal = GetRsiSignal(mto - mtoEma, prevMto - prevMtoEma, mto, prevMto, 50, -50);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mto", mtoList },
            { "Signal", mtoEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mtoList;
        stockData.IndicatorName = IndicatorName.MassThrustOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Midpoint Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateMidpointOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 26, int signalLength = 9)
    {
        List<double> moList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double hh = highestList[i];
            double ll = lowestList[i];

            double mo = hh - ll != 0 ? MinOrMax(100 * ((2 * currentValue) - hh - ll) / (hh - ll), 100, -100) : 0;
            moList.AddRounded(mo);
        }

        var moEmaList = GetMovingAverageList(stockData, maType, signalLength, moList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mo = moList[i];
            double moEma = moEmaList[i];
            double prevMo = i >= 1 ? moList[i - 1] : 0;
            double prevMoEma = i >= 1 ? moEmaList[i - 1] : 0;

            var signal = GetRsiSignal(mo - moEma, prevMo - prevMoEma, mo, prevMo, 70, -70);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mo", moList },
            { "Signal", moEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = moList;
        stockData.IndicatorName = IndicatorName.MidpointOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Morphed Sine Wave
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="power"></param>
    /// <returns></returns>
    public static StockData CalculateMorphedSineWave(this StockData stockData, int length = 14, double power = 100)
    {
        List<double> sList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double p = length / (2 * Math.PI);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevS1 = i >= 1 ? sList[i - 1] : 0;
            double prevS2 = i >= 2 ? sList[i - 2] : 0;
            double c = (currentValue * power) + Math.Sin(i / p);

            double s = c / power;
            sList.AddRounded(s);

            var signal = GetCompareSignal(s - prevS1, prevS1 - prevS2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Msw", sList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sList;
        stockData.IndicatorName = IndicatorName.MorphedSineWave;

        return stockData;
    }

    /// <summary>
    /// Calculates the Move Tracker
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateMoveTracker(this StockData stockData)
    {
        List<double> mtList = new();
        List<double> mtSignalList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevMt = mtList.LastOrDefault();
            double mt = MinPastValues(i, 1, currentValue - prevValue);
            mtList.AddRounded(mt);

            double prevMtSignal = mtSignalList.LastOrDefault();
            double mtSignal = mt - prevMt;
            mtSignalList.AddRounded(mtSignal);

            var signal = GetCompareSignal(mt - mtSignal, prevMt - prevMtSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mt", mtList },
            { "Signal", mtSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mtList;
        stockData.IndicatorName = IndicatorName.MoveTracker;

        return stockData;
    }

    /// <summary>
    /// Calculates the Multi Level Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateMultiLevelIndicator(this StockData stockData, int length = 14, double factor = 10000)
    {
        List<double> zList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double prevOpen = i >= length ? openList[i - length] : 0;
            double currentOpen = openList[i];
            double currentClose = inputList[i];
            double prevZ1 = i >= 1 ? zList[i - 1] : 0;
            double prevZ2 = i >= 2 ? zList[i - 2] : 0;

            double z = (currentClose - currentOpen - (currentClose - prevOpen)) * factor;
            zList.AddRounded(z);

            var signal = GetRsiSignal(z - prevZ1, prevZ1 - prevZ2, z, prevZ1, 5, -5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mli", zList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zList;
        stockData.IndicatorName = IndicatorName.MultiLevelIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Modified Gann Hilo Activator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateModifiedGannHiloActivator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 50, double mult = 1)
    {
        List<double> gannHiloList = new();
        List<double> cList = new();
        List<double> dList = new();
        List<double> gList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highestHigh = highestList[i];
            double lowestLow = lowestList[i];
            double currentClose = inputList[i];
            double currentOpen = openList[i];
            double max = Math.Max(currentClose, currentOpen);
            double min = Math.Min(currentClose, currentOpen);
            double a = highestHigh - max;
            double b = min - lowestLow;

            double c = max + (a * mult);
            cList.AddRounded(c);

            double d = min - (b * mult);
            dList.AddRounded(d);
        }

        var eList = GetMovingAverageList(stockData, maType, length, cList);
        var fList = GetMovingAverageList(stockData, maType, length, dList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double f = fList[i];
            double e = eList[i];

            double prevG = gList.LastOrDefault();
            double g = currentClose > e ? 1 : currentClose > f ? 0 : prevG;
            gList.AddRounded(g);

            double prevGannHilo = gannHiloList.LastOrDefault();
            double gannHilo = (g * f) + ((1 - g) * e);
            gannHiloList.AddRounded(gannHilo);

            var signal = GetCompareSignal(currentClose - gannHilo, prevClose - prevGannHilo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ghla", gannHiloList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gannHiloList;
        stockData.IndicatorName = IndicatorName.ModifiedGannHiloActivator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Market Direction Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateMarketDirectionIndicator(this StockData stockData, int fastLength = 13, int slowLength = 55)
    {
        List<double> mdiList = new();
        List<double> cp2List = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double len1Sum = tempList.TakeLastExt(fastLength - 1).Sum();
            double len2Sum = tempList.TakeLastExt(slowLength - 1).Sum();

            double prevCp2 = cp2List.LastOrDefault();
            double cp2 = ((fastLength * len2Sum) - (slowLength * len1Sum)) / (slowLength - fastLength);
            cp2List.AddRounded(cp2);

            double prevMdi = mdiList.LastOrDefault();
            double mdi = currentValue + prevValue != 0 ? 100 * (prevCp2 - cp2) / ((currentValue + prevValue) / 2) : 0;
            mdiList.AddRounded(mdi);

            var signal = GetCompareSignal(mdi, prevMdi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mdi", mdiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mdiList;
        stockData.IndicatorName = IndicatorName.MarketDirectionIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mobility Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateMobilityOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length1 = 10, int length2 = 14, int signalLength = 7)
    {
        List<double> moList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double hMax = highestList[i];
            double lMin = lowestList[i];
            double prevC = i >= length2 ? inputList[i - length2] : 0;
            double rx = length1 != 0 ? (hMax - lMin) / length1 : 0;

            int imx = 1;
            double pdfmx = 0, pdfc = 0, rx1, bu, bl, bu1, bl1, pdf;
            for (int j = 1; j <= length1; j++)
            {
                bu = lMin + (j * rx);
                bl = bu - rx;

                double currHigh = i >= j ? highList[i - j] : 0;
                double currLow = i >= j ? lowList[i - j] : 0;
                double hMax1 = currHigh, lMin1 = currLow;
                for (int k = 2; k < length2; k++)
                {
                    double high = i >= j + k ? highList[i - (j + k)] : 0;
                    double low = i >= j + k ? lowList[i - (j + k)] : 0;
                    hMax1 = Math.Max(high, hMax1);
                    lMin1 = Math.Min(low, lMin1);
                }

                rx1 = length1 != 0 ? (hMax1 - lMin1) / length1 : 0; //-V3022
                bl1 = lMin1 + ((j - 1) * rx1);
                bu1 = lMin1 + (j * rx1);

                pdf = 0;
                for (int k = 1; k <= length2; k++)
                {
                    double high = i >= j + k ? highList[i - (j + k)] : 0;
                    double low = i >= j + k ? lowList[i - (j + k)] : 0;

                    if (high <= bu1)
                    {
                        pdf += 1;
                    }
                    if (high <= bu1 || low >= bu1)
                    {
                        if (high <= bl1)
                        {
                            pdf -= 1;
                        }
                        if (high <= bl || low >= bl1)
                        {
                            continue;
                        }
                        else
                        {
                            pdf -= high - low != 0 ? (bl1 - low) / (high - low) : 0;
                        }
                    }
                    else
                    {
                        pdf += high - low != 0 ? (bu1 - low) / (high - low) : 0;
                    }
                }

                pdf = length2 != 0 ? pdf / length2 : 0;
                pdfmx = j == 1 ? pdf : pdfmx;
                imx = j == 1 ? j : imx;
                pdfmx = Math.Max(pdf, pdfmx);
                pdfc = j == 1 ? pdf : pdfc;
                pdfc = prevC > bl && prevC <= bu ? pdf : pdfc;
            }

            double pmo = lMin + ((imx - 0.5m) * rx);
            double mo = pdfmx != 0 ? 100 * (1 - (pdfc / pdfmx)) : 0;
            mo = prevC < pmo ? -mo : mo;
            moList.AddRounded(-mo);
        }

        var moWmaList = GetMovingAverageList(stockData, maType, signalLength, moList);
        var moSigList = GetMovingAverageList(stockData, maType, signalLength, moWmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mo = moWmaList[i];
            double moSig = moSigList[i];
            double prevMo = i >= 1 ? moWmaList[i - 1] : 0;
            double prevMoSig = i >= 1 ? moSigList[i - 1] : 0;

            var signal = GetCompareSignal(mo - moSig, prevMo - prevMoSig);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mo", moWmaList },
            { "Signal", moSigList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = moWmaList;
        stockData.IndicatorName = IndicatorName.MobilityOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mass Thrust Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMassThrustIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<double> mtiList = new();
        List<double> advList = new();
        List<double> decList = new();
        List<double> advVolList = new();
        List<double> decVolList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double currentVolume = volumeList[i];

            double adv = i >= 1 && currentValue > prevValue ? MinPastValues(i, 1, currentValue - prevValue) : 0;
            advList.AddRounded(adv);

            double dec = i >= 1 && currentValue < prevValue ? MinPastValues(i, 1, prevValue - currentValue) : 0;
            decList.AddRounded(dec);

            double advSum = advList.TakeLastExt(length).Sum();
            double decSum = decList.TakeLastExt(length).Sum();

            double advVol = currentValue > prevValue && advSum != 0 ? currentVolume / advSum : 0;
            advVolList.AddRounded(advVol);

            double decVol = currentValue < prevValue && decSum != 0 ? currentVolume / decSum : 0;
            decVolList.AddRounded(decVol);

            double advVolSum = advVolList.TakeLastExt(length).Sum();
            double decVolSum = decVolList.TakeLastExt(length).Sum();

            double mti = ((advSum * advVolSum) - (decSum * decVolSum)) / 1000000;
            mtiList.AddRounded(mti);
        }

        var mtiEmaList = GetMovingAverageList(stockData, maType, length, mtiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mtiEma = mtiEmaList[i];
            double prevMtiEma = i >= 1 ? mtiEmaList[i - 1] : 0;

            var signal = GetCompareSignal(mtiEma, prevMtiEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mti", mtiList },
            { "Signal", mtiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mtiList;
        stockData.IndicatorName = IndicatorName.MassThrustIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Decision Point Breadth Swenlin Trading Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDecisionPointBreadthSwenlinTradingOscillator(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 5)
    {
        List<double> iList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double advance = currentValue > prevValue ? 1 : 0;
            double decline = currentValue < prevValue ? 1 : 0;

            double iVal = advance + decline != 0 ? 1000 * (advance - decline) / (advance + decline) : 0;
            iList.AddRounded(iVal);
        }

        var ivalEmaList = GetMovingAverageList(stockData, maType, length, iList);
        var stoList = GetMovingAverageList(stockData, maType, length, ivalEmaList);
        var stoEmaList = GetMovingAverageList(stockData, maType, length, stoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double sto = stoList[i];
            double stoEma = stoEmaList[i];
            double prevSto = i >= 1 ? stoList[i - 1] : 0;
            double prevStoEma = i >= 1 ? stoEmaList[i - 1] : 0;

            var signal = GetCompareSignal(sto - stoEma, prevSto - prevStoEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dpbsto", stoList },
            { "Signal", stoEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stoList;
        stockData.IndicatorName = IndicatorName.DecisionPointBreadthSwenlinTradingOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Delta Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateDeltaMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 10, int length2 = 5)
    {
        List<double> deltaList = new();
        List<double> deltaHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentClose = inputList[i];
            double prevOpen = i >= length2 ? openList[i - length2] : 0;

            double delta = currentClose - prevOpen;
            deltaList.AddRounded(delta);
        }

        var deltaSmaList = GetMovingAverageList(stockData, maType, length1, deltaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double delta = deltaList[i];
            double deltaSma = deltaSmaList[i];

            double prevDeltaHistogram = deltaHistogramList.LastOrDefault();
            double deltaHistogram = delta - deltaSma;
            deltaHistogramList.AddRounded(deltaHistogram);

            var signal = GetCompareSignal(deltaHistogram, prevDeltaHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Delta", deltaList },
            { "Signal", deltaSmaList },
            { "Histogram", deltaHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = deltaList;
        stockData.IndicatorName = IndicatorName.DeltaMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Detrended Synthetic Price
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDetrendedSyntheticPrice(this StockData stockData, int length = 14)
    {
        List<double> dspList = new();
        List<double> ema1List = new();
        List<double> ema2List = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        double alpha = length > 2 ? (double)2 / (length + 1) : 0.67m;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double high = Math.Max(currentHigh, prevHigh);
            double low = Math.Min(currentLow, prevLow);
            double price = (high + low) / 2;
            double prevEma1 = i >= 1 ? ema1List[i - 1] : price;
            double prevEma2 = i >= 1 ? ema2List[i - 1] : price;

            double ema1 = (alpha * price) + ((1 - alpha) * prevEma1);
            ema1List.AddRounded(ema1);

            double ema2 = (alpha / 2 * price) + ((1 - (alpha / 2)) * prevEma2);
            ema2List.AddRounded(ema2);

            double prevDsp = dspList.LastOrDefault();
            double dsp = ema1 - ema2;
            dspList.AddRounded(dsp);

            var signal = GetCompareSignal(dsp, prevDsp);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dsp", dspList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dspList;
        stockData.IndicatorName = IndicatorName.DetrendedSyntheticPrice;

        return stockData;
    }

    /// <summary>
    /// Calculates the Derivative Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateDerivativeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 14,
        int length2 = 9, int length3 = 5, int length4 = 3)
    {
        List<double> s1List = new();
        List<double> s2List = new();
        List<double> s1SmaList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length1).CustomValuesList;
        var rsiEma1List = GetMovingAverageList(stockData, maType, length3, rsiList);
        var rsiEma2List = GetMovingAverageList(stockData, maType, length4, rsiEma1List);

        for (int i = 0; i < rsiList.Count; i++)
        {
            double prevS1 = s1List.LastOrDefault();
            double s1 = rsiEma2List[i];
            s1List.AddRounded(s1);

            double prevS1Sma = s1SmaList.LastOrDefault();
            double s1Sma = s1List.TakeLastExt(length2).Average();
            s1SmaList.AddRounded(s1Sma);

            double s2 = s1 - s1Sma;
            s2List.AddRounded(s2);

            var signal = GetCompareSignal(s1 - s1Sma, prevS1 - prevS1Sma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Do", s2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = s2List;
        stockData.IndicatorName = IndicatorName.DerivativeOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Demand Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateDemandOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 10, int length2 = 2, int length3 = 20)
    {
        List<double> rangeList = new();
        List<double> doList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];

            double range = highest - lowest;
            rangeList.AddRounded(range);
        }

        var vaList = GetMovingAverageList(stockData, maType, length1, rangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double va = vaList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double pctChg = prevValue != 0 ? MinPastValues(i, 1, currentValue - prevValue) / Math.Abs(prevValue) * 100 : 0;
            double currentVolume = stockData.Volumes[i];
            double k = va != 0 ? (3 * currentValue) / va : 0;
            double pctK = pctChg * k;
            double volPctK = pctK != 0 ? currentVolume / pctK : 0;
            double bp = currentValue > prevValue ? currentVolume : volPctK;
            double sp = currentValue > prevValue ? volPctK : currentVolume;

            double dosc = bp - sp;
            doList.AddRounded(dosc);
        }

        var doEmaList = GetMovingAverageList(stockData, maType, length3, doList);
        var doSigList = GetMovingAverageList(stockData, maType, length1, doEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double doSig = doSigList[i];
            double prevSig1 = i >= 1 ? doSigList[i - 1] : 0;
            double prevSig2 = i >= 2 ? doSigList[i - 1] : 0;

            var signal = GetCompareSignal(doSig - prevSig1, prevSig1 - prevSig2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Do", doEmaList },
            { "Signal", doSigList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = doEmaList;
        stockData.IndicatorName = IndicatorName.DemandOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Double Smoothed Momenta
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateDoubleSmoothedMomenta(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 2,
        int length2 = 5, int length3 = 25)
    {
        List<double> momList = new();
        List<double> srcLcList = new();
        List<double> hcLcList = new();
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

            double hcLc = hc - lc;
            hcLcList.AddRounded(hcLc);
        }

        var topEma1List = GetMovingAverageList(stockData, maType, length2, srcLcList);
        var topEma2List = GetMovingAverageList(stockData, maType, length3, topEma1List);
        var botEma1List = GetMovingAverageList(stockData, maType, length2, hcLcList);
        var botEma2List = GetMovingAverageList(stockData, maType, length3, botEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double top = topEma2List[i];
            double bot = botEma2List[i];

            double mom = bot != 0 ? MinOrMax(100 * top / bot, 100, 0) : 0;
            momList.AddRounded(mom);
        }

        var momEmaList = GetMovingAverageList(stockData, maType, length3, momList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double mom = momList[i];
            double momEma = momEmaList[i];
            double prevMom = i >= 1 ? momList[i - 1] : 0;
            double prevMomEma = i >= 1 ? momEmaList[i - 1] : 0;

            var signal = GetCompareSignal(mom - momEma, prevMom - prevMomEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dsm", momList },
            { "Signal", momEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = momList;
        stockData.IndicatorName = IndicatorName.DoubleSmoothedMomenta;

        return stockData;
    }

    /// <summary>
    /// Calculates the Didi Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateDidiIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 3, int length2 = 8,
        int length3 = 20)
    {
        List<double> curtaList = new();
        List<double> longaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mediumSmaList = GetMovingAverageList(stockData, maType, length2, inputList);
        var shortSmaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var longSmaList = GetMovingAverageList(stockData, maType, length3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double mediumSma = mediumSmaList[i];
            double shortSma = shortSmaList[i];
            double longSma = longSmaList[i];

            double prevCurta = curtaList.LastOrDefault();
            double curta = mediumSma != 0 ? shortSma / mediumSma : 0;
            curtaList.AddRounded(curta);

            double prevLonga = longaList.LastOrDefault();
            double longa = mediumSma != 0 ? longSma / mediumSma : 0;
            longaList.AddRounded(longa);

            var signal = GetCompareSignal(curta - longa, prevCurta - prevLonga);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Curta", curtaList },
            { "Media", mediumSmaList },
            { "Longa", longSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.DidiIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Disparity Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDisparityIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<double> disparityIndexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentSma = smaList[i];

            double prevDisparityIndex = disparityIndexList.LastOrDefault();
            double disparityIndex = currentSma != 0 ? (currentValue - currentSma) / currentSma * 100 : 0;
            disparityIndexList.AddRounded(disparityIndex);

            var signal = GetCompareSignal(disparityIndex, prevDisparityIndex);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Di", disparityIndexList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = disparityIndexList;
        stockData.IndicatorName = IndicatorName.DisparityIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Damping Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static StockData CalculateDampingIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 5, 
        double threshold = 1.5m)
    {
        List<double> rangeList = new();
        List<double> diList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            
            double range = currentHigh - currentLow;
            rangeList.AddRounded(range);
        }

        var rangeSmaList = GetMovingAverageList(stockData, maType, length, rangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevSma1 = i >= 1 ? rangeSmaList[i - 1] : 0;
            double prevSma6 = i >= 6 ? rangeSmaList[i - 6] : 0;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevSma = i >= 1 ? smaList[i - 1] : 0;
            double currentSma = smaList[i];

            double di = prevSma6 != 0 ? prevSma1 / prevSma6 : 0;
            diList.AddRounded(di);

            var signal = GetVolatilitySignal(currentValue - currentSma, prevValue - prevSma, di, threshold);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Di", diList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = diList;
        stockData.IndicatorName = IndicatorName.DampingIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Directional Trend Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateDirectionalTrendIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 14,
        int length2 = 10, int length3 = 5)
    {
        List<double> dtiList = new();
        List<double> diffList = new();
        List<double> absDiffList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double hmu = currentHigh - prevHigh > 0 ? currentHigh - prevHigh : 0;
            double lmd = currentLow - prevLow < 0 ? (currentLow - prevLow) * -1 : 0;

            double diff = hmu - lmd;
            diffList.AddRounded(diff);

            double absDiff = Math.Abs(diff);
            absDiffList.AddRounded(absDiff);
        }
        
        var diffEma1List = GetMovingAverageList(stockData, maType, length1, diffList);
        var absDiffEma1List = GetMovingAverageList(stockData, maType, length1, absDiffList);
        var diffEma2List = GetMovingAverageList(stockData, maType, length2, diffEma1List);
        var absDiffEma2List = GetMovingAverageList(stockData, maType, length2, absDiffEma1List);
        var diffEma3List = GetMovingAverageList(stockData, maType, length3, diffEma2List);
        var absDiffEma3List = GetMovingAverageList(stockData, maType, length3, absDiffEma2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double diffEma3 = diffEma3List[i];
            double absDiffEma3 = absDiffEma3List[i];
            double prevDti1 = i >= 1 ? dtiList[i - 1] : 0;
            double prevDti2 = i >= 2 ? dtiList[i - 2] : 0;

            double dti = absDiffEma3 != 0 ? MinOrMax(100 * diffEma3 / absDiffEma3, 100, -100) : 0;
            dtiList.AddRounded(dti);

            var signal = GetRsiSignal(dti - prevDti1, prevDti1 - prevDti2, dti, prevDti1, 25, -25);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dti", dtiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dtiList;
        stockData.IndicatorName = IndicatorName.DirectionalTrendIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Drunkard Walk
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateDrunkardWalk(this StockData stockData, int length1 = 80, int length2 = 14)
    {
        List<double> tempHighList = new();
        List<double> tempLowList = new();
        List<double> upAtrList = new();
        List<double> dnAtrList = new();
        List<double> upwalkList = new();
        List<double> dnwalkList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double highestHigh = highestList[i];
            double lowestLow = lowestList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double currentHigh = highList[i];
            tempHighList.AddRounded(currentHigh);

            double currentLow = lowList[i];
            tempLowList.AddRounded(currentLow);

            double tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            int maxIndex = tempHighList.LastIndexOf(highestHigh);
            int minIndex = tempLowList.LastIndexOf(lowestLow);
            int dnRun = i - maxIndex;
            int upRun = i - minIndex;

            double prevAtrUp = upAtrList.LastOrDefault();
            double upK = upRun != 0 ? (double)1 / upRun : 0;
            double atrUp = (tr * upK) + (prevAtrUp * (1 - upK));
            upAtrList.AddRounded(atrUp);

            double prevAtrDn = dnAtrList.LastOrDefault();
            var dnK = dnRun != 0 ? (double)1 / dnRun : 0;
            double atrDn = (tr * dnK) + (prevAtrDn * (1 - dnK));
            dnAtrList.AddRounded(atrDn);

            double upDen = atrUp > 0 ? atrUp : 1;
            double prevUpWalk = upwalkList.LastOrDefault();
            double upWalk = upRun > 0 ? (currentHigh - lowestLow) / (Sqrt(upRun) * upDen) : 0;
            upwalkList.AddRounded(upWalk);

            double dnDen = atrDn > 0 ? atrDn : 1;
            double prevDnWalk = dnwalkList.LastOrDefault();
            double dnWalk = dnRun > 0 ? (highestHigh - currentLow) / (Sqrt(dnRun) * dnDen) : 0;
            dnwalkList.AddRounded(dnWalk);

            var signal = GetCompareSignal(upWalk - dnWalk, prevUpWalk - prevDnWalk);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpWalk", upwalkList },
            { "DnWalk", dnwalkList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.DrunkardWalk;

        return stockData;
    }

    /// <summary>
    /// Calculates the DT Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateDTOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length1 = 13, 
        int length2 = 8, int length3 = 5, int length4 = 3)
    {
        List<double> stoRsiList = new();
        List<double> skList = new();
        List<double> sdList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var wilderMovingAvgList = GetMovingAverageList(stockData, maType, length1, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(wilderMovingAvgList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double wima = wilderMovingAvgList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double prevSd1 = i >= 1 ? sdList[i - 1] : 0;
            double prevSd2 = i >= 2 ? sdList[i - 2] : 0;

            double stoRsi = highest - lowest != 0 ? MinOrMax(100 * (wima - lowest) / (highest - lowest), 100, 0) : 0;
            stoRsiList.AddRounded(stoRsi);

            double sk = stoRsiList.TakeLastExt(length3).Average();
            skList.AddRounded(sk);

            double sd = skList.TakeLastExt(length4).Average();
            sdList.AddRounded(sd);

            var signal = GetRsiSignal(sd - prevSd1, prevSd1 - prevSd2, sd, prevSd1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dto", skList },
            { "Signal", sdList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = skList;
        stockData.IndicatorName = IndicatorName.DTOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Smoothed Delta Ratio Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSmoothedDeltaRatioOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 100)
    {
        List<double> bList = new();
        List<double> cList = new();
        List<double> absChgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double sma = smaList[i];
            double prevSma = i >= length ? smaList[i - length] : 0;

            double absChg = Math.Abs(MinPastValues(i, length, currentValue - prevValue));
            absChgList.AddRounded(absChg);

            double b = MinPastValues(i, length, sma - prevSma);
            bList.AddRounded(b);
        }

        var aList = GetMovingAverageList(stockData, maType, length, absChgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double a = aList[i];
            double b = bList[i];
            double prevC1 = i >= 1 ? cList[i - 1] : 0;
            double prevC2 = i >= 2 ? cList[i - 2] : 0;

            double c = a != 0 ? MinOrMax(b / a, 1, 0) : 0;
            cList.AddRounded(c);

            var signal = GetRsiSignal(c - prevC1, prevC1 - prevC2, c, prevC1, 0.8m, 0.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sdro", cList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cList;
        stockData.IndicatorName = IndicatorName.SmoothedDeltaRatioOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Support and Resistance Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateSupportAndResistanceOscillator(this StockData stockData)
    {
        List<double> sroList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double currentClose = inputList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
            double prevSro1 = i >= 1 ? sroList[i - 1] : 0;
            double prevSro2 = i >= 2 ? sroList[i - 2] : 0;

            double sro = tr != 0 ? MinOrMax((currentHigh - currentOpen + (currentClose - currentLow)) / (2 * tr), 1, 0) : 0;
            sroList.AddRounded(sro);

            var signal = GetRsiSignal(sro - prevSro1, prevSro1 - prevSro2, sro, prevSro1, 0.7m, 0.3m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sro", sroList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sroList;
        stockData.IndicatorName = IndicatorName.SupportAndResistanceOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stationary Extrapolated Levels Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateStationaryExtrapolatedLevelsOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 200)
    {
        List<double> extList = new();
        List<double> yList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double prevY = i >= length ? yList[i - length] : 0;
            double prevY2 = i >= length * 2 ? yList[i - (length * 2)] : 0;

            double y = currentValue - sma;
            yList.AddRounded(y);

            double ext = ((2 * prevY) - prevY2) / 2;
            extList.AddRounded(ext);
        }

        stockData.CustomValuesList = extList;
        var oscList = CalculateStochasticOscillator(stockData, maType, length: length * 2).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double osc = oscList[i];
            double prevOsc1 = i >= 1 ? oscList[i - 1] : 0;
            double prevOsc2 = i >= 2 ? oscList[i - 2] : 0;

            var signal = GetRsiSignal(osc - prevOsc1, prevOsc1 - prevOsc2, osc, prevOsc1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Selo", oscList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oscList;
        stockData.IndicatorName = IndicatorName.StationaryExtrapolatedLevelsOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sell Gravitation Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSellGravitationIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20)
    {
        List<double> v3List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentClose = inputList[i];
            double currentOpen = openList[i];
            double v1 = currentClose - currentOpen;
            double v2 = currentHigh - currentLow;

            double v3 = v2 != 0 ? v1 / v2 : 0;
            v3List.AddRounded(v3);
        }

        var sgiList = GetMovingAverageList(stockData, maType, length, v3List);
        var sgiEmaList = GetMovingAverageList(stockData, maType, length, sgiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double sgi = sgiList[i];
            double sgiEma = sgiEmaList[i];
            double prevSgi = i >= 1 ? sgiList[i - 1] : 0;
            double prevSgiEma = i >= 1 ? sgiEmaList[i - 1] : 0;

            var signal = GetCompareSignal(sgi - sgiEma, prevSgi - prevSgiEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sgi", sgiList },
            { "Signal", sgiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sgiList;
        stockData.IndicatorName = IndicatorName.SellGravitationIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Strength of Movement
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothingLength"></param>
    /// <returns></returns>
    public static StockData CalculateStrengthOfMovement(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length1 = 10, 
        int length2 = 3, int smoothingLength = 3)
    {
        List<double> aaSeList = new();
        List<double> sSeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length2 - 1 ? inputList[i - (length2 - 1)] : 0;
            double moveSe = MinPastValues(i, length2 - 1, currentValue - prevValue);
            double avgMoveSe = moveSe / (length2 - 1);

            double aaSe = prevValue != 0 ? avgMoveSe / prevValue : 0;
            aaSeList.AddRounded(aaSe);
        }

        var bList = GetMovingAverageList(stockData, maType, length1, aaSeList);
        stockData.CustomValuesList = bList;
        var stoList = CalculateStochasticOscillator(stockData, maType, length: length1).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double bSto = stoList[i];

            double sSe = (bSto * 2) - 100;
            sSeList.AddRounded(sSe);
        }

        var ssSeList = GetMovingAverageList(stockData, maType, smoothingLength, sSeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ssSe = ssSeList[i];
            double prevSsse = i >= 1 ? ssSeList[i - 1] : 0;

            var signal = GetCompareSignal(ssSe, prevSsse);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Som", ssSeList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ssSeList;
        stockData.IndicatorName = IndicatorName.StrengthOfMovement;

        return stockData;
    }
  
    /// <summary>
    /// Calculates the Spearman Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateSpearmanIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 10, 
        int signalLength = 3)
    {
        List<double> coefCorrList = new();
        List<double> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            var enumerableList = tempList.TakeLastExt(length).Select(x => (double)x);
            var orderedList = enumerableList.AsQueryExpr().OrderBy(j => j).Run();

            var sc = Correlation.Spearman(enumerableList, orderedList);
            sc = IsValueNullOrInfinity(sc) ? 0 : sc;
            coefCorrList.AddRounded((double)sc * 100);
        }

        var sigList = GetMovingAverageList(stockData, maType, signalLength, coefCorrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double sc = coefCorrList[i];
            double prevSc = i >= 1 ? coefCorrList[i - 1] : 0;
            double sig = sigList[i];
            double prevSig = i >= 1 ? sigList[i - 1] : 0;

            var signal = GetCompareSignal(sc - sig, prevSc - prevSig);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Si", coefCorrList },
            { "Signal", sigList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = coefCorrList;
        stockData.IndicatorName = IndicatorName.SpearmanIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Smoothed Williams Accumulation Distribution
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSmoothedWilliamsAccumulationDistribution(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14)
    {
        List<Signal> signalsList = new();

        var wadList = CalculateWilliamsAccumulationDistribution(stockData).CustomValuesList;
        var wadSignalList = GetMovingAverageList(stockData, maType, length, wadList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double wad = wadList[i];
            double wadSma = wadSignalList[i];
            double prevWad = i >= 1 ? wadList[i - 1] : 0;
            double prevWadSma = i >= 1 ? wadSignalList[i - 1] : 0;

            var signal = GetCompareSignal(wad - wadSma, prevWad - prevWadSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Swad", wadList },
            { "Signal", wadSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wadList;
        stockData.IndicatorName = IndicatorName.SmoothedWilliamsAccumulationDistribution;

        return stockData;
    }

    /// <summary>
    /// Calculates the Smoothed Rate of Change
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothingLength"></param>
    /// <returns></returns>
    public static StockData CalculateSmoothedRateOfChange(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 21, 
        int smoothingLength = 13)
    {
        List<double> srocList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, smoothingLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentMa = maList[i];
            double prevMa = i >= length ? maList[i - length] : 0;
            double mom = currentMa - prevMa;

            double prevSroc = srocList.LastOrDefault();
            double sroc = prevMa != 0 ? 100 * mom / prevMa : 100;
            srocList.AddRounded(sroc);

            var signal = GetCompareSignal(sroc, prevSroc);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sroc", srocList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = srocList;
        stockData.IndicatorName = IndicatorName.SmoothedRateOfChange;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sentiment Zone Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateSentimentZoneOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.TripleExponentialMovingAverage,
        int fastLength = 14, int slowLength = 30, double factor = 0.95m)
    {
        List<double> rList = new();
        List<double> szoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double r = currentValue > prevValue ? 1 : -1;
            rList.AddRounded(r);
        }

        var spList = GetMovingAverageList(stockData, maType, fastLength, rList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double sp = spList[i];

            double szo = fastLength != 0 ? 100 * sp / fastLength : 0;
            szoList.AddRounded(szo);
        }

        var (highestList, lowestList) = GetMaxAndMinValuesList(szoList, slowLength);
        for (int i = 0; i < stockData.Count; i++)
        {
            double highest = highestList[i];
            double lowest = lowestList[i];
            double range = highest - lowest;
            double ob = lowest + (range * factor);
            double os = highest - (range * factor);
            double szo = szoList[i];
            double prevSzo1 = i >= 1 ? szoList[i - 1] : 0;
            double prevSzo2 = i >= 2 ? szoList[i - 2] : 0;

            var signal = GetRsiSignal(szo - prevSzo1, prevSzo1 - prevSzo2, szo, prevSzo1, ob, os, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Szo", szoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = szoList;
        stockData.IndicatorName = IndicatorName.SentimentZoneOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Simple Cycle
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSimpleCycle(this StockData stockData, int length = 50)
    {
        List<double> srcList = new();
        List<double> cEmaList = new();
        List<double> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double a = (double)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevC1 = i >= 1 ? cList[i - 1] : 0;
            double prevC2 = i >= 2 ? cList[i - 2] : 0;
            double prevSrc = i >= length ? srcList[i - length] : 0;

            double src = currentValue + prevC1;
            srcList.AddRounded(src);

            double cEma = CalculateEMA(prevC1, cEmaList.LastOrDefault(), length);
            cEmaList.AddRounded(cEma);

            double b = prevC1 - cEma;
            double c = (a * (src - prevSrc)) + ((1 - a) * b);
            cList.AddRounded(c);

            var signal = GetCompareSignal(c - prevC1, prevC1 - prevC2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sc", cList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cList;
        stockData.IndicatorName = IndicatorName.SimpleCycle;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stiffness Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothingLength"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static StockData CalculateStiffnessIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 100, int length2 = 60, int smoothingLength = 3, double threshold = 90)
    {
        List<double> aboveList = new();
        List<double> stiffValueList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double sma = smaList[i];
            double stdDev = stdDevList[i];
            double bound = sma - (0.2m * stdDev);

            double above = currentValue > bound ? 1 : 0;
            aboveList.AddRounded(above);

            double aboveSum = aboveList.TakeLastExt(length2).Sum();
            double stiffValue = length2 != 0 ? aboveSum * 100 / length2 : 0;
            stiffValueList.AddRounded(stiffValue);
        }

        var stiffnessList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, smoothingLength, stiffValueList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double stiffness = stiffnessList[i];
            double prevStiffness = i >= 1 ? stiffnessList[i - 1] : 0;

            var signal = GetCompareSignal(stiffness - threshold, prevStiffness - threshold);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Si", stiffnessList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stiffnessList;
        stockData.IndicatorName = IndicatorName.StiffnessIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Super Trend Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateSuperTrendFilter(this StockData stockData, int length = 200, double factor = 0.9m)
    {
        List<double> tList = new();
        List<double> srcList = new();
        List<double> trendUpList = new();
        List<double> trendDnList = new();
        List<double> trendList = new();
        List<double> tslList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double p = Pow(length, 2), a = 2 / (p + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevTsl1 = i >= 1 ? tslList[i - 1] : currentValue;
            double prevTsl2 = i >= 2 ? tslList[i - 2] : 0;
            double d = Math.Abs(currentValue - prevTsl1);

            double prevT = i >= 1 ? tList[i - 1] : d;
            double t = (a * d) + ((1 - a) * prevT);
            tList.AddRounded(t);

            double prevSrc = srcList.LastOrDefault();
            double src = (factor * prevTsl1) + ((1 - factor) * currentValue);
            srcList.AddRounded(src);

            double up = prevTsl1 - t;
            double dn = prevTsl1 + t;

            double prevTrendUp = trendUpList.LastOrDefault();
            double trendUp = prevSrc > prevTrendUp ? Math.Max(up, prevTrendUp) : up;
            trendUpList.AddRounded(trendUp);

            double prevTrendDn = trendDnList.LastOrDefault();
            double trendDn = prevSrc < prevTrendDn ? Math.Min(dn, prevTrendDn) : dn;
            trendDnList.AddRounded(trendDn);

            double prevTrend = i >= 1 ? trendList[i - 1] : 1;
            double trend = src > prevTrendDn ? 1 : src < prevTrendUp ? -1 : prevTrend;
            trendList.AddRounded(trend);

            double tsl = trend == 1 ? trendDn : trendUp;
            tslList.AddRounded(tsl);

            var signal = GetCompareSignal(tsl - prevTsl1, prevTsl1 - prevTsl2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Stf", tslList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tslList;
        stockData.IndicatorName = IndicatorName.SuperTrendFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the SMI Ergodic Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateSMIErgodicIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int fastLength = 5,
        int slowLength = 20, int signalLength = 5)
    {
        List<double> pcList = new();
        List<double> absPCList = new();
        List<double> smiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double pc = MinPastValues(i, 1, currentValue - prevValue);
            pcList.AddRounded(pc);

            double absPC = Math.Abs(pc);
            absPCList.AddRounded(absPC);
        }

        var pcSmooth1List = GetMovingAverageList(stockData, maType, fastLength, pcList); 
        var pcSmooth2List = GetMovingAverageList(stockData, maType, slowLength, pcSmooth1List);
        var absPCSmooth1List = GetMovingAverageList(stockData, maType, fastLength, absPCList);
        var absPCSmooth2List = GetMovingAverageList(stockData, maType, slowLength, absPCSmooth1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double absSmooth2PC = absPCSmooth2List[i];
            double smooth2PC = pcSmooth2List[i];

            double smi = absSmooth2PC != 0 ? MinOrMax(100 * smooth2PC / absSmooth2PC, 100, -100) : 0;
            smiList.AddRounded(smi);
        }

        var smiSignalList = GetMovingAverageList(stockData, maType, signalLength, smiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double smi = smiList[i];
            double smiSignal = smiSignalList[i];
            double prevSmi = i >= 1 ? smiList[i - 1] : 0;
            double prevSmiSignal = i >= 1 ? smiSignalList[i - 1] : 0;

            var signal = GetRsiSignal(smi - smiSignal, prevSmi - prevSmiSignal, smi, prevSmi, 10, -10);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Smi", smiList },
            { "Signal", smiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = smiList;
        stockData.IndicatorName = IndicatorName.SMIErgodicIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Simple Lines
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateSimpleLines(this StockData stockData, int length = 10, double mult = 10)
    {
        List<double> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double s = 0.01m * 100 * ((double)1 / length);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevA = i >= 1 ? aList[i - 1] : currentValue;
            double prevA2 = i >= 2 ? aList[i - 2] : currentValue;
            double x = currentValue + ((prevA - prevA2) * mult);

            prevA = i >= 1 ? aList[i - 1] : x;
            double a = x > prevA + s ? prevA + s : x < prevA - s ? prevA - s : prevA;
            aList.AddRounded(a);

            var signal = GetCompareSignal(a - prevA, prevA - prevA2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sl", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.SimpleLines;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sector Rotation Model
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="marketData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateSectorRotationModel(this StockData stockData, StockData marketData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 25, int length2 = 75)
    {
        List<double> oscList = new();
        List<Signal> signalsList = new();

        if (stockData.Count == marketData.Count)
        {
            var bull1List = CalculateRateOfChange(stockData, length1).CustomValuesList;
            var bull2List = CalculateRateOfChange(stockData, length2).CustomValuesList;
            var bear1List = CalculateRateOfChange(marketData, length1).CustomValuesList;
            var bear2List = CalculateRateOfChange(marketData, length2).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                double bull1 = bull1List[i];
                double bull2 = bull2List[i];
                double bear1 = bear1List[i];
                double bear2 = bear2List[i];
                double bull = (bull1 + bull2) / 2;
                double bear = (bear1 + bear2) / 2;

                double osc = 100 * (bull - bear);
                oscList.AddRounded(osc);
            }

            var oscEmaList = GetMovingAverageList(stockData, maType, length1, oscList);
            for (int i = 0; i < stockData.Count; i++)
            {
                double oscEma = oscEmaList[i];
                double prevOscEma1 = i >= 1 ? oscEmaList[i - 1] : 0;
                double prevOscEma2 = i >= 2 ? oscEmaList[i - 2] : 0;

                var signal = GetCompareSignal(oscEma - prevOscEma1, prevOscEma1 - prevOscEma2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Srm", oscList },
                { "Signal", oscEmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = oscList;
            stockData.IndicatorName = IndicatorName.SectorRotationModel;
        }

        return stockData;
    }

    /// <summary>
    /// Calculates the Enhanced Williams R
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateEnhancedWilliamsR(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14, 
        int signalLength = 5)
    {
        List<double> ewrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);
        var (highestList1, lowestList1) = GetMaxAndMinValuesList(inputList, length);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(volumeList, length);

        double af = length < 10 ? 0.25m : ((double)length / 32) - 0.0625m;
        int smaLength = MinOrMax((int)Math.Ceiling((double)length / 2));

        var srcSmaList = GetMovingAverageList(stockData, maType, smaLength, inputList);
        var volSmaList = GetMovingAverageList(stockData, maType, smaLength, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double maxVol = highestList2[i];
            double minVol = lowestList2[i];
            double maxSrc = highestList1[i];
            double minSrc = lowestList1[i];
            double srcSma = srcSmaList[i];
            double volSma = volSmaList[i];
            double volume = volumeList[i];
            double volWr = maxVol - minVol != 0 ? 2 * ((volume - volSma) / (maxVol - minVol)) : 0;
            double srcWr = maxSrc - minSrc != 0 ? 2 * ((currentValue - srcSma) / (maxSrc - minSrc)) : 0;
            double srcSwr = maxSrc - minSrc != 0 ? 2 * (MinPastValues(i, 1, currentValue - prevValue) / (maxSrc - minSrc)) : 0;

            double ewr = ((volWr > 0 && srcWr > 0 && currentValue > prevValue) || (volWr > 0 && srcWr < 0 && currentValue < prevValue)) && srcSwr + af != 0 ?
                ((50 * (srcWr * (srcSwr + af) * volWr)) + srcSwr + af) / (srcSwr + af) : 25 * ((srcWr * (volWr + 1)) + 2);
            ewrList.AddRounded(ewr);
        }

        var ewrSignalList = GetMovingAverageList(stockData, maType, signalLength, ewrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ewr = ewrList[i];
            double ewrSignal = ewrSignalList[i];
            double prevEwr = i >= 1 ? ewrList[i - 1] : 0;
            double prevEwrSignal = i >= 1 ? ewrSignalList[i - 1] : 0;

            var signal = GetRsiSignal(ewr - ewrSignal, prevEwr - prevEwrSignal, ewr, prevEwr, 100, -100);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ewr", ewrList },
            { "Signal", ewrSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ewrList;
        stockData.IndicatorName = IndicatorName.EnhancedWilliamsR;

        return stockData;
    }

    /// <summary>
    /// Calculates the Earning Support Resistance Levels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="inputName"></param>
    /// <returns></returns>
    public static StockData CalculateEarningSupportResistanceLevels(this StockData stockData, InputName inputName = InputName.MedianPrice)
    {
        List<double> mode1List = new();
        List<double> mode2List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentHigh = highList[i];
            double prevClose = i >= 1 ? closeList[i - 1] : 0;
            double prevLow = i >= 2 ? lowList[i - 2] : 0;
            double prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            double prevValue1 = i >= 1 ? inputList[i - 1] : 0;

            double prevMode1 = mode1List.LastOrDefault();
            double mode1 = (prevLow + currentHigh) / 2;
            mode1List.AddRounded(mode1);

            double prevMode2 = mode2List.LastOrDefault();
            double mode2 = (prevValue2 + currentValue + prevClose) / 3;
            mode2List.AddRounded(mode2);

            var signal = GetBullishBearishSignal(currentValue - Math.Max(mode1, mode2), prevValue1 - Math.Max(prevMode1, prevMode2),
                currentValue - Math.Min(mode1, mode2), prevValue1 - Math.Min(prevMode1, prevMode2));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Esr", mode1List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mode1List;
        stockData.IndicatorName = IndicatorName.EarningSupportResistanceLevels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Elder Market Thermometer
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateElderMarketThermometer(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 22)
    {
        List<double> emtList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;

            double emt = currentHigh < prevHigh && currentLow > prevLow ? 0 : currentHigh - prevHigh > prevLow - currentLow ? Math.Abs(currentHigh - prevHigh) :
                Math.Abs(prevLow - currentLow);
            emtList.AddRounded(emt);
        }

        var aemtList = GetMovingAverageList(stockData, maType, length, emtList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentEma = emaList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevEma = i >= 1 ? emaList[i - 1] : 0;
            double emt = emtList[i];
            double emtEma = aemtList[i];

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, emt, emtEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emt", emtList },
            { "Signal", aemtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emtList;
        stockData.IndicatorName = IndicatorName.ElderMarketThermometer;

        return stockData;
    }

    /// <summary>
    /// Calculates the Elliott Wave Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateElliottWaveOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int fastLength = 5,
        int slowLength = 34)
    {
        List<double> ewoList = new();
        List<double> ewoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var sma34List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentSma5 = smaList[i];
            double currentSma34 = sma34List[i];

            double ewo = currentSma5 - currentSma34;
            ewoList.AddRounded(ewo);
        }

        var ewoSignalLineList = GetMovingAverageList(stockData, maType, fastLength, ewoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ewo = ewoList[i];
            double ewoSignalLine = ewoSignalLineList[i];

            double prevEwoHistogram = ewoHistogramList.LastOrDefault();
            double ewoHistogram = ewo - ewoSignalLine;
            ewoHistogramList.AddRounded(ewoHistogram);

            var signal = GetCompareSignal(ewoHistogram, prevEwoHistogram);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ewo", ewoList },
            { "Signal", ewoSignalLineList },
            { "Histogram", ewoHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ewoList;
        stockData.IndicatorName = IndicatorName.ElliottWaveOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ergodic Candlestick Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateErgodicCandlestickOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 32, int length2 = 12)
    {
        List<double> xcoList = new();
        List<double> xhlList = new();
        List<double> ecoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double currentClose = inputList[i];

            double xco = currentClose - currentOpen;
            xcoList.AddRounded(xco);

            double xhl = currentHigh - currentLow;
            xhlList.AddRounded(xhl);
        }

        var xcoEma1List = GetMovingAverageList(stockData, maType, length1, xcoList);
        var xcoEma2List = GetMovingAverageList(stockData, maType, length2, xcoEma1List);
        var xhlEma1List = GetMovingAverageList(stockData, maType, length1, xhlList);
        var xhlEma2List = GetMovingAverageList(stockData, maType, length2, xhlEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double xhlEma2 = xhlEma2List[i];
            double xcoEma2 = xcoEma2List[i];

            double eco = xhlEma2 != 0 ? 100 * xcoEma2 / xhlEma2 : 0;
            ecoList.AddRounded(eco);
        }

        var ecoSignalList = GetMovingAverageList(stockData, maType, length2, ecoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double eco = ecoList[i];
            double ecoEma = ecoSignalList[i];
            double prevEco = i >= 1 ? ecoList[i - 1] : 0;
            double prevEcoEma = i >= 1 ? ecoSignalList[i - 1] : 0;

            var signal = GetCompareSignal(eco - ecoEma, prevEco - prevEcoEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eco", ecoList },
            { "Signal", ecoSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ecoList;
        stockData.IndicatorName = IndicatorName.ErgodicCandlestickOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ergodic True Strength Index V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateErgodicTrueStrengthIndexV1(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 4, int length2 = 8, int length3 = 6, int signalLength = 3)
    {
        List<double> etsiList = new();
        List<double> priceDiffList = new();
        List<double> absPriceDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double priceDiff = MinPastValues(i, 1, currentValue - prevValue);
            priceDiffList.AddRounded(priceDiff);

            double absPriceDiff = Math.Abs(priceDiff);
            absPriceDiffList.AddRounded(absPriceDiff);
        }

        var diffEma1List = GetMovingAverageList(stockData, maType, length1, priceDiffList);
        var absDiffEma1List = GetMovingAverageList(stockData, maType, length1, absPriceDiffList);
        var diffEma2List = GetMovingAverageList(stockData, maType, length2, diffEma1List);
        var absDiffEma2List = GetMovingAverageList(stockData, maType, length2, absDiffEma1List);
        var diffEma3List = GetMovingAverageList(stockData, maType, length3, diffEma2List);
        var absDiffEma3List = GetMovingAverageList(stockData, maType, length3, absDiffEma2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double diffEma3 = diffEma3List[i];
            double absDiffEma3 = absDiffEma3List[i];

            double etsi = absDiffEma3 != 0 ? MinOrMax(100 * diffEma3 / absDiffEma3, 100, -100) : 0;
            etsiList.AddRounded(etsi);
        }

        var etsiSignalList = GetMovingAverageList(stockData, maType, signalLength, etsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double etsi = etsiList[i];
            double etsiSignal = etsiSignalList[i];
            double prevEtsi = i >= 1 ? etsiList[i - 1] : 0;
            double prevEtsiSignal = i >= 1 ? etsiSignalList[i - 1] : 0;

            var signal = GetCompareSignal(etsi - etsiSignal, prevEtsi - prevEtsiSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Etsi", etsiList },
            { "Signal", etsiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = etsiList;
        stockData.IndicatorName = IndicatorName.ErgodicTrueStrengthIndexV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ergodic True Strength Index V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <param name="length5"></param>
    /// <param name="length6"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateErgodicTrueStrengthIndexV2(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 21, int length2 = 9, int length3 = 9, int length4 = 17, int length5 = 6, int length6 = 2, int signalLength = 2)
    {
        List<double> etsi2List = new();
        List<double> etsi1List = new();
        List<double> priceDiffList = new();
        List<double> absPriceDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double priceDiff = MinPastValues(i, 1, currentValue - prevValue);
            priceDiffList.AddRounded(priceDiff);

            double absPriceDiff = Math.Abs(priceDiff);
            absPriceDiffList.AddRounded(absPriceDiff);
        }

        var diffEma1List = GetMovingAverageList(stockData, maType, length1, priceDiffList);
        var absDiffEma1List = GetMovingAverageList(stockData, maType, length1, absPriceDiffList);
        var diffEma4List = GetMovingAverageList(stockData, maType, length4, priceDiffList);
        var absDiffEma4List = GetMovingAverageList(stockData, maType, length4, absPriceDiffList);
        var diffEma2List = GetMovingAverageList(stockData, maType, length2, diffEma1List);
        var absDiffEma2List = GetMovingAverageList(stockData, maType, length2, absDiffEma1List);
        var diffEma5List = GetMovingAverageList(stockData, maType, length5, diffEma4List);
        var absDiffEma5List = GetMovingAverageList(stockData, maType, length5, absDiffEma4List);
        var diffEma3List = GetMovingAverageList(stockData, maType, length3, diffEma2List);
        var absDiffEma3List = GetMovingAverageList(stockData, maType, length3, absDiffEma2List);
        var diffEma6List = GetMovingAverageList(stockData, maType, length6, diffEma5List);
        var absDiffEma6List = GetMovingAverageList(stockData, maType, length6, absDiffEma5List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double diffEma6 = diffEma6List[i];
            double absDiffEma6 = absDiffEma6List[i];
            double diffEma3 = diffEma3List[i];
            double absDiffEma3 = absDiffEma3List[i];

            double etsi1 = absDiffEma3 != 0 ? MinOrMax(diffEma3 / absDiffEma3 * 100, 100, -100) : 0;
            etsi1List.AddRounded(etsi1);

            double etsi2 = absDiffEma6 != 0 ? MinOrMax(diffEma6 / absDiffEma6 * 100, 100, -100) : 0;
            etsi2List.AddRounded(etsi2);
        }

        var etsi2SignalList = GetMovingAverageList(stockData, maType, signalLength, etsi2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            double etsi2 = etsi2List[i];
            double etsi2Signal = etsi2SignalList[i];
            double prevEtsi2 = i >= 1 ? etsi2List[i - 1] : 0;
            double prevEtsi2Signal = i >= 1 ? etsi2SignalList[i - 1] : 0;

            var signal = GetCompareSignal(etsi2 - etsi2Signal, prevEtsi2 - prevEtsi2Signal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Etsi1", etsi1List },
            { "Etsi2", etsi2List },
            { "Signal", etsi2SignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = etsi2List;
        stockData.IndicatorName = IndicatorName.ErgodicTrueStrengthIndexV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ergodic Commodity Selection Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <param name="pointValue"></param>
    /// <returns></returns>
    public static StockData CalculateErgodicCommoditySelectionIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 32, int smoothLength = 5, double pointValue = 1)
    {
        List<double> ergodicCsiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        double k = 100 * (pointValue / Sqrt(length) / (150 + smoothLength));

        var adxList = CalculateAverageDirectionalIndex(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentValue = inputList[i];
            double adx = adxList[i];
            double prevAdx = i >= 1 ? adxList[i - 1] : 0;
            double adxR = (adx + prevAdx) * 0.5;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            double csi = length + tr > 0 ? k * adxR * tr / length : 0;

            double ergodicCsi = currentValue > 0 ? csi / currentValue : 0;
            ergodicCsiList.AddRounded(ergodicCsi);
        }

        var ergodicCsiSmaList = GetMovingAverageList(stockData, maType, smoothLength, ergodicCsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ergodicCsiSma = ergodicCsiSmaList[i];
            double prevErgodicCsiSma1 = i >= 1 ? ergodicCsiSmaList[i - 1] : 0;
            double prevErgodicCsiSma2 = i >= 2 ? ergodicCsiSmaList[i - 2] : 0;

            var signal = GetCompareSignal(ergodicCsiSma - prevErgodicCsiSma1, prevErgodicCsiSma1 - prevErgodicCsiSma2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ecsi", ergodicCsiList },
            { "Signal", ergodicCsiSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ergodicCsiList;
        stockData.IndicatorName = IndicatorName.ErgodicCommoditySelectionIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Enhanced Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateEnhancedIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14, 
        int signalLength = 8)
    {
        List<double> closewrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        int smaLength = MinOrMax((int)Math.Ceiling((double)length / 2));

        var smaList = GetMovingAverageList(stockData, maType, smaLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double dnm = highest - lowest;
            double sma = smaList[i];

            double closewr = dnm != 0 ? 2 * (currentValue - sma) / dnm : 0;
            closewrList.AddRounded(closewr);
        }

        var closewrSmaList = GetMovingAverageList(stockData, maType, signalLength, closewrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double closewr = closewrList[i];
            double closewrSma = closewrSmaList[i];
            double prevCloseWr = i >= 1 ? closewrList[i - 1] : 0;
            double prevCloseWrSma = i >= 1 ? closewrSmaList[i - 1] : 0;

            var signal = GetCompareSignal(closewr - closewrSma, prevCloseWr - prevCloseWrSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ei", closewrList },
            { "Signal", closewrSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = closewrList;
        stockData.IndicatorName = IndicatorName.EnhancedIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ema Wave Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateEmaWaveIndicator(this StockData stockData, int length1 = 5, int length2 = 25, int length3 = 50, int smoothLength = 4)
    {
        List<double> emaADiffList = new();
        List<double> emaBDiffList = new();
        List<double> emaCDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaAList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length1, inputList);
        var emaBList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length2, inputList);
        var emaCList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double emaA = emaAList[i];
            double emaB = emaBList[i];
            double emaC = emaCList[i];

            double emaADiff = currentValue - emaA;
            emaADiffList.AddRounded(emaADiff);

            double emaBDiff = currentValue - emaB;
            emaBDiffList.AddRounded(emaBDiff);

            double emaCDiff = currentValue - emaC;
            emaCDiffList.AddRounded(emaCDiff);
        }

        var waList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, smoothLength, emaADiffList);
        var wbList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, smoothLength, emaBDiffList);
        var wcList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, smoothLength, emaCDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double wa = waList[i];
            double wb = wbList[i];
            double wc = wcList[i];

            var signal = GetConditionSignal(wa > 0 && wb > 0 && wc > 0, wa < 0 && wb < 0 && wc < 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wa", waList },
            { "Wb", wbList },
            { "Wc", wcList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.EmaWaveIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ergodic Mean Deviation Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateErgodicMeanDeviationIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 32, int length2 = 5, int length3 = 5, int signalLength = 5)
    {
        List<double> ma1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentEma = emaList[i];

            double ma1 = currentValue - currentEma;
            ma1List.AddRounded(ma1);
        }

        var ma1EmaList = GetMovingAverageList(stockData, maType, length2, ma1List);
        var emdiList = GetMovingAverageList(stockData, maType, length3, ma1EmaList);
        var emdiSignalList = GetMovingAverageList(stockData, maType, signalLength, emdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double emdi = emdiList[i];
            double emdiSignal = emdiSignalList[i];
            double prevEmdi = i >= 1 ? emdiList[i - 1] : 0;
            double prevEmdiSignal = i >= 1 ? emdiSignalList[i - 1] : 0;

            var signal = GetCompareSignal(emdi - emdiSignal, prevEmdi - prevEmdiSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emdi", emdiList },
            { "Signal", emdiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emdiList;
        stockData.IndicatorName = IndicatorName.ErgodicMeanDeviationIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Efficient Price
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEfficientPrice(this StockData stockData, int length = 50)
    {
        List<double> epList = new();
        List<double> chgErList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double er = erList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double prevEp1 = i >= 1 ? epList[i - 1] : 0;
            double prevEp2 = i >= 2 ? epList[i - 2] : 0;

            double chgEr = MinPastValues(i, length, currentValue - prevValue) * er;
            chgErList.AddRounded(chgEr);

            double ep = chgErList.Sum();
            epList.AddRounded(ep);

            var signal = GetCompareSignal(ep - prevEp1, prevEp1 - prevEp2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ep", epList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = epList;
        stockData.IndicatorName = IndicatorName.EfficientPrice;

        return stockData;
    }

    /// <summary>
    /// Calculates the Efficient Auto Line
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="fastAlpha"></param>
    /// <param name="slowAlpha"></param>
    /// <returns></returns>
    public static StockData CalculateEfficientAutoLine(this StockData stockData, int length = 19, double fastAlpha = 0.0001m, double slowAlpha = 0.005m)
    {
        List<double> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double er = erList[i];
            double dev = (er * fastAlpha) + ((1 - er) * slowAlpha);

            double prevA = aList.LastOrDefault();
            double a = i < 9 ? currentValue : currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue : prevA;
            aList.AddRounded(a);

            var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eal", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.EfficientAutoLine;

        return stockData;
    }
}