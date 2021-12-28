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
        /// Calculates the exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateExponentialMovingAverage(this StockData stockData)
        {
            return CalculateExponentialMovingAverage(stockData, 14);
        }

        /// <summary>
        /// Calculates the exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateExponentialMovingAverage(this StockData stockData, int length)
        {
            List<decimal> emaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevEma = emaList.LastOrDefault();
                decimal ema = CalculateEMA(currentValue, prevEma, length);
                emaList.AddRounded(ema);

                var signal = GetCompareSignal(currentValue - ema, prevValue - prevEma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ema", emaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = emaList;
            stockData.IndicatorName = IndicatorName.ExponentialMovingAverage;

            return stockData;
        }
    }
}
