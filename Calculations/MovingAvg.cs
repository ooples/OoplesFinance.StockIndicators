namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the simple moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateSimpleMovingAverage(this StockData stockData, int length)
    {
        List<decimal> smaList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.AddRounded(currentValue);

            decimal prevSma = smaList.LastOrDefault();
            decimal sma = tempList.TakeLastExt(length).Average();
            smaList.AddRounded(sma);

            Signal signal = GetCompareSignal(currentValue - sma, prevValue - prevSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sma", smaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = smaList;
        stockData.IndicatorName = IndicatorName.SimpleMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the weighted moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> wmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal weight = length - j;
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevWma = wmaList.LastOrDefault();
            decimal wma = weightedSum != 0 ? sum / weightedSum : 0;
            wmaList.AddRounded(wma);

            Signal signal = GetCompareSignal(currentValue - wma, prevVal - prevWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wma", wmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wmaList;
        stockData.IndicatorName = IndicatorName.WeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the exponential moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateExponentialMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> emaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevEma = emaList.LastOrDefault();
            decimal ema = CalculateEMA(currentValue, prevEma, length);
            emaList.AddRounded(ema);

            var signal = GetCompareSignal(currentValue - ema, prevValue - prevEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ema", emaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emaList;
        stockData.IndicatorName = IndicatorName.ExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the triangular moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateTriangularMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 20)
    {
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var sma1List = GetMovingAverageList(stockData, maType, length, inputList);
        var tmaList = GetMovingAverageList(stockData, maType, length, sma1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal tma = tmaList.ElementAtOrDefault(i);
            decimal prevTma = i >= 1 ? tmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(currentValue - tma, prevValue - prevTma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tma", tmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tmaList;
        stockData.IndicatorName = IndicatorName.TriangularMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the hull moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateHullMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 20)
    {
        List<decimal> totalWeightedMAList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length2 = MinOrMax((int)Math.Ceiling((decimal)length / 2)); ;
        int sqrtLength = MinOrMax((int)Math.Ceiling(Sqrt(length)));

        var wma1List = GetMovingAverageList(stockData, maType, length, inputList);
        var wma2List = GetMovingAverageList(stockData, maType, length2, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentWMA1 = wma1List.ElementAtOrDefault(i);
            decimal currentWMA2 = wma2List.ElementAtOrDefault(i);

            decimal totalWeightedMA = (2 * currentWMA2) - currentWMA1;
            totalWeightedMAList.Add(totalWeightedMA);
        }

        var hullMAList = GetMovingAverageList(stockData, maType, sqrtLength, totalWeightedMAList);
        for (int j = 0; j < stockData.Count; j++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(j);
            decimal prevValue = j >= 1 ? inputList.ElementAtOrDefault(j - 1) : 0;
            decimal hullMa = hullMAList.ElementAtOrDefault(j);
            decimal prevHullMa = j >= 1 ? inputList.ElementAtOrDefault(j - 1) : 0;

            var signal = GetCompareSignal(currentValue - hullMa, prevValue - prevHullMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hma", hullMAList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hullMAList;
        stockData.IndicatorName = IndicatorName.HullMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the kaufman adaptive moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <returns></returns>
    public static StockData CalculateKaufmanAdaptiveMovingAverage(this StockData stockData, int length = 10, int fastLength = 2, int slowLength = 30)
    {
        List<decimal> volatilityList = new();
        List<decimal> erList = new();
        List<decimal> kamaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal fastAlpha = (decimal)2 / (fastLength + 1);
        decimal slowAlpha = (decimal)2 / (slowLength + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

            decimal volatility = Math.Abs(currentValue - prevValue);
            volatilityList.Add(volatility);

            decimal volatilitySum = volatilityList.TakeLastExt(length).Sum();
            decimal momentum = Math.Abs(currentValue - priorValue);

            decimal efficiencyRatio = volatilitySum != 0 ? momentum / volatilitySum : 0;
            erList.AddRounded(efficiencyRatio);

            decimal sc = Pow((efficiencyRatio * (fastAlpha - slowAlpha)) + slowAlpha, 2);
            decimal prevKama = kamaList.LastOrDefault();
            decimal currentKAMA = (sc * currentValue) + ((1 - sc) * prevKama);
            kamaList.Add(currentKAMA);

            var signal = GetCompareSignal(currentValue - currentKAMA, prevValue - prevKama);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Er", erList },
            { "Kama", kamaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kamaList;
        stockData.IndicatorName = IndicatorName.KaufmanAdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the arnaud legoux moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="sigma">The sigma.</param>
    /// <returns></returns>
    public static StockData CalculateArnaudLegouxMovingAverage(this StockData stockData, int length = 9, decimal offset = 0.85m, int sigma = 6)
    {
        List<decimal> almaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal m = offset * (length - 1);
        decimal s = (decimal)length / sigma;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = s != 0 ? Exp(-1 * Pow(j - m, 2) / (2 * Pow(s, 2))) : 0;
                decimal prevValue = i >= length - 1 - j ? inputList.ElementAtOrDefault(length - 1 - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevAlma = almaList.LastOrDefault();
            decimal alma = weightedSum != 0 ? sum / weightedSum : 0;
            almaList.Add(alma);

            var signal = GetCompareSignal(currentValue - alma, prevVal - prevAlma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Alma", almaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = almaList;
        stockData.IndicatorName = IndicatorName.ArnaudLegouxMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the end point moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="offset">The offset.</param>
    /// <returns></returns>
    public static StockData CalculateEndPointMovingAverage(this StockData stockData, int length = 11)
    {
        List<decimal> epmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = length - j - length;
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevEpma = epmaList.LastOrDefault();
            decimal epma = weightedSum != 0 ? 1 / weightedSum * sum : 0;
            epmaList.Add(epma);

            var signal = GetCompareSignal(currentValue - epma, prevVal - prevEpma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Epma", epmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = epmaList;
        stockData.IndicatorName = IndicatorName.EndPointMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the least squares moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateLeastSquaresMovingAverage(this StockData stockData, int length = 25)
    {
        List<decimal> lsmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var wmaList = CalculateWeightedMovingAverage(stockData, length).CustomValuesList;
        var smaList = CalculateSimpleMovingAverage(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentWma = wmaList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);

            decimal prevLsma = lsmaList.LastOrDefault();
            decimal lsma = (3 * currentWma) - (2 * currentSma);
            lsmaList.Add(lsma);

            var signal = GetCompareSignal(currentValue - lsma, prevValue - prevLsma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lsma", lsmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lsmaList;
        stockData.IndicatorName = IndicatorName.LeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the ehlers mother of adaptive moving averages.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="fastAlpha">The fast alpha.</param>
    /// <param name="slowAlpha">The slow alpha.</param>
    /// <returns></returns>
    public static StockData CalculateEhlersMotherOfAdaptiveMovingAverages(this StockData stockData, decimal fastAlpha = 0.5m, decimal slowAlpha = 0.05m)
    {
        List<decimal> famaList = new();
        List<decimal> mamaList = new();
        List<decimal> i2List = new();
        List<decimal> q2List = new();
        List<decimal> reList = new();
        List<decimal> imList = new();
        List<decimal> sPrdList = new();
        List<decimal> phaseList = new();
        List<decimal> periodList = new();
        List<decimal> smoothList = new();
        List<decimal> detList = new();
        List<decimal> q1List = new();
        List<decimal> i1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevPrice1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal previ2 = i >= 1 ? i2List.ElementAtOrDefault(i - 1) : 0;
            decimal prevq2 = i >= 1 ? q2List.ElementAtOrDefault(i - 1) : 0;
            decimal prevRe = i >= 1 ? reList.ElementAtOrDefault(i - 1) : 0;
            decimal prevIm = i >= 1 ? imList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSprd = i >= 1 ? sPrdList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPhase = i >= 1 ? phaseList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPeriod = i >= 1 ? periodList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPrice2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevPrice3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            decimal prevs2 = i >= 2 ? smoothList.ElementAtOrDefault(i - 2) : 0;
            decimal prevd2 = i >= 2 ? detList.ElementAtOrDefault(i - 2) : 0;
            decimal prevq1x2 = i >= 2 ? q1List.ElementAtOrDefault(i - 2) : 0;
            decimal previ1x2 = i >= 2 ? i1List.ElementAtOrDefault(i - 2) : 0;
            decimal prevd3 = i >= 3 ? detList.ElementAtOrDefault(i - 3) : 0;
            decimal prevs4 = i >= 4 ? smoothList.ElementAtOrDefault(i - 4) : 0;
            decimal prevd4 = i >= 4 ? detList.ElementAtOrDefault(i - 4) : 0;
            decimal prevq1x4 = i >= 4 ? q1List.ElementAtOrDefault(i - 4) : 0;
            decimal previ1x4 = i >= 4 ? i1List.ElementAtOrDefault(i - 4) : 0;
            decimal prevs6 = i >= 6 ? smoothList.ElementAtOrDefault(i - 6) : 0;
            decimal prevd6 = i >= 6 ? detList.ElementAtOrDefault(i - 6) : 0;
            decimal prevq1x6 = i >= 6 ? q1List.ElementAtOrDefault(i - 6) : 0;
            decimal previ1x6 = i >= 6 ? i1List.ElementAtOrDefault(i - 6) : 0;
            decimal prevMama = i >= 1 ? mamaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFama = i >= 1 ? famaList.ElementAtOrDefault(i - 1) : 0;

            decimal smooth = ((4 * currentValue) + (3 * prevPrice1) + (2 * prevPrice2) + prevPrice3) / 10;
            smoothList.Add(smooth);

            decimal det = ((0.0962m * smooth) + (0.5769m * prevs2) - (0.5769m * prevs4) - (0.0962m * prevs6)) * ((0.075m * prevPeriod) + 0.54m);
            detList.Add(det);

            decimal q1 = ((0.0962m * det) + (0.5769m * prevd2) - (0.5769m * prevd4) - (0.0962m * prevd6)) * ((0.075m * prevPeriod) + 0.54m);
            q1List.Add(q1);

            decimal i1 = prevd3;
            i1List.Add(i1);

            decimal j1 = ((0.0962m * i1) + (0.5769m * previ1x2) - (0.5769m * previ1x4) - (0.0962m * previ1x6)) * ((0.075m * prevPeriod) + 0.54m);
            decimal jq = ((0.0962m * q1) + (0.5769m * prevq1x2) - (0.5769m * prevq1x4) - (0.0962m * prevq1x6)) * ((0.075m * prevPeriod) + 0.54m);

            decimal i2 = i1 - jq;
            i2 = (0.2m * i2) + (0.8m * previ2);
            i2List.Add(i2);

            decimal q2 = q1 + j1;
            q2 = (0.2m * q2) + (0.8m * prevq2);
            q2List.Add(q2);

            decimal re = (i2 * previ2) + (q2 * prevq2);
            re = (0.2m * re) + (0.8m * prevRe);
            reList.Add(re);

            decimal im = (i2 * prevq2) - (q2 * previ2);
            im = (0.2m * im) + (0.8m * prevIm);
            imList.Add(im);

            var atan = re != 0 ? Atan(im / re) : 0;
            decimal period = atan != 0 ? 2 * (decimal)Math.PI / atan : 0;
            period = MinOrMax(period, 1.5m * prevPeriod, 0.67m * prevPeriod);
            period = MinOrMax(period, 50, 6);
            period = (0.2m * period) + (0.8m * prevPeriod);
            periodList.Add(period);

            decimal sPrd = (0.33m * period) + (0.67m * prevSprd);
            sPrdList.Add(sPrd);

            decimal phase = i1 != 0 ? 180 / (decimal)Math.PI * Atan(q1 / i1) : 0;
            phaseList.Add(phase);

            decimal deltaPhase = prevPhase - phase < 1 ? 1 : prevPhase - phase;
            decimal alpha = deltaPhase != 0 ? fastAlpha / deltaPhase : 0;
            alpha = alpha < slowAlpha ? slowAlpha : alpha;

            decimal mama = (alpha * currentValue) + ((1 - alpha) * prevMama);
            mamaList.Add(mama);

            decimal fama = (0.5m * alpha * mama) + ((1 - (0.5m * alpha)) * prevFama);
            famaList.Add(fama);

            var signal = GetCompareSignal(mama - fama, prevMama - prevFama);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fama", famaList },
            { "Mama", mamaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mamaList;
        stockData.IndicatorName = IndicatorName.EhlersMotherOfAdaptiveMovingAverages;

        return stockData;
    }

    /// <summary>
    /// Calculates the welles wilder moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateWellesWilderMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> wwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal k = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevWwma = wwmaList.LastOrDefault();
            decimal wwma = (currentValue * k) + (prevWwma * (1 - k));
            wwmaList.Add(wwma);

            var signal = GetCompareSignal(currentValue - wwma, prevValue - prevWwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wwma", wwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wwmaList;
        stockData.IndicatorName = IndicatorName.WellesWilderMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Tillson T3 Moving Average
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="vFactor">The v factor.</param>
    /// <returns></returns>
    public static StockData CalculateTillsonT3MovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 5, decimal vFactor = 0.7m)
    {
        List<decimal> t3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal c1 = -vFactor * vFactor * vFactor;
        decimal c2 = (3 * vFactor * vFactor) + (3 * vFactor * vFactor * vFactor);
        decimal c3 = (-6 * vFactor * vFactor) - (3 * vFactor) - (3 * vFactor * vFactor * vFactor);
        decimal c4 = 1 + (3 * vFactor) + (vFactor * vFactor * vFactor) + (3 * vFactor * vFactor);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
        var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);
        var ema4List = GetMovingAverageList(stockData, maType, length, ema3List);
        var ema5List = GetMovingAverageList(stockData, maType, length, ema4List);
        var ema6List = GetMovingAverageList(stockData, maType, length, ema5List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ema6 = ema6List.ElementAtOrDefault(i);
            decimal ema5 = ema5List.ElementAtOrDefault(i);
            decimal ema4 = ema4List.ElementAtOrDefault(i);
            decimal ema3 = ema3List.ElementAtOrDefault(i);

            decimal prevT3 = t3List.LastOrDefault();
            decimal t3 = (c1 * ema6) + (c2 * ema5) + (c3 * ema4) + (c4 * ema3);
            t3List.AddRounded(t3);

            var signal = GetCompareSignal(currentValue - t3, prevValue - prevT3);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "T3", t3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = t3List;
        stockData.IndicatorName = IndicatorName.TillsonT3MovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the triple exponential moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateTripleExponentialMovingAverage(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<decimal> temaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
        var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentEma1 = ema1List.ElementAtOrDefault(i);
            decimal currentEma2 = ema2List.ElementAtOrDefault(i);
            decimal currentEma3 = ema3List.ElementAtOrDefault(i);

            decimal prevTema = temaList.LastOrDefault();
            decimal tema = (3 * currentEma1) - (3 * currentEma2) + currentEma3;
            temaList.Add(tema);

            var signal = GetCompareSignal(currentValue - tema, prevValue - prevTema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tema", temaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = temaList;
        stockData.IndicatorName = IndicatorName.TripleExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the volume weighted average price.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="inputName">Name of the input.</param>
    /// <returns></returns>
    public static StockData CalculateVolumeWeightedAveragePrice(this StockData stockData, InputName inputName = InputName.TypicalPrice)
    {
        List<decimal> vwapList = new();
        List<decimal> tempVolList = new();
        List<decimal> tempVolPriceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            tempVolList.Add(currentVolume);

            decimal volumePrice = currentValue * currentVolume;
            tempVolPriceList.Add(volumePrice);

            decimal volPriceSum = tempVolPriceList.Sum();
            decimal volSum = tempVolList.Sum();

            decimal prevVwap = vwapList.LastOrDefault();
            decimal vwap = volSum != 0 ? volPriceSum / volSum : 0;
            vwapList.Add(vwap);

            var signal = GetCompareSignal(currentValue - vwap, prevValue - prevVwap);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vwap", vwapList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vwapList;
        stockData.IndicatorName = IndicatorName.VolumeWeightedAveragePrice;

        return stockData;
    }

    /// <summary>
    /// Calculates the volume weighted moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateVolumeWeightedMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14)
    {
        List<decimal> volumePriceList = new();
        List<decimal> vwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal currentVolumeSma = volumeSmaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal volumePrice = currentValue * currentVolume;
            volumePriceList.Add(volumePrice);

            decimal volumePriceSma = volumePriceList.TakeLastExt(length).Average();

            decimal prevVwma = vwmaList.LastOrDefault();
            decimal vwma = currentVolumeSma != 0 ? volumePriceSma / currentVolumeSma : 0;
            vwmaList.Add(vwma);

            var signal = GetCompareSignal(currentValue - vwma, prevValue - prevVwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vwma", vwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vwmaList;
        stockData.IndicatorName = IndicatorName.VolumeWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the ultimate moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="minLength">The minimum length.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="acc">The acc.</param>
    /// <returns></returns>
    public static StockData CalculateUltimateMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int minLength = 5, int maxLength = 50, decimal acc = 1)
    {
        List<decimal> umaList = new();
        List<decimal> posMoneyFlowList = new();
        List<decimal> negMoneyFlowList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var lenList = CalculateVariableLengthMovingAverage(stockData, maType, minLength, maxLength).OutputValues["Length"];
        var tpList = CalculateTypicalPrice(stockData).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentVolume = stockData.Volumes.ElementAtOrDefault(i);
            decimal typicalPrice = tpList.ElementAtOrDefault(i);
            decimal prevTypicalPrice = i >= 1 ? tpList.ElementAtOrDefault(i - 1) : 0;
            decimal length = MinOrMax(lenList.ElementAtOrDefault(i), maxLength, minLength);
            decimal rawMoneyFlow = typicalPrice * currentVolume;

            decimal posMoneyFlow = typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
            posMoneyFlowList.Add(posMoneyFlow);

            decimal negMoneyFlow = typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
            negMoneyFlowList.Add(negMoneyFlow);

            int len = (int)length;
            decimal posMoneyFlowTotal = posMoneyFlowList.TakeLastExt(len).Sum();
            decimal negMoneyFlowTotal = negMoneyFlowList.TakeLastExt(len).Sum();
            decimal mfiRatio = negMoneyFlowTotal != 0 ? MinOrMax(posMoneyFlowTotal / negMoneyFlowTotal, 1, 0) : 0;
            decimal mfi = negMoneyFlowTotal == 0 ? 100 : posMoneyFlowTotal == 0 ? 0 : MinOrMax(100 - (100 / (1 + mfiRatio)), 100, 0);
            decimal mfScaled = (mfi * 2) - 100;
            decimal p = acc + (Math.Abs(mfScaled) / 25);
            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= len - 1; j++)
            {
                decimal weight = Pow(len - j, p);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevUma = umaList.LastOrDefault();
            decimal uma = weightedSum != 0 ? sum / weightedSum : 0;
            umaList.Add(uma);

            var signal = GetCompareSignal(currentValue - uma, prevVal - prevUma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Uma", umaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = umaList;
        stockData.IndicatorName = IndicatorName.UltimateMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the variable length moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="minLength">The minimum length.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns></returns>
    public static StockData CalculateVariableLengthMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int minLength = 5, int maxLength = 50)
    {
        List<decimal> vlmaList = new();
        List<decimal> lengthList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, maxLength, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maxLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal a = sma - (1.75m * stdDev);
            decimal b = sma - (0.25m * stdDev);
            decimal c = sma + (0.25m * stdDev);
            decimal d = sma + (1.75m * stdDev);

            decimal prevLength = i >= 1 ? lengthList.ElementAtOrDefault(i - 1) : maxLength;
            decimal length = MinOrMax(currentValue >= b && currentValue <= c ? prevLength + 1 : currentValue < a ||
                currentValue > d ? prevLength - 1 : prevLength, maxLength, minLength);
            lengthList.Add(length);

            decimal sc = 2 / (length + 1);
            decimal prevVlma = i >= 1 ? vlmaList.ElementAtOrDefault(i - 1) : currentValue;
            decimal vlma = (currentValue * sc) + ((1 - sc) * prevVlma);
            vlmaList.Add(vlma);

            var signal = GetCompareSignal(currentValue - vlma, prevValue - prevVlma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Length", lengthList },
            { "Vlma", vlmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vlmaList;
        stockData.IndicatorName = IndicatorName.VariableLengthMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the ahrens moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAhrensMovingAverage(this StockData stockData, int length = 9)
    {
        List<decimal> ahmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorAhma = i >= length ? ahmaList.ElementAtOrDefault(i - length) : currentValue;

            decimal prevAhma = ahmaList.LastOrDefault();
            decimal ahma = prevAhma + ((currentValue - ((prevAhma + priorAhma) / 2)) / length);
            ahmaList.Add(ahma);

            var signal = GetCompareSignal(currentValue - ahma, prevValue - prevAhma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ahma", ahmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ahmaList;
        stockData.IndicatorName = IndicatorName.AhrensMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="fastLength">Length of the fast.</param>
    /// <param name="slowLength">Length of the slow.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveMovingAverage(this StockData stockData, int fastLength = 2, int slowLength = 14, int length = 14)
    {
        List<decimal> amaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length + 1);

        decimal fastAlpha = (decimal)2 / (fastLength + 1);
        decimal slowAlpha = (decimal)2 / (slowLength + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal hh = highestList.ElementAtOrDefault(i);
            decimal ll = lowestList.ElementAtOrDefault(i);
            decimal mltp = hh - ll != 0 ? MinOrMax(Math.Abs((2 * currentValue) - ll - hh) / (hh - ll), 1, 0) : 0;
            decimal ssc = (mltp * (fastAlpha - slowAlpha)) + slowAlpha;

            decimal prevAma = amaList.LastOrDefault();
            decimal ama = prevAma + (Pow(ssc, 2) * (currentValue - prevAma));
            amaList.Add(ama);

            var signal = GetCompareSignal(currentValue - ama, prevValue - prevAma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ama", amaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = amaList;
        stockData.IndicatorName = IndicatorName.AdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive exponential moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveExponentialMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 10)
    {
        List<decimal> aemaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        decimal mltp1 = (decimal)2 / (length + 1);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal hh = highestList.ElementAtOrDefault(i);
            decimal ll = lowestList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal mltp2 = hh - ll != 0 ? MinOrMax(Math.Abs((2 * currentValue) - ll - hh) / (hh - ll), 1, 0) : 0;
            decimal rate = mltp1 * (1 + mltp2);

            decimal prevAema = i >= 1 ? aemaList.LastOrDefault() : currentValue;
            decimal aema = i <= length ? sma : prevAema + (rate * (currentValue - prevAema));
            aemaList.Add(aema);

            var signal = GetCompareSignal(currentValue - aema, prevValue - prevAema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Aema", aemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aemaList;
        stockData.IndicatorName = IndicatorName.AdaptiveExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive autonomous recursive moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="gamma">The gamma.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveAutonomousRecursiveMovingAverage(this StockData stockData, int length = 14, decimal gamma = 3)
    {
        List<decimal> ma1List = new();
        List<decimal> ma2List = new();
        List<decimal> absDiffList = new();
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal er = erList.ElementAtOrDefault(i);
            decimal prevMa2 = i >= 1 ? ma2List.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal absDiff = Math.Abs(currentValue - prevMa2);
            absDiffList.Add(absDiff);

            decimal d = i != 0 ? absDiffList.Sum() / i * gamma : 0;
            dList.AddRounded(d);

            decimal c = currentValue > prevMa2 + d ? currentValue + d : currentValue < prevMa2 - d ? currentValue - d : prevMa2;
            decimal prevMa1 = i >= 1 ? ma1List.ElementAtOrDefault(i - 1) : currentValue;
            decimal ma1 = (er * c) + ((1 - er) * prevMa1);
            ma1List.Add(ma1);

            decimal ma2 = (er * ma1) + ((1 - er) * prevMa2);
            ma2List.Add(ma2);

            var signal = GetCompareSignal(currentValue - ma2, prevValue - prevMa2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "D", dList },
            { "Aarma", ma2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ma2List;
        stockData.IndicatorName = IndicatorName.AdaptiveAutonomousRecursiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the autonomous recursive moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="momLength">Length of the mom.</param>
    /// <param name="gamma">The gamma.</param>
    /// <returns></returns>
    public static StockData CalculateAutonomousRecursiveMovingAverage(this StockData stockData, int length = 14, int momLength = 7, decimal gamma = 3)
    {
        List<decimal> madList = new();
        List<decimal> ma1List = new();
        List<decimal> absDiffList = new();
        List<decimal> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - momLength) : 0;
            decimal prevMad = i >= 1 ? madList.ElementAtOrDefault(i - 1) : currentValue;

            decimal absDiff = Math.Abs(priorValue - prevMad);
            absDiffList.Add(absDiff);

            decimal d = i != 0 ? absDiffList.Sum() / i * gamma : 0;
            decimal c = currentValue > prevMad + d ? currentValue + d : currentValue < prevMad - d ? currentValue - d : prevMad;
            cList.Add(c);

            decimal ma1 = cList.TakeLastExt(length).Average();
            ma1List.Add(ma1);

            decimal mad = ma1List.TakeLastExt(length).Average();
            madList.Add(mad);

            var signal = GetCompareSignal(currentValue - mad, prevValue - prevMad);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Arma", madList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = madList;
        stockData.IndicatorName = IndicatorName.AutonomousRecursiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the atr filtered exponential moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="atrLength">Length of the atr.</param>
    /// <param name="stdDevLength">Length of the standard dev.</param>
    /// <param name="lbLength">Length of the lb.</param>
    /// <param name="min">The minimum.</param>
    /// <returns></returns>
    public static StockData CalculateAtrFilteredExponentialMovingAverage(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 45, int atrLength = 20, int stdDevLength = 10, int lbLength = 20, 
        decimal min = 5)
    {
        List<decimal> trValList = new();
        List<decimal> atrValPowList = new();
        List<decimal> tempList = new();
        List<decimal> stdDevList = new();
        List<decimal> emaAFPList = new();
        List<decimal> emaCTPList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

            decimal trVal = currentValue != 0 ? tr / currentValue : tr;
            trValList.Add(trVal);
        }

        var atrValList = GetMovingAverageList(stockData, maType, atrLength, trValList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal atrVal = atrValList.ElementAtOrDefault(i);

            decimal atrValPow = Pow(atrVal, 2);
            atrValPowList.Add(atrValPow);
        }

        var stdDevAList = GetMovingAverageList(stockData, maType, stdDevLength, atrValPowList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDevA = stdDevAList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal atrVal = atrValList.ElementAtOrDefault(i);
            tempList.Add(atrVal);

            decimal atrValSum = tempList.TakeLastExt(stdDevLength).Sum();
            decimal stdDevB = Pow(atrValSum, 2) / Pow(stdDevLength, 2);

            decimal stdDev = stdDevA - stdDevB >= 0 ? Sqrt(stdDevA - stdDevB) : 0;
            stdDevList.Add(stdDev);

            decimal stdDevLow = stdDevList.TakeLastExt(lbLength).Min();
            decimal stdDevFactorAFP = stdDev != 0 ? stdDevLow / stdDev : 0;
            decimal stdDevFactorCTP = stdDevLow != 0 ? stdDev / stdDevLow : 0;
            decimal stdDevFactorAFPLow = Math.Min(stdDevFactorAFP, min);
            decimal stdDevFactorCTPLow = Math.Min(stdDevFactorCTP, min);
            decimal alphaAfp = (2 * stdDevFactorAFPLow) / (length + 1);
            decimal alphaCtp = (2 * stdDevFactorCTPLow) / (length + 1);

            decimal prevEmaAfp = emaAFPList.LastOrDefault();
            decimal emaAfp = (alphaAfp * currentValue) + ((1 - alphaAfp) * prevEmaAfp);
            emaAFPList.Add(emaAfp);

            decimal prevEmaCtp = emaCTPList.LastOrDefault();
            decimal emaCtp = (alphaCtp * currentValue) + ((1 - alphaCtp) * prevEmaCtp);
            emaCTPList.Add(emaCtp);

            var signal = GetCompareSignal(currentValue - emaAfp, prevValue - prevEmaAfp);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Afp", emaAFPList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emaAFPList;
        stockData.IndicatorName = IndicatorName.AtrFilteredExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the adaptive least squares.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="smooth">The smooth.</param>
    /// <returns></returns>
    public static StockData CalculateAdaptiveLeastSquares(this StockData stockData, int length = 500, decimal smooth = 1.5m)
    {
        List<decimal> xList = new();
        List<decimal> yList = new();
        List<decimal> mxList = new();
        List<decimal> myList = new();
        List<decimal> regList = new();
        List<decimal> tempList = new();
        List<decimal> mxxList = new();
        List<decimal> myyList = new();
        List<decimal> mxyList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal index = i;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            tempList.Add(tr);

            decimal highest = tempList.TakeLastExt(length).Max();
            decimal alpha = highest != 0 ? MinOrMax(Pow(tr / highest, smooth), 0.99m, 0.01m) : 0.01m;
            decimal xx = index * index;
            decimal yy = currentValue * currentValue;
            decimal xy = index * currentValue;

            decimal prevX = i >= 1 ? xList.ElementAtOrDefault(i - 1) : index;
            decimal x = (alpha * index) + ((1 - alpha) * prevX);
            xList.Add(x);

            decimal prevY = i >= 1 ? yList.ElementAtOrDefault(i - 1) : currentValue;
            decimal y = (alpha * currentValue) + ((1 - alpha) * prevY);
            yList.Add(y);

            decimal dx = Math.Abs(index - x);
            decimal dy = Math.Abs(currentValue - y);

            decimal prevMx = i >= 1 ? mxList.ElementAtOrDefault(i - 1) : dx;
            decimal mx = (alpha * dx) + ((1 - alpha) * prevMx);
            mxList.Add(mx);

            decimal prevMy = i >= 1 ? myList.ElementAtOrDefault(i - 1) : dy;
            decimal my = (alpha * dy) + ((1 - alpha) * prevMy);
            myList.Add(my);

            decimal prevMxx = i >= 1 ? mxxList.ElementAtOrDefault(i - 1) : xx;
            decimal mxx = (alpha * xx) + ((1 - alpha) * prevMxx);
            mxxList.Add(mxx);

            decimal prevMyy = i >= 1 ? myyList.ElementAtOrDefault(i - 1) : yy;
            decimal myy = (alpha * yy) + ((1 - alpha) * prevMyy);
            myyList.Add(myy);

            decimal prevMxy = i >= 1 ? mxyList.ElementAtOrDefault(i - 1) : xy;
            decimal mxy = (alpha * xy) + ((1 - alpha) * prevMxy);
            mxyList.Add(mxy);

            decimal alphaVal = (2 / alpha) + 1;
            decimal a1 = alpha != 0 ? (Pow(alphaVal, 2) * mxy) - (alphaVal * mx * alphaVal * my) : 0;
            decimal tempVal = ((Pow(alphaVal, 2) * mxx) - Pow(alphaVal * mx, 2)) * ((Pow(alphaVal, 2) * myy) - Pow(alphaVal * my, 2));
            decimal b1 = tempVal >= 0 ? Sqrt(tempVal) : 0;
            decimal r = b1 != 0 ? a1 / b1 : 0;
            decimal a = mx != 0 ? r * (my / mx) : 0;
            decimal b = y - (a * x);

            decimal prevReg = regList.LastOrDefault();
            decimal reg = (x * a) + b;
            regList.Add(reg);

            var signal = GetCompareSignal(currentValue - reg, prevValue - prevReg);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Als", regList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = regList;
        stockData.IndicatorName = IndicatorName.AdaptiveLeastSquares;

        return stockData;
    }

    /// <summary>
    /// Calculates the alpha decreasing exponential moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateAlphaDecreasingExponentialMovingAverage(this StockData stockData)
    {
        List<decimal> emaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal alpha = (decimal)2 / (i + 1);

            decimal prevEma = emaList.LastOrDefault();
            decimal ema = (alpha * currentValue) + ((1 - alpha) * prevEma);
            emaList.Add(ema);

            var signal = GetCompareSignal(currentValue - ema, prevValue - prevEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ema", emaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = emaList;
        stockData.IndicatorName = IndicatorName.AlphaDecreasingExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the powered kaufman adaptive moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="factor">The factor.</param>
    /// <returns></returns>
    public static StockData CalculatePoweredKaufmanAdaptiveMovingAverage(this StockData stockData, int length = 100, decimal factor = 3)
    {
        List<decimal> aList = new();
        List<decimal> aSpList = new();
        List<decimal> perList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal er = erList.ElementAtOrDefault(i);
            decimal powSp = er != 0 ? 1 / er : factor;
            decimal perSp = Pow(er, powSp);

            decimal per = Pow(er, factor);
            perList.AddRounded(per);

            decimal prevA = i >= 1 ? aList.LastOrDefault() : currentValue;
            decimal a = (per * currentValue) + ((1 - per) * prevA);
            aList.AddRounded(a);

            decimal prevASp = i >= 1 ? aSpList.LastOrDefault() : currentValue;
            decimal aSp = (perSp * currentValue) + ((1 - perSp) * prevASp);
            aSpList.AddRounded(aSp);

            var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Per", perList },
            { "Pkama", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.PoweredKaufmanAdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the automatic filter.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAutoFilter(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 500)
    {
        List<decimal> regList = new();
        List<decimal> corrList = new();
        List<decimal> interList = new();
        List<decimal> slopeList = new();
        List<decimal> tempList = new();
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var yMaList = GetMovingAverageList(stockData, maType, length, inputList);
        var devList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dev = devList.ElementAtOrDefault(i);

            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal prevX = i >= 1 ? xList.ElementAtOrDefault(i - 1) : currentValue;
            decimal x = currentValue > prevX + dev ? currentValue : currentValue < prevX - dev ? currentValue : prevX;
            xList.Add(x);

            var corr = GoodnessOfFit.R(tempList.TakeLastExt(length).Select(x => (double)x), xList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.Add((decimal)corr);
        }

        var xMaList = GetMovingAverageList(stockData, maType, length, xList);
        stockData.CustomValuesList = xList;
        var mxList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal my = devList.ElementAtOrDefault(i);
            decimal mx = mxList.ElementAtOrDefault(i);
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal yMa = yMaList.ElementAtOrDefault(i);
            decimal xMa = xMaList.ElementAtOrDefault(i);
            decimal x = xList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal slope = mx != 0 ? corr * (my / mx) : 0;
            decimal inter = yMa - (slope * xMa);

            decimal prevReg = regList.LastOrDefault();
            decimal reg = (x * slope) + inter;
            regList.Add(reg);

            var signal = GetCompareSignal(currentValue - reg, prevValue - prevReg);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Af", regList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = regList;
        stockData.IndicatorName = IndicatorName.AutoFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the automatic line.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAutoLine(this StockData stockData, int length = 500)
    {
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var devList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dev = devList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevX = i >= 1 ? xList.ElementAtOrDefault(i - 1) : currentValue;
            decimal x = currentValue > prevX + dev ? currentValue : currentValue < prevX - dev ? currentValue : prevX;
            xList.Add(x);

            var signal = GetCompareSignal(currentValue - x, prevValue - prevX);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Al", xList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = xList;
        stockData.IndicatorName = IndicatorName.AutoLine;

        return stockData;
    }

    /// <summary>
    /// Calculates the automatic line with drift.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAutoLineWithDrift(this StockData stockData, int length = 500)
    {
        List<decimal> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal dev = stdDevList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal r = Math.Round(currentValue);

            decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : r;
            decimal priorA = i >= length + 1 ? aList.ElementAtOrDefault(i - (length + 1)) : r;
            decimal a = currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue :
                prevA + ((decimal)1 / (length * 2) * (prevA - priorA));
            aList.Add(a);

            var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Alwd", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.AutoLineWithDrift;

        return stockData;
    }

    /// <summary>
    /// Calculates the 1LC Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData Calculate1LCLeastSquaresMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14)
    {
        List<decimal> yList = new();
        List<decimal> tempList = new();
        List<decimal> corrList = new();
        List<decimal> indexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

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
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevY = yList.LastOrDefault();
            decimal y = sma + (corr * stdDev * 1.7m);
            yList.Add(y);

            var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "1lsma", yList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = yList;
        stockData.IndicatorName = IndicatorName._1LCLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the 3HMA
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData Calculate3HMA(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 50)
    {
        List<decimal> midList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int p = MinOrMax((int)Math.Ceiling((decimal)length / 2));
        int p1 = MinOrMax((int)Math.Ceiling((decimal)p / 3));
        int p2 = MinOrMax((int)Math.Ceiling((decimal)p / 2));

        var wma1List = GetMovingAverageList(stockData, maType, p1, inputList);
        var wma2List = GetMovingAverageList(stockData, maType, p2, inputList);
        var wma3List = GetMovingAverageList(stockData, maType, p, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal wma1 = wma1List.ElementAtOrDefault(i);
            decimal wma2 = wma2List.ElementAtOrDefault(i);
            decimal wma3 = wma3List.ElementAtOrDefault(i);

            decimal mid = (wma1 * 3) - wma2 - wma3;
            midList.Add(mid);
        }

        var aList = GetMovingAverageList(stockData, maType, p, midList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = aList.ElementAtOrDefault(i);
            decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "3hma", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName._3HMA;

        return stockData;
    }

    /// <summary>
    /// Calculates the Jsa Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateJsaMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> jmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevJma = jmaList.LastOrDefault();
            decimal jma = (currentValue + priorValue) / 2;
            jmaList.Add(jma);

            var signal = GetCompareSignal(currentValue - jma, prevValue - prevJma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Jma", jmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = jmaList;
        stockData.IndicatorName = IndicatorName.JsaMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Jurik Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="phase"></param>
    /// <param name="power"></param>
    /// <returns></returns>
    public static StockData CalculateJurikMovingAverage(this StockData stockData, int length = 7, decimal phase = 50, decimal power = 2)
    {
        List<decimal> e0List = new();
        List<decimal> e1List = new();
        List<decimal> e2List = new();
        List<decimal> jmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal phaseRatio = phase < -100 ? 0.5m : phase > 100 ? 2.5m : ((decimal)phase / 100) + 1.5m;
        decimal ratio = 0.45m * (length - 1);
        decimal beta = ratio / (ratio + 2);
        decimal alpha = Pow(beta, power);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevJma = jmaList.LastOrDefault();

            decimal prevE0 = e0List.LastOrDefault();
            decimal e0 = ((1 - alpha) * currentValue) + (alpha * prevE0);
            e0List.AddRounded(e0);

            decimal prevE1 = e1List.LastOrDefault();
            decimal e1 = ((currentValue - e0) * (1 - beta)) + (beta * prevE1);
            e1List.AddRounded(e1);

            decimal prevE2 = e2List.LastOrDefault();
            decimal e2 = ((e0 + (phaseRatio * e1) - prevJma) * Pow(1 - alpha, 2)) + (Pow(alpha, 2) * prevE2);
            e2List.AddRounded(e2);

            decimal jma = e2 + prevJma;
            jmaList.AddRounded(jma);

            Signal signal = GetCompareSignal(currentValue - jma, prevValue - prevJma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Jma", jmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = jmaList;
        stockData.IndicatorName = IndicatorName.JurikMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Zero Low Lag Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="lag"></param>
    /// <returns></returns>
    public static StockData CalculateZeroLowLagMovingAverage(this StockData stockData, int length = 50, decimal lag = 1.4m)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int lbLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorB = i >= lbLength ? bList.ElementAtOrDefault(i - lbLength) : currentValue;
            decimal priorA = i >= length ? aList.ElementAtOrDefault(i - length) : 0;

            decimal prevA = aList.LastOrDefault();
            decimal a = (lag * currentValue) + ((1 - lag) * priorB) + prevA;
            aList.Add(a);

            decimal aDiff = a - priorA;
            decimal prevB = bList.LastOrDefault();
            decimal b = aDiff / length;
            bList.Add(b);

            var signal = GetCompareSignal(currentValue - b, prevValue - prevB);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Zllma", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.ZeroLowLagMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Zero Lag Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateZeroLagExponentialMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<decimal> zemaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ema1 = ema1List.ElementAtOrDefault(i);
            decimal ema2 = ema2List.ElementAtOrDefault(i);
            decimal d = ema1 - ema2;

            decimal prevZema = zemaList.LastOrDefault();
            decimal zema = ema1 + d;
            zemaList.Add(zema);

            var signal = GetCompareSignal(currentValue - zema, prevValue - prevZema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Zema", zemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zemaList;
        stockData.IndicatorName = IndicatorName.ZeroLagExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Zero Lag Triple Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateZeroLagTripleExponentialMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.TripleExponentialMovingAverage, int length = 14)
    {
        List<decimal> zlTemaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var tma1List = GetMovingAverageList(stockData, maType, length, inputList);
        var tma2List = GetMovingAverageList(stockData, maType, length, tma1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal tma1 = tma1List.ElementAtOrDefault(i);
            decimal tma2 = tma2List.ElementAtOrDefault(i);
            decimal diff = tma1 - tma2;

            decimal prevZltema = zlTemaList.LastOrDefault();
            decimal zltema = tma1 + diff;
            zlTemaList.Add(zltema);

            var signal = GetCompareSignal(currentValue - zltema, prevValue - prevZltema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ztema", zlTemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zlTemaList;
        stockData.IndicatorName = IndicatorName.ZeroLagTripleExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Bryant Adaptive Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="maxLength"></param>
    /// <param name="trend"></param>
    /// <returns></returns>
    public static StockData CalculateBryantAdaptiveMovingAverage(this StockData stockData, int length = 14, int maxLength = 100, decimal trend = -1)
    {
        List<decimal> bamaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal er = erList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ver = Pow(er - (((2 * er) - 1) / 2 * (1 - trend)) + 0.5m, 2);
            decimal vLength = ver != 0 ? (length - ver + 1) / ver : 0;
            vLength = Math.Min(vLength, maxLength);
            decimal vAlpha = 2 / (vLength + 1);

            decimal prevBama = bamaList.LastOrDefault();
            decimal bama = (vAlpha * currentValue) + ((1 - vAlpha) * prevBama);
            bamaList.Add(bama);

            var signal = GetCompareSignal(currentValue - bama, prevValue - prevBama);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Bama", bamaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bamaList;
        stockData.IndicatorName = IndicatorName.BryantAdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Windowed Volume Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateWindowedVolumeWeightedMovingAverage(this StockData stockData, int length = 100)
    {
        List<decimal> bartlettWList = new();
        List<decimal> blackmanWList = new();
        List<decimal> hanningWList = new();
        List<decimal> bartlettVWList = new();
        List<decimal> blackmanVWList = new();
        List<decimal> hanningVWList = new();
        List<decimal> bartlettWvwmaList = new();
        List<decimal> blackmanWvwmaList = new();
        List<decimal> hanningWvwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal iRatio = (decimal)i / length;
            decimal bartlett = 1 - (2 * Math.Abs(i - ((decimal)length / 2)) / length);

            decimal bartlettW = bartlett * currentVolume;
            bartlettWList.Add(bartlettW);

            decimal bartlettWSum = bartlettWList.TakeLastExt(length).Sum();
            decimal bartlettVW = currentValue * bartlettW;
            bartlettVWList.Add(bartlettVW);

            decimal bartlettVWSum = bartlettVWList.TakeLastExt(length).Sum();
            decimal prevBartlettWvwma = bartlettWvwmaList.LastOrDefault();
            decimal bartlettWvwma = bartlettWSum != 0 ? bartlettVWSum / bartlettWSum : 0;
            bartlettWvwmaList.Add(bartlettWvwma);

            decimal blackman = 0.42m - (0.5m * Cos(2 * (decimal)Math.PI * iRatio)) + (0.08m * Cos(4 * (decimal)Math.PI * iRatio));
            decimal blackmanW = blackman * currentVolume;
            blackmanWList.Add(blackmanW);

            decimal blackmanWSum = blackmanWList.TakeLastExt(length).Sum();
            decimal blackmanVW = currentValue * blackmanW;
            blackmanVWList.Add(blackmanVW);

            decimal blackmanVWSum = blackmanVWList.TakeLastExt(length).Sum();
            decimal blackmanWvwma = blackmanWSum != 0 ? blackmanVWSum / blackmanWSum : 0;
            blackmanWvwmaList.Add(blackmanWvwma);

            decimal hanning = 0.5m - (0.5m * Cos(2 * (decimal)Math.PI * iRatio));
            decimal hanningW = hanning * currentVolume;
            hanningWList.Add(hanningW);

            decimal hanningWSum = hanningWList.TakeLastExt(length).Sum();
            decimal hanningVW = currentValue * hanningW;
            hanningVWList.Add(hanningVW);

            decimal hanningVWSum = hanningVWList.TakeLastExt(length).Sum();
            decimal hanningWvwma = hanningWSum != 0 ? hanningVWSum / hanningWSum : 0;
            hanningWvwmaList.Add(hanningWvwma);

            var signal = GetCompareSignal(currentValue - bartlettWvwma, prevValue - prevBartlettWvwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wvwma", bartlettWvwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bartlettWvwmaList;
        stockData.IndicatorName = IndicatorName.WindowedVolumeWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Well Rounded Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateWellRoundedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> yList = new();
        List<decimal> srcYList = new();
        List<decimal> srcEmaList = new();
        List<decimal> yEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSrcY = i >= 1 ? srcYList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSrcEma = i >= 1 ? srcEmaList.ElementAtOrDefault(i - 1) : 0;

            decimal prevA = aList.LastOrDefault();
            decimal a = prevA + (alpha * prevSrcY);
            aList.Add(a);

            decimal prevB = bList.LastOrDefault();
            decimal b = prevB + (alpha * prevSrcEma);
            bList.Add(b);

            decimal ab = a + b;
            decimal prevY = yList.LastOrDefault();
            decimal y = CalculateEMA(ab, prevY, 1);
            yList.Add(y);

            decimal srcY = currentValue - y;
            srcYList.Add(srcY);

            decimal prevYEma = yEmaList.LastOrDefault();
            decimal yEma = CalculateEMA(y, prevYEma, length);
            yEmaList.Add(yEma);

            decimal srcEma = currentValue - yEma;
            srcEmaList.Add(srcEma);

            var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wrma", yList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = yList;
        stockData.IndicatorName = IndicatorName.WellRoundedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Welles Wilder Summation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateWellesWilderSummation(this StockData stockData, int length = 14)
    {
        List<decimal> sumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevSum = sumList.LastOrDefault();
            decimal sum = prevSum - (prevSum / length) + currentValue;
            sumList.Add(sum);

            var signal = GetCompareSignal(currentValue - sum, prevValue - prevSum);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wws", sumList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sumList;
        stockData.IndicatorName = IndicatorName.WellesWilderSummation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Quick Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateQuickMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> qmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int peak = MinOrMax((int)Math.Ceiling((decimal)length / 3));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal num = 0, denom = 0;
            for (int j = 1; j <= length + 1; j++)
            {
                decimal mult = j <= peak ? (decimal)j / peak : (decimal)(length + 1 - j) / (length + 1 - peak);
                decimal prevValue = i >= j - 1 ? inputList.ElementAtOrDefault(i - (j - 1)) : 0;

                num += prevValue * mult;
                denom += mult;
            }

            decimal prevQma = qmaList.LastOrDefault();
            decimal qma = denom != 0 ? num / denom : 0;
            qmaList.Add(qma);

            var signal = GetCompareSignal(currentValue - qma, prevVal - prevQma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Qma", qmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = qmaList;
        stockData.IndicatorName = IndicatorName.QuickMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Quadratic Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateQuadraticMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> qmaList = new();
        List<decimal> powList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal pow = Pow(currentValue, 2);
            powList.Add(pow);

            decimal prevQma = qmaList.LastOrDefault();
            decimal powSma = powList.TakeLastExt(length).Average();
            decimal qma = powSma >= 0 ? Sqrt(powSma) : 0;
            qmaList.Add(qma);

            var signal = GetCompareSignal(currentValue - qma, prevValue - prevQma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Qma", qmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = qmaList;
        stockData.IndicatorName = IndicatorName.QuadraticMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Quadruple Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateQuadrupleExponentialMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20)
    {
        List<decimal> qemaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
        var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);
        var ema4List = GetMovingAverageList(stockData, maType, length, ema3List);
        var ema5List = GetMovingAverageList(stockData, maType, length, ema4List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ema1 = ema1List.ElementAtOrDefault(i);
            decimal ema2 = ema2List.ElementAtOrDefault(i);
            decimal ema3 = ema3List.ElementAtOrDefault(i);
            decimal ema4 = ema4List.ElementAtOrDefault(i);
            decimal ema5 = ema5List.ElementAtOrDefault(i);

            decimal prevQema = qemaList.LastOrDefault();
            decimal qema = (5 * ema1) - (10 * ema2) + (10 * ema3) - (5 * ema4) + ema5;
            qemaList.Add(qema);

            var signal = GetCompareSignal(currentValue - qema, prevValue - prevQema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Qema", qemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = qemaList;
        stockData.IndicatorName = IndicatorName.QuadrupleExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Quadratic Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="forecastLength"></param>
    /// <returns></returns>
    public static StockData CalculateQuadraticLeastSquaresMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 50, int forecastLength = 14)
    {
        List<decimal> nList = new();
        List<decimal> n2List = new();
        List<decimal> nn2List = new();
        List<decimal> nn2CovList = new();
        List<decimal> n2vList = new();
        List<decimal> n2vCovList = new();
        List<decimal> nvList = new();
        List<decimal> nvCovList = new();
        List<decimal> qlsmaList = new();
        List<decimal> fcastList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);

            decimal n = i;
            nList.Add(n);

            decimal n2 = Pow(n, 2);
            n2List.Add(n2);

            decimal nn2 = n * n2;
            nn2List.Add(nn2);

            decimal n2v = n2 * currentValue;
            n2vList.Add(n2v);

            decimal nv = n * currentValue;
            nvList.Add(nv);
        }

        var nSmaList = GetMovingAverageList(stockData, maType, length, nList);
        var n2SmaList = GetMovingAverageList(stockData, maType, length, n2List);
        var n2vSmaList = GetMovingAverageList(stockData, maType, length, n2vList);
        var nvSmaList = GetMovingAverageList(stockData, maType, length, nvList);
        var nn2SmaList = GetMovingAverageList(stockData, maType, length, nn2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nSma = nSmaList.ElementAtOrDefault(i);
            decimal n2Sma = n2SmaList.ElementAtOrDefault(i);
            decimal n2vSma = n2vSmaList.ElementAtOrDefault(i);
            decimal nvSma = nvSmaList.ElementAtOrDefault(i);
            decimal nn2Sma = nn2SmaList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);

            decimal nn2Cov = nn2Sma - (nSma * n2Sma);
            nn2CovList.Add(nn2Cov);

            decimal n2vCov = n2vSma - (n2Sma * sma);
            n2vCovList.Add(n2vCov);

            decimal nvCov = nvSma - (nSma * sma);
            nvCovList.Add(nvCov);
        }

        stockData.CustomValuesList = nList;
        var nVarianceList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        stockData.CustomValuesList = n2List;
        var n2VarianceList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal n2Variance = n2VarianceList.ElementAtOrDefault(i);
            decimal nVariance = nVarianceList.ElementAtOrDefault(i);
            decimal nn2Cov = nn2CovList.ElementAtOrDefault(i);
            decimal n2vCov = n2vCovList.ElementAtOrDefault(i);
            decimal nvCov = nvCovList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal n2Sma = n2SmaList.ElementAtOrDefault(i);
            decimal nSma = nSmaList.ElementAtOrDefault(i);
            decimal n2 = n2List.ElementAtOrDefault(i);
            decimal norm = (n2Variance * nVariance) - Pow(nn2Cov, 2);
            decimal a = norm != 0 ? ((n2vCov * nVariance) - (nvCov * nn2Cov)) / norm : 0;
            decimal b = norm != 0 ? ((nvCov * n2Variance) - (n2vCov * nn2Cov)) / norm : 0;
            decimal c = sma - (a * n2Sma) - (b * nSma);

            decimal prevQlsma = qlsmaList.LastOrDefault();
            decimal qlsma = (a * n2) + (b * i) + c;
            qlsmaList.Add(qlsma);

            decimal fcast = (a * Pow(i + forecastLength, 2)) + (b * (i + forecastLength)) + c;
            fcastList.Add(fcast);

            var signal = GetCompareSignal(currentValue - qlsma, prevValue - prevQlsma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Qlma", qlsmaList },
            { "Forecast", fcastList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = qlsmaList;
        stockData.IndicatorName = IndicatorName.QuadraticLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Quadratic Regression
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateQuadraticRegression(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 500)
    {
        List<decimal> tempList = new();
        List<decimal> x1List = new();
        List<decimal> x2List = new();
        List<decimal> x1SumList = new();
        List<decimal> x2SumList = new();
        List<decimal> x1x2List = new();
        List<decimal> x1x2SumList = new();
        List<decimal> x2PowList = new();
        List<decimal> x2PowSumList = new();
        List<decimal> ySumList = new();
        List<decimal> yx1List = new();
        List<decimal> yx2List = new();
        List<decimal> yx1SumList = new();
        List<decimal> yx2SumList = new();
        List<decimal> yList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal y = inputList.ElementAtOrDefault(i);
            tempList.Add(y);

            decimal x1 = i;
            x1List.Add(x1);

            decimal x2 = Pow(x1, 2);
            x2List.Add(x2);

            decimal x1x2 = x1 * x2;
            x1x2List.Add(x1x2);

            decimal yx1 = y * x1;
            yx1List.Add(yx1);

            decimal yx2 = y * x2;
            yx2List.Add(yx2);

            decimal x2Pow = Pow(x2, 2);
            x2PowList.Add(x2Pow);

            decimal ySum = tempList.TakeLastExt(length).Sum();
            ySumList.Add(ySum);

            decimal x1Sum = x1List.TakeLastExt(length).Sum();
            x1SumList.Add(x1Sum);

            decimal x2Sum = x2List.TakeLastExt(length).Sum();
            x2SumList.Add(x2Sum);

            decimal x1x2Sum = x1x2List.TakeLastExt(length).Sum();
            x1x2SumList.Add(x1x2Sum);

            decimal yx1Sum = yx1List.TakeLastExt(length).Sum();
            yx1SumList.Add(yx1Sum);

            decimal yx2Sum = yx2List.TakeLastExt(length).Sum();
            yx2SumList.Add(yx2Sum);

            decimal x2PowSum = x2PowList.TakeLastExt(length).Sum();
            x2PowSumList.Add(x2PowSum);
        }

        var max1List = GetMovingAverageList(stockData, maType, length, x1List);
        var max2List = GetMovingAverageList(stockData, maType, length, x2List);
        var mayList = GetMovingAverageList(stockData, maType, length, inputList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal x1Sum = x1SumList.ElementAtOrDefault(i);
            decimal x2Sum = x2SumList.ElementAtOrDefault(i);
            decimal x1x2Sum = x1x2SumList.ElementAtOrDefault(i);
            decimal x2PowSum = x2PowSumList.ElementAtOrDefault(i);
            decimal yx1Sum = yx1SumList.ElementAtOrDefault(i);
            decimal yx2Sum = yx2SumList.ElementAtOrDefault(i);
            decimal ySum = ySumList.ElementAtOrDefault(i);
            decimal may = mayList.ElementAtOrDefault(i);
            decimal max1 = max1List.ElementAtOrDefault(i);
            decimal max2 = max2List.ElementAtOrDefault(i);
            decimal x1 = x1List.ElementAtOrDefault(i);
            decimal x2 = x2List.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal s11 = x2Sum - (Pow(x1Sum, 2) / length);
            decimal s12 = x1x2Sum - ((x1Sum * x2Sum) / length);
            decimal s22 = x2PowSum - (Pow(x2Sum, 2) / length);
            decimal sy1 = yx1Sum - ((ySum * x1Sum) / length);
            decimal sy2 = yx2Sum - ((ySum * x2Sum) / length);
            decimal bot = (s22 * s11) - Pow(s12, 2);
            decimal b2 = bot != 0 ? ((sy1 * s22) - (sy2 * s12)) / bot : 0;
            decimal b3 = bot != 0 ? ((sy2 * s11) - (sy1 * s12)) / bot : 0;
            decimal b1 = may - (b2 * max1) - (b3 * max2);

            decimal prevY = yList.LastOrDefault();
            decimal y = b1 + (b2 * x1) + (b3 * x2);
            yList.Add(y);

            var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "QuadReg", yList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = yList;
        stockData.IndicatorName = IndicatorName.QuadraticRegression;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linear Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateLinearWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> lwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = length - j;
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevLwma = lwmaList.LastOrDefault();
            decimal lwma = weightedSum != 0 ? sum / weightedSum : 0;
            lwmaList.Add(lwma);

            var signal = GetCompareSignal(currentValue - lwma, prevVal - prevLwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lwma", lwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lwmaList;
        stockData.IndicatorName = IndicatorName.LinearWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Leo Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateLeoMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> lmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var wmaList = CalculateWeightedMovingAverage(stockData, length).CustomValuesList;
        var smaList = CalculateSimpleMovingAverage(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentWma = wmaList.ElementAtOrDefault(i);
            decimal currentSma = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevLma = lmaList.LastOrDefault();
            decimal lma = (2 * currentWma) - currentSma;
            lmaList.Add(lma);

            var signal = GetCompareSignal(currentValue - lma, prevValue - prevLma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Lma", lmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lmaList;
        stockData.IndicatorName = IndicatorName.LeoMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Light Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateLightLeastSquaresMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 250)
    {
        List<decimal> yList = new();
        List<decimal> indexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal index = i;
            indexList.Add(index);
        }

        var sma1List = GetMovingAverageList(stockData, maType, length, inputList);
        var sma2List = GetMovingAverageList(stockData, maType, length1, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma1 = sma1List.ElementAtOrDefault(i);
            decimal sma2 = sma2List.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal indexStdDev = indexStdDevList.ElementAtOrDefault(i);
            decimal indexSma = indexSmaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal c = stdDev != 0 ? (sma2 - sma1) / stdDev : 0;
            decimal z = indexStdDev != 0 && c != 0 ? (i - indexSma) / indexStdDev * c : 0;

            decimal prevY = yList.LastOrDefault();
            decimal y = sma1 + (z * stdDev);
            yList.Add(y);

            var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Llsma", yList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = yList;
        stockData.IndicatorName = IndicatorName.LightLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linear Extrapolation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateLinearExtrapolation(this StockData stockData, int length = 500)
    {
        List<decimal> extList = new();
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevY = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorY = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal priorY2 = i >= length * 2 ? inputList.ElementAtOrDefault(i - (length * 2)) : 0;
            decimal priorX = i >= length ? xList.ElementAtOrDefault(i - length) : 0;
            decimal priorX2 = i >= length * 2 ? xList.ElementAtOrDefault(i - (length * 2)) : 0;

            decimal x = i;
            xList.Add(i);

            decimal prevExt = extList.LastOrDefault();
            decimal ext = priorX2 - priorX != 0 && priorY2 - priorY != 0 ? priorY + ((x - priorX) / (priorX2 - priorX) * (priorY2 - priorY)) : priorY;
            extList.Add(ext);

            var signal = GetCompareSignal(currentValue - ext, prevY - prevExt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LinExt", extList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = extList;
        stockData.IndicatorName = IndicatorName.LinearExtrapolation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linear Regression Line
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateLinearRegressionLine(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14)
    {
        List<decimal> regList = new();
        List<decimal> corrList = new();
        List<decimal> yList = new();
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var yMaList = GetMovingAverageList(stockData, maType, length, inputList);
        var myList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            yList.Add(currentValue);

            decimal x = i;
            xList.Add(x);

            var corr = GoodnessOfFit.R(yList.TakeLastExt(length).Select(x => (double)x), xList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.Add((decimal)corr);
        }

        var xMaList = GetMovingAverageList(stockData, maType, length, xList);
        stockData.CustomValuesList = xList;
        var mxList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList; ;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal my = myList.ElementAtOrDefault(i);
            decimal mx = mxList.ElementAtOrDefault(i);
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal yMa = yMaList.ElementAtOrDefault(i);
            decimal xMa = xMaList.ElementAtOrDefault(i);
            decimal x = xList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal slope = mx != 0 ? corr * (my / mx) : 0;
            decimal inter = yMa - (slope * xMa);

            decimal prevReg = regList.LastOrDefault();
            decimal reg = (x * slope) + inter;
            regList.Add(reg);

            var signal = GetCompareSignal(currentValue - reg, prevValue - prevReg);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LinReg", regList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = regList;
        stockData.IndicatorName = IndicatorName.LinearRegressionLine;

        return stockData;
    }

    /// <summary>
    /// Calculates the IIR Least Squares Estimate
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateIIRLeastSquaresEstimate(this StockData stockData, int length = 100)
    {
        List<decimal> sList = new();
        List<decimal> sEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a = (decimal)4 / (length + 2);
        int halfLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevS = i >= 1 ? sList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevSEma = sEmaList.LastOrDefault();
            decimal sEma = CalculateEMA(prevS, prevSEma, halfLength);
            sEmaList.Add(prevSEma);

            decimal s = (a * currentValue) + prevS - (a * sEma);
            sList.Add(s);

            var signal = GetCompareSignal(currentValue - s, prevValue - prevS);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "IIRLse", sList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sList;
        stockData.IndicatorName = IndicatorName.IIRLeastSquaresEstimate;

        return stockData;
    }

    /// <summary>
    /// Calculates the Inverse Distance Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateInverseDistanceWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> idwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                decimal weight = 0;
                for (int k = 0; k <= length - 1; k++)
                {
                    decimal prevValue2 = i >= k ? inputList.ElementAtOrDefault(i - k) : 0;
                    weight += Math.Abs(prevValue - prevValue2);
                }

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevIdwma = idwmaList.LastOrDefault();
            decimal idwma = weightedSum != 0 ? sum / weightedSum : 0;
            idwmaList.Add(idwma);

            var signal = GetCompareSignal(currentValue - idwma, prevVal - prevIdwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Idwma", idwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = idwmaList;
        stockData.IndicatorName = IndicatorName.InverseDistanceWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trimean
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTrimean(this StockData stockData, int length = 14)
    {
        List<decimal> tempList = new();
        List<decimal> medianList = new();
        List<decimal> q1List = new();
        List<decimal> q3List = new();
        List<decimal> trimeanList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = tempList.LastOrDefault();
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            var lookBackList = tempList.TakeLastExt(length);

            decimal q1 = lookBackList.PercentileNearestRank(25);
            q1List.Add(q1);

            decimal median = lookBackList.PercentileNearestRank(50);
            medianList.Add(median);

            decimal q3 = lookBackList.PercentileNearestRank(75);
            q3List.Add(q3);

            decimal prevTrimean = trimeanList.LastOrDefault();
            decimal trimean = (q1 + (2 * median) + q3) / 4;
            trimeanList.Add(trimean);

            var signal = GetCompareSignal(currentValue - trimean, prevValue - prevTrimean);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Trimean", trimeanList },
            { "Q1", q1List },
            { "Median", medianList },
            { "Q3", q3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trimeanList;
        stockData.IndicatorName = IndicatorName.Trimean;

        return stockData;
    }

    /// <summary>
    /// Calculates the Optimal Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOptimalWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> tempList = new();
        List<decimal> owmaList = new();
        List<decimal> prevOwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevVal = tempList.LastOrDefault();
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal prevOwma = i >= 1 ? owmaList.ElementAtOrDefault(i - 1) : 0;
            prevOwmaList.Add(prevOwma);

            var corr = GoodnessOfFit.R(tempList.TakeLastExt(length).Select(x => (double)x), prevOwmaList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, (decimal)corr);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal owma = weightedSum != 0 ? sum / weightedSum : 0;
            owmaList.Add(owma);

            var signal = GetCompareSignal(currentValue - owma, prevVal - prevOwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Owma", owmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = owmaList;
        stockData.IndicatorName = IndicatorName.OptimalWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Overshoot Reduction Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateOvershootReductionMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14)
    {
        List<decimal> indexList = new();
        List<decimal> bList = new();
        List<decimal> dList = new();
        List<decimal> bSmaList = new();
        List<decimal> corrList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int length1 = (int)Math.Ceiling((decimal)length / 2);

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

        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal index = indexList.ElementAtOrDefault(i);
            decimal indexSma = indexSmaList.ElementAtOrDefault(i);
            decimal indexStdDev = indexStdDevList.ElementAtOrDefault(i);
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevD = i >= 1 ? dList.ElementAtOrDefault(i - 1) != 0 ? dList.ElementAtOrDefault(i - 1) : prevValue : prevValue;
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal a = indexStdDev != 0 && corr != 0 ? (index - indexSma) / indexStdDev * corr : 0;

            decimal b = Math.Abs(prevD - currentValue);
            bList.Add(b);

            decimal bSma = bList.TakeLastExt(length1).Average();
            bSmaList.Add(bSma);

            decimal highest = bSmaList.TakeLastExt(length).Max();
            decimal c = highest != 0 ? b / highest : 0;

            decimal d = sma + (a * (stdDev * c));
            dList.Add(d);

            var signal = GetCompareSignal(currentValue - d, prevValue - prevD);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Orma", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dList;
        stockData.IndicatorName = IndicatorName.OvershootReductionMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Variable Index Dynamic Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVariableIndexDynamicAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<decimal> vidyaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        var cmoList = CalculateChandeMomentumOscillator(stockData, maType, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentCmo = Math.Abs(cmoList.ElementAtOrDefault(i) / 100);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevVidya = vidyaList.LastOrDefault();
            decimal currentVidya = (currentValue * alpha * currentCmo) + (prevVidya * (1 - (alpha * currentCmo)));
            vidyaList.Add(currentVidya);

            var signal = GetCompareSignal(currentValue - currentVidya, prevValue - prevVidya);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vidya", vidyaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vidyaList;
        stockData.IndicatorName = IndicatorName.VariableIndexDynamicAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Natural Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateNaturalMovingAverage(this StockData stockData, int length = 40)
    {
        List<decimal> lnList = new();
        List<decimal> nmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.Add(ln);

            decimal num = 0, denom = 0;
            for (int j = 0; j < length; j++)
            {
                decimal currentLn = i >= j ? lnList.ElementAtOrDefault(i - j) : 0;
                decimal prevLn = i >= j + 1 ? lnList.ElementAtOrDefault(i - (j + 1)) : 0;
                decimal oi = Math.Abs(currentLn - prevLn);
                num += oi * (Sqrt(j + 1) - Sqrt(j));
                denom += oi;
            }

            decimal ratio = denom != 0 ? num / denom : 0;
            decimal prevNma = nmaList.LastOrDefault();
            decimal nma = (currentValue * ratio) + (prevValue * (1 - ratio));
            nmaList.Add(nma);

            var signal = GetCompareSignal(currentValue - nma, prevValue - prevNma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Nma", nmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nmaList;
        stockData.IndicatorName = IndicatorName.NaturalMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Symmetrically Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSymmetricallyWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> swmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int floorLength = (int)Math.Floor((decimal)length / 2);
        int roundLength = (int)Math.Round((decimal)length / 2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal nr = 0, nl = 0, sr = 0, sl = 0;
            if (floorLength == roundLength)
            {
                for (int j = 0; j <= floorLength - 1; j++)
                {
                    decimal wr = (length - (length - 1 - j)) * length;
                    decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                    nr += wr;
                    sr += prevVal * wr;
                }

                for (int j = floorLength; j <= length - 1; j++)
                {
                    decimal wl = (length - j) * length;
                    decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                    nl += wl;
                    sl += prevVal * wl;
                }
            }
            else
            {
                for (int j = 0; j <= floorLength; j++)
                {
                    decimal wr = (length - (length - 1 - j)) * length;
                    decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                    nr += wr;
                    sr += prevVal * wr;
                }

                for (int j = roundLength; j <= length - 1; j++)
                {
                    decimal wl = (length - j) * length;
                    decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                    nl += wl;
                    sl += prevVal * wl;
                }
            }

            decimal prevSwma = swmaList.LastOrDefault();
            decimal swma = nr + nl != 0 ? (sr + sl) / (nr + nl) : 0;
            swmaList.AddRounded(swma);

            var signal = GetCompareSignal(currentValue - swma, prevValue - prevSwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Swma", swmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = swmaList;
        stockData.IndicatorName = IndicatorName.SymmetricallyWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Generalized Double Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateGeneralizedDoubleExponentialMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 5, decimal factor = 0.7m)
    {
        List<decimal> gdList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentEma1 = ema1List.ElementAtOrDefault(i);
            decimal currentEma2 = ema2List.ElementAtOrDefault(i);

            decimal prevGd = gdList.LastOrDefault();
            decimal gd = (currentEma1 * (1 + factor)) - (currentEma2 * factor);
            gdList.Add(gd);

            var signal = GetCompareSignal(currentValue - gd, prevValue - prevGd);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gdema", gdList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gdList;
        stockData.IndicatorName = IndicatorName.GeneralizedDoubleExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the General Filter Estimator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="beta"></param>
    /// <param name="gamma"></param>
    /// <param name="zeta"></param>
    /// <returns></returns>
    public static StockData CalculateGeneralFilterEstimator(this StockData stockData, int length = 100, decimal beta = 5.25m, decimal gamma = 1,
        decimal zeta = 1)
    {
        List<decimal> dList = new();
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int p = beta != 0 ? (int)Math.Ceiling(length / beta) : 0;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorB = i >= p ? bList.ElementAtOrDefault(i - p) : currentValue;
            decimal a = currentValue - priorB;

            decimal prevB = i >= 1 ? bList.ElementAtOrDefault(i - 1) : currentValue;
            decimal b = prevB + (a / p * gamma);
            bList.Add(b);

            decimal priorD = i >= p ? dList.ElementAtOrDefault(i - p) : b;
            decimal c = b - priorD;

            decimal prevD = i >= 1 ? dList.ElementAtOrDefault(i - 1) : currentValue;
            decimal d = prevD + (((zeta * a) + ((1 - zeta) * c)) / p * gamma);
            dList.Add(d);

            var signal = GetCompareSignal(currentValue - d, prevValue - prevD);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Gfe", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dList;
        stockData.IndicatorName = IndicatorName.GeneralFilterEstimator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Henderson Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateHendersonWeightedMovingAverage(this StockData stockData, int length = 7)
    {
        List<decimal> hwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int termMult = MinOrMax((int)Math.Floor((decimal)(length - 1) / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                int m = termMult;
                int n = j - termMult;
                decimal numerator = 315 * (Pow(m + 1, 2) - Pow(n, 2)) * (Pow(m + 2, 2) - Pow(n, 2)) * (Pow(m + 3, 2) -
                    Pow(n, 2)) * ((3 * Pow(m + 2, 2)) - (11 * Pow(n, 2)) - 16);
                decimal denominator = 8 * (m + 2) * (Pow(m + 2, 2) - 1) * ((4 * Pow(m + 2, 2)) - 1) * ((4 * Pow(m + 2, 2)) - 9) *
                    ((4 * Pow(m + 2, 2)) - 25);
                decimal weight = denominator != 0 ? numerator / denominator : 0;
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevHwma = hwmaList.LastOrDefault();
            decimal hwma = weightedSum != 0 ? sum / weightedSum : 0;
            hwmaList.Add(hwma);

            var signal = GetCompareSignal(currentValue - hwma, prevVal - prevHwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hwma", hwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hwmaList;
        stockData.IndicatorName = IndicatorName.HendersonWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Holt Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="alphaLength"></param>
    /// <param name="gammaLength"></param>
    /// <returns></returns>
    public static StockData CalculateHoltExponentialMovingAverage(this StockData stockData, int alphaLength = 20, int gammaLength = 20)
    {
        List<decimal> hemaList = new();
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (alphaLength + 1);
        decimal gamma = (decimal)2 / (gammaLength + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevB = i >= 1 ? bList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevHema = hemaList.LastOrDefault();
            decimal hema = ((1 - alpha) * (prevHema + prevB)) + (alpha * currentValue);
            hemaList.Add(hema);

            decimal b = ((1 - gamma) * prevB) + (gamma * (hema - prevHema));
            bList.Add(b);

            var signal = GetCompareSignal(currentValue - hema, prevValue - prevHema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hema", hemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hemaList;
        stockData.IndicatorName = IndicatorName.HoltExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hull Estimate
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateHullEstimate(this StockData stockData, int length = 50)
    {
        List<decimal> hemaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int maLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var wmaList = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, maLength, inputList);
        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, maLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal currentWma = wmaList.ElementAtOrDefault(i);
            decimal currentEma = emaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevHema = hemaList.LastOrDefault();
            decimal hema = (3 * currentWma) - (2 * currentEma);
            hemaList.Add(hema);

            var signal = GetCompareSignal(currentValue - hema, prevValue - prevHema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "He", hemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hemaList;
        stockData.IndicatorName = IndicatorName.HullEstimate;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hampel Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="scalingFactor"></param>
    /// <returns></returns>
    public static StockData CalculateHampelFilter(this StockData stockData, int length = 14, decimal scalingFactor = 3)
    {
        List<decimal> tempList = new();
        List<decimal> absDiffList = new();
        List<decimal> hfList = new();
        List<decimal> hfEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = tempList.LastOrDefault();
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal sampleMedian = tempList.TakeLastExt(length).Median();
            decimal absDiff = Math.Abs(currentValue - sampleMedian);
            absDiffList.Add(absDiff);

            decimal mad = absDiffList.TakeLastExt(length).Median();
            decimal hf = absDiff <= scalingFactor * mad ? currentValue : sampleMedian;
            hfList.Add(hf);

            decimal prevHfEma = hfEmaList.LastOrDefault();
            decimal hfEma = (alpha * hf) + ((1 - alpha) * prevHfEma);
            hfEmaList.Add(hfEma);

            var signal = GetCompareSignal(currentValue - hfEma, prevValue - prevHfEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hf", hfEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hfEmaList;
        stockData.IndicatorName = IndicatorName.HampelFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hybrid Convolution Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateHybridConvolutionFilter(this StockData stockData, int length = 14)
    {
        List<decimal> outputList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevOutput = i >= 1 ? outputList.ElementAtOrDefault(i - 1) : currentValue;
            decimal output = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal sign = (0.5m * (1 - Cos(MinOrMax((decimal)j / length * Pi, 0.99m, 0.01m))));
                decimal d = sign - (0.5m * (1 - Cos(MinOrMax((decimal)(j - 1) / length, 0.99m, 0.01m))));
                decimal prevValue = i >= j - 1 ? inputList.ElementAtOrDefault(i - (j - 1)) : 0;
                output += ((sign * prevOutput) + ((1 - sign) * prevValue)) * d;
            }
            outputList.Add(output);

            var signal = GetCompareSignal(currentValue - output, prevVal - prevOutput);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hcf", outputList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = outputList;
        stockData.IndicatorName = IndicatorName.HybridConvolutionFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fibonacci Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFibonacciWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> fibonacciWmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal phi = (1 + Sqrt(5)) / 2;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal pow = Pow(phi, length - j);
                decimal weight = (pow - (Pow(-1, j) / pow)) / Sqrt(5);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevFwma = fibonacciWmaList.LastOrDefault();
            decimal fwma = weightedSum != 0 ? sum / weightedSum : 0;
            fibonacciWmaList.Add(fwma);

            var signal = GetCompareSignal(currentValue - fwma, prevVal - prevFwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fwma", fibonacciWmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fibonacciWmaList;
        stockData.IndicatorName = IndicatorName.FibonacciWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Farey Sequence Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFareySequenceWeightedMovingAverage(this StockData stockData, int length = 5)
    {
        List<decimal> fswmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal[] array = new decimal[4] { 0, 1, 1, length };
        List<decimal> resList = new();

        while (array[2] <= length)
        {
            decimal a = array[0];
            decimal b = array[1];
            decimal c = array[2];
            decimal d = array[3];
            decimal k = Math.Floor((length + b) / array[3]);

            array[0] = c;
            array[1] = d;
            array[2] = (k * c) - a;
            array[3] = (k * d) - b;

            decimal res = array[1] != 0 ? Math.Round(array[0] / array[1], 3) : 0;
            resList.Insert(0, res);
        }

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int k = 0; k < resList.Count; k++)
            {
                decimal prevValue = i >= k ? inputList.ElementAtOrDefault(i - k) : 0;
                decimal weight = resList.ElementAtOrDefault(k);

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevFswma = fswmaList.LastOrDefault();
            decimal fswma = weightedSum != 0 ? sum / weightedSum : 0;
            fswmaList.Add(fswma);

            var signal = GetCompareSignal(currentValue - fswma, prevVal - prevFswma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fswma", fswmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fswmaList;
        stockData.IndicatorName = IndicatorName.FareySequenceWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Fractal Adaptive Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersFractalAdaptiveMovingAverage(this StockData stockData, int length = 20)
    {
        List<decimal> filterList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        int halfP = MinOrMax((int)Math.Ceiling((decimal)length / 2));

        var (highestList1, lowestList1) = GetMaxAndMinValuesList(highList, lowList, length);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(highList, lowList, halfP);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevFilter = i >= 1 ? filterList.LastOrDefault() : currentValue;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal highestHigh1 = highestList1.ElementAtOrDefault(i);
            decimal lowestLow1 = lowestList1.ElementAtOrDefault(i);
            decimal highestHigh2 = highestList2.ElementAtOrDefault(i);
            decimal lowestLow2 = lowestList2.ElementAtOrDefault(i);
            decimal highestHigh3 = highestList2.ElementAtOrDefault(Math.Max(i - halfP, i));
            decimal lowestLow3 = lowestList2.ElementAtOrDefault(Math.Max(i - halfP, i));
            decimal n3 = (highestHigh1 - lowestLow1) / length;
            decimal n1 = (highestHigh2 - lowestLow2) / halfP;
            decimal n2 = (highestHigh3 - lowestLow3) / halfP;
            decimal dm = n1 > 0 && n2 > 0 && n3 > 0 ? (Log(n1 + n2) - Log(n3)) / Log(2) : 0;

            decimal alpha = MinOrMax(Exp(-4.6m * (dm - 1)), 1, 0.01m);
            decimal filter = (alpha * currentValue) + ((1 - alpha) * prevFilter);
            filterList.Add(filter);

            var signal = GetCompareSignal(currentValue - filter, prevValue - prevFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fama", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersFractalAdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Falling Rising Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFallingRisingFilter(this StockData stockData, int length = 14)
    {
        List<decimal> tempList = new();
        List<decimal> aList = new();
        List<decimal> errorList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : 0;
            decimal prevError = i >= 1 ? errorList.ElementAtOrDefault(i - 1) : 0;

            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            tempList.Add(prevValue);

            var lbList = tempList.TakeLastExt(length).ToList();
            decimal beta = currentValue > lbList.Max() || currentValue < lbList.Min() ? 1 : alpha;
            decimal a = prevA + (alpha * prevError) + (beta * prevError);
            aList.Add(a);

            decimal error = currentValue - a;
            errorList.Add(error);

            var signal = GetCompareSignal(error, prevError);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Frf", aList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = aList;
        stockData.IndicatorName = IndicatorName.FallingRisingFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fisher Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFisherLeastSquaresMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100)
    {
        List<decimal> bList = new();
        List<decimal> indexList = new();
        List<decimal> diffList = new();
        List<decimal> absDiffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevSrcList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var smaSrcList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal index = i;
            indexList.Add(index);
        }

        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDevSrc = stdDevSrcList.ElementAtOrDefault(i);
            decimal indexStdDev = indexStdDevList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevB = i >= 1 ? bList.ElementAtOrDefault(i - 1) : currentValue;
            decimal indexSma = indexSmaList.ElementAtOrDefault(i);
            decimal sma = smaSrcList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal diff = currentValue - prevB;
            diffList.Add(diff);

            decimal absDiff = Math.Abs(diff);
            absDiffList.Add(absDiff);

            decimal e = absDiffList.TakeLastExt(length).Average();
            decimal z = e != 0 ? diffList.TakeLastExt(length).Average() / e : 0;
            decimal r = Exp(2 * z) + 1 != 0 ? (Exp(2 * z) - 1) / (Exp(2 * z) + 1) : 0;
            decimal a = indexStdDev != 0 && r != 0 ? (i - indexSma) / indexStdDev * r : 0;

            decimal b = sma + (a * stdDevSrc);
            bList.Add(b);

            var signal = GetCompareSignal(currentValue - b, prevValue - prevB);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Flsma", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.FisherLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kaufman Adaptive Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKaufmanAdaptiveLeastSquaresMovingAverage(this StockData stockData,
        MovingAvgType maType = MovingAvgType.KaufmanAdaptiveMovingAverage, int length = 100)
    {
        List<decimal> kalsmaList = new();
        List<decimal> indexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var kamaList = CalculateKaufmanAdaptiveCorrelationOscillator(stockData, maType, length);
        var indexStList = kamaList.OutputValues["IndexSt"];
        var srcStList = kamaList.OutputValues["SrcSt"];
        var rList = kamaList.OutputValues["Kaco"];
        var srcMaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal index = i;
            indexList.Add(index);
        }

        var indexMaList = GetMovingAverageList(stockData, maType, length, indexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            var indexSt = indexStList.ElementAtOrDefault(i);
            var srcSt = srcStList.ElementAtOrDefault(i);
            var srcMa = srcMaList.ElementAtOrDefault(i);
            var indexMa = indexMaList.ElementAtOrDefault(i);
            var r = rList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal alpha = indexSt != 0 ? srcSt / indexSt * r : 0;
            decimal beta = srcMa - (alpha * indexMa);

            decimal prevKalsma = kalsmaList.LastOrDefault();
            decimal kalsma = (alpha * i) + beta;
            kalsmaList.Add(kalsma);

            var signal = GetCompareSignal(currentValue - kalsma, prevValue - prevKalsma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Kalsma", kalsmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kalsmaList;
        stockData.IndicatorName = IndicatorName.KaufmanAdaptiveLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kalman Smoother
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateKalmanSmoother(this StockData stockData, int length = 200)
    {
        List<decimal> veloList = new();
        List<decimal> kfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevKf = i >= 1 ? kfList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal dk = currentValue - prevKf;
            decimal smooth = prevKf + (dk * Sqrt((decimal)length / 10000 * 2));

            decimal prevVelo = i >= 1 ? veloList.ElementAtOrDefault(i - 1) : 0;
            decimal velo = prevVelo + ((decimal)length / 10000 * dk);
            veloList.Add(velo);

            decimal kf = smooth + velo;
            kfList.Add(kf);

            var signal = GetCompareSignal(currentValue - kf, prevValue - prevKf);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ks", kfList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kfList;
        stockData.IndicatorName = IndicatorName.KalmanSmoother;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Adjusted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateVolumeAdjustedMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, decimal factor = 0.67m)
    {
        List<decimal> priceVolumeRatioList = new();
        List<decimal> priceVolumeRatioSumList = new();
        List<decimal> vamaList = new();
        List<decimal> volumeRatioList = new();
        List<decimal> volumeRatioSumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList); ;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentVolume = volumeList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal volumeSma = volumeSmaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal volumeIncrement = volumeSma * factor;

            decimal volumeRatio = volumeIncrement != 0 ? currentVolume / volumeIncrement : 0;
            volumeRatioList.Add(volumeRatio);

            decimal priceVolumeRatio = currentValue * volumeRatio;
            priceVolumeRatioList.Add(priceVolumeRatio);

            decimal volumeRatioSum = volumeRatioList.TakeLastExt(length).Sum();
            volumeRatioSumList.Add(volumeRatioSum);

            decimal priceVolumeRatioSum = priceVolumeRatioList.TakeLastExt(length).Sum();
            priceVolumeRatioSumList.Add(priceVolumeRatioSum);

            decimal prevVama = vamaList.LastOrDefault();
            decimal vama = volumeRatioSum != 0 ? priceVolumeRatioSum / volumeRatioSum : 0;
            vamaList.Add(vama);

            var signal = GetCompareSignal(currentValue - vama, prevValue - prevVama);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vama", vamaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vamaList;
        stockData.IndicatorName = IndicatorName.VolumeAdjustedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volatility Wave Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="kf"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilityWaveMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 20, decimal kf = 2.5m)
    {
        List<decimal> zlmapList = new();
        List<decimal> pmaList = new();
        List<decimal> pList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int s = MinOrMax((int)Math.Ceiling(Sqrt(length)));

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal sdPct = currentValue != 0 ? stdDev / currentValue * 100 : 0;

            decimal p = sdPct >= 0 ? MinOrMax(Sqrt(sdPct) * kf, 4, 1) : 1;
            pList.Add(p);
        }

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal p = pList.ElementAtOrDefault(i);

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, p);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal pma = weightedSum != 0 ? sum / weightedSum : 0;
            pmaList.Add(pma);
        }

        var wmap1List = GetMovingAverageList(stockData, maType, s, pmaList);
        var wmap2List = GetMovingAverageList(stockData, maType, s, wmap1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal wmap1 = wmap1List.ElementAtOrDefault(i);
            decimal wmap2 = wmap2List.ElementAtOrDefault(i);

            decimal prevZlmap = zlmapList.LastOrDefault();
            decimal zlmap = (2 * wmap1) - wmap2;
            zlmapList.Add(zlmap);

            var signal = GetCompareSignal(currentValue - zlmap, prevValue - prevZlmap);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vwma", zlmapList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zlmapList;
        stockData.IndicatorName = IndicatorName.VolatilityWaveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Variable Adaptive Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVariableAdaptiveMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14)
    {
        List<decimal> vmaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        var cList = GetMovingAverageList(stockData, maType, length, inputList);
        var oList = GetMovingAverageList(stockData, maType, length, openList);
        var hList = GetMovingAverageList(stockData, maType, length, highList);
        var lList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal c = cList.ElementAtOrDefault(i);
            decimal o = oList.ElementAtOrDefault(i);
            decimal h = hList.ElementAtOrDefault(i);
            decimal l = lList.ElementAtOrDefault(i);
            decimal lv = h - l != 0 ? MinOrMax(Math.Abs(c - o) / (h - l), 0.99m, 0.01m) : 0;

            decimal prevVma = i >= 1 ? vmaList.ElementAtOrDefault(i - 1) : currentValue;
            decimal vma = (lv * currentValue) + ((1 - lv) * prevVma);
            vmaList.Add(vma);

            var signal = GetCompareSignal(currentValue - vma, prevValue - prevVma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vama", vmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vmaList;
        stockData.IndicatorName = IndicatorName.VariableAdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Variable Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVariableMovingAverage(this StockData stockData, int length = 6)
    {
        List<decimal> vmaList = new();
        List<decimal> pdmsList = new();
        List<decimal> pdisList = new();
        List<decimal> mdmsList = new();
        List<decimal> mdisList = new();
        List<decimal> isList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal k = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal pdm = Math.Max(currentValue - prevValue, 0);
            decimal mdm = Math.Max(prevValue - currentValue, 0);

            decimal prevPdms = pdmsList.LastOrDefault();
            decimal pdmS = ((1 - k) * prevPdms) + (k * pdm);
            pdmsList.Add(pdmS);

            decimal prevMdms = mdmsList.LastOrDefault();
            decimal mdmS = ((1 - k) * prevMdms) + (k * mdm);
            mdmsList.Add(mdmS);

            decimal s = pdmS + mdmS;
            decimal pdi = s != 0 ? pdmS / s : 0;
            decimal mdi = s != 0 ? mdmS / s : 0;

            decimal prevPdis = pdisList.LastOrDefault();
            decimal pdiS = ((1 - k) * prevPdis) + (k * pdi);
            pdisList.Add(pdiS);

            decimal prevMdis = mdisList.LastOrDefault();
            decimal mdiS = ((1 - k) * prevMdis) + (k * mdi);
            mdisList.Add(mdiS);

            decimal d = Math.Abs(pdiS - mdiS);
            decimal s1 = pdiS + mdiS;
            decimal dS1 = s1 != 0 ? d / s1 : 0;

            decimal prevIs = isList.LastOrDefault();
            decimal iS = ((1 - k) * prevIs) + (k * dS1);
            isList.Add(iS);

            var lbList = isList.TakeLastExt(length).ToList();
            decimal hhv = lbList.Max();
            decimal llv = lbList.Min();
            decimal d1 = hhv - llv;
            decimal vI = d1 != 0 ? (iS - llv) / d1 : 0;

            decimal prevVma = vmaList.LastOrDefault();
            decimal vma = ((1 - k) * vI * prevVma) + (k * vI * currentValue);
            vmaList.Add(vma);

            var signal = GetCompareSignal(currentValue - vma, prevValue - prevVma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vma", vmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vmaList;
        stockData.IndicatorName = IndicatorName.VariableMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volatility Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="lbLength"></param>
    /// <param name="smoothLength"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateVolatilityMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20, int lbLength = 10, int smoothLength = 3)
    {
        List<decimal> kList = new();
        List<decimal> vma1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, lbLength, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, lbLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal dev = stdDevList.ElementAtOrDefault(i);
            decimal upper = sma + dev;
            decimal lower = sma - dev;

            decimal k = upper - lower != 0 ? (currentValue - sma) / (upper - lower) * 100 * 2 : 0;
            kList.Add(k);
        }

        var kMaList = GetMovingAverageList(stockData, maType, smoothLength, kList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal kMa = kMaList.ElementAtOrDefault(i);
            decimal kNorm = Math.Min(Math.Max(kMa, -100), 100);
            decimal kAbs = Math.Round(Math.Abs(kNorm) / lbLength);
            decimal kRescaled = RescaleValue(kAbs, 10, 0, length, 0, true);
            int vLength = (int)Math.Round(Math.Max(kRescaled, 1));

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= vLength - 1; j++)
            {
                decimal weight = vLength - j;
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal vma1 = weightedSum != 0 ? sum / weightedSum : 0;
            vma1List.Add(vma1);
        }

        var vma2List = GetMovingAverageList(stockData, maType, smoothLength, vma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal vma = vma2List.ElementAtOrDefault(i);
            decimal prevVma = i >= 1 ? vma2List.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(currentValue - vma, prevValue - prevVma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vma", vma2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vma2List;
        stockData.IndicatorName = IndicatorName.VolatilityMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vertical Horizontal Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVerticalHorizontalMovingAverage(this StockData stockData, int length = 50)
    {
        List<decimal> changeList = new();
        List<decimal> vhmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);

            decimal priceChange = Math.Abs(currentValue - priorValue);
            changeList.Add(priceChange);

            decimal numerator = highest - lowest;
            decimal denominator = changeList.TakeLastExt(length).Sum();
            decimal vhf = denominator != 0 ? numerator / denominator : 0;

            decimal prevVhma = vhmaList.LastOrDefault();
            decimal vhma = prevVhma + (Pow(vhf, 2) * (currentValue - prevVhma));
            vhmaList.Add(vhma);

            var signal = GetCompareSignal(currentValue - vhma, prevValue - prevVhma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Vhma", vhmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vhmaList;
        stockData.IndicatorName = IndicatorName.VerticalHorizontalMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the McNicholl Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMcNichollMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 20)
    {
        List<decimal> mnmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ema1 = ema1List.ElementAtOrDefault(i);
            decimal ema2 = ema2List.ElementAtOrDefault(i);

            decimal prevMnma = mnmaList.LastOrDefault();
            decimal mnma = 1 - alpha != 0 ? (((2 - alpha) * ema1) - ema2) / (1 - alpha) : 0;
            mnmaList.Add(mnma);

            var signal = GetCompareSignal(currentValue - mnma, prevValue - prevMnma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mnma", mnmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mnmaList;
        stockData.IndicatorName = IndicatorName.McNichollMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Compound Ratio Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateCompoundRatioMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
        int length = 20)
    {
        List<decimal> coraRawList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal r = Pow(length, ((decimal)1 / (length - 1)) - 1);
        int smoothLength = Math.Max((int)Math.Round(Math.Sqrt(length)), 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sum = 0, weightedSum = 0, bas = 1 + (r * 2);
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(bas, length - i);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal coraRaw = weightedSum != 0 ? sum / weightedSum : 0;
            coraRawList.Add(coraRaw);
        }

        var coraWaveList = GetMovingAverageList(stockData, maType, smoothLength, coraRawList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal coraWave = coraWaveList.ElementAtOrDefault(i);
            decimal prevCoraWave = i >= 1 ? coraWaveList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(currentValue - coraWave, prevValue - prevCoraWave);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Crma", coraWaveList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = coraWaveList;
        stockData.IndicatorName = IndicatorName.CompoundRatioMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Cubed Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateCubedWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> cwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, 3);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevCwma = cwmaList.LastOrDefault();
            decimal cwma = weightedSum != 0 ? sum / weightedSum : 0;
            cwmaList.Add(cwma);

            var signal = GetCompareSignal(currentValue - cwma, prevVal - prevCwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cwma", cwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cwmaList;
        stockData.IndicatorName = IndicatorName.CubedWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Corrected Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateCorrectedMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 35)
    {
        List<decimal> cmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var v1List = CalculateStandardDeviationVolatility(stockData, length).OutputValues["Variance"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal prevCma = i >= 1 ? cmaList.ElementAtOrDefault(i - 1) : sma;
            decimal v1 = v1List.ElementAtOrDefault(i);
            decimal v2 = Pow(prevCma - sma, 2);
            decimal v3 = v1 == 0 || v2 == 0 ? 1 : v2 / (v1 + v2);

            decimal tolerance = Pow(10, -5), err = 1, kPrev = 1, k = 1;
            for (int j = 0; j <= 5000; j++)
            {
                if (err > tolerance)
                {
                    k = v3 * kPrev * (2 - kPrev);
                    err = kPrev - k;
                    kPrev = k;
                }
            }

            decimal cma = prevCma + (k * (sma - prevCma));
            cmaList.Add(cma);

            var signal = GetCompareSignal(currentValue - cma, prevValue - prevCma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cma", cmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cmaList;
        stockData.IndicatorName = IndicatorName.CorrectedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Double Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDoubleExponentialMovingAverage(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
    {
        List<decimal> demaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentEma = ema1List.ElementAtOrDefault(i);
            decimal currentEma2 = ema2List.ElementAtOrDefault(i);

            decimal prevDema = demaList.LastOrDefault();
            decimal dema = (2 * currentEma) - currentEma2;
            demaList.Add(dema);

            var signal = GetCompareSignal(currentValue - dema, prevValue - prevDema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dema", demaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = demaList;
        stockData.IndicatorName = IndicatorName.DoubleExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Pentuple Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePentupleExponentialMovingAverage(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20)
    {
        List<decimal> pemaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
        var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);
        var ema4List = GetMovingAverageList(stockData, maType, length, ema3List);
        var ema5List = GetMovingAverageList(stockData, maType, length, ema4List);
        var ema6List = GetMovingAverageList(stockData, maType, length, ema5List);
        var ema7List = GetMovingAverageList(stockData, maType, length, ema6List);
        var ema8List = GetMovingAverageList(stockData, maType, length, ema7List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ema1 = ema1List.ElementAtOrDefault(i);
            decimal ema2 = ema2List.ElementAtOrDefault(i);
            decimal ema3 = ema3List.ElementAtOrDefault(i);
            decimal ema4 = ema4List.ElementAtOrDefault(i);
            decimal ema5 = ema5List.ElementAtOrDefault(i);
            decimal ema6 = ema6List.ElementAtOrDefault(i);
            decimal ema7 = ema7List.ElementAtOrDefault(i);
            decimal ema8 = ema8List.ElementAtOrDefault(i);

            decimal prevPema = pemaList.LastOrDefault();
            decimal pema = (8 * ema1) - (28 * ema2) + (56 * ema3) - (70 * ema4) + (56 * ema5) - (28 * ema6) + (8 * ema7) - ema8;
            pemaList.Add(pema);

            var signal = GetCompareSignal(currentValue - pema, prevValue - prevPema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pema", pemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pemaList;
        stockData.IndicatorName = IndicatorName.PentupleExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Polynomial Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePolynomialLeastSquaresMovingAverage(this StockData stockData, int length = 100)
    {
        List<decimal> sumPow3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevSumPow3 = sumPow3List.LastOrDefault();
            decimal x1Pow1Sum, x2Pow1Sum, x1Pow2Sum, x2Pow2Sum, x1Pow3Sum, x2Pow3Sum, wPow1, wPow2, wPow3, sumPow1 = 0, sumPow2 = 0, sumPow3 = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevValue = i >= j - 1 ? inputList.ElementAtOrDefault(i - (j - 1)) : 0;
                decimal x1 = (decimal)j / length;
                decimal x2 = (decimal)(j - 1) / length;
                decimal ax1 = x1 * x1;
                decimal ax2 = x2 * x2;

                decimal b1Pow1Sum = 0, b2Pow1Sum = 0, b1Pow2Sum = 0, b2Pow2Sum = 0, b1Pow3Sum = 0, b2Pow3Sum = 0;
                for (int k = 1; k <= 3; k++)
                {
                    decimal b1 = (decimal)1 / k * Sin(x1 * k * Pi);
                    decimal b2 = (decimal)1 / k * Sin(x2 * k * Pi);

                    b1Pow1Sum += k == 1 ? b1 : 0;
                    b2Pow1Sum += k == 1 ? b2 : 0;
                    b1Pow2Sum += k <= 2 ? b1 : 0;
                    b2Pow2Sum += k <= 2 ? b2 : 0;
                    b1Pow3Sum += k <= 3 ? b1 : 0; //-V3022
                    b2Pow3Sum += k <= 3 ? b2 : 0; //-V3022
                }

                x1Pow1Sum = ax1 + b1Pow1Sum;
                x2Pow1Sum = ax2 + b2Pow1Sum;
                wPow1 = x1Pow1Sum - x2Pow1Sum;
                sumPow1 += prevValue * wPow1;
                x1Pow2Sum = ax1 + b1Pow2Sum;
                x2Pow2Sum = ax2 + b2Pow2Sum;
                wPow2 = x1Pow2Sum - x2Pow2Sum;
                sumPow2 += prevValue * wPow2;
                x1Pow3Sum = ax1 + b1Pow3Sum;
                x2Pow3Sum = ax2 + b2Pow3Sum;
                wPow3 = x1Pow3Sum - x2Pow3Sum;
                sumPow3 += prevValue * wPow3;
            }
            sumPow3List.Add(sumPow3);

            var signal = GetCompareSignal(currentValue - sumPow3, prevVal - prevSumPow3);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Plsma", sumPow3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sumPow3List;
        stockData.IndicatorName = IndicatorName.PolynomialLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Parametric Corrective Linear Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="alpha"></param>
    /// <param name="per"></param>
    /// <returns></returns>
    public static StockData CalculateParametricCorrectiveLinearMovingAverage(this StockData stockData, int length = 50, decimal alpha = 1, 
        decimal per = 35)
    {
        List<decimal> w1List = new();
        List<decimal> w2List = new();
        List<decimal> vw1List = new();
        List<decimal> vw2List = new();
        List<decimal> rrma1List = new();
        List<decimal> rrma2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
            decimal p1 = i + 1 - (per / 100 * length);
            decimal p2 = i + 1 - ((100 - per) / 100 * length);

            decimal w1 = p1 >= 0 ? p1 : alpha * p1;
            w1List.Add(w1);

            decimal w2 = p2 >= 0 ? p2 : alpha * p2;
            w2List.Add(w2);

            decimal vw1 = prevValue * w1;
            vw1List.Add(vw1);

            decimal vw2 = prevValue * w2;
            vw2List.Add(vw2);

            decimal wSum1 = w1List.TakeLastExt(length).Sum();
            decimal wSum2 = w2List.TakeLastExt(length).Sum();
            decimal sum1 = vw1List.TakeLastExt(length).Sum();
            decimal sum2 = vw2List.TakeLastExt(length).Sum();

            decimal prevRrma1 = rrma1List.LastOrDefault();
            decimal rrma1 = wSum1 != 0 ? sum1 / wSum1 : 0;
            rrma1List.Add(rrma1);

            decimal prevRrma2 = rrma2List.LastOrDefault();
            decimal rrma2 = wSum2 != 0 ? sum2 / wSum2 : 0;
            rrma2List.Add(rrma2);

            var signal = GetCompareSignal(rrma1 - rrma2, prevRrma1 - prevRrma2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pclma", rrma1List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rrma1List;
        stockData.IndicatorName = IndicatorName.ParametricCorrectiveLinearMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Parabolic Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateParabolicWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> pwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, 2);
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevPwma = pwmaList.LastOrDefault();
            decimal pwma = weightedSum != 0 ? sum / weightedSum : 0;
            pwmaList.Add(pwma);

            var signal = GetCompareSignal(currentValue - pwma, prevVal - prevPwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pwma", pwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pwmaList;
        stockData.IndicatorName = IndicatorName.ParabolicWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Parametric Kalman Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateParametricKalmanFilter(this StockData stockData, int length = 50)
    {
        List<decimal> errList = new();
        List<decimal> estList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal priorEst = i >= length ? estList.ElementAtOrDefault(i - length) : prevValue;
            decimal errMea = Math.Abs(priorEst - currentValue);
            decimal errPrv = Math.Abs((currentValue - prevValue) * -1);
            decimal prevErr = i >= 1 ? errList.ElementAtOrDefault(i - 1) : errPrv;
            decimal kg = prevErr != 0 ? prevErr / (prevErr + errMea) : 0;
            decimal prevEst = i >= 1 ? estList.ElementAtOrDefault(i - 1) : prevValue;

            decimal est = prevEst + (kg * (currentValue - prevEst));
            estList.AddRounded(est);

            decimal err = (1 - kg) * errPrv;
            errList.AddRounded(err);

            Signal signal = GetCompareSignal(currentValue - est, prevValue - prevEst);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pkf", estList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = estList;
        stockData.IndicatorName = IndicatorName.ParametricKalmanFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the T Step Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="sc"></param>
    /// <returns></returns>
    public static StockData CalculateTStepLeastSquaresMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100, decimal sc = 0.5m)
    {
        List<decimal> lsList = new();
        List<decimal> bList = new();
        List<decimal> chgList = new();
        List<decimal> tempList = new();
        List<decimal> corrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var efRatioList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            decimal efRatio = efRatioList.ElementAtOrDefault(i);
            decimal prevB = i >= 1 ? bList.ElementAtOrDefault(i - 1) : currentValue;
            decimal er = 1 - efRatio;

            decimal chg = Math.Abs(currentValue - prevB);
            chgList.Add(chg);

            decimal a = chgList.Average() * (1 + er);
            decimal b = currentValue > prevB + a ? currentValue : currentValue < prevB - a ? currentValue : prevB;
            bList.Add(b);

            var corr = GoodnessOfFit.R(bList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.Add((decimal)corr);
        }

        stockData.CustomValuesList = bList;
        var bStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var bSmaList = GetMovingAverageList(stockData, maType, length, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal corr = corrList.ElementAtOrDefault(i);
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal bStdDev = bStdDevList.ElementAtOrDefault(i);
            decimal bSma = bSmaList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevLs = i >= 1 ? lsList.ElementAtOrDefault(i - 1) : currentValue;
            decimal b = bList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal tslsma = (sc * currentValue) + ((1 - sc) * prevLs);
            decimal alpha = bStdDev != 0 ? corr * stdDev / bStdDev : 0;
            decimal beta = sma - (alpha * bSma);

            decimal ls = (alpha * b) + beta;
            lsList.Add(ls);

            var signal = GetCompareSignal(currentValue - ls, prevValue - prevLs);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tslsma", lsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lsList;
        stockData.IndicatorName = IndicatorName.TStepLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Tillson IE2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTillsonIE2(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 15)
    {
        List<decimal> ie2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var linRegList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal a0 = linRegList.ElementAtOrDefault(i);
            decimal a1 = i >= 1 ? linRegList.ElementAtOrDefault(i - 1) : 0;
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal m = a0 - a1 + sma;

            decimal prevIe2 = ie2List.LastOrDefault();
            decimal ie2 = (m + a0) / 2;
            ie2List.Add(ie2);

            var signal = GetCompareSignal(currentValue - ie2, prevValue - prevIe2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ie2", ie2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ie2List;
        stockData.IndicatorName = IndicatorName.TillsonIE2;

        return stockData;
    }

    /// <summary>
    /// Calculates the R2 Adaptive Regression
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateR2AdaptiveRegression(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 100)
    {
        List<decimal> outList = new();
        List<decimal> tempList = new();
        List<decimal> x2List = new();
        List<decimal> x2PowList = new();
        List<decimal> y1List = new();
        List<decimal> y2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
        var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDev = stdDevList.ElementAtOrDefault(i);
            decimal sma = smaList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal currentValue = inputList.ElementAtOrDefault(i);
            tempList.Add(currentValue);

            var lbList = tempList.TakeLastExt(length).Select(x => (double)x);
            decimal y1 = linregList.ElementAtOrDefault(i);
            y1List.Add(y1);

            decimal x2 = i >= 1 ? outList.ElementAtOrDefault(i - 1) : currentValue;
            x2List.Add(x2);

            var x2LbList = x2List.TakeLastExt(length).Select(x => (double)x).ToList();
            var r2x2 = GoodnessOfFit.R(x2LbList, lbList);
            r2x2 = IsValueNullOrInfinity(r2x2) ? 0 : r2x2;
            decimal x2Avg = (decimal)x2LbList.TakeLastExt(length).Average();
            decimal x2Dev = x2 - x2Avg;

            decimal x2Pow = Pow(x2Dev, 2);
            x2PowList.Add(x2Pow);

            decimal x2PowAvg = x2PowList.TakeLastExt(length).Average();
            decimal x2StdDev = x2PowAvg >= 0 ? Sqrt(x2PowAvg) : 0;
            decimal a = x2StdDev != 0 ? stdDev * (decimal)r2x2 / x2StdDev : 0;
            decimal b = sma - (a * x2Avg);

            decimal y2 = (a * x2) + b;
            y2List.Add(y2);

            var ry1 = Math.Pow(GoodnessOfFit.R(y1List.TakeLastExt(length).Select(x => (double)x), lbList), 2);
            ry1 = IsValueNullOrInfinity(ry1) ? 0 : ry1;
            var ry2 = Math.Pow(GoodnessOfFit.R(y2List.TakeLastExt(length).Select(x => (double)x), lbList), 2);
            ry2 = IsValueNullOrInfinity(ry2) ? 0 : ry2;

            decimal prevOutVal = outList.LastOrDefault();
            decimal outval = ((decimal)ry1 * y1) + ((decimal)ry2 * y2) + ((1 - (decimal)(ry1 + ry2)) * x2);
            outList.Add(outval);

            var signal = GetCompareSignal(currentValue - outval, prevValue - prevOutVal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "R2ar", outList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = outList;
        stockData.IndicatorName = IndicatorName.R2AdaptiveRegression;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ratio OCHL Averager
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateRatioOCHLAverager(this StockData stockData)
    {
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal b = currentHigh - currentLow != 0 ? Math.Abs(currentValue - currentOpen) / (currentHigh - currentLow) : 0;
            decimal c = b > 1 ? 1 : b;

            decimal prevD = i >= 1 ? dList.ElementAtOrDefault(i - 1) : currentValue;
            decimal d = (c * currentValue) + ((1 - c) * prevD);
            dList.Add(d);

            var signal = GetCompareSignal(currentValue - d, prevValue - prevD);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rochla", dList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dList;
        stockData.IndicatorName = IndicatorName.RatioOCHLAverager;

        return stockData;
    }

    /// <summary>
    /// Calculates the Regularized Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="lambda"></param>
    /// <returns></returns>
    public static StockData CalculateRegularizedExponentialMovingAverage(this StockData stockData, int length = 14, decimal lambda = 0.5m)
    {
        List<decimal> remaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRema1 = i >= 1 ? remaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRema2 = i >= 2 ? remaList.ElementAtOrDefault(i - 2) : 0;

            decimal rema = (prevRema1 + (alpha * (currentValue - prevRema1)) + (lambda * ((2 * prevRema1) - prevRema2))) / (lambda + 1);
            remaList.Add(rema);

            var signal = GetCompareSignal(currentValue - rema, prevValue - prevRema1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rema", remaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = remaList;
        stockData.IndicatorName = IndicatorName.RegularizedExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Repulsion Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRepulsionMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 100)
    {
        List<decimal> maList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var sma1List = GetMovingAverageList(stockData, maType, length, inputList);
        var sma2List = GetMovingAverageList(stockData, maType, length * 2, inputList);
        var sma3List = GetMovingAverageList(stockData, maType, length * 3, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal sma1 = sma1List.ElementAtOrDefault(i);
            decimal sma2 = sma2List.ElementAtOrDefault(i);
            decimal sma3 = sma3List.ElementAtOrDefault(i);

            decimal prevMa = maList.LastOrDefault();
            decimal ma = sma3 + sma2 - sma1;
            maList.Add(ma);

            var signal = GetCompareSignal(currentValue - ma, prevValue - prevMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rma", maList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = maList;
        stockData.IndicatorName = IndicatorName.RepulsionMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Retention Acceleration Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRetentionAccelerationFilter(this StockData stockData, int length = 50)
    {
        List<decimal> altmaList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList1, lowestList1) = GetMaxAndMinValuesList(highList, lowList, length);
        var (highestList2, lowestList2) = GetMaxAndMinValuesList(highList, lowList, length * 2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal highest1 = highestList1.ElementAtOrDefault(i);
            decimal lowest1 = lowestList1.ElementAtOrDefault(i);
            decimal highest2 = highestList2.ElementAtOrDefault(i);
            decimal lowest2 = lowestList2.ElementAtOrDefault(i);
            decimal ar = 2 * (highest1 - lowest1);
            decimal br = 2 * (highest2 - lowest2);
            decimal k1 = ar != 0 ? (1 - ar) / ar : 0;
            decimal k2 = br != 0 ? (1 - br) / br : 0;
            decimal alpha = k1 != 0 ? k2 / k1 : 0;
            decimal r1 = alpha != 0 && highest1 >= 0 ? Sqrt(highest1) / 4 * ((alpha - 1) / alpha) * (k2 / (k2 + 1)) : 0;
            decimal r2 = highest2 >= 0 ? Sqrt(highest2) / 4 * (alpha - 1) * (k1 / (k1 + 1)) : 0;
            decimal factor = r1 != 0 ? r2 / r1 : 0;
            decimal altk = Pow(factor >= 1 ? 1 : factor, Sqrt(length)) * ((decimal)1 / length);

            decimal prevAltma = i >= 1 ? altmaList.ElementAtOrDefault(i - 1) : currentValue;
            decimal altma = (altk * currentValue) + ((1 - altk) * prevAltma);
            altmaList.Add(altma);

            var signal = GetCompareSignal(currentValue - altma, prevValue - prevAltma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Raf", altmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = altmaList;
        stockData.IndicatorName = IndicatorName.RetentionAccelerationFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Reverse Engineering Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="rsiLevel"></param>
    /// <returns></returns>
    public static StockData CalculateReverseEngineeringRelativeStrengthIndex(this StockData stockData, int length = 14, decimal rsiLevel = 50)
    {
        List<decimal> aucList = new();
        List<decimal> adcList = new();
        List<decimal> revRsiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal expPeriod = (2 * length) - 1;
        decimal k = 2 / (expPeriod + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevAuc = i >= 1 ? aucList.ElementAtOrDefault(i - 1) : 1;
            decimal prevAdc = i >= 1 ? adcList.ElementAtOrDefault(i - 1) : 1;

            decimal auc = currentValue > prevValue ? (k * (currentValue - prevValue)) + ((1 - k) * prevAuc) : (1 - k) * prevAuc;
            aucList.Add(auc);

            decimal adc = currentValue > prevValue ? ((1 - k) * prevAdc) : (k * (prevValue - currentValue)) + ((1 - k) * prevAdc);
            adcList.Add(adc);

            decimal rsiValue = (length - 1) * ((adc * rsiLevel / (100 - rsiLevel)) - auc);
            decimal prevRevRsi = revRsiList.LastOrDefault();
            decimal revRsi = rsiValue >= 0 ? currentValue + rsiValue : currentValue + (rsiValue * (100 - rsiLevel) / rsiLevel);
            revRsiList.Add(revRsi);

            var signal = GetCompareSignal(currentValue - revRsi, prevValue - prevRevRsi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rersi", revRsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = revRsiList;
        stockData.IndicatorName = IndicatorName.ReverseEngineeringRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Right Sided Ricker Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="pctWidth"></param>
    /// <returns></returns>
    public static StockData CalculateRightSidedRickerMovingAverage(this StockData stockData, int length = 50, decimal pctWidth = 60)
    {
        List<decimal> rrmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal width = pctWidth / 100 * length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal w = 0, vw = 0;
            for (int j = 0; j < length; j++)
            {
                decimal prevV = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                w += (1 - Pow(j / width, 2)) * Exp(-(Pow(j, 2) / (2 * Pow(width, 2))));
                vw += prevV * w;
            }
            
            decimal prevRrma = rrmaList.LastOrDefault();
            decimal rrma = w != 0 ? vw / w : 0;
            rrmaList.Add(rrma);

            var signal = GetCompareSignal(currentValue - rrma, prevValue - prevRrma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rsrma", rrmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rrmaList;
        stockData.IndicatorName = IndicatorName.RightSidedRickerMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Recursive Moving Trend Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRecursiveMovingTrendAverage(this StockData stockData, int length = 14)
    {
        List<decimal> botList = new();
        List<decimal> nResList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBot = i >= 1 ? botList.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevNRes = i >= 1 ? nResList.ElementAtOrDefault(i - 1) : currentValue;

            decimal bot = ((1 - alpha) * prevBot) + currentValue;
            botList.Add(bot);

            decimal nRes = ((1 - alpha) * prevNRes) + (alpha * (currentValue + bot - prevBot));
            nResList.Add(nRes);

            var signal = GetCompareSignal(currentValue - nRes, prevValue - prevNRes);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rmta", nResList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nResList;
        stockData.IndicatorName = IndicatorName.RecursiveMovingTrendAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Reverse Moving Average Convergence Divergence
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="signalLength"></param>
    /// <param name="macdLevel"></param>
    /// <returns></returns>
    public static StockData CalculateReverseMovingAverageConvergenceDivergence(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int fastLength = 12, int slowLength = 26, int signalLength = 9,
        decimal macdLevel = 0)
    {
        List<decimal> pMacdLevelList = new();
        List<decimal> pMacdEqList = new();
        List<decimal> histogramList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal fastAlpha = (decimal)2 / (1 + fastLength);
        decimal slowAlpha = (decimal)2 / (1 + slowLength);

        var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevFastEma = i >= 1 ? fastEmaList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSlowEma = i >= 1 ? slowEmaList.ElementAtOrDefault(i - 1) : 0;

            decimal pMacdEq = fastAlpha - slowAlpha != 0 ? ((prevFastEma * fastAlpha) - (prevSlowEma * slowAlpha)) / (fastAlpha - slowAlpha) : 0;
            pMacdEqList.AddRounded(pMacdEq);

            decimal pMacdLevel = fastAlpha - slowAlpha != 0 ? (macdLevel - (prevFastEma * (1 - fastAlpha)) + (prevSlowEma * (1 - slowAlpha))) /
                (fastAlpha - slowAlpha) : 0;
            pMacdLevelList.AddRounded(pMacdLevel);
        }

        var pMacdEqSignalList = GetMovingAverageList(stockData, maType, signalLength, pMacdEqList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pMacdEq = pMacdEqList.ElementAtOrDefault(i);
            decimal pMacdEqSignal = pMacdEqSignalList.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPMacdEq = i >= 1 ? pMacdEqList.ElementAtOrDefault(i - 1) : 0;

            decimal macdHistogram = pMacdEq - pMacdEqSignal;
            histogramList.AddRounded(macdHistogram);

            Signal signal = GetCompareSignal(currentValue - pMacdEq, prevValue - prevPMacdEq);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rmacd", pMacdEqList },
            { "Signal", pMacdEqSignalList },
            { "Histogram", histogramList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pMacdEqList;
        stockData.IndicatorName = IndicatorName.ReverseMovingAverageConvergenceDivergence;

        return stockData;
    }

    /// <summary>
    /// Calculates the linear regression.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateLinearRegression(this StockData stockData, int length = 14)
    {
        List<decimal> slopeList = new();
        List<decimal> interceptList = new();
        List<decimal> predictedTomorrowList = new();
        List<decimal> predictedTodayList = new();
        List<decimal> xList = new();
        List<decimal> yList = new();
        List<decimal> xyList = new();
        List<decimal> x2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = yList.LastOrDefault();
            decimal currentValue = inputList.ElementAtOrDefault(i);
            yList.Add(currentValue);

            decimal x = i;
            xList.Add(x);

            decimal xy = x * currentValue;
            xyList.Add(xy);

            decimal sumX = xList.TakeLastExt(length).Sum();
            decimal sumY = yList.TakeLastExt(length).Sum();
            decimal sumXY = xyList.TakeLastExt(length).Sum();
            decimal sumX2 = x2List.TakeLastExt(length).Sum();
            decimal top = (length * sumXY) - (sumX * sumY);
            decimal bottom = (length * sumX2) - Pow(sumX, 2);

            decimal b = bottom != 0 ? top / bottom : 0;
            slopeList.Add(b);

            decimal a = length != 0 ? (sumY - (b * sumX)) / length : 0;
            interceptList.Add(a);

            decimal predictedToday = a + (b * x);
            predictedTodayList.Add(predictedToday);

            decimal prevPredictedNextDay = predictedTomorrowList.LastOrDefault();
            decimal predictedNextDay = a + (b * (x + 1));
            predictedTomorrowList.Add(predictedNextDay);

            var signal = GetCompareSignal(currentValue - predictedNextDay, prevValue - prevPredictedNextDay, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "LinearRegression", predictedTodayList },
            { "PredictedTomorrow", predictedTomorrowList },
            { "Slope", slopeList },
            { "Intercept", interceptList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = predictedTodayList;
        stockData.IndicatorName = IndicatorName.LinearRegression;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Adaptive Q
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="fastAlpha"></param>
    /// <param name="slowAlpha"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageAdaptiveQ(this StockData stockData, int length = 10, decimal fastAlpha = 0.667m, 
        decimal slowAlpha = 0.0645m)
    {
        List<decimal> maaqList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMaaq = i >= 1 ? maaqList.ElementAtOrDefault(i - 1) : currentValue;
            decimal er = erList.ElementAtOrDefault(i);
            decimal temp = (er * fastAlpha) + slowAlpha;

            decimal maaq = prevMaaq + (Pow(temp, 2) * (currentValue - prevMaaq));
            maaqList.Add(maaq);

            var signal = GetCompareSignal(currentValue - maaq, prevValue - prevMaaq);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Maaq", maaqList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = maaqList;
        stockData.IndicatorName = IndicatorName.MovingAverageAdaptiveQ;

        return stockData;
    }

    /// <summary>
    /// Calculates the McGinley Dynamic Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static StockData CalculateMcGinleyDynamicIndicator(this StockData stockData, int length = 14, decimal k = 0.6m)
    {
        List<decimal> mdiList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevMdi = i >= 1 ? mdiList.LastOrDefault() : currentValue;
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal ratio = prevMdi != 0 ? currentValue / prevMdi : 0;
            decimal bottom = k * length * Pow(ratio, 4);

            decimal mdi = bottom != 0 ? prevMdi + ((currentValue - prevMdi) / Math.Max(bottom, 1)) : currentValue;
            mdiList.Add(mdi);

            var signal = GetCompareSignal(currentValue - mdi, prevValue - prevMdi);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mdi", mdiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mdiList;
        stockData.IndicatorName = IndicatorName.McGinleyDynamicIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Median Average Adaptive Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersMedianAverageAdaptiveFilter(this StockData stockData, int length = 39, decimal threshold = 0.002m)
    {
        List<decimal> filterList = new();
        List<decimal> value2List = new();
        List<decimal> smthList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentPrice = inputList.ElementAtOrDefault(i);
            decimal prevP1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevP2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevP3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;

            decimal smth = (currentPrice + (2 * prevP1) + (2 * prevP2) + prevP3) / 6;
            smthList.Add(smth);

            int len = length;
            decimal value3 = 0.2m, value2 = 0, prevV2 = value2List.LastOrDefault(), alpha;
            while (value3 > threshold && len > 0)
            {
                alpha = (decimal)2 / (len + 1);
                decimal value1 = smthList.TakeLastExt(len).Median();
                value2 = (alpha * smth) + ((1 - alpha) * prevV2);
                value3 = value1 != 0 ? Math.Abs(value1 - value2) / value1 : value3;
                len -= 2;
            }
            value2List.Add(value2);

            len = len < 3 ? 3 : len;
            alpha = (decimal)2 / (len + 1);

            decimal prevFilter = filterList.LastOrDefault();
            decimal filter = (alpha * smth) + ((1 - alpha) * prevFilter);
            filterList.Add(filter);

            var signal = GetCompareSignal(currentPrice - filter, prevP1 - prevFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Maaf", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersMedianAverageAdaptiveFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Middle High Low Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateMiddleHighLowMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 14, int length2 = 10)
    {
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var mhlList = CalculateMidpoint(stockData, length2).CustomValuesList;
        var mhlMaList = GetMovingAverageList(stockData, maType, length1, mhlList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal currentMhlMa = mhlMaList.ElementAtOrDefault(i);
            decimal prevMhlma = i >= 1 ? mhlMaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(currentValue - currentMhlMa, prevValue - prevMhlma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mhlma", mhlMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mhlMaList;
        stockData.IndicatorName = IndicatorName.MiddleHighLowMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average V3
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageV3(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 14, int length2 = 3)
    {
        List<decimal> nmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal lamdaRatio = (decimal)length1 / length2;
        decimal alpha = length1 - lamdaRatio != 0 ? lamdaRatio * (length1 - 1) / (length1 - lamdaRatio) : 0;

        var ma1List = GetMovingAverageList(stockData, maType, length1, inputList);
        var ma2List = GetMovingAverageList(stockData, maType, length2, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ma1 = ma1List.ElementAtOrDefault(i);
            decimal ma2 = ma2List.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevNma = nmaList.LastOrDefault();
            decimal nma = ((1 + alpha) * ma1) - (alpha * ma2);
            nmaList.Add(nma);

            var signal = GetCompareSignal(currentValue - nma, prevValue - prevNma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mav3", nmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = nmaList;
        stockData.IndicatorName = IndicatorName.MovingAverageV3;

        return stockData;
    }

    /// <summary>
    /// Calculates the Multi Depth Zero Lag Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMultiDepthZeroLagExponentialMovingAverage(this StockData stockData, int length = 50)
    {
        List<decimal> alpha1List = new();
        List<decimal> beta1List = new();
        List<decimal> alpha2List = new();
        List<decimal> beta2List = new();
        List<decimal> alpha3List = new();
        List<decimal> beta3List = new();
        List<decimal> mda1List = new();
        List<decimal> mda2List = new();
        List<decimal> mda3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = (decimal)2 / (length + 1);
        decimal a2 = Exp(-Sqrt(2) * Pi / length);
        decimal a3 = Exp(-Pi / length);
        decimal b2 = 2 * a2 * Cos(Sqrt(2) * Pi / length);
        decimal b3 = 2 * a3 * Cos(Sqrt(3) * Pi / length);
        decimal c = Exp(-2 * Pi / length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevAlpha1 = i >= 1 ? alpha1List.ElementAtOrDefault(i - 1) : currentValue;
            decimal alpha1 = (a1 * currentValue) + ((1 - a1) * prevAlpha1);
            alpha1List.Add(alpha1);

            decimal prevAlpha2 = i >= 1 ? alpha2List.ElementAtOrDefault(i - 1) : currentValue;
            decimal priorAlpha2 = i >= 2 ? alpha2List.ElementAtOrDefault(i - 2) : currentValue;
            decimal alpha2 = (b2 * prevAlpha2) - (a2 * a2 * priorAlpha2) + ((1 - b2 + (a2 * a2)) * currentValue);
            alpha2List.Add(alpha2);

            decimal prevAlpha3 = i >= 1 ? alpha3List.ElementAtOrDefault(i - 1) : currentValue;
            decimal prevAlpha3_2 = i >= 2 ? alpha3List.ElementAtOrDefault(i - 2) : currentValue;
            decimal prevAlpha3_3 = i >= 3 ? alpha3List.ElementAtOrDefault(i - 3) : currentValue;
            decimal alpha3 = ((b3 + c) * prevAlpha3) - ((c + (b3 * c)) * prevAlpha3_2) + (c * c * prevAlpha3_3) + ((1 - b3 + c) * (1 - c) * currentValue);
            alpha3List.Add(alpha3);

            decimal detrend1 = currentValue - alpha1;
            decimal detrend2 = currentValue - alpha2;
            decimal detrend3 = currentValue - alpha3;

            decimal prevBeta1 = i >= 1 ? beta1List.ElementAtOrDefault(i - 1) : 0;
            decimal beta1 = (a1 * detrend1) + ((1 - a1) * prevBeta1);
            beta1List.Add(beta1);

            decimal prevBeta2 = i >= 1 ? beta2List.ElementAtOrDefault(i - 1) : 0;
            decimal prevBeta2_2 = i >= 2 ? beta2List.ElementAtOrDefault(i - 2) : 0;
            decimal beta2 = (b2 * prevBeta2) - (a2 * a2 * prevBeta2_2) + ((1 - b2 + (a2 * a2)) * detrend2);
            beta2List.Add(beta2);

            decimal prevBeta3_2 = i >= 2 ? beta3List.ElementAtOrDefault(i - 2) : 0;
            decimal prevBeta3_3 = i >= 3 ? beta3List.ElementAtOrDefault(i - 3) : 0;
            decimal beta3 = ((b3 + c) * prevBeta3_2) - ((c + (b3 * c)) * prevBeta3_2) + (c * c * prevBeta3_3) + ((1 - b3 + c) * (1 - c) * detrend3);
            beta3List.Add(beta3);

            decimal mda1 = alpha1 + ((decimal)1 / 1 * beta1);
            mda1List.Add(mda1);

            decimal prevMda2 = mda2List.LastOrDefault();
            decimal mda2 = alpha2 + ((decimal)1 / 2 * beta2);
            mda2List.Add(mda2);

            decimal mda3 = alpha3 + ((decimal)1 / 3 * beta3);
            mda3List.Add(mda3);

            var signal = GetCompareSignal(currentValue - mda2, prevValue - prevMda2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Md2Pole", mda2List },
            { "Md1Pole", mda1List },
            { "Md3Pole", mda3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mda2List;
        stockData.IndicatorName = IndicatorName.MultiDepthZeroLagExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Modular Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="beta"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static StockData CalculateModularFilter(this StockData stockData, int length = 200, decimal beta = 0.8m, decimal z = 0.5m)
    {
        List<decimal> b2List = new();
        List<decimal> c2List = new();
        List<decimal> os2List = new();
        List<decimal> ts2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevB2 = i >= 1 ? b2List.ElementAtOrDefault(i - 1) : currentValue;
            decimal b2 = currentValue > (alpha * currentValue) + ((1 - alpha) * prevB2) ? currentValue : (alpha * currentValue) + ((1 - alpha) * prevB2);
            b2List.Add(b2);

            decimal prevC2 = i >= 1 ? c2List.ElementAtOrDefault(i - 1) : currentValue;
            decimal c2 = currentValue < (alpha * currentValue) + ((1 - alpha) * prevC2) ? currentValue : (alpha * currentValue) + ((1 - alpha) * prevC2);
            c2List.Add(c2);

            decimal prevOs2 = os2List.LastOrDefault();
            decimal os2 = currentValue == b2 ? 1 : currentValue == c2 ? 0 : prevOs2;
            os2List.Add(os2);

            decimal upper2 = (beta * b2) + ((1 - beta) * c2);
            decimal lower2 = (beta * c2) + ((1 - beta) * b2);

            decimal prevTs2 = ts2List.LastOrDefault();
            decimal ts2 = (os2 * upper2) + ((1 - os2) * lower2);
            ts2List.Add(ts2);

            var signal = GetCompareSignal(currentValue - ts2, prevValue - prevTs2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Mf", ts2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ts2List;
        stockData.IndicatorName = IndicatorName.ModularFilter;

        return stockData;
    }
}
