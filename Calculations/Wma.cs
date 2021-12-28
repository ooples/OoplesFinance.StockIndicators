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
        /// Calculates the weighted moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateWeightedMovingAverage(this StockData stockData)
        {
            return CalculateWeightedMovingAverage(stockData, 14);
        }

        /// <summary>
        /// Calculates the weighted moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateWeightedMovingAverage(this StockData stockData, int length)
        {
            List<decimal> wmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j < length; j++)
                {
                    decimal weight = length - j;
                    decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal prevWma = wmaList.LastOrDefault();
                decimal wma = weightedSum != 0 ? sum / weightedSum : 0;
                wmaList.AddRounded(wma);

                Signal signal = GetCompareSignal(currentValue - wma, prevVal - prevWma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wma", wmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = wmaList;
            stockData.IndicatorName = IndicatorName.WeightedMovingAverage;

            return stockData;
        }
    }
}
