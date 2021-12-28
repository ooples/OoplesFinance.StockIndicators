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
        public static StockData CalculateBollingerBands(this StockData stockData, decimal stdDevMult, MovingAvgType movingAvgType, int length)
        {
            List<decimal> upperBandList = new();
            List<decimal> lowerBandList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            var smaList = GetMovingAverageList(stockData, movingAvgType, length, inputList);
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
    }
}
