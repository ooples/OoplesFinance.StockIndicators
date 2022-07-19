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
    /// Calculates the Price Channel
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="pct">The PCT.</param>
    /// <returns></returns>
    public static StockData CalculatePriceChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 21, 
        decimal pct = 0.06m)
    {
        List<decimal> upperPriceChannelList = new();
        List<decimal> lowerPriceChannelList = new();
        List<decimal> midPriceChannelList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentEma = emaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal upperPriceChannel = currentEma * (1 + pct);
            upperPriceChannelList.AddRounded(upperPriceChannel);

            decimal lowerPriceChannel = currentEma * (1 - pct);
            lowerPriceChannelList.AddRounded(lowerPriceChannel);

            decimal prevMidPriceChannel = midPriceChannelList.LastOrDefault();
            decimal midPriceChannel = (upperPriceChannel + lowerPriceChannel) / 2;
            midPriceChannelList.AddRounded(midPriceChannel);

            var signal = GetCompareSignal(currentValue - midPriceChannel, prevValue - prevMidPriceChannel);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperChannel", upperPriceChannelList },
            { "LowerChannel", lowerPriceChannelList },
            { "MiddleChannel", midPriceChannelList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.PriceChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the donchian channels.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateDonchianChannels(this StockData stockData, int length = 20)
    {
        List<decimal> upperChannelList = new();
        List<decimal> lowerChannelList = new();
        List<decimal> middleChannelList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal upperChannel = highestList[i];
            upperChannelList.AddRounded(upperChannel);

            decimal lowerChannel = lowestList[i];
            lowerChannelList.AddRounded(lowerChannel);

            decimal prevMiddleChannel = middleChannelList.LastOrDefault();
            decimal middleChannel = (upperChannel + lowerChannel) / 2;
            middleChannelList.AddRounded(middleChannel);

            var signal = GetCompareSignal(currentValue - middleChannel, prevValue - prevMiddleChannel);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperChannel", upperChannelList },
            { "LowerChannel", lowerChannelList },
            { "MiddleChannel", middleChannelList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.DonchianChannels;

        return stockData;
    }

    /// <summary>
    /// Calculates the standard deviation channel.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="length">The length.</param>
    /// <param name="stdDevMult">The standard dev mult.</param>
    /// <returns></returns>
    public static StockData CalculateStandardDeviationChannel(this StockData stockData, int length = 40, decimal stdDevMult = 2)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDeviationList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;
        var regressionList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = regressionList[i];
            decimal currentValue = inputList[i];
            decimal currentStdDev = stdDeviationList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? regressionList[i - 1] : 0;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (currentStdDev * stdDevMult);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (currentStdDev * stdDevMult);
            lowerBandList.AddRounded(lowerBand);

            Signal signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, upperBand, prevUpperBand, lowerBand, prevLowerBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", regressionList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.StandardDeviationChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the stoller average range channels.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="atrMult">The atr mult.</param>
    /// <returns></returns>
    public static StockData CalculateStollerAverageRangeChannels(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14, decimal atrMult = 2)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = smaList[i];
            decimal currentValue = inputList[i];
            decimal currentAtr = atrList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? smaList[i - 1] : 0;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (currentAtr * atrMult);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (currentAtr * atrMult);
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
        stockData.IndicatorName = IndicatorName.StollerAverageRangeChannels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Channel
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<decimal> midChannelList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var highMaList = GetMovingAverageList(stockData, maType, length, highList);
        var lowMaList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal upperChannel = highMaList[i];
            decimal lowerChannel = lowMaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevMidChannel = midChannelList.LastOrDefault();
            decimal midChannel = (upperChannel + lowerChannel) / 2;
            midChannelList.AddRounded(midChannel);

            var signal = GetCompareSignal(currentValue - midChannel, prevValue - prevMidChannel);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", highMaList },
            { "MiddleBand", midChannelList },
            { "LowerBand", lowMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.MovingAverageChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Envelope
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="mult">The mult.</param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageEnvelope(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 20, decimal mult = 0.025m)
    {
        List<decimal> upperEnvelopeList = new();
        List<decimal> lowerEnvelopeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentSma20 = smaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevSma20 = i >= 1 ? smaList[i - 1] : 0;
            decimal factor = currentSma20 * mult;

            decimal upperEnvelope = currentSma20 + factor;
            upperEnvelopeList.AddRounded(upperEnvelope);

            decimal lowerEnvelope = currentSma20 - factor;
            lowerEnvelopeList.AddRounded(lowerEnvelope);

            var signal = GetCompareSignal(currentValue - currentSma20, prevValue - prevSma20);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperEnvelopeList },
            { "MiddleBand", smaList },
            { "LowerBand", lowerEnvelopeList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.MovingAverageEnvelope;

        return stockData;
    }

    /// <summary>
    /// Calculates the fractal chaos bands.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateFractalChaosBands(this StockData stockData)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevHigh1 = i >= 1 ? highList[i - 1] : 0;
            decimal prevHigh2 = i >= 2 ? highList[i - 2] : 0;
            decimal prevHigh3 = i >= 3 ? highList[i - 3] : 0;
            decimal prevLow1 = i >= 1 ? lowList[i - 1] : 0;
            decimal prevLow2 = i >= 2 ? lowList[i - 2] : 0;
            decimal prevLow3 = i >= 3 ? lowList[i - 3] : 0;
            decimal currentClose = inputList[i];
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal oklUpper = prevHigh1 < prevHigh2 ? 1 : 0;
            decimal okrUpper = prevHigh3 < prevHigh2 ? 1 : 0;
            decimal oklLower = prevLow1 > prevLow2 ? 1 : 0;
            decimal okrLower = prevLow3 > prevLow2 ? 1 : 0;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = oklUpper == 1 && okrUpper == 1 ? prevHigh2 : prevUpperBand;
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = oklLower == 1 && okrLower == 1 ? prevLow2 : prevLowerBand;
            lowerBandList.AddRounded(lowerBand);

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (upperBand + lowerBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetBollingerBandsSignal(currentClose - middleBand, prevClose - prevMiddleBand, currentClose, prevClose, upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.FractalChaosBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Average True Range Channel
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <param name="mult">The mult.</param>
    /// <returns></returns>
    public static StockData CalculateAverageTrueRangeChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14, decimal mult = 2.5m)
    {
        List<decimal> innerTopAtrChannelList = new();
        List<decimal> innerBottomAtrChannelList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal atr = atrList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma = smaList[i];
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;

            decimal prevTopInner = innerTopAtrChannelList.LastOrDefault();
            decimal topInner = Math.Round(currentValue + (atr * mult));
            innerTopAtrChannelList.AddRounded(topInner);

            decimal prevBottomInner = innerBottomAtrChannelList.LastOrDefault();
            decimal bottomInner = Math.Round(currentValue - (atr * mult));
            innerBottomAtrChannelList.AddRounded(bottomInner);

            var signal = GetBollingerBandsSignal(currentValue - sma, prevValue - prevSma, currentValue, prevValue, topInner,
                prevTopInner, bottomInner, prevBottomInner);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", innerTopAtrChannelList },
            { "MiddleBand", smaList },
            { "LowerBand", innerBottomAtrChannelList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.AverageTrueRangeChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ultimate Moving Average Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateUltimateMovingAverageBands(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int minLength = 5, int maxLength = 50, decimal stdDevMult = 2)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var umaList = CalculateUltimateMovingAverage(stockData, maType, minLength, maxLength, 1).CustomValuesList;
        var stdevList = CalculateStandardDeviationVolatility(stockData, maType, minLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevVal = i >= 1 ? inputList[i - 1] : 0;
            decimal uma = umaList[i];
            decimal prevUma = i >= 1 ? umaList[i - 1] : 0;
            decimal stdev = stdevList[i];

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = uma + (stdDevMult * stdev);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = uma - (stdDevMult * stdev);
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - uma, prevVal - prevUma, currentValue, prevVal, upperBand, prevUpperBand,
                lowerBand, prevLowerBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", umaList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.UltimateMovingAverageBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Uni Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="ubFac"></param>
    /// <param name="lbFac"></param>
    /// <param name="type1"></param>
    /// <returns></returns>
    public static StockData CalculateUniChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 10, decimal ubFac = 0.02m, decimal lbFac = 0.02m, bool type1 = false)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentSma = smaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;

            decimal prevUb = upperBandList.LastOrDefault();
            decimal ub = type1 ? currentSma + ubFac : currentSma + (currentSma * ubFac);
            upperBandList.AddRounded(ub);

            decimal prevLb = lowerBandList.LastOrDefault();
            decimal lb = type1 ? currentSma - lbFac : currentSma - (currentSma * lbFac);
            lowerBandList.AddRounded(lb);

            var signal = GetBollingerBandsSignal(currentValue - currentSma, prevValue - prevSma, currentValue, prevValue, ub, prevUb, lb, prevLb);
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
        stockData.IndicatorName = IndicatorName.UniChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Wilson Relative Price Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <param name="overbought"></param>
    /// <param name="oversold"></param>
    /// <param name="upperNeutralZone"></param>
    /// <param name="lowerNeutralZone"></param>
    /// <returns></returns>
    public static StockData CalculateWilsonRelativePriceChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 34, int smoothLength = 1, decimal overbought = 70, decimal oversold = 30, decimal upperNeutralZone = 55,
        decimal lowerNeutralZone = 45)
    {
        List<decimal> rsiOverboughtList = new();
        List<decimal> rsiOversoldList = new();
        List<decimal> rsiUpperNeutralZoneList = new();
        List<decimal> rsiLowerNeutralZoneList = new();
        List<decimal> s1List = new();
        List<decimal> s2List = new();
        List<decimal> u1List = new();
        List<decimal> u2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var rsiList = CalculateRelativeStrengthIndex(stockData, maType, length, smoothLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rsi = rsiList[i];

            decimal rsiOverbought = rsi - overbought;
            rsiOverboughtList.AddRounded(rsiOverbought);

            decimal rsiOversold = rsi - oversold;
            rsiOversoldList.AddRounded(rsiOversold);

            decimal rsiUpperNeutralZone = rsi - upperNeutralZone;
            rsiUpperNeutralZoneList.AddRounded(rsiUpperNeutralZone);

            decimal rsiLowerNeutralZone = rsi - lowerNeutralZone;
            rsiLowerNeutralZoneList.AddRounded(rsiLowerNeutralZone);
        }

        var obList = GetMovingAverageList(stockData, maType, smoothLength, rsiOverboughtList);
        var osList = GetMovingAverageList(stockData, maType, smoothLength, rsiOversoldList);
        var nzuList = GetMovingAverageList(stockData, maType, smoothLength, rsiUpperNeutralZoneList);
        var nzlList = GetMovingAverageList(stockData, maType, smoothLength, rsiLowerNeutralZoneList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal ob = obList[i];
            decimal os = osList[i];
            decimal nzu = nzuList[i];
            decimal nzl = nzlList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevS1 = s1List.LastOrDefault();
            decimal s1 = currentValue - (currentValue * os / 100);
            s1List.AddRounded(s1);

            decimal prevU1 = u1List.LastOrDefault();
            decimal u1 = currentValue - (currentValue * ob / 100);
            u1List.AddRounded(u1);

            decimal prevU2 = u2List.LastOrDefault();
            decimal u2 = currentValue - (currentValue * nzu / 100);
            u2List.AddRounded(u2);

            decimal prevS2 = s2List.LastOrDefault();
            decimal s2 = currentValue - (currentValue * nzl / 100);
            s2List.AddRounded(s2);

            var signal = GetBullishBearishSignal(currentValue - Math.Min(u1, u2), prevValue - Math.Min(prevU1, prevU2),
                currentValue - Math.Max(s1, s2), prevValue - Math.Max(prevS1, prevS2));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "S1", s1List },
            { "S2", s2List },
            { "U1", u1List },
            { "U2", u2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.WilsonRelativePriceChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Linear Channels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateLinearChannels(this StockData stockData, int length = 14, decimal mult = 50)
    {
        List<decimal> aList = new();
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal s = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevA2 = i >= 2 ? aList[i - 2] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal x = currentValue + ((prevA - prevA2) * mult);

            decimal a = x > prevA + s ? prevA + s : x < prevA - s ? prevA - s : prevA;
            aList.AddRounded(a);

            decimal up = a + (Math.Abs(a - prevA) * mult);
            decimal dn = a - (Math.Abs(a - prevA) * mult);

            decimal prevUpper = upperList.LastOrDefault();
            decimal upper = up == a ? prevUpper : up;
            upperList.AddRounded(upper);

            decimal prevLower = lowerList.LastOrDefault();
            decimal lower = dn == a ? prevLower : dn;
            lowerList.AddRounded(lower);

            var signal = GetBollingerBandsSignal(currentValue - a, prevValue - prevA, currentValue, prevValue, upper, prevUpper, lower, prevLower);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.LinearChannels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Interquartile Range Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateInterquartileRangeBands(this StockData stockData, int length = 14, decimal mult = 1.5m)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var trimeanList = CalculateTrimean(stockData, length);
        var q1List = trimeanList.OutputValues["Q1"];
        var q3List = trimeanList.OutputValues["Q3"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal q1 = q1List[i];
            decimal q3 = q3List[i];
            decimal iqr = q3 - q1;

            decimal upperBand = q3 + (mult * iqr);
            upperBandList.AddRounded(upperBand);

            decimal lowerBand = q1 - (mult * iqr);
            lowerBandList.AddRounded(lowerBand);

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (upperBand + lowerBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetCompareSignal(currentValue - middleBand, prevValue - prevMiddleBand);
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
        stockData.IndicatorName = IndicatorName.InterquartileRangeBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Narrow Sideways Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="stdDevMult"></param>
    /// <returns></returns>
    public static StockData CalculateNarrowSidewaysChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 14, decimal stdDevMult = 3)
    {
        var narrowChannelList = CalculateBollingerBands(stockData, maType, length, stdDevMult);
        var upperBandList = narrowChannelList.OutputValues["UpperBand"];
        var middleBandList = narrowChannelList.OutputValues["MiddleBand"];
        var lowerBandList = narrowChannelList.OutputValues["LowerBand"];
        var signalsList = narrowChannelList.SignalsList;

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", middleBandList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.NarrowSidewaysChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the G Channels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateGChannels(this StockData stockData, int length = 100)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> midList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevA = aList.LastOrDefault();
            decimal prevB = bList.LastOrDefault();
            decimal factor = length != 0 ? (prevA - prevB) / length : 0;

            decimal a = Math.Max(currentValue, prevA) - factor;
            aList.AddRounded(a);

            decimal b = Math.Min(currentValue, prevB) + factor;
            bList.AddRounded(b);

            decimal prevMid = midList.LastOrDefault();
            decimal mid = (a + b) / 2;
            midList.AddRounded(mid);

            var signal = GetCompareSignal(currentValue - mid, prevValue - prevMid);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aList },
            { "MiddleBand", midList },
            { "LowerBand", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.GChannels;

        return stockData;
    }

    /// <summary>
    /// Calculates the High Low Moving Average
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateHighLowMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 14)
    {
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var upperBandList = GetMovingAverageList(stockData, maType, length, highestList);
        var lowerBandList = GetMovingAverageList(stockData, maType, length, lowestList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal upperBand = upperBandList[i];
            decimal lowerBand = lowerBandList[i];

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (upperBand + lowerBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetCompareSignal(currentValue - middleBand, prevValue - prevMiddleBand);
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
        stockData.IndicatorName = IndicatorName.HighLowMovingAverage;

        return stockData;
    }

    /// <summary>
    /// Calculates the High Low Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="pctShift"></param>
    /// <returns></returns>
    public static StockData CalculateHighLowBands(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14,
        decimal pctShift = 1)
    {
        List<decimal> highBandList = new();
        List<decimal> lowBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var tmaList1 = GetMovingAverageList(stockData, maType, length, inputList);
        var tmaList2 = GetMovingAverageList(stockData, maType, length, tmaList1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal tma = tmaList2[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevTma = i >= 1 ? tmaList2[i - 1] : 0;

            decimal prevHighBand = highBandList.LastOrDefault();
            decimal highBand = tma + (tma * pctShift / 100);
            highBandList.AddRounded(highBand);

            decimal prevLowBand = lowBandList.LastOrDefault();
            decimal lowBand = tma - (tma * pctShift / 100);
            lowBandList.AddRounded(lowBand);

            var signal = GetBollingerBandsSignal(currentValue - tma, prevValue - prevTma, currentValue, prevValue, highBand, prevHighBand,
                lowBand, prevLowBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", highBandList },
            { "MiddleBand", tmaList2 },
            { "LowerBand", lowBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.HighLowBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hurst Cycle Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="fastMult"></param>
    /// <param name="slowMult"></param>
    /// <returns></returns>
    public static StockData CalculateHurstCycleChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod,
        int fastLength = 10, int slowLength = 30, decimal fastMult = 1, decimal slowMult = 3)
    {
        List<decimal> sctList = new();
        List<decimal> scbList = new();
        List<decimal> mctList = new();
        List<decimal> mcbList = new();
        List<decimal> scmmList = new();
        List<decimal> mcmmList = new();
        List<decimal> omedList = new();
        List<decimal> oshortList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int scl = MinOrMax((int)Math.Ceiling((decimal)fastLength / 2));
        int mcl = MinOrMax((int)Math.Ceiling((decimal)slowLength / 2));
        int scl_2 = MinOrMax((int)Math.Ceiling((decimal)scl / 2));
        int mcl_2 = MinOrMax((int)Math.Ceiling((decimal)mcl / 2));

        var sclAtrList = CalculateAverageTrueRange(stockData, maType, scl).CustomValuesList;
        var mclAtrList = CalculateAverageTrueRange(stockData, maType, mcl).CustomValuesList;
        var sclRmaList = GetMovingAverageList(stockData, maType, scl, inputList);
        var mclRmaList = GetMovingAverageList(stockData, maType, mcl, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal sclAtr = sclAtrList[i];
            decimal mclAtr = mclAtrList[i];
            decimal prevSclRma = i >= scl_2 ? sclRmaList[i - scl_2] : currentValue;
            decimal prevMclRma = i >= mcl_2 ? mclRmaList[i - mcl_2] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal scm_off = fastMult * sclAtr;
            decimal mcm_off = slowMult * mclAtr;

            decimal prevSct = sctList.LastOrDefault();
            decimal sct = prevSclRma + scm_off;
            sctList.AddRounded(sct);

            decimal prevScb = scbList.LastOrDefault();
            decimal scb = prevSclRma - scm_off;
            scbList.AddRounded(scb);

            decimal mct = prevMclRma + mcm_off;
            mctList.AddRounded(mct);

            decimal mcb = prevMclRma - mcm_off;
            mcbList.AddRounded(mcb);

            decimal scmm = (sct + scb) / 2;
            scmmList.AddRounded(scmm);

            decimal mcmm = (mct + mcb) / 2;
            mcmmList.AddRounded(mcmm);

            decimal omed = mct - mcb != 0 ? (scmm - mcb) / (mct - mcb) : 0;
            omedList.AddRounded(omed);

            decimal oshort = mct - mcb != 0 ? (currentValue - mcb) / (mct - mcb) : 0;
            oshortList.AddRounded(oshort);

            var signal = GetBullishBearishSignal(currentValue - sct, prevValue - prevSct, currentValue - scb, prevValue - prevScb);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastUpperBand", sctList },
            { "SlowUpperBand", mctList },
            { "FastMiddleBand", scmmList },
            { "SlowMiddleBand", mcmmList },
            { "FastLowerBand", scbList },
            { "SlowLowerBand", mcbList },
            { "OMed", omedList },
            { "OShort", oshortList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.HurstCycleChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hurst Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="innerMult"></param>
    /// <param name="outerMult"></param>
    /// <param name="extremeMult"></param>
    /// <returns></returns>
    public static StockData CalculateHurstBands(this StockData stockData, int length = 10, decimal innerMult = 1.6m, decimal outerMult = 2.6m,
        decimal extremeMult = 4.2m)
    {
        List<decimal> cmaList = new();
        List<decimal> upperExtremeBandList = new();
        List<decimal> lowerExtremeBandList = new();
        List<decimal> upperOuterBandList = new();
        List<decimal> lowerOuterBandList = new();
        List<decimal> upperInnerBandList = new();
        List<decimal> lowerInnerBandList = new();
        List<decimal> dPriceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int displacement = MinOrMax((int)Math.Ceiling((decimal)length / 2) + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevCma1 = i >= 1 ? cmaList[i - 1] : 0;
            decimal prevCma2 = i >= 2 ? cmaList[i - 2] : 0;

            decimal dPrice = i >= displacement ? inputList[i - displacement] : 0;
            dPriceList.AddRounded(dPrice);

            decimal cma = dPrice == 0 ? prevCma1 + (prevCma1 - prevCma2) : dPriceList.TakeLastExt(length).Average();
            cmaList.AddRounded(cma);

            decimal extremeBand = cma * extremeMult / 100;
            decimal outerBand = cma * outerMult / 100;
            decimal innerBand = cma * innerMult / 100;

            decimal upperExtremeBand = cma + extremeBand;
            upperExtremeBandList.AddRounded(upperExtremeBand);

            decimal lowerExtremeBand = cma - extremeBand;
            lowerExtremeBandList.AddRounded(lowerExtremeBand);

            decimal upperInnerBand = cma + innerBand;
            upperInnerBandList.AddRounded(upperInnerBand);

            decimal lowerInnerBand = cma - innerBand;
            lowerInnerBandList.AddRounded(lowerInnerBand);

            decimal upperOuterBand = cma + outerBand;
            upperOuterBandList.AddRounded(upperOuterBand);

            decimal lowerOuterBand = cma - outerBand;
            lowerOuterBandList.AddRounded(lowerOuterBand);

            var signal = GetCompareSignal(currentValue - cma, prevValue - prevCma1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperExtremeBand", upperExtremeBandList },
            { "UpperOuterBand", upperOuterBandList },
            { "UpperInnerBand", upperInnerBandList },
            { "MiddleBand", cmaList },
            { "LowerExtremeBand", lowerExtremeBandList },
            { "LowerOuterBand", lowerOuterBandList },
            { "LowerInnerBand", lowerInnerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.HurstBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Hirashima Sugita RS
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateHirashimaSugitaRS(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int length = 1000)
    {
        List<decimal> d1List = new();
        List<decimal> absD1List = new();
        List<decimal> d2List = new();
        List<decimal> basisList = new();
        List<decimal> upper1List = new();
        List<decimal> lower1List = new();
        List<decimal> upper2List = new();
        List<decimal> lower2List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal ema = emaList[i];

            decimal d1 = currentValue - ema;
            d1List.AddRounded(d1);

            decimal absD1 = Math.Abs(d1);
            absD1List.AddRounded(absD1);
        }

        var wmaList = GetMovingAverageList(stockData, maType, length, absD1List);
        stockData.CustomValuesList = d1List;
        var s1List = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema = emaList[i];
            decimal s1 = s1List[i];
            decimal currentValue = inputList[i];
            decimal x = ema + s1;

            decimal d2 = currentValue - x;
            d2List.AddRounded(d2);
        }

        stockData.CustomValuesList = d2List;
        var s2List = CalculateLinearRegression(stockData, length).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal ema = emaList[i];
            decimal s1 = s1List[i];
            decimal s2 = s2List[i];
            decimal prevS2 = i >= 1 ? s2List[i - 1] : 0;
            decimal wma = wmaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevBasis = basisList.LastOrDefault();
            decimal basis = ema + s1 + (s2 - prevS2);
            basisList.AddRounded(basis);

            decimal upper1 = basis + wma;
            upper1List.AddRounded(upper1);

            decimal lower1 = basis - wma;
            lower1List.AddRounded(lower1);

            decimal upper2 = upper1 + wma;
            upper2List.AddRounded(upper2);

            decimal lower2 = lower1 - wma;
            lower2List.AddRounded(lower2);

            var signal = GetCompareSignal(currentValue - basis, prevValue - prevBasis);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand1", upper1List },
            { "UpperBand2", upper2List },
            { "MiddleBand", basisList },
            { "LowerBand1", lower1List },
            { "LowerBand2", lower2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.HirashimaSugitaRS;

        return stockData;
    }

    /// <summary>
    /// Calculates the Flagging Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateFlaggingBands(this StockData stockData, int length = 14)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> tavgList = new();
        List<decimal> tsList = new();
        List<decimal> tosList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var stdDevList = CalculateStandardDeviationVolatility(stockData, length: length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal stdDev = stdDevList[i];
            decimal prevA1 = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevB1 = i >= 1 ? bList[i - 1] : currentValue;
            decimal prevA2 = i >= 2 ? aList[i - 2] : currentValue;
            decimal prevB2 = i >= 2 ? bList[i - 2] : currentValue;
            decimal prevA3 = i >= 3 ? aList[i - 3] : currentValue;
            decimal prevB3 = i >= 3 ? bList[i - 3] : currentValue;
            decimal l = stdDev != 0 ? (decimal)1 / length * stdDev : 0;

            decimal a = currentValue > prevA1 ? prevA1 + (currentValue - prevA1) : prevA2 == prevA3 ? prevA2 - l : prevA2;
            aList.AddRounded(a);

            decimal b = currentValue < prevB1 ? prevB1 + (currentValue - prevB1) : prevB2 == prevB3 ? prevB2 + l : prevB2;
            bList.AddRounded(b);

            decimal prevTos = tosList.LastOrDefault();
            decimal tos = currentValue > prevA2 ? 1 : currentValue < prevB2 ? 0 : prevTos;
            tosList.AddRounded(tos);

            decimal prevTavg = tavgList.LastOrDefault();
            decimal avg = (a + b) / 2;
            decimal tavg = tos == 1 ? (a + avg) / 2 : (b + avg) / 2;
            tavgList.AddRounded(tavg);

            decimal ts = (tos * b) + ((1 - tos) * a);
            tsList.AddRounded(ts);

            var signal = GetCompareSignal(currentValue - tavg, prevValue - prevTavg);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aList },
            { "MiddleBand", tavgList },
            { "LowerBand", bList },
            { "TrailingStop", tsList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.FlaggingBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kirshenbaum Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="stdDevFactor"></param>
    /// <returns></returns>
    public static StockData CalculateKirshenbaumBands(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 30, int length2 = 20, decimal stdDevFactor = 1)
    {
        List<decimal> topList = new();
        List<decimal> bottomList = new();
        List<decimal> tempInputList = new();
        List<decimal> tempLinRegList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var linRegList = CalculateLinearRegression(stockData, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentEma = emaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal currentValue = inputList[i];
            tempInputList.AddRounded(currentValue);

            decimal currentLinReg = linRegList[i];
            tempLinRegList.AddRounded(currentLinReg);

            var stdError = GoodnessOfFit.PopulationStandardError(tempLinRegList.TakeLastExt(length2).Select(x => (double)x),
                tempInputList.TakeLastExt(length2).Select(x => (double)x));
            stdError = IsValueNullOrInfinity(stdError) ? 0 : stdError;
            decimal ratio = (decimal)stdError * stdDevFactor;

            decimal prevTop = topList.LastOrDefault();
            decimal top = currentEma + ratio;
            topList.AddRounded(top);

            decimal prevBottom = bottomList.LastOrDefault();
            decimal bottom = currentEma - ratio;
            bottomList.AddRounded(bottom);

            var signal = GetBullishBearishSignal(currentValue - top, prevValue - prevTop, currentValue - bottom, prevValue - prevBottom);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", topList },
            { "MiddleBand", emaList },
            { "LowerBand", bottomList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.KirshenbaumBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Kaufman Adaptive Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="stdDevFactor"></param>
    /// <returns></returns>
    public static StockData CalculateKaufmanAdaptiveBands(this StockData stockData, int length = 100, decimal stdDevFactor = 3)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<decimal> powMaList = new();
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal er = Pow(erList[i], stdDevFactor);
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (currentValue * er) + ((1 - er) * prevMiddleBand);
            middleBandList.AddRounded(middleBand);

            decimal prevPowMa = powMaList.LastOrDefault();
            decimal powMa = (Pow(currentValue, 2) * er) + ((1 - er) * prevPowMa);
            powMaList.AddRounded(powMa);

            decimal kaufmanDev = powMa - Pow(middleBand, 2) >= 0 ? Sqrt(powMa - Pow(middleBand, 2)) : 0;
            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + kaufmanDev;
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - kaufmanDev;
            lowerBandList.AddRounded(lowerBand);

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
        stockData.IndicatorName = IndicatorName.KaufmanAdaptiveBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Keltner Channels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="multFactor"></param>
    /// <returns></returns>
    public static StockData CalculateKeltnerChannels(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 20, int length2 = 10, decimal multFactor = 2)
    {
        List<decimal> upperChannelList = new();
        List<decimal> lowerChannelList = new();
        List<decimal> midChannelList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentEma20Day = emaList[i];
            decimal currentAtr10Day = atrList[i];

            decimal upperChannel = currentEma20Day + (multFactor * currentAtr10Day);
            upperChannelList.AddRounded(upperChannel);

            decimal lowerChannel = currentEma20Day - (multFactor * currentAtr10Day);
            lowerChannelList.AddRounded(lowerChannel);

            decimal prevMidChannel = midChannelList.LastOrDefault();
            decimal midChannel = (upperChannel + lowerChannel) / 2;
            midChannelList.AddRounded(midChannel);

            var signal = GetCompareSignal(currentValue - midChannel, prevValue - prevMidChannel);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperChannelList },
            { "MiddleBand", midChannelList },
            { "LowerBand", lowerChannelList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.KeltnerChannels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vortex Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVortexBands(this StockData stockData, MovingAvgType maType = MovingAvgType.McNichollMovingAverage,
        int length = 20)
    {
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<decimal> diffList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var basisList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal basis = basisList[i];
            decimal currentValue = inputList[i];

            decimal diff = currentValue - basis;
            diffList.AddRounded(diff);
        }

        var diffMaList = GetMovingAverageList(stockData, maType, length, diffList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal diffMa = diffMaList[i];
            decimal basis = basisList[i];
            decimal dev = 2 * diffMa;

            decimal upper = basis + dev;
            upperList.AddRounded(upper);

            decimal lower = basis - dev;
            lowerList.AddRounded(lower);

            var signal = GetConditionSignal(upper > lower && upper > basis, lower > upper && lower > basis);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperList },
            { "MiddleBand", basisList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.VortexBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Volume Adaptive Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateVolumeAdaptiveBands(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 100)
    {
        List<decimal> upList = new();
        List<decimal> dnList = new();
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

        var aList = GetMovingAverageList(stockData, maType, length, volumeList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal a = Math.Max(aList[i], 1);
            decimal b = a * -1;

            decimal prevUp = i >= 1 ? upList[i - 1] : currentValue;
            decimal up = a != 0 ? (prevUp + (currentValue * a)) / a : 0;
            upList.AddRounded(up);

            decimal prevDn = i >= 1 ? dnList[i - 1] : currentValue;
            decimal dn = b != 0 ? (prevDn + (currentValue * b)) / b : 0;
            dnList.AddRounded(dn);
        }

        var upSmaList = GetMovingAverageList(stockData, maType, length, upList);
        var dnSmaList = GetMovingAverageList(stockData, maType, length, dnList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal upperBand = upSmaList[i];
            decimal lowerBand = dnSmaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (upperBand + lowerBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetCompareSignal(currentValue - middleBand, prevValue - prevMiddleBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upSmaList },
            { "MiddleBand", middleBandList },
            { "LowerBand", dnSmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.VolumeAdaptiveBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Variable Moving Average Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateVariableMovingAverageBands(this StockData stockData, MovingAvgType maType = MovingAvgType.VariableMovingAverage,
        int length = 6, decimal mult = 1.5m)
    {
        List<decimal> ubandList = new();
        List<decimal> lbandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var maList = GetMovingAverageList(stockData, maType, length, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentAtr = atrList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal vma = maList[i];
            decimal prevVma = i >= 1 ? maList[i - 1] : 0;
            decimal o = mult * currentAtr;

            decimal prevUband = ubandList.LastOrDefault();
            decimal uband = vma + o;
            ubandList.AddRounded(uband);

            decimal prevLband = lbandList.LastOrDefault();
            decimal lband = vma - o;
            lbandList.AddRounded(lband);

            var signal = GetBollingerBandsSignal(currentValue - vma, prevValue - prevVma, currentValue, prevValue, uband, prevUband, lband, prevLband);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", ubandList },
            { "MiddleBand", maList },
            { "LowerBand", lbandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.VariableMovingAverageBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Vervoort Volatility Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="devMult"></param>
    /// <param name="lowBandMult"></param>
    /// <returns></returns>
    public static StockData CalculateVervoortVolatilityBands(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 8, int length2 = 13, decimal devMult = 3.55m, decimal lowBandMult = 0.9m)
    {
        List<decimal> typicalList = new();
        List<decimal> deviationList = new();
        List<decimal> ubList = new();
        List<decimal> lbList = new();
        List<decimal> tempList = new();
        List<decimal> medianAvgSmaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, lowList, _, _) = GetInputValuesList(stockData);

        var medianAvgList = GetMovingAverageList(stockData, maType, length1, inputList);
        var medianAvgEmaList = GetMovingAverageList(stockData, maType, length1, medianAvgList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal medianAvg = medianAvgList[i];
            tempList.AddRounded(medianAvg);

            decimal currentValue = inputList[i];
            decimal currentLow = lowList[i];
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal typical = currentValue >= prevValue ? currentValue - prevLow : prevValue - currentLow;
            typicalList.AddRounded(typical);

            decimal typicalSma = typicalList.TakeLastExt(length2).Average();
            decimal deviation = devMult * typicalSma;
            deviationList.AddRounded(deviation);

            decimal medianAvgSma = tempList.TakeLastExt(length1).Average();
            medianAvgSmaList.AddRounded(medianAvgSma);
        }

        var devHighList = GetMovingAverageList(stockData, maType, length1, deviationList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal devHigh = devHighList[i];
            decimal midline = medianAvgSmaList[i];
            decimal medianAvgEma = medianAvgEmaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMidline = i >= 1 ? medianAvgSmaList[i - 1] : 0;
            decimal devLow = lowBandMult * devHigh;

            decimal prevUb = ubList.LastOrDefault();
            decimal ub = medianAvgEma + devHigh;
            ubList.AddRounded(ub);

            decimal prevLb = lbList.LastOrDefault();
            decimal lb = medianAvgEma - devLow;
            lbList.AddRounded(lb);

            var signal = GetBollingerBandsSignal(currentValue - midline, prevValue - prevMidline, currentValue, prevValue, ub, prevUb, lb, prevLb);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", ubList },
            { "MiddleBand", medianAvgSmaList },
            { "LowerBand", lbList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.VervoortVolatilityBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Headley Acceleration Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculatePriceHeadleyAccelerationBands(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 20, decimal factor = 0.001m)
    {
        List<decimal> ubList = new();
        List<decimal> lbList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        var middleBandList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal mult = currentHigh + currentLow != 0 ? 4 * factor * 1000 * (currentHigh - currentLow) / (currentHigh + currentLow) : 0;

            decimal outerUb = currentHigh * (1 + mult);
            ubList.AddRounded(outerUb);

            decimal outerLb = currentLow * (1 - mult);
            lbList.AddRounded(outerLb);
        }

        var suList = GetMovingAverageList(stockData, maType, length, ubList);
        var slList = GetMovingAverageList(stockData, maType, length, lbList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal middleBand = middleBandList[i];
            decimal prevMiddleBand = i >= 1 ? middleBandList[i - 1] : 0;
            decimal outerUbSma = suList[i];
            decimal prevOuterUbSma = i >= 1 ? suList[i - 1] : 0;
            decimal outerLbSma = slList[i];
            decimal prevOuterLbSma = i >= 1 ? slList[i - 1] : 0;

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, 
                outerUbSma, prevOuterUbSma, outerLbSma, prevOuterLbSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", suList },
            { "MiddleBand", middleBandList },
            { "LowerBand", slList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.PriceHeadleyAccelerationBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Pseudo Polynomial Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="morph"></param>
    /// <returns></returns>
    public static StockData CalculatePseudoPolynomialChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length = 14, decimal morph = 0.9m)
    {
        List<decimal> kList = new();
        List<decimal> yK1List = new();
        List<decimal> indexList = new();
        List<decimal> middleBandList = new();
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal y = inputList[i];
            decimal prevK = i >= length ? kList[i - length] : y;
            decimal prevK2 = i >= length * 2 ? kList[i - (length * 2)] : y;
            decimal prevIndex = i >= length ? indexList[i - length] : 0;
            decimal prevIndex2 = i >= length * 2 ? indexList[i - (length * 2)] : 0;
            decimal ky = (morph * prevK) + ((1 - morph) * y);
            decimal ky2 = (morph * prevK2) + ((1 - morph) * y);

            decimal index = i;
            indexList.AddRounded(i);

            decimal k = prevIndex2 - prevIndex != 0 ? ky + ((index - prevIndex) / (prevIndex2 - prevIndex) * (ky2 - ky)) : 0;
            kList.AddRounded(k);
        }

        var k1List = GetMovingAverageList(stockData, maType, length, kList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal k1 = k1List[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal yk1 = Math.Abs(currentValue - k1);
            yK1List.AddRounded(yk1);

            decimal er = i != 0 ? yK1List.Sum() / i : 0;
            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = k1 + er;
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = k1 - er;
            lowerBandList.AddRounded(lowerBand);

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (upperBand + lowerBand) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.PseudoPolynomialChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Projected Support and Resistance
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateProjectedSupportAndResistance(this StockData stockData, int length = 25)
    {
        List<decimal> support1List = new();
        List<decimal> resistance1List = new();
        List<decimal> support2List = new();
        List<decimal> resistance2List = new();
        List<decimal> middleList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];
            decimal range = highestHigh - lowestLow;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal support1 = lowestLow - (0.25m * range);
            support1List.AddRounded(support1);

            decimal support2 = lowestLow - (0.5m * range);
            support2List.AddRounded(support2);

            decimal resistance1 = highestHigh + (0.25m * range);
            resistance1List.AddRounded(resistance1);

            decimal resistance2 = highestHigh + (0.5m * range);
            resistance2List.AddRounded(resistance2);

            decimal prevMiddle = middleList.LastOrDefault();
            decimal middle = (support1 + support2 + resistance1 + resistance2) / 4;
            middleList.AddRounded(middle);

            var signal = GetCompareSignal(currentValue - middle, prevValue - prevMiddle);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Support1", support1List },
            { "Support2", support2List },
            { "Resistance1", resistance1List },
            { "Resistance2", resistance2List },
            { "MiddleBand", middleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ProjectedSupportAndResistance;

        return stockData;
    }

    /// <summary>
    /// Calculates the Prime Number Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePrimeNumberBands(this StockData stockData, int length = 5)
    {
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        stockData.CustomValuesList = highList;
        var pnoUpBandList = CalculatePrimeNumberOscillator(stockData, length).CustomValuesList;
        stockData.CustomValuesList = lowList;
        var pnoDnBandList = CalculatePrimeNumberOscillator(stockData, length).CustomValuesList;
        var (upperBandList, _) = GetMaxAndMinValuesList(pnoUpBandList, length);
        var (_, lowerBandList) = GetMaxAndMinValuesList(pnoDnBandList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList[i];
            decimal prevUpBand1 = i >= 1 ? upperBandList[i - 1] : 0;
            decimal prevUpBand2 = i >= 2 ? upperBandList[i - 1] : 0;
            decimal prevDnBand1 = i >= 1 ? lowerBandList[i - 1] : 0;
            decimal prevDnBand2 = i >= 2 ? lowerBandList[i - 1] : 0;
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;

            var signal = GetBullishBearishSignal(close - prevUpBand1, prevClose - prevUpBand2, close - prevDnBand1, prevClose - prevDnBand2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.PrimeNumberBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Periodic Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculatePeriodicChannel(this StockData stockData, int length1 = 500, int length2 = 2)
    {
        List<decimal> tempList = new();
        List<decimal> indexList = new();
        List<decimal> corrList = new();
        List<decimal> absIndexCumDiffList = new();
        List<decimal> sinList = new();
        List<decimal> inSinList = new();
        List<decimal> absSinCumDiffList = new();
        List<decimal> absInSinCumDiffList = new();
        List<decimal> absDiffList = new();
        List<decimal> kList = new();
        List<decimal> absKDiffList = new();
        List<decimal> osList = new();
        List<decimal> apList = new();
        List<decimal> bpList = new();
        List<decimal> cpList = new();
        List<decimal> alList = new();
        List<decimal> blList = new();
        List<decimal> clList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevValue = tempList.LastOrDefault();
            decimal currentValue = inputList[i];
            tempList.AddRounded(currentValue);

            decimal index = i;
            indexList.AddRounded(index);

            decimal indexCum = i != 0 ? indexList.Sum() / i : 0;
            decimal indexCumDiff = i - indexCum;
            decimal absIndexCumDiff = Math.Abs(i - indexCum);
            absIndexCumDiffList.AddRounded(absIndexCumDiff);

            decimal absIndexCum = i != 0 ? absIndexCumDiffList.Sum() / i : 0;
            decimal z = absIndexCum != 0 ? indexCumDiff / absIndexCum : 0;

            var corr = GoodnessOfFit.R(indexList.TakeLastExt(length2).Select(x => (double)x), tempList.TakeLastExt(length2).Select(x => (double)x));
            corr = IsValueNullOrInfinity(corr) ? 0 : corr;
            corrList.AddRounded((decimal)corr);

            decimal s = i * Math.Sign(corrList.Sum());
            decimal sin = Sin(s / length1);
            sinList.AddRounded(sin);

            decimal inSin = Sin(s / length1) * -1;
            inSinList.AddRounded(inSin);

            decimal sinCum = i != 0 ? sinList.Sum() / i : 0;
            decimal inSinCum = i != 0 ? inSinList.Sum() / i : 0;
            decimal sinCumDiff = sin - sinCum;
            decimal inSinCumDiff = inSin - inSinCum;

            decimal absSinCumDiff = Math.Abs(sin - sinCum);
            absSinCumDiffList.AddRounded(absSinCumDiff);

            decimal absSinCum = i != 0 ? absSinCumDiffList.Sum() / i : 0;
            decimal absInSinCumDiff = Math.Abs(inSin - inSinCum);
            absInSinCumDiffList.AddRounded(absInSinCumDiff);

            decimal absInSinCum = i != 0 ? absInSinCumDiffList.Sum() / i : 0;
            decimal zs = absSinCum != 0 ? sinCumDiff / absSinCum : 0;
            decimal inZs = absInSinCum != 0 ? inSinCumDiff / absInSinCum : 0;
            decimal cum = i != 0 ? tempList.Sum() / i : 0;

            decimal absDiff = Math.Abs(currentValue - cum);
            absDiffList.AddRounded(absDiff);

            decimal absDiffCum = i != 0 ? absDiffList.Sum() / i : 0;
            decimal prevK = kList.LastOrDefault();
            decimal k = cum + ((z + zs) * absDiffCum);
            kList.AddRounded(k);

            decimal inK = cum + ((z + inZs) * absDiffCum);
            decimal absKDiff = Math.Abs(currentValue - k);
            absKDiffList.AddRounded(absKDiff);

            decimal absInKDiff = Math.Abs(currentValue - inK);
            decimal os = i != 0 ? absKDiffList.Sum() / i : 0;
            osList.AddRounded(os);

            decimal ap = k + os;
            apList.AddRounded(ap);

            decimal bp = ap + os;
            bpList.AddRounded(bp);

            decimal cp = bp + os;
            cpList.AddRounded(cp);

            decimal al = k - os;
            alList.AddRounded(al);

            decimal bl = al - os;
            blList.AddRounded(bl);

            decimal cl = bl - os;
            clList.AddRounded(cl);

            var signal = GetCompareSignal(currentValue - k, prevValue - prevK);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "K", kList },
            { "Os", osList },
            { "Ap", apList },
            { "Bp", bpList },
            { "Cp", cpList },
            { "Al", alList },
            { "Bl", blList },
            { "Cl", clList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.PeriodicChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Line Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePriceLineChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 100)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> sizeAList = new();
        List<decimal> sizeBList = new();
        List<decimal> sizeCList = new();
        List<decimal> midList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal atr = atrList[i];
            decimal prevA1 = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevB1 = i >= 1 ? bList[i - 1] : currentValue;
            decimal prevA2 = i >= 2 ? aList[i - 2] : 0;
            decimal prevB2 = i >= 2 ? bList[i - 2] : 0;
            decimal prevSizeA = i >= 1 ? sizeAList[i - 1] : atr / length;
            decimal prevSizeB = i >= 1 ? sizeBList[i - 1] : atr / length;
            decimal prevSizeC = i >= 1 ? sizeCList[i - 1] : atr / length;

            decimal sizeA = prevA1 - prevA2 > 0 ? atr : prevSizeA;
            sizeAList.AddRounded(sizeA);

            decimal sizeB = prevB1 - prevB2 < 0 ? atr : prevSizeB;
            sizeBList.AddRounded(sizeB);

            decimal sizeC = prevA1 - prevA2 > 0 || prevB1 - prevB2 < 0 ? atr : prevSizeC;
            sizeCList.AddRounded(sizeC);

            decimal a = Math.Max(currentValue, prevA1) - (sizeA / length);
            aList.AddRounded(a);

            decimal b = Math.Min(currentValue, prevB1) + (sizeB / length);
            bList.AddRounded(b);

            decimal prevMid = midList.LastOrDefault();
            decimal mid = (a + b) / 2;
            midList.AddRounded(mid);

            var signal = GetCompareSignal(currentValue - mid, prevValue - prevMid);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aList },
            { "MiddleBand", midList },
            { "LowerBand", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.PriceLineChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Price Curve Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculatePriceCurveChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 100)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> sizeList = new();
        List<decimal> aChgList = new();
        List<decimal> bChgList = new();
        List<decimal> midList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal atr = atrList[i];
            decimal prevA1 = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevB1 = i >= 1 ? bList[i - 1] : currentValue;
            decimal prevA2 = i >= 2 ? aList[i - 2] : 0;
            decimal prevB2 = i >= 2 ? bList[i - 2] : 0;
            decimal prevSize = i >= 1 ? sizeList[i - 1] : atr / length;

            decimal size = prevA1 - prevA2 > 0 || prevB1 - prevB2 < 0 ? atr : prevSize;
            sizeList.AddRounded(size);

            decimal aChg = prevA1 > prevA2 ? 1 : 0;
            aChgList.AddRounded(aChg);

            decimal bChg = prevB1 < prevB2 ? 1 : 0;
            bChgList.AddRounded(bChg);

            int maxIndexA = aChgList.LastIndexOf(1);
            int maxIndexB = bChgList.LastIndexOf(1);
            int barsSinceA = aChgList.Count - 1 - maxIndexA;
            int barsSinceB = bChgList.Count - 1 - maxIndexB;

            decimal a = Math.Max(currentValue, prevA1) - (size / Pow(length, 2) * (barsSinceA + 1));
            aList.AddRounded(a);

            decimal b = Math.Min(currentValue, prevB1) + (size / Pow(length, 2) * (barsSinceB + 1));
            bList.AddRounded(b);

            decimal prevMid = midList.LastOrDefault();
            decimal mid = (a + b) / 2;
            midList.AddRounded(mid);

            var signal = GetCompareSignal(currentValue - mid, prevValue - prevMid);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aList },
            { "MiddleBand", midList },
            { "LowerBand", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.PriceCurveChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Projection Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateProjectionBands(this StockData stockData, int length = 14)
    {
        List<decimal> puList = new();
        List<decimal> plList = new();
        List<decimal> middleBandList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        stockData.CustomValuesList = lowList;
        var lowSlopeList = CalculateLinearRegression(stockData, length).OutputValues["Slope"];
        stockData.CustomValuesList = highList;
        var highSlopeList = CalculateLinearRegression(stockData, length).OutputValues["Slope"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevPu = i >= 1 ? puList[i - 1] : 0;
            decimal prevPl = i >= 1 ? plList[i - 1] : 0;

            decimal pu = currentHigh, pl = currentLow;
            for (int j = 1; j <= length; j++)
            {
                decimal highSlope = i >= j ? highSlopeList[i - j] : 0;
                decimal lowSlope = i >= j ? lowSlopeList[i - j] : 0;
                decimal pHigh = i >= j - 1 ? highList[i - (j - 1)] : 0;
                decimal pLow = i >= j - 1 ? lowList[i - (j - 1)] : 0;
                decimal vHigh = pHigh + (highSlope * j);
                decimal vLow = pLow + (lowSlope * j);
                pu = Math.Max(pu, vHigh);
                pl = Math.Min(pl, vLow);
            }
            puList.AddRounded(pu);
            plList.AddRounded(pl);

            decimal prevMiddleBand = middleBandList.LastOrDefault();
            decimal middleBand = (pu + pl) / 2;
            middleBandList.AddRounded(middleBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, pu, prevPu, pl, prevPl);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", puList },
            { "MiddleBand", middleBandList },
            { "LowerBand", plList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ProjectionBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Trend Trader Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <param name="bandStep"></param>
    /// <returns></returns>
    public static StockData CalculateTrendTraderBands(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
        int length = 21, decimal mult = 3, decimal bandStep = 20)
    {
        List<decimal> retList = new();
        List<decimal> outerUpperBandList = new();
        List<decimal> outerLowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal close = inputList[i];
            decimal prevHighest = i >= 1 ? highestList[i - 1] : 0;
            decimal prevLowest = i >= 1 ? lowestList[i - 1] : 0;
            decimal prevAtr = i >= 1 ? atrList[i - 1] : 0;
            decimal atrMult = prevAtr * mult;
            decimal highLimit = prevHighest - atrMult;
            decimal lowLimit = prevLowest + atrMult;

            decimal ret = close > highLimit && close > lowLimit ? highLimit : close < lowLimit && close < highLimit ? lowLimit : retList.LastOrDefault();
            retList.AddRounded(ret);
        }

        var retEmaList = GetMovingAverageList(stockData, maType, length, retList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal retEma = retEmaList[i];
            decimal close = inputList[i];
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal prevRetEma = i >= 1 ? retEmaList[i - 1] : 0;

            decimal prevOuterUpperBand = outerUpperBandList.LastOrDefault();
            decimal outerUpperBand = retEma + bandStep;
            outerUpperBandList.AddRounded(outerUpperBand);

            decimal prevOuterLowerBand = outerLowerBandList.LastOrDefault();
            decimal outerLowerBand = retEma - bandStep;
            outerLowerBandList.AddRounded(outerLowerBand);

            var signal = GetBollingerBandsSignal(close - retEma, prevClose - prevRetEma, close, prevClose, outerUpperBand, 
                prevOuterUpperBand, outerLowerBand, prevOuterLowerBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", outerUpperBandList },
            { "MiddleBand", retEmaList },
            { "LowerBand", outerLowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.TrendTraderBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Time and Money Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateTimeAndMoneyChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
        int length1 = 41, int length2 = 82)
    {
        List<decimal> yomList = new();
        List<decimal> yomSquaredList = new();
        List<decimal> varyomList = new();
        List<decimal> somList = new();
        List<decimal> chPlus1List = new();
        List<decimal> chMinus1List = new();
        List<decimal> chPlus2List = new();
        List<decimal> chMinus2List = new();
        List<decimal> chPlus3List = new();
        List<decimal> chMinus3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int halfLength = MinOrMax((int)Math.Ceiling((decimal)length1 / 2));

        var smaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevBasis = i >= halfLength ? smaList[i - halfLength] : 0;

            decimal yom = prevBasis != 0 ? 100 * (currentValue - prevBasis) / prevBasis : 0;
            yomList.AddRounded(yom);

            decimal yomSquared = Pow(yom, 2);
            yomSquaredList.AddRounded(yomSquared);
        }

        var avyomList = GetMovingAverageList(stockData, maType, length2, yomList);
        var yomSquaredSmaList = GetMovingAverageList(stockData, maType, length2, yomSquaredList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevVaryom = i >= halfLength ? varyomList[i - halfLength] : 0;
            decimal avyom = avyomList[i];
            decimal yomSquaredSma = yomSquaredSmaList[i];

            decimal varyom = yomSquaredSma - (avyom * avyom);
            varyomList.AddRounded(varyom);

            decimal som = prevVaryom >= 0 ? Sqrt(prevVaryom) : 0;
            somList.AddRounded(som);
        }

        var sigomList = GetMovingAverageList(stockData, maType, length1, somList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal som = somList[i];
            decimal prevSom = i >= 1 ? somList[i - 1] : 0;
            decimal sigom = sigomList[i];
            decimal prevSigom = i >= 1 ? sigomList[i - 1] : 0;
            decimal basis = smaList[i];

            decimal chPlus1 = basis * (1 + (0.01m * sigom));
            chPlus1List.AddRounded(chPlus1);

            decimal chMinus1 = basis * (1 - (0.01m * sigom));
            chMinus1List.AddRounded(chMinus1);

            decimal chPlus2 = basis * (1 + (0.02m * sigom));
            chPlus2List.AddRounded(chPlus2);

            decimal chMinus2 = basis * (1 - (0.02m * sigom));
            chMinus2List.AddRounded(chMinus2);

            decimal chPlus3 = basis * (1 + (0.03m * sigom));
            chPlus3List.AddRounded(chPlus3);

            decimal chMinus3 = basis * (1 - (0.03m * sigom));
            chMinus3List.AddRounded(chMinus3);

            var signal = GetCompareSignal(som - sigom, prevSom - prevSigom);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ch+1", chPlus1List },
            { "Ch-1", chMinus1List },
            { "Ch+2", chPlus2List },
            { "Ch-2", chMinus2List },
            { "Ch+3", chPlus3List },
            { "Ch-3", chMinus3List },
            { "Median", sigomList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.TrendTraderBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Tirone Levels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTironeLevels(this StockData stockData, int length = 20)
    {
        List<decimal> tlhList = new();
        List<decimal> clhList = new();
        List<decimal> blhList = new();
        List<decimal> amList = new();
        List<decimal> ehList = new();
        List<decimal> elList = new();
        List<decimal> rhList = new();
        List<decimal> rlList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal hh = highestList[i];
            decimal ll = lowestList[i];

            decimal tlh = hh - ((hh - ll) / 3);
            tlhList.AddRounded(tlh);

            decimal clh = ll + ((hh - ll) / 2);
            clhList.AddRounded(clh);

            decimal blh = ll + ((hh - ll) / 3);
            blhList.AddRounded(blh);

            decimal prevAm = amList.LastOrDefault();
            decimal am = (hh + ll + currentValue) / 3;
            amList.AddRounded(am);

            decimal eh = am + (hh - ll);
            ehList.AddRounded(eh);

            decimal el = am - (hh - ll);
            elList.AddRounded(el);

            decimal rh = (2 * am) - ll;
            rhList.AddRounded(rh);

            decimal rl = (2 * am) - hh;
            rlList.AddRounded(rl);

            var signal = GetCompareSignal(currentValue - am, prevValue - prevAm);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tlh", tlhList },
            { "Clh", clhList },
            { "Blh", blhList },
            { "Am", amList },
            { "Eh", ehList },
            { "El", elList },
            { "Rh", rhList },
            { "Rl", rlList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.TironeLevels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Time Series Forecast
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateTimeSeriesForecast(this StockData stockData, int length = 500)
    {
        List<decimal> absDiffList = new();
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var tsList = CalculateLinearRegression(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal ts = tsList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevTs = i >= 1 ? tsList[i - 1] : 0;

            decimal absDiff = Math.Abs(currentValue - ts);
            absDiffList.AddRounded(absDiff);

            decimal e = i != 0 ? absDiffList.Sum() / i : 0;
            decimal prevA = aList.LastOrDefault();
            decimal a = ts + e;
            aList.AddRounded(a);

            decimal prevB = bList.LastOrDefault();
            decimal b = ts - e;
            bList.AddRounded(b);

            var signal = GetBollingerBandsSignal(currentValue - ts, prevValue - prevTs, currentValue, prevValue, a, prevA, b, prevB);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aList },
            { "MiddleBand", tsList },
            { "LowerBand", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tsList;
        stockData.IndicatorName = IndicatorName.TimeSeriesForecast;

        return stockData;
    }

    /// <summary>
    /// Calculates the Range Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="stdDevFactor"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRangeBands(this StockData stockData, decimal stdDevFactor = 1, 
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(smaList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = smaList[i];
            decimal currentValue = inputList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? smaList[i - 1] : 0;
            decimal rangeDev = highest - lowest;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (rangeDev * stdDevFactor);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (rangeDev * stdDevFactor);
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.RangeBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Range Identifier
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRangeIdentifier(this StockData stockData, int length = 34)
    {
        List<decimal> upList = new();
        List<decimal> downList = new();
        List<decimal> midList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevUp = upList.LastOrDefault();
            decimal prevDown = downList.LastOrDefault();

            decimal up = currentValue < prevUp && currentValue > prevDown ? prevUp : currentHigh;
            upList.AddRounded(up);

            decimal down = currentValue < prevUp && currentValue > prevDown ? prevDown : currentLow;
            downList.AddRounded(down);

            decimal prevMid = midList.LastOrDefault();
            decimal mid = (up + down) / 2;
            midList.AddRounded(mid);

            var signal = GetCompareSignal(currentValue - mid, prevValue - prevMid);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upList },
            { "MiddleBand", midList },
            { "LowerBand", downList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.RangeIdentifier;

        return stockData;
    }

    /// <summary>
    /// Calculates the Rate Of Change Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="smoothLength"></param>
    /// <returns></returns>
    public static StockData CalculateRateOfChangeBands(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length = 12, int smoothLength = 3)
    {
        List<decimal> rocSquaredList = new();
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();

        var rocList = CalculateRateOfChange(stockData, length).CustomValuesList;
        var middleBandList = GetMovingAverageList(stockData, maType, smoothLength, rocList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal roc = rocList[i];
            decimal middleBand = middleBandList[i];
            decimal prevMiddleBand1 = i >= 1 ? middleBandList[i - 1] : 0;
            decimal prevMiddleBand2 = i >= 2 ? middleBandList[i - 2] : 0;

            decimal rocSquared = Pow(roc, 2);
            rocSquaredList.AddRounded(rocSquared);

            decimal squaredAvg = rocSquaredList.TakeLastExt(length).Average();
            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = Sqrt(squaredAvg);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = -upperBand;
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(middleBand - prevMiddleBand1, prevMiddleBand1 - prevMiddleBand2, middleBand, prevMiddleBand1, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.RateOfChangeBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Root Moving Average Squared Error Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="stdDevFactor"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateRootMovingAverageSquaredErrorBands(this StockData stockData, decimal stdDevFactor = 1, 
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<decimal> powList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sma = smaList[i];
            decimal currentValue = inputList[i];

            decimal pow = Pow(currentValue - sma, 2);
            powList.AddRounded(pow);
        }

        var powSmaList = GetMovingAverageList(stockData, maType, length, powList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = smaList[i];
            decimal currentValue = inputList[i];
            decimal powSma = powSmaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? smaList[i - 1] : 0;
            decimal rmaseDev = Sqrt(powSma);

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (rmaseDev * stdDevFactor);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (rmaseDev * stdDevFactor);
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.RootMovingAverageSquaredErrorBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageBands(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 10, int slowLength = 50, decimal mult = 1)
    {
        List<decimal> sqList = new();
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var fastMaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var slowMaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal fastMa = fastMaList[i];
            decimal slowMa = slowMaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevFastMa = i >= 1 ? fastMaList[i - 1] : 0;

            decimal sq = Pow(slowMa - fastMa, 2);
            sqList.AddRounded(sq);

            decimal dev = Sqrt(sqList.TakeLastExt(fastLength).Average()) * mult;
            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = slowMa + dev;
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = slowMa - dev;
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - fastMa, prevValue - prevFastMa, currentValue, prevValue, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", fastMaList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.MovingAverageBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Support Resistance
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageSupportResistance(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 10, decimal factor = 2)
    {
        List<decimal> topList = new();
        List<decimal> bottomList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal supportLevel = 1 + (factor / 100);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentSma = smaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevSma = i >= 1 ? smaList[i - 1] : 0;

            decimal top = currentSma * supportLevel;
            topList.AddRounded(top);

            decimal bottom = supportLevel != 0 ? currentSma / supportLevel : 0;
            bottomList.AddRounded(bottom);

            var signal = GetCompareSignal(currentValue - currentSma, prevValue - prevSma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", topList },
            { "MiddleBand", smaList },
            { "LowerBand", bottomList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.MovingAverageSupportResistance;

        return stockData;
    }

    /// <summary>
    /// Calculates the Motion To Attraction Channels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMotionToAttractionChannels(this StockData stockData, int length = 14)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> cList = new();
        List<decimal> dList = new();
        List<decimal> aMaList = new();
        List<decimal> bMaList = new();
        List<decimal> avgMaList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alpha = (decimal)1 / length;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevAMa = i >= 1 ? aMaList[i - 1] : currentValue;
            decimal prevBMa = i >= 1 ? bMaList[i - 1] : currentValue;

            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal a = currentValue > prevAMa ? currentValue : prevA;
            aList.AddRounded(a);

            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal b = currentValue < prevBMa ? currentValue : prevB;
            bList.AddRounded(b);

            decimal prevC = cList.LastOrDefault();
            decimal c = b - prevB != 0 ? prevC + alpha : a - prevA != 0 ? 0 : prevC;
            cList.AddRounded(c);

            decimal prevD = dList.LastOrDefault();
            decimal d = a - prevA != 0 ? prevD + alpha : b - prevB != 0 ? 0 : prevD;
            dList.AddRounded(d);

            decimal avg = (a + b) / 2;
            decimal aMa = (c * avg) + ((1 - c) * a);
            aMaList.AddRounded(aMa);

            decimal bMa = (d * avg) + ((1 - d) * b);
            bMaList.AddRounded(bMa);

            decimal prevAvgMa = avgMaList.LastOrDefault();
            decimal avgMa = (aMa + bMa) / 2;
            avgMaList.AddRounded(avgMa);

            var signal = GetCompareSignal(currentValue - avgMa, prevValue - prevAvgMa);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aMaList },
            { "MiddleBand", avgMaList },
            { "LowerBand", bMaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.MotionToAttractionChannels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mean Absolute Error Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="stdDevFactor"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMeanAbsoluteErrorBands(this StockData stockData, decimal stdDevFactor = 1, 
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 14)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<decimal> devList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = smaList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? smaList[i - 1] : 0;

            decimal dev = Math.Abs(currentValue - middleBand);
            devList.AddRounded(dev);

            decimal maeDev = i != 0 ? devList.Sum() / i : 0;
            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (maeDev * stdDevFactor);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (maeDev * stdDevFactor);
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.MeanAbsoluteErrorBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Mean Absolute Deviation Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="stdDevFactor"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMeanAbsoluteDeviationBands(this StockData stockData, decimal stdDevFactor = 2, 
        MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);
        var devList = CalculateStandardDeviationVolatility(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal middleBand = smaList[i];
            decimal currentValue = inputList[i];
            decimal currentStdDeviation = devList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? smaList[i - 1] : 0;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = middleBand + (currentStdDeviation * stdDevFactor);
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = middleBand - (currentStdDeviation * stdDevFactor);
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, 
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.MeanAbsoluteErrorBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Moving Average Displaced Envelope
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="pct"></param>
    /// <returns></returns>
    public static StockData CalculateMovingAverageDisplacedEnvelope(this StockData stockData, 
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 9, int length2 = 13, decimal pct = 0.5m)
    {
        List<decimal> upperEnvelopeList = new();
        List<decimal> lowerEnvelopeList = new();
        List<decimal> middleEnvelopeList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevEma = i >= length2 ? emaList[i - length2] : 0;

            decimal prevUpperEnvelope = upperEnvelopeList.LastOrDefault();
            decimal upperEnvelope = prevEma * ((100 + pct) / 100);
            upperEnvelopeList.AddRounded(upperEnvelope);

            decimal prevLowerEnvelope = lowerEnvelopeList.LastOrDefault();
            decimal lowerEnvelope = prevEma * ((100 - pct) / 100);
            lowerEnvelopeList.AddRounded(lowerEnvelope);

            decimal prevMiddleEnvelope = middleEnvelopeList.LastOrDefault();
            decimal middleEnvelope = (upperEnvelope + lowerEnvelope) / 2;
            middleEnvelopeList.AddRounded(middleEnvelope);

            var signal = GetBollingerBandsSignal(currentValue - middleEnvelope, prevValue - prevMiddleEnvelope, currentValue, prevValue,
                upperEnvelope, prevUpperEnvelope, lowerEnvelope, prevLowerEnvelope);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperEnvelopeList },
            { "MiddleBand", middleEnvelopeList },
            { "LowerBand", lowerEnvelopeList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.MovingAverageDisplacedEnvelope;

        return stockData;
    }

    /// <summary>
    /// Calculates the Dema 2 Lines
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateDema2Lines(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int fastLength = 10, int slowLength = 40)
    {
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var ema1List = GetMovingAverageList(stockData, maType, fastLength, inputList);
        var ema2List = GetMovingAverageList(stockData, maType, slowLength, inputList);
        var dema1List = GetMovingAverageList(stockData, maType, fastLength, ema1List);
        var dema2List = GetMovingAverageList(stockData, maType, slowLength, ema2List);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dema1 = dema1List[i];
            decimal dema2 = dema2List[i];
            decimal prevDema1 = i >= 1 ? dema1List[i - 1] : 0;
            decimal prevDema2 = i >= 1 ? dema2List[i - 1] : 0;

            var signal = GetCompareSignal(dema1 - dema2, prevDema1 - prevDema2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dema1", dema1List },
            { "Dema2", dema2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.Dema2Lines;

        return stockData;
    }

    /// <summary>
    /// Calculates the Dynamic Support and Resistance
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDynamicSupportAndResistance(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 25)
    {
        List<decimal> supportList = new();
        List<decimal> resistanceList = new();
        List<decimal> middleList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        decimal mult = Sqrt(length);

        var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal currentAvgTrueRange = atrList[i];
            decimal highestHigh = highestList[i];
            decimal lowestLow = lowestList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal support = highestHigh - (currentAvgTrueRange * mult);
            supportList.AddRounded(support);

            decimal resistance = lowestLow + (currentAvgTrueRange * mult);
            resistanceList.AddRounded(resistance);

            decimal prevMiddle = middleList.LastOrDefault();
            decimal middle = (support + resistance) / 2;
            middleList.AddRounded(middle);

            var signal = GetCompareSignal(currentValue - middle, prevValue - prevMiddle);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Support", supportList },
            { "Resistance", resistanceList },
            { "MiddleBand", middleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.DynamicSupportAndResistance;

        return stockData;
    }

    /// <summary>
    /// Calculates the Daily Average Price Delta
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDailyAveragePriceDelta(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 21)
    {
        List<decimal> topList = new();
        List<decimal> bottomList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        var smaHighList = GetMovingAverageList(stockData, maType, length, highList);
        var smaLowList = GetMovingAverageList(stockData, maType, length, lowList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal high = highList[i];
            decimal low = lowList[i];
            decimal highSma = smaHighList[i];
            decimal lowSma = smaLowList[i];
            decimal dapd = highSma - lowSma;

            decimal prevTop = topList.LastOrDefault();
            decimal top = high + dapd;
            topList.AddRounded(top);

            decimal prevBottom = bottomList.LastOrDefault();
            decimal bottom = low - dapd;
            bottomList.AddRounded(bottom);

            var signal = GetConditionSignal(high > prevTop, low < prevBottom);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", topList },
            { "LowerBand", bottomList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.DailyAveragePriceDelta;

        return stockData;
    }

    /// <summary>
    /// Calculates the D Envelope
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="devFactor"></param>
    /// <returns></returns>
    public static StockData CalculateDEnvelope(this StockData stockData, int length = 20, decimal devFactor = 2)
    {
        List<decimal> mtList = new();
        List<decimal> utList = new();
        List<decimal> dtList = new();
        List<decimal> mt2List = new();
        List<decimal> ut2List = new();
        List<decimal> butList = new();
        List<decimal> bltList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alp = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevMt = mtList.LastOrDefault();
            decimal mt = (alp * currentValue) + ((1 - alp) * prevMt);
            mtList.AddRounded(mt);

            decimal prevUt = utList.LastOrDefault();
            decimal ut = (alp * mt) + ((1 - alp) * prevUt);
            utList.AddRounded(ut);

            decimal prevDt = dtList.LastOrDefault();
            decimal dt = (2 - alp) * (mt - ut) / (1 - alp);
            dtList.AddRounded(dt);

            decimal prevMt2 = mt2List.LastOrDefault();
            decimal mt2 = (alp * Math.Abs(currentValue - dt)) + ((1 - alp) * prevMt2);
            mt2List.AddRounded(mt2);

            decimal prevUt2 = ut2List.LastOrDefault();
            decimal ut2 = (alp * mt2) + ((1 - alp) * prevUt2);
            ut2List.AddRounded(ut2);

            decimal dt2 = (2 - alp) * (mt2 - ut2) / (1 - alp);
            decimal prevBut = butList.LastOrDefault();
            decimal but = dt + (devFactor * dt2);
            butList.AddRounded(but);

            decimal prevBlt = bltList.LastOrDefault();
            decimal blt = dt - (devFactor * dt2);
            bltList.AddRounded(blt);

            var signal = GetBollingerBandsSignal(currentValue - dt, prevValue - prevDt, currentValue, prevValue, but, prevBut, blt, prevBlt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", butList },
            { "MiddleBand", dtList },
            { "LowerBand", bltList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.DEnvelope;

        return stockData;
    }

    /// <summary>
    /// Calculates the Smart Envelope
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateSmartEnvelope(this StockData stockData, int length = 14, decimal factor = 1)
    {
        List<decimal> aList = new();
        List<decimal> bList = new();
        List<decimal> aSignalList = new();
        List<decimal> bSignalList = new();
        List<decimal> avgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevB = i >= 1 ? bList[i - 1] : currentValue;
            decimal prevASignal = aSignalList.LastOrDefault();
            decimal prevBSignal = bSignalList.LastOrDefault();
            decimal diff = Math.Abs(currentValue - prevValue);

            decimal a = Math.Max(currentValue, prevA) - (Math.Min(Math.Abs(currentValue - prevA), diff) / length * prevASignal);
            aList.AddRounded(a);

            decimal b = Math.Min(currentValue, prevB) + (Math.Min(Math.Abs(currentValue - prevB), diff) / length * prevBSignal);
            bList.AddRounded(b);

            decimal aSignal = b < prevB ? -factor : factor;
            aSignalList.AddRounded(aSignal);

            decimal bSignal = a > prevA ? -factor : factor;
            bSignalList.AddRounded(bSignal);

            decimal prevAvg = avgList.LastOrDefault();
            decimal avg = (a + b) / 2;
            avgList.AddRounded(avg);

            var signal = GetCompareSignal(currentValue - avg, prevValue - prevAvg);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aList },
            { "MiddleBand", avgList },
            { "LowerBand", bList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.SmartEnvelope;

        return stockData;
    }

    /// <summary>
    /// Calculates the Support Resistance
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateSupportResistance(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<decimal> resList = new();
        List<decimal> suppList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal sma = i >= 1 ? smaList[i - 1] : 0;
            bool crossAbove = prevValue < sma && currentValue >= sma;
            bool crossBelow = prevValue > sma && currentValue <= sma;

            decimal prevRes = resList.LastOrDefault();
            decimal res = crossBelow ? highest : i >= 1 ? prevRes : highest;
            resList.AddRounded(res);

            decimal prevSupp = suppList.LastOrDefault();
            decimal supp = crossAbove ? lowest : i >= 1 ? prevSupp : lowest;
            suppList.AddRounded(supp);

            var signal = GetBullishBearishSignal(currentValue - res, prevValue - prevRes, currentValue - supp, prevValue - prevSupp);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Support", suppList },
            { "Resistance", resList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.SupportResistance;

        return stockData;
    }

    /// <summary>
    /// Calculates the Stationary Extrapolated Levels
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateStationaryExtrapolatedLevels(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 200)
    {
        List<decimal> extList = new();
        List<decimal> yList = new();
        List<decimal> xList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var smaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal sma = smaList[i];
            decimal priorY = i >= length ? yList[i - length] : 0;
            decimal priorY2 = i >= length * 2 ? yList[i - (length * 2)] : 0;
            decimal priorX = i >= length ? xList[i - length] : 0;
            decimal priorX2 = i >= length * 2 ? xList[i - (length * 2)] : 0;

            decimal x = i;
            xList.AddRounded(i);

            decimal y = currentValue - sma;
            yList.AddRounded(y);

            decimal ext = priorX2 - priorX != 0 && priorY2 - priorY != 0 ? (priorY + ((x - priorX) / (priorX2 - priorX) * (priorY2 - priorY))) / 2 : 0;
            extList.AddRounded(ext);
        }

        var (highestList1, lowestList1) = GetMaxAndMinValuesList(extList, length);
        var (upperBandList, lowerBandList) = GetMaxAndMinValuesList(highestList1, lowestList1, length);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal y = yList[i];
            decimal ext = extList[i];
            decimal prevY = i >= 1 ? yList[i - 1] : 0;
            decimal prevExt = i >= 1 ? extList[i - 1] : 0;

            var signal = GetCompareSignal(y - ext, prevY - prevExt);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", yList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.StationaryExtrapolatedLevels;

        return stockData;
    }

    /// <summary>
    /// Calculates the Scalper's Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateScalpersChannel(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 15, 
        int length2 = 20)
    {
        List<decimal> scalperList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length1);

        var smaList = GetMovingAverageList(stockData, maType, length2, inputList);
        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal currentSma = smaList[i];
            decimal currentAtr = atrList[i];

            decimal prevScalper = scalperList.LastOrDefault();
            decimal scalper = Pi * currentAtr > 0 ? currentSma - Log(Pi * currentAtr) : currentSma;
            scalperList.AddRounded(scalper);

            var signal = GetCompareSignal(currentValue - scalper, prevValue - prevScalper);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", highestList },
            { "MiddleBand", scalperList },
            { "LowerBand", lowestList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ScalpersChannel;

        return stockData;
    }

    /// <summary>
    /// Calculates the Smoothed Volatility Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="deviation"></param>
    /// <param name="bandAdjust"></param>
    /// <returns></returns>
    public static StockData CalculateSmoothedVolatilityBands(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length1 = 20, int length2 = 21, decimal deviation = 2.4m, decimal bandAdjust = 0.9m)
    {
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int atrPeriod = (length1 * 2) - 1;

        var atrList = CalculateAverageTrueRange(stockData, maType, atrPeriod).CustomValuesList;
        var maList = GetMovingAverageList(stockData, maType, length1, inputList);
        var middleBandList = GetMovingAverageList(stockData, maType, length2, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal atr = atrList[i];
            decimal middleBand = middleBandList[i];
            decimal ma = maList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevMiddleBand = i >= 1 ? middleBandList[i - 1] : 0;
            decimal atrBuf = atr * deviation;

            decimal prevUpperBand = upperBandList.LastOrDefault();
            decimal upperBand = currentValue != 0 ? ma + (ma * atrBuf / currentValue) : ma;
            upperBandList.AddRounded(upperBand);

            decimal prevLowerBand = lowerBandList.LastOrDefault();
            decimal lowerBand = currentValue != 0 ? ma - (ma * atrBuf * bandAdjust / currentValue) : ma;
            lowerBandList.AddRounded(lowerBand);

            var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue,
                upperBand, prevUpperBand, lowerBand, prevLowerBand);
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
        stockData.IndicatorName = IndicatorName.SmoothedVolatilityBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Extended Recursive Bands
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateExtendedRecursiveBands(this StockData stockData, int length = 100)
    {
        List<decimal> aClassicList = new();
        List<decimal> bClassicList = new();
        List<decimal> cClassicList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal sc = (decimal)2 / (length + 1);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevAClassic = i >= 1 ? aClassicList[i - 1] : currentValue;
            decimal prevBClassic = i >= 1 ? bClassicList[i - 1] : currentValue;

            decimal aClassic = Math.Max(prevAClassic, currentValue) - (sc * Math.Abs(currentValue - prevAClassic));
            aClassicList.AddRounded(aClassic);

            decimal bClassic = Math.Min(prevBClassic, currentValue) + (sc * Math.Abs(currentValue - prevBClassic));
            bClassicList.AddRounded(bClassic);

            decimal prevCClassic = cClassicList.LastOrDefault();
            decimal cClassic = (aClassic + bClassic) / 2;
            cClassicList.AddRounded(cClassic);

            var signal = GetCompareSignal(currentValue - cClassic, prevValue - prevCClassic);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", aClassicList },
            { "MiddleBand", cClassicList },
            { "LowerBand", bClassicList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.ExtendedRecursiveBands;

        return stockData;
    }

    /// <summary>
    /// Calculates the Efficient Trend Step Channel
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateEfficientTrendStepChannel(this StockData stockData, int length = 100, int fastLength = 50, int slowLength = 200)
    {
        List<decimal> val2List = new();
        List<decimal> upperList = new();
        List<decimal> lowerList = new();
        List<decimal> aList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length).OutputValues["Er"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];

            decimal val2 = currentValue * 2;
            val2List.AddRounded(val2);
        }

        stockData.CustomValuesList = val2List;
        var stdDevFastList = CalculateStandardDeviationVolatility(stockData, length: fastLength).CustomValuesList;
        stockData.CustomValuesList = val2List;
        var stdDevSlowList = CalculateStandardDeviationVolatility(stockData, length: slowLength).CustomValuesList;
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal er = erList[i];
            decimal fastStdDev = stdDevFastList[i];
            decimal slowStdDev = stdDevSlowList[i];
            decimal prevA = i >= 1 ? aList[i - 1] : currentValue;
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal dev = (er * fastStdDev) + ((1 - er) * slowStdDev);

            decimal a = currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue : prevA;
            aList.AddRounded(a);

            decimal prevUpper = upperList.LastOrDefault();
            decimal upper = a + dev;
            upperList.AddRounded(upper);

            decimal prevLower = lowerList.LastOrDefault();
            decimal lower = a - dev;
            lowerList.AddRounded(lower);

            var signal = GetBollingerBandsSignal(currentValue - a, prevValue - prevA, currentValue, prevValue, upper, prevUpper, lower, prevLower);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperList },
            { "MiddleBand", aList },
            { "LowerBand", lowerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.EfficientTrendStepChannel;

        return stockData;
    }
}
