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
        public static StockData CalculateBollingerBands(this StockData stockData, decimal stdDevMult, MovingAvgType maType, int length)
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
    }
}
