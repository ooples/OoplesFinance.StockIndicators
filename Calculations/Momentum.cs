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
    /// Calculates the Dynamic Momentum Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateDynamicMomentumOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 10,
        int length2 = 20)
    {
        List<decimal> dmoList = new();
        List<decimal> highestList = new();
        List<decimal> lowestList = new();
        List<Signal> signalsList = new();

        var stochList = CalculateStochasticOscillator(stockData, maType, length: length1, smoothLength1: length1, smoothLength2: length2);
        var stochSmaList = stochList.OutputValues["FastD"];
        var smaValList = stochList.OutputValues["SlowD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal smaVal = smaValList[i];
            decimal stochSma = stochSmaList[i];
            decimal prevDmo1 = i >= 1 ? dmoList[i - 1] : 0;
            decimal prevDmo2 = i >= 2 ? dmoList[i - 2] : 0;

            decimal prevHighest = highestList.LastOrDefault();
            decimal highest = stochSma > prevHighest ? stochSma : prevHighest;
            highestList.AddRounded(highest);

            decimal prevLowest = i >= 1 ? lowestList.LastOrDefault() : decimal.MaxValue;
            decimal lowest = stochSma < prevLowest ? stochSma : prevLowest;
            lowestList.AddRounded(lowest);

            decimal midpoint = MinOrMax((lowest + highest) / 2, 100, 0);
            decimal dmo = MinOrMax(midpoint - (smaVal - stochSma), 100, 0);
            dmoList.AddRounded(dmo);

            var signal = GetRsiSignal(dmo - prevDmo1, prevDmo1 - prevDmo2, dmo, prevDmo1, 77, 23);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dmo", dmoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dmoList;
        stockData.IndicatorName = IndicatorName.DynamicMomentumOscillator;

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
    public static StockData CalculatePriceMomentumOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 35,
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal roc = prevValue != 0 ? (currentValue - prevValue) / prevValue * 100 : 0;

            decimal prevRocMa1 = rocMaList.LastOrDefault();
            decimal rocMa = prevRocMa1 + ((roc - prevRocMa1) * sc1);
            rocMaList.AddRounded(rocMa);

            decimal prevPmo = pmoList.LastOrDefault();
            decimal pmo = prevPmo + (((rocMa * 10) - prevPmo) * sc2);
            pmoList.AddRounded(pmo);
        }

        var pmoSignalList = GetMovingAverageList(stockData, maType, signalLength, pmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pmo = pmoList[i];
            decimal prevPmo = i >= 1 ? pmoList[i - 1] : 0;
            decimal pmoSignal = pmoSignalList[i];
            decimal prevPmoSignal = i >= 1 ? pmoSignalList[i - 1] : 0;

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
    /// Calculates the Anchored Momentum
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="smoothLength">Length of the smooth.</param>
    /// <param name="signalLength">Length of the signal.</param>
    /// <param name="momentumLength">Length of the momentum.</param>
    /// <returns></returns>
    public static StockData CalculateAnchoredMomentum(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int smoothLength = 7,
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
            decimal currentEma = emaList[i];

            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal sma = tempList.TakeLastExt(p).Average();
            decimal prevAmom = amomList.LastOrDefault();
            decimal amom = sma != 0 ? 100 * ((currentEma / sma) - 1) : 0;
            amomList.AddRounded(amom);

            decimal prevAmoms = amomsList.LastOrDefault();
            decimal amoms = amomList.TakeLastExt(signalLength).Average();
            amomsList.AddRounded(amoms);

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
            decimal mo = moList[i];
            decimal bbPct = bbPctList[i];
            decimal mfi1 = mfi1List[i];
            decimal mfi2 = mfi2List[i];
            decimal mfi3 = mfi3List[i];
            decimal advSum = advSumList[i];
            decimal decSum = decSumList[i];
            decimal ratio = decSum != 0 ? advSum / decSum : 0;

            decimal utm = (200 * bbPct) + (100 * ratio) + (2 * mo) + (1.5m * mfi3) + (3 * mfi2) + (3 * mfi1);
            utmList.AddRounded(utm);
        }

        stockData.CustomValuesList = utmList;
        var utmRsiList = CalculateRelativeStrengthIndex(stockData, maType, length1, length1).CustomValuesList;
        var utmiList = GetMovingAverageList(stockData, maType, length1, utmRsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal utmi = utmiList[i];
            decimal prevUtmi1 = i >= 1 ? utmiList[i - 1] : 0;
            decimal prevUtmi2 = i >= 2 ? utmiList[i - 2] : 0;

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
    /// Calculates the Compare Price Momentum Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="marketDataClass"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateComparePriceMomentumOscillator(this StockData stockData, StockData marketDataClass,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 20, int length2 = 35, int signalLength = 10)
    {
        List<decimal> cpmoList = new();
        List<Signal> signalsList = new();

        if (stockData.Count == marketDataClass.InputValues.Count)
        {
            var pmoList = CalculatePriceMomentumOscillator(stockData, maType, length1, length2, signalLength).CustomValuesList;
            var spPmoList = CalculatePriceMomentumOscillator(marketDataClass, maType, length1, length2, signalLength).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal pmo = pmoList[i];
                decimal spPmo = spPmoList[i];

                decimal prevCpmo = cpmoList.LastOrDefault();
                decimal cpmo = pmo - spPmo;
                cpmoList.AddRounded(cpmo);

                var signal = GetCompareSignal(cpmo, prevCpmo);
                signalsList.Add(signal);
            }
        }

        stockData.OutputValues = new()
        {
            { "Cpmo", cpmoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cpmoList;
        stockData.IndicatorName = IndicatorName.ComparePriceMomentumOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Tick Line Momentum Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateTickLineMomentumOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 10, int smoothLength = 5)
    {
        List<decimal> cumoList = new();
        List<decimal> cumoSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevMa = i >= 1 ? maList[i - 1] : 0;

            decimal cumo = currentValue > prevMa ? 1 : currentValue < prevMa ? -1 : 0;
            cumoList.AddRounded(cumo);

            decimal cumoSum = cumoList.Sum();
            cumoSumList.AddRounded(cumoSum);
        }

        stockData.CustomValuesList = cumoSumList;
        var rocList = CalculateRateOfChange(stockData, smoothLength).CustomValuesList;
        var tlmoList = GetMovingAverageList(stockData, maType, smoothLength, rocList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tlmo = tlmoList[i];
            decimal prevTlmo1 = i >= 1 ? tlmoList[i - 1] : 0;
            decimal prevTlmo2 = i >= 2 ? tlmoList[i - 2] : 0;

            var signal = GetRsiSignal(tlmo - prevTlmo1, prevTlmo1 - prevTlmo2, tlmo, prevTlmo1, 5, -5);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tlmo", tlmoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tlmoList;
        stockData.IndicatorName = IndicatorName.TickLineMomentumOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Momentum Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMomentumOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14)
    {
        List<decimal> momentumOscillatorList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList[i];
            decimal prevPrice = i >= length ? inputList[i - length] : 0;

            decimal momentumOscillator = prevPrice != 0 ? currentPrice / prevPrice * 100 : 0;
            momentumOscillatorList.AddRounded(momentumOscillator);
        }

        var emaList = GetMovingAverageList(stockData, maType, length, momentumOscillatorList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal momentum = emaList[i];
            decimal prevMomentum = i >= 1 ? emaList[i - 1] : 0;

            var signal = GetCompareSignal(momentum, prevMomentum);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mo", momentumOscillatorList },
            { "Signal", emaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = momentumOscillatorList;
        stockData.IndicatorName = IndicatorName.MomentumOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Relative Momentum Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateRelativeMomentumIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int length1 = 14, int length2 = 3)
    {
        List<decimal> rsiList = new();
        List<decimal> lossList = new();
        List<decimal> gainList = new();
        List<decimal> rsiHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length2 ? inputList[i - length2] : 0;
            decimal priceChg = currentValue - prevValue;

            decimal loss = priceChg < 0 ? Math.Abs(priceChg) : 0;
            lossList.AddRounded(loss);

            decimal gain = priceChg > 0 ? priceChg : 0;
            gainList.AddRounded(gain);
        }

        var avgGainList = GetMovingAverageList(stockData, maType, length1, gainList);
        var avgLossList = GetMovingAverageList(stockData, maType, length1, lossList);
        for (int i = 0; i < inputList.Count; i++)
        {
            decimal avgGain = avgGainList[i];
            decimal avgLoss = avgLossList[i];
            decimal rs = avgLoss != 0 ? avgGain / avgLoss : 0;

            decimal rsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.AddRounded(rsi);
        }

        var rsiSignalList = GetMovingAverageList(stockData, maType, length1, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList[i];
            decimal rsiSignal = rsiSignalList[i];
            decimal prevRsi = i >= 1 ? rsiList[i - 1] : 0;

            decimal prevRsiHistogram = rsiHistogramList.LastOrDefault();
            decimal rsiHistogram = rsi - rsiSignal;
            rsiHistogramList.AddRounded(rsiHistogram);

            var signal = GetRsiSignal(rsiHistogram, prevRsiHistogram, rsi, prevRsi, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rmi", rsiList },
            { "Signal", rsiSignalList },
            { "Histogram", rsiHistogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rsiList;
        stockData.IndicatorName = IndicatorName.RelativeMomentumIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Decision Point Price Momentum Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateDecisionPointPriceMomentumOscillator(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 35, int length2 = 20, int signalLength = 10)
    {
        List<decimal> pmol2List = new();
        List<decimal> pmolList = new();
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal smPmol2 = (decimal)2 / length1;
        decimal smPmol = (decimal)2 / length2;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ival = prevValue != 0 ? currentValue / prevValue * 100 : 100;
            decimal prevPmol = pmolList.LastOrDefault();
            decimal prevPmol2 = pmol2List.LastOrDefault();

            decimal pmol2 = ((ival - 100 - prevPmol2) * smPmol2) + prevPmol2;
            pmol2List.AddRounded(pmol2);

            decimal pmol = (((10 * pmol2) - prevPmol) * smPmol) + prevPmol;
            pmolList.AddRounded(pmol);
        }

        var pmolsList = GetMovingAverageList(stockData, maType, signalLength, pmolList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pmol = pmolList[i];
            decimal pmols = pmolsList[i];

            decimal prevD = dList.LastOrDefault();
            decimal d = pmol - pmols;
            dList.AddRounded(d);

            var signal = GetCompareSignal(d, prevD);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dppmo", pmolList },
            { "Signal", pmolsList },
            { "Histogram", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pmolList;
        stockData.IndicatorName = IndicatorName.DecisionPointPriceMomentumOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Dynamic Momentum Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="upLimit"></param>
    /// <param name="dnLimit"></param>
    /// <returns></returns>
    public static StockData CalculateDynamicMomentumIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 5,
        int length2 = 10, int length3 = 14, int upLimit = 30, int dnLimit = 5)
    {
        List<decimal> lossList = new();
        List<decimal> gainList = new();
        List<decimal> dmiSmaList = new();
        List<decimal> dmiSignalSmaList = new();
        List<decimal> dmiHistogramSmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var standardDeviationList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
        var stdDeviationSmaList = GetMovingAverageList(stockData, maType, length2, standardDeviationList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal asd = stdDeviationSmaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            int dTime;
            try
            {
                dTime = asd != 0 ? Math.Min(upLimit, (int)Math.Ceiling(length3 / asd)) : 0;
            }
            catch
            {
                dTime = upLimit;
            }

            int dmiLength = Math.Max(Math.Min(dTime, upLimit), dnLimit);
            decimal priceChg = currentValue - prevValue;

            decimal loss = priceChg < 0 ? Math.Abs(priceChg) : 0;
            lossList.AddRounded(loss);

            decimal gain = priceChg > 0 ? priceChg : 0;
            gainList.AddRounded(gain);

            decimal avgGainSma = gainList.TakeLastExt(dmiLength).Average();
            decimal avgLossSma = lossList.TakeLastExt(dmiLength).Average();
            decimal rsSma = avgLossSma != 0 ? avgGainSma / avgLossSma : 0;

            decimal prevDmiSma = dmiSmaList.LastOrDefault();
            decimal dmiSma = avgLossSma == 0 ? 100 : avgGainSma == 0 ? 0 : 100 - (100 / (1 + rsSma));
            dmiSmaList.AddRounded(dmiSma);

            decimal dmiSignalSma = dmiSmaList.TakeLastExt(dmiLength).Average();
            dmiSignalSmaList.AddRounded(dmiSignalSma);

            decimal prevDmiHistogram = dmiHistogramSmaList.LastOrDefault();
            decimal dmiHistogramSma = dmiSma - dmiSignalSma;
            dmiHistogramSmaList.AddRounded(dmiHistogramSma);

            var signal = GetRsiSignal(dmiHistogramSma, prevDmiHistogram, dmiSma, prevDmiSma, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dmi", dmiSmaList },
            { "Signal", dmiSignalSmaList },
            { "Histogram", dmiHistogramSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dmiSmaList;
        stockData.IndicatorName = IndicatorName.DynamicMomentumIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Squeeze Momentum Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSqueezeMomentumIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<decimal> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal midprice = (highest + lowest) / 2;
            decimal sma = smaList[i];
            decimal midpriceSmaAvg = (midprice + sma) / 2;

            decimal diff = currentValue - midpriceSmaAvg;
            diffList.AddRounded(diff);
        }

        stockData.CustomValuesList = diffList;
        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal predictedToday = linregList[i];
            decimal prevPredictedToday = i >= 1 ? linregList[i - 1] : 0;

            var signal = GetCompareSignal(predictedToday, prevPredictedToday);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Smi", linregList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = linregList;
        stockData.IndicatorName = IndicatorName.SqueezeMomentumIndicator;

        return stockData;
    }
}
