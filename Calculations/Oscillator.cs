using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;
using static OoplesFinance.StockIndicators.Helpers.SignalHelper;
using static OoplesFinance.StockIndicators.Helpers.CalculationsHelper;
using static OoplesFinance.StockIndicators.Helpers.MathHelper;
using OoplesFinance.StockIndicators.Enums;

namespace OoplesFinance.StockIndicators
{
    public static partial class Calculations
    {
        /// <summary>
        /// Calculates the index of the commodity channel.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateCommodityChannelIndex(this StockData stockData)
        {
            int length = 20;
            var maType = MovingAvgType.SimpleMovingAverage;
            var inputType = InputName.TypicalPrice;
            var constant = 0.015m;

            return CalculateCommodityChannelIndex(stockData, length, maType, inputType, constant);
        }

        /// <summary>
        /// Calculates the index of the commodity channel.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <returns></returns>
        public static StockData CalculateCommodityChannelIndex(this StockData stockData, int length, MovingAvgType maType)
        {
            var inputType = InputName.TypicalPrice;
            var constant = 0.015m;

            return CalculateCommodityChannelIndex(stockData, length, maType, inputType, constant);
        }

        /// <summary>
        /// Calculates the index of the commodity channel.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="constant">The constant.</param>
        /// <returns></returns>
        public static StockData CalculateCommodityChannelIndex(this StockData stockData, int length, MovingAvgType maType, InputName inputName, 
            decimal constant)
        {
            List<decimal> cciList = new();
            List<decimal> tpDevDiffList = new();
            List<Signal> signalsList = new();

            var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);
            var tpSmaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal tpSma = tpSmaList.ElementAtOrDefault(i);

                decimal tpDevDiff = Math.Abs(currentValue - tpSma);
                tpDevDiffList.Add(tpDevDiff);
            }

            var tpMeanDevList = GetMovingAverageList(stockData, maType, length, tpDevDiffList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevCci1 = i >= 1 ? cciList.ElementAtOrDefault(i - 1) : 0;
                decimal prevCci2 = i >= 2 ? cciList.ElementAtOrDefault(i - 2) : 0;
                decimal tpMeanDev = tpMeanDevList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal tpSma = tpSmaList.ElementAtOrDefault(i);

                decimal cci = tpMeanDev != 0 ? (currentValue - tpSma) / (constant * tpMeanDev) : 0;
                cciList.Add(cci);

                var signal = GetRsiSignal(cci - prevCci1, prevCci1 - prevCci2, cci, prevCci1, 100, -100);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Cci", cciList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = cciList;
            stockData.IndicatorName = IndicatorName.CommodityChannelIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the awesome oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateAwesomeOscillator(this StockData stockData)
        {
            int fastLength = 5, slowLength = 34;
            var maType = MovingAvgType.SimpleMovingAverage;
            var inputName = InputName.MedianPrice;

            return CalculateAwesomeOscillator(stockData, maType, inputName, fastLength, slowLength);
        }

        /// <summary>
        /// Calculates the awesome oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <returns></returns>
        public static StockData CalculateAwesomeOscillator(this StockData stockData, MovingAvgType maType, InputName inputName, 
            int fastLength, int slowLength)
        {
            List<decimal> aoList = new();
            List<Signal> signalsList = new();

            var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);
            var fastSmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
            var slowSmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal fastSma = fastSmaList.ElementAtOrDefault(i);
                decimal slowSma = slowSmaList.ElementAtOrDefault(i);

                decimal prevAo = aoList.LastOrDefault();
                decimal ao = fastSma - slowSma;
                aoList.Add(ao);

                var signal = GetCompareSignal(ao, prevAo);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ao", aoList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = aoList;
            stockData.IndicatorName = IndicatorName.AwesomeOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the accelerator oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateAcceleratorOscillator(this StockData stockData)
        {
            int fastLength = 5, slowLength = 34, smoothLength = 5;
            var maType = MovingAvgType.SimpleMovingAverage;
            var inputName = InputName.MedianPrice;

            return CalculateAcceleratorOscillator(stockData, maType, inputName, fastLength, slowLength, smoothLength);
        }

        /// <summary>
        /// Calculates the accelerator oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="smoothLength">Length of the smooth.</param>
        /// <returns></returns>
        public static StockData CalculateAcceleratorOscillator(this StockData stockData, MovingAvgType maType, InputName inputName,
            int fastLength, int slowLength, int smoothLength)
        {
            List<decimal> acList = new();
            List<Signal> signalsList = new();

            var awesomeOscList = CalculateAwesomeOscillator(stockData, maType, inputName, fastLength, slowLength).CustomValuesList;
            var awesomeOscMaList = GetMovingAverageList(stockData, maType, smoothLength, awesomeOscList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ao = awesomeOscList.ElementAtOrDefault(i);
                decimal aoSma = awesomeOscMaList.ElementAtOrDefault(i);

                decimal prevAc = acList.LastOrDefault();
                decimal ac = ao - aoSma;
                acList.Add(ac);

                var signal = GetCompareSignal(ac, prevAc);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ac", acList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = acList;
            stockData.IndicatorName = IndicatorName.AcceleratorOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the choppiness index.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateChoppinessIndex(this StockData stockData)
        {
            var maType = MovingAvgType.ExponentialMovingAverage;
            var length = 14;

            return CalculateChoppinessIndex(stockData, maType, length);
        }

        /// <summary>
        /// Calculates the choppiness index.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateChoppinessIndex(this StockData stockData, MovingAvgType maType, int length)
        {
            List<decimal> ciList = new();
            List<decimal> trList = new();
            List<Signal> signalsList = new();

            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestHighList, lowestLowList) = GetMaxAndMinValuesList(highList, lowList, length);
            var emaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;
                decimal highestHigh = highestHighList.ElementAtOrDefault(i);
                decimal lowestLow = lowestLowList.ElementAtOrDefault(i);
                decimal range = highestHigh - lowestLow;

                decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
                trList.Add(tr);

                decimal trSum = trList.TakeLastExt(length).Sum();
                decimal ci = range > 0 ? 100 * Log10(trSum / range) / Log10((double)length) : 0;
                ciList.Add(ci);

                var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, ci, 38.2m);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ci", ciList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ciList;
            stockData.IndicatorName = IndicatorName.ChoppinessIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the ulcer index.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateUlcerIndex(this StockData stockData)
        {
            int length = 14;

            return CalculateUlcerIndex(stockData, length);
        }

        /// <summary>
        /// Calculates the ulcer index.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateUlcerIndex(this StockData stockData, int length)
        {
            List<decimal> ulcerIndexList = new();
            List<decimal> pctDrawdownSquaredList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            var (highestList, _) = GetMaxAndMinValuesList(inputList, length);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal maxValue = highestList.ElementAtOrDefault(i);
                decimal prevUlcerIndex1 = i >= 1 ? ulcerIndexList.ElementAtOrDefault(i - 1) : 0;
                decimal prevUlcerIndex2 = i >= 2 ? ulcerIndexList.ElementAtOrDefault(i - 2) : 0;

                decimal pctDrawdownSquared = maxValue != 0 ? Pow((currentValue - maxValue) / maxValue * 100, 2) : 0;
                pctDrawdownSquaredList.Add(pctDrawdownSquared);

                decimal squaredAvg = pctDrawdownSquaredList.TakeLastExt(length).Average();

                decimal ulcerIndex = squaredAvg >= 0 ? Sqrt((double)squaredAvg) : 0;
                ulcerIndexList.Add(ulcerIndex);

                var signal = GetCompareSignal(ulcerIndex - prevUlcerIndex1, prevUlcerIndex1 - prevUlcerIndex2, true);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ui", ulcerIndexList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ulcerIndexList;
            stockData.IndicatorName = IndicatorName.UlcerIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the force.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateForceIndex(this StockData stockData)
        {
            var maType = MovingAvgType.ExponentialMovingAverage;
            var length = 14;

            return CalculateForceIndex(stockData, maType, length);
        }

        /// <summary>
        /// Calculates the index of the force.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateForceIndex(this StockData stockData, MovingAvgType maType, int length)
        {
            List<decimal> rawForceList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal rawForce = (currentValue - prevValue) * currentVolume;
                rawForceList.Add(rawForce);
            }

            var forceList = GetMovingAverageList(stockData, maType, length, rawForceList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal force = forceList.ElementAtOrDefault(i);
                decimal prevForce1 = i >= 1 ? forceList.ElementAtOrDefault(i - 1) : 0;
                decimal prevForce2 = i >= 2 ? forceList.ElementAtOrDefault(i - 2) : 0;

                var signal = GetCompareSignal(force - prevForce1, prevForce1 - prevForce2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Fi", forceList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = forceList;
            stockData.IndicatorName = IndicatorName.ForceIndex;

            return stockData;
        }

        public static StockData CalculateMoneyFlowIndex(this StockData stockData)
        {
            var inputName = InputName.TypicalPrice;
            var length = 14;

            return CalculateMoneyFlowIndex(stockData, inputName, length);
        }

        /// <summary>
        /// Calculates the index of the money flow.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateMoneyFlowIndex(this StockData stockData, InputName inputName, int length)
        {
            List<decimal> mfiList = new();
            List<decimal> posMoneyFlowList = new();
            List<decimal> negMoneyFlowList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal typicalPrice = inputList.ElementAtOrDefault(i);
                decimal prevTypicalPrice = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMfi1 = i >= 1 ? mfiList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMfi2 = i >= 2 ? mfiList.ElementAtOrDefault(i - 2) : 0;
                decimal rawMoneyFlow = typicalPrice * currentVolume;

                decimal posMoneyFlow = typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
                posMoneyFlowList.Add(posMoneyFlow);

                decimal negMoneyFlow = typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
                negMoneyFlowList.Add(negMoneyFlow);

                decimal posMoneyFlowTotal = posMoneyFlowList.TakeLastExt(length).Sum();
                decimal negMoneyFlowTotal = negMoneyFlowList.TakeLastExt(length).Sum();
                decimal mfiRatio = negMoneyFlowTotal != 0 ? MinOrMax(posMoneyFlowTotal / negMoneyFlowTotal, 1, 0) : 0;

                decimal mfi = negMoneyFlowTotal == 0 ? 100 : posMoneyFlowTotal == 0 ? 0 : MinOrMax(100 - (100 / (1 + mfiRatio)), 100, 0);
                mfiList.Add(mfi);

                var signal = GetRsiSignal(mfi - prevMfi1, prevMfi1 - prevMfi2, mfi, prevMfi1, 80, 20);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Mfi", mfiList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = mfiList;
            stockData.IndicatorName = IndicatorName.MoneyFlowIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the klinger volume oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateKlingerVolumeOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 34, int slowLength = 55, 
            int signalLength = 13)
        {
            List<decimal> kvoList = new();
            List<decimal> trendList = new();
            List<decimal> dmList = new();
            List<decimal> cmList = new();
            List<decimal> vfList = new();
            List<decimal> kvoHistoList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal mom = currentValue - prevValue;

                decimal prevTrend = trendList.LastOrDefault();
                decimal trend = mom > 0 ? 1 : mom < 0 ? -1 : prevTrend;
                trendList.Add(trend);

                decimal prevDm = dmList.LastOrDefault();
                decimal dm = currentHigh - currentLow;
                dmList.Add(dm);

                decimal prevCm = cmList.LastOrDefault();
                decimal cm = trend == prevTrend ? prevCm + dm : prevDm + dm;
                cmList.Add(cm);

                decimal temp = cm != 0 ? Math.Abs((2 * (dm / cm)) - 1) : -1;
                decimal vf = currentVolume * temp * trend * 100;
                vfList.Add(vf);
            }

            var ema34List = GetMovingAverageList(stockData, maType, fastLength, vfList);
            var ema55List = GetMovingAverageList(stockData, maType, slowLength, vfList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ema34 = ema34List.ElementAtOrDefault(i);
                decimal ema55 = ema55List.ElementAtOrDefault(i);

                decimal klingerOscillator = ema34 - ema55;
                kvoList.Add(klingerOscillator);
            }

            var kvoSignalList = GetMovingAverageList(stockData, maType, signalLength, kvoList);
            for (int k = 0; k < stockData.Count; k++)
            {
                decimal klingerOscillator = kvoList.ElementAtOrDefault(k);
                decimal koSignalLine = kvoSignalList.ElementAtOrDefault(k);

                decimal prevKlingerOscillatorHistogram = kvoHistoList.LastOrDefault();
                decimal klingerOscillatorHistogram = klingerOscillator - koSignalLine;
                kvoHistoList.Add(klingerOscillatorHistogram);

                var signal = GetCompareSignal(klingerOscillatorHistogram, prevKlingerOscillatorHistogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Kvo", kvoList },
                { "KvoSignal", kvoSignalList },
                { "KvoHistogram", kvoHistoList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = kvoList;
            stockData.IndicatorName = IndicatorName.KlingerVolumeOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the on balance volume.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateOnBalanceVolume(this StockData stockData, MovingAvgType maType, int length = 20)
        {
            List<decimal> obvList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevObv = obvList.LastOrDefault();
                decimal obv = currentValue > prevValue ? prevObv + currentVolume : currentValue < prevValue ? prevObv - currentVolume : prevObv;
                obvList.Add(obv);
            }

            var obvSignalList = GetMovingAverageList(stockData, maType, length, obvList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal obv = obvList.ElementAtOrDefault(i);
                decimal prevObv = i >= 1 ? obvList.ElementAtOrDefault(i - 1) : 0;
                decimal obvSig = obvSignalList.ElementAtOrDefault(i);
                decimal prevObvSig = i >= 1 ? obvSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(obv - obvSig, prevObv - prevObvSig);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Obv", obvList },
                { "ObvSignal", obvSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = obvList;
            stockData.IndicatorName = IndicatorName.OnBalanceVolume;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the negative volume.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateNegativeVolumeIndex(this StockData stockData, MovingAvgType maType, int length = 255)
        {
            List<decimal> nviList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevVolume = i >= 1 ? volumeList.ElementAtOrDefault(i - 1) : 0;
                decimal pctChg = CalculatePercentChange(currentClose, prevClose);

                decimal prevNvi = nviList.LastOrDefault();
                decimal nvi = currentVolume >= prevVolume ? prevNvi : prevNvi + (prevNvi * pctChg);
                nviList.Add(nvi);
            }

            var nviSignalList = GetMovingAverageList(stockData, maType, length, nviList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal nvi = nviList.ElementAtOrDefault(i);
                decimal prevNvi = i >= 1 ? nviList.ElementAtOrDefault(i - 1) : 0;
                decimal nviSignal = nviSignalList.ElementAtOrDefault(i);
                decimal prevNviSignal = i >= 1 ? nviSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(nvi - nviSignal, prevNvi - prevNviSignal);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Nvi", nviList },
                { "NviSignal", nviSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = nviList;
            stockData.IndicatorName = IndicatorName.NegativeVolumeIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the positive volume.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculatePositiveVolumeIndex(this StockData stockData, MovingAvgType maType, int length = 255)
        {
            List<decimal> pviList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevVolume = i >= 1 ? volumeList.ElementAtOrDefault(i - 1) : 0;
                decimal pctChg = CalculatePercentChange(currentClose, prevClose);

                decimal prevPvi = pviList.LastOrDefault();
                decimal pvi = currentVolume <= prevVolume ? prevPvi : prevPvi + (prevPvi * pctChg);
                pviList.Add(pvi);
            }

            var pviSignalList = GetMovingAverageList(stockData, maType, length, pviList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal pvi = pviList.ElementAtOrDefault(i);
                decimal prevPvi = i >= 1 ? pviList.ElementAtOrDefault(i - 1) : 0;
                decimal pviSignal = pviSignalList.ElementAtOrDefault(i);
                decimal prevPviSignal = i >= 1 ? pviSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(pvi - pviSignal, prevPvi - prevPviSignal);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Pvi", pviList },
                { "PviSignal", pviSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = pviList;
            stockData.IndicatorName = IndicatorName.PositiveVolumeIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the balance of power.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateBalanceOfPower(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> balanceOfPowerList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal currentOpen = openList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);

                decimal balanceOfPower = currentHigh - currentLow != 0 ? (currentClose - currentOpen) / (currentHigh - currentLow) : 0;
                balanceOfPowerList.Add(balanceOfPower);
            }

            var bopSignalList = GetMovingAverageList(stockData, maType, length, balanceOfPowerList);
            for (int i = 0; i < stockData.ClosePrices.Count; i++)
            {
                decimal bop = balanceOfPowerList.ElementAtOrDefault(i);
                decimal bopMa = bopSignalList.ElementAtOrDefault(i);
                decimal prevBop = i >= 1 ? balanceOfPowerList.ElementAtOrDefault(i - 1) : 0;
                decimal prevBopMa = i >= 1 ? bopSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(bop - bopMa, prevBop - prevBopMa);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Bop", balanceOfPowerList },
                { "BopSignal", bopSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = balanceOfPowerList;
            stockData.IndicatorName = IndicatorName.BalanceOfPower;

            return stockData;
        }

        /// <summary>
        /// Calculates the rate of change.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateRateOfChange(this StockData stockData, int length = 12)
        {
            List<decimal> rocList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
                decimal prevRoc1 = i >= 1 ? rocList.ElementAtOrDefault(i - 1) : 0;
                decimal prevRoc2 = i >= 2 ? rocList.ElementAtOrDefault(i - 2) : 0;

                decimal roc = prevValue != 0 ? (currentValue - prevValue) / prevValue * 100 : 0;
                rocList.Add(roc);

                var signal = GetCompareSignal(roc - prevRoc1, prevRoc1 - prevRoc2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Roc", rocList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = rocList;
            stockData.IndicatorName = IndicatorName.RateOfChange;

            return stockData;
        }

        /// <summary>
        /// Calculates the percentage price oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculatePercentagePriceOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 12, 
            int slowLength = 26, int signalLength = 9)
        {
            List<decimal> ppoList = new();
            List<decimal> ppoHistogramList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
            var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal fastEma = fastEmaList.ElementAtOrDefault(i);
                decimal slowEma = slowEmaList.ElementAtOrDefault(i);

                decimal ppo = slowEma != 0 ? 100 * (fastEma - slowEma) / slowEma : 0;
                ppoList.Add(ppo);
            }

            var ppoSignalList = GetMovingAverageList(stockData, maType, signalLength, ppoList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ppo = ppoList.ElementAtOrDefault(i);
                decimal ppoSignalLine = ppoSignalList.ElementAtOrDefault(i);

                decimal prevPpoHistogram = ppoHistogramList.LastOrDefault();
                decimal ppoHistogram = ppo - ppoSignalLine;
                ppoHistogramList.Add(ppoHistogram);

                var signal = GetCompareSignal(ppoHistogram, prevPpoHistogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ppo", ppoList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ppoList;
            stockData.IndicatorName = IndicatorName.PercentagePriceOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the percentage volume oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculatePercentageVolumeOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 12,
            int slowLength = 26, int signalLength = 9)
        {
            List<decimal> pvoList = new();
            List<decimal> pvoHistogramList = new();
            List<Signal> signalsList = new();
            var (_, _, _, _, volumeList) = GetInputValuesList(stockData);

            var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, volumeList);
            var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, volumeList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal fastEma = fastEmaList.ElementAtOrDefault(i);
                decimal slowEma = slowEmaList.ElementAtOrDefault(i);

                decimal pvo = slowEma != 0 ? 100 * (fastEma - slowEma) / slowEma : 0;
                pvoList.Add(pvo);
            }

            var pvoSignalList = GetMovingAverageList(stockData, maType, signalLength, pvoList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal pvo = pvoList.ElementAtOrDefault(i);
                decimal pvoSignalLine = pvoSignalList.ElementAtOrDefault(i);

                decimal prevPvoHistogram = pvoHistogramList.LastOrDefault();
                decimal pvoHistogram = pvo - pvoSignalLine;
                pvoHistogramList.Add(pvoHistogram);

                var signal = GetCompareSignal(pvoHistogram, prevPvoHistogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Pvo", pvoList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = pvoList;
            stockData.IndicatorName = IndicatorName.PercentageVolumeOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the chaikin money flow.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateChaikinMoneyFlow(this StockData stockData, int length = 20)
        {
            List<decimal> chaikinMoneyFlowList = new();
            List<decimal> tempVolumeList = new();
            List<decimal> moneyFlowVolumeList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                    (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
                decimal prevCmf1 = i >= 1 ? chaikinMoneyFlowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevCmf2 = i >= 2 ? chaikinMoneyFlowList.ElementAtOrDefault(i - 2) : 0;

                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                tempVolumeList.AddRounded(currentVolume);

                decimal moneyFlowVolume = moneyFlowMultiplier * currentVolume;
                moneyFlowVolumeList.Add(moneyFlowVolume);

                decimal volumeSum = tempVolumeList.TakeLastExt(length).Sum();
                decimal mfVolumeSum = moneyFlowVolumeList.TakeLastExt(length).Sum();

                decimal cmf = volumeSum != 0 ? mfVolumeSum / volumeSum : 0;
                chaikinMoneyFlowList.AddRounded(cmf);

                var signal = GetCompareSignal(cmf - prevCmf1, prevCmf1 - prevCmf2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Cmf", chaikinMoneyFlowList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = chaikinMoneyFlowList;
            stockData.IndicatorName = IndicatorName.ChaikinMoneyFlow;

            return stockData;
        }

        /// <summary>
        /// Calculates the accumulation distribution line.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAccumulationDistributionLine(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> adlList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal moneyFlowMultiplier = currentHigh - currentLow != 0 ?
                    (currentClose - currentLow - (currentHigh - currentClose)) / (currentHigh - currentLow) : 0;
                decimal moneyFlowVolume = moneyFlowMultiplier * currentVolume;

                decimal prevAdl = adlList.LastOrDefault();
                decimal adl = prevAdl + moneyFlowVolume;
                adlList.Add(adl);
            }

            var adlSignalList = GetMovingAverageList(stockData, maType, length, adlList);
            for (int i = 0; i < stockData.Count; i++)
            {
                var adl = adlList.ElementAtOrDefault(i);
                var prevAdl = i >= 1 ? adlList.ElementAtOrDefault(i - 1) : 0;
                var adlSignal = adlSignalList.ElementAtOrDefault(i);
                var prevAdlSignal = i >= 1 ? adlSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(adl - adlSignal, prevAdl - prevAdlSignal);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Adl", adlList },
                { "AdlSignal", adlSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = adlList;
            stockData.IndicatorName = IndicatorName.AccumulationDistributionLine;

            return stockData;
        }

        /// <summary>
        /// Calculates the chaikin oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <returns></returns>
        public static StockData CalculateChaikinOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 3, int slowLength = 10)
        {
            List<decimal> chaikinOscillatorList = new();
            List<Signal> signalsList = new();

            var adlList = CalculateAccumulationDistributionLine(stockData, maType, fastLength).CustomValuesList;
            var adl3EmaList = GetMovingAverageList(stockData, maType, fastLength, adlList);
            var adl10EmaList = GetMovingAverageList(stockData, maType, slowLength, adlList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal adl3Ema = adl3EmaList.ElementAtOrDefault(i);
                decimal adl10Ema = adl10EmaList.ElementAtOrDefault(i);

                decimal prevChaikinOscillator = chaikinOscillatorList.LastOrDefault();
                decimal chaikinOscillator = adl3Ema - adl10Ema;
                chaikinOscillatorList.Add(chaikinOscillator);

                var signal = GetCompareSignal(chaikinOscillator, prevChaikinOscillator);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "ChaikinOsc", chaikinOscillatorList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = chaikinOscillatorList;
            stockData.IndicatorName = IndicatorName.ChaikinOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the ichimoku cloud.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="tenkanLength">Length of the tenkan.</param>
        /// <param name="kiiunLength">Length of the kiiun.</param>
        /// <param name="senkouLength">Length of the senkou.</param>
        /// <returns></returns>
        public static StockData CalculateIchimokuCloud(this StockData stockData, int tenkanLength = 9, int kiiunLength = 26, int senkouLength = 52)
        {
            List<decimal> tenkanSenList = new();
            List<decimal> kiiunSenList = new();
            List<decimal> senkouSpanAList = new();
            List<decimal> senkouSpanBList = new();
            List<Signal> signalsList = new();
            var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

            var (tenkanHighList, tenkanLowList) = GetMaxAndMinValuesList(highList, lowList, tenkanLength);
            var (kiiunHighList, kiiunLowList) = GetMaxAndMinValuesList(highList, lowList, kiiunLength);
            var (senkouHighList, senkouLowList) = GetMaxAndMinValuesList(highList, lowList, senkouLength);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal highest1 = tenkanHighList.ElementAtOrDefault(i);
                decimal lowest1 = tenkanLowList.ElementAtOrDefault(i);
                decimal highest2 = kiiunHighList.ElementAtOrDefault(i);
                decimal lowest2 = kiiunLowList.ElementAtOrDefault(i);
                decimal highest3 = senkouHighList.ElementAtOrDefault(i);
                decimal lowest3 = senkouLowList.ElementAtOrDefault(i);

                decimal prevTenkanSen = tenkanSenList.LastOrDefault();
                decimal tenkanSen = (highest1 + lowest1) / 2;
                tenkanSenList.Add(tenkanSen);

                decimal prevKiiunSen = kiiunSenList.LastOrDefault();
                decimal kiiunSen = (highest2 + lowest2) / 2;
                kiiunSenList.Add(kiiunSen);

                decimal senkouSpanA = (tenkanSen + kiiunSen) / 2;
                senkouSpanAList.Add(senkouSpanA);

                decimal senkouSpanB = (highest3 + lowest3) / 2;
                senkouSpanBList.Add(senkouSpanB);

                var signal = GetCompareSignal(tenkanSen - kiiunSen, prevTenkanSen - prevKiiunSen);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "TenkanSen", tenkanSenList },
                { "KiiunSen", kiiunSenList },
                { "SenkouSpanA", senkouSpanAList },
                { "SenkouSpanB", senkouSpanBList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.IchimokuCloud;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the alligator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="iawLength">Length of the iaw.</param>
        /// <param name="iawOffset">The iaw offset.</param>
        /// <param name="teethLength">Length of the teeth.</param>
        /// <param name="teethOffset">The teeth offset.</param>
        /// <param name="lipsLength">Length of the lips.</param>
        /// <param name="lipsOffset">The lips offset.</param>
        /// <returns></returns>
        public static StockData CalculateAlligatorIndex(this StockData stockData, InputName inputName, MovingAvgType maType, int iawLength = 13, 
            int iawOffset = 8, int teethLength = 8, int teethOffset = 5, int lipsLength = 5, int lipsOffset = 3)
        {
            List<decimal> displacedJawList = new();
            List<decimal> displacedTeethList = new();
            List<decimal> displacedLipsList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var iawList = GetMovingAverageList(stockData, maType, iawLength, inputList);
            var teethList = GetMovingAverageList(stockData, maType, teethLength, inputList);
            var lipsList = GetMovingAverageList(stockData, maType, lipsLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevJaw = displacedJawList.LastOrDefault();
                decimal displacedJaw = i >= iawOffset ? iawList.ElementAtOrDefault(i - iawOffset) : 0;
                displacedJawList.Add(displacedJaw);

                decimal prevTeeth = displacedTeethList.LastOrDefault();
                decimal displacedTeeth = i >= teethOffset ? teethList.ElementAtOrDefault(i - teethOffset) : 0;
                displacedTeethList.Add(displacedTeeth);

                decimal prevLips = displacedLipsList.LastOrDefault();
                decimal displacedLips = i >= lipsOffset ? lipsList.ElementAtOrDefault(i - lipsOffset) : 0;
                displacedLipsList.Add(displacedLips);

                var signal = GetBullishBearishSignal(displacedLips - Math.Max(displacedJaw, displacedTeeth), prevLips - Math.Max(prevJaw, prevTeeth),
                    displacedLips - Math.Min(displacedJaw, displacedTeeth), prevLips - Math.Min(prevJaw, prevTeeth));
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Lips", displacedLipsList },
                { "Teeth", displacedTeethList },
                { "Jaws", displacedJawList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.AlligatorIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the gator oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="iawLength">Length of the iaw.</param>
        /// <param name="iawOffset">The iaw offset.</param>
        /// <param name="teethLength">Length of the teeth.</param>
        /// <param name="teethOffset">The teeth offset.</param>
        /// <param name="lipsLength">Length of the lips.</param>
        /// <param name="lipsOffset">The lips offset.</param>
        /// <returns></returns>
        public static StockData CalculateGatorOscillator(this StockData stockData, InputName inputName, MovingAvgType maType, int iawLength = 13,
            int iawOffset = 8, int teethLength = 8, int teethOffset = 5, int lipsLength = 5, int lipsOffset = 3)
        {
            List<decimal> topList = new();
            List<decimal> bottomList = new();
            List<Signal> signalsList = new();

            var alligatorList = CalculateAlligatorIndex(stockData, inputName, maType, iawLength, iawOffset, teethLength, teethOffset, lipsLength, 
                lipsOffset).OutputValues;
            var iawList = alligatorList["Jaw"];
            var teethList = alligatorList["Teeth"];
            var lipsList = alligatorList["Lips"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal iaw = iawList.ElementAtOrDefault(i);
                decimal teeth = teethList.ElementAtOrDefault(i);
                decimal lips = lipsList.ElementAtOrDefault(i);

                decimal prevTop = topList.LastOrDefault();
                decimal top = Math.Abs(iaw - teeth);
                topList.AddRounded(top);

                decimal prevBottom = bottomList.LastOrDefault();
                decimal bottom = -Math.Abs(teeth - lips);
                bottomList.AddRounded(bottom);

                var signal = GetCompareSignal(top - bottom, prevTop - prevBottom);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Top", topList },
                { "Bottom", bottomList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.GatorOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the ultimate oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length1">The length1.</param>
        /// <param name="length2">The length2.</param>
        /// <param name="length3">The length3.</param>
        /// <returns></returns>
        public static StockData CalculateUltimateOscillator(this StockData stockData, int length1 = 7, int length2 = 14, int length3 = 28)
        {
            List<decimal> uoList = new();
            List<decimal> bpList = new();
            List<decimal> trList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal minValue = Math.Min(currentLow, prevClose);
                decimal maxValue = Math.Max(currentHigh, prevClose);
                decimal prevUo1 = i >= 1 ? uoList.ElementAtOrDefault(i - 1) : 0;
                decimal prevUo2 = i >= 2 ? uoList.ElementAtOrDefault(i - 2) : 0;

                decimal buyingPressure = currentClose - minValue;
                bpList.Add(buyingPressure);

                decimal trueRange = maxValue - minValue;
                trList.Add(trueRange);

                decimal bp7Sum = bpList.TakeLastExt(length1).Sum();
                decimal bp14Sum = bpList.TakeLastExt(length2).Sum();
                decimal bp28Sum = bpList.TakeLastExt(length3).Sum();
                decimal tr7Sum = trList.TakeLastExt(length1).Sum();
                decimal tr14Sum = trList.TakeLastExt(length2).Sum();
                decimal tr28Sum = trList.TakeLastExt(length3).Sum();
                decimal avg7 = tr7Sum != 0 ? bp7Sum / tr7Sum : 0;
                decimal avg14 = tr14Sum != 0 ? bp14Sum / tr14Sum : 0;
                decimal avg28 = tr28Sum != 0 ? bp28Sum / tr28Sum : 0;

                decimal ultimateOscillator = MinOrMax(100 * (((4 * avg7) + (2 * avg14) + avg28) / (4 + 2 + 1)), 100, 0);
                uoList.Add(ultimateOscillator);

                var signal = GetRsiSignal(ultimateOscillator - prevUo1, prevUo1 - prevUo2, ultimateOscillator, prevUo1, 70, 30);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Uo", uoList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = uoList;
            stockData.IndicatorName = IndicatorName.UltimateOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the vortex indicator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateVortexIndicator(this StockData stockData, int length = 14)
        {
            List<decimal> vmPlusList = new();
            List<decimal> trueRangeList = new();
            List<decimal> vmMinusList = new();
            List<decimal> viPlus14List = new();
            List<decimal> viMinus14List = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;

                decimal vmPlus = Math.Abs(currentHigh - prevLow);
                vmPlusList.Add(vmPlus);

                decimal vmMinus = Math.Abs(currentLow - prevHigh);
                vmMinusList.Add(vmMinus);

                decimal trueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);
                trueRangeList.Add(trueRange);

                decimal vmPlus14 = vmPlusList.TakeLastExt(length).Sum();
                decimal vmMinus14 = vmMinusList.TakeLastExt(length).Sum();
                decimal trueRange14 = trueRangeList.TakeLastExt(length).Sum();

                decimal prevViPlus14 = viPlus14List.LastOrDefault();
                decimal viPlus14 = trueRange14 != 0 ? vmPlus14 / trueRange14 : 0;
                viPlus14List.Add(viPlus14);

                decimal prevViMinus14 = viMinus14List.LastOrDefault();
                decimal viMinus14 = trueRange14 != 0 ? vmMinus14 / trueRange14 : 0;
                viMinus14List.Add(viMinus14);

                var signal = GetCompareSignal(viPlus14 - viMinus14, prevViPlus14 - prevViMinus14);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "ViPlus", viPlus14List },
                { "ViMinus", viMinus14List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.VortexIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the super trend.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="atrMult">The atr mult.</param>
        /// <returns></returns>
        public static StockData CalculateSuperTrend(this StockData stockData, MovingAvgType maType, int length = 22, decimal atrMult = 3)
        {
            List<decimal> longStopList = new();
            List<decimal> shortStopList = new();
            List<decimal> dirList = new();
            List<decimal> trendList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var atrList = CalculateAverageTrueRange(stockData, maType, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentAtr = atrList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal atrValue = atrMult * currentAtr;
                decimal tempLongStop = currentValue - atrValue;
                decimal tempShortStop = currentValue + atrValue;

                decimal prevLongStop = longStopList.LastOrDefault();
                decimal longStop = prevValue > prevLongStop ? Math.Max(tempLongStop, prevLongStop) : tempLongStop;
                longStopList.Add(longStop);

                decimal prevShortStop = shortStopList.LastOrDefault();
                decimal shortStop = prevValue < prevShortStop ? Math.Max(tempShortStop, prevShortStop) : tempShortStop;
                shortStopList.Add(shortStop);

                decimal prevDir = dirList.LastOrDefault();
                decimal dir = prevDir == -1 && currentValue > prevShortStop ? 1 : prevDir == 1 && currentValue < prevLongStop ? -1 : prevDir;
                dirList.Add(dir);

                decimal prevTrend = trendList.LastOrDefault();
                decimal trend = dir > 0 ? longStop : shortStop;
                trendList.Add(trend);

                var signal = GetCompareSignal(currentValue - trend, prevValue - prevTrend);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Trend", trendList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = trendList;
            stockData.IndicatorName = IndicatorName.SuperTrend;

            return stockData;
        }

        /// <summary>
        /// Calculates the trix.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateTrix(this StockData stockData, MovingAvgType maType, int length = 15, int signalLength = 9)
        {
            List<decimal> trixList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
            var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ema3 = ema3List.ElementAtOrDefault(i);
                decimal prevEma3 = i >= 1 ? ema3List.ElementAtOrDefault(i - 1) : 0;

                decimal trix = CalculatePercentChange(ema3, prevEma3);
                trixList.Add(trix);
            }

            var trixSignalList = GetMovingAverageList(stockData, maType, signalLength, trixList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal trix = trixList.ElementAtOrDefault(i);
                decimal trixSignal = trixSignalList.ElementAtOrDefault(i);
                decimal prevTrix = i >= 1 ? trixList.ElementAtOrDefault(i - 1) : 0;
                decimal prevTrixSignal = i >= 1 ? trixSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(trix - trixSignal, prevTrix - prevTrixSignal);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Trix", trixList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = trixList;
            stockData.IndicatorName = IndicatorName.Trix;

            return stockData;
        }

        /// <summary>
        /// Calculates the williams r.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateWilliamsR(this StockData stockData, int length = 14)
        {
            List<decimal> williamsRList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal highestHigh = highestList.ElementAtOrDefault(i);
                decimal lowestLow = lowestList.ElementAtOrDefault(i);
                decimal prevWilliamsR1 = i >= 1 ? williamsRList.ElementAtOrDefault(i - 1) : 0;
                decimal prevWilliamsR2 = i >= 2 ? williamsRList.ElementAtOrDefault(i - 2) : 0;

                decimal williamsR = highestHigh - lowestLow != 0 ? -100 * (highestHigh - currentClose) / (highestHigh - lowestLow) : -100;
                williamsRList.Add(williamsR);

                var signal = GetRsiSignal(williamsR - prevWilliamsR1, prevWilliamsR1 - prevWilliamsR2, williamsR, prevWilliamsR1, -20, -80);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Williams%R", williamsRList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = williamsRList;
            stockData.IndicatorName = IndicatorName.WilliamsR;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the true strength.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length1">The length1.</param>
        /// <param name="length2">The length2.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateTrueStrengthIndex(this StockData stockData, MovingAvgType maType, int length1 = 25, int length2 = 13, 
            int signalLength = 7)
        {
            List<decimal> pcList = new();
            List<decimal> absPCList = new();
            List<decimal> tsiList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal pc = currentValue - prevValue;
                pcList.Add(pc);

                decimal absPC = Math.Abs(pc);
                absPCList.Add(absPC);
            }

            var pcSmooth1List = GetMovingAverageList(stockData, maType, length1, pcList);
            var pcSmooth2List = GetMovingAverageList(stockData, maType, length2, pcSmooth1List);
            var absPCSmooth1List = GetMovingAverageList(stockData, maType, length1, absPCList);
            var absPCSmooth2List = GetMovingAverageList(stockData, maType, length2, absPCSmooth1List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal absSmooth2PC = absPCSmooth2List.ElementAtOrDefault(i);
                decimal smooth2PC = pcSmooth2List.ElementAtOrDefault(i);

                decimal tsi = absSmooth2PC != 0 ? MinOrMax(100 * smooth2PC / absSmooth2PC, 100, -100) : 0;
                tsiList.Add(tsi);
            }

            var tsiSignalList = GetMovingAverageList(stockData, maType, signalLength, tsiList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal tsi = tsiList.ElementAtOrDefault(i);
                decimal tsiSignal = tsiSignalList.ElementAtOrDefault(i);
                decimal prevTsi = i >= 1 ? tsiList.ElementAtOrDefault(i - 1) : 0;
                decimal prevTsiSignal = i >= 1 ? tsiSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetRsiSignal(tsi - tsiSignal, prevTsi - prevTsiSignal, tsi, prevTsi, 25, -25);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Tsi", tsiList },
                { "Signal", tsiSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tsiList;
            stockData.IndicatorName = IndicatorName.TrueStrengthIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the stochastic oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateStochasticOscillator(this StockData stockData, MovingAvgType maType, int length = 14, int signalLength = 3)
        {
            List<decimal> fastKList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal highestHigh = highestList.ElementAtOrDefault(i);
                decimal lowestLow = lowestList.ElementAtOrDefault(i);

                decimal fastK = highestHigh - lowestLow != 0 ? MinOrMax((currentValue - lowestLow) / (highestHigh - lowestLow) * 100, 100, 0) : 0;
                fastKList.Add(fastK);
            }

            var fastDList = GetMovingAverageList(stockData, maType, signalLength, fastKList);
            var slowDList = GetMovingAverageList(stockData, maType, signalLength, fastDList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal slowK = fastDList.ElementAtOrDefault(i);
                decimal slowD = slowDList.ElementAtOrDefault(i);
                decimal prevSlowk = i >= 1 ? fastDList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSlowd = i >= 1 ? slowDList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetRsiSignal(slowK - slowD, prevSlowk - prevSlowd, slowK, prevSlowk, 80, 20);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "FastK", fastKList },
                { "FastD", fastDList },
                { "SlowD", slowDList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = fastKList;
            stockData.IndicatorName = IndicatorName.StochasticOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the price momentum oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length1">The length1.</param>
        /// <param name="length2">The length2.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculatePriceMomentumOscillator(this StockData stockData, MovingAvgType maType, int length1 = 35, 
            int length2 = 20, int signalLength = 10)
        {
            List<decimal> pmoList = new();
            List<decimal> rocMaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            
            decimal sc1 = 2 / (decimal)length1;
            decimal sc2 = 2 / (decimal)length2;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal roc = prevValue != 0 ? (currentValue - prevValue) / prevValue * 100 : 0;

                decimal prevRocMa1 = rocMaList.LastOrDefault();
                decimal rocMa = prevRocMa1 + ((roc - prevRocMa1) * sc1);
                rocMaList.AddRounded(rocMa);

                decimal prevPmo = pmoList.LastOrDefault();
                decimal pmo = prevPmo + (((rocMa * 10) - prevPmo) * sc2);
                pmoList.Add(pmo);
            }

            var pmoSignalList = GetMovingAverageList(stockData, maType, signalLength, pmoList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal pmo = pmoList.ElementAtOrDefault(i);
                decimal prevPmo = i >= 1 ? pmoList.ElementAtOrDefault(i - 1) : 0;
                decimal pmoSignal = pmoSignalList.ElementAtOrDefault(i);
                decimal prevPmoSignal = i >= 1 ? pmoSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(pmo - pmoSignal, prevPmo - prevPmoSignal);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Pmo", pmoList },
                { "Signal", pmoSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = pmoList;
            stockData.IndicatorName = IndicatorName.PriceMomentumOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the schaff trend cycle.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="cycleLength">Length of the cycle.</param>
        /// <returns></returns>
        public static StockData CalculateSchaffTrendCycle(this StockData stockData, MovingAvgType maType, int fastLength = 23, int slowLength = 50, 
            int cycleLength = 10)
        {
            List<decimal> macdList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema23List = GetMovingAverageList(stockData, maType, fastLength, inputList);
            var ema50List = GetMovingAverageList(stockData, maType, slowLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentEma23 = ema23List.ElementAtOrDefault(i);
                decimal currentEma50 = ema50List.ElementAtOrDefault(i);

                decimal macd = currentEma23 - currentEma50;
                macdList.Add(macd);
            }

            stockData.CustomValuesList = macdList;
            var stcList = CalculateStochasticOscillator(stockData, maType, cycleLength, cycleLength).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal stc = stcList.ElementAtOrDefault(i);
                decimal prevStc1 = i >= 1 ? stcList.ElementAtOrDefault(i - 1) : 0;
                decimal prevStc2 = i >= 2 ? stcList.ElementAtOrDefault(i - 2) : 0;

                var signal = GetRsiSignal(stc - prevStc1, prevStc1 - prevStc2, stc, prevStc1, 75, 25);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Stc", stcList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = stcList;
            stockData.IndicatorName = IndicatorName.SchaffTrendCycle;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the elder ray.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateElderRayIndex(this StockData stockData, MovingAvgType maType, int length = 13)
        {
            List<decimal> bullPowerList = new();
            List<decimal> bearPowerList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            var emaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);

                decimal prevBullPower = bullPowerList.LastOrDefault();
                decimal bullPower = currentHigh - currentEma;
                bullPowerList.Add(bullPower);

                decimal prevBearPower = bearPowerList.LastOrDefault();
                decimal bearPower = currentLow - currentEma;
                bearPowerList.Add(bearPower);

                var signal = GetCompareSignal(bullPower - bearPower, prevBullPower - prevBearPower);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "BullPower", bullPowerList },
                { "BearPower", bearPowerList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.ElderRayIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the absolute price oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <returns></returns>
        public static StockData CalculateAbsolutePriceOscillator(this StockData stockData, MovingAvgType maType, int fastLength = 10, int slowLength = 20)
        {
            List<decimal> apoList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var fastEmaList = GetMovingAverageList(stockData, maType, fastLength, inputList);
            var slowEmaList = GetMovingAverageList(stockData, maType, slowLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal fastEma = fastEmaList.ElementAtOrDefault(i);
                decimal slowEma = slowEmaList.ElementAtOrDefault(i);

                decimal prevApo = apoList.LastOrDefault();
                decimal apo = fastEma - slowEma;
                apoList.Add(apo);

                var signal = GetCompareSignal(apo, prevApo);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Apo", apoList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = apoList;
            stockData.IndicatorName = IndicatorName.AbsolutePriceOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the aroon oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAroonOscillator(this StockData stockData, int length = 25)
        {
            List<decimal> aroonOscillatorList = new();
            List<decimal> tempList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentPrice = inputList.ElementAtOrDefault(i);
                tempList.Add(currentPrice);

                decimal maxPrice = highestList.ElementAtOrDefault(i);
                int maxIndex = tempList.LastIndexOf(maxPrice);
                decimal minPrice = lowestList.ElementAtOrDefault(i);
                int minIndex = tempList.LastIndexOf(minPrice);
                int daysSinceMax = i - maxIndex;
                int daysSinceMin = i - minIndex;
                decimal aroonUp = (decimal)(length - daysSinceMax) / length * 100;
                decimal aroonDown = (decimal)(length - daysSinceMin) / length * 100;

                decimal prevAroonOscillator = aroonOscillatorList.LastOrDefault();
                decimal aroonOscillator = aroonUp - aroonDown;
                aroonOscillatorList.Add(aroonOscillator);

                var signal = GetCompareSignal(aroonOscillator, prevAroonOscillator);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Aroon", aroonOscillatorList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = aroonOscillatorList;
            stockData.IndicatorName = IndicatorName.AroonOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the absolute strength.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="maLength">Length of the ma.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateAbsoluteStrengthIndex(this StockData stockData, int length = 10, int maLength = 21, int signalLength = 34)
        {
            List<decimal> AList = new();
            List<decimal> MList = new();
            List<decimal> DList = new();
            List<decimal> mtList = new();
            List<decimal> utList = new();
            List<decimal> abssiEmaList = new();
            List<decimal> dList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal alp = (decimal)2 / (signalLength + 1);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevA = AList.LastOrDefault();
                decimal A = currentValue > prevValue && prevValue != 0 ? prevA + ((currentValue / prevValue) - 1) : prevA;
                AList.Add(A);

                decimal prevM = MList.LastOrDefault();
                decimal M = currentValue == prevValue ? prevM + ((decimal)1 / length) : prevM;
                MList.Add(M);

                decimal prevD = DList.LastOrDefault();
                decimal D = currentValue < prevValue && currentValue != 0 ? prevD + ((prevValue / currentValue) - 1) : prevD;
                DList.Add(D);

                decimal abssi = (D + M) / 2 != 0 ? 1 - (1 / (1 + ((A + M) / 2 / ((D + M) / 2)))) : 1;
                decimal abssiEma = CalculateEMA(abssi, abssiEmaList.LastOrDefault(), maLength);
                abssiEmaList.Add(abssiEma);

                decimal abssio = abssi - abssiEma;
                decimal prevMt = mtList.LastOrDefault();
                decimal mt = (alp * abssio) + ((1 - alp) * prevMt);
                mtList.Add(mt);

                decimal prevUt = utList.LastOrDefault();
                decimal ut = (alp * mt) + ((1 - alp) * prevUt);
                utList.Add(ut);

                decimal s = (2 - alp) * (mt - ut) / (1 - alp);
                decimal prevd = dList.LastOrDefault();
                decimal d = abssio - s;
                dList.Add(d);

                var signal = GetCompareSignal(d, prevd);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Asi", dList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = dList;
            stockData.IndicatorName = IndicatorName.AbsoluteStrengthIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the anchored momentum.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="smoothLength">Length of the smooth.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <param name="momentumLength">Length of the momentum.</param>
        /// <returns></returns>
        public static StockData CalculateAnchoredMomentum(this StockData stockData, MovingAvgType maType, int smoothLength = 7, 
            int signalLength = 8, int momentumLength = 10)
        {
            List<decimal> tempList = new();
            List<decimal> amomList = new();
            List<decimal> amomsList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int p = MinOrMax((2 * momentumLength) + 1);

            var emaList = GetMovingAverageList(stockData, maType, smoothLength, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentEma = emaList.ElementAtOrDefault(i);

                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.Add(currentValue);

                decimal sma = tempList.TakeLastExt(p).Average();
                decimal prevAmom = amomList.LastOrDefault();
                decimal amom = sma != 0 ? 100 * ((currentEma / sma) - 1) : 0;
                amomList.Add(amom);

                decimal prevAmoms = amomsList.LastOrDefault();
                decimal amoms = amomList.TakeLastExt(signalLength).Average();
                amomsList.Add(amoms);

                var signal = GetCompareSignal(amom - amoms, prevAmom - prevAmoms);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Amom", amomList },
                { "Signal", amomsList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = amomList;
            stockData.IndicatorName = IndicatorName.AnchoredMomentum;

            return stockData;
        }

        /// <summary>
        /// Calculates the index of the accumulative swing.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAccumulativeSwingIndex(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> accumulativeSwingIndexList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentOpen = openList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHighCurrentClose = prevHigh - currentClose;
                decimal prevLowCurrentClose = prevLow - currentClose;
                decimal prevClosePrevOpen = prevClose - prevOpen;
                decimal currentHighPrevClose = currentHigh - prevClose;
                decimal currentLowPrevClose = currentLow - prevClose;
                decimal t = currentHigh - currentLow;
                decimal k = Math.Max(Math.Abs(prevHighCurrentClose), Math.Abs(prevLowCurrentClose));
                decimal r = currentHighPrevClose > Math.Max(currentLowPrevClose, t) ? currentHighPrevClose - (0.5m * currentLowPrevClose) + (0.25m * prevClosePrevOpen) :
                    currentLowPrevClose > Math.Max(currentHighPrevClose, t) ? currentLowPrevClose - (0.5m * currentHighPrevClose) + (0.25m * prevClosePrevOpen) :
                    t > Math.Max(currentHighPrevClose, currentLowPrevClose) ? t + (0.25m * prevClosePrevOpen) : 0;
                decimal swingIndex = r != 0 && t != 0 ? 50 * ((prevClose - currentClose + (0.5m * prevClosePrevOpen) + 
                    (0.25m * (currentClose - currentOpen))) / r) * (k / t) : 0;

                decimal prevSwingIndex = accumulativeSwingIndexList.LastOrDefault();
                decimal accumulativeSwingIndex = prevSwingIndex + swingIndex;
                accumulativeSwingIndexList.Add(accumulativeSwingIndex);
            }

            var asiOscillatorList = GetMovingAverageList(stockData, maType, length, accumulativeSwingIndexList);
            for (int i = 0; i < stockData.Count; i++)
            {
                var asi = accumulativeSwingIndexList.ElementAtOrDefault(i);
                var prevAsi = i >= 1 ? accumulativeSwingIndexList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(asi, prevAsi);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Asi", accumulativeSwingIndexList },
                { "Signal", asiOscillatorList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = accumulativeSwingIndexList;
            stockData.IndicatorName = IndicatorName.AccumulativeSwingIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive stochastic.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveStochastic(this StockData stockData, int length = 50, int fastLength = 50, int slowLength = 200)
        {
            List<decimal> stcList = new();
            List<Signal> signalsList = new();

            var srcList = CalculateLinearRegression(stockData, Math.Abs(slowLength - fastLength)).CustomValuesList;
            var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];
            var (highest1List, lowest1List) = GetMaxAndMinValuesList(srcList, fastLength);
            var (highest2List, lowest2List) = GetMaxAndMinValuesList(srcList, slowLength);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal er = erList.ElementAtOrDefault(i);
                decimal src = srcList.ElementAtOrDefault(i);
                decimal highest1 = highest1List.ElementAtOrDefault(i);
                decimal lowest1 = lowest1List.ElementAtOrDefault(i);
                decimal highest2 = highest2List.ElementAtOrDefault(i);
                decimal lowest2 = lowest2List.ElementAtOrDefault(i);
                decimal prevStc1 = i >= 1 ? stcList.ElementAtOrDefault(i - 1) : 0;
                decimal prevStc2 = i >= 2 ? stcList.ElementAtOrDefault(i - 2) : 0;
                decimal a = (er * highest1) + ((1 - er) * highest2);
                decimal b = (er * lowest1) + ((1 - er) * lowest2);

                decimal stc = a - b != 0 ? MinOrMax((src - b) / (a - b), 1, 0) : 0;
                stcList.Add(stc);

                var signal = GetRsiSignal(stc - prevStc1, prevStc1 - prevStc2, stc, prevStc1, 0.8m, 0.2m);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ast", stcList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = stcList;
            stockData.IndicatorName = IndicatorName.AdaptiveStochastic;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive ergodic candlestick oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="smoothLength">Length of the smooth.</param>
        /// <param name="stochLength">Length of the stoch.</param>
        /// <param name="signalLength">Length of the signal.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveErgodicCandlestickOscillator(this StockData stockData, MovingAvgType maType, int smoothLength = 5, 
            int stochLength = 14, int signalLength = 9)
        {
            List<decimal> came1List = new();
            List<decimal> came2List = new();
            List<decimal> came11List = new();
            List<decimal> came22List = new();
            List<decimal> ecoList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

            decimal mep = (decimal)2 / (smoothLength + 1);
            decimal ce = (stochLength + smoothLength) * 2;

            var stochList = CalculateStochasticOscillator(stockData, maType, stochLength, stochLength).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal stoch = stochList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentOpen = openList.ElementAtOrDefault(i);
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal vrb = Math.Abs(stoch - 50) / 50;

                decimal prevCame1 = came1List.LastOrDefault();
                decimal came1 = i < ce ? currentClose - currentOpen : prevCame1 + (mep * vrb * (currentClose - currentOpen - prevCame1));
                came1List.Add(came1);

                decimal prevCame2 = came2List.LastOrDefault();
                decimal came2 = i < ce ? currentHigh - currentLow : prevCame2 + (mep * vrb * (currentHigh - currentLow - prevCame2));
                came2List.Add(came2);

                decimal prevCame11 = came11List.LastOrDefault();
                decimal came11 = i < ce ? came1 : prevCame11 + (mep * vrb * (came1 - prevCame11));
                came11List.Add(came11);

                decimal prevCame22 = came22List.LastOrDefault();
                decimal came22 = i < ce ? came2 : prevCame22 + (mep * vrb * (came2 - prevCame22));
                came22List.Add(came22);

                decimal eco = came22 != 0 ? came11 / came22 * 100 : 0;
                ecoList.Add(eco);
            }

            var seList = GetMovingAverageList(stockData, maType, signalLength, ecoList);
            for (int i = 0; i < stockData.Count; i++)
            {
                var eco = ecoList.ElementAtOrDefault(i);
                var se = seList.ElementAtOrDefault(i);
                var prevEco = i >= 1 ? ecoList.ElementAtOrDefault(i - 1) : 0;
                var prevSe = i >= 1 ? seList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(eco - se, prevEco - prevSe);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Eco", ecoList },
                { "Signal", seList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ecoList;
            stockData.IndicatorName = IndicatorName.AdaptiveErgodicCandlestickOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the average money flow oscillator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="smoothLength">Length of the smooth.</param>
        /// <returns></returns>
        public static StockData CalculateAverageMoneyFlowOscillator(this StockData stockData, MovingAvgType maType, int length = 5, int smoothLength = 3)
        {
            List<decimal> chgList = new();
            List<decimal> rList = new();
            List<decimal> kList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            var avgvList = GetMovingAverageList(stockData, maType, length, volumeList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal chg = currentValue - prevValue;
                chgList.Add(chg);
            }

            var avgcList = GetMovingAverageList(stockData, maType, length, chgList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal avgv = avgvList.ElementAtOrDefault(i);
                decimal avgc = avgcList.ElementAtOrDefault(i);

                decimal r = Math.Abs(avgv * avgc) > 0 ? Log(Math.Abs(avgv * avgc)) * Math.Sign(avgc) : 0;
                rList.Add(r);

                var list = rList.TakeLastExt(length).ToList();
                decimal rh = list.Max();
                decimal rl = list.Min();
                decimal rs = rh != rl ? (r - rl) / (rh - rl) * 100 : 0;

                decimal k = (rs * 2) - 100;
                kList.Add(k);
            }

            var ksList = GetMovingAverageList(stockData, maType, smoothLength, kList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ks = ksList.ElementAtOrDefault(i);
                decimal prevKs = i >= 1 ? ksList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(ks, prevKs);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Amfo", ksList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ksList;
            stockData.IndicatorName = IndicatorName.AverageMoneyFlowOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the absolute strength MTF indicator.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="smoothLength">Length of the smooth.</param>
        /// <returns></returns>
        public static StockData CalculateAbsoluteStrengthMTFIndicator(this StockData stockData, MovingAvgType maType, int length = 50, int smoothLength = 25)
        {
            List<decimal> prevValuesList = new();
            List<decimal> bulls0List = new();
            List<decimal> bears0List = new();
            List<decimal> bulls1List = new();
            List<decimal> bears1List = new();
            List<decimal> bulls2List = new();
            List<decimal> bears2List = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                prevValuesList.Add(prevValue);
            }

            var price1List = GetMovingAverageList(stockData, maType, length, inputList);
            var price2List = GetMovingAverageList(stockData, maType, length, prevValuesList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal price1 = price1List.ElementAtOrDefault(i);
                decimal price2 = price2List.ElementAtOrDefault(i);
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);
                decimal high = highList.ElementAtOrDefault(i);
                decimal low = lowList.ElementAtOrDefault(i);
                decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;

                decimal bulls0 = 0.5m * (Math.Abs(price1 - price2) + (price1 - price2));
                bulls0List.Add(bulls0);

                decimal bears0 = 0.5m * (Math.Abs(price1 - price2) - (price1 - price2));
                bears0List.Add(bears0);

                decimal bulls1 = price1 - lowest;
                bulls1List.Add(bulls1);

                decimal bears1 = highest - price1;
                bears1List.Add(bears1);

                decimal bulls2 = 0.5m * (Math.Abs(high - prevHigh) + (high - prevHigh));
                bulls2List.Add(bulls2);

                decimal bears2 = 0.5m * (Math.Abs(prevLow - low) + (prevLow - low));
                bears2List.Add(bears2);
            }

            var avgBulls0List = GetMovingAverageList(stockData, maType, length, bulls0List);
            var avgBears0List = GetMovingAverageList(stockData, maType, length, bears0List);
            var avgBulls1List = GetMovingAverageList(stockData, maType, length, bulls1List);
            var avgBears1List = GetMovingAverageList(stockData, maType, length, bears1List);
            var avgBulls2List = GetMovingAverageList(stockData, maType, length, bulls2List);
            var avgBears2List = GetMovingAverageList(stockData, maType, length, bears2List);
            var smthBulls0List = GetMovingAverageList(stockData, maType, smoothLength, bulls0List);
            var smthBears0List = GetMovingAverageList(stockData, maType, smoothLength, bears0List);
            var smthBulls1List = GetMovingAverageList(stockData, maType, smoothLength, bulls1List);
            var smthBears1List = GetMovingAverageList(stockData, maType, smoothLength, bears1List);
            var smthBulls2List = GetMovingAverageList(stockData, maType, smoothLength, bulls2List);
            var smthBears2List = GetMovingAverageList(stockData, maType, smoothLength, bears2List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal bulls = smthBulls0List.ElementAtOrDefault(i);
                decimal bears = smthBears0List.ElementAtOrDefault(i);
                decimal prevBulls = i >= 1 ? smthBulls0List.ElementAtOrDefault(i - 1) : 0;
                decimal prevBears = i >= 1 ? smthBears0List.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(bulls - bears, prevBulls - prevBears);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Bulls", smthBulls0List },
                { "Bears", smthBears0List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.AbsoluteStrengthMTFIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the 4 Percentage Price Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <param name="length3"></param>
        /// <param name="length4"></param>
        /// <param name="length5"></param>
        /// <param name="length6"></param>
        /// <param name="blueMult"></param>
        /// <param name="yellowMult"></param>
        /// <returns></returns>
        public static StockData Calculate4PercentagePriceOscillator(this StockData stockData,
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 5, int length2 = 8, int length3 = 10, int length4 = 17,
            int length5 = 14, int length6 = 16, decimal blueMult = 4.3m, decimal yellowMult = 1.4m)
        {
            List<decimal> ppo1List = new();
            List<decimal> ppo2List = new();
            List<decimal> ppo3List = new();
            List<decimal> ppo4List = new();
            List<decimal> ppo2HistogramList = new();
            List<decimal> ppo4HistogramList = new();
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
                decimal macd2 = ema17 - ema8;
                decimal macd3 = ema10 - ema16;
                decimal macd4 = ema5 - ema10;

                decimal ppo1 = ema14 != 0 ? macd1 / ema14 * 100 : 0;
                ppo1List.Add(ppo1);

                decimal ppo2 = ema8 != 0 ? macd2 / ema8 * 100 : 0;
                ppo2List.Add(ppo2);

                decimal ppo3 = ema16 != 0 ? macd3 / ema16 * 100 : 0;
                ppo3List.Add(ppo3);

                decimal ppo4 = ema10 != 0 ? macd4 / ema10 * 100 : 0;
                ppo4List.Add(ppo4);
            }

            var ppo1SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo1List);
            var ppo2SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo2List);
            var ppo3SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo3List);
            var ppo4SignalLineList = GetMovingAverageList(stockData, maType, length1, ppo4List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ppo1 = ppo1List.ElementAtOrDefault(i);
                decimal ppo1SignalLine = ppo1SignalLineList.ElementAtOrDefault(i);
                decimal ppo2 = ppo2List.ElementAtOrDefault(i);
                decimal ppo2SignalLine = ppo2SignalLineList.ElementAtOrDefault(i);
                decimal ppo3 = ppo3List.ElementAtOrDefault(i);
                decimal ppo3SignalLine = ppo3SignalLineList.ElementAtOrDefault(i);
                decimal ppo4 = ppo4List.ElementAtOrDefault(i);
                decimal ppo4SignalLine = ppo4SignalLineList.ElementAtOrDefault(i);
                decimal ppo1Histogram = ppo1 - ppo1SignalLine;
                decimal ppoBlue = blueMult * ppo1Histogram;

                decimal prevPpo2Histogram = ppo2HistogramList.LastOrDefault();
                decimal ppo2Histogram = ppo2 - ppo2SignalLine;
                ppo2HistogramList.Add(ppo2Histogram);

                decimal ppo3Histogram = ppo3 - ppo3SignalLine;
                decimal ppoYellow = yellowMult * ppo3Histogram;

                decimal prevPpo4Histogram = ppo4HistogramList.LastOrDefault();
                decimal ppo4Histogram = ppo4 - ppo4SignalLine;
                ppo4HistogramList.Add(ppo4Histogram);

                decimal maxPpo = Math.Max(ppoBlue, Math.Max(ppoYellow, Math.Max(ppo2Histogram, ppo4Histogram)));
                decimal minPpo = Math.Min(ppoBlue, Math.Min(ppoYellow, Math.Min(ppo2Histogram, ppo4Histogram)));
                decimal currentPpo = (ppoBlue + ppoYellow + ppo2Histogram + ppo4Histogram) / 4;
                decimal ppoStochastic = maxPpo - minPpo != 0 ? MinOrMax((currentPpo - minPpo) / (maxPpo - minPpo) * 100, 100, 0) : 0;

                var signal = GetCompareSignal(ppo4Histogram - ppo2Histogram, prevPpo4Histogram - prevPpo2Histogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ppo1", ppo4List },
                { "Signal1", ppo4SignalLineList },
                { "Histogram1", ppo4HistogramList },
                { "Ppo2", ppo2List },
                { "Signal2", ppo2SignalLineList },
                { "Histogram2", ppo2HistogramList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName._4PercentagePriceOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Japanese Correlation Coefficient
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateJapaneseCorrelationCoefficient(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 50)
        {
            List<decimal> joList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

            var hList = GetMovingAverageList(stockData, maType, length1, highList);
            var lList = GetMovingAverageList(stockData, maType, length1, lowList);
            var cList = GetMovingAverageList(stockData, maType, length1, inputList);
            var highestList = GetMaxAndMinValuesList(hList, length1).Item1;
            var lowestList = GetMaxAndMinValuesList(lList, length1).Item2;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal c = cList.ElementAtOrDefault(i);
                decimal prevC = i >= length ? cList.ElementAtOrDefault(i - length) : 0;
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);
                decimal prevJo1 = i >= 1 ? joList.ElementAtOrDefault(i - 1) : 0;
                decimal prevJo2 = i >= 2 ? joList.ElementAtOrDefault(i - 2) : 0;
                decimal cChg = c - prevC;

                decimal jo = highest - lowest != 0 ? cChg / (highest - lowest) : 0;
                joList.Add(jo);

                var signal = GetCompareSignal(jo - prevJo1, prevJo1 - prevJo2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Jo", joList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = joList;
            stockData.IndicatorName = IndicatorName.JapaneseCorrelationCoefficient;

            return stockData;
        }

        /// <summary>
        /// Calculates the Jma Rsx Clone
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateJmaRsxClone(this StockData stockData, int length = 14)
        {
            List<decimal> rsxList = new();
            List<decimal> f8List = new();
            List<decimal> f28List = new();
            List<decimal> f30List = new();
            List<decimal> f38List = new();
            List<decimal> f40List = new();
            List<decimal> f48List = new();
            List<decimal> f50List = new();
            List<decimal> f58List = new();
            List<decimal> f60List = new();
            List<decimal> f68List = new();
            List<decimal> f70List = new();
            List<decimal> f78List = new();
            List<decimal> f80List = new();
            List<decimal> f88List = new();
            List<decimal> f90_List = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal f18 = (decimal)3 / (length + 2);
            decimal f20 = 1 - f18;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevRsx1 = i >= 1 ? rsxList.ElementAtOrDefault(i - 1) : 0;
                decimal prevRsx2 = i >= 2 ? rsxList.ElementAtOrDefault(i - 2) : 0;

                decimal prevF8 = f8List.LastOrDefault();
                decimal f8 = 100 * currentValue;
                f8List.Add(f8);

                decimal f10 = prevF8;
                decimal v8 = f8 - f10;

                decimal prevF28 = f28List.LastOrDefault();
                decimal f28 = (f20 * prevF28) + (f18 * v8);
                f28List.Add(f28);

                decimal prevF30 = f30List.LastOrDefault();
                decimal f30 = (f18 * f28) + (f20 * prevF30);
                f30List.Add(f30);

                decimal vC = (f28 * 1.5m) - (f30 * 0.5m);
                decimal prevF38 = f38List.LastOrDefault();
                decimal f38 = (f20 * prevF38) + (f18 * vC);
                f38List.Add(f38);

                decimal prevF40 = f40List.LastOrDefault();
                decimal f40 = (f18 * f38) + (f20 * prevF40);
                f40List.Add(f40);

                decimal v10 = (f38 * 1.5m) - (f40 * 0.5m);
                decimal prevF48 = f48List.LastOrDefault();
                decimal f48 = (f20 * prevF48) + (f18 * v10);
                f48List.Add(f48);

                decimal prevF50 = f50List.LastOrDefault();
                decimal f50 = (f18 * f48) + (f20 * prevF50);
                f50List.Add(f50);

                decimal v14 = (f48 * 1.5m) - (f50 * 0.5m);
                decimal prevF58 = f58List.LastOrDefault();
                decimal f58 = (f20 * prevF58) + (f18 * Math.Abs(v8));
                f58List.Add(f58);

                decimal prevF60 = f60List.LastOrDefault();
                decimal f60 = (f18 * f58) + (f20 * prevF60);
                f60List.Add(f60);

                decimal v18 = (f58 * 1.5m) - (f60 * 0.5m);
                decimal prevF68 = f68List.LastOrDefault();
                decimal f68 = (f20 * prevF68) + (f18 * v18);
                f68List.Add(f68);

                decimal prevF70 = f70List.LastOrDefault();
                decimal f70 = (f18 * f68) + (f20 * prevF70);
                f70List.Add(f70);

                decimal v1C = (f68 * 1.5m) - (f70 * 0.5m);
                decimal prevF78 = f78List.LastOrDefault();
                decimal f78 = (f20 * prevF78) + (f18 * v1C);
                f78List.Add(f78);

                decimal prevF80 = f80List.LastOrDefault();
                decimal f80 = (f18 * f78) + (f20 * prevF80);
                f80List.Add(f80);

                decimal v20 = (f78 * 1.5m) - (f80 * 0.5m);
                decimal prevF88 = f88List.LastOrDefault();
                decimal prevF90_ = f90_List.LastOrDefault();
                decimal f90_ = prevF90_ == 0 ? 1 : prevF88 <= prevF90_ ? prevF88 + 1 : prevF90_ + 1;
                f90_List.Add(f90_);

                decimal f88 = prevF90_ == 0 && length - 1 >= 5 ? length - 1 : 5;
                decimal f0 = f88 >= f90_ && f8 != f10 ? 1 : 0;
                decimal f90 = f88 == f90_ && f0 == 0 ? 0 : f90_;
                decimal v4_ = f88 < f90 && v20 > 0 ? MinOrMax(((v14 / v20) + 1) * 50, 100, 0) : 50;
                decimal rsx = v4_ > 100 ? 100 : v4_ < 0 ? 0 : v4_;
                rsxList.Add(rsx);

                var signal = GetRsiSignal(rsx - prevRsx1, prevRsx1 - prevRsx2, rsx, prevRsx1, 70, 30);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Rsx", rsxList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = rsxList;
            stockData.IndicatorName = IndicatorName.JmaRsxClone;

            return stockData;
        }
        
        /// <summary>
        /// Calculates the Jrc Fractal Dimension
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <param name="smoothLength"></param>
        /// <returns></returns>
        public static StockData CalculateJrcFractalDimension(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length1 = 20, int length2 = 5, int smoothLength = 5)
        {
            List<decimal> smallSumList = new();
            List<decimal> smallRangeList = new();
            List<decimal> fdList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            int wind1 = MinOrMax((length2 - 1) * length1);
            int wind2 = MinOrMax(length2 * length1);
            decimal nLog = Log(length2);

            var (highest1List, lowest1List) = GetMaxAndMinValuesList(highList, lowList, length1);
            var (highest2List, lowest2List) = GetMaxAndMinValuesList(highList, lowList, wind2);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal highest1 = highest1List.ElementAtOrDefault(i);
                decimal lowest1 = lowest1List.ElementAtOrDefault(i);
                decimal prevValue1 = i >= length1 ? inputList.ElementAtOrDefault(i - length1) : 0;
                decimal highest2 = highest2List.ElementAtOrDefault(i);
                decimal lowest2 = lowest2List.ElementAtOrDefault(i);
                decimal prevValue2 = i >= wind2 ? inputList.ElementAtOrDefault(i - wind2) : 0;
                decimal bigRange = Math.Max(prevValue2, highest2) - Math.Min(prevValue2, lowest2);

                decimal prevSmallRange = i >= wind1 ? smallRangeList.ElementAtOrDefault(i - wind1) : 0;
                decimal smallRange = Math.Max(prevValue1, highest1) - Math.Min(prevValue1, lowest1);
                smallRangeList.AddRounded(smallRange);

                decimal prevSmallSum = i >= 1 ? smallSumList.LastOrDefault() : smallRange;
                decimal smallSum = prevSmallSum + smallRange - prevSmallRange;
                smallSumList.AddRounded(smallSum);

                decimal value1 = wind1 != 0 ? smallSum / wind1 : 0;
                decimal value2 = value1 != 0 ? bigRange / value1 : 0;
                decimal temp = value2 > 0 ? Log(value2) : 0;

                decimal fd = nLog != 0 ? 2 - (temp / nLog) : 0;
                fdList.AddRounded(fd);
            }

            var jrcfdList = GetMovingAverageList(stockData, maType, smoothLength, fdList);
            var jrcfdSignalList = GetMovingAverageList(stockData, maType, smoothLength, jrcfdList);
            for (int i = 0; i < stockData.Count; i++)
            {
                var jrcfd = jrcfdList.ElementAtOrDefault(i);
                var jrcfdSignal = jrcfdSignalList.ElementAtOrDefault(i);
                var prevJrcfd = i >= 1 ? jrcfdList.ElementAtOrDefault(i - 1) : 0;
                var prevJrcfdSignal = i >= 1 ? jrcfdSignalList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(jrcfd - jrcfdSignal, prevJrcfd - prevJrcfdSignal, true);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Jrcfd", jrcfdList },
                { "Signal", jrcfdSignalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = jrcfdList;
            stockData.IndicatorName = IndicatorName.JrcFractalDimension;

            return stockData;
        }

        /// <summary>
        /// Calculates the Zweig Market Breadth Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateZweigMarketBreadthIndicator(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 10)
        {
            List<decimal> advDiffList = new();
            List<decimal> advancesList = new();
            List<decimal> declinesList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal advance = currentValue > prevValue ? 1 : 0;
                advancesList.Add(advance);

                decimal decline = currentValue < prevValue ? 1 : 0;
                declinesList.Add(decline);

                decimal advSum = advancesList.TakeLastExt(length).Sum();
                decimal decSum = declinesList.TakeLastExt(length).Sum();

                decimal advDiff = advSum + decSum != 0 ? advSum / (advSum + decSum) : 0;
                advDiffList.Add(advDiff);
            }

            var zmbtiList = GetMovingAverageList(stockData, maType, length, advDiffList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevZmbti1 = i >= 1 ? zmbtiList.ElementAtOrDefault(i - 1) : 0;
                decimal prevZmbti2 = i >= 2 ? zmbtiList.ElementAtOrDefault(i - 2) : 0;
                decimal zmbti = zmbtiList.ElementAtOrDefault(i);

                var signal = GetRsiSignal(zmbti - prevZmbti1, prevZmbti1 - prevZmbti2, zmbti, prevZmbti1, 0.615m, 0.4m);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Zmbti", zmbtiList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = zmbtiList;
            stockData.IndicatorName = IndicatorName.ZweigMarketBreadthIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Z Distance From Vwap Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateZDistanceFromVwapIndicator(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.VolumeWeightedAveragePrice, int length = 20)
        {
            List<decimal> zscoreList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var vwapList = GetMovingAverageList(stockData, maType, length, inputList);
            stockData.CustomValuesList = vwapList;
            var vwapSdList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevZScore1 = i >= 1 ? zscoreList.ElementAtOrDefault(i - 1) : 0;
                decimal prevZScore2 = i >= 2 ? zscoreList.ElementAtOrDefault(i - 2) : 0;
                decimal mean = vwapList.ElementAtOrDefault(i);
                decimal vwapsd = vwapSdList.ElementAtOrDefault(i);

                decimal zscore = vwapsd != 0 ? (currentValue - mean) / vwapsd : 0;
                zscoreList.Add(zscore);

                var signal = GetRsiSignal(zscore - prevZScore1, prevZScore1 - prevZScore2, zscore, prevZScore1, 2, -2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Zscore", zscoreList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = zscoreList;
            stockData.IndicatorName = IndicatorName.ZDistanceFromVwap;

            return stockData;
        }

        /// <summary>
        /// Calculates the Z Score
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="matype"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateZScore(this StockData stockData, MovingAvgType matype = MovingAvgType.SimpleMovingAverage, int length = 14)
        {
            List<decimal> zScorePopulationList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, matype, length, inputList);
            var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal dev = currentValue - sma;
                decimal stdDevPopulation = stdDevList.ElementAtOrDefault(i);

                decimal prevZScorePopulation = zScorePopulationList.LastOrDefault();
                decimal zScorePopulation = stdDevPopulation != 0 ? dev / stdDevPopulation : 0;
                zScorePopulationList.Add(zScorePopulation);

                var signal = GetCompareSignal(zScorePopulation, prevZScorePopulation);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Zscore", zScorePopulationList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = zScorePopulationList;
            stockData.IndicatorName = IndicatorName.ZScore;

            return stockData;
        }

        /// <summary>
        /// Calculates the Zero Lag Smoothed Cycle
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateZeroLagSmoothedCycle(this StockData stockData, int length = 100)
        {
            List<decimal> ax1List = new();
            List<decimal> lx1List = new();
            List<decimal> ax2List = new();
            List<decimal> lx2List = new();
            List<decimal> ax3List = new();
            List<decimal> lcoList = new();
            List<decimal> filterList = new();
            List<decimal> lcoSma1List = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

            var linregList = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal linreg = linregList.ElementAtOrDefault(i);

                decimal ax1 = currentValue - linreg;
                ax1List.Add(ax1);
            }

            stockData.CustomValuesList = ax1List;
            var ax1LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ax1 = ax1List.ElementAtOrDefault(i);
                decimal ax1Linreg = ax1LinregList.ElementAtOrDefault(i);

                decimal lx1 = ax1 + (ax1 - ax1Linreg);
                lx1List.Add(lx1);
            }

            stockData.CustomValuesList = lx1List;
            var lx1LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal lx1 = lx1List.ElementAtOrDefault(i);
                decimal lx1Linreg = lx1LinregList.ElementAtOrDefault(i);

                decimal ax2 = lx1 - lx1Linreg;
                ax2List.Add(ax2);
            }

            stockData.CustomValuesList = ax2List;
            var ax2LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ax2 = ax2List.ElementAtOrDefault(i);
                decimal ax2Linreg = ax2LinregList.ElementAtOrDefault(i);

                decimal lx2 = ax2 + (ax2 - ax2Linreg);
                lx2List.Add(lx2);
            }

            stockData.CustomValuesList = lx2List;
            var lx2LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal lx2 = lx2List.ElementAtOrDefault(i);
                decimal lx2Linreg = lx2LinregList.ElementAtOrDefault(i);

                decimal ax3 = lx2 - lx2Linreg;
                ax3List.Add(ax3);
            }

            stockData.CustomValuesList = ax3List;
            var ax3LinregList = CalculateLinearRegression(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ax3 = ax3List.ElementAtOrDefault(i);
                decimal ax3Linreg = ax3LinregList.ElementAtOrDefault(i);

                decimal prevLco = lcoList.LastOrDefault();
                decimal lco = ax3 + (ax3 - ax3Linreg);
                lcoList.Add(lco);

                decimal lcoSma1 = lcoList.TakeLastExt(length1).Average();
                lcoSma1List.Add(lcoSma1);

                decimal lcoSma2 = lcoSma1List.TakeLastExt(length1).Average();
                decimal prevFilter = filterList.LastOrDefault();
                decimal filter = -lcoSma2 * 2;
                filterList.Add(filter);

                var signal = GetCompareSignal(lco - filter, prevLco - prevFilter);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Lco", lcoList },
                { "Filter", filterList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = filterList;
            stockData.IndicatorName = IndicatorName.ZeroLagSmoothedCycle;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bayesian Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="stdDevMult"></param>
        /// <param name="lowerThreshold"></param>
        /// <returns></returns>
        public static StockData CalculateBayesianOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 20, decimal stdDevMult = 2.5m, decimal lowerThreshold = 15)
        {
            List<decimal> probBbUpperUpSeqList = new();
            List<decimal> probBbUpperDownSeqList = new();
            List<decimal> probBbBasisUpSeqList = new();
            List<decimal> probBbBasisUpList = new();
            List<decimal> probBbBasisDownSeqList = new();
            List<decimal> probBbBasisDownList = new();
            List<decimal> sigmaProbsDownList = new();
            List<decimal> sigmaProbsUpList = new();
            List<decimal> probPrimeList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var bbList = CalculateBollingerBands(stockData, stdDevMult, maType, length);
            var upperBbList = bbList.OutputValues["UpperBand"];
            var basisList = bbList.OutputValues["MiddleBand"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal upperBb = upperBbList.ElementAtOrDefault(i);
                decimal basis = basisList.ElementAtOrDefault(i);

                decimal probBbUpperUpSeq = currentValue > upperBb ? 1 : 0;
                probBbUpperUpSeqList.Add(probBbUpperUpSeq);

                decimal probBbUpperUp = probBbUpperUpSeqList.TakeLastExt(length).Average();

                decimal probBbUpperDownSeq = currentValue < upperBb ? 1 : 0;
                probBbUpperDownSeqList.Add(probBbUpperDownSeq);

                decimal probBbUpperDown = probBbUpperDownSeqList.TakeLastExt(length).Average();
                decimal probUpBbUpper = probBbUpperUp + probBbUpperDown != 0 ? probBbUpperUp / (probBbUpperUp + probBbUpperDown) : 0;
                decimal probDownBbUpper = probBbUpperUp + probBbUpperDown != 0 ? probBbUpperDown / (probBbUpperUp + probBbUpperDown) : 0;

                decimal probBbBasisUpSeq = currentValue > basis ? 1 : 0;
                probBbBasisUpSeqList.Add(probBbBasisUpSeq);

                decimal probBbBasisUp = probBbBasisUpSeqList.TakeLastExt(length).Average();
                probBbBasisUpList.Add(probBbBasisUp);

                decimal probBbBasisDownSeq = currentValue < basis ? 1 : 0;
                probBbBasisDownSeqList.Add(probBbBasisDownSeq);

                decimal probBbBasisDown = probBbBasisDownSeqList.TakeLastExt(length).Average();
                probBbBasisDownList.Add(probBbBasisDown);

                decimal probUpBbBasis = probBbBasisUp + probBbBasisDown != 0 ? probBbBasisUp / (probBbBasisUp + probBbBasisDown) : 0;
                decimal probDownBbBasis = probBbBasisUp + probBbBasisDown != 0 ? probBbBasisDown / (probBbBasisUp + probBbBasisDown) : 0;

                decimal prevSigmaProbsDown = sigmaProbsDownList.LastOrDefault();
                decimal sigmaProbsDown = probUpBbUpper != 0 && probUpBbBasis != 0 ? ((probUpBbUpper * probUpBbBasis) / (probUpBbUpper * probUpBbBasis)) +
                    ((1 - probUpBbUpper) * (1 - probUpBbBasis)) : 0;
                sigmaProbsDownList.Add(sigmaProbsDown);

                decimal prevSigmaProbsUp = sigmaProbsUpList.LastOrDefault();
                decimal sigmaProbsUp = probDownBbUpper != 0 && probDownBbBasis != 0 ? ((probDownBbUpper * probDownBbBasis) / (probDownBbUpper * probDownBbBasis)) +
                    ((1 - probDownBbUpper) * (1 - probDownBbBasis)) : 0;
                sigmaProbsUpList.Add(sigmaProbsUp);

                decimal prevProbPrime = probPrimeList.LastOrDefault();
                decimal probPrime = sigmaProbsDown != 0 && sigmaProbsUp != 0 ? ((sigmaProbsDown * sigmaProbsUp) / (sigmaProbsDown * sigmaProbsUp)) +
                    ((1 - sigmaProbsDown) * (1 - sigmaProbsUp)) : 0;
                probPrimeList.Add(probPrime);

                bool longUsingProbPrime = probPrime > lowerThreshold / 100 && prevProbPrime == 0;
                bool longUsingSigmaProbsUp = sigmaProbsUp < 1 && prevSigmaProbsUp == 1;
                bool shortUsingProbPrime = probPrime == 0 && prevProbPrime > lowerThreshold / 100;
                bool shortUsingSigmaProbsDown = sigmaProbsDown < 1 && prevSigmaProbsDown == 1;

                var signal = GetConditionSignal(longUsingProbPrime || longUsingSigmaProbsUp, shortUsingProbPrime || shortUsingSigmaProbsDown);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "SigmaProbsDown", sigmaProbsDownList },
                { "SigmaProbsUp", sigmaProbsUpList },
                { "ProbPrime", probPrimeList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.BayesianOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bear Power Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateBearPowerIndicator(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> bpiList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal close = inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal open = openList.ElementAtOrDefault(i);
                decimal high = highList.ElementAtOrDefault(i);
                decimal low = lowList.ElementAtOrDefault(i);

                decimal bpi = close < open ? high - low : prevClose > open ? Math.Max(close - open, high - low) :
                    close > open ? Math.Max(open - low, high - close) : prevClose > open ? Math.Max(prevClose - low, high - close) :
                    high - close > close - low ? high - low : prevClose > open ? Math.Max(prevClose - open, high - low) :
                    high - close < close - low ? open - low : close > open ? Math.Max(close - low, high - close) :
                    close > open ? Math.Max(prevClose - open, high - close) : prevClose < open ? Math.Max(open - low, high - close) : high - low;
                bpiList.Add(bpi);
            }

            var bpiEmaList = GetMovingAverageList(stockData, maType, length, bpiList);
            for (int i = 0; i < stockData.Count; i++)
            {
                var bpi = bpiList.ElementAtOrDefault(i);
                var bpiEma = bpiEmaList.ElementAtOrDefault(i);
                var prevBpi = i >= 1 ? bpiList.ElementAtOrDefault(i - 1) : 0;
                var prevBpiEma = i >= 1 ? bpiEmaList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(bpi - bpiEma, prevBpi - prevBpiEma, true);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "BearPower", bpiList },
                { "Signal", bpiEmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bpiList;
            stockData.IndicatorName = IndicatorName.BearPowerIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bull Power Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateBullPowerIndicator(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> bpiList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal close = inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal open = openList.ElementAtOrDefault(i);
                decimal high = highList.ElementAtOrDefault(i);
                decimal low = lowList.ElementAtOrDefault(i);

                decimal bpi = close < open ? Math.Max(high - open, close - low) : prevClose < open ? Math.Max(high - prevClose, close - low) :
                    close > open ? Math.Max(open - prevClose, high - low) : prevClose > open ? high - low :
                    high - close > close - low ? high - open : prevClose < open ? Math.Max(high - prevClose, close - low) :
                    high - close < close - low ? Math.Max(open - close, high - low) : prevClose > open ? high - low :
                    prevClose > open ? Math.Max(high - open, close - low) : prevClose < open ? Math.Max(open - close, high - low) : high - low;
                bpiList.Add(bpi);
            }

            var bpiEmaList = GetMovingAverageList(stockData, maType, length, bpiList);
            for (int i = 0; i < stockData.Count; i++)
            {
                var bpi = bpiList.ElementAtOrDefault(i);
                var bpiEma = bpiEmaList.ElementAtOrDefault(i);
                var prevBpi = i >= 1 ? bpiList.ElementAtOrDefault(i - 1) : 0;
                var prevBpiEma = i >= 1 ? bpiEmaList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(bpi - bpiEma, prevBpi - prevBpiEma, true);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "BullPower", bpiList },
                { "Signal", bpiEmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bpiList;
            stockData.IndicatorName = IndicatorName.BullPowerIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Belkhayate Timing
        /// </summary>
        /// <param name="stockData"></param>
        /// <returns></returns>
        public static StockData CalculateBelkhayateTiming(this StockData stockData)
        {
            List<decimal> bList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevHigh1 = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow1 = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHigh2 = i >= 2 ? highList.ElementAtOrDefault(i - 2) : 0;
                decimal prevLow2 = i >= 2 ? lowList.ElementAtOrDefault(i - 2) : 0;
                decimal prevHigh3 = i >= 3 ? highList.ElementAtOrDefault(i - 3) : 0;
                decimal prevLow3 = i >= 3 ? lowList.ElementAtOrDefault(i - 3) : 0;
                decimal prevHigh4 = i >= 4 ? highList.ElementAtOrDefault(i - 4) : 0;
                decimal prevLow4 = i >= 4 ? lowList.ElementAtOrDefault(i - 4) : 0;
                decimal prevB1 = i >= 1 ? bList.ElementAtOrDefault(i - 1) : 0;
                decimal prevB2 = i >= 2 ? bList.ElementAtOrDefault(i - 2) : 0;
                decimal middle = (((currentHigh + currentLow) / 2) + ((prevHigh1 + prevLow1) / 2) + ((prevHigh2 + prevLow2) / 2) + 
                    ((prevHigh3 + prevLow3) / 2) + ((prevHigh4 + prevLow4) / 2)) / 5;
                decimal scale = ((currentHigh - currentLow + (prevHigh1 - prevLow1) + (prevHigh2 - prevLow2) + (prevHigh3 - prevLow3) + 
                    (prevHigh4 - prevLow4)) / 5) * 0.2m;

                decimal b = scale != 0 ? (currentValue - middle) / scale : 0;
                bList.Add(b);

                var signal = GetRsiSignal(b - prevB1, prevB1 - prevB2, b, prevB1, 4, -4);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Belkhayate", bList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bList;
            stockData.IndicatorName = IndicatorName.BelkhayateTiming;

            return stockData;
        }

        /// <summary>
        /// Calculates the Better Volume Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="lbLength"></param>
        /// <returns></returns>
        public static StockData CalculateBetterVolumeIndicator(this StockData stockData, int length = 8, int lbLength = 2)
        {
            List<decimal> v1List = new();
            List<decimal> v2List = new();
            List<decimal> v3List = new();
            List<decimal> v4List = new();
            List<decimal> v5List = new();
            List<decimal> v6List = new();
            List<decimal> v7List = new();
            List<decimal> v8List = new();
            List<decimal> v9List = new();
            List<decimal> v10List = new();
            List<decimal> v11List = new();
            List<decimal> v12List = new();
            List<decimal> v13List = new();
            List<decimal> v14List = new();
            List<decimal> v15List = new();
            List<decimal> v16List = new();
            List<decimal> v17List = new();
            List<decimal> v18List = new();
            List<decimal> v19List = new();
            List<decimal> v20List = new();
            List<decimal> v21List = new();
            List<decimal> v22List = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, lbLength);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal currentOpen = openList.ElementAtOrDefault(i);
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal highLowRange = highest - lowest;
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;
                decimal range = CalculateTrueRange(currentHigh, currentLow, prevClose);

                decimal prevV1 = v1List.LastOrDefault();
                decimal v1 = currentClose > currentOpen ? range / ((2 * range) + currentOpen - currentClose) * currentVolume :
                    currentClose < currentOpen ? (range + currentClose - currentOpen) / ((2 * range) + currentClose - currentOpen) * currentVolume : 
                    0.5m * currentVolume;
                v1List.Add(v1);

                decimal prevV2 = v2List.LastOrDefault();
                decimal v2 = currentVolume - v1;
                v2List.Add(v2);

                decimal prevV3 = v3List.LastOrDefault();
                decimal v3 = v1 + v2;
                v3List.Add(v3);

                decimal v4 = v1 * range;
                v4List.AddRounded(v4);

                decimal v5 = (v1 - v2) * range;
                v5List.AddRounded(v5);

                decimal v6 = v2 * range;
                v6List.AddRounded(v6);

                decimal v7 = (v2 - v1) * range;
                v7List.AddRounded(v7);

                decimal v8 = range != 0 ? v1 / range : 0;
                v8List.AddRounded(v8);

                decimal v9 = range != 0 ? (v1 - v2) / range : 0;
                v9List.AddRounded(v9);

                decimal v10 = range != 0 ? v2 / range : 0;
                v10List.AddRounded(v10);

                decimal v11 = range != 0 ? (v2 - v1) / range : 0;
                v11List.AddRounded(v11);

                decimal v12 = range != 0 ? v3 / range : 0;
                v12List.AddRounded(v12);

                decimal v13 = v3 + prevV3;
                v13List.AddRounded(v13);

                decimal v14 = (v1 + prevV1) * highLowRange;
                v14List.AddRounded(v14);

                decimal v15 = (v1 + prevV1 - v2 - prevV2) * highLowRange;
                v15List.AddRounded(v15);

                decimal v16 = (v2 + prevV2) * highLowRange;
                v16List.AddRounded(v16);

                decimal v17 = (v2 + prevV2 - v1 - prevV1) * highLowRange;
                v17List.AddRounded(v17);

                decimal v18 = highLowRange != 0 ? (v1 + prevV1) / highLowRange : 0;
                v18List.AddRounded(v18);

                decimal v19 = highLowRange != 0 ? (v1 + prevV1 - v2 - prevV2) / highLowRange : 0;
                v19List.AddRounded(v19);

                decimal v20 = highLowRange != 0 ? (v2 + prevV2) / highLowRange : 0;
                v20List.AddRounded(v20);

                decimal v21 = highLowRange != 0 ? (v2 + prevV2 - v1 - prevV1) / highLowRange : 0;
                v21List.AddRounded(v21);

                decimal v22 = highLowRange != 0 ? v13 / highLowRange : 0;
                v22List.AddRounded(v22);

                bool c1 = v3 == v3List.TakeLastExt(length).Min();
                bool c2 = v4 == v4List.TakeLastExt(length).Max() && currentClose > currentOpen;
                bool c3 = v5 == v5List.TakeLastExt(length).Max() && currentClose > currentOpen;
                bool c4 = v6 == v6List.TakeLastExt(length).Max() && currentClose < currentOpen;
                bool c5 = v7 == v7List.TakeLastExt(length).Max() && currentClose < currentOpen;
                bool c6 = v8 == v8List.TakeLastExt(length).Min() && currentClose < currentOpen;
                bool c7 = v9 == v9List.TakeLastExt(length).Min() && currentClose < currentOpen;
                bool c8 = v10 == v10List.TakeLastExt(length).Min() && currentClose > currentOpen;
                bool c9 = v11 == v11List.TakeLastExt(length).Min() && currentClose > currentOpen;
                bool c10 = v12 == v12List.TakeLastExt(length).Max();
                bool c11 = v13 == v13List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose > prevOpen;
                bool c12 = v14 == v14List.TakeLastExt(length).Max() && currentClose > currentOpen && prevClose > prevOpen;
                bool c13 = v15 == v15List.TakeLastExt(length).Max() && currentClose > currentOpen && prevClose < prevOpen;
                bool c14 = v16 == v16List.TakeLastExt(length).Min() && currentClose < currentOpen && prevClose < prevOpen;
                bool c15 = v17 == v17List.TakeLastExt(length).Min() && currentClose < currentOpen && prevClose < prevOpen;
                bool c16 = v18 == v18List.TakeLastExt(length).Min() && currentClose < currentOpen && prevClose < prevOpen;
                bool c17 = v19 == v19List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose < prevOpen;
                bool c18 = v20 == v20List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose > prevOpen;
                bool c19 = v21 == v21List.TakeLastExt(length).Min() && currentClose > currentOpen && prevClose > prevOpen;
                bool c20 = v22 == v22List.TakeLastExt(length).Min();
                bool climaxUp = c2 || c3 || c8 || c9 || c12 || c13 || c18 || c19;
                bool climaxDown = c4 || c5 || c6 || c7 || c14 || c15 || c16 || c17;
                bool churn = c10 || c20;
                bool lowVolue = c1 || c11;

                var signal = GetConditionSignal(climaxUp, climaxDown);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Bvi", v1List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = v1List;
            stockData.IndicatorName = IndicatorName.BetterVolumeIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bilateral Stochastic Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="signalLength"></param>
        /// <returns></returns>
        public static StockData CalculateBilateralStochasticOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 100, int signalLength = 20)
        {
            List<decimal> bullList = new();
            List<decimal> bearList = new();
            List<decimal> rangeList = new();
            List<decimal> maxList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);
            var (highestList, lowestList) = GetMaxAndMinValuesList(smaList, length);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);

                decimal range = highest - lowest;
                rangeList.Add(range);
            }

            var rangeSmaList = GetMovingAverageList(stockData, maType, length, rangeList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);
                decimal rangeSma = rangeSmaList.ElementAtOrDefault(i);

                decimal bull = rangeSma != 0 ? (sma / rangeSma) - (lowest / rangeSma) : 0;
                bullList.Add(bull);

                decimal bear = rangeSma != 0 ? Math.Abs((sma / rangeSma) - (highest / rangeSma)) : 0;
                bearList.Add(bear);

                decimal max = Math.Max(bull, bear);
                maxList.Add(max);
            }

            var signalList = GetMovingAverageList(stockData, maType, signalLength, maxList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal bull = bullList.ElementAtOrDefault(i);
                decimal bear = bearList.ElementAtOrDefault(i);
                decimal sig = signalList.ElementAtOrDefault(i);

                var signal = GetConditionSignal(bull > bear || bull > sig, bear > bull || bull < sig);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Bull", bullList },
                { "Bear", bearList },
                { "Bso", maxList },
                { "Signal", signalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = maxList;
            stockData.IndicatorName = IndicatorName.BilateralStochasticOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Buff Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="fastLength"></param>
        /// <param name="slowLength"></param>
        /// <returns></returns>
        public static StockData CalculateBuffAverage(this StockData stockData, int fastLength = 5, int slowLength = 20)
        {
            List<decimal> priceVolList = new();
            List<decimal> fastBuffList = new();
            List<decimal> slowBuffList = new();
            List<decimal> tempVolumeList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);

                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                tempVolumeList.Add(currentVolume);

                decimal priceVol = currentValue * currentVolume;
                priceVolList.Add(priceVol);

                decimal fastBuffNum = priceVolList.TakeLastExt(fastLength).Sum();
                decimal fastBuffDenom = tempVolumeList.TakeLastExt(fastLength).Sum();

                decimal prevFastBuff = fastBuffList.LastOrDefault();
                decimal fastBuff = fastBuffDenom != 0 ? fastBuffNum / fastBuffDenom : 0;
                fastBuffList.Add(fastBuff);

                decimal slowBuffNum = priceVolList.TakeLastExt(slowLength).Sum();
                decimal slowBuffDenom = tempVolumeList.TakeLastExt(slowLength).Sum();

                decimal prevSlowBuff = slowBuffList.LastOrDefault();
                decimal slowBuff = slowBuffDenom != 0 ? slowBuffNum / slowBuffDenom : 0;
                slowBuffList.Add(slowBuff);

                var signal = GetCompareSignal(fastBuff - slowBuff, prevFastBuff - prevSlowBuff);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "FastBuff", fastBuffList },
                { "SlowBuff", slowBuffList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.BuffAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Uber Trend Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateUberTrendIndicator(this StockData stockData, int length = 14)
        {
            List<decimal> advList = new();
            List<decimal> decList = new();
            List<decimal> advVolList = new();
            List<decimal> decVolList = new();
            List<decimal> utiList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevUti1 = i >= 1 ? utiList.ElementAtOrDefault(i - 1) : 0;
                decimal prevUti2 = i >= 2 ? utiList.ElementAtOrDefault(i - 2) : 0;

                decimal adv = currentValue > prevValue ? currentValue - prevValue : 0;
                advList.Add(adv);

                decimal dec = currentValue < prevValue ? prevValue - currentValue : 0;
                decList.Add(dec);

                decimal advSum = advList.TakeLastExt(length).Sum();
                decimal decSum = decList.TakeLastExt(length).Sum();

                decimal advVol = currentValue > prevValue && advSum != 0 ? currentVolume / advSum : 0;
                advVolList.Add(advVol);

                decimal decVol = currentValue < prevValue && decSum != 0 ? currentVolume / decSum : 0;
                decVolList.Add(decVol);

                decimal advVolSum = advVolList.TakeLastExt(length).Sum();
                decimal decVolSum = decVolList.TakeLastExt(length).Sum();
                decimal top = decSum != 0 ? advSum / decSum : 0;
                decimal bot = decVolSum != 0 ? advVolSum / decVolSum : 0;
                decimal ut = bot != 0 ? top / bot : 0;
                decimal utRev = top != 0 ? -1 * bot / top : 0;

                decimal uti = ut + 1 != 0 ? (ut - 1) / (ut + 1) : 0;
                utiList.Add(uti);

                var signal = GetCompareSignal(uti - prevUti1, prevUti1 - prevUti2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Uti", utiList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = utiList;
            stockData.IndicatorName = IndicatorName.UberTrendIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Ultimate Volatility Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateUltimateVolatilityIndicator(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> uviList = new();
            List<decimal> absList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, openList, _) = GetInputValuesList(stockData);

            var maList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentOpen = openList.ElementAtOrDefault(i);
                decimal currentClose = inputList.ElementAtOrDefault(i);
                decimal currentMa = maList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevMa = i >= 1 ? maList.ElementAtOrDefault(i - 1) : 0;

                decimal abs = Math.Abs(currentClose - currentOpen);
                absList.Add(abs);

                decimal uvi = (decimal)1 / length * absList.TakeLastExt(length).Sum();
                uviList.Add(uvi);

                var signal = GetVolatilitySignal(currentClose - currentMa, prevClose - prevMa, uvi, 1);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Uvi", uviList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = uviList;
            stockData.IndicatorName = IndicatorName.UltimateVolatilityIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Ultimate Trader Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="lbLength"></param>
        /// <param name="smoothLength"></param>
        /// <param name="rangeLength"></param>
        /// <returns></returns>
        public static StockData CalculateUltimateTraderOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, 
            int length = 10, int lbLength = 5, int smoothLength = 4, int rangeLength = 2)
        {
            List<decimal> dxList = new();
            List<decimal> dxiList = new();
            List<decimal> trList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, rangeLength);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
                trList.AddRounded(tr);
            }

            stockData.CustomValuesList = trList;
            var trStoList = CalculateStochasticOscillator(stockData, maType, lbLength, lbLength).CustomValuesList;
            stockData.CustomValuesList = volumeList;
            var vStoList = CalculateStochasticOscillator(stockData, maType, lbLength, lbLength).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal close = inputList.ElementAtOrDefault(i);
                decimal body = close - openList.ElementAtOrDefault(i);
                decimal high = highList.ElementAtOrDefault(i);
                decimal low = lowList.ElementAtOrDefault(i);
                decimal range = high - low;
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal c = close - prevClose;
                decimal sign = Math.Sign(c);
                decimal highest = highestList.ElementAtOrDefault(i);
                decimal lowest = lowestList.ElementAtOrDefault(i);
                decimal vSto = vStoList.ElementAtOrDefault(i);
                decimal trSto = trStoList.ElementAtOrDefault(i);
                decimal k1 = range != 0 ? body / range * 100 : 0;
                decimal k2 = range == 0 ? 0 : ((close - low) / range * 100 * 2) - 100;
                decimal k3 = c == 0 || highest - lowest == 0 ? 0 : ((close - lowest) / (highest - lowest) * 100 * 2) - 100;
                decimal k4 = highest - lowest != 0 ? c / (highest - lowest) * 100 : 0;
                decimal k5 = sign * trSto;
                decimal k6 = sign * vSto;
                decimal bullScore = Math.Max(0, k1) + Math.Max(0, k2) + Math.Max(0, k3) + Math.Max(0, k4) + Math.Max(0, k5) + Math.Max(0, k6);
                decimal bearScore = -1 * (Math.Min(0, k1) + Math.Min(0, k2) + Math.Min(0, k3) + Math.Min(0, k4) + Math.Min(0, k5) + Math.Min(0, k6));

                decimal dx = bearScore != 0 ? bullScore / bearScore : 0;
                dxList.Add(dx);

                decimal dxi = (2 * (100 - (100 / (1 + dx)))) - 100;
                dxiList.Add(dxi);
            }

            var dxiavgList = GetMovingAverageList(stockData, maType, lbLength, dxiList);
            var dxisList = GetMovingAverageList(stockData, maType, smoothLength, dxiavgList);
            var dxissList = GetMovingAverageList(stockData, maType, smoothLength, dxisList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal dxis = dxisList.ElementAtOrDefault(i);
                decimal dxiss = dxissList.ElementAtOrDefault(i);
                decimal prevDxis = i >= 1 ? dxisList.ElementAtOrDefault(i - 1) : 0;
                decimal prevDxiss = i >= 1 ? dxissList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(dxis - dxiss, prevDxis - prevDxiss);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Uto", dxisList },
                { "Signal", dxissList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = dxisList;
            stockData.IndicatorName = IndicatorName.UltimateTraderOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Upside Potential Ratio
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="bmk"></param>
        /// <returns></returns>
        public static StockData CalculateUpsidePotentialRatio(this StockData stockData, int length = 30, decimal bmk = 0.05m)
        {
            List<decimal> retList = new();
            List<decimal> upsidePotentialList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal barMin = 60 * 24;
            decimal minPerYr = 60 * 24 * 30 * 12;
            decimal barsPerYr = minPerYr / barMin;
            decimal ratio = (decimal)1 / length;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
                decimal bench = Pow(1 + bmk, length / (double)barsPerYr) - 1;

                decimal ret = prevValue != 0 ? (currentValue / prevValue) - 1 : 0;
                retList.Add(ret);

                decimal downSide = 0, upSide = 0;
                for (int j = i - (length + 1); j <= i; j++)
                {
                    decimal iValue = j >= 0 && i >= j ? retList.ElementAtOrDefault(i - j) : 0;

                    if (iValue < bench)
                    {
                        downSide += Pow(iValue - bench, 2) * ratio;
                    }
                    if (iValue > bench)
                    {
                        upSide += (iValue - bench) * ratio;
                    }
                }

                decimal prevUpsidePotential = upsidePotentialList.LastOrDefault();
                decimal upsidePotential = downSide >= 0 ? upSide / Sqrt(downSide) : 0;
                upsidePotentialList.Add(upsidePotential);

                var signal = GetCompareSignal(upsidePotential - 5, prevUpsidePotential - 5);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Upr", upsidePotentialList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = upsidePotentialList;
            stockData.IndicatorName = IndicatorName.UpsidePotentialRatio;

            return stockData;
        }

        /// <summary>
        /// Calculates the Ultimate Momentum Indicator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="inputName"></param>
        /// <param name="maType"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <param name="length3"></param>
        /// <param name="length4"></param>
        /// <param name="length5"></param>
        /// <param name="length6"></param>
        /// <param name="stdDevMult"></param>
        /// <returns></returns>
        public static StockData CalculateUltimateMomentumIndicator(this StockData stockData, InputName inputName = InputName.TypicalPrice, 
            MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length1 = 13, int length2 = 19, int length3 = 21, int length4 = 39, 
            int length5 = 50, int length6 = 200, decimal stdDevMult = 1.5m)
        {
            List<decimal> utmList = new();
            List<Signal> signalsList = new();

            var moVar = CalculateMcClellanOscillator(stockData, maType, fastLength: length2, slowLength: length4);
            var advSumList = moVar.OutputValues["AdvSum"];
            var decSumList = moVar.OutputValues["DecSum"];
            var moList = moVar.OutputValues["Mo"];
            var bbPctList = CalculateBollingerBandsPercentB(stockData, stdDevMult, maType, length5).CustomValuesList;
            var mfi1List = CalculateMoneyFlowIndex(stockData, inputName, length2).CustomValuesList;
            var mfi2List = CalculateMoneyFlowIndex(stockData, inputName, length3).CustomValuesList;
            var mfi3List = CalculateMoneyFlowIndex(stockData, inputName, length4).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal mo = moList.ElementAtOrDefault(i);
                decimal bbPct = bbPctList.ElementAtOrDefault(i);
                decimal mfi1 = mfi1List.ElementAtOrDefault(i);
                decimal mfi2 = mfi2List.ElementAtOrDefault(i);
                decimal mfi3 = mfi3List.ElementAtOrDefault(i);
                decimal advSum = advSumList.ElementAtOrDefault(i);
                decimal decSum = decSumList.ElementAtOrDefault(i);
                decimal ratio = decSum != 0 ? advSum / decSum : 0;

                decimal utm = (200 * bbPct) + (100 * ratio) + (2 * mo) + (1.5m * mfi3) + (3 * mfi2) + (3 * mfi1);
                utmList.Add(utm);
            }

            stockData.CustomValuesList = utmList;
            var utmRsiList = CalculateRelativeStrengthIndex(stockData, maType, length1, length1).CustomValuesList;
            var utmiList = GetMovingAverageList(stockData, maType, length1, utmRsiList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal utmi = utmiList.ElementAtOrDefault(i);
                decimal prevUtmi1 = i >= 1 ? utmiList.ElementAtOrDefault(i - 1) : 0;
                decimal prevUtmi2 = i >= 2 ? utmiList.ElementAtOrDefault(i - 2) : 0;

                var signal = GetCompareSignal(utmi - prevUtmi1, prevUtmi1 - prevUtmi2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Utm", utmiList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = utmiList;
            stockData.IndicatorName = IndicatorName.UltimateMomentumIndicator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Upside Downside Volume
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateUpsideDownsideVolume(this StockData stockData, int length = 50)
        {
            List<decimal> upVolList = new();
            List<decimal> downVolList = new();
            List<decimal> upDownVolumeList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal upVol = currentValue > prevValue ? currentVolume : 0;
                upVolList.Add(upVol);

                decimal downVol = currentValue < prevValue ? currentVolume * -1 : 0;
                downVolList.Add(downVol);

                decimal upVolSum = upVolList.TakeLastExt(length).Sum();
                decimal downVolSum = downVolList.TakeLastExt(length).Sum();

                decimal prevUpDownVol = upDownVolumeList.LastOrDefault();
                decimal upDownVol = downVolSum != 0 ? upVolSum / downVolSum : 0;
                upDownVolumeList.Add(upDownVol);

                var signal = GetCompareSignal(upDownVol, prevUpDownVol);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Udv", upDownVolumeList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = upDownVolumeList;
            stockData.IndicatorName = IndicatorName.UpsideDownsideVolume;

            return stockData;
        }

        /// <summary>
        /// Calculates the Uhl Ma Crossover System
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateUhlMaCrossoverSystem(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 100)
        {
            List<decimal> cmaList = new();
            List<decimal> ctsList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);
            var varList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal prevVar = i >= length ? varList.ElementAtOrDefault(i - length) : 0;
                decimal prevCma = i >= 1 ? cmaList.LastOrDefault() : currentValue;
                decimal prevCts = i >= 1 ? ctsList.LastOrDefault() : currentValue;
                decimal secma = Pow(sma - prevCma, 2);
                decimal sects = Pow(currentValue - prevCts, 2);
                decimal ka = prevVar < secma && secma != 0 ? 1 - (prevVar / secma) : 0;
                decimal kb = prevVar < sects && sects != 0 ? 1 - (prevVar / sects) : 0;

                decimal cma = (ka * sma) + ((1 - ka) * prevCma);
                cmaList.Add(cma);

                decimal cts = (kb * currentValue) + ((1 - kb) * prevCts);
                ctsList.Add(cts);

                var signal = GetCompareSignal(cts - cma, prevCts - prevCma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Cts", ctsList },
                { "Cma", cmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.UhlMaCrossoverSystem;

            return stockData;
        }

        /// <summary>
        /// Calculates the McClellan Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="fastLength"></param>
        /// <param name="slowLength"></param>
        /// <param name="signalLength"></param>
        /// <param name="mult"></param>
        /// <returns></returns>
        public static StockData CalculateMcClellanOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
            int fastLength = 19, int slowLength = 39, int signalLength = 9, decimal mult = 1000)
        {
            List<decimal> advancesList = new();
            List<decimal> declinesList = new();
            List<decimal> advancesSumList = new();
            List<decimal> declinesSumList = new();
            List<decimal> ranaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal advance = currentValue > prevValue ? 1 : 0;
                advancesList.Add(advance);

                decimal decline = currentValue < prevValue ? 1 : 0;
                declinesList.Add(decline);

                decimal advanceSum = advancesList.TakeLastExt(fastLength).Sum();
                advancesSumList.Add(advanceSum);

                decimal declineSum = declinesList.TakeLastExt(fastLength).Sum();
                declinesSumList.Add(declineSum);

                decimal rana = advanceSum + declineSum != 0 ? mult * (advanceSum - declineSum) / (advanceSum + declineSum) : 0;
                ranaList.Add(rana);
            }

            stockData.CustomValuesList = ranaList;
            var moList = CalculateMovingAverageConvergenceDivergence(stockData, maType, fastLength, slowLength, signalLength);
            var mcclellanOscillatorList = moList.OutputValues["Macd"];
            var mcclellanSignalLineList = moList.OutputValues["Signal"];
            var mcclellanHistogramList = moList.OutputValues["Histogram"];
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal mcclellanHistogram = mcclellanHistogramList.ElementAtOrDefault(i);
                decimal prevMcclellanHistogram = i >= 1 ? mcclellanHistogramList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(mcclellanHistogram, prevMcclellanHistogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "AdvSum", advancesSumList },
                { "DecSum", declinesSumList },
                { "Mo", mcclellanOscillatorList },
                { "Signal", mcclellanSignalLineList },
                { "Histogram", mcclellanHistogramList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = mcclellanOscillatorList;
            stockData.IndicatorName = IndicatorName.McClellanOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Woodie Commodity Channel Index
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="fastLength"></param>
        /// <param name="slowLength"></param>
        /// <returns></returns>
        public static StockData CalculateWoodieCommodityChannelIndex(this StockData stockData, MovingAvgType maType, int fastLength = 6, int slowLength = 14)
        {
            List<decimal> histogramList = new();
            List<Signal> signalsList = new();

            var cciList = CalculateCommodityChannelIndex(stockData, slowLength, maType).CustomValuesList;
            var turboCciList = CalculateCommodityChannelIndex(stockData, fastLength, maType).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal cci = cciList.ElementAtOrDefault(i);
                decimal cciTurbo = turboCciList.ElementAtOrDefault(i);

                decimal prevCciHistogram = histogramList.LastOrDefault();
                decimal cciHistogram = cciTurbo - cci;
                histogramList.AddRounded(cciHistogram);

                var signal = GetCompareSignal(cciHistogram, prevCciHistogram);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "FastCci", turboCciList },
                { "SlowCci", cciList },
                { "Histogram", histogramList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.WoodieCommodityChannelIndex;

            return stockData;
        }

        /// <summary>
        /// Calculates the Wave Trend Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="inputName"></param>
        /// <param name="maType"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <param name="smoothLength"></param>
        /// <returns></returns>
        public static StockData CalculateWaveTrendOscillator(this StockData stockData, InputName inputName = InputName.FullTypicalPrice, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 10, int length2 = 21, int smoothLength = 4)
        {
            List<decimal> absApEsaList = new();
            List<decimal> ciList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _, _) = GetInputValuesList(inputName, stockData);

            var emaList = GetMovingAverageList(stockData, maType, length1, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ap = inputList.ElementAtOrDefault(i);
                decimal esa = emaList.ElementAtOrDefault(i);

                decimal absApEsa = Math.Abs(ap - esa);
                absApEsaList.Add(absApEsa);
            }

            var dList = GetMovingAverageList(stockData, maType, length1, absApEsaList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal ap = inputList.ElementAtOrDefault(i);
                decimal esa = emaList.ElementAtOrDefault(i);
                decimal d = dList.ElementAtOrDefault(i);

                decimal ci = d != 0 ? (ap - esa) / (0.015m * d) : 0;
                ciList.Add(ci);
            }

            var tciList = GetMovingAverageList(stockData, maType, length2, ciList);
            var wt2List = GetMovingAverageList(stockData, maType, smoothLength, tciList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal tci = tciList.ElementAtOrDefault(i);
                decimal wt2 = wt2List.ElementAtOrDefault(i);
                decimal prevTci = i >= 1 ? tciList.ElementAtOrDefault(i - 1) : 0;
                decimal prevWt2 = i >= 1 ? wt2List.ElementAtOrDefault(i - 1) : 0;

                var signal = GetRsiSignal(tci - wt2, prevTci - prevWt2, tci, prevTci, 53, -53);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wto", tciList },
                { "Signal", wt2List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tciList;
            stockData.IndicatorName = IndicatorName.WaveTrendOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Williams Fractals
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateWilliamsFractals(this StockData stockData, int length = 2)
        {
            List<decimal> upFractalList = new();
            List<decimal> dnFractalList = new();
            List<Signal> signalsList = new();
            var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevHigh = i >= length - 2 ? highList.ElementAtOrDefault(i - (length - 2)) : 0;
                decimal prevHigh1 = i >= length - 1 ? highList.ElementAtOrDefault(i - (length - 1)) : 0;
                decimal prevHigh2 = i >= length ? highList.ElementAtOrDefault(i - length) : 0;
                decimal prevHigh3 = i >= length + 1 ? highList.ElementAtOrDefault(i - (length + 1)) : 0;
                decimal prevHigh4 = i >= length + 2 ? highList.ElementAtOrDefault(i - (length + 2)) : 0;
                decimal prevHigh5 = i >= length + 3 ? highList.ElementAtOrDefault(i - (length + 3)) : 0;
                decimal prevHigh6 = i >= length + 4 ? highList.ElementAtOrDefault(i - (length + 4)) : 0;
                decimal prevHigh7 = i >= length + 5 ? highList.ElementAtOrDefault(i - (length + 5)) : 0;
                decimal prevHigh8 = i >= length + 8 ? highList.ElementAtOrDefault(i - (length + 6)) : 0;
                decimal prevLow = i >= length - 2 ? lowList.ElementAtOrDefault(i - (length - 2)) : 0;
                decimal prevLow1 = i >= length - 1 ? lowList.ElementAtOrDefault(i - (length - 1)) : 0;
                decimal prevLow2 = i >= length ? lowList.ElementAtOrDefault(i - length) : 0;
                decimal prevLow3 = i >= length + 1 ? lowList.ElementAtOrDefault(i - (length + 1)) : 0;
                decimal prevLow4 = i >= length + 2 ? lowList.ElementAtOrDefault(i - (length + 2)) : 0;
                decimal prevLow5 = i >= length + 3 ? lowList.ElementAtOrDefault(i - (length + 3)) : 0;
                decimal prevLow6 = i >= length + 4 ? lowList.ElementAtOrDefault(i - (length + 4)) : 0;
                decimal prevLow7 = i >= length + 5 ? lowList.ElementAtOrDefault(i - (length + 5)) : 0;
                decimal prevLow8 = i >= length + 8 ? lowList.ElementAtOrDefault(i - (length + 6)) : 0;

                decimal prevUpFractal = upFractalList.LastOrDefault();
                decimal upFractal = (prevHigh4 < prevHigh2 && prevHigh3 < prevHigh2 && prevHigh1 < prevHigh2 && prevHigh < prevHigh2) ||
                    (prevHigh5 < prevHigh2 && prevHigh4 < prevHigh2 && prevHigh3 == prevHigh2 && prevHigh1 < prevHigh2 && prevHigh1 < prevHigh2) ||
                    (prevHigh6 < prevHigh2 && prevHigh5 < prevHigh2 && prevHigh4 == prevHigh2 && prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 && 
                    prevHigh < prevHigh2) || (prevHigh7 < prevHigh2 && prevHigh6 < prevHigh2 && prevHigh5 == prevHigh2 && prevHigh4 == prevHigh2 && 
                    prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 && prevHigh < prevHigh2) || (prevHigh8 < prevHigh2 && prevHigh7 < prevHigh2 && 
                    prevHigh6 == prevHigh2 && prevHigh5 <= prevHigh2 && prevHigh4 == prevHigh2 && prevHigh3 <= prevHigh2 && prevHigh1 < prevHigh2 && 
                    prevHigh < prevHigh2) ? 1 : 0;
                upFractalList.Add(upFractal);

                decimal prevDnFractal = dnFractalList.LastOrDefault();
                decimal dnFractal = (prevLow4 > prevLow2 && prevLow3 > prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow5 > prevLow2 && 
                    prevLow4 > prevLow2 && prevLow3 == prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow6 > prevLow2 && 
                    prevLow5 > prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) || 
                    (prevLow7 > prevLow2 && prevLow6 > prevLow2 && prevLow5 == prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && 
                    prevLow1 > prevLow2 && prevLow > prevLow2) || (prevLow8 > prevLow2 && prevLow7 > prevLow2 && prevLow6 == prevLow2 && 
                    prevLow5 >= prevLow2 && prevLow4 == prevLow2 && prevLow3 >= prevLow2 && prevLow1 > prevLow2 && prevLow > prevLow2) ? 1 : 0;
                dnFractalList.Add(dnFractal);

                var signal = GetCompareSignal(upFractal - dnFractal, prevUpFractal - prevDnFractal);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "UpFractal", upFractalList },
                { "DnFractal", dnFractalList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.WilliamsFractals;

            return stockData;
        }

        /// <summary>
        /// Calculates the Williams Accumulation Distribution
        /// </summary>
        /// <param name="stockData"></param>
        /// <returns></returns>
        public static StockData CalculateWilliamsAccumulationDistribution(this StockData stockData)
        {
            List<decimal> wadList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal close = inputList.ElementAtOrDefault(i);
                decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
                decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;

                decimal prevWad = wadList.LastOrDefault();
                decimal wad = close > prevClose ? prevWad + close - prevLow : close < prevClose ? prevWad + close - prevHigh : 0;
                wadList.Add(wad);

                var signal = GetCompareSignal(wad, prevWad);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wad", wadList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = wadList;
            stockData.IndicatorName = IndicatorName.WilliamsAccumulationDistribution;

            return stockData;
        }

        /// <summary>
        /// Calculates the Wami Oscillator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <returns></returns>
        public static StockData CalculateWamiOscillator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
            int length1 = 13, int length2 = 4)
        {
            List<decimal> diffList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal diff = currentValue - prevValue;
                diffList.Add(diff);
            }

            var wma1List = GetMovingAverageList(stockData, MovingAvgType.WeightedMovingAverage, length2, diffList);
            var ema2List = GetMovingAverageList(stockData, maType, length1, wma1List);
            var wamiList = GetMovingAverageList(stockData, maType, length1, ema2List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal wami = wamiList.ElementAtOrDefault(i);
                decimal prevWami = i >= 1 ? wamiList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(wami, prevWami);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wami", wamiList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = wamiList;
            stockData.IndicatorName = IndicatorName.WamiOscillator;

            return stockData;
        }

        /// <summary>
        /// Calculates the Waddah Attar Explosion
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="fastLength"></param>
        /// <param name="slowLength"></param>
        /// <param name="sensitivity"></param>
        /// <returns></returns>
        public static StockData CalculateWaddahAttarExplosion(this StockData stockData, int fastLength = 20, int slowLength = 40, decimal sensitivity = 150)
        {
            List<decimal> t1List = new();
            List<decimal> t2List = new();
            List<decimal> e1List = new();
            List<decimal> temp1List = new();
            List<decimal> temp2List = new();
            List<decimal> temp3List = new();
            List<decimal> trendUpList = new();
            List<decimal> trendDnList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var macd1List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
            var bbList = CalculateBollingerBands(stockData, length: fastLength);
            var upperBollingerBandList = bbList.OutputValues["UpperBand"];
            var lowerBollingerBandList = bbList.OutputValues["LowerBand"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                temp1List.Add(prevValue1);

                decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
                temp2List.Add(prevValue2);

                decimal prevValue3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
                temp3List.Add(prevValue3);
            }

            stockData.CustomValuesList = temp1List;
            var macd2List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
            stockData.CustomValuesList = temp2List;
            var macd3List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
            stockData.CustomValuesList = temp3List;
            var macd4List = CalculateMovingAverageConvergenceDivergence(stockData, fastLength: fastLength, slowLength: slowLength).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentMacd1 = macd1List.ElementAtOrDefault(i);
                decimal currentMacd2 = macd2List.ElementAtOrDefault(i);
                decimal currentMacd3 = macd3List.ElementAtOrDefault(i);
                decimal currentMacd4 = macd4List.ElementAtOrDefault(i);
                decimal currentUpperBB = upperBollingerBandList.ElementAtOrDefault(i);
                decimal currentLowerBB = lowerBollingerBandList.ElementAtOrDefault(i);

                decimal t1 = (currentMacd1 - currentMacd2) * sensitivity;
                t1List.AddRounded(t1);

                decimal t2 = (currentMacd3 - currentMacd4) * sensitivity;
                t2List.AddRounded(t2);

                decimal prevE1 = e1List.LastOrDefault();
                decimal e1 = currentUpperBB - currentLowerBB;
                e1List.Add(e1);

                decimal prevTrendUp = trendUpList.LastOrDefault();
                decimal trendUp = (t1 >= 0) ? t1 : 0;
                trendUpList.Add(trendUp);

                decimal trendDown = (t1 < 0) ? (-1 * t1) : 0;
                trendDnList.Add(trendDown);

                var signal = GetConditionSignal(trendUp > prevTrendUp && trendUp > e1 && e1 > prevE1 && trendUp > fastLength && e1 > fastLength, 
                    trendUp < e1);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "T1", t1List },
                { "T2", t2List },
                { "E1", e1List },
                { "TrendUp", trendUpList },
                { "TrendDn", trendDnList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.WaddahAttarExplosion;

            return stockData;
        }

        /// <summary>
        /// Calculates the Qma Sma Difference
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateQmaSmaDifference(this StockData stockData, int length = 14)
        {
            List<decimal> cList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var qmaList = CalculateQuadraticMovingAverage(stockData, length).CustomValuesList;
            var smaList = CalculateSimpleMovingAverage(stockData, length).CustomValuesList;
            var emaList = CalculateExponentialMovingAverage(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentEma = emaList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal qma = qmaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevEma = i >= 1 ? emaList.ElementAtOrDefault(i - 1) : 0;

                decimal prevC = cList.LastOrDefault();
                decimal c = qma - sma;
                cList.Add(c);

                var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, c, prevC);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "QmaSmaDiff", cList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = cList;
            stockData.IndicatorName = IndicatorName.QmaSmaDifference;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quantitative Qualitative Estimation
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="smoothLength"></param>
        /// <param name="fastFactor"></param>
        /// <param name="slowFactor"></param>
        /// <returns></returns>
        public static StockData CalculateQuantitativeQualitativeEstimation(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14, int smoothLength = 5, decimal fastFactor = 2.618m, 
            decimal slowFactor = 4.236m)
        {
            List<decimal> atrRsiList = new();
            List<decimal> fastAtrRsiList = new();
            List<decimal> slowAtrRsiList = new();
            List<Signal> signalsList = new();

            int wildersLength = (length * 2) - 1;

            var rsiValueList = CalculateRelativeStrengthIndex(stockData, maType, length, smoothLength);
            var rsiEmaList = rsiValueList.OutputValues["Signal"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentRsiEma = rsiEmaList.ElementAtOrDefault(i);
                decimal prevRsiEma = i >= 1 ? rsiEmaList.ElementAtOrDefault(i - 1) : 0;

                decimal atrRsi = Math.Abs(currentRsiEma - prevRsiEma);
                atrRsiList.Add(atrRsi);
            }

            var atrRsiEmaList = GetMovingAverageList(stockData, maType, wildersLength, atrRsiList);
            var atrRsiEmaSmoothList = GetMovingAverageList(stockData, maType, wildersLength, atrRsiEmaList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal atrRsiEmaSmooth = atrRsiEmaSmoothList.ElementAtOrDefault(i);
                decimal prevAtrRsiEmaSmooth = i >= 1 ? atrRsiEmaSmoothList.ElementAtOrDefault(i - 1) : 0;

                decimal prevFastTl = fastAtrRsiList.LastOrDefault();
                decimal fastTl = atrRsiEmaSmooth * fastFactor;
                fastAtrRsiList.Add(fastTl);

                decimal prevSlowTl = slowAtrRsiList.LastOrDefault();
                decimal slowTl = atrRsiEmaSmooth * slowFactor;
                slowAtrRsiList.Add(slowTl);

                var signal = GetBullishBearishSignal(atrRsiEmaSmooth - Math.Max(fastTl, slowTl), prevAtrRsiEmaSmooth - Math.Max(prevFastTl, prevSlowTl),
                    atrRsiEmaSmooth - Math.Min(fastTl, slowTl), prevAtrRsiEmaSmooth - Math.Min(prevFastTl, prevSlowTl));
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "FastAtrRsi", fastAtrRsiList },
                { "SlowAtrRsi", slowAtrRsiList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = new List<decimal>();
            stockData.IndicatorName = IndicatorName.QuantitativeQualitativeEstimation;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quasi White Noise
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="noiseLength"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static StockData CalculateQuasiWhiteNoise(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
            int length = 20, int noiseLength = 500, decimal divisor = 40)
        {
            List<decimal> whiteNoiseList = new();
            List<decimal> whiteNoiseVarianceList = new();
            List<Signal> signalsList = new();

            var connorsRsiList = CalculateConnorsRelativeStrengthIndex(stockData, maType, noiseLength, noiseLength, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal connorsRsi = connorsRsiList.ElementAtOrDefault(i);
                decimal prevConnorsRsi1 = i >= 1 ? connorsRsiList.ElementAtOrDefault(i - 1) : 0;
                decimal prevConnorsRsi2 = i >= 2 ? connorsRsiList.ElementAtOrDefault(i - 2) : 0;

                decimal whiteNoise = (connorsRsi - 50) * (1 / divisor);
                whiteNoiseList.Add(whiteNoise);

                var signal = GetRsiSignal(connorsRsi - prevConnorsRsi1, prevConnorsRsi1 - prevConnorsRsi2, connorsRsi, prevConnorsRsi1, 70, 30);
                signalsList.Add(signal);
            }

            var whiteNoiseSmaList = GetMovingAverageList(stockData, maType, noiseLength, whiteNoiseList);
            stockData.CustomValuesList = whiteNoiseList;
            var whiteNoiseStdDevList = CalculateStandardDeviationVolatility(stockData, noiseLength).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal whiteNoiseStdDev = whiteNoiseStdDevList.ElementAtOrDefault(i);

                decimal whiteNoiseVariance = Pow(whiteNoiseStdDev, 2);
                whiteNoiseVarianceList.Add(whiteNoiseVariance);
            }

            stockData.OutputValues = new()
            {
                { "WhiteNoise", whiteNoiseList },
                { "WhiteNoiseMa", whiteNoiseSmaList },
                { "WhiteNoiseStdDev", whiteNoiseStdDevList },
                { "WhiteNoiseVariance", whiteNoiseVarianceList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = whiteNoiseList;
            stockData.IndicatorName = IndicatorName.QuasiWhiteNoise;

            return stockData;
        }
    }
}
