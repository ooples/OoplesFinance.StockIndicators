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
        /// Calculates the relative strength index.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateRelativeStrengthIndex(this StockData stockData)
        {
            return CalculateRelativeStrengthIndex(stockData, MovingAvgType.ExponentialMovingAverage, 14, 3);
        }

        /// <summary>
        /// Calculates the relative strength index.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="movingAvgType">Average type of the moving.</param>
        /// <param name="length">The length.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateRelativeStrengthIndex(this StockData stockData, MovingAvgType movingAvgType, int length, int signalLength)
        {
            List<decimal> rsiList = new();
            List<decimal> rsList = new();
            List<decimal> lossList = new();
            List<decimal> gainList = new();
            List<decimal> rsiHistogramList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priceChg = currentValue - prevValue;

                decimal loss = priceChg < 0 ? Math.Abs(priceChg) : 0;
                lossList.AddRounded(loss);

                decimal gain = priceChg > 0 ? priceChg : 0;
                gainList.AddRounded(gain);
            }

            var avgGainList = GetMovingAverageList(stockData, movingAvgType, length, gainList);
            var avgLossList = GetMovingAverageList(stockData, movingAvgType, length, lossList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal avgGain = avgGainList.ElementAtOrDefault(i);
                decimal avgLoss = avgLossList.ElementAtOrDefault(i);

                decimal rs = avgLoss != 0 ? MinOrMax(avgGain / avgLoss, 1, 0) : 0;
                rsList.AddRounded(rs);

                decimal rsi = avgLoss == 0 ? 100 : avgGain == 0 ? 0 : MinOrMax(100 - (100 / (1 + rs)), 100, 0);
                rsiList.AddRounded(rsi);
            }

            var rsiSignalList = GetMovingAverageList(stockData, movingAvgType, signalLength, rsiList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal rsi = rsiList.ElementAtOrDefault(i);
                decimal prevRsi = i >= 1 ? rsiList.ElementAtOrDefault(i - 1) : 0;
                decimal rsiSignal = rsiSignalList.ElementAtOrDefault(i);

                decimal prevRsiHistogram = rsiHistogramList.LastOrDefault();
                decimal rsiHistogram = rsi - rsiSignal;
                rsiHistogramList.AddRounded(rsiHistogram);

                var signal = GetRsiSignal(rsiHistogram, prevRsiHistogram, rsi, prevRsi, 70, 30);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Rsi", rsiList },
                { "Signal", rsiSignalList },
                { "Histogram", rsiHistogramList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = rsiList;
            stockData.IndicatorName = IndicatorName.RelativeStrengthIndex;

            return stockData;
        }
    }
}
