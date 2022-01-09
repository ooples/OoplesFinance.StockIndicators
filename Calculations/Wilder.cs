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
        /// Calculates the average true range.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateAverageTrueRange(this StockData stockData)
        {
            var maType = MovingAvgType.ExponentialMovingAverage;
            var length = 14;

            return CalculateAverageTrueRange(stockData, maType, length);
        }

        /// <summary>
        /// Calculates the average true range.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAverageTrueRange(this StockData stockData, MovingAvgType maType, int length)
        {
            List<decimal> trList = new();
            List<Signal> signalsList = new();

            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var emaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal currentTrueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);
                trList.AddRounded(currentTrueRange);
            }

            var atrList = GetMovingAverageList(stockData, maType, length, trList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal atr = atrList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
                decimal prevAtr = i >= 1 ? atrList.ElementAtOrDefault(i - 1) : 0;
                decimal atrEma = CalculateEMA(atr, prevAtr, length);

                var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, atr, atrEma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Atr", atrList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = atrList;
            stockData.IndicatorName = IndicatorName.AverageTrueRange;

            return stockData;
        }

        /// <summary>
        /// Calculates the average index of the directional.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateAverageDirectionalIndex(this StockData stockData)
        {
            var maType = MovingAvgType.ExponentialMovingAverage;
            int length = 14;

            return CalculateAverageDirectionalIndex(stockData, maType, length);
        }

        /// <summary>
        /// Calculates the average index of the directional.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAverageDirectionalIndex(this StockData stockData, MovingAvgType maType, int length)
        {
            List<decimal> dmPlusList = new();
            List<decimal> dmMinusList = new();
            List<decimal> diPlus14List = new();
            List<decimal> diMinus14List = new();
            List<decimal> trList = new();
            List<decimal> diList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal highDiff = currentHigh - prevHigh;
                decimal lowDiff = prevLow - currentLow;

                decimal dmPlus = highDiff > lowDiff ? Math.Max(highDiff, 0) : 0;
                dmPlusList.AddRounded(dmPlus);

                decimal dmMinus = highDiff < lowDiff ? Math.Max(lowDiff, 0) : 0;
                dmMinusList.AddRounded(dmMinus);

                decimal tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
                trList.AddRounded(tr);
            }

            var dmPlus14List = GetMovingAverageList(stockData, maType, length, dmPlusList);
            var dmMinus14List = GetMovingAverageList(stockData, maType, length, dmMinusList);
            var tr14List = GetMovingAverageList(stockData, maType, length, trList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal dmPlus14 = dmPlus14List.ElementAtOrDefault(i);
                decimal dmMinus14 = dmMinus14List.ElementAtOrDefault(i);
                decimal trueRange14 = tr14List.ElementAtOrDefault(i);
                decimal diPlus = trueRange14 != 0 ? MinOrMax(100 * dmPlus14 / trueRange14, 100, 0) : 0;
                decimal diMinus = trueRange14 != 0 ? MinOrMax(100 * dmMinus14 / trueRange14, 100, 0) : 0;
                decimal diDiff = Math.Abs(diPlus - diMinus);
                decimal diSum = diPlus + diMinus;

                decimal di = diSum != 0 ? MinOrMax(100 * diDiff / diSum, 100, 0) : 0;
                diList.AddRounded(di);
            }

            var adxList = GetMovingAverageList(stockData, maType, length, diList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal diPlus = diPlus14List.ElementAtOrDefault(i);
                decimal diMinus = diMinus14List.ElementAtOrDefault(i);
                decimal prevDiPlus = i >= 1 ? diPlus14List.ElementAtOrDefault(i - 1) : 0;
                decimal prevDiMinus = i >= 1 ? diMinus14List.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(diPlus - diMinus, prevDiPlus - prevDiMinus);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "DiPlus", diPlus14List },
                { "DiMinus", diMinus14List },
                { "Adx", adxList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = adxList;
            stockData.IndicatorName = IndicatorName.AverageDirectionalIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the Welles Wilder Volatility System
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static StockData CalculateWellesWilderVolatilitySystem(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 63, int length2 = 21, decimal factor = 3)
        {
            List<decimal> vstopList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;
            var emaList = GetMovingAverageList(stockData, maType, length1, inputList);
            var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length2);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentAtr = atrList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevVStop = vstopList.LastOrDefault();
                decimal sic = currentValue > currentEma ? highest : lowest;
                decimal vstop = currentValue > currentEma ? sic - (factor * currentAtr) : sic + (factor * currentAtr);
                vstopList.Add(vstop);

                var signal = GetCompareSignal(currentValue - vstop, prevValue - prevVStop);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wwvs", vstopList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = vstopList;
            stockData.IndicatorName = IndicatorName.WellesWilderVolatilitySystem;

            return stockData;
        }
    }
}
