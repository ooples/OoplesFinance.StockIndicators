using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Helpers;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;
using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
using static OoplesFinance.StockIndicators.Helpers.SignalHelper;

namespace OoplesFinance.StockIndicators
{
    public static partial class Calculations
    {
        /// <summary>
        /// Calculates the moving average convergence divergence.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateMovingAverageConvergenceDivergence(this StockData stockData)
        {
            return CalculateMovingAverageConvergenceDivergence(stockData, MovingAvgType.ExponentialMovingAverage, 12, 26, 9);
        }

        /// <summary>
        /// Calculates the moving average convergence divergence.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="movingAvgType">Average type of the moving.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateMovingAverageConvergenceDivergence(this StockData stockData, MovingAvgType movingAvgType, int fastLength, int slowLength, int signalLength)
        {
            List<decimal> macdList = new();
            List<decimal> macdHistogramList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var fastEmaList = GetMovingAverageList(stockData, movingAvgType, fastLength, inputList);
            var slowEmaList = GetMovingAverageList(stockData, movingAvgType, slowLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal fastEma = fastEmaList.ElementAtOrDefault(i);
                decimal slowEma = slowEmaList.ElementAtOrDefault(i);

                decimal macd = fastEma - slowEma;
                macdList.AddRounded(macd);
            }

            var macdSignalLineList = GetMovingAverageList(stockData, movingAvgType, signalLength, macdList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal macd = macdList.ElementAtOrDefault(i);
                decimal macdSignalLine = macdSignalLineList.ElementAtOrDefault(i);

                decimal prevMacdHistogram = macdHistogramList.LastOrDefault();
                decimal macdHistogram = macd - macdSignalLine;
                macdHistogramList.AddRounded(macdHistogram);

                var signal = GetCompareSignal(macdHistogram, prevMacdHistogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Macd", macdList },
                { "Signal", macdSignalLineList },
                { "Histogram", macdHistogramList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = macdList;
            stockData.IndicatorName = IndicatorName.MovingAverageConvergenceDivergence;

            return stockData;
        }
    }
}
