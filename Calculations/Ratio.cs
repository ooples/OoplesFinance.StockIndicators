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
    /// Calculates the Martin Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateMartinRatio(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 30, double bmk = 0.02m)
    {
        List<double> martinList = new();
        List<double> benchList = new();
        List<double> retList = new();

        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double barMin = 60 * 24;
        double minPerYr = 60 * 24 * 30 * 12;
        double barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;

            double bench = Pow(1 + bmk, length / barsPerYr) - 1;
            benchList.AddRounded(bench);

            double ret = prevValue != 0 ? (100 * (currentValue / prevValue)) - 1 - (bench * 100) : 0;
            retList.AddRounded(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        stockData.CustomValuesList = retList;
        var ulcerIndexList = CalculateUlcerIndex(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double ulcerIndex = ulcerIndexList[i];
            double retSma = retSmaList[i];

            double prevMartin = martinList.LastOrDefault();
            double martin = ulcerIndex != 0 ? retSma / ulcerIndex : 0;
            martinList.AddRounded(martin);

            var signal = GetCompareSignal(martin - 2, prevMartin - 2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mr", martinList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = martinList;
        stockData.IndicatorName = IndicatorName.MartinRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Upside Potential Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateUpsidePotentialRatio(this StockData stockData, int length = 30, double bmk = 0.05m)
    {
        List<double> retList = new();
        List<double> upsidePotentialList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double barMin = 60 * 24;
        double minPerYr = 60 * 24 * 30 * 12;
        double barsPerYr = minPerYr / barMin;
        double ratio = (double)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double bench = Pow(1 + bmk, length / barsPerYr) - 1;

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);

            double downSide = 0, upSide = 0;
            for (int j = 0; j < length; j++)
            {
                double iValue = i >= j ? retList[i - j] : 0;
                downSide += iValue < bench ? Pow(iValue - bench, 2) * ratio : 0;
                upSide += iValue > bench ? (iValue - bench) * ratio : 0;
            }

            double prevUpsidePotential = upsidePotentialList.LastOrDefault();
            double upsidePotential = downSide >= 0 ? upSide / Sqrt(downSide) : 0;
            upsidePotentialList.AddRounded(upsidePotential);

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
    /// Calculates the Information Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateInformationRatio(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 30, double bmk = 0.05m)
    {
        List<double> infoList = new();
        List<double> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double barMin = 60 * 24;
        double minPerYr = 60 * 24 * 30 * 12;
        double barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);
        }

        stockData.CustomValuesList = retList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double stdDeviation = stdDevList[i];
            double retSma = retSmaList[i];
            double bench = Pow(1 + bmk, length / barsPerYr) - 1;

            double prevInfo = infoList.LastOrDefault();
            double info = stdDeviation != 0 ? (retSma - bench) / stdDeviation : 0;
            infoList.AddRounded(info);

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
    /// Calculates the Omega Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateOmegaRatio(this StockData stockData, int length = 30, double bmk = 0.05m)
    {
        List<double> omegaList = new();
        List<double> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double barMin = 60 * 24;
        double minPerYr = 60 * 24 * 30 * 12;
        double barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double bench = Pow(1 + bmk, length / barsPerYr) - 1;

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);

            double downSide = 0, upSide = 0;
            for (int j = 0; j < length; j++)
            {
                double iValue = i >= j ? retList[i - j] : 0;
                downSide += iValue < bench ? bench - iValue : 0;
                upSide += iValue > bench ? iValue - bench : 0;
            }

            double prevOmega = omegaList.LastOrDefault();
            double omega = downSide != 0 ? upSide / downSide : 0;
            omegaList.AddRounded(omega);

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
    /// Calculates the Volatility Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="breakoutLevel"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilityRatio(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14, double breakoutLevel = 0.5m)
    {
        List<double> vrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length - 1);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double currentEma = emaList[i];
            double prevHighest = i >= 1 ? highestList[i - 1] : 0;
            double prevLowest = i >= 1 ? lowestList[i - 1] : 0;
            double priorValue = i >= length + 1 ? inputList[i - (length + 1)] : 0;
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevEma = i >= 1 ? emaList[i - 1] : 0;
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            double max = priorValue != 0 ? Math.Max(prevHighest, priorValue) : prevHighest;
            double min = priorValue != 0 ? Math.Min(prevLowest, priorValue) : prevLowest;

            double vr = max - min != 0 ? tr / (max - min) : 0;
            vrList.AddRounded(vr);

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
    /// Calculates the Calmar Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateCalmarRatio(this StockData stockData, int length = 30)
    {
        List<double> calmarList = new();
        List<double> ddList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, _) = GetMaxAndMinValuesList(inputList, length);

        double barMin = 60 * 24;
        double minPerYr = 60 * 24 * 30 * 12;
        double barsPerYr = minPerYr / barMin;
        double power = barsPerYr / (length * 15);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double maxDn = highestList[i];

            double dd = maxDn != 0 ? (currentValue - maxDn) / maxDn : 0;
            ddList.AddRounded(dd);

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            double annualReturn = 1 + ret >= 0 ? Pow(1 + ret, power) - 1 : 0;
            double maxDd = ddList.TakeLastExt(length).Min();

            double prevCalmar = calmarList.LastOrDefault();
            double calmar = maxDd != 0 ? annualReturn / Math.Abs(maxDd) : 0;
            calmarList.AddRounded(calmar);

            var signal = GetCompareSignal(calmar - 2, prevCalmar - 2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cr", calmarList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = calmarList;
        stockData.IndicatorName = IndicatorName.CalmarRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Treynor Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="beta"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateTreynorRatio(this StockData stockData, int length = 30, double beta = 1, double bmk = 0.02m)
    {
        List<double> treynorList = new();
        List<double> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double barMin = 60 * 24, minPerYr = 60 * 24 * 30 * 12, barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double bench = Pow(1 + bmk, length / barsPerYr) - 1;

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);

            double retSma = retList.TakeLastExt(length).Average();
            double prevTreynor = treynorList.LastOrDefault();
            double treynor = beta != 0 ? (retSma - bench) / beta : 0;
            treynorList.AddRounded(treynor);

            var signal = GetCompareSignal(treynor - 2, prevTreynor - 2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tr", treynorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = treynorList;
        stockData.IndicatorName = IndicatorName.TreynorRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sortino Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateSortinoRatio(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 30, 
        double bmk = 0.02m)
    {
        List<double> sortinoList = new();
        List<double> retList = new();
        List<double> deviationSquaredList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double minPerYr = 60 * 24 * 30 * 12, barMin = 60 * 24, barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double bench = Pow(1 + bmk, length / barsPerYr) - 1;

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 - bench : 0;
            retList.AddRounded(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double ret = retList[i];
            double retSma = retSmaList[i];
            double currentDeviation = Math.Min(ret - retSma, 0);

            double deviationSquared = Pow(currentDeviation, 2);
            deviationSquaredList.AddRounded(deviationSquared);
        }

        var divisionOfSumList = GetMovingAverageList(stockData, maType, length, deviationSquaredList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double divisionOfSum = divisionOfSumList[i];
            double stdDeviation = Sqrt(divisionOfSum);
            double retSma = retSmaList[i];

            double prevSortino = sortinoList.LastOrDefault();
            double sortino = stdDeviation != 0 ? retSma / stdDeviation : 0;
            sortinoList.AddRounded(sortino);

            var signal = GetCompareSignal(sortino - 2, prevSortino - 2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sr", sortinoList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sortinoList;
        stockData.IndicatorName = IndicatorName.SortinoRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sharpe Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="bmk"></param>
    /// <returns></returns>
    public static StockData CalculateSharpeRatio(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 30, 
        double bmk = 0.02m)
    {
        List<double> sharpeList = new();
        List<double> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        double minPerYr = 60 * 24 * 30 * 12, barMin = 60 * 24, barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double prevValue = i >= length ? inputList[i - length] : 0;
            double bench = Pow(1 + bmk, length / barsPerYr) - 1;

            double ret = prevValue != 0 ? (currentValue / prevValue) - 1 - bench : 0;
            retList.AddRounded(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        stockData.CustomValuesList = retList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            double stdDeviation = stdDevList[i];
            double retSma = retSmaList[i];

            double prevSharpe = sharpeList.LastOrDefault();
            double sharpe = stdDeviation != 0 ? retSma / stdDeviation : 0;
            sharpeList.AddRounded(sharpe);

            var signal = GetCompareSignal(sharpe - 2, prevSharpe - 2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sr", sharpeList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sharpeList;
        stockData.IndicatorName = IndicatorName.SharpeRatio;

        return stockData;
    }

    /// <summary>
    /// Calculates the Shinohara Intensity Ratio
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateShinoharaIntensityRatio(this StockData stockData, int length = 14)
    {
        List<double> tempOpenList = new();
        List<double> tempLowList = new();
        List<double> tempHighList = new();
        List<double> prevCloseList = new();
        List<double> ratioAList = new();
        List<double> ratioBList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double high = highList[i];
            tempHighList.AddRounded(high);

            double low = lowList[i];
            tempLowList.AddRounded(low);

            double open = openList[i];
            tempOpenList.AddRounded(open);

            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            prevCloseList.AddRounded(prevClose);

            double highSum = tempHighList.TakeLastExt(length).Sum();
            double lowSum = tempLowList.TakeLastExt(length).Sum();
            double openSum = openList.TakeLastExt(length).Sum();
            double prevCloseSum = prevCloseList.TakeLastExt(length).Sum();
            double bullA = highSum - openSum;
            double bearA = openSum - lowSum;
            double bullB = highSum - prevCloseSum;
            double bearB = prevCloseSum - lowSum;

            double prevRatioA = ratioAList.LastOrDefault();
            double ratioA = bearA != 0 ? bullA / bearA * 100 : 0;
            ratioAList.AddRounded(ratioA);

            double prevRatioB = ratioBList.LastOrDefault();
            double ratioB = bearB != 0 ? bullB / bearB * 100 : 0;
            ratioBList.AddRounded(ratioB);

            var signal = GetCompareSignal(ratioA - ratioB, prevRatioA - prevRatioB);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "ARatio", ratioAList },
            { "BRatio", ratioBList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<double>();
        stockData.IndicatorName = IndicatorName.ShinoharaIntensityRatio;

        return stockData;
    }
}