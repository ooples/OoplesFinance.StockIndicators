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
        /// Calculates the simple moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateSimpleMovingAverage(this StockData stockData)
        {
            return CalculateSimpleMovingAverage(stockData, 14);
        }

        /// <summary>
        /// Calculates the simple moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateSimpleMovingAverage(this StockData stockData, int length)
        {
            List<decimal> smaList = new();
            List<decimal> tempList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevValue = tempList.LastOrDefault();
                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.AddRounded(currentValue);

                decimal prevSma = smaList.LastOrDefault();
                decimal sma = tempList.TakeLast(length).Average();
                smaList.AddRounded(sma);

                Signal signal = GetCompareSignal(currentValue - sma, prevValue - prevSma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Sma", smaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = smaList;
            stockData.IndicatorName = IndicatorName.SimpleMovingAverage;

            return stockData;
        }
    }
}
