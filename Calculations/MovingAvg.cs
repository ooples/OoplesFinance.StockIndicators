using MathNet.Numerics;
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
        /// Calculates the simple moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateSimpleMovingAverage(this StockData stockData)
        {
            int length = 14;

            return CalculateSimpleMovingAverage(stockData, length);
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
                decimal sma = tempList.TakeLastExt(length).Average();
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

        /// <summary>
        /// Calculates the weighted moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateWeightedMovingAverage(this StockData stockData)
        {
            int length = 14;

            return CalculateWeightedMovingAverage(stockData, length);
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

        /// <summary>
        /// Calculates the exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateExponentialMovingAverage(this StockData stockData)
        {
            int length = 14;

            return CalculateExponentialMovingAverage(stockData, length);
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

        /// <summary>
        /// Calculates the triangular moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateTriangularMovingAverage(this StockData stockData)
        {
            var maType = MovingAvgType.SimpleMovingAverage;
            var length = 20;

            return CalculateTriangularMovingAverage(stockData, maType, length);
        }

        /// <summary>
        /// Calculates the triangular moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateTriangularMovingAverage(this StockData stockData, MovingAvgType maType, int length)
        {
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);
            var sma1List = GetMovingAverageList(stockData, maType, length, inputList);
            var tmaList = GetMovingAverageList(stockData, maType, length, sma1List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal tma = tmaList.ElementAtOrDefault(i);
                decimal prevTma = i >= 1 ? tmaList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(currentValue - tma, prevValue - prevTma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Tma", tmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = tmaList;
            stockData.IndicatorName = IndicatorName.TriangularMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the hull moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateHullMovingAverage(this StockData stockData)
        {
            var maType = MovingAvgType.WeightedMovingAverage;
            var length = 20;

            return CalculateHullMovingAverage(stockData, maType, length);
        }

        /// <summary>
        /// Calculates the hull moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateHullMovingAverage(this StockData stockData, MovingAvgType maType, int length)
        {
            List<decimal> totalWeightedMAList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int length2 = MinOrMax((int)Math.Ceiling((decimal)length / 2)); ;
            int sqrtLength = MinOrMax((int)Math.Ceiling(Sqrt(length)));

            var wma1List = GetMovingAverageList(stockData, maType, length, inputList);
            var wma2List = GetMovingAverageList(stockData, maType, length2, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentWMA1 = wma1List.ElementAtOrDefault(i);
                decimal currentWMA2 = wma2List.ElementAtOrDefault(i);

                decimal totalWeightedMA = (2 * currentWMA2) - currentWMA1;
                totalWeightedMAList.Add(totalWeightedMA);
            }

            var hullMAList = GetMovingAverageList(stockData, maType, sqrtLength, totalWeightedMAList);
            for (int j = 0; j < stockData.Count; j++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(j);
                decimal prevValue = j >= 1 ? inputList.ElementAtOrDefault(j - 1) : 0;
                decimal hullMa = hullMAList.ElementAtOrDefault(j);
                decimal prevHullMa = j >= 1 ? inputList.ElementAtOrDefault(j - 1) : 0;

                var signal = GetCompareSignal(currentValue - hullMa, prevValue - prevHullMa);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Hma", hullMAList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = hullMAList;
            stockData.IndicatorName = IndicatorName.HullMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the kaufman adaptive moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <returns></returns>
        public static StockData CalculateKaufmanAdaptiveMovingAverage(this StockData stockData, int length = 10, int fastLength = 2, int slowLength = 30)
        {
            List<decimal> volatilityList = new();
            List<decimal> erList = new();
            List<decimal> kamaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal fastAlpha = (decimal)2 / (fastLength + 1);
            decimal slowAlpha = (decimal)2 / (slowLength + 1);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;

                decimal volatility = Math.Abs(currentValue - prevValue);
                volatilityList.Add(volatility);

                decimal volatilitySum = volatilityList.TakeLastExt(length).Sum();
                decimal momentum = Math.Abs(currentValue - priorValue);

                decimal efficiencyRatio = volatilitySum != 0 ? momentum / volatilitySum : 0;
                erList.AddRounded(efficiencyRatio);

                decimal sc = Pow((efficiencyRatio * (fastAlpha - slowAlpha)) + slowAlpha, 2);
                decimal prevKama = kamaList.LastOrDefault();
                decimal currentKAMA = (sc * currentValue) + ((1 - sc) * prevKama);
                kamaList.Add(currentKAMA);

                var signal = GetCompareSignal(currentValue - currentKAMA, prevValue - prevKama);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Er", erList },
                { "Kama", kamaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = kamaList;
            stockData.IndicatorName = IndicatorName.KaufmanAdaptiveMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the arnaud legoux moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="sigma">The sigma.</param>
        /// <returns></returns>
        public static StockData CalculateArnaudLegouxMovingAverage(this StockData stockData, int length = 9, decimal offset = 0.85m, int sigma = 6)
        {
            List<decimal> almaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal m = offset * (length - 1);
            decimal s = (decimal)length / sigma;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j <= length - 1; j++)
                {
                    decimal weight = s != 0 ? Exp(-1 * Pow(j - m, 2) / (2 * Pow(s, 2))) : 0;
                    decimal prevValue = i >= length - 1 - j ? inputList.ElementAtOrDefault(length - 1 - j) : 0;

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal prevAlma = almaList.LastOrDefault();
                decimal alma = weightedSum != 0 ? sum / weightedSum : 0;
                almaList.Add(alma);

                var signal = GetCompareSignal(currentValue - alma, prevVal - prevAlma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Alma", almaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = almaList;
            stockData.IndicatorName = IndicatorName.ArnaudLegouxMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the end point moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static StockData CalculateEndPointMovingAverage(this StockData stockData, int length = 11, int offset = 4)
        {
            List<decimal> sumList = new();
            List<decimal> weightedSumList = new();
            List<decimal> epmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j <= length - 1; j++)
                {
                    decimal weight = length - j - length;
                    decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal prevEpma = epmaList.LastOrDefault();
                decimal epma = weightedSum != 0 ? 1 / weightedSum * sum : 0;
                epmaList.Add(epma);

                var signal = GetCompareSignal(currentValue - epma, prevVal - prevEpma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Epma", epmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = epmaList;
            stockData.IndicatorName = IndicatorName.EndPointMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the least squares moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateLeastSquaresMovingAverage(this StockData stockData, int length = 25)
        {
            List<decimal> lsmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var wmaList = CalculateWeightedMovingAverage(stockData, length).CustomValuesList;
            var smaList = CalculateSimpleMovingAverage(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentWma = wmaList.ElementAtOrDefault(i);
                decimal currentSma = smaList.ElementAtOrDefault(i);

                decimal prevLsma = lsmaList.LastOrDefault();
                decimal lsma = (3 * currentWma) - (2 * currentSma);
                lsmaList.Add(lsma);

                var signal = GetCompareSignal(currentValue - lsma, prevValue - prevLsma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Lsma", lsmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = lsmaList;
            stockData.IndicatorName = IndicatorName.LeastSquaresMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the ehlers mother of adaptive moving averages.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="fastAlpha">The fast alpha.</param>
        /// <param name="slowAlpha">The slow alpha.</param>
        /// <returns></returns>
        public static StockData CalculateEhlersMotherOfAdaptiveMovingAverages(this StockData stockData, decimal fastAlpha = 0.5m, decimal slowAlpha = 0.05m)
        {
            List<decimal> famaList = new();
            List<decimal> mamaList = new();
            List<decimal> i2List = new();
            List<decimal> q2List = new();
            List<decimal> reList = new();
            List<decimal> imList = new();
            List<decimal> sPrdList = new();
            List<decimal> phaseList = new();
            List<decimal> periodList = new();
            List<decimal> smoothList = new();
            List<decimal> detList = new();
            List<decimal> q1List = new();
            List<decimal> i1List = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevPrice1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal previ2 = i >= 1 ? i2List.ElementAtOrDefault(i - 1) : 0;
                decimal prevq2 = i >= 1 ? q2List.ElementAtOrDefault(i - 1) : 0;
                decimal prevRe = i >= 1 ? reList.ElementAtOrDefault(i - 1) : 0;
                decimal prevIm = i >= 1 ? imList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSprd = i >= 1 ? sPrdList.ElementAtOrDefault(i - 1) : 0;
                decimal prevPhase = i >= 1 ? phaseList.ElementAtOrDefault(i - 1) : 0;
                decimal prevPeriod = i >= 1 ? periodList.ElementAtOrDefault(i - 1) : 0;
                decimal prevPrice2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
                decimal prevPrice3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
                decimal prevs2 = i >= 2 ? smoothList.ElementAtOrDefault(i - 2) : 0;
                decimal prevd2 = i >= 2 ? detList.ElementAtOrDefault(i - 2) : 0;
                decimal prevq1x2 = i >= 2 ? q1List.ElementAtOrDefault(i - 2) : 0;
                decimal previ1x2 = i >= 2 ? i1List.ElementAtOrDefault(i - 2) : 0;
                decimal prevd3 = i >= 3 ? detList.ElementAtOrDefault(i - 3) : 0;
                decimal prevs4 = i >= 4 ? smoothList.ElementAtOrDefault(i - 4) : 0;
                decimal prevd4 = i >= 4 ? detList.ElementAtOrDefault(i - 4) : 0;
                decimal prevq1x4 = i >= 4 ? q1List.ElementAtOrDefault(i - 4) : 0;
                decimal previ1x4 = i >= 4 ? i1List.ElementAtOrDefault(i - 4) : 0;
                decimal prevs6 = i >= 6 ? smoothList.ElementAtOrDefault(i - 6) : 0;
                decimal prevd6 = i >= 6 ? detList.ElementAtOrDefault(i - 6) : 0;
                decimal prevq1x6 = i >= 6 ? q1List.ElementAtOrDefault(i - 6) : 0;
                decimal previ1x6 = i >= 6 ? i1List.ElementAtOrDefault(i - 6) : 0;
                decimal prevMama = i >= 1 ? mamaList.ElementAtOrDefault(i - 1) : 0;
                decimal prevFama = i >= 1 ? famaList.ElementAtOrDefault(i - 1) : 0;

                decimal smooth = ((4 * currentValue) + (3 * prevPrice1) + (2 * prevPrice2) + prevPrice3) / 10;
                smoothList.Add(smooth);

                decimal det = ((0.0962m * smooth) + (0.5769m * prevs2) - (0.5769m * prevs4) - (0.0962m * prevs6)) * ((0.075m * prevPeriod) + 0.54m);
                detList.Add(det);

                decimal q1 = ((0.0962m * det) + (0.5769m * prevd2) - (0.5769m * prevd4) - (0.0962m * prevd6)) * ((0.075m * prevPeriod) + 0.54m);
                q1List.Add(q1);

                decimal i1 = prevd3;
                i1List.Add(i1);

                decimal j1 = ((0.0962m * i1) + (0.5769m * previ1x2) - (0.5769m * previ1x4) - (0.0962m * previ1x6)) * ((0.075m * prevPeriod) + 0.54m);
                decimal jq = ((0.0962m * q1) + (0.5769m * prevq1x2) - (0.5769m * prevq1x4) - (0.0962m * prevq1x6)) * ((0.075m * prevPeriod) + 0.54m);

                decimal i2 = i1 - jq;
                i2 = (0.2m * i2) + (0.8m * previ2);
                i2List.Add(i2);

                decimal q2 = q1 + j1;
                q2 = (0.2m * q2) + (0.8m * prevq2);
                q2List.Add(q2);

                decimal re = (i2 * previ2) + (q2 * prevq2);
                re = (0.2m * re) + (0.8m * prevRe);
                reList.Add(re);

                decimal im = (i2 * prevq2) - (q2 * previ2);
                im = (0.2m * im) + (0.8m * prevIm);
                imList.Add(im);

                var atan = re != 0 ? Atan(im / re) : 0;
                decimal period = atan != 0 ? 2 * (decimal)Math.PI / atan : 0;
                period = MinOrMax(period, 1.5m * prevPeriod, 0.67m * prevPeriod);
                period = MinOrMax(period, 50, 6);
                period = (0.2m * period) + (0.8m * prevPeriod);
                periodList.Add(period);

                decimal sPrd = (0.33m * period) + (0.67m * prevSprd);
                sPrdList.Add(sPrd);

                decimal phase = i1 != 0 ? 180 / (decimal)Math.PI * Atan(q1 / i1) : 0;
                phaseList.Add(phase);

                decimal deltaPhase = prevPhase - phase < 1 ? 1 : prevPhase - phase;
                decimal alpha = deltaPhase != 0 ? fastAlpha / deltaPhase : 0;
                alpha = alpha < slowAlpha ? slowAlpha : alpha;

                decimal mama = (alpha * currentValue) + ((1 - alpha) * prevMama);
                mamaList.Add(mama);

                decimal fama = (0.5m * alpha * mama) + ((1 - (0.5m * alpha)) * prevFama);
                famaList.Add(fama);

                var signal = GetCompareSignal(mama - fama, prevMama - prevFama);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Fama", famaList },
                { "Mama", mamaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = mamaList;
            stockData.IndicatorName = IndicatorName.EhlersMotherOfAdaptiveMovingAverages;

            return stockData;
        }

        /// <summary>
        /// Calculates the welles wilder moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateWellesWilderMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> wwmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal k = (decimal)1 / length;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevWwma = wwmaList.LastOrDefault();
                decimal wwma = (currentValue * k) + (prevWwma * (1 - k));
                wwmaList.Add(wwma);

                var signal = GetCompareSignal(currentValue - wwma, prevValue - prevWwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wwma", wwmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = wwmaList;
            stockData.IndicatorName = IndicatorName.WellesWilderMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the t3 moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="vFactor">The v factor.</param>
        /// <returns></returns>
        public static StockData CalculateT3MovingAverage(this StockData stockData, MovingAvgType maType, int length = 5, decimal vFactor = 0.7m)
        {
            List<decimal> t3List = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal c1 = -vFactor * vFactor * vFactor;
            decimal c2 = (3 * vFactor * vFactor) + (3 * vFactor * vFactor * vFactor);
            decimal c3 = (-6 * vFactor * vFactor) - (3 * vFactor) - (3 * vFactor * vFactor * vFactor);
            decimal c4 = 1 + (3 * vFactor) + (vFactor * vFactor * vFactor) + (3 * vFactor * vFactor);

            var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
            var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);
            var ema4List = GetMovingAverageList(stockData, maType, length, ema3List);
            var ema5List = GetMovingAverageList(stockData, maType, length, ema4List);
            var ema6List = GetMovingAverageList(stockData, maType, length, ema5List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal ema6 = ema6List.ElementAtOrDefault(i);
                decimal ema5 = ema5List.ElementAtOrDefault(i);
                decimal ema4 = ema4List.ElementAtOrDefault(i);
                decimal ema3 = ema3List.ElementAtOrDefault(i);

                decimal prevT3 = t3List.LastOrDefault();
                decimal t3 = (c1 * ema6) + (c2 * ema5) + (c3 * ema4) + (c4 * ema3);
                t3List.AddRounded(t3);

                var signal = GetCompareSignal(currentValue - t3, prevValue - prevT3);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "T3", t3List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = t3List;
            stockData.IndicatorName = IndicatorName.T3MovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the triple exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateTripleExponentialMovingAverage(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> temaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
            var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentEma1 = ema1List.ElementAtOrDefault(i);
                decimal currentEma2 = ema2List.ElementAtOrDefault(i);
                decimal currentEma3 = ema3List.ElementAtOrDefault(i);

                decimal prevTema = temaList.LastOrDefault();
                decimal tema = (3 * currentEma1) - (3 * currentEma2) + currentEma3;
                temaList.Add(tema);

                var signal = GetCompareSignal(currentValue - tema, prevValue - prevTema);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Tema", temaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = temaList;
            stockData.IndicatorName = IndicatorName.TripleExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the volume weighted average price.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="inputName">Name of the input.</param>
        /// <returns></returns>
        public static StockData CalculateVolumeWeightedAveragePrice(this StockData stockData, InputName inputName = InputName.TypicalPrice)
        {
            List<decimal> vwapList = new();
            List<decimal> tempVolList = new();
            List<decimal> tempVolPriceList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _, volumeList) = GetInputValuesList(inputName, stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                tempVolList.Add(currentVolume);

                decimal volumePrice = currentValue * currentVolume;
                tempVolPriceList.Add(volumePrice);

                decimal volPriceSum = tempVolPriceList.Sum();
                decimal volSum = tempVolList.Sum();

                decimal prevVwap = vwapList.LastOrDefault();
                decimal vwap = volSum != 0 ? volPriceSum / volSum : 0;
                vwapList.Add(vwap);

                var signal = GetCompareSignal(currentValue - vwap, prevValue - prevVwap);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Vwap", vwapList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = vwapList;
            stockData.IndicatorName = IndicatorName.VolumeWeightedAveragePrice;

            return stockData;
        }

        /// <summary>
        /// Calculates the volume weighted moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateVolumeWeightedMovingAverage(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> volumePriceList = new();
            List<decimal> vwmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            var volumeSmaList = GetMovingAverageList(stockData, maType, length, volumeList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal currentVolumeSma = volumeSmaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal volumePrice = currentValue * currentVolume;
                volumePriceList.Add(volumePrice);

                decimal volumePriceSma = volumePriceList.TakeLastExt(length).Average();

                decimal prevVwma = vwmaList.LastOrDefault();
                decimal vwma = currentVolumeSma != 0 ? volumePriceSma / currentVolumeSma : 0;
                vwmaList.Add(vwma);

                var signal = GetCompareSignal(currentValue - vwma, prevValue - prevVwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Vwma", vwmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = vwmaList;
            stockData.IndicatorName = IndicatorName.VolumeWeightedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the ultimate moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="minLength">The minimum length.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <param name="acc">The acc.</param>
        /// <returns></returns>
        public static StockData CalculateUltimateMovingAverage(this StockData stockData, MovingAvgType maType, int minLength = 5, int maxLength = 50, 
            decimal acc = 1)
        {
            List<decimal> umaList = new();
            List<decimal> posMoneyFlowList = new();
            List<decimal> negMoneyFlowList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var lenList = CalculateVariableLengthMovingAverage(stockData, maType, minLength, maxLength).OutputValues["Length"];
            var tpList = CalculateTypicalPrice(stockData).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentVolume = stockData.Volumes.ElementAtOrDefault(i);
                decimal typicalPrice = tpList.ElementAtOrDefault(i);
                decimal prevTypicalPrice = i >= 1 ? tpList.ElementAtOrDefault(i - 1) : 0;
                decimal length = MinOrMax(lenList.ElementAtOrDefault(i), maxLength, minLength);
                decimal rawMoneyFlow = typicalPrice * currentVolume;

                decimal posMoneyFlow = typicalPrice > prevTypicalPrice ? rawMoneyFlow : 0;
                posMoneyFlowList.Add(posMoneyFlow);

                decimal negMoneyFlow = typicalPrice < prevTypicalPrice ? rawMoneyFlow : 0;
                negMoneyFlowList.Add(negMoneyFlow);

                int len = (int)length;
                decimal posMoneyFlowTotal = posMoneyFlowList.TakeLastExt(len).Sum();
                decimal negMoneyFlowTotal = negMoneyFlowList.TakeLastExt(len).Sum();
                decimal mfiRatio = negMoneyFlowTotal != 0 ? MinOrMax(posMoneyFlowTotal / negMoneyFlowTotal, 1, 0) : 0;
                decimal mfi = negMoneyFlowTotal == 0 ? 100 : posMoneyFlowTotal == 0 ? 0 : MinOrMax(100 - (100 / (1 + mfiRatio)), 100, 0);
                decimal mfScaled = (mfi * 2) - 100;
                decimal p = acc + (Math.Abs(mfScaled) / 25);
                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j <= len - 1; j++)
                {
                    decimal weight = Pow(len - j, (double)p);
                    decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal prevUma = umaList.LastOrDefault();
                decimal uma = weightedSum != 0 ? sum / weightedSum : 0;
                umaList.Add(uma);

                var signal = GetCompareSignal(currentValue - uma, prevVal - prevUma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Uma", umaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = umaList;
            stockData.IndicatorName = IndicatorName.UltimateMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the variable length moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="minLength">The minimum length.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        public static StockData CalculateVariableLengthMovingAverage(this StockData stockData, MovingAvgType maType, int minLength = 5, int maxLength = 50)
        {
            List<decimal> vlmaList = new();
            List<decimal> lengthList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, maxLength, inputList);
            var stdDevList = CalculateStandardDeviationVolatility(stockData, maxLength).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal stdDev = stdDevList.ElementAtOrDefault(i);
                decimal a = sma - (1.75m * stdDev);
                decimal b = sma - (0.25m * stdDev);
                decimal c = sma + (0.25m * stdDev);
                decimal d = sma + (1.75m * stdDev);

                decimal prevLength = i >= 1 ? lengthList.ElementAtOrDefault(i - 1) : maxLength;
                decimal length = MinOrMax(currentValue >= b && currentValue <= c ? prevLength + 1 : currentValue < a || 
                    currentValue > d ? prevLength - 1 : prevLength, maxLength, maxLength);
                lengthList.Add(length);

                decimal sc = 2 / (length + 1);
                decimal prevVlma = i >= 1 ? vlmaList.ElementAtOrDefault(i - 1) : currentValue;
                decimal vlma = (currentValue * sc) + ((1 - sc) * prevVlma);
                vlmaList.Add(vlma);

                var signal = GetCompareSignal(currentValue - vlma, prevValue - prevVlma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Length", lengthList },
                { "Vlma", vlmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = vlmaList;
            stockData.IndicatorName = IndicatorName.VariableLengthMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the ahrens moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAhrensMovingAverage(this StockData stockData, int length = 9)
        {
            List<decimal> ahmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priorAhma = i >= length ? ahmaList.ElementAtOrDefault(i - length) : currentValue;

                decimal prevAhma = ahmaList.LastOrDefault();
                decimal ahma = prevAhma + ((currentValue - ((prevAhma + priorAhma) / 2)) / length);
                ahmaList.Add(ahma);

                var signal = GetCompareSignal(currentValue - ahma, prevValue - prevAhma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ahma", ahmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ahmaList;
            stockData.IndicatorName = IndicatorName.AhrensMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="fastLength">Length of the fast.</param>
        /// <param name="slowLength">Length of the slow.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveMovingAverage(this StockData stockData, int fastLength = 2, int slowLength = 14, int length = 14)
        {
            List<decimal> amaList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length + 1);

            decimal fastAlpha = (decimal)2 / (fastLength + 1);
            decimal slowAlpha = (decimal)2 / (slowLength + 1);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal hh = highestList.ElementAtOrDefault(i);
                decimal ll = lowestList.ElementAtOrDefault(i);
                decimal mltp = hh - ll != 0 ? MinOrMax(Math.Abs((2 * currentValue) - ll - hh) / (hh - ll), 1, 0) : 0;
                decimal ssc = (mltp * (fastAlpha - slowAlpha)) + slowAlpha;

                decimal prevAma = amaList.LastOrDefault();
                decimal ama = prevAma + (Pow(ssc, 2) * (currentValue - prevAma));
                amaList.Add(ama);

                var signal = GetCompareSignal(currentValue - ama, prevValue - prevAma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ama", amaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = amaList;
            stockData.IndicatorName = IndicatorName.AdaptiveMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveExponentialMovingAverage(this StockData stockData, MovingAvgType maType, int length = 10)
        {
            List<decimal> aemaList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
            var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

            decimal mltp1 = (decimal)2 / (length + 1);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal hh = highestList.ElementAtOrDefault(i);
                decimal ll = lowestList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal mltp2 = hh - ll != 0 ? MinOrMax(Math.Abs((2 * currentValue) - ll - hh) / (hh - ll), 1, 0) : 0;
                decimal rate = mltp1 * (1 + mltp2);

                decimal prevAema = i >= 1 ? aemaList.LastOrDefault() : currentValue;
                decimal aema = i <= length ? sma : prevAema + (rate * (currentValue - prevAema));
                aemaList.Add(aema);

                var signal = GetCompareSignal(currentValue - aema, prevValue - prevAema);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Aema", aemaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = aemaList;
            stockData.IndicatorName = IndicatorName.AdaptiveExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive autonomous recursive moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="gamma">The gamma.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveAutonomousRecursiveMovingAverage(this StockData stockData, int length = 14, decimal gamma = 3)
        {
            List<decimal> ma1List = new();
            List<decimal> ma2List = new();
            List<decimal> absDiffList = new();
            List<decimal> dList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal er = erList.ElementAtOrDefault(i);
                decimal prevMa2 = i >= 1 ? ma2List.ElementAtOrDefault(i - 1) : currentValue;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal absDiff = Math.Abs(currentValue - prevMa2);
                absDiffList.Add(absDiff);

                decimal d = i != 0 ? absDiffList.Sum() / i * gamma : 0;
                dList.AddRounded(d);

                decimal c = currentValue > prevMa2 + d ? currentValue + d : currentValue < prevMa2 - d ? currentValue - d : prevMa2;
                decimal prevMa1 = i >= 1 ? ma1List.ElementAtOrDefault(i - 1) : currentValue;
                decimal ma1 = (er * c) + ((1 - er) * prevMa1);
                ma1List.Add(ma1);

                decimal ma2 = (er * ma1) + ((1 - er) * prevMa2);
                ma2List.Add(ma2);

                var signal = GetCompareSignal(currentValue - ma2, prevValue - prevMa2);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "D", dList },
                { "Aarma", ma2List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = ma2List;
            stockData.IndicatorName = IndicatorName.AdaptiveAutonomousRecursiveMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the autonomous recursive moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="momLength">Length of the mom.</param>
        /// <param name="gamma">The gamma.</param>
        /// <returns></returns>
        public static StockData CalculateAutonomousRecursiveMovingAverage(this StockData stockData, int length = 14, int momLength = 7, decimal gamma = 3)
        {
            List<decimal> madList = new();
            List<decimal> ma1List = new();
            List<decimal> absDiffList = new();
            List<decimal> cList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - momLength) : 0;
                decimal prevMad = i >= 1 ? madList.ElementAtOrDefault(i - 1) : currentValue;

                decimal absDiff = Math.Abs(priorValue - prevMad);
                absDiffList.Add(absDiff);

                decimal d = i != 0 ? absDiffList.Sum() / i * gamma : 0;
                decimal c = currentValue > prevMad + d ? currentValue + d : currentValue < prevMad - d ? currentValue - d : prevMad;
                cList.Add(c);

                decimal ma1 = cList.TakeLastExt(length).Average();
                ma1List.Add(ma1);

                decimal mad = ma1List.TakeLastExt(length).Average();
                madList.Add(mad);

                var signal = GetCompareSignal(currentValue - mad, prevValue - prevMad);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Arma", madList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = madList;
            stockData.IndicatorName = IndicatorName.AutonomousRecursiveMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the atr filtered exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <param name="atrLength">Length of the atr.</param>
        /// <param name="stdDevLength">Length of the standard dev.</param>
        /// <param name="lbLength">Length of the lb.</param>
        /// <param name="min">The minimum.</param>
        /// <returns></returns>
        public static StockData CalculateAtrFilteredExponentialMovingAverage(this StockData stockData, MovingAvgType maType, int length = 45, 
            int atrLength = 20, int stdDevLength = 10, int lbLength = 20, decimal min = 5)
        {
            List<decimal> trValList = new();
            List<decimal> atrValPowList = new();
            List<decimal> tempList = new();
            List<decimal> stdDevList = new();
            List<decimal> emaAFPList = new();
            List<decimal> emaCTPList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);

                decimal trVal = currentValue != 0 ? tr / currentValue : tr;
                trValList.Add(trVal);
            }

            var atrValList = GetMovingAverageList(stockData, maType, atrLength, trValList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal atrVal = atrValList.ElementAtOrDefault(i);

                decimal atrValPow = Pow(atrVal, 2);
                atrValPowList.Add(atrValPow);
            }

            var stdDevAList = GetMovingAverageList(stockData, maType, stdDevLength, atrValPowList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal stdDevA = stdDevAList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal atrVal = atrValList.ElementAtOrDefault(i);
                tempList.Add(atrVal);

                decimal atrValSum = tempList.TakeLastExt(stdDevLength).Sum();
                decimal stdDevB = Pow(atrValSum, 2) / Pow(stdDevLength, 2);

                decimal stdDev = stdDevA - stdDevB >= 0 ? Sqrt(stdDevA - stdDevB) : 0;
                stdDevList.Add(stdDev);

                decimal stdDevLow = stdDevList.TakeLastExt(lbLength).Min();
                decimal stdDevFactorAFP = stdDev != 0 ? stdDevLow / stdDev : 0;
                decimal stdDevFactorCTP = stdDevLow != 0 ? stdDev / stdDevLow : 0;
                decimal stdDevFactorAFPLow = Math.Min(stdDevFactorAFP, min);
                decimal stdDevFactorCTPLow = Math.Min(stdDevFactorCTP, min);
                decimal alphaAfp = (2 * stdDevFactorAFPLow) / (length + 1);
                decimal alphaCtp = (2 * stdDevFactorCTPLow) / (length + 1);

                decimal prevEmaAfp = emaAFPList.LastOrDefault();
                decimal emaAfp = (alphaAfp * currentValue) + ((1 - alphaAfp) * prevEmaAfp);
                emaAFPList.Add(emaAfp);

                decimal prevEmaCtp = emaCTPList.LastOrDefault();
                decimal emaCtp = (alphaCtp * currentValue) + ((1 - alphaCtp) * prevEmaCtp);
                emaCTPList.Add(emaCtp);

                var signal = GetCompareSignal(currentValue - emaAfp, prevValue - prevEmaAfp);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Afp", emaAFPList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = emaAFPList;
            stockData.IndicatorName = IndicatorName.AtrFilteredExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the adaptive least squares.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="smooth">The smooth.</param>
        /// <returns></returns>
        public static StockData CalculateAdaptiveLeastSquares(this StockData stockData, int length = 500, decimal smooth = 1.5m)
        {
            List<decimal> xList = new();
            List<decimal> yList = new();
            List<decimal> mxList = new();
            List<decimal> myList = new();
            List<decimal> regList = new();
            List<decimal> tempList = new();
            List<decimal> mxxList = new();
            List<decimal> myyList = new();
            List<decimal> mxyList = new();
            List<Signal> signalsList = new();
            var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal index = i;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentHigh = highList.ElementAtOrDefault(i);
                decimal currentLow = lowList.ElementAtOrDefault(i);

                decimal tr = CalculateTrueRange(currentHigh, currentLow, prevValue);
                tempList.Add(tr);

                decimal highest = tempList.TakeLastExt(length).Max();
                decimal alpha = highest != 0 ? MinOrMax(Pow(tr / highest, (double)smooth), 0.99m, 0.01m) : 0.01m;
                decimal xx = index * index;
                decimal yy = currentValue * currentValue;
                decimal xy = index * currentValue;

                decimal prevX = i >= 1 ? xList.ElementAtOrDefault(i - 1) : index;
                decimal x = (alpha * index) + ((1 - alpha) * prevX);
                xList.Add(x);

                decimal prevY = i >= 1 ? yList.ElementAtOrDefault(i - 1) : currentValue;
                decimal y = (alpha * currentValue) + ((1 - alpha) * prevY);
                yList.Add(y);

                decimal dx = Math.Abs(index - x);
                decimal dy = Math.Abs(currentValue - y);

                decimal prevMx = i >= 1 ? mxList.ElementAtOrDefault(i - 1) : dx;
                decimal mx = (alpha * dx) + ((1 - alpha) * prevMx);
                mxList.Add(mx);

                decimal prevMy = i >= 1 ? myList.ElementAtOrDefault(i - 1) : dy;
                decimal my = (alpha * dy) + ((1 - alpha) * prevMy);
                myList.Add(my);

                decimal prevMxx = i >= 1 ? mxxList.ElementAtOrDefault(i - 1) : xx;
                decimal mxx = (alpha * xx) + ((1 - alpha) * prevMxx);
                mxxList.Add(mxx);

                decimal prevMyy = i >= 1 ? myyList.ElementAtOrDefault(i - 1) : yy;
                decimal myy = (alpha * yy) + ((1 - alpha) * prevMyy);
                myyList.Add(myy);

                decimal prevMxy = i >= 1 ? mxyList.ElementAtOrDefault(i - 1) : xy;
                decimal mxy = (alpha * xy) + ((1 - alpha) * prevMxy);
                mxyList.Add(mxy);

                decimal alphaVal = (2 / alpha) + 1;
                decimal a1 = alpha != 0 ? (Pow(alphaVal, 2) * mxy) - (alphaVal * mx * alphaVal * my) : 0;
                decimal tempVal = ((Pow(alphaVal, 2) * mxx) - Pow(alphaVal * mx, 2)) * ((Pow(alphaVal, 2) * myy) - Pow(alphaVal * my, 2));
                decimal b1 = tempVal >= 0 ? Sqrt(tempVal) : 0;
                decimal r = b1 != 0 ? a1 / b1 : 0;
                decimal a = mx != 0 ? r * (my / mx) : 0;
                decimal b = y - (a * x);

                decimal prevReg = regList.LastOrDefault();
                decimal reg = (x * a) + b;
                regList.Add(reg);

                var signal = GetCompareSignal(currentValue - reg, prevValue - prevReg);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Als", regList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = regList;
            stockData.IndicatorName = IndicatorName.AdaptiveLeastSquares;

            return stockData;
        }

        /// <summary>
        /// Calculates the alpha decreasing exponential moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <returns></returns>
        public static StockData CalculateAlphaDecreasingExponentialMovingAverage(this StockData stockData)
        {
            List<decimal> emaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal alpha = (decimal)2 / (i + 1);

                decimal prevEma = emaList.LastOrDefault();
                decimal ema = (alpha * currentValue) + ((1 - alpha) * prevEma);
                emaList.Add(ema);

                var signal = GetCompareSignal(currentValue - ema, prevValue - prevEma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ema", emaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = emaList;
            stockData.IndicatorName = IndicatorName.AlphaDecreasingExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the powered kaufman adaptive moving average.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <param name="factor">The factor.</param>
        /// <returns></returns>
        public static StockData CalculatePoweredKaufmanAdaptiveMovingAverage(this StockData stockData, int length = 100, decimal factor = 3)
        {
            List<decimal> aList = new();
            List<decimal> aSpList = new();
            List<decimal> perList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal er = erList.ElementAtOrDefault(i);
                decimal powSp = er != 0 ? 1 / er : factor;
                decimal perSp = Pow(er, (double)powSp);

                decimal per = Pow(er, (double)factor);
                perList.AddRounded(per);

                decimal prevA = i >= 1 ? aList.LastOrDefault() : currentValue;
                decimal a = (per * currentValue) + ((1 - per) * prevA);
                aList.AddRounded(a);

                decimal prevASp = i >= 1 ? aSpList.LastOrDefault() : currentValue;
                decimal aSp = (perSp * currentValue) + ((1 - perSp) * prevASp);
                aSpList.AddRounded(aSp);

                var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Per", perList },
                { "Pkama", aList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = aList;
            stockData.IndicatorName = IndicatorName.PoweredKaufmanAdaptiveMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the automatic filter.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAutoFilter(this StockData stockData, MovingAvgType maType, int length = 500)
        {
            List<decimal> regList = new();
            List<decimal> corrList = new();
            List<decimal> interList = new();
            List<decimal> slopeList = new();
            List<decimal> tempList = new();
            List<decimal> xList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var yMaList = GetMovingAverageList(stockData, maType, length, inputList);
            var devList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal dev = devList.ElementAtOrDefault(i);

                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.Add(currentValue);

                decimal prevX = i >= 1 ? xList.ElementAtOrDefault(i - 1) : currentValue;
                decimal x = currentValue > prevX + dev ? currentValue : currentValue < prevX - dev ? currentValue : prevX;
                xList.Add(x);

                var corr = GoodnessOfFit.R(tempList.TakeLastExt(length).Select(x => (double)x), xList.TakeLastExt(length).Select(x => (double)x));
                corr = IsValueNullOrInfinity(corr) ? 0 : corr;
                corrList.Add((decimal)corr);
            }

            var xMaList = GetMovingAverageList(stockData, maType, length, xList);
            stockData.CustomValuesList = xList;
            var mxList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal my = devList.ElementAtOrDefault(i);
                decimal mx = mxList.ElementAtOrDefault(i);
                decimal corr = corrList.ElementAtOrDefault(i);
                decimal yMa = yMaList.ElementAtOrDefault(i);
                decimal xMa = xMaList.ElementAtOrDefault(i);
                decimal x = xList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal slope = mx != 0 ? corr * (my / mx) : 0;
                decimal inter = yMa - (slope * xMa);

                decimal prevReg = regList.LastOrDefault();
                decimal reg = (x * slope) + inter;
                regList.Add(reg);

                var signal = GetCompareSignal(currentValue - reg, prevValue - prevReg);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Af", regList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = regList;
            stockData.IndicatorName = IndicatorName.AutoFilter;

            return stockData;
        }

        /// <summary>
        /// Calculates the automatic line.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="maType">Type of the ma.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAutoLine(this StockData stockData, MovingAvgType maType, int length = 500)
        {
            List<decimal> xList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var devList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal dev = devList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevX = i >= 1 ? xList.ElementAtOrDefault(i - 1) : currentValue;
                decimal x = currentValue > prevX + dev ? currentValue : currentValue < prevX - dev ? currentValue : prevX;
                xList.Add(x);

                var signal = GetCompareSignal(currentValue - x, prevValue - prevX);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Al", xList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = xList;
            stockData.IndicatorName = IndicatorName.AutoLine;

            return stockData;
        }

        /// <summary>
        /// Calculates the automatic line with drift.
        /// </summary>
        /// <param name="stockData">The stock data.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static StockData CalculateAutoLineWithDrift(this StockData stockData, int length = 500)
        {
            List<decimal> aList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal dev = stdDevList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal r = Math.Round(currentValue);

                decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : r;
                decimal priorA = i >= length + 1 ? aList.ElementAtOrDefault(i - (length + 1)) : r;
                decimal a = currentValue > prevA + dev ? currentValue : currentValue < prevA - dev ? currentValue : 
                    prevA + ((decimal)1 / (length * 2) * (prevA - priorA));
                aList.Add(a);

                var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Alwd", aList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = aList;
            stockData.IndicatorName = IndicatorName.AutoLineWithDrift;

            return stockData;
        }

        /// <summary>
        /// Calculates the 1LC Least Squares Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData Calculate1LCLeastSquaresMovingAverage(this StockData stockData, MovingAvgType maType, int length = 14)
        {
            List<decimal> yList = new();
            List<decimal> tempList = new();
            List<decimal> corrList = new();
            List<decimal> indexList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);
            var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.Add(currentValue);

                decimal index = i;
                indexList.Add(index);

                var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
                corr = IsValueNullOrInfinity(corr) ? 0 : corr;
                corrList.Add((decimal)corr);
            }

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal corr = corrList.ElementAtOrDefault(i);
                decimal stdDev = stdDevList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevY = yList.LastOrDefault();
                decimal y = sma + (corr * stdDev * 1.7m);
                yList.Add(y);

                var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "1lsma", yList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = yList;
            stockData.IndicatorName = IndicatorName._1LCLeastSquaresMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the 3HMA
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData Calculate3HMA(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 50)
        {
            List<decimal> midList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int p = MinOrMax((int)Math.Ceiling((decimal)length / 2));
            int p1 = MinOrMax((int)Math.Ceiling((decimal)p / 3));
            int p2 = MinOrMax((int)Math.Ceiling((decimal)p / 2));

            var wma1List = GetMovingAverageList(stockData, maType, p1, inputList);
            var wma2List = GetMovingAverageList(stockData, maType, p2, inputList);
            var wma3List = GetMovingAverageList(stockData, maType, p, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal wma1 = wma1List.ElementAtOrDefault(i);
                decimal wma2 = wma2List.ElementAtOrDefault(i);
                decimal wma3 = wma3List.ElementAtOrDefault(i);

                decimal mid = (wma1 * 3) - wma2 - wma3;
                midList.Add(mid);
            }

            var aList = GetMovingAverageList(stockData, maType, p, midList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal a = aList.ElementAtOrDefault(i);
                decimal prevA = i >= 1 ? aList.ElementAtOrDefault(i - 1) : 0;
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                var signal = GetCompareSignal(currentValue - a, prevValue - prevA);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "3hma", aList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = aList;
            stockData.IndicatorName = IndicatorName._3HMA;

            return stockData;
        }
        
        /// <summary>
        /// Calculates the Jsa Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateJsaMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> jmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal priorValue = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevJma = jmaList.LastOrDefault();
                decimal jma = (currentValue + priorValue) / 2;
                jmaList.Add(jma);

                var signal = GetCompareSignal(currentValue - jma, prevValue - prevJma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Jma", jmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = jmaList;
            stockData.IndicatorName = IndicatorName.JsaMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Jurik Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="phase"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static StockData CalculateJurikMovingAverage(this StockData stockData, int length = 7, decimal phase = 50, decimal power = 2)
        {
            List<decimal> e0List = new();
            List<decimal> e1List = new();
            List<decimal> e2List = new();
            List<decimal> jmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal phaseRatio = phase < -100 ? 0.5m : phase > 100 ? 2.5m : ((decimal)phase / 100) + 1.5m;
            decimal ratio = 0.45m * (length - 1);
            decimal beta = ratio / (ratio + 2);
            decimal alpha = Pow(beta, (double)power);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevJma = jmaList.LastOrDefault();

                decimal prevE0 = e0List.LastOrDefault();
                decimal e0 = ((1 - alpha) * currentValue) + (alpha * prevE0);
                e0List.AddRounded(e0);

                decimal prevE1 = e1List.LastOrDefault();
                decimal e1 = ((currentValue - e0) * (1 - beta)) + (beta * prevE1);
                e1List.AddRounded(e1);

                decimal prevE2 = e2List.LastOrDefault();
                decimal e2 = ((e0 + (phaseRatio * e1) - prevJma) * Pow(1 - alpha, 2)) + (Pow(alpha, 2) * prevE2);
                e2List.AddRounded(e2);

                decimal jma = e2 + prevJma;
                jmaList.AddRounded(jma);

                Signal signal = GetCompareSignal(currentValue - jma, prevValue - prevJma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Jma", jmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = jmaList;
            stockData.IndicatorName = IndicatorName.JurikMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Zero Low Lag Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="lag"></param>
        /// <returns></returns>
        public static StockData CalculateZeroLowLagMovingAverage(this StockData stockData, int length = 50, decimal lag = 1.4m)
        {
            List<decimal> aList = new();
            List<decimal> bList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int lbLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priorB = i >= lbLength ? bList.ElementAtOrDefault(i - lbLength) : currentValue;
                decimal priorA = i >= length ? aList.ElementAtOrDefault(i - length) : 0;

                decimal prevA = aList.LastOrDefault();
                decimal a = (lag * currentValue) + ((1 - lag) * priorB) + prevA;
                aList.Add(a);

                decimal aDiff = a - priorA;
                decimal prevB = bList.LastOrDefault();
                decimal b = aDiff / length;
                bList.Add(b);

                var signal = GetCompareSignal(currentValue - b, prevValue - prevB);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Zllma", bList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bList;
            stockData.IndicatorName = IndicatorName.ZeroLowLagMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Zero Lag Exponential Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateZeroLagExponentialMovingAverage(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 14)
        {
            List<decimal> zemaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal ema1 = ema1List.ElementAtOrDefault(i);
                decimal ema2 = ema2List.ElementAtOrDefault(i);
                decimal d = ema1 - ema2;

                decimal prevZema = zemaList.LastOrDefault();
                decimal zema = ema1 + d;
                zemaList.Add(zema);

                var signal = GetCompareSignal(currentValue - zema, prevValue - prevZema);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Zema", zemaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = zemaList;
            stockData.IndicatorName = IndicatorName.ZeroLagExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Zero Lag Triple Exponential Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateZeroLagTripleExponentialMovingAverage(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.TripleExponentialMovingAverage, int length = 14)
        {
            List<decimal> zlTemaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var tma1List = GetMovingAverageList(stockData, maType, length, inputList);
            var tma2List = GetMovingAverageList(stockData, maType, length, tma1List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal tma1 = tma1List.ElementAtOrDefault(i);
                decimal tma2 = tma2List.ElementAtOrDefault(i);
                decimal diff = tma1 - tma2;

                decimal prevZltema = zlTemaList.LastOrDefault();
                decimal zltema = tma1 + diff;
                zlTemaList.Add(zltema);

                var signal = GetCompareSignal(currentValue - zltema, prevValue - prevZltema);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Ztema", zlTemaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = zlTemaList;
            stockData.IndicatorName = IndicatorName.ZeroLagTripleExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Bryant Adaptive Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="maxLength"></param>
        /// <param name="trend"></param>
        /// <returns></returns>
        public static StockData CalculateBryantAdaptiveMovingAverage(this StockData stockData, int length = 14, int maxLength = 100, decimal trend = -1)
        {
            List<decimal> bamaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var erList = CalculateKaufmanAdaptiveMovingAverage(stockData, length: length).OutputValues["Er"];

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal er = erList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal ver = Pow(er - (((2 * er) - 1) / 2 * (1 - trend)) + 0.5m, 2);
                decimal vLength = ver != 0 ? (length - ver + 1) / ver : 0;
                vLength = Math.Min(vLength, maxLength);
                decimal vAlpha = 2 / (vLength + 1);

                decimal prevBama = bamaList.LastOrDefault();
                decimal bama = (vAlpha * currentValue) + ((1 - vAlpha) * prevBama);
                bamaList.Add(bama);

                var signal = GetCompareSignal(currentValue - bama, prevValue - prevBama);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Bama", bamaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bamaList;
            stockData.IndicatorName = IndicatorName.BryantAdaptiveMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Windowed Volume Weighted Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateWindowedVolumeWeightedMovingAverage(this StockData stockData, int length = 100)
        {
            List<decimal> bartlettWList = new();
            List<decimal> blackmanWList = new();
            List<decimal> hanningWList = new();
            List<decimal> bartlettVWList = new();
            List<decimal> blackmanVWList = new();
            List<decimal> hanningVWList = new();
            List<decimal> bartlettWvwmaList = new();
            List<decimal> blackmanWvwmaList = new();
            List<decimal> hanningWvwmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, volumeList) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentVolume = volumeList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal iRatio = (decimal)i / length;
                decimal bartlett = 1 - (2 * Math.Abs(i - ((decimal)length / 2)) / length);

                decimal bartlettW = bartlett * currentVolume;
                bartlettWList.Add(bartlettW);

                decimal bartlettWSum = bartlettWList.TakeLastExt(length).Sum();
                decimal bartlettVW = currentValue * bartlettW;
                bartlettVWList.Add(bartlettVW);

                decimal bartlettVWSum = bartlettVWList.TakeLastExt(length).Sum();
                decimal prevBartlettWvwma = bartlettWvwmaList.LastOrDefault();
                decimal bartlettWvwma = bartlettWSum != 0 ? bartlettVWSum / bartlettWSum : 0;
                bartlettWvwmaList.Add(bartlettWvwma);

                decimal blackman = 0.42m - (0.5m * Cos(2 * (decimal)Math.PI * iRatio)) + (0.08m * Cos(4 * (decimal)Math.PI * iRatio));
                decimal blackmanW = blackman * currentVolume;
                blackmanWList.Add(blackmanW);

                decimal blackmanWSum = blackmanWList.TakeLastExt(length).Sum();
                decimal blackmanVW = currentValue * blackmanW;
                blackmanVWList.Add(blackmanVW);

                decimal blackmanVWSum = blackmanVWList.TakeLastExt(length).Sum();
                decimal blackmanWvwma = blackmanWSum != 0 ? blackmanVWSum / blackmanWSum : 0;
                blackmanWvwmaList.Add(blackmanWvwma);

                decimal hanning = 0.5m - (0.5m * Cos(2 * (decimal)Math.PI * iRatio));
                decimal hanningW = hanning * currentVolume;
                hanningWList.Add(hanningW);

                decimal hanningWSum = hanningWList.TakeLastExt(length).Sum();
                decimal hanningVW = currentValue * hanningW;
                hanningVWList.Add(hanningVW);

                decimal hanningVWSum = hanningVWList.TakeLastExt(length).Sum();
                decimal hanningWvwma = hanningWSum != 0 ? hanningVWSum / hanningWSum : 0;
                hanningWvwmaList.Add(hanningWvwma);

                var signal = GetCompareSignal(currentValue - bartlettWvwma, prevValue - prevBartlettWvwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wvwma", bartlettWvwmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = bartlettWvwmaList;
            stockData.IndicatorName = IndicatorName.WindowedVolumeWeightedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Well Rounded Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateWellRoundedMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> aList = new();
            List<decimal> bList = new();
            List<decimal> yList = new();
            List<decimal> srcYList = new();
            List<decimal> srcEmaList = new();
            List<decimal> yEmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal alpha = (decimal)2 / (length + 1);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSrcY = i >= 1 ? srcYList.ElementAtOrDefault(i - 1) : 0;
                decimal prevSrcEma = i >= 1 ? srcEmaList.ElementAtOrDefault(i - 1) : 0;

                decimal prevA = aList.LastOrDefault();
                decimal a = prevA + (alpha * prevSrcY);
                aList.Add(a);

                decimal prevB = bList.LastOrDefault();
                decimal b = prevB + (alpha * prevSrcEma);
                bList.Add(b);

                decimal ab = a + b;
                decimal prevY = yList.LastOrDefault();
                decimal y = CalculateEMA(ab, prevY, 1);
                yList.Add(y);

                decimal srcY = currentValue - y;
                srcYList.Add(srcY);

                decimal prevYEma = yEmaList.LastOrDefault();
                decimal yEma = CalculateEMA(y, prevYEma, length);
                yEmaList.Add(yEma);

                decimal srcEma = currentValue - yEma;
                srcEmaList.Add(srcEma);

                var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wrma", yList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = yList;
            stockData.IndicatorName = IndicatorName.WellRoundedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Welles Wilder Summation
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateWellesWilderSummation(this StockData stockData, int length = 14)
        {
            List<decimal> sumList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevSum = sumList.LastOrDefault();
                decimal sum = prevSum - (prevSum / length) + currentValue;
                sumList.Add(sum);

                var signal = GetCompareSignal(currentValue - sum, prevValue - prevSum);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Wws", sumList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = sumList;
            stockData.IndicatorName = IndicatorName.WellesWilderSummation;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quick Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateQuickMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> qmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int peak = MinOrMax((int)Math.Ceiling((decimal)length / 3));

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal num = 0, denom = 0;
                for (int j = 1; j <= length + 1; j++)
                {
                    decimal mult = j <= peak ? (decimal)j / peak : (decimal)(length + 1 - j) / (length + 1 - peak);
                    decimal prevValue = i >= j - 1 ? inputList.ElementAtOrDefault(i - (j - 1)) : 0;

                    num += prevValue * mult;
                    denom += mult;
                }

                decimal prevQma = qmaList.LastOrDefault();
                decimal qma = denom != 0 ? num / denom : 0;
                qmaList.Add(qma);

                var signal = GetCompareSignal(currentValue - qma, prevVal - prevQma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Qma", qmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = qmaList;
            stockData.IndicatorName = IndicatorName.QuickMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quadratic Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateQuadraticMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> qmaList = new();
            List<decimal> powList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal pow = Pow(currentValue, 2);
                powList.Add(pow);

                decimal prevQma = qmaList.LastOrDefault();
                decimal powSma = powList.TakeLastExt(length).Average();
                decimal qma = powSma >= 0 ? Sqrt(powSma) : 0;
                qmaList.Add(qma);

                var signal = GetCompareSignal(currentValue - qma, prevValue - prevQma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Qma", qmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = qmaList;
            stockData.IndicatorName = IndicatorName.QuadraticMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quadruple Exponential Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateQuadrupleExponentialMovingAverage(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 20)
        {
            List<decimal> qemaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);
            var ema3List = GetMovingAverageList(stockData, maType, length, ema2List);
            var ema4List = GetMovingAverageList(stockData, maType, length, ema3List);
            var ema5List = GetMovingAverageList(stockData, maType, length, ema4List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal ema1 = ema1List.ElementAtOrDefault(i);
                decimal ema2 = ema2List.ElementAtOrDefault(i);
                decimal ema3 = ema3List.ElementAtOrDefault(i);
                decimal ema4 = ema4List.ElementAtOrDefault(i);
                decimal ema5 = ema5List.ElementAtOrDefault(i);

                decimal prevQema = qemaList.LastOrDefault();
                decimal qema = (5 * ema1) - (10 * ema2) + (10 * ema3) - (5 * ema4) + ema5;
                qemaList.Add(qema);

                var signal = GetCompareSignal(currentValue - qema, prevValue - prevQema);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Qema", qemaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = qemaList;
            stockData.IndicatorName = IndicatorName.QuadrupleExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quadratic Least Squares Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="forecastLength"></param>
        /// <returns></returns>
        public static StockData CalculateQuadraticLeastSquaresMovingAverage(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 50, int forecastLength = 14)
        {
            List<decimal> nList = new();
            List<decimal> n2List = new();
            List<decimal> nn2List = new();
            List<decimal> nn2CovList = new();
            List<decimal> n2vList = new();
            List<decimal> n2vCovList = new();
            List<decimal> nvList = new();
            List<decimal> nvCovList = new();
            List<decimal> qlsmaList = new();
            List<decimal> fcastList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var smaList = GetMovingAverageList(stockData, maType, length, inputList);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);

                decimal n = i;
                nList.Add(n);

                decimal n2 = Pow(n, 2);
                n2List.Add(n2);

                decimal nn2 = n * n2;
                nn2List.Add(nn2);

                decimal n2v = n2 * currentValue;
                n2vList.Add(n2v);

                decimal nv = n * currentValue;
                nvList.Add(nv);
            }

            var nSmaList = GetMovingAverageList(stockData, maType, length, nList);
            var n2SmaList = GetMovingAverageList(stockData, maType, length, n2List);
            var n2vSmaList = GetMovingAverageList(stockData, maType, length, n2vList);
            var nvSmaList = GetMovingAverageList(stockData, maType, length, nvList);
            var nn2SmaList = GetMovingAverageList(stockData, maType, length, nn2List);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal nSma = nSmaList.ElementAtOrDefault(i);
                decimal n2Sma = n2SmaList.ElementAtOrDefault(i);
                decimal n2vSma = n2vSmaList.ElementAtOrDefault(i);
                decimal nvSma = nvSmaList.ElementAtOrDefault(i);
                decimal nn2Sma = nn2SmaList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);

                decimal nn2Cov = nn2Sma - (nSma * n2Sma);
                nn2CovList.Add(nn2Cov);

                decimal n2vCov = n2vSma - (n2Sma * sma);
                n2vCovList.Add(n2vCov);

                decimal nvCov = nvSma - (nSma * sma);
                nvCovList.Add(nvCov);
            }

            stockData.CustomValuesList = nList;
            var nVarianceList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            stockData.CustomValuesList = n2List;
            var n2VarianceList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal n2Variance = n2VarianceList.ElementAtOrDefault(i);
                decimal nVariance = nVarianceList.ElementAtOrDefault(i);
                decimal nn2Cov = nn2CovList.ElementAtOrDefault(i);
                decimal n2vCov = n2vCovList.ElementAtOrDefault(i);
                decimal nvCov = nvCovList.ElementAtOrDefault(i);
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal n2Sma = n2SmaList.ElementAtOrDefault(i);
                decimal nSma = nSmaList.ElementAtOrDefault(i);
                decimal n2 = n2List.ElementAtOrDefault(i);
                decimal norm = (n2Variance * nVariance) - Pow(nn2Cov, 2);
                decimal a = norm != 0 ? ((n2vCov * nVariance) - (nvCov * nn2Cov)) / norm : 0;
                decimal b = norm != 0 ? ((nvCov * n2Variance) - (n2vCov * nn2Cov)) / norm : 0;
                decimal c = sma - (a * n2Sma) - (b * nSma);

                decimal prevQlsma = qlsmaList.LastOrDefault();
                decimal qlsma = (a * n2) + (b * i) + c;
                qlsmaList.Add(qlsma);

                decimal fcast = (a * Pow(i + forecastLength, 2)) + (b * (i + forecastLength)) + c;
                fcastList.Add(fcast);

                var signal = GetCompareSignal(currentValue - qlsma, prevValue - prevQlsma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Qlma", qlsmaList },
                { "Forecast", fcastList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = qlsmaList;
            stockData.IndicatorName = IndicatorName.QuadraticLeastSquaresMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Quadratic Regression
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateQuadraticRegression(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 500)
        {
            List<decimal> tempList = new();
            List<decimal> x1List = new();
            List<decimal> x2List = new();
            List<decimal> x1SumList = new();
            List<decimal> x2SumList = new();
            List<decimal> x1x2List = new();
            List<decimal> x1x2SumList = new();
            List<decimal> x2PowList = new();
            List<decimal> x2PowSumList = new();
            List<decimal> ySumList = new();
            List<decimal> yx1List = new();
            List<decimal> yx2List = new();
            List<decimal> yx1SumList = new();
            List<decimal> yx2SumList = new();
            List<decimal> yList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal y = inputList.ElementAtOrDefault(i);
                tempList.Add(y);

                decimal x1 = i;
                x1List.Add(x1);

                decimal x2 = Pow(x1, 2);
                x2List.Add(x2);

                decimal x1x2 = x1 * x2;
                x1x2List.Add(x1x2);

                decimal yx1 = y * x1;
                yx1List.Add(yx1);

                decimal yx2 = y * x2;
                yx2List.Add(yx2);

                decimal x2Pow = Pow(x2, 2);
                x2PowList.Add(x2Pow);

                decimal ySum = tempList.TakeLastExt(length).Sum();
                ySumList.Add(ySum);

                decimal x1Sum = x1List.TakeLastExt(length).Sum();
                x1SumList.Add(x1Sum);

                decimal x2Sum = x2List.TakeLastExt(length).Sum();
                x2SumList.Add(x2Sum);

                decimal x1x2Sum = x1x2List.TakeLastExt(length).Sum();
                x1x2SumList.Add(x1x2Sum);

                decimal yx1Sum = yx1List.TakeLastExt(length).Sum();
                yx1SumList.Add(yx1Sum);

                decimal yx2Sum = yx2List.TakeLastExt(length).Sum();
                yx2SumList.Add(yx2Sum);

                decimal x2PowSum = x2PowList.TakeLastExt(length).Sum();
                x2PowSumList.Add(x2PowSum);
            }

            var max1List = GetMovingAverageList(stockData, maType, length, x1List);
            var max2List = GetMovingAverageList(stockData, maType, length, x2List);
            var mayList = GetMovingAverageList(stockData, maType, length, inputList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal x1Sum = x1SumList.ElementAtOrDefault(i);
                decimal x2Sum = x2SumList.ElementAtOrDefault(i);
                decimal x1x2Sum = x1x2SumList.ElementAtOrDefault(i);
                decimal x2PowSum = x2PowSumList.ElementAtOrDefault(i);
                decimal yx1Sum = yx1SumList.ElementAtOrDefault(i);
                decimal yx2Sum = yx2SumList.ElementAtOrDefault(i);
                decimal ySum = ySumList.ElementAtOrDefault(i);
                decimal may = mayList.ElementAtOrDefault(i);
                decimal max1 = max1List.ElementAtOrDefault(i);
                decimal max2 = max2List.ElementAtOrDefault(i);
                decimal x1 = x1List.ElementAtOrDefault(i);
                decimal x2 = x2List.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal s11 = x2Sum - (Pow(x1Sum, 2) / length);
                decimal s12 = x1x2Sum - ((x1Sum * x2Sum) / length);
                decimal s22 = x2PowSum - (Pow(x2Sum, 2) / length);
                decimal sy1 = yx1Sum - ((ySum * x1Sum) / length);
                decimal sy2 = yx2Sum - ((ySum * x2Sum) / length);
                decimal bot = (s22 * s11) - Pow(s12, 2);
                decimal b2 = bot != 0 ? ((sy1 * s22) - (sy2 * s12)) / bot : 0;
                decimal b3 = bot != 0 ? ((sy2 * s11) - (sy1 * s12)) / bot : 0;
                decimal b1 = may - (b2 * max1) - (b3 * max2);

                decimal prevY = yList.LastOrDefault();
                decimal y = b1 + (b2 * x1) + (b3 * x2);
                yList.Add(y);

                var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "QuadReg", yList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = yList;
            stockData.IndicatorName = IndicatorName.QuadraticRegression;

            return stockData;
        }

        /// <summary>
        /// Calculates the Linear Weighted Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateLinearWeightedMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> lwmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j <= length - 1; j++)
                {
                    decimal weight = length - j;
                    decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal prevLwma = lwmaList.LastOrDefault();
                decimal lwma = weightedSum != 0 ? sum / weightedSum : 0;
                lwmaList.Add(lwma);

                var signal = GetCompareSignal(currentValue - lwma, prevVal - prevLwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Lwma", lwmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = lwmaList;
            stockData.IndicatorName = IndicatorName.LinearWeightedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Leo Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateLeoMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> lmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var wmaList = CalculateWeightedMovingAverage(stockData, length).CustomValuesList;
            var smaList = CalculateSimpleMovingAverage(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentWma = wmaList.ElementAtOrDefault(i);
                decimal currentSma = smaList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevLma = lmaList.LastOrDefault();
                decimal lma = (2 * currentWma) - currentSma;
                lmaList.Add(lma);

                var signal = GetCompareSignal(currentValue - lma, prevValue - prevLma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Lma", lmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = lmaList;
            stockData.IndicatorName = IndicatorName.LeoMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Light Least Squares Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateLightLeastSquaresMovingAverage(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 250)
        {
            List<decimal> yList = new();
            List<decimal> indexList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int length1 = MinOrMax((int)Math.Ceiling((decimal)length / 2));

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal index = i;
                indexList.Add(index);
            }

            var sma1List = GetMovingAverageList(stockData, maType, length, inputList);
            var sma2List = GetMovingAverageList(stockData, maType, length1, inputList);
            var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            stockData.CustomValuesList = indexList;
            var indexStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal sma1 = sma1List.ElementAtOrDefault(i);
                decimal sma2 = sma2List.ElementAtOrDefault(i);
                decimal stdDev = stdDevList.ElementAtOrDefault(i);
                decimal indexStdDev = indexStdDevList.ElementAtOrDefault(i);
                decimal indexSma = indexSmaList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal c = stdDev != 0 ? (sma2 - sma1) / stdDev : 0;
                decimal z = indexStdDev != 0 && c != 0 ? (i - indexSma) / indexStdDev * c : 0;

                decimal prevY = yList.LastOrDefault();
                decimal y = sma1 + (z * stdDev);
                yList.Add(y);

                var signal = GetCompareSignal(currentValue - y, prevValue - prevY);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Llsma", yList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = yList;
            stockData.IndicatorName = IndicatorName.LightLeastSquaresMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Linear Extrapolation
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateLinearExtrapolation(this StockData stockData, int length = 500)
        {
            List<decimal> extList = new();
            List<decimal> xList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevY = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priorY = i >= length ? inputList.ElementAtOrDefault(i - length) : 0;
                decimal priorY2 = i >= length * 2 ? inputList.ElementAtOrDefault(i - (length * 2)) : 0;
                decimal priorX = i >= length ? xList.ElementAtOrDefault(i - length) : 0;
                decimal priorX2 = i >= length * 2 ? xList.ElementAtOrDefault(i - (length * 2)) : 0;

                decimal x = i;
                xList.Add(i);

                decimal prevExt = extList.LastOrDefault();
                decimal ext = priorX2 - priorX != 0 && priorY2 - priorY != 0 ? priorY + ((x - priorX) / (priorX2 - priorX) * (priorY2 - priorY)) : priorY;
                extList.Add(ext);

                var signal = GetCompareSignal(currentValue - ext, prevY - prevExt);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "LinExt", extList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = extList;
            stockData.IndicatorName = IndicatorName.LinearExtrapolation;

            return stockData;
        }

        /// <summary>
        /// Calculates the Linear Regression Line
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateLinearRegressionLine(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 14)
        {
            List<decimal> regList = new();
            List<decimal> corrList = new();
            List<decimal> yList = new();
            List<decimal> xList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var yMaList = GetMovingAverageList(stockData, maType, length, inputList);
            var myList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                yList.Add(currentValue);

                decimal x = i;
                xList.Add(x);

                var corr = GoodnessOfFit.R(yList.TakeLastExt(length).Select(x => (double)x), xList.TakeLastExt(length).Select(x => (double)x));
                corr = IsValueNullOrInfinity(corr) ? 0 : corr;
                corrList.Add((decimal)corr);
            }

            var xMaList = GetMovingAverageList(stockData, maType, length, xList);
            stockData.CustomValuesList = xList;
            var mxList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList; ;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal my = myList.ElementAtOrDefault(i);
                decimal mx = mxList.ElementAtOrDefault(i);
                decimal corr = corrList.ElementAtOrDefault(i);
                decimal yMa = yMaList.ElementAtOrDefault(i);
                decimal xMa = xMaList.ElementAtOrDefault(i);
                decimal x = xList.ElementAtOrDefault(i);
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal slope = mx != 0 ? corr * (my / mx) : 0;
                decimal inter = yMa - (slope * xMa);

                decimal prevReg = regList.LastOrDefault();
                decimal reg = (x * slope) + inter;
                regList.Add(reg);

                var signal = GetCompareSignal(currentValue - reg, prevValue - prevReg);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "LinReg", regList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = regList;
            stockData.IndicatorName = IndicatorName.LinearRegressionLine;

            return stockData;
        }

        /// <summary>
        /// Calculates the IIR Least Squares Estimate
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateIIRLeastSquaresEstimate(this StockData stockData, int length = 100)
        {
            List<decimal> sList = new();
            List<decimal> sEmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal a = (decimal)4 / (length + 2);
            int halfLength = MinOrMax((int)Math.Ceiling((decimal)length / 2));

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevS = i >= 1 ? sList.ElementAtOrDefault(i - 1) : currentValue;
                decimal prevSEma = sEmaList.LastOrDefault();
                decimal sEma = CalculateEMA(prevS, prevSEma, halfLength);
                sEmaList.Add(prevSEma);

                decimal s = (a * currentValue) + prevS - (a * sEma);
                sList.Add(s);

                var signal = GetCompareSignal(currentValue - s, prevValue - prevS);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "IIRLse", sList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = sList;
            stockData.IndicatorName = IndicatorName.IIRLeastSquaresEstimate;

            return stockData;
        }

        /// <summary>
        /// Calculates the Inverse Distance Weighted Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateInverseDistanceWeightedMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> idwmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevVal = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j <= length - 1; j++)
                {
                    decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                    decimal weight = 0;
                    for (int k = 0; k <= length - 1; k++)
                    {
                        decimal prevValue2 = i >= k ? inputList.ElementAtOrDefault(i - k) : 0;
                        weight += Math.Abs(prevValue - prevValue2);
                    }

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal prevIdwma = idwmaList.LastOrDefault();
                decimal idwma = weightedSum != 0 ? sum / weightedSum : 0;
                idwmaList.Add(idwma);

                var signal = GetCompareSignal(currentValue - idwma, prevVal - prevIdwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Idwma", idwmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = idwmaList;
            stockData.IndicatorName = IndicatorName.InverseDistanceWeightedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Trimean
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateTrimean(this StockData stockData, int length = 14)
        {
            List<decimal> tempList = new();
            List<decimal> medianList = new();
            List<decimal> q1List = new();
            List<decimal> q3List = new();
            List<decimal> trimeanList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevValue = tempList.LastOrDefault();
                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.Add(currentValue);

                var lookBackList = tempList.TakeLastExt(length);

                decimal q1 = lookBackList.PercentileNearestRank(25);
                q1List.Add(q1);

                decimal median = lookBackList.PercentileNearestRank(50);
                medianList.Add(median);

                decimal q3 = lookBackList.PercentileNearestRank(75);
                q3List.Add(q3);

                decimal prevTrimean = trimeanList.LastOrDefault();
                decimal trimean = (q1 + (2 * median) + q3) / 4;
                trimeanList.Add(trimean);

                var signal = GetCompareSignal(currentValue - trimean, prevValue - prevTrimean);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Trimean", trimeanList },
                { "Q1", q1List },
                { "Median", medianList },
                { "Q3", q3List }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = trimeanList;
            stockData.IndicatorName = IndicatorName.Trimean;

            return stockData;
        }

        /// <summary>
        /// Calculates the Optimal Weighted Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateOptimalWeightedMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> tempList = new();
            List<decimal> owmaList = new();
            List<decimal> prevOwmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal prevVal = tempList.LastOrDefault();
                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.Add(currentValue);

                decimal prevOwma = i >= 1 ? owmaList.ElementAtOrDefault(i - 1) : 0;
                prevOwmaList.Add(prevOwma);

                var corr = GoodnessOfFit.R(tempList.TakeLastExt(length).Select(x => (double)x), prevOwmaList.TakeLastExt(length).Select(x => (double)x));
                corr = IsValueNullOrInfinity((double)corr) ? 0 : corr;

                decimal sum = 0, weightedSum = 0;
                for (int j = 0; j <= length - 1; j++)
                {
                    decimal weight = Pow(length - j, corr);
                    decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;

                    sum += prevValue * weight;
                    weightedSum += weight;
                }

                decimal owma = weightedSum != 0 ? sum / weightedSum : 0;
                owmaList.Add(owma);

                var signal = GetCompareSignal(currentValue - owma, prevVal - prevOwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Owma", owmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = owmaList;
            stockData.IndicatorName = IndicatorName.OptimalWeightedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Overshoot Reduction Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateOvershootReductionMovingAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, 
            int length = 14)
        {
            List<decimal> indexList = new();
            List<decimal> bList = new();
            List<decimal> dList = new();
            List<decimal> bSmaList = new();
            List<decimal> corrList = new();
            List<decimal> tempList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int length1 = (int)Math.Ceiling((decimal)length / 2);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal index = i;
                indexList.Add(index);

                decimal currentValue = inputList.ElementAtOrDefault(i);
                tempList.Add(currentValue);

                var corr = GoodnessOfFit.R(indexList.TakeLastExt(length).Select(x => (double)x), tempList.TakeLastExt(length).Select(x => (double)x));
                corr = IsValueNullOrInfinity(corr) ? 0 : corr;
                corrList.Add((decimal)corr);
            }

            var indexSmaList = GetMovingAverageList(stockData, maType, length, indexList);
            var smaList = GetMovingAverageList(stockData, maType, length, inputList);
            var stdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            stockData.CustomValuesList = indexList;
            var indexStdDevList = CalculateStandardDeviationVolatility(stockData, length).CustomValuesList;
            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal index = indexList.ElementAtOrDefault(i);
                decimal indexSma = indexSmaList.ElementAtOrDefault(i);
                decimal indexStdDev = indexStdDevList.ElementAtOrDefault(i);
                decimal corr = corrList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal prevD = i >= 1 ? dList.ElementAtOrDefault(i - 1) != 0 ? dList.ElementAtOrDefault(i - 1) : prevValue : prevValue;
                decimal sma = smaList.ElementAtOrDefault(i);
                decimal stdDev = stdDevList.ElementAtOrDefault(i);
                decimal a = indexStdDev != 0 && corr != 0 ? (index - indexSma) / indexStdDev * corr : 0;

                decimal b = Math.Abs(prevD - currentValue);
                bList.Add(b);

                decimal bSma = bList.TakeLastExt(length1).Average();
                bSmaList.Add(bSma);

                decimal highest = bSmaList.TakeLastExt(length).Max();
                decimal c = highest != 0 ? b / highest : 0;

                decimal d = sma + (a * (stdDev * c));
                dList.Add(d);

                var signal = GetCompareSignal(currentValue - d, prevValue - prevD);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Orma", dList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = dList;
            stockData.IndicatorName = IndicatorName.OvershootReductionMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Variable Index Dynamic Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateVariableIndexDynamicAverage(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
            int length = 14)
        {
            List<decimal> vidyaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            decimal alpha = (decimal)2 / (length + 1);

            var cmoList = CalculateChandeMomentumOscillator(stockData, maType, length: length).CustomValuesList;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal currentCmo = Math.Abs(cmoList.ElementAtOrDefault(i) / 100);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal prevVidya = vidyaList.LastOrDefault();
                decimal currentVidya = (currentValue * alpha * currentCmo) + (prevVidya * (1 - (alpha * currentCmo)));
                vidyaList.Add(currentVidya);

                var signal = GetCompareSignal(currentValue - currentVidya, prevValue - prevVidya);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Vidya", vidyaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = vidyaList;
            stockData.IndicatorName = IndicatorName.VariableIndexDynamicAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Natural Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateNaturalMovingAverage(this StockData stockData, int length = 40)
        {
            List<decimal> lnList = new();
            List<decimal> nmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal ln = currentValue > 0 ? Log(currentValue) * 1000 : 0;
                lnList.Add(ln);

                decimal num = 0, denom = 0;
                for (int j = 0; j < length; j++)
                {
                    decimal currentLn = i >= j ? lnList.ElementAtOrDefault(i - j) : 0;
                    decimal prevLn = i >= j + 1 ? lnList.ElementAtOrDefault(i - (j + 1)) : 0;
                    decimal oi = Math.Abs(currentLn - prevLn);
                    num += oi * (Sqrt(j + 1) - Sqrt(j));
                    denom += oi;
                }

                decimal ratio = denom != 0 ? num / denom : 0;
                decimal prevNma = nmaList.LastOrDefault();
                decimal nma = (currentValue * ratio) + (prevValue * (1 - ratio));
                nmaList.Add(nma);

                var signal = GetCompareSignal(currentValue - nma, prevValue - prevNma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Nma", nmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = nmaList;
            stockData.IndicatorName = IndicatorName.NaturalMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Symmetrically Weighted Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static StockData CalculateSymmetricallyWeightedMovingAverage(this StockData stockData, int length = 14)
        {
            List<decimal> swmaList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int floorLength = (int)Math.Floor((decimal)length / 2);
            int roundLength = (int)Math.Round((decimal)length / 2);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

                decimal nr = 0, nl = 0, sr = 0, sl = 0;
                if (floorLength == roundLength)
                {
                    for (int j = 0; j <= floorLength - 1; j++)
                    {
                        decimal wr = (length - (length - 1 - j)) * length;
                        decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                        nr += wr;
                        sr += prevVal * wr;
                    }

                    for (int j = floorLength; j <= length - 1; j++)
                    {
                        decimal wl = (length - j) * length;
                        decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                        nl += wl;
                        sl += prevVal * wl;
                    }
                }
                else
                {
                    for (int j = 0; j <= floorLength; j++)
                    {
                        decimal wr = (length - (length - 1 - j)) * length;
                        decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                        nr += wr;
                        sr += prevVal * wr;
                    }

                    for (int j = roundLength; j <= length - 1; j++)
                    {
                        decimal wl = (length - j) * length;
                        decimal prevVal = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                        nl += wl;
                        sl += prevVal * wl;
                    }
                }

                decimal prevSwma = swmaList.LastOrDefault();
                decimal swma = nr + nl != 0 ? (sr + sl) / (nr + nl) : 0;
                swmaList.AddRounded(swma);

                var signal = GetCompareSignal(currentValue - swma, prevValue - prevSwma);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Swma", swmaList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = swmaList;
            stockData.IndicatorName = IndicatorName.SymmetricallyWeightedMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the Generalized Double Exponential Moving Average
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="maType"></param>
        /// <param name="length"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static StockData CalculateGeneralizedDoubleExponentialMovingAverage(this StockData stockData, 
            MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length = 5, decimal factor = 0.7m)
        {
            List<decimal> gdList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            var ema1List = GetMovingAverageList(stockData, maType, length, inputList);
            var ema2List = GetMovingAverageList(stockData, maType, length, ema1List);

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal currentEma1 = ema1List.ElementAtOrDefault(i);
                decimal currentEma2 = ema2List.ElementAtOrDefault(i);

                decimal prevGd = gdList.LastOrDefault();
                decimal gd = (currentEma1 * (1 + factor)) - (currentEma2 * factor);
                gdList.Add(gd);

                var signal = GetCompareSignal(currentValue - gd, prevValue - prevGd);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Gdema", gdList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = gdList;
            stockData.IndicatorName = IndicatorName.GeneralizedDoubleExponentialMovingAverage;

            return stockData;
        }

        /// <summary>
        /// Calculates the General Filter Estimator
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="length"></param>
        /// <param name="beta"></param>
        /// <param name="gamma"></param>
        /// <param name="zeta"></param>
        /// <returns></returns>
        public static StockData CalculateGeneralFilterEstimator(this StockData stockData, int length = 100, decimal beta = 5.25m, decimal gamma = 1,
            decimal zeta = 1)
        {
            List<decimal> dList = new();
            List<decimal> bList = new();
            List<Signal> signalsList = new();
            var (inputList, _, _, _, _) = GetInputValuesList(stockData);

            int p = beta != 0 ? (int)Math.Ceiling(length / beta) : 0;

            for (int i = 0; i < stockData.Count; i++)
            {
                decimal currentValue = inputList.ElementAtOrDefault(i);
                decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
                decimal priorB = i >= p ? bList.ElementAtOrDefault(i - p) : currentValue;
                decimal a = currentValue - priorB;

                decimal prevB = i >= 1 ? bList.ElementAtOrDefault(i - 1) : currentValue;
                decimal b = prevB + (a / p * gamma);
                bList.Add(b);

                decimal priorD = i >= p ? dList.ElementAtOrDefault(i - p) : b;
                decimal c = b - priorD;

                decimal prevD = i >= 1 ? dList.ElementAtOrDefault(i - 1) : currentValue;
                decimal d = prevD + (((zeta * a) + ((1 - zeta) * c)) / p * gamma);
                dList.Add(d);

                var signal = GetCompareSignal(currentValue - d, prevValue - prevD);
                signalsList.Add(signal);
            }

            stockData.OutputValues = new()
            {
                { "Gfe", dList }
            };
            stockData.SignalsList = signalsList;
            stockData.CustomValuesList = dList;
            stockData.IndicatorName = IndicatorName.GeneralFilterEstimator;

            return stockData;
        }
    }
}
