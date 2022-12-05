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
        List<double> dmoList = new();
        List<double> highestList = new();
        List<double> lowestList = new();
        List<Signal> signalsList = new();

        var stochList = CalculateStochasticOscillator(stockData, maType, length: length1, smoothLength1: length1, smoothLength2: length2);
        var stochSmaList = stochList.OutputValues["FastD"];
        var smaValList = stochList.OutputValues["SlowD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            double smaVal = smaValList[i];
            double stochSma = stochSmaList[i];
            double prevDmo1 = i >= 1 ? dmoList[i - 1] : 0;
            double prevDmo2 = i >= 2 ? dmoList[i - 2] : 0;

            double prevHighest = highestList.LastOrDefault();
            double highest = stochSma > prevHighest ? stochSma : prevHighest;
            highestList.AddRounded(highest);

            double prevLowest = i >= 1 ? lowestList.LastOrDefault() : double.MaxValue;
            double lowest = stochSma < prevLowest ? stochSma : prevLowest;
            lowestList.AddRounded(lowest);

            double midpoint = MinOrMax((lowest + highest) / 2, 100, 0);
            double dmo = MinOrMax(midpoint - (smaVal - stochSma), 100, 0);
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
        List<double> pmoList = new();
        List<double> rocMaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double sc1 = 2 / (double)length1;
        double sc2 = 2 / (double)length2;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double roc = prevValue != 0 ? MinPastValues(i, 1, currentValue - prevValue) / prevValue * 100 : 0;

            double prevRocMa1 = rocMaList.LastOrDefault();
            double rocMa = prevRocMa1 + ((roc - prevRocMa1) * sc1);
            rocMaList.AddRounded(rocMa);

            double prevPmo = pmoList.LastOrDefault();
            double pmo = prevPmo + (((rocMa * 10) - prevPmo) * sc2);
            pmoList.AddRounded(pmo);
        }

        var pmoSignalList = GetMovingAverageList(stockData, maType, signalLength, pmoList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double pmo = pmoList[i];
            double prevPmo = i >= 1 ? pmoList[i - 1] : 0;
            double pmoSignal = pmoSignalList[i];
            double prevPmoSignal = i >= 1 ? pmoSignalList[i - 1] : 0;

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
        List<double> tempList = new();
        List<double> amomList = new();
        List<double> amomsList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int p = MinOrMax((2 * momentumLength) + 1);

        var emaList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentEma = emaList[i];

            double currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            double sma = tempList.TakeLastExt(p).Average();
            double prevAmom = amomList.LastOrDefault();
            double amom = sma != 0 ? 100 * ((currentEma / sma) - 1) : 0;
            amomList.AddRounded(amom);

            double prevAmoms = amomsList.LastOrDefault();
            double amoms = amomList.TakeLastExt(signalLength).Average();
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
        int length5 = 50, int length6 = 200, double stdDevMult = 1.5)
    {
        List<double> utmList = new();
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
            double mo = moList[i];
            double bbPct = bbPctList[i];
            double mfi1 = mfi1List[i];
            double mfi2 = mfi2List[i];
            double mfi3 = mfi3List[i];
            double advSum = advSumList[i];
            double decSum = decSumList[i];
            double ratio = decSum != 0 ? advSum / decSum : 0;

            double utm = (200 * bbPct) + (100 * ratio) + (2 * mo) + (1.5 * mfi3) + (3 * mfi2) + (3 * mfi1);
            utmList.AddRounded(utm);
        }

        stockData.CustomValuesList = utmList;
        var utmRsiList = CalculateRelativeStrengthIndex(stockData, maType, length1, length1).CustomValuesList;
        var utmiList = GetMovingAverageList(stockData, maType, length1, utmRsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double utmi = utmiList[i];
            double prevUtmi1 = i >= 1 ? utmiList[i - 1] : 0;
            double prevUtmi2 = i >= 2 ? utmiList[i - 2] : 0;

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
        List<double> cpmoList = new();
        List<Signal> signalsList = new();

        if (stockData.Count == marketDataClass.InputValues.Count)
        {
            var pmoList = CalculatePriceMomentumOscillator(stockData, maType, length1, length2, signalLength).CustomValuesList;
            var spPmoList = CalculatePriceMomentumOscillator(marketDataClass, maType, length1, length2, signalLength).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                double pmo = pmoList[i];
                double spPmo = spPmoList[i];

                double prevCpmo = cpmoList.LastOrDefault();
                double cpmo = pmo - spPmo;
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
        List<double> cumoList = new();
        List<double> cumoSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevMa = i >= 1 ? maList[i - 1] : 0;

            double cumo = currentValue > prevMa ? 1 : currentValue < prevMa ? -1 : 0;
            cumoList.AddRounded(cumo);

            double cumoSum = cumoList.Sum();
            cumoSumList.AddRounded(cumoSum);
        }

        stockData.CustomValuesList = cumoSumList;
        var rocList = CalculateRateOfChange(stockData, smoothLength).CustomValuesList;
        var tlmoList = GetMovingAverageList(stockData, maType, smoothLength, rocList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double tlmo = tlmoList[i];
            double prevTlmo1 = i >= 1 ? tlmoList[i - 1] : 0;
            double prevTlmo2 = i >= 2 ? tlmoList[i - 2] : 0;

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
        List<double> momentumOscillatorList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentPrice = inputList[i];
            double prevPrice = i >= length ? inputList[i - length] : 0;

            double momentumOscillator = prevPrice != 0 ? currentPrice / prevPrice * 100 : 0;
            momentumOscillatorList.AddRounded(momentumOscillator);
        }

        var emaList = GetMovingAverageList(stockData, maType, length, momentumOscillatorList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double momentum = emaList[i];
            double prevMomentum = i >= 1 ? emaList[i - 1] : 0;

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
        List<double> rsiList = new();
        List<double> lossList = new();
        List<double> gainList = new();
        List<double> rsiHistogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length2 ? inputList[i - length2] : 0;
            double priceChg = MinPastValues(i, length2, currentValue - prevValue);

            double loss = i >= length2 && priceChg < 0 ? Math.Abs(priceChg) : 0;
            lossList.AddRounded(loss);

            double gain = i >= length2 && priceChg > 0 ? priceChg : 0;
            gainList.AddRounded(gain);
        }

        var avgGainList = GetMovingAverageList(stockData, maType, length1, gainList);
        var avgLossList = GetMovingAverageList(stockData, maType, length1, lossList);
        for (int i = 0; i < inputList.Count; i++)
        {
            double avgGain = avgGainList[i];
            double avgLoss = avgLossList[i];
            double rs = avgLoss != 0 ? avgGain / avgLoss : 0;

            double rsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
            rsiList.AddRounded(rsi);
        }

        var rsiSignalList = GetMovingAverageList(stockData, maType, length1, rsiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double rsi = rsiList[i];
            double rsiSignal = rsiSignalList[i];
            double prevRsi = i >= 1 ? rsiList[i - 1] : 0;

            double prevRsiHistogram = rsiHistogramList.LastOrDefault();
            double rsiHistogram = rsi - rsiSignal;
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
        List<double> pmol2List = new();
        List<double> pmolList = new();
        List<double> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double smPmol2 = (double)2 / length1;
        double smPmol = (double)2 / length2;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double ival = prevValue != 0 ? currentValue / prevValue * 100 : 100;
            double prevPmol = pmolList.LastOrDefault();
            double prevPmol2 = pmol2List.LastOrDefault();

            double pmol2 = ((ival - 100 - prevPmol2) * smPmol2) + prevPmol2;
            pmol2List.AddRounded(pmol2);

            double pmol = (((10 * pmol2) - prevPmol) * smPmol) + prevPmol;
            pmolList.AddRounded(pmol);
        }

        var pmolsList = GetMovingAverageList(stockData, maType, signalLength, pmolList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double pmol = pmolList[i];
            double pmols = pmolsList[i];

            double prevD = dList.LastOrDefault();
            double d = pmol - pmols;
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
        List<double> lossList = new();
        List<double> gainList = new();
        List<double> dmiSmaList = new();
        List<double> dmiSignalSmaList = new();
        List<double> dmiHistogramSmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var standardDeviationList = CalculateStandardDeviationVolatility(stockData, maType, length1).CustomValuesList;
        var stdDeviationSmaList = GetMovingAverageList(stockData, maType, length2, standardDeviationList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double asd = stdDeviationSmaList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

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
            double priceChg = MinPastValues(i, 1, currentValue - prevValue);

            double loss = i >= 1 && priceChg < 0 ? Math.Abs(priceChg) : 0;
            lossList.AddRounded(loss);

            double gain = i >= 1 && priceChg > 0 ? priceChg : 0;
            gainList.AddRounded(gain);

            double avgGainSma = gainList.TakeLastExt(dmiLength).Average();
            double avgLossSma = lossList.TakeLastExt(dmiLength).Average();
            double rsSma = avgLossSma != 0 ? avgGainSma / avgLossSma : 0;

            double prevDmiSma = dmiSmaList.LastOrDefault();
            double dmiSma = avgLossSma == 0 ? 100 : avgGainSma == 0 ? 0 : 100 - (100 / (1 + rsSma));
            dmiSmaList.AddRounded(dmiSma);

            double dmiSignalSma = dmiSmaList.TakeLastExt(dmiLength).Average();
            dmiSignalSmaList.AddRounded(dmiSignalSma);

            double prevDmiHistogram = dmiHistogramSmaList.LastOrDefault();
            double dmiHistogramSma = dmiSma - dmiSignalSma;
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
        List<double> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double midprice = (highest + lowest) / 2;
            double sma = smaList[i];
            double midpriceSmaAvg = (midprice + sma) / 2;

            double diff = currentValue - midpriceSmaAvg;
            diffList.AddRounded(diff);
        }

        stockData.CustomValuesList = diffList;
        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double predictedToday = linregList[i];
            double prevPredictedToday = i >= 1 ? linregList[i - 1] : 0;

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
