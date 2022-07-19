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
    /// Calculates the bollinger bands.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="stdDevMult">The standard dev mult.</param>
    /// <param name="movingAvgType">Average type of the moving.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateBollingerBands(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20, decimal stdDevMult = 2)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDeviationList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = smaList[i];
            decimal currentValue = inputList[i];
            decimal currentStdDeviation = stdDeviationList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? smaList[i - 1] : 0;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (currentStdDeviation * stdDevMult);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (currentStdDeviation * stdDevMult);
            lowerBandList.AddRounded(lowerBand);

            Signal signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, upperBand, prevUpperBand, lowerBand, prevLowerBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", smaList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.BollingerBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive price zone indicator.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="pct">The PCT.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptivePriceZoneIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 20, decimal pct = 2)
    {
        List<decimal> xHLList = new();
        List<decimal> outerUpBandList = new();
        List<decimal> outerDnBandList = new();
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        int nP = MinOrMax((int)Math.Ceiling(Sqrt(length)));

        var ema1List = GetMovingAverageList(stockData, maType, nP, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, nP, ema1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];

            decimal xHL = currentHigh - currentLow;
            xHLList.AddRounded(xHL);
        }

        var xHLEma1List = GetMovingAverageList(stockData, maType, nP, xHLList);
        var xHLEma2List = GetMovingAverageList(stockData, maType, nP, xHLEma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal xVal1 = ema2List[i];
            decimal xVal2 = xHLEma2List[i];

            decimal prevUpBand = outerUpBandList.LastOrDefault();
            decimal outerUpBand = (pct * xVal2) + xVal1;
            outerUpBandList.AddRounded(outerUpBand);

            decimal prevDnBand = outerDnBandList.LastOrDefault();
            decimal outerDnBand = xVal1 - (pct * xVal2);
            outerDnBandList.AddRounded(outerDnBand);

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (outerUpBand + outerDnBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, outerUpBand,
                prevUpBand, outerDnBand, prevDnBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", outerUpBandList },
            { "MiddleBand", middleBandList },
            { "LowerBand", outerDnBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.AdaptivePriceZoneIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Auto Dispersion Bands
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAutoDispersionBands(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
        int length = 90, int smoothLength = 140)
    {
        List<decimal> middleBandList = new();
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> aMaxList = new();
        List<decimal> bMinList = new();
        List<decimal> x2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal x = currentValue - prevValue;

            decimal x2 = x * x;
            x2List.AddRounded(x2);

            decimal x2Sma = x2List.TakeLastExt(length).Average();
            decimal sq = x2Sma >= 0 ? Sqrt(x2Sma) : 0;

            decimal a = currentValue + sq;
            aList.AddRounded(a);

            decimal b = currentValue - sq;
            bList.AddRounded(b);

            decimal aMax = aList.TakeLastExt(length).Max();
            aMaxList.AddRounded(aMax);

            decimal bMin = bList.TakeLastExt(length).Min();
            bMinList.AddRounded(bMin);
        }

        var aMaList = GetMovingAverageList(stockData, maType, length, aMaxList);
        var upperBandList = GetMovingAverageList(stockData, maType, smoothLength, aMaList);
        var bMaList = GetMovingAverageList(stockData, maType, length, bMinList);
        var lowerBandList = GetMovingAverageList(stockData, maType, smoothLength, bMaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal upperBand = upperBandList[i];
            decimal lowerBand = lowerBandList[i];
            decimal prevUpperBand = i >= 1 ? upperBandList[i - 1] : 0;
            decimal prevLowerBand = i >= 1 ? lowerBandList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (upperBand + lowerBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, upperBand,
                prevUpperBand, lowerBand, prevLowerBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", middleBandList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.AutoDispersionBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bollinger Bands Fibonacci Ratios
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="fibRatio1"></param>
    /// <param name="fibRatio2"></param>
    /// <param name="fibRatio3"></param>
    /// <returns></returns>
    public static StockData CalculateBollingerBandsFibonacciRatios(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20, decimal fibRatio1 = 1.618m, decimal fibRatio2 = 2.618m, decimal fibRatio3 = 4.236m)
    {
        List<decimal> fibTop3List = new();
        List<decimal> fibBottom3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal atr = atrList[i];
            decimal sma = smaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;
            decimal r1 = atr * fibRatio1;
            decimal r2 = atr * fibRatio2;
            decimal r3 = atr * fibRatio3;

            decimal prevFibTop3 = fibTop3List.LastOrDefault();
            decimal fibTop3 = sma + r3;
            fibTop3List.AddRounded(fibTop3);

            decimal fibTop2 = sma + r2;
            decimal fibTop1 = sma + r1;
            decimal fibBottom1 = sma - r1;
            decimal fibBottom2 = sma - r2;

            decimal prevFibBottom3 = fibBottom3List.LastOrDefault();
            decimal fibBottom3 = sma - r3;
            fibBottom3List.AddRounded(fibBottom3);

            var signal = GetBollingerBandsSignal(currentValue - sma, prevValue - prevSma, currentValue, prevValue, fibTop3, prevFibTop3, 
                fibBottom3, prevFibBottom3);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", fibTop3List },
            { "MiddleBand", smaList },
            { "LowerBand", fibBottom3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.BollingerBandsFibonacciRatios;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bollinger Bands Average True Range
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="atrLength"></param>
    /// <param name="length"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateBollingerBandsAvgTrueRange(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int atrLength = 22, int length = 55, decimal stdDevMult = 2)
    {
        List<decimal> atrDevList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var bollingerBands = CalculateBollingerBands(stockData, maType, length, stdDevMult);
        var upperBandList = bollingerBands.OutputValues["UpperBand"];
        var lowerBandList = bollingerBands.OutputValues["LowerBand"];
        var emaList = GetMovingAverageList(stockData, maType, atrLength, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, atrLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentEma = emaList[i];
            decimal currentAtr = atrList[i];
            decimal upperBand = upperBandList[i];
            decimal lowerBand = lowerBandList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevEma = i >= 1 ? emaList[i - 1] : 0;
            decimal bbDiff = upperBand - lowerBand;

            decimal atrDev = bbDiff != 0 ? currentAtr / bbDiff : 0;
            atrDevList.AddRounded(atrDev);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, atrDev, 0.5m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "AtrDev", atrDevList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = atrDevList;
        stockData.IndicatorName = IndicatorName.BollingerBandsAverageTrueRange;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bollinger Bands using Atr Pct
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="bbLength"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateBollingerBandsWithAtrPct(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, int bbLength = 20, decimal stdDevMult = 2)
    {
        List<decimal> aptrList = new();
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        decimal ratio = (decimal)2 / (length + 1);

        var smaList = GetMovingAverageList(stockData, maType, bbLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal basis = smaList[i];
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal lh = currentHigh - currentLow;
            decimal hc = Math.Abs(currentHigh - prevValue);
            decimal lc = Math.Abs(currentLow - prevValue);
            decimal mm = Math.Max(Math.Max(lh, hc), lc);
            decimal prevBasis = i >= 1 ? smaList[i - 1] : 0;
            decimal atrs = mm == hc ? hc / (prevValue + (hc / 2)) : mm == lc ? lc / (currentLow + (lc / 2)) : mm == lh ? lh /
                (currentLow + (lh / 2)) : 0;

            decimal prevAptr = aptrList.LastOrDefault();
            decimal aptr = (100 * atrs * ratio) + (prevAptr * (1 - ratio));
            aptrList.AddRounded(aptr);

            decimal dev = stdDevMult * aptr;
            decimal prevUpper = upperList.LastOrDefault();
            decimal upper = basis + (basis * dev / 100);
            upperList.AddRounded(upper);

            decimal prevLower = lowerList.LastOrDefault();
            decimal lower = basis - (basis * dev / 100);
            lowerList.AddRounded(lower);

            var signal = GetBollingerBandsSignal(currentValue - basis, prevValue - prevBasis, currentValue, prevValue, upper, prevUpper, lower, prevLower);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperList },
            { "MiddleBand", smaList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.BollingerBandsWithAtrPct;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bollinger Bands %B
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="stdDevMult"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateBollingerBandsPercentB(this StockData stockData, decimal stdDevMult = 2,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<decimal> pctBList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var bbList = CalculateBollingerBands(stockData, maType, length, stdDevMult);
        var upperBandList = bbList.OutputValues["UpperBand"];
        var lowerBandList = bbList.OutputValues["LowerBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal upperBand = upperBandList[i];
            decimal lowerBand = lowerBandList[i];
            decimal prevPctB1 = i >= 1 ? pctBList[i - 1] : 0;
            decimal prevPctB2 = i >= 2 ? pctBList[i - 2] : 0;

            decimal pctB = upperBand - lowerBand != 0 ? (currentValue - lowerBand) / (upperBand - lowerBand) * 100 : 0;
            pctBList.AddRounded(pctB);

            Signal signal = GetRsiSignal(pctB - prevPctB1, prevPctB1 - prevPctB2, pctB, prevPctB1, 100, 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "PctB", pctBList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pctBList;
        stockData.IndicatorName = IndicatorName.BollingerBandsPercentB;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bollinger Bands Width
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="stdDevMult"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateBollingerBandsWidth(this StockData stockData, decimal stdDevMult = 2,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<decimal> bbWidthList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var bbList = CalculateBollingerBands(stockData, maType, length, stdDevMult);
        var upperBandList = bbList.OutputValues["UpperBand"];
        var lowerBandList = bbList.OutputValues["LowerBand"];
        var middleBandList = bbList.OutputValues["MiddleBand"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal upperBand = upperBandList[i];
            decimal lowerBand = lowerBandList[i];
            decimal middleBand = middleBandList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? middleBandList[i - 1] : 0;

            decimal prevBbWidth = bbWidthList.LastOrDefault();
            decimal bbWidth = middleBand != 0 ? (upperBand - lowerBand) / middleBand : 0;
            bbWidthList.AddRounded(bbWidth);

            Signal signal = GetVolatilitySignal(currentValue - middleBand, prevValue - prevMiddleBand, bbWidth, prevBbWidth);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "BbWidth", bbWidthList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bbWidthList;
        stockData.IndicatorName = IndicatorName.BollingerBandsWidth;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vervoort Modified Bollinger Band Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="inputName"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothLength"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateVervoortModifiedBollingerBandIndicator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.TripleExponentialMovingAverage, InputName inputName = InputName.FullTypicalPrice, int length1 = 18,
        int length2 = 200, int smoothLength = 8, decimal stdDevMult = 1.6m)
    {
        List<decimal> haOpenList = new();
        List<decimal> hacList = new();
        List<decimal> zlhaList = new();
        List<decimal> percbList = new();
        List<decimal> ubList = new();
        List<decimal> lbList = new();
        List<decimal> percbSignalList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _, _) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentValue = inputList[i];
            decimal prevOhlc = i >= 1 ? inputList[i - 1] : 0;

            decimal prevHaOpen = haOpenList.LastOrDefault();
            decimal haOpen = (prevOhlc + prevHaOpen) / 2;
            haOpenList.AddRounded(haOpen);

            decimal haC = (currentValue + haOpen + Math.Max(currentHigh, haOpen) + Math.Min(currentLow, haOpen)) / 4;
            hacList.AddRounded(haC);
        }

        var tma1List = GetMovingAverageList(stockData, maType, smoothLength, hacList);
        var tma2List = GetMovingAverageList(stockData, maType, smoothLength, tma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tma1 = tma1List[i];
            decimal tma2 = tma2List[i];
            decimal diff = tma1 - tma2;

            decimal zlha = tma1 + diff;
            zlhaList.AddRounded(zlha);
        }

        var zlhaTemaList = GetMovingAverageList(stockData, maType, smoothLength, zlhaList);
        stockData.CustomValuesList = zlhaTemaList;
        var zlhaTemaStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
        var wmaZlhaTemaList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, length1, zlhaTemaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal zihaTema = zlhaTemaList[i];
            decimal zihaTemaStdDev = zlhaTemaStdDevList[i];
            decimal wmaZihaTema = wmaZlhaTemaList[i];

            decimal percb = zihaTemaStdDev != 0 ? (zihaTema + (2 * zihaTemaStdDev) - wmaZihaTema) / (4 * zihaTemaStdDev) * 100 : 0;
            percbList.AddRounded(percb);
        }

        stockData.CustomValuesList = percbList;
        var percbStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length2).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = percbList[i];
            decimal percbStdDev = percbStdDevList[i];
            decimal prevValue = i >= 1 ? percbList[i - 1] : 0;

            decimal prevUb = ubList.LastOrDefault();
            decimal ub = 50 + (stdDevMult * percbStdDev);
            ubList.AddRounded(ub);

            decimal prevLb = lbList.LastOrDefault();
            decimal lb = 50 - (stdDevMult * percbStdDev);
            lbList.AddRounded(lb);

            decimal prevPercbSignal = percbSignalList.LastOrDefault();
            decimal percbSignal = (ub + lb) / 2;
            percbSignalList.AddRounded(percbSignal);

            Signal signal = GetBollingerBandsSignal(currentValue - percbSignal, prevValue - prevPercbSignal, currentValue,
                    prevValue, ub, prevUb, lb, prevLb);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", ubList },
            { "MiddleBand", percbList },
            { "LowerBand", lbList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.VervoortModifiedBollingerBandIndicator;

        return stockData;
    }
}