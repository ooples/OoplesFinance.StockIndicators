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
        /// Calculates the moving average convergence divergence.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateMovingAverageConvergenceDivergence(this StockData stockData)
        {
            int fastLength = 12, slowLength = 26, signalLength = 9;

            return CalculateMovingAverageConvergenceDivergence(stockData, MovingAvgType.ExponentialMovingAverage, fastLength, slowLength, signalLength);
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

        public static StockData Calculate4MovingAverageConvergenceDivergence(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 5, int length2 = 8, int length3 = 10, int length4 = 17,
            int length5 = 14, int length6 = 16, decimal blueMult = 4.3m, decimal yellowMult = 1.4m)
        {
            List<decimal> macd1List = new();
            List<decimal> macd2List = new();
            List<decimal> macd3List = new();
            List<decimal> macd4List = new();
            List<decimal> macd2HistogramList = new();
            List<decimal> macd4HistogramList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema5List = GetMovingAverageList(stockData, maType, length1, inputList);
            var ema8List = GetMovingAverageList(stockData, maType, length2, inputList);
            var ema10List = GetMovingAverageList(stockData, maType, length3, inputList);
            var ema17List = GetMovingAverageList(stockData, maType, length4, inputList);
            var ema14List = GetMovingAverageList(stockData, maType, length5, inputList);
            var ema16List = GetMovingAverageList(stockData, maType, length6, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ema5 = ema5List.ElementAtOrDefault(i);
                decimal ema8 = ema8List.ElementAtOrDefault(i);
                decimal ema10 = ema10List.ElementAtOrDefault(i);
                decimal ema14 = ema14List.ElementAtOrDefault(i);
                decimal ema16 = ema16List.ElementAtOrDefault(i);
                decimal ema17 = ema17List.ElementAtOrDefault(i);

                decimal macd1 = ema17 - ema14;
                macd1List.Add(macd1);

                decimal macd2 = ema17 - ema8;
                macd2List.Add(macd2);

                decimal macd3 = ema10 - ema16;
                macd3List.Add(macd3);

                decimal macd4 = ema5 - ema10;
                macd4List.Add(macd4);
            }

            var macd1SignalLineList = GetMovingAverageList(stockData, maType, length1, macd1List);
            var macd2SignalLineList = GetMovingAverageList(stockData, maType, length1, macd2List);
            var macd3SignalLineList = GetMovingAverageList(stockData, maType, length1, macd3List);
            var macd4SignalLineList = GetMovingAverageList(stockData, maType, length1, macd4List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal macd1 = macd1List.ElementAtOrDefault(i);
                decimal macd1SignalLine = macd1SignalLineList.ElementAtOrDefault(i);
                decimal macd2 = macd2List.ElementAtOrDefault(i);
                decimal macd2SignalLine = macd2SignalLineList.ElementAtOrDefault(i);
                decimal macd3 = macd3List.ElementAtOrDefault(i);
                decimal macd3SignalLine = macd3SignalLineList.ElementAtOrDefault(i);
                decimal macd4 = macd4List.ElementAtOrDefault(i);
                decimal macd4SignalLine = macd4SignalLineList.ElementAtOrDefault(i);
                decimal macd1Histogram = macd1 - macd1SignalLine;
                decimal macdBlue = blueMult * macd1Histogram;

                decimal prevMacd2Histogram = macd2HistogramList.LastOrDefault();
                decimal macd2Histogram = macd2 - macd2SignalLine;
                macd2HistogramList.Add(macd2Histogram);

                decimal macd3Histogram = macd3 - macd3SignalLine;
                decimal macdYellow = yellowMult * macd3Histogram;

                decimal prevMacd4Histogram = macd4HistogramList.LastOrDefault();
                decimal macd4Histogram = macd4 - macd4SignalLine;
                macd4HistogramList.Add(macd4Histogram);

                decimal maxMacd = Math.Max(macdBlue, Math.Max(macdYellow, Math.Max(macd2Histogram, macd4Histogram)));
                decimal minMacd = Math.Min(macdBlue, Math.Min(macdYellow, Math.Min(macd2Histogram, macd4Histogram)));
                decimal currentMacd = (macdBlue + macdYellow + macd2Histogram + macd4Histogram) / 4;
                decimal macdStochastic = maxMacd - minMacd != 0 ? MinOrMax((currentMacd - minMacd) / (maxMacd - minMacd) * 100, 100, 0) : 0;

                var signal = GetCompareSignal(macd4Histogram - macd2Histogram, prevMacd4Histogram - prevMacd2Histogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Macd1", macd4List },
                { "Signal1", macd4SignalLineList },
                { "Histogram1", macd4HistogramList },
                { "Macd2", macd2List },
                { "Signal2", macd2SignalLineList },
                { "Histogram2", macd2HistogramList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName._4MovingAverageConvergenceDivergence;

            return stockData;
        }
    }
}
