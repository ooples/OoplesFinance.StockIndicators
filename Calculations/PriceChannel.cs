using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;
using static OoplesFinance.StockIndicators.Helpers.SignalHelper;
using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
using static OoplesFinance.StockIndicators.Helpers.MathHelper;
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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevA = aList.LastOrDefault();
                decimal prevB = bList.LastOrDefault();
                decimal factor = length != 0 ? (prevA - prevB) / length : 0;

                decimal a = Math.Max(currentValue, prevA) - factor;
                aList.Add(a);

                decimal b = Math.Min(currentValue, prevB) + factor;
                bList.Add(b);

                decimal prevMid = midList.LastOrDefault();
                decimal mid = (a + b) / 2;
                midList.Add(mid);

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

            for (int j = 0; j < stockData.Count; j++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(j);
                decimal prevValue = j >= 1 ? inputList.ElementAtOrDefault(j - 1) : 0;
                decimal upperBand = upperBandList.ElementAtOrDefault(j);
                decimal lowerBand = lowerBandList.ElementAtOrDefault(j);

                decimal prevMiddleBand = middleBandList.LastOrDefault();
                decimal middleBand = (upperBand + lowerBand) / 2;
                middleBandList.Add(middleBand);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal tma = tmaList2.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevTma = i >= 1 ? tmaList2.ElementAtOrDefault(i - 1) : 0;

                decimal prevHighBand = highBandList.LastOrDefault();
                decimal highBand = tma + (tma * pctShift / 100);
                highBandList.Add(highBand);

                decimal prevLowBand = lowBandList.LastOrDefault();
                decimal lowBand = tma - (tma * pctShift / 100);
                lowBandList.Add(lowBand);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal sclAtr = sclAtrList.ElementAtOrDefault(i);
                decimal mclAtr = mclAtrList.ElementAtOrDefault(i);
                decimal prevSclRma = i >= scl_2 ? sclRmaList.ElementAtOrDefault(i - scl_2) : currentValue;
                decimal prevMclRma = i >= mcl_2 ? mclRmaList.ElementAtOrDefault(i - mcl_2) : currentValue;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal scm_off = fastMult * sclAtr;
                decimal mcm_off = slowMult * mclAtr;

                decimal prevSct = sctList.LastOrDefault();
                decimal sct = prevSclRma + scm_off;
                sctList.Add(sct);

                decimal prevScb = scbList.LastOrDefault();
                decimal scb = prevSclRma - scm_off;
                scbList.Add(scb);

                decimal mct = prevMclRma + mcm_off;
                mctList.Add(mct);

                decimal mcb = prevMclRma - mcm_off;
                mcbList.Add(mcb);

                decimal scmm = (sct + scb) / 2;
                scmmList.Add(scmm);

                decimal mcmm = (mct + mcb) / 2;
                mcmmList.Add(mcmm);

                decimal omed = mct - mcb != 0 ? (scmm - mcb) / (mct - mcb) : 0;
                omedList.Add(omed);

                decimal oshort = mct - mcb != 0 ? (currentValue - mcb) / (mct - mcb) : 0;
                oshortList.Add(oshort);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevCma1 = i >= 1 ? cmaList.ElementAtOrDefault(i - 1) : 0;
                decimal prevCma2 = i >= 2 ? cmaList.ElementAtOrDefault(i - 2) : 0;

                decimal dPrice = i >= displacement ? inputList.ElementAtOrDefault(i - displacement) : 0;
                dPriceList.Add(dPrice);

                decimal cma = dPrice == 0 ? prevCma1 + (prevCma1 - prevCma2) : dPriceList.TakeLastExt(length).Average();
                cmaList.Add(cma);

                decimal extremeBand = cma * extremeMult / 100;
                decimal outerBand = cma * outerMult / 100;
                decimal innerBand = cma * innerMult / 100;

                decimal upperExtremeBand = cma + extremeBand;
                upperExtremeBandList.Add(upperExtremeBand);

                decimal lowerExtremeBand = cma - extremeBand;
                lowerExtremeBandList.Add(lowerExtremeBand);

                decimal upperInnerBand = cma + innerBand;
                upperInnerBandList.Add(upperInnerBand);

                decimal lowerInnerBand = cma - innerBand;
                lowerInnerBandList.Add(lowerInnerBand);

                decimal upperOuterBand = cma + outerBand;
                upperOuterBandList.Add(upperOuterBand);

                decimal lowerOuterBand = cma - outerBand;
                lowerOuterBandList.Add(lowerOuterBand);

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
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal ema = emaList.ElementAtOrDefault(i);

                decimal d1 = currentValue - ema;
                d1List.Add(d1);

                decimal absD1 = Math.Abs(d1);
                absD1List.Add(absD1);
            }

            var wmaList = GetMovingAverageList(stockData, maType, length, absD1List);
            stockData.CustomValuesList = d1List;
            var s1List = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ema = emaList.ElementAtOrDefault(i);
                decimal s1 = s1List.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal x = ema + s1;

                decimal d2 = currentValue - x;
                d2List.Add(d2);
            }

            stockData.CustomValuesList = d2List;
            var s2List = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ema = emaList.ElementAtOrDefault(i);
                decimal s1 = s1List.ElementAtOrDefault(i);
                decimal s2 = s2List.ElementAtOrDefault(i);
                decimal prevS2 = i >= 1 ? s2List.ElementAtOrDefault(i - 1) : 0;
                decimal wma = wmaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevBasis = basisList.LastOrDefault();
                decimal basis = ema + s1 + (s2 - prevS2);
                basisList.Add(basis);

                decimal upper1 = basis + wma;
                upper1List.Add(upper1);

                decimal lower1 = basis - wma;
                lower1List.Add(lower1);

                decimal upper2 = upper1 + wma;
                upper2List.Add(upper2);

                decimal lower2 = lower1 - wma;
                lower2List.Add(lower2);

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

            var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal stdDev = stdDevList.ElementAtOrDefault(i);
                decimal prevA1 = i >= 1 ? aList.ElementAtOrDefault(i - 1) : currentValue;
                decimal prevB1 = i >= 1 ? bList.ElementAtOrDefault(i - 1) : currentValue;
                decimal prevA2 = i >= 2 ? aList.ElementAtOrDefault(i - 2) : currentValue;
                decimal prevB2 = i >= 2 ? bList.ElementAtOrDefault(i - 2) : currentValue;
                decimal prevA3 = i >= 3 ? aList.ElementAtOrDefault(i - 3) : currentValue;
                decimal prevB3 = i >= 3 ? bList.ElementAtOrDefault(i - 3) : currentValue;
                decimal l = stdDev != 0 ? (decimal)1 / length * stdDev : 0;

                decimal a = currentValue > prevA1 ? prevA1 + (currentValue - prevA1) : prevA2 == prevA3 ? prevA2 - l : prevA2;
                aList.Add(a);

                decimal b = currentValue < prevB1 ? prevB1 + (currentValue - prevB1) : prevB2 == prevB3 ? prevB2 + l : prevB2;
                bList.Add(b);

                decimal prevTos = tosList.LastOrDefault();
                decimal tos = currentValue > prevA2 ? 1 : currentValue < prevB2 ? 0 : prevTos;
                tosList.Add(tos);

                decimal prevTavg = tavgList.LastOrDefault();
                decimal avg = (a + b) / 2;
                decimal tavg = tos == 1 ? (a + avg) / 2 : (b + avg) / 2;
                tavgList.Add(tavg);

                decimal ts = (tos * b) + ((1 - tos) * a);
                tsList.Add(ts);

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
    }
}
