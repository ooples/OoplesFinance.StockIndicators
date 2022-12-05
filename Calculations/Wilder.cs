//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the average true range.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAverageTrueRange(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14)
    {
        List<double> trList = new();
        List<Signal> signalsList = new();

        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;

            double currentTrueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trList.AddRounded(currentTrueRange);
        }

        var atrList = GetMovingAverageList(stockData, maType, length, trList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];
            double atr = atrList[i];
            double currentEma = emaList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;
            double prevEma = i >= 1 ? emaList[i - 1] : 0;
            double prevAtr = i >= 1 ? atrList[i - 1] : 0;
            double atrEma = CalculateEMA(atr, prevAtr, length);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, atr, atrEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Atr", atrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = atrList;
        stockData.IndicatorName = IndicatorName.AverageTrueRange;

        return stockData;
    }

    /// <summary>
    /// Calculates the average index of the directional.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAverageDirectionalIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 14)
    {
        List<double> dmPlusList = new();
        List<double> dmMinusList = new();
        List<double> diPlusList = new();
        List<double> diMinusList = new();
        List<double> trList = new();
        List<double> diList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double highDiff = currentHigh - prevHigh;
            double lowDiff = prevLow - currentLow;

            double dmPlus = highDiff > lowDiff ? Math.Max(highDiff, 0) : 0;
            dmPlusList.AddRounded(dmPlus);

            double dmMinus = highDiff < lowDiff ? Math.Max(lowDiff, 0) : 0;
            dmMinusList.AddRounded(dmMinus);

            double tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trList.AddRounded(tr);
        }

        var dmPlus14List = GetMovingAverageList(stockData, maType, length, dmPlusList);
        var dmMinus14List = GetMovingAverageList(stockData, maType, length, dmMinusList);
        var tr14List = GetMovingAverageList(stockData, maType, length, trList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double dmPlus14 = dmPlus14List[i];
            double dmMinus14 = dmMinus14List[i];
            double trueRange14 = tr14List[i];

            double diPlus = trueRange14 != 0 ? MinOrMax(100 * dmPlus14 / trueRange14, 100, 0) : 0;
            diPlusList.AddRounded(diPlus);

            double diMinus = trueRange14 != 0 ? MinOrMax(100 * dmMinus14 / trueRange14, 100, 0) : 0;
            diMinusList.AddRounded(diMinus);

            double diDiff = Math.Abs(diPlus - diMinus);
            double diSum = diPlus + diMinus;

            double di = diSum != 0 ? MinOrMax(100 * diDiff / diSum, 100, 0) : 0;
            diList.AddRounded(di);
        }

        var adxList = GetMovingAverageList(stockData, maType, length, diList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double diPlus = diPlusList[i];
            double diMinus = diMinusList[i];
            double prevDiPlus = i >= 1 ? diPlusList[i - 1] : 0;
            double prevDiMinus = i >= 1 ? diMinusList[i - 1] : 0;

            var signal = GetCompareSignal(diPlus - diMinus, prevDiPlus - prevDiMinus);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "DiPlus", diPlusList },
            { "DiMinus", diMinusList },
            { "Adx", adxList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = adxList;
        stockData.IndicatorName = IndicatorName.AverageDirectionalIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Welles Wilder Volatility System
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateWellesWilderVolatilitySystem(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 63, int length2 = 21, double factor = 3)
    {
        List<double> vstopList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;
        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentAtr = atrList[i];
            double currentEma = emaList[i];
            double highest = highestList[i];
            double lowest = lowestList[i];
            double currentValue = inputList[i];
            double prevValue = i >= 1 ? inputList[i - 1] : 0;

            double prevVStop = vstopList.LastOrDefault();
            double sic = currentValue > currentEma ? highest : lowest;
            double vstop = currentValue > currentEma ? sic - (factor * currentAtr) : sic + (factor * currentAtr);
            vstopList.AddRounded(vstop);

            var signal = GetCompareSignal(currentValue - vstop, prevValue - prevVStop);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wwvs", vstopList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vstopList;
        stockData.IndicatorName = IndicatorName.WellesWilderVolatilitySystem;

        return stockData;
    }
}