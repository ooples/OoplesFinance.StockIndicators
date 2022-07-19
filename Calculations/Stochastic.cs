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
    /// Calculates the Stochastic Oscillator
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="smoothLength1">Length of the K signal.</param>
    /// <param name="smoothLength2">Length of the D signal.</param>
    /// <returns></returns>
    public static StockData CalculateStochasticOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, int smoothLength1 = 3, int smoothLength2 = 3)
    {
        List<decimal> fastKList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];

            decimal fastK = highestHigh - lowestLow != 0 ? MinOrMax((currentValue - lowestLow) / (highestHigh - lowestLow) * 100, 100, 0) : 0;
            fastKList.AddRounded(fastK);
        }

        var fastDList = GetMovingAverageList(stockData, maType, smoothLength1, fastKList);
        var slowDList = GetMovingAverageList(stockData, maType, smoothLength2, fastDList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal slowK = fastDList[i];
            decimal slowD = slowDList[i];
            decimal prevSlowk = i >= 1 ? fastDList[i - 1] : 0;
            decimal prevSlowd = i >= 1 ? slowDList[i - 1] : 0;

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
            decimal er = erList[i];
            decimal src = srcList[i];
            decimal highest1 = highest1List[i];
            decimal lowest1 = lowest1List[i];
            decimal highest2 = highest2List[i];
            decimal lowest2 = lowest2List[i];
            decimal prevStc1 = i >= 1 ? stcList[i - 1] : 0;
            decimal prevStc2 = i >= 2 ? stcList[i - 2] : 0;
            decimal a = (er * highest1) + ((1 - er) * highest2);
            decimal b = (er * lowest1) + ((1 - er) * lowest2);

            decimal stc = a - b != 0 ? MinOrMax((src - b) / (a - b), 1, 0) : 0;
            stcList.AddRounded(stc);

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
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];

            decimal range = highest - lowest;
            rangeList.AddRounded(range);
        }

        var rangeSmaList = GetMovingAverageList(stockData, maType, length, rangeList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal rangeSma = rangeSmaList[i];

            decimal bull = rangeSma != 0 ? (sma / rangeSma) - (lowest / rangeSma) : 0;
            bullList.AddRounded(bull);

            decimal bear = rangeSma != 0 ? Math.Abs((sma / rangeSma) - (highest / rangeSma)) : 0;
            bearList.AddRounded(bear);

            decimal max = Math.Max(bull, bear);
            maxList.AddRounded(max);
        }

        var signalList = GetMovingAverageList(stockData, maType, signalLength, maxList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal bull = bullList[i];
            decimal bear = bearList[i];
            decimal sig = signalList[i];

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
                decimal hh = i >= j ? highestList[i - j] : 0;
                decimal ll = i >= j ? lowestList[i - j] : 0;
                decimal c = i >= j ? inputList[i - j] : 0;
                decimal range = hh - ll;
                decimal frac = range != 0 ? (c - ll) / range : 0;
                decimal ratio = 1 / Sqrt(j + 1);
                weightSum += frac * ratio;
                denomSum += ratio;
            }

            decimal rawNst = denomSum != 0 ? (200 * weightSum / denomSum) - 100 : 0;
            rawNstList.AddRounded(rawNst);
        }

        var nstList = GetMovingAverageList(stockData, maType, smoothLength, rawNstList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nst = nstList[i];
            decimal prevNst1 = i >= 1 ? nstList[i - 1] : 0;
            decimal prevNst2 = i >= 2 ? nstList[i - 2] : 0;

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
            decimal wma1 = wmaList[i];
            decimal wma2 = wma2List[i];
            decimal wma3 = wma3List[i];
            decimal wma4 = wma4List[i];
            decimal wma5 = wma5List[i];
            decimal wma6 = wma6List[i];
            decimal wma7 = wma7List[i];
            decimal wma8 = wma8List[i];
            decimal wma9 = wma9List[i];
            decimal wma10 = wma10List[i];

            decimal rbw = ((wma1 * 5) + (wma2 * 4) + (wma3 * 3) + (wma4 * 2) + wma5 + wma6 + wma7 + wma8 + wma9 + wma10) / 20;
            rbwList.AddRounded(rbw);
        }

        var (highestList, lowestList) = GetMaxAndMinValuesList(rbwList, stochLength);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal rbw = rbwList[i];
            decimal prevFtso1 = i >= 1 ? ftsoList[i - 1] : 0;
            decimal prevFtso2 = i >= 2 ? ftsoList[i - 2] : 0;

            decimal num = rbw - lowest;
            numList.AddRounded(num);

            decimal denom = highest - lowest;
            denomList.AddRounded(denom);

            decimal numSum = numList.TakeLastExt(smoothLength).Sum();
            decimal denomSum = denomList.TakeLastExt(smoothLength).Sum();
            decimal rbws = denomSum + 0.0001m != 0 ? MinOrMax(numSum / (denomSum + 0.0001m) * 100, 100, 0) : 0;
            decimal x = 0.1m * (rbws - 50);

            decimal ftso = MinOrMax((((Exp(2 * x) - 1) / (Exp(2 * x) + 1)) + 1) * 50, 100, 0);
            ftsoList.AddRounded(ftso);

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
            decimal v4 = v4List[i];
            decimal slowK = slowKList[i];

            decimal fsst = (500 * v4) + slowK;
            fsstList.AddRounded(fsst);
        }

        var wfsstList = GetMovingAverageList(stockData, maType, length4, fsstList);
        for (int i = 0; i < wfsstList.Count; i++)
        {
            decimal fsst = fsstList[i];
            decimal wfsst = wfsstList[i];
            decimal prevFsst = i >= 1 ? fsstList[i - 1] : 0;
            decimal prevWfsst = i >= 1 ? wfsstList[i - 1] : 0;

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
    /// Calculates the Premier Stochastic Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculatePremierStochasticOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 8, int smoothLength = 25)
    {
        List<decimal> nskList = new();
        List<decimal> psoList = new();
        List<Signal> signalsList = new();

        int len = MinOrMax((int)Math.Ceiling(Sqrt(smoothLength)));

        var stochasticRsiList = CalculateStochasticOscillator(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sk = stochasticRsiList[i];

            decimal nsk = 0.1m * (sk - 50);
            nskList.AddRounded(nsk);
        }

        var nskEmaList = GetMovingAverageList(stockData, maType, len, nskList);
        var ssList = GetMovingAverageList(stockData, maType, len, nskEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ss = ssList[i];
            decimal prevPso1 = i >= 1 ? psoList[i - 1] : 0;
            decimal prevPso2 = i >= 2 ? psoList[i - 2] : 0;
            decimal expss = Exp(ss);

            decimal pso = expss + 1 != 0 ? MinOrMax((expss - 1) / (expss + 1), 1, -1) : 0;
            psoList.AddRounded(pso);

            var signal = GetRsiSignal(pso - prevPso1, prevPso1 - prevPso2, pso, prevPso1, 0.9m, -0.9m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pso", psoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = psoList;
        stockData.IndicatorName = IndicatorName.PremierStochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fast Turbo Stochastics
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="turboLength"></param>
    /// <returns></returns>
    public static StockData CalculateTurboStochasticsFast(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 20, int length2 = 10, int turboLength = 2)
    {
        List<Signal> signalsList = new();

        int turbo = turboLength < 0 ? Math.Max(turboLength, length2 * -1) : turboLength > 0 ? Math.Min(turboLength, length2) : 0;

        var fastKList = CalculateStochasticOscillator(stockData, maType, length: length1).CustomValuesList;
        var fastDList = GetMovingAverageList(stockData, maType, length1, fastKList);
        stockData.CustomValuesList = fastKList;
        var tsfKList = CalculateLinearRegression(stockData, length2 + turbo).CustomValuesList;
        stockData.CustomValuesList = fastDList;
        var tsfDList = CalculateLinearRegression(stockData, length2 + turbo).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal tsfD = tsfDList[i];
            decimal tsfK = tsfKList[i];
            decimal prevTsfk = i >= 1 ? tsfKList[i - 1] : 0;
            decimal prevTsfd = i >= 1 ? tsfDList[i - 1] : 0;

            var signal = GetRsiSignal(tsfK - tsfD, prevTsfk - prevTsfd, tsfK, prevTsfk, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tsf", tsfKList },
            { "Signal", tsfDList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsfKList;
        stockData.IndicatorName = IndicatorName.TurboStochasticsFast;

        return stockData;
    }

    /// <summary>
    /// Calculates the Slow Turbo Stochastics
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="turboLength"></param>
    /// <returns></returns>
    public static StockData CalculateTurboStochasticsSlow(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 20, int length2 = 10, int turboLength = 2)
    {
        List<decimal> tssDList = new();
        List<decimal> tssKList = new();
        List<Signal> signalsList = new();

        int turbo = turboLength < 0 ? Math.Max(turboLength, length2 * -1) : turboLength > 0 ? Math.Min(turboLength, length2) : 0;

        var fastKList = CalculateStochasticOscillator(stockData, maType, length: length1).CustomValuesList;
        var slowKList = GetMovingAverageList(stockData, maType, length1, fastKList);
        var slowDList = GetMovingAverageList(stockData, maType, length1, slowKList);
        stockData.CustomValuesList = slowKList;
        var tsfKList = CalculateLinearRegression(stockData, length2 + turbo).CustomValuesList;
        stockData.CustomValuesList = slowDList;
        var tsfDList = CalculateLinearRegression(stockData, length2 + turbo).CustomValuesList;

        for (int i = 0; i < tssDList.Count; i++)
        {
            decimal tssD = tssDList[i];
            decimal tssK = tssKList[i];
            decimal prevTssk = i >= 1 ? tssKList[i - 1] : 0;
            decimal prevTssd = i >= 1 ? tssDList[i - 1] : 0;

            var signal = GetRsiSignal(tssK - tssD, prevTssk - prevTssd, tssK, prevTssk, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tsf", tsfKList },
            { "Signal", tsfDList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsfKList;
        stockData.IndicatorName = IndicatorName.TurboStochasticsSlow;

        return stockData;
    }

    /// <summary>
    /// Calculates the Recursive Stochastic
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static StockData CalculateRecursiveStochastic(this StockData stockData, int length = 200, decimal alpha = 0.1m)
    {
        List<decimal> kList = new();
        List<decimal> maList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal stoch = highest - lowest != 0 ? (currentValue - lowest) / (highest - lowest) * 100 : 0;
            decimal prevK1 = i >= 1 ? kList[i - 1] : 0;
            decimal prevK2 = i >= 2 ? kList[i - 2] : 0;

            decimal ma = (alpha * stoch) + ((1 - alpha) * prevK1);
            maList.AddRounded(ma);

            var lbList = maList.TakeLastExt(length).ToList();
            decimal highestMa = lbList.Max();
            decimal lowestMa = lbList.Min();

            decimal k = highestMa - lowestMa != 0 ? MinOrMax((ma - lowestMa) / (highestMa - lowestMa) * 100, 100, 0) : 0;
            kList.AddRounded(k);

            var signal = GetRsiSignal(k - prevK1, prevK1 - prevK2, k, prevK1, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsto", kList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kList;
        stockData.IndicatorName = IndicatorName.RecursiveStochastic;

        return stockData;
    }

    /// <summary>
    /// Calculates the DiNapoli Preferred Stochastic Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateDiNapoliPreferredStochasticOscillator(this StockData stockData, int length1 = 8, int length2 = 3, int length3 = 3)
    {
        List<decimal> rList = new();
        List<decimal> sList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal max = highestList[i];
            decimal min = lowestList[i];
            decimal fast = max - min != 0 ? MinOrMax((currentValue - min) / (max - min) * 100, 100, 0) : 0;

            decimal prevR = rList.LastOrDefault();
            decimal r = prevR + ((fast - prevR) / length2);
            rList.AddRounded(r);

            decimal prevS = sList.LastOrDefault();
            decimal s = prevS + ((r - prevS) / length3);
            sList.AddRounded(s);

            var signal = GetRsiSignal(r - s, prevR - prevS, r, prevR, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dpso", rList },
            { "Signal", sList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rList;
        stockData.IndicatorName = IndicatorName.DiNapoliPreferredStochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Double Smoothed Stochastic
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateDoubleSmoothedStochastic(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 2,
        int length2 = 3, int length3 = 15, int length4 = 3)
    {
        List<decimal> dssList = new();
        List<decimal> numList = new();
        List<decimal> denomList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];

            decimal num = currentValue - lowestLow;
            numList.AddRounded(num);

            decimal denom = highestHigh - lowestLow;
            denomList.AddRounded(denom);
        }

        var ssNumList = GetMovingAverageList(stockData, maType, length2, numList);
        var ssDenomList = GetMovingAverageList(stockData, maType, length2, denomList);
        var dsNumList = GetMovingAverageList(stockData, maType, length3, ssNumList);
        var dsDenomList = GetMovingAverageList(stockData, maType, length3, ssDenomList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dsNum = dsNumList[i];
            decimal dsDenom = dsDenomList[i];

            decimal dss = dsDenom != 0 ? MinOrMax(100 * dsNum / dsDenom, 100, 0) : 0;
            dssList.AddRounded(dss);
        }

        var sdssList = GetMovingAverageList(stockData, maType, length4, dssList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dss = dssList[i];
            decimal sdss = sdssList[i];
            decimal prevDss = i >= 1 ? dssList[i - 1] : 0;
            decimal prevSdss = i >= 1 ? sdssList[i - 1] : 0;

            var signal = GetRsiSignal(dss - sdss, prevDss - prevSdss, dss, prevDss, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dss", dssList },
            { "Signal", sdssList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dssList;
        stockData.IndicatorName = IndicatorName.DoubleSmoothedStochastic;

        return stockData;
    }

    /// <summary>
    /// Calculates the Double Stochastic Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateDoubleStochasticOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14,
        int smoothLength = 3)
    {
        List<decimal> doubleKList = new();
        List<Signal> signalsList = new();

        var stochasticList = CalculateStochasticOscillator(stockData, maType, length: length).CustomValuesList;
        var (highestList, lowestList) = GetMaxAndMinValuesList(stochasticList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal slowK = stochasticList[i];
            decimal highestSlowK = highestList[i];
            decimal lowestSlowK = lowestList[i];

            decimal doubleK = highestSlowK - lowestSlowK != 0 ? MinOrMax((slowK - lowestSlowK) / (highestSlowK - lowestSlowK) * 100, 100, 0) : 0;
            doubleKList.AddRounded(doubleK);
        }

        var doubleSlowKList = GetMovingAverageList(stockData, maType, smoothLength, doubleKList);
        var doubleKSignalList = GetMovingAverageList(stockData, maType, smoothLength, doubleSlowKList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal doubleSlowK = doubleSlowKList[i];
            decimal doubleKSignal = doubleKSignalList[i];
            decimal prevDoubleslowk = i >= 1 ? doubleSlowKList[i - 1] : 0;
            decimal prevDoubleKSignal = i >= 1 ? doubleKSignalList[i - 1] : 0;

            var signal = GetRsiSignal(doubleSlowK - doubleKSignal, prevDoubleslowk - prevDoubleKSignal, doubleSlowK, prevDoubleslowk, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dso", doubleSlowKList },
            { "Signal", doubleKSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = doubleSlowKList;
        stockData.IndicatorName = IndicatorName.DoubleStochasticOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the DMI Stochastic
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <param name="length4"></param>
    /// <returns></returns>
    public static StockData CalculateDMIStochastic(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 10,
        int length2 = 10, int length3 = 3, int length4 = 3)
    {
        List<decimal> dmiOscillatorList = new();
        List<decimal> fastKList = new();
        List<Signal> signalsList = new();

        var adxList = CalculateAverageDirectionalIndex(stockData, maType, length1);
        var diPlusList = adxList.OutputValues["DiPlus"];
        var diMinusList = adxList.OutputValues["DiMinus"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pdi = diPlusList[i];
            decimal ndi = diMinusList[i];

            decimal dmiOscillator = ndi - pdi;
            dmiOscillatorList.AddRounded(dmiOscillator);
        }

        var (highestList, lowestList) = GetMaxAndMinValuesList(dmiOscillatorList, length2);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal dmiOscillator = dmiOscillatorList[i];

            decimal fastK = highest - lowest != 0 ? MinOrMax((dmiOscillator - lowest) / (highest - lowest) * 100, 100, 0) : 0;
            fastKList.AddRounded(fastK);
        }

        var slowKList = GetMovingAverageList(stockData, maType, length3, fastKList);
        var dmiStochList = GetMovingAverageList(stockData, maType, length4, slowKList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dmiStoch = dmiStochList[i];
            decimal prevDmiStoch1 = i >= 1 ? dmiStochList[i - 1] : 0;
            decimal prevDmiStoch2 = i >= 2 ? dmiStochList[i - 2] : 0;

            var signal = GetRsiSignal(dmiStoch - prevDmiStoch1, prevDmiStoch1 - prevDmiStoch2, dmiStoch, prevDmiStoch1, 90, 10);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "DmiStochastic", dmiStochList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dmiStochList;
        stockData.IndicatorName = IndicatorName.DMIStochastic;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stochastic Momentum Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="smoothLength1"></param>
    /// <param name="smoothLength2"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticMomentumIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 2, int length2 = 8, int smoothLength1 = 5, int smoothLength2 = 5)
    {
        List<decimal> dList = new();
        List<decimal> hlList = new();
        List<decimal> smiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];
            decimal median = (highestHigh + lowestLow) / 2;

            decimal diff = currentValue - median;
            dList.AddRounded(diff);

            decimal highLow = highestHigh - lowestLow;
            hlList.AddRounded(highLow);
        }

        var dEmaList = GetMovingAverageList(stockData, maType, length2, dList);
        var hlEmaList = GetMovingAverageList(stockData, maType, length2, hlList);
        var dSmoothEmaList = GetMovingAverageList(stockData, maType, smoothLength1, dEmaList);
        var hlSmoothEmaList = GetMovingAverageList(stockData, maType, smoothLength1, hlEmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hlSmoothEma = hlSmoothEmaList[i];
            decimal dSmoothEma = dSmoothEmaList[i];
            decimal hl2 = hlSmoothEma / 2;

            decimal smi = hl2 != 0 ? MinOrMax(100 * dSmoothEma / hl2, 100, -100) : 0;
            smiList.AddRounded(smi);
        }

        var smiSignalList = GetMovingAverageList(stockData, maType, smoothLength2, smiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal smi = smiList[i];
            decimal smiSignal = smiSignalList[i];
            decimal prevSmi = i >= 1 ? smiList[i - 1] : 0;
            decimal prevSmiSignal = i >= 1 ? smiSignalList[i - 1] : 0;

            var signal = GetCompareSignal(smi - smiSignal, prevSmi - prevSmiSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Smi", smiList },
            { "Signal", smiSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = smiList;
        stockData.IndicatorName = IndicatorName.StochasticMomentumIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stochastic Fast Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength1"></param>
    /// <param name="smoothLength2"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticFastOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 14, int smoothLength1 = 3, int smoothLength2 = 2)
    {
        List<Signal> signalsList = new();

        var fastKList = CalculateStochasticOscillator(stockData, maType, length, smoothLength1, smoothLength2);
        var pkList = fastKList.OutputValues["FastD"];
        var pdList = fastKList.OutputValues["SlowD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pkEma = pkList[i];
            decimal pdEma = pdList[i];
            decimal prevPkema = i >= 1 ? pkList[i - 1] : 0;
            decimal prevPdema = i >= 1 ? pdList[i - 1] : 0;

            var signal = GetRsiSignal(pkEma - pdEma, prevPkema - prevPdema, pkEma, prevPkema, 80, 20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sfo", pkList },
            { "Signal", pdList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pkList;
        stockData.IndicatorName = IndicatorName.StochasticFastOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stochastic Custom Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticCustomOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 7,
        int length2 = 3, int length3 = 12)
    {
        List<decimal> numList = new();
        List<decimal> denomList = new();
        List<decimal> sckList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];

            decimal num = currentValue - lowestLow;
            numList.AddRounded(num);

            decimal denom = highestHigh - lowestLow;
            denomList.AddRounded(denom);
        }

        var numSmaList = GetMovingAverageList(stockData, maType, length2, numList);
        var denomSmaList = GetMovingAverageList(stockData, maType, length2, denomList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal numSma = numSmaList[i];
            decimal denomSma = denomSmaList[i];

            decimal sck = denomSma != 0 ? MinOrMax(numSma / denomSma * 100, 100, 0) : 0;
            sckList.AddRounded(sck);
        }

        var scdList = GetMovingAverageList(stockData, maType, length3, sckList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sck = sckList[i];
            decimal scd = scdList[i];
            decimal prevSck = i >= 1 ? sckList[i - 1] : 0;
            decimal prevScd = i >= 1 ? scdList[i - 1] : 0;

            var signal = GetRsiSignal(sck - scd, prevSck - prevScd, sck, prevSck, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sco", sckList },
            { "Signal", scdList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sckList;
        stockData.IndicatorName = IndicatorName.StochasticCustomOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stochastic Regular
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateStochasticRegular(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 5, 
        int length2 = 3)
    {
        List<Signal> signalsList = new();

        var stoList = CalculateStochasticOscillator(stockData, maType, length1, length2, length2);
        var fastKList = stoList.CustomValuesList;
        var skList = stoList.OutputValues["FastD"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fk = fastKList[i];
            decimal sk = skList[i];
            decimal prevFk = i >= 1 ? fastKList[i - 1] : 0;
            decimal prevSk = i >= 1 ? skList[i - 1] : 0;

            var signal = GetRsiSignal(fk - sk, prevFk - prevSk, fk, prevFk, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sco", fastKList },
            { "Signal", skList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fastKList;
        stockData.IndicatorName = IndicatorName.StochasticRegular;

        return stockData;
    }

    /// <summary>
    /// Calculates the Swami Stochastics
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateSwamiStochastics(this StockData stockData, int fastLength = 12, int slowLength = 48)
    {
        List<decimal> numList = new();
        List<decimal> denomList = new();
        List<decimal> stochList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, slowLength - fastLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal prevStoch1 = i >= 1 ? stochList[i - 1] : 0;
            decimal prevStoch2 = i >= 2 ? stochList[i - 2] : 0;

            decimal pNum = numList.LastOrDefault();
            decimal num = (currentValue - lowest + pNum) / 2;
            numList.AddRounded(num);

            decimal pDenom = denomList.LastOrDefault();
            decimal denom = (highest - lowest + pDenom) / 2;
            denomList.AddRounded(denom);

            decimal stoch = denom != 0 ? MinOrMax((0.2m * num / denom) + (0.8m * prevStoch1), 1, 0) : 0;
            stochList.AddRounded(stoch);

            var signal = GetRsiSignal(stoch - prevStoch1, prevStoch1 - prevStoch2, stoch, prevStoch1, 0.8m, 0.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ss", stochList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stochList;
        stockData.IndicatorName = IndicatorName.SwamiStochastics;

        return stockData;
    }
}
