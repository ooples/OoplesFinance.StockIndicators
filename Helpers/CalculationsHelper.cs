using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Exceptions;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Helpers.MathHelper;

namespace OoplesFinance.StockIndicators.Helpers
{
    public static class CalculationsHelper
    {
        public static List<decimal> GetMovingAverageList(StockData stockData, MovingAvgType movingAvgType, int length, List<decimal>? customValuesList = null)
        {
            List<decimal> movingAvgList = new();

            if (customValuesList != null)
            {
                stockData.CustomValuesList = customValuesList;
            }

            switch (movingAvgType)
            {
                case MovingAvgType.ExponentialMovingAverage:
                    movingAvgList = stockData.CalculateExponentialMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.SimpleMovingAverage:
                    movingAvgList = stockData.CalculateSimpleMovingAverage(length).CustomValuesList;
                    break;
                case MovingAvgType.WeightedMovingAverage:
                    movingAvgList = stockData.CalculateWeightedMovingAverage(length).CustomValuesList;
                    break;
                default:
                    Console.WriteLine($"Moving Avg Name: {movingAvgType} not supported!");
                    break;
            }

            return movingAvgList;
        }

        public static List<decimal> GetInputValuesList(InputName inputName, StockData stockData)
        {
            return inputName switch
            {
                InputName.Open => stockData.OpenPrices,
                InputName.Close => stockData.ClosePrices,
                InputName.High => stockData.HighPrices,
                InputName.Low => stockData.LowPrices,
                InputName.Volume => stockData.Volumes,
                InputName.MedianPrice => stockData.CalculateMedianPrice().CustomValuesList,
                InputName.TypicalPrice => stockData.CalculateTypicalPrice().CustomValuesList,
                InputName.FullTypicalPrice => stockData.CalculateFullTypicalPrice().CustomValuesList,
                InputName.WeightedClose => stockData.CalculateWeightedClose().CustomValuesList,
                _ => stockData.ClosePrices,
            };
        }

        public static (List<decimal> inputList, List<decimal> highList, List<decimal> lowList, List<decimal> openList, List<decimal> volumeList) GetInputValuesList(StockData stockData)
        {
            List<decimal> inputList;
            List<decimal> highList;
            List<decimal> lowList;
            List<decimal> openList;
            List<decimal> volumeList;

            if (stockData.CustomValuesList != null && stockData.CustomValuesList.Count > 0)
            {
                inputList = stockData.CustomValuesList;
            }
            else if ((stockData.CustomValuesList == null || (stockData.CustomValuesList != null && stockData.CustomValuesList.Count == 0)) &&
                stockData.SignalsList != null && stockData.SignalsList.Count > 0)
            {
                throw new CalculationException($"Calculations based off of {stockData.IndicatorName} can't be completed because this indicator doesn't have a single output.");
            }
            else
            {
                inputList = stockData.InputValues;
            }

            if (inputList.Count > 0)
            {
                decimal sum = inputList.Sum();

                if (inputList.SequenceEqual(stockData.Volumes) || sum < stockData.LowPrices.Sum() || sum > stockData.HighPrices.Sum())
                {
                    var minMaxList = GetMaxAndMinValuesList(inputList, 0);
                    highList = minMaxList.Item1;
                    lowList = minMaxList.Item2;
                }
                else
                {
                    highList = stockData.HighPrices;
                    lowList = stockData.LowPrices;
                }
            }
            else
            {
                highList = stockData.HighPrices;
                lowList = stockData.LowPrices;
            }

            openList = stockData.OpenPrices;
            volumeList = stockData.Volumes;

            return (inputList, highList, lowList, openList, volumeList);
        }

        public static decimal CalculateEMA(decimal currentValue, decimal prevEma, int length = 14)
        {
            decimal k = MinOrMax((decimal)2 / (length + 1), 0.99m, 0.01m);
            decimal ema = (currentValue * k) + (prevEma * (1 - k));

            return ema;
        }

        public static (List<decimal>, List<decimal>) GetMaxAndMinValuesList(List<decimal> inputs, int days)
        {
            List<decimal> highestValuesList = new();
            List<decimal> lowestValuesList = new();
            List<decimal> inputList = new();

            for (int i = 0; i < inputs.Count; i++)
            {
                decimal input = inputs.ElementAt(i);
                inputList.Add(input);

                var list = inputList.TakeLast(Math.Max(days, 2)).ToList();

                decimal highestValue = list.Max();
                highestValuesList.Add(highestValue);

                decimal lowestValue = list.Min();
                lowestValuesList.Add(lowestValue);
            }

            return (highestValuesList, lowestValuesList);
        }

        public static void AddRounded(this List<decimal> list, decimal value)
        {
            list.Add(Math.Round(value, 4));
        }
    }
}
