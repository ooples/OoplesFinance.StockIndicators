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
        int length = 30, decimal bmk = 0.02m)
    {
        List<decimal> martinList = new();
        List<decimal> benchList = new();
        List<decimal> retList = new();

        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal barMin = 60 * 24;
        decimal minPerYr = 60 * 24 * 30 * 12;
        decimal barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;

            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;
            benchList.AddRounded(bench);

            decimal ret = prevValue != 0 ? (100 * (currentValue / prevValue)) - 1 - (bench * 100) : 0;
            retList.AddRounded(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        stockData.CustomValuesList = retList;
        var ulcerIndexList = CalculateUlcerIndex(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ulcerIndex = ulcerIndexList[i];
            decimal retSma = retSmaList[i];

            decimal prevMartin = martinList.LastOrDefault();
            decimal martin = ulcerIndex != 0 ? retSma / ulcerIndex : 0;
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);

            decimal downSide = 0, upSide = 0;
            for (int j = 0; j < length; j++)
            {
                decimal iValue = i >= j ? retList[i - j] : 0;
                downSide += iValue < bench ? Pow(iValue - bench, 2) * ratio : 0;
                upSide += iValue > bench ? (iValue - bench) * ratio : 0;
            }

            decimal prevUpsidePotential = upsidePotentialList.LastOrDefault();
            decimal upsidePotential = downSide >= 0 ? upSide / Sqrt(downSide) : 0;
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
        int length = 30, decimal bmk = 0.05m)
    {
        List<decimal> infoList = new();
        List<decimal> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal barMin = 60 * 24;
        decimal minPerYr = 60 * 24 * 30 * 12;
        decimal barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);
        }

        stockData.CustomValuesList = retList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDeviation = stdDevList[i];
            decimal retSma = retSmaList[i];
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal prevInfo = infoList.LastOrDefault();
            decimal info = stdDeviation != 0 ? (retSma - bench) / stdDeviation : 0;
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);

            decimal downSide = 0, upSide = 0;
            for (int j = 0; j < length; j++)
            {
                decimal iValue = i >= j ? retList[i - j] : 0;
                downSide += iValue < bench ? bench - iValue : 0;
                upSide += iValue > bench ? iValue - bench : 0;
            }

            decimal prevOmega = omegaList.LastOrDefault();
            decimal omega = downSide != 0 ? upSide / downSide : 0;
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
        int length = 14, decimal breakoutLevel = 0.5m)
    {
        List<decimal> vrList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length - 1);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentEma = emaList[i];
            decimal prevHighest = i >= 1 ? highestList[i - 1] : 0;
            decimal prevLowest = i >= 1 ? lowestList[i - 1] : 0;
            decimal priorValue = i >= length + 1 ? inputList[i - (length + 1)] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevEma = i >= 1 ? emaList[i - 1] : 0;
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            decimal max = priorValue != 0 ? Math.Max(prevHighest, priorValue) : prevHighest;
            decimal min = priorValue != 0 ? Math.Min(prevLowest, priorValue) : prevLowest;

            decimal vr = max - min != 0 ? tr / (max - min) : 0;
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
        List<decimal> calmarList = new();
        List<decimal> ddList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, _) = GetMaxAndMinValuesList(inputList, length);

        decimal barMin = 60 * 24;
        decimal minPerYr = 60 * 24 * 30 * 12;
        decimal barsPerYr = minPerYr / barMin;
        decimal power = barsPerYr / (length * 15);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal maxDn = highestList[i];

            decimal dd = maxDn != 0 ? (currentValue - maxDn) / maxDn : 0;
            ddList.AddRounded(dd);

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            decimal annualReturn = 1 + ret >= 0 ? Pow(1 + ret, power) - 1 : 0;
            decimal maxDd = ddList.TakeLastExt(length).Min();

            decimal prevCalmar = calmarList.LastOrDefault();
            decimal calmar = maxDd != 0 ? annualReturn / Math.Abs(maxDd) : 0;
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
    public static StockData CalculateTreynorRatio(this StockData stockData, int length = 30, decimal beta = 1, decimal bmk = 0.02m)
    {
        List<decimal> treynorList = new();
        List<decimal> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal barMin = 60 * 24, minPerYr = 60 * 24 * 30 * 12, barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.AddRounded(ret);

            decimal retSma = retList.TakeLastExt(length).Average();
            decimal prevTreynor = treynorList.LastOrDefault();
            decimal treynor = beta != 0 ? (retSma - bench) / beta : 0;
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
        decimal bmk = 0.02m)
    {
        List<decimal> sortinoList = new();
        List<decimal> retList = new();
        List<decimal> deviationSquaredList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal minPerYr = 60 * 24 * 30 * 12, barMin = 60 * 24, barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 - bench : 0;
            retList.AddRounded(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ret = retList[i];
            decimal retSma = retSmaList[i];
            decimal currentDeviation = Math.Min(ret - retSma, 0);

            decimal deviationSquared = Pow(currentDeviation, 2);
            deviationSquaredList.AddRounded(deviationSquared);
        }

        var divisionOfSumList = GetMovingAverageList(stockData, maType, length, deviationSquaredList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal divisionOfSum = divisionOfSumList[i];
            decimal stdDeviation = Sqrt(divisionOfSum);
            decimal retSma = retSmaList[i];

            decimal prevSortino = sortinoList.LastOrDefault();
            decimal sortino = stdDeviation != 0 ? retSma / stdDeviation : 0;
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
        decimal bmk = 0.02m)
    {
        List<decimal> sharpeList = new();
        List<decimal> retList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal minPerYr = 60 * 24 * 30 * 12, barMin = 60 * 24, barsPerYr = minPerYr / barMin;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 - bench : 0;
            retList.AddRounded(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        stockData.CustomValuesList = retList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDeviation = stdDevList[i];
            decimal retSma = retSmaList[i];

            decimal prevSharpe = sharpeList.LastOrDefault();
            decimal sharpe = stdDeviation != 0 ? retSma / stdDeviation : 0;
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
        List<decimal> tempOpenList = new();
        List<decimal> tempLowList = new();
        List<decimal> tempHighList = new();
        List<decimal> prevCloseList = new();
        List<decimal> ratioAList = new();
        List<decimal> ratioBList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal high = highList[i];
            tempHighList.AddRounded(high);

            decimal low = lowList[i];
            tempLowList.AddRounded(low);

            decimal open = openList[i];
            tempOpenList.AddRounded(open);

            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            prevCloseList.AddRounded(prevClose);

            decimal highSum = tempHighList.TakeLastExt(length).Sum();
            decimal lowSum = tempLowList.TakeLastExt(length).Sum();
            decimal openSum = openList.TakeLastExt(length).Sum();
            decimal prevCloseSum = prevCloseList.TakeLastExt(length).Sum();
            decimal bullA = highSum - openSum;
            decimal bearA = openSum - lowSum;
            decimal bullB = highSum - prevCloseSum;
            decimal bearB = prevCloseSum - lowSum;

            decimal prevRatioA = ratioAList.LastOrDefault();
            decimal ratioA = bearA != 0 ? bullA / bearA * 100 : 0;
            ratioAList.AddRounded(ratioA);

            decimal prevRatioB = ratioBList.LastOrDefault();
            decimal ratioB = bearB != 0 ? bullB / bearB * 100 : 0;
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
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ShinoharaIntensityRatio;

        return stockData;
    }
}