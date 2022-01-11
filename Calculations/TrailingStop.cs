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
        /// Calculates the adaptive trailing stop.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="factor">The factor.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveTrailingStop(this StockData stockData, int length = 100, decimal factor = 3)
        {
            List<decimal> upList = new();
            List<decimal> dnList = new();
            List<decimal> aList = new();
            List<decimal> bList = new();
            List<decimal> osList = new();
            List<decimal> tsList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var perList = CalculatePoweredKaufmanAdaptiveMovingAverage(stockData, length, factor).OutputValues["Per"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal per = perList.ElementAtOrDefault(i);

                decimal prevA = i >= 1 ? aList.LastOrDefault() : currentValue;
                decimal a = Math.Max(currentValue, prevA) - (Math.Abs(currentValue - prevA) * per);
                aList.Add(a);

                decimal prevB = i >= 1 ? bList.LastOrDefault() : currentValue;
                decimal b = Math.Min(currentValue, prevB) + (Math.Abs(currentValue - prevB) * per);
                bList.Add(b);

                decimal prevUp = upList.LastOrDefault();
                decimal up = a > prevA ? a : a < prevA && b < prevB ? a : prevUp;
                upList.Add(up);

                decimal prevDn = dnList.LastOrDefault();
                decimal dn = b < prevB ? b : b > prevB && a > prevA ? b : prevDn;
                dnList.Add(dn);

                decimal prevOs = osList.LastOrDefault();
                decimal os = up > currentValue ? 1 : dn > currentValue ? 0 : prevOs;
                osList.Add(os);

                decimal prevTs = tsList.LastOrDefault();
                decimal ts = (os * dn) + ((1 - os) * up);
                tsList.Add(ts);

                var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ts", tsList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tsList;
            stockData.IndicatorName = IndicatorName.AdaptiveTrailingStop;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive autonomous recursive trailing stop.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="gamma">The gamma.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveAutonomousRecursiveTrailingStop(this StockData stockData, int length = 14, decimal gamma = 3)
        {
            List<decimal> tsList = new();
            List<decimal> osList = new();
            List<decimal> upperList = new();
            List<decimal> lowerList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var aamaList = CalculateAdaptiveAutonomousRecursiveMovingAverage(stockData, length, gamma);
            var ma2List = aamaList.CustomValuesList;
            var dList = aamaList.OutputValues["D"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal ma2 = ma2List.ElementAtOrDefault(i);
                decimal d = dList.ElementAtOrDefault(i);

                decimal prevUpper = upperList.LastOrDefault();
                decimal upper = ma2 + d;
                upperList.Add(upper);

                decimal prevLower = lowerList.LastOrDefault();
                decimal lower = ma2 - d;
                lowerList.Add(lower);

                decimal prevOs = osList.LastOrDefault();
                decimal os = currentValue > prevUpper ? 1 : currentValue < prevLower ? 0 : prevOs;
                osList.Add(os);

                decimal prevTs = tsList.LastOrDefault();
                decimal ts = (os * lower) + ((1 - os) * upper);
                tsList.Add(ts);

                var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ts", tsList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tsList;
            stockData.IndicatorName = IndicatorName.AdaptiveAutonomousRecursiveTrailingStop;

            return stockData;
        }

        /// <summary>
        /// Calculates the parabolic sar.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="start">The start.</param>
        /// <param name="increment">The increment.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns></returns>
        public static StockData CalculateParabolicSAR(this StockData stockData, decimal start = 0.02m, decimal increment = 0.02m, decimal maximum = 0.2m)
        {
            List<decimal> sarList = new();
            List<decimal> nextSarList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevHigh1 = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow1 = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHigh2 = i >= 2 ? highList.ElementAtOrDefault(i - 2) : 0;
                decimal prevLow2 = i >= 2 ? lowList.ElementAtOrDefault(i - 2) : 0;

                bool uptrend;
                decimal ep, prevSAR, prevEP, SAR, af = start;
                if (currentValue > prevValue)
                {
                    uptrend = true;
                    ep = currentHigh;
                    prevSAR = prevLow1;
                    prevEP = currentHigh;
                }
                else
                {
                    uptrend = false;
                    ep = currentLow;
                    prevSAR = prevHigh1;
                    prevEP = currentLow;
                }
                SAR = prevSAR + (start * (prevEP - prevSAR));

                if (uptrend)
                {
                    if (SAR > currentLow)
                    {
                        uptrend = false;
                        SAR = Math.Max(ep, currentHigh);
                        ep = currentLow;
                        af = start;
                    }
                }
                else
                {
                    if (SAR < currentHigh)
                    {
                        uptrend = true;
                        SAR = Math.Min(ep, currentLow);
                        ep = currentHigh;
                        af = start;
                    }
                }

                if (uptrend)
                {
                    if (currentHigh > ep)
                    {
                        ep = currentHigh;
                        af = Math.Min(af + increment, maximum);
                    }
                }
                else
                {
                    if (currentLow < ep)
                    {
                        ep = currentLow;
                        af = Math.Min(af + increment, maximum);
                    }
                }

                if (uptrend)
                {
                    SAR = i > 1 ? Math.Min(SAR, prevLow2) : Math.Min(SAR, prevLow1);
                }
                else
                {
                    SAR = i > 1 ? Math.Max(SAR, prevHigh2) : Math.Max(SAR, prevHigh1);
                }
                sarList.Add(SAR);

                decimal prevNextSar = nextSarList.LastOrDefault();
                decimal nextSar = SAR + (af * (ep - SAR));
                nextSarList.Add(nextSar);

                var signal = GetCompareSignal(currentHigh - nextSar, prevHigh1 - prevNextSar);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Sar", nextSarList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = nextSarList;
            stockData.IndicatorName = IndicatorName.ParabolicSAR;

            return stockData;
        }

        /// <summary>
        /// Calculates the chandelier exit.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="mult">The mult.</param>
        /// <returns></returns>
        public static StockData CalculateChandelierExit(this StockData stockData, MovingAvgType maType, int length = 22, decimal mult = 3)
        {
            List<decimal> chandelierExitLongList = new();
            List<decimal> chandelierExitShortList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

            var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentAvgTrueRange = atrList.ElementAtOrDefault(i);
                decimal highestHigh = highestList.ElementAtOrDefault(i);
                decimal lowestLow = lowestList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevExitLong = chandelierExitLongList.LastOrDefault();
                decimal chandelierExitLong = highestHigh - (currentAvgTrueRange * mult);
                chandelierExitLongList.Add(chandelierExitLong);

                decimal prevExitShort = chandelierExitShortList.LastOrDefault();
                decimal chandelierExitShort = lowestLow + (currentAvgTrueRange * mult);
                chandelierExitShortList.Add(chandelierExitShort);

                var signal = GetBullishBearishSignal(currentValue - chandelierExitLong, prevValue - prevExitLong, currentValue - chandelierExitShort, prevValue - prevExitShort);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "ExitLong", chandelierExitLongList },
                { "ExitShort", chandelierExitShortList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.ChandelierExit;

            return stockData;
        }

        /// <summary>
        /// Calculates the average true range trailing stops.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length1">The length1.</param>
        /// <param name="length2">The length2.</param>
        /// <param name="factor">The factor.</param>
        /// <returns></returns>
        public static StockData CalculateAverageTrueRangeTrailingStops(this StockData stockData, MovingAvgType maType, int length1 = 63, 
            int length2 = 21, decimal factor = 3)
        {
            List<decimal> atrtsList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;
            var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal currentAtr = atrList.ElementAtOrDefault(i);
                decimal prevAtrts = i >= 1 ? atrtsList.LastOrDefault() : currentValue;
                var upTrend = currentValue > currentEma;
                var dnTrend = currentValue <= currentEma;

                decimal atrts = upTrend ? Math.Max(currentValue - (factor * currentAtr), prevAtrts) : dnTrend ? 
                    Math.Min(currentValue + (factor * currentAtr), prevAtrts) : prevAtrts;
                atrtsList.Add(atrts);

                var signal = GetCompareSignal(currentValue - atrts, prevValue - prevAtrts);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Atrts", atrtsList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = atrtsList;
            stockData.IndicatorName = IndicatorName.AverageTrueRangeTrailingStops;

            return stockData;
        }

        /// <summary>
        /// Calculates the Linear Trailing Stop
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="mult"></param>
        /// <returns></returns>
        public static StockData CalculateLinearTrailingStop(this StockData stockData, int length = 14, decimal mult = 28)
        {
            List<decimal> aList = new();
            List<decimal> osList = new();
            List<decimal> tsList = new();
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
                aList.Add(a);

                decimal up = a + (Math.Abs(a - prevA) * mult);
                decimal dn = a - (Math.Abs(a - prevA) * mult);

                decimal prevUpper = upperList.LastOrDefault();
                decimal upper = up == a ? prevUpper : up;
                upperList.Add(upper);

                decimal prevLower = lowerList.LastOrDefault();
                decimal lower = dn == a ? prevLower : dn;
                lowerList.Add(lower);

                decimal prevOs = osList.LastOrDefault();
                decimal os = currentValue > upper ? 1 : currentValue > lower ? 0 : prevOs;
                osList.Add(os);

                decimal prevTs = tsList.LastOrDefault();
                decimal ts = (os * lower) + ((1 - os) * upper);
                tsList.Add(ts);

                var signal = GetCompareSignal(currentValue - ts, prevValue - prevTs);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ts", tsList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tsList;
            stockData.IndicatorName = IndicatorName.LinearTrailingStop;

            return stockData;
        }
    }
}
