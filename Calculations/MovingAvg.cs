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
    /// Calculates the simple moving average.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateSimpleMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> smaList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentValue = inputList[i];
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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal weight = length - j;
                decimal prevValue = i >= j ? inputList[i - j] : 0;

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal tma = tmaList[i];
            decimal prevTma = i >= 1 ? tmaList[i - 1] : 0;

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
            decimal currentWMA1 = wma1List[i];
            decimal currentWMA2 = wma2List[i];

            decimal totalWeightedMA = (2 * currentWMA2) - currentWMA1;
            totalWeightedMAList.AddRounded(totalWeightedMA);
        }

        var hullMAList = GetMovingAverageList(stockData, maType, sqrtLength, totalWeightedMAList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal hullMa = hullMAList[i];
            decimal prevHullMa = i >= 1 ? inputList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorValue = i >= length ? inputList[i - length] : 0;

            decimal volatility = Math.Abs(currentValue - prevValue);
            volatilityList.AddRounded(volatility);

            decimal volatilitySum = volatilityList.TakeLastExt(length).Sum();
            decimal momentum = Math.Abs(currentValue - priorValue);

            decimal efficiencyRatio = volatilitySum != 0 ? momentum / volatilitySum : 0;
            erList.AddRounded(efficiencyRatio);

            decimal sc = Pow((efficiencyRatio * (fastAlpha - slowAlpha)) + slowAlpha, 2);
            decimal prevKama = kamaList.LastOrDefault();
            decimal currentKAMA = (sc * currentValue) + ((1 - sc) * prevKama);
            kamaList.AddRounded(currentKAMA);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = s != 0 ? Exp(-1 * Pow(j - m, 2) / (2 * Pow(s, 2))) : 0;
                decimal prevValue = i >= length - 1 - j ? inputList[i - (length - 1 - j)] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevAlma = almaList.LastOrDefault();
            decimal alma = weightedSum != 0 ? sum / weightedSum : 0;
            almaList.AddRounded(alma);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = length - j - length;
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevEpma = epmaList.LastOrDefault();
            decimal epma = weightedSum != 0 ? 1 / weightedSum * sum : 0;
            epmaList.AddRounded(epma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentWma = wmaList[i];
            decimal currentSma = smaList[i];

            decimal prevLsma = lsmaList.LastOrDefault();
            decimal lsma = (3 * currentWma) - (2 * currentSma);
            lsmaList.AddRounded(lsma);

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
            decimal currentValue = inputList[i];
            decimal prevPrice1 = i >= 1 ? inputList[i - 1] : 0;
            decimal previ2 = i >= 1 ? i2List[i - 1] : 0;
            decimal prevq2 = i >= 1 ? q2List[i - 1] : 0;
            decimal prevRe = i >= 1 ? reList[i - 1] : 0;
            decimal prevIm = i >= 1 ? imList[i - 1] : 0;
            decimal prevSprd = i >= 1 ? sPrdList[i - 1] : 0;
            decimal prevPhase = i >= 1 ? phaseList[i - 1] : 0;
            decimal prevPeriod = i >= 1 ? periodList[i - 1] : 0;
            decimal prevPrice2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevPrice3 = i >= 3 ? inputList[i - 3] : 0;
            decimal prevs2 = i >= 2 ? smoothList[i - 2] : 0;
            decimal prevd2 = i >= 2 ? detList[i - 2] : 0;
            decimal prevq1x2 = i >= 2 ? q1List[i - 2] : 0;
            decimal previ1x2 = i >= 2 ? i1List[i - 2] : 0;
            decimal prevd3 = i >= 3 ? detList[i - 3] : 0;
            decimal prevs4 = i >= 4 ? smoothList[i - 4] : 0;
            decimal prevd4 = i >= 4 ? detList[i - 4] : 0;
            decimal prevq1x4 = i >= 4 ? q1List[i - 4] : 0;
            decimal previ1x4 = i >= 4 ? i1List[i - 4] : 0;
            decimal prevs6 = i >= 6 ? smoothList[i - 6] : 0;
            decimal prevd6 = i >= 6 ? detList[i - 6] : 0;
            decimal prevq1x6 = i >= 6 ? q1List[i - 6] : 0;
            decimal previ1x6 = i >= 6 ? i1List[i - 6] : 0;
            decimal prevMama = i >= 1 ? mamaList[i - 1] : 0;
            decimal prevFama = i >= 1 ? famaList[i - 1] : 0;

            decimal smooth = ((4 * currentValue) + (3 * prevPrice1) + (2 * prevPrice2) + prevPrice3) / 10;
            smoothList.AddRounded(smooth);

            decimal det = ((0.0962m * smooth) + (0.5769m * prevs2) - (0.5769m * prevs4) - (0.0962m * prevs6)) * ((0.075m * prevPeriod) + 0.54m);
            detList.AddRounded(det);

            decimal q1 = ((0.0962m * det) + (0.5769m * prevd2) - (0.5769m * prevd4) - (0.0962m * prevd6)) * ((0.075m * prevPeriod) + 0.54m);
            q1List.AddRounded(q1);

            decimal i1 = prevd3;
            i1List.AddRounded(i1);

            decimal j1 = ((0.0962m * i1) + (0.5769m * previ1x2) - (0.5769m * previ1x4) - (0.0962m * previ1x6)) * ((0.075m * prevPeriod) + 0.54m);
            decimal jq = ((0.0962m * q1) + (0.5769m * prevq1x2) - (0.5769m * prevq1x4) - (0.0962m * prevq1x6)) * ((0.075m * prevPeriod) + 0.54m);

            decimal i2 = i1 - jq;
            i2 = (0.2m * i2) + (0.8m * previ2);
            i2List.AddRounded(i2);

            decimal q2 = q1 + j1;
            q2 = (0.2m * q2) + (0.8m * prevq2);
            q2List.AddRounded(q2);

            decimal re = (i2 * previ2) + (q2 * prevq2);
            re = (0.2m * re) + (0.8m * prevRe);
            reList.AddRounded(re);

            decimal im = (i2 * prevq2) - (q2 * previ2);
            im = (0.2m * im) + (0.8m * prevIm);
            imList.AddRounded(im);

            var atan = re != 0 ? Atan(im / re) : 0;
            decimal period = atan != 0 ? 2 * Pi / atan : 0;
            period = MinOrMax(period, 1.5m * prevPeriod, 0.67m * prevPeriod);
            period = MinOrMax(period, 50, 6);
            period = (0.2m * period) + (0.8m * prevPeriod);
            periodList.AddRounded(period);

            decimal sPrd = (0.33m * period) + (0.67m * prevSprd);
            sPrdList.AddRounded(sPrd);

            decimal phase = i1 != 0 ? Atan(q1 / i1).ToDegrees() : 0;
            phaseList.AddRounded(phase);

            decimal deltaPhase = prevPhase - phase < 1 ? 1 : prevPhase - phase;
            decimal alpha = deltaPhase != 0 ? fastAlpha / deltaPhase : 0;
            alpha = alpha < slowAlpha ? slowAlpha : alpha;

            decimal mama = (alpha * currentValue) + ((1 - alpha) * prevMama);
            mamaList.AddRounded(mama);

            decimal fama = (0.5m * alpha * mama) + ((1 - (0.5m * alpha)) * prevFama);
            famaList.AddRounded(fama);

            var signal = GetCompareSignal(mama - fama, prevMama - prevFama);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Fama", famaList },
            { "Mama", mamaList },
            { "I1", i1List },
            { "Q1", q1List },
            { "SmoothPeriod", sPrdList },
            { "Smooth", smoothList },
            { "Real", reList },
            { "Imag", imList }
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevWwma = wwmaList.LastOrDefault();
            decimal wwma = (currentValue * k) + (prevWwma * (1 - k));
            wwmaList.AddRounded(wwma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ema6 = ema6List[i];
            decimal ema5 = ema5List[i];
            decimal ema4 = ema4List[i];
            decimal ema3 = ema3List[i];

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentEma1 = ema1List[i];
            decimal currentEma2 = ema2List[i];
            decimal currentEma3 = ema3List[i];

            decimal prevTema = temaList.LastOrDefault();
            decimal tema = (3 * currentEma1) - (3 * currentEma2) + currentEma3;
            temaList.AddRounded(tema);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal currentVolume = volumeList[i];
            tempVolList.AddRounded(currentVolume);

            decimal volumePrice = currentValue * currentVolume;
            tempVolPriceList.AddRounded(volumePrice);

            decimal volPriceSum = tempVolPriceList.Sum();
            decimal volSum = tempVolList.Sum();

            decimal prevVwap = vwapList.LastOrDefault();
            decimal vwap = volSum != 0 ? volPriceSum / volSum : 0;
            vwapList.AddRounded(vwap);

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
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal currentVolumeSma = volumeSmaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal volumePrice = currentValue * currentVolume;
            volumePriceList.AddRounded(volumePrice);

            decimal volumePriceSma = volumePriceList.TakeLastExt(length).Average();

            decimal prevVwma = vwmaList.LastOrDefault();
            decimal vwma = currentVolumeSma != 0 ? volumePriceSma / currentVolumeSma : 0;
            vwmaList.AddRounded(vwma);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;
            decimal currentVolume = stockData.Volumes[i];
            decimal typicalPrice = tpList[i];
            decimal prevTypicalPrice = i >= 1 ? tpList[i - 1] : 0;
            decimal length = MinOrMax(lenList[i], maxLength, minLength);
            decimal rawMoneyFlow = typicalPrice * currentVolume;

            decimal posMoneyFlow = typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
            posMoneyFlowList.AddRounded(posMoneyFlow);

            decimal negMoneyFlow = typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
            negMoneyFlowList.AddRounded(negMoneyFlow);

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
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevUma = umaList.LastOrDefault();
            decimal uma = weightedSum != 0 ? sum / weightedSum : 0;
            umaList.AddRounded(uma);

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, maxLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma = smaList[i];
            decimal stdDev = stdDevList[i];
            decimal a = sma - (1.75m * stdDev);
            decimal b = sma - (0.25m * stdDev);
            decimal c = sma + (0.25m * stdDev);
            decimal d = sma + (1.75m * stdDev);

            decimal prevLength = i >= 1 ? lengthList[i - 1] : maxLength;
            decimal length = MinOrMax(currentValue >= b && currentValue <= c ? prevLength + 1 : currentValue < a ||
                currentValue > d ? prevLength - 1 : prevLength, maxLength, minLength);
            lengthList.AddRounded(length);

            decimal sc = 2 / (length + 1);
            decimal prevVlma = i >= 1 ? vlmaList[i - 1] : currentValue;
            decimal vlma = (currentValue * sc) + ((1 - sc) * prevVlma);
            vlmaList.AddRounded(vlma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorAhma = i >= length ? ahmaList[i - length] : currentValue;

            decimal prevAhma = ahmaList.LastOrDefault();
            decimal ahma = prevAhma + ((currentValue - ((prevAhma + priorAhma) / 2)) / length);
            ahmaList.AddRounded(ahma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal hh = highestList[i];
            decimal ll = lowestList[i];
            decimal mltp = hh - ll != 0 ? MinOrMax(Math.Abs((2 * currentValue) - ll - hh) / (hh - ll), 1, 0) : 0;
            decimal ssc = (mltp * (fastAlpha - slowAlpha)) + slowAlpha;

            decimal prevAma = amaList.LastOrDefault();
            decimal ama = prevAma + (Pow(ssc, 2) * (currentValue - prevAma));
            amaList.AddRounded(ama);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal hh = highestList[i];
            decimal ll = lowestList[i];
            decimal sma = smaList[i];
            decimal mltp2 = hh - ll != 0 ? MinOrMax(Math.Abs((2 * currentValue) - ll - hh) / (hh - ll), 1, 0) : 0;
            decimal rate = mltp1 * (1 + mltp2);

            decimal prevAema = i >= 1 ? aemaList.LastOrDefault() : currentValue;
            decimal aema = i <= length ? sma : prevAema + (rate * (currentValue - prevAema));
            aemaList.AddRounded(aema);

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
            decimal currentValue = inputList[i];
            decimal er = erList[i];
            decimal prevMa2 = i >= 1 ? ma2List[i - 1] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal absDiff = Math.Abs(currentValue - prevMa2);
            absDiffList.AddRounded(absDiff);

            decimal d = i != 0 ? absDiffList.Sum() / i * gamma : 0;
            dList.AddRounded(d);

            decimal c = currentValue > prevMa2 + d ? currentValue + d : currentValue < prevMa2 - d ? currentValue - d : prevMa2;
            decimal prevMa1 = i >= 1 ? ma1List[i - 1] : currentValue;
            decimal ma1 = (er * c) + ((1 - er) * prevMa1);
            ma1List.AddRounded(ma1);

            decimal ma2 = (er * ma1) + ((1 - er) * prevMa2);
            ma2List.AddRounded(ma2);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorValue = i >= length ? inputList[i - momLength] : 0;
            decimal prevMad = i >= 1 ? madList[i - 1] : currentValue;

            decimal absDiff = Math.Abs(priorValue - prevMad);
            absDiffList.AddRounded(absDiff);

            decimal d = i != 0 ? absDiffList.Sum() / i * gamma : 0;
            decimal c = currentValue > prevMad + d ? currentValue + d : currentValue < prevMad - d ? currentValue - d : prevMad;
            cList.AddRounded(c);

            decimal ma1 = cList.TakeLastExt(length).Average();
            ma1List.AddRounded(ma1);

            decimal mad = ma1List.TakeLastExt(length).Average();
            madList.AddRounded(mad);

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
            decimal currentValue = inputList[i];
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

            decimal trVal = currentValue != 0 ? tr / currentValue : tr;
            trValList.AddRounded(trVal);
        }

        var atrValList = GetMovingAverageList(stockData, maType, atrLength, trValList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal atrVal = atrValList[i];

            decimal atrValPow = Pow(atrVal, 2);
            atrValPowList.AddRounded(atrValPow);
        }

        var stdDevAList = GetMovingAverageList(stockData, maType, stdDevLength, atrValPowList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDevA = stdDevAList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal atrVal = atrValList[i];
            tempList.AddRounded(atrVal);

            decimal atrValSum = tempList.TakeLastExt(stdDevLength).Sum();
            decimal stdDevB = Pow(atrValSum, 2) / Pow(stdDevLength, 2);

            decimal stdDev = stdDevA - stdDevB >= 0 ? Sqrt(stdDevA - stdDevB) : 0;
            stdDevList.AddRounded(stdDev);

            decimal stdDevLow = stdDevList.TakeLastExt(lbLength).Min();
            decimal stdDevFactorAFP = stdDev != 0 ? stdDevLow / stdDev : 0;
            decimal stdDevFactorCTP = stdDevLow != 0 ? stdDev / stdDevLow : 0;
            decimal stdDevFactorAFPLow = Math.Min(stdDevFactorAFP, min);
            decimal stdDevFactorCTPLow = Math.Min(stdDevFactorCTP, min);
            decimal alphaAfp = (2 * stdDevFactorAFPLow) / (length + 1);
            decimal alphaCtp = (2 * stdDevFactorCTPLow) / (length + 1);

            decimal prevEmaAfp = emaAFPList.LastOrDefault();
            decimal emaAfp = (alphaAfp * currentValue) + ((1 - alphaAfp) * prevEmaAfp);
            emaAFPList.AddRounded(emaAfp);

            decimal prevEmaCtp = emaCTPList.LastOrDefault();
            decimal emaCtp = (alphaCtp * currentValue) + ((1 - alphaCtp) * prevEmaCtp);
            emaCTPList.AddRounded(emaCtp);

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
            decimal currentValue = inputList[i];
            decimal index = i;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];

            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
            tempList.AddRounded(tr);

            decimal highest = tempList.TakeLastExt(length).Max();
            decimal alpha = highest != 0 ? MinOrMax(Pow(tr / highest, smooth), 0.99m, 0.01m) : 0.01m;
            decimal xx = index * index;
            decimal yy = currentValue * currentValue;
            decimal xy = index * currentValue;

            decimal prevX = i >= 1 ? xList[i - 1] : index;
            decimal x = (alpha * index) + ((1 - alpha) * prevX);
            xList.AddRounded(x);

            decimal prevY = i >= 1 ? yList[i - 1] : currentValue;
            decimal y = (alpha * currentValue) + ((1 - alpha) * prevY);
            yList.AddRounded(y);

            decimal dx = Math.Abs(index - x);
            decimal dy = Math.Abs(currentValue - y);

            decimal prevMx = i >= 1 ? mxList[i - 1] : dx;
            decimal mx = (alpha * dx) + ((1 - alpha) * prevMx);
            mxList.AddRounded(mx);

            decimal prevMy = i >= 1 ? myList[i - 1] : dy;
            decimal my = (alpha * dy) + ((1 - alpha) * prevMy);
            myList.AddRounded(my);

            decimal prevMxx = i >= 1 ? mxxList[i - 1] : xx;
            decimal mxx = (alpha * xx) + ((1 - alpha) * prevMxx);
            mxxList.AddRounded(mxx);

            decimal prevMyy = i >= 1 ? myyList[i - 1] : yy;
            decimal myy = (alpha * yy) + ((1 - alpha) * prevMyy);
            myyList.AddRounded(myy);

            decimal prevMxy = i >= 1 ? mxyList[i - 1] : xy;
            decimal mxy = (alpha * xy) + ((1 - alpha) * prevMxy);
            mxyList.AddRounded(mxy);

            decimal alphaVal = (2 / alpha) + 1;
            decimal a1 = alpha != 0 ? (Pow(alphaVal, 2) * mxy) - (alphaVal * mx * alphaVal * my) : 0;
            decimal tempVal = ((Pow(alphaVal, 2) * mxx) - Pow(alphaVal * mx, 2)) * ((Pow(alphaVal, 2) * myy) - Pow(alphaVal * my, 2));
            decimal b1 = tempVal >= 0 ? Sqrt(tempVal) : 0;
            decimal r = b1 != 0 ? a1 / b1 : 0;
            decimal a = mx != 0 ? r * (my / mx) : 0;
            decimal b = y - (a * x);

            decimal prevReg = regList.LastOrDefault();
            decimal reg = (x * a) + b;
            regList.AddRounded(reg);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal alpha = (decimal)2 / (i + 1);

            decimal prevEma = emaList.LastOrDefault();
            decimal ema = (alpha * currentValue) + ((1 - alpha) * prevEma);
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal er = erList[i];
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
        var devList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dev = devList[i];

            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal prevX = i >= 1 ? xList[i - 1] : currentValue;
            decimal x = currentValue > prevX + dev ? currentValue : currentValue < prevX - dev ? currentValue : prevX;
            xList.AddRounded(x);

            var corr = GoodnessOfFit.R(tempList.TakeLastExt(length).Select(x => (double)x), xList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((decimal)corr);
        }

        var xMaList = GetMovingAverageList(stockData, maType, length, xList);
        stockData.CustomValuesList = xList;
        var mxList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal my = devList[i];
            decimal mx = mxList[i];
            decimal corr = corrList[i];
            decimal yMa = yMaList[i];
            decimal xMa = xMaList[i];
            decimal x = xList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal slope = mx != 0 ? corr * (my / mx) : 0;
            decimal inter = yMa - (slope * xMa);

            decimal prevReg = regList.LastOrDefault();
            decimal reg = (x * slope) + inter;
            regList.AddRounded(reg);

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

        var devList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dev = devList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevX = i >= 1 ? xList[i - 1] : currentValue;
            decimal x = currentValue > prevX + dev ? currentValue : currentValue < prevX - dev ? currentValue : prevX;
            xList.AddRounded(x);

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

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal dev = stdDevList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal r = Math.Round(currentValue);

            decimal prevA = i >= 1 ? aList[i - 1] : r;
            decimal priorA = i >= length + 1 ? aList[i - (length + 1)] : r;
            decimal a = currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue :
                prevA + ((decimal)1 / (length * 2) * (prevA - priorA));
            aList.AddRounded(a);

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal index = i;
            indexList.AddRounded(index);

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((decimal)corr);
        }

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList[i];
            decimal corr = corrList[i];
            decimal stdDev = stdDevList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevY = yList.LastOrDefault();
            decimal y = sma + (corr * stdDev * 1.7m);
            yList.AddRounded(y);

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
            decimal wma1 = wma1List[i];
            decimal wma2 = wma2List[i];
            decimal wma3 = wma3List[i];

            decimal mid = (wma1 * 3) - wma2 - wma3;
            midList.AddRounded(mid);
        }

        var aList = GetMovingAverageList(stockData, maType, p, midList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = aList[i];
            decimal prevA = i >= 1 ? aList[i - 1] : 0;
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal priorValue = i >= length ? inputList[i - length] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevJma = jmaList.LastOrDefault();
            decimal jma = (currentValue + priorValue) / 2;
            jmaList.AddRounded(jma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorB = i >= lbLength ? bList[i - lbLength] : currentValue;
            decimal priorA = i >= length ? aList[i - length] : 0;

            decimal prevA = aList.LastOrDefault();
            decimal a = (lag * currentValue) + ((1 - lag) * priorB) + prevA;
            aList.AddRounded(a);

            decimal aDiff = a - priorA;
            decimal prevB = bList.LastOrDefault();
            decimal b = aDiff / length;
            bList.AddRounded(b);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ema1 = ema1List[i];
            decimal ema2 = ema2List[i];
            decimal d = ema1 - ema2;

            decimal prevZema = zemaList.LastOrDefault();
            decimal zema = ema1 + d;
            zemaList.AddRounded(zema);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal tma1 = tma1List[i];
            decimal tma2 = tma2List[i];
            decimal diff = tma1 - tma2;

            decimal prevZltema = zlTemaList.LastOrDefault();
            decimal zltema = tma1 + diff;
            zlTemaList.AddRounded(zltema);

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
            decimal currentValue = inputList[i];
            decimal er = erList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ver = Pow(er - (((2 * er) - 1) / 2 * (1 - trend)) + 0.5m, 2);
            decimal vLength = ver != 0 ? (length - ver + 1) / ver : 0;
            vLength = Math.Min(vLength, maxLength);
            decimal vAlpha = 2 / (vLength + 1);

            decimal prevBama = bamaList.LastOrDefault();
            decimal bama = (vAlpha * currentValue) + ((1 - vAlpha) * prevBama);
            bamaList.AddRounded(bama);

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
            decimal currentValue = inputList[i];
            decimal currentVolume = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal iRatio = (decimal)i / length;
            decimal bartlett = 1 - (2 * Math.Abs(i - ((decimal)length / 2)) / length);

            decimal bartlettW = bartlett * currentVolume;
            bartlettWList.AddRounded(bartlettW);

            decimal bartlettWSum = bartlettWList.TakeLastExt(length).Sum();
            decimal bartlettVW = currentValue * bartlettW;
            bartlettVWList.AddRounded(bartlettVW);

            decimal bartlettVWSum = bartlettVWList.TakeLastExt(length).Sum();
            decimal prevBartlettWvwma = bartlettWvwmaList.LastOrDefault();
            decimal bartlettWvwma = bartlettWSum != 0 ? bartlettVWSum / bartlettWSum : 0;
            bartlettWvwmaList.AddRounded(bartlettWvwma);

            decimal blackman = 0.42m - (0.5m * Cos(2 * (decimal)Math.PI * iRatio)) + (0.08m * Cos(4 * (decimal)Math.PI * iRatio));
            decimal blackmanW = blackman * currentVolume;
            blackmanWList.AddRounded(blackmanW);

            decimal blackmanWSum = blackmanWList.TakeLastExt(length).Sum();
            decimal blackmanVW = currentValue * blackmanW;
            blackmanVWList.AddRounded(blackmanVW);

            decimal blackmanVWSum = blackmanVWList.TakeLastExt(length).Sum();
            decimal blackmanWvwma = blackmanWSum != 0 ? blackmanVWSum / blackmanWSum : 0;
            blackmanWvwmaList.AddRounded(blackmanWvwma);

            decimal hanning = 0.5m - (0.5m * Cos(2 * (decimal)Math.PI * iRatio));
            decimal hanningW = hanning * currentVolume;
            hanningWList.AddRounded(hanningW);

            decimal hanningWSum = hanningWList.TakeLastExt(length).Sum();
            decimal hanningVW = currentValue * hanningW;
            hanningVWList.AddRounded(hanningVW);

            decimal hanningVWSum = hanningVWList.TakeLastExt(length).Sum();
            decimal hanningWvwma = hanningWSum != 0 ? hanningVWSum / hanningWSum : 0;
            hanningWvwmaList.AddRounded(hanningWvwma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevSrcY = i >= 1 ? srcYList[i - 1] : 0;
            decimal prevSrcEma = i >= 1 ? srcEmaList[i - 1] : 0;

            decimal prevA = aList.LastOrDefault();
            decimal a = prevA + (alpha * prevSrcY);
            aList.AddRounded(a);

            decimal prevB = bList.LastOrDefault();
            decimal b = prevB + (alpha * prevSrcEma);
            bList.AddRounded(b);

            decimal ab = a + b;
            decimal prevY = yList.LastOrDefault();
            decimal y = CalculateEMA(ab, prevY, 1);
            yList.AddRounded(y);

            decimal srcY = currentValue - y;
            srcYList.AddRounded(srcY);

            decimal prevYEma = yEmaList.LastOrDefault();
            decimal yEma = CalculateEMA(y, prevYEma, length);
            yEmaList.AddRounded(yEma);

            decimal srcEma = currentValue - yEma;
            srcEmaList.AddRounded(srcEma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevSum = sumList.LastOrDefault();
            decimal sum = prevSum - (prevSum / length) + currentValue;
            sumList.AddRounded(sum);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal num = 0, denom = 0;
            for (int j = 1; j <= length + 1; j++)
            {
                decimal mult = j <= peak ? (decimal)j / peak : (decimal)(length + 1 - j) / (length + 1 - peak);
                decimal prevValue = i >= j - 1 ? inputList[i - (j - 1)] : 0;

                num += prevValue * mult;
                denom += mult;
            }

            decimal prevQma = qmaList.LastOrDefault();
            decimal qma = denom != 0 ? num / denom : 0;
            qmaList.AddRounded(qma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal pow = Pow(currentValue, 2);
            powList.AddRounded(pow);

            decimal prevQma = qmaList.LastOrDefault();
            decimal powSma = powList.TakeLastExt(length).Average();
            decimal qma = powSma >= 0 ? Sqrt(powSma) : 0;
            qmaList.AddRounded(qma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ema1 = ema1List[i];
            decimal ema2 = ema2List[i];
            decimal ema3 = ema3List[i];
            decimal ema4 = ema4List[i];
            decimal ema5 = ema5List[i];

            decimal prevQema = qemaList.LastOrDefault();
            decimal qema = (5 * ema1) - (10 * ema2) + (10 * ema3) - (5 * ema4) + ema5;
            qemaList.AddRounded(qema);

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
            decimal currentValue = inputList[i];

            decimal n = i;
            nList.AddRounded(n);

            decimal n2 = Pow(n, 2);
            n2List.AddRounded(n2);

            decimal nn2 = n * n2;
            nn2List.AddRounded(nn2);

            decimal n2v = n2 * currentValue;
            n2vList.AddRounded(n2v);

            decimal nv = n * currentValue;
            nvList.AddRounded(nv);
        }

        var nSmaList = GetMovingAverageList(stockData, maType, length, nList);
        var n2SmaList = GetMovingAverageList(stockData, maType, length, n2List);
        var n2vSmaList = GetMovingAverageList(stockData, maType, length, n2vList);
        var nvSmaList = GetMovingAverageList(stockData, maType, length, nvList);
        var nn2SmaList = GetMovingAverageList(stockData, maType, length, nn2List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal nSma = nSmaList[i];
            decimal n2Sma = n2SmaList[i];
            decimal n2vSma = n2vSmaList[i];
            decimal nvSma = nvSmaList[i];
            decimal nn2Sma = nn2SmaList[i];
            decimal sma = smaList[i];

            decimal nn2Cov = nn2Sma - (nSma * n2Sma);
            nn2CovList.AddRounded(nn2Cov);

            decimal n2vCov = n2vSma - (n2Sma * sma);
            n2vCovList.AddRounded(n2vCov);

            decimal nvCov = nvSma - (nSma * sma);
            nvCovList.AddRounded(nvCov);
        }

        stockData.CustomValuesList = nList;
        var nVarianceList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = n2List;
        var n2VarianceList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal n2Variance = n2VarianceList[i];
            decimal nVariance = nVarianceList[i];
            decimal nn2Cov = nn2CovList[i];
            decimal n2vCov = n2vCovList[i];
            decimal nvCov = nvCovList[i];
            decimal sma = smaList[i];
            decimal n2Sma = n2SmaList[i];
            decimal nSma = nSmaList[i];
            decimal n2 = n2List[i];
            decimal norm = (n2Variance * nVariance) - Pow(nn2Cov, 2);
            decimal a = norm != 0 ? ((n2vCov * nVariance) - (nvCov * nn2Cov)) / norm : 0;
            decimal b = norm != 0 ? ((nvCov * n2Variance) - (n2vCov * nn2Cov)) / norm : 0;
            decimal c = sma - (a * n2Sma) - (b * nSma);

            decimal prevQlsma = qlsmaList.LastOrDefault();
            decimal qlsma = (a * n2) + (b * i) + c;
            qlsmaList.AddRounded(qlsma);

            decimal fcast = (a * Pow(i + forecastLength, 2)) + (b * (i + forecastLength)) + c;
            fcastList.AddRounded(fcast);

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
            decimal y = inputList[i];
            tempList.AddRounded(y);

            decimal x1 = i;
            x1List.AddRounded(x1);

            decimal x2 = Pow(x1, 2);
            x2List.AddRounded(x2);

            decimal x1x2 = x1 * x2;
            x1x2List.AddRounded(x1x2);

            decimal yx1 = y * x1;
            yx1List.AddRounded(yx1);

            decimal yx2 = y * x2;
            yx2List.AddRounded(yx2);

            decimal x2Pow = Pow(x2, 2);
            x2PowList.AddRounded(x2Pow);

            decimal ySum = tempList.TakeLastExt(length).Sum();
            ySumList.AddRounded(ySum);

            decimal x1Sum = x1List.TakeLastExt(length).Sum();
            x1SumList.AddRounded(x1Sum);

            decimal x2Sum = x2List.TakeLastExt(length).Sum();
            x2SumList.AddRounded(x2Sum);

            decimal x1x2Sum = x1x2List.TakeLastExt(length).Sum();
            x1x2SumList.AddRounded(x1x2Sum);

            decimal yx1Sum = yx1List.TakeLastExt(length).Sum();
            yx1SumList.AddRounded(yx1Sum);

            decimal yx2Sum = yx2List.TakeLastExt(length).Sum();
            yx2SumList.AddRounded(yx2Sum);

            decimal x2PowSum = x2PowList.TakeLastExt(length).Sum();
            x2PowSumList.AddRounded(x2PowSum);
        }

        var max1List = GetMovingAverageList(stockData, maType, length, x1List);
        var max2List = GetMovingAverageList(stockData, maType, length, x2List);
        var mayList = GetMovingAverageList(stockData, maType, length, inputList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal x1Sum = x1SumList[i];
            decimal x2Sum = x2SumList[i];
            decimal x1x2Sum = x1x2SumList[i];
            decimal x2PowSum = x2PowSumList[i];
            decimal yx1Sum = yx1SumList[i];
            decimal yx2Sum = yx2SumList[i];
            decimal ySum = ySumList[i];
            decimal may = mayList[i];
            decimal max1 = max1List[i];
            decimal max2 = max2List[i];
            decimal x1 = x1List[i];
            decimal x2 = x2List[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
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
            yList.AddRounded(y);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = length - j;
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevLwma = lwmaList.LastOrDefault();
            decimal lwma = weightedSum != 0 ? sum / weightedSum : 0;
            lwmaList.AddRounded(lwma);

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
            decimal currentValue = inputList[i];
            decimal currentWma = wmaList[i];
            decimal currentSma = smaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevLma = lmaList.LastOrDefault();
            decimal lma = (2 * currentWma) - currentSma;
            lmaList.AddRounded(lma);

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
            indexList.AddRounded(index);
        }

        var sma1List = GetMovingAverageList(stockData, maType, length, inputList);
        var sma2List = GetMovingAverageList(stockData, maType, length1, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma1 = sma1List[i];
            decimal sma2 = sma2List[i];
            decimal stdDev = stdDevList[i];
            decimal indexStdDev = indexStdDevList[i];
            decimal indexSma = indexSmaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal c = stdDev != 0 ? (sma2 - sma1) / stdDev : 0;
            decimal z = indexStdDev != 0 && c != 0 ? (i - indexSma) / indexStdDev * c : 0;

            decimal prevY = yList.LastOrDefault();
            decimal y = sma1 + (z * stdDev);
            yList.AddRounded(y);

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
            decimal currentValue = inputList[i];
            decimal prevY = i >= 1 ? inputList[i - 1] : 0;
            decimal priorY = i >= length ? inputList[i - length] : 0;
            decimal priorY2 = i >= length * 2 ? inputList[i - (length * 2)] : 0;
            decimal priorX = i >= length ? xList[i - length] : 0;
            decimal priorX2 = i >= length * 2 ? xList[i - (length * 2)] : 0;

            decimal x = i;
            xList.AddRounded(i);

            decimal prevExt = extList.LastOrDefault();
            decimal ext = priorX2 - priorX != 0 && priorY2 - priorY != 0 ? priorY + ((x - priorX) / (priorX2 - priorX) * (priorY2 - priorY)) : priorY;
            extList.AddRounded(ext);

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
        var myList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            yList.AddRounded(currentValue);

            decimal x = i;
            xList.AddRounded(x);

            var corr = GoodnessOfFit.R(yList.TakeLastExt(length).Select(x => (double)x), xList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((decimal)corr);
        }

        var xMaList = GetMovingAverageList(stockData, maType, length, xList);
        stockData.CustomValuesList = xList;
        var mxList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList; ;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal my = myList[i];
            decimal mx = mxList[i];
            decimal corr = corrList[i];
            decimal yMa = yMaList[i];
            decimal xMa = xMaList[i];
            decimal x = xList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal slope = mx != 0 ? corr * (my / mx) : 0;
            decimal inter = yMa - (slope * xMa);

            decimal prevReg = regList.LastOrDefault();
            decimal reg = (x * slope) + inter;
            regList.AddRounded(reg);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevS = i >= 1 ? sList[i - 1] : currentValue;
            decimal prevSEma = sEmaList.LastOrDefault();
            decimal sEma = CalculateEMA(prevS, prevSEma, halfLength);
            sEmaList.AddRounded(prevSEma);

            decimal s = (a * currentValue) + prevS - (a * sEma);
            sList.AddRounded(s);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                decimal weight = 0;
                for (int k = 0; k <= length - 1; k++)
                {
                    decimal prevValue2 = i >= k ? inputList[i - k] : 0;
                    weight += Math.Abs(prevValue - prevValue2);
                }

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevIdwma = idwmaList.LastOrDefault();
            decimal idwma = weightedSum != 0 ? sum / weightedSum : 0;
            idwmaList.AddRounded(idwma);

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
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            var lookBackList = tempList.TakeLastExt(length);

            decimal q1 = lookBackList.PercentileNearestRank(25);
            q1List.AddRounded(q1);

            decimal median = lookBackList.PercentileNearestRank(50);
            medianList.AddRounded(median);

            decimal q3 = lookBackList.PercentileNearestRank(75);
            q3List.AddRounded(q3);

            decimal prevTrimean = trimeanList.LastOrDefault();
            decimal trimean = (q1 + (2 * median) + q3) / 4;
            trimeanList.AddRounded(trimean);

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
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal prevOwma = i >= 1 ? owmaList[i - 1] : 0;
            prevOwmaList.AddRounded(prevOwma);

            var corr = GoodnessOfFit.R(tempList.TakeLastExt(length).Select(x => (double)x), prevOwmaList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, (decimal)corr);
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal owma = weightedSum != 0 ? sum / weightedSum : 0;
            owmaList.AddRounded(owma);

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
            indexList.AddRounded(index);

            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((decimal)corr);
        }

        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal index = indexList[i];
            decimal indexSma = indexSmaList[i];
            decimal indexStdDev = indexStdDevList[i];
            decimal corr = corrList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevD = i >= 1 ? dList[i - 1] != 0 ? dList[i - 1] : prevValue : prevValue;
            decimal sma = smaList[i];
            decimal stdDev = stdDevList[i];
            decimal a = indexStdDev != 0 && corr != 0 ? (index - indexSma) / indexStdDev * corr : 0;

            decimal b = Math.Abs(prevD - currentValue);
            bList.AddRounded(b);

            decimal bSma = bList.TakeLastExt(length1).Average();
            bSmaList.AddRounded(bSma);

            decimal highest = bSmaList.TakeLastExt(length).Max();
            decimal c = highest != 0 ? b / highest : 0;

            decimal d = sma + (a * (stdDev * c));
            dList.AddRounded(d);

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
            decimal currentValue = inputList[i];
            decimal currentCmo = Math.Abs(cmoList[i] / 100);
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevVidya = vidyaList.LastOrDefault();
            decimal currentVidya = (currentValue * alpha * currentCmo) + (prevVidya * (1 - (alpha * currentCmo)));
            vidyaList.AddRounded(currentVidya);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
            lnList.AddRounded(ln);

            decimal num = 0, denom = 0;
            for (int j = 0; j < length; j++)
            {
                decimal currentLn = i >= j ? lnList[i - j] : 0;
                decimal prevLn = i >= j + 1 ? lnList[i - (j + 1)] : 0;
                decimal oi = Math.Abs(currentLn - prevLn);
                num += oi * (Sqrt(j + 1) - Sqrt(j));
                denom += oi;
            }

            decimal ratio = denom != 0 ? num / denom : 0;
            decimal prevNma = nmaList.LastOrDefault();
            decimal nma = (currentValue * ratio) + (prevValue * (1 - ratio));
            nmaList.AddRounded(nma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal nr = 0, nl = 0, sr = 0, sl = 0;
            if (floorLength == roundLength)
            {
                for (int j = 0; j <= floorLength - 1; j++)
                {
                    decimal wr = (length - (length - 1 - j)) * length;
                    decimal prevVal = i >= j ? inputList[i - j] : 0;
                    nr += wr;
                    sr += prevVal * wr;
                }

                for (int j = floorLength; j <= length - 1; j++)
                {
                    decimal wl = (length - j) * length;
                    decimal prevVal = i >= j ? inputList[i - j] : 0;
                    nl += wl;
                    sl += prevVal * wl;
                }
            }
            else
            {
                for (int j = 0; j <= floorLength; j++)
                {
                    decimal wr = (length - (length - 1 - j)) * length;
                    decimal prevVal = i >= j ? inputList[i - j] : 0;
                    nr += wr;
                    sr += prevVal * wr;
                }

                for (int j = roundLength; j <= length - 1; j++)
                {
                    decimal wl = (length - j) * length;
                    decimal prevVal = i >= j ? inputList[i - j] : 0;
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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentEma1 = ema1List[i];
            decimal currentEma2 = ema2List[i];

            decimal prevGd = gdList.LastOrDefault();
            decimal gd = (currentEma1 * (1 + factor)) - (currentEma2 * factor);
            gdList.AddRounded(gd);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorB = i >= p ? bList[i - p] : currentValue;
            decimal a = currentValue - priorB;

            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal b = prevB + (a / p * gamma);
            bList.AddRounded(b);

            decimal priorD = i >= p ? dList[i - p] : b;
            decimal c = b - priorD;

            decimal prevD = i >= 1 ? dList[i - 1] : currentValue;
            decimal d = prevD + (((zeta * a) + ((1 - zeta) * c)) / p * gamma);
            dList.AddRounded(d);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

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
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevHwma = hwmaList.LastOrDefault();
            decimal hwma = weightedSum != 0 ? sum / weightedSum : 0;
            hwmaList.AddRounded(hwma);

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
            decimal currentValue = inputList[i];
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevHema = hemaList.LastOrDefault();
            decimal hema = ((1 - alpha) * (prevHema + prevB)) + (alpha * currentValue);
            hemaList.AddRounded(hema);

            decimal b = ((1 - gamma) * prevB) + (gamma * (hema - prevHema));
            bList.AddRounded(b);

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
            decimal currentValue = inputList[i];
            decimal currentWma = wmaList[i];
            decimal currentEma = emaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevHema = hemaList.LastOrDefault();
            decimal hema = (3 * currentWma) - (2 * currentEma);
            hemaList.AddRounded(hema);

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
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal sampleMedian = tempList.TakeLastExt(length).Median();
            decimal absDiff = Math.Abs(currentValue - sampleMedian);
            absDiffList.AddRounded(absDiff);

            decimal mad = absDiffList.TakeLastExt(length).Median();
            decimal hf = absDiff <= scalingFactor * mad ? currentValue : sampleMedian;
            hfList.AddRounded(hf);

            decimal prevHfEma = hfEmaList.LastOrDefault();
            decimal hfEma = (alpha * hf) + ((1 - alpha) * prevHfEma);
            hfEmaList.AddRounded(hfEma);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal prevOutput = i >= 1 ? outputList[i - 1] : currentValue;
            decimal output = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal sign = (0.5m * (1 - Cos(MinOrMax((decimal)j / length * Pi, 0.99m, 0.01m))));
                decimal d = sign - (0.5m * (1 - Cos(MinOrMax((decimal)(j - 1) / length, 0.99m, 0.01m))));
                decimal prevValue = i >= j - 1 ? inputList[i - (j - 1)] : 0;
                output += ((sign * prevOutput) + ((1 - sign) * prevValue)) * d;
            }
            outputList.AddRounded(output);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal pow = Pow(phi, length - j);
                decimal weight = (pow - (Pow(-1, j) / pow)) / Sqrt(5);
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevFwma = fibonacciWmaList.LastOrDefault();
            decimal fwma = weightedSum != 0 ? sum / weightedSum : 0;
            fibonacciWmaList.AddRounded(fwma);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j < resList.Count; j++)
            {
                decimal prevValue = i >= j ? inputList[i - j] : 0;
                decimal weight = resList[j];

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevFswma = fswmaList.LastOrDefault();
            decimal fswma = weightedSum != 0 ? sum / weightedSum : 0;
            fswmaList.AddRounded(fswma);

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
            decimal currentValue = inputList[i];
            decimal prevFilter = i >= 1 ? filterList.LastOrDefault() : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal highestHigh1 = highestList1[i];
            decimal lowestLow1 = lowestList1[i];
            decimal highestHigh2 = highestList2[i];
            decimal lowestLow2 = lowestList2[i];
            decimal highestHigh3 = highestList2[Math.Max(i - halfP, i)];
            decimal lowestLow3 = lowestList2[Math.Max(i - halfP, i)];
            decimal n3 = (highestHigh1 - lowestLow1) / length;
            decimal n1 = (highestHigh2 - lowestLow2) / halfP;
            decimal n2 = (highestHigh3 - lowestLow3) / halfP;
            decimal dm = n1 > 0 && n2 > 0 && n3 > 0 ? (Log(n1 + n2) - Log(n3)) / Log(2) : 0;

            decimal alpha = MinOrMax(Exp(-4.6m * (dm - 1)), 1, 0.01m);
            decimal filter = (alpha * currentValue) + ((1 - alpha) * prevFilter);
            filterList.AddRounded(filter);

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
            decimal currentValue = inputList[i];
            decimal prevA = i >= 1 ? aList[i - 1] : 0;
            decimal prevError = i >= 1 ? errorList[i - 1] : 0;

            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            tempList.AddRounded(prevValue);

            var lbList = tempList.TakeLastExt(length).ToList();
            decimal beta = currentValue > lbList.Max() || currentValue < lbList.Min() ? 1 : alpha;
            decimal a = prevA + (alpha * prevError) + (beta * prevError);
            aList.AddRounded(a);

            decimal error = currentValue - a;
            errorList.AddRounded(error);

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

        var stdDevSrcList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var smaSrcList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal index = i;
            indexList.AddRounded(index);
        }

        stockData.CustomValuesList = indexList;
        var indexStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDevSrc = stdDevSrcList[i];
            decimal indexStdDev = indexStdDevList[i];
            decimal currentValue = inputList[i];
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal indexSma = indexSmaList[i];
            decimal sma = smaSrcList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal diff = currentValue - prevB;
            diffList.AddRounded(diff);

            decimal absDiff = Math.Abs(diff);
            absDiffList.AddRounded(absDiff);

            decimal e = absDiffList.TakeLastExt(length).Average();
            decimal z = e != 0 ? diffList.TakeLastExt(length).Average() / e : 0;
            decimal r = Exp(2 * z) + 1 != 0 ? (Exp(2 * z) - 1) / (Exp(2 * z) + 1) : 0;
            decimal a = indexStdDev != 0 && r != 0 ? (i - indexSma) / indexStdDev * r : 0;

            decimal b = sma + (a * stdDevSrc);
            bList.AddRounded(b);

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
            indexList.AddRounded(index);
        }

        var indexMaList = GetMovingAverageList(stockData, maType, length, indexList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            var indexSt = indexStList[i];
            var srcSt = srcStList[i];
            var srcMa = srcMaList[i];
            var indexMa = indexMaList[i];
            var r = rList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal alpha = indexSt != 0 ? srcSt / indexSt * r : 0;
            decimal beta = srcMa - (alpha * indexMa);

            decimal prevKalsma = kalsmaList.LastOrDefault();
            decimal kalsma = (alpha * i) + beta;
            kalsmaList.AddRounded(kalsma);

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
            decimal currentValue = inputList[i];
            decimal prevKf = i >= 1 ? kfList[i - 1] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal dk = currentValue - prevKf;
            decimal smooth = prevKf + (dk * Sqrt((decimal)length / 10000 * 2));

            decimal prevVelo = i >= 1 ? veloList[i - 1] : 0;
            decimal velo = prevVelo + ((decimal)length / 10000 * dk);
            veloList.AddRounded(velo);

            decimal kf = smooth + velo;
            kfList.AddRounded(kf);

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
            decimal currentVolume = volumeList[i];
            decimal currentValue = inputList[i];
            decimal volumeSma = volumeSmaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal volumeIncrement = volumeSma * factor;

            decimal volumeRatio = volumeIncrement != 0 ? currentVolume / volumeIncrement : 0;
            volumeRatioList.AddRounded(volumeRatio);

            decimal priceVolumeRatio = currentValue * volumeRatio;
            priceVolumeRatioList.AddRounded(priceVolumeRatio);

            decimal volumeRatioSum = volumeRatioList.TakeLastExt(length).Sum();
            volumeRatioSumList.AddRounded(volumeRatioSum);

            decimal priceVolumeRatioSum = priceVolumeRatioList.TakeLastExt(length).Sum();
            priceVolumeRatioSumList.AddRounded(priceVolumeRatioSum);

            decimal prevVama = vamaList.LastOrDefault();
            decimal vama = volumeRatioSum != 0 ? priceVolumeRatioSum / volumeRatioSum : 0;
            vamaList.AddRounded(vama);

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

        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDev = stdDevList[i];
            decimal currentValue = inputList[i];
            decimal sdPct = currentValue != 0 ? stdDev / currentValue * 100 : 0;

            decimal p = sdPct >= 0 ? MinOrMax(Sqrt(sdPct) * kf, 4, 1) : 1;
            pList.AddRounded(p);
        }

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal p = pList[i];

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, p);
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal pma = weightedSum != 0 ? sum / weightedSum : 0;
            pmaList.AddRounded(pma);
        }

        var wmap1List = GetMovingAverageList(stockData, maType, s, pmaList);
        var wmap2List = GetMovingAverageList(stockData, maType, s, wmap1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal wmap1 = wmap1List[i];
            decimal wmap2 = wmap2List[i];

            decimal prevZlmap = zlmapList.LastOrDefault();
            decimal zlmap = (2 * wmap1) - wmap2;
            zlmapList.AddRounded(zlmap);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal c = cList[i];
            decimal o = oList[i];
            decimal h = hList[i];
            decimal l = lList[i];
            decimal lv = h - l != 0 ? MinOrMax(Math.Abs(c - o) / (h - l), 0.99m, 0.01m) : 0;

            decimal prevVma = i >= 1 ? vmaList[i - 1] : currentValue;
            decimal vma = (lv * currentValue) + ((1 - lv) * prevVma);
            vmaList.AddRounded(vma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal pdm = Math.Max(currentValue - prevValue, 0);
            decimal mdm = Math.Max(prevValue - currentValue, 0);

            decimal prevPdms = pdmsList.LastOrDefault();
            decimal pdmS = ((1 - k) * prevPdms) + (k * pdm);
            pdmsList.AddRounded(pdmS);

            decimal prevMdms = mdmsList.LastOrDefault();
            decimal mdmS = ((1 - k) * prevMdms) + (k * mdm);
            mdmsList.AddRounded(mdmS);

            decimal s = pdmS + mdmS;
            decimal pdi = s != 0 ? pdmS / s : 0;
            decimal mdi = s != 0 ? mdmS / s : 0;

            decimal prevPdis = pdisList.LastOrDefault();
            decimal pdiS = ((1 - k) * prevPdis) + (k * pdi);
            pdisList.AddRounded(pdiS);

            decimal prevMdis = mdisList.LastOrDefault();
            decimal mdiS = ((1 - k) * prevMdis) + (k * mdi);
            mdisList.AddRounded(mdiS);

            decimal d = Math.Abs(pdiS - mdiS);
            decimal s1 = pdiS + mdiS;
            decimal dS1 = s1 != 0 ? d / s1 : 0;

            decimal prevIs = isList.LastOrDefault();
            decimal iS = ((1 - k) * prevIs) + (k * dS1);
            isList.AddRounded(iS);

            var lbList = isList.TakeLastExt(length).ToList();
            decimal hhv = lbList.Max();
            decimal llv = lbList.Min();
            decimal d1 = hhv - llv;
            decimal vI = d1 != 0 ? (iS - llv) / d1 : 0;

            decimal prevVma = vmaList.LastOrDefault();
            decimal vma = ((1 - k) * vI * prevVma) + (k * vI * currentValue);
            vmaList.AddRounded(vma);

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, lbLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList[i];
            decimal currentValue = inputList[i];
            decimal dev = stdDevList[i];
            decimal upper = sma + dev;
            decimal lower = sma - dev;

            decimal k = upper - lower != 0 ? (currentValue - sma) / (upper - lower) * 100 * 2 : 0;
            kList.AddRounded(k);
        }

        var kMaList = GetMovingAverageList(stockData, maType, smoothLength, kList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal kMa = kMaList[i];
            decimal kNorm = Math.Min(Math.Max(kMa, -100), 100);
            decimal kAbs = Math.Round(Math.Abs(kNorm) / lbLength);
            decimal kRescaled = RescaleValue(kAbs, 10, 0, length, 0, true);
            int vLength = (int)Math.Round(Math.Max(kRescaled, 1));

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= vLength - 1; j++)
            {
                decimal weight = vLength - j;
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal vma1 = weightedSum != 0 ? sum / weightedSum : 0;
            vma1List.AddRounded(vma1);
        }

        var vma2List = GetMovingAverageList(stockData, maType, smoothLength, vma1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal vma = vma2List[i];
            decimal prevVma = i >= 1 ? vma2List[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal priorValue = i >= length ? inputList[i - length] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];

            decimal priceChange = Math.Abs(currentValue - priorValue);
            changeList.AddRounded(priceChange);

            decimal numerator = highest - lowest;
            decimal denominator = changeList.TakeLastExt(length).Sum();
            decimal vhf = denominator != 0 ? numerator / denominator : 0;

            decimal prevVhma = vhmaList.LastOrDefault();
            decimal vhma = prevVhma + (Pow(vhf, 2) * (currentValue - prevVhma));
            vhmaList.AddRounded(vhma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ema1 = ema1List[i];
            decimal ema2 = ema2List[i];

            decimal prevMnma = mnmaList.LastOrDefault();
            decimal mnma = 1 - alpha != 0 ? (((2 - alpha) * ema1) - ema2) / (1 - alpha) : 0;
            mnmaList.AddRounded(mnma);

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
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal coraRaw = weightedSum != 0 ? sum / weightedSum : 0;
            coraRawList.AddRounded(coraRaw);
        }

        var coraWaveList = GetMovingAverageList(stockData, maType, smoothLength, coraRawList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal coraWave = coraWaveList[i];
            decimal prevCoraWave = i >= 1 ? coraWaveList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, 3);
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevCwma = cwmaList.LastOrDefault();
            decimal cwma = weightedSum != 0 ? sum / weightedSum : 0;
            cwmaList.AddRounded(cwma);

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
        var v1List = CalculateStandardDeviationVolatility(stockData, maType, length).OutputValues["Variance"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma = smaList[i];
            decimal prevCma = i >= 1 ? cmaList[i - 1] : sma;
            decimal v1 = v1List[i];
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
            cmaList.AddRounded(cma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentEma = ema1List[i];
            decimal currentEma2 = ema2List[i];

            decimal prevDema = demaList.LastOrDefault();
            decimal dema = (2 * currentEma) - currentEma2;
            demaList.AddRounded(dema);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ema1 = ema1List[i];
            decimal ema2 = ema2List[i];
            decimal ema3 = ema3List[i];
            decimal ema4 = ema4List[i];
            decimal ema5 = ema5List[i];
            decimal ema6 = ema6List[i];
            decimal ema7 = ema7List[i];
            decimal ema8 = ema8List[i];

            decimal prevPema = pemaList.LastOrDefault();
            decimal pema = (8 * ema1) - (28 * ema2) + (56 * ema3) - (70 * ema4) + (56 * ema5) - (28 * ema6) + (8 * ema7) - ema8;
            pemaList.AddRounded(pema);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal prevSumPow3 = sumPow3List.LastOrDefault();
            decimal x1Pow1Sum, x2Pow1Sum, x1Pow2Sum, x2Pow2Sum, x1Pow3Sum, x2Pow3Sum, wPow1, wPow2, wPow3, sumPow1 = 0, sumPow2 = 0, sumPow3 = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevValue = i >= j - 1 ? inputList[i - (j - 1)] : 0;
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
            sumPow3List.AddRounded(sumPow3);

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
            decimal prevValue = i >= length ? inputList[i - length] : 0;
            decimal p1 = i + 1 - (per / 100 * length);
            decimal p2 = i + 1 - ((100 - per) / 100 * length);

            decimal w1 = p1 >= 0 ? p1 : alpha * p1;
            w1List.AddRounded(w1);

            decimal w2 = p2 >= 0 ? p2 : alpha * p2;
            w2List.AddRounded(w2);

            decimal vw1 = prevValue * w1;
            vw1List.AddRounded(vw1);

            decimal vw2 = prevValue * w2;
            vw2List.AddRounded(vw2);

            decimal wSum1 = w1List.TakeLastExt(length).Sum();
            decimal wSum2 = w2List.TakeLastExt(length).Sum();
            decimal sum1 = vw1List.TakeLastExt(length).Sum();
            decimal sum2 = vw2List.TakeLastExt(length).Sum();

            decimal prevRrma1 = rrma1List.LastOrDefault();
            decimal rrma1 = wSum1 != 0 ? sum1 / wSum1 : 0;
            rrma1List.AddRounded(rrma1);

            decimal prevRrma2 = rrma2List.LastOrDefault();
            decimal rrma2 = wSum2 != 0 ? sum2 / wSum2 : 0;
            rrma2List.AddRounded(rrma2);

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
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, 2);
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevPwma = pwmaList.LastOrDefault();
            decimal pwma = weightedSum != 0 ? sum / weightedSum : 0;
            pwmaList.AddRounded(pwma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorEst = i >= length ? estList[i - length] : prevValue;
            decimal errMea = Math.Abs(priorEst - currentValue);
            decimal errPrv = Math.Abs((currentValue - prevValue) * -1);
            decimal prevErr = i >= 1 ? errList[i - 1] : errPrv;
            decimal kg = prevErr != 0 ? prevErr / (prevErr + errMea) : 0;
            decimal prevEst = i >= 1 ? estList[i - 1] : prevValue;

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal efRatio = efRatioList[i];
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal er = 1 - efRatio;

            decimal chg = Math.Abs(currentValue - prevB);
            chgList.AddRounded(chg);

            decimal a = chgList.Average() * (1 + er);
            decimal b = currentValue > prevB + a ? currentValue : currentValue < prevB - a ? currentValue : prevB;
            bList.AddRounded(b);

            var corr = GoodnessOfFit.R(bList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((decimal)corr);
        }

        stockData.CustomValuesList = bList;
        var bStdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var bSmaList = GetMovingAverageList(stockData, maType, length, bList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal corr = corrList[i];
            decimal stdDev = stdDevList[i];
            decimal bStdDev = bStdDevList[i];
            decimal bSma = bSmaList[i];
            decimal sma = smaList[i];
            decimal currentValue = inputList[i];
            decimal prevLs = i >= 1 ? lsList[i - 1] : currentValue;
            decimal b = bList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal tslsma = (sc * currentValue) + ((1 - sc) * prevLs);
            decimal alpha = bStdDev != 0 ? corr * stdDev / bStdDev : 0;
            decimal beta = sma - (alpha * bSma);

            decimal ls = (alpha * b) + beta;
            lsList.AddRounded(ls);

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
            decimal sma = smaList[i];
            decimal a0 = linRegList[i];
            decimal a1 = i >= 1 ? linRegList[i - 1] : 0;
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal m = a0 - a1 + sma;

            decimal prevIe2 = ie2List.LastOrDefault();
            decimal ie2 = (m + a0) / 2;
            ie2List.AddRounded(ie2);

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
        var stdDevList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal stdDev = stdDevList[i];
            decimal sma = smaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            var lbList = tempList.TakeLastExt(length).Select(x => (double)x);
            decimal y1 = linregList[i];
            y1List.AddRounded(y1);

            decimal x2 = i >= 1 ? outList[i - 1] : currentValue;
            x2List.AddRounded(x2);

            var x2LbList = x2List.TakeLastExt(length).Select(x => (double)x).ToList();
            var r2x2 = GoodnessOfFit.R(x2LbList, lbList);
            r2x2 = IsValueNullOrInfinity(r2x2) ? 0 : r2x2;
            decimal x2Avg = (decimal)x2LbList.TakeLastExt(length).Average();
            decimal x2Dev = x2 - x2Avg;

            decimal x2Pow = Pow(x2Dev, 2);
            x2PowList.AddRounded(x2Pow);

            decimal x2PowAvg = x2PowList.TakeLastExt(length).Average();
            decimal x2StdDev = x2PowAvg >= 0 ? Sqrt(x2PowAvg) : 0;
            decimal a = x2StdDev != 0 ? stdDev * (decimal)r2x2 / x2StdDev : 0;
            decimal b = sma - (a * x2Avg);

            decimal y2 = (a * x2) + b;
            y2List.AddRounded(y2);

            var ry1 = Math.Pow(GoodnessOfFit.R(y1List.TakeLastExt(length).Select(x => (double)x), lbList), 2);
            ry1 = IsValueNullOrInfinity(ry1) ? 0 : ry1;
            var ry2 = Math.Pow(GoodnessOfFit.R(y2List.TakeLastExt(length).Select(x => (double)x), lbList), 2);
            ry2 = IsValueNullOrInfinity(ry2) ? 0 : ry2;

            decimal prevOutVal = outList.LastOrDefault();
            decimal outval = ((decimal)ry1 * y1) + ((decimal)ry2 * y2) + ((1 - (decimal)(ry1 + ry2)) * x2);
            outList.AddRounded(outval);

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
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentOpen = openList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal b = currentHigh - currentLow != 0 ? Math.Abs(currentValue - currentOpen) / (currentHigh - currentLow) : 0;
            decimal c = b > 1 ? 1 : b;

            decimal prevD = i >= 1 ? dList[i - 1] : currentValue;
            decimal d = (c * currentValue) + ((1 - c) * prevD);
            dList.AddRounded(d);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevRema1 = i >= 1 ? remaList[i - 1] : 0;
            decimal prevRema2 = i >= 2 ? remaList[i - 2] : 0;

            decimal rema = (prevRema1 + (alpha * (currentValue - prevRema1)) + (lambda * ((2 * prevRema1) - prevRema2))) / (lambda + 1);
            remaList.AddRounded(rema);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma1 = sma1List[i];
            decimal sma2 = sma2List[i];
            decimal sma3 = sma3List[i];

            decimal prevMa = maList.LastOrDefault();
            decimal ma = sma3 + sma2 - sma1;
            maList.AddRounded(ma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal highest1 = highestList1[i];
            decimal lowest1 = lowestList1[i];
            decimal highest2 = highestList2[i];
            decimal lowest2 = lowestList2[i];
            decimal ar = 2 * (highest1 - lowest1);
            decimal br = 2 * (highest2 - lowest2);
            decimal k1 = ar != 0 ? (1 - ar) / ar : 0;
            decimal k2 = br != 0 ? (1 - br) / br : 0;
            decimal alpha = k1 != 0 ? k2 / k1 : 0;
            decimal r1 = alpha != 0 && highest1 >= 0 ? Sqrt(highest1) / 4 * ((alpha - 1) / alpha) * (k2 / (k2 + 1)) : 0;
            decimal r2 = highest2 >= 0 ? Sqrt(highest2) / 4 * (alpha - 1) * (k1 / (k1 + 1)) : 0;
            decimal factor = r1 != 0 ? r2 / r1 : 0;
            decimal altk = Pow(factor >= 1 ? 1 : factor, Sqrt(length)) * ((decimal)1 / length);

            decimal prevAltma = i >= 1 ? altmaList[i - 1] : currentValue;
            decimal altma = (altk * currentValue) + ((1 - altk) * prevAltma);
            altmaList.AddRounded(altma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevAuc = i >= 1 ? aucList[i - 1] : 1;
            decimal prevAdc = i >= 1 ? adcList[i - 1] : 1;

            decimal auc = currentValue > prevValue ? (k * (currentValue - prevValue)) + ((1 - k) * prevAuc) : (1 - k) * prevAuc;
            aucList.AddRounded(auc);

            decimal adc = currentValue > prevValue ? ((1 - k) * prevAdc) : (k * (prevValue - currentValue)) + ((1 - k) * prevAdc);
            adcList.AddRounded(adc);

            decimal rsiValue = (length - 1) * ((adc * rsiLevel / (100 - rsiLevel)) - auc);
            decimal prevRevRsi = revRsiList.LastOrDefault();
            decimal revRsi = rsiValue >= 0 ? currentValue + rsiValue : currentValue + (rsiValue * (100 - rsiLevel) / rsiLevel);
            revRsiList.AddRounded(revRsi);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal w = 0, vw = 0;
            for (int j = 0; j < length; j++)
            {
                decimal prevV = i >= j ? inputList[i - j] : 0;
                w += (1 - Pow(j / width, 2)) * Exp(-(Pow(j, 2) / (2 * Pow(width, 2))));
                vw += prevV * w;
            }
            
            decimal prevRrma = rrmaList.LastOrDefault();
            decimal rrma = w != 0 ? vw / w : 0;
            rrmaList.AddRounded(rrma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevBot = i >= 1 ? botList[i - 1] : currentValue;
            decimal prevNRes = i >= 1 ? nResList[i - 1] : currentValue;

            decimal bot = ((1 - alpha) * prevBot) + currentValue;
            botList.AddRounded(bot);

            decimal nRes = ((1 - alpha) * prevNRes) + (alpha * (currentValue + bot - prevBot));
            nResList.AddRounded(nRes);

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
            decimal prevFastEma = i >= 1 ? fastEmaList[i - 1] : 0;
            decimal prevSlowEma = i >= 1 ? slowEmaList[i - 1] : 0;

            decimal pMacdEq = fastAlpha - slowAlpha != 0 ? ((prevFastEma * fastAlpha) - (prevSlowEma * slowAlpha)) / (fastAlpha - slowAlpha) : 0;
            pMacdEqList.AddRounded(pMacdEq);

            decimal pMacdLevel = fastAlpha - slowAlpha != 0 ? (macdLevel - (prevFastEma * (1 - fastAlpha)) + (prevSlowEma * (1 - slowAlpha))) /
                (fastAlpha - slowAlpha) : 0;
            pMacdLevelList.AddRounded(pMacdLevel);
        }

        var pMacdEqSignalList = GetMovingAverageList(stockData, maType, signalLength, pMacdEqList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal pMacdEq = pMacdEqList[i];
            decimal pMacdEqSignal = pMacdEqSignalList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevPMacdEq = i >= 1 ? pMacdEqList[i - 1] : 0;

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
            decimal currentValue = inputList[i];
            yList.AddRounded(currentValue);

            decimal x = i;
            xList.AddRounded(x);

            decimal xy = x * currentValue;
            xyList.AddRounded(xy);

            decimal sumX = xList.TakeLastExt(length).Sum();
            decimal sumY = yList.TakeLastExt(length).Sum();
            decimal sumXY = xyList.TakeLastExt(length).Sum();
            decimal sumX2 = x2List.TakeLastExt(length).Sum();
            decimal top = (length * sumXY) - (sumX * sumY);
            decimal bottom = (length * sumX2) - Pow(sumX, 2);

            decimal b = bottom != 0 ? top / bottom : 0;
            slopeList.AddRounded(b);

            decimal a = length != 0 ? (sumY - (b * sumX)) / length : 0;
            interceptList.AddRounded(a);

            decimal predictedToday = a + (b * x);
            predictedTodayList.AddRounded(predictedToday);

            decimal prevPredictedNextDay = predictedTomorrowList.LastOrDefault();
            decimal predictedNextDay = a + (b * (x + 1));
            predictedTomorrowList.AddRounded(predictedNextDay);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMaaq = i >= 1 ? maaqList[i - 1] : currentValue;
            decimal er = erList[i];
            decimal temp = (er * fastAlpha) + slowAlpha;

            decimal maaq = prevMaaq + (Pow(temp, 2) * (currentValue - prevMaaq));
            maaqList.AddRounded(maaq);

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
            decimal currentValue = inputList[i];
            decimal prevMdi = i >= 1 ? mdiList.LastOrDefault() : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal ratio = prevMdi != 0 ? currentValue / prevMdi : 0;
            decimal bottom = k * length * Pow(ratio, 4);

            decimal mdi = bottom != 0 ? prevMdi + ((currentValue - prevMdi) / Math.Max(bottom, 1)) : currentValue;
            mdiList.AddRounded(mdi);

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
            decimal currentPrice = inputList[i];
            decimal prevP1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevP2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevP3 = i >= 3 ? inputList[i - 3] : 0;

            decimal smth = (currentPrice + (2 * prevP1) + (2 * prevP2) + prevP3) / 6;
            smthList.AddRounded(smth);

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
            value2List.AddRounded(value2);

            len = len < 3 ? 3 : len;
            alpha = (decimal)2 / (len + 1);

            decimal prevFilter = filterList.LastOrDefault();
            decimal filter = (alpha * smth) + ((1 - alpha) * prevFilter);
            filterList.AddRounded(filter);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentMhlMa = mhlMaList[i];
            decimal prevMhlma = i >= 1 ? mhlMaList[i - 1] : 0;

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
            decimal ma1 = ma1List[i];
            decimal ma2 = ma2List[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevNma = nmaList.LastOrDefault();
            decimal nma = ((1 + alpha) * ma1) - (alpha * ma2);
            nmaList.AddRounded(nma);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevAlpha1 = i >= 1 ? alpha1List[i - 1] : currentValue;
            decimal alpha1 = (a1 * currentValue) + ((1 - a1) * prevAlpha1);
            alpha1List.AddRounded(alpha1);

            decimal prevAlpha2 = i >= 1 ? alpha2List[i - 1] : currentValue;
            decimal priorAlpha2 = i >= 2 ? alpha2List[i - 2] : currentValue;
            decimal alpha2 = (b2 * prevAlpha2) - (a2 * a2 * priorAlpha2) + ((1 - b2 + (a2 * a2)) * currentValue);
            alpha2List.AddRounded(alpha2);

            decimal prevAlpha3 = i >= 1 ? alpha3List[i - 1] : currentValue;
            decimal prevAlpha3_2 = i >= 2 ? alpha3List[i - 2] : currentValue;
            decimal prevAlpha3_3 = i >= 3 ? alpha3List[i - 3] : currentValue;
            decimal alpha3 = ((b3 + c) * prevAlpha3) - ((c + (b3 * c)) * prevAlpha3_2) + (c * c * prevAlpha3_3) + ((1 - b3 + c) * (1 - c) * currentValue);
            alpha3List.AddRounded(alpha3);

            decimal detrend1 = currentValue - alpha1;
            decimal detrend2 = currentValue - alpha2;
            decimal detrend3 = currentValue - alpha3;

            decimal prevBeta1 = i >= 1 ? beta1List[i - 1] : 0;
            decimal beta1 = (a1 * detrend1) + ((1 - a1) * prevBeta1);
            beta1List.AddRounded(beta1);

            decimal prevBeta2 = i >= 1 ? beta2List[i - 1] : 0;
            decimal prevBeta2_2 = i >= 2 ? beta2List[i - 2] : 0;
            decimal beta2 = (b2 * prevBeta2) - (a2 * a2 * prevBeta2_2) + ((1 - b2 + (a2 * a2)) * detrend2);
            beta2List.AddRounded(beta2);

            decimal prevBeta3_2 = i >= 2 ? beta3List[i - 2] : 0;
            decimal prevBeta3_3 = i >= 3 ? beta3List[i - 3] : 0;
            decimal beta3 = ((b3 + c) * prevBeta3_2) - ((c + (b3 * c)) * prevBeta3_2) + (c * c * prevBeta3_3) + ((1 - b3 + c) * (1 - c) * detrend3);
            beta3List.AddRounded(beta3);

            decimal mda1 = alpha1 + ((decimal)1 / 1 * beta1);
            mda1List.AddRounded(mda1);

            decimal prevMda2 = mda2List.LastOrDefault();
            decimal mda2 = alpha2 + ((decimal)1 / 2 * beta2);
            mda2List.AddRounded(mda2);

            decimal mda3 = alpha3 + ((decimal)1 / 3 * beta3);
            mda3List.AddRounded(mda3);

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
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevB2 = i >= 1 ? b2List[i - 1] : currentValue;
            decimal b2 = currentValue > (alpha * currentValue) + ((1 - alpha) * prevB2) ? currentValue : (alpha * currentValue) + ((1 - alpha) * prevB2);
            b2List.AddRounded(b2);

            decimal prevC2 = i >= 1 ? c2List[i - 1] : currentValue;
            decimal c2 = currentValue < (alpha * currentValue) + ((1 - alpha) * prevC2) ? currentValue : (alpha * currentValue) + ((1 - alpha) * prevC2);
            c2List.AddRounded(c2);

            decimal prevOs2 = os2List.LastOrDefault();
            decimal os2 = currentValue == b2 ? 1 : currentValue == c2 ? 0 : prevOs2;
            os2List.AddRounded(os2);

            decimal upper2 = (beta * b2) + ((1 - beta) * c2);
            decimal lower2 = (beta * c2) + ((1 - beta) * b2);

            decimal prevTs2 = ts2List.LastOrDefault();
            decimal ts2 = (os2 * upper2) + ((1 - os2) * lower2);
            ts2List.AddRounded(ts2);

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

    /// <summary>
    /// Calculates the Damped Sine Wave Weighted Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDampedSineWaveWeightedFilter(this StockData stockData, int length = 50)
    {
        List<decimal> dswwfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal w, wSum = 0, wvSum = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevValue = i >= j - 1 ? inputList[i - (j - 1)] : 0;

                w = Sin(MinOrMax(2 * Pi * (decimal)j / length, 0.99m, 0.01m)) / j;
                wvSum += w * prevValue;
                wSum += w;
            }

            decimal prevDswwf = dswwfList.LastOrDefault();
            decimal dswwf = wSum != 0 ? wvSum / wSum : 0;
            dswwfList.AddRounded(dswwf);

            var signal = GetCompareSignal(currentValue - dswwf, prevVal - prevDswwf);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dswwf", dswwfList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dswwfList;
        stockData.IndicatorName = IndicatorName.DampedSineWaveWeightedFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Double Exponential Smoothing
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="alpha"></param>
    /// <param name="gamma"></param>
    /// <returns></returns>
    public static StockData CalculateDoubleExponentialSmoothing(this StockData stockData, decimal alpha = 0.01m, decimal gamma = 0.9m)
    {
        List<decimal> sList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal x = inputList[i];
            decimal prevX = i >= 1 ? inputList[i - 1] : 0;
            decimal prevS = i >= 1 ? sList[i - 1] : 0;
            decimal prevS2 = i >= 2 ? sList[i - 2] : 0;
            decimal sChg = prevS - prevS2;

            decimal s = (alpha * x) + ((1 - alpha) * (prevS + (gamma * (sChg + ((1 - gamma) * sChg)))));
            sList.AddRounded(s);

            var signal = GetCompareSignal(x - s, prevX - prevS);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Des", sList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sList;
        stockData.IndicatorName = IndicatorName.DoubleExponentialSmoothing;

        return stockData;
    }

    /// <summary>
    /// Calculates the Distance Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDistanceWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> dwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                decimal distanceSum = 0;
                for (int k = 0; k < length; k++)
                {
                    decimal prevValue2 = i >= k ? inputList[i - k] : 0;

                    distanceSum += Math.Abs(prevValue - prevValue2);
                }

                decimal weight = distanceSum != 0 ? 1 / distanceSum : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevDwma = dwmaList.LastOrDefault();
            decimal dwma = weightedSum != 0 ? sum / weightedSum : 0;
            dwmaList.AddRounded(dwma);

            var signal = GetCompareSignal(currentValue - dwma, prevVal - prevDwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dwma", dwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dwmaList;
        stockData.IndicatorName = IndicatorName.DistanceWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Dynamically Adjustable Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDynamicallyAdjustableFilter(this StockData stockData, int length = 14)
    {
        List<decimal> outList = new();
        List<decimal> kList = new();
        List<decimal> srcList = new();
        List<decimal> srcDevList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevOut = i >= 1 ? outList[i - 1] : currentValue;
            decimal prevK = i >= 1 ? kList[i - 1] : 0;

            decimal src = currentValue + (currentValue - prevOut);
            srcList.AddRounded(src);

            decimal outVal = prevOut + (prevK * (src - prevOut));
            outList.AddRounded(outVal);

            decimal srcSma = srcList.TakeLastExt(length).Average();
            decimal srcDev = Pow(src - srcSma, 2);
            srcDevList.AddRounded(srcDev);

            decimal srcStdDev = Sqrt(srcDevList.TakeLastExt(length).Average());
            decimal k = src - outVal != 0 ? Math.Abs(src - outVal) / (Math.Abs(src - outVal) + (srcStdDev * length)) : 0;
            kList.AddRounded(k);

            var signal = GetCompareSignal(currentValue - outVal, prevValue - prevOut);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Daf", outList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = outList;
        stockData.IndicatorName = IndicatorName.DynamicallyAdjustableFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Dynamically Adjustable Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateDynamicallyAdjustableMovingAverage(this StockData stockData, int fastLength = 6, int slowLength = 200)
    {
        List<decimal> kList = new();
        List<decimal> amaList = new();
        List<decimal> tempList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var shortStdDevList = CalculateStandardDeviationVolatility(stockData, length: fastLength).CustomValuesList;
        var longStdDevList = CalculateStandardDeviationVolatility(stockData, length: slowLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal a = shortStdDevList[i];
            decimal b = longStdDevList[i];
            decimal v = a != 0 ? (b / a) + fastLength : fastLength;

            decimal prevValue = tempList.LastOrDefault();
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            int p;
            try
            {
                p = (int)Math.Round(v);
            }
            catch
            {
                p = slowLength;
            }

            decimal prevK = i >= p ? kList[i - p] : 0;
            decimal k = tempList.Sum();
            kList.AddRounded(k);

            decimal prevAma = amaList.LastOrDefault();
            decimal ama = p != 0 ? (k - prevK) / p : 0;
            amaList.AddRounded(ama);

            var signal = GetCompareSignal(currentValue - ama, prevValue - prevAma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dama", amaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = amaList;
        stockData.IndicatorName = IndicatorName.DynamicallyAdjustableMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Spencer 21 Point Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateSpencer21PointMovingAverage(this StockData stockData)
    {
        List<decimal> spmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= 20; j++)
            {
                var weight = j switch
                {
                    0 => -1,
                    1 => -3,
                    2 => -5,
                    3 => -5,
                    4 => -2,
                    5 => 6,
                    6 => 18,
                    7 => 33,
                    8 => 47,
                    9 => 57,
                    10 => 60,
                    11 => 57,
                    12 => 47,
                    13 => 33,
                    14 => 18,
                    15 => 6,
                    16 => -2,
                    17 => -5,
                    18 => -5,
                    19 => -3,
                    20 => -1,
                    _ => 0,
                };
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevSpma = spmaList.LastOrDefault();
            decimal spma = weightedSum != 0 ? sum / weightedSum : 0;
            spmaList.AddRounded(spma);

            var signal = GetCompareSignal(currentValue - spma, prevVal - prevSpma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "S21ma", spmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = spmaList;
        stockData.IndicatorName = IndicatorName.Spencer21PointMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Spencer 15 Point Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateSpencer15PointMovingAverage(this StockData stockData)
    {
        List<decimal> spmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= 14; j++)
            {
                var weight = j switch
                {
                    0 => -3,
                    1 => -6,
                    2 => -5,
                    3 => 3,
                    4 => 21,
                    5 => 46,
                    6 => 67,
                    7 => 74,
                    8 => 67,
                    9 => 46,
                    10 => 21,
                    11 => 3,
                    12 => -5,
                    13 => -6,
                    14 => -3,
                    _ => 0,
                };
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevSpma = spmaList.LastOrDefault();
            decimal spma = weightedSum != 0 ? sum / weightedSum : 0;
            spmaList.AddRounded(spma);

            var signal = GetCompareSignal(currentValue - spma, prevVal - prevSpma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "S15ma", spmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = spmaList;
        stockData.IndicatorName = IndicatorName.Spencer15PointMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Square Root Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSquareRootWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> srwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Pow(length - j, 0.5m);
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevSrwma = srwmaList.LastOrDefault();
            decimal srwma = weightedSum != 0 ? sum / weightedSum : 0;
            srwmaList.AddRounded(srwma);

            var signal = GetCompareSignal(currentValue - srwma, prevVal - prevSrwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Srwma", srwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = srwmaList;
        stockData.IndicatorName = IndicatorName.SquareRootWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Shapeshifting Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateShapeshiftingMovingAverage(this StockData stockData, int length = 50)
    {
        List<decimal> filtXList = new();
        List<decimal> filtNList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sumX = 0, weightedSumX = 0, sumN = 0, weightedSumN = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal x = (decimal)j / (length - 1);
                decimal n = -1 + (x * 2);
                decimal wx = 1 - (2 * x / (Pow(x, 4) + 1));
                decimal wn = 1 - (2 * Pow(n, 2) / (Pow(n, 4 - (4 % 2)) + 1));
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sumX += prevValue * wx;
                weightedSumX += wx;
                sumN += prevValue * wn;
                weightedSumN += wn;
            }

            decimal prevFiltX = filtXList.LastOrDefault();
            decimal filtX = weightedSumX != 0 ? sumX / weightedSumX : 0;
            filtXList.AddRounded(filtX);

            decimal filtN = weightedSumN != 0 ? sumN / weightedSumN : 0;
            filtNList.AddRounded(filtN);

            var signal = GetCompareSignal(currentValue - filtX, prevVal - prevFiltX);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sma", filtXList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtXList;
        stockData.IndicatorName = IndicatorName.ShapeshiftingMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Self Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSelfWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> wmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal pValue = i >= j ? inputList[i - j] : 0;
                decimal weight = i >= length + j ? inputList[i - (length + j)] : 0;
                weightSum += weight;
                sum += weight * pValue;
            }

            decimal prevWma = wmaList.LastOrDefault();
            decimal wma = weightSum != 0 ? sum / weightSum : 0;
            wmaList.AddRounded(wma);

            var signal = GetCompareSignal(currentValue - wma, prevValue - prevWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Swma", wmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wmaList;
        stockData.IndicatorName = IndicatorName.SelfWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sine Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSineWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> swmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal sum = 0, weightedSum = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal weight = Sin((j + 1) * Pi / (length + 1));
                decimal prevValue = i >= j ? inputList[i - j] : 0;

                sum += prevValue * weight;
                weightedSum += weight;
            }

            decimal prevSwma = swmaList.LastOrDefault();
            decimal swma = weightedSum != 0 ? sum / weightedSum : 0;
            swmaList.AddRounded(swma);

            var signal = GetCompareSignal(currentValue - swma, prevVal - prevSwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Swma", swmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = swmaList;
        stockData.IndicatorName = IndicatorName.SineWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Simplified Weighted Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSimplifiedWeightedMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> wmaList = new();
        List<decimal> cmlList = new();
        List<decimal> cmlSumList = new();
        List<decimal> tempList = new();
        List<decimal> sumList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal cml = tempList.Sum();
            cmlList.AddRounded(cml);

            decimal prevCmlSum = i >= length ? cmlSumList[i - length] : 0;
            decimal cmlSum = cmlList.Sum();
            cmlSumList.AddRounded(cmlSum);

            decimal prevSum = sumList.LastOrDefault();
            decimal sum = cmlSum - prevCmlSum;
            sumList.AddRounded(sum);

            decimal prevWma = wmaList.LastOrDefault();
            decimal wma = ((length * cml) - prevSum) / (length * (decimal)(length + 1) / 2);
            wmaList.AddRounded(wma);

            var signal = GetCompareSignal(currentValue - wma, prevValue - prevWma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Swma", wmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = wmaList;
        stockData.IndicatorName = IndicatorName.SimplifiedWeightedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Simplified Least Squares Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSimplifiedLeastSquaresMovingAverage(this StockData stockData, int length = 14)
    {
        List<decimal> cmlList = new();
        List<decimal> cmlSumList = new();
        List<decimal> tempList = new();
        List<decimal> sumList = new();
        List<decimal> lsmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal prevCml = i >= length ? cmlList[i - length] : 0;
            decimal cml = tempList.Sum();
            cmlList.AddRounded(cml);

            decimal prevCmlSum = i >= length ? cmlSumList[i - length] : 0;
            decimal cmlSum = cmlList.Sum();
            cmlSumList.AddRounded(cmlSum);

            decimal prevSum = sumList.LastOrDefault();
            decimal sum = cmlSum - prevCmlSum;
            sumList.AddRounded(sum);

            decimal wma = ((length * cml) - prevSum) / (length * (decimal)(length + 1) / 2);
            decimal prevLsma = lsmaList.LastOrDefault();
            decimal lsma = length != 0 ? (3 * wma) - (2 * (cml - prevCml) / length) : 0;
            lsmaList.AddRounded(lsma);

            var signal = GetCompareSignal(currentValue - lsma, prevValue - prevLsma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Slsma", lsmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = lsmaList;
        stockData.IndicatorName = IndicatorName.SimplifiedLeastSquaresMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sharp Modified Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSharpModifiedMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> shmmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentSma = smaList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;

            decimal slope = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevValue = i >= j - 1 ? inputList[i - (j - 1)] : 0;
                decimal factor = 1 + (2 * (j - 1));
                slope += prevValue * (length - factor) / 2;
            }

            decimal prevShmma = shmmaList.LastOrDefault();
            decimal shmma = currentSma + (6 * slope / ((length + 1) * length));
            shmmaList.AddRounded(shmma);

            var signal = GetCompareSignal(currentValue - shmma, prevVal - prevShmma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Smma", shmmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = shmmaList;
        stockData.IndicatorName = IndicatorName.SharpModifiedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Slow Smoothed Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSlowSmoothedMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 15)
    {
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int w2 = MinOrMax((int)Math.Ceiling((decimal)length / 3));
        int w1 = MinOrMax((int)Math.Ceiling((decimal)(length - w2) / 2));
        int w3 = MinOrMax((int)Math.Floor((decimal)(length - w2) / 2));

        var l1List = GetMovingAverageList(stockData, maType, w1, inputList);
        var l2List = GetMovingAverageList(stockData, maType, w2, l1List);
        var l3List = GetMovingAverageList(stockData, maType, w3, l2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal l3 = l3List[i];
            decimal prevL3 = i >= 1 ? l3List[i - 1] : 0;

            var signal = GetCompareSignal(currentValue - l3, prevValue - prevL3);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ssma", l3List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = l3List;
        stockData.IndicatorName = IndicatorName.SlowSmoothedMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Sequentially Filtered Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSequentiallyFilteredMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 50)
    {
        List<decimal> sfmaList = new();
        List<decimal> signList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma = smaList[i];
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;

            decimal a = Math.Sign(sma - prevSma);
            signList.AddRounded(a);

            decimal sum = signList.TakeLastExt(length).Sum();
            decimal alpha = Math.Abs(sum) == length ? 1 : 0;
            decimal prevSfma = i >= 1 ? sfmaList[i - 1] : sma;
            decimal sfma = (alpha * sma) + ((1 - alpha) * prevSfma);
            sfmaList.AddRounded(sfma);

            var signal = GetCompareSignal(currentValue - sfma, prevValue - prevSfma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sfma", sfmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = sfmaList;
        stockData.IndicatorName = IndicatorName.SequentiallyFilteredMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Svama
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSvama(this StockData stockData, int length = 14)
    {
        List<decimal> hList = new();
        List<decimal> lList = new();
        List<decimal> cMaxList = new();
        List<decimal> cMinList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal a = volumeList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevH = i >= 1 ? hList[i - 1] : a;
            decimal h = a > prevH ? a : prevH;
            hList.AddRounded(h);

            decimal prevL = i >= 1 ? lList[i - 1] : a;
            decimal l = a < prevL ? a : prevL;
            lList.AddRounded(l);

            decimal bMax = h != 0 ? a / h : 0;
            decimal bMin = a != 0 ? l / a : 0;

            decimal prevCMax = i >= 1 ? cMaxList[i - 1] : currentValue;
            decimal cMax = (bMax * currentValue) + ((1 - bMax) * prevCMax);
            cMaxList.AddRounded(cMax);

            decimal prevCMin = i >= 1 ? cMinList[i - 1] : currentValue;
            decimal cMin = (bMin * currentValue) + ((1 - bMin) * prevCMin);
            cMinList.AddRounded(cMin);

            var signal = GetCompareSignal(currentValue - cMax, prevValue - prevCMax);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Svama", cMaxList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cMaxList;
        stockData.IndicatorName = IndicatorName.Svama;

        return stockData;
    }

    /// <summary>
    /// Calculates the Setting Less Trend Step Filtering
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateSettingLessTrendStepFiltering(this StockData stockData)
    {
        List<decimal> chgList = new();
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal prevA = aList.LastOrDefault();
            decimal sc = Math.Abs(currentValue - prevB) + prevA != 0 ? Math.Abs(currentValue - prevB) / (Math.Abs(currentValue - prevB) + prevA) : 0;
            decimal sltsf = (sc * currentValue) + ((1 - sc) * prevB);

            decimal chg = Math.Abs(sltsf - prevB);
            chgList.AddRounded(chg);

            decimal a = chgList.Average() * (1 + sc);
            aList.AddRounded(a);

            decimal b = sltsf > prevB + a ? sltsf : sltsf < prevB - a ? sltsf : prevB;
            bList.AddRounded(b);

            var signal = GetCompareSignal(currentValue - b, prevValue - prevB);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Sltsf", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bList;
        stockData.IndicatorName = IndicatorName.SettingLessTrendStepFiltering;

        return stockData;
    }

    /// <summary>
    /// Calculates the Elastic Volume Weighted Moving Average V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateElasticVolumeWeightedMovingAverageV1(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 40, decimal mult = 20)
    {
        List<decimal> evwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentAvgVolume = volumeSmaList[i];
            decimal currentVolume = volumeList[i];
            decimal n = currentAvgVolume * mult;

            decimal prevEVWMA = i >= 1 ? evwmaList.LastOrDefault() : currentValue;
            decimal evwma = n > 0 ? (((n - currentVolume) * prevEVWMA) + (currentVolume * currentValue)) / n : 0; ;
            evwmaList.AddRounded(evwma);

            var signal = GetCompareSignal(currentValue - evwma, prevValue - prevEVWMA);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Evwma", evwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = evwmaList;
        stockData.IndicatorName = IndicatorName.ElasticVolumeWeightedMovingAverageV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Elastic Volume Weighted Moving Average V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateElasticVolumeWeightedMovingAverageV2(this StockData stockData, int length = 14)
    {
        List<decimal> tempList = new();
        List<decimal> evwmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal currentVolume = volumeList[i];
            tempList.AddRounded(currentVolume);

            decimal volumeSum = tempList.TakeLastExt(length).Sum();
            decimal prevEvwma = evwmaList.LastOrDefault();
            decimal evwma = volumeSum != 0 ? (((volumeSum - currentVolume) * prevEvwma) + (currentVolume * currentValue)) / volumeSum : 0;
            evwmaList.AddRounded(evwma);

            var signal = GetCompareSignal(currentValue - evwma, prevValue - prevEvwma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Evwma", evwmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = evwmaList;
        stockData.IndicatorName = IndicatorName.ElasticVolumeWeightedMovingAverageV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Equity Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEquityMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> chgXList = new();
        List<decimal> chgXCumList = new();
        List<decimal> xList = new();
        List<decimal> eqmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma = smaList[i];
            decimal prevEqma = i >= 1 ? eqmaList[i - 1] : currentValue;

            decimal prevX = xList.LastOrDefault();
            decimal x = Math.Sign(currentValue - sma);
            xList.AddRounded(x);

            decimal chgX = (currentValue - prevValue) * prevX;
            chgXList.AddRounded(chgX);

            decimal chgXCum = (currentValue - prevValue) * x;
            chgXCumList.AddRounded(chgXCum);

            decimal opteq = chgXCumList.Sum();
            decimal req = chgXList.TakeLastExt(length).Sum();
            decimal alpha = opteq != 0 ? MinOrMax(req / opteq, 0.99m, 0.01m) : 0.99m;

            decimal eqma = (alpha * currentValue) + ((1 - alpha) * prevEqma);
            eqmaList.AddRounded(eqma);

            var signal = GetCompareSignal(currentValue - eqma, prevValue - prevEqma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eqma", eqmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = eqmaList;
        stockData.IndicatorName = IndicatorName.EquityMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Edge Preserving Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateEdgePreservingFilter(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 200, 
        int smoothLength = 50)
    {
        List<decimal> osList = new();
        List<decimal> absOsList = new();
        List<decimal> hList = new();
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> cList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal sma = smaList[i];

            decimal os = currentValue - sma;
            osList.AddRounded(os);

            decimal absOs = Math.Abs(os);
            absOsList.AddRounded(absOs);
        }

        stockData.CustomValuesList = absOsList;
        var pList = CalculateLinearRegression(stockData, smoothLength).CustomValuesList;
        var (highestList, _) = GetMaxAndMinValuesList(pList, length);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal p = pList[i];
            decimal highest = highestList[i];
            decimal os = osList[i];

            decimal prevH = hList.LastOrDefault();
            decimal h = highest != 0 ? p / highest : 0;
            hList.AddRounded(h);

            decimal cnd = h == 1 && prevH != 1 ? 1 : 0;
            decimal sign = cnd == 1 && os < 0 ? 1 : cnd == 1 && os > 0 ? -1 : 0;
            bool condition = sign != 0;

            decimal prevA = i >= 1 ? aList[i - 1] : 1;
            decimal a = condition ? 1 : prevA + 1;
            aList.AddRounded(a);

            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal b = a == 1 ? currentValue : prevB + currentValue;
            bList.AddRounded(b);

            decimal prevC = cList.LastOrDefault();
            decimal c = a != 0 ? b / a : 0;
            cList.AddRounded(c);

            var signal = GetCompareSignal(currentValue - c, prevValue - prevC);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Epf", cList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cList;
        stockData.IndicatorName = IndicatorName.EdgePreservingFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 2 Pole Super Smoother Filter V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers2PoleSuperSmootherFilterV2(this StockData stockData, int length = 10)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a = Exp(MinOrMax(-1.414m * Pi / length, -0.01m, -0.99m));
        decimal b = 2 * a * Cos(MinOrMax(1.414m * Pi / length, 0.99m, 0.01m));
        decimal c2 = b;
        decimal c3 = -a * a;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;

            decimal filt = (c1 * ((currentValue + prevValue) / 2)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "E2ssf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers2PoleSuperSmootherFilterV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 3 Pole Super Smoother Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers3PoleSuperSmootherFilter(this StockData stockData, int length = 20)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal arg = MinOrMax(Pi / length, 0.99m, 0.01m);
        decimal a1 = Exp(-arg);
        decimal b1 = 2 * a1 * Cos(1.738m * arg);
        decimal c1 = a1 * a1;
        decimal coef2 = b1 + c1;
        decimal coef3 = -(c1 + (b1 * c1));
        decimal coef4 = c1 * c1;
        decimal coef1 = 1 - coef2 - coef3 - coef4;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;
            decimal prevFilter3 = i >= 3 ? filtList[i - 3] : 0;

            decimal filt = i < 4 ? currentValue : (coef1 * currentValue) + (coef2 * prevFilter1) + (coef3 * prevFilter2) + (coef4 * prevFilter3);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "E3ssf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers3PoleSuperSmootherFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 2 Pole Butterworth Filter V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers2PoleButterworthFilterV1(this StockData stockData, int length = 10)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a = Exp(MinOrMax(-1.414m * Pi / length, -0.01m, -0.99m));
        decimal b = 2 * a * Cos(MinOrMax(1.414m * 1.25m * Pi / length, 0.99m, 0.01m));
        decimal c2 = b;
        decimal c3 = -a * a;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;

            decimal filt = (c1 * currentValue) + (c2 * prevFilter1) + (c3 * prevFilter2);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "E2bf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers2PoleButterworthFilterV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 2 Pole Butterworth Filter V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers2PoleButterworthFilterV2(this StockData stockData, int length = 15)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a = Exp(MinOrMax(-1.414m * Pi / length, -0.01m, -0.99m));
        decimal b = 2 * a * Cos(MinOrMax(1.414m * Pi / length, 0.99m, 0.01m));
        decimal c2 = b;
        decimal c3 = -a * a;
        decimal c1 = (1 - b + Pow(a, 2)) / 4;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevValue3 = i >= 3 ? inputList[i - 3] : 0;

            decimal filt = i < 3 ? currentValue : (c1 * (currentValue + (2 * prevValue1) + prevValue3)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue1 - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "E2bf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers2PoleButterworthFilterV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 3 Pole Butterworth Filter V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers3PoleButterworthFilterV1(this StockData stockData, int length = 10)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a = Exp(MinOrMax(-Pi / length, -0.01m, -0.99m));
        decimal b = 2 * a * Cos(MinOrMax(1.738m * Pi / length, 0.99m, 0.01m));
        decimal c = a * a;
        decimal d2 = b + c;
        decimal d3 = -(c + (b * c));
        decimal d4 = c * c;
        decimal d1 = 1 - d2 - d3 - d4;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;
            decimal prevFilter3 = i >= 3 ? filtList[i - 3] : 0;

            decimal filt = (d1 * currentValue) + (d2 * prevFilter1) + (d3 * prevFilter2) + (d4 * prevFilter3);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "E3bf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers3PoleButterworthFilterV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 3 Pole Butterworth Filter V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers3PoleButterworthFilterV2(this StockData stockData, int length = 15)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(MinOrMax(-Pi / length, -0.01m, -0.99m));
        decimal b1 = 2 * a1 * Cos(MinOrMax(1.738m * Pi / length, 0.99m, 0.01m));
        decimal c1 = a1 * a1;
        decimal coef2 = b1 + c1;
        decimal coef3 = -(c1 + (b1 * c1));
        decimal coef4 = c1 * c1;
        decimal coef1 = (1 - b1 + c1) * (1 - c1) / 8;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevValue3 = i >= 3 ? inputList[i - 3] : 0;
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;
            decimal prevFilter3 = i >= 3 ? filtList[i - 3] : 0;

            decimal filt = i < 4 ? currentValue : (coef1 * (currentValue + (3 * prevValue1) + (3 * prevValue2) + prevValue3)) + (coef2 * prevFilter1) +
                (coef3 * prevFilter2) + (coef4 * prevFilter3);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue1 - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "E3bf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers3PoleButterworthFilterV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Gaussian Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersGaussianFilter(this StockData stockData, int length = 14)
    {
        List<decimal> gf1List = new();
        List<decimal> gf2List = new();
        List<decimal> gf3List = new();
        List<decimal> gf4List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal cosVal = MinOrMax(2 * Pi / length, 0.99m, 0.01m);
        decimal beta1 = (1 - Cos(cosVal)) / (Pow(2, (decimal)1 / 1) - 1);
        decimal beta2 = (1 - Cos(cosVal)) / (Pow(2, (decimal)1 / 2) - 1);
        decimal beta3 = (1 - Cos(cosVal)) / (Pow(2, (decimal)1 / 3) - 1);
        decimal beta4 = (1 - Cos(cosVal)) / (Pow(2, (decimal)1 / 4) - 1);
        decimal alpha1 = -beta1 + Sqrt(Pow(beta1, 2) + (2 * beta1));
        decimal alpha2 = -beta2 + Sqrt(Pow(beta2, 2) + (2 * beta2));
        decimal alpha3 = -beta3 + Sqrt(Pow(beta3, 2) + (2 * beta3));
        decimal alpha4 = -beta4 + Sqrt(Pow(beta4, 2) + (2 * beta4));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevGf1 = i >= 1 ? gf1List[i - 1] : 0;
            decimal prevGf2_1 = i >= 1 ? gf2List[i - 1] : 0;
            decimal prevGf2_2 = i >= 2 ? gf2List[i - 2] : 0;
            decimal prevGf3_1 = i >= 1 ? gf3List[i - 1] : 0;
            decimal prevGf4_1 = i >= 1 ? gf4List[i - 1] : 0;
            decimal prevGf3_2 = i >= 2 ? gf3List[i - 2] : 0;
            decimal prevGf4_2 = i >= 2 ? gf4List[i - 2] : 0;
            decimal prevGf3_3 = i >= 3 ? gf3List[i - 3] : 0;
            decimal prevGf4_3 = i >= 3 ? gf4List[i - 3] : 0;
            decimal prevGf4_4 = i >= 4 ? gf4List[i - 4] : 0;

            decimal gf1 = (alpha1 * currentValue) + ((1 - alpha1) * prevGf1);
            gf1List.AddRounded(gf1);

            decimal gf2 = (Pow(alpha2, 2) * currentValue) + (2 * (1 - alpha2) * prevGf2_1) - (Pow(1 - alpha2, 2) * prevGf2_2);
            gf2List.AddRounded(gf2);

            decimal gf3 = (Pow(alpha3, 3) * currentValue) + (3 * (1 - alpha3) * prevGf3_1) - (3 * Pow(1 - alpha3, 2) * prevGf3_2) +
                (Pow(1 - alpha3, 3) * prevGf3_3);
            gf3List.AddRounded(gf3);

            decimal gf4 = (Pow(alpha4, 4) * currentValue) + (4 * (1 - alpha4) * prevGf4_1) - (6 * Pow(1 - alpha4, 2) * prevGf4_2) +
                (4 * Pow(1 - alpha4, 3) * prevGf4_3) - (Pow(1 - alpha4, 4) * prevGf4_4);
            gf4List.AddRounded(gf4);

            var signal = GetCompareSignal(currentValue - gf4, prevValue - prevGf4_1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Egf1", gf1List },
            { "Egf2", gf2List },
            { "Egf3", gf3List },
            { "Egf4", gf4List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = gf4List;
        stockData.IndicatorName = IndicatorName.EhlersGaussianFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Recursive Median Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersRecursiveMedianFilter(this StockData stockData, int length1 = 5, int length2 = 12)
    {
        List<decimal> tempList = new();
        List<decimal> rmfList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alphaArg = MinOrMax(2 * Pi / length2, 0.99m, 0.01m);
        decimal alphaArgCos = Cos(alphaArg);
        decimal alpha = alphaArgCos != 0 ? (alphaArgCos + Sin(alphaArg) - 1) / alphaArgCos : 0;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = tempList.LastOrDefault();
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal median = tempList.TakeLastExt(length1).Median();
            decimal prevRmf = rmfList.LastOrDefault();
            decimal rmf = (alpha * median) + ((1 - alpha) * prevRmf);
            rmfList.AddRounded(rmf);

            var signal = GetCompareSignal(currentValue - rmf, prevValue - prevRmf);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ermf", rmfList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rmfList;
        stockData.IndicatorName = IndicatorName.EhlersRecursiveMedianFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Super Smoother Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersSuperSmootherFilter(this StockData stockData, int length = 10)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(MinOrMax(-1.414m * Pi / length, -0.01m, -0.99m));
        decimal b1 = 2 * a1 * Cos(MinOrMax(1.414m * Pi / length, 0.99m, 0.01m));
        decimal coeff2 = b1;
        decimal coeff3 = (-1 * a1) * a1;
        decimal coeff1 = 1 - coeff2 - coeff3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;

            decimal prevFilt = filtList.LastOrDefault();
            decimal filt = (coeff1 * ((currentValue + prevValue) / 2)) + (coeff2 * prevFilter1) + (coeff3 * prevFilter2);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Essf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.EhlersSuperSmootherFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers 2 Pole Super Smoother Filter V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlers2PoleSuperSmootherFilterV1(this StockData stockData, int length = 15)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(MinOrMax(-1.414m * Pi / length, -0.01m, -0.99m));
        decimal b1 = 2 * a1 * Cos(MinOrMax(1.414m * Pi / length, 0.99m, 0.01m));
        decimal coef2 = b1;
        decimal coef3 = -a1 * a1;
        decimal coef1 = 1 - coef2 - coef3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevFilter1 = i >= 1 ? filtList[i - 1] : 0;
            decimal prevFilter2 = i >= 2 ? filtList[i - 2] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal filt = i < 3 ? currentValue : (coef1 * currentValue) + (coef2 * prevFilter1) + (coef3 * prevFilter2);
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Essf", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.Ehlers2PoleSuperSmootherFilterV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Average Error Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersAverageErrorFilter(this StockData stockData, int length = 27)
    {
        List<decimal> filtList = new();
        List<decimal> ssfList = new();
        List<decimal> e1List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(MinOrMax(-1.414m * Pi / length, -0.01m, -0.99m));
        decimal b1 = 2 * a1 * Cos(MinOrMax(1.414m * Pi / length, 0.99m, 0.01m));
        decimal c2 = b1;
        decimal c3 = -1 * a1 * a1;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevE11 = i >= 1 ? e1List[i - 1] : 0;
            decimal prevE12 = i >= 2 ? e1List[i - 2] : 0;
            decimal prevSsf1 = i >= 1 ? ssfList[i - 1] : 0;
            decimal prevSsf2 = i >= 2 ? ssfList[i - 2] : 0;

            decimal ssf = i < 3 ? currentValue : (0.5m * c1 * (currentValue + prevValue)) + (c2 * prevSsf1) + (c3 * prevSsf2);
            ssfList.AddRounded(ssf);

            decimal e1 = i < 3 ? 0 : (c1 * (currentValue - ssf)) + (c2 * prevE11) + (c3 * prevE12);
            e1List.AddRounded(e1);

            decimal prevFilt = filtList.LastOrDefault();
            decimal filt = ssf + e1;
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eaef", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.EhlersAverageErrorFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Laguerre Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersLaguerreFilter(this StockData stockData, decimal alpha = 0.2m)
    {
        List<decimal> filterList = new();
        List<decimal> firList = new();
        List<decimal> l0List = new();
        List<decimal> l1List = new();
        List<decimal> l2List = new();
        List<decimal> l3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevP1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevP2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevP3 = i >= 3 ? inputList[i - 3] : 0;
            decimal prevL0 = i >= 1 ? l0List.LastOrDefault() : currentValue;
            decimal prevL1 = i >= 1 ? l1List.LastOrDefault() : currentValue;
            decimal prevL2 = i >= 1 ? l2List.LastOrDefault() : currentValue;
            decimal prevL3 = i >= 1 ? l3List.LastOrDefault() : currentValue;

            decimal l0 = (alpha * currentValue) + ((1 - alpha) * prevL0);
            l0List.AddRounded(l0);

            decimal l1 = (-1 * (1 - alpha) * l0) + prevL0 + ((1 - alpha) * prevL1);
            l1List.AddRounded(l1);

            decimal l2 = (-1 * (1 - alpha) * l1) + prevL1 + ((1 - alpha) * prevL2);
            l2List.AddRounded(l2);

            decimal l3 = (-1 * (1 - alpha) * l2) + prevL2 + ((1 - alpha) * prevL3);
            l3List.AddRounded(l3);

            decimal prevFilter = filterList.LastOrDefault();
            decimal filter = (l0 + (2 * l1) + (2 * l2) + l3) / 6;
            filterList.AddRounded(filter);

            decimal prevFir = firList.LastOrDefault();
            decimal fir = (currentValue + (2 * prevP1) + (2 * prevP2) + prevP3) / 6;
            firList.AddRounded(fir);

            var signal = GetCompareSignal(filter - fir, prevFilter - prevFir);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Elf", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersLaguerreFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Adaptive Laguerre Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersAdaptiveLaguerreFilter(this StockData stockData, int length1 = 14, int length2 = 5)
    {
        List<decimal> filterList = new();
        List<decimal> l0List = new();
        List<decimal> l1List = new();
        List<decimal> l2List = new();
        List<decimal> l3List = new();
        List<decimal> diffList = new();
        List<decimal> midList = new();
        List<decimal> alphaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevL0 = i >= 1 ? l0List.LastOrDefault() : currentValue;
            decimal prevL1 = i >= 1 ? l1List.LastOrDefault() : currentValue;
            decimal prevL2 = i >= 1 ? l2List.LastOrDefault() : currentValue;
            decimal prevL3 = i >= 1 ? l3List.LastOrDefault() : currentValue;
            decimal prevFilter = i >= 1 ? filterList.LastOrDefault() : currentValue;

            decimal diff = Math.Abs(currentValue - prevFilter);
            diffList.AddRounded(diff);

            var list = diffList.TakeLastExt(length1).ToList();
            decimal highestHigh = list.Max();
            decimal lowestLow = list.Min();

            decimal mid = highestHigh - lowestLow != 0 ? (diff - lowestLow) / (highestHigh - lowestLow) : 0;
            midList.AddRounded(mid);

            decimal prevAlpha = i >= 1 ? alphaList.LastOrDefault() : (decimal)2 / (length1 + 1);
            decimal alpha = mid != 0 ? midList.TakeLastExt(length2).Median() : prevAlpha;
            alphaList.AddRounded(alpha);

            decimal l0 = (alpha * currentValue) + ((1 - alpha) * prevL0);
            l0List.AddRounded(l0);

            decimal l1 = (-1 * (1 - alpha) * l0) + prevL0 + ((1 - alpha) * prevL1);
            l1List.AddRounded(l1);

            decimal l2 = (-1 * (1 - alpha) * l1) + prevL1 + ((1 - alpha) * prevL2);
            l2List.AddRounded(l2);

            decimal l3 = (-1 * (1 - alpha) * l2) + prevL2 + ((1 - alpha) * prevL3);
            l3List.AddRounded(l3);

            decimal filter = (l0 + (2 * l1) + (2 * l2) + l3) / 6;
            filterList.AddRounded(filter);

            var signal = GetCompareSignal(currentValue - filter, prevValue - prevFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ealf", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersAdaptiveLaguerreFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Leading Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="alpha1"></param>
    /// <param name="alpha2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersLeadingIndicator(this StockData stockData, decimal alpha1 = 0.25m, decimal alpha2 = 0.33m)
    {
        List<decimal> leadList = new();
        List<decimal> leadIndicatorList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevLead = leadList.LastOrDefault();
            decimal lead = (2 * currentValue) + ((alpha1 - 2) * prevValue) + ((1 - alpha1) * prevLead);
            leadList.AddRounded(lead);

            decimal prevLeadIndicator = leadIndicatorList.LastOrDefault();
            decimal leadIndicator = (alpha2 * lead) + ((1 - alpha2) * prevLeadIndicator);
            leadIndicatorList.AddRounded(leadIndicator);

            var signal = GetCompareSignal(currentValue - leadIndicator, prevValue - prevLeadIndicator);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eli", leadIndicatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = leadIndicatorList;
        stockData.IndicatorName = IndicatorName.EhlersLeadingIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Optimum Elliptic Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersOptimumEllipticFilter(this StockData stockData)
    {
        List<decimal> oefList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevOef1 = i >= 1 ? oefList[i - 1] : 0;
            decimal prevOef2 = i >= 2 ? oefList[i - 2] : 0;

            decimal oef = (0.13785m * currentValue) + (0.0007m * prevValue1) + (0.13785m * prevValue2) + (1.2103m * prevOef1) - (0.4867m * prevOef2);
            oefList.AddRounded(oef);

            var signal = GetCompareSignal(currentValue - oef, prevValue1 - prevOef1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emoef", oefList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = oefList;
        stockData.IndicatorName = IndicatorName.EhlersOptimumEllipticFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Modified Optimum Elliptic Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersModifiedOptimumEllipticFilter(this StockData stockData)
    {
        List<decimal> moefList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : currentValue;
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : prevValue1;
            decimal prevValue3 = i >= 3 ? inputList[i - 3] : prevValue2;
            decimal prevMoef1 = i >= 1 ? moefList[i - 1] : currentValue;
            decimal prevMoef2 = i >= 2 ? moefList[i - 2] : prevMoef1;

            decimal moef = (0.13785m * ((2 * currentValue) - prevValue1)) + (0.0007m * ((2 * prevValue1) - prevValue2)) +
                (0.13785m * ((2 * prevValue2) - prevValue3)) + (1.2103m * prevMoef1) - (0.4867m * prevMoef2);
            moefList.AddRounded(moef);

            var signal = GetCompareSignal(currentValue - moef, prevValue1 - prevMoef1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emoef", moefList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = moefList;
        stockData.IndicatorName = IndicatorName.EhlersModifiedOptimumEllipticFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersFilter(this StockData stockData, int length1 = 15, int length2 = 5)
    {
        List<decimal> filterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal num = 0, sumC = 0;
            for (int j = 0; j <= length1 - 1; j++)
            {
                decimal currentPrice = i >= j ? inputList[i - j] : 0;
                decimal prevPrice = i >= j + length2 ? inputList[i - (j + length2)] : 0;
                decimal priceDiff = Math.Abs(currentPrice - prevPrice);

                num += priceDiff * currentPrice;
                sumC += priceDiff;
            }

            decimal prevEhlersFilter = filterList.LastOrDefault();
            decimal ehlersFilter = sumC != 0 ? num / sumC : 0;
            filterList.AddRounded(ehlersFilter);

            var signal = GetCompareSignal(currentValue - ehlersFilter, prevValue - prevEhlersFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ef", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Distance Coefficient Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDistanceCoefficientFilter(this StockData stockData, int length = 14)
    {
        List<decimal> filterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal srcSum = 0, coefSum = 0;
            for (int count = 0; count <= length - 1; count++)
            {
                decimal prevCount = i >= count ? inputList[i - count] : 0;

                decimal distance = 0;
                for (int lookBack = 1; lookBack <= length - 1; lookBack++)
                {
                    decimal prevCountLookBack = i >= count + lookBack ? inputList[i - (count + lookBack)] : 0;
                    distance += Pow(prevCount - prevCountLookBack, 2);
                }

                srcSum += distance * prevCount;
                coefSum += distance;
            }

            decimal prevFilter = filterList.LastOrDefault();
            decimal filter = coefSum != 0 ? srcSum / coefSum : 0;
            filterList.AddRounded(filter);

            var signal = GetCompareSignal(currentValue - filter, prevValue - prevFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Edcf", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersDistanceCoefficientFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Finite Impulse Response Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="coef1"></param>
    /// <param name="coef2"></param>
    /// <param name="coef3"></param>
    /// <param name="coef4"></param>
    /// <param name="coef5"></param>
    /// <param name="coef6"></param>
    /// <param name="coef7"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersFiniteImpulseResponseFilter(this StockData stockData, decimal coef1 = 1, decimal coef2 = 3.5m, decimal coef3 = 4.5m,
        decimal coef4 = 3, decimal coef5 = 0.5m, decimal coef6 = -0.5m, decimal coef7 = -1.5m)
    {
        List<decimal> filterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal coefSum = coef1 + coef2 + coef3 + coef4 + coef5 + coef6 + coef7;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevValue3 = i >= 3 ? inputList[i - 3] : 0;
            decimal prevValue4 = i >= 4 ? inputList[i - 4] : 0;
            decimal prevValue5 = i >= 5 ? inputList[i - 5] : 0;
            decimal prevValue6 = i >= 6 ? inputList[i - 6] : 0;

            decimal prevFilter = filterList.LastOrDefault();
            decimal filter = ((coef1 * currentValue) + (coef2 * prevValue1) + (coef3 * prevValue2) + (coef4 * prevValue3) + 
                (coef5 * prevValue4) + (coef6 * prevValue5) + (coef7 * prevValue6)) / coefSum;
            filterList.AddRounded(filter);

            var signal = GetCompareSignal(currentValue - filter, prevValue1 - prevFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Efirf", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersFiniteImpulseResponseFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Infinite Impulse Response Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersInfiniteImpulseResponseFilter(this StockData stockData, int length = 14)
    {
        List<decimal> filterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)2 / (length + 1);
        int lag = MinOrMax((int)Math.Ceiling((1 / alpha) - 1));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= lag ? inputList[i - lag] : 0;
            decimal prevFilter1 = i >= 1 ? filterList[i - 1] : 0;

            decimal filter = (alpha * (currentValue + (currentValue - prevValue))) + ((1 - alpha) * prevFilter1);
            filterList.AddRounded(filter);

            var signal = GetCompareSignal(currentValue - filter, prevValue - prevFilter1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eiirf", filterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filterList;
        stockData.IndicatorName = IndicatorName.EhlersInfiniteImpulseResponseFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Deviation Scaled Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDeviationScaledMovingAverage(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.Ehlers2PoleSuperSmootherFilterV2, int fastLength = 20, int slowLength = 40)
    {
        List<decimal> edsma2PoleList = new();
        List<decimal> zerosList = new();
        List<decimal> avgZerosList = new();
        List<decimal> scaledFilter2PoleList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : 0;

            decimal prevZeros = zerosList.LastOrDefault();
            decimal zeros = currentValue - prevValue2;
            zerosList.AddRounded(zeros);

            decimal avgZeros = (zeros + prevZeros) / 2;
            avgZerosList.AddRounded(avgZeros);
        }

        var ssf2PoleList = GetMovingAverageList(stockData, maType, fastLength, avgZerosList);
        stockData.CustomValuesList = ssf2PoleList;
        var ssf2PoleStdDevList = CalculateStandardDeviationVolatility(stockData, length: slowLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentSsf2Pole = ssf2PoleList[i];
            decimal currentSsf2PoleStdDev = ssf2PoleStdDevList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevScaledFilter2Pole = scaledFilter2PoleList.LastOrDefault();
            decimal scaledFilter2Pole = currentSsf2PoleStdDev != 0 ? currentSsf2Pole / currentSsf2PoleStdDev : prevScaledFilter2Pole;
            scaledFilter2PoleList.AddRounded(scaledFilter2Pole);

            decimal alpha2Pole = MinOrMax(5 * Math.Abs(scaledFilter2Pole) / slowLength, 0.99m, 0.01m);
            decimal prevEdsma2pole = edsma2PoleList.LastOrDefault();
            decimal edsma2Pole = (alpha2Pole * currentValue) + ((1 - alpha2Pole) * prevEdsma2pole);
            edsma2PoleList.AddRounded(edsma2Pole);

            var signal = GetCompareSignal(currentValue - edsma2Pole, prevValue - prevEdsma2pole);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Edsma", edsma2PoleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = edsma2PoleList;
        stockData.IndicatorName = IndicatorName.EhlersDeviationScaledMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Hann Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersHannMovingAverage(this StockData stockData, int length = 20)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevFilt = i >= 1 ? filtList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal filtSum = 0, coefSum = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevV = i >= j - 1 ? inputList[i - (j - 1)] : 0;
                decimal cos = 1 - Cos(2 * Pi * ((decimal)j / (length + 1)));
                filtSum += cos * prevV;
                coefSum += cos;
            }

            decimal filt = coefSum != 0 ? filtSum / coefSum : 0;
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ehma", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.EhlersHannMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Deviation Scaled Super Smoother
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDeviationScaledSuperSmoother(this StockData stockData, MovingAvgType maType = MovingAvgType.EhlersHannMovingAverage,
        int length1 = 12, int length2 = 50)
    {
        List<decimal> momList = new();
        List<decimal> dsssList = new();
        List<decimal> filtPowList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int hannLength = (int)Math.Ceiling(length1 / 1.4m);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal priorValue = i >= length1 ? inputList[i - length1] : 0;

            decimal mom = currentValue - priorValue;
            momList.AddRounded(mom);
        }

        var filtList = GetMovingAverageList(stockData, maType, hannLength, momList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal filt = filtList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevDsss1 = i >= 1 ? dsssList[i - 1] : 0;
            decimal prevDsss2 = i >= 2 ? dsssList[i - 2] : 0;

            decimal filtPow = Pow(filt, 2);
            filtPowList.AddRounded(filtPow);

            decimal filtPowMa = filtPowList.TakeLastExt(length2).Average();
            decimal rms = filtPowMa > 0 ? Sqrt(filtPowMa) : 0;
            decimal scaledFilt = rms != 0 ? filt / rms : 0;
            decimal a1 = Exp(-1.414m * Pi * Math.Abs(scaledFilt) / length1);
            decimal b1 = 2 * a1 * Cos(1.414m * Pi * Math.Abs(scaledFilt) / length1);
            decimal c2 = b1;
            decimal c3 = -a1 * a1;
            decimal c1 = 1 - c2 - c3;

            decimal dsss = (c1 * ((currentValue + prevValue) / 2)) + (c2 * prevDsss1) + (c3 * prevDsss2);
            dsssList.AddRounded(dsss);

            var signal = GetCompareSignal(currentValue - dsss, prevValue - prevDsss1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Edsss", dsssList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = dsssList;
        stockData.IndicatorName = IndicatorName.EhlersDeviationScaledSuperSmoother;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Zero Lag Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersZeroLagExponentialMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 14)
    {
        List<decimal> dList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int lag = MinOrMax((int)Math.Floor((decimal)(length - 1) / 2));

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= lag ? inputList[i - lag] : 0;

            decimal d = currentValue + (currentValue - prevValue);
            dList.AddRounded(d);
        }

        var zemaList = GetMovingAverageList(stockData, maType, length, dList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal zema = zemaList[i];
            decimal prevZema = i >= 1 ? zemaList[i - 1] : 0;

            var signal = GetCompareSignal(currentValue - zema, prevValue - prevZema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ezlema", zemaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zemaList;
        stockData.IndicatorName = IndicatorName.EhlersZeroLagExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Variable Index Dynamic Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersVariableIndexDynamicAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
        int fastLength = 9, int slowLength = 30)
    {
        List<decimal> vidyaList = new();
        List<decimal> longPowList = new();
        List<decimal> shortPowList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var shortAvgList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var longAvgList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal shortAvg = shortAvgList[i];
            decimal longAvg = longAvgList[i];

            decimal shortPow = Pow(currentValue - shortAvg, 2);
            shortPowList.AddRounded(shortPow);

            decimal shortMa = shortPowList.TakeLastExt(fastLength).Average();
            decimal shortRms = shortMa > 0 ? Sqrt(shortMa) : 0;

            decimal longPow = Pow(currentValue - longAvg, 2);
            longPowList.AddRounded(longPow);

            decimal longMa = longPowList.TakeLastExt(slowLength).Average();
            decimal longRms = longMa > 0 ? Sqrt(longMa) : 0;
            decimal kk = longRms != 0 ? MinOrMax(0.2m * shortRms / longRms, 0.99m, 0.01m) : 0;

            decimal prevVidya = vidyaList.LastOrDefault();
            decimal vidya = (kk * currentValue) + ((1 - kk) * prevVidya);
            vidyaList.AddRounded(vidya);

            var signal = GetCompareSignal(currentValue - vidya, prevValue - prevVidya);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Evidya", vidyaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vidyaList;
        stockData.IndicatorName = IndicatorName.EhlersVariableIndexDynamicAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Kaufman Adaptive Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersKaufmanAdaptiveMovingAverage(this StockData stockData, int length = 20)
    {
        List<decimal> kamaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal priorValue = i >= length - 1 ? inputList[i - (length - 1)] : 0;

            decimal deltaSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal cValue = i >= j ? inputList[i - j] : 0;
                decimal pValue = i >= j + 1 ? inputList[i - (j + 1)] : 0;
                deltaSum += Math.Abs(cValue - pValue);
            }

            decimal ef = deltaSum != 0 ? Math.Min(Math.Abs(currentValue - priorValue) / deltaSum, 1) : 0;
            decimal s = Pow((0.6667m * ef) + 0.0645m, 2);

            decimal prevKama = kamaList.LastOrDefault();
            decimal kama = (s * currentValue) + ((1 - s) * prevKama);
            kamaList.AddRounded(kama);

            var signal = GetCompareSignal(currentValue - kama, prevValue - prevKama);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ekama", kamaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = kamaList;
        stockData.IndicatorName = IndicatorName.EhlersKaufmanAdaptiveMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers All Pass Phase Shifter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="qq"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersAllPassPhaseShifter(this StockData stockData, int length = 20, decimal qq = 0.5m)
    {
        List<decimal> phaserList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a2 = qq != 0 && length != 0 ? -2 * Cos(2 * Pi / length) / qq : 0;
        decimal a3 = qq != 0 ? Pow(1 / qq, 2) : 0;
        decimal b2 = length != 0 ? -2 * qq * Cos(2 * Pi / length) : 0;
        decimal b3 = Pow(qq, 2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevPhaser1 = i >= 1 ? phaserList[i - 1] : 0;
            decimal prevPhaser2 = i >= 2 ? phaserList[i - 2] : 0;

            decimal phaser = (b3 * (currentValue + (a2 * prevValue1) + (a3 * prevValue2))) - (b2 * prevPhaser1) - (b3 * prevPhaser2);
            phaserList.AddRounded(phaser);

            var signal = GetCompareSignal(currentValue - phaser, prevValue1 - prevPhaser1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eapps", phaserList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = phaserList;
        stockData.IndicatorName = IndicatorName.EhlersAllPassPhaseShifter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Chebyshev Low Pass Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersChebyshevLowPassFilter(this StockData stockData)
    {
        List<decimal> v1Neg2List = new();
        List<decimal> waveNeg2List = new();
        List<decimal> v1Neg1List = new();
        List<decimal> waveNeg1List = new();
        List<decimal> v10List = new();
        List<decimal> wave0List = new();
        List<decimal> v11List = new();
        List<decimal> wave1List = new();
        List<decimal> v12List = new();
        List<decimal> wave2List = new();
        List<decimal> v13List = new();
        List<decimal> wave3List = new();
        List<decimal> v14List = new();
        List<decimal> wave4List = new();
        List<decimal> v15List = new();
        List<decimal> wave5List = new();
        List<decimal> v16List = new();
        List<decimal> wave6List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue1 = i >= 1 ? inputList[i - 1] : 0;
            decimal prevValue2 = i >= 2 ? inputList[i - 2] : 0;
            decimal prevV1Neg2_1 = i >= 1 ? v1Neg2List[i - 1] : 0;
            decimal prevV1Neg2_2 = i >= 2 ? v1Neg2List[i - 2] : 0;
            decimal prevWaveNeg2_1 = i >= 1 ? waveNeg2List[i - 1] : 0;
            decimal prevWaveNeg2_2 = i >= 2 ? waveNeg2List[i - 2] : 0;
            decimal prevV1Neg1_1 = i >= 1 ? v1Neg1List[i - 1] : 0;
            decimal prevV1Neg1_2 = i >= 2 ? v1Neg1List[i - 2] : 0;
            decimal prevWaveNeg1_1 = i >= 1 ? waveNeg1List[i - 1] : 0;
            decimal prevWaveNeg1_2 = i >= 2 ? waveNeg1List[i - 2] : 0;
            decimal prevV10_1 = i >= 1 ? v10List[i - 1] : 0;
            decimal prevV10_2 = i >= 2 ? v10List[i - 2] : 0;
            decimal prevWave0_1 = i >= 1 ? wave0List[i - 1] : 0;
            decimal prevWave0_2 = i >= 2 ? wave0List[i - 2] : 0;
            decimal prevV11_1 = i >= 1 ? v11List[i - 1] : 0;
            decimal prevV11_2 = i >= 2 ? v11List[i - 2] : 0;
            decimal prevWave1_1 = i >= 1 ? wave1List[i - 1] : 0;
            decimal prevWave1_2 = i >= 2 ? wave1List[i - 2] : 0;
            decimal prevV12_1 = i >= 1 ? v12List[i - 1] : 0;
            decimal prevV12_2 = i >= 2 ? v12List[i - 2] : 0;
            decimal prevWave2_1 = i >= 1 ? wave2List[i - 1] : 0;
            decimal prevWave2_2 = i >= 2 ? wave2List[i - 2] : 0;
            decimal prevV13_1 = i >= 1 ? v13List[i - 1] : 0;
            decimal prevV13_2 = i >= 2 ? v13List[i - 2] : 0;
            decimal prevWave3_1 = i >= 1 ? wave3List[i - 1] : 0;
            decimal prevWave3_2 = i >= 2 ? wave3List[i - 2] : 0;
            decimal prevV14_1 = i >= 1 ? v14List[i - 1] : 0;
            decimal prevV14_2 = i >= 2 ? v14List[i - 2] : 0;
            decimal prevWave4_1 = i >= 1 ? wave4List[i - 1] : 0;
            decimal prevWave4_2 = i >= 2 ? wave4List[i - 2] : 0;
            decimal prevV15_1 = i >= 1 ? v15List[i - 1] : 0;
            decimal prevV15_2 = i >= 2 ? v15List[i - 2] : 0;
            decimal prevWave5_1 = i >= 1 ? wave5List[i - 1] : 0;
            decimal prevWave5_2 = i >= 2 ? wave5List[i - 2] : 0;
            decimal prevV16_1 = i >= 1 ? v16List[i - 1] : 0;
            decimal prevV16_2 = i >= 2 ? v16List[i - 2] : 0;
            decimal prevWave6_1 = i >= 1 ? wave6List[i - 1] : 0;
            decimal prevWave6_2 = i >= 2 ? wave6List[i - 2] : 0;

            decimal v1Neg2 = (0.080778m * (currentValue + (1.907m * prevValue1) + prevValue2)) + (0.293m * prevV1Neg2_1) - (0.063m * prevV1Neg2_2);
            v1Neg2List.AddRounded(v1Neg2);

            decimal waveNeg2 = v1Neg2 + (0.513m * prevV1Neg2_1) + prevV1Neg2_2 + (0.451m * prevWaveNeg2_1) - (0.481m * prevWaveNeg2_2);
            waveNeg2List.AddRounded(waveNeg2);

            decimal v1Neg1 = (0.021394m * (currentValue + (1.777m * prevValue1) + prevValue2)) + (0.731m * prevV1Neg1_1) - (0.166m * prevV1Neg1_2);
            v1Neg1List.AddRounded(v1Neg1);

            decimal waveNeg1 = v1Neg1 + (0.977m * prevV1Neg1_1) + prevV1Neg1_2 + (1.008m * prevWaveNeg1_1) - (0.561m * prevWaveNeg1_2);
            waveNeg1List.AddRounded(waveNeg1);

            decimal v10 = (0.0095822m * (currentValue + (1.572m * prevValue1) + prevValue2)) + (1.026m * prevV10_1) - (0.282m * prevV10_2);
            v10List.AddRounded(v10);

            decimal wave0 = v10 + (0.356m * prevV10_1) + prevV10_2 + (1.329m * prevWave0_1) - (0.644m * prevWave0_2);
            wave0List.AddRounded(wave0);

            decimal v11 = (0.00461m * (currentValue + (1.192m * prevValue1) + prevValue2)) + (1.281m * prevV11_1) - (0.426m * prevV11_2);
            v11List.AddRounded(v11);

            decimal wave1 = v11 - (0.384m * prevV11_1) + prevV11_2 + (1.565m * prevWave1_1) - (0.729m * prevWave1_2);
            wave1List.AddRounded(wave1);

            decimal v12 = (0.0026947m * (currentValue + (0.681m * prevValue1) + prevValue2)) + (1.46m * prevV12_1) - (0.543m * prevV12_2);
            v12List.AddRounded(v12);

            decimal wave2 = v12 - (0.966m * prevV12_1) + prevV12_2 + (1.703m * prevWave2_1) - (0.793m * prevWave2_2);
            wave2List.AddRounded(wave2);

            decimal v13 = (0.0017362m * (currentValue + (0.012m * prevValue1) + prevValue2)) + (1.606m * prevV13_1) - (0.65m * prevV13_2);
            v13List.AddRounded(v13);

            decimal wave3 = v13 - (1.408m * prevV13_1) + prevV13_2 + (1.801m * prevWave3_1) - (0.848m * prevWave3_2);
            wave3List.AddRounded(wave3);

            decimal v14 = (0.0013738m * (currentValue - (0.669m * prevValue1) + prevValue2)) + (1.716m * prevV14_1) - (0.74m * prevV14_2);
            v14List.AddRounded(v14);

            decimal wave4 = v14 - (1.685m * prevV14_1) + prevV14_2 + (1.866m * prevWave4_1) - (0.89m * prevWave4_2);
            wave4List.AddRounded(wave4);

            decimal v15 = (0.0010794m * (currentValue - (1.226m * prevValue1) + prevValue2)) + (1.8m * prevV15_1) - (0.811m * prevV15_2);
            v15List.AddRounded(v15);

            decimal wave5 = v15 - (1.842m * prevV15_1) + prevV15_2 + (1.91m * prevWave5_1) - (0.922m * prevWave5_2);
            wave5List.AddRounded(wave5);

            decimal v16 = (0.001705m * (currentValue - (1.659m * prevValue1) + prevValue2)) + (1.873m * prevV16_1) - (0.878m * prevV16_2);
            v16List.AddRounded(v16);

            decimal wave6 = v16 - (1.957m * prevV16_1) + prevV16_2 + (1.946m * prevWave6_1) - (0.951m * prevWave6_2);
            wave6List.AddRounded(wave6);

            var signal = GetCompareSignal(currentValue - waveNeg2, prevValue1 - prevWaveNeg2_1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eclpf-2", waveNeg2List },
            { "Eclpf-1", waveNeg1List },
            { "Eclpf0", wave0List },
            { "Eclpf1", wave1List },
            { "Eclpf2", wave2List },
            { "Eclpf3", wave3List },
            { "Eclpf4", wave4List },
            { "Eclpf5", wave5List },
            { "Eclpf6", wave6List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = waveNeg2List;
        stockData.IndicatorName = IndicatorName.EhlersChebyshevLowPassFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Better Exponential Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersBetterExponentialMovingAverage(this StockData stockData, int length = 20)
    {
        List<decimal> emaList = new();
        List<decimal> bEmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal val = length != 0 ? Cos(2 * Pi / length) + Sin(2 * Pi / length) : 0;
        decimal alpha = val != 0 ? MinOrMax((val - 1) / val, 0.99m, 0.01m) : 0.01m;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevEma1 = i >= 1 ? emaList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal ema = (alpha * currentValue) + ((1 - alpha) * prevEma1);
            emaList.AddRounded(ema);

            decimal prevBEma = bEmaList.LastOrDefault();
            decimal bEma = (alpha * ((currentValue + prevValue) / 2)) + ((1 - alpha) * prevEma1);
            bEmaList.AddRounded(bEma);

            var signal = GetCompareSignal(currentValue - bEma, prevValue - prevBEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ebema", bEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = bEmaList;
        stockData.IndicatorName = IndicatorName.EhlersBetterExponentialMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Hamming Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="pedestal"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersHammingMovingAverage(this StockData stockData, int length = 20, decimal pedestal = 3)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevFilt = i >= 1 ? filtList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal filtSum = 0, coefSum = 0;
            for (int j = 0; j < length; j++)
            {
                decimal prevV = i >= j ? inputList[i - j] : 0;
                decimal sine = Sin(pedestal + ((Pi - (2 * pedestal)) * ((decimal)j / (length - 1))));
                filtSum += sine * prevV;
                coefSum += sine;
            }

            decimal filt = coefSum != 0 ? filtSum / coefSum : 0;
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ehma", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.EhlersHammingMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Triangle Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersTriangleMovingAverage(this StockData stockData, int length = 20)
    {
        List<decimal> filtList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal l2 = (decimal)length / 2;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevFilt = i >= 1 ? filtList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal filtSum = 0, coefSum = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevV = i >= j - 1 ? inputList[i - (j - 1)] : 0;
                decimal c = j < l2 ? j : j > l2 ? length + 1 - j : l2;
                filtSum += c * prevV;
                coefSum += c;
            }

            decimal filt = coefSum != 0 ? filtSum / coefSum : 0;
            filtList.AddRounded(filt);

            var signal = GetCompareSignal(currentValue - filt, prevValue - prevFilt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Etma", filtList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = filtList;
        stockData.IndicatorName = IndicatorName.EhlersTriangleMovingAverage;

        return stockData;
    }
}
