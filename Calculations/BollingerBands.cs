using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Helpers;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;
using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
using static OoplesFinance.StockIndicators.Helpers.SignalHelper;
using static OoplesFinance.StockIndicators.Helpers.MathHelper;

namespace OoplesFinance.StockIndicators
{
    public static partial class Calculations
    {
        /// <summary>
        /// Calculates the bollinger bands.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateBollingerBands(this StockData stockData)
        {
            return CalculateBollingerBands(stockData, 2, MovingAvgType.SimpleMovingAverage, 20);
        }

        /// <summary>
        /// Calculates the bollinger bands.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="stdDevMult">The standard dev mult.</param>
        /// <param name="movingAvgType">Average type of the moving.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateBollingerBands(this StockData stockData, decimal stdDevMult = 2, 
            MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
        {
            List<decimal> upperBandList = new();
            List<decimal> lowerBandList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            var smaList = GetMovingAverageList(stockData, maType, length, inputList);
            var stdDeviationList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal middleBand = smaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentStdDeviation = stdDeviationList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMiddleBand = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;

                decimal prevUpperBand = upperBandList.LastOrDefault();
                decimal upperBand = middleBand + (currentStdDeviation * stdDevMult);
                upperBandList.AddRounded(upperBand);

                decimal prevLowerBand = lowerBandList.LastOrDefault();
                decimal lowerBand = middleBand - (currentStdDeviation * stdDevMult);
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
            stockData.IndicatorName = IndicatorName.BollingerBands;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive price zone indicator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="pct">The PCT.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptivePriceZoneIndicator(this StockData stockData, MovingAvgType maType, int length = 20, decimal pct = 2)
        {
            List<decimal> xHLList = new();
            List<decimal> outerUpBandList = new();
            List<decimal> outerDnBandList = new();
            List<decimal> middleBandList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            int nP = MinOrMax((int)Math.Ceiling(Sqrt((double)length)));

            var ema1List = GetMovingAverageList(stockData, maType, nP, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, nP, ema1List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);

                decimal xHL = currentHigh - currentLow;
                xHLList.Add(xHL);
            }

            var xHLEma1List = GetMovingAverageList(stockData, maType, nP, xHLList);
            var xHLEma2List = GetMovingAverageList(stockData, maType, nP, xHLEma1List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal xVal1 = ema2List.ElementAtOrDefault(i);
                decimal xVal2 = xHLEma2List.ElementAtOrDefault(i);

                decimal prevUpBand = outerUpBandList.LastOrDefault();
                decimal outerUpBand = (pct * xVal2) + xVal1;
                outerUpBandList.Add(outerUpBand);

                decimal prevDnBand = outerDnBandList.LastOrDefault();
                decimal outerDnBand = xVal1 - (pct * xVal2);
                outerDnBandList.Add(outerDnBand);

                decimal prevMiddleBand = middleBandList.LastOrDefault();
                decimal middleBand = (outerUpBand + outerDnBand) / 2;
                middleBandList.Add(middleBand);

                var signal = GetBollingerBandsSignal(currentValue - middleBand, prevValue - prevMiddleBand, currentValue, prevValue, outerUpBand,
                    prevUpBand, outerDnBand, prevDnBand);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "UpperBand", outerUpBandList },
                { "MiddleBand", middleBandList },
                { "LowerBand", outerDnBandList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.AdaptivePriceZoneIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the automatic dispersion bands.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="smoothLength">Length of the smooth.</param>
        /// <returns></returns>
        public static StockData CalculateAutoDispersionBands(this StockData stockData, MovingAvgType maType, int length = 90, int smoothLength = 140)
        {
            List<decimal> middleBandList = new();
            List<decimal> aList = new();
            List<decimal> bList = new();
            List<decimal> aMaxList = new();
            List<decimal> bMinList = new();
            List<decimal> x2List = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
                decimal x = currentValue - prevValue;

                decimal x2 = x * x;
                x2List.Add(x2);

                decimal x2Sma = x2List.TakeLast(length).Average();
                decimal sq = x2Sma >= 0 ? Sqrt(x2Sma) : 0;

                decimal a = currentValue + sq;
                aList.Add(a);

                decimal b = currentValue - sq;
                bList.Add(b);

                decimal aMax = aList.TakeLast(length).Max();
                aMaxList.Add(aMax);

                decimal bMin = bList.TakeLast(length).Min();
                bMinList.Add(bMin);
            }

            var aMaList = GetMovingAverageList(stockData, maType, length, aMaxList);
            var upperBandList = GetMovingAverageList(stockData, maType, smoothLength, aMaList);
            var bMaList = GetMovingAverageList(stockData, maType, length, bMinList);
            var lowerBandList = GetMovingAverageList(stockData, maType, smoothLength, bMaList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal upperBand = upperBandList.ElementAtOrDefault(i);
                decimal lowerBand = lowerBandList.ElementAtOrDefault(i);
                decimal prevUpperBand = i >= 1 ? upperBandList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLowerBand = i >= 1 ? lowerBandList.ElementAtOrDefault(i - 1) : 0;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevMiddleBand = middleBandList.LastOrDefault();
                decimal middleBand = (upperBand + lowerBand) / 2;
                middleBandList.Add(middleBand);

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
            stockData.IndicatorName = IndicatorName.AutoDispersionBands;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bollinger Bands Fibonacci Ratios
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="fibRatio1"></param>
        /// <param name="fibRatio2"></param>
        /// <param name="fibRatio3"></param>
        /// <returns></returns>
        public static StockData CalculateBollingerBandsFibonacciRatios(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 20, decimal fibRatio1 = 1.618m, decimal fibRatio2 = 2.618m, decimal fibRatio3 = 4.236m)
        {
            List<decimal> fibTop3List = new();
            List<decimal> fibBottom3List = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;
            var smaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal atr = atrList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSma = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;
                decimal r1 = atr * fibRatio1;
                decimal r2 = atr * fibRatio2;
                decimal r3 = atr * fibRatio3;

                decimal prevFibTop3 = fibTop3List.LastOrDefault();
                decimal fibTop3 = sma + r3;
                fibTop3List.Add(fibTop3);

                decimal fibTop2 = sma + r2;
                decimal fibTop1 = sma + r1;
                decimal fibBottom1 = sma - r1;
                decimal fibBottom2 = sma - r2;

                decimal prevFibBottom3 = fibBottom3List.LastOrDefault();
                decimal fibBottom3 = sma - r3;
                fibBottom3List.Add(fibBottom3);

                var signal = GetBollingerBandsSignal(currentValue - sma, prevValue - prevSma, currentValue, prevValue, fibTop3, prevFibTop3, fibBottom3, prevFibBottom3);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "UpperBand", fibTop3List },
                { "MiddleBand", smaList },
                { "LowerBand", fibBottom3List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.BollingerBandsFibonacciRatios;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bollinger Bands Average True Range
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="atrLength"></param>
        /// <param name="length"></param>
        /// <param name="stdDevMult"></param>
        /// <returns></returns>
        public static StockData CalculateBollingerBandsAvgTrueRange(this StockData stockData, MovingAvgType maType, int atrLength = 22, int length = 55, 
            decimal stdDevMult = 2)
        {
            List<decimal> atrDevList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var bollingerBands = CalculateBollingerBands(stockData, stdDevMult, maType, length);
            var upperBandList = bollingerBands.OutputValues["UpperBand"];
            var lowerBandList = bollingerBands.OutputValues["LowerBand"];
            var emaList = GetMovingAverageList(stockData, maType, atrLength, inputList);
            var atrList = CalculateAverageTrueRange(stockData, maType, atrLength).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal currentAtr = atrList.ElementAtOrDefault(i);
                decimal upperBand = upperBandList.ElementAtOrDefault(i);
                decimal lowerBand = lowerBandList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
                decimal bbDiff = upperBand - lowerBand;

                decimal atrDev = bbDiff != 0 ? currentAtr / bbDiff : 0;
                atrDevList.Add(atrDev);

                var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, atrDev, 0.5m);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "AtrDev", atrDevList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = atrDevList;
            stockData.IndicatorName = IndicatorName.BollingerBandsAverageTrueRange;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bollinger Bands using Atr Pct
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="bbLength"></param>
        /// <param name="stdDevMult"></param>
        /// <returns></returns>
        public static StockData CalculateBollingerBandsWithAtrPct(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 14, int bbLength = 20, decimal stdDevMult = 2)
        {
            List<decimal> aptrList = new();
            List<decimal> upperList = new();
            List<decimal> lowerList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            decimal ratio = (decimal)2 / (length + 1);

            var smaList = GetMovingAverageList(stockData, maType, bbLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal basis = smaList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal lh = currentHigh - currentLow;
                decimal hc = Math.Abs(currentHigh - prevClose);
                decimal lc = Math.Abs(currentLow - prevClose);
                decimal mm = Math.Max(Math.Max(lh, hc), lc);
                decimal prevBasis = i >= 1 ? smaList.ElementAtOrDefault(i - 1) : 0;
                decimal atrs = mm == hc ? hc / (prevClose + (hc / 2)) : mm == lc ? lc / (currentLow + (lc / 2)) : mm == lh ? lh / 
                    (currentLow + (lh / 2)) : 0;

                decimal prevAptr = aptrList.LastOrDefault();
                decimal aptr = (100 * atrs * ratio) + (prevAptr * (1 - ratio));
                aptrList.Add(aptr);

                decimal dev = stdDevMult * aptr;
                decimal prevUpper = upperList.LastOrDefault();
                decimal upper = basis + (basis * dev / 100);
                upperList.Add(upper);

                decimal prevLower = lowerList.LastOrDefault();
                decimal lower = basis - (basis * dev / 100);
                lowerList.Add(lower);

                var signal = GetBollingerBandsSignal(currentValue - basis, prevValue - prevBasis, currentValue, prevValue, upper, prevUpper, lower, prevLower);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "UpperBand", upperList },
                { "MiddleBand", smaList },
                { "LowerBand", lowerList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.BollingerBandsWithAtrPct;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bollinger Bands %B
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="stdDevMult"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateBollingerBandsPercentB(this StockData stockData, decimal stdDevMult = 2, 
            MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
        {
            List<decimal> pctBList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var bbList = CalculateBollingerBands(stockData, stdDevMult, maType, length);
            var upperBandList = bbList.OutputValues["UpperBand"];
            var lowerBandList = bbList.OutputValues["LowerBand"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal upperBand = upperBandList.ElementAtOrDefault(i);
                decimal lowerBand = lowerBandList.ElementAtOrDefault(i);
                decimal prevPctB1 = i >= 1 ? pctBList.ElementAtOrDefault(i - 1) : 0;
                decimal prevPctB2 = i >= 2 ? pctBList.ElementAtOrDefault(i - 2) : 0;

                decimal pctB = upperBand - lowerBand != 0 ? (currentValue - lowerBand) / (upperBand - lowerBand) * 100 : 0;
                pctBList.AddRounded(pctB);

                Signal signal = GetRsiSignal(pctB - prevPctB1, prevPctB1 - prevPctB2, pctB, prevPctB1, 100, 0);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "PctB", pctBList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = pctBList;
            stockData.IndicatorName = IndicatorName.BollingerBandsPercentB;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bollinger Bands Width
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="stdDevMult"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateBollingerBandsWidth(this StockData stockData, decimal stdDevMult = 2,
            MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
        {
            List<decimal> bbWidthList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var bbList = CalculateBollingerBands(stockData, stdDevMult, maType, length);
            var upperBandList = bbList.OutputValues["UpperBand"];
            var lowerBandList = bbList.OutputValues["LowerBand"];
            var middleBandList = bbList.OutputValues["MiddleBand"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal upperBand = upperBandList.ElementAtOrDefault(i);
                decimal lowerBand = lowerBandList.ElementAtOrDefault(i);
                decimal middleBand = middleBandList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMiddleBand = i >= 1 ? middleBandList.ElementAtOrDefault(i - 1) : 0;

                decimal prevBbWidth = bbWidthList.LastOrDefault();
                decimal bbWidth = middleBand != 0 ? (upperBand - lowerBand) / middleBand : 0;
                bbWidthList.AddRounded(bbWidth);

                Signal signal = GetVolatilitySignal(currentValue - middleBand, prevValue - prevMiddleBand, bbWidth, prevBbWidth);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "BbWidth", bbWidthList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bbWidthList;
            stockData.IndicatorName = IndicatorName.BollingerBandsWidth;

            return stockData;
        }
    }
}
