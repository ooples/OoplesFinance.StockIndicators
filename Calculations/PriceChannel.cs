using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;
using static OoplesFinance.StockIndicators.Helpers.SignalHelper;
using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
using OoplesFinance.StockIndicators.Enums;

namespace OoplesFinance.StockIndicators
{
    public static partial class Calculations
    {
        /// <summary>
        /// Calculates the price channel.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="pct">The PCT.</param>
        /// <returns></returns>
        public static StockData CalculatePriceChannel(this StockData stockData, MovingAvgType maType, int length = 21, decimal pct = 0.06m)
        {
            List<decimal> upperPriceChannelList = new();
            List<decimal> lowerPriceChannelList = new();
            List<decimal> midPriceChannelList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var emaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal upperPriceChannel = currentEma * (1 + pct);
                upperPriceChannelList.Add(upperPriceChannel);

                decimal lowerPriceChannel = currentEma * (1 - pct);
                lowerPriceChannelList.Add(lowerPriceChannel);

                decimal prevMidPriceChannel = midPriceChannelList.LastOrDefault();
                decimal midPriceChannel = (upperPriceChannel + lowerPriceChannel) / 2;
                midPriceChannelList.Add(midPriceChannel);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal upperChannel = highestList.ElementAtOrDefault(i);
                upperChannelList.Add(upperChannel);

                decimal lowerChannel = lowestList.ElementAtOrDefault(i);
                lowerChannelList.Add(lowerChannel);

                decimal prevMiddleChannel = middleChannelList.LastOrDefault();
                decimal middleChannel = (upperChannel + lowerChannel) / 2;
                middleChannelList.Add(middleChannel);

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

            var stdDeviationList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            var regressionList = CalculateLinearRegression(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal middleBand = regressionList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentStdDev = stdDeviationList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMiddleBand = i >= 1 ? regressionList.ElementAtOrDefault(i - 1) : 0;

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
        public static StockData CalculateStollerAverageRangeChannels(this StockData stockData, MovingAvgType maType, int length = 14, decimal atrMult = 2)
        {
            List<decimal> upperBandList = new();
            List<decimal> lowerBandList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);
            var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal middleBand = smaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentAtr = atrList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMiddleBand = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;

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
        /// Calculates the moving average channel.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateMovingAverageChannel(this StockData stockData, MovingAvgType maType, int length = 20)
        {
            List<decimal> midChannelList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            var highMaList = GetMovingAverageList(stockData, maType, length, highList);
            var lowMaList = GetMovingAverageList(stockData, maType, length, lowList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal upperChannel = highMaList.ElementAtOrDefault(i);
                decimal lowerChannel = lowMaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevMidChannel = midChannelList.LastOrDefault();
                decimal midChannel = (upperChannel + lowerChannel) / 2;
                midChannelList.Add(midChannel);

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
        /// Calculates the moving average envelope.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="mult">The mult.</param>
        /// <returns></returns>
        public static StockData CalculateMovingAverageEnvelope(this StockData stockData, MovingAvgType maType, int length = 20, decimal mult = 0.025m)
        {
            List<decimal> upperEnvelopeList = new();
            List<decimal> lowerEnvelopeList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentSma20 = smaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSma20 = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;
                decimal factor = currentSma20 * mult;

                decimal upperEnvelope = currentSma20 + factor;
                upperEnvelopeList.Add(upperEnvelope);

                decimal lowerEnvelope = currentSma20 - factor;
                lowerEnvelopeList.Add(lowerEnvelope);

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
                decimal prevHigh1 = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHigh2 = i >= 2 ? highList.ElementAtOrDefault(i - 2) : 0;
                decimal prevHigh3 = i >= 3 ? highList.ElementAtOrDefault(i - 3) : 0;
                decimal prevLow1 = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow2 = i >= 2 ? lowList.ElementAtOrDefault(i - 2) : 0;
                decimal prevLow3 = i >= 3 ? lowList.ElementAtOrDefault(i - 3) : 0;
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal oklUpper = prevHigh1 < prevHigh2 ? 1 : 0;
                decimal okrUpper = prevHigh3 < prevHigh2 ? 1 : 0;
                decimal oklLower = prevLow1 > prevLow2 ? 1 : 0;
                decimal okrLower = prevLow3 > prevLow2 ? 1 : 0;

                decimal prevUpperBand = upperBandList.LastOrDefault();
                decimal upperBand = oklUpper == 1 && okrUpper == 1 ? prevHigh2 : prevUpperBand;
                upperBandList.Add(upperBand);

                decimal prevLowerBand = lowerBandList.LastOrDefault();
                decimal lowerBand = oklLower == 1 && okrLower == 1 ? prevLow2 : prevLowerBand;
                lowerBandList.Add(lowerBand);

                decimal prevMiddleBand = middleBandList.LastOrDefault();
                decimal middleBand = (upperBand + lowerBand) / 2;
                middleBandList.Add(middleBand);

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
        /// Calculates the average true range channel.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="mult">The mult.</param>
        /// <returns></returns>
        public static StockData CalculateAverageTrueRangeChannel(this StockData stockData, MovingAvgType maType, int length = 14, decimal mult = 2.5m)
        {
            List<decimal> innerTopAtrChannelList = new();
            List<decimal> innerBottomAtrChannelList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
            var smaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal atr = atrList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;

                decimal prevTopInner = innerTopAtrChannelList.LastOrDefault();
                decimal topInner = Math.Round(currentValue + (atr * mult));
                innerTopAtrChannelList.Add(topInner);

                decimal prevBottomInner = innerBottomAtrChannelList.LastOrDefault();
                decimal bottomInner = Math.Round(currentValue - (atr * mult));
                innerBottomAtrChannelList.Add(bottomInner);

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
        /// <param name="smoothLength"></param>
        /// <param name="stdDevMult"></param>
        /// <returns></returns>
        public static StockData CalculateUltimateMovingAverageBands(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int minLength = 5, int maxLength = 50, int smoothLength = 4, decimal stdDevMult = 2)
        {
            List<decimal> upperBandList = new();
            List<decimal> lowerBandList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var umaList = CalculateUltimateMovingAverage(stockData, maType, minLength, maxLength, 1).CustomValuesList;
            var stdevList = CalculateStandardDeviationVolatility(stockData, minLength).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal uma = umaList.ElementAtOrDefault(i);
                decimal prevUma = i >= 1 ? umaList.ElementAtOrDefault(i - 1) : 0;
                decimal stdev = stdevList.ElementAtOrDefault(i);

                decimal prevUpperBand = upperBandList.LastOrDefault();
                decimal upperBand = uma + (stdDevMult * stdev);
                upperBandList.Add(upperBand);

                decimal prevLowerBand = lowerBandList.LastOrDefault();
                decimal lowerBand = uma - (stdDevMult * stdev);
                lowerBandList.Add(lowerBand);

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
                decimal currentSma = smaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;

                decimal prevUb = upperBandList.LastOrDefault();
                decimal ub = type1 ? currentSma + ubFac : currentSma + (currentSma * ubFac);
                upperBandList.Add(ub);

                decimal prevLb = lowerBandList.LastOrDefault();
                decimal lb = type1 ? currentSma - lbFac : currentSma - (currentSma * lbFac);
                lowerBandList.Add(lb);

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
                decimal rsi = rsiList.ElementAtOrDefault(i);

                decimal rsiOverbought = rsi - overbought;
                rsiOverboughtList.Add(rsiOverbought);

                decimal rsiOversold = rsi - oversold;
                rsiOversoldList.Add(rsiOversold);

                decimal rsiUpperNeutralZone = rsi - upperNeutralZone;
                rsiUpperNeutralZoneList.Add(rsiUpperNeutralZone);

                decimal rsiLowerNeutralZone = rsi - lowerNeutralZone;
                rsiLowerNeutralZoneList.Add(rsiLowerNeutralZone);
            }

            var obList = GetMovingAverageList(stockData, maType, smoothLength, rsiOverboughtList);
            var osList = GetMovingAverageList(stockData, maType, smoothLength, rsiOversoldList);
            var nzuList = GetMovingAverageList(stockData, maType, smoothLength, rsiUpperNeutralZoneList);
            var nzlList = GetMovingAverageList(stockData, maType, smoothLength, rsiLowerNeutralZoneList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal ob = obList.ElementAtOrDefault(i);
                decimal os = osList.ElementAtOrDefault(i);
                decimal nzu = nzuList.ElementAtOrDefault(i);
                decimal nzl = nzlList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevS1 = s1List.LastOrDefault();
                decimal s1 = currentValue - (currentValue * os / 100);
                s1List.Add(s1);

                decimal prevU1 = u1List.LastOrDefault();
                decimal u1 = currentValue - (currentValue * ob / 100);
                u1List.Add(u1);

                decimal prevU2 = u2List.LastOrDefault();
                decimal u2 = currentValue - (currentValue * nzu / 100);
                u2List.Add(u2);

                decimal prevS2 = s2List.LastOrDefault();
                decimal s2 = currentValue - (currentValue * nzl / 100);
                s2List.Add(s2);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : currentValue;
                decimal prevA2 = i >= 2 ? aList.ElementAtOrDefault(i - 2) : currentValue;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal x = currentValue + ((prevA - prevA2) * mult);

                decimal a = x > prevA + s ? prevA + s : x < prevA - s ? prevA - s : prevA;
                aList.AddRounded(a);

                decimal up = a + (Math.Abs(a - prevA) * mult);
                decimal dn = a - (Math.Abs(a - prevA) * mult);

                decimal prevUpper = upperList.LastOrDefault();
                decimal upper = up == a ? prevUpper : up;
                upperList.Add(upper);

                decimal prevLower = lowerList.LastOrDefault();
                decimal lower = dn == a ? prevLower : dn;
                lowerList.Add(lower);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal q1 = q1List.ElementAtOrDefault(i);
                decimal q3 = q3List.ElementAtOrDefault(i);
                decimal iqr = q3 - q1;

                decimal upperBand = q3 + (mult * iqr);
                upperBandList.Add(upperBand);

                decimal lowerBand = q1 - (mult * iqr);
                lowerBandList.Add(lowerBand);

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
    }
}
