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
        /// Calculates the standard deviation volatility.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateStandardDeviationVolatility(this StockData stockData)
        {
            return CalculateStandardDeviationVolatility(stockData, 20);
        }

        /// <summary>
        /// Calculates the standard deviation volatility.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateStandardDeviationVolatility(this StockData stockData, int length)
        {
            List<decimal> stdDevVolatilityList = new();
            List<decimal> deviationSquaredList = new();
            List<decimal> divisionOfSumList = new();
            List<decimal> stdDevEmaList = new();
            List<Signal> signalsList = new();

            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            var smaList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length, inputList);
            var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal avgPrice = smaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
                decimal currentDeviation = currentValue - avgPrice;

                decimal deviationSquared = Pow(currentDeviation, 2);
                deviationSquaredList.AddRounded(deviationSquared);

                decimal divisionOfSum = deviationSquaredList.TakeLast(length).Average();
                divisionOfSumList.AddRounded(divisionOfSum);

                decimal stdDevVolatility = divisionOfSum >= 0 ? Sqrt(divisionOfSum) : 0;
                stdDevVolatilityList.AddRounded(stdDevVolatility);

                decimal stdDevEma = CalculateEMA(stdDevVolatility, stdDevEmaList.LastOrDefault(), length);
                stdDevEmaList.AddRounded(stdDevEma);

                Signal signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, stdDevVolatility, stdDevEma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "StdDev", stdDevVolatilityList },
                { "Signal", stdDevEmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = stdDevVolatilityList;
            stockData.IndicatorName = IndicatorName.StandardDeviationVolatility;

            return stockData;
        }
    }
}
