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
    /// <param name="maType">Type of the ma.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateAwesomeOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        InputName inputName = InputName.MedianPrice, int fastLength = 5, int slowLength = 34)
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
    /// Calculates the ulcer index.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateUlcerIndex(this StockData stockData, int length = 14)
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
    public static StockData CalculateIchimokuCloud(this StockData stockData, int tenkanLength = 9, int kijunLength = 26, int senkouLength = 52)
    {
        List<decimal> tenkanSenList = new();
        List<decimal> kijunSenList = new();
        List<decimal> senkouSpanAList = new();
        List<decimal> senkouSpanBList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        var (tenkanHighList, tenkanLowList) = GetMaxAndMinValuesList(highList, lowList, tenkanLength);
        var (kijunHighList, kijunLowList) = GetMaxAndMinValuesList(highList, lowList, kijunLength);
        var (senkouHighList, senkouLowList) = GetMaxAndMinValuesList(highList, lowList, senkouLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest1 = tenkanHighList.ElementAtOrDefault(i);
            decimal lowest1 = tenkanLowList.ElementAtOrDefault(i);
            decimal highest2 = kijunHighList.ElementAtOrDefault(i);
            decimal lowest2 = kijunLowList.ElementAtOrDefault(i);
            decimal highest3 = senkouHighList.ElementAtOrDefault(i);
            decimal lowest3 = senkouLowList.ElementAtOrDefault(i);

            decimal prevTenkanSen = tenkanSenList.LastOrDefault();
            decimal tenkanSen = (highest1 + lowest1) / 2;
            tenkanSenList.Add(tenkanSen);

            decimal prevKijunSen = kijunSenList.LastOrDefault();
            decimal kijunSen = (highest2 + lowest2) / 2;
            kijunSenList.Add(kijunSen);

            decimal senkouSpanA = (tenkanSen + kijunSen) / 2;
            senkouSpanAList.Add(senkouSpanA);

            decimal senkouSpanB = (highest3 + lowest3) / 2;
            senkouSpanBList.Add(senkouSpanB);

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
    /// Calculates the index of the elder ray.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateElderRayIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 13)
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
    /// Calculates the Adaptive Ergodic Candlestick Oscillator
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
        var vwapSdList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

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
    public static StockData CalculateZScore(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> zScorePopulationList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

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
        var varList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

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

        var cciList = CalculateCommodityChannelIndex(stockData, maType: maType, length: slowLength).CustomValuesList;
        var turboCciList = CalculateCommodityChannelIndex(stockData, maType: maType, length: fastLength).CustomValuesList;

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
                (prevHigh5 < prevHigh2 && prevHigh4 < prevHigh2 && prevHigh3 == prevHigh2 && prevHigh1 < prevHigh2) ||
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
        var whiteNoiseStdDevList = CalculateStandardDeviationVolatility(stockData, maType, noiseLength).CustomValuesList;
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

        var stdDeviationList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

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
        var stochasticList = CalculateStochasticOscillator(stockData, length: stochLength, smoothLength1: stochKLength, smoothLength2: stochDLength);
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

            var signal = GetConditionSignal(colSlowL || colFastL, colSlowS || colFastS);
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
            decimal fastF1b = (decimal)1 / (i + 1) * Sin(fastF1x * (i + 1) * Pi);
            fastF1bList.Add(fastF1b);

            decimal fastF1bSum = fastF1bList.TakeLastExt(fastLength).Sum();
            decimal fastF1pol = (fastF1x * fastF1x) + fastF1bSum;
            decimal fastF2x = length != 0 ? (decimal)i / length : 0;
            decimal fastF2b = (decimal)1 / (i + 1) * Sin(fastF2x * (i + 1) * Pi);
            fastF2bList.Add(fastF2b);

            decimal fastF2bSum = fastF2bList.TakeLastExt(fastLength).Sum();
            decimal fastF2pol = (fastF2x * fastF2x) + fastF2bSum;
            decimal fastW = fastF1pol - fastF2pol;
            decimal fastVW = prevValue * fastW;
            fastVWList.Add(fastVW);

            decimal fastVWSum = fastVWList.TakeLastExt(length).Sum();
            decimal slowF1x = length != 0 ? (decimal)(i + 1) / length : 0;
            decimal slowF1b = (decimal)1 / (i + 1) * Sin(slowF1x * (i + 1) * Pi);
            slowF1bList.Add(slowF1b);

            decimal slowF1bSum = slowF1bList.TakeLastExt(slowLength).Sum();
            decimal slowF1pol = (slowF1x * slowF1x) + slowF1bSum;
            decimal slowF2x = length != 0 ? (decimal)i / length : 0;
            decimal slowF2b = (decimal)1 / (i + 1) * Sin(slowF2x * (i + 1) * Pi);
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
        var v4List = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
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

        var cciList = CalculateCommodityChannelIndex(stockData, maType: maType, length: cciLength).CustomValuesList;

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
        var sdList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
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
        var ccDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
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
        var xpAbsStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length3).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal xpAbsAvg = xpAbsAvgList.ElementAtOrDefault(i);
            decimal xpAbsStdDev = xpAbsStdDevList.ElementAtOrDefault(i);
            decimal prevKpoBuffer1 = i >= 1 ? kpoBufferList.ElementAtOrDefault(i - 1) : 0;
            decimal prevKpoBuffer2 = i >= 2 ? kpoBufferList.ElementAtOrDefault(i - 2) : 0;

            decimal tmpVal = xpAbsAvg + (devFactor * xpAbsStdDev);
            decimal maxVal = Math.Max(90, tmpVal);

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
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
        var diffStdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
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

            decimal totalPairs = length * (decimal)(length - 1) / 2;
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
        var hwidthList = CalculateStandardDeviationVolatility(stockData, length: length1).CustomValuesList;
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
            bool dtr = (keepAll2 || prevKeepAll2) && keep23;
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
        List<decimal> cmvCList = new();
        List<decimal> cmvOList = new();
        List<decimal> cmvHList = new();
        List<decimal> cmvLList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, closeList, _) = GetInputValuesList(inputName, stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        var fList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal v = atrList.ElementAtOrDefault(i);
            decimal f = fList.ElementAtOrDefault(i);
            decimal prevCmvc1 = i >= 1 ? cmvCList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCmvc2 = i >= 2 ? cmvCList.ElementAtOrDefault(i - 2) : 0;
            decimal currentClose = closeList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);

            decimal cmvC = v != 0 ? MinOrMax((currentClose - f) / (v * Pow(length, 0.5m)), 1, -1) : 0;
            cmvCList.Add(cmvC);

            decimal cmvO = v != 0 ? MinOrMax((currentOpen - f) / (v * Pow(length, 0.5m)), 1, -1) : 0;
            cmvOList.Add(cmvO);

            decimal cmvH = v != 0 ? MinOrMax((currentHigh - f) / (v * Pow(length, 0.5m)), 1, -1) : 0;
            cmvHList.Add(cmvH);

            decimal cmvL = v != 0 ? MinOrMax((currentLow - f) / (v * Pow(length, 0.5m)), 1, -1) : 0;
            cmvLList.Add(cmvL);

            var signal = GetRsiSignal(cmvC - prevCmvc1, prevCmvc1 - prevCmvc2, cmvC, prevCmvc1, 0.5m, -0.5m);
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
        stockData.CustomValuesList = new List<decimal>();
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
        int length = 14, decimal increment = 1)
    {
        List<decimal> valueList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;

            decimal prevValue = valueList.LastOrDefault();
            decimal value = currentLow >= prevHigh ? prevValue + increment : currentHigh <= prevLow ? prevValue - increment : prevValue;
            valueList.Add(value);
        }

        var valueEmaList = GetMovingAverageList(stockData, maType, length, valueList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal value = valueList.ElementAtOrDefault(i);
            decimal valueEma = valueEmaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? valueList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValueEma = i >= 1 ? valueEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> conHiList = new();
        List<decimal> conLowList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal prevConHi = conHiList.LastOrDefault();
            decimal conHi = i >= 1 ? Math.Max(prevConHi, currentHigh) : currentHigh;
            conHiList.Add(conHi);

            decimal prevConLow = conLowList.LastOrDefault();
            decimal conLow = i >= 1 ? Math.Min(prevConLow, currentLow) : currentLow;
            conLowList.Add(conLow);

            var signal = GetConditionSignal(conHi > prevConHi, conLow < prevConLow);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ch", conHiList },
            { "Cl", conLowList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> emaAngleList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        var emaList = GetMovingAverageList(stockData, maType, length2, closeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal range = highest - lowest != 0 ? 25 / (highest - lowest) * lowest : 0;
            decimal avg = inputList.ElementAtOrDefault(i);
            decimal y = avg != 0 && range != 0 ? (prevEma - ema) / avg * range : 0;
            decimal c = Sqrt(1 + (y * y));
            decimal emaAngle1 = c != 0 ? Math.Round(Acos(1 / c).ToDegrees()) : 0;

            decimal prevEmaAngle = emaAngleList.LastOrDefault();
            decimal emaAngle = y > 0 ? -emaAngle1 : emaAngle1;
            emaAngleList.Add(emaAngle);

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
        List<decimal> aList = new();
        List<decimal> colList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal a = (i + 1) * (priorValue - prevValue);
            aList.Add(a);

            decimal prevCol = colList.LastOrDefault();
            decimal col = aList.TakeLastExt(length).Sum();
            colList.Add(col);

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
        List<decimal> chaikinVolatilityList = new();
        List<decimal> highLowList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal highLow = currentHigh - currentLow;
            highLowList.Add(highLow);
        }

        var highLowEmaList = GetMovingAverageList(stockData, maType, length1, highLowList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highLowEma = highLowEmaList.ElementAtOrDefault(i);
            decimal prevHighLowEma = i >= length2 ? highLowEmaList.ElementAtOrDefault(i - length2) : 0;

            decimal prevChaikinVolatility = chaikinVolatilityList.LastOrDefault();
            decimal chaikinVolatility = prevHighLowEma != 0 ? (highLowEma - prevHighLowEma) / prevHighLowEma * 100 : 0;
            chaikinVolatilityList.Add(chaikinVolatility);

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
        List<decimal> value5List = new();
        List<decimal> value6List = new();
        List<decimal> value7List = new();
        List<decimal> momList = new();
        List<decimal> sumList = new();
        List<decimal> errSumList = new();
        List<decimal> value70List = new();
        List<decimal> confluenceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, closeList, _) = GetInputValuesList(inputName, stockData);

        int stl = (int)Math.Ceiling((length * 2) - 1 - 0.5m);
        int itl = (int)Math.Ceiling((stl * 2) - 1 - 0.5m);
        int ltl = (int)Math.Ceiling((itl * 2) - 1 - 0.5m);
        int hoff = (int)Math.Ceiling(((decimal)length / 2) - 0.5m);
        int soff = (int)Math.Ceiling(((decimal)stl / 2) - 0.5m);
        int ioff = (int)Math.Ceiling(((decimal)itl / 2) - 0.5m);
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
            decimal sAvg = sAvgList.ElementAtOrDefault(i);
            decimal priorSAvg = i >= soff ? sAvgList.ElementAtOrDefault(i - soff) : 0;
            decimal priorHAvg = i >= hoff ? hAvgList.ElementAtOrDefault(i - hoff) : 0;
            decimal iAvg = iAvgList.ElementAtOrDefault(i);
            decimal priorIAvg = i >= ioff ? iAvgList.ElementAtOrDefault(i - ioff) : 0;
            decimal lAvg = lAvgList.ElementAtOrDefault(i);
            decimal hAvg = hAvgList.ElementAtOrDefault(i);
            decimal prevSAvg = i >= 1 ? sAvgList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHAvg = i >= 1 ? hAvgList.ElementAtOrDefault(i - 1) : 0;
            decimal prevIAvg = i >= 1 ? iAvgList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLAvg = i >= 1 ? lAvgList.ElementAtOrDefault(i - 1) : 0;
            decimal h2 = h2AvgList.ElementAtOrDefault(i);
            decimal s2 = s2AvgList.ElementAtOrDefault(i);
            decimal i2 = i2AvgList.ElementAtOrDefault(i);
            decimal l2 = l2AvgList.ElementAtOrDefault(i);
            decimal ftpAvg = ftpAvgList.ElementAtOrDefault(i);
            decimal priorValue5 = i >= hoff ? value5List.ElementAtOrDefault(i - hoff) : 0;
            decimal priorValue6 = i >= soff ? value6List.ElementAtOrDefault(i - soff) : 0;
            decimal priorValue7 = i >= ioff ? value7List.ElementAtOrDefault(i - ioff) : 0;
            decimal priorSum = i >= soff ? sumList.ElementAtOrDefault(i - soff) : 0;
            decimal priorHAvg2 = i >= soff ? hAvgList.ElementAtOrDefault(i - soff) : 0;
            decimal prevErrSum = i >= 1 ? errSumList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMom = i >= 1 ? momList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue70 = i >= 1 ? value70List.ElementAtOrDefault(i - 1) : 0;
            decimal prevConfluence1 = i >= 1 ? confluenceList.ElementAtOrDefault(i - 1) : 0;
            decimal prevConfluence2 = i >= 2 ? confluenceList.ElementAtOrDefault(i - 2) : 0;
            decimal value2 = sAvg - priorHAvg;
            decimal value3 = iAvg - priorSAvg;
            decimal value12 = lAvg - priorIAvg;
            decimal momSig = value2 + value3 + value12;
            decimal derivH = (hAvg * 2) - prevHAvg;
            decimal derivS = (sAvg * 2) - prevSAvg;
            decimal derivI = (iAvg * 2) - prevIAvg;
            decimal derivL = (lAvg * 2) - prevLAvg;
            decimal sumDH = length * derivH;
            decimal sumDS = stl * derivS;
            decimal sumDI = itl * derivI;
            decimal sumDL = ltl * derivL;
            decimal n1h = h2 * hLength;
            decimal n1s = s2 * sLength;
            decimal n1i = i2 * iLength;
            decimal n1l = l2 * lLength;
            decimal drh = sumDH - n1h;
            decimal drs = sumDS - n1s;
            decimal dri = sumDI - n1i;
            decimal drl = sumDL - n1l;
            decimal hSum = h2 * (length - 1);
            decimal sSum = s2 * (stl - 1);
            decimal iSum = i2 * (itl - 1);
            decimal lSum = ftpAvg * (ltl - 1);

            decimal value5 = (hSum + drh) / length;
            value5List.Add(value5);

            decimal value6 = (sSum + drs) / stl;
            value6List.Add(value6);

            decimal value7 = (iSum + dri) / itl;
            value7List.Add(value7);

            decimal value13 = (lSum + drl) / ltl;
            decimal value9 = value6 - priorValue5;
            decimal value10 = value7 - priorValue6;
            decimal value14 = value13 - priorValue7;

            decimal mom = value9 + value10 + value14;
            momList.Add(mom);

            decimal ht = Sin(value5 * 2 * Pi / 360) + Cos(value5 * 2 * Pi / 360);
            decimal hta = Sin(hAvg * 2 * Pi / 360) + Cos(hAvg * 2 * Pi / 360);
            decimal st = Sin(value6 * 2 * Pi / 360) + Cos(value6 * 2 * Pi / 360);
            decimal sta = Sin(sAvg * 2 * Pi / 360) + Cos(sAvg * 2 * Pi / 360);
            decimal it = Sin(value7 * 2 * Pi / 360) + Cos(value7 * 2 * Pi / 360);
            decimal ita = Sin(iAvg * 2 * Pi / 360) + Cos(iAvg * 2 * Pi / 360);

            decimal sum = ht + st + it;
            sumList.Add(sum);

            decimal err = hta + sta + ita;
            decimal cond2 = (sum > priorSum && hAvg < priorHAvg2) || (sum < priorSum && hAvg > priorHAvg2) ? 1 : 0;
            decimal phase = cond2 == 1 ? -1 : 1;

            decimal errSum = (sum - err) * phase;
            errSumList.Add(errSum);

            decimal value70 = value5 - value13;
            value70List.Add(value70);

            decimal errSig = errSumList.TakeLastExt(soff).Average();
            decimal value71 = value70List.TakeLastExt(length).Average();
            decimal errNum = errSum > 0 && errSum < prevErrSum && errSum < errSig ? 1 : errSum > 0 && errSum < prevErrSum && errSum > errSig ? 2 :
                errSum > 0 && errSum > prevErrSum && errSum < errSig ? 2 : errSum > 0 && errSum > prevErrSum && errSum > errSig ? 3 :
                errSum < 0 && errSum > prevErrSum && errSum > errSig ? -1 : errSum < 0 && errSum < prevErrSum && errSum > errSig ? -2 :
                errSum < 0 && errSum > prevErrSum && errSum < errSig ? -2 : errSum < 0 && errSum < prevErrSum && errSum < errSig ? -3 : 0;
            decimal momNum = mom > 0 && mom < prevMom && mom < momSig ? 1 : mom > 0 && mom < prevMom && mom > momSig ? 2 :
                mom > 0 && mom > prevMom && mom < momSig ? 2 : mom > 0 && mom > prevMom && mom > momSig ? 3 :
                mom < 0 && mom > prevMom && mom > momSig ? -1 : mom < 0 && mom < prevMom && mom > momSig ? -2 :
                mom < 0 && mom > prevMom && mom < momSig ? -2 : mom < 0 && mom < prevMom && mom < momSig ? -3 : 0;
            decimal tcNum = value70 > 0 && value70 < prevValue70 && value70 < value71 ? 1 : value70 > 0 && value70 < prevValue70 && value70 > value71 ? 2 :
                value70 > 0 && value70 > prevValue70 && value70 < value71 ? 2 : value70 > 0 && value70 > prevValue70 && value70 > value71 ? 3 :
                value70 < 0 && value70 > prevValue70 && value70 > value71 ? -1 : value70 < 0 && value70 < prevValue70 && value70 > value71 ? -2 :
                value70 < 0 && value70 > prevValue70 && value70 < value71 ? -2 : value70 < 0 && value70 < prevValue70 && value70 < value71 ? -3 : 0;
            decimal value42 = errNum + momNum + tcNum;

            decimal confluence = value42 > 0 && value70 > 0 ? value42 : value42 < 0 && value70 < 0 ? value42 :
                (value42 > 0 && value70 < 0) || (value42 < 0 && value70 > 0) ? value42 / 10 : 0;
            confluenceList.Add(confluence);

            decimal res1 = confluence >= 1 ? confluence : 0;
            decimal res2 = confluence <= -1 ? confluence : 0;
            decimal res3 = confluence == 0 ? 0 : confluence > -1 && confluence < 1 ? 10 * confluence : 0;

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
        List<decimal> rocTotalList = new();
        List<Signal> signalsList = new();

        var roc11List = CalculateRateOfChange(stockData, fastLength).CustomValuesList;
        var roc14List = CalculateRateOfChange(stockData, slowLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentRoc11 = roc11List.ElementAtOrDefault(i);
            decimal currentRoc14 = roc14List.ElementAtOrDefault(i);

            decimal rocTotal = currentRoc11 + currentRoc14;
            rocTotalList.Add(rocTotal);
        }

        var coppockCurveList = GetMovingAverageList(stockData, maType, length, rocTotalList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal coppockCurve = coppockCurveList.ElementAtOrDefault(i);
            decimal prevCoppockCurve = i >= 1 ? coppockCurveList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> sList = new();
        List<decimal> bullSlopeList = new();
        List<decimal> bearSlopeList = new();
        List<Signal> signalsList = new();

        var rsi1List = CalculateRelativeStrengthIndex(stockData, length: length1).CustomValuesList;
        var rsi2List = CalculateRelativeStrengthIndex(stockData, length: smoothLength).CustomValuesList;
        var rsiSmaList = GetMovingAverageList(stockData, maType, smoothLength, rsi2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsiSma = rsiSmaList.ElementAtOrDefault(i);
            decimal rsiDelta = i >= length2 ? rsi1List.ElementAtOrDefault(i - length2) : 0;

            decimal s = rsiDelta + rsiSma;
            sList.Add(s);
        }

        var sFastSmaList = GetMovingAverageList(stockData, maType, fastLength, sList);
        var sSlowSmaList = GetMovingAverageList(stockData, maType, slowLength, sList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal s = sList.ElementAtOrDefault(i);
            decimal sFastSma = sFastSmaList.ElementAtOrDefault(i);
            decimal sSlowSma = sSlowSmaList.ElementAtOrDefault(i);

            decimal prevBullSlope = bullSlopeList.LastOrDefault();
            decimal bullSlope = s - Math.Max(sFastSma, sSlowSma);
            bullSlopeList.Add(bullSlope);

            decimal prevBearSlope = bearSlopeList.LastOrDefault();
            decimal bearSlope = s - Math.Min(sFastSma, sSlowSma);
            bearSlopeList.Add(bearSlope);

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
        int length = 14, decimal pointValue = 50, decimal margin = 3000, decimal commission = 10)
    {
        List<decimal> csiList = new();
        List<decimal> csiSmaList = new();
        List<Signal> signalsList = new();

        decimal k = 100 * (pointValue / Sqrt(margin) / (150 + commission));

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        var adxList = CalculateAverageDirectionalIndex(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal atr = atrList.ElementAtOrDefault(i);
            decimal adxRating = adxList.ElementAtOrDefault(i);

            decimal prevCsi = csiList.LastOrDefault();
            decimal csi = k * atr * adxRating;
            csiList.Add(csi);

            decimal prevCsiSma = csiSmaList.LastOrDefault();
            decimal csiSma = csiList.TakeLastExt(length).Average();
            csiSmaList.Add(csiSma);

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
    /// Calculates the Pivot Detector Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculatePivotDetectorOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length1 = 200, int length2 = 14)
    {
        List<decimal> pdoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length2).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal prevPdo1 = i >= 1 ? pdoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPdo2 = i >= 2 ? pdoList.ElementAtOrDefault(i - 2) : 0;

            decimal pdo = currentValue > sma ? (rsi - 35) / (85 - 35) * 100 : currentValue <= sma ? (rsi - 20) / (70 - 20) * 100 : 0;
            pdoList.Add(pdo);

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
        List<decimal> percentChangeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevPcc = percentChangeList.LastOrDefault();
            decimal pcc = prevValue - 1 != 0 ? prevPcc + (currentValue / (prevValue - 1)) : 0;
            percentChangeList.Add(pcc);
        }

        var pctChgWmaList = GetMovingAverageList(stockData, maType, length, percentChangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pcc = percentChangeList.ElementAtOrDefault(i);
            decimal pccWma = pctChgWmaList.ElementAtOrDefault(i);
            decimal prevPcc = i >= 1 ? percentChangeList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPccWma = i >= 1 ? pctChgWmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> pnoList = new();
        List<decimal> pno1List = new();
        List<decimal> pno2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal ratio = currentValue * length / 100;
            Int64 convertedValue = (Int64)Math.Round(currentValue);
            Int64 sqrtValue = currentValue >= 0 ? (Int64)Math.Round(Sqrt(currentValue)) : 0;
            Int64 maxValue = (Int64)Math.Round(currentValue + ratio);
            Int64 minValue = (Int64)Math.Round(currentValue - ratio);

            decimal pno = 0, pno1 = 0, pno2 = 0;
            for (Int64 j = convertedValue; j <= maxValue; j++)
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
            pno1List.Add(pno1);

            for (Int64 l = convertedValue; l >= minValue; l--)
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
            pno2List.Add(pno2);

            decimal prevPno = pnoList.LastOrDefault();
            pno = pno1 - currentValue < currentValue - pno2 ? pno1 - currentValue : pno2 - currentValue;
            pno = pno == 0 ? prevPno : pno;
            pnoList.Add(pno);

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
        List<decimal> specialKList = new();
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
            decimal roc10Sma = roc10SmaList.ElementAtOrDefault(i);
            decimal roc15Sma = roc15SmaList.ElementAtOrDefault(i);
            decimal roc20Sma = roc20SmaList.ElementAtOrDefault(i);
            decimal roc30Sma = roc30SmaList.ElementAtOrDefault(i);
            decimal roc40Sma = roc40SmaList.ElementAtOrDefault(i);
            decimal roc65Sma = roc65SmaList.ElementAtOrDefault(i);
            decimal roc75Sma = roc75SmaList.ElementAtOrDefault(i);
            decimal roc100Sma = roc100SmaList.ElementAtOrDefault(i);
            decimal roc195Sma = roc195SmaList.ElementAtOrDefault(i);
            decimal roc265Sma = roc265SmaList.ElementAtOrDefault(i);
            decimal roc390Sma = roc390SmaList.ElementAtOrDefault(i);
            decimal roc530Sma = roc530SmaList.ElementAtOrDefault(i);

            decimal specialK = (roc10Sma * 1) + (roc15Sma * 2) + (roc20Sma * 3) + (roc30Sma * 4) + (roc40Sma * 1) + (roc65Sma * 2) + (roc75Sma * 3) +
                (roc100Sma * 4) + (roc195Sma * 1) + (roc265Sma * 2) + (roc390Sma * 3) + (roc530Sma * 4);
            specialKList.Add(specialK);
        }

        var specialKSignalList = GetMovingAverageList(stockData, maType, smoothLength, specialKList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal specialK = specialKList.ElementAtOrDefault(i);
            decimal specialKSignal = specialKSignalList.ElementAtOrDefault(i);
            decimal prevSpecialK = i >= 1 ? specialKList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSpecialKSignal = i >= 1 ? specialKSignalList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> pzoList = new();
        List<decimal> dvolList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal dvol = Math.Sign(currentValue - prevValue) * currentValue;
            dvolList.Add(dvol);
        }

        var dvmaList = GetMovingAverageList(stockData, maType, length, dvolList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal vma = emaList.ElementAtOrDefault(i);
            decimal dvma = dvmaList.ElementAtOrDefault(i);
            decimal prevPzo1 = i >= 1 ? pzoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPzo2 = i >= 2 ? pzoList.ElementAtOrDefault(i - 2) : 0;

            decimal pzo = vma != 0 ? MinOrMax(100 * dvma / vma, 100, -100) : 0;
            pzoList.Add(pzo);

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
        List<decimal> kpiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal prevKpi = kpiList.LastOrDefault();
            decimal kpi = prevValue != 0 ? (currentValue - prevValue) * 100 / prevValue : 0;
            kpiList.Add(kpi);

            var signal = GetCompareSignal(kpi, prevKpi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pi", kpiList }
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
        List<decimal> c2cList = new();
        List<decimal> fracEffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal pfe = Sqrt(Pow(currentValue - priorValue, 2) + 100);

            decimal c2c = Sqrt(Pow(currentValue - prevValue, 2) + 1);
            c2cList.Add(c2c);

            decimal c2cSum = c2cList.TakeLastExt(length).Sum();
            decimal efRatio = c2cSum != 0 ? pfe / c2cSum * 100 : 0;

            decimal fracEff = currentValue - priorValue > 0 ? efRatio : -efRatio;
            fracEffList.Add(fracEff);
        }

        var emaList = GetMovingAverageList(stockData, maType, smoothLength, fracEffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema = emaList.ElementAtOrDefault(i);
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> pgoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal atr = atrList.ElementAtOrDefault(i);

            decimal prevPgo = pgoList.LastOrDefault();
            decimal pgo = atr != 0 ? (currentValue - sma) / atr : 0;
            pgoList.Add(pgo);

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
        List<decimal> pcoList = new();
        List<decimal> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, lowList, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            
            decimal diff = currentClose - currentLow;
            diffList.Add(diff);
        }

        var diffSmaList = GetMovingAverageList(stockData, maType, length, diffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentAtr = atrList.ElementAtOrDefault(i);
            decimal prevPco1 = i >= 1 ? pcoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPco2 = i >= 2 ? pcoList.ElementAtOrDefault(i - 2) : 0;
            decimal diffSma = diffSmaList.ElementAtOrDefault(i);

            decimal pco = currentAtr != 0 ? diffSma / currentAtr * 100 : 0;
            pcoList.Add(pco);

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
        List<decimal> pciList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal mom = currentValue - prevValue;

            decimal positiveSum = 0, negativeSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal prevValue2 = i >= length - j ? inputList.ElementAtOrDefault(i - (length - j)) : 0;
                decimal gradient = prevValue + (mom * (length - j) / (length - 1));
                decimal deviation = prevValue2 - gradient;
                positiveSum = deviation > 0 ? positiveSum + deviation : positiveSum + 0;
                negativeSum = deviation < 0 ? negativeSum - deviation : negativeSum + 0;
            }
            decimal sum = positiveSum + negativeSum;

            decimal pci = sum != 0 ? MinOrMax(100 * positiveSum / sum, 100, 0) : 0;
            pciList.Add(pci);
        }

        var pciSmoothedList = GetMovingAverageList(stockData, maType, smoothLength, pciList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pciSmoothed = pciSmoothedList.ElementAtOrDefault(i);
            decimal prevPciSmoothed1 = i >= 1 ? pciSmoothedList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPciSmoothed2 = i >= 2 ? pciSmoothedList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> sign1List = new();
        List<decimal> sign2List = new();
        List<decimal> sign3List = new();
        List<decimal> absOsList = new();
        List<decimal> osList = new();
        List<decimal> hList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int h = 0; h < stockData.Count; h++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(h);
            decimal sma = smaList.ElementAtOrDefault(h);

            decimal os = currentValue - sma;
            osList.Add(os);

            decimal absOs = Math.Abs(os);
            absOsList.Add(absOs);
        }

        stockData.CustomValuesList = absOsList;
        var pList = CalculateLinearRegression(stockData, smoothLength).CustomValuesList;
        var (highestList, _) = GetMaxAndMinValuesList(pList, length);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal os = osList.ElementAtOrDefault(i);
            decimal p = pList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);

            decimal prevH = i >= 1 ? hList.ElementAtOrDefault(i - 1) : 0;
            decimal h = highest != 0 ? p / highest : 0;
            hList.Add(h);

            decimal mod1 = h == 1 && prevH != 1 ? 1 : 0;
            decimal mod2 = h < 0.8m ? 1 : 0;
            decimal mod3 = prevH == 1 && h < prevH ? 1 : 0;

            decimal sign1 = mod1 == 1 && os < 0 ? 1 : mod1 == 1 && os > 0 ? -1 : 0;
            sign1List.Add(sign1);

            decimal sign2 = mod2 == 1 && os < 0 ? 1 : mod2 == 1 && os > 0 ? -1 : 0;
            sign2List.Add(sign2);

            decimal sign3 = mod3 == 1 && os < 0 ? 1 : mod3 == 1 && os > 0 ? -1 : 0;
            sign3List.Add(sign3);

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
        List<decimal> condList = new();
        List<decimal> psyList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPsy1 = i >= 1 ? psyList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPsy2 = i >= 2 ? psyList.ElementAtOrDefault(i - 2) : 0;

            decimal cond = currentValue > prevValue ? 1 : 0;
            condList.Add(cond);

            decimal condSum = condList.TakeLastExt(length).Sum();
            decimal psy = length != 0 ? condSum / length * 100 : 0;
            psyList.Add(psy);

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
        List<decimal> avgList = new();
        List<decimal> hyList = new();
        List<decimal> ylList = new();
        List<decimal> abList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        var cList = GetMovingAverageList(stockData, maType, smoothLength, inputList);
        var oList = GetMovingAverageList(stockData, maType, smoothLength, openList);
        var hList = GetMovingAverageList(stockData, maType, smoothLength, highList);
        var lList = GetMovingAverageList(stockData, maType, smoothLength, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal c = cList.ElementAtOrDefault(i);
            decimal o = oList.ElementAtOrDefault(i);

            decimal avg = (c + o) / 2;
            avgList.Add(avg);
        }

        var yList = GetMovingAverageList(stockData, maType, length, avgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal y = yList.ElementAtOrDefault(i);
            decimal h = hList.ElementAtOrDefault(i);
            decimal l = lList.ElementAtOrDefault(i);

            decimal hy = h - y;
            hyList.Add(hy);

            decimal yl = y - l;
            ylList.Add(yl);
        }

        var aList = GetMovingAverageList(stockData, maType, length, hyList);
        var bList = GetMovingAverageList(stockData, maType, length, ylList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = aList.ElementAtOrDefault(i);
            decimal b = bList.ElementAtOrDefault(i);

            decimal ab = a - b;
            abList.Add(ab);
        }

        var oscList = GetMovingAverageList(stockData, maType, length, abList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal osc = oscList.ElementAtOrDefault(i);
            decimal prevOsc = i >= 1 ? oscList.ElementAtOrDefault(i - 1) : 0;
            decimal a = aList.ElementAtOrDefault(i);
            decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(osc - a, prevOsc - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BullLine", aList },
            { "Trigger", oscList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> bullCountList = new();
        List<decimal> bearCountList = new();
        List<decimal> totalPowerList = new();
        List<decimal> adjBullCountList = new();
        List<decimal> adjBearCountList = new();
        List<Signal> signalsList = new();

        var elderPowerList = CalculateElderRayIndex(stockData, maType, length2);
        var bullPowerList = elderPowerList.OutputValues["BullPower"];
        var bearPowerList = elderPowerList.OutputValues["BearPower"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bullPower = bullPowerList.ElementAtOrDefault(i);
            decimal bearPower = bearPowerList.ElementAtOrDefault(i);

            decimal bullCount = bullPower > 0 ? 1 : 0;
            bullCountList.Add(bullCount);

            decimal bearCount = bearPower < 0 ? 1 : 0;
            bearCountList.Add(bearCount);

            decimal bullCountSum = bullCountList.TakeLastExt(length1).Sum();
            decimal bearCountSum = bearCountList.TakeLastExt(length1).Sum();

            decimal totalPower = length1 != 0 ? 100 * Math.Abs(bullCountSum - bearCountSum) / length1 : 0;
            totalPowerList.Add(totalPower);

            decimal prevAdjBullCount = adjBullCountList.LastOrDefault();
            decimal adjBullCount = length1 != 0 ? 100 * bullCountSum / length1 : 0;
            adjBullCountList.Add(adjBullCount);

            decimal prevAdjBearCount = adjBearCountList.LastOrDefault();
            decimal adjBearCount = length1 != 0 ? 100 * bearCountSum / length1 : 0;
            adjBearCountList.Add(adjBearCount);

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
        decimal alpha = 0.5m)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> smoList = new();
        List<decimal> smoSmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var sma2List = GetMovingAverageList(stockData, maType, length, smaList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal sma2 = sma2List.ElementAtOrDefault(i);

            decimal smoSma = (alpha * sma) + ((1 - alpha) * sma2);
            smoSmaList.Add(smoSma);

            decimal smo = (alpha * currentValue) + ((1 - alpha) * sma);
            smoList.Add(smo);

            decimal smoSmaHighest = smoSmaList.TakeLastExt(length).Max();
            decimal smoSmaLowest = smoSmaList.TakeLastExt(length).Min();
            decimal smoHighest = smoList.TakeLastExt(length).Max();
            decimal smoLowest = smoList.TakeLastExt(length).Min();

            decimal a = smoHighest - smoLowest != 0 ? (currentValue - smoLowest) / (smoHighest - smoLowest) : 0;
            aList.Add(a);

            decimal b = smoSmaHighest - smoSmaLowest != 0 ? (sma - smoSmaLowest) / (smoSmaHighest - smoSmaLowest) : 0;
            bList.Add(b);
        }

        var aSmaList = GetMovingAverageList(stockData, maType, length, aList);
        var bSmaList = GetMovingAverageList(stockData, maType, length, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = aSmaList.ElementAtOrDefault(i);
            decimal b = bSmaList.ElementAtOrDefault(i);
            decimal prevA = i >= 1 ? aSmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevB = i >= 1 ? bSmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> trList = new();
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
            decimal currentEma200 = ma1List.ElementAtOrDefault(i);
            decimal currentEma50 = ma2List.ElementAtOrDefault(i);
            decimal currentRoc125 = ltRocList.ElementAtOrDefault(i);
            decimal currentRoc20 = mtRocList.ElementAtOrDefault(i);
            decimal currentPpoHistogram = ppoHistList.ElementAtOrDefault(i);
            decimal currentRsi = rsiList.ElementAtOrDefault(i);
            decimal currentPrice = inputList.ElementAtOrDefault(i);
            decimal prevTr1 = i >= 1 ? trList.ElementAtOrDefault(i - 1) : 0;
            decimal prevTr2 = i >= 2 ? trList.ElementAtOrDefault(i - 2) : 0;
            decimal ltMa = currentEma200 != 0 ? 0.3m * 100 * (currentPrice - currentEma200) / currentEma200 : 0;
            decimal ltRoc = 0.3m * 100 * currentRoc125;
            decimal mtMa = currentEma50 != 0 ? 0.15m * 100 * (currentPrice - currentEma50) / currentEma50 : 0;
            decimal mtRoc = 0.15m * 100 * currentRoc20;
            decimal currentValue = currentPpoHistogram;
            decimal prevValue = i >= length8 ? ppoHistList.ElementAtOrDefault(i - length8) : 0;
            decimal slope = length8 != 0 ? (currentValue - prevValue) / length8 : 0;
            decimal stPpo = 0.05m * 100 * slope;
            decimal stRsi = 0.05m * currentRsi;

            decimal tr = Math.Min(100, Math.Max(0, ltMa + ltRoc + mtMa + mtRoc + stPpo + stRsi));
            trList.Add(tr);

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
        List<decimal> uList = new();
        List<decimal> oList = new();
        List<Signal> signalsList = new();

        var sList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal s = sList.ElementAtOrDefault(i);
            decimal prevS = i >= 1 ? sList.ElementAtOrDefault(i - 1) : 0;
            decimal wa = Asin(Math.Sign(s - prevS)) * 2;
            decimal wb = Asin(Math.Sign(1)) * 2;

            decimal u = wa + (2 * Pi * Math.Round((wa - wb) / (2 * Pi)));
            uList.Add(u);
        }

        stockData.CustomValuesList = uList;
        var uLinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal u = uLinregList.ElementAtOrDefault(i);
            decimal prevO1 = i >= 1 ? oList.ElementAtOrDefault(i - 1) : 0;
            decimal prevO2 = i >= 2 ? oList.ElementAtOrDefault(i - 2) : 0;

            decimal o = Atan(u);
            oList.Add(o);

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
        decimal threshold = 50, decimal limit = 0)
    {
        List<decimal> bufHistNoList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length1).CustomValuesList;
        var stochastic1List = CalculateStochasticOscillator(stockData, maType, length: length2, smoothLength, smoothLength).OutputValues["FastD"];
        var stochastic2List = CalculateStochasticOscillator(stockData, maType, length: length1, smoothLength, smoothLength).OutputValues["FastD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal stoch1 = stochastic1List.ElementAtOrDefault(i);
            decimal stoch2 = stochastic2List.ElementAtOrDefault(i);
            decimal bufRsi = rsi - threshold;
            decimal bufStoch1 = stoch1 - threshold;
            decimal bufStoch2 = stoch2 - threshold;
            decimal bufHistUp = bufRsi > limit && bufStoch1 > limit && bufStoch2 > limit ? bufStoch2 : 0;
            decimal bufHistDn = bufRsi < limit && bufStoch1 < limit && bufStoch2 < limit ? bufStoch2 : 0;

            decimal prevBufHistNo = bufHistNoList.LastOrDefault();
            decimal bufHistNo = bufHistUp - bufHistDn;
            bufHistNoList.Add(bufHistNo);

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
        List<decimal> upList = new();
        List<decimal> dnList = new();
        List<decimal> midList = new();
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
            decimal rsiSma = maList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal mab = mabList.ElementAtOrDefault(i);
            decimal mbb = mbbList.ElementAtOrDefault(i);
            decimal prevMab = i >= 1 ? mabList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMbb = i >= 1 ? mbbList.ElementAtOrDefault(i - 1) : 0;
            decimal offs = 1.6185m * stdDev;

            decimal prevUp = upList.LastOrDefault();
            decimal up = rsiSma + offs;
            upList.Add(up);

            decimal prevDn = dnList.LastOrDefault();
            decimal dn = rsiSma - offs;
            dnList.Add(dn);

            decimal mid = (up + dn) / 2;
            midList.Add(mid);

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
        List<decimal> bList = new();
        List<decimal> cList = new();
        List<decimal> upList = new();
        List<decimal> dnList = new();
        List<decimal> osList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = emaList.ElementAtOrDefault(i);
            decimal prevA = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

            decimal b = a > prevA ? a : 0;
            bList.Add(b);

            decimal c = a < prevA ? a : 0;
            cList.Add(c);
        }

        stockData.CustomValuesList = bList;
        var bStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = cList;
        var cStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = emaList.ElementAtOrDefault(i);
            decimal b = bStdDevList.ElementAtOrDefault(i);
            decimal c = cStdDevList.ElementAtOrDefault(i);

            decimal prevUp = upList.LastOrDefault();
            decimal up = a + b != 0 ? a / (a + b) : 0;
            upList.Add(up);

            decimal prevDn = dnList.LastOrDefault();
            decimal dn = a + c != 0 ? a / (a + c) : 0;
            dnList.Add(dn);

            decimal os = prevUp == 1 && up != 1 ? 1 : prevDn == 1 && dn != 1 ? -1 : 0;
            osList.Add(os);

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
        List<decimal> bullsList = new();
        List<decimal> bearsList = new();
        List<decimal> netList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal hiup = Math.Max(high - prevHigh, 0);
            decimal loup = Math.Max(low - prevLow, 0);
            decimal hidn = Math.Min(high - prevHigh, 0);
            decimal lodn = Math.Min(low - prevLow, 0);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal range = highest - lowest;

            decimal bulls = range != 0 ? Math.Min((hiup + loup) / range, 1) * 100 : 0;
            bullsList.Add(bulls);

            decimal bears = range != 0 ? Math.Max((hidn + lodn) / range, -1) * -100 : 0;
            bearsList.Add(bears);
        }

        var avgBullsList = GetMovingAverageList(stockData, maType, length1, bullsList);
        var avgBearsList = GetMovingAverageList(stockData, maType, length1, bearsList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal avgBulls = avgBullsList.ElementAtOrDefault(i);
            decimal avgBears = avgBearsList.ElementAtOrDefault(i);

            decimal net = avgBulls - avgBears;
            netList.Add(net);
        }

        var tpxList = GetMovingAverageList(stockData, maType, smoothLength, netList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tpx = tpxList.ElementAtOrDefault(i);
            decimal prevTpx = i >= 1 ? tpxList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> maRatingList = new();
        List<decimal> oscRatingList = new();
        List<decimal> totalRatingList = new();
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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal prevRsi = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
            decimal ma10 = ma10List.ElementAtOrDefault(i);
            decimal ma20 = ma20List.ElementAtOrDefault(i);
            decimal ma30 = ma30List.ElementAtOrDefault(i);
            decimal ma50 = ma50List.ElementAtOrDefault(i);
            decimal ma100 = ma100List.ElementAtOrDefault(i);
            decimal ma200 = ma200List.ElementAtOrDefault(i);
            decimal hullMa = hullMaList.ElementAtOrDefault(i);
            decimal vwma = vwmaList.ElementAtOrDefault(i);
            decimal conLine = tenkanList.ElementAtOrDefault(i);
            decimal baseLine = kijunList.ElementAtOrDefault(i);
            decimal leadLine1 = senkouAList.ElementAtOrDefault(i);
            decimal leadLine2 = senkouBList.ElementAtOrDefault(i);
            decimal kSto = stoKList.ElementAtOrDefault(i);
            decimal prevKSto = i >= 1 ? stoKList.ElementAtOrDefault(i - 1) : 0;
            decimal dSto = stoDList.ElementAtOrDefault(i);
            decimal prevDSto = i >= 1 ? stoDList.ElementAtOrDefault(i - 1) : 0;
            decimal cci = cciList.ElementAtOrDefault(i);
            decimal prevCci = i >= 1 ? cciList.ElementAtOrDefault(i - 1) : 0;
            decimal adx = adxList.ElementAtOrDefault(i);
            decimal adxPlus = adxPlusList.ElementAtOrDefault(i);
            decimal prevAdxPlus = i >= 1 ? adxPlusList.ElementAtOrDefault(i - 1) : 0;
            decimal adxMinus = adxMinusList.ElementAtOrDefault(i);
            decimal prevAdxMinus = i >= 1 ? adxMinusList.ElementAtOrDefault(i - 1) : 0;
            decimal ao = aoList.ElementAtOrDefault(i);
            decimal prevAo1 = i >= 1 ? aoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevAo2 = i >= 2 ? aoList.ElementAtOrDefault(i - 2) : 0;
            decimal mom = momentumList.ElementAtOrDefault(i);
            decimal prevMom = i >= 1 ? momentumList.ElementAtOrDefault(i - 1) : 0;
            decimal macd = macdList.ElementAtOrDefault(i);
            decimal macdSig = macdSignalList.ElementAtOrDefault(i);
            decimal kStoRsi = stoRsiKList.ElementAtOrDefault(i);
            decimal prevKStoRsi = i >= 1 ? stoRsiKList.ElementAtOrDefault(i - 1) : 0;
            decimal dStoRsi = stoRsiDList.ElementAtOrDefault(i);
            decimal prevDStoRsi = i >= 1 ? stoRsiDList.ElementAtOrDefault(i - 1) : 0;
            bool upTrend = currentValue > ma50;
            bool dnTrend = currentValue < ma50;
            decimal wr = williamsPctList.ElementAtOrDefault(i);
            decimal prevWr = i >= 1 ? williamsPctList.ElementAtOrDefault(i - 1) : 0;
            decimal bullPower = bullPowerList.ElementAtOrDefault(i);
            decimal prevBullPower = i >= 1 ? bullPowerList.ElementAtOrDefault(i - 1) : 0;
            decimal bearPower = bearPowerList.ElementAtOrDefault(i);
            decimal prevBearPower = i >= 1 ? bearPowerList.ElementAtOrDefault(i - 1) : 0;
            decimal uo = uoList.ElementAtOrDefault(i);

            decimal maRating = 0;
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
            maRatingList.Add(maRating);

            decimal oscRating = 0;
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
            oscRatingList.Add(oscRating);

            decimal totalRating = (maRating + oscRating) / 2;
            totalRatingList.Add(totalRating);

            var signal = GetConditionSignal(totalRating > 0.1m, totalRating < -0.1m);
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
        List<decimal> buySellSwitchList = new();
        List<decimal> sbsList = new();
        List<decimal> clrsList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal prevClose1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevClose2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevClose3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);
            decimal triggerSell = prevClose1 < close && (prevClose2 < prevClose1 || prevClose3 < prevClose1) ? 1 : 0;
            decimal triggerBuy = prevClose1 > close && (prevClose2 > prevClose1 || prevClose3 > prevClose1) ? 1 : 0;

            decimal prevBuySellSwitch = buySellSwitchList.LastOrDefault();
            decimal buySellSwitch = triggerSell == 1 ? 1 : triggerBuy == 1 ? 0 : prevBuySellSwitch;
            buySellSwitchList.Add(buySellSwitch);

            decimal prevSbs = sbsList.LastOrDefault();
            decimal sbs = triggerSell == 1 && prevBuySellSwitch == 0 ? high : triggerBuy == 1 && prevBuySellSwitch == 1 ? low : prevSbs;
            sbsList.Add(sbs);

            decimal prevClrs = clrsList.LastOrDefault();
            decimal clrs = triggerSell == 1 && prevBuySellSwitch == 0 ? 1 : triggerBuy == 1 && prevBuySellSwitch == 1 ? -1 : prevClrs;
            clrsList.Add(clrs);

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
        List<decimal> tetherLineList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevTetherLine = tetherLineList.LastOrDefault();
            decimal tetherLine = (highest + lowest) / 2;
            tetherLineList.Add(tetherLine);

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
        List<decimal> v1List = new();
        List<decimal> stochList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

            decimal v1 = currentValue > prevValue ? tr / (currentValue - prevValue) : tr;
            v1List.Add(v1);

            var lbList = v1List.TakeLastExt(length).ToList();
            decimal v2 = lbList.Min();
            decimal v3 = lbList.Max();

            decimal stoch = v3 - v2 != 0 ? MinOrMax(100 * (v1 - v2) / (v3 - v2), 100, 0) : MinOrMax(100 * (v1 - v2), 100, 0);
            stochList.Add(stoch);
        }

        var triList = GetMovingAverageList(stockData, maType, length, stochList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tri = triList.ElementAtOrDefault(i);
            decimal prevTri1 = i >= 1 ? triList.ElementAtOrDefault(i - 1) : 0;
            decimal prevTri2 = i >= 2 ? triList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<decimal> risingList = new();
        List<decimal> fallingList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal highest = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal lowest = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;

            decimal rising = currentHigh > highest ? 1 : 0;
            risingList.Add(rising);

            decimal falling = currentLow < lowest ? 1 : 0;
            fallingList.Add(falling);

            decimal a = i - risingList.LastIndexOf(1);
            decimal b = i - fallingList.LastIndexOf(1);

            decimal prevUpper = upperList.LastOrDefault();
            decimal upper = length != 0 ? ((a > length ? length : a) / length) - 0.5m : 0;
            upperList.Add(upper);

            decimal prevLower = lowerList.LastOrDefault();
            decimal lower = length != 0 ? ((b > length ? length : b) / length) - 0.5m : 0;
            lowerList.Add(lower);

            var signal = GetCompareSignal((lower * -1) - (upper * -1), (prevLower * -1) - (prevUpper * -1));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> swingTrd1List = new();
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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
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

            decimal swingTrd1 = highest - lowest != 0 ? 100 * (currentValue - ((r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8 + r9 + r10) / 10)) / 
                (highest - lowest) : 0;
            swingTrd1List.Add(swingTrd1);
        }

        var swingTrd2List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length3, swingTrd1List);
        var swingTrd3List = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length3, swingTrd2List);
        var rmoList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length4, swingTrd3List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rmo = rmoList.ElementAtOrDefault(i);
            decimal prevRmo = i >= 1 ? rmoList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(rmo, prevRmo);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rmo", rmoList }
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
        List<decimal> rainbowOscillatorList = new();
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
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
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
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
            decimal highestRainbow = Math.Max(r1, Math.Max(r2, Math.Max(r3, Math.Max(r4, Math.Max(r5, Math.Max(r6, Math.Max(r7, Math.Max(r8, 
                Math.Max(r9, r10)))))))));
            decimal lowestRainbow = Math.Min(r1, Math.Min(r2, Math.Min(r3, Math.Min(r4, Math.Min(r5, Math.Min(r6, Math.Min(r7, Math.Min(r8, 
                Math.Min(r9, r10)))))))));

            decimal prevRainbowOscillator = rainbowOscillatorList.LastOrDefault();
            decimal rainbowOscillator = highest - lowest != 0 ? 100 * ((currentValue - ((r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8 + r9 + r10) / 10)) / 
                (highest - lowest)) : 0;
            rainbowOscillatorList.Add(rainbowOscillator);

            decimal upperBand = highest - lowest != 0 ? 100 * ((highestRainbow - lowestRainbow) / (highest - lowest)) : 0;
            upperBandList.Add(upperBand);

            decimal lowerBand = -upperBand;
            lowerBandList.Add(lowerBand);

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
        List<decimal> rwiLowList = new();
        List<decimal> rwiHighList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        decimal sqrt = Sqrt(length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentAtr = atrList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= length ? highList.ElementAtOrDefault(i - length) : 0;
            decimal prevLow = i >= length ? lowList.ElementAtOrDefault(i - length) : 0;
            decimal bottom = currentAtr * sqrt;

            decimal prevRwiLow = rwiLowList.LastOrDefault();
            decimal rwiLow = bottom != 0 ? (prevHigh - currentLow) / bottom : 0;
            rwiLowList.Add(rwiLow);

            decimal prevRwiHigh = rwiHighList.LastOrDefault();
            decimal rwiHigh = bottom != 0 ? (currentHigh - prevLow) / bottom : 0;
            rwiHighList.Add(rwiHigh);

            var signal = GetCompareSignal(rwiHigh - rwiLow, prevRwiHigh - prevRwiLow);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "RwiHigh", rwiHighList },
            { "RwiLow", rwiLowList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> raviList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaFastList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var smaSlowList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastMA = smaFastList.ElementAtOrDefault(i);
            decimal slowMA = smaSlowList.ElementAtOrDefault(i);
            decimal prevRavi1 = i >= 1 ? raviList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRavi2 = i >= 2 ? raviList.ElementAtOrDefault(i - 2) : 0;

            decimal ravi = slowMA != 0 ? (fastMA - slowMA) / slowMA * 100 : 0;
            raviList.Add(ravi);

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
        List<decimal> rsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, lowList, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentMa = maList.ElementAtOrDefault(i);

            decimal rsi = currentValue != 0 ? (currentLow - currentMa) / currentValue * 100 : 0;
            rsiList.Add(rsi);
        }

        var rsiMaList = GetMovingAverageList(stockData, maType, smoothLength, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiMaList.ElementAtOrDefault(i);
            decimal prevRsiMa = i >= 1 ? rsiMaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRsi = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
            decimal rsiMa = rsiMaList.ElementAtOrDefault(i);

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
        int length = 14, decimal alpha = 0.6m)
    {
        List<decimal> bList = new();
        List<decimal> bChgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);
        stockData.CustomValuesList = emaList;
        var rsiList = CalculateRelativeStrengthIndex(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList.ElementAtOrDefault(i);
            decimal priorB = i >= length ? bList.ElementAtOrDefault(i - length) : 0;
            decimal a = rsi / 100;
            decimal prevBChg1 = i >= 1 ? bChgList.ElementAtOrDefault(i - 1) : a;
            decimal prevBChg2 = i >= 2 ? bChgList.ElementAtOrDefault(i - 2) : 0;

            decimal b = (alpha * a) + ((1 - alpha) * prevBChg1);
            bList.Add(b);

            decimal bChg = b - priorB;
            bChgList.Add(bChg);

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
        List<decimal> roscList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentLinReg = linRegList.ElementAtOrDefault(i);

            decimal prevRosc = roscList.LastOrDefault();
            decimal rosc = currentLinReg != 0 ? 100 * ((currentValue / currentLinReg) - 1) : 0;
            roscList.Add(rosc);

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
        List<decimal> rdosList = new();
        List<decimal> aList = new();
        List<decimal> dList = new();
        List<decimal> nList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal a = currentValue > prevValue ? 1 : 0;
            aList.Add(a);

            decimal d = currentValue < prevValue ? 1 : 0;
            dList.Add(d);

            decimal n = currentValue == prevValue ? 1 : 0;
            nList.Add(n);

            decimal prevRdos = rdosList.LastOrDefault();
            decimal aSum = aList.TakeLastExt(length).Sum();
            decimal dSum = dList.TakeLastExt(length).Sum();
            decimal nSum = nList.TakeLastExt(length).Sum();
            decimal rdos = aSum > 0 || dSum > 0 || nSum > 0 ? (Pow(aSum, 2) - Pow(dSum, 2)) / Pow(aSum + nSum + dSum, 2) : 0;
            rdosList.Add(rdos);

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
        List<decimal> spreadList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastEma = fastEmaList.ElementAtOrDefault(i);
            decimal slowEma = slowEmaList.ElementAtOrDefault(i);

            decimal spread = fastEma - slowEma;
            spreadList.Add(spread);
        }

        stockData.CustomValuesList = spreadList;
        var rsList = CalculateRelativeStrengthIndex(stockData, length: length).CustomValuesList;
        var rssList = GetMovingAverageList(stockData, maType, smoothLength, rsList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rss = rssList.ElementAtOrDefault(i);
            decimal prevRss1 = i >= 1 ? rssList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRss2 = i >= 2 ? rssList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> r1List = new();
        List<decimal> rs3List = new();
        List<decimal> rs2List = new();
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (spInputList, _, _, _, _) = GetInputValuesList(marketData);

        if (stockData.Count == marketData.Count)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentSp = spInputList.ElementAtOrDefault(i);

                decimal prevR1 = r1List.LastOrDefault();
                decimal r1 = currentSp != 0 ? currentValue / currentSp * 100 : prevR1;
                r1List.Add(r1);
            }

            var fastMaList = GetMovingAverageList(stockData, maType, length3, r1List);
            var medMaList = GetMovingAverageList(stockData, maType, length2, fastMaList);
            var slowMaList = GetMovingAverageList(stockData, maType, length4, fastMaList);
            var vSlowMaList = GetMovingAverageList(stockData, maType, length5, slowMaList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal fastMa = fastMaList.ElementAtOrDefault(i);
                decimal medMa = medMaList.ElementAtOrDefault(i);
                decimal slowMa = slowMaList.ElementAtOrDefault(i);
                decimal vSlowMa = vSlowMaList.ElementAtOrDefault(i);
                decimal t1 = fastMa >= medMa && medMa >= slowMa && slowMa >= vSlowMa ? 10 : 0;
                decimal t2 = fastMa >= medMa && medMa >= slowMa && slowMa < vSlowMa ? 9 : 0;
                decimal t3 = fastMa < medMa && medMa >= slowMa && slowMa >= vSlowMa ? 9 : 0;
                decimal t4 = fastMa < medMa && medMa >= slowMa && slowMa < vSlowMa ? 5 : 0;

                decimal rs2 = t1 + t2 + t3 + t4;
                rs2List.Add(rs2);
            }

            var rs2MaList = GetMovingAverageList(stockData, maType, length1, rs2List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal rs2 = rs2List.ElementAtOrDefault(i);
                decimal rs2Ma = rs2MaList.ElementAtOrDefault(i);
                decimal prevRs3_1 = i >= 1 ? rs3List.ElementAtOrDefault(i - 1) : 0;
                decimal prevRs3_2 = i >= 2 ? rs3List.ElementAtOrDefault(i - 1) : 0;

                decimal x = rs2 >= 5 ? 1 : 0;
                xList.Add(x);

                decimal rs3 = rs2 >= 5 || rs2 > rs2Ma ? xList.TakeLastExt(length4).Sum() / length4 * 100 : 0;
                rs3List.Add(rs3);

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
        List<decimal> rviList = new();
        List<decimal> numeratorList = new();
        List<decimal> denominatorList = new();
        List<decimal> signalLineList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevOpen1 = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;
            decimal prevClose1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh1 = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOpen2 = i >= 2 ? openList.ElementAtOrDefault(i - 2) : 0;
            decimal prevClose2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHigh2 = i >= 2 ? highList.ElementAtOrDefault(i - 2) : 0;
            decimal prevOpen3 = i >= 3 ? openList.ElementAtOrDefault(i - 3) : 0;
            decimal prevClose3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            decimal prevHigh3 = i >= 3 ? highList.ElementAtOrDefault(i - 3) : 0;
            decimal a = currentClose - currentOpen;
            decimal b = prevClose1 - prevOpen1;
            decimal c = prevClose2 - prevOpen2;
            decimal d = prevClose3 - prevOpen3;
            decimal e = currentHigh - currentLow;
            decimal f = prevHigh1 - prevOpen1;
            decimal g = prevHigh2 - prevOpen2;
            decimal h = prevHigh3 - prevOpen3;

            decimal numerator = (a + (2 * b) + (2 * c) + d) / 6;
            numeratorList.Add(numerator);

            decimal denominator = (e + (2 * f) + (2 * g) + h) / 6;
            denominatorList.Add(denominator);
        }

        var numeratorAvgList = GetMovingAverageList(stockData, maType, length, numeratorList);
        var denominatorAvgList = GetMovingAverageList(stockData, maType, length, denominatorList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal numeratorAvg = numeratorAvgList.ElementAtOrDefault(i);
            decimal denominatorAvg = denominatorAvgList.ElementAtOrDefault(i);
            decimal k = i >= 1 ? rviList.ElementAtOrDefault(i - 1) : 0;
            decimal l = i >= 2 ? rviList.ElementAtOrDefault(i - 2) : 0;
            decimal m = i >= 3 ? rviList.ElementAtOrDefault(i - 3) : 0;

            decimal rvi = denominatorAvg != 0 ? numeratorAvg / denominatorAvg : 0;
            rviList.Add(rvi);

            decimal prevSignalLine = signalLineList.LastOrDefault();
            decimal signalLine = (rvi + (2 * k) + (2 * l) + m) / 6;
            signalLineList.Add(signalLine);

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
        List<decimal> bullPowerList = new();
        List<decimal> bearPowerList = new();
        List<decimal> repulseList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;

            decimal bullPower = currentClose != 0 ? 100 * ((3 * currentClose) - (2 * lowestLow) - prevOpen) / currentClose : 0;
            bullPowerList.Add(bullPower);

            decimal bearPower = currentClose != 0 ? 100 * (prevOpen + (2 * highestHigh) - (3 * currentClose)) / currentClose : 0;
            bearPowerList.Add(bearPower);
        }

        var bullPowerEmaList = GetMovingAverageList(stockData, maType, length * 5, bullPowerList);
        var bearPowerEmaList = GetMovingAverageList(stockData, maType, length * 5, bearPowerList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bullPowerEma = bullPowerEmaList.ElementAtOrDefault(i);
            decimal bearPowerEma = bearPowerEmaList.ElementAtOrDefault(i);

            decimal repulse = bullPowerEma - bearPowerEma;
            repulseList.Add(repulse);
        }

        var repulseEmaList = GetMovingAverageList(stockData, maType, length, repulseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal repulse = repulseList.ElementAtOrDefault(i);
            decimal prevRepulse = i >= 1 ? repulseList.ElementAtOrDefault(i - 1) : 0;
            decimal repulseEma = repulseEmaList.ElementAtOrDefault(i);
            decimal prevRepulseEma = i >= 1 ? repulseEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> absChgList = new();
        List<decimal> kList = new();
        List<decimal> cList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal prevK1 = i >= 1 ? kList.ElementAtOrDefault(i - 1) : 0;
            decimal prevK2 = i >= 2 ? kList.ElementAtOrDefault(i - 2) : 0;

            decimal absChg = Math.Abs(currentClose - prevClose);
            absChgList.Add(absChg);

            var lbList = absChgList.TakeLastExt(length).ToList();
            decimal highest = lbList.Max();
            decimal lowest = lbList.Min();
            decimal s = highest - lowest != 0 ? (absChg - lowest) / (highest - lowest) * 100 : 0;
            decimal weight = s / 100;

            decimal prevC = i >= 1 ? cList.ElementAtOrDefault(i - 1) : currentClose;
            decimal c = (weight * currentClose) + ((1 - weight) * prevC);
            cList.Add(c);

            decimal prevH = i >= 1 ? prevC : currentHigh;
            decimal h = (weight * currentHigh) + ((1 - weight) * prevH);
            decimal prevL = i >= 1 ? prevC : currentLow;
            decimal l = (weight * currentLow) + ((1 - weight) * prevL);
            decimal prevO = i >= 1 ? prevC : currentOpen;
            decimal o = (weight * currentOpen) + ((1 - weight) * prevO);

            decimal k = (c + h + l + o) / 4;
            kList.Add(k);

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
        List<decimal> tvbList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList.ElementAtOrDefault(i);
            decimal open = openList.ElementAtOrDefault(i);
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);

            decimal tvb = (3 * close) - (low + open + high);
            tvbList.Add(tvb);
        }

        var roList = GetMovingAverageList(stockData, maType, length, tvbList);
        var roEmaList = GetMovingAverageList(stockData, maType, length, roList);
        for (int j = 0; j < stockData.Count; j++)
        {
            decimal ro = roList.ElementAtOrDefault(j);
            decimal roEma = roEmaList.ElementAtOrDefault(j);
            decimal prevRo = j >= 1 ? roList.ElementAtOrDefault(j - 1) : 0;
            decimal prevRoEma = j >= 1 ? roEmaList.ElementAtOrDefault(j - 1) : 0;

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
        List<decimal> indexList = new();
        List<decimal> tempList = new();
        List<decimal> corrList = new();
        List<decimal> lList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal index = i;
            indexList.Add(index);

            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.Add((decimal)corr);
        }

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal indexStdDev = indexStdDevList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal indexSma = indexSmaList.ElementAtOrDefault(i);
            decimal a = indexStdDev != 0 ? corr * (stdDev / indexStdDev) : 0;
            decimal b = sma - (a * indexSma);

            decimal l = currentValue - a - (b * currentValue);
            lList.Add(l);
        }

        var lSmaList = GetMovingAverageList(stockData, maType, length, lList);
        for (int j = 0; j < stockData.Count; j++)
        {
            decimal l = lSmaList.ElementAtOrDefault(j);
            decimal prevL1 = j >= 1 ? lSmaList.ElementAtOrDefault(j - 1) : 0;
            decimal prevL2 = j >= 2 ? lSmaList.ElementAtOrDefault(j - 2) : 0;

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
        List<decimal> rsingList = new();
        List<decimal> upList = new();
        List<decimal> dnList = new();
        List<decimal> rangeList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal high = highList.ElementAtOrDefault(i);
            decimal low = lowList.ElementAtOrDefault(i);

            decimal range = high - low;
            rangeList.Add(range);
        }

        stockData.CustomValuesList = rangeList;
        var stdevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal ma = maList.ElementAtOrDefault(i);
            decimal stdev = stdevList.ElementAtOrDefault(i);
            decimal range = rangeList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal vwr = ma != 0 ? currentVolume / ma : 0;
            decimal blr = stdev != 0 ? range / stdev : 0;
            bool isUp = currentValue > prevValue;
            bool isDn = currentValue < prevValue;
            bool isEq = currentValue == prevValue;

            decimal prevUpCount = upList.LastOrDefault();
            decimal upCount = isEq ? 0 : isUp ? (prevUpCount <= 0 ? 1 : prevUpCount + 1) : (prevUpCount >= 0 ? -1 : prevUpCount - 1);
            upList.Add(upCount);

            decimal prevDnCount = dnList.LastOrDefault();
            decimal dnCount = isEq ? 0 : isDn ? (prevDnCount <= 0 ? 1 : prevDnCount + 1) : (prevDnCount >= 0 ? -1 : prevDnCount - 1);
            dnList.Add(dnCount);

            decimal pmo = currentValue - prevValue;
            decimal rsing = vwr * blr * pmo;
            rsingList.Add(rsing);
        }

        var rsingMaList = GetMovingAverageList(stockData, maType, length, rsingList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsing = rsingMaList.ElementAtOrDefault(i);
            decimal prevRsing1 = i >= 1 ? rsingMaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRsing2 = i >= 2 ? rsingMaList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> rsmkList = new();
        List<decimal> logRatioList = new();
        List<decimal> logDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (spInputList, _, _, _, _) = GetInputValuesList(marketData);

        if (stockData.Count == marketData.Count)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal spValue = spInputList.ElementAtOrDefault(i);
                decimal prevLogRatio = i >= length ? logRatioList.ElementAtOrDefault(i - length) : 0;

                decimal logRatio = spValue != 0 ? currentValue / spValue : 0;
                logRatioList.Add(logRatio);

                decimal logDiff = logRatio - prevLogRatio;
                logDiffList.Add(logDiff);
            }

            var logDiffEmaList = GetMovingAverageList(stockData, maType, smoothLength, logDiffList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal logDiffEma = logDiffEmaList.ElementAtOrDefault(i);

                decimal prevRsmk = rsmkList.LastOrDefault();
                decimal rsmk = logDiffEma * 100;
                rsmkList.Add(rsmk);

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
        List<decimal> chgXList = new();
        List<decimal> reqList = new();
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal sma = smaList.ElementAtOrDefault(i);

            decimal prevX = xList.LastOrDefault();
            decimal x = Math.Sign(currentValue - sma);
            xList.Add(x);

            decimal chgX = (currentValue - prevValue) * prevX;
            chgXList.Add(chgX);

            decimal prevReq = reqList.LastOrDefault();
            decimal req = chgXList.TakeLastExt(length).Sum();
            reqList.Add(req);

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
        List<decimal> highLowList = new();
        List<decimal> ratioList = new();
        List<decimal> massIndexList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal highLow = currentHigh - currentLow;
            highLowList.Add(highLow);
        }

        var firstEmaList = GetMovingAverageList(stockData, maType, length1, highLowList);
        var secondEmaList = GetMovingAverageList(stockData, maType, length2, firstEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal firstEma = firstEmaList.ElementAtOrDefault(i);
            decimal secondEma = secondEmaList.ElementAtOrDefault(i);

            decimal ratio = secondEma != 0 ? firstEma / secondEma : 0;
            ratioList.Add(ratio);

            decimal massIndex = ratioList.TakeLastExt(length3).Sum();
            massIndexList.Add(massIndex);
        }

        var massIndexSignalList = GetMovingAverageList(stockData, maType, signalLength, massIndexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal massIndex = massIndexList.ElementAtOrDefault(i);
            decimal massIndexEma = massIndexSignalList.ElementAtOrDefault(i);
            decimal prevMassIndex = i >= 1 ? massIndexList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMassIndexEma = i >= 1 ? massIndexSignalList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> topList = new();
        List<decimal> botList = new();
        List<decimal> mtoList = new();
        List<decimal> advList = new();
        List<decimal> decList = new();
        List<decimal> advVolList = new();
        List<decimal> decVolList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentVolume = volumeList.ElementAtOrDefault(i);

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

            decimal top = (advSum * advVolSum) - (decSum * decVolSum);
            topList.Add(top);

            decimal bot = (advSum * advVolSum) + (decSum * decVolSum);
            botList.Add(bot);

            decimal mto = bot != 0 ? 100 * top / bot : 0;
            mtoList.Add(mto);
        }

        var mtoEmaList = GetMovingAverageList(stockData, maType, length, mtoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mto = mtoList.ElementAtOrDefault(i);
            decimal mtoEma = mtoEmaList.ElementAtOrDefault(i);
            decimal prevMto = i >= 1 ? mtoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMtoEma = i >= 1 ? mtoEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> moList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal hh = highestList.ElementAtOrDefault(i);
            decimal ll = lowestList.ElementAtOrDefault(i);

            decimal mo = hh - ll != 0 ? MinOrMax(100 * ((2 * currentValue) - hh - ll) / (hh - ll), 100, -100) : 0;
            moList.Add(mo);
        }

        var moEmaList = GetMovingAverageList(stockData, maType, signalLength, moList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mo = moList.ElementAtOrDefault(i);
            decimal moEma = moEmaList.ElementAtOrDefault(i);
            decimal prevMo = i >= 1 ? moList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMoEma = i >= 1 ? moEmaList.ElementAtOrDefault(i - 1) : 0;

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
    public static StockData CalculateMorphedSineWave(this StockData stockData, int length = 14, decimal power = 100)
    {
        List<decimal> sList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal p = length / (2 * Pi);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevS1 = i >= 1 ? sList.ElementAtOrDefault(i - 1) : 0;
            decimal prevS2 = i >= 2 ? sList.ElementAtOrDefault(i - 2) : 0;
            decimal c = (currentValue * power) + Sin(i / p);

            decimal s = c / power;
            sList.Add(s);

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
        List<decimal> mtList = new();
        List<decimal> mtSignalList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevMt = mtList.LastOrDefault();
            decimal mt = currentValue - prevValue;
            mtList.Add(mt);

            decimal prevMtSignal = mtSignalList.LastOrDefault();
            decimal mtSignal = mt - prevMt;
            mtSignalList.Add(mtSignal);

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
    public static StockData CalculateMultiLevelIndicator(this StockData stockData, int length = 14, decimal factor = 10000)
    {
        List<decimal> zList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevOpen = i >= length ? openList.ElementAtOrDefault(i - length) : 0;
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevZ1 = i >= 1 ? zList.ElementAtOrDefault(i - 1) : 0;
            decimal prevZ2 = i >= 2 ? zList.ElementAtOrDefault(i - 2) : 0;

            decimal z = (currentClose - currentOpen - (currentClose - prevOpen)) * factor;
            zList.Add(z);

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
        int length = 50, decimal mult = 1)
    {
        List<decimal> gannHiloList = new();
        List<decimal> cList = new();
        List<decimal> dList = new();
        List<decimal> gList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal max = Math.Max(currentClose, currentOpen);
            decimal min = Math.Min(currentClose, currentOpen);
            decimal a = highestHigh - max;
            decimal b = min - lowestLow;

            decimal c = max + (a * mult);
            cList.Add(c);

            decimal d = min - (b * mult);
            dList.Add(d);
        }

        var eList = GetMovingAverageList(stockData, maType, length, cList);
        var fList = GetMovingAverageList(stockData, maType, length, dList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal f = fList.ElementAtOrDefault(i);
            decimal e = eList.ElementAtOrDefault(i);

            decimal prevG = gList.LastOrDefault();
            decimal g = currentClose > e ? 1 : currentClose > f ? 0 : prevG;
            gList.Add(g);

            decimal prevGannHilo = gannHiloList.LastOrDefault();
            decimal gannHilo = (g * f) + ((1 - g) * e);
            gannHiloList.Add(gannHilo);

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
        List<decimal> mdiList = new();
        List<decimal> cp2List = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal len1Sum = tempList.TakeLastExt(fastLength - 1).Sum();
            decimal len2Sum = tempList.TakeLastExt(slowLength - 1).Sum();

            decimal prevCp2 = cp2List.LastOrDefault();
            decimal cp2 = ((fastLength * len2Sum) - (slowLength * len1Sum)) / (slowLength - fastLength);
            cp2List.Add(cp2);

            decimal prevMdi = mdiList.LastOrDefault();
            decimal mdi = currentValue + prevValue != 0 ? 100 * (prevCp2 - cp2) / ((currentValue + prevValue) / 2) : 0;
            mdiList.Add(mdi);

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
        List<decimal> moList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hMax = highestList.ElementAtOrDefault(i);
            decimal lMin = lowestList.ElementAtOrDefault(i);
            decimal prevC = i >= length2 ? inputList.ElementAtOrDefault(i - length2) : 0;
            decimal rx = length1 != 0 ? (hMax - lMin) / length1 : 0;

            int imx = 1;
            decimal pdfmx = 0, pdfc = 0, rx1, bu, bl, bu1, bl1, pdf;
            for (int j = 1; j <= length1; j++)
            {
                bu = lMin + (j * rx);
                bl = bu - rx;

                decimal currHigh = i >= j ? highList.ElementAtOrDefault(i - j) : 0;
                decimal currLow = i >= j ? lowList.ElementAtOrDefault(i - j) : 0;
                decimal hMax1 = currHigh, lMin1 = currLow;
                for (int k = 2; k < length2; k++)
                {
                    decimal high = i >= j + k ? highList.ElementAtOrDefault(i - (j + k)) : 0;
                    decimal low = i >= j + k ? lowList.ElementAtOrDefault(i - (j + k)) : 0;
                    hMax1 = Math.Max(high, hMax1);
                    lMin1 = Math.Min(low, lMin1);
                }

                rx1 = length1 != 0 ? (hMax1 - lMin1) / length1 : 0; //-V3022
                bl1 = lMin1 + ((j - 1) * rx1);
                bu1 = lMin1 + (j * rx1);

                pdf = 0;
                for (int k = 1; k <= length2; k++)
                {
                    decimal high = i >= j + k ? highList.ElementAtOrDefault(i - (j + k)) : 0;
                    decimal low = i >= j + k ? lowList.ElementAtOrDefault(i - (j + k)) : 0;

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

            decimal pmo = lMin + ((imx - 0.5m) * rx);
            decimal mo = pdfmx != 0 ? 100 * (1 - (pdfc / pdfmx)) : 0;
            mo = prevC < pmo ? -mo : mo;
            moList.AddRounded(-mo);
        }

        var moWmaList = GetMovingAverageList(stockData, maType, signalLength, moList);
        var moSigList = GetMovingAverageList(stockData, maType, signalLength, moWmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mo = moWmaList.ElementAtOrDefault(i);
            decimal moSig = moSigList.ElementAtOrDefault(i);
            decimal prevMo = i >= 1 ? moWmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMoSig = i >= 1 ? moSigList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> mtiList = new();
        List<decimal> advList = new();
        List<decimal> decList = new();
        List<decimal> advVolList = new();
        List<decimal> decVolList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentVolume = volumeList.ElementAtOrDefault(i);

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

            decimal mti = ((advSum * advVolSum) - (decSum * decVolSum)) / 1000000;
            mtiList.Add(mti);
        }

        var mtiEmaList = GetMovingAverageList(stockData, maType, length, mtiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mtiEma = mtiEmaList.ElementAtOrDefault(i);
            decimal prevMtiEma = i >= 1 ? mtiEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> iList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal advance = currentValue > prevValue ? 1 : 0;
            decimal decline = currentValue < prevValue ? 1 : 0;

            decimal iVal = advance + decline != 0 ? 1000 * (advance - decline) / (advance + decline) : 0;
            iList.Add(iVal);
        }

        var ivalEmaList = GetMovingAverageList(stockData, maType, length, iList);
        var stoList = GetMovingAverageList(stockData, maType, length, ivalEmaList);
        var stoEmaList = GetMovingAverageList(stockData, maType, length, stoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sto = stoList.ElementAtOrDefault(i);
            decimal stoEma = stoEmaList.ElementAtOrDefault(i);
            decimal prevSto = i >= 1 ? stoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevStoEma = i >= 1 ? stoEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> deltaList = new();
        List<decimal> deltaHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevOpen = i >= length2 ? openList.ElementAtOrDefault(i - length2) : 0;

            decimal delta = currentClose - prevOpen;
            deltaList.Add(delta);
        }

        var deltaSmaList = GetMovingAverageList(stockData, maType, length1, deltaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal delta = deltaList.ElementAtOrDefault(i);
            decimal deltaSma = deltaSmaList.ElementAtOrDefault(i);

            decimal prevDeltaHistogram = deltaHistogramList.LastOrDefault();
            decimal deltaHistogram = delta - deltaSma;
            deltaHistogramList.Add(deltaHistogram);

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
        List<decimal> dspList = new();
        List<decimal> ema1List = new();
        List<decimal> ema2List = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        decimal alpha = length > 2 ? (decimal)2 / (length + 1) : 0.67m;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal high = Math.Max(currentHigh, prevHigh);
            decimal low = Math.Min(currentLow, prevLow);
            decimal price = (high + low) / 2;
            decimal prevEma1 = i >= 1 ? ema1List.ElementAtOrDefault(i - 1) : price;
            decimal prevEma2 = i >= 1 ? ema2List.ElementAtOrDefault(i - 1) : price;

            decimal ema1 = (alpha * price) + ((1 - alpha) * prevEma1);
            ema1List.Add(ema1);

            decimal ema2 = (alpha / 2 * price) + ((1 - (alpha / 2)) * prevEma2);
            ema2List.Add(ema2);

            decimal prevDsp = dspList.LastOrDefault();
            decimal dsp = ema1 - ema2;
            dspList.Add(dsp);

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
        List<decimal> s1List = new();
        List<decimal> s2List = new();
        List<decimal> s1SmaList = new();
        List<Signal> signalsList = new();

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length: length1).CustomValuesList;
        var rsiEma1List = GetMovingAverageList(stockData, maType, length3, rsiList);
        var rsiEma2List = GetMovingAverageList(stockData, maType, length4, rsiEma1List);

        for (int i = 0; i < rsiList.Count; i++)
        {
            decimal prevS1 = s1List.LastOrDefault();
            decimal s1 = rsiEma2List.ElementAtOrDefault(i);
            s1List.Add(s1);

            decimal prevS1Sma = s1SmaList.LastOrDefault();
            decimal s1Sma = s1List.TakeLastExt(length2).Average();
            s1SmaList.Add(s1Sma);

            decimal s2 = s1 - s1Sma;
            s2List.Add(s2);

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
        List<decimal> rangeList = new();
        List<decimal> doList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);

            decimal range = highest - lowest;
            rangeList.Add(range);
        }

        var vaList = GetMovingAverageList(stockData, maType, length1, rangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal va = vaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal pctChg = prevValue != 0 ? (currentValue - prevValue) / Math.Abs(prevValue) * 100 : 0;
            decimal currentVolume = stockData.Volumes.ElementAtOrDefault(i);
            decimal k = va != 0 ? (3 * currentValue) / va : 0;
            decimal pctK = pctChg * k;
            decimal volPctK = pctK != 0 ? currentVolume / pctK : 0;
            decimal bp = currentValue > prevValue ? currentVolume : volPctK;
            decimal sp = currentValue > prevValue ? volPctK : currentVolume;

            decimal dosc = bp - sp;
            doList.Add(dosc);
        }

        var doEmaList = GetMovingAverageList(stockData, maType, length3, doList);
        var doSigList = GetMovingAverageList(stockData, maType, length1, doEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal doSig = doSigList.ElementAtOrDefault(i);
            decimal prevSig1 = i >= 1 ? doSigList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSig2 = i >= 2 ? doSigList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> momList = new();
        List<decimal> srcLcList = new();
        List<decimal> hcLcList = new();
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

            decimal hcLc = hc - lc;
            hcLcList.Add(hcLc);
        }

        var topEma1List = GetMovingAverageList(stockData, maType, length2, srcLcList);
        var topEma2List = GetMovingAverageList(stockData, maType, length3, topEma1List);
        var botEma1List = GetMovingAverageList(stockData, maType, length2, hcLcList);
        var botEma2List = GetMovingAverageList(stockData, maType, length3, botEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal top = topEma2List.ElementAtOrDefault(i);
            decimal bot = botEma2List.ElementAtOrDefault(i);

            decimal mom = bot != 0 ? MinOrMax(100 * top / bot, 100, 0) : 0;
            momList.Add(mom);
        }

        var momEmaList = GetMovingAverageList(stockData, maType, length3, momList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mom = momList.ElementAtOrDefault(i);
            decimal momEma = momEmaList.ElementAtOrDefault(i);
            decimal prevMom = i >= 1 ? momList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMomEma = i >= 1 ? momEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> curtaList = new();
        List<decimal> longaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mediumSmaList = GetMovingAverageList(stockData, maType, length2, inputList);
        var shortSmaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var longSmaList = GetMovingAverageList(stockData, maType, length3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal mediumSma = mediumSmaList.ElementAtOrDefault(i);
            decimal shortSma = shortSmaList.ElementAtOrDefault(i);
            decimal longSma = longSmaList.ElementAtOrDefault(i);

            decimal prevCurta = curtaList.LastOrDefault();
            decimal curta = mediumSma != 0 ? shortSma / mediumSma : 0;
            curtaList.Add(curta);

            decimal prevLonga = longaList.LastOrDefault();
            decimal longa = mediumSma != 0 ? longSma / mediumSma : 0;
            longaList.Add(longa);

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
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> disparityIndexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);

            decimal prevDisparityIndex = disparityIndexList.LastOrDefault();
            decimal disparityIndex = currentSma != 0 ? (currentValue - currentSma) / currentSma * 100 : 0;
            disparityIndexList.Add(disparityIndex);

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
        decimal threshold = 1.5m)
    {
        List<decimal> rangeList = new();
        List<decimal> diList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            
            decimal range = currentHigh - currentLow;
            rangeList.Add(range);
        }

        var rangeSmaList = GetMovingAverageList(stockData, maType, length, rangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevSma1 = i >= 1 ? rangeSmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSma6 = i >= 6 ? rangeSmaList.ElementAtOrDefault(i - 6) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;
            decimal currentSma = smaList.ElementAtOrDefault(i);

            decimal di = prevSma6 != 0 ? prevSma1 / prevSma6 : 0;
            diList.Add(di);

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
        List<decimal> dtiList = new();
        List<decimal> diffList = new();
        List<decimal> absDiffList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal hmu = currentHigh - prevHigh > 0 ? currentHigh - prevHigh : 0;
            decimal lmd = currentLow - prevLow < 0 ? (currentLow - prevLow) * -1 : 0;

            decimal diff = hmu - lmd;
            diffList.Add(diff);

            decimal absDiff = Math.Abs(diff);
            absDiffList.Add(absDiff);
        }
        
        var diffEma1List = GetMovingAverageList(stockData, maType, length1, diffList);
        var absDiffEma1List = GetMovingAverageList(stockData, maType, length1, absDiffList);
        var diffEma2List = GetMovingAverageList(stockData, maType, length2, diffEma1List);
        var absDiffEma2List = GetMovingAverageList(stockData, maType, length2, absDiffEma1List);
        var diffEma3List = GetMovingAverageList(stockData, maType, length3, diffEma2List);
        var absDiffEma3List = GetMovingAverageList(stockData, maType, length3, absDiffEma2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal diffEma3 = diffEma3List.ElementAtOrDefault(i);
            decimal absDiffEma3 = absDiffEma3List.ElementAtOrDefault(i);
            decimal prevDti1 = i >= 1 ? dtiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevDti2 = i >= 2 ? dtiList.ElementAtOrDefault(i - 2) : 0;

            decimal dti = absDiffEma3 != 0 ? MinOrMax(100 * diffEma3 / absDiffEma3, 100, -100) : 0;
            dtiList.Add(dti);

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
        List<decimal> tempHighList = new();
        List<decimal> tempLowList = new();
        List<decimal> upAtrList = new();
        List<decimal> dnAtrList = new();
        List<decimal> upwalkList = new();
        List<decimal> dnwalkList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highestHigh = highestList.ElementAtOrDefault(i);
            decimal lowestLow = lowestList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal currentHigh = highList.ElementAtOrDefault(i);
            tempHighList.Add(currentHigh);

            decimal currentLow = lowList.ElementAtOrDefault(i);
            tempLowList.Add(currentLow);

            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            int maxIndex = tempHighList.LastIndexOf(highestHigh);
            int minIndex = tempLowList.LastIndexOf(lowestLow);
            int dnRun = i - maxIndex;
            int upRun = i - minIndex;

            decimal prevAtrUp = upAtrList.LastOrDefault();
            decimal upK = upRun != 0 ? (decimal)1 / upRun : 0;
            decimal atrUp = (tr * upK) + (prevAtrUp * (1 - upK));
            upAtrList.Add(atrUp);

            decimal prevAtrDn = dnAtrList.LastOrDefault();
            var dnK = dnRun != 0 ? (decimal)1 / dnRun : 0;
            decimal atrDn = (tr * dnK) + (prevAtrDn * (1 - dnK));
            dnAtrList.Add(atrDn);

            decimal upDen = atrUp > 0 ? atrUp : 1;
            decimal prevUpWalk = upwalkList.LastOrDefault();
            decimal upWalk = upRun > 0 ? (currentHigh - lowestLow) / (Sqrt(upRun) * upDen) : 0;
            upwalkList.Add(upWalk);

            decimal dnDen = atrDn > 0 ? atrDn : 1;
            decimal prevDnWalk = dnwalkList.LastOrDefault();
            decimal dnWalk = dnRun > 0 ? (highestHigh - currentLow) / (Sqrt(dnRun) * dnDen) : 0;
            dnwalkList.Add(dnWalk);

            var signal = GetCompareSignal(upWalk - dnWalk, prevUpWalk - prevDnWalk);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpWalk", upwalkList },
            { "DnWalk", dnwalkList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> stoRsiList = new();
        List<decimal> skList = new();
        List<decimal> sdList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var wilderMovingAvgList = GetMovingAverageList(stockData, maType, length1, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(wilderMovingAvgList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wima = wilderMovingAvgList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal prevSd1 = i >= 1 ? sdList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSd2 = i >= 2 ? sdList.ElementAtOrDefault(i - 2) : 0;

            decimal stoRsi = highest - lowest != 0 ? MinOrMax(100 * (wima - lowest) / (highest - lowest), 100, 0) : 0;
            stoRsiList.Add(stoRsi);

            decimal sk = stoRsiList.TakeLastExt(length3).Average();
            skList.Add(sk);

            decimal sd = skList.TakeLastExt(length4).Average();
            sdList.Add(sd);

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
        List<decimal> bList = new();
        List<decimal> cList = new();
        List<decimal> absChgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal prevSma = i >= length ? smaList.ElementAtOrDefault(i - length) : 0;

            decimal absChg = Math.Abs(currentValue - prevValue);
            absChgList.Add(absChg);

            decimal b = sma - prevSma;
            bList.Add(b);
        }

        var aList = GetMovingAverageList(stockData, maType, length, absChgList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = aList.ElementAtOrDefault(i);
            decimal b = bList.ElementAtOrDefault(i);
            decimal prevC1 = i >= 1 ? cList.ElementAtOrDefault(i - 1) : 0;
            decimal prevC2 = i >= 2 ? cList.ElementAtOrDefault(i - 2) : 0;

            decimal c = a != 0 ? MinOrMax(b / a, 1, 0) : 0;
            cList.Add(c);

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
        List<decimal> sroList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
            decimal prevSro1 = i >= 1 ? sroList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSro2 = i >= 2 ? sroList.ElementAtOrDefault(i - 2) : 0;

            decimal sro = tr != 0 ? MinOrMax((currentHigh - currentOpen + (currentClose - currentLow)) / (2 * tr), 1, 0) : 0;
            sroList.Add(sro);

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
        List<decimal> extList = new();
        List<decimal> yList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal prevY = i >= length ? yList.ElementAtOrDefault(i - length) : 0;
            decimal prevY2 = i >= length * 2 ? yList.ElementAtOrDefault(i - (length * 2)) : 0;

            decimal y = currentValue - sma;
            yList.Add(y);

            decimal ext = ((2 * prevY) - prevY2) / 2;
            extList.Add(ext);
        }

        stockData.CustomValuesList = extList;
        var oscList = CalculateStochasticOscillator(stockData, maType, length: length * 2).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal osc = oscList.ElementAtOrDefault(i);
            decimal prevOsc1 = i >= 1 ? oscList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOsc2 = i >= 2 ? oscList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> v3List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal v1 = currentClose - currentOpen;
            decimal v2 = currentHigh - currentLow;

            decimal v3 = v2 != 0 ? v1 / v2 : 0;
            v3List.Add(v3);
        }

        var sgiList = GetMovingAverageList(stockData, maType, length, v3List);
        var sgiEmaList = GetMovingAverageList(stockData, maType, length, sgiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sgi = sgiList.ElementAtOrDefault(i);
            decimal sgiEma = sgiEmaList.ElementAtOrDefault(i);
            decimal prevSgi = i >= 1 ? sgiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSgiEma = i >= 1 ? sgiEmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> aaSeList = new();
        List<decimal> sSeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length2 - 1 ? inputList.ElementAtOrDefault(i - (length2 - 1)) : 0;
            decimal moveSe = currentValue - prevValue;
            decimal avgMoveSe = moveSe / (length2 - 1);

            decimal aaSe = prevValue != 0 ? avgMoveSe / prevValue : 0;
            aaSeList.Add(aaSe);
        }

        var bList = GetMovingAverageList(stockData, maType, length1, aaSeList);
        stockData.CustomValuesList = bList;
        var stoList = CalculateStochasticOscillator(stockData, maType, length: length1).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bSto = stoList.ElementAtOrDefault(i);

            decimal sSe = (bSto * 2) - 100;
            sSeList.Add(sSe);
        }

        var ssSeList = GetMovingAverageList(stockData, maType, smoothingLength, sSeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ssSe = ssSeList.ElementAtOrDefault(i);
            decimal prevSsse = i >= 1 ? ssSeList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> coefCorrList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            var enumerableList = tempList.TakeLastExt(length).Select(x => (double)x);
            var orderedList = enumerableList.AsQueryExpr().OrderBy(j => j).Run();

            var sc = Correlation.Spearman(enumerableList, orderedList);
            sc = IsValueNullOrInfinity(sc) ? 0 : sc;
            coefCorrList.Add((decimal)sc * 100);
        }

        var sigList = GetMovingAverageList(stockData, maType, signalLength, coefCorrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sc = coefCorrList.ElementAtOrDefault(i);
            decimal prevSc = i >= 1 ? coefCorrList.ElementAtOrDefault(i - 1) : 0;
            decimal sig = sigList.ElementAtOrDefault(i);
            decimal prevSig = i >= 1 ? sigList.ElementAtOrDefault(i - 1) : 0;

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
            decimal wad = wadList.ElementAtOrDefault(i);
            decimal wadSma = wadSignalList.ElementAtOrDefault(i);
            decimal prevWad = i >= 1 ? wadList.ElementAtOrDefault(i - 1) : 0;
            decimal prevWadSma = i >= 1 ? wadSignalList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> srocList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, smoothingLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentMa = maList.ElementAtOrDefault(i);
            decimal prevMa = i >= length ? maList.ElementAtOrDefault(i - length) : 0;
            decimal mom = currentMa - prevMa;

            decimal prevSroc = srocList.LastOrDefault();
            decimal sroc = prevMa != 0 ? 100 * mom / prevMa : 100;
            srocList.Add(sroc);

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
        int fastLength = 14, int slowLength = 30, decimal factor = 0.95m)
    {
        List<decimal> rList = new();
        List<decimal> szoList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal r = currentValue > prevValue ? 1 : -1;
            rList.Add(r);
        }

        var spList = GetMovingAverageList(stockData, maType, fastLength, rList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sp = spList.ElementAtOrDefault(i);

            decimal szo = fastLength != 0 ? 100 * sp / fastLength : 0;
            szoList.Add(szo);
        }

        var (highestList, lowestList) = GetMaxAndMinValuesList(szoList, slowLength);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal range = highest - lowest;
            decimal ob = lowest + (range * factor);
            decimal os = highest - (range * factor);
            decimal szo = szoList.ElementAtOrDefault(i);
            decimal prevSzo1 = i >= 1 ? szoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSzo2 = i >= 2 ? szoList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> srcList = new();
        List<decimal> cEmaList = new();
        List<decimal> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevC1 = i >= 1 ? cList.ElementAtOrDefault(i - 1) : 0;
            decimal prevC2 = i >= 2 ? cList.ElementAtOrDefault(i - 2) : 0;
            decimal prevSrc = i >= length ? srcList.ElementAtOrDefault(i - length) : 0;

            decimal src = currentValue + prevC1;
            srcList.Add(src);

            decimal cEma = CalculateEMA(prevC1, cEmaList.LastOrDefault(), length);
            cEmaList.Add(cEma);

            decimal b = prevC1 - cEma;
            decimal c = (a * (src - prevSrc)) + ((1 - a) * b);
            cList.Add(c);

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
        int length1 = 100, int length2 = 60, int smoothingLength = 3, decimal threshold = 90)
    {
        List<decimal> aboveList = new();
        List<decimal> stiffValueList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal bound = sma - (0.2m * stdDev);

            decimal above = currentValue > bound ? 1 : 0;
            aboveList.Add(above);

            decimal aboveSum = aboveList.TakeLastExt(length2).Sum();
            decimal stiffValue = length2 != 0 ? aboveSum * 100 / length2 : 0;
            stiffValueList.Add(stiffValue);
        }

        var stiffnessList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, smoothingLength, stiffValueList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stiffness = stiffnessList.ElementAtOrDefault(i);
            decimal prevStiffness = i >= 1 ? stiffnessList.ElementAtOrDefault(i - 1) : 0;

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
    public static StockData CalculateSuperTrendFilter(this StockData stockData, int length = 200, decimal factor = 0.9m)
    {
        List<decimal> tList = new();
        List<decimal> srcList = new();
        List<decimal> trendUpList = new();
        List<decimal> trendDnList = new();
        List<decimal> trendList = new();
        List<decimal> tslList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal p = Pow(length, 2), a = 2 / (p + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevTsl1 = i >= 1 ? tslList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevTsl2 = i >= 2 ? tslList.ElementAtOrDefault(i - 2) : 0;
            decimal d = Math.Abs(currentValue - prevTsl1);

            decimal prevT = i >= 1 ? tList.ElementAtOrDefault(i - 1) : d;
            decimal t = (a * d) + ((1 - a) * prevT);
            tList.Add(t);

            decimal prevSrc = srcList.LastOrDefault();
            decimal src = (factor * prevTsl1) + ((1 - factor) * currentValue);
            srcList.Add(src);

            decimal up = prevTsl1 - t;
            decimal dn = prevTsl1 + t;

            decimal prevTrendUp = trendUpList.LastOrDefault();
            decimal trendUp = prevSrc > prevTrendUp ? Math.Max(up, prevTrendUp) : up;
            trendUpList.Add(trendUp);

            decimal prevTrendDn = trendDnList.LastOrDefault();
            decimal trendDn = prevSrc < prevTrendDn ? Math.Min(dn, prevTrendDn) : dn;
            trendDnList.Add(trendDn);

            decimal prevTrend = i >= 1 ? trendList.ElementAtOrDefault(i - 1) : 1;
            decimal trend = src > prevTrendDn ? 1 : src < prevTrendUp ? -1 : prevTrend;
            trendList.Add(trend);

            decimal tsl = trend == 1 ? trendDn : trendUp;
            tslList.Add(tsl);

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
        List<decimal> pcList = new();
        List<decimal> absPCList = new();
        List<decimal> smiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal pc = currentValue - prevValue;
            pcList.Add(pc);

            decimal absPC = Math.Abs(currentValue - prevValue);
            absPCList.Add(absPC);
        }

        var pcSmooth1List = GetMovingAverageList(stockData, maType, fastLength, pcList); 
        var pcSmooth2List = GetMovingAverageList(stockData, maType, slowLength, pcSmooth1List);
        var absPCSmooth1List = GetMovingAverageList(stockData, maType, fastLength, absPCList);
        var absPCSmooth2List = GetMovingAverageList(stockData, maType, slowLength, absPCSmooth1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal absSmooth2PC = absPCSmooth2List.ElementAtOrDefault(i);
            decimal smooth2PC = pcSmooth2List.ElementAtOrDefault(i);

            decimal smi = absSmooth2PC != 0 ? MinOrMax(100 * smooth2PC / absSmooth2PC, 100, -100) : 0;
            smiList.Add(smi);
        }

        var smiSignalList = GetMovingAverageList(stockData, maType, signalLength, smiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal smi = smiList.ElementAtOrDefault(i);
            decimal smiSignal = smiSignalList.ElementAtOrDefault(i);
            decimal prevSmi = i >= 1 ? smiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSmiSignal = i >= 1 ? smiSignalList.ElementAtOrDefault(i - 1) : 0;

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
    public static StockData CalculateSimpleLines(this StockData stockData, int length = 10, decimal mult = 10)
    {
        List<decimal> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal s = 0.01m * 100 * ((decimal)1 / length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevA2 = i >= 2 ? aList.ElementAtOrDefault(i - 2) : currentValue;
            decimal x = currentValue + ((prevA - prevA2) * mult);

            prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : x;
            decimal a = x > prevA + s ? prevA + s : x < prevA - s ? prevA - s : prevA;
            aList.Add(a);

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
        List<decimal> oscList = new();
        List<Signal> signalsList = new();

        if (stockData.Count == marketData.Count)
        {
            var bull1List = CalculateRateOfChange(stockData, length1).CustomValuesList;
            var bull2List = CalculateRateOfChange(stockData, length2).CustomValuesList;
            var bear1List = CalculateRateOfChange(marketData, length1).CustomValuesList;
            var bear2List = CalculateRateOfChange(marketData, length2).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal bull1 = bull1List.ElementAtOrDefault(i);
                decimal bull2 = bull2List.ElementAtOrDefault(i);
                decimal bear1 = bear1List.ElementAtOrDefault(i);
                decimal bear2 = bear2List.ElementAtOrDefault(i);
                decimal bull = (bull1 + bull2) / 2;
                decimal bear = (bear1 + bear2) / 2;

                decimal osc = 100 * (bull - bear);
                oscList.Add(osc);
            }

            var oscEmaList = GetMovingAverageList(stockData, maType, length1, oscList);
            for (int j = 0; j < stockData.Count; j++)
            {
                decimal oscEma = oscEmaList.ElementAtOrDefault(j);
                decimal prevOscEma1 = j >= 1 ? oscEmaList.ElementAtOrDefault(j - 1) : 0;
                decimal prevOscEma2 = j >= 2 ? oscEmaList.ElementAtOrDefault(j - 2) : 0;

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
        List<decimal> ewrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);
        var (highestList1, lowestList1) = GetMaxAndMinValuesList(inputList, length);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(volumeList, length);

        decimal af = length < 10 ? 0.25m : ((decimal)length / 32) - 0.0625m;
        int smaLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var srcSmaList = GetMovingAverageList(stockData, maType, smaLength, inputList);
        var volSmaList = GetMovingAverageList(stockData, maType, smaLength, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal maxVol = highestList2.ElementAtOrDefault(i);
            decimal minVol = lowestList2.ElementAtOrDefault(i);
            decimal maxSrc = highestList1.ElementAtOrDefault(i);
            decimal minSrc = lowestList1.ElementAtOrDefault(i);
            decimal srcSma = srcSmaList.ElementAtOrDefault(i);
            decimal volSma = volSmaList.ElementAtOrDefault(i);
            decimal volume = volumeList.ElementAtOrDefault(i);
            decimal volWr = maxVol - minVol != 0 ? 2 * ((volume - volSma) / (maxVol - minVol)) : 0;
            decimal srcWr = maxSrc - minSrc != 0 ? 2 * ((currentValue - srcSma) / (maxSrc - minSrc)) : 0;
            decimal srcSwr = maxSrc - minSrc != 0 ? 2 * ((currentValue - prevValue) / (maxSrc - minSrc)) : 0;

            decimal ewr = ((volWr > 0 && srcWr > 0 && currentValue > prevValue) || (volWr > 0 && srcWr < 0 && currentValue < prevValue)) && srcSwr + af != 0 ?
                ((50 * (srcWr * (srcSwr + af) * volWr)) + srcSwr + af) / (srcSwr + af) : 25 * ((srcWr * (volWr + 1)) + 2);
            ewrList.Add(ewr);
        }

        var ewrSignalList = GetMovingAverageList(stockData, maType, signalLength, ewrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ewr = ewrList.ElementAtOrDefault(i);
            decimal ewrSignal = ewrSignalList.ElementAtOrDefault(i);
            decimal prevEwr = i >= 1 ? ewrList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEwrSignal = i >= 1 ? ewrSignalList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> mode1List = new();
        List<decimal> mode2List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, closeList, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? closeList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 2 ? lowList.ElementAtOrDefault(i - 2) : 0;
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevMode1 = mode1List.LastOrDefault();
            decimal mode1 = (prevLow + currentHigh) / 2;
            mode1List.Add(mode1);

            decimal prevMode2 = mode2List.LastOrDefault();
            decimal mode2 = (prevValue2 + currentValue + prevClose) / 3;
            mode2List.Add(mode2);

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
        List<decimal> emtList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;

            decimal emt = currentHigh < prevHigh && currentLow > prevLow ? 0 : currentHigh - prevHigh > prevLow - currentLow ? Math.Abs(currentHigh - prevHigh) :
                Math.Abs(prevLow - currentLow);
            emtList.Add(emt);
        }

        var aemtList = GetMovingAverageList(stockData, maType, length, emtList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal emt = emtList.ElementAtOrDefault(i);
            decimal emtEma = aemtList.ElementAtOrDefault(i);

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
        List<decimal> ewoList = new();
        List<decimal> ewoHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var sma34List = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentSma5 = smaList.ElementAtOrDefault(i);
            decimal currentSma34 = sma34List.ElementAtOrDefault(i);

            decimal ewo = currentSma5 - currentSma34;
            ewoList.Add(ewo);
        }

        var ewoSignalLineList = GetMovingAverageList(stockData, maType, fastLength, ewoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ewo = ewoList.ElementAtOrDefault(i);
            decimal ewoSignalLine = ewoSignalLineList.ElementAtOrDefault(i);

            decimal prevEwoHistogram = ewoHistogramList.LastOrDefault();
            decimal ewoHistogram = ewo - ewoSignalLine;
            ewoHistogramList.Add(ewoHistogram);

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
        List<decimal> xcoList = new();
        List<decimal> xhlList = new();
        List<decimal> ecoList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentClose = inputList.ElementAtOrDefault(i);

            decimal xco = currentClose - currentOpen;
            xcoList.Add(xco);

            decimal xhl = currentHigh - currentLow;
            xhlList.Add(xhl);
        }

        var xcoEma1List = GetMovingAverageList(stockData, maType, length1, xcoList);
        var xcoEma2List = GetMovingAverageList(stockData, maType, length2, xcoEma1List);
        var xhlEma1List = GetMovingAverageList(stockData, maType, length1, xhlList);
        var xhlEma2List = GetMovingAverageList(stockData, maType, length2, xhlEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal xhlEma2 = xhlEma2List.ElementAtOrDefault(i);
            decimal xcoEma2 = xcoEma2List.ElementAtOrDefault(i);

            decimal eco = xhlEma2 != 0 ? 100 * xcoEma2 / xhlEma2 : 0;
            ecoList.Add(eco);
        }

        var ecoSignalList = GetMovingAverageList(stockData, maType, length2, ecoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal eco = ecoList.ElementAtOrDefault(i);
            decimal ecoEma = ecoSignalList.ElementAtOrDefault(i);
            decimal prevEco = i >= 1 ? ecoList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEcoEma = i >= 1 ? ecoSignalList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> etsiList = new();
        List<decimal> priceDiffList = new();
        List<decimal> absPriceDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal priceDiff = currentValue - prevValue;
            priceDiffList.Add(priceDiff);

            decimal absPriceDiff = Math.Abs(priceDiff);
            absPriceDiffList.Add(absPriceDiff);
        }

        var diffEma1List = GetMovingAverageList(stockData, maType, length1, priceDiffList);
        var absDiffEma1List = GetMovingAverageList(stockData, maType, length1, absPriceDiffList);
        var diffEma2List = GetMovingAverageList(stockData, maType, length2, diffEma1List);
        var absDiffEma2List = GetMovingAverageList(stockData, maType, length2, absDiffEma1List);
        var diffEma3List = GetMovingAverageList(stockData, maType, length3, diffEma2List);
        var absDiffEma3List = GetMovingAverageList(stockData, maType, length3, absDiffEma2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal diffEma3 = diffEma3List.ElementAtOrDefault(i);
            decimal absDiffEma3 = absDiffEma3List.ElementAtOrDefault(i);

            decimal etsi = absDiffEma3 != 0 ? MinOrMax(100 * diffEma3 / absDiffEma3, 100, -100) : 0;
            etsiList.Add(etsi);
        }

        var etsiSignalList = GetMovingAverageList(stockData, maType, signalLength, etsiList);
        for (int k = 0; k < stockData.Count; k++)
        {
            decimal etsi = etsiList.ElementAtOrDefault(k);
            decimal etsiSignal = etsiSignalList.ElementAtOrDefault(k);
            decimal prevEtsi = k >= 1 ? etsiList.ElementAtOrDefault(k - 1) : 0;
            decimal prevEtsiSignal = k >= 1 ? etsiSignalList.ElementAtOrDefault(k - 1) : 0;

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
        List<decimal> etsi2List = new();
        List<decimal> etsi1List = new();
        List<decimal> priceDiffList = new();
        List<decimal> absPriceDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal priceDiff = currentValue - prevValue;
            priceDiffList.Add(priceDiff);

            decimal absPriceDiff = Math.Abs(currentValue - prevValue);
            absPriceDiffList.Add(absPriceDiff);
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
            decimal diffEma6 = diffEma6List.ElementAtOrDefault(i);
            decimal absDiffEma6 = absDiffEma6List.ElementAtOrDefault(i);
            decimal diffEma3 = diffEma3List.ElementAtOrDefault(i);
            decimal absDiffEma3 = absDiffEma3List.ElementAtOrDefault(i);

            decimal etsi1 = absDiffEma3 != 0 ? MinOrMax(diffEma3 / absDiffEma3 * 100, 100, -100) : 0;
            etsi1List.Add(etsi1);

            decimal etsi2 = absDiffEma6 != 0 ? MinOrMax(diffEma6 / absDiffEma6 * 100, 100, -100) : 0;
            etsi2List.Add(etsi2);
        }

        var etsi2SignalList = GetMovingAverageList(stockData, maType, signalLength, etsi2List);
        for (int k = 0; k < stockData.Count; k++)
        {
            decimal etsi2 = etsi2List.ElementAtOrDefault(k);
            decimal etsi2Signal = etsi2SignalList.ElementAtOrDefault(k);
            decimal prevEtsi2 = k >= 1 ? etsi2List.ElementAtOrDefault(k - 1) : 0;
            decimal prevEtsi2Signal = k >= 1 ? etsi2SignalList.ElementAtOrDefault(k - 1) : 0;

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
        int length = 32, int smoothLength = 5, decimal pointValue = 1)
    {
        List<decimal> ergodicCsiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        decimal k = 100 * (pointValue / Sqrt(length) / (150 + smoothLength));

        var adxList = CalculateAverageDirectionalIndex(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal adx = adxList.ElementAtOrDefault(i);
            decimal prevAdx = i >= 1 ? adxList.ElementAtOrDefault(i - 1) : 0;
            decimal adxR = (adx + prevAdx) * 0.5m;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            decimal csi = length + tr > 0 ? k * adxR * tr / length : 0;

            decimal ergodicCsi = currentValue > 0 ? csi / currentValue : 0;
            ergodicCsiList.Add(ergodicCsi);
        }

        var ergodicCsiSmaList = GetMovingAverageList(stockData, maType, smoothLength, ergodicCsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ergodicCsiSma = ergodicCsiSmaList.ElementAtOrDefault(i);
            decimal prevErgodicCsiSma1 = i >= 1 ? ergodicCsiSmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevErgodicCsiSma2 = i >= 2 ? ergodicCsiSmaList.ElementAtOrDefault(i - 2) : 0;

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
        List<decimal> closewrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        int smaLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var smaList = GetMovingAverageList(stockData, maType, smaLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal dnm = highest - lowest;
            decimal sma = smaList.ElementAtOrDefault(i);

            decimal closewr = dnm != 0 ? 2 * (currentValue - sma) / dnm : 0;
            closewrList.Add(closewr);
        }

        var closewrSmaList = GetMovingAverageList(stockData, maType, signalLength, closewrList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal closewr = closewrList.ElementAtOrDefault(i);
            decimal closewrSma = closewrSmaList.ElementAtOrDefault(i);
            decimal prevCloseWr = i >= 1 ? closewrList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCloseWrSma = i >= 1 ? closewrSmaList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> emaADiffList = new();
        List<decimal> emaBDiffList = new();
        List<decimal> emaCDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaAList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length1, inputList);
        var emaBList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length2, inputList);
        var emaCList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal emaA = emaAList.ElementAtOrDefault(i);
            decimal emaB = emaBList.ElementAtOrDefault(i);
            decimal emaC = emaCList.ElementAtOrDefault(i);

            decimal emaADiff = currentValue - emaA;
            emaADiffList.Add(emaADiff);

            decimal emaBDiff = currentValue - emaB;
            emaBDiffList.Add(emaBDiff);

            decimal emaCDiff = currentValue - emaC;
            emaCDiffList.Add(emaCDiff);
        }

        var waList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, smoothLength, emaADiffList);
        var wbList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, smoothLength, emaBDiffList);
        var wcList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, smoothLength, emaCDiffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wa = waList.ElementAtOrDefault(i);
            decimal wb = wbList.ElementAtOrDefault(i);
            decimal wc = wcList.ElementAtOrDefault(i);

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
        stockData.CustomValuesList = new List<decimal>();
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
        List<decimal> ma1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);

            decimal ma1 = currentValue - currentEma;
            ma1List.Add(ma1);
        }

        var ma1EmaList = GetMovingAverageList(stockData, maType, length2, ma1List);
        var emdiList = GetMovingAverageList(stockData, maType, length3, ma1EmaList);
        var emdiSignalList = GetMovingAverageList(stockData, maType, signalLength, emdiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal emdi = emdiList.ElementAtOrDefault(i);
            decimal emdiSignal = emdiSignalList.ElementAtOrDefault(i);
            decimal prevEmdi = i >= 1 ? emdiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEmdiSignal = i >= 1 ? emdiSignalList.ElementAtOrDefault(i - 1) : 0;

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
        List<decimal> epList = new();
        List<decimal> chgErList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal er = erList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal prevEp1 = i >= 1 ? epList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEp2 = i >= 2 ? epList.ElementAtOrDefault(i - 2) : 0;

            decimal chgEr = (currentValue - prevValue) * er;
            chgErList.Add(chgEr);

            decimal ep = chgErList.Sum();
            epList.Add(ep);

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
    public static StockData CalculateEfficientAutoLine(this StockData stockData, int length = 19, decimal fastAlpha = 0.0001m, decimal slowAlpha = 0.005m)
    {
        List<decimal> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal er = erList.ElementAtOrDefault(i);
            decimal dev = (er * fastAlpha) + ((1 - er) * slowAlpha);

            decimal prevA = aList.LastOrDefault();
            decimal a = i < 9 ? currentValue : currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue : prevA;
            aList.Add(a);

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