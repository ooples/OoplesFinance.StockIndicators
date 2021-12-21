using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;
using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
using static OoplesFinance.StockIndicators.Helpers.SignalHelper;
using static OoplesFinance.StockIndicators.Helpers.MathHelper;
using OoplesFinance.StockIndicators.Helpers;

namespace OoplesFinance.StockIndicators
{
    public static class Calculations
    {
        public static StockData CalculateMovingAverageConvergenceDivergence(this StockData stockData, MovingAvgType movingAvgType = MovingAvgType.ExponentialMovingAverage,
            int fastLength = 12, int slowLength = 26, int signalLength = 9)
        {
            List<decimal> macdList = new();
            List<decimal> macdHistogramList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            var fastEmaList = GetMovingAverageList(stockData, movingAvgType, fastLength, inputs.inputList);
            var slowEmaList = GetMovingAverageList(stockData, movingAvgType, slowLength, inputs.inputList);

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

        public static StockData CalculateExponentialMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> emaList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputs.inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputs.inputList.ElementAtOrDefault(i - 1) : 0;

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

        public static StockData CalculateRelativeStrengthIndex(this StockData stockData, MovingAvgType movingAvgType = MovingAvgType.ExponentialMovingAverage,
            int length = 14, int signalLength = 3)
        {
            List<decimal> rsiList = new();
            List<decimal> rsList = new();
            List<decimal> lossList = new();
            List<decimal> gainList = new();
            List<decimal> rsiHistogramList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputs.inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputs.inputList.ElementAtOrDefault(i - 1) : 0;
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

        public static StockData CalculateSimpleMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> smaList = new();
            List<decimal> tempList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevValue = tempList.LastOrDefault();
                decimal currentValue = inputs.inputList.ElementAtOrDefault(i);
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

        public static StockData CalculateTypicalPrice(this StockData stockData)
        {
            List<decimal> tpList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = inputs.highList.ElementAtOrDefault(i);
                decimal currentLow = inputs.lowList.ElementAtOrDefault(i);
                decimal currentClose = inputs.inputList.ElementAtOrDefault(i);
                decimal prevTypicalPrice1 = i >= 1 ? tpList.ElementAtOrDefault(i - 1) : 0;
                decimal prevTypicalPrice2 = i >= 2 ? tpList.ElementAtOrDefault(i - 2) : 0;

                decimal typicalPrice = (currentHigh + currentLow + currentClose) / 3;
                tpList.AddRounded(typicalPrice);

                Signal signal = GetCompareSignal(typicalPrice - prevTypicalPrice1, prevTypicalPrice1 - prevTypicalPrice2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Tp", tpList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tpList;
            stockData.IndicatorName = IndicatorName.TypicalPrice;

            return stockData;
        }

        public static StockData CalculateMedianPrice(this StockData stockData)
        {
            List<decimal> medianPriceList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentLow = inputs.lowList.ElementAtOrDefault(i);
                decimal currentHigh = inputs.highList.ElementAtOrDefault(i);
                decimal prevMedianPrice1 = i >= 1 ? medianPriceList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMedianPrice2 = i >= 2 ? medianPriceList.ElementAtOrDefault(i - 2) : 0;

                decimal medianPrice = (currentHigh + currentLow) / 2;
                medianPriceList.AddRounded(medianPrice);

                Signal signal = GetCompareSignal(medianPrice - prevMedianPrice1, prevMedianPrice1 - prevMedianPrice2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "MedianPrice", medianPriceList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = medianPriceList;
            stockData.IndicatorName = IndicatorName.MedianPrice;

            return stockData;
        }

        public static StockData CalculateFullTypicalPrice(this StockData stockData)
        {
            List<decimal> fullTpList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = inputs.highList.ElementAtOrDefault(i);
                decimal currentLow = inputs.lowList.ElementAtOrDefault(i);
                decimal currentClose = inputs.inputList.ElementAtOrDefault(i);
                decimal currentOpen = inputs.openList.ElementAtOrDefault(i);
                decimal prevTypicalPrice1 = i >= 1 ? fullTpList.ElementAtOrDefault(i - 1) : 0;
                decimal prevTypicalPrice2 = i >= 2 ? fullTpList.ElementAtOrDefault(i - 2) : 0;

                decimal typicalPrice = (currentHigh + currentLow + currentClose + currentOpen) / 4;
                fullTpList.AddRounded(typicalPrice);

                Signal signal = GetCompareSignal(typicalPrice - prevTypicalPrice1, prevTypicalPrice1 - prevTypicalPrice2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "FullTp", fullTpList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = fullTpList;
            stockData.IndicatorName = IndicatorName.FullTypicalPrice;

            return stockData;
        }

        public static StockData CalculateWeightedClose(this StockData stockData)
        {
            List<decimal> weightedCloseList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = inputs.inputList.ElementAtOrDefault(i);
                decimal currentLow = inputs.lowList.ElementAtOrDefault(i);
                decimal currentClose = inputs.inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputs.inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevWeightedClose = weightedCloseList.LastOrDefault();
                decimal weightedClose = (currentHigh + currentLow + (currentClose * 2)) / 4;
                weightedCloseList.AddRounded(weightedClose);

                Signal signal = GetCompareSignal(currentClose - weightedClose, prevClose - prevWeightedClose);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "WeightedClose", weightedCloseList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = weightedCloseList;
            stockData.IndicatorName = IndicatorName.WeightedClose;

            return stockData;
        }

        public static StockData CalculateWeightedMovingAverage(this StockData stockData, int days = 14)
        {
            List<decimal> wmaList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputs.inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputs.inputList.ElementAtOrDefault(i - 1) : 0;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j < days; j++)
                {
                    decimal weight = days - j;
                    decimal prevValue = i >= j ? inputs.inputList.ElementAtOrDefault(i - j) : 0;

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

        public static StockData CalculateStandardDeviationVolatility(this StockData stockData, int length = 20)
        {
            List<decimal> stdDevVolatilityList = new();
            List<decimal> deviationSquaredList = new();
            List<decimal> divisionOfSumList = new();
            List<decimal> stdDevEmaList = new();
            List<Signal> signalsList = new();

            var inputs = GetInputValuesList(stockData);
            var smaList = GetMovingAverageList(stockData, MovingAvgType.SimpleMovingAverage, length, inputs.inputList);
            var emaList = GetMovingAverageList(stockData, MovingAvgType.ExponentialMovingAverage, length, inputs.inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal currentValue = inputs.inputList.ElementAtOrDefault(i);
                decimal avgPrice = smaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputs.inputList.ElementAtOrDefault(i - 1) : 0;
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

        public static StockData CalculateBollingerBands(this StockData stockData, decimal stdDevMult = 2, MovingAvgType movingAvgType = MovingAvgType.SimpleMovingAverage, int length = 20)
        {
            List<decimal> upperBandList = new();
            List<decimal> lowerBandList = new();
            List<Signal> signalsList = new();
            var inputs = GetInputValuesList(stockData);
            var smaList = GetMovingAverageList(stockData, movingAvgType, length, inputs.inputList);
            var stdDeviationList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal middleBand = smaList.ElementAtOrDefault(i);
                decimal currentValue = inputs.inputList.ElementAtOrDefault(i);
                decimal currentStdDeviation = stdDeviationList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputs.inputList.ElementAtOrDefault(i - 1) : 0;
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
