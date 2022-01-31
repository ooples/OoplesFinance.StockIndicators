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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;
            benchList.Add(bench);

            decimal ret = prevValue != 0 ? (100 * (currentValue / prevValue)) - 1 - (bench * 100) : 0;
            retList.Add(ret);
        }

        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        stockData.CustomValuesList = retList;
        var ulcerIndexList = CalculateUlcerIndex(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ulcerIndex = ulcerIndexList.ElementAtOrDefault(i);
            decimal retSma = retSmaList.ElementAtOrDefault(i);

            decimal prevMartin = martinList.LastOrDefault();
            decimal martin = ulcerIndex != 0 ? retSma / ulcerIndex : 0;
            martinList.Add(martin);

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);

            decimal downSide = 0, upSide = 0;
            for (int j = 0; j < length; j++)
            {
                decimal iValue = i >= j ? retList.ElementAtOrDefault(i - j) : 0;
                downSide += iValue < bench ? Pow(iValue - bench, 2) * ratio : 0;
                upSide += iValue > bench ? (iValue - bench) * ratio : 0;
            }

            decimal prevUpsidePotential = upsidePotentialList.LastOrDefault();
            decimal upsidePotential = downSide >= 0 ? upSide / Sqrt(downSide) : 0;
            upsidePotentialList.Add(upsidePotential);

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);
        }

        stockData.CustomValuesList = retList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var retSmaList = GetMovingAverageList(stockData, maType, length, retList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDeviation = stdDevList.ElementAtOrDefault(i);
            decimal retSma = retSmaList.ElementAtOrDefault(i);
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal prevInfo = infoList.LastOrDefault();
            decimal info = stdDeviation != 0 ? (retSma - bench) / stdDeviation : 0;
            infoList.Add(info);

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);

            decimal downSide = 0, upSide = 0;
            for (int j = 0; j < length; j++)
            {
                decimal iValue = i >= j ? retList.ElementAtOrDefault(i - j) : 0;
                downSide += iValue < bench ? bench - iValue : 0;
                upSide += iValue > bench ? iValue - bench : 0;
            }

            decimal prevOmega = omegaList.LastOrDefault();
            decimal omega = downSide != 0 ? upSide / downSide : 0;
            omegaList.Add(omega);

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevHighest = i >= 1 ? highestList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLowest = i >= 1 ? lowestList.ElementAtOrDefault(i - 1) : 0;
            decimal priorValue = i >= length + 1 ? inputList.ElementAtOrDefault(i - (length + 1)) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            decimal max = priorValue != 0 ? Math.Max(prevHighest, priorValue) : prevHighest;
            decimal min = priorValue != 0 ? Math.Min(prevLowest, priorValue) : prevLowest;

            decimal vr = max - min != 0 ? tr / (max - min) : 0;
            vrList.Add(vr);

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal maxDn = highestList.ElementAtOrDefault(i);

            decimal dd = maxDn != 0 ? (currentValue - maxDn) / maxDn : 0;
            ddList.Add(dd);

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            decimal annualReturn = 1 + ret >= 0 ? Pow(1 + ret, power) - 1 : 0;
            decimal maxDd = ddList.TakeLastExt(length).Min();

            decimal prevCalmar = calmarList.LastOrDefault();
            decimal calmar = maxDd != 0 ? annualReturn / Math.Abs(maxDd) : 0;
            calmarList.Add(calmar);

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
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal bench = Pow(1 + bmk, length / barsPerYr) - 1;

            decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
            retList.Add(ret);

            decimal retSma = retList.TakeLastExt(length).Average();
            decimal prevTreynor = treynorList.LastOrDefault();
            decimal treynor = beta != 0 ? (retSma - bench) / beta : 0;
            treynorList.Add(treynor);

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
}